using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace FirewatchHighFpsFix
{
    [HarmonyPatch(typeof(vgPlayerNavigationController), "ApplyForces")]
    internal static class ApplyForcesPatch
    {
        private static readonly FieldInfo MovementForcesField =
            AccessTools.Field(typeof(vgPlayerNavigationController), "movementForces");

        private static readonly FieldInfo MoveDeltaField =
            AccessTools.Field(typeof(vgPlayerNavigationController), "moveDelta");

        [HarmonyPostfix]
        private static void Postfix(vgPlayerNavigationController __instance)
        {
            vgPlayerController playerController = __instance.GetComponent<vgPlayerController>();
            if (playerController == null || playerController.GetLocomotionInput().sqrMagnitude < 0.01f)
            {
                return;
            }

            // Firewatch reconstructs the next frame's velocity from an extremely small
            // displacement. At high frame rates, precision loss can reduce that result to
            // zero and prevent acceleration from accumulating. Preserve the requested,
            // collision-adjusted horizontal velocity while movement input is active.
            Vector3 requestedVelocity = (Vector3)MoveDeltaField.GetValue(__instance);
            requestedVelocity.y = 0f;
            MovementForcesField.SetValue(__instance, requestedVelocity);
        }
    }
}
