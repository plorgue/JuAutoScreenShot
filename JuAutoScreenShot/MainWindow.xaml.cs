using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Screen = System.Windows.Forms.Screen;
using Point = System.Windows.Point;
using System.Windows.Media.Imaging;
using System.Globalization;
using System.Drawing.Imaging;

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

        public void setPoints(Point[] pts)
        {
            this.Points = pts;
            tblock_dim.Text = Dimension;
            tblock_loc.Text = Position;
            HaveTowPts = true;
            btn_start.IsEnabled = CanStart;
        }

        private int count = 0;

        public int Count
        {
            get => count++;
            private set => count = value;
        }


        private string dirOutput;
        public string DirOutput
        {
            get { return dirOutput; }
            set
            {   dirOutput = value;  }
        }

        private float frequency;

        public float Frequency
        {
            get { return frequency; }
            set
            {   frequency = value; }
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

        //int[] position = new int[] { 0, 0 };
        //int[] dimension = new int[] { 0, 0 };

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
            TimeSpan startTimeSpan = TimeSpan.Zero;
            TimeSpan periodTimeSpan = TimeSpan.FromSeconds(Frequency);

            System.Threading.Timer timer = new System.Threading.Timer((obj) =>
            {
                SceenShotLoop();
            }, null, startTimeSpan, periodTimeSpan);
        }

        private void Button_Click_Area(object sender, RoutedEventArgs e)
        {
            if (IsStarted == false)
            {
                // WindowState = WindowState.Minimized;

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

        public void SceenShotLoop()
        {
            //Bitmap screen = GetSreenshot();
            //int x = (int)Math.Min(Points[0].X, Points[1].X);
            //int y = (int)Math.Min(Points[0].Y, Points[1].Y);
            //int w = (int)Math.Abs(Points[0].X - Points[1].X);
            //int h = (int)Math.Abs(Points[0].Y - Points[1].Y);
            //Bitmap screenCrop = Crop(screen, x, y, w, h);
            //
            //string now = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture).Substring(0,19).Replace(':', '-');
            //string path = string.Format("{0}\\{1}.jpg", dirOutput, now);
            //
            //
            //ImageCodecInfo myImageCodecInfo = GetEncoderInfo("image/jpeg");
            //
            //Encoder myEncoder = Encoder.Quality;
            //EncoderParameters myEncoderParameters = new EncoderParameters(1);
            //EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 100L);
            //myEncoderParameters.Param[0] = myEncoderParameter;
            //
            //screenCrop.Save(path, myImageCodecInfo, myEncoderParameters);
            Debug.WriteLine("Loop:"+Count);
        }
    }
}
