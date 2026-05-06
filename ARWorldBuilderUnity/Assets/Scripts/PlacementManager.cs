using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;     // Unity 6 uses new Input System

/// <summary>
/// Core placement controller.
/// 1. Detects AR planes via ARRaycastManager.
/// 2. Shows a ghost/preview of the selected prefab.
/// 3. On tap: checks grid availability → places object.
/// 4. Rejects vertical surfaces (walls/ceilings).
/// </summary>
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

    // Reusable raycast hit list — allocated once, never garbage collected per frame
    private static readonly List<ARRaycastHit> hits = new();

    [Header("Reticle")]
    [Tooltip("Assign a simple reticle prefab (e.g., a flat circle) to show where the object will land.")]
    public GameObject reticlePrefab;
    private GameObject reticleInstance;

    // -------------------------------------------------------
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

    // -------------------------------------------------------
    void Update()
    {
        if (!isInPlacementMode || selectedPrefab == null) return;

        Vector2 screenPos = GetPrimaryTouchOrCenter();

        if (raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            // --- Reject walls and ceilings ---
            // hitPose.up is the surface normal.
            // Dot with world Vector3.up:  1 = flat floor,  0 = wall,  -1 = ceiling.
            // minHorizontalDot = 0.85 means we only accept near-horizontal surfaces.
            float upDot = Vector3.Dot(hitPose.up, Vector3.up);
            if (upDot < minHorizontalDot)
            {
                previewInstance?.SetActive(false);
                reticleInstance?.SetActive(false);
                return;
            }

            Vector3 snappedPos = GridManager.Instance.SnapToGrid(hitPose.position);
            bool canPlace = GridManager.Instance.CanPlace(snappedPos, previewPlaceable.footprintMetres);

            // Show reticle on the surface
            if (reticleInstance != null)
            {
                reticleInstance.SetActive(true);
                reticleInstance.transform.position = snappedPos;
                reticleInstance.transform.rotation = hitPose.rotation;
            }

            // Compute the final rotation once — used by BOTH preview and placement.
            // This guarantees the ghost and the placed object always look identical.
            // We only take Y (yaw) so the object stays upright on the floor.
            Quaternion placementRotation = Quaternion.Euler(0f, hitPose.rotation.eulerAngles.y, 0f);

            // Update ghost preview position, rotation and color
            if (previewInstance != null)
            {
                previewInstance.SetActive(true);
                previewInstance.transform.rotation = placementRotation * Quaternion.Euler(previewPlaceable.placementRotationOffset);
                previewPlaceable.UpdatePreview(snappedPos, canPlace);
            }

            // Tap to confirm placement
            if (canPlace && TapDetected())
            {
                PlaceObject(snappedPos, placementRotation);
            }
        }
        else
        {
            // Ray didn't hit any plane — hide preview and reticle
            previewInstance?.SetActive(false);
            reticleInstance?.SetActive(false);
        }
    }

    // -------------------------------------------------------
    /// <summary>Called by SidePanelUI when the user picks an object.</summary>
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

    // -------------------------------------------------------
    public void CancelPlacement()
    {
        isInPlacementMode = false;
        selectedPrefab = null;

        if (previewInstance != null) Destroy(previewInstance);
        previewInstance = null;
        previewPlaceable = null;

        if (reticleInstance != null) reticleInstance.SetActive(false);
    }

    // -------------------------------------------------------
    private void PlaceObject(Vector3 position, Quaternion rotation)
    {
        previewPlaceable.ConfirmPlacement(position, Quaternion.Euler(0f, rotation.eulerAngles.y, 0f));

        // Detach references — the confirmed object now lives independently
        previewInstance = null;
        previewPlaceable = null;

        // Deselect after placing — user must pick from panel again to place another
        CancelPlacement();
    }

    // -------------------------------------------------------
    // Input — Unity 6 New Input System with editor mouse fallback
    // -------------------------------------------------------
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

        // Last resort: screen centre (useful when no input device is active)
        return new Vector2(Screen.width / 2f, Screen.height / 2f);
    }
}