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
    public float checkRadius = 0.5f;     // Radius to check for collisions at target position (fallback if no collider)
    public int maxAttempts = 10;         // Maximum attempts to find valid position
    
    [Header("Forward Detection")]
    [Tooltip("Enable forward obstacle detection while walking")]
    public bool enableForwardDetection = true;
    [Tooltip("Distance to check ahead for obstacles")]
    public float forwardDetectionDistance = 1.5f;
    [Tooltip("Width of detection area")]
    public float forwardDetectionWidth = 1f;
    [Tooltip("Stop duration when obstacle detected (seconds)")]
    public float obstacleStopDuration = 1f;
    [Tooltip("Detection interval when stopped (seconds)")]
    public float stoppedDetectionInterval = 1f;
    
    private float detectionTimer = 0f;
    private bool isStopped = false;
    private float stoppedTimer = 0f;
    
    [Header("Angry State")]
    [Tooltip("Angry image to show when pedestrian is angry")]
    public GameObject angryImage;
    [Tooltip("Duration in seconds that pedestrian stays angry after collision")]
    public float angryDuration = 2f;
    
    private bool isWalking = false;
    private Vector3 targetPosition;
    private float stopTimer = 0f;
    private Vector3 startPosition;       // Store starting position for area calculation
    private Vector3 walkingDirection;
    private Collider2D pedestrianCollider;  // Reference to pedestrian's own collider
    
    // Angry state variables
    private bool isAngry = false;
    private float angryTimer = 0f;

    void Start()
    {
        // Store the starting position as the center of walking area
        startPosition = transform.position;
        
        // Get pedestrian's collider for overlap detection
        pedestrianCollider = GetComponent<Collider2D>();
        
        // Initialize angry state
        if (angryImage != null)
        {
            angryImage.SetActive(false);
        }
        
        // animator = GetComponent<Animator>();
        // animator.SetBool("isWalking", false);
        PickNewTarget();
    }

    void Update()
    {
        // Handle angry state timer
        if (isAngry)
        {
            angryTimer -= Time.deltaTime;
            if (angryTimer <= 0f)
            {
                SetAngry(false);
            }
        }
        
        if (!isWalking)
        {
            stopTimer += Time.deltaTime;
            if (stopTimer >= stopDuration)
            {
                PickNewTarget();
            }
            return;
        }

        // Handle stopped state (similar to vehicle logic)
        if (isStopped)
        {
            stoppedTimer += Time.deltaTime;
            detectionTimer += Time.deltaTime;
            
            // Check if stop duration is over
            if (stoppedTimer >= obstacleStopDuration)
            {
                // Time to check for obstacles again
                if (detectionTimer >= stoppedDetectionInterval)
                {
                    detectionTimer = 0f;
                    
                    // Check if path is clear
                    if (!DetectObstacleAhead())
                    {
                        // Path is clear, resume walking
                        isStopped = false;
                        stoppedTimer = 0f;
                    }
                    else
                    {
                        // Still blocked, pick new target
                        AvoidObstacle();
                    }
                }
            }
            
            return; // Don't move while stopped
        }

        // Forward detection to avoid obstacles while walking
        if (enableForwardDetection && DetectObstacleAhead())
        {
            // Obstacle detected ahead, stop
            isStopped = true;
            stoppedTimer = 0f;
            detectionTimer = 0f;
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

        animator.SetBool("isWalking", isWalking);
        animator.SetFloat("walkingX", walkingDirection.x);
        animator.SetFloat("walkingY", walkingDirection.y);
    }
    
    // Detect obstacles ahead while walking (using box detection like vehicle)
    bool DetectObstacleAhead()
    {
        if (walkingDirection == Vector3.zero)
            return false;
        
        // Calculate detection box position (in front of pedestrian)
        Vector3 detectionCenter = transform.position + walkingDirection * (forwardDetectionDistance * 0.5f);
        
        // Detection box size
        Vector2 boxSize = new Vector2(forwardDetectionWidth, forwardDetectionDistance);
        
        // Calculate rotation angle
        float angle = Mathf.Atan2(walkingDirection.y, walkingDirection.x) * Mathf.Rad2Deg;
        
        // Check for obstacles (get all colliders in the area)
        Collider2D[] hits = Physics2D.OverlapBoxAll(detectionCenter, boxSize, angle, obstacleLayer);
        
        // Get all colliders in this pedestrian's hierarchy to exclude them
        Collider2D[] selfColliders = GetComponentsInChildren<Collider2D>();
        
        // Check if any hit is not part of this pedestrian
        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;
            
            // Check if this collider belongs to this pedestrian
            bool isSelf = false;
            foreach (Collider2D selfCollider in selfColliders)
            {
                if (hit == selfCollider)
                {
                    isSelf = true;
                    break;
                }
            }
            
            // If not self, it's an obstacle
            if (!isSelf)
            {
                return true;
            }
        }
        
        return false;
    }

    void OnBecameVisible()
    {
        // Start moving when visible
        isWalking = true;
        animator.SetBool("isWalking", isWalking);
        // animator.SetBool("isWalking", true);
    }

    void OnBecameInvisible()
    {
        // Optional: stop when out of view
        isWalking = false;
        animator.SetBool("isWalking", isWalking);
        // animator.SetBool("isWalking", false);
    }
    
    // Collision with player or vehicle (using Collision2D)
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            SetAngry(true);
            Debug.Log($"Pedestrian {gameObject.name}: Hit by player, becoming angry!");
        }
        else if (collision.gameObject.CompareTag("Vehicle"))
        {
            // Vehicle hit pedestrian - pick new target to avoid getting stuck
            AvoidObstacle();
            Debug.Log($"Pedestrian {gameObject.name}: Hit by vehicle, picking new target!");
        }
    }
    
    // Trigger collision with player or vehicle (using Trigger)
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SetAngry(true);
            Debug.Log($"Pedestrian {gameObject.name}: Hit by player (trigger), becoming angry!");
        }
        else if (collision.CompareTag("Vehicle"))
        {
            // Vehicle hit pedestrian - pick new target to avoid getting stuck
            AvoidObstacle();
            Debug.Log($"Pedestrian {gameObject.name}: Hit by vehicle (trigger), picking new target!");
        }
    }
    
    // Called when pedestrian collides with obstacle (vehicle)
    public void AvoidObstacle()
    {
        // Stop current movement and show idle animation
        isWalking = false;
        isStopped = true;
        stoppedTimer = 0f;
        detectionTimer = 0f;
        stopTimer = 0f;
        
        // Update animator to idle state
        if (animator != null)
        {
            animator.SetBool("isWalking", false);
        }
        
        // Will pick new target after obstacleStopDuration
    }
    
    // Set angry state
    void SetAngry(bool angry)
    {
        isAngry = angry;
        
        if (isAngry)
        {
            angryTimer = angryDuration;
        }
        
        // Toggle angry image
        if (angryImage != null)
        {
            angryImage.SetActive(isAngry);
        }
    }
    
    // Public method to check if pedestrian is angry
    public bool IsAngry()
    {
        return isAngry;
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

            walkingDirection = (newTarget - transform.position).normalized;
            
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
        animator.SetBool("isWalking", isWalking);
    }
    
    // Check if a position is valid (not inside obstacle)
    bool IsPositionValid(Vector3 position)
    {
        // If pedestrian has a collider, check if the collider would overlap with obstacles at target position
        if (pedestrianCollider != null)
        {
            // Calculate offset from current position to target position
            Vector2 offset = (Vector2)(position - transform.position);
            
            // Check collision based on collider type
            if (pedestrianCollider is BoxCollider2D boxCollider)
            {
                // Get box collider properties
                Vector2 boxSize = boxCollider.size * transform.localScale;
                Vector2 boxCenter = (Vector2)position + boxCollider.offset;
                float angle = transform.eulerAngles.z;
                
                // Check if box would overlap with any obstacle at target position
                Collider2D hitCollider = Physics2D.OverlapBox(boxCenter, boxSize, angle, obstacleLayer);
                return hitCollider == null;
            }
            else if (pedestrianCollider is CircleCollider2D circleCollider)
            {
                // Get circle collider properties
                float radius = circleCollider.radius * Mathf.Max(transform.localScale.x, transform.localScale.y);
                Vector2 circleCenter = (Vector2)position + circleCollider.offset;
                
                // Check if circle would overlap with any obstacle at target position
                Collider2D hitCollider = Physics2D.OverlapCircle(circleCenter, radius, obstacleLayer);
                return hitCollider == null;
            }
            else if (pedestrianCollider is CapsuleCollider2D capsuleCollider)
            {
                // Get capsule collider properties
                Vector2 capsuleSize = capsuleCollider.size * transform.localScale;
                Vector2 capsuleCenter = (Vector2)position + capsuleCollider.offset;
                float angle = transform.eulerAngles.z;
                
                // Check if capsule would overlap with any obstacle at target position
                Collider2D hitCollider = Physics2D.OverlapCapsule(capsuleCenter, capsuleSize, capsuleCollider.direction, angle, obstacleLayer);
                return hitCollider == null;
            }
        }
        
        // Fallback: Use simple circle check if no collider or unsupported type
        Collider2D fallbackHit = Physics2D.OverlapCircle(position, checkRadius, obstacleLayer);
        return fallbackHit == null;
    }
    
    // Visualize the walking area and check radius in editor
    void OnDrawGizmosSelected()
    {
        // Draw walking area
        Gizmos.color = Color.yellow;
        Vector3 center = Application.isPlaying ? startPosition : transform.position;
        Gizmos.DrawWireSphere(center, areaSize);
        
        // Draw current target and collision detection area
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            
            // Visualize based on collider type
            if (pedestrianCollider is BoxCollider2D boxCollider)
            {
                Vector2 boxSize = boxCollider.size * transform.localScale;
                Gizmos.DrawWireCube(targetPosition + (Vector3)boxCollider.offset, boxSize);
            }
            else if (pedestrianCollider is CircleCollider2D circleCollider)
            {
                float radius = circleCollider.radius * Mathf.Max(transform.localScale.x, transform.localScale.y);
                Gizmos.DrawWireSphere(targetPosition + (Vector3)circleCollider.offset, radius);
            }
            else if (pedestrianCollider is CapsuleCollider2D capsuleCollider)
            {
                // Approximate capsule visualization with sphere
                Vector2 capsuleSize = capsuleCollider.size * transform.localScale;
                float radius = Mathf.Max(capsuleSize.x, capsuleSize.y) * 0.5f;
                Gizmos.DrawWireSphere(targetPosition + (Vector3)capsuleCollider.offset, radius);
            }
            else
            {
                // Fallback visualization
                Gizmos.DrawWireSphere(targetPosition, checkRadius);
            }
            
            Gizmos.DrawLine(transform.position, targetPosition);
            
            // Draw forward detection area (like vehicle)
            if (enableForwardDetection && walkingDirection != Vector3.zero)
            {
                Gizmos.color = isStopped ? Color.red : Color.cyan;
                Vector3 detectionCenter = transform.position + walkingDirection * (forwardDetectionDistance * 0.5f);
                
                // Draw detection box
                float angle = Mathf.Atan2(walkingDirection.y, walkingDirection.x) * Mathf.Rad2Deg;
                Matrix4x4 rotationMatrix = Matrix4x4.TRS(detectionCenter, 
                    Quaternion.Euler(0, 0, angle), 
                    Vector3.one);
                Gizmos.matrix = rotationMatrix;
                Gizmos.DrawWireCube(Vector3.zero, new Vector3(forwardDetectionWidth, forwardDetectionDistance, 0));
                Gizmos.matrix = Matrix4x4.identity;
            }
        }
    }
}
