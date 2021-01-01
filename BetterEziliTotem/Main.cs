using System.IO;
using System.Linq;
using Assets.Scripts.Models.Towers.Behaviors.Abilities;
using Assets.Scripts.Unity;
using Harmony;
using MelonLoader;

[assembly: MelonInfo(typeof(BetterEziliTotem.Main), "Better Ezili Totem", "1.0.0", "doombubbles")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]
namespace BetterEziliTotem
{
    public class Main : MelonMod
    {
        private static readonly string Dir = $"{Directory.GetCurrentDirectory()}\\Mods\\BetterEziliTotem";
        private static readonly string Config = $"{Dir}\\config.txt";

        public static int AbilityCooldown = 5399;

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();
            MelonLogger.Log("Better Ezili Totem Enabled");

            Directory.CreateDirectory($"{Dir}");
            if (File.Exists(Config))
            {
                MelonLogger.Log("Reading config file");
                using (StreamReader sr = File.OpenText(Config))
                {
                    string s = "";
                    while ((s = sr.ReadLine()) != null)
                    {
                        if (s.Contains("AbilityCooldown"))
                        {
                            AbilityCooldown = int.Parse(s.Substring(s.IndexOf('=') + 1));
                        }
                    }
                }
            }
            else
            {
                MelonLogger.Log("Creating config file");
                using (StreamWriter sw = File.CreateText(Config))
                {
                    sw.WriteLine("AbilityCooldown=" + AbilityCooldown);
                }
            }
        }
        
        
        [HarmonyPatch(typeof(Game), "GetVersionString")]
        public class GamePatch
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                for (int i = 7; i <= 20; i++)
                {
                    var towerModel = Game.instance.model.GetTowerFromId("Ezili " + i);
                    var ability = towerModel.behaviors.First(b => b.name.Contains("Totem")).Cast<AbilityModel>();

                    ability.livesCost = 0;
                    ability.cooldownFrames = AbilityCooldown;
                }
            }
        }
    }

}