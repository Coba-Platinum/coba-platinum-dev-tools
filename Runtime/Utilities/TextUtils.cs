using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}
