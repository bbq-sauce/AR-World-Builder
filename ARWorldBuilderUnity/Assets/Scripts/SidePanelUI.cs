using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Slide-in / slide-out side panel.
/// Dynamically spawns one button per registered prefab.
/// </summary>
public class SidePanelUI : MonoBehaviour
{
    [Header("Panel References")]
    [Tooltip("The RectTransform of the panel that slides in/out.")]
    public RectTransform panelRect;

    [Tooltip("Button that toggles the panel open/closed.")]
    public Button toggleButton;

    [Tooltip("Parent transform where object buttons are spawned.")]
    public Transform buttonContainer;

    [Tooltip("Prefab for each selectable-object button.")]
    public GameObject objectButtonPrefab;   // A Button prefab with Image + Text

    [Header("Animation")]
    public float slideDuration = 0.25f;

    // Positions: panel hidden (off-screen right) vs shown
    private Vector2 hiddenPos;
    private Vector2 shownPos;
    private bool isPanelOpen = false;
    private Coroutine slideCoroutine;

    // -------------------------------------------------------
    void Start()
    {
        // Calculate slide positions based on panel width
        float panelWidth = panelRect.rect.width;
        shownPos = panelRect.anchoredPosition;
        hiddenPos = new Vector2(shownPos.x + panelWidth, shownPos.y);

        // Start hidden
        panelRect.anchoredPosition = hiddenPos;

        toggleButton.onClick.AddListener(TogglePanel);
    }

    // -------------------------------------------------------
    /// <summary>
    /// Register all available prefabs — call this from an
    /// ObjectCatalog script or directly from the Inspector.
    /// </summary>
    public void RegisterObjects(ObjectEntry[] entries)
    {
        // Clear old buttons
        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);

        foreach (var entry in entries)
        {
            var btnGO = Instantiate(objectButtonPrefab, buttonContainer);
            var btn = btnGO.GetComponent<Button>();

            // Set label
            var label = btnGO.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (label != null) label.text = entry.displayName;

            // Set icon if available
            var icon = btnGO.transform.Find("Icon")?.GetComponent<Image>();
            if (icon != null && entry.icon != null)
            {
                icon.sprite = entry.icon;
                icon.enabled = true;
            }

            // Capture for lambda
            var capturedPrefab = entry.prefab;
            btn.onClick.AddListener(() =>
            {
                PlacementManager.Instance.SelectPrefab(capturedPrefab);
                ClosePanel(); // optionally close panel after selection
            });
        }
    }

    // -------------------------------------------------------
    public void TogglePanel()
    {
        if (isPanelOpen) ClosePanel(); else OpenPanel();
    }

    public void OpenPanel()
    {
        isPanelOpen = true;
        SlidePanel(shownPos);
        UpdateToggleLabel("✕");
    }

    public void ClosePanel()
    {
        isPanelOpen = false;
        SlidePanel(hiddenPos);
        UpdateToggleLabel("☰");
    }

    // -------------------------------------------------------
    private void SlidePanel(Vector2 target)
    {
        if (slideCoroutine != null) StopCoroutine(slideCoroutine);
        slideCoroutine = StartCoroutine(SlideRoutine(target));
    }

    private IEnumerator SlideRoutine(Vector2 target)
    {
        Vector2 start = panelRect.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / slideDuration);
            panelRect.anchoredPosition = Vector2.Lerp(start, target, t);
            yield return null;
        }

        panelRect.anchoredPosition = target;
    }

    private void UpdateToggleLabel(string text)
    {
        var label = toggleButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (label != null) label.text = text;
    }
}
