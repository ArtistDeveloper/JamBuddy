using StarterAssets;
using UnityEngine;

namespace Jambuddy.Adohi.Character
{
    public class IKHandler : MonoBehaviour
    {
        private Animator animator;
        private ThirdPersonController controller;
        [Header("IK Settings")]
        public Transform grabTarget; // 스마트폰의 위치
        public float ikWeight = 1.0f; // IK 적용 강도
        public Vector3 minRotationEuler; // 손목 회전
        public Vector3 maxRotationEuler; // 손목 회전
        void Start()
        {
            animator = GetComponent<Animator>();
            controller = GetComponent<ThirdPersonController>();
        }

        void OnAnimatorIK(int layerIndex)
        {
            if (animator)
            {
                // 오른손 IK 설정
                if (grabTarget != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, ikWeight);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, ikWeight);
                    animator.SetIKPosition(AvatarIKGoal.RightHand, grabTarget.position);

                    // 손목 회전 설정 (로컬 회전값을 적용)
                    Transform rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
                    var verticalAngle =  Vector3.Lerp(minRotationEuler, maxRotationEuler, controller.VerticalLerpValue);

                    Quaternion localRotation = Quaternion.Euler(verticalAngle);

                    // 손목 회전 적용 (오른손의 부모를 기준으로 로컬 회전 설정)
                    rightHand.localRotation = localRotation;

                    
                    // 손목에 고정된 로컬 회전값을 IK에 설정
                    animator.SetIKRotation(AvatarIKGoal.RightHand, rightHand.rotation);

                   
                }
                else
                {
                    Debug.LogWarning("Smartphone Position not assigned!");
                }
            }
        }
    }
}
