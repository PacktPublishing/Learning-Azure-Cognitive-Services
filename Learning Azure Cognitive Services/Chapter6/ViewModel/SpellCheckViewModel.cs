using Chapter6.Contracts;
using Chapter6.Interface;
using Chapter6.Model;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Windows.Input;

namespace Chapter6.ViewModel
{
    public class SpellCheckViewModel : ObservableObject
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

        private string _preContext;
        public string PreContext
        {
            get { return _preContext; }
            set
            {
                _preContext = value;
                RaisePropertyChangedEvent("PreContext");
            }
        }

        private string _postContext;
        public string PostContext
        {
            get { return _postContext; }
            set
            {
                _postContext = value;
                RaisePropertyChangedEvent("PostContext");
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

        public ICommand ExecuteOperationCommand { get; private set; }

        /// <summary>
        /// SpellCheckViewModel constructor. Creates the <see cref="WebRequest"/> object and the command object
        /// </summary>
        public SpellCheckViewModel()
        {
            _webRequest = new WebRequest("ROOT_URI", "API_KEY");
            ExecuteOperationCommand = new DelegateCommand(ExecuteOperation, CanExecuteOperation);
        }

        /// <summary>
        /// Determines if commands can be executed
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>True if the input query is entered, false otherwise</returns>
        private bool CanExecuteOperation(object obj)
        {
            return !string.IsNullOrEmpty(InputQuery);
        }

        /// <summary>
        /// Command function to execute the spell check operation.
        /// </summary>
        /// <param name="obj"></param>
        private async void ExecuteOperation(object obj)
        {
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            queryString["text"] = InputQuery;
            queryString["mkt"] = "en-us";
            //queryString["mode"] = "proof";

            if (!string.IsNullOrEmpty(PreContext))
                queryString["preContextText"] = PreContext;

            if(!string.IsNullOrEmpty(PostContext))
                queryString["postContextText"] = PostContext;
            
            SpellCheckResponse response = await _webRequest.MakeRequest<object, SpellCheckResponse>(HttpMethod.Get, queryString.ToString());
            ParseResults(response);
        }

        /// <summary>
        /// Function to parse a given spell check result. 
        /// Makes sure all is displayed in the UI
        /// </summary>
        /// <param name="response"><see cref="SpellCheckResponse"/></param>
        private void ParseResults(SpellCheckResponse response)
        {
            if(response == null || response.flaggedTokens == null || response.flaggedTokens.Count == 0)
            {
                Result = "No suggestions found";
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("Spell checking results:\n\n");
            
            foreach (FlaggedTokens tokens in response.flaggedTokens)
            {
                if (!string.IsNullOrEmpty(tokens.token))
                    sb.AppendFormat("Token is: {0}\n", tokens.token);

                if(tokens.suggestions != null || tokens.suggestions.Count != 0)
                {
                    foreach (Suggestions suggestion in tokens.suggestions)
                    {
                        sb.AppendFormat("Suggestion: {0} - with score: {1}\n", suggestion.suggestion, suggestion.score);
                    }

                    sb.Append("\n");
                }
            }
            
            Result = sb.ToString();
        }
    }
}