using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement; // <-- IMPORTANT: Add this line!

public class MainMenuController : MonoBehaviour
{
    // This script is now much simpler!
    private UIDocument _uiDocument;
    private Button _startButton;
    private Button _leaderboardButton;

    private void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
        var root = _uiDocument.rootVisualElement;
        _startButton = root.Q<Button>("start-game-button");
        _leaderboardButton = root.Q<Button>("leaderboard-button");

        if (_startButton != null)
        {
            _startButton.clicked += StartGame;
        }

        if (_leaderboardButton != null)
        {
            // Future 
        }
    }

    private void StartGame()
    {
        Debug.Log("Start Game button clicked! Loading scene: Map1");
        SceneManager.LoadScene("Map1");
    }
}