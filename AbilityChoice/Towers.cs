using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Models;
using Assets.Scripts.Models.Effects;
using Assets.Scripts.Models.GenericBehaviors;
using Assets.Scripts.Models.Towers;
using Assets.Scripts.Models.Towers.Behaviors;
using Assets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors;
using Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors;
using Assets.Scripts.Models.Towers.Behaviors.Emissions;
using Assets.Scripts.Models.Towers.Projectiles.Behaviors;
using Assets.Scripts.Models.Towers.Projectiles;
using Assets.Scripts.Models.Towers.TowerFilters;
using Assets.Scripts.Models.Towers.Weapons;
using Assets.Scripts.Models.Towers.Weapons.Behaviors;
using Assets.Scripts.Unity;
using Assets.Scripts.Unity.UI_New.InGame;
using Assets.Scripts.Utils;
using BTD_Mod_Helper.Extensions;
using UnhollowerBaseLib;
using CreateEffectOnExpireModel = Assets.Scripts.Models.Towers.Projectiles.Behaviors.CreateEffectOnExpireModel;
using static Assets.Scripts.Models.Towers.TowerType;

namespace AbilityChoice
{
    public class Towers
    {
        #region Primary
        
        public static void SuperMonkeyFanClub(TowerModel model)
        {
            model.GetWeapon().rate *= 0.06f / .475f;
            model.range += 20;
            model.GetAttackModels()[0].range += 20;
            
            foreach (var projectileModel in model.GetAllProjectiles())
            {
                if (projectileModel.display == null)
                {
                    continue;
                }
                projectileModel.GetBehavior<TravelStraitModel>().lifespan *= 2f;
                projectileModel.GetBehavior<TravelStraitModel>().lifespanFrames *= 2;
            }
        }

        public static void PlasmaMonkeyFanClub(TowerModel model)
        {
            model.GetWeapon().rate *= 0.03f / .475f;
            model.range += 20;
            model.GetAttackModels()[0].range += 20;

            ProjectileModel plasmaModel =
                Game.instance.model.GetTower(SuperMonkey, 2, 0, 0).GetWeapon().projectile;

            foreach (var weaponProjectile in model.GetAllProjectiles())
            {
                if (weaponProjectile.display == null)
                {
                    continue;
                }
                weaponProjectile.display = plasmaModel.display;
                weaponProjectile.GetBehavior<DisplayModel>().display = plasmaModel.display;
                weaponProjectile.GetDamageModel().damage += 2;
                weaponProjectile.GetDamageModel().immuneBloonProperties = BloonProperties.Purple;
                weaponProjectile.pierce += 5;
                
                
                weaponProjectile.GetBehavior<TravelStraitModel>().lifespan *= 2f;
                weaponProjectile.GetBehavior<TravelStraitModel>().lifespanFrames *= 2;
            }
        }

        public static void TurboCharge(TowerModel model)
        {
            var mK = model.GetAbility().GetBehavior<TurboModel>().lifespanFrames > 600;
            if (mK)
            {
                model.GetWeapon().rate *= .07f / .8f;
            }
            else
            {
                model.GetWeapon().rate *= .1f / .8f;
            }
        }

        public static void PermaCharge(TowerModel model)
        {
            model.GetWeapon().projectile.GetDamageModel().damage += 3;
        }

        public static void MOABAssassin(TowerModel model)
        {
            var realProjectile = model.GetWeapon().projectile.GetBehavior<CreateProjectileOnContactModel>()
                .projectile;
            realProjectile.GetBehaviors<DamageModifierForTagModel>().First(m => m.tag == "Moabs").damageAddative += 18;
        }

        public static void MOABEliminator(TowerModel model)
        {
            var realProjectile = model.GetWeapon().projectile.GetBehavior<CreateProjectileOnContactModel>()
                .projectile;
            realProjectile.GetBehaviors<DamageModifierForTagModel>().First(m => m.tag == "Moabs").damageAddative += 99;
        }

        public static void BombBlitz(TowerModel model)
        {
            foreach (var projectileModel in model.GetAllProjectiles())
            {
                if (projectileModel.GetDamageModel() != null)
                {
                    projectileModel.GetDamageModel().damage += 3;
                }
            }
            
        }

