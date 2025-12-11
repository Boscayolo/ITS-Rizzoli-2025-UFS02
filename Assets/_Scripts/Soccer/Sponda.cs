using UnityEngine;

public class Sponda : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.CompareTag(SoccerManager.Instance?.BallTag))
        {
            if (Porta.ballRespawnCrt != null) return;
            SoccerManager.Instance?.ResetBall();
        }
    }
}
