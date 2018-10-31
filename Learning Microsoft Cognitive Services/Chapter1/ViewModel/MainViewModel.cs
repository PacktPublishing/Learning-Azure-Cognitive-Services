using Chapter1.Interface;
using System.Windows.Input;
using System;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using Chapter1.Model;
using System.Threading;
using System.Media;
using System.Collections.Generic;

namespace Chapter1.ViewModel
{
    public class MainViewModel : ObservableObject, IDisposable
    {
        private TextToSpeak _textToSpeak;
        private string _filePath;
        private FaceServiceClient _faceServiceClient;

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

        private string _statusText;
        public string StatusText
        {
            get { return _statusText; }
            set
            {
                _statusText = value;
                RaisePropertyChangedEvent("StatusText");
            }
        }
        
        public ICommand BrowseButtonCommand { get; private set; }
        public ICommand DetectFaceCommand { get; private set; }

        /// <summary>
        /// Constructor for MainViewModel. Creates the required API clients, and ICommand objects
        /// </summary>
        public MainViewModel()
        {
            StatusText = "Status: Waiting for image...";

            _faceServiceClient = new FaceServiceClient("FACE_API_KEY", "ROOT_URI");

            BrowseButtonCommand = new DelegateCommand(Browse);
            DetectFaceCommand = new DelegateCommand(DetectFace, CanDetectFace);

            _textToSpeak = new TextToSpeak();
            _textToSpeak.OnAudioAvailable += _textToSpeak_OnAudioAvailable;
            _textToSpeak.OnError += _textToSpeak_OnError;
            GenerateToken();
        }

        private async void GenerateToken()
        {
            if (await _textToSpeak.GenerateAuthenticationToken("BING_SPEECH_API_KEY"))
                _textToSpeak.GenerateHeaders();
        }

        /// <summary>
        /// Function to browse for an image and open it. When opened it will be displayed in the UI
        /// </summary>
        /// <param name="obj"></param>
        private void Browse(object obj)
        {
            var openDialog = new Microsoft.Win32.OpenFileDialog();

            openDialog.Filter = "JPEG Image(*.jpg)|*.jpg";
            bool? result = openDialog.ShowDialog();

            if (!(bool)result) return;

            _filePath = openDialog.FileName;

            Uri fileUri = new Uri(_filePath);
            BitmapImage image = new BitmapImage(fileUri);
            
            image.CacheOption = BitmapCacheOption.None;
            image.UriSource = fileUri;

            ImageSource = image;
            StatusText = "Status: Image loaded...";
        }

        /// <summary>
        /// Async function to execute DetectFace command. Will detect faces and speak the number of faces out loud, if speakers is connected
        /// </summary>
        /// <param name="obj"></param>
        private async void DetectFace(object obj)
        {
            FaceRectangle[] faceRects = await UploadAndDetectFacesAsync();

            string textToSpeak = "No faces detected";

            if (faceRects.Length == 1)
                textToSpeak = "1 face detected";
            else if (faceRects.Length > 1)
                textToSpeak = $"{faceRects.Length} faces detected";

            Debug.WriteLine(textToSpeak);
            
            await _textToSpeak.SpeakAsync(textToSpeak, CancellationToken.None);
        }

        /// <summary>
        /// Function to say if we CanExecute the command
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>Returns true if an image is loaded, false otherwise</returns>
        private bool CanDetectFace(object obj)
        {
            return !string.IsNullOrEmpty(ImageSource?.UriSource.ToString());
        }

        /// <summary>
        /// Async function to detect faces in an image. 
        /// </summary>
        /// <returns>Returns a <see cref="FaceRectangle"/> array containing all faces detected </returns>
        private async Task<FaceRectangle[]> UploadAndDetectFacesAsync()
        {
            StatusText = "Status: Detecting faces...";

            try
            {
                using (Stream imageFileStream = File.OpenRead(_filePath))
                {
                    Face[] faces = await _faceServiceClient.DetectAsync(imageFileStream, true, true, new List<FaceAttributeType>() { FaceAttributeType.Age });

                    List<double> ages = faces.Select(face => face.FaceAttributes.Age).ToList();
                    FaceRectangle[] faceRects = faces.Select(face => face.FaceRectangle).ToArray();

                    StatusText = "Status: Finished detecting faces...";

                    foreach(var age in ages)
                    {
                        Console.WriteLine(age);
                    }

                    return faceRects;
                }
            }
            catch(Exception ex)
            {
                StatusText = $"Status: Failed to detect faces - {ex.Message}";

                return new FaceRectangle[0];
            }
        }

        /// <summary>
        /// Event handler to play audio from text-to-speech conversions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _textToSpeak_OnAudioAvailable(object sender, AudioEventArgs e)
        {
            SoundPlayer player = new SoundPlayer(e.EventData);
            player.Play();
            e.EventData.Dispose();
        }

        /// <summary>
        /// Event handler for text-to-speech errors
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _textToSpeak_OnError(object sender, AudioErrorEventArgs e)
        {
            StatusText = $"Status: Audio service failed -  {e.ErrorMessage}";
        }

        public void Dispose()
        {
            _faceServiceClient.Dispose();
        }
    }
}
