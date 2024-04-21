using Microsoft.Win32;
using Microsoft.VisualBasic;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Security.Cryptography;
using System.Threading;
using System.Data.Common;
using System.Windows.Input;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Markup;

namespace KhTracker
{
    /// <summary>
    /// Interaction logic for ObjectivesWindow.xaml
    /// </summary>
    public partial class ObjectivesWindow : Window, IColorableWindow
    {
        public bool canClose = false;
        readonly Data data;
        public Grid objGrid;
        public ToggleButton[,] buttons;
        public Color[,] originalColors;
        public bool[,] annotationStatus;
        public Dictionary<string, Color> currentColors = GridWindow.GetColorSettings();
        public List<string> assets = new List<string>();
        public int numRows;
        public int numColumns;
        public ColorPickerWindow colorPickerWindow;
        private int objectivesNeed = 0;

        //lookup table for what size the grid should be for certain objective counts
        private Dictionary<int, Tuple<int,int>> objSizeLookup = new Dictionary<int, Tuple<int, int>>()
        {
            {001, new Tuple<int,int>(1, 1)},
            {002, new Tuple<int,int>(1, 2)},
            {003, new Tuple<int,int>(1, 3)},
            {004, new Tuple<int,int>(2, 2)},
            //{005, new Tuple<int,int>(1, 5)},
            {006, new Tuple<int,int>(2, 3)},
            //{007, new Tuple<int,int>(1, 7)},
            {008, new Tuple<int,int>(2, 4)},
            {009, new Tuple<int,int>(3, 3)},
            {010, new Tuple<int,int>(2, 5)},
            {012, new Tuple<int,int>(3, 4)},
            //{014, new Tuple<int,int>(2, 7)},
            {015, new Tuple<int,int>(3, 5)},
            {016, new Tuple<int,int>(4, 4)},
            {018, new Tuple<int,int>(3, 6)},
            {020, new Tuple<int,int>(4, 5)},
            {021, new Tuple<int,int>(3, 7)},
            {024, new Tuple<int,int>(4, 6)},
            {025, new Tuple<int,int>(5, 5)},
            {027, new Tuple<int,int>(3, 9)},
            {028, new Tuple<int,int>(4, 7)},
            {030, new Tuple<int,int>(5, 6)},
            {032, new Tuple<int,int>(4, 8)},
            {033, new Tuple<int,int>(3, 11)},
            {035, new Tuple<int,int>(5, 7)},
            {036, new Tuple<int,int>(6, 6)},
            {040, new Tuple<int,int>(5, 8)},
            {042, new Tuple<int,int>(6, 7)},
            {044, new Tuple<int,int>(4, 11)},
            {045, new Tuple<int,int>(5, 9)},
            {048, new Tuple<int,int>(6, 8)},
            {049, new Tuple<int,int>(7, 7)},
            {050, new Tuple<int,int>(5, 10)},
            {052, new Tuple<int,int>(4, 13)},
            {054, new Tuple<int,int>(6, 9)},
            {055, new Tuple<int,int>(5, 11)},
            {056, new Tuple<int,int>(7, 8)},
            {060, new Tuple<int,int>(6, 10)},
            {063, new Tuple<int,int>(7, 9)},
            {064, new Tuple<int,int>(8, 8)},
            {065, new Tuple<int,int>(5, 13)},
            {066, new Tuple<int,int>(6, 11)},
            {070, new Tuple<int,int>(7, 10)},
            {072, new Tuple<int,int>(8, 9)},
            {075, new Tuple<int,int>(5, 15)},
            {077, new Tuple<int,int>(7, 11)},
            {078, new Tuple<int,int>(6, 13)},
            {080, new Tuple<int,int>(8, 10)},
            {081, new Tuple<int,int>(9, 9)},
            {084, new Tuple<int,int>(7, 12)},
            {088, new Tuple<int,int>(8, 11)},
            {090, new Tuple<int,int>(9, 10)},
            {091, new Tuple<int,int>(7, 13)},
            {096, new Tuple<int,int>(8, 12)},
            {098, new Tuple<int,int>(7, 14)},
            {099, new Tuple<int,int>(9, 11)},
            {100, new Tuple<int,int>(10, 10)},
            {104, new Tuple<int,int>(8, 13)},
            {105, new Tuple<int,int>(7, 15)},
            {108, new Tuple<int,int>(9, 12)},
            {110, new Tuple<int,int>(10, 11)},
            {112, new Tuple<int,int>(8, 14)},
            {117, new Tuple<int,int>(9, 13)},
            {120, new Tuple<int,int>(10, 12)},
        };
        //lookup for what objectives use what assets
        private Dictionary<string, Tuple<string, int>> assetLookup = new Dictionary<string, Tuple<string, int>>
        {
            {"TwilightThorn", new Tuple<string, int>("Item Bonus", 33)},
            {"Axel1", new Tuple < string, int >("Item Bonus", 73)},
            {"Struggle", new Tuple<string, int>("Popup", 519)},
            {"Axel", new Tuple<string, int>("Stat Bonus", 34)},
            {"GridDataRoxas", new Tuple<string, int>("Popup", 558)},
            {"MysteriousTower", new Tuple<string, int>("Popup", 286)},
            {"Sandlot", new Tuple<string, int>("Popup", 294)},
            {"BetwixtAndBetween", new Tuple<string, int>("Item Bonus", 63)},
            {"GridDataAxel", new Tuple<string, int>("Popup", 561)},
            {"Bailey", new Tuple<string, int>("Item Bonus", 47)},
            {"HBDemyx", new Tuple<string, int>("Item and Stat Bonus", 28)},
            {"1000Heartless", new Tuple<string, int>("Item Bonus", 60)},
            {"GridSephiroth", new Tuple<string, int>("Stat Bonus", 35)},
            {"GridDataDemyx", new Tuple<string, int>("Popup", 560)},
            {"EndOfCoR", new Tuple<string, int>("Chest", 579)},
            {"Transport", new Tuple<string, int>("Stat Bonus", 72)},
            {"Mountain", new Tuple<string, int>("Popup", 495)},
            {"Cave", new Tuple<string, int>("Item Bonus", 43)},
            {"ShanYu", new Tuple<string, int>("Item and Stat Bonus", 9)},
            {"StormRider", new Tuple<string, int>("Item Bonus", 10)},
            {"GridDataXigbar", new Tuple<string, int>("Popup", 555)},
            {"Thresholder", new Tuple<string, int>("Item Bonus", 2)},
            {"Beast", new Tuple<string, int>("Stat Bonus", 12)},
            {"DarkThorn", new Tuple<string, int>("Item and Stat Bonus", 3)},
            {"Xaldin", new Tuple<string, int>("Item and Stat Bonus", 4)},
            {"GridDataXaldin", new Tuple<string, int>("Popup", 559)},
            {"Cerberus", new Tuple<string, int>("Item Bonus", 5)},
            {"Urns", new Tuple<string, int>("Item Bonus", 57)},
            {"OCPete", new Tuple<string, int>("Item Bonus", 6)},
            {"Hydra", new Tuple<string, int>("Item and Stat Bonus", 7)},
            {"AuronStatue", new Tuple<string, int>("Popup", 295)},
            {"Hades", new Tuple<string, int>("Item and Stat Bonus", 8)},
            {"CupPP", new Tuple<string, int>("Popup", 513)},
            {"CupC", new Tuple<string, int>("Popup", 515)},
            {"CupT", new Tuple<string, int>("Popup", 514)},
            {"CupGoF", new Tuple<string, int>("Popup", 516)},
            {"GridZexion", new Tuple<string, int>("Stat Bonus", 66)},
            {"GridZexionData", new Tuple<string, int>("Popup", 551)},
            {"Minnie", new Tuple<string, int>("Item and Stat Bonus", 38)},
            {"Windows", new Tuple<string, int>("Popup", 368)},
            {"BoatPete", new Tuple<string, int>("Item Bonus", 16)},
            {"DCPete", new Tuple<string, int>("Item and Stat Bonus", 17)},
            {"GridMarluxia", new Tuple<string, int>("Stat Bonus", 67)},
            {"GridMarluxiaData", new Tuple<string, int>("Popup", 553)},
            {"GridLingeringWill", new Tuple<string, int>("Stat Bonus", 70)},
            {"1Minute", new Tuple<string, int>("Popup", 329)},
            {"Medallions", new Tuple<string, int>("Item Bonus", 62)},
            {"Barrels", new Tuple<string, int>("Stat Bonus", 39)},
            {"Barbossa", new Tuple<string, int>("Item and Stat Bonus", 21)},
            {"GrimReaper1", new Tuple<string, int>("Item Bonus", 59)},
            {"GrimReaper", new Tuple<string, int>("Item Bonus", 22)},
            {"GridDataLuxord", new Tuple<string, int>("Popup", 557)},
            {"Abu", new Tuple<string, int>("Item Bonus", 42)},
            {"TreasureRoom", new Tuple<string, int>("Stat Bonus", 46)},
            {"Lords", new Tuple<string, int>("Item Bonus", 37)},
            {"GenieJafar", new Tuple<string, int>("Item Bonus", 15)},
            {"GridLexaeus", new Tuple<string, int>("Stat Bonus", 65)},
            {"GridLexaeusData", new Tuple<string, int>("Popup", 550)},
            {"PrisonKeeper", new Tuple<string, int>("Item and Stat Bonus", 18)},
            {"OogieBoogie", new Tuple<string, int>("Stat Bonus", 19)},
            {"Children", new Tuple<string, int>("Stat Bonus", 40)},
            {"ObjectivePresents1", new Tuple<string, int>("Popup", 297)},
            {"ObjectivePresents2", new Tuple<string, int>("Popup", 298)},
            {"Experiment", new Tuple<string, int>("Stat Bonus", 20)},
            {"GridVexen", new Tuple<string, int>("Stat Bonus", 64)},
            {"GridVexenData", new Tuple<string, int>("Popup", 549)},
            {"Simba", new Tuple<string, int>("Popup", 264)},
            {"Hyenas1", new Tuple<string, int>("Stat Bonus", 49)},
            {"Scar", new Tuple<string, int>("Stat Bonus", 29)},
            {"Hyenas2", new Tuple<string, int>("Stat Bonus", 50)},
            {"GroundShaker", new Tuple<string, int>("Item and Stat Bonus", 30)},
            {"GridDataSaix", new Tuple<string, int>("Popup", 556)},
            {"Screens", new Tuple<string, int>("Stat Bonus", 45)},
            {"HostileProgram", new Tuple<string, int>("Item and Stat Bonus", 31)},
            {"SolarSailer", new Tuple<string, int>("Item Bonus", 61)},
            {"MCP", new Tuple<string, int>("Item and Stat Bonus", 32)},
            {"GridLarxene", new Tuple<string, int>("Stat Bonus", 68)},
            {"GridLarxeneData", new Tuple<string, int>("Popup", 552)},
            {"Roxas", new Tuple<string, int>("Item and Stat Bonus", 69)},
            {"Xigbar", new Tuple<string, int>("Stat Bonus", 23)},
            {"Luxord", new Tuple<string, int>("Item and Stat Bonus", 24)},
            {"Saix", new Tuple<string, int>("Stat Bonus", 25)},
            {"Xemnas1", new Tuple<string, int>("Double Stat Bonus", 26)},
            {"GridDataFinalXemnas", new Tuple<string, int>("Popup", 554)},
            {"SpookyCave", new Tuple<string, int>("Popup", 284)},
            {"StarryHill", new Tuple<string, int>("Popup", 285)},
            {"Tutorial", new Tuple<string, int>("Popup", 367)},
            {"Ursula", new Tuple<string, int>("Popup", 287)},
            {"NewDay", new Tuple<string, int>("Popup", 279)},
            {"Valor3", new Tuple<string, int>("Valor Level", 3)},
            {"Valor5", new Tuple<string, int>("Valor Level", 5)},
            {"Valor7", new Tuple<string, int>("Valor Level", 7)},
            {"Wisdom3", new Tuple<string, int>("Wisdom Level", 3)},
            {"Wisdom5", new Tuple<string, int>("Wisdom Level", 5)},
            {"Wisdom7", new Tuple<string, int>("Wisdom Level", 7)},
            {"Limit3", new Tuple<string, int>("Limit Level", 3)},
            {"Limit5", new Tuple<string, int>("Limit Level", 5)},
            {"Limit7", new Tuple<string, int>("Limit Level", 7)},
            {"Master3", new Tuple<string, int>("Master Level", 3)},
            {"Master5", new Tuple<string, int>("Master Level", 5)},
            {"Master7", new Tuple<string, int>("Master Level", 7)},
            {"Final3", new Tuple<string, int>("Final Level", 3)},
            {"Final5", new Tuple<string, int>("Final Level", 5)},
            {"Final7", new Tuple<string, int>("Final Level", 7)},
            {"PuzzAwakening", new Tuple<string, int>("Creation", 0)},
            {"PuzzHeart", new Tuple<string, int>("Creation", 1)},
            {"PuzzDuality", new Tuple<string, int>("Creation", 2)},
            {"PuzzFrontier", new Tuple<string, int>("Creation", 3)},
            {"PuzzDaylight", new Tuple<string, int>("Creation", 4)},
            {"PuzzSunset", new Tuple<string, int>("Creation", 5)},
        };

