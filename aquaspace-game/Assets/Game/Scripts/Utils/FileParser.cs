using System;
using System.IO;

public static class FileParser
{
    public static ParsedFileData Parse(string filePath)
    {
        string categoryFromFolder = ResolveCategoryFromPath(filePath);
        string name = Path.GetFileNameWithoutExtension(filePath);
        string[] parts = name.Split('_', StringSplitOptions.RemoveEmptyEntries);

        if (!string.IsNullOrEmpty(categoryFromFolder))
        {
            string timestamp = GetSafeTimestamp(filePath);
            string type = name;

            if (parts.Length >= 2)
            {
                timestamp = parts[parts.Length - 1];
                type = string.Join("_", parts, 0, parts.Length - 1);
            }

            return new ParsedFileData
            {
                category = categoryFromFolder,
                type = type,
                timestamp = timestamp
            };
        }

        if (parts.Length != 3)
        {
            throw new Exception("Invalid naming. Use fish/trash folder mods or category_type_timestamp filename.");
        }

        return new ParsedFileData
        {
            category = parts[0],
            type = parts[1],
            timestamp = parts[2]
        };
    }

    private static string ResolveCategoryFromPath(string filePath)
    {
        DirectoryInfo directory = new FileInfo(filePath).Directory;
        while (directory != null)
        {
            if (directory.Name.Equals("fish", StringComparison.OrdinalIgnoreCase))
            {
                return "fish";
            }

            if (directory.Name.Equals("trash", StringComparison.OrdinalIgnoreCase))
            {
                return "trash";
            }

            directory = directory.Parent;
        }

        return null;
    }

    private static string GetSafeTimestamp(string filePath)
    {
        try
        {
            return File.GetLastWriteTimeUtc(filePath).Ticks.ToString();
        }
        catch
        {
            return DateTime.UtcNow.Ticks.ToString();
        }
    }
}

public struct ParsedFileData
{
    public string category;
    public string type;
    public string timestamp;
}

