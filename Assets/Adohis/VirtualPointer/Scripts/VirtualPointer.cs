using UnityEngine;


namespace Jambuddy.Adohi.Character.Smartphone
{
    public class VirtualPointer : MonoBehaviour
    {
        public RectTransform virtualCursor; // 가상의 포인터 (UI 이미지 등)
        public float pointerSpeed = 1f; // 포인터 이동 속도 (0-1 범위에 맞춰 속도 조정)
        public Camera mainCamera; // 메인 카메라
        public Camera planeCamera; // 메인 카메라
        public GameObject targetPlane; // 3D Plane 오브젝트 (Collider가 필요)

        private Vector2 normalizedPosition = new Vector2(0.5f, 0.5f); // 가상 포인터의 노멀라이즈된 위치 (0-1)
        private Vector2 planeNormalizedPosition = new Vector2(0.5f, 0.5f);
        private Vector2 screenPosition = new Vector2(0.5f, 0.5f);
        public Vector2 NormalizedPosition => normalizedPosition; // 현재 가상 포인터 노멀라이즈된 위치
        public Vector2 PlaneNormalizedPosition => planeNormalizedPosition; // 현재 가상 포인터 노멀라이즈된 위치
        public bool IsLeftClick => Input.GetMouseButtonDown(0); // 왼쪽 클릭 여부
        public bool IsLeftClickReleased => Input.GetMouseButtonUp(0); // 왼쪽 클릭 해제 여부
        public bool IsLeftClickHeld => Input.GetMouseButton(0); // 왼쪽 클릭 유지 여부

        public RectTransform dragUI; // 미리 설정된 사각형 객체

        private bool isActivate;
        
        
        private Vector2 startMousePosition; // 드래그 시작 위치
        private bool isDragging = false; // 드래그 상태 확인


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

                if (Input.GetMouseButtonDown(0)) // 마우스 클릭 시작
                {
                    StartDrawing();
                }

                if (isDragging && Input.GetMouseButton(0)) // 마우스 드래그 중
                {
                    UpdateRectangle();
                }

                if (isDragging && Input.GetMouseButtonUp(0)) // 마우스를 놓음
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
            // 마우스 이동 값 (0-1 범위에 맞게 조정)
            Vector2 moveDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            normalizedPosition += moveDelta * pointerSpeed * Time.unscaledDeltaTime;

            // 0-1 범위를 넘지 않도록 클램프
            normalizedPosition = new Vector2(
                Mathf.Clamp01(normalizedPosition.x),
                Mathf.Clamp01(normalizedPosition.y)
            );


            MapScreenToPlane(normalizedPosition);


            virtualCursor.position = screenPosition;
        }

        private void UpdateCursorPosition()
        {
            // 노멀라이즈된 위치를 실제 화면 좌표로 변환
            Vector2 screenPosition = new Vector2(
                planeNormalizedPosition.x * Screen.width,
                planeNormalizedPosition.y * Screen.height
            );
            virtualCursor.position = screenPosition;
        }

        private Ray GetScreenPointerRay()
        {
            // 가상 포인터의 위치를 기준으로 Ray 반환
            Vector2 screenPosition = new Vector2(
                normalizedPosition.x * Screen.width,
                normalizedPosition.y * Screen.height
            );
            return mainCamera.ScreenPointToRay(screenPosition);
        }

        public Ray GetPlanePointerRay()
        {
            /*            // 가상 포인터의 위치를 기준으로 Ray 반환
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

            // Ray 생성
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
            // 1. 정규화된 플레인 좌표를 월드 좌표로 변환
            // 플레인의 크기
            Vector3 planeScale = targetPlane.transform.lossyScale;
            float planeWidth = 10f;  // 플레인의 실제 가로 크기
            float planeHeight = 10f; // 플레인의 실제 세로 크기

            // 플레인의 로컬 좌표 계산
            Vector3 localPoint = new Vector3(
                -(normalizedPlaneCoord.x - 0.5f) * planeWidth, // X축 변환
                0,                                           // 평면의 Y축 값
                -(normalizedPlaneCoord.y - 0.5f) * planeHeight // Z축 변환
            );

            // 로컬 좌표를 월드 좌표로 변환
            Vector3 worldPoint = targetPlane.transform.TransformPoint(localPoint);

            // 2. 월드 좌표를 스크린 좌표로 변환
            Vector3 screenPoint = mainCamera.WorldToScreenPoint(worldPoint);

            // 3. 스크린 좌표를 0-1 정규화 좌표로 변환
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

        // 플레인의 범위를 (0, 0)~(1, 1)로 제한
        private Vector2 ClampToPlaneBounds(Vector2 coord)
        {
            return new Vector2(
                Mathf.Clamp(coord.x, 0f, 1f),
                Mathf.Clamp(coord.y, 0f, 1f)
            );
        }

        void StartDrawing()
        {
            // 드래그 시작 위치 저장
            startMousePosition = screenPosition;

            // 사각형 활성화 및 초기화
            dragUI.gameObject.SetActive(true);
            dragUI.position = startMousePosition;
            dragUI.sizeDelta = Vector2.zero;

            isDragging = true;
        }

        void UpdateRectangle()
        {
            Vector2 currentMousePosition = screenPosition;
            Vector2 size = currentMousePosition - startMousePosition;

            // 사각형 위치와 크기 업데이트
            dragUI.sizeDelta = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));
            dragUI.position = startMousePosition + size / 2f;
        }

        void EndDrawing()
        {
            // 사각형 비활성화
            dragUI.gameObject.SetActive(false);
            isDragging = false;
        }

        void OnDrawGizmos()
        {
            if (mainCamera == null || targetPlane == null)
                return;
            // 레이 생성
            Ray ray = GetPlanePointerRay();
            Gizmos.color = Color.red;
            Gizmos.DrawLine(ray.origin, ray.origin + ray.direction * 100); // 레이 시각화

            // 플레인의 중심과 노멀 벡터
            Vector3 planeCenter = targetPlane.transform.position;
            Vector3 planeNormal = targetPlane.transform.up;

            // 플레인 시각화 (크기 설정)
            Vector3 planeRight = targetPlane.transform.right * targetPlane.transform.lossyScale.x;
            Vector3 planeForward = targetPlane.transform.forward * targetPlane.transform.lossyScale.z;
            Vector3 topLeft = planeCenter - planeRight * 0.5f + planeForward * 0.5f;
            Vector3 topRight = planeCenter + planeRight * 0.5f + planeForward * 0.5f;
            Vector3 bottomLeft = planeCenter - planeRight * 0.5f - planeForward * 0.5f;
            Vector3 bottomRight = planeCenter + planeRight * 0.5f - planeForward * 0.5f;

            // 플레인 외곽선 그리기
            Gizmos.color = Color.green;
            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);

            // 평면에 레이 투영 시점 계산
            Plane plane = new Plane(planeNormal, planeCenter);
            if (plane.Raycast(ray, out float distance))
            {
                Vector3 intersection = ray.GetPoint(distance);

                // 교차점 그리기
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(intersection, 0.1f);
            }
        }
    }



}
