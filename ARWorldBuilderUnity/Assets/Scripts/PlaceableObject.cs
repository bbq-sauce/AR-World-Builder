using UnityEngine;

/// <summary>
/// Attach this to every prefab that can be placed in the AR world.
/// Define its footprint so the GridManager knows how many cells it needs.
/// </summary>
public class PlaceableObject : MonoBehaviour
{
    [Header("Identity")]
    [Tooltip("Unique ID auto-assigned at runtime. Leave blank.")]
    public string objectId;

    [Header("Footprint (metres)")]
    [Tooltip("Width (X) and Depth (Z) of the object's base in world-space metres.")]
    public Vector2 footprintMetres = new Vector2(0.2f, 0.2f);

    [Header("Placement Rotation Offset")]
    [Tooltip("Euler angle offset applied on top of the placement rotation. " +
             "If your model rotates -90 on X when placed, set this to (90, 0, 0). " +
             "Tweak in Play Mode until the placed object matches the preview.")]
    public Vector3 placementRotationOffset = Vector3.zero;

    [Header("Visual Feedback")]
    public Material validPlacementMaterial;
    public Material invalidPlacementMaterial;
    public Material defaultMaterial;

    private Renderer[] renderers;
    private bool isPlaced = false;

    void Awake()
    {
        objectId = System.Guid.NewGuid().ToString();
        renderers = GetComponentsInChildren<Renderer>();
    }

    public bool IsPlaced => isPlaced;

    /// <summary>Called by PlacementManager to confirm final placement.</summary>
    public void ConfirmPlacement(Vector3 worldPos, Quaternion baseRotation)
    {
        isPlaced = true;
        transform.position = worldPos;

        // Apply the base placement rotation then add the per-prefab offset.
        // This corrects FBX axis mismatches without touching the import settings.
        transform.rotation = baseRotation * Quaternion.Euler(placementRotationOffset);

        GridManager.Instance.OccupyCells(objectId, worldPos, footprintMetres);
        SetMaterial(defaultMaterial);
    }

    /// <summary>Called every frame while the object is being previewed.</summary>
    public void UpdatePreview(Vector3 worldPos, bool canPlace)
    {
        transform.position = worldPos;
        SetMaterial(canPlace ? validPlacementMaterial : invalidPlacementMaterial);
    }

    /// <summary>Remove from scene and free grid cells.</summary>
    public void RemoveFromWorld()
    {
        GridManager.Instance.FreeCells(objectId);
        Destroy(gameObject);
    }

    private void SetMaterial(Material mat)
    {
        if (mat == null) return;
        foreach (var r in renderers) r.material = mat;
    }
}