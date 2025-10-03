# VAPI Voice Agent Integration with IntakeQ

This integration allows VAPI voice agents to interact with IntakeQ to look up clients, manage appointments, and query invoice information through natural voice conversations.

## Features

- **Client Lookup**: Search for clients by name, email, or phone number
- **Appointment Management**: View upcoming appointments and create new ones
- **Invoice Queries**: Check outstanding balances and invoice details
- **Voice-Optimized Responses**: Natural language summaries for voice interactions
- **Webhook Integration**: Direct integration with VAPI webhooks

## Quick Start

### 1. Install Dependencies

```bash
# Add to your project
dotnet add package IntakeQ.ApiClient
```

### 2. Configure Services

```csharp
// In Program.cs or Startup.cs
using IntakeQ.ApiClient.Extensions;

// Option 1: Using configuration
builder.Services.AddIntakeQVapiServices(builder.Configuration);

// Option 2: Using explicit API keys
builder.Services.AddIntakeQVapiServices("your-api-key", "partner-api-key");
```

### 3. Configuration (appsettings.json)

```json
{
  "IntakeQ": {
    "ApiKey": "your-intakeq-api-key",
    "PartnerApiKey": "your-partner-api-key" // Optional
  }
}
```

## API Endpoints

### Client Operations

#### Search Clients
```http
GET /api/vapi/clients/search?searchTerm=John Doe
```

#### Find Client by Phone
```http
GET /api/vapi/clients/by-phone?phoneNumber=555-123-4567
```

#### Get Client Profile
```http
GET /api/vapi/clients/{clientId}/profile
```

#### Get Client Summary (Voice-Optimized)
```http
GET /api/vapi/clients/{clientId}/summary
```

### Appointment Operations

#### Get Client Appointments
```http
GET /api/vapi/clients/{clientId}/appointments?startDate=2024-01-01&endDate=2024-12-31
```

#### Get Upcoming Appointments
```http
GET /api/vapi/clients/{clientId}/appointments/upcoming?daysAhead=30
```

#### Get Appointment Summary (Voice-Optimized)
```http
GET /api/vapi/clients/{clientId}/appointments/summary
```

#### Create Appointment
```http
POST /api/vapi/appointments
Content-Type: application/json

{
  "clientId": 123,
  "serviceId": "service-123",
  "locationId": "location-123",
  "practitionerId": "practitioner-123",
  "startDateTime": "2024-01-15T10:00:00Z",
  "status": "confirmed",
  "clientNote": "Voice agent booking",
  "sendClientEmailNotification": true,
  "reminderType": "email"
}
```

### Invoice Operations

#### Get Client Invoices
```http
GET /api/vapi/clients/{clientId}/invoices
```

#### Get Outstanding Invoices
```http
GET /api/vapi/clients/{clientId}/invoices/outstanding
```

#### Get Invoice Summary (Voice-Optimized)
```http
GET /api/vapi/clients/{clientId}/invoices/summary
```

## VAPI Webhook Integration

### Webhook Endpoint
```http
POST /api/vapi/webhook
Content-Type: application/json

{
  "message": "Find client with phone number 555-123-4567",
  "callId": "call-123",
  "phoneNumber": "+15551234567",
  "context": {}
}
```

### Supported Voice Commands

The webhook supports natural language processing for:

- **Client Search**: "Find client John Smith", "Search for phone 555-123-4567"
- **Appointment Queries**: "Show my appointments", "When is my next appointment?"
- **Invoice Queries**: "What's my balance?", "Show my outstanding invoices"

## Usage Examples

### Basic Client Lookup

```csharp
using IntakeQ.ApiClient.Services;

var vapiService = new VapiIntakeQService("your-api-key");

// Search by phone number
var client = await vapiService.FindClientByPhoneAsync("555-123-4567");
if (client != null)
{
    Console.WriteLine($"Found: {client.Name} ({client.Email})");
}

// Get client summary for voice response
var summary = await vapiService.GetClientSummaryForVoiceAsync(client.ClientId);
Console.WriteLine(summary);
// Output: "Client: John Smith, Age: 35, Phone: 555-123-4567. Next appointment: January 15, 2024 at 10:00 AM with Dr. Johnson. Outstanding balance: $150.00"
```

### Appointment Management

