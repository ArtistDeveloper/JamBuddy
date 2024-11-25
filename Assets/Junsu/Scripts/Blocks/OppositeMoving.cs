using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Jambuddy.Junsu
{
    public class OppositeMoving : Block
    {
        private readonly float SPEED = 10f;

        private NavMeshAgent _agent;
        float _originSpeed;

        public Action OnMoveOppositeComplete; // Coroutine 완료 이벤트

        public override void ApplyEffect(EffectTarget target)
        {
            target.StartCoroutine(MoveOppositeOfPlayer(target.transform, EventDuration.OPPO_MOVING));
        }

        // 움직이고 있는 물체가 duration 동안 유저의 반대 방향으로 이동
        private IEnumerator MoveOppositeOfPlayer(Transform targetTransform, float duration)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogError("Player object not found!");
                yield break;
            }

            // 대상의 NavMesh Agent Speed는 Opposite Moving이 적용될 동안은 무효화
            _agent = targetTransform.GetComponent<NavMeshAgent>();
            if (_agent != null)
            {
                _originSpeed = _agent.speed;
                _agent.speed = 0f;
            }

            // 플레이어와의 방향 계산 (x, z축만 사용)
            Vector3 directionToPlayer = new Vector3(
                player.transform.position.x - targetTransform.position.x,
                0, // y축 무시
                player.transform.position.z - targetTransform.position.z
            ).normalized;

            // 반대 방향 계산
            Vector3 oppositeDirection = -directionToPlayer;

            // 지속 시간 동안 이동
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                // 이동: 매 프레임마다 업데이트 (y축 고정)
                targetTransform.position += oppositeDirection * Time.deltaTime * SPEED; // 속도 조정 가능
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            if (_agent != null)
            {
                _agent.speed = _originSpeed;
            }

            OnMoveOppositeComplete.Invoke();
        }
    }
}
