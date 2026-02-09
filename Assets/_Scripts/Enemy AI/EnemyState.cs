using UnityEngine;

public enum AIState { IDLE, PATROL, CHASE, SHOOT, FLEE, SEARCH, DEAD }

/// <summary>
/// Base class for all enemy states
/// </summary>
public abstract class EnemyState
{
    protected EnemyAIController controller;

    protected EnemyState(EnemyAIController _controller)
    {
        this.controller = _controller;
    }

    public virtual void Enter() { }
    public virtual void Tick() { }
    public virtual void Exit() { }
}

// ============================================================
// STATE: IDLE
// ============================================================
public class EnemyState_Idle : EnemyState
{
    public EnemyState_Idle(EnemyAIController controller) : base(controller) { }

    public override void Enter()
    {
        Debug.Log("[IDLE] Enemy waiting...");

        if (controller.agent != null)
        {
            controller.agent.isStopped = true;
            controller.agent.velocity = Vector3.zero;
        }
    }

    public override void Tick()
    {
        bool canSeePlayer = controller.vision.canSeePlayer;
        bool isInEngageRange = controller.vision.distanceToPlayer <= controller.engageDistance;

        // Entra in CHASE/SHOOT se può vedere il player ED è dentro la distanza di ingaggio
        if (canSeePlayer && isInEngageRange)
        {
            float distToPlayer = controller.vision.distanceToPlayer;

            if (distToPlayer <= controller.shootRange)
                controller.ChangeState(AIState.SHOOT);
            else
                controller.ChangeState(AIState.CHASE);
        }
    }
}

// ============================================================
// STATE: PATROL
// ============================================================
public class EnemyState_Patrol : EnemyState
{
    private int currentWaypointIndex = 0;
    private float waitTimer = 0f;
    private float waitDuration = 0f;
    private bool isWaiting = false;

    public EnemyState_Patrol(EnemyAIController controller) : base(controller) { }

    public override void Enter()
    {
        Debug.Log("[PATROL] Starting patrol...");

        if (controller.agent != null)
        {
            controller.agent.isStopped = false;
            controller.agent.speed = controller.patrolSpeed;
        }

        // Trova il waypoint più vicino
        if (controller.patrolPath != null && controller.patrolPath.waypoints.Length > 0)
        {
            currentWaypointIndex = GetClosestWaypointIndex();
            GoToCurrentWaypoint();
        }
    }

    public override void Tick()
    {
        // Check per player visibile
        bool canSeePlayer = controller.vision.canSeePlayer;
        bool isInEngageRange = controller.vision.distanceToPlayer <= controller.engageDistance;

        if (canSeePlayer && isInEngageRange)
        {
            float distToPlayer = controller.vision.distanceToPlayer;

            if (distToPlayer <= controller.shootRange)
                controller.ChangeState(AIState.SHOOT);
            else
                controller.ChangeState(AIState.CHASE);
            return;
        }

        // Logica di pattugliamento
        if (controller.patrolPath == null || controller.patrolPath.waypoints.Length == 0)
        {
            controller.ChangeState(AIState.IDLE);
            return;
        }

        // Se è in attesa al waypoint
        if (isWaiting)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitDuration)
            {
                // Finita l'attesa, vai al prossimo waypoint
                isWaiting = false;
                currentWaypointIndex = (currentWaypointIndex + 1) % controller.patrolPath.waypoints.Length;
                GoToCurrentWaypoint();
            }
        }
        else
        {
            // Controlla se ha raggiunto il waypoint
            if (HasReachedWaypoint())
            {
                // Inizia attesa casuale
                isWaiting = true;
                waitTimer = 0f;
                waitDuration = Random.Range(controller.waypointWaitMin, controller.waypointWaitMax);

                if (controller.agent != null)
                {
                    controller.agent.isStopped = true;
                    controller.agent.velocity = Vector3.zero;
                }

                Debug.Log($"[PATROL] Reached waypoint {currentWaypointIndex}, waiting {waitDuration:F1}s");
            }
        }
    }

    public override void Exit()
    {
        Debug.Log("[PATROL] Exiting patrol.");
        isWaiting = false;
    }

    private void GoToCurrentWaypoint()
    {
        if (controller.agent != null && controller.patrolPath.waypoints.Length > 0)
        {
            Vector3 destination = controller.patrolPath.waypoints[currentWaypointIndex].position;
            controller.agent.isStopped = false;
            controller.agent.SetDestination(destination);
            Debug.Log($"[PATROL] Moving to waypoint {currentWaypointIndex}");
        }
    }

    private bool HasReachedWaypoint()
    {
        if (controller.agent == null || controller.patrolPath.waypoints.Length == 0)
            return false;

        Vector3 waypointPos = controller.patrolPath.waypoints[currentWaypointIndex].position;
        float distance = Vector3.Distance(controller.transform.position, waypointPos);

        return distance <= controller.waypointReachedDistance && !controller.agent.pathPending;
    }

    private int GetClosestWaypointIndex()
    {
        if (controller.patrolPath.waypoints.Length == 0)
            return 0;

        int closestIndex = 0;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < controller.patrolPath.waypoints.Length; i++)
        {
            float dist = Vector3.Distance(controller.transform.position, controller.patrolPath.waypoints[i].position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestIndex = i;
            }
        }

        return closestIndex;
    }
}

