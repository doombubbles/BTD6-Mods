using System.Linq;
using Assets.Scripts.Models.Towers;
using Assets.Scripts.Models.Towers.Projectiles.Behaviors;
using BTD_Mod_Helper.Api.Towers;
using BTD_Mod_Helper.Extensions;
using CardMonkey.Displays;
using CardMonkey.Displays.Projectiles;

namespace CardMonkey.Upgrades.MiddlePath
{
    public class AcesWild : ModUpgrade<CardMonkey>
    {
        public override int Path => MIDDLE;
        public override int Tier => 3;
        public override int Cost => 1500;
        
        public override string DisplayName => "Aces Wild";
        public override string Description => "Powerful Ace cards do more damage, further increased against Ceramic and Fortified Bloons.";


        public override void ApplyUpgrade(TowerModel tower)
        {
            foreach (var projectile in tower.GetWeapons().Select(weaponModel => weaponModel.projectile))
            {
                projectile.GetDamageModel().damage++;
                projectile.AddBehavior(new DamageModifierForTagModel("DamageModifierForTagModel_Ceramic", "Ceramic",
                    1, 3, false, false));
                projectile.AddBehavior(new DamageModifierForTagModel("DamageModifierForTagModel_Fortified", "Fortified",
                    1, 3, false, false));
                projectile.ApplyDisplay<WildAceCardDisplay>();
            }
            
            
        }
    }
}