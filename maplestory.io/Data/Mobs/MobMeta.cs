using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PKG1;

namespace maplestory.io.Data.Mobs
{
    public class MobMeta
    {
        /// <summary>
        /// Determines if a monster can hit you by touching you.
        /// </summary>
        public bool? IsBodyAttack; // bodyAttack
        /// <summary>
        /// Mobs level.
        /// </summary>
        public int? Level; // level
        /// <summary>
        /// Mob's Max HP
        /// </summary>
        public long? MaxHP; // maxHP
        /// <summary>
        /// Mob's Max MP
        /// </summary>
        public long? MaxMP; // maxMP
        /// <summary>
        /// Movement speed, only for ground monsters
        /// </summary>
        public int? Speed; // speed
        /// <summary>
        /// Flying speed, only for flying monsters
        /// </summary>
        public int? FlySpeed; // flySpeed
        /// <summary>
        /// Physical attack damage
        /// </summary>
        public int? PhysicalDamage; // PADamage
        /// <summary>
        /// Physical defense
        /// </summary>
        public int? PhysicalDefense; // PDDamage
        /// <summary>
        /// Magical Attack Damage
        /// </summary>
        public int? MagicDamage; // MADamage
        /// <summary>
        /// Magical Attack Defense
        /// </summary>
        public int? MagicDefense; // MDDamage
        /// <summary>
        /// Accuracy
        /// </summary>
        public int? Accuracy; // acc
        /// <summary>
        /// Evasion
        /// </summary>
        public int? Evasion; // eva
        /// <summary>
        /// Experience given upon killing
        /// </summary>
        public long? EXP; // exp
        /// <summary>
        /// If the mob is undead (can be damaged by heal or not)
        /// </summary>
        public bool? IsUndead; // undead
        /// <summary>
        /// Minimum amount of damage required to knockback
        /// </summary>
        public long? MinimumPushDamage; // pushed
        /// <summary>
        /// How much HP is recovered over time limit
        /// </summary>
        public int? HPRecovery; // hpRecovery
        /// <summary>
        /// How much MP is recovered over time limit
        /// </summary>
        public int? MPRecovery; // mpRecovery
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
        public int? HPTagColor; // hpTagColor
        /// <summary>
        /// HP gauge Tag background color
        /// </summary>
        public int? HPTagBackgroundColor; // hpTagBgcolor
        /// <summary>
        /// If the HP gauge should be hidden
        /// </summary>
        public bool? HPGaugeHide; // HPgaugeHide
        /// <summary>
        /// If the mob will respawn upon death
        /// </summary>
        public bool? NoRespawn; // noRegen
        /// <summary>
        /// Spawn these mobs after death
        /// </summary>
        public IEnumerable<MobInfo> RevivesMonsterId; // revive [0,1,...]
        /// <summary>
        /// Takes the info from another mob
        /// </summary>
        public int? LinksToOtherMob; // link
        /// <summary>
        /// Can only be damaged by normal attack
        /// </summary>
        public bool? OnlyNormalAttack; // onlyNormalAttack
        /// <summary>
        /// How much damage will be done to the monster, no matter what
        /// </summary>
        public uint? FixedDamageAmount; // fixedDamage
        /// <summary>
        /// If the mob is a boss
        /// </summary>
        public bool? IsBoss; // boss
        /// <summary>
        /// If the monster will automatically aggro against you
        /// </summary>
        public bool? IsAutoAggro; // firstAttack
        /// <summary>
        /// If item drops are public / free game
        /// </summary>
        public bool? PublicReward; // publicReward
        /// <summary>
        /// If the item drops are spread across the map (think Horntail or Zakum)
        /// </summary>
        public bool? ExplosiveReward; // explosiveReward
        /// <summary>
        /// If the mob is invincible
        /// </summary>
        public bool? IsInvincible; // invincible
        /// <summary>
        /// Monster can not attack you but you can attack it
        /// </summary>
        public bool? NoAttack; // notAttack
        /// <summary>
        /// Mob despawns after this time
        /// </summary>
        public int? RemoveAfterTime; // removeAfter
        /// <summary>
        /// Buff given to players that kill this mob
        /// </summary>
        public uint? BuffId; // buff
        /// <summary>
        /// Hides the mob name
        /// </summary>
        public bool? HideName; // hideName
        /// <summary>
        /// Monster book ID
        /// </summary>
        public uint? MonsterBookId; // mbookID
        /// <summary>
        /// PDR
        /// </summary>
        public int? PhysicalDefenseRate;
        /// <summary>
        /// MDR, not sure how this differs from PDR
        /// </summary>
        public int? MagicDefenseRate;

