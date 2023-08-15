using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public GridOptionsWindow()
        {
            InitializeComponent();

            List<Category> categories = new List<Category>
            {
                new Category { 
                    CategoryName = "Tracker Settings",
                    SubCategories = new List<SubCategory>
                    {
                        new SubCategory { 
                            SubCategoryName = "Board Size",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.TextBox, Description = "Number of Rows", DefaultValue = "5" },
                                new Option { Type = OptionType.TextBox, Description = "Number of Columns", DefaultValue = "5"  }
                            }
                        },
                        new SubCategory { 
                            SubCategoryName = "Bingo Logic",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.CheckBox, Description = "Include Bingo Logic" },
                            }
                        },
                        new SubCategory { 
                            SubCategoryName = "Battleship Logic" ,
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.CheckBox, Description = "Include Battleship Logic" },
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
                        },
                        new SubCategory { 
                            SubCategoryName = "Progression",
                        },
                        new SubCategory { 
                            SubCategoryName = "Magics",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.CheckBox, Description = "Level 1 Magics" },
                                new Option { Type = OptionType.CheckBox, Description = "Level 2 Magics" },
                                new Option { Type = OptionType.CheckBox, Description = "Level 3 Magics" },
                            }
                        },
                        new SubCategory { 
                            SubCategoryName = "Summons",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.CheckBox, Description = "Summons" },
                            }
                        },
                        new SubCategory { 
                            SubCategoryName = "Drives",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.CheckBox, Description = "Drives" },
                                new Option { Type = OptionType.CheckBox, Description = "Light & Darkness Counts as Final" },
                            }
                        },
                        new SubCategory { 
                            SubCategoryName = "Proofs",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.CheckBox, Description = "Proof of Connection" },
                                new Option { Type = OptionType.CheckBox, Description = "Proof of Nonexistence" },
                                new Option { Type = OptionType.CheckBox, Description = "Proof of Peace" },
                                new Option { Type = OptionType.CheckBox, Description = "Promise Charm" },
                            }
                        },
                        new SubCategory { 
                            SubCategoryName = "SC/OM",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.CheckBox, Description = "SC/OM" },
                            }
                        },
                        new SubCategory { 
                            SubCategoryName = "Torn Pages",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.CheckBox, Description = "Torn Page 1" },
                                new Option { Type = OptionType.CheckBox, Description = "Torn Page 2" },
                                new Option { Type = OptionType.CheckBox, Description = "Torn Page 3" },
                                new Option { Type = OptionType.CheckBox, Description = "Torn Page 4" },
                                new Option { Type = OptionType.CheckBox, Description = "Torn Page 5" },
                            }
                        },
                        new SubCategory { 
                            SubCategoryName = "Reports",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.TextBox, Description = "Max Reports", DefaultValue = "13" },
                            }
                        },
                        new SubCategory { 
                            SubCategoryName = "Visit Unlocks",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.TextBox, Description = "Max Visit Unlocks", DefaultValue = "11" },
                            }
                        },
                        new SubCategory { 
                            SubCategoryName = "Miscellaneous",
                            Options = new List<Option>
                            {
                                new Option { Type = OptionType.CheckBox, Description = "Hades Cup Trophy" },
                                new Option { Type = OptionType.CheckBox, Description = "Olympus Stone" },
                                new Option { Type = OptionType.CheckBox, Description = "Unknown Disk" },
                                new Option { Type = OptionType.CheckBox, Description = "Munny Pouches" },
                            }
                        },
                    }
                },
            };

            this.DataContext = categories;

        }
        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
