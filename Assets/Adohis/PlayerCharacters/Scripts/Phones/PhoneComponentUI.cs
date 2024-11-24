using Cysharp.Threading.Tasks;
using DG.Tweening;
using Jambuddy.Adohi.Character.Hack;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Jambuddy.Adohi.Character.Smartphone
{
    public class PhoneComponentUI : MonoBehaviour
    {
        [SerializeField] private RectTransform menuContainer; // 메뉴 UI를 담는 부모 컨테이너
        [SerializeField] private GameObject menuItemPrefab;   // 메뉴 항목의 프리팹
        [SerializeField] private float focusScale = 1.2f;     // 포커스 시 확대 비율
        [SerializeField] private float animationDuration = 0.2f; // 애니메이션 시간
        [SerializeField] private float itemSpacing = 50f;     // 메뉴 항목 간 간격

        [Header("Evnet")]
        public UnityEvent<int> onHackMenuSelected;

        private List<RectTransform> menuItems = new List<RectTransform>();
        private int currentIndex = 0;
        private RectTransform previousFocusedItem = null; // 이전에 포커스된 항목
        private RectTransform currentFocusedItem = null;  // 현재 포커스된 항목
        private bool isActivate = false;
        private void Start()
        {
            GenerateInitialMenuItem();
            UpdateFocus(); // 초기 포커스 설정
            UpdateMenuPosition(); // 초기 정렬

        }

        private void OnEnable()
        {
            onHackMenuSelected.AddListener(HackAbilityManager.Instance.ProcessHack);
            CharacterModeManager.Instance.onHackModeEnter.AddListener(Activate);
            CharacterModeManager.Instance.onDefaultModeEnter.AddListener(DeActivate);
        }

        private void OnDisable()
        {
            onHackMenuSelected.AddListener(HackAbilityManager.Instance.ProcessHack);
            CharacterModeManager.Instance.onHackModeEnter.RemoveListener(Activate);
            CharacterModeManager.Instance.onDefaultModeEnter.RemoveListener(DeActivate);
        }

        private void Update()
        {
            if (isActivate)
            {
                HandleInput();
            }
        }

        private void Activate()
        {
            isActivate = true;
        }

        private void DeActivate()
        {
            isActivate = false;
        }

        private void GenerateInitialMenuItem()
        {
            foreach (var hackAbility in HackAbilityManager.Instance.hackAbilities)
            {
                AddMenuItem(hackAbility.abilityName);
            }
        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                ChangeFocus(-1);
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                ChangeFocus(1);
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                SelectMenuItem();
            }
        }

        private void ChangeFocus(int direction)
        {
            int previousIndex = currentIndex;
            currentIndex = Mathf.Clamp(currentIndex + direction, 0, menuItems.Count - 1);

            if (previousIndex != currentIndex)
            {
                UpdateFocus();
            }
        }

        private void UpdateFocus()
        {
            // 이전 포커스 항목 애니메이션 취소 및 원상복구
            if (previousFocusedItem != null)
            {
                previousFocusedItem.DOKill(); // 이전 애니메이션 취소
                previousFocusedItem.DOScale(1.0f, animationDuration).SetEase(Ease.OutBack).SetUpdate(true);
            }

            // 현재 포커스 항목 애니메이션
            currentFocusedItem = menuItems[currentIndex];
            currentFocusedItem.DOKill(); // 현재 애니메이션 취소
            currentFocusedItem.DOScale(focusScale, animationDuration).SetEase(Ease.OutBack).SetUpdate(true);

            // 이전 포커스 항목 업데이트
            previousFocusedItem = currentFocusedItem;
        }

        private void SelectMenuItem()
        {
            onHackMenuSelected.Invoke(currentIndex);
        }

        [Button]
        public void AddMenuItem(string labelText)
        {
            GameObject newItem = Instantiate(menuItemPrefab, menuContainer);
            TextMeshProUGUI label = newItem.GetComponentInChildren<TextMeshProUGUI>();
            label.text = labelText;

            RectTransform rectTransform = newItem.GetComponent<RectTransform>();
            menuItems.Add(rectTransform);

            // 새 메뉴 항목 위치 계산
            float yOffset = -itemSpacing * (menuItems.Count - 1);
            rectTransform.anchoredPosition = new Vector2(0, yOffset);

            // 컨테이너 재정렬
            UpdateMenuPosition();
        }

        private void UpdateMenuPosition()
        {
            if (menuItems.Count == 0) return;

            // 전체 메뉴 높이 계산
            float totalHeight = (menuItems.Count - 1) * itemSpacing;

            // 부모 컨테이너를 가운데 정렬
            menuContainer.anchoredPosition = new Vector2(
                menuContainer.anchoredPosition.x,
                totalHeight / 2
            );
        }
    }
}