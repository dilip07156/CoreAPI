using MailKit.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VGER_WAPI.Helpers;
using VGER_WAPI.Models;
using VGER_WAPI_CLASSES;
//comment before commiting to development
//using Microsoft.Office.Interop.Outlook;
//using OutlookApp = Microsoft.Office.Interop.Outlook.Application;

namespace VGER_WAPI.Repositories
{
    public class EmailRepository : IEmailRepository
    {
        #region
        private readonly MongoContext _MongoContext = null;
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _env;
        private readonly IGenericRepository _genericRepository;
        private readonly IDocumentStoreRepository _documentStoreRepository;
        #endregion

        public EmailRepository(IOptions<MongoSettings> settings, IConfiguration configuration, IHostingEnvironment env, IGenericRepository genericRepository, IDocumentStoreRepository documentStoreRepository)
        {
            _MongoContext = new MongoContext(settings);
            _configuration = configuration;
            _env = env;
            _genericRepository = genericRepository;
            _documentStoreRepository = documentStoreRepository;
        }

        /// <summary>
        /// Create Email Template and Send Email function
        /// </summary>
        /// <param name="request">Required params for template creation and send email</param>
        /// <returns>response</returns>
        public async Task<EmailGetRes> GenerateEmail(EmailGetReq request)
        {
            EmailGetRes response = new EmailGetRes();
            try
            {
                List<EmailTemplateGetRes> emailContent = new List<EmailTemplateGetRes>();

                if (!string.IsNullOrEmpty(request.DocumentType))
                {
                    string docType = request.DocumentType.ToUpper();
                    string pathToFile = "";
                    if (docType != DocType.SENDTOCLIENT)
                    {
                        pathToFile = GetPath(docType);
                        if (string.IsNullOrEmpty(pathToFile))
                        {
                            response.ResponseStatus.Status = "Error";
                            response.ResponseStatus.ErrorMessage = "File path not found";
                            return response;
                        }
                    }

                    switch (docType)
                    {
                        case DocType.BOOKXX:
                            {
                                if (request?.PositionIds?.Count > 1)
                                {
                                    emailContent = await CreateMultipleBookXXEmailTemplate(request, pathToFile);
                                }
                                else
                                {
                                    emailContent = await CreateBookXXEmailTemplate(request, pathToFile);
                                }
                                break;
                            }
                        case DocType.BOOKKK:
                            {
                                emailContent = await CreateBookKKEmailTemplate(request, pathToFile);
                                break;
                            }
                        case DocType.BOOKPROV:
                            {
                                emailContent = await CreateBOOKPROVEmailTemplate(request, pathToFile);
                                break;
                            }
                        case DocType.BOOKOPTXX:
                            {
                                emailContent = await CreateBOOKOPTXXEmailTemplate(request, pathToFile);
                                break;
                            }
                        case DocType.HOTELNOTAVAILABLE:
                            {
                                emailContent = await CreateHotelNotAvailableEmailTemplate(request, pathToFile);
                                break;
                            }
                        case DocType.HOTELAVAILABLE:
                            {
                                emailContent = await CreateHotelAvailableEmailTemplate(request, pathToFile);
                                break;
                            }
                        case DocType.FOLLOWUP:
                            {
                                emailContent = await CreateFollowupEmailTemplate(request, pathToFile);
                                break;
                            }
                        case DocType.FOLLOWUPEXPENSIVE:
                            {
                                emailContent = await CreateFollowupExpensiveEmailTemplate(request, pathToFile);
                                break;
                            }
                        case DocType.BOOKOPTEXT:
                            {
                                emailContent = await CreateExtendOptionDateEmailTemplate(request, pathToFile);
                                break;
                            }
                        case DocType.GOAHEAD:
                            { 
                                emailContent = await CreateGoAheadBookingEmailTemplate(request, pathToFile);
                                break;
                            }
                        case DocType.REMIND:
                            {
                                emailContent = await CreateHotelRequestEmailTemplate(request, pathToFile);
                                break;
                            }
                        case DocType.TESTEMAIL:
                            {
                                emailContent = await CreateHotelRequestEmailTemplate(request, pathToFile);
                                break;
                            }
                        case DocType.BOOKREQ://HOTELREQUESTS
                            {
                                emailContent = await CreateHotelRequestEmailTemplate(request, pathToFile);
                                break;
                            }
                        case DocType.SALESSUBMITQUOTE: //sales to costing pipeline
                        case DocType.COAPPROVAL:  //costing pipeline approval || Amendment Pipeline approval 
                        case DocType.CAPAPPROVAL: //costing approval pipeline approval
                        case DocType.CAPREJECT:   //costing approval pipeline reject 
                            {
                                emailContent = await CreateSubmitQuoteEmailTemplate(request, pathToFile);
                                break;
                            }
                        case DocType.SENDTOCLIENT: //Agent Approval Pipeline->Send to client
                            {
                                emailContent = await CreateSendToClientEmailTemplate(request);
                                break;
                            }
                        case DocType.MAILAGENTACCEPT://Agent Approval Pipeline->From Mail Button Agent Accept
                        case DocType.MAILAGENTREJECT://Agent Approval Pipeline->From Mail Button Send Suggestion
                            {
                                emailContent = await CreateAcceptOrSuggestEmailTemplate(request, pathToFile);
                                break;
                            }
                        case DocType.ACCEPTWITHOUTPROPOSAL://Agent Approval Pipeline->ACCEPT WITHOUT PROPOSAL
                            {
                                emailContent = await CreateAcceptWithoutProposalEmailTemplate(request, pathToFile);
                                break;
                            }
                        case DocType.COREJECT:
                            {
                                emailContent = await CreateRejectCommercialEmailTemplate(request, pathToFile);
                                break;
                            }
                        case DocType.PWDRECOVER:
                            {
                                emailContent = CreateUserPasswordRecoverEmailTemplate(request, pathToFile);
                                break;
                            }
                        case DocType.QUOTEFOLLOWUP:
                            {
                                emailContent = await CreateQuoteFollowupEmailTemplate(request, pathToFile);
                                break;
                            }
                        case DocType.ERRORREPORT:
                            {                               
                                emailContent = await CreateErrorReportEmailTemplate(request, pathToFile); 
                                break;
                            }
                        case DocType.OPSHOTELCONFIRM: //OPS Hotel Booking Confirmation
                            {
                                emailContent = await CreateOPSBookConfirmEmailTemplate(request, pathToFile);
                                break;
                            }
                        case DocType.OPSPOSAMEND: //OPS Position Booking Amendment
                            {
                                emailContent = await CreateOPSPositionAmendmentEmailTemplate(request, pathToFile);
                                break;
                            }
                        default:
                            return response;
                    }

                    if (request.IsSendEmail == true && emailContent != null && emailContent.Count > 0)
                    {
                        try
                        { 
                            for (int i = 0; i < emailContent.Count; i++)
                            {
                                if (emailContent[i].ResponseStatus.Status.ToLower() == "success")
                                {
                                    emailContent[i].UserEmail = request.UserEmail;

                                    switch (docType)
                                    {
                                        case DocType.HOTELNOTAVAILABLE:
                                            {
                                                response = await SendEmail(emailContent[i], "default");
                                                break;
                                            }
                                        case DocType.HOTELAVAILABLE:
                                            {
                                                response = await SendEmail(emailContent[i], "default");
                                                break;
                                            }
                                        default:
                                            { 
                                                response = await SendEmail(emailContent[i],"",((DocType.GOAHEAD == "GO-AHEAD")?true:false));
                                                break;

                                            }
                                    }

                                    //response = await SendEmail(emailContent[i]);
                                    if (response != null && response.EmailTemplateGetRes != null && response.EmailTemplateGetRes.Count > 0)
                                    {
                                        request.SupplierId = emailContent[i].SupplierId;
                                        request.AlternateServiceId = emailContent[i].AlternateServiceId;
                                        response.EmailTemplateGetRes[0].Document_Id = emailContent[i].Document_Id;
                                        request.DocumentType = request.DocumentType == "SALES-AGENTACCEPT_AWP" ? "SALES-AGENTACCEPT" : request.DocumentType;
                                        request.PositionId = emailContent[i].EmailGetReq.PositionId;
                                        response.EmailTemplateGetRes[0].SendStatus = response.EmailTemplateGetRes[0].ResponseStatus?.Status?.ToLower() == "success" ? "Sent" : "Not Sent";
                                        bool flag = await SaveDocumentStore(response.EmailTemplateGetRes[0], request);
                                    }
                                }
                                else
                                { 
                                    response.ResponseStatus.Status = "Error";
                                    response.ResponseStatus.ErrorMessage += emailContent[i].ResponseStatus.ErrorMessage + "|";
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            response.ResponseStatus.Status = "Error";
                            response.ResponseStatus.ErrorMessage = ex.Message;
                        }
                    }
                    else if (request.IsSendEmail == false && emailContent != null && emailContent.Count > 0)
                    {
                        if (request.IsSaveDocStore == false)
                        {
                            response.EmailTemplateGetRes = emailContent;
                            response.ResponseStatus.Status = "Success";
                            response.ResponseStatus.ErrorMessage = "Email content generated";
                        }
                        else
                        {
                            for (int i = 0; i < emailContent.Count; i++)
                            {
                                if (emailContent[i].ResponseStatus.Status.ToLower() == "success")
                                {
                                    response.EmailTemplateGetRes = new List<EmailTemplateGetRes>();
                                    emailContent[i].UserEmail = request.UserEmail;
                                    request.SupplierId = emailContent[i].SupplierId;
                                    request.AlternateServiceId = emailContent[i].AlternateServiceId;                                     
                                    response.EmailTemplateGetRes.Add(emailContent[i]); 
                                    request.DocumentType = request.DocumentType == "SALES-AGENTACCEPT_AWP" ? "SALES-AGENTACCEPT" : request.DocumentType;
                                    request.PositionId = emailContent[i].EmailGetReq.PositionId;
                                    response.EmailTemplateGetRes[0].SendStatus = "Not Sent";
                                    bool flag = await SaveDocumentStore(response.EmailTemplateGetRes[0], request);
                                    response.ResponseStatus.Status = "Success";
                                    response.ResponseStatus.StatusMessage = "Document Saved Successfully.";
                                }
                                else
                                {
                                    response.ResponseStatus.Status = "Error";
                                    response.ResponseStatus.ErrorMessage += emailContent[i].ResponseStatus.ErrorMessage + "|";
                                }
                            }
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Error";
                        response.ResponseStatus.ErrorMessage = "Email content not found";
                    }
                }
                else
                { 
                    response.ResponseStatus.Status = "Error";
                    response.ResponseStatus.ErrorMessage = "Document Type not can not be Null/Empty";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Error";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }

            response.ResponseStatus.ErrorMessage = response.ResponseStatus?.ErrorMessage?.Trim().TrimEnd('|').Trim();
            return response;
        }

        #region Email Template Common Methods 
        /// <summary>
        /// To get template path from config file
        /// </summary>
        /// <param name="documentType"></param>
        /// <returns></returns>
        public string GetPath(string documentType)
        {
            string templatePath, pathToFile;
            try
            {
                string filePath = _env.ContentRootPath;
                string doctype = documentType.ToUpper();

                switch (doctype)
                {
                    case DocType.BOOKXX:
                        {
                            templatePath = _configuration.GetValue<string>("EmailTemplates:HotelBookingCancellation");
                            break;
                        }
                    case DocType.BOOKKK:
                        {
                            templatePath = _configuration.GetValue<string>("EmailTemplates:HotelBookingConfirmation");
                            break;
                        }
                    case DocType.BOOKPROV:
                        {
                            templatePath = _configuration.GetValue<string>("EmailTemplates:HotelBookingProvisional");
                            break;
                        }
                    case DocType.BOOKOPTXX:
                        {
                            templatePath = _configuration.GetValue<string>("EmailTemplates:HotelProvisionalBookingCancellation");
                            break;
                        }
                    case DocType.HOTELNOTAVAILABLE:
                        {
                            templatePath = _configuration.GetValue<string>("EmailTemplates:HotelNotAvailableEmail");
                            break;
                        }
                    case DocType.HOTELAVAILABLE:
                        {
                            templatePath = _configuration.GetValue<string>("EmailTemplates:HotelAvailableEmail");
                            break;
                        }
                    case DocType.FOLLOWUP:
                        {
                            templatePath = _configuration.GetValue<string>("EmailTemplates:Followup");
                            break;
                        }
                    case DocType.FOLLOWUPEXPENSIVE:
                        {
                            templatePath = _configuration.GetValue<string>("EmailTemplates:Followup-Expensive");
                            break;
                        }
                    case DocType.BOOKOPTEXT:
                        {
                            templatePath = _configuration.GetValue<string>("EmailTemplates:ExtendOptionDate");
                            break;
                        }
                    case DocType.GOAHEAD:
                        {
                            templatePath = _configuration.GetValue<string>("EmailTemplates:GoAheadBooking");
                            break;
                        }
                    case DocType.REMIND:
                        {
                            templatePath = _configuration.GetValue<string>("EmailTemplates:HotelReservationRequestEmail");
                            break;
                        }
                    case DocType.TESTEMAIL:
                        {
                            templatePath = _configuration.GetValue<string>("EmailTemplates:HotelReservationRequestEmail");
                            break;
                        }
                    case DocType.BOOKREQ://HOTELREQUESTS
                        {
                            templatePath = _configuration.GetValue<string>("EmailTemplates:HotelReservationRequestEmail");
                            break;
                        }
                    case DocType.SALESSUBMITQUOTE:
                        {
                            templatePath = _configuration.GetValue<string>("EmailTemplates:Submit_Quote");
                            break;
                        }
                    case DocType.COAPPROVAL:
                        {
                            templatePath = _configuration.GetValue<string>("EmailTemplates:Submit_Commercial");
                            break;
                        }
                    case DocType.COREJECT:
                        {
                            templatePath = _configuration.GetValue<string>("EmailTemplates:Reject_Commercial");
                            break;
                        }
                    case DocType.CAPAPPROVAL:
                        {
                            templatePath = _configuration.GetValue<string>("EmailTemplates:Approve_Quotation");
                            break;
                        }
                    case DocType.CAPREJECT:
                        {
                            templatePath = _configuration.GetValue<string>("EmailTemplates:Reject_Quotation");
                            break;
                        }
                    case DocType.MAILAGENTACCEPT:
                        {
                            templatePath = _configuration.GetValue<string>("EmailTemplates:AcceptSendToClient");
                            break;
                        }
                    case DocType.MAILAGENTREJECT:
                        {
                            templatePath = _configuration.GetValue<string>("EmailTemplates:SuggestSendToClient");
                            break;
                        }
                    case DocType.ACCEPTWITHOUTPROPOSAL:
                        {
                            templatePath = _configuration.GetValue<string>("EmailTemplates:AcceptWithoutProposal");
                            break;
                        }
                    case DocType.PWDRECOVER:
                        {
                            templatePath = _configuration.GetValue<string>("EmailTemplates:PasswordRecoverTemp");
                            break;
                        }
                    case DocType.QUOTEFOLLOWUP:
                        {
                            templatePath = _configuration.GetValue<string>("EmailTemplates:Quote_Followup");
                            break;
                        }
                    case DocType.ERRORREPORT:
                        {
                            templatePath = _configuration.GetValue<string>("EmailTemplates:ErrorReport");
                            break;
                        }
                    case DocType.OPSHOTELCONFIRM:
                        {
                            templatePath = _configuration.GetValue<string>("EmailTemplates:OpsHotelConfirm");
                            break;
                        }
                    case DocType.OPSROOMING:
                        {
                            templatePath = _configuration.GetValue<string>("EmailTemplates:OPSROOMING");
                            break;
                        }
                    case DocType.OPSPOSAMEND:
                        {
                            templatePath = _configuration.GetValue<string>("EmailTemplates:OPSPOSAMEND");
                            break;
                        }
                    default:
                        return string.Empty;
                }

                pathToFile = filePath + templatePath;
            }
            catch (Exception ex)
            {
                pathToFile = ex.Message;
            }
            return pathToFile;
        }

        /// <summary>
        /// Send email function
        /// </summary>
        /// <param name="emailContent">email details generated for email template creation</param>
        /// <returns>Response Status i.e. Mail sent or Not sent</returns>
        public async Task<EmailGetRes> SendEmail(EmailTemplateGetRes emailContent, string config = "", bool IsLog = false)
        {
            EmailGetRes response = new EmailGetRes() { };

            try
            {
                if (!string.IsNullOrEmpty(emailContent.Subject))
                {
                    var smtpdetails = GetSmtpCredentials(emailContent.UserEmail, config);
                    if (smtpdetails != null)
                    {
                        var emailMessage = new MimeMessage();

                        //if (!string.IsNullOrWhiteSpace(smtpdetails.DomainName))
                        //{
                        //	emailContent.From = Encrypt.DecryptData("", smtpdetails.UserName);
                        //}

                        var IsTestEnv = _configuration.GetValue<string>("IsTestEnv");

                        if (IsTestEnv == "1")
                        {
                            //emailContent.From = _configuration.GetValue<string>("FromAddress");
                            emailContent.To = _configuration.GetValue<string>("ToAddress");
                            emailContent.CC = _configuration.GetValue<string>("ToCc");
                        }
                        emailContent.BCC = _configuration.GetValue<string>("ToBcc");

                        //From
                        emailMessage.From.Add(new MailboxAddress(emailContent.From));

                        //To
                        if (!string.IsNullOrWhiteSpace(emailContent.To))
                        {
                            if (emailContent.To.Contains(";"))
                            {
                                List<string> lst = Regex.Split(emailContent.To, ";").ToList();
                                foreach (var a in lst)
                                {
                                    if (!string.IsNullOrEmpty(a))
                                    {
                                        emailMessage.To.Add(new MailboxAddress(a));
                                    }
                                }
                            }
                            else
                            {
                                emailMessage.To.Add(new MailboxAddress(emailContent.To));
                            }
                        }

                        //CC
                        if (!string.IsNullOrWhiteSpace(emailContent.CC))
                        {
                            if (emailContent.CC.Contains(";"))
                            {
                                List<string> lst = Regex.Split(emailContent.CC, ";").ToList();
                                foreach (var a in lst)
                                {
                                    if (!string.IsNullOrEmpty(a))
                                    {
                                        emailMessage.Cc.Add(new MailboxAddress(a));
                                    }
                                }
                            }
                            else
                            {
                                emailMessage.Cc.Add(new MailboxAddress(emailContent.CC));
                            }
                        }

                        //BCC
                        if (!string.IsNullOrWhiteSpace(emailContent.BCC))
                        {
                            if (emailContent.BCC.Contains(";"))
                            {
                                List<string> lst = Regex.Split(emailContent.BCC, ";").ToList();
                                foreach (var a in lst)
                                {
                                    if (!string.IsNullOrEmpty(a))
                                    {
                                        emailMessage.Bcc.Add(new MailboxAddress(a));
                                    }
                                }
                            }
                            else
                            {
                                emailMessage.Bcc.Add(new MailboxAddress(emailContent.BCC));
                            }
                        }

                        emailMessage.Subject = emailContent.Subject;

                        if (!string.IsNullOrWhiteSpace(emailContent.Importance))
                        {
                            if (emailContent.Importance.ToLower() == "high")
                            {
                                emailMessage.Importance = MessageImportance.High;
                            }
                        }

                        var bodyBuilder = new BodyBuilder();
                        bodyBuilder.HtmlBody = emailContent.Body; 

                        if (emailContent.Attachment != null && emailContent.Attachment.Count > 0)
                        {
                            string filepath = "";
                            foreach (var item in emailContent.Attachment)
                            {
                                if (!string.IsNullOrEmpty(item))
                                {
                                    if (emailContent.PathType == "sendtoclient" || emailContent.PathType == "proposalpdfpath")
                                    {
                                        filepath = Path.Combine(_configuration.GetValue<string>("ProposalPDFPath"), item);
                                        if (File.Exists(filepath))
                                        {
                                            var fileBytes = File.ReadAllBytes(filepath);
                                            bodyBuilder.Attachments.Add(item, fileBytes, new MimeKit.ContentType("application", "pdf"));
                                        }
                                        if (emailContent.DocumentPath == null)
                                        {
                                            emailContent.DocumentPath = new List<string>();
                                            emailContent.DocumentPath.Add(filepath);
                                        }
                                        else if (emailContent.DocumentPath?.Where(a => a == filepath).Count() < 0)
                                        {
                                            emailContent.DocumentPath.Add(filepath);
                                        }
                                    }
                                }
                            }
                            emailMessage.Body = bodyBuilder.ToMessageBody();
                        }
                        else
                        {
                            emailMessage.Body = bodyBuilder.ToMessageBody();

                        }

                        //Decrypt Username and Password of Mail server credentials
                        var username = Encrypt.DecryptData("", smtpdetails.UserName);
                        var password = Encrypt.DecryptData("", smtpdetails.Password);

                        //comment before commiting to development
                        //OutlookApp outlookApp = new OutlookApp();
                        //MailItem mailItem = (MailItem)outlookApp.CreateItem(OlItemType.olMailItem);

                        //mailItem.BCC = emailContent.From;
                        //mailItem.To = emailContent.To;
                        //mailItem.CC = emailContent.CC;
                        //mailItem.Subject = emailContent.Subject;
                        //mailItem.HTMLBody = emailContent.Body;
                        //mailItem.Importance = OlImportance.olImportanceHigh;    //Set a high priority to the message
                        //mailItem.Display(false);

                        #region Uncomment this section for SMTP authentication. This method is using Smtp to send mail
                        using (var client = new MailKit.Net.Smtp.SmtpClient())
                        {
                            client.CheckCertificateRevocation = false;
                            try
                            { 
                                if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
                                {
                                    await client.ConnectAsync(smtpdetails.Server, Convert.ToInt32(smtpdetails.Port), SecureSocketOptions.StartTls).ConfigureAwait(false);
                                    await client.AuthenticateAsync(username, password).ConfigureAwait(false);                                     
                                }
                                else
                                {
                                    await client.ConnectAsync(smtpdetails.Server, Convert.ToInt32(smtpdetails.Port), SecureSocketOptions.None).ConfigureAwait(false);
                                }
                                await client.SendAsync(emailMessage).ConfigureAwait(false);
                                response.EmailSentOn = DateTime.Now;
                                await client.DisconnectAsync(true).ConfigureAwait(false);
                                response.ResponseStatus.Status = "Success";
                                response.ResponseStatus.StatusMessage = "Mail sent";
                            }
                            catch (System.Exception ex)
                            { 
                                response.ResponseStatus.ErrorMessage = "Mail not sent: " + ex.Message;
                                response.ResponseStatus.ErrorMessage += " | " + emailContent.EmailGetReq?.ErrorDescription;
                                response.ResponseStatus.Status = "Error";
                                //response.ResponseStatus.Status = "Success";
                                //response.ResponseStatus.StatusMessage = "Mail sent";
                            }

                            EmailTemplateGetRes obj = new EmailTemplateGetRes()
                            {
                                To = emailContent.To,
                                From = emailContent.From,
                                BCC = emailContent.BCC,
                                CC = emailContent.CC,
                                Subject = emailContent.Subject,
                                Body = emailContent.Body,
                                UserEmail = emailContent.UserEmail,
                                SendVia = "E",
                                DocumentReference = emailContent.DocumentReference,
                                DocumentPath = emailContent.DocumentPath,
                                Client = emailContent.Client,
                                Importance = emailContent.Importance
                            };

                            response.EmailTemplateGetRes.Add(obj);
                            response.EmailTemplateGetRes[0].ResponseStatus = response.ResponseStatus;
                        }
                        #endregion 
                        if (emailContent.EmailGetReq.IsSaveDocStore == true)
                        {
                            EmailGetReq request = emailContent.EmailGetReq;
                            response.EmailTemplateGetRes[0].SendStatus = response.EmailTemplateGetRes[0].ResponseStatus?.Status?.ToLower() == "success" ? "Sent" : "Not Sent";
                            bool flag = await SaveDocumentStore(response.EmailTemplateGetRes[0], request);
                        }
                    }
                    else
                    { 
                        response.ResponseStatus.ErrorMessage = "Smtp details not found";
                        response.ResponseStatus.Status = "Error";
                    }
                }
                else
                { 
                    response.ResponseStatus.ErrorMessage = "Subject can not be null/empty.";
                    response.ResponseStatus.Status = "Error";
                }
            }

            catch (System.Exception ex)
            {
                string msg = ex.ToString();
                response.ResponseStatus.ErrorMessage = msg;
                response.ResponseStatus.Status = "Error"; 
            }
             
            return response;
        }

        /// <summary>
        /// To get smtp details from mMailServerConfiguration collection
        /// </summary>
        /// <param name="emailId"> Required field (Logged in User email Id)</param>
        /// <returns></returns>
        public mMailServerConfiguration GetSmtpCredentials(string emailId, string typeconfig = "")
        {
            try
            {
                mMailServerConfiguration config = new mMailServerConfiguration();
                if (!string.IsNullOrWhiteSpace(emailId))
                {
                    //Get details from mMailServerConfiguration
                    var dom = !string.IsNullOrWhiteSpace(emailId) && emailId.Contains('@') ? emailId.Split('@')[1] : string.Empty;
                    if (!string.IsNullOrWhiteSpace(dom))
                    {
                        config = _MongoContext.mMailServerConfiguration.AsQueryable().Where(x => x.DomainName.Contains(dom)).FirstOrDefault();
                        if (config == null || string.IsNullOrWhiteSpace(config.DomainName) || typeconfig == "default")
                        {
                            config = _MongoContext.mMailServerConfiguration.AsQueryable().Where(x => x.DomainName.Contains("coxandkings.com")).FirstOrDefault();
                        }
                        //if (dom.ToLower() == "coxandkings.ae")
                        //{
                        //	config = _MongoContext.mMailServerConfiguration.AsQueryable().Where(x => x.DomainName.Contains(dom)).FirstOrDefault();
                        //}
                        //else
                        //{
                        //	config = _MongoContext.mMailServerConfiguration.AsQueryable().Where(x => x.DomainName == string.Empty).FirstOrDefault();
                        //}
                        //else
                        //{
                        //	config = _MongoContext.mMailServerConfiguration.AsQueryable().Where(x => x.DomainName == string.Empty).FirstOrDefault();
                        //}
                    }
                }
                return config;
            }
            catch (System.Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Saving details in mDocumentStore collection
        /// </summary>
        /// <param name="response">fetching EmailContent details in response object like To,CC,From,Bcc,Body etc</param>
        /// <param name="request">Params required to create Email Template</param>
        /// <returns></returns>
        public async Task<bool> SaveDocumentStore(EmailTemplateGetRes response, EmailGetReq request)
        {
            bool flag = false;
            try
            {

                List<string> listTo = new List<string>();
                if (!string.IsNullOrEmpty(response.To) && (response.To.Contains(";") || response.To.Contains(",")))
                {
                    listTo = Regex.Split(response.To, ";").ToList();
                }
                else
                {
                    listTo.Add(response.To);
                }

                List<string> listCC = new List<string>();
                if (!string.IsNullOrEmpty(response.CC) && (response.CC.Contains(";") || response.CC.Contains(",")))
                {
                    listCC = Regex.Split(response.CC, ";").ToList();
                }
                else
                {
                    listCC.Add(response.CC);
                }

                List<string> listBCC = new List<string>();
                if (!string.IsNullOrEmpty(response.BCC) && (response.BCC.Contains(";") || response.BCC.Contains(",")))
                {
                    listBCC = Regex.Split(response.BCC, ";").ToList();
                }
                else
                {
                    listBCC.Add(response.BCC);
                }

                DocumentStoreSetReq req = new DocumentStoreSetReq();
                req.mDocumentStore = new mDocumentStore
                {
                    DocumentType = request.DocumentType,
                    BookingNumber = request.BookingNo,
                    PositionId = request.PositionId,
                    SupplierId = request.SupplierId,
                    From = response.From,
                    To = listTo,
                    CC = listCC,
                    BCC = listBCC,
                    Subject = response.Subject,
                    Body = response.Body,
                    SendDate = DateTime.Now,
                    SendStatus = response.SendStatus,
                    Create_Date = DateTime.Now,
                    Create_User = response.UserEmail,
                    DocumentReference = response.DocumentReference,
                    DocumentPath = response.DocumentPath,
                    Send_Via = response.SendVia,
                    AlternateServiceId = request.AlternateServiceId,
                    QRFID = request.QrfId,
                    ClientId = response.Client,
                    ErrorMessage = response?.ResponseStatus?.ErrorMessage,
                    QRFPriceId = request.QRFPriceId,
                    MailStatus = request.MailStatus,
                    SystemCompany_Id = request.SystemCompany_Id,
                    VoyagerUser_Id = request.PlacerUserId,
                    Importance = response.Importance
                };
                req.Document_Id = response.Document_Id;
                DocumentStoreSetRes res = await _documentStoreRepository.SetDocumentStore(req);
                if (res.ResponseStatus.Status.ToLower() == "success") return flag = true; else return flag = false;
            }
            catch (Exception ex)
            {
                response.ResponseStatus.ErrorMessage = "Email details not stored in document store collection" + " " + ex.Message;
                response.ResponseStatus.Status = "Error";
            }
            return flag;
        }

        #endregion

        #region Email Template Methods

        #region Bookings Module
        //Hotel Booking Cancellation
        public async Task<List<EmailTemplateGetRes>> CreateBookXXEmailTemplate(EmailGetReq request, string pathToFile)
        {
            List<EmailTemplateGetRes> lstResponse = new List<EmailTemplateGetRes>();
            EmailTemplateGetRes response = new EmailTemplateGetRes();
            try
            {
                var builder = new StringBuilder();
                using (StreamReader SourceReader = File.OpenText(pathToFile))
                {
                    builder.Append(SourceReader.ReadToEnd());
                }

                if (!string.IsNullOrEmpty(request.BookingNo))
                {
                    var resBooking = await _MongoContext.Bookings.FindAsync(a => a.BookingNumber == request.BookingNo).Result.FirstOrDefaultAsync();
                    if (resBooking != null)
                    {
                        string RoomingDetails = "";
                        var position = resBooking.Positions.Where(a => a.Position_Id == request.PositionId).FirstOrDefault();
                        var ProductSrp = _MongoContext.mProducts_Lite.AsQueryable().Where(a => a.VoyagerProduct_Id == position.Product_Id).FirstOrDefault();
                        var supplierEmail = position.SupplierInfo.Contact_Email;
                        var mUser = await _MongoContext.mUsers.FindAsync(a => a.VoyagerUser_Id == request.PlacerUserId).Result.FirstOrDefaultAsync();
                        var mCompany = await _MongoContext.mCompanies.FindAsync(a => a.Company_Id == mUser.Company_Id).Result.FirstOrDefaultAsync();
                        var resContact = mCompany.ContactDetails.Where(a => a.Contact_Id == mUser.Contact_Id).FirstOrDefault();
                        foreach (var Room in position.BookingRoomsAndPrices)
                        {
                            if (Room != null && !string.IsNullOrEmpty(Room.RoomName) && !string.IsNullOrEmpty(Room.PersonType))
                            {
                                if (RoomingDetails != "") RoomingDetails += "+ ";
                                if (Room.RoomName.ToLower() == "child")
                                {
                                    RoomingDetails += (Room.Req_Count + " (" + Room.PersonType + ")");
                                }
                                else
                                {
                                    RoomingDetails += (Room.Req_Count + " " + Room.RoomName);
                                }
                            }
                        }

                        if (position != null && resContact != null)
                        {
                            int days = Convert.ToInt32(position.DURATION);
                            days = days + 1;
                            #region replace email content
                            builder.Replace("[#Product_Supplier_Contact_Details_for_Groups]", position.SupplierInfo.Contact_Name);
                            builder.Replace("[#Hotel_Name]", ProductSrp.ProdName);
                            builder.Replace("[#Hotel_Address_Line_1]", !string.IsNullOrWhiteSpace(ProductSrp.Address) ? (ProductSrp.Address + ",<br/>") : "");
                            builder.Replace("[#Hotel_City]", ProductSrp.CityName);
                            builder.Replace("[#Hotel_Country]", ProductSrp.CountryName);
                            builder.Replace("[#BookingReferenceNumber]", resBooking.BookingNumber);
                            builder.Replace("[#ClientTourName]", resBooking.CustRef);
                            builder.Replace("[#TourStartDate]", Convert.ToDateTime(position.STARTDATE).ToString("dd-MMM-yyyy"));
                            builder.Replace("[#TourEndDate]", Convert.ToDateTime(position.ENDDATE).ToString("dd-MMM-yyyy"));
                            builder.Replace("[#TourDays]", days.ToString());
                            builder.Replace("[#TourNights]", position.DURATION);
                            builder.Replace("[#TourLevelRooms]", RoomingDetails);
                            builder.Replace("[#Placer_Name]", resContact.FIRSTNAME + " " + resContact.LastNAME);
                            builder.Replace("[#Placer_Contact_Number]", resContact.TEL);
                            builder.Replace("[#Placer_Email]", resContact.MAIL);
                            #endregion

                            string hotelplaceremail = "";
                            if (!string.IsNullOrWhiteSpace(position.HotelPLacer_ID))
                            {
                                //List<CompanyContacts> comp = new List<CompanyContacts>();
                                //var contacts = _MongoContext.mCompanies.AsQueryable().Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a=>a.Contact_Id== position.HotelPLacer_ID)).Select(x=>x.ContactDetails).ToList();
                                //contacts.ForEach(y => comp.AddRange(y));

                                var contacts = _MongoContext.mCompanies.AsQueryable().Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a => a.Contact_Id == position.HotelPLacer_ID)).FirstOrDefault().ContactDetails;
                                hotelplaceremail = contacts.Where(a => a.Contact_Id == position.HotelPLacer_ID).FirstOrDefault().MAIL;
                                if (string.IsNullOrWhiteSpace(hotelplaceremail))
                                    hotelplaceremail = resBooking.StaffDetails.Staff_OpsUser_Email;
                            }
                            else
                                hotelplaceremail = resBooking.StaffDetails.Staff_OpsUser_Email;

                            var email = GetSmtpCredentials(request.UserEmail)?.UserName;
                            response.From = Encrypt.DecryptData("", email);
                            response.To = supplierEmail;
                            response.CC = request.UserEmail + ";" + hotelplaceremail;
                            response.Body = builder.ToString();
                            response.Subject = resBooking.BookingNumber + " / " + position.City + " – Cancellation";
                            response.EmailGetReq.PositionId = request?.PositionId;
                            response.SupplierId = position?.SupplierInfo?.Id;

                            response.ResponseStatus.Status = "Success";
                            response.ResponseStatus.ErrorMessage = "Mail Template Created Successfully.";
                        }
                        else
                        {
                            response.ResponseStatus.Status = "Error";
                            response.ResponseStatus.ErrorMessage = "Position Details Not Found";
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Error";
                        response.ResponseStatus.ErrorMessage = "Booking details not found";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Error";
                    response.ResponseStatus.ErrorMessage = "Booking number not found";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Error";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }

            lstResponse.Add(response);
            lstResponse[0].ResponseStatus = response.ResponseStatus;
            return lstResponse;
        }

        //Multiple Hotel Booking Cancellation
        public async Task<List<EmailTemplateGetRes>> CreateMultipleBookXXEmailTemplate(EmailGetReq request, string pathToFile)
        {
            try
            {
                List<EmailTemplateGetRes> lstResponse = new List<EmailTemplateGetRes>();
                EmailTemplateGetRes response = new EmailTemplateGetRes() { ResponseStatus = new ResponseStatus() };
                if (!string.IsNullOrEmpty(request.BookingNo))
                {
                    var resBooking = await _MongoContext.Bookings.FindAsync(a => a.BookingNumber == request.BookingNo).Result.FirstOrDefaultAsync();
                    if (resBooking != null)
                    {
                        var posIds = request.PositionIds;
                        var positions = resBooking.Positions.Where(a => posIds.Contains(a.Position_Id)).ToList();
                        var mUser = await _MongoContext.mUsers.FindAsync(a => a.VoyagerUser_Id == request.PlacerUserId).Result.FirstOrDefaultAsync();
                        var mCompany = await _MongoContext.mCompanies.FindAsync(a => a.Company_Id == mUser.Company_Id).Result.FirstOrDefaultAsync();
                        var resContact = mCompany.ContactDetails.Where(a => a.Contact_Id == mUser.Contact_Id).FirstOrDefault();
                        int counter = 0;

                        foreach (var pos in positions)
                        {
                            var builder = new StringBuilder();
                            string RoomingDetails = "";

                            using (StreamReader SourceReader = File.OpenText(pathToFile))
                            {
                                builder.Append(SourceReader.ReadToEnd());
                            }

                            response = new EmailTemplateGetRes();
                            var ProductSrp = _MongoContext.mProducts_Lite.AsQueryable().Where(a => a.VoyagerProduct_Id == pos.Product_Id).FirstOrDefault();
                            var supplierEmail = pos.SupplierInfo.Contact_Email;

                            foreach (var Room in pos.BookingRoomsAndPrices)
                            {
                                if (Room != null && !string.IsNullOrEmpty(Room.RoomName) && !string.IsNullOrEmpty(Room.PersonType))
                                {
                                    if (RoomingDetails != "") RoomingDetails += "+ ";
                                    if (Room.RoomName.ToLower() == "child")
                                    {
                                        RoomingDetails += (Room.Req_Count + " (" + Room.PersonType + ")");
                                    }
                                    else
                                    {
                                        RoomingDetails += (Room.Req_Count + " " + Room.RoomName);
                                    }
                                }
                            }

                            if (pos != null && resContact != null)
                            {
                                int days = Convert.ToInt32(pos.DURATION);
                                days = days + 1;
                                #region replace email content
                                builder.Replace("[#Product_Supplier_Contact_Details_for_Groups]", pos.SupplierInfo.Contact_Name);
                                builder.Replace("[#Hotel_Name]", ProductSrp.ProdName);
                                builder.Replace("[#Hotel_Address_Line_1]", ProductSrp.Address);
                                builder.Replace("[#Hotel_City]", ProductSrp.CityName);
                                builder.Replace("[#Hotel_Country]", ProductSrp.CountryName);
                                builder.Replace("[#BookingReferenceNumber]", resBooking.BookingNumber);
                                builder.Replace("[#ClientTourName]", resBooking.CustRef);
                                builder.Replace("[#TourStartDate]", Convert.ToDateTime(pos.STARTDATE).ToString("dd-MMM-yyyy"));
                                builder.Replace("[#TourEndDate]", Convert.ToDateTime(pos.ENDDATE).ToString("dd-MMM-yyyy"));
                                builder.Replace("[#TourDays]", days.ToString());
                                builder.Replace("[#TourNights]", pos.DURATION);
                                builder.Replace("[#TourLevelRooms]", RoomingDetails);
                                builder.Replace("[#Placer_Name]", resContact.FIRSTNAME + " " + resContact.LastNAME);
                                builder.Replace("[#Placer_Contact_Number]", resContact.TEL);
                                builder.Replace("[#Placer_Email]", resContact.MAIL);
                                #endregion

                                string hotelplaceremail = "";
                                if (!string.IsNullOrWhiteSpace(pos.HotelPLacer_ID))
                                {
                                    //List<CompanyContacts> comp = new List<CompanyContacts>();
                                    //var contacts = _MongoContext.mCompanies.AsQueryable().Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a=>a.Contact_Id== position.HotelPLacer_ID)).Select(x=>x.ContactDetails).ToList();
                                    //contacts.ForEach(y => comp.AddRange(y));

                                    var contacts = _MongoContext.mCompanies.AsQueryable().Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a => a.Contact_Id == pos.HotelPLacer_ID)).FirstOrDefault().ContactDetails;
                                    hotelplaceremail = contacts.Where(a => a.Contact_Id == pos.HotelPLacer_ID).FirstOrDefault().MAIL;
                                    if (string.IsNullOrWhiteSpace(hotelplaceremail))
                                        hotelplaceremail = resBooking.StaffDetails.Staff_OpsUser_Email;
                                }
                                else
                                    hotelplaceremail = resBooking.StaffDetails.Staff_OpsUser_Email;

                                var email = GetSmtpCredentials(request.UserEmail)?.UserName;
                                response.From = Encrypt.DecryptData("", email);
                                response.To = supplierEmail;
                                response.CC = request.UserEmail + ";" + hotelplaceremail;
                                response.Body = builder.ToString();
                                response.Subject = resBooking.BookingNumber + " / " + pos.City + " – Cancellation";
                                response.EmailGetReq.PositionId = pos?.Position_Id;
                                response.SupplierId = pos?.SupplierInfo?.Id;

                                response.ResponseStatus.Status = "Success";
                                response.ResponseStatus.ErrorMessage = "Mail Template Created Successfully.";
                            }
                            else
                            {
                                response.ResponseStatus.Status = "Error";
                                response.ResponseStatus.ErrorMessage = "Position Details Not Found";
                            }

                            lstResponse.Add(response);
                            lstResponse[counter].ResponseStatus = response.ResponseStatus;
                            counter = counter + 1;
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Error";
                        response.ResponseStatus.ErrorMessage = "Booking details not found";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Error";
                    response.ResponseStatus.ErrorMessage = "Booking number not found";
                }

                return lstResponse;
            }
            catch (System.Exception ex)
            {
                throw;
            }
        }

        //Hotel Booking Confirmation
        public async Task<List<EmailTemplateGetRes>> CreateBookKKEmailTemplate(EmailGetReq request, string pathToFile)
        {
            List<EmailTemplateGetRes> lstResponse = new List<EmailTemplateGetRes>();
            EmailTemplateGetRes response = new EmailTemplateGetRes();
            try
            {
                var builder = new StringBuilder();

                using (StreamReader SourceReader = File.OpenText(pathToFile))
                {
                    builder.Append(SourceReader.ReadToEnd());
                }

                if (!string.IsNullOrEmpty(request.BookingNo))
                {
                    var resBooking = await _MongoContext.Bookings.FindAsync(a => a.BookingNumber == request.BookingNo).Result.FirstOrDefaultAsync();
                    if (resBooking != null)
                    {
                        var position = resBooking.Positions.Where(a => a.Position_Id == request.PositionId).FirstOrDefault();
                        var ProductSrp = _MongoContext.mProducts_Lite.AsQueryable().Where(a => a.VoyagerProduct_Id == position.Product_Id).FirstOrDefault();
                        var mUser = await _MongoContext.mUsers.FindAsync(a => a.VoyagerUser_Id == request.PlacerUserId).Result.FirstOrDefaultAsync();
                        var mCompany = await _MongoContext.mCompanies.FindAsync(a => a.Company_Id == mUser.Company_Id).Result.FirstOrDefaultAsync();
                        var resContact = mCompany.ContactDetails.Where(a => a.Contact_Id == mUser.Contact_Id).FirstOrDefault();
                        var supplierEmail = position.SupplierInfo.Contact_Email;

                        if (position != null && resContact != null)
                        {
                            int days = Convert.ToInt32(position.DURATION);
                            days = days + 1;
                            #region replace email content
                            builder.Replace("{{Product_Supplier_Contact_Details_for_Groups}}", position.SupplierInfo.Contact_Name);
                            builder.Replace("{{Hotel_Name}}", ProductSrp.ProdName);
                            builder.Replace("{{Hotel_Address_Line_1}}", string.IsNullOrWhiteSpace(ProductSrp.Address) ? "" : (ProductSrp.Address + ",</br>"));
                            builder.Replace("{{Hotel_City}}", ProductSrp.CityName);
                            builder.Replace("{{Hotel_Country}}", ProductSrp.CountryName);
                            builder.Replace("{{Our_Booking_Reference_Number}}", resBooking.BookingNumber);
                            builder.Replace("{{Client_Tour_Name}}", resBooking.CustRef);
                            builder.Replace("{{Check_In_Date}}", Convert.ToDateTime(position.STARTDATE).ToString("dd-MMM-yyyy"));
                            builder.Replace("{{Check_Out_Date}}", Convert.ToDateTime(position.ENDDATE).ToString("dd-MMM-yyyy"));
                            builder.Replace("{{Position_Duration}}", position.DURATION);
                            builder.Replace("{{Placer_Name}}", resContact.FIRSTNAME + " " + resContact.LastNAME);
                            builder.Replace("{{Placer_Contact_Number}}", resContact.TEL);
                            builder.Replace("{{Placer_Email}}", resContact.MAIL);
                            builder.Replace("{{Nationality}}", resBooking.GuestDetails.Nationality_Name);
                            builder.Replace("{{Breakfast_Type}}", position.BreakFastType);

                            var rooms = new StringBuilder();
                            if (position.BookingRoomsAndPrices != null && position.BookingRoomsAndPrices.Count > 0)
                            {
                                for (int a = 0; a < position.BookingRoomsAndPrices.Count; a++)
                                {
                                    var persontype = position.BookingRoomsAndPrices[a].PersonType; if (!string.IsNullOrWhiteSpace(persontype)) { persontype = "(" + persontype + ")"; } else { persontype = ""; }
                                    var categoryname = (position.BookingRoomsAndPrices[a].CategoryName == null ? "" : position.BookingRoomsAndPrices[a].CategoryName.ToUpper()) + " " + position.BookingRoomsAndPrices[a].RoomName + " " + persontype;
                                    rooms.Append("<tr>");
                                    rooms.Append("<td>" + categoryname + "</td>");
                                    rooms.Append("<td>" + position.BookingRoomsAndPrices[a].Req_Count + "</td>");
                                    rooms.Append("<td>" + position.BookingRoomsAndPrices[a].BuyPrice + "(" + position.BookingRoomsAndPrices[a].BuyCurrency_Name + ")" + "</td>");
                                    rooms.Append("<td>" + position.HOTELMEALPLAN + "</td>");
                                    if (a == 0)
                                    {
                                        var interconnectrooms = position.InterConnectingRooms != null ? "InterConnecting Rooms : " + position.InterConnectingRooms + "<br>" : "";
                                        var washchngeroom = (position.WashChangeRoom != null && position.WashChangeRoom > 0) ? "Wash and Change Rooms : " + position.WashChangeRoom + "<br>" : "";
                                        var latecheckout = position.LateCheckout != null ? "<br> Late Check out : " + position.LateCheckout : "";
                                        rooms.Append("<td rowspan=" + position.BookingRoomsAndPrices.Count + ">" + interconnectrooms + washchngeroom + latecheckout + "<br></td>");
                                        rooms.Append("<td rowspan=" + position.BookingRoomsAndPrices.Count + ">" + position.Special_Requests + "</td>");
                                    }
                                    rooms.Append("</tr>");
                                }
                            }
                            else
                            {
                                rooms.Append("<tr>");
                                rooms.Append("<td></td>");
                                rooms.Append("<td></td>");
                                rooms.Append("<td></td>");
                                rooms.Append("<td></td>");
                                rooms.Append("<td></td>");
                                rooms.Append("<td></td>");
                                rooms.Append("</tr>");
                            }
                            builder.Replace("{{BookingRooms}}", rooms.ToString());
                            #endregion

                            var email = GetSmtpCredentials(request.UserEmail)?.UserName;
                            string configMail = Encrypt.DecryptData("", email);
                            if (request.Module.ToLower() == "ops")
                            {
                                string centralMailBox = "";
                                var contactMails = mCompany.ContactDetails?.Where(a => a.IsCentralEmail == true && !string.IsNullOrWhiteSpace(a.MAIL)).Select(a => a.MAIL).ToList();
                                if (contactMails?.Count > 0)
                                {
                                    centralMailBox = string.Join(";", contactMails);
                                }

                                response.From = resBooking.StaffDetails.Staff_OpsUser_Email;
                                response.CC = !string.IsNullOrWhiteSpace(centralMailBox) ? centralMailBox + ";" + configMail : configMail;
                                response.Subject = "BOOKING CONFIRMATION - " + resBooking.BookingNumber + " - " + resBooking.CustRef + " – ("
                                                    + Convert.ToDateTime(position.STARTDATE).ToString("dd-MMM-yyyy") + " - " + Convert.ToDateTime(position.ENDDATE).ToString("dd-MMM-yyyy") + ")";
                            }
                            else
                            {
                                string hotelplaceremail = "";
                                if (!string.IsNullOrWhiteSpace(position.HotelPLacer_ID))
                                {
                                    var contacts = _MongoContext.mCompanies.AsQueryable().Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a => a.Contact_Id == position.HotelPLacer_ID)).FirstOrDefault().ContactDetails;
                                    hotelplaceremail = contacts.Where(a => a.Contact_Id == position.HotelPLacer_ID).FirstOrDefault().MAIL;
                                    if (string.IsNullOrWhiteSpace(hotelplaceremail))
                                        hotelplaceremail = resBooking.StaffDetails.Staff_OpsUser_Email;
                                }
                                else
                                    hotelplaceremail = resBooking.StaffDetails.Staff_OpsUser_Email;

                                response.From = configMail;
                                response.CC = request.UserEmail + ";" + hotelplaceremail;
                                response.Subject = resBooking.BookingNumber + " / " + position.City + " – Confirmation";
                            }

                            response.To = supplierEmail;
                            response.Body = builder.ToString();
                            response.SupplierId = string.IsNullOrWhiteSpace(request.SupplierId) ? position.SupplierInfo?.Id : request.SupplierId;
                            response.EmailGetReq.PositionId = position?.Position_Id;

                            response.ResponseStatus.Status = "Success";
                            response.ResponseStatus.ErrorMessage = "Mail Template Created Successfully.";
                        }
                        else
                        {
                            response.ResponseStatus.Status = "Error";
                            response.ResponseStatus.ErrorMessage = "Position Details Not Found";
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Error";
                        response.ResponseStatus.ErrorMessage = "Booking details not found";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Error";
                    response.ResponseStatus.ErrorMessage = "Booking number can not be null/empty.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Error";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }
            lstResponse.Add(response);
            lstResponse[0].ResponseStatus = response.ResponseStatus;
            return lstResponse;
        }

        //Hotel Booking Provisional
        public async Task<List<EmailTemplateGetRes>> CreateBOOKPROVEmailTemplate(EmailGetReq request, string pathToFile)
        {
            List<EmailTemplateGetRes> lstResponse = new List<EmailTemplateGetRes>();
            EmailTemplateGetRes response = new EmailTemplateGetRes();
            try
            {
                var builder = new StringBuilder();
                using (StreamReader SourceReader = File.OpenText(pathToFile))
                {
                    builder.Append(SourceReader.ReadToEnd());
                }

                if (!string.IsNullOrEmpty(request.BookingNo))
                {
                    var resBooking = await _MongoContext.Bookings.FindAsync(a => a.BookingNumber == request.BookingNo).Result.FirstOrDefaultAsync();
                    if (resBooking != null)
                    {
                        var position = resBooking.Positions.Where(a => a.Position_Id == request.PositionId).FirstOrDefault();
                        var ProductSrp = _MongoContext.mProducts_Lite.AsQueryable().Where(a => a.VoyagerProduct_Id == position.Product_Id).FirstOrDefault();
                        var mUser = await _MongoContext.mUsers.FindAsync(a => a.VoyagerUser_Id == request.PlacerUserId).Result.FirstOrDefaultAsync();
                        var mCompany = await _MongoContext.mCompanies.FindAsync(a => a.Company_Id == mUser.Company_Id).Result.FirstOrDefaultAsync();
                        var resContact = mCompany.ContactDetails.Where(a => a.Contact_Id == mUser.Contact_Id).FirstOrDefault();
                        var supplierEmail = position.SupplierInfo.Contact_Email;

                        if (position != null && resContact != null)
                        {
                            int days = Convert.ToInt32(position.DURATION);
                            days = days + 1;
                            #region replace email content
                            builder.Replace("{{Product_Supplier_Contact_Details_for_Groups}}", position.SupplierInfo.Contact_Name);
                            builder.Replace("{{Hotel_Name}}", ProductSrp.ProdName);
                            builder.Replace("{{Hotel_Address_Line_1}}", ProductSrp.Address);
                            builder.Replace("{{Hotel_City}}", ProductSrp.CityName);
                            builder.Replace("{{Hotel_Country}}", ProductSrp.CountryName);
                            builder.Replace("{{Our_Booking_Reference_Number}}", resBooking.BookingNumber);
                            builder.Replace("{{Client_Tour_Name}}", resBooking.CustRef);
                            builder.Replace("{{Check_In_Date}}", Convert.ToDateTime(position.STARTDATE).ToString("dd-MMM-yyyy"));
                            builder.Replace("{{Check_Out_Date}}", Convert.ToDateTime(position.ENDDATE).ToString("dd-MMM-yyyy"));
                            builder.Replace("{{Position_Duration}}", position.DURATION);
                            builder.Replace("{{PositionOptionDate}}", position.OPTIONDATE != null ? Convert.ToDateTime(position.OPTIONDATE).ToString("dd-MMM-yyyy") : string.Empty);
                            builder.Replace("{{Placer_Name}}", resContact.FIRSTNAME + " " + resContact.LastNAME);
                            builder.Replace("{{Placer_Contact_Number}}", resContact.TEL);
                            builder.Replace("{{Placer_Email}}", resContact.MAIL);

                            var rooms = new StringBuilder();
                            if (position.BookingRoomsAndPrices != null && position.BookingRoomsAndPrices.Count > 0)
                            {
                                for (int a = 0; a < position.BookingRoomsAndPrices.Count; a++)
                                {
                                    var persontype = position.BookingRoomsAndPrices[a].PersonType; if (!string.IsNullOrWhiteSpace(persontype)) { persontype = "(" + persontype + ")"; } else { persontype = ""; }
                                    var categoryname = (position.BookingRoomsAndPrices[a].CategoryName == null ? "" : position.BookingRoomsAndPrices[a].CategoryName.ToUpper()) + " " + position.BookingRoomsAndPrices[a].RoomName + " " + persontype;
                                    rooms.Append("<tr>");
                                    rooms.Append("<td>" + categoryname + "</td>");
                                    rooms.Append("<td>" + position.BookingRoomsAndPrices[a].Req_Count + "</td>");
                                    rooms.Append("<td>" + position.BookingRoomsAndPrices[a].BuyPrice + "(" + position.BookingRoomsAndPrices[a].BuyCurrency_Name + ")" + "</td>");
                                    rooms.Append("<td>" + position.HOTELMEALPLAN + "</td>");
                                    if (a == 0)
                                    {
                                        var interconnectrooms = position.InterConnectingRooms != null ? "InterConnecting Rooms : " + position.InterConnectingRooms + "<br>" : "";
                                        var washchngeroom = (position.WashChangeRoom != null && position.WashChangeRoom > 0) ? "Wash and Change Rooms : " + position.WashChangeRoom + "<br>" : "";
                                        var latecheckout = position.LateCheckout != null ? "Late Check out : " + position.LateCheckout : "";
                                        rooms.Append("<td rowspan=" + position.BookingRoomsAndPrices.Count + ">" + interconnectrooms + washchngeroom + latecheckout + "<br></td>");
                                        rooms.Append("<td rowspan=" + position.BookingRoomsAndPrices.Count + ">" + position.Special_Requests + "</td>");
                                    }
                                    rooms.Append("</tr>");
                                }
                            }
                            else
                            {
                                rooms.Append("<tr>");
                                rooms.Append("<td></td>");
                                rooms.Append("<td></td>");
                                rooms.Append("<td></td>");
                                rooms.Append("<td></td>");
                                rooms.Append("<td></td>");
                                rooms.Append("<td></td>");
                                rooms.Append("</tr>");
                            }
                            builder.Replace("{{BookingRooms}}", rooms.ToString());
                            #endregion

                            string hotelplaceremail = "";
                            if (!string.IsNullOrWhiteSpace(position.HotelPLacer_ID))
                            {
                                var contacts = _MongoContext.mCompanies.AsQueryable().Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a => a.Contact_Id == position.HotelPLacer_ID)).FirstOrDefault().ContactDetails;
                                hotelplaceremail = contacts.Where(a => a.Contact_Id == position.HotelPLacer_ID).FirstOrDefault().MAIL;
                                if (string.IsNullOrWhiteSpace(hotelplaceremail))
                                    hotelplaceremail = resBooking.StaffDetails.Staff_OpsUser_Email;
                            }
                            else
                                hotelplaceremail = resBooking.StaffDetails.Staff_OpsUser_Email;

                            var email = GetSmtpCredentials(request.UserEmail)?.UserName;
                            response.From = Encrypt.DecryptData("", email);
                            response.To = supplierEmail;
                            response.CC = request.UserEmail + ";" + hotelplaceremail;
                            response.Body = builder.ToString();
                            response.Subject = resBooking.BookingNumber + " / " + position.City + " – Provisional";
                            response.EmailGetReq.PositionId = position?.Position_Id;

                            response.ResponseStatus.Status = "Success";
                            response.ResponseStatus.ErrorMessage = "Mail Template Created Successfully.";
                        }
                        else
                        {
                            response.ResponseStatus.Status = "Error";
                            response.ResponseStatus.ErrorMessage = "Position Details Not Found";
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Error";
                        response.ResponseStatus.ErrorMessage = "Booking details not found";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Error";
                    response.ResponseStatus.ErrorMessage = "Booking number not found";
                }

            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Error";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }
            lstResponse.Add(response);
            lstResponse[0].ResponseStatus = response.ResponseStatus;
            return lstResponse;
        }

        //Hotel Provisional Booking Cancellation
        public async Task<List<EmailTemplateGetRes>> CreateBOOKOPTXXEmailTemplate(EmailGetReq request, string pathToFile)
        {
            List<EmailTemplateGetRes> lstResponse = new List<EmailTemplateGetRes>();
            EmailTemplateGetRes response = new EmailTemplateGetRes();
            try
            {
                var builder = new StringBuilder();
                using (StreamReader SourceReader = File.OpenText(pathToFile))
                {
                    builder.Append(SourceReader.ReadToEnd());
                }

                if (!string.IsNullOrEmpty(request.BookingNo))
                {
                    var resBooking = await _MongoContext.Bookings.FindAsync(a => a.BookingNumber == request.BookingNo).Result.FirstOrDefaultAsync();
                    if (resBooking != null)
                    {
                        string RoomingDetails = "";
                        var position = resBooking.Positions.Where(a => a.Position_Id == request.PositionId).FirstOrDefault();
                        var supplierEmail = position.SupplierInfo.Contact_Email;
                        var mUser = await _MongoContext.mUsers.FindAsync(a => a.VoyagerUser_Id == request.PlacerUserId).Result.FirstOrDefaultAsync();
                        var mCompany = await _MongoContext.mCompanies.FindAsync(a => a.Company_Id == mUser.Company_Id).Result.FirstOrDefaultAsync();
                        var resContact = mCompany.ContactDetails.Where(a => a.Contact_Id == mUser.Contact_Id).FirstOrDefault();
                        foreach (var Room in position.BookingRoomsAndPrices)
                        {
                            if (Room != null && !string.IsNullOrEmpty(Room.RoomName) && !string.IsNullOrEmpty(Room.PersonType))
                            {
                                if (RoomingDetails != "") RoomingDetails += "+ ";
                                if (Room.RoomName.ToLower() == "child")
                                {
                                    RoomingDetails += (Room.Req_Count + " (" + Room.PersonType + ")");
                                }
                                else
                                {
                                    RoomingDetails += (Room.Req_Count + " " + Room.RoomName);
                                }
                            }
                        }

                        if (position != null && resContact != null)
                        {
                            int days = Convert.ToInt32(position.DURATION);
                            days = days + 1;
                            #region replace email content
                            builder.Replace("{{Our_Booking_Reference_Number}}", resBooking.BookingNumber);
                            builder.Replace("{{Client_Tour_Name}}", resBooking.CustRef);
                            builder.Replace("{{TourStartDate}}", Convert.ToDateTime(position.STARTDATE).ToString("dd-MMM-yyyy"));
                            builder.Replace("{{TourEndDate}}", Convert.ToDateTime(position.ENDDATE).ToString("dd-MMM-yyyy"));
                            builder.Replace("{{TourDays}}", days.ToString());
                            builder.Replace("{{TourNights}}", position.DURATION);
                            builder.Replace("{{TourLevelRooms}}", RoomingDetails);
                            builder.Replace("{{Placer_Name}}", resContact.FIRSTNAME + " " + resContact.LastNAME);
                            builder.Replace("{{Placer_Contact_Number}}", resContact.TEL);
                            builder.Replace("{{Placer_Email}}", resContact.MAIL);
                            #endregion

                            string hotelplaceremail = "";
                            if (!string.IsNullOrWhiteSpace(position.HotelPLacer_ID))
                            {
                                var contacts = _MongoContext.mCompanies.AsQueryable().Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a => a.Contact_Id == position.HotelPLacer_ID)).FirstOrDefault().ContactDetails;
                                hotelplaceremail = contacts.Where(a => a.Contact_Id == position.HotelPLacer_ID).FirstOrDefault().MAIL;
                                if (string.IsNullOrWhiteSpace(hotelplaceremail))
                                    hotelplaceremail = resBooking.StaffDetails.Staff_OpsUser_Email;
                            }
                            else
                                hotelplaceremail = resBooking.StaffDetails.Staff_OpsUser_Email;

                            var email = GetSmtpCredentials(request.UserEmail)?.UserName;
                            response.From = Encrypt.DecryptData("", email);
                            response.To = supplierEmail;
                            response.CC = request.UserEmail + ";" + hotelplaceremail;
                            response.Body = builder.ToString();
                            response.Subject = resBooking.BookingNumber + " / " + position.City + " – Provisional Booking Cancellation";
                            response.EmailGetReq.PositionId = position?.Position_Id;

                            response.ResponseStatus.Status = "Success";
                            response.ResponseStatus.ErrorMessage = "Mail Template Created Successfully.";
                        }
                        else
                        {
                            response.ResponseStatus.Status = "Error";
                            response.ResponseStatus.ErrorMessage = "Position Details Not Found";
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Error";
                        response.ResponseStatus.ErrorMessage = "Booking details not found";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Error";
                    response.ResponseStatus.ErrorMessage = "Booking number not found";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Error";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }
            lstResponse.Add(response);
            lstResponse[0].ResponseStatus = response.ResponseStatus;
            return lstResponse;
        }

        //Send Hotel Available Email 
        public async Task<List<EmailTemplateGetRes>> CreateHotelAvailableEmailTemplate(EmailGetReq request, string pathToFile)
        {
            List<EmailTemplateGetRes> lstResponse = new List<EmailTemplateGetRes>();
            EmailTemplateGetRes response = new EmailTemplateGetRes();
            try
            {
                StringBuilder builder = new StringBuilder();

                var resBooking = await _MongoContext.Bookings.FindAsync(a => a.BookingNumber == request.BookingNo).Result.FirstOrDefaultAsync();
                if (resBooking != null)
                {
                    var position = resBooking.Positions.Where(a => a.Position_Id == request.PositionId).FirstOrDefault();

                    if (position != null && position.AlternateServices.Count > 0)
                    {
                        var AltSvc = position.AlternateServices.Where(a => a.AlternateServies_Id == request.AlternateServiceId).FirstOrDefault();
                        if (AltSvc != null && AltSvc.SupplierInfo != null)
                        {
                            var ProductSrp = _MongoContext.mProducts_Lite.AsQueryable().Where(a => a.VoyagerProduct_Id == AltSvc.Product_Id).FirstOrDefault();
                            string strFromEmail = AltSvc.SupplierInfo.Contact_Email;
                            if (!string.IsNullOrEmpty(strFromEmail))
                            {
                                using (StreamReader SourceReader = File.OpenText(pathToFile))
                                {
                                    builder.Append(SourceReader.ReadToEnd());
                                }
                                builder.Replace("{{CUSTOMER_NAME}}", AltSvc.SupplierInfo.Contact_Name);
                                builder.Replace("{{HOTEL_NAME}}", ProductSrp.ProdName);
                                builder.Replace("{{HOTEL_ADDRESS}}", ProductSrp.Address);
                                builder.Replace("{{HOTEL_CITY}}", ProductSrp.CityName);
                                builder.Replace("{{HOTEL_COUNTRY}}", ProductSrp.CountryName);

                                builder.Replace("{{Booking_Number}}", resBooking.BookingNumber);
                                builder.Replace("{{Tour_Name}}", resBooking.CustRef);
                                builder.Replace("{{Check_In_Date}}", Convert.ToDateTime(position.STARTDATE).ToString("dd-MMM-yyyy"));
                                builder.Replace("{{Check_Out_Date}}", Convert.ToDateTime(position.ENDDATE).ToString("dd-MMM-yyyy"));
                                builder.Replace("{{No_Of_Nights}}", position.DURATION);
                                builder.Replace("{{Option_Date}}", (position.OPTIONDATE != null ? Convert.ToDateTime(position.OPTIONDATE).ToString("dd-MMM-yyyy") : ""));
                                builder.Replace("{{Supplier_CompanyName}}", AltSvc.SupplierInfo.Name);
                                builder.Replace("{{Email}}", AltSvc.SupplierInfo.Contact_Email);
                                builder.Replace("{{Phone}}", AltSvc.SupplierInfo.Contact_Tel);

                                var Rooms = new StringBuilder();
                                if (AltSvc.Request_RoomsAndPrices != null && AltSvc.Request_RoomsAndPrices.Count > 0)
                                {
                                    foreach (var Room in AltSvc.Request_RoomsAndPrices)
                                    {
                                        Rooms.Append("<tr>");
                                        Rooms.Append("<td>" + Room.RoomName + " for " + Room.PersonType + "</td>");
                                        Rooms.Append("<td>" + Room.Req_Count + "</td>");
                                        Rooms.Append("<td>" + Room.BuyCurrency_Name + " " + Room.BuyPrice + "</td>");
                                        Rooms.Append("</tr>");
                                    }
                                }
                                else
                                {
                                    Rooms.Append("<tr>");
                                    Rooms.Append("<td colspan='3'>No data found</td>");
                                    Rooms.Append("</tr>");
                                }

                                builder.Replace("{{BookingRooms}}", Rooms.ToString());

                                string hotelplaceremail = "";
                                if (!string.IsNullOrWhiteSpace(position.HotelPLacer_ID))
                                {
                                    var contacts = _MongoContext.mCompanies.AsQueryable().Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a => a.Contact_Id == position.HotelPLacer_ID)).FirstOrDefault().ContactDetails;
                                    hotelplaceremail = contacts.Where(a => a.Contact_Id == position.HotelPLacer_ID).FirstOrDefault().MAIL;
                                    if (string.IsNullOrWhiteSpace(hotelplaceremail))
                                        hotelplaceremail = resBooking.StaffDetails.Staff_OpsUser_Email;
                                }
                                else
                                    hotelplaceremail = resBooking.StaffDetails.Staff_OpsUser_Email;

                                var email = GetSmtpCredentials(request.UserEmail)?.UserName;
                                response.To = Encrypt.DecryptData("", email);
                                response.From = strFromEmail;
                                response.CC = hotelplaceremail;
                                response.Body = builder.ToString();
                                response.Subject = resBooking.BookingNumber + " / " + ProductSrp.CityName;
                                response.AlternateServiceId = request.AlternateServiceId;
                                response.SupplierId = request.SupplierId;
                                response.EmailGetReq.PositionId = position?.Position_Id;

                                response.ResponseStatus.Status = "Success";
                                response.ResponseStatus.ErrorMessage = "Mail Template Created Successfully.";
                            }
                            else
                            {
                                response.ResponseStatus.ErrorMessage = "To Email Id can not be null/blank.";
                                response.ResponseStatus.Status = "Error";
                            }
                        }
                        else
                        {
                            response.ResponseStatus.ErrorMessage = "Invalid AlternateService Id.";
                            response.ResponseStatus.Status = "Error";
                        }
                    }
                    else
                    {
                        response.ResponseStatus.ErrorMessage = "Invalid Position Id.";
                        response.ResponseStatus.Status = "Error";
                    }
                }
                else
                {
                    response.ResponseStatus.ErrorMessage = "Invalid Booking Number.";
                    response.ResponseStatus.Status = "Error";
                }
            }
            catch (System.Exception ex)
            {
                response.ResponseStatus.ErrorMessage = ex.Message;
                response.ResponseStatus.Status = "Error";
            }
            lstResponse.Add(response);
            lstResponse[0].ResponseStatus = response.ResponseStatus;
            return lstResponse;
        }

        //Send Hotel Not Available Email 
        public async Task<List<EmailTemplateGetRes>> CreateHotelNotAvailableEmailTemplate(EmailGetReq request, string pathToFile)
        {
            List<EmailTemplateGetRes> lstResponse = new List<EmailTemplateGetRes>();
            EmailTemplateGetRes response = new EmailTemplateGetRes();
            try
            {
                StringBuilder builder = new StringBuilder();

                var resBooking = await _MongoContext.Bookings.FindAsync(a => a.BookingNumber == request.BookingNo).Result.FirstOrDefaultAsync();
                if (resBooking != null)
                {
                    var position = resBooking.Positions.Where(a => a.Position_Id == request.PositionId).FirstOrDefault();

                    if (position != null && position.AlternateServices.Count > 0)
                    {
                        var AltSvc = position.AlternateServices.Where(a => a.AlternateServies_Id == request.AlternateServiceId).FirstOrDefault();
                        if (AltSvc != null && AltSvc.SupplierInfo != null)
                        {
                            var ProductSrp = _MongoContext.mProducts_Lite.AsQueryable().Where(a => a.VoyagerProduct_Id == AltSvc.Product_Id).FirstOrDefault();
                            string strFromEmail = AltSvc.SupplierInfo.Contact_Email;
                            if (!string.IsNullOrEmpty(strFromEmail))
                            {
                                using (StreamReader SourceReader = File.OpenText(pathToFile))
                                {
                                    builder.Append(SourceReader.ReadToEnd());
                                }
                                builder.Replace("{{CUSTOMER_NAME}}", AltSvc.SupplierInfo.Contact_Name);
                                builder.Replace("{{HOTEL_NAME}}", ProductSrp.ProdName);
                                builder.Replace("{{HOTEL_ADDRESS}}", ProductSrp.Address);
                                builder.Replace("{{HOTEL_CITY}}", ProductSrp.CityName);
                                builder.Replace("{{HOTEL_COUNTRY}}", ProductSrp.CountryName);

                                builder.Replace("{{Booking_Number}}", resBooking.BookingNumber);
                                builder.Replace("{{Tour_Name}}", resBooking.CustRef);
                                builder.Replace("{{Check_In_Date}}", Convert.ToDateTime(position.STARTDATE).ToString("dd-MMM-yyyy"));
                                builder.Replace("{{Check_Out_Date}}", Convert.ToDateTime(position.ENDDATE).ToString("dd-MMM-yyyy"));
                                builder.Replace("{{No_Of_Nights}}", position.DURATION);
                                builder.Replace("{{Option_Date}}", (position.OPTIONDATE != null ? Convert.ToDateTime(position.OPTIONDATE).ToString("dd-MMM-yyyy") : ""));
                                builder.Replace("{{Supplier_CompanyName}}", AltSvc.SupplierInfo.Name);

                                string hotelplaceremail = "";
                                if (!string.IsNullOrWhiteSpace(position.HotelPLacer_ID))
                                {
                                    var contacts = _MongoContext.mCompanies.AsQueryable().Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a => a.Contact_Id == position.HotelPLacer_ID)).FirstOrDefault().ContactDetails;
                                    hotelplaceremail = contacts.Where(a => a.Contact_Id == position.HotelPLacer_ID).FirstOrDefault().MAIL;
                                    if (string.IsNullOrWhiteSpace(hotelplaceremail))
                                        hotelplaceremail = resBooking.StaffDetails.Staff_OpsUser_Email;
                                }
                                else
                                    hotelplaceremail = resBooking.StaffDetails.Staff_OpsUser_Email;

                                var email = GetSmtpCredentials(request.UserEmail)?.UserName;
                                response.To = Encrypt.DecryptData("", email);
                                response.From = strFromEmail;
                                response.CC = hotelplaceremail;
                                response.Subject = resBooking.BookingNumber + " / " + ProductSrp.CityName;
                                response.Body = builder.ToString();
                                response.AlternateServiceId = request.AlternateServiceId;
                                response.SupplierId = request.SupplierId;
                                response.EmailGetReq.PositionId = position?.Position_Id;

                                response.ResponseStatus.Status = "Success";
                                response.ResponseStatus.StatusMessage = "Mail Template Created Successfully.";
                            }
                            else
                            {
                                response.ResponseStatus.ErrorMessage = "To Email Id can not be null/blank.";
                                response.ResponseStatus.Status = "Error";
                            }
                        }
                        else
                        {
                            response.ResponseStatus.ErrorMessage = "Invalid AlternateService Id.";
                            response.ResponseStatus.Status = "Error";
                        }
                    }
                    else
                    {
                        response.ResponseStatus.ErrorMessage = "Invalid Position Id.";
                        response.ResponseStatus.Status = "Error";
                    }
                }
                else
                {
                    response.ResponseStatus.ErrorMessage = "Invalid Booking Number.";
                    response.ResponseStatus.Status = "Error";
                }
            }
            catch (System.Exception ex)
            {
                response.ResponseStatus.ErrorMessage = ex.Message;
                response.ResponseStatus.Status = "Error";
            }
            lstResponse.Add(response);
            lstResponse[0].ResponseStatus = response.ResponseStatus;
            return lstResponse;
        }

        //Followup Email 
        public async Task<List<EmailTemplateGetRes>> CreateFollowupEmailTemplate(EmailGetReq request, string pathToFile)
        {
            List<EmailTemplateGetRes> lstResponse = new List<EmailTemplateGetRes>();
            EmailTemplateGetRes response = new EmailTemplateGetRes();
            try
            {
                var builder = new StringBuilder();
                using (StreamReader SourceReader = File.OpenText(pathToFile))
                {
                    builder.Append(SourceReader.ReadToEnd());
                }

                if (!string.IsNullOrEmpty(request.BookingNo))
                {
                    var resBooking = await _MongoContext.Bookings.FindAsync(a => a.BookingNumber == request.BookingNo).Result.FirstOrDefaultAsync();
                    if (resBooking != null)
                    {
                        var position = resBooking.Positions.Where(a => a.Position_Id == request.PositionId).FirstOrDefault();

                        var ProductSrp = new mProducts_Lite();
                        var AlternateService = new AlternateServices();

                        if (string.IsNullOrEmpty(request.AlternateServiceId))
                        {
                            ProductSrp = _MongoContext.mProducts_Lite.AsQueryable().Where(a => a.VoyagerProduct_Id == position.Product_Id).FirstOrDefault();
                        }
                        else
                        {
                            AlternateService = position.AlternateServices.Where(a => a.AlternateServies_Id == request.AlternateServiceId).FirstOrDefault();
                            if (AlternateService != null)
                                ProductSrp = _MongoContext.mProducts_Lite.AsQueryable().Where(a => a.VoyagerProduct_Id == AlternateService.Product_Id).FirstOrDefault();
                        }

                        var statusName = _MongoContext.mStatus.AsQueryable().Where(a => a.Status == position.STATUS).Select(a => a.Description).FirstOrDefault();
                        var supplierEmail = position.SupplierInfo.Contact_Email;

                        if (position != null)
                        {
                            int days = Convert.ToInt32(position.DURATION);
                            days = days + 1;
                            #region replace email content
                            builder.Replace("{{Product_Supplier_Contact_Details_for_Groups}}", position.SupplierInfo.Contact_Name);
                            builder.Replace("{{Hotel_Name}}", ProductSrp.ProdName);
                            builder.Replace("{{Hotel_Address_Line_1}}", ProductSrp.Address);
                            builder.Replace("{{Hotel_City}}", ProductSrp.CityName);
                            builder.Replace("{{Hotel_Country}}", ProductSrp.CountryName);
                            builder.Replace("{{Our_Booking_Reference_Number}}", resBooking.BookingNumber);
                            builder.Replace("{{Client_Tour_Name}}", resBooking.CustRef);
                            builder.Replace("{{Check_In_Date}}", Convert.ToDateTime(position.STARTDATE).ToString("dd-MMM-yyyy"));
                            builder.Replace("{{Check_Out_Date}}", Convert.ToDateTime(position.ENDDATE).ToString("dd-MMM-yyyy"));
                            builder.Replace("{{Position_Duration}}", position.DURATION);
                            builder.Replace("{{Option_Date}}", position.OPTIONDATE != null ? Convert.ToDateTime(position.OPTIONDATE).ToString("dd-MMM-yyyy") : string.Empty);
                            builder.Replace("{{Position_Status}}", statusName);
                            builder.Replace("{{Supplier_Contact_Name}}", string.IsNullOrEmpty(request.AlternateServiceId) ? position.SupplierInfo.Contact_Name : AlternateService?.SupplierInfo?.Contact_Name);
                            builder.Replace("{{Supplier_Company_Name}}", string.IsNullOrEmpty(request.AlternateServiceId) ? position.SupplierInfo.Name : AlternateService?.SupplierInfo?.Name);

                            var rooms = new StringBuilder();

                            if (string.IsNullOrEmpty(request.AlternateServiceId))
                            {

                                if (position.BookingRoomsAndPrices != null && position.BookingRoomsAndPrices.Count > 0)
                                {
                                    for (int a = 0; a < position.BookingRoomsAndPrices.Count; a++)
                                    {
                                        var persontype = position.BookingRoomsAndPrices[a].PersonType; if (!string.IsNullOrWhiteSpace(persontype)) { persontype = "(" + persontype + ")"; } else { persontype = ""; }
                                        var categoryname = (position.BookingRoomsAndPrices[a].CategoryName == null ? "" : position.BookingRoomsAndPrices[a].CategoryName.ToUpper()) + " " + position.BookingRoomsAndPrices[a].RoomName + " " + persontype;
                                        rooms.Append("<tr>");
                                        rooms.Append("<td>" + categoryname + "</td>");
                                        rooms.Append("<td>" + position.BookingRoomsAndPrices[a].Req_Count + "</td>");
                                        rooms.Append("<td>" + position.BookingRoomsAndPrices[a].BuyPrice + " (" + position.BookingRoomsAndPrices[a].BuyCurrency_Name + ")" + "</td>");
                                        rooms.Append("</tr>");
                                    }
                                }
                                else
                                {
                                    rooms.Append("<tr>");
                                    rooms.Append("<td></td>");
                                    rooms.Append("<td></td>");
                                    rooms.Append("<td></td>");
                                    rooms.Append("</tr>");
                                }

                            }
                            else
                            {
                                if (AlternateService.Request_RoomsAndPrices != null && AlternateService.Request_RoomsAndPrices.Count > 0)
                                {
                                    for (int a = 0; a < AlternateService.Request_RoomsAndPrices.Count; a++)
                                    {
                                        var persontype = AlternateService.Request_RoomsAndPrices[a].PersonType; if (!string.IsNullOrWhiteSpace(persontype)) { persontype = "(" + persontype + ")"; } else { persontype = ""; }
                                        var categoryname = (AlternateService.Request_RoomsAndPrices[a].CategoryName == null ? "" : AlternateService.Request_RoomsAndPrices[a].CategoryName.ToUpper()) + " " + AlternateService.Request_RoomsAndPrices[a].RoomName + " " + persontype;
                                        rooms.Append("<tr>");
                                        rooms.Append("<td>" + categoryname + "</td>");
                                        rooms.Append("<td>" + AlternateService.Request_RoomsAndPrices[a].Req_Count + "</td>");
                                        rooms.Append("<td>" + AlternateService.Request_RoomsAndPrices[a].BuyPrice + " (" + AlternateService.Request_RoomsAndPrices[a].BuyCurrency_Name + ")" + "</td>");
                                        rooms.Append("</tr>");
                                    }
                                }
                                else
                                {
                                    rooms.Append("<tr>");
                                    rooms.Append("<td></td>");
                                    rooms.Append("<td></td>");
                                    rooms.Append("<td></td>");
                                    rooms.Append("</tr>");
                                }
                            }

                            builder.Replace("{{BookingRooms}}", rooms.ToString());
                            #endregion

                            string hotelplaceremail = "";
                            if (!string.IsNullOrWhiteSpace(position.HotelPLacer_ID))
                            {
                                var contacts = _MongoContext.mCompanies.AsQueryable().Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a => a.Contact_Id == position.HotelPLacer_ID)).FirstOrDefault().ContactDetails;
                                hotelplaceremail = contacts.Where(a => a.Contact_Id == position.HotelPLacer_ID).FirstOrDefault().MAIL;
                                if (string.IsNullOrWhiteSpace(hotelplaceremail))
                                    hotelplaceremail = resBooking.StaffDetails.Staff_OpsUser_Email;
                            }
                            else
                                hotelplaceremail = resBooking.StaffDetails.Staff_OpsUser_Email;

                            var email = GetSmtpCredentials(request.UserEmail)?.UserName;
                            response.From = Encrypt.DecryptData("", email);
                            response.To = supplierEmail;
                            response.CC = request.UserEmail + ";" + hotelplaceremail;
                            response.Body = builder.ToString();
                            //response.Subject = resBooking.BookingNumber + " / " + position.City + " – Followup";
                            response.Subject = resBooking.BookingNumber + " , " + position.City;
                            response.AlternateServiceId = request.AlternateServiceId;
                            response.SupplierId = string.IsNullOrEmpty(request.AlternateServiceId) ? position.SupplierInfo.Id : request.SupplierId;

                            response.EmailGetReq.BookingNo = request.BookingNo;
                            response.EmailGetReq.PositionId = request.PositionId;
                            response.EmailGetReq.AlternateServiceId = request.AlternateServiceId;
                            response.EmailGetReq.DocumentType = request.DocumentType;
                            response.EmailGetReq.SupplierId = string.IsNullOrEmpty(request.AlternateServiceId) ? position.SupplierInfo.Id : request.SupplierId;

                            response.ResponseStatus.Status = "Success";
                            response.ResponseStatus.StatusMessage = "Mail Template Created Successfully.";
                        }
                        else
                        {
                            response.ResponseStatus.Status = "Error";
                            response.ResponseStatus.ErrorMessage = "Position Details Not Found";
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Error";
                        response.ResponseStatus.ErrorMessage = "Booking details not found";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Error";
                    response.ResponseStatus.ErrorMessage = "Booking number not found";
                }
            }
            catch (System.Exception ex)
            {
                response.ResponseStatus.Status = "Error";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }
            lstResponse.Add(response);
            lstResponse[0].ResponseStatus = response.ResponseStatus;
            return lstResponse;
        }

        //Followup Expensive Email 
        public async Task<List<EmailTemplateGetRes>> CreateFollowupExpensiveEmailTemplate(EmailGetReq request, string pathToFile)
        {
            List<EmailTemplateGetRes> lstResponse = new List<EmailTemplateGetRes>();
            EmailTemplateGetRes response = new EmailTemplateGetRes();
            try
            {
                var builder = new StringBuilder();
                using (StreamReader SourceReader = File.OpenText(pathToFile))
                {
                    builder.Append(SourceReader.ReadToEnd());
                }

                if (!string.IsNullOrEmpty(request.BookingNo))
                {
                    var resBooking = await _MongoContext.Bookings.FindAsync(a => a.BookingNumber == request.BookingNo).Result.FirstOrDefaultAsync();
                    if (resBooking != null)
                    {
                        var position = resBooking.Positions.Where(a => a.Position_Id == request.PositionId).FirstOrDefault();
                        var AlternateService = position.AlternateServices.Where(a => a.AlternateServies_Id == request.AlternateServiceId).FirstOrDefault();
                        var ProductSrp = new mProducts_Lite();
                        if (AlternateService != null)
                            ProductSrp = _MongoContext.mProducts_Lite.AsQueryable().Where(a => a.VoyagerProduct_Id == AlternateService.Product_Id).FirstOrDefault();
                        var supplierEmail = position.SupplierInfo.Contact_Email;

                        if (position != null)
                        {
                            int days = Convert.ToInt32(position.DURATION);
                            days = days + 1;
                            #region replace email content
                            builder.Replace("{{Product_Supplier_Contact_Details_for_Groups}}", position.SupplierInfo.Contact_Name);
                            builder.Replace("{{Hotel_Name}}", ProductSrp.ProdName);
                            builder.Replace("{{Hotel_Address_Line_1}}", ProductSrp.Address);
                            builder.Replace("{{Hotel_City}}", ProductSrp.CityName);
                            builder.Replace("{{Hotel_Country}}", ProductSrp.CountryName);
                            builder.Replace("{{Our_Booking_Reference_Number}}", resBooking.BookingNumber);
                            builder.Replace("{{Client_Tour_Name}}", resBooking.CustRef);
                            builder.Replace("{{Check_In_Date}}", Convert.ToDateTime(position.STARTDATE).ToString("dd-MMM-yyyy"));
                            builder.Replace("{{Check_Out_Date}}", Convert.ToDateTime(position.ENDDATE).ToString("dd-MMM-yyyy"));
                            builder.Replace("{{Position_Duration}}", position.DURATION);
                            builder.Replace("{{Option_Date}}", position.OPTIONDATE != null ? Convert.ToDateTime(position.OPTIONDATE).ToString("dd-MMM-yyyy") : string.Empty);
                            builder.Replace("{{Supplier_Contact_Name}}", AlternateService?.SupplierInfo?.Contact_Name);
                            builder.Replace("{{Supplier_Company_Name}}", AlternateService?.SupplierInfo?.Name);

                            var rooms = new StringBuilder();
                            if (AlternateService.Request_RoomsAndPrices != null && AlternateService.Request_RoomsAndPrices.Count > 0)
                            {
                                for (int a = 0; a < AlternateService.Request_RoomsAndPrices.Count; a++)
                                {
                                    var persontype = AlternateService.Request_RoomsAndPrices[a].PersonType; if (!string.IsNullOrWhiteSpace(persontype)) { persontype = "(" + persontype + ")"; } else { persontype = ""; }
                                    var categoryname = (AlternateService.Request_RoomsAndPrices[a].CategoryName == null ? "" : AlternateService.Request_RoomsAndPrices[a].CategoryName.ToUpper()) + " " + AlternateService.Request_RoomsAndPrices[a].RoomName + " " + persontype;
                                    rooms.Append("<tr>");
                                    rooms.Append("<td>" + categoryname + "</td>");
                                    rooms.Append("<td>" + AlternateService.Request_RoomsAndPrices[a].Req_Count + "</td>");
                                    rooms.Append("<td>" + AlternateService.Request_RoomsAndPrices[a].BuyPrice + " (" + AlternateService.Request_RoomsAndPrices[a].BuyCurrency_Name + ")" + "</td>");
                                    rooms.Append("</tr>");
                                }
                            }
                            else
                            {
                                rooms.Append("<tr>");
                                rooms.Append("<td></td>");
                                rooms.Append("<td></td>");
                                rooms.Append("<td></td>");
                                rooms.Append("</tr>");
                            }
                            builder.Replace("{{BookingRooms}}", rooms.ToString());
                            #endregion

                            string hotelplaceremail = "";
                            if (!string.IsNullOrWhiteSpace(position.HotelPLacer_ID))
                            {
                                var contacts = _MongoContext.mCompanies.AsQueryable().Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a => a.Contact_Id == position.HotelPLacer_ID)).FirstOrDefault().ContactDetails;
                                hotelplaceremail = contacts.Where(a => a.Contact_Id == position.HotelPLacer_ID).FirstOrDefault().MAIL;
                                if (string.IsNullOrWhiteSpace(hotelplaceremail))
                                    hotelplaceremail = resBooking.StaffDetails.Staff_OpsUser_Email;
                            }
                            else
                                hotelplaceremail = resBooking.StaffDetails.Staff_OpsUser_Email;

                            var email = GetSmtpCredentials(request.UserEmail)?.UserName;
                            response.From = Encrypt.DecryptData("", email);
                            response.To = supplierEmail;
                            response.CC = request.UserEmail + ";" + hotelplaceremail;
                            response.Body = builder.ToString();
                            response.Subject = resBooking.BookingNumber + ", " + position.City + ", Rate Check";
                            response.AlternateServiceId = request.AlternateServiceId;
                            response.SupplierId = request.SupplierId;

                            response.EmailGetReq.BookingNo = request.BookingNo;
                            response.EmailGetReq.PositionId = request.PositionId;
                            response.EmailGetReq.AlternateServiceId = request.AlternateServiceId;
                            response.EmailGetReq.DocumentType = request.DocumentType;
                            response.EmailGetReq.SupplierId = request.SupplierId;

                            response.ResponseStatus.Status = "Success";
                            response.ResponseStatus.StatusMessage = "Mail Template Created Successfully.";
                        }
                        else
                        {
                            response.ResponseStatus.Status = "Error";
                            response.ResponseStatus.ErrorMessage = "Position Details Not Found";
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Error";
                        response.ResponseStatus.ErrorMessage = "Booking details not found";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Error";
                    response.ResponseStatus.ErrorMessage = "Booking number not found";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Error";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }
            lstResponse.Add(response);
            lstResponse[0].ResponseStatus = response.ResponseStatus;
            return lstResponse;
        }

        //Extend Option Date
        public async Task<List<EmailTemplateGetRes>> CreateExtendOptionDateEmailTemplate(EmailGetReq request, string pathToFile)
        {
            try
            {
                List<EmailTemplateGetRes> lstResponse = new List<EmailTemplateGetRes>();
                EmailTemplateGetRes response = new EmailTemplateGetRes();
                var builder = new StringBuilder();

                using (StreamReader SourceReader = File.OpenText(pathToFile))
                {
                    builder.Append(SourceReader.ReadToEnd());
                }

                if (!string.IsNullOrEmpty(request.BookingNo))
                {
                    var resBooking = await _MongoContext.Bookings.FindAsync(a => a.BookingNumber == request.BookingNo).Result.FirstOrDefaultAsync();
                    if (resBooking != null)
                    {
                        var position = resBooking.Positions.Where(a => a.Position_Id == request.PositionId).FirstOrDefault();
                        var AlternateService = position.AlternateServices.Where(a => a.AlternateServies_Id == request.AlternateServiceId).FirstOrDefault();
                        var ProductSrp = new mProducts_Lite();
                        if (AlternateService != null)
                            ProductSrp = _MongoContext.mProducts_Lite.AsQueryable().Where(a => a.VoyagerProduct_Id == AlternateService.Product_Id).FirstOrDefault();
                        var statusName = _MongoContext.mStatus.AsQueryable().Where(a => a.Status == position.STATUS).Select(a => a.Description).FirstOrDefault();
                        var supplierEmail = position.SupplierInfo.Contact_Email;

                        if (position != null)
                        {
                            int days = Convert.ToInt32(position.DURATION);
                            days = days + 1;
                            #region replace email content
                            builder.Replace("{{Product_Supplier_Contact_Details_for_Groups}}", position.SupplierInfo.Contact_Name);
                            builder.Replace("{{Hotel_Name}}", ProductSrp.ProdName);
                            builder.Replace("{{Hotel_Address_Line_1}}", ProductSrp.Address);
                            builder.Replace("{{Hotel_City}}", ProductSrp.CityName);
                            builder.Replace("{{Hotel_Country}}", ProductSrp.CountryName);
                            builder.Replace("{{Our_Booking_Reference_Number}}", resBooking.BookingNumber);
                            builder.Replace("{{Client_Tour_Name}}", resBooking.CustRef);
                            builder.Replace("{{Check_In_Date}}", Convert.ToDateTime(position.STARTDATE).ToString("dd-MMM-yyyy"));
                            builder.Replace("{{Check_Out_Date}}", Convert.ToDateTime(position.ENDDATE).ToString("dd-MMM-yyyy"));
                            builder.Replace("{{Position_Duration}}", position.DURATION);
                            builder.Replace("{{Option_Date}}", position.OPTIONDATE != null ? Convert.ToDateTime(position.OPTIONDATE).ToString("dd-MMM-yyyy") : string.Empty);
                            builder.Replace("{{Position_Status}}", statusName);
                            builder.Replace("{{Supplier_Contact_Name}}", AlternateService?.SupplierInfo?.Contact_Name);
                            builder.Replace("{{Supplier_Company_Name}}", AlternateService?.SupplierInfo?.Name);
                            #endregion

                            string hotelplaceremail = "";
                            if (!string.IsNullOrWhiteSpace(position.HotelPLacer_ID))
                            {
                                var contacts = _MongoContext.mCompanies.AsQueryable().Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a => a.Contact_Id == position.HotelPLacer_ID)).FirstOrDefault().ContactDetails;
                                hotelplaceremail = contacts.Where(a => a.Contact_Id == position.HotelPLacer_ID).FirstOrDefault().MAIL;
                                if (string.IsNullOrWhiteSpace(hotelplaceremail))
                                    hotelplaceremail = resBooking.StaffDetails.Staff_OpsUser_Email;
                            }
                            else
                                hotelplaceremail = resBooking.StaffDetails.Staff_OpsUser_Email;

                            var email = GetSmtpCredentials(request.UserEmail)?.UserName;
                            response.From = Encrypt.DecryptData("", email);
                            response.To = supplierEmail;
                            response.CC = request.UserEmail + ";" + hotelplaceremail;
                            response.Body = builder.ToString();
                            //string optdt = position.OPTIONDATE != null ? (" ," + position.OPTIONDATE.Value.ToString("dd MMM yyyy")) : "";
                            response.Subject = resBooking.BookingNumber + " ," + position.City + ", Extend Option Date";
                            response.AlternateServiceId = request.AlternateServiceId;
                            response.SupplierId = request.SupplierId;

                            response.EmailGetReq.BookingNo = request.BookingNo;
                            response.EmailGetReq.PositionId = request.PositionId;
                            response.EmailGetReq.AlternateServiceId = request.AlternateServiceId;
                            response.EmailGetReq.DocumentType = request.DocumentType;
                            response.EmailGetReq.SupplierId = request.SupplierId;

                            response.ResponseStatus.Status = "Success";
                            response.ResponseStatus.StatusMessage = "Mail Template Created Successfully.";
                        }
                        else
                        {
                            response.ResponseStatus.Status = "Error";
                            response.ResponseStatus.ErrorMessage = "Position Details Not Found";
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Error";
                        response.ResponseStatus.ErrorMessage = "Booking details not found";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Error";
                    response.ResponseStatus.ErrorMessage = "Booking number not found";
                }
                lstResponse.Add(response);
                lstResponse[0].ResponseStatus = response.ResponseStatus;
                return lstResponse;
            }
            catch (System.Exception ex)
            {
                throw;
            }
        }

        //Handover : Go Ahead Booking
        public async Task<List<EmailTemplateGetRes>> CreateGoAheadBookingEmailTemplate(EmailGetReq request, string pathToFile)
        {
            List<EmailTemplateGetRes> lstResponse = new List<EmailTemplateGetRes>();
            EmailTemplateGetRes response = new EmailTemplateGetRes();
            try
            { 
                var dtformat = "";
                var departuredt = new DateTime();

                var builder = new StringBuilder();
                using (StreamReader SourceReader = File.OpenText(pathToFile))
                {
                    builder.Append(SourceReader.ReadToEnd());
                }

                var resQuote = _MongoContext.mQuote.AsQueryable().Where(a => a.QRFID == request.QrfId).FirstOrDefault();
                if (resQuote != null)
                {
                    var resultGoAhead = await _MongoContext.mGoAhead.FindAsync(m => m.QRFID == request.QrfId && m.GoAheadId == request.GoAheadId && m.IsDeleted == false).Result.FirstOrDefaultAsync();

                    if (resultGoAhead != null)
                    {
                        var QRFPrice = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == request.QrfId && a.QRFPrice_Id == resultGoAhead.QRFPriceId).FirstOrDefault();
                        if (QRFPrice != null)
                        {
                            var toEmail = _MongoContext.mContacts.AsQueryable().Where(x => x.VoyagerContact_Id == resultGoAhead.OperationUserID).Select(y => y.MAIL).FirstOrDefault();

                            //var toEmail = QRFPrice.SalesOfficer;

                            builder.Replace("[#AgentGroupNm]", QRFPrice.AgentProductInfo.TourName);
                            builder.Replace("[#AgentCustEmail]", QRFPrice.AgentInfo.EmailAddress);
                            builder.Replace("[#AgentName]", QRFPrice.AgentInfo.AgentName);
                            if (!string.IsNullOrEmpty(resultGoAhead.Remarks))
                            {
                                var remarks = "<p><b>Sales Team Remarks</b></p><p>" + resultGoAhead.Remarks.Trim() + "</p>";
                                builder.Replace("[#GoAheadRemarks]", remarks);
                            }
                            else
                            {
                                builder.Replace("[#GoAheadRemarks]", "");
                            }
                            builder.Replace("[#QRFAgentName]", QRFPrice.AgentProductInfo.TourName);
                            builder.Replace("[#SalesOFficerEmail]", QRFPrice.SalesOfficer);
                            builder.Replace("[#QRFID]", QRFPrice.QRFID.ToString());
                            builder.Replace("[#CostingOfficerEmail]", QRFPrice.CostingOfficer);
                            builder.Replace("[#QRFNationality]", QRFPrice.AgentProductInfo.Product);
                            builder.Replace("[#ProductAccountantEmail]", QRFPrice.ProductAccountant);
                            builder.Replace("[#QRFDestination]", QRFPrice.AgentProductInfo.Destination);

                            var Depatures = new StringBuilder();
                            TimeSpan dtdiff = new TimeSpan();

                            var childtype = "";
                            var chkflag = "";
                            string strBookingNo = "";
                            if (resultGoAhead.Depatures != null && resultGoAhead.Depatures.Count > 0 && !string.IsNullOrWhiteSpace(request.DepartureId))
                            {
                                long deptId = Convert.ToInt32(request.DepartureId);
                                var item = resultGoAhead.Depatures.Where(x => x.DepatureId == deptId).FirstOrDefault();

                                strBookingNo = " (" + request.BookingNo + ")";

                                departuredt = Convert.ToDateTime(item.DepatureDate);
                                Depatures.Append("<tr><td>");
                                //departuredt = departuredt.AddDays(1);
                                dtformat = departuredt.ToString("dd MMM yyyy");
                                Depatures.Append(dtformat);
                                chkflag = item.IsMaterialised ? "<input type='checkbox' disabled='true' checked='checked'/>" : "<input type='checkbox' disabled='true'/>";
                                Depatures.Append("</td><td>" + chkflag + "</td><td>");
                                if (item.PassengerRoomInfo != null && item.PassengerRoomInfo.Count > 0)
                                {
                                    Depatures.Append("<p><b>Rooms</b></p><ul>");
                                    foreach (var adult in item.PassengerRoomInfo)
                                    {
                                        Depatures.Append("<li>" + adult.RoomTypeName + " - " + adult.RoomCount.ToString() + " </li>");
                                    }
                                    Depatures.Append("</ul>");
                                }
                                if (item.ChildInfo != null && item.ChildInfo.Count > 0)
                                {
                                    Depatures.Append("<p><b>Children</b></p><ul>");
                                    foreach (var child in item.ChildInfo)
                                    {
                                        childtype = child.Type == "CHILDWITHOUTBED" ? "Child - Bed" : child.Type == "CHILDWITHBED" ? "Child + Bed" : child.Type == "INFANT" ? "Infant" : child.Type;
                                        Depatures.Append("<li>" + childtype + " (" + child.Age.ToString() + "yr) " + child.Number + "</li>");
                                    }
                                    Depatures.Append("</ul>");
                                }
                                Depatures.Append("</td>");
                                dtdiff = DateTime.Now.Subtract(departuredt);
                                Depatures.Append("<td>" + dtdiff.Days + " days</td></tr>");


                                //foreach (var item in resultGoAhead.Depatures)
                                //{
                                //	departuredt = Convert.ToDateTime(item.DepatureDate);
                                //	Depatures.Append("<tr><td>");
                                //	departuredt = departuredt.AddDays(1);
                                //	dtformat = departuredt.ToString("dd MMM yyyy");
                                //	Depatures.Append(dtformat);
                                //	chkflag = item.IsMaterialised ? "<input type='checkbox' disabled='true' checked='checked'/>" : "<input type='checkbox' disabled='true'/>";
                                //	Depatures.Append("</td><td>" + chkflag + "</td><td>");
                                //	if (item.PassengerRoomInfo != null && item.PassengerRoomInfo.Count > 0)
                                //	{
                                //		Depatures.Append("<p><b>Rooms</b></p><ul>");
                                //		foreach (var adult in item.PassengerRoomInfo)
                                //		{
                                //			Depatures.Append("<li>" + adult.RoomTypeName + " - " + adult.RoomCount.ToString() + " </li>");
                                //		}
                                //		Depatures.Append("</ul>");
                                //	}
                                //	if (item.ChildInfo != null && item.ChildInfo.Count > 0)
                                //	{
                                //		Depatures.Append("<p><b>Children</b></p><ul>");
                                //		foreach (var child in item.ChildInfo)
                                //		{
                                //			childtype = child.Type == "CHILDWITHOUTBED" ? "Child - Bed" : child.Type == "CHILDWITHBED" ? "Child + Bed" : child.Type == "INFANT" ? "Infant" : child.Type;
                                //			Depatures.Append("<li>" + childtype + " (" + child.Age.ToString() + "yr) " + child.Number + "</li>");
                                //		}
                                //		Depatures.Append("</ul>");
                                //	}
                                //	Depatures.Append("</td>");
                                //	dtdiff = DateTime.Now.Subtract(departuredt);
                                //	Depatures.Append("<td>" + dtdiff.Days + " days</td></tr>");
                                //}
                                builder.Replace("[#Depatures]", Depatures.ToString());
                            }
                            else
                            {
                                builder.Replace("[#Depatures]", "<tr><td colspan='4'>No Departure Details Found.</td></tr>");
                            }

                            var email = GetSmtpCredentials(request.UserEmail)?.UserName;
                            response.CC = Encrypt.DecryptData("", email);
                            response.Client = QRFPrice.AgentInfo.AgentID;
                            response.Body = builder.ToString();
                            response.Subject = "GO AHEAD - " + QRFPrice.AgentInfo.AgentName + " - " + QRFPrice.AgentProductInfo.TourName + strBookingNo;
                            response.To = toEmail;
                            response.From = request.UserEmail; 

                            response.ResponseStatus.Status = "Success";
                            response.ResponseStatus.StatusMessage = "Mail Template Created Successfully.";
                        }
                        else
                        { 
                            response.ResponseStatus.ErrorMessage = "QRFPrice Details not exists.";
                            response.ResponseStatus.Status = "Error";
                        }
                    }
                    else
                    {
                        response.ResponseStatus.ErrorMessage = "mGoAhead Details not exists.";
                        response.ResponseStatus.Status = "Error";
                    }
                }
                else
                {
                    response.ResponseStatus.ErrorMessage = "QRFID not exists.";
                    response.ResponseStatus.Status = "Error";
                }
                lstResponse.Add(response);
                lstResponse[0].ResponseStatus = response.ResponseStatus;

            }
            catch (System.Exception ex)
            {
                response.ResponseStatus.ErrorMessage = ex.Message;
                response.ResponseStatus.Status = "Error";
                lstResponse.Add(response);
                lstResponse[0].ResponseStatus = response.ResponseStatus;
            }

            return lstResponse;
        }

        //Send SendHotelRequest,Remind,TestEmail Email 
        public async Task<List<EmailTemplateGetRes>> CreateHotelRequestEmailTemplate(EmailGetReq request, string pathToFile)
        {
            List<EmailTemplateGetRes> lstResponse = new List<EmailTemplateGetRes>();
            EmailTemplateGetRes response = new EmailTemplateGetRes();
            try
            {
                var resBooking = await _MongoContext.Bookings.FindAsync(a => a.BookingNumber == request.BookingNo).Result.FirstOrDefaultAsync();

                if (resBooking != null)
                {
                    var Position = resBooking.Positions.Where(a => a.Position_Id == request.PositionId).FirstOrDefault();

                    if (Position != null && Position.AlternateServices.Count > 0)
                    {
                        StringBuilder builder = new StringBuilder();
                        string RoomingDetails = "", SuppName = "", toEmail = "";
                        List<string> lstProductId = new List<string>();
                        List<SupplierContactDetails> SuppliersList = new List<SupplierContactDetails>();

                        var mUser = await _MongoContext.mUsers.FindAsync(a => a.VoyagerUser_Id == request.PlacerUserId).Result.FirstOrDefaultAsync();
                        var mCompany = await _MongoContext.mCompanies.FindAsync(a => a.Company_Id == mUser.Company_Id).Result.FirstOrDefaultAsync();
                        var resContact = mCompany.ContactDetails.Where(a => a.Contact_Id == mUser.Contact_Id).FirstOrDefault();

                        if (resContact != null)
                        {
                            foreach (var Room in Position.BookingRoomsAndPrices)
                            {
                                if (Room != null && !string.IsNullOrEmpty(Room.RoomName) && !string.IsNullOrEmpty(Room.PersonType))
                                {
                                    if (RoomingDetails != "") RoomingDetails += "+ ";
                                    if (Room.RoomName.ToLower() == "child")
                                    {
                                        RoomingDetails += (Room.Req_Count + "(" + Room.PersonType + ")");
                                    }
                                    else
                                    {
                                        RoomingDetails += (Room.Req_Count + Room.RoomName);
                                    }
                                }
                            }

                            var defHotelPosition = new AlternateServices();
                            if (request.DocumentType == DocType.REMIND)
                            {
                                Position.AlternateServices = Position.AlternateServices.Where(a => a.AlternateServies_Id == request.AlternateServiceId && (a.IsBlackListed == null || a.IsBlackListed == false) && a.Requested_On != null).ToList();
                                lstProductId = Position.AlternateServices.Select(a => a.Product_Id).ToList();
                            }
                            else if (request.DocumentType == DocType.TESTEMAIL)
                            {
                                Position.AlternateServices = Position.AlternateServices.Where(a => a.IsBlackListed == null || a.IsBlackListed == false).Take(1).ToList();
                                lstProductId = Position.AlternateServices.Select(a => a.Product_Id).ToList();
                            }
                            else
                            {
                                defHotelPosition = Position.AlternateServices.Where(a => a.Product_Id == Position.Product_Id).FirstOrDefault();
                                Position.AlternateServices = Position.AlternateServices.Where(a => a.IsBlackListed == null || a.IsBlackListed == false && (a.Requested_On == null || a.Requested_On < Convert.ToDateTime("01-01-2000"))).ToList();
                                lstProductId = Position.AlternateServices.Select(a => a.Product_Id).ToList();
                                lstProductId.Add(Position.Product_Id);
                            }

                            var ProductSrpList = _MongoContext.mProducts_Lite.AsQueryable().Where(a => lstProductId.Contains(a.VoyagerProduct_Id)).ToList();
                            var ProductSrp = ProductSrpList.Where(a => a.VoyagerProduct_Id == Position.Product_Id).FirstOrDefault();

                            //the below functionality used for sending mail for Position's Supplier as AlternateService
                            if (request.DocumentType == "BOOK-REQ")
                            {
                                if (ProductSrp != null && ProductSrp.Placeholder != true && Position.AlternateServices.Where(a => a.Product_Id == Position.Product_Id
                                    && a.SupplierInfo != null && a.SupplierInfo?.Id == Position.SupplierInfo?.Id).Count() < 1)
                                {
                                    defHotelPosition = defHotelPosition == null ? new AlternateServices() : defHotelPosition;

                                    if ((defHotelPosition.IsBlackListed == null || defHotelPosition.IsBlackListed == false) && (defHotelPosition.Requested_On == null || defHotelPosition.Requested_On < Convert.ToDateTime("01-01-2000")))
                                    {
                                        Position.AlternateServices.Add(new AlternateServices
                                        {
                                            AlternateServies_Id = request.PosAlternateServiceId,
                                            SortOrder = 1,
                                            IsBlackListed = false,
                                            Product_Id = Position.Product_Id,
                                            Product_Name = Position.Product_Name,
                                            Country_Id = Position.Country_Id,
                                            Country = Position.Country,
                                            City_Id = Position.City_Id,
                                            City = Position.City,
                                            Requested_On = null,
                                            Attributes = Position.Attributes,
                                            SupplierInfo = Position.SupplierInfo,
                                            Request_RoomsAndPrices = Position.BookingRoomsAndPrices,
                                            AuditTrail = Position.AuditTrail
                                        });
                                    }
                                }
                            }

                            foreach (var AltSvc in Position.AlternateServices)
                            {
                                SuppName = "";
                                if ((request.Source ?? "") == "opschangeproduct")
                                {
                                    if (AltSvc.SupplierInfo.Id != Position.SupplierInfo.Id)
                                    {
                                        continue;
                                    }
                                }
                                if (AltSvc != null && AltSvc.SupplierInfo != null && !string.IsNullOrEmpty(AltSvc.SupplierInfo.Contact_Name) && !string.IsNullOrEmpty(AltSvc.SupplierInfo.Contact_Email))
                                {
                                    SuppName = AltSvc.SupplierInfo.Contact_Name.Replace("Reservation Department", "Partner").Replace("Reservations Department", "Partner");// + ",<br>" + AltSvc.SupplierInfo.Name;
                                    ProductSrp = ProductSrpList.Where(a => a.VoyagerProduct_Id == AltSvc.Product_Id).FirstOrDefault();
                                    SuppliersList.Add(new SupplierContactDetails
                                    {
                                        AltSvcId = AltSvc.AlternateServies_Id,
                                        SupplierId = AltSvc.SupplierInfo?.Id,
                                        SupplierName = SuppName,
                                        SupplierEmail = AltSvc.SupplierInfo.Contact_Email,
                                        ProdName = ProductSrp.ProdName,
                                        Address = ProductSrp.Address,
                                        City = ProductSrp.CityName,
                                        Country = ProductSrp.CountryName
                                    });
                                    if (request.DocumentType == DocType.BOOKREQ)
                                    {
                                        AltSvc.Requested_On = DateTime.Now;
                                        AltSvc.Request_Status = "Sent";
                                        AltSvc.Availability_Status = "PENDING";
                                    }
                                }
                            }
                            if (SuppliersList.Count > 0)
                            {
                                for (int i = 0; i < SuppliersList.Count; i++)
                                {
                                    response = new EmailTemplateGetRes();
                                    if (request.DocumentType == DocType.TESTEMAIL)
                                    {
                                        toEmail = resContact.MAIL;
                                    }
                                    else if (request.DocumentType == DocType.BOOKREQ || request.DocumentType == DocType.REMIND)
                                    {
                                        toEmail = SuppliersList[i].SupplierEmail;
                                    }
                                    if (!string.IsNullOrEmpty(toEmail))
                                    {
                                        builder = new StringBuilder();
                                        using (StreamReader SourceReader = File.OpenText(pathToFile))
                                        {
                                            builder.Append(SourceReader.ReadToEnd());
                                        }
                                        #region replace email content
                                        builder = HotelRequestEmailDetails(SuppliersList[i], resBooking, Position, resContact, request.WebURLInitial, RoomingDetails, builder);
                                        #endregion

                                        string hotelplaceremail = "";
                                        if (!string.IsNullOrWhiteSpace(Position.HotelPLacer_ID))
                                        {
                                            var contacts = _MongoContext.mCompanies.AsQueryable().Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a => a.Contact_Id == Position.HotelPLacer_ID)).FirstOrDefault().ContactDetails;
                                            hotelplaceremail = contacts.Where(a => a.Contact_Id == Position.HotelPLacer_ID).FirstOrDefault().MAIL;
                                            if (string.IsNullOrWhiteSpace(hotelplaceremail))
                                                hotelplaceremail = resBooking.StaffDetails.Staff_OpsUser_Email;
                                        }
                                        else
                                            hotelplaceremail = resBooking.StaffDetails.Staff_OpsUser_Email;

                                        var email = GetSmtpCredentials(request.UserEmail)?.UserName;
                                        response.From = Encrypt.DecryptData("", email);
                                        response.AlternateServiceId = SuppliersList[i].AltSvcId;
                                        response.SupplierId = SuppliersList[i].SupplierId;
                                        response.Subject = resBooking.BookingNumber + " / " + SuppliersList[i].City;
                                        response.To = toEmail;
                                        response.CC = resContact.MAIL + ";" + hotelplaceremail;
                                        response.Body = builder.ToString();
                                        response.UserEmail = request.UserEmail;
                                        response.EmailGetReq.PositionId = Position?.Position_Id;

                                        response.ResponseStatus = new ResponseStatus() { Status = "Success", ErrorMessage = "Mail Template created successfully." };
                                        lstResponse.Add(response);
                                        lstResponse[0].ResponseStatus = response.ResponseStatus;
                                    }
                                    else
                                    {
                                        response.ResponseStatus = new ResponseStatus() { Status = "Error", ErrorMessage = "To Email Id can not be blank/null." };
                                        lstResponse.Add(response);
                                        lstResponse[0].ResponseStatus = response.ResponseStatus;
                                    }
                                }
                            }
                            else
                            {
                                response.ResponseStatus.ErrorMessage = "Supplier details not found to send the mail.";
                                response.ResponseStatus.Status = "Error";
                                lstResponse.Add(response);
                                lstResponse[0].ResponseStatus = response.ResponseStatus;
                            }
                        }
                        else
                        {
                            response.ResponseStatus.ErrorMessage = "Hotel placer contact details not exists.";
                            response.ResponseStatus.Status = "Error";
                            lstResponse.Add(response);
                            lstResponse[0].ResponseStatus = response.ResponseStatus;
                        }
                    }
                    else
                    {
                        response.ResponseStatus.ErrorMessage = "Invalid Position Id.";
                        response.ResponseStatus.Status = "Error";
                        lstResponse.Add(response);
                        lstResponse[0].ResponseStatus = response.ResponseStatus;
                    }
                }
                else
                {
                    response.ResponseStatus.ErrorMessage = "Invalid Booking Number.";
                    response.ResponseStatus.Status = "Error";
                    lstResponse.Add(response);
                    lstResponse[0].ResponseStatus = response.ResponseStatus;
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.ErrorMessage = ex.Message;
                response.ResponseStatus.Status = "Error";
                lstResponse.Add(response);
                lstResponse[0].ResponseStatus = response.ResponseStatus;
            }
            return lstResponse;
        }

        //Quote Followup Email 
        public async Task<List<EmailTemplateGetRes>> CreateQuoteFollowupEmailTemplate(EmailGetReq request, string pathToFile)
        {
            try
            {
                List<EmailTemplateGetRes> lstResponse = new List<EmailTemplateGetRes>();
                EmailTemplateGetRes response = new EmailTemplateGetRes();
                var builder = new StringBuilder();

                using (StreamReader SourceReader = File.OpenText(pathToFile))
                {
                    builder.Append(SourceReader.ReadToEnd());
                }

                if (!string.IsNullOrEmpty(request.QrfId))
                {
                    var resQuote = await _MongoContext.mQRFPrice.FindAsync(a => a.QRFID == request.QrfId && a.IsCurrentVersion == true).Result.FirstOrDefaultAsync();
                    if (resQuote == null)
                    {
                        var resQuoteMain = await _MongoContext.mQuote.FindAsync(a => a.QRFID == request.QrfId).Result.FirstOrDefaultAsync();
                        resQuote = new mQRFPrice();
                        resQuote.SalesOfficer = resQuoteMain.SalesPerson;
                        resQuote.FollowUp = resQuoteMain.FollowUp;
                        resQuote.AgentInfo = resQuoteMain.AgentInfo;
                        resQuote.AgentProductInfo.TourName = resQuoteMain.AgentProductInfo.TourName;
                    }
                    if (resQuote != null)
                    {
                        var FollowUp = resQuote.FollowUp.Where(a => a.FollowUp_Id == request.FollowUpId).FirstOrDefault();
                        var contactList = _MongoContext.mCompanies.AsQueryable().Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a => a.Contact_Id == FollowUp.FollowUpTask[0].FromContact_Id)).FirstOrDefault()?.ContactDetails;
                        var contact = contactList?.Where(a => a.Contact_Id == FollowUp.FollowUpTask[0].FromContact_Id).FirstOrDefault();

                        if (FollowUp != null)
                        {
                            #region replace email content
                            //var SalesOfficer = resQuote.SalesOfficer;
                            //var CompanyList = _MongoContext.mCompanies.AsQueryable();
                            //var ToUserContacts = CompanyList.Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a => a.MAIL == SalesOfficer)).FirstOrDefault()?.ContactDetails;
                            //var ToUser = ToUserContacts?.Where(a => a.MAIL == SalesOfficer).FirstOrDefault();
                            //var ToUserName = ToUser.CommonTitle + " " + ToUser.FIRSTNAME + " " + ToUser.LastNAME;

                            builder.Replace("[QRF_Number]", resQuote.QRFID);
                            builder.Replace("[Agent_Tour_Name]", resQuote.AgentProductInfo.TourName);
                            builder.Replace("[Contact_Name]", FollowUp.FollowUpTask[0].ToName);
                            builder.Replace("[Create_Date]", FollowUp.CreateDate.ToString("MMMM dd yyyy"));
                            builder.Replace("[Task]", FollowUp.FollowUpTask[0].Task);
                            builder.Replace("[FollowUp_Date_Time]", Convert.ToDateTime(FollowUp.FollowUpTask[0].FollowUpDateTime).ToString("MMMM dd yyyy - HH mm"));
                            builder.Replace("[FollowupType]", FollowUp.FollowUpTask[0].FollowUpType);
                            builder.Replace("[FollowUpPerson]", FollowUp.FollowUpTask[0].ToName + "(" + FollowUp.FollowUpTask[0].ToEmail + ")");
                            builder.Replace("[Notes]", FollowUp.FollowUpTask[0].Notes);

                            builder.Replace("[From_Contact_Name]", FollowUp.FollowUpTask[0].FromName);
                            builder.Replace("[From_Contact_Email_Address]", FollowUp.FollowUpTask[0].FromEmail);
                            if (contact != null)
                            {
                                builder.Replace("[From_Contact_Telephone_Number]", contact.TEL);
                                builder.Replace("[From_Contact_Mobile_Number]", contact.MOBILE);
                            }
                            else
                            {
                                builder.Replace("[From_Contact_Telephone_Number]", "");
                                builder.Replace("[From_Contact_Mobile_Number]", "");
                            }
                            #endregion

                            response.From = FollowUp.FollowUpTask[0].FromEmail;
                            response.To = FollowUp.FollowUpTask[0].ToEmail;
                            //response.CC = request.UserEmail;
                            response.Body = builder.ToString();
                            response.Subject = "FOLLOW UP: " + resQuote.QRFID + " - " + resQuote.AgentInfo.AgentName + " - " + resQuote.AgentProductInfo.TourName + " - " + FollowUp.FollowUpTask[0].Task;

                            response.EmailGetReq.QrfId = request.QrfId;
                            response.EmailGetReq.FollowUpId = request.FollowUpId;

                            response.ResponseStatus.Status = "Success";
                            response.ResponseStatus.StatusMessage = "Mail Template Created Successfully.";
                        }
                        else
                        {
                            response.ResponseStatus.Status = "Error";
                            response.ResponseStatus.ErrorMessage = "FollowUp Details Not Found";
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Error";
                        response.ResponseStatus.ErrorMessage = "Quote details not found";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Error";
                    response.ResponseStatus.ErrorMessage = "QRF number not found";
                }
                lstResponse.Add(response);
                lstResponse[0].ResponseStatus = response.ResponseStatus;
                return lstResponse;
            }
            catch (System.Exception ex)
            {
                throw;
            }
        }
        #endregion

        #region Sales Module
        public async Task<List<EmailTemplateGetRes>> CreateSubmitQuoteEmailTemplate(EmailGetReq request, string pathToFile)
        {
            List<EmailTemplateGetRes> lstResponse = new List<EmailTemplateGetRes>();
            EmailTemplateGetRes response = new EmailTemplateGetRes();
            try
            {
                var resQuote = await _MongoContext.mQuote.FindAsync(a => a.QRFID == request.QrfId).Result.FirstOrDefaultAsync();

                if (resQuote != null)
                {
                    var resQRFPriceList = await _MongoContext.mQRFPrice.FindAsync(a => a.QRFID == request.QrfId && a.IsCurrentVersion == true).Result.ToListAsync();

                    if (resQRFPriceList != null && resQRFPriceList.Count > 0)
                    {
                        var resQRFPrice = resQRFPriceList.OrderByDescending(a => a.VersionId).FirstOrDefault();

                        StringBuilder builder = new StringBuilder();
                        response = new EmailTemplateGetRes();
                        request.QRFPriceId = resQRFPrice.QRFPrice_Id;

                        if (!string.IsNullOrEmpty(request.EnquiryPipeline))
                        {
                            using (StreamReader SourceReader = File.OpenText(pathToFile))
                            {
                                builder.Append(SourceReader.ReadToEnd());
                            }

                            builder.Replace("[#QRFID]", Convert.ToString(request.QrfId));
                            builder.Replace("[#Date]", DateTime.Now.ToString("dd MMM yyyy"));
                            builder.Replace("[#Remarks]", request.Remarks);

                            var email = GetSmtpCredentials(request.UserEmail)?.UserName;
                            var commonCC = Encrypt.DecryptData("", email);

                            if (request.EnquiryPipeline.ToLower() == "quote pipeline")
                            {
                                response.To = resQRFPrice.CostingOfficer;
                                response.CC = resQRFPrice.SalesOfficer + ";" + commonCC;
                                builder.Replace("[#SalesManager]", request.UserName);
                                response.Subject = "COSTING REQUIRED For " + request.UserName + " : QRFID : " + request.QrfId + "-" + resQRFPrice.AgentProductInfo.TourName;
                            }
                            else if (request.EnquiryPipeline.ToLower() == "costing pipeline")
                            {
                                response.To = resQRFPrice.ProductAccountant;
                                response.CC = resQRFPrice.CostingOfficer + ";" + resQRFPrice.SalesOfficer + ";" + commonCC;
                                builder.Replace("[#Costing Officer]", request.UserName);
                                response.Subject = "APPROVAL REQUIRED For " + request.UserName + " : QRFID : " + request.QrfId + "-" + resQRFPrice.AgentProductInfo.TourName;
                            }
                            else if (request.EnquiryPipeline.ToLower() == "costing approval pipeline")
                            {
                                builder.Replace("[#Costing Officer]", request.UserName);

                                if (request.IsApproveQuote)
                                {
                                    response.To = resQRFPrice.SalesOfficer;
                                    response.CC = resQRFPrice.CostingOfficer + ";" + resQRFPrice.ProductAccountant + ";" + commonCC;
                                    response.Subject = "COSTING APPROVED BY " + request.UserName + " : QRFID : " + request.QrfId + "-" + resQRFPrice.AgentProductInfo.TourName;
                                }
                                else
                                {
                                    response.To = resQRFPrice.CostingOfficer;
                                    response.CC = resQRFPrice.ProductAccountant + ";" + resQRFPrice.SalesOfficer + ";" + commonCC;
                                    response.Subject = "COSTING REJECTED BY " + request.UserName + " : QRFID : " + request.QrfId + "-" + resQRFPrice.AgentProductInfo.TourName;
                                }
                            }
                            else if (request.EnquiryPipeline.ToLower() == "amendment pipeline")
                            {
                                response.To = resQRFPrice.ProductAccountant;
                                response.CC = commonCC;
                                builder.Replace("[#Costing Officer]", request.UserName);
                                response.Subject = "COSTING APPROVED BY " + request.UserName + " : QRFID : " + request.QrfId + "-" + resQRFPrice.AgentProductInfo.TourName;
                            }

                            response.From = request.UserEmail;
                            response.Body = builder.ToString();
                            response.ResponseStatus = new ResponseStatus() { Status = "Success", ErrorMessage = "Mail Template created successfully." };
                            lstResponse.Add(response);
                            lstResponse[0].ResponseStatus = response.ResponseStatus;
                        }
                        else
                        {
                            response.ResponseStatus.ErrorMessage = "To Email Id can not be null/blank.";
                            response.ResponseStatus.Status = "Error";
                        }
                    }
                    else
                    {
                        response.ResponseStatus.ErrorMessage = "QRFID " + request.QrfId + " not found in mQRFPrice.";
                        response.ResponseStatus.Status = "Error";
                        lstResponse.Add(response);
                        lstResponse[0].ResponseStatus = response.ResponseStatus;
                    }
                }
                else
                {
                    response.ResponseStatus.ErrorMessage = "QRFID " + request.QrfId + " not found in mQuote.";
                    response.ResponseStatus.Status = "Error";
                    lstResponse.Add(response);
                    lstResponse[0].ResponseStatus = response.ResponseStatus;
                }
            }
            catch (System.Exception ex)
            {
                response.ResponseStatus.ErrorMessage = ex.Message;
                response.ResponseStatus.Status = "Error";
                lstResponse.Add(response);
                lstResponse[0].ResponseStatus = response.ResponseStatus;
            }
            return lstResponse;
        }
        //Reject Commercial Email
        public async Task<List<EmailTemplateGetRes>> CreateRejectCommercialEmailTemplate(EmailGetReq request, string pathToFile)
        {
            List<EmailTemplateGetRes> lstResponse = new List<EmailTemplateGetRes>();
            EmailTemplateGetRes response = new EmailTemplateGetRes();
            try
            {
                var builder = new StringBuilder();
                using (StreamReader SourceReader = File.OpenText(pathToFile))
                {
                    builder.Append(SourceReader.ReadToEnd());
                }

                var resQRFPriceList = await _MongoContext.mQRFPrice.FindAsync(a => a.QRFID == request.QrfId && a.IsCurrentVersion == true).Result.ToListAsync();

                if (resQRFPriceList != null && resQRFPriceList.Count > 0)
                {
                    var mQRFPrice = resQRFPriceList.OrderByDescending(a => a.VersionId).FirstOrDefault();

                    #region replace email content
                    builder.Replace("[#QRFID]", Convert.ToString(request.QrfId));
                    builder.Replace("[#Date]", DateTime.Now.ToString("dd MMM yyyy"));
                    builder.Replace("[#Remarks]", request.Remarks);
                    builder.Replace("[#Costing Officer]", request.UserName);
                    #endregion

                    var email = GetSmtpCredentials(request.UserEmail)?.UserName;
                    var commonCC = Encrypt.DecryptData("", email);
                    response.To = mQRFPrice.SalesOfficer;
                    response.From = request.UserEmail;
                    response.CC = commonCC + ";" + mQRFPrice.CostingOfficer;     // ?? request.UserEmail;
                    response.Body = builder.ToString();
                    response.Subject = "COSTING REJECTED BY " + request.UserName + ": QRFID : " + request.QrfId + " - " + mQRFPrice.AgentProductInfo.TourName;

                    response.ResponseStatus.Status = "Success";
                    response.ResponseStatus.ErrorMessage = "Mail Template Created Successfully.";
                }
                else
                {
                    response.ResponseStatus.Status = "Error";
                    response.ResponseStatus.ErrorMessage = "Quote Details Not Found in mQRFPrice.";
                }

                lstResponse.Add(response);
                lstResponse[0].ResponseStatus = response.ResponseStatus;
            }
            catch (System.Exception ex)
            {
                response.ResponseStatus.ErrorMessage = ex.Message;
                response.ResponseStatus.Status = "Error";
                lstResponse.Add(response);
                lstResponse[0].ResponseStatus = response.ResponseStatus;
            }
            return lstResponse;
        }
        #endregion

        #region Agent Approval Pipeline
        //Agent Approval Pipeline->Send To Client Email
        public async Task<List<EmailTemplateGetRes>> CreateSendToClientEmailTemplate(EmailGetReq request)
        {
            List<EmailTemplateGetRes> lstResponse = new List<EmailTemplateGetRes>();
            EmailTemplateGetRes response = new EmailTemplateGetRes();
            try
            {
                var resQuote = await _MongoContext.mQuote.FindAsync(a => a.QRFID == request.QrfId).Result.FirstOrDefaultAsync();

                if (resQuote != null)
                {
                    var resQRFPriceList = await _MongoContext.mQRFPrice.FindAsync(a => a.QRFID == request.QrfId && a.IsCurrentVersion == true).Result.ToListAsync();

                    if (resQRFPriceList != null && resQRFPriceList.Count > 0)
                    {
                        var resQRFPrice = resQRFPriceList.OrderByDescending(a => a.VersionId).FirstOrDefault();

                        response = new EmailTemplateGetRes();
                        request.QRFPriceId = resQRFPrice.QRFPrice_Id;
                        response.To = resQRFPrice.AgentInfo.EmailAddress;

                        if (!string.IsNullOrEmpty(response.To))
                        {
                            string strDocumentId = Guid.NewGuid().ToString();

                            string path = _configuration.GetValue<string>("UIBaseUrl") + "AgentApproval/AcceptSendToClient";
                            request.EmailHtml = request.EmailHtml.Replace("[#accept]", path + "?QRFID=" + request.QrfId + "&id=" + request.QRFPriceId + "&emailid=" + strDocumentId + "&status=accepted");

                            //string keeporiginalpath = _configuration.GetValue<string>("UIBaseUrl") + "AgentApproval/AcceptSendToClient";
                            //request.SendToClientHtml = request.SendToClientHtml.Replace("[#keeporiginal]", keeporiginalpath + "?QRFID=" + request.QRFID.ToString() + "&id=" + request.QRFPriceID + "&status=original");

                            string suggestionspath = _configuration.GetValue<string>("UIBaseUrl") + "AgentApproval/GetSuggestSendToClient";
                            request.EmailHtml = request.EmailHtml.Replace("[#suggestions]", suggestionspath + "?QRFID=" + request.QrfId + "&id=" + request.QRFPriceId + "&emailid=" + strDocumentId + "&status=suggest");

                            var email = GetSmtpCredentials(request.UserEmail)?.UserName;
                            response.CC = Encrypt.DecryptData("", email);
                            response.CC = string.IsNullOrEmpty(request.ToCC) ? response.CC : response.CC + ";" + request.ToCC;
                            response.From = request.UserEmail;
                            response.Subject = resQRFPrice.AgentProductInfo.TourName + " For " + resQRFPrice.AgentInfo.AgentName + "(" + request.QrfId + ")";
                            response.Attachment.Add(CommonFunction.FormatFileName(resQRFPrice.AgentProductInfo.TourName) + ".pdf");
                            response.Body = request.EmailHtml;
                            response.ResponseStatus = new ResponseStatus() { Status = "Success", ErrorMessage = "Mail Template created successfully." };
                            response.Document_Id = strDocumentId;
                            response.PathType = "sendtoclient";
                            lstResponse.Add(response);
                            lstResponse[0].ResponseStatus = response.ResponseStatus;
                        }
                        else
                        {
                            response.ResponseStatus.ErrorMessage = "To Email Id can not be null/blank.";
                            response.ResponseStatus.Status = "Error";
                        }
                    }
                    else
                    {
                        response.ResponseStatus.ErrorMessage = "QRFID " + request.QrfId + " not found in mQRFPrice.";
                        response.ResponseStatus.Status = "Error";
                        lstResponse.Add(response);
                        lstResponse[0].ResponseStatus = response.ResponseStatus;
                    }
                }
                else
                {
                    response.ResponseStatus.ErrorMessage = "QRFID " + request.QrfId + " not found in mQuote.";
                    response.ResponseStatus.Status = "Error";
                    lstResponse.Add(response);
                    lstResponse[0].ResponseStatus = response.ResponseStatus;
                }
            }
            catch (System.Exception ex)
            {
                response.ResponseStatus.ErrorMessage = ex.Message;
                response.ResponseStatus.Status = "Error";
                lstResponse.Add(response);
            }
            return lstResponse;
        }

        //Agent Approval Pipeline->Accept/Suggest
        public async Task<List<EmailTemplateGetRes>> CreateAcceptOrSuggestEmailTemplate(EmailGetReq request, string pathToFile)
        {
            List<EmailTemplateGetRes> lstResponse = new List<EmailTemplateGetRes>();
            EmailTemplateGetRes response = new EmailTemplateGetRes();
            try
            {
                var resQuote = await _MongoContext.mQuote.FindAsync(a => a.QRFID == request.QrfId).Result.FirstOrDefaultAsync();

                if (resQuote != null)
                {
                    var resQRFPriceList = await _MongoContext.mQRFPrice.FindAsync(a => a.QRFID == request.QrfId && a.IsCurrentVersion == true).Result.ToListAsync();

                    if (resQRFPriceList != null && resQRFPriceList.Count > 0)
                    {
                        var resQRFPrice = resQRFPriceList.OrderByDescending(a => a.VersionId).FirstOrDefault();

                        StringBuilder builder = new StringBuilder();
                        response = new EmailTemplateGetRes();
                        request.QRFPriceId = resQRFPrice.QRFPrice_Id;
                        response.To = resQRFPrice.SalesOfficer;
                        var email = GetSmtpCredentials(request.UserEmail)?.UserName;
                        response.CC = Encrypt.DecryptData("", email);
                        response.From = request.UserEmail;

                        if (!string.IsNullOrEmpty(response.To))
                        {
                            using (StreamReader SourceReader = File.OpenText(pathToFile))
                            {
                                builder.Append(SourceReader.ReadToEnd());
                            }

                            builder.Replace("[#QRFID]", Convert.ToString(request.QrfId));
                            builder.Replace("[#Date]", DateTime.Now.ToString("dd MMM yyyy"));
                            builder.Replace("[#Remarks]", request.Remarks);
                            if (request.IsUI)
                            {
                                builder.Replace("[#ClientName]", request.UserName);
                            }
                            else
                            {
                                //var contacts = _MongoContext.mCompanies.AsQueryable().Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a => a.MAIL == resQRFPrice.ProductAccountant)).FirstOrDefault().ContactDetails;
                                //var name = contacts.Where(a => a.MAIL == resQRFPrice.ProductAccountant).FirstOrDefault();

                                //builder.Replace("[#ClientName]", name.FIRSTNAME + " " + name.LastNAME);
                                var name = (resQRFPrice.AgentInfo.ContactPerson) ?? "";
                                builder.Replace("[#ClientName]", name);
                            }

                            if (request.EnquiryPipeline.ToLower() == "agent approval pipeline" && request.MailStatus == "accepted")
                            {
                                response.Subject = "Proposal Accepted FOR QRFID :" + request.QrfId + "-" + resQRFPrice.AgentProductInfo.TourName;
                                if (!string.IsNullOrEmpty(resQuote.AgentProductInfo.TourName))
                                {
                                    response.PathType = "sendtoclient";
                                    response.Attachment = new List<string>();
                                    response.Attachment.Add(CommonFunction.FormatFileName(resQRFPrice.AgentProductInfo.TourName) + ".pdf");
                                }
                            }
                            else if (request.EnquiryPipeline.ToLower() == "agent approval pipeline" && request.MailStatus == "suggest")
                            {
                                response.Subject = "Amendment Requested FOR QRFID :" + request.QrfId + "-" + resQRFPrice.AgentProductInfo.TourName;
                            }

                            response.Body = builder.ToString();
                            response.ResponseStatus = new ResponseStatus() { Status = "Success", ErrorMessage = "Mail Template created successfully." };
                            lstResponse.Add(response);
                            lstResponse[0].ResponseStatus = response.ResponseStatus;
                        }
                        else
                        {
                            response.ResponseStatus.ErrorMessage = "To Email Id can not be null/blank.";
                            response.ResponseStatus.Status = "Error";
                        }
                    }
                    else
                    {
                        response.ResponseStatus.ErrorMessage = "QRFID " + request.QrfId + " not found in mQRFPrice.";
                        response.ResponseStatus.Status = "Error";
                        lstResponse.Add(response);
                        lstResponse[0].ResponseStatus = response.ResponseStatus;
                    }
                }
                else
                {
                    response.ResponseStatus.ErrorMessage = "QRFID " + request.QrfId + " not found in mQuote.";
                    response.ResponseStatus.Status = "Error";
                    lstResponse.Add(response);
                    lstResponse[0].ResponseStatus = response.ResponseStatus;
                }
            }
            catch (System.Exception ex)
            {
                response.ResponseStatus.ErrorMessage = ex.Message;
                response.ResponseStatus.Status = "Error";
                lstResponse.Add(response);
            }
            return lstResponse;
        }

        //Agent Approval Pipeline->Accept Without Proposal
        public async Task<List<EmailTemplateGetRes>> CreateAcceptWithoutProposalEmailTemplate(EmailGetReq request, string pathToFile)
        {
            List<EmailTemplateGetRes> lstResponse = new List<EmailTemplateGetRes>();
            EmailTemplateGetRes response = new EmailTemplateGetRes();
            try
            {
                var resQuote = await _MongoContext.mQuote.FindAsync(a => a.QRFID == request.QrfId).Result.FirstOrDefaultAsync();

                if (resQuote != null)
                {
                    var resQRFPriceList = await _MongoContext.mQRFPrice.FindAsync(a => a.QRFID == request.QrfId && a.IsCurrentVersion == true).Result.ToListAsync();

                    if (resQRFPriceList != null && resQRFPriceList.Count > 0)
                    {
                        var resQRFPrice = resQRFPriceList.OrderByDescending(a => a.VersionId).FirstOrDefault();

                        StringBuilder builder = new StringBuilder();
                        response = new EmailTemplateGetRes();
                        request.QRFPriceId = resQRFPrice.QRFPrice_Id;
                        response.To = resQRFPrice.SalesOfficer;
                        var email = GetSmtpCredentials(request.UserEmail)?.UserName;
                        response.CC = Encrypt.DecryptData("", email);
                        response.From = request.UserEmail;

                        if (!string.IsNullOrEmpty(response.To))
                        {
                            using (StreamReader SourceReader = File.OpenText(pathToFile))
                            {
                                builder.Append(SourceReader.ReadToEnd());
                            }

                            builder.Replace("[#QRFID]", Convert.ToString(request.QrfId));
                            builder.Replace("[#Date]", DateTime.Now.ToString("dd MMM yyyy"));
                            builder.Replace("[#ClientName]", request.UserName);

                            if (request.EnquiryPipeline.ToLower() == "agent approval pipeline")
                            {
                                response.Subject = "Proposal Accepted FOR QRFID :" + request.QrfId + "-" + resQRFPrice.AgentProductInfo.TourName;
                            }

                            response.Body = builder.ToString();
                            response.ResponseStatus = new ResponseStatus() { Status = "Success", ErrorMessage = "Mail Template created successfully." };
                            lstResponse.Add(response);
                            lstResponse[0].ResponseStatus = response.ResponseStatus;
                        }
                        else
                        {
                            response.ResponseStatus.ErrorMessage = "To Email Id can not be null/blank.";
                            response.ResponseStatus.Status = "Error";
                        }
                    }
                    else
                    {
                        response.ResponseStatus.ErrorMessage = "QRFID " + request.QrfId + " not found in mQRFPrice.";
                        response.ResponseStatus.Status = "Error";
                        lstResponse.Add(response);
                        lstResponse[0].ResponseStatus = response.ResponseStatus;
                    }
                }
                else
                {
                    response.ResponseStatus.ErrorMessage = "QRFID " + request.QrfId + " not found in mQuote.";
                    response.ResponseStatus.Status = "Error";
                    lstResponse.Add(response);
                    lstResponse[0].ResponseStatus = response.ResponseStatus;
                }
            }
            catch (System.Exception ex)
            {
                response.ResponseStatus.ErrorMessage = ex.Message;
                response.ResponseStatus.Status = "Error";
                lstResponse.Add(response);
            }
            return lstResponse;
        }
        #endregion

        #region User
        //UserPasswordRecover Email
        public List<EmailTemplateGetRes> CreateUserPasswordRecoverEmailTemplate(EmailGetReq request, string pathToFile)
        {
            List<EmailTemplateGetRes> lstResponse = new List<EmailTemplateGetRes>();
            EmailTemplateGetRes response = new EmailTemplateGetRes();
            try
            {
                var builder = new StringBuilder();
                using (StreamReader SourceReader = File.OpenText(pathToFile))
                {
                    builder.Append(SourceReader.ReadToEnd());
                }
                if (!string.IsNullOrEmpty(request.UserEmail))
                {
                    #region replace email content
                    builder.Replace("[#Password]", request.Remarks); //request.Remarks contains Password
                    #endregion

                    //var email = GetSmtpCredentials(request.UserEmail)?.UserName;
                    response.To = request.UserEmail;
                    response.From = "voyager.support@ckdms.com"; //Encrypt.DecryptData("", email);
                    response.Body = builder.ToString();
                    response.Subject = "Forget Password Mail - Password Reminder";

                    response.ResponseStatus.Status = "Success";
                    response.ResponseStatus.ErrorMessage = "Mail Template Created Successfully.";
                }
                else
                {
                    response.ResponseStatus.Status = "Error";
                    response.ResponseStatus.ErrorMessage = "To Email Id can not be null/blank.";
                }

                lstResponse.Add(response);
                lstResponse[0].ResponseStatus = response.ResponseStatus;
            }
            catch (System.Exception ex)
            {
                response.ResponseStatus.ErrorMessage = ex.Message;
                response.ResponseStatus.Status = "Error";
                lstResponse.Add(response);
            }
            return lstResponse;
        }
        #endregion

        #region Error Report Email 
        //Error Report Email 
        public async Task<List<EmailTemplateGetRes>> CreateErrorReportEmailTemplate(EmailGetReq request, string pathToFile)
        {
            try
            {
                List<EmailTemplateGetRes> lstResponse = new List<EmailTemplateGetRes>();
                EmailTemplateGetRes response = new EmailTemplateGetRes();
                var builder = new StringBuilder();
                string Subject = "";
                string header = "";

                using (StreamReader SourceReader = File.OpenText(pathToFile))
                {
                    builder.Append(SourceReader.ReadToEnd());
                }
                if (!string.IsNullOrWhiteSpace(request.QrfId))
                {
                    var resQuote = _MongoContext.mQuote.AsQueryable().Where(a => a.QRFID == request.QrfId).FirstOrDefault();
                    if (resQuote != null)
                    {
                        var QRFPrice = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == request.QrfId && a.IsCurrentVersion == true).FirstOrDefault();
                        if (QRFPrice != null)
                        {
                            header = " REF: " + request.QrfId + " - " + QRFPrice.AgentProductInfo.TourName;
                            Subject = "Error occured on Voyager WRF - " + request.QrfId + " - " + QRFPrice.AgentProductInfo.TourName;
                        }
                    }
                }
                else
                {
                    header = request.Header;
                    Subject = request.Subject;
                }

                builder.Replace("[#header]", header);
                builder.Replace("[#Error_Description]", "Voyager " + request.ErrorDescription ?? "");
                builder.Replace("[#Source]", request.ErrorSource ?? "");
                builder.Replace("[#ErrorType]", request.ErrorCode ?? "");
                builder.Replace("[#Message]", request.ErrorMessage ?? "");
                builder.Replace("[#StackTrace]", request.ErrorStackTrace ?? "");

                var email = GetSmtpCredentials(request.UserEmail, "default")?.UserName;
                response.CC = _configuration.GetValue<string>("ToCc");
                response.Body = builder.ToString();
                response.Subject = Subject;
                response.To = _configuration.GetValue<string>("ToAddress");
                response.From = request.UserEmail;
                response.EmailGetReq = request;
                response.ResponseStatus.Status = "Success";
                response.ResponseStatus.StatusMessage = "Mail Template Created Successfully.";
                lstResponse.Add(response);

                return lstResponse;
            }
            catch (System.Exception ex)
            {
                throw;
            }
        }
        #endregion

        #region OPS Module
        //OPS Booking Confirmation
        public async Task<List<EmailTemplateGetRes>> CreateOPSBookConfirmEmailTemplate(EmailGetReq request, string pathToFile)
        {
            List<EmailTemplateGetRes> lstResponse = new List<EmailTemplateGetRes>();
            EmailTemplateGetRes response = new EmailTemplateGetRes();

            try
            {
                var builder = new StringBuilder();

                using (StreamReader SourceReader = File.OpenText(pathToFile))
                {
                    builder.Append(SourceReader.ReadToEnd());
                }

                if (!string.IsNullOrEmpty(request.BookingNo))
                {
                    var resBooking = await _MongoContext.Bookings.FindAsync(a => a.BookingNumber == request.BookingNo).Result.FirstOrDefaultAsync();
                    if (resBooking != null)
                    {
                        if (!string.IsNullOrWhiteSpace(resBooking.StaffDetails?.Staff_OpsUser_Id))
                        {
                            var ExcludeList = new List<string>() { "-", "X", "J", "C" };
                            resBooking.Positions = resBooking?.Positions?.Where(a => !ExcludeList.Contains(a.STATUS) && a.ProductType.ToLower().Trim() == "hotel").OrderBy(a => a.STARTDATE).ThenBy(a => a.STARTTIME).ToList();
                            var statusList = resBooking.Positions.Select(a => a.STATUS).ToList();
                            var posStatus = _MongoContext.mStatus.AsQueryable().Where(a => statusList.Contains(a.Status)).ToList();

                            int dur = Convert.ToInt32(resBooking.Duration);
                            string days = (dur + 1).ToString();
                            string RoomingDetails = "";

                            var mUser = await _MongoContext.mUsers.FindAsync(a => a.Contact_Id == resBooking.StaffDetails.Staff_OpsUser_Id).Result.FirstOrDefaultAsync();
                            var mCompany = await _MongoContext.mCompanies.FindAsync(a => a.Company_Id == mUser.Company_Id).Result.FirstOrDefaultAsync();
                            var resContact = mCompany.ContactDetails.Where(a => a.Contact_Id == mUser.Contact_Id).FirstOrDefault();

                            foreach (var Room in resBooking.BookingRooms)
                            {
                                if (Room != null && !string.IsNullOrEmpty(Room.SUBPROD))
                                {
                                    if (RoomingDetails != "") RoomingDetails += " ";
                                    if (Room.ROOMNO != null && Room.ROOMNO > 0)
                                    {
                                        RoomingDetails += (Room.ROOMNO + " X " + Room.SUBPROD + (!string.IsNullOrEmpty(Room.PersonType) ? " (" + Room.PersonType + ")" : "")) + ",";
                                    }
                                }
                            }

                            RoomingDetails = RoomingDetails.Trim().TrimEnd(',');
                            builder.Replace("{{BookingReferenceNumber}}", resBooking.BookingNumber);
                            builder.Replace("{{ClientTourName}}", resBooking.CustRef);
                            builder.Replace("{{TourStartDate}}", resBooking.STARTDATE != null ? resBooking.STARTDATE.Value.ToString("dd-MMM-yyyy") : "");
                            builder.Replace("{{TourEndDate}}", resBooking.ENDDATE != null ? resBooking.ENDDATE.Value.ToString("dd-MMM-yyyy") : "");
                            builder.Replace("{{TourNights}}", dur.ToString());
                            builder.Replace("{{TourDays}}", days);
                            builder.Replace("{{TourLevelRooms}}", RoomingDetails);
                            builder.Replace("{{Placer_Name}}", resContact.FIRSTNAME + " " + resContact.LastNAME);
                            builder.Replace("{{Placer_Contact_Number}}", resContact.TEL);
                            builder.Replace("{{Placer_Email}}", resBooking.StaffDetails.Staff_OpsUser_Email);

                            var pos = new StringBuilder();
                            if (resBooking?.Positions.Count > 0)
                            {
                                string Nationality = resBooking.GuestDetails.Nationality_Name;
                                string Supplement = "";

                                for (int i = 0; i < resBooking.Positions.Count; i++)
                                {
                                    var posST = posStatus.Where(a => a.Status.ToLower().Trim() == resBooking.Positions[i].STATUS.ToLower().Trim()).FirstOrDefault();
                                    RoomingDetails = "";
                                    Supplement = "";

                                    if (resBooking.Positions[i]?.BookingRoomsAndPrices?.Count > 0)
                                    {
                                        foreach (var Room in resBooking.Positions[i].BookingRoomsAndPrices)
                                        {
                                            if (Room != null && !string.IsNullOrEmpty(Room.RoomName) && !string.IsNullOrEmpty(Room.PersonType))
                                            {
                                                if (RoomingDetails != "") RoomingDetails += " ";
                                                if (Room.Req_Count != null && Room.Req_Count > 0)
                                                {
                                                    RoomingDetails += (Room.Req_Count + " X " + Room.RoomName + (!string.IsNullOrEmpty(Room.PersonType) ? " (" + Room.PersonType + ")" : "")) + ",";
                                                }
                                            }
                                        }
                                        RoomingDetails = RoomingDetails.Trim().TrimEnd(',');
                                    }
                                    else
                                    {
                                        RoomingDetails = "Rooom details not found";
                                    }

                                    if (resBooking.Positions[i]?.BudgetSupplements?.Count > 0)
                                    {
                                        var roomTWIN = resBooking.Positions[i].BudgetSupplements.Where(a => a.RoomShortCode.ToLower().Trim() == "twin").ToList();
                                        for (int j = 0; j < roomTWIN.Count; j++)
                                        {
                                            Supplement += "A Supplement of " + roomTWIN[j].BudgetSuppCurrencyName + " " + Convert.ToString(Math.Round(Convert.ToDecimal(roomTWIN[j].BudgetSupplementAmount), 2)) + " is applicable due to " + roomTWIN[j].BudgetSupplementReason;
                                        }
                                    }

                                    pos.Append("<tr>");
                                    pos.Append("<td>" + Nationality + "</td>");
                                    pos.Append("<td>" + resBooking.Positions[i].City + "</td>");
                                    pos.Append("<td>" + resBooking.Positions[i].Product_Name + "</td>");
                                    pos.Append("<td>" + resBooking.Positions[i].DURATION + "</td>");
                                    pos.Append("<td>" + resBooking.Positions[i].STARTDATE.Value.ToString("dd-MMM-yyyy") + "</td>");
                                    pos.Append("<td>" + resBooking.Positions[i].ENDDATE.Value.ToString("dd-MMM-yyyy") + "</td>");
                                    pos.Append("<td>" + resBooking.Positions[i].HOTELMEALPLAN + "</td>");
                                    pos.Append("<td>" + RoomingDetails + "</td>");
                                    pos.Append("<td>" + (posST != null ? posST.Description : resBooking.Positions[i].STATUS) + "</td>");
                                    pos.Append("<td>" + (resBooking.Positions[i].OPTIONDATE != null ? resBooking.Positions[i].OPTIONDATE.Value.ToString("dd-MMM-yyyy") : "") + "</td>");
                                    pos.Append("<td>" + Supplement + "</td>");
                                    pos.Append("</tr>");
                                }
                            }
                            else
                            {
                                pos.Append("<tr>");
                                pos.Append("<td colspan='11'>Position details not found.</td>");
                                pos.Append("</tr>");
                            }

                            builder.Replace("{{trItinerary}}", pos.ToString());

                            var email = GetSmtpCredentials(resBooking.StaffDetails.Staff_OpsUser_Email)?.UserName;
                            response.From = Encrypt.DecryptData("", email);
                            response.To = resBooking.StaffDetails.Staff_SalesUser_Email;
                            response.CC = request.UserEmail;
                            response.Subject = resBooking.BookingNumber + " - " + resBooking.CustRef + " – Hotel Confirmation";
                            response.Body = builder.ToString();
                            response.Importance = "High";

                            response.ResponseStatus.Status = "Success";
                            response.ResponseStatus.ErrorMessage = "Mail Template Created Successfully.";
                        }
                        else
                        {
                            response.ResponseStatus.Status = "Error";
                            response.ResponseStatus.ErrorMessage = "Staff_OpsUser_Id can not be null/empty.";
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Error";
                        response.ResponseStatus.ErrorMessage = "Booking details not found.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Error";
                    response.ResponseStatus.ErrorMessage = "Booking number can not be null/empty.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Error";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }

            lstResponse.Add(response);
            lstResponse[0].ResponseStatus = response.ResponseStatus;
            return lstResponse;
        }

        //OPS Position Amendment
        public async Task<List<EmailTemplateGetRes>> CreateOPSPositionAmendmentEmailTemplate(EmailGetReq request, string pathToFile)
        {
            List<EmailTemplateGetRes> lstResponse = new List<EmailTemplateGetRes>();
            EmailTemplateGetRes response = new EmailTemplateGetRes();
            try
            {
                if (!string.IsNullOrEmpty(request.BookingNo))
                {
                    var resBooking = await _MongoContext.Bookings.FindAsync(a => a.BookingNumber == request.BookingNo).Result.FirstOrDefaultAsync();
                    if (resBooking != null)
                    {
                        if (request.PositionIds==null || request.PositionIds?.Count == 0) request.PositionIds = new List<string>() { request.PositionId };

                        var positions = resBooking.Positions.Where(a => request.PositionIds.Contains(a.Position_Id)).ToList();
                        var productIds = positions.Select(a => a.Product_Id).ToList();
                        var ProductSrp = _MongoContext.mProducts_Lite.AsQueryable().Where(a => productIds.Contains(a.VoyagerProduct_Id)).ToList();
                        var mUser = await _MongoContext.mUsers.FindAsync(a => a.VoyagerUser_Id == request.PlacerUserId).Result.FirstOrDefaultAsync();
                        var mCompany = await _MongoContext.mCompanies.FindAsync(a => a.Company_Id == mUser.Company_Id).Result.FirstOrDefaultAsync();
                        var resContact = mCompany.ContactDetails.Where(a => a.Contact_Id == mUser.Contact_Id).FirstOrDefault();
                        string supplierEmail = "";
                        string centralMailBox = "";
                        int days = 0;
                        var objProductSrp = new mProducts_Lite();

                        if (positions?.Count > 0)
                        {
                            if (resContact != null)
                            {
                                var email = GetSmtpCredentials(request.UserEmail)?.UserName;
                                string configMail = Encrypt.DecryptData("", email);
                                var contactMails = mCompany.ContactDetails?.Where(a => a.IsCentralEmail == true && !string.IsNullOrWhiteSpace(a.MAIL)).Select(a => a.MAIL).ToList();
                                if (contactMails?.Count > 0)
                                {
                                    centralMailBox = string.Join(";", contactMails);
                                }

                                foreach (var position in positions)
                                {
                                    response = new EmailTemplateGetRes();
                                    var builder = new StringBuilder();
                                    objProductSrp = new mProducts_Lite();
                                    supplierEmail = position.SupplierInfo.Contact_Email;

                                    using (StreamReader SourceReader = File.OpenText(pathToFile))
                                    {
                                        builder.Append(SourceReader.ReadToEnd());
                                    }

                                    days = Convert.ToInt32(position.DURATION) + 1;
                                    objProductSrp = ProductSrp.Where(a => a.VoyagerProduct_Id == position.Product_Id).FirstOrDefault();

                                    #region replace email content
                                    builder.Replace("{{Product_Supplier_Contact_Details_for_Groups}}", position.SupplierInfo.Contact_Name);
                                    builder.Replace("{{Hotel_Name}}", objProductSrp.ProdName);
                                    builder.Replace("{{Hotel_Address_Line_1}}", string.IsNullOrWhiteSpace(objProductSrp.Address) ? "" : (objProductSrp.Address + ",</br>"));
                                    builder.Replace("{{Hotel_City}}", objProductSrp.CityName);
                                    builder.Replace("{{Hotel_Country}}", objProductSrp.CountryName);
                                    builder.Replace("{{Our_Booking_Reference_Number}}", resBooking.BookingNumber);
                                    builder.Replace("{{Client_Tour_Name}}", resBooking.CustRef);
                                    builder.Replace("{{Check_In_Date}}", Convert.ToDateTime(position.STARTDATE).ToString("dd-MMM-yyyy"));
                                    builder.Replace("{{Check_Out_Date}}", Convert.ToDateTime(position.ENDDATE).ToString("dd-MMM-yyyy"));
                                    builder.Replace("{{Position_Duration}}", position.DURATION);
                                    builder.Replace("{{Placer_Name}}", resContact.FIRSTNAME + " " + resContact.LastNAME);
                                    builder.Replace("{{Placer_Contact_Number}}", resContact.TEL);
                                    builder.Replace("{{Placer_Email}}", resContact.MAIL);

                                    var rooms = new StringBuilder();
                                    if (position.BookingRoomsAndPrices != null && position.BookingRoomsAndPrices.Count > 0)
                                    {
                                        for (int a = 0; a < position.BookingRoomsAndPrices.Count; a++)
                                        {
                                            var persontype = position.BookingRoomsAndPrices[a].PersonType; if (!string.IsNullOrWhiteSpace(persontype)) { persontype = "(" + persontype + ")"; } else { persontype = ""; }
                                            var categoryname = (position.BookingRoomsAndPrices[a].CategoryName == null ? "" : position.BookingRoomsAndPrices[a].CategoryName.ToUpper()) + " " + position.BookingRoomsAndPrices[a].RoomName + " " + persontype;
                                            rooms.Append("<tr>");
                                            rooms.Append("<td>" + categoryname + "</td>");
                                            rooms.Append("<td>" + position.BookingRoomsAndPrices[a].Req_Count + "</td>");
                                            rooms.Append("<td>" + position.BookingRoomsAndPrices[a].BuyPrice + "(" + position.BookingRoomsAndPrices[a].BuyCurrency_Name + ")" + "</td>");
                                            rooms.Append("<td>" + position.HOTELMEALPLAN + "</td>");
                                            if (a == 0)
                                            {
                                                var interconnectrooms = position.InterConnectingRooms != null ? "InterConnecting Rooms : " + position.InterConnectingRooms + "<br>" : "";
                                                var washchngeroom = (position.WashChangeRoom != null && position.WashChangeRoom > 0) ? "Wash and Change Rooms : " + position.WashChangeRoom + "<br>" : "";
                                                var latecheckout = position.LateCheckout != null ? "<br> Late Check out : " + position.LateCheckout : "";
                                                rooms.Append("<td rowspan=" + position.BookingRoomsAndPrices.Count + ">" + interconnectrooms + washchngeroom + latecheckout + "<br></td>");
                                                rooms.Append("<td rowspan=" + position.BookingRoomsAndPrices.Count + ">" + position.Special_Requests + "</td>");
                                            }
                                            rooms.Append("</tr>");
                                        }
                                    }
                                    else
                                    {
                                        rooms.Append("<tr>");
                                        rooms.Append("<td></td>");
                                        rooms.Append("<td></td>");
                                        rooms.Append("<td></td>");
                                        rooms.Append("<td></td>");
                                        rooms.Append("<td></td>");
                                        rooms.Append("<td></td>");
                                        rooms.Append("</tr>");
                                    }
                                    builder.Replace("{{BookingRooms}}", rooms.ToString());
                                    #endregion

                                    response.Subject = "BOOKING AMENDMENT - " + resBooking.BookingNumber + " - " + resBooking.CustRef + " – ("
                                                        + Convert.ToDateTime(position.STARTDATE).ToString("dd-MMM-yyyy") + " - " + Convert.ToDateTime(position.ENDDATE).ToString("dd-MMM-yyyy") + ")";

                                    response.From = resBooking.StaffDetails.Staff_OpsUser_Email;
                                    response.CC = !string.IsNullOrWhiteSpace(centralMailBox) ? centralMailBox + ";" + configMail : configMail;
                                    response.To = supplierEmail;
                                    response.Body = builder.ToString();
                                    response.SupplierId = position.SupplierInfo.Id;
                                    response.EmailGetReq.PositionId = position.Position_Id;

                                    response.ResponseStatus.Status = "Success";
                                    response.ResponseStatus.ErrorMessage = "Mail Template Created Successfully.";

                                    lstResponse.Add(response);
                                }
                            }
                            else
                            {
                                response.ResponseStatus.Status = "Error";
                                response.ResponseStatus.ErrorMessage = "Contact Details Not Found";
                                lstResponse.Add(response);
                                lstResponse[0].ResponseStatus = response.ResponseStatus;
                            }
                        }
                        else
                        {
                            response.ResponseStatus.Status = "Error";
                            response.ResponseStatus.ErrorMessage = "Position Details Not Found";
                            lstResponse.Add(response);
                            lstResponse[0].ResponseStatus = response.ResponseStatus;
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Error";
                        response.ResponseStatus.ErrorMessage = "Booking details not found";
                        lstResponse.Add(response);
                        lstResponse[0].ResponseStatus = response.ResponseStatus;
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Error";
                    response.ResponseStatus.ErrorMessage = "Booking number can not be null/empty.";
                    lstResponse.Add(response);
                    lstResponse[0].ResponseStatus = response.ResponseStatus;
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Error";
                response.ResponseStatus.ErrorMessage = ex.Message;
                lstResponse.Add(response);
                lstResponse[0].ResponseStatus = response.ResponseStatus;
            }

            return lstResponse;
        }
        #endregion

        #endregion

        #region Helper Methods
        public StringBuilder HotelRequestEmailDetails(SupplierContactDetails objSupplierContactDetails, Bookings resBooking, Positions Position, CompanyContacts resContact, string WebURLInitial, string RoomingDetails, StringBuilder builder)
        {
            builder.Replace("{{CUSTOMER_NAME}}", objSupplierContactDetails.SupplierName);
            builder.Replace("{{HOTEL_NAME}}", objSupplierContactDetails.ProdName);
            builder.Replace("{{HOTEL_ADDRESS}}", objSupplierContactDetails.Address);
            builder.Replace("{{HOTEL_CITY}}", objSupplierContactDetails.City);
            builder.Replace("{{HOTEL_COUNTRY}}", objSupplierContactDetails.Country);

            builder.Replace("{{Booking_Number}}", resBooking.BookingNumber);
            builder.Replace("{{Tour_Name}}", resBooking.CustRef);
            builder.Replace("{{Nationality}}", resBooking.GuestDetails.Nationality_Name);
            builder.Replace("{{City_Name}}", Position.City);
            builder.Replace("{{No_Of_Nights}}", Position.DURATION);
            builder.Replace("{{Check_In_Date}}", Convert.ToDateTime(Position.STARTDATE).ToString("dd-MMM-yyyy"));
            builder.Replace("{{Check_Out_Date}}", Convert.ToDateTime(Position.ENDDATE).ToString("dd-MMM-yyyy"));
            builder.Replace("{{Meal_Plan}}", Position.HOTELMEALPLAN);
            builder.Replace("{{Rooming_Details}}", RoomingDetails);
            builder.Replace("{{Placer_Name}}", resContact.FIRSTNAME + " " + resContact.LastNAME);
            builder.Replace("{{Placer_Contact_Number}}", resContact.TEL);
            builder.Replace("{{Placer_Email}}", resContact.MAIL);
            builder.Replace("{{Position_Id}}", Position.Position_Id);
            builder.Replace("{{Alt_Svc_Id}}", objSupplierContactDetails.AltSvcId);
            builder.Replace("{{WebURLInitial}}", WebURLInitial);

            return builder;
        }
        #endregion
    }
}
