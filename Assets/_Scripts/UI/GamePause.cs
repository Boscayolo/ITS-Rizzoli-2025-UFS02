using System.ComponentModel;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;


public class GamePause : MonoBehaviour
{
    [SerializeField] GameObject pausePanel;
    [SerializeField] Button unpauseButton;
    [SerializeField, Unity.Collections.ReadOnly]bool paused;


    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Pause();
        }
    }

    void Pause()
    {
        if(!paused) paused = true;

        pausePanel.SetActive(true);
        unpauseButton.onClick.AddListener(Unpause);

        Time.timeScale = 0f;
        Utilities.SetCursorLocked(false);
    }

    public void Unpause()
    {
        if (paused) paused = false;

        pausePanel.SetActive(false);
        unpauseButton.onClick.RemoveListener(Unpause);

        Time.timeScale = 1f;
        Utilities.SetCursorLocked(true);
    }

    public void TogglePause()
    {
        paused = !paused;

        pausePanel.SetActive(paused);
        unpauseButton.onClick.AddListener(TogglePause);

        Time.timeScale = paused ? 0f : 1f;
        Utilities.SetCursorLocked(!paused);
    }

    public void SetPaused(bool paused)
    {
        pausePanel.SetActive(paused);
        Time.timeScale = paused ? 0f : 1f;
        Utilities.SetCursorLocked(!paused);
    }
}
