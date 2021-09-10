using System;
using System.Collections.Generic;
using Assets.Scripts.Unity.Bridge;
using Assets.Scripts.Unity.UI_New.InGame.TowerSelectionMenu.TowerSelectionMenuThemes;
using Assets.Scripts.Unity.Utils;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Extensions;
using Harmony;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using static TempleSacrificeHelper.Main;
using Object = UnityEngine.Object;

namespace TempleSacrificeHelper
{
    [HarmonyPatch(typeof(TSMThemeDefault), nameof(TSMThemeDefault.TowerInfoChanged))]
    public class TSMThemeDefault_TowerInfoChanged
    {
        public static Dictionary<string, NK_TextMeshProUGUI> templeText;
        public static Dictionary<string, GameObject> templeIcons;
        public static GameObject templeInfoButton;

        public static GameObject paragonButton;
        public static NK_TextMeshProUGUI paragonButtonText;
        public static Dictionary<string, NK_TextMeshProUGUI> paragonText;
        public static Dictionary<string, GameObject> paragonIcons;

        public static bool showingCategories;

        [HarmonyPostfix]
        public static void Postfix(TSMThemeDefault __instance, TowerToSimulation tower)
        {
            if (templeText != null && templeIcons != null)
            {
                foreach (var key in templeText.Keys)
                {
                    templeText[key].gameObject.SetActiveRecursively(false);
                    templeIcons[key].SetActiveRecursively(false);
                }

                templeInfoButton.SetActiveRecursively(false);
            }
            
            if (paragonText != null && paragonIcons != null)
            {
                foreach (var key in paragonText.Keys)
                {
                    paragonText[key].gameObject.SetActiveRecursively(false);
                    paragonIcons[key].SetActiveRecursively(false);
                }
            }

            if (paragonButton != null && paragonButtonText != null)
            {
                paragonButton.SetActiveRecursively(false);
                paragonButtonText.gameObject.SetActiveRecursively(false);
            }

            if (tower.Def.upgrades.Any(model =>
                model.upgrade == SunTemple || model.upgrade == TrueSunGod))
            {
                if (templeText == null || templeIcons == null)
                {
                    SetUpTempleUI(__instance);
                }

                if (!templeSacrificesOff && templeText != null && templeIcons != null)
                {
                    var worths = Utils.GetTowerWorths(tower.tower);
                    var colors = Utils.GetColors(worths, tower.Def.tiers[0] == 4);
                    foreach (var key in templeText.Keys)
                    {
                        templeText[key].gameObject.SetActiveRecursively(true);
                        templeIcons[key].SetActiveRecursively(true);
                        templeText[key].SetText("$" + worths[key]);
                        templeText[key].color = colors[key];
                    }
                }

                templeInfoButton.SetActiveRecursively(true);
            }
            else if (tower.CanUpgradeToParagon())
            {
                if (paragonButton == null || paragonButtonText == null)
                {
                    SetUpParagonUI(__instance);
                }

                var i = Utils.GetParagonDegree(tower, out var investmentInfo);
                
                if (paragonText != null && paragonIcons != null && showingCategories)
                {
                    foreach (var key in paragonText.Keys)
                    {
                        paragonText[key].gameObject.SetActiveRecursively(true);
                        paragonIcons[key].SetActiveRecursively(true);
                    }

                    paragonText["Pops"].SetText(((int)investmentInfo.powerFromPops).ToString());
                    paragonText["Cash"].SetText(((int)investmentInfo.powerFromMoneySpent).ToString());
                    paragonText["NonTier5s"].SetText(((int)investmentInfo.powerFromNonTier5Tiers).ToString());
                    paragonText["Tier5s"].SetText(((int)investmentInfo.powerFromTier5Count).ToString());

                    paragonText["Pops"].color = investmentInfo.powerFromPops >= MaxFromPops ? Color.green : Color.white;
                    paragonText["Cash"].color = investmentInfo.powerFromMoneySpent >= MaxFromCash ? Color.green : Color.white;
                    paragonText["NonTier5s"].color = investmentInfo.powerFromNonTier5Tiers >= MaxFromNonTier5s ? Color.green : Color.white;
                    paragonText["Tier5s"].color = investmentInfo.powerFromTier5Count >= MaxFromTier5s ? Color.green : Color.white;

                }

                paragonButtonText.SetText(i.ToString());
                paragonButtonText.gameObject.SetActiveRecursively(true);
                paragonButton.SetActiveRecursively(true);
                
            }
        }

