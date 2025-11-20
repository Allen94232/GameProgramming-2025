using UnityEngine;

public class Pigeon : MonoBehaviour
{
    [Header("Pigeon Trigger")]
    public float scareRadius = 5f;  // how close the player must be
    public float flyAwaySpeed = 5f;
    [Header("Pigeon Animator")]
    public Animator animator;
    private bool isScared = false;
    private Vector2 flyDirection = Vector2.up;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<Collider2D>().enabled = true;
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
            FlyAway(playerPosition);
        }
    }

    void FlyAway(Vector3 playerPosition)
    {
        isScared = true;
        
        Vector2 awayFromPlayer = ((Vector2)transform.position - (Vector2)playerPosition).normalized;
        
        float randomAngle = Random.Range(-30f, 30f);
        flyDirection = RotateVector(awayFromPlayer, randomAngle);

        animator.SetFloat("FlyX", flyDirection.x);
        animator.SetFloat("FlyY", flyDirection.y);
        
        Debug.Log($"{gameObject.name} flies away in direction {flyDirection}!");
        Destroy(gameObject, 2f);
    }
    
    // ���U��k:����V�q
    Vector2 RotateVector(Vector2 vector, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        
        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos
        );
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            // Pigeon is crushed by player -> fly away immediately
            FlyAway(collision.transform.position);

            //dead animation
            //Debug.Log("You crush a bird");
            //float newMood = MoodController.Instance.GetMoodValue() - 5f;
            //newMood = Mathf.Max(newMood, 0);
            //MoodController.Instance.SetMoodValue(newMood);
        }
    }
}
