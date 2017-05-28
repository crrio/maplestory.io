using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using reWZ.WZProperties;
using System.Reflection;

namespace WZData.MapleStory.Items
{
    public class CashEffect
    {
        public static List<string> unknownEffects = new List<string>();
        public IEnumerable<FrameBook> defaultFrameBook;
        public IEnumerable<FrameBook> alert;
        public IEnumerable<FrameBook> walk1;
        public IEnumerable<FrameBook> walk2;
        public IEnumerable<FrameBook> stand1;
        public IEnumerable<FrameBook> stand2;
        public IEnumerable<FrameBook> swingO1;
        public IEnumerable<FrameBook> swingO2;
        public IEnumerable<FrameBook> swingO3;
        public IEnumerable<FrameBook> swingOF;
        public IEnumerable<FrameBook> swingT1;
        public IEnumerable<FrameBook> swingT2;
        public IEnumerable<FrameBook> swingT3;
        public IEnumerable<FrameBook> swingTF;
        public IEnumerable<FrameBook> swingP1;
        public IEnumerable<FrameBook> swingP2;
        public IEnumerable<FrameBook> swingPF;
        public IEnumerable<FrameBook> stabO1;
        public IEnumerable<FrameBook> stabO2;
        public IEnumerable<FrameBook> stabOF;
        public IEnumerable<FrameBook> stabT1;
        public IEnumerable<FrameBook> stabT2;
        public IEnumerable<FrameBook> stabTF;
        public IEnumerable<FrameBook> shoot1;
        public IEnumerable<FrameBook> shoot2;
        public IEnumerable<FrameBook> shootF;
        public IEnumerable<FrameBook> heal;
        public IEnumerable<FrameBook> fly;
        public IEnumerable<FrameBook> jump;
        public IEnumerable<FrameBook> dead;
        public IEnumerable<FrameBook> proneStab;
        public IEnumerable<FrameBook> prone;
        public IEnumerable<FrameBook> sit;
        public IEnumerable<FrameBook> ladder;
        public IEnumerable<FrameBook> rope;
        public IEnumerable<FrameBook> ladder2;
        public IEnumerable<FrameBook> rope2;
        public IEnumerable<FrameBook> savage;
        public IEnumerable<FrameBook> alert2;
        public IEnumerable<FrameBook> alert3;
        public IEnumerable<FrameBook> alert4;
        public IEnumerable<FrameBook> alert5;
        public IEnumerable<FrameBook> alert6;
        public IEnumerable<FrameBook> paralyze;
        public IEnumerable<FrameBook> shoot6;
        public IEnumerable<FrameBook> magic1;
        public IEnumerable<FrameBook> magic2;
        public IEnumerable<FrameBook> magic3;
        public IEnumerable<FrameBook> magic4;
        public IEnumerable<FrameBook> magic5;
        public IEnumerable<FrameBook> burster1;
        public IEnumerable<FrameBook> burster2;
        public IEnumerable<FrameBook> avenger;
        public IEnumerable<FrameBook> assaulter;
        public IEnumerable<FrameBook> prone2;
        public IEnumerable<FrameBook> assassination;
        public IEnumerable<FrameBook> assassinationS;
        public IEnumerable<FrameBook> rush;
        public IEnumerable<FrameBook> rush2;
        public IEnumerable<FrameBook> sanctuary;
        public IEnumerable<FrameBook> meteor;
        public IEnumerable<FrameBook> blizzard;
        public IEnumerable<FrameBook> genesis;
        public IEnumerable<FrameBook> brandish1;
        public IEnumerable<FrameBook> brandish2;
        public IEnumerable<FrameBook> ninjastorm;
        public IEnumerable<FrameBook> chainlightning;
        public IEnumerable<FrameBook> showdown;
        public IEnumerable<FrameBook> smokeshell;
        public IEnumerable<FrameBook> holyshield;
        public IEnumerable<FrameBook> resurrection;
        public IEnumerable<FrameBook> straight;
        public IEnumerable<FrameBook> handgun;
        public IEnumerable<FrameBook> doublefire;
        public IEnumerable<FrameBook> triplefire;
        public IEnumerable<FrameBook> fake;
        public IEnumerable<FrameBook> doubleupper;
        public IEnumerable<FrameBook> eburster;
        public IEnumerable<FrameBook> screw;
        public IEnumerable<FrameBook> backspin;
        public IEnumerable<FrameBook> eorb;
        public IEnumerable<FrameBook> dragonstrike;
        public IEnumerable<FrameBook> airstrike;
        public IEnumerable<FrameBook> edrain;
        public IEnumerable<FrameBook> backstep;
        public IEnumerable<FrameBook> timeleap;
        public IEnumerable<FrameBook> shot;
        public IEnumerable<FrameBook> recovery;
        public IEnumerable<FrameBook> fist;
        public IEnumerable<FrameBook> fireburner;
        public IEnumerable<FrameBook> coolingeffect;
        public IEnumerable<FrameBook> homing;
        public IEnumerable<FrameBook> rapidfire;
        public IEnumerable<FrameBook> cannon;
        public IEnumerable<FrameBook> torpedo;
        public IEnumerable<FrameBook> darksight;
        public IEnumerable<FrameBook> bamboo;
        public IEnumerable<FrameBook> wave;
        public IEnumerable<FrameBook> blade;
        public IEnumerable<FrameBook> souldriver;
        public IEnumerable<FrameBook> firestrike;
        public IEnumerable<FrameBook> flamegear;
        public IEnumerable<FrameBook> stormbreak;
        public IEnumerable<FrameBook> shockwave;
        public IEnumerable<FrameBook> demolition;
        public IEnumerable<FrameBook> snatch;
        public IEnumerable<FrameBook> windshot;
        public IEnumerable<FrameBook> vampire;
        public IEnumerable<FrameBook> swingT2PoleArm;
        public IEnumerable<FrameBook> swingP1PoleArm;
        public IEnumerable<FrameBook> swingP2PoleArm;
        public IEnumerable<FrameBook> combatStep;
        public IEnumerable<FrameBook> doubleSwing;
        public IEnumerable<FrameBook> poleArmPush;
        public IEnumerable<FrameBook> finalCharge;
        public IEnumerable<FrameBook> finalToss;
        public IEnumerable<FrameBook> finalBlow;
        public IEnumerable<FrameBook> comboSmash;
        public IEnumerable<FrameBook> comboFenrir;
        public IEnumerable<FrameBook> fullSwingDouble;
        public IEnumerable<FrameBook> overSwingDouble;
        public IEnumerable<FrameBook> overSwingTriple;
        public IEnumerable<FrameBook> rollingSpin;
        public IEnumerable<FrameBook> comboTempest;
        public IEnumerable<FrameBook> floatFramebook;
        public IEnumerable<FrameBook> fly2;
        public IEnumerable<FrameBook> fly2Move;
        public IEnumerable<FrameBook> magicmissile;
        public IEnumerable<FrameBook> fireCircle;
        public IEnumerable<FrameBook> lightingBolt;
        public IEnumerable<FrameBook> dragonBreathe;
        public IEnumerable<FrameBook> breathe_prepare;
        public IEnumerable<FrameBook> icebreathe_prepare;
        public IEnumerable<FrameBook> illusion;
        public IEnumerable<FrameBook> dragonIceBreathe;
        public IEnumerable<FrameBook> magicFlare;
        public IEnumerable<FrameBook> elementalReset;
        public IEnumerable<FrameBook> magicRegistance;
        public IEnumerable<FrameBook> magicBooster;
        public IEnumerable<FrameBook> magicShield;
        public IEnumerable<FrameBook> killingWing;
        public IEnumerable<FrameBook> recoveryAura;
        public IEnumerable<FrameBook> OnixBlessing;
        public IEnumerable<FrameBook> Earthquake;
        public IEnumerable<FrameBook> soulStone;
        public IEnumerable<FrameBook> dragonThrust;
        public IEnumerable<FrameBook> darkFog;
        public IEnumerable<FrameBook> ghostLettering;
        public IEnumerable<FrameBook> slow;
        public IEnumerable<FrameBook> mapleHero;
        public IEnumerable<FrameBook> Awakening;
        public IEnumerable<FrameBook> flameWheel;
        public IEnumerable<FrameBook> swingD1;
        public IEnumerable<FrameBook> swingD2;
        public IEnumerable<FrameBook> stabD1;
        public IEnumerable<FrameBook> tripleStab;
        public IEnumerable<FrameBook> flyingAssaulter;
        public IEnumerable<FrameBook> tornadoDash;
        public IEnumerable<FrameBook> tornadoDashStop;
        public IEnumerable<FrameBook> fatalBlow;
        public IEnumerable<FrameBook> flashBang;
        public IEnumerable<FrameBook> owlDead;
        public IEnumerable<FrameBook> upperStab;
        public IEnumerable<FrameBook> chainPull;
        public IEnumerable<FrameBook> chainAttack;
        public IEnumerable<FrameBook> monsterBombPrepare;
        public IEnumerable<FrameBook> monsterBombThrow;
        public IEnumerable<FrameBook> suddenRaid;
        public IEnumerable<FrameBook> finalCutPrepare;
        public IEnumerable<FrameBook> ride3;
        public IEnumerable<FrameBook> getoff3;
        public IEnumerable<FrameBook> demonFly;
        public IEnumerable<FrameBook> demonFly2;
        public IEnumerable<FrameBook> demonJumpUpward;
        public IEnumerable<FrameBook> demonJumpForward;
        public IEnumerable<FrameBook> demonGravity;
        public IEnumerable<FrameBook> devilishPower;
        public IEnumerable<FrameBook> devilFly;
        public IEnumerable<FrameBook> devilFly2;
        public IEnumerable<FrameBook> backDefault;

