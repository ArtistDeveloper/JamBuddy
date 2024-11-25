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
        public GameObject vfxPrefab; // Inspector에서 이 프리팹을 할당

        private void OnCollisionEnter(Collision other)
        {
            if (other.collider.CompareTag("Monster"))
            {
                if (isOppositeMoving)
                {
                    GameObject vfxInstance = Instantiate(vfxPrefab, other.transform.position, Quaternion.identity);

                    Animator animator = vfxInstance.GetComponent<Animator>();
                    if (animator != null)
                    {
                        animator.Play("OppositeAnim");
                    }
                    
                    Destroy(vfxInstance, 1.0f); // 애니메이션 길이에 맞춰서 조정

                    other.transform.GetComponent<Monster>().TakeDamage(opposite_damage);
                }

                if (isRotation)
                {
                    other.transform.GetComponent<Monster>().TakeDamage(rotation_damage);

                    GameObject vfxInstance = Instantiate(vfxPrefab, other.transform.position, Quaternion.identity);

                    Animator animator = vfxInstance.GetComponent<Animator>();
                    if (animator != null)
                    {
                        animator.Play("OppositeAnim");
                    }

                    Destroy(vfxInstance, 1.0f); // 애니메이션 길이에 맞춰서 조정
                }
            }
        }
    }
}