        private static void SetUpParagonUI(TSMThemeDefault __instance)
        {
            var popImage = __instance.gameObject.GetComponentInChildrenByName<Image>("PopImage");
            var info = __instance.gameObject.GetComponentInChildrenByName<TSMButton>("InfoToggleButton");
            var rightButton = __instance.gameObject.GetComponentInChildrenByName<RectTransform>("ForwardsButton");
            var rightButtonPosition = rightButton.position;

            paragonButton = Object.Instantiate(info.gameObject, __instance.transform, true);
            var transformPosition = paragonButton.transform.position;
            transformPosition.x = rightButtonPosition.x;
            paragonButton.transform.position = transformPosition;

            paragonButton.GetComponent<Button>().SetOnClick(() =>
            {
                // eventually toggle buttons to see individual power sources
                showingCategories = !showingCategories;
                foreach (var key in paragonText.Keys)
                {
                    paragonText[key].gameObject.SetActiveRecursively(showingCategories);
                    paragonIcons[key].SetActiveRecursively(showingCategories);
                }
            });

            paragonButtonText = Object.Instantiate(__instance.popCountText, paragonButton.transform, false);
            paragonButtonText.alignment = TextAlignmentOptions.Center;
            transformPosition = paragonButtonText.transform.position;
            transformPosition.x = rightButtonPosition.x * .96f;
            paragonButtonText.transform.position = transformPosition;
            
            paragonText = new Dictionary<string, NK_TextMeshProUGUI>
            {
                ["Cash"] = Object.Instantiate(__instance.popCountText, __instance.transform, true),
                ["Pops"] = Object.Instantiate(__instance.popCountText, __instance.transform, true),
                ["NonTier5s"] = Object.Instantiate(__instance.popCountText, __instance.transform, true),
                ["Tier5s"] = Object.Instantiate(__instance.popCountText, __instance.transform, true)
            };

            paragonIcons = new Dictionary<string, GameObject>
            {
                ["Cash"] = Object.Instantiate(popImage.gameObject, __instance.transform, true),
                ["Pops"] = Object.Instantiate(popImage.gameObject, __instance.transform, true),
                ["NonTier5s"] = Object.Instantiate(popImage.gameObject, __instance.transform, true),
                ["Tier5s"] = Object.Instantiate(popImage.gameObject, __instance.transform, true)
            };
            
            var i = -1;
            var unit = __instance.popCountText.fontSize / 2f;
            foreach (var key in paragonIcons.Keys)
            {
                paragonText[key].transform.Translate(0, i * unit, 0);
                paragonIcons[key].transform.Translate(0, i * unit, 0);
                i--;
            }
            
            AtlasLateBinding.Instance.OnAtlasRequested("Ui", new Action<SpriteAtlas>(atlas =>
            {
                paragonIcons["NonTier5s"].GetComponent<Image>().SetSprite(atlas.GetSprite("UpgradeContainerBlue"));
                paragonIcons["Tier5s"].GetComponent<Image>().SetSprite(atlas.GetSprite("UpgradeContainerTier5"));
                paragonIcons["Cash"].GetComponent<Image>().SetSprite(atlas.GetSprite("CoinIcon"));
                
                paragonButton.GetComponent<Image>().SetSprite(atlas.GetSprite("UpgradeContainerParagon"));
            }));
            
        }

        private static void SetUpTempleUI(TSMThemeDefault __instance)
        {
            var popImage = __instance.gameObject.GetComponentInChildrenByName<Image>("PopImage");
            var info = __instance.gameObject.GetComponentInChildrenByName<TSMButton>("InfoToggleButton");
            var rightButton =
                __instance.gameObject.GetComponentInChildrenByName<RectTransform>("ForwardsButton");

            templeInfoButton = Object.Instantiate(info.gameObject, __instance.transform, true);
            var transformPosition = templeInfoButton.transform.position;
            transformPosition.x = rightButton.position.x;
            templeInfoButton.transform.position = transformPosition;


            var image = templeInfoButton.GetComponent<Image>();
            image.SetSprite(ModContent.GetSprite<Main>(templeSacrificesOff ? "Off" : "On"));

            templeInfoButton.GetComponent<Button>().SetOnClick(() =>
            {
                templeSacrificesOff = !templeSacrificesOff;
                foreach (var key in templeText.Keys)
                {
                    templeText[key].gameObject.SetActiveRecursively(!templeSacrificesOff);
                    templeIcons[key].SetActiveRecursively(!templeSacrificesOff);
                }
                image.SetSprite(ModContent.GetSprite<Main>(templeSacrificesOff ? "Off" : "On"));
            });

            templeText = new Dictionary<string, NK_TextMeshProUGUI>
            {
                ["Primary"] = Object.Instantiate(__instance.popCountText, __instance.transform, true),
                ["Military"] = Object.Instantiate(__instance.popCountText, __instance.transform, true),
                ["Magic"] = Object.Instantiate(__instance.popCountText, __instance.transform, true),
                ["Support"] = Object.Instantiate(__instance.popCountText, __instance.transform, true)
            };

            templeIcons = new Dictionary<string, GameObject>
            {
                ["Primary"] = Object.Instantiate(popImage.gameObject, __instance.transform, true),
                ["Military"] = Object.Instantiate(popImage.gameObject, __instance.transform, true),
                ["Magic"] = Object.Instantiate(popImage.gameObject, __instance.transform, true),
                ["Support"] = Object.Instantiate(popImage.gameObject, __instance.transform, true)
            };
            var i = -1;


            var unit = __instance.popCountText.fontSize / 2f;
            foreach (var key in templeIcons.Keys)
            {
                var icon = templeIcons[key].GetComponent<Image>();
                AtlasLateBinding.Instance.OnAtlasRequested("MainMenuUiAtlas", new Action<SpriteAtlas>(
                    atlas =>
                    {
                        var sprite = atlas.GetSprite(key + "Btn");
                        //sprite.texture.mipMapBias = -1;
                        sprite.texture.filterMode = FilterMode.Bilinear;
                        icon.SetSprite(sprite);
                    }));
                templeText[key].transform.Translate(0, i * unit, 0);
                templeIcons[key].transform.Translate(0, i * unit, 0);
                i--;
            }
        }
    }
}