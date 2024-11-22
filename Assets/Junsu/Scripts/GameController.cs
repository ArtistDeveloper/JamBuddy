using System.Threading;
using UnityEngine;

namespace Jambuddy.Junsu
{
    public class GameController : MonoBehaviour
    {
        private void Start()
        {
            EffectTargetManager.ApplyBlock("gravity");
        }
    }
}