// ============================================================
// STATE: CHASE
// ============================================================
public class EnemyState_Chase : EnemyState
{
    private float tooCloseTimer = 0f;

    public EnemyState_Chase(EnemyAIController controller) : base(controller) { }

    public override void Enter()
    {
        Debug.Log("[CHASE] Chasing player!");

        if (controller.agent != null)
        {
            controller.agent.isStopped = false;
            controller.agent.speed = controller.chaseSpeed;
        }

        tooCloseTimer = 0f;
    }

    public override void Tick()
    {
        if (!controller.vision.hasTarget)
        {
            // Perso il target, vai in SEARCH
            if (controller.hasLastKnownPosition)
                controller.ChangeState(AIState.SEARCH);
            else
                controller.ChangeState(AIState.PATROL);
            return;
        }

        bool canSeePlayer = controller.vision.canSeePlayer;
        bool isInEngageRange = controller.vision.distanceToPlayer <= controller.engageDistance;
        float distToPlayer = controller.vision.distanceToPlayer;

        // Non vede più il player
        if (!canSeePlayer)
        {
            if (controller.hasLastKnownPosition)
                controller.ChangeState(AIState.SEARCH);
            else
                controller.ChangeState(AIState.PATROL);
            return;
        }

        // Fuori dalla engage distance
        if (!isInEngageRange)
        {
            controller.ChangeState(AIState.PATROL);
            return;
        }

        // Player troppo vicino -> accumula timer per FLEE
        if (distToPlayer <= controller.fleeDistance)
        {
            tooCloseTimer += Time.deltaTime;

            if (tooCloseTimer >= controller.tooCloseTimer)
            {
                controller.ChangeState(AIState.FLEE);
                return;
            }
        }
        else
        {
            // Reset timer se il player si allontana
            tooCloseTimer = 0f;
        }

        // Dentro shoot range -> passa a SHOOT
        if (distToPlayer <= controller.shootRange)
        {
            controller.ChangeState(AIState.SHOOT);
            return;
        }

        // Continua a inseguire
        if (controller.agent != null)
        {
            controller.agent.SetDestination(controller.vision.targetPosition);
        }
    }

    public override void Exit()
    {
        Debug.Log("[CHASE] Stop chasing.");
        tooCloseTimer = 0f;
    }
}

// ============================================================
// STATE: SHOOT
// ============================================================
public class EnemyState_Shoot : EnemyState
{
    private float tooCloseTimer = 0f;

    public EnemyState_Shoot(EnemyAIController controller) : base(controller) { }

    public override void Enter()
    {
        Debug.Log("[SHOOT] Start shooting!");

        if (controller.agent != null)
        {
            controller.agent.isStopped = true;
            controller.agent.velocity = Vector3.zero;
        }

        tooCloseTimer = 0f;
    }

