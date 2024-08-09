﻿using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
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
        public Dictionary<string, Color> ObjButtonColors;
        private Button LastClickedButton;
        public bool canClose = false;
        public IColorableWindow colorableWindow;

        public ColorPickerWindow(IColorableWindow window, Dictionary<string, Color> currentColors, bool objWindow = false)
        {
            colorableWindow = window;

            InitializeComponent();

            if (!objWindow)
            {
                // Initialize the button colors
                // NOTE: This will make the ButtonColors tied to currentColors with the exact same memory 
                ButtonColors = currentColors ?? new Dictionary<string, Color>
                {
                    { "Unmarked Color", Colors.DimGray },
                    { "Marked Color", Colors.Green },
                    { "Annotated Color", Colors.Orange },
                    { "Bingo Color", Colors.Purple },
                    { "Hint Color", Colors.White },
                    { "Battleship Miss Color", Colors.DeepSkyBlue },
                    { "Battleship Hit Color", Colors.Red },
                    { "Battleship Sunk Color", Colors.Pink }
                };

                // Set button colors and foreground text colors initially
                UnmarkedColorButton.Background = new SolidColorBrush(ButtonColors["Unmarked Color"]);
                SetForegroundColor(ButtonColors["Unmarked Color"], UnmarkedColorButton);
                MarkedColorButton.Background = new SolidColorBrush(ButtonColors["Marked Color"]);
                SetForegroundColor(ButtonColors["Marked Color"], MarkedColorButton);
                AnnotatedColorButton.Background = new SolidColorBrush(ButtonColors["Annotated Color"]);
                SetForegroundColor(ButtonColors["Annotated Color"], AnnotatedColorButton);
                BingoColorButton.Background = new SolidColorBrush(ButtonColors["Bingo Color"]);
                SetForegroundColor(ButtonColors["Bingo Color"], AnnotatedColorButton);
                HintColorButton.Background = new SolidColorBrush(ButtonColors["Hint Color"]);
                SetForegroundColor(ButtonColors["Hint Color"], HintColorButton);
                BattleshipMissColorButton.Background = new SolidColorBrush(ButtonColors["Battleship Miss Color"]);
                SetForegroundColor(ButtonColors["Battleship Miss Color"], BattleshipMissColorButton);
                BattleshipHitColorButton.Background = new SolidColorBrush(ButtonColors["Battleship Hit Color"]);
                SetForegroundColor(ButtonColors["Battleship Hit Color"], BattleshipHitColorButton);
                BattleshipSunkColorButton.Background = new SolidColorBrush(ButtonColors["Battleship Sunk Color"]);
                SetForegroundColor(ButtonColors["Battleship Sunk Color"], BattleshipSunkColorButton);
            }
            else
            {
                // Initialize the button colors
                ObjButtonColors = currentColors ?? new Dictionary<string, Color>
                {
                    { "Uncollected Color", Colors.DimGray },
                    { "Collected Color", Colors.Green },
                    { "Marked Color", Colors.Orange },
                    { "Win Condition Met Color", Colors.Purple },
                };

                // Set button colors and foreground text colors initially
                ObjUnmarkedColorButton.Background = new SolidColorBrush(ObjButtonColors["Uncollected Color"]);
                SetForegroundColor(ObjButtonColors["Uncollected Color"], UnmarkedColorButton);

                ObjCollectedColorButton.Background = new SolidColorBrush(ObjButtonColors["Collected Color"]);
                SetForegroundColor(ObjButtonColors["Collected Color"], MarkedColorButton);

                ObjAnnotatedColorButton.Background = new SolidColorBrush(ObjButtonColors["Marked Color"]);
                SetForegroundColor(ObjButtonColors["Marked Color"], AnnotatedColorButton);

                ObjCompletedColorButton.Background = new SolidColorBrush(ObjButtonColors["Win Condition Met Color"]);
                SetForegroundColor(ObjButtonColors["Win Condition Met Color"], AnnotatedColorButton);

                UnmarkedColorButton.Visibility = Visibility.Collapsed;
                MarkedColorButton.Visibility = Visibility.Collapsed;
                AnnotatedColorButton.Visibility = Visibility.Collapsed;
                BingoColorButton.Visibility = Visibility.Collapsed;
                HintColorButton.Visibility = Visibility.Collapsed;
                BattleshipMissColorButton.Visibility = Visibility.Collapsed;
                BattleshipHitColorButton.Visibility = Visibility.Collapsed;
                BattleshipSunkColorButton.Visibility = Visibility.Collapsed;

                ObjUnmarkedColorButton.Visibility = Visibility.Visible;
                ObjCollectedColorButton.Visibility = Visibility.Visible;
                ObjAnnotatedColorButton.Visibility = Visibility.Visible;
                ObjCompletedColorButton.Visibility = Visibility.Visible;
            }
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.ColorWindowY = RestoreBounds.Top;
            Properties.Settings.Default.ColorWindowX = RestoreBounds.Left;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Properties.Settings.Default.ColorWindowWidth = RestoreBounds.Width;
            Properties.Settings.Default.ColorWindowHeight = RestoreBounds.Height;
        }

        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            colorableWindow.HandleClosing(this);
            Hide();
            ColorControls.Visibility = Visibility.Collapsed;
            if (!canClose)
            {
                e.Cancel = true;
            }
        }

        private void SetForegroundColor(Color color, Button button)
        {
            // Calculate the luminance of the SelectedColor
            double luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
            // If luminance is greater than 0.5, the color is closer to white, so use a dark color for the foreground; otherwise, use a light color.
            if (luminance > 0.5)
            {
                button.Foreground = new SolidColorBrush(Colors.Black); // Dark foreground for lighter backgrounds
            }
            else
            {
                button.Foreground = new SolidColorBrush(Colors.White); // Light foreground for darker backgrounds
            }
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                // Cast the Content to TextBlock
                if (button.Content is TextBlock textBlock)
                {
                    // sets the background of button to its current color
                    LastClickedButton = button;
                    if (button.Name.StartsWith("Obj"))
                        SelectedColor = ObjButtonColors[textBlock.Text]; // Use buttonText here
                    else
                        SelectedColor = ButtonColors[textBlock.Text]; // Use buttonText here

                    // update the preview background
                    PreviewBorder.Background = new SolidColorBrush(SelectedColor);

                    // update the foreground text
                    SetForegroundColor(SelectedColor, button);

                    // reveals the color slider
                    ColorControls.Visibility = Visibility.Visible;
                }
            }
        }

        private void SelectColor_Click(object sender, RoutedEventArgs e)
        {
            LastClickedButton.Background = new SolidColorBrush(SelectedColor);
            var lastClickedTextBlock = LastClickedButton.Content as TextBlock;  

            if (LastClickedButton.Name.StartsWith("Obj"))
            {
                ObjButtonColors[lastClickedTextBlock.Text] = SelectedColor; // Update the dictionary
                ObjSaveColorSettings(lastClickedTextBlock.Text, SelectedColor); // Save the dictionary
            }
            else
            {
                ButtonColors[lastClickedTextBlock.Text] = SelectedColor; // Update the dictionary
                SaveColorSettings(lastClickedTextBlock.Text, SelectedColor); // Save the dictionary
            }

            SetForegroundColor(SelectedColor, LastClickedButton);
        }

        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            SelectedColor = e.NewValue ?? Colors.Transparent;
            PreviewBorder.Background = new SolidColorBrush(SelectedColor);
        }

        private void SaveColorSettings(string colorType, Color newColor)
        {
            switch (colorType)
            {
                case "Unmarked Color":
                    Properties.Settings.Default.UnmarkedColor = newColor;
                    break;
                case "Marked Color":
                    Properties.Settings.Default.MarkedColor = newColor;
                    break;
                case "Annotated Color":
                    Properties.Settings.Default.AnnotatedColor = newColor;
                    break;
                case "Bingo Color":
                    Properties.Settings.Default.BingoColor = newColor;
                    break;
                case "Hint Color":
                    Properties.Settings.Default.HintColor = newColor;
                    break;
                case "Battleship Miss Color":
                    Properties.Settings.Default.BattleshipMissColor = newColor;
                    break;
                case "Battleship Hit Color":
                    Properties.Settings.Default.BattleshipHitColor = newColor;
                    break;
                case "Battleship Sunk Color":
                    Properties.Settings.Default.BattleshipSunkColor = newColor;
                    break;

                default:
                    Console.WriteLine("Color type not implemented.");
                    break;
            }
        }

        private void ObjSaveColorSettings(string colorType, Color newColor)
        {
            switch (colorType)
            {
                case "Uncollected Color":
                    Properties.Settings.Default.ObjUnmarkedColorButton = newColor;
                    break;
                case "Collected Color":
                    Properties.Settings.Default.ObjCollectedColorButton = newColor;
                    break;
                case "Marked Color":
                    Properties.Settings.Default.ObjAnnotatedColorButton = newColor;
                    break;
                case "Win Condition Met Color":
                    Properties.Settings.Default.ObjCompletedColorButton = newColor;
                    break;
                default:
                    Console.WriteLine("Color type not implemented.");
                    break;
            }
        }
    }

}
