using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using Kinect.Toolbox;

namespace CAGA.NUI
{
    public class GestureRecognizer : IDisposable
    {
        public event Action<SortedList> GestureRecognzied;

        bool isRunning;
        SwipeGestureDetector swipeGestureRecognizer = null;
        TemplatedGestureDetector circleGestureRecognizer = null;
        string circleKBPath;

        public GestureRecognizer()
        {
            /*
            swipeGestureRecognizer = new SwipeGestureDetector();
            swipeGestureRecognizer.OnGestureDetected += OnSwipeGestureDetected;

            using (Stream recordStream = File.Open(circleKBPath, FileMode.OpenOrCreate))
            {
                circleGestureRecognizer = new TemplatedGestureDetector("Circle", recordStream);
                circleGestureRecognizer.TraceTo(gestureCanvas, Colors.Red);
                circleGestureRecognizer.OnGestureDetected += OnCircleGestureDetected;
            }
            */
        }

        public void Start()
        {
            isRunning = true;
        }

        public void Stop()
        {
            isRunning = false;
        }

        public void Update(SkeletonFrame frame)
        {
            if (isRunning == false)
            {
                return;
            }

            Skeleton [] skeletons = Tools.GetSkeletons(frame);

            foreach (var skeleton in skeletons)
            {
                if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                    continue;
                // just the 1st skeleton at the moment
                
                //TrackGestures(skeleton);

                

            }

        }

        public void Dispose()
        { 
        }
    }
}
