using UnityEngine;

/// <summary>
/// Enemy AI Controller - Simplified Version
/// </summary>
public class EnemyAIController : MonoBehaviour
{
    [Header("References")]
    public EnemyShooter shooter;

    [Header("Combat Settings")]
    public float shootRange = 18f;

    // Current state
    public AIState currentState = AIState.IDLE;

    // Components (public so EnemyState can access them)
    public VisionScanner vision;
    public Health health;

    // State object
    private EnemyState currentStateObject;

    void Start()
    {
        vision = GetComponent<VisionScanner>();
        health = GetComponent<Health>();

        if (shooter == null)
            shooter = GetComponentInChildren<EnemyShooter>();

        health.OnDied += OnDeath;

        ChangeState(AIState.IDLE);
    }

    void Update()
    {
        if (currentState == AIState.DEAD)
            return;

        currentStateObject?.Tick();
    }

    public void ChangeState(AIState newState)
    {
        currentStateObject?.Exit();

        currentState = newState;

        currentStateObject = newState switch
        {
            AIState.IDLE => new EnemyState_Idle(this),
            AIState.SHOOT => new EnemyState_Shoot(this),
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

    void OnDeath(Health health)
    {
        ChangeState(AIState.DEAD);
    }

    void OnDestroy()
    {
        if (health != null)
            health.OnDied -= OnDeath;
    }
}