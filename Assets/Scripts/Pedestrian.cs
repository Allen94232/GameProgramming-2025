using UnityEngine;

public class Pedestrian : MonoBehaviour
{
    [Header("Walking Setting")]
    public Animator animator;
    public float walkingSpeed = 2f;
    public float areaSize = 10f;         // 可以亂走的範圍半徑
    public float stopDuration = 2f;      // 停下來的時間
    private bool isWalking = false;
    private Vector3 targetPosition;
    private float stopTimer = 0f;

    void Start()
    {
        // animator = GetComponent<Animator>();
        // animator.SetBool("isWalking", false);
        PickNewTarget();
    }

    void Update()
    {
        if (!isWalking)
        {
            stopTimer += Time.deltaTime;
            if (stopTimer >= stopDuration)
            {
                PickNewTarget();
            }
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, walkingSpeed * Time.deltaTime);

        // 看向走的方向
        Vector3 dir = targetPosition - transform.position;
        if (dir != Vector3.zero)
        {
            transform.forward = Vector3.Lerp(transform.forward, dir.normalized, Time.deltaTime * 5f);
        }

        // 抵達目標
        if (Vector3.Distance(transform.position, targetPosition) < 0.3f)
        {
            isWalking = false;
            // animator.SetBool("isWalking", false);
            stopTimer = 0f;
        }
    }

    void OnBecameVisible()
    {
        // 被看到時開始活動
        isWalking = true;
        // animator.SetBool("isWalking", true);
    }

    void OnBecameInvisible()
    {
        // 可選：離開視野就停下
        isWalking = false;
        // animator.SetBool("isWalking", false);
    }

    void PickNewTarget()
    {
        // 在原點附近隨機選新座標
        Vector2 randomPos = Random.insideUnitCircle * areaSize;
        targetPosition = new Vector3(randomPos.x, transform.position.y, randomPos.y);
        isWalking = true;
        // animator.SetBool("isWalking", true);
    }
}
