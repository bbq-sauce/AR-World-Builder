using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARPlaneManager))]
public class ARPlaneVisualizerToggle : MonoBehaviour
{
    private ARPlaneManager planeManager;
    private bool planesVisible = true;

    void Awake() => planeManager = GetComponent<ARPlaneManager>();

    void OnEnable() => planeManager.trackablesChanged.AddListener(OnPlanesChanged);
    void OnDisable() => planeManager.trackablesChanged.RemoveListener(OnPlanesChanged);
    private void OnPlanesChanged(ARTrackablesChangedEventArgs<ARPlane> args)
    {
        foreach (var plane in args.added)
            SetPlaneVisible(plane, planesVisible);
    }

    public void ShowPlanes()
    {
        planesVisible = true;
        foreach (var plane in planeManager.trackables)
            SetPlaneVisible(plane, true);
    }

    public void HidePlanes()
    {
        planesVisible = false;
        foreach (var plane in planeManager.trackables)
            SetPlaneVisible(plane, false);
    }

    private void SetPlaneVisible(ARPlane plane, bool visible)
    {
        var meshRenderer = plane.GetComponent<MeshRenderer>();
        if (meshRenderer != null) meshRenderer.enabled = visible;
    }
}
