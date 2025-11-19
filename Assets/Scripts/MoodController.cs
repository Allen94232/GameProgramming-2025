using TMPro;
using UnityEngine;

public class MoodController : MonoBehaviour
{
    public static MoodController Instance { get; private set; }

    private float moodValue = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("MoodController: Singleton created and persisting across scenes");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Remove Start - initialization now handled by GameManager's OnSceneLoaded
    
    public void Init()
    {
        moodValue = GameManager.Instance.initialMood;
        GameUIController.Instance.UpdateMoodDisplay(moodValue);
    }

    public float GetMoodValue()
    {
        return moodValue;
    }

    public void SetMoodValue(float moodValue)
    {
        this.moodValue = moodValue;
        GameUIController.Instance.UpdateMoodDisplay(moodValue);
    }
}
