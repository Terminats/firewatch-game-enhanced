using System.Collections;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FirewatchHighFpsFix
{
    [HarmonyPatch(typeof(vgSettingsMenuController), "ShowMenu")]
    internal static class SettingsMenuPatch
    {
        private static GameObject fpsCheckbox;
        private static GameObject dialogueCheckbox;

        [HarmonyPostfix]
        private static void Postfix(vgSettingsMenuController __instance, int index)
        {
            if (index != 0 || Plugin.FpsCounter == null ||
                (fpsCheckbox != null && dialogueCheckbox != null))
            {
                return;
            }

            GameObject template = FindTemplate(__instance);
            if (template == null)
            {
                return;
            }

            if (fpsCheckbox == null)
            {
                fpsCheckbox = CreateCheckbox(
                    template,
                    "FPS Counter",
                    1,
                    Plugin.FpsCounter.Enabled,
                    Plugin.FpsCounter.SetEnabled);
            }

            if (dialogueCheckbox == null)
            {
                dialogueCheckbox = CreateCheckbox(
                    template,
                    "Unlimited Dialogue Time",
                    2,
                    DialogueTimerPatch.Enabled,
                    DialogueTimerPatch.SetEnabled);
            }

            RebuildSelection(__instance);
        }

        private static GameObject CreateCheckbox(
            GameObject template,
            string label,
            int siblingOffset,
            bool initialValue,
            UnityAction<bool> onValueChanged)
        {
            GameObject checkbox = Object.Instantiate(template);
            checkbox.SetActive(false);
            checkbox.name = label;
            checkbox.transform.SetParent(template.transform.parent, false);
            checkbox.transform.SetSiblingIndex(template.transform.GetSiblingIndex() + siblingOffset);

            RemoveOriginalOptionLogic(checkbox);
            SetLabel(checkbox, label);
            ConnectToggle(checkbox, initialValue, onValueChanged);

            checkbox.SetActive(true);
            return checkbox;
        }

        private static GameObject FindTemplate(vgSettingsMenuController settingsMenu)
        {
            IList screens = (IList)AccessTools.Field(
                typeof(vgSettingsMenuController), "screens").GetValue(settingsMenu);
            if (screens == null || screens.Count == 0)
            {
                return null;
            }

            object generalScreen = screens[0];
            Component rootAnimator = (Component)AccessTools.Field(
                generalScreen.GetType(), "rootAnimator").GetValue(generalScreen);
            if (rootAnimator == null)
            {
                return null;
            }

            Transform[] children = rootAnimator.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i].name == "Minimal Interface Checkbox")
                {
                    return children[i].gameObject;
                }
            }

            return null;
        }

        private static void RemoveOriginalOptionLogic(GameObject checkbox)
        {
            MonoBehaviour[] behaviours = checkbox.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                string typeName = behaviours[i].GetType().Name;
                if (typeName == "PlayMakerFSM" || typeName == "vgLocalizeUIText")
                {
                    Object.DestroyImmediate(behaviours[i]);
                }
            }
        }

        private static void SetLabel(GameObject checkbox, string label)
        {
            TextMeshProUGUI[] labels = checkbox.GetComponentsInChildren<TextMeshProUGUI>(true);
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i].text = label;
            }
        }

        private static void ConnectToggle(
            GameObject checkbox,
            bool initialValue,
            UnityAction<bool> onValueChanged)
        {
            Toggle toggle = checkbox.GetComponentInChildren<Toggle>(true);
            if (toggle == null)
            {
                return;
            }

            toggle.onValueChanged.RemoveAllListeners();
            toggle.isOn = initialValue;
            toggle.onValueChanged.AddListener(onValueChanged);

            Button[] buttons = checkbox.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].onClick.RemoveAllListeners();
                buttons[i].onClick.AddListener(delegate { toggle.isOn = !toggle.isOn; });
            }
        }

        private static void RebuildSelection(vgSettingsMenuController settingsMenu)
        {
            vgActiveSelectionGroup selectionGroup = (vgActiveSelectionGroup)
                AccessTools.Field(typeof(vgSettingsMenuController), "selectionGroup").GetValue(settingsMenu);
            if (selectionGroup != null)
            {
                AccessTools.Method(typeof(vgActiveSelectionGroup), "RebuildSelectableList").Invoke(
                    selectionGroup, null);
            }
        }
    }
}
