using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

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
        headTransform = FindXrealCameraCenter() ?? Camera.main.transform;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || IsXrealTriggerPressed())
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

    private static Transform FindXrealCameraCenter()
    {
        Type inputType = FindType("NRKernal.NRInput");
        PropertyInfo cameraCenter = inputType?.GetProperty("CameraCenter", BindingFlags.Public | BindingFlags.Static);
        return cameraCenter?.GetValue(null) as Transform;
    }

    private static bool IsXrealTriggerPressed()
    {
        Type inputType = FindType("NRKernal.NRInput");
        Type buttonType = FindType("NRKernal.ControllerButton");
        MethodInfo getButtonDown = inputType?.GetMethod("GetButtonDown", BindingFlags.Public | BindingFlags.Static);
        if (buttonType == null || getButtonDown == null)
        {
            return false;
        }

        try
        {
            object trigger = Enum.Parse(buttonType, "TRIGGER");
            return (bool)getButtonDown.Invoke(null, new[] { trigger });
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static Type FindType(string fullName)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType(fullName, false))
            .FirstOrDefault(type => type != null);
    }
}
