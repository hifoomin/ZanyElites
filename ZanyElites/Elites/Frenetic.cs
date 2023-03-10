using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using ZanyElites.VFX;

namespace ZanyElites.Elites
{
    internal class Frenetic : EliteEquipmentBase<Frenetic>
    {
        public override string EliteEquipmentName => "Their Outrage";

        public override string EliteAffixToken => "FRENETIC";

        public override string EliteEquipmentPickupDesc => "Become an aspect of madness.";

        public override string EliteEquipmentFullDescription => "Become an aspect of madness.";

        public override string EliteEquipmentLore => "";

        public override string EliteModifier => "Frenetic";

        public override GameObject EliteEquipmentModel => CreateAffixModel(new Color32(185, 154, 134, 255));

        public override Sprite EliteEquipmentIcon => Main.zanyelites.LoadAsset<Sprite>("Assets/ZanyElites/texAffixFrenetic.png");

        public override Sprite EliteBuffIcon => null;

        public override Texture2D EliteRampTexture => Main.zanyelites.LoadAsset<Texture2D>("Assets/ZanyElites/texFreneticRamp.png");

        public override float DamageMultiplier => 1.25f;
        public override float HealthMultiplier => 4.5f;

        public static BuffDef FreneticOnHitBuff;

        public override CombatDirector.EliteTierDef[] CanAppearInEliteTiers => EliteAPI.GetCombatDirectorEliteTiers().Where(x => x.eliteTypes.Contains(Addressables.LoadAssetAsync<EliteDef>("RoR2/Base/EliteFire/edFire.asset").WaitForCompletion())).ToArray();

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateEquipment();
            CreateEliteTiers();
            CreateElite();

            FreneticOnHitBuff = ScriptableObject.CreateInstance<BuffDef>();

            FreneticOnHitBuff.name = "FreneticOnHit";
            FreneticOnHitBuff.isHidden = true;
            FreneticOnHitBuff.canStack = true;
            FreneticOnHitBuff.isDebuff = false;

            ContentAddition.AddBuffDef(FreneticOnHitBuff);

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
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender)
            {
                args.moveSpeedMultAdd += sender.GetBuffCount(FreneticOnHitBuff) * 0.1f;
                args.attackSpeedMultAdd += sender.GetBuffCount(FreneticOnHitBuff) * 0.1f;
                args.cooldownMultAdd -= sender.GetBuffCount(FreneticOnHitBuff) * 0.1f;
            }
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            var attacker = damageInfo.attacker;
            if (attacker)
            {
                var body = attacker.GetComponent<CharacterBody>();
                if (body && body.equipmentSlot && body.HasBuff(EliteBuffDef))
                {
                    body.AddTimedBuff(FreneticOnHitBuff, 5f, 3);
                    var effect = FreneticOnHit.VFX;
                    EffectManager.SpawnEffect(effect, new EffectData { _origin = body.corePosition, rotation = Util.QuaternionSafeLookRotation(body.equipmentSlot.GetAimRay().direction) }, true);
                }
            }
            orig(self, damageInfo, victim);
        }

        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            if (self.HasBuff(EliteBuffDef) && self.equipmentSlot)
            {
                self.attackSpeed += (1 - self.healthComponent.combinedHealthFraction) * 0.4f;
                self.moveSpeed += (1 - self.healthComponent.combinedHealthFraction) * 0.4f;
                var sl = self.skillLocator;
                if (sl)
                {
                    if (sl.primary) sl.primary.cooldownScale *= 1 - ((1 - self.healthComponent.combinedHealthFraction) * 0.4f);

                    if (sl.secondary) sl.secondary.cooldownScale *= 1 - ((1 - self.healthComponent.combinedHealthFraction) * 0.4f);

                    if (sl.utility) sl.utility.cooldownScale *= 1 - ((1 - self.healthComponent.combinedHealthFraction) * 0.4f);

                    if (sl.special) sl.special.cooldownScale *= 1 - ((1 - self.healthComponent.combinedHealthFraction) * 0.4f);
                }
            }
        }

        //If you want an on use effect, implement it here as you would with a normal equipment.
        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            return false;
        }
    }
}