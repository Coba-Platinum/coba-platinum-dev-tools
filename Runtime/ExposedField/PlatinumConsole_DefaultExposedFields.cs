using CobaPlatinum.DebugTools.ExposedFields;
using UnityEngine;

namespace CobaPlatinum.DebugTools.PlatinumConsole.DefaultExposedFields
{
    public class PlatinumConsole_DefaultExposedFields : MonoBehaviour
    {
        [ExposedField]
        public float timeScale;

        void Update()
        {
            timeScale = Time.timeScale;
        }
    }
}
