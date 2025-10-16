using UnityEngine;
using UnityEngine.Events;

public class DraggableTarget : MonoBehaviour
{
    private GameObject currentObj = null;

    // Add a UnityEvent that passes the dropped object
    [System.Serializable]
    public class DropEvent : UnityEvent<GameObject> { }

    [Header("Events")]
    public DropEvent onObjectDropped;


    // Called when a draggable object is dropped on this target
    public void OnObjectDropped(Draggable draggable)
    {
        currentObj = draggable.gameObject;

        // Invoke the event so you can assign behavior in Inspector
        onObjectDropped?.Invoke(currentObj);

        Destroy(draggable.gameObject);
    }
}
