using HarmonyLib;
using UnityEngine;

namespace FirewatchHighFpsFix
{
    [HarmonyPatch(typeof(vgPlayerTargeting), "Update")]
    internal static class TargetingPerformancePatch
    {
        private const float TargetingUpdateInterval = 1f / 60f;
        private static float accumulatedTime;

        [HarmonyPrefix]
        private static bool Prefix()
        {
            accumulatedTime += Time.unscaledDeltaTime;
            if (accumulatedTime < TargetingUpdateInterval)
            {
                return false;
            }

            accumulatedTime %= TargetingUpdateInterval;
            return true;
        }
    }
}