        public static void BladeMaelstrom(TowerModel model)
        {
            model.range += 9;

            var neva = Game.instance.model.GetTower(MonkeyAce, 0, 0, 3);
            var behavior = neva.GetAllProjectiles()[0].GetBehavior<TrackTargetModel>().Duplicate();

            behavior.TurnRate *= 3;
            behavior.constantlyAquireNewTarget = true;
            behavior.useLifetimeAsDistance = true;
            
            var weaponProjectile = model.GetWeapon().projectile;
            weaponProjectile.AddBehavior(behavior);
            weaponProjectile.pierce += 6;
            weaponProjectile.GetBehavior<TravelStraitModel>().lifespanFrames *= 4;
            weaponProjectile.GetBehavior<TravelStraitModel>().lifespan *= 4f;
        }

        public static void SuperMaelstrom(TowerModel model)
        {
            model.range += 20;

            var neva = Game.instance.model.GetTower(MonkeyAce, 0, 0, 3);
            var behavior = neva.GetAllProjectiles()[0].GetBehavior<TrackTargetModel>().Duplicate();

            behavior.TurnRate *= 3;
            behavior.constantlyAquireNewTarget = true;
            behavior.useLifetimeAsDistance = true;
            
            var weaponProjectile = model.GetWeapon().projectile;
            weaponProjectile.AddBehavior(behavior);
            weaponProjectile.pierce += 14;
            weaponProjectile.GetBehavior<TravelStraitModel>().lifespanFrames *= 16;
            weaponProjectile.GetBehavior<TravelStraitModel>().lifespan *= 16f;
        }

        public static void Snowstorm(TowerModel model)
        {
            var realSlow = model.GetBehavior<SlowBloonsZoneModel>();
            
            var totem = Game.instance.model.GetTowerFromId("NaturesWardTotem");
            
            var slow = totem.GetBehaviors<SlowBloonsZoneModel>().First(b => !b.name.Contains("NonMoabs")).Duplicate();
            slow.zoneRadius = realSlow.zoneRadius;
            slow.bindRadiusToTowerRange = true;
            slow.radiusOffset = realSlow.radiusOffset;
            
            model.AddBehavior(slow);
        }

        public static void AbsoluteZero(TowerModel model)
        {
            var realSlow = model.GetBehavior<SlowBloonsZoneModel>();
            
            var totem = Game.instance.model.GetTowerFromId("NaturesWardTotem");
            
            var slow = totem.GetBehaviors<SlowBloonsZoneModel>().First(b => !b.name.Contains("NonMoabs")).Duplicate();
            slow.zoneRadius = realSlow.zoneRadius;
            slow.bindRadiusToTowerRange = true;
            slow.radiusOffset = realSlow.radiusOffset;
            slow.mutator.Cast<SlowBloonsZoneModel.Mutator>().speedScale -= .1f;
            
            
            model.AddBehavior(slow);

            var buff = new RateSupportModel("RateSupportZoneModel_AbilityChoice", 2f / 3f, true, "AbsoluteZeroRateBuff2", false, 1,
                new Il2CppReferenceArray<TowerFilterModel>(
                    new TowerFilterModel[]
                    {
                        new FilterInBaseTowerIdModel("FilterInBaseTowerIdModel_",
                            new Il2CppStringArray(new []{IceMonkey}))
                    }
                )
            , "", "");
            buff.showBuffIcon = false;
            buff.appliesToOwningTower = true;
            model.AddBehavior(buff);
        }

