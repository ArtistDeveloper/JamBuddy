using StarterAssets;
using UnityEngine;

namespace Jambuddy.Adohi.Character
{
    public class IKHandler : MonoBehaviour
    {
        private Animator animator;
        private ThirdPersonController controller;
        [Header("IK Settings")]
        public Transform grabTarget; // ����Ʈ���� ��ġ
        public float ikWeight = 1.0f; // IK ���� ����
        public Vector3 minRotationEuler; // �ո� ȸ��
        public Vector3 maxRotationEuler; // �ո� ȸ��
        void Start()
        {
            animator = GetComponent<Animator>();
            controller = GetComponent<ThirdPersonController>();
        }

        void OnAnimatorIK(int layerIndex)
        {
            if (animator)
            {
                // ������ IK ����
                if (grabTarget != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, ikWeight);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, ikWeight);
                    animator.SetIKPosition(AvatarIKGoal.RightHand, grabTarget.position);

                    // �ո� ȸ�� ���� (���� ȸ������ ����)
                    Transform rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
                    var verticalAngle =  Vector3.Lerp(minRotationEuler, maxRotationEuler, controller.VerticalLerpValue);

                    Quaternion localRotation = Quaternion.Euler(verticalAngle);

                    // �ո� ȸ�� ���� (�������� �θ� �������� ���� ȸ�� ����)
                    rightHand.localRotation = localRotation;

                    
                    // �ո� ������ ���� ȸ������ IK�� ����
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
