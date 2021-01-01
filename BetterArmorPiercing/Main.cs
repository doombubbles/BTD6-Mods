using System.IO;
using System.Linq;
using Assets.Scripts.Models.Towers.Behaviors.Attack;
using Assets.Scripts.Models.Towers.Projectiles.Behaviors;
using Assets.Scripts.Unity;
using Harmony;
using MelonLoader;

[assembly: MelonInfo(typeof(BetterArmorPiercing.Main), "Better Armor Piercing", "1.1.1", "doombubbles")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]
namespace BetterArmorPiercing
{
    public class Main : MelonMod
    {
        private static readonly string Dir = $"{Directory.GetCurrentDirectory()}\\Mods\\BetterArmorPiercing";
        private static readonly string Config = $"{Dir}\\config.txt";

        private static int ArmorPiercingDartsCost = 3000;
        private static int HeatTippedDartsBonus = 1;

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();
            MelonLogger.Log("Better Armor Piercing Enabled");
            
            Directory.CreateDirectory($"{Dir}");
            if (File.Exists(Config))
            {
                MelonLogger.Log("Reading config file");
                using (StreamReader sr = File.OpenText(Config))
                {
                    string s = "";
                    while ((s = sr.ReadLine()) != null)
                    {
                        if (s.Contains("ArmorPiercingDartsCost"))
                        {
                            ArmorPiercingDartsCost = int.Parse(s.Substring(s.IndexOf('=') + 1));
                        } else if (s.Contains("HeatTippedDartsBonus"))
                        {
                            HeatTippedDartsBonus = int.Parse(s.Substring(s.IndexOf('=') + 1));
                        }
                    }
                }
            }
            else
            {
                MelonLogger.Log("Creating config file");
                using (StreamWriter sw = File.CreateText(Config))
                {
                    sw.WriteLine("ArmorPiercingDartsCost=" + ArmorPiercingDartsCost);
                    sw.WriteLine("HeatTippedDartsBonus=" + HeatTippedDartsBonus);
                }
            }
        }

        [HarmonyPatch(typeof(Game), "GetVersionString")]
        public class GamePatch
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                Game.instance.model.GetUpgrade("Armor Piercing Darts").cost = ArmorPiercingDartsCost;
                
                foreach (var towerModel in Game.instance.model.towers)
                {
                    if (!towerModel.appliedUpgrades.Contains("Armor Piercing Darts")) continue;
                    
                    var attackModel = towerModel.behaviors.First(b=>b.name.Contains("AttackModel")).Cast<AttackModel>();
                    
                    var projectile = attackModel.weapons[0].projectile;
                    DamageModel damage = projectile.behaviors.First(b=>b.name.Contains("Damage")).Cast<DamageModel>();
                    damage.damageTypes = new[] { "Normal" };
                    if (towerModel.appliedUpgrades.Contains("Heat-tipped Darts"))
                    {
                        damage.damage += HeatTippedDartsBonus;
                    }
                    
                }
            }
        }
    }
}