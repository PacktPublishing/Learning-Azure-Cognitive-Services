using End_to_End.Interface;
using End_to_End.Model;
using Microsoft.ProjectOxford.Common.Contract;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Microsoft.ProjectOxford.SpeakerRecognition;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using VideoFrameAnalyzer;

namespace End_to_End.ViewModel
{
    public class HomeViewModel : ObservableObject
    {
        private FaceServiceClient _faceServiceClient;
        private SpeakerIdentification _speakerIdentification;

        private Recording _recording;

        private FrameGrabber<CameraResult> _frameGrabber;
        private static readonly ImageEncodingParam[] s_jpegParams = {
            new ImageEncodingParam(ImwriteFlags.JpegQuality, 60)
        };

        #region Properties

        private ObservableCollection<PersonGroup> _personGroups = new ObservableCollection<PersonGroup>();
        public ObservableCollection<PersonGroup> PersonGroups
        {
            get { return _personGroups; }
            set
            {
                _personGroups = value;
                RaisePropertyChangedEvent("PersonGroups");
            }
        }

        private PersonGroup _selectedPersonGroup;        
        public PersonGroup SelectedPersonGroup
        {
            get { return _selectedPersonGroup; }
            set
            {
                _selectedPersonGroup = value;
                RaisePropertyChangedEvent("PersonGroups");
            }
        }
        
        private BitmapImage _imageSource;
        public BitmapImage ImageSource
        {
            get { return _imageSource; }
            set
            {
                _imageSource = value;
                RaisePropertyChangedEvent("ImageSource");
            }
        }
        
        private string _systemResponse;
        public string SystemResponse
        {
            get { return _systemResponse; }
            set
            {
                _systemResponse = value;
                RaisePropertyChangedEvent("SystemResponse");
            }
        }

        #endregion Properties

        #region ICommand properties

        public ICommand StopCameraCommand { get; private set; }
        public ICommand StartCameraCommand { get; private set; }
        public ICommand UploadOwnerImageCommand { get; private set; }
        public ICommand StartSpeakerRecording { get; private set; }
        public ICommand StopSpeakerRecording { get; private set; }

        #endregion ICommand properties

        /// <summary>
        /// HomeViewModel constructor. Assigsn API service clients, creates a <see cref="FrameGrabber{AnalysisResultType}"/> and <see cref="Recording"/> objects
        /// </summary>
        /// <param name="faceServiceClient"><see cref="FaceServiceClient"/> object</param>
        /// <param name="emotionServiceClient"><see cref="EmotionServiceClient"/> object</param>
        /// <param name="speakerIdentification"><see cref="ISpeakerIdentificationServiceClient"/> object</param>
        public HomeViewModel(FaceServiceClient faceServiceClient, ISpeakerIdentificationServiceClient speakerIdentification)
        {
            _faceServiceClient = faceServiceClient;
            _speakerIdentification = new SpeakerIdentification(speakerIdentification);

            _frameGrabber = new FrameGrabber<CameraResult>();
            _recording = new Recording();

            Initialize();
        }

        /// <summary>
        /// Function to initizlie the view model
        /// Will get person groups, create command properties and subscribe to given events
        /// </summary>
        private void Initialize()
        {
            GetPersonGroups();
            _frameGrabber.NewFrameProvided += OnNewFrameProvided;
            _frameGrabber.NewResultAvailable += OnResultAvailable;
            _frameGrabber.AnalysisFunction = EmotionAnalysisAsync;

            StopCameraCommand = new DelegateCommand(StopCamera);
            StartCameraCommand = new DelegateCommand(StartCamera, CanStartCamera);
            UploadOwnerImageCommand = new DelegateCommand(UploadOwnerImage, CanUploadOwnerImage);

            _speakerIdentification.OnSpeakerIdentificationError += OnSpeakerIdentificationError;
            _speakerIdentification.OnSpeakerIdentificationStatusUpdated += OnSpeakerIdentificationStatusReceived;

            _recording.OnAudioStreamAvailable += OnSpeakerRecordingAvailable;
            _recording.OnRecordingError += OnSpeakerRecordingError;

            StartSpeakerRecording = new DelegateCommand(StartSpeaker);
            StopSpeakerRecording = new DelegateCommand(StopSpeaker);
        }

        /// <summary>
        /// Command fucntion to stop a microphone recording
        /// </summary>
        /// <param name="obj"></param>
        private void StopSpeaker(object obj)
        {
            _recording.StopRecording();
        }

