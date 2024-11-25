using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace Jambuddy.Junsu
{
    public class Rotation : Block
    {
        private readonly float SPEED = 300.0f;

        public override void ApplyEffect(EffectTarget target)
        {
            target.StartCoroutine(Rotate(target.transform, EventDuration.ROTATION, SPEED)); // 10초 동안 회전
        }

        private IEnumerator Rotate(Transform targetTransform, float duration, float speed)
        {
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                targetTransform.Rotate(Vector3.up, speed * Time.deltaTime); // 초당 speed만큼 회전
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
    }
}
