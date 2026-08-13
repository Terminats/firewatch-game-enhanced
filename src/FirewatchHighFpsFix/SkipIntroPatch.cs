using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FirewatchHighFpsFix
{
    internal static class SkipIntroState
    {
        internal static GameObject Button;
        internal static GameObject Forrest64Button;
    }

    [HarmonyPatch(typeof(vgMainMenuController), "Start")]
    internal static class SkipIntroButtonPatch
    {
        [HarmonyPostfix]
        private static void Postfix(vgMainMenuController __instance)
        {
            Button template = FindNewGameButton(__instance);
            if (template == null)
            {
                return;
            }

            if (SkipIntroState.Button == null)
            {
                SkipIntroState.Button = CreateButton(
                    template,
                    "New Game Skip Intro",
                    "NEW GAME (SKIP INTRO)",
                    1,
                    delegate { BeginSkipIntro(__instance); });
            }

            RebuildSelection(template.gameObject);
        }

        internal static GameObject CreateButton(
            Button template,
            string objectName,
            string label,
            int siblingOffset,
            UnityEngine.Events.UnityAction onClick)
        {
            GameObject clone = UnityEngine.Object.Instantiate(template.gameObject);
            clone.SetActive(false);
            clone.name = objectName;
            clone.transform.SetParent(template.transform.parent, false);
            clone.transform.SetSiblingIndex(
                template.transform.GetSiblingIndex() + siblingOffset);

            RemoveOriginalLogic(clone);
            SetLabel(clone, label);

            Button button = clone.GetComponent<Button>();
            if (button == null)
            {
                button = clone.GetComponentInChildren<Button>(true);
            }

            if (button == null)
            {
                UnityEngine.Object.Destroy(clone);
                return null;
            }

            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(onClick);

            clone.SetActive(true);
            return clone;
        }

        private static Button FindNewGameButton(vgMainMenuController controller)
        {
            Button[] buttons = controller.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                int listenerCount = buttons[i].onClick.GetPersistentEventCount();
                for (int j = 0; j < listenerCount; j++)
                {
                    if (buttons[i].onClick.GetPersistentMethodName(j) == "OnClickNewGame")
                    {
                        return buttons[i];
                    }
                }
            }

            return null;
        }

        private static void BeginSkipIntro(vgMainMenuController controller)
        {
            AccessTools.Method(typeof(vgMainMenuController), "OnClickPrelude").Invoke(
                controller,
                null);
        }

        private static void RemoveOriginalLogic(GameObject button)
        {
            MonoBehaviour[] behaviours = button.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                string typeName = behaviours[i].GetType().Name;
                if (typeName == "PlayMakerFSM" || typeName == "vgLocalizeUIText")
                {
                    UnityEngine.Object.DestroyImmediate(behaviours[i]);
                }
            }
        }

        private static void SetLabel(GameObject button, string text)
        {
            TextMeshProUGUI[] labels = button.GetComponentsInChildren<TextMeshProUGUI>(true);
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i].text = text;
            }
        }

        internal static void RebuildSelection(GameObject button)
        {
            vgActiveSelectionGroup group = button.GetComponentInParent<vgActiveSelectionGroup>();
            if (group != null)
            {
                AccessTools.Method(typeof(vgActiveSelectionGroup), "RebuildSelectableList").Invoke(
                    group,
                    null);
            }
        }
    }

    [HarmonyPatch(typeof(vgSpecialFeaturesMenuController), "OnEnable")]
    internal static class Forrest64SpecialFeaturesButtonPatch
    {
        [HarmonyPostfix]
        private static void Postfix(vgSpecialFeaturesMenuController __instance)
        {
            if (Plugin.ShowForrest64Prototype == null ||
                !Plugin.ShowForrest64Prototype.Value)
            {
                return;
            }

            Button template = FindFreeRoamButton(__instance);
            if (template == null)
            {
                return;
            }

            if (SkipIntroState.Forrest64Button == null)
            {
                SkipIntroState.Forrest64Button = SkipIntroButtonPatch.CreateButton(
                    template,
                    "Forrest 64 Prototype",
                    "FORREST 64 (PROTOTYPE)",
                    1,
                    Forrest64Controller.Start);
            }

            SkipIntroButtonPatch.RebuildSelection(template.gameObject);
        }

        private static Button FindFreeRoamButton(
            vgSpecialFeaturesMenuController controller)
        {
            Button[] buttons = controller.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                int listenerCount = buttons[i].onClick.GetPersistentEventCount();
                for (int j = 0; j < listenerCount; j++)
                {
                    if (buttons[i].onClick.GetPersistentMethodName(j) ==
                        "OnClickNewFreeRoamGame")
                    {
                        return buttons[i];
                    }
                }
            }

            return null;
        }
    }
}
