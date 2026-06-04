using UnityEngine;

namespace LeapHandInteraction.Interaction
{
    /// <summary>
    /// Marks an object as grabbable and stores per-object interaction state.
    /// Attach to any 3D asset (cube, prop, the client's provided models) that the
    /// user should be able to pick up and move with a Leap pinch.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Grabbable : MonoBehaviour
    {
        [Tooltip("If the object has a Rigidbody, it is moved with velocity for natural throw/release.")]
        public bool useThrowOnRelease = true;

        [Tooltip("Highlight color when a hand is hovering and can grab.")]
        public Color hoverColor = new Color(0.3f, 0.8f, 1f);

        public bool IsGrabbed { get; private set; }

        private Rigidbody _rb;
        private Renderer _renderer;
        private Color _baseColor;
        private bool _hasBaseColor;

        // Velocity estimate kept for throw-on-release.
        private Vector3 _lastPosition;
        private Vector3 _estimatedVelocity;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _renderer = GetComponentInChildren<Renderer>();
            if (_renderer != null && _renderer.material.HasProperty("_Color"))
            {
                _baseColor = _renderer.material.color;
                _hasBaseColor = true;
            }
            _lastPosition = transform.position;
        }

        public void SetHover(bool hovering)
        {
            if (IsGrabbed || !_hasBaseColor) return;
            _renderer.material.color = hovering ? hoverColor : _baseColor;
        }

        public void OnGrabBegin()
        {
            IsGrabbed = true;
            if (_rb != null)
            {
                _rb.isKinematic = true;
                _rb.interpolation = RigidbodyInterpolation.Interpolate;
            }
            _lastPosition = transform.position;
            _estimatedVelocity = Vector3.zero;
        }

        /// <summary>Move the object to the smoothed target pose set by the hand.</summary>
        public void OnGrabHold(Vector3 targetPos, Quaternion targetRot, float deltaTime)
        {
            // Critically-damped move toward the target keeps motion buttery even if
            // the source point still has micro-jitter after filtering.
            Vector3 newPos = Vector3.Lerp(transform.position, targetPos, 1f - Mathf.Exp(-25f * deltaTime));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 1f - Mathf.Exp(-20f * deltaTime));

            if (deltaTime > 0f)
                _estimatedVelocity = (newPos - _lastPosition) / deltaTime;
            _lastPosition = newPos;
            transform.position = newPos;
        }

        public void OnGrabEnd()
        {
            IsGrabbed = false;
            if (_rb != null)
            {
                _rb.isKinematic = false;
                if (useThrowOnRelease)
                    _rb.velocity = _estimatedVelocity;
            }
            if (_hasBaseColor)
                _renderer.material.color = _baseColor;
        }
    }
}
