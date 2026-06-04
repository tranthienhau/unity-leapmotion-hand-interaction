using UnityEngine;
using LeapHandInteraction.Interaction;

namespace LeapHandInteraction.Demo
{
    /// <summary>
    /// Builds the 3 demo "samples" the brief asks for: a few grabbable objects the
    /// user can move on screen with the Leap Motion controller. Swap the primitive
    /// for the client-provided 3D assets by assigning a prefab.
    /// </summary>
    public class SampleSceneSpawner : MonoBehaviour
    {
        [Tooltip("Optional: client-provided 3D asset. If null, spawns physics cubes.")]
        [SerializeField] private GameObject _assetPrefab;

        [SerializeField] private int _count = 3;
        [SerializeField] private float _spacing = 0.12f;
        [SerializeField] private Vector3 _origin = new Vector3(-0.12f, 0.1f, 0.25f);

        private void Start()
        {
            for (int i = 0; i < _count; i++)
            {
                Vector3 pos = _origin + new Vector3(i * _spacing, 0f, 0f);
                GameObject go = _assetPrefab != null
                    ? Instantiate(_assetPrefab, pos, Quaternion.identity)
                    : MakeCube(pos);
                go.name = $"Sample_{i + 1}";
                EnsureGrabbable(go);
            }
        }

        private static GameObject MakeCube(Vector3 pos)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.localScale = Vector3.one * 0.06f;
            cube.transform.position = pos;
            Rigidbody rb = cube.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.mass = 0.2f;
            return cube;
        }

        private static void EnsureGrabbable(GameObject go)
        {
            if (go.GetComponent<Collider>() == null)
                go.AddComponent<BoxCollider>();
            if (go.GetComponent<Grabbable>() == null)
                go.AddComponent<Grabbable>();
        }
    }
}
