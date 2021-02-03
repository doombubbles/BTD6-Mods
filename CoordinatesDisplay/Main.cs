using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Assets.Scripts.Unity;
using Assets.Scripts.Unity.UI_New.InGame;
using Assets.Scripts.Unity.UI_New.InGame.TowerSelectionMenu;
using Assets.Scripts.Unity.UI_New.Popups;
using Assets.Scripts.Utils;
using Harmony;
using MelonLoader;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using Object = UnityEngine.Object;

[assembly: MelonInfo(typeof(CoordinatesDisplay.Main), "Coordinates Display", "1.2.1", "doombubbles")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]
namespace CoordinatesDisplay
{
    public class Main : MelonMod
    {
        public static GameObject display;
        public static AssetBundle assetBundle;

        public static bool showDisplay = true;

        public static bool definitelyTryPlaceNext;

        public static bool changePopup = false;
        
        public static void Place(float x, float y)
        {
            var inputManager = InGame.instance.inputManager;
            inputManager.towerPositionWorld = new Vector2(x, y);
            definitelyTryPlaceNext = true;
            inputManager.TryPlace();
        }
        
        
        [HarmonyPatch(typeof(InputManager), nameof(InputManager.TryPlace))]
        internal class InputManager_TryPlace
        {
            [HarmonyPrefix]
            internal static bool Prefix()
            {
                if (definitelyTryPlaceNext)
                {
                    definitelyTryPlaceNext = false;
                    return true;
                }

                return !(PopupScreen.instance != null && PopupScreen.instance.IsPopupActive());
            }
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Insert) && InGame.instance != null)
            {
                var inputManager = InGame.instance.inputManager;
                if (inputManager.inPlacementMode)
                {
                    PopupScreen.instance.ShowSetNamePopup("Place Tower at Coordinates", 
                        "Use the form X.x, Y.y like \"42.0, 6.9\" (note the comma)",
                        new Action<string>(s =>
                        {
                            var split = s.Split(',');
                            float x = float.Parse(split[0].Trim());
                            float y = float.Parse(split[1].Trim());
                            
                            Place(x, y);
                        }), Math.Round(inputManager.towerPositionWorld.x, 1) + ", " + Math.Round(inputManager.towerPositionWorld.y, 1));
                    changePopup = true;
                }
                else
                {
                    showDisplay = !showDisplay;
                }
            }

            if (changePopup && PopupScreen.instance.GetFirstActivePopup() != null)
            {
                PopupScreen.instance.GetFirstActivePopup().GetComponentInChildren<TMP_InputField>()
                    .characterValidation = TMP_InputField.CharacterValidation.None;
                changePopup = false;
            }


            if (Game.instance == null || InGame.instance == null || InGame.Bridge == null || !showDisplay
                || (TowerSelectionMenu.instance != null && TowerSelectionMenu.instance.GetSelectedTower() != null))
            {
                HideMMDisplay();
                return;
            }

            if (display is null)
                CreateMMDisplay();

            ShowMMDisplay();
        }
        
        private void HideMMDisplay()
        {
            if (display != null)
                display.SetActive(false);
        }

        private void ShowMMDisplay()
        {
            if (!display.active)
                display.SetActive(true);

            var inputManager = InGame.instance.inputManager;

            display.GetComponentInChildren<Text>().text = inputManager.cursorPositionWorld.ToString();
        }
        
        private void CreateMMDisplay()
        {
            var resource = Resources.display;
            assetBundle = AssetBundle.LoadFromMemory(resource);

            var canvas = assetBundle.LoadAsset("Canvas").Cast<GameObject>();
            SetTexture(canvas.transform.Find("Image").GetComponent<Image>(), "image");
            
            display = Object.Instantiate(canvas).Cast<GameObject>();
        }

        public static Texture2D GetTexture(string name) {
            object bitmap = Resources.ResourceManager.GetObject(name);
            if (bitmap != null) {
                MemoryStream memory = new MemoryStream();
                (bitmap as Bitmap)?.Save(memory, ImageFormat.Png);
                Texture2D texture = new Texture2D(0, 0);
                ImageConversion.LoadImage(texture, memory.ToArray());
                memory.Close();
                return texture;
            }
            return null;
        }
        public static void SetTexture(Image image, string name) {
            Texture2D texture = GetTexture(name);
            SetTexture(image, texture);
        }
        
        public static void SetTexture(Image image, Texture2D texture = null, Sprite sprite = null)
        {
            if (sprite == null)
            {
                sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2());
            } else if (texture == null)
            {
                texture = sprite.texture;
            }
            image.canvasRenderer.SetTexture(texture);
            image.sprite = sprite;
        }
    }
}