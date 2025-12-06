using UnityEngine;

/// <summary>
/// Waypoint for vehicle path - defines a point on the road
/// </summary>
public class RoadWaypoint : MonoBehaviour
{
    [Header("Next Waypoint")]
    [Tooltip("Next waypoint in the path (leave empty for end point)")]
    public RoadWaypoint nextWaypoint;
    
    [Header("Visualization")]
    [Tooltip("Show connections in Scene view")]
    public bool showGizmos = true;
    
    [Tooltip("Gizmo sphere radius")]
    public float gizmoRadius = 0.5f;

    // Get the position of this waypoint
    public Vector3 GetPosition()
    {
        return transform.position;
    }

    // Check if this is the last waypoint
    public bool IsEndPoint()
    {
        return nextWaypoint == null;
    }

    // Visualize waypoints and connections in editor
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // Draw waypoint sphere
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, gizmoRadius);

        // Draw arrow to next waypoint
        if (nextWaypoint != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 direction = nextWaypoint.transform.position - transform.position;
            Gizmos.DrawLine(transform.position, nextWaypoint.transform.position);
            
            // Draw arrow head
            Vector3 arrowHead = nextWaypoint.transform.position - direction.normalized * 1f;
            Vector3 perpendicular = Vector3.Cross(direction, Vector3.forward).normalized * 0.5f;
            Gizmos.DrawLine(nextWaypoint.transform.position, arrowHead + perpendicular);
            Gizmos.DrawLine(nextWaypoint.transform.position, arrowHead - perpendicular);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        // Highlight selected waypoint
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, gizmoRadius * 0.5f);

        // Draw thick line to next waypoint
        if (nextWaypoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, nextWaypoint.transform.position);
        }
    }
}
