using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace DBCommunicator.Proxy
{
    public class ServiceProxy
    {
        HttpClient client;
        private readonly IConfiguration _configuration;

        public ServiceProxy(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string ServiceBaseUrl
        {
            get
            {
                return _configuration.GetValue<string>("ServiceBaseUrl");
            }
        }

        public async Task<dynamic> GetData(string URL, Type ResponseType, string ticket)
        {
            client = new HttpClient();

            string url = ServiceBaseUrl + URL;

            var content = new StringContent("", Encoding.UTF8, "application/json");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ticket);

            //var response = await client.PostAsync(url, content);
            HttpResponseMessage responseMessage = await client.PostAsync(url, content);

            var responseJsonString = await responseMessage.Content.ReadAsStringAsync();

            try
            {
                return (dynamic)JsonConvert.DeserializeObject(responseJsonString, ResponseType);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<dynamic> PostData(string URL, object Param, Type ResponseType)
        {
            var json = JsonConvert.SerializeObject(Param);

            client = new HttpClient();

            string url = ServiceBaseUrl + URL;

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            //var response = await client.PostAsync(url, content);
            HttpResponseMessage responseMessage = await client.PostAsync(url, content);

            var responseJsonString = await responseMessage.Content.ReadAsStringAsync();

            try
            {
                return (dynamic)JsonConvert.DeserializeObject(responseJsonString, ResponseType);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<dynamic> PostData(string URL, object Param, Type ResponseType, string ticket)
        {
            var json = JsonConvert.SerializeObject(Param);

            client = new HttpClient();

            string url = ServiceBaseUrl + URL;

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ticket);

            //var response = await client.PostAsync(url, content);
            HttpResponseMessage responseMessage = await client.PostAsync(url, content);

            var responseJsonString = await responseMessage.Content.ReadAsStringAsync();

            try
            {
                return (dynamic)JsonConvert.DeserializeObject(responseJsonString, ResponseType);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
