using System;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

namespace FirewatchHighFpsFix
{
    internal static class FreeRoamPatch
    {
        private const string CaveFsmName = "Cave State Change";
        private const string CaveExitEvent = "FreeRoamExitedCave";

        internal static void Initialize()
        {
            GameObject runnerObject = new GameObject(
                "Firewatch Enhanced Free Roam Patch");
            UnityEngine.Object.DontDestroyOnLoad(runnerObject);
            runnerObject.hideFlags = HideFlags.HideAndDontSave;
            runnerObject.AddComponent<FreeRoamPatchRunner>();
        }

        internal static void Apply()
        {
            UnlockNativeFreeRoamButton();
            PreserveCaveExitEvents();
        }

        private static void PreserveCaveExitEvents()
        {
            PlayMakerFSM[] fsms =
                Resources.FindObjectsOfTypeAll<PlayMakerFSM>();
            for (int i = 0; i < fsms.Length; i++)
            {
                PlayMakerFSM fsm = fsms[i];
                if (fsm == null || fsm.gameObject == null ||
                    fsm.FsmName != CaveFsmName ||
                    fsm.Fsm.KeepDelayedEventsOnStateExit)
                {
                    continue;
                }

                if (SendsCaveExitEvent(fsm))
                {
                    fsm.Fsm.KeepDelayedEventsOnStateExit = true;
                }
            }
        }

        private static bool SendsCaveExitEvent(PlayMakerFSM fsm)
        {
            FsmState[] states = fsm.FsmStates;
            for (int i = 0; i < states.Length; i++)
            {
                states[i].LoadActions();
                FsmStateAction[] actions = states[i].Actions;
                for (int j = 0; actions != null && j < actions.Length; j++)
                {
                    SendEventByName sendEvent =
                        actions[j] as SendEventByName;
                    if (sendEvent != null && sendEvent.sendEvent != null &&
                        string.Equals(
                            sendEvent.sendEvent.Value,
                            CaveExitEvent,
                            StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static void UnlockNativeFreeRoamButton()
        {
            vgSpecialFeaturesMenuController menu =
                UnityEngine.Object.FindObjectOfType<
                    vgSpecialFeaturesMenuController>();
            if (menu == null)
            {
                return;
            }

            GameObject enabledRoot = null;
            GameObject disabledRoot = null;
            Transform[] children = menu.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i].name == "SpecialFeaturesFreeRoam")
                {
                    enabledRoot = children[i].gameObject;
                }
                else if (children[i].name ==
                    "SpecialFeaturesFreeRoam_disabled")
                {
                    disabledRoot = children[i].gameObject;
                }
            }

            if (enabledRoot == null || enabledRoot.activeSelf)
            {
                return;
            }

            if (disabledRoot != null)
            {
                disabledRoot.SetActive(false);
            }

            enabledRoot.SetActive(true);
            SkipIntroButtonPatch.RebuildSelection(enabledRoot);
        }
    }

    internal sealed class FreeRoamPatchRunner : MonoBehaviour
    {
        private float nextUpdate;

        private void Update()
        {
            if (Time.realtimeSinceStartup < nextUpdate)
            {
                return;
            }

            nextUpdate = Time.realtimeSinceStartup + 1f;
            FreeRoamPatch.Apply();
        }
    }
}
