using UnityEngine;
using UnityEngine.InputSystem;
using Streets.Input;
using Streets.Inventory;
using Streets.UI;

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

        [Header("Jump Settings")]
        [SerializeField] private bool jumpEnabled = true;
        [SerializeField] private float jumpHeight = 1.5f;

        [Header("Look Settings")]
        [SerializeField] private float mouseSensitivity = 100f;
        [SerializeField] private float maxLookAngle = 90f;
        [SerializeField] private Transform cameraTransform;

        [Header("Stamina Settings")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float staminaDrainRate = 20f;
        [SerializeField] private float staminaRegenRate = 10f;
        [SerializeField] private float staminaRegenDelay = 1f;
        [SerializeField] [Range(0f, 1f)] private float minStaminaPercentToSprint = 0.25f;

        [Header("Inventory References")]
        [SerializeField] private InventorySystem inventorySystem;
        [SerializeField] private HotbarSystem hotbarSystem;
        [SerializeField] private InventoryUI inventoryUI;

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
        public bool JumpEnabled { get => jumpEnabled; set => jumpEnabled = value; }

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
            // Ensure inputActions is created (OnEnable can run before Awake with prefabs)
            if (inputActions == null)
            {
                inputActions = new InputSystem_Actions();
            }

            inputActions.Enable();
            inputActions.Player.Move.performed += OnMove;
            inputActions.Player.Move.canceled += OnMove;
            inputActions.Player.Look.performed += OnLook;
            inputActions.Player.Look.canceled += OnLook;
            inputActions.Player.Sprint.performed += OnSprintPressed;
            inputActions.Player.Sprint.canceled += OnSprintReleased;
            inputActions.Player.Jump.performed += OnJump;

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
            inputActions.Player.Jump.performed -= OnJump;
            inputActions.Disable();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update()
        {
            HandleInventoryInput();

            // Skip movement/look when inventory is open
            if (inventoryUI != null && inventoryUI.IsOpen)
            {
                return;
            }

            HandleGroundCheck();
            HandleLook();
            HandleMovement();
            HandleStamina();
            ApplyGravity();
        }

        private void HandleInventoryInput()
        {
            // Toggle inventory with Tab
            if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
            {
                inventoryUI?.Toggle();
            }

            // Hotbar keys 1-4 (only when inventory is closed)
            if (inventoryUI == null || !inventoryUI.IsOpen)
            {
                if (Keyboard.current != null)
                {
                    if (Keyboard.current.digit1Key.wasPressedThisFrame)
                    {
                        hotbarSystem?.UseHotbarSlot(0);
                    }
                    else if (Keyboard.current.digit2Key.wasPressedThisFrame)
                    {
                        hotbarSystem?.UseHotbarSlot(1);
                    }
                    else if (Keyboard.current.digit3Key.wasPressedThisFrame)
                    {
                        hotbarSystem?.UseHotbarSlot(2);
                    }
                    else if (Keyboard.current.digit4Key.wasPressedThisFrame)
                    {
                        hotbarSystem?.UseHotbarSlot(3);
                    }
                }
            }
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

                if (!canSprint && currentStamina >= maxStamina * minStaminaPercentToSprint)
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

        private void OnJump(InputAction.CallbackContext context)
        {
            if (jumpEnabled && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
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
