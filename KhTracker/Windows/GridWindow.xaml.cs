﻿using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.IO;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Diagnostics.Eventing.Reader;
using System.Data.Common;

namespace KhTracker
{
    /// <summary>
    /// Interaction logic for GridWindow.xaml
    /// </summary>
    /// 

    public interface IColorableWindow
    {
        void HandleClosing(ColorPickerWindow sender);
    }

    public partial class GridWindow : Window, IColorableWindow
    {
        public bool canClose = false;
        readonly Data data;
        public GridOptionsWindow gridOptionsWindow;
        public ColorPickerWindow colorPickerWindow;

        public int numRows;
        public int numColumns;
        public string seedName;
        public bool bingoLogic;
        public bool battleshipLogic;
        public bool bunterLogic;
        public List<Dictionary<string, object>> bunterBosses;
        public bool fogOfWar;
        public Dictionary<string, int> fogOfWarSpan = new Dictionary<string, int>()
        {
            { "W", 1 },
            { "E", 1 },
            { "N", 1 },
            { "S", 1 },
            { "NW", 0 },
            { "NE", 0 },
            { "SW", 0 },
            { "SE", 0 },
        };

        public Grid grid;
        public ToggleButton[,] buttons;
        public Color[,] originalColors;
        public bool[,] bingoStatus;
        public bool[,] annotationStatus;
        public Dictionary<string, bool> gridSettings = new Dictionary<string, bool>();
        public Dictionary<string, Color> currentColors = new Dictionary<string, Color>();
        public Dictionary<string, ContentControl> bossHintContentControls = new Dictionary<string, ContentControl>();
        public Dictionary<string, Border> bossHintBorders = new Dictionary<string, Border>();
        public List<string> assets;
        public ToggleButton lastTrackedButton;

        // battleship specific
        private Random random;
        public int seed;
        public int[,] placedShips;
        private List<Tuple<int, int>> possibleShipHeads;
        public List<int> shipSizes = new List<int>() { 1, 1 }; // Assuming you have this set somewhere
        private int currentShipId = 1; // Start with 1 and increment for each ship
        public int minShipCount = 5;
        public int maxShipCount = 5;
        private List<int> sampledShipSizes = new List<int>();
        public bool[,] battleshipSunkStatus;
        public bool battleshipRandomCount = false;
        public bool coloredHints = false;
        public int coloredHintsDistance = 1;
        public List<Color> coloredHintsColors;

        public GridWindow(Data dataIn)
        {
            data = dataIn;

            InitializeComponent();
            InitOptions();

            try
            {
                gridSettings = JsonSerializer.Deserialize<Dictionary<string, bool>>(Properties.Settings.Default.GridSettings);
            }
            catch (JsonException)
            {
                MessageBox.Show("Grid settings did not deserialize correctly. Please reconfigure the settings.");
            }
            currentColors = GetColorSettings();

            numRows = Properties.Settings.Default.GridWindowRows;
            numColumns = Properties.Settings.Default.GridWindowColumns;

            bingoLogic = Properties.Settings.Default.GridBingoLogic;
            battleshipLogic = Properties.Settings.Default.GridBattleshipLogic;
            bunterLogic = Properties.Settings.Default.GridBunterLogic;
            battleshipRandomCount = Properties.Settings.Default.GridBattleshipRandomCount;
            minShipCount = Properties.Settings.Default.GridMinShipCount;
            maxShipCount = Properties.Settings.Default.GridMaxShipCount;
            coloredHints = Properties.Settings.Default.GridColoredHints;
            coloredHintsDistance = Properties.Settings.Default.GridColoredHintsDistance;
            // +1 is becuase you need coloredHintsDistance colors for the misses and 1 color for the hit.
            coloredHintsColors = GenerateGradient(currentColors["Battleship Hit Color"], currentColors["Battleship Miss Color"], coloredHintsDistance + 1);
            try
            {
                shipSizes = JsonSerializer.Deserialize<List<int>>(Properties.Settings.Default.GridShipSizes);
            }
            catch (JsonException)
            {
                MessageBox.Show("Ships file did not deserialize correctly.");
                shipSizes = new List<int> { 1, 1 };
            }
            fogOfWar = Properties.Settings.Default.GridFogOfWar;
            try
            {
                fogOfWarSpan = JsonSerializer.Deserialize<Dictionary<string, int>>(Properties.Settings.Default.GridFogOfWarSpan);
            }
            catch (JsonException)
            {
                MessageBox.Show("Fog of war dictionary had an error loading. Please try again.");
            }
            gridOptionsWindow = new GridOptionsWindow(this, data);
            colorPickerWindow = new ColorPickerWindow(this, currentColors);

            GenerateGrid(numRows, numColumns);
            //Item.UpdateTotal += new Item.TotalHandler(UpdateTotal);

            Top = Properties.Settings.Default.GridWindowY;
            Left = Properties.Settings.Default.GridWindowX;

            Width = Properties.Settings.Default.GridWindowWidth;
            Height = Properties.Settings.Default.GridWindowHeight;
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.GridWindowY = RestoreBounds.Top;
            Properties.Settings.Default.GridWindowX = RestoreBounds.Left;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Properties.Settings.Default.GridWindowWidth = RestoreBounds.Width;
            Properties.Settings.Default.GridWindowHeight = RestoreBounds.Height;
        }

        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            gridOptionsWindow.Hide();
            colorPickerWindow.Hide();
            if (!canClose)
            {
                e.Cancel = true;
            }
        }

