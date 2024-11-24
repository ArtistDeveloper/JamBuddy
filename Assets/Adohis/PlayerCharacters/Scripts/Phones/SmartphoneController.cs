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
        [SerializeField] private CinemachineVirtualCamera cam1; // ù ��° ī�޶�
        [SerializeField] private CinemachineVirtualCamera cam2; // �� ��° ī�޶�

        [Header("Fade")]
        [SerializeField] private Image blurImage;  // ���̵� ȿ���� ���� CanvasGroup
        [SerializeField] private float blurFadeDuration = 0.5f;     // ���̵� �ð�
        [SerializeField] private Ease blurEase;     // ���̵� �ð�

        [Header("Noise")]
        [SerializeField] private Volume noiseVolume;  // ���̵� ȿ���� ���� CanvasGroup
        [SerializeField] private float noiseFadeDuration = 0.5f;     // ���̵� �ð�
        [SerializeField] private Ease noiseEase;     // ���̵� �ð�

        private bool isCam1Active = true; // ���� Ȱ��ȭ�� ī�޶� ����
        private CancellationTokenSource _cancellationTokenSource;
        private void Start()
        {
            // �ʱ� ����: ù ��° ī�޶� Ȱ��ȭ
            cam1.Priority = 10;
            cam2.Priority = 0;
            //if (blurImage != null) blurImage.a = 0; // ���̵� �ʱ�ȭ
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
            // ���� �۾� ���
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
                // LShift �Է� ����
                if (Input.GetMouseButtonDown(1))
                {
                    await SwitchCamerasAsync();
                }
                await UniTask.Yield(); // ���� ������ ���
            }
        }

        private async UniTask SwitchCamerasAsync()
        {
            // ���̵� ȿ�� ����

            // ī�޶� �켱���� ����Ī
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
            // ���̵� ȿ�� ����
        }

        private async UniTask BlurFadeEffectAsync(float targetAlpha)
        {
            if (blurImage == null) return;

            // DoTween���� CanvasGroup�� Alpha �� ����
            await blurImage.DOFade(targetAlpha, blurFadeDuration).SetUpdate(true).SetEase(blurEase);
        }

        private async UniTask NoiseFadeEffectAsync(float targetWeight)
        {
            if (noiseVolume == null) return;
                // DoTween���� CanvasGroup�� Alpha �� ����
            await DOTween.To(
                () => noiseVolume.weight,            // ���� weight ��
                x => noiseVolume.weight = x,         // weight ���� ������Ʈ
                targetWeight,                   // ��ǥ weight ��
                noiseFadeDuration                    // ���̵� ���� �ð�
                )
                .From(1f)
                .SetUpdate(true)
                .SetEase(noiseEase);
        }
    }

}
