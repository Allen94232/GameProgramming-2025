using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    private bool isFirstSceneLoad = true;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset first load flag when returning to main menu
        if (scene.name == "MainMenuScene")
        {
            // destroy game manager when returning to main menu
            isFirstSceneLoad = true;
            Destroy(gameObject);
            return;
        }

        // Handle game scene loading
        isFirstSceneLoad = false;
        
        // Immediately find and hide UI to prevent visual delay
        FindSceneReferences();
        //HideGameOverUI();

        StartCoroutine(ToggleGameObjectsAfterDelay());

        // Then initialize other controllers
        StartCoroutine(InitializeAfterSceneLoad());
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
    public float gameTime = 180f;

    //private GameObject winUI;
    //private GameObject loseUI;

    private GameObject speedDetectors;
    private GameObject sprinklers;

    private bool isGameStarted = false;
    private bool isGameWin = false;
    private bool isGameLose = false;

    private float timer;

    private void Start()
    {
        // Only initialize on first scene load (before OnSceneLoaded triggers)
        if (isFirstSceneLoad)
        {
            timer = gameTime;
            FindSceneReferences();
            //HideGameOverUI();
            StartCoroutine(ToggleGameObjectsAfterDelay());
            Init();
        }
    }

    // Find UI GameObjects in the current scene
    private void FindSceneReferences()
    {
        //winUI = GameObject.Find("WinUI");
        //loseUI = GameObject.Find("LoseUI");

        speedDetectors = GameObject.Find("SpeedDetectors");
        sprinklers = GameObject.Find("Sprinklers");

       /* if (winUI == null)
            Debug.LogWarning("GameManager: Cannot find WinUI in scene!");
          if (loseUI == null)
            Debug.LogWarning("GameManager: Cannot find LoseUI in scene!"); */
    }

    // Immediately hide game over UI

    /*private void HideGameOverUI()
    {
        if (winUI != null)
            winUI.SetActive(false);
        if (loseUI != null)
            loseUI.SetActive(false);
    }*/

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
        // Don't update if in main menu
        if (SceneManager.GetActiveScene().name == "MainMenuScene")
            return;
            
        if (!isGameStarted)
        {
            //if (Input.GetKeyDown(KeyCode.Space))
            //{
            //    StartGame();
            //}

            //return;
        }

        if (isGameWin || isGameLose)
        {
            CheckIfGameOver();
            return;
        }

        if (GameUIController.Instance != null)
            GameUIController.Instance.UpdateTimer();
    }

    // Reset game state flags
    private void Init()
    {
        Debug.Log("GameManager.Init() called");
        
        isGameStarted = false;
        isGameWin = false;
        isGameLose = false;

        timer = gameTime;
    }

    public void StartGame()
    {
        isGameStarted = true;
    }

    public void ResetGame()
    {
        Init();
        
        if (GameUIController.Instance != null)
            GameUIController.Instance.Init();
        if (PlayerController.Instance != null)
            PlayerController.Instance.Init();
        if (MoodController.Instance != null)
            MoodController.Instance.Init();
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
            if (GameUIController.Instance != null)
            {
                GameUIController.Instance.ShowWinScreen();
            }
            Time.timeScale = 0f;
            isGameWin = false;
        }

        else if (isGameLose)
        {
            Debug.Log("You Lose!");
            if (GameUIController.Instance != null)
            {
                GameUIController.Instance.ShowLoseScreen();
            }
            Time.timeScale = 0f;
            isGameLose = false;
        }

        /*if (winUI == null || loseUI == null)
        {
            Debug.LogError("GameManager: UI references are null in CheckIfGameOver!");
            return;
        }
        
        if (isGameWin && !winUI.activeSelf)
        {
            Debug.Log("You Win!");
            winUI.SetActive(true);
            Time.timeScale = 0f;
        }
        
        else if (isGameLose && !loseUI.activeSelf)
        {
            Debug.Log("You Lose!");
            loseUI.SetActive(true);
            Time.timeScale = 0f;
        }
        */
    }
}
