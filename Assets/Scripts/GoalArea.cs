using UnityEngine;

namespace CrystalQuest
{
    /// <summary>
    /// Detects when the player reaches the exit portal once the objective is complete.
    /// </summary>
    public class GoalArea : MonoBehaviour
    {
        private GameManager manager;

        public void Initialize(GameManager owner)
        {
            manager = owner;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (manager != null && other.CompareTag("Player"))
            {
                manager.CompleteObjective();
            }
        }
    }
}
