using BepInEx;
using HarmonyLib;

namespace FirewatchHighFpsFix
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "pl.firewatch.highfpsfix";
        public const string PluginName = "Firewatch High FPS Fix";
        public const string PluginVersion = "1.1.0";

        internal static NoclipController Noclip { get; private set; }

        private void Awake()
        {
            new Harmony(PluginGuid).PatchAll();
            Noclip = new NoclipController();
        }

        private void OnDestroy()
        {
            if (Noclip != null)
            {
                Noclip.Disable();
            }
        }

    }
}
