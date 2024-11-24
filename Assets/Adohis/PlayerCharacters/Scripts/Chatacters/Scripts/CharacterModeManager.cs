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
        // 핵 모드 이벤트
        public UnityEvent onHackModeStart;     // 핵 모드 진입 시작
        public UnityEvent onHackModeEnter;     // 핵 모드 진입 완료
        public UnityEvent onDefaultModeExit;   // 기본 모드 종료

        // 기본 모드 이벤트
        public UnityEvent onDefaultModeStart;  // 기본 모드 진입 시작
        public UnityEvent onDefaultModeEnter;  // 기본 모드 진입 완료
        public UnityEvent onHackModeExit;      // 핵 모드 종료

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

            // 현재 모드와 타겟 모드 결정
            var targetMode = CurrentMode == CharacterMode.Default ? CharacterMode.Hack : CharacterMode.Default;

            // 진입 시작 이벤트 호출
            if (targetMode == CharacterMode.Hack)
            {
                onHackModeStart?.Invoke();
            }
            else
            {
                onDefaultModeStart?.Invoke();
            }

            // 타임스케일 변화 (진입 애니메이션)
            float initialTimeScale = Time.timeScale;
            float targetTimeScale = targetMode == CharacterMode.Hack ? hackModeTimeScale : 1.0f;

            await DOTween.To(
                () => Time.timeScale,
                value => Time.timeScale = value,
                targetTimeScale,
                modeSwapDuration
            ).SetUpdate(true).SetEase(Ease.InOutQuad).AsyncWaitForCompletion();


            // 진입 완료 이벤트 호출
            if (targetMode == CharacterMode.Hack)
            {
                onHackModeEnter?.Invoke();
            }
            else
            {
                onDefaultModeEnter?.Invoke();
            }

            // 기존 모드 종료 이벤트 호출
            if (CurrentMode == CharacterMode.Hack)
            {
                onHackModeExit?.Invoke();
            }
            else
            {
                onDefaultModeExit?.Invoke();
            }

            // 모드 전환 완료
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

            // 강제 모드 전환에 따른 이벤트 호출
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