```csharp
// Get upcoming appointments
var appointments = await vapiService.GetUpcomingAppointmentsAsync(clientId, 30);

// Get appointment summary for voice
var appointmentSummary = vapiService.GetAppointmentSummaryForVoice(appointments);
Console.WriteLine(appointmentSummary);
// Output: "Found 2 appointments: January 15, 2024 at 10:00 AM with Dr. Johnson for Consultation. January 22, 2024 at 2:00 PM with Dr. Smith for Follow-up."

// Create new appointment
var request = new CreateAppointmentRequest
{
    ClientId = clientId,
    ServiceId = "service-123",
    LocationId = "location-123",
    PractitionerId = "practitioner-123",
    StartDateTime = DateTime.Now.AddDays(7),
    Status = "confirmed",
    SendClientEmailNotification = true
};

var appointment = await vapiService.CreateAppointmentAsync(request);
```

### Invoice Queries

```csharp
// Get outstanding invoices
var invoices = await vapiService.GetOutstandingInvoicesAsync(clientId);

// Get invoice summary for voice
var invoiceSummary = vapiService.GetInvoiceSummaryForVoice(invoices);
Console.WriteLine(invoiceSummary);
// Output: "Found 2 invoices with a total outstanding balance of $150.00. Invoice 1001 due January 10, 2024 for $75.00. Invoice 1002 due January 20, 2024 for $75.00."
```

## VAPI Configuration

### 1. Create VAPI Assistant

In your VAPI dashboard, create a new assistant with:

- **Name**: IntakeQ Voice Assistant
- **Model**: GPT-4 or similar
- **System Message**: "You are a helpful assistant for a healthcare practice. You can help clients with appointments, billing, and general inquiries."

### 2. Configure Webhook

Set your webhook URL to: `https://your-domain.com/api/vapi/webhook`

### 3. Example VAPI Assistant Configuration

```json
{
  "assistant": {
    "name": "IntakeQ Assistant",
    "model": {
      "provider": "openai",
      "model": "gpt-4",
      "temperature": 0.7,
      "maxTokens": 1000
    },
    "voice": {
      "provider": "elevenlabs",
      "voiceId": "voice-id-here"
    },
    "systemMessage": "You are a helpful assistant for a healthcare practice. You can help clients with appointments, billing, and general inquiries. When clients call, greet them warmly and ask how you can help them today.",
    "webhookUrl": "https://your-domain.com/api/vapi/webhook",
    "webhookSecret": "your-webhook-secret"
  }
}
```

## Error Handling

The integration includes comprehensive error handling:

```csharp
try
{
    var client = await vapiService.FindClientByPhoneAsync("555-123-4567");
}
catch (VapiIntakeQException ex)
{
    // Handle IntakeQ-specific errors
    Console.WriteLine($"IntakeQ Error: {ex.Message}");
}
catch (Exception ex)
{
    // Handle general errors
    Console.WriteLine($"General Error: {ex.Message}");
}
```

## Security Considerations

1. **API Key Security**: Store API keys securely using environment variables or Azure Key Vault
2. **Webhook Security**: Implement webhook signature verification
3. **Rate Limiting**: Implement rate limiting for API endpoints
4. **Data Privacy**: Ensure compliance with healthcare data regulations (HIPAA, etc.)

## Advanced Configuration

### Custom Configuration

```csharp
var config = new VapiIntakeQConfiguration
{
    ApiKey = "your-api-key",
    PartnerApiKey = "your-partner-api-key",
    DefaultAppointmentLookAheadDays = 30,
    MaxVoiceSearchResults = 3,
    EnableDetailedLogging = true
};
```

### Dependency Injection with Custom Configuration

```csharp
services.Configure<VapiIntakeQConfiguration>(configuration.GetSection("VapiIntakeQ"));
services.AddIntakeQVapiServices(configuration);
```

## Troubleshooting

### Common Issues

1. **API Key Invalid**: Verify your IntakeQ API key is correct and has proper permissions
2. **Client Not Found**: Check phone number format and ensure client exists in IntakeQ
3. **Webhook Not Responding**: Verify webhook URL is accessible and returns proper JSON
4. **Voice Response Issues**: Check that responses are formatted for voice (avoid special characters)

### Debugging

Enable detailed logging:

```csharp
var config = new VapiIntakeQConfiguration
{
    EnableDetailedLogging = true
};
```

## Support

For issues with this integration:

1. Check the IntakeQ API documentation: https://support.intakeq.com/intakeq-api
2. Review VAPI documentation: https://docs.vapi.ai/
3. Check application logs for detailed error messages

## License

This integration is provided under the same license as the IntakeQ.ApiClient library.
