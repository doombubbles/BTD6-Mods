using System;
using Assets.Scripts.Models.Bloons.Behaviors;
using Assets.Scripts.Models.Towers;
using Assets.Scripts.Models.Towers.Behaviors;
using Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors;
using Assets.Scripts.Models.Towers.Behaviors.Emissions;
using Assets.Scripts.Models.Towers.Behaviors.Emissions.Behaviors;
using Assets.Scripts.Models.Towers.Filters;
using Assets.Scripts.Models.Towers.Projectiles;
using Assets.Scripts.Models.Towers.Projectiles.Behaviors;
using Assets.Scripts.Models.Towers.Weapons.Behaviors;
using Assets.Scripts.Unity;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Extensions;
using Il2CppSystem.Collections.Generic;
using UnhollowerBaseLib;
using static Assets.Scripts.Models.Towers.TowerType;

namespace MegaKnowledge
{
    public class Towers
    {
        public static void SplodeyDarts(TowerModel model)
        {
            foreach (var projectileModel in model.GetDescendants<ProjectileModel>().ToList())
            {
                var bomb = Game.instance.model.GetTowerFromId(BombShooter).GetWeapon()
                    .projectile.Duplicate();
                var pb = bomb.GetBehavior<CreateProjectileOnContactModel>();
                var sound = bomb.GetBehavior<CreateSoundOnProjectileCollisionModel>();
                var effect = bomb.GetBehavior<CreateEffectOnContactModel>();


                if (model.appliedUpgrades.Contains("Enhanced Eyesight"))
                {
                    pb.projectile.GetBehavior<ProjectileFilterModel>().filters
                        .GetItemOfType<FilterModel, FilterInvisibleModel>().isActive = false;
                }

                /*pb.name = "CreateProjectileOnContactModel_SplodeyDarts";
                sound.name = "CreateSoundOnProjectileCollisionModel_SplodeyDarts";
                effect.name = "CreateEffectOnContactModel_SplodeyDarts";
                projectileModel.AddBehavior(pb);
                projectileModel.AddBehavior(sound);
                projectileModel.AddBehavior(effect);*/

                var behavior = new CreateProjectileOnExhaustFractionModel(
                    "CreateProjectileOnExhaustFractionModel_SplodeyDarts",
                    pb.projectile, pb.emission, 1f, 1f, true);
                projectileModel.AddBehavior(behavior);

                var soundBehavior = new CreateSoundOnProjectileExhaustModel(
                    "CreateSoundOnProjectileExhaustModel_SplodeyDarts",
                    sound.sound1, sound.sound2, sound.sound3, sound.sound4, sound.sound5);
                projectileModel.AddBehavior(soundBehavior);

                var eB = new CreateEffectOnExhaustedModel("CreateEffectOnExhaustedModel_SplodeyDarts", "", 0f, false,
                    false, effect.effectModel);
                projectileModel.AddBehavior(eB);
            }
        }


        public static void DoubleRanga(TowerModel model)
        {
            var weaponModel = model.GetAttackModel().weapons[0];
            var random = new RandomArcEmissionModel("RandomArcEmissionModel_", 2, 0, 0, 30, 1, null);
            var eM = new ArcEmissionModel("ArcEmissionModel_", 2, 0, 30, null, false);
            weaponModel.emission = eM;
        }

        public static void RealHealthyBananas(TowerModel model)
        {
            var amount = model.tier + 1;
            var bonusLivesPerRoundModel = model.GetBehavior<BonusLivesPerRoundModel>();
            if (bonusLivesPerRoundModel == null)
            {
                model.AddBehavior(new BonusLivesPerRoundModel("BonusLivesPerRoundModel_HealthyBananas", amount, 1.25f,
                    "eb70b6823aec0644c81f873e94cb26cc"));
            }
            else
            {
                bonusLivesPerRoundModel.amount = amount;
            }
        }

