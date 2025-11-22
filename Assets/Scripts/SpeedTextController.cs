using TMPro;
using UnityEngine;

public class SpeedTextController : MonoBehaviour
{
    [SerializeField] private Color normalSpeedColor = Color.white;
    [SerializeField] private Color overSpeedColor = Color.red;
    [SerializeField] private int overSpeedValue = 20;

    [SerializeField] private int currSpeed;

    private TextMeshProUGUI speedText;
    private Material textMaterial;

    private void Start()
    {
        currSpeed = 0;

        speedText = GetComponent<TextMeshProUGUI>();
        
        // Create a proper material instance for this text component
        // Using fontMaterial (not fontSharedMaterial) automatically creates an instance
        textMaterial = speedText.fontMaterial;
        
        // Set initial face color using the correct property
        textMaterial.SetColor("_FaceColor", normalSpeedColor);
        speedText.text = currSpeed.ToString();
    }

    public void SpeedChanged(float speed, bool isCovered = false)
    {
        currSpeed = Mathf.FloorToInt(speed);

        if (currSpeed > overSpeedValue)
        {
            // Change face color of the text
            textMaterial.SetColor("_FaceColor", overSpeedColor);

            // Only apply mood damage if detector is not covered
            if (!isCovered)
            {
                float newMood = MoodController.Instance.GetMoodValue() - 20f;
                newMood = Mathf.Max(newMood, 0);
                MoodController.Instance.SetMoodValue(newMood);
            }
        }
        else
        {
            // Change face color of the text
            textMaterial.SetColor("_FaceColor", normalSpeedColor);
        }

        speedText.text = currSpeed.ToString();
    }

    private void OnDestroy()
    {
        // Clean up material instance to prevent memory leak
        if (textMaterial != null)
        {
            Destroy(textMaterial);
        }
    }
}
