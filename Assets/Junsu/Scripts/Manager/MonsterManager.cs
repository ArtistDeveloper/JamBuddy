using System.Collections.Generic;
using UnityEngine;

namespace Jambuddy.Junsu
{
    public class MonsterManager : MonoBehaviour
    {
        private List<GameObject> activeMonsters = new List<GameObject>();

        public MonsterSpawner _monsterSpawner;

        public Transform spawnArea;

        private void Start()
        {
            InitMonsterSpawner(10);
            RandomSpawn(25);
        }

        // 몬스터 생성 지형 확인
        private void OnDrawGizmos()
        {
            if (spawnArea == null) return;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spawnArea.position, 25f); // 최소 거리
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(spawnArea.position, 80f); // 최대 거리
        }

        private void RandomSpawn(int repetition)
        {
            string[] names = Util.GetNamesOfEnumElement(typeof(MonsterType));
            
            for (int i = 0; i < repetition; i++)
            {
                int rand = Random.Range(0, names.Length);
                _monsterSpawner.SpawnMonster(names[rand]);
            }
        }

        private void InitMonsterSpawner(int size)
        {
            _monsterSpawner = new MonsterSpawner(spawnArea, size);
        }

        public void RegisterMonster(GameObject monster)
        {
            activeMonsters.Add(monster);
        }

        public void DeregisterMonster(GameObject monster)
        {
            activeMonsters.Remove(monster);

            Monster behavior = monster.GetComponent<Monster>();
            //spawner.ReturnToPool(, monster);
        }
    }
}

