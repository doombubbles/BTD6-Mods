using System.IO;
using System.Linq;
using Assets.Main.Scenes;
using Assets.Scripts.Models.Map;
using Assets.Scripts.Models.Powers;
using Assets.Scripts.Models.Profile;
using Assets.Scripts.Models.Towers;
using Assets.Scripts.Models.TowerSets;
using Assets.Scripts.Simulation.Input;
using Assets.Scripts.Simulation.Towers;
using Assets.Scripts.Unity;
using Assets.Scripts.Unity.Bridge;
using Assets.Scripts.Unity.UI_New;
using Assets.Scripts.Unity.UI_New.InGame;
using Assets.Scripts.Unity.UI_New.InGame.RightMenu.Powers;
using Assets.Scripts.Unity.UI_New.InGame.TowerSelectionMenu.TowerSelectionMenuThemes;
using Assets.Scripts.Unity.UI_New.Upgrade;
using Harmony;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using NKHook6.Api.Extensions;
using UnityEngine;
using UnityEngine.UI;
using InputManager = Assets.Scripts.Unity.UI_New.InGame.InputManager;
using Vector2 = Assets.Scripts.Simulation.SMath.Vector2;

[assembly: MelonInfo(typeof(PowersInShop.Main), "Powers In Shop", "1.1.1", "doombubbles")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]
namespace PowersInShop
{
    public class Main : MelonMod
    {
        private static readonly string Dir = $"{Directory.GetCurrentDirectory()}\\Mods\\PowersInShop";
        private static readonly string Config = $"{Dir}\\config.txt";

        public static Dictionary<string, int> Powers = new Dictionary<string, int>();
        public static Dictionary<string, int> TrackPowers = new Dictionary<string, int>();

        public static bool AllowInChimps = false;
        public static bool RestrictAsSupport = true;
        public static int RechargePrice = 500;

