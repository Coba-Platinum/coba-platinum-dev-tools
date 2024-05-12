using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System;
using System.Text.RegularExpressions;

namespace CobaPlatinum.Utilities.Versioning
{
    public class CobaPlatinum_BuildVersionProcessor : IPreprocessBuildWithReport
    {
        

        public int callbackOrder => 0;

        public static bool autoIncrement = true;
        public static bool devBuild = true;
        public static VersionBuildType buildType = VersionBuildType.PATCH;
        public static VersionDevStage devStage = VersionDevStage.ALPHA;

        public void OnPreprocessBuild(BuildReport _report)
        {
            int[] currentVersion = FindCurrentVersion();

            if(autoIncrement)
                UpdateVersion(currentVersion);
        }

        public static int[] FindCurrentVersion()
        {
            //Split to find string of current version
            string insideParentheses = Regex.Match(PlayerSettings.bundleVersion, @"\(([^)]*)\)").Groups[1].Value;
            string[] currentVersionString = insideParentheses.Split('.');
            int[] currentVersion = new int[] { 0, 0, 0 };

            if(currentVersionString.Length == currentVersion.Length)
            {
                for (int i = 0; i < currentVersionString.Length; i++)
                {
                    currentVersion[i] = int.Parse(currentVersionString[i]);
                }
            }
                
            return currentVersion;
        }

        public static void UpdateVersion(int[] version)
        {
            int[] newVersion = version;
            switch(buildType)
            {
                case VersionBuildType.PATCH:
                    newVersion[2]++;
                    break;
                case VersionBuildType.MINOR:
                    newVersion[1]++;
                    newVersion[2] = 0;
                    break;
                case VersionBuildType.MAJOR:
                    newVersion[0]++;
                    newVersion[1] = 0;
                    newVersion[2] = 0;
                    break;
                default:
                    newVersion[2]++;
                    break;
            }

            string date = DateTime.Now.ToString("d");
            if(devBuild)
                PlayerSettings.bundleVersion = string.Format("DEV {4} Version ({0}.{1}.{2}) - {3}", newVersion[0], newVersion[1], newVersion[2], date, devStage);
            else
                PlayerSettings.bundleVersion = string.Format("{4} Version ({0}.{1}.{2}) - {3}", newVersion[0], newVersion[1], newVersion[2], date, devStage);
        }

        public static void SetVersion(int[] version)
        {
            int[] newVersion = version;

            string date = DateTime.Now.ToString("d");
            if (devBuild)
                PlayerSettings.bundleVersion = string.Format("DEV {4} Version ({0}.{1}.{2}) - {3}", newVersion[0], newVersion[1], newVersion[2], date, devStage);
            else
                PlayerSettings.bundleVersion = string.Format("{4} Version ({0}.{1}.{2}) - {3}", newVersion[0], newVersion[1], newVersion[2], date, devStage);

            EditorUtility.DisplayDialog("Build Version Set!", "The build version of the project has been set to: \n" + PlayerSettings.bundleVersion, "Ok");
        }
    }

    public enum VersionBuildType
    {
        PATCH,
        MINOR,
        MAJOR
    }
    public enum VersionDevStage
    {
        ALPHA,
        BETA,
        RELEASE
    }
}