        public static void Dreadnought(TowerModel model)
        {
            var attackModel = model.GetAttackModel();
            var flameGrape = Game.instance.model.GetTower(MonkeyBuccaneer, 0, 2, 0).GetWeapons()[3]
                .projectile;
            foreach (var projectileModel in attackModel.GetDescendants<ProjectileModel>().ToList())
            {
                if (projectileModel.name.Contains("Explosion") || projectileModel.GetDamageModel() == null)
                {
                    continue;
                }


                projectileModel.collisionPasses = flameGrape.collisionPasses;
                projectileModel.AddBehavior(flameGrape.GetBehavior<AddBehaviorToBloonModel>().Duplicate());
                if (model.appliedUpgrades.Contains("Buccaneer-Hot Shot"))
                {
                    projectileModel.GetBehavior<AddBehaviorToBloonModel>().GetBehavior<DamageOverTimeModel>()
                        .triggerImmediate = true;
                    projectileModel.scale = .5f;
                }
                else
                {
                    projectileModel.scale = .75f;
                }

                projectileModel.display = "c840e245a0b1deb4284cfc3f953e16cf";
                projectileModel.GetDamageModel().immuneBloonProperties = BloonProperties.None;
            }
        }

        public static void CrystalBall(TowerModel model)
        {
            if (!model.appliedUpgrades.Contains("Guided Magic")) return;

            model.ignoreBlockers = false;

            var guidedMagic = model.GetWeapon().projectile.GetBehavior<TrackTargetModel>();
            foreach (var attackModel in model.GetAttackModels())
            {
                if (attackModel.GetBehavior<TargetFirstPrioCamoModel>() != null)
                {
                    attackModel.RemoveBehavior<TargetFirstPrioCamoModel>();
                    attackModel.AddBehavior(new TargetFirstSharedRangeModel("TargetFirstSharedRangeModel_",
                        true, true, false, false));
                }

                if (attackModel.GetBehavior<TargetLastPrioCamoModel>() != null)
                {
                    attackModel.RemoveBehavior<TargetLastPrioCamoModel>();
                    attackModel.AddBehavior(new TargetLastSharedRangeModel("TargetLastSharedRangeModel_",
                        true, true, false, false));
                }

                if (attackModel.GetBehavior<TargetClosePrioCamoModel>() != null)
                {
                    attackModel.RemoveBehavior<TargetClosePrioCamoModel>();
                    attackModel.AddBehavior(new TargetCloseSharedRangeModel("TargetCloseSharedRangeModel_",
                        true, true, false, false));
                }

                if (attackModel.GetBehavior<TargetStrongPrioCamoModel>() != null)
                {
                    attackModel.RemoveBehavior<TargetStrongPrioCamoModel>();
                    attackModel.AddBehavior(new TargetStrongSharedRangeModel("TargetStrongSharedRangeModel_",
                        true, true, false, false));
                }

                attackModel.attackThroughWalls = false;
            }

            foreach (var weaponModel in model.GetWeapons())
            {
                weaponModel.emission.AddBehavior(
                    new EmissionCamoIfTargetIsCamoModel("EmissionCamoIfTargetIsCamoModel_CamoEmissionBehavior"));
            }

            foreach (var projectileModel in model.GetDescendants<ProjectileModel>().ToList())
            {
                var travelStraitModel = projectileModel.GetBehavior<TravelStraitModel>();
                if (travelStraitModel != null)
                {
                    var newLifeSpan = travelStraitModel.Lifespan * (150 / travelStraitModel.Speed);
                    travelStraitModel.Lifespan = Math.Max(travelStraitModel.Lifespan, newLifeSpan);
                    if (projectileModel.GetBehavior<TrackTargetModel>() == null)
                    {
                        projectileModel.AddBehavior(guidedMagic.Duplicate());
                    }
                }

                projectileModel.ignoreBlockers = false;
            }
        }

