using Chapter2.Interface;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Vision;

namespace Chapter2.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        private IVisionServiceClient _visionClient;
        private FaceServiceClient _faceServiceClient;

        private CelebrityViewModel _celebrityVm;
        public CelebrityViewModel CelebrityVm
        {
            get { return _celebrityVm; }
            set
            {
                _celebrityVm = value;
                RaisePropertyChangedEvent("CelebrityVm");
            }
        }

        private DescriptionViewModel _descriptionVm;
        public DescriptionViewModel DescriptionVm
        {
            get { return _descriptionVm; }
            set
            {
                _descriptionVm = value;
                RaisePropertyChangedEvent("DescriptionVm");
            }
        }

        private ImageAnalysisViewModel _imageAnalysisVm;
        public ImageAnalysisViewModel ImageAnalysisVm
        {
            get { return _imageAnalysisVm; }
            set
            {
                _imageAnalysisVm = value;
                RaisePropertyChangedEvent("ImageAnalysisVm");
            }
        }

        private OcrViewModel _ocrVm;
        public OcrViewModel OcrVm
        {
            get { return _ocrVm; }
            set
            {
                _ocrVm = value;
                RaisePropertyChangedEvent("OcrVm");
            }
        }

        private ThumbnailViewModel _thumbnailVm;
        public ThumbnailViewModel ThumbnailVm
        {
            get { return _thumbnailVm; }
            set
            {
                _thumbnailVm = value;
                RaisePropertyChangedEvent("ThumbnailVm");
            }
        }

        private FaceGroupingViewModel _faceGroupingVm;
        public FaceGroupingViewModel FaceGroupingVm
        {
            get { return _faceGroupingVm; }
            set
            {
                _faceGroupingVm = value;
                RaisePropertyChangedEvent("FaceGroupingVm");
            }
        }


        private SimilarFaceViewModel _similarFaceVm;
        public SimilarFaceViewModel SimilarFaceVm
        {
            get { return _similarFaceVm; }
            set
            {
                _similarFaceVm = value;
                RaisePropertyChangedEvent("SimilarFaceVm");
            }
        }

        private FaceVerificationViewModel _faceVerificationVm;

        public FaceVerificationViewModel FaceVerificationVm
        {
            get { return _faceVerificationVm; }
            set
            {
                _faceVerificationVm = value;
                RaisePropertyChangedEvent("FaceVerificationVm");
            }
        }

        /// <summary>
        /// MainViewModel constructor. Creates the required API clients and ViewModels
        /// </summary>
        public MainViewModel()
        {
            _visionClient = new VisionServiceClient("VISION_API_KEY", "ROOT_URI");

            CelebrityVm = new CelebrityViewModel(_visionClient);
            DescriptionVm = new DescriptionViewModel(_visionClient);
            ImageAnalysisVm = new ImageAnalysisViewModel(_visionClient);
            OcrVm = new OcrViewModel(_visionClient);
            ThumbnailVm = new ThumbnailViewModel(_visionClient);

            _faceServiceClient = new FaceServiceClient("FACE_API_KEY", "ROOT_URI");

            SimilarFaceVm = new SimilarFaceViewModel(_faceServiceClient);
            FaceGroupingVm = new FaceGroupingViewModel(_faceServiceClient);
            FaceVerificationVm = new FaceVerificationViewModel(_faceServiceClient);
        }
    }
}