    public override void Tick()
    {
        if (!controller.vision.hasTarget)
        {
            // Perso il target, vai in SEARCH
            if (controller.hasLastKnownPosition)
                controller.ChangeState(AIState.SEARCH);
            else
                controller.ChangeState(AIState.PATROL);
            return;
        }

        bool canSeePlayer = controller.vision.canSeePlayer;
        bool isInEngageRange = controller.vision.distanceToPlayer <= controller.engageDistance;
        bool isInShootRange = controller.vision.distanceToPlayer <= controller.shootRange;
        float distToPlayer = controller.vision.distanceToPlayer;

        // Non vede più il player
        if (!canSeePlayer)
        {
            if (controller.hasLastKnownPosition)
                controller.ChangeState(AIState.SEARCH);
            else
                controller.ChangeState(AIState.PATROL);
            return;
        }

        // Fuori dalla engage range
        if (!isInEngageRange)
        {
            controller.ChangeState(AIState.PATROL);
            return;
        }

        // Player troppo vicino -> accumula timer per FLEE
        if (distToPlayer <= controller.fleeDistance)
        {
            tooCloseTimer += Time.deltaTime;

            if (tooCloseTimer >= controller.tooCloseTimer)
            {
                controller.ChangeState(AIState.FLEE);
                return;
            }
        }
        else
        {
            tooCloseTimer = 0f;
        }

        // Fuori dalla shoot range ma dentro engage range -> CHASE
        if (!isInShootRange)
        {
            controller.ChangeState(AIState.CHASE);
            return;
        }

        // Ruota il corpo verso il player
        controller.FaceTowards(controller.vision.aimPoint);

        // Spara
        if (controller.shooter != null)
        {
            controller.shooter.TryShoot();
        }
    }

    public override void Exit()
    {
        Debug.Log("[SHOOT] Stop shooting.");
        tooCloseTimer = 0f;
    }
}

// ============================================================
// STATE: FLEE
// ============================================================
public class EnemyState_Flee : EnemyState
{
    private Vector3 fleeDestination;
    private bool hasValidDestination = false;
    private float recalculateTimer = 0f;
    private const float RECALCULATE_INTERVAL = 1f;

    public EnemyState_Flee(EnemyAIController controller) : base(controller) { }

    public override void Enter()
    {
        Debug.Log("[FLEE] Player too close! Fleeing!");

        if (controller.agent != null)
        {
            controller.agent.isStopped = false;
            controller.agent.speed = controller.fleeSpeed;
        }

        CalculateFleeDestination();
    }

    public override void Tick()
    {
        if (!controller.vision.hasTarget)
        {
            // Se non c'è più target, torna a PATROL
            controller.ChangeState(AIState.PATROL);
            return;
        }

        float distToPlayer = controller.vision.distanceToPlayer;

        // Raggiunta distanza di sicurezza
        if (distToPlayer >= controller.fleeSafeDistance)
        {
            // Torna a combattere
            if (controller.vision.canSeePlayer && distToPlayer <= controller.engageDistance)
            {
                if (distToPlayer <= controller.shootRange)
                    controller.ChangeState(AIState.SHOOT);
                else
                    controller.ChangeState(AIState.CHASE);
            }
            else
            {
                controller.ChangeState(AIState.PATROL);
            }
            return;
        }

        // Ricalcola destinazione periodicamente
        recalculateTimer += Time.deltaTime;
        if (recalculateTimer >= RECALCULATE_INTERVAL)
        {
            recalculateTimer = 0f;
            CalculateFleeDestination();
        }

        // Continua a fuggire
        if (hasValidDestination && controller.agent != null)
        {
            // Se vicino alla destinazione, ricalcola
            if (Vector3.Distance(controller.transform.position, fleeDestination) < 2f)
            {
                CalculateFleeDestination();
            }
        }
    }

    public override void Exit()
    {
        Debug.Log("[FLEE] Reached safe distance.");
        hasValidDestination = false;
    }

