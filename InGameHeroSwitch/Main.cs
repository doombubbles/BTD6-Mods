using System;
using System.IO;
using Assets.Scripts.Models.Profile;
using Assets.Scripts.Models.TowerSets;
using Assets.Scripts.Simulation.Input;
using Assets.Scripts.Unity.UI_New.InGame;
using Assets.Scripts.Unity.UI_New.InGame.RightMenu;
using Assets.Scripts.Unity.UI_New.InGame.StoreMenu;
using Harmony;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(InGameHeroSwitch.Main), "In-Game Hero Switch", "1.0.3", "doombubbles")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]
namespace InGameHeroSwitch
{
    public class Main : MelonMod
    {
        public static ProfileModel profile;
        public static List<TowerDetailsModel> allTowers = new List<TowerDetailsModel>();
        public static TowerInventory towerInventory;
        public static string realSelectedHero;


        private static readonly string Dir = $"{Directory.GetCurrentDirectory()}\\Mods\\InGameHeroSwitch";
        private static readonly string Config = $"{Dir}\\config.txt";

        public static KeyCode CycleUp = KeyCode.PageUp;
        public static KeyCode CycleDown = KeyCode.PageDown;
        public static bool CycleIfPlaced;
        
        public override void OnApplicationStart()
        {
            Directory.CreateDirectory($"{Dir}");
            if (File.Exists(Config))
            {
                MelonLogger.Log("Reading config file");
                using (StreamReader sr = File.OpenText(Config))
                {
                    string s = "";
                    while ((s = sr.ReadLine()) != null)
                    {
                        try
                        {
                            if (s.StartsWith("#"))
                            {
                                continue;
                            }

                            if (s.StartsWith("CycleUp"))
                            {
                                CycleUp = (KeyCode) Enum.Parse(typeof(KeyCode), s.Substring(s.IndexOf(char.Parse("=")) + 1));
                            }
                            if (s.StartsWith("CycleDown"))
                            {
                                CycleDown = (KeyCode) Enum.Parse(typeof(KeyCode), s.Substring(s.IndexOf(char.Parse("=")) + 1));
                            } 
                            if (s.StartsWith("CycleIfPlaced"))
                            {
                                CycleIfPlaced = bool.Parse(s.Substring(s.IndexOf(char.Parse("=")) + 1));
                            } 
                        }
                        catch (Exception e)
                        {
                            MelonLogger.LogError("Malformed line " + s);
                            e.GetType(); //just to get rid of the warning lol
                        }
                    }
                }
            }
            else
            {
                MelonLogger.Log("Creating config file");
                using (StreamWriter sw = File.CreateText(Config))
                {
                    sw.WriteLine("CycleUp=" + CycleUp);
                    sw.WriteLine("CycleDown=" + CycleDown);
                    sw.WriteLine("CycleIfPlaced=" + CycleIfPlaced);
                }
                MelonLogger.Log("Done Creating");
            }
        }

        public static void ChangeHero(int delta)
        {
            var hero = realSelectedHero;

            var index = profile.unlockedHeroes.IndexOf(hero);
            if (index < 0 || (!CycleIfPlaced &&
                ShopMenu.instance.GetTowerButtonFromBaseId(hero).GetLockedState() == TowerPurchaseLockState.TowerInventoryLocked))
            {
                return;
            }
            
            index = (index + delta + profile.unlockedHeroes.Count) % profile.unlockedHeroes.Count;
            var newHero = profile.unlockedHeroes[index];

            foreach (var unlockedHero in profile.unlockedHeroes)
            {
                foreach (var towerDetailsModel in allTowers)
                {
                    if (towerDetailsModel.name.Contains(newHero))
                    {
                        towerDetailsModel.towerCount = 1;
                    }
                    else if (towerDetailsModel.name.Contains(unlockedHero))
                    {
                        towerDetailsModel.towerCount = 0;
                    }
                }
            }

            towerInventory.SetTowerMaxes(allTowers);
            realSelectedHero = newHero;
            ShopMenu.instance.ClearButtons();
            ShopMenu.instance.Initialise();
            ShopMenu.instance.PostInitialised();
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(CycleUp))
            {
                ChangeHero(-1);
            } else if (Input.GetKeyDown(CycleDown))
            {
                ChangeHero(1);
            }
        }
        
        [HarmonyPatch(typeof(TowerInventory), nameof(TowerInventory.Init))]
        internal class TowerInventory_Init
        {

            [HarmonyPrefix]
            internal static bool Prefix(TowerInventory __instance, ref List<TowerDetailsModel> allTowersInTheGame)
            {
                towerInventory = __instance;
                allTowers = allTowersInTheGame;
                realSelectedHero = InGame.instance.SelectedHero;
                return true;
            }
        }
        
        [HarmonyPatch(typeof(ProfileModel), nameof(ProfileModel.Validate))]
        internal class ProfileModel_Validate
        {
            [HarmonyPostfix]
            public static void Postfix(ProfileModel __instance)
            {
                profile = __instance;
            }
        }
    }
}