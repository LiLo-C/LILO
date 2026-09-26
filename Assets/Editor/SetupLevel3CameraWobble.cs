using Lilo.MonoBehaviours.Camera;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SetupLevel3CameraWobble
{
    [MenuItem("LILO/Attach Level 3 Camera Wobble")]
    public static void Apply()
    {
        const string scenePath = "Assets/Scenes/OfficeLevel3.unity";
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        UnityEngine.Camera camera = null;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (UnityEngine.Camera candidate in root.GetComponentsInChildren<UnityEngine.Camera>(true))
            {
                if (!candidate.CompareTag("MainCamera"))
                    continue;
                camera = candidate;
                break;
            }
            if (camera != null)
                break;
        }

        if (camera == null)
            throw new System.InvalidOperationException("OfficeLevel3 has no tagged Main Camera.");

        Level3CameraWobble wobble = camera.GetComponent<Level3CameraWobble>();
        if (wobble == null)
            wobble = camera.gameObject.AddComponent<Level3CameraWobble>();

        SerializedObject serializedWobble = new SerializedObject(wobble);
        serializedWobble.FindProperty("inverseTilt").boolValue = true;
        serializedWobble.FindProperty("tiltSensitivity").floatValue = 0.8f;
        serializedWobble.FindProperty("maximumTiltDegrees").floatValue = 9f;
        serializedWobble.FindProperty("rollWobbleDegrees").floatValue = 11.8125f;
        serializedWobble.FindProperty("pitchWobbleDegrees").floatValue = 5.90625f;
        serializedWobble.FindProperty("wobbleIntensity").floatValue = 0.75f;
        serializedWobble.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Level3Camera] Applied inverse phone tilt and reduced wobble to OfficeLevel3 Main Camera.");
    }
}
