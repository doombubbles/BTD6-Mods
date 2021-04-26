using System;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts.Models.Towers;
using Assets.Scripts.Simulation.Towers.Behaviors;
using Assets.Scripts.Unity;
using Assets.Scripts.Unity.Bridge;
using Assets.Scripts.Unity.UI_New.InGame;
using Assets.Scripts.Unity.UI_New.InGame.TowerSelectionMenu;
using Assets.Scripts.Unity.UI_New.InGame.TowerSelectionMenu.TowerSelectionMenuThemes;
using Assets.Scripts.Unity.UI_New.Main;
using Assets.Scripts.Utils;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api.InGame_Mod_Options;
using Harmony;
using MelonLoader;
using UnhollowerRuntimeLib;
using UnityEngine;
using Image = UnityEngine.UI.Image;
using Main = TempleSacrificeHelper.Main;
using Object = UnityEngine.Object;

[assembly: MelonInfo(typeof(Main), "Temple Sacrifice Helper", "1.0.3", "doombubbles")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]
namespace TempleSacrificeHelper
{
    public class Main : BloonsTD6Mod
    {
        public override string MelonInfoCsURL =>
            "https://raw.githubusercontent.com/doombubbles/BTD6-Mods/main/TempleSacrificeHelper/Main.cs";

        public override string LatestURL =>
            "https://github.com/doombubbles/BTD6-Mods/blob/main/TempleSacrificeHelper/TempleSacrificeHelper.dll?raw=true";
        
        public static readonly ModSettingInt TempleAlternateCost = new ModSettingInt(50000)
        {
            displayName = "Alternate Sub Temple Cost",
            minValue = 0
        };
        public static readonly ModSettingInt GodAlternateCost = new ModSettingInt(100000)
        {
            displayName = "Alternate True Sun God Cost",
            minValue = 0
        };
        
        public static bool SacrificesOff = false;
        public static Sprite leftSprite = null;
        public static Sprite rightSprite = null;
        

        [HarmonyPatch(typeof(MonkeyTemple), nameof(MonkeyTemple.StartSacrifice))]
        public class MonkeyTemple_StartSacrifice
        {
            [HarmonyPrefix]
            public static bool Prefix()
            {
                return !SacrificesOff;
            }
        }
        
        [HarmonyPatch(typeof(TowerSelectionMenu), nameof(TowerSelectionMenu.OnUpdate))]
        public class TowerSelectionMenu_OnUpdate
        {
            [HarmonyPostfix]
            public static void Postfix(TowerSelectionMenu __instance)
            {
                if (__instance.upgradeButtons != null && __instance.selectedTower?.tower?.towerModel?.baseId == TowerType.SuperMonkey)
                {
                    if (SacrificesOff)
                    {
                        foreach (var instanceUpgradeButton in __instance.upgradeButtons)
                        {
                            var upgradeModel = instanceUpgradeButton.upgradeButton.GetUpgradeModel();
                            if (upgradeModel == null)
                            {
                                continue;
                            }
                            if (upgradeModel.name == "Sun Temple")
                            {
                                Utils.ModifyTemple(upgradeModel);
                            } else if (upgradeModel.name == "True Sun God")
                            {
                                Utils.ModifyGod(upgradeModel);
                            }
                            else
                            {
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
                            if (upgradeModel.name == "Sun Temple")
                            {
                                Utils.DefaultTemple(upgradeModel);
                            } else if (upgradeModel.name == "True Sun God")
                            {
                                Utils.DefaultGod(upgradeModel);
                            }
                            else
                            {
                                continue;
                            }
                            instanceUpgradeButton.UpdateCost();
                            instanceUpgradeButton.UpdateVisuals(0, false);
                        }
                    }
                }
            }
        }

        public override void OnMainMenu()
        {
            SacrificesOff = false;

            TSMTheme_Patch.text = null;
            TSMTheme_Patch.icons = null;
        }


        [HarmonyPatch(typeof(TSMThemeAmbidextrousRangs), nameof(TSMThemeAmbidextrousRangs.TowerInfoChanged))]
        public class TSMTheme_Patch {
            private static SpriteReference magicSprite = null;

            public static Dictionary<String, NK_TextMeshProUGUI> text = null;
            public static Dictionary<String, GameObject> icons = null;

