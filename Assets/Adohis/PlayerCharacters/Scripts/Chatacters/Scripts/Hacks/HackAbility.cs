using UnityEngine;

namespace Jambuddy.Adohi.Character.Hack
{
    [CreateAssetMenu(fileName = "HackAbility", menuName = "Scriptable Objects/HackAbility")]
    public class HackAbility : ScriptableObject
    {
        public string abilityName;
        public string abilitytoolTip;
        public float abilityCost;

    }

}
