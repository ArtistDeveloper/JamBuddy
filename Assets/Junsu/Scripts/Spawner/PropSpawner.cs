using UnityEngine;


using MoreMountains.Feedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace Jambuddy.Junsu
{
    public enum PropType
    {
        Barrel,
    }

    public class PropSpawner
    {
        private List<GameObject> _PropPrefabs = new List<GameObject>();

        private Vector3 _spawnArea;

        private Dictionary<string, Queue<GameObject>> _poolDictionary;

        private int _poolSize = 20;

        private const int GROWTH = 10;

        private const float _SPWAN_HEIGNT = 3f;

        private float _MIN_DISTANCE = 5;

        private float _MAX_DISTANCE = 15;

        public PropSpawner(int poolSize)
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

        public PropSpawner(Vector3 spawnArea, int poolSize)
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

        public void Update()
        {
            GameObject go = UnityEngine.GameObject.FindGameObjectWithTag("Player");
            if (go != null)
                _spawnArea = go.transform.position;
        }

        private void GetResource()
        {
            string[] names = Util.GetNamesOfEnumElement(typeof(PropType));
            for (int i = 0; i < names.Length; i++)
            {
                _PropPrefabs.Add(Resources.Load<GameObject>($"Junsu/Prefabs/Prop/{names[i]}"));
            }
        }

        private void InitializePools()
        {
            _poolDictionary = new Dictionary<string, Queue<GameObject>>();

            foreach (var prefab in _PropPrefabs)
            {
                Queue<GameObject> objectPool = new Queue<GameObject>();

                for (int i = 0; i < _poolSize; i++)
                {
                    GameObject go = UnityEngine.Object.Instantiate(prefab);
                    go.SetActive(false);
                    objectPool.Enqueue(go);
                }

                if (!_poolDictionary.ContainsKey(prefab.name))
                {
                    _poolDictionary.Add(prefab.name, objectPool);
                }
            }
        }

        public void RandomSpawn(int repetition)
        {
            string[] names = Util.GetNamesOfEnumElement(typeof(PropType));

            for (int i = 0; i < repetition; i++)
            {
                int rand = UnityEngine.Random.Range(0, names.Length);
                SpawnProp(names[rand]);
            }
        }

        private IEnumerator RandomSpawnRoutine(float wait)
        {
            string[] names = Util.GetNamesOfEnumElement(typeof(PropType));

            while (true)
            {
                yield return new WaitForSeconds(wait);
                int rand = UnityEngine.Random.Range(0, names.Length);
                SpawnProp(names[rand]);
            }
        }

        public GameObject SpawnProp(string propType)
        {
            if (!_poolDictionary.ContainsKey(propType))
            {
                Debug.LogWarning($"해당하는 프롭 타입이 없습니다: {propType}");
                return null;
            }
            if (_poolDictionary[propType].Count == 0)
            {
                for (int i = 0; i < GROWTH; i++)
                {
                    GameObject go = UnityEngine.Object.Instantiate(Resources.Load<GameObject>($"Junsu/Prefabs/Prop/{propType}"));
                    go.SetActive(false);
                    _poolDictionary[propType].Enqueue(go);
                }
            }

            GameObject prop = _poolDictionary[propType].Dequeue();
            prop.SetActive(true);

            // 랜덤 각도와 거리 생성
            float angle = UnityEngine.Random.Range(0f, 360f);
            float radius = UnityEngine.Random.Range(_MIN_DISTANCE, _MAX_DISTANCE);

            // 원형 좌표 계산
            float x = radius * Mathf.Cos(angle * Mathf.Deg2Rad);
            float z = radius * Mathf.Sin(angle * Mathf.Deg2Rad);

            Vector3 spawnPosition = new Vector3(x, _SPWAN_HEIGNT, z);
            prop.transform.position = _spawnArea + spawnPosition;

            return prop;
        }

        public void ReturnToPool(string propType, GameObject prop)
        {
            if (!_poolDictionary.ContainsKey(propType))
            {
                Debug.LogError($"해당하는 프롭 타입이 없습니다: {propType}");
                UnityEngine.Object.Destroy(prop); // 풀이 없는 경우 파괴 처리
                return;
            }

            prop.SetActive(false); // 비활성화 처리
            _poolDictionary[propType].Enqueue(prop); // 풀에 다시 추가
        }
    }
}
