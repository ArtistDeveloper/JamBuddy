using UnityEngine;

namespace Jambuddy.Junsu
{
    public class Player : MonoBehaviour
    {
        public int health = 100;

        public void TakeDamage(int damage)
        {
            health -= damage;
            Debug.Log($"Health: {health}");

            if (health <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.LogWarning("Player Died");
        }
    }
}
