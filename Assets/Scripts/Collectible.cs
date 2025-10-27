using UnityEngine;

namespace CrystalQuest
{
    /// <summary>
    /// Rotates the collectible and notifies the manager when the player touches it.
    /// </summary>
    public class Collectible : MonoBehaviour
    {
        [SerializeField]
        private float rotationSpeed = 55f;

        private GameManager manager;

        public void Initialize(GameManager owner)
        {
            manager = owner;
        }

        private void Update()
        {
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (manager != null && other.CompareTag("Player"))
            {
                manager.RegisterCollection(this);
            }
        }
    }
}
