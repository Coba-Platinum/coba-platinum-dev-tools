using UnityEngine;
using UnityEditor;
using System;

using CobaPlatinum.TextUtilities;
using CobaPlatinum.Utilities.Versioning;

public class CP_PackageBuild : EditorWindow
{
    GUIStyle headerStyle;
    Color headerColor = TextUtils.UnnormalizedColor(0, 168, 255);

    public string buildPath = "";
    public string packagePath = "";
    public string packageName = "";

    public int[] currentVersion;

    [MenuItem("Coba Platinum/Package Build")]
    public static void ShowWindow()
    {
        CP_PackageBuild window = GetWindow<CP_PackageBuild>("Package Build");
        window.minSize = new Vector2(600, 600);
        window.maxSize = new Vector2(600, 600);
    }

    private void OnGUI()
    {
        headerStyle = new GUIStyle(GUI.skin.box);
        headerStyle.normal.background = Texture2D.whiteTexture; // must be white to tint properly
        headerStyle.normal.textColor = Color.white; // whatever you want
        headerStyle.fontSize = 14;

        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = headerColor;
        GUILayout.FlexibleSpace();
        GUILayout.Box("COBA PLATINUM TOOLS - PACKAGE BUILD", headerStyle, GUILayout.Width(Screen.width));
        GUILayout.FlexibleSpace();
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Package Options", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Package Name");
        packageName = EditorGUILayout.TextField(packageName);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Popup("Package build for", 0, new string[] { "Coba Platinum Patcher" });
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Package Manifest", EditorStyles.boldLabel);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Build Bersion", EditorStyles.boldLabel);

        int autoIncrement = 0;
        if (CP_BuildVersionProcessor.autoIncrement)
            autoIncrement = 0;
        else
            autoIncrement = 1;

        autoIncrement = EditorGUILayout.Popup("Build Incrementing", autoIncrement, new string[] { "Automatic", "Manual" });

        if (autoIncrement == 0)
            CP_BuildVersionProcessor.autoIncrement = true;
        else
            CP_BuildVersionProcessor.autoIncrement = false;

        if(!CP_BuildVersionProcessor.autoIncrement)
            EditorGUILayout.HelpBox("The current build version will not automatically update when you build the project as long as build incrementing is set to manual!", MessageType.Warning);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Current Build Version - Major.Minor.Patch");
        EditorGUILayout.BeginHorizontal();
        if (!CP_BuildVersionProcessor.autoIncrement)
        {
            currentVersion[0] = EditorGUILayout.IntField(currentVersion[0]);
            currentVersion[1] = EditorGUILayout.IntField(currentVersion[1]);
            currentVersion[2] = EditorGUILayout.IntField(currentVersion[2]);
            if (GUILayout.Button("Set Version"))
            {
                CP_BuildVersionProcessor.SetVersion(currentVersion);
            }
        }
        else
        {
            currentVersion = CP_BuildVersionProcessor.FindCurrentVersion();

            EditorGUILayout.IntField(currentVersion[0]);
            EditorGUILayout.IntField(currentVersion[1]);
            EditorGUILayout.IntField(currentVersion[2]);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if(CP_BuildVersionProcessor.autoIncrement)
            CP_BuildVersionProcessor.buildType = (VersionBuildType)EditorGUILayout.EnumPopup("Version Build Level", CP_BuildVersionProcessor.buildType);

        CP_BuildVersionProcessor.devStage = (VersionDevStage)EditorGUILayout.EnumPopup("Version Build Level", CP_BuildVersionProcessor.devStage);
        CP_BuildVersionProcessor.devBuild = EditorGUILayout.Toggle("Version is DEV build", CP_BuildVersionProcessor.devBuild);

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Build Details", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Project Build Location");
        buildPath = EditorGUILayout.TextField(buildPath);
        if(GUILayout.Button("Browse"))
        {
            buildPath = EditorUtility.OpenFolderPanel("Select Build Folder", "", "");
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Create Package Location");
        packagePath = EditorGUILayout.TextField(packagePath);
        if (GUILayout.Button("Browse"))
        {
            packagePath = EditorUtility.OpenFolderPanel("Select Package Folder", "", "");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Build Options", EditorStyles.boldLabel);
        if(GUILayout.Button("Package Build"))
        {
            PackageBuild();
        }
        EditorGUILayout.EndVertical();
    }

    private void PackageBuild()
    {
        VerifySettings();
    }

    private bool VerifySettings()
    {
        if (buildPath.Equals(""))
        {
            EditorUtility.DisplayDialog("Failed to Build Package!", "The build location of the project has not been selected! Please select a build location and try again!", "Ok");
            return false;
        }

        if (packagePath.Equals(""))
        {
            EditorUtility.DisplayDialog("Failed to Build Package!", "The package location of the project has not been selected! Please select a package location and try again!", "Ok");
            return false;
        }

        if (packageName.Equals(""))
        {
            EditorUtility.DisplayDialog("Failed to Build Package!", "The package name of the project has not been selected! Please select a package name and try again!", "Ok");
            return false;
        }

        return true;
    }
}