using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ZanyElites.VFX
{
    public class FreneticOnHit : EffectBase<FreneticOnHit>
    {
        public override string Key => "RoR2/Base/TeamWarCry/TeamWarCryAura.prefab";

        public override string NewName => "FreneticOnHitVFX";

        public static GameObject VFX;

        public override void Init()
        {
            base.Init();
            Object.Destroy(ClonedObject.GetComponent<AkGameObj>());
            Object.Destroy(ClonedObject.GetComponent<AkEvent>());

            ClonedObject.AddComponent<EffectComponent>();

            var transform = ClonedObject.transform;

            var soundwave = transform.GetChild(0).GetComponent<ParticleSystemRenderer>();
            soundwave.material.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Common/ColorRamps/texRampWarbanner2.png").WaitForCompletion());

            var distortion2 = transform.GetChild(2).GetComponent<ParticleSystem>().main.startColor;
            distortion2.color = new Color32(205, 130, 69, 255);

            var light = transform.GetChild(3).GetComponent<Light>();
            light.color = new Color32(215, 139, 49, 255);
            light.range = 7f;
            light.intensity = 13f;

            VFX = ClonedObject;
        }

        public override void Register()
        {
            base.Register();
        }
    }
}