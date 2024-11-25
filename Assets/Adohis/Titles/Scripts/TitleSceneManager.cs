using Cysharp.Threading.Tasks;
using Jambuddy.Adohi.Scenes;
using UnityEngine;

namespace Jambuddy.Adohi.Title
{
    public class TitleSceneManager : MonoBehaviour
    {
        public SceneTransition transition;

        public KeyCode startKey = KeyCode.Space;

        private bool isSceneLoadStart;

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(startKey))
            {
                if (!isSceneLoadStart)
                {
                    transition.LoadScene();
                }
            }
        }
    }

}
