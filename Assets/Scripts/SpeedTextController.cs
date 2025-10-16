using TMPro;
using UnityEngine;

public class SpeedTextController : MonoBehaviour
{
    [SerializeField] private Color normalSpeedColor = Color.white;
    [SerializeField] private Color overSpeedColor = Color.red;
    [SerializeField] private int overSpeedValue = 20;

    [SerializeField] private int currSpeed;


    private TextMeshProUGUI speedText;

    private void Start()
    {
        currSpeed = 0;

        speedText = GetComponent<TextMeshProUGUI>();
        speedText.color = normalSpeedColor;
        speedText.text = currSpeed.ToString();
    }

    public void SpeedChanged(float speed)
    {
        currSpeed = Mathf.FloorToInt(speed);

        if (currSpeed > overSpeedValue)
        {
            speedText.color = overSpeedColor;

            float newMood = MoodController.Instance.GetMoodValue() - 20f;
            newMood = Mathf.Max(newMood, 0);
            MoodController.Instance.SetMoodValue(newMood);
        }
        else
        {
            speedText.color = normalSpeedColor;
        }

        speedText.text = currSpeed.ToString();
    }
}
