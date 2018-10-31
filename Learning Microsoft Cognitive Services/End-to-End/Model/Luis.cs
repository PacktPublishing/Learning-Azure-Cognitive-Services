using Microsoft.Cognitive.LUIS;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace End_to_End.Model
{
    public class Luis
    {
        public event EventHandler<LuisUtteranceResultEventArgs> OnLuisUtteranceResultUpdated;
        
        private LuisClient _luisClient;

        /// <summary>
        /// Luis constructor. Will assign a <see cref="LuisClient"/> to the private member
        /// </summary>
        /// <param name="luisClient">A <see cref="LuisClient"/> object</param>
        public Luis(LuisClient luisClient)
        {
            _luisClient = luisClient;
        }

        /// <summary>
        /// Function to make a LUIS request. Will pass results on to the ProcessResult function to parse the results
        /// </summary>
        /// <param name="input">The request string</param>
        /// <returns></returns>
        public async Task RequestAsync(string input)
        {
            try
            {
                LuisResult result = await _luisClient.Predict(input);
                ProcessResult(result);
            }
            catch(Exception ex)
            {
                RaiseOnLuisUtteranceResultUpdated(new LuisUtteranceResultEventArgs { Status = "Failed", Message = ex.Message });
            }
        }

        /// <summary>
        /// Function to reply to a LUIS result. Will be used if the previous response requires more information
        /// The function will pass any results to the ProcessResult function
        /// </summary>
        /// <param name="previousResult">The <see cref="LuisResult"/> of the previous API call</param>
        /// <param name="input">Response to the LUIS dialog question</param>
        /// <returns></returns>
        public async Task ReplyAsync(LuisResult previousResult, string input)
        {
            try
            {
                LuisResult result = await _luisClient.Reply(previousResult, input);
                ProcessResult(result);
            }
            catch(Exception ex)
            {
                RaiseOnLuisUtteranceResultUpdated(new LuisUtteranceResultEventArgs { Status = "Failed", Message = ex.Message });
            }
        }

        /// <summary>
        /// Function to process LUIS results. Creates a new <see cref="LuisUtteranceResultEventArgs"/> objects and populates it with information
        /// </summary>
        /// <param name="result">The <see cref="LuisResult"/> object from the API call</param>
        private void ProcessResult(LuisResult result)
        {
            LuisUtteranceResultEventArgs args = new LuisUtteranceResultEventArgs();

            args.RequiresReply = !string.IsNullOrEmpty(result.DialogResponse?.Prompt);
            args.DialogResponse = !string.IsNullOrEmpty(result.DialogResponse?.Prompt) ? result.DialogResponse.Prompt : string.Empty;

            if (!string.IsNullOrEmpty(result.TopScoringIntent.Name))
            {
                var intentName = result.TopScoringIntent.Name;
                args.IntentName = intentName;
            }
            else
            {
                args.IntentName = string.Empty;
            }

            if(result.Entities.Count > 0)
            {
                var entity = result.Entities.First().Value;
                if(entity.Count > 0)
                {
                    var entityName = entity.First().Value;
                    args.EntityName = entityName;
                }
            }

            args.Status = "Succeeded";
            args.Message = $"Top intent is {result.TopScoringIntent.Name} with score {result.TopScoringIntent.Score}. Found {result.Entities.Count} entities.";

            RaiseOnLuisUtteranceResultUpdated(args);
        }

        /// <summary>
        /// Helper function to raise OnLuisUtteranceResultUpdated events
        /// </summary>
        /// <param name="args"></param>
        private void RaiseOnLuisUtteranceResultUpdated(LuisUtteranceResultEventArgs args)
        {
            OnLuisUtteranceResultUpdated?.Invoke(this, args);
        }
    }
    
    /// <summary>
    /// EventArgs class for LUIS results. Contains status, message and dialog response amongts other information
    /// </summary>
    public class LuisUtteranceResultEventArgs : EventArgs
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public bool RequiresReply { get; set; }
        public string DialogResponse { get; set; }
        public string IntentName { get; set; }
        public string EntityName { get; set; }
    }
}