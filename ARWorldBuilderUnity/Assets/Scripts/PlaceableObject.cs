using UnityEngine;

public class PlaceableObject : MonoBehaviour
{
    [Header("Identity")]
    [Tooltip("Unique ID auto-assigned at runtime. Leave blank.")]
    public string objectId;

    [Header("Footprint (metres)")]
    [Tooltip("Width (X) and Depth (Z) of the object's base in world-space metres.")]
    public Vector2 footprintMetres = new Vector2(0.2f, 0.2f);

    [Header("Placement Rotation Offset")]
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

    public void ConfirmPlacement(Vector3 worldPos, Quaternion baseRotation)
    {
        isPlaced = true;
        transform.position = worldPos;

        transform.rotation = baseRotation * Quaternion.Euler(placementRotationOffset);

        GridManager.Instance.OccupyCells(objectId, worldPos, footprintMetres);
        SetMaterial(defaultMaterial);
    }
    public void UpdatePreview(Vector3 worldPos, bool canPlace)
    {
        transform.position = worldPos;
        SetMaterial(canPlace ? validPlacementMaterial : invalidPlacementMaterial);
    }

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