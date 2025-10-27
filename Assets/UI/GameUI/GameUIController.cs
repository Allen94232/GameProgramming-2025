using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class GameUIController : MonoBehaviour
{
    public static GameUIController Instance { get; private set; }

    private UIDocument _uiDocument;
    private VisualElement _root;
    private Button _pauseButton;
    private Button _resumeButton; 
    private Button _homeButton;
    private Button _restartButton;
    private VisualElement _pauseMenuContainer; 

    private Label _moodLabel;
    private float moodValue;
    
    private Label _timerLabel;
    private float _timeRemaining;
    public float levelTimeInSeconds = 300f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _uiDocument = GetComponent<UIDocument>();
        _root = _uiDocument.rootVisualElement;

        // CRITICAL: Set root to ignore pointer events
        // This allows clicks to pass through to UGUI buttons and GameObjects
        _root.pickingMode = PickingMode.Ignore;

        // Query UI elements
        _pauseButton = _root.Q<Button>("pause-button");
        _resumeButton = _root.Q<Button>("resume-button");
        _homeButton = _root.Q<Button>("home-button");
        _restartButton = _root.Q<Button>("restart-button");
        _pauseMenuContainer = _root.Q<VisualElement>("pause-menu-container");
        _timerLabel = _root.Q<Label>("timer-label");
        _moodLabel = _root.Q<Label>("mood-display-label");

        // Configure picking mode for all elements
        ConfigurePickingModes();

        // Register button events
        if (_pauseButton != null) _pauseButton.clicked += PauseGame;
        if (_resumeButton != null) _resumeButton.clicked += ResumeGame;
        if (_homeButton != null) _homeButton.clicked += GoToMainMenu;
        if (_restartButton != null) _restartButton.clicked += RestartLevel;
    }

    private void ConfigurePickingModes()
    {
        // HUD container needs to block clicks
        var hudContainer = _root.Q<VisualElement>("hud-container");
        if (hudContainer != null)
        {
            hudContainer.pickingMode = PickingMode.Position;
        }

        // Pause button and its parents need to accept clicks
        if (_pauseButton != null)
        {
            _pauseButton.pickingMode = PickingMode.Position;
            EnablePickingForParents(_pauseButton);
        }

        // Pause menu container needs to block clicks when visible
        if (_pauseMenuContainer != null)
        {
            _pauseMenuContainer.pickingMode = PickingMode.Position;
        }

        // All buttons in pause menu need to accept clicks
        if (_resumeButton != null)
        {
            _resumeButton.pickingMode = PickingMode.Position;
            EnablePickingForParents(_resumeButton);
        }
        if (_homeButton != null)
        {
            _homeButton.pickingMode = PickingMode.Position;
            EnablePickingForParents(_homeButton);
        }
        if (_restartButton != null)
        {
            _restartButton.pickingMode = PickingMode.Position;
            EnablePickingForParents(_restartButton);
        }

        // Labels should ignore picking
        if (_timerLabel != null)
            _timerLabel.pickingMode = PickingMode.Ignore;
        if (_moodLabel != null)
            _moodLabel.pickingMode = PickingMode.Ignore;

        Debug.Log("GameUIController: Picking modes configured successfully");
    }

    // Recursively enable picking for parent elements (except root)
    private void EnablePickingForParents(VisualElement element)
    {
        if (element == null || element == _root || element.parent == null)
            return;

        element.parent.pickingMode = PickingMode.Position;
        EnablePickingForParents(element.parent);
    }

    void Start()
    {
        Init();
    }

    public void Init()
    {
        moodValue = GameManager.Instance.initialMood;
        _timeRemaining = GameManager.Instance.gameTime;
        
        // Update UI displays immediately
        UpdateMoodDisplay(moodValue);
        UpdateTimerDisplay();
    }

    private void RestartLevel()
    {
        Debug.Log("Restarting Level...");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGame();
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public float GetMoodValue()
    {
        return moodValue;
    }

    public void SetMoodValue(float newMoodValue)
    {
        Debug.Log("SetMoodValue: " + newMoodValue);
        moodValue = Mathf.Clamp(newMoodValue, 0, GameManager.Instance.maxMood);
        UpdateMoodDisplay(moodValue);
    }

    public void UpdateMoodDisplay(float newMoodValue)
    {
        if (_moodLabel != null)
        {
            _moodLabel.text = "Mood: " + Mathf.RoundToInt(newMoodValue).ToString();
        }
    }

    public void UpdateTimer()
    {
        if (_timeRemaining > 0)
        {
            _timeRemaining -= Time.deltaTime;
            UpdateTimerDisplay();
        }
        else
        {
            _timeRemaining = 0;
            UpdateTimerDisplay();
            GameManager.Instance.GameLose();
        }
    }

    private void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(_timeRemaining / 60);
        int seconds = Mathf.FloorToInt(_timeRemaining % 60);

        if (_timerLabel != null)
        {
            _timerLabel.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    private void GoToMainMenu()
    {
        Debug.Log("Returning to Main Menu Scene...");
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
    }
    
    private void PauseGame()
    {
        Debug.Log("Game Paused!");
        _pauseMenuContainer.style.display = DisplayStyle.Flex;
        Time.timeScale = 0f;
    }

    private void ResumeGame()
    {
        Debug.Log("Game Resumed!");
        _pauseMenuContainer.style.display = DisplayStyle.None;
        Time.timeScale = 1f;
    }
}

