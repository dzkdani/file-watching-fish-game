using System;
using System.Collections.Generic;

/// <summary>
/// Tracks file modification times to detect changes and removals.
/// </summary>
public class FileStateTracker
{
    public event Action<string> FileChanged;
    public event Action<string> FileRemoved;

    private readonly Dictionary<string, DateTime> _trackedFileWriteTimes = new(StringComparer.OrdinalIgnoreCase);

    public void UpdateFileState(string filePath, DateTime lastWriteTime)
    {
        if (_trackedFileWriteTimes.TryGetValue(filePath, out DateTime knownWriteTime))
        {
            if (knownWriteTime != lastWriteTime)
            {
                _trackedFileWriteTimes[filePath] = lastWriteTime;
                FileChanged?.Invoke(filePath);
            }
        }
        else
        {
            _trackedFileWriteTimes[filePath] = lastWriteTime;
            FileChanged?.Invoke(filePath);
        }
    }

    public void RemoveTrackedFile(string filePath)
    {
        if (_trackedFileWriteTimes.Remove(filePath))
        {
            FileRemoved?.Invoke(filePath);
        }
    }

    public void CleanupRemovedFiles(HashSet<string> currentFiles)
    {
        var removedFiles = new List<string>();
        
        foreach (var trackedFile in _trackedFileWriteTimes.Keys)
        {
            if (!currentFiles.Contains(trackedFile))
            {
                removedFiles.Add(trackedFile);
            }
        }

        foreach (var removedFile in removedFiles)
        {
            RemoveTrackedFile(removedFile);
        }
    }

    public bool HasTrackedFiles()
    {
        return _trackedFileWriteTimes.Count > 0;
    }

    public void Clear()
    {
        _trackedFileWriteTimes.Clear();
    }
}
