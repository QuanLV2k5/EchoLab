using System.IO;
using System.Linq;
using System.Text;
using EchoLab.Player;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace EchoLab.EditorTools
{
    /// <summary>
    /// Kiểm tra tự động 5 scene ở Edit Mode (không cần bấm Play): Player / AudioListener, Missing Script, spawn trên sàn,
    /// NavMesh đã bake và đường đi của quái tới điểm tuần tra / nguồn âm.
    /// Điểm đặt marker được tìm theo tên trong cây Transform (ổn định hơn truy vấn component trong batch mode).
    /// Menu: EchoLab > Map > Validate All Maps. Batch: -executeMethod EchoLab.EditorTools.MapValidator.RunBatch (ghi validation.txt vào thư mục ECHO_OUT).
    /// </summary>
    public static class MapValidator
    {
        private static readonly string[] Scenes =
        {
            "Level01_TinhThuc", "Level02_DongDienCuoiCung", "Level03_TiengGoiTrongBongToi", "Level04_BuongCachLy", "Level05_NhungConSoBiCheGiau"
        };

        [MenuItem("EchoLab/Map/Validate All Maps")]
        public static void RunMenu() { Debug.Log(Validate()); }

        public static void RunBatch()
        {
            string outDir = System.Environment.GetEnvironmentVariable("ECHO_OUT") ?? "Temp/MapDump";
            Directory.CreateDirectory(outDir);
            File.WriteAllText(System.IO.Path.Combine(outDir, "validation.txt"), Validate());
        }

        private static string Validate()
        {
            var sb = new StringBuilder();
            int fails = 0;
            foreach (var name in Scenes)
            {
                var scene = EditorSceneManager.OpenScene("Assets/_EchoLab/Scenes/Levels/" + name + ".unity", OpenSceneMode.Single);
                sb.AppendLine("=== " + name);
                void Ok(string m) => sb.AppendLine("  PASS  " + m);
                void Fail(string m) { fails++; sb.AppendLine("  FAIL  " + m); }
                void Info(string m) => sb.AppendLine("  INFO  " + m);

                var all = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true)).ToArray();
                Info("Tổng số Transform: " + all.Length);

                var players = All<FirstPersonController>(scene).Where(p => p.gameObject.activeInHierarchy).ToArray();
                if (players.Length == 1) Ok("đúng 1 Player"); else Fail("số Player = " + players.Length);
                var listeners = All<AudioListener>(scene).Where(a => a.enabled && a.gameObject.activeInHierarchy).ToArray();
                if (listeners.Length == 1) Ok("đúng 1 AudioListener đang bật"); else Fail("số AudioListener = " + listeners.Length);

                int missing = 0, nullMat = 0;
                foreach (var t in all)
                {
                    missing += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
                    foreach (var r in t.GetComponents<Renderer>()) foreach (var m in r.sharedMaterials) if (m == null) nullMat++;
                }
                if (missing == 0 && nullMat == 0) Ok("không có Missing Script / material rỗng"); else Fail($"Missing script={missing}, material rỗng={nullMat}");

                var root = all.FirstOrDefault(t => t.name == "00 • HANDOFF");
                if (root == null) { Fail("thiếu nhóm 00 • HANDOFF"); continue; }
                int markerObjects = root.GetComponentsInChildren<Transform>(true).Length - 1;
                Ok($"nhóm 00 • HANDOFF có {markerObjects} object con");

                var spawn = all.FirstOrDefault(t => t.name == "SpawnPoint");
                if (spawn == null) Fail("thiếu SpawnPoint");
                else if (Ground(spawn.position, out float drop)) Ok($"SpawnPoint đứng trên sàn (cách mặt sàn {drop:0.00} m)"); else Fail("SpawnPoint không có sàn bên dưới");
                var exit = all.FirstOrDefault(t => t.name == "ExitPoint");
                if (exit == null) Fail("thiếu ExitPoint");
                else if (Ground(exit.position, out _)) Ok("ExitPoint có sàn bên dưới"); else Fail("ExitPoint không có sàn bên dưới");

                var surfaces = All<NavMeshSurface>(scene);
                bool baked = surfaces.Length > 0 && surfaces.All(s => s.navMeshData != null);
                Info($"NavMeshSurface: {surfaces.Length}, đã bake dữ liệu: {baked}");
                if (!baked) { Fail("NavMesh chưa có dữ liệu bake trong scene"); continue; }
                var tri = NavMesh.CalculateTriangulation();
                Info($"NavMesh có {tri.vertices.Length} đỉnh, {tri.indices.Length / 3} tam giác");

                var patrolRoot = all.FirstOrDefault(t => t.name == "PatrolPoints");
                var devRoot = all.FirstOrDefault(t => t.name == "PuzzleDevices");
                var points = patrolRoot != null ? patrolRoot.Cast<Transform>().ToArray() : new Transform[0];
                var home = points.FirstOrDefault(p => p.name.EndsWith("_HOME"));
                foreach (var p in points)
                    if (NavMesh.SamplePosition(p.position, out _, 0.8f, NavMesh.AllAreas)) Ok($"{p.name} nằm trên NavMesh"); else Fail($"{p.name} không nằm trên NavMesh");
                if (home != null)
                {
                    foreach (var p in points.Where(p => p != home)) NavPath(sb, home.position, p.position, $"{home.name} -> {p.name}", Fail);
                    if (devRoot != null)
                        foreach (var d in devRoot.Cast<Transform>().Where(d => d.name.Contains("_HORN_") || d.name.EndsWith("_ALARM")))
                            NavPath(sb, home.position, d.position, $"{home.name} -> {d.name} (nguồn âm)", Fail);
                }
                else Info("map này không có điểm quái đứng ban đầu (không kiểm đường quái)");
            }
            sb.AppendLine();
            sb.AppendLine(fails == 0 ? "TỔNG KẾT: không có FAIL" : "TỔNG KẾT: " + fails + " FAIL");
            return sb.ToString();
        }

        private static T[] All<T>(Scene scene) where T : Component
        {
            return scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true)).ToArray();
        }

        private static bool Ground(Vector3 p, out float drop)
        {
            drop = 0;
            if (Physics.Raycast(p + Vector3.up * 1.5f, Vector3.down, out var hit, 4f, ~0, QueryTriggerInteraction.Ignore)) { drop = p.y - hit.point.y; return true; }
            return false;
        }

        private static void NavPath(StringBuilder sb, Vector3 a, Vector3 b, string label, System.Action<string> fail)
        {
            // Mặt sàn của các map ở y = 0. Nguồn âm gắn trên cao hoặc tủ đặt trên sàn: quái đi tới điểm sàn ngay bên dưới.
            // (NavMesh còn bao cả mặt trên của trần, là một "đảo" không nối với sàn, nên không chọn điểm gần nhất theo khối cầu.)
            var target = new Vector3(b.x, 0f, b.z);
            if (!NavMesh.SamplePosition(a, out var ha, 1.5f, NavMesh.AllAreas) || !NavMesh.SamplePosition(target, out var hb, 2f, NavMesh.AllAreas)) { fail(label + ": điểm không nằm gần NavMesh"); return; }
            var path = new NavMeshPath();
            bool found = NavMesh.CalculatePath(ha.position, hb.position, NavMesh.AllAreas, path);
            float len = 0; for (int i = 1; i < path.corners.Length; i++) len += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            if (found && path.status == NavMeshPathStatus.PathComplete) sb.AppendLine($"  PASS  đường quái {label}: {len:0.0} m");
            else
            {
                var end = path.corners.Length > 0 ? path.corners[path.corners.Length - 1] : ha.position;
                fail($"{label}: đường đi {path.status}; điểm đích trên NavMesh ({hb.position.x:0.0}; {hb.position.y:0.0}; {hb.position.z:0.0}), đường đi dừng tại ({end.x:0.0}; {end.y:0.0}; {end.z:0.0}) sau {len:0.0} m");
            }
        }
    }
}
