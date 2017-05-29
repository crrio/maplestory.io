using reWZ.WZProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WZData.MapleStory.Mob
{
    public class NPCMeta
    {
        /// <summary>
        /// Determines if a monster can hit you by touching you.
        /// </summary>
        public bool IsBodyAttack; // bodyAttack
        /// <summary>
        /// Mobs level.
        /// </summary>
        public int Level; // level
        /// <summary>
        /// Mob's Max HP
        /// </summary>
        public long MaxHP; // maxHP
        /// <summary>
        /// Mob's Max MP
        /// </summary>
        public long MaxMP; // maxMP
        /// <summary>
        /// Movement speed, only for ground monsters
        /// </summary>
        public int Speed; // speed
        /// <summary>
        /// Flying speed, only for flying monsters
        /// </summary>
        public int FlySpeed; // flySpeed
        /// <summary>
        /// Physical attack damage
        /// </summary>
        public int PhysicalDamage; // PADamage
        /// <summary>
        /// Physical defense
        /// </summary>
        public int PhysicalDefense; // PDDamage
        /// <summary>
        /// Magical Attack Damage
        /// </summary>
        public int MagicDamage; // MADamage
        /// <summary>
        /// Magical Attack Defense
        /// </summary>
        public int MagicDefense; // MDDamage
        /// <summary>
        /// Accuracy
        /// </summary>
        public int Accuracy; // acc
        /// <summary>
        /// Evasion
        /// </summary>
        public int Evasion; // eva
        /// <summary>
        /// Experience given upon killing
        /// </summary>
        public long EXP; // exp
        /// <summary>
        /// If the mob is undead (can be damaged by heal or not)
        /// </summary>
        public bool IsUndead; // undead
        /// <summary>
        /// Minimum amount of damage required to knockback
        /// </summary>
        public long MinimumPushDamage; // pushed
        /// <summary>
        /// How much HP is recovered over time limit
        /// </summary>
        public int HPRecovery; // hpRecovery
        /// <summary>
        /// How much MP is recovered over time limit
        /// </summary>
        public int MPRecovery; // mpRecovery
        /// <summary>
        /// Elemental resistence / weakness
        /// H1I2F3 = ICE-least effective FIRE-most effective HOLY-Non-effective
        /// </summary>
        public string ElementalAttributes; // elemAttr
        /// <summary>
        /// If it summons other monsters / how / etc
        /// </summary>
        public SummonType SummonType; // summonType
        /// <summary>
        /// HP gauge Tag color
        /// </summary>
        public int HPTagColor; // hpTagColor
        /// <summary>
        /// HP gauge Tag background color
        /// </summary>
        public int HPTagBackgroundColor; // hpTagBgcolor
        /// <summary>
        /// If the HP gauge should be hidden
        /// </summary>
        public bool HPGaugeHide; // HPgaugeHide
        /// <summary>
        /// If the mob will respawn upon death
        /// </summary>
        public bool NoRespawn; // noRegen
        /// <summary>
        /// Spawn these mobs after death
        /// </summary>
        public List<int> RevivesMonsterId; // revive [0,1,...]
        /// <summary>
        /// Takes the info from another mob
        /// </summary>
        public string LinksToOtherMob; // link
        /// <summary>
        /// Can only be damaged by normal attack
        /// </summary>
        public bool OnlyNormalAttack; // onlyNormalAttack
        /// <summary>
        /// How much damage will be done to the monster, no matter what
        /// </summary>
        public uint? FixedDamageAmount; // fixedDamage
        /// <summary>
        /// If the mob is a boss
        /// </summary>
        public bool IsBoss; // boss
        /// <summary>
        /// If the monster will automatically aggro against you
        /// </summary>
        public bool IsAutoAggro; // firstAttack
        /// <summary>
        /// If item drops are public / free game
        /// </summary>
        public bool PublicReward; // publicReward
        /// <summary>
        /// If the item drops are spread across the map (think Horntail or Zakum)
        /// </summary>
        public bool ExplosiveReward; // explosiveReward
        /// <summary>
        /// If the mob is invincible
        /// </summary>
        public bool IsInvincible; // invincible
        /// <summary>
        /// Monster can not attack you but you can attack it
        /// </summary>
        public bool NoAttack; // notAttack
        /// <summary>
        /// Mob despawns after this time
        /// </summary>
        public int RemoveAfterTime; // removeAfter
        /// <summary>
        /// Buff given to players that kill this mob
        /// </summary>
        public uint? BuffId; // buff
        /// <summary>
        /// Hides the mob name
        /// </summary>
        public bool HideName; // hideName
        /// <summary>
        /// Monster book ID
        /// </summary>
        public uint? MonsterBookId; // mbookID
        /// <summary>
        /// PDR
        /// </summary>
        public int PhysicalDefenseRate;
        /// <summary>
        /// MDR, not sure how this differs from PDR
        /// </summary>
        public int MagicDefenseRate;

        public static NPCMeta Parse(WZObject info)
        {
            NPCMeta result = new NPCMeta();

            result.IsBodyAttack = info.HasChild("bodyAttack") && info["bodyAttack"].ValueOrDefault<int>(-1) == 1;
            result.Level = info.HasChild("level") ? info["level"].ValueOrDefault<int>(1) : 1;
            result.MaxHP = info.HasChild("maxHP") ? (long)(uint)info["maxHP"].ValueOrDefault<int>(0) : -1;
            result.MaxMP = info.HasChild("maxMP") ? (long)(uint)info["maxMP"].ValueOrDefault<int>(0) : -1;
            result.Speed = info.HasChild("speed") ? info["speed"].ValueOrDefault<int>(-1) : -1;
            result.FlySpeed = info.HasChild("flySpeed") ? info["flySpeed"].ValueOrDefault<int>(-1) : -1;
            result.PhysicalDamage = info.HasChild("PADamage") ? info["PADamage"].ValueOrDefault<int>(-1) : -1;
            result.PhysicalDefense = info.HasChild("PDDamage") ? info["PDDamage"].ValueOrDefault<int>(-1) : -1;
            result.PhysicalDefenseRate = info.HasChild("PDRate") ? info["PDRate"].ValueOrDefault<int>(0) : 0;
            result.MagicDamage = info.HasChild("MADamage") ? info["MADamage"].ValueOrDefault<int>(-1) : -1;
            result.MagicDefense = info.HasChild("MDDamage") ? info["MDDamage"].ValueOrDefault<int>(-1) : -1;
            result.MagicDefenseRate = info.HasChild("MDRate") ? info["MDRate"].ValueOrDefault<int>(0) : 0;
            result.Accuracy = info.HasChild("acc") ? info["acc"].ValueOrDefault<int>(-1) : -1;
            result.Evasion = info.HasChild("eva") ? info["eva"].ValueOrDefault<int>(-1) : -1;
            result.EXP = info.HasChild("exp") ? (long)(uint)info["exp"].ValueOrDefault<int>(0) : 0;
            result.IsUndead = info.HasChild("undead") && info["undead"].ValueOrDefault<int>(-1) == 1;
            result.MinimumPushDamage = info.HasChild("pushed") ? info["pushed"].ValueOrDefault<int>(0) : 0;
            result.HPRecovery = info.HasChild("hpRecovery") ? info["hpRecovery"].ValueOrDefault<int>(-1) : -1;
            result.MPRecovery = info.HasChild("mpRecovery") ? info["mpRecovery"].ValueOrDefault<int>(-1) : -1;
            result.ElementalAttributes = info.HasChild("elemAttr") ? info["elemAttr"].ValueOrDefault<string>(null) : null;
            result.SummonType = info.HasChild("summonType") ? (SummonType)info["summonType"].ValueOrDefault<int>(1) : SummonType.Normal;
            result.HPTagColor = info.HasChild("hpTagColor") ? info["hpTagColor"].ValueOrDefault<int>(-1) : -1;
            result.HPTagBackgroundColor = info.HasChild("hpTagBgcolor") ? info["hpTagBgcolor"].ValueOrDefault<int>(-1) : -1;
            result.HPGaugeHide = info.HasChild("HPgaugeHide") && info["HPgaugeHide"].ValueOrDefault<int>(-1) == 1;
            result.NoRespawn = info.HasChild("noRegen") && info["noRegen"].ValueOrDefault<int>(-1) == 1;
            result.RevivesMonsterId = info.HasChild("revive") ? info["revive"].Select(c => c.ValueOrDefault<int>(0)).ToList() : null; // revive [0 = info.HasChild("...]") ? 
            result.LinksToOtherMob = info.HasChild("link") ? info["link"].ValueOrDefault<string>(null) : null;
            result.OnlyNormalAttack = info.HasChild("onlyNormalAttack") && info["onlyNormalAttack"].ValueOrDefault<int>(-1) == 1;
            result.FixedDamageAmount = info.HasChild("fixedDamage") ? (uint?)info["fixedDamage"].ValueOrDefault<int>(0) : null;
            result.IsBoss = info.HasChild("boss") && info["boss"].ValueOrDefault<int>(-1) == 1;
            result.IsAutoAggro = info.HasChild("firstAttack") && info["firstAttack"].ValueOrDefault<int>(-1) == 1;
            result.PublicReward = info.HasChild("publicReward") && info["publicReward"].ValueOrDefault<int>(-1) == 1;
            result.ExplosiveReward = info.HasChild("explosiveReward") && info["explosiveReward"].ValueOrDefault<int>(-1) == 1;
            result.IsInvincible = info.HasChild("invincible") && info["invincible"].ValueOrDefault<int>(-1) == 1;
            result.NoAttack = info.HasChild("notAttack") && info["notAttack"].ValueOrDefault<int>(-1) == 1;
            result.RemoveAfterTime = info.HasChild("removeAfter") ? info["removeAfter"].ValueOrDefault<int>(-1) : -1;
            result.BuffId = info.HasChild("buff") ? (uint?)info["buff"].ValueOrDefault<int>(0) : null;
            result.HideName = info.HasChild("hideName") && info["hideName"].ValueOrDefault<int>(-1) == 1;
            result.MonsterBookId = info.HasChild("mbookID") ? (uint?)info["mbookID"].ValueOrDefault<int>(0) : null;

            return result;
        }
    }

    public class SelfDestruction
    {
        public bool Action;
        public long HPRequired;
        public bool RemoveAfterTime;
    }

    public enum SummonType
    {
        Normal = 1,
        CanSummonMonsters = 2
    }
}
