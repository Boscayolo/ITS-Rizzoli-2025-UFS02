using UnityEngine;

public class EnemyShooter : Shooter
{
    [Header("Detection")]
    [SerializeField] private float detectionRadius = 50f;
    [SerializeField] private float scanFrequency = 0.25f;
    [SerializeField] private float spotReactionTime = 0.15f;

    [Header("Engage")]
    [SerializeField] private float engageDistance = 50f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Weapon Pitch")]
    [SerializeField] private Transform weaponHolderTransform;
    [SerializeField] private float weaponPitchSpeed = 10f;
    [SerializeField] private float minWeaponPitch = -45f;
    [SerializeField] private float maxWeaponPitch = 60f;

    [Header("Prediction")]
    [Range(0f, 1f)]
    [SerializeField] private float velocityLerp = 0.4f;
    [SerializeField] private float maxLeadTime = 1.25f;
    [SerializeField] private float aimHeightFallback = 1.2f;

    private Transform playerTr;
    private Collider playerCollider;
    private Health myHealth;
    private Transform weaponHolder;

    private bool playerSpotted;
    private float spotTimer;
    private float scanTimer;

    private bool hasPlayerPos;
    private Vector3 lastPlayerPos;
    private Vector3 estimatedPlayerVel;

    private Vector3 cachedAimPoint;

    protected override void Awake()
    {
        base.Awake();
        myHealth = GetComponent<Health>();
        weaponHolder = weaponHolderTransform;

        scanTimer = 0f; // scan immediato
    }

    private void Update()
    {
        ScanForPlayer();

        if (!playerSpotted || playerTr == null || currentWeapon == null)
            return;

        UpdatePlayerVelocityEstimate();

        if (spotTimer < spotReactionTime)
        {
            spotTimer += Time.deltaTime;
            return;
        }

        cachedAimPoint = GetPredictedAimPoint();

        if (!CanEngage(cachedAimPoint))
            return;

        RotateTowardsAimPoint(cachedAimPoint);

        if (bulletsLeft > 0)
        {
            // line of sight prima di sparare
            if (CheckLineOfSight(cachedAimPoint))
                TryShoot();
        }
        else if (!reloading)
        {
            Reload();
        }
    }

    private void ScanForPlayer()
    {
        scanTimer -= Time.deltaTime;
        if (scanTimer > 0f) return;
        scanTimer = Mathf.Max(0.01f, scanFrequency);

        bool previouslySpotted = playerSpotted;
        playerSpotted = false;
        playerTr = null;
        playerCollider = null;

        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, ~0, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < colliders.Length; i++)
        {
            Health h = colliders[i].GetComponentInParent<Health>();
            if (h != null && h.Team == Team.Player)
            {
                playerSpotted = true;
                playerTr = h.transform;
                playerCollider = colliders[i]; // spesso è un child collider, va benissimo per bounds.center

                if (!previouslySpotted)
                {
                    spotTimer = 0f;
                    ResetVelocityEstimate();
                }
                return;
            }
        }