        private void DownloadCardSetting(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON Files (*.json)|*.json",
                FileName = "settings.json"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                string jsonString = DownloadCardSetting();
                System.IO.File.WriteAllText(saveFileDialog.FileName, jsonString);
            }
        }

        public string DownloadCardSetting()
        {

            var combinedSettings = new
            {
                battleshipLogic,
                battleshipRandomCount,
                bunterLogic,
                bingoLogic,
                coloredHints,
                coloredHintsDistance,
                fogOfWar,
                fogOfWarSpan,
                gridSettings,
                maxShipCount,
                minShipCount,
                numColumns,
                numRows,
                seedName,
                shipSizes,
            };

            string jsonString = JsonSerializer.Serialize(combinedSettings);
            return jsonString;
        }

        private void UploadCardSetting(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                DefaultExt = ".json",
                Filter = "JSON Files (*.json)|*.json",
                Title = "Select Grid Settings File",
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string jsonString = System.IO.File.ReadAllText(openFileDialog.FileName);
                    UploadCardSetting(jsonString);
                }
                catch (JsonException)
                {
                    Console.WriteLine("Card setting file did not read correctly. Please try editing it and try again. If the issue persists, please report it to #tracker-discussion.");
                    return;
                }
            }
        }

        public void UploadCardSetting(string jsonString)
        {
            using (JsonDocument doc = JsonDocument.Parse(jsonString))
            {
                var root = doc.RootElement;

                battleshipLogic = root.TryGetProperty("battleshipLogic", out JsonElement battleshipLogicElement)
                    ? battleshipLogicElement.GetBoolean()
                    : Properties.Settings.Default.GridBattleshipLogic;

                battleshipRandomCount = root.TryGetProperty("battleshipRandomCount", out JsonElement battleshipRandomCountElement)
                    ? battleshipRandomCountElement.GetBoolean()
                    : Properties.Settings.Default.GridBattleshipRandomCount;

                bingoLogic = root.TryGetProperty("bingoLogic", out JsonElement bingoLogicElement)
                    ? bingoLogicElement.GetBoolean()
                    : Properties.Settings.Default.GridBingoLogic;

                bunterLogic = root.TryGetProperty("bunterLogic", out JsonElement bunterLogicElement)
                    ? bunterLogicElement.GetBoolean()
                    : Properties.Settings.Default.GridBunterLogic;

                coloredHints = root.TryGetProperty("coloredHints", out JsonElement coloredHintsElement)
                    ? coloredHintsElement.GetBoolean()
                    : Properties.Settings.Default.GridColoredHints;

                coloredHintsDistance = root.TryGetProperty("coloredHintsDistance", out JsonElement coloredHintsDistanceElement)
                    ? coloredHintsDistanceElement.GetInt32()
                    : Properties.Settings.Default.GridColoredHintsDistance;

                fogOfWar = root.TryGetProperty("fogOfWar", out JsonElement fogOfWarElement)
                    ? fogOfWarElement.GetBoolean()
                    : Properties.Settings.Default.GridFogOfWar;

                // Deserialize with a default value if key is missing
                fogOfWarSpan = root.TryGetProperty("fogOfWarSpan", out JsonElement fogOfWarSpanElement)
                    ? JsonSerializer.Deserialize<Dictionary<string, int>>(fogOfWarSpanElement.GetRawText())
                    : JsonSerializer.Deserialize<Dictionary<string, int>>(Properties.Settings.Default.GridFogOfWarSpan);

                gridSettings = root.TryGetProperty("gridSettings", out JsonElement gridSettingsElement)
                    ? JsonSerializer.Deserialize<Dictionary<string, bool>>(gridSettingsElement.GetRawText())
                    : JsonSerializer.Deserialize<Dictionary<string, bool>>(Properties.Settings.Default.GridSettings);

                maxShipCount = root.TryGetProperty("maxShipCount", out JsonElement maxShipCountLogicElement)
                    ? maxShipCountLogicElement.GetInt32()
                    : Properties.Settings.Default.GridMaxShipCount;

                minShipCount = root.TryGetProperty("minShipCount", out JsonElement minShipCountLogicElement)
                    ? minShipCountLogicElement.GetInt32()
                    : Properties.Settings.Default.GridMinShipCount;

                numColumns = root.TryGetProperty("numColumns", out JsonElement numColumnsElement)
                    ? numColumnsElement.GetInt32()
                    : Properties.Settings.Default.GridWindowColumns;

                numRows = root.TryGetProperty("numRows", out JsonElement numRowsElement)
                    ? numRowsElement.GetInt32()
                    : Properties.Settings.Default.GridWindowRows;

                shipSizes = root.TryGetProperty("shipSizes", out JsonElement shipSizesElement)
                    ? JsonSerializer.Deserialize<List<int>>(shipSizesElement.GetRawText())
                    : JsonSerializer.Deserialize<List<int>>(Properties.Settings.Default.GridShipSizes);
            }

            // ensure all of the grid settings keys are present
            var defaultSettingsJson = Properties.Settings.Default.Properties["GridSettings"].DefaultValue;
            var defaultSettings = JsonSerializer.Deserialize<Dictionary<string, bool>>((string)defaultSettingsJson);

            // find the keys that should be removed
            var keysToRemove = gridSettings.Keys.Where(key => !defaultSettings.ContainsKey(key)).ToList();

            // remove these keys
            foreach (var key in keysToRemove)
            {
                if (gridSettings.ContainsKey(key))
                    gridSettings.Remove(key);
            }

            // Add missing required keys with their default values
            foreach (var key in defaultSettings.Keys)
            {
                if (!gridSettings.ContainsKey(key))
                {
                    gridSettings.Add(key, defaultSettings[key]);
                }
            }

            // update number of reports
            int numReports = 0;
            for (int i = 1; i <= 13; i++)
            {
                if (gridSettings[$"Report{i}"])
                    numReports++;
            }

            // update number of unlocks
            List<string> unlockNames = (MainWindow.data.VisitLocks.Select(item => item.Name)).ToList();
            int numUnlocks = 0;
            foreach (string unlock in unlockNames)
            {
                if (gridSettings[unlock])
                    numUnlocks++;
            }

            // update number of chest locks
            List<string> worldChestLockNames = (MainWindow.data.ChestLocks.Select(item => item.Name)).ToList();
            int numChestLocks = 0;
            foreach (string chestLock in worldChestLockNames)
            {
                if (gridSettings[chestLock])
                    numChestLocks++;
            }

            if (SavePreviousGridSettingsOption.IsChecked)
            {
                Properties.Settings.Default.GridWindowRows = numRows;
                Properties.Settings.Default.GridWindowColumns = numColumns;
                Properties.Settings.Default.GridSettings = JsonSerializer.Serialize(gridSettings);
                Properties.Settings.Default.GridBingoLogic = bingoLogic;
                Properties.Settings.Default.GridBattleshipLogic = battleshipLogic;
                Properties.Settings.Default.GridShipSizes = JsonSerializer.Serialize(shipSizes);
                Properties.Settings.Default.GridBattleshipRandomCount = battleshipRandomCount;
                Properties.Settings.Default.GridFogOfWar = fogOfWar;
                Properties.Settings.Default.GridFogOfWarSpan = JsonSerializer.Serialize(fogOfWarSpan);
                Properties.Settings.Default.GridMaxShipCount = maxShipCount;
                Properties.Settings.Default.GridMinShipCount = minShipCount;
                Properties.Settings.Default.GridWindowNumReports = numReports;
                Properties.Settings.Default.GridWindowNumUnlocks = numUnlocks;
                Properties.Settings.Default.GridWindowNumChestLocks = numChestLocks;
            }
            grid.Children.Clear();
            GenerateGrid(numRows, numColumns, null, true, true);
            gridOptionsWindow.InitializeData(this, data);
            gridOptionsWindow.UpdateGridOptionsUI();
        }

        private void SetSeedname(object sender, RoutedEventArgs e)
        {
            var inputDialog = new SeedNamer();
            if (inputDialog.ShowDialog() == true)
            {
                seedName = inputDialog.InputText;
            }
            GenerateGrid(numRows, numColumns, seedName);
        }

        private void Grid_Options(object sender, RoutedEventArgs e)
        {
            gridOptionsWindow.Show();
        }

        private void getAssetPrefix()
        {
            string style = TelevoIconsOption.IsChecked ? "Grid_Min-" : "Grid_Old-";

            for (int i = 0; i < assets.Count; i++)
            {
                if (CustomGridIconsOption.IsChecked && MainWindow.CusGridImagesList.Contains("Grid_Cus-" + assets[i]))
                {
                    assets[i] = "Grid_Cus-" + assets[i];
                    continue;
                }

                assets[i] = style + assets[i];
            }
        }

        private void updateAssetPrefix(bool usedCustomToggle = false)
        {
            if (assets == null || assets.Count == 0)
                return;

            bool useCustom = CustomGridIconsOption.IsChecked;
            string prefix1 = "Grid_Old-";
            string prefix2 = "Grid_Min-";
            if (SonicIconsOption.IsChecked)
            {
                prefix1 = "Grid_Min-";
                prefix2 = "Grid_Old-";
            }

            for (int i = 0; i < assets.Count; i++)
            {
                //if already a custom prefix then skip
                if (useCustom && assets[i].StartsWith("Grid_Cus-"))
                    continue;

                //if custom toggle on then check for and replace normal prefix with custom one
                if (useCustom)
                {
                    string cusCheck = assets[i].Replace(prefix1, "Grid_Cus-");
                    if (usedCustomToggle)
                        cusCheck = assets[i].Replace(prefix2, "Grid_Cus-");
                    if (MainWindow.CusGridImagesList.Contains(cusCheck))
                    {
                        assets[i] = cusCheck;
                        continue;
                    }
                }

                //if custom toggle is off check if prefix was custom and fix it else replace as normal
                if (assets[i].StartsWith("Grid_Cus-"))
                    assets[i] = assets[i].Replace("Grid_Cus-", prefix2);
                else
                    assets[i] = assets[i].Replace(prefix1, prefix2);
            }

            Change_Icons();
        }

        private void Change_Icons()
        {
            if (grid == null)
                return;

            foreach (var child in grid.Children)
            {
                string prefix = "Grid_Min-";
                if (SonicIconsOption.IsChecked)
                    prefix = "Grid_Old-";

                //check if it's a toggle button
                if (child is ToggleButton square)
                {
                    string squareTag = square.Tag.ToString().Remove(0, 9);

                    if (CustomGridIconsOption.IsChecked && assets.Contains("Grid_Cus-" + squareTag))
                    {
                        prefix = "Grid_Cus-";
                    }

                    //only edit fog of war squares tag
                    if (fogOfWar)
                    {
                        if (square.Content == null || (square.Content is Image test && test.Source.ToString().EndsWith("QuestionMark.png")))
                        {
                            square.Tag = prefix + squareTag;
                            continue;
                        }
                    }

                    //check 
                    square.SetResourceReference(ContentProperty, prefix + squareTag);
                    square.Tag = prefix + squareTag;

                }
            }
        }

        private List<string> Asset_Collection(int seed = 1)
        {
            Random rng = new Random(seed);

            // RE-randomize which reports get included
            var numReports = Properties.Settings.Default.GridWindowNumReports;
            var randomReports = Enumerable.Range(1, 13).OrderBy(g => rng.Next()).Take(numReports).ToList();
            foreach (int reportNum in Enumerable.Range(1, 13).ToList())
                gridSettings[$"Report{reportNum}"] = randomReports.Contains(reportNum);

            // RE-randomize which visit unlocks get included
            List<string> unlockNames = (MainWindow.data.VisitLocks.Select(item => item.Name)).ToList();
            int numUnlocks = Properties.Settings.Default.GridWindowNumUnlocks;
            var randomUnlocks = Enumerable.Range(1, unlockNames.Count).OrderBy(g => rng.Next()).Take(numUnlocks).ToList();
            foreach (int i in Enumerable.Range(1, unlockNames.Count).ToList())
                gridSettings[unlockNames[i - 1]] = randomUnlocks.Contains(i);

            // RE-randomize which visit world chest locks get included
            List<string> worldChestLockNames = (MainWindow.data.ChestLocks.Select(item => item.Name)).ToList();
            int numChestLocks = Properties.Settings.Default.GridWindowNumChestLocks;
            var randomChestLocks = Enumerable.Range(1, worldChestLockNames.Count).OrderBy(g => rng.Next()).Take(numChestLocks).ToList();
            foreach (int i in Enumerable.Range(1, worldChestLockNames.Count).ToList())
                gridSettings[worldChestLockNames[i - 1]] = randomChestLocks.Contains(i);

            List<string> imageKeys = new List<string>();
            //use gridAssetList dictionary in Codes.cs as resource for every valid grid square 
            foreach (string resourceName in Codes.gridAssetList)
            {
                // add the item to the grid settings dictionary if it doesn't exist already (IN ACCORDANCE WITH USER SETTINGS)
                if (!gridSettings.ContainsKey(resourceName))
                    gridSettings[resourceName] = true;
                //if setting exists add it to list if true
                if (gridSettings[resourceName])
                    imageKeys.Add(resourceName);
            }

            //Shuffle the imageKeys then return it
            imageKeys = imageKeys.OrderBy(x => rng.Next()).ToList();
            return imageKeys;
        }

        public static Dictionary<string, Color> GetColorSettings()
        {

            var unmarkedColor = Properties.Settings.Default.UnmarkedColor;
            var markedColor = Properties.Settings.Default.MarkedColor;
            var annotatedColor = Properties.Settings.Default.AnnotatedColor;
            var bingoColor = Properties.Settings.Default.BingoColor;
            var hintColor = Properties.Settings.Default.HintColor;
            var battleshipMissColor = Properties.Settings.Default.BattleshipMissColor;
            var battleshipHitColor = Properties.Settings.Default.BattleshipHitColor;
            var battleshipSunkColor = Properties.Settings.Default.BattleshipSunkColor;

            return new Dictionary<string, Color>()
            {
                { "Unmarked Color", Color.FromArgb(unmarkedColor.A, unmarkedColor.R, unmarkedColor.G, unmarkedColor.B) },
                { "Marked Color", Color.FromArgb(markedColor.A, markedColor.R, markedColor.G, markedColor.B) },
                { "Annotated Color", Color.FromArgb(annotatedColor.A, annotatedColor.R, annotatedColor.G, annotatedColor.B) },
                { "Bingo Color", Color.FromArgb(bingoColor.A, bingoColor.R, bingoColor.G, bingoColor.B) },
                { "Hint Color", Color.FromArgb(hintColor.A, hintColor.R, hintColor.G, hintColor.B) },
                { "Battleship Miss Color", Color.FromArgb(battleshipMissColor.A, battleshipMissColor.R, battleshipMissColor.G, battleshipMissColor.B) },
                { "Battleship Hit Color", Color.FromArgb(battleshipHitColor.A, battleshipHitColor.R, battleshipHitColor.G, battleshipHitColor.B) },
                { "Battleship Sunk Color", Color.FromArgb(battleshipSunkColor.A, battleshipSunkColor.R, battleshipSunkColor.G, battleshipSunkColor.B) }
            };
        }

        public void Button_Click(object sender, RoutedEventArgs e, int i, int j)
        {
            var button = (ToggleButton)sender;
            if (lastTrackedButton != null)
            {
                lastTrackedButton.BorderBrush = new SolidColorBrush(Colors.Gray);
                lastTrackedButton.BorderThickness = new Thickness(0.1);  // Adjust thickness as needed
            }

            lastTrackedButton = button;
            // is this null check even needed?
            if (lastTrackedButton != null)
            {
                lastTrackedButton.BorderBrush = new SolidColorBrush(Colors.Yellow);
                lastTrackedButton.BorderThickness = new Thickness(5.5);  // Adjust thickness as needed
            }
            annotationStatus[i, j] = false;
            if (battleshipLogic)
            {
                if (battleshipSunkStatus[i, j])
                {
                    ShipSunkCheck(i, j);
                    UpdateShipStatusUI();
                }
                else if (button.IsChecked ?? false || annotationStatus[i, j])
                {
                    ShipSunkCheck(i, j);
                    UpdateShipStatusUI();
                }
                else
                {
                    originalColors[i, j] = currentColors["Unmarked Color"];
                    SetColorForButton(button.Background, currentColors["Unmarked Color"]);
                }
            }
            else
            {
                if (button.IsChecked ?? false || annotationStatus[i, j])
                {
                    originalColors[i, j] = currentColors["Marked Color"];
                    SetColorForButton(button.Background, currentColors["Marked Color"]);
                }
                else
                {
                    originalColors[i, j] = currentColors["Unmarked Color"];
                    SetColorForButton(button.Background, currentColors["Unmarked Color"]);
                }
                if (bingoLogic)
                {
                    BingoCheck(i, j);
                    UpdateBingoCells();
                }
            }
            if (fogOfWar)
            {
                // reveal the button's own property
                buttons[i, j].SetResourceReference(ContentProperty, assets[(i * numColumns) + j]);
                buttons[i, j].ToolTip = ((string)buttons[i, j].Tag).Split('-')[1];
                // reveal the button's neighbors' properties
                int westRange = fogOfWarSpan.ContainsKey("W") ? fogOfWarSpan["W"] : 1;
                int eastRange = fogOfWarSpan.ContainsKey("E") ? fogOfWarSpan["E"] : 1;
                int northRange = fogOfWarSpan.ContainsKey("N") ? fogOfWarSpan["N"] : 1;
                int southRange = fogOfWarSpan.ContainsKey("S") ? fogOfWarSpan["S"] : 1;
                int northwestRange = fogOfWarSpan.ContainsKey("NW") ? fogOfWarSpan["NW"] : 0;
                int northeastRange = fogOfWarSpan.ContainsKey("NE") ? fogOfWarSpan["NE"] : 0;
                int southwestRange = fogOfWarSpan.ContainsKey("SW") ? fogOfWarSpan["SW"] : 0;
                int southeastRange = fogOfWarSpan.ContainsKey("SE") ? fogOfWarSpan["SE"] : 0;
                // west check
                for (int west = 1; west <= westRange; west++)
                {
                    if ((j - west) >= 0)
                    {
                        var currentButton = buttons[i, j - west];
                        currentButton.SetResourceReference(ContentProperty, assets[(i * numColumns) + (j - west)]);
                    }

                }
                // east check
                for (int east = 1; east <= eastRange; east++)
                {
                    if ((j + east) < numColumns)
                    {
                        var currentButton = buttons[i, j + east];
                        currentButton.SetResourceReference(ContentProperty, assets[(i * numColumns) + (j + east)]);
                    }
                }
                // north check
                for (int north = 1; north <= northRange; north++)
                {
                    if ((i - north) >= 0)
                    {
                        var currentButton = buttons[i - north, j];
                        currentButton.SetResourceReference(ContentProperty, assets[((i - north) * numColumns) + j]);
                    }
                }
                // south check
                for (int south = 1; south <= southRange; south++)
                {
                    if ((i + south) < numRows)
                    {
                        var currentButton = buttons[i + south, j];
                        currentButton.SetResourceReference(ContentProperty, assets[((i + south) * numColumns) + j]);
                    }
                }
                // northwest check
                for (int northwest = 1; northwest <= northwestRange; northwest++)
                {
                    if ((i - northwest) >= 0 && (j - northwest) >= 0)
                    {
                        var currentButton = buttons[i - northwest, j - northwest];
                        currentButton.SetResourceReference(ContentProperty, assets[((i - northwest) * numColumns) + (j - northwest)]);
                    }
                }
                // northeast check
                for (int northeast = 1; northeast <= northeastRange; northeast++)
                {
                    if ((i - northeast) >= 0 && (j + northeast) < numColumns)
                    {
                        var currentButton = buttons[i - northeast, j + northeast];
                        currentButton.SetResourceReference(ContentProperty, assets[((i - northeast) * numColumns) + (j + northeast)]);
                    }
                }
                // southwest check
                for (int southwest = 1; southwest <= southwestRange; southwest++)
                {
                    if ((i + southwest) < numRows && (j - southwest) >= 0)
                    {
                        var currentButton = buttons[i + southwest, j - southwest];
                        currentButton.SetResourceReference(ContentProperty, assets[((i + southwest) * numColumns) + (j - southwest)]);
                    }
                }
                // southeast check
                for (int southeast = 1; southeast <= southeastRange; southeast++)
                {
                    if ((i + southeast) < numRows && (j + southeast) < numColumns)
                    {
                        var currentButton = buttons[i + southeast, j + southeast];
                        currentButton.SetResourceReference(ContentProperty, assets[((i + southeast) * numColumns) + (j + southeast)]);
                    }
                }
            }
        }

        public void Button_RightClick(object sender, RoutedEventArgs e, int i, int j)
        {
            var button = (ToggleButton)sender;
            if (annotationStatus[i, j])
            {
                annotationStatus[i, j] = false;
                SetColorForButton(button.Background, originalColors[i, j]);
            }
            else
            {
                annotationStatus[i, j] = true;
                SetColorForButton(button.Background, currentColors["Annotated Color"]);
            }
        }

        public void Button_Hover(object sender, RoutedEventArgs e, int i, int j)
        {
            var button = (ToggleButton)sender;
            SetColorForButton(button.Background, Colors.Silver);
        }

        public void Button_ExitHover(object sender, RoutedEventArgs e, int i, int j)
        {
            var button = (ToggleButton)sender;
            SetColorForButton(button.Background, annotationStatus[i, j] ? currentColors["Annotated Color"] : originalColors[i, j]);
        }

        public void GenerateGrid(object sender, RoutedEventArgs e)
        {
            grid.Children.Clear();
            GenerateGrid(numRows, numColumns);
        }

        private static string RandomSeedName(int length, int? seed = null)
        {
            var randValue = seed == null ? new Random() : new Random((int)seed);
            string alphanumeric = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            return new string(Enumerable.Repeat(alphanumeric, length)
              .Select(s => s[randValue.Next(s.Length)]).ToArray());
        }

        public void GenerateGrid(int rows = 5, int columns = 5, string seedString = null, bool userTrigger = true, bool presetUpload = false)
        {
            //reset banner visibility
            UpdateGridBanner(false);

            // default to 5x5 grid if negative value manages to make it in
            if (rows <= 0 || columns <= 0)
            {
                rows = 5;
                columns = 5;
            }

            grid = new Grid();
            // only trigger the options updates if the grid is changed by the user and not an automated check (e.g. bunterCheck)
            if (userTrigger)
            {
                gridOptionsWindow.InitializeData(this, data);
                gridOptionsWindow.UpdateGridOptionsUI();
            }

            buttons = new ToggleButton[rows, columns];
            // ensure that the cells all start with the unmarked color as their original colors
            originalColors = new Color[rows, columns];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    originalColors[i, j] = currentColors["Unmarked Color"];
                }
            }
            bingoStatus = new bool[rows, columns];
            battleshipSunkStatus = new bool[rows, columns];
            annotationStatus = new bool[rows, columns];

            seedName = seedString;

            if (seedName == null && (data?.convertedSeedHash ?? -1) > 0 && (data.firstGridOnSeedLoad || presetUpload))
            {
                string settingsString = $"{numRows}{numColumns}{bingoLogic}{battleshipLogic}{bunterLogic}{fogOfWar}{fogOfWarSpan}{shipSizes}{battleshipRandomCount}{minShipCount}{maxShipCount}{gridSettings}{coloredHints}{coloredHintsColors}{data.convertedSeedHash}";
                seed = settingsString.GetHashCode();
                seedName = $"[TIED TO SEED] {RandomSeedName(8, seed)}";
                data.firstGridOnSeedLoad = false;
            }
            else 
            {
                if (seedName == null)
                    seedName = RandomSeedName(8);
                seed = seedName.GetHashCode();
            }
            Seedname.Header = "Seed: " + seedName;
            Seedname.Header = (fogOfWar ? "Fog of War ON    " : "    ") + Seedname.Header;

            // handle bunter checks
            if (data.BossRandoFound)
                bunterCheck(bunterBosses);
            // get raw check names
            assets = Asset_Collection(seed);
            // set the content resource reference with style
            getAssetPrefix();

            // if there aren't enough assets to fit the grid, get the grid closest to the user input that can contain all assets
            int numChecks = assets.Count;
            int originalNumRows = rows;
            int originalNumColumns = columns;
            if (rows * columns > numChecks)
            {
                while (rows * columns > numChecks)
                {
                    int currentMax = Math.Max(rows, columns);
                    if (currentMax == rows)
                        rows -= 1;
                    else
                        columns -= 1;
                }
                numRows = rows;
                numColumns = columns;
                MessageBox.Show($"NOTE: Your original request for a grid of size {originalNumRows} x {originalNumColumns} is not possible with only {numChecks} allowed checks. Grid has been reduced to size of {numRows} x {numColumns}");
            }

            // generate the grid
            for (int i = 0; i < numRows; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            }

            for (int j = 0; j < numColumns; j++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            }

            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numColumns; j++)
                {
                    ToggleButton button = new ToggleButton();
                    bool buttonContentRevealed = buttons[i, j] != null && ((buttons[i, j].IsChecked ?? false) || buttons[i, j].Content != null);
                    
                    if (!fogOfWar || buttonContentRevealed)
                        button.SetResourceReference(ContentProperty, assets[(i * numColumns) + j]);
                    else
                    {
                        if (FogIconOption.IsChecked)
                        {
                            //check for custom
                            if (CustomGridIconsOption.IsChecked && Directory.Exists("CustomImages/Grid/"))
                            {
                                if (File.Exists("CustomImages/Grid/QuestionMark.png"))
                                {
                                    button.SetResourceReference(ContentProperty, "Grid_QuestionMark-Custom");
                                }
                                else
                                    button.SetResourceReference(ContentProperty, "Grid_QuestionMark");
                            }
                            else
                                button.SetResourceReference(ContentProperty, "Grid_QuestionMark");
                        }
                    }

                    button.Background = new SolidColorBrush(currentColors["Unmarked Color"]);
                    button.Tag = assets[(i * numColumns) + j].ToString();
                    button.Style = (Style)FindResource("ColorToggleButton");
                    // keep i and j static for the button
                    int current_i = i;
                    int current_j = j;
                    button.Click += (sender, e) => Button_Click(sender, e, current_i, current_j);
                    button.MouseRightButtonUp += (sender, e) => Button_RightClick(sender, e, current_i, current_j);
                    button.MouseEnter += (sender, e) => Button_Hover(sender, e, current_i, current_j);
                    button.MouseLeave += (sender, e) => Button_ExitHover(sender, e, current_i, current_j);
                    Grid.SetRow(button, i);
                    Grid.SetColumn(button, j);
                    buttons[i, j] = button;
                    grid.Children.Add(button);
                    //if (!fogOfWar)
                    //    button.ToolTip = ((string)button.Tag).Split('-')[1];
                }
            }

            // generate battleship board
            if (battleshipLogic)
            {
                if (battleshipRandomCount)
                {
                    Random rng = new Random(seed);
                    if (minShipCount > maxShipCount)
                        minShipCount = maxShipCount;
                    int numToSample = rng.Next(minShipCount, maxShipCount + 1);
                    sampledShipSizes = new List<int>();

                    for (int i = 0; i < numToSample; i++)
                    {
                        int randomIndex = rng.Next(shipSizes.Count);
                        sampledShipSizes.Add(shipSizes[randomIndex]);
                    }
                }
                else
                    sampledShipSizes = shipSizes;
                if (coloredHints)
                    coloredHintsColors = GenerateGradient(currentColors["Battleship Hit Color"], currentColors["Battleship Miss Color"], coloredHintsDistance + 1);
                placedShips = GenerateSameBoard(numRows, numColumns);
            }

            // generate the boss hints
            if (data.BossRandoFound)
            {
                bossHintContentControls = new Dictionary<string, ContentControl>();
                bossHintBorders = new Dictionary<string, Border>();
                for (int i = 0; i < numRows; i++)
                {
                    for (int j = 0; j < numColumns; j++)
                    {
                        // Create a new Grid as a container for the ContentControl
                        Grid hintContainer = new Grid
                        {
                            // Set the container to fill the grid cell
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Stretch
                        };

                        // Define row definitions for the hintContainer grid
                        int coveragePercentage = 32;
                        hintContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(coveragePercentage, GridUnitType.Star) }); // coveragePercentage for the hint
                        hintContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(100 - coveragePercentage, GridUnitType.Star) }); // 100 - coveragePercentage remains empty
                        hintContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100 - coveragePercentage, GridUnitType.Star) }); // 100 - coveragePercentage remains empty
                        hintContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(coveragePercentage, GridUnitType.Star) }); // coveragePercentage for the hint

                        // Create a Border with a white background for the top right cell
                        //Border whiteBackground = new Border
                        //{
                        //    Background = new SolidColorBrush(Colors.Transparent), // Make the inside of the border transparent
                        //    BorderBrush = new SolidColorBrush(Colors.White), // Set the color of the border edges
                        //    BorderThickness = new Thickness(3), // Set the thickness of the edges
                        //                                        // The rest of the properties remain the same
                        //};
                        Border whiteBackground = new Border
                        {
                            // this will be the background when a boss hint is acquired
                            //Background = new SolidColorBrush(Colors.White),
                        };

                        string bossName = ((string)buttons[i, j].Tag).Split('-')[1];
                        bossHintBorders[bossName] = whiteBackground;

                        // Set the Border to occupy the top 35% and the right 35% of the hintContainer
                        Grid.SetRow(whiteBackground, 0);
                        Grid.SetColumn(whiteBackground, 1);
                        hintContainer.Children.Add(whiteBackground);

                        // Create the ContentControl with desired properties
                        ContentControl contentControl = new ContentControl
                        {
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Stretch,
                        };

                        // Naming the ContentControl using its grid position
                        bossHintContentControls[bossName] = contentControl;

                        // Add the ContentControl to the first row of the hintContainer
                        Grid.SetRow(contentControl, 0); // Place it in the top 35% row
                        Grid.SetColumn(contentControl, 1); // Place it in the right 35% column
                        hintContainer.Children.Add(contentControl);

                        // Set the hintContainer to be in the specific cell of the main grid
                        Grid.SetRow(hintContainer, i);
                        Grid.SetColumn(hintContainer, j);

                        // Add the hintContainer to the main grid, instead of directly adding the contentControl
                        grid.Children.Add(hintContainer);
                    }
                }
            }
            // Add grid to the window or other container
            DynamicGrid.Children.Add(grid);
        }

        public void SetColorForButton(Brush buttonBackground, Color newColor)
        {
            ((SolidColorBrush)buttonBackground).Color = newColor;
        }

        public Color GetColorFromButton(Brush buttonBackground)
        {
            return ((SolidColorBrush)buttonBackground).Color;
        }

        public void BingoCheck(int i, int j)
        {

            // remove any bingos if we are unclicking
            if (buttons[i, j].IsChecked == false)
            {
                // remove unclicked button's bingo status
                bingoStatus[i, j] = false;

                // check if we can have diagonal bingos
                if (numRows == numColumns)
                {
                    // remove left diagonal
                    if (i == j)
                    {
                        for (int index = 0; index < numRows; index++)
                        {
                            if (bingoStatus[index, index])
                            {
                                // check that the button in question is not a part of a row or column bingo before removing bingo background
                                bool partOfRowBingo = true;
                                bool partOfColumnBingo = true;
                                for (int check = 0; check < numRows; check++)
                                {
                                    if (!bingoStatus[index, check])
                                        partOfRowBingo = false;
                                    if (!bingoStatus[check, index])
                                        partOfColumnBingo = false;
                                    if (!partOfRowBingo && !partOfColumnBingo)
                                    {
                                        if (index != i)
                                        {
                                            // check that the middle button (if it exists) is not part of the other diagonal bingo
                                            if (index * 2 == numRows - 1)
                                            {
                                                for (int diagCheck = 0; diagCheck < numRows; diagCheck++)
                                                {
                                                    if (!bingoStatus[diagCheck, numRows - 1 - diagCheck])
                                                    {
                                                        bingoStatus[index, index] = false;
                                                        break;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                bingoStatus[index, index] = false;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // remove right diagonal
                    if (i == numRows - 1 - j)
                    {
                        for (int index = 0; index < numRows; index++)
                        {
                            if (bingoStatus[index, numRows - 1 - index])
                            {
                                // check that the button in question is not a part of a row or column bingo before removing bingo background
                                bool partOfRowBingo = true;
                                bool partOfColumnBingo = true;
                                for (int check = 0; check < numRows; check++)
                                {
                                    if (!bingoStatus[index, check])
                                        partOfRowBingo = false;
                                    if (!bingoStatus[check, numRows - 1 - index])
                                        partOfColumnBingo = false;
                                    if (!partOfRowBingo && !partOfColumnBingo)
                                    {
                                        if (index != i)
                                        {
                                            // check that the middle button (if it exists) is not part of the other diagonal bingo
                                            if (index * 2 == numRows - 1)
                                            {
                                                for (int diagCheck = 0; diagCheck < numRows; diagCheck++)
                                                {
                                                    if (!bingoStatus[diagCheck, diagCheck])
                                                    {
                                                        bingoStatus[index, numRows - 1 - index] = false;
                                                        break;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                bingoStatus[index, numRows - 1 - index] = false;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                // remove vertical bingo
                for (int row = 0; row < numRows; row++)
                {
                    bool partOfRowBingo = true;
                    bool partOfLeftDiagBingo = true;
                    bool partOfRightDiagBingo = true;

                    if (bingoStatus[row, j])
                    {
                        // check that the button in question is not a part of a row bingo before removing bingo background 
                        for (int colCheck = 0; colCheck < numColumns; colCheck++)
                        {
                            if (row != i)
                            {

                                if (!bingoStatus[row, colCheck])
                                {
                                    partOfRowBingo = false;
                                    break;
                                }
                            }
                        }
                        if (numRows == numColumns)
                        {
                            // check that the button in question is not a part of left diagonal bingo before removing bingo background 
                            if (j == row)
                            {
                                for (int leftDiagCheck = 0; leftDiagCheck < numRows; leftDiagCheck++)
                                {
                                    if (!bingoStatus[leftDiagCheck, leftDiagCheck])
                                    {
                                        partOfLeftDiagBingo = false;
                                        break;
                                    }
                                }
                            }
                            else
                                partOfLeftDiagBingo = false;
                            // check that the button in question is not a part of right diagonal bingo before removing bingo background
                            if (j == numRows - 1 - row)
                            {
                                for (int rightDiagCheck = 0; rightDiagCheck < numRows; rightDiagCheck++)
                                {
                                    if (!bingoStatus[rightDiagCheck, numRows - 1 - rightDiagCheck])
                                    {
                                        partOfRightDiagBingo = false;
                                        break;
                                    }
                                }
                            }
                            else
                                partOfRightDiagBingo = false;
                        }
                        else
                        {
                            partOfLeftDiagBingo = false;
                            partOfRightDiagBingo = false;
                        }
                        if (!partOfRowBingo && !partOfLeftDiagBingo && !partOfRightDiagBingo)
                            bingoStatus[row, j] = false;
                    }
                }

                // remove horizontal bingo
                for (int col = 0; col < numColumns; col++)
                {
                    bool partOfColumnBingo = true;
                    bool partOfLeftDiagBingo = true;
                    bool partOfRightDiagBingo = true;

                    if (bingoStatus[i, col])
                    {
                        // check that the button in question is not a part of a column bingo before removing bingo background 
                        for (int rowCheck = 0; rowCheck < numRows; rowCheck++)
                        {
                            if (col != j)
                            {
                                if (!bingoStatus[rowCheck, col])
                                {
                                    partOfColumnBingo = false;
                                    break;
                                }
                            }
                        }
                        if (numRows == numColumns)
                        {
                            // check that the button in question is not a part of left diagonal bingo before removing bingo background 
                            if (i == col)
                            {
                                for (int leftDiagCheck = 0; leftDiagCheck < numRows; leftDiagCheck++)
                                {
                                    if (!bingoStatus[leftDiagCheck, leftDiagCheck])
                                    {
                                        partOfLeftDiagBingo = false;
                                        break;
                                    }
                                }
                            }
                            else
                                partOfLeftDiagBingo = false;
                            // check that the button in question is not a part of right diagonal bingo before removing bingo background
                            if (i == numRows - 1 - col)
                            {
                                for (int rightDiagCheck = 0; rightDiagCheck < numRows; rightDiagCheck++)
                                {
                                    if (!bingoStatus[rightDiagCheck, numRows - 1 - rightDiagCheck])
                                    {
                                        partOfRightDiagBingo = false;
                                        break;
                                    }
                                }
                            }
                            else
                                partOfRightDiagBingo = false;
                        }
                        else
                        {
                            partOfLeftDiagBingo = false;
                            partOfRightDiagBingo = false;
                        }
                        if (!partOfColumnBingo && !partOfLeftDiagBingo && !partOfRightDiagBingo)
                            bingoStatus[i, col] = false;
                    }
                }
            }

            // add any bingos if we are clicking
            else
            {
                // check if we can have diagonal bingos
                if (numRows == numColumns)
                {
                    // add left diagonal
                    if (i == j)
                    {
                        for (int index = 0; index < numRows; index++)
                        {
                            if (buttons[index, index].IsChecked == false)
                            {
                                break;
                            }
                            if (index == numRows - 1)
                            {
                                for (int bingoIndex = 0; bingoIndex < numRows; bingoIndex++)
                                    bingoStatus[bingoIndex, bingoIndex] = true;
                            }
                        }

                    }

                    // add right diagonal
                    if (i == numRows - 1 - j)
                    {
                        for (int index = 0; index < numRows; index++)
                        {
                            if (buttons[index, numRows - 1 - index].IsChecked == false)
                            {
                                break;
                            }
                            if (index == numRows - 1)
                            {
                                for (int bingoIndex = 0; bingoIndex < numRows; bingoIndex++)
                                    bingoStatus[bingoIndex, numRows - 1 - bingoIndex] = true;
                            }
                        }

                    }
                }
                // add vertical bingo
                for (int row = 0; row < numRows; row++)
                {
                    if (buttons[row, j].IsChecked == false)
                    {
                        break;
                    }
                    if (row == numRows - 1)
                    {
                        for (int bingoRow = 0; bingoRow < numRows; bingoRow++)
                            bingoStatus[bingoRow, j] = true;
                    }
                }

                // add horizontal bingo
                for (int col = 0; col < numColumns; col++)
                {
                    if (buttons[i, col].IsChecked == false)
                    {
                        break;
                    }
                    if (col == numColumns - 1)
                    {
                        for (int bingoCol = 0; bingoCol < numColumns; bingoCol++)
                            bingoStatus[i, bingoCol] = true;
                    }
                }
            }
        }

        public void UpdateBingoCells()
        {
            for (int row = 0; row < numRows; row++)
            {
                for (int column = 0; column < numColumns; column++)
                {
                    if (bingoStatus[row, column])
                    {
                        SetColorForButton(buttons[row, column].Background, currentColors["Bingo Color"]);
                        originalColors[row, column] = GetColorFromButton(buttons[row, column].Background);
                        annotationStatus[row, column] = false;
                    }
                    else if (buttons[row, column].IsChecked ?? false)
                    {
                        originalColors[row, column] = (buttons[row, column].IsChecked ?? false) ? currentColors["Marked Color"] : currentColors["Unmarked Color"];
                        if (annotationStatus[row, column])
                            SetColorForButton(buttons[row, column].Background, currentColors["Marked Color"]);
                    }
                    else
                    {
                        SetColorForButton(buttons[row, column].Background, currentColors["Unmarked Color"]);
                        originalColors[row, column] = GetColorFromButton(buttons[row, column].Background);
                    }
                    if (annotationStatus[row, column])
                    {
                        SetColorForButton(buttons[row, column].Background, currentColors["Annotated Color"]);
                    }
                }
            }
        }
        
        public void bunterCheck(List<Dictionary<string, object>> bosses)
        {
            //don't bother performing the check if bosses is null
            if (bosses == null)
                return;
            // update valid bosses for grid tracker
            foreach (var bosspair in bosses)
            {
                string bossOrig = bosspair["original"].ToString();
                // string bossRepl = bosspair["new"].ToString();

                if (data.codes.bossNameConversion.ContainsKey(bossOrig))
                {
                    if (gridSettings.ContainsKey(data.codes.bossNameConversion[bossOrig]))
                        gridSettings[data.codes.bossNameConversion[bossOrig]] = false;
                    else if (gridSettings.ContainsKey("Grid" + data.codes.bossNameConversion[bossOrig]))
                        gridSettings["Grid" + data.codes.bossNameConversion[bossOrig]] = false;
                }
            }

            foreach (var bosspair in bosses)
            {
                string bossOrig = bosspair["original"].ToString();
                string bossRepl = bosspair["new"].ToString();

                // disable bosses not in the values of the boss enemy dict
                if (data.codes.bossNameConversion.ContainsKey(bossOrig))
                {
                    if (gridSettings.ContainsKey(data.codes.bossNameConversion[bossOrig]))
                    {
                        var convertedBossName = data.codes.bossNameConversion[bossOrig];
                        gridSettings[convertedBossName] = gridSettings[convertedBossName] ? true : data.BossList.ContainsValue(bossOrig);
                    }
                    else if (gridSettings.ContainsKey("Grid" + data.codes.bossNameConversion[bossOrig]))
                    {
                        var convertedBossName = "Grid" + data.codes.bossNameConversion[bossOrig];
                        gridSettings[convertedBossName] = gridSettings[convertedBossName] ? true : data.BossList.ContainsValue(bossOrig);
                    }
                }
            }

            foreach (var bosspair in bosses)
            {
                string bossOrig = bosspair["original"].ToString();
                string bossRepl = bosspair["new"].ToString();

                //Special case, check axel first or it'll crash
                // if STT is off, ensure only the Data Axel replacement is eligible
                if (bossOrig == "Axel II")
                {
                    if (!data.BossList.ContainsKey("Axel I"))
                    {
                        bool axelTwoKeyExists = data.BossList.ContainsKey(bossOrig);
                        bool dataAxelKeyExists = data.BossList.ContainsKey(bossOrig.Replace("II", "(Data)"));
                        bool axelTwoReplacementKeyExists = data.codes.bossNameConversion.ContainsKey(data.BossList[bossOrig]);
                        bool dataAxelReplacementKeyExists = data.codes.bossNameConversion.ContainsKey(data.BossList[bossOrig.Replace("II", "(Data)")]);
                        bool valueBossesEqual = (data.codes.bossNameConversion[data.BossList[bossOrig]] != data.codes.bossNameConversion[data.BossList[bossOrig.Replace("II", "(Data)")]]);
                        if (axelTwoKeyExists && dataAxelKeyExists && axelTwoReplacementKeyExists && dataAxelReplacementKeyExists && valueBossesEqual)
                        {
                            if (gridSettings.ContainsKey(data.codes.bossNameConversion[bossRepl]))
                                gridSettings[data.codes.bossNameConversion[bossRepl]] = false;
                            else if (gridSettings.ContainsKey("Grid" + data.codes.bossNameConversion[bossRepl]))
                                gridSettings["Grid" + data.codes.bossNameConversion[bossRepl]] = false;
                        }
                    }
                }
                // if STT is on, ensure only the Axel II replacement is eligible
                else if (bossOrig == "Axel (Data)")
                {
                    if (data.BossList.ContainsKey("Axel I"))
                    {
                        bool dataAxelKeyExists = data.BossList.ContainsKey(bossOrig);
                        bool axelTwoKeyExists = data.BossList.ContainsKey(bossOrig.Replace("(Data)", "II"));
                        bool dataAxelReplacementKeyExists = data.codes.bossNameConversion.ContainsKey(data.BossList[bossOrig]);
                        bool axelTwoReplacementKeyExists = data.codes.bossNameConversion.ContainsKey(data.BossList[bossOrig.Replace("(Data)", "II")]);
                        bool valueBossesEqual = (data.codes.bossNameConversion[data.BossList[bossOrig]] != data.codes.bossNameConversion[data.BossList[bossOrig.Replace("(Data)", "II")]]);
                        if (dataAxelKeyExists && axelTwoKeyExists && dataAxelReplacementKeyExists && axelTwoReplacementKeyExists && valueBossesEqual)
                        {
                            if (gridSettings.ContainsKey(data.codes.bossNameConversion[bossRepl]))
                                gridSettings[data.codes.bossNameConversion[bossRepl]] = false;
                            else if (gridSettings.ContainsKey("Grid" + data.codes.bossNameConversion[bossRepl]))
                                gridSettings["Grid" + data.codes.bossNameConversion[bossRepl]] = false;
                        }
                    }
                }
                // disable bosses in data arenas
                else if (bossOrig.Contains("(Data)"))
                {
                    bool nonDataVersionExists = data.BossList.ContainsKey(bossOrig.Replace(" (Data)", "")); // non-data version exists
                    bool dataKeyExists = data.codes.bossNameConversion.ContainsKey(data.BossList[bossOrig]); // ensure the data version of the new boss name can be converted
                    bool newBossKeyExists = data.codes.bossNameConversion.ContainsKey(data.BossList[bossOrig.Replace(" (Data)", "")]); // ensure the new boss name can be converted
                    bool valueBossesEqual = data.codes.bossNameConversion[data.BossList[bossOrig]] != data.codes.bossNameConversion[data.BossList[bossOrig.Replace(" (Data)", "")]]; // check if the converted names of the new bosses are not the same
                    if (nonDataVersionExists && dataKeyExists && newBossKeyExists && valueBossesEqual)
                    {
                        if (gridSettings.ContainsKey(data.codes.bossNameConversion[bossRepl]))
                            gridSettings[data.codes.bossNameConversion[bossRepl]] = false;
                        else if (gridSettings.ContainsKey("Grid" + data.codes.bossNameConversion[bossRepl]))
                            gridSettings["Grid" + data.codes.bossNameConversion[bossRepl]] = false;
                    }
                }
                // disable cups replacements
                else if (bossOrig.Contains("Cups"))
                {
                    if (bossOrig == "Hades Cups" || bossOrig == "Pete Cups")
                        continue;

                    bool nonCupsVersionExists1 = data.BossList.ContainsKey(bossOrig.Replace(" (Cups)", "")); // non-cups version exists
                    bool cupsKeyExists1 = data.codes.bossNameConversion.ContainsKey(data.BossList[bossOrig]); // ensure the cups version of the new boss name can be converted
                    bool newBossKeyExists1 = data.codes.bossNameConversion.ContainsKey(bossOrig.Replace(" (Cups)", "")); // ensure the new boss name can be converted
                    bool valueBossesEqual1 = data.codes.bossNameConversion[data.BossList[bossOrig]] != data.codes.bossNameConversion[data.BossList[bossOrig.Replace(" (Cups)", "")]]; // check if the converted names of the new bosses are not the same
                    bool sameBossCheck1 = (nonCupsVersionExists1 && cupsKeyExists1 && newBossKeyExists1 && valueBossesEqual1);

                    bool nonCupsVersionExists2 = data.BossList.ContainsKey(bossOrig.Replace(" Cups", "")); // non-cups version exists
                    bool cupsKeyExists2 = data.codes.bossNameConversion.ContainsKey(data.BossList[bossOrig]); // ensure the cups version of the new boss name can be converted
                    bool newBossKeyExists2 = nonCupsVersionExists2 && data.codes.bossNameConversion.ContainsKey(bossOrig.Replace(" Cups", "")); // ensure the new boss name can be converted
                    bool sameBossCheck2 = (nonCupsVersionExists2 && cupsKeyExists2 && newBossKeyExists2 && (data.codes.bossNameConversion[data.BossList[bossOrig]] != data.codes.bossNameConversion[data.BossList[bossOrig.Replace(" Cups", "")]]));
                    if (sameBossCheck1 || sameBossCheck2)
                    {
                        if (gridSettings.ContainsKey(data.codes.bossNameConversion[bossRepl]))
                            gridSettings[data.codes.bossNameConversion[bossRepl]] = false;
                        else if (gridSettings.ContainsKey("Grid" + data.codes.bossNameConversion[bossRepl]))
                            gridSettings["Grid" + data.codes.bossNameConversion[bossRepl]] = false;
                    }
                }

                // if Hades is an org member, ensure it's the right one
                else if (bossOrig == "Hades II")
                {
                    bool hadesTwoKeyExists = data.BossList.ContainsKey(bossOrig);
                    bool hadesTwoOneKeyExists = data.BossList.ContainsKey(bossOrig + " (1)");
                    bool hadesTwoReplacementKeyExists = data.codes.bossNameConversion.ContainsKey(data.BossList[bossOrig]);
                    bool hadesTwoOneReplacementKeyExists = data.codes.bossNameConversion.ContainsKey(data.BossList[bossOrig + " (1)"]);
                    bool valueBossesEqual = data.codes.bossNameConversion[data.BossList[bossOrig]] != data.codes.bossNameConversion[data.BossList[bossOrig + " (1)"]];
                    if (hadesTwoKeyExists && hadesTwoOneKeyExists && hadesTwoReplacementKeyExists && hadesTwoOneReplacementKeyExists && valueBossesEqual)
                    {
                        if (gridSettings.ContainsKey(data.codes.bossNameConversion[bossRepl]))
                            gridSettings[data.codes.bossNameConversion[bossRepl]] = false;
                        else if (gridSettings.ContainsKey("Grid" + data.codes.bossNameConversion[bossRepl]))
                            gridSettings["Grid" + data.codes.bossNameConversion[bossRepl]] = false;
                    }
                }

                // if STT is off, ensure only the Data Axel replacement is eligible
                if (bossOrig == "Axel II")
                {
                    if (!data.BossList.ContainsKey("Axel I"))
                    {
                        bool axelTwoKeyExists = data.BossList.ContainsKey(bossOrig);
                        bool dataAxelKeyExists = data.BossList.ContainsKey(bossOrig.Replace("II", "(Data)"));
                        bool axelTwoReplacementKeyExists = data.codes.bossNameConversion.ContainsKey(data.BossList[bossOrig]);
                        bool dataAxelReplacementKeyExists = data.codes.bossNameConversion.ContainsKey(data.BossList[bossOrig.Replace("II", "(Data)")]);
                        bool valueBossesEqual = (data.codes.bossNameConversion[data.BossList[bossOrig]] != data.codes.bossNameConversion[data.BossList[bossOrig.Replace("II", "(Data)")]]);
                        if (axelTwoKeyExists && dataAxelKeyExists && axelTwoReplacementKeyExists && dataAxelReplacementKeyExists && valueBossesEqual)
                        {
                            if (gridSettings.ContainsKey(data.codes.bossNameConversion[bossRepl]))
                                gridSettings[data.codes.bossNameConversion[bossRepl]] = false;
                            else if (gridSettings.ContainsKey("Grid" + data.codes.bossNameConversion[bossRepl]))
                                gridSettings["Grid" + data.codes.bossNameConversion[bossRepl]] = false;
                        }
                    }
                }

                // if STT is on, ensure only the Axel II replacement is eligible
                if (bossOrig == "Axel (Data)")
                {
                    if (data.BossList.ContainsKey("Axel I"))
                    {
                        bool dataAxelKeyExists = data.BossList.ContainsKey(bossOrig);
                        bool axelTwoKeyExists = data.BossList.ContainsKey(bossOrig.Replace("(Data)", "II"));
                        bool dataAxelReplacementKeyExists = data.codes.bossNameConversion.ContainsKey(data.BossList[bossOrig]);
                        bool axelTwoReplacementKeyExists = data.codes.bossNameConversion.ContainsKey(data.BossList[bossOrig.Replace("(Data)", "II")]);
                        bool valueBossesEqual = (data.codes.bossNameConversion[data.BossList[bossOrig]] != data.codes.bossNameConversion[data.BossList[bossOrig.Replace("(Data)", "II")]]);
                        if (dataAxelKeyExists && axelTwoKeyExists && dataAxelReplacementKeyExists && axelTwoReplacementKeyExists && valueBossesEqual)
                        {
                            if (gridSettings.ContainsKey(data.codes.bossNameConversion[bossRepl]))
                                gridSettings[data.codes.bossNameConversion[bossRepl]] = false;
                            else if (gridSettings.ContainsKey("Grid" + data.codes.bossNameConversion[bossRepl]))
                                gridSettings["Grid" + data.codes.bossNameConversion[bossRepl]] = false;
                        }
                    }
                }
            }
            // remove Pete OC replacement if technically they are different
            if (bunterLogic)
            {
                if (data.BossList["Pete OC II"] != data.BossList["Pete TR"])
                    gridSettings[data.codes.bossNameConversion[data.BossList["Pete OC II"]]] = false;
            }
        }

        public int[,] GenerateSameBoard(int numRows, int numColumns)
        {
            random = new Random(seed);
            placedShips = new int[numRows, numColumns];

            // Initialize all possible starting points for ship heads
            possibleShipHeads = Enumerable.Range(0, numRows)
                                           .SelectMany(row => Enumerable.Range(0, numColumns), (row, col) => new Tuple<int, int>(row, col))
                                           .ToList();

            if (!TryPlaceShips(0))
            {
                MessageBox.Show("Failed to place all ships. As a result, no ships have been placed. Please increase the grid dimensions or lower the number/size of ships.");
                placedShips = new int[numRows, numColumns];
            }

            return placedShips;
        }

        private bool TryPlaceShips(int shipIndex)
        {
            if (shipIndex >= sampledShipSizes.Count)
            {
                return true; // All ships placed successfully
            }

            int shipSize = sampledShipSizes[shipIndex];
            // Shuffle possible starting points to ensure random selection
            var shuffledShipHeads = possibleShipHeads.OrderBy(x => random.Next()).ToList();
            int maxPlacementAttempts = 100;
            int shipPlacementAttempts = 0;

            foreach (var head in shuffledShipHeads)
            {
                var directions = new List<string> { "down", "right" };
                // Shuffle directions to randomize the orientation selection
                var shuffledDirections = directions.OrderBy(x => random.Next()).ToList();

                foreach (var direction in shuffledDirections)
                {
                    if (IsDirectionValid(placedShips, head.Item1, head.Item2, shipSize, direction, numRows, numColumns))
                    {
                        PlaceShip(head, shipSize, direction);
                        if (TryPlaceShips(shipIndex + 1))
                        {
                            return true;
                        }
                        // Remove the ship if placing subsequent ships failed, effectively backtracking
                        RemoveShip(head, shipSize, direction);
                        shipPlacementAttempts++;
                        if (shipPlacementAttempts < maxPlacementAttempts)
                            return false;
                    }
                }
            }

            return false; // Unable to place this ship, prompting backtracking
        }

        private void RemoveShip(Tuple<int, int> shipHead, int shipSize, string direction)
        {
            int x = shipHead.Item1;
            int y = shipHead.Item2;

            if (direction == "down")
            {
                for (int i = 0; i < shipSize; i++)
                {
                    placedShips[x + i, y] = 0; // Mark the cell as empty
                }
            }
            else if (direction == "right")
            {
                for (int i = 0; i < shipSize; i++)
                {
                    placedShips[x, y + i] = 0; // Mark the cell as empty
                }
            }
        }

        // Your existing IsDirectionValid method goes here unchanged.

        private void PlaceShip(Tuple<int, int> shipHead, int shipSize, string direction)
        {
            int x = shipHead.Item1;
            int y = shipHead.Item2;

            if (direction == "down")
            {
                for (int i = 0; i < shipSize; i++)
                {
                    placedShips[x + i, y] = currentShipId;
                }
            }
            else if (direction == "right")
            {
                for (int i = 0; i < shipSize; i++)
                {
                    placedShips[x, y + i] = currentShipId;
                }
            }
            currentShipId++; // Move to the next ship ID for the next ship
        }

        bool IsDirectionValid(int[,] board, int x, int y, int shipSize, string direction, int numRows, int numColumns)
        {
            if (direction == "down")
            {
                if (x + shipSize > numRows) return false; // Ship goes out of bounds
                for (int i = 0; i < shipSize; i++)
                {
                    // Check if the cell is already occupied
                    if (board[x + i, y] != 0) return false;

                    // Check adjacent cells for existing ships (excluding diagonal cells)
                    if (y > 0 && board[x + i, y - 1] != 0) return false; // Left
                    if (y < numColumns - 1 && board[x + i, y + 1] != 0) return false; // Right
                    if (i == 0 && x > 0 && board[x - 1, y] != 0) return false; // Above the first cell
                    if (i == shipSize - 1 && x + shipSize < numRows && board[x + shipSize, y] != 0) return false; // Below the last cell
                }
            }
            else if (direction == "right")
            {
                if (y + shipSize > numColumns) return false; // Ship goes out of bounds
                for (int i = 0; i < shipSize; i++)
                {
                    // Check if the cell is already occupied
                    if (board[x, y + i] != 0) return false;

                    // Check adjacent cells for existing ships (excluding diagonal cells)
                    if (x > 0 && board[x - 1, y + i] != 0) return false; // Above
                    if (x < numRows - 1 && board[x + 1, y + i] != 0) return false; // Below
                    if (i == 0 && y > 0 && board[x, y - 1] != 0) return false; // Left of the first cell
                    if (i == shipSize - 1 && y + shipSize < numColumns && board[x, y + shipSize] != 0) return false; // Right of the last cell
                }
            }

            return true;
        }

        private void ShipSunkCheck(int hitRow, int hitColumn)
        {
            bool shipSunk = true;
            int shipId = placedShips[hitRow, hitColumn];

            // Iterate over the entire grid to check if any part of the ship is not hit
            if (shipId > 0)
            {
                for (int row = 0; row < numRows; row++)
                {
                    for (int column = 0; column < numColumns; column++)
                    {
                        // If we find a part of the ship that has not been hit, return false
                        if (placedShips[row, column] == shipId && !(buttons[row, column].IsChecked ?? false))
                        {
                            shipSunk = false;
                        }
                        if (!shipSunk)
                            break;
                    }
                    if (!shipSunk)
                        break;
                }
                for (int row = 0; row < numRows; row++)
                {
                    for (int column = 0; column < numColumns; column++)
                    {
                        if (placedShips[row, column] == shipId)
                        {
                            battleshipSunkStatus[row, column] = shipSunk;
                        }
                    }
                }
            }
        }

        private void UpdateShipStatusUI()
        {
            for (int row = 0; row < numRows; row++)
            {
                for (int column = 0; column < numColumns; column++)
                {
                    Color missColor = coloredHints ? coloredHintsColors[DistanceToNearestShip(row, column)] : currentColors["Battleship Miss Color"];
                    if (battleshipSunkStatus[row, column] && placedShips.Cast<int>().Max() > 0)
                    {
                        SetColorForButton(buttons[row, column].Background, currentColors["Battleship Sunk Color"]);
                        originalColors[row, column] = GetColorFromButton(buttons[row, column].Background);
                        annotationStatus[row, column] = false;
                    }
                    else if (buttons[row, column].IsChecked ?? false)
                    {    
                        originalColors[row, column] = (placedShips[row, column] != 0) ? currentColors["Battleship Hit Color"] : missColor;
                        if (!annotationStatus[row, column])
                            SetColorForButton(buttons[row, column].Background, (placedShips[row, column] != 0) ? currentColors["Battleship Hit Color"] : missColor);
                    }
                    else
                    {
                        SetColorForButton(buttons[row, column].Background, currentColors["Unmarked Color"]);
                        originalColors[row, column] = GetColorFromButton(buttons[row, column].Background);
                    }
                    if (annotationStatus[row, column])
                    {
                        SetColorForButton(buttons[row, column].Background, currentColors["Annotated Color"]);
                    }
                }
            }
            bool allShipsSunk = true;
            for (int row = 0; row < numRows; row++)
            {
                for (int column = 0; column < numColumns; column++)
                {
                    if (placedShips[row, column] != 0)
                    {
                        if (!battleshipSunkStatus[row, column])
                            allShipsSunk = false;
                        // stop checking for all ships sunk
                        if (!allShipsSunk)
                            break;
                    }
                }
                // stop checking for all ships sunk
                if (!allShipsSunk)
                    break;
            }
            if (allShipsSunk && placedShips.Cast<int>().Max() > 0)
            {
                // spoil the card with all the remaining unchecked cells
                for (int row = 0; row < numRows; row++)
                {
                    for (int column = 0; column < numColumns; column++)
                    {
                        if (!buttons[row, column].IsChecked ?? false)
                        {
                            buttons[row, column].SetResourceReference(ContentProperty, assets[(row * numColumns) + column]);
                            SetColorForButton(buttons[row, column].Background, coloredHints ? coloredHintsColors[DistanceToNearestShip(row, column)] : currentColors["Battleship Miss Color"]);
                            originalColors[row, column] = GetColorFromButton(buttons[row, column].Background);
                        }
                    }
                }
                UpdateGridBanner(true, "SUNK ALL SHIPS!", "H");
            }
        }

        public int DistanceToNearestShip(int i, int j)
        {
            int distanceToNearestShip = coloredHintsDistance;
            for (int row = 0; row < numRows; row++)
            {
                for (int column = 0; column < numColumns; column++)
                {
                    if (placedShips[row, column] > 0)
                    {
                        int currentDistance = Math.Abs(row - i) + Math.Abs(column - j);
                        if (currentDistance < distanceToNearestShip)
                            distanceToNearestShip = currentDistance;
                    }
                }
            }
            return distanceToNearestShip;
        }

        public static List<Color> GenerateGradient(Color colorA, Color colorB, int steps, double dropOffIntensity = 1.0)
        {
            List<Color> gradient = new List<Color>();

            for (int i = 0; i < steps; i++)
            {
                double ratio = (double)i / (steps - 1);
                double dropOff = (i > 0 ? 1.0 : 0.0);
                byte r = (byte)(dropOffIntensity * dropOff * colorA.R + ratio * (colorB.R - dropOffIntensity * dropOff * colorA.R));
                byte g = (byte)(dropOffIntensity * dropOff * colorA.G + ratio * (colorB.G - dropOffIntensity * dropOff * colorA.G));
                byte b = (byte)(dropOffIntensity * dropOff * colorA.B + ratio * (colorB.B - dropOffIntensity * dropOff * colorA.B));
                //byte r = (byte)(colorA.R + ratio * (colorB.R - colorA.R));
                //byte g = (byte)(colorA.G + ratio * (colorB.G - colorA.G));
                //byte b = (byte)(colorA.B + ratio * (colorB.B - colorA.B));

                gradient.Add(Color.FromRgb(r, g, b));
            }

            return gradient;
        }

        // updates colors upon close
        private void PickColor_Click(object sender, RoutedEventArgs e)
        {
            // prompt user for new colors
            colorPickerWindow.Show();
        }

        public void HandleClosing(ColorPickerWindow sender)
        {

            coloredHintsColors = GenerateGradient(currentColors["Battleship Hit Color"], currentColors["Battleship Miss Color"], coloredHintsDistance + 1);
            //update the new colors on the card
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numColumns; j++)
                {
                    Color missColor = coloredHints ? coloredHintsColors[DistanceToNearestShip(i, j)] : currentColors["Battleship Miss Color"];
                    if (annotationStatus[i, j])
                        SetColorForButton(buttons[i, j].Background, currentColors["Annotated Color"]);
                    if (battleshipLogic)
                    {
                        if (battleshipSunkStatus[i, j])
                        {
                            SetColorForButton(buttons[i, j].Background, currentColors["Battleship Sunk Color"]);
                        }
                        else
                        {
                            bool squareIsShip = placedShips[i, j] != 0;
                            SetColorForButton(buttons[i, j].Background, (bool)buttons[i, j].IsChecked ? (squareIsShip ? currentColors["Battleship Hit Color"] : missColor) : currentColors["Unmarked Color"]);
                        }
                    }
                    else
                    {
                        SetColorForButton(buttons[i, j].Background, (bool)buttons[i, j].IsChecked ? currentColors["Marked Color"] : currentColors["Unmarked Color"]);
                        if (bingoLogic)
                        {
                            BingoCheck(i, j);
                            UpdateBingoCells();
                        }
                    }

                    // update what the original colors on the grid are
                    originalColors[i, j] = ((SolidColorBrush)buttons[i, j].Background).Color;
                }
            }
            // update the hint color
            foreach (string key in bossHintBorders.Keys)
            {
                if (bossHintBorders[key].Background != null)
                {
                    SetColorForButton(bossHintBorders[key].Background, currentColors["Hint Color"]);
                }
            }
            sender.Hide();
        }

        private void InitOptions()
        {
            // save grid settings
            SavePreviousGridSettingsOption.IsChecked = Properties.Settings.Default.SavePreviousGridSetting;
            SavePreviousGridSettingsToggle(SavePreviousGridSettingsOption.IsChecked);

            // enable fog of war image
            FogIconOption.IsChecked = Properties.Settings.Default.FogIconSetting;
            FogIconToggle(FogIconOption.IsChecked);

            // enable televo icons
            TelevoIconsOption.IsChecked = Properties.Settings.Default.TelevoIcons;
            TelevoIconsToggle(TelevoIconsOption.IsChecked);

            // enable sonic icons
            SonicIconsOption.IsChecked = Properties.Settings.Default.SonicIcons;
            SonicIconsToggle(SonicIconsOption.IsChecked);

            // enable custom images
            CustomGridIconsOption.IsChecked = Properties.Settings.Default.GridCustomImages;
            CustomGridIconsToggle(CustomGridIconsOption.IsChecked);
        }
    
        private void UpdateGridBanner(bool showBanner, string textMain = "", string textIcon = "")
        {
            //Update Text
            BannerIconL.Text = textIcon;
            BannerIconR.Text = textIcon;
            BannerMain.Text = textMain;

            //Banner Visibility
            if (showBanner)
                GridTextHeader.Height = new GridLength(0.1, GridUnitType.Star);
            else
                GridTextHeader.Height = new GridLength(0, GridUnitType.Star);
        }  
    }
}
