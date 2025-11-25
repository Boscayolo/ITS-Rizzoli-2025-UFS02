using System;
using UnityEngine;


public enum Team
{
    Neutral = 0,
    Player = 1,
    Enemy = 2
}

[DisallowMultipleComponent]
public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private Team team = Team.Neutral;
    [SerializeField] private bool destroyOnDeath = true;

    private int currentHealth;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public Team Team => team;

    /// Azione chiamata sul cambio di vita.
    public event Action<float, float> OnHealthChanged; //currentHealth, maxHealth

    /// Azione chiamata alla morte.
    public event Action<Health> OnDied;

    private bool _isDead;

    private void Awake()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        if (currentHealth == 0)
            currentHealth = maxHealth;

        _isDead = currentHealth <= 0;
        NotifyHealthChanged();
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || _isDead)
            return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        NotifyHealthChanged();

        if (currentHealth == 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || _isDead)
            return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        NotifyHealthChanged();
    }

    public void ResetHealth()
    {
        _isDead = false;
        currentHealth = maxHealth;
        NotifyHealthChanged();
    }

    private void NotifyHealthChanged()
    {
        Debug.Log($"VITA CORRENTE: {currentHealth}/{maxHealth}");
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        if (_isDead)
            return;

        _isDead = true;
        OnDied?.Invoke(this);

        if (destroyOnDeath)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
