using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;

public class BuildPostProcessor
{
    [PostProcessBuild]
    public static void OnPostBuild(BuildTarget target, string pathToBuiltProject)
    {
        string buildFolder = Path.GetDirectoryName(pathToBuiltProject);
        string rootConfig = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string sourceConfig = Path.Combine(rootConfig, "config.json");
        string targetConfig = Path.Combine(buildFolder, "config.json");

        if (File.Exists(sourceConfig))
        {
            File.Copy(sourceConfig, targetConfig, true);
            UnityEngine.Debug.Log("Config copied to build folder.");
        }
    }
}