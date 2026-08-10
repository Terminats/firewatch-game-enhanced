using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FirewatchHighFpsFix
{
    internal static class SkipIntroState
    {
        internal static GameObject Button;
    }

    [HarmonyPatch(typeof(vgMainMenuController), "Start")]
    internal static class SkipIntroButtonPatch
    {
        [HarmonyPostfix]
        private static void Postfix(vgMainMenuController __instance)
        {
            if (SkipIntroState.Button != null)
            {
                return;
            }

            Button template = FindNewGameButton(__instance);
            if (template == null)
            {
                return;
            }

            GameObject clone = UnityEngine.Object.Instantiate(template.gameObject);
            clone.SetActive(false);
            clone.name = "New Game Skip Intro";
            clone.transform.SetParent(template.transform.parent, false);
            clone.transform.SetSiblingIndex(template.transform.GetSiblingIndex() + 1);

            RemoveOriginalLogic(clone);
            SetLabel(clone, "NEW GAME (SKIP INTRO)");

            Button button = clone.GetComponent<Button>();
            if (button == null)
            {
                button = clone.GetComponentInChildren<Button>(true);
            }

            if (button == null)
            {
                UnityEngine.Object.Destroy(clone);
                return;
            }

            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(delegate { BeginSkipIntro(__instance); });

            SkipIntroState.Button = clone;
            clone.SetActive(true);
            RebuildSelection(clone);
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

        private static void RebuildSelection(GameObject button)
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
}
