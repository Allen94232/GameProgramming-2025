using UnityEngine;

public class Pigeon : MonoBehaviour
{
    [Header("Pigeon Trigger")]
    public float scareRadius = 5f;  // how close the player must be
    public float flyAwaySpeed = 5f;
    private bool isScared = false;
    public Vector2 flyDirection = Vector2.up;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<Collider2D>().enabled = true;
        flyDirection = Random.insideUnitCircle.normalized;
    }

    // Update is called once per frame
    void Update()
    {
        if (isScared)
        {
            // Move away (simulate flying)
            transform.position += (Vector3)(flyDirection * flyAwaySpeed * Time.deltaTime);
        }
    }

    public void TryScare(Vector3 playerPosition)
    {
        if (isScared) return;

        float distance = Vector3.Distance(playerPosition, transform.position);
        if (distance <= scareRadius)
        {
            GetComponent<Collider2D>().enabled = false;
            FlyAway();
        }
    }

    void FlyAway()
    {
        isScared = true;
        Debug.Log($"{gameObject.name} flies away!");
        Destroy(gameObject, 2f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            //dead animation
            Debug.Log("You crush a bird");
            float newMood = MoodController.Instance.GetMoodValue() - 5f;
            newMood = Mathf.Max(newMood, 0);
            MoodController.Instance.SetMoodValue(newMood);
        }
    }
}