        public static void TackAttack(TowerModel model)
        {
            model.GetAttackModel().fireWithoutTarget = true;

            foreach (var projectileModel in model.GetDescendants<ProjectileModel>().ToList())
            {
                var tsm = projectileModel.GetBehavior<TravelStraitModel>();
                if (tsm != null)
                {
                    tsm.Lifespan *= 1.5f;
                }
            }

            if (model.appliedUpgrades.Contains("Ring of Fire"))
            {
                model.range *= 1.5f;
            }
        }

        public static void IceFortress(TowerModel model)
        {
            var behavior = new RemoveBloonModifiersModel("RemoveBloonModifiersModel_", false, true, false, false, false,
                new List<string>());
            foreach (var projectileModel in model.GetDescendants<ProjectileModel>().ToList())
            {
                projectileModel.AddBehavior(behavior.Duplicate());
            }

            model.AddBehavior(new OverrideCamoDetectionModel("OverrideCamoDetectionModel_", true));
        }

        public static void RifleRange(TowerModel model)
        {
            var damage = model.GetWeapon().projectile.GetDamageModel().damage;
            model.GetWeapon().AddBehavior(new CritMultiplierModel("CritMultiplierModel_", damage * 2, 1, 6,
                "252e82e70578330429a758339e10fd25", true));

            model.GetWeapon().projectile.AddBehavior(new ShowTextOnHitModel("ShowTextOnHitModel_",
                "3dcdbc19136c60846ab944ada06695c0", 0.5f, false, ""));
        }

        public static void XrayVision(TowerModel model)
        {
            model.ignoreBlockers = true;
            foreach (var attackModel in model.GetAttackModels())
            {
                attackModel.attackThroughWalls = true;
            }

            foreach (var projectileModel in model.GetDescendants<ProjectileModel>().ToList())
            {
                projectileModel.ignoreBlockers = true;
                if (projectileModel.pierce > 0)
                {
                    projectileModel.pierce += model.tier + 1;
                }
            }
        }

        public static void DigitalAmplification(TowerModel model)
        {
            model.range *= 2;

            foreach (var attackModel in model.GetAttackModels())
            {
                attackModel.range *= 2;
            }
        }

        public static void MortarEmpowerment(TowerModel towerModel)
        {
            var boomer = Game.instance.model.GetTowerFromId(BoomerangMonkey);
            var attackModel = towerModel.GetAttackModel();
            foreach (var boomerTargetType in boomer.targetTypes)
            {
                towerModel.targetTypes = towerModel.targetTypes.AddTo(boomerTargetType);
            }


            var targetSelectedPointModel = attackModel.GetBehavior<TargetSelectedPointModel>();
            attackModel.RemoveBehavior<TargetSelectedPointModel>();
            attackModel.targetProvider = null;

            attackModel.AddBehavior(boomer.GetAttackModel().GetBehavior<TargetFirstModel>().Duplicate());
            attackModel.AddBehavior(boomer.GetAttackModel().GetBehavior<TargetLastModel>().Duplicate());
            attackModel.AddBehavior(boomer.GetAttackModel().GetBehavior<TargetCloseModel>().Duplicate());
            attackModel.AddBehavior(boomer.GetAttackModel().GetBehavior<TargetStrongModel>().Duplicate());

            attackModel.AddBehavior(targetSelectedPointModel);

            towerModel.towerSelectionMenuThemeId = "ActionButton";

            /*var ageModel = towerModel.GetWeapon().projectile.GetBehavior<AgeModel>();
            if (ageModel != null)
            {
                ageModel.Lifespan /= 2;
            }*/
        }

