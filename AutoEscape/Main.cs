using System.IO;
using Assets.Scripts.Simulation.Bloons;
using Assets.Scripts.Unity.UI_New.InGame;
using Assets.Scripts.Utils;
using Harmony;
using MelonLoader;
using NKHook6.Api.Extensions;

[assembly: MelonInfo(typeof(AutoEscape.Main), "AutoEscape", "1.0.0", "doombubbles")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]
namespace AutoEscape
{
    public class Main : MelonMod
    {
        [HarmonyPatch(typeof(Bloon), nameof(Bloon.Leaked))]
        internal class Bloon_Leaked
        {
            [HarmonyPrefix]
            internal static bool Prefix(Bloon __instance)
            {
                if (__instance.GetModifiedTotalLeakDamage() >= InGame.instance.getHealth() + InGame.Bridge.simulation.Shield)
                {
                    if (!InGame.instance.quitting)
                    {
                        InGame.instance.Quit();
                        MelonLogger.Log("You're Welcome.");
                    }
                    return false;
                }
                return true;
            }
        }
        
    }
}