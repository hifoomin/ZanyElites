using R2API;
using UnityEngine;

namespace ZanyElites.VFX
{
    internal class WhimsicalOnHit : EffectBase
    {
        public override string Key => "RoR2/Junk/Treebot/TreebotShockwavePullEffect.prefab";

        public override string NewName => "WhimsicalOnHitVFX";

        public static GameObject VFX;

        public override void Init()
        {
            base.Init();
            var transform = ClonedObject.transform;
            var pollenSingle = transform.GetChild(1);
            var pollenDust = transform.GetChild(2);
            var pollenRadial = transform.GetChild(3);
            var pollenSingle2 = transform.GetChild(4);
            var distortionWave2 = transform.GetChild(7).GetComponent<ParticleSystem>().main.startColor;
            pollenSingle.gameObject.SetActive(false);
            pollenDust.gameObject.SetActive(false);
            pollenRadial.gameObject.SetActive(false);
            pollenSingle2.gameObject.SetActive(false);
            distortionWave2.color = new Color32(208, 157, 183, 255);

            ContentAddition.AddEffect(ClonedObject);

            VFX = ClonedObject;
        }
    }
}