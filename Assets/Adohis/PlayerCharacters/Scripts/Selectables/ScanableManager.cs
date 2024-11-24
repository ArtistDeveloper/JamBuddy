using Pixelplacement;
using System.Collections.Generic;
using UnityEngine;



namespace Jambuddy.Adohi.Selection
{
    public class ScanableManager : Singleton<ScanableManager>
    {
        public List<Scanable> scanable = new();

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void RegisterScanable(Scanable selectable)
        {
            if (scanable.Contains(selectable))
            {
                Debug.LogError("Scanable already registered");
                return;
            }
            scanable.Add(selectable);
        }

        public void UnRegisterScanable(Scanable selectable)
        {
            if (!scanable.Contains(selectable))
            {
                Debug.LogWarning("Scanable was not registered");
                return;
            }
            scanable.Remove(selectable);
        }
    }


}
