using UnityEngine;
using System.Collections;

public class WaterSprayTrigger : MonoBehaviour
{
    [Header("Water Spray Settings")]
    public bool isOpening = true;
    
    [Header("Auto Reopen Settings")]
    [Tooltip("Time in seconds before water spray automatically reopens")]
    public float autoReopenDelay = 10f;

    [Header("Visual Effects")]
    public ParticleSystem waterSprayEffect;

    private bool playerInside = false;
    private bool wasPlayerNotifiedEnter = false; // Track if we've notified PlayerController
    private Collider2D triggerCollider;
    private Coroutine autoReopenCoroutine;

    private void Awake()
    {
        Collider2D[] colliders = GetComponents<Collider2D>();
        
        foreach (Collider2D col in colliders)
        {
            if (col.isTrigger)
            {
                triggerCollider = col;
                break;
            }
        }
        
        if (triggerCollider == null && colliders.Length > 0)
        {
            triggerCollider = colliders[0];
            Debug.LogWarning($"WaterSprayTrigger on {gameObject.name}: No trigger collider found, using first collider");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            
            // Only notify PlayerController if spray is on
            if (isOpening)
            {
                NotifyPlayerEnter();
            }
            
            Debug.Log($"{gameObject.name}: Player entered water spray area!");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            
            // Always notify exit if we previously notified enter
            if (wasPlayerNotifiedEnter)
            {
                NotifyPlayerExit();
            }
            
            Debug.Log($"{gameObject.name}: Player exited water spray area!");
        }
    }

    private void Update()
    {
        // Handle spray turning off while player is inside
        if (!isOpening && playerInside && wasPlayerNotifiedEnter)
        {
            NotifyPlayerExit();
            Debug.Log($"{gameObject.name}: Water spray turned off, player no longer affected.");
        }

        // Handle spray turning on while player is already inside
        if (isOpening && playerInside && !wasPlayerNotifiedEnter)
        {
            // Double check if player is still touching the collider
            if (PlayerController.Instance != null)
            {
                Collider2D playerCollider = PlayerController.Instance.GetComponent<Collider2D>();
                if (playerCollider != null && triggerCollider != null && triggerCollider.IsTouching(playerCollider))
                {
                    NotifyPlayerEnter();
                    Debug.Log($"{gameObject.name}: Water spray turned on, player already inside!");
                }
            }
        }
    }

    // Notify PlayerController that player entered water
    private void NotifyPlayerEnter()
    {
        if (!wasPlayerNotifiedEnter && PlayerController.Instance != null)
        {
            PlayerController.Instance.EnterWaterArea();
            wasPlayerNotifiedEnter = true;
        }
    }

    // Notify PlayerController that player exited water
    private void NotifyPlayerExit()
    {
        if (wasPlayerNotifiedEnter && PlayerController.Instance != null)
        {
            PlayerController.Instance.ExitWaterArea();
            wasPlayerNotifiedEnter = false;
        }
    }

    public void ToggleWaterSpray()
    {
        isOpening = !isOpening;
        
        if (!isOpening)
        {
            StartAutoReopenTimer();
            Debug.Log($"{gameObject.name}: Water spray turned OFF. Will automatically reopen in {autoReopenDelay} seconds.");

            if (waterSprayEffect != null && waterSprayEffect.isPlaying)
            {
                waterSprayEffect.Stop();
            }
        }
        else
        {
            CancelAutoReopenTimer();
            Debug.Log($"{gameObject.name}: Water spray turned ON manually.");

            if (waterSprayEffect != null && !waterSprayEffect.isPlaying)
            {
                waterSprayEffect.Play();
            }
        }
    }

    private void StartAutoReopenTimer()
    {
        CancelAutoReopenTimer();
        autoReopenCoroutine = StartCoroutine(AutoReopenCoroutine());
    }

    private void CancelAutoReopenTimer()
    {
        if (autoReopenCoroutine != null)
        {
            StopCoroutine(autoReopenCoroutine);
            autoReopenCoroutine = null;
        }
    }

    private IEnumerator AutoReopenCoroutine()
    {
        yield return new WaitForSeconds(autoReopenDelay);
        
        if (!isOpening)
        {
            isOpening = true;
            Debug.Log($"{gameObject.name}: Water spray automatically reopened after timer!");

            if (waterSprayEffect != null && !waterSprayEffect.isPlaying)
            {
                waterSprayEffect.Play();
            }

            BroadcastMessage("RefreshVisualState", SendMessageOptions.DontRequireReceiver);
        }
        
        autoReopenCoroutine = null;
    }

    public void SetAutoReopenDelay(float delayInSeconds)
    {
        autoReopenDelay = delayInSeconds;
    }

    public bool IsAutoReopenActive()
    {
        return autoReopenCoroutine != null;
    }

    public float GetRemainingReopenTime()
    {
        return autoReopenCoroutine != null ? autoReopenDelay : 0f;
    }

    public bool IsPlayerInside()
    {
        return playerInside;
    }

    private void OnDestroy()
    {
        // Clean up: notify player exit if this sprinkler is destroyed while player is inside
        if (wasPlayerNotifiedEnter)
        {
            NotifyPlayerExit();
        }
        
        CancelAutoReopenTimer();
    }

    private void OnDisable()
    {
        // Clean up: notify player exit if this sprinkler is disabled while player is inside
        if (wasPlayerNotifiedEnter)
        {
            NotifyPlayerExit();
        }
        
        CancelAutoReopenTimer();
    }
}
