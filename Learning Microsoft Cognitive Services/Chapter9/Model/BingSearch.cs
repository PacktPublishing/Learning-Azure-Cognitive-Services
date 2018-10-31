using Chapter9.Contracts;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Chapter9.Model
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
        /// Function to search for images. Uses an endpoint for image search
        /// </summary>
        /// <param name="query">The query to search for</param>
        /// <returns>The <see cref="ImageSearchResponse"/>, containing image details</returns>
        public async Task<ImageSearchResponse> SearchImages(string query)
        {
            string endpoint = string.Format("{0}{1}", "https://api.cognitive.microsoft.com/bing/v7.0/images/search?q=", query);

            try
            {
                ImageSearchResponse response = await _webRequest.MakeRequest<ImageSearchResponse>(endpoint);

                return response;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return null;
        }
        
        /// <summary>
        /// Function to search for videos. Uses an endpoint for video search
        /// </summary>
        /// <param name="query">The query to search for</param>
        /// <returns>The <see cref="VideoSearchResponse"/>, containing video details</returns>
        public async Task<VideoSearchResponse> SearchVideos(string query)
        {
            string endpoint = string.Format("{0}{1}", "https://api.cognitive.microsoft.com/bing/v7.0/videos/search?q=", query);

            try
            {
                VideoSearchResponse response = await _webRequest.MakeRequest<VideoSearchResponse>(endpoint);

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