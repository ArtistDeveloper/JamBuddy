using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Jambuddy.Adohi.Character.Smartphone
{
    public class SmartphoneController : MonoBehaviour
    {
        [SerializeField] private GameObject smartphone; // ����Ʈ�� ������Ʈ
        [SerializeField] private float timeScaleSlowDuration = 1f; // �������� �ð�
        [SerializeField] private float holdTimeScale = 0f; // ���� Ÿ�ӽ�����
        [SerializeField] private float timeScaleResetDuration = 1f; // ���󺹱� �ð�
        [SerializeField] private float smartphoneRaiseDuration = 0.5f; // ����Ʈ�� ��� ���� �ð�
        [SerializeField] private Vector3 raisedPositionOffset = new Vector3(0, 1, 0); // ����Ʈ�� ��ġ ������
        [SerializeField] private Ease smartphoneAnimationEase = Ease.OutQuad; // �ִϸ��̼� ��¡

        private bool isSmartphoneRaised = false;
        private Vector3 originalPosition;

        private void Start()
        {
            if (smartphone != null)
            {
                originalPosition = smartphone.transform.localPosition;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                if (isSmartphoneRaised)
                {
                    LowerSmartphone().Forget();
                }
                else
                {
                    RaiseSmartphone().Forget();
                }
            }
        }

        private async UniTask RaiseSmartphone()
        {
            if (smartphone == null) return;

            isSmartphoneRaised = true;

            // ����Ʈ�� ��� ����
            smartphone.transform.DOLocalMove(originalPosition + raisedPositionOffset, smartphoneRaiseDuration)
                .SetEase(smartphoneAnimationEase);

            // Ÿ�ӽ����� ������
            await DOTween.To(() => Time.timeScale, x => Time.timeScale = x, holdTimeScale, timeScaleSlowDuration)
                .SetEase(Ease.OutQuad)
                .AsyncWaitForCompletion();

            Time.timeScale = holdTimeScale; // ������ ����
        }

        private async UniTask LowerSmartphone()
        {
            if (smartphone == null) return;

            isSmartphoneRaised = false;

            // ����Ʈ�� ������ ����
            smartphone.transform.DOLocalMove(originalPosition, smartphoneRaiseDuration)
                .SetEase(smartphoneAnimationEase);

            // Ÿ�ӽ����� ����
            await DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1f, timeScaleResetDuration)
                .SetEase(Ease.InQuad)
                .AsyncWaitForCompletion();

            Time.timeScale = 1f; // ���� �ӵ� ����
        }
    }

}
