using End_to_End.Interface;
using End_to_End.Model;
using Microsoft.Cognitive.LUIS;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System;
using System.Linq;
using System.Media;
using System.Diagnostics;
using System.Threading;
using End_to_End.Contracts;
using Microsoft.ProjectOxford.Vision;
using System.Collections.Generic;
using Microsoft.ProjectOxford.Vision.Contract;

namespace End_to_End.ViewModel
{
    public class LuisViewModel : ObservableObject, IDisposable
    {
        private Luis _luis;
        private SpeechToText _sttClient;
        private TextToSpeech _ttsClient;
        private BingSearch _bingSearch;
        private IVisionServiceClient _visionClient;
        
        private bool _requiresResponse = false;
        private LuisResult _lastResult = null;

        private string _bingApiKey = "BING_SPEECH_API_KEY";

        private string _inputText;
        public string InputText
        {
            get { return _inputText; }
            set
            {
                _inputText = value;
                RaisePropertyChangedEvent("InputText");
            }
        }

        private string _resultText;
        public string ResultText
        {
            get { return _resultText; }
            set
            {
                _resultText = value;
                RaisePropertyChangedEvent("ResultText");
            }
        }

        public ICommand RecordUtteranceCommand { get; private set; }
        public ICommand ExecuteUtteranceCommand { get; private set; }

        /// <summary>
        /// LuisViewModel constructor.
        /// Creates vision client, luis client, speech-to-text and text-to-speech clients, as well as ICommand objects
        /// </summary>
        public LuisViewModel()
        {
            _bingSearch = new BingSearch();

            _visionClient = new VisionServiceClient("FACE_API_KEY", "ROOT_URI");

            _luis = new Luis(new LuisClient("LUIS_APP_ID", "LUIS_API_KEY"));
            _luis.OnLuisUtteranceResultUpdated += OnLuisUtteranceResultUpdated;

            _sttClient = new SpeechToText(_bingApiKey);
            _sttClient.OnSttStatusUpdated += OnSttStatusUpdated;

            _ttsClient = new TextToSpeech();
            _ttsClient.OnAudioAvailable += OnTtsAudioAvailable;
            _ttsClient.OnError += OnTtsError;
            GenerateHeaders();

            RecordUtteranceCommand = new DelegateCommand(RecordUtterance);
            ExecuteUtteranceCommand = new DelegateCommand(ExecuteUtterance, CanExecuteUtterance);
        }

        private async void GenerateHeaders()
        {
            if (await _ttsClient.GenerateAuthenticationToken(_bingApiKey))
                _ttsClient.GenerateHeaders();
        }

        /// <summary>
        /// Command function to start recording of LUIS commands, using a microphone
        /// </summary>
        /// <param name="obj"></param>
        private void RecordUtterance(object obj)
        {
            _sttClient.StartMicToText();
        }

        /// <summary>
        /// Function to determine if a LUIS call can be made
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>True if input text is entered, false otherwise</returns>
        private bool CanExecuteUtterance(object obj)
        {
            return !string.IsNullOrEmpty(InputText);
        }

        /// <summary>
        /// Command function to execute LUIS requests
        /// </summary>
        /// <param name="obj"></param>
        private void ExecuteUtterance(object obj)
        {
            CallLuis(InputText);
        }

        /// <summary>
        /// Function to call LUIS. Will either call for a request or response, depending on if a request has previously been made or not
        /// </summary>
        /// <param name="input">Query input for LUIS</param>
        private async void CallLuis(string input)
        {
            if (!_requiresResponse)
            {
                await _luis.RequestAsync(input);
            }
            else
            {
                await _luis.ReplyAsync(_lastResult, input);
                _requiresResponse = false;
            }
        }

        /// <summary>
        /// Event handler function for <see cref="TextToSpeech"/> audio events. Will play audio streams through the speakers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTtsAudioAvailable(object sender, AudioEventArgs e)
        {
            SoundPlayer player = new SoundPlayer(e.EventData);
            player.Play();
            e.EventData.Dispose();
        }

        /// <summary>
        /// Event handler function for <see cref="TextToSpeech"/> errors.
        /// Prints errors to the debug console
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTtsError(object sender, AudioErrorEventArgs e)
        {
            Debug.WriteLine($"Status: Audio service failed -  {e.ErrorMessage}");
        }

