using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShooter : Shooter
{
    [SerializeField] Image reloadCircle;
    private PlayerInputActions inputActions;
    private bool isHoldingFire;

    protected override void Awake()
    {
        base.Awake();

        reloadCircle.fillAmount = 0f;

        inputActions = new PlayerInputActions();

        inputActions.Player.Reload.performed += _ => Reload();

        inputActions.Player.Fire.performed += _ =>
        {
            if (currentWeapon == null) return;

            if (currentWeapon.shootingBehavior == ShootingBehavior.Semi_Auto)
            {
                // 1 colpo per pressione
                TryShoot();
            }
            else
            {
                // Auto: tieni premuto
                isHoldingFire = true;
            }
        };

        inputActions.Player.Fire.canceled += _ =>
        {
            isHoldingFire = false;
        };
    }

    private void Update()
    {
        if (currentWeapon == null) return;

        if (currentWeapon.shootingBehavior == ShootingBehavior.Auto)
        {
            if (isHoldingFire)
            {
                if (bulletsLeft > 0) TryShoot();
                else if (!reloading) Reload();
            }
        }
        // Semi_Auto: spara solo su performed (gestito sopra)
    }

    protected override void FirePellet(Vector3 direction, bool ballistic, Vector3 aimPoint)
    {
        // Player: hitscan (raycast)
        if (muzzle == null || currentWeapon == null) return;

        RaycastHit hit;
        bool hitSomething = Physics.Raycast(muzzle.position, direction, out hit, currentWeapon.range);

        // Determina il punto finale del tracer
        Vector3 endPoint = hitSomething ? hit.point : muzzle.position + direction * currentWeapon.range;

        // Spawna il tracer visuale se configurato
        if (currentWeapon.tracerPrefab != null)
        {
            GameObject tracerObj = Instantiate(currentWeapon.tracerPrefab);
            BulletTracer tracer = tracerObj.GetComponent<BulletTracer>();

            if (tracer != null)
            {
                tracer.Initialize(muzzle.position, endPoint, currentWeapon.tracerLifetime, currentWeapon.tracerColor);
            }
            else
            {
                Debug.LogWarning("TracerPrefab non ha il component BulletTracer!");
                Destroy(tracerObj);
            }
        }

        // Applica danno se ha colpito qualcosa
        if (!hitSomething || hit.collider == null) return;

        if (hit.collider.TryGetComponent(out Health healthHit) && healthHit.Team != Team.Player)
        {
            healthHit.TakeDamage(currentWeapon.bulletDamage);
        }
    }

    protected override IEnumerator ReloadWait(float reloadTime)
    {
        float elapsed = 0f;
        while (elapsed < reloadTime)
        {
            elapsed += Time.deltaTime;
            reloadCircle.fillAmount = elapsed / reloadTime;
            yield return null;
        }

        reloadCircle.fillAmount = 0f;
        bulletsLeft = currentWeapon != null ? currentWeapon.clipSize : bulletsLeft;
        reloading = false;
        reloadingCrt = null;
    }

   
    private void OnEnable() => inputActions?.Enable();
    private void OnDisable() => inputActions?.Disable();
    private void OnDestroy() => inputActions?.Dispose();
}