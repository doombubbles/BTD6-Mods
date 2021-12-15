using System.Linq;
using Assets.Scripts.Models;
using Assets.Scripts.Models.Towers.Mods;
using Assets.Scripts.Models.TowerSets.Mods;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using PowersInShop.Towers;
using Main = PowersInShop.Main;

[assembly: MelonInfo(typeof(Main), "Powers In Shop", "2.0.0", "doombubbles")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace PowersInShop
{
    public class Main : BloonsTD6Mod    
    {
        private static readonly ModSettingBool AllowInChimps = false;
        public static readonly ModSettingBool AllowInRestrictedModes = true;
        
        public static readonly ModSettingInt BananaFarmerCost = 500;
        public static readonly ModSettingInt TechBotCost = 500;
        public static readonly ModSettingInt PontoonCost = 750;
        public static readonly ModSettingInt PortableLakeCost = 750;
        public static readonly ModSettingInt EnergisingTotemCost = 1000;
        public static readonly ModSettingInt RoadSpikesCost = 50;
        public static readonly ModSettingInt GlueTrapCost = 100;
        public static readonly ModSettingInt CamoTrapCost = 100;
        public static readonly ModSettingInt MoabMineCost = 500;

        public static readonly ModSettingInt RoadSpikesPierce = 20;
        public static readonly ModSettingInt GlueTrapPierce = 300;
        public static readonly ModSettingInt MoabMinePierce = 1;
        public static readonly ModSettingInt CamoTrapPierce = 500;

        public static readonly ModSettingInt TotemRechargeCost = 500;
        public static readonly ModSettingDouble TotemAttackSpeed = .15;

        public override string MelonInfoCsURL =>
            "https://raw.githubusercontent.com/doombubbles/BTD6-Mods/main/PowersInShop/Main.cs";

        public override string LatestURL =>
            "https://github.com/doombubbles/BTD6-Mods/blob/main/PowersInShop/PowersInShop.dll?raw=true";

        public override void OnGameObjectsReset()
        {
            EnergisingTotem.TSMThemeEnergisingTotem_Selected.lastOpened = false; //UI is reset, so we have to as well
        }

        [HarmonyPatch(typeof(GameModel), nameof(GameModel.CreateModded), typeof(GameModel), typeof(List<ModModel>))]
        internal class GameModel_CreateModded
        {
            [HarmonyPrefix]
            internal static bool Prefix(List<ModModel> mods)
            {
                var chimps = mods.FirstOrDefault(model => model.name == "Clicks");
                if (chimps != null)
                {
                    var chimpsMutators = chimps.mutatorMods.ToList();
                    var existingLocks = ModContent.GetInstances<ModPowerTower>()
                        .ToDictionary(tower => tower.Id, tower => chimpsMutators.OfType<LockTowerModModel>()
                                .FirstOrDefault(model => model.towerToLock != tower.Id));
                    
                    if (AllowInChimps)
                    {
                        foreach (var lockTowerModel in existingLocks.Values.Where(lockTowerModel =>
                                     lockTowerModel != null))
                        {
                            chimpsMutators.Remove(lockTowerModel);
                        }
                    }
                    else
                    {
                        foreach (var (id, lockTowerModel) in existingLocks)
                        {
                            if (lockTowerModel == null)
                            {
                                chimpsMutators.Add(new LockTowerModModel("Clicks", id));
                            }
                        }
                    }

                    chimps.mutatorMods = chimpsMutators.ToIl2CppReferenceArray();
                }

                return true;
            }
        }
    }
}