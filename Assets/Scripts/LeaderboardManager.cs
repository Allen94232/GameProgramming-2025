using UnityEngine;
using LootLocker.Requests;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }
    
    [Header("LootLocker Settings")]
    [Tooltip("Default leaderboard ID")]
    public int defaultLeaderboardId = 0;
    
    [Header("Multi-Level Leaderboard Configuration")]
    [Tooltip("Leaderboard ID settings for each level")]
    public LeaderboardConfig[] leaderboardConfigs;
    
    private bool isSessionStarted = false;
    private string currentPlayerID = "";
    private string playerIdentifier = ""; // Permanent unique identifier (GUID)
    private string playerName = "";
    
    // Event for session ready
    public event System.Action OnSessionReady;
    
    // Player name related constants
    private const string PLAYER_NAME_KEY = "PlayerName";
    private const string PLAYER_IDENTIFIER_KEY = "LootLockerPlayerIdentifier";
    
    [System.Serializable]
    public class LeaderboardConfig
    {
        [Tooltip("Level name")]
        public string levelName;
        
        [Tooltip("Leaderboard ID for this level")]
        public int leaderboardId;
    }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        LoadPlayerName();
        StartGuestSession();
    }
    
    // Load or generate player name
    private void LoadPlayerName()
    {
        playerName = PlayerPrefs.GetString(PLAYER_NAME_KEY, "");
        
        if (string.IsNullOrEmpty(playerName))
        {
            // First time playing, generate a random name
            playerName = "Player" + Random.Range(1000, 9999);
            PlayerPrefs.SetString(PLAYER_NAME_KEY, playerName);
            PlayerPrefs.Save();
            Debug.Log($"Generated new player name: {playerName}");
        }
        else
        {
            Debug.Log($"Loaded player name: {playerName}");
        }
    }
    
    // Set player name (allow player customization)
    public void SetPlayerName(string newName)
    {
        if (string.IsNullOrEmpty(newName) || newName.Length < 2)
        {
            Debug.LogWarning("Player name too short!");
            return;
        }
        
        playerName = newName;
        PlayerPrefs.SetString(PLAYER_NAME_KEY, playerName);
        PlayerPrefs.Save();
        Debug.Log($"Player name updated to: {playerName}");
        
        // Sync to LootLocker server (using Player Name API)
        UpdatePlayerNameOnServer(newName);
    }
    
    // Update player name on LootLocker server
    private void UpdatePlayerNameOnServer(string newName)
    {
        if (!isSessionStarted)
        {
            Debug.LogWarning("LootLocker session not started, cannot update server name");
            return;
        }
        
        LootLockerSDKManager.SetPlayerName(newName, (response) =>
        {
            if (response.success)
            {
                Debug.Log($"Server name updated: {newName}");
            }
            else
            {
                Debug.LogWarning($"Failed to update server name: {response.errorData?.message}");
            }
        });
    }
    
    // Get current player name
    public string GetPlayerName()
    {
        return playerName;
    }
    
    // Get leaderboard ID for specified level
    public int GetLeaderboardId(string levelName)
    {
        if (leaderboardConfigs != null)
        {
            foreach (var config in leaderboardConfigs)
            {
                if (config.levelName == levelName)
                {
                    return config.leaderboardId;
                }
            }
        }
        
        Debug.LogWarning($"Level '{levelName}' leaderboard config not found, using default ID");
        return defaultLeaderboardId;
    }
    
    // Get leaderboard ID for current scene
    public int GetCurrentLeaderboardId()
    {
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        return GetLeaderboardId(currentSceneName);
    }
    
    // Start guest session (no login required)
    private void StartGuestSession()
    {
        // Use a persistent unique identifier for the player
        playerIdentifier = PlayerPrefs.GetString(PLAYER_IDENTIFIER_KEY, "");
        
        if (string.IsNullOrEmpty(playerIdentifier))
        {
            // First time running, generate a unique ID
            playerIdentifier = System.Guid.NewGuid().ToString();
            PlayerPrefs.SetString(PLAYER_IDENTIFIER_KEY, playerIdentifier);
            PlayerPrefs.Save();
            Debug.Log($"First run, generated new Player Identifier: {playerIdentifier}");
        }
        else
        {
            Debug.Log($"Loaded existing Player Identifier: {playerIdentifier}");
        }
        
        LootLockerSDKManager.StartGuestSession(playerIdentifier, (response) =>
        {
            if (response.success)
            {
                Debug.Log($"Guest login successful! Player ID: {response.player_id}");
                isSessionStarted = true;
                currentPlayerID = response.player_id.ToString();
                
                // Trigger event to notify listeners that session is ready
                OnSessionReady?.Invoke();
            }
            else
            {
                Debug.LogError($"Guest login failed: {response.errorData.message}");
                isSessionStarted = false;
            }
        });
    }
    
    // Get player's unique identifier (for leaderboard queries)
    public string GetPlayerIdentifier()
    {
        return playerIdentifier;
    }
    
    // Check if session is ready
    public bool IsSessionReady()
    {
        return isSessionStarted;
    }
    
    // Submit remaining time to leaderboard (higher is better)
    public void SubmitRemainingTime(float timeInSeconds, string levelName = null, string customPlayerName = null)
    {
        if (!isSessionStarted)
        {
            Debug.LogWarning("LootLocker session not started yet!");
            return;
        }
        
        // Determine which leaderboard to use
        int leaderboardId = string.IsNullOrEmpty(levelName) ? GetCurrentLeaderboardId() : GetLeaderboardId(levelName);
        
        // Use custom name or default player name
        string nameToDisplay = string.IsNullOrEmpty(customPlayerName) ? playerName : customPlayerName;
        
        Debug.Log($"=== Preparing to submit score ===");
        Debug.Log($"Player name: {nameToDisplay}");
        Debug.Log($"Player Identifier (GUID): {playerIdentifier}");
        Debug.Log($"Leaderboard ID: {leaderboardId}");
        
        // Convert time to milliseconds for better precision
        int scoreInMilliseconds = Mathf.RoundToInt(timeInSeconds * 1000);
        
        // Use pure GUID as member_id to ensure same player recognition after name change
        // Store player name in metadata for custom display
        string level = string.IsNullOrEmpty(levelName) ? UnityEngine.SceneManagement.SceneManager.GetActiveScene().name : levelName;
        string metadata = $"{{\"playerName\":\"{nameToDisplay}\",\"level\":\"{level}\",\"mood\":\"{MoodController.Instance.GetMoodValue()}\",\"playerId\":\"{currentPlayerID}\"}}";
        
        Debug.Log($"Submitting score - GUID: {playerIdentifier}, Name: {nameToDisplay}, Metadata: {metadata}");
        
        LootLockerSDKManager.SubmitScore(playerIdentifier, scoreInMilliseconds, leaderboardId.ToString(), metadata, (response) =>
        {
            if (response.success)
            {
                Debug.Log($"Score submitted successfully! Level: {level}, Player: {nameToDisplay}, Remaining time: {timeInSeconds:F2}s, Rank: {response.rank}");
            }
            else
            {
                Debug.LogError($"Score submission failed: {response.errorData.message}");
            }
        });
    }
    
    
    // Get top players from leaderboard
    public void GetTopPlayers(int count, System.Action<LootLockerLeaderboardMember[]> onComplete, string levelName = null)
    {
        if (!isSessionStarted)
        {
            Debug.LogWarning("LootLocker session not started yet!");
            return;
        }
        
        // Determine which leaderboard to use
        int leaderboardId = string.IsNullOrEmpty(levelName) ? GetCurrentLeaderboardId() : GetLeaderboardId(levelName);
        
        LootLockerSDKManager.GetScoreList(leaderboardId.ToString(), count, 0, (response) =>
        {
            if (response.success)
            {
                // check if items is null (leaderboard might be empty)
                if (response.items != null && response.items.Length > 0)
                {
                    Debug.Log($"Retrieved {response.items.Length} leaderboard entries from leaderboard {leaderboardId}");
                    onComplete?.Invoke(response.items);
                }
                else
                {
                    Debug.Log($"Leaderboard {leaderboardId} is empty");
                    onComplete?.Invoke(new LootLockerLeaderboardMember[0]);
                }
            }
            else
            {
                Debug.LogError("Error getting leaderboard: " + response.errorData.message);
                onComplete?.Invoke(new LootLockerLeaderboardMember[0]);
            }
        });
    }
    
    // Get player's rank and score
    public void GetPlayerRank(System.Action<int, int> onComplete, string levelName = null)
    {
        if (!isSessionStarted)
        {
            Debug.LogWarning("LootLocker session not started yet!");
            return;
        }
        
        // Determine which leaderboard to use
        int leaderboardId = string.IsNullOrEmpty(levelName) ? GetCurrentLeaderboardId() : GetLeaderboardId(levelName);
        
        // Query using pure GUID, can still find own score after name change
        Debug.Log($"Querying player rank, using GUID: {playerIdentifier}");
        
        LootLockerSDKManager.GetMemberRank(leaderboardId.ToString(), playerIdentifier, (response) =>
        {
            if (response != null && response.success && response.rank > 0)
            {
                Debug.Log($"Player rank found: Rank {response.rank}, Score: {response.score}");
                onComplete?.Invoke(response.rank, response.score);
            }
            else
            {
                if (response != null && !response.success)
                {
                    Debug.Log($"Player has not submitted score to leaderboard {leaderboardId} yet");
                }
                onComplete?.Invoke(-1, 0);
            }
        });
    }
}