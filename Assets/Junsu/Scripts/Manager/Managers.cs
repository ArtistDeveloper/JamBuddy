using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Resources;

namespace Jambuddy.Junsu
{
    public class Managers : MonoBehaviour
    {
        private static Managers s_instance;

        private MonsterManager _monsterMan;

        private GameManager _gameMan;
        private PropSpawner _propSpawner;

        public static Managers Instance
        {
            get
            {
                Init(); 
                return s_instance;
            }
        }


        // 몬스터 생성 지형 확인
        private void OnDrawGizmos()
        {
            Vector3 spawnArea = new Vector3(0f, 0f, 0f);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spawnArea, 25f); // 최소 거리
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(spawnArea, 80f); // 최대 거리
        }

        public GameManager GameMan { get { return Instance._gameMan; } }

        public MonsterManager MonsterMan 
        {
            get
            { 
                if (_monsterMan == null)
                {
                    _monsterMan = new MonsterManager();
                    _monsterMan.Init();
                }
                return Instance._monsterMan; 
            } 
        }

        public PropSpawner PropSpawner
        {
            get
            {
                if (_propSpawner == null)
                {
                    _propSpawner = new PropSpawner(10);
                    _propSpawner.Init();
                }
                return Instance._propSpawner;
            }
        }

        private void Start()
        {
            Init();
            
            //_monsterMan.Init();
            _gameMan = s_instance.transform.GetComponentInChildren<GameManager>();
        }

        private void Update()
        {
            _monsterMan.MonSpawner.OnUpdate();
        }

        private static void Init()
        {
            if (s_instance == null)
            {
                GameObject go = GameObject.Find("@Managers");
                if (go == null)
                {
                    go = new GameObject { name = "@Managers" };
                    go.AddComponent<Managers>();
                }

                DontDestroyOnLoad(go);
                s_instance = go.GetComponent<Managers>();

                Transform gmt = go.transform.Find("@GameManager");
                if (gmt == null)
                {
                    GameObject gm = new GameObject { name = "@GameManager" };
                    gm.AddComponent<GameManager>();
                    gm.transform.parent = go.transform;
                }                
            }
        }
    }
}
