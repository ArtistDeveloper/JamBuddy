using Jambuddy.Adohi.Character.Hack;
using Jambuddy.Adohi.Selection;
using Micosmo.SensorToolkit;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

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


        private void Start()
        {
            HackAbilityManager.Instance.selector = this;
        }

        private void OnEnable()
        {
            CharacterModeManager.Instance.onHackModeStart.AddListener(Activate);
            CharacterModeManager.Instance.onDefaultModeStart.AddListener(DeActivate);
            HackAbilityManager.Instance.onHackProcessed.AddListener(_ => ClearSelection());
            HackAbilityManager.Instance.onHackFailed.AddListener(_ => ClearSelection());
        }

        private void OnDisable()
        {
            CharacterModeManager.Instance.onHackModeStart.RemoveListener(Activate);
            CharacterModeManager.Instance.onDefaultModeStart.RemoveListener(DeActivate);
            HackAbilityManager.Instance.onHackProcessed.RemoveListener(_ => ClearSelection());
            HackAbilityManager.Instance.onHackFailed.RemoveListener(_ => ClearSelection());
        }

        void LateUpdate()
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
                //DrawDragRectangle();
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
                    //UpdateSelectionVisuals();
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
                        isInSelection = true;
                        break; // 하나라도 포함되면 추가
                    }
                }

                if (isInSelection)
                {
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
                    //UpdateSelectionVisuals();
                }
            }

            //UpdateSelectionVisuals();
        }

        /// <summary>
        /// 콜라이더가 선택 영역에 겹치는지 확인
        /// </summary>
        private bool IsColliderInSelection(Collider collider, Rect selectionRect)
        {
            Camera cam = virtualPointer.planeCamera;
            Bounds bounds = collider.bounds;

            // 메쉬콜라이더인 경우 정밀 검사
            if (collider is MeshCollider meshCollider && meshCollider.convex == false)
            {
                Mesh mesh = meshCollider.sharedMesh;
                if (mesh == null)
                    return false;

                Vector3[] vertices = mesh.vertices;
                foreach (Vector3 vertex in vertices)
                {
                    Vector3 worldPos = collider.transform.TransformPoint(vertex); // 로컬 -> 월드 좌표 변환
                    Vector3 viewportPos = cam.WorldToViewportPoint(worldPos);

                    if (selectionRect.Contains(new Vector2(viewportPos.x, viewportPos.y), true))
                    {
                        return true;
                    }
                }

                return false;
            }

            // 일반 콜라이더의 AABB를 기준으로 검사
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

/*
        private void UpdateSelectionVisuals()
        {
            foreach (var obj in FindObjectsByLayer(selectableLayer))
            {
                SetSelectedVisual(obj, false); // 선택 표시
            }

            foreach (var obj in scanableObjects)
            {
                SetSelectedVisual(obj, true); // 선택 표시
            }
        }

        private void SetSelectedVisual(GameObject obj, bool isSelected)
        {
            *//*// 선택된 시각 효과 처리
            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = isSelected ? Color.green : Color.white;
            }*//*

            if (obj.TryGetComponent(out Selectable selectable))
            {
                if (isSelected)
                {
                    selectable.OnSelectEnter();
                }
                else
                {
                    selectable.OnSelectExit();
                }
            }
            else
            {
                var renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = isSelected ? Color.green : Color.white;
                }
                
            }
        }
*/
        private Rect GetScreenRect(Vector2 start, Vector2 end)
        {
            Vector2 topLeft = Vector2.Min(start, end);
            Vector2 bottomRight = Vector2.Max(start, end);
            return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
        }

        private List<GameObject> FindObjectsByLayer(LayerMask layer)
        {
            List<GameObject> objects = new List<GameObject>();
            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(sortMode: FindObjectsSortMode.InstanceID);

            foreach (GameObject obj in allObjects)
            {
                if (((1 << obj.layer) & layer) != 0)
                {
                    objects.Add(obj);
                }
            }

            return objects;
        }
    }
}
