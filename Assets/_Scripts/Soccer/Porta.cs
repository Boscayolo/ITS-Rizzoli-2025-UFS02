using System.Collections;
using UnityEngine;

public class Porta : MonoBehaviour
{
    public static Coroutine ballRespawnCrt;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(SoccerManager.Instance?.BallTag))
        {
            Debug.Log("Punto!");
            SoccerManager.Instance?.ScorePoints(1);

            if(ballRespawnCrt == null) ballRespawnCrt = StartCoroutine(BallResetWait());
        }
    }

    IEnumerator BallResetWait()
    {
        yield return new WaitForSeconds(1f);

        SoccerManager.Instance?.ResetBall();
        ballRespawnCrt = null;
    }
}
