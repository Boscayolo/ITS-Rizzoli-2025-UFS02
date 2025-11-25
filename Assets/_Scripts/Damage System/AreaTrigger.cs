using System.Collections.Generic;
using UnityEngine;

public enum AreaEffectType { HEAL, DAMAGE }
public enum TriggerType { ON_TRIGGER_ENTER, ON_TRIGGER_EXIT, ON_TRIGGER_STAY }

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class AreaTrigger : MonoBehaviour
{
    [Header("Mode")]
    [SerializeField] private AreaEffectType areaType = AreaEffectType.DAMAGE;
    [SerializeField] private TriggerType triggerType = TriggerType.ON_TRIGGER_ENTER;

    [Header("Effect")]
    [SerializeField] private int amount = 10;

    [Tooltip("Se attivo, applica l'effetto solo agli oggetti con Health.Team = Player.")]
    [SerializeField] private bool onlyAffectPlayer = true;

    [Header("ON_TRIGGER_STAY settings")]
    [Tooltip("Intervallo (in secondi) tra un'applicazione e la successiva quando TriggerType = ON_TRIGGER_STAY.")]
    [Min(0f)]
    [SerializeField] private float tickInterval = 0.5f;

    // serve solo per ON_TRIGGER_STAY (per non applicare ogni frame)
    private readonly Dictionary<Health, float> _nextTickTime = new();

    private void Reset()
    {
        // promemoria: deve essere trigger per funzionare
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnValidate()
    {
        // evita casi degeneri
        if (amount < 0) amount = 0;
        if (tickInterval < 0f) tickInterval = 0f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerType != TriggerType.ON_TRIGGER_ENTER) return;

        if (TryGetValidHealth(other, out var health))
            ApplyOnce(health);
    }

    private void OnTriggerExit(Collider other)
    {
        // pulizia per stay
        if (triggerType == TriggerType.ON_TRIGGER_STAY)
        {
            if (other.TryGetComponent<Health>(out var h))
                _nextTickTime.Remove(h);
        }

        if (triggerType != TriggerType.ON_TRIGGER_EXIT) return;

        if (TryGetValidHealth(other, out var health))
            ApplyOnce(health);
    }

    private void OnTriggerStay(Collider other)
    {
        if (triggerType != TriggerType.ON_TRIGGER_STAY) return;

        if (!TryGetValidHealth(other, out var health))
            return;

        float now = Time.time;

        if (!_nextTickTime.TryGetValue(health, out float next))
            next = 0f;

        if (now < next)
            return;

        ApplyOnce(health);

        float interval = tickInterval <= 0f ? 0f : tickInterval;
        _nextTickTime[health] = now + interval;
    }

    private bool TryGetValidHealth(Collider other, out Health health)
    {
        health = null;

        // Prova sul collider stesso…
        if (!other.TryGetComponent(out health))
        {
            // …oppure sul parent (comodo se il collider è su un child)
            health = other.GetComponentInParent<Health>();
        }

        if (health == null)
            return false;

        if (onlyAffectPlayer && health.Team != Team.Player)
            return false;

        return true;
    }

    private void ApplyOnce(Health health)
    {
        if (amount <= 0) return;

        if (areaType == AreaEffectType.DAMAGE)
            health.TakeDamage(amount);
        else
            health.Heal(amount);
    }
}
