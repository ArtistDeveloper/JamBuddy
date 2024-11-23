using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jambuddy.Junsu
{
    public enum MonsterType
    {
        MonsterCapsule,
        MonsterCube,
    }

    public class MonsterSpawner
    {
        private List<GameObject> _monsterPrefabs = new List<GameObject>();

        private Transform _spawnArea;

        public int _poolSize = 20;

        private Dictionary<string, Queue<GameObject>> poolDictionary;

        public MonsterSpawner(Transform spawnArea, int poolSize)
        {
            _spawnArea = spawnArea;
            _poolSize = poolSize;
            Init();
        }

        public void Init()
        {
            GetResource();
            InitializePools();
        }

        private void GetResource()
        {
            string[] names = Util.GetNamesOfEnumElement(typeof(MonsterType));
            for (int i = 0; i < names.Length; i++)
            {
                _monsterPrefabs.Add(Resources.Load<GameObject>($"Junsu/Prefabs/Monster/{names[i]}"));
            }
        }

        private void InitializePools()
        {
            poolDictionary = new Dictionary<string, Queue<GameObject>>();

            foreach (var prefab in _monsterPrefabs)
            {
                Queue<GameObject> objectPool = new Queue<GameObject>();

                for (int i = 0; i < _poolSize; i++)
                {
                    GameObject go = UnityEngine.Object.Instantiate(prefab);
                    go.SetActive(false);
                    objectPool.Enqueue(go);
                }

                poolDictionary.Add(prefab.name, objectPool);
            }
        }

        public GameObject SpawnMonster(string monsterType)
        {
            if (!poolDictionary.ContainsKey(monsterType) || poolDictionary[monsterType].Count == 0)
            {
                Debug.LogWarning($"No pool available for monster type: {monsterType}");
                return null;
            }

            GameObject monster = poolDictionary[monsterType].Dequeue();
            monster.SetActive(true);

            Vector3 spawnPosition = new Vector3(
                UnityEngine.Random.Range(-5f, 5f),
                0f,
                UnityEngine.Random.Range(-5f, 5f)
            );
            monster.transform.position = _spawnArea.position + spawnPosition;

            return monster;
        }

        public void ReturnToPool(string monsterType, GameObject monster)
        {
            if (!poolDictionary.ContainsKey(monsterType))
            {
                Debug.LogError($"No pool exists for monster type: {monsterType}");
                UnityEngine.Object.Destroy(monster); // 풀이 없는 경우 파괴 처리
                return;
            }

            monster.SetActive(false); // 비활성화 처리
            poolDictionary[monsterType].Enqueue(monster); // 풀에 다시 추가
        }
    }

}
