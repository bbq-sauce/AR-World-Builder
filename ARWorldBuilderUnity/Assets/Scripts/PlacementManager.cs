using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;   


[RequireComponent(typeof(ARRaycastManager))]
public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance { get; private set; }

    [Header("AR References")]
    private ARRaycastManager raycastManager;
    private ARPlaneManager planeManager;

    [Header("Placement State")]
    private GameObject previewInstance;
    private PlaceableObject previewPlaceable;
    private GameObject selectedPrefab;
    private bool isInPlacementMode = false;

    [Header("Surface Filter")]
    [Tooltip("How horizontal a surface must be before placement is allowed. " +
             "1 = perfectly flat, 0 = any angle. 0.85 blocks walls but allows slight slopes.")]
    [Range(0f, 1f)]
    public float minHorizontalDot = 0.85f;
    private static readonly List<ARRaycastHit> hits = new();

    [Header("Reticle")]
    [Tooltip("Assign a simple reticle prefab (e.g., a flat circle) to show where the object will land.")]
    public GameObject reticlePrefab;
    private GameObject reticleInstance;

   
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        raycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();
    }

    void Start()
    {
        if (reticlePrefab != null)
        {
            reticleInstance = Instantiate(reticlePrefab);
            reticleInstance.SetActive(false);
        }
    }

  
    void Update()
    {
        if (!isInPlacementMode || selectedPrefab == null) return;

        Vector2 screenPos = GetPrimaryTouchOrCenter();

        if (raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            float upDot = Vector3.Dot(hitPose.up, Vector3.up);
            if (upDot < minHorizontalDot)
            {
                previewInstance?.SetActive(false);
                reticleInstance?.SetActive(false);
                return;
            }

            Vector3 snappedPos = GridManager.Instance.SnapToGrid(hitPose.position);
            bool canPlace = GridManager.Instance.CanPlace(snappedPos, previewPlaceable.footprintMetres);

            Quaternion placementRotation = Quaternion.Euler(0f, hitPose.rotation.eulerAngles.y, 0f);

            if (previewInstance != null)
            {
                previewInstance.SetActive(true);
                previewInstance.transform.rotation = placementRotation * Quaternion.Euler(previewPlaceable.placementRotationOffset);
                previewPlaceable.UpdatePreview(snappedPos, canPlace);
            }

            if (canPlace && TapDetected())
            {
                PlaceObject(snappedPos, placementRotation);
            }
        }
        else
        {
            previewInstance?.SetActive(false);
            reticleInstance?.SetActive(false);
        }
    }

    public void SelectPrefab(GameObject prefab)
    {
        CancelPlacement();

        selectedPrefab = prefab;
        isInPlacementMode = true;

        previewInstance = Instantiate(prefab);
        previewPlaceable = previewInstance.GetComponent<PlaceableObject>();

        if (previewPlaceable == null)
        {
            Debug.LogError($"[PlacementManager] Prefab '{prefab.name}' is missing a PlaceableObject component!");
            CancelPlacement();
        }
    }
    public void CancelPlacement()
    {
        isInPlacementMode = false;
        selectedPrefab = null;

        if (previewInstance != null) Destroy(previewInstance);
        previewInstance = null;
        previewPlaceable = null;

        if (reticleInstance != null) reticleInstance.SetActive(false);
    }

    private void PlaceObject(Vector3 position, Quaternion rotation)
    {
        previewPlaceable.ConfirmPlacement(position, Quaternion.Euler(0f, rotation.eulerAngles.y, 0f));
        previewInstance = null;
        previewPlaceable = null;
        CancelPlacement();
    }
    
    private bool TapDetected()
    {
        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;
            return touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began;
        }
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
    }

    private Vector2 GetPrimaryTouchOrCenter()
    {
        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;
            if (touch.press.isPressed)
                return touch.position.ReadValue();
        }
        if (Mouse.current != null)
            return Mouse.current.position.ReadValue();

        return new Vector2(Screen.width / 2f, Screen.height / 2f);
    }
}