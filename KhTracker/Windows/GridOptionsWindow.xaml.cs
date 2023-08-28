using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KhTracker
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    /// 
    public class Category
    {
        public string CategoryName { get; set; }
        public List<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
    }

    public class SubCategory
    {
        public string SubCategoryName { get; set; }
        public List<Option> Options { get; set; } = new List<Option>();
    }

    public enum OptionType { CheckBox, TextBox }

    public class Option
    {
        public OptionType Type { get; set; }
        public string Description { get; set; }
        public string DefaultValue { get; set; }

    }



    public class OptionTemplateSelector : DataTemplateSelector
    {
        public DataTemplate CheckBoxTemplate { get; set; }
        public DataTemplate TextBoxTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is Option option)
            {
                switch (option.Type)
                {
                    case OptionType.CheckBox:
                        return CheckBoxTemplate;
                    case OptionType.TextBox:
                        return TextBoxTemplate;
                    default:
                        throw new Exception("Unknown type");
                }
            }
            return null;
        }
    }


    public partial class GridOptionsWindow : Window
    {

        public GridWindow _gridWindow;
        public Data _data;
        int newNumRows;
        int newNumColumns;
        List<Category> categories;
        public GridOptionsWindow(GridWindow gridWindow, Data data)
        {
            InitializeComponent();
            _gridWindow = gridWindow;
            newNumRows = gridWindow.numRows;
            newNumColumns = gridWindow.numColumns;
            _data = data;

            categories = new List<Category>
            {
                new Category {
                    CategoryName = "Tracker Settings",
                    SubCategories = new List<SubCategory>
                    {
                        new SubCategory {
                            SubCategoryName = "Board Size",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.TextBox, Description = "Number of Rows", DefaultValue = $"{newNumRows}" },
                                new Option { Type = OptionType.TextBox, Description = "Number of Columns", DefaultValue = $"{newNumColumns}"  }
                            }
                        },
                        new SubCategory {
                            SubCategoryName = "Bingo Logic",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.CheckBox, Description = "Include Bingo Logic", DefaultValue = (_gridWindow.gridSettings.ContainsKey("GlobalBingoLogic") ? _gridWindow.gridSettings["GlobalBingoLogic"] : false).ToString() },
                            }
                        },
                        new SubCategory {
                            SubCategoryName = "Battleship Logic" ,
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.CheckBox, Description = "Include Battleship Logic", DefaultValue = (_gridWindow.gridSettings.ContainsKey("GlobalBattleshipLogic") ? _gridWindow.gridSettings["GlobalBattleshipLogic"] : false).ToString() },
                            }
                        }
                    }
                },
                new Category {
                    CategoryName = "Allowed Checks",
                    SubCategories = new List<SubCategory>
                    {
                        new SubCategory {
                            SubCategoryName = "Bosses",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.CheckBox, Description = "Axel I", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Axel1") ? _gridWindow.gridSettings["Axel1"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Axel II", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Axel") ? _gridWindow.gridSettings["Axel"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Barbossa", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Barbossa") ? _gridWindow.gridSettings["Barbossa"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Blizzard Lord (NS)", DefaultValue = "false" },
                                new Option { Type = OptionType.CheckBox, Description = "The Beast", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Beast") ? _gridWindow.gridSettings["Beast"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Cerberus", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Cerberus") ? _gridWindow.gridSettings["Cerberus"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Cloud (NS)", DefaultValue = "false" },
                                new Option { Type = OptionType.CheckBox, Description = "Dark Thorn", DefaultValue = (_gridWindow.gridSettings.ContainsKey("DarkThorn") ? _gridWindow.gridSettings["DarkThorn"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Demyx", DefaultValue = (_gridWindow.gridSettings.ContainsKey("HBDemyx") ? _gridWindow.gridSettings["HBDemyx"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "The Experiment", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Experiment") ? _gridWindow.gridSettings["Experiment"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Grim Reaper I", DefaultValue = (_gridWindow.gridSettings.ContainsKey("GrimReaper1") ? _gridWindow.gridSettings["GrimReaper1"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Grim Reaper II", DefaultValue = (_gridWindow.gridSettings.ContainsKey("GrimReaper") ? _gridWindow.gridSettings["GrimReaper"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Groundshaker", DefaultValue = _gridWindow.gridSettings["GroundShaker"].ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Hades", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Hades") ? _gridWindow.gridSettings["Hades"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Hayner (NS)", DefaultValue = "false" },
                                new Option { Type = OptionType.CheckBox, Description = "Hercules (NS)", DefaultValue = "false" },
                                new Option { Type = OptionType.CheckBox, Description = "Hostile Program", DefaultValue = (_gridWindow.gridSettings.ContainsKey("HostileProgram") ? _gridWindow.gridSettings["HostileProgram"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Hydra", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Hydra") ? _gridWindow.gridSettings["Hydra"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Jafar", DefaultValue = (_gridWindow.gridSettings.ContainsKey("GenieJafar") ? _gridWindow.gridSettings["GenieJafar"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Leon (NS)", DefaultValue = "false" },                                
                                new Option { Type = OptionType.CheckBox, Description = "Luxord", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Luxord") ? _gridWindow.gridSettings["Luxord"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "MCP", DefaultValue = (_gridWindow.gridSettings.ContainsKey("MCP") ? _gridWindow.gridSettings["MCP"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Past Pete", DefaultValue = (_gridWindow.gridSettings.ContainsKey("OldPete") ? _gridWindow.gridSettings["OldPete"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Oogie Boogie", DefaultValue = (_gridWindow.gridSettings.ContainsKey("OogieBoogie") ? _gridWindow.gridSettings["OogieBoogie"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Pete TR", DefaultValue = (_gridWindow.gridSettings.ContainsKey("DCPete") ? _gridWindow.gridSettings["DCPete"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Prison Keeper", DefaultValue = (_gridWindow.gridSettings.ContainsKey("PrisonKeeper") ? _gridWindow.gridSettings["PrisonKeeper"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Riku (NS)", DefaultValue = "false" },
                                new Option { Type = OptionType.CheckBox, Description = "Roxas", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Roxas") ? _gridWindow.gridSettings["Roxas"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Saix", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Saix") ? _gridWindow.gridSettings["Saix"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Sark", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Sark") ? _gridWindow.gridSettings["Sark"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Scar", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Scar") ? _gridWindow.gridSettings["Scar"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Seifer (NS)", DefaultValue = "false" },                                
                                new Option { Type = OptionType.CheckBox, Description = "Setzer (NS)", DefaultValue = "false" },
                                new Option { Type = OptionType.CheckBox, Description = "Shadow Stalker (NS)", DefaultValue = "false" },
                                new Option { Type = OptionType.CheckBox, Description = "Shan-Yu", DefaultValue = (_gridWindow.gridSettings.ContainsKey("ShanYu") ? _gridWindow.gridSettings["ShanYu"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Storm Rider", DefaultValue = (_gridWindow.gridSettings.ContainsKey("StormRider") ? _gridWindow.gridSettings["StormRider"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Thresholder", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Thresholder") ? _gridWindow.gridSettings["Thresholder"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Tifa (NS)", DefaultValue = "false" },
                                new Option { Type = OptionType.CheckBox, Description = "Twilight Thorn", DefaultValue = (_gridWindow.gridSettings.ContainsKey("TwilightThorn") ? _gridWindow.gridSettings["TwilightThorn"] : true).ToString() },                                
                                new Option { Type = OptionType.CheckBox, Description = "Vivi (NS)", DefaultValue = "false" },
                                new Option { Type = OptionType.CheckBox, Description = "Volcano Lord (NS)", DefaultValue = "false" },
                                new Option { Type = OptionType.CheckBox, Description = "Xaldin", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Xaldin") ? _gridWindow.gridSettings["Xaldin"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Xemnas", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Xemnas1") ? _gridWindow.gridSettings["Xemnas1"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Xigbar", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Xigbar") ? _gridWindow.gridSettings["Xigbar"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Yuffie (NS)", DefaultValue = "false" },                                
                            }
                        },
                        new SubCategory {
                            SubCategoryName = "Superbosses",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.CheckBox, Description = "Axel (Data)", DefaultValue = (_gridWindow.gridSettings.ContainsKey("GridDataAxel") ? _gridWindow.gridSettings["GridDataAxel"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Demyx (Data)", DefaultValue = (_gridWindow.gridSettings.ContainsKey("GridDataDemyx") ? _gridWindow.gridSettings["GridDataDemyx"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Larxene", DefaultValue = (_gridWindow.gridSettings.ContainsKey("GridLarxene") ? _gridWindow.gridSettings["GridLarxene"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Larxene (Data)", DefaultValue = (_gridWindow.gridSettings.ContainsKey("GridLarxeneData") ? _gridWindow.gridSettings["GridLarxeneData"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Lexaeus", DefaultValue = (_gridWindow.gridSettings.ContainsKey("GridLexaeus") ? _gridWindow.gridSettings["GridLexaeus"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Lexaeus (Data)", DefaultValue = (_gridWindow.gridSettings.ContainsKey("GridLexaeusData") ? _gridWindow.gridSettings["GridLexaeusData"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Lingering Will", DefaultValue = (_gridWindow.gridSettings.ContainsKey("GridLingeringWill") ? _gridWindow.gridSettings["GridLingeringWill"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Luxord (Data)", DefaultValue = (_gridWindow.gridSettings.ContainsKey("GridDataLuxord") ? _gridWindow.gridSettings["GridDataLuxord"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Marluxia", DefaultValue = (_gridWindow.gridSettings.ContainsKey("GridMarluxia") ? _gridWindow.gridSettings["GridMarluxia"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Marluxia (Data)", DefaultValue = (_gridWindow.gridSettings.ContainsKey("GridMarluxiaData") ? _gridWindow.gridSettings["GridMarluxiaData"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Roxas (Data)", DefaultValue = (_gridWindow.gridSettings.ContainsKey("GridDataRoxas") ? _gridWindow.gridSettings["GridDataRoxas"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Saix (Data)", DefaultValue = (_gridWindow.gridSettings.ContainsKey("GridDataSaix") ? _gridWindow.gridSettings["GridDataSaix"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Sephiroth", DefaultValue = (_gridWindow.gridSettings.ContainsKey("GridSephiroth") ? _gridWindow.gridSettings["GridSephiroth"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Vexen", DefaultValue = (_gridWindow.gridSettings.ContainsKey("GridVexen") ? _gridWindow.gridSettings["GridVexen"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Vexen (Data)", DefaultValue = (_gridWindow.gridSettings.ContainsKey("GridVexenData") ? _gridWindow.gridSettings["GridVexenData"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Xaldin (Data)", DefaultValue = (_gridWindow.gridSettings.ContainsKey("GridDataXaldin") ? _gridWindow.gridSettings["GridDataXaldin"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Xemnas (Data)", DefaultValue = (_gridWindow.gridSettings.ContainsKey("GridDataXemnas") ? _gridWindow.gridSettings["GridDataXemnas"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Xigbar (Data)", DefaultValue = (_gridWindow.gridSettings.ContainsKey("GridDataXigbar") ? _gridWindow.gridSettings["GridDataXigbar"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Zexion", DefaultValue = (_gridWindow.gridSettings.ContainsKey("GridZexion") ? _gridWindow.gridSettings["GridZexion"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Zexion (Data)", DefaultValue = (_gridWindow.gridSettings.ContainsKey("GridZexionData") ? _gridWindow.gridSettings["GridZexionData"] : true).ToString() },
                            }
                        },
                        new SubCategory { 
                            SubCategoryName = "Progression",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.CheckBox, Description = "Agrabah", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Abu") ? _gridWindow.gridSettings["Abu"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Atlantica", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Ursula") ? _gridWindow.gridSettings["Ursula"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Beast's Castle", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Dragoons") ? _gridWindow.gridSettings["Dragoons"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Cavern of Rememberance Fights", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Fight1") ? _gridWindow.gridSettings["Fight1"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Disney Castle", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Minnie") ? _gridWindow.gridSettings["Minnie"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Drive Levels", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Drive2") ? _gridWindow.gridSettings["Drive2"] : false).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Halloween Town", DefaultValue = (_gridWindow.gridSettings.ContainsKey("CandyCaneLane") ? _gridWindow.gridSettings["CandyCaneLane"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Hollow Bastion", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Bailey") ? _gridWindow.gridSettings["Bailey"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Hundred Acre Wood", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Piglet") ? _gridWindow.gridSettings["Piglet"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Land of Dragons", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Missions") ? _gridWindow.gridSettings["Missions"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Olympus Coliseum", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Urns") ? _gridWindow.gridSettings["Urns"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Port Royal", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Town") ? _gridWindow.gridSettings["Town"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Pride Lands", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Simba") ? _gridWindow.gridSettings["Simba"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Simulated Twilight Town", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Minigame") ? _gridWindow.gridSettings["Minigame"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Space Paranoids", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Screens") ? _gridWindow.gridSettings["Screens"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "The World That Never Was", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Roxas") ? _gridWindow.gridSettings["Roxas"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Twilight Town", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Station") ? _gridWindow.gridSettings["Station"] : true).ToString() },  
                            }
                        },
                        new SubCategory { 
                            SubCategoryName = "Magics",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.CheckBox, Description = "Level 1 Magics", DefaultValue = (_gridWindow.gridSettings.ContainsKey("GridFire1") ? _gridWindow.gridSettings["GridFire1"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Level 2 Magics", DefaultValue = (_gridWindow.gridSettings.ContainsKey("GridFire2") ? _gridWindow.gridSettings["GridFire2"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Level 3 Magics", DefaultValue = (_gridWindow.gridSettings.ContainsKey("GridFire3") ? _gridWindow.gridSettings["GridFire3"] : true).ToString() },
                            }
                        },
                        new SubCategory { 
                            SubCategoryName = "Summons",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.CheckBox, Description = "Summons", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Lamp") ? _gridWindow.gridSettings["Lamp"] : true).ToString() },
                            }
                        },
                        new SubCategory { 
                            SubCategoryName = "Drives",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.CheckBox, Description = "Drives", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Valor") ? _gridWindow.gridSettings["Valor"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Light & Darkness Counts as Final", DefaultValue = "true" },
                            }
                        },
                        new SubCategory { 
                            SubCategoryName = "Proofs",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.CheckBox, Description = "Proof of Connection", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Connection") ? _gridWindow.gridSettings["Connection"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Proof of Nonexistence", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Nonexistence") ? _gridWindow.gridSettings["Nonexistence"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Proof of Peace", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Peace") ? _gridWindow.gridSettings["Peace"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Promise Charm", DefaultValue = (_gridWindow.gridSettings.ContainsKey("PromiseCharm") ? _gridWindow.gridSettings["PromiseCharm"] : true).ToString() },
                            }
                        },
                        new SubCategory { 
                            SubCategoryName = "SC/OM",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.CheckBox, Description = "SC/OM", DefaultValue = (_gridWindow.gridSettings.ContainsKey("SecondChance") ? _gridWindow.gridSettings["SecondChance"] : true).ToString() },
                            }
                        },
                        new SubCategory { 
                            SubCategoryName = "Torn Pages",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.CheckBox, Description = "Torn Page 1", DefaultValue = (_gridWindow.gridSettings.ContainsKey("GridTornPage1") ? _gridWindow.gridSettings["GridTornPage1"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Torn Page 2", DefaultValue = (_gridWindow.gridSettings.ContainsKey("GridTornPage2") ? _gridWindow.gridSettings["GridTornPage2"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Torn Page 3", DefaultValue = (_gridWindow.gridSettings.ContainsKey("GridTornPage3") ? _gridWindow.gridSettings["GridTornPage3"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Torn Page 4", DefaultValue = (_gridWindow.gridSettings.ContainsKey("GridTornPage4") ? _gridWindow.gridSettings["GridTornPage4"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Torn Page 5", DefaultValue = (_gridWindow.gridSettings.ContainsKey("GridTornPage5") ? _gridWindow.gridSettings["GridTornPage5"] : true).ToString() },
                            }
                        },
                        new SubCategory { 
                            SubCategoryName = "Reports",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.TextBox, Description = "Max Reports", DefaultValue = (_gridWindow.gridNumericalSettings.ContainsKey("NumReports") ? _gridWindow.gridNumericalSettings["NumReports"] : 11).ToString() },
                            }
                        },
                        new SubCategory { 
                            SubCategoryName = "Visit Unlocks",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.TextBox, Description = "Max Visit Unlocks", DefaultValue = (_gridWindow.gridNumericalSettings.ContainsKey("NumUnlocks") ? _gridWindow.gridNumericalSettings["NumUnlocks"] : 13).ToString() },
                            }
                        },
                        new SubCategory { 
                            SubCategoryName = "Miscellaneous",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.CheckBox, Description = "Hades Cup Trophy", DefaultValue = (_gridWindow.gridSettings.ContainsKey("HadesCup") ? _gridWindow.gridSettings["HadesCup"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Olympus Stone", DefaultValue = (_gridWindow.gridSettings.ContainsKey("OlympusStone") ? _gridWindow.gridSettings["OlympusStone"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Unknown Disk", DefaultValue = (_gridWindow.gridSettings.ContainsKey("UnknownDisk") ? _gridWindow.gridSettings["UnknownDisk"] : true).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Munny Pouches", DefaultValue = (_gridWindow.gridSettings.ContainsKey("MunnyPouch1") ? _gridWindow.gridSettings["MunnyPouch1"] : true).ToString() },
                            }
                        },
                    }
                },
            };
            DataContext = categories;
        }

        private void UpdateGridSettings(Data data)
        {
            // update grid size
            newNumRows = int.Parse(categories.FirstOrDefault(c => c.CategoryName == "Tracker Settings")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Board Size")?.Options.FirstOrDefault(o => o.Description == "Number of Rows")?.DefaultValue);
            newNumColumns = int.Parse(categories.FirstOrDefault(c => c.CategoryName == "Tracker Settings")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Board Size")?.Options.FirstOrDefault(o => o.Description == "Number of Columns")?.DefaultValue);
            _gridWindow.gridNumericalSettings["NumRows"] = newNumRows;
            _gridWindow.gridNumericalSettings["NumColumns"] = newNumColumns;
            _gridWindow.numRows = newNumRows;
            _gridWindow.numColumns = newNumColumns;
            _gridWindow.grid.Children.Clear();

            // update bingo logic
            bool includeGlobalBingoLogic = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Tracker Settings")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Bingo Logic")?.Options.FirstOrDefault(o => o.Description == "Include Bingo Logic")?.DefaultValue);
            _gridWindow.gridSettings["GlobalBingoLogic"] = includeGlobalBingoLogic;
                

            // update progression
            bool AGProg = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Progression")?.Options.FirstOrDefault(o => o.Description == "Agrabah")?.DefaultValue);
            bool ATProg = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Progression")?.Options.FirstOrDefault(o => o.Description == "Atlantica")?.DefaultValue);
            bool BCProg = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Progression")?.Options.FirstOrDefault(o => o.Description == "Beast's Castle")?.DefaultValue);
            bool CoRProg = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Progression")?.Options.FirstOrDefault(o => o.Description == "Disney Castle")?.DefaultValue);
            bool DCProg = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Progression")?.Options.FirstOrDefault(o => o.Description == "Disney Castle")?.DefaultValue);
            bool DrivesProg = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Progression")?.Options.FirstOrDefault(o => o.Description == "Drive Levels")?.DefaultValue);
            bool HTProg = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Progression")?.Options.FirstOrDefault(o => o.Description == "Halloween Town")?.DefaultValue);
            bool HBProg = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Progression")?.Options.FirstOrDefault(o => o.Description == "Hollow Bastion")?.DefaultValue);
            bool HAWProg = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Progression")?.Options.FirstOrDefault(o => o.Description == "Hundred Acre Wood")?.DefaultValue);
            bool LODProg = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Progression")?.Options.FirstOrDefault(o => o.Description == "Land of Dragons")?.DefaultValue);
            bool OCProg = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Progression")?.Options.FirstOrDefault(o => o.Description == "Olympus Coliseum")?.DefaultValue);
            bool PRProg = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Progression")?.Options.FirstOrDefault(o => o.Description == "Port Royal")?.DefaultValue);
            bool PLProg = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Progression")?.Options.FirstOrDefault(o => o.Description == "Pride Lands")?.DefaultValue);
            bool STTProg = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Progression")?.Options.FirstOrDefault(o => o.Description == "Simulated Twilight Town")?.DefaultValue);
            bool SPProg = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Progression")?.Options.FirstOrDefault(o => o.Description == "Space Paranoids")?.DefaultValue);
            bool TWTNWProg = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Progression")?.Options.FirstOrDefault(o => o.Description == "The World That Never Was")?.DefaultValue);
            bool TTProg = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Progression")?.Options.FirstOrDefault(o => o.Description == "Twilight Town")?.DefaultValue);

            Dictionary<string, bool> progDict = new Dictionary<string, bool>()
            {
                { "Agrabah", AGProg },
                { "Atlantica", ATProg },
                { "BeastsCastle", BCProg },
                { "GoA", CoRProg },
                { "DisneyCastle", DCProg },
                { "DrivesLevels", DrivesProg },
                { "HalloweenTown", HTProg },
                { "HollowBastion", HBProg },
                { "HundredAcreWood", HAWProg },
                { "LandofDragons", LODProg },
                { "OlympusColiseum", OCProg },
                { "PortRoyal", PRProg },
                { "PrideLands", PLProg },
                { "SimulatedTwilightTown", STTProg },
                { "SpaceParanoids", SPProg },
                { "TWTNW", TWTNWProg },
                { "TwilightTown", TTProg }
            };

            foreach (var prog in progDict)
            {
                string world = prog.Key;
                bool include = prog.Value;
                
                if (world != "DrivesLevels")
                {
                    foreach (var progEvent in data.ProgressKeys[world])
                    {
                        if (_gridWindow.gridSettings.ContainsKey(progEvent))
                            _gridWindow.gridSettings[progEvent] = include;
                    }
                }
                else
                {
                    var driveLevels = new[] { "Drive2", "Drive3", "Drive4", "Drive5", "Drive6", "Drive7" };
                    foreach (var driveLevel in driveLevels)
                    {
                        if (_gridWindow.gridSettings.ContainsKey(driveLevel))
                            _gridWindow.gridSettings[driveLevel] = DrivesProg;
                    }
                }
            }

            // update bosses (Note: This will overwrite the boss flags set in the progression code above)
            var bosses = categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Bosses");
            foreach (var boss in bosses.Options)
            {
                bool includeBoss = bool.Parse(bosses?.Options.FirstOrDefault(o => o.Description == boss.Description)?.DefaultValue);
                if (data.codes.bossNameConversion.ContainsKey(boss.Description) && _gridWindow.gridSettings.ContainsKey(data.codes.bossNameConversion[boss.Description]))
                    _gridWindow.gridSettings[data.codes.bossNameConversion[boss.Description]] = includeBoss;
            }


            // update superbosses (Note: This will overwrite the boss flags set in the progression code above)
            var superbosses = categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Superbosses");
            foreach (var superboss in superbosses.Options)
            {
                bool includeBoss = bool.Parse(superbosses?.Options.FirstOrDefault(o => o.Description == superboss.Description)?.DefaultValue);
                if (data.codes.bossNameConversion.ContainsKey(superboss.Description) && _gridWindow.gridSettings.ContainsKey(data.codes.bossNameConversion[superboss.Description]))
                    _gridWindow.gridSettings["Grid" + data.codes.bossNameConversion[superboss.Description]] = includeBoss;
            }

            // update magics
            var spellNames = new[] { "Fire", "Blizzard", "Thunder", "Cure", "Magnet", "Reflect" };
            bool includeLevel1Magics = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Magics")?.Options.FirstOrDefault(o => o.Description == "Level 1 Magics")?.DefaultValue);
            bool includeLevel2Magics = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Magics")?.Options.FirstOrDefault(o => o.Description == "Level 2 Magics")?.DefaultValue);
            bool includeLevel3Magics = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Magics")?.Options.FirstOrDefault(o => o.Description == "Level 3 Magics")?.DefaultValue);
            foreach (var spell in spellNames)
            {
                _gridWindow.gridSettings[$"Grid{spell}1"] = includeLevel1Magics;
                _gridWindow.gridSettings[$"Grid{spell}2"] = includeLevel2Magics;
                _gridWindow.gridSettings[$"Grid{spell}3"] = includeLevel3Magics;
            }

            // update summons
            var summonNames = new[] { "Baseball", "Feather", "Lamp", "Ukulele" };
            bool includeSummons = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Summons")?.Options.FirstOrDefault(o => o.Description == "Summons")?.DefaultValue);
            foreach (var summon in summonNames)
                _gridWindow.gridSettings[$"{summon}"] = includeSummons;

            // update drives
            var driveNames = new[] { "Valor", "Wisdom", "Limit", "Master", "Final" };
            bool includeDrives = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Drives")?.Options.FirstOrDefault(o => o.Description == "Drives")?.DefaultValue);
            foreach (var drive in driveNames)
                _gridWindow.gridSettings[$"{drive}"] = includeDrives;

            // update proofs
            var proofNames = new[] { "Peace", "Connection", "Nonexistence", "PromiseCharm" };
            bool includeConnection = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Proofs")?.Options.FirstOrDefault(o => o.Description == "Proof of Connection")?.DefaultValue);
            bool includeNonexistence = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Proofs")?.Options.FirstOrDefault(o => o.Description == "Proof of Nonexistence")?.DefaultValue);
            bool includePeace = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Proofs")?.Options.FirstOrDefault(o => o.Description == "Proof of Peace")?.DefaultValue);
            bool includePromiseCharm = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Proofs")?.Options.FirstOrDefault(o => o.Description == "Promise Charm")?.DefaultValue);
            _gridWindow.gridSettings["Connection"] = includeConnection;
            _gridWindow.gridSettings["Nonexistence"] = includeNonexistence;
            _gridWindow.gridSettings["Peace"] = includePeace;
            _gridWindow.gridSettings["PromiseCharm"] = includePromiseCharm;

            // update SCOM
            var scomNames = new[] { "SecondChance", "OnceMore" };
            bool includeSCOM = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "SC/OM")?.Options.FirstOrDefault(o => o.Description == "SC/OM")?.DefaultValue);
            foreach (var scom in scomNames)
                _gridWindow.gridSettings[$"{scom}"] = includeSCOM;

            // update torn pages
            for (int i = 1; i <= 5; i++)
            {
                bool includeTP = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Torn Pages")?.Options.FirstOrDefault(o => o.Description == $"Torn Page {i}")?.DefaultValue);
                _gridWindow.gridSettings[$"GridTornPage{i}"] = includeTP;
            }

            // update reports
            // randomize which reports get included
            int numReports = int.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Reports")?.Options.FirstOrDefault(o => o.Description == "Max Reports")?.DefaultValue);
            var randomReports = Enumerable.Range(1, 13).OrderBy(g => Guid.NewGuid()).Take(numReports).ToList();
            foreach (int reportNum in Enumerable.Range(1, 13).ToList())
                _gridWindow.gridSettings[$"Report{reportNum}"] = randomReports.Contains(reportNum) ? true : false;
            _gridWindow.gridNumericalSettings["NumReports"] = numReports;

            // update visit unlocks
            // randomize which visit unlocks get included
            var unlockNames = new[] { "AladdinWep", "AuronWep", "BeastWep", "IceCream", "JackWep", "MembershipCard", "MulanWep", "Picture", "SimbaWep", "SparrowWep", "TronWep" };
            int numUnlocks = int.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Visit Unlocks")?.Options.FirstOrDefault(o => o.Description == "Max Visit Unlocks")?.DefaultValue);
            var randomUnlocks = Enumerable.Range(1, unlockNames.Length).OrderBy(g => Guid.NewGuid()).Take(numUnlocks).ToList();
            foreach (int i in Enumerable.Range(1, unlockNames.Length).ToList())
                _gridWindow.gridSettings[unlockNames[i - 1]] = randomUnlocks.Contains(i) ? true : false;
            _gridWindow.gridNumericalSettings["NumUnlocks"] = numUnlocks;

            // update miscellaneous
            bool includeHadesCup = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Miscellaneous")?.Options.FirstOrDefault(o => o.Description == "Hades Cup Trophy")?.DefaultValue);
            bool includeOlympusStone = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Miscellaneous")?.Options.FirstOrDefault(o => o.Description == "Olympus Stone")?.DefaultValue);
            bool includeUnknownDisk = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Miscellaneous")?.Options.FirstOrDefault(o => o.Description == "Unknown Disk")?.DefaultValue);
            bool includeMunnyPouch1 = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Miscellaneous")?.Options.FirstOrDefault(o => o.Description == "Munny Pouches")?.DefaultValue);
            bool includeMunnyPouch2 = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Miscellaneous")?.Options.FirstOrDefault(o => o.Description == "Munny Pouches")?.DefaultValue);
            _gridWindow.gridSettings["HadesCup"] = includeHadesCup;
            _gridWindow.gridSettings["OlympusStone"] = includeOlympusStone;
            _gridWindow.gridSettings["UnknownDisk"] = includeUnknownDisk;
            _gridWindow.gridSettings["MunnyPouch1"] = includeMunnyPouch1;
            _gridWindow.gridSettings["MunnyPouch2"] = includeMunnyPouch2;

            SaveSettings(_gridWindow.gridSettings);
            SaveNumericalSettings(_gridWindow.gridNumericalSettings);

            // generate new grid
            _gridWindow.GenerateGrid(newNumRows, newNumColumns);
        }

        private void SaveSettings(Dictionary<string, bool> settings)
        {
            string serializedSettings = JsonSerializer.Serialize(settings);

            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\GridTracker");
            key.SetValue("Settings", serializedSettings);
            key.Close();
        }

        private void SaveNumericalSettings(Dictionary<string, int> numericalSettings)
        {
            string serializedSettings = JsonSerializer.Serialize(numericalSettings);

            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\GridTrackerNumbers");
            key.SetValue("Settings", serializedSettings);
            key.Close();
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            UpdateGridSettings(_data);
        }
    }

    //private void SelectAll_Checked(object sender, RoutedEventArgs e)
    //{
    //    var checkbox = sender as CheckBox;
    //    var expander = checkbox.Parent as Expander;
    //    var itemsControl = FindChild<ItemsControl>(expander, "YourItemsControlName"); // name your ItemsControl if you haven't

    //    foreach (var option in itemsControl.Items)
    //    {
    //        var item = (Option)option;
    //        if (item.Description != "Select All")
    //        {
    //            item.DefaultValue = "True";
    //        }
    //    }
    //}

    //private void SelectAll_Unchecked(object sender, RoutedEventArgs e)
    //{
    //    var checkbox = sender as CheckBox;
    //    var expander = checkbox.Parent as Expander;
    //    var itemsControl = FindChild<ItemsControl>(expander, "YourItemsControlName");

    //    foreach (var option in itemsControl.Items)
    //    {
    //        var item = (Option)option;
    //        if (item.Description != "Select All")
    //        {
    //            item.DefaultValue = "False";
    //        }
    //    }
    //}

    //// Utility method to find a child of a control by type
    //public static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
    //{
    //    // Confirm parent and childName are valid. 
    //    if (parent == null) return null;

    //    T foundChild = null;

    //    int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
    //    for (int i = 0; i < childrenCount; i++)
    //    {
    //        var child = VisualTreeHelper.GetChild(parent, i);
    //        T childType = child as T;
    //        if (childType == null)
    //        {
    //            foundChild = FindChild<T>(child, childName);
    //            if (foundChild != null) break;
    //        }
    //        else if (!string.IsNullOrEmpty(childName))
    //        {
    //            FrameworkElement frameworkElement = child as FrameworkElement;
    //            if (frameworkElement != null && frameworkElement.Name == childName)
    //            {
    //                foundChild = (T)child;
    //                break;
    //            }
    //        }
    //        else
    //        {
    //            foundChild = (T)child;
    //            break;
    //        }
    //    }

    //    return foundChild;
    //}

}
