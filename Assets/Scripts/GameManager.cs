using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private void Awake()
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

    [Header("GameObjects and Transforms")]
    public GameObject player;
    public Transform playerStartPoint;
    public GameObject destinationPoint;

    [Header("Game Timer")]
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
    }

    private void Update()
    {
        if (!isGameStarted)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartGame();
            }

            return;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetGame();
        }

        TimerCountdown();

        CheckIfGameOver();
    }

    private void Init()
    {
        isGameStarted = false;
        isGameWin = false;
        isGameLose = false;

        timer = gameTime;
        UpdateTimerUI();

        winUI.SetActive(false);
        loseUI.SetActive(false);
    }

    public void StartGame()
    {
        isGameStarted = true;
    }

    public void ResetGame()
    {
        Init();
    }

    private void GameWin()
    {
        isGameWin = true;
    }

    private void GameLose()
    {
        isGameLose = true;
    }

    private void TimerCountdown()
    {
        if (isGameWin || isGameLose || !isGameStarted)
        {
            return;
        }
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = 0f;
            if (!isGameWin)
                isGameLose = true;
        }
    }

    private void UpdateTimerUI()
    {
        timerUI.text = Mathf.CeilToInt(timer).ToString();
    }

    private void CheckIfGameOver()
    {
        if (isGameWin)
        {
            Debug.Log("You Win!");
            if (winUI != null)
                winUI.SetActive(true);
            //ResetGame();
        }
        else if (isGameLose)
        {
            Debug.Log("You Lose!");
            if (loseUI != null)
                loseUI.SetActive(true);
            //ResetGame();
        }
        else
        {
            return;
        }
    }
}
