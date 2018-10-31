using Chapter2.Interface;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Chapter2.ViewModel
{
    public class FaceVerificationViewModel : ObservableObject
    {
        private FaceServiceClient _faceServiceClient;
        private Guid _faceId1 = Guid.Empty;
        private Guid _faceId2 = Guid.Empty;

        private BitmapImage _image1Source;
        public BitmapImage Image1Source
        {
            get { return _image1Source; }
            set
            {
                _image1Source = value;
                RaisePropertyChangedEvent("Image1Source");
            }
        }

        private BitmapImage _image2Source;
        public BitmapImage Image2Source
        {
            get { return _image2Source; }
            set
            {
                _image2Source = value;
                RaisePropertyChangedEvent("Image2Source");
            }
        }

        private string _faceVerificationResult;
        public string FaceVerificationResult
        {
            get { return _faceVerificationResult; }
            set
            {
                _faceVerificationResult = value;
                RaisePropertyChangedEvent("FaceVerificationResult");
            }
        }

        public ICommand BrowseImage1Command { get; private set; }
        public ICommand BrowseImage2Command { get; private set; }
        public ICommand VerifyImageCommand { get; private set; }

        /// <summary>
        /// FaceVerificationViewModel constructor. Assigns the <see cref="FaceServiceClient"/> API client
        /// </summary>
        /// <param name="faceServiceClient"><see cref="FaceServiceClient"/></param>
        public FaceVerificationViewModel(FaceServiceClient faceServiceClient)
        {
            _faceServiceClient = faceServiceClient;

            Initialize();
        }
        /// <summary>
        /// Function to intialize the ViewModel, by creating the command for browse images and verify them 
        /// </summary>
        private void Initialize()
        {
            BrowseImage1Command = new DelegateCommand(BrowseImage1);
            BrowseImage2Command = new DelegateCommand(BrowseImage2);
            VerifyImageCommand = new DelegateCommand(VerifyFace, CanVerifyFace);
        }

        /// <summary>
        /// Command function to browse for the first image
        /// </summary>
        /// <param name="obj"></param>
        private async void BrowseImage1(object obj)
        {
            Image1Source = await BrowseImageAsync(1);
        }

        /// <summary>
        /// Command function to browse for the second image
        /// </summary>
        /// <param name="obj"></param>
        private async void BrowseImage2(object obj)
        {
            Image2Source = await BrowseImageAsync(2);
        }

        /// <summary>
        /// Function to browse for images. Will detect faces in the image and assign the Face ID to a member, for usage in the face verification process
        /// </summary>
        /// <param name="imagenumber"></param>
        /// <returns></returns>
        private async Task<BitmapImage> BrowseImageAsync(int imagenumber)
        {
            var openDialog = new Microsoft.Win32.OpenFileDialog();

            openDialog.Filter = "Image Files(*.jpg, *.gif, *.bmp, *.png)|*.jpg;*.jpeg;*.gif;*.bmp;*.png";
            bool? result = openDialog.ShowDialog();

            if (!(bool)result) return null;

            string filePath = openDialog.FileName;

            try
            {
                using (Stream fileStream = File.OpenRead(filePath))
                {
                    Face[] detectedFaces = await _faceServiceClient.DetectAsync(fileStream);

                    if (imagenumber == 1)
                        _faceId1 = detectedFaces[0].FaceId;
                    else
                        _faceId2 = detectedFaces[0].FaceId;
                }
            }
            catch (FaceAPIException ex)
            {
                Debug.WriteLine(ex.ErrorMessage);
                FaceVerificationResult = $"Failed to detect faces with error message: {ex.ErrorMessage}";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to detect face: {ex.Message}");
            }

            Uri fileUri = new Uri(filePath);
            BitmapImage image = new BitmapImage(fileUri);

            image.CacheOption = BitmapCacheOption.None;
            image.UriSource = fileUri;

            return image;
        }

        /// <summary>
        /// Function to determine whether or not we can execute the verify command
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>Returns true if both images has been loaded, otherwise false</returns>
        private bool CanVerifyFace(object obj)
        {
            return !_faceId1.Equals(Guid.Empty) && !_faceId2.Equals(Guid.Empty);
        }

        /// <summary>
        /// Command function to verify if faces in two images is similar. Will output the result and probability in the UI
        /// </summary>
        /// <param name="obj"></param>
        private async void VerifyFace(object obj)
        {
            try
            {
                VerifyResult verificationResult = await _faceServiceClient.VerifyAsync(_faceId1, _faceId2);

                FaceVerificationResult = $"The two provided faces is identical: {verificationResult.IsIdentical}, with confidence: {verificationResult.Confidence}";
            }
            catch(FaceAPIException ex)
            {
                Debug.WriteLine(ex.ErrorMessage);
                FaceVerificationResult = $"Failed to verify faces with error message: {ex.ErrorMessage}";
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