        /// <summary>
        /// Event handler function for <see cref="SpeechToText"/> client updates
        /// Displays all results in the UI, and calls LUIS if a complete sentence have been converted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSttStatusUpdated(object sender, SpeechToTextEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => 
            {
                StringBuilder sb = new StringBuilder();

                if(e.Status == SttStatus.Success)
                {
                    if(!string.IsNullOrEmpty(e.Message))
                    {
                        sb.AppendFormat("Result message: {0}\n\n", e.Message);
                    }

                    if(e.Results != null && e.Results.Count != 0)
                    {
                        sb.Append("Retrieved the following results:\n");
                        foreach(string sentence in e.Results)
                        {
                            sb.AppendFormat("{0}\n\n", sentence);
                        }

                        sb.Append("Calling LUIS with the top result\n");

                        CallLuis(e.Results.FirstOrDefault());
                    }
                }
                else
                {
                    sb.AppendFormat("Could not convert speech to text: {0}\n", e.Message);
                }

                sb.Append("\n");

                ResultText = sb.ToString();
            });
        }

        /// <summary>
        /// Event handler function for LUIS results updates. Will print the results to the UI.
        /// If the results requires a response, the application will ask for it through the speakers, and 
        /// wait for the response
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLuisUtteranceResultUpdated(object sender, LuisUtteranceResultEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                StringBuilder sb = new StringBuilder(ResultText);
                
                _requiresResponse = e.RequiresReply;

                sb.AppendFormat("Status: {0}\n", e.Status);
                sb.AppendFormat("Summary: {0}\n\n", e.Message);
                
                if (!string.IsNullOrEmpty(e.IntentName))
                    await TriggerActionExectution(e.IntentName, e.EntityName);

                ResultText = sb.ToString();
            });
        }

        /// <summary>
        /// Function to execute a function based on the action triggered
        /// </summary>
        /// <param name="actionName">The triggered LUIS action</param>
        /// <param name="actionValue">The LUIS action parameter provided</param>
        /// <returns></returns>
        private async Task TriggerActionExectution(string intentName, string entity)
        {
            LuisActions action;

            if (!Enum.TryParse(intentName, true, out action)) return;

            switch(action)
            {
                case LuisActions.GetRoomTemperature:
                case LuisActions.SetRoomTemperature:
                case LuisActions.None:
                default:
                    break;
                case LuisActions.GetNews:
                    await GetLatestNews(entity);
                    break;
            }
        }

        /// <summary>
        /// Function to retrieve the latests news. With a successful response, it will call ParseNews with the first result
        /// </summary>
        /// <param name="queryString">The query to search for news with</param>
        /// <returns></returns>
        private async Task GetLatestNews(string queryString)
        {
            BingNewsResponse news = await _bingSearch.SearchNews(queryString, SafeSearch.Moderate);

            if (news != null && (news.value == null || news.value.Length == 0)) return;
            
            await ParseNews(news.value[0]);
        }

        /// <summary>
        /// Function to parse news articles. Will speak the news out on speakers, using the <see cref="TextToSpeech"/> client
        /// </summary>
        /// <param name="newsArticle"></param>
        /// <returns></returns>
        private async Task ParseNews(Value newsArticle)
        {
            string imageDescription = await GetImageDescription(newsArticle.image.thumbnail.contentUrl);

            string articleDescription = $"{newsArticle.name}, published {newsArticle.datePublished}. Description: {newsArticle.description}. Corresponding image is {imageDescription}";

            await _ttsClient.SpeakAsync(articleDescription, CancellationToken.None);
        }

        /// <summary>
        /// Function to get an image description based on a content URL for an image
        /// </summary>
        /// <param name="contentUrl">The URL for the image</param>
        /// <returns>A string with image description, or "none" if it couldn't be found</returns>
        private async Task<string> GetImageDescription(string contentUrl)
        {
            try
            {
                AnalysisResult imageAnalysisResult = await _visionClient.AnalyzeImageAsync(contentUrl, new List<VisualFeature>() { VisualFeature.Description });

                if (imageAnalysisResult == null || imageAnalysisResult.Description?.Captions?.Length == 0) return "none";

                return imageAnalysisResult.Description.Captions.First().Text;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return "none";
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
                _sttClient.Dispose();
        }
    }
}