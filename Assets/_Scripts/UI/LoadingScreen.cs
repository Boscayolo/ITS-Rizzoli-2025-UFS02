using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance;

    [SerializeField] GameObject loadingPanel;
    [SerializeField] Image loadingBar;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        loadingPanel.SetActive(false);

        if(SceneChanger.Instance != null)
        {
            SceneChanger.Instance.OnLoadingStarted += EnableLoadingScreen;
            SceneChanger.Instance.OnLoadingCompleted += DisableLoadingScreen;
        }
    }

    private void OnDestroy()
    {
        if (SceneChanger.Instance != null)
        {
            SceneChanger.Instance.OnLoadingStarted -= EnableLoadingScreen;
            SceneChanger.Instance.OnLoadingCompleted -= DisableLoadingScreen;
        }
    }

    public void EnableLoadingScreen()
    {
        loadingPanel.SetActive(true);
        loadingBar.fillAmount = 0f;
        SceneChanger.Instance.OnProgress += UpdateLoadingBar;        
    }

    public void DisableLoadingScreen()
    {
        loadingPanel.SetActive(false);
        SceneChanger.Instance.OnProgress -= UpdateLoadingBar;

        loadingBar.fillAmount = 0f;
    }

    public void UpdateLoadingBar(float progress)
    {
        loadingBar.fillAmount = progress;
    }

}