        // Se lo perdi
        if (previouslySpotted)
        {
            spotTimer = 0f;
            ResetVelocityEstimate();
        }
    }

    private void ResetVelocityEstimate()
    {
        hasPlayerPos = false;
        estimatedPlayerVel = Vector3.zero;
        lastPlayerPos = Vector3.zero;
    }

    private void UpdatePlayerVelocityEstimate()
    {
        if (playerTr == null) return;

        Vector3 pos = playerTr.position;
        float dt = Mathf.Max(Time.deltaTime, 0.0001f);

        if (!hasPlayerPos)
        {
            hasPlayerPos = true;
            lastPlayerPos = pos;
            estimatedPlayerVel = Vector3.zero;
            return;
        }

        Vector3 rawVel = (pos - lastPlayerPos) / dt;
        estimatedPlayerVel = Vector3.Lerp(estimatedPlayerVel, rawVel, velocityLerp);
        lastPlayerPos = pos;
    }

    private Vector3 GetPlayerAimPointRaw()
    {
        if (playerTr == null)
            return transform.position;

        if (playerCollider != null)
            return playerCollider.bounds.center;

        Collider c = playerTr.GetComponentInChildren<Collider>();
        if (c != null)
            return c.bounds.center;

        return playerTr.position + Vector3.up * aimHeightFallback;
    }


    private Vector3 GetPredictedAimPoint()
    {
        Vector3 origin = (muzzle != null) ? muzzle.position : transform.position;
        Vector3 target = GetPlayerAimPointRaw();

        float s = Mathf.Max(currentWeapon.bulletSpeed, 0.001f);
        Vector3 v = estimatedPlayerVel;
        Vector3 r = target - origin;

        // |r + v t| = s t  -> (v·v - s^2)t^2 + 2(r·v)t + (r·r) = 0
        float a = Vector3.Dot(v, v) - s * s;
        float b = 2f * Vector3.Dot(r, v);
        float c = Vector3.Dot(r, r);

        float t = 0f;

        if (Mathf.Abs(a) < 0.0001f)
        {
            if (Mathf.Abs(b) > 0.0001f) t = -c / b;
            else t = 0f;
        }
        else
        {
            float disc = b * b - 4f * a * c;

            if (disc < 0f)
            {
                t = r.magnitude / s;
            }
            else
            {
                float sqrt = Mathf.Sqrt(disc);
                float t1 = (-b - sqrt) / (2f * a);
                float t2 = (-b + sqrt) / (2f * a);

                if (t1 > 0f && t2 > 0f) t = Mathf.Min(t1, t2);
                else if (t1 > 0f) t = t1;
                else if (t2 > 0f) t = t2;
                else t = 0f;
            }
        }

        t = Mathf.Clamp(t, 0f, maxLeadTime);
        return target + v * t;
    }

    private bool CanEngage(Vector3 aimPoint)
    {
        float dist = Vector3.Distance(transform.position, aimPoint);
        return dist <= engageDistance;
    }

    private void RotateTowardsAimPoint(Vector3 aimPoint)
    {
        // Yaw corpo
        Vector3 toTarget = aimPoint - transform.position;
        Vector3 dirY = new Vector3(toTarget.x, 0f, toTarget.z);

        if (dirY.sqrMagnitude > 0.0001f)
        {
            Quaternion yawRot = Quaternion.LookRotation(dirY.normalized);
            transform.rotation = Quaternion.Lerp(transform.rotation, yawRot, rotationSpeed * Time.deltaTime);
        }

        // Pitch arma (se presente)
        if (weaponHolder != null && muzzle != null)
        {
            Vector3 dir = (aimPoint - muzzle.position).normalized;

            float horizontal = new Vector3(dir.x, 0f, dir.z).magnitude;
            float targetPitch = -Mathf.Atan2(dir.y, Mathf.Max(horizontal, 0.0001f)) * Mathf.Rad2Deg;
            targetPitch = Mathf.Clamp(targetPitch, minWeaponPitch, maxWeaponPitch);

            float currentPitch = weaponHolder.localEulerAngles.x;
            if (currentPitch > 180f) currentPitch -= 360f;

            float lerpedPitch = Mathf.Lerp(currentPitch, targetPitch, weaponPitchSpeed * Time.deltaTime);
            weaponHolder.localRotation = Quaternion.Euler(lerpedPitch, 0f, 0f);
        }
    }

    private bool CheckLineOfSight(Vector3 aimPoint)
    {
        if (muzzle == null) return false;

        Vector3 origin = muzzle.position;
        Vector3 dir = (aimPoint - origin).normalized;
        float dist = Vector3.Distance(origin, aimPoint);

        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist, ~0, QueryTriggerInteraction.Ignore))
        {
            Health h = hit.collider.GetComponentInParent<Health>();
            return (h != null && h.Team == Team.Player);
        }

        return false;
    }

    protected override Vector3 GetAimPoint()
    {
        // Shooter base chiede “aim point”: per l’enemy usiamo quello predetto (cachato)
        return cachedAimPoint != Vector3.zero ? cachedAimPoint : GetPlayerAimPointRaw();
    }

    protected override void FirePellet(Vector3 direction, bool ballistic, Vector3 aimPoint)
    {
        // Enemy: projectile
        if (muzzle == null || currentWeapon == null) return;
        if (currentWeapon.bulletPrefab == null) return;

        Vector3 spawnPos = muzzle.position;
        Quaternion rot = Quaternion.LookRotation(direction);

        GameObject bulletObj = Instantiate(currentWeapon.bulletPrefab, spawnPos, rot);

        // Se il prefab ha un Rigidbody, abilitiamo la gravità per Ballistic
        if (bulletObj.TryGetComponent(out Rigidbody rb))
        {
            rb.useGravity = ballistic;
        }

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            Collider myCollider = GetComponent<Collider>();
            bullet.Initialize(currentWeapon.bulletSpeed, myHealth.Team, currentWeapon.bulletDamage, myCollider);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, engageDistance);
    }
}
