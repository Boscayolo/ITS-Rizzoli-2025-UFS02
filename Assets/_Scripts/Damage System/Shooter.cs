using UnityEngine;

public class Shooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private Transform muzzle;
    [SerializeField] private Aimer aimer;   // se non c'è, userà muzzle.forward

    [Header("Owner")]
    [SerializeField] private Team ownerTeam = Team.Player;

    [Header("Shooting Settings")]
    [Tooltip("Shots per second")]
    [SerializeField] private float fireRate = 2f;
    [SerializeField] private bool autoFire = false;

    private float _nextShotTime;

    protected virtual void Awake()
    {
        if (aimer == null)
        {
            aimer = GetComponent<Aimer>();
        }
    }

    protected virtual void Update()
    {
        if (autoFire)
        {
            TryShoot();
        }
    }

    //Chiamata per lo sparo. Viene accettato lo sparo in base al fire rate
    public void TryShoot()
    {
        if (Time.time < _nextShotTime)
            return;

        Shoot();
    }

    //Metodo sparo di base, ignora il fire rate se chiamato direttamente
    public void Shoot()
    {
        if (bulletPrefab == null || muzzle == null)
        {
            Debug.LogWarning($"{name}: Shooter missing bulletPrefab or muzzle reference.");
            return;
        }

        _nextShotTime = Time.time + 1f / fireRate;

        Vector3 direction =
            aimer != null
                ? aimer.GetAimDirection()
                : muzzle.forward;

        Bullet newBullet = Instantiate(
            bulletPrefab,
            muzzle.position,
            Quaternion.LookRotation(direction)
        );

        newBullet.Init(direction, ownerTeam);
    }

    /// Metodo comodo da chiamare direttamente dall'input del player.
    public void TriggerShotFromInput()
    {
        TryShoot();
    }
}
