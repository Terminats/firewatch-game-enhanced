using System.Collections;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FirewatchHighFpsFix
{
    [HarmonyPatch(typeof(vgSettingsMenuController), "ShowMenu")]
    internal static class SettingsMenuPatch
    {
        private const string CheckboxName = "FPS Counter";
        private static GameObject checkboxObject;

        [HarmonyPostfix]
        private static void Postfix(vgSettingsMenuController __instance, int index)
        {
            if (index == 0 && Plugin.FpsCounter != null && checkboxObject == null)
            {
                CreateCheckbox(__instance);
            }
        }

        private static void CreateCheckbox(vgSettingsMenuController settingsMenu)
        {
            GameObject template = FindTemplate(settingsMenu);
            if (template == null)
            {
                return;
            }

            checkboxObject = Object.Instantiate(template);
            checkboxObject.SetActive(false);
            checkboxObject.name = CheckboxName;
            checkboxObject.transform.SetParent(template.transform.parent, false);
            checkboxObject.transform.SetSiblingIndex(template.transform.GetSiblingIndex() + 1);

            RemoveOriginalOptionLogic();
            SetLabel();
            ConnectToggle();

            checkboxObject.SetActive(true);

            vgActiveSelectionGroup selectionGroup = (vgActiveSelectionGroup)
                AccessTools.Field(typeof(vgSettingsMenuController), "selectionGroup").GetValue(settingsMenu);
            if (selectionGroup != null)
            {
                AccessTools.Method(typeof(vgActiveSelectionGroup), "RebuildSelectableList").Invoke(
                    selectionGroup, null);
            }
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

        private static void RemoveOriginalOptionLogic()
        {
            MonoBehaviour[] behaviours = checkboxObject.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                string typeName = behaviours[i].GetType().Name;
                if (typeName == "PlayMakerFSM" || typeName == "vgLocalizeUIText")
                {
                    Object.DestroyImmediate(behaviours[i]);
                }
            }
        }

        private static void SetLabel()
        {
            TextMeshProUGUI[] labels = checkboxObject.GetComponentsInChildren<TextMeshProUGUI>(true);
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i].text = "FPS Counter";
            }
        }

        private static void ConnectToggle()
        {
            Toggle toggle = checkboxObject.GetComponentInChildren<Toggle>(true);
            if (toggle == null)
            {
                return;
            }

            toggle.onValueChanged.RemoveAllListeners();
            toggle.isOn = Plugin.FpsCounter.Enabled;
            toggle.onValueChanged.AddListener(Plugin.FpsCounter.SetEnabled);

            Button[] buttons = checkboxObject.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].onClick.RemoveAllListeners();
                buttons[i].onClick.AddListener(delegate { toggle.isOn = !toggle.isOn; });
            }
        }
    }
}
