using UnityEngine;
using UnityEngine.SceneManagement;

public class SoccerManager : MonoBehaviour
{
    public static SoccerManager Instance;

    [SerializeField] string ballTag = "Ball";
    public string BallTag => ballTag;
    [SerializeField] string ballSpawnTag = "BallSpawn";
    public string BallSpawnTag => ballSpawnTag;

    [SerializeField] int points = 0;
    [SerializeField] int pointsToNextGame = 5;

    GameObject ballObject;
    GameObject ballSpawnPoint;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else if(Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += RefreshLevelReferences;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= RefreshLevelReferences;
    }

    private void RefreshLevelReferences(Scene scene, LoadSceneMode loadSceneMode)
    {
        ballObject = GameObject.FindGameObjectWithTag(ballTag);
        ballSpawnPoint = GameObject.FindGameObjectWithTag(ballSpawnTag);

        ResetBall();
    }

    public void ScorePoints(int _points)
    {
        points += _points;
        Debug.Log($"Scored: {_points}! Total points: {points}");

        if (points >= pointsToNextGame) ResetGame();
    }

    private void ResetGame()
    {
        points = 0;
        ResetBall();
        Debug.Log("Game reset");
    }

    public void ResetBall()
    {
        if (ballObject != null && ballSpawnPoint != null)
        {
            ballObject.transform.position = ballSpawnPoint.transform.position;

            Rigidbody ballRb = ballObject.GetComponent<Rigidbody>();

            ballRb.linearVelocity = Vector3.zero;
            ballRb.angularVelocity = Vector3.zero;
        }
    }

}
