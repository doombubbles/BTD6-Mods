using Assets.Scripts.Models.Towers;
using Assets.Scripts.Unity.UI_New.InGame.TowerSelectionMenu;
using BTD_Mod_Helper.Api.Enums;
using Harmony;

namespace TempleSacrificeHelper
{
    [HarmonyPatch(typeof(TowerSelectionMenu), nameof(TowerSelectionMenu.OnUpdate))]
    public class TowerSelectionMenu_OnUpdate
    {
        [HarmonyPostfix]
        public static void Postfix(TowerSelectionMenu __instance)
        {
            if (__instance.upgradeButtons != null &&
                __instance.selectedTower?.tower?.towerModel?.baseId == TowerType.SuperMonkey)
            {
                if (Main.templeSacrificesOff)
                {
                    foreach (var instanceUpgradeButton in __instance.upgradeButtons)
                    {
                        var upgradeModel = instanceUpgradeButton.upgradeButton.GetUpgradeModel();
                        if (upgradeModel == null)
                        {
                            continue;
                        }

                        switch (upgradeModel.name)
                        {
                            case UpgradeType.SunTemple:
                                Utils.ModifyTemple(upgradeModel);
                                break;
                            case UpgradeType.TrueSunGod:
                                Utils.ModifyGod(upgradeModel);
                                break;
                            default:
                                continue;
                        }

                        instanceUpgradeButton.UpdateCost();
                        instanceUpgradeButton.UpdateVisuals(0, false);
                    }
                }
                else
                {
                    foreach (var instanceUpgradeButton in __instance.upgradeButtons)
                    {
                        var upgradeModel = instanceUpgradeButton.upgradeButton.GetUpgradeModel();
                        if (upgradeModel == null)
                        {
                            continue;
                        }

                        switch (upgradeModel.name)
                        {
                            case UpgradeType.SunTemple:
                                Utils.DefaultTemple(upgradeModel);
                                break;
                            case UpgradeType.TrueSunGod:
                                Utils.DefaultGod(upgradeModel);
                                break;
                            default:
                                continue;
                        }

                        instanceUpgradeButton.UpdateCost();
                        instanceUpgradeButton.UpdateVisuals(0, false);
                    }
                }
            }
        }
    }
}