using UnityEngine;

public class Stone : MonoBehaviour
{
    [Header("Fall Setting")]
    public float fallRadius = 5f;
    public float fallSpeed = 5f;
    public float startHeight = 10f;
    public Sprite[] stoneSprite;

    [Header("Shadow Setting")]
    public GameObject shadowPrefab;
    public float minShadowScale = 0.2f;
    public float maxShadowScale = 1f;  

    private GameObject shadowInstance;
    private bool isFall = false;
    private bool fallen = false;
    private Vector3 fallDirection = Vector3.down;
    private Vector3 originalPosition;
    private Vector3 startPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalPosition = transform.position;
        startPosition = originalPosition + Vector3.up * startHeight;

        transform.position = startPosition;

        GetComponent<Collider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<SpriteRenderer>().sprite = stoneSprite[0];

        if (shadowPrefab != null)
        {
            shadowInstance = Instantiate(shadowPrefab, originalPosition, Quaternion.identity);
            shadowInstance.transform.localScale = Vector3.one * minShadowScale;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(isFall && !fallen)
        {
            transform.position += (Vector3)(fallDirection * fallSpeed * Time.deltaTime);

            // 👇 影子由小變大
            if (shadowInstance != null)
            {
                float t = Mathf.InverseLerp(0f, startHeight, startPosition.y - transform.position.y);
                float scale = Mathf.Lerp(minShadowScale, maxShadowScale, t);
                shadowInstance.transform.localScale = Vector3.one * scale;
            }

            if (transform.position.y <= originalPosition.y)
            {
                transform.position = originalPosition; // 對齊到地面
                isFall = false;
                fallen = true;

                // GetComponent<Rigidbody2D>().simulated = false;
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                sr.sprite = stoneSprite[1];

                GetComponent<Collider2D>().enabled = true;
            }
        }
        
    }

    public void TryFall(Vector3 playerPosition)
    {
        if(fallen || isFall) return;

        float distance = Vector3.Distance(playerPosition, originalPosition);
        if(distance <= fallRadius)
        {
            isFall = true;
            GetComponent<SpriteRenderer>().enabled = true;
            // GetComponent<Rigidbody2D>().simulated = true;
        }
    }
}
