using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;

namespace EchoLab.Player
{
    /// <summary>Bật/tắt nguồn sáng của đèn pin bằng phím F.</summary>
    [MovedFrom(true, "EchoLab.Player", null, "EchoLabFlashlightController")]
    [DisallowMultipleComponent]
    public sealed class FlashlightController : MonoBehaviour
    {
        [SerializeField, Tooltip("Light của object FlashlightSpot trong prefab tay cầm.")]
        private Light flashlight;
        [SerializeField] private FirstPersonController playerController;
        [SerializeField] private bool startOn = true;

        private InputAction toggleAction;
        public bool IsOn => flashlight != null && flashlight.enabled;

        private void Awake()
        {
            if (flashlight == null) flashlight = GetComponentInChildren<Light>(true);
            if (playerController == null) playerController = GetComponentInParent<FirstPersonController>();

            if (flashlight == null || playerController == null)
            {
                Debug.LogError("ECHO LAB: Gán FlashlightSpot và FirstPersonController cho đèn pin.", this);
                enabled = false;
                return;
            }

            toggleAction = new InputAction("ToggleFlashlight", InputActionType.Button, "<Keyboard>/f");
            SetLightEnabled(startOn);
        }

        private void OnEnable()
        {
            toggleAction?.Enable();
        }

        private void Update()
        {
            // WasPressedThisFrame chỉ đổi trạng thái một lần cho mỗi lần nhấn F.
            if (playerController.HasControl && toggleAction.WasPressedThisFrame()) Toggle();
        }

        public void Toggle()
        {
            SetLightEnabled(!IsOn);
        }

        public void SetLightEnabled(bool value)
        {
            // Chỉ tắt Light, giữ tay cầm và script hoạt động để bật lại bằng F.
            if (flashlight != null) flashlight.enabled = value;
        }

        private void OnDisable()
        {
            toggleAction?.Disable();
        }

        private void OnDestroy()
        {
            toggleAction?.Dispose();
        }
    }
}
