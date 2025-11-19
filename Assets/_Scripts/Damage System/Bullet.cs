using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private bool usePhysics = true;

    [Header("Debug")]
    [SerializeField] private bool drawDebugDirection = false;

    private Rigidbody _rb;
    private Vector3 _direction;
    private Team _ownerTeam;

    public int Damage => damage;
    public Team OwnerTeam => _ownerTeam;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        // Consigliato: collider come trigger
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnEnable()
    {
        if (lifeTime > 0f)
        {
            CancelInvoke(nameof(DisableSelf));
            Invoke(nameof(DisableSelf), lifeTime);
        }
    }

    //inizializzazione del proiettile
    public void Init(Vector3 direction, Team ownerTeam)
    {
        _direction = direction.normalized;
        _ownerTeam = ownerTeam;

        if (usePhysics && _rb != null)
        {
            _rb.isKinematic = false;
            _rb.linearVelocity = _direction * speed;
        }
    }

    private void Update()
    {
        if (!usePhysics)
        {
            transform.position += _direction * (speed * Time.deltaTime);
        }

        if (drawDebugDirection)
        {
            Debug.DrawRay(transform.position, _direction, Color.red);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ignora se colpisce se stesso
        if (other.gameObject == gameObject)
            return;

        // Controlla se l'altro ha un componente Health
        if (other.TryGetComponent(out Health health))
        {
            // Evita di colpire entit� della stessa squadra
            if (health.Team == _ownerTeam)
                return;

            // Applica il danno
            health.TakeDamage(damage);

            // Per ora distruggiamo il proiettile (in futuro: pooling)
            DisableSelf();
        }
        else
        {
            // Se vogliamo che il proiettile venga distrutto solo se tocca certe cose,
            // possiamo filtrare via tag/layer. Per ora lo distruggiamo sempre.
            DisableSelf();
        }
    }

    private void DisableSelf()
    {
        if (!gameObject.activeInHierarchy)
            return;

        // Se usi pooling, qui potresti fare: gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
