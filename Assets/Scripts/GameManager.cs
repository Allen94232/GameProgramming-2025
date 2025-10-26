using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
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
        if (scene.name != "MainMenuScene")
        {
            StartCoroutine(InitializeAfterSceneLoad());
        }
    }

    private System.Collections.IEnumerator InitializeAfterSceneLoad()
    {
        yield return null;
        
        FindSceneReferences();
        
        Init();
        
        if (GameUIController.Instance != null)
            GameUIController.Instance.Init();
        if (PlayerController.Instance != null)
            PlayerController.Instance.Init();
        if (MoodController.Instance != null)
            MoodController.Instance.Init();
    }

    [Header("Game Parameters")]
    public float maxMood = 200;
    public float initialMood = 100;
    public float gameTime = 180f; // in seconds

    // [Header("GameObjects and Transforms")]
    // public GameObject player;
    // public Transform playerStartPoint;
    // public GameObject destinationPoint;
    // [Header("Game UI")]
    // public TextMeshProUGUI timerUI;
    // public GameObject winUI;
    // public GameObject loseUI;

    private GameObject winUI;
    private GameObject loseUI;

    private bool isGameStarted = false;
    private bool isGameWin = false;
    private bool isGameLose = false;

    private float timer;

    private void Start()
    {
        timer = gameTime;
        FindSceneReferences();
        Init();
    }

    private void FindSceneReferences()
    {
        winUI = GameObject.Find("WinUI");
        loseUI = GameObject.Find("LoseUI");
        
        if (winUI == null)
            Debug.LogWarning("GameManager: Cannot find WinUI in scene!");
        if (loseUI == null)
            Debug.LogWarning("GameManager: Cannot find LoseUI in scene!");
    }

    private void Update()
    {
        if (!isGameStarted)
        {
            //if (Input.GetKeyDown(KeyCode.Space))
            //{
            //    StartGame();
            //}

            //return;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            //ResetGame();
        }

        if (isGameWin || isGameLose)
        {
            CheckIfGameOver();
            return;
        }

        if (GameUIController.Instance != null)
            GameUIController.Instance.UpdateTimer();
    }

    private void Init()
    {
        isGameStarted = false;
        isGameWin = false;
        isGameLose = false;

        timer = gameTime;

        if (winUI != null)
            winUI.SetActive(false);

        if (loseUI != null)
            loseUI.SetActive(false);
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
            if (winUI != null)
            {
                winUI.SetActive(true);
            }
            else
            {
                Debug.LogError("GameManager: WinUI is null when trying to show win screen!");
            }

            Time.timeScale = 0f;
        }
        else if (isGameLose)
        {
            Debug.Log("You Lose!");
            if (loseUI != null)
            {
                loseUI.SetActive(true);
            }
            else
            {
                Debug.LogError("GameManager: LoseUI is null when trying to show lose screen!");
            }

            Time.timeScale = 0f;
        }
    }
}
