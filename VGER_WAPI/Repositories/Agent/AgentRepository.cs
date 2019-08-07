using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI.Helpers;
using VGER_WAPI.Models;
using VGER_WAPI.Providers;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Repositories
{
    public class AgentRepository : IAgentRepository
    {
        #region Private Variable Declaration
        private readonly IConfiguration _configuration;
        private readonly MongoContext _MongoContext = null;
        private readonly IGenericRepository _GenericRepository;
        private AgentProviders agentProviders;
        #endregion

        public AgentRepository(IOptions<MongoSettings> settings, IGenericRepository GenericRepository, IConfiguration configuration)
        {
            _configuration = configuration;
            _MongoContext = new MongoContext(settings);
            _GenericRepository = GenericRepository;
            agentProviders = new AgentProviders(configuration);
        }

        public async Task<AgentGetRes> GetAgentDetails(AgentGetReq request)
        {
            AgentGetRes response = new AgentGetRes();
            try
            {
                var UserCompany_Id = _MongoContext.mUsers.AsQueryable().Where(x => x.VoyagerUser_Id == request.UserId).Select(y => y.Company_Id).FirstOrDefault();
                var AdminRoleId = _MongoContext.mRoles.AsQueryable().Where(x => x.LoweredRoleName == "administrator").Select(y => y.Voyager_Role_Id).FirstOrDefault();
                var AdminRole = _MongoContext.mUsersInRoles.AsQueryable().Where(x => x.UserId == request.UserId && x.RoleId == AdminRoleId).FirstOrDefault();

                FilterDefinition<mCompanies> filter;
                filter = Builders<mCompanies>.Filter.Empty;
                if (AdminRole == null)//means user is not an Admin
                {
                    var CoreCompany_Id = _MongoContext.mSystem.AsQueryable().Select(y => y.CoreCompany_Id).FirstOrDefault();
                    if (UserCompany_Id == CoreCompany_Id)
                    {
                        if (!string.IsNullOrWhiteSpace(CoreCompany_Id))
                        {
                            filter = filter & Builders<mCompanies>.Filter.Where(x => x.Company_Id != CoreCompany_Id);
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(UserCompany_Id))
                        {
                            filter = filter & Builders<mCompanies>.Filter.Where(x => x.ParentAgent_Id == UserCompany_Id);
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(request.CompanyId))
                {
                    filter = filter & Builders<mCompanies>.Filter.Where(x => x.Company_Id == request.CompanyId);
                }
                if (!string.IsNullOrWhiteSpace(request.AgentName))
                {
                    filter = filter & Builders<mCompanies>.Filter.Where(x => x.Name.Trim().ToLower().Contains(request.AgentName.Trim().ToLower()));
                }
                if (!string.IsNullOrWhiteSpace(request.AgentReference))
                {
                    filter = filter & Builders<mCompanies>.Filter.Where(x => x.CompanyCode.ToLower().Contains(request.AgentReference.Trim().ToLower()));
                }
                if (request.CountryId.HasValue && !(request.CountryId.Value == Guid.Empty))
                {
                    filter = filter & Builders<mCompanies>.Filter.Where(x => x.Country_Id == Convert.ToString(request.CountryId));
                }
                if (!string.IsNullOrWhiteSpace(request.Status) && request.Status.ToLower() == "inactive")
                {
                    filter = filter & Builders<mCompanies>.Filter.Where(x => x.STATUS != " " && x.STATUS != "" && x.STATUS != null);
                }
                else
                {
                    filter = filter & Builders<mCompanies>.Filter.Where(x => x.STATUS != "X" && x.STATUS != "-");
                }

                filter = filter & Builders<mCompanies>.Filter.Where(x => (x.Iscustomer == true || x.Issubagent == true));

                var result = await _MongoContext.mCompanies.Find(filter).Sort("{Name: 1}").Skip(request.Start).Limit(request.Length).Project(x => new AgentList
                {
                    CompanyId = x.Company_Id,
                    CityId = x.City_Id,
                    City = x.CityName,
                    CountryId = x.Country_Id,
                    Country = x.CountryName,
                    Code = x.CompanyCode,
                    Name = x.Name,
                    IsSupplier = x.Issupplier
                }).ToListAsync();

                if (result.Count > 0)
                {
                    response.AgentTotalCount = Convert.ToInt32(_MongoContext.mCompanies.Find(filter).Count());
                }
                response.AgentList = result;//.OrderBy(x => x.Name).ToList();							

                return response;
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
            }
            return response;
        }

        public async Task<AgentGetRes> GetAgentDetailedInfo(AgentGetReq request)
        {
            try
            {
                AgentGetRes getResponse = new AgentGetRes();
                mCompanies response = new mCompanies();
                response = _MongoContext.mCompanies.AsQueryable().Where(x => x.Company_Id == request.CompanyId).FirstOrDefault();

                if (response != null && response.Products != null)
                {
                    if (response.Products.Count > 0)
                    {
                        getResponse.ProductsTotalCount = response.Products.Count();
                    }

                    if (request.Length == 0) request.Length = 10;
                    response.Products = response.Products.OrderBy(x => x.ProductName).Skip(request.Start).Take(request.Length).ToList();
                }

                if (response?.Issupplier == null || response?.Issupplier == false)
                {
                    var CoreCompanyId = _MongoContext.mSystem.AsQueryable().Select(x => x.CoreCompany_Id).FirstOrDefault();

                    if (!string.IsNullOrEmpty(CoreCompanyId))
                    {
                        var comp = _MongoContext.mCompanies.AsQueryable().Where(x => x.HeadOffice_Id == CoreCompanyId).ToList();
                        if (comp.Count > 0)
                        {
                            var saledOffices = comp.Select(a => new ChildrenCompanies { Company_Id = a.Company_Id, Company_Name = a.Name, Company_Code = a.CompanyCode, ParentCompany_Id = a.ParentAgent_Id }).ToList();
                            response.SalesOffices = saledOffices;
                        }
                    }
                }
                getResponse.AgentDetails = response;
                return getResponse;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<AgentSetRes> SetAgentDetailedInfo(AgentSetReq request)
        {
            AgentSetRes response = new AgentSetRes();
            CompanyOfficerGetRes getresponse = new CompanyOfficerGetRes();

            try
            {
                mCompanies companies = new mCompanies();
                string SystemCompany_Id = string.Empty;

                if (string.IsNullOrEmpty(request.companies.Company_Id))
                {
                    var lstAgents = _MongoContext.mCompanies.AsQueryable().Where(x => x.Name.ToLower() == request.companies.Name.ToLower() && x.Country_Id == request.companies.Country_Id && x.City_Id == request.companies.City_Id).ToList();

                    if (lstAgents.Count() > 0)
                    {
                        response.ResponseStatus.Status = "Error";
                        response.ResponseStatus.ErrorMessage = "Record already exists";
                    }
                    else
                    {
                        //Jira 853 - When Agent is created, automatically assign Sales Officer to it
                        if (request.companies.Issupplier == false)
                        {
                            var loggedInUserCompanyId = request.companies.ParentAgent_Id;
                            getresponse = GetSalesOfficesOfSystemCompany().Result;
                            if (getresponse != null && getresponse.Branches != null && getresponse.Branches.Count > 0 && !string.IsNullOrWhiteSpace(loggedInUserCompanyId))
                            {
                                bool flag = getresponse.Branches.Any(x => x.Company_Id == request.companies.ParentAgent_Id);
                                if (flag)
                                {
                                    companies.ParentAgent_Id = request.companies.ParentAgent_Id;
                                    companies.ParentAgent_Name = request.companies.ParentAgent_Name;
                                }
                            }
                        }

                        //Add
                        companies.Company_Id = Guid.NewGuid().ToString();
                        companies.Name = request.companies.Name;
                        companies.CompanyCode = request.companies.CompanyCode;
                        companies.Issupplier = request.companies.Issupplier == true ? true : false;
                        companies.Iscustomer = request.companies.Issupplier == true ? false : true;
                        companies.Issubagent = false;
                        companies.Street = request.companies.Street;
                        companies.Street2 = request.companies.Street2;
                        companies.Street3 = request.companies.Street3;
                        companies.Zipcode = request.companies.Zipcode;
                        companies.CityName = request.companies.CityName;
                        companies.CountryName = request.companies.CountryName;
                        companies.Country_Id = request.companies.Country_Id;
                        companies.City_Id = request.companies.City_Id;
                        companies.STATUS = " ";
                        companies.ContactBy = request.companies.ContactBy;
                        companies.CreateUser = request.companies.CreateUser;
                        companies.CreateDate = DateTime.Now;

                        if (!string.IsNullOrWhiteSpace(request.LoggedInUserContactId) && companies.Iscustomer == true)
                        {
                            GetSystemCompany(request.LoggedInUserContactId, out SystemCompany_Id);
                        }
                        companies.SystemCompany_Id = SystemCompany_Id;

                        //Add default contact 
                        if (request.companies.ContactDetails.Count > 0)
                        {
                            CompanyContacts newContact = new CompanyContacts();
                            newContact.Contact_Id = Guid.NewGuid().ToString();
                            newContact.Company_Id = companies.Company_Id;
                            newContact.Company_Name = request.companies.Name;
                            newContact.Default = 1;
                            newContact.ActualCompany_Name_AsShared = request.companies.Name;
                            newContact.CommonTitle = request.companies.ContactDetails[0].CommonTitle;
                            newContact.FIRSTNAME = request.companies.ContactDetails[0].FIRSTNAME;
                            newContact.LastNAME = request.companies.ContactDetails[0].LastNAME;
                            newContact.TEL = request.companies.ContactDetails[0].TEL;
                            newContact.MAIL = request.companies.ContactDetails[0].MAIL;
                            newContact.UserName = request.companies.ContactDetails[0].MAIL;
                            newContact.STATUS = " ";
                            newContact.CreateUser = request.companies.ContactDetails[0].CreateUser;
                            newContact.CreateDate = DateTime.Now;
                            companies.ContactDetails.Add(newContact);

                            response.CompanyId = companies.Company_Id;
                            response.ContactId = companies.ContactDetails[0].Contact_Id;
                            response.CompanyCode = companies.CompanyCode;
                            await _MongoContext.mCompanies.InsertOneAsync(companies);
                            response.ResponseStatus.Status = "Success";
                            response.ResponseStatus.StatusMessage = "Record Saved Successfully.";
                        }
                    }
                }
                else
                {
                    //Update
                    companies = _MongoContext.mCompanies.AsQueryable().Where(x => x.Company_Id == request.companies.Company_Id).FirstOrDefault();

                    if (companies != null)
                    {
                        if (request.companies != null)
                        {
                            if (request.IsCompany)
                            {
                                companies.Name = request.companies.Name ?? companies.Name;
                                companies.CompanyCode = request.companies.CompanyCode ?? companies.CompanyCode;
                                companies.Issupplier = request.companies.Issupplier ?? companies.Issupplier;
                                companies.Iscustomer = request.companies.Iscustomer ?? companies.Iscustomer;
                                companies.Issubagent = request.companies.Issubagent ?? companies.Issubagent;
                                companies.Street = request.companies.Street ?? companies.Street;
                                companies.Street2 = request.companies.Street2 ?? companies.Street2;
                                companies.Street3 = request.companies.Street3 ?? companies.Street3;
                                companies.Zipcode = request.companies.Zipcode ?? companies.Zipcode;
                                companies.CityName = request.companies.CityName;
                                companies.CountryName = request.companies.CountryName;
                                companies.Country_Id = request.companies.Country_Id;
                                companies.City_Id = request.companies.City_Id;
                                companies.ParentAgent_Id = request.companies.ParentAgent_Id;
                                companies.ParentAgent_Name = request.companies.ParentAgent_Name;
                                companies.DefaultMarkup_Id = request.companies.DefaultMarkup_Id ?? companies.DefaultMarkup_Id;
                                companies.B2b2bmarkup_Id = request.companies.B2b2bmarkup_Id ?? companies.B2b2bmarkup_Id;
                                companies.LogoFilePath = request.companies.LogoFilePath ?? companies.LogoFilePath;
                                companies.HeadOffice_Id = request.companies.HeadOffice_Id ?? companies.HeadOffice_Id;
                                companies.HeadOffice_Name = request.companies.HeadOffice_Name ?? companies.HeadOffice_Name;
                                companies.AutoDelivery = request.companies.AutoDelivery ?? companies.AutoDelivery;
                                companies.AutoGenerate = request.companies.AutoGenerate ?? companies.AutoGenerate;
                                companies.VATAPPLICABLE = request.companies.VATAPPLICABLE ?? companies.VATAPPLICABLE;
                                companies.STATUS = request.companies.STATUS ?? " ";
                                companies.B2C = request.companies.B2C;
                                companies.ISUSER = request.companies.ISUSER;
                                companies.ContactBy = request.companies.ContactBy;
                                companies.EditUser = request.companies.EditUser ?? "";
                                companies.EditDate = DateTime.Now;
                                response.CompanyCode = companies.CompanyCode;

                                ReplaceOneResult replaceResult = await _MongoContext.mCompanies.ReplaceOneAsync(Builders<mCompanies>.Filter.Eq("Company_Id", companies.Company_Id), companies);
                                response.ResponseStatus.Status = replaceResult.MatchedCount > 0 ? "Success" : "Failure";
                                response.ResponseStatus.ErrorMessage = replaceResult.MatchedCount > 0 ? "Saved Successfully." : "Details not updated.";
                            }
                            if (request.IsSystemUser)
                            {
                                companies.ISUSER = request.companies.ISUSER;
                                companies.ContactBy = request.companies.ContactBy;
                                companies.EditUser = request.companies.EditUser ?? "";
                                companies.EditDate = DateTime.Now;

                                var resultUSer = _MongoContext.mCompanies.FindOneAndUpdate(Builders<mCompanies>.Filter.Eq("Company_Id", companies.Company_Id),
                                 Builders<mCompanies>.Update.Set("ISUSER", companies.ISUSER).Set("ContactBy", companies.ContactBy)
                                  .Set("EditUser", companies.EditUser).Set("EditDate", companies.EditDate));
                            }
                        }
                        if (request.companies.AccountDetails != null && request.companies.AccountDetails.Count > 0)
                        {
                            if (companies.AccountDetails == null) companies.AccountDetails = new List<CompanyAccounts>();
                            if (companies.AccountDetails.Count < 1) companies.AccountDetails.Add(new CompanyAccounts());

                            companies.AccountDetails[0].Name = request.companies.AccountDetails[0].Name ?? companies.AccountDetails[0].Name;
                            companies.AccountDetails[0].VATNumber = request.companies.AccountDetails[0].VATNumber ?? companies.AccountDetails[0].VATNumber;
                            companies.AccountDetails[0].FinanceContact = request.companies.AccountDetails[0].FinanceContact;
                            companies.AccountDetails[0].AccountNumber = request.companies.AccountDetails[0].AccountNumber;
                            companies.AccountDetails[0].AccountName = request.companies.AccountDetails[0].AccountName;
                            companies.AccountDetails[0].BankName = request.companies.AccountDetails[0].BankName;
                            companies.AccountDetails[0].BankAddress = request.companies.AccountDetails[0].BankAddress;
                            companies.AccountDetails[0].SortCode = request.companies.AccountDetails[0].SortCode;
                            companies.AccountDetails[0].IBAN = request.companies.AccountDetails[0].IBAN;
                            companies.AccountDetails[0].Swift = request.companies.AccountDetails[0].Swift;
                            companies.AccountDetails[0].FinanceNote = request.companies.AccountDetails[0].FinanceNote;

                            await _MongoContext.mCompanies.UpdateOneAsync(Builders<mCompanies>.Filter.Eq("Company_Id", companies.Company_Id),
                            Builders<mCompanies>.Update.Set("AccountDetails", companies.AccountDetails)
                            .Set("EditUser", request.EditUser).Set("EditDate", DateTime.Now));
                        }
                        if (request.companies.ContactDetails != null && request.companies.ContactDetails.Count > 0)
                        {
                            if (request.companies.ContactDetails[0].Default == 1)
                            {
                                companies.ContactDetails.ForEach(x => x.Default = 2);
                            }

                            string HashedPassword = ""; string password = "";
                            if (request.IsNewContact)
                            {
                                CompanyContacts newContact = new CompanyContacts();
                                newContact.Contact_Id = Guid.NewGuid().ToString();
                                newContact.Company_Id = request.companies.ContactDetails[0].Company_Id;
                                newContact.Company_Name = request.companies.ContactDetails[0].Company_Name;
                                newContact.Default = request.companies.ContactDetails[0].Default;
                                newContact.ActualCompany_Id_AsShared = request.companies.ContactDetails[0].ActualCompany_Id_AsShared;
                                newContact.ActualContact_Id_AsShared = request.companies.ContactDetails[0].ActualContact_Id_AsShared;
                                newContact.ActualCompany_Name_AsShared = request.companies.ContactDetails[0].ActualCompany_Name_AsShared;
                                newContact.CommonTitle = request.companies.ContactDetails[0].CommonTitle;
                                newContact.TITLE = request.companies.ContactDetails[0].TITLE;
                                newContact.FIRSTNAME = request.companies.ContactDetails[0].FIRSTNAME;
                                newContact.LastNAME = request.companies.ContactDetails[0].LastNAME;
                                newContact.TEL = request.companies.ContactDetails[0].TEL;
                                newContact.MOBILE = request.companies.ContactDetails[0].MOBILE;
                                newContact.FAX = request.companies.ContactDetails[0].FAX;
                                newContact.MAIL = request.companies.ContactDetails[0].MAIL;
                                newContact.WEB = request.companies.ContactDetails[0].WEB;
                                newContact.DEPARTMENT = request.companies.ContactDetails[0].DEPARTMENT;
                                newContact.StartPage_Id = request.companies.ContactDetails[0].StartPage_Id;
                                newContact.Start_Page = request.companies.ContactDetails[0].Start_Page;
                                newContact.Systemuser_id = request.companies.ContactDetails[0].Systemuser_id;
                                newContact.UserName = request.companies.ContactDetails[0].UserName ?? request.companies.ContactDetails[0].MAIL;
                                newContact.Password = request.companies.ContactDetails[0].Password;

                                password = request.companies.ContactDetails[0].Password;
                                if (!string.IsNullOrEmpty(password))
                                    HashedPassword = Encrypt.Sha256encrypt(password);

                                newContact.Password = HashedPassword;
                                newContact.PasswordSalt = request.companies.ContactDetails[0].PasswordSalt;
                                newContact.PasswordQuestion = request.companies.ContactDetails[0].PasswordQuestion;
                                newContact.PasswordAnswer = request.companies.ContactDetails[0].PasswordAnswer;
                                newContact.PasswordFormat = request.companies.ContactDetails[0].PasswordFormat;
                                newContact.IsApproved = request.companies.ContactDetails[0].IsApproved;
                                newContact.IsLockedOut = request.companies.ContactDetails[0].IsLockedOut;
                                newContact.STATUS = request.companies.ContactDetails[0].STATUS;
                                newContact.ContactOrder = request.companies.ContactDetails[0].ContactOrder;
                                newContact.IsOperationDefault = request.companies.ContactDetails[0].IsOperationDefault;
                                newContact.IsCentralEmail = request.companies.ContactDetails[0].IsCentralEmail;
                                newContact.CreateUser = request.companies.ContactDetails[0].CreateUser;
                                newContact.CreateDate = DateTime.Now;
                                companies.ContactDetails.Add(newContact);
                                response.ContactId = newContact.Contact_Id;
                                response.UserName = newContact.MAIL;
                            }
                            else
                            {
                                foreach (var contact in companies.ContactDetails)
                                {
                                    foreach (var newContact in request.companies.ContactDetails)
                                    {
                                        if (contact.Contact_Id == newContact.Contact_Id)
                                        {
                                            contact.Company_Id = newContact.Company_Id ?? contact.Company_Id;
                                            contact.Company_Name = newContact.Company_Name ?? contact.Company_Name;
                                            contact.ActualCompany_Id_AsShared = newContact.ActualCompany_Id_AsShared ?? contact.ActualCompany_Id_AsShared;
                                            contact.ActualCompany_Name_AsShared = newContact.ActualCompany_Name_AsShared ?? contact.ActualCompany_Name_AsShared;
                                            contact.ActualContact_Id_AsShared = newContact.ActualContact_Id_AsShared ?? contact.ActualContact_Id_AsShared;
                                            contact.CommonTitle = newContact.CommonTitle ?? contact.CommonTitle;
                                            contact.TITLE = newContact.TITLE ?? contact.TITLE;
                                            contact.FIRSTNAME = newContact.FIRSTNAME ?? contact.FIRSTNAME;
                                            contact.LastNAME = newContact.LastNAME ?? contact.LastNAME;
                                            contact.TEL = newContact.TEL ?? contact.TEL;
                                            contact.MOBILE = newContact.MOBILE ?? contact.MOBILE;
                                            contact.FAX = newContact.FAX ?? contact.FAX;
                                            contact.MAIL = newContact.MAIL ?? contact.MAIL;
                                            contact.WEB = newContact.WEB ?? contact.WEB;
                                            contact.DEPARTMENT = newContact.DEPARTMENT ?? contact.DEPARTMENT;
                                            contact.STATUS = newContact.STATUS ?? " ";
                                            contact.Systemuser_id = newContact.Systemuser_id ?? contact.Systemuser_id;
                                            contact.ContactOrder = newContact.ContactOrder ?? contact.ContactOrder;
                                            contact.IsOperationDefault = newContact.IsOperationDefault;
                                            contact.IsCentralEmail = newContact.IsCentralEmail;
                                            contact.EditUser = newContact.EditUser;
                                            contact.EditDate = DateTime.Now;
                                            contact.UserName = newContact.UserName ?? contact.UserName;
                                            contact.PasswordText = newContact.PasswordText ?? contact.PasswordText;

                                            password = contact.PasswordText;
                                            if (!string.IsNullOrEmpty(password))
                                                HashedPassword = Encrypt.Sha256encrypt(password);
                                            contact.Password = HashedPassword;

                                            if (newContact.FIRSTNAME != null)
                                            {
                                                contact.Default = newContact.Default ?? contact.Default;
                                                contact.StartPage_Id = newContact.StartPage_Id;
                                                contact.Start_Page = newContact.Start_Page;
                                            }

                                            //if (!string.IsNullOrEmpty(contact.UserName))
                                            //{
                                            //    contact.PasswordText = newContact.Password;
                                            //    password = newContact.Password;
                                            //    if (!string.IsNullOrEmpty(password))
                                            //        HashedPassword = Encrypt.Sha256encrypt(password);
                                            //    contact.Password = HashedPassword;
                                            //}
                                            //contact.PasswordText = string.IsNullOrEmpty(contact.UserName) ? contact.PasswordText : newContact.Password;

                                            //password = newContact.Password;
                                            //if (!string.IsNullOrEmpty(password))
                                            //    HashedPassword = Encrypt.Sha256encrypt(password);

                                            //contact.Password = string.IsNullOrEmpty(contact.UserName) ? contact.Password : HashedPassword;
                                            contact.PasswordSalt = newContact.PasswordSalt ?? contact.PasswordSalt;
                                            contact.PasswordQuestion = newContact.PasswordQuestion ?? contact.PasswordQuestion;
                                            contact.PasswordAnswer = newContact.PasswordAnswer ?? contact.PasswordAnswer;
                                            contact.PasswordFormat = newContact.PasswordFormat ?? contact.PasswordFormat;
                                            contact.IsApproved = newContact.IsApproved ?? contact.IsApproved;
                                            contact.IsLockedOut = newContact.IsLockedOut ?? contact.IsLockedOut;
                                            response.CompanyId = newContact.Company_Id;
                                            response.ContactId = newContact.Contact_Id;
                                            response.UserName = contact.UserName;
                                        }
                                    }
                                }
                            }

                            await _MongoContext.mCompanies.UpdateOneAsync(Builders<mCompanies>.Filter.Eq("Company_Id", companies.Company_Id),
                            Builders<mCompanies>.Update.Set("ContactDetails", companies.ContactDetails)
                            .Set("EditUser", request.EditUser).Set("EditDate", DateTime.Now));
                        }
                        if (request.companies.TermsAndConditions != null && request.companies.TermsAndConditions.Count > 0)
                        {

                            if (request.IsNewTermAndCondition)
                            {
                                CompanyTerms newObj = new CompanyTerms();
                                newObj.TermsAndConditions_Id = Guid.NewGuid().ToString();
                                newObj.Company_Id = request.companies.TermsAndConditions[0].Company_Id;
                                newObj.DocumentType = request.companies.TermsAndConditions[0].DocumentType;
                                newObj.OrderNr = request.companies.TermsAndConditions[0].OrderNr;
                                newObj.BusinessType = request.companies.TermsAndConditions[0].BusinessType;
                                newObj.Section = request.companies.TermsAndConditions[0].Section;
                                newObj.SubSection = request.companies.TermsAndConditions[0].SubSection;
                                newObj.TermsDescription = request.companies.TermsAndConditions[0].TermsDescription;
                                newObj.CreateDate = DateTime.Now;
                                newObj.CreateUser = request.companies.TermsAndConditions[0].CreateUser;
                                companies.TermsAndConditions.Add(newObj);
                                response.TermsAndConditionsId = newObj.TermsAndConditions_Id;
                            }
                            else if (request.IsRemoveCondition)
                            {
                                CompanyTerms newobj = new CompanyTerms();
                                newobj.Company_Id = request.companies.TermsAndConditions[0].Company_Id;
                                newobj.TermsAndConditions_Id = request.companies.TermsAndConditions[0].TermsAndConditions_Id;
                                companies.TermsAndConditions.RemoveAll(x => x.Company_Id.ToUpper() == request.companies.TermsAndConditions[0].Company_Id.ToUpper() && x.TermsAndConditions_Id.ToUpper() == request.companies.TermsAndConditions[0].TermsAndConditions_Id.ToUpper());
                            }
                            else
                            {
                                foreach (var terms in companies.TermsAndConditions)
                                {
                                    foreach (var newTerms in request.companies.TermsAndConditions)
                                    {
                                        if (terms.TermsAndConditions_Id != newTerms.TermsAndConditions_Id && terms.OrderNr == newTerms.OrderNr && terms.DocumentType == newTerms.DocumentType
                                            && terms.BusinessType == newTerms.BusinessType && terms.Section == newTerms.Section && terms.SubSection == newTerms.SubSection && terms.TermsDescription == newTerms.TermsDescription)
                                        {
                                            response.ResponseStatus.Status = "Failure";
                                            response.ResponseStatus.ErrorMessage = "Duplicate Record";
                                        }
                                    }
                                }
                                if (response.ResponseStatus.Status != "Failure")
                                {
                                    foreach (var terms in companies.TermsAndConditions)
                                    {
                                        foreach (var newTerms in request.companies.TermsAndConditions)
                                        {
                                            if (terms.TermsAndConditions_Id == newTerms.TermsAndConditions_Id)
                                            {
                                                terms.Company_Id = newTerms.Company_Id;
                                                terms.DocumentType = newTerms.DocumentType;
                                                terms.OrderNr = newTerms.OrderNr;
                                                terms.BusinessType = newTerms.BusinessType;
                                                terms.Section = newTerms.Section;
                                                terms.SubSection = newTerms.SubSection;
                                                terms.TermsDescription = newTerms.TermsDescription;
                                                terms.EditUser = newTerms.EditUser;
                                                terms.EditDate = DateTime.Now;
                                                response.TermsAndConditionsId = newTerms.TermsAndConditions_Id;
                                            }
                                        }
                                    }
                                }
                            }

                            await _MongoContext.mCompanies.UpdateOneAsync(Builders<mCompanies>.Filter.Eq("Company_Id", companies.Company_Id),
                           Builders<mCompanies>.Update.Set("TermsAndConditions", companies.TermsAndConditions)
                           .Set("EditUser", request.EditUser).Set("EditDate", DateTime.Now));
                        }
                        if (request.companies.PaymentDetails != null && request.companies.PaymentDetails.Count > 0)
                        {
                            if (request.IsNewPaymentDetail)
                            {
                                PaymentDetails newObj = new PaymentDetails();
                                newObj.PaymentDetails_Id = Guid.NewGuid().ToString();
                                newObj.Company_Id = request.companies.PaymentDetails[0].Company_Id;
                                newObj.Currency_Id = request.companies.PaymentDetails[0].Currency_Id;
                                newObj.Currency = request.companies.PaymentDetails[0].Currency;
                                newObj.Details = request.companies.PaymentDetails[0].Details;
                                newObj.CreateDate = DateTime.Now;
                                newObj.CreateUser = request.companies.PaymentDetails[0].CreateUser;
                                companies.PaymentDetails.Add(newObj);
                                response.PaymentDetailsId = newObj.PaymentDetails_Id;
                            }
                            else if (request.IsRemovePaymentDetail)
                            {
                                PaymentDetails newobj = new PaymentDetails();
                                newobj.Company_Id = request.companies.PaymentDetails[0].Company_Id;
                                newobj.PaymentDetails_Id = request.companies.PaymentDetails[0].PaymentDetails_Id;
                                companies.PaymentDetails.RemoveAll(x => x.Company_Id == request.companies.PaymentDetails[0].Company_Id && x.PaymentDetails_Id == request.companies.PaymentDetails[0].PaymentDetails_Id);
                            }
                            else
                            {
                                foreach (var dtls in companies.PaymentDetails)
                                {
                                    foreach (var newDtls in request.companies.PaymentDetails)
                                    {
                                        if (dtls.PaymentDetails_Id != newDtls.PaymentDetails_Id && dtls.Details == newDtls.Details && dtls.Currency_Id == newDtls.Currency_Id
                                            && dtls.Currency == newDtls.Currency)
                                        {
                                            response.ResponseStatus.Status = "Failure";
                                            response.ResponseStatus.ErrorMessage = "Duplicate Record";
                                        }
                                    }
                                }
                                if (response.ResponseStatus.Status != "Failure")
                                {
                                    foreach (var dtls in companies.PaymentDetails)
                                    {
                                        foreach (var newDtls in request.companies.PaymentDetails)
                                        {
                                            if (dtls.PaymentDetails_Id == newDtls.PaymentDetails_Id)
                                            {
                                                dtls.Company_Id = newDtls.Company_Id;
                                                dtls.Currency_Id = newDtls.Currency_Id;
                                                dtls.Currency = newDtls.Currency;
                                                dtls.Details = newDtls.Details;
                                                dtls.EditUser = newDtls.EditUser;
                                                dtls.EditDate = DateTime.Now;
                                                response.PaymentDetailsId = newDtls.PaymentDetails_Id;
                                            }
                                        }
                                    }
                                }
                            }

                            await _MongoContext.mCompanies.UpdateOneAsync(Builders<mCompanies>.Filter.Eq("Company_Id", companies.Company_Id),
                           Builders<mCompanies>.Update.Set("PaymentDetails", companies.PaymentDetails)
                           .Set("EditUser", request.EditUser).Set("EditDate", DateTime.Now));
                        }
                        if (request.companies.Resources != null && request.companies.Resources.Count > 0)
                        {
                            foreach (var res in companies.Resources)
                            {
                                foreach (var newRes in request.companies.Resources)
                                {
                                    if (res.CompanyResources_Id == newRes.CompanyResources_Id)
                                    {
                                        res.ResourcesType_Id = newRes.ResourcesType_Id;
                                        res.Company_Id = newRes.Company_Id;
                                        res.ResourcesType = newRes.ResourcesType;
                                        res.FileName = newRes.FileName;
                                        res.Name = newRes.Name;
                                        res.OrderNr = newRes.OrderNr;
                                        res.FilePath = newRes.FilePath;
                                        res.EditUser = newRes.EditUser;
                                        res.EditDate = DateTime.Now;
                                    }
                                }
                            }

                            //   await _MongoContext.mCompanies.UpdateOneAsync(Builders<mCompanies>.Filter.Eq("Company_Id", companies.Company_Id),
                            //Builders<mCompanies>.Update.Set("Resources", companies.Resources));
                        }
                        if (request.companies.Markups != null && request.companies.Markups.Count > 0)
                        {
                            if (companies.Markups == null)
                            {
                                companies.Markups = new List<CompanyMarkup>();
                                companies.Markups.Add(new CompanyMarkup() { Markup_For = "Groups" });
                                companies.Markups.Add(new CompanyMarkup() { Markup_For = "FIT" });
                                companies.Markups.Add(new CompanyMarkup() { Markup_For = "Series" });
                                companies.Markups.Add(new CompanyMarkup() { Markup_For = "B2B2B" });
                            }

                            foreach (var markup in companies.Markups)
                            {
                                foreach (var newMarkUp in request.companies.Markups)
                                {
                                    if (markup.Markup_For.ToLower() == newMarkUp.Markup_For.ToLower())
                                    {
                                        markup.Markup_For = newMarkUp.Markup_For ?? markup.Markup_For;
                                        markup.Markup_Basis = newMarkUp.Markup_Basis ?? newMarkUp.Markup_Basis;
                                        if (newMarkUp.Markup_Basis == "Percentage / Scheme" && !string.IsNullOrEmpty(newMarkUp.Markup_Id) && !string.IsNullOrEmpty(newMarkUp.Markup))
                                        {
                                            markup.Markup_Id = newMarkUp.Markup_Id;
                                            markup.Markup = newMarkUp.Markup;
                                            markup.Markup_Value = null;
                                        }
                                        else if (newMarkUp.Markup_Basis == "Amount" && newMarkUp.Markup_Value != null)
                                        {
                                            markup.Markup_Id = null;
                                            markup.Markup = null;
                                            markup.Markup_Value = newMarkUp.Markup_Value;
                                        }
                                        else if (newMarkUp.Markup_Basis == "Select")
                                        {
                                            markup.Markup_Basis = null;
                                            markup.Markup_Id = null;
                                            markup.Markup = null;
                                            markup.Markup_Value = null;
                                        }
                                        else
                                        {
                                            markup.Markup_Id = markup.Markup_Id;
                                            markup.Markup = markup.Markup;
                                            markup.Markup_Value = markup.Markup_Value;
                                        }
                                    }
                                }
                            }

                            await _MongoContext.mCompanies.UpdateOneAsync(Builders<mCompanies>.Filter.Eq("Company_Id", companies.Company_Id),
                        Builders<mCompanies>.Update.Set("Markups", companies.Markups)
                        .Set("EditUser", request.EditUser).Set("EditDate", DateTime.Now));
                        }
                        if (request.companies.SalesOffices != null && request.companies.SalesOffices.Count > 0)
                        {
                            foreach (var sales in companies.SalesOffices)
                            {
                                foreach (var newSales in request.companies.SalesOffices)
                                {
                                    sales.ParentCompany_Id = newSales.ParentCompany_Id;
                                    sales.Company_Id = newSales.Company_Id;
                                    sales.Company_Code = newSales.Company_Code;
                                    sales.Company_Name = newSales.Company_Name;
                                }
                            }
                        }
                        if (request.companies.Branches != null && request.companies.Branches.Count > 0)
                        {
                            if (request.IsNewBranch)
                            {
                                ChildrenCompanies newBranch = new ChildrenCompanies();
                                newBranch.ParentCompany_Id = request.companies.Branches[0].ParentCompany_Id;
                                newBranch.Company_Id = request.companies.Branches[0].Company_Id;
                                newBranch.Company_Code = request.companies.Branches[0].Company_Code;
                                newBranch.Company_Name = request.companies.Branches[0].Company_Name;
                                companies.Branches.Add(newBranch);

                                var res = await _MongoContext.mCompanies.FindOneAndUpdateAsync(Builders<mCompanies>.Filter.Eq("Company_Id", request.companies.Company_Id),
                                Builders<mCompanies>.Update.Set("Branches", companies.Branches)
                                .Set("EditUser", request.EditUser).Set("EditDate", DateTime.Now));
                            }
                            else if (request.IsRemoveBranch)
                            {
                                ChildrenCompanies newBranch = new ChildrenCompanies();
                                newBranch.ParentCompany_Id = request.companies.Branches[0].ParentCompany_Id;
                                newBranch.Company_Id = request.companies.Branches[0].Company_Id;
                                newBranch.Company_Code = request.companies.Branches[0].Company_Code;
                                newBranch.Company_Name = request.companies.Branches[0].Company_Name;
                                companies.Branches.RemoveAll(x => x.Company_Id == request.companies.Branches[0].Company_Id && x.ParentCompany_Id == request.companies.Branches[0].ParentCompany_Id);
                                companies.ContactDetails.RemoveAll(x => x.ActualCompany_Id_AsShared == request.companies.Branches[0].Company_Id);

                                var res = await _MongoContext.mCompanies.FindOneAndUpdateAsync(Builders<mCompanies>.Filter.Eq("Company_Id", request.companies.Company_Id),
                                Builders<mCompanies>.Update.Set("Branches", companies.Branches).Set("ContactDetails", companies.ContactDetails)
                                .Set("EditUser", request.EditUser).Set("EditDate", DateTime.Now));
                            }

                        }
                        if (request.companies.PaymentTerms != null && request.companies.PaymentTerms.Count > 0)
                        {
                            if (request.IsNewPaymentTerms)
                            {
                                PaymentTerms newObj = new PaymentTerms();
                                newObj.PaymentTerms_Id = Guid.NewGuid().ToString();
                                newObj.Company_Id = request.companies.PaymentTerms[0].Company_Id;
                                newObj.From = request.companies.PaymentTerms[0].From;
                                newObj.Days = request.companies.PaymentTerms[0].Days;
                                newObj.ValueType = request.companies.PaymentTerms[0].ValueType;
                                newObj.Value = request.companies.PaymentTerms[0].Value;
                                newObj.Currency_Id = request.companies.PaymentTerms[0].Currency_Id;
                                newObj.Currency = request.companies.PaymentTerms[0].Currency;
                                newObj.VoucherReleased = request.companies.PaymentTerms[0].VoucherReleased;
                                newObj.Crea_Dt = DateTime.Now;
                                newObj.Crea_Us = request.companies.PaymentTerms[0].Crea_Us;
                                newObj.STATUS = request.companies.PaymentTerms[0].STATUS;
                                newObj.BusiType = request.companies.PaymentTerms[0].BusiType;
                                companies.PaymentTerms.Add(newObj);
                                response.PaymentTermsId = newObj.PaymentTerms_Id;
                            }
                            else
                            {
                                foreach (var pay in companies.PaymentTerms)
                                {
                                    foreach (var newPay in request.companies.PaymentTerms)
                                    {
                                        if (pay.PaymentTerms_Id != newPay.PaymentTerms_Id && pay.From == newPay.From && pay.Days == newPay.Days && pay.ValueType == newPay.ValueType && pay.Value == newPay.Value
                                            && pay.Currency_Id == newPay.Currency_Id && pay.Currency == newPay.Currency && pay.BusiType == newPay.BusiType)
                                        {
                                            response.ResponseStatus.Status = "Failure";
                                            response.ResponseStatus.ErrorMessage = "Duplicate Record";
                                        }
                                    }
                                }
                                if (response.ResponseStatus.Status != "Failure")
                                {
                                    foreach (var pay in companies.PaymentTerms)
                                    {
                                        foreach (var newPay in request.companies.PaymentTerms)
                                        {
                                            if (pay.PaymentTerms_Id == newPay.PaymentTerms_Id)
                                            {
                                                pay.Company_Id = newPay.Company_Id;
                                                pay.From = newPay.From;
                                                pay.Days = newPay.Days;
                                                pay.ValueType = newPay.ValueType;
                                                pay.Value = newPay.Value;
                                                pay.Currency_Id = newPay.Currency_Id;
                                                pay.Currency = newPay.Currency;
                                                pay.VoucherReleased = newPay.VoucherReleased;
                                                pay.Modi_Dt = DateTime.Now;
                                                pay.Modi_Us = newPay.Modi_Us;
                                                pay.STATUS = newPay.STATUS ?? " ";
                                                pay.BusiType = newPay.BusiType;
                                                response.PaymentTermsId = newPay.PaymentTerms_Id;

                                            }
                                        }
                                    }
                                }
                            }
                            await _MongoContext.mCompanies.UpdateOneAsync(Builders<mCompanies>.Filter.Eq("Company_Id", companies.Company_Id),
                         Builders<mCompanies>.Update.Set("PaymentTerms", companies.PaymentTerms)
                         .Set("EditUser", request.EditUser).Set("EditDate", DateTime.Now));

                        }
                        if (request.companies.TaxRegestrationDetails != null && request.companies.TaxRegestrationDetails.Count > 0)
                        {
                            if (request.IsNewTaxRegistrationDetails)
                            {
                                TaxRegestrationDetails newObj = new TaxRegestrationDetails();
                                newObj.TaxReg_Id = Guid.NewGuid().ToString();
                                newObj.Type_Id = request.companies.TaxRegestrationDetails.FirstOrDefault()?.Type_Id;
                                newObj.Type = request.companies.TaxRegestrationDetails.FirstOrDefault()?.Type;
                                newObj.State_Id = request.companies.TaxRegestrationDetails.FirstOrDefault()?.State_Id;
                                newObj.State = request.companies.TaxRegestrationDetails.FirstOrDefault()?.State;
                                newObj.Number = request.companies.TaxRegestrationDetails.FirstOrDefault()?.Number;
                                newObj.Taxstatus_Id = request.companies.TaxRegestrationDetails.FirstOrDefault()?.Taxstatus_Id;
                                newObj.TaxStatus = request.companies.TaxRegestrationDetails.FirstOrDefault()?.TaxStatus;
                                newObj.Status = request.companies.TaxRegestrationDetails.FirstOrDefault()?.Status;
                                newObj.CreateDate = DateTime.Now;
                                newObj.CreateUser = request.companies.TaxRegestrationDetails.FirstOrDefault()?.CreateUser;
                                newObj.Company_id = request.companies.TaxRegestrationDetails.FirstOrDefault()?.Company_id;
                                companies.TaxRegestrationDetails.Add(newObj);
                                response.TaxRegId = newObj.TaxReg_Id;
                            }
                            else
                            {
                                foreach (var tax in companies.TaxRegestrationDetails)
                                {
                                    foreach (var newtax in request.companies.TaxRegestrationDetails)
                                    {
                                        if (tax.TaxReg_Id != newtax.TaxReg_Id && tax.Type == newtax.Type && tax.Number == newtax.Number && tax.TaxStatus == newtax.TaxStatus)
                                        {
                                            response.ResponseStatus.Status = "Failure";
                                            response.ResponseStatus.ErrorMessage = "Duplicate Record";
                                        }
                                    }
                                }
                                if (response.ResponseStatus.Status != "Failure")
                                {
                                    foreach (var tax in companies.TaxRegestrationDetails)
                                    {
                                        foreach (var newtax in request.companies.TaxRegestrationDetails)
                                        {
                                            if (tax.TaxReg_Id == newtax.TaxReg_Id)
                                            {
                                                tax.Company_id = newtax.Company_id;
                                                tax.Type = newtax.Type;
                                                tax.Type_Id = newtax.Type_Id;
                                                tax.Number = newtax.Number;
                                                tax.State = newtax.State;
                                                tax.State_Id = newtax.State_Id;
                                                tax.TaxStatus = newtax.TaxStatus;
                                                tax.Taxstatus_Id = newtax.Taxstatus_Id;
                                                tax.EditDate = DateTime.Now;
                                                tax.EditUser = newtax.EditUser;
                                                tax.Status = newtax.Status;
                                                response.TaxRegId = newtax.TaxReg_Id;

                                            }
                                        }
                                    }
                                }
                            }
                            await _MongoContext.mCompanies.UpdateOneAsync(Builders<mCompanies>.Filter.Eq("Company_Id", companies.Company_Id),
                         Builders<mCompanies>.Update.Set("TaxRegestrationDetails", companies.TaxRegestrationDetails)
                         .Set("EditUser", request.EditUser).Set("EditDate", DateTime.Now));

                        }
                        if (request.companies.EmergencyContacts != null && request.companies.EmergencyContacts.Count > 0)
                        {
                            if (request.IsNewEmergencyContact)
                            {
                                if (request.companies.EmergencyContacts[0].Default == true)
                                {
                                    companies.EmergencyContacts.ForEach(x => x.Default = false);
                                }

                                EmergencyContacts newContact = new EmergencyContacts();
                                newContact.EmergencyContact_Id = Guid.NewGuid().ToString();
                                newContact.Company_Id = request.companies.EmergencyContacts[0].Company_Id;
                                newContact.Country_Id = request.companies.EmergencyContacts[0].Country_Id;
                                newContact.Country = request.companies.EmergencyContacts[0].Country;
                                newContact.EmergencyNo = request.companies.EmergencyContacts[0].EmergencyNo;
                                newContact.Default = request.companies.EmergencyContacts[0].Default;
                                newContact.Contact_Id = request.companies.EmergencyContacts[0].Contact_Id;
                                newContact.ContactName = request.companies.EmergencyContacts[0].ContactName;
                                newContact.ContactMail = request.companies.EmergencyContacts[0].ContactMail;
                                newContact.ContactTel = request.companies.EmergencyContacts[0].ContactTel;
                                newContact.BusiType = request.companies.EmergencyContacts[0].BusiType;
                                newContact.Status = request.companies.EmergencyContacts[0].Status;
                                companies.EmergencyContacts.Add(newContact);
                                response.EmergencyContactId = newContact.EmergencyContact_Id;
                            }
                            else
                            {
                                foreach (var cont in companies.EmergencyContacts)
                                {
                                    foreach (var newCont in request.companies.EmergencyContacts)
                                    {
                                        if (cont.EmergencyContact_Id != newCont.EmergencyContact_Id && cont.Country_Id == newCont.Country_Id && cont.Country == newCont.Country
                                            && cont.ContactName == newCont.ContactName && cont.EmergencyNo == newCont.EmergencyNo)
                                        {
                                            response.ResponseStatus.Status = "Failure";

                                            if (cont.Status == "X")
                                                response.ResponseStatus.ErrorMessage = "Record is inactive and already exists. Please Activate it";
                                            else
                                                response.ResponseStatus.ErrorMessage = "Duplicate Record";
                                        }
                                    }
                                }
                                if (response.ResponseStatus.Status != "Failure")
                                {
                                    if (request.companies.EmergencyContacts[0].Default == true)
                                    {
                                        companies.EmergencyContacts.ForEach(x => x.Default = false);
                                    }

                                    foreach (var cont in companies.EmergencyContacts)
                                    {
                                        foreach (var newCont in request.companies.EmergencyContacts)
                                        {
                                            if (cont.EmergencyContact_Id == newCont.EmergencyContact_Id)
                                            {
                                                cont.Company_Id = newCont.Company_Id ?? cont.Company_Id;
                                                cont.Country_Id = newCont.Country_Id ?? cont.Country_Id;
                                                cont.Country = newCont.Country ?? cont.Country;
                                                cont.EmergencyNo = newCont.EmergencyNo ?? cont.EmergencyNo;
                                                cont.Default = newCont.Default ?? cont.Default;
                                                cont.Contact_Id = newCont.Contact_Id ?? cont.Contact_Id;
                                                cont.ContactName = newCont.ContactName ?? cont.ContactName;
                                                cont.ContactMail = newCont.ContactMail ?? cont.ContactMail;
                                                cont.ContactTel = newCont.ContactTel ?? cont.ContactTel;
                                                cont.BusiType = newCont.BusiType ?? cont.BusiType;
                                                cont.Status = newCont.Status ?? " ";
                                                response.EmergencyContactId = newCont.EmergencyContact_Id;
                                            }
                                        }
                                    }
                                }
                            }

                            await _MongoContext.mCompanies.UpdateOneAsync(Builders<mCompanies>.Filter.Eq("Company_Id", companies.Company_Id),
                           Builders<mCompanies>.Update.Set("EmergencyContacts", companies.EmergencyContacts)
                           .Set("EditUser", request.EditUser).Set("EditDate", DateTime.Now));
                        }

                        if (request.companies.Targets?.Count > 0)
                        {
                            var objmCompanies = new mCompanies();
                            if (request.IsNewCompanyTarget)
                            {
                                request.companies.Targets.ForEach(a => { a.Income = Convert.ToDecimal(a.Income); a.TargetId = Guid.NewGuid().ToString(); a.CreateDate = DateTime.Now; a.CreateUser = request.EditUser; });
                                response.Targets = request.companies.Targets.FirstOrDefault();
                                await _MongoContext.mCompanies.UpdateOneAsync(Builders<mCompanies>.Filter.Eq("Company_Id", companies.Company_Id),
                                    Builders<mCompanies>.Update.PushEach("Targets", request.companies.Targets)
                                    .Set("EditUser", request.EditUser).Set("EditDate", DateTime.Now));
                            }

                            if (request.IsCompanyTarget)
                            {
                                response.Targets = request.companies.Targets.FirstOrDefault();
                                foreach (var item in request.companies.Targets)
                                {
                                    var target = companies.Targets.Where(a => a.TargetId == item.TargetId).FirstOrDefault();
                                    if (target != null)
                                    {
                                        item.EditDate = DateTime.Now;
                                        item.EditUser = request.EditUser;
                                        item.CreateDate = target.CreateDate;
                                        item.CreateUser = target.CreateUser;
                                        item.Income = Convert.ToDecimal(item.Income);

                                        objmCompanies = await _MongoContext.mCompanies.FindOneAndUpdateAsync(m => m.Company_Id == companies.Company_Id && m.Targets.Any(a => a.TargetId == item.TargetId),
                                           Builders<mCompanies>.Update.Set(m => m.Targets[-1], item));
                                    }
                                }
                            }

                            if (request.IsNewContactTarget)
                            {
                                var contact = request.companies.ContactDetails.FirstOrDefault();
                                response.Targets = contact.Targets.FirstOrDefault();
                                contact.Targets.ForEach(a => { a.Income = Convert.ToDecimal(a.Income); a.TargetId = Guid.NewGuid().ToString(); a.CreateDate = DateTime.Now; a.CreateUser = request.EditUser; });

                                objmCompanies = await _MongoContext.mCompanies.FindOneAndUpdateAsync(m => m.Company_Id == companies.Company_Id
                                                && m.ContactDetails.Any(a => a.Contact_Id == contact.Contact_Id),
                                                Builders<mCompanies>.Update.PushEach(a => a.ContactDetails[-1].Targets, contact.Targets));
                            }

                            if (request.IsContactTarget)
                            {
                                var contact = request.companies.ContactDetails.FirstOrDefault();
                                var target = contact.Targets.FirstOrDefault();
                                response.Targets = target;

                                companies.ContactDetails.Where(a => a.Contact_Id == contact.Contact_Id).FirstOrDefault().Targets.Where(a => a.TargetId == target.TargetId)?.ToList().ForEach(a =>
                                {
                                    a.EditDate = DateTime.Now; a.EditUser = request.EditUser; a.Bookings = target.Bookings; a.Income = target.Income;
                                    a.Month = target.Month; a.Passengers = target.Passengers; a.QRFs = target.QRFs;
                                });

                                foreach (var item in companies.ContactDetails)
                                {
                                    if (item.Contact_Id == contact.Contact_Id)
                                    {
                                        objmCompanies = await _MongoContext.mCompanies.FindOneAndUpdateAsync(m => m.Company_Id == companies.Company_Id
                                                        && m.ContactDetails.Any(a => a.Contact_Id == contact.Contact_Id),
                                                        Builders<mCompanies>.Update.Set(a => a.ContactDetails[-1].Targets, item.Targets));
                                    }
                                }
                            }
                        }
                    }
                }

                //ReplaceOneResult replaceResult = await _MongoContext.mCompanies.ReplaceOneAsync(Builders<mCompanies>.Filter.Eq("Company_Id", companies.Company_Id), companies);
                //response.ResponseStatus.Status = replaceResult.MatchedCount > 0 ? "Success" : "Failure";
                //response.ResponseStatus.ErrorMessage = replaceResult.MatchedCount > 0 ? "Saved Successfully." : "Details not updated.";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }
            return response;
        }

        public async Task<List<mStatus>> GetStatusForAgents()
        {
            return _MongoContext.mStatus.AsQueryable().Where(x => x.ForCompany == true).Distinct().ToList();
        }

        public async Task<List<mDefStartPage>> GetStartPageForAgents()
        {
            return _MongoContext.mDefStartPage.AsQueryable().ToList();
        }

        public async Task<mUsers> GetUserDetailsByContactId(AgentGetReq request)
        {
            mUsers user = new mUsers();
            if (!string.IsNullOrEmpty(request.ContactId) && !string.IsNullOrEmpty(request.CompanyId))
            {
                user = _MongoContext.mUsers.AsQueryable().Where(x => x.Contact_Id == request.ContactId && x.Company_Id == request.CompanyId).FirstOrDefault();
            }
            if (!string.IsNullOrEmpty(request.UserName))
            {
                user = _MongoContext.mUsers.AsQueryable().Where(x => !string.IsNullOrEmpty(x.UserName) && x.UserName.ToLower()== request.UserName.ToLower().Trim()).FirstOrDefault();
            }
            return user;
        }

        public async Task<List<Attributes>> GetDefDocumentTypes()
        {
            return _MongoContext.mDefDocumentTypes.AsQueryable().Where(x => x.ForCustomer == true).Select(x => new Attributes { Attribute_Id = x.DocumentTypeId, AttributeName = x.DocumentType }).Distinct().ToList();
        }

        public async Task<List<Attributes>> GetProductTypes()
        {
            return _MongoContext.mProductType.AsQueryable().Select(x => new Attributes { Attribute_Id = x.VoyagerProductType_Id, AttributeName = x.Prodtype }).Distinct().OrderBy(a => a.AttributeName).ToList();
        }

        public async Task<CompanyOfficerGetRes> GetCompanyOfficers(CompanyOfficerGetReq request)
        {
            CompanyOfficerGetRes response = new CompanyOfficerGetRes();
            try
            {
                //var SystemCompany_Id = _MongoContext.mCompanies.AsQueryable().Where(x => x.Company_Id == request.CompanyId).Select(y => y.SystemCompany_Id).FirstOrDefault();

                //var ContactDetails = _MongoContext.mCompanies.AsQueryable().Where(x => x.Company_Id == SystemCompany_Id).Select(y => y.ContactDetails).FirstOrDefault();

                //if (ContactDetails != null && ContactDetails.Count > 0)
                //{
                //    response.ContactDetails = ContactDetails.OrderBy(a => a.MAIL).ToList();

                //    if (!string.IsNullOrEmpty(request.UserRole))
                //    {
                //        response.ContactDetails = response.ContactDetails.Where(a => a.Roles.Any(b => b.RoleName == request.UserRole)).ToList();
                //    }
                //}

                var result = (from r in _MongoContext.mRoles.AsQueryable()
                              join ur in _MongoContext.mUsersInRoles.AsQueryable() on r.Voyager_Role_Id equals ur.RoleId
                              join u in _MongoContext.mUsers.AsQueryable() on ur.UserId equals u.VoyagerUser_Id
                              where request.UserRole == r.RoleName
                              select new UserDetails
                              {
                                  UserId = u.VoyagerUser_Id,
                                  UserRoleId = r.Voyager_Role_Id,
                                  UserRole = r.RoleName,
                                  FirstName = u.FirstName,
                                  LastName = u.LastName,
                                  Email = u.Email
                              }).Distinct().ToList();

                result.ForEach(a => response.ContactDetails.Add(new CompanyContacts() { MAIL = a.Email }));
                response.ContactDetails = response.ContactDetails.OrderBy(a => a.MAIL).ToList();
                response.ResponseStatus.Status = "Success";
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }
            return response;
        }

        public async Task<CompanyOfficerGetRes> GetCompanyContacts(CompanyOfficerGetReq request)
        {
            CompanyOfficerGetRes response = new CompanyOfficerGetRes();
            try
            {
                var ContactDetails = _MongoContext.mCompanies.AsQueryable().Where(x => x.Company_Id == request.CompanyId).Select(y => y.ContactDetails).FirstOrDefault();

                if (ContactDetails != null && ContactDetails.Count > 0)
                {
                    response.ContactDetails = ContactDetails.OrderBy(a => a.MAIL).ToList();

                    if (!string.IsNullOrEmpty(request.ContactId))
                    {
                        response.ContactDetails = response.ContactDetails.Where(a => a.Contact_Id == request.ContactId).ToList();
                    }
                }

                if (request.IsHeadOfficeUser)
                {
                    var HeadOffice_Id = _MongoContext.mCompanies.AsQueryable().Where(x => x.Company_Id == request.CompanyId).Select(y => y.HeadOffice_Id).FirstOrDefault();
                    var ContactDetailsHO = _MongoContext.mCompanies.AsQueryable().Where(x => x.Company_Id == HeadOffice_Id).Select(y => y.ContactDetails).FirstOrDefault();


                    if (ContactDetailsHO != null && ContactDetailsHO.Count > 0)
                    {
                        response.ContactDetails.AddRange(ContactDetailsHO.OrderBy(a => a.MAIL).ToList());

                        if (!string.IsNullOrEmpty(request.ContactId))
                        {
                            response.ContactDetails = response.ContactDetails.Where(a => a.Contact_Id == request.ContactId).ToList();
                        }
                    }
                }

                response.ResponseStatus.Status = "Success";
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }
            return response;
        }

        public async Task<CompanyOfficerGetRes> GetSalesOfficesOfSystemCompany()
        {
            CompanyOfficerGetRes response = new CompanyOfficerGetRes();
            try
            {
                var coreCompanyId = _MongoContext.mSystem.AsQueryable().FirstOrDefault()?.CoreCompany_Id;
                if (!string.IsNullOrWhiteSpace(coreCompanyId))
                {
                    var company = _MongoContext.mCompanies.AsQueryable().Where(x => x.Company_Id == coreCompanyId).FirstOrDefault();
                    response.Branches = company.Branches;
                    response.ResponseStatus.Status = "Success";
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "CoreCompanyId not found";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }
            return response;
        }


        public async Task<CompanyOfficerGetRes> GetSalesOfficesByCompanyId(string CompanyId)
        {
            CompanyOfficerGetRes response = new CompanyOfficerGetRes();
            try
            {
                if (!string.IsNullOrWhiteSpace(CompanyId))
                {
                    var company = _MongoContext.mCompanies.AsQueryable().Where(x => x.Company_Id == CompanyId).FirstOrDefault();
                    response.Branches = company.Branches;
                    response.ResponseStatus.Status = "Success";
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "CompanyId not found";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }
            return response;
        }

        public bool GetSystemCompany(string LoggedInUserContact_Id, out string SystemCompanyId)
        {
            try
            {
                SystemCompanyId = _MongoContext.mCompanies.AsQueryable().Where(x => x.ContactDetails.Any(y => y.Contact_Id == LoggedInUserContact_Id)).FirstOrDefault()?.SystemCompany_Id;
                if (!string.IsNullOrWhiteSpace(SystemCompanyId))
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                SystemCompanyId = "";
                return false;
            }
        }

        #region mComapnies->Target, mCompnies->Contacts->Targets
        public async Task<TargetGetRes> GetCompanyTargets(AgentGetReq request)
        {
            TargetGetRes response = new TargetGetRes()
            {
                ResponseStatus = new ResponseStatus(),
                TargetList = new List<Targets>(),
                ContactId = request.ContactId,
                CompanyId = request.CompanyId,
                ActionType = request.ActionType
            };
            try
            {
                var System = _MongoContext.mSystem.AsQueryable().FirstOrDefault();
                if (System != null)
                {
                    var objCurrency = _MongoContext.mCurrency.AsQueryable().Where(a => a.VoyagerCurrency_Id == System.BaseCurrency_Id).FirstOrDefault();
                    response.Currency = objCurrency.Currency;

                    if (!string.IsNullOrEmpty(request.CompanyId) && !string.IsNullOrEmpty(request.ContactId))
                    {
                        var objmCompanies = await _MongoContext.mCompanies.Find(x => x.Company_Id == request.CompanyId).FirstOrDefaultAsync();
                        var ContactDetails = objmCompanies?.ContactDetails;
                        var ContactTargets = ContactDetails.Where(a => a.Contact_Id == request.ContactId).Select(a => a.Targets)?.FirstOrDefault();
                        response.TargetList = ContactTargets;
                        response.ResponseStatus.Status = "Success";
                    }
                    else if (!string.IsNullOrEmpty(request.CompanyId))
                    {
                        var objmCompanies = await _MongoContext.mCompanies.Find(x => x.Company_Id == request.CompanyId).FirstOrDefaultAsync();
                        response.TargetList = objmCompanies?.Targets;
                        response.ResponseStatus.Status = "Success";
                    }
                }
                else
                {
                    response.ResponseStatus.ErrorMessage = "Base currency is not found in mSystem MongoDB.";
                    response.ResponseStatus.Status = "Failure";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }
            return response;
        }
        #endregion

        #region 3rd party Search Agent Details

        /// <summary>
        /// GetPartnerAgentDetails used for getting fetch CompanyId from VGER-mongodb based on "PartnerEntityCode" and "Application" provided by any 3rd party
        /// </summary>
        /// <param name="request"></param>
        /// <returns>
        /// Fetch CompanyId from VGER-mongodb
        /// </returns>
        public async Task<AgentThirdPartyGetRes> GetPartnerAgentDetails(AgentThirdPartyGetReq request)
        {
            AgentThirdPartyGetRes response = new AgentThirdPartyGetRes();

            try
            {
                var data = _MongoContext.mCompanies.AsQueryable().Where(a => a.Mappings != null
                        && a.Mappings.Any(b => b.PartnerEntityCode == request.PartnerEntityCode && b.Application.ToLower() == request.Application.ToLower())).FirstOrDefault();

                response.CompanyId = (data != null && data.Mappings != null && data.Mappings.Any()) ? data.Company_Id : "";
                response.ResponseStatus.Status = "Success";
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }

        /// <summary>
        /// GetPartnerAgentContactDetails used for getting fetch CompanyId and ContactId from VGER-mongodb based on "PartnerEntityCode" and "Application" provided by any 3rd party
        /// </summary>
        /// <param name="request"></param>
        /// <returns>
        /// Fetch CompanyId and ContactId from VGER-mongodb
        /// </returns>
        public async Task<AgentThirdPartyGetRes> GetPartnerAgentContactDetails(AgentThirdPartyGetReq request)
        {
            AgentThirdPartyGetRes response = new AgentThirdPartyGetRes();

            try
            {
                var data = _MongoContext.mCompanies.AsQueryable().Where(a => a.ContactDetails != null &&
                            a.ContactDetails.Any(b => b.Mappings != null &&
                            b.Mappings.Any(c => c.PartnerEntityCode == request.PartnerEntityCode
                                            && c.Application.ToLower() == request.Application.ToLower()))).FirstOrDefault();

                if (data != null && data.ContactDetails != null && data.ContactDetails.Any())
                {
                    response.CompanyId = data.Company_Id;
                    response.ContactId = data.ContactDetails.Where(a => a.Mappings != null && a.Mappings.Any(b => b.PartnerEntityCode == request.PartnerEntityCode)).Select(x => x.Contact_Id).FirstOrDefault();
                }
                //response.ResponseStatus.Status = !string.IsNullOrEmpty(response.ContactId) ? "Success" : "Failure";
                response.ResponseStatus.Status = "Success";
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }

        #endregion

        #region 3rd party AgentInfo or ContactInfo

        /// <summary>
		/// CreateUpdatePartnerAgentDetails used for saving Partner agent(Company) details in mCompanies collection VGER-mongodb
		/// </summary>
		/// <param name="request">ManageAgentReq with basic info from the 3rd party partner</param>
		/// <returns>
		/// Fetch the saved agent(Company) info from VGER-mongodb
		/// </returns>
		public async Task<ManageAgentRes> CreateUpdatePartnerAgentDetails(ManageAgentReq request)
        {
            mCompanies response = new mCompanies();
            ManageAgentRes returnData = new ManageAgentRes();
            AgentThirdPartyGetReq checkAgentInfo = new AgentThirdPartyGetReq();
            checkAgentInfo.PartnerEntityCode = request.AgentInfo.ApplicationEntityCode;
            checkAgentInfo.Application = request.Application;
            string outCompanyId = string.Empty;

            var ApplicationInfo = _MongoContext.mApplications.AsQueryable().Where(a => a.Application_Name.ToLower() == request.Application.ToLower()).FirstOrDefault();

            var existingAgentInfo = await GetPartnerAgentDetails(checkAgentInfo);

            if (existingAgentInfo != null && !string.IsNullOrEmpty(existingAgentInfo.CompanyId))
            {
                response = _MongoContext.mCompanies.AsQueryable().Where(a => a.Company_Id == existingAgentInfo.CompanyId).FirstOrDefault();
                if (response.Name != request.AgentInfo.Name || response.City_Id != request.AgentInfo.City || response.Country_Id != request.AgentInfo.Country)
                {
                    if (IsDuplicateCompany(request.AgentInfo.Name, request.AgentInfo.Country, request.AgentInfo.City, false, out outCompanyId) && response.Company_Id != outCompanyId)
                    {
                        returnData.ResponseStatus.ErrorMessage = "Company/Agent details already exist.";
                        returnData.ResponseStatus.Status = "Duplicate";
                        return returnData;
                    }
                }
                response.Name = request.AgentInfo.Name;
                response.Zipcode = request.AgentInfo.PostCode;
                response.Street = request.AgentInfo.Address1;
                response.Street2 = request.AgentInfo.Address2;
                response.City_Id = request.AgentInfo.City;
                response.Country_Id = request.AgentInfo.Country;
                response.CityName = request.AgentInfo.CityName;
                response.CountryName = request.AgentInfo.CountryName;
                response.STATUS = " ";
                response.EditUser = request.CreatedUser;
                response.EditDate = DateTime.Now;
                var mappingInfo = response.Mappings.AsQueryable().Where(a => a.Application_Id == ApplicationInfo.Application_Id && a.PartnerEntityCode == request.AgentInfo.ApplicationEntityCode).FirstOrDefault();
                mappingInfo.PartnerEntityName = request.AgentInfo.Name;
                mappingInfo.PartnerEntityType = _configuration.GetValue<string>("MappingDefault:PartnerEntityTypeAccount");
                mappingInfo.Action = _configuration.GetValue<string>("MappingDefault:ActionUpdate");
                mappingInfo.Status = string.Empty;
                mappingInfo.EditUser = request.CreatedUser;
                mappingInfo.EditDate = DateTime.Now;

                ReplaceOneResult replaceResult = await _MongoContext.mCompanies.ReplaceOneAsync(Builders<mCompanies>.Filter.Eq("Company_Id", response.Company_Id), response);
            }
            else
            {
                if (IsDuplicateCompany(request.AgentInfo.Name, request.AgentInfo.Country, request.AgentInfo.City, false, out outCompanyId))
                {
                    if (!string.IsNullOrEmpty(outCompanyId))
                    {
                        response = _MongoContext.mCompanies.AsQueryable().Where(a => a.Company_Id == outCompanyId).FirstOrDefault();

                        if (response != null)
                        {
                            if (response.Mappings == null)
                            {
                                response.Mappings = new List<Mappings>();
                            }
                            else if (response.Mappings.Where(a => a.Application_Id == ApplicationInfo.Application_Id).Any())
                            {
                                returnData.ResponseStatus.ErrorMessage = "Company/Agent details already exist.";
                                returnData.ResponseStatus.Status = "Duplicate";
                                return returnData;
                            }
                            var mappingInfo1 = new Mappings();
                            mappingInfo1.Application = ApplicationInfo.Application_Name;
                            mappingInfo1.Application_Id = ApplicationInfo.Application_Id;
                            mappingInfo1.PartnerEntityCode = request.AgentInfo.ApplicationEntityCode;
                            mappingInfo1.PartnerEntityName = request.AgentInfo.Name;
                            mappingInfo1.PartnerEntityType = _configuration.GetValue<string>("MappingDefault:PartnerEntityTypeAccount");
                            mappingInfo1.Action = _configuration.GetValue<string>("MappingDefault:ActionUpdate");
                            mappingInfo1.Status = string.Empty;
                            mappingInfo1.CreateUser = request.CreatedUser;
                            mappingInfo1.CreateDate = DateTime.Now;
                            response.Mappings.Add(mappingInfo1);

                            response.Name = request.AgentInfo.Name;
                            response.Zipcode = request.AgentInfo.PostCode;
                            response.Street = request.AgentInfo.Address1;
                            response.Street2 = request.AgentInfo.Address2;
                            response.City_Id = request.AgentInfo.City;
                            response.Country_Id = request.AgentInfo.Country;
                            response.CityName = request.AgentInfo.CityName;
                            response.CountryName = request.AgentInfo.CountryName;
                            response.STATUS = " ";
                            response.EditUser = request.CreatedUser;
                            response.EditDate = DateTime.Now;
                            ReplaceOneResult replaceResult = await _MongoContext.mCompanies.ReplaceOneAsync(Builders<mCompanies>.Filter.Eq("Company_Id", response.Company_Id), response);
                            returnData.CompanyInfo = response;
                            returnData.ResponseStatus.ErrorMessage = "Company/Agent info saved successfully.";
                            returnData.ResponseStatus.Status = "Success";
                            return returnData;
                        }
                        else
                        {
                            returnData.ResponseStatus.ErrorMessage = "Company/Agent details already exist.";
                            returnData.ResponseStatus.Status = "Duplicate";
                            return returnData;
                        }
                    }
                    else
                    {
                        returnData.ResponseStatus.ErrorMessage = "Company/Agent details already exist.";
                        returnData.ResponseStatus.Status = "Duplicate";
                        return returnData;
                    }
                }

                string userSystemCompanyId = string.Empty;
                bool isSystemCompanyId = GetSystemCompany(request.LoggedInUserContactId, out userSystemCompanyId);
                if (isSystemCompanyId)
                {
                    response.Markups = _MongoContext.mCompanies.AsQueryable().Where(a => a.Company_Id == userSystemCompanyId).FirstOrDefault().Markups;
                }

                response.Company_Id = Guid.NewGuid().ToString();
                response.Name = request.AgentInfo.Name;
                response.Zipcode = request.AgentInfo.PostCode;
                response.Street = request.AgentInfo.Address1;
                response.Street2 = request.AgentInfo.Address2;
                response.City_Id = request.AgentInfo.City;
                response.Country_Id = request.AgentInfo.Country;
                response.CityName = request.AgentInfo.CityName;
                response.CountryName = request.AgentInfo.CountryName;
                response.Issupplier = false;
                response.Iscustomer = true;
                response.Issubagent = false;
                response.SystemCompany_Id = userSystemCompanyId;
                response.STATUS = " ";
                response.ContactBy = request.CreatedUser;
                response.CreateUser = request.CreatedUser;
                response.CreateDate = DateTime.Now;

                #region Call Bridge service to get company code

                GetCompany_RS result = new GetCompany_RS();
                GetCompany_RQ request1 = new GetCompany_RQ();
                request1.Type = "Agent";
                result = agentProviders.GetLatestSQLReferenceNumber(request1, request.Token).Result;
                if (result != null && result.ResponseStatus.Status.ToLower() == "success")
                {
                    response.CompanyCode = Convert.ToString(result.ReferenceNumber);
                }
                else
                {
                    //error
                    returnData.ResponseStatus.ErrorMessage = "Failed to generate Company Code.";
                    returnData.ResponseStatus.Status = "Failure";
                    returnData.ResponseStatus.StatusMessage = "CompanyCodeError";
                    return returnData;
                }

                #endregion

                response.Mappings = new List<Mappings>();
                var mappingInfo = new Mappings();
                mappingInfo.Application = ApplicationInfo.Application_Name;
                mappingInfo.Application_Id = ApplicationInfo.Application_Id;
                mappingInfo.PartnerEntityCode = request.AgentInfo.ApplicationEntityCode;
                mappingInfo.PartnerEntityName = request.AgentInfo.Name;
                mappingInfo.PartnerEntityType = _configuration.GetValue<string>("MappingDefault:PartnerEntityTypeAccount");
                mappingInfo.Action = _configuration.GetValue<string>("MappingDefault:ActionCreate");
                mappingInfo.Status = string.Empty;
                mappingInfo.CreateUser = request.CreatedUser;
                mappingInfo.CreateDate = DateTime.Now;
                response.Mappings.Add(mappingInfo);

                await _MongoContext.mCompanies.InsertOneAsync(response);
            }

            //Call Bridge service to set Agent in Company table in Sql
            ResponseStatus responseAgentInfo = new ResponseStatus();
            SetCompany_RQ requestAgentInfo = new SetCompany_RQ();
            requestAgentInfo.Company_Id = response.Company_Id;
            requestAgentInfo.User = request.CreatedUser;
            responseAgentInfo = agentProviders.SetCompany(requestAgentInfo, request.Token).Result;
            if (responseAgentInfo != null && !string.IsNullOrEmpty(responseAgentInfo.Status) && responseAgentInfo.Status.ToLower() != "success")
            {
                returnData.ResponseStatus.Status = responseAgentInfo.Status;
                returnData.ResponseStatus.ErrorMessage = "Error occured while calling Bridge Set Company" + responseAgentInfo.StatusMessage;
                return returnData;
            }

            returnData.CompanyInfo = response;
            returnData.ResponseStatus.ErrorMessage = "Company/Agent info saved successfully.";
            returnData.ResponseStatus.Status = "Success";
            return returnData;
        }

        public string GetPartnerMappingCity(string CityId)
        {
            var cityInfo = _GenericRepository.GetPartnerCityDetails(new Attributes { Attribute_Id = CityId }).Result;

            return cityInfo.ResortInfo.ResortName;
        }

        public string GetPartnerMappingCountry(string CountryId)
        {
            var countryInfo = _GenericRepository.GetPartnerCountryDetails(new Attributes { Attribute_Id = CountryId }).Result;

            return countryInfo.ResortInfo.ResortName;
        }

        public bool IsDuplicateCompany(string AgentName, string CountryId, string CityId, bool IsSupplier, out string CompanyId)
        {
            CompanyId = string.Empty;
            FilterDefinition<mCompanies> filter = Builders<mCompanies>.Filter.Empty;
            if (!string.IsNullOrWhiteSpace(CountryId))
            {
                filter = filter & Builders<mCompanies>.Filter.Eq(x => x.Country_Id, CountryId);
            }
            if (!string.IsNullOrWhiteSpace(AgentName))
            {
                filter = filter & Builders<mCompanies>.Filter.Where(x => x.Name.ToLower() == AgentName.ToLower());
                //filter = filter & Builders<mCompanies>.Filter.Regex(x => x.Name, new BsonRegularExpression(new Regex(AgentName, RegexOptions.IgnoreCase)));
            }
            if (!string.IsNullOrWhiteSpace(CityId))
            {
                filter = filter & Builders<mCompanies>.Filter.Eq(x => x.City_Id, CityId);
            }

            filter = filter & Builders<mCompanies>.Filter.Eq(x => x.Issupplier, false);

            var fetchData = _MongoContext.mCompanies.Find(filter).FirstOrDefault();

            //var afetchData = _MongoContext.mCompanies.AsQueryable().Where(a => a.Country_Id == CountryId && a.City_Id == CityId && a.Name == AgentName).ToList();
            if (fetchData != null && !string.IsNullOrEmpty(fetchData.Company_Id))
            {
                CompanyId = fetchData.Company_Id;
                return true;
            }
            return false;
        }

        /// <summary>
		/// CreatePartnerAgentContactDetails used for saving Partner agent(add contact of a Company) details in mCompanies collection VGER-mongodb
		/// </summary>
		/// <param name="request">ManageAgentContactReq with basic info from the 3rd party partner</param>
		/// <returns>
		/// Fetch the saved agent(Company with all the contacts) info from VGER-mongodb
		/// </returns>
        public async Task<AgentThirdPartyGetRes> CreatePartnerAgentContactDetails(ManageAgentContactReq request)
        {
            AgentThirdPartyGetRes response = new AgentThirdPartyGetRes();

            try
            {
                var ApplicationInfo = _MongoContext.mApplications.AsQueryable().Where(a => a.Application_Name.ToLower() == request.ContactMappingInfo.Application.ToLower()).FirstOrDefault();

                var data = _MongoContext.mCompanies.AsQueryable().Where(a => a.Company_Id == request.SelectedCompanyId /*&& a.ContactDetails != null &&
                            a.ContactDetails.Any(b => b.Mappings != null &&
                            b.Mappings.Any(c => c.PartnerEntityCode == request.CompanyId
                                            && c.Application.ToLower() == request.ContactMappingInfo.Application.ToLower()))*/).FirstOrDefault();

                //var data1 = _MongoContext.mCompanies.AsQueryable().Where(a => a.Mappings != null
                //        && a.Mappings.Any(b => b.PartnerEntityCode == request.CompanyId && b.Application.ToLower() == request.ContactMappingInfo.Application.ToLower())).FirstOrDefault();

                if (data != null && data.ContactDetails == null)
                {
                    data.ContactDetails = new List<CompanyContacts>();
                }

                CompanyContacts newContact = new CompanyContacts();
                newContact.Contact_Id = Guid.NewGuid().ToString();
                newContact.Company_Id = request.SelectedCompanyId;
                newContact.Company_Name = data.Name;
                newContact.Default = 1;
                newContact.ActualCompany_Name_AsShared = data.Name;
                newContact.CommonTitle = request.ContactMappingInfo.Title;
                newContact.FIRSTNAME = request.ContactMappingInfo.FirstName;
                newContact.LastNAME = request.ContactMappingInfo.LastName;
                newContact.TEL = request.ContactMappingInfo.Telephone;
                newContact.MAIL = request.ContactMappingInfo.Email;
                newContact.UserName = string.Empty;
                newContact.STATUS = " ";
                newContact.CreateUser = request.ContactMappingInfo.CreateUser;
                newContact.CreateDate = DateTime.Now;

                var mMappingData = new ContactMappings();
                mMappingData.Application = ApplicationInfo.Application_Name;
                mMappingData.Application_Id = ApplicationInfo.Application_Id;
                mMappingData.PartnerEntityCode = request.ContactMappingInfo.PartnerEntityCode;
                mMappingData.PartnerEntityName = request.ContactMappingInfo.PartnerEntityName;
                mMappingData.PartnerEntityType = _configuration.GetValue<string>("MappingDefault:PartnerEntityTypeContact");
                mMappingData.Action = _configuration.GetValue<string>("MappingDefault:ActionCreate");
                mMappingData.Status = string.Empty;
                mMappingData.Telephone = request.ContactMappingInfo.Telephone;
                mMappingData.Email = request.ContactMappingInfo.Email;
                mMappingData.CreateDate = DateTime.Now;
                mMappingData.CreateUser = request.ContactMappingInfo.CreateUser;

                if (newContact.Mappings == null)
                {
                    newContact.Mappings = new List<ContactMappings>();
                }
                newContact.Mappings.Add(mMappingData);

                data.ContactDetails.Add(newContact);

                var resultUSer = _MongoContext.mCompanies.FindOneAndUpdate(Builders<mCompanies>.Filter.Eq("Company_Id", request.SelectedCompanyId),
                                 Builders<mCompanies>.Update.Set("ContactDetails", data.ContactDetails)
                                  .Set("EditUser", newContact.CreateUser).Set("EditDate", newContact.CreateDate));

                response.ContactId = newContact.Contact_Id;

                //Call Bridge service to add default contact in Contact table in Sql
                ResponseStatus result1 = new ResponseStatus();
                SetCompanyContact_RQ request1 = new SetCompanyContact_RQ();
                request1.Contact_Id = newContact.Contact_Id;
                request1.User = request.ContactMappingInfo.CreateUser;
                result1 = agentProviders.SetCompanyContact(request1, request.Token).Result;
                if (result1 != null && !string.IsNullOrEmpty(result1.Status) && result1.Status.ToLower() != "success")
                {
                    response.ResponseStatus.Status = result1.Status;
                    response.ResponseStatus.ErrorMessage = "Error occured while calling Bridge Set Contact" + result1.StatusMessage;
                    return response;
                }

                response.ResponseStatus.Status = "Success";
                response.ResponseStatus.ErrorMessage = "Contact info saved successfully.";
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }

        /// <summary>
        /// UpdatePartnerAgentContactDetails used for saving Partner agent(edit contact of a Company) details in mCompanies collection VGER-mongodb
        /// </summary>
        /// <param name="request">ManageAgentContactReq with basic info from the 3rd party partner</param>
        /// <returns>
        /// Fetch the saved agent(Company with all the contacts) info from VGER-mongodb
        /// </returns>
        public async Task<AgentThirdPartyGetRes> UpdatePartnerAgentContactDetails(ManageAgentContactReq request)
        {
            AgentThirdPartyGetRes response = new AgentThirdPartyGetRes();

            try
            {
                var ApplicationInfo = _MongoContext.mApplications.AsQueryable().Where(a => a.Application_Name.ToLower() == request.ContactMappingInfo.Application.ToLower()).FirstOrDefault();

                var data = _MongoContext.mCompanies.AsQueryable().Where(a => a.Company_Id == request.SelectedCompanyId && a.ContactDetails != null &&
                            a.ContactDetails.Any(b => b.Mappings != null &&
                            b.Mappings.Any(/*c => c.PartnerEntityCode == request.CompanyId
                                            && c.Application.ToLower() == request.ContactMappingInfo.Application.ToLower()*/))).FirstOrDefault();


                CompanyContacts newContact = new CompanyContacts();

                newContact = data.ContactDetails.Where(a => a.Mappings != null && a.Mappings.Any(b => b.PartnerEntityCode == request.ContactMappingInfo.PartnerEntityCode)).FirstOrDefault();

                var mMappingData = newContact.Mappings.Where(a => a.PartnerEntityCode == request.ContactMappingInfo.PartnerEntityCode).FirstOrDefault();

                newContact.CommonTitle = request.ContactMappingInfo.Title;
                newContact.FIRSTNAME = request.ContactMappingInfo.FirstName;
                newContact.LastNAME = request.ContactMappingInfo.LastName;
                newContact.TEL = request.ContactMappingInfo.Telephone;
                newContact.MAIL = request.ContactMappingInfo.Email;
                newContact.UserName = string.Empty;
                newContact.STATUS = " ";
                newContact.EditUser = request.ContactMappingInfo.CreateUser;
                newContact.EditDate = DateTime.Now;

                mMappingData.PartnerEntityName = request.ContactMappingInfo.PartnerEntityName;
                mMappingData.PartnerEntityType = _configuration.GetValue<string>("MappingDefault:PartnerEntityTypeContact");
                mMappingData.Action = _configuration.GetValue<string>("MappingDefault:ActionUpdate");
                mMappingData.Telephone = request.ContactMappingInfo.Telephone;
                mMappingData.Email = request.ContactMappingInfo.Email;
                mMappingData.EditUser = request.ContactMappingInfo.CreateUser;
                mMappingData.EditDate = DateTime.Now;

                var resultUSer = _MongoContext.mCompanies.FindOneAndUpdate(Builders<mCompanies>.Filter.Eq("Company_Id", request.SelectedCompanyId),
                                 Builders<mCompanies>.Update.Set("ContactDetails", data.ContactDetails)
                                  .Set("EditUser", request.ContactMappingInfo.CreateUser).Set("EditDate", DateTime.Now));

                response.ContactId = newContact.Contact_Id;

                //Call Bridge service to add default contact in Contact table in Sql
                ResponseStatus result1 = new ResponseStatus();
                SetCompanyContact_RQ request1 = new SetCompanyContact_RQ();
                request1.Contact_Id = newContact.Contact_Id;
                request1.User = request.ContactMappingInfo.CreateUser;
                result1 = agentProviders.SetCompanyContact(request1, request.Token).Result;
                if (result1 != null && !string.IsNullOrEmpty(result1.Status) && result1.Status.ToLower() != "success")
                {
                    response.ResponseStatus.Status = result1.Status;
                    response.ResponseStatus.ErrorMessage = "Error occured while calling Bridge Set Contact" + result1.StatusMessage;
                    return response;
                }

                response.ResponseStatus.Status = "Success";
                response.ResponseStatus.ErrorMessage = "Contact info saved successfully.";
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }

        #endregion
    }
}
