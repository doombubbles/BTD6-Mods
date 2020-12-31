using System.IO;
using System.Linq;
using System.Net.Mime;
using Assets.Scripts.Models;
using Assets.Scripts.Models.Towers;
using Assets.Scripts.Models.Towers.Behaviors.Abilities;
using Assets.Scripts.Models.Towers.Behaviors.Attack;
using Assets.Scripts.Models.Towers.Projectiles;
using Assets.Scripts.Models.Towers.Projectiles.Behaviors;
using Assets.Scripts.Models.Towers.Upgrades;
using Assets.Scripts.Models.Towers.Weapons;
using Assets.Scripts.Models.TowerSets;
using Assets.Scripts.Simulation.Objects;
using Assets.Scripts.Simulation.Towers;
using Assets.Scripts.Unity;
using Assets.Scripts.Unity.Localization;
using Assets.Scripts.Utils;
using Harmony;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using NKHook6.Api.Events;
using NKHook6.Api.Events._Towers;
using UnhollowerBaseLib;

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
                MelonLogger.Log("Done reading");
            }
            else
            {
                MelonLogger.Log("Creating config file");
                using (StreamWriter sw = File.CreateText(Config))
                {
                    sw.WriteLine("ArmorPiercingDartsCost=3000");
                    sw.WriteLine("HeatTippedDartsBonus=1");
                }
                MelonLogger.Log("Done Creating");
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