        public ObjectivesWindow(Data dataIn)
        {
            InitializeComponent();
            InitOptions();

            data = dataIn;

            colorPickerWindow = new ColorPickerWindow(this, currentColors);

            //GenerateObjGrid(hints);

            Top = Properties.Settings.Default.ObjectiveWindowY;
            Left = Properties.Settings.Default.ObjectiveWindowX;

            Width = Properties.Settings.Default.ObjectiveWindowWidth;
            Height = Properties.Settings.Default.ObjectiveWindowHeight;

        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.ObjectiveWindowY = RestoreBounds.Top;
            Properties.Settings.Default.ObjectiveWindowX = RestoreBounds.Left;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Properties.Settings.Default.ObjectiveWindowWidth = RestoreBounds.Width;
            Properties.Settings.Default.ObjectiveWindowHeight = RestoreBounds.Height;
        }

        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            colorPickerWindow.Hide();
            if (!canClose)
            {
                e.Cancel = true;
            }
        }

        private void InitOptions()
        {
            //// enable televo icons
            //TelevoIconsOption.IsChecked = Properties.Settings.Default.TelevoIcons;
            //TelevoIconsToggle(TelevoIconsOption.IsChecked);
            //
            //// enable sonic icons
            //SonicIconsOption.IsChecked = Properties.Settings.Default.SonicIcons;
            //SonicIconsToggle(SonicIconsOption.IsChecked);
        }

