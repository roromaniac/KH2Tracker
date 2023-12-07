using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;

public class FileProperties
{
    public string checktype { get; set; }
    public string included { get; set; }
}

public static class DirectoryToJson

{

    public static void Main(string path)
    {
        Convert(path);
    }

    public static void Convert(string path)
    {
        var filesTree = new Dictionary<string, object>();

        foreach (var dirPath in Directory.GetDirectories(path, "*", SearchOption.AllDirectories))
        {
            var subDict = filesTree;
            var parts = dirPath.Substring(path.Length).TrimStart('\\').Split('\\');

            foreach (var part in parts)
            {
                if (!subDict.ContainsKey(part))
                {
                    subDict[part] = new Dictionary<string, object>();
                }

                subDict = subDict[part] as Dictionary<string, object>;
            }

            foreach (var filePath in Directory.GetFiles(dirPath))
            {
                var fileWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                subDict[fileWithoutExtension] = new FileProperties { checktype = "unknown", included = "" };
            }
        }

        var json = JsonSerializer.Serialize(filesTree, new JsonSerializerOptions { WriteIndented = true });

        File.WriteAllText("output.json", json);
    }
}