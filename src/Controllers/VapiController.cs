using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IntakeQ.ApiClient.Services;
using IntakeQ.ApiClient.Models;

namespace IntakeQ.ApiClient.Controllers
{
    /// <summary>
    /// API Controller for VAPI voice agent integration with IntakeQ
    /// </summary>
    [ApiController]
    [Route("api/vapi")]
    public class VapiController : ControllerBase
    {
        private readonly VapiIntakeQService _vapiService;

        public VapiController(VapiIntakeQService vapiService)
        {
            _vapiService = vapiService;
        }

        #region Client Operations

        /// <summary>
        /// Search for clients by name, email, or phone
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <returns>List of matching clients</returns>
        [HttpGet("clients/search")]
        public async Task<ActionResult<List<ClientSearchResult>>> SearchClients(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return BadRequest("Search term is required");
                }

                var clients = await _vapiService.SearchClientsAsync(searchTerm);
                return Ok(clients);
            }
            catch (VapiIntakeQException ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Find client by phone number
        /// </summary>
        /// <param name="phoneNumber">Phone number to search for</param>
        /// <returns>Matching client or 404 if not found</returns>
        [HttpGet("clients/by-phone")]
        public async Task<ActionResult<ClientSearchResult>> FindClientByPhone(string phoneNumber)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(phoneNumber))
                {
                    return BadRequest("Phone number is required");
                }

                var client = await _vapiService.FindClientByPhoneAsync(phoneNumber);
                if (client == null)
                {
                    return NotFound("Client not found");
                }

