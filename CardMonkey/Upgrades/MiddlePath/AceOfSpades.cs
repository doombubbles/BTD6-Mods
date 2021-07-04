using System.Linq;
using Assets.Scripts.Models.Towers;
using Assets.Scripts.Models.Towers.Projectiles;
using Assets.Scripts.Models.Towers.Projectiles.Behaviors;
using BTD_Mod_Helper.Api.Towers;
using BTD_Mod_Helper.Extensions;
using CardMonkey.Displays;
using CardMonkey.Displays.Projectiles;

namespace CardMonkey.Upgrades.MiddlePath
{
    public class AceOfSpades : ModUpgrade<CardMonkey>
    {
        public override int Path => MIDDLE;
        public override int Tier => 5;
        public override int Cost => 44440;
        
        public override string DisplayName => "Ace of Spades";
        public override string Description => "Aces are even more deadly, dealing extreme damage to MOAB class bloons.";


        public override void ApplyUpgrade(TowerModel tower)
        {
            tower.range += 10;
            tower.GetAttackModel().range += 10;
            
            foreach (var projectile in tower.GetWeapons().Select(weaponModel => weaponModel.projectile))
            {
                projectile.GetDamageModel().damage += 3;
                
                foreach (var damageModifierForTagModel in projectile.GetBehaviors<DamageModifierForTagModel>())
                {
                    damageModifierForTagModel.damageAddative += 42;
                }
                
                projectile.AddBehavior(new DamageModifierForTagModel("DamageModifierForTagModel_Moabs", "Moabs",
                    1, 45, false, false));
                
                projectile.ApplyDisplay<AceOfSpadesCardDisplay>();
            }

            var abilityModel = tower.GetAbility();
            abilityModel.resetCooldownOnTierUpgrade = true;
            abilityModel.icon = GetSpriteReference(Icon);
            var projectileModel = abilityModel.GetDescendant<ProjectileModel>();
            projectileModel.ApplyDisplay<AceOfSpadesAbilityDisplay>();
            projectileModel.GetDamageModel().damage = 1000;
            projectileModel.pierce = 10000;
            foreach (var damageModifierForTagModel in projectileModel.GetBehaviors<DamageModifierForTagModel>())
            {
                damageModifierForTagModel.damageAddative = 2000;
            }
            projectileModel.AddBehavior(new DamageModifierForTagModel("DamageModifierForTagModel_Moabs", "Moabs",
                1, 2000, false, false));
        }
    }
}