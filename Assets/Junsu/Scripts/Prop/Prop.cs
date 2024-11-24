using System;
using UnityEngine;

namespace Jambuddy.Junsu
{
    public class Prop : MonoBehaviour
    {
        public bool isOppositeMoving;
        public bool isRotation;
        public int opposite_damage = 10;
        public int rotation_damage = 10;

        private void OnCollisionEnter(Collision other)
        {
            if (other.collider.CompareTag("Monster"))
            {
                if (isOppositeMoving)
                {
                    other.transform.GetComponent<Monster>().TakeDamage(opposite_damage);
                }

                if (isRotation)
                {
                    other.transform.GetComponent<Monster>().TakeDamage(rotation_damage);
                }
            }
        }
    }
}
