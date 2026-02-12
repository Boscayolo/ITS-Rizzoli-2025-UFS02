using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class EndGameUI : MonoBehaviour
{
    public static EndGameUI instance;

    [SerializeField] GameObject winLoseContainer;
    [SerializeField] GameObject winScreen;
    [SerializeField] GameObject loseScreen;
    [SerializeField] Button restartLevelButton;
    [SerializeField] Button mainMenuButton;

    private void Awake()
    {
        instance = this;

        winLoseContainer.SetActive(false);

        if (restartLevelButton) restartLevelButton.onClick.AddListener(() =>
        {
            SceneChanger.Instance?.LoadSingleAsync(SceneManager.GetActiveScene().buildIndex);
        });

        if (mainMenuButton) mainMenuButton.onClick.AddListener(() =>
        {
            SceneChanger.Instance?.LoadSingleAsync(0); //00 mainMenu buildIndex
        });
     }

    private void OnDestroy()
    {
        if (restartLevelButton) restartLevelButton.onClick.RemoveListener(() =>
        {
            SceneChanger.Instance?.LoadSingleAsync(SceneManager.GetActiveScene().buildIndex);
        });

        if (mainMenuButton) mainMenuButton.onClick.RemoveListener(() =>
        {
            SceneChanger.Instance?.LoadSingleAsync(0); //00 mainMenu buildIndex
        });
    }

    public void ShowWinLose(bool win)
    {
        Utilities.SetCursorLocked(false);

        winLoseContainer.SetActive(true);

        winScreen.SetActive(win);
        loseScreen.SetActive(!win);
    }


}
