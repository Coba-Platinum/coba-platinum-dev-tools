using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

using CobaPlatinum.DebugTools;
using CobaPlatinum.DebugTools.Console.DefaultCommands;
using CobaPlatinum.DebugTools.Console.DefaultExposedFields;
using CobaPlatinum.TextUtilities;

[CustomEditor(typeof(CP_DebugWindow))]
public class CP_DebugWindowEditor : Editor
{
    private CP_DebugWindow targetObject;
    private int currentTab;
    private bool showConsoleTests = false;

    SerializedProperty m_ShowDebugWindow;
    SerializedProperty m_TabIndex;
    SerializedProperty m_WindowRect;
    SerializedProperty m_MaxConsoleMessages;
    SerializedProperty m_UnityTag;
    SerializedProperty m_ConsoleTag;

    GUIStyle headerStyle;
    Color headerColor = TextUtils.UnnormalizedColor(0, 168, 255);

    private void OnEnable()
    {
        targetObject = (CP_DebugWindow)target;

        m_ShowDebugWindow = serializedObject.FindProperty("showDebugWindow");
        m_TabIndex = serializedObject.FindProperty("tabIndex");
        m_WindowRect = serializedObject.FindProperty("windowRect");
        m_MaxConsoleMessages = serializedObject.FindProperty("maxConsoleMessages");
        m_UnityTag = serializedObject.FindProperty("UNITY_TAG");
        m_ConsoleTag = serializedObject.FindProperty("PLATINUM_CONSOLE_TAG");
    }

    public override void OnInspectorGUI()
    {
        headerStyle = new GUIStyle(GUI.skin.box);
        headerStyle.normal.background = Texture2D.whiteTexture; // must be white to tint properly
        headerStyle.normal.textColor = Color.white; // whatever you want
        headerStyle.fontSize = 14;

        //base.OnInspectorGUI();

        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = headerColor;
        GUILayout.FlexibleSpace();
        GUILayout.Box("COBA PLATINUM DEBUG WINDOW", headerStyle, GUILayout.Width(Screen.width));
        GUILayout.FlexibleSpace();
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical("box");
        currentTab = GUILayout.Toolbar(currentTab, new string[] { "Debug Window", "Debug Console", "Exposed Variables", "Settings" });
        EditorGUILayout.EndVertical();

        switch (currentTab)
        {
            case 0:
                DrawDebugWindowInspector();
                break;
            case 1:
                DrawDebugConsoleInspector();
                break;
            case 2:
                DrawExposedVariablesInspector();
                break;
            case 3:
                DrawSettingsInspector();
                break;
            default:
                DrawDebugWindowInspector();
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawDebugWindowInspector()
    {
        EditorGUILayout.BeginHorizontal("box");
        GUILayout.Label("DEBUG WINDOW", EditorStyles.largeLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Debug Window Info", EditorStyles.boldLabel);

        m_ShowDebugWindow.intValue = EditorGUILayout.Popup("Debug Window State", m_ShowDebugWindow.intValue, new string[] { "Closed", "Open" });
        EditorGUILayout.LabelField("Current Tab Index", m_TabIndex.intValue.ToString());

        EditorGUILayout.EndVertical();
    }

    private void DrawDebugConsoleInspector()
    {
        EditorGUILayout.BeginHorizontal("box");
        GUILayout.Label("DEBUG CONSOLE", EditorStyles.largeLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Debug Console Output", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(m_MaxConsoleMessages);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Output Tags", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical();
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(m_UnityTag, new GUIContent("Unity Tag"));
        EditorGUILayout.PropertyField(m_ConsoleTag, new GUIContent("Console Tag"));
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        GUIStyle style = EditorStyles.foldout;
        FontStyle previousStyle = style.fontStyle;
        style.fontStyle = FontStyle.Bold;
        showConsoleTests = EditorGUILayout.Foldout(showConsoleTests, "Debug Console Tests");
        style.fontStyle = previousStyle;

        if (showConsoleTests)
        {
            EditorGUILayout.Space();

            if (GUILayout.Button("Send Test Unity Log"))
            {
                Debug.Log("This is a test Unity Log!");
            }
            if (GUILayout.Button("Send Test Unity Warning"))
            {
                Debug.LogWarning("This is a test Unity Warning!");
            }
            if (GUILayout.Button("Send Test Unity Error"))
            {
                Debug.LogError("This is a test Unity Error!");
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Send Test Unity Log"))
            {
                CP_DebugWindow.Log(this, "This is a test CP Console Log!");
            }
            if (GUILayout.Button("Send Test Unity Warning"))
            {
                CP_DebugWindow.LogWarning(this, "This is a test CP Console Warning!");
            }
            if (GUILayout.Button("Send Test Unity Error"))
            {
                CP_DebugWindow.LogError(this, "This is a test CP Console Error!");
            }

            EditorGUILayout.Space();
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Debug Console Commands", EditorStyles.boldLabel);

        EditorGUILayout.Space();
        if (GUILayout.Button("Add Default Commands"))
        {
            CP_DefaultCommands defaultCommandsComponent = ObjectFactory.AddComponent<CP_DefaultCommands>(targetObject.gameObject);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawExposedVariablesInspector()
    {
        EditorGUILayout.BeginHorizontal("box");
        GUILayout.Label("DEBUG EXPOSED VARIABLES", EditorStyles.largeLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Debug Console Exposed Fields", EditorStyles.boldLabel);

        EditorGUILayout.Space();
        if (GUILayout.Button("Add Default Exposed Fields"))
        {
            CP_DefaultExposedFields defaultExposedFieldsComponent = ObjectFactory.AddComponent<CP_DefaultExposedFields>(targetObject.gameObject);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawSettingsInspector()
    {
        EditorGUILayout.BeginHorizontal("box");
        GUILayout.Label("SETTINGS", EditorStyles.largeLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Debug Window Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(m_WindowRect, new GUIContent("Editor Window Rect"));
        EditorGUILayout.EndVertical();
    }
}
