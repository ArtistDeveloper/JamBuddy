using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.UI;

namespace Jambuddy.Adohi.UIs
{
    public class FillControlByAtom : MonoBehaviour
    {
        [Header("Health Settings")]
        public FloatReference minValue; // 최소 체력
        public FloatReference maxValue; // 최대 체력
        public FloatReference currentValue; // 현재 체력

        [Header("Fill Settings")]
        public float minFill = 0f; // 최소 fill 값 (0.0 ~ 1.0)
        public float maxFill = 1f; // 최대 fill 값 (0.0 ~ 1.0)

        [Header("UI Reference")]
        public Image fillImage; // UI Image (Fill 방식)

        void Update()
        {
            UpdateFillAmount();
        }

        private void UpdateFillAmount()
        {
            if (fillImage == null) return;

            // 체력을 기준으로 Fill Amount 계산
            float normalizedHealth = Mathf.InverseLerp(minValue.Value, maxValue.Value, currentValue.Value);
            fillImage.fillAmount = Mathf.Lerp(minFill, maxFill, normalizedHealth);
        }
    }

}
