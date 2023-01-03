using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Projectile;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using ZanyElites.VFX;

namespace ZanyElites.Elites
{
    internal class Erroneous : EliteEquipmentBase<Whimsical>
    {
        public override string EliteEquipmentName => "The Fallacy";

        public override string EliteAffixToken => "ERRONEOUS";

        public override string EliteEquipmentPickupDesc => "Become an aspect of expansion.";

        public override string EliteEquipmentFullDescription => "Become an aspect of expansion.";

        public override string EliteEquipmentLore => "";

        public override string EliteModifier => "Erroneous";

        public override GameObject EliteEquipmentModel => CreateAffixModel(new Color32(134, 98, 125, 255));

        public override Sprite EliteEquipmentIcon => Main.zanyelites.LoadAsset<Sprite>("Assets/ZanyElites/texAffixErroneous.png");

        public override Sprite EliteBuffIcon => null;

        public override Texture2D EliteRampTexture => Main.zanyelites.LoadAsset<Texture2D>("Assets/ZanyElites/texErroneousRamp.png");

        public override CombatDirector.EliteTierDef[] CanAppearInEliteTiers => EliteAPI.GetCombatDirectorEliteTiers().Where(x => x.eliteTypes.Contains(Addressables.LoadAssetAsync<EliteDef>("RoR2/Base/EliteFire/edFire.asset").WaitForCompletion())).ToArray();

        public override float DamageMultiplier => 1.5f;

        public bool copySpikestripWhichCopiesAetherium;

        public static float speedMultiplier = 0.5f;

        public static float coefficientMultiplier = 0.5f;

        public static float sizeMultiplier = 0.75f;

        public static int projectileCount = 8;

        public bool shouldRun;

        public enum SplitType
        {
            None,
            Normal,
            Radius
        }

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateEquipment();
            CreateEliteTiers();
            CreateElite();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
        }

        private void CreateEliteTiers()
        {
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            On.RoR2.Projectile.ProjectileManager.FireProjectile_FireProjectileInfo += ProjectileManager_FireProjectile_FireProjectileInfo;
            On.RoR2.Projectile.ProjectileManager.InitializeProjectile += ProjectileManager_InitializeProjectile;
            On.RoR2.Projectile.ProjectileGhostController.Awake += ProjectileGhostController_Awake;
            // todo: copy spikestrip artifact of spread code
        }

        private void ProjectileGhostController_Awake(On.RoR2.Projectile.ProjectileGhostController.orig_Awake orig, ProjectileGhostController self)
        {
            if (shouldRun)
            {
                ParticleSystem[] componentsInChildren = self.GetComponentsInChildren<ParticleSystem>();
                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    var localScale = componentsInChildren[i].gameObject.transform.localScale;
                    componentsInChildren[i].gameObject.transform.localScale = new(localScale.x * sizeMultiplier, localScale.y * sizeMultiplier, localScale.z * sizeMultiplier);
                    var main = componentsInChildren[i].main;
                    main.scalingMode = ParticleSystemScalingMode.Local;
                }
            }

