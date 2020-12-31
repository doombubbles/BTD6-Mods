using System;
using System.IO;
using System.Linq;
using Assets.Main.Scenes;
using Assets.Scripts.Models;
using Assets.Scripts.Models.Audio;
using Assets.Scripts.Models.GenericBehaviors;
using Assets.Scripts.Models.Map;
using Assets.Scripts.Models.Powers;
using Assets.Scripts.Models.Powers.Effects;
using Assets.Scripts.Models.Profile;
using Assets.Scripts.Models.Towers;
using Assets.Scripts.Models.Towers.Behaviors;
using Assets.Scripts.Models.Towers.Projectiles;
using Assets.Scripts.Models.TowerSets;
using Assets.Scripts.Simulation.Input;
using Assets.Scripts.Simulation.Objects;
using Assets.Scripts.Simulation.Towers;
using Assets.Scripts.Simulation.Towers.Projectiles;
using Assets.Scripts.Unity;
using Assets.Scripts.Unity.UI_New;
using Assets.Scripts.Unity.UI_New.InGame;
using Assets.Scripts.Unity.UI_New.Upgrade;
using Harmony;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using UnhollowerBaseLib;
using UnityEngine;
using UnityEngine.UI;
using InputManager = Assets.Scripts.Unity.UI_New.InGame.InputManager;
using Vector2 = Assets.Scripts.Simulation.SMath.Vector2;

[assembly: MelonInfo(typeof(PowersInShop.Main), "Powers In Shop", "1.0.0", "doombubbles")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]
namespace PowersInShop
{
    public class Main : MelonMod
    {
        static readonly string Dir = $"{Directory.GetCurrentDirectory()}\\Mods\\PowersInShop";
        static readonly string Config = $"{Dir}\\config.txt";

        static Dictionary<string, int> Powers = new Dictionary<string, int>();
        static Dictionary<string, int> TrackPowers = new Dictionary<string, int>();

        static bool AllowInChimps = false;
        static bool RestrictAsSupport = true;

