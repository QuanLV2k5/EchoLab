using System;

namespace EchoLab.Gameplay.Integration
{
    // ----------------------------------------------------------------------------------------------------------------
    // CÁC MARKER BÀN GIAO (chỉ chứa dữ liệu, KHÔNG có logic chơi).
    // Mục đích: cho thành viên lập trình biết đối tượng nào trong scene là thiết bị / cửa / điểm xuất phát / lối ra,
    // ID duy nhất, ai được kích hoạt và dữ liệu đầu vào – đầu ra. Hệ thống TimeLoop, Echo, AI và GameFlow do
    // lập trình viên viết và đọc các marker này; map KHÔNG tự chạy câu đố khi chỉ có các marker.
    // ----------------------------------------------------------------------------------------------------------------

    /// <summary>Ai được phép kích hoạt một thiết bị.</summary>
    [Flags]
    public enum ActorMask
    {
        None = 0,
        Player = 1,
        Echo = 2,
        Both = Player | Echo
    }

    public enum DeviceKind
    {
        Generator, Keycard, Reader, Plate, Switch, Terminal, Alarm, Horn, Relay, Screen, Pad, ClueBoard, Lift
    }

    public enum PointKind
    {
        Spawn, Exit, Patrol, MonsterHome
    }
}
