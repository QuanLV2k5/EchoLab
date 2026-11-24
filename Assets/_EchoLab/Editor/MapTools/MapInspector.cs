using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoLab.EditorTools
{
    /// <summary>Công cụ đọc scene (chỉ đọc, không sửa): xuất cấu trúc ra file text để đánh giá map. Chạy bằng -executeMethod.</summary>
    public static class MapInspector
    {
        public static void DumpAll()
        {
            string outDir = System.Environment.GetEnvironmentVariable("ECHO_OUT") ?? "Temp/MapDump";
            Directory.CreateDirectory(outDir);
            foreach (var guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets/_EchoLab/Scenes" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                File.WriteAllText(Path.Combine(outDir, scene.name + ".txt"), Dump(scene));
            }
        }

        static string Dump(Scene scene)
        {
            var sb = new StringBuilder();
            sb.AppendLine("SCENE " + scene.path);
            int total = 0;
            foreach (var root in scene.GetRootGameObjects()) total += root.GetComponentsInChildren<Transform>(true).Length;
            sb.AppendLine("TOTAL_TRANSFORMS " + total);
            foreach (var root in scene.GetRootGameObjects()) Walk(root.transform, 0, sb);
            sb.AppendLine("== LIGHTS ==");
            foreach (var l in Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                sb.AppendLine($"{PathOf(l.transform)} {l.type} int={l.intensity:0.##} range={l.range:0.#} pos={V(l.transform.position)}");
            sb.AppendLine("== TEXTMESH ==");
            foreach (var t in Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                sb.AppendLine($"{PathOf(t.transform)} pos={V(t.transform.position)} text=\"{t.text.Replace("\n", " | ")}\"");
            sb.AppendLine("== COLLIDERS (non-mesh summary) ==");
            int boxes = 0, meshc = 0, triggers = 0;
            foreach (var c in Object.FindObjectsByType<Collider>(FindObjectsInactive.Include, FindObjectsSortMode.None)) { if (c is BoxCollider) boxes++; if (c is MeshCollider) meshc++; if (c.isTrigger) triggers++; }
            sb.AppendLine($"box={boxes} mesh={meshc} triggers={triggers}");
            sb.AppendLine("== MISSING ==");
            foreach (var root in scene.GetRootGameObjects())
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                {
                    int m = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
                    if (m > 0) sb.AppendLine("MISSING_SCRIPT " + PathOf(t) + " x" + m);
                    foreach (var r in t.GetComponents<Renderer>())
                        foreach (var mat in r.sharedMaterials) if (mat == null) sb.AppendLine("NULL_MATERIAL " + PathOf(t));
                }
            return sb.ToString();
        }

        static void Walk(Transform t, int depth, StringBuilder sb)
        {
            if (depth > 2) return;
            var b = Bounds(t);
            string comps = "";
            foreach (var c in t.GetComponents<Component>()) if (c != null && !(c is Transform)) comps += c.GetType().Name + ",";
            sb.AppendLine($"{new string(' ', depth * 2)}{t.name} [{t.childCount}] pos={V(t.position)} size={V(b.size)} {(t.gameObject.activeSelf ? "" : "(inactive) ")}{comps}");
            foreach (Transform c in t) Walk(c, depth + 1, sb);
        }

        static Bounds Bounds(Transform t)
        {
            var rs = t.GetComponentsInChildren<Renderer>();
            if (rs.Length == 0) return new Bounds(t.position, Vector3.zero);
            var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds); return b;
        }
        static string V(Vector3 v) => $"({v.x:0.#},{v.y:0.#},{v.z:0.#})";
        static string PathOf(Transform t) { string p = t.name; while (t.parent != null) { t = t.parent; p = t.name + "/" + p; } return p; }
    }
}
