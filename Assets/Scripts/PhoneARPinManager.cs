using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Phone version of the spatial-pin experience. ARCore reports detected
/// surfaces, the centre reticle shows whether the phone is aiming at one, and
/// a tap places a note at that real-world pose for the current AR session.
/// </summary>
[RequireComponent(typeof(ARRaycastManager))]
public sealed class PhoneARPinManager : MonoBehaviour
{
    private static readonly List<ARRaycastHit> Hits = new();

    [SerializeField] private Camera arCamera;
    [SerializeField] private Color noteColor = new(0.08f, 0.1f, 0.14f, 0.94f);

    private ARRaycastManager raycastManager;
    private Image reticleImage;
    private Text statusText;
    private int pinCount;

    private void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        if (arCamera == null)
        {
            arCamera = Camera.main;
        }
    }

    private void Start()
    {
        CreateHeadUpDisplay();
    }

    private void Update()
    {
        if (arCamera == null)
        {
            return;
        }

        Vector2 centreScreenPoint = new(Screen.width * 0.5f, Screen.height * 0.5f);
        bool hasSurfaceAtCentre = TryGetPlacementPose(centreScreenPoint, out _);
        SetReticleState(hasSurfaceAtCentre);

        Touchscreen touchscreen = Touchscreen.current;
        if (touchscreen == null || !touchscreen.primaryTouch.press.wasPressedThisFrame)
        {
            return;
        }

        Vector2 tapPosition = touchscreen.primaryTouch.position.ReadValue();
        if (TryGetPlacementPose(tapPosition, out Pose placementPose))
        {
            PlacePin(placementPose);
        }
        else
        {
            SetStatus("MOVE SLOWLY UNTIL A SURFACE IS FOUND");
        }
    }

    /// <summary>Called by the scene-creation tool to wire up the AR camera.</summary>
    public void Configure(Camera camera)
    {
        arCamera = camera;
    }

    private bool TryGetPlacementPose(Vector2 screenPosition, out Pose pose)
    {
        Hits.Clear();
        if (raycastManager.Raycast(screenPosition, Hits, TrackableType.PlaneWithinPolygon))
        {
            pose = Hits[0].pose;
            return true;
        }

        pose = default;
        return false;
    }

    private void PlacePin(Pose placementPose)
    {
        Vector3 position = placementPose.position + Vector3.up * 0.02f;
        GameObject note = CreateNoteCard($"Spatial Pin #{++pinCount}");

        // A world-space Canvas has its visible UI side facing local -Z. Put
        // local +Z away from the phone so the note faces the user.
        note.transform.SetPositionAndRotation(position, Quaternion.LookRotation(position - arCamera.transform.position));
        SetStatus($"PIN {pinCount} PLACED");
    }

    private void CreateHeadUpDisplay()
    {
        var canvasObject = new GameObject("Phone AR HUD", typeof(Canvas), typeof(CanvasScaler));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);

        var reticle = new GameObject("Surface Reticle", typeof(RectTransform), typeof(Image));
        reticle.transform.SetParent(canvasObject.transform, false);
        RectTransform reticleRect = reticle.GetComponent<RectTransform>();
        reticleRect.anchorMin = reticleRect.anchorMax = new Vector2(0.5f, 0.5f);
        reticleRect.sizeDelta = new Vector2(24f, 24f);
        reticleImage = reticle.GetComponent<Image>();

        var status = new GameObject("Placement Status", typeof(RectTransform), typeof(Text));
        status.transform.SetParent(canvasObject.transform, false);
        RectTransform statusRect = status.GetComponent<RectTransform>();
        statusRect.anchorMin = statusRect.anchorMax = new Vector2(0.5f, 0.5f);
        statusRect.anchoredPosition = new Vector2(0f, -115f);
        statusRect.sizeDelta = new Vector2(920f, 110f);

        statusText = status.GetComponent<Text>();
        statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        statusText.fontSize = 32;
        statusText.fontStyle = FontStyle.Bold;
        statusText.alignment = TextAnchor.MiddleCenter;
        statusText.color = new Color(0.4f, 1f, 1f, 0.92f);
        SetStatus("SCAN A FLOOR OR TABLE, THEN TAP TO PIN");
        SetReticleState(false);
    }

    private void SetReticleState(bool canPlace)
    {
        if (reticleImage != null)
        {
            reticleImage.color = canPlace ? new Color(0.25f, 1f, 0.45f) : new Color(1f, 0.75f, 0.2f);
        }
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
        canvas.worldCamera = arCamera;

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
}
