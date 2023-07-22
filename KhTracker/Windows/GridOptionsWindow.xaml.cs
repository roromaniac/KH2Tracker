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
    }
}
