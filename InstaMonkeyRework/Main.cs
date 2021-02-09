using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Models.Powers;
using Assets.Scripts.Models.Profile;
using Assets.Scripts.Models.Towers;
using Assets.Scripts.Simulation.Towers;
using Assets.Scripts.Unity;
using Assets.Scripts.Unity.Player;
using Assets.Scripts.Unity.UI_New.InGame;
using Assets.Scripts.Unity.UI_New.InGame.RightMenu;
using Assets.Scripts.Unity.UI_New.InGame.RightMenu.Powers;
using Assets.Scripts.Unity.UI_New.InGame.StoreMenu;
using Assets.Scripts.Unity.UI_New.Popups;
using BloonsTD6_Mod_Helper;
using BloonsTD6_Mod_Helper.Extensions;
using Harmony;
using MelonLoader;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

[assembly: MelonInfo(typeof(InstaMonkeyRework.Main), "Insta Monkey Rework", "1.0.1", "doombubbles")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace InstaMonkeyRework
{
    public class Main : BloonsTD6Mod
    {
        public static bool ActuallyConsumeInsta = false;
        public static bool AllowActuallyPlaceInsta = false;

        public static Dictionary<int, string> SavedPlacedInstas;

        public override void OnMainMenu()
        {
            SavedPlacedInstas = new Dictionary<int, string>();
        }

        public override void OnMatchStart()
        {
            SavedPlacedInstas = new Dictionary<int, string>();
        }

        public override void OnRestart(bool removeSave)
        {
            SavedPlacedInstas = new Dictionary<int, string>();
        }

        public static int GetCostForThing(TowerModel towerModel)
        {
            var cost = Game.instance.model.GetTowerWithName(towerModel.name).cost;
            foreach (var appliedUpgrade in towerModel.GetAppliedUpgrades())
            {
                cost += appliedUpgrade.cost;
            }

            switch (InGame.instance.SelectedDifficulty)
            {
                case "Easy":
                    cost *= .85f;
                    break;
                case "Hard":
                    cost *= 1.08f;
                    break;
                case "Impoppable":
                    cost *= 1.2f;
                    break;
            }

            cost *= 1 - .05f * towerModel.tier;
            return (int) (5 * Math.Round(cost / 5));
        }

        public static int GetCostForThing(Tower tower)
        {
            var cost = Game.instance.model.GetTowerWithName(tower.towerModel.name).cost;

            var towerManager = InGame.instance.GetTowerManager();
            var zoneDiscount = towerManager.GetZoneDiscount(tower.Position.ToVector3(), 0, 0);
            var discountMultiplier = towerManager.GetDiscountMultiplier(zoneDiscount);
            cost *= 1 - discountMultiplier;

            foreach (var appliedUpgrade in tower.towerModel.GetAppliedUpgrades())
            {
                float upgradeCost = appliedUpgrade.cost;
                zoneDiscount = towerManager.GetZoneDiscount(tower.Position.ToVector3(), appliedUpgrade.path,
                    appliedUpgrade.tier);
                discountMultiplier = towerManager.GetDiscountMultiplier(zoneDiscount);
                upgradeCost *= 1 - discountMultiplier;
                cost += upgradeCost;
            }

            switch (InGame.instance.SelectedDifficulty)
            {
                case "Easy":
                    cost *= .85f;
                    break;
                case "Hard":
                    cost *= 1.08f;
                    break;
                case "Impoppable":
                    cost *= 1.2f;
                    break;
            }

            cost *= 1 - .05f * tower.towerModel.tier;
            return (int) (5 * Math.Round(cost / 5));
        }

        public static int GetTotalPlaced(string name)
        {
            return SavedPlacedInstas.Values.Count(value => value == name);
        }

        public override void OnUpdate()
        {
            if (InstaTowersMenu.instaTowersInstance != null && InGame.instance != null
            && InGame.instance.bridge != null)
            {
                foreach (var button in InstaTowersMenu.instaTowersInstance
                    .GetComponentsInChildren<StandardInstaTowerButton>())
                {
                    var costText = button.GetComponentsInChildren<TextMeshProUGUI>()
                        .FirstOrDefault(text => text.name == "Cost");
                    if (costText == null) continue;
                    var cost = TextToInt(costText);
                    costText.color = InGame.instance.GetCash() >= cost ? Color.white : Color.red;


                    var useCount = button.GetUseCount();
                    var placed = GetTotalPlaced(button.instaTowerModel.name);
                    var discountText = button.GetComponentsInChildren<TextMeshProUGUI>()
                        .First(text => text.name == "Discount");
                    if (placed >= useCount)
                    {
                        costText.enabled = false;
                        discountText.enabled = false;
                        button.powerCountText.color = Color.red;
                    }
                    else
                    {
                        costText.enabled = true;
                        discountText.enabled = true;
                        button.powerCountText.color = Color.white;
                    }
                }
            }
        }

        public static int TextToInt(TextMeshProUGUI textMeshProUGUI)
        {
            return int.Parse(textMeshProUGUI.text.Substring(1).Replace(",", ""));
        }

        public override void OnTowerSaved(Tower tower, TowerSaveDataModel saveData)
        {
            if (SavedPlacedInstas.ContainsKey(tower.Id))
            {
                saveData.metaData["InstaMonkeyRework"] = SavedPlacedInstas[tower.Id];
            }
        }

        public override void OnTowerLoaded(Tower tower, TowerSaveDataModel saveData)
        {
            if (saveData.metaData.ContainsKey("InstaMonkeyRework"))
            {
                SavedPlacedInstas[tower.Id] = saveData.metaData["InstaMonkeyRework"];
            }
        }

        public override void OnTowerDestroyed(Tower tower)
        {
            if (SavedPlacedInstas.ContainsKey(tower.Id))
            {
                SavedPlacedInstas.Remove(tower.Id);
            }
        }

        [HarmonyPatch(typeof(InputManager), nameof(InputManager.Update))]
        internal class InputManager_EnterInstaMode
        {
            [HarmonyPostfix]
            internal static void Postfix(InputManager __instance)
            {
                if (!__instance.inInstaMode || AllowActuallyPlaceInsta)
                {
                    return;
                }

                var useCount = __instance.instaButton.GetUseCount();
                var placed = GetTotalPlaced(__instance.instaModel.name);
                var cost = GetCostForThing(__instance.instaModel);
                if (placed >= useCount || cost > InGame.instance.GetCash())
                {
                    PopupScreen.instance.ShowPopup(PopupScreen.Placement.inGameCenter, "Real Insta Warning",
                        "You are placing an actual Insta Monkey, and doing so will remove it from your inventory. Are you sure you want to continue?",
                        null, "Yes",
                        new Action(__instance.CancelAllPlacementActions), "No", Popup.TransitionAnim.Scale
                    );
                }

                AllowActuallyPlaceInsta = true;
            }
        }

        [HarmonyPatch(typeof(InputManager), nameof(InputManager.TryPlace))]
        internal class InputManager_TryPlace
        {
            [HarmonyPrefix]
            internal static bool Prefix()
            {
                return !(PopupScreen.instance != null && PopupScreen.instance.IsPopupActive());
            }
        }

        [HarmonyPatch(typeof(InputManager), nameof(InputManager.ExitInstaMode))]
        internal class InputManager_ExitInstaMode
        {
            [HarmonyPostfix]
            internal static void Postfix(InputManager __instance)
            {
                AllowActuallyPlaceInsta = false;
            }
        }


        [HarmonyPatch(typeof(Btd6Player), nameof(Btd6Player.ConsumeInstaTower))]
        internal class Btd6Player_ConsumeInstaTower
        {
            [HarmonyPrefix]
            internal static bool Prefix()
            {
                if (ActuallyConsumeInsta)
                {
                    ActuallyConsumeInsta = false;
                    return true;
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(TowerManager), nameof(TowerManager.CreateTower))]
        internal class TowerManager_CreateTower
        {
            [HarmonyPostfix]
            internal static void Postfix(Tower __result, TowerModel def, bool isInstaTower)
            {
                if (isInstaTower && (!InGame.instance.IsCoop || __result.owner == Game.instance.nkGI.PeerID))
                {
                    var cost = GetCostForThing(def);
                    if (InGame.instance.GetCash() >= cost)
                    {
                        cost = GetCostForThing(__result);
                        InGame.instance.AddCash(-cost);
                        __result.worth = cost;
                        SavedPlacedInstas[__result.Id] = def.name;
                    }
                    else
                    {
                        ActuallyConsumeInsta = true;
                        Game.instance.GetBtd6Player().ConsumeInstaTower(def.baseId, def.tiers);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(StandardInstaTowerButton), nameof(StandardInstaTowerButton.UpdateUseCount))]
        internal class StandardInstaTowerButton_UpdateUseCount
        {
            [HarmonyPostfix]
            internal static void Postfix(StandardInstaTowerButton __instance, int useCount)
            {
                var amountAvailable = useCount - GetTotalPlaced(__instance.instaTowerModel.name);
                __instance.powerCountText.SetText(amountAvailable + "/" + useCount);
            }
        }

        [HarmonyPatch(typeof(StandardInstaTowerButton), nameof(StandardInstaTowerButton.SetPower))]
        internal class StandardInstaTowerButton_SetPower
        {
            [HarmonyPostfix]
            internal static void Postfix(StandardInstaTowerButton __instance, PowerModel powerModel, bool isInsta)
            {
                var costText = __instance.GetComponentsInChildren<TextMeshProUGUI>()
                    .FirstOrDefault(text => text.name == "Cost");

                float unit = __instance.tiers.fontSize / 3;
                if (costText == null)
                {
                    costText = Object.Instantiate(__instance.tiers, __instance.tiers.transform.parent, true);
                    costText.name = "Cost";
                    costText.transform.Translate(0, unit, 0);
                    costText.color = Color.red;
                }

                var cost = GetCostForThing(powerModel.tower);
                costText.SetText($"${cost:n0}");

                var tier = __instance.instaTowerModel.tier;
                var discountText = __instance.GetComponentsInChildren<TextMeshProUGUI>()
                    .FirstOrDefault(text => text.name == "Discount");
                if (discountText == null)
                {
                    discountText = Object.Instantiate(__instance.powerCountText,
                        __instance.powerCountText.transform.parent, true);
                    discountText.name = "Discount";
                    discountText.transform.Translate(unit * 3, 0, 0);
                    discountText.color = Color.green;
                }

                if (tier > 0)
                {
                    discountText.SetText("-" + tier * 5 + "%");
                }
                else
                {
                    discountText.SetText("");
                }
            }
        }

        [HarmonyPatch(typeof(TowerPurchaseButton), nameof(TowerPurchaseButton.OnPointerClick))]
        internal class TowerPurchaseButton_OnPointerClick
        {
            [HarmonyPostfix]
            internal static void Postfix(TowerPurchaseButton __instance, PointerEventData eventData)
            {
                if (eventData.button == PointerEventData.InputButton.Right && __instance.baseTowerModel.IsBaseTower &&
                    !__instance.hero && InGame.instance.GetGameModel().powersEnabled)
                {
                    RightMenu.instance.ShowPowersMenu();
                    PowersMenu.instance.ShowInstaMonkeys();
                    InstaTowersMenu.instaTowersInstance.Show(__instance.baseTowerModel);

                    InGame.instance.InputManager.ExitTowerMode();
                }
            }
        }
    }
}