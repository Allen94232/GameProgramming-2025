using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using LootLocker.Requests;
using System.Collections.Generic;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    private UIDocument _uiDocument;

    // Main menu components
    private Button _startButton;
    private Button _leaderboardButton;
    private TextField _playerNameInput;
    private Button _confirmNameButton;
    private Label _nameWarningLabel;

    // Leaderboard panel components
    private VisualElement _leaderboardPanel;
    private Button _closeLeaderboardButton;
    private Button _refreshLeaderboardButton;
    private DropdownField _levelDropdown;
    private ScrollView _leaderboardScrollView;
    private Label _playerRankLabel;

    private List<string> _availableLevels = new List<string>();
    private string _currentSelectedLevel = "";
    
    private bool _isWaitingForSession = false;
    private Coroutine _sessionCheckCoroutine = null;

    private void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
        var root = _uiDocument.rootVisualElement;

        // Get main menu components
        _startButton = root.Q<Button>("start-game-button");
        _leaderboardButton = root.Q<Button>("leaderboard-button");
        _playerNameInput = root.Q<TextField>("player-name-input");
        _confirmNameButton = root.Q<Button>("confirm-name-button");
        _nameWarningLabel = root.Q<Label>("name-warning-label");

        // Get leaderboard panel components
        _leaderboardPanel = root.Q<VisualElement>("leaderboard-panel");
        _closeLeaderboardButton = root.Q<Button>("close-leaderboard-button");
        _refreshLeaderboardButton = root.Q<Button>("refresh-leaderboard-button");
        _levelDropdown = root.Q<DropdownField>("level-dropdown");
        _leaderboardScrollView = root.Q<ScrollView>("leaderboard-scroll-view");
        _playerRankLabel = root.Q<Label>("player-rank-label");

        // Setup button events
        if (_startButton != null)
            _startButton.clicked += StartGame;

        if (_leaderboardButton != null)
            _leaderboardButton.clicked += ShowLeaderboard;

        if (_confirmNameButton != null)
            _confirmNameButton.clicked += OnConfirmNameClicked;

        if (_closeLeaderboardButton != null)
            _closeLeaderboardButton.clicked += HideLeaderboard;

        if (_refreshLeaderboardButton != null)
            _refreshLeaderboardButton.clicked += RefreshLeaderboard;

        // Setup TextField Enter key event
        if (_playerNameInput != null)
        {
            _playerNameInput.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    evt.StopPropagation(); // Prevent event propagation
                    OnConfirmNameClicked();
                }
            }, TrickleDown.TrickleDown);
        }

        // Initialize
        InitializePlayerName();
        // Don't setup dropdown in Awake, do it when opening leaderboard panel instead
        
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
        Debug.Log("LootLocker session ready, refreshing leaderboard if needed");
        
        // If leaderboard is open and waiting for session, refresh it
        if (_isWaitingForSession && _leaderboardPanel != null && _leaderboardPanel.style.display == DisplayStyle.Flex)
        {
            _isWaitingForSession = false;
            RefreshLeaderboard();
        }
    }

    private void InitializePlayerName()
    {
        if (LeaderboardManager.Instance != null && _playerNameInput != null)
        {
            string currentName = LeaderboardManager.Instance.GetPlayerName();
            
            // If LeaderboardManager hasn't initialized yet, wait a moment
            if (string.IsNullOrEmpty(currentName))
            {
                // Delayed initialization to ensure LeaderboardManager has loaded
                StartCoroutine(DelayedInitializePlayerName());
            }
            else
            {
                _playerNameInput.value = currentName;
                Debug.Log($"Loaded player name: {currentName}");
            }
        }
        else if (_playerNameInput != null)
        {
            // If LeaderboardManager doesn't exist yet, delayed initialization
            StartCoroutine(DelayedInitializePlayerName());
        }
    }
    
    private IEnumerator DelayedInitializePlayerName()
    {
        // Wait 0.1 seconds for LeaderboardManager to initialize
        yield return new WaitForSeconds(0.1f);
        
        if (LeaderboardManager.Instance != null && _playerNameInput != null)
        {
            string currentName = LeaderboardManager.Instance.GetPlayerName();
            if (!string.IsNullOrEmpty(currentName))
            {
                _playerNameInput.value = currentName;
                Debug.Log($"Loaded player name: {currentName}");
            }
        }
    }

    private void SetupLevelDropdown()
    {
        Debug.Log("=== SetupLevelDropdown started ===");
        
        if (LeaderboardManager.Instance == null)
        {
            Debug.LogError("LeaderboardManager.Instance is null! Please ensure LeaderboardManager GameObject exists in scene.");
            return;
        }
        
        if (_levelDropdown == null)
        {
            Debug.LogError("_levelDropdown is null!");
            return;
        }

        Debug.Log($"LeaderboardManager exists, loading configuration...");

        // Get level list from LeaderboardManager
        _availableLevels.Clear();

        if (LeaderboardManager.Instance.leaderboardConfigs != null)
        {
            Debug.Log($"leaderboardConfigs is not null, length: {LeaderboardManager.Instance.leaderboardConfigs.Length}");
            
            if (LeaderboardManager.Instance.leaderboardConfigs.Length > 0)
            {
                foreach (var config in LeaderboardManager.Instance.leaderboardConfigs)
                {
                    Debug.Log($"  - Level: {config.levelName}, ID: {config.leaderboardId}");
                    _availableLevels.Add(config.levelName);
                }
            }
            else
            {
                Debug.LogWarning("leaderboardConfigs array length is 0");
            }
        }
        else
        {
            Debug.LogWarning("leaderboardConfigs is null");
        }

        if (_availableLevels.Count > 0)
        {
            Debug.Log($"Successfully loaded {_availableLevels.Count} level configurations");
            _levelDropdown.choices = _availableLevels;
            _levelDropdown.value = _availableLevels[0];
            _currentSelectedLevel = _availableLevels[0];
            Debug.Log($"Default selected level: {_currentSelectedLevel}");

            _levelDropdown.RegisterValueChangedCallback(evt =>
            {
                _currentSelectedLevel = evt.newValue;
                Debug.Log($"Switched level to: {_currentSelectedLevel}");
                RefreshLeaderboard();
            });
        }
        else
        {
            // No leaderboards configured
            Debug.LogWarning("No leaderboards configured! Please set Leaderboard Configs in LeaderboardManager Inspector.");
            _levelDropdown.choices = new List<string> { "Not Configured" };
            _levelDropdown.value = "Not Configured";
            _currentSelectedLevel = "";
        }
        
        Debug.Log("=== SetupLevelDropdown finished ===");
    }

    private void OnConfirmNameClicked()
    {
        if (_playerNameInput == null || LeaderboardManager.Instance == null) return;

        string newName = _playerNameInput.value.Trim();

        // Hide previous warning
        HideNameWarning();

        // Validate name length
        if (string.IsNullOrEmpty(newName))
        {
            ShowNameWarning("⚠ Please enter a player name");
            return;
        }

        if (newName.Length < 2)
        {
            ShowNameWarning("⚠ Name must be at least 2 characters");
            return;
        }

        if (newName.Length > 20)
        {
            ShowNameWarning("⚠ Name cannot exceed 20 characters");
            return;
        }

        // Name is valid, save and display success message
        LeaderboardManager.Instance.SetPlayerName(newName);
        ShowNameWarning("✅ Name updated to: " + newName, true);
        Debug.Log($"Name updated to: {newName}");

        // Auto-hide success message after 2 seconds
        StartCoroutine(HideWarningAfterDelay(2f));
    }

    private void ShowNameWarning(string message, bool isSuccess = false)
    {
        if (_nameWarningLabel == null) return;

        _nameWarningLabel.text = message;
        _nameWarningLabel.style.display = DisplayStyle.Flex;
        
        // Set color based on success status
        if (isSuccess)
        {
            _nameWarningLabel.style.color = new Color(100f/255f, 255f/255f, 100f/255f); // Light green
        }
        else
        {
            _nameWarningLabel.style.color = new Color(255f/255f, 100f/255f, 100f/255f); // Light red
        }
    }

    private void HideNameWarning()
    {
        if (_nameWarningLabel == null) return;
        _nameWarningLabel.style.display = DisplayStyle.None;
    }

    private IEnumerator HideWarningAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideNameWarning();
    }

    private void StartGame()
    {
        Debug.Log("Start Game button clicked! Loading scene: Level 1");
        SceneManager.LoadScene("Level 1");
    }

    private void ShowLeaderboard()
    {
        if (_leaderboardPanel == null) return;

        // Re-setup dropdown each time leaderboard opens to ensure LeaderboardManager is initialized
        SetupLevelDropdown();

        _leaderboardPanel.style.display = DisplayStyle.Flex;
        RefreshLeaderboard();
    }

    private void HideLeaderboard()
    {
        if (_leaderboardPanel == null) return;
        _leaderboardPanel.style.display = DisplayStyle.None;
        
        // Stop session checking when closing leaderboard
        if (_sessionCheckCoroutine != null)
        {
            StopCoroutine(_sessionCheckCoroutine);
            _sessionCheckCoroutine = null;
        }
        _isWaitingForSession = false;
    }

    private void RefreshLeaderboard()
    {
        if (LeaderboardManager.Instance == null) return;

        // Check if there's a valid level selection
        if (string.IsNullOrEmpty(_currentSelectedLevel))
        {
            Debug.LogWarning("No level selected or leaderboard not configured!");
            
            // Display error message
            if (_leaderboardScrollView != null)
            {
                _leaderboardScrollView.Clear();
                var errorLabel = new Label("⚠️ Please configure leaderboard in Unity Inspector");
                errorLabel.style.color = new Color(1f, 0.5f, 0.5f);
                errorLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                errorLabel.style.marginTop = 20;
                errorLabel.style.fontSize = 18;
                _leaderboardScrollView.Add(errorLabel);
                
                var helpLabel = new Label("\nSteps:\n1. Select LeaderboardManager GameObject\n2. Set Leaderboard Configs in Inspector\n3. Enter level name and corresponding Leaderboard ID");
                helpLabel.style.color = Color.white;
                helpLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                helpLabel.style.whiteSpace = WhiteSpace.Normal;
                helpLabel.style.fontSize = 14;
                _leaderboardScrollView.Add(helpLabel);
            }
            
            if (_playerRankLabel != null)
                _playerRankLabel.text = "Please configure leaderboard first";
            
            return;
        }
        
        // Check if LootLocker session is ready
        if (!LeaderboardManager.Instance.IsSessionReady())
        {
            Debug.Log("LootLocker session not ready yet, showing loading message");
            _isWaitingForSession = true;
            
            // Display loading message
            if (_leaderboardScrollView != null)
            {
                _leaderboardScrollView.Clear();
                var loadingContainer = new VisualElement();
                loadingContainer.style.alignItems = Align.Center;
                loadingContainer.style.justifyContent = Justify.Center;
                loadingContainer.style.flexGrow = 1;
                loadingContainer.style.paddingTop = 40;
                
                var loadingLabel = new Label("Connecting to LootLocker...");
                loadingLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
                loadingLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                loadingLabel.style.fontSize = 20;
                loadingLabel.style.marginBottom = 10;
                loadingContainer.Add(loadingLabel);
                
                var pleaseWaitLabel = new Label("Please wait...");
                pleaseWaitLabel.style.color = new Color(0.6f, 0.6f, 0.6f);
                pleaseWaitLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                pleaseWaitLabel.style.fontSize = 16;
                loadingContainer.Add(pleaseWaitLabel);
                
                _leaderboardScrollView.Add(loadingContainer);
            }
            
            if (_playerRankLabel != null)
                _playerRankLabel.text = "Connecting...";
            
            // Start checking for session readiness
            if (_sessionCheckCoroutine != null)
            {
                StopCoroutine(_sessionCheckCoroutine);
            }
            _sessionCheckCoroutine = StartCoroutine(CheckSessionAndRefresh());
            
            return;
        }
        
        // Session is ready, clear waiting flag
        _isWaitingForSession = false;

        // Clear existing content
        _leaderboardScrollView?.Clear();

        // Display loading message
        if (_playerRankLabel != null)
            _playerRankLabel.text = "Loading...";

        // Get leaderboard data
        LeaderboardManager.Instance.GetTopPlayers(10, OnLeaderboardLoaded, _currentSelectedLevel);

        // Get player rank
        LeaderboardManager.Instance.GetPlayerRank(OnPlayerRankLoaded, _currentSelectedLevel);
    }

    private void OnLeaderboardLoaded(LootLockerLeaderboardMember[] members)
    {
        if (_leaderboardScrollView == null) return;

        _leaderboardScrollView.Clear();

        if (members == null || members.Length == 0)
        {
            var emptyLabel = new Label("No leaderboard data yet");
            emptyLabel.style.color = Color.white;
            emptyLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            emptyLabel.style.marginTop = 20;
            _leaderboardScrollView.Add(emptyLabel);
            return;
        }

        // Add each leaderboard entry
        for (int i = 0; i < members.Length; i++)
        {
            var member = members[i];
            var entry = CreateLeaderboardEntry(member);
            _leaderboardScrollView.Add(entry);
        }
    }
    
    private VisualElement CreateLeaderboardEntry(LootLockerLeaderboardMember member)
    {
        var container = new VisualElement();
        container.style.flexDirection = FlexDirection.Row;
        container.style.paddingTop = 8;
        container.style.paddingBottom = 8;
        container.style.paddingLeft = 10;
        container.style.paddingRight = 10;
        container.style.marginBottom = 2;

        // Set background color based on rank
        if (member.rank == 1)
            container.style.backgroundColor = new Color(1f, 0.84f, 0f, 0.2f);
        else if (member.rank == 2)
            container.style.backgroundColor = new Color(0.75f, 0.75f, 0.75f, 0.2f);
        else if (member.rank == 3)
            container.style.backgroundColor = new Color(0.8f, 0.5f, 0.2f, 0.2f);
        else
            container.style.backgroundColor = new Color(0, 0, 0, 0.3f);

        // Rank label
        string rankText = "";
        if (member.rank == 1) rankText = "1";
        else if (member.rank == 2) rankText = "2";
        else if (member.rank == 3) rankText = "3";
        else rankText = member.rank.ToString();

        var rankLabel = new Label(rankText);
        rankLabel.style.width = 80;
        rankLabel.style.color = Color.white;
        rankLabel.style.fontSize = 16;
        container.Add(rankLabel);

        // Player name - extract from metadata
        string displayName = ExtractPlayerNameFromMetadata(member.metadata);
        
        if (string.IsNullOrEmpty(displayName))
        {
            // No name in metadata, use member_id (GUID)
            displayName = member.member_id;
            
            // If it's GUID format, simplify display
            if (displayName.Length > 20 && displayName.Contains("-"))
            {
                string shortId = displayName.Substring(0, 8);
                displayName = $"Player{shortId}";
            }
        }
        
        var nameLabel = new Label(displayName);
        nameLabel.style.flexGrow = 1;
        nameLabel.style.color = Color.white;
        nameLabel.style.fontSize = 16;
        container.Add(nameLabel);

        // Time
        float timeInSeconds = member.score / 1000f;
        string timeString = FormatTime(timeInSeconds);
        var timeLabel = new Label(timeString);
        timeLabel.style.width = 150;
        timeLabel.style.color = Color.white;
        timeLabel.style.fontSize = 16;
        container.Add(timeLabel);

        return container;
    }

    private void OnPlayerRankLoaded(int rank, int score)
    {
        if (_playerRankLabel == null) return;

        if (rank > 0)
        {
            float timeInSeconds = score / 1000f;
            string timeString = FormatTime(timeInSeconds);
            _playerRankLabel.text = $"Your Rank: #{rank} | Time Remaining: {timeString}";
        }
        else
        {
            _playerRankLabel.text = "You have no record on this level yet";
        }
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        int milliseconds = Mathf.FloorToInt((timeInSeconds * 1000f) % 1000f);
        return $"{minutes:00}:{seconds:00}.{milliseconds:000}";
    }

    // Extract player name from metadata JSON
    private string ExtractPlayerNameFromMetadata(string metadata)
    {
        if (string.IsNullOrEmpty(metadata))
        {
            return null;
        }

        try
        {
            // Simple JSON parsing (find "playerName":"xxx")
            int nameStart = metadata.IndexOf("\"playerName\":\"");
            if (nameStart >= 0)
            {
                nameStart += "\"playerName\":\"".Length;
                int nameEnd = metadata.IndexOf("\"", nameStart);
                if (nameEnd > nameStart)
                {
                    string playerName = metadata.Substring(nameStart, nameEnd - nameStart);
                    return playerName;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to parse metadata: {e.Message}");
        }

        return null;
    }
    
    // Check session readiness every 0.25 seconds and auto-refresh when ready
    private IEnumerator CheckSessionAndRefresh()
    {
        Debug.Log("Started checking for LootLocker session readiness...");
        
        while (_isWaitingForSession)
        {
            yield return new WaitForSeconds(0.25f);
            
            if (LeaderboardManager.Instance != null && LeaderboardManager.Instance.IsSessionReady())
            {
                Debug.Log("LootLocker session ready! Auto-refreshing leaderboard...");
                _isWaitingForSession = false;
                _sessionCheckCoroutine = null;
                
                // Auto-refresh leaderboard
                RefreshLeaderboard();
                yield break;
            }
        }
    }
}
