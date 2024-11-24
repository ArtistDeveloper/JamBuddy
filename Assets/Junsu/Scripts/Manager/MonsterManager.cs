using System.Collections.Generic;
using UnityEngine;

namespace Jambuddy.Junsu
{
    public class MonsterManager
    {
        private List<GameObject> activeMonsters = new List<GameObject>();

        private MonsterSpawner _monSpawner;

        public MonsterSpawner MonSpawner { get { return _monSpawner; } }


        public void Init()
        {
            InitMonsterSpawner(10);
        }

        private void InitMonsterSpawner(int size)
        {
            _monSpawner = new MonsterSpawner(size);
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

