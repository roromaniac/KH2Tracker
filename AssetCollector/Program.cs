using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;

public class FileProperties
{
    public string checktype { get; set; }
    public bool included { get; set; }
}

public static class DirectoryToJson

{

    static void Main(string[] args)
    {
        string path = args.Length > 0 ? args[0] : throw new ArgumentException("Directory path is required as an argument");
        DirectoryToJson.Convert(path);
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
                subDict[fileWithoutExtension] = new FileProperties { checktype = "unknown", included = false };
            }
        }

        var json = JsonSerializer.Serialize(filesTree, new JsonSerializerOptions { WriteIndented = true });

        File.WriteAllText("output.json", json);
    }
}