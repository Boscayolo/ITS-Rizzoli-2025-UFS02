using UnityEngine;

public class EnemyShooter : Shooter
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private bool autoFindPlayer = true;
    [SerializeField] private string playerTag = "Player";

    [Header("Engage")]
    [SerializeField] private float engageDistance = 25f;
    [SerializeField] private bool requireLineOfSight = true;

    [Tooltip("Layer considerati come ostacoli/bersagli per la line of sight.")]
    [SerializeField] private LayerMask lineOfSightMask = Physics.DefaultRaycastLayers;

    [Header("Aiming (Aimer-like)")]
    [Tooltip("Che cosa ruota per mirare. Se null usa questo transform.")]
    [SerializeField] private Transform rotateRoot;

    [Tooltip("Origine del Raycast di visibilità. Se null usa rotateRoot (o transform).")]
    [SerializeField] private Transform sightOrigin;

    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private bool lockYRotation = true;

    [Tooltip("Se > 0, spara solo quando è allineato entro questo angolo (gradi).")]
    [SerializeField] private float shootAngleThreshold = 10f;

    private bool _seesTarget;

    public Transform Target
    {
        get => target;
        set => target = value;
    }

    public bool SeesTarget => _seesTarget;

    protected override void Awake()
    {
        base.Awake();

        if (rotateRoot == null) rotateRoot = transform;
        if (sightOrigin == null) sightOrigin = rotateRoot;

        AutoFindTargetIfMissing();
    }

    protected override void Update()
    {
        // Non chiamiamo base.Update() così ignoriamo autoFire dello Shooter base

        AutoFindTargetIfMissing();
        if (target == null) return;

        RotateTowardsTarget();

        if (!IsTargetInEngageRange())
            return;

        if (requireLineOfSight && !_seesTarget)
            return;

        if (shootAngleThreshold > 0f && !IsFacingTarget(shootAngleThreshold))
            return;

        // Rispetta il fireRate definito in Shooter (via TryShoot)
        TryShoot();
    }

    private void FixedUpdate()
    {
        _seesTarget = false;

        if (target == null)
            return;

        Vector3 origin = sightOrigin != null ? sightOrigin.position : transform.position;

        Vector3 toTarget = target.position - origin;
        if (lockYRotation) toTarget.y = 0f;

        float dist = toTarget.magnitude;
        if (dist < 0.001f || dist > engageDistance)
            return;

        Vector3 dir = toTarget / dist;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist, lineOfSightMask, QueryTriggerInteraction.Ignore))
        {
            Transform hitT = hit.collider.transform;
            _seesTarget = (hitT == target) || hitT.IsChildOf(target);
        }
    }

    private void AutoFindTargetIfMissing()
    {
        if (!autoFindPlayer || target != null) return;

        GameObject p = GameObject.FindGameObjectWithTag(playerTag);
        if (p != null) target = p.transform;
    }

    private bool IsTargetInEngageRange()
    {
        Vector3 delta = target.position - rotateRoot.position;
        if (lockYRotation) delta.y = 0f;
        return delta.sqrMagnitude <= engageDistance * engageDistance;
    }

    private bool IsFacingTarget(float maxAngleDeg)
    {
        Vector3 delta = target.position - rotateRoot.position;
        if (lockYRotation) delta.y = 0f;
        if (delta.sqrMagnitude < 0.0001f) return true;

        float angle = Vector3.Angle(rotateRoot.forward, delta.normalized);
        return angle <= maxAngleDeg;
    }

    private void RotateTowardsTarget()
    {
        Vector3 dir = target.position - rotateRoot.position;
        if (lockYRotation) dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f)
            return;

        Quaternion desired = Quaternion.LookRotation(dir.normalized);
        rotateRoot.rotation = Quaternion.Slerp(rotateRoot.rotation, desired, rotationSpeed * Time.deltaTime);
    }
}
