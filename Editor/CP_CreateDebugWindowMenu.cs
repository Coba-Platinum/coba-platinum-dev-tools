using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Presets;
using UnityEditor.SceneManagement;
using CobaPlatinum.DebugTools;

//#if !UNITY_2021_2_OR_NEWER
using UnityEditor.Experimental.SceneManagement;
//#endif

namespace CobaPlatinum.EditorUtilities
{
    public class CP_CreateDebugWindowMenu
    {
        [MenuItem("Coba Platinum/DebugTools/Add Debug Window", false, 30)]
        static void CreateDebugWindowObject()
        {
            GameObject go = ObjectFactory.CreateGameObject("CP_Debug Window");

            // Add support for new prefab mode
            StageUtility.PlaceGameObjectInCurrentStage(go);

            CP_DebugWindow debugWindowComponent = ObjectFactory.AddComponent<CP_DebugWindow>(go);
        }
    }
}
