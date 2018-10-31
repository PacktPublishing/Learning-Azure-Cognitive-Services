using NAudio.Utils;
using NAudio.Wave;
using System;
using System.IO;

namespace End_to_End.Model
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
        /// Recording constructor. Will call InitializeRecorder
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
        /// Function to initialize a recorder
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
        /// Event handler function for when data is available from the recorder.
        /// Will write the available data to a file
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
        /// Event handler function for when a recording is stopped.
        /// Will dispose the filewriter, and send the recorded stream to the caller.
        /// Will reinitialize the recorder afterwards
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
        /// Helper function to raise OnAudioStreamAvailable events
        /// </summary>
        /// <param name="args"></param>
        private void RaiseRecordingAudioAvailable(RecordingAudioAvailableEventArgs args)
        {
            OnAudioStreamAvailable?.Invoke(this, args);
        }

        /// <summary>
        /// Helper function to raise OnRecordingError events
        /// </summary>
        /// <param name="args"></param>
        private void RaiseRecordingError(RecordingErrorEventArgs args)
        {
            OnRecordingError?.Invoke(this, args);
        }
    }

    /// <summary>
    /// EventArgs class for when audio is available from a recording.
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
    /// EventArgs class for error messages in a recording
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