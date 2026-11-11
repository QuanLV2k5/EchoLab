using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;

namespace EchoLab.Player
{
    /// <summary>Điều khiển góc nhìn thứ nhất: đi bộ, chạy, nhìn và khoá con trỏ.</summary>
    [MovedFrom(true, "EchoLab.Player", null, "EchoLabFpsController")]
    [DefaultExecutionOrder(-10)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class FirstPersonController : MonoBehaviour
    {
        // Vận tốc nhỏ hướng xuống giữ nhân vật tiếp xúc với mặt sàn.
        private const float GroundedVerticalSpeed = -2f;

        [Header("References")]
        [SerializeField, Tooltip("Camera con nằm ở tầm mắt của Player.")]
        private Transform cameraPivot;

        [Header("Movement (metres per second)")]
        [SerializeField, Min(0.1f)] private float walkSpeed = 3f;
        [SerializeField, Min(0.1f)] private float sprintSpeed = 5.5f;
        [SerializeField, Tooltip("Gia tốc hướng xuống; luôn là số âm.")]
        private float gravity = -20f;

        [Header("Mouse look (degrees per pixel)")]
        [SerializeField, Range(0.01f, 1f)] private float mouseSensitivity = 0.08f;
        [SerializeField, Range(1f, 89f)] private float pitchLimit = 80f;

        private CharacterController characterController;
        private InputActionMap inputMap;
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction sprintAction;
        private InputAction releaseCursorAction;
        private float verticalVelocity;
        private float pitch;
        private bool controlEnabled = true;
        private bool skipNextLook = true;

        public bool IsSprinting { get; private set; }

        // UI, đèn pin và các cơ chế khác có thể dùng cùng điều kiện nhận đầu vào.
        public bool HasControl => isActiveAndEnabled && controlEnabled
            && Application.isFocused && Cursor.lockState == CursorLockMode.Locked
            && Time.timeScale > 0f;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();

            if (cameraPivot == null)
            {
                Camera childCamera = GetComponentInChildren<Camera>(true);
                if (childCamera != null) cameraPivot = childCamera.transform;
            }

            if (cameraPivot == null)
            {
                Debug.LogError("ECHO LAB: Gán PlayerCamera vào Camera Pivot.", this);
                enabled = false;
                return;
            }

            pitch = Mathf.Clamp(Mathf.DeltaAngle(0f, cameraPivot.localEulerAngles.x), -pitchLimit, pitchLimit);
            CreateInputActions();
        }

        private void CreateInputActions()
        {
            inputMap = new InputActionMap("Player");
            moveAction = inputMap.AddAction("Move", InputActionType.Value, expectedControlLayout: "Vector2");
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");

            lookAction = inputMap.AddAction("Look", InputActionType.PassThrough,
                binding: "<Mouse>/delta", expectedControlLayout: "Vector2");
            sprintAction = inputMap.AddAction("Sprint", InputActionType.Button,
                binding: "<Keyboard>/leftShift");
            releaseCursorAction = inputMap.AddAction("ReleaseCursor", InputActionType.Button,
                binding: "<Keyboard>/escape");
        }

        private void OnEnable()
        {
            inputMap?.Enable();
        }

        private void Start()
        {
            if (controlEnabled && Application.isFocused) LockCursor();
        }

        private void Update()
        {
            if (inputMap == null || !characterController.enabled) return;

            UpdateCursorLock();
            if (Time.timeScale <= 0f)
            {
                IsSprinting = false;
                return;
            }

            bool canReadInput = HasControl;
            UpdateMouseLook(canReadInput);
            UpdateMovement(canReadInput);
        }

        private void UpdateCursorLock()
        {
            if (releaseCursorAction.WasPressedThisFrame())
            {
                UnlockCursor();
                return;
            }

            bool clickedGameView = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            if (controlEnabled && Application.isFocused && Time.timeScale > 0f
                && Cursor.lockState != CursorLockMode.Locked && clickedGameView)
            {
                LockCursor();
            }
        }

        private void UpdateMouseLook(bool canReadInput)
        {
            if (!canReadInput)
            {
                skipNextLook = true;
                return;
            }

            // Bỏ frame đầu sau khi khoá chuột để tránh góc nhìn nhảy đột ngột.
            if (skipNextLook)
            {
                skipNextLook = false;
                return;
            }

            Vector2 mouseDelta = lookAction.ReadValue<Vector2>();
            // Mouse delta đã là dịch chuyển trong frame, không nhân thêm deltaTime.
            transform.Rotate(0f, mouseDelta.x * mouseSensitivity, 0f);
            pitch = Mathf.Clamp(pitch - mouseDelta.y * mouseSensitivity, -pitchLimit, pitchLimit);
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void UpdateMovement(bool canReadInput)
        {
            Vector2 moveInput = canReadInput
                ? Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f)
                : Vector2.zero;

            // Giới hạn hướng chéo để W+D không nhanh hơn W.
            IsSprinting = canReadInput && moveInput.sqrMagnitude > 0.001f && sprintAction.IsPressed();
            float speed = IsSprinting ? sprintSpeed : walkSpeed;
            Vector3 horizontalVelocity = (transform.right * moveInput.x + transform.forward * moveInput.y) * speed;

            // CharacterController.Move không tự cộng trọng lực.
            if (characterController.isGrounded && verticalVelocity < 0f)
                verticalVelocity = GroundedVerticalSpeed;

            verticalVelocity += gravity * Time.deltaTime;
            Vector3 velocity = horizontalVelocity + Vector3.up * verticalVelocity;
            characterController.Move(velocity * Time.deltaTime);
        }

        /// <summary>Khoá/mở thao tác khi game chuyển trạng thái hoặc hiện UI.</summary>
        public void SetControlEnabled(bool value)
        {
            controlEnabled = value;
            IsSprinting = false;
            if (value && isActiveAndEnabled && Application.isFocused) LockCursor();
            else UnlockCursor();
        }

        /// <summary>Xoá vận tốc khi dịch chuyển; không tự reset vị trí và góc nhìn.</summary>
        public void ResetMotion()
        {
            verticalVelocity = 0f;
            IsSprinting = false;
            skipNextLook = true;
        }

        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            skipNextLook = true;
        }

        private void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            skipNextLook = true;
            IsSprinting = false;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus) UnlockCursor();
            skipNextLook = true;
        }

        private void OnDisable()
        {
            inputMap?.Disable();
            UnlockCursor();
        }

        private void OnDestroy()
        {
            inputMap?.Dispose();
        }

        private void OnValidate()
        {
            sprintSpeed = Mathf.Max(walkSpeed, sprintSpeed);
            gravity = Mathf.Min(-0.1f, gravity);
        }
    }
}
