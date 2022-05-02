using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Models;
using Assets.Scripts.Models.Towers;
using Assets.Scripts.Models.Towers.Behaviors.Abilities;
using Assets.Scripts.Simulation.Towers;
using Assets.Scripts.Unity.UI_New.InGame;
using Assets.Scripts.Unity.UI_New.Popups;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using Assets.Scripts.Models.Profile;
using Assets.Scripts.Models.Towers.Behaviors;
using Assets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors;
using Assets.Scripts.Simulation.Utils;
using Assets.Scripts.Unity;
using BTD_Mod_Helper;
using MelonLoader;
using NinjaKiwi.NKMulti;

[assembly: MelonInfo(typeof(AbilityChoice.Main), "Ability Choice", "1.0.13", "doombubbles")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace AbilityChoice
{
    public class Main : BloonsTD6Mod
    {
        public override string MelonInfoCsURL =>
            "https://raw.githubusercontent.com/doombubbles/BTD6-Mods/main/AbilityChoice/Main.cs";

        public override string LatestURL =>
            "https://github.com/doombubbles/BTD6-Mods/blob/main/AbilityChoice/AbilityChoice.dll?raw=true";
    
        public static HashSet<int> CurrentTowerIDs = new HashSet<int>();
        public static Dictionary<int, int> CurrentBoostIDs = new Dictionary<int, int>();

        private static readonly Dictionary<string, string> AllUpgrades = new Dictionary<string, string>
        {
            {"Plasma Monkey Fan Club", "Permanently shoots powerful plasma blasts itself."},
            {"Super Monkey Fan Club", "Gains permanent Super Attack speed and range itself."},
            {"Perma Charge", "Smaller but constant damage buff."},
            {"Turbo Charge", "Smaller but constant attack speed buff."},
            {"MOAB Eliminator", "Does extremely further increased MOAB damage."},
            {"MOAB Assassin", "Does even further increased MOAB damage."},
            {"Bomb Blitz", "More explosion damage."},
            {"Super Maelstrom", "More range, pierce, and lifespan."},
            {"Blade Maelstrom", "More range and pierce; blades seek out Bloons."},
            {"Absolute Zero", "Further slow, buffs nearby Ice Monkey always."},
            {"Snowstorm", "Partially slows MOAB class bloons."},
            {"Glue Storm", "Main attacks apply weakening, slower ability glue."},
            {"Glue Strike", "Main attacks apply weakening ability glue."},
            {"Pre-emptive Strike", "Gains even frequenter First Strike style Missile attacks."},
            {"First Strike Capability", "Gains frequent First Strike style Missile attacks."},
            {"Buccaneer-Pirate Lord", "All attacks do significantly increased MOAB and Ceramic damage."},
            {"Buccaneer-Monkey Pirates", "Cannon attacks do significantly increased MOAB and Ceramic  damage."},
            {"M.A.D", "Shoots Rocket Storm missiles alongside its main attack."},
            {"Rocket Storm", "Shoots Rocket Storm missiles alongside its main attack."},
            {"Pop and Awe", "Occasionally causes mini-Pop and Awe effects on target."},
            {"Elite Sniper", "Always gives one crate at the start of each round."},
            {"Supply Drop", "Always gives one crate at the start of each round."},
            {"Special Poperations", "Previous effects, and Marine permanently attacks from inside the heli."},
            {"Support Chinook", "Always gives one crate at the start of each round (Move ability unchanged)."},
            {"Tsar Bomba", "Occasionally drops mini Tsar Bombs."},
            {"Ground Zero", "Occasionally drops mini Ground Zeros."},
            {"Wizard Lord Phoenix", "Super Phoenix is also permanent but weaker."},
            {"Summon Phoenix", "Phoenix is permanent but weaker."},
            {"The Anti-Bloon", "Frequently does a weaker version of Annihilation ability."},
            {"Legend of the Night", "Increased range."},
            {"Dark Champion", "Increased range."},
            {"Dark Knight", "Increased range."},
            {"Tech Terror", "Frequently does a weaker version of Annihilation ability."},
            {"Grand Saboteur", "Ninja's attack have further range and pierce, and do more damage to stronger Bloon types."},
            {"Bloon Sabotage", "Ninja’s attacks have more range and slow down Bloons themselves."},
            {"Spirit of the Forest", "Also 25 lives per round."},
            {"Jungle's Bounty", "$200 per round, nearby income increased by 15%."},
            {"Total Transformation", "Has permanent extra strength transformation laser attack on self only."},
            {"Transforming Tonic", "Has permanent but weaker transformation laser attack."},
            {"Monkey-Nomics", "No longer has a maximum capacity."},
            {"IMF loan", "Maximum capacity is $17,500 (by default)."},
            {"Homeland Defense", "Permanent global attack speed buff."},
            {"Call to Arms", "Permanent weaker nearby attack speed buff."},
            {"Ultraboost", "Modified Ability: Permanently boost (based on tier) one tower at a time."},
            {"Overclock", "Modified Ability: Permanently boost (based on tier) one tower at a time."},
            {"Carpet of Spikes", "Launches a continuous stream of spikes across the track."},
            {"Spike Storm", "Launches a continuous stream of spikes across the track."}
        };

        private static Dictionary<int, TowerModel> CoOpTowerModelCache = new Dictionary<int, TowerModel>();

        public override void OnMainMenu()
        {
            ResetCaches();
        }

        public override void OnRestart(bool removeSave)
        {
            ResetCaches();
        }

        public void ResetCaches()
        {
            if (InGame.instance == null || !InGame.instance.quitting)
            {
                CurrentTowerIDs = new HashSet<int>();
                CurrentBoostIDs = new Dictionary<int, int>();
                CoOpTowerModelCache = new Dictionary<int, TowerModel>();
                
            }
        }

        public override bool ActOnMessage(Message message)
        {
            if (message.Code != "AbilityChoice") return false;
            
            var abilityChoiceMessage = Game.instance.GetNkGI().ReadMessage<string>(message);
            var towerId = int.Parse(abilityChoiceMessage.Split(' ')[1]);

            var tower = InGame.instance.GetTowerManager().GetTowerById(towerId);
            if (abilityChoiceMessage.Contains("Enable"))
            {
                EnableForTower(tower, CoOpTowerModelCache[towerId]);
            }
            else if (abilityChoiceMessage.Contains("Disable"))
            {
                DisableForTower(tower);
            } 
            else if (abilityChoiceMessage.Contains("Boost"))
            {
                var toId = int.Parse(abilityChoiceMessage.Split(' ')[2]);
                var to = InGame.instance.GetTowerManager().GetTowerById(toId);
                Overclock.AddBoost(tower, to);
            }
            

            return true;
        }

        public static void AskApplyToTower(Tower tower, string upgradeName, TowerModel newBaseTowerModel)
        {
            if (!InGame.instance.IsCoop || tower.owner == Game.instance.GetNkGI().PeerID)
            {
                var nkGI = Game.instance.GetNkGI();
                PopupScreen.instance.ShowPopup(PopupScreen.Placement.inGameCenter,
                    "Ability Choice (Can't be Undone)",
                    $"Do you want to forego the {upgradeName} ability to instead get \"{AllUpgrades[upgradeName]}\"",
                    new Action(() =>
                    {
                        EnableForTower(tower, newBaseTowerModel);
                        if (InGame.instance.IsCoop && nkGI != null)
                        {
                            nkGI.SendMessage("Enable: " + tower.Id, null, "AbilityChoice");
                        }
                    }), "Yes",
                    new Action(() =>
                    {
                        DisableForTower(tower);
                        if (InGame.instance.IsCoop && nkGI != null)
                        {
                            nkGI.SendMessage("Disable: " + tower.Id, null, "AbilityChoice");
                        }
                    }), "No", Popup.TransitionAnim.Scale
                );
            }
            else
            {
                CoOpTowerModelCache[tower.Id] = newBaseTowerModel;
            }
        }

        public override void OnTowerUpgraded(Tower tower, string upgradeName, TowerModel newBaseTowerModel)
        {
            if (AllUpgrades.ContainsKey(upgradeName))
            {
                AskApplyToTower(tower, upgradeName, newBaseTowerModel);
            } else if (CurrentTowerIDs.Contains(tower.Id))
            {
                EnableForTower(tower, newBaseTowerModel);
            }
        }

        //[HarmonyPatch(typeof(TowerManager), nameof(TowerManager.CreateTower))]
        [HarmonyPatch(typeof(TowerManager.TowerCreateDef), nameof(TowerManager.TowerCreateDef.Invoke))]
        internal class TowerManager_CreateTower
        {
            [HarmonyPostfix]
            //internal static void Postfix(Tower __result, bool isInstaTower)
            internal static void Postfix(Tower tower, TowerModel def, bool isInsta, bool isFromSave)
            {
                if (!isInsta || blockLoading)
                {
                    return;
                }
                var towerModel = def;
                var upgradeName = AllUpgrades.Keys.FirstOrDefault(s => towerModel.appliedUpgrades.Contains(s));
                if (upgradeName == default) return;
                    
                AskApplyToTower(tower, upgradeName, towerModel);
            }
        }

        private static bool blockLoading = false;
        
        [HarmonyPatch(typeof(MapSaveLoader), nameof(MapSaveLoader.LoadMapSaveData))]
        internal class MapSaveLoader_LoadMapSaveData
        {
            [HarmonyPrefix]
            internal static void Prefix()
            {
                blockLoading = true;
            }

            [HarmonyPostfix]
            internal static void Postfix()
            {
                blockLoading = false;
            }
        }







        public static void EnableForTower(Tower tower, TowerModel towerModel)
        {
            CurrentTowerIDs.Add(tower.Id);

            var removeAbility = true;

            towerModel = towerModel.Duplicate();

            foreach (var upgrade in AllUpgrades.Keys)
            {
                if (tower.towerModel.appliedUpgrades.Contains(upgrade))
                {
                    if (upgrade == "Summon Phoenix")
                    {
                        foreach (var t2s in InGame.instance.UnityToSimulation.GetAllTowers())
                        {
                            if (t2s.tower.parentTowerId == tower.Id)
                            {
                                t2s.tower.Destroy();
                                break;
                            }
                        }
                    }
                    if (upgrade == "Wizard Lord Phoenix")
                    {
                        foreach (var t2s in InGame.instance.UnityToSimulation.GetAllTowers())
                        {
                            if (t2s.tower.parentTowerId == tower.Id)
                            {
                                if (t2s.tower.towerModel.baseId == "LordPhoenix")
                                {
                                    t2s.tower.Destroy();
                                }
                                else
                                {
                                    var lord = Game.instance.model.GetTower(TowerType.WizardMonkey, tower.towerModel.tiers[0], 5, tower.towerModel.tiers[2]);
                                    var phoenix = lord.GetBehavior<TowerCreateTowerModel>().towerModel;
                                    t2s.tower.SetTowerModel(phoenix);
                                }
                            }
                        }
                    }
                    
                    var methodName = upgrade.Replace(" ", "").Replace("'", "")
                        .Replace("-", "").Replace(".", "");
                    
                    var methodInfo = typeof(Towers).GetMethod(methodName);
                    if (methodInfo == null)
                    {
                        MelonLogger.Msg("Couldn't find method " + methodName);
                    }
                    else
                    {
                        methodInfo.Invoke(null, new object[] {towerModel});
                    }

                    if (upgrade == "Supply Drop" || upgrade == "Elite Sniper" || upgrade == "Carpet of Spikes" ||
                        upgrade == "Support Chinook" || upgrade == "Special Poperations" || upgrade == "Overclock" ||
                        upgrade == "Ultraboost")
                    {
                        removeAbility = false;
                    }
                    
                    break;
                }
            }

            if (removeAbility)
            {
                towerModel.behaviors = towerModel.behaviors.RemoveItemOfType<Model, AbilityModel>();
            }

            tower.SetTowerModel(towerModel);

            InGame.instance.bridge.OnAbilitiesChangedSim();
        }

        public static void DisableForTower(Tower tower)
        {
            CurrentTowerIDs.Remove(tower.Id);
            InGame.instance.bridge.OnAbilitiesChangedSim();
            if (CurrentBoostIDs.ContainsKey(tower.Id))
            {
                Overclock.RemoveBoostOn(CurrentBoostIDs[tower.Id]);
                CurrentBoostIDs.Remove(tower.Id);
            }
        }

        [HarmonyPatch(typeof(Tower), nameof(Tower.SetSaveData))]
        public class Tower_SetSaveData
        {
            [HarmonyPostfix]
            public static void Postfix(Tower __instance, TowerSaveDataModel towerData)
            {
                if (InGame.instance.IsCoop)
                {
                    return;
                }
                
                if (towerData.metaData.ContainsKey("AbilityChoice"))
                {
                    EnableForTower(__instance, __instance.towerModel);
                }

                if (towerData.metaData.ContainsKey("AbilityChoiceBoosting"))
                {
                    var id = int.Parse(towerData.metaData["AbilityChoiceBoosting"]);
                    var tower = InGame.instance.GetTowerManager().GetTowerByIdLastSave(id);
                    CurrentBoostIDs[__instance.Id] = tower.Id;
                }

                if (towerData.metaData.ContainsKey("AbilityChoiceStacks"))
                {
                    int stacks = int.Parse(towerData.metaData["AbilityChoiceStacks"]);
                    Overclock.UltraBoostFixes[__instance] = stacks;
                }

                if (towerData.metaData.ContainsKey("AbilityChoicePhoenix"))
                {
                    var phoenix = __instance.towerModel.Duplicate();
                    
                    phoenix.behaviors = phoenix.behaviors.RemoveItemOfType<Model, TowerExpireModel>();
                    foreach (var weaponModel in phoenix.GetWeapons())
                    {
                        weaponModel.rate *= 3f;
                    }
                    
                    __instance.SetTowerModel(phoenix);
                }
            }
        }

        [HarmonyPatch(typeof(Tower), nameof(Tower.GetSaveData))]
        public class Tower_GetSaveData
        {
            [HarmonyPostfix]
            public static void Postfix(ref TowerSaveDataModel __result, Tower __instance)
            {
                if (InGame.instance.IsCoop)
                {
                    return;
                }
                
                if (CurrentTowerIDs.Contains(__instance.Id))
                {
                    __result.metaData["AbilityChoice"] = "Yup";
                }

                if (CurrentBoostIDs.ContainsKey(__instance.Id))
                {
                    __result.metaData["AbilityChoiceBoosting"] = "" + CurrentBoostIDs[__instance.Id];
                }
                
                var mutator = __instance.GetMutatorById("Ultraboost");
                if (mutator != null)
                {
                    var stacks = mutator.mutator.Cast<OverclockPermanentModel.OverclockPermanentMutator>().stacks;
                    __result.metaData["AbilityChoiceStacks"] = "" + stacks;
                }

                if (__instance.towerModel.baseId == "PermaPhoenix" || __instance.towerModel.baseId == "LordPhoenix")
                {
                    var id = __instance.parentTowerId;
                    var tower = InGame.instance.GetTowerManager().GetTowerById(id);
                    if (CurrentTowerIDs.Contains(id) && (__instance.towerModel.baseId == "LordPhoenix" || tower.towerModel.tier < 5))
                    {
                        __result.metaData["AbilityChoicePhoenix"] = "Yup";
                    }
                }
            }
        }
    }
}