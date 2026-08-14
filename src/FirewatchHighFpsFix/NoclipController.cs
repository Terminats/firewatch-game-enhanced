using HarmonyLib;
using UnityEngine;

namespace FirewatchHighFpsFix
{
    internal sealed class NoclipController
    {
        private const float NormalSpeed = 12f;
        private const float FastSpeed = 40f;

        private vgPlayerNavigationController navigation;
        private vgPlayerController playerController;
        private Renderer playerRenderer;
        private GameObject ringObject;
        private GameObject reticuleActiveRing;
        private CharacterController characterController;
        private bool navigationWasEnabled;
        private bool controllerWasEnabled;
        private bool rendererWasEnabled;
        private bool rendererVisibilityChanged;
        private bool ringWasActive;
        private bool ringVisibilityChanged;
        private bool reticuleActiveRingWasActive;
        private bool reticuleActiveRingVisibilityChanged;
        private bool active;

        internal void Update()
        {
            if (Input.GetKeyDown(KeyCode.F8))
            {
                if (active)
                {
                    Disable();
                }
                else
                {
                    Enable();
                }
            }

            if (!active)
            {
                return;
            }

            if (navigation == null)
            {
                active = false;
                return;
            }

            MovePlayer();
            HideNoclipVisuals();
        }


        private void Enable()
        {
            navigation = Object.FindObjectOfType<vgPlayerNavigationController>();
            if (navigation == null)
            {
                return;
            }

            characterController = navigation.GetComponent<CharacterController>();
            playerController = navigation.GetComponent<vgPlayerController>();
            navigationWasEnabled = navigation.enabled;
            controllerWasEnabled = characterController != null && characterController.enabled;

            if (playerController != null)
            {
                playerRenderer = (Renderer)AccessTools.Field(
                    typeof(vgPlayerController),
                    "playerRenderer").GetValue(playerController);
                if (playerRenderer != null)
                {
                    rendererWasEnabled = playerRenderer.enabled;
                    playerRenderer.enabled = false;
                    rendererVisibilityChanged = true;
                }

                ringObject = (GameObject)AccessTools.Field(
                    typeof(vgPlayerController),
                    "ringObject").GetValue(playerController);
                if (ringObject != null)
                {
                    ringWasActive = ringObject.activeSelf;
                    ringObject.SetActive(false);
                    ringVisibilityChanged = true;
                }
            }

            vgHudManager hudManager = vgHudManager.Instance;
            if (hudManager != null)
            {
                reticuleActiveRing = (GameObject)AccessTools.Field(
                    typeof(vgHudManager),
                    "reticuleActiveRing").GetValue(hudManager);
                if (reticuleActiveRing != null)
                {
                    reticuleActiveRingWasActive = reticuleActiveRing.activeSelf;
                    reticuleActiveRing.SetActive(false);
                    reticuleActiveRingVisibilityChanged = true;
                }
            }

            navigation.enabled = false;
            if (characterController != null)
            {
                characterController.enabled = false;
            }

            active = true;
        }

        internal void Disable()
        {
            if (!active)
            {
                return;
            }

            if (characterController != null)
            {
                characterController.enabled = controllerWasEnabled;
            }

            if (navigation != null)
            {
                navigation.enabled = navigationWasEnabled;
            }

            if (rendererVisibilityChanged && playerRenderer != null)
            {
                playerRenderer.enabled = rendererWasEnabled;
            }
            if (ringVisibilityChanged && ringObject != null)
            {
                ringObject.SetActive(ringWasActive);
            }
            if (reticuleActiveRingVisibilityChanged && reticuleActiveRing != null)
            {
                reticuleActiveRing.SetActive(reticuleActiveRingWasActive);
            }

            rendererVisibilityChanged = false;
            ringVisibilityChanged = false;
            reticuleActiveRingVisibilityChanged = false;
            active = false;
        }

        private void HideNoclipVisuals()
        {
            if (ringObject != null && ringObject.activeSelf)
            {
                ringObject.SetActive(false);
            }

            if (reticuleActiveRing != null && reticuleActiveRing.activeSelf)
            {
                reticuleActiveRing.SetActive(false);
            }
        }

        private void MovePlayer()
        {
            Camera playerCamera = Camera.main;
            Vector3 forward = playerCamera != null ? playerCamera.transform.forward : navigation.transform.forward;
            Vector3 right = playerCamera != null ? playerCamera.transform.right : navigation.transform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 direction = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) direction += forward;
            if (Input.GetKey(KeyCode.S)) direction -= forward;
            if (Input.GetKey(KeyCode.D)) direction += right;
            if (Input.GetKey(KeyCode.A)) direction -= right;
            if (Input.GetKey(KeyCode.Space)) direction += Vector3.up;
            if (Input.GetKey(KeyCode.LeftControl)) direction += Vector3.down;

            if (direction.sqrMagnitude > 1f)
            {
                direction.Normalize();
            }

            float speed = Input.GetKey(KeyCode.LeftShift) ? FastSpeed : NormalSpeed;
            navigation.transform.position += direction * speed * Time.unscaledDeltaTime;
        }
    }

    [HarmonyPatch(typeof(vgPlayerController), "Update")]
    internal static class PlayerUpdatePatch
    {
        [HarmonyPostfix]
        private static void Postfix()
        {
            if (Plugin.Noclip != null)
            {
                Plugin.Noclip.Update();
            }

            if (Plugin.FpsCounter != null)
            {
                Plugin.FpsCounter.Update();
            }
        }
    }
}
