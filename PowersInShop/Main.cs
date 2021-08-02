using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Models.Map;
using Assets.Scripts.Models.Powers;
using Assets.Scripts.Models.Profile;
using Assets.Scripts.Models.Towers;
using Assets.Scripts.Models.Towers.Behaviors;
using Assets.Scripts.Models.TowerSets;
using Assets.Scripts.Simulation.Input;
using Assets.Scripts.Simulation.Towers;
using Assets.Scripts.Simulation.Track;
using Assets.Scripts.Unity;
using Assets.Scripts.Unity.Bridge;
using Assets.Scripts.Unity.UI_New;
using Assets.Scripts.Unity.UI_New.InGame;
using Assets.Scripts.Unity.UI_New.InGame.RightMenu;
using Assets.Scripts.Unity.UI_New.InGame.TowerSelectionMenu.TowerSelectionMenuThemes;
using Assets.Scripts.Unity.UI_New.Upgrade;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using InputManager = Assets.Scripts.Unity.UI_New.InGame.InputManager;
using Vector2 = Assets.Scripts.Simulation.SMath.Vector2;

[assembly: MelonInfo(typeof(PowersInShop.Main), "Powers In Shop", "1.2.3", "doombubbles")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace PowersInShop
{
    public class Main : BloonsTD6Mod
    {
        public override string MelonInfoCsURL =>
            "https://raw.githubusercontent.com/doombubbles/BTD6-Mods/main/PowersInShop/Main.cs";

        public override string LatestURL =>
            "https://github.com/doombubbles/BTD6-Mods/blob/main/PowersInShop/PowersInShop.dll?raw=true";


        private static readonly ModSettingInt CostBananaFarmer = 500;
        private static readonly ModSettingInt CostTechBotCost = 500;
        private static readonly ModSettingInt CostPontoon = 750;
        private static readonly ModSettingInt CostPortableLake = 750;
        private static readonly ModSettingInt CostEnergisingTotem = 1000;
        private static readonly ModSettingInt CostRoadSpikes = 50;
        private static readonly ModSettingInt CostGlueTrap = 100;
        private static readonly ModSettingInt CostCamoTrap = 100;
        private static readonly ModSettingInt CostMoabMine = 500;

        private static readonly ModSettingInt PierceRoadSpikes = 20;
        private static readonly ModSettingInt PierceGlueTrap = 300;
        private static readonly ModSettingInt PierceMoabMine = 1;
        private static readonly ModSettingInt PierceCamoTrap = 500;

        private static readonly ModSettingBool AllowInChimps = false;
        private static readonly ModSettingBool RestrictAsSupport = true;
        private static readonly ModSettingInt RechargePrice = 500;
        private static readonly ModSettingDouble AttackSpeedBoost = .15;


        public static readonly Dictionary<string, ModSettingInt> Powers = new Dictionary<string, ModSettingInt>
        {
            {"BananaFarmer", CostBananaFarmer},
            {"TechBot", CostTechBotCost},
            {"Pontoon", CostPontoon},
            {"PortableLake", CostPortableLake},
            {"EnergisingTotem", CostEnergisingTotem},
            {"RoadSpikes", CostRoadSpikes},
            {"GlueTrap", CostGlueTrap},
            {"CamoTrap", CostCamoTrap},
            {"MoabMine", CostMoabMine}
        };

        public static readonly Dictionary<string, ModSettingInt> TrackPowers = new Dictionary<string, ModSettingInt>
        {
            {"RoadSpikes", PierceRoadSpikes},
            {"GlueTrap", PierceGlueTrap},
            {"MoabMine", PierceMoabMine},
            {"CamoTrap", PierceCamoTrap}
        };

        public override void OnTitleScreen()
        {
            foreach (var towerModel in TrackPowers.Keys.Select(Utils.CreateTower))
            {
                AddTowerToGame(towerModel);
            }

            foreach (var power in Powers.Keys)
            {
                var powerModel = Game.instance.model.GetPowerWithName(power);

                if (powerModel.tower != null)
                {
                    if (powerModel.tower.icon == null)
                    {
                        powerModel.tower.icon = powerModel.icon;
                    }

                    powerModel.tower.cost = Powers[power];
                    powerModel.tower.towerSet = "Support";

                    if (power == "EnergisingTotem")
                    {
                        var behavior = powerModel.tower.GetBehavior<RateSupportModel>();
                        behavior.multiplier = (float) (1 - AttackSpeedBoost);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(InputManager), nameof(InputManager.EnterPlacementMode))]
        internal class InputManager_EnterPlacementMode
        {
            [HarmonyPostfix]
            internal static void Patch(TowerModel forTowerModel)
            {
                if (TrackPowers.ContainsKey(forTowerModel.name))
                {
                    var image = ShopMenu.instance.GetTowerButtonFromBaseId(forTowerModel.baseId).gameObject.transform
                        .Find("Icon").GetComponent<Image>();
                    InGameObjects.instance.PowerIconStart(image.sprite);
                }
            }
        }

        [HarmonyPatch(typeof(InputManager), nameof(InputManager.Update))]
        internal class InputManager_Update
        {
            [HarmonyPostfix]
            internal static void Patch()
            {
                var inputManager = InGame.instance.inputManager;
                var towerModel = inputManager.towerModel;
                if (towerModel != null && inputManager.inPlacementMode && TrackPowers.ContainsKey(towerModel.name))
                {
                    var map = InGame.instance.UnityToSimulation.simulation.Map;
                    InGameObjects.instance.PowerIconUpdate(inputManager.GetCursorPosition(),
                        map.CanPlace(new Vector2(inputManager.cursorPositionWorld), towerModel));
                }
            }
        }

        [HarmonyPatch(typeof(InputManager), nameof(InputManager.ExitPlacementMode))]
        internal class InputManager_ExitPlacementMode
        {
            [HarmonyPostfix]
            internal static void Patch()
            {
                if (InGameObjects.instance.powerIcon != null)
                {
                    InGameObjects.instance.PowerIconEnd();
                }
            }
        }

        [HarmonyPatch(typeof(TSMThemeEnergisingTotem), nameof(TSMThemeEnergisingTotem.OnButtonPress))]
        public class TSMThemeEnergisingTotem_OnButtonPress
        {
            [HarmonyPrefix]
            public static bool Prefix(TSMThemeEnergisingTotem __instance, TowerToSimulation tower)
            {
                if (tower.worth > 0)
                {
                    var cash = InGame.instance.bridge.simulation.cashManagers.entries[0].value.cash.Value;
                    if (cash < CostForDifficulty(RechargePrice, InGame.instance))
                    {
                        return false;
                    }

                    InGame.instance.bridge.simulation.cashManagers.entries[0].value.cash.Value =
                        cash - CostForDifficulty(RechargePrice, InGame.instance);
                    var mm = Game.instance.playerService.Player.Data.monkeyMoney.Value;
                    Game.instance.playerService.Player.Data.monkeyMoney.Value = mm + 20;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(TSMThemeEnergisingTotem), nameof(TSMThemeEnergisingTotem.Selected))]
        public class TSMThemeEnergisingTotem_Selected
        {
            public static bool lastOpened;
            private static string og;
            private static Color color;
            private static Color32 outline;

            [HarmonyPostfix]
            public static void Postfix(TSMThemeEnergisingTotem __instance, TowerToSimulation tower)
            {
                if (og == null)
                {
                    og = __instance.rechargeCostText.m_text;
                    color = __instance.rechargeCostText.color;
                    outline = __instance.rechargeCostText.outlineColor;
                }

                if (tower.worth > 0)
                {
                    __instance.rechargeCostText.SetText("$" + CostForDifficulty(RechargePrice, InGame.instance));
                    __instance.rechargeCostText.outlineColor = new Color32(0, 0, 0, 0);
                    __instance.rechargeCostText.color = Color.white;

                    if (!lastOpened)
                    {
                        __instance.rechargeButton.transform.GetChild(1).Translate(-5000, 0, 0);
                        __instance.rechargeButton.transform.GetChild(1).GetChild(0).Translate(5000, 0, 0);
                        __instance.rechargeButton.transform.GetChild(1).GetChild(1).Translate(4970, 0, 0);
                    }

                    lastOpened = true;
                }
                else
                {
                    __instance.rechargeCostText.SetText(og);
                    __instance.rechargeCostText.outlineColor = outline;
                    __instance.rechargeCostText.color = color;
                    if (lastOpened)
                    {
                        __instance.rechargeButton.transform.GetChild(1).Translate(5000, 0, 0);
                        __instance.rechargeButton.transform.GetChild(1).GetChild(0).Translate(-5000, 0, 0);
                        __instance.rechargeButton.transform.GetChild(1).GetChild(1).Translate(-4970, 0, 0);
                    }

                    lastOpened = false;
                }
            }
        }

        [HarmonyPatch(typeof(Map), nameof(Map.CanPlace))]
        internal class Map_CanPlace
        {
            [HarmonyPostfix]
            internal static void Patch(ref bool __result, Vector2 at, TowerModel tm)
            {
                if (TrackPowers.ContainsKey(tm.name))
                {
                    var map = InGame.instance.UnityToSimulation.simulation.Map;
                    __result = map.GetAllAreasOfTypeThatTouchPoint(at).ToArray()
                        .Any(area => area.areaModel.type == AreaType.track);
                }
            }
        }

        public override void OnProfileLoaded(ProfileModel result)
        {
            var unlockedTowers = result.unlockedTowers;

            foreach (var power in Powers.Keys.Where(power => Powers[power] > 0))
            {
                if (!unlockedTowers.Contains(power))
                {
                    unlockedTowers.Add(power);
                }
            }
        }

        [HarmonyPatch(typeof(Tower), nameof(Tower.Initialise))]
        public class Tower_Initialise
        {
            [HarmonyPostfix]
            public static void Postfix(Tower __instance)
            {
                if (TrackPowers.ContainsKey(__instance.towerModel.name))
                {
                    var powerBehaviorModel = Game.instance.model.GetPowerWithName(__instance.towerModel.name)
                        .GetBehavior<PowerBehaviorModel>();

                    InGame.instance.UnityToSimulation.simulation.powerManager.GetInstance(powerBehaviorModel)
                        .Activate(__instance.Position.ToVector2(), powerBehaviorModel, 0);
                }
            }
        }

        [HarmonyPatch(typeof(Tower), nameof(Tower.OnDestroy))]
        public class Tower_OnDestroy
        {
            [HarmonyPostfix]
            public static void Postfix(Tower __instance)
            {
                if (TrackPowers.ContainsKey(__instance.towerModel.name) &&
                    (!InGame.instance.IsCoop || __instance.owner == Game.instance.GetNkGI().PeerID))
                {
                    ShopMenu.instance.GetTowerButtonFromBaseId(__instance.towerModel.baseId).ButtonActivated();
                }
            }
        }

        public override void OnTowerInventoryInitialized(TowerInventory towerInventory, 
            Il2CppSystem.Collections.Generic.List<TowerDetailsModel> allTowersInTheGame)
        {
            var i = allTowersInTheGame.Count;
            foreach (var power in Powers.Keys.Where(power => Powers[power] >= 0))
            {
                var powerDetails = new ShopTowerDetailsModel(power, i++, 0, 0, 0, -1, 0, null);
                allTowersInTheGame.Add(powerDetails);
            }

            TSMThemeEnergisingTotem_Selected.lastOpened = false; //UI is reset, so we have to as well
        }

        [HarmonyPatch(typeof(UpgradeScreen), nameof(UpgradeScreen.UpdateUi))]
        public class UpgradeScreen_UpdateUi
        {
            [HarmonyPrefix]
            public static bool Prefix(ref string towerId)
            {
                foreach (var power in Powers.Keys)
                {
                    if (towerId.Contains(power))
                    {
                        towerId = TowerType.DartMonkey;
                    }
                }

                return true;
            }
        }

        private static void RestrictTowers()
        {
            if (RestrictAsSupport && InGame.instance.SelectedMode.Contains("Only") ||
                !AllowInChimps && InGame.instance.SelectedMode.Contains("Clicks"))
            {
                var towerInventory = InGame.instance.GetTowerInventory();
                foreach (var tower in towerInventory.towerDisplayOrder)
                {
                    if (Powers.ContainsKey(tower))
                    {
                        towerInventory.towerMaxes[tower] = 0;
                    }
                }
            }
        }
        
        public override void OnMatchStart()
        {
            RestrictTowers();
        }

        public override void OnRestart(bool removeSave)
        {
            RestrictTowers();
        }
    }
}