using Chapter2.Interface;
using Microsoft.ProjectOxford.Vision;
using System;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.IO;
using System.Diagnostics;

namespace Chapter2.ViewModel
{
    public class ThumbnailViewModel : ObservableObject
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

        private BitmapImage _thumbnail;
        public BitmapImage Thumbnail
        {
            get { return _thumbnail; }
            set
            {
                _thumbnail = value;
                RaisePropertyChangedEvent("Thumbnail");
            }
        }

        public ICommand BrowseAndGenerateThumbnailCommand { get; private set; }

        /// <summary>
        /// ThumbnailViewModel constructor. Assigns the <see cref="IVisionServiceClient"/> API client
        /// </summary>
        /// <param name="visionClient"><see cref="IVisionServiceClient"/></param>
        public ThumbnailViewModel(IVisionServiceClient visionClient)
        {
            _visionClient = visionClient;
            Initialize();            
        }

        /// <summary>
        /// Function to intialize the ViewModel, by creating the command for generating a thumbnail of an image
        /// </summary>
        private void Initialize()
        {
            BrowseAndGenerateThumbnailCommand = new DelegateCommand(BrowseAndAnalyze);
        }

        /// <summary>
        /// Function to browse for an image and get a thumbnail for it. Thumbnail size is set to 250x250 pixels
        /// </summary>
        /// <param name="obj"></param>
        private async void BrowseAndAnalyze(object obj)
        {
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

            try
            { 
                using (Stream fileStream = File.OpenRead(filePath))
                {
                    byte[] thumbnailResult = await _visionClient.GetThumbnailAsync(fileStream, 250, 250);

                    if(thumbnailResult != null && thumbnailResult.Length != 0)
                        CreateThumbnail(thumbnailResult);
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Failed to analyze image: {ex.Message}");
            }
        }

        /// <summary>
        /// Function to generate a BitmapImage from the byte array returned in the thumbnail result
        /// </summary>
        /// <param name="thumbnailResult">byte array returned from the thumbnail generation</param>
        private void CreateThumbnail(byte[] thumbnailResult)
        {
            try
            {
                MemoryStream ms = new MemoryStream(thumbnailResult);
                ms.Seek(0, SeekOrigin.Begin);

                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.None;
                image.StreamSource = ms;
                image.EndInit();

                Thumbnail = image;                
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Failed to create thumbnail: {ex.Message}");
            }
        }
    }
}