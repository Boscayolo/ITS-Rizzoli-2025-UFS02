using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float crouchSpeedMultiplier = 0.5f;

    [Header("View")]
    [SerializeField] Transform cameraRoot;
    [SerializeField] float mouseSensitivity = 25f;
    [SerializeField] float maxLookUp = 80f;
    [SerializeField] float maxLookDown = -80f;

    [Header("Jump & Gravity")]
    [SerializeField] float jumpHeight = 1.5f;
    [SerializeField] float gravity = -9.81f;

    [Header("Crouch")]
    [SerializeField] float crouchHeight = 1.0f;

    CharacterController controller;
    PlayerInputActions inputActions;

    Vector2 moveInput;     // direzione WASD
    Vector2 lookInput;     // input per camera
    float verticalVelocity;
    float xRotation;       // rotazione verticale camera

    float originalHeight;
    Vector3 originalCenter;
    bool isCrouching;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        originalHeight = controller.height;
        originalCenter = controller.center;

        // Istanzio la classe generata dall'Input System
        inputActions = new PlayerInputActions();

        // --- MOVE ---
        inputActions.Player.Move.performed += ctx => //ctx = CallbackContext
        {
            moveInput = ctx.ReadValue<Vector2>();
        };
        inputActions.Player.Move.canceled += ctx =>
        {
            moveInput = Vector2.zero;
        };

        // --- LOOK ---
        inputActions.Player.Look.performed += ctx =>
        {
            lookInput = ctx.ReadValue<Vector2>();
        };
        inputActions.Player.Look.canceled += ctx =>
        {
            lookInput = Vector2.zero;
        };

        // --- JUMP ---
        inputActions.Player.Jump.performed += ctx =>
        {
            TryJump();
        };

        // --- CROUCH (tenere premuto) ---
        inputActions.Player.Crouch.performed += ctx =>
        {
            StartCrouch();
        };
        inputActions.Player.Crouch.canceled += ctx =>
        {
            StopCrouch();
        };
    }

    void OnEnable()
    {
        inputActions.Enable();
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

    void Update()
    {
        Movement();
        Look();
    }


    void Look()
    {
        // moltiplico per deltaTime per rendere la sensibilità indipendente dal framerate
        Vector2 look = lookInput * mouseSensitivity * Time.deltaTime;

        // Rotazione orizzontale del player (yaw)
        transform.Rotate(Vector3.up * look.x);

        // Rotazione verticale della camera (pitch)
        xRotation -= look.y;
        xRotation = Mathf.Clamp(xRotation, maxLookDown, maxLookUp);

        if (cameraRoot != null)
        {
            cameraRoot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
        else
        {
            Debug.LogWarning("FirstPersonController: cameraRoot non assegnato.");
        }
    }

    void Movement()
    {
        bool isGrounded = controller.isGrounded;

        // reset leggero per tenerlo attaccato al suolo
        if (isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        // direzione locale (in base all'orientamento del player)
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        // velocità: più lenta se crouch
        float currentSpeed = isCrouching ? moveSpeed * crouchSpeedMultiplier : moveSpeed;
        move *= currentSpeed;

        // gravità
        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);
    }

    void TryJump()
    {
        // può saltare solo se è a terra e non è accovacciato
        if (controller.isGrounded && !isCrouching)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    void StartCrouch()
    {
        if (isCrouching) return;

        isCrouching = true;
        controller.height = crouchHeight;

        // abbassa il centro per non farlo "fluttuare"
        controller.center = new Vector3(
            originalCenter.x,
            crouchHeight / 2f,
            originalCenter.z
        );
    }

    void StopCrouch()
    {
        if (!isCrouching) return;

        isCrouching = false;
        controller.height = originalHeight;
        controller.center = originalCenter;
    }
}
