using Jambuddy.Adohi.Character.Smartphone;
using Pixelplacement;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Jambuddy.Adohi.Character.Hack
{
    public class HackAbilityManager : Singleton<HackAbilityManager>
    {
        
        [Header("Hacks")]
        public HackAbility[] hackAbilities;

        [Header("UnityEvents")]
        public UnityEvent<string> onHackProcessed;
        public UnityEvent<string> onHackFailed;

        [HideInInspector] public ObjectSelector selector;

        

        public void ProcessHack(int hackIndex)
        {
            var amountOfTarget = selector.scanableObjects.Count;
            ProcessHack(hackIndex, amountOfTarget);
        }

        public bool ProcessHack(int hackIndex, int amountOfTarget)
        {
            if (hackIndex < 0 || hackIndex >= hackAbilities.Length)
            {
                Debug.LogError("Hack index is wrong");
                return false;
            }

            var requieredEnergy = hackAbilities[hackIndex].abilityCost * amountOfTarget;

            if (requieredEnergy > CharacterManager.Instance.currentEnergy)
            {
                FailHack();
                return false;
            }
            else
            {
                SuccessHack(hackIndex, amountOfTarget);
                return true;
            }
        }

        public void SuccessHack(int hackIndex, int amountOfTarget)
        {
            var requieredEnergy = hackAbilities[hackIndex].abilityCost * amountOfTarget;

            CharacterManager.Instance.currentEnergy.Value -= requieredEnergy;

            onHackProcessed.Invoke(hackAbilities[hackIndex].abilityName);
            Debug.Log("Hack Successed");
        }

        public void FailHack()
        {
            Debug.Log("Hack failed");
        }
    }

}
