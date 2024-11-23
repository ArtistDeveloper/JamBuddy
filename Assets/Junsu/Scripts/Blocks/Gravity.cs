using Unity.VisualScripting;
using UnityEngine;

namespace Jambuddy.Junsu
{
    public class Gravity : Block
    {
        public override void ApplyEffect(EffectTarget target)
        {
            target.AddComponent<Rigidbody>();
        }
    }
}