        public static void GlueStrike(TowerModel model)
        {
            var realWeapon = model.GetWeapon();
            var ability = model.GetAbilites()[0];
            var abilityAttack = ability.GetBehavior<ActivateAttackModel>().attacks[0].Duplicate();
            var abilityWeapon = abilityAttack.weapons[0];

            var behaviors = abilityWeapon.projectile.GetBehaviors<AddBehaviorToBloonModel>();
            var damageBoost = behaviors[behaviors.Count - 1];
            var abilitySlow = abilityWeapon.projectile.GetBehavior<SlowModel>();
            var realSlow = realWeapon.projectile.GetBehavior<SlowModel>();
            var realProjectile2 = realWeapon.projectile.GetBehavior<CreateProjectileOnContactModel>().projectile;
            var realSlow2 = realProjectile2.GetBehavior<SlowModel>();

            realSlow.lifespan = abilitySlow.lifespan;
            realSlow.layers = abilitySlow.layers;
            realSlow.mutator.multiplier = abilitySlow.Multiplier;
            realSlow2.lifespan = abilitySlow.lifespan;
            realSlow2.layers = abilitySlow.layers;
            realSlow2.mutator.multiplier = abilitySlow.Multiplier;

            realWeapon.projectile.AddBehavior(damageBoost);
            realProjectile2.AddBehavior(damageBoost);
        }
        
        public static void GlueStorm(TowerModel model)
        {
            GlueStrike(model);
            model.range *= 2;
            model.GetAttackModels()[0].range *= 2;
            model.GetWeapon().rate /= 2f;
        }
        #endregion

        #region Military
        
        public static void FirstStrikeCapability(TowerModel model)
        {
            var abilityAttack = model.GetAbilites()[0].GetBehavior<ActivateAttackModel>().attacks[0].Duplicate();
            var abilityWeapon = abilityAttack.weapons[0];

            abilityWeapon.rate = model.GetAbilites()[0].cooldown / 50f;

            foreach (var createProjectileOnExpireModel in abilityWeapon.projectile.GetBehaviors<CreateProjectileOnExpireModel>())
            {
                createProjectileOnExpireModel.projectile.GetDamageModel().damage /= 50;
                if (createProjectileOnExpireModel.projectile.radius > 10)
                {
                    createProjectileOnExpireModel.projectile.radius /= 2f;
                }
            }

            var asset = abilityWeapon.projectile.GetBehavior<CreateEffectOnExpireModel>();
            asset.assetId = "";
            asset.effectModel = new EffectModel(asset.name, asset.assetId, .5f, asset.lifespan, false ,
                false, false, false, false, false, false);
            
            model.AddBehavior(abilityAttack);
        }
        
        public static void PreemptiveStrike(TowerModel model)
        {
            FirstStrikeCapability(model);
        }
        
        
        public static void BuccaneerMonkeyPirates(TowerModel model)
        {
            foreach (var projectileModel in model.GetAllProjectiles())
            {
                if (projectileModel.id == "Explosion")
                {
                    projectileModel.AddBehavior(new DamageModifierForTagModel("MoabDamage", "Moabs", 1.0f, 20, false, false));
                    projectileModel.AddBehavior(new DamageModifierForTagModel("MoabDamage", "Ceramic", 1.0f, 10, false, false));
                }
            }
        }

        public static void BuccaneerPirateLord(TowerModel model)
        {
            foreach (var projectileModel in model.GetAllProjectiles())
            {
                if (projectileModel.GetDamageModel() != null)
                {
                    projectileModel.AddBehavior(new DamageModifierForTagModel("MoabDamage", "Moabs", 1.0f, 10, false, false));
                    projectileModel.AddBehavior(new DamageModifierForTagModel("MoabDamage", "Ceramic", 1.0f, 10, false, false));
                }
                if (projectileModel.id == "Explosion")
                {
                    foreach (var damageModifierForTagModel in projectileModel.GetBehaviors<DamageModifierForTagModel>())
                    {
                        damageModifierForTagModel.damageAddative += 20;
                    }
                }
            }
        }

