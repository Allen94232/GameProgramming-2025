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
    [SerializeField] private float padding = 15f;

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

        Vector3 targetViewportPos = minimapCamera.WorldToViewportPoint(target.position);

        bool isVisible = targetViewportPos.x >= 0 && targetViewportPos.x <= 1 &&
                         targetViewportPos.y >= 0 && targetViewportPos.y <= 1 &&
                         targetViewportPos.z > 0;

        if (isVisible)
        {

            if (arrowImage.enabled) arrowImage.enabled = false;
            return;
        }
        else
        {
            if (!arrowImage.enabled) arrowImage.enabled = true;
        }

        Vector2 directionFromCenter = new Vector2(targetViewportPos.x - 0.5f, targetViewportPos.y - 0.5f);

        float angle = Mathf.Atan2(directionFromCenter.y, directionFromCenter.x) * Mathf.Rad2Deg;
        arrowRect.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);

        Vector2 clampedPos = directionFromCenter;
        float maxDist = Mathf.Max(Mathf.Abs(directionFromCenter.x), Mathf.Abs(directionFromCenter.y));
        
        if (maxDist > 0)
        {
            clampedPos = (directionFromCenter / maxDist) * 0.5f;
        }

        float width = minimapRect.rect.width;
        float height = minimapRect.rect.height;

        Vector2 screenPos = new Vector2(
            clampedPos.x * width, 
            clampedPos.y * height
        );

        screenPos.x = Mathf.Clamp(screenPos.x, -width/2 + padding, width/2 - padding);
        screenPos.y = Mathf.Clamp(screenPos.y, -height/2 + padding, height/2 - padding);

        arrowRect.anchoredPosition = screenPos;
    }
}