        public static MobMeta Parse(WZProperty info)
        {
            MobMeta result = new MobMeta();

            result.IsBodyAttack = info.ResolveFor<bool>("bodyAttack");
            result.Level = info.ResolveFor<int>("level");
            result.MaxHP = info.ResolveFor<long>("maxHP");
            result.MaxMP = info.ResolveFor<long>("maxMP");
            result.Speed = info.ResolveFor<int>("speed");
            result.FlySpeed = info.ResolveFor<int>("flySpeed");
            result.PhysicalDamage = info.ResolveFor<int>("PADamage");
            result.PhysicalDefense = info.ResolveFor<int>("PDDamage");
            result.PhysicalDefenseRate = info.ResolveFor<int>("PDRate");
            result.MagicDamage = info.ResolveFor<int>("MADamage");
            result.MagicDefense = info.ResolveFor<int>("MDDamage");
            result.MagicDefenseRate = info.ResolveFor<int>("MDRate");
            result.Accuracy = info.ResolveFor<int>("acc");
            result.Evasion = info.ResolveFor<int>("eva");
            result.EXP = info.ResolveFor<long>("exp");
            result.IsUndead = info.ResolveFor<bool>("undead");
            result.MinimumPushDamage = info.ResolveFor<int>("pushed");
            result.HPRecovery = info.ResolveFor<int>("hpRecovery");
            result.MPRecovery = info.ResolveFor<int>("mpRecovery");
            result.ElementalAttributes = info.ResolveForOrNull<string>("elemAttr");
            result.SummonType = (SummonType)(info.ResolveFor<int>("summonType") ?? 1);
            result.HPTagColor = info.ResolveFor<int>("hpTagColor");
            result.HPTagBackgroundColor = info.ResolveFor<int>("hpTagBgcolor");
            result.HPGaugeHide = info.ResolveFor<bool>("HPgaugeHide");
            result.NoRespawn = info.ResolveFor<bool>("noRegen");
            result.RevivesMonsterId = info.Resolve("revive")?.Children?.Select(c => Convert.ToInt32(((IWZPropertyVal) c).GetValue())).Select(c => MobInfo.GetFromId(info, c)).Where(c => c != null);
            result.LinksToOtherMob = info.ResolveFor<int>("link");
            result.OnlyNormalAttack = info.ResolveFor<bool>("onlyNormalAttack");
            result.FixedDamageAmount = info.ResolveFor<uint>("fixedDamage");
            result.IsBoss = info.ResolveFor<bool>("boss");
            result.IsAutoAggro = info.ResolveFor<bool>("firstAttack");
            result.PublicReward = info.ResolveFor<bool>("publicReward");
            result.ExplosiveReward = info.ResolveFor<bool>("explosiveReward");
            result.IsInvincible = info.ResolveFor<bool>("invincible");
            result.NoAttack = info.ResolveFor<bool>("notAttack");
            result.RemoveAfterTime = info.ResolveFor<int>("removeAfter");
            result.BuffId = info.ResolveFor<uint>("buff");
            result.HideName = info.ResolveFor<bool>("hideName");
            result.MonsterBookId = info.ResolveFor<uint>("mbookID");

            return result;
        }
    }

    public class SelfDestruction
    {
        public bool? Action;
        public long HPRequired;
        public bool? RemoveAfterTime;
    }

    public enum SummonType
    {
        Normal = 1,
        CanSummonMonsters = 2
    }
}
