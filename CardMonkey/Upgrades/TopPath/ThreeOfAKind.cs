using Assets.Scripts.Models.Towers;
using Assets.Scripts.Models.Towers.Behaviors.Emissions;
using BTD_Mod_Helper.Api.Towers;
using BTD_Mod_Helper.Extensions;
using MelonLoader;

namespace CardMonkey.Upgrades.TopPath
{
    public class ThreeOfAKind : ModUpgrade<CardMonkey>
    {
        public override int Path => TOP;
        public override int Tier => 2;
        public override int Cost => 700;
        
        public override string DisplayName => "Three of a Kind";
        public override string Description => "Throws three cards at a time";


        public override void ApplyUpgrade(TowerModel tower)
        {
            tower.GetWeapon().emission = new ArcEmissionModel("ArcEmissionModel_", 3, 0, 15, null, false, false);
        }
    }
}