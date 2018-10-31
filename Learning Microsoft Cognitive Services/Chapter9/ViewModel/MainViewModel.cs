using Chapter9.Interface;
using System.Windows.Input;
using System;
using Chapter9.Model;
using System.Linq;
using System.Collections.Generic;
using Chapter9.Contracts;
using System.Text;

namespace Chapter9.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        private BingAutoSuggest _autoSuggest;
        private BingSearch _bingSearch;
        
        public IEnumerable<SearchType> SearchTypes
        {
            get { return Enum.GetValues(typeof(SearchType)).Cast<SearchType>(); }
        }

        private SearchType _selectedSearchType;
        public SearchType SelectedSearchType
        {
            get { return _selectedSearchType; }
            set
            {
                _selectedSearchType = value;
                RaisePropertyChangedEvent("SelectedSearchType");
            }
        }

        private string _searchQuery;
        public string SearchQuery
        {
            get { return _searchQuery; }
            set
            {
                _searchQuery = value;
                RaisePropertyChangedEvent("SearchQuery");
                GetAutosuggestions();
            }
        }

        private IEnumerable<string> _suggestions;
        public IEnumerable<string> Suggestions
        {
            get { return _suggestions; }
            set
            {
                _suggestions = value;
                RaisePropertyChangedEvent("Suggestions");
            }
        }

        private string _searchResult;
        public string SearchResult
        {
            get { return _searchResult; }
            set
            {
                _searchResult = value;
                RaisePropertyChangedEvent("SearchResult");
            }
        }

        public ICommand SearchCommand { get; private set; }

        /// <summary>
        /// MainViewModel constructor. Creates <see cref="BingAutoSuggest"/> and <see cref="BingSearch"/> objects and the search command
        /// </summary>
        public MainViewModel()
        {
            _autoSuggest = new BingAutoSuggest();
            _bingSearch = new BingSearch();

            SearchCommand = new DelegateCommand(Search);

            SelectedSearchType = SearchTypes.FirstOrDefault();
        }

        /// <summary>
        /// Command function to call the search API. Will call the correct search, based on the selected search type
        /// </summary>
        /// <param name="obj"></param>
        private async void Search(object obj)
        {
            SearchResult = string.Empty;
            switch(SelectedSearchType)
            {
                case SearchType.ImageSearch:
                    var imageResponse = await _bingSearch.SearchImages(SearchQuery);
                    ParseImageResponse(imageResponse);
                    break;
                case SearchType.VideoSearch:
                    var videoResponse = await _bingSearch.SearchVideos(SearchQuery);
                    ParseVideoResponse(videoResponse);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Function to parse image responses. Will display image title, image host and image URL in the UI
        /// </summary>
        /// <param name="imageResponse"></param>
        private void ParseImageResponse(ImageSearchResponse imageResponse)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Image search results:\n\n");
            sb.AppendFormat("# of results: {0}\n\n", imageResponse.totalEstimatedMatches);

            foreach (Value image in imageResponse.value)
            {
                sb.AppendFormat("\tImage name: {0}\n\tImage size: {1}\n\tImage host: {2}\n\tImage URL: {3}\t\n\n", image.name, image.contentSize, image.hostPageDisplayUrl, image.contentUrl);
            }

            SearchResult = sb.ToString();
        }

        /// <summary>
        /// Function to parse the video response. Will display video duration, video URL and video name in the UI
        /// </summary>
        /// <param name="videoResponse"></param>
        private void ParseVideoResponse(VideoSearchResponse videoResponse)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Video search results:\n\n");
            sb.AppendFormat("# of results: {0}\n\n", videoResponse.totalEstimatedMatches);

            foreach (VideoValue video in videoResponse.value)
            {
                sb.AppendFormat("\tVideo name: {0}\n\tVideo duration: {1}\n\tVideo URL: {2}\t\n", video.name, video.duration, video.contentUrl);

                foreach(Publisher publisher in video.publisher)
                {
                    sb.AppendFormat("\tPublisher: {0}\n", publisher.name);
                }

                sb.Append("\n");
            }

            SearchResult = sb.ToString();
        }

        /// <summary>
        /// Function to get auto suggestions. Will be triggered whenever the search query is changed
        /// Upon successful result, it will update the suggestion list
        /// </summary>
        private async void GetAutosuggestions()
        {
            var results = await _autoSuggest.Suggest(SearchQuery);

            if (results == null || results.Count == 0) return;

            Suggestions = results;
        }
    }

    /// <summary>
    /// The possible search types
    /// </summary>
    public enum SearchType
    {
        ImageSearch,
        VideoSearch,
    }
}