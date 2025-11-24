using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    public enum PlayerStatus
    {
        Normal,
        InWater
    }

    [Header("Player Status")]
    public PlayerStatus Playerstatus;

    [Header("Original Speed Settings")]
    public float oriMaxForwardSpeed = 30f;
    public float oriMaxBackwardSpeed = 10f;

    [Header("Original Acceleration Settings")]
    public float oriAcceleration = 10f;
    public float oriDeceleration = 8f;
    public float oriBrakeDeceleration = 20f;
    public float oriBackwardAcceleration = 6f;

    [Header("Original Rotation Settings")]
    public float oriTurnSpeed = 240f;
    [Tooltip("Maximum handlebar angle when stationary (in degrees)")]
    public float maxStationaryHandlebarAngle = 45f;
    [Tooltip("Speed of handlebar rotation when stationary")]
    public float stationaryHandlebarTurnSpeed = 72f;
    [Tooltip("Minimum turn speed factor at low speeds (e.g., 0.7 = 70% turn speed)")]
    public float minTurnSpeedFactor = 0.7f;

    [Header("Cooldown Settings")]
    public float collisionCooldown = 1.5f;

    [Header("Mood Speed Settings")]
    [Tooltip("Base mood value that corresponds to original speed (default 100)")]
    public float baseMoodValue = 100f;
    [Tooltip("Minimum speed multiplier when mood is 0 (e.g., 0.5 = 50% speed)")]
    public float minSpeedMultiplier = 0.5f;
    [Tooltip("Maximum speed multiplier when mood is 200 (e.g., 1.5 = 150% speed)")]
    public float maxSpeedMultiplier = 1.7f;
    
    [Header("Env Effects")]
    public bool inWater = false;

    [Header("Sprite")]
    public Sprite[] sprites;
    public Animator animator;
    
    // Counter to track how many water spray areas the player is in
    private int waterAreaCount = 0;

    [Header("Bell Settings")]
    public AudioClip bellSound;
    private float bellTimer = 0f;
    private AudioSource audioSource;
    [Tooltip("Duration of control reduction after ringing bell (seconds)")]
    public float bellControlReductionDuration = 0.5f;
    [Tooltip("Turn speed multiplier during bell control reduction (0.75 = 25% reduction)")]
    public float bellTurnSpeedMultiplier = 0.5f;
    [Tooltip("Brake effectiveness multiplier during bell control reduction (0.5 = 50% reduction)")]
    public float bellBrakeMultiplier = 0.5f;
    private float bellControlReductionTimer = 0f;

    [Header("Renderer")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color normalColor = Color.green;
    [SerializeField] private Color inWaterColor = new Color(0.5f, 0.8f, 1f);

    // Current runtime settings (used for movement logic)
    private float currentMaxForwardSpeed;
    private float currentMaxBackwardSpeed;
    private float currentAcceleration;
    private float currentDeceleration;
    private float currentBrakeDeceleration;
    private float currentBackwardAcceleration;
    private float currentTurnSpeed;
    private BirdPoop[] birdPoops;
    
    // External speed multiplier (e.g., from bird poop)
    private float externalSpeedMultiplier = 1f;
    
    // Handlebar angle when stationary (relative to body)
    private float currentHandlebarAngle = 0f;
    private float stationaryRotation = 0f;

    private Rigidbody2D rb;
    private float currentSpeed = 0f;
    private float moveInput;

    // Mood drain timer variables
    private float moodDrainInterval = 1f;
    private float moodDrainTimer = 1f;

    // Collision timer variables
    private float collisionTimer = 0.5f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        birdPoops = FindObjectsByType<BirdPoop>(FindObjectsSortMode.None);
        ApplySettingsForStatus(PlayerStatus.Normal);

        Init();
    }

    void Update()
    {
        // Update player status
        UpdatePlayerStatus();

        // Update settings before processing movement
        ApplySettingsForStatus(Playerstatus);

        // Handle mood draining while in water
        HandleMoodDrain();

        moveInput = Input.GetAxisRaw("Vertical");
        
        // Support Shift key for braking/backward movement
        if (Input.GetKey(KeyCode.LeftShift))
        {
            moveInput = -1f;
        }

        collisionTimer += Time.deltaTime;

        bellTimer += Time.deltaTime;
        
        // Update bell control reduction timer
        if (bellControlReductionTimer > 0f)
        {
            bellControlReductionTimer -= Time.deltaTime;
        }
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryRingBell();
        }
        
        foreach (var poop in birdPoops)
        {
            poop.TryFall(transform.position);
        }

        // Choose animation method (comment/uncomment based on preference)
        //UpdateSpriteByRotation();  // Direct sprite switching (instant)
        //UpdateSpriteWithAnimation();  // Animator-based (smooth transitions)
    }

    void FixedUpdate()
    {
        SyncSpeedWithPhysics();
        HandleMovement();
        HandleRotation();
    }

    public void Init()
    {
        Playerstatus = PlayerStatus.Normal;
        externalSpeedMultiplier = 1f;
        ApplySettingsForStatus(Playerstatus);
        waterAreaCount = 0;
        inWater = false;
        currentHandlebarAngle = 0f;
        stationaryRotation = rb.rotation;
    }

    // Called by WaterSprayTrigger when player enters water area
    public void EnterWaterArea()
    {
        waterAreaCount++;
        inWater = waterAreaCount > 0;
        
        Debug.Log($"Player entered water area. Total active water areas: {waterAreaCount}");
    }

    // Called by WaterSprayTrigger when player exits water area
    public void ExitWaterArea()
    {
        waterAreaCount--;
        // Ensure count doesn't go negative
        waterAreaCount = Mathf.Max(0, waterAreaCount);
        inWater = waterAreaCount > 0;
        
        Debug.Log($"Player exited water area. Total active water areas: {waterAreaCount}");
    }

    // Get current water area count (useful for debugging)
    public int GetWaterAreaCount()
    {
        return waterAreaCount;
    }

    void SyncSpeedWithPhysics()
    {
        // Project current velocity onto the forward direction to find actual speed
        float actualSpeed = Vector2.Dot(rb.linearVelocity, transform.up);
        currentSpeed = actualSpeed;
    }

    void HandleMovement()
    {
        // Apply bell control reduction to brake effectiveness
        float brakeMultiplier = IsUnderBellControlReduction() ? bellBrakeMultiplier : 1f;
        
        if (moveInput > 0)
        {
            currentSpeed += currentAcceleration * Time.fixedDeltaTime;
            currentSpeed = Mathf.Min(currentSpeed, currentMaxForwardSpeed);
        }
        else if (moveInput < 0)
        {
            if (currentSpeed > 0)
            {
                currentSpeed -= currentBrakeDeceleration * brakeMultiplier * Time.fixedDeltaTime;
            }
            else
            {
                currentSpeed -= currentBackwardAcceleration * Time.fixedDeltaTime;
                currentSpeed = Mathf.Max(currentSpeed, -currentMaxBackwardSpeed);
            }
        }
        else
        {
            if (currentSpeed > 0)
            {
                currentSpeed -= currentDeceleration * Time.fixedDeltaTime;
                currentSpeed = Mathf.Max(currentSpeed, 0);
            }
            else if (currentSpeed < 0)
            {
                currentSpeed += currentDeceleration * Time.fixedDeltaTime;
                currentSpeed = Mathf.Min(currentSpeed, 0);
            }
        }

        rb.linearVelocity = transform.up * currentSpeed;
    }

