using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField] EntityDataSO m_Entity;
    [SerializeField] Health m_Health;
    [SerializeField] EnemyAIController m_Controller;
    public Health EntityHealth => m_Health;
    [SerializeField] Shooter m_Shooter;
    [SerializeField] MeshRenderer m_Renderer;

    private void Start()
    {
        if (m_Entity == null)
        {
            Debug.Log("ENTITY DATA IS MISSING, ABORTING INITIALIZATION");
            return;
        }

        if (m_Health == null)
        {
            m_Health = GetComponentInChildren<Health>();
        }

        if (m_Shooter == null)
        {
            m_Shooter = GetComponentInChildren<Shooter>();
        }

        if (m_Controller == null)
        {
            m_Controller = GetComponent<EnemyAIController>();
        }

        InitializeEntity(m_Entity);
    }

    public void InitializeEntity(EntityDataSO entity)
    {
        m_Entity = entity;

        // Inizializza Health
        if (m_Health != null)
        {
            m_Health.SetHealth(m_Entity.maxEntityHealth, m_Entity.startingEntityHealth);

            if(m_Health.Team == Team.Player)
            {
                GameManager.Instance?.RegisterPlayer(this);
            }
        }

        // Equipaggia arma
        if (m_Shooter != null)
        {
            m_Shooter.EquipWeapon(m_Entity.weapon);
        }

        // Configura engage distance tramite AIController (non più Shooter)
        if (m_Controller != null)
        {
            m_Controller.SetEngageDistance(m_Entity.engageDistance);
        }

        // Aspetto visivo
        if (m_Renderer != null)
            m_Renderer.material.color = m_Entity.entityColor;

        transform.localScale = Vector3.one * m_Entity.size;
    }
}