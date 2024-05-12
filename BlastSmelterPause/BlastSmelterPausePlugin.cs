using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BlastSmelterPause.Patches;
using HarmonyLib;
using UnityEngine;

namespace BlastSmelterPause
{
    [BepInPlugin(MyGUID, PluginName, VersionString)]
    public class BlastSmelterPausePlugin : BaseUnityPlugin
    {
        private const string MyGUID = "com.equinox.BlastSmelterPause";
        private const string PluginName = "BlastSmelterPause";
        private const string VersionString = "1.1.0";

        private static readonly Harmony Harmony = new Harmony(MyGUID);
        public static ManualLogSource Log = new ManualLogSource(PluginName);

        private void Awake() {
            Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loading...");
            Harmony.PatchAll();

            Harmony.CreateAndPatchAll(typeof(BlastSmelterInstancePatch));
            Harmony.CreateAndPatchAll(typeof(DrillInstancePatch));

            Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loaded.");
            Log = Logger;
        }
    }
}
