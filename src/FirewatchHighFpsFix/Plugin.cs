using BepInEx;
using HarmonyLib;

namespace FirewatchHighFpsFix
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "pl.firewatch.highfpsfix";
        public const string PluginName = "Firewatch High FPS Fix";
        public const string PluginVersion = "1.0.0";

        private void Awake()
        {
            new Harmony(PluginGuid).PatchAll();
        }
    }
}
