using System;
using UnityEngine;

namespace Jambuddy.Junsu
{
    public class EffectTargetManager : MonoBehaviour
    {
        public static Action<string> onBlock;

        public static void ApplyBlock(string blockType)
        {
            if (onBlock == null)
            {
                Debug.Log("No registered OnBlockApplied");
                return;
            }
            onBlock.Invoke(blockType);
        }
    }

}
