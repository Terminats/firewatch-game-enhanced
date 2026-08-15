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
        public const string PluginVersion = "1.2.2";

        internal static ConfigEntry<bool> IgnoreGamepads { get; private set; }
        internal static ConfigEntry<bool> ShowForrest64Prototype { get; private set; }
        internal static NoclipController Noclip { get; private set; }
        internal static FpsCounter FpsCounter { get; private set; }

        private void Awake()
        {
            IgnoreGamepads = Config.Bind(
                "Controls",
                "IgnoreGamepads",
                true,
                "Ignore input from connected controllers and virtual gamepads.");
            ShowForrest64Prototype = Config.Bind(
                "Experimental",
                "ShowForrest64Prototype",
                false,
                "Show the unfinished Forrest 64 prototype in Special Features.");
            TextureStreamingPatch.Apply();
            new Harmony(PluginGuid).PatchAll();
            FreeRoamPatch.Initialize();
            if (ShowForrest64Prototype.Value)
            {
                Forrest64Controller.Initialize();
            }
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

            SaveLoadingPerformancePatch.RestoreUploadTimeSlice();
            Forrest64Controller.Dispose();
        }

        private void Update()
        {
            if (ShowForrest64Prototype.Value)
            {
                Forrest64Controller.Update();
            }
        }

        private void OnGUI()
        {
            if (ShowForrest64Prototype.Value)
            {
                Forrest64Controller.OnGUI();
            }
        }

    }
}
