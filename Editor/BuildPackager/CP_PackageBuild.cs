using UnityEngine;
using UnityEditor;
using System;

using CobaPlatinum.TextUtilities;
using CobaPlatinum.Utilities.Versioning;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;

public class CP_PackageBuild : EditorWindow
{
    GUIStyle headerStyle;
    Color headerColor = TextUtils.UnnormalizedColor(0, 168, 255);

    public string buildPath = "";
    public string packagePath = "";
    public string packageName = "";

    public int[] currentVersion;

    public PackageManifestAsset manifestAsset;

    [MenuItem("Coba Platinum/Package Build")]
    public static void ShowWindow()
    {
        CP_PackageBuild window = GetWindow<CP_PackageBuild>("Package Build");
        window.titleContent = new GUIContent("Package Build", EditorGUIUtility.ObjectContent(CreateInstance<CP_PackageBuild>(), typeof(CP_PackageBuild)).image);
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
        if (GUILayout.Button("Add Compression DLL"))
        {
            File.WriteAllText(Application.dataPath + "/csc.rsp", "-r:System.IO.Compression.FileSystem.dll");
            EditorUtility.DisplayDialog("Added missing DLL!", "Added missing DLL: System.IO.Compression.FileSystem.dll", "Ok");
        }
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal("box");
        GUILayout.Label("PACKAGE DETAILS", EditorStyles.largeLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Package Name");
        packageName = EditorGUILayout.TextField(packageName);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Popup("Package build for", 0, new string[] { "Coba Platinum Patcher" });
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal("box");
        GUILayout.Label("PACKAGE MANIFEST", EditorStyles.largeLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical("box");
        manifestAsset = (PackageManifestAsset)EditorGUILayout.ObjectField("Package Manifest Asset", manifestAsset, typeof(PackageManifestAsset), false);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal("box");
        GUILayout.Label("BUILD VERSION", EditorStyles.largeLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical("box");

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

        if (!CP_BuildVersionProcessor.autoIncrement)
        {
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("The current build version will not automatically update as long as 'Build Incrementing' is set to 'Manual'! " +
                "Set 'Build Incrementing' to 'Automatic' if you would like the version to dynamically update when you build the project!", MessageType.Warning);
        }

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

        if (CP_BuildVersionProcessor.autoIncrement)
            CP_BuildVersionProcessor.buildType = (VersionBuildType)EditorGUILayout.EnumPopup("Version Build Level", CP_BuildVersionProcessor.buildType);

        CP_BuildVersionProcessor.devStage = (VersionDevStage)EditorGUILayout.EnumPopup("Version Build Level", CP_BuildVersionProcessor.devStage);
        CP_BuildVersionProcessor.devBuild = EditorGUILayout.Toggle("Version is DEV build", CP_BuildVersionProcessor.devBuild);

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal("box");
        GUILayout.Label("BUILD DETAILS", EditorStyles.largeLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Project Build Location");
        buildPath = EditorGUILayout.TextField(buildPath);
        if (GUILayout.Button("Browse"))
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

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal("box");
        GUILayout.Label("PACKAGE OPTIONS", EditorStyles.largeLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Package Existing"))
        {
            PackageBuild();
        }

        if (GUILayout.Button("Build and Package"))
        {
            PackageBuild();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    private void PackageBuild()
    {
        VerifySettings();

        EditorUtility.DisplayProgressBar("Packaging Build", "initializing", 0);

        string exportPath = (packagePath + "/" + string.Format("{0}.{1}.{2}", currentVersion[0], currentVersion[1], currentVersion[2]));
        string fullPackagePath = (exportPath + "/" + packageName);

        try
        {
            EditorUtility.DisplayProgressBar("Packaging Build", "Creating package directory: " + fullPackagePath, 0.25f);
            // Determine whether the directory exists.
            if (Directory.Exists(fullPackagePath))
            {
                Debug.Log("That path exists already.");

                // Delete the directory.
                Directory.Delete(fullPackagePath, true);
                Debug.Log(fullPackagePath + " was deleted successfully.");
            }

            // Try to create the directory.
            DirectoryInfo di = Directory.CreateDirectory(fullPackagePath);
            Debug.Log(string.Format(di.FullName + " was created successfully at {0}.", Directory.GetCreationTime(fullPackagePath).ToString()));

            EditorUtility.DisplayProgressBar("Packaging Build", "Creating " + exportPath + "/Verstion.txt", 0.5f);

            // Create the file, or overwrite if the file exists.
            File.WriteAllText(exportPath + "/Verstion.txt", PlayerSettings.bundleVersion.ToString());
            Debug.Log(string.Format(exportPath + "/Verstion.txt" + " was created successfully at {0}.", File.GetCreationTime(exportPath + "\\Verstion.txt").ToString()));

            EditorUtility.DisplayProgressBar("Packaging Build", "Creating " + exportPath + "/Manifest.json", 0.75f);

            // Create the file, or overwrite if the file exists.
            File.WriteAllLines(exportPath + "/Manifest.json", CreateJsonContent());
            Debug.Log(string.Format(exportPath + "/Manifest.json" + " was created successfully at {0}.", File.GetCreationTime(exportPath + "\\Verstion.txt").ToString()));

            EditorUtility.DisplayProgressBar("Packaging Build", "Done", 1f);

            EditorUtility.DisplayDialog("Build Packaged!", string.Format("Build successfuly packaged! \nPackage Name: {0} \nVersion: {1} \nCreated at: {2}", packageName, PlayerSettings.bundleVersion, exportPath), "Ok");

            DirectoryInfo directoryInfo = new DirectoryInfo(buildPath);

            foreach (DirectoryInfo dir in directoryInfo.GetDirectories())
            {
                ZipFile.CreateFromDirectory(dir.FullName, fullPackagePath + "/" + dir.Name + ".zip");
                Directory.Delete(dir.FullName, true);
            }

            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                File.Move(file.FullName, fullPackagePath + "/" + file.Name);
            }

            ZipFile.CreateFromDirectory(fullPackagePath, packagePath + "/" + packageName + ".zip");
        }
        catch (Exception e)
        {
            Debug.Log(string.Format("The process failed: {0}", e.ToString()));
            EditorUtility.DisplayDialog("Package build failed!", string.Format("The process failed: {0}", e.ToString()), "Ok");
        }

        EditorUtility.ClearProgressBar();
    }

    public List<string> CreateJsonContent()
    {
        List<string> jsonLines = new List<string>();

        jsonLines.Add("{");
        jsonLines.Add(string.Format("\"alternateDirectory\":\"{0}\",", packageName));
        jsonLines.Add("\"filesToCopy\":[");

        for (int i = 0; i < manifestAsset.manifestFiles.Length; i++)
        {
            jsonLines.Add("   {\"fileName\":\"" + manifestAsset.manifestFiles[i].fileName + "\", \"fromDir\":\"" 
                + manifestAsset.manifestFiles[i].originDirectory + "\", \"toDir\":\"" + manifestAsset.manifestFiles[i].targetDirectory 
                + "\", \"zipFile\":\"" + manifestAsset.manifestFiles[i].isCompressed + "\"},");
        }
        jsonLines.Add("   {\"fileName\":\"" + manifestAsset.manifestFiles[manifestAsset.manifestFiles.Length - 1].fileName + "\", \"fromDir\":\""
                + manifestAsset.manifestFiles[manifestAsset.manifestFiles.Length - 1].originDirectory + "\", \"toDir\":\"" + manifestAsset.manifestFiles[manifestAsset.manifestFiles.Length - 1].targetDirectory
                + "\", \"zipFile\":\"" + manifestAsset.manifestFiles[manifestAsset.manifestFiles.Length - 1].isCompressed + "\"},");

        jsonLines.Add("}");

        return jsonLines;
    }

    private bool VerifySettings()
    {
        if (buildPath.Equals(""))
        {
            EditorUtility.DisplayDialog("Failed to Package Build!", "The build location of the project has not been selected! Please select a build location and try again!", "Ok");
            return false;
        }

        if (packagePath.Equals(""))
        {
            EditorUtility.DisplayDialog("Failed to Package Build!", "The package location of the project has not been selected! Please select a package location and try again!", "Ok");
            return false;
        }

        if (packageName.Equals(""))
        {
            EditorUtility.DisplayDialog("Failed to Package Build!", "The package name of the project has not been selected! Please select a package name and try again!", "Ok");
            return false;
        }

        return true;
    }
}