        /// <summary>
        /// Command function to start a microphone recording
        /// </summary>
        /// <param name="obj"></param>
        private void StartSpeaker(object obj)
        {
            _recording.StartRecording();
        }

        /// <summary>
        /// Event handler function for recording available events. 
        /// Will take the selected speaker profiles and use the recording to identify a speaker
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnSpeakerRecordingAvailable(object sender, RecordingAudioAvailableEventArgs e)
        {
            try
            {
                List<Guid> profiles = await _speakerIdentification.ListSpeakerProfiles();
                _speakerIdentification.IdentifySpeaker(e.AudioStream, profiles.ToArray());
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Event handler function for speaker recording errors.
        /// Displays the errors in the debug console
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSpeakerRecordingError(object sender, RecordingErrorEventArgs e)
        {
            Debug.WriteLine(e.ErrorMessage);
        }

        /// <summary>
        /// Event handler function for speaker identification status updates
        /// Displays the identified profile ID in the UI, if it exists
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSpeakerIdentificationStatusReceived(object sender, SpeakerIdentificationStatusUpdateEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => 
            {
                if (e.IdentifiedProfile == null) return;

                SystemResponse = $"Hi there, {e.IdentifiedProfile.IdentifiedProfileId}";
            });
        }

        /// <summary>
        /// Event handler function for speaker identification errors.
        /// Displays erros in the debug console
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSpeakerIdentificationError(object sender, SpeakerIdentificationErrorEventArgs e)
        {
            Debug.WriteLine(e.ErrorMessage);
        }

        /// <summary>
        /// Function to determine if one can upload face images
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>True if a person group has been selected, false otherwise</returns>
        private bool CanUploadOwnerImage(object obj)
        {
            return SelectedPersonGroup != null;
        }

        /// <summary>
        /// Function to identify a person using an image. 
        /// Will ask the user to browse for an image, upload it, detect faces and try to identify it.
        /// Will print status messages to the UI
        /// </summary>
        /// <param name="obj"></param>
        private async void UploadOwnerImage(object obj)
        {
            try
            {
                SystemResponse = "Identifying...";

                var openDialog = new Microsoft.Win32.OpenFileDialog();

                openDialog.Filter = "Image Files(*.jpg, *.gif, *.bmp, *.png)|*.jpg;*.jpeg;*.gif;*.bmp;*.png";
                bool? result = openDialog.ShowDialog();

                if (!(bool)result) return;

                string filePath = openDialog.FileName;

                Uri fileUri = new Uri(filePath);
                BitmapImage image = new BitmapImage(fileUri);

                image.CacheOption = BitmapCacheOption.None;
                image.UriSource = fileUri;

                ImageSource = image;

                using (Stream imageFile = File.OpenRead(filePath))
                {
                    Face[] faces = await _faceServiceClient.DetectAsync(imageFile);
                    Guid[] faceIds = faces.Select(face => face.FaceId).ToArray();

                    IdentifyResult[] personsIdentified = await _faceServiceClient.IdentifyAsync(SelectedPersonGroup.PersonGroupId, faceIds, 1);

                    foreach(IdentifyResult personIdentified in personsIdentified)
                    { 
                        if(personIdentified.Candidates.Length == 0)     
                        {
                            SystemResponse = "Failed to identify you.";
                            break;
                        }

                        Guid personId = personIdentified.Candidates[0].PersonId;
                        Person person = await _faceServiceClient.GetPersonAsync(SelectedPersonGroup.PersonGroupId, personId);

                        if(person != null)
                        {
                            SystemResponse = $"Welcome home, {person.Name}";
                            break;
                        }
                    }
                }
            }
            catch(FaceAPIException ex)
            {
                SystemResponse = $"Failed to identify you: {ex.ErrorMessage}";
            }
            catch(Exception ex)
            {
                SystemResponse = $"Failed to identify you: {ex.Message}";
            }            
        }

        /// <summary>
        /// Function to retrieve all person groups
        /// </summary>
        private async void GetPersonGroups()
        {
            try
            {
                PersonGroup[] personGroups = await _faceServiceClient.ListPersonGroupsAsync();

                if (personGroups == null || personGroups.Length == 0) return;

                PersonGroups.Clear();

                foreach(PersonGroup group in personGroups)
                {
                    PersonGroups.Add(group);
                }
            }
            catch(FaceAPIException ex)
            {
                SystemResponse = $"Failed to get person groups: {ex.ErrorMessage}";
            }
            catch(Exception ex)
            {
                SystemResponse = $"Failed to get person groups: {ex.Message}";
            }
        }

        /// <summary>
        /// Function to determine if the web camera image grabbing can be started
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>True if any web cameras exist and a person group has been selected, false otherwise</returns>
        private bool CanStartCamera(object obj)
        {
            return _frameGrabber.GetNumCameras() > 0 && SelectedPersonGroup != null;
        }

        /// <summary>
        /// Command function to start the web camera image grabbing process
        /// </summary>
        /// <param name="obj"></param>
        private async void StartCamera(object obj)
        {
            _frameGrabber.TriggerAnalysisOnInterval(TimeSpan.FromSeconds(5));
            await _frameGrabber.StartProcessingCameraAsync();
        }

        /// <summary>
        /// Command function to stop web camera recording
        /// </summary>
        /// <param name="obj"></param>
        private async void StopCamera(object obj)
        {
            await _frameGrabber.StopProcessingAsync();
        }

        /// <summary>
        /// Function to create a BitmapSource and display it as an image in the UI, based on a given frame from a web camera
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNewFrameProvided(object sender, FrameGrabber<CameraResult>.NewFrameEventArgs e)
        {          
            Application.Current.Dispatcher.Invoke(() =>
            {
                BitmapSource bitmapSource = e.Frame.Image.ToBitmapSource();

                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                MemoryStream memoryStream = new MemoryStream();
                BitmapImage image = new BitmapImage();

                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(memoryStream);

                memoryStream.Position = 0;
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = memoryStream;
                image.EndInit();

                memoryStream.Close();

                ImageSource = image;
            });
        }

        /// <summary>
        /// Event handler function to handle new results from the web camera. 
        /// Proceeds to analyze emotions and prints it to the UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnResultAvailable(object sender, FrameGrabber<CameraResult>.NewResultEventArgs e)
        {
            var analysisResult = e.Analysis.EmotionScores;

            if (analysisResult == null) return;

            string emotion = AnalyseEmotions(analysisResult);

            Application.Current.Dispatcher.Invoke(() =>
            {
                SystemResponse = $"You seem to be {emotion} today.";
            });
        }

        /// <summary>
        /// Function to analyze face emotions, based on a given result score
        /// </summary>
        /// <param name="analysisResult"><see cref="Scores"/> result object</param>
        /// <returns>A string with the current emotion</returns>
        private string AnalyseEmotions(EmotionScores analysisResult)
        {
            string emotion = string.Empty;

            var sortedEmotions = analysisResult.ToRankedList();

            string currentEmotion = sortedEmotions.First().Key;

            switch(currentEmotion)
            {
                case "Anger":
                    emotion = "angry";
                    break;
                case "Contempt":
                    emotion = "contempt";
                    break;
                case "Disgust":
                    emotion = "disgusted";
                    break;
                case "Fear":
                    emotion = "scared";
                    break;
                case "Happiness":
                    emotion = "happy";
                    break;
                case "Neutral":
                default:
                    emotion = "neutral";
                    break;
                case "Sadness":
                    emotion = "sad";
                    break;
                case "Suprise":
                    emotion = "suprised";
                    break;
            }

            return emotion;
        }

        /// <summary>
        /// Function to analyze face emotions using a given frame from a web camera
        /// </summary>
        /// <param name="frame"><see cref="VideoFrame"/> from a web camera</param>
        /// <returns><see cref="CameraResult"/> containig an array of all emotions recognized</returns>
        private async Task<CameraResult> EmotionAnalysisAsync(VideoFrame frame)
        {
            MemoryStream jpg = frame.Image.ToMemoryStream(".jpg", s_jpegParams);

            try
            {
                Face[] face = await _faceServiceClient.DetectAsync(jpg, true, false, new List<FaceAttributeType> { FaceAttributeType.Emotion });
                EmotionScores emotions = face.First()?.FaceAttributes?.Emotion;

                return new CameraResult
                {
                    EmotionScores = emotions
                };
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Failed to analyze emotions: {ex.Message}");

                return null;
            }            
        }
    }

    /// <summary>
    /// Internal class containing an array of face emotion scores
    /// </summary>
    internal class CameraResult
    {
        public EmotionScores EmotionScores { get; set; } = null;
    }
}