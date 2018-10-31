using Microsoft.ProjectOxford.SpeakerRecognition;
using Microsoft.ProjectOxford.SpeakerRecognition.Contract.Identification;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace End_to_End.Model
{
    public class SpeakerIdentification
    {
        public event EventHandler<SpeakerIdentificationStatusUpdateEventArgs> OnSpeakerIdentificationStatusUpdated;
        public event EventHandler<SpeakerIdentificationErrorEventArgs> OnSpeakerIdentificationError;

        private ISpeakerIdentificationServiceClient _speakerIdentificationClient;

        /// <summary>
        /// SpeakerIdentification constructor.
        /// </summary>
        /// <param name="speakerIdentificationClient"><see cref="ISpeakerIdentificationServiceClient"/></param>
        public SpeakerIdentification(ISpeakerIdentificationServiceClient speakerIdentificationClient)
        {
            _speakerIdentificationClient = speakerIdentificationClient;
        }

        /// <summary>
        /// Function to identifiy a speaker based on audio input
        /// </summary>
        /// <param name="audioStream">The audio stream to use for identification</param>
        /// <param name="speakerIds">An array of GUIDs with potential correct speaker profiles</param>
        public async void IdentifySpeaker(Stream audioStream, Guid[] speakerIds)
        {
            try
            {
                OperationLocation location = await _speakerIdentificationClient.IdentifyAsync(audioStream, speakerIds);

                if (location == null)
                {
                    RaiseOnIdentificationError(new SpeakerIdentificationErrorEventArgs("Failed to identify speaker."));
                    return;
                }

                GetIdentificationOperationStatus(location);                
            }
            catch(IdentificationException ex)
            {
                RaiseOnIdentificationError(new SpeakerIdentificationErrorEventArgs($"Failed to identify speaker: {ex.Message}"));
            }
            catch (Exception ex)
            {
                RaiseOnIdentificationError(new SpeakerIdentificationErrorEventArgs($"Failed to identify speaker: {ex.Message}"));
            }
        }

        /// <summary>
        /// Function to create a speaker profile. Defaults to en-US language
        /// </summary>
        /// <returns>The GUID for the newly created speaker profile</returns>
        public async Task<Guid> CreateSpeakerProfile()
        {
            try
            {
                CreateProfileResponse response = await _speakerIdentificationClient.CreateProfileAsync("en-US");

                if (response == null)
                {
                    RaiseOnIdentificationError(new SpeakerIdentificationErrorEventArgs("Failed to create speaker profile."));
                    return Guid.Empty;
                }

                return response.ProfileId;
            }
            catch (IdentificationException ex)
            {
                RaiseOnIdentificationError(new SpeakerIdentificationErrorEventArgs($"Failed to create speaker profile: {ex.Message}"));

                return Guid.Empty;
            }
            catch (Exception ex)
            {
                RaiseOnIdentificationError(new SpeakerIdentificationErrorEventArgs($"Failed to create speaker profile: {ex.Message}"));

                return Guid.Empty;
            }
        }
        
        /// <summary>
        /// Function to create a speaker enrollment for a given speaker profile
        /// </summary>
        /// <param name="audioStream">The audio stream provided by a given speaker</param>
        /// <param name="profileId">The GUID for the speaker profile to enroll</param>
        public async void CreateSpeakerEnrollment(Stream audioStream, Guid profileId)
        {
            try
            {
                OperationLocation location = await _speakerIdentificationClient.EnrollAsync(audioStream, profileId);

                if (location == null)
                {
                    RaiseOnIdentificationError(new SpeakerIdentificationErrorEventArgs("Failed to start enrollment process."));
                    return;
                }

                GetEnrollmentOperationStatus(location);
            }
            catch (IdentificationException ex)
            {
                RaiseOnIdentificationError(new SpeakerIdentificationErrorEventArgs($"Failed to add speaker enrollment: {ex.Message}"));
            }
            catch (Exception ex)
            {
                RaiseOnIdentificationError(new SpeakerIdentificationErrorEventArgs($"Failed to add speaker enrollment: {ex.Message}"));
            }
        }

        /// <summary>
        /// Function to list all speaker profiles in the system
        /// </summary>
        /// <returns>A list of GUIDs for all speaker profiles</returns>
        public async Task<List<Guid>> ListSpeakerProfiles()
        {
            try
            {
                List<Guid> speakerProfiles = new List<Guid>();

                Profile[] profiles = await _speakerIdentificationClient.GetProfilesAsync();

                if (profiles == null || profiles.Length == 0)
                {
                    RaiseOnIdentificationError(new SpeakerIdentificationErrorEventArgs("No profiles exist"));
                    return null;
                }

                foreach (Profile profile in profiles)
                {
                    speakerProfiles.Add(profile.ProfileId);
                }

                return speakerProfiles;
            }
            catch (IdentificationException ex)
            {
                RaiseOnIdentificationError(new SpeakerIdentificationErrorEventArgs($"Failed to retrieve speaker profile list: {ex.Message}"));
                return null;
            }
            catch (Exception ex)
            {
                RaiseOnIdentificationError(new SpeakerIdentificationErrorEventArgs($"Failed to retrieve speaker profile list: {ex.Message}"));
                return null;
            }

        }

        /// <summary>
        /// Function to retrieve a given speaker profile
        /// </summary>
        /// <param name="profileId">GUID for the profile to retrieve</param>
        public async void GetSpeakerProfile(Guid profileId)
        {
            try
            {
                Profile profile = await _speakerIdentificationClient.GetProfileAsync(profileId);

                if (profile == null)
                {
                    RaiseOnIdentificationError(new SpeakerIdentificationErrorEventArgs($"No speaker profile found for ID {profileId}"));
                    return;
                }
            }
            catch (IdentificationException ex)
            {
                RaiseOnIdentificationError(new SpeakerIdentificationErrorEventArgs($"Failed to retrieve speaker profile: {ex.Message}"));
            }
            catch (Exception ex)
            {
                RaiseOnIdentificationError(new SpeakerIdentificationErrorEventArgs($"Failed to retrieve speaker profile: {ex.Message}"));
            }
        }

        /// <summary>
        /// Function to delete a given profile
        /// </summary>
        /// <param name="profileId">GUID for the profile to delete</param>
        public async void DeleteSpeakerProfile(Guid profileId)
        {
            try
            {
                await _speakerIdentificationClient.DeleteProfileAsync(profileId);
            }
            catch (IdentificationException ex)
            {
                RaiseOnIdentificationError(new SpeakerIdentificationErrorEventArgs($"Failed to delete speaker profile: {ex.Message}"));
            }
            catch (Exception ex)
            {
                RaiseOnIdentificationError(new SpeakerIdentificationErrorEventArgs($"Failed to delete speaker profile: {ex.Message}"));
            }
        }

        /// <summary>
        /// Function to reset enrollments for a given profile
        /// </summary>
        /// <param name="profileId">GUID for the profile to reset enrollments for</param>
        public async void ResetEnrollments(Guid profileId)
        {
            try
            {
                await _speakerIdentificationClient.ResetEnrollmentsAsync(profileId);
            }
            catch (IdentificationException ex)
            {
                RaiseOnIdentificationError(new SpeakerIdentificationErrorEventArgs($"Failed to reset enrollments: {ex.Message}"));
            }
            catch (Exception ex)
            {
                RaiseOnIdentificationError(new SpeakerIdentificationErrorEventArgs($"Failed to reset enrollments: {ex.Message}"));
            }
        }

        /// <summary>
        /// Function to get the identification status
        /// </summary>
        /// <param name="location">The <see cref="OperationLocation"/> for the current identification process</param>
        private async void GetIdentificationOperationStatus(OperationLocation location)
        {
            try
            {
                while (true)
                {
                    IdentificationOperation result = await _speakerIdentificationClient.CheckIdentificationStatusAsync(location);

                    if (result.Status != Status.Running)
                    {
                        RaiseOnIdentificationStatusUpdated(new SpeakerIdentificationStatusUpdateEventArgs(result.Status.ToString(),
                            $"Enrollment finished with message: {result.Message}.")
                        { IdentifiedProfile = result.ProcessingResult });

                        break;
                    }

                    RaiseOnIdentificationStatusUpdated(new SpeakerIdentificationStatusUpdateEventArgs(result.Status.ToString(), "Identifying..."));

                    await Task.Delay(1000);
                }
            }
            catch (IdentificationException ex)
            {
                RaiseOnIdentificationError(new SpeakerIdentificationErrorEventArgs($"Failed to get operation status: {ex.Message}"));
            }
            catch (Exception ex)
            {
                RaiseOnIdentificationError(new SpeakerIdentificationErrorEventArgs($"Failed to get operation status: {ex.Message}"));
            }
        }

        /// <summary>
        /// Function to get the enrollment operation status. 
        /// </summary>
        /// <param name="location">The <see cref="OperationLocation"/> for the enrollment</param>
        private async void GetEnrollmentOperationStatus(OperationLocation location)
        {
            try
            {
                while(true)
                { 
                    EnrollmentOperation result = await _speakerIdentificationClient.CheckEnrollmentStatusAsync(location);

                    if(result.Status != Status.Running)
                    {
                        RaiseOnIdentificationStatusUpdated(new SpeakerIdentificationStatusUpdateEventArgs(result.Status.ToString(),
                            $"Enrollment finished. Enrollment status: {result.ProcessingResult.EnrollmentStatus.ToString()}"));

                        break;
                    }

                    RaiseOnIdentificationStatusUpdated(new SpeakerIdentificationStatusUpdateEventArgs(result.Status.ToString(), "Enrolling..."));

                    await Task.Delay(1000);
                }
            }
            catch (IdentificationException ex)
            {
                RaiseOnIdentificationError(new SpeakerIdentificationErrorEventArgs($"Failed to get operation status: {ex.Message}"));
            }
            catch (Exception ex)
            {
                RaiseOnIdentificationError(new SpeakerIdentificationErrorEventArgs($"Failed to get operation status: {ex.Message}"));
            }
        }

        /// <summary>
        /// Helper functions to raise speaker identification status updates
        /// </summary>
        /// <param name="args"></param>
        private void RaiseOnIdentificationStatusUpdated(SpeakerIdentificationStatusUpdateEventArgs args)
        {
            OnSpeakerIdentificationStatusUpdated?.Invoke(this, args);
        }

        /// <summary>
        /// Helper function to raise speaker identification errors
        /// </summary>
        /// <param name="args"></param>
        private void RaiseOnIdentificationError(SpeakerIdentificationErrorEventArgs args)
        {
            OnSpeakerIdentificationError?.Invoke(this, args);
        }
    }
    
    /// <summary>
    /// EventArgs class for speaker identification status updates. Contains a status text, message and <see cref="Identification"/> profile
    /// </summary>
    public class SpeakerIdentificationStatusUpdateEventArgs : EventArgs
    {
        public string Status { get; private set; }
        public string Message { get; private set; }
        public Identification IdentifiedProfile { get; set; }

        public SpeakerIdentificationStatusUpdateEventArgs(string status, string message)
        {
            Status = status;
            Message = message;
        }
    }

    /// <summary>
    /// EventArgs class for speaker identification errors
    /// </summary>
    public class SpeakerIdentificationErrorEventArgs : EventArgs
    {
        public string ErrorMessage { get; private set; }

        public SpeakerIdentificationErrorEventArgs(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }
}