        public static void DartlingEmpowerment(TowerModel towerModel)
        {
            if (towerModel.appliedUpgrades.Contains("Bloon Area Denial System"))
            {
                return;
            }

            var boomer = Game.instance.model.GetTowerFromId(BoomerangMonkey);
            var attackModel = towerModel.GetAttackModel();

            foreach (var boomerTargetType in boomer.targetTypes)
            {
                towerModel.targetTypes = towerModel.targetTypes.AddTo(boomerTargetType);
            }

            attackModel.AddBehavior(boomer.GetAttackModel().GetBehavior<RotateToTargetModel>().Duplicate());

            var targetPointerModel = attackModel.GetBehavior<TargetPointerModel>();
            var targetSelectedPointModel = attackModel.GetBehavior<TargetSelectedPointModel>();

            attackModel.RemoveBehavior<TargetPointerModel>();
            attackModel.RemoveBehavior<TargetSelectedPointModel>();

            attackModel.AddBehavior(boomer.GetAttackModel().GetBehavior<TargetFirstModel>().Duplicate());
            attackModel.AddBehavior(boomer.GetAttackModel().GetBehavior<TargetLastModel>().Duplicate());
            attackModel.AddBehavior(boomer.GetAttackModel().GetBehavior<TargetCloseModel>().Duplicate());
            attackModel.AddBehavior(boomer.GetAttackModel().GetBehavior<TargetStrongModel>().Duplicate());

            attackModel.AddBehavior(targetPointerModel);
            attackModel.AddBehavior(targetSelectedPointModel);

            if (towerModel.appliedUpgrades.Contains("Faster Swivel"))
            {
                var travelStraitModel = attackModel.weapons[0].projectile.GetBehavior<TravelStraitModel>();
                if (travelStraitModel != null)
                {
                    travelStraitModel.Speed *= 47f / 35f;
                }
            }
        }

        public static void SpikeEmpowerment(TowerModel towerModel)
        {
            var mortar = Game.instance.model.GetTowerFromId(MortarMonkey);
            towerModel.towerSelectionMenuThemeId = "MortarMonkey";

            towerModel.targetTypes = new Il2CppReferenceArray<TargetType>(mortar.targetTypes);

            if (towerModel.targetTypes.Length > 1)
            {
                while (towerModel.targetTypes.Length > 1)
                {
                    towerModel.targetTypes = towerModel.targetTypes.RemoveItemOfType<TargetType, TargetType>();
                }

                towerModel.targetTypes[0].id = "TargetSelectedPoint";
                towerModel.targetTypes[0].intID = -1;
                towerModel.targetTypes[0].actionOnCreate = true;
                towerModel.targetTypes[0].isActionable = true;
            }


            towerModel.GetAttackModel().RemoveBehavior<TargetTrackModel>();
            towerModel.GetAttackModel().RemoveBehavior<SmartTargetTrackModel>();
            towerModel.GetAttackModel().RemoveBehavior<CloseTargetTrackModel>();
            towerModel.GetAttackModel().RemoveBehavior<FarTargetTrackModel>();


            var targetSelectedPointModel = towerModel.GetAttackModel().GetBehavior<TargetSelectedPointModel>();
            if (targetSelectedPointModel == null)
            {
                var tspm = new TargetSelectedPointModel("TargetSelectedPointModel_", true,
                    false, "4e88dd78c6e800d41a6df5b02d592082", .5f, "",
                    false, false, "", true);
                towerModel.GetAttackModel().AddBehavior(tspm);
            }

            towerModel.UpdateTargetProviders();
            towerModel.GetDescendant<ArriveAtTargetModel>().filterCollisionWhileMoving = false;
        }

        public static void BloonAreNotPrepared(TowerModel model)
        {
            if (model.appliedUpgrades.Contains("Heart of Vengeance"))
            {
                foreach (var weaponModel in model.GetWeapons())
                {
                    var lbasm = weaponModel.GetBehavior<LifeBasedAttackSpeedModel>();
                    if (lbasm != null)
                    {
                        var bonus = lbasm.lifeCap * lbasm.ratePerLife + lbasm.baseRateIncrease;
                        weaponModel.Rate /= 1 + bonus;
                        weaponModel.RemoveBehavior<LifeBasedAttackSpeedModel>();
                    }
                }
            }

            if (model.appliedUpgrades.Contains("Druid of Wrath"))
            {
                var dbasm = model.GetBehavior<DamageBasedAttackSpeedModel>();
                if (dbasm != null)
                {
                    var bonus = dbasm.maxStacks * dbasm.increasePerThreshold;
                    foreach (var weaponModel in model.GetWeapons())
                    {
                        weaponModel.Rate /= 1 + bonus;
                    }
                }
            }

            if (model.appliedUpgrades.Contains("Avatar of Wrath"))
            {
                var dvem = model.GetBehavior<DruidVengeanceEffectModel>();
                if (dvem != null)
                {
                    var dmwm = dvem.damageModifierWrathModel;
                    dmwm.rbeThreshold = 1;
                    dvem.epicGlowEffectStacks = -1;
                }
            }
        }

