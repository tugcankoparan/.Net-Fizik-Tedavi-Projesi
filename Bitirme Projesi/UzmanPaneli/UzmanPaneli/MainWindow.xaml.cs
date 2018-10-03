
namespace UzmanPaneli
{
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using Microsoft.Kinect;
    using System.Windows.Media.Media3D;
    using System;
    using System.Threading;
    using System.Data.SqlClient;



    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static bool valqq = false;
        public static int count = 0;
        public static double NeckAngleRightX;
        public static double NeckAngleRightY;
        public static double NeckAngleLeftX;
        public static double NeckAngleLeftY;

        public static double ElbowRightX;
        public static double ElbowRightY;
        public static double ElbowLeftX;
        public static double ElbowLeftY;

        public static double KneeRightX;
        public static double KneeRightY;
        public static double KneeLeftX;
        public static double KneeLeftY;

        public static double KneeDistance;

        ///Deneme 
        ///
        public static double ElboxRightXD;
        public static double ElbowLeftxD;
        public static double KneeRightXD;
        public static double KneeLeftXD;
        JointCollection joints;
        public static Skeleton skeleton;

        /// <summary>
        /// Width of output drawing
        /// </summary>
        private const float RenderWidth = 640.0f;

        /// <summary>
        /// Height of our output drawing
        /// </summary>
        private const float RenderHeight = 480.0f;

        /// <summary>
        /// Thickness of drawn joint lines
        /// </summary>
        private const double JointThickness = 3;

        /// <summary>
        /// Thickness of body center ellipse
        /// </summary>
        private const double BodyCenterThickness = 10;

        /// <summary>
        /// Thickness of clip edge rectangles
        /// </summary>
        private const double ClipBoundsThickness = 10;

        /// <summary>
        /// Brush used to draw skeleton center point
        /// </summary>
        private readonly Brush centerPointBrush = Brushes.Blue;

