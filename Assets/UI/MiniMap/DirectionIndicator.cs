using UnityEngine;
using UnityEngine.UI;

public class DirectionIndicator : MonoBehaviour
{
    [Header("Object Assignments")]
    [SerializeField] private Transform target;         
    [SerializeField] private Transform player;          
    [SerializeField] private Camera minimapCamera;
    [SerializeField] private RectTransform minimapRect;      
    
    [Header("Settings")]
    [SerializeField] private float rotationOffset = -90f;
    [SerializeField] private float indicatorRadius = 50f; // Radius of circle around player
    [Tooltip("Minimum distance from minimap edge")]
    [SerializeField] private float edgePadding = 15f;

    private RectTransform arrowRect;
    private Image arrowImage;

    void Start()
    {
        arrowRect = GetComponent<RectTransform>();
        arrowImage = GetComponent<Image>();
    }

    void Update()
    {
        if (target == null || minimapCamera == null || player == null || minimapRect == null || arrowImage == null) return;

        // Check if target is visible on minimap
        Vector3 targetViewportPos = minimapCamera.WorldToViewportPoint(target.position);

        bool isVisible = targetViewportPos.x >= 0 && targetViewportPos.x <= 1 &&
                         targetViewportPos.y >= 0 && targetViewportPos.y <= 1 &&
                         targetViewportPos.z > 0;

        if (isVisible)
        {
            // Target is visible on minimap, hide indicator
            if (arrowImage.enabled) arrowImage.enabled = false;
            return;
        }
        else
        {
            // Target is outside minimap, show indicator
            if (!arrowImage.enabled) arrowImage.enabled = true;
        }

        // Calculate direction from player to target in world space
        Vector3 directionToTarget = (target.position - player.position).normalized;
        
        // Calculate angle for arrow rotation
        float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
        arrowRect.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);

        // Get player position in viewport coordinates
        Vector3 playerViewportPos = minimapCamera.WorldToViewportPoint(player.position);
        
        // Convert player viewport position to minimap UI coordinates (centered at 0,0)
        float width = minimapRect.rect.width;
        float height = minimapRect.rect.height;
        
        Vector2 playerScreenPos = new Vector2(
            (playerViewportPos.x - 0.5f) * width,
            (playerViewportPos.y - 0.5f) * height
        );

        // Calculate indicator position on circle around player
        // Use 2D direction (x and y from world space direction)
        Vector2 direction2D = new Vector2(directionToTarget.x, directionToTarget.y).normalized;
        Vector2 indicatorOffset = direction2D * indicatorRadius;
        
        // Final position = player position + circular offset
        Vector2 finalPos = playerScreenPos + indicatorOffset;

        // Clamp to minimap boundaries with padding
        finalPos.x = Mathf.Clamp(finalPos.x, -width/2 + edgePadding, width/2 - edgePadding);
        finalPos.y = Mathf.Clamp(finalPos.y, -height/2 + edgePadding, height/2 - edgePadding);

        arrowRect.anchoredPosition = finalPos;
    }
}