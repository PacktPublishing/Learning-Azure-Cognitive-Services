using Chapter5.Interface;
using Chapter5.Model;
using Microsoft.ProjectOxford.SpeakerRecognition;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Chapter5.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        private bool _isVerificationAudio = false;

        private SpeakerVerification _speakerVerification;
        private Recording _recording;

        private ObservableCollection<string> _verificationPhrases = new ObservableCollection<string>();
        public ObservableCollection<string> VerificationPhrases
        {
            get { return _verificationPhrases; }
            set
            {
                _verificationPhrases = value;
                RaisePropertyChangedEvent("VerificationPhrases");
            }
        }

        private ObservableCollection<Guid> _speakerProfiles = new ObservableCollection<Guid>(); 
        public ObservableCollection<Guid> SpeakerProfiles
        {
            get { return _speakerProfiles; }
            set
            {
                _speakerProfiles = value;
                RaisePropertyChangedEvent("SpeakerProfiles");
            }
        }

        private Guid _selectedSpeakerProfile;
        public Guid SelectedSpeakerProfile
        {
            get { return _selectedSpeakerProfile; }
            set
            {
                _selectedSpeakerProfile = value;
                RaisePropertyChangedEvent("SelectedSpeakerProfile");
            }
        }

        private string _statusText;
        public string StatusText
        {
            get { return _statusText; }
            set
            {
                _statusText = value;
                RaisePropertyChangedEvent("StatusText");
            }
        }

        public ICommand AddSpeakerCommand { get; private set; }
        public ICommand DeleteSpeakerProfileCommand { get; private set; }
        public ICommand EnrollSpeakerCommand { get; private set; }
        public ICommand StopRecordingCommand { get; private set; }
        public ICommand ResetEnrollmentsCommand { get; private set; }
        public ICommand StartVerificationCommand { get; private set; }
        public ICommand StopVerificationCommand { get; private set; }

        /// <summary>
        /// MainViewModel constructor. Creates a new SpeakerVerificationServiceClient, using the Speech Recognition API key.
        /// Hooks up events to event handlers, and creates command objects.
        /// Retrieves speaker profiles and verification phrases
        /// </summary>
        public MainViewModel()
        {
            _recording = new Recording();
            _recording.OnAudioStreamAvailable += OnAudioStreamAvailable;
            _recording.OnRecordingError += OnRecordingError;

            _speakerVerification = new SpeakerVerification(new SpeakerVerificationServiceClient("API_KEY_HERE"));
            _speakerVerification.OnSpeakerVerificationError += OnSpeakerVerificationError;
            _speakerVerification.OnSpeakerVerificationStatusUpdated += OnSpeakerVerificationStatus;

            AddSpeakerCommand = new DelegateCommand(AddSpeaker);
            DeleteSpeakerProfileCommand = new DelegateCommand(DeleteSpeaker, CanExecuteSpeakerCommands);
            EnrollSpeakerCommand = new DelegateCommand(StartEnrolling, CanExecuteSpeakerCommands);
            StopRecordingCommand = new DelegateCommand(StopEnrolling);
            ResetEnrollmentsCommand = new DelegateCommand(ResetEnrollments, CanExecuteSpeakerCommands);
            StartVerificationCommand = new DelegateCommand(StartVerification, CanExecuteSpeakerCommands);
            StopVerificationCommand = new DelegateCommand(StopVerification);

            GetSpeakerProfiles();
            GetVerificationPhrases();
        }

        /// <summary>
        /// Function to retrieve all registered speaker profiles
        /// Adds all profiles to a list, displayed in the UI
        /// </summary>
        private async void GetSpeakerProfiles()
        {
            List<Guid> profiles = await _speakerVerification.ListSpeakerProfiles();

            if (profiles == null || profiles.Count == 0) return;

            foreach(Guid profile in profiles)
            {
                SpeakerProfiles.Add(profile);
            }
        }

        /// <summary>
        /// Function to retrieve all available verification phrases. 
        /// Adds all phrases to a list, displayed in the UI
        /// </summary>
        private async void GetVerificationPhrases()
        {
            List<string> phrases = await _speakerVerification.GetVerificationPhrase();

            foreach(string phrase in phrases)
            {
                VerificationPhrases.Add(phrase);
            }
        }

        #region Event handlers

        /// <summary>
        /// Function to handle OnAudioStreamAvailable event. Will either create a speaker or verify
        /// the speaker based on previous inputs.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAudioStreamAvailable(object sender, RecordingAudioAvailableEventArgs e)
        {
            if (_isVerificationAudio)
                _speakerVerification.VerifySpeaker(e.AudioStream, SelectedSpeakerProfile);
            else
                _speakerVerification.CreateSpeakerEnrollment(e.AudioStream, SelectedSpeakerProfile);
        }

        /// <summary>
        /// Event handler to display any errors during recording
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRecordingError(object sender, RecordingErrorEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                StatusText = e.ErrorMessage;
            });
        }

        /// <summary>
        /// Event handler to display any errors during speaker verification
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSpeakerVerificationError(object sender, SpeakerVerificationErrorEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => 
            {
                StatusText = e.ErrorMessage;
            });
        }

        /// <summary>
        /// Event handler to display the verification result 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSpeakerVerificationStatus(object sender, SpeakerVerificationStatusUpdateEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (e.VerifiedProfile != null)
                    StatusText = $"Verification result: {e.VerifiedProfile.Result} with {e.VerifiedProfile.Confidence} confidence. \nPhrase used: {e.VerifiedProfile.Phrase}";
                else
                    StatusText = e.Message;
            });
        }

        #endregion Event handlers

        #region Commands 

        /// <summary>
        /// Command function to add a new speaker profile
        /// </summary>
        /// <param name="obj"></param>
        private async void AddSpeaker(object obj)
        {
            Guid profileId = await _speakerVerification.CreateSpeakerProfile();
            StatusText = $"Created new speaker profile with ID {profileId.ToString()}";

            GetSpeakerProfiles();
        }

        /// <summary>
        /// Determines if we can execute speaker commands
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>True if we have selected a speaker</returns>
        private bool CanExecuteSpeakerCommands(object obj)
        {
            return SelectedSpeakerProfile != null && !SelectedSpeakerProfile.Equals(Guid.Empty);
        }

        /// <summary>
        /// Command function to delete a speaker profile
        /// </summary>
        /// <param name="obj"></param>
        private void DeleteSpeaker(object obj)
        {
            _speakerVerification.DeleteSpeakerProfile(SelectedSpeakerProfile);
        }

        /// <summary>
        /// Command function to start the enrollment process for a given speaker. 
        /// Will record audio 
        /// </summary>
        /// <param name="obj"></param>
        private void StartEnrolling(object obj)
        {
            _recording.StartRecording();
            _isVerificationAudio = false;
        }

        /// <summary>
        /// Command function to stop enrollment recording
        /// </summary>
        /// <param name="obj"></param>
        private void StopEnrolling(object obj)
        {
            _recording.StopRecording();
        }

        /// <summary>
        /// Command function to reset the enrollment for a given speaker
        /// </summary>
        /// <param name="obj"></param>
        private void ResetEnrollments(object obj)
        {
            _speakerVerification.ResetEnrollments(SelectedSpeakerProfile);
        }

        /// <summary>
        /// Command function to start the verification recording
        /// </summary>
        /// <param name="obj"></param>
        private void StartVerification(object obj)
        {
            _recording.StartRecording();
            _isVerificationAudio = true;
        }

        /// <summary>
        /// Command function to stop verification recording
        /// </summary>
        /// <param name="obj"></param>
        private void StopVerification(object obj)
        {
            _recording.StopRecording();
        }

        #endregion 
    }
}