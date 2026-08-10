using HarmonyLib;
using UnityEngine;

namespace FirewatchHighFpsFix
{
    internal static class FovSetting
    {
        internal const float Minimum = 30f;
        internal const float Default = 55f;
        internal const float Maximum = 110f;
        private const float BaseFieldOfView = 55f;
        private const string PreferenceName = "fovAdjust";

        private static readonly System.Reflection.FieldInfo FovAdjustField =
            AccessTools.Field(typeof(vgCameraController), "fovAdjust");

        private static readonly System.Reflection.FieldInfo GoalFovField =
            AccessTools.Field(typeof(vgCameraController), "goalFOV");

        internal static float Value
        {
            get
            {
                if (vgPlayerPrefs.Instance == null)
                {
                    return Default;
                }

                int defaultAdjustment = Mathf.RoundToInt(
                    (Default - BaseFieldOfView) * 100f);
                float value = BaseFieldOfView +
                    vgPlayerPrefs.Instance.GetInt(PreferenceName, defaultAdjustment) / 100f;
                return Mathf.Clamp(value, Minimum, Maximum);
            }
        }

        internal static void SetValue(float value)
        {
            value = Mathf.Round(Mathf.Clamp(value, Minimum, Maximum));
            float newAdjustment = value - BaseFieldOfView;

            if (vgPlayerPrefs.Instance != null)
            {
                vgPlayerPrefs.Instance.SetInt(
                    PreferenceName,
                    Mathf.RoundToInt(newAdjustment * 100f));
                vgPlayerPrefs.Instance.Save();
            }

            vgCameraController[] controllers =
                Object.FindObjectsOfType<vgCameraController>();
            for (int i = 0; i < controllers.Length; i++)
            {
                float oldAdjustment = (float)FovAdjustField.GetValue(controllers[i]);
                float currentGoal = (float)GoalFovField.GetValue(controllers[i]);
                FovAdjustField.SetValue(controllers[i], newAdjustment);
                GoalFovField.SetValue(
                    controllers[i],
                    currentGoal + newAdjustment - oldAdjustment);
            }
        }
    }
}
