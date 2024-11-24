using Jambuddy.Adohi.Character.Hack;
using System;
using UnityEngine;

namespace Jambuddy.Junsu
{
    public class EffectTargetManager : MonoBehaviour
    {
        public static Action<string> onAddBlock;
        public static Action onApplyEffect;

        private void OnEnable()
        {
            HackAbilityManager.Instance.onHackProcessed.AddListener(AddBlock);
        }

        private void OnDisable()
        {
            HackAbilityManager.Instance.onHackProcessed.RemoveListener(AddBlock);
        }

        public static void AddBlock(string blockType)
        {
            if (onAddBlock == null)
            {
                Debug.Log("No registered OnBlockApplied");
                return;
            }
            onAddBlock.Invoke(blockType);
        }

        public static void InvokeApplyBlock()
        {
            if (onApplyEffect == null)
            {
                Debug.Log("No registered onApplyEffect");
                return;
            }
            onApplyEffect.Invoke();
        }
    }
}
