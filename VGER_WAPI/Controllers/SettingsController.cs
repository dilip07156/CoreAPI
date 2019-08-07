using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VGER_WAPI.Helpers;
using VGER_WAPI.Models;
using VGER_WAPI.Repositories;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Controllers
{
	[Produces("application/json")]
	[Route("api/Settings")]
	public class SettingsController : Controller
    {
		#region Private Variable Declaration
		private readonly ISettingsRepository _settingsRepository;
		private readonly MongoContext _MongoContext = null;
        private readonly IMSDynamicsRepository _mSDynamicsRepository;
        #endregion

        public SettingsController(ISettingsRepository settingsRepository, IOptions<MongoSettings> settings, IMSDynamicsRepository mSDynamicsRepository)
		{
			_settingsRepository = settingsRepository;
			_MongoContext = new MongoContext(settings);
            _mSDynamicsRepository = mSDynamicsRepository;
		}

		[Authorize]
		[HttpPost]
		[Route("GetSalesPipelineRoles")]
		public async Task<SettingsGetRes> GetSalesPipelineRoles([FromBody] SettingsGetReq request)
		{
			var response = new SettingsGetRes();
			try
			{
				response = await _settingsRepository.GetSalesPipelineRoles(request);
				response.ResponseStatus.Status = "Success";
				response.ResponseStatus.ErrorMessage = response != null ? "" : "No Records Found.";
			}
			catch (Exception ex)
			{
				response.ResponseStatus.Status = "Failure";
				response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
			}

			return response;
		}

		[Authorize]
		[HttpPost]
		[Route("SetSalesPipelineRoles")]
		public async Task<SettingsSetRes> SetSalesPipelineRoles([FromBody] SettingsSetReq request)
		{
			var response = new SettingsSetRes();
			try
			{
				response = await _settingsRepository.SetSalesPipelineRoles(request);
				response.ResponseStatus.Status = "Success";
				response.ResponseStatus.ErrorMessage = response != null ? "" : "No Records Found.";
			}
			catch (Exception ex)
			{
				response.ResponseStatus.Status = "Failure";
				response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
			}

			return response;
		}

		[Authorize]
		[HttpPost]
		[Route("DeleteSalesPipelineRoles")]
		public async Task<SettingsSetRes> DeleteSalesPipelineRoles([FromBody] SettingsSetReq request)
		{
			var response = new SettingsSetRes();
			try
			{
				response = await _settingsRepository.DeleteSalesPipelineRoles(request);
				response.ResponseStatus.Status = "Success";
				response.ResponseStatus.ErrorMessage = response != null ? "" : "No Records Found.";
			}
			catch (Exception ex)
			{
				response.ResponseStatus.Status = "Failure";
				response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
			}

			return response;
		}

        /// <summary>
        /// GetAutomatedSalesPipelineRoles used for getting UserId and User Name by QRFID, RoleName 
        /// If Quote has destination then fetch the user details by Destination else by Sales office in mSalesPipelinesRoles collection
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetAutomatedSalesPipelineRoles")]
        public async Task<SettingsAutomatedGetRes> GetAutomatedSalesPipelineRoles([FromBody] SettingsAutomatedGetReq request)
        {
            var response = new SettingsAutomatedGetRes();
            try
            {
                response = await _settingsRepository.GetAutomatedSalesPipelineRoles(request);
                response.ResponseStatus.Status = "Success";
                response.ResponseStatus.ErrorMessage = response != null ? "" : "No Records Found.";
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }

        #region Integration Credentials

        [Authorize]
        [HttpPost]
        [Route("GetIntegrationCredentials")]
        public async Task<Integration_Search_Response> GetIntegrationCredentials([FromBody] Integration_Search_Request request)
        {
            var response = new Integration_Search_Response();

            response = await _settingsRepository.GetIntegrationCredentials(request);
            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("GetApplicationAttributes")]
        public async Task<Integration_Search_Response> GetApplicationAttributes([FromBody] Integration_Search_Request request)
        {
            var response = new Integration_Search_Response();

            response = await _settingsRepository.GetApplicationAttributes(request);
            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("CheckCredentialsExit")]
        public async Task<bool> CheckCredentialsExit([FromBody] Integration_Search_Request request)
        {
            var response = new Integration_Search_Response();

            bool data = await _settingsRepository.CheckCredentialsExit(request);

            return data;
        }

        [Authorize]
        [HttpPost]
        [Route("SaveIntegrationCredentials")]
        public async Task<Integration_Search_Response> SaveIntegrationCredentials([FromBody] Integration_Search_Request request)
        {
            var response = new Integration_Search_Response();

            response = await _settingsRepository.SaveIntegrationCredentials(request);
            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("DeleteIntegrationCredentials")]
        public async Task<Integration_Search_Response> DeleteIntegrationCredentials([FromBody] Integration_Search_Request request)
        {
            var response = new Integration_Search_Response();

            response = await _settingsRepository.DeleteIntegrationCredentials(request);
            return response;
        }

        #endregion

        #region Out Bound Integration Credentials

        [Authorize]
        [HttpPost]
        [Route("CheckOutBoundConfigExit")]
        public async Task<bool> CheckOutBoundConfigExit([FromBody] OutBoundIntegrationCredentialsReq request)
        {
            var response = new OutBoundIntegrationCredentialsRes();

            bool data = await _settingsRepository.CheckOutBoundConfigExit(request);

            return data;
        }

        [Authorize]
        [HttpPost]
        [Route("SaveOutBoundIntegrationCredentials")]
        public async Task<OutBoundIntegrationCredentialsRes> SaveOutBoundIntegrationCredentials([FromBody] OutBoundIntegrationCredentialsReq request)
        {
            var response = new OutBoundIntegrationCredentialsRes();

            response = await _settingsRepository.SaveOutBoundIntegrationCredentials(request);

            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("GetOutBoundIntegrationCredentialsList")]
        public async Task<OutBoundIntegrationCredentialsRes> GetOutBoundIntegrationCredentialsList([FromBody] OutBoundIntegrationCredentialsReq request)
        {
            var response = new OutBoundIntegrationCredentialsRes();

            response = await _settingsRepository.GetOutBoundIntegrationCredentialsList(request);
            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("DeleteOutBoundIntegrationCredentials")]
        public async Task<OutBoundIntegrationCredentialsRes> DeleteOutBoundIntegrationCredentials([FromBody] OutBoundIntegrationCredentialsReq request)
        {
            var response = new OutBoundIntegrationCredentialsRes();

            response = await _settingsRepository.DeleteOutBoundIntegrationCredentials(request);
            return response;
        }

        #endregion

        #region Integration Application Mapping Data

        [Authorize]
        [HttpPost]
        [Route("CheckApplicationMappingExists")]
        public async Task<bool> CheckApplicationMappingExists([FromBody] IntegrationMappingDataReq request)
        {
            bool data = await _settingsRepository.CheckApplicationMappingExists(request);
            return data;
        }

        [Authorize]
        [HttpPost]
        [Route("SaveIntegrationApplicationMappingInfo")]
        public async Task<IntegrationMappingDataRes> SaveIntegrationApplicationMappingInfo([FromBody] IntegrationMappingDataReq request)
        {
            var response = await _settingsRepository.SaveIntegrationApplicationMappingInfo(request);
            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("GetApplicationMappingList")]
        public async Task<IntegrationMappingDataRes> GetApplicationMappingList([FromBody] IntegrationMappingDataReq request)
        {
            var response = new IntegrationMappingDataRes();

            response = await _settingsRepository.GetApplicationMappingList(request);
            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("GetApplicationMappingDataList")]
        public async Task<IntegrationMappingDataRes> GetApplicationMappingDataList([FromBody] IntegrationMappingDataReq request)
        {
            var response = new IntegrationMappingDataRes();

            response = await _settingsRepository.GetApplicationMappingDataList(request);
            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("CheckApplicationMappingDataExists")]
        public async Task<bool> CheckApplicationMappingDataExists([FromBody] IntegrationMappingDataReq request)
        {
            bool data = await _settingsRepository.CheckApplicationMappingDataExists(request);
            return data;
        }

        [Authorize]
        [HttpPost]
        [Route("SaveIntegrationApplicationMappingDataInfo")]
        public async Task<IntegrationMappingDataRes> SaveIntegrationApplicationMappingDataInfo([FromBody] IntegrationMappingDataReq request)
        {
            var response = await _settingsRepository.SaveIntegrationApplicationMappingDataInfo(request);
            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("DeleteIntegrationApplicationMappingInfo")]
        public async Task<IntegrationMappingDataRes> DeleteIntegrationApplicationMappingInfo([FromBody] IntegrationMappingDataReq request)
        {
            var response = new IntegrationMappingDataRes();

            response = await _settingsRepository.DeleteIntegrationApplicationMappingInfo(request);
            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("DeleteIntegrationApplicationMappingItemsInfo")]
        public async Task<IntegrationMappingDataRes> DeleteIntegrationApplicationMappingItemsInfo([FromBody] IntegrationMappingDataReq request)
        {
            var response = new IntegrationMappingDataRes();

            response = await _settingsRepository.DeleteIntegrationApplicationMappingItemsInfo(request);
            return response;
        }

        #endregion

        #region Integration Platform

        [Authorize]
        [HttpPost]
        [Route("GetIntegrationPlatform")]
        public async Task<Integration_Search_Response> GetIntegrationPlatform([FromBody] Integration_Search_Request request)
        {
            var response = new Integration_Search_Response();

            //response = await _settingsRepository.GetIntegrationCredentials(request);
            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("CheckPlatformExit")]
        public async Task<bool> CheckPlatformExit([FromBody] IntegartionPlatform_Req request)
        {
            var response = new IntegartionPlatform_Res();

            bool data = await _settingsRepository.CheckPlatformExit(request);
            return data;
        }

        [Authorize]
        [HttpPost]
        [Route("SaveIntegrationPlatform")]
        public async Task<IntegartionPlatform_Res> SaveIntegrationPlatform([FromBody] IntegartionPlatform_Req request)
        {
            var response = new IntegartionPlatform_Res();

            response = await _settingsRepository.SaveIntegrationPlatform(request);
            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("GetIntegrationPlatformList")]
        public async Task<IntegartionPlatform_Res> GetIntegrationPlatformList([FromBody] IntegartionPlatform_Req request)
        {
            var response = new IntegartionPlatform_Res();

            response = await _settingsRepository.GetIntegrationPlatformList(request);
            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("GetIntegrationPlatformConfigInfo")]
        public async Task<IntegartionPlatform_Req> GetIntegrationPlatformConfigInfo([FromBody] IntegartionPlatform_Req request)
        {
            request = await _settingsRepository.GetIntegrationPlatformConfigInfo(request);
            return request;
        }

        [Authorize]
        [HttpPost]
        [Route("GetIntegrationConfigInfo")]
        public async Task<IntegartionPlatform_Req> GetIntegrationConfigInfo([FromBody] IntegartionPlatform_Req request)
        {
            request = await _mSDynamicsRepository.GetIntegrationConfigInfo(request);
            return request;
        }

        [Authorize]
        [HttpPost]
        [Route("GetAllApplicationMappingDataList")]
        public async Task<IntegrationMappingDataRes> GetAllApplicationMappingDataList([FromBody] IntegrationMappingDataReq request)
        {
            var response = await _mSDynamicsRepository.GetAllApplicationMappingDataList(request);
            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("UpdatemQuoteNmQRFPriceMapping")]
        public async Task<bool> UpdatemQuoteNmQRFPriceMapping([FromBody] QuoteMappingReq request)
        {
            var response = await _mSDynamicsRepository.UpdatemQuoteNmQRFPriceMapping(request);
            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("AddOpportunityMapping")]
        public async Task<bool> AddOpportunityMapping([FromBody] QuoteMappingReq request)
        {
            var response = await _mSDynamicsRepository.AddOpportunityMapping(request);
            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("UpdatemBookingMapping")]
        public async Task<bool> UpdatemBookingMapping([FromBody] BookingMappingReq request)
        {
            var response = await _mSDynamicsRepository.UpdatemBookingMapping(request);
            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("DeleteIntegrationPlatform")]
        public async Task<IntegartionPlatform_Res> DeleteIntegrationPlatform([FromBody] IntegartionPlatform_Req request)
        {
            var response = new IntegartionPlatform_Res();

            response = await _settingsRepository.DeleteIntegrationPlatform(request);
            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("GetIntegrationRedirection")]
        public async Task<List<IntegrationConfigurationInfo>> GetIntegrationRedirection([FromBody] IntegrationLoginRequest request)
        {
            var response = new List<IntegrationConfigurationInfo>();

            response = await _settingsRepository.GetIntegrationRedirection(request);
            return response;
        }

        #endregion
    }
}