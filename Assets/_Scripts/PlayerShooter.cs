using UnityEngine;

public class PlayerShooter : Shooter
{
    private PlayerInputActions inputActions;
    private bool isHoldingFire;

    protected override void Awake()
    {
        base.Awake();

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

        if (!hitSomething || hit.collider == null) return;

        if (hit.collider.TryGetComponent(out Health healthHit) && healthHit.Team != Team.Player)
        {
            healthHit.TakeDamage(currentWeapon.bulletDamage);
        }
    }

    private void OnEnable() => inputActions?.Enable();
    private void OnDisable() => inputActions?.Disable();
    private void OnDestroy() => inputActions?.Dispose();
}
