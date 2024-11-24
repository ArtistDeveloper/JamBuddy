using Cinemachine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

namespace Jambuddy.Adohi.Character.Smartphone
{
    public class SmartphoneController : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera cam1; // 첫 번째 카메라
        [SerializeField] private CinemachineVirtualCamera cam2; // 두 번째 카메라

        [Header("Fade")]
        [SerializeField] private Image blurImage;  // 페이드 효과를 위한 CanvasGroup
        [SerializeField] private float blurFadeDuration = 0.5f;     // 페이드 시간
        [SerializeField] private Ease blurEase;     // 페이드 시간

        [Header("Noise")]
        [SerializeField] private Volume noiseVolume;  // 페이드 효과를 위한 CanvasGroup
        [SerializeField] private float noiseFadeDuration = 0.5f;     // 페이드 시간
        [SerializeField] private Ease noiseEase;     // 페이드 시간

        private bool isCam1Active = true; // 현재 활성화된 카메라 상태
        private CancellationTokenSource _cancellationTokenSource;
        private void Start()
        {
            // 초기 설정: 첫 번째 카메라 활성화
            cam1.Priority = 10;
            cam2.Priority = 0;
            //if (blurImage != null) blurImage.a = 0; // 페이드 초기화
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

            // 카메라 우선순위 스위칭
            isCam1Active = !isCam1Active;


            

            if (isCam1Active)
            {
                cam1.Priority = 10;
                cam2.Priority = 0;
                BlurFadeEffectAsync(0);
                //NoiseFadeEffectAsync(1);
            }
            else
            {
                cam1.Priority = 0;
                cam2.Priority = 10;
                BlurFadeEffectAsync(1);
                NoiseFadeEffectAsync(0);
            }
            // 페이드 효과 종료
        }

        private async UniTask BlurFadeEffectAsync(float targetAlpha)
        {
            if (blurImage == null) return;

            // DoTween으로 CanvasGroup의 Alpha 값 조정
            await blurImage.DOFade(targetAlpha, blurFadeDuration).SetUpdate(true).SetEase(blurEase);
        }

        private async UniTask NoiseFadeEffectAsync(float targetWeight)
        {
            if (noiseVolume == null) return;
                // DoTween으로 CanvasGroup의 Alpha 값 조정
            await DOTween.To(
                () => noiseVolume.weight,            // 현재 weight 값
                x => noiseVolume.weight = x,         // weight 값을 업데이트
                targetWeight,                   // 목표 weight 값
                noiseFadeDuration                    // 페이드 지속 시간
                )
                .From(1f)
                .SetUpdate(true)
                .SetEase(noiseEase);
        }
    }

}