        public static void RocketStorm(TowerModel model)
        {
            var abilityAttack = model.GetAbilites()[0].GetBehavior<ActivateAttackModel>().attacks[0].Duplicate();

            var abilityWeapon = abilityAttack.weapons[0];
            var realWeapon = model.GetWeapon();
            abilityWeapon.emission = realWeapon.emission;
            abilityWeapon.GetBehavior<EjectEffectModel>().effectModel.lifespan = .05f;
            abilityWeapon.rate /= 4;


            if (abilityWeapon.projectile.GetBehavior<CreateProjectileOnContactModel>().projectile.HasBehavior<SlowModel>())
            {
                abilityWeapon.projectile.GetBehavior<CreateProjectileOnContactModel>().projectile.GetBehavior<SlowModel>().lifespan /= 3;
                abilityWeapon.projectile.GetBehavior<CreateProjectileOnContactModel>().projectile.GetBehavior<SlowModel>()
                    .dontRefreshDuration = true;
                abilityWeapon.projectile.GetBehavior<CreateProjectileOnBlockerCollideModel>().projectile.GetBehavior<SlowModel>().lifespan /= 3;
                abilityWeapon.projectile.GetBehavior<CreateProjectileOnBlockerCollideModel>().projectile.GetBehavior<SlowModel>()
                    .dontRefreshDuration = true;
            }
            
            model.GetAttackModels()[0].AddWeapon(abilityWeapon);
        }
        
        public static void MAD(TowerModel model)
        {
            RocketStorm(model);
        }


        public static void PopandAwe(TowerModel model)
        {
            var realWeapon = model.GetWeapon();
            var ability = model.GetAbilites()[0];
            var abilityAttack = ability.GetBehavior<ActivateAttackModel>().attacks[0].Duplicate();
            var abilityWeapon = abilityAttack.weapons[0];

            var popAndEffect = ability.GetBehavior<CreateEffectOnAbilityModel>().effectModel.Duplicate();
            popAndEffect.lifespan /= 8f;
            
            var newWeapon = realWeapon.Duplicate();
            var weaponEffect = newWeapon.projectile.GetBehavior<CreateEffectOnExpireModel>();
            weaponEffect.assetId = "";
            weaponEffect.effectModel = popAndEffect;
            weaponEffect.effectModel.scale /= 3f;
            weaponEffect.effectModel.useCenterPosition = false;
            weaponEffect.effectModel.lifespan /= 2f;
            newWeapon.rate = 4f;
            
            var newProjectile = abilityWeapon.projectile;
            newProjectile.GetBehavior<AgeModel>().lifespanFrames = 1;
            newProjectile.radius = realWeapon.projectile.radius * 2;

            newProjectile.behaviors = newProjectile.behaviors.RemoveItemOfType<Model, ClearHitBloonsModel>();

            newWeapon.projectile.GetBehavior<CreateProjectileOnExhaustFractionModel>().projectile = newProjectile;
            newWeapon.projectile.behaviors = newWeapon.projectile.behaviors
                .RemoveItemOfType<Model, CreateEffectOnExhaustFractionModel>();

            var sound = Game.instance.model.GetTower(MortarMonkey, 5).GetWeapon().projectile
                .GetBehavior<CreateSoundOnProjectileExhaustModel>();
            newWeapon.projectile.behaviors = newWeapon.projectile.behaviors
                .RemoveItemOfType<Model, CreateSoundOnProjectileExhaustModel>();
            newWeapon.projectile.AddBehavior(sound);

            model.GetAttackModels()[0].AddWeapon(newWeapon);
        }


        public static void SupplyDrop(TowerModel model)
        {
            var ability = model.GetAbilites()[0];
            var behavior = new ActivateAbilityOnRoundStartModel("ActivateAbilityOnRoundStartModel_SupplyDrop", ability.Duplicate());
            ability.enabled = false;
            model.AddBehavior(behavior);
        }

        public static void EliteSniper(TowerModel model)
        {
            SupplyDrop(model);
        }
        
        public static void SupportChinook(TowerModel model)
        {
            var ability = model.GetAbilites()[1];
            var behavior = new ActivateAbilityOnRoundStartModel("ActivateAbilityOnRoundStartModel_SupportChinook", ability.Duplicate());
            ability.enabled = false;
            model.AddBehavior(behavior);
        }
        
