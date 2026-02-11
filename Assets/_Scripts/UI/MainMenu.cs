using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button startGameAsyncButton; // optional

    [Header("Scene")]
    [SerializeField] private int gameSceneBuildIndex = 1; // "01" in Build Settings

    private void Awake()
    {
        if (startGameButton == null)
        {
            Debug.LogError("[MainMenu] StartGameButton reference is missing.");
            enabled = false;
            return;
        }

        startGameButton.onClick.AddListener(LoadGameSingleSync);

        if (startGameAsyncButton != null)
            startGameAsyncButton.onClick.AddListener(LoadGameSingleAsync);
    }

    private void OnDestroy()
    {
        if (startGameButton != null)
            startGameButton.onClick.RemoveListener(LoadGameSingleSync);

        if (startGameAsyncButton != null)
            startGameAsyncButton.onClick.RemoveListener(LoadGameSingleAsync);
    }

    // --- Sync (Single) ---
    private void LoadGameSingleSync()
    {
        if (SceneChanger.Instance == null)
        {
            Debug.LogError("[MainMenu] SceneChangeManager.Instance is null. Add it to the starting scene.");
            return;
        }

        SceneChanger.Instance.LoadSingle(gameSceneBuildIndex);
    }

    // --- Async (Single) ---
    private void LoadGameSingleAsync()
    {
        if (SceneChanger.Instance == null)
        {
            Debug.LogError("[MainMenu] SceneChangeManager.Instance is null. Add it to the starting scene.");
            return;
        }

        SceneChanger.Instance.LoadSingleAsync(
            gameSceneBuildIndex,
            onProgress: p => Debug.Log($"[MainMenu] Loading game scene... {p:0.00}"),
            onComplete: () => Debug.Log("[MainMenu] Game scene loaded!")
        );
    }
}
