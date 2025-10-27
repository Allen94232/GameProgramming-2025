using UnityEngine;

public class Pedestrian : MonoBehaviour
{
    [Header("Walking Setting")]
    public Animator animator;
    public float walkingSpeed = 2f;
    public float areaSize = 10f;         // Random walking area radius
    public float stopDuration = 2f;      // Stop duration
    
    [Header("Collision Detection")]
    public LayerMask obstacleLayer;      // Layer for walls and obstacles
    public float checkRadius = 0.5f;     // Radius to check for collisions at target position
    public int maxAttempts = 10;         // Maximum attempts to find valid position
    
    private bool isWalking = false;
    private Vector3 targetPosition;
    private float stopTimer = 0f;
    private Vector3 startPosition;       // Store starting position for area calculation

    void Start()
    {
        // Store the starting position as the center of walking area
        startPosition = transform.position;
        
        // animator = GetComponent<Animator>();
        // animator.SetBool("isWalking", false);
        PickNewTarget();
    }

    void Update()
    {
        if (!isWalking)
        {
            stopTimer += Time.deltaTime;
            if (stopTimer >= stopDuration)
            {
                PickNewTarget();
            }
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, walkingSpeed * Time.deltaTime);

        // Face the walking direction
        //Vector3 dir = targetPosition - transform.position;
        //if (dir != Vector3.zero)
        //{
        //    transform.forward = Vector3.Lerp(transform.forward, dir.normalized, Time.deltaTime * 5f);
        //}

        // Check if reached target
        if (Vector3.Distance(transform.position, targetPosition) < 0.3f)
        {
            isWalking = false;
            // animator.SetBool("isWalking", false);
            stopTimer = 0f;
        }
    }

    void OnBecameVisible()
    {
        // Start moving when visible
        isWalking = true;
        // animator.SetBool("isWalking", true);
    }

    void OnBecameInvisible()
    {
        // Optional: stop when out of view
        isWalking = false;
        // animator.SetBool("isWalking", false);
    }

    void PickNewTarget()
    {
        Vector3 newTarget = Vector3.zero;
        bool foundValidPosition = false;
        
        // Try multiple times to find a valid position
        for (int i = 0; i < maxAttempts; i++)
        {
            // Pick random position around starting point
            Vector2 randomPos = Random.insideUnitCircle * areaSize;
            newTarget = new Vector3(
                startPosition.x + randomPos.x, 
                startPosition.y + randomPos.y, 
                transform.position.z
            );
            
            // Check if position is valid (not inside collider)
            if (IsPositionValid(newTarget))
            {
                foundValidPosition = true;
                break;
            }
        }
        
        // If no valid position found, stay at current position
        if (foundValidPosition)
        {
            targetPosition = newTarget;
            isWalking = true;
            // animator.SetBool("isWalking", true);
        }
        else
        {
            Debug.LogWarning($"Pedestrian {gameObject.name}: Could not find valid target position after {maxAttempts} attempts");
            // Stay still and try again later
            isWalking = false;
            stopTimer = 0f;
        }
    }
    
    // Check if a position is valid (not inside obstacle)
    private bool IsPositionValid(Vector3 position)
    {
        // Use OverlapCircle to check for colliders at the target position
        Collider2D hitCollider = Physics2D.OverlapCircle(position, checkRadius, obstacleLayer);
        
        // Position is valid if no collider was hit
        return hitCollider == null;
    }
    
    // Visualize the walking area and check radius in editor
    void OnDrawGizmosSelected()
    {
        // Draw walking area
        Gizmos.color = Color.yellow;
        Vector3 center = Application.isPlaying ? startPosition : transform.position;
        Gizmos.DrawWireSphere(center, areaSize);
        
        // Draw current target and check radius
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetPosition, checkRadius);
            Gizmos.DrawLine(transform.position, targetPosition);
        }
    }
}