        public static void SpecialPoperations(TowerModel model)
        {
            var ability = model.GetAbilites()[1];
            var behavior = new ActivateAbilityOnRoundStartModel("ActivateAbilityOnRoundStartModel_SpecialPoperations", ability.Duplicate());
            ability.enabled = false;
            model.AddBehavior(behavior);

            var specialPops = model.GetAbilites()[2];
            model.behaviors = model.behaviors.RemoveItem(specialPops);

            var marine = specialPops.GetBehavior<FindDeploymentLocationModel>().towerModel;

            var weapon = marine.GetAttackModels()[0].weapons[0].Duplicate();

            var airBehavior = model.GetAttackModels()[0].weapons[0].GetBehavior<FireFromAirUnitModel>();
            weapon.behaviors = new Il2CppReferenceArray<WeaponBehaviorModel>(new WeaponBehaviorModel[] {airBehavior});

            weapon.ejectX = weapon.ejectY = weapon.ejectZ = 0;

            weapon.emission = model.GetWeapon().emission.Duplicate();
            weapon.emission.Cast<EmissionWithOffsetsModel>().throwMarkerOffsetModels =
                new Il2CppReferenceArray<ThrowMarkerOffsetModel>(new[]
                {
                    weapon.emission.Cast<EmissionWithOffsetsModel>().throwMarkerOffsetModels[0]
                });
            weapon.emission.Cast<EmissionWithOffsetsModel>().throwMarkerOffsetModels[0].ejectX = 0;
            weapon.emission.Cast<EmissionWithOffsetsModel>().projectileCount = 1;

            model.GetAttackModels()[0].AddWeapon(weapon);
        }
        
        public static void GroundZero(TowerModel model)
        {
            var ability = model.GetAbilites()[0];
            var abilityAttack = ability.GetBehavior<ActivateAttackModel>().attacks[0].Duplicate();
            var abilityWeapon = abilityAttack.weapons[0];
            
            var effectModel = ability.GetBehavior<CreateEffectOnAbilityModel>().effectModel.Duplicate();
            effectModel.scale = .5f;
            effectModel.useCenterPosition = false;

            var effectBehavior =
                new CreateEffectOnExhaustFractionModel("CreateEffectOnExhaustFractionModel_GroundZero", "", effectModel, 0, false, 1.0f, -1f, false);
            abilityWeapon.projectile.AddBehavior(effectBehavior);
            abilityWeapon.rate = 6;
            abilityWeapon.projectile.GetDamageModel().damage = 100;
            abilityWeapon.projectile.radius = 100;
            
            var airBehavior = model.GetAttackModels()[0].weapons[0].GetBehavior<FireFromAirUnitModel>();
            abilityWeapon.behaviors = new Il2CppReferenceArray<WeaponBehaviorModel>(new WeaponBehaviorModel[] {airBehavior});

            var sound = ability.GetBehavior<CreateSoundOnAbilityModel>().sound;
            var soundBehavior =
                new CreateSoundOnProjectileExhaustModel("CreateSoundOnProjectileExhaustModel_GroundZero", sound, sound, sound, sound, sound);
            
            abilityWeapon.projectile.AddBehavior(soundBehavior);
            
            model.AddBehavior(abilityAttack);
        }

        public static void TsarBomba(TowerModel model)
        {
            GroundZero(model);

            var attackModels = model.GetAttackModels();
            attackModels[attackModels.Count].weapons[0].projectile.GetDamageModel().damage = 400;
            attackModels[attackModels.Count].weapons[0].rate = 5;
            attackModels[attackModels.Count].weapons[0].projectile.GetBehavior<SlowModel>().lifespan /= 8;
        }
        #endregion


        #region Magic

        public static void SummonPhoenix(TowerModel model)
        {
            var lord = Game.instance.model.GetTower(WizardMonkey, model.tiers[0], 5, model.tiers[2]);

            var permaBehavior = lord.GetBehavior<TowerCreateTowerModel>().Duplicate();

            permaBehavior.towerModel.GetWeapon().rate *= 3;
            
            model.AddBehavior(permaBehavior);
        }

        public static void WizardLordPhoenix(TowerModel model)
        {
            var permaBehavior = model.GetBehavior<TowerCreateTowerModel>().Duplicate();
            var lordPhoenix = model.GetAbilites()[0].GetBehavior<AbilityCreateTowerModel>().towerModel.Duplicate();

            lordPhoenix.behaviors = lordPhoenix.behaviors.RemoveItemOfType<Model, TowerExpireModel>();
            foreach (var weaponModel in lordPhoenix.GetWeapons())
            {
                weaponModel.rate *= 3f;
            }

            permaBehavior.towerModel = lordPhoenix;
            
            model.AddBehavior(permaBehavior);
        }

