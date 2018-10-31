using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter1.Model
{
    /// <summary>
    /// Class to execute text-to-speech conversions
    /// </summary>
    public class TextToSpeak
    {
        public event EventHandler<AudioEventArgs> OnAudioAvailable;
        public event EventHandler<AudioErrorEventArgs> OnError;
        
        private string _gender;
        private string _voiceName;
        private string _outputFormat;
        private string _authorizationToken;
        private string _token; 

        private List<KeyValuePair<string, string>> _headers = new List<KeyValuePair<string, string>>();

        private const string RequestUri = "https://speech.platform.bing.com/synthesize";
        private const string SsmlTemplate = "<speak version='1.0' xml:lang='en-US'><voice xml:lang='en-US' xml:gender='{0}' name='{1}'>{2}</voice></speak>";

        /// <summary>
        /// Constructor sets a few default parameters
        /// </summary>
        public TextToSpeak()
        {
            _gender = "Female";
            _outputFormat = "riff-16khz-16bit-mono-pcm";
            _voiceName = "Microsoft Server Speech Text to Speech Voice (en-US, ZiraRUS)";
        }

        /// <summary>
        /// Function in charge of generating the authentication token
        /// </summary>
        /// <param name="clientSecret">Bing Speech API key</param>
        /// <returns>Returns true if a token is generated, false otherwise</returns>
        public async Task<bool> GenerateAuthenticationToken(string clientSecret)
        {
            Authentication auth = new Authentication(clientSecret);

            try
            {
                await auth.Initialize();
                _token = auth.Token;

                if (_token != null)
                {
                    _authorizationToken = $"Bearer {_token}";

                    return true;
                }
                else
                {
                    RaiseOnError(new AudioErrorEventArgs("Failed to generate authentication token."));
                    return false;
                }
            }
            catch(Exception ex)
            {
                RaiseOnError(new AudioErrorEventArgs($"Failed to generate authentication token - {ex.Message}"));

                return false;
            }
        }

        /// <summary>
        /// Function to generate a few default headers. Authentication token goes here, as well as application type and output format
        /// </summary>
        public void GenerateHeaders()
        {
            _headers.Add(new KeyValuePair<string, string>("Content-Type", "application/ssml+xml"));
            _headers.Add(new KeyValuePair<string, string>("X-Microsoft-OutputFormat", _outputFormat));
            _headers.Add(new KeyValuePair<string, string>("Authorization", _authorizationToken));
            _headers.Add(new KeyValuePair<string, string>("X-Search-AppId", Guid.NewGuid().ToString("N")));
            _headers.Add(new KeyValuePair<string, string>("X-Search-ClientID", Guid.NewGuid().ToString("N")));
            _headers.Add(new KeyValuePair<string, string>("User-Agent", "Chapter1"));
        }

        /// <summary>
        /// Function to execute text-to-speech conversion. Will raise an event with audio if call is successful
        /// </summary>
        /// <param name="textToSpeak">Text string to be converted to spoken audio</param>
        /// <param name="cancellationToken">Task cancellation token</param>
        /// <returns>Returns a Task</returns>
        public Task SpeakAsync(string textToSpeak, CancellationToken cancellationToken)
        {
            var cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler() { CookieContainer = cookieContainer };
            var client = new HttpClient(handler);

            foreach(var header in _headers)
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }

            var request = new HttpRequestMessage(HttpMethod.Post, RequestUri)
            {
                Content = new StringContent(string.Format(SsmlTemplate, _gender, _voiceName, textToSpeak))
            };

            var httpTask = client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            var saveTask = httpTask.ContinueWith(
                async (responseMessage, token) =>
                {
                    try
                    {
                        if(responseMessage.IsCompleted && responseMessage.Result != null && responseMessage.Result.IsSuccessStatusCode)
                        {
                            var httpStream = await responseMessage.Result.Content.ReadAsStreamAsync().ConfigureAwait(false);
                            RaiseOnAudioAvailable(new AudioEventArgs(httpStream));
                        }
                        else
                        {
                            RaiseOnError(new AudioErrorEventArgs($"Service returned {responseMessage.Result.StatusCode}"));
                        }
                    }
                    catch(Exception e)
                    {
                        RaiseOnError(new AudioErrorEventArgs(e.GetBaseException().Message));
                    }
                    finally
                    {
                        responseMessage.Dispose();
                        request.Dispose();
                        client.Dispose();
                        handler.Dispose();
                    }
                }, TaskContinuationOptions.AttachedToParent, cancellationToken);
            
            return saveTask;
        }

        /// <summary>
        /// Function to raise <see cref="OnAudioAvailable"/> event
        /// </summary>
        /// <param name="args"><see cref="AudioEventArgs"/> containing event data as a Stream</param>
        private void RaiseOnAudioAvailable(AudioEventArgs args)
        {
            OnAudioAvailable?.Invoke(this, args);
        }

        /// <summary>
        /// Function to raise <see cref="OnError"/> event
        /// </summary>
        /// <param name="args"><see cref="AudioErrorEventArgs"/> containing an error message</param>
        private void RaiseOnError(AudioErrorEventArgs args)
        {
            OnError?.Invoke(this, args);
        }
    }

    /// <summary>
    /// EventArgs class for available audio events
    /// </summary>
    public class AudioEventArgs : EventArgs
    {
        public AudioEventArgs(Stream eventData)
        {
            EventData = eventData;
        }

        public Stream EventData { get; private set; } 
    }

    /// <summary>
    /// EventArgs class for error events
    /// </summary>
    public class AudioErrorEventArgs : EventArgs
    {
        public AudioErrorEventArgs(string message)
        {
            ErrorMessage = message;
        }

        public string ErrorMessage { get; private set; }
    }
}