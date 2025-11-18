using UnityEngine;

public class DirectionIndicator : MonoBehaviour
{
    [Header("Object Assignments")]
    [Tooltip("The destination object for the arrow to point at.")]
    [SerializeField] private Transform target;

    [Tooltip("The Camera used for your minimap.")]
    [SerializeField] private Camera minimapCamera;

    [Tooltip("The GameObject for the player. Its position is used for the rotation calculation.")]
    [SerializeField] private Transform playerTransform;

    [Tooltip("The child GameObject that has the arrow sprite. This is what we will show/hide.")]
    [SerializeField] private GameObject indicatorVisual; 

    void Update()
    {
        if (target == null || minimapCamera == null || indicatorVisual == null)
        {
            return; 
        }

        Vector3 viewportPoint = new Vector3(0.2f, 0.2f, 10f);
        Vector3 worldPoint = minimapCamera.ViewportToWorldPoint(viewportPoint);
        transform.position = worldPoint;

        Vector3 targetViewportPosition = minimapCamera.WorldToViewportPoint(target.position);

        bool isTargetVisible = targetViewportPosition.z > 0 &&
                               targetViewportPosition.x > 0 && targetViewportPosition.x < 1 &&
                               targetViewportPosition.y > 0 && targetViewportPosition.y < 1;

        indicatorVisual.SetActive(!isTargetVisible);


        if (indicatorVisual.activeSelf)
        {
            Vector3 direction = target.position - playerTransform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle - 90f);
            transform.rotation = rotation; 
        }
    }
}
