using BepInEx.Configuration;
using R2API;
using RoR2;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
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

        public override CombatDirector.EliteTierDef[] CanAppearInEliteTiers => EliteAPI.GetCombatDirectorEliteTiers().Where(x => x.eliteTypes.Contains(Addressables.LoadAssetAsync<EliteDef>("RoR2/Base/EliteFire/edFire.asset").WaitForCompletion())).ToArray();

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
                    float force = -4000f * damageInfo.procCoefficient;
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