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

namespace KhTracker
{
    /// <summary>
    /// Interaction logic for GridWindow.xaml
    /// </summary>
    public partial class GridWindow : Window
    {
        public bool canClose = false;
        //Dictionary<string, int> worlds = new Dictionary<string, int>();
        //Dictionary<string, int> others = new Dictionary<string, int>();
        //Dictionary<string, int> totals = new Dictionary<string, int>();
        //Dictionary<string, int> important = new Dictionary<string, int>();
        //Dictionary<string, ContentControl> Progression = new Dictionary<string, ContentControl>();
        Data data;
        public GridOptionsWindow gridOptionsWindow;
        public ColorPickerWindow colorPickerWindow;

        public int numRows;
        public int numColumns;
        public string seedName;
        public bool bingoLogic;
        public bool battleshipLogic;

        public Grid grid;
        public ToggleButton[,] buttons;
        public Dictionary<string, bool> gridSettings = new Dictionary<string, bool>();
        public Dictionary<string, Color> currentColors = new Dictionary<string, Color>();
        public Dictionary<string, ContentControl> bossHintContentControls = new Dictionary<string, ContentControl>();
        public Dictionary<string, Border> bossHintBorders = new Dictionary<string, Border>();
        public List<string> assets;

        public GridWindow(Data dataIn)
        {
            InitializeComponent();
            InitOptions();

            gridSettings = JsonSerializer.Deserialize<Dictionary<string, bool>>(Properties.Settings.Default.GridSettings);
            currentColors = GetColorSettings();

            numRows = Properties.Settings.Default.GridWindowRows;
            numColumns = Properties.Settings.Default.GridWindowColumns;

            bingoLogic = Properties.Settings.Default.GridWindowBingoLogic;
            battleshipLogic = Properties.Settings.Default.GridWindowBattleshipLogic;

            GenerateGrid(numRows, numColumns);
            //Item.UpdateTotal += new Item.TotalHandler(UpdateTotal);

            data = dataIn;
            gridOptionsWindow = new GridOptionsWindow(this, data);
            colorPickerWindow = new ColorPickerWindow(this, currentColors);

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
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JSON Files (*.json)|*.json";
            saveFileDialog.FileName = "settings.json";
            if (saveFileDialog.ShowDialog() == true)
            {
                var combinedSettings = new
                {
                    bingoLogic = bingoLogic,
                    battleshipLogic = battleshipLogic,
                    numRows = numRows,
                    numColumns = numColumns,
                    seedName = seedName,
                    gridSettings = gridSettings
                };

                var jsonString = JsonSerializer.Serialize(combinedSettings);
                System.IO.File.WriteAllText(saveFileDialog.FileName, jsonString);
            }
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
                    var jsonString = System.IO.File.ReadAllText(openFileDialog.FileName);

                    using (JsonDocument doc = JsonDocument.Parse(jsonString))
                    {

                        var root = doc.RootElement;
                        bingoLogic = root.GetProperty("bingoLogic").GetBoolean();
                        battleshipLogic = root.GetProperty("battleshipLogic").GetBoolean();
                        numRows = root.GetProperty("numRows").GetInt32();
                        numColumns = root.GetProperty("numColumns").GetInt32();
                        seedName = root.GetProperty("seedName").GetString();
                        gridSettings = JsonSerializer.Deserialize<Dictionary<string, bool>>(root.GetProperty("gridSettings").GetRawText());
                    }

                    if (SavePreviousGridSettingsOption.IsChecked) {
                        Properties.Settings.Default.GridWindowRows = numRows;
                        Properties.Settings.Default.GridWindowColumns = numColumns;
                        Properties.Settings.Default.GridSettings = JsonSerializer.Serialize<Dictionary<string, bool>>(gridSettings);
                        Properties.Settings.Default.GridWindowBingoLogic = bingoLogic;
                        Properties.Settings.Default.GridWindowBattleshipLogic = battleshipLogic;
                    }

                    // update number of reports
                    int numReports = 0;
                    for (int i = 1; i <= 13; i++)
                    {
                        if (gridSettings[$"Report{i}"])
                            numReports++;
                    }
                    Properties.Settings.Default.GridWindowNumReports = numReports;

                    // update number of unlocks
                    var unlockNames = new[] { "AladdinWep", "AuronWep", "BeastWep", "IceCream", "JackWep", "MembershipCard", "MulanWep", "Picture", "SimbaWep", "SparrowWep", "TronWep" };
                    int numUnlocks = 0;
                    foreach (string unlock in unlockNames)
                    {
                        if (gridSettings[unlock])
                            numUnlocks++;
                    }
                    Properties.Settings.Default.GridWindowNumUnlocks = numUnlocks;
                }
                catch
                {
                    Console.WriteLine("FILE DID NOT READ CORRECTLY");
                    return;
                }
            }
            grid.Children.Clear();
            GenerateGrid(numRows, numColumns, seedName);
            // re-init the Grid OptionsWindow so that the properties of grid get re-defined
            gridOptionsWindow = new GridOptionsWindow(this, data);
            gridOptionsWindow.UpdateGridSettings(data);
            
        }

        private void Grid_Options(object sender, RoutedEventArgs e)
        {
       
            gridOptionsWindow.Show();

        }

        private List<string> Change_Icons(List<string> imageKeys)
        {
            if (TelevoIconsOption.IsChecked)
            {
                for (int i = 0; i < imageKeys.Count; i++)
                {
                    imageKeys[i] = imageKeys[i].Replace("Old-", "Min-");
                }
            }
            if (SonicIconsOption.IsChecked)
            {
                for (int i = 0; i < imageKeys.Count; i++)
                {
                    imageKeys[i] = imageKeys[i].Replace("Min-", "Old-");
                }
            }
            return imageKeys;
        }

        private List<string> Asset_Collection(int seed = 1)
        {

            List<ResourceDictionary> itemsDictionaries = new List<ResourceDictionary>();

            var trackableChecksDict = new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/ItemDictionary.xaml")
            };
            itemsDictionaries.Add(trackableChecksDict);

            var trackableProgressionDict = new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/ProgressionDictionary.xaml")
            };
            itemsDictionaries.Add(trackableProgressionDict);

            var trackableItemsDict = new Dictionary<object, object>();

            foreach (ResourceDictionary rd in itemsDictionaries)
            {
                foreach (DictionaryEntry entry in rd)
                {
                    if (entry.Value is GridLabelledImage img && img.GridAllowed)
                    {
                        // regardless of image style, the image ID should be consistent so we just gather all of them from Min
                        if (((string)entry.Key).Split('-')[0] == "Min")
                        {
                            // add the item to the grid settings dictionary if it doesn't exist already (IN ACCORDANCE WITH USER SETTINGS)
                            string checkName = ((string)entry.Key).Split('-')[1];
                            gridSettings[checkName] = gridSettings.ContainsKey(checkName) ? gridSettings[checkName] : img.GridAllowed;
                            if (gridSettings[checkName])
                                trackableItemsDict[checkName] = entry.Value;
                        }
                    }

                }
            }

            // RE-randomize which reports get included
            var numReports = Properties.Settings.Default.GridWindowNumReports;
            var randomReports = Enumerable.Range(1, 13).OrderBy(g => Guid.NewGuid()).Take(numReports).ToList();
            foreach (int reportNum in Enumerable.Range(1, 13).ToList())
                gridSettings[$"Report{reportNum}"] = randomReports.Contains(reportNum) ? true : false;

            // RE-randomize which visit unlocks get included
            var unlockNames = new[] { "AladdinWep", "AuronWep", "BeastWep", "IceCream", "JackWep", "MembershipCard", "MulanWep", "Picture", "SimbaWep", "SparrowWep", "TronWep" };
            int numUnlocks = Properties.Settings.Default.GridWindowNumUnlocks;
            var randomUnlocks = Enumerable.Range(1, unlockNames.Length).OrderBy(g => Guid.NewGuid()).Take(numUnlocks).ToList();
            foreach (int i in Enumerable.Range(1, unlockNames.Length).ToList())
                gridSettings[unlockNames[i - 1]] = randomUnlocks.Contains(i) ? true : false;

            Random rng = new Random(seed);
            var randomizedItemsDict = trackableItemsDict.OrderBy(x => rng.Next()).ToDictionary(x => x.Key, x => x.Value);

            List<string> imageKeys = new List<string>();

            foreach (KeyValuePair<object, object> kvp in randomizedItemsDict)
            {
                imageKeys.Add((string)kvp.Key);
            }

            return imageKeys;
        }

        private Dictionary<string, Color> GetColorSettings()
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
            if (currentColors.ContainsKey("Original Color") && GetColorFromButton(button.Background) == currentColors["Annotated Color"])
                SetColorForButton(button.Background, currentColors["Original Color"]);
            if (GetColorFromButton(button.Background) == currentColors["Unmarked Color"] || GetColorFromButton(button.Background) == currentColors["Annotated Color"])
            {
                SetColorForButton(button.Background, currentColors["Marked Color"]);
            }
            else
            {
                SetColorForButton(button.Background, currentColors["Unmarked Color"]);
            }
            if (bingoLogic)
                BingoCheck(grid, i, j);
        }

        public void Button_RightClick(object sender, RoutedEventArgs e)
        {
            var button = (ToggleButton)sender;
            if (GetColorFromButton(button.Background) == currentColors["Annotated Color"])
            {
                SetColorForButton(button.Background, currentColors["Original Color"]);
            }
            else
            {
                currentColors["Original Color"] = GetColorFromButton(button.Background);
                SetColorForButton(button.Background, currentColors["Annotated Color"]);
            }
        }


        public void GenerateGrid(object sender, RoutedEventArgs e)
        {
            grid.Children.Clear();
            GenerateGrid(numRows, numColumns);
        }

        public void GenerateGrid(int rows = 5, int columns = 5, string seedString = null, bool iconChange = false)
        {
            int seed;
            grid = new Grid();
            buttons = new ToggleButton[rows, columns];
            var randValue = new Random();
            string alphanumeric = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            seedName = seedString;

            if (seedName == null && (data?.convertedSeedHash ?? -1) > 0 && data.firstGridOnSeedLoad)
            {
                seedName = "[TIED TO SEED]";
                seed = data.convertedSeedHash;
                data.firstGridOnSeedLoad = false;
            }
            else 
            {
                if (seedName == null)
                    seedName = new string(Enumerable.Range(0, 8).Select(_ => alphanumeric[randValue.Next(alphanumeric.Length)]).ToArray());
                seed = seedName.GetHashCode();
            }
            Seedname.Header = "Seed: " + seedName;

            if (iconChange)
                // switch image style
                assets = Change_Icons(assets);
            else
            {
                // get raw check names
                assets = Asset_Collection(seed);
                // set the content resource reference with style
                string style = TelevoIconsOption.IsChecked ? "Min-" : "Old-";
                assets = assets.Select(item => style + item).ToList();
            }



            if (rows * columns <= 0)
            {
                numRows = 5;
                numColumns = 5;
            }

            // if there aren't enough assets to fit the grid, get the grid closest to the user input that can contain all assets
            int numGlobalSettings = gridSettings.Keys.Count(k => k.StartsWith("Global"));
            int numChecks = assets.Count - numGlobalSettings;
            int originalNumRows = rows;
            int originalNumColumns = columns;
            if (rows * columns > numChecks)
            {
                while (true)
                {
                    int currentMax = Math.Max(rows, columns);
                    if (currentMax == rows)
                        rows -= 1;
                    else
                        columns -= 1;
                    if (!(rows * columns > numChecks))
                    {
                        numRows = rows;
                        numColumns = columns;
                        MessageBox.Show($"NOTE: Your original request for a grid of size {originalNumRows} x {originalNumColumns} is not possible with only {numChecks} allowed checks. Grid has been reduced to size of {numRows} x {numColumns}");
                        break;
                    }
                }
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
                    button.SetResourceReference(ContentProperty, assets[(i * numColumns) + j]);
                    button.Background = new SolidColorBrush(currentColors["Unmarked Color"]);
                    button.Tag = assets[(i * numColumns) + j].ToString();
                    Console.WriteLine(button.Tag);
                    button.Style = (Style)FindResource("ColorToggleButton");
                    // keep i and j static for the button
                    int current_i = i;
                    int current_j = j;
                    button.Click += (sender, e) => Button_Click(sender, e, current_i, current_j);
                    button.MouseRightButtonUp += Button_RightClick;
                    Grid.SetRow(button, i);
                    Grid.SetColumn(button, j);
                    buttons[i, j] = button;
                    grid.Children.Add(button);
                }
            }

            // generate the boss hints
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

        public void BingoCheck(Grid grid, int i, int j)
        {

            int rowCount = grid.RowDefinitions.Count;
            int columnCount = grid.ColumnDefinitions.Count;


            // remove any bingos if we are unclicking
            if (buttons[i, j].IsChecked == false)
            {
                // check if we can have diagonal bingos
                if (rowCount == columnCount)
                {
                    // remove left diagonal
                    if (i == j)
                    {
                        for (int index = 0; index < rowCount; index++)
                        {
                            if (GetColorFromButton(buttons[index, index].Background).Equals(currentColors["Bingo Color"]))
                            {
                                // check that the button in question is not a part of a row or column bingo before removing bingo background
                                bool part_of_row_bingo = true;
                                bool part_of_column_bingo = true;
                                for (int check = 0; check < rowCount; check++)
                                {
                                    if (!GetColorFromButton(buttons[index, check].Background).Equals(currentColors["Bingo Color"]))
                                        part_of_row_bingo = false;
                                    if (!GetColorFromButton(buttons[check, index].Background).Equals(currentColors["Bingo Color"]))
                                        part_of_column_bingo = false;
                                    if (!part_of_row_bingo && !part_of_column_bingo)
                                    {
                                        if (index != i)
                                        {
                                            // check that the middle button (if it exists) is not part of the other diagonal bingo
                                            if (index * 2 == rowCount - 1)
                                            {
                                                for (int diag_check = 0; diag_check < rowCount; diag_check++)
                                                {
                                                    if (!GetColorFromButton(buttons[diag_check, rowCount - 1 - diag_check].Background).Equals(currentColors["Bingo Color"]))
                                                    {
                                                        SetColorForButton(buttons[index, index].Background, currentColors["Marked Color"]);
                                                        break;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                SetColorForButton(buttons[index, index].Background, currentColors["Marked Color"]);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // remove right diagonal
                    if (i == rowCount - 1 - j)
                    {
                        for (int index = 0; index < rowCount; index++)
                        {
                            
                            if (GetColorFromButton(buttons[index, rowCount - 1 - index].Background).Equals(currentColors["Bingo Color"]))
                            {
                                // check that the button in question is not a part of a row or column bingo before removing bingo background
                                bool part_of_row_bingo = true;
                                bool part_of_column_bingo = true;
                                for (int check = 0; check < rowCount; check++)
                                {
                                    if (!GetColorFromButton(buttons[index, check].Background).Equals(currentColors["Bingo Color"]))
                                        part_of_row_bingo = false;
                                    if (!GetColorFromButton(buttons[check, rowCount - 1 - index].Background).Equals(currentColors["Bingo Color"]))
                                        part_of_column_bingo = false;
                                    if (!part_of_row_bingo && !part_of_column_bingo)
                                    {
                                        if (index != i)
                                        {
                                            // check that the middle button (if it exists) is not part of the other diagonal bingo
                                            if (index * 2 == rowCount - 1)
                                            {
                                                for (int diag_check = 0; diag_check < rowCount; diag_check++)
                                                {
                                                    if (!GetColorFromButton(buttons[diag_check, diag_check].Background).Equals(currentColors["Bingo Color"]))
                                                    {
                                                        SetColorForButton(buttons[index, index].Background, currentColors["Marked Color"]);
                                                        break;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                SetColorForButton(buttons[index, rowCount - 1 - index].Background, currentColors["Marked Color"]);
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
                for (int row = 0; row < rowCount; row++)
                {
                    bool part_of_row_bingo = true;
                    bool part_of_left_diag_bingo = true;
                    bool part_of_right_diag_bingo = true;

                    if (GetColorFromButton(buttons[row, j].Background).Equals(currentColors["Bingo Color"]))
                    {
                        // check that the button in question is not a part of a row bingo before removing bingo background 
                        for (int col_check = 0; col_check < columnCount; col_check++)
                        {
                            if (row != i)
                            {
                                
                                if (!GetColorFromButton(buttons[row, col_check].Background).Equals(currentColors["Bingo Color"]))
                                {
                                    part_of_row_bingo = false;
                                    break;
                                }
                            }
                        }
                        if (rowCount == columnCount)
                        {
                            // check that the button in question is not a part of left diagonal bingo before removing bingo background 
                            if (j == row)
                            {
                                for (int left_diag_check = 0; left_diag_check < rowCount; left_diag_check++)
                                {
                                    if (!GetColorFromButton(buttons[left_diag_check, left_diag_check].Background).Equals(currentColors["Bingo Color"]))
                                    {
                                        part_of_left_diag_bingo = false;
                                        break;
                                    }
                                }
                            }
                            else
                                part_of_left_diag_bingo = false;
                            // check that the button in question is not a part of right diagonal bingo before removing bingo background
                            if (j == rowCount - 1 - row)
                            {
                                for (int right_diag_check = 0; right_diag_check < rowCount; right_diag_check++)
                                {
                                    if (!GetColorFromButton(buttons[right_diag_check, rowCount - 1 - right_diag_check].Background).Equals(currentColors["Bingo Color"]))
                                    {
                                        part_of_right_diag_bingo = false;
                                        break;
                                    }
                                }
                            }
                            else
                                part_of_right_diag_bingo = false;
                        }
                        else
                        {
                            part_of_left_diag_bingo = false;
                            part_of_right_diag_bingo = false;
                        }
                        if (!part_of_row_bingo && !part_of_left_diag_bingo && !part_of_right_diag_bingo)
                            SetColorForButton(buttons[row, j].Background, currentColors["Marked Color"]);
                    }
                }

                // remove horizontal bingo
                for (int col = 0; col < columnCount; col++)
                {
                    bool part_of_column_bingo = true;
                    bool part_of_left_diag_bingo = true;
                    bool part_of_right_diag_bingo = true;
                    
                    if (GetColorFromButton(buttons[i, col].Background).Equals(currentColors["Bingo Color"]))
                    {
                        // check that the button in question is not a part of a col bingo before removing bingo background 
                        for (int row_check = 0; row_check < rowCount; row_check++)
                        {
                            if (col != j)
                            { 
                                if (!GetColorFromButton(buttons[row_check, col].Background).Equals(currentColors["Bingo Color"]))
                                {
                                    part_of_column_bingo = false;
                                    break;
                                }
                            }
                        }
                        if (rowCount == columnCount)
                        {
                            // check that the button in question is not a part of left diagonal bingo before removing bingo background 
                            if (i == col)
                            {
                                for (int left_diag_check = 0; left_diag_check < rowCount; left_diag_check++)
                                {
                                    if (!GetColorFromButton(buttons[left_diag_check, left_diag_check].Background).Equals(currentColors["Bingo Color"]))
                                    {
                                        part_of_left_diag_bingo = false;
                                        break;
                                    }
                                }
                            }
                            else
                                part_of_left_diag_bingo = false;
                            // check that the button in question is not a part of right diagonal bingo before removing bingo background
                            if (i == rowCount - 1 - col)
                            {
                                for (int right_diag_check = 0; right_diag_check < rowCount; right_diag_check++)
                                {
                                    if (!GetColorFromButton(buttons[right_diag_check, rowCount - 1 - right_diag_check].Background).Equals(currentColors["Bingo Color"]))
                                    {
                                        part_of_right_diag_bingo = false;
                                        break;
                                    }
                                }
                            }
                            else
                                part_of_right_diag_bingo = false;
                        }
                        else
                        {
                            part_of_left_diag_bingo = false;
                            part_of_right_diag_bingo = false;
                        }
                        if (!part_of_column_bingo && !part_of_left_diag_bingo && !part_of_right_diag_bingo)
                            SetColorForButton(buttons[i, col].Background, currentColors["Marked Color"]);
                    }
                }
            }

            // add any bingos if we are clicking
            else
            {
                // check if we can have diagonal bingos
                if (rowCount == columnCount)
                {
                    // add left diagonal
                    if (i == j)
                    {
                        for (int index = 0; index < rowCount; index++)
                        {
                            if (buttons[index, index].IsChecked == false)
                            {
                                break;
                            }
                            if (index == rowCount - 1)
                            {
                                for (int bingo_index = 0; bingo_index < rowCount; bingo_index++)
                                    SetColorForButton(buttons[bingo_index, bingo_index].Background, currentColors["Bingo Color"]);
                            }
                        }

                    }

                    // add right diagonal
                    if (i == rowCount - 1 - j)
                    {
                        for (int index = 0; index < rowCount; index++)
                        {
                            if (buttons[index, rowCount - 1 - index].IsChecked == false)
                            {
                                break;
                            }
                            if (index == rowCount - 1)
                            {
                                for (int bingo_index = 0; bingo_index < rowCount; bingo_index++)
                                    SetColorForButton(buttons[bingo_index, rowCount - 1 - bingo_index].Background, currentColors["Bingo Color"]);
                            }
                        }

                    }
                }
                // add vertical bingo
                for (int row = 0; row < rowCount; row++)
                {
                    if (buttons[row, j].IsChecked == false)
                    {
                        break;
                    }
                    if (row == rowCount - 1)
                    {
                        for (int bingo_row = 0; bingo_row < rowCount; bingo_row++)
                            SetColorForButton(buttons[bingo_row, j].Background, currentColors["Bingo Color"]);
                    }
                }

                // add horizontal bingo
                for (int col = 0; col < columnCount; col++)
                {
                    if (buttons[i, col].IsChecked == false)
                    {
                        break;
                    }
                    if (col == columnCount - 1)
                    {
                        for (int bingo_col = 0; bingo_col < columnCount; bingo_col++)
                            SetColorForButton(buttons[i, bingo_col].Background, currentColors["Bingo Color"]);
                    }
                }
            }
        }
        // updates colors upon close
        private void PickColor_Click(object sender, RoutedEventArgs e)
        {
            // prompt user for new colors
            colorPickerWindow.Show();
        }

        private void InitOptions()
        {
            // save grid settings
            SavePreviousGridSettingsOption.IsChecked = Properties.Settings.Default.SavePreviousGridSetting;
            SavePreviousGridSettingsToggle(SavePreviousGridSettingsOption.IsChecked);

            // enable televo icons
            TelevoIconsOption.IsChecked = Properties.Settings.Default.TelevoIcons;
            TelevoIconsToggle(TelevoIconsOption.IsChecked);

            // enable sonic icons
            SonicIconsOption.IsChecked = Properties.Settings.Default.SonicIcons;
            SonicIconsToggle(SonicIconsOption.IsChecked);
        }
    }
}
