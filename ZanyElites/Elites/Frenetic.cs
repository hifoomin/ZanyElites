using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ZanyElites.Elites
{
    internal class ExampleEliteEquipment : EliteEquipmentBase<ExampleEliteEquipment>
    {
        public override string EliteEquipmentName => "Their Outrage";

        public override string EliteAffixToken => "AFFIX_FRENETIC";

        public override string EliteEquipmentPickupDesc => "Become an aspect of madness.";

        public override string EliteEquipmentFullDescription => "Become an aspect of madness.";

        public override string EliteEquipmentLore => "";

        public override string EliteModifier => "Frenetic";

        public override GameObject EliteEquipmentModel => new GameObject();

        public override Sprite EliteEquipmentIcon => null;

        public override Sprite EliteBuffIcon => null;

        public override Texture2D EliteRampTexture => Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Bandit2/texBanditCoatDiffuse.png").WaitForCompletion();

        public override float DamageMultiplier => 1.75f;
        public override float HealthMultiplier => 5f;

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

            //Additional note: since this accepts an array, it supports multiple elite tier defs, but do not put a cost of 0 on the cost multiplier.
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender && sender.equipmentSlot && sender.HasBuff(EliteBuffDef))
            {
                args.baseAttackSpeedAdd += (1 - sender.healthComponent.combinedHealthFraction) * 0.5f;
                args.moveSpeedMultAdd += (1 - sender.healthComponent.combinedHealthFraction) * 0.5f;
            }
        }

        //If you want an on use effect, implement it here as you would with a normal equipment.
        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            return false;
        }
    }
}