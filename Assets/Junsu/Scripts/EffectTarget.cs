using UnityEngine;

namespace Jambuddy.Junsu
{
    public class EffectTarget : MonoBehaviour
    {
        private void Awake()
        {
            EffectTargetManager.onBlock -= HandleBlockApplication;
            EffectTargetManager.onBlock += HandleBlockApplication;
        }

        //void OnEnable()
        //{
        //    EffectTargetManager.OnBlockApplied -= HandleBlockApplication;
        //    EffectTargetManager.OnBlockApplied += HandleBlockApplication;
        //}

        //void OnDisable()
        //{
        //    EffectTargetManager.OnBlockApplied -= HandleBlockApplication;
        //}


        public void HandleBlockApplication(string blockType)
        {
            Debug.Log($"received block: {blockType}");
            // 블록 타입에 따라 행동 정의
            switch (blockType)
            {
                case "gravity":
                    ApplyGravity();
                    break;
                case "rotation":
                    ApplyRotation();
                    break;
                    // 다른 블록 타입 추가
            }
        }

        private void ApplyGravity()
        {
            // 중력 적용 로직
            Debug.Log($"{gameObject.name} is affected by gravity.");
        }

        private void ApplyRotation()
        {
            // 회전 적용 로직
            Debug.Log($"{gameObject.name} is rotating.");
        }
    }
}
