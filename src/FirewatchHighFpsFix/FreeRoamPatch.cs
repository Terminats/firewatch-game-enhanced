using System;
using HarmonyLib;
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

    [HarmonyPatch(typeof(Fsm), "ProcessEvent")]
    internal static class FreeRoamCaveExitFactPatch
    {
        private const string TimeOfDayFsmName = "Amazing Time Of Day Robot";
        private const string CaveExitEvent = "FreeRoamExitedCave";

        private static void Prefix(Fsm __instance, ref FsmEvent fsmEvent)
        {
            if (__instance == null || fsmEvent == null ||
                __instance.Name != TimeOfDayFsmName)
            {
                return;
            }

            vgEventManager eventManager = vgEventManager.Instance;
            if (__instance.ActiveStateName == "State 2")
            {
                int triggerNumber;
                if (int.TryParse(fsmEvent.Name, out triggerNumber) &&
                    (triggerNumber < 1 || triggerNumber > 38))
                {
                    FsmInt currentTrigger =
                        __instance.Variables.GetFsmInt("CurrentTriggerNumber");
                    FsmInt upcomingTrigger =
                        __instance.Variables.GetFsmInt("UpcomingTriggerNumber");
                    if (currentTrigger != null)
                    {
                        currentTrigger.Value = 1;
                    }
                    if (upcomingTrigger != null)
                    {
                        upcomingTrigger.Value = 2;
                    }

                    if (eventManager != null)
                    {
                        eventManager.SetFact(
                            "TODCurrentTrigger",
                            vgFactOperation.Set,
                            1f,
                            vgBlackboardLocationHint.Global);
                    }

                    FsmEvent firstTrigger = __instance.GetEvent("1");
                    if (firstTrigger != null)
                    {
                        fsmEvent = firstTrigger;
                    }
                }
            }

            if (fsmEvent.Name == CaveExitEvent && eventManager != null)
            {
                eventManager.SetFact(
                    "FreeRoamInCave",
                    false,
                    vgBlackboardLocationHint.Global);
            }
        }
    }
}
