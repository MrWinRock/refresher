using UnityEngine;

namespace Game5
{
    /// <summary>
    /// Keeps the world rotation of this object fixed to a specific value.
    /// Useful for Particle Systems using Collision2D that are children of rotating objects.
    /// </summary>
    [ExecuteAlways]
    public class KeepWorldRotation : MonoBehaviour
    {
        [SerializeField] private Vector3 worldRotation = Vector3.zero;

        private void LateUpdate()
        {
            transform.rotation = Quaternion.Euler(worldRotation);
        }
    }
}
