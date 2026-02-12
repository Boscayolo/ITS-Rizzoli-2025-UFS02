using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public static SceneChanger Instance { get; private set; }

    /// switch che utilizziamo quando è in corso una operazione di load/unload.
    public bool IsBusy => _opsInFlight > 0;

    /// <summary>Eventi utili per UI/loading screen/logging.</summary>
    public event Action<string> OnOperationStarted;
    public event Action<string> OnOperationCompleted;

    public event Action OnLoadingStarted;
    public event Action OnLoadingCompleted;
    public event Action<float> OnProgress;

    [Header("Options")]
    [SerializeField] private bool dontDestroyOnLoad = true;

    private int _opsInFlight;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);
    }

    // =========================================================
    // 1) LOAD SCENE "CLASSICO" (SYNC)
    // =========================================================

    public void LoadSingle(int buildIndex)
        => LoadSync(buildIndex, null, LoadSceneMode.Single);

    public void LoadSingle(string sceneName)
        => LoadSync(null, sceneName, LoadSceneMode.Single);

    // =========================================================
    // 2) LOAD ADDITIVE (SYNC) 
    // =========================================================

    public void LoadAdditive(int buildIndex)
        => LoadSync(buildIndex, null, LoadSceneMode.Additive);

    public void LoadAdditive(string sceneName)
        => LoadSync(null, sceneName, LoadSceneMode.Additive);

    // =========================================================
    // 3) UNLOAD (SYNC) — raramente usato, ma esiste (Unity scarica scene)
    // Nota: in pratica di solito si usa l'Unload async.
    // =========================================================

    public void UnloadAdditive(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("[SceneChangeManager] UnloadAdditive: sceneName vuoto.");
            return;
        }

        if (!SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            Debug.LogWarning($"[SceneChangeManager] UnloadAdditive: '{sceneName}' non è caricata.");
            return;
        }

        // Unity non ha un vero Unload "sync": chiamata async consigliata.
        StartCoroutine(UnloadRoutine(sceneName, onProgress: null, onComplete: null));
    }

    // =========================================================
    // 4) LOAD ASYNC — consigliato per scene pesanti / UX migliore
    // =========================================================

    public Coroutine LoadSingleAsync(int buildIndex, Action<float> onProgress = null, Action onComplete = null)
        => LoadAsync(buildIndex, null, LoadSceneMode.Single, onProgress, onComplete);

    public Coroutine LoadSingleAsync(string sceneName, Action<float> onProgress = null, Action onComplete = null)
        => LoadAsync(null, sceneName, LoadSceneMode.Single, onProgress, onComplete);

    public Coroutine LoadAdditiveAsync(int buildIndex, Action<float> onProgress = null, Action onComplete = null)
        => LoadAsync(buildIndex, null, LoadSceneMode.Additive, onProgress, onComplete);

    public Coroutine LoadAdditiveAsync(string sceneName, Action<float> onProgress = null, Action onComplete = null)
        => LoadAsync(null, sceneName, LoadSceneMode.Additive, onProgress, onComplete);

    // =========================================================
    // 5) UNLOAD ASYNC — il modo “giusto” di scaricare scene additive
    // =========================================================

    public Coroutine UnloadAdditiveAsync(string sceneName, Action<float> onProgress = null, Action onComplete = null)
        => StartCoroutine(UnloadRoutine(sceneName, onProgress, onComplete));

    // =========================================================
    // Utility: Set Active Scene (molto utile con Additive)
    // =========================================================

    public bool SetActiveScene(string sceneName)
    {
        var s = SceneManager.GetSceneByName(sceneName);
        if (!s.isLoaded)
        {
            Debug.LogWarning($"[SceneChangeManager] SetActiveScene: '{sceneName}' non è caricata.");
            return false;
        }

        SceneManager.SetActiveScene(s);
        return true;
    }

    // =========================================================
    // Internals
    // =========================================================

    private void LoadSync(int? buildIndex, string sceneName, LoadSceneMode mode)
    {
        if (IsBusy)
        {
            Debug.LogWarning("[SceneChangeManager] LoadSync ignorato: operazione già in corso.");
            return;
        }

        if (!ValidateTarget(buildIndex, sceneName))
            return;

        string label = sceneName ?? $"Index {buildIndex.Value}";

        OnOperationStarted?.Invoke($"LoadSync {mode}: {label}");
        OnLoadingStarted?.Invoke();

        if (sceneName != null)
            SceneManager.LoadScene(sceneName, mode);
        else
            SceneManager.LoadScene(buildIndex.Value, mode);

        OnOperationCompleted?.Invoke($"LoadSync {mode}: {label}");
        OnLoadingCompleted?.Invoke();
    }

    private Coroutine LoadAsync(int? buildIndex, string sceneName, LoadSceneMode mode,
                                Action<float> onProgress, Action onComplete)
    {
        if (IsBusy)
        {
            Debug.LogWarning("[SceneChangeManager] LoadAsync ignorato: operazione già in corso.");
            return null;
        }

        if (!ValidateTarget(buildIndex, sceneName))
            return null;

        return StartCoroutine(LoadRoutine(buildIndex, sceneName, mode, onProgress, onComplete));
    }

    private IEnumerator LoadRoutine(int? buildIndex, string sceneName, LoadSceneMode mode,
                                    Action<float> onProgress, Action onComplete)
    {
        _opsInFlight++;
        string label = sceneName ?? $"Index {buildIndex.Value}";
        OnOperationStarted?.Invoke($"LoadAsync {mode}: {label}");
        OnLoadingStarted?.Invoke();

        AsyncOperation op = (sceneName != null)
            ? SceneManager.LoadSceneAsync(sceneName, mode)
            : SceneManager.LoadSceneAsync(buildIndex.Value, mode);

        if (op == null)
        {
            Debug.LogError("[SceneChangeManager] LoadSceneAsync ha restituito null.");
            _opsInFlight--;
            yield break;
        }

        // Nota: op.progress arriva tipicamente a ~0.9 finché non è pronto.
        while (!op.isDone)
        {
            onProgress?.Invoke(op.progress);
            OnProgress?.Invoke(op.progress);
            yield return null;
        }

        OnOperationCompleted?.Invoke($"LoadAsync {mode}: {label}");
        OnLoadingCompleted?.Invoke();
        onComplete?.Invoke();

        _opsInFlight--;
    }

    private IEnumerator UnloadRoutine(string sceneName, Action<float> onProgress, Action onComplete)
    {
        if (IsBusy)
        {
            Debug.LogWarning("[SceneChangeManager] Unload ignorato: operazione già in corso.");
            yield break;
        }

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("[SceneChangeManager] Unload: sceneName vuoto.");
            yield break;
        }

        var s = SceneManager.GetSceneByName(sceneName);
        if (!s.isLoaded)
        {
            Debug.LogWarning($"[SceneChangeManager] Unload: '{sceneName}' non è caricata.");
            yield break;
        }

        _opsInFlight++;
        OnOperationStarted?.Invoke($"UnloadAsync: {sceneName}");
        OnLoadingStarted?.Invoke();

        AsyncOperation op = SceneManager.UnloadSceneAsync(sceneName);
        if (op == null)
        {
            Debug.LogError("[SceneChangeManager] UnloadSceneAsync ha restituito null.");
            _opsInFlight--;
            yield break;
        }

        while (!op.isDone)
        {
            onProgress?.Invoke(op.progress);
            yield return null;
        }

        OnOperationCompleted?.Invoke($"UnloadAsync: {sceneName}");
        OnLoadingCompleted?.Invoke();
        onComplete?.Invoke();

        _opsInFlight--;
    }

    private bool ValidateTarget(int? buildIndex, string sceneName)
    {
        if (sceneName == null)
        {
            if (!buildIndex.HasValue)
            {
                Debug.LogError("[SceneChangeManager] Target scena non valido (né index né name).");
                return false;
            }

            int idx = buildIndex.Value;
            if (idx < 0 || idx >= SceneManager.sceneCountInBuildSettings)
            {
                Debug.LogError($"[SceneChangeManager] Build index non valido: {idx}. Controlla Build Settings.");
                return false;
            }
            return true;
        }

        // controllo “soft” che la scena esista in Build Settings
        if (!IsSceneInBuildSettings(sceneName))
        {
            Debug.LogError($"[SceneChangeManager] La scena '{sceneName}' non risulta nelle Build Settings.");
            return false;
        }

        return true;
    }

    private bool IsSceneInBuildSettings(string sceneName)
    {
        int count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (string.Equals(name, sceneName, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
