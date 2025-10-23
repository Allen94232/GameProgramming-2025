using UnityEngine;
using UnityEngine.UIElements; // We need this for UI Toolkit
using UnityEngine.SceneManagement;

public class GameUIController : MonoBehaviour
{
    public static GameUIController Instance { get; private set; }

    private UIDocument _uiDocument;
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
        var root = _uiDocument.rootVisualElement;

        // UI elements
        _pauseButton = root.Q<Button>("pause-button");
        _resumeButton = root.Q<Button>("resume-button");
        _homeButton = root.Q<Button>("home-button");
        _restartButton = root.Q<Button>("restart-button");
        _pauseMenuContainer = root.Q<VisualElement>("pause-menu-container");
        _timerLabel = root.Q<Label>("timer-label");
        _moodLabel = root.Q<Label>("mood-display-label");

        if (_pauseButton != null) _pauseButton.clicked += PauseGame;
        if (_resumeButton != null) _resumeButton.clicked += ResumeGame;
        if (_homeButton != null) _homeButton.clicked += GoToMainMenu;
        if (_restartButton != null) _restartButton.clicked += RestartLevel;

    }

    void Start()
    {
        moodValue = 100;
        _timeRemaining = levelTimeInSeconds;
    }

    void Update()
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
        }

        //for mood points    
        UpdateMoodDisplay();

    }

    private void RestartLevel()
    {
        Debug.Log("Restarting Level...");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public float GetMoodValue()
    {
        return moodValue;
    }

    public void SetMoodValue(float newMoodValue)
    {
        Debug.Log("new value" + newMoodValue);
        moodValue = Mathf.Clamp(newMoodValue, 0, 100);
    }

    private void UpdateMoodDisplay()
    {
        if (_moodLabel != null)
        {
            _moodLabel.text = "Mood: " + moodValue.ToString();
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

