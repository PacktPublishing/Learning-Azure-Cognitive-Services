using Microsoft.CognitiveServices.SpeechRecognition;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace End_to_End.Model
{
    public class SpeechToText : IDisposable
    {
        public event EventHandler<SpeechToTextEventArgs> OnSttStatusUpdated;

        private DataRecognitionClient _dataRecClient;
        private MicrophoneRecognitionClient _micRecClient;
        private SpeechRecognitionMode _speechMode = SpeechRecognitionMode.ShortPhrase;

        private string _language = "en-US";

        private bool _isMicRecording = false;

        /// <summary>
        /// SpeechToText constructor. Creates the recording objects and calls the Initialize function
        /// </summary>
        /// <param name="bingApiKey">Bing Speech API key</param>
        public SpeechToText(string bingApiKey)
        {
            _dataRecClient = SpeechRecognitionServiceFactory.CreateDataClientWithIntentUsingEndpointUrl(_language, bingApiKey, "LUIS_ENDPOINT");
            _micRecClient = SpeechRecognitionServiceFactory.CreateMicrophoneClient(_speechMode, _language, bingApiKey);

            Initialize();
        }
              
        /// <summary>
        /// Function to start recording audio with a microphone
        /// </summary>
        public void StartMicToText()
        {
            _micRecClient.StartMicAndRecognition();
            _isMicRecording = true;
        }

        /// <summary>
        /// Function to convert from an audio file to text, through the Bing Speech API
        /// </summary>
        /// <param name="audioFileName"></param>
        public void StartAudioFileToText(string audioFileName)
        {
            using (FileStream fileStream = new FileStream(audioFileName, FileMode.Open, FileAccess.Read))
            {
                int bytesRead = 0;
                byte[] buffer = new byte[1024];

                try
                {
                    do
                    {
                        bytesRead = fileStream.Read(buffer, 0, buffer.Length);

                        _dataRecClient.SendAudio(buffer, bytesRead);
                    }
                    while (bytesRead > 0);
                }
                catch(Exception ex)
                {
                    Debug.WriteLine($"Exception caught: {ex.Message}");
                }
                finally
                {
                    _dataRecClient.EndAudio();
                }
            }
        }

        /// <summary>
        /// Funciton to initialize the recording clients
        /// </summary>
        private void Initialize()
        {
            _micRecClient.OnMicrophoneStatus += OnMicrophoneStatus;
            _micRecClient.OnPartialResponseReceived += OnPartialResponseReceived;
            _micRecClient.OnResponseReceived += OnResponseReceived;
            _micRecClient.OnConversationError += OnConversationErrorReceived;

            _dataRecClient.OnIntent += OnIntentReceived;
            _dataRecClient.OnPartialResponseReceived += OnPartialResponseReceived;
            _dataRecClient.OnConversationError += OnConversationErrorReceived;
            _dataRecClient.OnResponseReceived += OnResponseReceived;
        }

        /// <summary>
        /// Event handler function for intents. Raises a status event with the received intent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnIntentReceived(object sender, SpeechIntentEventArgs e)
        {
            SpeechToTextEventArgs args = new SpeechToTextEventArgs(SttStatus.Success, $"Intent received: {e.Intent.ToString()}.\nPayload: {e.Payload}");
            RaiseSttStatusUpdated(args);
        }

        /// <summary>
        /// Event handler function for changes in microphone status. Will print status in the debug console
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMicrophoneStatus(object sender, MicrophoneEventArgs e)
        {
            Debug.WriteLine($"Microphone status changed to recording: {e.Recording}");
        }

        /// <summary>
        /// Event handler function for partial responses. Prints the response to the debug console
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPartialResponseReceived(object sender, PartialSpeechResponseEventArgs e)
        {
            Debug.WriteLine($"Partial response received: {e.PartialResult}");
        }

        /// <summary>
        /// Event handler function to handle responses. Will add phrases to a list
        /// and publish them with the status event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnResponseReceived(object sender, SpeechResponseEventArgs e)
        {
            if (_isMicRecording) StopMicRecording();

            RecognizedPhrase[] recognizedPhrases = e.PhraseResponse.Results;
            List<string> phrasesToDisplay = new List<string>();

            foreach(RecognizedPhrase phrase in recognizedPhrases)
            {
                phrasesToDisplay.Add(phrase.DisplayText);
            }

            SpeechToTextEventArgs args = new SpeechToTextEventArgs(SttStatus.Success, $"STT completed with status: {e.PhraseResponse.RecognitionStatus.ToString()}", phrasesToDisplay);

            RaiseSttStatusUpdated(args);
        }

        /// <summary>
        /// Event handler function for conversation errors.
        /// Will raise the status event with the error message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConversationErrorReceived(object sender, SpeechErrorEventArgs e)
        {
            if (_isMicRecording) StopMicRecording();

            string message = $"Speech to text failed with status code: {e.SpeechErrorCode.ToString()}, and error message: {e.SpeechErrorText}";
            SpeechToTextEventArgs args = new SpeechToTextEventArgs(SttStatus.Error, message);

            RaiseSttStatusUpdated(args);
        }

        /// <summary>
        /// Function to stop microphone recordings
        /// </summary>
        private void StopMicRecording()
        {
            _micRecClient.EndMicAndRecognition();
            _isMicRecording = false;
        }

        /// <summary>
        /// Helper function to raise OnSttStatusUpdated events
        /// </summary>
        /// <param name="args"></param>
        private void RaiseSttStatusUpdated(SpeechToTextEventArgs args)
        {
            OnSttStatusUpdated?.Invoke(this, args);
        }

        #region Dispose

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_micRecClient != null)
                {
                    _micRecClient.EndMicAndRecognition();

                    _micRecClient.OnMicrophoneStatus -= OnMicrophoneStatus;
                    _micRecClient.OnPartialResponseReceived -= OnPartialResponseReceived;
                    _micRecClient.OnResponseReceived -= OnResponseReceived;
                    _micRecClient.OnConversationError -= OnConversationErrorReceived;

                    _micRecClient.Dispose();
                    _micRecClient = null;
                }

                if(_dataRecClient != null)
                {
                    _dataRecClient.OnIntent -= OnIntentReceived;
                    _dataRecClient.OnPartialResponseReceived -= OnPartialResponseReceived;
                    _dataRecClient.OnConversationError -= OnConversationErrorReceived;
                    _dataRecClient.OnResponseReceived -= OnResponseReceived;

                    _dataRecClient.Dispose();
                    _dataRecClient = null;
                }
            }
        }

        #endregion Dispose
    }

    public enum SttStatus
    {
        Success,
        Error
    }

    /// <summary>
    /// Event args class for speech to text events. Contains <see cref="SttStatus"/>, message and a list of results
    /// </summary>
    public class SpeechToTextEventArgs : EventArgs
    {
        public SttStatus Status { get; private set; }
        public string Message { get; private set; }
        public List<string> Results { get; private set; }

        public SpeechToTextEventArgs(SttStatus status, string message, List<string> results = null)
        {
            Status = status;
            Message = message;
            Results = results;
        }
    }
}