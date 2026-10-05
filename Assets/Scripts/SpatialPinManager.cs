using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR;

/// <summary>Creates a small note one-and-a-half metres along the user's gaze.</summary>
public sealed class SpatialPinManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField, Min(0.1f)] private float spawnDistance = 1.5f;
    [SerializeField] private Color noteColor = new Color(0.08f, 0.1f, 0.14f, 0.94f);

    private Transform headTransform;
    private int pinCount;

    private void Start()
    {
        EnsureEditorCamera();
        // The XREAL SDK exposes its headset camera through Unity XR; the XR
        // Origin camera should be tagged MainCamera by the SDK setup flow.
        headTransform = Camera.main.transform;
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
        // A Canvas renders from its local-forward side, so point it back at the user.
        note.transform.SetPositionAndRotation(position, Quaternion.LookRotation(headTransform.position - position));
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
    }

    private static bool IsXrTriggerPressed()
    {
        var controllers = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.HeldInHand,
            controllers);

        foreach (InputDevice controller in controllers)
        {
            if (controller.TryGetFeatureValue(CommonUsages.triggerButton, out bool pressed) && pressed)
            {
                return true;
            }
        }

        return false;
    }
}