        public static void Oktoberfest(TowerModel model)
        {
            var brew = model.GetDescendant<AddBerserkerBrewToProjectileModel>();
            if (brew != null)
            {
                brew.cap = (int) (brew.cap * 1.5);
                //brew.rebuffBlockTime = 0;
                //brew.rebuffBlockTimeFrames = 0;
                var brewCheck = brew.towerBehaviors[0].Cast<BerserkerBrewCheckModel>();
                brewCheck.maxCount = (int) (brewCheck.maxCount * 1.5);
            }

            var dip = model.GetDescendant<AddAcidicMixtureToProjectileModel>();
            if (dip != null)
            {
                dip.cap = (int) (dip.cap * 1.5);
                //brew.rebuffBlockTime = 0;
                //brew.rebuffBlockTimeFrames = 0;
                var dipCheck = dip.towerBehaviors[0].Cast<AcidicMixtureCheckModel>();
                dipCheck.maxCount = (int) (dipCheck.maxCount * 1.5);
            }
        }

        public static void AllPowerToThrusters(TowerModel model)
        {
            var heliMovementModel = model.GetDescendant<HeliMovementModel>();

            heliMovementModel.maxSpeed *= 5;
            heliMovementModel.brakeForce *= 5;
            heliMovementModel.movementForceStart *= 5;
            heliMovementModel.movementForceEnd *= 5;
            heliMovementModel.movementForceEndSquared =
                heliMovementModel.movementForceEnd * heliMovementModel.movementForceEnd;
            heliMovementModel.strafeDistance *= 5;
            heliMovementModel.strafeDistanceSquared =
                heliMovementModel.strafeDistance * heliMovementModel.strafeDistance;
        }

        public static void AttackAndSupport(TowerModel model)
        {
            if (model.GetBehavior<SubmergeModel>() == null) return;

            model.targetTypes = Game.instance.model.GetTowerFromId(MonkeySub).targetTypes;

            var submergeEffect = model.GetBehavior<SubmergeEffectModel>().effectModel;
            var submerge = model.GetBehavior<SubmergeModel>();

            if (submerge.heroXpScale > 1.0)
            {
                model.AddBehavior(new HeroXpScaleSupportModel("HeroXpScaleSupportModel_", true, submerge.heroXpScale,
                    null));
            }

            if (submerge.abilityCooldownSpeedScale > 1.0)
            {
                model.AddBehavior(new AbilityCooldownScaleSupportModel("AbilityCooldownScaleSupportModel_",
                    true, submerge.abilityCooldownSpeedScale, true, false, null,
                    submerge.buffLocsName, submerge.buffIconName, false, submerge.supportMutatorPriority));
            }

            model.RemoveBehavior<SubmergeModel>();

            foreach (var attackModel in model.GetAttackModels())
            {
                if (attackModel.name.Contains("Submerge"))
                {
                    attackModel.name = attackModel.name.Replace("Submerged", "");
                    attackModel.weapons[0].GetBehavior<EjectEffectModel>().effectModel.assetId =
                        submerge.attackDisplayPath;
                }

                attackModel.RemoveBehavior<SubmergedTargetModel>();
            }

            model.AddBehavior(new CreateEffectAfterTimeModel("CreateEffectAfterTimeModel_", submergeEffect, 0f, true));
        }


