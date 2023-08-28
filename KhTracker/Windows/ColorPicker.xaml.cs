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
    public partial class ColorPickerWindow : Window
    {
        public Color SelectedColor { get; private set; }
        public Dictionary<string, Color> ButtonColors;
        private Button LastClickedButton;

        public ColorPickerWindow(Dictionary<string, Color> currentColors)
        {
            InitializeComponent();

            // Initialize the button colors
            ButtonColors = currentColors ?? new Dictionary<string, Color>
            {
                { "Unmarked Color", Colors.DimGray },
                { "Marked Color", Colors.Green },
                { "Annotated Color", Colors.Orange },
                { "Bingo Color", Colors.Purple }
            };

            // Set button colors initially
            UnmarkedColorButton.Background = new SolidColorBrush(ButtonColors["Unmarked Color"]);
            MarkedColorButton.Background = new SolidColorBrush(ButtonColors["Marked Color"]);
            AnnotatedColorButton.Background = new SolidColorBrush(ButtonColors["Annotated Color"]);
            BingoColorButton.Background = new SolidColorBrush(ButtonColors["Bingo Color"]);
        }


        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                LastClickedButton = button;
                Color selectedColor = ButtonColors[(string)button.Content];
                RedSlider.Value = selectedColor.R;
                GreenSlider.Value = selectedColor.G;
                BlueSlider.Value = selectedColor.B;
                UpdateColorPreview();
                ColorControls.Visibility = Visibility.Visible;
            }
        }

        private void SelectColor_Click(object sender, RoutedEventArgs e)
        {
            SelectedColor = Color.FromRgb((byte)RedSlider.Value, (byte)GreenSlider.Value, (byte)BlueSlider.Value);
            LastClickedButton.Background = new SolidColorBrush(SelectedColor);
            ButtonColors[(string)LastClickedButton.Content] = SelectedColor; // Update the dictionary
            SaveColorSettings(ButtonColors);// Save the dictionary
        }

        private void ColorSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var red = (byte)RedSlider.Value;
            var green = (byte)GreenSlider.Value;
            var blue = (byte)BlueSlider.Value;
            var rgbColor = Color.FromRgb(red, green, blue);
            var color = new SolidColorBrush(rgbColor);
            PreviewBorder.Background = color;
            SelectedColor = rgbColor;
        }

        private void UpdateColorPreview()
        {
            var red = (byte)RedSlider.Value;
            var green = (byte)GreenSlider.Value;
            var blue = (byte)BlueSlider.Value;
            var rgbColor = Color.FromRgb(red, green, blue);
            PreviewBorder.Background = new SolidColorBrush(rgbColor);
            SelectedColor = rgbColor;
        }

        private void SaveColorSettings(Dictionary<string, Color> colorSettings)
        {
            string serializedSettings = JsonSerializer.Serialize(colorSettings);

            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\GridTrackerColors");
            key.SetValue("Settings", serializedSettings);
            key.Close();
        }
    }

}
