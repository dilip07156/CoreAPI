using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace VGER_DISTRIBUTION.Proxy
{
    public class ServiceProxy
    {

        #region Private Variable Declaration

        HttpClient client;
        private IConfiguration _configuration;

        #endregion

        public ServiceProxy(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string WAPIBaseUrl
        {
            get
            {
                return _configuration.GetValue<string>("WAPIBaseUrl");
            }
        }

        public string ServiceBaseUrl
        {
            get
            {
                return _configuration.GetValue<string>("ServiceBaseUrl");
            }
        }

        public string DistributionServiceBaseUrl
        {
            get
            {
                return _configuration.GetValue<string>("DistributionServiceBaseUrl");
            }
        }

        public string BookingServiceBaseUrl
        {
            get
            {
                return _configuration.GetValue<string>("BookingServiceBaseUrl");
            }
        }

        public string BridgeServiceBaseUrl
        {
            get
            {
                return _configuration.GetValue<string>("BridgeServiceBaseUrl");
            }
        }

        public string TravelogixxBaseUrl {
            get
            {
                return _configuration.GetValue<string>("TravelogixxBaseUrl");
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

            string url = WAPIBaseUrl + URL;

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

        public async Task<dynamic> GetServiceData(string URL, Type ResponseType, string ticket = null)
        {
            client = new HttpClient();
            string url = BookingServiceBaseUrl + URL;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ticket);
            try
            {
                HttpResponseMessage responseMessage = await client.GetAsync(url);
                var responseJsonString = await responseMessage.Content.ReadAsStringAsync();
                return (dynamic)JsonConvert.DeserializeObject(responseJsonString, ResponseType);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<dynamic> PostData(string URL, object Param, Type ResponseType, string ticket, string ServiceType = null)
        {
            string url = "";
            var json = JsonConvert.SerializeObject(Param);

            client = new HttpClient();

            if (ServiceType == "Distribution")
                url = DistributionServiceBaseUrl + URL;
            else if (ServiceType == "Bridge")
                url = BridgeServiceBaseUrl + URL;
            else
                url = WAPIBaseUrl + URL;

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

        public async Task<dynamic> PostForTravelogiData(string URL, object Param, Type ResponseType, string ServiceType = null)
        {
            var json = JsonConvert.SerializeObject(Param);

            client = new HttpClient();

            string url = TravelogixxBaseUrl + URL;

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

    }
}
