using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : Shooter
{
    private PlayerInputActions _inputActions;
    private bool _isShootingHeld;

    #region Unity lifecycle

    protected override void Awake()
    {
        base.Awake(); // inizializza l'aimer dello Shooter

        _inputActions = new PlayerInputActions();

        // Se vuoi: "click singolo" = fire una volta su performed
        _inputActions.Player.Fire.performed += OnFirePerformed;

        // Se vuoi tenere premuto per auto-fire: usiamo anche canceled
        _inputActions.Player.Fire.started += OnFireStarted;
        _inputActions.Player.Fire.canceled += OnFireCanceled;
    }

    private void OnEnable()
    {
        _inputActions?.Enable();
    }

    private void OnDisable()
    {
        _inputActions?.Disable();
    }

    private void OnDestroy()
    {
        if (_inputActions != null)
        {
            _inputActions.Player.Fire.performed -= OnFirePerformed;
            _inputActions.Player.Fire.started -= OnFireStarted;
            _inputActions.Player.Fire.canceled -= OnFireCanceled;
        }
    }

    protected override void Update()
    {
        // Non chiamiamo base.Update() così ignoriamo l'autoFire dello Shooter base
        if (_isShootingHeld)
        {
            // Rispetta il fireRate definito in Shooter
            TryShoot();
        }
    }

    #endregion

    #region Input callbacks

    // chiamato quando il tasto/grilletto viene premuto (inizio pressione)
    private void OnFireStarted(InputAction.CallbackContext ctx)
    {
        _isShootingHeld = true;
    }

    // chiamato quando l'azione "Fire" viene registrata come "performed"
    // (per molti binding coincide con l'inizio pressione; puoi tenerlo come "colpo immediato")
    private void OnFirePerformed(InputAction.CallbackContext ctx)
    {
        // Primo colpo immediato
        TryShoot();
    }

    // chiamato quando il tasto/grilletto viene rilasciato
    private void OnFireCanceled(InputAction.CallbackContext ctx)
    {
        _isShootingHeld = false;
    }

    #endregion
}
