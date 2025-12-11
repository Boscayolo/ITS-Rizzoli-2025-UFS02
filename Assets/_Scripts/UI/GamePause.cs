using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
public class GamePause : MonoBehaviour
{
    private PlayerInputActions _playerInput;

    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button unpauseButton;
    [SerializeField] private bool startPaused = false;

    private bool paused;

    private void Awake()
    {
        _playerInput = new PlayerInputActions();

        _playerInput.Player.Pause.performed += OnPausePerformed;
        //_playerInput.Player.Pause.performed += (ctx) => TogglePause(); //equivalente alla riga superiore

        //if (unpauseButton != null)
        //    unpauseButton.onClick.AddListener(TogglePause);

        SetPaused(startPaused);
    }

    private void OnEnable()
    {
        _playerInput?.Enable();
    }
    private void OnDisable()
    { 
        _playerInput?.Disable();
    }

    private void OnDestroy()
    {
        if (_playerInput != null)
            _playerInput.Player.Pause.performed -= OnPausePerformed;
        //_playerInput.Player.Pause.performed -= (ctx) => TogglePause();

        //if (unpauseButton != null)
        //    unpauseButton.onClick.RemoveListener(TogglePause);

        _playerInput?.Dispose();
    }

    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        TogglePause();
    }

    public void TogglePause() => SetPaused(!paused);

    public void SetPaused(bool value)
    {
        paused = value;

        if (pausePanel != null)
            pausePanel.SetActive(paused);

        Time.timeScale = paused ? 0f : 1f;
        Utilities.SetCursorLocked(!paused);
    }

    public void Pause()
    {
        if (pausePanel != null)
            pausePanel.SetActive(true);

        Time.timeScale = 0f;
        Utilities.SetCursorLocked(true);
    }

    public void Unpause()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = 1f;
        Utilities.SetCursorLocked(false);
    }
}
