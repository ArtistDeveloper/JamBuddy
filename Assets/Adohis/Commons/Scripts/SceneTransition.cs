using EasyTransition;
using UnityEngine;

namespace Jambuddy.Adohi.Scenes
{
    public class SceneTransition : MonoBehaviour
    {
        public int nextSceneIndex;
        public float loadDelay;
        public TransitionSettings transition;

        public void LoadTransition()
        {
            TransitionManager.Instance().Transition(transition, 0f);
        }

        public void LoadScene()
        {
            TransitionManager.Instance().Transition(nextSceneIndex, transition, loadDelay);
        }
    }


}
