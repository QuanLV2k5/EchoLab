using UnityEngine;

namespace EchoLab.Gameplay
{
    public sealed class AtmospherePulse : MonoBehaviour
    {
        public Light target;
        public float baseIntensity = 4f;
        public bool unstable;
        public float seed;
        private void Update()
        {
            if (target == null) return;
            float wave = 0.96f + 0.04f * Mathf.Sin(Time.unscaledTime * 1.8f + seed);
            if (unstable && Mathf.PerlinNoise(seed, Time.unscaledTime * 5) > 0.73f) wave *= 0.2f;
            target.intensity = baseIntensity * wave;
        }
    }
}
