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
        public static Managers Instance
        {
            get
            {
                Init(); 
                return s_instance;
            }
        }

        private MonsterManager _monsterMan;
        private GameManager _gameMan;

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
        public GameManager GameMan { get { return Instance._gameMan; } }


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
