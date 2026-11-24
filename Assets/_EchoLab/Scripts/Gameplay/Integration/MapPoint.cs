using UnityEngine;

namespace EchoLab.Gameplay.Integration
{
    /// <summary>Điểm xuất phát, lối ra, điểm tuần tra, nơi quái đứng ban đầu.</summary>
    public sealed class MapPoint : MonoBehaviour
    {
        public PointKind kind = PointKind.Spawn;
        public string pointId = "";
        [Tooltip("Thứ tự trong tuyến tuần tra (chỉ dùng cho Patrol).")]
        public int order;
        [Tooltip("Scene sẽ tải khi Player chạm lối ra (chỉ dùng cho Exit); chỉ có tác dụng khi hệ thống GameFlow đọc nó.")]
        public string nextScene = "";
        [TextArea(1, 4)] public string notes = "";

        private void OnDrawGizmos()
        {
            Gizmos.color = kind == PointKind.Spawn ? Color.green : kind == PointKind.Exit ? Color.cyan : kind == PointKind.MonsterHome ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.3f, 0.35f);
        }
    }
}
