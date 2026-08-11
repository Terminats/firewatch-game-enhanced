using System.Collections;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FirewatchHighFpsFix
{
    [HarmonyPatch(typeof(vgHudManager), "Start")]
    internal static class SubtitleScaleSetting
    {
        internal const float Minimum = 75f;
        internal const float Default = 100f;
        internal const float Maximum = 200f;

        private const string PreferenceName =
            "FirewatchCommunityPatch.SubtitleScale";

        private static readonly System.Reflection.FieldInfo SubtitleSpeakerNameField =
            AccessTools.Field(typeof(vgHudManager), "subtitleSpeakerName");

        private static readonly System.Reflection.FieldInfo SubtitleTextField =
            AccessTools.Field(typeof(vgHudManager), "subtitleText");

        private static readonly System.Reflection.FieldInfo SubtitleObjectField =
            AccessTools.Field(typeof(vgHudManager), "subtitleObject");

        private static int hudInstanceId;
        private static TextMeshProUGUI speakerName;
        private static TextMeshProUGUI subtitleText;
        private static float originalSpeakerFontSize;
        private static float originalSpeakerFontSizeMin;
        private static float originalSpeakerFontSizeMax;
        private static float originalSubtitleFontSize;
        private static float originalSubtitleFontSizeMin;
        private static float originalSubtitleFontSizeMax;
        private static RectTransform subtitleBackground;
        private static RectTransform subtitleGroup;
        private static Vector2 originalBackgroundOffsetMin;
        private static Vector2 originalBackgroundOffsetMax;

        internal static float Value
        {
            get
            {
                return Mathf.Clamp(
                    PlayerPrefs.GetInt(PreferenceName, (int)Default),
                    Minimum,
                    Maximum);
            }
        }

        internal static void SetValue(float value)
        {
            int roundedValue = Mathf.RoundToInt(
                Mathf.Clamp(value, Minimum, Maximum));
            PlayerPrefs.SetInt(PreferenceName, roundedValue);
            PlayerPrefs.Save();
            Apply();
        }

        [HarmonyPostfix]
        private static void Postfix(vgHudManager __instance)
        {
            CaptureOriginalSizes(__instance);
            Apply();
        }

        private static void CaptureOriginalSizes(vgHudManager hud)
        {
            if (hud == null || hud.GetInstanceID() == hudInstanceId)
            {
                return;
            }

            hudInstanceId = hud.GetInstanceID();
            speakerName = (TextMeshProUGUI)SubtitleSpeakerNameField.GetValue(hud);
            subtitleText = (TextMeshProUGUI)SubtitleTextField.GetValue(hud);

            if (speakerName != null)
            {
                originalSpeakerFontSize = speakerName.fontSize;
                originalSpeakerFontSizeMin = speakerName.fontSizeMin;
                originalSpeakerFontSizeMax = speakerName.fontSizeMax;
            }

            if (subtitleText != null)
            {
                originalSubtitleFontSize = subtitleText.fontSize;
                originalSubtitleFontSizeMin = subtitleText.fontSizeMin;
                originalSubtitleFontSizeMax = subtitleText.fontSizeMax;
            }

            CaptureBackground(hud);
        }

        private static void Apply()
        {
            float scale = Value / 100f;
            ApplyScale(
                speakerName,
                originalSpeakerFontSize,
                originalSpeakerFontSizeMin,
                originalSpeakerFontSizeMax,
                scale);
            ApplyScale(
                subtitleText,
                originalSubtitleFontSize,
                originalSubtitleFontSizeMin,
                originalSubtitleFontSizeMax,
                scale);
            ApplyBackgroundPadding(scale);
            ResizeBackgroundToText(scale);
        }

        private static void CaptureBackground(vgHudManager hud)
        {
            subtitleBackground = null;
            subtitleGroup = null;
            GameObject subtitleObject =
                (GameObject)SubtitleObjectField.GetValue(hud);
            if (subtitleObject == null)
            {
                return;
            }

            RectTransform[] rects =
                subtitleObject.GetComponentsInChildren<RectTransform>(true);
            for (int i = 0; i < rects.Length; i++)
            {
                if (rects[i].name == "SubtitleGroup")
                {
                    subtitleGroup = rects[i];
                }
                else if (rects[i].name == "Subtitle Background")
                {
                    subtitleBackground = rects[i];
                }
            }

            if (subtitleBackground != null)
            {
                originalBackgroundOffsetMin = subtitleBackground.offsetMin;
                originalBackgroundOffsetMax = subtitleBackground.offsetMax;
            }
        }

        private static void ApplyBackgroundPadding(float scale)
        {
            if (subtitleBackground == null)
            {
                return;
            }

            float amount = Mathf.Max(0f, scale - 1f);
            Vector2 padding = new Vector2(
                64f * amount,
                32f * amount);
            subtitleBackground.offsetMin = originalBackgroundOffsetMin - padding;
            subtitleBackground.offsetMax = originalBackgroundOffsetMax + padding;
        }

        private static void ResizeBackgroundToText(float scale)
        {
            if (subtitleGroup == null || subtitleText == null ||
                string.IsNullOrEmpty(subtitleText.text))
            {
                return;
            }

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(subtitleGroup);
            subtitleText.ForceMeshUpdate();
            float speakerHeight = speakerName == null
                ? 0f
                : speakerName.preferredHeight;
            float requiredHeight = Mathf.Max(
                subtitleText.preferredHeight,
                speakerHeight) + 48f * scale;
            subtitleGroup.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                requiredHeight);
            LayoutRebuilder.ForceRebuildLayoutImmediate(subtitleGroup);
        }

        [HarmonyPatch(typeof(vgHudManager), "DisplaySubtitle")]
        private static class DisplaySubtitlePatch
        {
            [HarmonyPostfix]
            private static void Postfix(vgHudManager __instance)
            {
                Apply();
                __instance.StartCoroutine(RebuildAfterLayout());
            }

            private static IEnumerator RebuildAfterLayout()
            {
                yield return null;
                Apply();
            }
        }

        private static void ApplyScale(
            TextMeshProUGUI text,
            float originalSize,
            float originalMinimum,
            float originalMaximum,
            float scale)
        {
            if (text == null)
            {
                return;
            }

            text.fontSize = originalSize * scale;
            text.fontSizeMin = originalMinimum * scale;
            text.fontSizeMax = originalMaximum * scale;
            text.SetLayoutDirty();
        }
    }
}
