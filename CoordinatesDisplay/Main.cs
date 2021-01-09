using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Assets.Scripts.Unity;
using Assets.Scripts.Unity.UI_New.InGame;
using Assets.Scripts.Unity.UI_New.Popups;
using Harmony;
using MelonLoader;
using NKHook6.Api.Events;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using Object = UnityEngine.Object;

[assembly: MelonInfo(typeof(CoordinatesDisplay.Main), "Coordinates Display", "1.0.0", "doombubbles")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]
namespace CoordinatesDisplay
{
    public class Main : MelonMod
    {
        public static GameObject display;
        public static AssetBundle assetBundle;

        public static bool definitelyTryPlaceNext;

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();
            EventRegistry.instance.listen(typeof(Main));
        }
        
        [EventAttribute("KeyPressEvent")]
        public static void onEvent(KeyEvent e)
        {
            KeyCode key = e.key;
            if (key == KeyCode.Insert && InGame.instance != null)
            {
                var inputManager = InGame.instance.inputManager;
                if (inputManager.inPlacementMode)
                {
                    PopupScreen.instance.ShowSetValuePopup("Set X Coordinate", 
                        "Ninja Kiwi doesn't like dots, so type in the coordinate you want multiplied by 10. For example, \"207\" for 20.7",
                        new System.Action<int>(x =>
                        {
                            PopupScreen.instance.ShowSetValuePopup("Set Y Coordinate", 
                                "Ninja Kiwi doesn't like dots, so type in the coordinate you want multiplied by 10. For example, \"207\" for 20.7",
                                new System.Action<int>(y =>
                                {
                                    Place(x/ 10f, y / 10f);
                                }), (int)inputManager.towerPositionWorld.y * 10) ;
                        }), (int)inputManager.towerPositionWorld.x * 10);
                    
                    
                }
            }
        }

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
            if (Game.instance == null || InGame.instance == null || InGame.Bridge == null)
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