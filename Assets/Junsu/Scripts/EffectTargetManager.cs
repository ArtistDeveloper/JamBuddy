using System;
using UnityEngine;

namespace Jambuddy.Junsu
{
    public class EffectTargetManager : MonoBehaviour
    {
        public static Action<string> onAddBlock;
        public static Action onApplyEffect;

        public static void AddBlock(string blockType)
        {
            if (onAddBlock == null)
            {
                Debug.Log("No registered OnBlockApplied");
                return;
            }
            onAddBlock.Invoke(blockType);
        }

        public static void ApplyBlock()
        {
            onApplyEffect.Invoke();
        }
    }
}
