using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace KhTracker
{
    /// <summary>
    /// Interaction logic for ObjectivesWindow.xaml
    /// </summary>

    public partial class ObjectivesWindow : Window, IColorableWindow
    {
        MainWindow window = (MainWindow)App.Current.MainWindow;

        public bool canClose = false;
        readonly Data data;
        public Grid objGrid;
        public ToggleButton[,] buttons;
        public Color[,] originalColors;
        public bool[,] annotationStatus;
        public Dictionary<string, Color> currentColors = GetColorSettings();
        public List<string> assets = new List<string>();
        public int numRows;
        public int numColumns;
        public ColorPickerWindow colorPickerWindow;
        private int objectivesNeed = 0;
        public int oneHourPoints = 0;
        public bool endCorChest = false;

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
            {"MysteriousTower", new Tuple<string, int>("Popup", 286)},
            {"Sandlot", new Tuple<string, int>("Popup", 294)},
            {"BetwixtAndBetween", new Tuple<string, int>("Item Bonus", 63)},
            {"Bailey", new Tuple<string, int>("Item Bonus", 47)},
            {"HBDemyx", new Tuple<string, int>("Item and Stat Bonus", 28)},
            {"1000Heartless", new Tuple<string, int>("Item Bonus", 60)},
            {"EndOfCoR", new Tuple<string, int>("Chest", 579)},
            {"Transport", new Tuple<string, int>("Stat Bonus", 72)},
            {"Mountain", new Tuple<string, int>("Popup", 495)},
            {"Cave", new Tuple<string, int>("Item Bonus", 43)},
            {"ShanYu", new Tuple<string, int>("Item and Stat Bonus", 9)},
            {"StormRider", new Tuple<string, int>("Item Bonus", 10)},
            {"Thresholder", new Tuple<string, int>("Item Bonus", 2)},
            {"Beast", new Tuple<string, int>("Stat Bonus", 12)},
            {"DarkThorn", new Tuple<string, int>("Item and Stat Bonus", 3)},
            {"Xaldin", new Tuple<string, int>("Item and Stat Bonus", 4)},
            {"Cerberus", new Tuple<string, int>("Item Bonus", 5)},
            {"Urns", new Tuple<string, int>("Item Bonus", 57)},
            {"OCPete", new Tuple<string, int>("Item Bonus", 6)},
            {"Hydra", new Tuple<string, int>("Item and Stat Bonus", 7)},
            {"AuronStatue", new Tuple<string, int>("Popup", 295)},
            {"Hades", new Tuple<string, int>("Item and Stat Bonus", 8)},
            {"Minnie", new Tuple<string, int>("Item and Stat Bonus", 38)},
            {"Windows", new Tuple<string, int>("Popup", 368)},
            {"BoatPete", new Tuple<string, int>("Item Bonus", 16)},
            {"DCPete", new Tuple<string, int>("Item and Stat Bonus", 17)},
            {"1Minute", new Tuple<string, int>("Popup", 329)},
            {"Medallions", new Tuple<string, int>("Item Bonus", 62)},
            {"Barrels", new Tuple<string, int>("Stat Bonus", 39)},
            {"Barbossa", new Tuple<string, int>("Item and Stat Bonus", 21)},
            {"GrimReaper1", new Tuple<string, int>("Item Bonus", 59)},
            {"GrimReaper", new Tuple<string, int>("Item Bonus", 22)},
            {"Abu", new Tuple<string, int>("Item Bonus", 42)},
            {"TreasureRoom", new Tuple<string, int>("Stat Bonus", 46)},
            {"Lords", new Tuple<string, int>("Item Bonus", 37)},
            {"GenieJafar", new Tuple<string, int>("Item Bonus", 15)},
            {"PrisonKeeper", new Tuple<string, int>("Item and Stat Bonus", 18)},
            {"OogieBoogie", new Tuple<string, int>("Stat Bonus", 19)},
            {"Children", new Tuple<string, int>("Stat Bonus", 40)},
            {"ObjectivePresents1", new Tuple<string, int>("Popup", 297)},
            {"ObjectivePresents2", new Tuple<string, int>("Popup", 298)},
            {"Experiment", new Tuple<string, int>("Stat Bonus", 20)},
            {"Simba", new Tuple<string, int>("Popup", 264)},
            {"Hyenas1", new Tuple<string, int>("Stat Bonus", 49)},
            {"Scar", new Tuple<string, int>("Stat Bonus", 29)},
            {"Hyenas2", new Tuple<string, int>("Stat Bonus", 50)},
            {"GroundShaker", new Tuple<string, int>("Item and Stat Bonus", 30)},
            {"Screens", new Tuple<string, int>("Stat Bonus", 45)},
            {"HostileProgram", new Tuple<string, int>("Item and Stat Bonus", 31)},
            {"SolarSailer", new Tuple<string, int>("Item Bonus", 61)},
            {"MCP", new Tuple<string, int>("Item and Stat Bonus", 32)},
            {"Roxas", new Tuple<string, int>("Item and Stat Bonus", 69)},
            {"Xigbar", new Tuple<string, int>("Stat Bonus", 23)},
            {"Luxord", new Tuple<string, int>("Item and Stat Bonus", 24)},
            {"Saix", new Tuple<string, int>("Stat Bonus", 25)},
            {"Xemnas1", new Tuple<string, int>("Double Stat Bonus", 26)},
            {"SpookyCave", new Tuple<string, int>("Popup", 284)},
            {"StarryHill", new Tuple<string, int>("Popup", 285)},
            {"Tutorial", new Tuple<string, int>("Popup", 367)},
            {"Ursula", new Tuple<string, int>("Popup", 287)},
            {"ObjectiveNewDay", new Tuple<string, int>("Popup", 279)},
            //Super bosses	
            {"DataAxel", new Tuple<string, int>("Popup", 561)},
            {"DataDemyx", new Tuple<string, int>("Popup", 560)},
            {"DataXaldin", new Tuple<string, int>("Popup", 559)},
            {"DataRoxas", new Tuple<string, int>("Popup", 558)},
            {"DataXigbar", new Tuple<string, int>("Popup", 555)},
            {"DataLuxord", new Tuple<string, int>("Popup", 557)},
            {"DataSaix", new Tuple<string, int>("Popup", 556)},
            {"DataXemnas", new Tuple<string, int>("Popup", 554)},
            {"Larxene", new Tuple<string, int>("Stat Bonus", 68)},
            {"LarxeneData", new Tuple<string, int>("Popup", 552)},
            {"Marluxia", new Tuple<string, int>("Stat Bonus", 67)},
            {"MarluxiaData", new Tuple<string, int>("Popup", 553)},
            {"Lexaeus", new Tuple<string, int>("Stat Bonus", 65)},
            {"LexaeusData", new Tuple<string, int>("Popup", 550)},
            {"Vexen", new Tuple<string, int>("Stat Bonus", 64)},
            {"VexenData", new Tuple<string, int>("Popup", 549)},
            {"Zexion", new Tuple<string, int>("Stat Bonus", 66)},
            {"ZexionData", new Tuple<string, int>("Popup", 551)},
            {"Sephiroth", new Tuple<string, int>("Stat Bonus", 35)},
            {"LingeringWill", new Tuple<string, int>("Stat Bonus", 70)},
            //forms
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
            //cups
            {"CupPP", new Tuple<string, int>("Popup", 513)},
            {"CupC", new Tuple<string, int>("Popup", 515)},
            {"CupT", new Tuple<string, int>("Popup", 514)},
            {"CupGoF", new Tuple<string, int>("Popup", 516)},
            //puzzles
            {"PuzzAwakening", new Tuple<string, int>("Creation", 0)},
            {"PuzzHeart", new Tuple<string, int>("Creation", 1)},
            {"PuzzDuality", new Tuple<string, int>("Creation", 2)},
            {"PuzzFrontier", new Tuple<string, int>("Creation", 3)},
            {"PuzzDaylight", new Tuple<string, int>("Creation", 4)},
            {"PuzzSunset", new Tuple<string, int>("Creation", 5)},
        };
        //1hour objectives
        public  Dictionary<string, int> oneHourAssets = new Dictionary<string, int>()
        {
	            {"Bailey", 10},
	            {"Corridor", 15},
	            {"1000Heartless", 30},
	            {"EndOfCoR", 30},
	            {"Transport", 60},
	            {"Missions", 10},
	            {"Mountain", 10},
	            {"Cave", 15},
	            {"StormRider", 30},
	            {"Beast", 10},
	            {"Minnie", 10},
	            {"Windows", 20},
	            {"BoatPete", 20},
	            {"Medallions", 15},
	            {"Barrels", 15},
	            {"TreasureRoom", 20},
	            {"GenieJafar", 30},
	            {"OogieBoogie", 20},
	            {"Children", 15},
	            {"ObjectivePresents1", 15},
	            {"ObjectivePresents2", 15},
	            {"Hyenas1", 10},
	            {"Hyenas2", 20},
	            {"GroundShaker", 30},
	            {"Screens", 15},
	            {"SolarSailer", 20},
	            {"MCP", 30},
	            {"Urns", 10},
	            {"AuronStatue", 20},
	            {"Valor3", 10},
	            {"Valor5", 15},
	            {"Valor7", 20},
	            {"Wisdom3", 10},
	            {"Wisdom5", 15},
	            {"Wisdom7", 20},
	            {"Limit3", 10},
	            {"Limit5", 15},
	            {"Limit7", 20},
	            {"Master3", 10},
	            {"Master5", 15},
	            {"Master7", 20},
	            {"Final3", 10},
	            {"Final5", 15},
	            {"Final7", 20},
	            {"Cerberus", 10},
	            {"OCPete", 20},
	            {"Hydra", 20},
	            {"Hades", 30},
	            {"OldPete", 10},
	            {"DCPete", 30},
	            {"Thresholder", 10},
	            {"ShadowStalker", 15},
	            {"DarkThorn", 30},
	            {"Xaldin", 30},
	            {"ShanYu", 20},
	            {"Riku", 25},
	            {"HBDemyx", 20},
	            {"Barbossa", 20},
	            {"GrimReaper1", 20},
	            {"GrimReaper", 30},
	            {"Lords", 25},
	            {"PrisonKeeper", 10},
	            {"Experiment", 30},
	            {"Scar", 20},
	            {"HostileProgram", 20},
	            {"Roxas", 15},
	            {"Xigbar", 20},
	            {"Luxord", 25},
	            {"Saix", 20},
	            {"Xemnas1", 25},
	            {"DataAxel", 40},
	            {"DataRoxas", 40},
	            {"DataDemyx", 40},
	            {"DataXigbar", 40},
	            {"DataXaldin", 40},
	            {"DataLuxord", 40},
	            {"DataSaix", 40},
	            {"DataFinalXemnas", 40},
	            {"Zexion", 30},
	            {"ZexionData", 40},
	            {"Marluxia", 30},
	            {"MarluxiaData", 40},
	            {"Lexaeus", 30},
	            {"LexaeusData", 40},
	            {"Vexen", 30},
	            {"VexenData", 40},
	            {"Larxene", 30},
	            {"LarxeneData", 40},
	            {"Sephiroth", 40},
	            {"LingeringWill", 40},
        };
        //override 1hour objectives
        public bool oneHourCustom = false;
        public Dictionary<string, int> oneHourOverrideAssets = new Dictionary<string, int>();
        public Dictionary<string, int> oneHourOverrideBonus = new Dictionary<string, int>();
        public Dictionary<string, double> oneHourOverrideMulti = new Dictionary<string, double>();


        public ObjectivesWindow(Data dataIn)
        {
            InitializeComponent();

            data = dataIn;

            InitOptions();

            colorPickerWindow = new ColorPickerWindow(this, currentColors, true);

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
            // enable televo icons
            ObjTelevoIconsOption.IsChecked = Properties.Settings.Default.ObjectiveTelevo;
            ObjTelevoIconsToggle(ObjTelevoIconsOption.IsChecked);
            
            // enable sonic icons
            ObjSonicIconsOption.IsChecked = Properties.Settings.Default.ObjectiveSonic;
            ObjSonicIconsToggle(ObjSonicIconsOption.IsChecked);

            //enable custom icons
            ObjCustomIconsOption.IsChecked = Properties.Settings.Default.ObjectiveCustom;
            ObjCustomIconsToggle(ObjCustomIconsOption.IsChecked);
        }

        public void GenerateObjGrid(Dictionary<string, object> hintObject)
        {
            //reset banner visibility
            UpdateGridBanner(true, "OBJECTIVES COMPLETED");

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
                    //skip dupes (shouldn't happen anymore, but kept just in case)
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
            //sort the assets gathered
            assets = assets.OrderBy(d => assetLookup.Keys.ToList().IndexOf(d)).ToList();

            //fix icon prefix for assets
            getAssetPrefix();

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

            // generate the grid
            for (int i = 0; i < numRows; i++)
            {
                objGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            }

            for (int j = 0; j < numColumns; j++)
            {
                objGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            }

            int buttonDone = 0;
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numColumns; j++)
                {
                    if (buttonDone >= assets.Count)
                        continue;

                    ToggleButton button = new ToggleButton();
                    bool buttonContentRevealed = buttons[i, j] != null && ((buttons[i, j].IsChecked ?? false) || buttons[i, j].Content != null);
                    button.SetResourceReference(ContentProperty, assets[(i * numColumns) + j]);
                    button.Background = new SolidColorBrush(currentColors["Uncollected Color"]);
                    button.Tag = assets[(i * numColumns) + j].ToString();
                    button.Style = (Style)FindResource("ColorToggleButton");
                    // keep i and j static for the button
                    int current_i = i;
                    int current_j = j;
                    button.Click += (sender, e) => Button_Click(sender, e, current_i, current_j);
                    button.MouseRightButtonUp += (sender, e) => Button_RightClick(sender, e, current_i, current_j);
		            button.MouseWheel += (sender, e) => Button_Scroll(sender, e, current_i, current_j);
                    Grid.SetRow(button, i);
                    Grid.SetColumn(button, j);
                    buttons[i, j] = button;
                    objGrid.Children.Add(button);

                    buttonDone++;
                }
            }
            // Add grid to the window or other container
            DynamicGrid.Children.Add(objGrid);
        }

        public void GenerateOneHourObjGrid()
        {
            //reset banner visibility
            UpdateGridBanner(true, "1HR OBJECTIVES", "1HROVERRIDE");

            //override setup
            oneHourOverrideAssets.Clear();
            oneHourOverrideBonus.Clear();
            oneHourOverrideMulti.Clear();
            //if (File.Exists("KhTrackerSettings/OneHourSettingsOverride.json"))
            //{
            //    using (var overrideFile = new StreamReader("KhTrackerSettings/OneHourSettingsOverride.json"))
            //    {
            //        var overrideObject = JsonSerializer.Deserialize<Dictionary<string, object>>(overrideFile.ReadToEnd());
            //
            //        oneHourOverrideAssets = JsonSerializer.Deserialize<Dictionary<string, int>>(overrideObject["objectivePointList"].ToString());
            //        
            //        oneHourOverrideBonus.Add("asArenaBonusPoints", Int32.Parse(overrideObject["asArenaBonusPoints"].ToString()));
            //        oneHourOverrideBonus.Add("dataArenaBonusPoints", Int32.Parse(overrideObject["dataArenaBonusPoints"].ToString()));
            //        oneHourOverrideBonus.Add("sephiArenaBonusPoints", Int32.Parse(overrideObject["sephiArenaBonusPoints"].ToString()));
            //        oneHourOverrideBonus.Add("terraArenaBonusPoints", Int32.Parse(overrideObject["terraArenaBonusPoints"].ToString()));
            //        oneHourOverrideBonus.Add("dataXemnasArenaBonusPoints", Int32.Parse(overrideObject["dataXemnasArenaBonusPoints"].ToString()));
            //
            //        oneHourOverrideBonus.Add("pirateMinuteFightBonus", Int32.Parse(overrideObject["pirateMinuteFightBonus"].ToString()));
            //        oneHourOverrideBonus.Add("missionsBonus", Int32.Parse(overrideObject["missionsBonus"].ToString()));
            //        oneHourOverrideBonus.Add("summitBonus", Int32.Parse(overrideObject["summitBonus"].ToString()));
            //        oneHourOverrideBonus.Add("throneRoomBonus", Int32.Parse(overrideObject["throneRoomBonus"].ToString()));
            //        oneHourOverrideBonus.Add("throneRoomBonusEarly", Int32.Parse(overrideObject["throneRoomBonusEarly"].ToString()));
            //
            //        oneHourOverrideBonus.Add("gridHeight", Int32.Parse(overrideObject["gridHeight"].ToString()));
            //        oneHourOverrideBonus.Add("gridWidth", Int32.Parse(overrideObject["gridWidth"].ToString()));
            //        oneHourOverrideBonus.Add("objectiveCount", Int32.Parse(overrideObject["objectiveCount"].ToString()));
            //
            //        oneHourOverrideMulti.Add("bossMultiplierAfterFullClear", Double.Parse(overrideObject["bossMultiplierAfterFullClear"].ToString()));
            //        oneHourOverrideMulti.Add("lordsArenaMultiplier", Double.Parse(overrideObject["lordsArenaMultiplier"].ToString()));
            //
            //        if(overrideObject.ContainsKey("bossHintingHome"))
            //        {
            //            data.BossHomeHinting = overrideObject["bossHintingHome"].ToString().ToLower() == "true";
            //        }
            //
            //        overrideFile.Close();
            //    }
            //
            //    oneHourCustom = false;
            //}


            //build asset list
            assets.Clear();
            Random rng = new Random(data.convertedSeedHash);
            if (oneHourCustom)
                assets = oneHourOverrideAssets.Keys.ToList();
            else
                assets = oneHourAssets.Keys.ToList();

            #region Random Coices

            //decide which of the CO org members fights to keep (AS vs Data)
            if (rng.Next(2) == 0)
            {
                assets.Remove("Zexion");
            }
            else
            {
                assets.Remove("ZexionData");
            }
            if (rng.Next(2) == 0)
            {
                assets.Remove("Marluxia");
            }
            else
            {
                assets.Remove("MarluxiaData");
            }
            if (rng.Next(2) == 0)
            {
                assets.Remove("Lexaeus");
            }   
            else
            {
                assets.Remove("LexaeusData");
            }
            if (rng.Next(2) == 0)
            {
                assets.Remove("Vexen");
            }
            else
            {
                assets.Remove("VexenData");
            }
            if (rng.Next(2) == 0)
            {
                assets.Remove("Larxene");
            }
            else
            {
                assets.Remove("LarxeneData");
            }
            //for each form, remove two of the 3 objectives
            int valor = rng.Next(3);
            int wisdom = rng.Next(3);
            int limit = rng.Next(3);
            int master = rng.Next(3);
            int final = rng.Next(3);
            if (valor == 0)
            {
                assets.Remove("Valor5");
                assets.Remove("Valor7");
            }
            else if (valor == 1)
            {
                assets.Remove("Valor3");
                assets.Remove("Valor7");
            }
            else
            {
                assets.Remove("Valor3");
                assets.Remove("Valor5");
            }
            if (wisdom == 0)
            {
                assets.Remove("Wisdom5");
                assets.Remove("Wisdom7");
            }
            else if (wisdom == 1)
            {
                assets.Remove("Wisdom3");
                assets.Remove("Wisdom7");
            }
            else
            {
                assets.Remove("Wisdom3");
                assets.Remove("Wisdom5");
            }
            if (limit == 0)
            {
                assets.Remove("Limit5");
                assets.Remove("Limit7");
            }
            else if (limit == 1)
            {
                assets.Remove("Limit3");
                assets.Remove("Limit7");
            }
            else
            {
                assets.Remove("Limit3");
                assets.Remove("Limit5");
            }
            if (master == 0)
            {
                assets.Remove("Master5");
                assets.Remove("Master7");
            }
            else if (master == 1)
            {
                assets.Remove("Master3");
                assets.Remove("Master7");
            }
            else
            {
                assets.Remove("Master3");
                assets.Remove("Master5");
            }
            if (final == 0)
            {
                assets.Remove("Final5");
                assets.Remove("Final7");
            }
            else if (final == 1)
            {
                assets.Remove("Final3");
                assets.Remove("Final7");
            }
            else
            {
                assets.Remove("Final3");
                assets.Remove("Final5");
            }

            #endregion

            // number of objectives to use (7 is defaut)
            if (oneHourCustom)
                assets = assets.OrderBy(x => rng.Next()).Take(oneHourOverrideBonus["objectiveCount"]).ToList();
            else
                assets = assets.OrderBy(x => rng.Next()).Take(7).ToList();

            //fix icon prefix for assets
            getAssetPrefixOneHour();

            //get grid size
            if (oneHourCustom)
            {
                numRows = oneHourOverrideBonus["gridHeight"];
                numColumns = oneHourOverrideBonus["gridWidth"];

                //if these values are not set up properly for number of objectives then default to 
                //looking for grid size in size lookup table
                if (numRows * numColumns >= oneHourOverrideBonus["objectiveCount"])
                {
                    int objectiveCount = assets.Count;
                    int blankSquares = 0;
                    while (!objSizeLookup.ContainsKey(objectiveCount + blankSquares))
                    {
                        blankSquares++;
                    }
                    numRows = objSizeLookup[objectiveCount + blankSquares].Item1;
                    numColumns = objSizeLookup[objectiveCount + blankSquares].Item2;
                }
            }
            else
            {
                int objectiveCount = assets.Count;
                int blankSquares = 0;
                while (!objSizeLookup.ContainsKey(objectiveCount + blankSquares))
                {
                    blankSquares++;
                }
                numRows = objSizeLookup[objectiveCount + blankSquares].Item1;
                numColumns = objSizeLookup[objectiveCount + blankSquares].Item2;
            }

            objGrid = new Grid();
            buttons = new ToggleButton[numRows, numColumns];
            originalColors = new Color[numRows, numColumns];
            annotationStatus = new bool[numRows, numColumns];

            // generate the grid
            for (int i = 0; i < numRows; i++)
            {
                objGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            }

            for (int j = 0; j < numColumns; j++)
            {
                objGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            }

            int buttonDone = 0;
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numColumns; j++)
                {
                    if (buttonDone >= assets.Count)
                        continue;

                    Grid squareContent = (Grid)FindResource(assets[buttonDone]);
                    
                    //fix point value display for squares when using override
                    if (oneHourCustom)
                    {
                        foreach (var item in squareContent.Children)
                        {
                            if (item is Viewbox box)
                            {
                                OutlinedTextBlock textbox = (OutlinedTextBlock)box.Child;
                                textbox.Text = oneHourOverrideAssets[assets[buttonDone].Remove(0, 8)].ToString() + " Points";
                            }
                        }
                    }

                    ToggleButton button = new ToggleButton();
                    bool buttonContentRevealed = buttons[i, j] != null && ((buttons[i, j].IsChecked ?? false) || buttons[i, j].Content != null);
                    //button.SetResourceReference(ContentProperty, assets[(i * numColumns) + j]);
                    button.Content = squareContent;
                    button.Background = new SolidColorBrush(currentColors["Uncollected Color"]);
                    button.Tag = assets[(i * numColumns) + j].ToString();
                    button.Style = (Style)FindResource("ColorToggleButton");
                    // keep i and j static for the button
                    int current_i = i;
                    int current_j = j;
                    button.Click += (sender, e) => Button_Click(sender, e, current_i, current_j);
                    button.MouseRightButtonUp += (sender, e) => Button_RightClick(sender, e, current_i, current_j);
		            button.MouseWheel += (sender, e) => Button_Scroll(sender, e, current_i, current_j);
                    Grid.SetRow(button, i);
                    Grid.SetColumn(button, j);
                    buttons[i, j] = button;
                    objGrid.Children.Add(button);

                    buttonDone++;
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
                if (textIcon == "1HROVERRIDE")
                {
                    GridTextHeader.Height = new GridLength(0.15, GridUnitType.Star);
                    objBannerIconL.Width = new GridLength(0.5, GridUnitType.Star);
                    objBannerIconR.Width = new GridLength(0.5, GridUnitType.Star);
                    CollectionGrid.Visibility = Visibility.Collapsed;
                    OBannerIconL.Text = "";
                    OBannerIconR.Text = "";
                }
                else
                {
                    GridTextHeader.Height = new GridLength(0.15, GridUnitType.Star);
                    objBannerIconL.Width = new GridLength(0.2, GridUnitType.Star);
                    objBannerIconR.Width = new GridLength(2.3, GridUnitType.Star);
                    CollectionGrid.Visibility = Visibility.Visible;
                }
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
                SetColorForButton(button.Background, currentColors["Collected Color"]);
            }
            else
            {
                SetColorForButton(button.Background, currentColors["Uncollected Color"]);
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
                SetColorForButton(button.Background, currentColors["Marked Color"]);
            }
        }
	
	    public void Button_Scroll(object sender, MouseWheelEventArgs e, int i, int j)
	    {
    		var button = (ToggleButton)sender;
    		int buttonState = 0;
    		//get button status - checked, annotated, or none
    		if (annotationStatus[i, j])
    		{
        		annotationStatus[i, j] = true;
        		buttonState = -1;
    		}
    		else if (button.IsChecked ?? true)
    		{
        		annotationStatus[i, j] = false;
        		buttonState = 1;
    		}
    		else
    		{
        		annotationStatus[i, j] = false;
        		buttonState = 0;
    		}

    		//Console.WriteLine(buttonState);
    		if (e.Delta < 0) //mouse scroll up
    		{
        		buttonState += 1;
        		if (buttonState > 1)
            		buttonState = -1;
    		}
    		else if (e.Delta > 0) //mouse scroll down
    		{
        		buttonState -= 1;
        		if (buttonState < -1)
            		buttonState = 1;
    		}
    		//Console.WriteLine(buttonState);
    		//Console.WriteLine(button.IsChecked ?? true);

    		if (buttonState == 1)
    		{
        		button.IsChecked = true;
        		annotationStatus[i, j] = false;
        		Button_RightClick(sender, e, i, j);
        		Button_Click(sender, e, i, j);
    		}
    		else if (buttonState == -1)
    		{
        		button.IsChecked = false;
        		annotationStatus[i, j] = true;
        		Button_Click(sender, e, i, j);
        		Button_RightClick(sender, e, i, j);
    		}
    		else
    		{
        		button.IsChecked = false;
        		annotationStatus[i, j] = false;
        		Button_RightClick(sender, e, i, j);
        		Button_Click(sender, e, i, j);
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
                        SetColorForButton(square.Background, currentColors["Win Condition Met Color"]);
                    }
                }
                else
                {
                    foreach (ToggleButton square in completeSquares)
                    {
                        SetColorForButton(square.Background, currentColors["Collected Color"]);
                    }
                }

                CollectedValue.Text = completeSquares.Count.ToString();
            }
            else
            {
                if (objGrid == null)
                    return;

                int testPoints = 0;
                int marksTotal = 0;
                foreach (var square in objGrid.Children)
                {
                    if (square is ToggleButton button && button.IsChecked == true)
                    {
                        if (!oneHourCustom)
                            testPoints += oneHourAssets[button.Tag.ToString().Remove(0, 8)];
                        else
                            testPoints += oneHourOverrideAssets[button.Tag.ToString().Remove(0, 8)];

                        marksTotal++;
                    }
                }

                oneHourPoints = testPoints;
                window.UpdatePointScore(0);
                Console.WriteLine("writing marks to game | " + marksTotal);
                window.SetOneHourMarks(marksTotal);
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

        public static Dictionary<string, Color> GetColorSettings()
        {

            var unmarkedColor = Properties.Settings.Default.ObjUnmarkedColorButton;
            var markedColor = Properties.Settings.Default.ObjCollectedColorButton;
            var annotatedColor = Properties.Settings.Default.ObjAnnotatedColorButton;
            var bingoColor = Properties.Settings.Default.ObjCompletedColorButton;

            return new Dictionary<string, Color>()
            {
                { "Uncollected Color", Color.FromArgb(unmarkedColor.A, unmarkedColor.R, unmarkedColor.G, unmarkedColor.B) },
                { "Collected Color", Color.FromArgb(markedColor.A, markedColor.R, markedColor.G, markedColor.B) },
                { "Marked Color", Color.FromArgb(annotatedColor.A, annotatedColor.R, annotatedColor.G, annotatedColor.B) },
                { "Win Condition Met Color", Color.FromArgb(bingoColor.A, bingoColor.R, bingoColor.G, bingoColor.B) },
            };
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
                            SetColorForButton(buttons[i, j].Background, currentColors["Marked Color"]);
                        else
                        {
                            SetColorForButton(buttons[i, j].Background, (bool)buttons[i, j].IsChecked ? currentColors["Collected Color"] : currentColors["Uncollected Color"]);
                        }
                    }
                }
            }
            checkNeeded();
            sender.Hide();
        }

        //Image toggle functions
        private void getAssetPrefix()
        {
            string style = ObjTelevoIconsOption.IsChecked ? "Obj_Min-" : "Obj_Old-";

            for (int i = 0; i < assets.Count; i++)
            {
                if (ObjCustomIconsOption.IsChecked && MainWindow.CusObjImagesList.Contains("Obj_Cus-" + assets[i]))
                {
                    assets[i] = "Obj_Cus-" + assets[i];
                    continue;
                }

                assets[i] = style + assets[i];
            }
        }

        private void updateAssetPrefix(bool usedCustomToggle = false)
        {
            bool useCustom = ObjCustomIconsOption.IsChecked;

            string prefix1 = "Obj_Old-";
            string prefix2 = "Obj_Min-";
            if (ObjSonicIconsOption.IsChecked)
            {
                prefix1 = "Obj_Min-";
                prefix2 = "Obj_Old-";
            }

            for (int i = 0; i < assets.Count; i++)
            {
                //if already a custom prefix then skip
                if (useCustom && assets[i].StartsWith("Obj_Cus-"))
                    continue;

                //if custom toggle on then check for and replace normal prefix with custom one
                if (useCustom)
                {
                    string cusCheck = assets[i].Replace(prefix1, "Obj_Cus-");
                    if (usedCustomToggle)
                            cusCheck = assets[i].Replace(prefix2, "Obj_Cus-");
                    if (MainWindow.CusObjImagesList.Contains(cusCheck))
                    {
                        assets[i] = cusCheck;
                        continue;
                    }
                }

                //if custom toggle is off check if prefix was custom and fix it else replace as normal
                if (assets[i].StartsWith("Obj_Cus-"))
                    assets[i] = assets[i].Replace("Obj_Cus-", prefix2);
                else
                    assets[i] = assets[i].Replace(prefix1, prefix2);
            }
            Change_Icons();
        }

        private void Change_Icons()
        {
            if (objGrid == null)
                return;

            string prefix = "Obj_Min-";
            if (ObjSonicIconsOption.IsChecked)
                prefix = "Obj_Old-";

            foreach (var child in objGrid.Children)
            {
                //check if it's a toggle button
                if (child is ToggleButton square)
                {
                    string squareTag = square.Tag.ToString().Remove(0, 8);
                    //check 
                    if (ObjCustomIconsOption.IsChecked && assets.Contains("Obj_Cus-" + squareTag))
                    {
                        square.SetResourceReference(ContentProperty, "Obj_Cus-" + squareTag);
                        continue;
                    }

                    square.SetResourceReference(ContentProperty, prefix + squareTag);
                }
            }
        }

        private void getAssetPrefixOneHour()
        {
            string style = ObjTelevoIconsOption.IsChecked ? "1HR_Min-" : "1HR_Old-";

            for (int i = 0; i < assets.Count; i++)
            {
                //if (ObjCustomIconsOption.IsChecked && MainWindow.CusObjImagesList.Contains("1HR_Cus-" + assets[i]))
                //{
                //    assets[i] = "1HR_Cus-" + assets[i];
                //    continue;
                //}

                assets[i] = style + assets[i];
            }
        }

        private void updateAssetPrefixOneHour(bool usedCustomToggle = false)
        {
            bool useCustom = ObjCustomIconsOption.IsChecked;

            string prefix1 = "1HR_Old-";
            string prefix2 = "1HR_Min-";
            if (ObjSonicIconsOption.IsChecked)
            {
                prefix1 = "1HR_Min-";
                prefix2 = "1HR_Old-";
            }

            for (int i = 0; i < assets.Count; i++)
            {
                //if already a custom prefix then skip
                //if (useCustom && assets[i].StartsWith("1HR_Cus-"))
                //    continue;

                //if custom toggle on then check for and replace normal prefix with custom one
                //if (useCustom)
                //{
                //    string cusCheck = assets[i].Replace(prefix1, "1HR_Cus-");
                //    if (usedCustomToggle)
                //        cusCheck = assets[i].Replace(prefix2, "1HR_Cus-");
                //    if (MainWindow.CusObjImagesList.Contains(cusCheck))
                //    {
                //        assets[i] = cusCheck;
                //        continue;
                //    }
                //}

                //if custom toggle is off check if prefix was custom and fix it else replace as normal
                //if (assets[i].StartsWith("1HR_Cus-"))
                //    assets[i] = assets[i].Replace("1HR_Cus-", prefix2);
                //else
                assets[i] = assets[i].Replace(prefix1, prefix2);
            }
            Change_IconsOneHour();
        }

        private void Change_IconsOneHour()
        {
            if (objGrid == null)
                return;

            string prefix = "1HR_Min-";
            if (ObjSonicIconsOption.IsChecked)
                prefix = "1HR_Old-";

            foreach (var child in objGrid.Children)
            {
                //check if it's a toggle button
                if (child is ToggleButton square)
                {
                    string squareTag = square.Tag.ToString().Remove(0, 8);
                    Grid squareContent = (Grid)FindResource(prefix + squareTag);

                    if (oneHourCustom)
                    {
                        foreach (var item in squareContent.Children)
                        {
                            if (item is Viewbox box)
                            {
                                OutlinedTextBlock textbox = (OutlinedTextBlock)box.Child;
                                textbox.Text = oneHourOverrideAssets[squareTag].ToString() + " Points";
                            }
                        }
                    }

                    //check 
                    //if (ObjCustomIconsOption.IsChecked && assets.Contains("1HR_Cus-" + squareTag))
                    //{
                    //    square.SetResourceReference(ContentProperty, "1HR_Cus-" + squareTag);
                    //    continue;
                    //}

                    //square.SetResourceReference(ContentProperty, prefix + squareTag);
                    square.Content = squareContent;
                }
            }
        }

        private void ObjTelevoIconsToggle(object sender, RoutedEventArgs e)
        {
            ObjTelevoIconsToggle(ObjTelevoIconsOption.IsChecked);
        }
        private void ObjTelevoIconsToggle(bool toggle)
        {
            Properties.Settings.Default.ObjectiveTelevo = toggle;
            ObjTelevoIconsOption.IsChecked = toggle;
            ObjSonicIconsOption.IsChecked = !toggle;

            if (data.oneHourMode)
                updateAssetPrefixOneHour();
            else
                updateAssetPrefix();
        }

        private void ObjSonicIconsToggle(object sender, RoutedEventArgs e)
        {
            ObjSonicIconsToggle(ObjSonicIconsOption.IsChecked);
        }
        private void ObjSonicIconsToggle(bool toggle)
        {
            Properties.Settings.Default.ObjectiveSonic = toggle;
            ObjSonicIconsOption.IsChecked = toggle;
            ObjTelevoIconsOption.IsChecked = !toggle;

            if (data.oneHourMode)
                updateAssetPrefixOneHour();
            else
                updateAssetPrefix();
        }

        private void ObjCustomIconsToggle(object sender, RoutedEventArgs e)
        {
            ObjCustomIconsToggle(ObjSonicIconsOption.IsChecked);
        }

        private void ObjCustomIconsToggle(bool toggle)
        {
            Properties.Settings.Default.ObjectiveCustom = toggle;

            if (data.oneHourMode)
                updateAssetPrefixOneHour(true);
            else
                updateAssetPrefix(true);
        }
    }
}
