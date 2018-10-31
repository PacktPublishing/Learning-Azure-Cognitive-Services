using End_to_End.Contracts;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace End_to_End.Model
{
    public class BingSearch
    {
        private BingWebRequest _webRequest;

        /// <summary>
        /// BingSearch constructor. Creates a new <see cref="BingWebRequest"/> with an API key
        /// </summary>
        public BingSearch()
        {
            _webRequest = new BingWebRequest("API_KEY_HERE");
        }

        /// <summary>
        /// Function to search for news within a specified category
        /// </summary>
        /// <param name="query">News category to search in</param>
        /// <returns><see cref="BingNewsResponse"/> object with the latest news in the given category</returns>
        public async Task<BingNewsResponse> SearchNewsCategory(string query)
        {
            string endpoint = string.Format("{0}{1}&mkt=en-US", "https://api.cognitive.microsoft.com/bing/v7.0/news?category=", query);

            try
            {
                BingNewsResponse response = await _webRequest.MakeRequest<BingNewsResponse>(endpoint);

                return response;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Function to search for the latest news based on a given query. Will filter news based on safe search parameter specified
        /// </summary>
        /// <param name="query">Query to search for</param>
        /// <param name="safeSearch"><see cref="SafeSearch"/> for the search</param>
        /// <returns><see cref="BingNewsResponse"/> object containing the 5 latests news</returns>
        public async Task<BingNewsResponse> SearchNews(string query, SafeSearch safeSearch)
        {
            string endpoint = string.Format("{0}{1}&safeSearch={2}&count=5&mkt=en-US", "https://api.cognitive.microsoft.com/bing/v7.0/news/search?q=", query, safeSearch.ToString());

            try
            {
                BingNewsResponse response = await _webRequest.MakeRequest<BingNewsResponse>(endpoint);

                return response;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Function to search the web for a given query
        /// </summary>
        /// <param name="query">Query to search for</param>
        /// <param name="safeSearch"><see cref="SafeSearch"/> parameter for the search</param>
        /// <returns><see cref="WebSearchResponse"/> object containing 5 search results</returns>
        public async Task<WebSearchResponse> SearchWeb(string query, SafeSearch safeSearch)
        {
            string endpoint = string.Format("{0}{1}&safeSearch={2}&count=5&mkt=en-US", "https://api.cognitive.microsoft.com/bing/v7.0/search?q=", query, safeSearch.ToString());

            try
            {
                WebSearchResponse response = await _webRequest.MakeRequest<WebSearchResponse>(endpoint);

                return response;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return null;
        }
    }
}