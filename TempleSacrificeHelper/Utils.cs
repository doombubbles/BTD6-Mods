using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Assets.Scripts.Models.Towers.Behaviors;
using Assets.Scripts.Models.Towers.Upgrades;
using Assets.Scripts.Simulation.Towers;
using Assets.Scripts.Simulation.Towers.Behaviors;
using Assets.Scripts.Unity.Bridge;
using Assets.Scripts.Unity.UI_New.InGame;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Extensions;
using MelonLoader;
using UnityEngine;
using static Assets.Scripts.Simulation.Towers.Behaviors.ParagonTower;
using Color = UnityEngine.Color;
using Image = UnityEngine.UI.Image;
using Vector2 = Assets.Scripts.Simulation.SMath.Vector2;

namespace TempleSacrificeHelper
{
    public class Utils
    {
        public static void DefaultTemple(UpgradeModel upgradeModel)
        {
            upgradeModel.confirmation = "Sun Temple";
            upgradeModel.cost = BloonsTD6Mod.CostForDifficulty(100000, InGame.instance);
        }

        public static void ModifyTemple(UpgradeModel upgradeModel)
        {
            upgradeModel.confirmation = "";
            upgradeModel.cost = BloonsTD6Mod.CostForDifficulty(Main.TempleAlternateCost, InGame.instance);
        }

        public static void DefaultGod(UpgradeModel upgradeModel)
        {
            upgradeModel.confirmation = "True Sun Temple";
            upgradeModel.cost = BloonsTD6Mod.CostForDifficulty(500000, InGame.instance);
        }

        public static void ModifyGod(UpgradeModel upgradeModel)
        {
            upgradeModel.confirmation = "";
            upgradeModel.cost = BloonsTD6Mod.CostForDifficulty(Main.GodAlternateCost, InGame.instance);
        }

        public static Dictionary<string, Color> GetColors(Dictionary<string, float> worths, bool god)
        {
            var ret = new Dictionary<string, Color>();
            if (!god)
            {
                var worst = "";
                var min = float.MaxValue;
                foreach (var key in worths.Keys)
                {
                    if (worths[key] < min)
                    {
                        worst = key;
                        min = worths[key];
                    }
                }

                ret[worst] = Color.red;
            }


            foreach (var key in worths.Keys)
            {
                if (ret.ContainsKey(key)) continue;
                var worth = worths[key];
                var color = Color.red;
                if (worth > 50000)
                {
                    color = Color.green;
                }
                else if (key == "Magic")
                {
                    if (worth > 1000)
                    {
                        color = Color.white;
                    }
                }
                else if (worth > 300)
                {
                    color = Color.white;
                }

                ret[key] = color;
            }

            return ret;
        }

        public static Dictionary<string, float> GetTowerWorths(Tower tower)
        {
            return new Dictionary<string, float>
            {
                ["Primary"] = GetTowerSetWorth("Primary", tower),
                ["Military"] = GetTowerSetWorth("Military", tower),
                ["Magic"] = GetTowerSetWorth("Magic", tower),
                ["Support"] = GetTowerSetWorth("Support", tower)
            };
        }

        private static float GetTowerSetWorth(string towerSet, Tower tower)
        {
            return InGame.instance.GetTowerManager()
                .GetTowersInRange(tower.Position, tower.towerModel.range)
                .ToList()
                .Where(t => t.towerModel.towerSet == towerSet && t.Id != tower.Id)
                .Sum(t => t.worth);
        }

        private static ParagonTower FakeParagonTower(Tower tower)
        {
            var paragonTower = new ParagonTower
            {
                Sim = tower.Sim
            };
            paragonTower.Initialise(tower.entity, tower.model);
            return paragonTower;
        }

        public static int GetParagonDegree(TowerToSimulation tower, out InvestmentInfo investmentInfo)
        {
            var index = 0;
            var paragonTower = FakeParagonTower(tower.tower);
            investmentInfo = InGame.instance.GetAllTowerToSim()
                .Where(tts => tts.Def.baseId == tower.Def.baseId)
                .Select(tts => paragonTower.GetTowerInvestment(tts.tower, tts.Def.tier >= 5 ? index++ : index))
                .Aggregate(paragonTower.CombineInvestments);

            var requirements = InGame.instance.GetGameModel().paragonDegreeDataModel.powerDegreeRequirements;

            var degree = 0;
            while (investmentInfo.totalInvestment >= requirements[degree])
            {
                degree++;
                if (degree == 100)
                {
                    break;
                }
            }

            paragonTower.Destroy();
            return degree;
        }
    }
}