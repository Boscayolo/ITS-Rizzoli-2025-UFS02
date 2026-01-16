using UnityEngine;

public class EnemyShooter : Shooter
{
    private Transform playerTr;
    private Health playerHealth;
    private Health myHealth;
    private Transform weaponHolder;

    [SerializeField] private float engageDistance = 50f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float weaponPitchSpeed = 10f;
    [SerializeField] private float minWeaponPitch = -45f;
    [SerializeField] private float maxWeaponPitch = 60f;
    [SerializeField] private float aimPredictionFactor = 10f;

    [Header("Weapon properties")]
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform weaponHolderTransform;

    [Header("Detection properties")]
    [SerializeField] private float detectionRadius = 50f;
    [SerializeField] private float scanFrequency = 0.5f;
    [SerializeField] private float spotReactionTime = 1.5f;

    private bool playerSpotted;
    private float spotTimer;
    private float scanTimer;

    protected override void Awake()
    {
        base.Awake();
        myHealth = GetComponent<Health>();
        weaponHolder = weaponHolderTransform;

        if (weaponHolder == null)
        {
            Debug.LogError("[EnemyShooter] WeaponHolder non assegnato! Assegnalo nell'inspector.");
        }

        scanTimer = scanFrequency;
    }

    private void Update()
    {
        ScanForPlayer();

        if (!playerSpotted || playerTr == null)
        {
            return;
        }

        // Se ha visto il giocatore, incrementa il timer di reazione
        if (spotTimer < spotReactionTime)
        {
            spotTimer += Time.deltaTime;
            return;
        }

        // Ruota verso il giocatore predetto
        RotateTowardsPredictedPosition();

        // Spara se ha munizioni
        if (bulletsLeft > 0)
        {
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

        if (scanTimer > 0) return;

        scanTimer = scanFrequency;

        // Cerca il giocatore in una sfera (360 gradi intorno al nemico)
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);

        bool previouslySpotted = playerSpotted;
        playerSpotted = false;
        playerTr = null;
        playerHealth = null;

        foreach (Collider col in colliders)
        {
            if (col.TryGetComponent(out Health health))
            {
                if (health.Team == Team.Player)
                {
                    playerSpotted = true;
                    playerTr = col.transform;
                    playerHealth = health;

                    // Resetta il timer SOLO se non era già stato avvistato
                    if (!previouslySpotted)
                    {
                        spotTimer = 0;
                    }
                    break;
                }
            }
        }

        // Se non vede più il giocatore, resetta
        if (!playerSpotted && previouslySpotted)
        {
            spotTimer = 0;
        }
    }

    private void RotateTowardsPredictedPosition()
    {
        Vector3 predictedPosition = GetPredictedPlayerPosition();
        Vector3 directionToPredicted = (predictedPosition - transform.position).normalized;

        // CORPO: Ruota solo su Y verso il giocatore
        Vector3 directionYOnly = new Vector3(directionToPredicted.x, 0, directionToPredicted.z).normalized;
        Quaternion targetYawRotation = Quaternion.LookRotation(directionYOnly);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetYawRotation, rotationSpeed * Time.deltaTime);

        // ARMA: Ruota su X (pitch) per mirare verticalmente
        if (weaponHolder != null)
        {
            // Calcola il pitch necessario
            float targetPitch = -Mathf.Asin(Mathf.Clamp(directionToPredicted.y, -1f, 1f)) * Mathf.Rad2Deg;
            targetPitch = Mathf.Clamp(targetPitch, minWeaponPitch, maxWeaponPitch);

            // Estrai il pitch attuale dell'arma
            Vector3 currentEuler = weaponHolder.localEulerAngles;
            float currentPitch = currentEuler.x;
            if (currentPitch > 180) currentPitch -= 360;

            // Lerp il pitch
            float lerpedPitch = Mathf.Lerp(currentPitch, targetPitch, weaponPitchSpeed * Time.deltaTime);

            // Applica la rotazione X al weaponHolder
            weaponHolder.localRotation = Quaternion.Euler(lerpedPitch, 0, 0);
        }
    }

    private Vector3 GetPredictedPlayerPosition()
    {
        Vector3 currentPlayerPos = playerTr.position;

        // Cerca il Rigidbody del giocatore
        Rigidbody playerRb = playerTr.GetComponent<Rigidbody>();

        if (playerRb != null)
        {
            Vector3 playerVelocity = playerRb.linearVelocity;
            return currentPlayerPos + playerVelocity * aimPredictionFactor;
        }

        return currentPlayerPos;
    }

    protected override void Shoot()
    {
        if (playerTr == null || !playerSpotted)
        {
            return;
        }

        // Verifica line of sight prima di sparare
        if (!CheckLineOfSight(playerTr))
        {
            return;
        }

        Debug.Log("Enemy Shooting");

        bulletsLeft--;

        // Istanzia il bullet dalla posizione della muzzle
        Vector3 bulletSpawnPos = muzzle.position;
        Quaternion bulletRotation = Quaternion.LookRotation(muzzle.forward);

        GameObject bulletObj = Instantiate(bulletPrefab, bulletSpawnPos, bulletRotation);
        Bullet bullet = bulletObj.GetComponent<Bullet>();

        if (bullet != null)
        {
            Collider myCollider = GetComponent<Collider>();
            bullet.Initialize(bulletSpeed, myHealth.Team, bulletDamage, myCollider);
        }
    }

    private bool CheckLineOfSight(Transform target)
    {
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        Vector3 rayOrigin = transform.position;
        RaycastHit hit;

        if (Physics.Raycast(rayOrigin, directionToTarget, out hit, engageDistance))
        {
            // Se colpisce il giocatore o un suo child, la linea di vista è libera
            if (hit.collider.TryGetComponent(out Health health) && health.Team == Team.Player)
            {
                return true;
            }
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        // Visualizza la sfera di detection
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Visualizza la distanza di engagement
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, engageDistance);
    }
}