        public static void TechTerror(TowerModel model)
        {
            var ability = model.GetAbilites()[0];
            var abilityAttack = ability.GetBehavior<ActivateAttackModel>().attacks[0].Duplicate();
            var abilityWeapon = abilityAttack.weapons[0];

            var effect = ability.GetBehavior<CreateEffectOnAbilityModel>().effectModel;
            abilityWeapon.projectile.display = effect.assetId;
            var effectBehavior =
                new CreateEffectOnExhaustFractionModel("CreateEffectOnExhaustFractionModel_Annihilation", "", effect, 0, false, 1.0f, -1f, false);
            abilityWeapon.projectile.AddBehavior(effectBehavior);
            /*
            var sound = ability.GetBehavior<CreateSoundOnAbilityModel>().sound;
            var soundBehavior =
                new CreateSoundOnProjectileExhaustModel("AbilityChoice", sound, sound, sound, sound, sound);
            abilityWeapon.projectile.AddBehavior(soundBehavior);
            */

            abilityWeapon.projectile.GetDamageModel().damage /= 20;
            abilityWeapon.rate = 2.25f;

            abilityAttack.range = abilityWeapon.projectile.radius - 10;
            abilityAttack.fireWithoutTarget = false;
            
            model.AddBehavior(abilityAttack);
        }

        public static void TheAntiBloon(TowerModel model)
        {
            TechTerror(model);
        }

        public static void DarkKnight(TowerModel model)
        {
            model.range += 10;
        }
        
        public static void DarkChampion(TowerModel model)
        {
            model.range += 20;
        }
        
        public static void LegendoftheNight(TowerModel model)
        {
            model.range += 30;
        }

        public static void BloonSabotage(TowerModel model)
        {
            model.range += 10;
            var ability = model.GetAbilites()[0];
            var abilityAttack = ability.GetBehavior<ActivateAttackModel>().attacks[0].Duplicate();
            var abilityWeapon = abilityAttack.weapons[0];
            var slowMutator = abilityWeapon.projectile.GetBehavior<SlowMinusAbilityDurationModel>().Mutator;

            var dontSlowBadBehavior = abilityWeapon.projectile.GetBehavior<SlowModifierForTagModel>();

            var slowBehavior = new SlowModel("Sabotage", 0f, 2f, slowMutator.mutationId, "", 999,
                new Il2CppSystem.Collections.Generic.Dictionary<string, AssetPathModel>(), 0, true, false, null,
                false, false) {mutator = slowMutator};
            
            
            foreach (var weaponModel in model.GetWeapons())
            {
                weaponModel.projectile.AddBehavior(slowBehavior);
                weaponModel.projectile.AddBehavior(dontSlowBadBehavior);
                weaponModel.projectile.pierce += 5;

                weaponModel.projectile.GetDamageModel().immuneBloonProperties = BloonProperties.None;
            }
        }
        
        public static void GrandSaboteur(TowerModel model)
        {
            BloonSabotage(model);
            model.range += 10;

            List<string[]> tags = new List<string[]>
            {
                new []{"Moab"},
                new []{"Bfb"},
                new []{"Zomg"},
                new []{"Ddt"},
                new []{"Bad"},
            };
            
            foreach (var weaponModel in model.GetWeapons())
            {
                for (var i = 0; i < tags.Count; i++)
                {
                    var t = tags[i];
                    var behavior = new DamageModifierForTagModel("DamageModifierForTagModel_" + i, t[0], 1.0f, 
                        10 * (i + 1), false, false) {tags = t};
                    weaponModel.projectile.AddBehavior(behavior);
                    weaponModel.projectile.pierce += 10;

                    weaponModel.projectile.GetDamageModel().immuneBloonProperties = BloonProperties.None;
                }
            }
        }


