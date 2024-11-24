using Drawing;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Jambuddy.Adohi.Selection
{
    [RequireComponent(typeof(Outline))]
    public class Scanable : MonoBehaviour
    {

        public string tooltipName;
        public string tooltip;
        private Outline outline;
        public float selectedOutlineWidth = 5f;
        

        void Start()
        {
            outline = GetComponent<Outline>();
            outline.OutlineWidth = 0f;
            outline.OutlineMode = Outline.Mode.OutlineVisible;

            Register();
        }

        public void Highlight(bool isHighlighted)
        {
            if (isHighlighted)
            {
                outline.OutlineWidth = selectedOutlineWidth;
            }
            else
            {
                outline.OutlineWidth = 0f;
            }
        }

        [Button]
        public void OnSelectEnter()
        {
            Highlight(true);
        }

        public void OnSelectExit()
        {
            Highlight(false);
        }

        private void Register()
        {
            ScanableManager.Instance.RegisterScanable(this);
        }

        private void UnRegister()
        {
            ScanableManager.Instance.UnRegisterScanable(this);
        }
    }

}
