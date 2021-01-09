using System.Linq;
using Assets.Scripts.Models.TowerSets;
using Assets.Scripts.Simulation.Input;
using Assets.Scripts.Unity;
using Assets.Scripts.Unity.Menu;
using Assets.Scripts.Unity.UI_New.HeroInGame;
using Assets.Scripts.Unity.UI_New.InGame;
using Assets.Scripts.Unity.UI_New.InGame.RightMenu;
using Assets.Scripts.Unity.UI_New.InGame.StoreMenu;
using Assets.Scripts.Unity.UI_New.Main.HeroSelect;
using Assets.Scripts.Unity.UI_New.Main.MonkeySelect;
using Assets.Scripts.Unity.UI_New.Transitions;
using Harmony;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using NKHook6.Api.Extensions;
using NKHook6.Api.Events;
using UnityEngine;
using Random = System.Random;

[assembly: MelonInfo(typeof(InGameHeroSwitch.Main), "In-Game Hero Switch", "1.0.0", "doombubbles")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]
namespace InGameHeroSwitch
{
    public class Main : MelonMod
    {

        public static List<TowerDetailsModel> allTowers = new List<TowerDetailsModel>();
        public static TowerInventory towerInventory;
        public static string realSelectedHero;

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();
            EventRegistry.instance.listen(typeof(Main));
        }

        public static void ChangeHero(int delta)
        {
            var profile = Game.instance.getProfileModel();

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
        
        [HarmonyPatch(typeof(HeroInGameScreen), nameof(HeroInGameScreen.Awake))]
        internal class HeroInGameScreen_Awake
        {
            [HarmonyPostfix]
            internal static void Postfix(HeroInGameScreen __instance)
            {
                
            }
        }

        [EventAttribute("KeyPressEvent")]
        public static void onEvent(KeyEvent e)
        {
            KeyCode key = e.key;
            if (key == KeyCode.PageUp)
            {
                ChangeHero(-1);
            } else if (key == KeyCode.PageDown)
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
    }
}