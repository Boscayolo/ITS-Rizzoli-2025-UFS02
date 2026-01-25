using UnityEngine;

public enum AIState { IDLE, SHOOT, DEAD }

/// <summary>
/// Base class for all enemy states
/// </summary>
public abstract class EnemyState
{
    protected EnemyAIController controller;

    protected EnemyState(EnemyAIController controller)
    {
        this.controller = controller;
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
        bool isInRange = controller.vision.distanceToPlayer <= controller.shootRange;

        if (canSeePlayer && isInRange)
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
        bool isInRange = controller.vision.distanceToPlayer <= controller.shootRange;

        if (!canSeePlayer || !isInRange)
        {
            controller.ChangeState(AIState.IDLE);
            return;
        }

        // Face the player
        controller.FaceTowards(controller.vision.aimPoint);

        // Shoot
        if (controller.shooter != null)
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