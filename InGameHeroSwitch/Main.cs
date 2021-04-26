using System;
using System.IO;
using System.Linq;
using Assets.Scripts.Models.Profile;
using Assets.Scripts.Models.TowerSets;
using Assets.Scripts.Simulation.Input;
using Assets.Scripts.Unity;
using Assets.Scripts.Unity.UI_New.InGame;
using Assets.Scripts.Unity.UI_New.InGame.RightMenu;
using Assets.Scripts.Unity.UI_New.InGame.StoreMenu;
using BTD_Mod_Helper;
using Harmony;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(InGameHeroSwitch.Main), "In-Game Hero Switch", "1.0.4", "doombubbles")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace InGameHeroSwitch
{
    public class Main : BloonsTD6Mod
    {
        public override string MelonInfoCsURL =>
            "https://raw.githubusercontent.com/doombubbles/BTD6-Mods/main/InGameHeroSwitch/Main.cs";

        public override string LatestURL =>
            "https://github.com/doombubbles/BTD6-Mods/blob/main/InGameHeroSwitch/InGameHeroSwitch.dll?raw=true";
        
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
                MelonLogger.Msg("Reading config file");
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
                                CycleUp = (KeyCode) Enum.Parse(typeof(KeyCode),
                                    s.Substring(s.IndexOf(char.Parse("=")) + 1));
                            }

                            if (s.StartsWith("CycleDown"))
                            {
                                CycleDown = (KeyCode) Enum.Parse(typeof(KeyCode),
                                    s.Substring(s.IndexOf(char.Parse("=")) + 1));
                            }

                            if (s.StartsWith("CycleIfPlaced"))
                            {
                                CycleIfPlaced = bool.Parse(s.Substring(s.IndexOf(char.Parse("=")) + 1));
                            }
                        }
                        catch (Exception e)
                        {
                            MelonLogger.Error("Malformed line " + s);
                            e.GetType(); //just to get rid of the warning lol
                        }
                    }
                }
            }
            else
            {
                MelonLogger.Msg("Creating config file");
                using (StreamWriter sw = File.CreateText(Config))
                {
                    sw.WriteLine("CycleUp=" + CycleUp);
                    sw.WriteLine("CycleDown=" + CycleDown);
                    sw.WriteLine("CycleIfPlaced=" + CycleIfPlaced);
                }

                MelonLogger.Msg("Done Creating");
            }
        }

        public static void ChangeHero(int delta)
        {
            var hero = realSelectedHero;

            if (!CycleIfPlaced && ShopMenu.instance.GetTowerButtonFromBaseId(hero).GetLockedState() ==
                TowerPurchaseLockState.TowerInventoryLocked)
            {
                return;
            }

            var heroDetailsModels = Game.instance.model.heroSet.Select(tdm => tdm.Cast<HeroDetailsModel>());
            var heroes = heroDetailsModels as HeroDetailsModel[] ?? heroDetailsModels.ToArray();
            
            var index = heroes.First(hdm => hdm.towerId == hero).towerIndex;
            var newHero = heroes.First(hdm => hdm.towerIndex == (index + delta + heroes.Length) % heroes.Length).towerId;
            
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
            }
            else if (Input.GetKeyDown(CycleDown))
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