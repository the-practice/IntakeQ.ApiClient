using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using IntakeQ.ApiClient.Services;

namespace IntakeQ.ApiClient.Extensions
{
    /// <summary>
    /// Extension methods for configuring IntakeQ VAPI services
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add IntakeQ VAPI services to the service collection
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddIntakeQVapiServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register IntakeQ API client
            services.AddSingleton<ApiClient>(provider =>
            {
                var apiKey = configuration["IntakeQ:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new InvalidOperationException("IntakeQ:ApiKey configuration is required");
                }
                return new ApiClient(apiKey);
            });

            // Register Partner API client if configured
            services.AddSingleton<PartnerApiClient>(provider =>
            {
                var partnerApiKey = configuration["IntakeQ:PartnerApiKey"];
                if (string.IsNullOrEmpty(partnerApiKey))
                {
                    return null; // Partner API is optional
                }
                return new PartnerApiClient(partnerApiKey);
            });

            // Register VAPI service
            services.AddScoped<VapiIntakeQService>(provider =>
            {
                var apiKey = configuration["IntakeQ:ApiKey"];
                var partnerApiKey = configuration["IntakeQ:PartnerApiKey"];

                if (!string.IsNullOrEmpty(partnerApiKey))
                {
                    return new VapiIntakeQService(apiKey, partnerApiKey);
                }
                else
                {
                    return new VapiIntakeQService(apiKey);
                }
            });

            return services;
        }

        /// <summary>
        /// Add IntakeQ VAPI services with explicit API keys
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="apiKey">IntakeQ API key</param>
        /// <param name="partnerApiKey">Partner API key (optional)</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddIntakeQVapiServices(this IServiceCollection services, string apiKey, string partnerApiKey = null)
        {
            // Register IntakeQ API client
            services.AddSingleton<ApiClient>(provider => new ApiClient(apiKey));

            // Register Partner API client if provided
            if (!string.IsNullOrEmpty(partnerApiKey))
            {
                services.AddSingleton<PartnerApiClient>(provider => new PartnerApiClient(partnerApiKey));
            }

            // Register VAPI service
            services.AddScoped<VapiIntakeQService>(provider =>
            {
                if (!string.IsNullOrEmpty(partnerApiKey))
                {
                    return new VapiIntakeQService(apiKey, partnerApiKey);
                }
                else
                {
                    return new VapiIntakeQService(apiKey);
                }
            });

            return services;
        }
    }
}
