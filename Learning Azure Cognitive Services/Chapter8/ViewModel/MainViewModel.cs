using Chapter8.Contracts;
using Chapter8.Interface;
using Chapter8.Model;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Windows.Input;

namespace Chapter8.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        private WebRequest _webRequest;

        private string _endpoint;
        public string Endpoint
        {
            get { return _endpoint; }
            set
            {
                _endpoint = value;
                RaisePropertyChangedEvent("Endpoint");
                _webRequest?.SetEndpoint(value);
            }
        }
        
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

        private string _queryExpression;
        public string QueryExpression
        {
            get { return _queryExpression; }
            set
            {
                _queryExpression = value;
                RaisePropertyChangedEvent("QueryExpression");
            }
        }

        private string _results;
        public string Results
        {
            get { return _results; }
            set
            {
                _results = value;
                RaisePropertyChangedEvent("Results");
            }
        }

        private ObservableCollection<string> _availableQueryExpressions = new ObservableCollection<string>();
        public ObservableCollection<string> AvailableQueryExpressions
        {
            get { return _availableQueryExpressions; }
            set
            {
                _availableQueryExpressions = value;
                RaisePropertyChangedEvent("AvailableQueryExpressions");
            }
        }

        public ICommand InterpretCommand { get; private set; }
        public ICommand EvaluateCommand { get; private set; }
        public ICommand CalculateHistogramCommand { get; private set; }

        /// <summary>
        /// MainViewModel constructor, which creates a <see cref="WebRequest"/> object and command properties
        /// </summary>
        public MainViewModel()
        {
            Endpoint = "https://api.labs.cognitive.microsoft.com/academic/v1.0/";
            _webRequest = new WebRequest(Endpoint, "API_KEY");

            InterpretCommand = new DelegateCommand(Interpret, CanInterpret);
            EvaluateCommand = new DelegateCommand(Evaluate, CanExecuteCommands);
            CalculateHistogramCommand = new DelegateCommand(CalculateHistogram, CanExecuteCommands);
        }

        /// <summary>
        /// Function to determine if interpretation can be executed
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>Returns true if input query is entered, false otherwise</returns>
        private bool CanInterpret(object obj)
        {
            return !string.IsNullOrEmpty(InputQuery);
        }

        /// <summary>
        /// Command function for interpretations. Constructs a query string and makes a web request
        /// Will get an <see cref="InterpretResponse"/> object, which is used to get a list of available interpretations
        /// </summary>
        /// <param name="obj"></param>
        private async void Interpret(object obj)
        {
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            queryString["query"] = InputQuery;
            queryString["complete"] = "1";
            //queryString["count"] = "10";
            //queryString["offset"] = "0";
            //queryString["timeout"] = "1000";
            //queryString["model"] = "latest";

            InterpretResponse response = await _webRequest.MakeRequest<object, InterpretResponse>(HttpMethod.Get, $"interpret?{queryString.ToString()}");

            if (response == null || response.interpretations.Length == 0) return;

            ObservableCollection<string> tempList = new ObservableCollection<string>();

            foreach (Interpretation interpretation in response.interpretations)
            {
                foreach (Rule rule in interpretation.rules)
                {
                    tempList.Add(rule.output.value);
                }
            }

            AvailableQueryExpressions = tempList;
            QueryExpression = AvailableQueryExpressions.FirstOrDefault();
        }

        /// <summary>
        /// Function to determine if evaluate or calculate histogram operations can be executed
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>True if a query expression is selected, false otherwise</returns>
        private bool CanExecuteCommands(object obj)
        {
            return !string.IsNullOrEmpty(QueryExpression);
        }

        /// <summary>
        /// Command function for evaluate operation. Constructs a query string and makes a call to the API
        /// If successful, it will get a <see cref="EvaluateResponse"/> in response, which will be parsed
        /// and displayed in the UI
        /// </summary>
        /// <param name="obj"></param>
        private async void Evaluate(object obj)
        {
            string queryString = $"expr={QueryExpression}&attributes=Id,Ti,Y,D,CC,AA.AuN";

            //queryString += "&model=latest";
            //queryString += "&count=10";
            //queryString += "&offset=0";5
            //queryString += "&orderby=name:asc";

            EvaluateResponse response = await _webRequest.MakeRequest<object, EvaluateResponse>(HttpMethod.Get, $"evaluate?{queryString}");

            if (response == null || response.entities.Length == 0) return;

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("Expression {0} returned {1} entities\n\n", response.expr, response.entities.Length);

            foreach (Entity entity in response.entities)
            {
                sb.AppendFormat("Paper title: {0}\n\tDate: {1}\n", entity.Ti, entity.D);

                sb.Append("Authors:\n");
                foreach (AA author in entity.AA)
                {
                    sb.AppendFormat("\t{0}\n", author.AuN);
                }

                sb.Append("\n");
            }

            Results = sb.ToString();
        }

        /// <summary>
        /// Command function for calculating histogram. Constructs a query string and makes a call to the API
        /// If successful, it will get a <see cref="HistogramResponse"/> in response, which will be parsed
        /// and displayed in the UI.
        /// </summary>
        /// <param name="obj"></param>
        private async void CalculateHistogram(object obj)
        {
            string queryString = $"expr={QueryExpression}&attributes=Y,F.FN";

            //queryString += "&model=latest";
            //queryString += "&count=10";
            //queryString += "&offset=0";

            HistogramResponse response = await _webRequest.MakeRequest<object, HistogramResponse>(HttpMethod.Get, $"calchistogram?{queryString}");

            if (response == null || response.histograms.Length == 0) return;

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("Totalt number of matching entities: {0}\n", response.num_entities);

            foreach (Histogram histogram in response.histograms)
            {
                sb.AppendFormat("Attribute: {0}\n", histogram.attribute);
                foreach (HistogramY histogramY in histogram.histogram)
                {
                    sb.AppendFormat("\tValue '{0}' was found {1} times\n", histogramY.value, histogramY.count);
                }

                sb.Append("\n");
            }

            Results = sb.ToString();
        }
    }
}