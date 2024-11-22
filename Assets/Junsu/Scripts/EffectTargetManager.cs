using System;
using UnityEngine;

public class EffectTargetManager : MonoBehaviour
{
    public static Action<string> OnBlockApplied;

    public void ApplyBlock(string blockType)
    {
        OnBlockApplied.Invoke(blockType);
    }
}
