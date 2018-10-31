using System;
using System.Threading;
using System.Net.Http;
using System.Threading.Tasks;

namespace Chapter1.Model
{
    /// <summary>
    /// Class to generate authentication tokens to use for Bing Speech API
    /// </summary>
    public class Authentication : IDisposable
    {
        private string _clientSecret;
        private string _token;
        private Timer _tokenRenewer;

        private const int TokenRefreshInterval = 9;

        public string Token { get { return _token; } }

        /// <summary>
        /// Constructor - Sets the request details, gets a token and starts a timer for token renewal
        /// </summary>
        /// <param name="clientSecret">Bing Speech API key</param>
        public Authentication(string clientSecret)
        {
            _clientSecret = clientSecret;
        }

        public async Task Initialize()
        {
            _token = await GetToken();

            _tokenRenewer = new Timer(new TimerCallback(OnTokenExpiredCallback),
                                           this,
                                           TimeSpan.FromMinutes(TokenRefreshInterval),
                                           TimeSpan.FromMilliseconds(-1));
        }

        /// <summary>
        /// Function that executes when the timers refresh interval is reached. Will generate a new token
        /// </summary>
        /// <param name="stateInfo"></param>
        private async void OnTokenExpiredCallback(object stateInfo)
        {
            try
            {
                string newAccessToken = await GetToken();
                _token = newAccessToken;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Failed renewing access token. Details: {0}", ex.Message));
            }
            finally
            {
                _tokenRenewer.Change(TimeSpan.FromMinutes(TokenRefreshInterval), TimeSpan.FromMilliseconds(-1));
            }
        }

        /// <summary>
        /// Function to generate an application token
        /// </summary>
        /// <returns>Returns the <see cref="string"/> token if call is successful, null otherwise </returns>
        private async Task<string> GetToken()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _clientSecret);
                UriBuilder uriBuilder = new UriBuilder("https://api.cognitive.microsoft.com/sts/v1.0");
                uriBuilder.Path += "/issueToken";

                var result = await client.PostAsync(uriBuilder.Uri.AbsoluteUri, null);
                return await result.Content.ReadAsStringAsync();
            }
        }

        public void Dispose()
        {            
            _tokenRenewer.Dispose();
        }
    }
}