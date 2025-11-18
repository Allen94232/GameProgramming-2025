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
    
    private Label _timerLabel;
    private float _timeRemaining;
    
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

        // button events
        if (_pauseButton != null) _pauseButton.clicked += PauseGame;
        if (_resumeButton != null) _resumeButton.clicked += ResumeGame;
        if (_homeButton != null) _homeButton.clicked += GoToMainMenu;
        if (_restartButton != null) _restartButton.clicked += RestartLevel;
        if (_restartButtonWin != null) _restartButtonWin.clicked += RestartLevel;
        if (_restartButtonLose != null) _restartButtonLose.clicked += RestartLevel;
        if (_homeButtonWin != null) _homeButtonWin.clicked += GoToMainMenu;
        if (_homeButtonLose != null) _homeButtonLose.clicked += GoToMainMenu;
        
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
        
        // Update UI display
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
                var loadingLabel = new Label("Connecting to LootLocker...\nPlease wait...");
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
                        
                        // Check if this is a new record
                        if (_currentRunTime > bestTime)
                        {
                            _bestTimeLabel.text += " (NEW RECORD!)";
                            _bestTimeLabel.style.color = new Color(1f, 0.84f, 0f); // Gold color
                        }
                        else
                        {
                            _bestTimeLabel.style.color = new Color(180f/255f, 180f/255f, 180f/255f);
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
        container.style.flexDirection = FlexDirection.Row;
        container.style.paddingTop = 6;
        container.style.paddingBottom = 6;
        container.style.paddingLeft = 10;
        container.style.paddingRight = 10;
        container.style.marginBottom = 1;
        container.style.alignItems = Align.Center;
        
        // Highlight player's own entry
        string playerIdentifier = LeaderboardManager.Instance?.GetPlayerIdentifier();
        bool isCurrentPlayer = (member.member_id == playerIdentifier);
        
        if (isCurrentPlayer)
        {
            container.style.backgroundColor = new Color(11f/255f, 255f/255f, 11f/255f, 0.25f);
            container.style.borderLeftWidth = 3;
            container.style.borderLeftColor = new Color(11f/255f, 255f/255f, 11f/255f);
        }
        else if (member.rank == 1)
        {
            container.style.backgroundColor = new Color(1f, 0.84f, 0f, 0.12f);
        }
        else if (member.rank == 2)
        {
            container.style.backgroundColor = new Color(0.75f, 0.75f, 0.75f, 0.12f);
        }
        else if (member.rank == 3)
        {
            container.style.backgroundColor = new Color(0.8f, 0.5f, 0.2f, 0.12f);
        }
        else
        {
            container.style.backgroundColor = new Color(0, 0, 0, 0.3f);
        }
        
        // Rank with medal for top 3
        string rankText = "";
        Color rankColor = Color.white;
        
        if (member.rank == 1) 
        {
            rankText = "#1";
            rankColor = new Color(1f, 0.84f, 0f); // Gold
        }
        else if (member.rank == 2) 
        {
            rankText = "#2";
            rankColor = new Color(0.75f, 0.75f, 0.75f); // Silver
        }
        else if (member.rank == 3) 
        {
            rankText = "#3";
            rankColor = new Color(0.8f, 0.5f, 0.2f); // Bronze
        }
        else 
        {
            rankText = $"#{member.rank}";
        }

        var rankLabel = new Label(rankText);
        rankLabel.style.width = 70;
        rankLabel.style.color = rankColor;
        rankLabel.style.fontSize = 16;
        rankLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        container.Add(rankLabel);
        
        // Player name
        string displayName = ExtractPlayerNameFromMetadata(member.metadata);
        if (string.IsNullOrEmpty(displayName))
        {
            displayName = member.member_id;
            if (displayName.Length > 20 && displayName.Contains("-"))
            {
                string shortId = displayName.Substring(0, 8);
                displayName = $"Player{shortId}";
            }
        }
        
        var nameLabel = new Label(displayName);
        nameLabel.style.flexGrow = 1;
        nameLabel.style.color = isCurrentPlayer ? new Color(11f/255f, 255f/255f, 11f/255f) : Color.white;
        nameLabel.style.fontSize = 15;
        if (isCurrentPlayer)
        {
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        }
        container.Add(nameLabel);
        
        // Time
        float timeInSeconds = member.score / 1000f;
        var timeLabel = new Label(FormatTime(timeInSeconds));
        timeLabel.style.width = 120;
        timeLabel.style.color = member.rank <= 3 ? rankColor : Color.white;
        timeLabel.style.fontSize = 15;
        timeLabel.style.unityTextAlign = TextAnchor.MiddleRight;
        if (member.rank <= 3)
        {
            timeLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        }
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


