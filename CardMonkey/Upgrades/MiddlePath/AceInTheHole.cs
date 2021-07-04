using Assets.Scripts.Models;
using Assets.Scripts.Models.Towers;
using Assets.Scripts.Models.Towers.Behaviors.Abilities;
using Assets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors;
using Assets.Scripts.Models.Towers.Behaviors.Attack;
using Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors;
using Assets.Scripts.Models.Towers.Behaviors.Emissions;
using Assets.Scripts.Models.Towers.Behaviors.Emissions.Behaviors;
using Assets.Scripts.Models.Towers.Filters;
using Assets.Scripts.Models.Towers.Projectiles.Behaviors;
using Assets.Scripts.Unity;
using BTD_Mod_Helper.Api.Towers;
using BTD_Mod_Helper.Extensions;
using CardMonkey.Displays;
using CardMonkey.Displays.Projectiles;
using CardMonkey.Upgrades.BottomPath;
using MelonLoader;
using UnhollowerBaseLib;

namespace CardMonkey.Upgrades.MiddlePath
{
    public class AceInTheHole : ModUpgrade<CardMonkey>
    {
        public override int Path => MIDDLE;
        public override int Tier => 4;
        public override int Cost => 4440;

        public override string DisplayName => "Ace in the Hole";

        public override string Description =>
            "Ability: Throws a super powerful Ace card that seeks Bloons along the track.";


        public override void ApplyUpgrade(TowerModel tower)
        {
            var abilityModel = new AbilityModel("AbilityModel_AceInTheHole", "Ace in the Hole",
                "Throws a super powerful Ace card that seeks Bloons along the track.", 1, 0,
                GetSpriteReference(Icon), 44f, null, false, false, null,
                0, 0, 9999999, false, false);
            tower.AddBehavior(abilityModel);

            var activateAttackModel = new ActivateAttackModel("ActivateAttackModel_AceInTheHole", 0, true,
                new Il2CppReferenceArray<AttackModel>(1), true, false, false, false, false);
            abilityModel.AddBehavior(activateAttackModel);

            var attackModel = activateAttackModel.attacks[0] =
                Game.instance.model.GetTower(TowerType.BoomerangMonkey, 4).GetAttackModel().Duplicate();
            activateAttackModel.AddChildDependant(attackModel);

            attackModel.behaviors = attackModel.behaviors
                .RemoveItemOfType<Model, RotateToTargetModel>()
                .RemoveItemOfType<Model, TargetStrongModel>()
                .RemoveItemOfType<Model, TargetLastModel>()
                .RemoveItemOfType<Model, TargetCloseModel>();

            var targetFirstModel = attackModel.GetBehavior<TargetFirstModel>();
            targetFirstModel.isSelectable = false;
            attackModel.targetProvider = targetFirstModel;

            attackModel.range = 2000;
            attackModel.attackThroughWalls = true;


            var weapon = attackModel.weapons[0];
            weapon.emission.AddBehavior(new EmissionRotationOffBloonDirectionModel("EmissionRotationOffBloonDirectionModel", false, false));
            
            var projectileModel = weapon.projectile;
            
            projectileModel.ApplyDisplay<AceInTheHoleAbilityDisplay>();
            projectileModel.pierce = 1000;
            projectileModel.RemoveBehavior<RotateModel>();
            projectileModel.GetBehavior<RetargetOnContactModel>().distance = 2000;
            projectileModel.GetDamageModel().damage = 20;
            projectileModel.GetDamageModel().immuneBloonProperties = BloonProperties.None;
            projectileModel.AddBehavior(new DamageModifierForTagModel("DamageModifierForTagModel_Ceramic", "Ceramic",
                1, 30, false, false));
            projectileModel.AddBehavior(new DamageModifierForTagModel("DamageModifierForTagModel_Fortified", "Fortified",
                1, 30, false, false));
            projectileModel.GetBehavior<TravelStraitModel>().Speed = 500f;
            projectileModel.GetBehavior<TravelStraitModel>().Lifespan = 5.0f;
        }
    }
}