using System.Collections;
using UnityEngine;

namespace Jambuddy.Junsu
{
    public class Sizing : Block
    {
        public override void ApplyEffect(EffectTarget target)
        {
            // target의 MonoBehaviour를 통해 코루틴 실행
            target.StartCoroutine(ChangeSizeTemporarily(target, target.transform, 1f, 2f, 5f));
        }

        private IEnumerator ChangeSizeTemporarily(MonoBehaviour runner, Transform targetTransform, float duration, float scaleMultiplier, float revertDelay)
        {
            Vector3 originalScale = targetTransform.localScale; // 현재 크기 저장
            Vector3 targetScale = originalScale * scaleMultiplier; // 변경할 크기 계산

            // 크기 증가 애니메이션
            yield return runner.StartCoroutine(ScaleOverTime(targetTransform, originalScale, targetScale, duration));

            // 일정 시간 대기 (크기 유지)
            yield return new WaitForSeconds(revertDelay);

            // 원래 크기로 복구 애니메이션
            yield return runner.StartCoroutine(ScaleOverTime(targetTransform, targetScale, originalScale, duration));
        }

        private IEnumerator ScaleOverTime(Transform targetTransform, Vector3 startScale, Vector3 endScale, float duration)
        {
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / duration); // 진행 비율 계산
                targetTransform.localScale = Vector3.Lerp(startScale, endScale, progress); // 선형 보간으로 크기 변경
                yield return null;
            }

            targetTransform.localScale = endScale; // 보정: 최종 크기 설정
        }
    }
}
