using EasyTransition;
using Jambuddy.Adohi.Scenes;
using Jambuddy.Adohi.Title;
using Pixelplacement;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Jambuddy.Adohi.Character
{
    public class CharacterManager : Singleton<CharacterManager>
    {
        public FloatReference maxHealth;
        public FloatReference currentHealth;
        public FloatReference maxStamina;
        public FloatReference currentStamina;
        public FloatReference maxEnergy;
        public FloatReference currentEnergy;

        public SceneTransition sceneTransition;

        private bool isSceneTranstioning;

        private void Awake()
        {
            currentHealth.Value = maxHealth;
            currentStamina.Value = maxStamina;
            currentEnergy.Value = maxEnergy;
        }
        private void Update()
        {
            if (currentHealth <= 0f && !isSceneTranstioning)
            {
                GameEnd();
            }
        }

        public void GetDamage(float damage)
        {
            currentHealth.Value -= damage;
        }

        public void GameEnd()
        {
            isSceneTranstioning = true;
            sceneTransition.LoadScene();
        }
    }
}