        public static void ShadowDouble(TowerModel model)
        {
            var attackModel = model.GetAttackModel();
            var weapon = attackModel.weapons[0];
            var newWeapon = weapon.Duplicate();
            newWeapon.projectile.display = ModContent.GetDisplayGUID<ShadowShuriken>();
            weapon.AddBehavior(new FireAlternateWeaponModel("FireAlternateWeaponModel_", 1));

            newWeapon.AddBehavior(new FireWhenAlternateWeaponIsReadyModel("FireWhenAlternateWeaponIsReadyModel_", 1));
            newWeapon.AddBehavior(new FilterTargetAngleFilterModel("FilterTargetAngleFilterModel_", 45.0f, 180f, true,
                56));

            var arcEmissionModel = newWeapon.emission.TryCast<ArcEmissionModel>();
            if (arcEmissionModel != null)
            {
                newWeapon.emission.AddBehavior(
                    new EmissionArcRotationOffTowerDirectionModel("EmissionArcRotationOffTowerDirectionModel_", 180));
            }
            else
            {
                newWeapon.emission.AddBehavior(
                    new EmissionRotationOffTowerDirectionModel("EmissionRotationOffTowerDirectionModel_", 180));
            }

            newWeapon.name += " Secondary";
            newWeapon.ejectX *= -1;

            var trackTargetWithinTimeModel = newWeapon.projectile.GetBehavior<TrackTargetWithinTimeModel>();
            if (trackTargetWithinTimeModel != null)
            {
                trackTargetWithinTimeModel.name += "Behind";
            }


            attackModel.AddWeapon(newWeapon);
        }

        public static void AceHardware(TowerModel model)
        {
            var towerModel = Game.instance.model.GetTower(MonkeyAce, 0, 0, 4);
            var attack = towerModel.GetAttackModels()[1].Duplicate();
            var weapon = attack.weapons[0];
            weapon.RemoveBehavior<AlternateProjectileModel>();
            attack.range = 60 + 20 * model.tier;
            weapon.Rate = .6f - .1f * model.tier;
            weapon.projectile.GetDamageModel().damage = 1 + model.tier / 2;
            weapon.projectile.pierce = model.GetWeapon().projectile.pierce;
            if (model.appliedUpgrades.Contains("Spy Plane"))
            {
                weapon.projectile.filters.GetItemOfType<FilterModel, FilterInvisibleModel>().isActive = false;
            }

            model.AddBehavior(attack);
        }

        public static void GorillaGlue(TowerModel model)
        {
            foreach (var projectileModel in model.GetDescendants<ProjectileModel>().ToList())
            {
                var amount = Math.Max(1, model.tier);
                if (model.tier == 4)
                {
                    amount++;
                }

                if (model.tier == 5)
                {
                    amount += 5;
                }

                var damageModel = projectileModel.GetDamageModel();
                if (damageModel == null)
                {
                    damageModel = new DamageModel("DamageModel_", amount, 0f,
                        true, false, true, BloonProperties.None);
                    projectileModel.AddBehavior(damageModel);
                }
                else
                {
                    damageModel.damage += amount;
                }

                if (model.appliedUpgrades.Contains("MOAB Glue"))
                {
                    var damageModifierForTagModel =
                        new DamageModifierForTagModel("DamageModifierForTagModel_", "Moabs", 1.0f, amount * 9, false,
                            true);
                    projectileModel.AddBehavior(damageModifierForTagModel);

                    projectileModel.hasDamageModifiers = true;
                }
            }
        }

        public static void BombVoyage(TowerModel model)
        {
            foreach (var projectileModel in model.GetDescendants<ProjectileModel>().ToList())
            {
                var damageModel = projectileModel.GetBehavior<DamageModel>();
                if (damageModel != null)
                {
                    damageModel.immuneBloonProperties = BloonProperties.None;
                }

                var travelStraitModel = projectileModel.GetBehavior<TravelStraitModel>();
                var createProjectileOnContactModel = projectileModel.GetBehavior<CreateProjectileOnContactModel>();
                if (travelStraitModel != null && createProjectileOnContactModel != null)
                {
                    travelStraitModel.Speed *= 2;
                }
            }
        }
    }
}