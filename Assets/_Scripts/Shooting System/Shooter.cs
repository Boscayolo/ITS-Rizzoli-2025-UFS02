using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Shooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] protected Transform muzzle;

    [Header("Weapon")]
    [SerializeField] protected WeaponData currentWeapon;
    [SerializeField] protected Transform weaponHolderTransform;

    [SerializeField] protected int bulletsLeft;
    [SerializeField] protected int bulletPoolSize = 30;

    protected bool readyToShoot;
    protected bool reloading;

    protected Coroutine reloadingCrt;
    protected Coroutine fireRateCrt;

    protected virtual void Awake()
    {
        readyToShoot = true;

        if (currentWeapon != null)
            EquipWeapon(currentWeapon);
    }

    public virtual void EquipWeapon(WeaponData weapon, bool refillAmmo = true)
    {
        currentWeapon = weapon;

        //if (currentWeapon == null)
        //    return;

        if (refillAmmo)
            bulletsLeft = currentWeapon.clipSize;

        if (weaponHolderTransform.childCount > 1)
        {
            Destroy(weaponHolderTransform.GetChild(1).gameObject);
        }

        Transform weaponTransform = Instantiate(weapon.weaponModel, weaponHolderTransform.position, weaponHolderTransform.rotation).transform;
        weaponTransform.SetParent(weaponHolderTransform);
    }

    public virtual void TryShoot()
    {
        if (currentWeapon == null) return;
        if (!readyToShoot) return;
        if (reloading) return;
        if (bulletsLeft <= 0) return;

        // Consuma 1 colpo per “trigger pull”
        bulletsLeft--;

        Vector3 aimPoint = GetAimPoint();
        Vector3 baseDir = GetDirectionToAimPoint(aimPoint);

        // Applica pattern
        switch (currentWeapon.bulletPattern)
        {
            case BulletPattern.Linear:
                FirePellet(baseDir, ballistic: false, aimPoint);
                break;

            case BulletPattern.Shotgun:
                int pellets = Mathf.Max(1, currentWeapon.pellets);
                for (int i = 0; i < pellets; i++)
                {
                    Vector3 dir = ApplySpread(baseDir, currentWeapon.spreadAngle);
                    FirePellet(dir, ballistic: false, aimPoint);
                }
                break;

            case BulletPattern.Ballistic:
                Vector3 ballisticDir = baseDir;

                if (currentWeapon.useBallisticAim &&
                    TryGetBallisticDirection(muzzle.position, aimPoint, currentWeapon.bulletSpeed,
                        Mathf.Abs(Physics.gravity.y) * currentWeapon.gravityMultiplier,
                        out Vector3 solvedDir))
                {
                    ballisticDir = solvedDir;
                }

                FirePellet(ballisticDir, ballistic: true, aimPoint);
                break;
        }

        // Cooldown fire rate
        readyToShoot = false;
        float secondsBetweenShots = 1f / Mathf.Max(currentWeapon.fireRate, 0.0001f);
        if (fireRateCrt != null) StopCoroutine(fireRateCrt);
        fireRateCrt = StartCoroutine(FireRateCd(secondsBetweenShots));
    }

    protected virtual void Reload()
    {
        if (currentWeapon == null) return;
        if (reloadingCrt != null) return;

        reloading = true;
        reloadingCrt = StartCoroutine(ReloadWait(currentWeapon.reloadTime));
    }

    protected virtual IEnumerator ReloadWait(float reloadTime)
    {
        yield return new WaitForSeconds(reloadTime);
        bulletsLeft = currentWeapon != null ? currentWeapon.clipSize : bulletsLeft;
        reloading = false;
        reloadingCrt = null;
    }

    protected IEnumerator FireRateCd(float fireRateSeconds)
    {
        yield return new WaitForSeconds(fireRateSeconds);
        readyToShoot = true;
        fireRateCrt = null;
    }

    // --- Hooks (override per Enemy/Player) ---

    /// <summary>
    /// Punto che stai “mirando”. Player: avanti. Enemy: posizione predetta del player.
    /// </summary>
    protected virtual Vector3 GetAimPoint()
    {
        if (muzzle == null) return transform.position + transform.forward;
        float r = currentWeapon != null ? currentWeapon.range : 50f;
        return muzzle.position + muzzle.forward * r;
    }

    protected virtual Vector3 GetDirectionToAimPoint(Vector3 aimPoint)
    {
        if (muzzle == null) return transform.forward;
        Vector3 dir = (aimPoint - muzzle.position);
        return dir.sqrMagnitude > 0.0001f ? dir.normalized : muzzle.forward;
    }

    #region POOLING
    //POOLING
    protected Dictionary<GameObject, Queue<GameObject>> bulletPools = new();
    protected void EnsurePoolFor(GameObject prefab)
    {
        if (prefab == null || bulletPools.ContainsKey(prefab)) return;

        Queue<GameObject> q = new Queue<GameObject>(Mathf.Max(1, bulletPoolSize));
        for (int i = 0; i < Mathf.Max(1, bulletPoolSize); i++)
        {
            var go = Instantiate(prefab);
            go.SetActive(false);
            q.Enqueue(go);
        }
        bulletPools[prefab] = q;
    }

    protected GameObject TakeFromQueue(GameObject prefab)
    {
        var q = bulletPools[prefab];
        var go = q.Dequeue();

        if (go.activeSelf)
        {
            // riciclo forzato
            go.SetActive(false);
        }

        q.Enqueue(go);
        return go;
    }

    #endregion

    /// <summary>
    /// “Sparo elementare”: 1 pellet/raycast o 1 proiettile. Implementato dalle classi figlie.
    /// </summary>
    protected abstract void FirePellet(Vector3 direction, bool ballistic, Vector3 aimPoint);

    // --- Helpers ---

    protected Vector3 ApplySpread(Vector3 dir, float spreadAngle)
    {
        if (spreadAngle <= 0f) return dir;

        float yaw = Random.Range(-spreadAngle, spreadAngle);
        float pitch = Random.Range(-spreadAngle, spreadAngle);

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        return (rot * dir).normalized;
    }

    /// <summary>
    /// Calcolo direzione balistica per colpire target con velocità iniziale speed e gravità g (positiva).
    /// Ritorna false se non c’è soluzione reale.
    /// </summary>
    protected bool TryGetBallisticDirection(Vector3 origin, Vector3 target, float speed, float g, out Vector3 dir)
    {
        dir = Vector3.zero;

        Vector3 toTarget = target - origin;
        Vector3 toTargetXZ = new Vector3(toTarget.x, 0f, toTarget.z);

        float x = toTargetXZ.magnitude;
        float y = toTarget.y;

        if (x < 0.001f)
            return false;

        float v = Mathf.Max(0.001f, speed);
        float v2 = v * v;
        float v4 = v2 * v2;

        float discriminant = v4 - g * (g * x * x + 2f * y * v2);
        if (discriminant < 0f)
            return false;

        float sqrt = Mathf.Sqrt(discriminant);

        // angolo basso (più “teso”)
        float angle = Mathf.Atan((v2 - sqrt) / (g * x));

        Vector3 dirXZ = toTargetXZ.normalized;
        dir = (dirXZ * Mathf.Cos(angle) + Vector3.up * Mathf.Sin(angle)).normalized;
        return true;
    }
}
