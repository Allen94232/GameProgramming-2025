using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public GameObject player; // The player the camera will follow

    [Header("Camera Mode")]
    [Tooltip("Enable look-ahead camera (false = fixed center camera)")]
    public bool enableLookAhead = false;

    [Header("Look Ahead Settings")]
    [Tooltip("Maximum offset distance in front of player based on movement direction")]
    public float lookAheadDistance = 3f;
    [Tooltip("How quickly camera moves to new position (higher = faster)")]
    public float smoothSpeed = 5f;
    [Tooltip("Minimum speed before look-ahead effect activates")]
    public float minSpeedThreshold = 2f;
    [Tooltip("Speed at which look-ahead reaches maximum distance")]
    public float maxSpeedForLookAhead = 20f;

    private Vector3 targetOffset;
    private Rigidbody2D playerRb;
    private float cameraZ;

    void Start()
    {
        if (player != null)
        {
            playerRb = player.GetComponent<Rigidbody2D>();
        }
        cameraZ = transform.position.z;
    }

    void LateUpdate()
    {
        if (player == null) return;

        if (enableLookAhead)
        {
            // Look-ahead camera mode with smooth following
            UpdateLookAheadCamera();
        }
        else
        {
            // Fixed center camera - directly lock to player position
            Vector3 targetPosition = player.transform.position;
            targetPosition.z = cameraZ;
            transform.position = targetPosition;
        }
    }

    void UpdateLookAheadCamera()
    {
        Vector3 basePosition = player.transform.position;
        
        if (playerRb != null)
        {
            // Get player's velocity
            Vector2 velocity = playerRb.linearVelocity;
            float speed = velocity.magnitude;
            
            // Calculate look-ahead offset based on speed and direction
            if (speed > minSpeedThreshold)
            {
                // Normalize speed to 0-1 range
                float speedFactor = Mathf.Clamp01((speed - minSpeedThreshold) / (maxSpeedForLookAhead - minSpeedThreshold));
                
                // Calculate offset in movement direction
                Vector3 movementDirection = velocity.normalized;
                Vector3 newTargetOffset = movementDirection * lookAheadDistance * speedFactor;
                
                // Smooth the offset transition
                targetOffset = Vector3.Lerp(targetOffset, newTargetOffset, Time.deltaTime * smoothSpeed);
            }
            else
            {
                // When stopped or moving slowly, return to center
                targetOffset = Vector3.Lerp(targetOffset, Vector3.zero, Time.deltaTime * smoothSpeed);
            }
        }
        else
        {
            targetOffset = Vector3.zero;
        }
        
        // Calculate target position with offset
        Vector3 targetPosition = basePosition + targetOffset;
        targetPosition.z = cameraZ;
        
        // Smooth movement using Lerp
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);
    }
}
