using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;

namespace maplestory.io.Data.Items
{
    public class EquipInfo
    {
        readonly static string[] mustContainOne = new string[] { "reqSTR", "reqDEX", "reqINT", "reqLUK", "reqPOP", "reqJob", "reqJob2", "reqSpecJob", "reqLevel", "tuc", "incSTR", "incDEX", "incINT", "incLUK", "incMHP", "incMMP", "incPAD", "incMAD", "incPDD", "incMDD", "incACC", "incEVA", "incCraft", "incSpeed", "incJump", "tradeBlock", "equipTradeBlock", "exItem", "charmEXP", "willEXP", "charismaEXP", "craftEXP", "senseEXP", "tradeAvailable", "superiorEqp", "noPotential", "unchangeable", "durability", "accountSharable", "attack", "attackSpeed", "bdR", "bossReward", "imdR", "islot", "vslot", "android", "grade" };


        /// <summary>
        /// Requires this STR before can be equipped
        /// </summary>
        public int? reqSTR;
        /// <summary>
        /// Requires this DEX before can be equipped
        /// </summary>
        public int? reqDEX;
        /// <summary>
        /// Requires this INT before can be equipped
        /// </summary>
        public int? reqINT;
        /// <summary>
        /// Requires this LUK before can be equipped
        /// </summary>
        public int? reqLUK;
        /// <summary>
        /// Requires this fame before can be equipped
        /// </summary>
        public int? reqPOP;
        /// <summary>
        /// Requires this job before can be equipped
        /// </summary>
        public int? reqJob;
        /// <summary>
        /// Requires this secondary job before can be equipped
        /// </summary>
        public int? reqJob2;
        /// <summary>
        /// Requires this special job before can be equipped
        /// </summary>
        public int? reqSpecJob;
        /// <summary>
        /// Requires this level before can be equipped
        /// </summary>
        public int? reqLevel;
        /// <summary>
        /// Scroll Count?
        /// </summary>
        public int? tuc;
        /// <summary>
        /// Increases the user's STR by
        /// </summary>
        public int? incSTR;
        /// <summary>
        /// Increases the user's DEX by
        /// </summary>
        public int? incDEX;
        /// <summary>
        /// Increases the user's INT by
        /// </summary>
        public int? incINT;
        /// <summary>
        /// Increases the user's LUK by
        /// </summary>
        public int? incLUK;
        /// <summary>
        /// Increases the user's MHP by
        /// </summary>
        public int? incMHP;
        /// <summary>
        /// Increases the user's MMP by
        /// </summary>
        public int? incMMP;
        /// <summary>
        /// Increases the user's PAD by
        /// </summary>
        public int? incPAD;
        /// <summary>
        /// Increases the user's MAD by
        /// </summary>
        public int? incMAD;
        /// <summary>
        /// Increases the user's PDD by
        /// </summary>
        public int? incPDD;
        /// <summary>
        /// Increases the user's MDD by
        /// </summary>
        public int? incMDD;
        /// <summary>
        /// Increases the user's ACC by
        /// </summary>
        public int? incACC;
        /// <summary>
        /// Increases the user's EVA by
        /// </summary>
        public int? incEVA;
        /// <summary>
        /// Increases the user's Craft by
        /// </summary>
        public int? incCraft;
        /// <summary>
        /// Increases the user's Speed by
        /// </summary>
        public int? incSpeed;
        /// <summary>
        /// Increases the user's Jump by
        /// </summary>
        public int? incJump;
        /// <summary>
        /// Is trade blocked
        /// </summary>
        public bool? tradeBlock;
        /// <summary>
        /// Is tradeblocked after equipped
        /// </summary>
        public bool? equipTradeBlock;
        /// <summary>
        /// Is an Exclusive/Unique item
        /// </summary>
        public string exItem;
        /// <summary>
        /// Increases the user's charm by this much when equipping
        /// </summary>
        public int? charmEXP;
        /// <summary>
        /// Increases the user's willpower by this much when equipping
        /// </summary>
        public int? willEXP;
        /// <summary>
        /// Increases the user's charisma by this much when equipping
        /// </summary>
        public int? charismaEXP;
        /// <summary>
        /// Increases the user's crafting by this much when equipping
        /// </summary>
        public int? craftEXP;
        /// <summary>
        /// Increases the user's insight by this much when equipping
        /// </summary>
        public int? senseEXP;
        /// <summary>
        /// The type of trading that's available
        /// </summary>
        public byte? tradeAvailable;
        /// <summary>
        /// If the item is a superior equip
        /// </summary>
        public bool? superiorEqp;
        /// <summary>
        /// The user can not put a potential on this item
        /// </summary>
        public bool? noPotential;
        /// <summary>
        /// The user can not change anything on this item
        /// </summary>
        public string unchangeable;
        /// <summary>
        /// This item has a durability
        /// </summary>
        public string durability;
        /// <summary>
        /// Is possible to move in account
        /// </summary>
        public bool? accountSharable;
        public int? attack;
        public int? attackSpeed;
        /// <summary>
        /// The boss damage percent this item gives
        /// </summary>
        public int? bdR;
        /// <summary>
        /// If this item has a reward for fighting against bosses
        /// </summary>
        public bool? bossReward;
        /// <summary>
        /// The ignore defense percent this item gives
        /// </summary>
        public int? imdR;

