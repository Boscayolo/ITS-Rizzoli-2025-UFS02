using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody rb;
    private float speed;
    private Team shooterTeam;
    private int damage;
    private bool initialized;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Inizializza il proiettile con i parametri necessari
    /// </summary>
    public void Initialize(float bulletSpeed, Team team, int bulletDamage, Collider shooterCollider = null)
    {
        speed = bulletSpeed;
        shooterTeam = team;
        damage = bulletDamage;
        initialized = true;

        // Ignora il collider dello sparatore per evitare il rinculo
        if (shooterCollider != null && TryGetComponent<Collider>(out Collider bulletCollider))
        {
            Physics.IgnoreCollision(bulletCollider, shooterCollider);
            Debug.Log($"[Bullet] Ignorando collider dello sparatore");
        }

        // Applica la velocità al rigidbody
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * speed;
        }

        Debug.Log($"Bullet initialized - Speed: {speed}, Team: {team}, Damage: {damage}");
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (!initialized) return;

        // Se il proiettile non è inizializzato non fa nulla
        Health healthHit = collision.GetComponent<Health>();

        if (healthHit != null && healthHit.Team != shooterTeam)
        {
            // Infligge danno al nemico/giocatore
            Debug.Log($"{collision.gameObject.name} è stato colpito dal proiettile!");
            healthHit.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Se colpisce qualcos'altro (muri, alleati, ecc) il proiettile viene distrutto
        if (collision.CompareTag("Wall") || collision.gameObject.layer == LayerMask.NameToLayer("Environment"))
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!initialized) return;

        Health healthHit = collision.gameObject.GetComponent<Health>();

        if (healthHit != null && healthHit.Team != shooterTeam)
        {
            Debug.Log($"{collision.gameObject.name} è stato colpito dal proiettile!");
            healthHit.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Distrugge il proiettile se colpisce qualcos'altro
        Destroy(gameObject);
    }

    /// <summary>
    /// Distrugge il proiettile dopo un certo tempo (fallback per proiettili persi)
    /// </summary>
    public void SetDestroyTimer(float timeInSeconds)
    {
        Destroy(gameObject, timeInSeconds);
    }
}