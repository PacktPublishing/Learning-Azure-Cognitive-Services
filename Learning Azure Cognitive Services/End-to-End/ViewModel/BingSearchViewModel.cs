using End_to_End.Contracts;
using End_to_End.Interface;
using End_to_End.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace End_to_End.ViewModel
{
    public class BingSearchViewModel : ObservableObject
    {
        private BingSearch _bingSearch;

        public IEnumerable<BingSearchType> AvailableSearchTypes { get { return Enum.GetValues(typeof(BingSearchType)).Cast<BingSearchType>(); } }

        private BingSearchType _selectedSearchType;
        public BingSearchType SelectedSearchType
        {
            get { return _selectedSearchType; }
            set
            {
                _selectedSearchType = value;
                RaisePropertyChangedEvent("SelectedSearchType");
            }
        }

        public IEnumerable<SafeSearch> SafeSearchFilter { get { return Enum.GetValues(typeof(SafeSearch)).Cast<SafeSearch>(); } }

        private SafeSearch _selectedSafeSearchFilter;
        public SafeSearch SelectedSafeSearchFilter
        {
            get { return _selectedSafeSearchFilter; }
            set
            {
                _selectedSafeSearchFilter = value;
                RaisePropertyChangedEvent("SelectedSafeSearchFilter");
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
            }
        }

        private string _searchResults;
        public string SearchResults
        {
            get { return _searchResults; }
            set
            {
                _searchResults = value;
                RaisePropertyChangedEvent("SearchResults");
            }
        }

        public ICommand SearchCommand { get; private set; }

        /// <summary>
        /// BingSearchViewModel constructor.
        /// Creates a new <see cref="BingSearch"/> object and an ICommand object to execute the search
        /// </summary>
        public BingSearchViewModel()
        {
            _bingSearch = new BingSearch();

            SearchCommand = new DelegateCommand(Search, CanSearch);
        }

        /// <summary>
        /// Function to determine if a search can be executed
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>True if input query is entered, false otherwise</returns>
        private bool CanSearch(object obj)
        {
            return !string.IsNullOrEmpty(SearchQuery);
        }

        /// <summary>
        /// Command function to execute a search. Chooses a search based on the selected search type
        /// </summary>
        /// <param name="obj"></param>
        private async void Search(object obj)
        {
            switch (SelectedSearchType)
            {
                case BingSearchType.News:
                    var newsResponse = await _bingSearch.SearchNews(SearchQuery, SelectedSafeSearchFilter);
                    ParseNewsResponse(newsResponse as BingNewsResponse);
                    break;
                case BingSearchType.NewsCategory:
                    var categoryResponse = await _bingSearch.SearchNewsCategory(SearchQuery);
                    ParseNewsResponse(categoryResponse as BingNewsResponse);
                    break;
                case BingSearchType.Web:
                    var webResponse = await _bingSearch.SearchWeb(SearchQuery, SelectedSafeSearchFilter);
                    ParseWebSearchResponse(webResponse as WebSearchResponse);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Function to parse news search responses
        /// Displays news article name, publish date and a short description in the UI
        /// </summary>
        /// <param name="bingNewsResponse"></param>
        private void ParseNewsResponse(BingNewsResponse bingNewsResponse)
        {
            StringBuilder sb = new StringBuilder();

            foreach(Value news in bingNewsResponse.value)
            {
                sb.AppendFormat("{0}\n", news.name);
                sb.AppendFormat("Published: {0}\n", news.datePublished);
                sb.AppendFormat("{0}\n\n", news.description);
            }

            SearchResults = sb.ToString();
        }

        /// <summary>
        /// Function to parse web search responses.
        /// Displays website name, URL and a short snippet in the UI
        /// </summary>
        /// <param name="webSearchResponse"></param>
        private void ParseWebSearchResponse(WebSearchResponse webSearchResponse)
        {
            StringBuilder sb = new StringBuilder();

            Webpages webPages = webSearchResponse.webPages;

            foreach (WebValue website in webPages.value)
            {
                sb.AppendFormat("{0}\n", website.name);
                sb.AppendFormat("URL: {0}\n", website.displayUrl);
                sb.AppendFormat("About: {0}\n\n", website.snippet);
            }

            SearchResults = sb.ToString();
        }
    }
}