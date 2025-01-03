using System;
using System.Collections;
using Jambuddy.Junsu;
using UnityEngine;

namespace Jambuddy.Junsu
{
    public class GameManager : MonoBehaviour
    {
        private int currentStage = 0; // 현재 단계

        public float _gameTime = 0f; // 경과 시간

        private Coroutine monsterSpawnRoutine;
        private Coroutine propSpawnRoutine;

        private readonly StageInfo[] _monsterStages = new StageInfo[]
        {
            new StageInfo(0f, 120f, 6, 1f),  // 1-1 단계, 원래는 20f. 테스트를 위해 1f로 해놓았음
            new StageInfo(120f, 300f, 8, 15f), // 1-2 단계
            new StageInfo(300f, 600f, 15, 10f), // 2-1 단계
            new StageInfo(600f, 900f, 18, 8f), // 2-2 단계
            new StageInfo(900f, 1200f, 25, 6f) // 3단계
        };

        private readonly StageInfo[] _propStages = new StageInfo[]
        {
            new StageInfo(0f, 120f, 12, 5f),  // 1-1 단계,  원래는 10f. 테스트를 위해 1f로 해놓았음
            new StageInfo(120f, 300f, 14, 8f), // 1-2 단계
            new StageInfo(300f, 600f, 16, 8f), // 2-1 단계
            new StageInfo(600f, 900f, 18, 6f), // 2-2 단계
            new StageInfo(900f, 1200f, 20, 6f) // 3단계
        };

        public float GameTime { get { return _gameTime; } }



        private void Start()
        {
            StartNextStage();
        }

        private void Update()
        {
            _gameTime += Time.deltaTime;
            //if (gameTime % 5.0f > 0 && gameTime % 5.0f <= 0.1)
            //{
            //    Debug.Log($"currentStage:{currentStage}, StageLength: {stages.Length}");
            //    Debug.Log($"gameTime:{gameTime}, [currentStage].endTime:{stages[currentStage].endTime}");
            //}

            // 현재 스테이지가 끝났는지 확인
            if (currentStage < _monsterStages.Length && _gameTime > _monsterStages[currentStage].endTime)
            {
                StartNextStage();
            }
        }

        private void StartNextStage()
        {
            if (currentStage >= _monsterStages.Length) return;

            StageInfo monsterStage = _monsterStages[currentStage];
            StageInfo propStage = _propStages[currentStage];

            Debug.Log($"Starting Stage {currentStage + 1}: {monsterStage.spawnFrequency} seconds per monster.");

            // 기존 코루틴 멈추기
            if (monsterSpawnRoutine != null)
            {
                StopCoroutine(monsterSpawnRoutine);
                currentStage++;
            }

            if (propSpawnRoutine != null)
            {
                StopCoroutine(propSpawnRoutine);
            }

            // 새로운 스폰 루틴 시작
            monsterSpawnRoutine = StartCoroutine(StartMonsterSpawning(monsterStage.spawnFrequency, monsterStage.totalObjects));
            propSpawnRoutine = StartCoroutine(StartPropSpawning(propStage.spawnFrequency, propStage.totalObjects));
        }

        public IEnumerator StartMonsterSpawning(float wait, int totalMonsters)
        {
            string[] names = Util.GetNamesOfEnumElement(typeof(MonsterType));

            // 첫 몬스터 소환
            int rand = UnityEngine.Random.Range(0, names.Length);
            Managers.Instance.MonsterMan.MonSpawner.SpawnMonster(names[rand]);

            while (true)
            {
                yield return new WaitForSeconds(wait);
                rand = UnityEngine.Random.Range(0, names.Length);
                GameObject mon = Managers.Instance.MonsterMan.MonSpawner.SpawnMonster(names[rand]);
                Managers.Instance.MonsterMan.activeMonsters.Add(mon);
            }
        }

        public IEnumerator StartPropSpawning(float wait, int totalMonsters)
        {
            string[] names = Util.GetNamesOfEnumElement(typeof(PropType));

            // 첫 프롭 소환
            int rand = UnityEngine.Random.Range(0, names.Length);
            Managers.Instance.PropSpawner.SpawnProp(names[rand]);

            while (true)
            {
                yield return new WaitForSeconds(wait);
                rand = UnityEngine.Random.Range(0, names.Length);
                Managers.Instance.PropSpawner.SpawnProp(names[rand]);
            }
        }
    }

    [System.Serializable]
    public struct StageInfo
    {
        public float startTime;
        public float endTime;
        public int totalObjects;
        public float spawnFrequency;

        public StageInfo(float startTime, float endTime, int totalMonsters, float spawnFrequency)
        {
            this.startTime = startTime;
            this.endTime = endTime;
            this.totalObjects = totalMonsters;
            this.spawnFrequency = spawnFrequency;
        }
    }
}
