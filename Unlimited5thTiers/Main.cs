using System.Linq;
using Assets.Scripts.Models.Towers;
using Assets.Scripts.Simulation.Towers;
using Assets.Scripts.Simulation.Towers.Behaviors;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using MelonLoader;

[assembly: MelonInfo(typeof(Unlimited5thTiers.Main), "Unlimited 5th Tiers +", "1.0.0", "doombubbles")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace Unlimited5thTiers
{
    public class Main : BloonsTD6Mod
    {
        private static readonly ModSettingBool AllowUnlimited5thTiers = true;
        private static readonly ModSettingBool AllowUnlimitedParagons = true;
        private static readonly ModSettingBool AllowUnlimitedVTSGs = true;

        public override string MelonInfoCsURL =>
            "https://raw.githubusercontent.com/doombubbles/BTD6-Mods/main/Unlimited5thTiers/Main.cs";

        public override string LatestURL =>
            "https://github.com/doombubbles/BTD6-Mods/blob/main/Unlimited5thTiers/Unlimited5thTiers.dll?raw=true";


        [HarmonyPatch(typeof(TowerManager), nameof(TowerManager.IsTowerPathTierLocked))]
        internal class TowerManager_IsTowerPathTierLocked
        {
            [HarmonyPostfix]
            internal static void Postfix(TowerManager __instance, ref bool __result)
            {
                if (AllowUnlimited5thTiers)
                {
                    __result = false;
                }
            }
        }

        [HarmonyPatch(typeof(MonkeyTemple), nameof(MonkeyTemple.CheckTCBOO))]
        internal class MonkeyTemple_CheckTCBOO
        {
            [HarmonyPrefix]
            internal static bool Prefix(MonkeyTemple __instance)
            {
                if (AllowUnlimitedVTSGs && __instance.checkTCBOO &&
                    __instance.monkeyTempleModel.weaponDelayFrames + __instance.lastSacrificed <=
                    __instance.Sim.time.elapsed && __instance.monkeyTempleModel.checkForThereCanOnlyBeOne
                    && __instance.lastSacrificed != __instance.Sim.time.elapsed)
                {
                    var superMonkeys = __instance.Sim.towerManager.GetTowersByBaseId(TowerType.SuperMonkey).ToList();
                    var robocop = superMonkeys.FirstOrDefault(tower => tower.towerModel.tiers[1] == 5);
                    var batman = superMonkeys.FirstOrDefault(tower => tower.towerModel.tiers[2] == 5);
                    if (batman != default && robocop != default)
                    {
                        __instance.SacrificeBatmanAndRoboCop(robocop, batman);
                    }

                    __instance.checkTCBOO = false;
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(MonkeyTemple), nameof(MonkeyTemple.StartSacrifice))]
        public class MonkeyTemple_StartSacrifice
        {
            [HarmonyPostfix]
            public static void Postfix(MonkeyTemple __instance)
            {
                if (__instance.monkeyTempleModel.checkForThereCanOnlyBeOne && !__instance.checkTCBOO)
                {
                    __instance.checkTCBOO = true;
                }
            }
        }


        [HarmonyPatch(typeof(Tower), nameof(Tower.CanUpgradeToParagon))]
        internal class Tower_CanUpgradeToParagon
        {
            [HarmonyPostfix]
            internal static void Postfix(Tower __instance, ref bool __result)
            {
                if (__instance.Sim.towerManager.IsParagonLocked(__instance, __instance.owner) ||
                    __instance.towerModel.paragonUpgrade == null || !AllowUnlimitedParagons)
                {
                    return;
                }

                var towers = __instance.Sim.towerManager.GetTowersByBaseId(__instance.towerModel.baseId).ToList();
                for (var i = 0; i < 3; i++)
                {
                    if (towers.All(tower => tower.towerModel.tiers[i] != 5))
                    {
                        return;
                    }
                }

                __result = true;
            }
        }
    }
}