        public override void OnApplicationStart()
        {
            Powers.Add("BananaFarmer", 500); 
            Powers.Add("TechBot", 500);
            Powers.Add("Pontoon", 1000);
            Powers.Add("PortableLake", 1000);
            Powers.Add("EnergisingTotem", 1500);
            Powers.Add("RoadSpikes", 50);
            Powers.Add("GlueTrap", 100);
            Powers.Add("CamoTrap", 100);
            Powers.Add("MoabMine", 500);
            
            TrackPowers.Add("RoadSpikes", 20);
            TrackPowers.Add("GlueTrap", 300);
            TrackPowers.Add("MoabMine", 1);
            TrackPowers.Add("CamoTrap", 500);
            


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
                        } else if (s.Contains("Pierce"))
                        {
                            var index = s.IndexOf('=');
                            var name = s.Substring(0, index).Replace("Pierce", "");
                            var pierce = int.Parse(s.Substring(index + 1));
                            if (TrackPowers.ContainsKey(name))
                            {
                                TrackPowers[name] = pierce;
                            }
                        } else if (s.Contains("Recharge"))
                        {
                            RechargePrice = int.Parse(s.Substring(s.IndexOf(char.Parse("=")) + 1));
                        }
                    }
                }
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
                    sw.WriteLine("AllowInCHIMPS=" + AllowInChimps);
                    sw.WriteLine("RestrictAsSupport=" + RestrictAsSupport);
                    foreach (var power in TrackPowers.Keys)
                    {
                        sw.WriteLine(power + "Pierce=" + TrackPowers[power]);
                    }
                    sw.WriteLine("RechargePrice=" + RechargePrice);
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
                    var towerModel = Utils.CreateTower(power);

                    Game.instance.model.towers = Game.instance.model.towers.Append(towerModel).ToArray();
                }
            }
        }
        
        [HarmonyPatch(typeof(InputManager), nameof(InputManager.EnterPlacementMode))]
        internal class InputManager_EnterPlacementMode
        {
            [HarmonyPostfix]
            internal static void Patch(TowerModel forTowerModel)
            {
                if (TrackPowers.ContainsKey(forTowerModel.name))
                {
                    int i = 7;
                    foreach (var power in TrackPowers.Keys)
                    {
                        if (power == forTowerModel.name)
                        {
                            break;
                        }
                        i++;
                    }
                    var image = PowersMenu.instance.gameObject.transform.GetChild(0).GetChild(2).GetChild(0)
                        .GetChild(i).GetChild(0).GetChild(1).gameObject.GetComponent<Image>();

                    //Utils.RecursivelyLogGameObject(PowersMenu.instance.gameObject.transform);
                    InGameObjects.instance.PowerIconStart(image.sprite);
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
                if (towerModel != null && inputManager.inPlacementMode && TrackPowers.ContainsKey(towerModel.name))
                { 
                    var map = InGame.instance.UnityToSimulation.simulation.Map;
                    InGameObjects.instance.PowerIconUpdate(inputManager.GetCursorPosition(), map.CanPlace(new Vector2(inputManager.cursorPositionWorld), towerModel));
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

        [HarmonyPatch(typeof(TSMThemeEnergisingTotem), nameof(TSMThemeEnergisingTotem.OnButtonPress))]
        public class TSMThemeEnergisingTotem_OnButtonPress
        {
            [HarmonyPrefix]
            public static bool Prefix(TSMThemeEnergisingTotem __instance, TowerToSimulation tower, TSMButton button)
            {
                if (tower.worth > 0)
                {
                    var cash = InGame.instance.getCash();
                    if (cash < Utils.RealRechargePrice())
                    {
                        return false;
                    }
                    InGame.instance.setCash(cash - Utils.RealRechargePrice());
                    var mm = Game.instance.playerService.Player.Data.monkeyMoney.Value;
                    Game.instance.playerService.Player.Data.monkeyMoney.Value = mm + 20;
                }

                return true;
            }
        }
        
        [HarmonyPatch(typeof(TSMThemeEnergisingTotem), nameof(TSMThemeEnergisingTotem.Selected))]
        public class TSMThemeEnergisingTotem_Selected
        {
            public static bool lastOpened;
            private static string og;
            private static Color color;
            private static Color32 outline;
            
            [HarmonyPostfix]
            public static void Postfix(TSMThemeEnergisingTotem __instance, TowerToSimulation tower)
            {
                if (og == null)
                {
                    og = __instance.rechargeCostText.m_text;
                    color = __instance.rechargeCostText.color;
                    outline = __instance.rechargeCostText.outlineColor;
                }
                
                if (tower.worth > 0)
                {
                    __instance.rechargeCostText.Method_Private_Virtual_Final_New_Void_String_0("$" + Utils.RealRechargePrice());
                    __instance.rechargeCostText.outlineColor = new Color32(0, 0, 0, 0);
                    __instance.rechargeCostText.color = Color.white;
                    
                    if (!lastOpened)
                    {
                        __instance.rechargeButton.transform.GetChild(1).Translate(-5000, 0, 0);
                        __instance.rechargeButton.transform.GetChild(1).GetChild(0).Translate(5000, 0, 0);
                        __instance.rechargeButton.transform.GetChild(1).GetChild(1).Translate(4970, 0, 0);
                    }

                    lastOpened = true;
                }
                else
                {
                    __instance.rechargeCostText.Method_Private_Virtual_Final_New_Void_String_0(og);
                    __instance.rechargeCostText.outlineColor = outline;
                    __instance.rechargeCostText.color = color;
                    if (lastOpened)
                    {
                        __instance.rechargeButton.transform.GetChild(1).Translate(5000, 0, 0);
                        __instance.rechargeButton.transform.GetChild(1).GetChild(0).Translate(-5000, 0, 0);
                        __instance.rechargeButton.transform.GetChild(1).GetChild(1).Translate(-4970, 0, 0);
                    }

                    lastOpened = false;
                }
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

                TSMThemeEnergisingTotem_Selected.lastOpened = false; //UI is reset, so we have to as well
                
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
        
    }
}