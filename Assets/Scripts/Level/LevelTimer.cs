using System;
using UnityEngine;
namespace GlitchCompiler.Level
{
    public sealed class LevelTimer : MonoBehaviour
    {
        public float Remaining { get; private set; }
        public float Duration { get; private set; }
        public float NormalizedElapsed => Duration <= 0f ? 1f : Mathf.Clamp01(1f - Remaining / Duration);
        public bool IsRunning { get; private set; }
        public event Action<float> Changed;
        public event Action Expired;

        private float multiplier = 1f;

        public void StartTimer(float seconds)
        {
            Duration = Mathf.Max(0f, seconds);
            Remaining = Duration;
            multiplier = 1f;
            IsRunning = true;
            Changed?.Invoke(Remaining);
        }

        public void Pause(bool value) => IsRunning = !value;
        public void Stop() => IsRunning = false;
        public void Add(float seconds) { Remaining = Mathf.Max(0f, Remaining + seconds); Changed?.Invoke(Remaining); }
        public void SetMultiplier(float value) => multiplier = Mathf.Max(0f, value);

        private void Update()
        {
            if (!IsRunning) return;
            Remaining -= Time.deltaTime * multiplier;
            if (Remaining > 0f) { Changed?.Invoke(Remaining); return; }
            Remaining = 0f;
            IsRunning = false;
            Changed?.Invoke(Remaining);
            Expired?.Invoke();
        }
    }
}
