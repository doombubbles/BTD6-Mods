using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Models.Towers;
using Assets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors;
using Assets.Scripts.Simulation.Objects;
using Assets.Scripts.Simulation.SMath;
using Assets.Scripts.Simulation.Towers;
using Assets.Scripts.Unity;
using Assets.Scripts.Unity.Bridge;
using Assets.Scripts.Unity.UI_New.InGame;
using Assets.Scripts.Utils;
using BloonsTD6_Mod_Helper.Extensions;
using Harmony;
using Vector3 = UnityEngine.Vector3;

namespace AbilityChoice
{
    public class Overclock
    {
        private static Vector3 lastCursorPosUnity = Vector3.zero;
        private static HashSet<TowerToSimulation> towers = new HashSet<TowerToSimulation>();

        private static int ultraBoostTimer;

        public static Dictionary<Tower, int> UltraBoostFixes = new Dictionary<Tower, int>();
        
        public static BehaviorMutator GetMutator(TowerModel engineer, int tier)
        {
            var model = engineer.GetAbilites()[0].GetBehavior<OverclockModel>().Duplicate();
            model.rateModifier = .45f / (.45f + 2f / 3f * (1.05f - .15f * tier));
            return new OverclockModel.OverclockMutator(model);
        }

        public static void AddBoost(Tower from, Tower to)
        {
            if (Main.CurrentBoostIDs.ContainsKey(from.Id))
            {
                RemoveBoostOn(Main.CurrentBoostIDs[from.Id]);
            }
            
            to.RemoveMutatorsById("Overclock");
            to.AddMutator(GetMutator(from.towerModel, to.towerModel.tier), -1, false);

            Main.CurrentBoostIDs[from.Id] = to.Id;
        }

        public static void UltraBoostStack(Tower to, int stack = 1)
        {
            var model = Game.instance.model.GetTower(TowerType.EngineerMonkey, 0 , 5).GetAbilites()[0].GetBehavior<OverclockPermanentModel>().Duplicate();
            var mutator = to.GetMutatorById("Ultraboost");
            if (mutator != null)
            {
                var stacks = mutator.mutator.Cast<OverclockPermanentModel.OverclockPermanentMutator>().stacks;
                if (stacks < model.maxStacks)
                {
                    var newMutator = model.MutatorByStack(Math.Min(stacks + stack, model.maxStacks));
                    to.RemoveMutatorsById("Ultraboost");
                    to.AddMutator(newMutator, -1, false);
                }
            }
            else
            {
                var newMutator = model.MutatorByStack(Math.Min(model.maxStacks, stack));
                to.AddMutator(newMutator, -1, false);
            }
        }

        public static void RemoveBoostOn(int id)
        {
            var otherTower = InGame.instance.GetTowerManager()
                .GetTowerById(id);
            otherTower?.RemoveMutatorsById("Overclock");
        }
        
        [HarmonyPatch(typeof(OverclockInput), nameof(OverclockInput.Update))]
        internal class OverclockInput_Update
        {
            [HarmonyPostfix]
            internal static void Postfix(OverclockInput __instance, Vector3 cursorPosUnityWorld)
            {
                lastCursorPosUnity = cursorPosUnityWorld;
                if (__instance.targetImages != null && Main.CurrentTowerIDs.Contains(__instance.tower.Id))
                {
                    foreach (var tts in __instance.targetImages.Keys)
                    {
                        towers.Add(tts);
                    }
                }
            }
        }
        
        [HarmonyPatch(typeof(OverclockInput), nameof(OverclockInput.ExitInputMode))]
        internal class OverclockInput_ExitInputMode
        {
            [HarmonyPostfix]
            internal static void Postfix(OverclockInput __instance)
            {
                if (__instance.ability.CooldownRemaining != 0 && Main.CurrentTowerIDs.Contains(__instance.tower.Id))
                {
                    var tower = towers.OrderBy(tts => tts.position.ToSMathVector().Distance(lastCursorPosUnity.ToSMathVector())).First().tower;

                    var engi = __instance.tower.tower;
                    AddBoost(engi, tower);

                    if (engi.towerModel.tier == 5)
                    {
                        ultraBoostTimer = 0;
                    }
                }
                
                towers = new HashSet<TowerToSimulation>();
            }
        }
        
        [HarmonyPatch(typeof(UnityToSimulation), nameof(UnityToSimulation.GetAllAbilities))]
        internal class UnityToSimulation_GetAllAbilities
        {
            [HarmonyPostfix]
            internal static void Postfix(ref Il2CppSystem.Collections.Generic.List<AbilityToSimulation> __result)
            {
                __result = __result.Where(a2s =>
                    !(Main.CurrentTowerIDs.Contains(a2s.Tower.Id) && a2s.model.displayName == "Overclock" || a2s.model.displayName == "Ultraboost"));
            }
        }
        
        [HarmonyPatch(typeof(InGame), nameof(InGame.SellTower))]
        internal class InGame_SellTower
        {
            [HarmonyPrefix]
            internal static void Prefix(TowerToSimulation tower)
            {
                if (tower.tower != null && Main.CurrentBoostIDs.ContainsKey(tower.tower.Id))
                {
                    RemoveBoostOn(Main.CurrentBoostIDs[tower.tower.Id]);
                    Main.CurrentBoostIDs.Remove(tower.tower.Id);
                }
            }
        }
        
        [HarmonyPatch(typeof(InGame), nameof(InGame.TowerUpgraded))]
        internal class InGame_TowerUpgraded
        {
            [HarmonyPostfix]
            internal static void Postfix(TowerToSimulation tower)
            {
                if (tower.tower != null)
                {
                    foreach (var boostingKey in Main.CurrentBoostIDs.Keys)
                    {
                        if (Main.CurrentBoostIDs[boostingKey] == tower.Id)
                        {
                            RemoveBoostOn(tower.Id);
                            var engi = InGame.instance.GetTowerManager().GetTowerById(boostingKey);
                            AddBoost(engi, tower.tower);
                            break;
                        }
                    }
                }
            }
        }
        
        [HarmonyPatch(typeof(InGame), nameof(InGame.Update))]
        internal class InGame_Update
        {
            [HarmonyPostfix]
            internal static void Postfix()
            {
                foreach (var tower in UltraBoostFixes.Keys)
                {
                    int stacks = UltraBoostFixes[tower];
                    tower.RemoveMutatorsById("Ultraboost");
                    UltraBoostStack(tower, stacks);
                }
                UltraBoostFixes = new Dictionary<Tower, int>();

                if (!TimeManager.inBetweenRounds)
                {
                    if (TimeManager.fastForwardActive)
                    {
                        ultraBoostTimer += TimeManager.compromisedFastFowardRate;
                    }
                    else
                    {
                        ultraBoostTimer += 1;
                    }
                }
                
                if (ultraBoostTimer >= 45 * 60)
                {
                    foreach (var boostingKey in Main.CurrentBoostIDs.Keys)
                    {
                        var engi = InGame.instance.GetTowerManager().GetTowerById(boostingKey);
                        if (engi.towerModel.tier == 5)
                        {
                            var tower = InGame.instance.GetTowerManager().GetTowerById(Main.CurrentBoostIDs[boostingKey]);
                            if (tower != null)
                            {
                                UltraBoostStack(tower);
                            }
                        }
                    }
                    ultraBoostTimer = 0;
                }
            }
        }
    }
}