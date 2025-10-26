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
        
        if (GameUIController.Instance != null)
            GameUIController.Instance.Init();
        if (PlayerController.Instance != null)
            PlayerController.Instance.Init();
        if (MoodController.Instance != null)
            MoodController.Instance.Init();
    }

    [Header("GameObjects and Transforms")]
    public GameObject player;
    public Transform playerStartPoint;
    public GameObject destinationPoint;

    [Header("Game Parameters")]
    public float maxMood = 100;
    public float initialMood = 50;
    public float gameTime = 180f; // in seconds

    [Header("Game UI")]
    public TextMeshProUGUI timerUI;
    public GameObject winUI;
    public GameObject loseUI;

    private bool isGameStarted = false;
    private bool isGameWin = false;
    private bool isGameLose = false;

    private float timer;

    private void Start()
    {
        timer = gameTime;

        Init();
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
            ResetGame();
        }

        if (isGameWin || isGameLose)
        {
            CheckIfGameOver();
            return;
        }


        GameUIController.Instance.UpdateTimer();

        // CheckIfGameOver();
    }

    private void Init()
    {
        isGameStarted = false;
        isGameWin = false;
        isGameLose = false;

        timer = gameTime;
        // UpdateTimerUI();

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
        GameUIController.Instance.Init();
        PlayerController.Instance.Init();
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

    //private void TimerCountdown()
    //{
    //    if (isGameWin || isGameLose || !isGameStarted)
    //    {
    //        return;
    //    }
    //    timer -= Time.deltaTime;
    //    if (timer <= 0f)
    //    {
    //        timer = 0f;
    //        if (!isGameWin)
    //            isGameLose = true;
    //    }
    //}

    //private void UpdateTimerUI()
    //{
    //    timerUI.text = Mathf.CeilToInt(timer).ToString();
    //}

    private void CheckIfGameOver()
    {
        if (isGameWin)
        {
            Debug.Log("You Win!");
            if (winUI != null)
                winUI.SetActive(true);

            Time.timeScale = 0f;
            //ResetGame();
        }
        else if (isGameLose)
        {
            Debug.Log("You Lose!");
            if (loseUI != null)
                loseUI.SetActive(true);

            Time.timeScale = 0f;
            //ResetGame();
        }
        else
        {
            return;
        }
    }
}
