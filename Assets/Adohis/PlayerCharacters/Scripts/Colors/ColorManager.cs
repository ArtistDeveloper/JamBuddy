using Pixelplacement;
using UniRx;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Jambuddy.Adohi.Colors
{
    public class ColorManager : Singleton<ColorManager>
    {
        public ColorReference neonPink;
        public ColorReference neonYellow;
        public ColorReference neonCyan;



        public float NeonPinkHue { get; private set; }
        public float NeonPinkSat { get; private set; }
        public float NeonPinkVal { get; private set; }
        public float NeonYellowHue { get; private set; }
        public float NeonYellowSat { get; private set; }
        public float NeonYellowVal { get; private set; }
        public float NeonCyanHue { get; private set; }
        public float NeonCyanSat { get; private set; }
        public float NeonCyanVal { get; private set; }


        private void Start()
        {
            neonPink.ObserveEveryValueChanged(c => c.Value).Subscribe(c => UpdateColorValues());
            neonYellow.ObserveEveryValueChanged(c => c.Value).Subscribe(c => UpdateColorValues());
            neonCyan.ObserveEveryValueChanged(c => c.Value).Subscribe(c => UpdateColorValues());
        }

        private void UpdateColorValues()
        {
            Color.RGBToHSV(neonPink, out var h, out var s, out var v);
            NeonPinkHue = h;
            NeonPinkSat = s;
            NeonPinkVal = v;
            Color.RGBToHSV(neonYellow, out h, out s, out v);
            NeonYellowHue = h;
            NeonYellowSat = s;
            NeonYellowVal = v;
            Color.RGBToHSV(neonCyan, out h, out s, out v);
            NeonCyanHue = h;
            NeonCyanSat = s;
            NeonCyanVal = v;
        }
    }

}
