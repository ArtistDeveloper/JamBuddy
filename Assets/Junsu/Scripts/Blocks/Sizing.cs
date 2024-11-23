using System.Collections;
using UnityEngine;

namespace Jambuddy.Junsu
{
    public class Sizing : Block
    {
        public override void ApplyEffect(EffectTarget target)
        {
            Debug.Log("Rotation 적용");
            target.StartCoroutine(IncreaseSize(target.transform, 1f));
        }

        private IEnumerator IncreaseSize(Transform targetTransform, float duration)
        {
            Vector3 originalScale = targetTransform.localScale; // 현재 크기 저장
            Vector3 targetScale = originalScale * 2f;          // 2배 크기 설정
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / duration); // 0에서 1 사이로 진행 비율 계산
                targetTransform.localScale = Vector3.Lerp(originalScale, targetScale, progress); // 선형 보간으로 크기 변경
                yield return null;
            }

            targetTransform.localScale = targetScale; // 보정: 최종 크기 설정
            Debug.Log("Size 증가 완료");
        }
    }
}