        /// <summary>
        /// Brush used for drawing joints that are currently tracked
        /// </summary>
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));

        /// <summary>
        /// Brush used for drawing joints that are currently inferred
        /// </summary>        
        private readonly Brush inferredJointBrush = Brushes.Yellow;

        /// <summary>
        /// Pen used for drawing bones that are currently tracked
        /// </summary>
        private readonly Pen trackedBonePen = new Pen(Brushes.Green, 6);

        /// <summary>
        /// Pen used for drawing bones that are currently inferred
        /// </summary>        
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor sensor;

        /// <summary>
        /// Drawing group for skeleton rendering output
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

        }

        /// <summary>
        /// Draws indicators to show which edges are clipping skeleton data
        /// </summary>
        /// <param name="skeleton">skeleton to draw clipping information for</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private static void RenderClippedEdges(Skeleton skeleton, DrawingContext drawingContext)
        {
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, RenderHeight - ClipBoundsThickness, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, RenderHeight));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(RenderWidth - ClipBoundsThickness, 0, ClipBoundsThickness, RenderHeight));
            }
        }

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {

            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

            // Display the drawing using our image control
            Image.Source = this.imageSource;

            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                // Turn on the skeleton stream to receive skeleton frames
                this.sensor.SkeletonStream.Enable();

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;

                // Start the sensor!
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
        }

        /// <summary>
        /// Event handler for Kinect sensor's SkeletonFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);

                }
            }

            using (DrawingContext dc = this.drawingGroup.Open())
            {
                // Draw a transparent background to set the render size
                dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));

                if (skeletons.Length != 0)
                {
                    foreach (Skeleton skel in skeletons)
                    {
                        RenderClippedEdges(skel, dc);

                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            this.DrawBonesAndJoints(skel, dc);
                        }
                        else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                        {
                            dc.DrawEllipse(
                            this.centerPointBrush,
                            null,
                            this.SkeletonPointToScreen(skel.Position),
                            BodyCenterThickness,
                            BodyCenterThickness);
                        }
                    }
                }

                // prevent drawing outside of our render area
                this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
            }
        }


        public bool vucutdikMi(JointCollection joints)
        {
            if (degerlerYakin(0.1f, joints[JointType.Spine].Position.X, joints[JointType.ShoulderCenter].Position.X))
            {
                return true;

            }
            else
            {
                return false;
            }
        }











        /// <summary>
        /// Draws a skeleton's bones and joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawBonesAndJoints(Skeleton skeleton, DrawingContext drawingContext)
        {




            double tempVal;

            ///  Boyunun Left-Right ve eksenlere göre açıları hesaplanması.
            tempVal = Math.Cos(skeleton.Joints[JointType.Head].Position.X - skeleton.Joints[JointType.ShoulderRight].Position.X);
            tempVal = Math.Acos(tempVal);
            NeckAngleRightX = 7 * tempVal * (180 / Math.PI) - 10;


            tempVal = Math.Cos(skeleton.Joints[JointType.Head].Position.X - skeleton.Joints[JointType.ShoulderLeft].Position.X);
            tempVal = Math.Acos(tempVal);
            NeckAngleLeftX = 7 * tempVal * (180 / Math.PI) - 10;

            tempVal = Math.Cos(skeleton.Joints[JointType.Head].Position.Y - skeleton.Joints[JointType.ShoulderRight].Position.Y);
            tempVal = Math.Acos(tempVal);
            NeckAngleRightY = 7 * tempVal * (180 / Math.PI) - 10;

            tempVal = Math.Cos(skeleton.Joints[JointType.Head].Position.X - skeleton.Joints[JointType.ShoulderRight].Position.X);
            tempVal = Math.Acos(tempVal);
            NeckAngleLeftY = 7 * tempVal * (180 / Math.PI) - 10;


            ///  Dirsek Left-Right ve eksenlere göre açıları hesaplanması.
            tempVal = Math.Cos(skeleton.Joints[JointType.WristRight].Position.X - skeleton.Joints[JointType.ShoulderRight].Position.X);
            tempVal = Math.Acos(tempVal);
            ElbowRightX = 7 * tempVal * (180 / Math.PI) - 10;

            tempVal = Math.Cos(skeleton.Joints[JointType.WristLeft].Position.X - skeleton.Joints[JointType.ShoulderLeft].Position.X);
            tempVal = Math.Acos(tempVal);
            ElbowLeftX = 7 * tempVal * (180 / Math.PI) - 10;

            tempVal = Math.Cos(skeleton.Joints[JointType.WristRight].Position.Y - skeleton.Joints[JointType.ShoulderRight].Position.Y);
            tempVal = Math.Acos(tempVal);
            ElbowRightY = 7 * tempVal * (180 / Math.PI) - 10;

            tempVal = Math.Cos(skeleton.Joints[JointType.WristLeft].Position.Y - skeleton.Joints[JointType.ShoulderLeft].Position.Y);
            tempVal = Math.Acos(tempVal);
            ElbowLeftY = 7 * tempVal * (180 / Math.PI) - 10;

            ///  Dizkapaklarının Left-Right ve eksenlere göre açıları hesaplanması. 
            tempVal = Math.Cos(skeleton.Joints[JointType.AnkleRight].Position.X - skeleton.Joints[JointType.HipRight].Position.X);
            tempVal = Math.Acos(tempVal);
            tempVal = 7 * tempVal * (180 / Math.PI);
            KneeRightX = tempVal / 2;

            tempVal = Math.Cos(skeleton.Joints[JointType.AnkleLeft].Position.X - skeleton.Joints[JointType.HipLeft].Position.X);
            tempVal = Math.Acos(tempVal);
            tempVal = 7 * tempVal * (180 / Math.PI);
            KneeLeftX = tempVal / 2;

            tempVal = Math.Cos(skeleton.Joints[JointType.AnkleRight].Position.Y - skeleton.Joints[JointType.HipRight].Position.Y);
            tempVal = Math.Acos(tempVal);
            tempVal = 7 * tempVal * (180 / Math.PI);
            KneeRightY = tempVal / 2;

            tempVal = Math.Cos(skeleton.Joints[JointType.AnkleLeft].Position.Y - skeleton.Joints[JointType.HipLeft].Position.Y);
            tempVal = Math.Acos(tempVal);
            tempVal = 7 * tempVal * (180 / Math.PI);
            KneeLeftY = tempVal / 2;

            ///Diz kapakları arasındaki mesafe
            ///
            tempVal = Math.Cos(skeleton.Joints[JointType.KneeLeft].Position.X - skeleton.Joints[JointType.KneeRight].Position.X);
            tempVal = Math.Acos(tempVal);
            KneeDistance = 7.3 * tempVal * (180 / Math.PI);

            lb1.Content = "BOYUN SAĞ-X : " + NeckAngleRightX;
            lb2.Content = "BOYUN SAĞ-Y : " + NeckAngleRightY;
            lb3.Content = "BOYUN SOL-X : " + NeckAngleLeftX;
            lb4.Content = "BOYUN SOL-Y : " + NeckAngleLeftY;
            lb5.Content = "DİRSEK SAĞ-X : " + ElbowRightX;
            lb6.Content = "DİRSEK SAĞ-Y : " + ElbowRightY;
            lb7.Content = "DİRSEK SOL-X : " + ElbowLeftX;
            lb8.Content = "DİRSEK SOL-Y : " + ElbowLeftY;
            lb9.Content = "DİZ SAĞ-X : " + KneeRightX;
            lb10.Content = "DİZ SAĞ-Y : " + KneeRightY;
            lb11.Content = "DİZ SOL-X : " + KneeLeftX;
            lb12.Content = "DİZ SOL-Y : " + KneeLeftY;
            lb13.Content = "DİZ MESAFESİ : " + KneeDistance;


            /* SOL KOL AÇI
            double deger = Math.Cos(skeleton.Joints[JointType.WristLeft].Position.X - skeleton.Joints[JointType.ShoulderLeft].Position.X);
            double veri = Math.Acos(deger);
            double yeni = 7.3 * veri * (180 / Math.PI);
            aci.Content = yeni.ToString();
                     */

            /* İKİ DİZ KAPAĞI ARASI AÇI
            double deger = Math.Cos(skeleton.Joints[JointType.KneeLeft].Position.X - skeleton.Joints[JointType.KneeRight].Position.X);
            double veri = Math.Acos(deger);
            double yeni = 7.3 * veri * (180 / Math.PI);
            aci.Content = yeni.ToString();
                 */




            /* SAĞ AYAK BİLEĞİ - KALÇA ARASI AÇI
           double deger = Math.Cos(skeleton.Joints[JointType.AnkleRight].Position.Y - skeleton.Joints[JointType.HipRight].Position.Y);
           double veri = Math.Acos(deger);
           double yeni = 7 * veri * (180 / Math.PI);
           yeni = yeni / 2;
           aci.Content = yeni.ToString();
           */

            /* BAŞ VE SAĞ OMUZ ARASI AÇI
            double deger = Math.Cos(skeleton.Joints[JointType.ShoulderCenter].Position.X - skeleton.Joints[JointType.ShoulderRight].Position.X);
            double veri = Math.Acos(deger);
            double yeni = 7 * veri * (180 / Math.PI)-10;
            
            aci.Content = yeni.ToString();
            */

            /* BAŞ VE SOL OMUZ ARASI AÇI
            double deger = Math.Cos(skeleton.Joints[JointType.ShoulderCenter].Position.X - skeleton.Joints[JointType.ShoulderLeft].Position.X);
            double veri = Math.Acos(deger);
            double yeni = 7 * veri * (180 / Math.PI)-10;
                        aci.Content = yeni.ToString();
            */

            /* BAŞ İLE SAĞ OMUZ YANA HAREKET
            double deger = Math.Cos(skeleton.Joints[JointType.Head].Position.Y - skeleton.Joints[JointType.ShoulderRight].Position.Y);
            double veri = Math.Acos(deger);
            double yeni = 7 * veri * (180 / Math.PI);
            aci.Content = yeni.ToString();
            */

            /* BAŞ İLE SOL OMUZ YANA HAREKET
            double deger = Math.Cos(skeleton.Joints[JointType.Head].Position.Y - skeleton.Joints[JointType.ShoulderLeft].Position.Y);
            double veri = Math.Acos(deger);
            double yeni = 7 * veri * (180 / Math.PI);
            aci.Content = yeni.ToString();
            */













            // Render Torso
            this.DrawBone(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.Spine);
            this.DrawBone(skeleton, drawingContext, JointType.Spine, JointType.HipCenter);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipLeft);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipRight);

            // Left Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowLeft, JointType.WristLeft);
            this.DrawBone(skeleton, drawingContext, JointType.WristLeft, JointType.HandLeft);

            // Right Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight);
            this.DrawBone(skeleton, drawingContext, JointType.WristRight, JointType.HandRight);

            // Left Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipLeft, JointType.KneeLeft);
            this.DrawBone(skeleton, drawingContext, JointType.KneeLeft, JointType.AnkleLeft);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleLeft, JointType.FootLeft);

            // Right Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipRight, JointType.KneeRight);
            this.DrawBone(skeleton, drawingContext, JointType.KneeRight, JointType.AnkleRight);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleRight, JointType.FootRight);

            // Render Joints
            foreach (Joint joint in skeleton.Joints)
            {
                Brush drawBrush = null;

                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;
                }
                else if (joint.TrackingState == JointTrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position), JointThickness, JointThickness);
                }
            }

        }

        /// <summary>
        /// Maps a SkeletonPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }

        /// <summary>
        /// Draws a bone line between two joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw bones from</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="jointType0">joint to start drawing from</param>
        /// <param name="jointType1">joint to end drawing at</param>
        private void DrawBone(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1)
        {
            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == JointTrackingState.NotTracked ||
                joint1.TrackingState == JointTrackingState.NotTracked)
            {
                return;
            }

            // Don't draw if both points are inferred
            if (joint0.TrackingState == JointTrackingState.Inferred &&
                joint1.TrackingState == JointTrackingState.Inferred)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
            {
                drawPen = this.trackedBonePen;
            }

            drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position), this.SkeletonPointToScreen(joint1.Position));
        }

        /// <summary>
        /// Handles the checking or unchecking of the seated mode combo box
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void CheckBoxSeatedModeChanged(object sender, RoutedEventArgs e)
        {
            if (null != this.sensor)
            {
                if (this.checkBoxSeatedMode.IsChecked.GetValueOrDefault())
                {
                    this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;

                }
                else
                {
                    this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
                }
            }
        }

        private static bool hareket2(JointCollection joints)
        {

            return
            joints[JointType.HandRight].Position.X > joints[JointType.Head].Position.X

            && degerlerYakin(0.1f, joints[JointType.HandRight].Position.X, joints[JointType.ElbowRight].Position.X)

            && joints[JointType.ElbowRight].Position.X > joints[JointType.ShoulderRight].Position.X

            && joints[JointType.ElbowRight].Position.X > joints[JointType.Head].Position.X

            && joints[JointType.HandRight].Position.Y > joints[JointType.ElbowRight].Position.Y

            && joints[JointType.ElbowRight].Position.Y > joints[JointType.ShoulderRight].Position.Y

            && joints[JointType.Head].Position.Y > joints[JointType.ElbowRight].Position.Y



            && joints[JointType.HandLeft].Position.X < joints[JointType.Head].Position.X

            && degerlerYakin(0.1f, joints[JointType.HandLeft].Position.X, joints[JointType.ElbowLeft].Position.X)

            && joints[JointType.ElbowLeft].Position.X < joints[JointType.ShoulderLeft].Position.X

            && joints[JointType.ElbowLeft].Position.X < joints[JointType.Head].Position.X

            && joints[JointType.HandLeft].Position.Y > joints[JointType.ElbowLeft].Position.Y

            && joints[JointType.ElbowLeft].Position.Y > joints[JointType.ShoulderLeft].Position.Y

            && joints[JointType.Head].Position.Y > joints[JointType.ElbowRight].Position.Y;

        }

        private void islemler(JointCollection joints)
        {

            if (hareket2(joints))
            {



            }
            else
            {
                label3.Content = "FAİLED";
                button1.Background = Brushes.Red;

            }


        }







        private static bool ikiDegerYakin(float a1, float a2, float factor)
        {
            return System.Math.Abs(a1 - a2) <= factor;
        }

        private static bool degerlerYakin(float factor, params float[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                for (int j = 0; j < array.Length; j++)
                {
                    if (!ikiDegerYakin(array[i], array[j], factor))
                        return false;
                }
            }
            return true;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            lb1.Content = "BOYUN SAĞ-X : " + NeckAngleRightX;
            lb2.Content = "BOYUN SAĞ-Y : " + NeckAngleRightY;
            lb3.Content = "BOYUN SOL-X : " + NeckAngleLeftX;
            lb4.Content = "BOYUN SOL-Y : " + NeckAngleLeftY;
            lb5.Content = "DİRSEK SAĞ-X : " + ElbowRightX;
            lb6.Content = "DİRSEK SAĞ-Y : " + ElbowRightY;
            lb7.Content = "DİRSEK SOL-X : " + ElbowLeftX;
            lb8.Content = "DİRSEK SOL-Y : " + ElbowLeftY;
            lb9.Content = "DİZ SAĞ-X : " + KneeRightX;
            lb10.Content = "DİZ SAĞ-Y : " + KneeRightY;
            lb11.Content = "DİZ SOL-X : " + KneeLeftX;
            lb12.Content = "DİZ SOL-Y : " + KneeLeftY;
            lb13.Content = "DİZ MESAFESİ : " + KneeDistance;

            //Deneme
            ElboxRightXD = ElbowRightX;
            ElbowLeftxD = ElbowLeftX;
            KneeRightXD = KneeRightX;
            KneeLeftXD = KneeLeftX;

        }

        private void bt2_Click(object sender, RoutedEventArgs e)
        {
            if (ElboxRightXD - ElbowRightX < 20 && ElboxRightXD - ElbowRightX > -20 &&
                ElbowLeftxD - ElbowLeftX < 20 && ElbowLeftxD - ElbowLeftX > -20 &&
                KneeRightXD - KneeRightX < 20 && KneeRightXD - KneeRightX > -20 &&
                KneeLeftXD - KneeLeftX < 20 && KneeLeftXD - KneeLeftX > -20)
            {

                label3.Content = "OK";
                button1.Background = Brushes.Green;
            }
            else
            {
                label3.Content = "FAİLED";
                button1.Background = Brushes.Red;

            }
        }

        public int deger = 0;

        public void namaz()
        {
            Db database = new Db();
            if (deger == 0)
            {

            }


            if (ElboxRightXD - ElbowRightX < 20 && ElboxRightXD - ElbowRightX > -20 &&
                ElbowLeftxD - ElbowLeftX < 20 && ElbowLeftxD - ElbowLeftX > -20 &&
                KneeRightXD - KneeRightX < 20 && KneeRightXD - KneeRightX > -20 &&
                KneeLeftXD - KneeLeftX < 20 && KneeLeftXD - KneeLeftX > -20)
            {

                deger++; //  burada ileriki aşamalar için goto kullanıp 2.hareketin verisi çağırılabilir
            }
            else
            {
                if (deger > 0)
                {
                    deger = 0;
                    count++;
                    label3.Content = count;
                }
            }










        }


        static int stepCount = 0;
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            Db d = new Db();
            String moveInfo = infTextBox.Text;
            String moveName = mNameTextBox.Text;

            if (stepCount == 0)
            {
                d.FirstStep(moveName, moveInfo, ElbowRightX, ElbowRightY, ElbowLeftX, ElbowLeftY, KneeRightX, KneeRightY, KneeLeftX, KneeLeftY, NeckAngleRightX, NeckAngleRightY, NeckAngleLeftX, NeckAngleLeftY, KneeDistance);
                stepCount++;
                
               
            }
            else if (stepCount > 0)
            {
                d.OtherSteps(ElbowRightX, ElbowRightY, ElbowLeftX, ElbowLeftY, KneeRightX, KneeRightY, KneeLeftX, KneeLeftY, NeckAngleRightX, NeckAngleRightY, NeckAngleLeftX, NeckAngleLeftY, KneeDistance);
                stepCount++;
               
            }








        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            AddImage img = new AddImage();
            img.Show();
        }
    }






}