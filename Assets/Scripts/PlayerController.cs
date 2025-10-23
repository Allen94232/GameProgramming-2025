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

    [Header("Env Effects")]
    public bool inWater = false;

    [Header("Bell Settings")]
    public AudioClip bellSound;
    private float bellTimer = 0f;
    private AudioSource audioSource;

    [Header("Renderer")]
    [SerializeField] private SpriteRenderer spriteRenderer; // assign in Inspector
    [SerializeField] private Color normalColor = Color.green;
    [SerializeField] private Color inWaterColor = new Color(0.5f, 0.8f, 1f); // light blue


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
    private float moodDrainInterval = 1f;  // drain every 1 second
    private float moodDrainTimer = 1f;     // internal timer

    // Collision timer variables
    private float collisionCooldown = 0.5f;
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
        stones = FindObjectsOfType<Stone>();
        ApplySettingsForStatus(PlayerStatus.Normal);
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

    void ApplySettingsForStatus(PlayerStatus newStatus)
    {
        switch (newStatus)
        {
            case PlayerStatus.Normal:
                currentMaxForwardSpeed = oriMaxForwardSpeed;
                currentMaxBackwardSpeed = oriMaxBackwardSpeed;
                currentAcceleration = oriAcceleration;
                currentDeceleration = oriDeceleration;
                currentBrakeDeceleration = oriBrakeDeceleration;
                currentBackwardAcceleration = oriBackwardAcceleration;
                currentTurnSpeed = oriTurnSpeed;

                // Change color when returning to normal
                if (spriteRenderer != null)
                    spriteRenderer.color = normalColor;

                break;

            case PlayerStatus.InWater:
                // You can tweak these multipliers as needed
                currentMaxForwardSpeed = oriMaxForwardSpeed * 0.6f;
                currentMaxBackwardSpeed = oriMaxBackwardSpeed * 0.6f;
                currentAcceleration = oriAcceleration * 0.7f;
                currentDeceleration = oriDeceleration * 0.7f;
                currentBrakeDeceleration = oriBrakeDeceleration * 0.7f;
                currentBackwardAcceleration = oriBackwardAcceleration * 0.7f;
                currentTurnSpeed = oriTurnSpeed * 0.8f;

                // Change color when in water
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
            // Reset timer when not in water
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

    void TryRingBell()
    {
        bellTimer = 0f; // reset timer

        // 🔔 Play sound
        if (audioSource != null && bellSound != null)
            audioSource.PlayOneShot(bellSound);

        Debug.Log("Bell rang!");
        ScareNearbyPigeons();
    }

    void ScareNearbyPigeons()
    {
        // Find all pigeons currently in the scene
        Pigeon[] pigeons = FindObjectsOfType<Pigeon>();

        foreach (var pigeon in pigeons)
        {
            pigeon.TryScare(transform.position);
        }
    }

}
