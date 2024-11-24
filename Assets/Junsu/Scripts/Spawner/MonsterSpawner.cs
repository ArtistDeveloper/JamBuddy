using MoreMountains.Feedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

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

        private Vector3 _spawnArea;

        private Dictionary<string, Queue<GameObject>> poolDictionary;
        
        private int _poolSize = 20;

        private const int GROWTH = 10;

        private const float _SPWAN_HEIGNT = 3f;

        private float _MIN_DISTANCE = 25f;

        private float _MAX_DISTANCE = 80f;

        public MonsterSpawner(int poolSize)
        {
            GameObject go = UnityEngine.GameObject.FindGameObjectWithTag("Player");
            if (go != null)
            {
                _spawnArea = go.transform.position;
            }
            else
            {
                _spawnArea = new Vector3(0f, 0f, 0f);
            }
            
            _poolSize = poolSize;
            Init();
        }

        public MonsterSpawner(Vector3 spawnArea, int poolSize)
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

        public void OnUpdate()
        {
            GameObject go = UnityEngine.GameObject.FindGameObjectWithTag("Player");
            if (go != null)
                _spawnArea = go.transform.position;
        }

        private void GetResource()
        {
            string[] names = Util.GetNamesOfEnumElement(typeof(MonsterType));
            for (int i = 0; i < names.Length; i++)
            {
                _monsterPrefabs.Add(Resources.Load<GameObject>($"Junsu/Prefabs/Monster/{names[i]}"));
            }
        }

        // 몬스터를 각 종류별로 _poolSize만큼 생성한다.
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

        public void RandomSpawn(int repetition)
        {
            string[] names = Util.GetNamesOfEnumElement(typeof(MonsterType));

            for (int i = 0; i < repetition; i++)
            {
                int rand = UnityEngine.Random.Range(0, names.Length);
                SpawnMonster(names[rand]);
            }
        }

        private IEnumerator RandomSpawnRoutine(float wait)
        {
            string[] names = Util.GetNamesOfEnumElement(typeof(MonsterType));

            while (true)
            {
                yield return new WaitForSeconds(wait);
                int rand = UnityEngine.Random.Range(0, names.Length);
                SpawnMonster(names[rand]);
            }
        }

        public GameObject SpawnMonster(string monsterType)
        {
            if (!poolDictionary.ContainsKey(monsterType))
            {
                Debug.LogWarning($"해당하는 몬스터 타입이 없습니다: {monsterType}");
                return null;
            }
            if (poolDictionary[monsterType].Count == 0)
            {
                for (int i = 0; i < GROWTH; i++)
                {
                    GameObject go = UnityEngine.Object.Instantiate(Resources.Load<GameObject>($"Junsu/Prefabs/Monster/{monsterType}"));
                    go.SetActive(false);
                    poolDictionary[monsterType].Enqueue(go);
                }
            }

            GameObject monster = poolDictionary[monsterType].Dequeue();
            monster.SetActive(true);

            // 랜덤 각도와 거리 생성
            float angle = UnityEngine.Random.Range(0f, 360f);
            float radius = UnityEngine.Random.Range(_MIN_DISTANCE, _MAX_DISTANCE);

            // 원형 좌표 계산
            float x = radius * Mathf.Cos(angle * Mathf.Deg2Rad);
            float z = radius * Mathf.Sin(angle * Mathf.Deg2Rad);

            Vector3 spawnPosition = new Vector3(x, _SPWAN_HEIGNT, z);
            monster.transform.position = _spawnArea + spawnPosition;

            return monster;
        }

        public void ReturnToPool(string monsterType, GameObject monster)
        {
            if (!poolDictionary.ContainsKey(monsterType))
            {
                Debug.LogError($"해당하는 몬스터 타입이 없습니다: {monsterType}");
                UnityEngine.Object.Destroy(monster); // 풀이 없는 경우 파괴 처리
                return;
            }

            monster.SetActive(false); // 비활성화 처리
            poolDictionary[monsterType].Enqueue(monster); // 풀에 다시 추가
        }
    }
}
