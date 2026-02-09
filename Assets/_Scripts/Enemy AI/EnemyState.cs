using UnityEngine;

public enum AIState { IDLE, SHOOT, DEAD }

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
    }

    public override void Tick()
    {
        bool canSeePlayer = controller.vision.canSeePlayer;
        bool isInEngageRange = controller.vision.distanceToPlayer <= controller.engageDistance;

        // Entra in SHOOT se può vedere il player ED è dentro la distanza di ingaggio
        if (canSeePlayer && isInEngageRange)
        {
            controller.ChangeState(AIState.SHOOT);
        }
    }
}

// ============================================================
// STATE: SHOOT
// ============================================================
public class EnemyState_Shoot : EnemyState
{
    public EnemyState_Shoot(EnemyAIController controller) : base(controller) { }

    public override void Enter()
    {
        Debug.Log("[SHOOT] Start shooting!");
    }

    public override void Tick()
    {
        if (!controller.vision.hasTarget)
        {
            controller.ChangeState(AIState.IDLE);
            return;
        }

        bool canSeePlayer = controller.vision.canSeePlayer;
        bool isInEngageRange = controller.vision.distanceToPlayer <= controller.engageDistance;
        bool isInShootRange = controller.vision.distanceToPlayer <= controller.shootRange;

        // Esci da SHOOT se non vedi più il player o è fuori dalla distanza di ingaggio
        if (!canSeePlayer || !isInEngageRange)
        {
            controller.ChangeState(AIState.IDLE);
            return;
        }

        // Ruota il corpo verso il player
        controller.FaceTowards(controller.vision.aimPoint);

        // Spara SOLO se è dentro la shoot range (più corta dell'engage range)
        if (isInShootRange && controller.shooter != null)
        {
            controller.shooter.TryShoot();
        }
    }

    public override void Exit()
    {
        Debug.Log("[SHOOT] Stop shooting.");
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
    }
}