using Assets.Scripts.Models.Towers;
using Assets.Scripts.Models.Towers.Behaviors;
using Assets.Scripts.Models.Towers.Filters;
using Assets.Scripts.Unity;
using BTD_Mod_Helper.Api.Towers;
using BTD_Mod_Helper.Extensions;

namespace CardMonkey.Upgrades.BottomPath
{
    public class MarkedCards : ModUpgrade<CardMonkey>
    {
        public override int Path => BOTTOM;
        public override int Tier => 4;
        public override int Cost => 2500;

        public override string Description => "Attacks extra fast permanently and with bigger range";


        public override void ApplyUpgrade(TowerModel tower)
        {
            tower.range += 15;
            tower.GetAttackModel().range += 15;
            foreach (var weaponModel in tower.GetWeapons())
            {
                weaponModel.Rate *= .5f;
            }
        }
    }
}