                return Ok(client);
            }
            catch (VapiIntakeQException ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Get detailed client profile
        /// </summary>
        /// <param name="clientId">Client ID</param>
        /// <returns>Client profile</returns>
        [HttpGet("clients/{clientId}/profile")]
        public async Task<ActionResult<ClientProfile>> GetClientProfile(int clientId)
        {
            try
            {
                var profile = await _vapiService.GetClientProfileAsync(clientId);
                return Ok(profile);
            }
            catch (VapiIntakeQException ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Get client summary for voice response
        /// </summary>
        /// <param name="clientId">Client ID</param>
        /// <returns>Natural language client summary</returns>
        [HttpGet("clients/{clientId}/summary")]
        public async Task<ActionResult<string>> GetClientSummary(int clientId)
        {
            try
            {
                var summary = await _vapiService.GetClientSummaryForVoiceAsync(clientId);
                return Ok(new { summary });
            }
            catch (VapiIntakeQException ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        #endregion

        #region Appointment Operations

        /// <summary>
        /// Get appointments for a client
        /// </summary>
        /// <param name="clientId">Client ID</param>
        /// <param name="startDate">Start date (optional)</param>
        /// <param name="endDate">End date (optional)</param>
        /// <returns>List of appointments</returns>
        [HttpGet("clients/{clientId}/appointments")]
        public async Task<ActionResult<List<AppointmentInfo>>> GetClientAppointments(
            int clientId, 
            DateTime? startDate = null, 
            DateTime? endDate = null)
        {
            try
            {
                var appointments = await _vapiService.GetClientAppointmentsAsync(clientId, startDate, endDate);
                return Ok(appointments);
            }
            catch (VapiIntakeQException ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Get upcoming appointments for a client
        /// </summary>
        /// <param name="clientId">Client ID</param>
        /// <param name="daysAhead">Number of days to look ahead (default 30)</param>
        /// <returns>List of upcoming appointments</returns>
        [HttpGet("clients/{clientId}/appointments/upcoming")]
        public async Task<ActionResult<List<AppointmentInfo>>> GetUpcomingAppointments(int clientId, int daysAhead = 30)
        {
            try
            {
                var appointments = await _vapiService.GetUpcomingAppointmentsAsync(clientId, daysAhead);
                return Ok(appointments);
            }
            catch (VapiIntakeQException ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Get appointment summary for voice response
        /// </summary>
        /// <param name="clientId">Client ID</param>
        /// <param name="daysAhead">Number of days to look ahead (default 30)</param>
        /// <returns>Natural language appointment summary</returns>
        [HttpGet("clients/{clientId}/appointments/summary")]
        public async Task<ActionResult<string>> GetAppointmentSummary(int clientId, int daysAhead = 30)
        {
            try
            {
                var appointments = await _vapiService.GetUpcomingAppointmentsAsync(clientId, daysAhead);
                var summary = _vapiService.GetAppointmentSummaryForVoice(appointments);
                return Ok(new { summary });
            }
            catch (VapiIntakeQException ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Create a new appointment
        /// </summary>
        /// <param name="request">Appointment creation request</param>
        /// <returns>Created appointment</returns>
        [HttpPost("appointments")]
        public async Task<ActionResult<AppointmentInfo>> CreateAppointment([FromBody] CreateAppointmentRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Appointment request is required");
                }

                var appointment = await _vapiService.CreateAppointmentAsync(request);
                return CreatedAtAction(nameof(GetClientAppointments), new { clientId = request.ClientId }, appointment);
            }
            catch (VapiIntakeQException ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        #endregion

        #region Invoice Operations

        /// <summary>
        /// Get invoices for a client
        /// </summary>
        /// <param name="clientId">Client ID</param>
        /// <returns>List of client invoices</returns>
        [HttpGet("clients/{clientId}/invoices")]
        public async Task<ActionResult<List<InvoiceInfo>>> GetClientInvoices(int clientId)
        {
            try
            {
                var invoices = await _vapiService.GetClientInvoicesAsync(clientId);
                return Ok(invoices);
            }
            catch (VapiIntakeQException ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Get outstanding invoices for a client
        /// </summary>
        /// <param name="clientId">Client ID</param>
        /// <returns>List of outstanding invoices</returns>
        [HttpGet("clients/{clientId}/invoices/outstanding")]
        public async Task<ActionResult<List<InvoiceInfo>>> GetOutstandingInvoices(int clientId)
        {
            try
            {
                var invoices = await _vapiService.GetOutstandingInvoicesAsync(clientId);
                return Ok(invoices);
            }
            catch (VapiIntakeQException ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Get invoice summary for voice response
        /// </summary>
        /// <param name="clientId">Client ID</param>
        /// <returns>Natural language invoice summary</returns>
        [HttpGet("clients/{clientId}/invoices/summary")]
        public async Task<ActionResult<string>> GetInvoiceSummary(int clientId)
        {
            try
            {
                var invoices = await _vapiService.GetOutstandingInvoicesAsync(clientId);
                var summary = _vapiService.GetInvoiceSummaryForVoice(invoices);
                return Ok(new { summary });
            }
            catch (VapiIntakeQException ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        #endregion

        #region VAPI Webhook Endpoints

        /// <summary>
        /// VAPI webhook endpoint for handling voice agent requests
        /// </summary>
        /// <param name="request">VAPI webhook request</param>
        /// <returns>Response for VAPI</returns>
        [HttpPost("webhook")]
        public async Task<ActionResult<VapiWebhookResponse>> HandleVapiWebhook([FromBody] VapiWebhookRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.Message))
                {
                    return BadRequest("Invalid webhook request");
                }

                var response = await ProcessVapiRequest(request);
                return Ok(response);
            }
            catch (VapiIntakeQException ex)
            {
                return StatusCode(500, new VapiWebhookResponse 
                { 
                    Message = $"Error processing request: {ex.Message}" 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new VapiWebhookResponse 
                { 
                    Message = "Internal server error" 
                });
            }
        }

        private async Task<VapiWebhookResponse> ProcessVapiRequest(VapiWebhookRequest request)
        {
            var message = request.Message.ToLowerInvariant();
            
            // Simple intent detection based on keywords
            if (message.Contains("search") || message.Contains("find") || message.Contains("look up"))
            {
                return await HandleClientSearchRequest(request);
            }
            else if (message.Contains("appointment") || message.Contains("schedule"))
            {
                return await HandleAppointmentRequest(request);
            }
            else if (message.Contains("invoice") || message.Contains("bill") || message.Contains("payment"))
            {
                return await HandleInvoiceRequest(request);
            }
            else
            {
                return new VapiWebhookResponse 
                { 
                    Message = "I can help you search for clients, manage appointments, or check invoices. What would you like to do?" 
                };
            }
        }

        private async Task<VapiWebhookResponse> HandleClientSearchRequest(VapiWebhookRequest request)
        {
            // Extract search term from the message
            var searchTerm = ExtractSearchTerm(request.Message);
            
            if (string.IsNullOrEmpty(searchTerm))
            {
                return new VapiWebhookResponse 
                { 
                    Message = "Please provide a name, email, or phone number to search for." 
                };
            }

            var clients = await _vapiService.SearchClientsAsync(searchTerm);
            
            if (!clients.Any())
            {
                return new VapiWebhookResponse 
                { 
                    Message = "No clients found matching your search." 
                };
            }

            var response = $"Found {clients.Count} client{(clients.Count == 1 ? "" : "s")}: ";
            foreach (var client in clients.Take(3)) // Limit to 3 results for voice
            {
                if (!string.IsNullOrEmpty(client?.Name))
                {
                    response += $"{client.Name}, ";
                }
            }

            return new VapiWebhookResponse { Message = response.TrimEnd(' ', ',') };
        }

        private async Task<VapiWebhookResponse> HandleAppointmentRequest(VapiWebhookRequest request)
        {
            // This would need more sophisticated parsing in a real implementation
            return new VapiWebhookResponse 
            { 
                Message = "I can help you view or schedule appointments. Please provide the client ID or phone number." 
            };
        }

        private async Task<VapiWebhookResponse> HandleInvoiceRequest(VapiWebhookRequest request)
        {
            // This would need more sophisticated parsing in a real implementation
            return new VapiWebhookResponse 
            { 
                Message = "I can help you check invoice information. Please provide the client ID or phone number." 
            };
        }

        private string ExtractSearchTerm(string message)
        {
            // Simple extraction - in a real implementation, you'd use NLP
            var words = message.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            // Look for patterns like "search for John" or "find client with phone 555-1234"
            for (int i = 0; i < words.Length - 1; i++)
            {
                if (words[i].Equals("for", StringComparison.OrdinalIgnoreCase) || 
                    words[i].Equals("with", StringComparison.OrdinalIgnoreCase))
                {
                    return string.Join(" ", words.Skip(i + 1));
                }
            }
            
            // If no pattern found, return the last few words
            return string.Join(" ", words.Skip(Math.Max(0, words.Length - 3)));
        }

        #endregion
    }

    #region VAPI Webhook Models

    public class VapiWebhookRequest
    {
        public string Message { get; set; }
        public string CallId { get; set; }
        public string PhoneNumber { get; set; }
        public Dictionary<string, object> Context { get; set; } = new Dictionary<string, object>();
    }

    public class VapiWebhookResponse
    {
        public string Message { get; set; }
        public Dictionary<string, object> Context { get; set; } = new Dictionary<string, object>();
    }

    #endregion
}
