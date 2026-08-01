using UnityEngine;

namespace Tidepool.Runtime
{
    [RequireComponent(typeof(Camera))]
    public class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
        [SerializeField, Min(0f)] private float smoothTime = 0.18f;

        private Vector3 velocity;

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            velocity = Vector3.zero;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 desiredPosition = target.position + offset;
            transform.position = smoothTime <= 0f
                ? desiredPosition
                : Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
        }
    }
}
