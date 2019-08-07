using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI_CLASSES;
using VGER_DISTRIBUTION.Proxy;

namespace VGER_DISTRIBUTION.Providers
{
    public class AgentProviders
    {
        #region Private Variable Declaration

        private readonly IConfiguration _configuration;
        ServiceProxy serviceProxy;

        #endregion

        public AgentProviders(IConfiguration configuration)
        {
            _configuration = configuration;
            serviceProxy = new ServiceProxy(_configuration);
        }

        #region AgentInfo

        public async Task<ManageAgentRes> GetAgentDetailedInfo(ManageAgentReq agentReq, string ticket)
        {
            ManageAgentRes response = new ManageAgentRes();
            //response = await serviceProxy.PostData(_configuration.GetValue<string>("AgentService:GetAgentDetailedInfo"), agentReq, typeof(AgentGetRes), ticket);
            return response;
        }

        public async Task<PartnerCountryCityRes> GetPartnerCityDetails(Attributes request, string countryCode, string countryResortCode, string ticket)
        {
            PartnerCountryCityRes response = new PartnerCountryCityRes();
            response = await serviceProxy.PostData(_configuration.GetValue<string>("ServiceCommon:GetPartnerCityDetails"), request, typeof(PartnerCountryCityRes), ticket);
            if (response.ResponseStatus != null && !string.IsNullOrEmpty(response.ResponseStatus.Status) && response.ResponseStatus.Status.ToLower() == "failure")
            {
                var cityInfo = new List<TravelogiCountryCityReq>();
                TravelogiCountryCityReq d = new TravelogiCountryCityReq();
                d.SourceSupplierCode = _configuration.GetValue<string>("TravelogiData:SourceSupplierCode");
                d.SourceSupplierCityCode = request.Attribute_Id;
                d.SourceSupplierCountryCode = countryCode;
                d.TargetSupplierCode = _configuration.GetValue<string>("TravelogiData:TargetSupplierCode");
                cityInfo.Add(d);
                List<TravelogiCountryCityRes> responseData = await serviceProxy.PostForTravelogiData(_configuration.GetValue<string>("Travelogi:CityMapping"),
                                                                cityInfo, typeof(List<TravelogiCountryCityRes>));
                if (responseData != null && responseData.Any() && !string.IsNullOrEmpty(responseData[0].Status) && responseData[0].Status == "Mapped")
                {
                    TravelogiCountryCityRes TravelogiCityInfo = new TravelogiCountryCityRes();
                    TravelogiCityInfo = responseData[0];
                    TravelogiCityInfo.TargetSupplierCityName = TravelogiCityInfo.TargetSupplierCityName.Trim();
                    TravelogiCityInfo.TargetSupplierCountryCode = countryResortCode;

                    //Based on CityCode
                    /*response = await serviceProxy.PostData(_configuration.GetValue<string>("ServiceCommon:GetPartnerCityDetailsBasedOnCode"),
                        responseData[0].TargetSupplierCityCode, typeof(PartnerCountryCityRes), ticket);*/

                    response = await serviceProxy.PostData(_configuration.GetValue<string>("ServiceCommon:GetPartnerCityDetailsBasedOnName"),
                    TravelogiCityInfo, typeof(PartnerCountryCityRes), ticket);
                }
            }
            return response;
        }

        public async Task<PartnerCountryCityRes> GetPartnerCountryDetails(Attributes request, string ticket)
        {
            PartnerCountryCityRes response = new PartnerCountryCityRes();
            response = await serviceProxy.PostData(_configuration.GetValue<string>("ServiceCommon:GetPartnerCountryDetails"), request, typeof(PartnerCountryCityRes), ticket);
            if (response.ResponseStatus != null && !string.IsNullOrEmpty(response.ResponseStatus.Status) && response.ResponseStatus.Status.ToLower() == "failure")
            {
                var countryInfo = new List<TravelogiCountryCityReq>();
                TravelogiCountryCityReq d = new TravelogiCountryCityReq();
                d.SourceSupplierCode = _configuration.GetValue<string>("TravelogiData:SourceSupplierCode");
                d.SourceSupplierCountryCode = request.Attribute_Id;
                d.TargetSupplierCode = _configuration.GetValue<string>("TravelogiData:TargetSupplierCode");
                countryInfo.Add(d);
                List<TravelogiCountryCityRes> responseData = await serviceProxy.PostForTravelogiData(_configuration.GetValue<string>("Travelogi:CountryMapping"), 
                                                                countryInfo, typeof(List<TravelogiCountryCityRes>));
                if (responseData != null && responseData.Any() && !string.IsNullOrEmpty(responseData[0].Status) && responseData[0].Status == "Mapped")
                {
                    response = await serviceProxy.PostData(_configuration.GetValue<string>("ServiceCommon:GetPartnerCountryDetailsBasedOnCode"), 
                        responseData[0].TargetSupplierCountryCode, typeof(PartnerCountryCityRes), ticket);
                }
            }
            return response;
        }

        public async Task<ManageAgentRes> CreateUpdatePartnerAgentDetails(ManageAgentReq request, string ticket)
        {
            ManageAgentRes response = new ManageAgentRes();
            response = await serviceProxy.PostData(_configuration.GetValue<string>("AgentService:CreateUpdatePartnerAgentDetails"), request, typeof(ManageAgentRes), ticket);
            return response;
        }

        public async Task<AgentThirdPartyGetRes> GetPartnerAgentDetails(AgentThirdPartyGetReq request, string ticket)
        {
            AgentThirdPartyGetRes response = new AgentThirdPartyGetRes();
            response = await serviceProxy.PostData(_configuration.GetValue<string>("AgentService:GetPartnerAgentDetails"), request, typeof(AgentThirdPartyGetRes), ticket);
            return response;
        }

        public async Task<AgentThirdPartyGetRes> GetPartnerAgentContactDetails(AgentThirdPartyGetReq request, string ticket)
        {
            AgentThirdPartyGetRes response = new AgentThirdPartyGetRes();
            response = await serviceProxy.PostData(_configuration.GetValue<string>("AgentService:GetPartnerAgentContactDetails"), request, typeof(AgentThirdPartyGetRes), ticket);
            return response;
        }

        public async Task<AgentThirdPartyGetRes> CreatePartnerAgentContactDetails(ManageAgentContactReq request, string ticket)
        {
            AgentThirdPartyGetRes response = new AgentThirdPartyGetRes();
            response = await serviceProxy.PostData(_configuration.GetValue<string>("AgentService:CreatePartnerAgentContactDetails"), request, typeof(AgentThirdPartyGetRes), ticket);
            return response;
        }

        public async Task<AgentThirdPartyGetRes> UpdatePartnerAgentContactDetails(ManageAgentContactReq request, string ticket)
        {
            AgentThirdPartyGetRes response = new AgentThirdPartyGetRes();
            response = await serviceProxy.PostData(_configuration.GetValue<string>("AgentService:UpdatePartnerAgentContactDetails"), request, typeof(AgentThirdPartyGetRes), ticket);
            return response;
        }

        #endregion
    }
}
