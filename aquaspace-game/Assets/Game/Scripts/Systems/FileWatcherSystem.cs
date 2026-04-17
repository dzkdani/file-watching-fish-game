using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class FileWatcherSystem : MonoBehaviour
{
    public float scanInterval = 1f;
    public string path = "";
    private string folderPath;
    private HashSet<string> processedFiles = new();

    void Start()
    {
        folderPath = Path.Combine(Application.streamingAssetsPath, path);
        StartCoroutine(ScanRoutine());
    }

    IEnumerator ScanRoutine()
    {
        while(true)
        {
            ScanFolder();
            yield return new WaitForSeconds(scanInterval);
        }
    }

    void ScanFolder()
    {
        var files = Directory.GetFiles(folderPath, "*.png");

        foreach(var file in files)
        {
            if(processedFiles.Contains(file)) continue;

            processedFiles.Add(file);
            SpawnSystem.Instance.ProcessFile(file);
        }
    }
}