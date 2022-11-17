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
    private bool showCanvasConsoleSettings = false;

    SerializedProperty m_ShowDebugWindow;
    SerializedProperty m_DebugWindowAlignment;
    SerializedProperty m_TabIndex;
    SerializedProperty m_WindowRect;
    SerializedProperty m_MaxConsoleMessages;
    SerializedProperty m_UnityTag;
    SerializedProperty m_ConsoleTag;
    SerializedProperty m_UseCanvasMode;

    //New Debug Window
    SerializedProperty m_DebugModeCanvas;
    SerializedProperty m_DebugConsoleOutput;
    SerializedProperty m_DebugConsoleScrollRect;
    SerializedProperty m_ExposedFieldsOutput;
    SerializedProperty m_QuickActionReferenceObject;
    SerializedProperty m_QuickActionObjectList;
    SerializedProperty m_DebugWindowObject;
    SerializedProperty m_CommandInputField;
    SerializedProperty m_SuggestedCommandsBox;
    SerializedProperty m_SuggestedCommandsText;

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
        m_DebugWindowAlignment = serializedObject.FindProperty("alignment");
        m_UseCanvasMode = serializedObject.FindProperty("useCanvasMode");

        //New Console Window
        m_DebugModeCanvas = serializedObject.FindProperty("debugModeCanvas");
        m_DebugConsoleOutput = serializedObject.FindProperty("debugConsoleOutput");
        m_DebugConsoleScrollRect = serializedObject.FindProperty("debugConsoleScrollRect");
        m_ExposedFieldsOutput = serializedObject.FindProperty("exposedFieldsOutput");
        m_QuickActionReferenceObject = serializedObject.FindProperty("quickActionReferenceObject");
        m_QuickActionObjectList = serializedObject.FindProperty("quickActionObjectList");
        m_DebugWindowObject = serializedObject.FindProperty("debugWindowObject");
        m_CommandInputField = serializedObject.FindProperty("commandInputField");
        m_SuggestedCommandsBox = serializedObject.FindProperty("suggestedCommandsBox");
        m_SuggestedCommandsText = serializedObject.FindProperty("suggestedCommandsText");
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

        EditorGUILayout.BeginVertical(EditorStyles.toolbar);
        currentTab = GUILayout.Toolbar(currentTab, new string[] { "Debug Window", "Debug Console", "Exposed Variables", "Settings" }, EditorStyles.toolbarButton);
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
        GUILayout.Label("Debug Window Canvas", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(m_DebugModeCanvas, new GUIContent("Debug Canvas"));
        EditorGUILayout.LabelField("Current Tab Index", m_TabIndex.intValue.ToString());

        EditorGUILayout.EndVertical();

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
        showCanvasConsoleSettings = EditorGUILayout.Foldout(showCanvasConsoleSettings, "Canvas Debug Console");
        style.fontStyle = previousStyle;

        if (showCanvasConsoleSettings)
        {
            EditorGUILayout.PropertyField(m_DebugWindowObject, new GUIContent("Debug Window"));
            EditorGUILayout.PropertyField(m_DebugConsoleOutput, new GUIContent("Console Output Text"));
            EditorGUILayout.PropertyField(m_DebugConsoleScrollRect, new GUIContent("Console Scroll Rect"));
            EditorGUILayout.PropertyField(m_CommandInputField, new GUIContent("Command Input Field"));
            EditorGUILayout.PropertyField(m_SuggestedCommandsBox, new GUIContent("Suggested Commands Box"));
            EditorGUILayout.PropertyField(m_SuggestedCommandsText, new GUIContent("Suggested Commands Text"));
            EditorGUILayout.PropertyField(m_ExposedFieldsOutput, new GUIContent("Exposed Fields Text"));
            EditorGUILayout.PropertyField(m_QuickActionReferenceObject, new GUIContent("Quick Actions Reference Object"));
            EditorGUILayout.PropertyField(m_QuickActionObjectList, new GUIContent("Quick Actions List"));
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
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
    }

    private void DrawExposedVariablesInspector()
    {
        EditorGUILayout.BeginHorizontal("box");
        GUILayout.Label("DEBUG EXPOSED VARIABLES", EditorStyles.largeLabel);
        EditorGUILayout.EndHorizontal();
    }

    private void DrawSettingsInspector()
    {
        EditorGUILayout.BeginHorizontal("box");
        GUILayout.Label("SETTINGS", EditorStyles.largeLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Debug Window Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(m_WindowRect, new GUIContent("Editor Window Rect"));
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(m_DebugWindowAlignment, new GUIContent("Editor Window Alignment"));
        EditorGUILayout.PropertyField(m_UseCanvasMode, new GUIContent("Use Canvas Mode"));
        EditorGUILayout.EndVertical();
    }
}
