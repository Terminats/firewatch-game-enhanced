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
        internal static FpsCounter FpsCounter { get; private set; }

        private void Awake()
        {
            new Harmony(PluginGuid).PatchAll();
            Noclip = new NoclipController();
            FpsCounter = new FpsCounter();
        }

        private void OnDestroy()
        {
            if (Noclip != null)
            {
                Noclip.Disable();
            }

            if (FpsCounter != null)
            {
                FpsCounter.Dispose();
            }
        }

    }
}
