using System.Collections.Generic;
using UnityEngine;

namespace Jambuddy.Adohi.UIs
{
    public class ColliderToUIBounds : MonoBehaviour
    {
        public Camera subCamera;  // UI가 연동될 카메라
        public Canvas uiCanvas;  // UI 캔버스
        public GameObject uiPrefab;  // 바운드 박스 UI 프리팹
        public LayerMask layerMask;  // 감지할 대상 레이어

        private Dictionary<Collider, GameObject> activeUIBoxes = new Dictionary<Collider, GameObject>();

        private void Start()
        {
            if (subCamera == null)
            {
                Debug.LogError("Sub camera is not assigned.");
                return;
            }

            if (uiCanvas == null)
            {
                Debug.LogError("Canvas is not assigned.");
                return;
            }
        }

        private void Update()
        {
            if (subCamera == null || uiCanvas == null) return;

            // 콜라이더가 포함된 모든 오브젝트를 추적하여 중복 없게 처리
            HashSet<GameObject> processedObjects = new HashSet<GameObject>();

            Collider[] visibleColliders = Physics.OverlapSphere(subCamera.transform.position, 1000f, layerMask);
            var planes = GeometryUtility.CalculateFrustumPlanes(subCamera);

            foreach (var col in visibleColliders)
            {
                GameObject obj = col.gameObject;

                if (processedObjects.Contains(obj))
                {
                    continue;
                }

                Collider[] colliders = obj.GetComponents<Collider>();
                Bounds combinedBounds = CalculateCombinedBounds(colliders);

                if (GeometryUtility.TestPlanesAABB(planes, combinedBounds))
                {
                    Vector3[] worldCorners = GetWorldCorners(combinedBounds);

                    bool anyPointVisible = false;
                    foreach (var point in worldCorners)
                    {
                        Vector3 direction = point - subCamera.transform.position;
                        Ray ray = new Ray(subCamera.transform.position, direction);
                        if (!Physics.Raycast(ray, out RaycastHit hit, direction.magnitude, layerMask) || hit.collider == col)
                        {
                            anyPointVisible = true;
                            break;
                        }
                    }

                    if (anyPointVisible)
                    {
                        if (!activeUIBoxes.ContainsKey(col))
                        {
                            GameObject newUI = Instantiate(uiPrefab, uiCanvas.transform);
                            activeUIBoxes[col] = newUI;
                        }
                        UpdateUIBounds(combinedBounds, activeUIBoxes[col]);
                        processedObjects.Add(obj);
                    }
                }
            }

            List<Collider> toRemove = new List<Collider>();
            foreach (var pair in activeUIBoxes)
            {
                if (!processedObjects.Contains(pair.Key.gameObject))
                {
                    Destroy(pair.Value);
                    toRemove.Add(pair.Key);
                }
            }

            foreach (var col in toRemove)
            {
                activeUIBoxes.Remove(col);
            }
        }

        private Bounds CalculateCombinedBounds(Collider[] colliders)
        {
            if (colliders.Length == 0)
            {
                return new Bounds();
            }

            Bounds combinedBounds = colliders[0].bounds;
            for (int i = 1; i < colliders.Length; i++)
            {
                combinedBounds.Encapsulate(colliders[i].bounds);
            }
            return combinedBounds;
        }

        private void UpdateUIBounds(Bounds bounds, GameObject uiBox)
        {
            RectTransform rectTransform = uiBox.GetComponent<RectTransform>();
            if (rectTransform == null) return;

            if (!IsBoundsValid(bounds)) return;

            Vector3[] worldCorners = GetWorldCorners(bounds);
            bool isVisible = false;
            foreach (var corner in worldCorners)
            {
                Vector3 viewportPoint = subCamera.WorldToViewportPoint(corner);
                if (viewportPoint.z > 0 &&
                    viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
                    viewportPoint.y >= 0 && viewportPoint.y <= 1)
                {
                    isVisible = true;
                    break;
                }
            }

            if (!isVisible)
            {
                uiBox.SetActive(false);
                return;
            }

            uiBox.SetActive(true);

            Vector2 screenMin = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 screenMax = new Vector2(float.MinValue, float.MinValue);

            foreach (var corner in worldCorners)
            {
                Vector3 viewportPoint = subCamera.WorldToViewportPoint(corner);

                if (viewportPoint.z > 0)
                {
                    screenMin = Vector2.Min(screenMin, new Vector2(viewportPoint.x, viewportPoint.y));
                    screenMax = Vector2.Max(screenMax, new Vector2(viewportPoint.x, viewportPoint.y));
                }
            }

            Vector2 size = screenMax - screenMin;
            rectTransform.anchorMin = screenMin;
            rectTransform.anchorMax = screenMax;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private Vector3[] GetWorldCorners(Bounds bounds)
        {
            return new Vector3[]
            {
        bounds.min,
        new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),
        new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),
        new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),
        new Vector3(bounds.max.x, bounds.max.y, bounds.min.z),
        new Vector3(bounds.max.x, bounds.min.y, bounds.max.z),
        new Vector3(bounds.min.x, bounds.max.y, bounds.max.z),
        bounds.max
            };
        }

        private bool IsBoundsValid(Bounds bounds)
        {
            return bounds.size != Vector3.zero &&
                   !float.IsInfinity(bounds.size.x) &&
                   !float.IsNaN(bounds.size.x) &&
                   !float.IsInfinity(bounds.size.y) &&
                   !float.IsNaN(bounds.size.y) &&
                   !float.IsInfinity(bounds.size.z) &&
                   !float.IsNaN(bounds.size.z);
        }
    }
}