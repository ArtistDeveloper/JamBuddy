using UnityEngine;

namespace Jambuddy.Adohi.Character.Particle
{
    public class DestroyOnParticleEnd : MonoBehaviour
    {
        private ParticleSystem particleSystem;

        void Awake()
        {
            // ���� GameObject�� ���� ParticleSystem ������Ʈ�� �����ɴϴ�.
            particleSystem = GetComponent<ParticleSystem>();

            if (particleSystem == null)
            {
                Debug.LogError("ParticleSystem�� �������� �ʾҽ��ϴ�! �� ��ũ��Ʈ�� ParticleSystem�� �ִ� GameObject�� �پ�� �մϴ�.");
            }
        }

        void Update()
        {
            // ��ƼŬ�� �����, �� �̻� Ȱ�� ���°� �ƴ� �� GameObject�� �����մϴ�.
            if (particleSystem != null && !particleSystem.IsAlive())
            {
                Destroy(gameObject);
            }
        }
    }

}
