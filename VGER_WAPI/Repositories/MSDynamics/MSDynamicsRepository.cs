using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI.Proxy;
using VGER_WAPI.Helpers;
using VGER_WAPI.Models;
using VGER_WAPI.Providers;
using VGER_WAPI_CLASSES;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace VGER_WAPI.Repositories
{
    public class MSDynamicsRepository: IMSDynamicsRepository
    {

        private readonly MongoContext _MongoContext = null;
        private readonly IQuoteRepository _quoteRepository;
        private readonly ICommonRepository _commonRepository;
        private MSDynamicsProviders _msDynamicsProviders;
        private readonly IConfiguration _configuration;
        private readonly IGenericRepository _genericRepository;

        public MSDynamicsRepository(IOptions<MongoSettings> settings, IQuoteRepository quoteRepository, ICommonRepository commonRepository, IConfiguration configuration, IGenericRepository genericRepository)
        {
            _MongoContext = new MongoContext(settings);
            _quoteRepository = quoteRepository;
            _commonRepository = commonRepository;
            _configuration = configuration;
            _msDynamicsProviders = new MSDynamicsProviders(_configuration);
            _genericRepository = genericRepository;
        }

        #region Integration Services

        public async Task<ResponseStatus> CreateOpportunity(string QRFId, string ParentQRFId, string ckLoginUser_Id)
        {
            var response = new ResponseStatus();
            try
            {
                IntegrationOpportunityReq requestInfo = new IntegrationOpportunityReq();

                requestInfo.OpportunityInfo = _quoteRepository.getQuoteInfo(QRFId).Result;

                var QrfPriceInfo = _quoteRepository.getQuotePriceInfo(QRFId).Result;

                if (QrfPriceInfo != null && !string.IsNullOrEmpty(QrfPriceInfo.QRFPrice_Id))
                {
                    requestInfo.OpportunityQRFPriceInfo = QrfPriceInfo;
                }

                //requestInfo.StatusCode = "Assigned";//ASSIGNED

                //Check Parent exists any mapping or not if yes than create Opportunity in CRM
                var ParentOpportunityInfo = _MongoContext.mQuote.AsQueryable().Where(a => a.QRFID == ParentQRFId).FirstOrDefault();

                var ParentMappingInfo = ParentOpportunityInfo.Mappings.Where(a => a.PartnerEntityType.ToLower() == "OPPORTUNITY".ToLower() && string.IsNullOrEmpty(a.Status)).FirstOrDefault();

                if (ParentMappingInfo != null)
                {
                    Integration_Search_Data IntegrationSearchData = _commonRepository.GetIntegrationCredentials(ParentMappingInfo.Application, ckLoginUser_Id).Result;

                    requestInfo.CredentialInfo.Key = IntegrationSearchData.Keys;
                    requestInfo.CredentialInfo.Source = IntegrationSearchData.Application_Name;
                    requestInfo.CredentialInfo.User = IntegrationSearchData.UserKey;

                    if (string.Compare(requestInfo.OpportunityInfo.CurrentPipeline, _configuration.GetValue<string>("PipeLines:Quote"), true) == 0)
                    {
                        requestInfo.StatusCode = "Assigned";//ASSIGNED
                    }
                    else if (string.Compare(requestInfo.OpportunityInfo.CurrentPipeline, _configuration.GetValue<string>("PipeLines:Costing"), true) == 0 || string.Compare(requestInfo.OpportunityInfo.CurrentPipeline, _configuration.GetValue<string>("PipeLines:CostingApproval"), true) == 0)
                    {
                        requestInfo.StatusCode = "QRF Raised";
                    }
                    else if (string.Compare(requestInfo.OpportunityInfo.CurrentPipeline, _configuration.GetValue<string>("PipeLines:Amendment"), true) == 0)
                    {
                        requestInfo.StatusCode = "REWORK";
                    }
                    else if (string.Compare(requestInfo.OpportunityInfo.CurrentPipeline, _configuration.GetValue<string>("PipeLines:AgentApproval"), true) == 0)
                    {
                        requestInfo.StatusCode = "QRF Raised";
                        var isMailSend = _MongoContext.mDocumentStore.AsQueryable().Where(x => x.QRFID == requestInfo.OpportunityInfo.QRFID && x.QRFPriceId == requestInfo.OpportunityQRFPriceInfo.QRFPrice_Id && (x.DocumentType == DocType.SENDTOCLIENT || x.DocumentType != DocType.MAILAGENTACCEPT)).Any();
                        if (isMailSend)
                        {
                            requestInfo.StatusCode = "AWAITING REVERT FROM CUSTOMER";
                        }
                    }

                    var CompanyInfo = GetCompanyInfo(requestInfo.OpportunityInfo.AgentInfo.AgentID).Result;

                    requestInfo.CustomerId = GetPartnerAccountInfo(CompanyInfo, requestInfo.CredentialInfo.Source);

                    var userInfo = _MongoContext.mUsers.AsQueryable().Where(a => a.VoyagerUser_Id == ckLoginUser_Id).FirstOrDefault();

                    var systemInfo = _MongoContext.mSystem.AsQueryable().Where(a => a.CoreCompany_Id == userInfo.Company_Id).FirstOrDefault();

                    requestInfo.GroupOfCompanies = systemInfo.Mappings.Where(a => a.PartnerEntityName.ToLower() == "Group of Companies".ToLower()).Select(b => b.PartnerEntityCode).FirstOrDefault();
                    requestInfo.GroupCompany = systemInfo.Mappings.Where(a => a.PartnerEntityName.ToLower() == "Group Company".ToLower()).Select(b => b.PartnerEntityCode).FirstOrDefault();
                    requestInfo.CompanyName = systemInfo.Mappings.Where(a => a.PartnerEntityName.ToLower() == "Company Name".ToLower()).Select(b => b.PartnerEntityCode).FirstOrDefault();

                    var agentInfo = GetCompanyInfo(requestInfo.OpportunityInfo.AgentInfo.AgentID).Result;

                    var SystemMappings = !string.IsNullOrEmpty(agentInfo.SystemCompany_Id) ? _MongoContext.mCompanies.AsQueryable().Where(a => a.Company_Id == agentInfo.SystemCompany_Id).Select(b => b.Mappings).FirstOrDefault() : null;
                    requestInfo.BU = SystemMappings != null && SystemMappings.Any() ? SystemMappings.Where(a => a.PartnerEntityName.ToLower() == "BU".ToLower()).Select(b => b.PartnerEntityCode).FirstOrDefault() : string.Empty;

                    requestInfo.CustomerId = agentInfo.Mappings != null && agentInfo.Mappings.Any() ? agentInfo.Mappings.Where(a => a.PartnerEntityType.ToLower() == "Account".ToLower()).Select(b => b.PartnerEntityCode).FirstOrDefault() : string.Empty;

                    var contactList = !string.IsNullOrEmpty(requestInfo.CustomerId) ? agentInfo.ContactDetails.Where(a => a.Mappings != null && a.Mappings.Any(b => !string.IsNullOrEmpty(b.PartnerEntityType) && b.PartnerEntityType.ToLower() == "Contact".ToLower())).Select(x => x.Mappings).FirstOrDefault() : null;
                    requestInfo.ContactId = contactList != null && contactList.Any() ? contactList.Where(b => b.PartnerEntityType.ToLower() == "Contact".ToLower()).Select(x => x.PartnerEntityCode).FirstOrDefault() : string.Empty;

                    requestInfo.OwnerId = userInfo.UserName;

                    requestInfo.SourceOfEnquiry = "Offline";
                    requestInfo.CompanyMarket = "DUBAI";

                    //requestInfo.PartnerEntityCode = request.PartnerEntityCode;
                    requestInfo.ClientType = "B2B";
                    requestInfo.SystemOpportunityType = "Booking Opportunity";
                    requestInfo.SBU = "OBT";//OutBound
                    requestInfo.POS = "SO";//Sales Office

                    requestInfo.StateCode = "0";
                    var ContactInfo = CompanyInfo.ContactDetails.Where(a => a.Contact_Id == requestInfo.OpportunityInfo.AgentInfo.ContactPersonID).FirstOrDefault();
                    requestInfo.Salutation = !string.IsNullOrEmpty(ContactInfo.TITLE) ? ContactInfo.TITLE: !string.IsNullOrEmpty(ContactInfo.CommonTitle) ? ContactInfo.CommonTitle.Replace(".",""): "Mr";
                    requestInfo.ContactFirstName = ContactInfo.FIRSTNAME;
                    requestInfo.ContactLastName = ContactInfo.LastNAME;
                    requestInfo.ContactEmail = ContactInfo.MAIL;
                    requestInfo.ContactMobile = ContactInfo.MOBILE;

                    response = _msDynamicsProviders.CreateNewOpportunityInfo(requestInfo).Result; 
                }
                else
                {
                    response.Status = "Failure";
                    response.ErrorMessage = "No mapping data to call MSDynamics api for Booking Opportunity Post.";
                }
            }
            catch (Exception ex)
            {
                response.Status = "Failure";
                response.ErrorMessage = "Failed while calling MSDynamics api for Booking Opportunity Post.";
            }


            return response;
        }

        public async Task<ResponseStatus> CreateUpdateOpportnity(string QRFId, string ckLoginUser_Id)
        {
            var response = new ResponseStatus();
            try
            {
                IntegrationOpportunityReq requestInfo = new IntegrationOpportunityReq();
                requestInfo.OpportunityInfo = _quoteRepository.getQuoteInfo(QRFId).Result;

                var QrfPriceInfo = _quoteRepository.getQuotePriceInfo(QRFId).Result;

                if (QrfPriceInfo != null && !string.IsNullOrEmpty(QrfPriceInfo.QRFPrice_Id))
                {
                    requestInfo.OpportunityQRFPriceInfo = QrfPriceInfo;
                }

                if (requestInfo.OpportunityInfo.Mappings != null && requestInfo.OpportunityInfo.Mappings.Any())
                {
                    requestInfo.StateCode = "0";//Open=0, won=1, lost=2

                    if (string.Compare(requestInfo.OpportunityInfo.CurrentPipeline, _configuration.GetValue<string>("PipeLines:Quote"), true) == 0)
                    {
                        requestInfo.StatusCode = "Assigned";//ASSIGNED
                    }
                    else if (string.Compare(requestInfo.OpportunityInfo.CurrentPipeline, _configuration.GetValue<string>("PipeLines:Costing"), true) == 0 || string.Compare(requestInfo.OpportunityInfo.CurrentPipeline, _configuration.GetValue<string>("PipeLines:CostingApproval"), true) == 0)
                    {
                        requestInfo.StatusCode = "QRF Raised";
                    }
                    else if (string.Compare(requestInfo.OpportunityInfo.CurrentPipeline, _configuration.GetValue<string>("PipeLines:Amendment"), true) == 0)
                    {
                        requestInfo.StatusCode = "REWORK";
                    }
                    else if (string.Compare(requestInfo.OpportunityInfo.CurrentPipeline, _configuration.GetValue<string>("PipeLines:AgentApproval"), true) == 0)
                    {
                        requestInfo.StatusCode = "QRF Raised";
                        var isMailSend = _MongoContext.mDocumentStore.AsQueryable().Where(x => x.QRFID == requestInfo.OpportunityInfo.QRFID && x.QRFPriceId == requestInfo.OpportunityQRFPriceInfo.QRFPrice_Id && (x.DocumentType == DocType.SENDTOCLIENT || x.DocumentType != DocType.MAILAGENTACCEPT)).Any();
                        if (isMailSend)
                        {
                            requestInfo.StatusCode = "AWAITING REVERT FROM CUSTOMER";
                        }
                    }
                    else if (string.Compare(requestInfo.OpportunityInfo.CurrentPipeline, _configuration.GetValue<string>("PipeLines:Handover"), true) == 0)
                    {
                        requestInfo.StatusCode = "AWAITING BOOKING AMOUNT";
                        //var isBooked = _MongoContext.mGoAhead.AsQueryable().Where(x => x.QRFID == requestInfo.OpportunityInfo.QRFID && x.QRFPriceId == requestInfo.OpportunityQRFPriceInfo.QRFPrice_Id && x.Depatures != null /*&& x.Depatures.Any(c => c.ConfirmStatus == true)*/).Any();
                        //if (isBooked)
                        //{
                        //    requestInfo.StatusCode = "BOOKED";
                        //    //requestInfo.StateCode = "1";
                        //}
                    }

                    var MappingInfo = requestInfo.OpportunityInfo.Mappings.Where(a => a.PartnerEntityType.ToLower() == "OPPORTUNITY".ToLower() && string.IsNullOrEmpty(a.Status)).FirstOrDefault();

                    if (MappingInfo != null && !string.IsNullOrEmpty(MappingInfo.PartnerEntityCode))
                    {
                        Integration_Search_Data IntegrationSearchData = _commonRepository.GetIntegrationCredentials(MappingInfo.Application, ckLoginUser_Id).Result;

                        requestInfo.CredentialInfo.Key = IntegrationSearchData.Keys;
                        requestInfo.CredentialInfo.Source = IntegrationSearchData.Application_Name;
                        requestInfo.CredentialInfo.User = IntegrationSearchData.UserKey;


                        var CompanyInfo = GetCompanyInfo(requestInfo.OpportunityInfo.AgentInfo.AgentID).Result;

                        requestInfo.CustomerId = GetPartnerAccountInfo(CompanyInfo, requestInfo.CredentialInfo.Source);

                        var userInfo = _MongoContext.mUsers.AsQueryable().Where(a => a.VoyagerUser_Id == ckLoginUser_Id).FirstOrDefault();

                        var systemInfo = _MongoContext.mSystem.AsQueryable().Where(a => a.CoreCompany_Id == userInfo.Company_Id).FirstOrDefault();

                        requestInfo.GroupOfCompanies = systemInfo.Mappings.Where(a => a.PartnerEntityName.ToLower() == "Group of Companies".ToLower()).Select(b => b.PartnerEntityCode).FirstOrDefault();
                        requestInfo.GroupCompany = systemInfo.Mappings.Where(a => a.PartnerEntityName.ToLower() == "Group Company".ToLower()).Select(b => b.PartnerEntityCode).FirstOrDefault();
                        requestInfo.CompanyName = systemInfo.Mappings.Where(a => a.PartnerEntityName.ToLower() == "Company Name".ToLower()).Select(b => b.PartnerEntityCode).FirstOrDefault();

                        var agentInfo = GetCompanyInfo(requestInfo.OpportunityInfo.AgentInfo.AgentID).Result;

                        var SystemMappings = !string.IsNullOrEmpty(agentInfo.SystemCompany_Id) ? _MongoContext.mCompanies.AsQueryable().Where(a => a.Company_Id == agentInfo.SystemCompany_Id).Select(b => b.Mappings).FirstOrDefault() : null;
                        requestInfo.BU = SystemMappings != null && SystemMappings.Any() ? SystemMappings.Where(a => a.PartnerEntityName.ToLower() == "BU".ToLower()).Select(b => b.PartnerEntityCode).FirstOrDefault() : string.Empty;

                        requestInfo.CustomerId = agentInfo.Mappings != null && agentInfo.Mappings.Any() ? agentInfo.Mappings.Where(a => a.PartnerEntityType.ToLower() == "Account".ToLower() ).Select(b => b.PartnerEntityCode).FirstOrDefault() : string.Empty;

                        var contactList = !string.IsNullOrEmpty(requestInfo.CustomerId) ? agentInfo.ContactDetails.Where(a => a.Mappings != null && a.Mappings.Any(b => !string.IsNullOrEmpty(b.PartnerEntityType) && b.PartnerEntityType.ToLower() == "Contact".ToLower())).Select(x => x.Mappings).FirstOrDefault(): null;
                        requestInfo.ContactId = contactList != null && contactList.Any() ? contactList.Where(b => b.PartnerEntityType.ToLower() == "Contact".ToLower()).Select(x => x.PartnerEntityCode).FirstOrDefault(): string.Empty;

                        requestInfo.OwnerId = userInfo.UserName;

                        requestInfo.SourceOfEnquiry = "Offline";
                        requestInfo.CompanyMarket = "DUBAI";
                        requestInfo.ClientType = "B2B";
                        requestInfo.SystemOpportunityType = "Booking Opportunity";

                        requestInfo.PartnerEntityCode = MappingInfo.PartnerEntityCode;

                        requestInfo.StateCode = "0";
                        requestInfo.SBU = "OBT";//OutBound
                        requestInfo.POS = "SO";//Sales Office

                        response = _msDynamicsProviders.CreateOpportunity(requestInfo).Result;
                    }
                    else
                    {
                        response.Status = "Failure";
                        response.ErrorMessage = "No mapping data to call MSDynamics api for Booking Opportunity Patch.";
                    }
                }
                else
                {
                    response.Status = "Failure";
                    response.ErrorMessage = "No mapping data to call MSDynamics api for Booking Opportunity Patch.";
                }
            }
            catch (Exception ex)
            {
                response.Status = "Failure";
                response.ErrorMessage = "Failed while calling MSDynamics api for Booking Opportunity Patch.";
                response.StatusMessage = ex.Message;
            }


            return response;
        }

        /// <summary>
        /// Get All the Integration Platform based on login ckUserCompanyId.
        /// Returns mIntegrationPlatform with the Application_Id and ApplicationName and Modules arrays.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public mIntegrationPlatform GetIntegrationPlatformInfo(IntegartionPlatform_Req request)
        {
            var PlatformBasedOnModelAction = _MongoContext.mIntegrationPlatform.AsQueryable().Where(a => a.ApplicationName.ToLower() == request.ApplicationName.ToLower()).FirstOrDefault();
            if (PlatformBasedOnModelAction != null)
            {
                var SelectedModel = PlatformBasedOnModelAction.Modules.Where(a => a.ModuleName.ToLower() == request.ModuleName.ToLower() && a.Status == "");
                if (SelectedModel != null && SelectedModel.Any())
                {
                    var SelectedAction = SelectedModel.FirstOrDefault().Actions.Where(a => a.ActionName.ToLower() == request.ActionName.ToLower() && a.Status == "" && a.TypeName.ToLower() != null && a.TypeName.ToLower() == request.TypeName.ToLower());
                    if (SelectedAction != null && SelectedAction.Any())
                    {
                        return PlatformBasedOnModelAction;
                    }
                }
            }

            return PlatformBasedOnModelAction;
        }

        /// <summary>
        /// GetIntegrationConfigInfo used for binding Configuration Info to text box
        /// Configuration URL, ApplicationFieldName, SystemFieldName from mIntegrationPlatform base on Application, Module and Action.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IntegartionPlatform_Req> GetIntegrationConfigInfo(IntegartionPlatform_Req request)
        {
            var configInfo = GetIntegrationPlatformInfo(request);

            var moduleInfo = configInfo != null ? configInfo.Modules.Where(a => a.ModuleName.ToLower() == request.ModuleName.ToLower() && a.Status == "").FirstOrDefault() : null;
            var actionInfo = moduleInfo != null ? moduleInfo.Actions.Where(a => a.ActionName.ToLower() == request.ActionName.ToLower() && a.Status == "" && a.TypeName.ToLower() == request.TypeName.ToLower()).FirstOrDefault() : null;

            if (actionInfo != null && actionInfo.Configurations != null && actionInfo.Configurations.Any(a => a.Status == ""))
            {
                request.Configurations = actionInfo.Configurations.Where(a=>a.Status == "" && a.BoundType.ToLower() == request.BoundType.ToLower() ).ToList();
            }

            return request;
        }

        public async Task<IntegrationMappingDataRes> GetAllApplicationMappingDataList(IntegrationMappingDataReq request)
        {
            IntegrationMappingDataRes response = new IntegrationMappingDataRes();

            try
            {
                var dataList = !string.IsNullOrEmpty(request.Application) ? _MongoContext.mIntegrationApplicationData.AsQueryable().Where(i => i.Application == request.Application && string.IsNullOrEmpty(i.Status)).FirstOrDefault()
                    : _MongoContext.mIntegrationApplicationData.AsQueryable().Where(i => i.ApplicationName.ToLower() == request.ApplicationName.ToLower() && string.IsNullOrEmpty(i.Status)).FirstOrDefault();

                var ApplicationMappingsInfo = dataList.ApplicationMappings;

                var EntityTypeList = request.TypeEntityInfoList.Select(kp => new Attributes { Attribute_Id = kp.Type.ToLower(), AttributeName = kp.Entity.ToLower() }).ToList();

                response.IntegrationMappingItemList = new List<IntegrationMappingItemInfo>();

                if (EntityTypeList != null && EntityTypeList.Any() && ApplicationMappingsInfo != null && ApplicationMappingsInfo.Any())
                {
                    foreach (var item in ApplicationMappingsInfo)
                    {
                        if (item.Mappings != null && item.Mappings.Any() && EntityTypeList.Where(a=> a.Attribute_Id.Contains(item.Type.ToLower()) && a.AttributeName == item.Entity.ToLower()).Any())
                        {
                            List<IntegrationMappingItemInfo> itemData = new List<IntegrationMappingItemInfo>();
                            
                            itemData = item.Mappings.Where(i => i.Status == "").Select(x =>
                                                new IntegrationMappingItemInfo
                                                {
                                                    Type = item.Type,
                                                    Entity = item.Entity,
                                                    IntegrationApplicationMappingItem_Id = x.IntegrationApplicationMappingItem_Id,
                                                    PartnerEntityCode = x.PartnerEntityCode,
                                                    PartnerEntityName = x.PartnerEntityName,
                                                    SystemEntityCode = x.SystemEntityCode,
                                                    SystemEntityName = x.SystemEntityName
                                                }).ToList();

                            response.IntegrationMappingItemList.AddRange(itemData);
                        }
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

        public async Task<mCompanies> GetCompanyInfo(string CompanyId)
        {
            mCompanies companyInfo = new mCompanies();

            companyInfo = _MongoContext.mCompanies.AsQueryable().Where(a => a.Company_Id == CompanyId).FirstOrDefault();

            return companyInfo;
        }

        public string GetPartnerAccountInfo(mCompanies companyInfo, string Source)
        {
            var PartnerEntityCode = companyInfo.Mappings.Where(a => a.PartnerEntityType.ToLower() == "account" && a.Application.ToLower() == Source.ToLower()).Select(x => x.PartnerEntityCode).FirstOrDefault();
            return PartnerEntityCode;
        }

        public string GetPartnerContactInfo(List<CompanyContacts> CompanyContactsListInfo, string ContactId, string Source)
        {
            var PartnerEntityInfo = CompanyContactsListInfo.Where(a => a.Contact_Id == ContactId && a.Mappings != null && a.Mappings.Any(i => i.Application.ToLower() == Source.ToLower())).Select(x => x.Mappings).FirstOrDefault();
            return PartnerEntityInfo.Select(a => a.PartnerEntityCode).FirstOrDefault();
        }

        public async Task<ResponseStatus> CreateUpdateQuotation(string QRFId, string VoyagerUserId)
        {
            var response = new ResponseStatus();

            try
            {
                IntegrationQuotationReq requestInfo = new IntegrationQuotationReq();

                requestInfo.QuotationInfo = _quoteRepository.getQuoteInfo(QRFId).Result;

                var QrfPriceInfo = _quoteRepository.getQuotePriceInfo(QRFId).Result;

                if (QrfPriceInfo != null && !string.IsNullOrEmpty(QrfPriceInfo.QRFPrice_Id))
                {
                    requestInfo.QuotationQRFPriceInfo = QrfPriceInfo;
                }

                var MappingQuotationInfo = requestInfo.QuotationQRFPriceInfo.Mappings.Where(a => a.PartnerEntityType.ToLower() == "QUOTE".ToLower() && string.IsNullOrEmpty(a.Status)).FirstOrDefault();
                var MappingInfo = requestInfo.QuotationInfo.Mappings.Where(a => a.PartnerEntityType.ToLower() == "OPPORTUNITY".ToLower() && string.IsNullOrEmpty(a.Status)).FirstOrDefault();

                if (MappingInfo != null && !string.IsNullOrEmpty(MappingInfo.PartnerEntityCode))
                {
                    Integration_Search_Data IntegrationSearchData = _commonRepository.GetIntegrationCredentials(MappingInfo.Application, VoyagerUserId).Result;

                    requestInfo.CredentialInfo.Key = IntegrationSearchData.Keys;
                    requestInfo.CredentialInfo.Source = IntegrationSearchData.Application_Name;
                    requestInfo.CredentialInfo.User = IntegrationSearchData.UserKey;

                    requestInfo.CRM_OpportunityId = MappingInfo.PartnerEntityCode;

                    var CompanyInfo = GetCompanyInfo(requestInfo.QuotationQRFPriceInfo.AgentInfo.AgentID).Result;

                    requestInfo.CustomerId = GetPartnerAccountInfo(CompanyInfo, requestInfo.CredentialInfo.Source);

                    var userInfo = _MongoContext.mUsers.AsQueryable().Where(a => a.VoyagerUser_Id == VoyagerUserId).FirstOrDefault();

                    var systemInfo = _MongoContext.mSystem.AsQueryable().Where(a => a.CoreCompany_Id == userInfo.Company_Id).FirstOrDefault();

                    requestInfo.GroupOfCompanies = systemInfo.Mappings.Where(a => a.PartnerEntityName.ToLower() == "Group of Companies".ToLower()).Select(b => b.PartnerEntityCode).FirstOrDefault();
                    requestInfo.GroupCompany = systemInfo.Mappings.Where(a => a.PartnerEntityName.ToLower() == "Group Company".ToLower()).Select(b => b.PartnerEntityCode).FirstOrDefault();
                    requestInfo.CompanyName = systemInfo.Mappings.Where(a => a.PartnerEntityName.ToLower() == "Company Name".ToLower()).Select(b => b.PartnerEntityCode).FirstOrDefault();
                    requestInfo.BookingEngine = systemInfo.Mappings.Where(a => a.PartnerEntityName.ToLower() == "Booking Engine".ToLower()).Select(b => b.PartnerEntityCode).FirstOrDefault();

                    var agentInfo = GetCompanyInfo(requestInfo.QuotationQRFPriceInfo.AgentInfo.AgentID).Result;

                    var SystemMappings = !string.IsNullOrEmpty(agentInfo.SystemCompany_Id) ? _MongoContext.mCompanies.AsQueryable().Where(a => a.Company_Id == agentInfo.SystemCompany_Id).Select(b => b.Mappings).FirstOrDefault() : null;
                    requestInfo.BU = SystemMappings != null && SystemMappings.Any() ? SystemMappings.Where(a => a.PartnerEntityName.ToLower() == "BU".ToLower()).Select(b => b.PartnerEntityCode).FirstOrDefault() : string.Empty;

                    requestInfo.CustomerId = agentInfo.Mappings != null && agentInfo.Mappings.Any() ? agentInfo.Mappings.Where(a => a.PartnerEntityType.ToLower() == "Account".ToLower()).Select(b => b.PartnerEntityCode).FirstOrDefault() : string.Empty;

                    var contactList = !string.IsNullOrEmpty(requestInfo.CustomerId) ? agentInfo.ContactDetails.Where(a => a.Mappings != null && a.Mappings.Any(b => !string.IsNullOrEmpty(b.PartnerEntityType) && b.PartnerEntityType.ToLower() == "Contact".ToLower())).Select(x => x.Mappings).FirstOrDefault() : null;
                    requestInfo.ContactId = contactList != null && contactList.Any() ? contactList.Where(b => b.PartnerEntityType.ToLower() == "Contact".ToLower()).Select(x => x.PartnerEntityCode).FirstOrDefault() : string.Empty;

                    requestInfo.OwnerId = userInfo.UserName;
                    requestInfo.CompanyMarket = "DUBAI";

                    requestInfo.PartnerEntityCode = MappingQuotationInfo != null && !string.IsNullOrEmpty(MappingQuotationInfo.PartnerEntityCode) ? MappingQuotationInfo.PartnerEntityCode: string.Empty;

                    requestInfo.ClientType = "B2B";
                    requestInfo.BookingSource = "Offline";
                    requestInfo.QuotationType = "Offline";
                    requestInfo.HolidayType = "Customized";

                    requestInfo.StatusCode = "In Progress";//Quotation Status
                    bool isMailSendOrAcceptWithOutProposal = _MongoContext.mDocumentStore.AsQueryable().Where(x => x.QRFID == requestInfo.QuotationQRFPriceInfo.QRFID && x.QRFPriceId == requestInfo.QuotationQRFPriceInfo.QRFPrice_Id && ((x.DocumentType == DocType.MAILAGENTACCEPT) || (x.DocumentType == DocType.ACCEPTWITHOUTPROPOSAL))).Any();
                    if (isMailSendOrAcceptWithOutProposal)
                    {
                        requestInfo.StatusCode = "Accepted";
                    }

                    requestInfo.ProductType = "HOLIDAY";
                    requestInfo.TypeOfProduct = "HOLIDAY";
                    requestInfo.SBU = "OBT";//OutBound
                    requestInfo.POS = "SO";//Sales Office

                    requestInfo.ProductLineItemList = GetQuotationCalculatedList(requestInfo.QuotationQRFPriceInfo);

                    response = _msDynamicsProviders.CreateUpdateQuotation(requestInfo).Result;

                    if (response != null && !string.IsNullOrEmpty(response.Status) && response.Status == "Success")
                    {
                        Task.Run(() => CreateUpdateOpportnity(QRFId, VoyagerUserId).Result); 
                    }
                }


            }
            catch (Exception ex)
            {
                response.Status = "Failure";
                response.ErrorMessage = "Failed while calling MSDynamics api for Quotation POST/PATCH.";
                response.StatusMessage = ex.Message;
            }

            return response;
        }

        public List<ProductLineItemQuotation> GetQuotationCalculatedList(mQRFPrice QuotationQRFPriceInfo)
        {
            List<ProductLineItemQuotation> plItemList = new List<ProductLineItemQuotation>();

            //get all the PassangerInfo with out 0 count.
            var setAgentPassangerInfo = QuotationQRFPriceInfo.AgentPassengerInfo.Where(b => b.count != 0).Select(a => new QRFAgentPassengerInfo { Type = a.Type, count = a.count, Age = a.Age }).ToList();
            //get the QRFAgentRoom based on no. of room type selected.
            var setQRFAgentRoom = QuotationQRFPriceInfo.QRFAgentRoom.Where(a => a.RoomCount != 0).Select(x => new QRFAgentRoom { RoomTypeID = x.RoomTypeID, RoomCount = x.RoomCount, RoomTypeName = x.RoomTypeName }).ToList();
            //get the earliest DepartureInfo.
            var setEarliestDeparture = QuotationQRFPriceInfo.Departures.Where(x => x.IsDeleted == false).OrderBy(a => a.Date).FirstOrDefault();

            //get PaxSlab based on range and AgentPassangerInfo
            //List<QRFPaxSlabs> QRFPaxSlabsList = new List<QRFPaxSlabs>();
            List<long> QRFPaxSlabsIdList = new List<long>();
            if (setAgentPassangerInfo != null && setAgentPassangerInfo.Any())
            {
                foreach (var itemPassanger in setAgentPassangerInfo)
                {
                    if (itemPassanger.Type == "ADULT")
                    {
                        //var selectedPaxSlabList = QuotationQRFPriceInfo.PaxSlabDetails.QRFPaxSlabs.Where(a => ((a.From > itemPassanger.count) || (a.From < itemPassanger.count && a.To > itemPassanger.count)) && a.IsDeleted == false).Select(b=>b.PaxSlab_Id);
                        var selectedPaxSlabList = QuotationQRFPriceInfo.PaxSlabDetails.QRFPaxSlabs.Where(a => a.From < itemPassanger.count && a.To > itemPassanger.count && a.IsDeleted == false).Select(b => b.PaxSlab_Id).ToList();
                        if (selectedPaxSlabList != null && !selectedPaxSlabList.Any())
                        {
                            selectedPaxSlabList = QuotationQRFPriceInfo.PaxSlabDetails.QRFPaxSlabs.Where(a => a.From == 10 && a.IsDeleted == false).Select(b => b.PaxSlab_Id).ToList();
                        }
                        //QRFPaxSlabsList.Add(selectedPaxSlab);
                        //QRFPaxSlabsIdList.Add(selectedPaxSlab.PaxSlab_Id);
                        QRFPaxSlabsIdList = selectedPaxSlabList;
                    }
                }
            }

            var QRFPackagePriceList = _MongoContext.mQRFPackagePrice.AsQueryable().Where(a => a.QRFID == QuotationQRFPriceInfo.QRFID && a.QRFPrice_Id == QuotationQRFPriceInfo.QRFPrice_Id && a.Departure_Id == setEarliestDeparture.Departure_Id && QRFPaxSlabsIdList.Contains(a.PaxSlab_Id)).ToList();

            if (setQRFAgentRoom != null && setQRFAgentRoom.Any() && QRFPackagePriceList != null && QRFPackagePriceList.Any())
            {
                var RoomTypeList = setQRFAgentRoom.Select(a => a.RoomTypeName.ToLower()).ToList();
                var notExistInAgentRoom = QRFPackagePriceList.Where(a => !RoomTypeList.Contains(a.RoomName.ToLower())).Select(x => x.RoomName).ToList();
                var getAdditionalRoomType = notExistInAgentRoom != null && notExistInAgentRoom.Any() ? setAgentPassangerInfo.Where(a => notExistInAgentRoom.Contains(a.Type)).Select(x => new QRFAgentRoom { RoomTypeID = "", RoomCount = x.count, RoomTypeName = x.Type }).ToList() : null;

                if (getAdditionalRoomType != null && getAdditionalRoomType.Any())
                {
                    foreach (var itemRoom in getAdditionalRoomType)
                    {
                        setQRFAgentRoom.Add(itemRoom);
                    }
                }

                foreach (var item in QRFPackagePriceList)
                {
                    var itemRoom = setQRFAgentRoom.Where(a => a.RoomTypeName.ToLower() == item.RoomName.ToLower()).FirstOrDefault();
                    ProductLineItemQuotation plItem = new ProductLineItemQuotation();
                    plItem.ProductLineItemNumber = item.QRFPackagePriceId;

                    plItem.RoomTypeName = itemRoom.RoomTypeName;
                    plItem.NoOfAdults = setAgentPassangerInfo.Where(a => a.Type.ToLower() == "ADULT".ToLower()).Select(b => b.count).FirstOrDefault();
                    plItem.NoOfDays = Convert.ToString((QuotationQRFPriceInfo.AgentProductInfo.Duration ?? 0) + 1);
                    plItem.NoOfChildren = setAgentPassangerInfo.Where(a => a.Type.ToLower() == "INFANT".ToLower() || a.Type.ToLower() == "CHILDWITHBED".ToLower() || a.Type.ToLower() == "CHILDWITHOUTBED".ToLower()).Sum(j => j.count);
                    plItem.NoOfRooms = itemRoom.RoomCount ?? 0;
                    plItem.NoofNights = Convert.ToString(QuotationQRFPriceInfo.AgentProductInfo.Duration ?? 0);
                    plItem.StartDate = QuotationQRFPriceInfo.Departures.Select(x => x.Date).OrderBy(a => a).FirstOrDefault();
                    plItem.EndDate = QuotationQRFPriceInfo.Departures.Select(x => x.Date).OrderByDescending(a => a).FirstOrDefault();

                    var itemFromCurrencyId = QuotationQRFPriceInfo.ExchangeRateSnapshot.ExchangeRateDetail.Where(a => a.CURRENCY == QuotationQRFPriceInfo.ExchangeRateSnapshot.REFCUR).Select(x => x.Currency_Id).FirstOrDefault();
                    var exchangeRateItem = _genericRepository.getExchangeRate(itemFromCurrencyId, QuotationQRFPriceInfo.AgentProductInfo.BudgetCurrencyID, QuotationQRFPriceInfo.QRFID);

                    plItem.CurrencyConversionRate = exchangeRateItem != null && !string.IsNullOrEmpty(exchangeRateItem.Value) ? Convert.ToDecimal(exchangeRateItem.Value) : 0;

                    var Occupancy = (item.RoomName != "INFANT" && item.RoomName != "CHILDWITHBED" && item.RoomName != "CHILDWITHOUTBED") ? (int)Enum.Parse(typeof(OccupancyTypeEnum), item.RoomName, true) : 1;
                    plItem.BillingAmount = Convert.ToDecimal(itemRoom.RoomCount * item.SellPrice * Occupancy);
                    plItem.ROE = Convert.ToString(plItem.CurrencyConversionRate);

                    plItemList.Add(plItem);
                }

                //ProductLineItemQuotation plAllItem = new ProductLineItemQuotation();
                //plAllItem.ProductLineItemNumber = plItemList[0].ProductLineItemNumber;

                //plAllItem.RoomTypeName = plItemList[0].RoomTypeName;
                //plAllItem.NoOfAdults = plItemList[0].NoOfAdults;
                //plAllItem.NoOfDays = plItemList[0].NoOfDays;
                //plAllItem.NoOfChildren = plItemList[0].NoOfChildren;
                //plAllItem.NoOfRooms = plItemList.Sum(a => a.NoOfRooms);
                //plAllItem.NoofNights = plItemList[0].NoofNights;
                //plAllItem.StartDate = plItemList[0].StartDate;
                //plAllItem.EndDate = plItemList[0].EndDate;

                //var fromCurrencyId = QuotationQRFPriceInfo.ExchangeRateSnapshot.ExchangeRateDetail.Where(a => a.CURRENCY == QuotationQRFPriceInfo.ExchangeRateSnapshot.REFCUR).Select(x => x.Currency_Id).FirstOrDefault();
                //var exchangeRate = _genericRepository.getExchangeRate(fromCurrencyId, QuotationQRFPriceInfo.AgentProductInfo.BudgetCurrencyID, QuotationQRFPriceInfo.QRFID);

                //plAllItem.CurrencyConversionRate = exchangeRate != null && !string.IsNullOrEmpty(exchangeRate.Value) ? Convert.ToDecimal(exchangeRate.Value) : 0;
                //plAllItem.BillingAmount = plItemList.Sum(a => a.BillingAmount);
                //plAllItem.ROE = QuotationQRFPriceInfo.ExchangeRateSnapshot.REFCUR;

                //plItemList = new List<ProductLineItemQuotation>();
                //plItemList.Add(plAllItem);
            }

            return plItemList;
        }

        public async Task<bool> UpdatemQuoteNmQRFPriceMapping(QuoteMappingReq request)
        {
            var quote = _quoteRepository.getQuoteInfo(request.QRFID).Result;
            if (quote.Mappings != null && quote.Mappings.Any())
            {
                var existingMappings = quote.Mappings.Where(a => a.PartnerEntityCode == request.PartnerEntityCode && a.Application.ToLower() == request.Source.ToLower()).FirstOrDefault();
                var ApplicationInfo = _MongoContext.mApplications.AsQueryable().Where(a => a.Application_Name.ToLower() == request.Source.ToLower()).FirstOrDefault();
                if (existingMappings != null && !string.IsNullOrEmpty(existingMappings.PartnerEntityCode))
                {
                    existingMappings.PartnerEntityType = _configuration.GetValue<string>("MappingDefault:PartnerEntityTypeQuote");
                    existingMappings.Action = _configuration.GetValue<string>("MappingDefault:ActionUpdate");
                    existingMappings.Status = string.Empty;
                    existingMappings.EditUser = request.CreatedBy;
                    existingMappings.EditDate = DateTime.Now;
                }
                else
                {
                    existingMappings = new QuoteMappings();
                    existingMappings.Application = ApplicationInfo.Application_Name;
                    existingMappings.Application_Id = ApplicationInfo.Application_Id;
                    existingMappings.PartnerEntityName = _configuration.GetValue<string>("MappingDefault:PartnerEntityTypeQuote");
                    existingMappings.PartnerEntityCode = request.PartnerEntityCode;
                    existingMappings.PartnerEntityType = _configuration.GetValue<string>("MappingDefault:PartnerEntityTypeQuote");
                    existingMappings.Action = _configuration.GetValue<string>("MappingDefault:ActionCreate");
                    existingMappings.Status = string.Empty;
                    existingMappings.CreateDate = DateTime.Now;
                    existingMappings.CreateUser = request.CreatedBy;
                    quote.Mappings.Add(existingMappings);
                }

                var resultFlag = await _MongoContext.mQuote.FindOneAndUpdateAsync(
                                            Builders<mQuote>.Filter.Eq("QRFID", request.QRFID),
                                            Builders<mQuote>.Update.
                                            Set("Mappings", quote.Mappings).
                                            Set("EditUser", request.CreatedBy).
                                            Set("EditDate", DateTime.Now)
                                            );

                var resultSubFlag = await _MongoContext.mQRFPrice.FindOneAndUpdateAsync(
                                            Builders<mQRFPrice>.Filter.Eq("QRFPrice_Id", request.QRFPrice_Id),
                                            Builders<mQRFPrice>.Update.
                                            Set("Mappings", quote.Mappings)
                                            );

                return true;
            }


            return false;
        }

        public async Task<ResponseStatus> CreateUpdateBooking(string BookingNo, string VoyagerUser)
        {
            var response = new ResponseStatus();

            try
            {
                IntegrationBookingReq requestInfo = new IntegrationBookingReq();

                requestInfo.BookingInfo = _quoteRepository.getBookingInfo(BookingNo).Result;

                var QuoteInfo = requestInfo.QuotationQRFPriceInfo = _quoteRepository.getQuotePriceInfo(requestInfo.BookingInfo.QRFID).Result;

                if (requestInfo.BookingInfo != null)
                {
                    var MappingBookingInfo = requestInfo.BookingInfo.Mappings != null ? requestInfo.BookingInfo.Mappings.Where(a => a.PartnerEntityType.ToLower() == "BOOKING".ToLower() && string.IsNullOrEmpty(a.Status)).FirstOrDefault(): null;
                    var MappingInfo = QuoteInfo.Mappings.Where(a => a.PartnerEntityType.ToLower() == "OPPORTUNITY".ToLower() && string.IsNullOrEmpty(a.Status)).FirstOrDefault();

                    if (MappingInfo != null && !string.IsNullOrEmpty(MappingInfo.PartnerEntityCode))
                    {
                        Integration_Search_Data IntegrationSearchData = _commonRepository.GetIntegrationCredentialsByUser(MappingInfo.Application, VoyagerUser).Result;

                        if (IntegrationSearchData != null)
                        {
                            requestInfo.CredentialInfo.Key = IntegrationSearchData.Keys;
                            requestInfo.CredentialInfo.Source = IntegrationSearchData.Application_Name;
                            requestInfo.CredentialInfo.User = IntegrationSearchData.UserKey;

                            requestInfo.CRM_OpportunityId = MappingInfo.PartnerEntityCode;

                            var CompanyInfo = GetCompanyInfo(requestInfo.BookingInfo.AgentInfo.Id).Result;
                            requestInfo.CustomerId = GetPartnerAccountInfo(CompanyInfo, requestInfo.CredentialInfo.Source);
                            var userInfo = _MongoContext.mUsers.AsQueryable().Where(a => a.VoyagerUser_Id == IntegrationSearchData.UserId).FirstOrDefault();

                            var systemInfo = _MongoContext.mSystem.AsQueryable().Where(a => a.CoreCompany_Id == userInfo.Company_Id).FirstOrDefault();
                            requestInfo.GroupOfCompanies = systemInfo.Mappings.Where(a => a.PartnerEntityName.ToLower() == "Group of Companies".ToLower()).Select(b => b.PartnerEntityCode).FirstOrDefault();
                            requestInfo.GroupCompany = systemInfo.Mappings.Where(a => a.PartnerEntityName.ToLower() == "Group Company".ToLower()).Select(b => b.PartnerEntityCode).FirstOrDefault();
                            requestInfo.CompanyName = systemInfo.Mappings.Where(a => a.PartnerEntityName.ToLower() == "Company Name".ToLower()).Select(b => b.PartnerEntityCode).FirstOrDefault();
                            requestInfo.BookingEngine = systemInfo.Mappings.Where(a => a.PartnerEntityName.ToLower() == "Booking Engine".ToLower()).Select(b => b.PartnerEntityCode).FirstOrDefault();

                            var agentInfo = GetCompanyInfo(QuoteInfo.AgentInfo.AgentID).Result;

                            var SystemMappings = !string.IsNullOrEmpty(agentInfo.SystemCompany_Id) ? _MongoContext.mCompanies.AsQueryable().Where(a => a.Company_Id == agentInfo.SystemCompany_Id).Select(b => b.Mappings).FirstOrDefault() : null;
                            requestInfo.BU = SystemMappings != null && SystemMappings.Any() ? SystemMappings.Where(a => a.PartnerEntityName.ToLower() == "BU".ToLower()).Select(b => b.PartnerEntityCode).FirstOrDefault() : string.Empty;

                            var contactList = !string.IsNullOrEmpty(requestInfo.CustomerId) ? agentInfo.ContactDetails.Where(a => a.Mappings != null && a.Mappings.Any(b => !string.IsNullOrEmpty(b.PartnerEntityType) && b.PartnerEntityType.ToLower() == "Contact".ToLower())).Select(x => x.Mappings).FirstOrDefault() : null;
                            requestInfo.ContactId = contactList != null && contactList.Any() ? contactList.Where(b => b.PartnerEntityType.ToLower() == "Contact".ToLower()).Select(x => x.PartnerEntityCode).FirstOrDefault() : string.Empty;

                            requestInfo.OwnerId = userInfo.UserName;
                            requestInfo.CompanyMarket = "DUBAI";

                            requestInfo.PartnerEntityCode = MappingBookingInfo != null && !string.IsNullOrEmpty(MappingBookingInfo.PartnerEntityCode) ? MappingBookingInfo.PartnerEntityCode : string.Empty;

                            requestInfo.ClientType = "B2B";
                            requestInfo.BookingSource = "Offline";
                            requestInfo.BookingType = "Offline";
                            requestInfo.HolidayType = "Customized";
                            requestInfo.StatusCode = requestInfo.BookingInfo.STATUS;

                            requestInfo.ProductType = "HOLIDAY";
                            requestInfo.TypeOfProduct = "HOLIDAY";
                            requestInfo.SBU = "OBT";//OutBound
                            requestInfo.POS = "SO";//Sales Office

                            requestInfo.ProductLineItemList = GetBookingCalculatedList(QuoteInfo, requestInfo.BookingInfo.BookingNumber);
                            requestInfo.ProductLineItemCityCountryList = GetCityCountryForBookings(requestInfo.BookingInfo);

                            response = _msDynamicsProviders.CreateUpdateBooking(requestInfo).Result;

                            if (response !=null && !string.IsNullOrEmpty(response.Status) && response.Status == "Success")
                            {
                                Task.Run(() => CreateUpdateOpportnity(requestInfo.BookingInfo.QRFID, userInfo.VoyagerUser_Id).Result);
                            }
                        }
                        else
                        {
                            response.Status = "Failure";
                            response.ErrorMessage = "User doesn't contains App Key Info.";
                        }
                    }
                    else
                    {
                        response.Status = "Failure";
                        response.ErrorMessage = "Opportunity Info doesn't exist against the Booking Info.";
                    }
                }
                else
                {
                    response.Status = "Failure";
                    response.ErrorMessage = "Booking Info doesn't exist.";
                }


            }
            catch (Exception ex)
            {
                response.Status = "Failure";
                response.ErrorMessage = "Failed while calling MSDynamics api for Booking POST/PATCH.";
                response.StatusMessage = ex.Message;
            }

            return response;
        }

        public List<ProductLineItemBooking> GetBookingCalculatedList(mQRFPrice QuotationQRFPriceInfo, string BookingNo)
        {
            List<ProductLineItemBooking> plItemList = new List<ProductLineItemBooking>();

            //get all the PassangerInfo with out 0 count.
            var setAgentPassangerInfo = QuotationQRFPriceInfo.AgentPassengerInfo.Where(b => b.count != 0).Select(a => new QRFAgentPassengerInfo { Type = a.Type, count = a.count, Age = a.Age }).ToList();
            //get the QRFAgentRoom based on no. of room type selected.
            var setQRFAgentRoom = QuotationQRFPriceInfo.QRFAgentRoom.Where(a => a.RoomCount != 0).Select(x => new QRFAgentRoom { RoomTypeID = x.RoomTypeID, RoomCount = x.RoomCount, RoomTypeName = x.RoomTypeName }).ToList();
            //get the earliest DepartureInfo.
            var setEarliestDeparture = QuotationQRFPriceInfo.Departures.Where(x => x.IsDeleted == false).OrderBy(a => a.Date).FirstOrDefault();

            //get PaxSlab based on range and AgentPassangerInfo
            //List<QRFPaxSlabs> QRFPaxSlabsList = new List<QRFPaxSlabs>();
            List<long> QRFPaxSlabsIdList = new List<long>();
            if (setAgentPassangerInfo != null && setAgentPassangerInfo.Any())
            {
                foreach (var itemPassanger in setAgentPassangerInfo)
                {
                    if (itemPassanger.Type == "ADULT")
                    {
                        //var selectedPaxSlabList = QuotationQRFPriceInfo.PaxSlabDetails.QRFPaxSlabs.Where(a => ((a.From > itemPassanger.count) || (a.From < itemPassanger.count && a.To > itemPassanger.count)) && a.IsDeleted == false).Select(b=>b.PaxSlab_Id);
                        var selectedPaxSlabList = QuotationQRFPriceInfo.PaxSlabDetails.QRFPaxSlabs.Where(a => a.From < itemPassanger.count && a.To > itemPassanger.count && a.IsDeleted == false).Select(b => b.PaxSlab_Id).ToList();
                        if (selectedPaxSlabList != null && !selectedPaxSlabList.Any())
                        {
                            selectedPaxSlabList = QuotationQRFPriceInfo.PaxSlabDetails.QRFPaxSlabs.Where(a => a.From == 10 && a.IsDeleted == false).Select(b => b.PaxSlab_Id).ToList();
                        }
                        //QRFPaxSlabsList.Add(selectedPaxSlab);
                        //QRFPaxSlabsIdList.Add(selectedPaxSlab.PaxSlab_Id);
                        QRFPaxSlabsIdList = selectedPaxSlabList;
                    }
                }
            }

            var QRFPackagePriceList = _MongoContext.mQRFPackagePrice.AsQueryable().Where(a => a.QRFID == QuotationQRFPriceInfo.QRFID && a.QRFPrice_Id == QuotationQRFPriceInfo.QRFPrice_Id && a.Departure_Id == setEarliestDeparture.Departure_Id && QRFPaxSlabsIdList.Contains(a.PaxSlab_Id)).ToList();

            if (setQRFAgentRoom != null && setQRFAgentRoom.Any() && QRFPackagePriceList != null && QRFPackagePriceList.Any())
            {
                var RoomTypeList = setQRFAgentRoom.Select(a => a.RoomTypeName.ToLower()).ToList();
                var notExistInAgentRoom = QRFPackagePriceList.Where(a => !RoomTypeList.Contains(a.RoomName.ToLower())).Select(x => x.RoomName).ToList();
                var getAdditionalRoomType = notExistInAgentRoom != null && notExistInAgentRoom.Any() ? setAgentPassangerInfo.Where(a => notExistInAgentRoom.Contains(a.Type)).Select(x => new QRFAgentRoom { RoomTypeID = "", RoomCount = x.count, RoomTypeName = x.Type }).ToList() : null;

                if (getAdditionalRoomType != null && getAdditionalRoomType.Any())
                {
                    foreach (var itemRoom in getAdditionalRoomType)
                    {
                        setQRFAgentRoom.Add(itemRoom);
                    }
                }

                foreach (var item in QRFPackagePriceList)
                {
                    var itemRoom = setQRFAgentRoom.Where(a => a.RoomTypeName.ToLower() == item.RoomName.ToLower()).FirstOrDefault();
                    ProductLineItemBooking plItem = new ProductLineItemBooking();
                    plItem.ProductLineItemNumber = item.QRFPackagePriceId + "-" + BookingNo;

                    plItem.RoomTypeName = itemRoom.RoomTypeName;
                    plItem.NoOfAdults = setAgentPassangerInfo.Where(a => a.Type.ToLower() == "ADULT".ToLower()).Select(b => b.count).FirstOrDefault();
                    plItem.NoOfDays = Convert.ToString((QuotationQRFPriceInfo.AgentProductInfo.Duration ?? 0) + 1);
                    plItem.NoOfChildren = setAgentPassangerInfo.Where(a => a.Type.ToLower() == "INFANT".ToLower() || a.Type.ToLower() == "CHILDWITHBED".ToLower() || a.Type.ToLower() == "CHILDWITHOUTBED".ToLower()).Sum(j => j.count);
                    plItem.NoOfRooms = itemRoom.RoomCount ?? 0;
                    plItem.NoofNights = Convert.ToString(QuotationQRFPriceInfo.AgentProductInfo.Duration ?? 0);
                    plItem.StartDate = QuotationQRFPriceInfo.Departures.Select(x => x.Date).OrderBy(a => a).FirstOrDefault();
                    plItem.EndDate = QuotationQRFPriceInfo.Departures.Select(x => x.Date).OrderByDescending(a => a).FirstOrDefault();

                    var itemFromCurrencyId = QuotationQRFPriceInfo.ExchangeRateSnapshot.ExchangeRateDetail.Where(a => a.CURRENCY == QuotationQRFPriceInfo.ExchangeRateSnapshot.REFCUR).Select(x => x.Currency_Id).FirstOrDefault();
                    var exchangeRateItem = _genericRepository.getExchangeRate(itemFromCurrencyId, QuotationQRFPriceInfo.AgentProductInfo.BudgetCurrencyID, QuotationQRFPriceInfo.QRFID);

                    plItem.CurrencyConversionRate = exchangeRateItem != null && !string.IsNullOrEmpty(exchangeRateItem.Value) ? Convert.ToDecimal(exchangeRateItem.Value) : 0;

                    var Occupancy = (item.RoomName != "INFANT" && item.RoomName != "CHILDWITHBED" && item.RoomName != "CHILDWITHOUTBED") ? (int)Enum.Parse(typeof(OccupancyTypeEnum), item.RoomName, true) : 1;
                    plItem.BillingAmount = Convert.ToDecimal(itemRoom.RoomCount * item.SellPrice * Occupancy);
                    plItem.ROE = Convert.ToString(plItem.CurrencyConversionRate);

                    plItemList.Add(plItem);
                }
            }

            /*
            var personTypeList = new List<string>{ "ADULT", "child + Bed", "Child - Bed", "INFANT" };

            //get all the PassangerInfo with out 0 count.
            var setAgentPassangerInfo = BookingInfo.BookingPax.Where(b => b.PERSONS != 0 && personTypeList.Contains(b.PERSTYPE) && string.IsNullOrEmpty(b.Status)).Select(a => new QRFAgentPassengerInfo { Type = a.PERSTYPE, count = a.PERSONS }).ToList();
            //get the QRFAgentRoom based on no. of room type selected.
            var setQRFAgentRoom = BookingInfo.BookingRooms.Where(a => string.IsNullOrEmpty(a.Status) && a.ROOMNO != 0).Select(x => new QRFAgentRoom { RoomTypeID = x.BookingRooms_ID, RoomCount = x.ROOMNO, RoomTypeName = x.SUBPROD }).ToList();
            
            //get the earliest DepartureInfo.
            //var setEarliestDeparture = BookingInfo.Departures.Where(x => x.IsDeleted == false).OrderBy(a => a.Date).FirstOrDefault();

            //get PaxSlab based on range and AgentPassangerInfo
            List<long> QRFPaxSlabsIdList = new List<long>();
            if (setAgentPassangerInfo != null && setAgentPassangerInfo.Any())
            {
                foreach (var itemPassanger in setAgentPassangerInfo)
                {
                    if (itemPassanger.Type == "ADULT")
                    {
                        var selectedPaxSlabList = QuotationQRFPriceInfo.PaxSlabDetails.QRFPaxSlabs.Where(a => a.From < itemPassanger.count && a.To > itemPassanger.count && a.IsDeleted == false).Select(b => b.PaxSlab_Id).ToList();
                        if (selectedPaxSlabList != null && !selectedPaxSlabList.Any())
                        {
                            selectedPaxSlabList = QuotationQRFPriceInfo.PaxSlabDetails.QRFPaxSlabs.Where(a => a.From == 10 && a.IsDeleted == false).Select(b => b.PaxSlab_Id).ToList();
                        }
                        QRFPaxSlabsIdList = selectedPaxSlabList;
                    }
                }
            }

            var QRFPackagePriceList = _MongoContext.mQRFPackagePrice.AsQueryable().Where(a => a.QRFID == QuotationQRFPriceInfo.QRFID && a.QRFPrice_Id == QuotationQRFPriceInfo.QRFPrice_Id && a.Departure_Id == setEarliestDeparture.Departure_Id && QRFPaxSlabsIdList.Contains(a.PaxSlab_Id)).ToList();

            if (setQRFAgentRoom != null && setQRFAgentRoom.Any() && QRFPackagePriceList != null && QRFPackagePriceList.Any())
            {
                var RoomTypeList = setQRFAgentRoom.Select(a => a.RoomTypeName.ToLower()).ToList();
                var notExistInAgentRoom = QRFPackagePriceList.Where(a => !RoomTypeList.Contains(a.RoomName.ToLower())).Select(x => x.RoomName).ToList();
                var getAdditionalRoomType = notExistInAgentRoom != null && notExistInAgentRoom.Any() ? setAgentPassangerInfo.Where(a => notExistInAgentRoom.Contains(a.Type)).Select(x => new QRFAgentRoom { RoomTypeID = "", RoomCount = x.count, RoomTypeName = x.Type }).ToList() : null;

                if (getAdditionalRoomType != null && getAdditionalRoomType.Any())
                {
                    foreach (var itemRoom in getAdditionalRoomType)
                    {
                        setQRFAgentRoom.Add(itemRoom);
                    }
                }

                foreach (var item in QRFPackagePriceList)
                {
                    var itemRoom = setQRFAgentRoom.Where(a => a.RoomTypeName.ToLower() == item.RoomName.ToLower()).FirstOrDefault();
                    ProductLineItemQuotation plItem = new ProductLineItemQuotation();
                    plItem.ProductLineItemNumber = item.QRFPackagePriceId;

                    plItem.RoomTypeName = itemRoom.RoomTypeName;
                    plItem.NoOfAdults = setAgentPassangerInfo.Where(a => a.Type.ToLower() == "ADULT".ToLower()).Select(b => b.count).FirstOrDefault();
                    plItem.NoOfDays = Convert.ToString((QuotationQRFPriceInfo.AgentProductInfo.Duration ?? 0) + 1);
                    plItem.NoOfChildren = setAgentPassangerInfo.Where(a => a.Type.ToLower() == "INFANT".ToLower() || a.Type.ToLower() == "CHILDWITHBED".ToLower() || a.Type.ToLower() == "CHILDWITHOUTBED".ToLower()).Sum(j => j.count);
                    plItem.NoOfRooms = itemRoom.RoomCount ?? 0;
                    plItem.NoofNights = Convert.ToString(QuotationQRFPriceInfo.AgentProductInfo.Duration ?? 0);
                    plItem.StartDate = QuotationQRFPriceInfo.Departures.Select(x => x.Date).OrderBy(a => a).FirstOrDefault();
                    plItem.EndDate = QuotationQRFPriceInfo.Departures.Select(x => x.Date).OrderByDescending(a => a).FirstOrDefault();

                    var itemFromCurrencyId = QuotationQRFPriceInfo.ExchangeRateSnapshot.ExchangeRateDetail.Where(a => a.CURRENCY == QuotationQRFPriceInfo.ExchangeRateSnapshot.REFCUR).Select(x => x.Currency_Id).FirstOrDefault();
                    var exchangeRateItem = _genericRepository.getExchangeRate(itemFromCurrencyId, QuotationQRFPriceInfo.AgentProductInfo.BudgetCurrencyID, QuotationQRFPriceInfo.QRFID);

                    plItem.CurrencyConversionRate = exchangeRateItem != null && !string.IsNullOrEmpty(exchangeRateItem.Value) ? Convert.ToDecimal(exchangeRateItem.Value) : 0;

                    var Occupancy = (item.RoomName != "INFANT" && item.RoomName != "CHILDWITHBED" && item.RoomName != "CHILDWITHOUTBED") ? (int)Enum.Parse(typeof(OccupancyTypeEnum), item.RoomName, true) : 1;
                    plItem.BillingAmount = Convert.ToDecimal(itemRoom.RoomCount * item.SellPrice * Occupancy);
                    plItem.ROE = QuotationQRFPriceInfo.ExchangeRateSnapshot.REFCUR;

                    plItemList.Add(plItem);
                }

                //ProductLineItemQuotation plAllItem = new ProductLineItemQuotation();
                //plAllItem.ProductLineItemNumber = plItemList[0].ProductLineItemNumber;

                //plAllItem.RoomTypeName = plItemList[0].RoomTypeName;
                //plAllItem.NoOfAdults = plItemList[0].NoOfAdults;
                //plAllItem.NoOfDays = plItemList[0].NoOfDays;
                //plAllItem.NoOfChildren = plItemList[0].NoOfChildren;
                //plAllItem.NoOfRooms = plItemList.Sum(a => a.NoOfRooms);
                //plAllItem.NoofNights = plItemList[0].NoofNights;
                //plAllItem.StartDate = plItemList[0].StartDate;
                //plAllItem.EndDate = plItemList[0].EndDate;

                //var fromCurrencyId = QuotationQRFPriceInfo.ExchangeRateSnapshot.ExchangeRateDetail.Where(a => a.CURRENCY == QuotationQRFPriceInfo.ExchangeRateSnapshot.REFCUR).Select(x => x.Currency_Id).FirstOrDefault();
                //var exchangeRate = _genericRepository.getExchangeRate(fromCurrencyId, QuotationQRFPriceInfo.AgentProductInfo.BudgetCurrencyID, QuotationQRFPriceInfo.QRFID);

                //plAllItem.CurrencyConversionRate = exchangeRate != null && !string.IsNullOrEmpty(exchangeRate.Value) ? Convert.ToDecimal(exchangeRate.Value) : 0;
                //plAllItem.BillingAmount = plItemList.Sum(a => a.BillingAmount);
                //plAllItem.ROE = QuotationQRFPriceInfo.ExchangeRateSnapshot.REFCUR;

                //plItemList = new List<ProductLineItemQuotation>();
                //plItemList.Add(plAllItem);
            }*/

            return plItemList;
        }

        public List<ProductLineItemCityCountryBooking> GetCityCountryForBookings(Bookings BookingInfo)
        {
            var listInfo = BookingInfo.Positions.Select(a => new { City = a.City_Id, Country = a.Country_Id }).Distinct();
            List<ProductLineItemCityCountryBooking> BookingCityCountryInfoList = new List<ProductLineItemCityCountryBooking>();

            if (listInfo != null && listInfo.Any())
            {
                var cityList = listInfo.Select(a => a.City);
                var countryList = listInfo.Select(a => a.Country);

                var finalCityCountryList = cityList.Concat(countryList);
                var resortDetails = _MongoContext.mResort.AsQueryable().Where(a => finalCityCountryList.Contains(a.Voyager_Resort_Id));

                foreach (var item in listInfo)
                {
                    ProductLineItemCityCountryBooking cityCountryInfo = new ProductLineItemCityCountryBooking();

                    cityCountryInfo.City = resortDetails.Where(a => a.Voyager_Resort_Id == item.City).Select(x => x.ResortCode).FirstOrDefault();
                    cityCountryInfo.Country = resortDetails.Where(a => a.Voyager_Resort_Id == item.Country).Select(x => x.ResortCode).FirstOrDefault();
                    BookingCityCountryInfoList.Add(cityCountryInfo);
                }

            }
            return BookingCityCountryInfoList;
        }

        public async Task<bool> UpdatemBookingMapping(BookingMappingReq request)
        {

            var quote = _quoteRepository.getQuoteInfo(request.QRFID).Result;
            if (quote.Mappings != null && quote.Mappings.Any())
            {
                var existingMappings = quote.Mappings.Where(a => a.PartnerEntityCode == request.PartnerEntityCode && a.Application.ToLower() == request.Source.ToLower()).FirstOrDefault();
                var ApplicationInfo = _MongoContext.mApplications.AsQueryable().Where(a => a.Application_Name.ToLower() == request.Source.ToLower()).FirstOrDefault();
                if (existingMappings != null && !string.IsNullOrEmpty(existingMappings.PartnerEntityCode))
                {
                    existingMappings.PartnerEntityType = _configuration.GetValue<string>("MappingDefault:PartnerEntityTypeBooking");
                    existingMappings.Action = _configuration.GetValue<string>("MappingDefault:ActionUpdate");
                    existingMappings.Status = string.Empty;
                    existingMappings.EditUser = request.CreatedBy;
                    existingMappings.EditDate = DateTime.Now;
                }
                else
                {
                    existingMappings = new QuoteMappings();
                    existingMappings.Application = ApplicationInfo.Application_Name;
                    existingMappings.Application_Id = ApplicationInfo.Application_Id;
                    existingMappings.PartnerEntityName = _configuration.GetValue<string>("MappingDefault:PartnerEntityTypeBooking");
                    existingMappings.PartnerEntityCode = request.PartnerEntityCode;
                    existingMappings.PartnerEntityType = _configuration.GetValue<string>("MappingDefault:PartnerEntityTypeBooking");
                    existingMappings.Action = _configuration.GetValue<string>("MappingDefault:ActionCreate");
                    existingMappings.Status = string.Empty;
                    existingMappings.CreateDate = DateTime.Now;
                    existingMappings.CreateUser = request.CreatedBy;
                    quote.Mappings.Add(existingMappings);
                }

                var resultFlag = await _MongoContext.mQuote.FindOneAndUpdateAsync(
                                            Builders<mQuote>.Filter.Eq("QRFID", request.QRFID),
                                            Builders<mQuote>.Update.
                                            Set("Mappings", quote.Mappings).
                                            Set("EditUser", request.CreatedBy).
                                            Set("EditDate", DateTime.Now)
                                            );

                var resultSubFlag = await _MongoContext.mQRFPrice.FindOneAndUpdateAsync(
                                            Builders<mQRFPrice>.Filter.Eq("QRFPrice_Id", request.QRFPrice_Id),
                                            Builders<mQRFPrice>.Update.
                                            Set("Mappings", quote.Mappings)
                                            );

                var BookingMappingItem = new BookingMapping();
                var BookingMappings = new List<BookingMapping>();
                BookingMappingItem.Application = existingMappings.Application;
                BookingMappingItem.Application_Id = existingMappings.Application_Id;
                BookingMappingItem.PartnerEntityName = existingMappings.PartnerEntityName;
                BookingMappingItem.PartnerEntityCode = existingMappings.PartnerEntityCode;
                BookingMappingItem.PartnerEntityType = existingMappings.PartnerEntityType;
                BookingMappingItem.Action = existingMappings.Action;
                BookingMappingItem.Status = existingMappings.Status;
                BookingMappingItem.CreateDate = existingMappings.CreateDate;
                BookingMappingItem.CreateUser = existingMappings.CreateUser;
                BookingMappingItem.EditUser = existingMappings.EditUser;
                BookingMappingItem.EditDate = existingMappings.EditDate;
                BookingMappings.Add(BookingMappingItem);

                var resultBookingFlag = await _MongoContext.Bookings.FindOneAndUpdateAsync(
                                            Builders<Bookings>.Filter.Eq("BookingNumber", request.BookingNo),
                                            Builders<Bookings>.Update.
                                            Set("Mappings", BookingMappings)
                                            );

                return true;
            }

            return false;
        }

        public async Task<bool> AddOpportunityMapping(QuoteMappingReq request)
        {
            var quote = _quoteRepository.getQuoteInfo(request.QRFID).Result;
            var ApplicationInfo = _MongoContext.mApplications.AsQueryable().Where(a => a.Application_Name.ToLower() == request.Source.ToLower()).FirstOrDefault();
            if (ApplicationInfo != null && !string.IsNullOrEmpty(ApplicationInfo.Application_Id))
            {
                try
                {
                    quote.Mappings = new List<QuoteMappings>();

                    var existingMappings = new QuoteMappings();
                    existingMappings.Application = ApplicationInfo.Application_Name;
                    existingMappings.Application_Id = ApplicationInfo.Application_Id;
                    existingMappings.PartnerEntityName = _configuration.GetValue<string>("MappingDefault:PartnerEntityTypeOpportunity");
                    existingMappings.PartnerEntityCode = request.PartnerEntityCode;
                    existingMappings.PartnerEntityType = _configuration.GetValue<string>("MappingDefault:PartnerEntityTypeOpportunity");
                    existingMappings.Action = _configuration.GetValue<string>("MappingDefault:ActionCreate");
                    existingMappings.Status = string.Empty;
                    existingMappings.CreateDate = DateTime.Now;
                    existingMappings.CreateUser = request.CreatedBy;
                    quote.Mappings.Add(existingMappings);

                    var resultFlag = await _MongoContext.mQuote.FindOneAndUpdateAsync(
                                                    Builders<mQuote>.Filter.Eq("QRFID", request.QRFID),
                                                    Builders<mQuote>.Update.
                                                    Set("Mappings", quote.Mappings).
                                                    Set("EditUser", request.CreatedBy).
                                                    Set("EditDate", DateTime.Now)
                                                    );

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            return false;
        }

        public async Task<ResponseStatus> RejectOpportunityInfo(string QRFId, string VoyagerUserId)
        {
            var response = new ResponseStatus();

            try
            {
                IntegrationOpportunityReq requestInfo = new IntegrationOpportunityReq();
                requestInfo.OpportunityInfo = _quoteRepository.getQuoteInfo(QRFId).Result;

                var QrfPriceInfo = _quoteRepository.getQuotePriceInfo(QRFId).Result;

                if (QrfPriceInfo != null && !string.IsNullOrEmpty(QrfPriceInfo.QRFPrice_Id))
                {
                    requestInfo.OpportunityQRFPriceInfo = QrfPriceInfo;
                }

                if (requestInfo.OpportunityInfo.Mappings != null && requestInfo.OpportunityInfo.Mappings.Any())
                {
                    var MappingInfo = requestInfo.OpportunityInfo.Mappings.Where(a => a.PartnerEntityType.ToLower() == "OPPORTUNITY".ToLower() && string.IsNullOrEmpty(a.Status)).FirstOrDefault();

                    if (MappingInfo != null && !string.IsNullOrEmpty(MappingInfo.PartnerEntityCode))
                    {
                        Integration_Search_Data IntegrationSearchData = _commonRepository.GetIntegrationCredentials(MappingInfo.Application, VoyagerUserId).Result;

                        requestInfo.CredentialInfo.Key = IntegrationSearchData.Keys;
                        requestInfo.CredentialInfo.Source = IntegrationSearchData.Application_Name;
                        requestInfo.CredentialInfo.User = IntegrationSearchData.UserKey;

                        requestInfo.StatusCode = "Lost";//for Opportunity as well for Quotation

                        response = _msDynamicsProviders.RejectOpportunityInfo(requestInfo).Result;
                    }
                    else
                    {
                        response.Status = "Failure";
                        response.ErrorMessage = "No mapping data to call MSDynamics api for Reject Booking Opportunity Patch.";
                    }
                }
                else
                {
                    response.Status = "Failure";
                    response.ErrorMessage = "No mapping data to call MSDynamics api for Reject Booking Opportunity Patch.";
                }
            }
            catch (Exception ex)
            {
                response.Status = "Failure";
                response.ErrorMessage = "Failed while calling MSDynamics api for Reject Booking Opportunity Patch. \n" + ex.Message;
            }

            return response;
        }

        #endregion

    }
}
