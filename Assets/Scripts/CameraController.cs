using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public GameObject player; // The player the camera will follow

    [Header("Look Ahead Settings")]
    [Tooltip("Maximum offset distance in front of player based on movement direction")]
    public float lookAheadDistance = 3f;
    [Tooltip("How quickly camera moves to new position (higher = faster)")]
    public float smoothSpeed = 5f;
    [Tooltip("Minimum speed before look-ahead effect activates")]
    public float minSpeedThreshold = 2f;
    [Tooltip("Speed at which look-ahead reaches maximum distance")]
    public float maxSpeedForLookAhead = 20f;

    private Vector3 currentVelocity;
    private Vector3 targetOffset;

    void LateUpdate()
    {
        if (player == null) return;

        // Get player's Rigidbody2D to detect movement direction
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        
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
                targetOffset = movementDirection * lookAheadDistance * speedFactor;
            }
            else
            {
                // When stopped or moving slowly, return to center
                targetOffset = Vector3.Lerp(targetOffset, Vector3.zero, Time.deltaTime * smoothSpeed * 0.5f);
            }
        }
        else
        {
            targetOffset = Vector3.zero;
        }
        
        // Calculate target position with offset
        Vector3 targetPosition = basePosition + targetOffset;
        targetPosition.z = transform.position.z;
        
        // Smoothly move camera to target position
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, 1f / smoothSpeed);
    }
}
