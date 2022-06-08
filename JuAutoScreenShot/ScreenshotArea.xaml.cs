using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace JuAutoScreenShot
{
    /// <summary>
    /// Logique d'interaction pour ScreenshotArea.xaml
    /// </summary>
    public partial class ScreenshotArea : Window
    {
        MainWindow mainWindow;
        System.Windows.Point[] area = new System.Windows.Point[2];
        int nbValidPoints = 0;
        Button validationButton = new Button { Content = "Valider", Height = 30, Width = 100, Cursor = Cursors.Hand };


        public ScreenshotArea()
        {
            InitializeComponent();
        }

        public ScreenshotArea(Bitmap screen, MainWindow mainWindow)
        {
            InitializeComponent();

            this.mainWindow = mainWindow;
            using (MemoryStream memory = new MemoryStream())
            {
                screen.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                // img_background.Source = bitmapimage;
                ImageBrush ib = new ImageBrush();
                ib.ImageSource = bitmapimage;
                canvas_screeshot.Background = ib;

                System.Windows.Shapes.Rectangle border = new System.Windows.Shapes.Rectangle
                {
                    Width = bitmapimage.Width,
                    Height = bitmapimage.Height,
                    Stroke = System.Windows.Media.Brushes.Red,
                    StrokeThickness = 4,
                    Margin = new Thickness(0, 0, 0, 0)

                };
                canvas_screeshot.Children.Add(border);

                validationButton.Click += new RoutedEventHandler(button_validation_Click);

            }
        }

        private void canvas_screeshot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point coord = e.GetPosition(this);

            if (nbValidPoints == 0 || nbValidPoints == 2)
            {
                area[0] = coord;
                if (nbValidPoints == 2) canvas_screeshot.Children.RemoveAt(2);
                nbValidPoints = 1;
            }
            else if (nbValidPoints == 1)
            {
                area[1] = coord;
                nbValidPoints = 2;
                canvas_screeshot.Children.Add(validationButton);
            }
        }

        private void canvas_screeshot_MouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point firstPoint = area[0];

            if (nbValidPoints == 1)
            {
                System.Windows.Point point = e.GetPosition(this);

                double rectWidth = Math.Abs(firstPoint.X - point.X);
                double rectHeight = Math.Abs(firstPoint.Y - point.Y);
                double marginTop = Math.Min(firstPoint.Y, point.Y);
                double marginLeft = Math.Min(firstPoint.X, point.X);
                if (canvas_screeshot.Children.Count > 1)
                {
                    System.Windows.Shapes.Rectangle rect = (System.Windows.Shapes.Rectangle)canvas_screeshot.Children[1];
                    rect.Width = rectWidth;
                    rect.Height = rectHeight;
                    rect.Margin = new Thickness(marginLeft, marginTop, 0, 0);
                }
                else
                {
                    SolidColorBrush filledColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
                    filledColor.Opacity = 0.15;
                    System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle
                    {
                        Width = Math.Abs(firstPoint.X - point.X),
                        Height = Math.Abs(firstPoint.Y - point.Y),
                        Stroke = System.Windows.Media.Brushes.White,
                        Fill = filledColor,
                        StrokeThickness = 1,
                        Margin = new Thickness(marginLeft, marginTop, 0, 0),
                    };
                    canvas_screeshot.Children.Add(rect);
                }

                validationButton.Margin = new Thickness(marginLeft + (rectWidth - 100) / 2, marginTop + (rectHeight - 30) / 2, 0, 0);

            }
        }

        private void button_validation_Click(object sender, RoutedEventArgs args)
        {
            System.Windows.Point[] absoluteArea = {
                PointToScreen(this.area[0]),
                PointToScreen(this.area[1]),
            };
            mainWindow.setPoints(absoluteArea);
            mainWindow.WindowState = WindowState.Normal;
            Debug.WriteLine(mainWindow.Dimension);
            this.Close();
        }
    }
}
