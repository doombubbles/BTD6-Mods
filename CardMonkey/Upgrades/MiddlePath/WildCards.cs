using Assets.Scripts.Models.Towers;
using BTD_Mod_Helper.Api.Towers;
using BTD_Mod_Helper.Extensions;
using CardMonkey.Displays;
using CardMonkey.Displays.Projectiles;

namespace CardMonkey.Upgrades.MiddlePath
{
    public class WildCards : ModUpgrade<CardMonkey>
    {
        public override int Path => MIDDLE;
        public override int Tier => 2;
        public override int Cost => 1000;
        
        public override string Description => "Cards can damage all Bloon types";


        public override void ApplyUpgrade(TowerModel tower)
        {
            foreach (var weaponModel in tower.GetWeapons())
            {
                weaponModel.projectile.GetDamageModel().immuneBloonProperties = BloonProperties.None;
                weaponModel.projectile.ApplyDisplay<RedWildCardDisplay>();
            }
        }
    }
}