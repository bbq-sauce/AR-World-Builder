using UnityEngine;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// Disables ARCameraBackground in editor/simulation so the XR Simulation
/// environment is visible. On a real device it stays enabled for the camera feed.
/// Attach to Main Camera.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraBackgroundToggle : MonoBehaviour
{
    void Awake()
    {
        var arBackground = GetComponent<ARCameraBackground>();

#if UNITY_EDITOR
        // In editor, ARCameraBackground has no real feed — it renders solid yellow.
        // Disable it so XR Simulation's camera renders through instead.
        if (arBackground != null)
        {
            arBackground.enabled = false;
            Debug.Log("[CameraBackgroundToggle] ARCameraBackground disabled for editor simulation.");
        }

        // Also force the camera clear flags to show the simulation environment
        var cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.clear;
        }
#else
        // On a real device, make sure it is enabled for the live camera feed
        if (arBackground != null)
            arBackground.enabled = true;
#endif
    }
}