using UnityEditor.Rendering.LookDev;
using UnityEngine;

// -------------------------------------------------------
// Data container for one placeable object type
// -------------------------------------------------------
[System.Serializable]
public class ObjectEntry
{
    public string displayName;
    public GameObject prefab;          // Must have PlaceableObject component
    public Sprite icon;            // Thumbnail shown in side panel
}

// -------------------------------------------------------
/// <summary>
/// Put this on the same GameObject as SidePanelUI (or any
/// Manager object). Assign your prefabs in the Inspector.
/// On Start it registers all entries with the side panel.
/// </summary>
public class ObjectCatalog : MonoBehaviour
{
    [Header("Placeable Objects")]
    [Tooltip("Add all placeable prefabs here. Each must have a PlaceableObject component.")]
    public ObjectEntry[] entries;

    [Header("References")]
    public SidePanelUI sidePanel;

    void Start()
    {
        if (sidePanel != null && entries != null)
            sidePanel.RegisterObjects(entries);
    }
}