        public bool isFollow;
        public static CashEffect Parse(WZDirectory itemWz, WZObject cashItem, WZObject effects)
        {
            CashEffect effect = new CashEffect();

            if (effects.HasChild("default"))
                effect.defaultFrameBook = FrameBook.Parse(itemWz, cashItem, effects["default"]);
            if (effects.HasChild("float"))
                effect.floatFramebook = FrameBook.Parse(itemWz, cashItem, effects["float"]);

            bool isOnlyDefault = false;

            if (effects.HasChild("follow"))
                effect.isFollow = effects["follow"].ValueOrDefault<int>(0) == 1;

            foreach (WZObject obj in effects)
            {
                int frameTest = 0;
                if (isOnlyDefault = (obj.Type == WZObjectType.Canvas || int.TryParse(obj.Name, out frameTest))) break;

                if (obj.Name == "default" || obj.ChildCount == 0) continue;

                FieldInfo frameBookProperty = typeof(CashEffect).GetField(obj.Name);

                if (frameBookProperty != null)
                    frameBookProperty.SetValue(effect, FrameBook.Parse(itemWz, cashItem, obj));
                else
                {
                    if (!unknownEffects.Contains(obj.Name))
                    {
                        unknownEffects.Add(obj.Name);
                        Console.WriteLine($"Unprocessed possible framebook: {obj.Name}");
                    }
                }
            }

            if (isOnlyDefault)
                effect.defaultFrameBook = FrameBook.Parse(itemWz, cashItem, effects);

            return effect;
        }
    }
}
