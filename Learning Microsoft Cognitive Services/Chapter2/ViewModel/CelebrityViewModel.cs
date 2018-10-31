using Chapter2.Interface;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using System;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Diagnostics;
using System.Text;

namespace Chapter2.ViewModel
{
    public class CelebrityViewModel : ObservableObject
    {
        private IVisionServiceClient _visionClient;

        private BitmapImage _iamgeSource;
        public BitmapImage ImageSource
        {
            get { return _iamgeSource; }
            set
            {
                _iamgeSource = value;
                RaisePropertyChangedEvent("ImageSource");
            }
        }

        private string _imageUrl;
        public string ImageUrl
        {
            get { return _imageUrl; }
            set
            {
                _imageUrl = value;
                RaisePropertyChangedEvent("ImageUrl");
            }
        }

        private string _celebrity;
        public string Celebrity
        {
            get { return _celebrity; }
            set
            {
                _celebrity = value;
                RaisePropertyChangedEvent("Celebrity");
            }
        }

        public ICommand LoadAndFindCelebrityCommand { get; private set; }

        /// <summary>
        /// CelebrityViewModel constructor. Assigns the <see cref="IVisionServiceClient"/> API client and creates the command object
        /// </summary>
        /// <param name="visionClient"><see cref="IVisionServiceClient"/></param>
        public CelebrityViewModel(IVisionServiceClient visionClient)
        {
            _visionClient = visionClient;
            LoadAndFindCelebrityCommand = new DelegateCommand(LoadAndFindCelebrity, CanFindCelebrity);
        }
        
        /// <summary>
        /// Function to determine whether or not we can execute a command
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>Returns true if we have an image, otherwise false</returns>
        private bool CanFindCelebrity(object obj)
        {
            return !string.IsNullOrEmpty(ImageUrl);
        }

        /// <summary>
        /// Async function to execute FindCelebrity command. Will output celebrities found in image to the UI
        /// </summary>
        /// <param name="obj"></param>
        private async void LoadAndFindCelebrity(object obj)
        {
            Uri fileUri = new Uri(ImageUrl);
            BitmapImage image = new BitmapImage(fileUri);

            image.CacheOption = BitmapCacheOption.None;
            image.UriSource = fileUri;

            ImageSource = image;

            try
            {
                AnalysisInDomainResult celebrityResult = await _visionClient.AnalyzeImageInDomainAsync(ImageUrl, "celebrities");

                if (celebrityResult != null)
                    Celebrity = celebrityResult.Result.ToString();
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Failed to analyze image: {ex.Message}");
            }
        }
    }
}
