using UnityEngine;


namespace Jambuddy.Adohi.Character.Smartphone
{
    public class VirtualPointer : MonoBehaviour
    {
        public RectTransform virtualCursor; // ������ ������ (UI �̹��� ��)
        public float pointerSpeed = 1f; // ������ �̵� �ӵ� (0-1 ������ ���� �ӵ� ����)
        public Camera mainCamera; // ���� ī�޶�
        public Camera planeCamera; // ���� ī�޶�
        public GameObject targetPlane; // 3D Plane ������Ʈ (Collider�� �ʿ�)

        private Vector2 normalizedPosition = new Vector2(0.5f, 0.5f); // ���� �������� ��ֶ������ ��ġ (0-1)
        private Vector2 planeNormalizedPosition = new Vector2(0.5f, 0.5f);
        private Vector2 screenPosition = new Vector2(0.5f, 0.5f);
        public Vector2 NormalizedPosition => normalizedPosition; // ���� ���� ������ ��ֶ������ ��ġ
        public Vector2 PlaneNormalizedPosition => planeNormalizedPosition; // ���� ���� ������ ��ֶ������ ��ġ
        public bool IsLeftClick => Input.GetMouseButtonDown(0); // ���� Ŭ�� ����
        public bool IsLeftClickReleased => Input.GetMouseButtonUp(0); // ���� Ŭ�� ���� ����
        public bool IsLeftClickHeld => Input.GetMouseButton(0); // ���� Ŭ�� ���� ����

        public RectTransform dragUI; // �̸� ������ �簢�� ��ü

        private bool isActivate;
        
        
        private Vector2 startMousePosition; // �巡�� ���� ��ġ
        private bool isDragging = false; // �巡�� ���� Ȯ��


        private void OnEnable()
        {
            CharacterModeManager.Instance.onHackModeEnter.AddListener(Activate);
            CharacterModeManager.Instance.onDefaultModeEnter.AddListener(DeActivate);

        }

        private void OnDisable()
        {
            CharacterModeManager.Instance.onHackModeEnter.RemoveListener(Activate);
            CharacterModeManager.Instance.onDefaultModeEnter.RemoveListener(DeActivate);
        }

        void Update()
        {
            if (isActivate)
            {
                MovePointer();

                if (Input.GetMouseButtonDown(0)) // ���콺 Ŭ�� ����
                {
                    StartDrawing();
                }

                if (isDragging && Input.GetMouseButton(0)) // ���콺 �巡�� ��
                {
                    UpdateRectangle();
                }

                if (isDragging && Input.GetMouseButtonUp(0)) // ���콺�� ����
                {
                    EndDrawing();
                }
            }
        }

        private void Activate()
        {
            normalizedPosition = new Vector2(0.5f, 0.5f);
            planeNormalizedPosition = new Vector2(0.5f, 0.5f);
            screenPosition = new Vector2(0.5f, 0.5f);
            virtualCursor.gameObject.SetActive(true);
            isActivate = true;
        }

        private void DeActivate()
        {
            virtualCursor.gameObject.SetActive(false);
            isActivate = false;
        }


        private void MovePointer()
        {
            // ���콺 �̵� �� (0-1 ������ �°� ����)
            Vector2 moveDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            normalizedPosition += moveDelta * pointerSpeed * Time.unscaledDeltaTime;

            // 0-1 ������ ���� �ʵ��� Ŭ����
            normalizedPosition = new Vector2(
                Mathf.Clamp01(normalizedPosition.x),
                Mathf.Clamp01(normalizedPosition.y)
            );


            MapScreenToPlane(normalizedPosition);


            virtualCursor.position = screenPosition;
        }

        private void UpdateCursorPosition()
        {
            // ��ֶ������ ��ġ�� ���� ȭ�� ��ǥ�� ��ȯ
            Vector2 screenPosition = new Vector2(
                planeNormalizedPosition.x * Screen.width,
                planeNormalizedPosition.y * Screen.height
            );
            virtualCursor.position = screenPosition;
        }

        private Ray GetScreenPointerRay()
        {
            // ���� �������� ��ġ�� �������� Ray ��ȯ
            Vector2 screenPosition = new Vector2(
                normalizedPosition.x * Screen.width,
                normalizedPosition.y * Screen.height
            );
            return mainCamera.ScreenPointToRay(screenPosition);
        }

        public Ray GetPlanePointerRay()
        {
            /*            // ���� �������� ��ġ�� �������� Ray ��ȯ
                        Vector2 screenPosition = new Vector2(
                            planeNormalizedPosition.x * Screen.width,
                            planeNormalizedPosition.y * Screen.height
                        );
                        //return planeCamera.ScreenPointToRay(screenPosition);*/
            var ray = planeCamera.ViewportPointToRay(new Vector3(planeNormalizedPosition.x, planeNormalizedPosition.y, 0f));

            return ray;
        }

