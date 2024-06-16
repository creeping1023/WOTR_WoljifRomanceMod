using HarmonyLib;
using JetBrains.Annotations;
using Kingmaker;
using Kingmaker.Blueprints.JsonSystem;
using System;
using TabletopTweaks.Config;
using TabletopTweaks.Utilities;
using UnityModManagerNet;

namespace WOTR_WoljifRomanceMod
{
    static class Main
    //public class Main
    {
        public static bool Enabled;
        //public static AreaWatcher LoadWatcher;
        static bool Load(UnityModManager.ModEntry modEntry)
        //internal static bool Load(UnityModManager.ModEntry modEntry)
        {
            var harmony = new Harmony(modEntry.Info.Id);
            ModSettings.ModEntry = modEntry;
            ModSettings.LoadAllSettings();
            harmony.PatchAll();
            PostPatchInitializer.Initialize();
            //LoadWatcher = new AreaWatcher();
            //Kingmaker.PubSubSystem.EventBus.Subscribe(LoadWatcher);
            return true;
        }
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Enabled = value;
            return true;
        }

        internal static void LogPatch(string v, object coupDeGraceAbility)
        {
            throw new NotImplementedException();
        }

        public static void Log(string msg)
        {
            ModSettings.ModEntry.Logger.Log(msg);
        }
        [System.Diagnostics.Conditional("DEBUG")]
        public static void LogDebug(string msg)
        {
            ModSettings.ModEntry.Logger.Log(msg);
        }
        public static void LogPatch(string action, [NotNull] IScriptableObjectWithAssetId bp)
        {
            Log($"{action}: {bp.AssetGuid} - {bp.name}");
        }
        public static void LogHeader(string msg)
        {
            Log($"--{msg.ToUpper()}--");
        }
        public static void Error(Exception e, string message)
        {
            Log(message);
            Log(e.ToString());
            PFLog.Mods.Error(message);
        }
        public static void Error(string message)
        {
            Log(message);
            PFLog.Mods.Error(message);
        }
    }
}

