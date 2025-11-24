using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using LootLocker.Requests;
using System.Collections;

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
    private VisualElement _winScreenContainer;
    private VisualElement _loseScreenContainer;
    private Button _restartButtonWin;
    private Button _restartButtonLose;
    private Button _homeButtonWin;
    private Button _homeButtonLose;
    
    private ScrollView _winLeaderboardScroll;
    private Label _currentTimeLabel;
    private Label _bestTimeLabel;

    private Label _moodLabel;
    private float moodValue;
    private VisualElement _moodBarFill; 
    private VisualElement _moodIcon;
    [Header("Mood Icons")]
    [SerializeField] private Sprite happyFace;
    [SerializeField] private Sprite neutralFace;
    [SerializeField] private Sprite sadFace;

    [Header("Mood Bar Colors")]
    [SerializeField] private Color happyColor = new Color(0.89f, 0.33f, 0.09f); 
    [SerializeField] private Color neutralColor = new Color(1f, 0.8f, 0.2f);   
    [SerializeField] private Color sadColor = new Color(0.8f, 0.2f, 0.2f);   
    
    private Label _timerLabel;
    private float _timeRemaining;
    private bool _hasTriggeredGameOver = false; // Prevent repeated GameLose calls
    
    private float _currentRunTime; // Store current run's remaining time
    private bool _isWaitingForSession = false;
    private Coroutine _sessionCheckCoroutine = null;

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

        // clicks to pass through
        _root.pickingMode = PickingMode.Ignore;

        // Query UI elements
        _pauseButton = _root.Q<Button>("pause-button");
        _resumeButton = _root.Q<Button>("resume-button");
        _homeButton = _root.Q<Button>("home-button");
        _restartButton = _root.Q<Button>("restart-button");
        _pauseMenuContainer = _root.Q<VisualElement>("pause-menu-container");

        _timerLabel = _root.Q<Label>("timer-label");
        _moodLabel = _root.Q<Label>("mood-display-label");
        _moodBarFill = _root.Q<VisualElement>("mood-bar-fill");
        _moodIcon = _root.Q<VisualElement>("mood-icon"); 

        _winScreenContainer = _root.Q<VisualElement>("win-screen-container");
        _loseScreenContainer = _root.Q<VisualElement>("lose-screen-container");

        // Win screen elements
        if (_winScreenContainer != null)
        {
            _restartButtonWin = _winScreenContainer.Q<Button>("restart-button-win");
            _homeButtonWin = _winScreenContainer.Q<Button>("home-button-win");
            _winLeaderboardScroll = _winScreenContainer.Q<ScrollView>("win-leaderboard-scroll");
            _currentTimeLabel = _winScreenContainer.Q<Label>("current-time-label");
            _bestTimeLabel = _winScreenContainer.Q<Label>("best-time-label");
        }
        
        // Lose screen elements
        if (_loseScreenContainer != null)
        {
            _restartButtonLose = _loseScreenContainer.Q<Button>("restart-button-lose");
            _homeButtonLose = _loseScreenContainer.Q<Button>("home-button-lose");
        }

        // Configure picking mode 
        ConfigurePickingModes();

        // button events (with sound effects)
        if (_pauseButton != null)
        {
            _pauseButton.clicked += () => { PlayButtonSound(); PauseGame(); };
            _pauseButton.RegisterCallback<MouseEnterEvent>(evt => PlayButtonHoverSound());
        }
        if (_resumeButton != null)
        {
            _resumeButton.clicked += () => { PlayButtonSound(); ResumeGame(); };
            _resumeButton.RegisterCallback<MouseEnterEvent>(evt => PlayButtonHoverSound());
        }
        if (_homeButton != null)
        {
            _homeButton.clicked += () => { PlayButtonSound(); GoToMainMenu(); };
            _homeButton.RegisterCallback<MouseEnterEvent>(evt => PlayButtonHoverSound());
        }
        if (_restartButton != null)
        {
            _restartButton.clicked += () => { PlayButtonSound(); RestartLevel(); };
            _restartButton.RegisterCallback<MouseEnterEvent>(evt => PlayButtonHoverSound());
        }
        if (_restartButtonWin != null)
        {
            _restartButtonWin.clicked += () => { PlayButtonSound(); RestartLevel(); };
            _restartButtonWin.RegisterCallback<MouseEnterEvent>(evt => PlayButtonHoverSound());
        }
        if (_restartButtonLose != null)
        {
            _restartButtonLose.clicked += () => { PlayButtonSound(); RestartLevel(); };
            _restartButtonLose.RegisterCallback<MouseEnterEvent>(evt => PlayButtonHoverSound());
        }
        if (_homeButtonWin != null)
        {
            _homeButtonWin.clicked += () => { PlayButtonSound(); GoToMainMenu(); };
            _homeButtonWin.RegisterCallback<MouseEnterEvent>(evt => PlayButtonHoverSound());
        }
        if (_homeButtonLose != null)
        {
            _homeButtonLose.clicked += () => { PlayButtonSound(); GoToMainMenu(); };
            _homeButtonLose.RegisterCallback<MouseEnterEvent>(evt => PlayButtonHoverSound());
        }
        
        // Subscribe to LootLocker session ready event
        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.OnSessionReady += OnLootLockerSessionReady;
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from event
        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.OnSessionReady -= OnLootLockerSessionReady;
        }
    }
    
    private void OnLootLockerSessionReady()
    {
        Debug.Log("LootLocker session ready, refreshing win screen leaderboard if needed");
        
        // If win screen is showing and waiting for session, reload leaderboard data
        if (_isWaitingForSession && _winScreenContainer != null && _winScreenContainer.style.display == DisplayStyle.Flex)
        {
            _isWaitingForSession = false;
            LoadWinScreenData();
        }
    }

    private void ConfigurePickingModes()
    {
        // block clicks
        var hudContainer = _root.Q<VisualElement>("hud-container");
        if (hudContainer != null)
        {
            hudContainer.pickingMode = PickingMode.Position;
        }

        // accept clicks
        if (_pauseButton != null)
        {
            _pauseButton.pickingMode = PickingMode.Position;
            EnablePickingForParents(_pauseButton);
        }

        if (_pauseMenuContainer != null)
        {
            _pauseMenuContainer.pickingMode = PickingMode.Position;
        }

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


        // ignore picking
        if (_timerLabel != null)
            _timerLabel.pickingMode = PickingMode.Ignore;
        if (_moodLabel != null)
            _moodLabel.pickingMode = PickingMode.Ignore;

        Debug.Log("GameUIController: Picking modes configured successfully");
    }

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
        _hasTriggeredGameOver = false; // Reset game over flag
        
        // Update UI display
        UpdateMoodDisplay(moodValue);
        UpdateTimerDisplay();
    }

    private void RestartLevel()
    {
        Debug.Log("Restarting Level...");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartCurrentLevel();
        }
        else
        {
            // Fallback if GameManager doesn't exist
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
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
            _moodLabel.text = Mathf.RoundToInt(newMoodValue).ToString();
        }

        // percentage calculation
        float maxMood = (GameManager.Instance != null) ? GameManager.Instance.maxMood : 100f;
        if (maxMood <= 0) maxMood = 100f;
        float percentage = Mathf.Clamp01(newMoodValue / maxMood);

        // Bar Width
        if (_moodBarFill != null)
        {
            _moodBarFill.style.width = Length.Percent(percentage * 100f);
        }

        // face and color
        // happy > 60% | neutral 30%-60% | sad < 30%
        
        if (percentage > 0.6f)
        {

            if (_moodIcon != null && happyFace != null) 
                _moodIcon.style.backgroundImage = new StyleBackground(happyFace);
            
            if (_moodBarFill != null)
                _moodBarFill.style.backgroundColor = happyColor;
        }
        else if (percentage > 0.3f)
        {

            if (_moodIcon != null && neutralFace != null) 
                _moodIcon.style.backgroundImage = new StyleBackground(neutralFace);

            if (_moodBarFill != null)
                _moodBarFill.style.backgroundColor = neutralColor;
        }
        else
        {

            if (_moodIcon != null && sadFace != null) 
                _moodIcon.style.backgroundImage = new StyleBackground(sadFace);

            if (_moodBarFill != null)
                _moodBarFill.style.backgroundColor = sadColor;
        }
    }

    public void UpdateTimer()
    {
        if (_timeRemaining > 0)
        {
            _timeRemaining -= Time.deltaTime;
            UpdateTimerDisplay();
        }
        else if (!_hasTriggeredGameOver)
        {
            _timeRemaining = 0;
            UpdateTimerDisplay();
            _hasTriggeredGameOver = true; // Set flag before calling GameLose
            GameManager.Instance.GameLose();
        }
    }

    private void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(_timeRemaining / 60);
        int seconds = Mathf.FloorToInt(_timeRemaining % 60);
        //float fraction = _timeRemaining % 1;
        int milliseconds = Mathf.FloorToInt((_timeRemaining * 1000f) % 1000f);

        if (_timerLabel != null)
        {
            _timerLabel.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
        }
    }

    private void GoToMainMenu()
    {
        Debug.Log("Returning to Main Menu Scene...");
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMainMenu();
        }
        else
        {
            // Fallback if GameManager doesn't exist
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenuScene");
        }
    }
    
    // Play button click sound effect
    private void PlayButtonSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSFX();
        }
    }
    
    // Play button hover sound effect
    private void PlayButtonHoverSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonHoverSFX();
        }
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

    public void ShowWinScreen()
    {
        if (_winScreenContainer != null)
        {
            _winScreenContainer.style.display = DisplayStyle.Flex;
            
            // Store current run time
            _currentRunTime = _timeRemaining;
            
            // Load and display leaderboard data
            LoadWinScreenData();
        }
    }
    
    private void LoadWinScreenData()
    {
        string currentLevelName = SceneManager.GetActiveScene().name;
        
        // Display current time
        if (_currentTimeLabel != null)
        {
            _currentTimeLabel.text = $"Time Remaining: {FormatTime(_currentRunTime)}";
        }
        
        // Check if LootLocker session is ready
        if (LeaderboardManager.Instance == null || !LeaderboardManager.Instance.IsSessionReady())
        {
            Debug.Log("LootLocker session not ready yet, showing loading message for win screen");
            _isWaitingForSession = true;
            
            // Display loading message in best time label
            if (_bestTimeLabel != null)
            {
                _bestTimeLabel.text = "Connecting to server...";
                _bestTimeLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
            }
            
            // Display loading in leaderboard
            if (_winLeaderboardScroll != null)
            {
                _winLeaderboardScroll.Clear();
                var loadingLabel = new Label("Connecting to Leaderboard...\nPlease wait...");
                loadingLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
                loadingLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                loadingLabel.style.marginTop = 40;
                loadingLabel.style.fontSize = 18;
                _winLeaderboardScroll.Add(loadingLabel);
            }
            
            // Start checking for session readiness
            if (_sessionCheckCoroutine != null)
            {
                StopCoroutine(_sessionCheckCoroutine);
            }
            _sessionCheckCoroutine = StartCoroutine(CheckSessionAndRefreshWinScreen());
            
            return;
        }
        
        // Session is ready, clear waiting flag
        _isWaitingForSession = false;
        
        // Get player's best time from leaderboard
        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.GetPlayerRank((rank, score) =>
            {
                if (_bestTimeLabel != null)
                {
                    if (rank > 0)
                    {
                        float bestTime = score / 1000f;
                        _bestTimeLabel.text = $"Your Best: {FormatTime(bestTime)}";
                        
                        // new record style
                        _bestTimeLabel.RemoveFromClassList("new-record-text");
                        if (_currentRunTime > bestTime)
                        {
                            _bestTimeLabel.text += " (NEW RECORD!)";
                            _bestTimeLabel.AddToClassList("new-record-text"); 
                        }
                    }
                    else
                    {
                        _bestTimeLabel.text = "Your Best: First Clear!";
                        _bestTimeLabel.style.color = new Color(1f, 0.84f, 0f);
                    }
                }
            }, currentLevelName);
            
            // Load top 5 players
            LeaderboardManager.Instance.GetTopPlayers(5, OnWinLeaderboardLoaded, currentLevelName);
        }
    }
    
    private void OnWinLeaderboardLoaded(LootLockerLeaderboardMember[] members)
    {
        if (_winLeaderboardScroll == null) return;
        
        _winLeaderboardScroll.Clear();
        
        if (members == null || members.Length == 0)
        {
            var emptyLabel = new Label("No leaderboard data yet");
            emptyLabel.style.color = Color.white;
            emptyLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            emptyLabel.style.marginTop = 20;
            _winLeaderboardScroll.Add(emptyLabel);
            return;
        }
        
        // Display each leaderboard entry
        for (int i = 0; i < members.Length; i++)
        {
            var member = members[i];
            var entry = CreateWinLeaderboardEntry(member);
            _winLeaderboardScroll.Add(entry);
        }
    }
    
    private VisualElement CreateWinLeaderboardEntry(LootLockerLeaderboardMember member)
    {
        var container = new VisualElement();
        container.AddToClassList("leaderboard-row");
        
        string playerIdentifier = LeaderboardManager.Instance?.GetPlayerIdentifier();
        bool isCurrentPlayer = (member.member_id == playerIdentifier);
        
        if (isCurrentPlayer)
        {
            container.AddToClassList("row-player");
        }
        else if (member.rank == 1) container.AddToClassList("row-gold");
        else if (member.rank == 2) container.AddToClassList("row-silver");
        else if (member.rank == 3) container.AddToClassList("row-bronze");
        else
        {
            container.style.backgroundColor = new Color(0, 0, 0, 0.3f);
        }
        
        // rank label
        string rankText = (member.rank <= 3) ? $"#{member.rank}" : member.rank.ToString();
        var rankLabel = new Label(rankText);
        rankLabel.AddToClassList("leaderboard-text"); 
        rankLabel.style.width = 80; 
        rankLabel.style.flexGrow = 0;
        container.Add(rankLabel);

        // name label
        string displayName = ExtractPlayerNameFromMetadata(member.metadata);
        if (string.IsNullOrEmpty(displayName))
        {
            displayName = member.member_id;
            if (displayName.Length > 20) displayName = "Player " + displayName.Substring(0, 4);
        }
    
        var nameLabel = new Label(displayName);
        nameLabel.AddToClassList("leaderboard-text");
        nameLabel.style.width = StyleKeyword.Auto;
        nameLabel.style.flexGrow = 1;
        nameLabel.style.marginLeft = 12;

    
        container.Add(nameLabel);
        
        // time label
        float timeInSeconds = member.score / 1000f;
        var timeLabel = new Label(FormatTime(timeInSeconds));
        timeLabel.AddToClassList("leaderboard-text");
        timeLabel.style.width = 160;
        timeLabel.style.marginRight =30;
        timeLabel.style.unityTextAlign = TextAnchor.MiddleRight;
        container.Add(timeLabel);
            
        return container;
    }
    
    private string ExtractPlayerNameFromMetadata(string metadata)
    {
        if (string.IsNullOrEmpty(metadata)) return null;
        
        try
        {
            int nameStart = metadata.IndexOf("\"playerName\":\"");
            if (nameStart >= 0)
            {
                nameStart += "\"playerName\":\"".Length;
                int nameEnd = metadata.IndexOf("\"", nameStart);
                if (nameEnd > nameStart)
                {
                    return metadata.Substring(nameStart, nameEnd - nameStart);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to parse metadata: {e.Message}");
        }
        
        return null;
    }

    public void ShowLoseScreen()
    {
        if (_loseScreenContainer != null)
        {
        _loseScreenContainer.style.display = DisplayStyle.Flex; 
        }
    }
    
    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        int milliseconds = Mathf.FloorToInt((timeInSeconds * 1000f) % 1000f);
        return $"{minutes:00}:{seconds:00}.{milliseconds:000}";
    }

    public float GetRemainingTime()
    {
        return _timeRemaining;
    }
    
    private IEnumerator CheckSessionAndRefreshWinScreen()
    {
        // Check every 0.25 seconds
        while (_isWaitingForSession)
        {
            yield return new WaitForSeconds(0.25f);
            
            if (LeaderboardManager.Instance != null && LeaderboardManager.Instance.IsSessionReady())
            {
                Debug.Log("Session connected! Refreshing win screen leaderboard...");
                _isWaitingForSession = false;
                LoadWinScreenData();
                yield break;
            }
        }
    }
}


