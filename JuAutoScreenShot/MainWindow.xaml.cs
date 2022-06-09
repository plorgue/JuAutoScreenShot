using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Point = System.Windows.Point;
using Screen = System.Windows.Forms.Screen;

namespace JuAutoScreenShot
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public bool IsStarted = false;
        public Point[] Points = new Point[2];
        public bool HaveTowPts = false;
        public Timer timerLoop;

        private int count = 0;

        public int Count
        {
            get => count++;
            private set => count = value;
        }
        public void resetCount() => count = 0;


        private string dirOutput;
        public string DirOutput
        {
            get { return dirOutput; }
            set
            { dirOutput = value; }
        }

        private float frequency;

        public float Frequency
        {
            get { return frequency; }
            set
            { frequency = value; }
        }

        public string Position
        {
            get { return "" + (int)Points[0].X + "px / " + (int)Points[0].Y + "px"; }
            private set { }
        }

        public string Dimension
        {
            get { return "" + (int)Math.Abs(Points[0].X - Points[1].X) + "px / " + (int)Math.Abs(Points[0].Y - Points[1].Y) + "px"; }
            private set { }
        }

        public bool CanStart
        {
            get { return HaveTowPts && Frequency > 0 && Directory.Exists(DirOutput); }
            private set { }
        }
        public void setPoints(Point[] pts)
        {
            this.Points = pts;
            tblock_dim.Text = Dimension;
            tblock_loc.Text = Position;
            HaveTowPts = true;
            btn_start.IsEnabled = CanStart;
        }

        private Bitmap GetSreenshot()
        {
            Bitmap bm = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics g = Graphics.FromImage(bm);
            g.CopyFromScreen(0, 0, 0, 0, bm.Size);
            return bm;
        }

        public MainWindow()
        {
            InitializeComponent();
            this.Frequency = 10;
            this.DataContext = this;
        }

        private void Button_Click_Start(object sender, RoutedEventArgs e)
        {
            if (!this.IsStarted)
            {
                AutoResetEvent autoEvent = new AutoResetEvent(false);

                TimeSpan startTimeSpan = TimeSpan.Zero;
                TimeSpan periodTimeSpan = TimeSpan.FromSeconds(Frequency);
                TimerCallback timerDelegate = new TimerCallback(ScreenShotLoop);
                timerLoop = new Timer(timerDelegate, autoEvent, startTimeSpan, periodTimeSpan);
                this.IsStarted = true;
                btn_start.Content = "Stop";
                btn_selectArea.IsEnabled = false;
                tbox_folder.IsEnabled = false;
                tbox_frequency.IsEnabled = false;

                dirOutput = dirOutput.Trim();
                if (dirOutput.EndsWith("\\") || dirOutput.EndsWith("/")) dirOutput = dirOutput.Substring(0, dirOutput.Length - 1);
                Directory.CreateDirectory(string.Format("{0}\\{1}", dirOutput, "base64"));
            }
            else
            {
                timerLoop.Dispose();
                resetCount();
                btn_start.Content = "Start";
                //tblock_count.Text = "";
                btn_selectArea.IsEnabled = true;
                tbox_folder.IsEnabled = true;
                tbox_frequency.IsEnabled = true;
            }
        }

        private void Button_Click_Area(object sender, RoutedEventArgs e)
        {
            if (IsStarted == false)
            {
                this.Hide();
                Bitmap screen = GetSreenshot();
                ScreenshotArea screeshotArea = new ScreenshotArea(screen, this);
                screeshotArea.Show();
            }
        }

        private void onTextChanged(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            switch (textBox.Name)
            {
                case "tbox_folder":
                    DirOutput = textBox.Text;
                    break;
                case "tbox_frequency":
                    float freq = 0;
                    float.TryParse(textBox.Text, out freq);
                    Frequency = freq;
                    break;
            }
            btn_start.IsEnabled = CanStart;
        }

        public static Bitmap Crop(Bitmap src, int x, int y, int w, int h)
        {
            Bitmap target = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(src, new Rectangle(0, 0, w, h), x, y, w, h, GraphicsUnit.Pixel);
                return target;
            }
        }

        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

        private static byte[] BitmapToBase64(Bitmap image)
        {
            string base64String = string.Empty;


            using (MemoryStream memoryStream = new MemoryStream())
            {
                image.Save(memoryStream, ImageFormat.Bmp);

                memoryStream.Position = 0;
                byte[] byteBuffer = memoryStream.ToArray();

                base64String = Convert.ToBase64String(byteBuffer);
                byteBuffer = null;
            }

            return Convert.FromBase64String(base64String);
        }

        private static Bitmap Base64ToBitmap(byte[] base64)
        {
            string base64string = Convert.ToBase64String(base64);
            byte[] byteBuffer = Convert.FromBase64String(base64string);

            /// If close stream an error is raised when save the bitmap
            /// It sould be fixed 
            //using (MemoryStream memoryStream = new MemoryStream(byteBuffer)) {}
            MemoryStream memoryStream = new MemoryStream(byteBuffer);
            memoryStream.Position = 0;
            Bitmap bmpReturn = (Bitmap)System.Drawing.Image.FromStream(memoryStream);
            return bmpReturn;
        }

        public void ScreenShotLoop(object obj)
        {
            Debug.WriteLine($"Loop:{Count}");

            Bitmap screen = GetSreenshot();
            int x = (int)Math.Min(Points[0].X, Points[1].X);
            int y = (int)Math.Min(Points[0].Y, Points[1].Y);
            int w = (int)Math.Abs(Points[0].X - Points[1].X);
            int h = (int)Math.Abs(Points[0].Y - Points[1].Y);
            Bitmap screenCrop = Crop(screen, x, y, w, h);

            string now = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture).Substring(0, 19).Replace(':', '-');

            // Save base64
            byte[] base64btm = BitmapToBase64(screenCrop);
            File.WriteAllBytes(string.Format("{0}\\{1}\\{2}", DirOutput, "base64", now), base64btm);

            // Decode base64
            // Bitmap screenCropDecode = Base64ToBitmap(base64btm);
            // Debug.WriteLine(screenCrop.Width);

            // Save as jpg
            string path = string.Format("{0}\\{1}.jpg", dirOutput, now);
            ImageCodecInfo myImageCodecInfo = GetEncoderInfo("image/jpeg");
            Encoder myEncoder = Encoder.Quality;
            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 95L);
            myEncoderParameters.Param[0] = myEncoderParameter;
            screenCrop.Save(path, myImageCodecInfo, myEncoderParameters);
            // screenCropDecode.Save(path, myImageCodecInfo, myEncoderParameters);

            screen.Dispose();
            screenCrop.Dispose();
            // screenCropDecode.Dispose();
        }
    }
}
