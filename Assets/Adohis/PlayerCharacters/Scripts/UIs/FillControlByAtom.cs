using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.UI;

namespace Jambuddy.Adohi.UIs
{
    public class FillControlByAtom : MonoBehaviour
    {
        [Header("Health Settings")]
        public FloatReference minValue; // �ּ� ü��
        public FloatReference maxValue; // �ִ� ü��
        public FloatReference currentValue; // ���� ü��

        [Header("Fill Settings")]
        public float minFill = 0f; // �ּ� fill �� (0.0 ~ 1.0)
        public float maxFill = 1f; // �ִ� fill �� (0.0 ~ 1.0)

        [Header("UI Reference")]
        public Image fillImage; // UI Image (Fill ���)

        void Update()
        {
            UpdateFillAmount();
        }

        private void UpdateFillAmount()
        {
            if (fillImage == null) return;

            // ü���� �������� Fill Amount ���
            float normalizedHealth = Mathf.InverseLerp(minValue.Value, maxValue.Value, currentValue.Value);
            fillImage.fillAmount = Mathf.Lerp(minFill, maxFill, normalizedHealth);
        }
    }

}
