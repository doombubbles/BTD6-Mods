using System.IO;
using Assets.Scripts.Models;
using Assets.Scripts.Models.Towers.Behaviors.Attack;
using Assets.Scripts.Models.Towers.Mods;
using Assets.Scripts.Unity;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api.InGame_Mod_Options;
using Harmony;
using Il2CppSystem.Collections.Generic;
using MelonLoader;

[assembly: MelonInfo(typeof(BetterUltraVision.Main), "Better UltraVision", "1.1.1", "doombubbles")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]
namespace BetterUltraVision
{
    public class Main : BloonsTD6Mod
    {
        public override string MelonInfoCsURL =>
            "https://raw.githubusercontent.com/doombubbles/BTD6-Mods/main/BetterUltraVision/Main.cs";

        public override string LatestURL =>
            "https://github.com/doombubbles/BTD6-Mods/blob/main/BetterUltraVision/BetterUltraVision.dll?raw=true";
        
        
        private static readonly ModSettingInt UltravisionRangeBonus = new ModSettingInt(6)
        {
            displayName = "Ultravision Range Bonus",
            minValue = 0
        };
        
        private static readonly ModSettingInt UltravisionCost = new ModSettingInt(1200)
        {
            displayName = "Ultravision Cost",
            minValue = 0
        };

        public override void OnNewGameModel(GameModel result, List<ModModel> mods)
        {
            Game.instance.model.GetUpgrade("Ultravision").cost = CostForDifficulty(UltravisionCost, mods);
            
            foreach (var towerModel in Game.instance.model.towers)
            {
                if (towerModel.appliedUpgrades.Contains("Ultravision"))
                {
                    towerModel.range += UltravisionRangeBonus - 3;
                }
            }
        }
    }
}