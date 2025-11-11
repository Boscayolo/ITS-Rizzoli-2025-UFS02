using UnityEngine;
using UnityEngine.UI;

public class Pause : MonoBehaviour
{
    [SerializeField] GameObject pausePanel;
    [SerializeField] Button resumeButton;

    public void EnablePause()
    {
        pausePanel.SetActive(true);
        Utilities.SetCursorLocked(true);

        Time.timeScale = 0f;

        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(DisablePause);
        }
    }

    public void DisablePause()
    {
        pausePanel.SetActive(false);
        Utilities.SetCursorLocked(false);

        Time.timeScale = 1f;

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(DisablePause);
        }
    }

    public void SetPauseEnabled(bool enabled)
    {
        pausePanel.SetActive(enabled);
        Utilities.SetCursorLocked(enabled);
    }
}
