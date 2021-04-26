using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.Models;
using Assets.Scripts.Models.Map;
using Assets.Scripts.Models.Towers;
using Assets.Scripts.Models.Towers.Mods;
using Assets.Scripts.Unity;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Extensions;
using BTD_Mod_Helper.Api;
using Harmony;
using MelonLoader;
using UnityEngine;
using static Assets.Scripts.Models.Towers.TowerType;
using static MegaKnowledge.MegaKnowledge;

[assembly: MelonInfo(typeof(MegaKnowledge.Main), "Mega Knowledge", "1.0.1", "doombubbles")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace MegaKnowledge
{
    public class Main : BloonsTD6Mod
    {
        public override string MelonInfoCsURL =>
            "https://raw.githubusercontent.com/doombubbles/BTD6-Mods/main/MegaKnowledge/Main.cs";

        public override string LatestURL =>
            "https://github.com/doombubbles/BTD6-Mods/blob/main/MegaKnowledge/MegaKnowledge.dll?raw=true";


        public static readonly Dictionary<string, MegaKnowledge> MegaKnowledges = new Dictionary<string, MegaKnowledge>
        {
            {
                SplodeyDarts, new MegaKnowledge("Splodey Darts", "Dart Monkey projectiles explode on expiration.",
                    "Primary", "MoreCash", -1200, -400, 0, DartMonkey,
                    "https://media.discordapp.net/attachments/800115046134186026/810247093888876564/SplodeyDarts.png")
            },
            {
                DoubleRanga, new MegaKnowledge("Double Ranga", "Boomerang Monkeys throw 2 Boomerangs at a time!",
                    "Primary", "MoreCash", -800, -400, 0, BoomerangMonkey,
                    "https://media.discordapp.net/attachments/800115046134186026/810260328529133628/DoubleRanga.png")
            },
            {
                BombVoyage, new MegaKnowledge("Bomb Voyage",
                    "Bomb Shooters' bombs travel much faster and can damage any Bloon type.",
                    "Primary", "MoreCash", -400, -400, 0, BombShooter,
                    "https://media.discordapp.net/attachments/800115046134186026/813119143468728390/BombVoyage.png")
            },
            {
                TackAttack, new MegaKnowledge("Tack Attack",
                    "Tack Shooter attacks constantly, and its projectiles travel farther.",
                    "Primary", "MoreCash", 0, -400, 0, TackShooter,
                    "https://media.discordapp.net/attachments/800115046134186026/810666019823157258/TackAttack.png")
            },
            {
                IceFortress, new MegaKnowledge("Ice See You",
                    "Ice Monkeys detect and remove Camo from Bloons.",
                    "Primary", "MoreCash", 400, -400, 0, IceMonkey,
                    "https://media.discordapp.net/attachments/800115046134186026/810660056362254386/IceFortress.png")
            },
            {
                GorillaGlue, new MegaKnowledge("Gorilla Glue",
                    "Glue Gunners' glue globs do moderate damage themselves.",
                    "Primary", "MoreCash", 800, -400, 0, GlueGunner,
                    "https://cdn.discordapp.com/attachments/800115046134186026/812958872036704256/GorillaGlue.png")
            },
            {
                RifleRange, new MegaKnowledge("Rifle Range",
                    "Sniper Monkey shots can critically strike for double damage.",
                    "Military", "BigBloonSabotage", -1200, -400, 0, SniperMonkey,
                    "https://media.discordapp.net/attachments/800115046134186026/810676434602295306/RifleRange.png")
            },
            {
                AttackAndSupport, new MegaKnowledge("Attack and Support",
                    "Monkey Subs don't need to submerge to gain submerged benefits.",
                    "Military", "BigBloonSabotage", -800, -400, 0, MonkeySub,
                    "https://media.discordapp.net/attachments/800115046134186026/812826877009461279/AttackAndSupport.png",
                    true)
            },
            {
                Dreadnought, new MegaKnowledge("Dreadnought",
                    "Monkey Buccaneers shoot molten cannon balls instead of darts and grapes.",
                    "Military", "BigBloonSabotage", -400, -400, 0, MonkeyBuccaneer,
                    "https://media.discordapp.net/attachments/800115046134186026/810370809872646184/Dreadnought.png")
            },
            {
                AceHardware, new MegaKnowledge("Ace Hardware",
                    "Monkey Aces get a new shorter ranged focus-firing gunner attack.",
                    "Military", "BigBloonSabotage", 0, -400, 0, MonkeyAce,
                    "https://cdn.discordapp.com/attachments/800115046134186026/812900285344383006/AcePrivateHangar.png")
            },
            {
                AllPowerToThrusters, new MegaKnowledge("All Power To Thrusters",
                    "Heli Pilots can move at hyper-sonic speeds",
                    "Military", "BigBloonSabotage", 400, -400, 0, HeliPilot,
                    "https://cdn.discordapp.com/attachments/800115046134186026/812780479690047488/AllPowerToThrusters.png")
            },
            {
                MortarEmpowerment, new MegaKnowledge("Mortar Empowerment",
                    "Mortar Monkey can attack like a regular tower.",
                    "Military", "BigBloonSabotage", 800, -400, 0, MortarMonkey,
                    "https://media.discordapp.net/attachments/800115046134186026/810362033249189969/MortarEmpowerment.png",
                    true)
            },
            {
                DartlingEmpowerment, new MegaKnowledge("Dartling Empowerment",
                    "Dartling Gunner can attack like a regular tower.",
                    "Military", "BigBloonSabotage", 1200, -400, 0, DartlingGunner,
                    "https://media.discordapp.net/attachments/800115046134186026/810553105128882226/DartlingEmpowerment.png",
                    true)
            },
            {
                CrystalBall, new MegaKnowledge("Crystal Ball",
                    "Instead of letting Wizard Monkeys see through walls, the Guided Magic upgrade gives them Advanced Intel style targeting.",
                    "Magic", "ManaShield", -2000, -400, 0, WizardMonkey,
                    "https://media.discordapp.net/attachments/800115046134186026/810613374068588574/CrystalBall.png")
            },
            {
                XrayVision, new MegaKnowledge("X-Ray Vision",
                    "Super Monkeys can see through walls and have increased pierce.",
                    "Magic", "ManaShield", -1600, -400, 0, SuperMonkey,
                    "https://cdn.discordapp.com/attachments/800115046134186026/810689149329604638/XrayVision.png")
            },
            {
                ShadowDouble, new MegaKnowledge("Shadow Double",
                    "Ninja Monkeys can throw extra Shurikens behind them if Bloons are present.",
                    "Magic", "ManaShield", -1200, -400, 0, NinjaMonkey,
                    "https://cdn.discordapp.com/attachments/800115046134186026/812832140068519966/ShadowDouble.png")
            },
            {
                Oktoberfest, new MegaKnowledge("Oktoberfest",
                    "Alchemist buff potions last for 50% more shots.",
                    "Magic", "ManaShield", -800, -400, 0, Alchemist,
                    "https://cdn.discordapp.com/attachments/800115046134186026/810968077889175572/Oktoberfest.png")
            },
            {
                BloonAreNotPrepared, new MegaKnowledge("Bloon are not Prepared",
                    "Druids' personal stacking buffs always have maximum effect.",
                    "Magic", "ManaShield", -400, -400, 0, Druid,
                    "https://media.discordapp.net/attachments/800115046134186026/810948418435940392/BloonAreNotPrepared.png")
            },
            {
                RealHealthyBananas, new MegaKnowledge("Real Healthy Bananas",
                    "Healthy Bananas makes all Banana Farms give 1 life per round per upgrade (aka tier + 1 per round).",
                    "Support", "BankDeposits", 0, -400, 0, BananaFarm,
                    "https://media.discordapp.net/attachments/800115046134186026/810336352255344680/RealHealthyBananas.png")
            },
            {
                SpikeEmpowerment, new MegaKnowledge("Spike Empowerment",
                    "Spike Factories choose the spot where their spikes land, and spikes damage Bloons while traveling.",
                    "Support", "BankDeposits", 400, -400, 0, SpikeFactory,
                    "https://cdn.discordapp.com/attachments/800115046134186026/810696194383937547/SpikeEmpowerment.png",
                    true)
            },
            {
                DigitalAmplification, new MegaKnowledge("Digital Amplification",
                    "Monkey Villages have greatly increased range.",
                    "Support", "BankDeposits", 800, -400, 0, MonkeyVillage,
                    "https://cdn.discordapp.com/attachments/800115046134186026/810689161111404594/DigitalAmplification.png")
            },
            {
                Overtime, new MegaKnowledge("Overtime",
                    "Engineers and their Sentries are permanently Overclocked.",
                    "Support", "BankDeposits", 1200, -400, 0, null,
                    "https://cdn.discordapp.com/attachments/800115046134186026/810683625111945216/Overtime.png")
            },
        };

        private static readonly Dictionary<string, TowerModel> BackupTowerModels = new Dictionary<string, TowerModel>();

        private static readonly string Dir = $"{Directory.GetCurrentDirectory()}\\Mods\\MegaKnowledge";
        private static readonly string Config = $"{Dir}\\config.txt";

        public override void OnApplicationStart()
        {
            Directory.CreateDirectory($"{Dir}");
            foreach (var megaKnowledge in MegaKnowledges.Values)
            {
                SpriteRegister.Instance.RegisterSpriteFromURL("Mods\\MegaKnowledge\\" + megaKnowledge.name + ".png",
                    megaKnowledge.URL, default, out megaKnowledge.GUID);
            }

            if (File.Exists(Config))
            {
                MelonLogger.Msg("Reading config file");
                using (StreamReader sr = File.OpenText(Config))
                {
                    string s = "";
                    while ((s = sr.ReadLine()) != null)
                    {
                        try
                        {
                            if (s.StartsWith("#"))
                            {
                                continue;
                            }

                            var key = s.Split('=')[0];
                            var value = s.Split('=')[1];

                            if (MegaKnowledges.ContainsKey(key))
                            {
                                MegaKnowledges[key].enabled = bool.Parse(value);
                            }
                        }
                        catch (Exception e)
                        {
                            MelonLogger.Error("Malformed line " + s);
                            e.GetType(); //just to get rid of the warning lol
                        }
                    }
                }
            }
            else
            {
                MelonLogger.Msg("Creating config file");
                using (StreamWriter sw = File.CreateText(Config))
                {
                    foreach (var (key, megaKnowledge) in MegaKnowledges)
                    {
                        sw.WriteLine(key + "=" + megaKnowledge.enabled);
                    }
                }

                MelonLogger.Msg("Done Creating");
            }
        }

        public override void OnMainMenu()
        {
            foreach (var towerModel in Game.instance.model.towers)
            {
                foreach (var (name, megaKnowledge) in MegaKnowledges)
                {
                    if (towerModel.baseId == megaKnowledge.tower && megaKnowledge.targetChanging)
                    {
                        var methodInfo = typeof(Towers).GetMethod(name);
                        if (methodInfo == null)
                        {
                            MelonLogger.Msg("Couldn't find method " + name);
                            continue;
                        }

                        if (!BackupTowerModels.ContainsKey(towerModel.name))
                        {
                            BackupTowerModels[towerModel.name] = towerModel.Duplicate();
                        }

                        if (!megaKnowledge.enabled)
                        {
                            Reset(towerModel);
                        }
                    }
                }
            }


            using (var sw = File.CreateText(Config))
            {
                foreach (var (key, megaKnowledge) in MegaKnowledges)
                {
                    sw.WriteLine(key + "=" + megaKnowledge.enabled);
                }
            }
        }

        private static void Reset(TowerModel towerModel)
        {
            var model = BackupTowerModels[towerModel.name].Duplicate();
            towerModel.behaviors = model.behaviors;
            towerModel.towerSelectionMenuThemeId = model.towerSelectionMenuThemeId;
            towerModel.targetTypes = model.targetTypes;
        }


        [HarmonyPatch(typeof(GameModel), nameof(GameModel.CreateModded),
            typeof(Il2CppSystem.Collections.Generic.List<string>), typeof(MapModel))]
        internal class GameModel_CreateModded
        {
            [HarmonyPrefix]
            internal static void Prefix(Il2CppSystem.Collections.Generic.List<string> activeMods)
            {
                var knowledge = !Game.instance.GetPlayerProfile().knowledgeDisabled;
                foreach (var activeMod in activeMods)
                {
                    knowledge &= Game.instance.model.mods.Where(modelMod => modelMod.name == activeMod)
                        .All(modelMod =>
                            modelMod.mutatorMods.All(mod => mod.TryCast<DisableMonkeyKnowledgeModModel>() == null));
                }

                foreach (var towerModel in Game.instance.model.towers)
                {
                    foreach (var (name, megaKnowledge) in MegaKnowledges)
                    {
                        if (!megaKnowledge.enabled || !megaKnowledge.targetChanging) continue;

                        if (towerModel.baseId == megaKnowledge.tower)
                        {
                            var methodInfo = typeof(Towers).GetMethod(name);
                            if (!knowledge)
                            {
                                Reset(towerModel);
                            }
                            else if (methodInfo == null)
                            {
                                MelonLogger.Msg("Couldn't find method " + name);
                            }
                            else
                            {
                                methodInfo.Invoke(null, new object[] {towerModel});
                            }
                        }
                    }
                }
            }

            [HarmonyPostfix]
            internal static void Postfix(ref GameModel __result,
                Il2CppSystem.Collections.Generic.List<string> activeMods)
            {
                var knowledge = !Game.instance.GetPlayerProfile().knowledgeDisabled;
                foreach (var activeMod in activeMods)
                {
                    knowledge &= Game.instance.model.mods.Where(modelMod => modelMod.name == activeMod)
                        .All(modelMod =>
                            modelMod.mutatorMods.All(mod => mod.TryCast<DisableMonkeyKnowledgeModModel>() == null));
                }

                if (!knowledge)
                {
                    return;
                }

                foreach (var towerModel in __result.towers)
                {
                    foreach (var (name, megaKnowledge) in MegaKnowledges)
                    {
                        if (!megaKnowledge.enabled || megaKnowledge.targetChanging) continue;

                        if (towerModel.baseId == megaKnowledge.tower)
                        {
                            var methodInfo = typeof(Towers).GetMethod(name);
                            if (methodInfo == null)
                            {
                                MelonLogger.Msg("Couldn't find method " + name);
                            }
                            else
                            {
                                methodInfo.Invoke(null, new object[] {towerModel});
                            }
                        }
                    }
                }
            }
        }

        public static void RecursivelyLog(GameObject gameObject, int depth = 0)
        {
            var str = gameObject.name;
            for (int i = 0;
                i < depth;
                i++)
            {
                str = "|  " + str;
            }

            str += " (";
            foreach (var component in gameObject.GetComponents<Component>())
            {
                str += " " + component.GetIl2CppType().Name;
            }

            str += ")";
            MelonLogger.Msg(str);
            for (int i = 0;
                i < gameObject.transform.childCount;
                i++)
            {
                RecursivelyLog(gameObject.transform.GetChild(i).gameObject, depth + 1);
            }
        }
    }
}