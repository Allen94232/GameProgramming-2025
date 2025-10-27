using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class Draggable : MonoBehaviour
{
    public LayerMask targetLayer;

    private Vector3 offset;
    private Camera cam;
    private Collider2D col;
    //private bool isDragging = false;

    void Start()
    {
        cam = Camera.main;
        col = GetComponent<Collider2D>();

        // Force physics refresh with delay to ensure it's draggable
        StartCoroutine(RefreshColliderWithDelay());
    }

    // Refresh collider after waiting for physics to settle
    private IEnumerator RefreshColliderWithDelay()
    {
        if (col != null)
        {
            col.enabled = false;
            
            // Wait for physics update
            yield return new WaitForFixedUpdate();
            
            // Wait for end of frame
            yield return new WaitForEndOfFrame();
            
            // Wait one more frame
            yield return null;
            
            col.enabled = true;
            
            // Force physics sync
            Physics2D.SyncTransforms();
            
            Debug.Log($"Draggable {gameObject.name}: Collider refreshed with full delay");
        }
    }

    void OnMouseDown()
    {
        // Calculate offset to avoid jump on click
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - new Vector3(mousePos.x, mousePos.y, transform.position.z);
        //isDragging = true;
    }

    void OnMouseDrag()
    {
        // Follow mouse position
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mousePos.x, mousePos.y, transform.position.z) + offset;
    }

    void OnMouseUp()
    {
        //isDragging = false;

        // Raycast or overlap to detect if dropped on a target
        Vector2 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos, targetLayer);

        if (hit != null)
        {
            DraggableTarget target = hit.GetComponent<DraggableTarget>();
            if (target != null)
            {
                // Call custom event or behavior
                OnDroppedOnTarget(target);
                return;
            }
        }

        Debug.Log("Dropped but not on any target.");
    }

    private void OnDroppedOnTarget(DraggableTarget target)
    {
        // Example: assign this object to the target
        target.SendMessage("OnObjectDropped", this, SendMessageOptions.DontRequireReceiver);
    }
}
