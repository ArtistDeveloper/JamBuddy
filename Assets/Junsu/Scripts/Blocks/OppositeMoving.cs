using System.Collections;
using UnityEngine;

namespace Jambuddy.Junsu
{
    public class OppositeMoving : Block
    {
        private readonly float SPEED = 5f;

        public override void ApplyEffect(EffectTarget target)
        {
            Debug.Log("Opposite Moving 적용");
            target.StartCoroutine(MoveOppositeOfPlayer(target.transform, 3f));
        }

        // 움직이고 있는 물체가 duration 동안 유저의 반대 방향으로 이동
        // (이 동안 몬스터의 방향벡터는 업데이트 될 필요 없음)
        private IEnumerator MoveOppositeOfPlayer(Transform targetTransform, float duration)
        {
            // 플레이어 위치 가져오기 (플레이어가 태그로 지정되어 있다고 가정)
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogError("Player object not found!");
                yield break;
            }

            // 대상의 초기 위치 저장
            Vector3 startPosition = targetTransform.position;

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
                targetTransform.position += oppositeDirection * Time.deltaTime * 5f; // 속도 조정 가능

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // 이동이 끝난 후 x, z 축 위치 고정 (y는 변화하지 않음)
            Vector3 finalPosition = new Vector3(
                startPosition.x + oppositeDirection.x * elapsedTime * SPEED,
                targetTransform.position.y, // 기존 y 위치 유지
                startPosition.z + oppositeDirection.z * elapsedTime * SPEED
            );

            targetTransform.position = finalPosition;
        }
    }
}
