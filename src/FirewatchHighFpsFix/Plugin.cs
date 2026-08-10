using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace FirewatchHighFpsFix
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "pl.firewatch.highfpsfix";
        public const string PluginName = "Firewatch Enhanced";
        public const string PluginVersion = "1.1.0";

        internal static ConfigEntry<bool> IgnoreGamepads { get; private set; }
        internal static NoclipController Noclip { get; private set; }
        internal static FpsCounter FpsCounter { get; private set; }

        private void Awake()
        {
            IgnoreGamepads = Config.Bind(
                "Controls",
                "IgnoreGamepads",
                true,
                "Ignore input from connected controllers and virtual gamepads.");
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
