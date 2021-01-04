using System.IO;
using System.Linq;
using Assets.Scripts.Models.Towers.Behaviors.Attack;
using Assets.Scripts.Unity;
using Harmony;
using MelonLoader;

[assembly: MelonInfo(typeof(BetterUltraVision.Main), "Better UltraVision", "1.1.0", "doombubbles")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]
namespace BetterUltraVision
{
    public class Main : MelonMod
    {
        private static readonly string Dir = $"{Directory.GetCurrentDirectory()}\\Mods\\BetterUltraVison";
        private static readonly string Config = $"{Dir}\\config.txt";

        private static int UltravisionRangeBonus = 6;
        private static int UltravisionCost = 1200;
        private static bool UltravisionSeeThroughWalls = false;
        
        public override void OnApplicationStart()
        {
            base.OnApplicationStart();
            MelonLogger.Log("Better Ultravision Enabled");
            
            Directory.CreateDirectory($"{Dir}");
            if (File.Exists(Config))
            {
                MelonLogger.Log("Reading config file");
                using (StreamReader sr = File.OpenText(Config))
                {
                    string s = "";
                    while ((s = sr.ReadLine()) != null)
                    {
                        if (s.Contains("UltravisionRangeBonus"))
                        {
                            UltravisionRangeBonus = int.Parse(s.Substring(s.IndexOf('=') + 1));
                        } else if (s.Contains("UltravisionCost"))
                        {
                            UltravisionCost = int.Parse(s.Substring(s.IndexOf('=') + 1));
                        } else if (s.Contains("UltravisionSeeThroughWalls"))
                        {
                            UltravisionSeeThroughWalls = bool.Parse(s.Substring(s.IndexOf('=') + 1));
                        }
                    }
                }
            }
            else
            {
                MelonLogger.Log("Creating config file");
                using (StreamWriter sw = File.CreateText(Config))
                {
                    sw.WriteLine("UltravisionRangeBonus=" + UltravisionRangeBonus);
                    sw.WriteLine("UltravisionCost=" + UltravisionCost);
                    sw.WriteLine("UltravisionSeeThroughWalls=" + UltravisionSeeThroughWalls);
                }
            }
        }

        [HarmonyPatch(typeof(Game), "GetVersionString")]
        public class GamePatch
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                Game.instance.model.GetUpgrade("Ultravision").cost = UltravisionCost;
                
                foreach (var towerModel in Game.instance.model.towers)
                {
                    if (towerModel.appliedUpgrades.Contains("Ultravision"))
                    {
                        towerModel.range += UltravisionRangeBonus - 3;
                        if (UltravisionSeeThroughWalls)
                        {
                            towerModel.ignoreBlockers = true;
                            foreach (var towerModelBehavior in towerModel.behaviors)
                            {
                                if (towerModelBehavior.name.Contains("AttackModel"))
                                {
                                    var attackModel = towerModelBehavior.Cast<AttackModel>();
                                    attackModel.attackThroughWalls = true;
                                    foreach (var weaponModel in attackModel.weapons)
                                    {
                                        if (weaponModel.projectile != null)
                                        {
                                            weaponModel.projectile.ignoreBlockers = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}