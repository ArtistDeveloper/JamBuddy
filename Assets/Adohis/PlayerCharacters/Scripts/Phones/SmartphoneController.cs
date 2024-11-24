using Cinemachine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using System;
using UnityEngine;

namespace Jambuddy.Adohi.Character.Smartphone
{
    public class SmartphoneController : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera cam1; // 첫 번째 카메라
        [SerializeField] private CinemachineVirtualCamera cam2; // 두 번째 카메라
        [SerializeField] private CanvasGroup transitionEffect;  // 페이드 효과를 위한 CanvasGroup
        [SerializeField] private float fadeDuration = 0.5f;     // 페이드 시간
        private bool isCam1Active = true; // 현재 활성화된 카메라 상태
        private CancellationTokenSource _cancellationTokenSource;
        private void Start()
        {
            // 초기 설정: 첫 번째 카메라 활성화
            cam1.Priority = 10;
            cam2.Priority = 0;
            if (transitionEffect != null) transitionEffect.alpha = 0; // 페이드 초기화
        }

        private void OnEnable()
        {
            CharacterModeManager.Instance.onHackModeStart.AddListener(HandleSwitchCamera);
            CharacterModeManager.Instance.onDefaultModeStart.AddListener(HandleSwitchCamera);
        }

        private void OnDisable()
        {
            CharacterModeManager.Instance.onHackModeStart.RemoveListener(HandleSwitchCamera);
            CharacterModeManager.Instance.onDefaultModeStart.RemoveListener(HandleSwitchCamera);
        }

        private async void HandleSwitchCamera()
        {
            // 기존 작업 취소
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await SwitchCamerasAsync();
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"Switching to Camera was canceled.");
            }
        }

        private async UniTask ListenForSwitch()
        {
            while (true)
            {
                // LShift 입력 감지
                if (Input.GetMouseButtonDown(1))
                {
                    await SwitchCamerasAsync();
                }
                await UniTask.Yield(); // 다음 프레임 대기
            }
        }

        private async UniTask SwitchCamerasAsync()
        {
            // 페이드 효과 시작
            if (transitionEffect != null) await FadeEffectAsync(1);

            // 카메라 우선순위 스위칭
            isCam1Active = !isCam1Active;
            cam1.Priority = isCam1Active ? 10 : 0;
            cam2.Priority = isCam1Active ? 0 : 10;

            // 페이드 효과 종료
            if (transitionEffect != null) await FadeEffectAsync(0);
        }

        private async UniTask FadeEffectAsync(float targetAlpha)
        {
            if (transitionEffect == null) return;

            // DoTween으로 CanvasGroup의 Alpha 값 조정
            await transitionEffect.DOFade(targetAlpha, fadeDuration).AsyncWaitForCompletion();
        }
    }

}
