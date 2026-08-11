using HarmonyLib;
using UnityEngine;

namespace FirewatchHighFpsFix
{
    [HarmonyPatch(typeof(vgSettingsManager), "RefreshAllSettings")]
    internal static class TextureStreamingPatch
    {
        private const int MinimumUploadBufferSizeMb = 32;

        [HarmonyPostfix]
        private static void Postfix()
        {
            Apply();
        }

        internal static void Apply()
        {
            if (QualitySettings.asyncUploadBufferSize < MinimumUploadBufferSizeMb)
            {
                QualitySettings.asyncUploadBufferSize = MinimumUploadBufferSizeMb;
            }
        }
    }
}
