using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [Tooltip("AudioSource for background music")]
    [SerializeField] private AudioSource bgmSource;
    
    [Tooltip("AudioSource for sound effects")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Background Music")]
    [Tooltip("BGM for Main Menu")]
    [SerializeField] private AudioClip mainMenuBGM;
    
    [Tooltip("BGM for Tutorial")]
    [SerializeField] private AudioClip tutorialBGM;
    
    [Tooltip("BGM for Level 1")]
    [SerializeField] private AudioClip level1BGM;
    
    [Tooltip("BGM for Level 2")]
    [SerializeField] private AudioClip level2BGM;

    [Header("Game Sound Effects")]
    [Tooltip("Sound effect when player wins")]
    [SerializeField] private AudioClip winSFX;
    
    [Tooltip("Sound effect when player loses")]
    [SerializeField] private AudioClip loseSFX;

    [Header("UI Sound Effects")]
    [Tooltip("Sound effect for button clicks")]
    [SerializeField] private AudioClip buttonClickSFX;
    
    [Tooltip("Sound effect for button hover")]
    [SerializeField] private AudioClip buttonHoverSFX;
    
    [Header("Gameplay Sound Effects")]
    [Tooltip("Sound effect when player collects a coin")]
    [SerializeField] private AudioClip coinCollectSFX;
    
    [Tooltip("Sound effect when toggling sprinkler on/off")]
    [SerializeField] private AudioClip sprinklerToggleSFX;
    
    [Tooltip("Sound effect when spray painting (draggable object dropped on target)")]
    [SerializeField] private AudioClip sprayPaintSFX;
    
    [Tooltip("Sound effect when player rings bell")]
    [SerializeField] private AudioClip bellSFX;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [Tooltip("Master volume for BGM")]
    [SerializeField] private float bgmVolume = 0.5f;
    
    [Range(0f, 1f)]
    [Tooltip("Master volume for SFX")]
    [SerializeField] private float sfxVolume = 0.7f;

    private string currentSceneName = "";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Create AudioSources if not assigned
            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
                bgmSource.loop = true;
                bgmSource.playOnAwake = false;
            }
            
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
                sfxSource.clip = null; // Ensure no clip is assigned initially
                // Optimize for low latency sound effects
                sfxSource.priority = 0; // Highest priority
                sfxSource.bypassEffects = true; // Bypass audio effects to reduce latency
                sfxSource.bypassListenerEffects = true;
                sfxSource.bypassReverbZones = true;
            }
            
            // Set initial volumes
            bgmSource.volume = bgmVolume;
            sfxSource.volume = sfxVolume;
            
            // Ensure no audio is playing initially
            if (bgmSource.isPlaying) bgmSource.Stop();
            if (sfxSource.isPlaying) sfxSource.Stop();
            
            // Optimize audio settings for low latency
            AudioConfiguration config = AudioSettings.GetConfiguration();
            
            // Subscribe to scene loaded event
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            Debug.Log("AudioManager: Singleton created and persisting across scenes");
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
        currentSceneName = scene.name;
        Debug.Log($"AudioManager: Scene loaded - {scene.name}");
        
        // Play appropriate BGM for the scene
        PlaySceneBGM(scene.name);
    }

    // Play BGM based on scene name
    private void PlaySceneBGM(string sceneName)
    {
        AudioClip clipToPlay = null;
        bool isGameLevel = false;

        switch (sceneName)
        {
            case "MainMenuScene":
                clipToPlay = mainMenuBGM;
                break;
            case "Tutorial":
                clipToPlay = tutorialBGM;
                isGameLevel = true;
                break;
            case "Level 1":
                clipToPlay = level1BGM;
                isGameLevel = true;
                break;
            case "Level 2":
                clipToPlay = level2BGM;
                isGameLevel = true;
                break;
            default:
                // For any other level, try to use Level 1 BGM as fallback
                clipToPlay = level1BGM;
                isGameLevel = true;
                break;
        }

        if (clipToPlay != null)
        {
            // Force restart for game levels to ensure music starts from beginning
            PlayBGM(clipToPlay, isGameLevel);
        }
        else
        {
            Debug.LogWarning($"AudioManager: No BGM assigned for scene '{sceneName}'");
        }
    }

    // Play background music
    public void PlayBGM(AudioClip clip, bool forceRestart = false)
    {
        if (clip == null)
        {
            Debug.LogWarning("AudioManager: Trying to play null BGM clip");
            return;
        }

        // If same clip is already playing and not forcing restart, don't restart
        if (bgmSource.clip == clip && bgmSource.isPlaying && !forceRestart)
        {
            return;
        }

        bgmSource.clip = clip;
        bgmSource.Play();
        Debug.Log($"AudioManager: Playing BGM - {clip.name}");
    }

    // Stop background music
    public void StopBGM()
    {
        bgmSource.Stop();
        Debug.Log("AudioManager: BGM stopped");
    }

    // Pause background music
    public void PauseBGM()
    {
        bgmSource.Pause();
        Debug.Log("AudioManager: BGM paused");
    }

    // Resume background music
    public void ResumeBGM()
    {
        bgmSource.UnPause();
        Debug.Log("AudioManager: BGM resumed");
    }

    // Play sound effect
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("AudioManager: Trying to play null SFX clip");
            return;
        }

        // Use PlayOneShot to allow multiple sound effects to play simultaneously
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    // Specific SFX methods for easy calling
    public void PlayWinSFX()
    {
        if (winSFX != null)
        {
            // Stop BGM before playing win sound
            StopBGM();
            PlaySFX(winSFX);
            Debug.Log("AudioManager: Playing win SFX (BGM stopped)");
        }
        else
        {
            Debug.LogWarning("AudioManager: Win SFX not assigned!");
        }
    }

    public void PlayLoseSFX()
    {
        if (loseSFX != null)
        {
            // Stop BGM before playing lose sound
            StopBGM();
            PlaySFX(loseSFX);
            Debug.Log("AudioManager: Playing lose SFX (BGM stopped)");
        }
        else
        {
            Debug.LogWarning("AudioManager: Lose SFX not assigned!");
        }
    }

    public void PlayButtonClickSFX()
    {
        if (buttonClickSFX != null)
        {
            // Use PlayOneShot for button clicks to avoid conflicts with other sounds
            sfxSource.PlayOneShot(buttonClickSFX, sfxVolume);
        }
        else
        {
            Debug.LogWarning("AudioManager: Button click SFX not assigned!");
        }
    }

    public void PlayButtonHoverSFX()
    {
        if (buttonHoverSFX != null)
        {
            // Also use PlayOneShot for hover sounds
            sfxSource.PlayOneShot(buttonHoverSFX, sfxVolume * 0.7f); // Lower volume for hover
        }
        // No warning for hover - it's optional
    }
    
    public void PlayCoinCollectSFX()
    {
        if (coinCollectSFX != null)
        {
            sfxSource.PlayOneShot(coinCollectSFX, sfxVolume);
        }
        else
        {
            Debug.LogWarning("AudioManager: Coin collect SFX not assigned!");
        }
    }
    
    public void PlaySprinklerToggleSFX()
    {
        if (sprinklerToggleSFX != null)
        {
            sfxSource.PlayOneShot(sprinklerToggleSFX, sfxVolume);
        }
        else
        {
            Debug.LogWarning("AudioManager: Sprinkler toggle SFX not assigned!");
        }
    }
    
    public void PlaySprayPaintSFX()
    {
        if (sprayPaintSFX != null)
        {
            sfxSource.PlayOneShot(sprayPaintSFX, sfxVolume);
        }
        else
        {
            Debug.LogWarning("AudioManager: Spray paint SFX not assigned!");
        }
    }
    
    public void PlayBellSFX()
    {
        if (bellSFX != null)
        {
            sfxSource.PlayOneShot(bellSFX, sfxVolume);
        }
        else
        {
            Debug.LogWarning("AudioManager: Bell SFX not assigned!");
        }
    }

    // Volume control methods
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        bgmSource.volume = bgmVolume;
        Debug.Log($"AudioManager: BGM volume set to {bgmVolume}");
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
        Debug.Log($"AudioManager: SFX volume set to {sfxVolume}");
    }

    public float GetBGMVolume()
    {
        return bgmVolume;
    }

    public float GetSFXVolume()
    {
        return sfxVolume;
    }

    // Toggle methods
    public void ToggleBGM()
    {
        if (bgmSource.isPlaying)
        {
            PauseBGM();
        }
        else
        {
            ResumeBGM();
        }
    }

    public void MuteBGM(bool mute)
    {
        bgmSource.mute = mute;
        Debug.Log($"AudioManager: BGM muted = {mute}");
    }

    public void MuteSFX(bool mute)
    {
        sfxSource.mute = mute;
        Debug.Log($"AudioManager: SFX muted = {mute}");
    }
}
