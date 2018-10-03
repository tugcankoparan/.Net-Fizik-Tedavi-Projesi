
namespace HastaGirisi
{
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using Microsoft.Kinect;
    using System;
    using System.Data.SqlClient;
    using System.Collections;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static bool ControlValue = false;

        
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

        static String ConString = "Data Source=DESKTOP-1U8JM22\\EXPRESSSERVER;Initial Catalog=ProjeDeneme;Integrated Security=True";
        static SqlConnection conn;
        static SqlCommand sqlCmd;
        static SqlDataReader sqlReader;
        static String query;

        public static bool AdimKontrol;
        public static bool HareketKontrol;

        public int deger = 0;
        public static int ArrayCount = 0;
        public static bool ControlLock = true;
        public static int MoveCount = 0;
        public static String StepStart = "";
        public static String StepStartTemp = "";

        public static double NeckAngleRightXT;
        public static double NeckAngleRightYT;
        public static double NeckAngleLeftXT;
        public static double NeckAngleLeftYT;

        public static double ElbowRightXT;
        public static double ElbowRightYT;
        public static double ElbowLeftXT;
        public static double ElbowLeftYT;

        public static double KneeRightXT;
        public static double KneeRightYT;
        public static double KneeLeftXT;
        public static double KneeLeftYT;

        public static double KneeDistanceT;
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

            if (ControlValue)
            {
                ConStart();
                
            }
         



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

        
        public static ArrayList Sonuc = new ArrayList(); //Sonuçların tutulduğu arrayList
        public String LabelContent = ""; // ArrayList ve Label'a yazmak için sonuç değişkeni
        public static int moveCounter = 0; // Hareketin kaç kere yapıldığının kontrolü için tutulan değilken
        public static int msCount = 0; 
        public static bool stepControl=true;
        public static String NextStepCount = "";
        public String HareketAdi;
      
       
        public void ConStart()
        {
            
            
            if (ControlLock)
            {
                HareketAdi = HareketListesi[ArrayCount].ToString();
                label1.Content = HareketAdi;

                ArrayCount++;
                label.Content = HareketListesi[ArrayCount];

                ArrayCount++;
                //Hareket Adım Sayısı kullanılacaksa bu satırdan alınacak

                ArrayCount++;
                StepStart = HareketListesi[ArrayCount].ToString();
                StepStartTemp = StepStart;

                ArrayCount++;
                MoveCount = Convert.ToInt32(HareketListesi[ArrayCount].ToString());
                ArrayCount++;
                //Image Getirme

               

                ControlLock = false;
            }


            if (stepControl)
            {

                query = "SELECT * FROM Adimlar where adimID = '" + StepStart + "'";
                sqlCmd = new SqlCommand(query, conn);
                sqlReader = sqlCmd.ExecuteReader();
                if (sqlReader.Read())
                {
                    NextStepCount = sqlReader[1].ToString();
                    ElbowRightXT = Convert.ToDouble(sqlReader[2].ToString());
                    ElbowRightYT = Convert.ToDouble(sqlReader[3].ToString());
                    ElbowLeftXT = Convert.ToDouble(sqlReader[4].ToString());
                    ElbowLeftYT = Convert.ToDouble(sqlReader[5].ToString());

                    KneeRightXT = Convert.ToDouble(sqlReader[6].ToString());
                    KneeRightYT = Convert.ToDouble(sqlReader[7].ToString());
                    KneeLeftXT = Convert.ToDouble(sqlReader[8].ToString());
                    KneeLeftYT = Convert.ToDouble(sqlReader[9].ToString());

                    NeckAngleRightXT = Convert.ToDouble(sqlReader[10].ToString());
                    NeckAngleRightYT = Convert.ToDouble(sqlReader[11].ToString());
                    NeckAngleLeftXT = Convert.ToDouble(sqlReader[12].ToString());
                    NeckAngleLeftYT = Convert.ToDouble(sqlReader[13].ToString());

                    KneeDistanceT = Convert.ToDouble(sqlReader[14].ToString());

                    stepControl = false;

                }
                sqlReader.Close();
            }

           



            if (ElbowRightXT - ElbowRightX < 20 && ElbowRightXT - ElbowRightX > -20 &&
                ElbowRightYT - ElbowRightY < 20 && ElbowRightYT - ElbowRightY > -20 &&
                ElbowLeftXT - ElbowLeftX < 20 && ElbowLeftXT - ElbowLeftX > -20 &&
                ElbowLeftYT - ElbowLeftY < 20 && ElbowLeftYT - ElbowLeftY > -20 &&
                NeckAngleRightXT - NeckAngleRightX < 20 && NeckAngleRightXT - NeckAngleRightX > -20 &&
                NeckAngleRightYT - NeckAngleRightY < 20 && NeckAngleRightYT - NeckAngleRightY > -20 &&
                NeckAngleLeftXT - NeckAngleLeftX < 20 && NeckAngleLeftXT - NeckAngleLeftX > -20 &&
                NeckAngleLeftYT - NeckAngleLeftY < 20 && NeckAngleLeftYT - NeckAngleLeftY > -20 &&
                KneeRightXT - KneeRightX < 20 && KneeRightXT - KneeRightX > -20 &&
                KneeRightYT - KneeRightY < 20 && KneeRightYT - KneeRightY > -20 &&
                KneeLeftXT - KneeLeftX < 20 && KneeLeftXT - KneeLeftX > -20 &&
                KneeLeftYT - KneeLeftY < 20 && KneeLeftYT - KneeLeftY > -20 
                )
                {
                    button1.Background = Brushes.Green;
                    msCount++;
                    if (msCount>3) // Adımı Tamamlamışsa
                    {
                        if (!NextStepCount.Equals("")) //Sonraki adım varsa
                        {

                            StepStart = NextStepCount; //Bir sonraki adımın keyi alıp veri çekme bloğunun kilidi açılır.
                            stepControl = true;
                            
                        }
                        else
                        {
                            moveCounter++;

                            if (MoveCount==moveCounter) // 1 hareket tamamlanmışsa
                            {
                                
                                moveCounter = 0;
                                ControlLock = true;
                                stepControl = true;

                            }
                            else // Hareketin sayısını tamamlamamışsa
                            {

                                LabelContent = HareketAdi + " Hareketi " + moveCounter.ToString() + " Defa Yapıldı.";

                                Sonuc.Add(LabelContent);
                                label2.Content = LabelContent;
                                StepStart = StepStartTemp;
                                stepControl = true;

                            }  


                        } //Hareket Bitmişse

                        msCount = 0;


                    }
                
            }
            else
            {
                button1.Background = Brushes.Red;
            }

            

        }



