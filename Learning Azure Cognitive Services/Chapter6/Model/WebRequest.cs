using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Chapter6.Model
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
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);
        }

        /// <summary>
        /// Function to make a web request
        /// </summary>
        /// <typeparam name="TRequest">If any request body is required, it's object type should be specified</typeparam>
        /// <typeparam name="TResponse">The response is expected to be deserialized into this object type</typeparam>
        /// <param name="method">The <see cref="HttpMethod"/> to be used</param>
        /// <param name="queryString">The query string</param>
        /// <param name="requestBody">Request body, not required</param>
        /// <returns>Returns a deserialized object, specified in TResponse</returns>
        public async Task<TResponse> MakeRequest<TRequest, TResponse>(HttpMethod method, string queryString, TRequest requestBody = default(TRequest))
        {
            try
            {
                string url = $"{_endpoint}{queryString}";
                var request = new HttpRequestMessage(method, url);

                if (requestBody != null)
                    request.Content = new StringContent(JsonConvert.SerializeObject(requestBody, _settings), Encoding.UTF8, JsonContentTypeHeader);

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