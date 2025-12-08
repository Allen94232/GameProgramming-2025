using UnityEngine;

/// <summary>
/// Vehicle controller that follows waypoint paths on roads
/// </summary>
public class Vehicle : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Vehicle movement speed")]
    public float speed = 10f;
    
    [Tooltip("How close to waypoint before moving to next (in units)")]
    public float waypointReachDistance = 0.5f;
    
    [Tooltip("Rotation smoothing (higher = smoother but slower turn)")]
    public float rotationSpeed = 5f;

    [Header("Path")]
    [Tooltip("Starting waypoint for this vehicle")]
    public RoadWaypoint startWaypoint;

    [Header("Sprite - 4 Directions Only")]
    [Tooltip("Vehicle sprite renderer")]
    public SpriteRenderer spriteRenderer;
    
    [Tooltip("Vehicle sprite facing up")]
    public Sprite spriteUp;
    
    [Tooltip("Vehicle sprite facing right")]
    public Sprite spriteRight;
    
    [Tooltip("Vehicle sprite facing down")]
    public Sprite spriteDown;
    
    [Tooltip("Vehicle sprite facing left")]
    public Sprite spriteLeft;
    
    [Header("Collider")]
    [Tooltip("Vehicle collider object (child object with collider component)")]
    public Transform vehicleColliderTransform;

    [Header("Obstacle Detection")]
    [Tooltip("Detection range in front of vehicle")]
    public float detectionDistance = 3f;
    
    [Tooltip("Width of detection area")]
    public float detectionWidth = 1.5f;
    
    [Tooltip("Layers to detect (Pedestrian, Player, etc.)")]
    public LayerMask obstacleLayer;
    
    [Tooltip("Stop duration when obstacle detected (seconds)")]
    public float stopDuration = 1f;
    
    [Tooltip("Detection interval when stopped (seconds)")]
    public float detectionInterval = 1f;

    [Header("Collision")]
    [Tooltip("Stop duration after being hit by player (seconds)")]
    public float hitStopDuration = 3f;
    
    [Tooltip("Stop duration when hitting pedestrian (seconds)")]
    public float pedestrianHitStopDuration = 1f;
    
    [Header("Fade Effect")]
    [Tooltip("Fade in duration when spawning (seconds)")]
    public float fadeInDuration = 1f;
    
    [Tooltip("Fade out duration when destroying (seconds)")]
    public float fadeOutDuration = 1f;

    // Current state
    private RoadWaypoint currentTargetWaypoint;
    private bool isMoving = false;
    private Vector3 moveDirection;
    
    // Obstacle detection state
    private bool isStopped = false;
    private float stopTimer = 0f;
    private float detectionTimer = 0f;
    private bool isHitByPlayer = false;
    private Collider2D vehicleCollider;
    
    // Fade state
    private bool isFadingOut = false;
    private float fadeTimer = 0f;

    void Start()
    {
        // Initialize sprite renderer if not assigned
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        // Start with transparent for fade in effect
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 0f;
            spriteRenderer.color = color;
        }
        
        // Get vehicle's own collider to exclude from detection
        // Check child collider first (if using vehicleColliderTransform)
        if (vehicleColliderTransform != null)
        {
            vehicleCollider = vehicleColliderTransform.GetComponent<Collider2D>();
        }
        else
        {
            vehicleCollider = GetComponent<Collider2D>();
        }

        // Set starting waypoint as first target
        if (startWaypoint != null)
        {
            currentTargetWaypoint = startWaypoint;
            isMoving = true;
            
            // Position vehicle at start waypoint
            transform.position = startWaypoint.GetPosition();
        }
        else
        {
            Debug.LogWarning($"Vehicle {gameObject.name}: No start waypoint assigned!");
            isMoving = false;
        }
    }

    void Update()
    {
        if (!isMoving || currentTargetWaypoint == null)
            return;
        
        // Handle fade in effect
        if (fadeTimer < fadeInDuration && !isFadingOut)
        {
            fadeTimer += Time.deltaTime;
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = Mathf.Clamp01(fadeTimer / fadeInDuration);
                spriteRenderer.color = color;
            }
        }
        
        // Handle fade out effect
        if (isFadingOut)
        {
            fadeTimer += Time.deltaTime;
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = Mathf.Clamp01(1f - (fadeTimer / fadeOutDuration));
                spriteRenderer.color = color;
            }
            
            // Destroy after fade out completes
            if (fadeTimer >= fadeOutDuration)
            {
                Destroy(gameObject);
            }
            return;
        }

        // Handle stopped state (obstacle or hit by player)
        if (isStopped)
        {
            stopTimer += Time.deltaTime;
            detectionTimer += Time.deltaTime;
            
            // Check if stop duration is over
            if (stopTimer >= (isHitByPlayer ? hitStopDuration : stopDuration))
            {
                // Time to check for obstacles again
                if (detectionTimer >= detectionInterval)
                {
                    detectionTimer = 0f;
                    
                    // Check if path is clear
                    if (!DetectObstacle())
                    {
                        // Path is clear, resume movement
                        isStopped = false;
                        isHitByPlayer = false;
                        stopTimer = 0f;
                    }
                    else
                    {
                        // Still blocked, reset stop timer to wait another cycle
                        stopTimer = 0f;
                    }
                }
            }
            
            return; // Don't move while stopped
        }

        // Normal movement - check for obstacles ahead
        if (DetectObstacle())
        {
            // Obstacle detected, stop vehicle
            isStopped = true;
            stopTimer = 0f;
            detectionTimer = 0f;
            return;
        }

        // Calculate move direction
        moveDirection = (currentTargetWaypoint.GetPosition() - transform.position).normalized;

        // Move towards target waypoint
        transform.position = Vector3.MoveTowards(
            transform.position,
            currentTargetWaypoint.GetPosition(),
            speed * Time.deltaTime
        );

        // Update sprite based on direction
        UpdateSprite();

        // Check if reached waypoint
        if (Vector3.Distance(transform.position, currentTargetWaypoint.GetPosition()) < waypointReachDistance)
        {
            ReachWaypoint();
        }
    }

    bool DetectObstacle()
    {
        if (moveDirection == Vector3.zero)
            return false;

        // Calculate detection box position (in front of vehicle)
        Vector3 detectionCenter = transform.position + moveDirection * (detectionDistance * 0.5f);
        
        // Detection box size
        Vector2 boxSize = new Vector2(detectionWidth, detectionDistance);
        
        // Calculate rotation angle
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        
        // Check for obstacles (get all colliders in the area)
        Collider2D[] hits = Physics2D.OverlapBoxAll(detectionCenter, boxSize, angle, obstacleLayer);
        
        // Get all colliders in this vehicle's hierarchy to exclude them
        Collider2D[] selfColliders = GetComponentsInChildren<Collider2D>();
        
        // Check if any hit is not part of this vehicle
        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;
            
            // Check if this collider belongs to this vehicle
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

    void ReachWaypoint()
    {
        // Check if this is the end of the path
        if (currentTargetWaypoint.IsEndPoint())
        {
            // Reached end - start fade out
            isFadingOut = true;
            fadeTimer = 0f;
            return;
        }

        // Move to next waypoint
        currentTargetWaypoint = currentTargetWaypoint.nextWaypoint;
    }

    void UpdateSprite()
    {
        if (spriteRenderer == null)
            return;

        // Calculate angle from movement direction
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        
        // Normalize angle to 0-360
        if (angle < 0) angle += 360f;

        Sprite newSprite = null;
        float spriteBaseAngle = 0f; // sprite 圖片本身朝向的角度

        // Determine sprite based on closest cardinal direction
        if (angle >= 315f || angle < 45f)
        {
            // Right (0度) - 使用朝右的圖片
            newSprite = spriteRight;
            spriteBaseAngle = 0f;
        }
        else if (angle >= 45f && angle < 135f)
        {
            // Up (90度) - 使用朝上的圖片
            newSprite = spriteUp;
            spriteBaseAngle = 90f;
        }
        else if (angle >= 135f && angle < 225f)
        {
            // Left (180度) - 使用朝左的圖片
            newSprite = spriteLeft;
            spriteBaseAngle = 180f;
        }
        else
        {
            // Down (270度) - 使用朝下的圖片
            newSprite = spriteDown;
            spriteBaseAngle = 270f;
        }

        // Update sprite if changed
        if (newSprite != null && spriteRenderer.sprite != newSprite)
        {
            spriteRenderer.sprite = newSprite;
        }
        
        // 計算需要額外旋轉的角度（實際方向 - sprite 基礎方向）
        float rotationOffset = angle - spriteBaseAngle;
        
        // 將旋轉角度標準化到 -180 到 180 之間
        if (rotationOffset > 180f) rotationOffset -= 360f;
        if (rotationOffset < -180f) rotationOffset += 360f;
        
        // 限制旋轉在 ±45 度之內
        rotationOffset = Mathf.Clamp(rotationOffset, -45f, 45f);
        
        // 只旋轉偏移量，不加上基礎角度（因為 sprite 圖片本身已經是朝向正確方向的）
        transform.rotation = Quaternion.Euler(0, 0, rotationOffset);
        
        // Rotate collider to match movement direction
        if (vehicleColliderTransform != null)
        {
            vehicleColliderTransform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }
    }
    // Collision with player and pedestrians
    void OnTriggerEnter2D(Collider2D collision)
    {
        // Stop immediately on any collision to prevent overlap
        if (!isStopped)
        {
            isStopped = true;
            stopTimer = 0f;
            detectionTimer = 0f;
        }
        
        if (collision.CompareTag("Player"))
        {
            HandlePlayerCollision();
        }
        else if (collision.CompareTag("Pedestrian"))
        {
            HandlePedestrianCollision(collision.gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Stop immediately on any collision to prevent overlap
        if (!isStopped)
        {
            isStopped = true;
            stopTimer = 0f;
            detectionTimer = 0f;
        }
        
        if (collision.gameObject.CompareTag("Player"))
        {
            HandlePlayerCollision();
        }
        else if (collision.gameObject.CompareTag("Pedestrian"))
        {
            HandlePedestrianCollision(collision.gameObject);
        }
    }

    void HandlePlayerCollision()
    {
        // Stop vehicle for 3 seconds after being hit
        isStopped = true;
        isHitByPlayer = true;
        stopTimer = 0f;
        detectionTimer = 0f;
        
        Debug.Log($"Vehicle hit by player! Stopping for {hitStopDuration} seconds");
        
        // Note: Player's mood reduction is handled in PlayerController.OnCollisionEnter2D
        // No need to reduce mood here to avoid double deduction
    }

    void HandlePedestrianCollision(GameObject pedestrian)
    {
        // Stop vehicle briefly when hitting pedestrian
        if (!isStopped)
        {
            isStopped = true;
            stopTimer = 0f;
            detectionTimer = 0f;
            
            Debug.Log($"Vehicle collided with pedestrian! Stopping for {stopDuration} seconds");
        }
        
        // Tell pedestrian to pick new target to avoid getting stuck
        Pedestrian pedestrianScript = pedestrian.GetComponent<Pedestrian>();
        if (pedestrianScript != null)
        {
            pedestrianScript.AvoidObstacle(pedestrianScript.vehicleStopDuration);
        }
    }

    // Visualize vehicle path and detection area in editor
    void OnDrawGizmosSelected()
    {
        if (startWaypoint == null)
            return;

        // Draw complete path from start waypoint
        Gizmos.color = Color.red;
        RoadWaypoint current = startWaypoint;
        Vector3 vehiclePos = Application.isPlaying ? transform.position : startWaypoint.GetPosition();
        
        Gizmos.DrawLine(vehiclePos, current.GetPosition());

        while (current != null && !current.IsEndPoint())
        {
            RoadWaypoint next = current.nextWaypoint;
            if (next != null)
            {
                Gizmos.DrawLine(current.GetPosition(), next.GetPosition());
            }
            current = next;
        }
        
        // Draw obstacle detection area (when playing)
        if (Application.isPlaying && moveDirection != Vector3.zero)
        {
            Gizmos.color = isStopped ? Color.red : Color.green;
            Vector3 detectionCenter = transform.position + moveDirection * (detectionDistance * 0.5f);
            
            // Draw detection box
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(detectionCenter, 
                Quaternion.Euler(0, 0, Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg), 
                Vector3.one);
            Gizmos.matrix = rotationMatrix;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(detectionWidth, detectionDistance, 0));
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}
