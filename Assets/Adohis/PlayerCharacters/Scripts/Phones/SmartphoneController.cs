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
        [SerializeField] private CinemachineVirtualCamera cam1; // ù ��° ī�޶�
        [SerializeField] private CinemachineVirtualCamera cam2; // �� ��° ī�޶�
        [SerializeField] private CanvasGroup transitionEffect;  // ���̵� ȿ���� ���� CanvasGroup
        [SerializeField] private float fadeDuration = 0.5f;     // ���̵� �ð�
        private bool isCam1Active = true; // ���� Ȱ��ȭ�� ī�޶� ����
        private CancellationTokenSource _cancellationTokenSource;
        private void Start()
        {
            // �ʱ� ����: ù ��° ī�޶� Ȱ��ȭ
            cam1.Priority = 10;
            cam2.Priority = 0;
            if (transitionEffect != null) transitionEffect.alpha = 0; // ���̵� �ʱ�ȭ
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
            if (transitionEffect != null) await FadeEffectAsync(1);

            // ī�޶� �켱���� ����Ī
            isCam1Active = !isCam1Active;
            cam1.Priority = isCam1Active ? 10 : 0;
            cam2.Priority = isCam1Active ? 0 : 10;

            // ���̵� ȿ�� ����
            if (transitionEffect != null) await FadeEffectAsync(0);
        }

        private async UniTask FadeEffectAsync(float targetAlpha)
        {
            if (transitionEffect == null) return;

            // DoTween���� CanvasGroup�� Alpha �� ����
            await transitionEffect.DOFade(targetAlpha, fadeDuration).AsyncWaitForCompletion();
        }
    }

}
