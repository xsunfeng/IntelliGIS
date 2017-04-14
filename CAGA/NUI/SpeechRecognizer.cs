using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections;
using Microsoft.Kinect;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;


namespace  CAGA.NUI
{
    public class SpeechRecognizer : IDisposable
    {
        //Thread workingThread;
        public bool isRunning;
        public SpeechRecognitionEngine speechRecognitionEngine;
        private KinectSensor kinectSensor;
        KinectAudioSource audioSource;
        Stream audioStream;
        string simText;

        string grammarFile;

        public event Action<SortedList> SpeechRecognized;
        public event Action<AudioState> AudioStateChanged;
        
        public SpeechRecognizer(string file, KinectSensor sensor)
        {
            this.grammarFile = file;
            this.kinectSensor = sensor;
            var grammar = new Grammar(grammarFile);

            if (kinectSensor == null)
            {
                // Create an in-process speech recognizer for the en-US locale.
                speechRecognitionEngine = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("en-US"));

                // Create and load a dictation grammar.
                speechRecognitionEngine.LoadGrammar(grammar);

                // Configure input to the speech recognizer.
                speechRecognitionEngine.SetInputToDefaultAudioDevice();
            }
            else // there is a kinect detected
            {
                audioSource = kinectSensor.AudioSource;
                audioSource.AutomaticGainControlEnabled = false;
                audioSource.BeamAngleMode = BeamAngleMode.Adaptive;

                Func<RecognizerInfo, bool> matchingFunc = r =>
                {
                    string value;
                    r.AdditionalInfo.TryGetValue("Kinect", out value);
                    return "True".Equals(value, StringComparison.InvariantCultureIgnoreCase) && "en-US".Equals(r.Culture.Name, StringComparison.InvariantCultureIgnoreCase);
                };
                var recognizerInfo = SpeechRecognitionEngine.InstalledRecognizers().Where(matchingFunc).FirstOrDefault();
                if (recognizerInfo == null)
                    return;

                speechRecognitionEngine = new SpeechRecognitionEngine(recognizerInfo.Id);

                speechRecognitionEngine.LoadGrammar(grammar);

                audioStream = audioSource.Start();
                speechRecognitionEngine.SetInputToAudioStream(audioStream, new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
            }
            speechRecognitionEngine.AudioStateChanged += onAudioStateChanged;
            speechRecognitionEngine.SpeechRecognized += onSpeechRecognized;
            speechRecognitionEngine.RecognizeCompleted += onSpeechRecognizeCompleted;
            speechRecognitionEngine.EmulateRecognizeCompleted += onEmulateRecognizeCompleted;
        }

        public void Start()
        {
            isRunning = true;
            speechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);
            simText = "";
        }

        

        public void Simulate(string speechText)
        {
            if (this.speechRecognitionEngine != null)
            {
                this.speechRecognitionEngine.RecognizeAsyncStop();
                this.simText = speechText;
            }
        }

        private SortedList ParseResult(SemanticValue value)
        {
            System.Collections.SortedList htResult = new SortedList();
           
            foreach (var item in value)
            {
                
                if (item.Value.Count > 0)
                {
                    htResult.Add(item.Key, ParseResult(item.Value));
                }
                else
                {
                    htResult.Add(item.Key, item.Value.Value.ToString());
                }
            }
            return htResult;
        }
 
        private void onAudioStateChanged(object sender, AudioStateChangedEventArgs e)
        {
            if (isRunning == false)
            {
                return;
            }
            if (AudioStateChanged != null)
            {
                AudioStateChanged(e.AudioState);
            }
            
        }

        private void onSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (isRunning == false)
            {
                return;
            }
            if (e.Result != null && e.Result.Confidence > 0.7)
            {
                System.Collections.SortedList htResult = ParseResult(e.Result.Semantics);
                if (SpeechRecognized != null)
                {
                    SpeechRecognized(htResult);
                }
                
            }
        }

        private void onSpeechRecognizeCompleted(object sender, RecognizeCompletedEventArgs e)
        {
            if (this.simText.Length > 0)
            {
                this.speechRecognitionEngine.EmulateRecognizeAsync(this.simText);
            }
        }

        private void onEmulateRecognizeCompleted(object sender, EmulateRecognizeCompletedEventArgs e)
        {
            this.simText = "";
            this.speechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);
        }

        public void Stop()
        {
            isRunning = false;
            this.speechRecognitionEngine.RecognizeAsyncCancel();
            simText = "";
        }

        public void Dispose()
        {
            Stop();
            
            audioStream.Close();
            audioSource.Stop();
            
        }
    }
}
