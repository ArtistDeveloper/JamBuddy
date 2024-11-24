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
        [SerializeField] private RectTransform menuContainer; // �޴� UI�� ��� �θ� �����̳�
        [SerializeField] private GameObject menuItemPrefab;   // �޴� �׸��� ������
        [SerializeField] private float focusScale = 1.2f;     // ��Ŀ�� �� Ȯ�� ����
        [SerializeField] private float animationDuration = 0.2f; // �ִϸ��̼� �ð�
        [SerializeField] private float itemSpacing = 50f;     // �޴� �׸� �� ����

        [Header("Evnet")]
        public UnityEvent<int> onHackMenuSelected;

        private List<RectTransform> menuItems = new List<RectTransform>();
        private int currentIndex = 0;
        private RectTransform previousFocusedItem = null; // ������ ��Ŀ���� �׸�
        private RectTransform currentFocusedItem = null;  // ���� ��Ŀ���� �׸�
        private bool isActivate = false;
        private void Start()
        {
            GenerateInitialMenuItem();
            UpdateFocus(); // �ʱ� ��Ŀ�� ����
            UpdateMenuPosition(); // �ʱ� ����

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
            // ���� ��Ŀ�� �׸� �ִϸ��̼� ��� �� ���󺹱�
            if (previousFocusedItem != null)
            {
                previousFocusedItem.DOKill(); // ���� �ִϸ��̼� ���
                previousFocusedItem.DOScale(1.0f, animationDuration).SetEase(Ease.OutBack).SetUpdate(true);
            }

            // ���� ��Ŀ�� �׸� �ִϸ��̼�
            currentFocusedItem = menuItems[currentIndex];
            currentFocusedItem.DOKill(); // ���� �ִϸ��̼� ���
            currentFocusedItem.DOScale(focusScale, animationDuration).SetEase(Ease.OutBack).SetUpdate(true);

            // ���� ��Ŀ�� �׸� ������Ʈ
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

            // �� �޴� �׸� ��ġ ���
            float yOffset = -itemSpacing * (menuItems.Count - 1);
            rectTransform.anchoredPosition = new Vector2(0, yOffset);

            // �����̳� ������
            UpdateMenuPosition();
        }

        private void UpdateMenuPosition()
        {
            if (menuItems.Count == 0) return;

            // ��ü �޴� ���� ���
            float totalHeight = (menuItems.Count - 1) * itemSpacing;

            // �θ� �����̳ʸ� ��� ����
            menuContainer.anchoredPosition = new Vector2(
                menuContainer.anchoredPosition.x,
                totalHeight / 2
            );
        }
    }
}