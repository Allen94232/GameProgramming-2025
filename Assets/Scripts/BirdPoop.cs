using UnityEngine;

public class BirdPoop : MonoBehaviour
{
    [Header("Fall Setting")]
    [Tooltip("Detection radius for triggering poop fall")]
    public float fallRadius = 6f;
    [Tooltip("Fall speed of poop (should be fast to surprise player)")]
    public float fallSpeed = 12f;
    [Tooltip("Starting height above ground")]
    public float startHeight = 10f;
    public Sprite[] poopSprite;

    [Header("Shadow Setting")]
    public GameObject shadowPrefab;
    public float minShadowScale = 0.5f;
    public float maxShadowScale = 2.5f;

    [Header("Player Effect Setting")]
    [Tooltip("Speed multiplier when player is in poop (0.4 = 40% speed, player moves at ~3.2 units/s instead of 8)")]
    public float speedMultiplier = 0.4f;
    
    [Tooltip("Mood damage per second when in poop (player needs ~2-3 seconds to pass through)")]
    public float moodDamagePerSecond = 8f;

    private GameObject shadowInstance;
    private bool isFall = false;
    private bool fallen = false;
    private Vector3 fallDirection = Vector3.down;
    private Vector3 originalPosition;
    private Vector3 startPosition;
    private bool playerInPoop = false;

    void Start()
    {
        originalPosition = transform.position;
        startPosition = originalPosition + Vector3.up * startHeight;

        transform.position = startPosition;

        // Disable collider while in air, enable after landing
        Collider2D col = GetComponent<Collider2D>();
        col.enabled = false;
        col.isTrigger = true; // Ensure it's a trigger
        
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<SpriteRenderer>().sprite = poopSprite[0];

        if (shadowPrefab != null)
        {
            shadowInstance = Instantiate(shadowPrefab, originalPosition, Quaternion.identity);
            shadowInstance.transform.localScale = Vector3.one * minShadowScale;
        }
    }

    void Update()
    {
        if (isFall && !fallen)
        {
            transform.position += (Vector3)(fallDirection * fallSpeed * Time.deltaTime);

            // Shadow grows as poop falls
            if (shadowInstance != null)
            {
                float t = Mathf.InverseLerp(0f, startHeight, startPosition.y - transform.position.y);
                float scale = Mathf.Lerp(minShadowScale, maxShadowScale, t);
                shadowInstance.transform.localScale = Vector3.one * scale;
            }

            if (transform.position.y <= originalPosition.y)
            {
                transform.position = originalPosition; // Align to ground
                isFall = false;
                fallen = true;

                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                sr.sprite = poopSprite[1];

                // Enable collider only after landing
                GetComponent<Collider2D>().enabled = true;

                // Destroy shadow
                Destroy(shadowInstance);
            }
        }

        // Apply continuous damage while player is in poop
        if (playerInPoop && MoodController.Instance != null)
        {
            float currentMood = MoodController.Instance.GetMoodValue();
            float newMood = currentMood - (moodDamagePerSecond * Time.deltaTime);
            MoodController.Instance.SetMoodValue(newMood);
        }
    }

    public void TryFall(Vector3 playerPosition)
    {
        if (fallen || isFall) return;

        float distance = Vector3.Distance(playerPosition, originalPosition);
        if (distance <= fallRadius)
        {
            isFall = true;
            GetComponent<SpriteRenderer>().enabled = true;
            // Keep collider disabled while falling
            GetComponent<Collider2D>().enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!fallen) return;

        if (other.CompareTag("Player"))
        {
            playerInPoop = true;
            PlayerController player = other.GetComponent<PlayerController>();
            
            if (player != null)
            {
                // Apply speed reduction
                player.ApplySpeedMultiplier(speedMultiplier);
                Debug.Log($"BirdPoop: Player entered poop, speed reduced to {speedMultiplier * 100}%");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInPoop = false;
            PlayerController player = other.GetComponent<PlayerController>();
            
            if (player != null)
            {
                // Remove speed reduction
                player.RemoveSpeedMultiplier();
                Debug.Log("BirdPoop: Player exited poop, speed restored");
            }
        }
    }

    public void ResetPoop()
    {
        isFall = false;
        fallen = false;
        playerInPoop = false;
        transform.position = startPosition;
        GetComponent<Collider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<SpriteRenderer>().sprite = poopSprite[0];
        
        if (shadowInstance != null)
        {
            Destroy(shadowInstance);
            shadowInstance = Instantiate(shadowPrefab, originalPosition, Quaternion.identity);
            shadowInstance.transform.localScale = Vector3.one * minShadowScale;
        }
    }
}
