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
    private Button _settingsButton;
    private Button _leaderboardButton;
    private TextField _playerNameInput;
    private Button _confirmNameButton;
    private Label _nameWarningLabel;

    // Settings panel components
    private VisualElement _settingsPanel;
    private Button _closeSettingsButton;
    private Slider _bgmVolumeSlider;
    private Slider _sfxVolumeSlider;
    private Label _bgmVolumeLabel;
    private Label _sfxVolumeLabel;

    // Help panel components
    private Button _helpButton;
    private VisualElement _helpPanel;
    private Button _closeHelpButton;
    private ScrollView _helpScrollView;

    // Level selection panel components
    private VisualElement _levelSelectionPanel;
    private Button _closeLevelSelectionButton;
    private Button _tutorialButton;
    private Button _level1Button;
    private Button _level2Button;

    // Leaderboard panel components
    private VisualElement _leaderboardPanel;
    private Button _closeLeaderboardButton;
    private Button _refreshLeaderboardButton;
    //private DropdownField _levelDropdown;
    private Button _prevLevelButton;
    private Button _nextLevelButton;
    private Label _currentLevelLabel;
    private ScrollView _leaderboardScrollView;
    private Label _playerRankLabel;

    private List<string> _availableLevels = new List<string>();
    private string _currentSelectedLevel = "";
    private int _currentLevelIndex = 0;
    
    private bool _isWaitingForSession = false;
    private Coroutine _sessionCheckCoroutine = null;

    private void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
        var root = _uiDocument.rootVisualElement;

        // Get main menu components
        _startButton = root.Q<Button>("start-game-button");
        _settingsButton = root.Q<Button>("settings-button");
        _leaderboardButton = root.Q<Button>("leaderboard-button");
        _playerNameInput = root.Q<TextField>("player-name-input");
        _confirmNameButton = root.Q<Button>("confirm-name-button");
        _nameWarningLabel = root.Q<Label>("name-warning-label");

        // Get settings panel components
        _settingsPanel = root.Q<VisualElement>("settings-panel");
        _closeSettingsButton = root.Q<Button>("close-settings-button");
        _bgmVolumeSlider = root.Q<Slider>("bgm-volume-slider");
        _sfxVolumeSlider = root.Q<Slider>("sfx-volume-slider");
        _bgmVolumeLabel = root.Q<Label>("bgm-volume-label");
        _sfxVolumeLabel = root.Q<Label>("sfx-volume-label");

        // Get help panel components
        _helpButton = root.Q<Button>("help-button");
        _helpPanel = root.Q<VisualElement>("help-panel");
        _closeHelpButton = root.Q<Button>("close-help-button");
        _helpScrollView = root.Q<ScrollView>("help-scroll-view");

        // Get level selection panel components
        _levelSelectionPanel = root.Q<VisualElement>("level-selection-panel");
        _closeLevelSelectionButton = root.Q<Button>("close-level-selection-button");
        _tutorialButton = root.Q<Button>("tutorial-button");
        _level1Button = root.Q<Button>("level-1-button");
        _level2Button = root.Q<Button>("level-2-button");

        // Get leaderboard panel components
        _leaderboardPanel = root.Q<VisualElement>("leaderboard-panel");
        _closeLeaderboardButton = root.Q<Button>("close-leaderboard-button");
        _refreshLeaderboardButton = root.Q<Button>("refresh-leaderboard-button");
        //_levelDropdown = root.Q<DropdownField>("level-dropdown");
        _prevLevelButton = root.Q<Button>("prev-level-button");
        _nextLevelButton = root.Q<Button>("next-level-button");
        _currentLevelLabel = root.Q<Label>("current-level-label");
        _leaderboardScrollView = root.Q<ScrollView>("leaderboard-scroll-view");
        _playerRankLabel = root.Q<Label>("player-rank-label");

        // Setup button events (with sound effects)
        if (_startButton != null)
        {
            _startButton.clicked += () => { PlayButtonSound(); ShowLevelSelection(); };
            _startButton.RegisterCallback<MouseEnterEvent>(evt => PlayButtonHoverSound());
        }

        if (_helpButton != null)
        {
            _helpButton.clicked += () => { PlayButtonSound(); ShowHelp(); };
            _helpButton.RegisterCallback<MouseEnterEvent>(evt => PlayButtonHoverSound());
        }

        if (_settingsButton != null)
        {
            _settingsButton.clicked += () => { PlayButtonSound(); ShowSettings(); };
            _settingsButton.RegisterCallback<MouseEnterEvent>(evt => PlayButtonHoverSound());
        }

        if (_leaderboardButton != null)
        {
            _leaderboardButton.clicked += () => { PlayButtonSound(); ShowLeaderboard(); };
            _leaderboardButton.RegisterCallback<MouseEnterEvent>(evt => PlayButtonHoverSound());
        }

        if (_confirmNameButton != null)
        {
            _confirmNameButton.clicked += () => { PlayButtonSound(); OnConfirmNameClicked(); };
            _confirmNameButton.RegisterCallback<MouseEnterEvent>(evt => PlayButtonHoverSound());
        }

        if (_closeHelpButton != null)
        {
            _closeHelpButton.clicked += () => { PlayButtonSound(); HideHelp(); };
            _closeHelpButton.RegisterCallback<MouseEnterEvent>(evt => PlayButtonHoverSound());
        }

        if (_closeLevelSelectionButton != null)
        {
            _closeLevelSelectionButton.clicked += () => { PlayButtonSound(); HideLevelSelection(); };
            _closeLevelSelectionButton.RegisterCallback<MouseEnterEvent>(evt => PlayButtonHoverSound());
        }

        if (_tutorialButton != null)
        {
            _tutorialButton.clicked += () => { PlayButtonSound(); StartLevel("Tutorial"); };
            _tutorialButton.RegisterCallback<MouseEnterEvent>(evt => PlayButtonHoverSound());
        }

        if (_level1Button != null)
        {
            _level1Button.clicked += () => { PlayButtonSound(); StartLevel("Level 1"); };
            _level1Button.RegisterCallback<MouseEnterEvent>(evt => PlayButtonHoverSound());
        }

        if (_level2Button != null)
        {
            _level2Button.clicked += () => { PlayButtonSound(); StartLevel("Level 2"); };
            _level2Button.RegisterCallback<MouseEnterEvent>(evt => PlayButtonHoverSound());
        }

        if (_prevLevelButton != null)
            _prevLevelButton.clicked += () => { ChangeLevel(-1); PlayButtonSound(); };
            
        if (_nextLevelButton != null)
            _nextLevelButton.clicked += () => { ChangeLevel(1); PlayButtonSound(); };

        if (_closeLeaderboardButton != null)
        {
            _closeLeaderboardButton.clicked += () => { PlayButtonSound(); HideLeaderboard(); };
            _closeLeaderboardButton.RegisterCallback<MouseEnterEvent>(evt => PlayButtonHoverSound());
        }

        if (_refreshLeaderboardButton != null)
        {
            _refreshLeaderboardButton.clicked += () => { PlayButtonSound(); RefreshLeaderboard(); };
            _refreshLeaderboardButton.RegisterCallback<MouseEnterEvent>(evt => PlayButtonHoverSound());
        }

        // Setup settings panel events
        if (_closeSettingsButton != null)
        {
            _closeSettingsButton.clicked += () => { PlayButtonSound(); HideSettings(); };
            _closeSettingsButton.RegisterCallback<MouseEnterEvent>(evt => PlayButtonHoverSound());
        }

        // Setup volume sliders
        if (_bgmVolumeSlider != null)
        {
            _bgmVolumeSlider.RegisterValueChangedCallback(OnBGMVolumeChanged);
            // Initialize from AudioManager
            if (AudioManager.Instance != null)
            {
                _bgmVolumeSlider.value = AudioManager.Instance.GetBGMVolume();
                UpdateBGMVolumeLabel(_bgmVolumeSlider.value);
            }
        }

        if (_sfxVolumeSlider != null)
        {
            _sfxVolumeSlider.RegisterValueChangedCallback(OnSFXVolumeChanged);
            // Initialize from AudioManager
            if (AudioManager.Instance != null)
            {
                _sfxVolumeSlider.value = AudioManager.Instance.GetSFXVolume();
                UpdateSFXVolumeLabel(_sfxVolumeSlider.value);
            }
        }

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
        InitializeTutorialContent();
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
            _currentLevelIndex = 0;
            UpdateLevelDisplay();
        }
        else
        {
            // No leaderboards configured
            Debug.LogWarning("No leaderboards configured! Please set Leaderboard Configs in LeaderboardManager Inspector.");
            _currentSelectedLevel = "";
            if (_currentLevelLabel != null) _currentLevelLabel.text = "No Levels";
        }
        
        Debug.Log("=== SetupLevelDropdown finished ===");
    }

    private void ChangeLevel(int direction)
    {
        if (_availableLevels.Count == 0) return;

        _currentLevelIndex += direction;
        
        if (_currentLevelIndex < 0) 
            _currentLevelIndex = _availableLevels.Count - 1;
        else if (_currentLevelIndex >= _availableLevels.Count) 
            _currentLevelIndex = 0;

        UpdateLevelDisplay();
        
        RefreshLeaderboard();
    }

    private void UpdateLevelDisplay()
    {
        if (_availableLevels.Count > 0 && _currentLevelLabel != null)
        {
            _currentSelectedLevel = _availableLevels[_currentLevelIndex];
            _currentLevelLabel.text = _currentSelectedLevel;
        }
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

    private void ShowLevelSelection()
    {
        if (_levelSelectionPanel == null) return;
        _levelSelectionPanel.style.display = DisplayStyle.Flex;
    }

    private void HideLevelSelection()
    {
        if (_levelSelectionPanel == null) return;
        _levelSelectionPanel.style.display = DisplayStyle.None;
    }

    private void StartLevel(string levelName)
    {
        Debug.Log($"Starting level: {levelName}");
        
        // Use GameManager if available
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadLevel(levelName);
        }
        else
        {
            // Fallback to direct scene loading
            SceneManager.LoadScene(levelName);
        }
    }

    private void ShowHelp()
    {
        if (_helpPanel == null) return;
        _helpPanel.style.display = DisplayStyle.Flex;
    }

    private void HideHelp()
    {
        if (_helpPanel == null) return;
        _helpPanel.style.display = DisplayStyle.None;
    }

    private void InitializeTutorialContent()
    {
        if (_helpScrollView == null) return;

        // Clear existing content
        _helpScrollView.Clear();

        // Tutorial data - set imagePath to null or empty string to hide image for that section
        var tutorials = new[]
        {
            // Game Controls
            new
            {
                imagePath = "",
                hasImage = false,
                text = "Game Controls:\n\nUse W, S / Left Shift to move forward and backward (brake)\n\nUse A, D to turn left and right\n\nPress Space to ring the bell and scare away animals in your path\n\nUse mouse to interact with objects (click, drag)"
            },
            // How to Play
            new
            {
                imagePath = "game intro",
                hasImage = true,
                text = "How to Play:\n\nTimer at top left shows remaining time. Reach the destination before time runs out! Minimap guides your direction\n\nMood meter at top left affects your movement speed. Keep high mood to move faster\n\nColliding with walls or obstacles reduces mood. Some objects increase or decrease mood"
            },
            // Objects: Coin
            new
            {
                imagePath = "coin2_20x20_0",
                hasImage = true,
                text = "Objects - Coin:\n\nCollecting coins makes you happy and increases mood"
            },
            // Objects: Obstacles
            new
            {
                imagePath = "ME_Singles_City_Props_48x48_Cone_6",
                hasImage = true,
                text = "Objects - Obstacles:\n\nCones, trash cans and other obstacles. Avoid collisions or your mood will drop"
            },
            // Objects: Pedestrians
            new
            {
                imagePath = "Modern_Exteriors_Characters_Postman_48x48_1_3",
                hasImage = true,
                text = "Objects - Pedestrians:\n\nWalk around randomly. Be careful not to hit them or they get angry and your mood drops"
            },
            // Objects: Birds
            new
            {
                imagePath = "Crow_idle_Left_48x48_4",
                hasImage = true,
                text = "Objects - Birds:\n\nGet close and press Space to ring bell and scare them away. Watch out for animals, they are obstacles too"
            },
            // Objects: Bird Poop
            new
            {
                imagePath = "bird poop_1",
                hasImage = true,
                text = "Objects - Bird Poop:\n\nWatch for shadows! Bird poop falls from above. Stepping on it slows you down and ruins your mood"
            },
            // Objects: Speed Camera
            new
            {
                imagePath = "ME_Singles_City_Props_48x48_Traffic_Sign_Modular_9",
                hasImage = true,
                text = "Objects - Speed Camera:\n\nSpeed over 20 is speeding. Tickets ruin your mood. Find spray paint nearby and drag it to cover the camera screen so you don't get a ticket (?" 
            },
            // Objects: Fire Hydrant
            new
            {
                imagePath = "ME_Singles_Fire_Station_48x48_Fire_Hydrant",
                hasImage = true,
                text = "Objects - Fire Hydrant:\n\nClick to turn it off. Getting wet makes you unhappy"
            },
            // Objects: Car
            new
            {
                imagePath = "Car_2_complete_48x48_1_0",
                hasImage = true,
                text = "Objects - Car:\n\nBe careful not to hit cars on the road. Collisions reduce your mood"
            },
            // Objects: Flag
            new
            {
                imagePath = "ME_Singles_School_48x48_Flag_2",
                hasImage = true,
                text = "Objects - Flag:\n\nThe destination! Touch it to successfully reach class on time"
            }
        };

        // Create tutorial items
        foreach (var tutorial in tutorials)
        {
            var itemContainer = new VisualElement();
            itemContainer.AddToClassList("tutorial-item");

            // Create image only if hasImage is true
            if (tutorial.hasImage && !string.IsNullOrEmpty(tutorial.imagePath))
            {
                string resourcePath = ExtractResourcePath(tutorial.imagePath);
                
                // Try to load as Sprite first (for .asset files from sprite sheets)
                var sprite = Resources.Load<Sprite>(resourcePath);
                Texture2D texture = null;
                float imageWidth = 0;
                float imageHeight = 0;
                
                if (sprite != null)
                {
                    texture = sprite.texture;
                    // Use sprite's actual rect size, not the entire texture
                    imageWidth = sprite.rect.width;
                    imageHeight = sprite.rect.height;
                }
                else
                {
                    // If not a sprite, try loading as Texture2D
                    texture = Resources.Load<Texture2D>(resourcePath);
                    if (texture != null)
                    {
                        imageWidth = texture.width;
                        imageHeight = texture.height;
                    }
                }
                
                if (texture != null)
                {
                    var image = new VisualElement();
                    image.AddToClassList("tutorial-image");
                    
                    // Use sprite if available, otherwise use texture
                    if (sprite != null)
                    {
                        image.style.backgroundImage = new StyleBackground(sprite);
                    }
                    else
                    {
                        image.style.backgroundImage = new StyleBackground(texture);
                    }
                    
                    // Set image size based on actual image dimensions
                    // Limit max width and height to maintain aspect ratio
                    float maxWidth = 400f; // Maximum width in pixels
                    float maxHeight = 200f; // Maximum height in pixels
                    float aspectRatio = imageHeight / imageWidth;
                    
                    float displayWidth = Mathf.Min(imageWidth, maxWidth);
                    float displayHeight = displayWidth * aspectRatio;
                    
                    // If height exceeds max, recalculate based on height constraint
                    if (displayHeight > maxHeight)
                    {
                        displayHeight = maxHeight;
                        displayWidth = displayHeight / aspectRatio;
                    }
                    
                    image.style.width = displayWidth;
                    image.style.height = displayHeight;
                    image.style.marginBottom = 15;
                    image.style.backgroundColor = new Color(1, 1, 1, 0.1f);
                    image.style.borderTopLeftRadius = 5;
                    image.style.borderTopRightRadius = 5;
                    image.style.borderBottomLeftRadius = 5;
                    image.style.borderBottomRightRadius = 5;
                    
                    itemContainer.Add(image);
                }
                else
                {
                    Debug.LogWarning($"Failed to load image (tried both Sprite and Texture2D): {tutorial.imagePath}");
                }
            }

            // Create text label
            var textLabel = new Label(tutorial.text);
            textLabel.AddToClassList("tutorial-text");
            itemContainer.Add(textLabel);

            _helpScrollView.Add(itemContainer);
        }
    }

    private string ExtractResourcePath(string projectPath)
    {
        // If path already looks like a resource path (no protocol or Assets), return as-is without extension
        if (!projectPath.Contains("://") && !projectPath.Contains("Assets/"))
        {
            // Remove extension if present
            int dotIndex = projectPath.LastIndexOf('.');
            if (dotIndex > 0)
            {
                return projectPath.Substring(0, dotIndex);
            }
            return projectPath;
        }
        
        // Extract resource path from Unity project path
        if (projectPath.Contains("Resources/"))
        {
            int startIndex = projectPath.IndexOf("Resources/") + "Resources/".Length;
            int endIndex = projectPath.LastIndexOf('.');
            if (endIndex > startIndex)
            {
                return projectPath.Substring(startIndex, endIndex - startIndex);
            }
        }
        return "";
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
        LeaderboardManager.Instance.GetTopPlayers(100, OnLeaderboardLoaded, _currentSelectedLevel);

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
        container.AddToClassList("leaderboard-row");
        /*container.style.flexDirection = FlexDirection.Row;
        container.style.paddingTop = 8;
        container.style.paddingBottom = 8;
        container.style.paddingLeft = 10;
        container.style.paddingRight = 10;
        container.style.marginBottom = 2;*/

        // Set background color based on rank
        if (member.rank == 1) container.AddToClassList("rank-gold");
        else if (member.rank == 2) container.AddToClassList("rank-silver");
        else if (member.rank == 3) container.AddToClassList("rank-bronze");
        else container.style.backgroundColor = new Color(0, 0, 0, 0.3f);

        //current player
        string playerIdentifier = LeaderboardManager.Instance?.GetPlayerIdentifier();
        if (!string.IsNullOrEmpty(playerIdentifier) && member.member_id == playerIdentifier)
        {
        container.AddToClassList("rank-current-player");
        }

        // Rank label
        var rankLabel = new Label($"#{member.rank}");
        // style (rank is always numbers, safe to use Roboto)
        rankLabel.AddToClassList("leaderboard-text"); 
        rankLabel.style.width = 80;  
        rankLabel.style.flexGrow = 0; 
        
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
        
        // Name label
        var nameLabel = new Label(displayName);
        //style - now using Noto Sans SC which supports both English and Chinese
        nameLabel.AddToClassList("leaderboard-text");
        nameLabel.style.width = StyleKeyword.Auto;
        nameLabel.style.flexGrow = 1; 
        nameLabel.style.marginLeft = 12;
        
        container.Add(nameLabel);

        //time
        float timeInSeconds = member.score / 1000f;
        string timeString = FormatTime(timeInSeconds);
        
        // remaining time label
        var timeLabel = new Label(timeString);
        //style
        timeLabel.AddToClassList("leaderboard-text");
        timeLabel.style.width = 160;
        timeLabel.style.marginRight = 45;
        timeLabel.style.unityTextAlign = TextAnchor.MiddleRight;
        
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
    
    // Helper method to detect Chinese characters
    private bool ContainsChinese(string text)
    {
        if (string.IsNullOrEmpty(text)) return false;
        
        foreach (char c in text)
        {
            // Check if character is in CJK (Chinese, Japanese, Korean) Unicode ranges
            if ((c >= 0x4E00 && c <= 0x9FFF) ||   // CJK Unified Ideographs
                (c >= 0x3400 && c <= 0x4DBF) ||   // CJK Unified Ideographs Extension A
                (c >= 0x20000 && c <= 0x2A6DF) || // CJK Unified Ideographs Extension B
                (c >= 0xF900 && c <= 0xFAFF) ||   // CJK Compatibility Ideographs
                (c >= 0x2F800 && c <= 0x2FA1F))   // CJK Compatibility Ideographs Supplement
            {
                return true;
            }
        }
        return false;
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

    // Settings panel methods
    private void ShowSettings()
    {
        if (_settingsPanel == null) return;
        _settingsPanel.style.display = DisplayStyle.Flex;
    }

    private void HideSettings()
    {
        if (_settingsPanel == null) return;
        _settingsPanel.style.display = DisplayStyle.None;
    }

    private void OnBGMVolumeChanged(ChangeEvent<float> evt)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBGMVolume(evt.newValue);
        }
        UpdateBGMVolumeLabel(evt.newValue);
    }

    private void OnSFXVolumeChanged(ChangeEvent<float> evt)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(evt.newValue);
        }
        UpdateSFXVolumeLabel(evt.newValue);
    }

    private void UpdateBGMVolumeLabel(float volume)
    {
        if (_bgmVolumeLabel != null)
        {
            _bgmVolumeLabel.text = $"{Mathf.RoundToInt(volume * 100)}%";
        }
    }

    private void UpdateSFXVolumeLabel(float volume)
    {
        if (_sfxVolumeLabel != null)
        {
            _sfxVolumeLabel.text = $"{Mathf.RoundToInt(volume * 100)}%";
        }
    }
}