        public void GenerateObjGrid(Dictionary<string, object> hintObject)
        {
            //reset banner visibility
            UpdateGridBanner(true, "OBJECTIVES NEEDED");

            //get total needed
            objectivesNeed = JsonSerializer.Deserialize<int>(hintObject["num_objectives_needed"].ToString());
            TotalValue.Text = objectivesNeed.ToString();
            CollectedValue.Text = "0";

            //build asset list
            assets.Clear();
            List<Dictionary<string, object>> objectives = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(hintObject["objective_locations"].ToString());
            foreach (var objective in objectives) 
            { 
                string type = objective["category"].ToString();
                int location = Int32.Parse(objective["location_id"].ToString());

                foreach (string asset in assetLookup.Keys)
                {
                    if (assets.Contains(asset))
                    {
                        continue;
                    }
                    if (assetLookup[asset].Item1 == type && assetLookup[asset].Item2 == location)
                    {
                        assets.Add(asset);
                    }
                }
            }
            //for now only use simple icons TODO: setup other styles
            string style = "Grid_Min-"; //TelevoIconsOption.IsChecked ? "Grid_Min-" : "Grid_Old-";
            assets = assets.Select(item => style + item).ToList();

            //get grid size
            int objectiveCount = assets.Count;
            int blankSquares = 0;
            while (!objSizeLookup.ContainsKey(objectiveCount+blankSquares))
            {
                blankSquares++;
            }
            numRows = objSizeLookup[objectiveCount + blankSquares].Item1;
            numColumns = objSizeLookup[objectiveCount + blankSquares].Item2;


            objGrid = new Grid();
            buttons = new ToggleButton[numRows, numColumns];
            originalColors = new Color[numRows, numColumns];
            annotationStatus = new bool[numRows, numColumns];

            // get raw check names
            //assets = Asset_Collection();
            // set the content resource reference with style
            //string style = TelevoIconsOption.IsChecked ? "Grid_Min-" : "Grid_Old-";
            //assets = assets.Select(item => style + item).ToList();
            //if custom images we need to fix the asset names
            //if (CustomGridIconsOption.IsChecked)
            //    Change_Icons();


            // generate the grid
            for (int i = 0; i < numRows; i++)
            {
                objGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            }

            for (int j = 0; j < numColumns; j++)
            {
                objGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            }

            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numColumns; j++)
                {
                    if ((i + 1) * (j + 1) > assets.Count)
                        continue;

                    ToggleButton button = new ToggleButton();
                    bool buttonContentRevealed = buttons[i, j] != null && ((buttons[i, j].IsChecked ?? false) || buttons[i, j].Content != null);

                    button.SetResourceReference(ContentProperty, assets[(i * numColumns) + j]);
                    button.Background = new SolidColorBrush(currentColors["Unmarked Color"]);
                    button.Tag = assets[(i * numColumns) + j].ToString();
                    button.Style = (Style)FindResource("ColorToggleButton");
                    // keep i and j static for the button
                    int current_i = i;
                    int current_j = j;
                    button.Click += (sender, e) => Button_Click(sender, e, current_i, current_j);
                    button.MouseRightButtonUp += (sender, e) => Button_RightClick(sender, e, current_i, current_j);
                    Grid.SetRow(button, i);
                    Grid.SetColumn(button, j);
                    buttons[i, j] = button;
                    objGrid.Children.Add(button);
                }
            }

            // Add grid to the window or other container
            DynamicGrid.Children.Add(objGrid);
        }

        public void UpdateGridBanner(bool showBanner, string textMain = "", string textIcon = "", string iconColor = "Banner_Gold")
        {
            //Update Text
            OBannerIconL.Text = textIcon;
            OBannerIconR.Text = textIcon;
            OBannerMain.Text = textMain;
            OBannerIconL.Fill = (LinearGradientBrush)FindResource(iconColor);
            OBannerIconR.Fill = (LinearGradientBrush)FindResource(iconColor);

            //Banner Visibility
            if (showBanner)
            {
                GridTextHeader.Height = new GridLength(0.1, GridUnitType.Star);
                objBannerIconL.Width = new GridLength(0.2, GridUnitType.Star);
                objBannerIconR.Width = new GridLength(2.3, GridUnitType.Star);
                CollectionGrid.Visibility = Visibility.Visible;

            }
            else
            {
                CollectionGrid.Visibility = Visibility.Collapsed;
                GridTextHeader.Height = new GridLength(100, GridUnitType.Star);
                objBannerIconL.Width = new GridLength(1, GridUnitType.Star);
                objBannerIconR.Width = new GridLength(1, GridUnitType.Star);
            }

        }

        public void Button_Click(object sender, RoutedEventArgs e, int i, int j)
        {
            var button = (ToggleButton)sender;
            annotationStatus[i, j] = false;

            if (button.IsChecked ?? false || annotationStatus[i, j])
            {
                SetColorForButton(button.Background, currentColors["Marked Color"]);
            }
            else
            {
                SetColorForButton(button.Background, currentColors["Unmarked Color"]);
            }

            checkNeeded();
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
                originalColors[i, j] = GetColorFromButton(button.Background);
                annotationStatus[i, j] = true;
                SetColorForButton(button.Background, currentColors["Annotated Color"]);
            }
        }

        public void checkNeeded()
        {
            if(objectivesNeed != 0)
            {
                List<ToggleButton> completeSquares = new List<ToggleButton>();
                foreach (var square in objGrid.Children)
                {
                    if (square is ToggleButton button)
                    {
                        if (button.IsChecked == true)
                            completeSquares.Add(button);
                    }

                }
                if (completeSquares.Count >= objectivesNeed)
                {
                    foreach (ToggleButton square in completeSquares)
                    {
                        SetColorForButton(square.Background, currentColors["Battleship Sunk Color"]);
                    }
                }
                else
                {
                    foreach (ToggleButton square in completeSquares)
                    {
                        SetColorForButton(square.Background, currentColors["Marked Color"]);
                    }
                }

                CollectedValue.Text = completeSquares.Count.ToString();
            }
        }

        public void SetColorForButton(Brush buttonBackground, Color newColor)
        {
            ((SolidColorBrush)buttonBackground).Color = newColor;
        }

        public Color GetColorFromButton(Brush buttonBackground)
        {
            return ((SolidColorBrush)buttonBackground).Color;
        }

        // updates colors upon close
        private void PickColor_Click(object sender, RoutedEventArgs e)
        {
            // prompt user for new colors
            colorPickerWindow.Show();
        }

        public void HandleClosing(ColorPickerWindow sender)
        {
            //update the new colors on the card
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numColumns; j++)
                {
                    // ensure the cell in the objectives grid has content
                    if (i * numColumns + j < assets.Count())
                    {
                        if (annotationStatus[i, j])
                            SetColorForButton(buttons[i, j].Background, currentColors["Annotated Color"]);
                        else
                        {
                            SetColorForButton(buttons[i, j].Background, (bool)buttons[i, j].IsChecked ? currentColors["Marked Color"] : currentColors["Unmarked Color"]);
                        }
                    }
                }
            }
            sender.Hide();
        }
    }
}
