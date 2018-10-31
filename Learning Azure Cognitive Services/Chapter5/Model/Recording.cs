using NAudio.Utils;
using NAudio.Wave;
using System;
using System.IO;

namespace Chapter5.Model
{
    public class Recording
    {
        public event EventHandler<RecordingAudioAvailableEventArgs> OnAudioStreamAvailable;
        public event EventHandler<RecordingErrorEventArgs> OnRecordingError;

        private const int SAMPLERATE = 16000;
        private const int CHANNELS = 1;

        private WaveIn _waveIn;
        private WaveFileWriter _fileWriter;
        private Stream _stream;

        /// <summary>
        /// Recording constructor - Calls an InitializeRecorder function to initialize the recorder object
        /// </summary>
        public Recording()
        {
            InitializeRecorder();
        }

        /// <summary>
        /// Function to start a recording
        /// </summary>
        public void StartRecording()
        {
            if (_waveIn == null) return;

            try
            {
                if(WaveIn.DeviceCount == 0)
                {
                    RaiseRecordingError(new RecordingErrorEventArgs("No microphones detected"));
                    return;
                }

                _waveIn.StartRecording();
            }
            catch(Exception ex)
            {
                RaiseRecordingError(new RecordingErrorEventArgs($"Error when starting microphone recording: {ex.Message}"));
            }
        }

        /// <summary>
        /// Function to stop a recording
        /// </summary>
        public void StopRecording()
        {
            _waveIn.StopRecording();
        }

        /// <summary>
        /// Function to initialize the recorder object.
        /// </summary>
        private void InitializeRecorder()
        {
            _waveIn = new WaveIn();
            _waveIn.DeviceNumber = 0;
            _waveIn.WaveFormat = new WaveFormat(SAMPLERATE, CHANNELS);
            _waveIn.DataAvailable += OnDataAvailable;
            _waveIn.RecordingStopped += OnRecordingStopped;            
        }
        
        /// <summary>
        /// Event handler for when audio data is available.
        /// Will write the available data to a file writer object
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            if(_fileWriter == null)
            {
                _stream = new IgnoreDisposeStream(new MemoryStream());
                _fileWriter = new WaveFileWriter(_stream, _waveIn.WaveFormat);
            }

            _fileWriter.Write(e.Buffer, 0, e.BytesRecorded);
        }

        /// <summary>
        /// Event handler to deal with the recorded stream once it has stopped
        /// Retrieves the audio stream and raises OnAudioStreamAvailable event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            _fileWriter.Dispose();
            _fileWriter = null;
            _stream.Seek(0, SeekOrigin.Begin);

            _waveIn.Dispose();
            InitializeRecorder();

            RaiseRecordingAudioAvailable(new RecordingAudioAvailableEventArgs(_stream));
        }

        /// <summary>
        /// Helper function to raise OnAudioStreamAvailable event
        /// </summary>
        /// <param name="args"><see cref="RecordingAudioAvailableEventArgs"/></param>
        private void RaiseRecordingAudioAvailable(RecordingAudioAvailableEventArgs args)
        {
            OnAudioStreamAvailable?.Invoke(this, args);
        }

        /// <summary>
        /// Helper function to raise OnRecordingError event
        /// </summary>
        /// <param name="args"><see cref="RecordingErrorEventArgs"/></param>
        private void RaiseRecordingError(RecordingErrorEventArgs args)
        {
            OnRecordingError?.Invoke(this, args);
        }
    }

    /// <summary>
    /// EventArgs class for when a recording has completed
    /// </summary>
    public class RecordingAudioAvailableEventArgs : EventArgs
    {
        public Stream AudioStream { get; private set; }

        public RecordingAudioAvailableEventArgs(Stream audioStream)
        {
            AudioStream = audioStream;
        }
    }

    /// <summary>
    /// EventArgs class for recording error events
    /// </summary>
    public class RecordingErrorEventArgs : EventArgs
    {
        public string ErrorMessage { get; private set; }

        public RecordingErrorEventArgs(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }
}