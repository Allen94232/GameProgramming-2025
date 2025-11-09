using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class SprinklerButton : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the WaterSprayTrigger component (can be on same GameObject or child)")]
    public WaterSprayTrigger waterSprayTrigger;

    [Header("Click Cooldown Settings")]
    [Tooltip("Time in seconds before button can be clicked again")]
    public float clickCooldown = 1f;
    private float lastClickTime = -999f; // Initialize to allow first click immediately

    [Header("Visual Feedback")]
    public SpriteRenderer buttonRenderer;
    public Sprite onSprite;
    public Sprite offSprite;
    public Color onColor = Color.green;
    public Color offColor = Color.red;
    
    [Header("Optional: Cooldown Visual")]
    [Tooltip("Color to show when button is on cooldown")]
    public Color cooldownColor = Color.gray;
    
    [Header("Optional: Water Visual Effect")]
    public GameObject waterVisualEffect;
    
    private Camera cam;
    private Collider2D buttonCollider;
    private bool isOnCooldown = false;

    void Start()
    {
        cam = Camera.main;
        buttonCollider = GetComponent<Collider2D>();
        
        // Force physics refresh with delay to ensure it's clickable
        StartCoroutine(RefreshColliderWithDelay());
        
        // Auto-find WaterSprayTrigger if not assigned
        if (waterSprayTrigger == null)
        {
            waterSprayTrigger = GetComponent<WaterSprayTrigger>();
            
            // If not on same GameObject, try to find in children
            if (waterSprayTrigger == null)
            {
                waterSprayTrigger = GetComponentInChildren<WaterSprayTrigger>();
            }
            
            // If still not found, try to find in parent
            if (waterSprayTrigger == null)
            {
                waterSprayTrigger = GetComponentInParent<WaterSprayTrigger>();
            }
        }
        
        // Auto-find SpriteRenderer if not assigned
        if (buttonRenderer == null)
        {
            buttonRenderer = GetComponent<SpriteRenderer>();
        }
        
        // Validate references
        if (waterSprayTrigger == null)
        {
            Debug.LogError($"SprinklerButton on {gameObject.name}: WaterSprayTrigger reference not found!");
        }
        
        // Initialize visual state
        UpdateVisualState();
    }

    // Refresh collider after waiting for physics to settle
    private IEnumerator RefreshColliderWithDelay()
    {
        if (buttonCollider != null)
        {
            buttonCollider.enabled = false;
            
            // Wait for physics update
            yield return new WaitForFixedUpdate();
            
            // Wait for end of frame
            yield return new WaitForEndOfFrame();
            
            // Wait one more frame
            yield return null;
            
            buttonCollider.enabled = true;
            
            // Force physics sync
            Physics2D.SyncTransforms();
            
            //Debug.Log($"SprinklerButton {gameObject.name}: Collider refreshed with full delay");
        }
    }

    void OnMouseDown()
    {
        // Check if button is on cooldown
        if (isOnCooldown)
        {
            float remainingTime = clickCooldown - (Time.time - lastClickTime);
            Debug.Log($"SprinklerButton on {gameObject.name}: On cooldown. Wait {remainingTime:F1} more seconds.");
            return;
        }

        // Check if we have a valid reference
        if (waterSprayTrigger == null)
        {
            Debug.LogError($"SprinklerButton on {gameObject.name}: Cannot toggle - WaterSprayTrigger is null!");
            return;
        }
        
        // Toggle water spray
        waterSprayTrigger.ToggleWaterSpray();

        // Start cooldown
        StartCooldown();

        Debug.Log($"Sprinkler button clicked! Water spray is now: {(waterSprayTrigger.isOpening ? "ON" : "OFF")}");
    }

    // Start the cooldown timer
    private void StartCooldown()
    {
        lastClickTime = Time.time;
        isOnCooldown = true;
        
        // Start cooldown coroutine
        StartCoroutine(CooldownCoroutine());
    }

    // Cooldown coroutine
    private IEnumerator CooldownCoroutine()
    {
        // Show cooldown visual feedback
        if (buttonRenderer != null)
        {
            Color originalColor = buttonRenderer.color;
            buttonRenderer.color = cooldownColor;
            
            // Wait for cooldown duration
            yield return new WaitForSeconds(clickCooldown);
            
            // Restore original color
            isOnCooldown = false;
            UpdateVisualState();
        }
        else
        {
            // If no renderer, just wait
            yield return new WaitForSeconds(clickCooldown);
            isOnCooldown = false;
        }
        
        Debug.Log($"SprinklerButton {gameObject.name}: Cooldown finished, ready to click again!");
    }

    // Update visual feedback based on water spray state
    private void UpdateVisualState()
    {
        if (waterSprayTrigger == null) return;
        
        // Don't update visual if on cooldown
        if (isOnCooldown) return;
        
        bool isOn = waterSprayTrigger.isOpening;
        
        // Update sprite if both sprites are assigned
        if (buttonRenderer != null)
        {
            if (onSprite != null && offSprite != null)
            {
                buttonRenderer.sprite = isOn ? onSprite : offSprite;
            }
            
            // Update color
            buttonRenderer.color = isOn ? onColor : offColor;
        }
        
        // Toggle water visual effect
        if (waterVisualEffect != null)
        {
            waterVisualEffect.SetActive(isOn);
        }
    }

    // Optional: Update visual state every frame if state changes externally
    void Update()
    {
        // Only update if state might have changed externally and not on cooldown
        if (!isOnCooldown && waterSprayTrigger != null && buttonRenderer != null)
        {
            bool isOn = waterSprayTrigger.isOpening;
            Color targetColor = isOn ? onColor : offColor;
            
            // Only update if color is different
            if (buttonRenderer.color != targetColor)
            {
                UpdateVisualState();
            }
        }
    }

    // Public method to manually update visual state (useful for reset/initialization)
    public void RefreshVisualState()
    {
        UpdateVisualState();
    }

    // Helper method to check current state
    public bool IsWaterSprayOn()
    {
        return waterSprayTrigger != null && waterSprayTrigger.isOpening;
    }

    // Check if button is currently on cooldown
    public bool IsOnCooldown()
    {
        return isOnCooldown;
    }

    // Get remaining cooldown time
    public float GetRemainingCooldownTime()
    {
        if (!isOnCooldown) return 0f;
        return Mathf.Max(0f, clickCooldown - (Time.time - lastClickTime));
    }
}
