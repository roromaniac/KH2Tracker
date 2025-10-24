using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.IO.Compression;
using Microsoft.Win32;
using System.Linq;
using System.Text;
using System.Text.Json;
using Path = System.IO.Path;
using KhTracker.Hotkeys;
using System.Windows.Input;
using MessageForm = System.Windows.Forms;
using System.Xml.Linq;
using System.Windows.Documents;
using System.Text.Json.Serialization;
using System.Security.Policy;
using System.Windows.Threading;

namespace KhTracker
{
    public partial class MainWindow : Window
    {
        /// 
        /// Save/load progress
        ///

        private void DropFile(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (Path.GetExtension(files[0]).ToUpper() == ".ZIP")
                    OpenKHSeed(files[0]);
                else if (Path.GetExtension(files[0]).ToUpper() == ".TSV")
                    Load(files[0]);
            }
        }

        private void SaveProgress(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                DefaultExt = ".tsv",
                Filter = "Tracker Save File (*.tsv)|*.tsv",
                FileName = "kh2fm-tracker-save",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                Save(saveFileDialog.FileName);
            }
        }

        public void Save(string filename)
        {
            #region Settings
            var settingInfo = new bool[31];
            //Display toggles
            settingInfo[0] = ReportsOption.IsChecked;
            settingInfo[1] = TornPagesOption.IsChecked;
            settingInfo[2] = PromiseCharmOption.IsChecked;
            settingInfo[3] = AbilitiesOption.IsChecked;
            settingInfo[4] = AntiFormOption.IsChecked;
            settingInfo[5] = VisitLockOption.IsChecked;
            settingInfo[6] = ExtraChecksOption.IsChecked;
            settingInfo[7] = SoraLevel01Option.IsChecked;
            settingInfo[8] = SoraLevel50Option.IsChecked;
            settingInfo[9] = SoraLevel99Option.IsChecked;
            //World toggles
            settingInfo[10] = SoraHeartOption.IsChecked;
            settingInfo[11] = DrivesOption.IsChecked;
            settingInfo[12] = SimulatedOption.IsChecked;
            settingInfo[13] = TwilightTownOption.IsChecked;
            settingInfo[14] = HollowBastionOption.IsChecked;
            settingInfo[15] = BeastCastleOption.IsChecked;
            settingInfo[16] = OlympusOption.IsChecked;
            settingInfo[17] = AgrabahOption.IsChecked;
            settingInfo[18] = LandofDragonsOption.IsChecked;
            settingInfo[19] = DisneyCastleOption.IsChecked;
            settingInfo[20] = PrideLandsOption.IsChecked;
            settingInfo[21] = PortRoyalOption.IsChecked;
            settingInfo[22] = HalloweenTownOption.IsChecked;
            settingInfo[23] = SpaceParanoidsOption.IsChecked;
            settingInfo[24] = TWTNWOption.IsChecked;
            settingInfo[25] = HundredAcreWoodOption.IsChecked;
            settingInfo[26] = AtlanticaOption.IsChecked;
            settingInfo[27] = SynthOption.IsChecked;
            settingInfo[28] = PuzzleOption.IsChecked;
            //new
            settingInfo[29] = ChestLockOption.IsChecked;

            #endregion

            #region ReportInfo
            var attempsInfo = new int[13];
            for (int i = 0; i < 13; ++i)
            {
                int attempts = 3;
                if (data.hintsLoaded)
                    attempts = data.reportAttempts[i];

                attempsInfo[i] = attempts;
            }
            #endregion

            #region WorldInfo
            Dictionary<string, object> worldvalueInfo = new Dictionary<string, object>();
            foreach (string worldKey in data.WorldsData.Keys.ToList())
            {
                var worldData = data.WorldsData[worldKey];
                List<string> worldItems = new List<string>();
                foreach (Item item in worldData.worldGrid.Children)
                {
                    worldItems.Add(item.Name);
                }
                var testingthing = new
                {
                    //Value = worldData.value.Text, //do i need this?
                    //Progression = worldData.progress, //or this?
                    Items = worldItems
                    //Hinted = worldData.hinted,
                    //HintedHint = worldData.hintedHint,
                    //GhostHint = worldData.containsGhost,
                    //Complete = worldData.complete,
                    //Locks = worldData.visitLocks,
                };
                worldvalueInfo.Add(worldKey, testingthing);
            };
            #endregion

            #region Counters
            var counterInfo = new int[8] { 1, 1, 1, 1, 1, 1, 0, 0 };
            counterInfo[0] = data.DriveLevels[0];
            counterInfo[1] = data.DriveLevels[1];
            counterInfo[2] = data.DriveLevels[2];
            counterInfo[3] = data.DriveLevels[3];
            counterInfo[4] = data.DriveLevels[4];
            counterInfo[5] = DeathCounter;
            counterInfo[6] = data.usedPages;
            #endregion

            FileStream file = File.Create(filename);
            StreamWriter writer = new StreamWriter(file);
            var saveInfo = new
            {
                Version = Title,
                SeedHash = data.seedHashVisual,
                Settings = settingInfo,
                SeedHints = data.openKHHintText,
                BossHints = data.openKHBossText,
                RandomSeed = data.convertedSeedHash,
                Worlds = worldvalueInfo,
                Reports = data.reportInformation,
                ReportLoc = data.reportLocations,
                ProgBossInfo = data.progBossInformation,
                Attemps = attempsInfo,
                Counters = counterInfo,
                ForcedFinal = data.forcedFinal,
                Events = data.eventLog,
                BossEvents = data.bossEventLog,
                BoardSettings = gridWindow.DownloadCardSetting(),
                BunterBosses = gridWindow.bunterBosses,
                OneHourMode = data.oneHourMode,
                DartsMode = data.dartsMode,
            };

            var saveFinal = JsonSerializer.Serialize(saveInfo);
            string saveFinal64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(saveFinal));
            string saveScrambled = ScrambleText(saveFinal64, true);
            writer.WriteLine(saveScrambled);
            writer.Close();
        }

        private void LoadProgress(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                DefaultExt = ".tsv",
                Filter = "Tracker Save File (*.tsv)|*.tsv",
                FileName = "kh2fm-tracker-save",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
            };
            if (openFileDialog.ShowDialog() == true)
            {
                Load(openFileDialog.FileName);
            }
        }

        private void Load(string filename)
        {
            if (!InProgressCheck("tsv"))
                return;

            //open file
            StreamReader reader = new StreamReader(File.Open(filename, FileMode.Open));
            var savescrambled = reader.ReadLine();
            reader.Close();

            //start reading save
            var save64 = ScrambleText(savescrambled, false);
            var saveData = Encoding.UTF8.GetString(Convert.FromBase64String(save64));
            var saveObject = JsonSerializer.Deserialize<Dictionary<string, object>>(saveData);

            //check save version
            if (saveObject.ContainsKey("Version"))
            {
                string saveVer = saveObject["Version"].ToString();
                if (saveVer != Title)
                {
                    //Console.WriteLine("Different save version!");
                    string message = "This save was made with a different version of the tracker. " +
                        "\n Loading this may cause unintended effects. " +
                        "\n Do you still want to continue loading?";
                    string caption = "Save Version Mismatch";
                    MessageForm.MessageBoxButtons buttons = MessageForm.MessageBoxButtons.YesNo;
                    MessageForm.DialogResult result;

                    result = MessageForm.MessageBox.Show(message, caption, buttons);
                    if (result == MessageForm.DialogResult.No)
                    {
                        return;
                    }
                }
            }

            //reset
            OnReset(null, null);

            //continue to loading normally
            LoadNormal(saveObject);

            if (data.wasTracking)
            {
                InitTracker();
            }
        }

        private void LoadNormal(Dictionary<string, object> Savefile)
        {
            //Check Settings
            if (Savefile.ContainsKey("Settings"))
            {
                var setting = JsonSerializer.Deserialize<bool[]>(Savefile["Settings"].ToString());
                //Display toggles
                ReportsToggle(setting[0]);
                TornPagesToggle(setting[1]);
                PromiseCharmToggle(setting[2]);
                AbilitiesToggle(setting[3]);
                AntiFormToggle(setting[4]);
                VisitLockToggle(setting[5]);
                ChestLockToggle(setting[29]);
                ExtraChecksToggle(setting[6]);
                if (setting[7])
                    SoraLevel01Toggle(true);
                else if (setting[8])
                    SoraLevel50Toggle(true);
                else if (setting[9])
                    SoraLevel99Toggle(true);
                //World toggles
                SoraHeartToggle(setting[10]);
                DrivesToggle(setting[11]);
                SimulatedToggle(setting[12]);
                TwilightTownToggle(setting[13]);
                HollowBastionToggle(setting[14]);
                BeastCastleToggle(setting[15]);
                OlympusToggle(setting[16]);
                AgrabahToggle(setting[17]);
                LandofDragonsToggle(setting[18]);
                DisneyCastleToggle(setting[19]);
                PrideLandsToggle(setting[20]);
                PortRoyalToggle(setting[21]);
                HalloweenTownToggle(setting[22]);
                SpaceParanoidsToggle(setting[23]);
                TWTNWToggle(setting[24]);
                HundredAcreWoodToggle(setting[25]);
                AtlanticaToggle(setting[26]);
                SynthToggle(setting[27]);
                PuzzleToggle(setting[28]);
            }

            //check if enemy rando data exists
            if (Savefile.ContainsKey("BossHints"))
            {
                if (Savefile["BossHints"].ToString() != "None")
                {
                    data.BossRandoFound = true;
                    data.openKHBossText = Savefile["BossHints"].ToString();

                    var enemyText = Encoding.UTF8.GetString(Convert.FromBase64String(data.openKHBossText));
                    try
                    {
                        var enemyObject = JsonSerializer.Deserialize<Dictionary<string, object>>(enemyText);
                        var bosses = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(enemyObject["BOSSES"].ToString());

                        foreach (var bosspair in bosses)
                        {
                            string bossOrig = bosspair["original"].ToString();
                            string bossRepl = bosspair["new"].ToString();

                            data.BossList.Add(bossOrig, bossRepl);
                        }
                    }
                    catch
                    {
                        data.BossRandoFound = false;
                        data.openKHBossText = "None";
                        App.logger?.Record("error while trying to parse bosses from save.");
                    }
                }
            }

            //use random seed from save
            if (Savefile.ContainsKey("RandomSeed"))
            {
                if (Savefile["RandomSeed"] != null)
                {
                    var seednumber = JsonSerializer.Deserialize<int>(Savefile["RandomSeed"].ToString());
                    data.convertedSeedHash = seednumber;
                }
            }

            //identify relevant bosses from save
            if (Savefile.ContainsKey("BunterBosses"))
            {
                gridWindow.bunterBosses = Savefile["BunterBosses"] != null ? JsonSerializer.Deserialize<List<Dictionary<string, object>>>(Savefile["BunterBosses"].ToString()) : null;
            }

            //check one hour toggle
            if (Savefile.ContainsKey("OneHourMode"))
            {
                if (Savefile["OneHourMode"].ToString().ToLower() == "true")
                {
                    data.oneHourMode = true;
                    OneHourOption.IsChecked = true;
                    data.BossHomeHinting = true;

                    // turn off other game modes
                    data.dartsMode = false;
                    DartsOption.IsChecked = false;
                }
                else
                {
                    data.oneHourMode = false;
                    OneHourOption.IsChecked = false;
                    data.BossHomeHinting = false;
                }
            }

            //check darts toggle
            if (Savefile.ContainsKey("DartsMode"))
            {
                if (Savefile["DartsMode"].ToString().ToLower() == "true")
                {
                    data.dartsMode = true;
                    DartsOption.IsChecked = true;

                    // turn off other game modes
                    data.oneHourMode = false;
                    OneHourOption.IsChecked = false;
                    data.BossHomeHinting = false;
                }
                else
                {
                    data.dartsMode = false;
                    DartsOption.IsChecked = false;
                }
            }

            //check hintsdata
            if (Savefile.ContainsKey("SeedHints"))
            {
                if (Savefile["SeedHints"].ToString() != "None")
                {
                    data.openKHHintText = Savefile["SeedHints"].ToString();
                    var hintText = Encoding.UTF8.GetString(Convert.FromBase64String(data.openKHHintText));
                    var hintObject = JsonSerializer.Deserialize<Dictionary<string, object>>(hintText);
                    var settings = new List<string>();
                    var hintableItems = new List<string>(JsonSerializer.Deserialize<List<string>>(hintObject["hintableItems"].ToString()));

                    data.ShouldResetHash = false;

                    if (hintObject.ContainsKey("emblems"))
                    {
                        data.EmblemMode = true;
                        Dictionary<string, int> emblemValues = new Dictionary<string, int>(JsonSerializer.Deserialize<Dictionary<string, int>>(hintObject["emblems"].ToString()));
                        EmblemTotalValue.Text = emblemValues["num_emblems_needed"].ToString();
                        displays.Add("Emblems");
                    }

                    if (hintObject.ContainsKey("settings"))
                    {
                        settings = JsonSerializer.Deserialize<List<string>>(hintObject["settings"].ToString());

                        #region Settings

                        TornPagesToggle(false);
                        AbilitiesToggle(false);
                        ReportsToggle(false);
                        ExtraChecksToggle(false);
                        VisitLockToggle(false);
                        ChestLockToggle(false);
                        foreach (string item in hintableItems)
                        {
                            switch (item)
                            {
                                case "page":
                                    TornPagesToggle(true);
                                    break;
                                case "ability":
                                    AbilitiesToggle(true);
                                    break;
                                case "report":
                                    ReportsToggle(true);
                                    break;
                                case "other":
                                    ExtraChecksToggle(true);
                                    break;
                                case "visit":
                                    VisitLockToggle(true);
                                    break;
                                case "keyblade":
                                    ChestLockToggle(true);
                                    break;
                                case "proof":
                                case "magic":
                                case "form":
                                case "summon":
                                default:
                                    break;
                            }
                        }


                        //item settings
                        PromiseCharmToggle(false);
                        AntiFormToggle(false);

                        //world settings
                        SoraHeartToggle(true);
                        DrivesToggle(false);
                        SimulatedToggle(false);
                        TwilightTownToggle(false);
                        HollowBastionToggle(false);
                        BeastCastleToggle(false);
                        OlympusToggle(false);
                        AgrabahToggle(false);
                        LandofDragonsToggle(false);
                        DisneyCastleToggle(false);
                        PrideLandsToggle(false);
                        PortRoyalToggle(false);
                        HalloweenTownToggle(false);
                        SpaceParanoidsToggle(false);
                        TWTNWToggle(false);
                        HundredAcreWoodToggle(false);
                        AtlanticaToggle(false);
                        PuzzleToggle(false);
                        SynthToggle(false);

                        //progression hints GoA Current Hint Count
                        data.WorldsData["GoA"].value.Visibility = Visibility.Hidden;

                        //settings visuals
                        SettingRow.Height = new GridLength(0.5, GridUnitType.Star);
                        Setting_BetterSTT.Width = new GridLength(0, GridUnitType.Star);
                        Setting_Level_01.Width = new GridLength(0, GridUnitType.Star);
                        Setting_Level_50.Width = new GridLength(0, GridUnitType.Star);
                        Setting_Level_99.Width = new GridLength(0, GridUnitType.Star);
                        Setting_Absent.Width = new GridLength(0, GridUnitType.Star);
                        Setting_Absent_Split.Width = new GridLength(0, GridUnitType.Star);
                        Setting_Datas.Width = new GridLength(0, GridUnitType.Star);
                        Setting_Sephiroth.Width = new GridLength(0, GridUnitType.Star);
                        Setting_Terra.Width = new GridLength(0, GridUnitType.Star);
                        Setting_Cups.Width = new GridLength(0, GridUnitType.Star);
                        Setting_HadesCup.Width = new GridLength(0, GridUnitType.Star);
                        Setting_Cavern.Width = new GridLength(0, GridUnitType.Star);
                        Setting_Transport.Width = new GridLength(0, GridUnitType.Star);
                        Double SpacerValue = 10;
                        #endregion

                        //load settings from hints
                        foreach (string setting in settings)
                        {
                            //Console.WriteLine("setting found = " + setting);
                            switch (setting)
                            {
                                //items
                                case "PromiseCharm":
                                    PromiseCharmToggle(true);
                                    break;
                                case "Anti-Form":
                                    AntiFormToggle(true);
                                    break;
                                //worlds
                                case "Level":
                                    SoraHeartToggle(false);
                                    SoraLevel01Toggle(true);
                                    //AbilitiesToggle(true);
                                    Setting_Level_01.Width = new GridLength(1.5, GridUnitType.Star);
                                    SpacerValue--;
                                    break;
                                case "ExcludeFrom50":
                                    SoraLevel50Toggle(true);
                                    //AbilitiesToggle(true);
                                    Setting_Level_50.Width = new GridLength(1.5, GridUnitType.Star);
                                    SpacerValue--;
                                    data.HintRevealOrder.Add("SorasHeart");
                                    break;
                                case "ExcludeFrom99":
                                    SoraLevel99Toggle(true);
                                    //AbilitiesToggle(true);
                                    Setting_Level_99.Width = new GridLength(1.5, GridUnitType.Star);
                                    SpacerValue--;
                                    data.HintRevealOrder.Add("SorasHeart");
                                    break;
                                case "Simulated Twilight Town":
                                    SimulatedToggle(true);
                                    data.enabledWorlds.Add("STT");
                                    data.HintRevealOrder.Add("SimulatedTwilightTown");
                                    break;
                                case "Hundred Acre Wood":
                                    HundredAcreWoodToggle(true);
                                    data.enabledWorlds.Add("HundredAcreWood");
                                    data.HintRevealOrder.Add("HundredAcreWood");
                                    break;
                                case "Atlantica":
                                    AtlanticaToggle(true);
                                    data.enabledWorlds.Add("Atlantica");
                                    data.HintRevealOrder.Add("Atlantica");
                                    break;
                                case "Puzzle":
                                    PuzzleToggle(true);
                                    if (!data.HintRevealOrder.Contains("PuzzSynth"))
                                        data.HintRevealOrder.Add("PuzzSynth");
                                    data.puzzlesOn = true;
                                    break;
                                case "Synthesis":
                                    SynthToggle(true);
                                    if (!data.HintRevealOrder.Contains("PuzzSynth"))
                                        data.HintRevealOrder.Add("PuzzSynth");
                                    //data.synthOn = true;
                                    break;
                                case "Form Levels":
                                    DrivesToggle(true);
                                    data.HintRevealOrder.Add("DriveForms");
                                    break;
                                case "Land of Dragons":
                                    LandofDragonsToggle(true);
                                    data.enabledWorlds.Add("LoD");
                                    data.HintRevealOrder.Add("LandofDragons");
                                    break;
                                case "Beast's Castle":
                                    BeastCastleToggle(true);
                                    data.enabledWorlds.Add("BC");
                                    data.HintRevealOrder.Add("BeastsCastle");
                                    break;
                                case "Hollow Bastion":
                                    HollowBastionToggle(true);
                                    data.enabledWorlds.Add("HB");
                                    data.HintRevealOrder.Add("HollowBastion");
                                    break;
                                case "Twilight Town":
                                    TwilightTownToggle(true);
                                    data.enabledWorlds.Add("TT");
                                    data.HintRevealOrder.Add("TwilightTown");
                                    break;
                                case "The World That Never Was":
                                    TWTNWToggle(true);
                                    data.enabledWorlds.Add("TWTNW");
                                    data.HintRevealOrder.Add("TWTNW");
                                    break;
                                case "Space Paranoids":
                                    SpaceParanoidsToggle(true);
                                    data.enabledWorlds.Add("SP");
                                    data.HintRevealOrder.Add("SpaceParanoids");
                                    break;
                                case "Port Royal":
                                    PortRoyalToggle(true);
                                    data.enabledWorlds.Add("PR");
                                    data.HintRevealOrder.Add("PortRoyal");
                                    break;
                                case "Olympus Coliseum":
                                    OlympusToggle(true);
                                    data.enabledWorlds.Add("OC");
                                    data.HintRevealOrder.Add("OlympusColiseum");
                                    break;
                                case "Agrabah":
                                    AgrabahToggle(true);
                                    data.enabledWorlds.Add("AG");
                                    data.HintRevealOrder.Add("Agrabah");
                                    break;
                                case "Halloween Town":
                                    HalloweenTownToggle(true);
                                    data.enabledWorlds.Add("HT");
                                    data.HintRevealOrder.Add("HalloweenTown");
                                    break;
                                case "Pride Lands":
                                    PrideLandsToggle(true);
                                    data.enabledWorlds.Add("PL");
                                    data.HintRevealOrder.Add("PrideLands");
                                    break;
                                case "Disney Castle / Timeless River":
                                    DisneyCastleToggle(true);
                                    data.enabledWorlds.Add("DC");
                                    data.HintRevealOrder.Add("DisneyCastle");
                                    break;
                                //settings
                                case "better_stt":
                                    Setting_BetterSTT.Width = new GridLength(1.1, GridUnitType.Star);
                                    SpacerValue--;
                                    break;
                                case "Cavern of Remembrance":
                                    Setting_Cavern.Width = new GridLength(1, GridUnitType.Star);
                                    SpacerValue--;
                                    break;
                                case "Data Split":
                                    Setting_Absent_Split.Width = new GridLength(1, GridUnitType.Star);
                                    SpacerValue--;
                                    data.dataSplit = true;
                                    break;
                                case "Absent Silhouettes":
                                    if (!data.dataSplit) //only use if we didn't already set the data split version
                                    {
                                        Setting_Absent.Width = new GridLength(1, GridUnitType.Star);
                                        SpacerValue--;
                                    }
                                    break;
                                case "Sephiroth":
                                    Setting_Sephiroth.Width = new GridLength(1, GridUnitType.Star);
                                    SpacerValue--;
                                    break;
                                case "Lingering Will (Terra)":
                                    Setting_Terra.Width = new GridLength(1, GridUnitType.Star);
                                    SpacerValue--;
                                    break;
                                case "Data Organization XIII":
                                    Setting_Datas.Width = new GridLength(1, GridUnitType.Star);
                                    SpacerValue--;
                                    break;
                                case "Transport to Remembrance":
                                    Setting_Transport.Width = new GridLength(1, GridUnitType.Star);
                                    SpacerValue--;
                                    break;
                                case "Olympus Cups":
                                    Setting_Cups.Width = new GridLength(1, GridUnitType.Star);
                                    SpacerValue--;
                                    break;
                                case "Hades Paradox Cup":
                                    Setting_HadesCup.Width = new GridLength(1, GridUnitType.Star);
                                    SpacerValue--;
                                    break;
                                case "ScoreMode":
                                    data.ScoreMode = true;
                                    break;
                                case "ProgressionHints":
                                    data.UsingProgressionHints = true;
                                    break;
                                case "objectives":
                                    data.objectiveMode = true;
                                    break;
                                case "OneHour":
                                    data.oneHourMode = true;
                                    break;
                            }
                        }

                        //prevent creations hinting twice for progression
                        //if ((puzzleOn || hintObject["hintsType"].ToString() == "Path") && !data.HintRevealOrder.Contains("PuzzSynth"))
                        //{
                        //    data.HintRevealOrder.Add("PuzzSynth");
                        //}

                        Setting_Spacer.Width = new GridLength(SpacerValue, GridUnitType.Star);
                        SettingsText.Text = "Settings:";

                    }

                    if (hintObject.ContainsKey("ProgressionType"))
                    {
                        data.progressionType = hintObject["ProgressionType"].ToString();
                    }

                    if (hintObject.ContainsKey("ProgressionSettings"))
                    {
                        displays.Add("Progression");

                        var progressionSettings = JsonSerializer.Deserialize<Dictionary<string, List<int>>>(hintObject["ProgressionSettings"].ToString());

                        if (data.progressionType == "Disabled")
                            data.progressionType = "Reports";

                        foreach (var setting in progressionSettings)
                        {
                            //Console.WriteLine("progression setting found = " + setting.Key);

                            switch (setting.Key)
                            {
                                case "HintCosts":
                                    data.HintCosts.Clear();
                                    foreach (int cost in setting.Value)
                                        data.HintCosts.Add(cost);
                                    data.HintCosts.Add(data.HintCosts[data.HintCosts.Count - 1] + 1); //duplicates the last cost for logic reasons
                                    break;
                                case "SimulatedTwilightTown":
                                    data.STT_ProgressionValues.Clear();
                                    foreach (int cost in setting.Value)
                                        data.STT_ProgressionValues.Add(cost);
                                    break;
                                case "TwilightTown":
                                    data.TT_ProgressionValues.Clear();
                                    foreach (int cost in setting.Value)
                                        data.TT_ProgressionValues.Add(cost);
                                    break;
                                case "HollowBastion":
                                    data.HB_ProgressionValues.Clear();
                                    foreach (int cost in setting.Value)
                                        data.HB_ProgressionValues.Add(cost);
                                    break;
                                case "CavernofRemembrance":
                                    data.CoR_ProgressionValues.Clear();
                                    foreach (int cost in setting.Value)
                                        data.CoR_ProgressionValues.Add(cost);
                                    break;
                                case "LandofDragons":
                                    data.LoD_ProgressionValues.Clear();
                                    foreach (int cost in setting.Value)
                                        data.LoD_ProgressionValues.Add(cost);
                                    break;
                                case "BeastsCastle":
                                    data.BC_ProgressionValues.Clear();
                                    foreach (int cost in setting.Value)
                                        data.BC_ProgressionValues.Add(cost);
                                    break;
                                case "OlympusColiseum":
                                    data.OC_ProgressionValues.Clear();
                                    foreach (int cost in setting.Value)
                                        data.OC_ProgressionValues.Add(cost);
                                    break;
                                case "DisneyCastle":
                                    data.DC_ProgressionValues.Clear();
                                    foreach (int cost in setting.Value)
                                        data.DC_ProgressionValues.Add(cost);
                                    break;
                                case "Agrabah":
                                    data.AG_ProgressionValues.Clear();
                                    foreach (int cost in setting.Value)
                                        data.AG_ProgressionValues.Add(cost);
                                    break;
                                case "PortRoyal":
                                    data.PR_ProgressionValues.Clear();
                                    foreach (int cost in setting.Value)
                                        data.PR_ProgressionValues.Add(cost);
                                    break;
                                case "HalloweenTown":
                                    data.HT_ProgressionValues.Clear();
                                    foreach (int cost in setting.Value)
                                        data.HT_ProgressionValues.Add(cost);
                                    break;
                                case "PrideLands":
                                    data.PL_ProgressionValues.Clear();
                                    foreach (int cost in setting.Value)
                                        data.PL_ProgressionValues.Add(cost);
                                    break;
                                case "HundredAcreWood":
                                    data.HAW_ProgressionValues.Clear();
                                    foreach (int cost in setting.Value)
                                        data.HAW_ProgressionValues.Add(cost);
                                    break;
                                case "SpaceParanoids":
                                    data.SP_ProgressionValues.Clear();
                                    foreach (int cost in setting.Value)
                                        data.SP_ProgressionValues.Add(cost);
                                    break;
                                case "TWTNW":
                                    data.TWTNW_ProgressionValues.Clear();
                                    foreach (int cost in setting.Value)
                                        data.TWTNW_ProgressionValues.Add(cost);
                                    break;
                                case "Atlantica":
                                    data.AT_ProgressionValues.Clear();
                                    foreach (int cost in setting.Value)
                                        data.AT_ProgressionValues.Add(cost);
                                    break;
                                case "ReportBonus":
                                    data.ReportBonus = setting.Value[0];
                                    break;
                                case "WorldCompleteBonus":
                                    data.WorldCompleteBonus = setting.Value[0];
                                    break;
                                case "Levels":
                                    data.Levels_ProgressionValues.Clear();
                                    foreach (int cost in setting.Value)
                                        data.Levels_ProgressionValues.Add(cost);
                                    break;
                                case "Drives":
                                    data.Drives_ProgressionValues.Clear();
                                    foreach (int cost in setting.Value)
                                        data.Drives_ProgressionValues.Add(cost);
                                    break;
                                case "FinalXemnasReveal":
                                    data.revealFinalXemnas = setting.Value[0] == 0 ? false : true;
                                    break;
                            }
                        }
                        //data.NumOfHints = data.HintCosts.Count;
                        //set text correctly
                        ProgressionCollectedValue.Visibility = Visibility.Visible;
                        ProgressionCollectedBar.Visibility = Visibility.Visible;
                        ProgressionCollectedValue.Text = "0";
                        ProgressionTotalValue.Text = data.HintCosts[0].ToString();
                    }

                    //gen objective window grid
                    if (objWindow.objGrid != null)
                        objWindow.objGrid.Children.Clear();


                    if (OneHourOption.IsChecked)
                    {
                        data.oneHourMode = true;
                        data.BossHomeHinting = true;

                        // turn off other game modes
                        data.dartsMode = false;
                    }
                    if (DartsOption.IsChecked)
                    {
                        data.dartsMode = true;

                        // turn off other game modes
                        data.oneHourMode = false;
                    }

                    if (data.objectiveMode)
                        objWindow.GenerateObjGrid(hintObject);
                    else if (data.oneHourMode || data.dartsMode)
                        objWindow.GenerateCustomObjGrid();
                    else
                        objWindow.UpdateGridBanner(false, "NO OBJECTIVES TO LOAD", "/", "Banner_Red");

                    switch (hintObject["hintsType"].ToString())
                    {
                        case "Shananas":
                            {
                                SetMode(Mode.OpenKHShanHints);
                                ShanHints(hintObject);
                            }
                            break;
                        case "JSmartee":
                            {
                                SetMode(Mode.OpenKHJsmarteeHints);
                                JsmarteeHints(hintObject);
                            }
                            break;
                        case "Points":
                            {
                                SetMode(Mode.PointsHints);
                                PointsHints(hintObject);
                            }
                            break;
                        case "Path":
                            {
                                SetMode(Mode.PathHints);
                                PathHints(hintObject);
                            }
                            break;
                        case "Spoiler":
                            {
                                SetMode(Mode.SpoilerHints);
                                SpoilerHints(hintObject);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            //replace the hint text with the one in the save
            //why? i dunno might be important incase the way i gen boss hints changes or somethin
            //if (Savefile.ContainsKey("Reports"))
            {
                data.reportInformation = JsonSerializer.Deserialize<List<Tuple<string, string, int>>>(Savefile["Reports"].ToString());
                data.reportLocations = JsonSerializer.Deserialize<List<string>>(Savefile["ReportLoc"].ToString());
                data.progBossInformation = JsonSerializer.Deserialize<List<Tuple<string, string, string>>>(Savefile["ProgBossInfo"].ToString());

                //fix reports attempts
                var attempts = JsonSerializer.Deserialize<int[]>(Savefile["Attemps"].ToString());
                string[] failNames = new string[4] { "Fail3", "Fail2", "Fail1", "Fail0" };

                for (int i = 0; i < 13; ++i)
                {
                    data.ReportAttemptVisual[i].SetResourceReference(ContentControl.ContentProperty, failNames[attempts[i]]);
                    data.reportAttempts[i] = attempts[i];
                }
            }

            //forced final check (unsure if this will actually help with it not mistracking)
            if (Savefile.ContainsKey("ForcedFinal"))
            {
                string forced = Savefile["ForcedFinal"].ToString().ToLower();
                if (forced == "true")
                    data.forcedFinal = true;
                else
                    data.forcedFinal = false;
            }

            //Update grid tracker with settings from save
            if (Savefile.ContainsKey("BoardSettings"))
            {
                gridWindow.UploadCardSetting(Savefile["BoardSettings"].ToString());
            }

            //track obtained items
            if (Savefile.ContainsKey("Worlds"))
            {
                var worlds = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(Savefile["Worlds"].ToString());
                foreach (var world in worlds)
                {
                    var itemlist = JsonSerializer.Deserialize<List<string>>(world.Value["Items"].ToString());
                    foreach (string item in itemlist)
                    {
                        WorldGrid grid = FindName(world.Key + "Grid") as WorldGrid;
                        Item importantCheck = FindName(item) as Item;
                        if (grid.ReportHandler(importantCheck))
                        {
                            //add items, skip ghosts. ghosts are always added by reports anyway
                            if (!item.StartsWith("Ghost_"))
                                grid.Add_Item(importantCheck);
                        }
                    }
                }
            }

            //track events/progression
            if (Savefile.ContainsKey("Events"))
            {
                var eventlist = JsonSerializer.Deserialize<List<Tuple<string, int, int, int, int, int>>>(Savefile["Events"].ToString());
                for (int i = 0; i < eventlist.Count; ++i)
                {
                    UpdateWorldProgress(null, true, eventlist[i]);
                }
            }

            //track boss events
            if (Savefile.ContainsKey("BossEvents") && data.BossRandoFound)
            {
                var bossEventlist = JsonSerializer.Deserialize<List<Tuple<string, int, int, int, int, int>>>(Savefile["BossEvents"].ToString());
                for (int i = 0; i < bossEventlist.Count; ++i)
                {
                    GetBoss(null, true, bossEventlist[i]);
                }
            }

            //fix counters
            if (Savefile.ContainsKey("Counters"))
            {
                var counters = JsonSerializer.Deserialize<int[]>(Savefile["Counters"].ToString());
                for (int i = 0; i < counters.Length; ++i)
                {
                    if (i < 5)
                    {
                        FakeDrivesProgressionBonus(i, counters[i]);
                    }
                    else if (i == 5)
                    {
                        //need to add sora levels one at a time to get points correctly
                        for (int l = 0; l < counters[i]; ++l)
                        {
                            FakeLevelsProgressionBonus(l + 1);
                        }
                    }
                }
                DeathCounter = counters[5];
                data.usedPages = counters[6];
            }

            //check hash
            if (Savefile.ContainsKey("SeedHash"))
            {
                if (Savefile["SeedHash"] != null)
                {
                    try
                    {
                        var hash = JsonSerializer.Deserialize<string[]>(Savefile["SeedHash"].ToString());
                        data.seedHashVisual = hash;

                        //Set Icons
                        HashIcon1.SetResourceReference(ContentProperty, hash[0]);
                        HashIcon2.SetResourceReference(ContentProperty, hash[1]);
                        HashIcon3.SetResourceReference(ContentProperty, hash[2]);
                        HashIcon4.SetResourceReference(ContentProperty, hash[3]);
                        HashIcon5.SetResourceReference(ContentProperty, hash[4]);
                        HashIcon6.SetResourceReference(ContentProperty, hash[5]);
                        HashIcon7.SetResourceReference(ContentProperty, hash[6]);
                        data.SeedHashLoaded = true;

                        //make visible
                        if (SeedHashOption.IsChecked)
                        {
                            SetHintText("");
                            HashGrid.Visibility = Visibility.Visible;
                        }
                    }
                    catch
                    {
                        data.seedHashVisual = null;
                        HashGrid.Visibility = Visibility.Hidden;
                        App.logger?.Record("error while trying to parse seed hash. text corrupted?");
                    }
                }

            }

            //end of loading
            data.saveFileLoaded = true;
            SetTimerStuff();
        }

        private string ScrambleText(string input, bool scramble)
        {
            //scrambles/unscrambles input text based on a seed
            //why have this? i dunno i suppose to make saves more "secure"
            //figure if people really want to cheat they would have to look at this code
            Random r = new Random(16964); //why this number? who knows... (let me know if you figure it out lol)
            if (scramble)
            {
                char[] chars = input.ToArray();
                for (int i = 0; i < chars.Length; i++)
                {
                    int randomIndex = r.Next(0, chars.Length);
                    char temp = chars[randomIndex];
                    chars[randomIndex] = chars[i];
                    chars[i] = temp;
                }
                return new string(chars);
            }
            else
            {
                char[] scramChars = input.ToArray();
                List<int> swaps = new List<int>();
                for (int i = 0; i < scramChars.Length; i++)
                {
                    swaps.Add(r.Next(0, scramChars.Length));
                }
                for (int i = scramChars.Length - 1; i >= 0; i--)
                {
                    char temp = scramChars[swaps[i]];
                    scramChars[swaps[i]] = scramChars[i];
                    scramChars[i] = temp;
                }
                return new string(scramChars);
            }
        }

        //trigger autotracker specific stuff with save loading
        private void FakeDrivesProgressionBonus(int drive, int level)
        {
            if (!data.UsingProgressionHints)
                return;

            while (drive == 0 && (level > data.DriveLevels[0]))
            {
                AddProgressionPoints(data.Drives_ProgressionValues[data.DriveLevels[0] - 1]);
                data.DriveLevels[0]++;
            }
            while (drive == 1 && (level > data.DriveLevels[1]))
            {
                AddProgressionPoints(data.Drives_ProgressionValues[data.DriveLevels[1] - 1]);
                data.DriveLevels[1]++;
            }
            while (drive == 2 && (level > data.DriveLevels[2]))
            {
                AddProgressionPoints(data.Drives_ProgressionValues[data.DriveLevels[2] - 1]);
                data.DriveLevels[2]++;
            }
            while (drive == 3 && (level > data.DriveLevels[3]))
            {
                AddProgressionPoints(data.Drives_ProgressionValues[data.DriveLevels[3] - 1]);
                data.DriveLevels[3]++;
            }
            while (drive == 4 && (level > data.DriveLevels[4]))
            {
                AddProgressionPoints(data.Drives_ProgressionValues[data.DriveLevels[4] - 1]);
                data.DriveLevels[4]++;
            }
        }

        private void FakeLevelsProgressionBonus(int level)
        {
            //if sora's current level is great than the max specified level (usually 50), then do nothing
            if (level > (data.Levels_ProgressionValues.Count * 10) || !data.UsingProgressionHints)
                return;

            //every 10 levels, reward the player the progression points for that part
            while (level > data.NextLevelMilestone)
            {
                data.NextLevelMilestone += 10;
                AddProgressionPoints(data.Levels_ProgressionValues[data.LevelsPreviousIndex++]);
            }
        }

        /// 
        /// Load hints
        ///

        private void OpenKHSeed(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                DefaultExt = ".zip",
                Filter = "OpenKH Seeds (*.zip)|*.zip",
                Title = "Select Seed File"
            };
            if (openFileDialog.ShowDialog() == true)
                OpenKHSeed(openFileDialog.FileName);
        }

        private void OpenKHSeed(string filename, bool extractedSeed = false)
        {
            if (!InProgressCheck("seed"))
                return;

            StreamReader reader = null;

            ZipArchive archive = null;
            ZipArchiveEntry hintsfile = null;
            ZipArchiveEntry hashfile = null;
            ZipArchiveEntry enemyfile = null;

            string hintsfileEx = null;
            string hashfileEx = null;
            string enemyfileEx = null;


            //load seed files
            if (extractedSeed)
            {
                string[] allFiles = System.IO.Directory.GetFiles(filename, "*.*", SearchOption.AllDirectories);
                foreach (string file in allFiles)
                {
                    //Check if the needed tracker file types are detected
                    if (file.Contains("HintFile.Hints"))
                    {
                        hintsfileEx = file;
                    }
                    else if (file.Contains("enemies.rando"))
                    {
                        enemyfileEx = file;
                    }
                    else if (file.Contains("randoseed-hash-icons.csv"))
                    {
                        hashfileEx = file;
                    }
                }
            }
            else
            {
                //read files from zip
                archive = ZipFile.OpenRead(filename);
                foreach (var entry in archive.Entries)
                {
                    switch (entry.Name)
                    {
                        case "HintFile.Hints":
                            hintsfile = entry;
                            break;
                        case "enemies.rando":
                            enemyfile = entry;
                            break;
                        case "randoseed-hash-icons.csv":
                            hashfile = entry;
                            break;
                        default:
                            break;
                    }
                }
            }

            //Quick Check for Generator version used
            if (hintsfile != null)
                reader = new StreamReader(hintsfile.Open());
            else if (hintsfileEx != null)
                reader = new StreamReader(hintsfileEx);
            else
                reader = null;

            if (reader != null)
            {
                string hintText = Encoding.UTF8.GetString(Convert.FromBase64String(reader.ReadToEnd()));
                Dictionary<string, object> hintObject = JsonSerializer.Deserialize<Dictionary<string, object>>(hintText);
                string version = "";
                reader.Close();

                if (hintObject.ContainsKey("generatorVersion"))
                {
                    version = hintObject["generatorVersion"].ToString();
                }

                if (version == "" || version.StartsWith("3.0"))
                {
                    MessageBox.Show("Update KH2 Randomizer Seed Gen to version 3.1 or above or use an older Tracker version to load this seed.", "Unsupported Seed!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            //load seed hash
            if (hashfile != null)
                reader = new StreamReader(hashfile.Open());
            else if (hashfileEx != null)
                reader = new StreamReader(hashfileEx);
            else
                reader = null;

            if (reader != null)
            {
                string[] separatingStrings = { "," };
                string text = reader.ReadToEnd();
                string[] hash = text.Split(separatingStrings, System.StringSplitOptions.RemoveEmptyEntries);
                reader.Close();

                //Set Icons
                HashIcon1.SetResourceReference(ContentProperty, hash[0]);
                HashIcon2.SetResourceReference(ContentProperty, hash[1]);
                HashIcon3.SetResourceReference(ContentProperty, hash[2]);
                HashIcon4.SetResourceReference(ContentProperty, hash[3]);
                HashIcon5.SetResourceReference(ContentProperty, hash[4]);
                HashIcon6.SetResourceReference(ContentProperty, hash[5]);
                HashIcon7.SetResourceReference(ContentProperty, hash[6]);
                data.SeedHashLoaded = true;
                data.seedHashVisual = hash;

                //make visible
                if (SeedHashOption.IsChecked)
                {
                    SetHintText("");
                    HashGrid.Visibility = Visibility.Visible;
                }

                HashToSeed(hash);
            }

            //load boss enemy files
            if (enemyfile != null)
                reader = new StreamReader(enemyfile.Open());
            else if (enemyfileEx != null)
                reader = new StreamReader(enemyfileEx);
            else
                reader = null;

            if (reader != null)
            {
                data.BossRandoFound = true;
                data.openKHBossText = reader.ReadToEnd();
                var enemyText = Encoding.UTF8.GetString(Convert.FromBase64String(data.openKHBossText));
                try
                {
                    var enemyObject = JsonSerializer.Deserialize<Dictionary<string, object>>(enemyText);
                    var bosses = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(enemyObject["BOSSES"].ToString());

                    foreach (var bosspair in bosses)
                    {
                        string bossOrig = bosspair["original"].ToString();
                        string bossRepl = bosspair["new"].ToString();

                        data.BossList.Add(bossOrig, bossRepl);
                    }

                    bunterCheck(bosses);
                }
                catch
                {
                    data.BossRandoFound = false;
                    data.openKHBossText = "None";
                    App.logger?.Record("error while trying to parse bosses.");
                }

                reader.Close();
            }

            //load hints file
            if (hintsfile != null)
                reader = new StreamReader(hintsfile.Open());
            else if (hintsfileEx != null)
                reader = new StreamReader(hintsfileEx);
            else
                reader = null;

            if (reader != null)
            {
                data.openKHHintText = reader.ReadToEnd();
                var hintText = Encoding.UTF8.GetString(Convert.FromBase64String(data.openKHHintText));
                var hintObject = JsonSerializer.Deserialize<Dictionary<string, object>>(hintText);
                var settings = new List<string>();
                var hintableItems = new List<string>(JsonSerializer.Deserialize<List<string>>(hintObject["hintableItems"].ToString()));
                var startingItems = new List<int>(JsonSerializer.Deserialize<List<int>>(hintObject["startingInventory"].ToString()));

                data.ShouldResetHash = false;

                if (hintObject.ContainsKey("emblems"))
                {
                    data.EmblemMode = true;
                    Dictionary<string, int> emblemValues = new Dictionary<string, int>(JsonSerializer.Deserialize<Dictionary<string, int>>(hintObject["emblems"].ToString()));
                    EmblemTotalValue.Text = emblemValues["num_emblems_needed"].ToString();
                    displays.Add("Emblems");
                }

                if (hintObject.ContainsKey("settings"))
                {
                    settings = JsonSerializer.Deserialize<List<string>>(hintObject["settings"].ToString());

                    #region Settings

                    TornPagesToggle(false);
                    AbilitiesToggle(false);
                    ReportsToggle(false);
                    ExtraChecksToggle(false);
                    VisitLockToggle(false);
                    ChestLockToggle(false);
                    foreach (string item in hintableItems)
                    {
                        switch (item)
                        {
                            case "page":
                                TornPagesToggle(true);
                                break;
                            case "ability":
                                AbilitiesToggle(true);
                                break;
                            case "report":
                                ReportsToggle(true);
                                break;
                            case "other":
                                ExtraChecksToggle(true);
                                break;
                            case "visit":
                                VisitLockToggle(true);
                                break;
                            case "keyblade":
                                ChestLockToggle(true);
                                break;
                            case "proof":
                            case "magic":
                            case "form":
                            case "summon":
                            default:
                                break;
                        }
                    }


                    //item settings
                    PromiseCharmToggle(false);
                    AntiFormToggle(false);

                    //world settings
                    SoraHeartToggle(true);
                    DrivesToggle(false);
                    SimulatedToggle(false);
                    TwilightTownToggle(false);
                    HollowBastionToggle(false);
                    BeastCastleToggle(false);
                    OlympusToggle(false);
                    AgrabahToggle(false);
                    LandofDragonsToggle(false);
                    DisneyCastleToggle(false);
                    PrideLandsToggle(false);
                    PortRoyalToggle(false);
                    HalloweenTownToggle(false);
                    SpaceParanoidsToggle(false);
                    TWTNWToggle(false);
                    HundredAcreWoodToggle(false);
                    AtlanticaToggle(false);
                    PuzzleToggle(false);
                    SynthToggle(false);

                    //progression hints GoA Current Hint Count
                    data.WorldsData["GoA"].value.Visibility = Visibility.Hidden;

                    //settings visuals
                    SettingRow.Height = new GridLength(0.5, GridUnitType.Star);
                    Setting_BetterSTT.Width = new GridLength(0, GridUnitType.Star);
                    Setting_Level_01.Width = new GridLength(0, GridUnitType.Star);
                    Setting_Level_50.Width = new GridLength(0, GridUnitType.Star);
                    Setting_Level_99.Width = new GridLength(0, GridUnitType.Star);
                    Setting_Absent.Width = new GridLength(0, GridUnitType.Star);
                    Setting_Absent_Split.Width = new GridLength(0, GridUnitType.Star);
                    Setting_Datas.Width = new GridLength(0, GridUnitType.Star);
                    Setting_Sephiroth.Width = new GridLength(0, GridUnitType.Star);
                    Setting_Terra.Width = new GridLength(0, GridUnitType.Star);
                    Setting_Cups.Width = new GridLength(0, GridUnitType.Star);
                    Setting_HadesCup.Width = new GridLength(0, GridUnitType.Star);
                    Setting_Cavern.Width = new GridLength(0, GridUnitType.Star);
                    Setting_Transport.Width = new GridLength(0, GridUnitType.Star);
                    Double SpacerValue = 10;
                    #endregion

                    //load settings from hints
                    foreach (string setting in settings)
                    {
                        //Console.WriteLine("setting found = " + setting);
                        switch (setting)
                        {
                            //items
                            case "PromiseCharm":
                                PromiseCharmToggle(true);
                                break;
                            case "Anti-Form":
                                AntiFormToggle(true);
                                break;
                            //worlds
                            case "Level":
                                SoraHeartToggle(false);
                                SoraLevel01Toggle(true);
                                //AbilitiesToggle(true);
                                Setting_Level_01.Width = new GridLength(1.5, GridUnitType.Star);
                                SpacerValue--;
                                break;
                            case "ExcludeFrom50":
                                SoraLevel50Toggle(true);
                                //AbilitiesToggle(true);
                                Setting_Level_50.Width = new GridLength(1.5, GridUnitType.Star);
                                SpacerValue--;
                                data.HintRevealOrder.Add("SorasHeart");
                                break;
                            case "ExcludeFrom99":
                                SoraLevel99Toggle(true);
                                //AbilitiesToggle(true);
                                Setting_Level_99.Width = new GridLength(1.5, GridUnitType.Star);
                                SpacerValue--;
                                data.HintRevealOrder.Add("SorasHeart");
                                break;
                            case "Simulated Twilight Town":
                                SimulatedToggle(true);
                                data.enabledWorlds.Add("STT");
                                data.HintRevealOrder.Add("SimulatedTwilightTown");
                                break;
                            case "Hundred Acre Wood":
                                HundredAcreWoodToggle(true);
                                data.enabledWorlds.Add("HundredAcreWood");
                                data.HintRevealOrder.Add("HundredAcreWood");
                                break;
                            case "Atlantica":
                                AtlanticaToggle(true);
                                data.enabledWorlds.Add("Atlantica");
                                data.HintRevealOrder.Add("Atlantica");
                                break;
                            case "Puzzle":
                                PuzzleToggle(true);
                                if (!data.HintRevealOrder.Contains("PuzzSynth"))
                                    data.HintRevealOrder.Add("PuzzSynth");
                                data.puzzlesOn = true;
                                break;
                            case "Synthesis":
                                SynthToggle(true);
                                if (!data.HintRevealOrder.Contains("PuzzSynth"))
                                    data.HintRevealOrder.Add("PuzzSynth");
                                //data.synthOn = true;
                                break;
                            case "Form Levels":
                                DrivesToggle(true);
                                data.HintRevealOrder.Add("DriveForms");
                                break;
                            case "Land of Dragons":
                                LandofDragonsToggle(true);
                                data.enabledWorlds.Add("LoD");
                                data.HintRevealOrder.Add("LandofDragons");
                                break;
                            case "Beast's Castle":
                                BeastCastleToggle(true);
                                data.enabledWorlds.Add("BC");
                                data.HintRevealOrder.Add("BeastsCastle");
                                break;
                            case "Hollow Bastion":
                                HollowBastionToggle(true);
                                data.enabledWorlds.Add("HB");
                                data.HintRevealOrder.Add("HollowBastion");
                                break;
                            case "Twilight Town":
                                TwilightTownToggle(true);
                                data.enabledWorlds.Add("TT");
                                data.HintRevealOrder.Add("TwilightTown");
                                break;
                            case "The World That Never Was":
                                TWTNWToggle(true);
                                data.enabledWorlds.Add("TWTNW");
                                data.HintRevealOrder.Add("TWTNW");
                                break;
                            case "Space Paranoids":
                                SpaceParanoidsToggle(true);
                                data.enabledWorlds.Add("SP");
                                data.HintRevealOrder.Add("SpaceParanoids");
                                break;
                            case "Port Royal":
                                PortRoyalToggle(true);
                                data.enabledWorlds.Add("PR");
                                data.HintRevealOrder.Add("PortRoyal");
                                break;
                            case "Olympus Coliseum":
                                OlympusToggle(true);
                                data.enabledWorlds.Add("OC");
                                data.HintRevealOrder.Add("OlympusColiseum");
                                break;
                            case "Agrabah":
                                AgrabahToggle(true);
                                data.enabledWorlds.Add("AG");
                                data.HintRevealOrder.Add("Agrabah");
                                break;
                            case "Halloween Town":
                                HalloweenTownToggle(true);
                                data.enabledWorlds.Add("HT");
                                data.HintRevealOrder.Add("HalloweenTown");
                                break;
                            case "Pride Lands":
                                PrideLandsToggle(true);
                                data.enabledWorlds.Add("PL");
                                data.HintRevealOrder.Add("PrideLands");
                                break;
                            case "Disney Castle / Timeless River":
                                DisneyCastleToggle(true);
                                data.enabledWorlds.Add("DC");
                                data.HintRevealOrder.Add("DisneyCastle");
                                break;
                            //settings
                            case "better_stt":
                                Setting_BetterSTT.Width = new GridLength(1.1, GridUnitType.Star);
                                SpacerValue--;
                                break;
                            case "Cavern of Remembrance":
                                Setting_Cavern.Width = new GridLength(1, GridUnitType.Star);
                                SpacerValue--;
                                break;
                            case "Data Split":
                                Setting_Absent_Split.Width = new GridLength(1, GridUnitType.Star);
                                SpacerValue--;
                                data.dataSplit = true;
                                break;
                            case "Absent Silhouettes":
                                if (!data.dataSplit) //only use if we didn't already set the data split version
                                {
                                    Setting_Absent.Width = new GridLength(1, GridUnitType.Star);
                                    SpacerValue--;
                                }
                                break;
                            case "Sephiroth":
                                Setting_Sephiroth.Width = new GridLength(1, GridUnitType.Star);
                                SpacerValue--;
                                break;
                            case "Lingering Will (Terra)":
                                Setting_Terra.Width = new GridLength(1, GridUnitType.Star);
                                SpacerValue--;
                                break;
                            case "Data Organization XIII":
                                Setting_Datas.Width = new GridLength(1, GridUnitType.Star);
                                SpacerValue--;
                                break;
                            case "Transport to Remembrance":
                                Setting_Transport.Width = new GridLength(1, GridUnitType.Star);
                                SpacerValue--;
                                break;
                            case "Olympus Cups":
                                Setting_Cups.Width = new GridLength(1, GridUnitType.Star);
                                SpacerValue--;
                                break;
                            case "Hades Paradox Cup":
                                Setting_HadesCup.Width = new GridLength(1, GridUnitType.Star);
                                SpacerValue--;
                                break;
                            case "ScoreMode":
                                data.ScoreMode = true;
                                break;
                            case "ProgressionHints":
                                data.UsingProgressionHints = true;
                                break;
                            case "objectives":
                                data.objectiveMode = true;
                                break;
                            case "OneHour":
                                data.oneHourMode = true;
                                break;
                        }
                    }

                    //adjust based on starting items
                    //TODO: first visit locks only, maybe do things with aux checks or potentially every item
                    //VisitLockCheck(startingItems);

                    //prevent creations hinting twice for progression
                    //if ((puzzleOn || hintObject["hintsType"].ToString() == "Path") && !data.HintRevealOrder.Contains("PuzzSynth"))
                    //{
                    //    data.HintRevealOrder.Add("PuzzSynth");
                    //}

                    Setting_Spacer.Width = new GridLength(SpacerValue, GridUnitType.Star);
                    SettingsText.Text = "Settings:";

                }

                if (hintObject.ContainsKey("ProgressionType"))
                {
                    data.progressionType = hintObject["ProgressionType"].ToString();
                }

                if (hintObject.ContainsKey("ProgressionSettings"))
                {
                    displays.Add("Progression");

                    var progressionSettings = JsonSerializer.Deserialize<Dictionary<string, List<int>>>(hintObject["ProgressionSettings"].ToString());

                    if (data.progressionType == "Disabled")
                        data.progressionType = "Reports";

                    foreach (var setting in progressionSettings)
                    {
                        //Console.WriteLine("progression setting found = " + setting.Key);

                        switch (setting.Key)
                        {
                            case "HintCosts":
                                data.HintCosts.Clear();
                                foreach (int cost in setting.Value)
                                    data.HintCosts.Add(cost);
                                data.HintCosts.Add(data.HintCosts[data.HintCosts.Count - 1] + 1); //duplicates the last cost for logic reasons
                                break;
                            case "SimulatedTwilightTown":
                                data.STT_ProgressionValues.Clear();
                                foreach (int cost in setting.Value)
                                    data.STT_ProgressionValues.Add(cost);
                                break;
                            case "TwilightTown":
                                data.TT_ProgressionValues.Clear();
                                foreach (int cost in setting.Value)
                                    data.TT_ProgressionValues.Add(cost);
                                break;
                            case "HollowBastion":
                                data.HB_ProgressionValues.Clear();
                                foreach (int cost in setting.Value)
                                    data.HB_ProgressionValues.Add(cost);
                                break;
                            case "CavernofRemembrance":
                                data.CoR_ProgressionValues.Clear();
                                foreach (int cost in setting.Value)
                                    data.CoR_ProgressionValues.Add(cost);
                                break;
                            case "LandofDragons":
                                data.LoD_ProgressionValues.Clear();
                                foreach (int cost in setting.Value)
                                    data.LoD_ProgressionValues.Add(cost);
                                break;
                            case "BeastsCastle":
                                data.BC_ProgressionValues.Clear();
                                foreach (int cost in setting.Value)
                                    data.BC_ProgressionValues.Add(cost);
                                break;
                            case "OlympusColiseum":
                                data.OC_ProgressionValues.Clear();
                                foreach (int cost in setting.Value)
                                    data.OC_ProgressionValues.Add(cost);
                                break;
                            case "DisneyCastle":
                                data.DC_ProgressionValues.Clear();
                                foreach (int cost in setting.Value)
                                    data.DC_ProgressionValues.Add(cost);
                                break;
                            case "Agrabah":
                                data.AG_ProgressionValues.Clear();
                                foreach (int cost in setting.Value)
                                    data.AG_ProgressionValues.Add(cost);
                                break;
                            case "PortRoyal":
                                data.PR_ProgressionValues.Clear();
                                foreach (int cost in setting.Value)
                                    data.PR_ProgressionValues.Add(cost);
                                break;
                            case "HalloweenTown":
                                data.HT_ProgressionValues.Clear();
                                foreach (int cost in setting.Value)
                                    data.HT_ProgressionValues.Add(cost);
                                break;
                            case "PrideLands":
                                data.PL_ProgressionValues.Clear();
                                foreach (int cost in setting.Value)
                                    data.PL_ProgressionValues.Add(cost);
                                break;
                            case "HundredAcreWood":
                                data.HAW_ProgressionValues.Clear();
                                foreach (int cost in setting.Value)
                                    data.HAW_ProgressionValues.Add(cost);
                                break;
                            case "SpaceParanoids":
                                data.SP_ProgressionValues.Clear();
                                foreach (int cost in setting.Value)
                                    data.SP_ProgressionValues.Add(cost);
                                break;
                            case "TWTNW":
                                data.TWTNW_ProgressionValues.Clear();
                                foreach (int cost in setting.Value)
                                    data.TWTNW_ProgressionValues.Add(cost);
                                break;
                            case "Atlantica":
                                data.AT_ProgressionValues.Clear();
                                foreach (int cost in setting.Value)
                                    data.AT_ProgressionValues.Add(cost);
                                break;
                            case "ReportBonus":
                                data.ReportBonus = setting.Value[0];
                                break;
                            case "WorldCompleteBonus":
                                data.WorldCompleteBonus = setting.Value[0];
                                break;
                            case "Levels":
                                data.Levels_ProgressionValues.Clear();
                                foreach (int cost in setting.Value)
                                    data.Levels_ProgressionValues.Add(cost);
                                break;
                            case "Drives":
                                data.Drives_ProgressionValues.Clear();
                                foreach (int cost in setting.Value)
                                    data.Drives_ProgressionValues.Add(cost);
                                break;
                            case "FinalXemnasReveal":
                                data.revealFinalXemnas = setting.Value[0] == 0 ? false : true;
                                break;
                        }
                    }
                    //data.NumOfHints = data.HintCosts.Count;
                    //set text correctly
                    ProgressionCollectedValue.Visibility = Visibility.Visible;
                    ProgressionCollectedBar.Visibility = Visibility.Visible;
                    ProgressionCollectedValue.Text = "0";
                    ProgressionTotalValue.Text = data.HintCosts[0].ToString();
                }
                
                if (hintObject.ContainsKey("level_data"))
                {
                    var levelData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(hintObject["level_data"].ToString());

                    try
                    {
                        foreach (var list in levelData)
                        {
                            if (list.Key == "Valor Level")
                            {
                                levelListHelper(list.Value, data.seedvalorChecks);
                                continue;
                            }
                            if (list.Key == "Wisdom Level")
                            {
                                levelListHelper(list.Value, data.seedwisdomChecks);
                                continue;
                            }
                            if (list.Key == "Limit Level")
                            {
                                levelListHelper(list.Value, data.seedlimitChecks);
                                continue;
                            }
                            if (list.Key == "Master Level")
                            {
                                levelListHelper(list.Value, data.seedmasterChecks);
                                continue;
                            }
                            if (list.Key == "Final Level")
                            {
                                levelListHelper(list.Value, data.seedfinalChecks);
                                continue;
                            }
                            if (list.Key == "Level")
                            {
                                foreach (string weapon in list.Value.Keys)
                                {
                                    var levelList = JsonSerializer.Deserialize<Dictionary<string, object>>(list.Value[weapon].ToString());

                                    if (weapon == "Sword")
                                    {
                                        levelListHelper(levelList, data.seedswordChecks);
                                        continue;
                                    }
                                    if (weapon == "Staff")
                                    {
                                        levelListHelper(levelList, data.seedstaffChecks);
                                        continue;
                                    }
                                    if (weapon == "Shield")
                                    {
                                        levelListHelper(levelList, data.seedshieldChecks);
                                        continue;
                                    }
                                }
                            }

                            data.seedLevelChecks = true;
                        }
                    }
                    catch
                    {
                        //Console.WriteLine("Level list not as expected, will fall back to ram loading");
                        data.seedLevelChecks = false;
                    }
                }

                //gen objective window grid
                if (objWindow.objGrid != null)
                    objWindow.objGrid.Children.Clear();

                if (OneHourOption.IsChecked)
                {
                    data.oneHourMode = true;
                    data.BossHomeHinting = true;

                    // turn off other game modes
                    data.dartsMode = false;
                }
                if (DartsOption.IsChecked)
                {
                    data.dartsMode = true;

                    // turn off other game modes
                    data.oneHourMode = false;
                }

                if (data.objectiveMode)
                    objWindow.GenerateObjGrid(hintObject);
                else if (data.oneHourMode || data.dartsMode)
                    objWindow.GenerateCustomObjGrid();
                else
                    objWindow.UpdateGridBanner(false, "NO OBJECTIVES TO LOAD", "/", "Banner_Red");

                switch (hintObject["hintsType"].ToString())
                {
                    case "Shananas":
                        {
                            SetMode(Mode.OpenKHShanHints);
                            ShanHints(hintObject);
                        }
                        break;
                    case "JSmartee":
                        {
                            SetMode(Mode.OpenKHJsmarteeHints);
                            JsmarteeHints(hintObject);
                        }
                        break;
                    case "Points":
                        {
                            SetMode(Mode.PointsHints);
                            PointsHints(hintObject);
                        }
                        break;
                    case "Path":
                        {
                            SetMode(Mode.PathHints);
                            PathHints(hintObject);
                        }
                        break;
                    case "Spoiler":
                        {
                            SetMode(Mode.SpoilerHints);
                            SpoilerHints(hintObject);
                        }
                        break;
                    default:
                        break;
                }

                data.hintsLoaded = true;

                reader.Close();
            }

            data.seedLoaded = true;
            toggleState(false);


            // regenerate the grid tracker
            gridWindow.grid.Children.Clear();
            gridWindow.GenerateGrid(gridWindow.numRows, gridWindow.numColumns, null, false);

            archive?.Dispose();

            //show icecream and sketches in 1hour mode
            if (data.oneHourMode)
            {
                //VisitLockToggle(false);
                //VisitLockToggle2(false);

                Grid VisitRow2 = ItemPool.Children[5] as Grid;
                double[] resetList = {
                    0.0,
                    0.6, 1.0,
                    0.1,
                    0.6, 1.0,
                    0.1,
                    0.6, 1.0,
                    0.1,
                    0.6, 1.0,
                    0.0,
                    0.0, 1.0};
                for (int i = 11; i < VisitRow2.ColumnDefinitions.Count; i++)
                {
                    if (i <= 14)
                        VisitRow2.ColumnDefinitions[i].Width = new GridLength(resetList[i], GridUnitType.Star);
                }

                HandleItemToggle(true, data.VisitLocks[24], false);
                HandleItemToggle(true, data.VisitLocks[25], false);
            }

            if (data.wasTracking)
            {
                InitTracker();
            }
            SetTimerStuff();
        }

        //here to prevent a lot of redundancy
        private void levelListHelper (Dictionary<string, object> seedList, List<Tuple<int, string>> dataList)
        {
            foreach (var entry in seedList)
            {
                int level = int.Parse(entry.Key);
                int item = int.Parse(entry.Value.ToString());
                if (data.codes.itemCodes.ContainsKey(item))
                {
                    dataList.Add(new Tuple<int, string>(level, data.codes.itemCodes[item]));
                }
            }
        }

        private void SetMode(Mode mode)
        {
            if (data.UsingProgressionHints)
            {
                data.BossHomeHinting = false;
            }

            if (mode == Mode.ShanHints || mode == Mode.OpenKHShanHints)
            {
                ModeDisplay.Header = "Shan Hints";
                data.mode = mode;
                //ReportsToggle(false);
            }
            else if (mode == Mode.JsmarteeHints || mode == Mode.OpenKHJsmarteeHints)
            {
                ModeDisplay.Header = "Jsmartee Hints";
                data.mode = mode;
                //ReportsToggle(true);
            }
            else if (mode == Mode.PointsHints)
            {
                ModeDisplay.Header = "Points Hints";
                data.mode = mode;
                //ReportsToggle(true);
                //high score mode should not be on if points hints is on
                if (data.ScoreMode == true)
                    data.ScoreMode = false;

                UpdatePointScore(0);
                ShowCheckCountToggle(null, null);
            }
            else if (mode == Mode.PathHints)
            {
                ModeDisplay.Header = "Path Hints";
                data.mode = mode;
                //ReportsToggle(true);
            }
            else if (mode == Mode.SpoilerHints)
            {
                ModeDisplay.Header = "Spoiler Hints";
                data.mode = mode;
            }

            if (data.ScoreMode && mode != Mode.PointsHints)
            {
                UpdatePointScore(0);
                ShowCheckCountToggle(null, null);

                ModeDisplay.Header += " | HSM";

                displays.Add("Score");

                //CollectionGrid.Visibility = Visibility.Collapsed;
                //ScoreGrid.Visibility = Visibility.Visible;
                //ProgressionCollectionGrid.Visibility = Visibility.Collapsed;
            }

            if (data.UsingProgressionHints)
            {
                data.WorldsData["GoA"].value.Visibility = Visibility.Visible;
                data.WorldsData["GoA"].value.Text = "0";
                if (data.progressionType == "Reports" && !data.ScoreMode)
                {
                    CollectionGrid.Visibility = Visibility.Collapsed;
                    ScoreGrid.Visibility = Visibility.Collapsed;
                    ProgressionCollectionGrid.Visibility = Visibility.Visible;
                    ChestIcon.SetResourceReference(ContentProperty, "ProgPoints");
                    ModeDisplay.Header += " | Progression";
                }
                else if (data.progressionType == "Bosses")
                {
                    CollectionGrid.Visibility = Visibility.Collapsed;
                    ScoreGrid.Visibility = Visibility.Collapsed;
                    ProgressionCollectionGrid.Visibility = Visibility.Visible;
                    ChestIcon.SetResourceReference(ContentProperty, "ProgPoints");
                    ModeDisplay.Header += " | Prog. Bosses";
                }
            }

            if (data.BossHomeHinting)
            {
                ModeDisplay.Header += " | Bosses Hint Home";
                data.WorldsData["GoA"].value.Visibility = Visibility.Visible;
                data.WorldsData["GoA"].value.Text = "0";
                GoA.SetResourceReference(ContentProperty, "OneHour");
            }

            if (data.EmblemMode)
            {
                ShowEmblemCountToggle(EmblemCountOption.IsChecked);
            }
        }

        //Turns the zip seed icon hash to a numerical based seed
        private void HashToSeed(string[] hash)
        {
            int icon1 = Codes.HashInt[hash[0]];
            int icon2 = Codes.HashInt[hash[1]];
            int icon3 = Codes.HashInt[hash[2]];
            int icon4 = Codes.HashInt[hash[3]];
            int icon5 = Codes.HashInt[hash[4]];
            int icon6 = Codes.HashInt[hash[5]];
            int icon7 = Codes.HashInt[hash[6]];

            int final = (icon1 + icon2) * (icon3 + icon4) * (icon5 + icon6) - icon7;
            data.convertedSeedHash = final;
        }

        private void OnReset(object sender, RoutedEventArgs e)
        {
            if (aTimer != null)
            {
                aTimer.Stop();
                aTimer = null;
                pcFilesLoaded = false;
            }

            if (sender != null && !AutoConnectOption.IsChecked)
                data.wasTracking = false;

            //chnage visuals based on if autotracking was done before
            if (data.wasTracking)
            {
                //connection trying visual
                Connect.Visibility = Visibility.Visible;
                Connect2.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (!AutoConnectOption.IsChecked)
                {
                    Connect.Visibility = Visibility.Collapsed;
                    Connect2.Visibility = Visibility.Collapsed;
                }

                //SettingRow.Height = new GridLength(0, GridUnitType.Star);
                FormRow.Height = new GridLength(0, GridUnitType.Star);
                Level.Visibility = Visibility.Collapsed;
                Strength.Visibility = Visibility.Collapsed;
                Magic.Visibility = Visibility.Collapsed;
                Defense.Visibility = Visibility.Collapsed;
            }

            toggleState(true);

            collectedChecks.Clear();
            newChecks.Clear();
            ModeDisplay.Header = "";
            HintTextMiddle.Text = "";
            HintTextBegin.Text = "";
            HintTextEnd.Text = "";
            data.mode = Mode.None;
            collected = 0;
            PointTotal = 0;
            data.SpoilerRevealTypes.Clear();
            data.SpoilerReportMode = false;
            data.SpoilerWorldCompletion = false;
            data.usedPages = 0;
            CollectedValue.Text = "0";
            data.ScoreMode = false;
            data.forcedFinal = false;
            data.BossRandoFound = false;
            data.dataSplit = false;
            data.BossList.Clear();
            data.bossEventLog.Clear();
            data.convertedSeedHash = 0;
            data.enabledWorlds.Clear();
            //data.seedgenVersion = "";
            //data.altFinalTracking = false;
            data.eventLog.Clear();
            data.openKHHintText = "None";
            data.openKHBossText = "None";
            data.hintsLoaded = false;
            data.seedLoaded = false;
            data.saveFileLoaded = false;
            data.firstGridOnSeedLoad = true;
            data.BossHomeHinting = false;
            data.bossHomeHintInformation.Clear();

            data.WorldOverlay.Clear();

            data.seedLevelChecks = false;
            data.seedswordChecks.Clear();
            data.seedshieldChecks.Clear();
            data.seedstaffChecks.Clear();
            data.seedvalorChecks.Clear();
            data.seedwisdomChecks.Clear();
            data.seedlimitChecks.Clear();
            data.seedmasterChecks.Clear();
            data.seedfinalChecks.Clear();

            //emblems
            EmblemGrid.Visibility = Visibility.Collapsed;
            EmblemCollectedValue.Text = "0";
            data.EmblemMode = false;
            if (objMark != null)
                objMark.Count = 0;

            //objective widow stuff
            data.objectiveMode = false;
            data.oneHourMode = false;
            objWindow.oneHourPoints = 0;
            data.earlyThroneRoom = 0;
            objWindow.endCorChest = false;
            objWindow.objectivesNeed = 0;
            objWindow.UpdateGridBanner(false, "NO OBJECTIVES TO LOAD", "/", "Banner_Red");

            //prog boss hint stuff
            BossHintTextMiddle.Text = "";
            BossHintTextBegin.Text = "";
            BossHintTextEnd.Text = "";
            data.progBossInformation.Clear();
            data.progressionType = "Disabled";
            InfoRow.Height = new GridLength(0.8, GridUnitType.Star);
            InfoTextRow.Height = new GridLength(1, GridUnitType.Star);
            BossTextRow.Height = new GridLength(0, GridUnitType.Star);
            MainTextRow.Height = new GridLength(1, GridUnitType.Star);
            HashBossSpacer.Height = new GridLength(0, GridUnitType.Star);
            DC_Row1.Height = new GridLength(0, GridUnitType.Star);
            TextRowSpacer.Height = new GridLength(0, GridUnitType.Star);
            Grid.SetColumnSpan(MainTextVB, 1);

            //clear progression hints stuff
            data.reportLocationsUsed = new List<bool>() { false, false, false, false, false, false, false, false, false, false, false, false, false };
            data.UsingProgressionHints = false;
            data.ProgressionPoints = 0;
            data.TotalProgressionPoints = 0;
            data.ReportBonus = 1;
            data.WorldCompleteBonus = 0;
            data.ProgressionCurrentHint = 0;
            data.WorldsEnabled = 0;
            data.HintRevealOrder.Clear();
            data.LevelsPreviousIndex = 0;
            data.NextLevelMilestone = 9;
            data.Levels_ProgressionValues = new List<int>() { 1, 1, 1, 2, 4 };
            data.Drives_ProgressionValues = new List<int>() { 0, 0, 0, 1, 0, 2 };
            data.DriveLevels = new List<int>() { 1, 1, 1, 1, 1 };
            data.HintRevealsStored.Clear();
            data.WorldsData["GoA"].value.Visibility = Visibility.Hidden;
            //clear last hinted green world
            if (data.previousWorldsHinted.Count >= 0)
            {
                foreach (var world in data.previousWorldsHinted)
                {
                    if (world == null || world == "")
                        continue;

                    foreach (var Box in data.WorldsData[world].top.Children.OfType<Rectangle>())
                    {
                        if (Box.Opacity != 0.9 && !Box.Name.EndsWith("SelWG"))
                            Box.Fill = (SolidColorBrush)FindResource("DefaultRec");

                        if (Box.Name.EndsWith("SelWG") && !WorldHighlightOption.IsChecked)
                            Box.Visibility = Visibility.Collapsed;
                    }
                }
            }
            data.previousWorldsHinted.Clear();
            data.StoredWorldCompleteBonus = new Dictionary<string, int>()
            {
                { "SorasHeart", 0 },
                { "DriveForms", 0 },
                { "SimulatedTwilightTown", 0 },
                { "TwilightTown", 0 },
                { "HollowBastion", 0 },
                { "BeastsCastle", 0 },
                { "OlympusColiseum", 0 },
                { "Agrabah", 0 },
                { "LandofDragons", 0 },
                { "HundredAcreWood", 0 },
                { "PrideLands", 0 },
                { "DisneyCastle", 0 },
                { "HalloweenTown", 0 },
                { "PortRoyal", 0 },
                { "SpaceParanoids", 0 },
                { "TWTNW", 0 },
                { "GoA", 0 },
                { "Atlantica", 0 },
                { "PuzzSynth", 0 }
            };
            data.STT_ProgressionValues = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8 };
            data.TT_ProgressionValues = new List<int>() { 1, 2, 3, 4, 5, 6, 7 };
            data.HB_ProgressionValues = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
            data.CoR_ProgressionValues = new List<int>() { 0, 0, 0, 0, 0 };
            data.BC_ProgressionValues = new List<int>() { 1, 2, 3, 4, 5, 6, 7 };
            data.OC_ProgressionValues = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            data.AG_ProgressionValues = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8 };
            data.LoD_ProgressionValues = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            data.HAW_ProgressionValues = new List<int>() { 1, 2, 3, 4, 5, 6 };
            data.PL_ProgressionValues = new List<int>() { 1, 2, 3, 4, 5, 6, 7 };
            data.AT_ProgressionValues = new List<int>() { 1, 2, 3 };
            data.DC_ProgressionValues = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            data.HT_ProgressionValues = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8 };
            data.PR_ProgressionValues = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            data.SP_ProgressionValues = new List<int>() { 1, 2, 3, 4, 5, 6 };
            data.TWTNW_ProgressionValues = new List<int>() { 1, 2, 3, 4, 5, 6, 7 };
            data.HintCosts = new List<int>() { 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10, 10 };

            //hotkey stuff
            data.usedHotkey = false;

            //unselect any currently selected world grid
            if (data.selected != null)
            {
                foreach (var Box in data.WorldsData[data.selected.Name].top.Children.OfType<Rectangle>())
                {
                    if (Box.Opacity != 0.9 && !Box.Name.EndsWith("SelWG"))
                        Box.Fill = (SolidColorBrush)FindResource("DefaultRec");

                    if (Box.Name.EndsWith("SelWG"))
                        Box.Visibility = Visibility.Collapsed;
                }
            }
            data.selected = null;

            //return items to itempool
            foreach (WorldData worldData in data.WorldsData.Values.ToList())
            {
                for (int j = worldData.worldGrid.Children.Count - 1; j >= 0; --j)
                {
                    Item item = worldData.worldGrid.Children[j] as Item;
                    Grid pool;

                    if (item.Name.StartsWith("Ghost_"))
                        pool = VisualTreeHelper.GetChild(ItemPool, ItemPool.Children.Count - 1) as Grid;
                    else
                        pool = data.Items[item.Name].Item2;

                    worldData.worldGrid.Children.Remove(worldData.worldGrid.Children[j]);
                    pool.Children.Add(item);

                    item.MouseDown -= item.Item_Return;
                    item.MouseEnter -= item.Report_Hover;
                    if (data.dragDrop)
                    {
                        item.MouseDoubleClick -= item.Item_Click;
                        item.MouseDoubleClick += item.Item_Click;
                        item.MouseMove -= item.Item_MouseMove;
                        item.MouseMove += item.Item_MouseMove;
                    }
                    else
                    {
                        item.MouseDown -= item.Item_MouseDown;
                        item.MouseDown += item.Item_MouseDown;
                        item.MouseUp -= item.Item_MouseUp;
                        item.MouseUp += item.Item_MouseUp;
                    }
                }
            }

            // Reset 1st column row heights
            RowDefinitionCollection rows1 = ((data.WorldsData["SorasHeart"].worldGrid.Parent as Grid).Parent as Grid).RowDefinitions;
            foreach (RowDefinition row in rows1)
            {
                // don't reset turned off worlds
                if (row.Height.Value != 0)
                    row.Height = new GridLength(1, GridUnitType.Star);
            }

            // Reset 2nd column row heights
            RowDefinitionCollection rows2 = ((data.WorldsData["DriveForms"].worldGrid.Parent as Grid).Parent as Grid).RowDefinitions;
            foreach (RowDefinition row in rows2)
            {
                // don't reset turned off worlds
                if (row.Height.Value != 0)
                    row.Height = new GridLength(1, GridUnitType.Star);
            }

            //fix puzzsynth value if it was hidden (progression hints)
            if (data.WorldsData["PuzzSynth"].value.Visibility == Visibility.Hidden)
            {
                data.WorldsData["PuzzSynth"].value.Visibility = Visibility.Visible;
            }

            foreach (var key in data.WorldsData.Keys.ToList())
            {
                data.WorldsData[key].complete = false;
                data.WorldsData[key].hintedItemList.Clear();
                data.WorldsData[key].progress = 0;

                //world cross reset
                string crossname = key + "Cross";

                if (data.WorldsData[key].top.FindName(crossname) is Image Cross)
                {
                    Cross.Visibility = Visibility.Collapsed;
                }

                //reset highlighted world
                foreach (Rectangle Box in data.WorldsData[key].top.Children.OfType<Rectangle>().Where(Box => Box.Name.EndsWith("SelWG")))
                {
                    Box.Visibility = Visibility.Collapsed;
                }
            }

            TwilightTownProgression.SetResourceReference(ContentProperty, "");
            HollowBastionProgression.SetResourceReference(ContentProperty, "");
            LandofDragonsProgression.SetResourceReference(ContentProperty, "");
            BeastsCastleProgression.SetResourceReference(ContentProperty, "");
            OlympusColiseumProgression.SetResourceReference(ContentProperty, "");
            SpaceParanoidsProgression.SetResourceReference(ContentProperty, "");
            HalloweenTownProgression.SetResourceReference(ContentProperty, "");
            PortRoyalProgression.SetResourceReference(ContentProperty, "");
            AgrabahProgression.SetResourceReference(ContentProperty, "");
            PrideLandsProgression.SetResourceReference(ContentProperty, "");
            DisneyCastleProgression.SetResourceReference(ContentProperty, "");
            HundredAcreWoodProgression.SetResourceReference(ContentProperty, "");
            SimulatedTwilightTownProgression.SetResourceReference(ContentProperty, "");
            TWTNWProgression.SetResourceReference(ContentProperty, "");
            AtlanticaProgression.SetResourceReference(ContentProperty, "");
            GoAProgression.SetResourceReference(ContentProperty, "");
            DriveFormsCap.SetResourceReference(ContentProperty, "");
            ChestIcon.SetResourceReference(ContentProperty, "Chest");

            SorasHeartWeapon.SetResourceReference(ContentProperty, "");

            NextLevelCol.Width = new GridLength(0, GridUnitType.Star);

            ValorM.Opacity = .45;
            WisdomM.Opacity = .45;
            LimitM.Opacity = .45;
            MasterM.Opacity = .45;
            FinalM.Opacity = .45;
            HighJump.Opacity = .45;
            QuickRun.Opacity = .45;
            DodgeRoll.Opacity = .45;
            AerialDodge.Opacity = .45;
            Glide.Opacity = .45;

            ValorLevel.Text = "1";
            WisdomLevel.Text = "1";
            LimitLevel.Text = "1";
            MasterLevel.Text = "1";
            FinalLevel.Text = "1";
            HighJumpLevel.Text = "";
            QuickRunLevel.Text = "";
            DodgeRollLevel.Text = "";
            AerialDodgeLevel.Text = "";
            GlideLevel.Text = "";

            fireLevel = 0;
            blizzardLevel = 0;
            thunderLevel = 0;
            cureLevel = 0;
            reflectLevel = 0;
            magnetLevel = 0;
            tornPageCount = 0;
            //munnyPouchCount = 0;

            AuronWepLevel = 0;
            MulanWepLevel = 0;
            BeastWepLevel = 0;
            JackWepLevel = 0;
            SimbaWepLevel = 0;
            SparrowWepLevel = 0;
            AladdinWepLevel = 0;
            TronWepLevel = 0;
            MembershipCardLevel = 0;
            IceCreamLevel = 0;
            RikuWepLevel = 0;
            KingsLetterLevel = 0;

            if (fire != null)
                fire.Level = 0;
            if (blizzard != null)
                blizzard.Level = 0;
            if (thunder != null)
                thunder.Level = 0;
            if (cure != null)
                cure.Level = 0;
            if (reflect != null)
                reflect.Level = 0;
            if (magnet != null)
                magnet.Level = 0;
            if (pages != null)
                pages.Quantity = 0;

            if (AuronWep != null)
                AuronWep.Level = 0;
            if (MulanWep != null)
                MulanWep.Level = 0;
            if (BeastWep != null)
                BeastWep.Level = 0;
            if (JackWep != null)
                JackWep.Level = 0;
            if (SimbaWep != null)
                SimbaWep.Level = 0;
            if (SparrowWep != null)
                SparrowWep.Level = 0;
            if (AladdinWep != null)
                AladdinWep.Level = 0;
            if (TronWep != null)
                TronWep.Level = 0;
            if (MembershipCard != null)
                MembershipCard.Level = 0;
            if (IceCream != null)
                IceCream.Level = 0;
            if (RikuWep != null)
                RikuWep.Level = 0;
            if (KingsLetter != null)
                KingsLetter.Level = 0;

            if (highJump != null)
                highJump.Level = 0;
            if (quickRun != null)
                quickRun.Level = 0;
            if (dodgeRoll != null)
                dodgeRoll.Level = 0;
            if (aerialDodge != null)
                aerialDodge.Level = 0;
            if (glide != null)
                glide.Level = 0;

            //hide & reset seed hash
            if (data.ShouldResetHash)
            {
                HashGrid.Visibility = Visibility.Collapsed;
                data.SeedHashLoaded = false;
            }

            foreach (string value in data.PointsDatanew.Keys.ToList())
            {
                data.PointsDatanew[value] = 0;
            }

            foreach (string world in WorldPoints.Keys.ToList())
            {
                WorldPoints[world] = 0;
                WorldPoints_c[world] = 0;
            }

            WorldGrid.Real_Fire = 0;
            WorldGrid.Real_Blizzard = 0;
            WorldGrid.Real_Thunder = 0;
            WorldGrid.Real_Cure = 0;
            WorldGrid.Real_Reflect = 0;
            WorldGrid.Real_Magnet = 0;
            WorldGrid.Real_Pages = 0;
            WorldGrid.Real_Pouches = 0;
            WorldGrid.Proof_Count = 0;
            WorldGrid.Form_Count = 0;
            WorldGrid.Summon_Count = 0;
            WorldGrid.Ability_Count = 0;
            WorldGrid.Report_Count = 0;
            WorldGrid.Visit_Count = 0;

            WorldGrid.Real_AuronWep = 0;
            WorldGrid.Real_MulanWep = 0;
            WorldGrid.Real_BeastWep = 0;
            WorldGrid.Real_JackWep = 0;
            WorldGrid.Real_SimbaWep = 0;
            WorldGrid.Real_SparrowWep = 0;
            WorldGrid.Real_AladdinWep = 0;
            WorldGrid.Real_TronWep = 0;
            WorldGrid.Real_MembershipCard = 0;
            WorldGrid.Real_IceCream = 0;
            WorldGrid.Real_RikuWep = 0;
            WorldGrid.Real_KingsLetter = 0;

            FireCount.Text = "3";
            BlizzardCount.Text = "3";
            ThunderCount.Text = "3";
            CureCount.Text = "3";
            ReflectCount.Text = "3";
            MagnetCount.Text = "3";
            PageCount.Text = "5";
            MunnyCount.Text = "2";

            WorldGrid.Ghost_Fire = 0;
            WorldGrid.Ghost_Blizzard = 0;
            WorldGrid.Ghost_Thunder = 0;
            WorldGrid.Ghost_Cure = 0;
            WorldGrid.Ghost_Reflect = 0;
            WorldGrid.Ghost_Magnet = 0;
            WorldGrid.Ghost_Pages = 0;
            WorldGrid.Ghost_Pouches = 0;
            WorldGrid.Ghost_Fire_obtained = 0;
            WorldGrid.Ghost_Blizzard_obtained = 0;
            WorldGrid.Ghost_Thunder_obtained = 0;
            WorldGrid.Ghost_Cure_obtained = 0;
            WorldGrid.Ghost_Reflect_obtained = 0;
            WorldGrid.Ghost_Magnet_obtained = 0;
            WorldGrid.Ghost_Pages_obtained = 0;
            WorldGrid.Ghost_Pouches_obtained = 0;

            BCCount.Text = "2";
            HTCount.Text = "2";
            PLCount.Text = "2";
            OCCount.Text = "2";
            LoDCount.Text = "2";
            PRCount.Text = "2";
            AGCount.Text = "2";
            SPCount.Text = "2";
            TWTNWCount.Text = "2";
            HBCount.Text = "2";
            DCCount.Text = "2";
            TTCount.Text = "3";

            WorldGrid.Ghost_AuronWep = 0;
            WorldGrid.Ghost_MulanWep = 0;
            WorldGrid.Ghost_BeastWep = 0;
            WorldGrid.Ghost_JackWep = 0;
            WorldGrid.Ghost_SimbaWep = 0;
            WorldGrid.Ghost_SparrowWep = 0;
            WorldGrid.Ghost_AladdinWep = 0;
            WorldGrid.Ghost_TronWep = 0;
            WorldGrid.Ghost_MembershipCard = 0;
            WorldGrid.Ghost_IceCream = 0;
            WorldGrid.Ghost_RikuWep = 0;
            WorldGrid.Ghost_KingsLetter = 0;
            WorldGrid.Ghost_AuronWep_obtained = 0;
            WorldGrid.Ghost_MulanWep_obtained = 0;
            WorldGrid.Ghost_BeastWep_obtained = 0;
            WorldGrid.Ghost_JackWep_obtained = 0;
            WorldGrid.Ghost_SimbaWep_obtained = 0;
            WorldGrid.Ghost_SparrowWep_obtained = 0;
            WorldGrid.Ghost_AladdinWep_obtained = 0;
            WorldGrid.Ghost_TronWep_obtained = 0;
            WorldGrid.Ghost_MembershipCard_obtained = 0;
            WorldGrid.Ghost_IceCream_obtained = 0;
            WorldGrid.Ghost_RikuWep_obtained = 0;
            WorldGrid.Ghost_KingsLetter_obtained = 0;

            Ghost_FireCount.Visibility = Visibility.Hidden;
            Ghost_BlizzardCount.Visibility = Visibility.Hidden;
            Ghost_ThunderCount.Visibility = Visibility.Hidden;
            Ghost_CureCount.Visibility = Visibility.Hidden;
            Ghost_ReflectCount.Visibility = Visibility.Hidden;
            Ghost_MagnetCount.Visibility = Visibility.Hidden;
            Ghost_PageCount.Visibility = Visibility.Hidden;
            Ghost_MunnyCount.Visibility = Visibility.Hidden;

            Ghost_BCCount.Visibility = Visibility.Hidden;
            Ghost_HTCount.Visibility = Visibility.Hidden;
            Ghost_PLCount.Visibility = Visibility.Hidden;
            Ghost_OCCount.Visibility = Visibility.Hidden;
            Ghost_LoDCount.Visibility = Visibility.Hidden;
            Ghost_PRCount.Visibility = Visibility.Hidden;
            Ghost_AGCount.Visibility = Visibility.Hidden;
            Ghost_SPCount.Visibility = Visibility.Hidden;
            Ghost_TWTNWCount.Visibility = Visibility.Hidden;
            Ghost_HBCount.Visibility = Visibility.Hidden;
            Ghost_DCCount.Visibility = Visibility.Hidden;
            Ghost_TTCount.Visibility = Visibility.Hidden;

            FireCount.Fill = (SolidColorBrush)FindResource("Color_Black");
            FireCount.Stroke = (SolidColorBrush)FindResource("Color_Trans");
            FireCount.Fill = (LinearGradientBrush)FindResource("Color_Fire");
            FireCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
            BlizzardCount.Fill = (SolidColorBrush)FindResource("Color_Black");
            BlizzardCount.Stroke = (SolidColorBrush)FindResource("Color_Trans");
            BlizzardCount.Fill = (LinearGradientBrush)FindResource("Color_Blizzard");
            BlizzardCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
            ThunderCount.Fill = (SolidColorBrush)FindResource("Color_Black");
            ThunderCount.Stroke = (SolidColorBrush)FindResource("Color_Trans");
            ThunderCount.Fill = (LinearGradientBrush)FindResource("Color_Thunder");
            ThunderCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
            CureCount.Fill = (SolidColorBrush)FindResource("Color_Black");
            CureCount.Stroke = (SolidColorBrush)FindResource("Color_Trans");
            CureCount.Fill = (LinearGradientBrush)FindResource("Color_Cure");
            CureCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
            MagnetCount.Fill = (SolidColorBrush)FindResource("Color_Black");
            MagnetCount.Stroke = (SolidColorBrush)FindResource("Color_Trans");
            MagnetCount.Fill = (LinearGradientBrush)FindResource("Color_Magnet");
            MagnetCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
            ReflectCount.Fill = (SolidColorBrush)FindResource("Color_Black");
            ReflectCount.Stroke = (SolidColorBrush)FindResource("Color_Trans");
            ReflectCount.Fill = (LinearGradientBrush)FindResource("Color_Reflect");
            ReflectCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
            PageCount.Fill = (SolidColorBrush)FindResource("Color_Black");
            PageCount.Stroke = (SolidColorBrush)FindResource("Color_Trans");
            PageCount.Fill = (LinearGradientBrush)FindResource("Color_Page");
            PageCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
            MunnyCount.Fill = (SolidColorBrush)FindResource("Color_Black");
            MunnyCount.Stroke = (SolidColorBrush)FindResource("Color_Trans");
            MunnyCount.Fill = (LinearGradientBrush)FindResource("Color_Pouch");
            MunnyCount.Stroke = (SolidColorBrush)FindResource("Color_Black");

            BCCount.Fill = (LinearGradientBrush)FindResource("Color_BC");
            BCCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
            HTCount.Fill = (LinearGradientBrush)FindResource("Color_HT");
            HTCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
            PLCount.Fill = (LinearGradientBrush)FindResource("Color_PL");
            PLCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
            OCCount.Fill = (LinearGradientBrush)FindResource("Color_OC");
            OCCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
            LoDCount.Fill = (LinearGradientBrush)FindResource("Color_LoD");
            LoDCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
            PRCount.Fill = (LinearGradientBrush)FindResource("Color_PR");
            PRCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
            AGCount.Fill = (LinearGradientBrush)FindResource("Color_AG");
            AGCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
            SPCount.Fill = (LinearGradientBrush)FindResource("Color_SP");
            SPCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
            TWTNWCount.Fill = (LinearGradientBrush)FindResource("Color_TWTNW");
            TWTNWCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
            HBCount.Fill = (LinearGradientBrush)FindResource("Color_HB");
            HBCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
            DCCount.Fill = (LinearGradientBrush)FindResource("Color_DC");
            DCCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
            TTCount.Fill = (LinearGradientBrush)FindResource("Color_TT");
            TTCount.Stroke = (SolidColorBrush)FindResource("Color_Black");

            Data.WorldItems.Clear();
            data.TrackedReports.Clear();

            CollectionGrid.Visibility = Visibility.Visible;
            ScoreGrid.Visibility = Visibility.Hidden;
            ProgressionCollectionGrid.Visibility = Visibility.Hidden;

            //reset settings row
            SettingsText.Text = "";
            Setting_BetterSTT.Width = new GridLength(0, GridUnitType.Star);
            Setting_Level_01.Width = new GridLength(0, GridUnitType.Star);
            Setting_Level_50.Width = new GridLength(0, GridUnitType.Star);
            Setting_Level_99.Width = new GridLength(0, GridUnitType.Star);
            Setting_Absent.Width = new GridLength(0, GridUnitType.Star);
            Setting_Absent_Split.Width = new GridLength(0, GridUnitType.Star);
            Setting_Datas.Width = new GridLength(0, GridUnitType.Star);
            Setting_Sephiroth.Width = new GridLength(0, GridUnitType.Star);
            Setting_Terra.Width = new GridLength(0, GridUnitType.Star);
            Setting_Cups.Width = new GridLength(0, GridUnitType.Star);
            Setting_HadesCup.Width = new GridLength(0, GridUnitType.Star);
            Setting_Cavern.Width = new GridLength(0, GridUnitType.Star);
            Setting_Transport.Width = new GridLength(0, GridUnitType.Star);
            Setting_Spacer.Width = new GridLength(10, GridUnitType.Star);

            //reset pathhints edits
            foreach (string key in data.WorldsData.Keys.ToList())
            {
                data.WorldsData[key].top.ColumnDefinitions[2].Width = new GridLength(0, GridUnitType.Star);

                var pathgrid = (Grid)data.WorldsData[key].top.FindName(key + "Path");

                pathgrid.Visibility = Visibility.Collapsed;
                foreach (Image child in pathgrid.Children)
                {
                    if (child.Name.Contains(key + "Path_Non")) //reset non icon to default image
                        child.Source = new BitmapImage(new Uri("Images/System/proof_nonexistence.png", UriKind.Relative));

                    child.Visibility = Visibility.Collapsed;
                }
            }

            UpdatePointScore(0);
            ReportsToggle(ReportsOption.IsChecked);
            TornPagesToggle(TornPagesOption.IsChecked);
            VisitLockToggle(VisitLockOption.IsChecked);
            ChestLockToggle(ChestLockOption.IsChecked);

            DeathCounter = 0;
            DeathValue.Text = "0";
            DeathCol.Width = new GridLength(0, GridUnitType.Star);
            DeathCounterGrid.Visibility = Visibility.Collapsed;

            foreach (Grid itempool in ItemPool.Children)
            {
                foreach (var item in itempool.Children)
                {
                    ContentControl check = item as ContentControl;

                    if (check != null && !check.Name.Contains("Ghost"))
                        check.Opacity = 1.0;
                }
            }

            NextLevelDisplay();

            //reset progression visuals
            PPCount.Width = new GridLength(1.15, GridUnitType.Star);
            PPSep.Width = new GridLength(0.3, GridUnitType.Star);

            ResetHints();

            SetWorldImage();

            //reset display stuff
            displays.Clear();

            if (data.wasTracking && sender != null)
                InitTracker();
        }

        private bool InProgressCheck(string type)
        {
            string message = "";
            string caption = "";

            if (data.seedLoaded | data.saveFileLoaded)
            {
                if (type == "tsv")
                {
                    message = "Hints were already loaded into the tracker!" +
                        "\n Any progress made so far would be lost if you continue." +
                        "\n Proceed anyway?";
                    caption = "Progress Load Confirmation";
                }
                if (type == "seed")
                {
                    message = "A Randomizer Seed was already loaded into the tracker!" +
                        "\n Any progress made so far would be lost if you continue." +
                        "\n Proceed anyway?";
                    caption = "Seed Load Confirmation";
                }
                if (type == "hints")
                {
                    message = "Hints were already loaded into the tracker!" +
                        "\n Any progress made so far would be lost if you continue." +
                        "\n Proceed anyway?";
                    caption = "Hints Load Confirmation";

                }

                MessageForm.MessageBoxButtons buttons = MessageForm.MessageBoxButtons.OKCancel;
                MessageForm.DialogResult result;

                result = MessageForm.MessageBox.Show(message, caption, buttons);
                if (result == MessageForm.DialogResult.Cancel)
                {
                    return false;
                }
                else
                {
                    OnReset(null, null);
                    return true;
                }
            }
            else
            {
                OnReset(null, null);
                return true;
            }
        }

        private void ResetHints()
        {
            data.hintsLoaded = false;
            data.reportLocations.Clear();
            data.reportInformation.Clear();
            data.bossHomeHintInformation.Clear();
            data.bossHomeRevealsStored.Clear();
            data.reportAttempts = new List<int>() { 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3 };

            foreach (var key in data.WorldsData.Keys.ToList())
            {
                data.WorldsData[key].hinted = false;
                data.WorldsData[key].hintedHint = false;
                data.WorldsData[key].containsGhost = false;
                //progression hints per world
                data.WorldsData[key].hintedProgression = false;
            }
            data.WorldsData["GoA"].hinted = true;

            foreach (ContentControl report in data.ReportAttemptVisual)
            {
                report.SetResourceReference(ContentProperty, "Fail0");
            }

            foreach (WorldData worldData in data.WorldsData.Values.ToList())
            {
                if (worldData.value != null)
                    SetWorldValue(worldData.value, -999999);
            }

            for (int i = 0; i < data.Reports.Count; ++i)
            {
                data.Reports[i].HandleItemReturn();
            }
        }

        private void toggleState(bool clickable)
        {
            ReportsOption.IsEnabled = clickable;
            TornPagesOption.IsEnabled = clickable;
            PromiseCharmOption.IsEnabled = clickable;
            AbilitiesOption.IsEnabled = clickable;
            AntiFormOption.IsEnabled = clickable;
            VisitLockOption.IsEnabled = clickable;
            VisitLockOption2.IsEnabled = clickable;
            ChestLockOption.IsEnabled = clickable;
            ExtraChecksOption.IsEnabled = clickable;
            SoraLevel01Option.IsEnabled = clickable;
            SoraLevel50Option.IsEnabled = clickable;
            SoraLevel99Option.IsEnabled = clickable;

            WorldToggleMenuItem.IsEnabled = clickable;
        }

        private void VisitLockCheck(List<int> startingItems)
        {
            //Check all starting items to see if at least 1 of every visit lock item is in the inventory.
            //if so then assume first visit locks are off and diabe them from the itempool
            bool stt_Lock = false;
            bool lod_Lock = false;
            bool tt_Lock = false;
            bool hb_Lock = false;
            bool bc_Lock = false;
            bool oc_Lock = false;
            bool dc_Lock = false;
            bool pr_Lock = false;
            bool ag_Lock = false;
            bool ht_Lock = false;
            bool pl_Lock = false;
            bool sp_Lock = false;
            bool twtnw_Lock = false;

            int total = 0;

            foreach (int check in startingItems)
            {
                switch(check)
                {
                    case 54:
                        if(!oc_Lock)
                        {
                            oc_Lock = true;
                            total++;
                        }
                        break;
                    case 55:
                        if (!lod_Lock)
                        {
                            lod_Lock = true;
                            total++;
                        }
                        break;
                    case 59:
                        if (!bc_Lock)
                        {
                            bc_Lock = true;
                            total++;
                        }
                        break;
                    case 60:
                        if (!ht_Lock)
                        {
                            ht_Lock = true;
                            total++;
                        }
                        break;
                    case 61:
                        if (!pl_Lock)
                        {
                            pl_Lock = true;
                            total++;
                        }
                        break;
                    case 62:
                        if (!pr_Lock)
                        {
                            pr_Lock = true;
                            total++;
                        }
                        break;
                    case 72:
                        if (!ag_Lock)
                        {
                            ag_Lock = true;
                            total++;
                        }
                        break;
                    case 73:
                        if (!twtnw_Lock)
                        {
                            twtnw_Lock = true;
                            total++;
                        }
                        break;
                    case 74:
                        if (!sp_Lock)
                        {
                            sp_Lock = true;
                            total++;
                        }
                        break;
                    case 368:
                        if (!stt_Lock)
                        {
                            stt_Lock = true;
                            total++;
                        }
                        break;
                    case 369:
                        if (!hb_Lock)
                        {
                            hb_Lock = true;
                            total++;
                        }
                        break;
                    case 375:
                        if (!tt_Lock)
                        {
                            tt_Lock = true;
                            total++;
                        }
                        break;
                    case 460:
                        if (!dc_Lock)
                        {
                            dc_Lock = true;
                            total++;
                        }
                        break;
                }
            }

            if(total == 13)
            {
                VisitLockToggle2(true);
            }
        }

        /// 
        /// Hotkey logic
        ///

        private void LoadHotkeyBind()
        {
            if (!Directory.Exists("./KhTrackerSettings"))
            {
                Directory.CreateDirectory("./KhTrackerSettings");
            }

            if (!File.Exists("./KhTrackerSettings/AutoTrackerKeybinds.txt"))
            {
                //Console.WriteLine("File not found, making");
                using (FileStream fs = File.Create("./KhTrackerSettings/AutoTrackerKeybinds.txt"))
                {
                    // Add some text to file    
                    Byte[] title = new UTF8Encoding(true).GetBytes("Control\n");
                    fs.Write(title, 0, title.Length);
                    byte[] author = new UTF8Encoding(true).GetBytes("F12");
                    fs.Write(author, 0, author.Length);
                }
            }
            string[] lines = System.IO.File.ReadAllLines("./KhTrackerSettings/AutoTrackerKeybinds.txt");
            string mod1 = "";
            ModifierKeys _mod1 = ModifierKeys.None;
            string mod2 = "";
            ModifierKeys _mod2 = ModifierKeys.None;
            string mod3 = "";
            ModifierKeys _mod3 = ModifierKeys.None;
            string key = "";
            int modsUsed = 0;
            Key _key;

            try
            {
                string temp = lines[0];
            }
            catch
            {
                Console.WriteLine("No hotkeys detected, loading defaults");
                mod1 = "Control";
                key = "F12";

                Console.WriteLine("idk = " + mod1 + " " + key);
                if (key == "1" || key == "2" || key == "3" || key == "4" || key == "5"
                     || key == "6" || key == "7" || key == "8" || key == "9" || key == "0")
                {
                    Enum.TryParse(ConvertKeyNumber(key, true), out _key);
                    data.startAutoTracker1 = new GlobalHotkey(_mod1, _key, StartHotkey);
                    HotkeysManager.AddHotkey(data.startAutoTracker1);

                    Enum.TryParse(ConvertKeyNumber(key, false), out _key);
                    data.startAutoTracker2 = new GlobalHotkey(_mod1, _key, StartHotkey);
                    HotkeysManager.AddHotkey(data.startAutoTracker2);
                    return;
                }
                Enum.TryParse(ConvertKey(key), out _key);
                data.startAutoTracker1 = new GlobalHotkey(_mod1, _key, StartHotkey);
                HotkeysManager.AddHotkey(data.startAutoTracker1);
                return;
            }

            //break out early if empty file
            if (lines.Length == 0)
            {
                Console.WriteLine("No keybind set");
                data.startAutoTracker1 = null;
                return;
            }

            if (lines.Length > 1)
                key = lines[1];

            //get first line, split around +'s
            string modifiers = lines[0].ToLower();
            if (modifiers.IndexOf('+') > 0)
            {
                mod1 = modifiers.Substring(0, modifiers.IndexOf('+'));
                modifiers = modifiers.Substring(modifiers.IndexOf('+') + 1);
                modsUsed++;
            }
            else
            {
                mod1 = modifiers;
            }
            if (modifiers.IndexOf('+') > 0)
            {
                mod2 = modifiers.Substring(0, modifiers.IndexOf('+'));
                modifiers = modifiers.Substring(modifiers.IndexOf('+') + 1);
                modsUsed++;
            }
            else
            {
                mod2 = modifiers;
            }
            if (modifiers.Length > 0)
            {
                mod3 = modifiers;
                modsUsed++;
            }

            if (mod1.Contains("ctrl"))
                mod1 = "control";
            if (mod2.Contains("ctrl"))
                mod2 = "control";
            if (mod3.Contains("ctrl"))
                mod3 = "control";

            //capitalize all letters
            mod1 = UpperCaseFirst(mod1);
            mod2 = UpperCaseFirst(mod2);
            mod3 = UpperCaseFirst(mod3);
            key = UpperCaseFirst(key);

            //if no modifiers, only 1 key
            if (key == "")
            {
                Enum.TryParse(mod1, out _key);
                data.startAutoTracker1 = new GlobalHotkey(ModifierKeys.None, _key, StartHotkey);
                HotkeysManager.AddHotkey(data.startAutoTracker1);
                return;
            }

            //check for modifiers, however many
            if (mod1 != "")
                Enum.TryParse(mod1, out _mod1);
            if (mod2 != "")
                Enum.TryParse(mod2, out _mod2);
            if (mod3 != "")
                Enum.TryParse(mod3, out _mod3);

            //per used amount
            if (modsUsed == 3)
            {
                Console.WriteLine("idk = " + mod1 + " " + mod2 + " " + mod3 + " " + key);
                if (key == "1" || key == "2" || key == "3" || key == "4" || key == "5"
                     || key == "6" || key == "7" || key == "8" || key == "9" || key == "0")
                {
                    Enum.TryParse(ConvertKeyNumber(key, true), out _key);
                    data.startAutoTracker1 = new GlobalHotkey((_mod1 | _mod2 | _mod3), _key, StartHotkey);
                    HotkeysManager.AddHotkey(data.startAutoTracker1);

                    Enum.TryParse(ConvertKeyNumber(key, false), out _key);
                    data.startAutoTracker2 = new GlobalHotkey((_mod1 | _mod2 | _mod3), _key, StartHotkey);
                    HotkeysManager.AddHotkey(data.startAutoTracker2);
                    return;
                }
                Enum.TryParse(key, out _key);
                data.startAutoTracker1 = new GlobalHotkey((_mod1 | _mod2 | _mod3), _key, StartHotkey);
                HotkeysManager.AddHotkey(data.startAutoTracker1);
                return;
            }
            else if (modsUsed == 2)
            {
                Console.WriteLine("idk = " + mod1 + " " + mod2 + " " + key);
                if (key == "1" || key == "2" || key == "3" || key == "4" || key == "5"
                     || key == "6" || key == "7" || key == "8" || key == "9" || key == "0")
                {
                    Enum.TryParse(ConvertKeyNumber(key, true), out _key);
                    data.startAutoTracker1 = new GlobalHotkey((_mod1 | _mod2), _key, StartHotkey);
                    HotkeysManager.AddHotkey(data.startAutoTracker1);

                    Enum.TryParse(ConvertKeyNumber(key, false), out _key);
                    data.startAutoTracker2 = new GlobalHotkey((_mod1 | _mod2), _key, StartHotkey);
                    HotkeysManager.AddHotkey(data.startAutoTracker2);
                    return;
                }
                Enum.TryParse(key, out _key);
                data.startAutoTracker1 = new GlobalHotkey((_mod1 | _mod2), _key, StartHotkey);
                HotkeysManager.AddHotkey(data.startAutoTracker1);
                return;
            }
            else
            {
                Console.WriteLine("idk = " + mod1 + " " + key);
                if (key == "1" || key == "2" || key == "3" || key == "4" || key == "5"
                     || key == "6" || key == "7" || key == "8" || key == "9" || key == "0")
                {
                    Enum.TryParse(ConvertKeyNumber(key, true), out _key);
                    data.startAutoTracker1 = new GlobalHotkey(_mod1, _key, StartHotkey);
                    HotkeysManager.AddHotkey(data.startAutoTracker1);

                    Enum.TryParse(ConvertKeyNumber(key, false), out _key);
                    data.startAutoTracker2 = new GlobalHotkey(_mod1, _key, StartHotkey);
                    HotkeysManager.AddHotkey(data.startAutoTracker2);
                    return;
                }
                Enum.TryParse(ConvertKey(key), out _key);
                data.startAutoTracker1 = new GlobalHotkey(_mod1, _key, StartHotkey);
                HotkeysManager.AddHotkey(data.startAutoTracker1);
                return;
            }
        }

        private void ProgScrollHotkey()
        {
            data.scrollUp1 = new GlobalHotkey(ModifierKeys.Control, Key.Up, GoAScrollUp);
            HotkeysManager.AddHotkey(data.scrollUp1);
            data.scrollDown1 = new GlobalHotkey(ModifierKeys.Control, Key.Down, GoAScrollDown);
            HotkeysManager.AddHotkey(data.scrollDown1);
            data.scrollUp2 = new GlobalHotkey(ModifierKeys.None, Key.PageUp, GoAScrollUp);
            HotkeysManager.AddHotkey(data.scrollUp2);
            data.scrollDown2 = new GlobalHotkey(ModifierKeys.None, Key.PageDown, GoAScrollDown);
            HotkeysManager.AddHotkey(data.scrollDown2);
        }

        private string UpperCaseFirst(string word)
        {
            if (word.Length <= 0)
                return "";

            string firstLetter1 = word.Substring(0, 1);
            string firstLetter2 = firstLetter1.ToUpper();
            string rest = word.Substring(1);

            return firstLetter2 + rest;
        }

        private string ConvertKey(string key)
        {
            switch (key)
            {
                case ".":
                    return "OemPeriod";
                case ",":
                    return "OemComma";
                case "?":
                    return "OemPeriod";
                case "\"":
                    return "OemQuestion";
                case "'":
                    return "OemQuotes";
                case "[":
                    return "OemOpenBrackets";
                case "{":
                    return "OemOpenBrackets";
                case "]":
                    return "OemCloseBrackets";
                case "}":
                    return "OemCloseBrackets";
                case "\\":
                    return "OemBackslash";
                case ":":
                    return "OemSemicolon";
                case ";":
                    return "OemSemicolon";
                case "-":
                    return "OemMinus";
                case "_":
                    return "OemMinus";
                case "+":
                    return "OemPlus";
                case "=":
                    return "OemPlus";
                case "|":
                    return "OemPipe";

                default:
                    return key;
            }
        }

        public void bunterCheck(List<Dictionary<string, object>> bosses)
        {
            // update the bunterBoss dictionary for the grid tracker
            gridWindow.bunterBosses = bosses;
            gridWindow.bunterCheck(gridWindow.bunterBosses);
        }

        private string ConvertKeyNumber(string num, bool type)
        {
            switch (num)
            {
                case "1":
                    if (type)
                        return "D1";
                    else
                        return "NumPad1";
                case "2":
                    if (type)
                        return "D2";
                    else
                        return "NumPad2";
                case "3":
                    if (type)
                        return "D3";
                    else
                        return "NumPad3";
                case "4":
                    if (type)
                        return "D4";
                    else
                        return "NumPad4";
                case "5":
                    if (type)
                        return "D5";
                    else
                        return "NumPad5";
                case "6":
                    if (type)
                        return "D6";
                    else
                        return "NumPad6";
                case "7":
                    if (type)
                        return "D7";
                    else
                        return "NumPad7";
                case "8":
                    if (type)
                        return "D8";
                    else
                        return "NumPad8";
                case "9":
                    if (type)
                        return "D9";
                    else
                        return "NumPad9";
                default:
                    if (type)
                        return "D0";
                    else
                        return "NumPad0";
            }
        }

        //Auto Load Hints
        public void AutoLoadHints()
        {
            //if a seed was manually loaded, AutoLoad does not overwrite the seed
            if (data.hintsLoaded || data.seedLoaded)
            {
                Console.WriteLine("Hints/seed already loaded, not continuing with AutoLoadHints");
                return;
            }

            //if (AutoLoadHintsOption.IsChecked == true)
            //    Console.WriteLine("AutoLoadHints enabled");
            //else
            //{
            //    Console.WriteLine("AutoLoadHints disabled");
            //    return;
            //}

            //Similar code in Toggle.cs
            //If the user *somehow* deletes/edits the path to something incorrect after starting/selecting AutoLoad Hints
            //This code will still check the path to make sure the tracker won't crash
            string[] OpenKHPath = System.IO.File.ReadAllLines("./KhTrackerSettings/OpenKHPath.txt");
            try
            {
                string temp = OpenKHPath[0];
            }
            catch
            {
                MessageBox.Show("OpenKH path does not exist. Please add/edit your path in the\n\"KhTrackerSettings/OpenKHPath.txt\" file next to the tracker.\nIf this issue persists, please ask for help\nin the KH2 Rando Discord server!\n\nWill auto-track now with no seed loaded.");
                Console.WriteLine("No OpenKH path found, aborting");
                return;
            }

            if (!Directory.Exists(OpenKHPath[0]))
            {
                MessageBox.Show("OpenKH path does not exist. Please add/edit your path in the\n\"KhTrackerSettings/OpenKHPath.txt\" file next to the tracker.\nIf this issue persists, please ask for help\nin the KH2 Rando Discord server!\n\nWill auto-track now with no seed loaded.");
                return;
            }
            else
                Console.WriteLine("OpenKH path found");

            //get path for the mods folder where the seed is extracted to
            string modsPath = OpenKHPath[0] + "\\mods\\kh2";
            //get path(s) for the hint files
            string[] hintFiles = System.IO.Directory.GetFiles(modsPath, "*.Hints", SearchOption.AllDirectories);

            //foreach (string hintFile in hintFiles)
            //    Console.WriteLine(hintFile);

            //if exactly 1 hint file is found, we're good to load it
            if (hintFiles.Length == 1)
                OpenKHSeed(System.IO.Directory.GetParent(hintFiles[0]).ToString(), true);
            //if more than 1 hint file is found, don't load anything
            else if (hintFiles.Length > 1)
                MessageBox.Show("Multiple hint files detected. Aborting Auto-Loading Hints.\nManually load your seed and/or re-check your Mod Manager.");
        }

        //new window test
        private void GridWindow_Open(object sender, RoutedEventArgs e)
        {
            //ExtraItemToggleCheck();
            gridWindow.Show();
        }

        private void objWindow_Open(object sender, RoutedEventArgs e)
        {
            objWindow.Show();
        }

        //extra toggles
        private void GTStartupToggle(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.GridTrackerStartup = GTStartupOption.IsChecked;
        }

        private void OTStartupToggle(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ObjTrackerStartup = OTStartupOption.IsChecked;
        }

        ///
        /// Timed Prog/Chest/Emblems switcher
        ///

        private static DispatcherTimer dispTimer;
        private List<String> displays = new List<String>();
        private int currDisplay = 0;
        //testing stuff for an alternator for text
        private void SetTimerStuff()
        {
            dispTimer?.Stop();
            dispTimer = new DispatcherTimer();
            dispTimer.Tick += OnTimedEvent2;
            dispTimer.Interval = new TimeSpan(0, 0, 0, 5, 0);
            dispTimer.Start();
        }

        private void OnTimedEvent2(object sender, EventArgs e)
        {
            if (displays.Count == 0)
                return;

            Console.WriteLine(displays[currDisplay].ToString());
            if (displays[currDisplay] == "Score")
            {
                CollectionGrid.Visibility = Visibility.Collapsed;
                ScoreGrid.Visibility = Visibility.Visible;
                ProgressionCollectionGrid.Visibility = Visibility.Collapsed;
                EmblemGrid.Visibility = Visibility.Collapsed;
                ChestIcon.SetResourceReference(ContentProperty, "Score");
            }
            else if (displays[currDisplay] == "Progression")
            {
                CollectionGrid.Visibility = Visibility.Collapsed;
                ScoreGrid.Visibility = Visibility.Collapsed;
                ProgressionCollectionGrid.Visibility = Visibility.Visible;
                EmblemGrid.Visibility = Visibility.Collapsed;
                ChestIcon.SetResourceReference(ContentProperty, "ProgPoints");
            }
            else if (displays[currDisplay] == "Emblems")
            {
                CollectionGrid.Visibility = Visibility.Collapsed;
                ScoreGrid.Visibility = Visibility.Collapsed;
                ProgressionCollectionGrid.Visibility = Visibility.Collapsed;
                EmblemGrid.Visibility = Visibility.Visible;
                ChestIcon.SetResourceReference(ContentProperty, "Emblem");
            }
            else
            {
                CollectionGrid.Visibility = Visibility.Visible;
                ScoreGrid.Visibility = Visibility.Collapsed;
                ProgressionCollectionGrid.Visibility = Visibility.Collapsed;
                EmblemGrid.Visibility = Visibility.Collapsed;
                ChestIcon.SetResourceReference(ContentProperty, "Chest");
            }

            currDisplay++;
            if (currDisplay >= displays.Count)
                currDisplay = 0;
        }
    }
}
