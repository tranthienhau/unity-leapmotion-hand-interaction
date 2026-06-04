using UnityEngine;
using Leap;

namespace LeapHandInteraction.Hands
{
    /// <summary>
    /// Detects a pinch (thumb tip to index tip) on a single Leap hand and exposes
    /// a stable pinch point. Uses hysteresis (separate enter/exit thresholds) so a
    /// hand hovering near the threshold doesn't rapidly toggle grab on/off.
    /// </summary>
    public class PinchDetector
    {
        // Distance in meters between thumb and index tips.
        public float PinchActivateDistance = 0.025f; // close to pinch
        public float PinchDeactivateDistance = 0.045f; // release a bit wider (hysteresis)

        public bool IsPinching { get; private set; }
        public Vector3 PinchPosition { get; private set; }
        public float PinchStrength { get; private set; }

        public Chirality Whichhand { get; }

        public PinchDetector(Chirality whichHand)
        {
            Whichhand = whichHand;
        }

        public void Update(Hand hand)
        {
            if (hand == null)
            {
                IsPinching = false;
                PinchStrength = 0f;
                return;
            }

            Vector3 thumbTip = hand.GetThumb().TipPosition;
            Vector3 indexTip = hand.GetIndex().TipPosition;
            float dist = Vector3.Distance(thumbTip, indexTip);

            // Leap also gives a built-in PinchStrength [0..1]; blend both signals.
            PinchStrength = Mathf.Clamp01(hand.PinchStrength);
            PinchPosition = Vector3.Lerp(thumbTip, indexTip, 0.5f);

            float threshold = IsPinching ? PinchDeactivateDistance : PinchActivateDistance;
            bool distancePinch = dist <= threshold;
            bool strengthPinch = IsPinching ? PinchStrength > 0.6f : PinchStrength > 0.8f;

            IsPinching = distancePinch || strengthPinch;
        }
    }
}