        public string islot, vslot;
        public int? android;
        public int? androidGrade;

        public IEnumerable<string> vslots { get => (new string[(vslot ?? "").Length / 2]).Select((c, i) => (vslot ?? "").Substring(i * 2, 2)); }
        public IEnumerable<string> islots { get => (new string[(islot ?? "").Length / 2]).Select((c, i) => (islot ?? "").Substring(i * 2, 2)); }

        public static EquipInfo Parse(WZProperty info)
        {
            if (!info.Children.Any(c => mustContainOne.Contains(c.NameWithoutExtension)))
                return null;

            EquipInfo results = new EquipInfo();

            results.reqSTR = info.ResolveFor<int>("reqSTR");
            results.reqDEX = info.ResolveFor<int>("reqDEX");
            results.reqINT = info.ResolveFor<int>("reqINT");
            results.reqLUK = info.ResolveFor<int>("reqLUK");
            results.reqPOP = info.ResolveFor<int>("reqPOP");
            results.reqJob = info.ResolveFor<int>("reqJob");
            results.reqJob2 = info.ResolveFor<int>("reqJob2");
            results.reqSpecJob = info.ResolveFor<int>("reqSpecJob");
            results.reqLevel = info.ResolveFor<int>("reqLevel");
            results.tuc = info.ResolveFor<int>("tuc");
            results.incSTR = info.ResolveFor<int>("incSTR");
            results.incDEX = info.ResolveFor<int>("incDEX");
            results.incINT = info.ResolveFor<int>("incINT");
            results.incLUK = info.ResolveFor<int>("incLUK");
            results.incMHP = info.ResolveFor<int>("incMHP");
            results.incMMP = info.ResolveFor<int>("incMMP");
            results.incPAD = info.ResolveFor<int>("incPAD");
            results.incMAD = info.ResolveFor<int>("incMAD");
            results.incPDD = info.ResolveFor<int>("incPDD");
            results.incMDD = info.ResolveFor<int>("incMDD");
            results.incACC = info.ResolveFor<int>("incACC");
            results.incEVA = info.ResolveFor<int>("incEVA");
            results.incCraft = info.ResolveFor<int>("incCraft");
            results.incSpeed = info.ResolveFor<int>("incSpeed");
            results.incJump = info.ResolveFor<int>("incJump");
            results.tradeBlock = info.ResolveFor<bool>("tradeBlock");
            results.equipTradeBlock = info.ResolveFor<bool>("equipTradeBlock");
            results.exItem = info.ResolveForOrNull<string>("exItem");
            results.charmEXP = info.ResolveFor<int>("charmEXP");
            results.willEXP = info.ResolveFor<int>("willEXP");
            results.charismaEXP = info.ResolveFor<int>("charismaEXP");
            results.craftEXP = info.ResolveFor<int>("craftEXP");
            results.senseEXP = info.ResolveFor<int>("senseEXP");
            results.tradeAvailable = info.ResolveFor<byte>("tradeAvailable");
            results.superiorEqp = info.ResolveFor<bool>("superiorEqp");
            results.noPotential = info.ResolveFor<bool>("noPotential");
            results.unchangeable = info.ResolveForOrNull<string>("unchangeable");
            results.durability = info.ResolveForOrNull<string>("durability");
            results.accountSharable = info.ResolveFor<bool>("accountSharable");
            results.attack = info.ResolveFor<int>("attack");
            results.attackSpeed = info.ResolveFor<int>("attackSpeed");
            results.bdR = info.ResolveFor<int>("bdR");
            results.bossReward = info.ResolveFor<bool>("bossReward");
            results.imdR = info.ResolveFor<int>("imdR");
            results.islot = info.ResolveForOrNull<string>("islot");
            results.vslot = info.ResolveForOrNull<string>("vslot");
            results.android = info.ResolveFor<int>("android");
            results.androidGrade = info.ResolveFor<int>("grade");

            return results;
        }
    }
}