void HandleRotation()
{
    float turnInput = -Input.GetAxis("Horizontal");
    
    // Apply bell control reduction to handlebar rotation
    float turnMultiplier = IsUnderBellControlReduction() ? bellTurnSpeedMultiplier : 1f;
    
    if (Mathf.Abs(currentSpeed) > 0.1f)
    {
        // Moving: realistic bicycle physics
        // - Handlebar rotation speed: constant (controlled by player's hand strength)
        // - Turning effectiveness: increases with speed (physics of angular momentum)
        
        // Reverse steering direction when moving backward (like a car)
        float turnDirection = currentSpeed > 0 ? 1f : -1f;
        
        // Speed affects turning effectiveness, not handlebar rotation speed
        // At low speed: same handlebar angle produces less turning
        // At high speed: same handlebar angle produces more turning
        float turnEffectiveness = Mathf.Abs(currentSpeed) / currentMaxForwardSpeed;
        turnEffectiveness = Mathf.Clamp(turnEffectiveness, minTurnSpeedFactor, 1f);
        
        // Final rotation = handlebar input × effectiveness × control penalties
        float rotationSpeed = currentTurnSpeed * turnEffectiveness * turnMultiplier;
        
        rb.MoveRotation(rb.rotation + turnInput * turnDirection * rotationSpeed * Time.fixedDeltaTime);
        
        // Reset handlebar angle when moving
        currentHandlebarAngle = 0f;
        stationaryRotation = rb.rotation;
    }
    else
    {
        // Stationary: simulate handlebar rotation (limited angle)
        if (Mathf.Abs(turnInput) > 0.01f)
        {
            // Adjust handlebar angle
            currentHandlebarAngle += turnInput * stationaryHandlebarTurnSpeed * turnMultiplier * Time.fixedDeltaTime;
            currentHandlebarAngle = Mathf.Clamp(currentHandlebarAngle, -maxStationaryHandlebarAngle, maxStationaryHandlebarAngle);
            
            // Apply visual rotation based on handlebar angle
            rb.MoveRotation(stationaryRotation + currentHandlebarAngle);
        }
    }
}
    void UpdatePlayerStatus()
    {
        if (inWater)
        {
            Playerstatus = PlayerStatus.InWater;
        }
        else
        {
            Playerstatus = PlayerStatus.Normal;
        }    
    }

    float GetMoodSpeedMultiplier()
    {
        if (MoodController.Instance == null)
            return 1f;

        float currentMood = MoodController.Instance.GetMoodValue();
        
        float normalizedMood = currentMood / baseMoodValue;
        
        if (normalizedMood <= 1f)
        {
            return Mathf.Lerp(minSpeedMultiplier, 1f, normalizedMood);
        }
        else
        {
            return Mathf.Lerp(1f, maxSpeedMultiplier, normalizedMood - 1f);
        }
    }

    void ApplySettingsForStatus(PlayerStatus newStatus)
    {
        float moodMultiplier = GetMoodSpeedMultiplier();
        float finalMultiplier = moodMultiplier * externalSpeedMultiplier;

        switch (newStatus)
        {
            case PlayerStatus.Normal:
                currentMaxForwardSpeed = oriMaxForwardSpeed * finalMultiplier;
                currentMaxBackwardSpeed = oriMaxBackwardSpeed * finalMultiplier;
                currentAcceleration = oriAcceleration * finalMultiplier;
                currentDeceleration = oriDeceleration * finalMultiplier;
                currentBrakeDeceleration = oriBrakeDeceleration * finalMultiplier;
                currentBackwardAcceleration = oriBackwardAcceleration * finalMultiplier;
                currentTurnSpeed = oriTurnSpeed * finalMultiplier;

                if (spriteRenderer != null)
                    spriteRenderer.color = normalColor;

                break;

            case PlayerStatus.InWater:
                currentMaxForwardSpeed = oriMaxForwardSpeed * 0.6f * finalMultiplier;
                currentMaxBackwardSpeed = oriMaxBackwardSpeed * 0.6f * finalMultiplier;
                currentAcceleration = oriAcceleration * 0.7f * finalMultiplier;
                currentDeceleration = oriDeceleration * 0.7f * finalMultiplier;
                currentBrakeDeceleration = oriBrakeDeceleration * 0.7f * finalMultiplier;
                currentBackwardAcceleration = oriBackwardAcceleration * 0.7f * finalMultiplier;
                currentTurnSpeed = oriTurnSpeed * 0.8f * finalMultiplier;

                if (spriteRenderer != null)
                    spriteRenderer.color = inWaterColor;

                break;
        }
    }

    void HandleMoodDrain()
    {
        if (Playerstatus == PlayerStatus.InWater)
        {
            moodDrainTimer += Time.deltaTime;
            if (moodDrainTimer >= moodDrainInterval)
            {
                moodDrainTimer = 0f;
                float newMood = MoodController.Instance.GetMoodValue() - 5f;
                newMood = Mathf.Max(newMood, 0);
                MoodController.Instance.SetMoodValue(newMood);
            }
        }
        else
        {
            moodDrainTimer = 1f;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collisionTimer < collisionCooldown)
        {
            return;
        }

        float newMood = MoodController.Instance.GetMoodValue() - 10f;
        newMood = Mathf.Max(newMood, 0);
        MoodController.Instance.SetMoodValue(newMood);

        collisionTimer = 0f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Destination"))
        {
            GameManager.Instance.GameWin();
        }
    }

    void TryRingBell()
    {
        bellTimer = 0f;

        if (audioSource != null && bellSound != null)
            audioSource.PlayOneShot(bellSound);

        // Apply control reduction penalty
        bellControlReductionTimer = bellControlReductionDuration;

        Debug.Log("Bell rang! Control reduced for " + bellControlReductionDuration + " seconds");
        ScareNearbyPigeons();
    }

    void ScareNearbyPigeons()
    {
        Pigeon[] pigeons = FindObjectsByType<Pigeon>(FindObjectsSortMode.None);

        foreach (var pigeon in pigeons)
        {
            pigeon.TryScare(transform.position);
        }
    }
    
    // Check if player is currently under bell control reduction effect
    bool IsUnderBellControlReduction()
    {
        return bellControlReductionTimer > 0f;
    }

    // Apply external speed multiplier (e.g., from bird poop)
    public void ApplySpeedMultiplier(float multiplier)
    {
        externalSpeedMultiplier = multiplier;
        ApplySettingsForStatus(Playerstatus);
    }

    // Remove external speed multiplier
    public void RemoveSpeedMultiplier()
    {
        externalSpeedMultiplier = 1f;
        ApplySettingsForStatus(Playerstatus);
    }

    private int currentSpriteIndex = -1; // Track current sprite to avoid unnecessary updates
    
    void UpdateSpriteByRotation()
    {
        float angle = rb.rotation % 360f;

        if (angle < 0)
            angle += 360f; // 確保角度為 0~360

        // Determine sprite index based on 8-directional angle ranges
        int newSpriteIndex = -1;
        
        if (angle >= 337.5f || angle < 22.5f)
        {
            newSpriteIndex = 0; // 上 (0°)
        }
        else if (angle >= 22.5f && angle < 67.5f)
        {
            newSpriteIndex = 1; // 右上 (45°)
        }
        else if (angle >= 67.5f && angle < 112.5f)
        {
            newSpriteIndex = 2; // 右 (90°)
        }
        else if (angle >= 112.5f && angle < 157.5f)
        {
            newSpriteIndex = 3; // 右下 (135°)
        }
        else if (angle >= 157.5f && angle < 202.5f)
        {
            newSpriteIndex = 4; // 下 (180°)
        }
        else if (angle >= 202.5f && angle < 247.5f)
        {
            newSpriteIndex = 5; // 左下 (225°)
        }
        else if (angle >= 247.5f && angle < 292.5f)
        {
            newSpriteIndex = 6; // 左 (270°)
        }
        else // 292.5f ~ 337.5f
        {
            newSpriteIndex = 7; // 左上 (315°)
        }
        
        // Only update sprite if direction changed (reduces unnecessary sprite assignments)
        if (newSpriteIndex != currentSpriteIndex && newSpriteIndex >= 0 && newSpriteIndex < sprites.Length)
        {
            currentSpriteIndex = newSpriteIndex;
            spriteRenderer.sprite = sprites[currentSpriteIndex];
        }
    }

    // Animation-based sprite updating (uses Blend Tree for direction control)
    void UpdateSpriteWithAnimation()
    {
        if (animator == null) return;
        
        float angle = rb.rotation % 360f;

        if (angle < 0)
            angle += 360f; // 確保角度為 0~360

        // Convert angle to radians for trigonometry
        float angleRad = angle * Mathf.Deg2Rad;
        
        // Calculate target X and Y components
        float targetX = Mathf.Sin(angleRad);
        float targetY = Mathf.Cos(angleRad);
        
        // Direct parameter update (no smoothing) for instant sprite switching
        // For 8 static sprites, smoothing creates ghosting effect instead of rotation
        animator.SetFloat("MoveX", targetX);
        animator.SetFloat("MoveY", targetY);
        
        // Debug (optional - uncomment to test)
        // Debug.Log($"Angle: {angle:F1}° → X: {targetX:F2}, Y: {targetY:F2}");
    }
    
    // Helper method to get direction index based on angle
    private int GetDirectionIndex(float angle)
    {
        if (angle >= 337.5f || angle < 22.5f)
            return 0; // 上 (0°)
        else if (angle >= 22.5f && angle < 67.5f)
            return 1; // 右上 (45°)
        else if (angle >= 67.5f && angle < 112.5f)
            return 2; // 右 (90°)
        else if (angle >= 112.5f && angle < 157.5f)
            return 3; // 右下 (135°)
        else if (angle >= 157.5f && angle < 202.5f)
            return 4; // 下 (180°)
        else if (angle >= 202.5f && angle < 247.5f)
            return 5; // 左下 (225°)
        else if (angle >= 247.5f && angle < 292.5f)
            return 6; // 左 (270°)
        else
            return 7; // 左上 (315°)
    }
}
