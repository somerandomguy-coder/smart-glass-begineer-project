using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR;
using XrCommonUsages = UnityEngine.XR.CommonUsages;
using XrInputDevice = UnityEngine.XR.InputDevice;

/// <summary>Creates a small note one-and-a-half metres along the user's gaze.</summary>
public sealed class SpatialPinManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField, Min(0.1f)] private float spawnDistance = 1.5f;
    [SerializeField] private Color noteColor = new Color(0.08f, 0.1f, 0.14f, 0.94f);

    private Transform headTransform;
    private Text statusText;
    private int pinCount;

    private void Start()
    {
        EnsureEditorCamera();
        // The XREAL SDK exposes its headset camera through Unity XR; the XR
        // Origin camera should be tagged MainCamera by the SDK setup flow.
        headTransform = Camera.main.transform;
        CreateHeadUpDisplay();
    }

    private void Update()
    {
        if ((Keyboard.current?.spaceKey.wasPressedThisFrame ?? false) || IsXrTriggerPressed())
        {
            SpawnPin();
        }
    }

    private void SpawnPin()
    {
        if (headTransform == null)
        {
            return;
        }

        Vector3 position = headTransform.position + headTransform.forward * spawnDistance;
        GameObject note = CreateNoteCard($"Spatial Pin #{++pinCount}");
        // A world-space Canvas renders its visible side toward local -Z. Point
        // local +Z away from the user so the text faces the user's gaze.
        note.transform.SetPositionAndRotation(position, Quaternion.LookRotation(position - headTransform.position));
        SetStatus($"PIN {pinCount} PLACED  •  LOOK AROUND");
    }

    /// <summary>
    /// Adds simple glasses-style feedback that is useful in both the Editor
    /// simulator and an Android/XREAL build. It is created in code so there is
    /// no prefab wiring for a first-time Unity user to manage.
    /// </summary>
    private void CreateHeadUpDisplay()
    {
        var canvasObject = new GameObject("Smart Glass HUD", typeof(Canvas), typeof(CanvasScaler));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        var reticle = new GameObject("Aim Reticle", typeof(RectTransform), typeof(Image));
        reticle.transform.SetParent(canvasObject.transform, false);
        RectTransform reticleRect = reticle.GetComponent<RectTransform>();
        reticleRect.anchorMin = reticleRect.anchorMax = new Vector2(0.5f, 0.5f);
        reticleRect.sizeDelta = new Vector2(12f, 12f);
        reticle.GetComponent<Image>().color = Color.cyan;

        var status = new GameObject("Placement Status", typeof(RectTransform), typeof(Text));
        status.transform.SetParent(canvasObject.transform, false);
        RectTransform statusRect = status.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0.5f, 0.5f);
        statusRect.anchorMax = new Vector2(0.5f, 0.5f);
        statusRect.anchoredPosition = new Vector2(0f, -82f);
        statusRect.sizeDelta = new Vector2(900f, 80f);

        statusText = status.GetComponent<Text>();
        statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        statusText.fontSize = 27;
        statusText.fontStyle = FontStyle.Bold;
        statusText.alignment = TextAnchor.MiddleCenter;
        statusText.color = new Color(0.4f, 1f, 1f, 0.92f);
        SetStatus("PIN READY  •  SPACE / CONTROLLER TRIGGER");
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    private GameObject CreateNoteCard(string message)
    {
        var root = new GameObject(message, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;

        RectTransform rect = root.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(400f, 250f);
        rect.localScale = Vector3.one * 0.002f;

        var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(root.transform, false);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = noteColor;

        var label = new GameObject("Label", typeof(RectTransform), typeof(Text));
        label.transform.SetParent(panel.transform, false);
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(28f, 28f);
        labelRect.offsetMax = new Vector2(-28f, -28f);
        Text text = label.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = message;
        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = 38;
        text.color = Color.cyan;
        return root;
    }

    private static void EnsureEditorCamera()
    {
        if (Camera.main != null)
        {
            return;
        }

        var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetPositionAndRotation(new Vector3(0f, 1.6f, 0f), Quaternion.identity);
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.backgroundColor = new Color(0.015f, 0.02f, 0.04f);
        camera.clearFlags = CameraClearFlags.SolidColor;

#if UNITY_EDITOR
        // The temporary camera exists only to make the starter scene testable
        // before an XREAL XR Origin has been added.
        cameraObject.AddComponent<EditorFlyCameraController>();
#endif
    }

    private static bool IsXrTriggerPressed()
    {
        var controllers = new List<XrInputDevice>();
        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.HeldInHand,
            controllers);

        foreach (XrInputDevice controller in controllers)
        {
            if (controller.TryGetFeatureValue(XrCommonUsages.triggerButton, out bool pressed) && pressed)
            {
                return true;
            }
        }

        return false;
    }
}
