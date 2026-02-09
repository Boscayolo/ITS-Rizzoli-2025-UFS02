using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float crouchSpeedMultiplier = 0.5f;
    [SerializeField] float sprintSpeedMultiplier = 1.5f;

    [Header("View")]
    [SerializeField] Transform cameraRoot;
    [SerializeField] float lookSensitivity = 25f;

    [Tooltip("Mouse delta: di solito FALSE. Gamepad stick: TRUE.")]
    [SerializeField] bool scaleLookByDeltaTime = false;

    [Tooltip("Se true, quando Time.timeScale == 0 il look viene bloccato (pausa).")]
    [SerializeField] bool freezeLookWhenTimeScaleZero = true;

    [SerializeField] float maxLookUp = 80f;
    [SerializeField] float maxLookDown = -80f;

    [Header("Jump & Gravity")]
    [SerializeField] float jumpHeight = 1.5f;
    [SerializeField] float gravity = -9.81f;

    [Header("Crouch")]
    [SerializeField] float crouchHeight = 1.0f;

    CharacterController controller;
    PlayerInputActions inputActions;

    Vector2 moveInput;
    float verticalVelocity;

    float pitch; // xRotation
    float yaw;

    float originalHeight;
    Vector3 originalCenter;
    bool isCrouching;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        originalHeight = controller.height;
        originalCenter = controller.center;

        inputActions = new PlayerInputActions();

        inputActions.Player.Jump.performed += _ => TryJump();
        inputActions.Player.Crouch.performed += _ => StartCrouch();
        inputActions.Player.Crouch.canceled += _ => StopCrouch();
        inputActions.Player.Sprint.performed += _ => StartSprint();
        inputActions.Player.Sprint.canceled += _ => StopSprint();

        // Init yaw dal player
        yaw = transform.eulerAngles.y;

        // Reset view iniziale (evita “guardo i piedi”)
        ResetView();
    }

    void OnEnable()
    {
        inputActions.Enable();
        // Quando riabiliti lo script, azzera eventuali delta “sporchi”
        ResetView();
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

    void Update()
    {
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        Movement();
    }

    void LateUpdate()
    {
        Look();
    }

    void ResetView()
    {
        pitch = 0f;

        // opzionale: riallinea yaw al transform corrente
        yaw = transform.eulerAngles.y;

        ApplyView();
    }

    void Look()
    {
        if (cameraRoot == null) return;

        if (freezeLookWhenTimeScaleZero && Time.timeScale == 0f)
            return;

        Vector2 rawLook = inputActions.Player.Look.ReadValue<Vector2>();

        float factor = lookSensitivity * (scaleLookByDeltaTime ? Time.deltaTime : 1f);

        yaw += rawLook.x * factor;
        pitch -= rawLook.y * factor;
        pitch = Mathf.Clamp(pitch, maxLookDown, maxLookUp);

        ApplyView();
    }

    void ApplyView()
    {
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        if (cameraRoot != null)
            cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    void Movement()
    {
        bool grounded = controller.isGrounded;

        if (grounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        float currentSpeed = isCrouching ? moveSpeed * crouchSpeedMultiplier : moveSpeed;
        move *= currentSpeed;

        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);
    }

    void TryJump()
    {
        if (controller.isGrounded && !isCrouching)
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    void StartCrouch()
    {
        if (isCrouching) return;

        isCrouching = true;
        controller.height = crouchHeight;
        controller.center = new Vector3(originalCenter.x, crouchHeight / 2f, originalCenter.z);
    }

    void StopCrouch()
    {
        if (!isCrouching) return;

        isCrouching = false;
        controller.height = originalHeight;
        controller.center = originalCenter;
    }

    void StartSprint()
    {

    }

    void StopSprint()
    {

    }
}
