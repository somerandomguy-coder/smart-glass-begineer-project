#if UNITY_EDITOR
using System.IO;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

/// <summary>Creates the basic AR Foundation hierarchy needed by the phone demo.</summary>
public static class PhoneARSceneCreator
{
    private const string ScenePath = "Assets/Scenes/PhoneARPinDemo.unity";

    [MenuItem("Tools/Smart Glass/Create Phone AR Scene")]
    private static void CreatePhoneArScene()
    {
        if (File.Exists(ScenePath) && !EditorUtility.DisplayDialog(
                "Replace Phone AR Scene?",
                "A PhoneARPinDemo scene already exists. Recreate it?",
                "Recreate", "Keep Existing"))
        {
            EditorSceneManager.OpenScene(ScenePath);
            return;
        }

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var sessionObject = new GameObject("AR Session");
        sessionObject.AddComponent<ARSession>();
        sessionObject.AddComponent<ARInputManager>();

        var originObject = new GameObject("XR Origin");
        XROrigin origin = originObject.AddComponent<XROrigin>();
        originObject.AddComponent<ARPlaneManager>();
        originObject.AddComponent<ARRaycastManager>();
        PhoneARPinManager pinManager = originObject.AddComponent<PhoneARPinManager>();

        var cameraOffset = new GameObject("Camera Offset");
        cameraOffset.transform.SetParent(originObject.transform, false);
        origin.CameraFloorOffsetObject = cameraOffset;

        var cameraObject = new GameObject("AR Camera");
        cameraObject.transform.SetParent(cameraOffset.transform, false);
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<ARCameraManager>();
        cameraObject.AddComponent<ARCameraBackground>();
        origin.Camera = camera;
        pinManager.Configure(camera);

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorUtility.DisplayDialog(
            "Phone AR Scene Ready",
            "PhoneARPinDemo has been created. Open Project Settings > XR Plug-in Management > Android and enable ARCore before building.",
            "OK");
    }
}
#endif
