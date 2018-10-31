using Chapter6.Contracts;
using Chapter6.Interface;
using Chapter6.Model;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Windows.Input;

namespace Chapter6.ViewModel
{
    public class TextAnalysisViewModel : ObservableObject
    {
        private WebRequest _webRequest;

        private string _inputQuery;
        public string InputQuery
        {
            get { return _inputQuery; }
            set
            {
                _inputQuery = value;
                RaisePropertyChangedEvent("InputQuery");
            }
        }

        private string _result;
        public string Result
        {
            get { return _result; }
            set
            {
                _result = value;
                RaisePropertyChangedEvent("Result");
            }
        }

        public ICommand DetectLanguageCommand { get; private set; }
        public ICommand DetectKeyPhrasesCommand { get; private set; }
        public ICommand DetectSentimentCommand { get; private set; }

        /// <summary>
        /// TextAnalysisViewModel constructor. Creates a new <see cref="WebRequest"/> object and command objects
        /// </summary>
        public TextAnalysisViewModel()
        {
            _webRequest = new WebRequest("ROOT_URI", "API_KEY");
            DetectLanguageCommand = new DelegateCommand(DetectLanguage, CanExecuteOperation);
            DetectKeyPhrasesCommand = new DelegateCommand(DetectKeyPhrases, CanExecuteOperation);
            DetectSentimentCommand = new DelegateCommand(DetectSentiment, CanExecuteOperation);
        }

        /// <summary>
        /// Determines if operations can execute
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>True if the input query is filled in, false otherwise</returns>
        private bool CanExecuteOperation(object obj)
        {
            return !string.IsNullOrEmpty(InputQuery);
        }

        /// <summary>
        /// Command function to detect the language of a given text
        /// Prints the result to the UI
        /// </summary>
        /// <param name="obj"></param>
        private async void DetectLanguage(object obj)
        {
            var queryString = HttpUtility.ParseQueryString("languages");
            TextRequests request = new TextRequests
            {
                documents = new List<TextDocumentRequest>
                {
                    new TextDocumentRequest { id = "FirstId", text = InputQuery }                    
                }
            };

            TextResponse response = await _webRequest.MakeRequest<TextRequests, TextResponse>(HttpMethod.Post, queryString.ToString(), request);

            if(response.documents == null || response.documents.Count == 0)
            {
                Result = "No languages was detected.";
                return;
            }

            StringBuilder sb = new StringBuilder();

            foreach (TextLanguageDocuments document in response.documents)
            {
                foreach (TextDetectedLanguages detectedLanguage in document.detectedLanguages)
                {
                    sb.AppendFormat("Detected language: {0} with score {1}\n", detectedLanguage.name, detectedLanguage.score);
                }
            }

            Result = sb.ToString();
        }

        /// <summary>
        /// Command function to detect the sentiment of a given text
        /// Prints the result to the UI
        /// </summary>
        /// <param name="obj"></param>
        private async void DetectSentiment(object obj)
        {
            var queryString = HttpUtility.ParseQueryString("sentiment");
            TextRequests request = new TextRequests
            {
                documents = new List<TextDocumentRequest>
                {
                    new TextDocumentRequest { id = "FirstId", text = InputQuery, language = "en" }
                }
            };

            TextSentimentResponse response = await _webRequest.MakeRequest<TextRequests, TextSentimentResponse>(HttpMethod.Post, queryString.ToString(), request);

            if(response.documents == null || response.documents?.Count == 0)
            {
                Result = "No sentiments detected";
                return;
            }

            StringBuilder sb = new StringBuilder();

            foreach (TextSentimentDocuments document in response.documents)
            {
                sb.AppendFormat("Document ID: {0}\n", document.id);

                if (document.score >= 0.5)
                    sb.AppendFormat("Sentiment is positive, with a score of {0}\n", document.score);
                else
                    sb.AppendFormat("Sentiment is negative with a score of {0}\n", document.score);
            }

            Result = sb.ToString();
        }

        /// <summary>
        /// Command function to detect key phrases in text.
        /// Will print the result to the UI
        /// </summary>
        /// <param name="obj"></param>
        private async void DetectKeyPhrases(object obj)
        {
            var queryString = HttpUtility.ParseQueryString("keyPhrases");
            TextRequests request = new TextRequests
            {
                documents = new List<TextDocumentRequest>
                {
                    new TextDocumentRequest { id = "FirstId", text = InputQuery, language = "en" }
                }
            };

            TextKeyPhrasesResponse response = await _webRequest.MakeRequest<TextRequests, TextKeyPhrasesResponse>(HttpMethod.Post, queryString.ToString(), request);

            if (response.documents == null || response.documents?.Count == 0)
            {
                Result = "No key phrases found.";
                return;
            }

            StringBuilder sb = new StringBuilder();
            
            foreach (TextKeyPhrasesDocuments document in response.documents)
            {
                sb.Append("Key phrases found:\n");
                foreach (string phrase in document.keyPhrases)
                {
                    sb.AppendFormat("{0}\n", phrase);
                }
            }

            Result = sb.ToString();
        }
    }
}