using UnityEngine;

namespace EchoLab.Gameplay.Integration
{
    /// <summary>
    /// Cửa. Mỗi cửa có một collider chắn (blocker) đang BẬT = cửa đóng. Logic thật sẽ do hệ thống lập trình tắt/bật blocker
    /// (hoặc thay bằng animation cửa của họ). SetOpenForDebug chỉ phục vụ đi thử map bằng MapNavigator, không phải điều kiện thắng.
    /// </summary>
    public sealed class DoorMarker : MonoBehaviour
    {
        public string doorId = "";
        [Tooltip("Collider chắn lối khi cửa đóng.")]
        public GameObject blocker;
        public bool startsOpen;
        [Tooltip("Mô tả điều kiện mở cửa theo thiết kế.")]
        [TextArea(2, 5)] public string openWhen = "";
        [Tooltip("true = cửa đóng lại khi reset vòng.")]
        public bool closesOnLoopReset = true;

        public bool DebugOpen { get; private set; }

        /// <summary>CHỈ để debug: bật/tắt collider chắn. Không được dùng làm cơ chế mở cửa của game.</summary>
        public void SetOpenForDebug(bool open)
        {
            DebugOpen = open;
            if (blocker != null) blocker.SetActive(!open);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = (blocker != null && blocker.activeSelf) ? new Color(1f, 0.2f, 0.2f, 0.9f) : new Color(0.2f, 1f, 0.3f, 0.9f);
            Gizmos.DrawWireCube(transform.position + Vector3.up * 1.5f, new Vector3(7f, 3f, 0.3f));
        }
    }
}
