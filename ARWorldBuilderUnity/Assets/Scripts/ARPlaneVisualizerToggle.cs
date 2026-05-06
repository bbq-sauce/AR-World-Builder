using UnityEngine;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// Shows detected AR planes while the user is scanning.
/// Hides plane visuals once the first object has been placed
/// (keeps detection active; only the mesh renderer is hidden).
/// </summary>
[RequireComponent(typeof(ARPlaneManager))]
public class ARPlaneVisualizerToggle : MonoBehaviour
{
    private ARPlaneManager planeManager;
    private bool planesVisible = true;

    void Awake() => planeManager = GetComponent<ARPlaneManager>();

    void OnEnable() => planeManager.trackablesChanged.AddListener(OnPlanesChanged);
    void OnDisable() => planeManager.trackablesChanged.RemoveListener(OnPlanesChanged);

    // Called when new planes are added/updated/removed
    private void OnPlanesChanged(ARTrackablesChangedEventArgs<ARPlane> args)
    {
        // Make sure newly detected planes respect current visibility state
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
