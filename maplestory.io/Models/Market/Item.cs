using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RethinkDb.Driver;
using RethinkDb.Driver.Ast;
using Newtonsoft.Json;
using WZData.ItemMetaInfo;

namespace maplestory.io.Models.Market
{
    public enum ItemRarity
    {
        Common = 0,
        Rare = 1,
        Epic = 2,
        Unique = 3,
        Legendary = 4
    }
    public class Item
    {
        public Item() { }

        public int itemId;
        public int quantity;
        public int soldCount;

        [JsonIgnore]
        private string internalCategory;
        public string category
        {
            get => internalCategory;
            set
            {
                if (value is string && value.Length > 1)
                    internalCategory = value;
            }
        }
        public string name, description, OverallCategory, SubCategory;
        public bool only;

        [JsonIgnore]
        private IconInfoFake internalIcon;
        public IconInfoFake icon
        {
            get => internalIcon ?? new IconInfoFake() { icon = $"/api/item/{itemId}/icon", iconRaw = $"/api/item/{itemId}/iconRaw" };
            set => internalIcon = value;
        }
        public ChairInfo chair;
        public SlotInfo slot;
        public CardInfo card;
        public ShopInfo shop;
        public CashInfo cash;
        public EquipInfo equip;


        public int bundle;
        public long price;
        public int expireTime;

        public sbyte upgradesAvailable;
        public sbyte numberOfPlusses;
        public short str, dex, intelligence, luk;
        /// <summary>
        /// Max HP
        /// </summary>
        public short mhp;
        /// <summary>
        /// Max MP
        /// </summary>
        public short mmp;
        public short watk;
        public short matk;
        public short wdef;
        public short mdef;
        public short acc;
        public short avoid;
        public short diligence;
        public short speed;
        public short jump;
        public short untradeable;
        public sbyte growth;
        public int hammerApplied;
        public short battleModeAtt;
        public int durability;
        public sbyte bossDmg;
        public sbyte ignoreDef;
        public string creator;
        public byte isIdentified;
        public ItemRarity rarity;
        public sbyte numberOfEnhancements;
        public short nebulite;
        public PotentialInfo[] potentials
        {
            get
            {
                if (internalPotentials == null) return null;

                return this.potentialIdOrdering
                    .Select(potentialId => this.internalPotentials.Where(internalPotential => internalPotential.id == potentialId).FirstOrDefault())
                    .ToArray();
            }

            set => internalPotentials = value;
        }
        [JsonIgnore]
        private PotentialInfo[] internalPotentials;
        public int[] potentialIdOrdering;

        public static object MapItem(ReqlExpr item)
        {
            return item.G("left")
                // Description is an object, pull its properties down
                .Merge(item.G("right").G("Description"))
                // Because we can't just tell Newtonsoft to use Id as the itemId
                // RethinkDb -> Deserialize itemId to id (null) -> Serialize id to itemId (null) :(
                .Merge(new { itemId = item.G("left").G("id") })
                // Category
                .Merge(
                    RethinkDB.R.Branch(
                        item.G("right").G("TypeInfo"),
                        item.G("right").G("TypeInfo"),
                        new { Category = "Unknown", OverallCategory = "Unknown", SubCategory = "Unknown" }
                    )
                )
                // Merge in the meta info, skip the icon as that'd be way too much bandwidth
                .Merge(item.G("right").G("MetaInfo").Without("Icon"))
                // Load in the item potentials
                .Merge(new { potentialIdOrdering = item.G("left").G("potentials") })
                .Merge(ItemPotentials(item))
                // Black list meta attributes
                .Without("unk1", "unk2", "unk3", "unk4", "unk5", "unk6", "unk7", "unk8", "WZFile", "WZFolder", "bpotential1Level", "bpotential2Level", "bpotential3Level", "potential1Level", "potential2Level", "potential3Level", "potential1", "potential2", "potential3", "bpotential1", "bpotential2", "bpotential3");
        }

        public static Branch ItemPotentials(ReqlExpr item)
        {
            return RethinkDB.R.Branch(
                // If it's an equip
                item.G("right").G("MetaInfo").G("Equip"),
                // Load the potentials
                RethinkDB.R.Expr(new
                {
                    // If it has potential lines
                    potentials = RethinkDB.R.Branch(
                        item.G("left").G("potentials"),
                        // Convert and eqJoin on the potentials table
                        RethinkDB.R.Db("maplestory").Table("potentialLevels").GetAll(RethinkDB.R.Args(item.G("left").G("potentials"))).OptArg("index", "PotentialId")
                        .Filter(new
                        {
                            Level = RethinkDB.R.Branch(
                                item.G("right").G("MetaInfo").G("Equip").G("reqLevel"),
                                item.G("right").G("MetaInfo").G("Equip").G("reqLevel"),
                                1
                            ).CoerceTo("number").Add(9).Div(10).Floor()
                        })
                        .EqJoin("PotentialId", RethinkDB.R.Db("maplestory").Table("potentials"))
                        .Zip()
                        // Black list meta attributes
                        .Without("Level", "PotentialId", "RequiredLevel")
                        .CoerceTo("array"),
                        RethinkDB.R.Expr(new object[0])
                    )
                }), new { }
            );
        }

        public static ReqlExpr GetItemCount(object filter)
        {
            if (filter == null) filter = new { };
            return RethinkDB.R.Db("maplestory").Table("rooms").Filter(filter).ConcatMap((room) =>
            {
                return room.G("shops").Values().ConcatMap((shop) =>
                {
                    return shop.G("items")
                        .Merge(new
                        {
                            server = room.G("server"),
                            shopId = room.G("id").Add("-").Add(shop.G("id").CoerceTo("string")),
                            channel = room.G("channel"),
                            createdAt = room.G("createTime"),
                            room = room.G("room"),
                            characterName = shop.G("characterName"),
                            shopName = shop.G("shopName"),
                        });
                });
            }).EqJoin("id", RethinkDB.R.Db("maplestory").Table("items")).Count();
        }
    }

    public class PotentialInfo
    {
        public string Message;
        public PotentialModifier[] Modifiers;
        public int OptionType;
        public int id;
        public string line
        {
            get
            {
                return this.Modifiers.Aggregate(Message, ((runningProduct, nextFactor) =>
                {
                    return runningProduct.Replace($"#{nextFactor.Item1}", nextFactor.Item2);
                }));
            }
        }
    }

    public class PotentialModifier { public string Item1, Item2; }
    public class IconInfoFake
    {
        public string icon, iconRaw;
    }
}
