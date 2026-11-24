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
        [SerializeField] private Transform viewRoot;

        private InputAction toggleAction;
        private Vector3 restPosition, previousPlayerPosition, lensPosition;
        private Quaternion restRotation, previousViewRotation;
        private Transform cameraTransform;
        private float stride, speed;
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
            if (viewRoot == null) viewRoot = transform;
            restPosition = viewRoot.localPosition;
            restRotation = viewRoot.localRotation;
            previousPlayerPosition = playerController.transform.position;
            cameraTransform = playerController.GetComponentInChildren<Camera>().transform;
            previousViewRotation = cameraTransform.rotation;
            lensPosition = flashlight.transform.localPosition;
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

        private void LateUpdate()
        {
            if (flashlight == null || playerController == null || Time.deltaTime <= 0) return;
            float dt = Time.deltaTime;
            Vector3 movement = playerController.transform.position - previousPlayerPosition;
            previousPlayerPosition = playerController.transform.position;
            movement.y = 0;
            // A loop teleport must never become a violent hand movement.
            float actualSpeed = movement.magnitude < .7f ? movement.magnitude / dt : 0;
            speed = Mathf.Lerp(speed, actualSpeed, 1 - Mathf.Exp(-dt * 10));
            stride += dt * Mathf.Lerp(7.5f, 11f, Mathf.InverseLerp(3, 5.5f, speed));
            float amount = Mathf.Clamp01(speed / 3f);
            Vector3 bob = new Vector3(Mathf.Sin(stride) * .0035f, Mathf.Cos(stride * 2) * .003f, 0) * amount;
            bob.y += Mathf.Sin(Time.time * 1.6f) * .0011f;
            if (playerController.IsSprinting) bob += new Vector3(-.008f, -.012f, -.01f);
            Vector3 lookDelta = (Quaternion.Inverse(previousViewRotation) * cameraTransform.rotation).eulerAngles;
            lookDelta.x = Mathf.DeltaAngle(0, lookDelta.x);
            lookDelta.y = Mathf.DeltaAngle(0, lookDelta.y);
            previousViewRotation = cameraTransform.rotation;
            Quaternion target = restRotation * Quaternion.Euler(
                Mathf.Clamp(-lookDelta.x * .35f, -1.5f, 1.5f),
                Mathf.Clamp(-lookDelta.y * .35f, -1.5f, 1.5f), Mathf.Sin(stride) * amount * .35f);
            float blend = 1 - Mathf.Exp(-dt * 12);
            viewRoot.localPosition = Vector3.Lerp(viewRoot.localPosition, restPosition + bob, blend);
            viewRoot.localRotation = Quaternion.Slerp(viewRoot.localRotation, target, blend);

            // Keep the emitter on the visible side of a nearby wall. The actual
            // lens and housing stay rigid; there is no visible cone geometry.
            flashlight.transform.localPosition = lensPosition;
            float lensDistance = Vector3.Dot(flashlight.transform.position - cameraTransform.position, cameraTransform.forward);
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out var hit,
                Mathf.Max(.1f, lensDistance + .08f), ~(1 << 8 | 1 << 2), QueryTriggerInteraction.Ignore))
            {
                float retreat = Mathf.Max(0, lensDistance - Mathf.Max(.02f, hit.distance - .055f));
                flashlight.transform.position -= cameraTransform.forward * retreat;
            }
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
