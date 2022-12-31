using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using UnityEngine;
using ZanyElites.VFX;

namespace ZanyElites.Elites
{
    internal class Whimsical : EliteEquipmentBase<Whimsical>
    {
        public override string EliteEquipmentName => "His Antipathy";

        public override string EliteAffixToken => "AFFIX_WHIMSICAL";

        public override string EliteEquipmentPickupDesc => "Become an aspect of repulsion.";

        public override string EliteEquipmentFullDescription => "Become an aspect of repulsion.";

        public override string EliteEquipmentLore => "";

        public override string EliteModifier => "Whimsical";

        public override GameObject EliteEquipmentModel => CreateAffixModel(new Color32(134, 98, 125, 255));

        public override Sprite EliteEquipmentIcon => Main.zanyelites.LoadAsset<Sprite>("Assets/ZanyElites/texAffixWhimsical.png");

        public override Sprite EliteBuffIcon => null;

        public override Texture2D EliteRampTexture => Main.zanyelites.LoadAsset<Texture2D>("Assets/ZanyElites/texWhimsicalRamp.png");

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
            //For this, if you want to create your own elite tier def to place your elite, you can do it here.
            //Otherwise, don't set CanAppearInEliteTiers and it will appear in the first vanilla tier.

            //In this we create our own tier which we'll put our elites in. It has:
            //- 6 times the base elite cost.
            //- It can only become available to spawn after the player has looped at least once.

            CanAppearInEliteTiers = new CombatDirector.EliteTierDef[]
            {
                new CombatDirector.EliteTierDef()
                {
                    costMultiplier = CombatDirector.baseEliteCostMultiplier,
                    eliteTypes = Array.Empty<EliteDef>(),
                    isAvailable = SetAvailability
                }
           };

            //Additional note: since this accepts an array, it supports multiple elite tier defs, but do not put a cost of 0 on the cost multiplier.
        }

        private bool SetAvailability(SpawnCard.EliteRules arg)
        {
            return arg == SpawnCard.EliteRules.Default;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            On.RoR2.HealthComponent.TakeDamageForce_Vector3_bool_bool += HealthComponent_TakeDamageForce_Vector3_bool_bool;
            On.RoR2.HealthComponent.TakeDamageForce_DamageInfo_bool_bool += HealthComponent_TakeDamageForce_DamageInfo_bool_bool;
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (damageInfo.attacker)
            {
                var body = damageInfo.attacker.GetComponent<CharacterBody>();
                if (body && body.equipmentSlot && body.HasBuff(EliteBuffDef))
                {
                    float force = -3500f * damageInfo.procCoefficient;
                    damageInfo.force = Vector3.Scale(damageInfo.force, -Vector3.one);
                    damageInfo.force += body.equipmentSlot.GetAimRay().direction * force;
                    damageInfo.canRejectForce = false;
                    EffectManager.SpawnEffect(WhimsicalOnHit.VFX, new EffectData { _origin = damageInfo.position, rotation = Util.QuaternionSafeLookRotation(body.equipmentSlot.GetAimRay().direction) }, true);
                }
            }
            orig(self, damageInfo);
        }

        private void HealthComponent_TakeDamageForce_DamageInfo_bool_bool(On.RoR2.HealthComponent.orig_TakeDamageForce_DamageInfo_bool_bool orig, HealthComponent self, DamageInfo damageInfo, bool alwaysApply, bool disableAirControlUntilCollision)
        {
            if (self.body && self.body.HasBuff(EliteBuffDef))
            {
                damageInfo.force = Vector3.zero;
            }
            orig(self, damageInfo, alwaysApply, disableAirControlUntilCollision);
        }

        private void HealthComponent_TakeDamageForce_Vector3_bool_bool(On.RoR2.HealthComponent.orig_TakeDamageForce_Vector3_bool_bool orig, HealthComponent self, Vector3 force, bool alwaysApply, bool disableAirControlUntilCollision)
        {
            if (self.body && self.body.HasBuff(EliteBuffDef))
            {
                force = Vector3.zero;
            }
            orig(self, force, alwaysApply, disableAirControlUntilCollision);
        }

        //If you want an on use effect, implement it here as you would with a normal equipment.
        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            return false;
        }
    }
}