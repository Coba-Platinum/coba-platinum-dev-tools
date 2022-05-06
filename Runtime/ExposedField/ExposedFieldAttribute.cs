using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CobaPlatinum.DebugTools.ExposedFields
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ExposedFieldAttribute : Attribute
    {
        public string displayName;

        public ExposedFieldAttribute(string _displayName)
        {
            displayName = _displayName;
        }

        public ExposedFieldAttribute()
        {

        }
    }
}
