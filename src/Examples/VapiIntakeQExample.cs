using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IntakeQ.ApiClient.Services;
using IntakeQ.ApiClient.Models;

namespace IntakeQ.ApiClient.Examples
{
    /// <summary>
    /// Example usage of VAPI IntakeQ integration
    /// </summary>
    public class VapiIntakeQExample
    {
        private readonly VapiIntakeQService _vapiService;

        public VapiIntakeQExample(string apiKey)
        {
            _vapiService = new VapiIntakeQService(apiKey);
        }

        /// <summary>
        /// Example: Search for a client by phone number
        /// </summary>
        public async Task<ClientSearchResult> SearchClientByPhoneExample()
        {
            try
            {
                // Search for client by phone number
                var client = await _vapiService.FindClientByPhoneAsync("555-123-4567");
                
                if (client != null)
                {
                    Console.WriteLine($"Found client: {client.Name} ({client.Email})");
                    return client;
                }
                else
                {
                    Console.WriteLine("Client not found");
                    return null;
                }
            }
            catch (VapiIntakeQException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Example: Get client summary for voice response
        /// </summary>
        public async Task<string> GetClientSummaryExample(int clientId)
        {
            try
            {
                var summary = await _vapiService.GetClientSummaryForVoiceAsync(clientId);
                Console.WriteLine($"Client Summary: {summary}");
                return summary;
            }
            catch (VapiIntakeQException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Example: Get upcoming appointments for a client
        /// </summary>
        public async Task<List<AppointmentInfo>> GetUpcomingAppointmentsExample(int clientId)
        {
            try
            {
                var appointments = await _vapiService.GetUpcomingAppointmentsAsync(clientId, 30);
                
                Console.WriteLine($"Found {appointments.Count} upcoming appointments:");
                foreach (var appointment in appointments)
                {
                    Console.WriteLine($"- {appointment.StartDate:MMM dd, yyyy} at {appointment.StartDate:h:mm tt} with {appointment.PractitionerName}");
                }
                
                return appointments;
            }
            catch (VapiIntakeQException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Example: Get appointment summary for voice response
        /// </summary>
        public async Task<string> GetAppointmentSummaryExample(int clientId)
        {
            try
            {
                var appointments = await _vapiService.GetUpcomingAppointmentsAsync(clientId, 30);
                var summary = _vapiService.GetAppointmentSummaryForVoice(appointments);
                
                Console.WriteLine($"Appointment Summary: {summary}");
                return summary;
            }
            catch (VapiIntakeQException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Example: Get outstanding invoices for a client
        /// </summary>
        public async Task<List<InvoiceInfo>> GetOutstandingInvoicesExample(int clientId)
        {
            try
            {
                var invoices = await _vapiService.GetOutstandingInvoicesAsync(clientId);
                
                Console.WriteLine($"Found {invoices.Count} outstanding invoices:");
                foreach (var invoice in invoices)
                {
                    Console.WriteLine($"- Invoice {invoice.Number}: {invoice.AmountDue:C} due {invoice.DueDate:MMM dd, yyyy}");
                }
                
                return invoices;
            }
            catch (VapiIntakeQException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Example: Get invoice summary for voice response
        /// </summary>
        public async Task<string> GetInvoiceSummaryExample(int clientId)
        {
            try
            {
                var invoices = await _vapiService.GetOutstandingInvoicesAsync(clientId);
                var summary = _vapiService.GetInvoiceSummaryForVoice(invoices);
                
                Console.WriteLine($"Invoice Summary: {summary}");
                return summary;
            }
            catch (VapiIntakeQException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Example: Create a new appointment
        /// </summary>
        public async Task<AppointmentInfo> CreateAppointmentExample(int clientId)
        {
            try
            {
                var request = new CreateAppointmentRequest
                {
                    ClientId = clientId,
                    ServiceId = "service-id-here",
                    LocationId = "location-id-here",
                    PractitionerId = "practitioner-id-here",
                    StartDateTime = DateTime.Now.AddDays(7), // One week from now
                    Status = "confirmed",
                    ClientNote = "Appointment created via VAPI",
                    PractitionerNote = "Voice agent booking",
                    SendClientEmailNotification = true,
                    ReminderType = "email"
                };

                var appointment = await _vapiService.CreateAppointmentAsync(request);
                
                Console.WriteLine($"Created appointment: {appointment.Id} for {appointment.StartDate:MMM dd, yyyy}");
                return appointment;
            }
            catch (VapiIntakeQException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Example: Complete voice agent workflow
        /// </summary>
        public async Task<string> CompleteVoiceWorkflowExample(string phoneNumber)
        {
            try
            {
                // Step 1: Find client by phone
                var client = await _vapiService.FindClientByPhoneAsync(phoneNumber);
                if (client == null)
                {
                    return "I couldn't find a client with that phone number. Please check the number and try again.";
                }

                // Step 2: Get client summary
                var clientSummary = await _vapiService.GetClientSummaryForVoiceAsync(client.ClientId);
                
                // Step 3: Get upcoming appointments
                var appointments = await _vapiService.GetUpcomingAppointmentsAsync(client.ClientId, 30);
                var appointmentSummary = _vapiService.GetAppointmentSummaryForVoice(appointments);
                
                // Step 4: Get outstanding invoices
                var invoices = await _vapiService.GetOutstandingInvoicesAsync(client.ClientId);
                var invoiceSummary = _vapiService.GetInvoiceSummaryForVoice(invoices);
                
                // Combine all information
                var fullSummary = $"{clientSummary}. {appointmentSummary}. {invoiceSummary}";
                
                Console.WriteLine($"Complete Summary: {fullSummary}");
                return fullSummary;
            }
            catch (VapiIntakeQException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return $"I'm sorry, I encountered an error: {ex.Message}";
            }
        }
    }
}
