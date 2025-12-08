using UnityEngine;

public class SpeedDetector : MonoBehaviour
{
    public SpeedTextController speedTextController;
    public float speedMultiplier = 0.8f;
    
    [Header("Cover State")]
    [Tooltip("Whether detector is covered by spray paint")]
    public bool isCovered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody2D rb = other.gameObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 playerVelocity = rb.linearVelocity;  // this is a 2D vector
                float playerSpeed = playerVelocity.magnitude; // scalar speed (length of velocity)

                playerSpeed *= speedMultiplier;

                // Pass covered state to SpeedTextController
                speedTextController.SpeedChanged(playerSpeed, isCovered);
            }
        }
        if (other.CompareTag("Vehicle"))
        {
            // 車輛使用 Kinematic Rigidbody，所以要從 Vehicle 組件獲取速度
            Vehicle vehicle = other.gameObject.GetComponent<Vehicle>();
            if (vehicle != null)
            {
                float carSpeed = vehicle.speed; // 直接使用車輛設定的速度
                carSpeed *= speedMultiplier;

                // 使用專門針對車輛的方法（不影響心情值）
                speedTextController.VehicleSpeedChanged(carSpeed, isCovered);
            }
        }
    }
    
    // Called by DraggableTarget UnityEvent when spray painted
    public void SetCovered(bool covered)
    {
        isCovered = covered;
        Debug.Log($"SpeedDetector {gameObject.name}: Covered state set to {isCovered}");
    }
}
