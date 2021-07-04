using Assets.Scripts.Models.Towers;
using Assets.Scripts.Models.Towers.Behaviors;
using Assets.Scripts.Models.Towers.Filters;
using Assets.Scripts.Unity;
using BTD_Mod_Helper.Api.Towers;
using BTD_Mod_Helper.Extensions;

namespace CardMonkey.Upgrades.BottomPath
{
    public class StackedDeck : ModUpgrade<CardMonkey>
    {
        public override int Path => BOTTOM;
        public override int Tier => 3;
        public override int Cost => 750;

        public override string Description => "Attacks extra fast at the start of the round";


        public override void ApplyUpgrade(TowerModel tower)
        {
            if (tower.tier == 3) // Higher tiers replace this with a permanent buff, so don't included it otherwise
            {
                var startOfRoundRateBuffModel = Game.instance.model.GetTower(TowerType.SpikeFactory, 0, 0, 2)
                    .GetBehavior<StartOfRoundRateBuffModel>().Duplicate();
                startOfRoundRateBuffModel.modifier = .5f;
                startOfRoundRateBuffModel.duration = 10;
                tower.AddBehavior(startOfRoundRateBuffModel);
            }
        }
    }
}