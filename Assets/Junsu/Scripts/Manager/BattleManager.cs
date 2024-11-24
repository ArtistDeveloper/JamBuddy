using Jambuddy.Junsu;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Jambuddy.Junsu
{
    public class BattleManager
    {
        // 싱글톤 패턴으로 BattleManager 관리
        private static BattleManager _instance;
        public static BattleManager Instance => _instance ??= new BattleManager();

        private MonsterManager _monsterManager;

        // 프롭 리스트 관리
        private readonly List<OppositeMoving> _props = new List<OppositeMoving>();

        // 이벤트: 몬스터가 데미지를 받을 때
        public event Action<GameObject, float> OnMonsterDamaged;
        public event Action<GameObject> OnMonsterDied;

        // 초기화 메서드
        public void Initialize(MonsterManager monsterManager)
        {
            _monsterManager = monsterManager;
            _monsterManager.Init();
            Debug.Log("BattleManager initialized with MonsterManager.");
        }

        // 프롭 등록
        public void RegisterProp(OppositeMoving prop)
        {
            if (!_props.Contains(prop))
            {
                _props.Add(prop);
            }
        }

        // 프롭 제거
        public void UnregisterProp(OppositeMoving prop)
        {
            if (_props.Remove(prop))
            {
                Debug.Log($"Prop unregistered.");
            }
        }

        // 프롭과 몬스터 충돌 처리
        //public void HandleCollision(OppositeMoving prop, GameObject monsterObject)
        //{
        //    if (prop.IsOppositeMoving && _props.Contains(prop))
        //    {
        //        Monster monster = monsterObject.GetComponent<Monster>();
        //        if (monster != null)
        //        {
        //            float damage = prop.Damage;
        //            monster.TakeDamage(damage);
        //            OnMonsterDamaged?.Invoke(monsterObject, damage);

        //            Debug.Log($"{prop.name} dealt {damage} damage to {monster.name}.");

        //            // 몬스터 사망 처리
        //            if (monster.Health <= 0)
        //            {
        //                HandleMonsterDeath(monsterObject);
        //            }
        //        }
        //    }
        //}

        // 몬스터 사망 처리
        private void HandleMonsterDeath(GameObject monsterObject)
        {
            Debug.Log($"{monsterObject.name} has died.");
            _monsterManager.DeregisterMonster(monsterObject);
            OnMonsterDied?.Invoke(monsterObject);
        }
    }
}
