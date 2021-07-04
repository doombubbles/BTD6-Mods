using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Models.Towers;
using Assets.Scripts.Models.TowerSets;
using BTD_Mod_Helper.Api.Towers;
using BTD_Mod_Helper.Extensions;
using CardMonkey.Displays;
using CardMonkey.Displays.Projectiles;

namespace CardMonkey
{
    /// <summary>
    /// The main class that adds the core tower to the game
    /// </summary>
    public class CardMonkey : ModTower
    {
        // public override string Portrait => "Don't need to override this, using the default of Name-Portrait";
        // public override string Icon => "Don't need to override this, using the default of Name-Icon";

        public override string TowerSet => PRIMARY;
        public override string BaseTower => TowerType.DartMonkey;
        public override int Cost => 400;

        public override int TopPathUpgrades => 5;
        public override int MiddlePathUpgrades => 5;
        public override int BottomPathUpgrades => 5;
        public override string Description => "Throws playing cards at Bloons";

        // public override string DisplayName => "Don't need to override this, the default turns it into 'Card Monkey'"

        public override void ModifyBaseTowerModel(TowerModel towerModel)
        {
            towerModel.range += 10;
            var attackModel = towerModel.GetAttackModel();
            attackModel.range += 10;
            
            
            var projectile = attackModel.weapons[0].projectile;
            projectile.ApplyDisplay<RedCardDisplay>();  // Make the projectiles look like Cards
            projectile.pierce += 2;
        }

        /// <summary>
        /// Make Card Monkey go right after the Boomerang Monkey in the shop
        /// </summary>
        public override int GetTowerIndex(List<TowerDetailsModel> towerSet)
        {
            return towerSet.First(model => model.towerId == TowerType.BoomerangMonkey).towerIndex + 1;
        }
    }
}