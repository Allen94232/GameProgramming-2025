using UnityEngine;

public class Coin : MonoBehaviour
{
    public int value = 1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            Debug.Log("Player collected a coin!");
            // add point
            float newMood = MoodController.Instance.GetMoodValue() + value;
            newMood = Mathf.Min(newMood, GameManager.Instance.maxMood);
            MoodController.Instance.SetMoodValue(newMood);

            Destroy(gameObject);
        }
    }
}