            orig(self);
            if (self.transform & shouldRun)
            {
                self.transform.localScale = new(self.transform.localScale.x * sizeMultiplier, self.transform.localScale.y * sizeMultiplier, self.transform.localScale.z * sizeMultiplier);
            }
        }

        private void ProjectileManager_InitializeProjectile(On.RoR2.Projectile.ProjectileManager.orig_InitializeProjectile orig, ProjectileController projectileController, FireProjectileInfo fireProjectileInfo)
        {
            orig(projectileController, fireProjectileInfo);
            var body = fireProjectileInfo.owner.gameObject.GetComponent<CharacterBody>();
            var splitType = CanProjectileSplit(fireProjectileInfo);
            var gameObject = projectileController.gameObject;
            if (body && body.HasBuff(EliteBuffDef) && gameObject && splitType > SplitType.None)
            {
                shouldRun = true;
                var transform = gameObject.transform;
                if (transform)
                {
                    transform.localScale = new(transform.localScale.x * sizeMultiplier, transform.localScale.y * sizeMultiplier, transform.localScale.z * sizeMultiplier);
                }

                var projectileDamage = gameObject.GetComponent<ProjectileDamage>();
                if (projectileDamage)
                {
                    projectileDamage.damage *= coefficientMultiplier;
                    projectileDamage.force *= coefficientMultiplier;
                }

                projectileController.procCoefficient *= coefficientMultiplier;

                var projectileSimple = gameObject.GetComponent<ProjectileSimple>();
                if (projectileSimple)
                {
                    projectileSimple.desiredForwardSpeed *= speedMultiplier;
                }

                var boomerangProjectile = gameObject.GetComponent<BoomerangProjectile>();
                if (boomerangProjectile)
                {
                    boomerangProjectile.travelSpeed *= speedMultiplier;
                }

                var missileController = gameObject.GetComponent<MissileController>();
                if (missileController)
                {
                    missileController.acceleration *= speedMultiplier;
                    missileController.maxVelocity *= speedMultiplier;
                    missileController.rollVelocity *= speedMultiplier;
                }

                foreach (ProjectileExplosion projectileExplosion in gameObject.GetComponents<ProjectileExplosion>())
                {
                    projectileExplosion.blastRadius *= sizeMultiplier;
                }
            }
            else
            {
                shouldRun = false;
            }
        }

        private void ProjectileManager_FireProjectile_FireProjectileInfo(On.RoR2.Projectile.ProjectileManager.orig_FireProjectile_FireProjectileInfo orig, ProjectileManager self, FireProjectileInfo fireProjectileInfo)
        {
            var body = fireProjectileInfo.owner.gameObject.GetComponent<CharacterBody>();
            if (!copySpikestripWhichCopiesAetherium)
            {
                if (body && body.HasBuff(EliteBuffDef))
                {
                    var splitType = CanProjectileSplit(fireProjectileInfo);
                    if (splitType > SplitType.None)
                    {
                        var forward = fireProjectileInfo.rotation * Vector3.forward;
                        var position = fireProjectileInfo.position;
                        var normalized = Vector3.ProjectOnPlane(Random.onUnitSphere, Vector3.up).normalized;
                        var num = 360f / projectileCount;
                        for (int i = 0; i < projectileCount; i++)
                        {
                            switch (splitType)
                            {
                                case SplitType.Normal:
                                    fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(Util.ApplySpread(forward, 0f, 15f, 1f, 0.5f, 0f, 0f));
                                    break;

                                case SplitType.Radius:
                                    var vector2 = Quaternion.AngleAxis(num * i, Vector3.up) * normalized;
                                    Ray ray = new(position, vector2);
                                    fireProjectileInfo.position = ray.GetPoint(5f);
                                    break;
                            }
                            try
                            {
                                copySpikestripWhichCopiesAetherium = true;
                                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                            }
                            finally { copySpikestripWhichCopiesAetherium = false; }
                        }
                    }
                }
            }
            else
            {
                shouldRun = false;
                orig(self, fireProjectileInfo);
            }
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (damageInfo.attacker)
            {
                var body = damageInfo.attacker.GetComponent<CharacterBody>();
                if (body && body.equipmentSlot && body.HasBuff(EliteBuffDef))
                {
                    // todo: spawn projectile on getting hit around itself
                    EffectManager.SpawnEffect(WhimsicalOnHit.VFX, new EffectData { _origin = damageInfo.position, rotation = Util.QuaternionSafeLookRotation(body.equipmentSlot.GetAimRay().direction) }, true);
                }
            }
            orig(self, damageInfo);
        }

        private SplitType CanProjectileSplit(FireProjectileInfo fireProjectileInfo)
        {
            SplitType splitType;
            if (fireProjectileInfo.projectilePrefab)
            {
                var projectileSimple = fireProjectileInfo.projectilePrefab.GetComponent<ProjectileSimple>();
                var boomerangProjectile = fireProjectileInfo.projectilePrefab.GetComponent<BoomerangProjectile>();

                var unmodifiedOrNotZeroInfo = fireProjectileInfo.useSpeedOverride && fireProjectileInfo.speedOverride != 0f;
                var unmodifiedOrNotZeroSimple = projectileSimple && !fireProjectileInfo.useSpeedOverride && projectileSimple.desiredForwardSpeed != 0f;
                var notZeroBoomerang = boomerangProjectile && boomerangProjectile.travelSpeed != 0f;
                var allCombined = unmodifiedOrNotZeroInfo || unmodifiedOrNotZeroSimple || notZeroBoomerang;
                if (allCombined)
                {
                    splitType = SplitType.Normal;
                }
                else
                {
                    splitType = SplitType.Radius;
                }
            }
            else
            {
                splitType = SplitType.None;
            }
            return splitType;
        }

        //If you want an on use effect, implement it here as you would with a normal equipment.
        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            return false;
        }
    }
}