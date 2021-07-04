using Assets.Scripts.Models.Towers;
using BTD_Mod_Helper.Api.Towers;
using BTD_Mod_Helper.Extensions;

namespace CardMonkey.Upgrades.BottomPath
{
    public class CardCounting : ModUpgrade<CardMonkey>
    {
        public override int Path => BOTTOM;
        public override int Tier => 1;
        public override int Cost => 300;
        
        public override string Description => "Throws cards faster";

        public override void ApplyUpgrade(TowerModel tower)
        {
            foreach (var weaponModel in tower.GetWeapons())
            {
                weaponModel.Rate *= .666666f;
            }
        }
    }
}