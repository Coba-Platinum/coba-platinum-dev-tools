using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CobaPlatinum.Utilities.Versioning;

[CreateAssetMenu(fileName = "Package Build Data", menuName = "Build Packager/Package Build Data")]
public class PackageBuildData : ScriptableObject
{
    public string packageName;
    public PackageManifestAsset packageManifestAsset;
    public bool autoIncrement;
    public VersionBuildType versionBuildLevel;
    public VersionDevStage versionDevStage;
    public bool isDevBuild;
    public string buildPath;
    public string packagePath;
    public int[] currentVersion = new int[] { 0, 0, 0 };
}
