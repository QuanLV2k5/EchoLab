using UnityEngine;

namespace EchoLab.Gameplay.Integration
{
    /// <summary>Thông tin chung của map: thứ tự trong chuỗi, scene kế tiếp, quy tắc vòng lặp theo thiết kế.</summary>
    public sealed class MapInfo : MonoBehaviour
    {
        public int mapNumber = 1;
        public string title = "";
        [Tooltip("Tên scene kế tiếp. Rỗng nếu là map cuối (chuyển tới màn kết thúc).")]
        public string nextScene = "";
        [Tooltip("Độ dài một vòng lặp (giây) theo thiết kế.")]
        public float loopSeconds = 90f;
        [Tooltip("Số Echo tối đa mà lời giải dự kiến cần cùng lúc.")]
        public int maxEchoesNeeded = 2;
        [TextArea(2, 6)] public string summary = "";
        [Tooltip("true = mọi thứ ở đây mới là bố cục/marker, logic câu đố chưa được lập trình nối vào.")]
        public bool logicIsPlaceholder = true;
    }
}
