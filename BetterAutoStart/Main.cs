using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Assets.Scripts.Unity.UI_New.InGame;
using Assets.Scripts.Unity.UI_New.InGame.ActionMenu;
using Assets.Scripts.Unity.UI_New.InGame.RightMenu;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Extensions;
using Harmony;
using MelonLoader;
using UnhollowerBaseLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

[assembly: MelonInfo(typeof(BetterAutoStart.Main), "Better Autostart", "1.0.2", "doombubbles")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace BetterAutoStart
{
    public class Main : BloonsTD6Mod
    {
        public override string MelonInfoCsURL =>
            "https://raw.githubusercontent.com/doombubbles/BTD6-Mods/main/BetterAutoStart/Main.cs";

        public override string LatestURL =>
            "https://github.com/doombubbles/BTD6-Mods/blob/main/BetterAutoStart/BetterAutoStart.dll?raw=true";
    
        public static Dictionary<string, Image> Images = new Dictionary<string, Image>
        {
            {"FastForwardOff", null},
            {"FastForwardOn", null},
            {"Go!", null}
        };
        
        public static void UpdateTextures()
        {
            if (ShopMenu.instance == null) return;

            var images = ShopMenu.instance.goButtonBtn.GetComponentsInChildren<Image>();

            foreach (var imageName in Images.Keys.Duplicate())
            {
                if (Images[imageName] == null || images.All(i => i.name != imageName + "(Clone)"))
                {
                    var img = images.FirstOrDefault(i => i.name == imageName);
                    if (img != null)
                    {
                        Images[imageName] = Object.Instantiate(img, img.transform.parent, true);
                        var texture = ModContent.GetTexture<Main>("alt");
                        Images[imageName].canvasRenderer.SetTexture(texture);
                        Images[imageName].sprite = Sprite.Create(texture, img.sprite.textureRect, img.sprite.pivot);
                        Images[imageName].enabled = false;
                    }
                }
            }
            
            foreach (var image in Images.Values.Where(image => image != null))
            {
                image.enabled = false;
            }
            
            if (InGame.instance.autoPlay)
            {
                images = ShopMenu.instance.goButtonBtn.GetComponentsInChildren<Image>();
                foreach (var image in images)
                {
                    var auto = images.FirstOrDefault(i => i.name == image.name + "(Clone)");
                    if (!(auto is null))
                    {
                        auto.enabled = true;
                    }
                }
            }

        }


        [HarmonyPatch(typeof(GoFastForwardToggle), nameof(GoFastForwardToggle.LateUpdate))]
        internal class GoFastForwardToggle_LateUpdate
        {
            [HarmonyPostfix]
            internal static void Postfix()
            {
                UpdateTextures();
            }
        }


        [HarmonyPatch(typeof(Button), nameof(Button.OnPointerClick))]
        internal class Button_OnPointerClick
        {
            [HarmonyPostfix]
            internal static void Postfix(Button __instance, PointerEventData eventData)
            {
                if (InGame.instance != null && eventData.button == PointerEventData.InputButton.Right
                                            && __instance.name == "FastFoward-Go") //yes this is a real typo in the name
                {
                    InGame.instance.ToggleAutoPlay(!InGame.instance.autoPlay);
                }
            }
        }
    }
}