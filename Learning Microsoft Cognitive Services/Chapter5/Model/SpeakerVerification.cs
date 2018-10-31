using Microsoft.ProjectOxford.SpeakerRecognition;
using Microsoft.ProjectOxford.SpeakerRecognition.Contract.Verification;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Chapter5.Model
{
    public class SpeakerVerification
    {
        public event EventHandler<SpeakerVerificationStatusUpdateEventArgs> OnSpeakerVerificationStatusUpdated;
        public event EventHandler<SpeakerVerificationErrorEventArgs> OnSpeakerVerificationError;

        private ISpeakerVerificationServiceClient _speakerVerificationClient;

        /// <summary>
        /// SpeakerVerification constructor.
        /// </summary>
        /// <param name="speakerVerificationClient"><see cref="ISpeakerVerificationServiceClient"/> verification client object</param>
        public SpeakerVerification(ISpeakerVerificationServiceClient speakerVerificationClient)
        {
            _speakerVerificationClient = speakerVerificationClient;
        }

        /// <summary>
        /// Function to verify a given speaker
        /// </summary>
        /// <param name="audioStream">Stream object containing the recorded verification phrase</param>
        /// <param name="speakerProfile">The ID (GUID) of the speaker profile to verify</param>
        public async void VerifySpeaker(Stream audioStream, Guid speakerProfile)
        {
            try
            {
                Verification verification = await _speakerVerificationClient.VerifyAsync(audioStream, speakerProfile);

                if (verification == null)
                {
                    RaiseOnVerificationError(new SpeakerVerificationErrorEventArgs("Failed to verify speaker."));
                    return;
                }

                RaiseOnVerificationStatusUpdate(new SpeakerVerificationStatusUpdateEventArgs("Verified", "Verified speaker") { VerifiedProfile = verification });             
            }
            catch(VerificationException ex)
            {
                RaiseOnVerificationError(new SpeakerVerificationErrorEventArgs($"Failed to identify speaker: {ex.Message}"));
            }
            catch (Exception ex)
            {
                RaiseOnVerificationError(new SpeakerVerificationErrorEventArgs($"Failed to identify speaker: {ex.Message}"));
            }
        }

        /// <summary>
        /// Function to create a new speaker profile
        /// </summary>
        /// <returns>Returns an ID (GUID) for the newly created speaker profile</returns>
        public async Task<Guid> CreateSpeakerProfile()
        {
            try
            {
                CreateProfileResponse response = await _speakerVerificationClient.CreateProfileAsync("en-US");

                if (response == null)
                {
                    RaiseOnVerificationError(new SpeakerVerificationErrorEventArgs("Failed to create speaker profile."));
                    return Guid.Empty;
                }

                return response.ProfileId;
            }
            catch (VerificationException ex)
            {
                RaiseOnVerificationError(new SpeakerVerificationErrorEventArgs($"Failed to create speaker profile: {ex.Message}"));

                return Guid.Empty;
            }
            catch (Exception ex)
            {
                RaiseOnVerificationError(new SpeakerVerificationErrorEventArgs($"Failed to create speaker profile: {ex.Message}"));

                return Guid.Empty;
            }
        }

        /// <summary>
        /// Function to retrieve a list of all possible verification phrases
        /// </summary>
        /// <returns>A list of strings with each verification phrase</returns>
        public async Task<List<string>> GetVerificationPhrase()
        {
            try
            {
                List<string> phrases = new List<string>();

                VerificationPhrase[] results = await _speakerVerificationClient.GetPhrasesAsync("en-US");

                foreach(VerificationPhrase phrase in results)
                {
                    phrases.Add(phrase.Phrase);
                }

                return phrases;
            }
            catch (VerificationException ex)
            {
                RaiseOnVerificationError(new SpeakerVerificationErrorEventArgs($"Failed to create speaker profile: {ex.Message}"));

                return null;
            }
            catch (Exception ex)
            {
                RaiseOnVerificationError(new SpeakerVerificationErrorEventArgs($"Failed to create speaker profile: {ex.Message}"));

                return null;
            }
        }
        
        /// <summary>
        /// Function to create an enrollment for a given speaker
        /// </summary>
        /// <param name="audioStream">Stream object with enrollment audio</param>
        /// <param name="profileId">The unique ID (GUID) of the speaker profile to enroll</param>
        public async void CreateSpeakerEnrollment(Stream audioStream, Guid profileId)
        {
            try
            {
                Enrollment enrollmentStatus = await _speakerVerificationClient.EnrollAsync(audioStream, profileId);

                if (enrollmentStatus == null)
                {
                    RaiseOnVerificationError(new SpeakerVerificationErrorEventArgs("Failed to start enrollment process."));
                    return;
                }

                RaiseOnVerificationStatusUpdate(new SpeakerVerificationStatusUpdateEventArgs("Succeeded", $"Enrollment status: {enrollmentStatus.EnrollmentStatus}"));
            }
            catch (VerificationException ex)
            {
                RaiseOnVerificationError(new SpeakerVerificationErrorEventArgs($"Failed to add speaker enrollment: {ex.Message}"));
            }
            catch (Exception ex)
            {
                RaiseOnVerificationError(new SpeakerVerificationErrorEventArgs($"Failed to add speaker enrollment: {ex.Message}"));
            }
        }

        /// <summary>
        /// Function to retrieve a list of all registered speaker profiles
        /// </summary>
        /// <returns>Returns a list of speaker profile IDs (GUID)</returns>
        public async Task<List<Guid>> ListSpeakerProfiles()
        {
            try
            {
                List<Guid> speakerProfiles = new List<Guid>();

                Profile[] profiles = await _speakerVerificationClient.GetProfilesAsync();

                if (profiles == null || profiles.Length == 0)
                {
                    RaiseOnVerificationError(new SpeakerVerificationErrorEventArgs("No profiles exist"));
                    return null;
                }

                foreach (Profile profile in profiles)
                {
                    speakerProfiles.Add(profile.ProfileId);
                }

                return speakerProfiles;
            }
            catch (VerificationException ex)
            {
                RaiseOnVerificationError(new SpeakerVerificationErrorEventArgs($"Failed to retrieve speaker profile list: {ex.Message}"));
                return null;
            }
            catch (Exception ex)
            {
                RaiseOnVerificationError(new SpeakerVerificationErrorEventArgs($"Failed to retrieve speaker profile list: {ex.Message}"));
                return null;
            }

        }

        /// <summary>
        /// Function to retrieve a given speaker profile.
        /// Currently not doing anything to it
        /// </summary>
        /// <param name="profileId">GUID - The unique ID of the profile to reset enrollment for</param>
        public async void GetSpeakerProfile(Guid profileId)
        {
            try
            {
                Profile profile = await _speakerVerificationClient.GetProfileAsync(profileId);

                if (profile == null)
                {
                    RaiseOnVerificationError(new SpeakerVerificationErrorEventArgs($"No speaker profile found for ID {profileId}"));
                    return;
                }
            }
            catch (VerificationException ex)
            {
                RaiseOnVerificationError(new SpeakerVerificationErrorEventArgs($"Failed to retrieve speaker profile: {ex.Message}"));
            }
            catch (Exception ex)
            {
                RaiseOnVerificationError(new SpeakerVerificationErrorEventArgs($"Failed to retrieve speaker profile: {ex.Message}"));
            }
        }

        /// <summary>
        /// Function to delete a given speaker profile
        /// </summary>
        /// <param name="profileId">GUID - The unique ID of the profile to reset enrollment for</param>
        public async void DeleteSpeakerProfile(Guid profileId)
        {
            try
            {
                await _speakerVerificationClient.DeleteProfileAsync(profileId);
            }
            catch (VerificationException ex)
            {
                RaiseOnVerificationError(new SpeakerVerificationErrorEventArgs($"Failed to delete speaker profile: {ex.Message}"));
            }
            catch (Exception ex)
            {
                RaiseOnVerificationError(new SpeakerVerificationErrorEventArgs($"Failed to delete speaker profile: {ex.Message}"));
            }
        }

        /// <summary>
        /// Function to reset enrollments
        /// </summary>
        /// <param name="profileId">GUID - The unique ID of the profile to reset enrollment for</param>
        public async void ResetEnrollments(Guid profileId)
        {
            try
            {
                await _speakerVerificationClient.ResetEnrollmentsAsync(profileId);
            }
            catch (VerificationException ex)
            {
                RaiseOnVerificationError(new SpeakerVerificationErrorEventArgs($"Failed to reset enrollments: {ex.Message}"));
            }
            catch (Exception ex)
            {
                RaiseOnVerificationError(new SpeakerVerificationErrorEventArgs($"Failed to reset enrollments: {ex.Message}"));
            }
        }
        
        /// <summary>
        /// Helper function to raise the OnSpeakerVerificationStatusUpdated event
        /// </summary>
        /// <param name="args"><see cref="SpeakerVerificationStatusUpdateEventArgs"/></param>
        private void RaiseOnVerificationStatusUpdate(SpeakerVerificationStatusUpdateEventArgs args)
        {
            OnSpeakerVerificationStatusUpdated?.Invoke(this, args);
        }

        /// <summary>
        /// Helper function to raise the OnSpeakerVerificationError event
        /// </summary>
        /// <param name="args"><see cref="SpeakerVerificationErrorEventArgs"/></param>
        private void RaiseOnVerificationError(SpeakerVerificationErrorEventArgs args)
        {
            OnSpeakerVerificationError?.Invoke(this, args);
        }
    }
    
    /// <summary>
    /// EventArgs class for updates to the status, during speaker recognition
    /// </summary>
    public class SpeakerVerificationStatusUpdateEventArgs : EventArgs
    {
        public string Status { get; private set; }
        public string Message { get; private set; }
        public Verification VerifiedProfile { get; set; }

        public SpeakerVerificationStatusUpdateEventArgs(string status, string message)
        {
            Status = status;
            Message = message;
        }
    }

    /// <summary>
    /// EventArgs class for errors in speaker verification
    /// </summary>
    public class SpeakerVerificationErrorEventArgs : EventArgs
    {
        public string ErrorMessage { get; private set; }

        public SpeakerVerificationErrorEventArgs(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }
}