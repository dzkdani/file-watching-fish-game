using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Handles scanning directories for files matching specified categories.
/// </summary>
public class FileScanner
{
    private readonly string _fishFolderPath;
    private readonly string _trashFolderPath;
    private readonly bool _scanRecursively;

    public FileScanner(string fishFolderPath, string trashFolderPath, bool scanRecursively = true)
    {
        _fishFolderPath = fishFolderPath;
        _trashFolderPath = trashFolderPath;
        _scanRecursively = scanRecursively;
    }

    public List<string> GetCategoryFiles()
    {
        SearchOption searchOption = _scanRecursively ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        List<string> files = new List<string>();

        if (Directory.Exists(_fishFolderPath))
        {
            files.AddRange(Directory.GetFiles(_fishFolderPath, "*.png", searchOption));
        }

        if (Directory.Exists(_trashFolderPath))
        {
            files.AddRange(Directory.GetFiles(_trashFolderPath, "*.png", searchOption));
        }

        return files;
    }

    public void ShuffleFiles(List<string> files)
    {
        for (int i = files.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            if (randomIndex == i)
            {
                continue;
            }

            string temp = files[i];
            files[i] = files[randomIndex];
            files[randomIndex] = temp;
        }
    }

    public bool TryGetFileModificationTime(string filePath, out DateTime lastWriteTime)
    {
        try
        {
            lastWriteTime = File.GetLastWriteTimeUtc(filePath);
            return true;
        }
        catch (IOException)
        {
            lastWriteTime = DateTime.MinValue;
            return false;
        }
    }
}
