using IL.RoR2.ContentManagement;
using R2API;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ZanyElites.VFX
{
    public abstract class EffectBase<T> : EffectBase where T : EffectBase<T>
    {
        public static T Instance { get; private set; }

        public EffectBase()
        {
            if (Instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting EffectBase was instantiated twice");
            Instance = this as T;
        }
    }

    public abstract class EffectBase
    {
        public abstract string Key { get; }
        public abstract string NewName { get; }
        protected GameObject ClonedObject;

        public virtual void Init()
        {
            Clone();
        }

        protected void Clone()
        {
            ClonedObject = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>(Key).WaitForCompletion(), NewName, false);
        }
    }
}