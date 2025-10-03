﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using IntakeQ.ApiClient.Helpers;
using IntakeQ.ApiClient.Models;
using Newtonsoft.Json;
using System.IO;
using Client = IntakeQ.ApiClient.Models.Client;
using ClientProfile = IntakeQ.ApiClient.Models.ClientProfile;


namespace IntakeQ.ApiClient
{
    public class ApiClient
    {
        private readonly string _apiKey;
        private readonly string _baseUrl = "https://intakeq.com/api/v1/";
        private static readonly HttpClient _httpClient = new HttpClient();

        public ApiClient(string apiKey)
        {
            _apiKey = apiKey;
        }

        private HttpRequestMessage GetHttpMessage(string methodName, HttpMethod methodType)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(_baseUrl + methodName),
                Method = methodType,
            };
            request.Headers.Add("X-Auth-Key", _apiKey);
            return request;
        }
        
        public async Task<IEnumerable<IntakeSummary>> GetIntakesSummary(string clientSearch, DateTime? startDate = null, DateTime? endDate = null, int? clientId = null, string externalClientId = null, int? pageNumber = null, bool getAll = false)
        {
            var parameters = new NameValueCollection();
            if (!string.IsNullOrEmpty(clientSearch))
                parameters.Add("client", clientSearch);
            if (startDate.HasValue)
                parameters.Add("startDate", startDate.Value.ToString("yyyy-MM-dd"));
            if (endDate.HasValue)
                parameters.Add("endDate", endDate.Value.ToString("yyyy-MM-dd"));
            if (clientId.HasValue)
                parameters.Add("clientId", clientId.Value.ToString());
            if (!string.IsNullOrEmpty(externalClientId))
                parameters.Add("externalClientId", externalClientId);
            if (pageNumber.HasValue)
                parameters.Add("page", pageNumber.Value.ToString());
            if (getAll)
                parameters.Add("all", "true");

            var request = GetHttpMessage("intakes/summary" + parameters.ToQueryString(), HttpMethod.Get);
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var intakes = JsonConvert.DeserializeObject<IEnumerable<IntakeSummary>>(json);
                return intakes;
                }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }

        public async Task<Intake> GetFullIntake(string intakeId)
        {
            var request = GetHttpMessage($"intakes/{intakeId}", HttpMethod.Get);

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var intake = JsonConvert.DeserializeObject<Intake>(json);
                return intake;
                }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }
        
        public async Task<Intake> SendForm(SendForm sendForm)
        {
            var request = GetHttpMessage($"intakes/send", HttpMethod.Post);
            request.Content = new StringContent(JsonConvert.SerializeObject(sendForm), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Intake>(json);
                }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }
        
        public async Task<Intake> ResendForm(ResendForm resendForm)
        {
            var request = GetHttpMessage($"intakes/resend", HttpMethod.Post);
            request.Content = new StringContent(JsonConvert.SerializeObject(resendForm), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Intake>(json);
                }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }
        
        /// <summary>
        /// Creates a form, but doesn't send it to the client. 
        /// </summary>
        public async Task<Intake> CreateForm(SendForm sendForm)
        {
            var request = GetHttpMessage($"intakes/create", HttpMethod.Post);
                request.Content = new StringContent(JsonConvert.SerializeObject(sendForm), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Intake>(json);
                }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }
        
        public async Task<IntakeAuthToken> CreateFormAuthToken(string intakeId)
        {
            var request = GetHttpMessage($"intakes/{intakeId}/token", HttpMethod.Post);

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IntakeAuthToken>(json);
                }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }
        
        public async Task<byte[]> DownloadPdf(string id, bool isNote = false)
        {
            var requestType = (isNote) ? "notes" : "intakes";
            var request = GetHttpMessage($"{requestType}/{id}/pdf", HttpMethod.Get);

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
                {   
                var bytes = await response.Content.ReadAsByteArrayAsync();
                return bytes;
                }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }

        public async Task DownloadPdfAndSave(string id, string destinationPath, bool isNote = false)
        {
            var requestType = (isNote) ? "notes" : "intakes";
            var request = GetHttpMessage($"{requestType}/{id}/pdf", HttpMethod.Get);

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
                {
                        using (var contentStream = await response.Content.ReadAsStreamAsync())
                {
                    using (var stream = new FileStream(destinationPath, FileMode.CreateNew))
                    {
                        await contentStream.CopyToAsync(stream);
                    }
                }
            }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }

        public async Task<IEnumerable<Client>> GetClients(string search = null, int? pageNumber = null, DateTime? dateCreatedStart = null, DateTime? dateCreatedEnd = null, Dictionary<string, string> customFields = null, string externalClientId = null)
        {
            var parameters = new NameValueCollection();
            if (!string.IsNullOrEmpty(search))
                parameters.Add("search", search);
            if (pageNumber.HasValue)
                parameters.Add("page", pageNumber.Value.ToString());
            if (dateCreatedStart.HasValue)
                parameters.Add("dateCreatedStart", dateCreatedStart.Value.ToString("yyyy-MM-dd"));
            if (dateCreatedEnd.HasValue)
                parameters.Add("dateCreatedEnd", dateCreatedEnd.Value.ToString("yyyy-MM-dd"));
            if (!string.IsNullOrEmpty(externalClientId))
                parameters.Add("externalClientId", externalClientId);

            if (customFields?.Count > 0)
            {
                foreach (var field in customFields)
                {
                    parameters.Add($"custom[{field.Key}]", field.Value);
                }
            }
                
            var request = GetHttpMessage("clients" + parameters.ToQueryString(), HttpMethod.Get);
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                    var clients = JsonConvert.DeserializeObject<IEnumerable<Client>>(json);
                    return clients;
                }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }
        
         public async Task<ClientProfile> GetClientProfile(int clientId)
        {
            var request = GetHttpMessage($"clients/profile/{clientId}", HttpMethod.Get);
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ClientProfile>(json);
                }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }

        public async Task<IEnumerable<ClientProfile>> GetClientsWithProfile(string search = null, int? pageNumber = null, DateTime? dateCreatedStart = null, DateTime? dateCreatedEnd = null, Dictionary<string, string> customFields = null, string externalClientId = null)
        {
            var parameters = new NameValueCollection();
            if (!string.IsNullOrEmpty(search))
                parameters.Add("search", search);
            if (pageNumber.HasValue)
                parameters.Add("page", pageNumber.Value.ToString());
            parameters.Add("includeProfile", "true");
            if (dateCreatedStart.HasValue)
                parameters.Add("dateCreatedStart", dateCreatedStart.Value.ToString("yyyy-MM-dd"));
            if (dateCreatedEnd.HasValue)
                parameters.Add("dateCreatedEnd", dateCreatedEnd.Value.ToString("yyyy-MM-dd"));
            if (!string.IsNullOrEmpty(externalClientId))
                parameters.Add("externalClientId", externalClientId);

            if (customFields?.Count > 0)
            {
                foreach (var field in customFields)
                {
                    parameters.Add($"custom[{field.Key}]", field.Value);
                }
            }
                
            var request = GetHttpMessage("clients" + parameters.ToQueryString(), HttpMethod.Get);
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                    var clients = JsonConvert.DeserializeObject<IEnumerable<ClientProfile>>(json);
                    return clients;
                }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }
        
        public async Task<ClientProfile> SaveClient(ClientProfile clientProfile)
        {
            var request = GetHttpMessage($"clients", HttpMethod.Post);
                request.Content = new StringContent(JsonConvert.SerializeObject(clientProfile), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ClientProfile>(json);
                }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }

        public async Task<byte[]> DownloadAttachment(string attachmentId)
        {
            var request = GetHttpMessage($"attachments/{attachmentId}", HttpMethod.Get);

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var bytes = await response.Content.ReadAsByteArrayAsync();
                return bytes;
                }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }

        public async Task DownloadAttachmentAndSave(string attachmentId, string destinationPath)
        {
            var request = GetHttpMessage($"attachments/{attachmentId}", HttpMethod.Get);

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
                {
                        using (var contentStream = await response.Content.ReadAsStreamAsync())
                {
                    using (var stream = new FileStream(destinationPath, FileMode.CreateNew))
                    {
                        await contentStream.CopyToAsync(stream);
                    }
                }
            }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }

        public async Task<Intake> UpdateOfficeUseAnswers(Intake intake)
        {
            //Let's clean the intake object to keep only the data we need to send
            var cleanIntake = new Intake()
            {
                Id = intake.Id,
                Questions = intake.Questions.Where(x => x.OfficeUse).ToList()
            };

            var request = GetHttpMessage($"intakes", HttpMethod.Post);
            request.Content = new StringContent(JsonConvert.SerializeObject(cleanIntake), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Intake>(json);
                }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }

        public async Task<IEnumerable<Appointment>> GetAppointments(DateTime? startDate = null, DateTime? endDate = null, string status = null, string clientSearch = null, string practitionerEmail = null, int? pageNumber = null)
        {
            var parameters = new NameValueCollection();
            if (!string.IsNullOrEmpty(clientSearch))
                parameters.Add("client", clientSearch);
            if (!string.IsNullOrEmpty(status))
                parameters.Add("status", status);
            if (!string.IsNullOrEmpty(practitionerEmail))
                parameters.Add("practitionerEmail", practitionerEmail);
            if (startDate.HasValue)
                parameters.Add("startDate", startDate.Value.ToString("yyyy-MM-dd"));
            if (endDate.HasValue)
                parameters.Add("endDate", endDate.Value.ToString("yyyy-MM-dd"));
            if (pageNumber.HasValue)
                parameters.Add("page", pageNumber.Value.ToString());

            var request = GetHttpMessage("appointments" + parameters.ToQueryString(), HttpMethod.Get);
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                    var appointments = JsonConvert.DeserializeObject<IEnumerable<Appointment>>(json);
                    return appointments;
            }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }

        public async Task<Appointment> GetAppointment(string appointmentId)
        {
            var request = GetHttpMessage($"appointments/{appointmentId}", HttpMethod.Get);

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                    var appointment = JsonConvert.DeserializeObject<Appointment>(json);
                    return appointment;
                }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }

        public async Task<AppointmentSettings> GetAppointmentSettings()
        {
            var request = GetHttpMessage($"appointments/settings", HttpMethod.Get);

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                    var settings = JsonConvert.DeserializeObject<AppointmentSettings>(json);
                    return settings;
                }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }
        
        public async Task<IEnumerable<Questionnaire>> GetQuestionnaires()
        {
            var request = GetHttpMessage("questionnaires", HttpMethod.Get);
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var questionnaires = JsonConvert.DeserializeObject<IEnumerable<Questionnaire>>(json);
                return questionnaires;
                }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }
        
        public async Task<IEnumerable<Invoice>> GetInvoicesByClient(int clientId)
        {
            var parameters = new NameValueCollection();
            parameters.Add("clientId", clientId.ToString());

            var request = GetHttpMessage("invoices" + parameters.ToQueryString(), HttpMethod.Get);

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<IEnumerable<Invoice>>(json);
                    return result;
                }
                else
                {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(error);
                }
        }
        
        public async Task<Appointment> CreateAppointment(CreateAppointmentDto dto)
        {
            var request = GetHttpMessage($"appointments", HttpMethod.Post);
                request.Content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Appointment>(json);
            }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }
        
        public async Task<Appointment> UpdateAppointment(UpdateAppointmentDto dto)
        {
            var request = GetHttpMessage($"appointments", HttpMethod.Put);
                request.Content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Appointment>(json);
            }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }

        public async Task<IEnumerable<Models.File>> GetFilesByClient(string clientId)
        {
            var parameters = new NameValueCollection();
            parameters.Add("clientId", clientId);

            var request = GetHttpMessage("files" + parameters.ToQueryString(), HttpMethod.Get);

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<IEnumerable<Models.File>>(json);
                return result;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(error);
            }
        }

        public async Task<byte[]> GetFile(string id)
        {
            var request = GetHttpMessage($"files/{id}", HttpMethod.Get);

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var bytes = await response.Content.ReadAsByteArrayAsync();
                return bytes;
                }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }

        public async Task<IEnumerable<Folder>> GetFolders()
        {
            var request = GetHttpMessage("folders" , HttpMethod.Get);

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<IEnumerable<Folder>>(json);
                    return result;
                }
                else
                {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(error);
                }
        }

        public async Task UploadFile(string clientId, byte[] fileData, string fileName, string contentType)
        {
            using (var content = new MultipartFormDataContent())
            {
                using (var streamContent = new StreamContent(new MemoryStream(fileData)))
                {
                    streamContent.Headers.Add("Content-Type", "application/octet-stream");
                    streamContent.Headers.Add("Content-Disposition", "form-data; name=\"file\"; filename=\"" + fileName + "\"");
                    streamContent.Headers.ContentType.MediaType = contentType;

                    content.Add(streamContent, "file", fileName);

                    HttpResponseMessage response = await _httpClient.PostAsync(new Uri($"{_baseUrl}files/{clientId}"), content);

                    if (!response.IsSuccessStatusCode)
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        throw new HttpRequestException(error);
                    }
                }
            }
        }

        public async Task DeleteFile(string id)
        {
            var request = GetHttpMessage($"files/{id}", HttpMethod.Delete);

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(error);
            }
        }
        
        public async Task<IEnumerable<Practitioner>> ListPractitioners(bool includeInactive = false)
        {
            var request = GetHttpMessage($"practitioners?includeInactive={includeInactive}", HttpMethod.Get);

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<IEnumerable<Practitioner>>(json);
                return result;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(error);
            }
        }

        public async Task<Practitioner> CreatePractitioner(Practitioner practitioner)
        {
            var request = GetHttpMessage($"practitioners", HttpMethod.Post);
            request.Content = new StringContent(JsonConvert.SerializeObject(practitioner), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Practitioner>(json);
            }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }
        
        public async Task<Practitioner> UpdatePractitioner(Practitioner practitioner)
        {
            var request = GetHttpMessage($"practitioners", HttpMethod.Put);
                request.Content = new StringContent(JsonConvert.SerializeObject(practitioner), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Practitioner>(json);
            }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }
        
        public async Task DisablePractitioner(string id)
        {
            var request = GetHttpMessage($"practitioners/{id}/disable", HttpMethod.Post);

                HttpResponseMessage response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }
        
        public async Task EnablePractitioner(string id)
        {
            var request = GetHttpMessage($"practitioners/{id}/enable", HttpMethod.Post);

                HttpResponseMessage response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }
        
        public async Task DeletePractitioner(string id)
        {
            var request = GetHttpMessage($"practitioners/{id}", HttpMethod.Delete);

                HttpResponseMessage response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }
        
        public async Task TransferPractitionerData(string sourcePractitionerId, string destinationPractitionerId)
        {
            var request = GetHttpMessage($"practitioners/{sourcePractitionerId}/transferData/{destinationPractitionerId}", HttpMethod.Post);

                HttpResponseMessage response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }
        
        public async Task TransferClientOwnership(string sourcePractitionerId, string destinationPractitionerId)
        {
            var request = GetHttpMessage($"practitioners/{sourcePractitionerId}/transferClientOwnership/{destinationPractitionerId}", HttpMethod.Post);

                HttpResponseMessage response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }
        
        public async Task<IEnumerable<Assistant>> ListAssistants()
        {
            var request = GetHttpMessage("assistants", HttpMethod.Get);

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<IEnumerable<Assistant>>(json);
                return result;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(error);
            }
        }
        
        public async Task<Assistant> CreateAssistant(Assistant assistant)
        {
            var request = GetHttpMessage($"assistants", HttpMethod.Post);
            request.Content = new StringContent(JsonConvert.SerializeObject(assistant), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Assistant>(json);
            }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }
        
        public async Task<Assistant> UpdateAssistant(Assistant assistant)
        {
            var request = GetHttpMessage($"assistants", HttpMethod.Put);
                request.Content = new StringContent(JsonConvert.SerializeObject(assistant), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Assistant>(json);
            }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }
        
        public async Task DeleteAssistant(string id)
        {
            var request = GetHttpMessage($"assistants/{id}", HttpMethod.Delete);
                

                HttpResponseMessage response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }

        public async Task<IEnumerable<TreatmentNoteSummary>> GetNotesSummary(string clientSearch = "", DateTime? startDate = null, DateTime? endDate = null, int? clientId = null, int? pageNumber = null, int? status = null)
        {
            var parameters = new NameValueCollection();
                if (!string.IsNullOrEmpty(clientSearch))
                    parameters.Add("client", clientSearch);
                if (startDate.HasValue)
                    parameters.Add("startDate", startDate.Value.ToString("yyyy-MM-dd"));
                if (endDate.HasValue)
                    parameters.Add("endDate", endDate.Value.ToString("yyyy-MM-dd"));
                if (clientId.HasValue)
                    parameters.Add("clientId", clientId.Value.ToString());
            if (pageNumber.HasValue)
                parameters.Add("page", pageNumber.Value.ToString());
            if (status.HasValue)
                parameters.Add("status", status.Value.ToString());

            var request = GetHttpMessage("notes/summary" + parameters.ToQueryString(), HttpMethod.Get);
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                    var notes = JsonConvert.DeserializeObject<IEnumerable<TreatmentNoteSummary>>(json);
                    return notes;
            }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }

        public async Task<TreatmentNote> GetFullNote(string noteId)
        {
            var request = GetHttpMessage($"notes/{noteId}", HttpMethod.Get);

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                    var note = JsonConvert.DeserializeObject<TreatmentNote>(json);
                    return note;
            }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }
    }
}
