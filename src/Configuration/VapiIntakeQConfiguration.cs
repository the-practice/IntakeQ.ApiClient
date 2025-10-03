using System;
using System.Collections.Generic;

namespace IntakeQ.ApiClient.Configuration
{
    /// <summary>
    /// Configuration settings for VAPI IntakeQ integration
    /// </summary>
    public class VapiIntakeQConfiguration
    {
        /// <summary>
        /// IntakeQ API key
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// IntakeQ Partner API key (optional)
        /// </summary>
        public string PartnerApiKey { get; set; }

        /// <summary>
        /// Base URL for IntakeQ API (defaults to production)
        /// </summary>
        public string BaseUrl { get; set; } = "https://intakeq.com/api/v1/";

        /// <summary>
        /// Partner API base URL (defaults to production)
        /// </summary>
        public string PartnerBaseUrl { get; set; } = "https://intakeq.com/api/partner/";

        /// <summary>
        /// Default number of days to look ahead for upcoming appointments
        /// </summary>
        public int DefaultAppointmentLookAheadDays { get; set; } = 30;

        /// <summary>
        /// Maximum number of search results to return for voice responses
        /// </summary>
        public int MaxVoiceSearchResults { get; set; } = 3;

        /// <summary>
        /// Enable detailed logging for debugging
        /// </summary>
        public bool EnableDetailedLogging { get; set; } = false;

        /// <summary>
        /// Validate configuration settings
        /// </summary>
        /// <returns>True if configuration is valid</returns>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(ApiKey);
        }

        /// <summary>
        /// Get validation errors
        /// </summary>
        /// <returns>List of validation errors</returns>
        public string[] GetValidationErrors()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                errors.Add("ApiKey is required");
            }

            if (DefaultAppointmentLookAheadDays <= 0)
            {
                errors.Add("DefaultAppointmentLookAheadDays must be greater than 0");
            }

            if (MaxVoiceSearchResults <= 0)
            {
                errors.Add("MaxVoiceSearchResults must be greater than 0");
            }

            return errors.ToArray();
        }
    }
}
