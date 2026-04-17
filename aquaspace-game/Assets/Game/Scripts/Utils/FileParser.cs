using System;
using System.IO;

public static class FileParser
{
    public static ParsedFileData Parse(string fileName)
    {
        var name = Path.GetFileNameWithoutExtension(fileName);
        var parts = name.Split('_');

        if(parts.Length != 3) throw new Exception("Invalid naming");

        return new ParsedFileData
        {
            category = parts[0],
            type = parts[1],
            timestamp = parts[2]
        };
    }
}


public struct ParsedFileData
{
    public string category;
    public string type;
    public string timestamp;
}

