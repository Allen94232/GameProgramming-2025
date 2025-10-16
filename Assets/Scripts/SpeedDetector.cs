using UnityEngine;

public class SpeedDetector : MonoBehaviour
{
    public SpeedTextController speedTextController;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody2D rb = other.gameObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 playerVelocity = rb.linearVelocity;  // this is a 2D vector
                float playerSpeed = playerVelocity.magnitude; // scalar speed (length of velocity)

                speedTextController.SpeedChanged(playerSpeed);
            }
        }
    }
}
