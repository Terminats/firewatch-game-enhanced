using UnityEngine;
using UnityEngine.UI;

namespace FirewatchHighFpsFix
{
    internal sealed class FpsCounter
    {
        private const float RefreshInterval = 0.25f;
        private const string EnabledPreference = "FirewatchCommunityPatch.ShowFpsCounter";

        private GameObject canvasObject;
        private Text text;
        private float elapsedTime;
        private int frameCount;

        internal bool Enabled
        {
            get { return PlayerPrefs.GetInt(EnabledPreference, 1) != 0; }
        }

        internal void Update()
        {
            if (!Enabled)
            {
                if (canvasObject != null && canvasObject.activeSelf)
                {
                    canvasObject.SetActive(false);
                }

                return;
            }

            if (text == null)
            {
                CreateUi();
            }
            else if (!canvasObject.activeSelf)
            {
                canvasObject.SetActive(true);
            }

            float deltaTime = Time.unscaledDeltaTime;
            if (deltaTime <= 0f)
            {
                return;
            }

            elapsedTime += deltaTime;
            frameCount++;

            if (elapsedTime >= RefreshInterval)
            {
                text.text = Mathf.RoundToInt(frameCount / elapsedTime) + " FPS";
                elapsedTime = 0f;
                frameCount = 0;
            }
        }

        internal void Dispose()
        {
            if (canvasObject != null)
            {
                Object.Destroy(canvasObject);
            }
        }

        internal void SetEnabled(bool value)
        {
            PlayerPrefs.SetInt(EnabledPreference, value ? 1 : 0);
            PlayerPrefs.Save();

            if (!value && canvasObject != null)
            {
                canvasObject.SetActive(false);
            }

            elapsedTime = 0f;
            frameCount = 0;
        }

        private void CreateUi()
        {
            canvasObject = new GameObject("FirewatchCommunityPatch.FpsCounter");
            Object.DontDestroyOnLoad(canvasObject);

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32767;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject textObject = new GameObject("FpsText");
            textObject.transform.SetParent(canvasObject.transform, false);

            RectTransform rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.one;
            rect.anchorMax = Vector2.one;
            rect.pivot = Vector2.one;
            rect.anchoredPosition = new Vector2(-12f, -10f);
            rect.sizeDelta = new Vector2(200f, 30f);

            text = textObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 18;
            text.alignment = TextAnchor.UpperRight;
            text.color = Color.white;
            text.raycastTarget = false;
            text.text = "-- FPS";

            Outline outline = textObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
            outline.effectDistance = new Vector2(1f, -1f);
        }
    }
}
