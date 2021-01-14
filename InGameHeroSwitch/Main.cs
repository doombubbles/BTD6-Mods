using System;
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

[assembly: MelonInfo(typeof(InGameHeroSwitch.Main), "In-Game Hero Switch", "1.0.1", "doombubbles")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]
namespace InGameHeroSwitch
{
    public class Main : MelonMod
    {
        public static ProfileModel profile;
        public static List<TowerDetailsModel> allTowers = new List<TowerDetailsModel>();
        public static TowerInventory towerInventory;
        public static string realSelectedHero;

        public static void ChangeHero(int delta)
        {
            var hero = realSelectedHero;

            var index = profile.unlockedHeroes.IndexOf(hero);
            if (index < 0 ||
                ShopMenu.instance.GetTowerButtonFromBaseId(hero).GetLockedState() == TowerPurchaseLockState.TowerInventoryLocked)
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

        [HarmonyPatch(typeof(Input), nameof(Input.GetKeyDown), typeof(KeyCode))]
        internal class Input_GetKeyDown
        {
            private static bool last = false;
            
            [HarmonyPostfix]
            public static void Postfix(bool __result, KeyCode key)
            {
                if (__result && !last)
                {
                    if (key == KeyCode.PageUp)
                    {
                        ChangeHero(-1);
                    } else if (key == KeyCode.PageDown)
                    {
                        ChangeHero(1);
                    }
                }

                last = __result;
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