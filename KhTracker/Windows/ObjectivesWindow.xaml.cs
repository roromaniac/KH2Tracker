using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace KhTracker
{
    /// <summary>
    /// Interaction logic for ObjectivesWindow.xaml
    /// </summary>
    /// 
    public class CustomObjectiveSettings
    {
        // darts mode is default since 1 hour has more points customization involved
        public Dictionary<string, int> objectivePointList { get; set; }
        public int objectiveCount { get; set; } = 25;
        public int gridHeight { get; set; } = 5;
        public int gridWidth { get; set; } = 5;
        public int asArenaBonus { get; set; } = 0;
        public int dataArenaBonus { get; set; } = 0;
        public int sephiArenaBonus { get; set; } = 0;
        public int terraArenaBonus { get; set; } = 0;
        public int dataXemnasArenaBonus { get; set; } = 0;
        public int pirateMinuteFightBonus { get; set; } = 0;
        public int missionsBonus { get; set; } = 0;
        public int summitBonus { get; set; } = 0;
        public int throneRoomBonus { get; set; } = 0;
        public int throneRoomBonusEarly { get; set; } = 0;
        public double bcDoubleFightMultiplier { get; set; } = 0.0;
        public double lordsArenaMultiplier { get; set; } = 0.0;
        public int maxFormObjectivesPerForm { get; set; } = 1;
        public int dataAndASCanBeObjectives { get; set; } = 0;
        public int objectivesToWin { get; set; } = 0;
        public int pointsToWin { get; set; } = 66;
        public Dictionary<string, string> oneHourReplacements { get; set; } = new Dictionary<string, string>();
    }

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
        public int objectivesNeed = 0;
        public int cgmPoints = 0;
        public bool endCorChest = false;

        //lookup table for what size the grid should be for certain objective counts
        private Dictionary<int, Tuple<int, int>> objSizeLookup = new Dictionary<int, Tuple<int, int>>()
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
            {"DataRoxas", new Tuple<string, int>("Popup", 558)},
            {"DataAxel", new Tuple<string, int>("Popup", 561)},
            {"DataDemyx", new Tuple<string, int>("Popup", 560)},
            {"Sephiroth", new Tuple<string, int>("Stat Bonus", 35)},
            {"DataXigbar", new Tuple<string, int>("Popup", 555)},
            {"DataXaldin", new Tuple<string, int>("Popup", 559)},
            {"Zexion", new Tuple<string, int>("Stat Bonus", 66)},
            {"ZexionData", new Tuple<string, int>("Popup", 551)},
            {"Marluxia", new Tuple<string, int>("Stat Bonus", 67)},
            {"MarluxiaData", new Tuple<string, int>("Popup", 553)},
            {"LingeringWill", new Tuple<string, int>("Stat Bonus", 70)},
            {"DataLuxord", new Tuple<string, int>("Popup", 557)},
            {"Lexaeus", new Tuple<string, int>("Stat Bonus", 65)},
            {"LexaeusData", new Tuple<string, int>("Popup", 550)},
            {"Vexen", new Tuple<string, int>("Stat Bonus", 64)},
            {"VexenData", new Tuple<string, int>("Popup", 549)},
            {"DataSaix", new Tuple<string, int>("Popup", 556)},
            {"Larxene", new Tuple<string, int>("Stat Bonus", 68)},
            {"LarxeneData", new Tuple<string, int>("Popup", 552)},
            {"DataXemnas", new Tuple<string, int>("Popup", 554)},
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

        //1hour objectives and settings
        public CustomObjectiveSettings oneHourAssets = new CustomObjectiveSettings()
        {
            objectivePointList = new Dictionary<string, int>
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
                {"LingeringWill", 40}
            },
            objectiveCount = 7,
            gridHeight = 2,
            gridWidth = 4,
            asArenaBonus = 10,
            dataArenaBonus = 20,
            sephiArenaBonus = 30,
            terraArenaBonus = 50,
            dataXemnasArenaBonus = 40,
            pirateMinuteFightBonus = 10,
            missionsBonus = 0,
            summitBonus = 10,
            throneRoomBonus = 30,
            throneRoomBonusEarly = 15,
            bcDoubleFightMultiplier = 1.5,
            lordsArenaMultiplier = 1.5,
            maxFormObjectivesPerForm = 2,
            dataAndASCanBeObjectives = 0,
        };

        //darts objectives and settings
        public CustomObjectiveSettings dartsAssets = new CustomObjectiveSettings()
        {
            objectivePointList = new Dictionary<string, int>
            {
                {"Minigame", 2},
                {"TwilightThorn", 3},
                {"Axel1", 2},
                {"Struggle", 2},
                {"Axel", 5},
                {"DataRoxas", 10},
                {"Station", 2},
                {"MysteriousTower", 2},
                {"Sandlot", 3},
                {"Mansion", 4},
                {"BetwixtAndBetween", 4},
                {"DataAxel", 11},
                {"Bailey", 2},
                {"Corridor", 3},
                {"HBDemyx", 4},
                {"FinalFantasy", 4},
                {"1000Heartless", 4},
                {"Sephiroth", 10},
                {"DataDemyx", 11},
                {"Fight1", 5},
                {"Fight2", 5},
                {"Transport", 11},
                {"Thresholder", 2},
                {"Beast", 2},
                {"DarkThorn", 3},
                {"Xaldin", 5},
                {"DataXaldin", 12},
                {"Cerberus", 3},
                {"Urns", 2},
                {"OCDemyx", 3},
                {"Hydra", 4},
                {"Hades", 5},
                {"Zexion", 5},
                {"ZexionData", 10},
                {"Abu", 2},
                {"TreasureRoom", 3},
                {"Lords", 4},
                {"GenieJafar", 5},
                {"Lexaeus", 5},
                {"LexaeusData", 10},
                {"Missions", 2},
                {"Mountain", 2},
                {"Cave", 3},
                {"Summit", 3},
                {"ShanYu", 4},
                {"Riku", 4},
                {"Snipers", 5},
                {"StormRider", 5},
                {"DataXigbar", 12},
                {"Kanga", 3},
                {"SpookyCave", 5},
                {"StarryHill", 9},
                {"Simba", 2},
                {"Hyenas1", 2},
                {"Scar", 4},
                {"Hyenas2", 3},
                {"GroundShaker", 5},
                {"DataSaix", 11},
                {"Minnie", 2},
                {"OldPete", 2},
                {"Windows", 3},
                {"BoatPete", 2},
                {"DCPete", 4},
                {"Marluxia", 9},
                {"MarluxiaData", 11},
                {"LingeringWill", 15},
                {"CandyCaneLane", 2},
                {"PrisonKeeper", 4},
                {"OogieBoogie", 4},
                {"ObjectivePresents2", 3},
                {"Experiment", 5},
                {"Vexen", 5},
                {"VexenData", 11},
                {"Town", 2},
                {"1Minute", 2},
                {"Barbossa", 4},
                {"GrimReaper1", 4},
                {"Gambler", 2},
                {"GrimReaper", 5},
                {"DataLuxord", 10},
                {"Screens", 2},
                {"HostileProgram", 3},
                {"SolarSailer", 4},
                {"MCP", 5},
                {"Larxene", 5},
                {"LarxeneData", 11},
                {"Roxas", 4},
                {"Xigbar", 4},
                {"Luxord", 3},
                {"Saix", 3},
                {"Xemnas1", 5},
                {"DataFinalXemnas", 12},
                {"Valor5", 3},
                {"Valor6", 5},
                {"Valor7", 9},
                {"Wisdom5", 3},
                {"Wisdom6", 5},
                {"Wisdom7", 9},
                {"Limit5", 3},
                {"Limit6", 5},
                {"Limit7", 9},
                {"Master5", 3},
                {"Master6", 5},
                {"Master7", 9},
                {"Final5", 3},
                {"Final6", 5},
                {"Final7", 9}
            },
            objectiveCount = 25,
            gridHeight = 5,
            gridWidth = 5,
            maxFormObjectivesPerForm = 2,
            dataAndASCanBeObjectives = 0,
            pointsToWin = 66,
        };

        //overrides for custom game modes
        public Dictionary<string, int> oneHourOverrideAssets = new Dictionary<string, int>();
        public Dictionary<string, int> oneHourOverrideBonus = new Dictionary<string, int>();
        public Dictionary<string, double> oneHourMultiplicativeBonus = new Dictionary<string, double>();
        public Dictionary<string, int> oneHourObjGridSettings = new Dictionary<string, int>();
        public Dictionary<string, int> dartsOverrideAssets = new Dictionary<string, int>();
        public Dictionary<string, int> dartsObjGridSettings = new Dictionary<string, int>();

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

            ObjPointsOrderOption.IsChecked = Properties.Settings.Default.ObjectivePointsOrder;
            ObjPointsOrderToggle(ObjPointsOrderOption.IsChecked);
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
            while (!objSizeLookup.ContainsKey(objectiveCount + blankSquares))
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
                    button.Background = new SolidColorBrush(currentColors["Unmarked Color"]);
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

        public CustomObjectiveSettings GetCustomGameModeAssets()
        {
            if (data.oneHourMode)
            {
                CustomObjectiveSettings customObjectiveSettings = JsonSerializer.Deserialize<CustomObjectiveSettings>(
                    JsonSerializer.Serialize(oneHourAssets)
                );
                if (Properties.Settings.Default.Custom1HRAssets)
                {
                    string oneHourAssetPath = Properties.Settings.Default.OneHourModeAssetsFilepath;
                    string directoryPath = Path.GetDirectoryName(oneHourAssetPath);
                    if (!Directory.Exists(directoryPath) || !File.Exists(oneHourAssetPath))
                    {
                        // Display an alert box with the error message
                        System.Windows.MessageBox.Show(
                            $"WARNING: Your one hour asset file is no longer existent. Reverting to default one hour assets.",
                            "Warning",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );
                        window.Custom1HRAssetsToggle(false);
                    }
                    else
                    {
                        string oneHourAssetPathContents = File.ReadAllText(oneHourAssetPath);
                        try
                        {
                            string json = File.ReadAllText(Properties.Settings.Default.OneHourModeAssetsFilepath);
                            var options = new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            };

                            customObjectiveSettings = JsonSerializer.Deserialize<CustomObjectiveSettings>(json, options);
                        }
                        catch (JsonException)
                        {
                            // Display an alert box with the error message
                            System.Windows.MessageBox.Show(
                                $"ERROR: One hour JSON deserialization failed. Please ensure the one hour mode assets file is valid JSON. Reverting to base one hour assets.",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error
                            );
                            window.Custom1HRAssetsToggle(false);
                        }
                    }
                }
                return customObjectiveSettings;
            }
            else if (data.dartsMode)
            {
                CustomObjectiveSettings customObjectiveSettings = JsonSerializer.Deserialize<CustomObjectiveSettings>(
                    JsonSerializer.Serialize(dartsAssets)
                );
                if (Properties.Settings.Default.CustomDartsAssets)
                {
                    string dartsAssetPath = Properties.Settings.Default.DartsModeAssetsFilepath;
                    string directoryPath = Path.GetDirectoryName(dartsAssetPath);
                    if (!Directory.Exists(directoryPath) || !File.Exists(dartsAssetPath))
                    {
                        // Display an alert box with the error message
                        System.Windows.MessageBox.Show(
                            $"WARNING: Your darts asset file is no longer existent. Reverting to default darts assets.",
                            "Warning",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );
                        window.CustomDartsAssetsToggle(false);
                    }
                    else
                    {
                        string dartsAssetPathContents = File.ReadAllText(dartsAssetPath);
                        try
                        {
                            string json = File.ReadAllText(Properties.Settings.Default.DartsModeAssetsFilepath);
                            var options = new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            };

                            customObjectiveSettings = JsonSerializer.Deserialize<CustomObjectiveSettings>(json, options);
                        }
                        catch (JsonException)
                        {
                            // Display an alert box with the error message
                            System.Windows.MessageBox.Show(
                                $"ERROR: Darts JSON deserialization failed. Please ensure the darts mode assets file is valid JSON. Reverting to base darts assets.",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error
                            );
                            window.CustomDartsAssetsToggle(false);
                        }
                    }
                }
                return customObjectiveSettings;
            }
            else
            {
                return new CustomObjectiveSettings();
            }
        }

        public void GenerateCustomObjGrid(bool keepMarkings = false)
        {
            CustomObjectiveSettings overrideObject = GetCustomGameModeAssets();

            // keeps track of previous button states if we're just sorting them
            Dictionary<string, (bool IsChecked, Color Background, bool IsAnnotated, double Opacity)> previousStates = null;

            if (keepMarkings && buttons != null)
            {
                previousStates = new Dictionary<string, (bool IsChecked, Color Background, bool IsAnnotated, double Opacity)>();

                for (int i = 0; i < buttons.GetLength(0); i++)
                {
                    for (int j = 0; j < buttons.GetLength(1); j++)
                    {
                        var btn = buttons[i, j];
                        if (btn?.Tag is string tag)
                        {
                            previousStates[tag] = (
                                btn.IsChecked ?? false,
                                (btn.Background as SolidColorBrush)?.Color
                                    ?? currentColors["Unmarked Color"],
                                annotationStatus[i, j],
                                btn.Opacity
                            );
                        }
                    }
                }
            }

            DynamicGrid.Children.Clear();   // removes any previous grid
            UpdateGridBanner(false);        // hides the banner

            if (data.dartsMode) {

                //reset banner visibility
                UpdateGridBanner(true, "DARTS OBJECTIVES", "DARTSOVERRIDE");

                //override setup
                dartsOverrideAssets.Clear();
                dartsObjGridSettings.Clear();

                dartsOverrideAssets = overrideObject.objectivePointList ?? dartsAssets.objectivePointList;

                // only show positive point objectives
                foreach (var item in dartsOverrideAssets.Where(kvp => kvp.Value <= 0).ToList())
                {
                    dartsOverrideAssets.Remove(item.Key);
                }

                dartsObjGridSettings.Add("gridHeight", Int32.Parse(overrideObject.gridHeight.ToString()));
                dartsObjGridSettings.Add("gridWidth", Int32.Parse(overrideObject.gridWidth.ToString()));
                dartsObjGridSettings.Add("objectiveCount", Int32.Parse(overrideObject.objectiveCount.ToString()));
                dartsObjGridSettings.Add("maxFormObjectivesPerForm", Int32.Parse(overrideObject.maxFormObjectivesPerForm.ToString()));
                dartsObjGridSettings.Add("dataAndASCanBeObjectives", Int32.Parse(overrideObject.dataAndASCanBeObjectives.ToString()));
                dartsObjGridSettings.Add("pointsToWin", Int32.Parse(overrideObject.pointsToWin.ToString()));

                // handle darts title
                TotalValue.Text = dartsObjGridSettings["pointsToWin"].ToString();
                CollectedValue.Text = "0";

            }
            else if (data.oneHourMode)
            {
                //reset banner visibility
                UpdateGridBanner(true, "1HR OBJECTIVES", "1HROVERRIDE");

                //override setup
                oneHourOverrideAssets.Clear();
                oneHourOverrideBonus.Clear();
                oneHourMultiplicativeBonus.Clear();
                oneHourObjGridSettings.Clear();

                //handle 1 hour replacements
                if (overrideObject.oneHourReplacements != null && overrideObject.oneHourReplacements.Count != 0)
                {
                    data.codes.oneHourReplacements = overrideObject.oneHourReplacements;
                }

                oneHourOverrideAssets = overrideObject.objectivePointList ?? oneHourAssets.objectivePointList;

                // only show positive point objectives
                foreach (var item in oneHourOverrideAssets.Where(kvp => kvp.Value <= 0).ToList())
                {
                    oneHourOverrideAssets.Remove(item.Key);
                }

                oneHourOverrideBonus.Add("asArenaBonus", Int32.Parse(overrideObject.asArenaBonus.ToString()));
                oneHourOverrideBonus.Add("dataArenaBonus", Int32.Parse(overrideObject.dataArenaBonus.ToString()));
                oneHourOverrideBonus.Add("sephiArenaBonus", Int32.Parse(overrideObject.sephiArenaBonus.ToString()));
                oneHourOverrideBonus.Add("terraArenaBonus", Int32.Parse(overrideObject.terraArenaBonus.ToString()));
                oneHourOverrideBonus.Add("dataXemnasArenaBonus", Int32.Parse(overrideObject.dataXemnasArenaBonus.ToString()));

                oneHourOverrideBonus.Add("pirateMinuteFightBonus", Int32.Parse(overrideObject.pirateMinuteFightBonus.ToString()));
                oneHourOverrideBonus.Add("missionsBonus", Int32.Parse(overrideObject.missionsBonus.ToString()));
                oneHourOverrideBonus.Add("summitBonus", Int32.Parse(overrideObject.summitBonus.ToString()));
                oneHourOverrideBonus.Add("throneRoomBonus", Int32.Parse(overrideObject.throneRoomBonus.ToString()));
                oneHourOverrideBonus.Add("throneRoomBonusEarly", Int32.Parse(overrideObject.throneRoomBonusEarly.ToString()));

                oneHourMultiplicativeBonus.Add("bcDoubleFightMultiplier", Double.Parse(overrideObject.bcDoubleFightMultiplier.ToString()));
                oneHourMultiplicativeBonus.Add("lordsArenaMultiplier", Double.Parse(overrideObject.lordsArenaMultiplier.ToString()));

                oneHourObjGridSettings.Add("gridHeight", Int32.Parse(overrideObject.gridHeight.ToString()));
                oneHourObjGridSettings.Add("gridWidth", Int32.Parse(overrideObject.gridWidth.ToString()));
                oneHourObjGridSettings.Add("objectiveCount", Int32.Parse(overrideObject.objectiveCount.ToString()));
                oneHourObjGridSettings.Add("maxFormObjectivesPerForm", Int32.Parse(overrideObject.maxFormObjectivesPerForm.ToString()));
                oneHourObjGridSettings.Add("dataAndASCanBeObjectives", Int32.Parse(overrideObject.dataAndASCanBeObjectives.ToString()));
                objectivesNeed = Int32.Parse(overrideObject.objectivesToWin.ToString());

                //if(overrideObject.ContainsKey("bossHintingHome"))
                //{
                //    data.BossHomeHinting = overrideObject["bossHintingHome"].ToString().ToLower() == "true";
                //}
            }

            //build asset list
            assets.Clear();
            Random rng = new Random(data.convertedSeedHash);
            if (data.dartsMode)
                assets = dartsOverrideAssets.Keys.ToList();
            else if (data.oneHourMode)
                assets = oneHourOverrideAssets.Keys.ToList();

            string style = ObjTelevoIconsOption.IsChecked ? "CGM_Min-" : "CGM_Old-";

            // only populate custom assets if they are valid
            // if they are invalid, revert to default settings
            bool invalidAssetFound = false;
            foreach (var asset in assets)
            {
                try
                {
                    Grid squareContent = (Grid)FindResource(style + asset);
                }
                catch (ResourceReferenceKeyNotFoundException)
                {
                    string gameMode = data.oneHourMode ? "one hour" : "darts";
                    System.Windows.MessageBox.Show(
                        $"WARNING: You have asset errors in your custom {gameMode} asset setup. Please correct them before loading a custom asset file. Reverting to default {gameMode} assets.",
                        "Warning",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    if (data.oneHourMode)
                    {
                        window.Custom1HRAssetsToggle(false);
                    }
                    else if (data.dartsMode)
                    {
                        window.CustomDartsAssetsToggle(false);
                    }

                    invalidAssetFound = true;
                    break;
                }
            }

            if (invalidAssetFound)
            {
                GenerateCustomObjGrid(keepMarkings);
                return;
            }

            #region Random Coices

            int allowDataAndAS =
                (data.dartsMode && dartsObjGridSettings.TryGetValue("dataAndASCanBeObjectives", out int d) && d == 0) ||
                (data.oneHourMode && oneHourObjGridSettings.TryGetValue("dataAndASCanBeObjectives", out int o) && o == 0)
                ? 0 : 1;

            if (allowDataAndAS == 0)
            {
                string[] members = { "Zexion", "Marluxia", "Lexaeus", "Vexen", "Larxene" };
                foreach (var member in members)
                {
                    string toRemove = rng.Next(2) == 0 ? member : member + "Data";
                    assets.Remove(toRemove);
                }
            }


            // for each form, only keep maxFormObjectivesPerForm form objectives
            var formObjectivesMap = new Dictionary<string, List<string>>()
            {
                { "Valor", new List<string>() },
                { "Wisdom", new List<string>() },
                { "Limit", new List<string>() },
                { "Master", new List<string>() },
                { "Final", new List<string>() }
            };

            foreach (var asset in assets)
            {
                foreach (var form in formObjectivesMap.Keys)
                {
                    if (asset.StartsWith(form))
                    {
                        formObjectivesMap[form].Add(asset);
                        break; // only matches one form prefix
                    }
                }
            }
            foreach (var form in new[] { "Valor", "Wisdom", "Limit", "Master", "Final" })
            {
                int maxFormObjectivesAllowed = data.oneHourMode
                    ? oneHourObjGridSettings["maxFormObjectivesPerForm"]
                    : (data.dartsMode ?
                    dartsObjGridSettings["maxFormObjectivesPerForm"]
                    : int.MaxValue);

                var formObjectives = formObjectivesMap[form];
                int formObjectivesToRemove = Math.Max(0, formObjectives.Count - maxFormObjectivesAllowed);

                if (formObjectivesToRemove > 0)
                {
                    var shuffledFormObjectives = formObjectives.OrderBy(x => rng.Next()).ToList();
                    for (int i = 0; i < formObjectivesToRemove; i++)
                    {
                        assets.Remove(shuffledFormObjectives[i]);
                    }
                }
            }
            #endregion

            // number of objectives to use (7 is defaut)
            if (data.dartsMode)
                assets = assets.OrderBy(x => rng.Next()).Take(dartsObjGridSettings["objectiveCount"]).ToList();
            else if (data.oneHourMode)
                assets = assets.OrderBy(x => rng.Next()).Take(oneHourObjGridSettings["objectiveCount"]).ToList();

            //fix icon prefix for assets
            getAssetPrefixCustomGameMode();

            if (Properties.Settings.Default.ObjectivePointsOrder)
            {
                // order by points
                if (data.dartsMode)
                    assets = assets.OrderBy(a => dartsOverrideAssets[a.Remove(0, 8)]).ToList();
                else if (data.oneHourMode)
                    assets = assets.OrderBy(a => oneHourOverrideAssets[a.Remove(0, 8)]).ToList();
            }
            else
            {
                // order by "world"
                if (data.dartsMode)
                    assets = assets.OrderBy(a => dartsOverrideAssets.Keys.ToList().IndexOf(a.Remove(0, 8))).ToList();
                else if (data.oneHourMode)
                    assets = assets.OrderBy(a => oneHourOverrideAssets.Keys.ToList().IndexOf(a.Remove(0, 8))).ToList();
            }

            //get grid size
            int objectiveCount = data.dartsMode ? dartsObjGridSettings["objectiveCount"] 
                : (data.oneHourMode ? oneHourObjGridSettings["objectiveCount"] : 0);

            numRows = data.dartsMode ? dartsObjGridSettings["gridHeight"]
                : (data.oneHourMode ? oneHourObjGridSettings["gridHeight"] : 0);
            numColumns = data.dartsMode ? dartsObjGridSettings["gridWidth"]
                : (data.oneHourMode ? oneHourObjGridSettings["gridWidth"] : 0);

            //if these values are not set up properly for number of objectives then default to 
            //looking for grid size in size lookup table
            if (numRows * numColumns >= objectiveCount)
            {
                int blankSquares = 0;
                while (!objSizeLookup.ContainsKey(objectiveCount + blankSquares))
                {
                    blankSquares++;
                }
                numRows = objSizeLookup[objectiveCount + blankSquares].Item1;
                numColumns = objSizeLookup[objectiveCount + blankSquares].Item2;
            }
            else
            {
                System.Windows.MessageBox.Show(
                    $"WARNING: Your number of rows and columns cannot fully contain the number of objectives. Reducing objective count to fit the grid size.",
                    "Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                assets = assets.Take(numRows * numColumns).ToList();
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
                    foreach (var item in squareContent.Children)
                    {
                        if (item is Viewbox box)
                        {
                            OutlinedTextBlock textbox = (OutlinedTextBlock)box.Child;
                            if (data.dartsMode)
                                textbox.Text = dartsOverrideAssets[assets[buttonDone].Remove(0, 8)].ToString() + " Points";
                            else if (data.oneHourMode)
                                textbox.Text = oneHourOverrideAssets[assets[buttonDone].Remove(0, 8)].ToString() + " Points";
                        }
                    }

                    ToggleButton button = new ToggleButton();
                    bool buttonContentRevealed = buttons[i, j] != null && ((buttons[i, j].IsChecked ?? false) || buttons[i, j].Content != null);
                    //button.SetResourceReference(ContentProperty, assets[(i * numColumns) + j]);
                    button.Content = squareContent;
                    button.Background = new SolidColorBrush(currentColors["Unmarked Color"]);
                    button.Tag = assets[(i * numColumns) + j].ToString();
                    button.Style = (Style)FindResource("ColorToggleButton");
                    if (keepMarkings && previousStates != null && previousStates.TryGetValue((string)button.Tag, out var state))
                    {
                        button.IsChecked = state.IsChecked;
                        button.Background = new SolidColorBrush(state.Background);
                        button.Opacity = state.Opacity;
                        annotationStatus[i, j] = state.IsAnnotated;
                    }
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
                if (new[] { "1HROVERRIDE", }.Contains(textIcon))
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
                    OBannerIconL.Text = "";
                    OBannerIconR.Text = "";
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
                SetColorForButton(button.Background, currentColors["Marked Color"]);
            }
            else
            {
                SetColorForButton(button.Background, currentColors["Unmarked Color"]);
            }
            button.Opacity = 1.0;

            checkNeeded();
        }


        public void Button_RightClick(object sender, RoutedEventArgs e, int i, int j)
        {
            var button = (ToggleButton)sender;
            //annotated to hidden
            if (annotationStatus[i, j] && button.Opacity == 1.0)
            {

                annotationStatus[i, j] = false;
                SetColorForButton(button.Background, originalColors[i, j]);
                button.Opacity = 0.2;
                //Console.WriteLine("1");
            }
            //hidden to neutral
            else if (button.Opacity == 0.2)
            {
                annotationStatus[i, j] = false;
                SetColorForButton(button.Background, originalColors[i, j]);
                button.Opacity = 1.0;
                //Console.WriteLine("2");
            }
            //neutral to annotated
            else
            {
                originalColors[i, j] = GetColorFromButton(button.Background);
                annotationStatus[i, j] = true;
                SetColorForButton(button.Background, currentColors["Annotated Color"]);
                //Console.WriteLine("3");
            }
        }

        public void Button_Scroll(object sender, MouseWheelEventArgs e, int i, int j)
        {
            var button = (ToggleButton)sender;
            int buttonState = 0;
            int prevState;
            //get button status - checked, annotated, or none
            if (button.Opacity < 1.0) //if hidden
            {
                buttonState = 2;
            }
            else if (annotationStatus[i, j]) //if annotated
            {
                buttonState = 3;
            }
            else if (button.IsChecked ?? true) //if clicked
            {
                buttonState = 1;
            }
            else //if neutral
            {
                buttonState = 0;
            }

            prevState = buttonState;
            //Console.WriteLine(buttonState);
            if (e.Delta > 0) //mouse scroll up
            {
                buttonState += 1;
                if (buttonState > 3)
                    buttonState = 0;
                //Console.WriteLine("down");
            }
            else if (e.Delta < 0) //mouse scroll down
            {
                buttonState -= 1;
                if (buttonState < 0)
                    buttonState = 3;
                //Console.WriteLine("up");
            }
            //Console.WriteLine(buttonState);
            //Console.WriteLine(button.IsChecked ?? true);

            if (buttonState == 1) //1 = clicked
            {
                button.IsChecked = true;
                annotationStatus[i, j] = false;
                Button_RightClick(sender, e, i, j);
                Button_Click(sender, e, i, j);
            }
            else if (buttonState == 2) //2 = hidden
            {
                button.IsChecked = false;
                annotationStatus[i, j] = false;
                button.Opacity = 0.2;
                SetColorForButton(button.Background, originalColors[i, j]);
                if (prevState == 1)
                    checkNeeded();
            }
            else if (buttonState == 3) //3 = annotated
            {
                button.IsChecked = false;
                annotationStatus[i, j] = true;
                Button_Click(sender, e, i, j);
                Button_RightClick(sender, e, i, j);
            }
            else //0 = neutral
            {
                button.IsChecked = false;
                annotationStatus[i, j] = false;
                button.Opacity = 1.0;
                Button_RightClick(sender, e, i, j);
                Button_Click(sender, e, i, j);
            }
        }


        public void checkNeeded()
        {
            if (objectivesNeed != 0)
            {
                List<Tuple<int, int>> completeSquares = new List<Tuple<int, int>>();
                for (int i = 0; i < numRows; i++)
                {
                    for (int j = 0; j < numColumns; j++)
                    {
                        if (buttons[i, j] != null && buttons[i, j].IsChecked == true)
                        {
                            completeSquares.Add(Tuple.Create(i, j));
                        }
                    }
                }

                if (completeSquares.Count >= objectivesNeed)
                {
                    foreach (var coord in completeSquares)
                    {
                        var button = buttons[coord.Item1, coord.Item2];
                        SetColorForButton(button.Background, currentColors["Win Condition Met Color"]);
                    }
                }
                else
                {
                    foreach (var coord in completeSquares)
                    {
                        var button = buttons[coord.Item1, coord.Item2];
                        if (annotationStatus[coord.Item1, coord.Item2])
                            SetColorForButton(button.Background, currentColors["Annotated Color"]);
                        else
                            SetColorForButton(button.Background, currentColors["Marked Color"]);
                    }
                }

                CollectedValue.Text = completeSquares.Count.ToString();
            }
            // if objectives aren't measured by count, they are measured by points in a custom game mode
            else
            {
                if (objGrid == null)
                    return;

                int collectedPoints = 0;
                int marksTotal = 0;
                for (int i = 0; i < numRows; i++)
                {
                    for (int j = 0; j < numColumns; j++)
                    {
                        var button = buttons[i, j];
                        if (button != null && button.IsChecked == true)
                        {
                            if (data.dartsMode)
                                collectedPoints += dartsOverrideAssets[button.Tag.ToString().Remove(0, 8)];
                            else
                                collectedPoints += oneHourOverrideAssets[button.Tag.ToString().Remove(0, 8)];

                            marksTotal++;
                        }
                    }
                }
                bool winCondition = false;
                if (data.oneHourMode)
                    winCondition = oneHourObjGridSettings.ContainsKey("pointsToWin") && collectedPoints >= oneHourObjGridSettings["pointsToWin"];
                else if (data.dartsMode)
                    winCondition = dartsObjGridSettings.ContainsKey("pointsToWin") && collectedPoints >= dartsObjGridSettings["pointsToWin"];
                for (int i = 0; i < numRows; i++)
                {
                    for (int j = 0; j < numColumns; j++)
                    {
                        var button = buttons[i, j];
                        if (button != null && button.IsChecked == true)
                        {
                            if (winCondition)
                            {
                                SetColorForButton(button.Background, currentColors["Win Condition Met Color"]);
                            }
                            else
                            {
                                if (annotationStatus[i, j])
                                    SetColorForButton(button.Background, currentColors["Annotated Color"]);
                                else
                                    SetColorForButton(button.Background, currentColors["Marked Color"]);
                            }
                        }
                    }
                }

                cgmPoints = collectedPoints;
                CollectedValue.Text = cgmPoints.ToString();
                window.UpdatePointScore(0);
                Console.WriteLine("writing marks to game | " + marksTotal);
                window.SetCompletionMarks(marksTotal);
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
                { "Unmarked Color", Color.FromArgb(unmarkedColor.A, unmarkedColor.R, unmarkedColor.G, unmarkedColor.B) },
                { "Marked Color", Color.FromArgb(markedColor.A, markedColor.R, markedColor.G, markedColor.B) },
                { "Annotated Color", Color.FromArgb(annotatedColor.A, annotatedColor.R, annotatedColor.G, annotatedColor.B) },
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
                            SetColorForButton(buttons[i, j].Background, currentColors["Annotated Color"]);
                        else
                        {
                            SetColorForButton(buttons[i, j].Background, (bool)buttons[i, j].IsChecked ? currentColors["Marked Color"] : currentColors["Unmarked Color"]);
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

        private void getAssetPrefixCustomGameMode()
        {
            string style = ObjTelevoIconsOption.IsChecked ? "CGM_Min-" : "CGM_Old-";

            for (int i = 0; i < assets.Count; i++)
            {
                //if (ObjCustomIconsOption.IsChecked && MainWindow.CusObjImagesList.Contains("CGM_Cus-" + assets[i]))
                //{
                //    assets[i] = "CGM_Cus-" + assets[i];
                //    continue;
                //}

                assets[i] = style + assets[i];
            }
        }

        private void Change_IconsCustomGameMode()
        {
            if (objGrid == null)
                return;

            string prefix = "CGM_Min-";
            if (ObjSonicIconsOption.IsChecked)
                prefix = "CGM_Old-";

            foreach (var child in objGrid.Children)
            {
                //check if it's a toggle button
                if (child is ToggleButton square)
                {
                    string squareTag = square.Tag.ToString().Remove(0, 8);
                    Grid squareContent = (Grid)FindResource(prefix + squareTag);

                    foreach (var item in squareContent.Children)
                    {
                        if (item is Viewbox box)
                        {
                            OutlinedTextBlock textbox = (OutlinedTextBlock)box.Child;
                            if (data.dartsMode)
                                textbox.Text = dartsOverrideAssets[squareTag].ToString() + " Points";
                            else if (data.oneHourMode)
                                textbox.Text = oneHourOverrideAssets[squareTag].ToString() + " Points";
                        }
                    }

                    //check 
                    //if (ObjCustomIconsOption.IsChecked && assets.Contains("CGM_Cus-" + squareTag))
                    //{
                    //    square.SetResourceReference(ContentProperty, "CGM_Cus-" + squareTag);
                    //    continue;
                    //}
                    //
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

            if (data.oneHourMode || data.dartsMode)
                Change_IconsCustomGameMode();
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

            if (data.oneHourMode || data.dartsMode)
                Change_IconsCustomGameMode();
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

            if (data.oneHourMode || data.dartsMode)
                Change_IconsCustomGameMode();
            else
                updateAssetPrefix(true);
        }

        private void ObjPointsOrderToggle(object sender, RoutedEventArgs e)
        {
            ObjPointsOrderToggle(ObjPointsOrderOption.IsChecked);
        }

        private void ObjPointsOrderToggle(bool toggle)
        {
            Properties.Settings.Default.ObjectivePointsOrder = toggle;
            GenerateCustomObjGrid(true);
        }
    }
}
