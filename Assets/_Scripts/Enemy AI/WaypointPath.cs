using UnityEngine;

/// <summary>
/// Waypoint Path for Enemy Patrol
/// Holds an array of Transform waypoints
/// </summary>
public class WaypointPath : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform[] waypoints;

    [Header("Gizmos")]
    public bool showGizmos = true;
    public Color pathColor = Color.cyan;
    public float waypointRadius = 0.5f;

    private void OnValidate()
    {
        // Auto-populate waypoints from children if empty
        if (waypoints == null || waypoints.Length == 0)
        {
            waypoints = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                waypoints[i] = transform.GetChild(i);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos || waypoints == null || waypoints.Length == 0)
            return;

        Gizmos.color = pathColor;

        // Draw waypoints
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;

            // Draw sphere at waypoint
            Gizmos.DrawWireSphere(waypoints[i].position, waypointRadius);

            // Draw line to next waypoint
            int nextIndex = (i + 1) % waypoints.Length;
            if (waypoints[nextIndex] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[nextIndex].position);
            }

            // Draw waypoint number
#if UNITY_EDITOR
            UnityEditor.Handles.Label(
                waypoints[i].position + Vector3.up * 1.5f,
                $"WP {i}",
                new GUIStyle()
                {
                    normal = new GUIStyleState() { textColor = pathColor },
                    fontSize = 12,
                    fontStyle = FontStyle.Bold
                }
            );
#endif
        }
    }

    /// <summary>
    /// Get waypoint at index (with bounds checking)
    /// </summary>
    public Transform GetWaypoint(int index)
    {
        if (waypoints == null || waypoints.Length == 0)
            return null;

        index = Mathf.Clamp(index, 0, waypoints.Length - 1);
        return waypoints[index];
    }

    /// <summary>
    /// Get total number of waypoints
    /// </summary>
    public int WaypointCount => waypoints != null ? waypoints.Length : 0;
}