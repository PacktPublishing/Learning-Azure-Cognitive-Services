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
    public class FaceGroupingViewModel : ObservableObject
    {
        private FaceServiceClient _faceServiceClient;
        private List<string> _imageFiles = new List<string>();
        private List<Guid> _faceIds = new List<Guid>();

        private BitmapImage _image1;
        public BitmapImage Image1
        {
            get { return _image1; }
            set
            {
                _image1 = value;
                RaisePropertyChangedEvent("Image1");
            }
        }

        private BitmapImage _image2;
        public BitmapImage Image2
        {
            get { return _image2; }
            set
            {
                _image2 = value;
                RaisePropertyChangedEvent("Image2");
            }
        }

        private BitmapImage _image3;
        public BitmapImage Image3
        {
            get { return _image3; }
            set
            {
                _image3 = value;
                RaisePropertyChangedEvent("Image3");
            }
        }

        private BitmapImage _image4;
        public BitmapImage Image4
        {
            get { return _image4; }
            set
            {
                _image4 = value;
                RaisePropertyChangedEvent("Image4");
            }
        }

        private BitmapImage _image5;
        public BitmapImage Image5
        {
            get { return _image5; }
            set
            {
                _image5 = value;
                RaisePropertyChangedEvent("Image5");
            }
        }

        private BitmapImage _image6;
        public BitmapImage Image6
        {
            get { return _image6; }
            set
            {
                _image6 = value;
                RaisePropertyChangedEvent("Image6");
            }
        }

        private string _faceGroupingResult;
        public string FaceGroupingResult
        {
            get { return _faceGroupingResult; }
            set
            {
                _faceGroupingResult = value;
                RaisePropertyChangedEvent("FaceGroupingResult");
            }
        }

        private string _imageId1;
        public string ImageId1
        {
            get { return _imageId1; }
            set
            {
                _imageId1 = value;
                RaisePropertyChangedEvent("ImageId1");
            }
        }

        private string _imageId2;
        public string ImageId2
        {
            get { return _imageId2; }
            set
            {
                _imageId2 = value;
                RaisePropertyChangedEvent("ImageId2");
            }
        }

        private string _imageId3;
        public string ImageId3
        {
            get { return _imageId3; }
            set
            {
                _imageId3 = value;
                RaisePropertyChangedEvent("ImageId3");
            }
        }

        private string _imageId4;
        public string ImageId4
        {
            get { return _imageId4; }
            set
            {
                _imageId4 = value;
                RaisePropertyChangedEvent("ImageId4");
            }
        }

        private string _imageId5;
        public string ImageId5
        {
            get { return _imageId5; }
            set
            {
                _imageId5 = value;
                RaisePropertyChangedEvent("ImageId5");
            }
        }

        private string _imageId6;
        public string ImageId6
        {
            get { return _imageId6; }
            set
            {
                _imageId6 = value;
                RaisePropertyChangedEvent("ImageId6");
            }
        }

        public ICommand GroupImagesCommand { get; private set; }

        /// <summary>
        /// FaceGroupingViewModel constructor. Assigns the <see cref="FaceServiceClient"/> API client
        /// </summary>
        /// <param name="faceServiceClient"><see cref="FaceServiceClient"/></param>
        public FaceGroupingViewModel(FaceServiceClient faceServiceClient)
        {
            _faceServiceClient = faceServiceClient;

            Initialize();
        }

        /// <summary>
        /// Function to intialize the ViewModel, by creating the command for grouping images. Also loads all images
        /// </summary>
        private void Initialize()
        {
            GroupImagesCommand = new DelegateCommand(GroupFaces);
            GetImageFilesAsync();
        }

        /// <summary>
        /// Function to execute the face grouping
        /// </summary>
        /// <param name="obj"></param>
        private async void GroupFaces(object obj)
        {
            try
            {
                GroupResult faceGroups = await _faceServiceClient.GroupAsync(_faceIds.ToArray());

                if (faceGroups != null)
                    FaceGroupingResult = ParseGroupResult(faceGroups);
            }
            catch(FaceAPIException ex)
            {
                FaceGroupingResult = $"Failed to group images, with error: {ex.ErrorMessage}";
                Debug.WriteLine(ex.ErrorMessage);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Function to parse face grouping results
        /// </summary>
        /// <param name="faceGroups"><see cref="GroupResult"/> from a face grouping call</param>
        /// <returns>A formatted string containing all faces in correct groups</returns>
        private string ParseGroupResult(GroupResult faceGroups)
        {
            StringBuilder result = new StringBuilder();

            List<Guid[]> groups = faceGroups.Groups;

            result.AppendFormat("There are {0} group(s)\n", groups.Count);
            result.Append("Groups:\t");

            foreach(Guid[] guid in groups)
            {
                foreach(Guid id in guid)
                {
                    result.AppendFormat("{0} - ", GetImageName(id));
                }

                result.Append("\n");
            }

            result.Append("Messy group:\t");

            Guid[] messyGroup = faceGroups.MessyGroup;
            foreach(Guid guid in messyGroup)
            {
                result.AppendFormat("{0} - ", GetImageName(guid));
            }

            return result.ToString();
        }

        /// <summary>
        /// Function to get image names based on the Guid ID of the image
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private string GetImageName(Guid id)
        {
            int index = _faceIds.FindIndex(a => a == id);

            if (index > -1)
                return $"Image {index + 1}";
            else
                return string.Empty;
        }

        /// <summary>
        /// Function to retrieve all images found in a folder, display them and detect faces in them.
        /// </summary>
        private async void GetImageFilesAsync()
        {
            _imageFiles = Directory.GetFiles("PersonGroup").ToList();

            for (int i = 0; i < _imageFiles.Count; i++)
            {
                Uri fileUri = new Uri(Path.GetFullPath(_imageFiles[i]));
                BitmapImage image = new BitmapImage(fileUri);

                image.CacheOption = BitmapCacheOption.None;
                image.UriSource = fileUri;

                try
                {
                    using (Stream fileStream = File.OpenRead(_imageFiles[i]))
                    {
                        Face[] faces = await _faceServiceClient.DetectAsync(fileStream);

                        _faceIds.Add(faces[0].FaceId);
                        CreateImageSources(image, i, faces[0].FaceId);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// Function to assign images to the corresponding image in the UI
        /// </summary>
        /// <param name="image">BitmapImage to be displayed</param>
        /// <param name="position">Position of the current image</param>
        /// <param name="faceId">ID of the face, in the current image</param>
        private void CreateImageSources(BitmapImage image, int position, Guid faceId)
        {
            switch(position)
            {
                case 0:
                    Image1 = image;
                    ImageId1 = faceId.ToString();
                    break;
                case 1:
                    Image2 = image;
                    ImageId2 = faceId.ToString();
                    break;
                case 2:
                    Image3 = image;
                    ImageId3 = faceId.ToString();
                    break;
                case 3:
                    Image4 = image;
                    ImageId4 = faceId.ToString();
                    break;
                case 4:
                    Image5 = image;
                    ImageId5 = faceId.ToString();
                    break;
                case 5:
                    Image6 = image;
                    ImageId6 = faceId.ToString();
                    break;
                default:
                    break;
            }
        }
    }
}
