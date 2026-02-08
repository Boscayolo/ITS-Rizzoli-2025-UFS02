using UnityEngine;

/// <summary>
/// Gestisce un tracer bullet con TrailRenderer.
/// Si auto-distrugge dopo un tempo configurabile.
/// </summary>
[RequireComponent(typeof(TrailRenderer))]
public class BulletTracer : MonoBehaviour
{
    [SerializeField] private TrailRenderer trailRenderer;
    private float lifetime = 0.2f;
    private float spawnTime;

    private Vector3 startPos;
    private Vector3 endPos;
    private float travelTime = 0.05f; // Tempo per viaggiare da start a end (molto veloce)
    private bool isMoving = false;

    /// <summary>
    /// Inizializza il tracer con posizione iniziale, finale e durata.
    /// </summary>
    public void Initialize(Vector3 startPosition, Vector3 endPosition, float tracerLifetime, Color color)
    {
        startPos = startPosition;
        endPos = endPosition;
        transform.position = startPos;

        lifetime = tracerLifetime;
        spawnTime = Time.time;
        isMoving = true;

        // Configura il colore del trail
        if (trailRenderer != null)
        {
            trailRenderer.startColor = color;
            trailRenderer.endColor = new Color(color.r, color.g, color.b, 0f); // Trasparente alla fine
        }
    }

    private void Update()
    {
        // Anima il movimento da start a end
        if (isMoving)
        {
            float elapsed = Time.time - spawnTime;
            float t = Mathf.Clamp01(elapsed / travelTime);

            transform.position = Vector3.Lerp(startPos, endPos, t);

            if (t >= 1f)
            {
                isMoving = false;
            }
        }

        // Auto-distruzione dopo il lifetime
        if (Time.time >= spawnTime + lifetime)
        {
            gameObject.SetActive(false);
        }
    }
}