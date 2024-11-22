using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Jambuddy.Adohi.Character.Smartphone
{
    public class SmartphoneController : MonoBehaviour
    {
        [SerializeField] private GameObject smartphone; // 스마트폰 오브젝트
        [SerializeField] private float timeScaleSlowDuration = 1f; // 느려지는 시간
        [SerializeField] private float holdTimeScale = 0f; // 멈춘 타임스케일
        [SerializeField] private float timeScaleResetDuration = 1f; // 원상복귀 시간
        [SerializeField] private float smartphoneRaiseDuration = 0.5f; // 스마트폰 드는 연출 시간
        [SerializeField] private Vector3 raisedPositionOffset = new Vector3(0, 1, 0); // 스마트폰 위치 오프셋
        [SerializeField] private Ease smartphoneAnimationEase = Ease.OutQuad; // 애니메이션 이징

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

            // 스마트폰 드는 연출
            smartphone.transform.DOLocalMove(originalPosition + raisedPositionOffset, smartphoneRaiseDuration)
                .SetEase(smartphoneAnimationEase);

            // 타임스케일 느려짐
            await DOTween.To(() => Time.timeScale, x => Time.timeScale = x, holdTimeScale, timeScaleSlowDuration)
                .SetEase(Ease.OutQuad)
                .AsyncWaitForCompletion();

            Time.timeScale = holdTimeScale; // 완전히 멈춤
        }

        private async UniTask LowerSmartphone()
        {
            if (smartphone == null) return;

            isSmartphoneRaised = false;

            // 스마트폰 내리는 연출
            smartphone.transform.DOLocalMove(originalPosition, smartphoneRaiseDuration)
                .SetEase(smartphoneAnimationEase);

            // 타임스케일 복귀
            await DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1f, timeScaleResetDuration)
                .SetEase(Ease.InQuad)
                .AsyncWaitForCompletion();

            Time.timeScale = 1f; // 정상 속도 복귀
        }
    }

}
