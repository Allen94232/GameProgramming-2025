using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Level Configuration")]
    public string[] availableLevels = new string[] { "Tutorial", "Level 1", "Level 2" }; // Add all your level scene names here
    
    private string currentLevelName = "";
    private bool isInGameLevel = false;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            Debug.Log("GameManager: Singleton created and persisting across scenes");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"GameManager: Scene loaded - {scene.name}");
        
        // Check if this is MainMenuScene
        if (scene.name == "MainMenuScene")
        {
            isInGameLevel = false;
            currentLevelName = "";
            Debug.Log("GameManager: Returned to Main Menu");
            return;
        }

        // Check if this is a game level
        bool isGameLevel = IsGameLevel(scene.name);
        
        if (isGameLevel)
        {
            isInGameLevel = true;
            currentLevelName = scene.name;
            
            Debug.Log($"GameManager: Starting level - {currentLevelName}");
            
            // Find scene-specific objects
            FindSceneReferences();

            // Disable certain objects temporarily
            StartCoroutine(ToggleGameObjectsAfterDelay());

            // Initialize controllers after scene is fully loaded
            StartCoroutine(InitializeAfterSceneLoad());
        }
    }
    
    // Check if scene name is a game level
    private bool IsGameLevel(string sceneName)
    {
        foreach (string levelName in availableLevels)
        {
            if (sceneName == levelName)
                return true;
        }
        return false;
    }

    private System.Collections.IEnumerator InitializeAfterSceneLoad()
    {
        // Wait one frame to ensure all Awake methods complete
        yield return null;
        
        Init();
        
        if (GameUIController.Instance != null)
            GameUIController.Instance.Init();
        else
            Debug.LogError("GameManager: GameUIController.Instance is null after scene load!");
            
        if (PlayerController.Instance != null)
            PlayerController.Instance.Init();
        if (MoodController.Instance != null)
            MoodController.Instance.Init();
    }

    [Header("Game Parameters")]
    public float maxMood = 200;
    public float initialMood = 100;
    
    [System.Serializable]
    public class LevelTimeConfig
    {
        public string levelName;
        public float timeLimit;
    }
    
    [Header("Level Time Configuration")]
    public LevelTimeConfig[] levelTimeConfigs = new LevelTimeConfig[]
    {
        new LevelTimeConfig { levelName = "Tutorial", timeLimit = 600f },  // 10分鐘
        new LevelTimeConfig { levelName = "Level 1", timeLimit = 180f },   // 3分鐘
        new LevelTimeConfig { levelName = "Level 2", timeLimit = 360f }    // 6分鐘
    };

    //private GameObject winUI;
    //private GameObject loseUI;

    private GameObject speedDetectors;
    private GameObject sprinklers;

    private bool isGameWin = false;
    private bool isGameLose = false;

    private float timer;

    // Remove Start method - initialization now handled by OnSceneLoaded
    // Start() is no longer needed with DontDestroyOnLoad pattern

    // Find UI GameObjects in the current scene
    private void FindSceneReferences()
    {
        speedDetectors = GameObject.Find("SpeedDetectors");
        sprinklers = GameObject.Find("Sprinklers");
    }

    // Disable speed detectors and sprinklers and enable them after 0.5 seconds
    private System.Collections.IEnumerator ToggleGameObjectsAfterDelay()
    {
        if (speedDetectors != null)
            speedDetectors.SetActive(false);
        if (sprinklers != null)
            sprinklers.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        if (speedDetectors != null)
            speedDetectors.SetActive(true);
        if (sprinklers != null)
            sprinklers.SetActive(true);
    }

    private void Update()
    {
        // Only update during game levels
        if (!isInGameLevel)
            return;

        // Check game over only once when flags are set
        if (isGameWin || isGameLose)
        {
            CheckIfGameOver();
            // Stop updating after game over is processed
            return;
        }

        if (GameUIController.Instance != null)
            GameUIController.Instance.UpdateTimer();
    }

    // Reset game state flags
    private void Init()
    {
        Debug.Log($"GameManager.Init() called for level: {currentLevelName}");
        
        isGameWin = false;
        isGameLose = false;

        // 根據當前關卡設定時間限制
        float levelTime = GetLevelTimeLimit(currentLevelName);
        timer = levelTime;
        
        Debug.Log($"GameManager: Level '{currentLevelName}' time limit set to {levelTime} seconds");
    }
    
    // 獲取指定關卡的時間限制
    private float GetLevelTimeLimit(string levelName)
    {
        foreach (var config in levelTimeConfigs)
        {
            if (config.levelName == levelName)
            {
                return config.timeLimit;
            }
        }
        
        // 如果找不到配置，返回預設值
        Debug.LogWarning($"GameManager: No time config found for level '{levelName}', using default 180s");
        return 180f;
    }
    
    // 獲取當前關卡的時間限制（公開方法供其他腳本使用）
    public float GetCurrentLevelTimeLimit()
    {
        return GetLevelTimeLimit(currentLevelName);
    }

    public void ResetGame()
    {
        Debug.Log($"Resetting level: {currentLevelName}");
        Init();
        
        if (GameUIController.Instance != null)
            GameUIController.Instance.Init();
        if (PlayerController.Instance != null)
            PlayerController.Instance.Init();
        if (MoodController.Instance != null)
            MoodController.Instance.Init();
    }
    
    public void LoadLevel(string levelName)
    {
        if (!IsGameLevel(levelName))
        {
            Debug.LogError($"GameManager: '{levelName}' is not a valid game level!");
            return;
        }
        
        Debug.Log($"GameManager: Loading level - {levelName}");
        Time.timeScale = 1f; // Ensure time is running
        SceneManager.LoadScene(levelName);
    }
    
    public void RestartCurrentLevel()
    {
        if (string.IsNullOrEmpty(currentLevelName))
        {
            Debug.LogError("GameManager: No current level to restart!");
            return;
        }
        
        Debug.Log($"GameManager: Restarting level - {currentLevelName}");
        Time.timeScale = 1f;
        SceneManager.LoadScene(currentLevelName);
    }
    
    public void ReturnToMainMenu()
    {
        Debug.Log("GameManager: Returning to Main Menu");
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
    }
    
    public string GetCurrentLevelName()
    {
        return currentLevelName;
    }
    
    public bool IsInGameLevel()
    {
        return isInGameLevel;
    }

    public void GameWin()
    {
        if (isGameWin || isGameLose)
            return;
        isGameWin = true;
    }

    public void GameLose()
    {
        if (isGameWin || isGameLose)
            return;
           isGameLose = true;
    }

    private void CheckIfGameOver()
    {
        if (isGameWin)
        {
            Debug.Log("You Win!");
            
            // Reset flag immediately to prevent repeated execution
            isGameWin = false;

            // Get remaining time (higher is better)
            float remainingTime = GameUIController.Instance != null ?
                GameUIController.Instance.GetRemainingTime() : 0;

            // Submit to leaderboard
            if (LeaderboardManager.Instance != null)
            {
                LeaderboardManager.Instance.SubmitRemainingTime(remainingTime);
            }

            // Play win sound effect
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayWinSFX();
            }
            
            if (GameUIController.Instance != null)
            {
                GameUIController.Instance.ShowWinScreen();
            }

            Time.timeScale = 0f;
        }

        else if (isGameLose)
        {
            Debug.Log("You Lose!");
            
            // Reset flag immediately to prevent repeated execution
            isGameLose = false;
            
            // Play lose sound effect
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayLoseSFX();
            }
            
            if (GameUIController.Instance != null)
            {
                GameUIController.Instance.ShowLoseScreen();
            }
            
            Time.timeScale = 0f;
        }
    }
}
