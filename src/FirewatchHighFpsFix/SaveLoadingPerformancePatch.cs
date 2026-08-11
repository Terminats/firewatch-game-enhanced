using HarmonyLib;
using UnityEngine;

namespace FirewatchHighFpsFix
{
    [HarmonyPatch]
    internal static class SaveLoadingPerformancePatch
    {
        private const int LoadingUploadTimeSliceMilliseconds = 33;
        private static int originalUploadTimeSlice;
        private static bool uploadTimeSliceChanged;

        [HarmonyPatch(typeof(vgLoadManager), "StartLoad")]
        [HarmonyPrefix]
        private static void StartLoadPrefix()
        {
            if (uploadTimeSliceChanged)
            {
                return;
            }

            originalUploadTimeSlice = QualitySettings.asyncUploadTimeSlice;
            QualitySettings.asyncUploadTimeSlice = LoadingUploadTimeSliceMilliseconds;
            uploadTimeSliceChanged = true;
        }

        [HarmonyPatch(typeof(vgLoadManager), "LoadFinished")]
        [HarmonyPostfix]
        private static void LoadFinishedPostfix()
        {
            RestoreUploadTimeSlice();
        }

        internal static void RestoreUploadTimeSlice()
        {
            if (!uploadTimeSliceChanged)
            {
                return;
            }

            QualitySettings.asyncUploadTimeSlice = originalUploadTimeSlice;
            uploadTimeSliceChanged = false;
        }
    }
}
