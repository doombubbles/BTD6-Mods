using Assets.Scripts.Models.Bloons.Behaviors;
using Assets.Scripts.Simulation.Bloons;
using Assets.Scripts.Unity;
using Assets.Scripts.Unity.UI_New.InGame;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Extensions;
using MelonLoader;

[assembly: MelonInfo(typeof(AutoEscape.Main), "AutoEscape", "1.0.3", "doombubbles")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]
namespace AutoEscape
{
    public class Main : BloonsTD6Mod
    {
        public override string MelonInfoCsURL =>
            "https://raw.githubusercontent.com/doombubbles/BTD6-Mods/main/AutoEscape/Main.cs";

        public override string LatestURL =>
            "https://github.com/doombubbles/BTD6-Mods/blob/main/AutoEscape/AutoEscape.dll?raw=true";
        
        public override bool PreBloonLeaked(Bloon bloon)
        {
            if (bloon.GetModifiedTotalLeakDamage() >= InGame.instance.bridge.GetHealth() + InGame.Bridge.simulation.Shield
                && !InGame.instance.IsSandbox && !bloon.bloonModel.HasBehavior<GoldenBloonModel>())
            {
                if (!InGame.instance.quitting)
                {
                    InGame.instance.Quit();
                    MelonLogger.Msg("You're Welcome.");
                }
                return false;
            }
            return true;
        }
        
    }
}