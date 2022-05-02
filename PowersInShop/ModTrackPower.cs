using System.Linq;
using Assets.Scripts.Models.Audio;
using Assets.Scripts.Models.GenericBehaviors;
using Assets.Scripts.Models.Map;
using Assets.Scripts.Models.Powers;
using Assets.Scripts.Models.Powers.Effects;
using Assets.Scripts.Models.Towers;
using Assets.Scripts.Models.Towers.Behaviors;
using Assets.Scripts.Models.Towers.Projectiles;
using Assets.Scripts.Models.Towers.Projectiles.Behaviors;
using Assets.Scripts.Simulation.Towers;
using Assets.Scripts.Simulation.Track;
using Assets.Scripts.Unity;
using Assets.Scripts.Unity.UI_New;
using Assets.Scripts.Unity.UI_New.InGame;
using Assets.Scripts.Unity.UI_New.InGame.RightMenu;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using CreateEffectOnExpireModel = Assets.Scripts.Models.Towers.Behaviors.CreateEffectOnExpireModel;
using Vector2 = Assets.Scripts.Simulation.SMath.Vector2;

namespace PowersInShop
{
    public abstract class ModTrackPower : ModPowerTower
    {
        protected abstract int Pierce { get; }

        public override string BaseTower => TowerType.NaturesWardTotem;
        protected abstract ProjectileModel GetProjectile(PowerModel powerModel);

        public override void ModifyBaseTowerModel(TowerModel towerModel)
        {
            var powerModel = Game.instance.model.GetPowerWithName(Name);

            towerModel.footprint = new CircleFootprintModel("CircleFootPrintModel_", 0, true, false, true);
            towerModel.radiusSquared = 9;
            towerModel.radius = 3;
            towerModel.range = 0;
            towerModel.showPowerTowerBuffs = false;

            towerModel.GetBehavior<CreateEffectOnExpireModel>().effectModel =
                powerModel.GetBehavior<CreateEffectOnPowerModel>().effectModel;

            var assetId = powerModel.GetBehavior<CreateSoundOnPowerModel>().sound.assetId;
            var createSoundOnTowerPlaceModel = towerModel.GetBehavior<CreateSoundOnTowerPlaceModel>();
            createSoundOnTowerPlaceModel.sound1.assetId = assetId;
            createSoundOnTowerPlaceModel.sound2.assetId = assetId;
            createSoundOnTowerPlaceModel.heroSound1 = SoundModel.blank;
            createSoundOnTowerPlaceModel.heroSound2 = SoundModel.blank;

            //tiny little caltrops
            towerModel.display = towerModel.GetBehavior<DisplayModel>().display = "8ab0e3fbb093a554d84a85e18fe2acac";

            var projectileModel = GetProjectile(powerModel).Duplicate();
            projectileModel.pierce = Pierce;
            if (projectileModel.maxPierce != 0)
            {
                projectileModel.maxPierce = Pierce;
            }

            towerModel.RemoveBehaviors<SlowModel>();
        }


        [HarmonyPatch(typeof(InputManager), nameof(InputManager.EnterPlacementMode))]
        internal class InputManager_EnterPlacementMode
        {
            [HarmonyPostfix]
            internal static void Postfix(TowerModel forTowerModel)
            {
                if (forTowerModel.GetModTower() is ModTrackPower trackPower)
                {
                    var image = ShopMenu.instance
                        .GetTowerButtonFromBaseId(trackPower.Id)
                        .gameObject.transform
                        .Find("Icon").GetComponent<Image>();
                    InGameObjects.instance.PowerIconStart(image.sprite);
                }
            }
        }

        [HarmonyPatch(typeof(InputManager), nameof(InputManager.Update))]
        internal class InputManager_Update
        {
            [HarmonyPostfix]
            internal static void Postfix()
            {
                var inputManager = InGame.instance.inputManager;
                var towerModel = inputManager.towerModel;
                if (towerModel != null && inputManager.inPlacementMode && towerModel.GetModTower() is ModTrackPower)
                {
                    var map = InGame.instance.UnityToSimulation.simulation.Map;
                    InGameObjects.instance.PowerIconUpdate(inputManager.GetCursorPosition(),
                        map.CanPlace(new Vector2(inputManager.cursorPositionWorld), towerModel));
                }
            }
        }

        [HarmonyPatch(typeof(InputManager), nameof(InputManager.ExitPlacementMode))]
        internal class InputManager_ExitPlacementMode
        {
            [HarmonyPostfix]
            internal static void Postfix()
            {
                if (InGameObjects.instance.powerIcon != null)
                {
                    InGameObjects.instance.PowerIconEnd();
                }
            }
        }

        [HarmonyPatch(typeof(Map), nameof(Map.CanPlace))]
        internal class Map_CanPlace
        {
            [HarmonyPostfix]
            internal static void Patch(ref bool __result, Vector2 at, TowerModel tm)
            {
                if (tm.GetModTower() is ModTrackPower)
                {
                    var map = InGame.instance.UnityToSimulation.simulation.Map;
                    __result = map.GetAllAreasOfTypeThatTouchPoint(at).ToArray()
                        .Any(area => area.areaModel.type == AreaType.track);
                }
            }
        }


        [HarmonyPatch(typeof(Tower), nameof(Tower.Initialise))]
        public class Tower_Initialise
        {
            [HarmonyPostfix]
            public static void Postfix(Tower __instance)
            {
                if (__instance.towerModel?.GetModTower() is ModTrackPower trackPower)
                {
                    var powerBehaviorModel = Game.instance.model.GetPowerWithName(trackPower.Name)
                        .GetBehavior<PowerBehaviorModel>();

                    InGame.instance.UnityToSimulation.simulation.powerManager.GetInstance(powerBehaviorModel)
                        .Activate(__instance.Position.ToVector2(), powerBehaviorModel, 0);
                }
            }
        }

        [HarmonyPatch(typeof(Tower), nameof(Tower.OnDestroy))]
        public class Tower_OnDestroy
        {
            [HarmonyPrefix]
            public static void Prefix(Tower __instance)
            {
                if (__instance.towerModel?.GetModTower() is ModTrackPower trackPower &&
                    (!InGame.instance.IsCoop || __instance.owner == Game.instance.GetNkGI().PeerID) &&
                    (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                {
                    ShopMenu.instance.GetTowerButtonFromBaseId(trackPower.Id).ButtonActivated();
                }
            }
        }
    }
}