    private void CalculateFleeDestination()
    {
        if (controller.TryGetFleePosition(out Vector3 fleePos))
        {
            fleeDestination = fleePos;
            hasValidDestination = true;

            if (controller.agent != null)
            {
                controller.agent.SetDestination(fleeDestination);
            }

            Debug.Log("[FLEE] New flee destination calculated");
        }
        else
        {
            // Se non trova una posizione valida, muoviti comunque lontano
            Vector3 awayDirection = (controller.transform.position - controller.vision.targetPosition).normalized;
            fleeDestination = controller.transform.position + awayDirection * 10f;
            hasValidDestination = false;

            Debug.LogWarning("[FLEE] Could not find valid flee position on NavMesh");
        }
    }
}

// ============================================================
// STATE: SEARCH
// ============================================================
public class EnemyState_Search : EnemyState
{
    private float searchTimer = 0f;
    private bool reachedLastPosition = false;
    private float lookAroundTimer = 0f;
    private float currentRotation = 0f;

    public EnemyState_Search(EnemyAIController controller) : base(controller) { }

    public override void Enter()
    {
        Debug.Log("[SEARCH] Lost player, searching last known position...");

        if (controller.agent != null)
        {
            controller.agent.isStopped = false;
            controller.agent.speed = controller.searchSpeed;
        }

        searchTimer = 0f;
        reachedLastPosition = false;
        lookAroundTimer = 0f;
        currentRotation = 0f;

        // Vai all'ultima posizione nota
        if (controller.hasLastKnownPosition && controller.agent != null)
        {
            controller.agent.SetDestination(controller.lastKnownPlayerPosition);
        }
    }

    public override void Tick()
    {
        searchTimer += Time.deltaTime;

        // Timeout: torna a PATROL
        if (searchTimer >= controller.searchDuration)
        {
            controller.ChangeState(AIState.PATROL);
            return;
        }

        // Se rivede il player, torna in combat
        if (controller.vision.canSeePlayer)
        {
            float distToPlayer = controller.vision.distanceToPlayer;

            if (distToPlayer <= controller.engageDistance)
            {
                if (distToPlayer <= controller.shootRange)
                    controller.ChangeState(AIState.SHOOT);
                else
                    controller.ChangeState(AIState.CHASE);
                return;
            }
        }

        // Controlla se ha raggiunto l'ultima posizione nota
        if (!reachedLastPosition)
        {
            if (controller.agent != null && !controller.agent.pathPending)
            {
                float distToLastPos = Vector3.Distance(
                    controller.transform.position,
                    controller.lastKnownPlayerPosition
                );

                if (distToLastPos <= 2f)
                {
                    reachedLastPosition = true;

                    if (controller.agent != null)
                    {
                        controller.agent.isStopped = true;
                        controller.agent.velocity = Vector3.zero;
                    }

                    Debug.Log("[SEARCH] Reached last known position, looking around...");
                }
            }
        }
        else
        {
            // Look around
            lookAroundTimer += Time.deltaTime;

            if (lookAroundTimer < controller.lookAroundDuration)
            {
                // Ruota di 360 gradi
                currentRotation += controller.lookAroundSpeed * Time.deltaTime;
                Vector3 lookDirection = Quaternion.Euler(0, currentRotation, 0) * Vector3.forward;
                controller.RotateTowards(lookDirection, controller.lookAroundSpeed);
            }
            else
            {
                // Finito il look around, torna a PATROL
                controller.ChangeState(AIState.PATROL);
            }
        }
    }

    public override void Exit()
    {
        Debug.Log("[SEARCH] Ending search.");
    }
}

// ============================================================
// STATE: DEAD
// ============================================================
public class EnemyState_Dead : EnemyState
{
    public EnemyState_Dead(EnemyAIController controller) : base(controller) { }

    public override void Enter()
    {
        Debug.Log("[DEAD] Enemy died!");

        controller.enabled = false;

        if (controller.agent != null)
        {
            controller.agent.isStopped = true;
            controller.agent.enabled = false;
        }
    }
}