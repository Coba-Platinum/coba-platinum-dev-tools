using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Manifect Asset", menuName = "Build Packager/Package Manifest/New Manifest Asset")]
public class PackageManifestAsset : ScriptableObject
{
    [HideInInspector] public string packageName;
    [HideInInspector] public string packageVersion;
    [HideInInspector] public string alternateDirectory;

    public ManifestFileEntry[] manifestFiles;
}

[System.Serializable]
public struct ManifestFileEntry
{
    public string fileName;
    public string originDirectory;
    public string targetDirectory;
    public bool isCompressed;
}
