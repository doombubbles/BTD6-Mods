using System;
using System.IO;
using System.Linq;
using Assets.Scripts.Models.TowerSets;
using Assets.Scripts.Unity;
using Assets.Scripts.Unity.UI_New.InGame;
using Assets.Scripts.Unity.UI_New.InGame.RightMenu;
using Assets.Scripts.Unity.UI_New.InGame.StoreMenu;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Extensions;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(InGameHeroSwitch.Main), "In-Game Hero Switch", "1.0.5", "doombubbles")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace InGameHeroSwitch
{
    public class Main : BloonsTD6Mod
    {
        public override string MelonInfoCsURL =>
            "https://raw.githubusercontent.com/doombubbles/BTD6-Mods/main/InGameHeroSwitch/Main.cs";

        public override string LatestURL =>
            "https://github.com/doombubbles/BTD6-Mods/blob/main/InGameHeroSwitch/InGameHeroSwitch.dll?raw=true";
        
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
            var hero = realSelectedHero ?? InGame.instance.SelectedHero;

            if (!CycleIfPlaced && ShopMenu.instance.GetTowerButtonFromBaseId(hero).GetLockedState() ==
                TowerPurchaseLockState.TowerInventoryLocked)
            {
                return;
            }

            var towerInventory = InGame.instance.GetTowerInventory();
            var unlockedHeroes = Game.instance.GetPlayerProfile().unlockedHeroes;

            var heroDetailsModels = InGame.instance.GetGameModel().heroSet.Select(tdm => tdm.Cast<HeroDetailsModel>());
            var heroes = heroDetailsModels as HeroDetailsModel[] ?? heroDetailsModels.ToArray();
            
            var index = heroes.First(hdm => hdm.towerId == hero).towerIndex;
            var newHero = "";
            while (!unlockedHeroes.Contains(newHero))
            {
                index += delta;
                index = (index + heroes.Length) % heroes.Length;
                newHero = heroes.First(hdm => hdm.towerIndex == index).towerId;
            }

            foreach (var unlockedHero in unlockedHeroes)
            {
                towerInventory.towerMaxes[unlockedHero] = 0;
            }
            
            towerInventory.towerMaxes[newHero] = 1;
            
            realSelectedHero = newHero;
            ShopMenu.instance.RebuildTowerSet();
            foreach (var button in ShopMenu.instance.activeTowerButtons)
            {
                button.Update();
            }
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
    }
}