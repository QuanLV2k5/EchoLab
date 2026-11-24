ECHO LAB
Unity 6000.3.25f1 | URP 17.3.0 | Input System 1.20.0

MỞ PROJECT SAU KHI CLONE
1. Unity Hub > Add project from disk: chọn thư mục có Assets, Packages, ProjectSettings.
2. Mở bằng đúng Unity 6000.3.25f1 và chờ import hoàn tất.
3. Thử cơ chế: Assets/_EchoLab/Scenes/Tests/FpsTest.unity.
4. Năm map hiện tại: Assets/_EchoLab/Scenes/Levels/Level01_TinhThuc … Level05_NhungConSoBiCheGiau.unity (xem mục NĂM MAP bên dưới).
   Level01.unity là bản sàn thử CŨ, không phải map gameplay. FpsTest.unity chỉ để thử controller.
5. Play và nhấp vào Game view: WASD di chuyển, Shift trái chạy, chuột nhìn, F đèn pin.
   Esc thả chuột; nhấp trái để điều khiển tiếp.

PHÂN CHIA CÔNG VIỆC
- Lập trình viên phụ trách Scripts/Player, Prefabs/Player và scene FpsTest.
- Thành viên thiết kế map phụ trách Level01 và bố trí môi trường, câu đố.
- Năm map Level01_TinhThuc … Level05 đã có bố cục; Level01.unity (sàn thử cũ) giữ lại làm tham chiếu.
- Tránh cùng sửa một scene hoặc cùng Apply một prefab trong cùng thời điểm.

CẤU TRÚC Assets/_EchoLab
Scripts/Player                 FirstPersonController.cs và FlashlightController.cs
Scenes/Tests                   Scene thử cơ chế
Scenes/Levels                  Scene thiết kế map
Prefabs/Player                 PF_Player và PF_FlashlightViewmodel
Prefabs/Environment/Structure   Tường, sàn, trần, cửa, cầu thang
Prefabs/Environment/Props       Giường, ghế, tủ, máy phát điện
Prefabs/Environment/Lighting    Đèn và biển chỉ dẫn
Prefabs/Gameplay                Visual và collider cho thiết bị câu đố
Prefabs/Items                   Đèn pin để nhặt và thẻ khoá
Prefabs/Characters              Người, Time Echo và quái vật
Art/Materials                  Material dùng chung
Animations/Controllers         Animator Controller
Assets/ThirdParty              Model, texture và giấy phép gốc

QUY ƯỚC
- File C# và class trùng tên, dùng PascalCase; namespace EchoLab.Player.
- Biến private dùng camelCase; field cần chỉnh trong Inspector có SerializeField.
- Scene dùng PascalCase, prefab dùng PF_ và PascalCase.
- Đổi tên/di chuyển asset trong Unity Project window để đi kèm .meta.
- Commit cả asset và .meta. Giữ Force Text và Visible Meta Files.
- Giữ nguyên tên và giấy phép file nguồn trong ThirdParty.

NĂM MAP (feature/map-flow-redesign)
Scene                              Nội dung
Level01_TinhThuc                   Phục hồi nguồn, thẻ, bàn đạp Echo, báo động, máy tính đồng bộ
Level02_DongDienCuoiCung           Ba relay A/B/C, mỗi relay chỉ giữ 8 giây
Level03_TiengGoiTrongBongToi       Hai loa dụ quái, thẻ, đầu đọc, cửa an ninh
Level04_BuongCachLy                Cửa ngoài / buồng / cửa trong, bàn đạp A và B, bảng Seal
Level05_NhungConSoBiCheGiau        Hai trạm giải mã, ba màn hình, mã 418

PHÍM KHI ĐI THỬ MAP (MapNavigator – chỉ để walkthrough, KHÔNG phải tiến trình game)
- 1–5 đổi map; R tải lại map hiện tại.
- O (DEBUG) tắt/bật collider chắn mọi cửa để đi dạo. Không tính là giải câu đố.

TRẠNG THÁI THẬT (cập nhật theo kiểm tra trong Unity, xem tài liệu .docx)
ĐÃ CÓ: bố cục 5 map, collider chắn 9 cánh cửa, NavMesh đã bake, điểm xuất phát/lối ra, nhóm marker '00 • HANDOFF'
       (SpawnPoint, ExitPoint, PuzzleDevices, Doors, PatrolPoints, MapInfo) với ID duy nhất, công cụ kiểm tra.
PLACEHOLDER: mọi câu đố, cửa mở/đóng, relay, loa, đầu đọc, máy tính, Echo, TimeLoop, AI quái, chuyển màn, menu, hướng dẫn, kết thúc
       CHƯA có logic. Marker chỉ chứa dữ liệu để thành viên lập trình nối vào.
CHƯA KIỂM CHỨNG: chạy Play Mode toàn tuyến, hành vi quái, hiệu năng, thời gian đo bằng chơi thật.

CÔNG CỤ EDITOR (Assets/_EchoLab/Editor/MapTools)
- Menu EchoLab > Map > Apply Handoff Markers (all maps): dựng lại nhóm marker.
- Menu EchoLab > Map > Validate All Maps: kiểm tra Player, AudioListener, Missing, ID trùng, collider cửa, NavMesh.
- Đặc tả luồng, lời giải, timeline và bảng bàn giao: Docs/ECHO_LAB_Dac_ta_luong_5_map_va_ban_giao.docx (nằm ngoài Assets nên Unity không import).

BẮT ĐẦU LÀM MAP
1. Tạo nhánh riêng từ nhánh map đang dùng rồi mở scene cần sửa.
2. Kéo prefab Environment vào object Environment, dùng grid snap và tỉ lệ 1 unit = 1 m.
3. Sàn/tường cần Collider đặc, Is Trigger tắt. Thử cửa với Player cao 1.8 m, bán kính 0.3 m.
4. Chỉ giữ một PF_Player và một AudioListener hoạt động.
5. Bổ sung biên cho map: sàn thử có mép mở nên đi ra ngoài sẽ rơi.
6. Thiết bị Gameplay hiện chưa có logic câu đố; trao đổi vị trí với thành viên lập trình.

GIT VÀ GIẤY PHÉP
- Repo chứa Assets, Packages, ProjectSettings và các file cấu hình tại root.
- Library, Temp, Logs, UserSettings và file IDE đã được .gitignore loại khỏi Git.
- Unity tự tạo lại Library trên máy người clone. Các file hiện tại không bắt buộc Git LFS.
- Trước khi làm: git pull. Sau khi làm: lưu scene, xem git diff, commit và push.
- Dùng GitHub PRIVATE và thêm bạn cùng nhóm làm collaborator theo điều khoản asset.
- Đọc Assets/ThirdParty/HorrorCollection/THIRD_PARTY_NOTICES.txt và LICENSE từng bộ.

Tài liệu Word và báo cáo của nhóm lưu trong Exercise bên ngoài Unity project.
