using Microsoft.Win32;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace KhTracker
{
    /// <summary>
    /// Interaction logic for GridWindow.xaml
    /// </summary>
    public partial class GridWindow : Window
    {
        public bool canClose = false;
        Dictionary<string, int> worlds = new Dictionary<string, int>();
        Dictionary<string, int> others = new Dictionary<string, int>();
        Dictionary<string, int> totals = new Dictionary<string, int>();
        Dictionary<string, int> important = new Dictionary<string, int>();
        Dictionary<string, ContentControl> Progression = new Dictionary<string, ContentControl>();
        Data data;

        public int numRows = 1;
        public int numColumns = 1;

        public Grid grid;
        public ToggleButton[,] buttons;
        public Dictionary<string, bool> gridSettings = new Dictionary<string, bool>();
        public Dictionary<string, Color> currentColors = new Dictionary<string, Color>
        {
            { "Unmarked Color", Colors.DimGray },
            { "Marked Color", Colors.Green },
            { "Annotated Color", Colors.Orange },
            { "Bingo Color", Colors.Purple }
        };


        public GridWindow(Data dataIn)
        {
            InitializeComponent();
            
            GenerateGrid(numRows, numColumns);
            //Item.UpdateTotal += new Item.TotalHandler(UpdateTotal);

            data = dataIn;

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

        void GridWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            if (!canClose)
            {
                e.Cancel = true;
            }
        }

        private void Grid_Options(object sender, RoutedEventArgs e)
        {
            GridOptionsWindow gridOptionsWindow = new GridOptionsWindow(this, data);
            gridOptionsWindow.ShowDialog();
        }

        private List<string> Asset_Collection(string visual_type = "Min", int seed = 1)
        {

            gridSettings = GetGridSettings();

            List<ResourceDictionary> itemsDictionaries = new List<ResourceDictionary> ();

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
                        // the split here addresses the image type e.g. min-valor will give me min
                        if (((string)entry.Key).Split('-')[0] == visual_type)
                        {
                            // add the item to the grid settings dictionary if it doesn't exist already (IN ACCORDANCE WITH USER SETTINGS)
                            string checkName = ((string)entry.Key).Split('-')[1];
                            gridSettings[checkName] = gridSettings.ContainsKey(checkName) ? gridSettings[checkName] : img.GridAllowed;
                            if (gridSettings[checkName])
                                trackableItemsDict[entry.Key] = entry.Value;
                        }
                    }

                }
            }

            Random rng = new Random(seed);
            var randomizedItemsDict = trackableItemsDict.OrderBy(x => rng.Next()).ToDictionary(x => x.Key, x => x.Value);

            List<string> imageKeys = new List<string>();

            foreach (KeyValuePair<object, object> kvp in randomizedItemsDict)
            {
                imageKeys.Add((string)kvp.Key);
            }

            return imageKeys;
        }

        private Dictionary<string, bool> GetGridSettings()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\GridTracker");
            if (key != null)
            {
                string serializedSettings = (string)key.GetValue("Settings");
                key.Close();

                if (!string.IsNullOrEmpty(serializedSettings))
                {
                    return JsonSerializer.Deserialize<Dictionary<string, bool>>(serializedSettings);
                }
            }

            return new Dictionary<string, bool>(); // Return default or empty settings if not found
        }

        public void Button_Click(object sender, RoutedEventArgs e, int i, int j)
        {
            var button = (ToggleButton)sender;
            if (((SolidColorBrush)button.Background).Color == currentColors["Unmarked Color"] || ((SolidColorBrush)button.Background).Color == currentColors["Annotated Color"])
            {
                ((SolidColorBrush)button.Background).Color = currentColors["Marked Color"];
            }
            else
            {
                ((SolidColorBrush)button.Background).Color = currentColors["Unmarked Color"];
            }
            if (gridSettings.ContainsKey("BingoLogic") && gridSettings["BingoLogic"])
                BingoCheck(grid, i, j);
        }

        public void Button_RightClick(object sender, RoutedEventArgs e)
        {
            var button = (ToggleButton)sender;
            if (((SolidColorBrush)button.Background).Color == currentColors["Annotated Color"])
            {
                ((SolidColorBrush)button.Background).Color = currentColors["Original Color"];
            }
            else
            {
                currentColors["Original Color"] = ((SolidColorBrush)button.Background).Color;
                ((SolidColorBrush)button.Background).Color = currentColors["Annotated Color"];
            }
        }

        public void GenerateGrid(object sender, RoutedEventArgs e)
        {
            GenerateGrid(numRows, numColumns);
        }
            
        public void GenerateGrid(int numRows = 5, int numColumns = 5)
        {
            grid = new Grid();
            buttons = new ToggleButton[numRows, numColumns];
            var randValue = new Random();
            string alphanumeric = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            string seedString = new string(Enumerable.Range(0, 8).Select(_ => alphanumeric[randValue.Next(alphanumeric.Length)]).ToArray());
            int seed = seedString.GetHashCode();
            Random rand = new Random(seed);
            Seedname.Header = "Seed: " + seedString;
            List<string> assets = Asset_Collection("Min", seed);

            //Console.WriteLine(assets[0]);

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
                    button.Background = new SolidColorBrush(currentColors["Unmarked Color"]);
                    button.SetResourceReference(ContentProperty, assets[(i * numColumns) + j]);
                    button.Tag = assets[(i * numColumns) + j].ToString();
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
            // Add grid to the window or other container
            DynamicGrid.Children.Add(grid);
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
                            if (((SolidColorBrush)buttons[index, index].Background).Color.Equals(currentColors["Bingo Color"]))
                            {
                                // check that the button in question is not a part of a row or column bingo before removing bingo background
                                bool part_of_row_bingo = true;
                                bool part_of_column_bingo = true;
                                for (int check = 0; check < rowCount; check++)
                                {
                                    if (!((SolidColorBrush)buttons[index, check].Background).Color.Equals(currentColors["Bingo Color"]))
                                        part_of_row_bingo = false;
                                    if (!((SolidColorBrush)buttons[check, index].Background).Color.Equals(currentColors["Bingo Color"]))
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
                                                    if (!((SolidColorBrush)buttons[diag_check, rowCount - 1 - diag_check].Background).Color.Equals(currentColors["Bingo Color"]))
                                                    {
                                                        buttons[index, index].Background = new SolidColorBrush(currentColors["Marked Color"]);
                                                        break;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                buttons[index, index].Background = new SolidColorBrush(currentColors["Marked Color"]);
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
                            if (((SolidColorBrush)buttons[index, rowCount - 1 - index].Background).Color.Equals(currentColors["Bingo Color"]))
                            {
                                // check that the button in question is not a part of a row or column bingo before removing bingo background
                                bool part_of_row_bingo = true;
                                bool part_of_column_bingo = true;
                                for (int check = 0; check < rowCount; check++)
                                {
                                    if (!((SolidColorBrush)buttons[index, check].Background).Color.Equals(currentColors["Bingo Color"]))
                                        part_of_row_bingo = false;
                                    if (!((SolidColorBrush)buttons[check, rowCount - 1 - index].Background).Color.Equals(currentColors["Bingo Color"]))
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
                                                    if (!((SolidColorBrush)buttons[diag_check, diag_check].Background).Color.Equals(currentColors["Bingo Color"]))
                                                    {
                                                        buttons[index, index].Background = new SolidColorBrush(currentColors["Marked Color"]);
                                                        break;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                buttons[index, rowCount - 1 - index].Background = new SolidColorBrush(currentColors["Marked Color"]);
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
                    
                    if (((SolidColorBrush)buttons[row, j].Background).Color.Equals(currentColors["Bingo Color"]))
                    {
                        // check that the button in question is not a part of a row bingo before removing bingo background 
                        for (int col_check = 0; col_check < columnCount; col_check++)
                        {
                            if (row != i)
                            {
                                if (!((SolidColorBrush)buttons[row, col_check].Background).Color.Equals(currentColors["Bingo Color"])) 
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
                                    if (!((SolidColorBrush)buttons[left_diag_check, left_diag_check].Background).Color.Equals(currentColors["Bingo Color"]))
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
                                    if (!((SolidColorBrush)buttons[right_diag_check, rowCount - 1 - right_diag_check].Background).Color.Equals(currentColors["Bingo Color"]))
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
                            buttons[row, j].Background = new SolidColorBrush(currentColors["Marked Color"]);
                    }
                }

                // remove horizontal bingo
                for (int col = 0; col < columnCount; col++)
                {
                    bool part_of_column_bingo = true;
                    bool part_of_left_diag_bingo = true;
                    bool part_of_right_diag_bingo = true;
                    
                    if (((SolidColorBrush)buttons[i, col].Background).Color.Equals(currentColors["Bingo Color"]))
                    {
                        // check that the button in question is not a part of a col bingo before removing bingo background 
                        for (int row_check = 0; row_check < rowCount; row_check++)
                        {
                            if (col != j)
                            {                                
                                if (!((SolidColorBrush)buttons[row_check, col].Background).Color.Equals(currentColors["Bingo Color"]))
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
                                    if (!((SolidColorBrush)buttons[left_diag_check, left_diag_check].Background).Color.Equals(currentColors["Bingo Color"]))
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
                                    if (!((SolidColorBrush)buttons[right_diag_check, rowCount - 1 - right_diag_check].Background).Color.Equals(currentColors["Bingo Color"]))
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
                            buttons[i, col].Background = new SolidColorBrush(currentColors["Marked Color"]);
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
                                    buttons[bingo_index, bingo_index].Background = new SolidColorBrush(currentColors["Bingo Color"]);
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
                                    buttons[bingo_index, rowCount - 1 - bingo_index].Background = new SolidColorBrush(currentColors["Bingo Color"]);
                            }
                        }

                    }
                }
                // add vertical bingo
                for (int row = 0; row < rowCount; row++)
                {
                    if (buttons[row, j].IsChecked == false)
                    {
                        break ;         
                    }
                    if (row == rowCount - 1)
                    {
                        for (int bingo_row = 0; bingo_row < rowCount; bingo_row++)
                            buttons[bingo_row, j].Background = new SolidColorBrush(currentColors["Bingo Color"]);
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
                            buttons[i, bingo_col].Background = new SolidColorBrush(currentColors["Bingo Color"]);
                    }
                }
            }
        }
        private void PickColor_Click(object sender, RoutedEventArgs e)
        {
            // prompt user for new colors
            var colorPicker = new ColorPickerWindow(currentColors);
            var oldAnnotatedColor = currentColors["Annotated Color"];
            if (colorPicker.ShowDialog() == true)
            {
                currentColors = colorPicker.ButtonColors;
            }
            //update the new colors on the card
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numColumns; j++)
                {
                    if (((SolidColorBrush)buttons[i, j].Background).Color.Equals(oldAnnotatedColor))
                        buttons[i, j].Background = new SolidColorBrush(currentColors["Annotated Color"]);
                    else
                        buttons[i, j].Background = (bool)buttons[i, j].IsChecked ? new SolidColorBrush(currentColors["Marked Color"]) : new SolidColorBrush(currentColors["Unmarked Color"]);
                    if (gridSettings.ContainsKey("BingoLogic") && gridSettings["BingoLogic"])
                        BingoCheck(grid, i, j); 
                }
            }
        }
    }
}
