using UnityEngine;

namespace EchoLab.Gameplay
{
    /// <summary>World text uses depth-tested URP rendering, and follows the dynamic font atlas.</summary>
    [ExecuteAlways]
    public sealed class WorldSignage : MonoBehaviour
    {
        private TextMesh[] labels;
        private Texture atlas;
        private MaterialPropertyBlock block;
        private void OnEnable()
        {
            labels = FindObjectsByType<TextMesh>(FindObjectsSortMode.None);
            block = new MaterialPropertyBlock();
            foreach (var label in labels) label.font.RequestCharactersInTexture(label.text, label.fontSize, label.fontStyle);
            RefreshAtlas();
            Font.textureRebuilt += OnFontRebuilt;
        }
        private void OnFontRebuilt(Font font) { if (labels != null && labels.Length > 0 && labels[0] != null && font == labels[0].font) RefreshAtlas(); }
        private void OnDisable() { Font.textureRebuilt -= OnFontRebuilt; }
        private void LateUpdate()
        {
            if (labels != null && labels.Length > 0 && labels[0] != null && atlas != labels[0].font.material.mainTexture) RefreshAtlas();
            if (Application.isPlaying) FaceReader();
        }

        /// <summary>
        /// A TextMesh is only readable from the side its back faces (it mirrors when seen from behind). Wall signs are built to face one way, but
        /// players reach them from either side, so each upright sign turns 180 degrees to the side the player is on. Floor and ceiling marks keep their orientation.
        /// </summary>
        private void FaceReader()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                var player = FindFirstObjectByType<EchoLab.Player.FirstPersonController>();
                if (player != null) camera = player.GetComponentInChildren<Camera>();
            }
            if (camera == null || labels == null) return;
            Vector3 eye = camera.transform.position;
            foreach (var label in labels)
            {
                if (label == null) continue;
                var t = label.transform;
                if (Mathf.Abs(t.forward.y) > 0.5f) continue;
                Vector3 toEye = eye - t.position; toEye.y = 0f;
                float distance = toEye.magnitude;
                if (distance < 0.05f) continue;
                // Readable when the viewer is on the -forward side; flip only when clearly behind to avoid flicker on the plane.
                if (Vector3.Dot(t.forward, toEye) > 0.15f * distance) t.Rotate(0f, 180f, 0f, Space.World);
            }
        }
        private void RefreshAtlas()
        {
            if (labels == null || labels.Length == 0) return;
            atlas = labels[0].font.material.mainTexture;
            block.SetTexture("_MainTex", atlas);
            foreach (var label in labels) if (label != null) label.GetComponent<Renderer>().SetPropertyBlock(block);
        }
    }
}
