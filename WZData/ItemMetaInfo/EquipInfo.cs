using reWZ.WZProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WZData.ItemMetaInfo
{
    public class EquipInfo
    {
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
        /// Unk
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

        public IEnumerable<string> vslots { get => (new string[vslot.Length / 2]).Select((c, i) => vslot.Substring(i * 2, 2)); }
        public IEnumerable<string> islots { get => (new string[islot.Length / 2]).Select((c, i) => islot.Substring(i * 2, 2)); }

        public static EquipInfo Parse(WZObject info)
        {
            //If it has none of the properties, return null.
            if (!(info.HasChild("reqSTR") || info.HasChild("reqDEX") || info.HasChild("reqINT") || info.HasChild("reqLUK") || info.HasChild("reqPOP") || info.HasChild("reqJob") || info.HasChild("reqJob2") || info.HasChild("reqSpecJob") || info.HasChild("reqLevel") || info.HasChild("tuc") || info.HasChild("incSTR") || info.HasChild("incDEX") || info.HasChild("incINT") || info.HasChild("incLUK") || info.HasChild("incMHP") || info.HasChild("incMMP") || info.HasChild("incPAD") || info.HasChild("incMAD") || info.HasChild("incPDD") || info.HasChild("incMDD") || info.HasChild("incACC") || info.HasChild("incEVA") || info.HasChild("incCraft") || info.HasChild("incSpeed") || info.HasChild("incJump") || info.HasChild("tradeBlock") || info.HasChild("equipTradeBlock") || info.HasChild("exItem") || info.HasChild("charmEXP") || info.HasChild("willEXP") || info.HasChild("charismaEXP") || info.HasChild("craftEXP") || info.HasChild("senseEXP") || info.HasChild("tradeAvailable") || info.HasChild("superiorEqp") || info.HasChild("noPotential") || info.HasChild("unchangeable") || info.HasChild("durability") || info.HasChild("accountSharable") || info.HasChild("attack") || info.HasChild("attackSpeed") || info.HasChild("bdR") || info.HasChild("bossReward") || info.HasChild("imdR") || info.HasChild("islot") || info.HasChild("vslot")))
                return null;

            EquipInfo results = new EquipInfo();

            if (info.HasChild("reqSTR"))
                results.reqSTR = info["reqSTR"].ValueOrDefault<int>(default(int));
            if (info.HasChild("reqDEX"))
                results.reqDEX = info["reqDEX"].ValueOrDefault<int>(default(int));
            if (info.HasChild("reqINT"))
                results.reqINT = info["reqINT"].ValueOrDefault<int>(default(int));
            if (info.HasChild("reqLUK"))
                results.reqLUK = info["reqLUK"].ValueOrDefault<int>(default(int));
            if (info.HasChild("reqPOP"))
                results.reqPOP = info["reqPOP"].ValueOrDefault<int>(default(int));
            if (info.HasChild("reqJob"))
                results.reqJob = info["reqJob"].ValueOrDefault<int>(default(int));
            if (info.HasChild("reqJob2"))
                results.reqJob2 = info["reqJob2"].ValueOrDefault<int>(default(int));
            if (info.HasChild("reqSpecJob"))
                results.reqSpecJob = info["reqSpecJob"].ValueOrDefault<int>(default(int));
            if (info.HasChild("reqLevel"))
                results.reqLevel = info["reqLevel"].ValueOrDefault<int>(default(int));
            if (info.HasChild("tuc"))
                results.tuc = info["tuc"].ValueOrDefault<int>(default(int));
            if (info.HasChild("incSTR"))
                results.incSTR = info["incSTR"].ValueOrDefault<int>(default(int));
            if (info.HasChild("incDEX"))
                results.incDEX = info["incDEX"].ValueOrDefault<int>(default(int));
            if (info.HasChild("incINT"))
                results.incINT = info["incINT"].ValueOrDefault<int>(default(int));
            if (info.HasChild("incLUK"))
                results.incLUK = info["incLUK"].ValueOrDefault<int>(default(int));
            if (info.HasChild("incMHP"))
                results.incMHP = info["incMHP"].ValueOrDefault<int>(default(int));
            if (info.HasChild("incMMP"))
                results.incMMP = info["incMMP"].ValueOrDefault<int>(default(int));
            if (info.HasChild("incPAD"))
                results.incPAD = info["incPAD"].ValueOrDefault<int>(default(int));
            if (info.HasChild("incMAD"))
                results.incMAD = info["incMAD"].ValueOrDefault<int>(default(int));
            if (info.HasChild("incPDD"))
                results.incPDD = info["incPDD"].ValueOrDefault<int>(default(int));
            if (info.HasChild("incMDD"))
                results.incMDD = info["incMDD"].ValueOrDefault<int>(default(int));
            if (info.HasChild("incACC"))
                results.incACC = info["incACC"].ValueOrDefault<int>(default(int));
            if (info.HasChild("incEVA"))
                results.incEVA = info["incEVA"].ValueOrDefault<int>(default(int));
            if (info.HasChild("incCraft"))
                results.incCraft = info["incCraft"].ValueOrDefault<int>(default(int));
            if (info.HasChild("incSpeed"))
                results.incSpeed = info["incSpeed"].ValueOrDefault<int>(default(int));
            if (info.HasChild("incJump"))
                results.incJump = info["incJump"].ValueOrDefault<int>(default(int));
            if (info.HasChild("tradeBlock"))
                results.tradeBlock = info["tradeBlock"].ValueOrDefault<int>(0) == 1;
            if (info.HasChild("equipTradeBlock"))
                results.equipTradeBlock = info["equipTradeBlock"].ValueOrDefault<int>(0) == 1;
            if (info.HasChild("exItem"))
                results.exItem = info["exItem"].ValueOrDefault<string>(default(string));
            if (info.HasChild("charmEXP"))
                results.charmEXP = info["charmEXP"].ValueOrDefault<int>(default(int));
            if (info.HasChild("willEXP"))
                results.willEXP = info["willEXP"].ValueOrDefault<int>(default(int));
            if (info.HasChild("charismaEXP"))
                results.charismaEXP = info["charismaEXP"].ValueOrDefault<int>(default(int));
            if (info.HasChild("craftEXP"))
                results.craftEXP = info["craftEXP"].ValueOrDefault<int>(default(int));
            if (info.HasChild("senseEXP"))
                results.senseEXP = info["senseEXP"].ValueOrDefault<int>(default(int));
            if (info.HasChild("tradeAvailable"))
                results.tradeAvailable = (byte)info["tradeAvailable"].ValueOrDefault<int>(default(int));
            if (info.HasChild("superiorEqp"))
                results.superiorEqp = info["superiorEqp"].ValueOrDefault<int>(0) == 1;
            if (info.HasChild("noPotential"))
                results.noPotential = info["noPotential"].ValueOrDefault<int>(0) == 1;
            if (info.HasChild("unchangeable"))
                results.unchangeable = info["unchangeable"].ValueOrDefault<string>(default(string));
            if (info.HasChild("durability"))
                results.durability = info["durability"].ValueOrDefault<string>(default(string));
            if (info.HasChild("accountSharable"))
                results.accountSharable = info["accountSharable"].ValueOrDefault<int>(0) == 1;
            if (info.HasChild("attack"))
                results.attack = info["attack"].ValueOrDefault<int>(default(int));
            if (info.HasChild("attackSpeed"))
                results.attackSpeed = info["attackSpeed"].ValueOrDefault<int>(default(int));
            if (info.HasChild("bdR"))
                results.bdR = info["bdR"].ValueOrDefault<int>(default(int));
            if (info.HasChild("bossReward"))
                results.bossReward = info["bossReward"].ValueOrDefault<int>(0) == 1;
            if (info.HasChild("imdR"))
                results.imdR = info["imdR"].ValueOrDefault<int>(default(int));
            if (info.HasChild("islot"))
                results.islot = info["islot"].ValueOrDefault<string>(null);
            if (info.HasChild("vslot"))
                results.vslot = info["vslot"].ValueOrDefault<string>(null);
            if (info.HasChild("android"))
                results.android = info["android"].ValueOrDefault<int?>(null);
            if (info.HasChild("grade"))
                results.androidGrade = info["grade"].ValueOrDefault<int?>(null);

            return results;
        }
    }
}
