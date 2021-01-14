using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Assets.Scripts.Models.Towers.Upgrades;
using Assets.Scripts.Simulation.Towers;
using Assets.Scripts.Unity.UI_New.InGame;
using UnityEngine;
using Color = UnityEngine.Color;
using Image = UnityEngine.UI.Image;

namespace TempleSacrificeHelper
{
    public class Utils
    {
        public static Texture2D GetTexture(string name) {
            object bitmap = Icons.ResourceManager.GetObject(name);
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
        
        public static void DefaultTemple(UpgradeModel upgradeModel)
        {
            upgradeModel.confirmation = "Sun Temple";
            upgradeModel.cost = 100000;
        }
        
        public static void ModifyTemple(UpgradeModel upgradeModel)
        {
            upgradeModel.confirmation = "";
            upgradeModel.cost = Main.TempleAlternateCost;
        }
        
        public static void DefaultGod(UpgradeModel upgradeModel)
        {
            upgradeModel.confirmation = "True Sun Temple";
            upgradeModel.cost = 500000;
        }
        
        public static void ModifyGod(UpgradeModel upgradeModel)
        {
            upgradeModel.confirmation = "";
            upgradeModel.cost = Main.GodAlternateCost;
        }


        public static Dictionary<string, Color> GetColors(Dictionary<string, float> worths, bool god)
        {
            Dictionary<string, Color> ret = new Dictionary<string, Color>();
            if (!god)
            {
                string worst = "";
                float min = float.MaxValue;
                foreach (var key in worths.Keys)
                {
                    if (worths[key] < min)
                    {
                        worst = key;
                        min = worths[key];
                    }
                }
                ret[worst] = Color.red;
            }


            foreach (var key in worths.Keys)
            {
                if (ret.ContainsKey(key)) continue;
                float worth = worths[key];
                Color color = Color.red;
                if (worth > 50000)
                {
                    color = Color.green;
                } else if (key == "Magic")
                {
                    if (worth > 1000)
                    {
                        color = Color.white;
                    }
                } else if (worth > 300)
                {
                    color = Color.white;
                }
                
                ret[key] = color;
            }

            return ret;
        }

        public static Dictionary<string, float> GetTowerWorths(Tower tower)
        {
            return new Dictionary<string, float>()
            {
                ["Primary"] = MyGetTowerSetWorth("Primary", tower),
                ["Military"] = MyGetTowerSetWorth("Military", tower),
                ["Magic"] = MyGetTowerSetWorth("Magic", tower),
                ["Support"] = MyGetTowerSetWorth("Support", tower)
            };
        }

        private static float MyGetTowerSetWorth(string towerSet, Tower tower)
        {
            float total = 0;

            var allTowers = InGame.instance.UnityToSimulation.GetAllTowers();
            foreach (var tts in allTowers)
            {
                if (tts.tower.towerModel.towerSet != towerSet || tower.Id == tts.tower.Id)
                {
                    continue;
                }
                var po1 = new Assets.Scripts.Simulation.SMath.Vector2(tts.tower.Position.X, tts.tower.Position.Y);
                var po2 = new Assets.Scripts.Simulation.SMath.Vector2(tower.Position.X, tower.Position.Y);
                if (po1.Distance(po2) < tower.towerModel.range)
                {
                    total += tts.worth;
                }
            }

            return total;
        }
    }
}