        public override void OnApplicationStart()
        {
            Powers.Add("BananaFarmer", 500); 
            Powers.Add("TechBot", 500);
            Powers.Add("Pontoon", 1000);
            Powers.Add("PortableLake", 1000);
            Powers.Add("RoadSpikes", 50);
            TrackPowers.Add("RoadSpikes", 20);
            Powers.Add("GlueTrap", 100);
            TrackPowers.Add("GlueTrap", 300);
            Powers.Add("CamoTrap", 100);
            TrackPowers.Add("CamoTrap", 500);
            Powers.Add("MoabMine", 500);
            TrackPowers.Add("MoabMine", 10);


            MelonLogger.Log("Powers In Shop mod loaded");
            Directory.CreateDirectory($"{Dir}");
            if (File.Exists(Config))
            {
                MelonLogger.Log("Reading config file");
                using (StreamReader sr = File.OpenText(Config))
                {
                    string s = "";
                    while ((s = sr.ReadLine()) != null)
                    {
                        if (s.StartsWith("#"))
                        {
                            continue;
                        }
                        
                        if (s.Contains("Cost"))
                        {
                            var index = s.IndexOf('=');
                            var name = s.Substring(0, index).Replace("Cost", "");
                            var cost = int.Parse(s.Substring(index + 1));
                            if (Powers.ContainsKey(name))
                            {
                                Powers[name] = cost;
                            }
                        } else if (s.Contains("AllowInCHIMPS"))
                        {
                            AllowInChimps = bool.Parse(s.Substring(s.IndexOf(char.Parse("=")) + 1));
                        } else if (s.Contains("RestrictAsSupport"))
                        {
                            RestrictAsSupport = bool.Parse(s.Substring(s.IndexOf(char.Parse("=")) + 1));
                        } if (s.Contains("Pierce"))
                        {
                            var index = s.IndexOf('=');
                            var name = s.Substring(0, index).Replace("Pierce", "");
                            var pierce = int.Parse(s.Substring(index + 1));
                            if (TrackPowers.ContainsKey(name))
                            {
                                TrackPowers[name] = pierce;
                            }
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
                    foreach (var power in Powers.Keys)
                    {
                        sw.WriteLine(power + "Cost=" + Powers[power]);
                    }
                    sw.WriteLine("#Set any of the above to -1 to disable them in the shop");
                    sw.WriteLine("AllowInCHIMPS=false");
                    sw.WriteLine("RestrictAsSupport=true");
                    foreach (var power in TrackPowers.Keys)
                    {
                        sw.WriteLine(power + "Pierce=" + TrackPowers[power]);
                    }
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
                foreach (var power in TrackPowers.Keys)
                {
                    var towerModel = CreateTower(power);

                    Game.instance.model.towers = Game.instance.model.towers.Append(towerModel).ToArray();
                }
            }
        }
        
        /*[HarmonyPatch(typeof(InputManager), nameof(InputManager.EnterPlacementMode))]
        internal class InputManager_EnterPlacementMode
        {
            [HarmonyPostfix]
            internal static void Patch(TowerModel forTowerModel)
            {
                if (TrackPowers.Contains(forTowerModel.name))
                {
                    Texture2D texture = Texture2D.blackTexture;
                    texture.Resize(20, 20);
                    InGameObjects.instance.PowerIconStart(Sprite.Create(texture, new Rect(0, 0, 20, 20), new UnityEngine.Vector2()));
                    InGameObjects.instance.powerIconImg = gameObject.AddComponent<Image>();
                }
            }
        }
        
        [HarmonyPatch(typeof(InputManager), nameof(InputManager.Update))]
        internal class InputManager_Update
        {
            [HarmonyPostfix]
            internal static void Patch()
            {
                var inputManager = InGame.instance.inputManager;
                var towerModel = inputManager.towerModel;
                if (towerModel != null && inputManager.inPlacementMode && TrackPowers.Contains(towerModel.name))
                { 
                    var map = InGame.instance.UnityToSimulation.simulation.Map;
                    var pos = inputManager.cursorPositionWorld; 
                    InGameObjects.instance.PowerIconUpdate(pos, map.CanPlace(new Vector2(pos), towerModel));
                }
            }
        }
        
        [HarmonyPatch(typeof(InputManager), nameof(InputManager.ExitPlacementMode))]
        internal class InputManager_ExitPlacementMode
        {
            [HarmonyPostfix]
            internal static void Patch()
            {
                if (InGameObjects.instance.powerIcon != null)
                {
                    InGameObjects.instance.PowerIconEnd();
                }
            }
        }
        */
        
        [HarmonyPatch(typeof(InGameObjects), nameof(InGameObjects.PowerIconStart))]
        internal class InGameObjects_PowerIconStart
        {
            [HarmonyPostfix]
            internal static void Patch(InGameObjects __instance, Sprite icon)
            {
                //MelonLogger.Log(icon.rect.);
            }
        }
        


        [HarmonyPatch(typeof(TowerModel), nameof(TowerModel.IsTowerPlaceableInAreaType))]
        internal class TowerModel_IsTowerPlaceableInAreaType
        {
            [HarmonyPostfix]
            internal static void Patch(TowerModel __instance, AreaType areaType, ref bool __result)
            {
                if (TrackPowers.ContainsKey(__instance.name))
                {
                    __result = areaType == AreaType.track;
                }
            }
        }
        
        [HarmonyPatch(typeof(ProfileModel), nameof(ProfileModel.Validate))]
        public class ProfileModelPatch
        {
            [HarmonyPostfix]
            public static void Postfix(ProfileModel __instance)
            {
                var unlockedTowers = __instance.unlockedTowers;

                foreach (var power in Powers.Keys)
                {
                    if (Powers[power] < 0)
                    {
                        continue;  
                    }
                    
                    if (!unlockedTowers.Contains(power))
                    {
                        unlockedTowers.Add(power);
                    }
                }
                
            }
        }
        
        [HarmonyPatch(typeof(TitleScreen), "UpdateVersion")]
        public class TitleScreenPatch
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                foreach (var power in Powers.Keys)
                {
                    if (Powers[power] < 0)
                    {
                        continue;
                    }
                    PowerModel powerModel = Game.instance.model.GetPowerWithName(power);

                    if (powerModel.tower != null)
                    {
                        if (powerModel.tower.icon == null)
                        {
                            powerModel.tower.icon = powerModel.icon;
                        }
                    
                        powerModel.tower.cost = Powers[power];
                        powerModel.tower.towerSet = "Support";
                    }
                }
            }
        }
        
        [HarmonyPatch(typeof(Tower), "Initialise")]
        public class Tower_Initialise
        {
            [HarmonyPostfix]
            public static void Postfix(Tower __instance)
            {
                if (TrackPowers.ContainsKey(__instance.towerModel.name))
                {
                    var powerBehaviorModel = Game.instance.model.GetPowerWithName(__instance.towerModel.name).behaviors
                        .First(b => !b.name.Contains("Create")).Cast<PowerBehaviorModel>();

                    InGame.instance.UnityToSimulation.simulation.powerManager.GetInstance(powerBehaviorModel).Activate(__instance.Position.ToVector2(), powerBehaviorModel, 0);
                    
                    
                }
                
            }
        }

        [HarmonyPatch(typeof(TowerInventory), "Init")]
        public class TowerInventoryPatch
        {
            public static List<TowerDetailsModel> allTowers = new List<TowerDetailsModel>();
            public static TowerInventory towerInventory;
            
            [HarmonyPrefix]
            public static bool Prefix(TowerInventory __instance, ref List<TowerDetailsModel> allTowersInTheGame)
            {
                int i = 22;
                foreach (var power in Powers.Keys)
                {
                    if (Powers[power] < 0)
                    {
                        continue;
                    }
                    
                    ShopTowerDetailsModel powerDetails = new ShopTowerDetailsModel(power, i++, 0, 0, 0, -1, 0, null);
                    allTowersInTheGame.Add(powerDetails);
                }

                allTowers = allTowersInTheGame;
                towerInventory = __instance;
                
                return true;
            }
        }
        
        [HarmonyPatch(typeof(UpgradeScreen), "UpdateUi")]
        public class UpgradeScreen_UpdateUi
        {
            [HarmonyPrefix]
            public static bool Prefix(ref string towerId)
            {
                foreach (var power in Powers.Keys)
                {
                    if (towerId.Contains(power))
                    {
                        towerId = "DartMonkey";
                    }
                }

                return true;
            }
        }
        
        [HarmonyPatch(typeof(InGame), nameof(InGame.StartMatch))]
        internal class InGame_StartMatch
        {
            [HarmonyPostfix]
            internal static void Postfix()
            {
                var towerInventory = TowerInventoryPatch.towerInventory;
                var allTowers = TowerInventoryPatch.allTowers;

                if (RestrictAsSupport && InGame.instance.SelectedMode.Contains("Only") ||
                    !AllowInChimps && InGame.instance.SelectedMode.Contains("Clicks"))
                {
                    foreach (var tower in allTowers)
                    {
                        foreach (var power in Powers.Keys)
                        {
                            if (tower.towerId == power)
                            {
                                tower.towerCount = 0;
                            }
                        }
                    }
                }
                

                towerInventory.SetTowerMaxes(allTowers);
            }
        }


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
            towerModel.cost = Powers[power];
            towerModel.display = power;
            towerModel.baseId = power;
            towerModel.towerSet = "Support";
            towerModel.radiusSquared = 16;
            towerModel.radius = 4;
            towerModel.range = 0;
            towerModel.footprint = new CircleFootprintModel(power, 0, true, false, true);
                    
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

                    
            var powerBehaviorModel = powerModel.behaviors.First(b => !b.name.Contains("Create"));
            var projectleModel = GetProjectileModel(powerBehaviorModel);
                    
            if (projectleModel != null)
            {
                var displayModel = towerModel.behaviors.First(b => b.name.Contains("Display"))
                    .Cast<DisplayModel>();
                if (CanBeCast<RoadSpikesModel>(powerBehaviorModel))
                {
                    displayModel.display = "8ab0e3fbb093a554d84a85e18fe2acac";
                    displayModel.scale = 2.0f;
                }
                else
                {
                    displayModel.display = projectleModel.display;
                }

                projectleModel.pierce = TrackPowers[power];
                if (projectleModel.maxPierce != 0)
                {
                    projectleModel.maxPierce = TrackPowers[power];
                }
            }

            //towerModel.behaviors = towerModel.behaviors.Where(b => !b.name.Contains("Display")).ToArray();

            //towerModel.behaviors.First(b => b.name.Contains("Display")).Cast<DisplayModel>().display = powerModel.icon.GUID;

            towerModel.behaviors = towerModel.behaviors.Where(b => !b.name.Contains("Slow")).ToArray();

            return towerModel;
        }
    }
}