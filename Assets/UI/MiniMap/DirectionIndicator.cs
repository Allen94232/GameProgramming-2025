using UnityEngine;

public class DirectionIndicator : MonoBehaviour
{
    [Header("Object Assignments")]
    [SerializeField] private Transform target;         
    [SerializeField] private Transform player;          
    [SerializeField] private Camera minimapCamera;      
    
    [Header("Settings")]
    // Your orange arrow points LEFT. Unity 0 degrees is RIGHT.
    // So we need a 180 offset to correct it.
    [SerializeField] private float rotationOffset = 180f; 

    private RectTransform arrowRect;

    void Start()
    {
        // Get the RectTransform of the object this script is attached to
        arrowRect = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (target == null || minimapCamera == null || player == null) return;

        // 1. Check if target is on screen
        Vector3 targetViewportPos = minimapCamera.WorldToViewportPoint(target.position);
        bool isVisible = targetViewportPos.x > 0 && targetViewportPos.x < 1 &&
                         targetViewportPos.y > 0 && targetViewportPos.y < 1 &&
                         targetViewportPos.z > 0;

        // 2. Show arrow ONLY if target is OFF screen
        if (isVisible)
        {
            // Hide the arrow if we can see the target
            if (gameObject.activeSelf) gameObject.SetActive(false);
        }
        else
        {
            // Show the arrow if target is missing
            if (!gameObject.activeSelf) gameObject.SetActive(true);

            // 3. Calculate Direction
            Vector3 direction = target.position - player.position;

            // 4. Calculate Angle
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // 5. Apply Rotation (Spin the UI Image)
            arrowRect.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
        }
    }
}