using System;
using UnityEngine;

namespace Jambuddy.Junsu
{
    public class Prop : MonoBehaviour
    {
        public bool isOppositeMoving;
        public int damage = 10;

        private void OnCollisionEnter(Collision other)
        {
            if (other.collider.CompareTag("Monster"))
            {
                if (isOppositeMoving)
                {
                    other.transform.GetComponent<Monster>().TakeDamage(damage);
                }
            }
        }
    }
}
