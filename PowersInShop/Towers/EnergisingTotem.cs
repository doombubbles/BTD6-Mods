using Assets.Scripts.Models.Towers;
using Assets.Scripts.Models.Towers.Behaviors;
using Assets.Scripts.Unity.Bridge;
using Assets.Scripts.Unity.UI_New.InGame;
using Assets.Scripts.Unity.UI_New.InGame.TowerSelectionMenu.TowerSelectionMenuThemes;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using UnityEngine;

namespace PowersInShop.Towers
{
    public class EnergisingTotem : ModPowerTower
    {
        public override string BaseTower => TowerType.EnergisingTotem;
        public override int Cost => Main.EnergisingTotemCost;
        public override int Order => 5;

        public override void ModifyBaseTowerModel(TowerModel towerModel)
        {
            towerModel.GetBehavior<RateSupportModel>().multiplier = (float) (1 - Main.TotemAttackSpeed);

            towerModel.GetBehavior<EnergisingTotemBehaviorModel>().monkeyMoneyCost = 0;
        }


        [HarmonyPatch(typeof(TSMThemeEnergisingTotem), nameof(TSMThemeEnergisingTotem.OnButtonPress))]
        public class TSMThemeEnergisingTotem_OnButtonPress
        {
            [HarmonyPrefix]
            public static bool Prefix(TSMThemeEnergisingTotem __instance, TowerToSimulation tower)
            {
                if (tower.worth > 0)
                {
                    var cash = InGame.instance.GetCash();
                    var cost = BloonsTD6Mod.CostForDifficulty(Main.TotemRechargeCost, InGame.instance);
                    if (cash < cost)
                    {
                        return false;
                    }

                    InGame.instance.SetCash(cash - cost);

                    /*var mm = Game.instance.playerService.Player.Data.monkeyMoney.Value;
                    Game.instance.playerService.Player.Data.monkeyMoney.Value = mm + 20;*/
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
                    __instance.rechargeCostText.SetText("$" +
                                                        BloonsTD6Mod.CostForDifficulty(Main.TotemRechargeCost,
                                                            InGame.instance));
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
    }
}