using UnityEngine;

public class EnemyShooter : Shooter
{
    private Transform playerTr;
    private Health playerHealth;
    private Health myHealth;

    [SerializeField] private float engageDistance = 50f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float aimPredictionFactor = 10f;

    [Header("Weapon properties")]
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private GameObject bulletPrefab;

    [Header("Detection properties")]
    [SerializeField] private float detectionRadius = 50f;
    [SerializeField] private float scanFrequency = 0.5f; // ogni quanto secondi scanna per il player
    [SerializeField] private float spotReactionTime = 1.5f; // tempo prima di iniziare a sparare dopo aver avvistato
    [SerializeField] private float raycastConeAngle = 30f; // angolo del cono di raycast

    private bool playerSpotted;
    private float spotTimer;
    private float scanTimer;

    protected override void Awake()
    {
        base.Awake();
        myHealth = GetComponent<Health>();
        scanTimer = scanFrequency;
    }

    private void Update()
    {
        // Scansiona l'area per il giocatore
        ScanForPlayer();

        Debug.Log($"[EnemyShooter] playerSpotted: {playerSpotted}, playerTr: {playerTr}, spotTimer: {spotTimer}/{spotReactionTime}");

        if (!playerSpotted || playerTr == null)
        {
            Debug.Log($"[EnemyShooter] Uscendo - playerSpotted: {playerSpotted}, playerTr: {playerTr}");
            return;
        }

        // Se ha visto il giocatore, incrementa il timer di reazione
        if (spotTimer < spotReactionTime)
        {
            spotTimer += Time.deltaTime;
            Debug.Log($"[EnemyShooter] Aspettando reazione... {spotTimer}/{spotReactionTime}");
            return;
        }

        Debug.Log($"[EnemyShooter] REAZIONE COMPLETATA! Inizio rotazione e sparo");

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
        Debug.Log($"[EnemyShooter] Scansione: trovati {colliders.Length} collider");

        bool previouslySpotted = playerSpotted;
        playerSpotted = false;
        playerTr = null;
        playerHealth = null;

        foreach (Collider col in colliders)
        {
            Debug.Log($"[EnemyShooter] Controllando collider: {col.gameObject.name}");

            if (col.TryGetComponent(out Health health))
            {
                Debug.Log($"[EnemyShooter] {col.gameObject.name} ha Health, Team: {health.Team}");

                if (health.Team == Team.Player)
                {
                    Debug.Log($"[EnemyShooter] Trovato giocatore nell'area!");
                    playerSpotted = true;
                    playerTr = col.transform;
                    playerHealth = health;

                    // Resetta il timer SOLO se non era già stato avvistato
                    if (!previouslySpotted)
                    {
                        spotTimer = 0;
                        Debug.Log($"[EnemyShooter] PRIMO AVVISTAMENTO - Timer azzerato");
                    }
                    break;
                }
            }
        }

        // Se non vede più il giocatore, resetta
        if (!playerSpotted && previouslySpotted)
        {
            spotTimer = 0;
            Debug.Log($"[EnemyShooter] Giocatore PERSO");
        }
    }

    private void RotateTowardsPredictedPosition()
    {
        Vector3 predictedPosition = GetPredictedPlayerPosition();
        Vector3 directionToPredicted = (predictedPosition - transform.position).normalized;

        // Calcola la rotazione desiderata (solo su Y)
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(directionToPredicted.x, 0, directionToPredicted.z));

        // Lerp la rotazione
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        Debug.Log($"[EnemyShooter] Rotazione verso: {targetRotation.eulerAngles}, Rotazione attuale: {transform.rotation.eulerAngles}");
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
            Debug.Log($"[EnemyShooter] Non posso sparare - playerTr: {playerTr}, playerSpotted: {playerSpotted}");
            return;
        }

        // Verifica line of sight prima di sparare
        if (!CheckLineOfSight(playerTr))
        {
            Debug.Log($"[EnemyShooter] Line of sight bloccata - non sparo");
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
        else
        {
            Debug.LogError($"[EnemyShooter] Il prefab non ha il componente Bullet!");
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

        // Visualizza il cono di visione
        Gizmos.color = Color.cyan;
        Vector3 coneDirection = transform.forward * engageDistance;
        Gizmos.DrawLine(transform.position, transform.position + coneDirection);

        float coneRadiusAtDistance = engageDistance * Mathf.Tan(raycastConeAngle * Mathf.Deg2Rad);
        Gizmos.DrawWireSphere(transform.position + coneDirection, coneRadiusAtDistance);
    }
}