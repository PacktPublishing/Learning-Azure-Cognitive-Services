using Chapter7.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Chapter7.Model
{
    public class WebRequest
    {
        private const string JsonContentTypeHeader = "application/json";

        private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private HttpClient _httpClient;

        private string _endpoint;

        /// <summary>
        /// WebRequest constructor, creates a new HttpClient
        /// </summary>
        /// <param name="uri">The URI for the given API endpoint</param>
        /// <param name="apiKey">The API key</param>
        public WebRequest(string uri, string apiKey)
        {
            _endpoint = uri;
            
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
        }

        /// <summary>
        /// Function to make a request to recommend items
        /// </summary>
        /// <param name="method">The <see cref="HttpMethod"/> to be used</param>
        /// <param name="queryString">The query string</param>
        /// <returns>Returns a list of recommended items</returns>
        public async Task<List<RecommendedItem>> RecommendItem(HttpMethod method, string queryString)
        {
            try
            {
                string url = $"{_endpoint}{queryString}";
                var request = new HttpRequestMessage(method, url);

                HttpResponseMessage response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = null;

                    if (response.Content != null)
                        responseContent = await response.Content.ReadAsStringAsync();

                    if (!string.IsNullOrWhiteSpace(responseContent))
                        return JsonConvert.DeserializeObject<List<RecommendedItem>>(responseContent, _settings);

                    return new List<RecommendedItem>();
                }
                else
                {
                    if (response.Content != null && response.Content.Headers.ContentType.MediaType.Contains(JsonContentTypeHeader))
                    {
                        var errorObjectString = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine(errorObjectString);
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return new List<RecommendedItem>();
        }

        /// <summary>
        /// Function to make a request to get a list of recommendation models
        /// </summary>
        /// <param name="method">The <see cref="HttpMethod"/> to be used</param>
        /// <returns>Returns a list of recommendation models</returns>
        public async Task<List<RecommandationModel>> GetModels(HttpMethod method)
        {
            try
            {
                var request = new HttpRequestMessage(method, _endpoint);
                
                HttpResponseMessage response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = null;

                    if (response.Content != null)
                        responseContent = await response.Content.ReadAsStringAsync();

                    if (!string.IsNullOrWhiteSpace(responseContent))
                        return JsonConvert.DeserializeObject<List<RecommandationModel>>(responseContent, _settings);

                    return new List<RecommandationModel>();
                }
                else
                {
                    if (response.Content != null && response.Content.Headers.ContentType.MediaType.Contains(JsonContentTypeHeader))
                    {
                        var errorObjectString = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine(errorObjectString);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return new List<RecommandationModel>();
        }
    }
}