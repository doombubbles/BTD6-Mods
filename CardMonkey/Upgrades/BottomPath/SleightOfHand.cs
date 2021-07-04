using Assets.Scripts.Models.Towers;
using Assets.Scripts.Models.Towers.Filters;
using BTD_Mod_Helper.Api.Towers;
using BTD_Mod_Helper.Extensions;

namespace CardMonkey.Upgrades.BottomPath
{
    public class SleightOfHand : ModUpgrade<CardMonkey>
    {
        public override int Path => BOTTOM;
        public override int Tier => 2;
        public override int Cost => 350;

        public override string DisplayName => "Sleight of Hand";
        public override string Description => "Can attack Camo Bloons";

        /// <summary>
        /// Default priority is 0, so this lower priority makes this Upgrade always apply last so that it will catch
        /// every single FilterInvisibleModel that might've been added.
        /// </summary>
        public override int Priority => -1;

        public override void ApplyUpgrade(TowerModel tower)
        {
            tower.GetDescendants<FilterInvisibleModel>().ForEach(model => model.isActive = false);
        }
    }
}