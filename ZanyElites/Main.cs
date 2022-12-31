using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using R2API;
using R2API.ContentManagement;
using R2API.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using ZanyElites.VFX;

namespace ZanyElites
{
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInDependency(RecalculateStatsAPI.PluginGUID)]
    [BepInDependency(R2APIContentManager.PluginGUID)]
    [BepInDependency(EliteAPI.PluginGUID)]
    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInDependency(PrefabAPI.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;

        public const string PluginAuthor = "HIFU";
        public const string PluginName = "ZanyElites";
        public const string PluginVersion = "0.0.1";

        public static AssetBundle zanyelites;

        public static ConfigFile ZanyElitesConfig;
        public static ManualLogSource ZanyElitesLogger;

        public List<EliteEquipmentBase> EliteEquipments = new();

        public static Dictionary<EliteEquipmentBase, bool> EliteEquipmentStatusDictionary = new();

        public void Awake()
        {
            ZanyElitesLogger = Logger;
            ZanyElitesConfig = Config;

            zanyelites = AssetBundle.LoadFromFile(Assembly.GetExecutingAssembly().Location.Replace("ZanyElites.dll", "zanyelites"));

            var effectTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(EffectBase)));

            foreach (var effectType in effectTypes)
            {
                var effect = (EffectBase)Activator.CreateInstance(effectType);
                effect.Init();
            }

            var EliteEquipmentTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(EliteEquipmentBase)));

            foreach (var eliteEquipmentType in EliteEquipmentTypes)
            {
                var eliteEquipment = (EliteEquipmentBase)Activator.CreateInstance(eliteEquipmentType);
                if (ValidateEliteEquipment(eliteEquipment, EliteEquipments))
                {
                    eliteEquipment.Init(Config);
                }
            }

            ZanyElitesLogger.LogInfo("==+----------------==ELITES==----------------+==");

            On.RoR2.CombatDirector.Spawn += CombatDirector_Spawn;
        }

        private bool CombatDirector_Spawn(On.RoR2.CombatDirector.orig_Spawn orig, RoR2.CombatDirector self, RoR2.SpawnCard spawnCard, RoR2.EliteDef eliteDef, Transform spawnTarget, RoR2.DirectorCore.MonsterSpawnDistance spawnDistance, bool preventOverhead, float valueMultiplier, RoR2.DirectorPlacementRule.PlacementMode placementMode)
        {
            if (eliteDef == Elites.Frenetic.Instance.EliteDef || eliteDef == Elites.Whimsical.Instance.EliteDef)
            {
                ChatMessage.Send("A Zany Elite has appeared!");
            }
            ZanyElitesLogger.LogFatal("CombatDirector.baseEliteCostMultiplier is " + );
            return orig(self, spawnCard, eliteDef, spawnTarget, spawnDistance, preventOverhead, valueMultiplier, placementMode);
        }

        public bool ValidateEliteEquipment(EliteEquipmentBase eliteEquipment, List<EliteEquipmentBase> eliteEquipmentList)
        {
            var enabled = Config.Bind<bool>("Equipment: " + eliteEquipment.EliteEquipmentName, "Enable Elite Equipment?", true, "Should this elite equipment appear in runs? If disabled, the associated elite will not appear in runs either.").Value;

            if (enabled)
            {
                eliteEquipmentList.Add(eliteEquipment);
                return true;
            }
            return false;
        }
    }
}