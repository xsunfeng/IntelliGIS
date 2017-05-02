using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Microsoft.Kinect;
using Microsoft.Speech.Recognition;
using Microsoft.Speech.Synthesis;
using Kinect.Toolbox;

namespace CAGA.NUI
{
    public class KinectManager
    {
        // current kinect sensor
        KinectSensor kinectSensor = null;
        
        public event Action<string> KinectStatusChanged;
        public event Action<SortedList> SpeechRecognized;
        public event Action<SortedList> GestureRecognzied;

        // list of managers for display kinect information, provided by Kinect.Toolbox
        ColorStreamManager colorManager = null;
        DepthStreamManager depthManager = null;
        SkeletonDisplayManager skeletonDisplayManager = null;

        // Speech recognizer
        public SpeechRecognizer speechRecognizer = null;
        
        
        // Gesture recognizer
        GestureRecognizer gestureRecognizer = null;

        public KinectManager()
        {
            try
            {
                //loop through all the Kinects attached to this PC, and start the first that is connected without an error.
                foreach (KinectSensor kinect in KinectSensor.KinectSensors)
                {
                    if (kinect.Status == KinectStatus.Connected)
                    {
                        kinectSensor = kinect;
                        break;
                    }
                }
                //listen to any status change for Kinects
                KinectSensor.KinectSensors.StatusChanged += Kinects_StatusChanged;
            }
            catch {
            }
        }

        
        public bool LoadKinectSensor()
        {
            if (kinectSensor == null)
                return false;

            kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            kinectSensor.ColorFrameReady += kinectRuntime_ColorFrameReady;

            kinectSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            kinectSensor.DepthFrameReady += kinectSensor_DepthFrameReady;

            kinectSensor.SkeletonStream.Enable(new TransformSmoothParameters
            {
                Smoothing = 0.5f,
                Correction = 0.5f,
                Prediction = 0.5f,
                JitterRadius = 0.05f,
                MaxDeviationRadius = 0.04f
            });
            kinectSensor.SkeletonFrameReady += kinectRuntime_SkeletonFrameReady;
            kinectSensor.Start();
            
            return true;
        }

        public void LoadSpeechRecognizer(string grammarFile)
        {
            speechRecognizer = new SpeechRecognizer(grammarFile, kinectSensor);
            speechRecognizer.SpeechRecognized += Speech_Recognized;
            
        }

        public void LoadGestureRecognizer()
        {
            gestureRecognizer = new GestureRecognizer();
            gestureRecognizer.GestureRecognzied += Gesture_Recognzied;
        }

        public void LoadColorManager(Image colorDisplay)
        {
            colorManager = new ColorStreamManager();
            colorDisplay.DataContext = colorManager;
        }

        public void LoadDepthManager(Image depthDisplay)
        {
            depthManager = new DepthStreamManager();
            depthDisplay.DataContext = depthManager;
        }

        public void LoadSkeletonDisplayManager(Canvas skeletonCanvas)
        {
            skeletonDisplayManager = new SkeletonDisplayManager(kinectSensor, skeletonCanvas);
        }

        public void StartSpeechRecognition()
        {
            if (speechRecognizer != null)
            {
                speechRecognizer.Start();
            }
        }

        public void StopSpeechRecognition()
        {
            if (speechRecognizer != null)
            {
                speechRecognizer.Stop();
            }
        }

        public void StartGestureRecognition()
        {
            if (gestureRecognizer != null)
            {
                gestureRecognizer.Start();
            }
        }

        public void StopGestureRecognition()
        {
            if (gestureRecognizer != null)
            {
                gestureRecognizer.Stop();
            }
        }

        public void Dispose()
        {
            if (skeletonDisplayManager != null)
            {
                skeletonDisplayManager = null;
            }
            if (colorManager != null)
            {
                colorManager = null;
            }
            if (depthManager != null)
            {
                depthManager = null;
            }

            if (speechRecognizer != null)
            {
                speechRecognizer.SpeechRecognized -= Speech_Recognized;
                speechRecognizer.Dispose();
                speechRecognizer = null;
            }
            
            if (gestureRecognizer != null)
            {
                gestureRecognizer.GestureRecognzied -= Gesture_Recognzied;
                gestureRecognizer.Dispose();
                gestureRecognizer = null;
            }

            if (kinectSensor != null)
            {
                kinectSensor.ColorFrameReady -= kinectRuntime_ColorFrameReady;
                kinectSensor.SkeletonFrameReady -= kinectRuntime_SkeletonFrameReady;
                kinectSensor.ColorFrameReady -= kinectRuntime_ColorFrameReady;
                kinectSensor.Stop();
                kinectSensor = null;
            }
        }



        void Kinects_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case KinectStatus.Connected:
                    if (kinectSensor == null)
                    {
                        kinectSensor = e.Sensor;

                        LoadKinectSensor();
                    }
                    break;
                case KinectStatus.Disconnected:
                case KinectStatus.NotReady:
                case KinectStatus.NotPowered:
                    if (kinectSensor == e.Sensor)
                    {
                        Dispose();
                    }
                    break;
                default:
                    break;
            }
            KinectStatusChanged(e.Status.ToString());
        }

        void kinectRuntime_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (var frame = e.OpenColorImageFrame())
            {
                if (frame == null)
                    return;

                ProcessColorFrame(frame);
            }
        }

        void ProcessColorFrame(ColorImageFrame frame)
        {
            if (colorManager != null)
            {
                colorManager.Update(frame);
            }
        }

        void kinectSensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {

            using (var frame = e.OpenDepthImageFrame())
            {
                if (frame == null)
                    return;

                ProcessDepthFrame(frame);
            }
        }

        void ProcessDepthFrame(DepthImageFrame frame)
        {
            if (depthManager != null)
            {
                depthManager.Update(frame);
            }
        }


        void kinectRuntime_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame == null)
                    return;
                ProcessSkeletonFrame(frame);
            }
        }

        void ProcessSkeletonFrame(SkeletonFrame frame)
        {
            if (gestureRecognizer != null)
            {
                gestureRecognizer.Update(frame);
            }

            if (skeletonDisplayManager != null)
            {
                skeletonDisplayManager.Draw(frame); 
            }
        }

        void Speech_Recognized(SortedList result)
        {
            if (SpeechRecognized != null)
            {
                SpeechRecognized(result);
            }
        }

        void Gesture_Recognzied(SortedList result)
        {
            if (GestureRecognzied != null)
            {
                GestureRecognzied(result);
            }
        }
    }
}
