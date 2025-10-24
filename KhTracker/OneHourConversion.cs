using System;
using System.IO;
using System.Text.RegularExpressions;

namespace KhTracker
{
    public static class CGMGenerator
    {
        [STAThread]
        public static void Main()
        {
            string inputPath = "C:\\Users\\Owner\\Documents\\RandoProjects\\KH2Tracker\\KhTracker\\ObjectiveDictionary.xaml";
            string baseDir = "C:\\Users\\Owner\\Documents\\RandoProjects\\KH2Tracker\\KhTracker";

            if (!File.Exists(inputPath))
            {
                Console.WriteLine("❌ Could not find " + inputPath);
                return;
            }

            string input = File.ReadAllText(inputPath);

            // Match the whole element so we can look inside it
            Regex elementPattern = new Regex(
                @"<(Image|Grid)\b(?<content>[\s\S]*?)</\1>|<(Image)\b(?<single>[^>]*)/>",
                RegexOptions.Singleline);

            var writers = new (string Prefix, StreamWriter Writer)[]
            {
                ("Min", new StreamWriter(Path.Combine(baseDir, "CGM_Min_Output.xaml"))),
                ("Old", new StreamWriter(Path.Combine(baseDir, "CGM_Old_Output.xaml"))),
                ("Cus", new StreamWriter(Path.Combine(baseDir, "CGM_Cus_Output.xaml")))
            };

            foreach (var w in writers)
            {
                w.Writer.WriteLine("<ResourceDictionary xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"");
                w.Writer.WriteLine("                    xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"");
                w.Writer.WriteLine("                    xmlns:local=\"clr-namespace:KhTracker\">");
                w.Writer.WriteLine();
            }

            foreach (Match match in elementPattern.Matches(input))
            {
                string content = match.Groups["content"].Success ? match.Groups["content"].Value : match.Groups["single"].Value;
                if (string.IsNullOrWhiteSpace(content)) continue;

                // Extract key, tooltip, and first source
                string key = GetAttr(content, "x:Key");
                string tooltip = GetAttr(content, "ToolTip");
                string source = GetAttr(content, "Source");

                // If no Source on the parent, look for the first <Image Source="..."> inside
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
                var writer = Array.Find(writers, w => w.Prefix == prefix).Writer;

                writer.WriteLine(string.Format("    <Grid x:Key=\"{0}\" x:Shared=\"false\" ToolTip=\"{1}\">", newKey, tooltip));
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

            foreach (var w in writers)
            {
                w.Writer.WriteLine("</ResourceDictionary>");
                w.Writer.Dispose();
            }

            Console.WriteLine("✅ Done! Wrote CGM_Min_Output.xaml, CGM_Old_Output.xaml, CGM_Cus_Output.xaml");
        }

        private static string GetAttr(string text, string attr)
        {
            var m = Regex.Match(text, attr + @"\s*=\s*""([^""]+)""");
            return m.Success ? m.Groups[1].Value : null;
        }
    }
}
