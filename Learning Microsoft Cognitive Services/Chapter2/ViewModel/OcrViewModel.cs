using Chapter2.Interface;
using System.Linq;
using System.Collections.Generic;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using System;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.IO;
using System.Diagnostics;
using System.Text;

namespace Chapter2.ViewModel
{
    public class OcrViewModel : ObservableObject
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
        
        private string _ocrResult;
        public string OcrResult
        {
            get { return _ocrResult; }
            set
            {
                _ocrResult = value;
                RaisePropertyChangedEvent("OcrResult");
            }
        }

        public ICommand BrowseAndAnalyzeImageCommand { get; private set; }

        /// <summary>
        /// OcrViewModel constructor. Assigns the <see cref="IVisionServiceClient"/> API client
        /// </summary>
        public OcrViewModel(IVisionServiceClient visionClient)
        {
            _visionClient = visionClient;
            Initialize();            
        }

        /// <summary>
        /// Function to intialize the ViewModel, by creating the command for parsing text in images
        /// </summary>
        private void Initialize()
        {
            BrowseAndAnalyzeImageCommand = new DelegateCommand(BrowseAndAnalyze);
        }

        /// <summary>
        /// Funciton to browse for an image. Text in images will be recognized by running a text analysis API call
        /// </summary>
        /// <param name="obj"></param>
        private async void BrowseAndAnalyze(object obj)
        {
            var openDialog = new Microsoft.Win32.OpenFileDialog();

            openDialog.Filter = "JPEG Image(*.jpg)|*.jpg";
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
                    OcrResults analysisResult = await _visionClient.RecognizeTextAsync(fileStream);

                    if(analysisResult != null)
                        OcrResult = PrintOcrResult(analysisResult);
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Failed to analyze image: {ex.Message}");
            }
        }

        /// <summary>
        /// If a text analysis calls succeeds, this function parses the results
        /// </summary>
        /// <param name="ocrResult"><see cref="OcrResult"/></param>
        /// <returns>A formatted string with the text found in an image</returns>
        private string PrintOcrResult(OcrResults ocrResult)
        {
            StringBuilder result = new StringBuilder();

            result.AppendFormat("Language is {0}\n", ocrResult.Language);
            result.Append("The words are:\n\n");
            
            foreach(var region in ocrResult.Regions)
            { 
                foreach(var line in region.Lines)
                { 
                    foreach(var text in line.Words)
                    { 
                        result.AppendFormat("{0} ", text.Text);
                    }

                    result.Append("\n");
                }
                result.Append("\n\n");
            }

            return result.ToString();
        }
    }
}