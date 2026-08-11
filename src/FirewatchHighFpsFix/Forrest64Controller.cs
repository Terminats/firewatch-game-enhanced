using UnityEngine;
using UnityEngine.SceneManagement;

namespace FirewatchHighFpsFix
{
    internal static class Forrest64Controller
    {
        private const int SceneBuildIndex = 150;
        private static bool initialized;
        private static bool active;
        private static bool activationPending;
        private static GUIStyle exitHintStyle;
        private static GUIStyle exitHintShadowStyle;

        internal static void Initialize()
        {
            if (initialized)
            {
                return;
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
            initialized = true;
        }

        internal static void Dispose()
        {
            if (initialized)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                initialized = false;
            }

            active = false;
            activationPending = false;
        }

        internal static void Start()
        {
            active = true;
            activationPending = true;
            vgSaveManager.Instance.UnloadEndGameSave();
            SceneManager.LoadScene(SceneBuildIndex, LoadSceneMode.Single);
        }

        internal static void Update()
        {
            if (!active)
            {
                return;
            }

            if (activationPending)
            {
                vgSuperByrnesMode mode =
                    Object.FindObjectOfType<vgSuperByrnesMode>();
                if (mode != null)
                {
                    if (!vgSuperByrnesMode.IsModeActive)
                    {
                        mode.EnableMode();
                    }

                    activationPending = false;
                }
            }

            if (Input.GetKeyDown(KeyCode.F10))
            {
                active = false;
                activationPending = false;
                PlayMakerFSM.BroadcastEvent("BackToMainMenu");
            }
        }

        internal static void OnGUI()
        {
            if (!active)
            {
                return;
            }

            if (exitHintStyle == null)
            {
                exitHintStyle = new GUIStyle(GUI.skin.label);
                exitHintStyle.alignment = TextAnchor.LowerRight;
                exitHintStyle.fontSize = 18;
                exitHintStyle.normal.textColor = Color.white;

                exitHintShadowStyle = new GUIStyle(exitHintStyle);
                exitHintShadowStyle.normal.textColor = new Color(0f, 0f, 0f, 0.8f);
            }

            const string hint = "PRESS F10 TO EXIT";
            Rect rect = new Rect(0f, 0f, Screen.width - 24f, Screen.height - 20f);
            Rect shadowRect = new Rect(rect.x + 2f, rect.y + 2f, rect.width, rect.height);
            GUI.Label(shadowRect, hint, exitHintShadowStyle);
            GUI.Label(rect, hint, exitHintStyle);
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.buildIndex == SceneBuildIndex)
            {
                active = true;
                activationPending = true;
            }
        }
    }
}
