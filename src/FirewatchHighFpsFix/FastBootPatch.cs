using System.Collections;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FirewatchHighFpsFix
{
    [HarmonyPatch(typeof(vgBoot), "Start")]
    internal static class FastBootPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(vgBoot __instance)
        {
            __instance.StartCoroutine(LoadMainNextFrame());
            return false;
        }

        private static IEnumerator LoadMainNextFrame()
        {
            yield return null;
            SceneManager.LoadScene("Main");
        }
    }
}
