using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI.Helpers;
using VGER_WAPI.Models;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Repositories
{
    public class SettingsRepository : ISettingsRepository
    {
        #region Private Variable Declaration
        private readonly MongoContext _MongoContext = null;
        private readonly IAgentRepository _agentRepository;
        private readonly IQuoteRepository _quoteRepository;
        #endregion

        public SettingsRepository(IOptions<MongoSettings> settings, IAgentRepository agentRepository, IQuoteRepository quoteRepository)
        {
            _MongoContext = new MongoContext(settings);
            _agentRepository = agentRepository;
            _quoteRepository = quoteRepository;
        }

        public async Task<SettingsGetRes> GetSalesPipelineRoles(SettingsGetReq request)
        {
            string SystemCompanyId = string.Empty;
            SettingsGetRes response = new SettingsGetRes();
            try
            {
                if (!string.IsNullOrWhiteSpace(request.LoggedInUserContact_Id))
                {
                    _agentRepository.GetSystemCompany(request.LoggedInUserContact_Id, out SystemCompanyId);

                    List<Values> lstValues = new List<Values>();

                    if (!string.IsNullOrWhiteSpace(request.Type))
                    {
                        lstValues = _MongoContext.mSalesPipelineRoles.AsQueryable().Where(x => x.Type.ToLower() == request.Type.ToLower() && x.SystemCompany_Id == SystemCompanyId).FirstOrDefault()?.Values;

                        if (lstValues != null && lstValues.Count > 0)
                        {
                            if (!string.IsNullOrWhiteSpace(request.DestinationId))
                            {
                                lstValues = lstValues.Where(x => x.TypeId == request.DestinationId).ToList();
                            }
                            if (!string.IsNullOrWhiteSpace(request.RoleId))
                            {
                                lstValues = lstValues.Where(x => x.RoleId == request.RoleId).ToList();
                            }
                            if (!string.IsNullOrWhiteSpace(request.UserId))
                            {
                                lstValues = lstValues.Where(x => x.UserId == request.UserId).ToList();
                            }

                            response.Values = lstValues.Where(x => string.IsNullOrWhiteSpace(x.Status)).ToList();
                            response.ResponseStatus.Status = "Success";
                        }
                        else
                        {
                            response.ResponseStatus.Status = "Failure";
                            response.ResponseStatus.ErrorMessage = "Values not found";
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "Type not found";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Loggedin user contact id not found";
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }
            return response;
        }

        public async Task<SettingsSetRes> SetSalesPipelineRoles(SettingsSetReq request)
        {
            SettingsSetRes response = new SettingsSetRes();
            try
            {
                if (!string.IsNullOrWhiteSpace(request.LoggedInUserContact_Id))
                {
                    string SystemCompanyId = string.Empty;
                    _agentRepository.GetSystemCompany(request.LoggedInUserContact_Id, out SystemCompanyId);

                    mTypeMaster result = (from u in _MongoContext.mTypeMaster.AsQueryable() where u.PropertyType.PropertyName == "QRF Masters" select u).FirstOrDefault();

                    if (!string.IsNullOrWhiteSpace(request.Type))
                    {
                        var role = await _MongoContext.mSalesPipelineRoles.FindAsync(x => x.Type.ToLower() == request.Type.ToLower() && x.SystemCompany_Id == SystemCompanyId).Result.FirstOrDefaultAsync();

                        if (role != null)
                        {
                            request.Values.RemoveAll(x => string.IsNullOrWhiteSpace(x.TypeId) && string.IsNullOrWhiteSpace(x.RoleId) && string.IsNullOrWhiteSpace(x.UserId));

                            foreach (var val in request.Values)
                            {
                                var eval = role.Values.Where(x => x.TypeId == val.TypeId && x.RoleId == val.RoleId && x.UserId == val.UserId).FirstOrDefault();
                                if (eval != null)
                                {
                                    eval.Status = val.Status;
                                    eval.EditUser = val.EditUser;
                                    eval.EditDate = val.EditDate;
                                }
                                else
                                {
                                    var name = "";
                                    if (request.Type.ToLower() == "destination")
                                        name = _quoteRepository.GetValueofAttributeFromMaster(result, "QRF Destination", val.TypeId);
                                    else
                                        name = val.TypeName;
                                    var username = _MongoContext.mUsers.AsQueryable().Where(x => x.VoyagerUser_Id == val.UserId).Select(x => x.UserName).FirstOrDefault();
                                    Values obj = new Values()
                                    {
                                        TypeId = val.TypeId,
                                        TypeName = name,
                                        RoleId = val.RoleId,
                                        RoleName = val.RoleName,
                                        UserId = val.UserId,
                                        UserName = (username ?? val.UserName),
                                        Status = val.Status,
                                        CreateDate = val.CreateDate,
                                        CreateUser = val.CreateUser,
                                        EditUser = val.EditUser,
                                        EditDate = val.EditDate
                                    };
                                    role.Values.Add(obj);
                                }
                            }

                            await _MongoContext.mSalesPipelineRoles.UpdateOneAsync(Builders<mSalesPipelineRoles>.Filter.Eq("Type", request.Type),
                            Builders<mSalesPipelineRoles>.Update.Set("Values", role.Values).Set("SystemCompany_Id", SystemCompanyId));

                            response.ResponseStatus.Status = "Success";
                            response.ResponseStatus.StatusMessage = "Record Saved Successfully.";
                        }
                        else
                        {
                            //Create new document
                            mSalesPipelineRoles newobj = new mSalesPipelineRoles();
                            newobj.Type = request.Type;
                            newobj.SystemCompany_Id = SystemCompanyId;

                            List<Values> lstValues = new List<Values>();
                            foreach (var val in request.Values)
                            {
                                var username = _MongoContext.mUsers.AsQueryable().Where(x => x.VoyagerUser_Id == val.UserId).Select(x => x.UserName).FirstOrDefault();
                                Values obj = new Values()
                                {
                                    TypeId = val.TypeId,
                                    TypeName = val.TypeName,
                                    RoleId = val.RoleId,
                                    RoleName = val.RoleName,
                                    UserId = val.UserId,
                                    UserName = (username ?? val.UserName),
                                    Status = string.Empty,
                                    CreateDate = DateTime.Now,
                                    CreateUser = val.CreateUser
                                };
                                lstValues.Add(obj);
                            }
                            newobj.Values = lstValues;

                            await _MongoContext.mSalesPipelineRoles.InsertOneAsync(newobj);
                            response.ResponseStatus.Status = "Success";
                            response.ResponseStatus.StatusMessage = "Record Saved Successfully.";
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "Type not found";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Loggedin user contact id not found";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }
            return response;
        }

        public async Task<SettingsSetRes> DeleteSalesPipelineRoles(SettingsSetReq request)
        {
            SettingsSetRes response = new SettingsSetRes();
            try
            {
                //if (!string.IsNullOrWhiteSpace(request.Type))
                //{
                //	var role = _MongoContext.mSalesPipelineRoles.AsQueryable().Where(x => x.Type.ToLower() == request.Type.ToLower()).FirstOrDefault();
                //	var val = role.Values.Where(x => x.TypeId == request.Values.TypeId && x.RoleId == request.Values.RoleId && x.UserId == request.Values.UserId).FirstOrDefault();

                //	if (val != null)
                //	{
                //		val.Status = "X";
                //		val.EditDate = DateTime.Now;
                //		val.EditUser = request.Values.EditUser;

                //		await _MongoContext.mSalesPipelineRoles.UpdateOneAsync(Builders<mSalesPipelineRoles>.Filter.Eq("Type", request.Type),
                //		Builders<mSalesPipelineRoles>.Update.Set("Values", role.Values));

                //		response.ResponseStatus.Status = "Success";
                //	}
                //	else
                //	{
                //		response.ResponseStatus.Status = "Failure";
                //		response.ResponseStatus.ErrorMessage = "Data not found";
                //	}
                //}
                //else
                //{
                //	response.ResponseStatus.Status = "Failure";
                //	response.ResponseStatus.ErrorMessage = "Type not found";
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }
            return response;
        }

        public async Task<SettingsAutomatedGetRes> GetAutomatedSalesPipelineRoles(SettingsAutomatedGetReq request)
        {
            SettingsAutomatedGetRes response = new SettingsAutomatedGetRes() { ResponseStatus = new ResponseStatus() };
            try
            {
                if (!string.IsNullOrEmpty(request.QRFId))
                {
                    string defUserEmailId = "";
                    string defUserId = "";

                    var objQuote = await _MongoContext.mQuote.FindAsync(a => a.QRFID == request.QRFId).Result.FirstOrDefaultAsync();
                    if (objQuote != null)
                    {
                        var mQuoteDestId = objQuote.AgentProductInfo?.DestinationID;
                        bool flag = false;

                        if (!string.IsNullOrEmpty(mQuoteDestId))
                        {
                            var objDestination = _MongoContext.mSalesPipelineRoles.AsQueryable().Where(a => a.Type.ToLower() == "destination").FirstOrDefault()?.
                                                Values.Where(a => a.RoleName.ToLower() == request.UserRole.ToLower() && a.TypeId == mQuoteDestId && string.IsNullOrEmpty(a.Status)).FirstOrDefault();

                            if (objDestination == null || string.IsNullOrEmpty(objDestination?.UserId))
                            {
                                flag = true;
                            }
                            else
                            {
                                defUserEmailId = objDestination.UserName;
                                defUserId = objDestination.UserId;
                                response.ResponseStatus.Status = "Success";
                            }
                        }

                        if (flag)
                        {
                            var syscompany_Id = _MongoContext.mCompanies.AsQueryable().Where(a => a.Company_Id == request.CompanyId).FirstOrDefault()?.SystemCompany_Id;
                            if (!string.IsNullOrEmpty(syscompany_Id))
                            {
                                var objSalesOffice = _MongoContext.mSalesPipelineRoles.AsQueryable().Where(a => a.Type.ToLower() == "salesoffice" && a.SystemCompany_Id == syscompany_Id).FirstOrDefault()?.
                                                  Values.Where(a => a.RoleName.ToLower() == request.UserRole.ToLower() && a.TypeId == request.CompanyId && string.IsNullOrEmpty(a.Status)).FirstOrDefault();

                                if (objSalesOffice != null && !string.IsNullOrEmpty(objSalesOffice.UserId))
                                {
                                    defUserEmailId = objSalesOffice.UserName;
                                    defUserId = objSalesOffice.UserId;
                                    response.ResponseStatus.Status = "Success";
                                }
                                else
                                {
                                    response.ResponseStatus.ErrorMessage = "SalesOffice users not found.";
                                    response.ResponseStatus.Status = "Error";
                                }
                            }
                            else
                            {
                                response.ResponseStatus.ErrorMessage = "SystemCompany_Id not found for CompanyId: " + request.CompanyId + " in mCompanies collection.";
                                response.ResponseStatus.Status = "Error";
                            }
                        }
                        response.UserId = defUserId;
                        response.UserEmailId = defUserEmailId;
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "QRFID can not exists in mQuote collection.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "QRFID can not be blank/null.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// GetIntegrationCredentials used for getting Integration Credentials based on Company Id and logIn user
        /// </summary>
        /// <param name="request"></param>
        /// <returns>
        /// Fetched all the Integration Credentials list for From mUser App_Keys
        /// </returns>
        public async Task<Integration_Search_Response> GetIntegrationCredentials(Integration_Search_Request request)
        {
            Integration_Search_Response response = new Integration_Search_Response();

            try
            {
                var App_Keys_List = _MongoContext.mUsers.AsQueryable().Where(x => x.App_Keys != null && x.App_Keys.Any()).ToList();
                //App_Keys_List.RemoveAll(x => x.App_Keys == null || !x.App_Keys.Any());
                if (!string.IsNullOrEmpty(request.Application_Id))
                {
                    App_Keys_List = App_Keys_List.Where(a => a.App_Keys.Select(b => b.Application_Id).ToArray().Contains(request.Application_Id)).ToList();
                }
                if (!string.IsNullOrEmpty(request.UserId))
                {
                    var UserBasedOnSysId = GetSelectedIntegrationCredentials(request);
                    App_Keys_List = App_Keys_List.Where(a => a.VoyagerUser_Id == UserBasedOnSysId.VoyagerUser_Id).ToList();
                }

                if (App_Keys_List != null && App_Keys_List.Any())
                {
                    foreach (var item in App_Keys_List.ToList())
                    {
                        response.IntegrationSearchDataList.AddRange(item.App_Keys.Where(a => a.Status == null || a.Status == "").Select(b => new Integration_Search_Data
                        {
                            Application_Id = b.Application_Id,
                            Application_Name = b.Application_Name,
                            Keys = request.IsExport ? GenerateCustomIntegrationKeyForSource(b.Application_Id, item.Email) : b.Key,
                            UserKey = request.IsExport ? GenerateCustomIntegrationKeyForSource(b.Key, item.Email): b.Key,
                            Status = b.Status,
                            UserId = item.VoyagerUser_Id,
                            UserName = item.UserName
                        }));

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// GetApplicationAttributes used for binding the DropDown or to list
        /// Fetched all the Application from mApplication like CRM etc.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<Integration_Search_Response> GetApplicationAttributes(Integration_Search_Request request)
        {
            Integration_Search_Response response = new Integration_Search_Response();
            try
            {
                var platformList = _MongoContext.mApplications.AsQueryable().ToList();
                response.Application_DataList.AddRange(
                    platformList.Select(a =>
                    new Attributes
                    {
                        Attribute_Id = a.Application_Id,
                        AttributeName = a.Application_Name
                    })
                    );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Check Integration Credentials Exit for the particular Platform or user
        /// From mUser.App_Keys returns boolean data.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<bool> CheckCredentialsExit(Integration_Search_Request request)
        {
            var UserBasedOnSysId = GetSelectedIntegrationCredentials(request);

            if (UserBasedOnSysId != null && UserBasedOnSysId.App_Keys != null && UserBasedOnSysId.App_Keys.Any() && UserBasedOnSysId.App_Keys.Where(a => a.Status == "" && a.Application_Id == request.Application_Id).Any())
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get All the Users based on login ckUserCompanyId based on two condition 1st with systemuser_id if not fount than 2nd based on Email(Mail) and company_Id.
        /// Returns mUser.App_Keys with the key
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public mUsers GetSelectedIntegrationCredentials(Integration_Search_Request request)
        {
            var companyWithContacts = _MongoContext.mCompanies.AsQueryable().Where(x => x.ContactDetails.Any() && x.Company_Id == request.ckUserCompanyId).Select(a => a.ContactDetails).FirstOrDefault();
            var contactData = companyWithContacts.Where(a => a.Contact_Id == request.UserId).FirstOrDefault();

            //1st condition
            var UserBasedOnSysId = _MongoContext.mUsers.AsQueryable().Where(a => a.VoyagerUser_Id == contactData.Systemuser_id).FirstOrDefault();

            //2nd condition
            if (UserBasedOnSysId == null)
            {
                UserBasedOnSysId = _MongoContext.mUsers.AsQueryable().Where(a =>!string.IsNullOrEmpty(a.Email) && a.Email.ToLower() == contactData.MAIL.ToLower().Trim() && a.Company_Id == contactData.Company_Id).FirstOrDefault();
            }

            return UserBasedOnSysId;
        }

        /// <summary>
        /// Save Integration Credentials for the selected Platform with user
        /// Save to mUser.App_Keys with the key for user.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<Integration_Search_Response> SaveIntegrationCredentials(Integration_Search_Request request)
        {
            Integration_Search_Response response = new Integration_Search_Response();

            try
            {
                var SelectedApplication = _MongoContext.mApplications.AsQueryable().Where(a => a.Application_Id == request.Application_Id).FirstOrDefault();
                string generatedKey = Encrypt.EncryptData(SelectedApplication.Key, GenarateIntegrationKey(request.ckUserCompanyId));
                
                var UserBasedOnSysId = GetSelectedIntegrationCredentials(request);

                if (UserBasedOnSysId.App_Keys == null)
                {
                    Integration_App_Keys obj = new Integration_App_Keys();
                    obj.Application_Id = request.Application_Id;
                    obj.Application_Name = _MongoContext.mApplications.AsQueryable().Where(a => a.Application_Id == request.Application_Id).Select(x => x.Application_Name).FirstOrDefault();
                    obj.Key = generatedKey;
                    obj.Status = "";
                    obj.CreateUser = request.CreatedUser;
                    obj.CreateDate = DateTime.Now;

                    UserBasedOnSysId.App_Keys = new List<Integration_App_Keys>();
                    UserBasedOnSysId.App_Keys.Add(obj);
                }
                else
                {
                    Integration_App_Keys updateApp = UserBasedOnSysId.App_Keys.Where(a => a.Application_Id == request.Application_Id && a.Status == "X").FirstOrDefault();
                    updateApp.Key = generatedKey;
                    updateApp.Status = "";
                    updateApp.EditDate = DateTime.Now;
                    updateApp.EditUser = request.CreatedUser;
                }

                if (UserBasedOnSysId != null)
                {
                    var resultUSer = _MongoContext.mUsers.FindOneAndUpdate(a => a.VoyagerUser_Id == UserBasedOnSysId.VoyagerUser_Id,
                        Builders<mUsers>.Update.Set(a => a.App_Keys, UserBasedOnSysId.App_Keys));
                    response.ResponseStatus.Status = "Success";
                    response.ResponseStatus.ErrorMessage = "Integration Credentials created successfully.";
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Delete Integration Credentials for the selected row
        /// Delete means it's a soft delete where status is set to 'X' to mUser.App_Keys with the key for user.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<Integration_Search_Response> DeleteIntegrationCredentials(Integration_Search_Request request)
        {
            Integration_Search_Response response = new Integration_Search_Response();

            var selectedUser = _MongoContext.mUsers.AsQueryable().Where(a => a.VoyagerUser_Id == request.UserId && a.App_Keys.AsQueryable().Where(x => x.Application_Id == request.Application_Id && x.Status == "").Any());

            var appKeyList = selectedUser.Select(a => a.App_Keys).FirstOrDefault();

            foreach (var item in appKeyList)
            {
                if (item.Application_Id == request.Application_Id && item.Status == "")
                {
                    item.Status = "X";
                    item.EditDate = DateTime.Now;
                    item.EditUser = request.CreatedUser;
                }
            }

            var resultUSer = _MongoContext.mUsers.FindOneAndUpdate(a => a.VoyagerUser_Id == request.UserId,
                        Builders<mUsers>.Update.Set(a => a.App_Keys, appKeyList));

            return response;
        }

        /// <summary>
        /// Genarate Integration Key with the combination of ckUserCompanyId
        /// Return the random key combination of ckUserCompanyId
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        public string GenarateIntegrationKey(string str)
        {
            Random random = new Random();
            int length = 32;
            return new string(Enumerable.Repeat(str, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Genarate Custom Integration Key based on Source
        /// Return the custom key combination of selected User Email where it's combination of two string
        /// 1st string(str1) will be encrypted with the  Application Key from mApplication collection for the source with Email.
        /// 2nd string(str2) will be encrypted with the Generated App_key against the user from mUser collection with Email.
        /// this is used for export to excel.
        /// </summary>
        /// <param></param>
        /// <returns>returns the 1st string and 2nd string</returns>
        public string GenerateCustomIntegrationKeyForSource(string Key, string UserEmail)
        {
            string str = Encrypt.EncryptData(Key, UserEmail);
            return str = str.Replace("/", "|");
            //return str;
        }

        /// <summary>
        /// Save Integration Platform for the selected Platform with Module and Action
        /// Save to mIntegrationPlatform with respective info.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IntegartionPlatform_Res> SaveIntegrationPlatform(IntegartionPlatform_Req request)
        {
            IntegartionPlatform_Res response = new IntegartionPlatform_Res();

            try
            {
                var getIntegartionInfo = GetSelectedIntegrationPlatform(request);//_MongoContext.mIntegrationPlatform.AsQueryable().Where(a => a.Application == request.Application).FirstOrDefault();

                if (getIntegartionInfo == null)
                {
                    mIntegrationPlatform obj = new mIntegrationPlatform();
                    obj.IntegrationPlatform_Id = Guid.NewGuid().ToString();
                    obj.Application = request.Application;
                    obj.ApplicationName = request.ApplicationName;

                    obj.Modules = new List<ModuleActionInfo>();

                    ModuleActionInfo moduleInfo = new ModuleActionInfo();
                    moduleInfo.Module = request.Module;
                    moduleInfo.ModuleName = request.ModuleName;

                    moduleInfo.Actions = new List<ActionInfo>();

                    ActionInfo actionInfo = new ActionInfo();
                    actionInfo.Action = request.Action;
                    actionInfo.ActionName = request.ActionName;
                    moduleInfo.Status = actionInfo.Status = "";
                    actionInfo.TypeName = request.TypeName;
                    moduleInfo.CreateUser = actionInfo.CreateUser = request.CreateUser;
                    moduleInfo.CreateDate = actionInfo.CreateDate = DateTime.Now;

                    if (request.Configurations != null && request.Configurations.Any())
                    {
                        actionInfo.Configurations = new List<IntegrationConfigurationInfo>();
                        foreach (var configItem in request.Configurations)
                        {
                            configItem.CreateDate = DateTime.Now;
                            configItem.CreateUser = request.CreateUser;
                            configItem.Status = "";
                            configItem.ConfigId = Guid.NewGuid().ToString();
                            actionInfo.Configurations.Add(configItem);
                        }
                    }

                    moduleInfo.Actions.Add(actionInfo);
                    obj.Modules.Add(moduleInfo);

                    _MongoContext.mIntegrationPlatform.InsertOne(obj);

                    response.ResponseStatus.Status = "Success";
                    response.ResponseStatus.ErrorMessage = "Integration Platform created successfully.";

                }
                else
                {
                    var Modules = getIntegartionInfo.Modules;//.Where(a => a.Module == request.Module /*&& a.Status == ""*/);

                    if (Modules != null && Modules.Any() && Modules.Any(a => a.Module == request.Module))
                    {
                        var ModuleAction = Modules.AsQueryable().Where(a => a.Module == request.Module).FirstOrDefault();

                        if (ModuleAction != null && ModuleAction.Actions != null && ModuleAction.Status == "" && ModuleAction.Actions.Any(a=>a.Action == request.Action && a.Status == "" && a.TypeName == request.TypeName))
                        {
                            var Action = ModuleAction.Actions.Where(a => a.Action == request.Action && a.Status == "" && a.TypeName == request.TypeName).FirstOrDefault();

                            foreach (var item in ModuleAction.Actions)
                            {
                                if (item.Action == request.Action && item.Status == "" && item.TypeName == request.TypeName)
                                {
                                    if (request.Configurations != null && request.Configurations.Any())
                                    {
                                        if (!CheckIfConfigurationExist(request))
                                        {
                                            var ExistingConfigs = item?.Configurations?.Where(a => a.ConfigId != null);
                                            item.Configurations = new List<IntegrationConfigurationInfo>();
                                            foreach (var configItem in request.Configurations)
                                            {
                                                var existingconfig = configItem.ConfigId != null ? ExistingConfigs.Where(x => x.ConfigId == configItem?.ConfigId).FirstOrDefault(): new IntegrationConfigurationInfo();
                                                if (!String.IsNullOrEmpty(configItem.ConfigId))
                                                {
                                                    existingconfig.ApplicationFieldName = configItem.ApplicationFieldName;
                                                    existingconfig.PlatformTypeName = configItem.PlatformTypeName;
                                                    existingconfig.BoundType = configItem.BoundType;
                                                    existingconfig.EntityName = configItem.EntityName;
                                                    existingconfig.SystemFieldName = configItem.SystemFieldName;
                                                    existingconfig.URL = configItem.URL;
                                                    existingconfig.EditUser = request.EditUser;
                                                    existingconfig.EditDate = DateTime.Now;
                                                    item.Configurations.Add(existingconfig);
                                                }
                                                else
                                                {
                                                    configItem.CreateDate = DateTime.Now;
                                                    configItem.CreateUser = request.CreateUser;
                                                    configItem.Status = "";
                                                    configItem.ConfigId = Guid.NewGuid().ToString();
                                                    item.Configurations.Add(configItem);
                                                }
                                            }
                                            response.ResponseStatus.Status = "Success";
                                            response.ResponseStatus.StatusMessage = "Configuration saved Successfully";
                                        }
                                        else {
                                            response.ResponseStatus.Status = "Failure";
                                            response.ResponseStatus.StatusMessage = "Configurations Already exist in database";
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (ModuleAction.Actions == null)
                            {
                                ModuleAction.Actions = new List<ActionInfo>();
                            }
                            else
                            {
                                var existingAction = ModuleAction.Actions.Where(a => a.Action == request.Action && a.Status == "X" && a.TypeName == request.TypeName).FirstOrDefault();
                                if (existingAction != null)
                                {
                                    existingAction.Action = request.Action;
                                    existingAction.ActionName = request.ActionName;
                                    existingAction.Status = "";
                                    existingAction.EditDate = DateTime.Now;
                                    existingAction.TypeName = request.TypeName;
                                    existingAction.EditUser = request.CreateUser;

                                    if (request.Configurations != null && request.Configurations.Any())
                                    {
                                        existingAction.Configurations = new List<IntegrationConfigurationInfo>();
                                        foreach (var configItem in request.Configurations)
                                        {
                                            configItem.CreateDate = DateTime.Now;
                                            configItem.CreateUser = request.CreateUser;
                                            configItem.Status = "";
                                            existingAction.Configurations.Add(configItem);
                                        }
                                    }
                                }
                                else
                                {
                                    var action = new ActionInfo();
                                    action.Action = request.Action;
                                    action.ActionName = request.ActionName;
                                    action.CreateUser = request.CreateUser;
                                    action.CreateDate = DateTime.Now;
                                    action.TypeName = request.TypeName;
                                    action.Status = "";

                                    if (request.Configurations != null && request.Configurations.Any())
                                    {
                                        action.Configurations = new List<IntegrationConfigurationInfo>();
                                        foreach (var configItem in request.Configurations)
                                        {
                                            configItem.CreateDate = DateTime.Now;
                                            configItem.CreateUser = request.CreateUser;
                                            configItem.Status = "";
                                            action.Configurations.Add(configItem);
                                        }
                                    }

                                    ModuleAction.Actions.Add(action);
                                }
                            }

                            ModuleAction.Module = request.Module;
                            ModuleAction.ModuleName = request.ModuleName;
                            ModuleAction.Status = "";
                            ModuleAction.EditUser = request.CreateUser;
                            ModuleAction.EditDate = DateTime.Now;
                        }
                    }
                    else
                    {
                        var ModuleAction = new ModuleActionInfo();
                        ModuleAction.Module = request.Module;
                        ModuleAction.ModuleName = request.ModuleName;
                        ModuleAction.Status = "";
                        ModuleAction.Actions = new List<ActionInfo>();
                        ModuleAction.CreateUser = request.CreateUser;
                        ModuleAction.CreateDate = DateTime.Now;

                        var action = new ActionInfo();
                        action.Action = request.Action;
                        action.ActionName = request.ActionName;
                        action.Status = "";
                        action.TypeName = request.TypeName;
                        action.CreateUser = request.CreateUser;
                        action.CreateDate = DateTime.Now;

                        if (request.Configurations != null && request.Configurations.Any())
                        {
                            action.Configurations = new List<IntegrationConfigurationInfo>();
                            foreach (var configItem in request.Configurations)
                            {
                                configItem.CreateDate = DateTime.Now;
                                configItem.CreateUser = request.CreateUser;
                                configItem.Status = "";
                                action.Configurations.Add(configItem);
                            }
                        }

                        ModuleAction.Actions.Add(action);

                        Modules.Add(ModuleAction);
                    }

                    var updated = await _MongoContext.mIntegrationPlatform.FindOneAndUpdateAsync(m => m.Application == request.Application,
                                            Builders<mIntegrationPlatform>.Update.Set(m => m.Modules, Modules));
                    if (!request.Configurations.Any())
                    {
                        response.ResponseStatus.Status = "Success";
                        response.ResponseStatus.ErrorMessage = "Integration Platform created successfully.";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }

            return response;
        }
        /// <summary>
        /// to check if Configuration exists
        /// </summary>
        /// <param name="configurations"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private bool CheckIfConfigurationExist(IntegartionPlatform_Req request)
        {
           
            var config = request.Configurations.ToArray();
            for (var i = 0; i < config.Length; i++)
            {
                for (var j = i + 1; j<= config.Length-1; j++)
                {
                    var firstobj = config[i];
                    if (firstobj.PlatformTypeName == config[j].PlatformTypeName && (firstobj.SystemFieldName ?? "").Trim() == (config[j].SystemFieldName ?? "").Trim() && (firstobj.URL ?? "").Trim() == (config[j].URL ?? "").Trim() 
                        && (firstobj.ApplicationFieldName ?? "").Trim() == (config[j].ApplicationFieldName ?? "").Trim() && (firstobj.BoundType ?? "").Trim() == (config[j].BoundType ?? "").Trim() 
                        && (firstobj.EntityName ?? "").Trim() == (config[j].EntityName ?? "").Trim())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Save Integration Platform for the selected Platform with Module and Action
        /// Save to mIntegrationPlatform with respective info for configuration.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IntegartionPlatform_Res> SaveIntegrationPlatformConfig(IntegartionPlatform_Req request)
        {
            IntegartionPlatform_Res response = new IntegartionPlatform_Res();

            try
            {

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Check Integration Platform Exit for the particular Platform, Module and Action
        /// From mIntegartionPlatform returns boolean data.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<bool> CheckPlatformExit(IntegartionPlatform_Req request)
        {
            var PlatformBasedOnModelAction = GetSelectedIntegrationPlatform(request);

            if (PlatformBasedOnModelAction != null && PlatformBasedOnModelAction.Modules != null && 
                PlatformBasedOnModelAction.Modules.Any(a=>a.Status == "" && a.Module == request.Module && a.Actions != null && a.Actions.Any(b=>b.Status == "" && b.Action == request.Action && b.TypeName == request.TypeName)))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get All the Integration Platform based on login ckUserCompanyId.
        /// Returns mIntegrationPlatform with the Application_Id and ApplicationName and Modules arrays.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public mIntegrationPlatform GetSelectedIntegrationPlatform(IntegartionPlatform_Req request)
        {
            var PlatformBasedOnModelAction = _MongoContext.mIntegrationPlatform.AsQueryable().Where(a => a.Application == request.Application).FirstOrDefault();
            if (PlatformBasedOnModelAction != null)
            {
                var SelectedModel = PlatformBasedOnModelAction.Modules.Where(a => a.Module == request.Module && a.Status == "");
                if (SelectedModel != null && SelectedModel.Any())
                {
                    var SelectedAction = SelectedModel.FirstOrDefault().Actions.Where(a => a.Action == request.Action && a.Status == "" && a.TypeName !=null && a.TypeName == request.TypeName);
                    if (SelectedAction != null && SelectedAction.Any())
                    {
                        return PlatformBasedOnModelAction;
                    }
                }
            }

            return PlatformBasedOnModelAction;
        }

        /// <summary>
        /// GetIntegrationPlatformList used for binding the grid or to list
        /// Fetched all the IntegrationPlatform from mIntegrationPlatform like CRM etc.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IntegartionPlatform_Res> GetIntegrationPlatformList(IntegartionPlatform_Req request)
        {
            IntegartionPlatform_Res response = new IntegartionPlatform_Res();

            try
            {
                var dataList = _MongoContext.mIntegrationPlatform.AsQueryable();
                response.AppModuleActionInfoList = new List<IntegartionPlatform_Req>();

                if (dataList != null && dataList.Any())
                {
                    foreach (var item in dataList)
                    {
                        if (item.Modules != null && item.Modules.Any())
                        {
                            foreach (var moduleItem in item.Modules)
                            {
                                if (moduleItem.Status == "" && moduleItem.Actions != null && moduleItem.Actions.Any())
                                {
                                    foreach (var actionItem in moduleItem.Actions)
                                    {
                                        if (actionItem.Status == "")
                                        {
                                            IntegartionPlatform_Req req = new IntegartionPlatform_Req();
                                            req.Application = item.Application;
                                            req.ApplicationName = item.ApplicationName;
                                            req.Module = moduleItem.Module;
                                            req.ModuleName = moduleItem.ModuleName;
                                            req.Action = actionItem.Action;
                                            req.ActionName = actionItem.ActionName;
                                            req.TypeName = actionItem.TypeName;

                                            response.AppModuleActionInfoList.Add(req);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (response.AppModuleActionInfoList != null && response.AppModuleActionInfoList.Any())
                {
                    if (!string.IsNullOrEmpty(request.Application))
                    {
                        response.AppModuleActionInfoList = response.AppModuleActionInfoList.Where(a => a.Application == request.Application).ToList();
                    }
                    if (!string.IsNullOrEmpty(request.Module))
                    {
                        response.AppModuleActionInfoList = response.AppModuleActionInfoList.Where(a => a.Module == request.Module).ToList();
                    }
                    if (!string.IsNullOrEmpty(request.Action))
                    {
                        response.AppModuleActionInfoList = response.AppModuleActionInfoList.Where(a => a.Action == request.Action).ToList();
                    }
                    if (!string.IsNullOrEmpty(request.TypeName))
                    {
                        response.AppModuleActionInfoList = response.AppModuleActionInfoList.Where(a => a.TypeName == request.TypeName).ToList();
                    }
                    response.PlatformTotalCount = response.AppModuleActionInfoList.Count;
                    response.AppModuleActionInfoList = response.AppModuleActionInfoList.OrderBy(x => x.ModuleName).ThenBy(x => x.ActionName).Skip(request.Start).Take(request.Length).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// GetIntegrationPlatformConfigInfo used for binding Configuration Info to text box
        /// Configuration URL, ApplicationFieldName, SystemFieldName from mIntegrationPlatform base on Application, Module and Action.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IntegartionPlatform_Req> GetIntegrationPlatformConfigInfo(IntegartionPlatform_Req request)
        {
            var configInfo = GetSelectedIntegrationPlatform(request);

            var moduleInfo = configInfo != null ? configInfo.Modules.Where(a => a.Module == request.Module && a.Status == "").FirstOrDefault() : null;
            var actionInfo = moduleInfo != null ? moduleInfo.Actions.Where(a => a.Action == request.Action && a.Status == "" && a.TypeName == request.TypeName).FirstOrDefault() : null;

            if (actionInfo != null && actionInfo.Configurations != null && actionInfo.Configurations.Any(a => a.Status == ""))
            {
                request.Configurations = actionInfo.Configurations;
            }
            else
            {
                request.Configurations = new List<IntegrationConfigurationInfo>();
                var defaultconfig = new IntegrationConfigurationInfo();
                defaultconfig.URL = string.Empty;
                defaultconfig.BoundType = string.Empty;
                defaultconfig.EntityName = string.Empty;
                defaultconfig.ApplicationFieldName = string.Empty;
                defaultconfig.SystemFieldName = string.Empty;
                request.Configurations.Add(defaultconfig);
            }

            return request;
        }

        /// <summary>
        /// Delete Integration Platform for the selected row
        /// Delete means it's a soft delete where status is set to 'X' to mIntegrationPlatform with the Module and Platform(action) and config for user.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IntegartionPlatform_Res> DeleteIntegrationPlatform(IntegartionPlatform_Req request)
        {
            IntegartionPlatform_Res response = new IntegartionPlatform_Res();

            var selectedApplication = _MongoContext.mIntegrationPlatform.AsQueryable().Where(a => a.Application == request.Application).FirstOrDefault();

            var selectedModule = selectedApplication.Modules.Where(a => a.Module == request.Module && a.Status == "").FirstOrDefault();

            if (selectedModule != null && !string.IsNullOrEmpty(request.Action))
            {
                var selectedAction = selectedModule.Actions.Where(a => a.Action == request.Action && a.Status == "").FirstOrDefault();
                if (selectedAction != null)
                {
                    selectedAction.Status = "X";
                    selectedAction.EditDate = DateTime.Now;
                    selectedAction.EditUser = request.CreateUser;
                    if (selectedAction.Configurations != null && selectedAction.Configurations.Any())
                    {
                        foreach (var selectedConfig in selectedAction.Configurations)
                        {
                            selectedConfig.Status = "X";
                            selectedConfig.EditDate = DateTime.Now;
                            selectedConfig.EditUser = request.CreateUser;
                        }
                    }
                    //if (selectedApplication.Modules.Where(a => a.Module == request.Module && a.Status == "").Select(x => x.Actions).Count() == 1)
                    //{
                    //    selectedModule.Status = "X";
                    //}
                    selectedModule.EditDate = DateTime.Now;
                    selectedModule.EditUser = request.CreateUser;
                }
            }

            var updated = await _MongoContext.mIntegrationPlatform.FindOneAndUpdateAsync(m => m.Application == request.Application,
                                            Builders<mIntegrationPlatform>.Update.Set(m => m.Modules, selectedApplication.Modules));

            return response;
        }

        /// <summary>
        /// Get Integration Platform Configartion for redirection
        /// get the IntegrationConfigurationInfo (i,e. Configuration Info) where it has to be rediected.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<List<IntegrationConfigurationInfo>> GetIntegrationRedirection(IntegrationLoginRequest request)
        {
            List<IntegrationConfigurationInfo> response = new List<IntegrationConfigurationInfo>();
            var selectedApplication = !string.IsNullOrEmpty(request.Module) ? _MongoContext.mIntegrationPlatform.AsQueryable().Where(a => a.ApplicationName.ToLower() == request.Source.ToLower()).FirstOrDefault() : new mIntegrationPlatform();
            var selectedModule = selectedApplication != null && selectedApplication.Modules != null && selectedApplication.Modules.Any() && !string.IsNullOrEmpty(request.Module) ? selectedApplication.Modules.Where(a => a.ModuleName.ToLower() == request.Module.ToLower() && a.Status == "").FirstOrDefault() : new ModuleActionInfo();
            
            if (selectedModule != null && !string.IsNullOrEmpty(request.Operation))
            {
                //Use only GET TypeName is needed for redirection.
                var selectedAction = selectedModule.Actions.Where(a => a.ActionName.ToLower() == request.Operation.ToLower() && a.Status == "" && (a.TypeName != null && a.TypeName != "") && a.TypeName.ToLower() == "get").FirstOrDefault();
                if (selectedAction != null && selectedAction.Configurations != null && selectedAction.Configurations.Any())
                {
                    //Use only ADDRESS PlatformTypeName is needed for redirection.
                    return response = selectedAction.Configurations.Where(a => a.PlatformTypeName != null && a.PlatformTypeName != "" && a.PlatformTypeName.ToLower() == "address")
                        .Select(x => new IntegrationConfigurationInfo { URL = x.URL, ApplicationFieldName = x.ApplicationFieldName, SystemFieldName = x.SystemFieldName }).ToList();
                }
            }

            return response;
        }

        #region Out Bound Integration Credentials

        /// <summary>
        /// Checkout bound Integration Credentials Exit for the particular config against the platform
        /// From mApplication returns boolean data.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>request</returns>
        public async Task<bool> CheckOutBoundConfigExit(OutBoundIntegrationCredentialsReq request)
        {

            var ConfigInfo = _MongoContext.mApplications.AsQueryable().Where(a => a.Application_Id == request.Application_Id).FirstOrDefault();
            if (ConfigInfo != null && ConfigInfo.Configurations != null && ConfigInfo.Configurations.Any())
            {
                var ConfigItem = ConfigInfo.Configurations.Where(a => a.Key.ToLower() == request.Key.ToLower() /*&& a.Value.ToLower() == request.Value.ToLower() */&& a.Status == "").FirstOrDefault();
                if (ConfigItem != null)
                {
                    if (!string.IsNullOrEmpty(request.ConfigId))
                    {
                        if (ConfigItem.ConfigId != request.ConfigId)
                        {
                            return true;
                        }
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Save Out Bound Integration Credentials for the selected Platform with Key and Value
        /// Save to mApplication.Configuration with the key and it's value.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<OutBoundIntegrationCredentialsRes> SaveOutBoundIntegrationCredentials(OutBoundIntegrationCredentialsReq request)
        {
            OutBoundIntegrationCredentialsRes response = new OutBoundIntegrationCredentialsRes();

            mApplications applicationInfo = _MongoContext.mApplications.AsQueryable().Where(j => j.Application_Id == request.Application_Id).FirstOrDefault();
            Integration_Configuration configInfo = new Integration_Configuration();

            try
            {
                if (!string.IsNullOrEmpty(request.ConfigId))
                {
                    configInfo = applicationInfo.Configurations.Where(j => j.ConfigId == request.ConfigId).FirstOrDefault();
                    configInfo.Key = request.Key.Trim();
                    configInfo.Value = Encrypt.EncryptData(configInfo.Key, request.Value.Trim());
                    //configInfo.Value = configInfo.Value.Replace("/", "|");
                    configInfo.Status = string.Empty;
                    configInfo.EditUser = request.EditUser;
                    configInfo.EditDate = DateTime.Now;
                }
                else
                {
                    configInfo = applicationInfo.Configurations != null && applicationInfo.Configurations.Any() ? applicationInfo.Configurations.Where(j => j.Key.ToLower() == request.Key.ToLower() && j.Status != "").FirstOrDefault() : null;
                    if (configInfo != null)
                    {
                        configInfo.Key = request.Key.Trim();
                        configInfo.Value = Encrypt.EncryptData(configInfo.Key, request.Value.Trim());
                        //configInfo.Value = configInfo.Value.Replace("/", "|");
                        configInfo.Status = string.Empty;
                        configInfo.EditUser = request.EditUser;
                        configInfo.EditDate = DateTime.Now;
                    }
                    else
                    {
                        if (applicationInfo.Configurations == null)
                        {
                            applicationInfo.Configurations = new List<Integration_Configuration>();
                        }

                        configInfo = new Integration_Configuration();
                        configInfo.ConfigId = Guid.NewGuid().ToString();
                        configInfo.Key = request.Key.Trim();
                        configInfo.Value = Encrypt.EncryptData(configInfo.Key, request.Value.Trim());
                        //configInfo.Value = configInfo.Value.Replace("/", "|");
                        configInfo.Status = string.Empty;
                        configInfo.CreateUser = request.CreatedUser;
                        configInfo.CreateDate = DateTime.Now;
                        applicationInfo.Configurations.Add(configInfo);
                    }

                }

                var finalResult = await _MongoContext.mApplications.FindOneAndUpdateAsync(mApplications => mApplications.Application_Id == request.Application_Id && mApplications.Status == "",
                                            Builders<mApplications>.Update.Set("Configurations", applicationInfo.Configurations));

                response.ResponseStatus.Status = "Success";
                response.ResponseStatus.ErrorMessage = "Application Configuration saved successfully.";
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// GetOutBoundIntegrationCredentialsList used for binding the grid or to list
        /// Fetched all the Application and it's config from mApplication like MSDYNAMICS etc.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<OutBoundIntegrationCredentialsRes> GetOutBoundIntegrationCredentialsList(OutBoundIntegrationCredentialsReq request)
        {
            OutBoundIntegrationCredentialsRes response = new OutBoundIntegrationCredentialsRes();

            try
            {
                IQueryable<mApplications> ApplicationInfo;
                var dataList = _MongoContext.mApplications.AsQueryable();
                if (dataList != null && dataList.Any())
                {
                    ApplicationInfo = !string.IsNullOrEmpty(request.Application_Id) ? dataList.Where(a => a.Application_Id == request.Application_Id && a.Status == "") : 
                        !string.IsNullOrEmpty(request.Application_Name) ? dataList.Where(a => a.Application_Name.ToLower() == request.Application_Name.ToLower() && a.Status == "") : dataList;

                    if (ApplicationInfo != null && ApplicationInfo.Any())
                    {
                        foreach (var item in ApplicationInfo)
                        {
                            if (item.Configurations != null && item.Configurations.Any())
                            {
                                foreach (var subItem in item.Configurations)
                                {
                                    if (subItem.Status == string.Empty)
                                    {
                                        OutBoundIntegration_Search_Data listItem = new OutBoundIntegration_Search_Data();
                                        listItem.Application_Id = item.Application_Id;
                                        listItem.Application_Name = item.Application_Name;
                                        listItem.ConfigId = subItem.ConfigId;
                                        listItem.Key = subItem.Key;
                                        listItem.Value = subItem.Value.Replace("/", "|");
                                        listItem.DecryptedValue = Encrypt.DecryptData(listItem.Key, subItem.Value);
                                        response.OutBoundIntegrationSearchDataList.Add(listItem); 
                                    }
                                } 
                            }
                        }

                        if (!string.IsNullOrEmpty(request.Key))
                        {
                            response.OutBoundIntegrationSearchDataList = response.OutBoundIntegrationSearchDataList.Where(a => a.Key.ToLower().Contains(request.Key.ToLower())).ToList();
                        }

                        if (!string.IsNullOrEmpty(request.Value))
                        {
                            response.OutBoundIntegrationSearchDataList = response.OutBoundIntegrationSearchDataList.Where(a => a.DecryptedValue.ToLower().Contains(request.Value.ToLower())).ToList();
                        }

                        response.TotalCount = response.OutBoundIntegrationSearchDataList.Count;
                        if (request.Length != 0)
                        {
                            response.OutBoundIntegrationSearchDataList = response.OutBoundIntegrationSearchDataList.OrderBy(x => x.Application_Name).ThenBy(x => x.Key).Skip(request.Start).Take(request.Length).ToList(); 
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Delete Out Bound Integration Credentials for the selected row
        /// Delete means it's a soft delete where status is set to 'X' to mApplication.Configuration.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<OutBoundIntegrationCredentialsRes> DeleteOutBoundIntegrationCredentials(OutBoundIntegrationCredentialsReq request)
        {
            OutBoundIntegrationCredentialsRes response = new OutBoundIntegrationCredentialsRes();

            var selectedApplication = _MongoContext.mApplications.AsQueryable().Where(a => a.Application_Id == request.Application_Id).FirstOrDefault();

            var selectedConfig = selectedApplication.Configurations.Where(a => a.ConfigId == request.ConfigId && a.Status == "").FirstOrDefault();

            if (selectedConfig != null)
            {
                selectedConfig.EditUser = request.EditUser;
                selectedConfig.EditDate = DateTime.Now;
                selectedConfig.Status = "X";

                var updated = await _MongoContext.mApplications.FindOneAndUpdateAsync(m => m.Application_Id == request.Application_Id && m.Status == "",
                                                Builders<mApplications>.Update.Set("Configurations", selectedApplication.Configurations));
            }

            response.ResponseStatus.Status = "Success";
            response.ResponseStatus.ErrorMessage = "Record deleted successfully.";

            return response;
        }

        #endregion

        #region Integration Application Mapping Data

        /// <summary>
        /// Check Integration Application Mapping Exit for the particular Platform, Type and Entity
        /// From mIntegartionApplicationData returns boolean data.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<bool> CheckApplicationMappingExists(IntegrationMappingDataReq request)
        {
            var IntegrationApplicationData = _MongoContext.mIntegrationApplicationData.AsQueryable().Where(a => a.Application == request.Application && string.IsNullOrEmpty(a.Status)).FirstOrDefault();
            if (IntegrationApplicationData != null)
            {
                var SelectedInfo = !string.IsNullOrEmpty(request.IntegrationApplicationMapping_Id) 
                    ? IntegrationApplicationData.ApplicationMappings.Where(i => i.Type.ToLower() == request.Type.ToLower() && i.Entity.ToLower() == request.Entity.ToLower() && i.IntegrationApplicationMapping_Id != request.IntegrationApplicationMapping_Id && i.Status == "").FirstOrDefault() 
                    : IntegrationApplicationData.ApplicationMappings.Where(i => i.Type.ToLower() == request.Type.ToLower() && i.Entity.ToLower() == request.Entity.ToLower() && i.Status == "").FirstOrDefault();
                if (SelectedInfo != null)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Save Integration Application Mapping for the selected Platform with Type and Entity
        /// Save to mIntegrationApplicationData with respective info Application Mapping.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IntegrationMappingDataRes> SaveIntegrationApplicationMappingInfo(IntegrationMappingDataReq request)
        {
            IntegrationMappingDataRes response = new IntegrationMappingDataRes();

            try
            {
                var IntegrationApplicationData = _MongoContext.mIntegrationApplicationData.AsQueryable().Where(a => a.Application == request.Application).FirstOrDefault();
                
                if (IntegrationApplicationData == null)
                {
                    mIntegrationApplicationData newInfo = new mIntegrationApplicationData();
                    newInfo.IntegrationApplicationData_Id = Guid.NewGuid().ToString();
                    newInfo.Application = request.Application;
                    newInfo.ApplicationName = request.ApplicationName;
                    newInfo.CreateUser = request.CreateUser;
                    newInfo.CreateDate = DateTime.Now;
                    newInfo.Status = string.Empty;

                    newInfo.ApplicationMappings = new List<ApplicationMapping>();

                    ApplicationMapping EntityTypeInfo = new ApplicationMapping();
                    EntityTypeInfo.IntegrationApplicationMapping_Id = Guid.NewGuid().ToString();
                    EntityTypeInfo.Type = request.Type;
                    EntityTypeInfo.Entity = request.Entity.Trim();
                    EntityTypeInfo.Status = string.Empty;
                    EntityTypeInfo.CreateDate = DateTime.Now.ToString();
                    EntityTypeInfo.CreateUser = request.CreateUser;

                    newInfo.ApplicationMappings.Add(EntityTypeInfo);

                    _MongoContext.mIntegrationApplicationData.InsertOne(newInfo);

                    response.ResponseStatus.Status = "Success";
                    response.ResponseStatus.ErrorMessage = "Integration Application Mapping saved successfully.";
                }
                else
                {
                    var AppMappingInfo = !string.IsNullOrEmpty(request.IntegrationApplicationMapping_Id) ?
                        IntegrationApplicationData.ApplicationMappings.Where(i => i.IntegrationApplicationMapping_Id == request.IntegrationApplicationMapping_Id).FirstOrDefault() : 
                        IntegrationApplicationData.ApplicationMappings.Where(i => i.Type.ToLower() == request.Type.ToLower() && i.Entity.ToLower() == request.Entity.ToLower()).FirstOrDefault();

                    if (IntegrationApplicationData.ApplicationMappings != null && IntegrationApplicationData.ApplicationMappings.Any() 
                        && AppMappingInfo != null && !string.IsNullOrEmpty(AppMappingInfo.IntegrationApplicationMapping_Id))
                    {
                        AppMappingInfo.Type = request.Type;
                        AppMappingInfo.Entity = request.Entity;
                        AppMappingInfo.EditDate = DateTime.Now.ToString();
                        AppMappingInfo.EditUser = request.EditUser;
                        AppMappingInfo.Status = string.Empty;
                    }
                    else
                    {
                        if(IntegrationApplicationData.ApplicationMappings == null)
                            IntegrationApplicationData.ApplicationMappings = new List<ApplicationMapping>();

                        ApplicationMapping EntityTypeInfo = new ApplicationMapping();
                        EntityTypeInfo.IntegrationApplicationMapping_Id = Guid.NewGuid().ToString();
                        EntityTypeInfo.Type = request.Type;
                        EntityTypeInfo.Entity = request.Entity.Trim();
                        EntityTypeInfo.Status = string.Empty;
                        EntityTypeInfo.CreateDate = DateTime.Now.ToString();
                        EntityTypeInfo.CreateUser = request.CreateUser;

                        IntegrationApplicationData.ApplicationMappings.Add(EntityTypeInfo);
                    }

                    IntegrationApplicationData.Status = string.Empty;
                    IntegrationApplicationData.EditDate = DateTime.Now;
                    IntegrationApplicationData.EditUser = request.EditUser;

                    var updated = await _MongoContext.mIntegrationApplicationData.FindOneAndUpdateAsync(m => m.Application == request.Application,
                                            Builders<mIntegrationApplicationData>.Update.Set(m => m.ApplicationMappings, IntegrationApplicationData.ApplicationMappings));

                    response.ResponseStatus.Status = "Success";
                    response.ResponseStatus.ErrorMessage = "Integration Application Mapping saved successfully.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// GetApplicationMappingList used for binding the grid or to list
        /// Fetched all the IntegrationApplicationData from mIntegrationApplicationData like MSDYNAMICS, Master|PickList, Type, Entity etc.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IntegrationMappingDataRes> GetApplicationMappingList(IntegrationMappingDataReq request)
        {
            IntegrationMappingDataRes response = new IntegrationMappingDataRes();

            try
            {
                var dataList = !string.IsNullOrEmpty(request.Application) ?
                    _MongoContext.mIntegrationApplicationData.AsQueryable().Where(i => i.Application == request.Application && string.IsNullOrEmpty(i.Status)) : _MongoContext.mIntegrationApplicationData.AsQueryable().Where(i => string.IsNullOrEmpty(i.Status));

                response.IntegrationMappingList = new List<IntegrationMappingInfo>();

                if (dataList != null && dataList.Any())
                {
                    foreach (var item in dataList)
                    {
                        if (item.ApplicationMappings != null && item.ApplicationMappings.Any())
                        {
                            List<IntegrationMappingInfo> ApplicationMappingList = new List<IntegrationMappingInfo>();
                            ApplicationMappingList = item.ApplicationMappings.Where(i => i.Status == "").Select(x =>
                              new IntegrationMappingInfo
                              {
                                  Application = item.Application,
                                  ApplicationName = item.ApplicationName,
                                  IntegrationApplicationData_Id = item.IntegrationApplicationData_Id,
                                  IntegrationApplicationMapping_Id = x.IntegrationApplicationMapping_Id,
                                  Type = x.Type,
                                  Entity = x.Entity
                              }).ToList();

                            response.IntegrationMappingList.AddRange(ApplicationMappingList);
                        }
                    }
                }

                if (response.IntegrationMappingList != null && response.IntegrationMappingList.Any())
                {
                    if (!string.IsNullOrEmpty(request.Type))
                    {
                        response.IntegrationMappingList = response.IntegrationMappingList.Where(a => a.Type == request.Type).ToList();
                    }
                    if (!string.IsNullOrEmpty(request.Entity))
                    {
                        response.IntegrationMappingList = response.IntegrationMappingList.Where(a => a.Entity.ToLower().Contains(request.Entity.ToLower())).ToList();
                    }

                    response.TotalCount = response.IntegrationMappingList.Count;
                    response.IntegrationMappingList = response.IntegrationMappingList.OrderBy(x => x.ApplicationName).ThenBy(x => x.Type).ThenBy(x => x.Entity).Skip(request.Start).Take(request.Length).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// GetApplicationMappingDataList used for binding the grid or to list
        /// Fetched all the IntegrationApplicationData from mIntegrationApplicationData like MSDYNAMICS, Master|PickList, Type, Entity etc.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IntegrationMappingDataRes> GetApplicationMappingDataList(IntegrationMappingDataReq request)
        {
            IntegrationMappingDataRes response = new IntegrationMappingDataRes();

            try
            {
                var dataList = _MongoContext.mIntegrationApplicationData.AsQueryable().Where(i => i.Application == request.Application && string.IsNullOrEmpty(i.Status)).FirstOrDefault();

                //var ApplicationMappingsInfo = dataList.ApplicationMappings.Where(a=>a.IntegrationApplicationMapping_Id == request.IntegrationApplicationMapping_Id && a.Status == "").FirstOrDefault();
                var ApplicationMappingsInfo = dataList.ApplicationMappings.Where(a => a.IntegrationApplicationMapping_Id == request.IntegrationApplicationMapping_Id && a.Status == "" && a.Mappings != null && a.Mappings.Any()).FirstOrDefault();

                response.IntegrationMappingItemList = new List<IntegrationMappingItemInfo>();

                if (ApplicationMappingsInfo != null)
                {
                    List<IntegrationMappingItemInfo> itemData = new List<IntegrationMappingItemInfo>();

                    if (ApplicationMappingsInfo.Mappings != null && ApplicationMappingsInfo.Mappings.Any())
                    {
                        itemData = ApplicationMappingsInfo.Mappings.Where(i => i.Status == "").Select(x =>
                                                new IntegrationMappingItemInfo
                                                {
                                                    IntegrationApplicationMappingItem_Id = x.IntegrationApplicationMappingItem_Id,
                                                    PartnerEntityCode = x.PartnerEntityCode,
                                                    PartnerEntityName = x.PartnerEntityName,
                                                    SystemEntityCode = x.SystemEntityCode,
                                                    SystemEntityName = x.SystemEntityName
                                                }).ToList();

                        response.IntegrationMappingItemList.AddRange(itemData); 
                    }

                    if (response.IntegrationMappingItemList != null && response.IntegrationMappingItemList.Any())
                    {
                        if (!string.IsNullOrEmpty(request.PartnerEntityCode))
                        {
                            response.IntegrationMappingItemList = response.IntegrationMappingItemList.Where(a => a.PartnerEntityCode.ToLower() == request.PartnerEntityCode.ToLower()).ToList();
                        }
                        if (!string.IsNullOrEmpty(request.PartnerEntityName))
                        {
                            response.IntegrationMappingItemList = response.IntegrationMappingItemList.Where(a => a.PartnerEntityName.ToLower() == request.PartnerEntityName.ToLower()).ToList();
                        }
                        if (!string.IsNullOrEmpty(request.SystemEntityName))
                        {
                            response.IntegrationMappingItemList = response.IntegrationMappingItemList.Where(a => a.SystemEntityName.ToLower() == request.SystemEntityName.ToLower()).ToList();
                        }
                        if (!string.IsNullOrEmpty(request.SystemEntityCode))
                        {
                            response.IntegrationMappingItemList = response.IntegrationMappingItemList.Where(a => a.SystemEntityCode.ToLower() == request.SystemEntityCode.ToLower()).ToList();
                        }

                        response.TotalCount = response.IntegrationMappingItemList.Count;
                        response.IntegrationMappingItemList = response.IntegrationMappingItemList.OrderBy(x => x.PartnerEntityName).ThenBy(x => x.SystemEntityName).Skip(request.Start).Take(request.Length).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Check Integration Application Mapping Exit for the particular PartnerEntityCode, PartnerEntityName, SystemEntityName and SystemEntityCode
        /// From mIntegartionApplicationData returns boolean data.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<bool> CheckApplicationMappingDataExists(IntegrationMappingDataReq request)
        {
            var IntegrationApplicationData = _MongoContext.mIntegrationApplicationData.AsQueryable().Where(a => a.Application == request.Application && string.IsNullOrEmpty(a.Status)).FirstOrDefault();
            if (IntegrationApplicationData != null)
            {
                var SelectedInfo = !string.IsNullOrEmpty(request.IntegrationApplicationMapping_Id)
                    ? IntegrationApplicationData.ApplicationMappings.Where(i => i.IntegrationApplicationMapping_Id == request.IntegrationApplicationMapping_Id && i.Status == "").FirstOrDefault()
                    : null;
                if (SelectedInfo != null && SelectedInfo.Mappings != null && SelectedInfo.Mappings.Any())
                {
                    var dataItem = string.IsNullOrEmpty(request.IntegrationApplicationMappingItem_Id) ? SelectedInfo.Mappings.Where(i => i.PartnerEntityCode.ToLower() == request.PartnerEntityCode.ToLower() && i.PartnerEntityName.ToLower() == request.PartnerEntityName.ToLower()
                    && i.SystemEntityCode.ToLower() == request.SystemEntityCode.ToLower() && i.SystemEntityName.ToLower() == request.SystemEntityName.ToLower() && i.Status == "").FirstOrDefault() :
                    SelectedInfo.Mappings.Where(i => i.IntegrationApplicationMappingItem_Id != request.IntegrationApplicationMappingItem_Id && i.PartnerEntityCode.ToLower() == request.PartnerEntityCode.ToLower() && i.PartnerEntityName.ToLower() == request.PartnerEntityName.ToLower()
                    && i.SystemEntityCode.ToLower() == request.SystemEntityCode.ToLower() && i.SystemEntityName.ToLower() == request.SystemEntityName.ToLower() && i.Status == "").FirstOrDefault();

                    if (dataItem != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Save Integration Application Mapping for the selected PartnerEntityCode, PartnerEntityName, SystemEntityName and SystemEntityCode
        /// Save to mIntegrationApplicationData with respective info Application Mapping.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IntegrationMappingDataRes> SaveIntegrationApplicationMappingDataInfo(IntegrationMappingDataReq request)
        {
            IntegrationMappingDataRes response = new IntegrationMappingDataRes();

            try
            {
                FilterDefinition<mIntegrationApplicationData> filter;
                filter = Builders<mIntegrationApplicationData>.Filter.Empty;

                if (!string.IsNullOrEmpty(request.Application))
                {
                    filter = filter & Builders<mIntegrationApplicationData>.Filter.Where(x => x.Application == request.Application);
                }

                if (!string.IsNullOrEmpty(request.IntegrationApplicationMapping_Id))
                {
                    filter = filter & Builders<mIntegrationApplicationData>.Filter.Where(x => x.ApplicationMappings != null && x.ApplicationMappings.Any(a => a.IntegrationApplicationMapping_Id == request.IntegrationApplicationMapping_Id));
                }

                var finalOutput = await _MongoContext.mIntegrationApplicationData.Find(filter).ToListAsync();
                var IntegrationApplicationData = finalOutput.Select(a => a.ApplicationMappings).FirstOrDefault();

                ApplicationMappingData itemData = new ApplicationMappingData();
                if (IntegrationApplicationData != null && IntegrationApplicationData.Any())
                {
                    var ApplicationMapping = IntegrationApplicationData.Where(i => i.IntegrationApplicationMapping_Id == request.IntegrationApplicationMapping_Id).FirstOrDefault();
                    itemData = !string.IsNullOrEmpty(request.IntegrationApplicationMappingItem_Id) ? ApplicationMapping.Mappings != null && ApplicationMapping.Mappings.Any() ? 
                        ApplicationMapping.Mappings.Where(i=>i.IntegrationApplicationMappingItem_Id == request.IntegrationApplicationMappingItem_Id).FirstOrDefault() : 
                        ApplicationMapping.Mappings.Where(i => i.PartnerEntityCode.ToLower() == request.PartnerEntityCode.ToLower() && i.PartnerEntityName.ToLower() == request.PartnerEntityName.ToLower()
                            && i.SystemEntityCode.ToLower() == request.SystemEntityCode.ToLower() && i.SystemEntityName.ToLower() == request.SystemEntityName.ToLower()).FirstOrDefault() :
                        ApplicationMapping.Mappings != null && ApplicationMapping.Mappings.Any() ? ApplicationMapping.Mappings.Where(i => i.PartnerEntityCode.ToLower() == request.PartnerEntityCode.ToLower() && i.PartnerEntityName.ToLower() == request.PartnerEntityName.ToLower()
                            && i.SystemEntityCode.ToLower() == request.SystemEntityCode.ToLower() && i.SystemEntityName.ToLower() == request.SystemEntityName.ToLower()).FirstOrDefault() : new ApplicationMappingData();

                    if (itemData != null && !string.IsNullOrEmpty(itemData.IntegrationApplicationMappingItem_Id))
                    {
                        //itemData = item.Mappings.Where(i => i.IntegrationApplicationMappingItem_Id == request.IntegrationApplicationMappingItem_Id).FirstOrDefault();
                        itemData.PartnerEntityCode = request.PartnerEntityCode.Trim();
                        itemData.PartnerEntityName = request.PartnerEntityName.Trim();
                        itemData.SystemEntityCode = request.SystemEntityCode.Trim();
                        itemData.SystemEntityName = request.SystemEntityName.Trim();
                        itemData.Status = string.Empty;
                        itemData.EditDate = DateTime.Now.ToString();
                        itemData.EditUser = request.EditUser;
                    }
                    else 
                    {
                        if (ApplicationMapping.Mappings == null)
                        ApplicationMapping.Mappings = new List<ApplicationMappingData>();

                        if (itemData == null)
                            itemData = new ApplicationMappingData();

                        itemData.IntegrationApplicationMappingItem_Id = Guid.NewGuid().ToString();
                        itemData.PartnerEntityCode = request.PartnerEntityCode.Trim();
                        itemData.PartnerEntityName = request.PartnerEntityName.Trim();
                        itemData.SystemEntityCode = request.SystemEntityCode.Trim();
                        itemData.SystemEntityName = request.SystemEntityName.Trim();
                        itemData.Status = string.Empty;
                        itemData.CreateDate = DateTime.Now.ToString();
                        itemData.CreateUser = request.CreateUser;
                        ApplicationMapping.Mappings.Add(itemData);
                    }

                    var updated = await _MongoContext.mIntegrationApplicationData.FindOneAndUpdateAsync(m => m.Application == request.Application,
                                            Builders<mIntegrationApplicationData>.Update.Set("ApplicationMappings", IntegrationApplicationData));

                    response.ResponseStatus.Status = "Success";
                    response.ResponseStatus.ErrorMessage = "Application Mapping Data saved successfully.";
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Application Mapping Data doesn't exists.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Delete Integration Application Mapping for the selected Application, Type and Entity
        /// Delete means it's a soft delete where status is set to 'X' to mIntegrationPlatform with the Application Mapping Data.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IntegrationMappingDataRes> DeleteIntegrationApplicationMappingInfo(IntegrationMappingDataReq request)
        {
            IntegrationMappingDataRes response = new IntegrationMappingDataRes();

            var IntegrationApplicationData = _MongoContext.mIntegrationApplicationData.AsQueryable().Where(a => a.Application == request.Application).FirstOrDefault();
            if (IntegrationApplicationData != null)
            {
                var SelectedInfo = !string.IsNullOrEmpty(request.IntegrationApplicationMapping_Id)
                    ? IntegrationApplicationData.ApplicationMappings.Where(i => i.IntegrationApplicationMapping_Id == request.IntegrationApplicationMapping_Id && i.Status == "").FirstOrDefault()
                    : null;
                if (SelectedInfo != null)
                {
                    if (SelectedInfo.Mappings != null && SelectedInfo.Mappings.Any())
                    {
                        foreach (var item in SelectedInfo.Mappings)
                        {
                            item.Status = "X";
                            item.EditDate = DateTime.Now.ToString();
                            item.EditUser = request.EditUser;
                        }

                    }

                    SelectedInfo.Status = "X";
                    SelectedInfo.EditDate = DateTime.Now.ToString();
                    SelectedInfo.EditUser = request.EditUser;

                    var updated = await _MongoContext.mIntegrationApplicationData.FindOneAndUpdateAsync(m => m.Application == request.Application,
                                                    Builders<mIntegrationApplicationData>.Update.Set("ApplicationMappings", IntegrationApplicationData.ApplicationMappings));

                    response.ResponseStatus.Status = "Success";
                    response.ResponseStatus.ErrorMessage = "Record deleted successfully.";
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Application Mapping Data doesn't exists.";
                }
            }
            else
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "Application Mapping Data doesn't exists.";
            }

            return response;
        }

        /// <summary>
        /// Delete Integration Application Mapping Data for the selected Application, Type and Entity
        /// Delete means it's a soft delete where status is set to 'X' to mIntegrationPlatform with the Application Mapping Data.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IntegrationMappingDataRes> DeleteIntegrationApplicationMappingItemsInfo(IntegrationMappingDataReq request)
        {
            IntegrationMappingDataRes response = new IntegrationMappingDataRes();

            var IntegrationApplicationData = _MongoContext.mIntegrationApplicationData.AsQueryable().Where(a => a.Application == request.Application).FirstOrDefault();
            if (IntegrationApplicationData != null)
            {
                var SelectedInfo = !string.IsNullOrEmpty(request.IntegrationApplicationMapping_Id)
                    ? IntegrationApplicationData.ApplicationMappings.Where(i => i.IntegrationApplicationMapping_Id == request.IntegrationApplicationMapping_Id && i.Status == "").FirstOrDefault()
                    : null;

                if (SelectedInfo != null)
                {
                    if (SelectedInfo.Mappings != null && SelectedInfo.Mappings.Any() && !string.IsNullOrEmpty(request.IntegrationApplicationMappingItem_Id))
                    {
                        var mappingData = SelectedInfo.Mappings.Where(a => a.IntegrationApplicationMappingItem_Id == request.IntegrationApplicationMappingItem_Id).FirstOrDefault();

                        if (mappingData != null)
                        {
                            mappingData.Status = "X";
                            mappingData.EditDate = DateTime.Now.ToString();
                            mappingData.EditUser = request.EditUser;

                            var updated = await _MongoContext.mIntegrationApplicationData.FindOneAndUpdateAsync(m => m.Application == request.Application,
                                                            Builders<mIntegrationApplicationData>.Update.Set("ApplicationMappings", IntegrationApplicationData.ApplicationMappings));

                            response.ResponseStatus.Status = "Success";
                            response.ResponseStatus.ErrorMessage = "Record deleted successfully.";
                        }
                        else
                        {
                            response.ResponseStatus.Status = "Failure";
                            response.ResponseStatus.ErrorMessage = "Mapping Data doesn't exists.";
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "Mapping Data doesn't exists.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Mapping Data doesn't exists.";
                }
            }
            else
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "Mapping Data doesn't exists.";
            }

            return response;
        }

        #endregion
    }
}
