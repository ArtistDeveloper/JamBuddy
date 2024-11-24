using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Jambuddy.Adohi.Character.Particle
{
    public class SetParticleColorByAtom : MonoBehaviour
    {
        private ParticleSystem _particleSystem;
        private ParticleSystem.MainModule main;
        public ColorReference colorReference;
        void Start()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            main = _particleSystem.main;
            main.startColor = colorReference.Value;
        }
    }

}
