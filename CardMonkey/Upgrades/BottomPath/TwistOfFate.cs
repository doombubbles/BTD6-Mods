using Assets.Scripts.Models.Towers;
using Assets.Scripts.Models.Towers.Behaviors;
using Assets.Scripts.Models.Towers.Filters;
using Assets.Scripts.Models.Towers.Projectiles.Behaviors;
using Assets.Scripts.Unity;
using Assets.Scripts.Utils;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Towers;
using BTD_Mod_Helper.Extensions;
using CardMonkey.Displays;
using CardMonkey.Displays.Projectiles;
using CardMonkey.Upgrades.MiddlePath;

namespace CardMonkey.Upgrades.BottomPath
{
    public class TwistOfFate : ModUpgrade<CardMonkey>
    {
        public override int Path => BOTTOM;
        public override int Tier => 5;
        public override int Cost => 50000;

        public override string DisplayName => "Twist of Fate";
        public override string Description => "Cards can explode, stun Bloons, and make Bloons give extra money.";


        public override void ApplyUpgrade(TowerModel tower)
        {
            tower.range += 10;
            tower.GetAttackModel().range += 10;
            
            var redCard = tower.GetWeapon();
            
            var goldCard = redCard.Duplicate();
            goldCard.name = "WeaponModel_GoldCard";
            var r2gAlch = Game.instance.model.GetTower(TowerType.Alchemist, 0, 0, 4);
            var increaseBloonWorthModel = r2gAlch.GetAttackModels()[1].GetDescendant<IncreaseBloonWorthModel>().Duplicate();
            var filterOutTagModel = r2gAlch.GetDescendant<FilterOutTagModel>().Duplicate();
            increaseBloonWorthModel.filter = filterOutTagModel;
            goldCard.projectile.collisionPasses = new[] {-1, 0};
            goldCard.projectile.AddBehavior(increaseBloonWorthModel);
            if (tower.appliedUpgrades.Contains(UpgradeID<WildCards>()))
            {
                goldCard.projectile.ApplyDisplay<GoldWildCardDisplay>();
            }
            else
            {
                goldCard.projectile.ApplyDisplay<GoldCardDisplay>();
            }
            tower.GetAttackModel().AddWeapon(goldCard);

            var blueCard = redCard.Duplicate();
            blueCard.name = "WeaponModel_Blue_card";
            var bloonImpact = Game.instance.model.GetTower(TowerType.BombShooter, 4);
            var slowModel = bloonImpact.GetDescendant<SlowModel>().Duplicate();
            var slowModifierForTagModel = bloonImpact.GetDescendant<SlowModifierForTagModel>().Duplicate();
            blueCard.projectile.collisionPasses = new[] {-1, 0};
            blueCard.projectile.AddBehavior(slowModel);
            blueCard.projectile.AddBehavior(slowModifierForTagModel);
            if (tower.appliedUpgrades.Contains(UpgradeID<WildCards>()))
            {
                blueCard.projectile.ApplyDisplay<BlueWildCardDisplay>();
            }
            else
            {
                blueCard.projectile.ApplyDisplay<BlueCardDisplay>();
            }

            blueCard.Rate *= 1.2f;
            tower.GetAttackModel().AddWeapon(blueCard);
            
            var bomb = Game.instance.model.GetTower(TowerType.BombShooter, 3).GetWeapon().projectile.Duplicate();
            var pb = bomb.GetBehavior<CreateProjectileOnContactModel>();
            var sound = bomb.GetBehavior<CreateSoundOnProjectileCollisionModel>();
            var effect = bomb.GetBehavior<CreateEffectOnContactModel>();
            
            var behavior = new CreateProjectileOnExhaustFractionModel(
                "CreateProjectileOnExhaustFractionModel_",
                pb.projectile, pb.emission, 1f, 1f, true);
            redCard.projectile.AddBehavior(behavior);

            var soundBehavior = new CreateSoundOnProjectileExhaustModel(
                "CreateSoundOnProjectileExhaustModel_",
                sound.sound1, sound.sound2, sound.sound3, sound.sound4, sound.sound5);
            redCard.projectile.AddBehavior(soundBehavior);

            var eB = new CreateEffectOnExhaustedModel("CreateEffectOnExhaustedModel_", "", 0f, false,
                false, effect.effectModel);
            redCard.projectile.AddBehavior(eB);
            redCard.Rate *= 0.8f;
            
            FileIOUtil.SaveObject($"Towers\\{tower.name}.json", tower);
        }
    }
}