using TMPro;
using UnityEngine;

public class MoodController : MonoBehaviour
{
    public static MoodController Instance { get; private set; }

    private float moodValue = 0;

    private TextMeshProUGUI moodUI;

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
        moodValue = 100;
        moodUI = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        moodUI.text = "Mood: " + moodValue.ToString();
    }
    

    public float GetMoodValue()
    {
        return moodValue;
    }

    public void SetMoodValue(float moodValue)
    {
        this.moodValue = moodValue;
    }
}
