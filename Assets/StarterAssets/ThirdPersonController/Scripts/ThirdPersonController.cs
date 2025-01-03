﻿using Cinemachine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Jambuddy.Adohi.Character;
using Jambuddy.Adohi.Colors;
using UnityAtoms.BaseAtoms;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;
        public float sprintCost = 0.2f;
        public float sprintRecovery = 0.2f;
        public float sprintRecoveryDelay = 2f;
        private FloatReference currentStamina => CharacterManager.Instance.currentStamina;
        private FloatReference maxStamina => CharacterManager.Instance.maxStamina;
        private bool isRecovering = false;
        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        [Header("GrabTargetController")]
        public float VerticalLerpValue { get; private set; }
        public Transform minHeightTransform;
        public Transform maxHeightTransform;
        public Transform grapTarget;

        [Header("Particles")]
        public ParticleSystem footStepParticle;
        public float footStepOffsetHeight = 0.1f;
        private int footstepCount;

        [Header("NoiseSetting")]
        public CinemachineVirtualCamera virtualCamera;
        public float runAmplitude = 2.0f;
        public float walkAmplitude = 0.5f;
        public float runFrequency = 2.0f;
        public float walkFrequency = 1.0f;
        public float transitionDuration = 0.5f;

        private CinemachineBasicMultiChannelPerlin noise;
        private Tween amplitudeTween;
        private Tween frequencyTween;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM 
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        private bool isControlLocked;
        private bool isRunning;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }


        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM 
            _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;

            if (virtualCamera != null)
            {
                noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            }
        }

        private void OnEnable()
        {
            CharacterModeManager.Instance.onHackModeStart.AddListener(LockControls);
            CharacterModeManager.Instance.onDefaultModeStart.AddListener(UnlockControls);
        }

        private void OnDisable()
        {
            CharacterModeManager.Instance.onHackModeStart.RemoveListener(LockControls);
            CharacterModeManager.Instance.onDefaultModeStart.RemoveListener(UnlockControls);
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            

            // 캐릭터가 카메라를 따라 회전
            transform.rotation = Quaternion.Euler(0.0f, _cinemachineTargetYaw, 0.0f);

            if(!isControlLocked)
            {
                Move();
                HandleJump();
                HandleStamina();

                var currentlyRunning = (_input.sprint && !isRecovering);

                if (currentlyRunning != isRunning)
                {
                    isRunning = currentlyRunning;
                    UpdateNoise(isRunning);
                }
            }

            
            HandleGravity();
            GroundedCheck();
        }

        private void LateUpdate()
        {
            if (!isControlLocked)
            {
                CameraRotation();
            }
        }

        public void LockControls()
        {
            isControlLocked = true;
        }

        public void UnlockControls()
        {
            isControlLocked = false;
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            //Vector3 shoulderOffset = new Vector3(0.5f, 1.5f, -0.5f); // 오른쪽으로, 위로, 뒤로
            //CinemachineCameraTarget.transform.position = transform.position + transform.TransformDirection(shoulderOffset);

            // 마우스 입력에 따른 카메라 회전 처리
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                // 입력값에 따라 카메라 회전 값 변경
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // 회전 각도 제한 (위아래: BottomClamp ~ TopClamp, 좌우 제한 없음)
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            VerticalLerpValue = 1f - Mathf.InverseLerp(BottomClamp, TopClamp, _cinemachineTargetPitch);

            var targetPosition = Vector3.Lerp(minHeightTransform.position, maxHeightTransform.position, VerticalLerpValue);

            grapTarget.transform.position = targetPosition;

            // 카메라 회전 설정
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(
                _cinemachineTargetPitch,
                _cinemachineTargetYaw,
                0.0f
            );




        }



        private void Move()
        {
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = (_input.sprint && !isRecovering) ? SprintSpeed : MoveSpeed;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // rotate to face input direction relative to camera position
                //transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }


            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // move the player
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void HandleStamina()
        {
            if (_input.sprint && !isRecovering)
            {
                currentStamina.Value -= sprintCost * Time.deltaTime;
                currentStamina.Value = Mathf.Max(currentStamina, 0f);

                if (currentStamina <= 0f)
                {
                    TriggerStaminaRecoveryDelay().Forget(); // 비동기 회복 지연
                }
            }
            else if (!isRecovering)
            {
                RecoverStamina();
            }
        }

        private void RecoverStamina()
        {
            currentStamina.Value += sprintRecovery * Time.deltaTime;
            currentStamina.Value = Mathf.Min(currentStamina, maxStamina);
        }

        private async UniTaskVoid TriggerStaminaRecoveryDelay()
        {
            isRecovering = true;
            await UniTask.Delay(System.TimeSpan.FromSeconds(sprintRecoveryDelay));
            isRecovering = false;
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private void HandleJump()
        {
            if (Grounded)
            {
                // Reset fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // Update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // Stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // Calculate the velocity needed to reach the desired jump height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // Update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                // Jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // Reset jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // If not grounded, prevent jumping
                _input.jump = false;
            }
        }

        private void HandleGravity()
        {
            if (!Grounded)
            {
                // Fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // Update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }
            }

            // Apply gravity over time if under terminal velocity
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        public void UpdateNoise(bool isRunning)
        {
            if (noise != null)
            {
                // 목표 값 설정
                float targetAmplitude = isRunning ? runAmplitude : walkAmplitude;
                float targetFrequency = isRunning ? runFrequency : walkFrequency;

                // 기존 Tween이 있으면 중지
                amplitudeTween?.Kill();
                frequencyTween?.Kill();

                // DOTween으로 부드럽게 전환
                amplitudeTween = DOTween.To(() => noise.m_AmplitudeGain,
                                            x => noise.m_AmplitudeGain = x,
                                            targetAmplitude,
                                            transitionDuration);
                frequencyTween = DOTween.To(() => noise.m_FrequencyGain,
                                            x => noise.m_FrequencyGain = x,
                                            targetFrequency,
                                            transitionDuration);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }

                var footStepParitcle = Instantiate(footStepParticle);
                var footStepMain = footStepParitcle.main;
                footStepParitcle.transform.position = transform.position + Vector3.up * footStepOffsetHeight;
                footStepParitcle.gameObject.SetActive(true);
                footStepMain.startColor = ColorManager.Instance.themaColors[footstepCount % ColorManager.Instance.themaColors.Count].Value;
                footstepCount++;
            }

        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);

                var footStepParitcle = Instantiate(footStepParticle);
                var footStepMain = footStepParitcle.main;
                footStepParitcle.transform.position = transform.position + Vector3.up * footStepOffsetHeight;
                footStepParitcle.gameObject.SetActive(true);
                footStepMain.startColor = ColorManager.Instance.themaColors[footstepCount % ColorManager.Instance.themaColors.Count].Value;
                footStepMain.startSize = new ParticleSystem.MinMaxCurve(2f, footStepMain.startSize.curve);

                footstepCount++;
            }
        }
    }
}