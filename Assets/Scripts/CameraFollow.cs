using UnityEngine;

namespace CrystalQuest
{
    /// <summary>
    /// Smoothly follows the player character from a slightly elevated angle.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField]
        private Vector3 offset = new Vector3(0f, 8f, -10f);

        [SerializeField]
        private float positionLerpSpeed = 4.5f;

        [SerializeField]
        private float rotationLerpSpeed = 6f;

        private Transform target;

        public void SetTarget(Transform followTarget)
        {
            target = followTarget;
            if (target != null)
            {
                transform.position = target.position + offset;
                transform.LookAt(target);
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, positionLerpSpeed * Time.deltaTime);

            Quaternion desiredRotation = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationLerpSpeed * Time.deltaTime);
        }
    }
}
