using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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

        public static int numRows = 5;
        public static int numColumns = 5;

        private Grid grid;
        private ToggleButton[,] buttons;


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
            GridOptionsWindow gridWindow = new GridOptionsWindow();
            gridWindow.ShowDialog();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = (ToggleButton)sender;
            if (button.Background == Brushes.LightGray)
            {
                button.Background = Brushes.Green;
            }
            else
            {
                button.Background = Brushes.LightGray;
            }

            BingoCheck(grid);
        }

        public void GenerateGrid(int numRows = 5, int numColumns = 5)
        {
            grid = new Grid();
            buttons = new ToggleButton[numRows, numColumns];

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
                    button.Background = Brushes.LightGray;
                    button.SetResourceReference(ContentProperty, "Min-Axel1");
                    button.Style = (Style)FindResource("ColorToggleButton");
                    button.Click += Button_Click;
                    Grid.SetRow(button, i);
                    Grid.SetColumn(button, j);
                    buttons[i, j] = button;
                    grid.Children.Add(button);
                }
            }
            // Add grid to the window or other container
            DynamicGrid.Children.Add(grid);
        }
        public void BingoCheck(Grid grid)
        {

            int rowCount = grid.RowDefinitions.Count;
            int columnCount = grid.ColumnDefinitions.Count;

            // check for horizontal bingos
            for (int row = 0; row < rowCount; row++)
            {
                
                int col;

                for (col = 0; col < columnCount; col++)
                {
                    if (buttons[row, col].IsChecked == false)
                    {
                        break;
                    }
                    else
                    {
                        if (col == columnCount - 1)
                        {
                            for (col = 0; col < columnCount; col++)
                            {
                                buttons[row, col].Background = Brushes.Purple;
                            }
                        }
                    }

                }

            }

            // check for vertical bingos
            for (int col = 0; col < columnCount; col++)
            {

                int row;

                for (row = 0; row < rowCount; row++)
                {
                    if (buttons[row, col].IsChecked == false)
                    {
                        break;
                    }
                    else
                    {
                        if (row == rowCount - 1)
                        {
                            for (row = 0; row < rowCount; row++)
                            {
                                buttons[row, col].Background = Brushes.Purple;
                            }
                        }
                    }

                }

            }

            // check for diagonal bingos
            if (rowCount == columnCount)
            {

                // left diagonal
                for (int index = 0; index < rowCount; index++)
                {
                    if (buttons[index, index].IsChecked == false)
                    {
                        break;
                    }
                    else
                    {
                        if (index == rowCount - 1)
                        {
                            for (index = 0; index < rowCount; index++)
                            {
                                buttons[index, index].Background = Brushes.Purple;
                            }
                        }
                    }
                }


                // right diagonal
                for (int index = 0; index < rowCount; index++)
                {
                    if (buttons[index, rowCount - index - 1].IsChecked == false)
                    {
                        break;
                    }
                    else
                    {
                        if (index == rowCount - 1)
                        {
                            for (index = 0; index < rowCount; index++)
                            {
                                buttons[index, rowCount - index - 1].Background = Brushes.Purple;
                            }
                        }
                    }
                }
            }
            
        }
    }
}
