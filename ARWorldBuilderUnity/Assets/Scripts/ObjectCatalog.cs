using UnityEngine;

[System.Serializable]
public class ObjectEntry
{
    public string displayName;
    public GameObject prefab;      
    public Sprite icon;           
}

public class ObjectCatalog : MonoBehaviour
{
    [Header("Placeable Objects")]
    public ObjectEntry[] entries;

    [Header("References")]
    public SidePanelUI sidePanel;

    void Start()
    {
        if (sidePanel != null && entries != null)
            sidePanel.RegisterObjects(entries);
    }
}
