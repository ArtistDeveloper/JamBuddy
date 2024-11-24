using UnityEngine;

namespace Jambuddy.Adohi.Character.Particle
{
    public class DestroyOnParticleEnd : MonoBehaviour
    {
        private ParticleSystem particleSystem;

        void Awake()
        {
            // 현재 GameObject에 붙은 ParticleSystem 컴포넌트를 가져옵니다.
            particleSystem = GetComponent<ParticleSystem>();

            if (particleSystem == null)
            {
                Debug.LogError("ParticleSystem이 감지되지 않았습니다! 이 스크립트는 ParticleSystem이 있는 GameObject에 붙어야 합니다.");
            }
        }

        void Update()
        {
            // 파티클이 멈췄고, 더 이상 활성 상태가 아닐 때 GameObject를 삭제합니다.
            if (particleSystem != null && !particleSystem.IsAlive())
            {
                Destroy(gameObject);
            }
        }
    }

}
