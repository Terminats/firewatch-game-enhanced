using HarmonyLib;
using UnityEngine;

namespace FirewatchHighFpsFix
{
    [HarmonyPatch(typeof(vgDialogInstance), "Update")]
    internal static class DialogueTimerPatch
    {
        private const string EnabledPreference = "FirewatchCommunityPatch.UnlimitedDialogueTime";

        internal static bool Enabled
        {
            get { return PlayerPrefs.GetInt(EnabledPreference, 0) != 0; }
        }

        internal static void SetEnabled(bool value)
        {
            PlayerPrefs.SetInt(EnabledPreference, value ? 1 : 0);
            PlayerPrefs.Save();
        }

        [HarmonyPrefix]
        private static void Prefix(ref float ___dialogStartTime)
        {
            if (Enabled)
            {
                ___dialogStartTime = Time.time;
            }
        }
    }
}
