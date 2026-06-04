using UnityEngine;
using Leap;
using LeapHandInteraction.Hands;

namespace LeapHandInteraction.Interaction
{
    /// <summary>
    /// One interactor per hand. Each frame it reads the tracked Leap hand, detects a
    /// pinch, and on pinch grabs the nearest Grabbable, then drives that object with a
    /// smoothed pinch point so dragging an object across the screen feels intuitive.
    ///
    /// Pipeline: Leap hand -> PinchDetector -> OneEuroFilter (de-jitter) ->
    /// Grabbable.OnGrabHold (critically-damped follow) -> throw on release.
    /// </summary>
    public class HandGrabInteractor
    {
        public Chirality WhichHand { get; }

        private readonly PinchDetector _pinch;
        private readonly OneEuroFilterVector3 _posFilter;
        private readonly float _grabRadius;

        private Grabbable _held;
        private Grabbable _hovered;
        private Vector3 _grabOffset;
        private Quaternion _rotOffset;

        public HandGrabInteractor(Chirality whichHand, float grabRadius = 0.06f)
        {
            WhichHand = whichHand;
            _grabRadius = grabRadius;
            _pinch = new PinchDetector(whichHand);
            _posFilter = new OneEuroFilterVector3(minCutoff: 1.2f, beta: 0.02f);
        }

        public void Update(Hand hand, float deltaTime)
        {
            if (hand == null)
            {
                ClearHover();
                Release();
                _posFilter.Reset();
                return;
            }

            _pinch.Update(hand);
            Vector3 rawPinch = _pinch.PinchPosition;
            Vector3 smoothedPinch = _posFilter.Filter(rawPinch, deltaTime);
            Quaternion handRot = hand.Rotation;

            if (_held == null)
            {
                UpdateHover(smoothedPinch);
                if (_pinch.IsPinching && _hovered != null)
                    Grab(_hovered, smoothedPinch, handRot);
            }
            else
            {
                if (!_pinch.IsPinching)
                {
                    Release();
                }
                else
                {
                    Vector3 target = smoothedPinch + handRot * _grabOffset;
                    Quaternion targetRot = handRot * _rotOffset;
                    _held.OnGrabHold(target, targetRot, deltaTime);
                }
            }
        }

        private void UpdateHover(Vector3 point)
        {
            Grabbable nearest = FindNearest(point, _grabRadius);
            if (nearest != _hovered)
            {
                if (_hovered != null) _hovered.SetHover(false);
                _hovered = nearest;
                if (_hovered != null) _hovered.SetHover(true);
            }
        }

        private void ClearHover()
        {
            if (_hovered != null) _hovered.SetHover(false);
            _hovered = null;
        }

        private void Grab(Grabbable g, Vector3 pinchPoint, Quaternion handRot)
        {
            _held = g;
            ClearHover();
            // Preserve the grab pose so the object doesn't snap to the pinch center.
            _grabOffset = Quaternion.Inverse(handRot) * (g.transform.position - pinchPoint);
            _rotOffset = Quaternion.Inverse(handRot) * g.transform.rotation;
            g.OnGrabBegin();
        }

        private void Release()
        {
            if (_held != null)
            {
                _held.OnGrabEnd();
                _held = null;
            }
        }

        private static Grabbable FindNearest(Vector3 point, float radius)
        {
            Collider[] hits = Physics.OverlapSphere(point, radius);
            Grabbable best = null;
            float bestDist = float.MaxValue;
            foreach (Collider c in hits)
            {
                Grabbable g = c.GetComponentInParent<Grabbable>();
                if (g == null || g.IsGrabbed) continue;
                float d = (g.transform.position - point).sqrMagnitude;
                if (d < bestDist)
                {
                    bestDist = d;
                    best = g;
                }
            }
            return best;
        }
    }
}
