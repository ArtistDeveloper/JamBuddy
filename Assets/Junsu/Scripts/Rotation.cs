using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace Jambuddy.Junsu
{
    public class Rotation : Block
    {
        public override void ApplyEffect(EffectTarget target)
        {
            Debug.Log("Rotation 적용");
            target.StartCoroutine(Rotate(target.transform, 2f)); // 3초 동안 회전
        }

        private IEnumerator Rotate(Transform targetTransform, float duration)
        {
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                targetTransform.Rotate(Vector3.up, 90 * Time.deltaTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
    }
}
