using Unity.Collections;
using UnityEngine;

/// <summary>
/// Enemy Vision System
/// Finds and tracks the player automatically using OverlapSphere
/// </summary>
public class VisionScanner : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectionRadius = 30f;
    public float checkInterval = 0.2f;
    public LayerMask obstacleMask = ~0;

    // Public results (read by EnemyAIController)
    [ReadOnly]public bool hasTarget = false;
    public bool canSeePlayer = false;
    public float distanceToPlayer = 999f;
    public Vector3 targetPosition;
    public Vector3 aimPoint;

    // Found target
    private Transform target;
    private Collider targetCollider;
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= checkInterval)
        {
            timer = 0f;
            ScanForPlayer();
        }
    }

    void ScanForPlayer()
    {
        // Reset values
        hasTarget = false;
        canSeePlayer = false;
        distanceToPlayer = 999f;
        target = null;
        targetCollider = null;

        // Find all colliders in detection radius
        Collider[] colliders = Physics.OverlapSphere(
            transform.position,
            detectionRadius,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        // Look for a player
        for (int i = 0; i < colliders.Length; i++)
        {
            Health health = colliders[i].GetComponentInParent<Health>();

            if (health != null && health.Team == Team.Player)
            {
                // Found the player
                target = health.transform;
                targetCollider = colliders[i];
                hasTarget = true;
                break;
            }
        }

        // If no player found, exit
        if (!hasTarget)
            return;

        // Update positions
        targetPosition = target.position;
        aimPoint = GetAimPoint();
        distanceToPlayer = Vector3.Distance(transform.position, targetPosition);

        // Check line of sight with raycast
        CheckLineOfSight();
    }

    void CheckLineOfSight()
    {
        Vector3 eyePosition = transform.position + Vector3.up * 1.5f;
        Vector3 targetCenter = aimPoint;
        Vector3 direction = targetCenter - eyePosition;
        float distance = direction.magnitude;

        RaycastHit hit;
        if (Physics.Raycast(eyePosition, direction.normalized, out hit, distance, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            // Check if we hit the player
            Health health = hit.collider.GetComponentInParent<Health>();

            if (health != null && health.Team == Team.Player)
                canSeePlayer = true;
        }

        // Debug line
#if UNITY_EDITOR
        Color lineColor = canSeePlayer ? Color.green : Color.red;
        Debug.DrawLine(eyePosition, targetCenter, lineColor, checkInterval);
#endif
    }

    Vector3 GetAimPoint()
    {
        if (targetCollider != null)
            return targetCollider.bounds.center;

        if (target != null)
        {
            Collider col = target.GetComponentInChildren<Collider>();
            if (col != null)
                return col.bounds.center;

            return target.position + Vector3.up * 1.2f;
        }

        return transform.position;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}