        public static void JunglesBounty(TowerModel model)
        {
            var village = Game.instance.model.GetTower(MonkeyVillage, 0, 0, 4);
            var behavior = village.GetBehavior<MonkeyCityIncomeSupportModel>().Duplicate();
            behavior.incomeModifier = 1.15f;
            behavior.appliesToOwningTower = false;
            behavior.isUnique = false;
            model.AddBehavior(behavior);

            var boat = Game.instance.model.GetTower(MonkeyBuccaneer, 0, 0, 3);
            var cash = boat.GetBehavior<PerRoundCashBonusTowerModel>().Duplicate();
            model.AddBehavior(cash);
        }

        public static void SpiritoftheForest(TowerModel model)
        {
            JunglesBounty(model);
            var lives = new BonusLivesPerRoundModel("BonusLivesPerRoundModel", 25, 1f, "9bef6b3a7356f834eb953cc79622cdef");
            model.AddBehavior(lives);
        }

        public static void TransformingTonic(TowerModel model)
        {
            model.range += 9;
            var ability = model.GetAbilites()[0];
            var abilityAttack = ability.GetBehavior<ActivateAttackModel>().attacks[0].Duplicate();
            var abilityWeapon = abilityAttack.weapons[0];

            abilityAttack.range = model.range;
            abilityWeapon.rate *= 3;
            
            model.AddBehavior(abilityAttack);
        }

        public static void TotalTransformation(TowerModel model)
        {
            model.range += 27;
            var ability = model.GetAbilites()[0];
            var abilityAttack = ability.GetBehavior<ActivateAttackModel>().attacks[0].Duplicate();
            var abilityWeapon = abilityAttack.weapons[0];

            abilityAttack.range = model.range;
            abilityWeapon.rate /= 2;
            abilityWeapon.projectile.pierce += 10;
            abilityWeapon.projectile.GetDamageModel().damage += 1;
            
            model.AddBehavior(abilityAttack);
        }
        
        #endregion

        #region Support

        public static void IMFloan(TowerModel model)
        {
            model.GetBehavior<BankModel>().capacity += 7500;
        }
        
        public static void MonkeyNomics(TowerModel model)
        {
            model.GetBehavior<BankModel>().capacity = 1000000000;
        }
        
        public static void CalltoArms(TowerModel model)
        {
            var ability = model.GetAbilites()[0];
            var c2a = ability.GetBehavior<CallToArmsModel>();
            var buffIndicator = c2a.Mutator.buffIndicator;

            var buff = new RateSupportModel("RateSupportModel_CallToArms", .8f, true, "Village:CallToArms", false, 1,
                new Il2CppReferenceArray<TowerFilterModel>(0), buffIndicator.buffName, buffIndicator.iconName)
            {
                onlyShowBuffIfMutated = true, 
                isUnique = true
            };


            model.AddBehavior(buff);
        }
        
        public static void HomelandDefense(TowerModel model)
        {
            var ability = model.GetAbilites()[0];
            var c2a = ability.GetBehavior<CallToArmsModel>();
            var buffIndicator = c2a.Mutator.buffIndicator;

            var buff = new RateSupportModel("RateSupportModel_HomelandDefense", .5f, true, "Village:HomelandDefense",
                true, 1,
                new Il2CppReferenceArray<TowerFilterModel>(0), buffIndicator.buffName, buffIndicator.iconName)
            {
                onlyShowBuffIfMutated = true, 
                isUnique = true
            };

            model.AddBehavior(buff);
        }

        public static void Overclock(TowerModel model)
        {
            //see Overclock.cs
        }
        
        public static void Ultraboost(TowerModel model)
        {
            //see Overclock.cs
        }
        
        public static void SpikeStorm(TowerModel model)
        {
            var ability = model.GetAbilites()[0];

            var abilityAttack = ability.GetBehavior<ActivateAttackModel>().attacks[0].Duplicate();
            var abilityWeapon = abilityAttack.weapons[0];
            abilityWeapon.fireBetweenRounds = false;
            abilityWeapon.rate *= 10;
            model.AddBehavior(abilityAttack);
            
        }
        
        public static void CarpetofSpikes(TowerModel model)
        {
            SpikeStorm(model);
            model.GetAbilites()[0].enabled = false;
        }

        #endregion
    }
}