using UnityEngine;

public class WaterSprayTrigger : MonoBehaviour
{
    public bool isOpening = true; // Controlled by external button to turn water spray on/off
    private bool playerInside = false;

    private Collider2D triggerCollider;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isOpening)
        {
            playerInside = true;
            Debug.Log("Player entered water spray!");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            Debug.Log("Player exited water spray!");
        }
    }

    private void Update()
    {
        // If water spray is turned off, treat player as not inside
        if (!isOpening && playerInside)
        {
            playerInside = false;
            Debug.Log("Water spray turned off, player no longer considered inside.");
        }

        // If spray is on but playerInside is false, check if player is already inside the collider
        if (isOpening && !playerInside)
        {
            Collider2D playerCollider = PlayerController.Instance.GetComponent<Collider2D>();
            if (triggerCollider.IsTouching(playerCollider))
            {
                playerInside = true;
                Debug.Log("Player was already inside when water spray turned on!");
            }
        }

        // Update player water status
        PlayerController.Instance.inWater = playerInside && isOpening;
    }

    public void ToggleWaterSpray()
    {
        isOpening = !isOpening;
    }

    public bool IsPlayerInside()
    {
        return playerInside;
    }
}
