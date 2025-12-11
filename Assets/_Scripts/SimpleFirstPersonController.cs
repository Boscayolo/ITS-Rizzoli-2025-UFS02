using UnityEngine;

public class SimplePlayerController : MonoBehaviour
{
    [Header("Movimento")]
    public float speed = 5f;
    public float sprintMultiplier = 1.5f;

    [Header("Camera")]
    public Transform cameraTransform;
    public float lookSensitivity = 3f;

    private bool isSprinting;
    private float xRotation = 0f;

    void Start()
    {
        Utilities.SetCursorLocked(true);
    }

    void Update()
    {
        Sprint();
        Move();

        Look();
    }

    // Movimento base con WASD, senza fisica
    void Move()
    {
        float horizontal = Input.GetAxis("Horizontal"); // A/D
        float vertical = Input.GetAxis("Vertical");   // W/S

        Vector3 inputDir = new Vector3(horizontal, 0f, vertical);

        if (inputDir.sqrMagnitude > 1f)
        {
            inputDir.Normalize(); //importante normalizzare la velocità, poichè adittivamente in diagonale ci si muoverebbe più veloci
        }

        float currentSpeed = 0f;

        if(isSprinting)
        {
            currentSpeed = speed * sprintMultiplier;
        }
        else
        {
            currentSpeed = speed;
        }
        
        //float currentSpeed = speed * (isSprinting ? sprintMultiplier : 1f); ///utilizzando gli operatori in line

        // Direzione relativa al facing del player
        Vector3 move = transform.TransformDirection(inputDir) * currentSpeed * Time.deltaTime; //deltaTime ci permette di muoverci in maniera indipendente dal frame rate
        transform.position += move;
    }

    void Sprint()
    {
        isSprinting = Input.GetKey(KeyCode.LeftShift); //se leftshift è premuto, allora il giocatore sta sprintando
    }

    // Rotazione player + camera con il mouse
    void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity * Time.deltaTime;

        // Rotazione orizzontale del player
        transform.Rotate(Vector3.up * mouseX);

        // Rotazione verticale della camera
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }
}
