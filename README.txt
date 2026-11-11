ECHO LAB
Unity 6000.3.25f1 | URP 17.3.0 | Input System 1.20.0

MỞ PROJECT SAU KHI CLONE
1. Unity Hub > Add project from disk: chọn thư mục có Assets, Packages, ProjectSettings.
2. Mở bằng đúng Unity 6000.3.25f1 và chờ import hoàn tất.
3. Thử cơ chế: Assets/_EchoLab/Scenes/Tests/FpsTest.unity.
4. Thiết kế map: Assets/_EchoLab/Scenes/Levels/Level01.unity.
5. Play và nhấp vào Game view: WASD di chuyển, Shift trái chạy, chuột nhìn, F đèn pin.
   Esc thả chuột; nhấp trái để điều khiển tiếp.

PHÂN CHIA CÔNG VIỆC
- Lập trình viên phụ trách Scripts/Player, Prefabs/Player và scene FpsTest.
- Thành viên thiết kế map phụ trách Level01 và bố trí môi trường, câu đố.
- Level01 hiện có sàn, ánh sáng thử và Player để bắt đầu dựng map.
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

BẮT ĐẦU LÀM MAP
1. Tạo nhánh feature/level01-layout rồi mở Level01.
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
