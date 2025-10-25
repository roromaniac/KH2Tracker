using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace KhTracker
{
    public static class CGMGenerator
    {
        [STAThread]
        public static void Main()
        {
            string inputPath = "C:\\Users\\Owner\\Documents\\RandoProjects\\KH2Tracker\\KhTracker\\ObjectiveDictionary.xaml";
            string extraPath = "C:\\Users\\Owner\\Documents\\RandoProjects\\KH2Tracker\\KhTracker\\OneHourDictionary.xaml"; // secondary file with already formatted grids
            string baseDir = "C:\\Users\\Owner\\Documents\\RandoProjects\\KH2Tracker\\KhTracker";

            if (!File.Exists(inputPath))
            {
                Console.WriteLine("❌ Could not find " + inputPath);
                return;
            }

            if (!File.Exists(extraPath))
            {
                Console.WriteLine("⚠️ Warning: could not find ExtraAssets.xaml — skipping merge.");
            }

            string input = File.ReadAllText(inputPath);
            string extra = File.Exists(extraPath) ? File.ReadAllText(extraPath) : string.Empty;

            // Pattern to match <Grid> or <Image> entries
            Regex elementPattern = new Regex(
                @"<(Image|Grid)\b(?<content>[\s\S]*?)</\1>|<(Image)\b(?<single>[^>]*)/>",
                RegexOptions.Singleline);

            // Writers per prefix
            var writers = new Dictionary<string, StreamWriter>
            {
                { "Min", new StreamWriter(Path.Combine(baseDir, "CGM_Min_Output.xaml")) },
                { "Old", new StreamWriter(Path.Combine(baseDir, "CGM_Old_Output.xaml")) },
                { "Cus", new StreamWriter(Path.Combine(baseDir, "CGM_Cus_Output.xaml")) }
            };

            // Write headers
            foreach (var w in writers.Values)
            {
                w.WriteLine("<ResourceDictionary xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"");
                w.WriteLine("                    xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"");
                w.WriteLine("                    xmlns:local=\"clr-namespace:KhTracker\">");
                w.WriteLine();
            }

            // Track which keys we’ve already added
            var addedKeys = new HashSet<string>();

            // Process main input (ObjectiveDictionary)
            ProcessSource(input, writers, addedKeys);

            // Merge extras (only keys not already written)
            if (!string.IsNullOrEmpty(extra))
                MergeExtras(extra, writers, addedKeys);

            // Write footers
            foreach (var w in writers.Values)
            {
                w.WriteLine("</ResourceDictionary>");
                w.Dispose();
            }

            Console.WriteLine("✅ Done! Merged outputs written to:");
            Console.WriteLine("   CGM_Min_Output.xaml, CGM_Old_Output.xaml, CGM_Cus_Output.xaml");
        }

        private static void ProcessSource(string text, Dictionary<string, StreamWriter> writers, HashSet<string> addedKeys)
        {
            Regex elementPattern = new Regex(
                @"<(Image|Grid)\b(?<content>[\s\S]*?)</\1>|<(Image)\b(?<single>[^>]*)/>",
                RegexOptions.Singleline);

            foreach (Match match in elementPattern.Matches(text))
            {
                string content = match.Groups["content"].Success ? match.Groups["content"].Value : match.Groups["single"].Value;
                if (string.IsNullOrWhiteSpace(content)) continue;

                string key = GetAttr(content, "x:Key");
                string tooltip = GetAttr(content, "ToolTip");
                string source = GetAttr(content, "Source");

                if (string.IsNullOrEmpty(source))
                {
                    Match innerImage = Regex.Match(content, @"<Image[^>]*Source\s*=\s*""([^""]+)""");
                    if (innerImage.Success)
                        source = innerImage.Groups[1].Value;
                }

                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(tooltip) || string.IsNullOrEmpty(source))
                    continue;

                string prefix = null;
                if (key.StartsWith("Obj_Min")) prefix = "Min";
                else if (key.StartsWith("Obj_Old")) prefix = "Old";
                else if (key.StartsWith("Obj_Cus")) prefix = "Cus";
                else continue;

                string newKey = key.Replace("Obj_", "CGM_");
                if (addedKeys.Contains(newKey)) continue;
                addedKeys.Add(newKey);

                var writer = writers[prefix];
                WriteGridBlock(writer, newKey, tooltip, source);
            }
        }

        private static void MergeExtras(string extra, Dictionary<string, StreamWriter> writers, HashSet<string> addedKeys)
        {
            Regex gridPattern = new Regex(@"<Grid[^>]*x:Key\s*=\s*""(?<key>[^""]+)""[\s\S]*?</Grid>", RegexOptions.Singleline);
            foreach (Match m in gridPattern.Matches(extra))
            {
                string key = m.Groups["key"].Value;
                if (addedKeys.Contains(key)) continue; // already in main set

                string prefix = null;
                if (key.Contains("_Min")) prefix = "Min";
                else if (key.Contains("_Old")) prefix = "Old";
                else if (key.Contains("_Cus")) prefix = "Cus";
                else continue;

                writers[prefix].WriteLine(m.Value.Trim());
                writers[prefix].WriteLine();
                addedKeys.Add(key);
            }

            Console.WriteLine("🧩 ExtraAssets merged successfully!");
        }

        private static void WriteGridBlock(StreamWriter writer, string key, string tooltip, string source)
        {
            writer.WriteLine(string.Format("    <Grid x:Key=\"{0}\" x:Shared=\"false\" ToolTip=\"{1}\">", key, tooltip));
            writer.WriteLine("        <Grid.RowDefinitions>");
            writer.WriteLine("            <RowDefinition/>");
            writer.WriteLine("            <RowDefinition Height=\"0.3*\"/>");
            writer.WriteLine("        </Grid.RowDefinitions>");
            writer.WriteLine(string.Format("        <Image Grid.Row=\"0\" Source=\"{0}\"/>", source));
            writer.WriteLine("        <Rectangle Fill=\"#D81E1E1E\" Panel.ZIndex=\"-1\" Grid.Row=\"1\" VerticalAlignment=\"Stretch\" HorizontalAlignment=\"Stretch\" Stretch=\"UniformToFill\"/>");
            writer.WriteLine("        <Viewbox Grid.Row=\"1\" Margin=\"10,0,10,0\">");
            writer.WriteLine("            <local:OutlinedTextBlock FontFamily=\"pack://application:,,,/Fonts/#KHMenu\" FontSize=\"15\" StrokeThickness=\"3\" Text=\"10 Points\" Fill=\"{StaticResource Banner_Gold}\"/>");
            writer.WriteLine("        </Viewbox>");
            writer.WriteLine("    </Grid>");
            writer.WriteLine();
        }

        private static string GetAttr(string text, string attr)
        {
            var m = Regex.Match(text, attr + @"\s*=\s*""([^""]+)""");
            return m.Success ? m.Groups[1].Value : null;
        }
    }
}
