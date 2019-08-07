using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace VGER_WAPI.Proxy
{
    public class ServiceProxy
    {
        HttpClient client;
        private IConfiguration _configuration;


        public ServiceProxy(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string MongoPushUrl
        {
            get
            {
                return _configuration.GetValue<string>("MongoPushUrl");
            }
        }

        public string BridgeServiceBaseUrl
        {
            get
            {
                return _configuration.GetValue<string>("BridgeServiceBaseUrl");
            }
        }

        public string IntegrationMSDynamicsUrl
        {
            get
            {
                return _configuration.GetValue<string>("IntegrationMSDynamicsUrl");
            }
        }

        public async Task<dynamic> PostData(string URL, object Param, Type ResponseType, string ServiceType = null)
        {
            var json = JsonConvert.SerializeObject(Param);

            client = new HttpClient();
            string url = "";

            if (ServiceType=="Bridge")
            {
                url = BridgeServiceBaseUrl + URL;
            }
            else if (ServiceType == "IntegrationMSDynamics")
            {
                url = IntegrationMSDynamicsUrl + URL;
            }
            else
            {
                url = MongoPushUrl + URL;
            } 

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

        public static void ServiceCall(string URL, object Param, string MongoPushUrl)
        {
            try
            {
                HttpClient client = new HttpClient();
                var json = JsonConvert.SerializeObject(Param);
                string url = MongoPushUrl + URL;
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                client.PostAsync(url, content);
                System.Threading.Thread.Sleep(1000);
            }
            catch (Exception ex)
            {

                throw;
            } 
        }

        public async Task<dynamic> PostData(string URL, object Param, Type ResponseType, string ticket, string ServiceType = null)
        {
            string url = "";
            var json = JsonConvert.SerializeObject(Param);

            client = new HttpClient();

            if (ServiceType == "Bridge")
            {
                url = BridgeServiceBaseUrl + URL;
            }
            else
            {
                url = MongoPushUrl + URL;
            }

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ticket);

            try
            {
                HttpResponseMessage responseMessage = await client.PostAsync(url, content);
                var responseJsonString = await responseMessage.Content.ReadAsStringAsync();
                return (dynamic)JsonConvert.DeserializeObject(responseJsonString, ResponseType);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}