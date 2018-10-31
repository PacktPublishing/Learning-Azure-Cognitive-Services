using Chapter2.Interface;
using System.Linq;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using System;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Diagnostics;
using System.Text;

namespace Chapter2.ViewModel
{
    public class DescriptionViewModel : ObservableObject
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

        private int _numberOfDescriptions;
        public int NumberOfDescriptions
        {
            get { return _numberOfDescriptions; }
            set
            {
                _numberOfDescriptions = value;
                RaisePropertyChangedEvent("NumberOfDescriptions");
            }
        }
        
        private string _description;
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                RaisePropertyChangedEvent("Description");
            }
        }

        public ICommand LoadAndDescribeCommand { get; private set; }

        /// <summary>
        /// DescriptionViewModel constructor. Assigns the <see cref="IVisionServiceClient"/> API client
        /// </summary>
        /// <param name="visionClient"><see cref="IVisionServiceClient"/></param>
        public DescriptionViewModel(IVisionServiceClient visionClient)
        {
            _visionClient = visionClient;
            Initialize();            
        }

        /// <summary>
        /// Function to intialize the ViewModel, by creating the command for describing an image
        /// </summary>
        private void Initialize()
        {
            LoadAndDescribeCommand = new DelegateCommand(LoadAndDescribe, CanLoadAndDescribe);
        }

        /// <summary>
        /// Function to determine whether or not we can describe an image
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>Returns true if an image has been loaded, otherwise false</returns>
        private bool CanLoadAndDescribe(object obj)
        {
            return !string.IsNullOrEmpty(ImageUrl);
        }

        /// <summary>
        /// Function to describe an image
        /// </summary>
        /// <param name="obj"></param>
        private async void LoadAndDescribe(object obj)
        {
            Uri fileUri = new Uri(ImageUrl);
            BitmapImage image = new BitmapImage(fileUri);

            image.CacheOption = BitmapCacheOption.None;
            image.UriSource = fileUri;

            ImageSource = image;

            try
            {
                AnalysisResult descriptionResult = await _visionClient.DescribeAsync(ImageUrl, NumberOfDescriptions); 

                if(descriptionResult != null)
                    Description = PrintDescriptionResult(descriptionResult);
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Failed to describe image: {ex.Message}");
            }
        }

        /// <summary>
        /// Function to output the image description to the UI
        /// </summary>
        /// <param name="description">Accepts <see cref="AnalysisResult"/> of the current image</param>
        /// <returns>A formatted string containing all possible image descriptions</returns>
        private string PrintDescriptionResult(AnalysisResult description)
        {
            StringBuilder result = new StringBuilder();

            if (description.Description != null)
            {
                result.AppendFormat("The number if descriptions is: {0}\n\n", description.Description.Captions.Count());

                foreach(var desc in description.Description.Captions)
                {
                    result.AppendFormat("Description: {0}\n", desc.Text);
                    result.AppendFormat("Probability: {0}\n\n", desc.Confidence);
                }
            }

            return result.ToString();
        }
    }
}