        public static String HareketID;
        public static int HareketAdet;
        public static ArrayList HareketListesi = new ArrayList();
        
        private void button2_Click(object sender, RoutedEventArgs e)
        {


            
            
            //Başlat button
            EgzersizSec ES = new EgzersizSec();
            query = "SELECT H.hAdi , H.hInfo , H.hAdimSay , H.AdimIDStart , T.HareketAdet  FROM Hareketler H , Tedavi T where T.KayitID='" + ES.getKayitID()+"' AND H.ID_column=T.HareketID";
            conn = new SqlConnection(ConString);
            conn.Open();
            sqlCmd = new SqlCommand(query, conn);
            sqlReader = sqlCmd.ExecuteReader();
            while (sqlReader.Read())
            {
                HareketListesi.Add(sqlReader[0].ToString()); // HareketAdı 0 - 6 - 12
                HareketListesi.Add(sqlReader[1].ToString()); // Hareket Bilgisi 1 -7 - 13
                HareketListesi.Add(sqlReader[2].ToString()); // HareketAdımSayısı 2 - 8 - 14
                HareketListesi.Add(sqlReader[3].ToString()); // Db adım başlangıç 3 - 9 - 15
                HareketListesi.Add(sqlReader[4].ToString()); // Kaç kere yapılacak 4 - 10 -16
            }
            sqlReader.Close();
            ControlValue = true;
            


        }
        



        private void bt2_Click(object sender, RoutedEventArgs e)
        {
            query = "Select hImage from Hareketler where ID_Column = 6033 ";
            conn = new SqlConnection(ConString);
            conn.Open();
            sqlCmd = new SqlCommand(query, conn);
            sqlReader = sqlCmd.ExecuteReader();
            if (sqlReader.Read())
            {
                byte[] imgD = ((byte[])sqlReader["hImage"]);
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream(imgD);
                bitmapImage.EndInit();
                image.Source = bitmapImage;


            }


        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            //Hareketi Atla button
            msCount = 0;
            ControlLock = true;
            stepControl = true;
            
        }
    }






}