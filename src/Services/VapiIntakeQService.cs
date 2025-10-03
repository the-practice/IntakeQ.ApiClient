using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntakeQ.ApiClient.Models;
using IntakeQ.ApiClient;

namespace IntakeQ.ApiClient.Services
{
    /// <summary>
    /// Service that integrates VAPI voice agent with IntakeQ API
    /// Provides functionality for client lookup, appointment scheduling, and invoice queries
    /// </summary>
    public class VapiIntakeQService
    {
        private readonly ApiClient _apiClient;
        private readonly PartnerApiClient _partnerApiClient;

        public VapiIntakeQService(string apiKey)
        {
            _apiClient = new ApiClient(apiKey);
        }

        public VapiIntakeQService(string apiKey, string partnerApiKey)
        {
            _apiClient = new ApiClient(apiKey);
            _partnerApiClient = new PartnerApiClient(partnerApiKey);
        }

        #region Client Lookup Operations

        /// <summary>
        /// Search for clients by name, email, or phone number
        /// </summary>
        /// <param name="searchTerm">Name, email, or phone to search for</param>
        /// <returns>List of matching clients</returns>
        public async Task<List<ClientSearchResult>> SearchClientsAsync(string searchTerm)
        {
            try
            {
                var clients = await _apiClient.GetClients(searchTerm);
                return clients.Select(c => new ClientSearchResult
                {
                    ClientId = c.ClientNumber,
                    Name = c.Name,
                    Email = c.Email,
                    Phone = c.Phone,
                    DisplayText = $"{c.Name} - {c.Email} - {c.Phone}"
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new VapiIntakeQException($"Error searching for clients: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get detailed client profile information
        /// </summary>
        /// <param name="clientId">Client ID</param>
        /// <returns>Detailed client profile</returns>
        public async Task<ClientProfile> GetClientProfileAsync(int clientId)
        {
            try
            {
                return await _apiClient.GetClientProfile(clientId);
            }
            catch (Exception ex)
            {
                throw new VapiIntakeQException($"Error retrieving client profile: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Find clients by phone number (useful for voice calls)
        /// </summary>
        /// <param name="phoneNumber">Phone number to search for</param>
        /// <returns>Matching client or null if not found</returns>
        public async Task<ClientSearchResult> FindClientByPhoneAsync(string phoneNumber)
        {
            try
            {
                // Use phone number as search term - the API will handle the lookup
                var clients = await _apiClient.GetClients(phoneNumber);
                var matchingClient = clients.FirstOrDefault();

                if (matchingClient != null)
                {
                    return new ClientSearchResult
                    {
                        ClientId = matchingClient.ClientNumber,
                        Name = matchingClient.Name,
                        Email = matchingClient.Email,
                        Phone = matchingClient.Phone,
                        DisplayText = $"{matchingClient.Name} - {matchingClient.Email}"
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new VapiIntakeQException($"Error finding client by phone: {ex.Message}", ex);
            }
        }

        #endregion

        #region Appointment Operations

        /// <summary>
        /// Get appointments for a specific client
        /// </summary>
        /// <param name="clientId">Client ID</param>
        /// <param name="startDate">Start date for search (optional)</param>
        /// <param name="endDate">End date for search (optional)</param>
        /// <returns>List of appointments</returns>
        public async Task<List<AppointmentInfo>> GetClientAppointmentsAsync(int clientId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var client = await _apiClient.GetClientProfile(clientId);
                var appointments = await _apiClient.GetAppointments(
                    startDate: startDate,
                    endDate: endDate,
                    clientSearch: client.Name
                );

                return appointments.Select(a => new AppointmentInfo
                {
                    Id = a.Id,
                    ClientName = a.ClientName,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    Status = a.Status,
                    ServiceName = a.ServiceName,
                    PractitionerName = a.PractitionerName,
                    LocationName = a.LocationName,
                    Duration = a.Duration,
                    Price = a.Price
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new VapiIntakeQException($"Error retrieving client appointments: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get upcoming appointments for a client
        /// </summary>
        /// <param name="clientId">Client ID</param>
        /// <param name="daysAhead">Number of days to look ahead (default 30)</param>
        /// <returns>List of upcoming appointments</returns>
        public async Task<List<AppointmentInfo>> GetUpcomingAppointmentsAsync(int clientId, int daysAhead = 30)
        {
            var startDate = DateTime.Now;
            var endDate = DateTime.Now.AddDays(daysAhead);
            return await GetClientAppointmentsAsync(clientId, startDate, endDate);
        }

        /// <summary>
        /// Create a new appointment
        /// </summary>
        /// <param name="request">Appointment creation request</param>
        /// <returns>Created appointment</returns>
        public async Task<AppointmentInfo> CreateAppointmentAsync(CreateAppointmentRequest request)
        {
            try
            {
                var dto = new CreateAppointmentDto
                {
                    ClientId = request.ClientId,
                    ServiceId = request.ServiceId,
                    LocationId = request.LocationId,
                    PractitionerId = request.PractitionerId,
                    UtcDateTime = ((DateTimeOffset)request.StartDateTime).ToUnixTimeSeconds(),
                    Status = request.Status ?? "confirmed",
                    ClientNote = request.ClientNote,
                    PractitionerNote = request.PractitionerNote,
                    SendClientEmailNotification = request.SendClientEmailNotification,
                    ReminderType = request.ReminderType ?? "email"
                };

                var appointment = await _apiClient.CreateAppointment(dto);
                
                return new AppointmentInfo
                {
                    Id = appointment.Id,
                    ClientName = appointment.ClientName,
                    StartDate = appointment.StartDate,
                    EndDate = appointment.EndDate,
                    Status = appointment.Status,
                    ServiceName = appointment.ServiceName,
                    PractitionerName = appointment.PractitionerName,
                    LocationName = appointment.LocationName,
                    Duration = appointment.Duration,
                    Price = appointment.Price
                };
            }
            catch (Exception ex)
            {
                throw new VapiIntakeQException($"Error creating appointment: {ex.Message}", ex);
            }
        }

        #endregion

        #region Invoice Operations

        /// <summary>
        /// Get invoices for a specific client
        /// </summary>
        /// <param name="clientId">Client ID</param>
        /// <returns>List of client invoices</returns>
        public async Task<List<InvoiceInfo>> GetClientInvoicesAsync(int clientId)
        {
            try
            {
                var invoices = await _apiClient.GetInvoicesByClient(clientId);
                
                return invoices.Select(i => new InvoiceInfo
                {
                    Id = i.Id,
                    Number = i.Number,
                    Status = i.Status,
                    ClientName = i.ClientName,
                    ClientEmail = i.ClientEmail,
                    TotalAmount = i.TotalAmount,
                    AmountDue = i.AmountDue,
                    AmountPaid = i.AmountPaid,
                    DueDate = DateTimeOffset.FromUnixTimeSeconds((long)i.DueDate).DateTime,
                    IssuedDate = DateTimeOffset.FromUnixTimeSeconds((long)i.IssuedDate).DateTime,
                    Currency = i.Currency,
                    Items = i.Items?.Select(item => new InvoiceItemInfo
                    {
                        Description = item.Description,
                        Price = item.Price,
                        Units = item.Units,
                        TotalAmount = item.TotalAmount
                    }).ToList() ?? new List<InvoiceItemInfo>()
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new VapiIntakeQException($"Error retrieving client invoices: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get outstanding invoices for a client
        /// </summary>
        /// <param name="clientId">Client ID</param>
        /// <returns>List of outstanding invoices</returns>
        public async Task<List<InvoiceInfo>> GetOutstandingInvoicesAsync(int clientId)
        {
            var allInvoices = await GetClientInvoicesAsync(clientId);
            return allInvoices.Where(i => i.AmountDue > 0).ToList();
        }

        #endregion

        #region Voice Agent Helper Methods

        /// <summary>
        /// Generate a natural language summary of client information for voice response
        /// </summary>
        /// <param name="clientId">Client ID</param>
        /// <returns>Natural language client summary</returns>
        public async Task<string> GetClientSummaryForVoiceAsync(int clientId)
        {
            try
            {
                var profile = await GetClientProfileAsync(clientId);
                var upcomingAppointments = await GetUpcomingAppointmentsAsync(clientId, 30);
                var outstandingInvoices = await GetOutstandingInvoicesAsync(clientId);

                var summary = $"Client: {profile.Name}";
                
                if (profile.DateOfBirth.HasValue)
                {
                    var age = DateTime.Now.Year - profile.DateOfBirth.Value.Year;
                    summary += $", Age: {age}";
                }

                if (!string.IsNullOrEmpty(profile.Phone))
                {
                    summary += $", Phone: {profile.Phone}";
                }

                if (upcomingAppointments.Any())
                {
                    var nextAppointment = upcomingAppointments.OrderBy(a => a.StartDate).First();
                    summary += $". Next appointment: {nextAppointment.StartDate:MMMM dd, yyyy at h:mm tt} with {nextAppointment.PractitionerName}";
                }

                if (outstandingInvoices.Any())
                {
                    var totalOutstanding = outstandingInvoices.Sum(i => i.AmountDue);
                    summary += $". Outstanding balance: {totalOutstanding:C}";
                }

                return summary;
            }
            catch (Exception ex)
            {
                throw new VapiIntakeQException($"Error generating client summary: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Generate appointment summary for voice response
        /// </summary>
        /// <param name="appointments">List of appointments</param>
        /// <returns>Natural language appointment summary</returns>
        public string GetAppointmentSummaryForVoice(List<AppointmentInfo> appointments)
        {
            if (!appointments.Any())
                return "No appointments found.";

            var summary = $"Found {appointments.Count} appointment{(appointments.Count == 1 ? "" : "s")}: ";
            
            foreach (var appointment in appointments.OrderBy(a => a.StartDate))
            {
                summary += $"{appointment.StartDate:MMMM dd, yyyy at h:mm tt} with {appointment.PractitionerName} for {appointment.ServiceName}. ";
            }

            return summary.Trim();
        }

        /// <summary>
        /// Generate invoice summary for voice response
        /// </summary>
        /// <param name="invoices">List of invoices</param>
        /// <returns>Natural language invoice summary</returns>
        public string GetInvoiceSummaryForVoice(List<InvoiceInfo> invoices)
        {
            if (!invoices.Any())
                return "No invoices found.";

            var totalOutstanding = invoices.Sum(i => i.AmountDue);
            var summary = $"Found {invoices.Count} invoice{(invoices.Count == 1 ? "" : "s")} with a total outstanding balance of {totalOutstanding:C}. ";

            foreach (var invoice in invoices.OrderBy(i => i.DueDate))
            {
                summary += $"Invoice {invoice.Number} due {invoice.DueDate:MMMM dd, yyyy} for {invoice.AmountDue:C}. ";
            }

            return summary.Trim();
        }

        #endregion

        #region Private Helper Methods

        private string CleanPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                return string.Empty;

            // Remove all non-digit characters
            return new string(phoneNumber.Where(char.IsDigit).ToArray());
        }

        #endregion
    }

    #region Supporting Classes

    public class ClientSearchResult
    {
        public int ClientId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string DisplayText { get; set; }
    }

    public class AppointmentInfo
    {
        public string Id { get; set; }
        public string ClientName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public string ServiceName { get; set; }
        public string PractitionerName { get; set; }
        public string LocationName { get; set; }
        public int Duration { get; set; }
        public double Price { get; set; }
    }

    public class CreateAppointmentRequest
    {
        public int ClientId { get; set; }
        public string ServiceId { get; set; }
        public string LocationId { get; set; }
        public string PractitionerId { get; set; }
        public DateTime StartDateTime { get; set; }
        public string Status { get; set; }
        public string ClientNote { get; set; }
        public string PractitionerNote { get; set; }
        public bool SendClientEmailNotification { get; set; } = true;
        public string ReminderType { get; set; }
    }

    public class InvoiceInfo
    {
        public string Id { get; set; }
        public int Number { get; set; }
        public string Status { get; set; }
        public string ClientName { get; set; }
        public string ClientEmail { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountDue { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime IssuedDate { get; set; }
        public string Currency { get; set; }
        public List<InvoiceItemInfo> Items { get; set; } = new List<InvoiceItemInfo>();
    }

    public class InvoiceItemInfo
    {
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal Units { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class VapiIntakeQException : Exception
    {
        public VapiIntakeQException(string message) : base(message) { }
        public VapiIntakeQException(string message, Exception innerException) : base(message, innerException) { }
    }

    #endregion
}
