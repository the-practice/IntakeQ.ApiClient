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
    public class PartnerApiClient
    {
        private readonly string _apiKey;
        private readonly string _baseUrl = "https://intakeq.com/api/partner/";
        private static readonly HttpClient _httpClient = new HttpClient();

        public PartnerApiClient(string apiKey)
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
        
        public async Task<IEnumerable<Practice>> GetPractices(int? pageNumber = null)
        {
            var parameters = new NameValueCollection();
            if (pageNumber.HasValue)
                parameters.Add("page", pageNumber.Value.ToString());

            var request = GetHttpMessage("practice" + parameters.ToQueryString(), HttpMethod.Get);
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var practices = JsonConvert.DeserializeObject<IEnumerable<Practice>>(json);
                return practices;
            }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }
        
        public async Task<Practice> GetPracticeById(string id)
        {
            var parameters = new NameValueCollection();
            parameters.Add("id", id);

            var request = GetHttpMessage("practice" + parameters.ToQueryString(), HttpMethod.Get);
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var practices = JsonConvert.DeserializeObject<List<Practice>>(json);
                return practices.FirstOrDefault();
            }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }
        
        public async Task<Practice> GetPracticeByExternalId(string externalPracticeId)
        {
            var parameters = new NameValueCollection();
            parameters.Add("externalPracticeId", externalPracticeId);

            var request = GetHttpMessage("practice" + parameters.ToQueryString(), HttpMethod.Get);
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var practices = JsonConvert.DeserializeObject<List<Practice>>(json);
                return practices.FirstOrDefault();
            }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }
   
        public async Task<Practice> CreatePractice(Practice practice)
        {
            var request = GetHttpMessage($"practice", HttpMethod.Post);
                request.Content = new StringContent(JsonConvert.SerializeObject(practice), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Practice>(json);
            }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }
        
        public async Task<Practice> UpdatePractice(Practice practice)
        {
            var request = GetHttpMessage($"practice", HttpMethod.Put);
            request.Content = new StringContent(JsonConvert.SerializeObject(practice), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Practice>(json);
            }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }

        public async Task<string> GetPracticeApiKey(string idOrExternalPracticeId)
        {
            var request = GetHttpMessage($"practice/{idOrExternalPracticeId}/key", HttpMethod.Get);

             HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(json);
                return result?.ApiKey;
            }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }
        
        public async Task<EphemeralToken> GetEphemeralToken(string idOrExternalPracticeId, string userId = null)
        {
            var parameters = new NameValueCollection();
            if (!string.IsNullOrEmpty(userId))
                parameters.Add("userId", userId);

            var request = GetHttpMessage($"practice/{idOrExternalPracticeId}/ephemeral" + parameters.ToQueryString(), HttpMethod.Get);

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<EphemeralToken>(json);
                return result;
            }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }
        
        public async Task<List<FormTemplate>> ListMasterForms()
        {
            var request = GetHttpMessage($"masterForms", HttpMethod.Get);

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<FormTemplate>>(json);
            }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }
        
        public async Task<List<FormTemplate>> ListPracticeForms(string practiceId)
        {
            var request = GetHttpMessage($"practice/{practiceId}/forms", HttpMethod.Get);

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<FormTemplate>>(json);
            }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }
        
        public async Task<FormTemplate> CopyForm(CopyFormOptions options)
        {
            var request = GetHttpMessage("practice/copyForm", HttpMethod.Post);
            request.Content = new StringContent(JsonConvert.SerializeObject(options), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<FormTemplate>(json);
            }
            else
            {
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            }
        }
    }
}
