using UnityEngine;

/// <summary>
/// Enemy Vision System
/// Checks if the enemy can see the player
/// </summary>
public class VisionScanner : MonoBehaviour
{
    [Header("Vision Settings")]
    public float maxDistance = 30f;
    public float checkInterval = 0.2f;
    public LayerMask obstacleMask = ~0;

    [Header("Target")]
    public Transform target;

    // Public results (read by EnemyAIController)
    public bool hasTarget = false;
    public bool canSeePlayer = false;
    public float distanceToPlayer = 999f;
    public Vector3 targetPosition;
    public Vector3 aimPoint;

    private float timer = 0f;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        hasTarget = (target != null);
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= checkInterval)
        {
            timer = 0f;
            CheckVision();
        }
    }

    void CheckVision()
    {
        canSeePlayer = false;
        distanceToPlayer = 999f;

        if (target == null)
        {
            hasTarget = false;
            return;
        }

        hasTarget = true;
        targetPosition = target.position;
        aimPoint = GetAimPoint(target);

        distanceToPlayer = Vector3.Distance(transform.position, targetPosition);

        if (distanceToPlayer > maxDistance)
            return;

        // Raycast to check line of sight
        Vector3 eyePosition = transform.position + Vector3.up * 1.5f;
        Vector3 targetCenter = targetPosition + Vector3.up * 1.0f;
        Vector3 direction = targetCenter - eyePosition;

        RaycastHit hit;
        if (Physics.Raycast(eyePosition, direction.normalized, out hit, maxDistance, obstacleMask))
        {
            bool hitTarget = hit.collider.transform == target;
            bool hitTargetChild = hit.collider.transform.IsChildOf(target);

            if (hitTarget || hitTargetChild)
                canSeePlayer = true;
        }

        // Debug line
#if UNITY_EDITOR
        Color lineColor = canSeePlayer ? Color.green : Color.red;
        Debug.DrawLine(eyePosition, targetCenter, lineColor, checkInterval);
#endif
    }

    Vector3 GetAimPoint(Transform t)
    {
        Collider col = t.GetComponentInChildren<Collider>();

        if (col != null)
            return col.bounds.center;

        return t.position + Vector3.up * 1.2f;
    }
}