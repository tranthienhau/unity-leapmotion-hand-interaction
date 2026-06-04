using UnityEngine;

namespace LeapHandInteraction.Hands
{
    /// <summary>
    /// One Euro Filter (Casiez et al. 2012) for low-latency jitter smoothing.
    /// Leap Motion palm/pinch positions are noisy frame-to-frame; raw values feel
    /// twitchy when driving an object. This filter removes jitter while keeping
    /// fast motion responsive (lag scales down as speed rises).
    /// </summary>
    public class OneEuroFilterVector3
    {
        private readonly float _minCutoff;
        private readonly float _beta;
        private readonly float _dCutoff;

        private Vector3 _prevValue;
        private Vector3 _prevDeriv;
        private bool _hasPrev;

        /// <param name="minCutoff">Lower = smoother but more lag at low speed.</param>
        /// <param name="beta">Higher = less lag at high speed (more responsive).</param>
        /// <param name="dCutoff">Cutoff for the derivative (speed) estimate.</param>
        public OneEuroFilterVector3(float minCutoff = 1.0f, float beta = 0.007f, float dCutoff = 1.0f)
        {
            _minCutoff = minCutoff;
            _beta = beta;
            _dCutoff = dCutoff;
        }

        public void Reset()
        {
            _hasPrev = false;
        }

        public Vector3 Filter(Vector3 value, float deltaTime)
        {
            if (deltaTime <= 0f) deltaTime = 1f / 90f; // Leap runs ~90-120 Hz

            if (!_hasPrev)
            {
                _prevValue = value;
                _prevDeriv = Vector3.zero;
                _hasPrev = true;
                return value;
            }

            Vector3 deriv = (value - _prevValue) / deltaTime;
            Vector3 smoothedDeriv = LowPass(deriv, _prevDeriv, Alpha(_dCutoff, deltaTime));

            float speed = smoothedDeriv.magnitude;
            float cutoff = _minCutoff + _beta * speed;

            Vector3 result = LowPass(value, _prevValue, Alpha(cutoff, deltaTime));

            _prevValue = result;
            _prevDeriv = smoothedDeriv;
            return result;
        }

        private static Vector3 LowPass(Vector3 x, Vector3 prev, float alpha)
        {
            return alpha * x + (1f - alpha) * prev;
        }

        private static float Alpha(float cutoff, float deltaTime)
        {
            float tau = 1f / (2f * Mathf.PI * cutoff);
            return 1f / (1f + tau / deltaTime);
        }
    }
}
