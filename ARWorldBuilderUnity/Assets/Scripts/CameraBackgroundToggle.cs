using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(Camera))]
public class CameraBackgroundToggle : MonoBehaviour
{
    void Awake()
    {
        var arBackground = GetComponent<ARCameraBackground>();
        var cam = GetComponent<Camera>();

#if UNITY_EDITOR
        if (arBackground != null) arBackground.enabled = false;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.Skybox;
        }
#else
        // On real device — enable camera background
        if (arBackground != null) arBackground.enabled = true;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
        }
#endif
    }
}