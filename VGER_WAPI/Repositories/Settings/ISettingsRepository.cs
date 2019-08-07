using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Repositories
{
    public interface ISettingsRepository
    {
		Task<SettingsGetRes> GetSalesPipelineRoles(SettingsGetReq request);

		Task<SettingsSetRes> SetSalesPipelineRoles(SettingsSetReq request);

		Task<SettingsSetRes> DeleteSalesPipelineRoles(SettingsSetReq request);

        Task<SettingsAutomatedGetRes> GetAutomatedSalesPipelineRoles(SettingsAutomatedGetReq request);

        Task<Integration_Search_Response> GetIntegrationCredentials(Integration_Search_Request request);

        string GenerateCustomIntegrationKeyForSource(string Key, string UserEmail);

        Task<Integration_Search_Response> GetApplicationAttributes(Integration_Search_Request request);

        Task<bool> CheckCredentialsExit(Integration_Search_Request request);

        Task<Integration_Search_Response> SaveIntegrationCredentials(Integration_Search_Request request);

        Task<Integration_Search_Response> DeleteIntegrationCredentials(Integration_Search_Request request);

        Task<IntegartionPlatform_Res> SaveIntegrationPlatform(IntegartionPlatform_Req request);

        Task<IntegartionPlatform_Res> SaveIntegrationPlatformConfig(IntegartionPlatform_Req request);

        Task<bool> CheckPlatformExit(IntegartionPlatform_Req request);

        Task<IntegartionPlatform_Res> GetIntegrationPlatformList(IntegartionPlatform_Req request);

        Task<IntegartionPlatform_Req> GetIntegrationPlatformConfigInfo(IntegartionPlatform_Req request);

        Task<IntegartionPlatform_Res> DeleteIntegrationPlatform(IntegartionPlatform_Req request);

        Task<List<IntegrationConfigurationInfo>> GetIntegrationRedirection(IntegrationLoginRequest request);

        #region Out Bound Integration Credentials

        Task<bool> CheckOutBoundConfigExit(OutBoundIntegrationCredentialsReq request);

        Task<OutBoundIntegrationCredentialsRes> SaveOutBoundIntegrationCredentials(OutBoundIntegrationCredentialsReq request);

        Task<OutBoundIntegrationCredentialsRes> GetOutBoundIntegrationCredentialsList(OutBoundIntegrationCredentialsReq request);

        Task<OutBoundIntegrationCredentialsRes> DeleteOutBoundIntegrationCredentials(OutBoundIntegrationCredentialsReq request);

        #endregion

        #region Integration Application Mapping Data

        Task<bool> CheckApplicationMappingExists(IntegrationMappingDataReq request);

        Task<IntegrationMappingDataRes> SaveIntegrationApplicationMappingInfo(IntegrationMappingDataReq request);

        Task<IntegrationMappingDataRes> GetApplicationMappingList(IntegrationMappingDataReq request);

        Task<IntegrationMappingDataRes> GetApplicationMappingDataList(IntegrationMappingDataReq request);

        Task<bool> CheckApplicationMappingDataExists(IntegrationMappingDataReq request);

        Task<IntegrationMappingDataRes> SaveIntegrationApplicationMappingDataInfo(IntegrationMappingDataReq request);

        Task<IntegrationMappingDataRes> DeleteIntegrationApplicationMappingInfo(IntegrationMappingDataReq request);

        Task<IntegrationMappingDataRes> DeleteIntegrationApplicationMappingItemsInfo(IntegrationMappingDataReq request);

        #endregion
    }
}
