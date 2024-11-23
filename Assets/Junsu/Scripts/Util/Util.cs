using Jambuddy.Junsu;
using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Jambuddy.Junsu
{
    public class Util
    {
        public static string[] GetNamesOfEnumElement(Type type)
        {
            string[] names = Enum.GetNames(type);
            return names;
        }
    }
}
