using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Jambuddy.Junsu
{
    public class Monster : MonoBehaviour
    {
        private Transform _target;
        private NavMeshAgent _agent;

        public int health = 100;
        public float moveSpeed;
        public int attackPower = 1;

        private bool _canAttack = true;

        protected virtual void Start()
        {
            _target = GameObject.FindGameObjectWithTag("Player").transform;
            _agent = GetComponent<NavMeshAgent>();
        }

        protected virtual void Update()
        {
            _agent.SetDestination(_target.position);
        }

        public void TakeDamage(int damage)
        {
            health -= damage;

            if (health <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            // 오브젝트 풀링 등을 처리
            gameObject.SetActive(false);
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject.CompareTag("Player"))
            {
                Player player = collider.gameObject.GetComponent<Player>();
                if (player != null && _canAttack)
                {
                    player.TakeDamage(attackPower);
                    StartCoroutine(AttackCooldown());
                }
            }
        }

        private IEnumerator AttackCooldown()
        {
            _canAttack = false;
            yield return new WaitForSeconds(5f);
            _canAttack = true;
        }
    }
}

