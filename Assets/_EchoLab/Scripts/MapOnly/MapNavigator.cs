using EchoLab.Gameplay.Integration;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace EchoLab.MapOnly
{
    /// <summary>
    /// Công cụ DEBUG đi thử map: 1-5 đổi map, R tải lại map, O bật/tắt mọi collider chắn cửa (chỉ để đi dạo).
    /// Đây KHÔNG phải hệ thống tiến trình, TimeLoop hay Echo; phím O không được tính là giải câu đố.
    /// </summary>
    public sealed class MapNavigator : MonoBehaviour
    {
        public static readonly string[] Maps =
        {
            "Level01_TinhThuc", "Level02_DongDienCuoiCung", "Level03_TiengGoiTrongBongToi", "Level04_BuongCachLy", "Level05_NhungConSoBiCheGiau"
        };

        private static readonly string[] Titles =
        {
            "MAP 1  /  TỈNH THỨC", "MAP 2  /  DÒNG ĐIỆN CUỐI CÙNG", "MAP 3  /  TIẾNG GỌI TRONG BÓNG TỐI", "MAP 4  /  BUỒNG CÁCH LY", "MAP 5  /  NHỮNG CON SỐ BỊ CHE GIẤU"
        };

        private GUIStyle style;
        private bool debugDoorsOpen;

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            Key[] keys = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5 };
            for (int i = 0; i < keys.Length; i++)
                if (keyboard[keys[i]].wasPressedThisFrame) SceneManager.LoadScene(Maps[i]);
            if (keyboard[Key.R].wasPressedThisFrame) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            if (keyboard[Key.O].wasPressedThisFrame)
            {
                debugDoorsOpen = !debugDoorsOpen;
                foreach (var door in FindObjectsByType<DoorMarker>(FindObjectsSortMode.None)) door.SetOpenForDebug(debugDoorsOpen);
            }
        }

        private void OnGUI()
        {
            if (style == null) style = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold, normal = { textColor = new Color(0.6f, 0.95f, 1f) } };
            int index = System.Array.IndexOf(Maps, SceneManager.GetActiveScene().name);
            GUI.Label(new Rect(16, 12, 900, 28), index >= 0 ? Titles[index] : SceneManager.GetActiveScene().name, style);
            GUI.Label(new Rect(16, 36, 900, 28), "WASD: đi   SHIFT: chạy   F: đèn pin   1–5: đổi map   R: về đầu map   O: [DEBUG] " + (debugDoorsOpen ? "đóng" : "mở") + " mọi cửa   ESC: thả chuột", style);
        }
    }
}
