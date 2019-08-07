using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VGER_WAPI.Helpers;
using VGER_WAPI.Models;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Repositories
{
    public class AgentApprovalRepository : IAgentApprovalRepository
    {
        #region Private Variable Declaration
        private readonly MongoContext _MongoContext = null;
        private readonly IGenericRepository _genericRepository;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly IQRFSummaryRepository _qRFSummaryRepository;
        private readonly IEmailRepository _emailRepository;
        private readonly IQuoteRepository _quoteRepository;
        #endregion

        public AgentApprovalRepository(IConfiguration configuration, IOptions<MongoSettings> settings, IHostingEnvironment env, IGenericRepository genericRepository,
            ICostsheetRepository costsheetRepository, IMasterRepository masterRepository, IUserRepository userRepository, IQRFSummaryRepository qRFSummaryRepository,
            IEmailRepository emailRepository, IQuoteRepository quoteRepository)
        {
            _MongoContext = new MongoContext(settings);
            _genericRepository = genericRepository;
            _qRFSummaryRepository = qRFSummaryRepository;
            _env = env;
            _configuration = configuration;
            _userRepository = userRepository;
            _emailRepository = emailRepository;
            _quoteRepository = quoteRepository;
        }

        #region Send to client Mail
        public async Task<SendToClientSetRes> SendToClientMail(SendToClientSetReq request)
        {
            SendToClientSetRes response = new SendToClientSetRes();
            try
            {
                var objEmailGetReq = new EmailGetReq()
                {
                    UserName = request.UserName,
                    UserEmail = request.UserEmail,
                    QrfId = request.QRFID,
                    DocumentType = DocType.SENDTOCLIENT,
                    EmailHtml = request.SendToClientHtml,
                    ToCC = request.ToCC
                };
                var responseStatusMail = await _emailRepository.GenerateEmail(objEmailGetReq);
                if (responseStatusMail == null || responseStatusMail.ResponseStatus == null || string.IsNullOrEmpty(responseStatusMail.ResponseStatus.Status))
                {
                    response.ResponseStatus = new ResponseStatus();
                    response.ResponseStatus.Status = "Error";
                    response.ResponseStatus.ErrorMessage = "Mail not sent.";
                }
                else if (responseStatusMail?.ResponseStatus?.Status.ToLower() == "success")
                {
                    response.ResponseStatus.Status = "Success";
                    response.ResponseStatus.ErrorMessage = "Mail Sent Successfully.";

                    #region Add Followup 
                    request.UserEmail = request.UserEmail.ToLower().Trim();
                    var AgentContact = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == request.QRFID && a.IsCurrentVersion == true).FirstOrDefault()?.AgentInfo?.EmailAddress?.ToLower().Trim();
                    var CompanyList = _MongoContext.mCompanies.AsQueryable();
                    var FromUserContacts = CompanyList.Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a => a.MAIL.ToLower() == request.UserEmail)).FirstOrDefault()?.ContactDetails;
                    var FromUser = FromUserContacts?.Where(a => a.MAIL.ToLower() == request.UserEmail).FirstOrDefault();
                    var ToUserContacts = CompanyList.Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a => a.MAIL.ToLower() == AgentContact)).FirstOrDefault()?.ContactDetails;
                    var ToUser = ToUserContacts?.Where(a => a.MAIL.ToLower() == AgentContact).FirstOrDefault();

                    FollowUpSetReq followUprequest = new FollowUpSetReq();
                    followUprequest.QRFID = request.QRFID;

                    FollowUpTask task = new FollowUpTask();
                    task.Task = "Proposal Sent";
                    task.FollowUpType = "External";
                    task.FollowUpDateTime = DateTime.Now;

                    task.FromEmail = request.UserEmail;
                    if (FromUser != null)
                    {
                        task.FromName = FromUser.CommonTitle + " " + FromUser.FIRSTNAME + " " + FromUser.LastNAME;
                        task.FromContact_Id = FromUser.Contact_Id;
                    }

                    task.ToEmail = AgentContact;
                    if (ToUser != null)
                    {
                        task.ToName = ToUser.CommonTitle + " " + ToUser.FIRSTNAME + " " + ToUser.LastNAME;
                        task.ToContact_Id = ToUser.Contact_Id;
                    }

                    task.Status = "Replied";
                    task.Notes = "Proposal Sent";

                    var FollowUpTaskList = new List<FollowUpTask>();
                    FollowUpTaskList.Add(task);

                    followUprequest.FollowUp.Add(new FollowUp
                    {
                        FollowUp_Id = Guid.NewGuid().ToString(),
                        FollowUpTask = FollowUpTaskList,
                        CreateUser = request.UserEmail,
                        CreateDate = DateTime.Now
                    });
                    await _quoteRepository.SetFollowUpForQRF(followUprequest);
                    #endregion
                }
                else
                {
                    response.ResponseStatus = new ResponseStatus();
                    response.ResponseStatus = responseStatusMail.ResponseStatus;
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.ErrorMessage = ex.Message;
                response.ResponseStatus.Status = "Error";
            }
            return response;
        }

        public async Task<SendToClientSetRes> GetSendToClientDetails(SendToClientGetReq request)
        {
            SendToClientSetRes response = new SendToClientSetRes();
            response.QRFID = request.QRFID;

            try
            {
                var resQuote = await _MongoContext.mQuote.FindAsync(a => a.QRFID == request.QRFID).Result.FirstOrDefaultAsync();
                if (resQuote != null)
                {
                    var QRFPrice = _MongoContext.mQRFPrice.AsQueryable().Where(x => x.QRFID == request.QRFID && x.IsCurrentVersion == true && x.IsDeleted == false).OrderByDescending(y => y.VersionId).FirstOrDefault();
                    if (QRFPrice != null)
                    {
                        response.AgentName = QRFPrice.AgentInfo.AgentName;
                        response.TourName = QRFPrice.AgentProductInfo.TourName;
                        response.ContactPerson = QRFPrice.AgentInfo.ContactPerson;
                        response.SalesOfficer = QRFPrice.SalesOfficer;
                        response.EmailAddress = QRFPrice.AgentInfo.EmailAddress;
                        response.ResponseStatus.Status = "Success";
                        response.QRFPriceID = QRFPrice.QRFPrice_Id;
                        response.TravellingDate = QRFPrice.Departures != null && QRFPrice.Departures.Count > 0 ? string.Join(", ", QRFPrice.Departures.OrderBy(a => a.Date).Select(a => Convert.ToDateTime(a.Date).ToString("MMM yyyy")).Distinct().ToList()) : "";
                        response.Destination = QRFPrice.AgentProductInfo.Destination;
                        ContactDetailsResponse objContactDetailsRes = _userRepository.GetContactsByEmailId(new ContactDetailsRequest { Email = QRFPrice.SalesOfficer });
                        if (objContactDetailsRes != null && objContactDetailsRes.Contacts != null)
                        {
                            var lastname = !string.IsNullOrEmpty(objContactDetailsRes.Contacts.LastNAME) ? " " + objContactDetailsRes.Contacts.LastNAME : "";
                            response.FullName = objContactDetailsRes.Contacts.FIRSTNAME + lastname;
                        }

                        var salesOfficerCompanyData = _MongoContext.mCompanies.AsQueryable().Where(a => a.Company_Id == request.UserCompanyId && a.ContactDetails != null && a.ContactDetails.Any()).Select(x => x.ContactDetails).FirstOrDefault();
                        //response.CentralMailBox = salesOfficerCompanyData.Where(a => a.Default == 1 && a.STATUS == "").Select(x=>x.MAIL).FirstOrDefault();

                        response.CentralMailBoxList = salesOfficerCompanyData.Where(a => a.IsCentralEmail == true && string.IsNullOrEmpty(a.STATUS.Trim())).Select(x => x.MAIL).ToList();

                        var AgentData = _MongoContext.mCompanies.AsQueryable().Where(a => a.ContactDetails != null && a.ContactDetails.Any(b => b.Company_Id == QRFPrice.AgentInfo.AgentID)).Select(x => x.ContactDetails);
                        response.ToCC = AgentData.FirstOrDefault().Where(x=> x.MAIL != null && x.MAIL != "").Select(a => a.MAIL).ToList();
                        if (response.ToCC != null && response.ToCC.Any())
                        {
                            if (!string.IsNullOrEmpty(response.EmailAddress))
                            {
                                response.ToCC = response.ToCC.Where(a => a != response.EmailAddress).ToList();
                            }
                        }
                        response.FromMail = response.SalesOfficer;
                    }
                    else
                    {
                        response.ResponseStatus.ErrorMessage = "QRFPrice Details not exists.";
                        response.ResponseStatus.Status = "Error";
                    }
                }
                else
                {
                    response.ResponseStatus.ErrorMessage = "QRFID not exists.";
                    response.ResponseStatus.Status = "Error";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.ErrorMessage = ex.Message;
                response.ResponseStatus.Status = "Error";
            }
            return response;
        }

        public async Task<AcceptSendToClientSetRes> AcceptSendToClient(SendToClientGetReq request)
        {
            AcceptSendToClientSetRes response = new AcceptSendToClientSetRes();
            response.CostingGetRes = new CostingGetRes();
            response.QRFID = request.QRFID;
            response.QRFPriceID = request.QRFPriceID;

            try
            {
                if (!string.IsNullOrEmpty(request.MailStatus))
                {
                    var resQuote = _MongoContext.mQuote.AsQueryable().Where(a => a.QRFID == request.QRFID).FirstOrDefault();
                    if (resQuote != null)
                    {
                        if (request.MailStatus.ToLower() == "accepted")
                        {
                            if (!string.IsNullOrEmpty(resQuote.SalesPerson))
                            {
                                var resultQRFPrice = await _MongoContext.mQRFPrice.FindAsync(m => m.QRFID == request.QRFID && m.QRFPrice_Id == request.QRFPriceID && m.IsDeleted == false).Result.FirstOrDefaultAsync();
                                ContactDetailsResponse objContactDetailsRes = _userRepository.GetContactsByEmailId(new ContactDetailsRequest { Email = resultQRFPrice.SalesOfficer });
                                if (objContactDetailsRes != null && objContactDetailsRes.Contacts != null)
                                {
                                    var lastname = !string.IsNullOrEmpty(objContactDetailsRes.Contacts.LastNAME) ? " " + objContactDetailsRes.Contacts.LastNAME : "";
                                    response.SalesOfficerName = objContactDetailsRes.Contacts.FIRSTNAME + lastname;
                                    response.SalesOfficerPhone = !string.IsNullOrEmpty(objContactDetailsRes.Contacts.MOBILE) ? objContactDetailsRes.Contacts.MOBILE : objContactDetailsRes.Contacts.TEL;

                                    if (resultQRFPrice != null)
                                    {
                                        response.CostingGetRes = GetCostingDetails(request.QRFID, resultQRFPrice.QRFPrice_Id, request.Document_Id);
                                        response.CostingGetRes.EnquiryPipeline = resQuote.CurrentPipeline;
                                        var objEmail = new mDocumentStore();
                                        var lstEmail = new List<mDocumentStore>();

                                        if (!string.IsNullOrEmpty(request.Type) && request.Type == "agentaccept")
                                        {
                                            lstEmail = _MongoContext.mDocumentStore.AsQueryable().Where(a => a.QRFID == request.QRFID && a.QRFPriceId == request.QRFPriceID
                                                       && a.DocumentType == DocType.SENDTOCLIENT).ToList();

                                            objEmail = lstEmail.Where(a => string.IsNullOrEmpty(a.MailStatus)).OrderByDescending(a => a.SendDate).FirstOrDefault();
                                        }
                                        else
                                        {
                                            objEmail = _MongoContext.mDocumentStore.AsQueryable().Where(a => a.QRFID == request.QRFID && a.QRFPriceId == request.QRFPriceID
                                                        && a.Document_Id == request.Document_Id).FirstOrDefault();
                                        }

                                        if (objEmail != null)
                                        {
                                            response.MailStatus = objEmail.MailStatus;
                                            if (string.IsNullOrEmpty(objEmail.MailStatus))
                                            {
                                                response.MailStatus = request.MailStatus;
                                                var objmDocumentStore = new mDocumentStore();

                                                if (!string.IsNullOrEmpty(request.Type) && request.Type == "agentaccept")
                                                {
                                                    request.Document_Id = objEmail.Document_Id;
                                                }
                                                else
                                                {
                                                    request.UserEmailId = objEmail.From;
                                                }
                                                objmDocumentStore = await _MongoContext.mDocumentStore.FindOneAndUpdateAsync(m => m.Document_Id == request.Document_Id,
                                                                        Builders<mDocumentStore>.Update.
                                                                        Set("Edit_User", request.UserEmailId).
                                                                        Set("Edit_Date", DateTime.Now).
                                                                        Set("MailStatus", request.MailStatus));

                                                if (objmDocumentStore != null)
                                                {
                                                    string res = await UpdatePipelineOnAcceptOrSuggest(new QuoteSetReq
                                                    {
                                                        EnquiryPipeline = "Agent Approval Pipeline",
                                                        PlacerEmail = request.UserEmailId,
                                                        QRFID = request.QRFID,
                                                        Remarks = request.Comments,
                                                        MailStatus = request.MailStatus.ToLower(),
                                                        QRFPriceID = request.QRFPriceID,
                                                        PlacerUser = request.UserName,
                                                        IsUI = request.Type == "agentaccept" ? true : false
                                                    });
                                                    response.MailStatus = "accepted";
                                                    response.Status = "notexists";
                                                    response.ResponseStatus.Status = "Success";
                                                    response.ResponseStatus.ErrorMessage = "Mail sent successfully.";
                                                }
                                                else
                                                {
                                                    response.Status = "invalid";
                                                    response.ResponseStatus.Status = "Error";
                                                    response.ResponseStatus.ErrorMessage = "Mail status not updated";
                                                }
                                            }
                                            else
                                            {
                                                response.Status = "exists";
                                                response.ResponseStatus.ErrorMessage = objEmail.MailStatus;
                                                response.ResponseStatus.Status = "Error";
                                            }
                                        }
                                        else
                                        {
                                            if (request.Type == "agentaccept")
                                            {
                                                response.MailStatus = request.MailStatus.ToLower();
                                                if (lstEmail?.Count > 0)
                                                {
                                                    objEmail = lstEmail.OrderByDescending(a => a.SendDate).FirstOrDefault();

                                                    if (objEmail.MailStatus == "accepted")
                                                    {
                                                        response.Status = "notsendtoclientaccepted";
                                                        response.ResponseStatus.ErrorMessage = "Previous proposal has been accepted by user already. Please send the email again if proposal is changed.";
                                                    }
                                                    else
                                                    {
                                                        response.Status = "notsendtoclientsuggestion";
                                                        response.ResponseStatus.ErrorMessage = "Previous proposal has been send the suggestion by user already. Please send the email again if proposal is changed.";
                                                    }
                                                }
                                                else
                                                {
                                                    response.Status = "notsendtoclientyet";
                                                    response.ResponseStatus.ErrorMessage = "You have not send the mail to Client.";
                                                }
                                            }
                                            else
                                            {
                                                response.Status = "invalid";
                                                response.ResponseStatus.ErrorMessage = "Document_Id Details not exists.";
                                            }
                                            response.ResponseStatus.Status = "Error";
                                            response.CostingGetRes.CostingGetProperties.AgentInfo.AgentName = resultQRFPrice.AgentInfo.AgentName;
                                            response.CostingGetRes.CostingGetProperties.AgentInfo.MobileNo = resultQRFPrice.AgentInfo.MobileNo;
                                        }
                                    }
                                    else
                                    {
                                        response.Status = "invalid";
                                        response.ResponseStatus.ErrorMessage = "QRFPrice Details not exists.";
                                        response.ResponseStatus.Status = "Error";
                                        response.CostingGetRes.CostingGetProperties.AgentInfo.AgentName = resultQRFPrice.AgentInfo.AgentName;
                                        response.CostingGetRes.CostingGetProperties.AgentInfo.MobileNo = resultQRFPrice.AgentInfo.MobileNo;
                                    }
                                }
                                else
                                {
                                    response.Status = "invalid";
                                    response.ResponseStatus.Status = "Error";
                                    response.ResponseStatus.ErrorMessage = "Sales Officer contact details can not be null.";
                                    response.CostingGetRes.CostingGetProperties.AgentInfo.AgentName = resultQRFPrice.AgentInfo.AgentName;
                                    response.CostingGetRes.CostingGetProperties.AgentInfo.MobileNo = resultQRFPrice.AgentInfo.MobileNo;
                                }
                            }
                            else
                            {
                                response.Status = "invalid";
                                response.ResponseStatus.Status = "Error";
                                response.ResponseStatus.ErrorMessage = "Sales Officer Email can not be null/blank";
                                response.CostingGetRes.CostingGetProperties.AgentInfo.AgentName = resQuote.AgentInfo.AgentName;
                                response.CostingGetRes.CostingGetProperties.AgentInfo.MobileNo = resQuote.AgentInfo.MobileNo;
                            }
                        }
                        else
                        {
                            response.Status = "invalid";
                            response.ResponseStatus.Status = "Error";
                            response.ResponseStatus.ErrorMessage = "Mail status not valid.";
                            response.CostingGetRes.CostingGetProperties.AgentInfo.AgentName = resQuote.AgentInfo.AgentName;
                            response.CostingGetRes.CostingGetProperties.AgentInfo.MobileNo = resQuote.AgentInfo.MobileNo;
                        }
                    }
                    else
                    {
                        response.Status = "invalid";
                        response.ResponseStatus.ErrorMessage = "QRFID not exists.";
                        response.ResponseStatus.Status = "Error";
                    }
                }
                else
                {
                    response.Status = "invalid";
                    response.ResponseStatus.Status = "Error";
                    response.ResponseStatus.ErrorMessage = "Mail status can not be blank.";
                }
            }
            catch (Exception ex)
            {
                response.Status = "invalid";
                response.ResponseStatus.Status = "Error";
                response.ResponseStatus.ErrorMessage = ex.Message.ToString();
            }
            return response;
        }

        public async Task<SendToClientGetRes> SetSuggestSendToClient(SendToClientGetReq request)
        {
            SendToClientGetRes response = new SendToClientGetRes();

            try
            {
                if (!string.IsNullOrEmpty(request.MailStatus) && request.MailStatus.ToLower() == "suggest")
                {
                    var resQuote = _MongoContext.mQuote.AsQueryable().Where(a => a.QRFID == request.QRFID).FirstOrDefault();
                    if (resQuote != null)
                    {
                        var resultQRFPrice = await _MongoContext.mQRFPrice.FindAsync(m => m.QRFID == request.QRFID && m.QRFPrice_Id == request.QRFPriceID && m.IsDeleted == false).Result.FirstOrDefaultAsync();

                        if (resultQRFPrice != null)
                        {
                            var objemail = _MongoContext.mDocumentStore.AsQueryable().Where(a => a.QRFID == request.QRFID && a.QRFPriceId == request.QRFPriceID
                                                        && a.Document_Id == request.Document_Id).FirstOrDefault();
                            if (objemail != null)
                            {
                                response.MailStatus = objemail.MailStatus;

                                if (string.IsNullOrEmpty(objemail.MailStatus))
                                {
                                    var objmQuote = await _MongoContext.mDocumentStore.FindOneAndUpdateAsync(m => m.Document_Id == request.Document_Id,
                                                                        Builders<mDocumentStore>.Update.
                                                                        Set("Edit_User", objemail.To.FirstOrDefault()).
                                                                        Set("Edit_Date", DateTime.Now).
                                                                        Set("MailStatus", "suggest"));

                                    string res = await UpdatePipelineOnAcceptOrSuggest(new QuoteSetReq
                                    {
                                        EnquiryPipeline = "Agent Approval Pipeline",
                                        PlacerEmail = objemail.From,
                                        QRFID = request.QRFID,
                                        Remarks = request.Comments,
                                        MailStatus = request.MailStatus.ToLower(),
                                        QRFPriceID = request.QRFPriceID
                                    });

                                    response.ResponseStatus.Status = "Success";
                                    response.ResponseStatus.ErrorMessage = "Suggestion details send successfully.";
                                }
                                else
                                {
                                    response.ResponseStatus.ErrorMessage = objemail.MailStatus;
                                    response.ResponseStatus.Status = "exists";
                                }
                            }
                            else
                            {
                                response.ResponseStatus.ErrorMessage = "QRFPrice Email ID Details not exists.";
                                response.ResponseStatus.Status = "Error";
                            }
                        }
                        else
                        {
                            response.ResponseStatus.ErrorMessage = "QRFPrice Details not exists.";
                            response.ResponseStatus.Status = "Error";
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Error";
                        response.ResponseStatus.ErrorMessage = "QRFID not exists.";
                    }
                }
                else
                {
                    response.ResponseStatus.ErrorMessage = "Mail status not valid";
                    response.ResponseStatus.Status = "Error";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.ErrorMessage = ex.Message;
                response.ResponseStatus.Status = "Error";
            }
            return response;
        }

        public async Task<CostingGetRes> GetSuggestSendToClient(GetSuggestionReq request)
        {
            CostingGetRes response = new CostingGetRes();
            response.CostingGetProperties = new CostingGetProperties();

            try
            {
                if (!string.IsNullOrEmpty(request.MailStatus))
                {
                    var resQuote = await _MongoContext.mQuote.FindAsync(a => a.QRFID == request.QRFID).Result.FirstOrDefaultAsync();
                    if (resQuote != null)
                    {
                        var resQRFPriceList = await _MongoContext.mQRFPrice.FindAsync(a => a.QRFID == request.QRFID && a.IsCurrentVersion == true).Result.ToListAsync();

                        if (resQRFPriceList != null && resQRFPriceList.Count > 0)
                        {
                            var resQRFPrice = resQRFPriceList.OrderByDescending(a => a.VersionId).FirstOrDefault();

                            if (request.MailStatus.ToLower() == "suggest")
                            {
                                response = GetCostingDetails(request.QRFID, request.QRFPriceID, request.Document_Id);
                                if (response != null && response.ResponseStatus != null && response.ResponseStatus.Status.ToLower() == "error")
                                {
                                    response.Status = "invalid";
                                    response.CostingGetProperties.AgentInfo.AgentName = resQRFPrice.AgentInfo.AgentName;
                                    response.CostingGetProperties.AgentInfo.MobileNo = resQRFPrice.AgentInfo.MobileNo;
                                }
                                if (!string.IsNullOrEmpty(resQRFPrice.SalesOfficer))
                                {
                                    ContactDetailsResponse objContactDetailsRes = _userRepository.GetContactsByEmailId(new ContactDetailsRequest { Email = resQRFPrice.SalesOfficer });
                                    if (objContactDetailsRes != null && objContactDetailsRes.Contacts != null)
                                    {
                                        var lastname = !string.IsNullOrEmpty(objContactDetailsRes.Contacts.LastNAME) ? " " + objContactDetailsRes.Contacts.LastNAME : "";
                                        response.SalesOfficerName = objContactDetailsRes.Contacts.FIRSTNAME + lastname;
                                        response.SalesOfficerPhone = !string.IsNullOrEmpty(objContactDetailsRes.Contacts.MOBILE) ? objContactDetailsRes.Contacts.MOBILE : objContactDetailsRes.Contacts.TEL;
                                        if (string.IsNullOrEmpty(response.MailStatus) && response.ResponseStatus != null && response.ResponseStatus.Status.ToLower() == "success")
                                        {
                                            response.MailStatus = "suggest";
                                            response.Status = "notexists";
                                        }
                                        else if (!string.IsNullOrEmpty(response.MailStatus) && response.ResponseStatus != null && response.ResponseStatus.Status.ToLower() == "success")
                                        {
                                            response.Status = "exists";
                                        }
                                    }
                                    else
                                    {
                                        response.Status = "invalid";
                                        response.ResponseStatus.ErrorMessage = "Contact details not exists.";
                                        response.ResponseStatus.Status = "Error";
                                    }
                                }
                                else
                                {
                                    response.Status = "invalid";
                                    response.ResponseStatus.ErrorMessage = "Sales Person email id not exists.";
                                    response.ResponseStatus.Status = "Error";
                                    response.CostingGetProperties.AgentInfo.AgentName = resQRFPrice.AgentInfo.AgentName;
                                    response.CostingGetProperties.AgentInfo.MobileNo = resQRFPrice.AgentInfo.MobileNo;
                                }
                                response.EnquiryPipeline = resQuote.CurrentPipeline;
                            }
                            else
                            {
                                response.Status = "invalid";
                                response.ResponseStatus.Status = "Error";
                                response.ResponseStatus.ErrorMessage = "Mail status not valid";
                                response.CostingGetProperties.AgentInfo.AgentName = resQRFPrice.AgentInfo.AgentName;
                                response.CostingGetProperties.AgentInfo.MobileNo = resQRFPrice.AgentInfo.MobileNo;
                            }
                        }
                        else
                        {
                            response.ResponseStatus.ErrorMessage = "QRFID not exists in mQRFPrice.";
                            response.ResponseStatus.Status = "Error";
                            response.Status = "invalid";
                        }
                    }
                    else
                    {
                        response.ResponseStatus.ErrorMessage = "QRFID not exists mQuote.";
                        response.ResponseStatus.Status = "Error";
                        response.Status = "invalid";
                    }
                }
                else
                {
                    response.Status = "invalid";
                    response.ResponseStatus.Status = "Error";
                    response.ResponseStatus.ErrorMessage = "Mail status can not be blank.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.ErrorMessage = ex.Message;
                response.ResponseStatus.Status = "Error";
                response.Status = "invalid";
            }
            return response;
        }

        public CostingGetRes GetCostingDetails(string QRFID, string QRFPriceId, string Document_Id)
        {
            CostingGetRes response = new CostingGetRes();

            var resultQRFPrice = _MongoContext.mQRFPrice.AsQueryable().Where(m => m.QRFID == QRFID && m.QRFPrice_Id == QRFPriceId && m.IsDeleted == false).FirstOrDefault();

            if (resultQRFPrice != null)
            {
                response.CostingGetProperties.QRFID = resultQRFPrice.QRFID;
                response.CostingGetProperties.VersionId = resultQRFPrice.VersionId;
                response.CostingGetProperties.VersionName = resultQRFPrice.VersionName;
                response.CostingGetProperties.VersionDescription = resultQRFPrice.VersionDescription;
                response.CostingGetProperties.IsCurrentVersion = resultQRFPrice.IsCurrentVersion;
                response.CostingGetProperties.SalesOfficer = resultQRFPrice.SalesOfficer;
                response.CostingGetProperties.CostingOfficer = resultQRFPrice.CostingOfficer;
                response.CostingGetProperties.ProductAccountant = resultQRFPrice.ProductAccountant;
                response.CostingGetProperties.ValidForAcceptance = resultQRFPrice.ValidForAcceptance;
                response.CostingGetProperties.ValidForTravel = resultQRFPrice.ValidForTravel;
                response.CostingGetProperties.AgentInfo = resultQRFPrice.AgentInfo;
                response.CostingGetProperties.AgentProductInfo = resultQRFPrice.AgentProductInfo;
                response.CostingGetProperties.AgentPassengerInfo = resultQRFPrice.AgentPassengerInfo;
                response.CostingGetProperties.AgentRoom = resultQRFPrice.QRFAgentRoom;
                response.CostingGetProperties.DepartureDates = resultQRFPrice.Departures;
                response.CostingGetProperties.Document_Id = "";

                var mail = _MongoContext.mDocumentStore.AsQueryable().Where(a => a.Document_Id == Document_Id).FirstOrDefault();
                if (mail != null)
                {
                    response.CostingGetProperties.Document_Id = Document_Id;
                    response.MailStatus = mail.MailStatus;
                    response.ResponseStatus.Status = "Success";
                }
                else
                {
                    response.ResponseStatus.ErrorMessage = "mQRFPrice Email ID Details not exists.";
                    response.ResponseStatus.Status = "Error";
                }
            }
            else
            {
                response.ResponseStatus.ErrorMessage = "mQRFPrice Details not exists.";
                response.ResponseStatus.Status = "Error";
            }
            return response;
        }

        public async Task<string> UpdatePipelineOnAcceptOrSuggest(QuoteSetReq request)
        {
            try
            {
                var resQuote = _MongoContext.mQuote.AsQueryable().Where(a => a.QRFID == request.QRFID).FirstOrDefault();
                if (resQuote != null)
                {
                    if (!string.IsNullOrEmpty(request.EnquiryPipeline))
                    {
                        if (request.EnquiryPipeline == "Agent Approval Pipeline" && request.MailStatus == "accepted")
                        {
                            await _MongoContext.mQuote.FindOneAndUpdateAsync(
                                                  Builders<mQuote>.Filter.Eq("QRFID", request.QRFID),
                                                  Builders<mQuote>.Update.
                                                  Set("CurrentPipeline", "Handover Pipeline").
                                                  Set("CurrentPipelineStep", "Itinerary").
                                                  Set("Remarks", request.Remarks).
                                                  Set("CurrentPipelineSubStep", "").
                                                  Set("QuoteResult", "Success").
                                                  Set("Status", "NewHandoverPipeline").
                                                  Set("EditUser", request.PlacerEmail).
                                                  Set("EditDate", DateTime.Now)
                                                  );
                            if (request.IsUI)
                            {
                                #region Add Followup 
                                request.PlacerEmail=request.PlacerEmail.ToLower().Trim();
                                var SalesOfficer = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == request.QRFID && a.IsCurrentVersion == true).FirstOrDefault().SalesOfficer?.ToLower().Trim();
                                var CompanyList = _MongoContext.mCompanies.AsQueryable();
                                var FromUserContacts = CompanyList.Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a => a.MAIL.ToLower() == request.PlacerEmail)).FirstOrDefault()?.ContactDetails;
                                var FromUser = FromUserContacts?.Where(a => a.MAIL.ToLower() == request.PlacerEmail).FirstOrDefault();
                                var ToUserContacts = CompanyList.Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a => a.MAIL.ToLower() == SalesOfficer)).FirstOrDefault()?.ContactDetails;
                                var ToUser = ToUserContacts?.Where(a => a.MAIL.ToLower() == SalesOfficer).FirstOrDefault();

                                FollowUpSetRes response = new FollowUpSetRes();
                                FollowUpSetReq followUprequest = new FollowUpSetReq();
                                followUprequest.QRFID = request.QRFID;

                                FollowUpTask task = new FollowUpTask();
                                task.Task = "Proposal Accepted";
                                task.FollowUpType = "Internal";
                                task.FollowUpDateTime = DateTime.Now;

                                task.FromEmail = request.PlacerEmail;
                                if (FromUser != null)
                                {
                                    task.FromName = FromUser.CommonTitle + " " + FromUser.FIRSTNAME + " " + FromUser.LastNAME;
                                    task.FromContact_Id = FromUser.Contact_Id;
                                }

                                task.ToEmail = SalesOfficer;
                                if (ToUser != null)
                                {
                                    task.ToName = ToUser.CommonTitle + " " + ToUser.FIRSTNAME + " " + ToUser.LastNAME;
                                    task.ToContact_Id = ToUser.Contact_Id;
                                }

                                task.Status = "Confirmed";
                                task.Notes = "Proposal Accepted";

                                var FollowUpTaskList = new List<FollowUpTask>();
                                FollowUpTaskList.Add(task);

                                followUprequest.FollowUp.Add(new FollowUp
                                {
                                    FollowUp_Id = Guid.NewGuid().ToString(),
                                    FollowUpTask = FollowUpTaskList,
                                    CreateUser = request.PlacerEmail,
                                    CreateDate = DateTime.Now
                                });
                                await _quoteRepository.SetFollowUpForQRF(followUprequest);
                                #endregion
                            }
                            else
                            {
                                #region Add Followup 
                                var AgentContact = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == request.QRFID && a.IsCurrentVersion == true).FirstOrDefault()?.AgentInfo?.EmailAddress?.ToLower().Trim();

                                var FromMail = new mMailServerConfiguration();
                                string FromUserMail = "";
                                if (!string.IsNullOrEmpty(AgentContact))
                                {
                                    FromMail = _emailRepository.GetSmtpCredentials(AgentContact);
                                }
                                else
                                {
                                    FromMail = _emailRepository.GetSmtpCredentials("matt.watson@coxandkings.com");
                                }
                                FromUserMail = Encrypt.DecryptData("", FromMail.UserName);

                                var SalesOfficer = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == request.QRFID && a.IsCurrentVersion == true).FirstOrDefault()?.SalesOfficer?.ToLower().Trim();
                                var CompanyList = _MongoContext.mCompanies.AsQueryable();
                                var ToUserContacts = CompanyList.Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a => a.MAIL.ToLower() == AgentContact)).FirstOrDefault()?.ContactDetails;
                                var ToUser = ToUserContacts?.Where(a => a.MAIL.ToLower() == AgentContact).FirstOrDefault();

                                FollowUpSetRes response = new FollowUpSetRes();
                                FollowUpSetReq followUprequest = new FollowUpSetReq();
                                followUprequest.QRFID = request.QRFID;

                                FollowUpTask task = new FollowUpTask();
                                task.Task = "Proposal Accepted";
                                task.FollowUpType = "External";
                                task.FollowUpDateTime = DateTime.Now;

                                task.FromEmail = FromUserMail;
                                if (!string.IsNullOrEmpty(FromUserMail))
                                {
                                    task.FromName = FromUserMail.Split('@')[0];
                                    task.FromContact_Id = "";
                                }

                                task.ToEmail = SalesOfficer;
                                if (ToUser != null)
                                {
                                    task.ToName = ToUser.CommonTitle + " " + ToUser.FIRSTNAME + " " + ToUser.LastNAME;
                                    task.ToContact_Id = ToUser.Contact_Id;
                                }

                                task.Status = "Replied";
                                task.Notes = "Proposal Accepted";

                                var FollowUpTaskList = new List<FollowUpTask>();
                                FollowUpTaskList.Add(task);

                                followUprequest.FollowUp.Add(new FollowUp
                                {
                                    FollowUp_Id = Guid.NewGuid().ToString(),
                                    FollowUpTask = FollowUpTaskList,
                                    CreateUser = request.PlacerEmail,
                                    CreateDate = DateTime.Now
                                });
                                await _quoteRepository.SetFollowUpForQRF(followUprequest);


                                #region Mail Send
                                EmailGetReq requestEmail = new EmailGetReq();
                                requestEmail.QrfId = request.QRFID;
                                requestEmail.FollowUpId = followUprequest.FollowUp[0].FollowUp_Id;
                                requestEmail.UserEmail = FromUserMail;
                                requestEmail.DocumentType = DocType.QUOTEFOLLOWUP;
                                await _emailRepository.GenerateEmail(requestEmail);
                                #endregion
                                #endregion
                            }
                        }
                        else if (request.EnquiryPipeline == "Agent Approval Pipeline" && request.MailStatus == "suggest")
                        {
                            await AmendmentQuote(new AmendmentQuoteReq { QRFID = request.QRFID, EditUser = request.PlacerEmail }, true);
                        }
                        #region Send Email
                        var objEmailGetReq = new EmailGetReq()
                        {
                            UserEmail = request.PlacerEmail,
                            UserName = request.PlacerUser,
                            QrfId = request.QRFID,
                            QRFPriceId = request.QRFPriceID,
                            Remarks = request.Remarks,
                            DocumentType = request.MailStatus == "accepted" ? DocType.MAILAGENTACCEPT : DocType.MAILAGENTREJECT,
                            EnquiryPipeline = request.EnquiryPipeline,
                            MailStatus = request.MailStatus,
                            IsUI = request.IsUI
                        };
                        var responseStatusMail = await _emailRepository.GenerateEmail(objEmailGetReq);
                        if (responseStatusMail == null || responseStatusMail.ResponseStatus == null || string.IsNullOrEmpty(responseStatusMail.ResponseStatus.Status))
                        {
                            responseStatusMail.ResponseStatus = new ResponseStatus();
                            responseStatusMail.ResponseStatus.Status = "Error";
                            responseStatusMail.ResponseStatus.ErrorMessage = "Mail not sent.";
                        }
                        #endregion
                        return request.QRFID;
                    }
                }
            }
            catch (Exception ex)
            {
                //return 0;
            }
            return "0";
        }
        #endregion

        public async Task<CommonResponse> AcceptWithoutProposal(EmailGetReq request)
        {
            CommonResponse objCommonResponse = new CommonResponse() { ResponseStatus = new ResponseStatus(), QRFID = request.QrfId };
            try
            {
                var resQuote = await _MongoContext.mQuote.FindAsync(a => a.QRFID == request.QrfId).Result.FirstOrDefaultAsync();
                if (resQuote != null)
                {
                    var QRFPrice = _MongoContext.mQRFPrice.AsQueryable().Where(x => x.QRFID == request.QrfId && x.IsCurrentVersion == true && x.IsDeleted == false).OrderByDescending(y => y.VersionId).FirstOrDefault();
                    if (QRFPrice != null)
                    {
                        #region Send Email
                        var objEmailGetReq = new EmailGetReq()
                        {
                            UserEmail = request.UserEmail,
                            UserName = request.UserName,
                            QrfId = request.QrfId,
                            QRFPriceId = QRFPrice.QRFPrice_Id,
                            EnquiryPipeline = "Agent Approval Pipeline",
                            PlacerUserId = request.PlacerUserId,
                            DocumentType = DocType.ACCEPTWITHOUTPROPOSAL
                        };
                        var responseStatusMail = await _emailRepository.GenerateEmail(objEmailGetReq);
                        
                        if (responseStatusMail == null || responseStatusMail.ResponseStatus == null || string.IsNullOrEmpty(responseStatusMail.ResponseStatus.Status))
                        {
                            objCommonResponse.ResponseStatus = new ResponseStatus();
                            objCommonResponse.ResponseStatus.Status = "Error";
                            objCommonResponse.ResponseStatus.ErrorMessage = "Mail not sent.";
                            return objCommonResponse;
                        }
                        else
                        {
                            objCommonResponse.ResponseStatus = responseStatusMail.ResponseStatus;
                        }
                        #endregion

                        if (objCommonResponse.ResponseStatus.Status.ToLower() == "success")
                        {
                            await _MongoContext.mQuote.FindOneAndUpdateAsync(
                                                  Builders<mQuote>.Filter.Eq("QRFID", request.QrfId),
                                                  Builders<mQuote>.Update.
                                                  Set("CurrentPipeline", "Handover Pipeline").
                                                  Set("CurrentPipelineStep", request.CurrentPipelineStep).
                                                  Set("CurrentPipelineSubStep", "").
                                                  Set("QuoteResult", "Success").
                                                  Set("Status", "NewHandoverPipeline").
                                                  Set("EditUser", request.UserName).
                                                  Set("EditDate", DateTime.Now).
                                                  Set("HandoverWithoutEmail", true));

                            objCommonResponse.ResponseStatus.Status = "Success";
                        } 
                    }
                    else
                    {
                        objCommonResponse.ResponseStatus.Status = "Failure";
                        objCommonResponse.ResponseStatus.ErrorMessage = "QRFID not exists in mQRFPrice.";
                    }
                }
                else
                {
                    objCommonResponse.ResponseStatus.Status = "Failure";
                    objCommonResponse.ResponseStatus.ErrorMessage = "QRFID not exists in mQuote.";
                }
            }
            catch (Exception ex)
            {
                objCommonResponse.ResponseStatus.Status = "Failure";
                objCommonResponse.ResponseStatus.ErrorMessage = "An Error Occurs:- " + ex.Message;
            }

            return objCommonResponse;
        }

        public async Task<CommonResponse> AmendmentQuote(AmendmentQuoteReq request, bool IsSuggestion = false)
        {
            CommonResponse response = new CommonResponse();
            try
            {
                var quote_old = _MongoContext.mQuote.AsQueryable().Where(a => a.QRFID == request.QRFID).FirstOrDefault();
                var quote_new = _MongoContext.mQuote.AsQueryable().Where(a => a.QRFID == request.QRFID).FirstOrDefault();

                if (string.IsNullOrEmpty(quote_old.LatestChild_QRFID))
                {
                    quote_new.QRFID = request.QRFID + "-1";
                    quote_new.AgentProductInfo.TourName = quote_new.AgentProductInfo.TourName + " - V1";
                }
                else
                {
                    string[] aa = Regex.Split(quote_old.LatestChild_QRFID, @"(\d+)(?!.*\d)");
                    quote_new.QRFID = aa[0] + (Convert.ToInt16(aa[1]) + 1);
                    quote_new.AgentProductInfo.TourName = quote_new.AgentProductInfo.TourName + " - V" + (Convert.ToInt16(aa[1]) + 1);
                }
                quote_new._Id = new MongoDB.Bson.ObjectId();
                quote_new.LatestChild_QRFID = "";
                quote_new.Parent_QRFID = request.QRFID;
                quote_new.CurrentPipeline = "Amendment Pipeline";
                quote_new.CurrentPipelineStep = "";
                quote_new.CurrentPipelineSubStep = "";
                quote_new.CreateDate = DateTime.Now;
                quote_new.CreateUser = request.EditUser;
                quote_new.EditDate = null;
                quote_new.EditUser = null;

                quote_old.LatestChild_QRFID = quote_new.QRFID;

                var FollowUp = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == request.QRFID && a.IsCurrentVersion == true).FirstOrDefault().FollowUp;
                quote_new.FollowUp = FollowUp;

                //New Quote Inserted, Old Quote updated
                await _MongoContext.mQuote.InsertOneAsync(quote_new);
                ReplaceOneResult replaceResult = await _MongoContext.mQuote.ReplaceOneAsync(Builders<mQuote>.Filter.Eq("QRFID", quote_old.QRFID), quote_old);

                var positionList = _MongoContext.mPosition.AsQueryable().Where(a => a.QRFID == request.QRFID).ToList();

                foreach (var pos in positionList)
                {
                    var positionPriceList = _MongoContext.mPositionPrice.AsQueryable().Where(a => a.PositionId == pos.PositionId).ToList();
                    var positionFOCList = _MongoContext.mPositionFOC.AsQueryable().Where(a => a.PositionId == pos.PositionId).ToList();

                    pos._Id = new MongoDB.Bson.ObjectId(); pos.QRFID = quote_new.QRFID; pos.PositionId = Guid.NewGuid().ToString();
                    await _MongoContext.mPosition.InsertOneAsync(pos);

                    if (positionPriceList != null)
                    {
                        if (positionFOCList.Count > 0)
                        {
                            positionPriceList.ForEach(a => { a._Id = new MongoDB.Bson.ObjectId(); a.QRFID = quote_new.QRFID; a.PositionId = pos.PositionId; a.PositionPriceId = Guid.NewGuid().ToString(); });
                            await _MongoContext.mPositionPrice.InsertManyAsync(positionPriceList);
                        }
                    }

                    if (positionFOCList != null)
                    {
                        if (positionFOCList.Count > 0)
                        {
                            positionFOCList.ForEach(a => { a._Id = new MongoDB.Bson.ObjectId(); a.QRFID = quote_new.QRFID; a.PositionId = pos.PositionId; a.PositionFOCId = Guid.NewGuid().ToString(); });
                            await _MongoContext.mPositionFOC.InsertManyAsync(positionFOCList);
                        }
                    }
                }

                //Function call for Costing
                // await _qRFSummaryRepository.SaveQRFPrice("Amendment", "Quote Amendment", quote_new.QRFID, request.EditUser);

                await _qRFSummaryRepository.SaveDefaultQRFPosition(quote_new.QRFID);
                await _qRFSummaryRepository.SaveDefaultGuesstimate(quote_new.QRFID);
                await _qRFSummaryRepository.SaveQRFPrice("Default", "Quote Amendment", quote_new.QRFID, request.EditUser);
                await _qRFSummaryRepository.SaveDefaultProposal(quote_new.QRFID, request.EditUser);

                if (IsSuggestion)
                {
                    #region Add Followup for Old_Quote
                    var AgentContact = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == request.QRFID && a.IsCurrentVersion == true).FirstOrDefault()?.AgentInfo?.EmailAddress?.Trim().ToLower();

                    var FromMail = new mMailServerConfiguration();
                    string FromUserMail = "";
                    if (!string.IsNullOrEmpty(AgentContact))
                    {
                        FromMail = _emailRepository.GetSmtpCredentials(AgentContact);
                    }
                    else
                    {
                        FromMail = _emailRepository.GetSmtpCredentials("matt.watson@coxandkings.com");
                    }
                    FromUserMail = Encrypt.DecryptData("", FromMail.UserName);

                    var SalesOfficer = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == quote_old.QRFID && a.IsCurrentVersion == true).FirstOrDefault()?.SalesOfficer?.ToLower().Trim();
                    var CompanyList = _MongoContext.mCompanies.AsQueryable();
                    var ToUserContacts = CompanyList.Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a => a.MAIL.ToLower() == AgentContact)).FirstOrDefault()?.ContactDetails;
                    var ToUser = ToUserContacts?.Where(a => a.MAIL.ToLower() == AgentContact).FirstOrDefault();

                    FollowUpSetReq followUprequest = new FollowUpSetReq();
                    followUprequest.QRFID = quote_old.QRFID;

                    FollowUpTask task = new FollowUpTask();
                    task.Task = "Amendment Requested";
                    task.FollowUpType = "External";
                    task.FollowUpDateTime = DateTime.Now;

                    task.FromEmail = FromUserMail;
                    if (!string.IsNullOrEmpty(FromUserMail))
                    {
                        task.FromName = FromUserMail.Split('@')[0];
                        task.FromContact_Id = "";
                    }

                    task.ToEmail = SalesOfficer;
                    if (ToUser != null)
                    {
                        task.ToName = ToUser.CommonTitle + " " + ToUser.FIRSTNAME + " " + ToUser.LastNAME;
                        task.ToContact_Id = ToUser.Contact_Id;
                    }

                    task.Status = "Replied";
                    task.Notes = "Amendment Requested";

                    var FollowUpTaskList = new List<FollowUpTask>();
                    FollowUpTaskList.Add(task);

                    followUprequest.FollowUp.Add(new FollowUp
                    {
                        FollowUp_Id = Guid.NewGuid().ToString(),
                        FollowUpTask = FollowUpTaskList,
                        CreateUser = request.EditUser,
                        CreateDate = DateTime.Now
                    });
                    await _quoteRepository.SetFollowUpForQRF(followUprequest);
                    #endregion

                    #region Add Followup for new_Quote

                    FollowUpSetReq followUprequestNew = new FollowUpSetReq();
                    followUprequestNew.QRFID = quote_new.QRFID;

                    FollowUpTask taskNew = new FollowUpTask();
                    taskNew.Task = "Amendment Requested";
                    taskNew.FollowUpType = "External";
                    taskNew.FollowUpDateTime = DateTime.Now;

                    task.FromEmail = FromUserMail;
                    if (!string.IsNullOrEmpty(FromUserMail))
                    {
                        task.FromName = FromUserMail.Split('@')[0];
                        task.FromContact_Id = "";
                    }

                    taskNew.ToEmail = SalesOfficer;
                    if (ToUser != null)
                    {
                        taskNew.ToName = ToUser.CommonTitle + " " + ToUser.FIRSTNAME + " " + ToUser.LastNAME;
                        taskNew.ToContact_Id = ToUser.Contact_Id;
                    }

                    taskNew.Status = "Requested";
                    taskNew.Notes = "Amendment Requested";

                    var FollowUpTaskListNew = new List<FollowUpTask>();
                    FollowUpTaskListNew.Add(taskNew);

                    followUprequestNew.FollowUp.Add(new FollowUp
                    {
                        FollowUp_Id = Guid.NewGuid().ToString(),
                        FollowUpTask = FollowUpTaskListNew,
                        CreateUser = request.EditUser,
                        CreateDate = DateTime.Now
                    });
                    await _quoteRepository.SetFollowUpForQRF(followUprequestNew);
                    #endregion

                    #region Mail Send
                    EmailGetReq requestEmail = new EmailGetReq();
                    requestEmail.QrfId = request.QRFID;
                    requestEmail.FollowUpId = followUprequest.FollowUp[0].FollowUp_Id;
                    requestEmail.UserEmail = FromUserMail;
                    requestEmail.DocumentType = DocType.QUOTEFOLLOWUP;
                    await _emailRepository.GenerateEmail(requestEmail);
                    #endregion
                }
                else
                {
                    #region Add Followup for Old_Quote
                    request.EditUser = request.EditUser.Trim().ToLower();
                    var CostingOfficer = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == quote_old.QRFID && a.IsCurrentVersion == true).FirstOrDefault()?.CostingOfficer?.ToLower().Trim();
                    var CompanyList = _MongoContext.mCompanies.AsQueryable();
                    var FromUserContacts = CompanyList.Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a => a.MAIL.ToLower() == request.EditUser)).FirstOrDefault()?.ContactDetails;
                    var FromUser = FromUserContacts?.Where(a => a.MAIL.ToLower() == request.EditUser).FirstOrDefault();
                    var ToUserContacts = CompanyList.Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a => a.MAIL.ToLower() == CostingOfficer)).FirstOrDefault()?.ContactDetails;
                    var ToUser = ToUserContacts?.Where(a => a.MAIL.ToLower() == CostingOfficer).FirstOrDefault();

                    FollowUpSetReq followUprequest = new FollowUpSetReq();
                    followUprequest.QRFID = quote_old.QRFID;

                    FollowUpTask task = new FollowUpTask();
                    task.Task = "Amendment Requested";
                    task.FollowUpType = "Internal";
                    task.FollowUpDateTime = DateTime.Now;

                    task.FromEmail = request.EditUser;
                    if (FromUser != null)
                    {
                        task.FromName = FromUser.CommonTitle + " " + FromUser.FIRSTNAME + " " + FromUser.LastNAME;
                        task.FromContact_Id = FromUser.Contact_Id;
                    }

                    task.ToEmail = CostingOfficer;
                    if (ToUser != null)
                    {
                        task.ToName = ToUser.CommonTitle + " " + ToUser.FIRSTNAME + " " + ToUser.LastNAME;
                        task.ToContact_Id = ToUser.Contact_Id;
                    }

                    task.Status = "Replied";
                    task.Notes = "Amendment Requested";

                    var FollowUpTaskList = new List<FollowUpTask>();
                    FollowUpTaskList.Add(task);

                    followUprequest.FollowUp.Add(new FollowUp
                    {
                        FollowUp_Id = Guid.NewGuid().ToString(),
                        FollowUpTask = FollowUpTaskList,
                        CreateUser = request.EditUser,
                        CreateDate = DateTime.Now
                    });
                    await _quoteRepository.SetFollowUpForQRF(followUprequest);
                    #endregion

                    #region Add Followup for new_Quote

                    FollowUpSetReq followUprequestNew = new FollowUpSetReq();
                    followUprequestNew.QRFID = quote_new.QRFID;

                    FollowUpTask taskNew = new FollowUpTask();
                    taskNew.Task = "Amendment Requested";
                    taskNew.FollowUpType = "Internal";
                    taskNew.FollowUpDateTime = DateTime.Now;

                    taskNew.FromEmail = request.EditUser;
                    if (FromUser != null)
                    {
                        taskNew.FromName = FromUser.CommonTitle + " " + FromUser.FIRSTNAME + " " + FromUser.LastNAME;
                        taskNew.FromContact_Id = FromUser.Contact_Id;
                    }

                    taskNew.ToEmail = CostingOfficer;
                    if (ToUser != null)
                    {
                        taskNew.ToName = ToUser.CommonTitle + " " + ToUser.FIRSTNAME + " " + ToUser.LastNAME;
                        taskNew.ToContact_Id = ToUser.Contact_Id;
                    }

                    taskNew.Status = "Requested";
                    taskNew.Notes = "Amendment Requested";

                    var FollowUpTaskListNew = new List<FollowUpTask>();
                    FollowUpTaskListNew.Add(taskNew);

                    followUprequestNew.FollowUp.Add(new FollowUp
                    {
                        FollowUp_Id = Guid.NewGuid().ToString(),
                        FollowUpTask = FollowUpTaskListNew,
                        CreateUser = request.EditUser,
                        CreateDate = DateTime.Now
                    });
                    await _quoteRepository.SetFollowUpForQRF(followUprequestNew);
                    #endregion
                }

                response.QRFID = quote_new.QRFID;
                response.ResponseStatus.Status = "Success";
                return response;

            }
            catch (Exception e)
            {
                response.ResponseStatus.ErrorMessage = e.Message;
                response.ResponseStatus.Status = "Failure";
                throw;
            }
        }

        public async Task<CommonResponse> CheckProposalGenerated(QuoteGetReq request)
        {
            CommonResponse objCommonResponse = new CommonResponse() { ResponseStatus = new ResponseStatus(), QRFID = request.QRFID };
            try
            {
                var resQuote = await _MongoContext.mQuote.FindAsync(a => a.QRFID == request.QRFID).Result.FirstOrDefaultAsync();
                if (resQuote != null)
                {
                    var QRFPrice = _MongoContext.mQRFPrice.AsQueryable().Where(x => x.QRFID == request.QRFID && x.IsCurrentVersion == true && x.IsDeleted == false).OrderByDescending(y => y.VersionId).FirstOrDefault();
                    if (QRFPrice != null)
                    {
                        var pdfTourFile = CommonFunction.FormatFileName(QRFPrice.AgentProductInfo.TourName) + ".pdf";
                        string filepath = Path.Combine(_configuration.GetValue<string>("ProposalPDFPath"), pdfTourFile);
                        if (File.Exists(filepath))
                        {
                            objCommonResponse.ResponseStatus.Status = "Success";
                        }
                        else
                        {
                            objCommonResponse.ResponseStatus.Status = "Failure";
                            objCommonResponse.ResponseStatus.ErrorMessage = "Proposal is not generated. Please generate the proposal document.";
                        }
                    }
                    else
                    {
                        objCommonResponse.ResponseStatus.Status = "Failure";
                        objCommonResponse.ResponseStatus.ErrorMessage = "QRFID not exists in mQRFPrice.";
                    }
                }
                else
                {
                    objCommonResponse.ResponseStatus.Status = "Failure";
                    objCommonResponse.ResponseStatus.ErrorMessage = "QRFID not exists in mQuote.";
                }
            }
            catch (Exception ex)
            {
                objCommonResponse.ResponseStatus.Status = "Failure";
                objCommonResponse.ResponseStatus.ErrorMessage = "An Error Occurs:- " + ex.Message;
            }

            return objCommonResponse;
        }
    }
}