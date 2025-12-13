using UnityEngine;
using UnityEngine.InputSystem;

namespace Streets.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 3f;
        [SerializeField] private float sprintSpeed = 6f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float groundCheckDistance = 0.4f;
        [SerializeField] private LayerMask groundMask;

        [Header("Look Settings")]
        [SerializeField] private float mouseSensitivity = 100f;
        [SerializeField] private float maxLookAngle = 90f;
        [SerializeField] private Transform cameraTransform;

        [Header("Stamina Settings")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float staminaDrainRate = 20f;
        [SerializeField] private float staminaRegenRate = 10f;
        [SerializeField] private float staminaRegenDelay = 1f;
        [SerializeField] private float minStaminaToSprint = 10f;

        // Components
        private CharacterController characterController;
        private InputSystem_Actions inputActions;

        // Movement state
        private Vector2 moveInput;
        private Vector2 lookInput;
        private Vector3 velocity;
        private bool isGrounded;
        private bool isSprinting;

        // Stamina state
        private float currentStamina;
        private float timeSinceStoppedSprinting;
        private bool canSprint = true;

        // Look state
        private float xRotation;

        // Properties for UI/other systems to access
        public float CurrentStamina => currentStamina;
        public float MaxStamina => maxStamina;
        public float StaminaPercent => currentStamina / maxStamina;
        public bool IsSprinting => isSprinting;
        public bool IsGrounded => isGrounded;
        public bool IsMoving => moveInput.sqrMagnitude > 0.01f;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            inputActions = new InputSystem_Actions();
            currentStamina = maxStamina;

            if (cameraTransform == null)
            {
                cameraTransform = Camera.main?.transform;
            }
        }

        private void OnEnable()
        {
            inputActions.Enable();
            inputActions.Player.Move.performed += OnMove;
            inputActions.Player.Move.canceled += OnMove;
            inputActions.Player.Look.performed += OnLook;
            inputActions.Player.Look.canceled += OnLook;
            inputActions.Player.Sprint.performed += OnSprintPressed;
            inputActions.Player.Sprint.canceled += OnSprintReleased;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnDisable()
        {
            inputActions.Player.Move.performed -= OnMove;
            inputActions.Player.Move.canceled -= OnMove;
            inputActions.Player.Look.performed -= OnLook;
            inputActions.Player.Look.canceled -= OnLook;
            inputActions.Player.Sprint.performed -= OnSprintPressed;
            inputActions.Player.Sprint.canceled -= OnSprintReleased;
            inputActions.Disable();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update()
        {
            HandleGroundCheck();
            HandleLook();
            HandleMovement();
            HandleStamina();
            ApplyGravity();
        }

        private void HandleGroundCheck()
        {
            isGrounded = Physics.CheckSphere(
                transform.position - new Vector3(0, characterController.height / 2f, 0),
                groundCheckDistance,
                groundMask
            );

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }
        }

        private void HandleLook()
        {
            float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
            float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

            if (cameraTransform != null)
            {
                cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            }

            transform.Rotate(Vector3.up * mouseX);
        }

        private void HandleMovement()
        {
            bool wantsToSprint = isSprinting && IsMoving && canSprint;
            float currentSpeed = wantsToSprint ? sprintSpeed : walkSpeed;

            Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
            characterController.Move(moveDirection * currentSpeed * Time.deltaTime);
        }

        private void HandleStamina()
        {
            bool isActivelySprinting = isSprinting && IsMoving && canSprint;

            if (isActivelySprinting)
            {
                currentStamina -= staminaDrainRate * Time.deltaTime;
                timeSinceStoppedSprinting = 0f;

                if (currentStamina <= 0f)
                {
                    currentStamina = 0f;
                    canSprint = false;
                }
            }
            else
            {
                timeSinceStoppedSprinting += Time.deltaTime;

                if (timeSinceStoppedSprinting >= staminaRegenDelay)
                {
                    currentStamina += staminaRegenRate * Time.deltaTime;
                    currentStamina = Mathf.Min(currentStamina, maxStamina);
                }

                if (!canSprint && currentStamina >= minStaminaToSprint)
                {
                    canSprint = true;
                }
            }
        }

        private void ApplyGravity()
        {
            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
        }

        // Input callbacks
        private void OnMove(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
        }

        private void OnLook(InputAction.CallbackContext context)
        {
            lookInput = context.ReadValue<Vector2>();
        }

        private void OnSprintPressed(InputAction.CallbackContext context)
        {
            isSprinting = true;
        }

        private void OnSprintReleased(InputAction.CallbackContext context)
        {
            isSprinting = false;
        }

        // Public methods for external systems
        public void SetStamina(float amount)
        {
            currentStamina = Mathf.Clamp(amount, 0f, maxStamina);
        }

        public void ModifyStamina(float delta)
        {
            currentStamina = Mathf.Clamp(currentStamina + delta, 0f, maxStamina);
        }
    }
}
