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
    }

    public partial class GridOptionsWindow : Window
    {
        public GridOptionsWindow()
        {
            InitializeComponent();

            List<Category> categories = new List<Category>
            {
                new Category { CategoryName = "Magics" },
                new Category { CategoryName = "Drives" },
                new Category { CategoryName = "Bosses" },
                new Category { CategoryName = "Reports" },
                new Category { CategoryName = "Summons" },
                new Category { CategoryName = "Category 6" },
                new Category { CategoryName = "Category 7" },
                new Category { CategoryName = "Category 8" },
                new Category { CategoryName = "Category 9" },
                new Category { CategoryName = "Category 10" },
            };

            this.DataContext = categories;

        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = "Enter text...";
            }
        }

        private void SetNumRows(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text == "Enter text...")
            {
                textBox.Text = "";
            }
            else
            {
                try
                {
                    GridWindow.numRows = Convert.ToInt32(textBox.Text);
                }
                catch (Exception)
                {
                    Console.WriteLine("You need to input an integer value for the number of rows.");
                }
            }
        }

        private void SetNumColumns(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text == "Enter text...")
            {
                textBox.Text = "";
            }
            else
            {
                try
                {
                    GridWindow.numColumns = Convert.ToInt32(textBox.Text);
                    Console.WriteLine(GridWindow.numColumns);
                }
                catch (Exception)
                {
                    Console.WriteLine("You need to input an integer value for the number of columns.");
                }
            }
        }
    }
}
