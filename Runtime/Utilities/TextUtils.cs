using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CobaPlatinum.TextUtilities
{
    public class TextUtils : MonoBehaviour
    {
        public static string ColoredText(object _text, string _color)
        {
            return "<color=#" + _color + ">" + _text + "</color>";
        }

        public static string ColoredText(object _text, Color _color)
        {
            return "<color=#" + ColorUtility.ToHtmlStringRGB(_color) + ">" + _text + "</color>";
        }

        public static Color UnnormalizedColor(int _red, int _green, int _blue)
        {
            return new Color((_red / 255.0f), (_green / 255.0f), (_blue / 255.0f));
        }
    }
}
