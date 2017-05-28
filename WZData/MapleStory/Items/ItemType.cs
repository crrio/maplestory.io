using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WZData
{
    public class ItemType
    {
        static Dictionary<string, Tuple<string, int, int>[]> equips = new Dictionary<string, Tuple<string, int, int>[]>{
            {"Accessory",
                new Tuple<string,int,int>[]{
                    new Tuple<string, int, int>("Badge", 1180000, 1190000),
                    new Tuple<string, int, int>("Belt", 1130000, 1140000),
                    new Tuple<string, int, int>("Earrings", 1030000, 1040000),
                    new Tuple<string, int, int>("Emblem", 1190000, 1190500),
                    new Tuple<string, int, int>("Face Accessory", 1010000, 1020000),
                    new Tuple<string, int, int>("Medal", 1140000, 1150000),
                    new Tuple<string, int, int>("Eye Decoration", 1020000, 1030000),
                    new Tuple<string, int, int>("Earring", 1030000, 1040000),
                    new Tuple<string, int, int>("Ring", 1110000, 1120000),
                    new Tuple<string, int, int>("Pendant", 1120000, 1130000),
                    new Tuple<string, int, int>("Pocket Item", 1160000, 1170000),
                    new Tuple<string, int, int>("Power Source", 1190200, 1190300),
                    new Tuple<string, int, int>("Shoulder Accessory", 1150000, 1160000),
                    new Tuple<string, int, int>("Totem", 1202000, 1202200)
                }
            },
            {"Armor",
                new Tuple<string,int,int>[]{
                    new Tuple<string, int, int>("Hat", 1000000, 1010000),
                    new Tuple<string, int, int>("Cape", 1100000, 1110000),
                    new Tuple<string, int, int>("Top", 1040000, 1050000),
                    new Tuple<string, int, int>("Glove", 1080000, 1090000),
                    new Tuple<string, int, int>("Overall", 1050000, 1060000),
                    new Tuple<string, int, int>("Bottom", 1060000, 1070000),
                    new Tuple<string, int, int>("Shield", 1090000, 1100000),
                    new Tuple<string, int, int>("Shoes", 1070000, 1080000),
                    new Tuple<string, int, int>("Test Armor", 1690100, 1690200)
                }
            },
            {"Other",
                new Tuple<string,int,int>[]{
                    new Tuple<string, int, int>("Android", 1660000, 1670000),
                    new Tuple<string, int, int>("Dragon Equipment", 1940000, 1980000),
                    new Tuple<string, int, int>("Mechanical Heart", 1670000, 1680000),
                    new Tuple<string, int, int>("Mechanic Equipment", 1610000, 1660000),
                    new Tuple<string, int, int>("Pet Equipment", 1800000, 1810000),
                    new Tuple<string, int, int>("Bits", 1680000, 1680200),
                    new Tuple<string, int, int>("Shovel", 1502000, 1502010),
                    new Tuple<string, int, int>("Pickaxe", 1512000, 1512010),
                    new Tuple<string, int, int>("Skill Effect", 1602000, 1602010),
                    new Tuple<string, int, int>("Pet Use", 1812000, 1833000)
                }
            },
            {"One-Handed Weapon",
                new Tuple<string,int,int>[]{
                    new Tuple<string, int, int>("Scepter", 1252000, 1253000),
                    new Tuple<string, int, int>("One-Handed Axe", 1310000, 1320000),
                    new Tuple<string, int, int>("Katara", 1340000, 1350000),
                    new Tuple<string, int, int>("Cane", 1360000, 1370000),
                    new Tuple<string, int, int>("Dagger", 1330000, 1340000),
                    new Tuple<string, int, int>("Desperado", 1232000, 1233000),
                    new Tuple<string, int, int>("Whip Blade", 1242000, 1243000),
                    new Tuple<string, int, int>("One-Handed Blunt Weapon", 1320000, 1330000),
                    new Tuple<string, int, int>("Shining Rod", 1212000, 1213000),
                    new Tuple<string, int, int>("Soul Shooter", 1222000, 1223000),
                    new Tuple<string, int, int>("Staff", 1380000, 1390000),
                    new Tuple<string, int, int>("One-Handed Sword", 1300000, 1310000),
                    new Tuple<string, int, int>("Wand", 1370000, 1380000),
                    new Tuple<string, int, int>("Test Weapon", 1690000, 1690100),
                    new Tuple<string, int, int>("Cash", 1701000, 1703000)
                }
            },
            {"Secondary Weapon",
                new Tuple<string,int,int>[]{
                    new Tuple<string, int, int>("Ballast", 1352910, 1352920),
                    new Tuple<string, int, int>("Magic Marble", 1352950, 1352960),
                    new Tuple<string, int, int>("Spellbook", 1352230, 1352260),
                    new Tuple<string, int, int>("Arrow Fletching", 1352260, 1352270),
                    new Tuple<string, int, int>("Powder Keg", 1352920, 1352930),
                    new Tuple<string, int, int>("Far Sight", 1352910, 1352920),
                    new Tuple<string, int, int>("Card", 1352100, 1352200),
                    new Tuple<string, int, int>("Iron Chain", 1352220, 1352230),
                    new Tuple<string, int, int>("Bow Thimble", 1352270, 1352280),
                    new Tuple<string, int, int>("Jewel", 1352970, 1352980),
                    new Tuple<string, int, int>("Document", 1352940, 1352950),
                    new Tuple<string, int, int>("Medal", 1352200, 1352210),
                    new Tuple<string, int, int>("Magic Arrow", 1350000, 1352100),
                    new Tuple<string, int, int>("Charm", 1352290, 1352300),
                    new Tuple<string, int, int>("Orb", 1352400, 1352500),
                    new Tuple<string, int, int>("Rosary", 1352210, 1352220),
                    new Tuple<string, int, int>("Dagger Scabbard", 1352280, 1352290),
                    new Tuple<string, int, int>("Wrist Band", 1352900, 1352910),
                    new Tuple<string, int, int>("Arrowhead", 1352960, 1352970),
                    new Tuple<string, int, int>("Jett's Core", 1352300, 1352310), // Guess
                    new Tuple<string, int, int>("Nova's Essence", 1352500, 1352510), // Guess
                    new Tuple<string, int, int>("Soul Ring", 1352600, 1352610), // Guess
                    new Tuple<string, int, int>("Magnum", 1352700, 1352710), // Guess
                    new Tuple<string, int, int>("Kodachi", 1352800, 1352810),
                    new Tuple<string, int, int>("Whistle", 1352810, 1352820),
                    new Tuple<string, int, int>("Jett Fists", 1352820, 1352830), //I have no idea, seems like a mixture of Jett secondaries and others?
                    new Tuple<string, int, int>("Mass", 1352930, 1352940), // Guess // Jet
                    new Tuple<string, int, int>("Fox Marble", 1353100, 1353110),
                    new Tuple<string, int, int>("Core Controller", 1353000, 1353010) // Guess
                    
                }
            },
            {"Two-Handed Weapon",
                new Tuple<string,int,int>[]{
                    new Tuple<string, int, int>("Two-Handed Axe", 1410000, 1420000),
                    new Tuple<string, int, int>("Bow", 1450000, 1460000),
                    new Tuple<string, int, int>("Crossbow", 1460000, 1470000),
                    new Tuple<string, int, int>("Dual Bowgun", 1520000, 1530000),
                    new Tuple<string, int, int>("Gun", 1490000, 1500000),
                    new Tuple<string, int, int>("Hand Cannon", 1530000, 1540000),
                    new Tuple<string, int, int>("Knuckle", 1480000, 1490000),//
                    new Tuple<string, int, int>("Two-Handed Blunt", 1420000, 1430000),
                    new Tuple<string, int, int>("Pole Arm", 1440000, 1450000),
                    new Tuple<string, int, int>("Spear", 1430000, 1440000),
                    new Tuple<string, int, int>("Two-Handed Sword", 1400000, 1410000),
                    new Tuple<string, int, int>("Claw", 1470000, 1480000),
                    new Tuple<string, int, int>("Katana", 1542000, 1543000),
                    new Tuple<string, int, int>("Fan", 1552000, 1553000),
                    new Tuple<string, int, int>("Lapis", 1562000, 1563000),
                    new Tuple<string, int, int>("Lazuli", 1572000, 1573000)
                }
            },
            {"Character",
                new Tuple<string,int,int>[]{
                    new Tuple<string, int, int>("Face", 20000, 25000),
                    new Tuple<string, int, int>("Hair", 30000, 40000)
                }
            },
            {"Monster",
                new Tuple<string,int,int>[]{
                    new Tuple<string, int, int>("Crusader Codex", 1172000, 1172010),
                    new Tuple<string, int, int>("Monster Battle", 1842000, 1893000)
                }
            },
            {"Mount",
                new Tuple<string,int,int>[]{
                    new Tuple<string, int, int>("Mount", 1902000, 1993000), // Should be further categorized
                }
            },
        };

        static Dictionary<string, Tuple<string, int, int>[]> use = new Dictionary<string, Tuple<string, int, int>[]>
        {
            {"Consumable",
                new Tuple<string,int,int>[]{
                new Tuple<string, int, int>("Potion", 2000000, 2010000),
                new Tuple<string, int, int>("Food and Drink", 2010000, 2022000),
                new Tuple<string, int, int>("Consumable", 2022000, 2024000),
                new Tuple<string, int, int>("Status Cure", 2050000, 2050100),
                new Tuple<string, int, int>("Pet Food", 2120000, 2120100),
                new Tuple<string, int, int>("Transformation", 2210000, 2230000),
                new Tuple<string, int, int>("EXP Potion", 2230000, 2240000),
                new Tuple<string, int, int>("Character Modification", 2350000, 2350100),
                new Tuple<string, int, int>("Transformation", 2360000, 2360100),
                new Tuple<string, int, int>("EXP Potion", 2370000, 2370100),
                new Tuple<string, int, int>("Time Saver", 2390000, 2390100),
                new Tuple<string, int, int>("Equipment Box", 2028000, 2029000),
                new Tuple<string, int, int>("Other", 2430000, 2440000),
                new Tuple<string, int, int>("Teleport Item", 2030000, 2040000),
                new Tuple<string, int, int>("Teleport Item", 2320000, 2323100), // Teleport rocks
                new Tuple<string, int, int>("EXP Buff", 2450000, 2450100),
                new Tuple<string, int, int>("Other", 2520000, 2520100), // Thrown by anyone items
                new Tuple<string, int, int>("Appearance", 2540000, 2545100),
                new Tuple<string, int, int>("Equipment Box", 2550000, 2550100),
                new Tuple<string, int, int>("Teleport Item", 2620000, 2620100),
                new Tuple<string, int, int>("Other", 2800000, 2801000), // PQ Items
                new Tuple<string, int, int>("Other", 2900000, 2901000), // PQ Items
                new Tuple<string, int, int>("Other", 2920000, 2920100) // Armor bypass keys
                }
            },
            {"Armor Scroll",
            new Tuple<string,int,int>[]{
                new Tuple<string, int, int>("Helmet", 2040000, 2040100),
                new Tuple<string, int, int>("Face", 2040100, 2040200),
                new Tuple<string, int, int>("Eye", 2040200, 2040300),
                new Tuple<string, int, int>("Earrings", 2040300, 2040400),
                new Tuple<string, int, int>("Topwear", 2040400, 2040500),
                new Tuple<string, int, int>("Overall", 2040500, 2040600),
                new Tuple<string, int, int>("Bottomwear", 2040600, 2040700),
                new Tuple<string, int, int>("Shoes", 2040700, 2040800),
                new Tuple<string, int, int>("Gloves", 2040800, 2040900),
                new Tuple<string, int, int>("Shield", 2040900, 2041000),
                new Tuple<string, int, int>("Cape", 2041000, 2041100),
                new Tuple<string, int, int>("Ring", 2041100, 2041200),
                new Tuple<string, int, int>("Pendant", 2041200, 2041300),
                new Tuple<string, int, int>("Belts", 2041300, 2041400),
                new Tuple<string, int, int>("Belts", 2046400, 2046500), // Mu Gong"s
                new Tuple<string, int, int>("Shoulder", 2041500, 2041600),
                new Tuple<string, int, int>("Armor", 2046200, 2046300),
                new Tuple<string, int, int>("Accessory", 2046300, 2046400),
                new Tuple<string, int, int>("Armor", 2046500, 2046600), // Renegades for Armor
                new Tuple<string, int, int>("Armor", 2046600, 2046700), // Legendary Armor
                new Tuple<string, int, int>("Accessory", 2046700, 2046800), // Azwan
                new Tuple<string, int, int>("Accessory", 2046800, 2046900),
                new Tuple<string, int, int>("Equipment", 2047900, 2048000), // ES 100-120
                new Tuple<string, int, int>("Pet", 2048000, 2048100),
                new Tuple<string, int, int>("Equipment", 2048100, 2048200), // Battle Mode Scroll...
                new Tuple<string, int, int>("Equipment", 2048500, 2048600), // Yggdrasil
                new Tuple<string, int, int>("Equipment", 2048600, 2048700), // Skill scrolls
                new Tuple<string, int, int>("Equipment", 2048700, 2048800), // Rebirth Flames
                new Tuple<string, int, int>("Pet", 2048800, 2048900),
                new Tuple<string, int, int>("Accessory", 2049200, 2049300),
                new Tuple<string, int, int>("Accessory", 2615000, 2615100),
                new Tuple<string, int, int>("Armor", 2616200, 2616300),
                new Tuple<string, int, int>("Armor", 2616000, 2616100),
                new Tuple<string, int, int>("Accessory", 2643000, 2643100)
            }
            },
            {"Weapon Scroll",
                new Tuple<string,int,int>[]{
                new Tuple<string, int, int>("Shining Rod", 2042100, 2042200),
                new Tuple<string, int, int>("Soul Shooter", 2042200, 2042300),
                new Tuple<string, int, int>("Desperado", 2042300, 2042400),
                new Tuple<string, int, int>("Whip Blade", 2042400, 2042500),
                new Tuple<string, int, int>("One-Handed Sword", 2043000, 2043100),
                new Tuple<string, int, int>("One-Handed Axe", 2043100, 2043200),
                new Tuple<string, int, int>("One-Handed BW", 2043200, 2043300),
                new Tuple<string, int, int>("Dagger", 2043300, 2043400),
                new Tuple<string, int, int>("Katara", 2043400, 2043500),
                new Tuple<string, int, int>("Cane", 2043600, 2043700),
                new Tuple<string, int, int>("Wand", 2043700, 2043800),
                new Tuple<string, int, int>("Staff", 2043800, 2043900),
                new Tuple<string, int, int>("Two-Handed Sword", 2044000, 2044100),
                new Tuple<string, int, int>("Two-Handed Axe", 2044100, 2044200),
                new Tuple<string, int, int>("Two-Handed BW", 2044200, 2044300),
                new Tuple<string, int, int>("Spear", 2044300, 2044400),
                new Tuple<string, int, int>("Polearm", 2044400, 2044500),
                new Tuple<string, int, int>("Bow", 2044500, 2044600),
                new Tuple<string, int, int>("Crossbow", 2044600, 2044700),
                new Tuple<string, int, int>("Claw", 2044700, 2044800),
                new Tuple<string, int, int>("Knuckle", 2044800, 2044900),
                new Tuple<string, int, int>("Gun", 2044900, 2045000),
                new Tuple<string, int, int>("Dual Bowgun", 2045200, 2045300),
                new Tuple<string, int, int>("Hand Cannon", 2045300, 2045400),
                new Tuple<string, int, int>("Katana", 2045400, 2045500),
                new Tuple<string, int, int>("Fan", 2045500, 2045600),
                new Tuple<string, int, int>("One-Handed Weapon", 2046000, 2046100),
                new Tuple<string, int, int>("Two-Handed Weapon", 2046100, 2046200),
                new Tuple<string, int, int>("One-Handed Weapon", 2046900, 2047000), // Grand
                new Tuple<string, int, int>("Two-Handed Weapon", 2047800, 2047900),
                new Tuple<string, int, int>("Zero", 2048900, 2049000),
                new Tuple<string, int, int>("Two-Handed Weapon", 2612000, 2612100),
                new Tuple<string, int, int>("One-Handed Weapon", 2613000, 2613100),
                new Tuple<string, int, int>("One-Handed Weapon", 2640000, 2640100)
                }
            },
            {"Special Scroll",
            new Tuple<string,int,int>[]{
                new Tuple<string, int, int>("Clean Slate Scroll", 2049000, 2049100),
                new Tuple<string, int, int>("Slot Carver", 2048200, 2048305),
                new Tuple<string, int, int>("Bonus Potential Scroll", 2048305, 2048400),
                new Tuple<string, int, int>("Chaos Scroll", 2049100, 2049200),
                new Tuple<string, int, int>("Equip Enhancement", 2049300, 2049400),
                new Tuple<string, int, int>("Potential Scroll", 2049400, 2049500),
                new Tuple<string, int, int>("Awakening Stamp", 2049500, 2049600),
                new Tuple<string, int, int>("Innocence Scroll", 2049600, 2049700),
                new Tuple<string, int, int>("Potential Scroll", 2049700, 2049800), // Epic / Unique
                new Tuple<string, int, int>("Fusion Anvil", 2049900, 2050000),
                new Tuple<string, int, int>("White Scroll", 2340000, 2340100),
                new Tuple<string, int, int>("Hammer", 2470000, 2470100),
                new Tuple<string, int, int>("Magnifying Glass", 2460000, 2460100),
                new Tuple<string, int, int>("Hammer", 2471000, 2471100),
                new Tuple<string, int, int>("Time Extension", 2490000, 2490100),
                new Tuple<string, int, int>("Lucky Day Scroll", 2530000, 2530100),
                new Tuple<string, int, int>("Protection Scroll", 2531000, 2531100),
                new Tuple<string, int, int>("Safety Scroll", 2532000, 2532100),
                new Tuple<string, int, int>("Guardian Scroll", 2533000, 2533100),
                new Tuple<string, int, int>("Equipment Change", 2570000, 2570100),
                new Tuple<string, int, int>("Soul Weapon", 2590000, 2600000),
                new Tuple<string, int, int>("Equipment Change", 2600000, 2600100),
                new Tuple<string, int, int>("Equipment Change", 2610000, 2610100), // Upgrade Anvil
                new Tuple<string, int, int>("Miracle Cube", 2710000, 2712000),
                new Tuple<string, int, int>("Trade", 2720000, 2720100),
                new Tuple<string, int, int>("Slot Carver", 2930000, 2930100) // Alien Socket Creator
            }
            },
            {"Character Modification",
                new Tuple<string,int,int>[]{
                new Tuple<string, int, int>("Mastery Book", 2280000, 2292000), // Sorry, too lazy to sort these...
                new Tuple<string, int, int>("AP/SP Reset", 2500000, 2500100),
                new Tuple<string, int, int>("AP/SP Reset", 2501000, 2501100),
                new Tuple<string, int, int>("Monster Taming", 2560000, 2560100),
                new Tuple<string, int, int>("Circulator", 2700000, 2702100),
                new Tuple<string, int, int>("Dust", 2940000, 2946000)
                }
            },
            {"Tablet",
            new Tuple<string,int,int>[]{
                new Tuple<string, int, int>("One-Handed Weapon", 2047000, 2047100),
                new Tuple<string, int, int>("Two-Handed Weapon", 2047100, 2047200),
                new Tuple<string, int, int>("Armor", 2047200, 2047300),
                new Tuple<string, int, int>("Accessory", 2047300, 2047400)
            }
            },
            {"Projectile",
                new Tuple<string,int,int>[]{
                new Tuple<string, int, int>("Arrow", 2060000, 2060005),
                new Tuple<string, int, int>("Thrown", 2060005, 2060007), // Why are snowballs here?!
                new Tuple<string, int, int>("Arrow", 2060007, 2060100),
                new Tuple<string, int, int>("Crossbow Bolt", 2061000, 2061100), // Arrow for Crossbow...
                new Tuple<string, int, int>("Thrown", 2070000, 2070100),
                new Tuple<string, int, int>("Bullet", 2330000, 2330100)
                }
            },
            {"Monster/Familiar",
            new Tuple<string,int,int>[]{
                new Tuple<string, int, int>("Monster Card", 2380000, 2389000),
                new Tuple<string, int, int>("Familiar Skill", 2860000, 2870000),
                new Tuple<string, int, int>("Familiar Card", 2870000, 2880000)
            }
            },
            {"Recipe",
                new Tuple<string,int,int>[]{
                new Tuple<string, int, int>("Smithing Recipe", 2510000, 2511000),
                new Tuple<string, int, int>("Accessory Crafting Recipe", 2511000, 2512000),
                new Tuple<string, int, int>("Alchemy Recipe", 2512000, 2513000)
                }
            },
            {"Other",
            new Tuple<string,int,int>[]{
                new Tuple<string, int, int>("Synergy Machine", 2048400, 2048500),
                new Tuple<string, int, int>("Megaphone", 2083000, 2085000),
                new Tuple<string, int, int>("Summoning Sack", 2100000, 2110000),
                new Tuple<string, int, int>("Message", 2160100, 2160200),
                new Tuple<string, int, int>("Lie Detector", 2190000, 2190100),
                new Tuple<string, int, int>("Wedding", 2240000, 2240100),
                new Tuple<string, int, int>("Monster Taming", 2260000, 2270100),
                new Tuple<string, int, int>("Owl", 2310000, 2310100),
                new Tuple<string, int, int>("Capsule", 2331000, 2332100),
                new Tuple<string, int, int>("Item Pot", 2440000, 2441100),
                new Tuple<string, int, int>("Message", 2480000, 2480100)
            }
            },
        };

        static Dictionary<string, Tuple<string, int, int>[]> etc = new Dictionary<string, Tuple<string, int, int>[]>
        {
            {"Other",
            new Tuple<string,int,int>[]{
                new Tuple<string, int, int>("Monster Drop", 4000000, 4010000),
                new Tuple<string, int, int>("Quest Item", 4030000, 4040000),
                new Tuple<string, int, int>("Cosmetic", 4055000, 4056000),
                new Tuple<string, int, int>("Minigame", 4080000, 4080200),
                new Tuple<string, int, int>("Other", 4140000, 4150000),
                new Tuple<string, int, int>("Pet Command", 4160000, 4161000),
                new Tuple<string, int, int>("Book", 4161000, 4170000),
                new Tuple<string, int, int>("Quest Item", 4220000, 4221000),
                new Tuple<string, int, int>("Effect", 4290000, 4290100),
                new Tuple<string, int, int>("Message", 4300000, 4301100),
                new Tuple<string, int, int>("Coin", 4310000, 4320000),
                new Tuple<string, int, int>("Other", 4320000, 4320100),
                new Tuple<string, int, int>("Container", 4330000, 4331000),
                new Tuple<string, int, int>("Item Pot", 4340000, 4340100),
                new Tuple<string, int, int>("Book", 4350000, 4351000),
                new Tuple<string, int, int>("Other", 4360000, 4361000), // Ranmaru"s Cage...
                new Tuple<string, int, int>("EXP Ticket", 4390000, 4391000),
                new Tuple<string, int, int>("Event Item", 4440000, 4450000), // Jewels
                new Tuple<string, int, int>("Book", 4460000, 4460100), // Explorer Book
            }
            },
            {"Cash Shop",
            new Tuple<string,int,int>[]{
                new Tuple<string, int, int>("Reward Item", 4170000, 4171000),
                new Tuple<string, int, int>("Wedding", 4210000, 4215000),
                new Tuple<string, int, int>("Reward Item", 4280000, 4281000),
                new Tuple<string, int, int>("Reward Item", 4400000, 4401000),
                new Tuple<string, int, int>("Potential Lock", 4410000, 4411000),
                new Tuple<string, int, int>("Fusion", 4420000, 4420100),
                new Tuple<string, int, int>("Reward Item", 4430000, 4430100),
                new Tuple<string, int, int>("Coupon", 4450000, 4451000)
            }
            },
            {"Crafting",
            new Tuple<string,int,int>[]{
                new Tuple<string, int, int>("Mineral Ore", 4010000, 4011000),
                new Tuple<string, int, int>("Mineral Processed", 4011000, 4012000), // Includes Ore Fragment
                new Tuple<string, int, int>("Rare Ore", 4020000, 4020100),
                new Tuple<string, int, int>("Rare Processed  Ore", 4021000, 4021013),
                new Tuple<string, int, int>("Crafting Item", 4021013, 4022000),
                new Tuple<string, int, int>("Herb", 4022000, 4023000),
                new Tuple<string, int, int>("Herb Oil", 4023000, 4023023),
                new Tuple<string, int, int>("Crafting Item", 4023023, 4026000),
                new Tuple<string, int, int>("Maker", 4130000, 4140000),
                new Tuple<string, int, int>("Maker", 4250000, 4261000)
                }
            }
        };

        static Dictionary<string, Tuple<string, int, int>[]> setup = new Dictionary<string, Tuple<string, int, int>[]>
        {
            {"Other",
            new Tuple<string,int,int>[]{
                new Tuple<string, int, int>("Chair", 3010000, 3020000),
                new Tuple<string, int, int>("Extractor", 3049000, 3049100),
                new Tuple<string, int, int>("Container", 3080000, 3080100),
                new Tuple<string, int, int>("Container", 3090000, 3090100), // Bit-Case
                new Tuple<string, int, int>("Title", 3700000, 3701000),
                new Tuple<string, int, int>("Other", 3800000, 3803000),
                new Tuple<string, int, int>("Decoration", 3990000, 3992039),
                new Tuple<string, int, int>("Key", 3992039, 3992100),
                new Tuple<string, int, int>("Event Item", 3993000, 3995000),
                new Tuple<string, int, int>("Other", 3995000, 3995100) // A .?  Really now?
            }
            },
            {"Commerci",
            new Tuple<string,int,int>[]{
                new Tuple<string, int, int>("Trade Good", 3100000, 3101000),
                new Tuple<string, int, int>("Ship Enhancement", 3102000, 3103000)
            }
            },
            {"Evolution Lab",
            new Tuple<string,int,int>[]{
                new Tuple<string, int, int>("Core", 3600000, 3604000), // There are some categories in here I didn"t do
                new Tuple<string, int, int>("Mission", 3604000, 3604100)
            }
            },
            {"Nebulite",
            new Tuple<string,int,int>[]{
                new Tuple<string, int, int>("Rank D Nebulite", 3060000, 3061000),
                new Tuple<string, int, int>("Rank C Nebulite", 3061000, 3062000),
                new Tuple<string, int, int>("Rank B Nebulite", 3062000, 3063000),
                new Tuple<string, int, int>("Rank A Nebulite", 3063000, 3064000),
                new Tuple<string, int, int>("Rank S Nebulite", 3064000, 3065000)
            }
            }
        };

        static Dictionary<string, Tuple<string, int, int>[]> cash = new Dictionary<string, Tuple<string, int, int>[]>(){
            {"Time Saver",
            new Tuple<string,int,int>[]{
                new Tuple<string, int, int>("Teleport Rock", 5040000, 5050000),
                new Tuple<string, int, int>("Package", 5330000, 5340000), // Is this right?
                new Tuple<string, int, int>("Item Store", 5450000, 5451000),
                new Tuple<string, int, int>("Quest Helper", 5660000, 5670000)
            }
            },
            {"Random Reward",
            new Tuple<string,int,int>[]{
                new Tuple<string, int, int>("Special Item", 5060002, 5061000), // e.g. Tommelise"s Key
                new Tuple<string, int, int>("Special Item", 5200000, 5210000),
                new Tuple<string, int, int>("Gachapon", 5220000, 5221000),
                new Tuple<string, int, int>("Other", 5221000, 5222000),
                new Tuple<string, int, int>("Surprise Box", 5222000, 5223000),
                new Tuple<string, int, int>("Gachapon", 5451000, 5452000), // Remote Gachapon
                new Tuple<string, int, int>("Gachapon", 5490000, 5500000),
                new Tuple<string, int, int>("Exchange Coupon", 5530000, 5534000) // Does this go here?
            }
            },
            {"Equipment Modification",
            new Tuple<string,int,int>[]{
                new Tuple<string, int, int>("Other", 5060000, 5060002),
                new Tuple<string, int, int>("Other", 5061000, 5062000),
                new Tuple<string, int, int>("Miracle Cube", 5062000, 5062800),
                new Tuple<string, int, int>("Scroll", 5063000, 5065000),
                new Tuple<string, int, int>("Scroll", 5067000, 5069000),
                new Tuple<string, int, int>("Other", 5500000, 5500010),
                new Tuple<string, int, int>("Other", 5502000, 5503000), // Bypass keys
                new Tuple<string, int, int>("Trade", 5520000, 5530000),
                new Tuple<string, int, int>("Other", 5534000, 5535000), // Is this right? (Tim"s Secret Lab)
                new Tuple<string, int, int>("Upgrade Slot", 5570000, 5580000),
                new Tuple<string, int, int>("Other", 5590000, 5600000), // Same as a bypass key...
                new Tuple<string, int, int>("Other", 5610000, 5620000), // Vega"s Spell
                new Tuple<string, int, int>("Scroll", 5640000, 5650000), // Pam"s Song
                new Tuple<string, int, int>("Other", 5750000, 5760000) // Neb Diffuser
            }
            },
            {"Character Modification",
            new Tuple<string,int,int>[]{
                new Tuple<string, int, int>("SP/AP Modification", 5050000, 5060000),
                new Tuple<string, int, int>("Circulator", 5062800, 5062900),
                new Tuple<string, int, int>("Protection", 5130000, 5140000),
                new Tuple<string, int, int>("EXP Coupon", 5210000, 5220000),
                new Tuple<string, int, int>("Drop Coupon", 5360000, 5370000),
                new Tuple<string, int, int>("Wedding", 5250000, 5252000),
                new Tuple<string, int, int>("Entry Pass", 5252000, 5253000), // Is this where these go?
                new Tuple<string, int, int>("Other", 5400000, 5410000), // Character name change
                new Tuple<string, int, int>("Inventory Slot", 5430000, 5440000),
                new Tuple<string, int, int>("Other", 5500010, 5502000),
                new Tuple<string, int, int>("Protection", 5510000, 5520000), // Wheels
                new Tuple<string, int, int>("Mastery Book", 5620000, 5630000),
                new Tuple<string, int, int>("EXP Coupons", 5710000, 5720000), // Quest Booster
                new Tuple<string, int, int>("Skill Modification", 5770000, 5780000),
                new Tuple<string, int, int>("Other", 5820000, 5830000)
            }
            },
            {"Weapon",
            new Tuple<string,int,int>[]{
                new Tuple<string, int, int>("Thrown", 5020000, 5030000)
            }
            },
            {"Accessory",
            new Tuple<string,int,int>[]{
                new Tuple<string, int, int>("Pendant", 5550000, 5560000)
            }
            },
            {"Appearance",
            new Tuple<string,int,int>[]{
                new Tuple<string, int, int>("Effect", 5010000, 5020000),
                new Tuple<string, int, int>("Hair Coupon", 5150000, 5151000),
                new Tuple<string, int, int>("Hair Color Coupon", 5151000, 5152000),
                new Tuple<string, int, int>("Face Coupon", 5152000, 5152100),
                new Tuple<string, int, int>("Cosmetic Lens", 5152100, 5152200),
                new Tuple<string, int, int>("Skin Coupon", 5153000, 5154000),
                new Tuple<string, int, int>("Hair Coupon", 5154000, 5155000),
                new Tuple<string, int, int>("Ear", 5155000, 5156000),
                new Tuple<string, int, int>("Facial Expression", 5160000, 5170000),
                new Tuple<string, int, int>("Transformation", 5300000, 5310000),
                new Tuple<string, int, int>("Hair Coupon", 5420000, 5430000), // Hair Memberships
                new Tuple<string, int, int>("Special", 5800000, 5800100)
            }
            },
            {"Pet",
            new Tuple<string,int,int>[]{
                new Tuple<string, int, int>("Pet Use", 5170000, 5200000),
                new Tuple<string, int, int>("Pet Food", 5240000, 5250000),
                new Tuple<string, int, int>("Pet Use", 5380000, 5390000),
                new Tuple<string, int, int>("Pet Use", 5460000, 5470000), // Pet Snack
                new Tuple<string, int, int>("Pet Appearance", 5780000, 5790000)
            }
            },
            {"Free Market",
            new Tuple<string,int,int>[]{
                new Tuple<string, int, int>("Hired Merchant", 5030000, 5040000),
                new Tuple<string, int, int>("Store Permit", 5140000, 5150000),
                new Tuple<string, int, int>("Other", 5230000, 5240000),
                new Tuple<string, int, int>("Hired Merchant", 5470000, 5480000),
                new Tuple<string, int, int>("Auction House", 5830000, 5840000),
                new Tuple<string, int, int>("Other", 5990000, 5990100)
            }
            },
            {"Messenger and Social",
            new Tuple<string,int,int>[]{
                new Tuple<string, int, int>("Weather Effect", 5065000, 5066000),
                new Tuple<string, int, int>("Messenger", 5070000, 5080000),
                new Tuple<string, int, int>("Kite", 5080000, 5090000),
                new Tuple<string, int, int>("Note", 5090000, 5100000),
                new Tuple<string, int, int>("Song", 5100000, 5110000),
                new Tuple<string, int, int>("Character Effect", 5110000, 5120000),
                new Tuple<string, int, int>("Weather Effect", 5120000, 5130000),
                new Tuple<string, int, int>("Character Effect", 5281000, 5282000), // Farts / floral scent
                new Tuple<string, int, int>("Guild Forum Emoticon", 5290000, 5300000),
                new Tuple<string, int, int>("Messageboard", 5370000, 5380000), // Chalkboards, mainly
                new Tuple<string, int, int>("Messenger", 5390000, 5400000)
            }
            },
            {"Miscellaneous",
            new Tuple<string,int,int>[]{
                new Tuple<string, int, int>("Cake vs Pie", 5670000, 5680000),
                new Tuple<string, int, int>("Other", 5680000, 5700000)
            }
            },
        };

        static Dictionary<string, Dictionary<string, Tuple<string, int, int>[]>> overall = new Dictionary<string, Dictionary<string, Tuple<string, int, int>[]>>{
            {"Equip", equips},
            {"Use", use},
            {"Setup", setup},
            {"Etc", etc},
            {"Cash", cash}
        };

        static List<ItemType> Categories = new List<ItemType>();

        static ItemType()
        {
            foreach (KeyValuePair<string, Dictionary<string, Tuple<string, int, int>[]>> overallCategory in overall)
                foreach (KeyValuePair<string, Tuple<string, int, int>[]> category in overallCategory.Value)
                    foreach (Tuple<string, int, int> subcategory in category.Value)
                        Categories.Add(new ItemType(overallCategory.Key, category.Key, subcategory.Item1, subcategory.Item2, subcategory.Item3));
        }

        public static ItemType FindCategory(int itemId) => Categories.Find((possible) => itemId >= possible.LowItemId && itemId <= possible.HighItemId) ?? new ItemType("Unknown", "Unknown", "Unknown", 0, 0);

        public string OverallCategory, Category, SubCategory;
        public int LowItemId, HighItemId;

        public ItemType(string overallCategory, string category, string subcategory, int lowItemId, int highItemId)
        {
            OverallCategory = overallCategory;
            Category = category;
            SubCategory = subcategory;
            LowItemId = lowItemId;
            HighItemId = highItemId;
        }
    }
}
