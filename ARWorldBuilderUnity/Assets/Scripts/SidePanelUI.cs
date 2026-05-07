using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SidePanelUI : MonoBehaviour
{
    [Header("Panel References")]
    public RectTransform panelRect;
    public Button toggleButton;

    public Transform buttonContainer;

    public GameObject objectButtonPrefab;   

    [Header("Animation")]
    public float slideDuration = 0.25f;

    private Vector2 hiddenPos;
    private Vector2 shownPos;
    private bool isPanelOpen = false;
    private Coroutine slideCoroutine;

    
    void Start()
    {
        float panelWidth = panelRect.rect.width;
        shownPos = panelRect.anchoredPosition;
        hiddenPos = new Vector2(shownPos.x + panelWidth, shownPos.y);

        panelRect.anchoredPosition = hiddenPos;

        toggleButton.onClick.AddListener(TogglePanel);
    }

    public void RegisterObjects(ObjectEntry[] entries)
    {
        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);

        foreach (var entry in entries)
        {
            var btnGO = Instantiate(objectButtonPrefab, buttonContainer);
            var btn = btnGO.GetComponent<Button>();

            var label = btnGO.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (label != null) label.text = entry.displayName;

            var icon = btnGO.transform.Find("Icon")?.GetComponent<Image>();
            if (icon != null && entry.icon != null)
            {
                icon.sprite = entry.icon;
                icon.enabled = true;
            }

            var capturedPrefab = entry.prefab;
            btn.onClick.AddListener(() =>
            {
                PlacementManager.Instance.SelectPrefab(capturedPrefab);
                ClosePanel(); 
            });
        }
    }

    public void TogglePanel()
    {
        if (isPanelOpen) ClosePanel(); else OpenPanel();
    }

    public void OpenPanel()
    {
        isPanelOpen = true;
        SlidePanel(shownPos);
    }

    public void ClosePanel()
    {
        isPanelOpen = false;
        SlidePanel(hiddenPos);
    }

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
}