        public Vector2 MapScreenToPlane(Vector2 screenCoord)
        {
            if (targetPlane == null)
            {
                Debug.LogWarning("Target plane not assigned!");
                return Vector2.zero;
            }

            // Ray ����
            Ray ray = GetScreenPointerRay();

            Vector3 planeCenter = targetPlane.transform.position;
            Vector3 planeNormal = targetPlane.transform.up;
            Vector3 localPoint = Vector2.zero;
            Plane plane = new Plane(planeNormal, planeCenter);
            if (plane.Raycast(ray, out float distance))
            {
                Vector3 intersection = ray.GetPoint(distance);
                localPoint = targetPlane.transform.InverseTransformPoint(intersection);
            }
            Vector3 localNormalizedPoint = new Vector3(
                    -(localPoint.x / 10f) + 0.5f,
                    0f,
                    -(localPoint.z / 10f) + 0.5f
                );

            normalizedPosition = MapPlaneToScreen(ClampToPlaneBounds(new Vector2(localNormalizedPoint.x, localNormalizedPoint.z)));
            planeNormalizedPosition = ClampToPlaneBounds(new Vector2(localNormalizedPoint.x, localNormalizedPoint.z));
            return planeNormalizedPosition;
        }

        public Vector2 MapPlaneToScreen(Vector2 normalizedPlaneCoord)
        {
            // 1. ����ȭ�� �÷��� ��ǥ�� ���� ��ǥ�� ��ȯ
            // �÷����� ũ��
            Vector3 planeScale = targetPlane.transform.lossyScale;
            float planeWidth = 10f;  // �÷����� ���� ���� ũ��
            float planeHeight = 10f; // �÷����� ���� ���� ũ��

            // �÷����� ���� ��ǥ ���
            Vector3 localPoint = new Vector3(
                -(normalizedPlaneCoord.x - 0.5f) * planeWidth, // X�� ��ȯ
                0,                                           // ����� Y�� ��
                -(normalizedPlaneCoord.y - 0.5f) * planeHeight // Z�� ��ȯ
            );

            // ���� ��ǥ�� ���� ��ǥ�� ��ȯ
            Vector3 worldPoint = targetPlane.transform.TransformPoint(localPoint);

            // 2. ���� ��ǥ�� ��ũ�� ��ǥ�� ��ȯ
            Vector3 screenPoint = mainCamera.WorldToScreenPoint(worldPoint);

            // 3. ��ũ�� ��ǥ�� 0-1 ����ȭ ��ǥ�� ��ȯ
            Vector2 normalizedScreenCoord = new Vector2(
                screenPoint.x / Screen.width,
                screenPoint.y / Screen.height
            );

            Vector2 screenPosition = new Vector2(
                normalizedScreenCoord.x * Screen.width,
                normalizedScreenCoord.y * Screen.height
            );
            this.screenPosition = screenPosition;
            return normalizedScreenCoord;
        }

        // �÷����� ������ (0, 0)~(1, 1)�� ����
        private Vector2 ClampToPlaneBounds(Vector2 coord)
        {
            return new Vector2(
                Mathf.Clamp(coord.x, 0f, 1f),
                Mathf.Clamp(coord.y, 0f, 1f)
            );
        }

        void StartDrawing()
        {
            // �巡�� ���� ��ġ ����
            startMousePosition = screenPosition;

            // �簢�� Ȱ��ȭ �� �ʱ�ȭ
            dragUI.gameObject.SetActive(true);
            dragUI.position = startMousePosition;
            dragUI.sizeDelta = Vector2.zero;

            isDragging = true;
        }

        void UpdateRectangle()
        {
            Vector2 currentMousePosition = screenPosition;
            Vector2 size = currentMousePosition - startMousePosition;

            // �簢�� ��ġ�� ũ�� ������Ʈ
            dragUI.sizeDelta = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));
            dragUI.position = startMousePosition + size / 2f;
        }

        void EndDrawing()
        {
            // �簢�� ��Ȱ��ȭ
            dragUI.gameObject.SetActive(false);
            isDragging = false;
        }

        void OnDrawGizmos()
        {
            if (mainCamera == null || targetPlane == null)
                return;
            // ���� ����
            Ray ray = GetPlanePointerRay();
            Gizmos.color = Color.red;
            Gizmos.DrawLine(ray.origin, ray.origin + ray.direction * 100); // ���� �ð�ȭ

            // �÷����� �߽ɰ� ��� ����
            Vector3 planeCenter = targetPlane.transform.position;
            Vector3 planeNormal = targetPlane.transform.up;

            // �÷��� �ð�ȭ (ũ�� ����)
            Vector3 planeRight = targetPlane.transform.right * targetPlane.transform.lossyScale.x;
            Vector3 planeForward = targetPlane.transform.forward * targetPlane.transform.lossyScale.z;
            Vector3 topLeft = planeCenter - planeRight * 0.5f + planeForward * 0.5f;
            Vector3 topRight = planeCenter + planeRight * 0.5f + planeForward * 0.5f;
            Vector3 bottomLeft = planeCenter - planeRight * 0.5f - planeForward * 0.5f;
            Vector3 bottomRight = planeCenter + planeRight * 0.5f - planeForward * 0.5f;

            // �÷��� �ܰ��� �׸���
            Gizmos.color = Color.green;
            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);

            // ��鿡 ���� ���� ���� ���
            Plane plane = new Plane(planeNormal, planeCenter);
            if (plane.Raycast(ray, out float distance))
            {
                Vector3 intersection = ray.GetPoint(distance);

                // ������ �׸���
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(intersection, 0.1f);
            }
        }
    }



}
