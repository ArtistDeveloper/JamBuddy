using System.Threading;
using UnityEngine;

namespace Jambuddy.Junsu
{
    public class GameController : MonoBehaviour
    {
        private void Start()
        {
            EffectTargetManager.AddBlock("gravity");
            EffectTargetManager.AddBlock("rotation");
        }

        public void ApplyBlock()
        {
            EffectTargetManager.ApplyBlock();
        }
    }
}