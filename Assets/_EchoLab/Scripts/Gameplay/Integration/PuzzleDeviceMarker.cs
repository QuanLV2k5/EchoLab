using System;
using UnityEngine;

namespace EchoLab.Gameplay.Integration
{
    /// <summary>Một thiết bị tương tác / manh mối. ID là khoá để hệ thống lập trình tham chiếu.</summary>
    public sealed class PuzzleDeviceMarker : MonoBehaviour
    {
        [Tooltip("ID duy nhất trong toàn dự án, dạng M2_RELAY_A.")]
        public string deviceId = "";
        public DeviceKind kind = DeviceKind.Switch;
        public ActorMask allowedActors = ActorMask.Player;
        [Tooltip("Thời gian phải giữ/đứng để kích hoạt (giây). 0 = tức thời.")]
        public float holdSeconds;
        [Tooltip("Sau khi kích hoạt thì tự tắt sau N giây. 0 = giữ đến khi nhả hoặc hết vòng.")]
        public float activeSeconds;
        [Tooltip("Điều kiện đầu vào (ID thiết bị / trạng thái cần có).")]
        public string[] inputs = Array.Empty<string>();
        [Tooltip("Kết quả đầu ra (ID cửa / thiết bị / sự kiện bị ảnh hưởng).")]
        public string[] outputs = Array.Empty<string>();
        public string startState = "";
        [Tooltip("Trạng thái khi vòng lặp reset.")]
        public string resetState = "";
        [Tooltip("true = giữ nguyên khi reset vòng (tiến trình map); false = reset mỗi vòng.")]
        public bool persistsAcrossLoops;
        [TextArea(2, 6)] public string notes = "";

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.6f, 0.1f);
            Gizmos.DrawWireCube(transform.position + Vector3.up * 0.1f, new Vector3(0.5f, 0.2f, 0.5f));
        }
    }
}
