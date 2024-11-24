using DG.Tweening;
using Pixelplacement;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Jambuddy.Adohi.Character
{
    public class CharacterModeManager : Singleton<CharacterModeManager>
    {
        public enum CharacterMode
        {
            Default,
            Hack
        }

        [Header("Settings")]
        public float modeSwapDuration = 1.0f;
        public float hackModeTimeScale = 0.2f;

        [Header("Events")]
        // �� ��� �̺�Ʈ
        public UnityEvent onHackModeStart;     // �� ��� ���� ����
        public UnityEvent onHackModeEnter;     // �� ��� ���� �Ϸ�
        public UnityEvent onDefaultModeExit;   // �⺻ ��� ����

        // �⺻ ��� �̺�Ʈ
        public UnityEvent onDefaultModeStart;  // �⺻ ��� ���� ����
        public UnityEvent onDefaultModeEnter;  // �⺻ ��� ���� �Ϸ�
        public UnityEvent onHackModeExit;      // �� ��� ����

        public CharacterMode CurrentMode { get; private set; } = CharacterMode.Default;


        public bool debug;
        private bool isSwapping = false;

        private void Start()
        {
            onHackModeStart.RegisterLogNameCallback(this, debug);
            onHackModeEnter.RegisterLogNameCallback(this, debug);
            onDefaultModeExit.RegisterLogNameCallback(this, debug);
            onDefaultModeStart.RegisterLogNameCallback(this, debug);
            onDefaultModeEnter.RegisterLogNameCallback(this, debug);
            onHackModeExit.RegisterLogNameCallback(this, debug);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                SwapMode();
            }
        }


        public async void SwapMode()
        {
            if (isSwapping)
            {
                Debug.LogWarning("Mode swap already in progress.");
                return;
            }

            isSwapping = true;

            // ���� ���� Ÿ�� ��� ����
            var targetMode = CurrentMode == CharacterMode.Default ? CharacterMode.Hack : CharacterMode.Default;

            // ���� ���� �̺�Ʈ ȣ��
            if (targetMode == CharacterMode.Hack)
            {
                onHackModeStart?.Invoke();
            }
            else
            {
                onDefaultModeStart?.Invoke();
            }

            // Ÿ�ӽ����� ��ȭ (���� �ִϸ��̼�)
            float initialTimeScale = Time.timeScale;
            float targetTimeScale = targetMode == CharacterMode.Hack ? hackModeTimeScale : 1.0f;

            await DOTween.To(
                () => Time.timeScale,
                value => Time.timeScale = value,
                targetTimeScale,
                modeSwapDuration
            ).SetUpdate(true).SetEase(Ease.InOutQuad).AsyncWaitForCompletion();


            // ���� �Ϸ� �̺�Ʈ ȣ��
            if (targetMode == CharacterMode.Hack)
            {
                onHackModeEnter?.Invoke();
            }
            else
            {
                onDefaultModeEnter?.Invoke();
            }

            // ���� ��� ���� �̺�Ʈ ȣ��
            if (CurrentMode == CharacterMode.Hack)
            {
                onHackModeExit?.Invoke();
            }
            else
            {
                onDefaultModeExit?.Invoke();
            }

            // ��� ��ȯ �Ϸ�
            CurrentMode = targetMode;
            isSwapping = false;
        }

        public void ForceMode(CharacterMode mode)
        {
            if (isSwapping)
            {
                Debug.LogWarning("Cannot force mode while swapping.");
                return;
            }

            // ���� ��� ��ȯ�� ���� �̺�Ʈ ȣ��
            if (mode == CharacterMode.Hack)
            {
                onDefaultModeExit?.Invoke();
                onHackModeStart?.Invoke();
                onHackModeEnter?.Invoke();
            }
            else
            {
                onHackModeExit?.Invoke();
                onDefaultModeStart?.Invoke();
                onDefaultModeEnter?.Invoke();
            }

            CurrentMode = mode;
            Time.timeScale = mode == CharacterMode.Hack ? hackModeTimeScale : 1.0f;
            Debug.Log($"Mode forcibly set to {mode}");
        }
    }

}
