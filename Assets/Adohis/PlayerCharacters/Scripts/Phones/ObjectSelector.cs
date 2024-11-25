using DG.Tweening;
using Jambuddy.Adohi.Character.Hack;
using Jambuddy.Adohi.Selection;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Jambuddy.Adohi.Character.Smartphone
{
    public class ObjectSelector : MonoBehaviour
    {
        private Vector2 dragStartPos; // 드래그 시작 위치
        private bool isDragging = false;
        private bool ctrlPressed;
        private bool shiftPressed;
        private bool isActivate;

        public VirtualPointer virtualPointer; // 가상 포인터 사용
        public LayerMask selectableLayer; // 선택 가능한 객체 레이어
        [HideInInspector] public List<Scanable> scanableObjects = new(); // 선택된 객체들

        public TextMeshProUGUI targetText;
        public TextMeshProUGUI alertText;
        [SerializeField] private float alertDuration = 1f; // 커지는 애니메이션 지속 시간
        [SerializeField] private float delayBeforeShrink = 2f; // 작아지기 전 대기 시간
        private Tween currentTween;  // 현재 트윈을 저장할 변수

        private void Start()
        {
            HackAbilityManager.Instance.selector = this;
        }

        private void OnEnable()
        {
            CharacterModeManager.Instance.onHackModeStart.AddListener(Activate);
            CharacterModeManager.Instance.onDefaultModeStart.AddListener(DeActivate);
            HackAbilityManager.Instance.onHackProcessed.AddListener(OnHackProcessed);
            HackAbilityManager.Instance.onHackFailed.AddListener(OnHackFailed);
        }

        private void OnDisable()
        {
            CharacterModeManager.Instance.onHackModeStart.RemoveListener(Activate);
            CharacterModeManager.Instance.onDefaultModeStart.RemoveListener(DeActivate);
            HackAbilityManager.Instance.onHackProcessed.RemoveListener(OnHackProcessed);
            HackAbilityManager.Instance.onHackFailed.RemoveListener(OnHackFailed);
        }

        void LateUpdate()
        {
            if (isActivate)
            {
                // 단순 클릭 및 드래그 시작
                if (virtualPointer.IsLeftClick)
                {
                    GetInput();
                    if (!(ctrlPressed || shiftPressed))
                    {
                        ClearSelection();
                    }
                    dragStartPos = virtualPointer.PlaneNormalizedPosition;
                    isDragging = true;
                }

                // 드래그 종료
                if (virtualPointer.IsLeftClickReleased && isDragging)
                {
                    isDragging = false;
                }

                // 드래그 중이면 디버그 시각화
                if (isDragging)
                {
                    HandleVirtualClick();
                    HandleDragSelection();
                }

                targetText.text = $"Target Selected\r\n<size=200%>[{scanableObjects.Count}]</size>";
            }
        }

        private void Activate()
        {
            scanableObjects = new();
            isActivate = true;
        }

        private void DeActivate()
        {
            ClearSelection();
            isActivate = false;
        }

        private void OnHackProcessed(string eventName)
        {
            alertText.text = $"{eventName}이(가) 적용되었습니다";
            currentTween?.Kill();

            // 새 트윈 생성
            currentTween = DOTween.Sequence()
                .Append(alertText.DOScale(1f, alertDuration).From(0)) // 커지는 애니메이션
                .AppendInterval(delayBeforeShrink)                   // 대기 시간
                .Append(alertText.DOScale(0f, alertDuration))  
                .SetUpdate(true)// 작아지는 애니메이션
                .OnComplete(() => Debug.Log("Alert animation completed!"));

            currentTween.Play();
            ClearSelection();
        }

        private void OnHackFailed(string eventName)
        {
            alertText.text = $"에너지가 부족합니다!";
            currentTween?.Kill();

            // 새 트윈 생성
            currentTween = DOTween.Sequence()
                .Append(alertText.DOScale(1f, alertDuration).From(0)) // 커지는 애니메이션
                .AppendInterval(delayBeforeShrink)                   // 대기 시간
                .Append(alertText.DOScale(0f, alertDuration))
                .SetUpdate(true)// 작아지는 애니메이션
                .OnComplete(() => Debug.Log("Alert animation completed!"));

            currentTween.Play();
            ClearSelection();
        }

        private void GetInput()
        {
            ctrlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            shiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }

        private void Select(Scanable scanable)
        {
            scanable.OnSelectEnter();
        }

        private void UnSelect(Scanable scanable)
        {
            scanable.OnSelectExit();
        }

        private void ClearSelection()
        {
            foreach (var obj in scanableObjects)
            {
                UnSelect(obj);
            }
            scanableObjects.Clear();
        }

        private void HandleVirtualClick()
        {
            Ray ray = virtualPointer.GetPlanePointerRay();
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * 100); // 레이 시각화

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, selectableLayer))
            {
                GameObject clickedObject = hit.collider.gameObject;
                if (clickedObject.TryGetComponent(out Scanable scanable))
                {
                    // 객체가 화면에서 가려졌는지 확인
                    if (IsFullyOccluded(hit.collider, selectableLayer))
                        return;

                    if (ctrlPressed)
                    {
                        // Ctrl + 클릭
                        if (scanableObjects.Contains(scanable))
                        {
                            UnSelect(scanable);
                            scanableObjects.Remove(scanable); // 그룹에서 제외
                        }
                    }
                    else
                    {
                        if (!scanableObjects.Contains(scanable))
                        {
                            scanableObjects.Add(scanable); // 그룹에 추가
                            Select(scanable);
                        }
                    }
                }
            }
        }

        private void HandleDragSelection()
        {
            Vector2 dragEndPos = virtualPointer.PlaneNormalizedPosition;
            Rect selectionRect = GetScreenRect(dragStartPos, dragEndPos);

            foreach (Scanable scanable in ScanableManager.Instance.scanable)
            {
                Collider[] colliders = scanable.GetComponentsInChildren<Collider>();
                bool isInSelection = false;

                foreach (Collider collider in colliders)
                {
                    if (IsColliderInSelection(collider, selectionRect))
                    {
                        if (!IsFullyOccluded(collider, selectableLayer))
                        {
                            isInSelection = true;
                            break; // 하나라도 포함되면 추가
                        }
                    }
                }

                if (isInSelection)
                {
                    if (ctrlPressed)
                    {
                        if (scanableObjects.Contains(scanable))
                        {
                            UnSelect(scanable);
                            scanableObjects.Remove(scanable); // 그룹에서 제외
                        }
                    }
                    else
                    {
                        if (!scanableObjects.Contains(scanable))
                        {
                            scanableObjects.Add(scanable); // 그룹에 추가
                            Select(scanable);
                        }
                    }
                }
            }
        }

        private bool IsFullyOccluded(Collider collider, LayerMask layerMask)
        {
            Camera cam = virtualPointer.planeCamera;
            Vector3[] samplePoints = GetColliderSamplePoints(collider);

            foreach (Vector3 point in samplePoints)
            {
                Vector3 screenPoint = cam.WorldToViewportPoint(point);
                if (screenPoint.z < 0 || screenPoint.x < 0 || screenPoint.x > 1 || screenPoint.y < 0 || screenPoint.y > 1)
                {
                    // 화면 밖에 있는 경우는 무시
                    continue;
                }

                Ray ray = new Ray(cam.transform.position, point - cam.transform.position);
                float distance = Vector3.Distance(cam.transform.position, point);

                if (!Physics.Raycast(ray, distance, layerMask))
                {
                    // 이 지점은 가려지지 않음
                    return false;
                }
            }

            // 모든 샘플 지점이 가려진 경우
            return true;
        }

        private Vector3[] GetColliderSamplePoints(Collider collider)
        {
            Bounds bounds = collider.bounds;

            return new Vector3[]
            {
                bounds.center, // 중심
                bounds.min, // AABB 최소 지점
                bounds.max, // AABB 최대 지점
                new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),
                new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),
                new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),
                new Vector3(bounds.max.x, bounds.max.y, bounds.min.z),
                new Vector3(bounds.max.x, bounds.min.y, bounds.max.z),
                new Vector3(bounds.min.x, bounds.max.y, bounds.max.z)
            };
        }

        private Rect GetScreenRect(Vector2 start, Vector2 end)
        {
            Vector2 topLeft = Vector2.Min(start, end);
            Vector2 bottomRight = Vector2.Max(start, end);
            return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
        }

        private bool IsColliderInSelection(Collider collider, Rect selectionRect)
        {
            Camera cam = virtualPointer.planeCamera;
            Bounds bounds = collider.bounds;

            Vector3[] corners = new Vector3[8];
            corners[0] = cam.WorldToViewportPoint(bounds.min);
            corners[1] = cam.WorldToViewportPoint(new Vector3(bounds.min.x, bounds.min.y, bounds.max.z));
            corners[2] = cam.WorldToViewportPoint(new Vector3(bounds.min.x, bounds.max.y, bounds.min.z));
            corners[3] = cam.WorldToViewportPoint(new Vector3(bounds.min.x, bounds.max.y, bounds.max.z));
            corners[4] = cam.WorldToViewportPoint(new Vector3(bounds.max.x, bounds.min.y, bounds.min.z));
            corners[5] = cam.WorldToViewportPoint(new Vector3(bounds.max.x, bounds.min.y, bounds.max.z));
            corners[6] = cam.WorldToViewportPoint(new Vector3(bounds.max.x, bounds.max.y, bounds.min.z));
            corners[7] = cam.WorldToViewportPoint(bounds.max);

            foreach (Vector3 corner in corners)
            {
                if (selectionRect.Contains(new Vector2(corner.x, corner.y), true))
                {
                    return true;
                }
            }

            return false;
        }
    }
}