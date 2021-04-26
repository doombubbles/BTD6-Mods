using System;
using System.Linq;
using Assets.Scripts.Models.Audio;
using Assets.Scripts.Models.GenericBehaviors;
using Assets.Scripts.Models.Powers;
using Assets.Scripts.Models.Powers.Effects;
using Assets.Scripts.Models.Towers;
using Assets.Scripts.Models.Towers.Behaviors;
using Assets.Scripts.Models.Towers.Projectiles;
using Assets.Scripts.Unity;
using Assets.Scripts.Unity.UI_New.InGame;
using MelonLoader;
using UnhollowerBaseLib;
using UnityEngine;


namespace PowersInShop
{
    public class Utils
    {
        public static bool CanBeCast<T>(Il2CppObjectBase obj) where T : Il2CppObjectBase
        {
            IntPtr nativeClassPtr = Il2CppClassPointerStore<T>.NativeClassPtr;
            IntPtr num = IL2CPP.il2cpp_object_get_class(obj.Pointer);
            return IL2CPP.il2cpp_class_is_assignable_from(nativeClassPtr, num);
        }

        public static ProjectileModel GetProjectileModel(PowerBehaviorModel powerBehaviorModel)
        {
            ProjectileModel projectleModel = null;

            if (CanBeCast<MoabMineModel>(powerBehaviorModel))
            {
                projectleModel = powerBehaviorModel.Cast<MoabMineModel>().projectileModel;
            } else if (CanBeCast<GlueTrapModel>(powerBehaviorModel))
            {
                projectleModel = powerBehaviorModel.Cast<GlueTrapModel>().projectileModel;
            } else if (CanBeCast<CamoTrapModel>(powerBehaviorModel))
            {
                projectleModel = powerBehaviorModel.Cast<CamoTrapModel>().projectileModel;
            } else if (CanBeCast<RoadSpikesModel>(powerBehaviorModel))
            {
                projectleModel = powerBehaviorModel.Cast<RoadSpikesModel>().projectileModel;
            }

            return projectleModel;
        }

        public static TowerModel CreateTower(string power)
        {
            PowerModel powerModel = Game.instance.model.GetPowerWithName(power);
            TowerModel towerModel =
                Game.instance.model.GetTowerWithName("NaturesWardTotem").Clone().Cast<TowerModel>();
            towerModel.name = power;
            towerModel.icon = powerModel.icon;
            towerModel.cost = Main.Powers[power];
            towerModel.display = power;
            towerModel.baseId = power;
            towerModel.towerSet = "Support";
            towerModel.radiusSquared = 9;
            towerModel.radius = 3;
            towerModel.range = 0;
            towerModel.footprint = new CircleFootprintModel(power, 0, true, false, true);
            towerModel.showPowerTowerBuffs = false;        
            
            towerModel.behaviors.First(b => b.name.Contains("OnExpire"))
                .Cast<CreateEffectOnExpireModel>().effectModel = powerModel.behaviors
                .First(b => b.name.Contains("Effect")).Cast<CreateEffectOnPowerModel>().effectModel;
                        
                
            towerModel.behaviors.First(b => b.name.Contains("Sound")).Cast<CreateSoundOnTowerPlaceModel>()
                .sound1.assetId = powerModel.behaviors.First(b => b.name.Contains("Sound")).Cast<CreateSoundOnPowerModel>().sound.assetId;
            towerModel.behaviors.First(b => b.name.Contains("Sound")).Cast<CreateSoundOnTowerPlaceModel>()
                .sound2.assetId = powerModel.behaviors.First(b => b.name.Contains("Sound")).Cast<CreateSoundOnPowerModel>().sound.assetId;
                
            towerModel.behaviors.First(b => b.name.Contains("Sound")).Cast<CreateSoundOnTowerPlaceModel>()
                .heroSound1 = new BlankSoundModel();
            towerModel.behaviors.First(b => b.name.Contains("Sound")).Cast<CreateSoundOnTowerPlaceModel>()
                .heroSound2 = new BlankSoundModel();

            var displayModel = towerModel.behaviors.First(b => b.name.Contains("Display"))
                .Cast<DisplayModel>();    
            displayModel.display = "8ab0e3fbb093a554d84a85e18fe2acac"; //tiny little caltrops
                    
            var powerBehaviorModel = powerModel.behaviors.First(b => !b.name.Contains("Create"));
            var projectleModel = GetProjectileModel(powerBehaviorModel);
            if (projectleModel != null)
            {
                projectleModel.pierce = Main.TrackPowers[power];
                if (projectleModel.maxPierce != 0)
                {
                    projectleModel.maxPierce = Main.TrackPowers[power];
                }
            }

            //towerModel.behaviors = towerModel.behaviors.Where(b => !b.name.Contains("Display")).ToArray();

            //towerModel.behaviors.First(b => b.name.Contains("Display")).Cast<DisplayModel>().display = powerModel.icon.GUID;

            towerModel.behaviors = towerModel.behaviors.Where(b => !b.name.Contains("Slow")).ToArray();

            return towerModel;
        }
        
        public static void RecursivelyLogGameObject(Transform gameObject, int depth = 0)
        {
            var str = gameObject.name;
            for (int i = 0; i < depth; i++)
            {
                str = "|  " + str;
            }
            MelonLogger.Msg(str);
            for (int i = 0; i < gameObject.childCount; i++)
            {
                RecursivelyLogGameObject(gameObject.transform.GetChild(i), depth + 1);
            }
        }
    }
}