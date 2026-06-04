using UnityEngine;
using Leap;

namespace LeapHandInteraction.Interaction
{
    /// <summary>
    /// Scene entry point. Pulls the latest frame from the Leap service each Update,
    /// feeds left/right hands to their interactors, and lets the user pinch-grab and
    /// move any Grabbable object on screen with smooth, intuitive hand control.
    ///
    /// Requires a LeapServiceProvider in the scene (from the Ultraleap Unity package,
    /// com.ultraleap.tracking). Assign it in the inspector or it is found at runtime.
    /// </summary>
    public class LeapInteractionManager : MonoBehaviour
    {
        [SerializeField] private LeapServiceProvider _leapProvider;

        [Header("Grab tuning")]
        [Tooltip("How close the pinch point must be to a Grabbable to pick it up (meters).")]
        [SerializeField] private float _grabRadius = 0.06f;

        private HandGrabInteractor _left;
        private HandGrabInteractor _right;

        private void Awake()
        {
            if (_leapProvider == null)
                _leapProvider = FindObjectOfType<LeapServiceProvider>();

            _left = new HandGrabInteractor(Chirality.Left, _grabRadius);
            _right = new HandGrabInteractor(Chirality.Right, _grabRadius);
        }

        private void Update()
        {
            if (_leapProvider == null) return;

            Frame frame = _leapProvider.CurrentFrame;
            float dt = Time.deltaTime;

            Hand leftHand = frame.GetHand(Chirality.Left);
            Hand rightHand = frame.GetHand(Chirality.Right);

            _left.Update(leftHand, dt);
            _right.Update(rightHand, dt);
        }
    }
}
