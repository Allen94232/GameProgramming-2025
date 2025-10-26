using TMPro;
using UnityEngine;

public class MoodController : MonoBehaviour
{
    public static MoodController Instance { get; private set; }

    private float moodValue = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        Init();
    }

    private void Update()
    {

    }
    
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
