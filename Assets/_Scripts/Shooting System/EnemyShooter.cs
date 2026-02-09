using System.Collections.Generic;
using UnityEngine;

public class EnemyShooter : Shooter
{
    [Header("Weapon Pitch")]
    [SerializeField] private float weaponPitchSpeed = 10f;
    [SerializeField] private float minWeaponPitch = -45f;
    [SerializeField] private float maxWeaponPitch = 60f;

    [Header("Prediction")]
    [Range(0f, 1f)]
    [SerializeField] private float velocityLerp = 0.4f;
    [SerializeField] private float maxLeadTime = 1.25f;
    [SerializeField] private float aimHeightFallback = 1.2f;

    private Health myHealth;
    private Transform weaponHolder;

    // Velocity estimation per prediction
    private bool hasPlayerPos;
    private Vector3 lastPlayerPos;
    private Vector3 estimatedPlayerVel;

    // Cache dell'aim point calcolato
    private Vector3 cachedAimPoint;

    // Reference al VisionScanner (impostato dall'AIController)
    private VisionScanner vision;

    protected override void Awake()
    {
        base.Awake();
        myHealth = GetComponent<Health>();
        weaponHolder = weaponHolderTransform;

        // Pre-warm opzionale per l'arma corrente
        if (currentWeapon != null && currentWeapon.bulletPrefab != null)
            EnsurePoolFor(currentWeapon.bulletPrefab);
    }

    /// <summary>
    /// Imposta il reference al VisionScanner (chiamato dall'AIController)
    /// </summary>
    public void SetVisionScanner(VisionScanner _vision)
    {
        vision = _vision;
        ResetVelocityEstimate();
    }

    private void Update()
    {
        // Se non abbiamo vision scanner o target, non facciamo nulla
        if (vision == null || !vision.hasTarget || currentWeapon == null)
        {
            ResetVelocityEstimate();
            return;
        }

        // Aggiorniamo la stima della velocità del player
        UpdatePlayerVelocityEstimate();

        // Calcoliamo l'aim point predetto
        cachedAimPoint = GetPredictedAimPoint();

        // Ruotiamo verso il target
        RotateTowardsAimPoint(cachedAimPoint);

        // Gestione munizioni
        if (bulletsLeft <= 0 && !reloading)
        {
            Reload();
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
        if (vision == null || !vision.hasTarget) return;

        Vector3 pos = vision.targetPosition;
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

    private Vector3 GetPredictedAimPoint()
    {
        if (vision == null || !vision.hasTarget)
            return transform.position;

        Vector3 origin = (muzzle != null) ? muzzle.position : transform.position;
        Vector3 target = vision.aimPoint; // Usiamo l'aim point del VisionScanner

        float s = Mathf.Max(currentWeapon.bulletSpeed, 0.001f);
        Vector3 v = estimatedPlayerVel;
        Vector3 r = target - origin;

        // Risolviamo l'equazione quadratica per la predizione
        // |r + v*t| = s*t  -> (v·v - s²)t² + 2(r·v)t + (r·r) = 0
        float a = Vector3.Dot(v, v) - s * s;
        float b = 2f * Vector3.Dot(r, v);
        float c = Vector3.Dot(r, r);

        float t = 0f;

        if (Mathf.Abs(a) < 0.0001f)
        {
            // Equazione lineare
            if (Mathf.Abs(b) > 0.0001f) t = -c / b;
            else t = 0f;
        }
        else
        {
            // Equazione quadratica
            float disc = b * b - 4f * a * c;

            if (disc < 0f)
            {
                // Nessuna soluzione reale, usiamo tempo base
                t = r.magnitude / s;
            }
            else
            {
                float sqrt = Mathf.Sqrt(disc);
                float t1 = (-b - sqrt) / (2f * a);
                float t2 = (-b + sqrt) / (2f * a);

                // Prendiamo la soluzione positiva minima
                if (t1 > 0f && t2 > 0f) t = Mathf.Min(t1, t2);
                else if (t1 > 0f) t = t1;
                else if (t2 > 0f) t = t2;
                else t = 0f;
            }
        }

        t = Mathf.Clamp(t, 0f, maxLeadTime);
        return target + v * t;
    }

    private void RotateTowardsAimPoint(Vector3 aimPoint)
    {
        // Pitch dell'arma (se presente weaponHolder)
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

    protected override Vector3 GetAimPoint()
    {
        // Shooter base chiede l'aim point: restituiamo quello predetto
        if (vision != null && vision.hasTarget)
            return cachedAimPoint != Vector3.zero ? cachedAimPoint : vision.aimPoint;

        return transform.forward * 100f;
    }

    protected override void FirePellet(Vector3 direction, bool ballistic, Vector3 aimPoint)
    {
        if (muzzle == null || currentWeapon == null) return;
        if (currentWeapon.bulletPrefab == null) return;

        // Assicuriamoci che il pool esista
        EnsurePoolFor(currentWeapon.bulletPrefab);

        // Prendi dal pool, configura e attiva
        GameObject bulletObj = TakeFromQueue(currentWeapon.bulletPrefab);

        bulletObj.transform.SetPositionAndRotation(
            muzzle.position, Quaternion.LookRotation(direction));
        bulletObj.SetActive(true);

        // Configura physics
        if (bulletObj.TryGetComponent(out Rigidbody rb))
        {
            rb.useGravity = ballistic;
            rb.linearVelocity = muzzle.forward * currentWeapon.bulletSpeed;
            rb.angularVelocity = Vector3.zero;
        }

        // Inizializza bullet component
        if (bulletObj.TryGetComponent(out Bullet bullet))
        {
            Collider myCollider = GetComponent<Collider>();
            bullet.Initialize(currentWeapon.bulletSpeed, myHealth.Team, currentWeapon.bulletDamage, myCollider);
        }
    }

    void OnDisable()
    {
        // Disattiva tutti i proiettili nei pool
        foreach (var kv in bulletPools)
            foreach (var b in kv.Value)
                if (b) b.SetActive(false);
    }
}