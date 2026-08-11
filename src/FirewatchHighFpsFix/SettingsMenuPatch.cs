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
        private static GameObject generalOptionsRow;
        private static GameObject subtitleScaleSlider;
        private static GameObject fovSlider;
        private static GameObject mouseAccelerationCheckbox;
        private static GameObject ignoreOtherInputDevicesCheckbox;
        private static TextMeshProUGUI fovValueLabel;
        private static TextMeshProUGUI subtitleScaleValueLabel;

        [HarmonyPostfix]
        private static void Postfix(vgSettingsMenuController __instance, int index)
        {
            if (Plugin.FpsCounter == null)
            {
                return;
            }

            if (index == 0 &&
                (fpsCheckbox == null || dialogueCheckbox == null || subtitleScaleSlider == null))
            {
                CreateGeneralOptions(__instance);
            }
            else if (index == 1 && fovSlider == null)
            {
                CreateFovSlider(__instance);
            }
            else if (index == 2 &&
                (mouseAccelerationCheckbox == null || ignoreOtherInputDevicesCheckbox == null))
            {
                CreateControlsOptions(__instance);
            }
        }

        private static void CreateGeneralOptions(vgSettingsMenuController settingsMenu)
        {
            GameObject template = FindTemplate(settingsMenu, 0, "Minimal Interface Checkbox");
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

            if (generalOptionsRow == null)
            {
                WrapGeneralOptionsInOneRow(template);
            }

            if (subtitleScaleSlider == null)
            {
                CreateSubtitleScaleSlider(settingsMenu);
            }

            RebuildSelection(settingsMenu, 0);
        }

        private static void WrapGeneralOptionsInOneRow(GameObject template)
        {
            if (fpsCheckbox == null || dialogueCheckbox == null ||
                fpsCheckbox.transform.parent != dialogueCheckbox.transform.parent)
            {
                return;
            }

            Transform parent = fpsCheckbox.transform.parent;
            int siblingIndex = Mathf.Min(
                fpsCheckbox.transform.GetSiblingIndex(),
                dialogueCheckbox.transform.GetSiblingIndex());

            generalOptionsRow = new GameObject(
                "Firewatch Enhanced Options",
                typeof(RectTransform),
                typeof(HorizontalLayoutGroup),
                typeof(LayoutElement));
            generalOptionsRow.transform.SetParent(parent, false);
            generalOptionsRow.transform.SetSiblingIndex(siblingIndex);

            RectTransform templateRect = template.GetComponent<RectTransform>();
            RectTransform rowRect = generalOptionsRow.GetComponent<RectTransform>();
            if (templateRect != null)
            {
                rowRect.anchorMin = templateRect.anchorMin;
                rowRect.anchorMax = templateRect.anchorMax;
                rowRect.pivot = templateRect.pivot;
                rowRect.sizeDelta = templateRect.sizeDelta;
            }

            LayoutElement templateLayout = template.GetComponent<LayoutElement>();
            LayoutElement rowLayout = generalOptionsRow.GetComponent<LayoutElement>();
            rowLayout.preferredHeight = templateLayout != null &&
                templateLayout.preferredHeight > 0f
                ? templateLayout.preferredHeight
                : Mathf.Max(40f, templateRect == null ? 0f : templateRect.rect.height);
            rowLayout.flexibleWidth = 1f;

            HorizontalLayoutGroup layout =
                generalOptionsRow.GetComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 24f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            MoveCheckboxIntoRow(fpsCheckbox);
            MoveCheckboxIntoRow(dialogueCheckbox);
            LayoutRebuilder.ForceRebuildLayoutImmediate(rowRect);
        }

        private static void MoveCheckboxIntoRow(GameObject checkbox)
        {
            checkbox.transform.SetParent(generalOptionsRow.transform, false);
            LayoutElement layout = checkbox.GetComponent<LayoutElement>();
            if (layout == null)
            {
                layout = checkbox.AddComponent<LayoutElement>();
            }

            layout.minWidth = 0f;
            layout.preferredWidth = 0f;
            layout.flexibleWidth = 1f;
            AdjustFocusGraphic(
                checkbox,
                checkbox == fpsCheckbox ? -160f : 64f);
        }

        private static void AdjustFocusGraphic(
            GameObject checkbox,
            float widthAdjustment)
        {
            RectTransform[] rects =
                checkbox.GetComponentsInChildren<RectTransform>(true);
            for (int i = 0; i < rects.Length; i++)
            {
                if (rects[i].name == "WidgetBackground")
                {
                    rects[i].sizeDelta += new Vector2(widthAdjustment, 0f);
                    return;
                }
            }
        }

        private static void CreateSubtitleScaleSlider(vgSettingsMenuController settingsMenu)
        {
            GameObject template = FindSliderTemplate(settingsMenu, 0);
            if (template == null)
            {
                return;
            }

            subtitleScaleSlider = Object.Instantiate(template);
            subtitleScaleSlider.SetActive(false);
            subtitleScaleSlider.name = "Subtitle Size";
            subtitleScaleSlider.transform.SetParent(template.transform.parent, false);
            if (generalOptionsRow != null &&
                generalOptionsRow.transform.parent == subtitleScaleSlider.transform.parent)
            {
                subtitleScaleSlider.transform.SetSiblingIndex(
                    generalOptionsRow.transform.GetSiblingIndex() + 1);
            }
            else
            {
                subtitleScaleSlider.transform.SetAsLastSibling();
            }

            RemoveOriginalOptionLogic(subtitleScaleSlider);
            ConfigureSliderLabels(
                subtitleScaleSlider,
                "Subtitle Size",
                Mathf.RoundToInt(SubtitleScaleSetting.Value) + "%",
                out subtitleScaleValueLabel);

            Slider slider = subtitleScaleSlider.GetComponentInChildren<Slider>(true);
            if (slider != null)
            {
                slider.onValueChanged.RemoveAllListeners();
                slider.minValue = SubtitleScaleSetting.Minimum;
                slider.maxValue = SubtitleScaleSetting.Maximum;
                slider.wholeNumbers = true;
                slider.value = SubtitleScaleSetting.Value;
                slider.onValueChanged.AddListener(SetSubtitleScale);
            }

            subtitleScaleSlider.SetActive(true);
        }

        private static void CreateFovSlider(vgSettingsMenuController settingsMenu)
        {
            GameObject template = FindSliderTemplate(settingsMenu, 1);
            if (template == null)
            {
                return;
            }

            fovSlider = Object.Instantiate(template);
            fovSlider.SetActive(false);
            fovSlider.name = "Field of View";
            fovSlider.transform.SetParent(template.transform.parent, false);
            fovSlider.transform.SetSiblingIndex(template.transform.GetSiblingIndex() + 1);

            RemoveOriginalOptionLogic(fovSlider);
            ConfigureFovLabels();

            Slider slider = fovSlider.GetComponentInChildren<Slider>(true);
            if (slider != null)
            {
                slider.onValueChanged.RemoveAllListeners();
                slider.minValue = FovSetting.Minimum;
                slider.maxValue = FovSetting.Maximum;
                slider.wholeNumbers = true;
                slider.value = FovSetting.Value;
                slider.onValueChanged.AddListener(SetFovValue);
            }

            fovSlider.SetActive(true);
            RebuildSelection(settingsMenu, 1);
        }

        private static void CreateControlsOptions(vgSettingsMenuController settingsMenu)
        {
            GameObject template = FindToggleTemplate(settingsMenu, 2);
            if (template == null || vgSettingsManager.Instance == null)
            {
                return;
            }

            if (mouseAccelerationCheckbox == null)
            {
                mouseAccelerationCheckbox = CreateCheckbox(
                    template,
                    "Mouse Acceleration",
                    1,
                    vgSettingsManager.Instance.mouseAcceleration,
                    SetMouseAcceleration);
            }

            if (ignoreOtherInputDevicesCheckbox == null)
            {
                vgSettingsManager.Instance.disableControllers = Plugin.IgnoreGamepads.Value;
                ignoreOtherInputDevicesCheckbox = CreateCheckbox(
                    template,
                    "Ignore Pads",
                    2,
                    Plugin.IgnoreGamepads.Value,
                    SetIgnoreOtherInputDevices);
            }

            RebuildSelection(settingsMenu, 2);
        }

        private static void SetMouseAcceleration(bool value)
        {
            if (vgSettingsManager.Instance != null)
            {
                vgSettingsManager.Instance.mouseAcceleration = value;
            }
        }

        private static void SetIgnoreOtherInputDevices(bool value)
        {
            Plugin.IgnoreGamepads.Value = value;
            if (vgSettingsManager.Instance != null)
            {
                vgSettingsManager.Instance.disableControllers = value;
            }
        }

        private static GameObject FindToggleTemplate(
            vgSettingsMenuController settingsMenu,
            int screenIndex)
        {
            IList screens = GetScreens(settingsMenu);
            if (screens == null || screens.Count <= screenIndex)
            {
                return null;
            }

            object screen = screens[screenIndex];
            Component rootAnimator = (Component)AccessTools.Field(
                screen.GetType(), "rootAnimator").GetValue(screen);
            Component inputModule = (Component)AccessTools.Field(
                screen.GetType(), "uiInputModule").GetValue(screen);

            GameObject template = FindToggleRow(rootAnimator);
            if (template == null)
            {
                template = FindToggleRow(inputModule);
            }

            return template;
        }

        private static GameObject FindToggleRow(Component root)
        {
            if (root == null)
            {
                return null;
            }

            Toggle[] toggles = root.GetComponentsInChildren<Toggle>(true);
            if (toggles.Length == 0)
            {
                return null;
            }

            Transform row = toggles[0].transform;
            GameObject toggleAncestor = row.gameObject;
            while (row.parent != null && row.parent != root.transform)
            {
                string rowName = row.name.ToLowerInvariant();
                if (rowName.Contains("inverty") || rowName.Contains("left handed"))
                {
                    return row.gameObject;
                }

                if (rowName.Contains("toggle"))
                {
                    toggleAncestor = row.gameObject;
                }

                if (row.parent.GetComponent<VerticalLayoutGroup>() != null)
                {
                    return row.gameObject;
                }

                row = row.parent;
            }

            return toggleAncestor;
        }

        private static GameObject FindSliderTemplate(
            vgSettingsMenuController settingsMenu,
            int screenIndex)
        {
            IList screens = GetScreens(settingsMenu);
            if (screens == null || screens.Count <= screenIndex)
            {
                return null;
            }

            object screen = screens[screenIndex];
            Component rootAnimator = (Component)AccessTools.Field(
                screen.GetType(), "rootAnimator").GetValue(screen);
            Component inputModule = (Component)AccessTools.Field(
                screen.GetType(), "uiInputModule").GetValue(screen);

            GameObject template = FindSliderRow(rootAnimator);
            if (template == null)
            {
                template = FindSliderRow(inputModule);
            }

            return template;
        }

        private static GameObject FindSliderRow(Component root)
        {
            if (root == null)
            {
                return null;
            }

            Slider[] sliders = root.GetComponentsInChildren<Slider>(true);
            for (int i = 0; i < sliders.Length; i++)
            {
                Transform row = sliders[i].transform;
                GameObject sliderAncestor = row.gameObject;
                while (row.parent != null && row.parent != root.transform)
                {
                    if (row.name.ToLowerInvariant().Contains("brightness"))
                    {
                        return row.gameObject;
                    }

                    if (row.name.ToLowerInvariant().Contains("slider"))
                    {
                        sliderAncestor = row.gameObject;
                    }

                    if (row.parent.GetComponent<VerticalLayoutGroup>() != null)
                    {
                        return row.gameObject;
                    }

                    row = row.parent;
                }

                if (sliderAncestor != null)
                {
                    return sliderAncestor;
                }
            }

            return null;
        }

        private static void ConfigureFovLabels()
        {
            ConfigureSliderLabels(
                fovSlider,
                "Field of View",
                Mathf.RoundToInt(FovSetting.Value).ToString(),
                out fovValueLabel);
        }

        private static void ConfigureSliderLabels(
            GameObject row,
            string label,
            string value,
            out TextMeshProUGUI valueLabel)
        {
            valueLabel = null;
            TextMeshProUGUI[] labels = row.GetComponentsInChildren<TextMeshProUGUI>(true);
            for (int i = 0; i < labels.Length; i++)
            {
                if (labels[i].gameObject.name.ToLowerInvariant().Contains("value"))
                {
                    valueLabel = labels[i];
                    valueLabel.text = value;
                }
                else
                {
                    labels[i].text = label;
                }
            }
        }

        private static void SetSubtitleScale(float value)
        {
            SubtitleScaleSetting.SetValue(value);
            if (subtitleScaleValueLabel != null)
            {
                subtitleScaleValueLabel.text = Mathf.RoundToInt(value) + "%";
            }
        }

        private static void SetFovValue(float value)
        {
            FovSetting.SetValue(value);
            if (fovValueLabel != null)
            {
                fovValueLabel.text = Mathf.RoundToInt(value).ToString();
            }
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
            if (siblingOffset < 0)
            {
                checkbox.transform.SetAsLastSibling();
            }
            else
            {
                checkbox.transform.SetSiblingIndex(template.transform.GetSiblingIndex() + siblingOffset);
            }

            RemoveOriginalOptionLogic(checkbox);
            SetLabel(checkbox, label);
            ConnectToggle(checkbox, initialValue, onValueChanged);

            checkbox.SetActive(true);
            return checkbox;
        }

        private static GameObject FindTemplate(
            vgSettingsMenuController settingsMenu,
            int screenIndex,
            string objectName)
        {
            IList screens = GetScreens(settingsMenu);
            if (screens == null || screens.Count <= screenIndex)
            {
                return null;
            }

            object generalScreen = screens[screenIndex];
            Component rootAnimator = (Component)AccessTools.Field(
                generalScreen.GetType(), "rootAnimator").GetValue(generalScreen);
            if (rootAnimator == null)
            {
                return null;
            }

            Transform[] children = rootAnimator.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i].name == objectName)
                {
                    return children[i].gameObject;
                }
            }

            return null;
        }

        private static IList GetScreens(vgSettingsMenuController settingsMenu)
        {
            return (IList)AccessTools.Field(
                typeof(vgSettingsMenuController), "screens").GetValue(settingsMenu);
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

        private static void RebuildSelection(
            vgSettingsMenuController settingsMenu,
            int screenIndex)
        {
            vgActiveSelectionGroup selectionGroup = (vgActiveSelectionGroup)
                AccessTools.Field(typeof(vgSettingsMenuController), "selectionGroup").GetValue(settingsMenu);
            if (selectionGroup != null)
            {
                AccessTools.Method(typeof(vgActiveSelectionGroup), "RebuildSelectableList").Invoke(
                    selectionGroup, null);
                selectionGroup.SetCurrentSelection(screenIndex);
            }
        }
    }
}
