using Chapter9.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Chapter9.Model
{
    public class BingAutoSuggest
    {
        private BingWebRequest _webRequest;

        /// <summary>
        /// BingAutoSuggest constructor. Creates a new <see cref="BingWebRequest"/> object
        /// </summary>
        public BingAutoSuggest()
        {
            _webRequest = new BingWebRequest("API_KEY_HERE");
        }

        /// <summary>
        /// Function to get autosuggestions, based on given queries. Uses the autosuggest endpoint
        /// </summary>
        /// <param name="query">Query string to generate auto suggestions from</param>
        /// <returns>List of suggested words</returns>
        public async Task<List<string>> Suggest(string query)
        {
            string endpoint = string.Format("{0}{1}&mkt=en-US", "https://api.cognitive.microsoft.com/bing/v7.0/suggestions/?q=", query);

            List<string> suggestionResult = new List<string>();

            try
            {
                BingAutoSuggestResponse response = await _webRequest.MakeRequest<BingAutoSuggestResponse>(endpoint);

                if (response == null || response.suggestionGroups.Length == 0) return suggestionResult;
                
                foreach(Suggestiongroup suggestionGroup in response.suggestionGroups)
                {
                    foreach(Searchsuggestion suggestion in suggestionGroup.searchSuggestions)
                    {
                        suggestionResult.Add(suggestion.displayText);
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return suggestionResult;
        }
    }
}