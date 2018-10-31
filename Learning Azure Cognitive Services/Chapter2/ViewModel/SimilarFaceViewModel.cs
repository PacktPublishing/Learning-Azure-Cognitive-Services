using Chapter2.Interface;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Chapter2.ViewModel
{
    public class SimilarFaceViewModel : ObservableObject
    {
        private bool _faceListExists = false;
        private FaceServiceClient _faceServiceClient;

        private string _faceListName;
        public string FaceListName
        {
            get { return _faceListName; }
            set
            {
                _faceListName = value;
                RaisePropertyChangedEvent("FaceListName");
            }
        }

        private ObservableCollection<Guid> _faceIds = new ObservableCollection<Guid>();
        public ObservableCollection<Guid> FaceIds
        {
            get { return _faceIds; }
            set
            {
                _faceIds = value;
                RaisePropertyChangedEvent("FaceIds");
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

        private string _similarResult;
        public string SimilarResult
        {
            get { return _similarResult; }
            set
            {
                _similarResult = value;
                RaisePropertyChangedEvent("SimilarResult");
            }
        }

        public ICommand CreateFaceListCommand { get; private set; }
        public ICommand FindSimilarFaceCommand { get; private set; }
        public ICommand AddExampleFacesToListCommand { get; private set; }

        /// <summary>
        /// SimilarFaceViewModel constructor. Assigns a <see cref="FaceServiceClient"/> API client
        /// </summary>
        /// <param name="faceServiceClient"><see cref="FaceServiceClient"/></param>
        public SimilarFaceViewModel(FaceServiceClient faceServiceClient)
        {
            _faceServiceClient = faceServiceClient;

            Initialize();
        }
        /// <summary>
        /// Function to intialize the ViewModel. Creates commands for creating face lists, finding similar faces and adding example faces. Also updates face IDs
        /// </summary>
        private async void Initialize()
        {
            FaceListName = "Chapter2";

            CreateFaceListCommand = new DelegateCommand(CreateFaceListAsync, CanCreateFaceList);
            FindSimilarFaceCommand = new DelegateCommand(FindSimilarFace);
            AddExampleFacesToListCommand = new DelegateCommand(AddExampleFacesToList, CanAddExampleFaces);

            await DoesFaceListExistAsync();
            UpdateFaceGuidsAsync();
        }
        
        /// <summary>
        /// Function to determine if we can execute the create face list command
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>True if we have entered a face list name, false otherwise</returns>
        private bool CanCreateFaceList(object obj)
        {
            return !string.IsNullOrEmpty(FaceListName);
        }

        /// <summary>
        /// Function to create new face list
        /// </summary>
        /// <param name="obj"></param>
        private async void CreateFaceListAsync(object obj)
        {
            try
            {
                if (!_faceListExists)
                {
                    await _faceServiceClient.CreateFaceListAsync(FaceListName.ToLower(), FaceListName, string.Empty);

                    await DoesFaceListExistAsync();
                }
            }
            catch(FaceAPIException ex)
            {
                Debug.WriteLine($"Failed to create face list - {ex.ErrorMessage}");
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Function to find similar faces. Will first detect any faces in the given photo, and use this result for the similar faces search
        /// </summary>
        /// <param name="obj"></param>
        private async void FindSimilarFace(object obj)
        {
            if (!_faceListExists || FaceIds.Count == 0)
                SimilarResult = "Face list does not exist";

            Guid findFaceGuid = await BrowseAndDetectFaces();

            if (findFaceGuid.Equals(Guid.Empty)) return;

            try
            { 
                SimilarPersistedFace[] similarFaces = await _faceServiceClient.FindSimilarAsync(findFaceGuid, FaceListName.ToLower(), 3);

                if (similarFaces == null || similarFaces.Length == 0)
                    SimilarResult = "No faces were similar";

                StringBuilder result = new StringBuilder();
                result.Append("Similar faces:\n");

                if (similarFaces.Length == 0)
                    result.Append("No similar faces found");

                foreach(SimilarPersistedFace similarFace in similarFaces)
                {
                    result.AppendFormat("Face ID: {0}\n", similarFace.PersistedFaceId);
                    result.AppendFormat("Probability: {0}\n\n", similarFace.Confidence);
                }

                SimilarResult = result.ToString();
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Function to determine if we can execute the command to add example faces to a face list
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>Returns true if face list exist, false otherwise</returns>
        private bool CanAddExampleFaces(object obj)
        {
            return _faceListExists;
        }

        /// <summary>
        /// Function to add example faces to a given face list. Will loop through a folder and add all image files in that folder
        /// </summary>
        /// <param name="obj"></param>
        private async void AddExampleFacesToList(object obj)
        {
            string personGroupDirectory = Path.Combine(Environment.CurrentDirectory, "PersonGroup");
            string[] images = GetImageFiles(personGroupDirectory);

            try
            { 
                foreach(string image in images)
                {
                    using (Stream fileStream = File.OpenRead(image))
                    {
                        Face[] faces = await _faceServiceClient.DetectAsync(fileStream);
                        FaceRectangle faceRectangle = faces[0].FaceRectangle;

                        AddPersistedFaceResult addFacesResult = 
                            await _faceServiceClient.AddFaceToFaceListAsync(
                                FaceListName.ToLower(), 
                                fileStream, null, faceRectangle);
                        UpdateFaceGuidsAsync();
                    }
                }
            }
            catch(FaceAPIException ex)
            {
                Debug.WriteLine($"Failed to add faces to face list: {ex.ErrorMessage}");
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Function to check if a given face list exist or not.
        /// </summary>
        /// <returns></returns>
        private async Task DoesFaceListExistAsync()
        {
            FaceListMetadata[] faceLists = await _faceServiceClient.ListFaceListsAsync();

            if (faceLists == null) return;

            _faceListExists = false;

            foreach (FaceListMetadata faceList in faceLists)
            {
                if (faceList.Name.Equals(FaceListName))
                {
                    _faceListExists = true;
                    break;
                }
            }
        }
        
        /// <summary>
        /// Function to browse for images and detect faces in that image. Will return the Guid id for the first face found in any image
        /// </summary>
        /// <returns></returns>
        private async Task<Guid> BrowseAndDetectFaces()
        {
            var openDialog = new Microsoft.Win32.OpenFileDialog();

            openDialog.Filter = "Image Files(*.jpg, *.gif, *.bmp, *.png)|*.jpg;*.jpeg;*.gif;*.bmp;*.png";
            bool? result = openDialog.ShowDialog();

            if (!(bool)result) return Guid.Empty;

            string filePath = openDialog.FileName;

            Uri fileUri = new Uri(filePath);
            BitmapImage image = new BitmapImage(fileUri);

            image.CacheOption = BitmapCacheOption.None;
            image.UriSource = fileUri;

            ImageSource = image;

            Guid faceGuid = Guid.Empty;

            try
            {
                using (Stream fileStream = File.OpenRead(filePath))
                {
                    Face[] detectedFaces = await _faceServiceClient.DetectAsync(fileStream);

                    faceGuid = detectedFaces[0].FaceId;
                }
            }
            catch(FaceAPIException ex)
            {
                Debug.WriteLine($"Failed to detect face: {ex.ErrorMessage}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to detect face: {ex.Message}");
            }

            return faceGuid;
        }

        /// <summary>
        /// Function to update the list of Face IDs, based on the example images added
        /// </summary>
        private async void UpdateFaceGuidsAsync()
        {
            if (!_faceListExists) return;

            try
            { 
                FaceList faceList = await _faceServiceClient.GetFaceListAsync(FaceListName.ToLower());

                if (faceList == null) return;

                PersistedFace[] faces = faceList.PersistedFaces; 

                foreach (PersistedFace face in faces)
                { 
                    FaceIds.Add(face.PersistedFaceId);
                }
            }
            catch(FaceAPIException ex)
            {
                Debug.WriteLine($"Failed to get face list - {ex.ErrorMessage}");
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        
        /// <summary>
        /// Function to retrieve all images in a given folder
        /// </summary>
        /// <param name="directories">String array of directories to find images in</param>
        /// <returns>String array of all images found</returns>
        private string[] GetImageFiles(string directory)
        {
            List<string> images = new List<string>();
            
            string[] files = Directory.GetFiles(directory);

            foreach (string file in files)
            {
                images.Add(file);
            }

            return images.ToArray();
        }
    }
}