            [HarmonyPostfix]
            public static void Postfix(TSMThemeAmbidextrousRangs __instance, TowerToSimulation tower) {
                if (text != null)
                {
                    foreach (var key in text.Keys)
                    {
                        text[key].gameObject.SetActiveRecursively(false);
                        icons[key].gameObject.SetActiveRecursively(false);
                    }
                }

                if (tower.Def.name.Contains(TowerType.SuperMonkey) && tower.Def.tiers[0] >= 3 && tower.Def.tiers[0] < 5) {
                    if (__instance.towerBackgroundImage.sprite == null) {
                        if (magicSprite == null) {
                            foreach (BaseTSMTheme t in TowerSelectionMenu.instance.themeManager.themes) {
                                if (t.GetIl2CppType().IsAssignableFrom(Il2CppType.Of<TSMThemeDefault>())) {
                                    TSMThemeDefault td = t.Cast<TSMThemeDefault>();
                                    if (td.magicSprite != null)
                                        magicSprite = td.magicSprite;
                                }
                            }
                        }
                        if (magicSprite != null) {
                            __instance.magicSprite = magicSprite;
                            ResourceLoader.LoadSpriteFromSpriteReferenceAsync(magicSprite,
                                __instance.towerBackgroundImage, false);
                        }
                    }
                    ;
                    

                    if (__instance.isMonkeyPortraitFlipped)
                    {
                        __instance.leftHandButton.gameObject.SetActive(false);
                        __instance.rightHandButton.gameObject.SetActive(true);
                    }
                    else
                    {
                        __instance.rightHandButton.gameObject.SetActive(false);
                        __instance.leftHandButton.gameObject.SetActive(true);
                    }

                    if (leftSprite == null && rightSprite == null)
                    {
                        leftSprite = __instance.leftHandButton.transform.Find("Icon").GetComponent<Image>().sprite;
                        rightSprite = __instance.rightHandButton.transform.Find("Icon").GetComponent<Image>().sprite;
                    }
                    
                    Utils.SetTexture(__instance.leftHandButton.transform.Find("Icon").GetComponent<Image>(), SacrificesOff ? "Off" : "On");
                    Utils.SetTexture(__instance.rightHandButton.transform.Find("Icon").GetComponent<Image>(), SacrificesOff ? "Off" : "On");


                    if (text == null)
                    {
                        text = new Dictionary<string, NK_TextMeshProUGUI>
                        {
                            ["Primary"] = Object.Instantiate(__instance.popCountText, __instance.transform, true),
                            ["Military"] = Object.Instantiate(__instance.popCountText, __instance.transform, true),
                            ["Magic"] = Object.Instantiate(__instance.popCountText, __instance.transform, true),
                            ["Support"] = Object.Instantiate(__instance.popCountText, __instance.transform, true)
                        };
                        
                        icons = new Dictionary<string, GameObject>
                        {
                            ["Primary"] = Object.Instantiate(__instance.gameObject.transform.Find("TSMPopInfoDefault").Find("PopImage").gameObject, __instance.transform, true),
                            ["Military"] = Object.Instantiate(__instance.gameObject.transform.Find("TSMPopInfoDefault").Find("PopImage").gameObject, __instance.transform, true),
                            ["Magic"] = Object.Instantiate(__instance.gameObject.transform.Find("TSMPopInfoDefault").Find("PopImage").gameObject, __instance.transform, true),
                            ["Support"] = Object.Instantiate(__instance.gameObject.transform.Find("TSMPopInfoDefault").Find("PopImage").gameObject, __instance.transform, true)
                        };

                        float unit = __instance.popCountText.fontSize / 2;
                        int i = -1;

                        foreach (var key in icons.Keys)
                        {
                            Utils.SetTexture(icons[key].transform.GetComponent<Image>(), key);
                            text[key].transform.Translate(0, i * unit, 0);
                            icons[key].transform.Translate(0, i * unit, 0);

                            i--;
                        }
                    }

                    if (!SacrificesOff)
                    {
                        var worths = Utils.GetTowerWorths(tower.tower);
                        var colors = Utils.GetColors(worths, tower.Def.tiers[0] == 4);
                        foreach (var key in text.Keys)
                        {
                            text[key].gameObject.SetActiveRecursively(true);
                            icons[key].gameObject.SetActiveRecursively(true);
                            text[key].SetText(
                                "$" + worths[key]);
                            text[key].color = colors[key];
                        }
                    }
                    

                } else if (tower.Def.baseId == TowerType.BoomerangMonkey && leftSprite != null && rightSprite != null)
                {
                    Utils.SetTexture(__instance.leftHandButton.transform.Find("Icon").GetComponent<Image>(), null, leftSprite);
                    Utils.SetTexture(__instance.rightHandButton.transform.Find("Icon").GetComponent<Image>(), null, rightSprite);
                }
            }
        }

        [HarmonyPatch(typeof(TSMThemeAmbidextrousRangs), nameof(TSMThemeAmbidextrousRangs.OnButtonPress))]
        public class Switch_Patch {

            [HarmonyPostfix]
            public static void Postfix(TSMThemeAmbidextrousRangs __instance, TowerToSimulation tower, TSMButton button) {
                if (tower.Def.name.Contains(TowerType.SuperMonkey) && tower.Def.tiers[0] >= 3 && tower.Def.tiers[0] < 5
                    && (button == __instance.leftHandButton || button == __instance.rightHandButton)) {
                    SacrificesOff = !SacrificesOff;
                    Utils.SetTexture(__instance.leftHandButton.transform.Find("Icon").GetComponent<Image>(), SacrificesOff ? "Off" : "On");
                    Utils.SetTexture(__instance.rightHandButton.transform.Find("Icon").GetComponent<Image>(), SacrificesOff ? "Off" : "On");
                    
                    foreach (var key in TSMTheme_Patch.text.Keys)
                    {
                        TSMTheme_Patch.text[key].gameObject.SetActiveRecursively(!SacrificesOff);
                        TSMTheme_Patch.icons[key].gameObject.SetActiveRecursively(!SacrificesOff);
                    }
                }
            }
        }

        private static void AddSwitch(TowerModel towerModel) => towerModel.towerSelectionMenuThemeId = "AmbidextrousRangs";
        public override void OnTitleScreen()
        {
            AddSwitch(Game.instance.model.GetTower(TowerType.SuperMonkey, 3, 0, 0));
            AddSwitch(Game.instance.model.GetTower(TowerType.SuperMonkey, 4, 0 ,0));
            for(int t = 1; t <= 2; t++) {
                AddSwitch(Game.instance.model.GetTower(TowerType.SuperMonkey, 3, t, 0));
                AddSwitch(Game.instance.model.GetTower(TowerType.SuperMonkey, 4, t ,0));
                    
                AddSwitch(Game.instance.model.GetTower(TowerType.SuperMonkey, 3, 0, t));
                AddSwitch(Game.instance.model.GetTower(TowerType.SuperMonkey, 4, 0 ,t));
            }
        }
    }
}