using System.Linq;
using EchoLab.Gameplay.Integration;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoLab.EditorTools
{
    /// <summary>
    /// Dựng nhóm "00 • HANDOFF" trong từng scene: marker điểm xuất phát, lối ra, thiết bị, cửa (collider chắn) và điểm tuần tra.
    /// Chạy được nhiều lần (xoá nhóm cũ rồi dựng lại). Không đổi kiến trúc map, không thêm logic chơi.
    /// Menu: EchoLab > Map > Apply Handoff Markers (all maps).
    /// </summary>
    public static class MapFlowBuilder
    {
        private const string RootName = "00 • HANDOFF";
        private const string LevelFolder = "Assets/_EchoLab/Scenes/Levels/";
        private const ActorMask P = ActorMask.Player;
        private const ActorMask B = ActorMask.Both;

        private sealed class Dev
        {
            public string id; public DeviceKind kind; public Vector3 pos; public ActorMask actors = P;
            public float hold, active; public string[] inputs = new string[0], outputs = new string[0];
            public string start = "", reset = "", notes = "";
        }
        private sealed class Door { public string id; public float z; public string openWhen; public bool closesOnReset = true; }
        private sealed class Pt { public PointKind kind; public string id; public Vector3 pos; public int order; public string next = ""; public string notes = ""; }
        private sealed class Spec
        {
            public string scene, title, next, summary; public int number, echoes;
            public Dev[] devs; public Door[] doors; public Pt[] pts; public Vector3 exitPos;
        }

        private static Vector3 V(float x, float y, float z) => new Vector3(x, y, z);
        private static string[] A(params string[] s) => s;

        private static Dev D(string id, DeviceKind k, Vector3 pos, ActorMask who, float hold, float active, string[] inp, string[] outp, string start, string reset, string notes)
            => new Dev { id = id, kind = k, pos = pos, actors = who, hold = hold, active = active, inputs = inp, outputs = outp, start = start, reset = reset, notes = notes };

        private static Spec[] Specs()
        {
            return new[]
            {
                new Spec {
                    scene = "Level01_TinhThuc", number = 1, title = "Tỉnh thức", next = "Level02_DongDienCuoiCung", echoes = 2, exitPos = V(0, 0, 54.5f),
                    summary = "Giai đoạn 1 (không Echo/quái): bật nguồn, lấy thẻ, qua cửa Access. Giai đoạn 2: học giữ bàn đạp bằng 1 Echo. Giai đoạn 3: báo động dụ quái. Giai đoạn 4: hai Echo giữ bàn đạp A/B, Player dùng máy tính đồng bộ mở cửa Escape.",
                    doors = new[] {
                        new Door { id = "M1_DOOR_ACCESS", z = 8f, closesOnReset = false, openWhen = "M1_GENERATOR=ON và Player đang mang M1_KEYCARD (thẻ chỉ Player mang). TIẾN TRÌNH: sau khi mở lần đầu thì giữ mở qua các vòng của map." },
                        new Door { id = "M1_DOOR_ECHOPLATE", z = 16f, closesOnReset = false, openWhen = "Mở khi M1_PLATE_ENTRY đang bị giữ (Player hoặc Echo). TIẾN TRÌNH: khi Player đã đi qua lần đầu thì giữ mở vĩnh viễn trong lần chơi map, nên Echo giữ bàn đạp này có thể bị bỏ." },
                        new Door { id = "M1_DOOR_ESCAPE", z = 48f, openWhen = "Mở khi M1_TERMINAL_SYNC xác nhận M1_PLATE_A và M1_PLATE_B cùng bị giữ." } },
                    devs = new[] {
                        D("M1_GENERATOR", DeviceKind.Generator, V(-3.8f,1.1f,5.4f), P, 0, 0, A(), A("M1_DOOR_ACCESS"), "OFF", "OFF", "Máy phát điện (biển POWER [E] START)."),
                        D("M1_KEYCARD", DeviceKind.Keycard, V(3.5f,1.1f,4.3f), P, 0, 0, A(), A("M1_DOOR_ACCESS"), "Có trên bàn", "Có trên bàn", "Vật phẩm: chỉ Player mang; Echo không nhặt và không nhân đôi vật phẩm."),
                        D("M1_PLATE_ENTRY", DeviceKind.Plate, V(-3.6f,0.1f,12f), B, 0, 0, A(), A("M1_DOOR_ECHOPLATE"), "Nhả", "Nhả", "Bàn đạp dạy Echo: phải có người đứng giữ để cửa EchoPlate mở. Biển: HOLD R TO RECORD."),
                        D("M1_ALARM", DeviceKind.Alarm, V(-4.5f,1.2f,23.4f), B, 0, 12, A(), A("NOISE@M1_ALARM"), "Tắt", "Tắt", "Báo động dụ quái rời tuyến tuần tra; phát âm thanh ~12 giây (thông số đề xuất)."),
                        D("M1_PLATE_A", DeviceKind.Plate, V(-3.5f,0.1f,43f), B, 0, 0, A(), A("M1_TERMINAL_SYNC"), "Nhả", "Nhả", "Bàn đạp A của câu đố cuối."),
                        D("M1_PLATE_B", DeviceKind.Plate, V(3.5f,0.1f,43f), B, 0, 0, A(), A("M1_TERMINAL_SYNC"), "Nhả", "Nhả", "Bàn đạp B của câu đố cuối."),
                        D("M1_TERMINAL_SYNC", DeviceKind.Terminal, V(0f,1.2f,44.6f), P, 0, 0, A("M1_PLATE_A=held","M1_PLATE_B=held"), A("M1_DOOR_ESCAPE"), "Khoá", "Khoá", "Máy tính đồng bộ: chỉ Player thao tác; cần cả hai bàn đạp đang bị giữ ở thời điểm bấm."),
                        D("M1_CLUE_01", DeviceKind.ClueBoard, V(4.6f,1.3f,7.6f), P, 0, 0, A(), A(), "Sẵn sàng", "Sẵn sàng", "Bảng ARCHIVE [E] READ: hướng dẫn nguồn + thẻ."),
                        D("M1_CLUE_02", DeviceKind.ClueBoard, V(4.7f,1.3f,15.6f), P, 0, 0, A(), A(), "Sẵn sàng", "Sẵn sàng", "Bảng ARCHIVE: cách ghi Echo (giữ R) trước cửa EchoPlate."),
                        D("M1_CLUE_03", DeviceKind.ClueBoard, V(4.5f,1.3f,23.6f), P, 0, 0, A(), A(), "Sẵn sàng", "Sẵn sàng", "Bảng ARCHIVE: âm thanh thu hút sinh vật."),
                        D("M1_CLUE_04", DeviceKind.ClueBoard, V(4.7f,1.3f,47.6f), P, 0, 0, A(), A(), "Sẵn sàng", "Sẵn sàng", "Bảng ARCHIVE: hai Echo giữ A/B, bạn đồng bộ.") },
                    pts = new[] {
                        new Pt { kind = PointKind.MonsterHome, id = "M1_MONSTER_HOME", pos = V(-3.7f,0.1f,30f) },
                        new Pt { kind = PointKind.Patrol, id = "M1_PATROL_1", pos = V(-3.7f,0f,29f), order = 1 },
                        new Pt { kind = PointKind.Patrol, id = "M1_PATROL_2", pos = V(0f,0f,32f), order = 2 },
                        new Pt { kind = PointKind.Patrol, id = "M1_PATROL_3", pos = V(3.2f,0f,36.5f), order = 3 },
                        new Pt { kind = PointKind.Patrol, id = "M1_PATROL_4", pos = V(-3.8f,0f,37.5f), order = 4 } } },

                new Spec {
                    scene = "Level02_DongDienCuoiCung", number = 2, title = "Dòng điện cuối cùng", next = "Level03_TiengGoiTrongBongToi", echoes = 2, exitPos = V(0, 0, 69.5f),
                    summary = "Ba relay A/B/C ở ba nhánh, mỗi relay chỉ duy trì 8 giây; phải kích hoạt theo thứ tự A→B→C và cùng hoạt động để khoá lưới điện và mở cổng thang máy.",
                    doors = new[] { new Door { id = "M2_DOOR_LIFT_GATE", z = 54f, openWhen = "Mở khi M2_GRID_LOCK = LOCKED (3 relay hợp lệ), giữ mở đến hết vòng." } },
                    devs = new[] {
                        D("M2_PRACTICE_RELAY", DeviceKind.Relay, V(3.6f,0.9f,5.2f), B, 0, 8, A(), A(), "Tắt", "Tắt", "Relay thử gần điểm xuất phát: cho thấy nguồn tự ngắt sau 8 giây (đèn đỏ/vàng/xanh)."),
                        D("M2_OPSLOG", DeviceKind.ClueBoard, V(-3.6f,0.7f,6.1f), P, 0, 0, A(), A(), "Sẵn sàng", "Sẵn sàng", "Nhật ký vận hành [E]: giải thích thứ tự A, B, C và cửa sổ 8 giây."),
                        D("M2_RELAY_A", DeviceKind.Relay, V(-24f,0.9f,26.6f), B, 0, 8, A(), A("M2_GRID_LOCK"), "Tắt", "Tắt", "Relay A (nhánh tây). Phải được kích hoạt ĐẦU TIÊN."),
                        D("M2_RELAY_B", DeviceKind.Relay, V(24f,0.9f,26.6f), B, 0, 8, A("M2_RELAY_A=active"), A("M2_GRID_LOCK"), "Tắt", "Tắt", "Relay B (nhánh đông). Hợp lệ khi A đang hoạt động."),
                        D("M2_RELAY_C", DeviceKind.Relay, V(3.5f,0.9f,51.2f), B, 0, 8, A("M2_RELAY_B=active"), A("M2_GRID_LOCK"), "Tắt", "Tắt", "Relay C (nhánh giữa, gần cổng thang máy). Hợp lệ khi B đang hoạt động."),
                        D("M2_GRID_LOCK", DeviceKind.Switch, V(0f,1.5f,53.6f), P, 0, 0, A("M2_RELAY_A","M2_RELAY_B","M2_RELAY_C"), A("M2_DOOR_LIFT_GATE","M2_LIFT"), "UNLOCKED", "UNLOCKED", "Thiết bị ảo (không có model): khi cả ba relay cùng active theo thứ tự A→B→C thì LOCKED đến hết vòng.") },
                    pts = new Pt[0] },

                new Spec {
                    scene = "Level03_TiengGoiTrongBongToi", number = 3, title = "Tiếng gọi trong bóng tối", next = "Level04_BuongCachLy", echoes = 1, exitPos = V(0, 0, 52.5f),
                    summary = "Khu lưu trữ mẫu vật có sinh vật tuần tra. Hai bảng loa trong hành lang kỹ thuật phát tiếng ở loa A/B để dụ sinh vật; lấy thẻ ở buồng đông và quét tại cửa an ninh.",
                    doors = new[] { new Door { id = "M3_DOOR_SECURITY", z = 50f, openWhen = "Mở khi Player quét M3_KEYCARD tại M3_READER." } },
                    devs = new[] {
                        D("M3_SPEAKER_A_SWITCH", DeviceKind.Switch, V(-17.4f,0.9f,21f), B, 0, 15, A(), A("M3_HORN_A"), "Tắt", "Tắt", "Công tắc loa A (hành lang kỹ thuật phía tây, gần lối vào). Phát tiếng ở M3_HORN_A 15 giây (tài liệu gốc 12 giây; tăng để đủ đệm cho người mới)."),
                        D("M3_HORN_A", DeviceKind.Horn, V(-13.8f,3.1f,42f), B, 0, 0, A("M3_SPEAKER_A_SWITCH"), A("NOISE@M3_HORN_A"), "Im", "Im", "Nguồn âm A (xa lối vào, phía tây-bắc). Sinh vật phải tìm được đường tới đây bằng NavMesh."),
                        D("M3_SPEAKER_B_SWITCH", DeviceKind.Switch, V(-17.4f,0.9f,36f), B, 0, 15, A(), A("M3_HORN_B"), "Tắt", "Tắt", "Công tắc loa B (hành lang kỹ thuật phía tây, sâu hơn)."),
                        D("M3_HORN_B", DeviceKind.Horn, V(13.8f,3.1f,20f), B, 0, 0, A("M3_SPEAKER_B_SWITCH"), A("NOISE@M3_HORN_B"), "Im", "Im", "Nguồn âm B (phía đông, gần lối vào) kéo sinh vật rời khỏi đường thoát."),
                        D("M3_KEYCARD", DeviceKind.Keycard, V(11.4f,0.9f,42.2f), P, 0, 0, A(), A("M3_READER"), "Có trên bàn", "Có trên bàn", "Thẻ truy cập cấp cao trong buồng đông; chỉ Player nhặt và mang."),
                        D("M3_READER", DeviceKind.Reader, V(3.3f,1.1f,46.3f), P, 0, 0, A("M3_KEYCARD"), A("M3_DOOR_SECURITY"), "Khoá", "Khoá", "Đầu đọc thẻ trước cửa an ninh.") },
                    pts = new[] {
                        new Pt { kind = PointKind.MonsterHome, id = "M3_WARDEN_HOME", pos = V(0f,0.1f,21f) },
                        new Pt { kind = PointKind.Patrol, id = "M3_PATROL_1", pos = V(0f,0f,20f), order = 1, notes = "entrance" },
                        new Pt { kind = PointKind.Patrol, id = "M3_PATROL_2", pos = V(-5f,0f,26f), order = 2 },
                        new Pt { kind = PointKind.Patrol, id = "M3_PATROL_3", pos = V(2.5f,0f,27f), order = 3 },
                        new Pt { kind = PointKind.Patrol, id = "M3_PATROL_4", pos = V(-3f,0f,34f), order = 4 },
                        new Pt { kind = PointKind.Patrol, id = "M3_PATROL_5", pos = V(5f,0f,38.5f), order = 5, notes = "booth door" },
                        new Pt { kind = PointKind.Patrol, id = "M3_PATROL_6", pos = V(-1f,0f,43.5f), order = 6, notes = "exit route" } } },

                new Spec {
                    scene = "Level04_BuongCachLy", number = 4, title = "Buồng cách ly", next = "Level05_NhungConSoBiCheGiau", echoes = 2, exitPos = V(0, 0, 48f),
                    summary = "Cửa ngoài, buồng trung gian, cửa trong. A giữ cửa ngoài; sau khi A nhả, B mới mở cửa trong; B phải nhả sau khi qua để đóng cửa sau và dùng bảng điều khiển cuối.",
                    doors = new[] {
                        new Door { id = "M4_DOOR_OUTER", z = 20f, openWhen = "Mở khi M4_PLATE_A đang bị giữ. Khi A đang giữ thì cửa trong bị khoá." },
                        new Door { id = "M4_DOOR_INNER", z = 32f, openWhen = "Mở khi M4_PLATE_B đang bị giữ VÀ M4_PLATE_A đã được nhả." },
                        new Door { id = "M4_DOOR_CONTROL", z = 46f, openWhen = "Mở sau khi M4_PANEL_SEAL xác nhận chu trình cách ly." } },
                    devs = new[] {
                        D("M4_MODEL", DeviceKind.ClueBoard, V(-4.2f,1.1f,7.2f), P, 0, 0, A(), A(), "Sẵn sàng", "Sẵn sàng", "Mô hình thu nhỏ minh hoạ quy tắc một cửa mở tại một thời điểm."),
                        D("M4_PLATE_A", DeviceKind.Plate, V(-3.4f,0.1f,16.6f), B, 0, 0, A(), A("M4_DOOR_OUTER"), "Nhả", "Nhả", "Bàn đạp A (trước cửa ngoài)."),
                        D("M4_PLATE_B", DeviceKind.Plate, V(2.6f,0.1f,28.6f), B, 0, 0, A("M4_PLATE_A=released"), A("M4_DOOR_INNER"), "Nhả", "Nhả", "Bàn đạp B (trong buồng, trước cửa trong)."),
                        D("M4_PANEL_SEAL", DeviceKind.Switch, V(5.4f,0.9f,38f), P, 0, 0, A("M4_PLATE_B=released","M4_DOOR_INNER=closed"), A("M4_DOOR_CONTROL"), "Khoá", "Khoá", "Công tắc Seal tại bảng điều khiển cuối; chỉ Player. Chỉ hợp lệ khi cửa trong đã đóng lại sau khi B nhả.") },
                    pts = new[] {
                        new Pt { kind = PointKind.MonsterHome, id = "M4_WATCHER_HOME", pos = V(-5f,0.1f,22f), notes = "Sinh vật phía sau kính, không đi vào buồng." },
                        new Pt { kind = PointKind.Patrol, id = "M4_LANE_1", pos = V(-5f,0f,22.5f), order = 1 },
                        new Pt { kind = PointKind.Patrol, id = "M4_LANE_2", pos = V(-5f,0f,26f), order = 2 },
                        new Pt { kind = PointKind.Patrol, id = "M4_LANE_3", pos = V(-5f,0f,29.5f), order = 3 } } },

                new Spec {
                    scene = "Level05_NhungConSoBiCheGiau", number = 5, title = "Những con số bị che giấu", next = "", echoes = 2, exitPos = V(0, 0, 36.2f),
                    summary = "Hai trạm giải mã ở hai đầu phải cùng hoạt động để màn hình dữ liệu hiện chữ số; đối chiếu ba màn hình rồi nhập mã tại máy tính trung tâm để mở cửa Optics.",
                    doors = new[] { new Door { id = "M5_DOOR_OPTICS", z = 34f, openWhen = "Mở khi M5_CIPHER_TERMINAL nhận mã đúng trong lúc cả hai pad đang bị giữ." } },
                    devs = new[] {
                        D("M5_JOURNAL", DeviceKind.ClueBoard, V(-5.1f,1.1f,5f), P, 0, 0, A(), A(), "Sẵn sàng", "Sẵn sàng", "Nhật ký giải mã [E]."),
                        D("M5_EXAMPLE", DeviceKind.ClueBoard, V(5.7f,2.2f,5f), P, 0, 0, A(), A(), "Sẵn sàng", "Sẵn sàng", "Ví dụ giải mã: A3 B8 C1 D6 E5 F2, đọc D>C>A>F>B>E, bỏ C và E → 6328."),
                        D("M5_PAD_1", DeviceKind.Pad, V(-26f,0.1f,23f), B, 0, 0, A(), A("M5_SCREEN_DIGITS","M5_CIPHER_TERMINAL"), "Nhả", "Nhả", "Trạm giải mã 1 (tây)."),
                        D("M5_PAD_2", DeviceKind.Pad, V(26f,0.1f,23f), B, 0, 0, A(), A("M5_SCREEN_DIGITS","M5_CIPHER_TERMINAL"), "Nhả", "Nhả", "Trạm giải mã 2 (đông)."),
                        D("M5_SCREEN_DIGITS", DeviceKind.Screen, V(-6f,2.5f,31.7f), P, 0, 0, A("M5_PAD_1=held","M5_PAD_2=held"), A(), "MẤT TÍN HIỆU (A..F = 0)", "MẤT TÍN HIỆU", "Màn 1. Khi cả hai pad bị giữ hiển thị: A:7 B:2 C:9 D:4 E:1 F:8. Hiện tại scene chỉ có chữ 'mất tín hiệu' tĩnh."),
                        D("M5_SCREEN_ORDER", DeviceKind.Screen, V(0f,2.5f,31.7f), P, 0, 0, A(), A(), "Luôn hiển thị: A>B>C>D>E>F", "Luôn hiển thị", "Màn 2: thứ tự đọc."),
                        D("M5_SCREEN_FAULTS", DeviceKind.Screen, V(6f,2.5f,31.7f), P, 0, 0, A(), A(), "Luôn hiển thị: lỗi A B C", "Luôn hiển thị", "Màn 3: dữ liệu lỗi cần bỏ qua (A, B, C)."),
                        D("M5_CIPHER_TERMINAL", DeviceKind.Terminal, V(0f,0.9f,24.4f), P, 0, 0, A("M5_PAD_1=held","M5_PAD_2=held","code"), A("M5_DOOR_OPTICS"), "Khoá", "Khoá", "Máy tính trung tâm. Mã đúng = chữ số D,E,F theo thứ tự đọc = 418 (đã bỏ A,B,C). Dữ liệu mã phải ổn định qua các vòng của cùng lần chơi level.") },
                    pts = new[] {
                        new Pt { kind = PointKind.MonsterHome, id = "M5_SPECIMEN_HOME", pos = V(-12f,0.1f,30f), notes = "Mẫu vật phía sau kính (Specimen 06)." },
                        new Pt { kind = PointKind.Patrol, id = "M5_CELL_1", pos = V(-12f,0f,28.8f), order = 1 },
                        new Pt { kind = PointKind.Patrol, id = "M5_CELL_2", pos = V(-11.2f,0f,30f), order = 2 },
                        new Pt { kind = PointKind.Patrol, id = "M5_CELL_3", pos = V(-12.6f,0f,31.2f), order = 3 } } }
            };
        }

        [MenuItem("EchoLab/Map/Apply Handoff Markers (all maps)")]
        public static void ApplyAll()
        {
            foreach (var spec in Specs())
            {
                var scene = EditorSceneManager.OpenScene(LevelFolder + spec.scene + ".unity", OpenSceneMode.Single);
                Apply(scene, spec);
                EditorSceneManager.SaveScene(scene);
                Debug.Log("[MapFlowBuilder] Đã áp dụng marker cho " + spec.scene);
            }
        }

        private static void Apply(Scene scene, Spec spec)
        {
            foreach (var old in scene.GetRootGameObjects().Where(g => g.name == RootName).ToArray()) Object.DestroyImmediate(old);

            var root = new GameObject(RootName);
            SceneManager.MoveGameObjectToScene(root, scene);
            var info = root.AddComponent<MapInfo>();
            info.mapNumber = spec.number; info.title = spec.title; info.nextScene = spec.next;
            info.loopSeconds = 90f; info.maxEchoesNeeded = spec.echoes; info.summary = spec.summary; info.logicIsPlaceholder = true;

            // Spawn: lấy đúng vị trí "Player spawn" sẵn có của scene.
            var spawnSrc = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true)).FirstOrDefault(t => t.name == "Player spawn");
            Transform spawn = NewChild(root.transform, "SpawnPoint", spawnSrc != null ? spawnSrc.position : Vector3.zero);
            var sp = spawn.gameObject.AddComponent<MapPoint>(); sp.kind = PointKind.Spawn; sp.pointId = "M" + spec.number + "_SPAWN";
            if (spawnSrc != null) spawn.rotation = spawnSrc.rotation;

            Transform exit = NewChild(root.transform, "ExitPoint", spec.exitPos);
            var ep = exit.gameObject.AddComponent<MapPoint>(); ep.kind = PointKind.Exit; ep.pointId = "M" + spec.number + "_EXIT"; ep.nextScene = spec.next;
            var trig = exit.gameObject.AddComponent<BoxCollider>(); trig.isTrigger = true; trig.size = new Vector3(6f, 3f, 1.5f); trig.center = new Vector3(0, 1.5f, 0);

            Transform devices = NewChild(root.transform, "PuzzleDevices", Vector3.zero);
            foreach (var d in spec.devs)
            {
                var t = NewChild(devices, d.id, d.pos);
                var m = t.gameObject.AddComponent<PuzzleDeviceMarker>();
                m.deviceId = d.id; m.kind = d.kind; m.allowedActors = d.actors; m.holdSeconds = d.hold; m.activeSeconds = d.active;
                m.inputs = d.inputs; m.outputs = d.outputs; m.startState = d.start; m.resetState = d.reset; m.notes = d.notes;
                m.persistsAcrossLoops = d.id == "M1_GENERATOR" || d.id == "M1_KEYCARD";
            }

            Transform doors = NewChild(root.transform, "Doors", Vector3.zero);
            foreach (var door in spec.doors)
            {
                var t = NewChild(doors, door.id, new Vector3(0, 0, door.z));
                var blocker = NewChild(t, "DoorBlocker", new Vector3(0, 1.5f, door.z));
                var col = blocker.gameObject.AddComponent<BoxCollider>(); col.size = new Vector3(7f, 3f, 0.3f);
                var m = t.gameObject.AddComponent<DoorMarker>();
                m.doorId = door.id; m.blocker = blocker.gameObject; m.startsOpen = false; m.openWhen = door.openWhen; m.closesOnLoopReset = door.closesOnReset;
            }

            Transform patrol = NewChild(root.transform, "PatrolPoints", Vector3.zero);
            foreach (var p in spec.pts)
            {
                var t = NewChild(patrol, p.id, p.pos);
                var m = t.gameObject.AddComponent<MapPoint>(); m.kind = p.kind; m.pointId = p.id; m.order = p.order; m.notes = p.notes;
            }
        }

        private static Transform NewChild(Transform parent, string name, Vector3 worldPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = worldPos;
            return go.transform;
        }
    }
}
