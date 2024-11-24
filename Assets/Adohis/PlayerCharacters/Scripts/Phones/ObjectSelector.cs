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
        private Vector2 dragStartPos; // �巡�� ���� ��ġ
        private bool isDragging = false;
        private bool ctrlPressed;
        private bool shiftPressed;
        private bool isActivate;

        public VirtualPointer virtualPointer; // ���� ������ ���
        public LayerMask selectableLayer; // ���� ������ ��ü ���̾�
        [HideInInspector] public List<Scanable> scanableObjects = new(); // ���õ� ��ü��


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
            // �ܼ� Ŭ�� �� �巡�� ����
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

            // �巡�� ����
            if (virtualPointer.IsLeftClickReleased && isDragging)
            {
                isDragging = false;
            }

            // �巡�� ���̸� ����� �ð�ȭ
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
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * 100); // ���� �ð�ȭ

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, selectableLayer))
            {
                GameObject clickedObject = hit.collider.gameObject;
                if (clickedObject.TryGetComponent(out Scanable scanable))
                {
                    if (ctrlPressed)
                    {
                        // Ctrl + Ŭ��
                        if (scanableObjects.Contains(scanable))
                        {
                            UnSelect(scanable);
                            scanableObjects.Remove(scanable); // �׷쿡�� ����
                        }
                    }
                    else
                    {
                        if (!scanableObjects.Contains(scanable))
                        {
                            scanableObjects.Add(scanable); // �׷쿡 �߰�
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
                        break; // �ϳ��� ���ԵǸ� �߰�
                    }
                }

                if (isInSelection)
                {
                    if (ctrlPressed)
                    {
                        // Ctrl + Ŭ��
                        if (scanableObjects.Contains(scanable))
                        {
                            UnSelect(scanable);
                            scanableObjects.Remove(scanable); // �׷쿡�� ����
                        }
                    }
                    else
                    {
                        if (!scanableObjects.Contains(scanable))
                        {
                            scanableObjects.Add(scanable); // �׷쿡 �߰�
                            Select(scanable);
                        }
                    }
                    //UpdateSelectionVisuals();
                }
            }

            //UpdateSelectionVisuals();
        }

        /// <summary>
        /// �ݶ��̴��� ���� ������ ��ġ���� Ȯ��
        /// </summary>
        private bool IsColliderInSelection(Collider collider, Rect selectionRect)
        {
            Camera cam = virtualPointer.planeCamera;
            Bounds bounds = collider.bounds;

            // �޽��ݶ��̴��� ��� ���� �˻�
            if (collider is MeshCollider meshCollider && meshCollider.convex == false)
            {
                Mesh mesh = meshCollider.sharedMesh;
                if (mesh == null)
                    return false;

                Vector3[] vertices = mesh.vertices;
                foreach (Vector3 vertex in vertices)
                {
                    Vector3 worldPos = collider.transform.TransformPoint(vertex); // ���� -> ���� ��ǥ ��ȯ
                    Vector3 viewportPos = cam.WorldToViewportPoint(worldPos);

                    if (selectionRect.Contains(new Vector2(viewportPos.x, viewportPos.y), true))
                    {
                        return true;
                    }
                }

                return false;
            }

            // �Ϲ� �ݶ��̴��� AABB�� �������� �˻�
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
                SetSelectedVisual(obj, false); // ���� ǥ��
            }

            foreach (var obj in scanableObjects)
            {
                SetSelectedVisual(obj, true); // ���� ǥ��
            }
        }

        private void SetSelectedVisual(GameObject obj, bool isSelected)
        {
            *//*// ���õ� �ð� ȿ�� ó��
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
