using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
//using System.Net.Sockets;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;


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
    public class Option : INotifyPropertyChanged
    {
        public OptionType Type { get; set; }
        private string description;
        private string defaultValue;
        private double textBoxWidth;
        public bool IsSelectAllOption { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public double TextBoxWidth
        {
            get { return textBoxWidth; }
            set
            {
                if (textBoxWidth != value)
                {
                    textBoxWidth = value;
                    OnPropertyChanged(nameof(TextBoxWidth));
                }
            }
        }

        public string Description
        {
            get { return description; }
            set
            {
                if (description != value)
                {
                    description = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

        public string DefaultValue
        {
            get { return defaultValue; }
            set
            {
                if (defaultValue != value)
                {
                    defaultValue = value;
                    OnPropertyChanged(nameof(DefaultValue));
                }
            }
        }

        private Visibility _visibility = Visibility.Visible;
        public Visibility Visibility
        {
            get => _visibility;
            set
            {
                if (_visibility != value)
                {
                    _visibility = value;
                    OnPropertyChanged(nameof(Visibility));
                }
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
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

    public class OptionVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Assuming `value` is the `Option` object
            if (value is Option option && string.IsNullOrEmpty(option.Description) && !option.IsSelectAllOption)
                return Visibility.Collapsed; // Hide spacer options
            return Visibility.Visible; // Show all other options
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsWarningNeededConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2 || !(values[0] is int) || !(values[1] is int))
                return false;

            int numSquares = (int)values[0];
            int trueChecksCount = (int)values[1];

            return trueChecksCount < numSquares;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BindingProxy : Freezable
    {
        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }

        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Data. This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new PropertyMetadata(null));
    }

    public partial class GridOptionsWindow : Window, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int TrueChecksCount
        {
            get { return _gridWindow.gridSettings.Count(kvp => kvp.Value == true && !new[] { "ForcingFinalCounts" }.Contains(kvp.Key)); }
        }

        private int _columnsInGrid = 4;
        public int ColumnsInGrid
        {
            get { return _columnsInGrid; }
            set
            {
                if (value != _columnsInGrid)
                {
                    _columnsInGrid = value;
                    OnPropertyChanged(nameof(ColumnsInGrid)); ;
                }
            }
        }

        public int NumSquares
        {
            get { return _gridWindow.numRows * _gridWindow.numColumns; }
        }

        private dynamic originalSettings;
        public bool canClose = false;
        public GridWindow _gridWindow;
        public Dictionary<string, bool> newGridSettings;
        public Data _data;
        int newNumRows;
        int newNumColumns;
        bool newBingoLogic;
        bool newBattleshipLogic;
        bool newBattleshipRandomCount;
        int newMaxShipCount;
        int newMinShipCount;
        List<int> newShipSizes;
        bool newFogOfWar;
        Dictionary<string, int> newFogOfWarSpan;
        List<Category> categories;
        readonly string[] nonChecks = { "Select All", "" };
        public GridOptionsWindow(GridWindow gridWindow, Data data)
        {
            InitializeComponent();
            InitializeData(gridWindow, data);
            UpdateGridOptionsUI();
        }

        public void InitializeData(GridWindow gridWindow, Data data)
        {
            _gridWindow = gridWindow;
            newBattleshipLogic = gridWindow.battleshipLogic;
            newBattleshipRandomCount = gridWindow.battleshipRandomCount;
            newBingoLogic = gridWindow.bingoLogic;
            newFogOfWar = gridWindow.fogOfWar;
            newFogOfWarSpan = gridWindow.fogOfWarSpan;
            newGridSettings = gridWindow.gridSettings;
            newMaxShipCount = gridWindow.maxShipCount;
            newMinShipCount = gridWindow.minShipCount;
            newNumColumns = gridWindow.numColumns;
            newNumRows = gridWindow.numRows;
            newShipSizes = gridWindow.shipSizes;
            _data = data;

            if (_gridWindow.SavePreviousGridSettingsOption.IsChecked)
            {
                Properties.Settings.Default.GridWindowRows = newNumRows;
                Properties.Settings.Default.GridWindowColumns = newNumColumns;
                Properties.Settings.Default.GridSettings = JsonSerializer.Serialize(newGridSettings);
                Properties.Settings.Default.GridWindowBingoLogic = newBingoLogic;
                Properties.Settings.Default.GridWindowBattleshipLogic = newBattleshipLogic;
                Properties.Settings.Default.ShipSizes = JsonSerializer.Serialize(newShipSizes);
                Properties.Settings.Default.BattleshipRandomCount = newBattleshipRandomCount;
                Properties.Settings.Default.FogOfWar = newFogOfWar;
                Properties.Settings.Default.FogOfWarSpan = JsonSerializer.Serialize(newFogOfWarSpan);
                Properties.Settings.Default.MaxShipCount = newMaxShipCount;
                Properties.Settings.Default.MinShipCount = newMinShipCount;
            }

            originalSettings = new
            {
                _gridWindow.numRows,
                _gridWindow.numColumns,
                _gridWindow.bingoLogic,
                _gridWindow.battleshipLogic,
                _gridWindow.seedName,
                _gridWindow.shipSizes,
                _gridWindow.fogOfWar,
                _gridWindow.fogOfWarSpan,
                _gridWindow.gridSettings,
                _gridWindow.minShipCount,
                _gridWindow.maxShipCount,
                _gridWindow.battleshipRandomCount
            };

            OnPropertyChanged(nameof(TrueChecksCount));
            OnPropertyChanged(nameof(NumSquares));
        }

        public void UpdateGridOptionsUI()
        {
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
                                new Option { Type = OptionType.TextBox, Description = "Number of Rows", DefaultValue = $"{newNumRows}", TextBoxWidth = 5},
                                new Option { Type = OptionType.TextBox, Description = "Number of Columns", DefaultValue = $"{newNumColumns}", TextBoxWidth = 5  }
                            }
                        },
                        new SubCategory {
                            SubCategoryName = "Bingo Logic",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.CheckBox, Description = "Include Bingo Logic", DefaultValue = $"{newBingoLogic}"  },
                            }
                        },
                        new SubCategory {
                            SubCategoryName = "Battleship Logic" ,
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.CheckBox, Description = "Include Battleship Logic", DefaultValue = $"{newBattleshipLogic}" },
                                new Option { Type = OptionType.CheckBox, Description = "Random Ship Count", DefaultValue = $"{newBattleshipRandomCount}" },
                                new Option { Type = OptionType.CheckBox, Description = "", DefaultValue = $"", Visibility = Visibility.Collapsed},
                                new Option { Type = OptionType.CheckBox, Description = "", DefaultValue = $"", Visibility = Visibility.Collapsed},
                                new Option { Type = OptionType.TextBox, Description = "Ship Sizes", DefaultValue = $"{string.Join(", ", newShipSizes)}", Visibility = newBattleshipLogic ? Visibility.Visible : Visibility.Collapsed },
                                new Option { Type = OptionType.TextBox, Description = "Min Ship Count", DefaultValue = $"{newMinShipCount}", Visibility = newBattleshipRandomCount ? Visibility.Visible : Visibility.Collapsed},
                                new Option { Type = OptionType.TextBox, Description = "Max Ship Count", DefaultValue = $"{newMaxShipCount}", Visibility = newBattleshipRandomCount ? Visibility.Visible : Visibility.Collapsed},
                                new Option { Type = OptionType.TextBox, Description = "", DefaultValue = $"", Visibility = Visibility.Collapsed},
                            }
                        },
                        new SubCategory {
                            SubCategoryName = "Fog of War Logic" ,
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.CheckBox, Description = "Include Fog of War Logic", DefaultValue = $"{newFogOfWar}" },
                                new Option { Type = OptionType.CheckBox, Description = "", DefaultValue = $"", Visibility = Visibility.Collapsed},
                                new Option { Type = OptionType.CheckBox, Description = "", DefaultValue = $"", Visibility = Visibility.Collapsed},
                                new Option { Type = OptionType.CheckBox, Description = "", DefaultValue = $"", Visibility = Visibility.Collapsed},
                                new Option { Type = OptionType.TextBox, Description = "West Hint Span", DefaultValue = $"{(newFogOfWarSpan.ContainsKey("W") ? newFogOfWarSpan["N"] : 1)}", Visibility = (newFogOfWar ? Visibility.Visible : Visibility.Collapsed) },
                                new Option { Type = OptionType.TextBox, Description = "East Hint Span", DefaultValue = $"{(newFogOfWarSpan.ContainsKey("E") ? newFogOfWarSpan["N"] : 1)}", Visibility = (newFogOfWar ? Visibility.Visible : Visibility.Collapsed) },
                                new Option { Type = OptionType.TextBox, Description = "North Hint Span", DefaultValue = $"{(newFogOfWarSpan.ContainsKey("N") ? newFogOfWarSpan["N"] : 1)}", Visibility = (newFogOfWar ? Visibility.Visible : Visibility.Collapsed) },
                                new Option { Type = OptionType.TextBox, Description = "South Hint Span", DefaultValue = $"{(newFogOfWarSpan.ContainsKey("S") ? newFogOfWarSpan["N"] : 1)}", Visibility = (newFogOfWar ? Visibility.Visible : Visibility.Collapsed) },
                                new Option { Type = OptionType.TextBox, Description = "NorthWest Hint Span", DefaultValue = $"{(newFogOfWarSpan.ContainsKey("NW") ? newFogOfWarSpan["NW"] : 1)}", Visibility = (newFogOfWar ? Visibility.Visible : Visibility.Collapsed) },
                                new Option { Type = OptionType.TextBox, Description = "NorthEast Hint Span", DefaultValue = $"{(newFogOfWarSpan.ContainsKey("NE") ? newFogOfWarSpan["NE"] : 1)}", Visibility = (newFogOfWar ? Visibility.Visible : Visibility.Collapsed) },
                                new Option { Type = OptionType.TextBox, Description = "SouthWest Hint Span", DefaultValue = $"{(newFogOfWarSpan.ContainsKey("SW") ? newFogOfWarSpan["SW"] : 1)}", Visibility = (newFogOfWar ? Visibility.Visible : Visibility.Collapsed) },
                                new Option { Type = OptionType.TextBox, Description = "SouthEast Hint Span", DefaultValue = $"{(newFogOfWarSpan.ContainsKey("SE") ? newFogOfWarSpan["SE"] : 1)}", Visibility = (newFogOfWar ? Visibility.Visible : Visibility.Collapsed) },
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
                                new Option { Type = OptionType.CheckBox, Description = "Armor Xemnas I", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridArmoredXemnas1") || _gridWindow.gridSettings["GridArmoredXemnas1"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Armor Xemnas II", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridArmoredXemnas2") || _gridWindow.gridSettings["GridArmoredXemnas2"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Axel I", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Axel1") || _gridWindow.gridSettings["Axel1"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Axel II", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Axel") || _gridWindow.gridSettings["Axel"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Barbossa", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Barbossa") || _gridWindow.gridSettings["Barbossa"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Blizzard Lord", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridBlizzardLord") || _gridWindow.gridSettings["GridBlizzardLord"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "The Beast", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Beast") || _gridWindow.gridSettings["Beast"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Cerberus", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Cerberus") || _gridWindow.gridSettings["Cerberus"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Cloud", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridCloud") || _gridWindow.gridSettings["GridCloud"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Dark Thorn", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("DarkThorn") || _gridWindow.gridSettings["DarkThorn"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Demyx", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("HBDemyx") || _gridWindow.gridSettings["HBDemyx"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "The Experiment", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Experiment") || _gridWindow.gridSettings["Experiment"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Final Xemnas", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridFinalXemnas") || _gridWindow.gridSettings["GridFinalXemnas"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Grim Reaper I", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GrimReaper1") || _gridWindow.gridSettings["GrimReaper1"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Grim Reaper II", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GrimReaper") || _gridWindow.gridSettings["GrimReaper"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Groundshaker", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GroundShaker") || _gridWindow.gridSettings["GroundShaker"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Hades", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Hades") || _gridWindow.gridSettings["Hades"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Hayner", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridHayner") || _gridWindow.gridSettings["GridHayner"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Hercules", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridHercules") || _gridWindow.gridSettings["GridHercules"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Hostile Program", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("HostileProgram") || _gridWindow.gridSettings["HostileProgram"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Hydra", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Hydra") || _gridWindow.gridSettings["Hydra"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Jafar", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GenieJafar") || _gridWindow.gridSettings["GenieJafar"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Leon", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridLeon") || _gridWindow.gridSettings["GridLeon"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Luxord", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Luxord") || _gridWindow.gridSettings["Luxord"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "MCP", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("MCP") || _gridWindow.gridSettings["MCP"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Past Pete", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("OldPete") || _gridWindow.gridSettings["OldPete"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Oogie Boogie", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("OogieBoogie") || _gridWindow.gridSettings["OogieBoogie"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Pete TR", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("DCPete") || _gridWindow.gridSettings["DCPete"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Prison Keeper", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("PrisonKeeper") || _gridWindow.gridSettings["PrisonKeeper"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Riku", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridRiku") || _gridWindow.gridSettings["GridRiku"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Roxas", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Roxas") || _gridWindow.gridSettings["Roxas"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Saix", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Saix") || _gridWindow.gridSettings["Saix"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Sark", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Sark") || _gridWindow.gridSettings["Sark"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Scar", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Scar") || _gridWindow.gridSettings["Scar"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Seifer", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridSeifer") || _gridWindow.gridSettings["GridSeifer"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Setzer", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridSetzer") || _gridWindow.gridSettings["GridSetzer"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Shadow Stalker", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridShadowStalker") || _gridWindow.gridSettings["GridShadowStalker"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Shan-Yu", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("ShanYu") || _gridWindow.gridSettings["ShanYu"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Storm Rider", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("StormRider") || _gridWindow.gridSettings["StormRider"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Thresholder", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Thresholder") || _gridWindow.gridSettings["Thresholder"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Tifa", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridTifa") || _gridWindow.gridSettings["GridTifa"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Twilight Thorn", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("TwilightThorn") || _gridWindow.gridSettings["TwilightThorn"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Vivi", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridVivi") || _gridWindow.gridSettings["GridVivi"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Volcano Lord", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridVolcanoLord") || _gridWindow.gridSettings["GridVolcanoLord"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Xaldin", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Xaldin") || _gridWindow.gridSettings["Xaldin"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Xemnas", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Xemnas1") || _gridWindow.gridSettings["Xemnas1"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Xigbar", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Xigbar") || _gridWindow.gridSettings["Xigbar"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Yuffie", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridYuffie") || _gridWindow.gridSettings["GridYuffie"]).ToString() },
                            }
                        },
                        new SubCategory {
                            SubCategoryName = "Superbosses",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.CheckBox, Description = "Axel (Data)", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridDataAxel") || _gridWindow.gridSettings["GridDataAxel"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Demyx (Data)", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridDataDemyx") || _gridWindow.gridSettings["GridDataDemyx"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Final Xemnas (Data)", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridDataFinalXemnas") || _gridWindow.gridSettings["GridDataFinalXemnas"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Larxene", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridLarxene") || _gridWindow.gridSettings["GridLarxene"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Larxene (Data)", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridLarxeneData") || _gridWindow.gridSettings["GridLarxeneData"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Lexaeus", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridLexaeus") || _gridWindow.gridSettings["GridLexaeus"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Lexaeus (Data)", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridLexaeusData") || _gridWindow.gridSettings["GridLexaeusData"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Luxord (Data)", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridDataLuxord") || _gridWindow.gridSettings["GridDataLuxord"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Marluxia", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridMarluxia") || _gridWindow.gridSettings["GridMarluxia"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Marluxia (Data)", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridMarluxiaData") || _gridWindow.gridSettings["GridMarluxiaData"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Roxas (Data)", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridDataRoxas") || _gridWindow.gridSettings["GridDataRoxas"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Saix (Data)", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridDataSaix") || _gridWindow.gridSettings["GridDataSaix"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Sephiroth", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridSephiroth") || _gridWindow.gridSettings["GridSephiroth"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Terra", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridLingeringWill") || _gridWindow.gridSettings["GridLingeringWill"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Vexen", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridVexen") || _gridWindow.gridSettings["GridVexen"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Vexen (Data)", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridVexenData") || _gridWindow.gridSettings["GridVexenData"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Xaldin (Data)", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridDataXaldin") || _gridWindow.gridSettings["GridDataXaldin"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Xemnas (Data)", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridDataXemnas") || _gridWindow.gridSettings["GridDataXemnas"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Xigbar (Data)", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridDataXigbar") || _gridWindow.gridSettings["GridDataXigbar"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Zexion", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridZexion") || _gridWindow.gridSettings["GridZexion"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Zexion (Data)", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridZexionData") || _gridWindow.gridSettings["GridZexionData"]).ToString() },
                            }
                        },
                        new SubCategory {
                            SubCategoryName = "Progression",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.CheckBox, Description = "Agrabah", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Abu") || _gridWindow.gridSettings["Abu"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Atlantica", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Ursula") || _gridWindow.gridSettings["Ursula"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Beast's Castle", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Dragoons") || _gridWindow.gridSettings["Dragoons"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Cavern of Rememberance Fights", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Fight1") || _gridWindow.gridSettings["Fight1"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Disney Castle", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Minnie") || _gridWindow.gridSettings["Minnie"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Drive Levels", DefaultValue = (_gridWindow.gridSettings.ContainsKey("Drive2") && _gridWindow.gridSettings["Drive2"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Halloween Town", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("CandyCaneLane") || _gridWindow.gridSettings["CandyCaneLane"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Hollow Bastion", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Bailey") || _gridWindow.gridSettings["Bailey"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Hundred Acre Wood", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Piglet") || _gridWindow.gridSettings["Piglet"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Land of Dragons", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Missions") || _gridWindow.gridSettings["Missions"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Olympus Coliseum", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Urns") || _gridWindow.gridSettings["Urns"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Port Royal", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Town") || _gridWindow.gridSettings["Town"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Pride Lands", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Simba") || _gridWindow.gridSettings["Simba"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Simulated Twilight Town", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Minigame") || _gridWindow.gridSettings["Minigame"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Space Paranoids", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Screens") || _gridWindow.gridSettings["Screens"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "The World That Never Was", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Roxas") || _gridWindow.gridSettings["Roxas"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Twilight Town", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Station") || _gridWindow.gridSettings["Station"]).ToString() },
                            }
                        },
                        new SubCategory {
                            SubCategoryName = "Magics",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.CheckBox, Description = "Level 1 Magics", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridFire1") || _gridWindow.gridSettings["GridFire1"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Level 2 Magics", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridFire2") || _gridWindow.gridSettings["GridFire2"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Level 3 Magics", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridFire3") || _gridWindow.gridSettings["GridFire3"]).ToString() },
                            }
                        },
                        new SubCategory {
                            SubCategoryName = "Summons",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.CheckBox, Description = "Summons", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Lamp") || _gridWindow.gridSettings["Lamp"]).ToString() },
                            }
                        },
                        new SubCategory {
                            SubCategoryName = "Drives",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.CheckBox, Description = "Drives", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Valor") || _gridWindow.gridSettings["Valor"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Light & Darkness Counts as Final", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("ForcingFinalCounts") || _gridWindow.gridSettings["ForcingFinalCounts"]).ToString() },
                            }
                        },
                        new SubCategory {
                            SubCategoryName = "Proofs",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.CheckBox, Description = "Proof of Connection", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Connection") || _gridWindow.gridSettings["Connection"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Proof of Nonexistence", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Nonexistence") || _gridWindow.gridSettings["Nonexistence"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Proof of Peace", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Peace") || _gridWindow.gridSettings["Peace"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Promise Charm", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("PromiseCharm") || _gridWindow.gridSettings["PromiseCharm"]).ToString() },
                            }
                        },
                        new SubCategory {
                            SubCategoryName = "SC/OM",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.CheckBox, Description = "SC/OM", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("SecondChance") || _gridWindow.gridSettings["SecondChance"]).ToString() },
                            }
                        },
                        new SubCategory {
                            SubCategoryName = "Torn Pages",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.CheckBox, Description = "Torn Page 1", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridTornPage1") || _gridWindow.gridSettings["GridTornPage1"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Torn Page 2", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridTornPage2") || _gridWindow.gridSettings["GridTornPage2"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Torn Page 3", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridTornPage3") || _gridWindow.gridSettings["GridTornPage3"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Torn Page 4", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridTornPage4") || _gridWindow.gridSettings["GridTornPage4"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Torn Page 5", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridTornPage5") || _gridWindow.gridSettings["GridTornPage5"]).ToString() },
                            }
                        },
                        new SubCategory {
                            SubCategoryName = "Reports",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.TextBox, Description = "Max Reports", DefaultValue = (Properties.Settings.Default.GridWindowNumReports).ToString() },
                            }
                        },
                        new SubCategory {
                            SubCategoryName = "World Chest Locks",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.TextBox, Description = "Max World Chest Locks", DefaultValue = (Properties.Settings.Default.GridWindowNumChestLocks).ToString() },
                            }
                        },
                        new SubCategory {
                            SubCategoryName = "Visit Unlocks",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.TextBox, Description = "Max Visit Unlocks", DefaultValue = (Properties.Settings.Default.GridWindowNumUnlocks).ToString() },
                            }
                        },
                        new SubCategory {
                            SubCategoryName = "Miscellaneous",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.CheckBox, Description = "Hades Cup Trophy", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("HadesCup") || _gridWindow.gridSettings["HadesCup"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Olympus Stone", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("OlympusStone") || _gridWindow.gridSettings["OlympusStone"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Unknown Disk", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("UnknownDisk") || _gridWindow.gridSettings["UnknownDisk"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Munny Pouches", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("GridMunnyPouch1") || _gridWindow.gridSettings["GridMunnyPouch1"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "Yeet the Bear", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("StarryHill") || _gridWindow.gridSettings["StarryHill"]).ToString() },
                                new Option { Type = OptionType.CheckBox, Description = "All 7 Drives", DefaultValue = (!_gridWindow.gridSettings.ContainsKey("Grid7Drives") || _gridWindow.gridSettings["Grid7Drives"]).ToString() },
                            }
                        },
                    }
                },
            };
            DataContext = categories;
            string[] selectAllCategories = { "Bosses", "Superbosses", "Progression", "Magics", "Proofs", "Torn Pages", "Miscellaneous" };
            foreach (Category category in categories)
            {
                List<SubCategory> subcategories = category.SubCategories;
                foreach (SubCategory subcategory in subcategories)
                {
                    if (selectAllCategories.Contains(subcategory.SubCategoryName))
                    {
                        List<Option> options = subcategory.Options;
                        int numberOfOptions = options.Count();
                        int spacersNeeded = ((ColumnsInGrid - (numberOfOptions % ColumnsInGrid)) % ColumnsInGrid) + ColumnsInGrid;

                        for (int i = 0; i < spacersNeeded; i++)
                        {
                            options.Add(new Option { Description = "" }); // Add spacers
                        }

                        options.Add(new Option { Type = OptionType.CheckBox, Description = "Select All", DefaultValue = "false", IsSelectAllOption = true });
                    }
                }
            }
        }

        private void UpdateTextBoxes(object sender, RoutedEventArgs e)
        {
            if (!(sender is TextBox textBox)) return;

            var option = textBox.DataContext as Option;
            textBox.Text = textBox.Text == "" ? option.DefaultValue : textBox.Text;
            
            if (option.Description == "Max Visit Unlocks")
            {
                int maxUnlocks = MainWindow.data.VisitLocks.Count;
                if (int.Parse(textBox.Text) > maxUnlocks)
                {
                    textBox.Text = maxUnlocks.ToString();
                }
            }

            if (option.Description == "Max World Chest Locks")
            {
                int maxChestLocks = MainWindow.data.VisitLocks.ConvertAll<string>(x => x.ToString()).Count;
                if (int.Parse(textBox.Text) > maxChestLocks)
                {
                    textBox.Text = maxChestLocks.ToString();
                }
            }

            if (option.Description == "Max Reports")
            {
                int maxReports = (MainWindow.data.VisitLocks.Select(item => item.Name)).ToList().Count;
                if (int.Parse(textBox.Text) > maxReports)
                {
                    textBox.Text = maxReports.ToString();
                }
            }

            if (option != null && textBox.Text != "")
            {
                option.DefaultValue = textBox.Text;
                UpdateGridSettings(_data, _gridWindow.SavePreviousGridSettingsOption.IsChecked);
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Use regular expression to allow only numeric input
            TextBox shipSizesTextBox = sender as TextBox;
            var currentOption = shipSizesTextBox.DataContext as Option;
            if (currentOption.Description == "Ship Sizes")
                e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, "^[0-9,]+$");
            else
            {
                e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, "^[0-9]+$");
            }
        }

        private void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                string text = (String)e.DataObject.GetData(typeof(String));
                // Use regular expression to check if text is numeric
                if (!System.Text.RegularExpressions.Regex.IsMatch(text, "^[0-9]+$"))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void SelectAllChecks(object sender, RoutedEventArgs e)
        {
            if (!(sender is CheckBox selectAllCheckbox)) return;

            bool isChecked = selectAllCheckbox.IsChecked ?? false;

            // Assuming the sender's DataContext is an Option and you can get the SubCategory from there
            if (!(selectAllCheckbox.DataContext is Option currentOption))
                return;
            if (!currentOption.IsSelectAllOption)
            {
                if (currentOption.Description == "Include Battleship Logic")
                {
                    if (selectAllCheckbox.IsChecked ?? false)
                    {
                        // un-toggle bingo logic
                        _gridWindow.bingoLogic = false;
                        var includeBingoOption = categories.FirstOrDefault(c => c.CategoryName == "Tracker Settings")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Bingo Logic")?.Options.FirstOrDefault(o => o.Description == "Include Bingo Logic");
                        includeBingoOption.DefaultValue = false.ToString();

                        // show ship sizes
                        var shipSizesOption = categories.SelectMany(c => c.SubCategories).SelectMany(sc => sc.Options).FirstOrDefault(o => o.Description == "Ship Sizes");
                        shipSizesOption.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        // hide ship sizes
                        var shipSizesOption = categories.SelectMany(c => c.SubCategories).SelectMany(sc => sc.Options).FirstOrDefault(o => o.Description == "Ship Sizes");
                        shipSizesOption.Visibility = Visibility.Collapsed;

                        // hide random ship counts
                        var minNumShips = categories.SelectMany(c => c.SubCategories).SelectMany(sc => sc.Options).FirstOrDefault(o => o.Description == "Min Ship Count");
                        minNumShips.Visibility = Visibility.Collapsed;

                        var maxNumShips = categories.SelectMany(c => c.SubCategories).SelectMany(sc => sc.Options).FirstOrDefault(o => o.Description == "Max Ship Count");
                        maxNumShips.Visibility = Visibility.Collapsed;
                    }

                }
                else if (currentOption.Description == "Include Bingo Logic")
                {
                    if (selectAllCheckbox.IsChecked ?? false)
                    {
                        // un-toggle battleship logic
                        _gridWindow.battleshipLogic = false;
                        var includeBattleshipOption = categories.FirstOrDefault(c => c.CategoryName == "Tracker Settings")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Battleship Logic")?.Options.FirstOrDefault(o => o.Description == "Include Battleship Logic");
                        includeBattleshipOption.DefaultValue = false.ToString();

                        // hide ship sizes
                        var shipSizesOption = categories.SelectMany(c => c.SubCategories).SelectMany(sc => sc.Options).FirstOrDefault(o => o.Description == "Ship Sizes");
                        shipSizesOption.Visibility = Visibility.Collapsed;

                        // hide random ship counts
                        var minNumShips = categories.SelectMany(c => c.SubCategories).SelectMany(sc => sc.Options).FirstOrDefault(o => o.Description == "Min Ship Count");
                        minNumShips.Visibility = Visibility.Collapsed;

                        var maxNumShips = categories.SelectMany(c => c.SubCategories).SelectMany(sc => sc.Options).FirstOrDefault(o => o.Description == "Max Ship Count");
                        maxNumShips.Visibility = Visibility.Collapsed;
                    }
                }
                else if (currentOption.Description == "Random Ship Count")
                {
                    if (selectAllCheckbox.IsChecked ?? false)
                    {
                        // show random ship counts
                        var minNumShips = categories.SelectMany(c => c.SubCategories).SelectMany(sc => sc.Options).FirstOrDefault(o => o.Description == "Min Ship Count");
                        minNumShips.Visibility = Visibility.Visible;

                        var maxNumShips = categories.SelectMany(c => c.SubCategories).SelectMany(sc => sc.Options).FirstOrDefault(o => o.Description == "Max Ship Count");
                        maxNumShips.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        // hide random ship counts
                        var minNumShips = categories.SelectMany(c => c.SubCategories).SelectMany(sc => sc.Options).FirstOrDefault(o => o.Description == "Min Ship Count");
                        minNumShips.Visibility = Visibility.Collapsed;

                        var maxNumShips = categories.SelectMany(c => c.SubCategories).SelectMany(sc => sc.Options).FirstOrDefault(o => o.Description == "Max Ship Count");
                        maxNumShips.Visibility = Visibility.Collapsed;
                    }
                }
                else if (currentOption.Description == "Include Fog of War Logic")
                {
                    foreach (string spanDescription in new[] { "West Hint Span", "East Hint Span", "North Hint Span", "South Hint Span", "NorthWest Hint Span", "NorthEast Hint Span", "SouthWest Hint Span", "SouthEast Hint Span"})
                    {
                        var currentSpanOption = categories.FirstOrDefault(c => c.CategoryName == "Tracker Settings")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Fog of War Logic")?.Options.FirstOrDefault(o => o.Description == $"{spanDescription}");
                        currentSpanOption.Visibility = selectAllCheckbox.IsChecked ?? false ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
                UpdateGridSettings(_data, _gridWindow.SavePreviousGridSettingsOption.IsChecked);
            }
            else
            {

                // Find the parent SubCategory
                var subCategory = categories.SelectMany(c => c.SubCategories).FirstOrDefault(sc => sc.Options.Contains(currentOption));
                if (subCategory == null) return;

                // Toggle all checkboxes based on the state of the "Select All" checkbox
                foreach (var option in subCategory.Options)
                {
                    if (!option.IsSelectAllOption)
                    {
                        option.DefaultValue = isChecked.ToString();
                    }
                }
            }
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.GridOptionsWindowY = RestoreBounds.Top;
            Properties.Settings.Default.GridOptionsWindowX = RestoreBounds.Left;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Properties.Settings.Default.GridOptionsWindowWidth = RestoreBounds.Width;
            //Properties.Settings.Default.GridOptionsWindowHeight = RestoreBounds.Height;
        }

        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            if (!canClose)
            {
                e.Cancel = true;
            }
        }

        private void UpdateGridSize(bool overwrite)
        {
            // update grid size
            newNumRows = int.Parse(categories.FirstOrDefault(c => c.CategoryName == "Tracker Settings")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Board Size")?.Options.FirstOrDefault(o => o.Description == "Number of Rows")?.DefaultValue);
            newNumColumns = int.Parse(categories.FirstOrDefault(c => c.CategoryName == "Tracker Settings")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Board Size")?.Options.FirstOrDefault(o => o.Description == "Number of Columns")?.DefaultValue);
            if (overwrite)
            {
                Properties.Settings.Default.GridWindowRows = newNumRows;
                Properties.Settings.Default.GridWindowColumns = newNumColumns;
            }
            _gridWindow.numRows = newNumRows;
            _gridWindow.numColumns = newNumColumns;
        }

        private void UpdateGlobalSettings(bool overwrite)
        {
            // update bingo logic
            bool includeGlobalBingoLogic = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Tracker Settings")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Bingo Logic")?.Options.FirstOrDefault(o => o.Description == "Include Bingo Logic")?.DefaultValue);
            _gridWindow.bingoLogic = includeGlobalBingoLogic;

            // update battleship logic
            bool includeGlobalBattleshipLogic = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Tracker Settings")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Battleship Logic")?.Options.FirstOrDefault(o => o.Description == "Include Battleship Logic")?.DefaultValue);
            _gridWindow.battleshipLogic = includeGlobalBattleshipLogic;

            var shipSizesOptionList = (categories.FirstOrDefault(c => c.CategoryName == "Tracker Settings")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Battleship Logic")?.Options.FirstOrDefault(o => o.Description == "Ship Sizes")?.DefaultValue);
            // text boxes are strings so we need to convert string to list if we are updating from the options window instead of uploading a card
            if (shipSizesOptionList.GetType() == typeof(string))
                _gridWindow.shipSizes = shipSizesOptionList
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries) // Split by comma
                    .SelectMany(chunk => chunk.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)) // Split by space after trimming
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(int.Parse)
                    .ToList();

            bool randomShipCount = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Tracker Settings")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Battleship Logic")?.Options.FirstOrDefault(o => o.Description == "Random Ship Count")?.DefaultValue);

            _gridWindow.battleshipRandomCount = randomShipCount;

            int minNumShips = int.Parse(categories.FirstOrDefault(c => c.CategoryName == "Tracker Settings")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Battleship Logic")?.Options.FirstOrDefault(o => o.Description == "Min Ship Count")?.DefaultValue);
            int maxNumShips = int.Parse(categories.FirstOrDefault(c => c.CategoryName == "Tracker Settings")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Battleship Logic")?.Options.FirstOrDefault(o => o.Description == "Max Ship Count")?.DefaultValue);

            _gridWindow.minShipCount = minNumShips;

            _gridWindow.maxShipCount = maxNumShips;

            bool includeFogOfWar = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Tracker Settings")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Fog of War Logic")?.Options.FirstOrDefault(o => o.Description == "Include Fog of War Logic")?.DefaultValue);
            _gridWindow.fogOfWar = includeFogOfWar;

            int westSpan = int.Parse(categories.FirstOrDefault(c => c.CategoryName == "Tracker Settings")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Fog of War Logic")?.Options.FirstOrDefault(o => o.Description == "West Hint Span")?.DefaultValue);
            int eastSpan = int.Parse(categories.FirstOrDefault(c => c.CategoryName == "Tracker Settings")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Fog of War Logic")?.Options.FirstOrDefault(o => o.Description == "East Hint Span")?.DefaultValue);
            int northSpan = int.Parse(categories.FirstOrDefault(c => c.CategoryName == "Tracker Settings")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Fog of War Logic")?.Options.FirstOrDefault(o => o.Description == "North Hint Span")?.DefaultValue);
            int southSpan = int.Parse(categories.FirstOrDefault(c => c.CategoryName == "Tracker Settings")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Fog of War Logic")?.Options.FirstOrDefault(o => o.Description == "South Hint Span")?.DefaultValue);
            int northWestSpan = int.Parse(categories.FirstOrDefault(c => c.CategoryName == "Tracker Settings")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Fog of War Logic")?.Options.FirstOrDefault(o => o.Description == "NorthWest Hint Span")?.DefaultValue);
            int northEastSpan = int.Parse(categories.FirstOrDefault(c => c.CategoryName == "Tracker Settings")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Fog of War Logic")?.Options.FirstOrDefault(o => o.Description == "NorthEast Hint Span")?.DefaultValue);
            int southWestSpan = int.Parse(categories.FirstOrDefault(c => c.CategoryName == "Tracker Settings")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Fog of War Logic")?.Options.FirstOrDefault(o => o.Description == "SouthWest Hint Span")?.DefaultValue);
            int southEastSpan = int.Parse(categories.FirstOrDefault(c => c.CategoryName == "Tracker Settings")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Fog of War Logic")?.Options.FirstOrDefault(o => o.Description == "SouthEast Hint Span")?.DefaultValue);

            _gridWindow.fogOfWarSpan["W"] = westSpan;
            _gridWindow.fogOfWarSpan["E"] = eastSpan;
            _gridWindow.fogOfWarSpan["N"] = northSpan;
            _gridWindow.fogOfWarSpan["S"] = southSpan;
            _gridWindow.fogOfWarSpan["NW"] = northWestSpan;
            _gridWindow.fogOfWarSpan["NE"] = northEastSpan;
            _gridWindow.fogOfWarSpan["SW"] = southWestSpan;
            _gridWindow.fogOfWarSpan["SE"] = southEastSpan;

            if (overwrite)
            {
                Properties.Settings.Default.GridWindowBingoLogic = includeGlobalBingoLogic;
                Properties.Settings.Default.GridWindowBattleshipLogic = includeGlobalBattleshipLogic;
                Properties.Settings.Default.BattleshipRandomCount = randomShipCount;
                Properties.Settings.Default.ShipSizes = JsonSerializer.Serialize(_gridWindow.shipSizes);
                Properties.Settings.Default.MinShipCount = minNumShips;
                Properties.Settings.Default.MaxShipCount = maxNumShips;
                Properties.Settings.Default.FogOfWar = includeFogOfWar;
                Properties.Settings.Default.FogOfWarSpan = JsonSerializer.Serialize(_gridWindow.fogOfWarSpan);
            }
        }

        private void UpdateProgression(Data data)
        {
            // update progression
            bool AGProg = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Progression")?.Options.FirstOrDefault(o => o.Description == "Agrabah")?.DefaultValue);
            bool ATProg = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Progression")?.Options.FirstOrDefault(o => o.Description == "Atlantica")?.DefaultValue);
            bool BCProg = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Progression")?.Options.FirstOrDefault(o => o.Description == "Beast's Castle")?.DefaultValue);
            bool CoRProg = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Progression")?.Options.FirstOrDefault(o => o.Description == "Cavern of Rememberance Fights")?.DefaultValue);
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
                    //removed "Drive2". it was the equavalant to a free space or "no drives collected"
                    var driveLevels = new[] { "Drive3", "Drive4", "Drive5", "Drive6", "Drive7" };
                    foreach (var driveLevel in driveLevels)
                    {
                        if (_gridWindow.gridSettings.ContainsKey(driveLevel))
                            _gridWindow.gridSettings[driveLevel] = DrivesProg;
                    }
                }
            }
        }

        private void UpdateBosses(Data data)
        {
            // update bosses (Note: This will overwrite the boss flags set in the progression code above)
            var bosses = categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Bosses");
            foreach (var boss in bosses.Options)
            {
                if (!nonChecks.Contains(boss.Description))
                {
                    bool includeBoss = bool.Parse(bosses?.Options.FirstOrDefault(o => o.Description == boss.Description)?.DefaultValue);
                    if (data.codes.bossNameConversion.ContainsKey(boss.Description) && _gridWindow.gridSettings.ContainsKey(data.codes.bossNameConversion[boss.Description]))
                        _gridWindow.gridSettings[data.codes.bossNameConversion[boss.Description]] = includeBoss;
                    else if (data.codes.bossNameConversion.ContainsKey(boss.Description) && _gridWindow.gridSettings.ContainsKey("Grid" + data.codes.bossNameConversion[boss.Description]))
                        _gridWindow.gridSettings["Grid" + data.codes.bossNameConversion[boss.Description]] = includeBoss;
                    else if (_gridWindow.gridSettings.ContainsKey(boss.Description))
                        _gridWindow.gridSettings[boss.Description] = includeBoss;
                }
            }
        }

        private void UpdateSuperbosses(Data data)
        {
            // update superbosses (Note: This will overwrite the boss flags set in the progression code above)
            var superbosses = categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Superbosses");
            foreach (var superboss in superbosses.Options)
            {
                if (!nonChecks.Contains(superboss.Description))
                    {
                    bool includeBoss = bool.Parse(superbosses?.Options.FirstOrDefault(o => o.Description == superboss.Description)?.DefaultValue);
                    if (data.codes.bossNameConversion.ContainsKey(superboss.Description) && _gridWindow.gridSettings.ContainsKey("Grid" + data.codes.bossNameConversion[superboss.Description]))
                        _gridWindow.gridSettings["Grid" + data.codes.bossNameConversion[superboss.Description]] = includeBoss;
                }
            }
        }

        private void UpdateMagics()
        {
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
        }

        private void UpdateSummons()
        {
            // update summons
            var summonNames = new[] { "Baseball", "Feather", "Lamp", "Ukulele" };
            bool includeSummons = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Summons")?.Options.FirstOrDefault(o => o.Description == "Summons")?.DefaultValue);
            foreach (var summon in summonNames)
                _gridWindow.gridSettings[$"{summon}"] = includeSummons;
        }

        private void UpdateDrives()
        {
            // update drives
            var driveNames = new[] { "Valor", "Wisdom", "Limit", "Master", "Final" };
            bool includeDrives = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Drives")?.Options.FirstOrDefault(o => o.Description == "Drives")?.DefaultValue);
            foreach (var drive in driveNames)
                _gridWindow.gridSettings[$"{drive}"] = includeDrives;
            bool ForcingFinalCounts = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Drives")?.Options.FirstOrDefault(o => o.Description == "Light & Darkness Counts as Final")?.DefaultValue);
            _gridWindow.gridSettings["ForcingFinalCounts"] = ForcingFinalCounts;
        }

        private void UpdateProofs()
        {
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
        }

        private void UpdateSCOM()
        {
            // update SCOM
            var scomNames = new[] { "SecondChance", "OnceMore" };
            bool includeSCOM = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "SC/OM")?.Options.FirstOrDefault(o => o.Description == "SC/OM")?.DefaultValue);
            foreach (var scom in scomNames)
                _gridWindow.gridSettings[$"{scom}"] = includeSCOM;
        }

        private void UpdateTornPages()
        {
            // update torn pages
            for (int i = 1; i <= 5; i++)
            {
                bool includeTP = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Torn Pages")?.Options.FirstOrDefault(o => o.Description == $"Torn Page {i}")?.DefaultValue);
                _gridWindow.gridSettings[$"GridTornPage{i}"] = includeTP;
            }
        }

        private void UpdateReports(bool overwrite)
        {
            // update reports
            // randomize which reports get included
            int numReports = int.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Reports")?.Options.FirstOrDefault(o => o.Description == "Max Reports")?.DefaultValue);
            var randomReports = Enumerable.Range(1, 13).OrderBy(g => Guid.NewGuid()).Take(numReports).ToList();
            foreach (int reportNum in Enumerable.Range(1, 13).ToList())
                _gridWindow.gridSettings[$"Report{reportNum}"] = randomReports.Contains(reportNum);
            if (overwrite)
                Properties.Settings.Default.GridWindowNumReports = numReports;
        }

        private void UpdateUnlocks(bool overwrite)
        {
            // update visit unlocks
            // randomize which visit unlocks get included

            List<string> unlockNames = (MainWindow.data.VisitLocks.Select(item => item.Name)).ToList();
            int numUnlocks = int.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Visit Unlocks")?.Options.FirstOrDefault(o => o.Description == "Max Visit Unlocks")?.DefaultValue);                                                                              	
            var randomUnlocks = Enumerable.Range(1, unlockNames.Count).OrderBy(g => Guid.NewGuid()).Take(numUnlocks).ToList();                                                                                                                                                                                                                      
            foreach (int i in Enumerable.Range(1, unlockNames.Count).ToList())                                                                                                                                                                                                                                                                      
                _gridWindow.gridSettings[unlockNames[i - 1]] = randomUnlocks.Contains(i);
            if (overwrite)
                Properties.Settings.Default.GridWindowNumUnlocks = numUnlocks;
        }

        private void UpdateWorldChestLocks(bool overwrite) 
        {
            // update visit unlocks
            // randomize which visit unlocks get included
            List<string> worldChestLockNames = (MainWindow.data.VisitLocks.Select(item => item.Name)).ToList();
            int numChestLocks = int.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "World Chest Locks")?.Options.FirstOrDefault(o => o.Description == "Max World Chest Locks")?.DefaultValue);
            var randomChestLocks = Enumerable.Range(1, worldChestLockNames.Count).OrderBy(g => Guid.NewGuid()).Take(numChestLocks).ToList();
            foreach (int i in Enumerable.Range(1, worldChestLockNames.Count).ToList())
                _gridWindow.gridSettings[worldChestLockNames[i - 1]] = randomChestLocks.Contains(i);
            if (overwrite)
                Properties.Settings.Default.GridWindowNumChestLocks = numChestLocks;
        }

        private void UpdateMiscellaneous()
        {
            // update miscellaneous
            bool includeHadesCup = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Miscellaneous")?.Options.FirstOrDefault(o => o.Description == "Hades Cup Trophy")?.DefaultValue);
            bool includeOlympusStone = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Miscellaneous")?.Options.FirstOrDefault(o => o.Description == "Olympus Stone")?.DefaultValue);
            bool includeUnknownDisk = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Miscellaneous")?.Options.FirstOrDefault(o => o.Description == "Unknown Disk")?.DefaultValue);
            bool includeMunnyPouch1 = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Miscellaneous")?.Options.FirstOrDefault(o => o.Description == "Munny Pouches")?.DefaultValue);
            bool includeMunnyPouch2 = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Miscellaneous")?.Options.FirstOrDefault(o => o.Description == "Munny Pouches")?.DefaultValue);
            bool includeYeetTheBear = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Miscellaneous")?.Options.FirstOrDefault(o => o.Description == "Yeet the Bear")?.DefaultValue);
            bool includeAllMaxDrives = bool.Parse(categories.FirstOrDefault(c => c.CategoryName == "Allowed Checks")?.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == "Miscellaneous")?.Options.FirstOrDefault(o => o.Description == "All 7 Drives")?.DefaultValue);
            _gridWindow.gridSettings["HadesCup"] = includeHadesCup;
            _gridWindow.gridSettings["OlympusStone"] = includeOlympusStone;
            _gridWindow.gridSettings["UnknownDisk"] = includeUnknownDisk;
            _gridWindow.gridSettings["GridMunnyPouch1"] = includeMunnyPouch1;
            _gridWindow.gridSettings["GridMunnyPouch2"] = includeMunnyPouch2;
            _gridWindow.gridSettings["StarryHill"] = includeYeetTheBear;
            _gridWindow.gridSettings["Grid7Drives"] = includeAllMaxDrives;
        }

        public void UpdateGridSettings(Data data, bool overwrite=true)
        {
            UpdateGridSize(overwrite);
            UpdateGlobalSettings(overwrite);
            UpdateProgression(data);
            UpdateBosses(data);
            UpdateSuperbosses(data);
            UpdateMagics();
            UpdateSummons();
            UpdateDrives();
            UpdateProofs();
            UpdateSCOM();
            UpdateTornPages();
            UpdateWorldChestLocks(overwrite);
            UpdateUnlocks(overwrite);
            UpdateReports(overwrite);
            UpdateMiscellaneous();

            OnPropertyChanged(nameof(TrueChecksCount));
            OnPropertyChanged(nameof(NumSquares));

            // write the updated settings
            if (overwrite)
                Properties.Settings.Default.GridSettings = JsonSerializer.Serialize<Dictionary<string, bool>>(_gridWindow.gridSettings);
        }

        private void SavePresetJson(object sender, RoutedEventArgs e)
        {
            UpdateGridSettings(_data, _gridWindow.SavePreviousGridSettingsOption.IsChecked);
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON Files (*.json)|*.json"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                var combinedSettings = new
                {
                    _gridWindow.numRows,
                    _gridWindow.numColumns,
                    _gridWindow.bingoLogic,
                    _gridWindow.battleshipLogic,
                    _gridWindow.seedName,
                    _gridWindow.shipSizes,
                    _gridWindow.fogOfWar,
                    _gridWindow.fogOfWarSpan,
                    _gridWindow.gridSettings,
                    _gridWindow.minShipCount,
                    _gridWindow.maxShipCount,
                    _gridWindow.battleshipRandomCount
                };

                var jsonString = JsonSerializer.Serialize(combinedSettings);
                System.IO.File.WriteAllText(saveFileDialog.FileName, jsonString);
            }

            _gridWindow.numRows = originalSettings.numRows;
            _gridWindow.numColumns = originalSettings.numColumns;
            _gridWindow.bingoLogic = originalSettings.bingoLogic;
            _gridWindow.battleshipLogic = originalSettings.battleshipLogic;
            _gridWindow.seedName = originalSettings.seedName;
            _gridWindow.shipSizes = originalSettings.shipSizes;
            _gridWindow.fogOfWar = originalSettings.fogOfWar;
            _gridWindow.fogOfWarSpan = originalSettings.fogOfWarSpan;
            _gridWindow.gridSettings = originalSettings.gridSettings;
            _gridWindow.minShipCount = originalSettings.minShipCount;
            _gridWindow.maxShipCount = originalSettings.maxShipCount;                 
            _gridWindow.battleshipRandomCount = originalSettings.battleshipRandomCount;
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            UpdateGridSettings(_data, _gridWindow.SavePreviousGridSettingsOption.IsChecked);
            // generate new grid
            _gridWindow.grid.Children.Clear();
            _gridWindow.GenerateGrid(newNumRows, newNumColumns);
        }
    }

}
