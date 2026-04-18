using UnityEngine;
using System.IO;

/// <summary>
/// Responsible for initializing and managing folder structure for the file watcher system.
/// </summary>
public class FolderSetup
{
    public string FolderPath { get; private set; }
    public string FishFolderPath { get; private set; }
    public string TrashFolderPath { get; private set; }

    public FolderSetup(string basePath = "")
    {
        InitializeFolders(basePath);
    }

    private void InitializeFolders(string basePath)
    {
        FolderPath = ResolveFolderPath(basePath);
        Directory.CreateDirectory(FolderPath);

        FishFolderPath = Path.Combine(FolderPath, "fish");
        TrashFolderPath = Path.Combine(FolderPath, "trash");

        Directory.CreateDirectory(FishFolderPath);
        Directory.CreateDirectory(TrashFolderPath);

        Debug.Log($"[FolderSetup] Fish folder: {FishFolderPath}");
        Debug.Log($"[FolderSetup] Trash folder: {TrashFolderPath}");
    }

    private string ResolveFolderPath(string basePath)
    {
        if (string.IsNullOrWhiteSpace(basePath))
        {
            return Application.persistentDataPath;
        }

        return Path.Combine(Application.persistentDataPath, basePath);
    }

    public bool IsValid()
    {
        return Directory.Exists(FolderPath);
    }
}
