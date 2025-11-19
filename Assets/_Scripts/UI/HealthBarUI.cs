using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("Riferimenti")]
    [SerializeField] private Image fillImage;          // l'Image con fillAmount
    [SerializeField] private Health health;

    private void Awake()
    {
        // Se non assegnata, prova a prendere l'Image sullo stesso GameObject
        if (fillImage == null)
            fillImage = GetComponentInChildren<Image>();


        if (health == null)
        {
            Debug.LogWarning($"{nameof(HealthBarUI)} su {gameObject.name} non ha un target Health valido.");
            return;
        }

        // Iscrizione all'evento di cambio vita
        health.OnHealthChanged += HandleHealthChanged;

        // Inizializza la barra allo stato attuale
        HandleHealthChanged(health.CurrentHealth, health.MaxHealth);
    }

    private void OnDestroy()
    {
        if (health != null)
            health.OnHealthChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(float current, float max)
    {
        if (fillImage == null)
            return;

        float fill = (max > 0) ? current / max : 0f;
        fillImage.fillAmount = fill;
    }
}
