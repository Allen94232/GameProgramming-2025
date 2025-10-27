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
    public float oriMaxForwardSpeed = 8f;
    public float oriMaxBackwardSpeed = 3f;

    [Header("Original Acceleration Settings")]
    public float oriAcceleration = 10f;
    public float oriDeceleration = 8f;
    public float oriBrakeDeceleration = 20f;
    public float oriBackwardAcceleration = 6f;

    [Header("Original Rotation Settings")]
    public float oriTurnSpeed = 200f;

    [Header("Cooldown Settings")]
    public float collisionCooldown = 0.5f;

    [Header("Mood Speed Settings")]
    [Tooltip("Base mood value that corresponds to original speed (default 100)")]
    public float baseMoodValue = 100f;
    [Tooltip("Minimum speed multiplier when mood is 0 (e.g., 0.5 = 50% speed)")]
    public float minSpeedMultiplier = 0.3f;
    [Tooltip("Maximum speed multiplier when mood is 200 (e.g., 1.5 = 150% speed)")]
    public float maxSpeedMultiplier = 2.0f;
    
    [Header("Env Effects")]
    public bool inWater = false;
    
    // Counter to track how many water spray areas the player is in
    private int waterAreaCount = 0;

    [Header("Bell Settings")]
    public AudioClip bellSound;
    private float bellTimer = 0f;
    private AudioSource audioSource;

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
    private Stone[] stones;

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
        stones = FindObjectsByType<Stone>(FindObjectsSortMode.None);
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

        collisionTimer += Time.deltaTime;

        bellTimer += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryRingBell();
        }

        foreach (var stone in stones)
        {
            stone.TryFall(transform.position);
        }
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
        ApplySettingsForStatus(Playerstatus);
        waterAreaCount = 0;
        inWater = false;
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
        if (moveInput > 0)
        {
            currentSpeed += currentAcceleration * Time.fixedDeltaTime;
            currentSpeed = Mathf.Min(currentSpeed, currentMaxForwardSpeed);
        }
        else if (moveInput < 0)
        {
            if (currentSpeed > 0)
            {
                currentSpeed -= currentBrakeDeceleration * Time.fixedDeltaTime;
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
        if (Mathf.Abs(currentSpeed) > 0.1f)
        {
            float turnInput = -Input.GetAxis("Horizontal");
            rb.MoveRotation(rb.rotation + turnInput * currentTurnSpeed * Time.fixedDeltaTime);
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

        switch (newStatus)
        {
            case PlayerStatus.Normal:
                currentMaxForwardSpeed = oriMaxForwardSpeed * moodMultiplier;
                currentMaxBackwardSpeed = oriMaxBackwardSpeed * moodMultiplier;
                currentAcceleration = oriAcceleration * moodMultiplier;
                currentDeceleration = oriDeceleration * moodMultiplier;
                currentBrakeDeceleration = oriBrakeDeceleration * moodMultiplier;
                currentBackwardAcceleration = oriBackwardAcceleration * moodMultiplier;
                currentTurnSpeed = oriTurnSpeed * moodMultiplier;

                if (spriteRenderer != null)
                    spriteRenderer.color = normalColor;

                break;

            case PlayerStatus.InWater:
                currentMaxForwardSpeed = oriMaxForwardSpeed * 0.6f * moodMultiplier;
                currentMaxBackwardSpeed = oriMaxBackwardSpeed * 0.6f * moodMultiplier;
                currentAcceleration = oriAcceleration * 0.7f * moodMultiplier;
                currentDeceleration = oriDeceleration * 0.7f * moodMultiplier;
                currentBrakeDeceleration = oriBrakeDeceleration * 0.7f * moodMultiplier;
                currentBackwardAcceleration = oriBackwardAcceleration * 0.7f * moodMultiplier;
                currentTurnSpeed = oriTurnSpeed * 0.8f * moodMultiplier;

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

        Debug.Log("Bell rang!");
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
}
