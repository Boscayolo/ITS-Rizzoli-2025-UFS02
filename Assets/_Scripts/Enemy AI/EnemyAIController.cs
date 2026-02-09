using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Enemy AI Controller - Complete Version with NavMesh
/// </summary>
public class EnemyAIController : MonoBehaviour
{
    [Header("References")]
    public EnemyShooter shooter;
    public WaypointPath patrolPath;

    [Header("Navigation")]
    public NavMeshAgent agent;

    [Header("Speed Settings")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float fleeSpeed = 6f;
    public float searchSpeed = 3f;

    [Header("Combat Settings")]
    public float shootRange = 18f;
    public float engageDistance = 50f;
    public float fleeDistance = 8f;           // Distanza per attivare FLEE
    public float fleeSafeDistance = 15f;      // Distanza di sicurezza per uscire da FLEE
    public float tooCloseTimer = 2f;          // Secondi prima di passare a FLEE se troppo vicino

    [Header("Search Settings")]
    public float searchDuration = 10f;        // Tempo massimo in stato SEARCH
    public float lookAroundDuration = 3f;     // Tempo di rotazione quando arriva all'ultima posizione
    public float lookAroundSpeed = 90f;       // Gradi/secondo

    [Header("Patrol Settings")]
    public float waypointWaitMin = 2f;        // Tempo minimo di sosta
    public float waypointWaitMax = 5f;        // Tempo massimo di sosta
    public float waypointReachedDistance = 1f;

    // Current state
    public AIState currentState = AIState.IDLE;

    // Components (public so EnemyState can access them)
    public VisionScanner vision;
    public Health health;

    // State object
    private EnemyState currentStateObject;

    // Last known player position (used by SEARCH state)
    public Vector3 lastKnownPlayerPosition { get; private set; }
    public bool hasLastKnownPosition { get; private set; }

    void Start()
    {
        // Ottieni componenti
        if (!vision) vision = GetComponent<VisionScanner>();
        if (!health) health = GetComponent<Health>();
        if (!shooter) shooter = GetComponentInChildren<EnemyShooter>();
        if (!agent) agent = GetComponent<NavMeshAgent>();

        // Configura NavMeshAgent
        if (agent != null)
        {
            agent.speed = patrolSpeed;
            agent.angularSpeed = 120f;
            agent.acceleration = 8f;
            agent.stoppingDistance = shootRange * 0.8f;
            agent.autoBraking = true;
        }

        // IMPORTANTE: Collega il VisionScanner allo Shooter
        if (shooter != null && vision != null)
        {
            shooter.SetVisionScanner(vision);
        }

        // Registra evento morte
        if (health != null)
            health.OnDied += OnDeath;

        // Inizia in stato PATROL o IDLE
        if (patrolPath != null && patrolPath.waypoints.Length > 0)
            ChangeState(AIState.PATROL);
        else
            ChangeState(AIState.IDLE);
    }

    void Update()
    {
        if (currentState == AIState.DEAD)
            return;

        // Aggiorna ultima posizione nota del player
        if (vision.hasTarget && vision.canSeePlayer)
        {
            lastKnownPlayerPosition = vision.targetPosition;
            hasLastKnownPosition = true;
        }

        currentStateObject?.Tick();
    }

    public void ChangeState(AIState newState)
    {
        currentStateObject?.Exit();

        currentState = newState;

        currentStateObject = newState switch
        {
            AIState.IDLE => new EnemyState_Idle(this),
            AIState.PATROL => new EnemyState_Patrol(this),
            AIState.CHASE => new EnemyState_Chase(this),
            AIState.SHOOT => new EnemyState_Shoot(this),
            AIState.FLEE => new EnemyState_Flee(this),
            AIState.SEARCH => new EnemyState_Search(this),
            AIState.DEAD => new EnemyState_Dead(this),
            _ => new EnemyState_Idle(this)
        };

        currentStateObject.Enter();
    }

    public void FaceTowards(Vector3 targetPoint)
    {
        Vector3 direction = targetPoint - transform.position;
        direction.y = 0f;

        if (direction.magnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                10f * Time.deltaTime
            );
        }
    }

    /// <summary>
    /// Rotazione graduale verso una direzione (per look around)
    /// </summary>
    public void RotateTowards(Vector3 direction, float speed)
    {
        if (direction.magnitude < 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            speed * Time.deltaTime
        );
    }

    /// <summary>
    /// Configura la distanza massima di ingaggio (chiamato da Entity durante l'inizializzazione)
    /// </summary>
    public void SetEngageDistance(float distance)
    {
        engageDistance = distance;

        // Aggiorna anche il VisionScanner se presente
        if (vision != null)
        {
            vision.detectionRadius = Mathf.Max(distance, vision.detectionRadius);
        }

        // Aggiorna stopping distance del NavMeshAgent
        if (agent != null)
        {
            agent.stoppingDistance = shootRange * 0.8f;
        }
    }

    /// <summary>
    /// Trova un punto di fuga valido sulla NavMesh
    /// </summary>
    public bool TryGetFleePosition(out Vector3 fleePosition)
    {
        fleePosition = transform.position;

        if (!vision.hasTarget) return false;

        // Direzione opposta al player
        Vector3 awayFromPlayer = (transform.position - vision.targetPosition).normalized;

        // Prova diverse distanze e angoli
        float[] distances = { 15f, 20f, 10f };
        float[] angles = { 0f, 30f, -30f, 60f, -60f, 90f, -90f };

        foreach (float dist in distances)
        {
            foreach (float angle in angles)
            {
                Vector3 direction = Quaternion.Euler(0, angle, 0) * awayFromPlayer;
                Vector3 targetPos = transform.position + direction * dist;

                // Verifica se la posizione è sulla NavMesh
                if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                {
                    // Verifica che il punto sia più lontano dal player
                    float currentDist = Vector3.Distance(transform.position, vision.targetPosition);
                    float newDist = Vector3.Distance(hit.position, vision.targetPosition);

                    if (newDist > currentDist)
                    {
                        fleePosition = hit.position;
                        return true;
                    }
                }
            }
        }

        return false;
    }

    void OnDeath(Health health)
    {
        ChangeState(AIState.DEAD);
    }

    void OnDestroy()
    {
        if (health != null)
            health.OnDied -= OnDeath;
    }

    void OnDrawGizmosSelected()
    {
        // Visualizza distanze
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, engageDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, fleeDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, fleeSafeDistance);

        // Visualizza ultima posizione nota
        if (hasLastKnownPosition)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(lastKnownPlayerPosition, 0.5f);
            Gizmos.DrawLine(transform.position, lastKnownPlayerPosition);
        }
    }
}