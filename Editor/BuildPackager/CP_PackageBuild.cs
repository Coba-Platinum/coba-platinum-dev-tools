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

    PackageBuildData packageBuildData;

    [MenuItem("Coba Platinum/Package Build")]
    public static void ShowWindow()
    {
        CP_PackageBuild window = GetWindow<CP_PackageBuild>("Package Build");
        window.titleContent = new GUIContent("Package Build", EditorGUIUtility.ObjectContent(CreateInstance<CP_PackageBuild>(), typeof(CP_PackageBuild)).image);
        window.minSize = new Vector2(600, 580);
        window.maxSize = new Vector2(600, 580);
    }

    private void OnGUI()
    {
        if(!File.Exists("Assets/Package Build Data.asset"))
        {
            PackageBuildData asset = ScriptableObject.CreateInstance<PackageBuildData>();
            AssetDatabase.CreateAsset(asset, "Assets/Package Build Data.asset");
            Debug.Log("Created Package Build Data.asset at " + "Assets/Package Build Data.asset");
            AssetDatabase.SaveAssets();
            return;
        }
        else if(packageBuildData == null)
        {
            packageBuildData = AssetDatabase.LoadAssetAtPath<PackageBuildData>("Assets/Package Build Data.asset");
            return;
        }

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
        packageBuildData.packageName = EditorGUILayout.TextField(packageBuildData.packageName);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Popup("Package build for", 0, new string[] { "Coba Platinum Patcher" });
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal("box");
        GUILayout.Label("PACKAGE MANIFEST", EditorStyles.largeLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical("box");
        packageBuildData.packageManifestAsset = (PackageManifestAsset)EditorGUILayout.ObjectField("Package Manifest Asset", packageBuildData.packageManifestAsset, typeof(PackageManifestAsset), false);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal("box");
        GUILayout.Label("BUILD VERSION", EditorStyles.largeLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical("box");

        int autoIncrement = 0;
        if (packageBuildData.autoIncrement)
            autoIncrement = 0;
        else
            autoIncrement = 1;

        autoIncrement = EditorGUILayout.Popup("Build Incrementing", autoIncrement, new string[] { "Automatic", "Manual" });

        if (autoIncrement == 0)
            packageBuildData.autoIncrement = true;
        else
            packageBuildData.autoIncrement = false;

        CP_BuildVersionProcessor.autoIncrement = packageBuildData.autoIncrement;

        if (!packageBuildData.autoIncrement)
        {
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("The current build version will not automatically update as long as 'Build Incrementing' is set to 'Manual'! " +
                "Set 'Build Incrementing' to 'Automatic' if you would like the version to dynamically update when you build the project!", MessageType.Warning);
        }

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Current Build Version - Major.Minor.Patch");
        EditorGUILayout.BeginHorizontal();
        if (!packageBuildData.autoIncrement)
        {
            packageBuildData.currentVersion[0] = EditorGUILayout.IntField(packageBuildData.currentVersion[0]);
            packageBuildData.currentVersion[1] = EditorGUILayout.IntField(packageBuildData.currentVersion[1]);
            packageBuildData.currentVersion[2] = EditorGUILayout.IntField(packageBuildData.currentVersion[2]);
            if (GUILayout.Button("Set Version"))
            {
                CP_BuildVersionProcessor.SetVersion(packageBuildData.currentVersion);
            }
        }
        else
        {
            packageBuildData.currentVersion = CP_BuildVersionProcessor.FindCurrentVersion();

            EditorGUILayout.IntField(packageBuildData.currentVersion[0]);
            EditorGUILayout.IntField(packageBuildData.currentVersion[1]);
            EditorGUILayout.IntField(packageBuildData.currentVersion[2]);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (packageBuildData.autoIncrement)
            packageBuildData.versionBuildLevel = (VersionBuildType)EditorGUILayout.EnumPopup("Version Build Level", packageBuildData.versionBuildLevel);

        CP_BuildVersionProcessor.buildType = packageBuildData.versionBuildLevel;

        packageBuildData.versionDevStage = (VersionDevStage)EditorGUILayout.EnumPopup("Version Build Stage", packageBuildData.versionDevStage);
        CP_BuildVersionProcessor.devStage = packageBuildData.versionDevStage;

        packageBuildData.isDevBuild = EditorGUILayout.Toggle("Version is DEV build", packageBuildData.isDevBuild);
        CP_BuildVersionProcessor.devBuild = packageBuildData.isDevBuild;

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal("box");
        GUILayout.Label("BUILD DETAILS", EditorStyles.largeLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Project Build Location");
        packageBuildData.buildPath = EditorGUILayout.TextField(packageBuildData.buildPath);
        if (GUILayout.Button("Browse"))
        {
            packageBuildData.buildPath = EditorUtility.OpenFolderPanel("Select Build Folder", "", "");
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Create Package Location");
        packageBuildData.packagePath = EditorGUILayout.TextField(packageBuildData.packagePath);
        if (GUILayout.Button("Browse"))
        {
            packageBuildData.packagePath = EditorUtility.OpenFolderPanel("Select Package Folder", "", "");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal("box");
        GUILayout.Label("PACKAGE OPTIONS", EditorStyles.largeLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Package Existing Build"))
        {
            PackageBuild();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        EditorUtility.SetDirty(packageBuildData);
    }

    private void PackageBuild()
    {
        if (VerifySettings())
        {
            EditorUtility.DisplayProgressBar("Packaging Build", "initializing", 0);

            string exportPath = (packageBuildData.packagePath + "/" + string.Format("{0}.{1}.{2}", packageBuildData.currentVersion[0], packageBuildData.currentVersion[1], packageBuildData.currentVersion[2]));
            string fullPackagePath = (exportPath + "/" + packageBuildData.packageName + "/" + packageBuildData.packageName);

            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(packageBuildData.buildPath);

                if (directoryInfo.GetDirectories().Length <= 0 || directoryInfo.GetFiles().Length <= 0)
                {
                    EditorUtility.DisplayDialog("Failed to Package Build!", "There are not any files within the build folder! Maybe you need to build the project?", "Ok");
                    EditorUtility.ClearProgressBar();
                    return;
                }
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayProgressBar("Packaging Build", "Creating package directory: " + fullPackagePath, 0.25f);
                // Determine whether the directory exists.
                if (Directory.Exists(fullPackagePath))
                {
                    Debug.Log("That path exists already.");

                    // Delete the directory.
                    Directory.Delete(fullPackagePath, true);
                    Debug.Log(fullPackagePath + " was deleted successfully.");
                }

                if (Directory.Exists(exportPath))
                {
                    Debug.Log("That path exists already.");

                    // Delete the directory.
                    Directory.Delete(exportPath, true);
                    Debug.Log(exportPath + " was deleted successfully.");
                }

                // Try to create the directory.
                DirectoryInfo di = Directory.CreateDirectory(fullPackagePath);
                Debug.Log(string.Format(di.FullName + " was created successfully at {0}.", Directory.GetCreationTime(fullPackagePath).ToString()));
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayProgressBar("Packaging Build", "Creating " + exportPath + "/Version.txt", 0.5f);

                // Create the file, or overwrite if the file exists.
                File.WriteAllText(exportPath + "/Version.txt", PlayerSettings.bundleVersion.ToString());
                Debug.Log(string.Format(exportPath + "/Version.txt" + " was created successfully at {0}.", File.GetCreationTime(exportPath + "/Version.txt").ToString()));
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayProgressBar("Packaging Build", "Creating " + exportPath + "/Manifest.json", 0.75f);

                // Create the file, or overwrite if the file exists.
                File.WriteAllLines(exportPath + "/Manifest.json", CreateJsonContent());
                Debug.Log(string.Format(exportPath + "/Manifest.json" + " was created successfully at {0}.", File.GetCreationTime(exportPath + "/Manifest.json").ToString()));

                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayProgressBar("Packaging Build", "Compressing package files", 0.9f);

                foreach (DirectoryInfo dir in directoryInfo.GetDirectories())
                {
                    Directory.CreateDirectory(fullPackagePath + "/" + dir.Name);
                    Directory.Move(dir.FullName, fullPackagePath + "/" + dir.Name + "/" + dir.Name);
                    ZipFile.CreateFromDirectory(fullPackagePath + "/" + dir.Name, fullPackagePath + "/" + dir.Name + ".zip");
                    Directory.Delete(fullPackagePath + "/" + dir.Name, true);
                }

                foreach (FileInfo file in directoryInfo.GetFiles())
                {
                    File.Move(file.FullName, fullPackagePath + "/" + file.Name);
                }

                ZipFile.CreateFromDirectory(exportPath + "/" + packageBuildData.packageName, exportPath + "/" + packageBuildData.packageName + ".zip");
                Directory.Delete(exportPath + "/" + packageBuildData.packageName, true);
            }
            catch (Exception e)
            {
                Debug.Log(string.Format("The process failed: {0}", e.ToString()));
                EditorUtility.DisplayDialog("Package build failed!", string.Format("The process failed: {0}", e.ToString()), "Ok");
                EditorUtility.ClearProgressBar();
                return;
            }

            EditorUtility.DisplayDialog("Build Packaged!", string.Format("Build successfuly packaged! \nPackage Name: {0} \nVersion: {1} \nCreated at: {2}", packageBuildData.packageName, PlayerSettings.bundleVersion, exportPath), "Ok");

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayProgressBar("Packaging Build", "Done", 1f);

            System.Diagnostics.Process.Start(exportPath);

            EditorUtility.ClearProgressBar();
        }
    }

    public List<string> CreateJsonContent()
    {
        List<string> jsonLines = new List<string>();

        jsonLines.Add("{");
        jsonLines.Add(string.Format("\"alternateDirectory\":\"{0}\",", packageBuildData.packageName));
        jsonLines.Add("\"filesToCopy\":[");

        for (int i = 0; i < packageBuildData.packageManifestAsset.manifestFiles.Length - 1; i++)
        {
            jsonLines.Add("   {\"fileName\":\"" + packageBuildData.packageManifestAsset.manifestFiles[i].fileName + "\", \"fromDir\":\"" 
                + packageBuildData.packageManifestAsset.manifestFiles[i].originDirectory + "\", \"toDir\":\"" + packageBuildData.packageManifestAsset.manifestFiles[i].targetDirectory 
                + "\", \"zipFile\":\"" + packageBuildData.packageManifestAsset.manifestFiles[i].isCompressed + "\"},");
        }
        jsonLines.Add("   {\"fileName\":\"" + packageBuildData.packageManifestAsset.manifestFiles[packageBuildData.packageManifestAsset.manifestFiles.Length - 1].fileName + "\", \"fromDir\":\""
                + packageBuildData.packageManifestAsset.manifestFiles[packageBuildData.packageManifestAsset.manifestFiles.Length - 1].originDirectory + "\", \"toDir\":\"" 
                + packageBuildData.packageManifestAsset.manifestFiles[packageBuildData.packageManifestAsset.manifestFiles.Length - 1].targetDirectory
                + "\", \"zipFile\":\"" + packageBuildData.packageManifestAsset.manifestFiles[packageBuildData.packageManifestAsset.manifestFiles.Length - 1].isCompressed + "\"}]");

        jsonLines.Add("}");

        return jsonLines;
    }

    private bool VerifySettings()
    {
        if (packageBuildData.buildPath.Equals(""))
        {
            EditorUtility.DisplayDialog("Failed to Package Build!", "The build location of the project has not been selected! Please select a build location and try again!", "Ok");
            return false;
        }

        if (packageBuildData.packagePath.Equals(""))
        {
            EditorUtility.DisplayDialog("Failed to Package Build!", "The package location of the project has not been selected! Please select a package location and try again!", "Ok");
            return false;
        }

        if (packageBuildData.packageName.Equals(""))
        {
            EditorUtility.DisplayDialog("Failed to Package Build!", "The package name of the project has not been selected! Please select a package name and try again!", "Ok");
            return false;
        }

        if (packageBuildData.packageManifestAsset == null)
        {
            EditorUtility.DisplayDialog("Failed to Package Build!", "The package manifest asset of the project has not been selected! Please select a package manifest asset and try again!", "Ok");
            return false;
        }

        return true;
    }
}
