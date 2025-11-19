using UnityEngine;

public class Aimer : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private float engageDistance = 100f;
    bool seesTarget;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private bool lockYRotation = true;

    public Transform Target
    {
        get => target;
        set => target = value;
    }

    private void Update()
    {
        RotateTowardsTarget();
    }

    private void FixedUpdate()
    {
        if (target != null)
        {
            RaycastHit playerHit;
            seesTarget = Physics.Raycast(transform.position, target.position, out playerHit, engageDistance) && playerHit.collider.transform == target;
        }
    }


    public Vector3 GetAimDirection()
    {
        if (target == null || !seesTarget)
        {
            return transform.forward;
        }

        Vector3 dir = (target.position - transform.position).normalized;

        if (lockYRotation)
        {
            dir.y = 0f;
            dir.Normalize();
        }

        return dir;
    }

    void RotateTowardsTarget()
    {
        if (target == null)
            return;

        Vector3 dir = target.position - transform.position;

        if (lockYRotation)
        {
            dir.y = 0f;
        }

        if (dir.sqrMagnitude < 0.0001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(dir.normalized);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}
