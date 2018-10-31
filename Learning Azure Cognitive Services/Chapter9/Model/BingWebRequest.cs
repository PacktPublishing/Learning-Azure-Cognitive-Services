using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace Chapter9.Model
{
    public class BingWebRequest
    {
        private const string JsonContentTypeHeader = "application/json";

        private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private HttpClient _httpClient;
        
        /// <summary>
        /// BingWebRequest constructor. Creates a new <see cref="HttpClient"/>, and adding a default header, for the API key
        /// </summary>
        /// <param name="apiKey">Bing Search API key, in string format</param>
        public BingWebRequest(string apiKey)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);
        }

        /// <summary>
        /// Function to make requests to the Bing Search API, using a given endpoint
        /// </summary>
        /// <typeparam name="TResponse">The response is expected to be deserialized into this object type</typeparam>
        /// <param name="url">Endpoint to use, with all query strings</param>
        /// <returns>Returns a deserialized object, specified in TResponse</returns>
        public async Task<TResponse> MakeRequest<TResponse>(string url)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                
                HttpResponseMessage response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = null;

                    if (response.Content != null)
                        responseContent = await response.Content.ReadAsStringAsync();
                    
                    if (!string.IsNullOrWhiteSpace(responseContent))
                        return JsonConvert.DeserializeObject<TResponse>(responseContent, _settings);

                    return default(TResponse);
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

            return default(TResponse);

        }
    }
}