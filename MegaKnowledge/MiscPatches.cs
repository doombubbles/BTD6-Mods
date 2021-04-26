using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Scripts.Models.Towers;
using Assets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors;
using Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors;
using Assets.Scripts.Simulation.Towers;
using Assets.Scripts.Simulation.Towers.Behaviors;
using Assets.Scripts.Simulation.Towers.Behaviors.Attack;
using Assets.Scripts.Simulation.Towers.Behaviors.Attack.Behaviors;
using Assets.Scripts.Simulation.Towers.Projectiles;
using Assets.Scripts.Simulation.Towers.Projectiles.Behaviors;
using Assets.Scripts.Simulation.Towers.Weapons.Behaviors;
using Assets.Scripts.Unity;
using Assets.Scripts.Unity.JsonConverters;
using Assets.Scripts.Unity.UI_New.InGame;
using Assets.Scripts.Utils;
using BTD_Mod_Helper.Extensions;
using Harmony;
using MelonLoader;
using Newtonsoft.Json;
using UnhollowerBaseLib;
using static Assets.Scripts.Models.Towers.TowerType;

namespace MegaKnowledge
{
    public class MiscPatches
    {
        [HarmonyPatch(typeof(RotateToPointer), nameof(RotateToPointer.SetRotation))]
        internal class RotateToPointer_SetRotation
        {
            [HarmonyPrefix]
            internal static bool Prefix(RotateToPointer __instance)
            {
                if (__instance.dartlingMaintainLastPos != null && __instance.dartlingMaintainLastPos.tower != null)
                {
                    return __instance.dartlingMaintainLastPos.tower.targetType.intID == -1;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(LineEffect), nameof(LineEffect.UpdateEffect))]
        internal class LineEffect_UpdateEffect
        {
            [HarmonyPrefix]
            internal static bool Prefix(LineEffect __instance)
            {
                if (__instance.rotateToPointer?.dartlingMaintainLastPos?.tower != null)
                {
                    if (__instance.rotateToPointer.dartlingMaintainLastPos.tower.targetType.intID != -1
                        && __instance.rotateToPointer.attack.target.bloon != null &&
                        !__instance.lineEffectModel.isLineDisplayEndless)
                    {
                        __instance.targetMagnitude =
                            __instance.rotateToPointer.attack.target.bloon.Position.Distance(__instance.rotateToPointer
                                .dartlingMaintainLastPos.tower.Position) - 20;
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(InGame), nameof(InGame.Update))]
        internal class InGame_Update
        {
            [HarmonyPostfix]
            internal static void Postfix(InGame __instance)
            {
                if (__instance.bridge == null || !Main.MegaKnowledges[MegaKnowledge.Overtime].enabled) return;
                var inGame = __instance;
                var overclock = Game.instance.model.GetTower(EngineerMonkey, 0, 4).GetAbility()
                    .GetBehavior<OverclockModel>();
                foreach (var tts in inGame.bridge.GetAllTowers())
                {
                    var baseId = tts.tower.towerModel.baseId;
                    if ((baseId == EngineerMonkey || baseId.Contains("Sentry")) &&
                        tts.tower.GetMutatorById("Overclock") == null)
                    {
                        tts.tower.AddMutator(overclock.Mutator, -1, false);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(TrackTargetWithinTime), nameof(TrackTargetWithinTime.CalculateDirection))]
        internal class TrackTargetWithinTime_CalculateDirection
        {
            [HarmonyPrefix]
            internal static bool Prefix(TrackTargetWithinTime __instance)
            {
                if (__instance.trackTargetWithinTimeModel.name.Contains("Behind") && __instance.projectile.ExhaustFraction < .2)
                {
                    return false;
                }
                
                return true;
            }
        }

        private static readonly Dictionary<int, (ArriveAtTarget, float)> SpikeInfo = new Dictionary<int, (ArriveAtTarget, float)>();

        [HarmonyPatch(typeof(Projectile), nameof(Projectile.CollideBloon))]
        internal class Projectile_CollideBloon
        {
            [HarmonyPrefix]
            internal static void Prefix(Projectile __instance, ref float __state)
            {
                if (__instance.emittedBy?.towerModel.baseId == SpikeFactory)
                {
                    __state = __instance.pierce;
                }
                else
                {
                    __state = 0;
                }
            }


            [HarmonyPostfix]
            internal static void Postfix(Projectile __instance, ref float __state)
            {
                if (__state > 0 && Main.MegaKnowledges[MegaKnowledge.SpikeEmpowerment].enabled)
                {
                    if (!SpikeInfo.ContainsKey(__instance.Id))
                    {
                        var projectileBehavior = __instance.projectileBehaviors.list.FirstOrDefault(b => b.TryCast<ArriveAtTarget>() != null);
                        if (projectileBehavior != null)
                        {
                            SpikeInfo[__instance.Id] = (projectileBehavior.Cast<ArriveAtTarget>(), 0);
                        }
                        else
                        {
                            return;
                        }
                    }
                    var (arriveAtTarget, pierce) = SpikeInfo[__instance.Id];
                    var arriveAtTargetModel = arriveAtTarget.arriveModel;
                    if (!arriveAtTargetModel.filterCollisionWhileMoving && arriveAtTarget.GetPercThruMovement() < .99)
                    {
                        pierce += __state - __instance.pierce;
                        __instance.pierce = __state;
                        
                        if (Math.Abs(pierce - __state) < .00001)
                        {
                            arriveAtTargetModel = arriveAtTargetModel.Duplicate();
                            arriveAtTargetModel.filterCollisionWhileMoving = true;
                            arriveAtTarget.UpdatedModel(arriveAtTargetModel);
                        }
                    }

                    SpikeInfo[__instance.Id] = (arriveAtTarget, pierce);
                }
            }
        }

        [HarmonyPatch(typeof(Projectile), nameof(Projectile.Destroy))]
        internal class Projectile_Destroy
        {
            [HarmonyPostfix]
            internal static void Postfix(Projectile __instance)
            {
                if (SpikeInfo.ContainsKey(__instance.Id))
                {
                    SpikeInfo.Remove(__instance.Id);
                }
            }
        }









    }
}