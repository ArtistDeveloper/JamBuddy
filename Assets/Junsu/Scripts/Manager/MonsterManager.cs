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

