using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Assets.Scripts.Unity.UI_New.InGame;
using Assets.Scripts.Unity.UI_New.InGame.ActionMenu;
using Assets.Scripts.Unity.UI_New.InGame.RightMenu;
using BloonsTD6_Mod_Helper;
using BloonsTD6_Mod_Helper.Extensions;
using Harmony;
using MelonLoader;
using UnhollowerBaseLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

[assembly: MelonInfo(typeof(BetterAutoStart.Main), "Better Autostart", "1.0.0", "doombubbles")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace BetterAutoStart
{
    public class Main : BloonsTD6Mod
    {
        public static Il2CppStructArray<byte> Alt;

        public static Dictionary<string, Image> Images = new Dictionary<string, Image>()
        {
            {"FastForwardOff", null},
            {"FastForwardOn", null},
            {"Go!", null}
        };

        public override void OnApplicationStart()
        {
            var bitmap = Icons.ResourceManager.GetObject("Alt");
            if (bitmap != null)
            {
                MemoryStream memory = new MemoryStream();
                (bitmap as Bitmap)?.Save(memory, ImageFormat.Png);
                Alt = memory.ToArray();
                memory.Close();
            }
        }
        
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
                        Images[imageName].canvasRenderer.SetTexture(GetAltTexture());
                        Images[imageName].sprite = Sprite.Create(GetAltTexture(), img.sprite.textureRect, img.sprite.pivot);
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

        public static Texture2D GetAltTexture()
        {
            Texture2D texture = new Texture2D(1024, 1024);
            ImageConversion.LoadImage(texture, Alt);
            return texture;
        }


        /*RenderTexture tmp = RenderTexture.GetTemporary(image.mainTexture.width, image.mainTexture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        Graphics.Blit(image.mainTexture, tmp);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = tmp;
        Texture2D myTexture2D = new Texture2D(image.mainTexture.width, image.mainTexture.height);
        myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
        myTexture2D.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(tmp);
        var bytes = ImageConversion.EncodeToPNG(myTexture2D);
        if (bytes == null)
        {
            MelonLogger.Log("The bytes are null!");
        }
        File.WriteAllBytes("C:\\Users\\James\\Pictures\\go.png", bytes);*/
    }
}