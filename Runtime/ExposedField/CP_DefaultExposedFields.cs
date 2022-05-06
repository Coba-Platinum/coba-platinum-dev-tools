using CobaPlatinum.DebugTools.ExposedFields;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CobaPlatinum.DebugTools.Console.DefaultExposedFields
{
    public class CP_DefaultExposedFields : MonoBehaviour
    {
        [ExposedField]
        public float timeScale;

        void Update()
        {
            timeScale = Time.timeScale;
        }
    }
}
