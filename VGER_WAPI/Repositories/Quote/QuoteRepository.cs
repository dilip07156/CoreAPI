using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VGER_WAPI.Helpers;
using VGER_WAPI.Models;
using VGER_WAPI_CLASSES;
using VGER_WAPI_CLASSES.QRFSummary;

namespace VGER_WAPI.Repositories
{
    public class QuoteRepository : IQuoteRepository
    {
        #region Private Variable Declaration
        private readonly MongoContext _MongoContext = null;
        private readonly IGenericRepository _genericRepository;
        private readonly IConfiguration _configuration;
        private readonly IMasterRepository _masterRepository;
        private readonly IProductRepository _productRepository;
        private readonly IAgentRepository _agentRepository;
        private readonly IProductPDPRepository _productPDPRepository;
        #endregion

        public QuoteRepository(IOptions<MongoSettings> settings, IGenericRepository genericRepository, IConfiguration configuration,
           IMasterRepository masterRepository, IProductRepository productRepository, IAgentRepository agentRepository, IProductPDPRepository productPDPRepository)
        {
            _MongoContext = new MongoContext(settings);
            _genericRepository = genericRepository;
            _configuration = configuration;
            _masterRepository = masterRepository;
            _productRepository = productRepository;
            _agentRepository = agentRepository;
            _productPDPRepository = productPDPRepository;
        }

        #region Agent 
        public List<AgentProperties> GetAgentCompanies(AgentCompanyReq request)
        {
            //IQueryable<AgentProperties> agents;


            var UserCompany_Id = _MongoContext.mUsers.AsQueryable().Where(x => x.VoyagerUser_Id == request.UserId).Select(y => y.Company_Id).FirstOrDefault();
            var AdminRoleId = _MongoContext.mRoles.AsQueryable().Where(x => x.LoweredRoleName == "administrator").Select(y => y.Voyager_Role_Id).FirstOrDefault();
            var AdminRole = _MongoContext.mUsersInRoles.AsQueryable().Where(x => x.UserId == request.UserId && x.RoleId == AdminRoleId).FirstOrDefault();

            FilterDefinition<mCompany> filter;
            filter = Builders<mCompany>.Filter.Empty;
            if (AdminRole == null)//means user is not an Admin
            {
                var CoreCompany_Id = _MongoContext.mSystem.AsQueryable().Select(y => y.CoreCompany_Id).FirstOrDefault();
                if (UserCompany_Id == CoreCompany_Id)
                {
                    if (!string.IsNullOrWhiteSpace(CoreCompany_Id))
                    {
                        filter = filter & Builders<mCompany>.Filter.Where(x => x.VoyagerCompany_Id != CoreCompany_Id && (x.Status == "" || x.Status == " " || x.Status == "  " || x.Status == null));
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(UserCompany_Id))
                    {
                        filter = filter & Builders<mCompany>.Filter.Where(x => x.ParentAgent_Id == UserCompany_Id && (x.Status == "" || x.Status == " " || x.Status == "  " || x.Status == null));
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(request.AgentName))
            {
                filter = filter & Builders<mCompany>.Filter.Where(c => (c.Iscustomer == true || c.Issubagent == true) && c.Issupplier == false && (c.Status == "" || c.Status == " " || c.Status == "  " || c.Status == null)
                            && c.Name.ToLower().Contains(request.AgentName.Trim().ToLower()));
            }
            else
            {
                filter = filter & Builders<mCompany>.Filter.Where(c => (c.Iscustomer == true || c.Issubagent == true) && c.Issupplier == false && (c.Status == "" || c.Status == " " || c.Status == "  " || c.Status == null));
            }

            var result = _MongoContext.mCompany.Find(filter).Project(c => new AgentProperties
            {
                VoyagerCompany_Id = c.VoyagerCompany_Id,
                Name = c.Name
            }).ToListAsync().Result;

            //if (!string.IsNullOrWhiteSpace(request.AgentName))
            //{
            //	agents = _MongoContext.mCompany.AsQueryable().Where(c => (c.Iscustomer == true || c.Issubagent == true) && c.Issupplier == false
            //				&& c.Name.ToLower().Contains(request.AgentName.Trim().ToLower()))
            //				.Select(c => new AgentProperties { VoyagerCompany_Id = c.VoyagerCompany_Id, Name = c.Name }).Distinct();
            //}
            //else
            //{
            //	agents = _MongoContext.mCompany.AsQueryable().Where(c => (c.Iscustomer == true || c.Issubagent == true) && c.Issupplier == false)
            //				.Select(c => new AgentProperties { VoyagerCompany_Id = c.VoyagerCompany_Id, Name = c.Name }).Distinct();
            //}
            return result;
        }

        public IQueryable<dynamic> GetAgentCompaniesfrommCompanies(AgentCompanyReq request)
        {
            IQueryable<AgentProperties> agents;

            var UserCompany_Id = _MongoContext.mUsers.AsQueryable().Where(x => x.VoyagerUser_Id == request.UserId).Select(y => y.Company_Id).FirstOrDefault();

            var AdminRoleId = _MongoContext.mRoles.AsQueryable().Where(x => x.LoweredRoleName == "administrator").Select(y => y.Voyager_Role_Id).FirstOrDefault();

            var AdminRole = _MongoContext.mUsersInRoles.AsQueryable().Where(x => x.UserId == request.UserId && x.RoleId == AdminRoleId).FirstOrDefault();

            if (AdminRole != null)
            {

                if (!string.IsNullOrEmpty(request.CompanyId))
                {
                    var branch = _MongoContext.mCompanies.AsQueryable().Where(a => a.Company_Id == request.CompanyId).Select(a => a.Branches).FirstOrDefault();
                    List<string> aa = branch.Select(a => a.Company_Id).ToList();

                    if (!string.IsNullOrWhiteSpace(request.AgentName))
                    {
                        agents = _MongoContext.mCompanies.AsQueryable().Where(c => (c.Iscustomer == true || c.Issubagent == true)
                        && (c.STATUS == "" || c.STATUS == " " || c.STATUS == "  " || c.STATUS == null)
                                    && (c.Name.ToLower().Contains(request.AgentName.Trim().ToLower()))
                                    && c.Company_Id != request.CompanyId
                                    && !aa.Contains(c.Company_Id)
                                    )
                                    .Select(c => new AgentProperties { VoyagerCompany_Id = c.Company_Id, Name = c.Name, Code = c.CompanyCode }).Distinct();
                    }
                    else
                    {
                        agents = _MongoContext.mCompanies.AsQueryable().Where(c => (c.Iscustomer == true || c.Issubagent == true)
                        && (c.STATUS == "" || c.STATUS == " " || c.STATUS == "  " || c.STATUS == null)
                         && c.Company_Id != request.CompanyId
                         && !aa.Contains(c.Company_Id)
                        )
                                    .Select(c => new AgentProperties { VoyagerCompany_Id = c.Company_Id, Name = c.Name, Code = c.CompanyCode }).Distinct();
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(request.AgentName))
                    {
                        agents = _MongoContext.mCompanies.AsQueryable().Where(c => (c.Iscustomer == true || c.Issubagent == true)
                        && (c.STATUS == "" || c.STATUS == " " || c.STATUS == "  " || c.STATUS == null)
                                    && (c.Name.ToLower().Contains(request.AgentName.Trim().ToLower()))
                                    )
                                    .Select(c => new AgentProperties { VoyagerCompany_Id = c.Company_Id, Name = c.Name, Code = c.CompanyCode }).Distinct();
                    }
                    else
                    {
                        agents = _MongoContext.mCompanies.AsQueryable().Where(c => (c.Iscustomer == true || c.Issubagent == true)
                        && (c.STATUS == "" || c.STATUS == " " || c.STATUS == "  " || c.STATUS == null)
                        )
                                    .Select(c => new AgentProperties { VoyagerCompany_Id = c.Company_Id, Name = c.Name, Code = c.CompanyCode }).Distinct();
                    }
                }
            }
            else
            {
                var CoreCompany_Id = _MongoContext.mSystem.AsQueryable().Select(y => y.CoreCompany_Id).FirstOrDefault();

                if (UserCompany_Id == CoreCompany_Id)
                {
                    if (!string.IsNullOrEmpty(request.CompanyId))
                    {
                        var branch = _MongoContext.mCompanies.AsQueryable().Where(a => a.Company_Id == request.CompanyId).Select(a => a.Branches).FirstOrDefault();
                        List<string> aa = branch.Select(a => a.Company_Id).ToList();

                        if (!string.IsNullOrWhiteSpace(request.AgentName))
                        {
                            agents = _MongoContext.mCompanies.AsQueryable().Where(c => (c.Iscustomer == true || c.Issubagent == true)
                            && (c.STATUS == "" || c.STATUS == " " || c.STATUS == "  " || c.STATUS == null)
                                        && (c.Name.ToLower().Contains(request.AgentName.Trim().ToLower()))
                                        && c.Company_Id != request.CompanyId
                                        && !aa.Contains(c.Company_Id)
                                        && c.Company_Id != CoreCompany_Id
                                        )
                                        .Select(c => new AgentProperties { VoyagerCompany_Id = c.Company_Id, Name = c.Name, Code = c.CompanyCode }).Distinct();
                        }
                        else
                        {
                            agents = _MongoContext.mCompanies.AsQueryable().Where(c => (c.Iscustomer == true || c.Issubagent == true)
                            && (c.STATUS == "" || c.STATUS == " " || c.STATUS == "  " || c.STATUS == null)
                             && c.Company_Id != request.CompanyId
                             && !aa.Contains(c.Company_Id)
                             && c.Company_Id != CoreCompany_Id
                            )
                                        .Select(c => new AgentProperties { VoyagerCompany_Id = c.Company_Id, Name = c.Name, Code = c.CompanyCode }).Distinct();
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(request.AgentName))
                        {
                            agents = _MongoContext.mCompanies.AsQueryable().Where(c => (c.Iscustomer == true || c.Issubagent == true)
                            && (c.STATUS == "" || c.STATUS == " " || c.STATUS == "  " || c.STATUS == null)
                                        && (c.Name.ToLower().Contains(request.AgentName.Trim().ToLower()))
                                        && c.Company_Id != CoreCompany_Id
                                        )
                                        .Select(c => new AgentProperties { VoyagerCompany_Id = c.Company_Id, Name = c.Name, Code = c.CompanyCode }).Distinct();
                        }
                        else
                        {
                            agents = _MongoContext.mCompanies.AsQueryable().Where(c => (c.Iscustomer == true || c.Issubagent == true)
                            && (c.STATUS == "" || c.STATUS == " " || c.STATUS == "  " || c.STATUS == null)
                             && c.Company_Id != CoreCompany_Id)
                                        .Select(c => new AgentProperties { VoyagerCompany_Id = c.Company_Id, Name = c.Name, Code = c.CompanyCode }).Distinct();
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(request.CompanyId))
                    {
                        var branch = _MongoContext.mCompanies.AsQueryable().Where(a => a.Company_Id == request.CompanyId).Select(a => a.Branches).FirstOrDefault();
                        List<string> aa = branch.Select(a => a.Company_Id).ToList();

                        if (!string.IsNullOrWhiteSpace(request.AgentName))
                        {
                            agents = _MongoContext.mCompanies.AsQueryable().Where(c => (c.Iscustomer == true || c.Issubagent == true)
                            && (c.STATUS == "" || c.STATUS == " " || c.STATUS == "  " || c.STATUS == null)
                                        && (c.Name.ToLower().Contains(request.AgentName.Trim().ToLower()))
                                        && c.Company_Id != request.CompanyId
                                        && !aa.Contains(c.Company_Id)
                                        && (c.ParentAgent_Id == UserCompany_Id || string.IsNullOrEmpty(UserCompany_Id))
                                        )
                                        .Select(c => new AgentProperties { VoyagerCompany_Id = c.Company_Id, Name = c.Name, Code = c.CompanyCode }).Distinct();
                        }
                        else
                        {
                            agents = _MongoContext.mCompanies.AsQueryable().Where(c => (c.Iscustomer == true || c.Issubagent == true)
                            && (c.STATUS == "" || c.STATUS == " " || c.STATUS == "  " || c.STATUS == null)
                             && c.Company_Id != request.CompanyId
                             && !aa.Contains(c.Company_Id)
                             && (c.ParentAgent_Id == UserCompany_Id || string.IsNullOrEmpty(UserCompany_Id))
                            )
                                        .Select(c => new AgentProperties { VoyagerCompany_Id = c.Company_Id, Name = c.Name, Code = c.CompanyCode }).Distinct();
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(request.AgentName))
                        {
                            agents = _MongoContext.mCompanies.AsQueryable().Where(c => (c.Iscustomer == true || c.Issubagent == true)
                            && (c.STATUS == "" || c.STATUS == " " || c.STATUS == "  " || c.STATUS == null)
                                        && (c.Name.ToLower().Contains(request.AgentName.Trim().ToLower()))
                                        && (c.ParentAgent_Id == UserCompany_Id || string.IsNullOrEmpty(UserCompany_Id))
                                        )
                                        .Select(c => new AgentProperties { VoyagerCompany_Id = c.Company_Id, Name = c.Name, Code = c.CompanyCode }).Distinct();
                        }
                        else
                        {
                            agents = _MongoContext.mCompanies.AsQueryable().Where(c => (c.Iscustomer == true || c.Issubagent == true)
                            && (c.STATUS == "" || c.STATUS == " " || c.STATUS == "  " || c.STATUS == null)
                            && (c.ParentAgent_Id == UserCompany_Id || string.IsNullOrEmpty(UserCompany_Id)))
                                        .Select(c => new AgentProperties { VoyagerCompany_Id = c.Company_Id, Name = c.Name, Code = c.CompanyCode }).Distinct();
                        }
                    }
                }
            }
            return agents;
        }

        public IQueryable<dynamic> GetSuppliersfrommCompanies(AgentCompanyReq request)
        {
            IQueryable<AgentProperties> agents;

            var UserCompany_Id = _MongoContext.mUsers.AsQueryable().Where(x => x.VoyagerUser_Id == request.UserId).Select(y => y.Company_Id).FirstOrDefault();

            var AdminRoleId = _MongoContext.mRoles.AsQueryable().Where(x => x.LoweredRoleName == "administrator").Select(y => y.Voyager_Role_Id).FirstOrDefault();

            var AdminRole = _MongoContext.mUsersInRoles.AsQueryable().Where(x => x.UserId == request.UserId && x.RoleId == AdminRoleId).FirstOrDefault();

            if (AdminRole != null)
            {

                if (!string.IsNullOrEmpty(request.CompanyId))
                {
                    var branch = _MongoContext.mCompanies.AsQueryable().Where(a => a.Company_Id == request.CompanyId).Select(a => a.Branches).FirstOrDefault();
                    List<string> aa = branch.Select(a => a.Company_Id).ToList();

                    if (!string.IsNullOrWhiteSpace(request.AgentName))
                    {
                        agents = _MongoContext.mCompanies.AsQueryable().Where(c => c.Issupplier == true
                                    && (c.Name.ToLower().Contains(request.AgentName.Trim().ToLower()))
                                    && c.Company_Id != request.CompanyId
                                    && !aa.Contains(c.Company_Id)
                                    )
                                    .Select(c => new AgentProperties { VoyagerCompany_Id = c.Company_Id, Name = c.Name, Code = c.CompanyCode }).Distinct();
                    }
                    else
                    {
                        agents = _MongoContext.mCompanies.AsQueryable().Where(c => c.Issupplier == true
                         && c.Company_Id != request.CompanyId
                         && !aa.Contains(c.Company_Id)
                        ).Select(c => new AgentProperties { VoyagerCompany_Id = c.Company_Id, Name = c.Name, Code = c.CompanyCode }).Distinct();
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(request.AgentName))
                    {
                        agents = _MongoContext.mCompanies.AsQueryable().Where(c => c.Issupplier == true
                                    && (c.Name.ToLower().Contains(request.AgentName.Trim().ToLower()))
                                    )
                                    .Select(c => new AgentProperties { VoyagerCompany_Id = c.Company_Id, Name = c.Name, Code = c.CompanyCode }).Distinct();
                    }
                    else
                    {
                        agents = _MongoContext.mCompanies.AsQueryable().Where(c => c.Issupplier == true
                        ).Select(c => new AgentProperties { VoyagerCompany_Id = c.Company_Id, Name = c.Name, Code = c.CompanyCode }).Distinct();
                    }
                }
            }
            else
            {
                var CoreCompany_Id = _MongoContext.mSystem.AsQueryable().Select(y => y.CoreCompany_Id).FirstOrDefault();

                if (UserCompany_Id == CoreCompany_Id)
                {
                    if (!string.IsNullOrEmpty(request.CompanyId))
                    {
                        var branch = _MongoContext.mCompanies.AsQueryable().Where(a => a.Company_Id == request.CompanyId).Select(a => a.Branches).FirstOrDefault();
                        List<string> aa = branch.Select(a => a.Company_Id).ToList();

                        if (!string.IsNullOrWhiteSpace(request.AgentName))
                        {
                            agents = _MongoContext.mCompanies.AsQueryable().Where(c => c.Issupplier == true
                                        && (c.Name.ToLower().Contains(request.AgentName.Trim().ToLower()))
                                        && c.Company_Id != request.CompanyId
                                        && !aa.Contains(c.Company_Id)
                                        && c.Company_Id != CoreCompany_Id
                                        )
                                        .Select(c => new AgentProperties { VoyagerCompany_Id = c.Company_Id, Name = c.Name, Code = c.CompanyCode }).Distinct();
                        }
                        else
                        {
                            agents = _MongoContext.mCompanies.AsQueryable().Where(c => c.Issupplier == true
                             && c.Company_Id != request.CompanyId
                             && !aa.Contains(c.Company_Id)
                             && c.Company_Id != CoreCompany_Id
                            )
                                        .Select(c => new AgentProperties { VoyagerCompany_Id = c.Company_Id, Name = c.Name, Code = c.CompanyCode }).Distinct();
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(request.AgentName))
                        {
                            agents = _MongoContext.mCompanies.AsQueryable().Where(c => c.Issupplier == true
                                        && (c.Name.ToLower().Contains(request.AgentName.Trim().ToLower()))
                                        && c.Company_Id != CoreCompany_Id
                                        )
                                        .Select(c => new AgentProperties { VoyagerCompany_Id = c.Company_Id, Name = c.Name, Code = c.CompanyCode }).Distinct();
                        }
                        else
                        {
                            agents = _MongoContext.mCompanies.AsQueryable().Where(c => c.Issupplier == true
                             && c.Company_Id != CoreCompany_Id)
                                        .Select(c => new AgentProperties { VoyagerCompany_Id = c.Company_Id, Name = c.Name, Code = c.CompanyCode }).Distinct();
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(request.CompanyId))
                    {
                        var branch = _MongoContext.mCompanies.AsQueryable().Where(a => a.Company_Id == request.CompanyId).Select(a => a.Branches).FirstOrDefault();
                        List<string> aa = branch.Select(a => a.Company_Id).ToList();

                        if (!string.IsNullOrWhiteSpace(request.AgentName))
                        {
                            agents = _MongoContext.mCompanies.AsQueryable().Where(c => c.Issupplier == true
                                        && (c.Name.ToLower().Contains(request.AgentName.Trim().ToLower()))
                                        && c.Company_Id != request.CompanyId
                                        && !aa.Contains(c.Company_Id)
                                        && (c.ParentAgent_Id == UserCompany_Id || string.IsNullOrEmpty(UserCompany_Id))
                                        )
                                        .Select(c => new AgentProperties { VoyagerCompany_Id = c.Company_Id, Name = c.Name, Code = c.CompanyCode }).Distinct();
                        }
                        else
                        {
                            agents = _MongoContext.mCompanies.AsQueryable().Where(c => c.Issupplier == true
                             && c.Company_Id != request.CompanyId
                             && !aa.Contains(c.Company_Id)
                             && (c.ParentAgent_Id == UserCompany_Id || string.IsNullOrEmpty(UserCompany_Id))
                            )
                                        .Select(c => new AgentProperties { VoyagerCompany_Id = c.Company_Id, Name = c.Name, Code = c.CompanyCode }).Distinct();
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(request.AgentName))
                        {
                            agents = _MongoContext.mCompanies.AsQueryable().Where(c => c.Issupplier == true
                                        && (c.Name.ToLower().Contains(request.AgentName.Trim().ToLower()))
                                        && (c.ParentAgent_Id == UserCompany_Id || string.IsNullOrEmpty(UserCompany_Id))
                                        )
                                        .Select(c => new AgentProperties { VoyagerCompany_Id = c.Company_Id, Name = c.Name, Code = c.CompanyCode }).Distinct();
                        }
                        else
                        {
                            agents = _MongoContext.mCompanies.AsQueryable().Where(c => c.Issupplier == true
                            && (c.ParentAgent_Id == UserCompany_Id || string.IsNullOrEmpty(UserCompany_Id)))
                                        .Select(c => new AgentProperties { VoyagerCompany_Id = c.Company_Id, Name = c.Name, Code = c.CompanyCode }).Distinct();
                        }
                    }
                }
            }
            agents = agents.Take(500);
            agents = agents.Where(x => x.Name != "");
            return agents;
        }

        public AgentProperties GetAgentCompaniesByID(string id)
        {
            //var agents = _MongoContext.mCompany.AsQueryable().Where(c => c.Iscustomer == true && c.VoyagerCompany_Id == id)
            //				.Select(c => new AgentProperties { VoyagerCompany_Id = c.VoyagerCompany_Id, Name = c.Name }).FirstOrDefault();
            //return agents;

            var agents = _MongoContext.mCompanies.AsQueryable().Where(c => c.Iscustomer == true && c.Company_Id == id)
                            .Select(c => new AgentProperties { VoyagerCompany_Id = c.Company_Id, Name = c.Name }).FirstOrDefault();
            return agents;
        }

        public CompanyDetailsRes GetCompanyDetails()
        {
            var System = _MongoContext.mSystem.AsQueryable().FirstOrDefault();

            var company = _MongoContext.mContacts.AsQueryable().Where(c => c.Company_Id == System.CoreCompany_Id && c.Default == 1)
                            .Select(c => new CompanyDetailsRes { SystemEmail = c.MAIL, SystemPhone = c.TEL, SystemWebsite = c.WEB }).FirstOrDefault();
            return company;
        }

        public IQueryable<dynamic> GetContactsForAgentCompany(AgentContactReq request)
        {
            //var contacts = _MongoContext.mContacts.AsQueryable().Where(c => c.Company_Id == request.Company_Id.Trim() && c.STATUS == "")
            //                .Select(c => new ContactProperties
            //                {
            //                    VoyagerContact_Id = c.VoyagerContact_Id,
            //                    FullName = string.IsNullOrEmpty(c.CommonTitle) ? c.FIRSTNAME + " " + c.LastNAME : c.CommonTitle + " " + c.FIRSTNAME + " " + c.LastNAME
            //                }).Distinct();
            //return contacts;

            return _MongoContext.mCompanies.AsQueryable().Where(c => c.Company_Id == request.Company_Id.Trim() && (c.STATUS == " " || c.STATUS == "" || c.STATUS == null))
                .Select(a => a.ContactDetails.Where(c => c.STATUS == null || c.STATUS == "" || c.STATUS == " " || c.STATUS == "  ")).FirstOrDefault().AsQueryable()
                .Select(b => new ContactProperties
                {
                    VoyagerContact_Id = b.Contact_Id,
                    FullName = string.IsNullOrEmpty(b.CommonTitle) ? b.FIRSTNAME + " " + b.LastNAME : b.CommonTitle + " " + b.FIRSTNAME + " " + b.LastNAME
                }).Distinct().OrderBy(x => x.FullName);
        }

        public string CheckDuplicateQRFTourName(AgentContactReq request)
        {
            try
            {
                if (_MongoContext.mQuote.AsQueryable().Where(c => c.AgentProductInfo.TourName == request.Company_Id).Count() > 0)
                {
                    return "invalid";
                }
                else
                {
                    return "valid";
                }
            }
            catch (Exception)
            {
                return "invalid";
            }
        }

        public async Task<AgentContactDetailsProperties> GetContactDetailsByAgentAndContactID(AgentContactDetailsReq request)
        {
            //FilterDefinition<mContacts> filter;
            //filter = Builders<mContacts>.Filter.Empty;

            //if (!string.IsNullOrWhiteSpace(request.VoyagerContact_Id))
            //{
            //	filter = filter & Builders<mContacts>.Filter.Eq(f => f.VoyagerContact_Id, request.VoyagerContact_Id.Trim());
            //}
            //var result = await _MongoContext.mContacts.Find(filter).Project(c => new AgentContactDetailsProperties
            //{
            //	MAIL = c.MAIL,
            //	MOBILE = c.MOBILE,
            //	CommonTitle = c.CommonTitle,
            //	Fax = c.FAX,
            //	FirstName = c.FIRSTNAME,
            //	LastName = c.LastNAME,
            //	Telephone = c.TEL

            //}).FirstOrDefaultAsync();

            //return result;

            var result = _MongoContext.mCompanies.AsQueryable().Where(x => x.ContactDetails.Any(y => y.Contact_Id == request.VoyagerContact_Id)).FirstOrDefault()?.ContactDetails;

            var result1 = result.Where(a => a.Contact_Id == request.VoyagerContact_Id).Select(c => new AgentContactDetailsProperties
            {
                MAIL = c.MAIL,
                MOBILE = c.MOBILE,
                CommonTitle = c.CommonTitle,
                Fax = c.FAX,
                FirstName = c.FIRSTNAME,
                LastName = c.LastNAME,
                Telephone = c.TEL
            }).FirstOrDefault();

            //var result = contacts.Select(c => new AgentContactDetailsProperties
            //{
            //	MAIL = c.MAIL,
            //	MOBILE = c.MOBILE,
            //	CommonTitle = c.CommonTitle,
            //	Fax = c.FAX,
            //	FirstName = c.FIRSTNAME,
            //	LastName = c.LastNAME,
            //	Telephone = c.TEL
            //}).FirstOrDefault();

            return result1 ?? new AgentContactDetailsProperties();
        }

        public ContactProperties GetContactsForAgentCompanyByID(string id)
        {
            //var contacts = _MongoContext.mCompanies.AsQueryable().Where(x => x.ContactDetails.Any(y => y.Contact_Id == id)).FirstOrDefault()?.ContactDetails
            //                //var contacts = _MongoContext.mContacts.AsQueryable().Where(c => c.VoyagerContact_Id == id)
            //                .Select(c => new ContactProperties
            //                {
            //                    VoyagerContact_Id = c.Contact_Id,
            //                    FullName = string.IsNullOrEmpty(c.CommonTitle) ? c.FIRSTNAME + " " + c.LastNAME : c.CommonTitle + " " + c.FIRSTNAME + " " + c.LastNAME
            //                }).FirstOrDefault();

            var contactsList = _MongoContext.mCompanies.AsQueryable().Where(x => x.ContactDetails.Any(y => y.Contact_Id == id)).FirstOrDefault().ContactDetails;

            var contacts = contactsList.Where(a => a.Contact_Id == id).Select(c => new ContactProperties
            {
                VoyagerContact_Id = c.Contact_Id,
                FullName = string.IsNullOrEmpty(c.CommonTitle) ? c.FIRSTNAME + " " + c.LastNAME : c.CommonTitle + " " + c.FIRSTNAME + " " + c.LastNAME
            }).FirstOrDefault();

            return contacts;
        }

        public string GetValueofAttributeFromMaster(mTypeMaster request, string AttributeName, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                var result = request.PropertyType.Attribute.Where(a => a.AttributeName.ToLower() == AttributeName.ToLower()).ToList();
                return result != null && result.Count > 0 ? result[0].Values.Where(a => a.AttributeValue_Id == value).FirstOrDefault().Value : "";
            }
            return null;
        }

        public async Task<string> InsertUpdateQRFAgentDetails(QUOTEAgentSetReq request)
        {
            mQuote mQuote = new mQuote();
            mQuote mqute = new mQuote();
            try
            {
                mTypeMaster result = (from u in _MongoContext.mTypeMaster.AsQueryable()
                                      where u.PropertyType.PropertyName == "QRF Masters"
                                      select u).FirstOrDefault();
                if (result != null)
                {
                    mQuote.AgentInfo = request.AgentInfo;
                    mQuote.AgentInfo.AgentName = GetAgentCompaniesByID(request.AgentInfo.AgentID).Name;
                    mQuote.AgentInfo.ContactPerson = GetContactsForAgentCompanyByID(request.AgentInfo.ContactPersonID) == null ? "" : GetContactsForAgentCompanyByID(request.AgentInfo.ContactPersonID).FullName;

                    mQuote.AgentPassengerInfo = request.AgentPassengerInfo;
                    mQuote.AgentPaymentInfo = request.AgentPaymentInfo;

                    mQuote.AgentProductInfo = request.AgentProductInfo;
                    mQuote.AgentProductInfo.Type = GetValueofAttributeFromMaster(result, "Quote Type", request.AgentProductInfo.TypeID);
                    //mQuote.AgentProductInfo.Division = GetValueofAttributeFromMaster(result, "Division Type", request.AgentProductInfo.DivisionID);
                    mQuote.AgentProductInfo.DivisionID = request.AgentProductInfo.DivisionID;
                    mQuote.AgentProductInfo.Division = request.AgentProductInfo.Division;
                    mQuote.AgentProductInfo.Product = request.AgentProductInfo.ProductID;
                    mQuote.AgentProductInfo.PurposeofTravel = GetValueofAttributeFromMaster(result, "Purpose of Travel", request.AgentProductInfo.PurposeofTravelID);
                    mQuote.AgentProductInfo.Destination = GetValueofAttributeFromMaster(result, "QRF Destination", request.AgentProductInfo.DestinationID);

                    List<AgentRoom> lstAgentRoom = new List<AgentRoom>();
                    foreach (var item in request.AgentRoomInfo)
                    {
                        //var value = GetValueofAttributeFromMaster(result, "Quote Room Type", item);
                        if (item != null && !string.IsNullOrEmpty(item.RoomTypeID))
                        {
                            AgentRoom objAgentRoom = new AgentRoom { RoomTypeID = item.RoomTypeID, RoomTypeName = item.RoomTypeName, RoomCount = item.RoomCount };
                            lstAgentRoom.Add(objAgentRoom);
                        }
                    }
                    mQuote.AgentRoom = lstAgentRoom;

                    var resultCur = _MongoContext.mCurrency.AsQueryable().Where(c => c.Currency == request.AgentProductInfo.BudgetCurrencyID).ToList();
                    if (resultCur != null && resultCur.Count() > 0)
                    {
                        mQuote.AgentProductInfo.BudgetCurrency = resultCur.FirstOrDefault().Name;
                        mQuote.AgentProductInfo.BudgetCurrencyCode = resultCur.FirstOrDefault().Currency;
                        mQuote.AgentProductInfo.BudgetCurrencyID = resultCur.FirstOrDefault().VoyagerCurrency_Id;
                    }
                    else
                    {
                        mQuote.AgentProductInfo.BudgetCurrency = "";
                        mQuote.AgentProductInfo.BudgetCurrencyCode = "";
                        mQuote.AgentProductInfo.BudgetCurrencyID = "";
                    }


                    //Data for Mapping in quote
                    if (!string.IsNullOrEmpty(request.Application) && !string.IsNullOrEmpty(request.PartnerEntityCode))
                    {

                        mqute = _MongoContext.mQuote.AsQueryable().Where(a => a.Mappings != null
                     && a.Mappings.Any(b => b.PartnerEntityCode == request.PartnerEntityCode && b.Application.ToLower() == request.Application.ToLower())).FirstOrDefault();

                        if (mqute == null)
                        {
                            // mQuote.Mappings = new List<QuoteMappings>();
                            List<QuoteMappings> mappings = new List<QuoteMappings>();
                            mApplications Application = _MongoContext.mApplications.AsQueryable().Where(a => a.Application_Name.ToLower() == request.Application.ToLower()).FirstOrDefault();
                            QuoteMappings quotemapping = new QuoteMappings();

                            quotemapping.Application_Id = Application.Application_Id;
                            quotemapping.Application = Application.Application_Name;
                            quotemapping.PartnerEntityName = request.Module;
                            quotemapping.PartnerEntityCode = request.PartnerEntityCode;
                            quotemapping.PartnerEntityType = _configuration.GetValue<string>("MappingDefault:PartnerEntityTypeOpportunity");
                            quotemapping.Action = _configuration.GetValue<string>("MappingDefault:ActionCreate");
                            quotemapping.Status = string.Empty;
                            quotemapping.CreateUser = request.SalesPerson;
                            quotemapping.CreateDate = DateTime.Now;
                            quotemapping.EditUser = null;
                            quotemapping.EditDate = null;
                            if (mQuote.Mappings == null)
                            {
                                mQuote.Mappings = new List<QuoteMappings>();
                            }

                            mQuote.Mappings.Add(quotemapping);
                        }
                        else
                        {

                            //  mQuote.Mappings = new List<QuoteMappings>();
                            List<QuoteMappings> mapping = new List<QuoteMappings>();
                            QuoteMappings quotemappings = new QuoteMappings();
                            var savedQuoteMappingDetails = mqute.Mappings.Where(x => x.PartnerEntityCode == request.PartnerEntityCode).FirstOrDefault();
                            savedQuoteMappingDetails.PartnerEntityType = _configuration.GetValue<string>("MappingDefault:PartnerEntityTypeOpportunity");
                            savedQuoteMappingDetails.Action = _configuration.GetValue<string>("MappingDefault:ActionUpdate");
                            savedQuoteMappingDetails.Status = string.Empty;
                            savedQuoteMappingDetails.EditUser = request.SalesPerson;
                            savedQuoteMappingDetails.EditDate = DateTime.Now;
                            mQuote.Mappings = mqute.Mappings;
                        }
                    }
                    //insert code
                    if (string.IsNullOrEmpty(request.QRFID))
                    {
                        QRFCounterRequest qrfCounterRequest = new QRFCounterRequest { CounterType = _configuration["CounterType:QRF"].ToString() };

                        mQuote.CurrentPipeline = request.CurrentPipeline;
                        mQuote.CurrentPipelineStep = request.CurrentPipelineStep;
                        mQuote.Remarks = request.Remarks;
                        mQuote.CurrentPipelineSubStep = request.CurrentPipelineSubStep;
                        mQuote.QuoteResult = request.QuoteResult;
                        mQuote.Status = request.Status;
                        mQuote.SalesPerson = request.SalesPerson;
                        mQuote.SalesPersonUserName = request.SalesPersonUserName;
                        mQuote.SalesPersonCompany = request.SalesPersonCompany;
                        mQuote.CreateUser = request.SalesPerson;
                        mQuote.CreateDate = DateTime.Now;
                        mQuote.EditDate = null;
                        mQuote.EditUser = null;
                        mQuote.RegenerateItinerary = true;

                        //Get CompanyId of logged in user from mCompanies using userid
                        string SystemCompanyId = string.Empty;
                        _agentRepository.GetSystemCompany(request.LoggedInUserContact_Id, out SystemCompanyId);

                        mQuote.SystemCompany_Id = SystemCompanyId;
                        mQuote.QRFID = _genericRepository.GetNextReferenceNumber(qrfCounterRequest).LastReferenceNumber.ToString();

                        #region ExchangeRate Sanpshot

                        var ExchangeRate = _MongoContext.mExchangeRate.AsQueryable().Where(a => a.DateMin <= DateTime.Now && DateTime.Now <= a.DateMax).FirstOrDefault();

                        if (ExchangeRate != null)
                        {
                            mQuote.ExchangeRateSnapshot.ExchangeRateSnapshot_ID = Guid.NewGuid().ToString();
                            mQuote.ExchangeRateSnapshot.ExchangeRate_id = ExchangeRate.ExchangeRateId;
                            mQuote.ExchangeRateSnapshot.EXRATE = ExchangeRate.ExRate;
                            mQuote.ExchangeRateSnapshot.REFCUR = ExchangeRate.RefCur;
                            mQuote.ExchangeRateSnapshot.DATEMIN = ExchangeRate.DateMin;
                            mQuote.ExchangeRateSnapshot.DATEMAX = ExchangeRate.DateMax;
                            mQuote.ExchangeRateSnapshot.VATRATE = ExchangeRate.VatRate;
                            mQuote.ExchangeRateSnapshot.Currency_Id = ExchangeRate.Currency_Id;
                            mQuote.ExchangeRateSnapshot.CREA_DT = DateTime.Now;

                            var ExchangeRateDetailList = _MongoContext.mExchangeRateDetail.AsQueryable().Where(a => a.ExchangeRate_Id == ExchangeRate.ExchangeRateId).ToList();
                            if (ExchangeRateDetailList?.Count > 0)
                            {
                                mQuote.ExchangeRateSnapshot.ExchangeRateDetail = ExchangeRateDetailList.Select(x => new ExchangeRateDetailSnapshot()
                                {
                                    ExchangeRateDetailSnapshot_Id = Guid.NewGuid().ToString(),
                                    ExchangeRateDetail_Id = x.ExchangeRateDetail_Id,
                                    Currency_Id = x.Currency_Id,
                                    CURRENCY = x.CURRENCY,
                                    RATE = x.RATE,
                                    ROUNDTO = x.ROUNDTO,
                                    CREA_DT = DateTime.Now
                                }).ToList();
                            }
                        }

                        #endregion

                        await _MongoContext.mQuote.InsertOneAsync(mQuote);
                        return mQuote.QRFID;

                    }//update code
                    else
                    {
                        var resultFlag = await _MongoContext.mQuote.FindOneAndUpdateAsync(
                                            Builders<mQuote>.Filter.Eq("QRFID", request.QRFID),
                                            Builders<mQuote>.Update.
                                            Set("AgentInfo", mQuote.AgentInfo).
                                            Set("AgentPassengerInfo", mQuote.AgentPassengerInfo).
                                            Set("AgentPaymentInfo", mQuote.AgentPaymentInfo).
                                            Set("AgentProductInfo", mQuote.AgentProductInfo).
                                            Set("AgentRoom", mQuote.AgentRoom).
                                            Set("CurrentPipeline", request.CurrentPipeline).
                                            Set("CurrentPipelineStep", request.CurrentPipelineStep).
                                            Set("Remarks", request.Remarks).
                                            Set("CurrentPipelineSubStep", request.CurrentPipelineSubStep).
                                            Set("QuoteResult", request.QuoteResult).
                                            Set("Mappings", mQuote.Mappings).
                                            Set("Status", request.Status).
                                            Set("SalesPerson", request.SalesPerson).
                                            Set("SalesPersonUserName", request.SalesPersonUserName).
                                            Set("SalesPersonCompany", request.SalesPersonCompany).
                                            Set("EditUser", request.SalesPerson).
                                            Set("EditDate", DateTime.Now)
                                            );
                        return request.QRFID;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (MongoWriteException)
            {
                //if (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
                //{
                //    // mwx.WriteError.Message contains the duplicate key error message
                //}
                return null;
            }
        }

        public async Task<IList<QuoteSearchDetails>> GetQRFAgentDetailsBySearchCriteria(QuoteSearchReq request)
        {
            // var result = _MongoContext.mQuote.AsQueryable().Where(q => q.AgentInfo.AgentNameText == QRFQUOTEAgentRequest.AgentInfo.AgentNameText.Trim()
            //&& q.QRFID == QRFQUOTEAgentRequest.QRFID && q.AgentProductInfo.AgentTourCode == QRFQUOTEAgentRequest.AgentProductInfo.AgentTourCode.Trim()
            //&& q.AgentProductInfo.AgentTourName.Contains(QRFQUOTEAgentRequest.AgentProductInfo.AgentTourName.Trim())
            //&& q.QRFStatus == QRFQUOTEAgentRequest.QRFStatus);

            try
            {
                if (request.To != null)
                    request.To = request.To.Value.AddHours(23).AddMinutes(59).AddSeconds(59);

                FilterDefinition<mQuote> filter;
                filter = Builders<mQuote>.Filter.Empty;

                var UserCompany_Id = _MongoContext.mUsers.AsQueryable().Where(x => x.VoyagerUser_Id == request.UserId).Select(y => y.Company_Id).FirstOrDefault();
                var AdminRoleId = _MongoContext.mRoles.AsQueryable().Where(x => x.LoweredRoleName == "administrator").Select(y => y.Voyager_Role_Id).FirstOrDefault();
                var AdminRole = _MongoContext.mUsersInRoles.AsQueryable().Where(x => x.UserId == request.UserId && x.RoleId == AdminRoleId).FirstOrDefault();

                if (AdminRole == null)//means user is not an Admin
                {
                    var CoreCompany_Id = _MongoContext.mSystem.AsQueryable().Select(y => y.CoreCompany_Id).FirstOrDefault();
                    if (UserCompany_Id == CoreCompany_Id)
                    {
                        if (!string.IsNullOrWhiteSpace(CoreCompany_Id))
                        {
                            filter = filter & Builders<mQuote>.Filter.Where(x => x.AgentInfo.AgentID != CoreCompany_Id);
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(UserCompany_Id))
                        {
                            var lstCompanies = _MongoContext.mCompanies.AsQueryable().Where(x => x.ParentAgent_Id == UserCompany_Id).Select(y => y.Company_Id).ToList();
                            filter = filter & Builders<mQuote>.Filter.Where(x => lstCompanies.Contains(x.AgentInfo.AgentID));
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(request.CurrentPipeline))
                {
                    filter = filter & Builders<mQuote>.Filter.Eq(f => f.CurrentPipeline, request.CurrentPipeline.Trim());
                }

                if (!string.IsNullOrEmpty(request.QRFID))
                {
                    filter = filter & Builders<mQuote>.Filter.Eq(f => f.QRFID, request.QRFID);
                }

                if (!string.IsNullOrWhiteSpace(request.AgentName))
                {
                    filter = filter & Builders<mQuote>.Filter.Regex(x => x.AgentInfo.AgentName, new BsonRegularExpression(new Regex(request.AgentName, RegexOptions.IgnoreCase)));
                }

                if (!string.IsNullOrWhiteSpace(request.TourCode))
                {
                    filter = filter & Builders<mQuote>.Filter.Regex(x => x.AgentProductInfo.TourCode, new BsonRegularExpression(new Regex(request.TourCode, RegexOptions.IgnoreCase)));
                }

                if (!string.IsNullOrWhiteSpace(request.TourName))
                {
                    filter = filter & Builders<mQuote>.Filter.Regex(x => x.AgentProductInfo.TourName, new BsonRegularExpression(new Regex(request.TourName, RegexOptions.IgnoreCase)));
                }

                if (!string.IsNullOrWhiteSpace(request.Priority))
                {
                    filter = filter & Builders<mQuote>.Filter.Regex(x => x.AgentProductInfo.Priority, new BsonRegularExpression(new Regex(request.Priority, RegexOptions.IgnoreCase)));
                }

                if (!string.IsNullOrWhiteSpace(request.QuoteResult))
                {
                    filter = filter & Builders<mQuote>.Filter.Regex(x => x.QuoteResult, new BsonRegularExpression(new Regex(request.QuoteResult, RegexOptions.IgnoreCase)));
                }

                if (!string.IsNullOrWhiteSpace(request.Division))
                {
                    filter = filter & Builders<mQuote>.Filter.Regex(x => x.AgentProductInfo.Division, new BsonRegularExpression(new Regex(request.Division, RegexOptions.IgnoreCase)));
                }

                if (!string.IsNullOrWhiteSpace(request.Destination))
                {
                    filter = filter & Builders<mQuote>.Filter.Where(x => x.AgentProductInfo.DestinationID == request.Destination);
                }

                //if (!string.IsNullOrWhiteSpace(request.QRFStatus))
                //{
                //    filter = filter & Builders<mQuote>.Filter.Regex(x => x.Status, new BsonRegularExpression(new Regex(request.QRFStatus, RegexOptions.IgnoreCase)));
                //}

                if (!string.IsNullOrWhiteSpace(request.Date))
                {
                    if (request.Date.ToLower().Trim() == "creation date")
                    {
                        if (request.From != null && request.To != null)
                        {
                            DateTime todt = new DateTime();
                            todt = request.To.Value.AddHours(23).AddMinutes(59).AddSeconds(59);
                            filter = filter & Builders<mQuote>.Filter.Where(x => x.CreateDate >= request.From && x.CreateDate <= todt);
                        }
                        else if (request.From != null && request.To == null)
                        {
                            filter = filter & Builders<mQuote>.Filter.Where(x => x.CreateDate >= request.From);
                        }
                        else if (request.From == null && request.To != null)
                        {
                            DateTime todt = new DateTime();
                            todt = request.To.Value.AddHours(23).AddMinutes(59).AddSeconds(59);
                            filter = filter & Builders<mQuote>.Filter.Where(x => x.CreateDate <= todt);
                        }
                    }
                    else if (request.Date.ToLower().Trim() == "travel date")
                    {
                        DateTime todt = new DateTime();
                        if (request.From != null)
                        {
                            filter = filter & Builders<mQuote>.Filter.Where(x => x.Departures.Any(a => a.Date >= request.From));
                        }
                        else if (request.From == null && request.To != null)
                        {
                            todt = request.To.Value.AddHours(23).AddMinutes(59).AddSeconds(59);
                            filter = filter & Builders<mQuote>.Filter.Where(x => x.Departures.Any(a => a.Date <= todt));
                        }
                    }
                }

                var result = await _MongoContext.mQuote.Find(filter).Project(q => new QuoteSearchDetails
                {
                    AgentCompanyName = q.AgentInfo.AgentName,
                    AgentContactName = q.AgentInfo.ContactPerson,
                    AgentMobileNo = q.AgentInfo.MobileNo,
                    AgentEmailAddress = q.AgentInfo.EmailAddress,
                    AgentPassengerInfo = q.AgentPassengerInfo.Where(a => a.Type.ToLower() == "adult").FirstOrDefault(),
                    TourName = q.AgentProductInfo.TourName,
                    TourCode = q.AgentProductInfo.TourCode,
                    Destination = q.AgentProductInfo.Destination,
                    QRFDuration = q.AgentProductInfo.Duration,
                    QRFID = q.QRFID,
                    CreateDate = q.CreateDate,
                    // FollowUpItem = GetFollowUpByQuoteSearchCriteria(request, q.QRFID),
                    DeparturesDate = q.Departures == null ? new List<DateTime?>() : q.Departures.Select(a => a.Date).ToList(),
                    FollowUp = GetLatestFollowUpForQRF(q.QRFID, q.CurrentPipeline)
                }).ToListAsync();

                if (!string.IsNullOrWhiteSpace(request.Date))
                {
                    if (request.Date.ToLower().Trim() == "creation date")
                    {
                        if (!string.IsNullOrEmpty(request.Month) && request.Year > 0)
                        {
                            result = result.Where(x => x.CreateDate.Year == request.Year && x.CreateDate.ToString("MMMM") == request.Month).ToList();
                        }
                    }
                    else if (request.Date.ToLower().Trim() == "travel date")
                    {
                        if (!string.IsNullOrEmpty(request.Month) && request.Year > 0)
                        {
                            result = result.Where(x => x.DeparturesDate.Any(a => a.Value.Year == request.Year && a.Value.ToString("MMMM") == request.Month)).ToList();
                        }
                    }
                }

                return result != null && result.Count > 0 ? result.OrderByDescending(q => q.CreateDate).ToList() : (new List<QuoteSearchDetails>());

            }
            catch (Exception e)
            {

                throw;
            }
        }

        public async Task<QuoteAgentGetProperties> GetQRFAgentDetailsByQRFID(QuoteAgentGetReq request)
        {
            var builder = Builders<mQuote>.Filter;
            var filter = builder.Where(q => q.QRFID == request.QRFID);
            bool IsLinkedQRFsExist = ChcekLinkedQRFsExist(request.QRFID).Result;
            return await _MongoContext.mQuote.Find(filter).Project(q => new QuoteAgentGetProperties
            {
                QRFID = q.QRFID,
                CurrentPipeline = q.CurrentPipeline,
                CurrentPipelineStep = q.CurrentPipelineStep,
                CurrentPipelineSubStep = q.CurrentPipelineSubStep,
                Status = q.Status,
                QuoteResult = q.QuoteResult,
                Remarks = q.Remarks,
                SalesPerson = q.SalesPerson,
                SalesPersonUserName = q.SalesPersonUserName,
                SalesPersonCompany = q.SalesPersonCompany,
                CostingOfficer = q.CostingOfficer,
                ValidForAcceptance = q.ValidForAcceptance,
                ValidForTravel = q.ValidForTravel,
                AgentInfo = q.AgentInfo,
                AgentProductInfo = q.AgentProductInfo,
                AgentPassengerInfo = q.AgentPassengerInfo,
                AgentRoom = q.AgentRoom,
                AgentPaymentInfo = q.AgentPaymentInfo,
                DepartureDates = q.Departures,
                IsLinkedQRFsExist = IsLinkedQRFsExist
            }).FirstOrDefaultAsync();
        }

        public async Task<DivisionGetRes> GetDivision(QuoteSearchReq request)
        {
            try
            {
                DivisionGetRes division = new DivisionGetRes();
                if (!string.IsNullOrWhiteSpace(request.UserName))
                {
                    var UserCompany = _MongoContext.mCompanies.AsQueryable().Where(x => x.ContactDetails.Any(y => y.UserName.ToLower() == request.UserName.ToLower().Trim())).Select(z => new { z.SystemCompany_Id, z.Company_Id }).FirstOrDefault();
                    var sysCompanyId = UserCompany.SystemCompany_Id;//.Select(z => z.SystemCompany_Id)
                    if (!string.IsNullOrWhiteSpace(sysCompanyId))
                    {
                        var company = _MongoContext.mCompanies.AsQueryable().Where(x => x.Company_Id == sysCompanyId).FirstOrDefault();
                        division.DivisionList = company.Branches.Select(x => new AttributeValues { AttributeValue_Id = x.Company_Id, Value = x.Company_Name }).OrderBy(a => a.Value).ToList();
                        division.CompanyDivision = UserCompany.Company_Id;
                    }
                }
                return division;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #endregion

        #region Departures
        public async Task<DepartureDateSetResponse> SetDepartureDatesForQRF_Id(DepartureDateSetRequest req)
        {
            var response = new DepartureDateSetResponse();
            QRFCounterRequest qrfCounterRequest = new QRFCounterRequest { CounterType = _configuration["CounterType:QRFDeparture"].ToString() };
            var result = _MongoContext.mQuote.AsQueryable().Where(a => a.QRFID == req.QRFID).FirstOrDefault();
            if (result != null)
            {
                List<DepartureDates> lstDepDates = result.Departures;

                //fetch the departures from req object which are deleted
                var lstdeptmodify = req.Departures.Where(a => a.IsDeleted).ToList();

                #region To set ValidForTravel and ValidForAcceptance in Quote
                string ValidForTravelDate = "";
                string ValidForAcceptance = "";
                List<DepartureDates> dates = new List<DepartureDates>();
                dates = req.Departures.Where(y => y.IsDeleted == false).OrderBy(x => x.Date).ToList();
                ValidForAcceptance = "On or before " + Convert.ToDateTime((DateTime.Now).AddDays(7)).ToString("dd MMM yy");

                if (dates.Count() == 1)
                {
                    ValidForTravelDate = Convert.ToDateTime(dates.First().Date).ToString("dd MMM yy");

                    //DateTime ldate = Convert.ToDateTime(dates.First().Date);
                    //DateTime date = ldate.AddDays(-15);
                    //if (date.Date < DateTime.Now.Date)
                    //    ValidForAcceptance = "On or before " + Convert.ToDateTime((DateTime.Now).AddDays(1)).ToString("dd MMM yy");
                    //else
                    //    ValidForAcceptance = "On or before " + Convert.ToDateTime(date).ToString("dd MMM yy");
                }
                else
                {
                    ValidForTravelDate = Convert.ToDateTime(dates.First().Date).ToString("dd MMM yy") + " - " + Convert.ToDateTime(dates.Last().Date).ToString("dd MMM yy");
                    //DateTime ldate = Convert.ToDateTime(dates.Last().Date);
                    //DateTime date = ldate.AddDays(-15);

                    //if (date.Date < DateTime.Now.Date)
                    //    ValidForAcceptance = "On or before " + Convert.ToDateTime((DateTime.Now).AddDays(1)).ToString("dd MMM yy");
                    //else
                    //    ValidForAcceptance = "On or before " + Convert.ToDateTime(date).ToString("dd MMM yy");
                }
                #endregion

                if (lstDepDates != null && lstDepDates.Count > 0)//if exists or not then update as whole Routing Info List
                {
                    req.Departures.AddRange(lstDepDates.Where(x => x.IsDeleted == true));
                    if (req.Departures != null && req.Departures.Count > 0)
                    {
                        //fetch the departures from req object which are new
                        var lstDeptNew = new List<DepartureDates>();
                        lstDeptNew.AddRange(req.Departures.FindAll(f => !lstDepDates.Exists(r => r.Departure_Id == f.Departure_Id)));

                        req.Departures.FindAll(f => !lstDepDates.Exists(r => r.Departure_Id == f.Departure_Id)).ForEach
                       (r =>
                       {
                           r.Departure_Id = _genericRepository.GetNextReferenceNumber(qrfCounterRequest).LastReferenceNumber;
                           r.CreateDate = DateTime.Now;
                           r.IsDeleted = r.IsDeleted;
                           r.EditUser = "";
                           r.EditDate = null;
                       });

                        req.Departures.FindAll(f => lstDepDates.Exists(r => r.Departure_Id == f.Departure_Id)).ForEach
                           (r =>
                           {
                               r.EditDate = DateTime.Now;
                               r.CreateDate = (lstDepDates.Where(l => l.Departure_Id == r.Departure_Id).Select(l => l.CreateDate).FirstOrDefault());
                               r.CreateUser = (lstDepDates.Where(l => l.Departure_Id == r.Departure_Id).Select(l => l.CreateUser).FirstOrDefault());
                               r.IsDeleted = r.IsDeleted;
                           });

                        var res = await _MongoContext.mQuote.FindOneAndUpdateAsync(Builders<mQuote>.Filter.Eq("QRFID", req.QRFID),
                               Builders<mQuote>.Update.Set("Departures", req.Departures).Set("CurrentPipeline", "Quote Pipeline").
                               Set("CurrentPipelineStep", "Quote").Set("CurrentPipelineSubStep", "DateRange").
                               Set("ValidForTravel", ValidForTravelDate).Set("ValidForAcceptance", ValidForAcceptance));


                        if (lstDeptNew?.Count > 0 || lstdeptmodify?.Count > 0)
                        {
                            await UpsertDeletePriceFocOnDatePaxSlabChange(new DatePaxDetailsSetRequest
                            {
                                DepartureDates = lstdeptmodify,
                                DepartureDatesNew = lstDeptNew,
                                QRFID = req.QRFID,
                                UserEmail = req.UserEmail
                            });
                        }

                        response.Status = "Success";
                    }
                    else
                    {
                        response.Status = "No Departure found";
                    }
                }
                else//insert code at 1st time
                {
                    req.Departures.RemoveAll(f => f.IsDeleted == true);
                    if (req.Departures != null && req.Departures.Count > 0)
                    {
                        req.Departures.ForEach(r =>
                        {
                            r.Departure_Id = _genericRepository.GetNextReferenceNumber(qrfCounterRequest).LastReferenceNumber;
                            r.IsDeleted = false;
                            r.CreateDate = DateTime.Now;
                            r.EditUser = "";
                            r.EditDate = null;
                        });

                        var resultFlag = await _MongoContext.mQuote.UpdateOneAsync(Builders<mQuote>.Filter.Eq("QRFID", req.QRFID),
                              Builders<mQuote>.Update.PushEach<DepartureDates>("Departures", req.Departures).Set("CurrentPipeline", "Quote Pipeline").
                               Set("CurrentPipelineStep", "Quote").Set("CurrentPipelineSubStep", "DateRange").
                               Set("ValidForTravel", ValidForTravelDate).Set("ValidForAcceptance", ValidForAcceptance));
                        response.Status = "Success";
                    }
                    else
                    {
                        response.Status = "No Departure record to insert";
                    }
                }
            }
            else
            {
                response.Status = "QRF doesn't Exist";
            }

            return response;
        }

        public DepartureDateGetResponse GetDepartureDatesForQRF_Id(DepartureDateGetRequest req)
        {
            var response = new DepartureDateGetResponse();
            try
            {
                var filters = Builders<mQuote>.Filter.Where(x => x.QRFID == req.QRFID);
                if (_MongoContext.mQuote.Find(filters).Count() > 0)
                {
                    if (req.date == null)
                    {
                        var res = from m in _MongoContext.mQuote.AsQueryable()
                                  where m.QRFID == req.QRFID
                                  select new DepartureDateGetResponse { DepartureDates = m.Departures };

                        response.DepartureDates = res.First().DepartureDates.Where(x => x.IsDeleted == false).ToList();
                        if (response.DepartureDates.Count() > 0)
                        {
                            response.Status = "Success";
                        }
                        else
                        {
                            response.Status = "No Departures Found";
                        }
                    }
                    else
                    {
                        var WarnMessage = GetWarning(req.date);
                        var departure = new DepartureDates { Date = req.date, Warning = Convert.ToString(WarnMessage) };
                        response.DepartureDates.Add(departure);
                        response.Status = "Success";
                    }
                }
                else
                {
                    if (req.date != null)
                    {
                        var WarnMessage = GetWarning(req.date);
                        var departure = new DepartureDates { Date = req.date, Warning = Convert.ToString(WarnMessage) };
                        response.DepartureDates.Add(departure);
                        response.Status = "Success";
                    }
                    else
                    {
                        response.Status = "Invalid Qrf";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.Status = "Exception Occured";
            }
            return response;
        }

        public string GetWarning(DateTime? date)
        {
            StringBuilder warn = new StringBuilder();
            if (date != null)
            {
                var res = (from m in _MongoContext.mHotDate.AsQueryable()
                           where (date >= m.StartDate && date <= m.EndDate)
                           select m.Name).Distinct().ToList();

                res.ForEach(x => { warn = warn.Append(x).Append(","); });
                if (warn.Length > 0)
                {
                    warn.Length--;
                }
            }
            return Convert.ToString(warn);
        }

        #endregion

        #region PaxSlabDetails
        public PaxGetResponse GetPaxSlabDetailsForQRF_Id(PaxGetRequest req)
        {
            var response = new PaxGetResponse();
            try
            {
                var filters = Builders<mQuote>.Filter.Where(x => x.QRFID == req.QRFID);
                if (_MongoContext.mQuote.Find(filters).Count() > 0)
                {
                    var res = (from m in _MongoContext.mQuote.AsQueryable()
                               where m.QRFID == req.QRFID
                               select new PaxGetResponse { PaxSlabDetails = m.PaxSlabDetails }).FirstOrDefault().PaxSlabDetails;
                    if (res != null && res.PaxSlabs.Count() > 0)
                    {
                        response.PaxSlabDetails.PaxSlabs = res.PaxSlabs.Where(x => x.IsDeleted == false).ToList();
                        response.PaxSlabDetails.HotelCategories = res.HotelCategories;
                        response.PaxSlabDetails.HotelChain = res.HotelChain;
                        response.PaxSlabDetails.HotelFlag = res.HotelFlag;
                        response.QRFID = req.QRFID;
                        response.Status = "Success";
                    }
                    else
                    {
                        PaxSetRequest SetReq = new PaxSetRequest { PaxSlabDetails = GetDefaultPaxSlab() };

                        if (SetReq != null && SetReq.PaxSlabDetails != null && SetReq.PaxSlabDetails.PaxSlabs != null && SetReq.PaxSlabDetails.PaxSlabs.Count > 0)
                        {
                            var resultFlag = _MongoContext.mQuote.UpdateOneAsync(Builders<mQuote>.Filter.Eq("QRFID", req.QRFID),
                              Builders<mQuote>.Update.Set("PaxSlabDetails", SetReq.PaxSlabDetails));

                            response.PaxSlabDetails = SetReq.PaxSlabDetails;
                            response.QRFID = req.QRFID;
                            response.Status = "Success";
                        }
                        else
                        {
                            response.Status = "No Pax Slab To Insert";
                        }
                    }
                }
                else
                {

                    response.Status = "Invalid Qrf";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.Status = "Exception Occured";
            }
            return response;
        }

        public async Task<PaxSetResponse> SetPaxSlabDetailsForQRF_Id(PaxSetRequest req)
        {
            var response = new PaxSetResponse();
            QRFCounterRequest qrfCounterRequest = new QRFCounterRequest { CounterType = _configuration["CounterType:QRFPaxSlab"].ToString() };
            var result = _MongoContext.mQuote.AsQueryable().Where(a => a.QRFID == req.QRFID).FirstOrDefault();
            if (result != null)
            {
                PaxSlabDetails lstPaxSlab = result.PaxSlabDetails;
                List<PaxSlabs> lstPaxSlabs = lstPaxSlab.PaxSlabs;
                if (lstPaxSlab != null)//if exists or not then update as whole Routing Info List
                {
                    //fetch the PaxSlabs from req object which are deleted
                    var lstPaxSlabsmodify = req.PaxSlabDetails.PaxSlabs.Where(a => a.IsDeleted).ToList();

                    req.PaxSlabDetails.PaxSlabs.AddRange(lstPaxSlabs.Where(x => x.IsDeleted == true));
                    if (req.PaxSlabDetails != null && req.PaxSlabDetails.PaxSlabs.Count > 0)
                    {
                        //fetch the departures from req object which are new
                        var lstPaxSlabsNew = new List<PaxSlabs>();
                        lstPaxSlabsNew.AddRange(req.PaxSlabDetails.PaxSlabs.FindAll(f => !lstPaxSlabs.Exists(r => r.PaxSlab_Id == f.PaxSlab_Id)));

                        //fetch the departures from req object which are modified the value of existing PaxSlabIds
                        lstPaxSlabsmodify.AddRange(req.PaxSlabDetails.PaxSlabs.FindAll(f =>
                        lstPaxSlabs.Exists(r => r.PaxSlab_Id == f.PaxSlab_Id
                        && (r.From.ToString() + " - " + r.To.ToString()) != f.From.ToString() + " - " + f.To.ToString()
                        && f.IsDeleted == false)));

                        req.PaxSlabDetails.CreateDate = lstPaxSlab.CreateDate;
                        req.PaxSlabDetails.CreateUser = lstPaxSlab.CreateUser;
                        req.PaxSlabDetails.EditDate = DateTime.Now;
                        req.PaxSlabDetails.EditUser = req.PaxSlabDetails.EditUser;

                        req.PaxSlabDetails.PaxSlabs.FindAll(f => !lstPaxSlabs.Exists(r => r.PaxSlab_Id == f.PaxSlab_Id)).ForEach
                       (r =>
                       {
                           r.PaxSlab_Id = _genericRepository.GetNextReferenceNumber(qrfCounterRequest).LastReferenceNumber;
                           r.IsDeleted = r.IsDeleted;
                           r.DeleteUser = "";
                           r.DeleteDate = null;
                           r.CreateDate = DateTime.Now;
                           r.EditDate = null;
                       });

                        req.PaxSlabDetails.PaxSlabs.FindAll(f => lstPaxSlabs.Exists(r => r.PaxSlab_Id == f.PaxSlab_Id)).ForEach
                           (r =>
                           {
                               r.DeleteDate = r.IsDeleted == true ? DateTime.Now : (lstPaxSlabs.Where(l => l.PaxSlab_Id == r.PaxSlab_Id).Select(l => l.DeleteDate).FirstOrDefault());
                               r.DeleteUser = r.IsDeleted == true ? r.DeleteUser : (lstPaxSlabs.Where(l => l.PaxSlab_Id == r.PaxSlab_Id).Select(l => l.DeleteUser).FirstOrDefault());
                               r.IsDeleted = r.IsDeleted;
                               r.CreateDate = (lstPaxSlabs.Where(l => l.PaxSlab_Id == r.PaxSlab_Id).Select(l => l.CreateDate).FirstOrDefault());
                               r.EditDate = DateTime.Now;
                           });

                        var res = await _MongoContext.mQuote.FindOneAndUpdateAsync(Builders<mQuote>.Filter.Eq("QRFID", req.QRFID),
                               Builders<mQuote>.Update.Set("PaxSlabDetails", req.PaxSlabDetails).Set("CurrentPipeline", "Quote Pipeline").
                               Set("CurrentPipelineStep", "Quote").Set("CurrentPipelineSubStep", "PaxRange"));

                        if (lstPaxSlabsNew?.Count > 0 || lstPaxSlabsmodify?.Count > 0)
                        {
                            await UpsertDeletePriceFocOnDatePaxSlabChange(new DatePaxDetailsSetRequest
                            {
                                PaxSlabs = lstPaxSlabsmodify,
                                PaxSlabsNew = lstPaxSlabsNew,
                                QRFID = req.QRFID,
                                UserEmail = req.UserEmail
                            });
                        }

                        response.Status = "Success";
                    }
                    else
                    {
                        response.Status = "No PaxSlabDetails found";
                    }
                }
                else//insert code at 1st time
                {
                    req.PaxSlabDetails.PaxSlabs.RemoveAll(f => f.IsDeleted == true);
                    if (req.PaxSlabDetails != null && req.PaxSlabDetails.PaxSlabs.Count > 0)
                    {
                        req.PaxSlabDetails.PaxSlabs.ForEach(r =>
                        {
                            r.PaxSlab_Id = _genericRepository.GetNextReferenceNumber(qrfCounterRequest).LastReferenceNumber;
                            r.IsDeleted = false;
                            r.DeleteUser = "";
                            r.DeleteDate = null;
                            r.CreateDate = DateTime.Now;
                            r.EditDate = null;
                        });
                        req.PaxSlabDetails.CreateDate = DateTime.Now;
                        req.PaxSlabDetails.EditDate = null;
                        req.PaxSlabDetails.EditUser = "";
                        var resultFlag = await _MongoContext.mQuote.UpdateOneAsync(Builders<mQuote>.Filter.Eq("QRFID", req.QRFID),
                              Builders<mQuote>.Update.Set("PaxSlabDetails", req.PaxSlabDetails).Set("CurrentPipeline", "Quote Pipeline").
                               Set("CurrentPipelineStep", "Quote").Set("CurrentPipelineSubStep", "PaxRange"));
                        response.Status = "Success";
                    }
                    else
                    {
                        response.Status = "No PaxSlabDetails record to insert";
                    }
                }
            }
            else
            {
                response.Status = "QRF doesn't Exist";
            }

            return response;
        }

        private PaxSlabDetails GetDefaultPaxSlab()
        {
            PaxSlabDetails response = new PaxSlabDetails();
            QRFCounterRequest qrfCounterRequest = new QRFCounterRequest { CounterType = _configuration["CounterType:QRFPaxSlab"].ToString() };

            string defCatId = "786fab83-6a95-49c8-ab98-7aa795b3902d";
            string defCat = "Standard";
            string defCoach = "49-Seater with WC and intercom";

            response.CreateDate = DateTime.Now;
            response.EditDate = null;
            response.EditUser = "";
            response.HotelFlag = "no";

            for (int i = 10; i < 50; i = i + 5)
            {
                response.PaxSlabs.Add(new PaxSlabs
                {
                    PaxSlab_Id = _genericRepository.GetNextReferenceNumber(qrfCounterRequest).LastReferenceNumber,
                    From = i,
                    To = i + 4,
                    DivideByCost = i,
                    Category = defCat,
                    Category_Id = defCatId,
                    CoachType = defCoach,
                    CoachType_Id = defCoach,
                    Brand = "",
                    Brand_Id = "",
                    HowMany = 1,
                    BudgetAmount = 0,
                    IsDeleted = false,
                    DeleteUser = "",
                    DeleteDate = null,
                    CreateDate = DateTime.Now,
                    EditDate = null
                });
            }
            //response.PaxSlabs.AddRange(new List<PaxSlabs>
            //{
            //    new PaxSlabs
            //    {
            //        PaxSlab_Id = _genericRepository.GetNextReferenceNumber(qrfCounterRequest).LastReferenceNumber,
            //        From = 10,
            //        To = 19,
            //        DivideByCost = 10,
            //        Category = defCat,
            //        Category_Id = defCatId,
            //        CoachType = defCoach,
            //        CoachType_Id = defCoach,
            //        Brand = "",
            //        Brand_Id = "",
            //        HowMany = 1,
            //        BudgetAmount = 0,
            //        IsDeleted = false,
            //        DeleteUser = "",
            //        DeleteDate = null,
            //        CreateDate = DateTime.Now,
            //        EditDate = null
            //    },
            //    new PaxSlabs
            //    {
            //        PaxSlab_Id = _genericRepository.GetNextReferenceNumber(qrfCounterRequest).LastReferenceNumber,
            //        From = 20,
            //        To = 29,
            //        DivideByCost = 20,
            //        Category = defCat,
            //        Category_Id = defCatId,
            //        CoachType = defCoach,
            //        CoachType_Id = defCoach,
            //        Brand = "",
            //        Brand_Id = "",
            //        HowMany = 1,
            //        BudgetAmount = 0,
            //        IsDeleted = false,
            //        DeleteUser = "",
            //        DeleteDate = null,
            //        CreateDate = DateTime.Now,
            //        EditDate = null
            //    },
            //    new PaxSlabs
            //    {
            //        PaxSlab_Id = _genericRepository.GetNextReferenceNumber(qrfCounterRequest).LastReferenceNumber,
            //        From = 30,
            //        To = 39,
            //        DivideByCost = 30,
            //        Category = defCat,
            //        Category_Id = defCatId,
            //        CoachType = defCoach,
            //        CoachType_Id = defCoach,
            //        Brand = "",
            //        Brand_Id = "",
            //        HowMany = 1,
            //        BudgetAmount = 0,
            //        IsDeleted = false,
            //        DeleteUser = "",
            //        DeleteDate = null,
            //        CreateDate = DateTime.Now,
            //        EditDate = null
            //    },
            //    new PaxSlabs
            //    {
            //        PaxSlab_Id = _genericRepository.GetNextReferenceNumber(qrfCounterRequest).LastReferenceNumber,
            //        From = 40,
            //        To = 49,
            //        DivideByCost = 40,
            //        Category = defCat,
            //        Category_Id = defCatId,
            //        CoachType = defCoach,
            //        CoachType_Id = defCoach,
            //        Brand = "",
            //        Brand_Id = "",
            //        HowMany = 1,
            //        BudgetAmount = 0,
            //        IsDeleted = false,
            //        DeleteUser = "",
            //        DeleteDate = null,
            //        CreateDate = DateTime.Now,
            //        EditDate = null
            //    },
            //});

            return response;
        }
        #endregion

        #region Routing Info
        public async Task<List<RoutingInfo>> GetQRFRouteDetailsByQRFID(RoutingGetReq request)
        {
            var builder = Builders<mQuote>.Filter;
            var filter = builder.Where(q => q.QRFID == request.QRFID);
            var result = await _MongoContext.mQuote.Find(filter).Project(r => r.RoutingInfo).FirstOrDefaultAsync();
            if (result == null)
            {
                var builder1 = Builders<mQRFPrice>.Filter;
                var filter1 = builder1.Where(q => q.QRFID == request.QRFID);
                var result1 = await _MongoContext.mQRFPrice.Find(filter1).Project(r => r.RoutingInfo).FirstOrDefaultAsync();

                if (result1 == null)
                {
                    return (new List<RoutingInfo>());
                }
                else
                {
                    return result1.Where(f => !f.IsDeleted && f.RouteSequence > 0).ToList().OrderBy(r => r.RouteSequence).ToList();
                }
            }
            else
            {
                return result.Where(f => !f.IsDeleted && f.RouteSequence > 0).ToList().OrderBy(r => r.RouteSequence).ToList();
            }
        }

        public async Task<string> InsertUpdateQRFRouteDetails(RoutingSetReq request)
        {
            QRFCounterRequest qrfCounterRequest = new QRFCounterRequest { CounterType = _configuration["CounterType:QRFRoute"].ToString() };

            UpdateResult resultFlag;
            var result = _MongoContext.mQuote.AsQueryable().Where(a => a.QRFID == request.QRFID).ToList();
            if (result != null && result.Count > 0)
            {
                List<RoutingInfo> lstRoutingInfo = result.Select(r => r.RoutingInfo).FirstOrDefault();
                if (lstRoutingInfo != null && lstRoutingInfo.Count > 0)//if exists or not then update as whole Routing Info List
                {
                    request.RoutingInfo.RemoveAll(f => f.RouteSequence == 0 && f.RouteID == 0);

                    if (request.RoutingInfo != null && request.RoutingInfo.Count > 0)
                    {
                        var lstCityIds = request.RoutingInfo.Select(a => a.FromCityID).ToList();
                        lstCityIds.AddRange(request.RoutingInfo.Select(a => a.ToCityID).ToList());
                        var lstCityInfo = _masterRepository.GetCityNamesByID(lstCityIds);

                        request.RoutingInfo.AddRange(lstRoutingInfo.Where(f => f.IsDeleted == true).ToList().Distinct());
                        DateTime dt = DateTime.Now;

                        request.RoutingInfo.FindAll(f => !lstRoutingInfo.Exists(r => r.RouteID == f.RouteID)).ForEach
                       (r =>
                       {
                           r.RouteID = _genericRepository.GetNextReferenceNumber(qrfCounterRequest).LastReferenceNumber;
                           r.CreateDate = dt;
                           r.IsDeleted = (r.RouteSequence == 0 ? true : false);
                           r.FromCityName = lstCityInfo.Where(a => a.Voyager_Resort_Id == r.FromCityID).FirstOrDefault().Lookup;
                           r.ToCityName = lstCityInfo.Where(a => a.Voyager_Resort_Id == r.ToCityID).FirstOrDefault().Lookup;
                           r.EditUser = "";
                           r.EditDate = null;
                       });

                        request.RoutingInfo.FindAll(f => lstRoutingInfo.Exists(r => r.RouteID == f.RouteID)).ForEach
                           (r =>
                           {
                               var lstRoute = lstRoutingInfo.Where(l => l.RouteID == r.RouteID).FirstOrDefault();
                               r.EditDate = dt;
                               r.CreateDate = lstRoute.CreateDate;
                               r.CreateUser = lstRoute.CreateUser;
                               r.IsDeleted = (r.RouteSequence == 0 ? true : false);
                               r.FromCityName = r.RouteSequence == 0 ? lstRoute.FromCityName : lstCityInfo.Where(a => a.Voyager_Resort_Id == r.FromCityID).FirstOrDefault().Lookup;
                               r.ToCityName = r.RouteSequence == 0 ? lstRoute.ToCityName : lstCityInfo.Where(a => a.Voyager_Resort_Id == r.ToCityID).FirstOrDefault().Lookup;
                               r.ToCityID = r.RouteSequence == 0 ? lstRoute.ToCityID : r.ToCityID;
                               r.FromCityID = r.RouteSequence == 0 ? lstRoute.FromCityID : r.FromCityID;
                               r.Days = r.RouteSequence == 0 ? lstRoute.Days : r.Days;
                               r.Nights = r.RouteSequence == 0 ? lstRoute.Nights : r.Nights;
                               r.IsLocalGuide = r.RouteSequence == 0 ? lstRoute.IsLocalGuide : r.IsLocalGuide;
                               r.PrefStarRating = r.RouteSequence == 0 ? lstRoute.PrefStarRating : r.PrefStarRating;
                           });
                        request.RoutingInfo = request.RoutingInfo.Distinct().ToList();

                        int ttlNights = request.RoutingInfo.Sum(a => a.Nights);
                        var res = await _MongoContext.mQuote.FindOneAndUpdateAsync(Builders<mQuote>.Filter.Eq("QRFID", request.QRFID),
                               Builders<mQuote>.Update.Set("RoutingInfo", request.RoutingInfo).Set("CurrentPipeline", "Quote Pipeline").
                                Set("AgentProductInfo.Duration", ttlNights).
                                Set("CurrentPipelineStep", "Quote").Set("CurrentPipelineSubStep", "Routing").Set("RegenerateItinerary", true));
                        if (res != null)
                        {
                            RoutingDaysSetReq req = new RoutingDaysSetReq()
                            {
                                QRFID = request.QRFID,
                                UserName = request.RoutingInfo.FirstOrDefault().CreateUser,
                                RoutingInfo = request.RoutingInfo.Where(a => a.IsDeleted == false).ToList(),
                                ExistingRoutingInfo = lstRoutingInfo
                            };
                            RoutingDaysGetRes objRoutingDaysGetRes = await InsertUpdateQRFRoutingDays(req);
                            if (objRoutingDaysGetRes.ResponseStatus.Status.ToLower() == "success")
                            {
                                return "1";
                            }
                            else
                            {
                                return objRoutingDaysGetRes.ResponseStatus.ErrorMessage;
                            }
                        }
                        else
                        {
                            return "Route Details not updated.";
                        }
                    }
                    else
                    {
                        return "1";
                    }
                }
                else//insert Route details at 1st time
                {
                    request.RoutingInfo.RemoveAll(f => f.RouteSequence == 0 && f.RouteID == 0);

                    if (request.RoutingInfo != null && request.RoutingInfo.Count > 0)
                    {
                        DateTime dt = DateTime.Now;
                        var lstCityIds = request.RoutingInfo.Select(a => a.FromCityID).ToList();
                        lstCityIds.AddRange(request.RoutingInfo.Select(a => a.ToCityID).ToList());
                        var lstCityInfo = _masterRepository.GetCityNamesByID(lstCityIds);

                        request.RoutingInfo.ForEach(r =>
                        {
                            r.RouteID = (_genericRepository.GetNextReferenceNumber(qrfCounterRequest).LastReferenceNumber);
                            r.CreateDate = dt;
                            r.FromCityName = lstCityInfo.Where(a => a.Voyager_Resort_Id == r.FromCityID).FirstOrDefault().Lookup;
                            r.ToCityName = lstCityInfo.Where(a => a.Voyager_Resort_Id == r.ToCityID).FirstOrDefault().Lookup;
                            r.EditUser = "";
                            r.EditDate = null;
                        });

                        int ttlNights = request.RoutingInfo.Sum(a => a.Nights);
                        resultFlag = await _MongoContext.mQuote.UpdateOneAsync(Builders<mQuote>.Filter.Eq("QRFID", request.QRFID),
                             Builders<mQuote>.Update.PushEach<RoutingInfo>("RoutingInfo", request.RoutingInfo).
                             Set("AgentProductInfo.Duration", ttlNights).
                             Set("CurrentPipeline", "Quote Pipeline").
                             Set("CurrentPipelineStep", "Quote").Set("CurrentPipelineSubStep", "Routing").Set("RegenerateItinerary", true));

                        if (resultFlag.ModifiedCount > 0)
                        {
                            RoutingDaysSetReq req = new RoutingDaysSetReq()
                            {
                                QRFID = request.QRFID,
                                UserName = request.RoutingInfo.FirstOrDefault().CreateUser,
                                RoutingInfo = request.RoutingInfo
                            };
                            RoutingDaysGetRes objRoutingDaysGetRes = await InsertUpdateQRFRoutingDays(req);
                            if (objRoutingDaysGetRes.ResponseStatus.Status.ToLower() == "success")
                            {
                                return "1";
                            }
                            else
                            {
                                return objRoutingDaysGetRes.ResponseStatus.ErrorMessage;
                            }
                        }
                        else
                        {
                            return "Route Details not inserted.";
                        }
                    }
                    else
                    {
                        return "1";
                    }
                }
            }
            else
            {
                return "QRF ID not exist.";
            }
        }
        #endregion

        #region RoutingDays  
        //public async Task<RoutingDaysGetRes> InsertUpdateQRFRoutingDays(RoutingDaysSetReq request)
        //{
        //    RoutingDaysGetRes response = new RoutingDaysGetRes();
        //    response.QRFID = request.QRFID;
        //    List<RoutingDays> lstRoutingDays = new List<RoutingDays>();
        //    List<RoutingDays> lstRoutingDaysDel = new List<RoutingDays>();
        //    UpdateResult resultFlag;
        //    string GridLabel = "";
        //    try
        //    {
        //        var builder = Builders<mQuote>.Filter;
        //        var filter = builder.Where(q => q.QRFID == request.QRFID);
        //        var result = await _MongoContext.mQuote.Find(filter).Project(r => new RoutingDaysInfo { RoutingInfo = r.RoutingInfo, RoutingDays = r.RoutingDays }).FirstOrDefaultAsync();
        //        if (result == null)
        //        {
        //            response.RoutingDays = lstRoutingDays;
        //            response.ResponseStatus.Status = "Failure";
        //            response.ResponseStatus.ErrorMessage = "QRF ID not exists.";
        //            return response;
        //        }
        //        else
        //        {
        //            result.RoutingInfo = request.RoutingInfo != null && request.RoutingInfo.Count > 0 ? request.RoutingInfo : result.RoutingInfo;
        //            result.RoutingInfo = result.RoutingInfo.Where(a => a.IsDeleted == false).ToList();

        //            if (result.RoutingInfo != null && result.RoutingInfo.Count > 0)
        //            {
        //                var rouitnginfo = result.RoutingInfo.OrderBy(a => a.RouteSequence).ToList();
        //                int day = 0;
        //                var lastroute = rouitnginfo.LastOrDefault();

        //                for (int d = 0; d < rouitnginfo.Count; d++)
        //                {
        //                    GridLabel = "";
        //                    var thisItem = rouitnginfo[d];
        //                    day = thisItem.Nights;

        //                    if (day == 0)
        //                    {
        //                        if (thisItem.FromCityName != thisItem.ToCityName)
        //                            GridLabel = CommonFunction.SplitString(thisItem.FromCityName, ',')[0] + ", " + CommonFunction.SplitString(thisItem.ToCityName, ',')[0];
        //                        else
        //                            GridLabel = CommonFunction.SplitString(thisItem.ToCityName, ',')[0];
        //                        if ((d + 1) != (rouitnginfo.Count))
        //                        {
        //                            for (int cn = (d + 1); cn < rouitnginfo.Count; cn++)
        //                            {
        //                                var nextItem = rouitnginfo[cn];
        //                                if (nextItem.Nights != 0)
        //                                {
        //                                    GridLabel = GridLabel + ", " + CommonFunction.SplitString(nextItem.ToCityName, ',')[0];
        //                                    if (nextItem.Nights == 1)
        //                                    {
        //                                        //d = d + (cn - 1);
        //                                        d = cn;
        //                                        lstRoutingDays.Add(AddRoutingDaysInfo(thisItem, "i", request.UserName, (lstRoutingDays.Count + 1), "", GridLabel));
        //                                        break;
        //                                    }
        //                                    else
        //                                    {
        //                                        for (int n = 1; n <= nextItem.Nights; n++)
        //                                        {
        //                                            if (n != 1)
        //                                            {
        //                                                GridLabel = CommonFunction.SplitString(nextItem.ToCityName, ',')[0];
        //                                            }
        //                                            lstRoutingDays.Add(AddRoutingDaysInfo(nextItem, "i", request.UserName, (lstRoutingDays.Count + 1), "", GridLabel));
        //                                        }
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    GridLabel = GridLabel + ", " + CommonFunction.SplitString(nextItem.ToCityName, ',')[0];
        //                                }
        //                            }
        //                        }
        //                        // lstRoutingDays.Add(AddRoutingDaysInfo(thisItem, "i", request.UserName, (lstRoutingDays.Count + 1), "", GridLabel));
        //                    }
        //                    else
        //                    {
        //                        if (day == 1)
        //                        {
        //                            if (thisItem.FromCityName != thisItem.ToCityName)
        //                                GridLabel = CommonFunction.SplitString(thisItem.FromCityName, ',')[0] + ", " + CommonFunction.SplitString(thisItem.ToCityName, ',')[0];
        //                            else
        //                                GridLabel = CommonFunction.SplitString(thisItem.ToCityName, ',')[0];
        //                            lstRoutingDays.Add(AddRoutingDaysInfo(thisItem, "i", request.UserName, (lstRoutingDays.Count + 1), "", GridLabel));
        //                        }
        //                        else
        //                        {
        //                            for (int n = 1; n <= day; n++)
        //                            {
        //                                if (n != 1)
        //                                {
        //                                    thisItem.FromCityID = thisItem.ToCityID;
        //                                    thisItem.FromCityName = thisItem.ToCityName;
        //                                }
        //                                if (thisItem.FromCityName != thisItem.ToCityName)
        //                                    GridLabel = CommonFunction.SplitString(thisItem.FromCityName, ',')[0] + ", " + CommonFunction.SplitString(thisItem.ToCityName, ',')[0];
        //                                else
        //                                    GridLabel = CommonFunction.SplitString(thisItem.ToCityName, ',')[0];
        //                                lstRoutingDays.Add(AddRoutingDaysInfo(thisItem, "i", request.UserName, (lstRoutingDays.Count + 1), "", GridLabel));
        //                            }
        //                        }
        //                    }

        //                    /*if (day == 1)
        //                        lstRoutingDays.Add(AddRoutingDaysInfo(thisItem, "i", request.UserName, (lstRoutingDays.Count + 1), ""));
        //                    else
        //                    {
        //                        for (int n = 1; n <= day; n++)
        //                        {
        //                            if (n == 1)
        //                                lstRoutingDays.Add(AddRoutingDaysInfo(thisItem, "i", request.UserName, (lstRoutingDays.Count + 1), ""));
        //                            else
        //                            {
        //                                thisItem.FromCityID = thisItem.ToCityID;
        //                                thisItem.FromCityName = thisItem.ToCityName;
        //                                lstRoutingDays.Add(AddRoutingDaysInfo(thisItem, "i", request.UserName, (lstRoutingDays.Count + 1), ""));
        //                            }
        //                        }
        //                    }*/
        //                }
        //                lastroute.FromCityID = lastroute.ToCityID;
        //                lastroute.FromCityName = lastroute.ToCityName;
        //                lastroute.ToCityID = "";
        //                lastroute.ToCityName = "";
        //                GridLabel = CommonFunction.SplitString(lastroute.FromCityName, ',')[0];
        //                lstRoutingDays.Add(AddRoutingDaysInfo(lastroute, "i", request.UserName, (lstRoutingDays.Count + 1), "", GridLabel));

        //                if (result.RoutingDays != null && result.RoutingDays.Count > 0)
        //                {
        //                    lstRoutingDaysDel.AddRange(result.RoutingDays.Where(a => a.IsDeleted == true).ToList());
        //                    result.RoutingDays = result.RoutingDays.Where(a => a.IsDeleted == false).ToList();
        //                    var routedaysid = new RoutingDays();
        //                    lstRoutingDays.ForEach(a =>
        //                    {
        //                        routedaysid = result.RoutingDays.Where(b => b.DayNo == a.DayNo && b.IsDeleted == false).FirstOrDefault();
        //                        a.RoutingDaysID = routedaysid != null ? routedaysid.RoutingDaysID : a.RoutingDaysID;
        //                    });

        //                    //if in Mongodb Quote collection Routing days are gerater than the Ruoting days enterd in UI level then mark as delete
        //                    if (result.RoutingDays.Count > lstRoutingDays.Count)
        //                    {
        //                        var cnt = result.RoutingDays.Count - lstRoutingDays.Count;
        //                        result.RoutingDays.TakeLast(cnt).ToList().ForEach(a => a.IsDeleted = true);
        //                        lstRoutingDaysDel.AddRange(result.RoutingDays.Where(a => a.IsDeleted == true).ToList());
        //                    }

        //                    response.RoutingDays = lstRoutingDays.Where(a => a.IsDeleted == false).OrderBy(a => a.RouteSequence).ToList();
        //                    lstRoutingDays.AddRange(lstRoutingDaysDel);
        //                    lstRoutingDays = lstRoutingDays.Distinct().OrderBy(a => a.IsDeleted).ThenBy(a => a.RouteSequence).ToList();

        //                    var res = await _MongoContext.mQuote.FindOneAndUpdateAsync(Builders<mQuote>.Filter.Eq("QRFID", request.QRFID),
        //                       Builders<mQuote>.Update.Set("RoutingDays", lstRoutingDays).
        //                       Set("CurrentPipeline", "Quote Pipeline").
        //                       Set("CurrentPipelineStep", "Quote").Set("CurrentPipelineSubStep", "Routing")
        //                       );
        //                    response.ResponseStatus.Status = res != null ? "Success" : "Failure";
        //                    response.ResponseStatus.ErrorMessage = res != null ? "" : "RoutingDays not updated";
        //                }
        //                else
        //                {
        //                    /*foreach (var item in rouitnginfo)
        //                    {
        //                        day = item.Nights;
        //                        for (int i = 1; i <= day; i++)
        //                        {
        //                            lstRoutingDays.Add(AddRoutingDaysInfo(item, "i", request.UserName, (lstRoutingDays.Count + 1), ""));
        //                        }
        //                    }*/

        //                    response.RoutingDays = lstRoutingDays.OrderBy(a => a.RouteSequence).ToList();

        //                    resultFlag = await _MongoContext.mQuote.UpdateOneAsync(Builders<mQuote>.Filter.Eq("QRFID", request.QRFID),
        //                     Builders<mQuote>.Update.PushEach<RoutingDays>("RoutingDays", response.RoutingDays).
        //                     Set("CurrentPipeline", "Quote Pipeline").
        //                     Set("CurrentPipelineStep", "Quote").Set("CurrentPipelineSubStep", "Routing"));

        //                    response.ResponseStatus.Status = resultFlag.ModifiedCount > 0 ? "Success" : "Failure";
        //                    response.ResponseStatus.ErrorMessage = resultFlag.ModifiedCount > 0 ? "" : "RoutingDays not updated";
        //                }
        //            }
        //            else //if routing info is not exists in DB then existing Routing days marked as IsDeleted to true in DB
        //            {
        //                if (result.RoutingDays != null && result.RoutingDays.Count > 0)
        //                {
        //                    result.RoutingDays.ForEach(r =>
        //                    {
        //                        r.EditUser = request.UserName;
        //                        r.EditDate = DateTime.Now;
        //                        r.IsDeleted = true;
        //                    });
        //                    var res = await _MongoContext.mQuote.FindOneAndUpdateAsync(Builders<mQuote>.Filter.Eq("QRFID", request.QRFID),
        //                           Builders<mQuote>.Update.Set("RoutingDays", result.RoutingDays).
        //                          Set("CurrentPipeline", "Quote Pipeline").
        //                          Set("CurrentPipelineStep", "Quote").Set("CurrentPipelineSubStep", "Routing"));

        //                    response.ResponseStatus.Status = res != null ? "Success" : "Failure";
        //                    response.ResponseStatus.ErrorMessage = res != null ? "" : "RoutingDays not updated";
        //                }
        //                else
        //                {
        //                    response.ResponseStatus.Status = "Failure";
        //                    response.ResponseStatus.ErrorMessage = "Rounting Details Not Found.";
        //                }
        //                response.RoutingDays = lstRoutingDays;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        response.RoutingDays = lstRoutingDays;
        //        response.ResponseStatus.Status = "Failure";
        //        response.ResponseStatus.ErrorMessage = ex.Message;
        //    }
        //  //  await UpdatePositions(request.UserName, request.QRFID);
        //    return response;
        //}

        public async Task<RoutingDaysGetRes> InsertUpdateQRFRoutingDays(RoutingDaysSetReq request)
        {
            RoutingDaysGetRes response = new RoutingDaysGetRes();
            response.QRFID = request.QRFID;
            List<RoutingDays> lstRoutingDays = new List<RoutingDays>();
            List<RoutingDays> lstRoutingDaysDel = new List<RoutingDays>();
            UpdateResult resultFlag;
            string GridLabel = "";
            string GridLabelIds = "";
            try
            {
                var builder = Builders<mQuote>.Filter;
                var filter = builder.Where(q => q.QRFID == request.QRFID);
                var result = await _MongoContext.mQuote.Find(filter).Project(r => new RoutingDaysInfo { RoutingInfo = r.RoutingInfo, RoutingDays = r.RoutingDays }).FirstOrDefaultAsync();
                if (result == null)
                {
                    response.RoutingDays = lstRoutingDays;
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "QRF ID not exists.";
                    return response;
                }
                else
                {
                    result.RoutingInfo = request.RoutingInfo != null && request.RoutingInfo.Count > 0 ? request.RoutingInfo : result.RoutingInfo;
                    result.RoutingInfo = result.RoutingInfo.Where(a => a.IsDeleted == false).ToList();

                    if (result.RoutingInfo != null && result.RoutingInfo.Count > 0)
                    {
                        int day = 0;
                        var rouitnginfo = result.RoutingInfo.OrderBy(a => a.RouteSequence).ToList();
                        var lastroute = rouitnginfo.LastOrDefault();
                        var lstRouteDays = new List<RoutingDays>();
                        var lstRoutingInfo = new List<RoutingInfo>();

                        if (result.RoutingDays != null && result.RoutingDays.Count > 0)
                        {
                            //if ruotines are deleted from UI then mark as delete in RoutingDays
                            result.RoutingDays.FindAll(r => !rouitnginfo.Exists(ri => ri.RouteID == r.RouteID)).ForEach(r =>
                            {
                                r.IsDeleted = true;
                                r.EditDate = DateTime.Now; r.EditUser = request.UserName;
                            });
                            lstRoutingDaysDel.AddRange(result.RoutingDays.Where(a => a.IsDeleted == true));
                            result.RoutingDays = result.RoutingDays.Where(a => a.IsDeleted == false).ToList();

                            for (int d = 0; d < rouitnginfo.Count; d++)
                            {
                                GridLabel = "";
                                GridLabelIds = "";
                                var thisItem = rouitnginfo[d];
                                day = thisItem.Nights;

                                lstRoutingInfo = request.ExistingRoutingInfo.Where(a => a.RouteID == rouitnginfo[d].RouteID && a.FromCityID == rouitnginfo[d].FromCityID && a.ToCityID == rouitnginfo[d].ToCityID).ToList();

                                if (lstRoutingInfo != null && lstRoutingInfo.Count > 0)
                                {
                                    UpdateRoutingDays(day, thisItem, rouitnginfo, lstRoutingDays, request.UserName, lstRoutingInfo, result, ref lstRoutingDaysDel, ref d);
                                }
                                else
                                {
                                    result.RoutingDays.FindAll(rd => rd.RouteID == rouitnginfo[d].RouteID).ForEach(r =>
                                    {
                                        r.EditUser = request.UserName;
                                        r.EditDate = DateTime.Now;
                                        r.IsDeleted = true;
                                    });
                                    lstRoutingDaysDel.AddRange(result.RoutingDays.Where(a => a.IsDeleted == true));
                                    result.RoutingDays = result.RoutingDays.Where(a => a.IsDeleted == false).ToList();

                                    UpdateRoutingDays(day, thisItem, rouitnginfo, lstRoutingDays, request.UserName, lstRoutingInfo, result, ref lstRoutingDaysDel, ref d);
                                }
                            }
                            if (lastroute.Nights != 0)
                            {
                                lastroute.FromCityID = lastroute.ToCityID;
                                lastroute.FromCityName = lastroute.ToCityName;
                                lastroute.ToCityID = "";
                                lastroute.ToCityName = "";
                                GridLabel = CommonFunction.SplitString(lastroute.FromCityName, ',')[0];
                                GridLabelIds = lastroute.FromCityID;
                                lstRouteDays = result.RoutingDays.Where(rd => rd.RouteID == lastroute.RouteID).ToList();
                                if (lstRouteDays != null && lstRouteDays.Count > 1)
                                {
                                    lstRoutingDays.Add(AddRoutingDaysInfo(lastroute, "", request.UserName, (lstRoutingDays.Count + 1), lstRouteDays.LastOrDefault().RoutingDaysID, GridLabel, GridLabelIds));
                                }
                                else
                                {
                                    lstRoutingDays.Add(AddRoutingDaysInfo(lastroute, "i", request.UserName, (lstRoutingDays.Count + 1), "", GridLabel, GridLabelIds));
                                }
                            }

                            lstRoutingDaysDel.AddRange(result.RoutingDays.Where(a => a.IsDeleted == true).ToList());
                            response.RoutingDays = lstRoutingDays.Where(a => a.IsDeleted == false).OrderBy(a => a.RouteSequence).ToList();

                            lstRoutingDays.AddRange(lstRoutingDaysDel);
                            lstRoutingDays = lstRoutingDays.Distinct().OrderBy(a => a.IsDeleted).ThenBy(a => a.RouteSequence).ToList();

                            var res = await _MongoContext.mQuote.FindOneAndUpdateAsync(Builders<mQuote>.Filter.Eq("QRFID", request.QRFID),
                               Builders<mQuote>.Update.Set("RoutingDays", lstRoutingDays).
                               Set("CurrentPipeline", "Quote Pipeline").
                               Set("CurrentPipelineStep", "Quote").Set("CurrentPipelineSubStep", "Routing")
                               );
                            response.ResponseStatus.Status = res != null ? "Success" : "Failure";
                            response.ResponseStatus.ErrorMessage = res != null ? "" : "RoutingDays not updated";
                        }
                        else
                        {
                            for (int d = 0; d < rouitnginfo.Count; d++)
                            {
                                GridLabel = "";
                                GridLabelIds = "";
                                var thisItem = rouitnginfo[d];
                                day = thisItem.Nights;

                                AddRoutingDays(lstRouteDays, day, thisItem, request.UserName, rouitnginfo, ref d);
                            }
                            if (lastroute.Nights != 0)
                            {
                                lastroute.FromCityID = lastroute.ToCityID;
                                lastroute.FromCityName = lastroute.ToCityName;
                                lastroute.ToCityID = "";
                                lastroute.ToCityName = "";
                                GridLabel = CommonFunction.SplitString(lastroute.FromCityName, ',')[0];
                                GridLabelIds = lastroute.FromCityID;
                                lstRouteDays.Add(AddRoutingDaysInfo(lastroute, "i", request.UserName, (lstRouteDays.Count + 1), "", GridLabel, GridLabelIds));
                            }
                            response.RoutingDays = lstRouteDays.OrderBy(a => a.RouteSequence).ToList();

                            resultFlag = await _MongoContext.mQuote.UpdateOneAsync(Builders<mQuote>.Filter.Eq("QRFID", request.QRFID),
                             Builders<mQuote>.Update.PushEach<RoutingDays>("RoutingDays", response.RoutingDays).
                             Set("CurrentPipeline", "Quote Pipeline").
                             Set("CurrentPipelineStep", "Quote").Set("CurrentPipelineSubStep", "Routing"));

                            response.ResponseStatus.Status = resultFlag.ModifiedCount > 0 ? "Success" : "Failure";
                            response.ResponseStatus.ErrorMessage = resultFlag.ModifiedCount > 0 ? "" : "RoutingDays not updated";
                        }
                    }
                    else //if routing info is not exists in DB then existing Routing days marked as IsDeleted to true in DB
                    {
                        if (result.RoutingDays != null && result.RoutingDays.Count > 0)
                        {
                            result.RoutingDays.ForEach(r =>
                            {
                                r.EditUser = request.UserName;
                                r.EditDate = DateTime.Now;
                                r.IsDeleted = true;
                            });
                            var res = await _MongoContext.mQuote.FindOneAndUpdateAsync(Builders<mQuote>.Filter.Eq("QRFID", request.QRFID),
                                   Builders<mQuote>.Update.Set("RoutingDays", result.RoutingDays).
                                  Set("CurrentPipeline", "Quote Pipeline").
                                  Set("CurrentPipelineStep", "Quote").Set("CurrentPipelineSubStep", "Routing"));

                            response.ResponseStatus.Status = res != null ? "Success" : "Failure";
                            response.ResponseStatus.ErrorMessage = res != null ? "" : "RoutingDays not updated";
                        }
                        else
                        {
                            response.ResponseStatus.Status = "Failure";
                            response.ResponseStatus.ErrorMessage = "Rounting Details Not Found.";
                        }
                        response.RoutingDays = lstRoutingDays;
                    }
                }
            }
            catch (Exception ex)
            {
                response.RoutingDays = lstRoutingDays;
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }
            await UpdatePositions(request.UserName, request.QRFID);
            return response;
        }

        public List<RoutingDays> UpdateRoutingDays(int day, RoutingInfo thisItem, List<RoutingInfo> rouitnginfo, List<RoutingDays> lstRoutingDays, string UserName, List<RoutingInfo> lstRoutingInfo, RoutingDaysInfo result, ref List<RoutingDays> lstRoutingDaysDel, ref int d)
        {
            int rountecnt = 0;
            int k = 0;
            string GridLabel = "";
            string GridLabelIds = "";
            List<RoutingDays> lstRouteDays = new List<RoutingDays>();

            if (day == 0)
            {
                if (thisItem.FromCityName != thisItem.ToCityName)
                {
                    GridLabel = CommonFunction.SplitString(thisItem.FromCityName, ',')[0] + ", " + CommonFunction.SplitString(thisItem.ToCityName, ',')[0];
                    GridLabelIds = thisItem.FromCityID + ", " + thisItem.ToCityID;
                }
                else
                {
                    GridLabel = CommonFunction.SplitString(thisItem.ToCityName, ',')[0];
                    GridLabelIds = thisItem.ToCityID;

                }
                if ((d + 1) != (rouitnginfo.Count))
                {
                    for (int cn = (d + 1); cn < rouitnginfo.Count; cn++)
                    {
                        var nextItem = rouitnginfo[cn];
                        lstRouteDays = result.RoutingDays.Where(a => a.RouteID == nextItem.RouteID && a.IsDeleted == false).ToList();

                        if (nextItem.Nights != 0)
                        {
                            GridLabel = GridLabel + ", " + CommonFunction.SplitString(nextItem.ToCityName, ',')[0];
                            GridLabelIds = GridLabelIds + ", " + nextItem.ToCityID;
                            if (nextItem.Nights == 1)
                            {
                                //d = d + (cn - 1);
                                d = cn;
                                if (lstRouteDays != null && lstRouteDays.Count > 0)
                                {
                                    lstRoutingDays.Add(AddRoutingDaysInfo(nextItem, "", UserName, (lstRoutingDays.Count + 1), lstRouteDays.FirstOrDefault().RoutingDaysID, GridLabel, GridLabelIds));
                                }
                                else
                                {
                                    lstRoutingDays.Add(AddRoutingDaysInfo(nextItem, "i", UserName, (lstRoutingDays.Count + 1), "", GridLabel, GridLabelIds));
                                }
                                break;
                            }
                            else
                            {
                                d = cn;
                                for (int n = 1; n <= nextItem.Nights; n++)
                                {
                                    if (n != 1)
                                    {
                                        GridLabel = CommonFunction.SplitString(nextItem.ToCityName, ',')[0];
                                        GridLabelIds = nextItem.ToCityID;
                                    }
                                    k = n - 1;
                                    if (lstRouteDays.Count > n)
                                    {
                                        lstRoutingDays.Add(AddRoutingDaysInfo(nextItem, "", UserName, (lstRoutingDays.Count + 1), lstRouteDays[k].RoutingDaysID, GridLabel, GridLabelIds));
                                    }
                                    else
                                    {
                                        lstRoutingDays.Add(AddRoutingDaysInfo(nextItem, "i", UserName, (lstRoutingDays.Count + 1), "", GridLabel, GridLabelIds));
                                    }
                                }
                            }
                        }
                        else
                        {
                            d = cn;
                            GridLabel = GridLabel + ", " + CommonFunction.SplitString(nextItem.ToCityName, ',')[0];
                            GridLabelIds = GridLabelIds + ", " + nextItem.ToCityID;

                            if (cn + 1 == rouitnginfo.Count)
                            {
                                if (lstRouteDays==null)
                                {
                                    lstRoutingDays.Add(AddRoutingDaysInfo(nextItem, "i", UserName, (lstRoutingDays.Count + 1), "", GridLabel, GridLabelIds));
                                }
                                else
                                {
                                    lstRoutingDays.Add(AddRoutingDaysInfo(nextItem, "", UserName, (lstRoutingDays.Count + 1), lstRouteDays.FirstOrDefault().RoutingDaysID, GridLabel, GridLabelIds));
                                }                                
                            }
                        }
                    }
                }
                else
                {
                    lstRoutingDays.Add(AddRoutingDaysInfo(thisItem, "i", UserName, (lstRoutingDays.Count + 1), "", GridLabel, GridLabelIds));
                }
            }
            else
            {
                if (lstRoutingInfo != null && lstRoutingInfo.Count > 0)
                {
                    lstRouteDays = result.RoutingDays.Where(a => a.RouteID == lstRoutingInfo.FirstOrDefault().RouteID && a.IsDeleted == false).ToList();
                    if (lstRouteDays != null && lstRouteDays.Count > 0)
                    {
                        rountecnt = lstRouteDays.Count;

                        if (rountecnt > day)
                        {
                            rountecnt = rountecnt - day;

                            for (int i = 0; i < day; i++)
                            {
                                if (thisItem.FromCityName != thisItem.ToCityName)
                                {
                                    GridLabel = CommonFunction.SplitString(thisItem.FromCityName, ',')[0] + ", " + CommonFunction.SplitString(thisItem.ToCityName, ',')[0];
                                    GridLabelIds = thisItem.FromCityID + ", " + thisItem.ToCityID;
                                }
                                else
                                {
                                    GridLabel = CommonFunction.SplitString(thisItem.ToCityName, ',')[0];
                                    GridLabelIds = thisItem.ToCityID;
                                }

                                lstRoutingDays.Add(AddRoutingDaysInfo(thisItem, "", UserName, (lstRoutingDays.Count + 1), lstRouteDays[i].RoutingDaysID, GridLabel, GridLabelIds));
                            }
                            if (lstRoutingInfo.FirstOrDefault().RouteID == rouitnginfo.LastOrDefault().RouteID && lstRouteDays.Count == (day + 1))
                            {

                            }
                            else
                            {
                                result.RoutingDays.FindAll(rd => lstRouteDays.TakeLast(rountecnt).ToList().Exists(r => r.RouteID == rd.RouteID && r.Days == rd.Days)).ForEach(r =>
                                {
                                    r.EditUser = UserName;
                                    r.EditDate = DateTime.Now;
                                    r.IsDeleted = true;
                                });
                                lstRoutingDaysDel.AddRange(result.RoutingDays.Where(a => a.IsDeleted == true));
                                result.RoutingDays = result.RoutingDays.Where(a => a.IsDeleted == false).ToList();
                            }
                        }
                        else if (rountecnt < day)
                        {
                            for (int i = 0; i < rountecnt; i++)
                            {
                                if (thisItem.FromCityName != thisItem.ToCityName)
                                {
                                    GridLabel = CommonFunction.SplitString(thisItem.FromCityName, ',')[0] + ", " + CommonFunction.SplitString(thisItem.ToCityName, ',')[0];
                                    GridLabelIds = thisItem.FromCityID + ", " + thisItem.ToCityID;
                                }
                                else
                                {
                                    GridLabel = CommonFunction.SplitString(thisItem.ToCityName, ',')[0];
                                    GridLabelIds = thisItem.ToCityID;
                                }

                                lstRoutingDays.Add(AddRoutingDaysInfo(thisItem, "", UserName, (lstRoutingDays.Count + 1), lstRouteDays[i].RoutingDaysID, GridLabel, GridLabelIds));
                            }
                            rountecnt = day - rountecnt;
                            for (int i = 1; i <= rountecnt; i++)
                            {
                                if (thisItem.FromCityName != thisItem.ToCityName)
                                {
                                    GridLabel = CommonFunction.SplitString(thisItem.FromCityName, ',')[0] + ", " + CommonFunction.SplitString(thisItem.ToCityName, ',')[0];
                                    GridLabelIds = thisItem.FromCityID + ", " + thisItem.ToCityID;

                                }
                                else
                                {
                                    GridLabel = CommonFunction.SplitString(thisItem.ToCityName, ',')[0];
                                    GridLabelIds = thisItem.ToCityID;
                                }

                                lstRoutingDays.Add(AddRoutingDaysInfo(thisItem, "i", UserName, (lstRoutingDays.Count + 1), "", GridLabel, GridLabelIds));
                            }
                        }
                        else if (rountecnt == day)
                        {
                            for (int n = 1; n <= day; n++)
                            {
                                if (n != 1)
                                {
                                    thisItem.FromCityID = thisItem.ToCityID;
                                    thisItem.FromCityName = thisItem.ToCityName;
                                }
                                if (thisItem.FromCityName != thisItem.ToCityName)
                                {
                                    GridLabel = CommonFunction.SplitString(thisItem.FromCityName, ',')[0] + ", " + CommonFunction.SplitString(thisItem.ToCityName, ',')[0];
                                    GridLabelIds = thisItem.FromCityID + ", " + thisItem.ToCityID;
                                }
                                else
                                {
                                    GridLabel = CommonFunction.SplitString(thisItem.ToCityName, ',')[0];
                                    GridLabelIds = thisItem.ToCityID;
                                }
                                k = n - 1;
                                lstRoutingDays.Add(AddRoutingDaysInfo(thisItem, "", UserName, (lstRoutingDays.Count + 1), lstRouteDays[k].RoutingDaysID, GridLabel, GridLabelIds));
                            }
                        }
                    }
                    else
                    {
                        AddRoutingDays(lstRoutingDays, day, thisItem, UserName, rouitnginfo, ref d);
                    }
                }
                else
                {
                    AddRoutingDays(lstRoutingDays, day, thisItem, UserName, rouitnginfo, ref d);
                }
            }

            return lstRoutingDays;
        }

        public List<RoutingDays> AddRoutingDays(List<RoutingDays> lstRoutingDays, int day, RoutingInfo thisItem, string UserName, List<RoutingInfo> rouitnginfo, ref int d)
        {
            string GridLabel;
            string GridLabelIds;
            if (day == 0)
            {
                if (thisItem.FromCityName != thisItem.ToCityName)
                {
                    GridLabel = CommonFunction.SplitString(thisItem.FromCityName, ',')[0] + ", " + CommonFunction.SplitString(thisItem.ToCityName, ',')[0];
                    GridLabelIds = thisItem.FromCityID + ", " + thisItem.ToCityID;

                }
                else
                {
                    GridLabel = CommonFunction.SplitString(thisItem.ToCityName, ',')[0];
                    GridLabelIds = thisItem.ToCityID;
                }
                if ((d + 1) != (rouitnginfo.Count))
                {
                    bool isNight = false;
                    for (int cn = (d + 1); cn < rouitnginfo.Count; cn++)
                    {
                        var nextItem = rouitnginfo[cn]; 
                        if (nextItem.Nights != 0)
                        {
                            GridLabel = GridLabel + ", " + CommonFunction.SplitString(nextItem.ToCityName, ',')[0];
                            GridLabelIds = GridLabelIds + ", " + nextItem.ToCityID;

                            if (nextItem.Nights == 1)
                            {
                                //d = d + (cn - 1);
                                d = cn;
                                lstRoutingDays.Add(AddRoutingDaysInfo(nextItem, "i", UserName, (lstRoutingDays.Count + 1), "", GridLabel, GridLabelIds));
                                break;
                            }
                            else
                            {
                                d = cn;
                                for (int n = 1; n <= nextItem.Nights; n++)
                                {
                                    if (n != 1)
                                    {
                                        GridLabel = CommonFunction.SplitString(nextItem.ToCityName, ',')[0];
                                        GridLabelIds = nextItem.ToCityID;
                                        // GridLabel = CommonFunction.SplitString(nextItem.FromCityName, ',')[0] + ", " + CommonFunction.SplitString(nextItem.ToCityName, ',')[0];
                                    }
                                    lstRoutingDays.Add(AddRoutingDaysInfo(nextItem, "i", UserName, (lstRoutingDays.Count + 1), "", GridLabel, GridLabelIds));
                                }
                            }
                        }
                        else
                        {
                            d = cn;
                            isNight = true;
                            GridLabel = GridLabel + ", " + CommonFunction.SplitString(nextItem.ToCityName, ',')[0];
                            GridLabelIds = GridLabelIds + ", " + nextItem.ToCityID;
                        }
                    }
                    if (isNight)
                    {
                        lstRoutingDays.Add(AddRoutingDaysInfo(thisItem, "i", UserName, (lstRoutingDays.Count + 1), "", GridLabel, GridLabelIds));
                    }
                }
            }
            else
            {
                if (day == 1)
                {
                    if (thisItem.FromCityName != thisItem.ToCityName)
                    {
                        GridLabel = CommonFunction.SplitString(thisItem.FromCityName, ',')[0] + ", " + CommonFunction.SplitString(thisItem.ToCityName, ',')[0];
                        GridLabelIds = thisItem.FromCityID + ", " + thisItem.ToCityID;
                    }
                    else
                    {
                        GridLabel = CommonFunction.SplitString(thisItem.ToCityName, ',')[0];
                        GridLabelIds = thisItem.ToCityID;
                    }

                    lstRoutingDays.Add(AddRoutingDaysInfo(thisItem, "i", UserName, (lstRoutingDays.Count + 1), "", GridLabel, GridLabelIds));
                }
                else
                {
                    for (int n = 1; n <= day; n++)
                    {
                        if (n != 1)
                        {
                            thisItem.FromCityID = thisItem.ToCityID;
                            thisItem.FromCityName = thisItem.ToCityName;
                        }
                        if (thisItem.FromCityName != thisItem.ToCityName)
                        {
                            GridLabel = CommonFunction.SplitString(thisItem.FromCityName, ',')[0] + ", " + CommonFunction.SplitString(thisItem.ToCityName, ',')[0];
                            GridLabelIds = thisItem.FromCityID + ", " + thisItem.ToCityID;
                        }
                        else
                        {
                            GridLabel = CommonFunction.SplitString(thisItem.ToCityName, ',')[0];
                            GridLabelIds = thisItem.ToCityID;
                        }

                        lstRoutingDays.Add(AddRoutingDaysInfo(thisItem, "i", UserName, (lstRoutingDays.Count + 1), "", GridLabel, GridLabelIds));
                    }
                }
            }

            return lstRoutingDays;
        }

        public RoutingDays AddRoutingDaysInfo(RoutingInfo obj, string flag, string username, int counter, string RoutingDaysID, string GridLabel = "", string GridLabelIds = "")
        {
            RoutingDays routingDays = new RoutingDays();
            if (flag == "i")
            {
                routingDays.RoutingDaysID = Guid.NewGuid().ToString();
                routingDays.CreateUser = username;
            }
            else
            {
                routingDays.CreateDate = obj.CreateDate;
                routingDays.CreateUser = obj.CreateUser;
                routingDays.EditDate = DateTime.Now;
                routingDays.EditUser = username;
                routingDays.RoutingDaysID = RoutingDaysID;
            }

            routingDays.Days = "Day " + counter;
            routingDays.DayNo = counter;
            routingDays.FromCityID = obj.FromCityID;
            routingDays.FromCityName = obj.FromCityName;
            routingDays.RouteID = obj.RouteID;
            routingDays.IsDeleted = obj.IsDeleted;
            routingDays.ToCityID = obj.ToCityID;
            routingDays.ToCityName = obj.ToCityName;
            routingDays.GridLabel = GridLabel;
            routingDays.GridLabelIds = GridLabelIds;
            routingDays.RouteSequence = obj.RouteSequence;
            return routingDays;
        }

        public async Task<RoutingDaysGetRes> GetQRFRoutingDays(RoutingDaysGetReq request)
        {
            RoutingDaysGetRes response = new RoutingDaysGetRes();
            var builder = Builders<mQuote>.Filter;
            var filter = builder.Where(q => q.QRFID == request.QRFID);
            var result = await _MongoContext.mQuote.Find(filter).Project(r => r.RoutingDays).FirstOrDefaultAsync();
            if (result != null && result.Count > 0)
            {
                result = result.Where(a => a.IsDeleted == false).OrderBy(a => a.DayNo).ToList();
            }
            response.RoutingDays = result != null && result.Count > 0 ? result : new List<RoutingDays>();
            response.QRFID = request.QRFID;
            response.ResponseStatus.Status = result != null && result.Count > 0 ? "Success" : "Failure";
            response.ResponseStatus.ErrorMessage = result != null && result.Count > 0 ? "" : "No Details Found.";
            return response;
        }

        public async Task<string> UpdatePositions(string username, string QRFID)
        {
            var builder = Builders<mQuote>.Filter;
            var filter = builder.Where(q => q.QRFID == QRFID);
            var result = await _MongoContext.mQuote.Find(filter).Project(r => new RoutingDaysInfo { RoutingInfo = r.RoutingInfo, RoutingDays = r.RoutingDays }).FirstOrDefaultAsync();
            var NoOfNights = 0;

            if (result != null)
            {
                var res = new mPosition();
                FilterDefinition<mPosition> filterRout;
                foreach (var item in result.RoutingDays)
                {
                    filterRout = Builders<mPosition>.Filter.Empty;
                    filterRout = filterRout & Builders<mPosition>.Filter.Where(x => x.QRFID == QRFID);
                    filterRout = filterRout & Builders<mPosition>.Filter.Where(x => x.RoutingDaysID == item.RoutingDaysID);
                    filterRout = filterRout & Builders<mPosition>.Filter.Where(x => x.IsDeleted == false);

                    //res = new mPosition();
                    NoOfNights = result.RoutingInfo.Where(a => a.RouteID == item.RouteID).Select(a => a.Nights).FirstOrDefault();
                    UpdateResult objUpdateResult = _MongoContext.mPosition.UpdateMany(filterRout,
                                                        Builders<mPosition>.Update.Set("IsDeleted", item.IsDeleted).
                                                       Set("EditUser", username).Set("DayNo", item.DayNo).Set("StartingFrom", item.Days).Set("Duration", NoOfNights).
                                                       Set("EditDate", DateTime.Now).Set("DeletedFrom", (item.IsDeleted ? "RoutingSaveService" : "")));
                }

                var positionList = _MongoContext.mPosition.AsQueryable().Where(a => a.QRFID == QRFID && a.IsDeleted == false).ToList();

                foreach (var position in positionList)
                {
                    var IsRoutingExist = result.RoutingDays.Any(a => a.RoutingDaysID == position.RoutingDaysID);

                    if (!IsRoutingExist)
                    {
                        filterRout = Builders<mPosition>.Filter.Empty;
                        filterRout = filterRout & Builders<mPosition>.Filter.Where(x => x.PositionId == position.PositionId);
                        //filterRout = filterRout & Builders<mPosition>.Filter.Where(x => x.IsDeleted == false);

                        if (position.ProductType == "Hotel")
                        {
                            res = new mPosition();
                            res = _MongoContext.mPosition.FindOneAndUpdate(filterRout,
                                                               Builders<mPosition>.Update.Set("IsDeleted", true).
                                                              Set("EditUser", username).
                                                              Set("EditDate", DateTime.Now).Set("DeletedFrom", "RoutingSaveService"));
                        }
                        else
                        {
                            var RoutingDay = result.RoutingDays.Where(a => a.DayNo <= position.DayNo).OrderByDescending(b => b.DayNo).FirstOrDefault();
                            if (RoutingDay != null)
                            {
                                NoOfNights = result.RoutingInfo.Where(a => a.RouteID == RoutingDay.RouteID).Select(a => a.Nights).FirstOrDefault();
                                res = new mPosition();
                                res = _MongoContext.mPosition.FindOneAndUpdate(filterRout,
                                                              Builders<mPosition>.Update.Set("RoutingDaysID", RoutingDay.RoutingDaysID).
                                                             Set("EditUser", username).Set("DayNo", RoutingDay.DayNo).Set("StartingFrom", RoutingDay.Days).Set("Duration", NoOfNights).
                                                             Set("EditDate", DateTime.Now).Set("DeletedFrom", "RoutingSaveService"));
                            }
                            else
                            {
                                res = new mPosition();
                                res = _MongoContext.mPosition.FindOneAndUpdate(filterRout,
                                                                   Builders<mPosition>.Update.Set("IsDeleted", true).
                                                                  Set("EditUser", username).
                                                                  Set("EditDate", DateTime.Now).Set("DeletedFrom", "RoutingSaveService"));
                            }
                        }
                    }
                }

                var delPositionIds = _MongoContext.mPosition.AsQueryable().Where(a => a.QRFID == QRFID && a.IsDeleted == true).Select(a => a.PositionId).ToList();
                if (delPositionIds?.Count > 0)
                {
                    bool delFlag = await _genericRepository.DeletePositionPriceFOC(delPositionIds, username);
                }
            }
            return "1";
        }

        public async Task<bool> AddHotels(string username, string QRFID, bool IsOverwriteExtPos)
        {
            var quote = _MongoContext.mQuote.AsQueryable().Where(a => a.QRFID == QRFID).FirstOrDefault();
            var positionList = _MongoContext.mPosition.AsQueryable().Where(a => a.QRFID == QRFID && a.IsDeleted == false).ToList();
            var hotelpositionList = positionList.Where(a => a.ProductType == "Hotel").ToList();
            var positionNewList = new List<mPosition>();

            #region AgentRooms
            List<string> roomTypeName = new List<string>();
            foreach (var data in quote.AgentRoom)
            {
                roomTypeName.Add(data.RoomTypeName.ToUpper());
            }
            #endregion

            var position = new mPosition();
            ProductSearchReq productSearchReq = new ProductSearchReq();
            ProdCategoryRangeGetReq catRangeReq = new ProdCategoryRangeGetReq();
            ProductSupplierGetReq objProductSupplierGetReq = new ProductSupplierGetReq();
            RoomDetailsInfo objRoomDetailsInfo = new RoomDetailsInfo();
            List<ProductRangeDetails> lstProductRangeDetails = new List<ProductRangeDetails>();
            List<ProdCategoryDetails> lstProdCategoryDetails = new List<ProdCategoryDetails>();

            if (quote != null)
            {
                var res = new mPosition();

                quote.RoutingInfo = quote.RoutingInfo?.Where(a => a.IsDeleted != true).OrderBy(a => a.RouteSequence).ToList();
                if (quote.RoutingInfo != null && quote.RoutingInfo.Count > 0)
                {
                    bool IsInsert = true;
                    int day = 0;
                    int prevnight = 0;
                    int sequence = 1;
                    int roomsequence = 1;
                    var ProductType = _MongoContext.mProductType.AsQueryable().Where(a => a.Prodtype == "Hotel").FirstOrDefault();
                    var products = _MongoContext.mProducts.AsQueryable().Where(a => a.ProductType == "Hotel" && a.Placeholder == true).ToList();

                    #region GetProductList

                    List<string> CityList = new List<string>();
                    List<string> CountryList = new List<string>();
                    List<string> StarRatingList = new List<string>();
                    List<string> ProductIdList = new List<string>();
                    List<Products> ProductList = new List<Products>();

                    quote.RoutingInfo.ForEach(a => CityList.Add(a.ToCityName.Split(",")[0].Trim()));
                    quote.RoutingInfo.ForEach(a => CountryList.Add(a.ToCityName.Split(",")[1].Trim()));
                    quote.RoutingInfo.ForEach(a => StarRatingList.Add(a.PrefStarRating));

                    productSearchReq = new ProductSearchReq();
                    productSearchReq.CityList = CityList.Distinct().ToList();
                    productSearchReq.CountryList = CountryList.Distinct().ToList();
                    productSearchReq.ProdName = "###";
                    productSearchReq.ProdType = new List<string>() { "Hotel" };
                    productSearchReq.StarRatingList = StarRatingList.Distinct().ToList();
                    productSearchReq.IsPlaceHolder = true;
                    var productSearchRes = await _productRepository.GetProductDetailsBySearchCriteria(productSearchReq);

                    if (productSearchRes?.Count > 0)
                    {
                        productSearchRes.ForEach(a => ProductIdList.Add(a.VoyagerProduct_Id));
                        ProductList = await _productPDPRepository.GetProductFullDetailsById(ProductIdList);
                    }

                    #endregion

                    foreach (var Route in quote.RoutingInfo.Where(a => a.Nights > 0))
                    {
                        IsInsert = true;
                        if (day == 0) { day = 1; }
                        else { day = prevnight + day; }

                        if (!string.IsNullOrEmpty(Route.PrefStarRating))
                        {
                            var RoutingDays = quote.RoutingDays.Where(a => a.Days == "Day " + day.ToString()).FirstOrDefault();

                            var oldPosition = hotelpositionList?.Where(a => a.RoutingDaysID == RoutingDays?.RoutingDaysID).FirstOrDefault();
                            if (oldPosition != null)
                            {
                                if (oldPosition.StarRating == Route.PrefStarRating)
                                {
                                    IsInsert = false;
                                }
                                else
                                {
                                    var oldPositionPH = products.Where(a => a.VoyagerProduct_Id == oldPosition?.ProductID).Select(b => b.Placeholder).FirstOrDefault();
                                    if (oldPositionPH == false || oldPositionPH == null)
                                    {
                                        IsInsert = false;
                                    }
                                    else
                                    {
                                        if (IsOverwriteExtPos == false)
                                        {
                                            IsInsert = false;
                                        }
                                    }
                                }
                            }

                            if (IsInsert)
                            {
                                if (oldPosition != null)
                                {
                                    //await _MongoContext.mPosition.FindOneAndUpdateAsync(Builders<mPosition>.Filter.Eq("PositionId", oldPosition.PositionId),
                                    //                               Builders<mPosition>.Update.Set("IsDeleted", true).
                                    //                              Set("EditUser", username).
                                    //                              Set("EditDate", DateTime.Now).Set("DeletedFrom", ("RoutingSaveService")));
                                    await _MongoContext.mPosition.FindOneAndDeleteAsync(Builders<mPosition>.Filter.Eq("PositionId", oldPosition.PositionId));
                                    await _genericRepository.DeletePositionPriceFOC(new List<string>() { oldPosition.PositionId }, username, false, true);
                                }
                                var ProductId = productSearchRes.Where(a => a.ProdLocation?.CityName == Route.ToCityName.Split(",")[0].Trim()
                                 && a.ProdLocation?.CountryName == Route.ToCityName.Split(",")[1].Trim() && a.StarRating == Route.PrefStarRating).Select(a => a.VoyagerProduct_Id).FirstOrDefault();

                                if (ProductId == null)
                                {
                                    ProductId = productSearchRes.Where(a => a.ProdLocation?.CountryName == Route.ToCityName.Split(",")[1].Trim() && a.StarRating == Route.PrefStarRating).Select(a => a.VoyagerProduct_Id).FirstOrDefault();
                                }

                                var Product = ProductList?.Where(a => a.VoyagerProduct_Id == ProductId).FirstOrDefault();
                                if (Product != null)
                                {
                                    var ProductCategory = Product.ProductCategories.Where(a => a.ProductCategoryName == "Standard").FirstOrDefault();

                                    position = new mPosition();
                                    position.QRFID = QRFID;
                                    position.PositionId = Guid.NewGuid().ToString();
                                    position.PositionSequence = sequence;
                                    position.ProductType = ProductType?.Prodtype;
                                    position.ProductTypeId = ProductType?.VoyagerProductType_Id;
                                    position.DayNo = RoutingDays == null ? 0 : RoutingDays.DayNo;
                                    position.RoutingDaysID = RoutingDays?.RoutingDaysID;
                                    position.StartingFrom = RoutingDays?.Days;
                                    position.CityID = Route.ToCityID;
                                    position.CityName = Route.ToCityName.Split(",")[0].Trim();
                                    position.CountryName = Route.ToCityName.Split(",")[1].Trim();

                                    position.ProductID = Product.VoyagerProduct_Id;
                                    position.ProductName = Product.ProductName;
                                    position.BudgetCategoryId = Product.HotelAdditionalInfo?.BdgPriceCategoryId;
                                    position.BudgetCategory = Product.HotelAdditionalInfo?.BdgPriceCategory;
                                    position.StarRating = Product.HotelAdditionalInfo?.StarRating;
                                    position.Location = Product.HotelAdditionalInfo?.Location;

                                    position.StartTime = "20:00";
                                    position.EndTime = "08:30";
                                    position.Duration = Route.Nights;
                                    position.KeepAs = "Included";
                                    position.CreateUser = username;
                                    position.CreateDate = DateTime.Now;
                                    position.IsDeleted = false;

                                    #region Meal Plan
                                    if (string.IsNullOrEmpty(quote.DefaultMealPlan))
                                        position.MealPlan = "BB";
                                    else
                                        position.MealPlan = quote.DefaultMealPlan;
                                    #endregion

                                    #region Rooms
                                    var DefaultRoom = ProductCategory.ProductRanges.Where(a => roomTypeName.Contains(a.ProductTemplateName) && a.AdditionalYn != true).ToList();
                                    if (DefaultRoom != null && DefaultRoom.Count > 0)
                                    {
                                        roomsequence = 1;
                                        DefaultRoom.ForEach(a => position.RoomDetailsInfo.Add(new RoomDetailsInfo()
                                        {
                                            RoomId = Guid.NewGuid().ToString(),
                                            RoomSequence = roomsequence++,
                                            ProductCategoryId = ProductCategory.ProductCategory_Id,
                                            ProductCategory = ProductCategory.ProductCategoryName,
                                            ProductRangeId = a.ProductRange_Id,
                                            ProductRange = a.ProductTemplateCode + " (" + a.PersonType + (string.IsNullOrEmpty(a.Agemin) ? "" : " | " + a.Agemin + " - " + a.Agemax) + ") ",
                                            IsSupplement = false,
                                            CreateUser = username,
                                            CreateDate = DateTime.Now,
                                            IsDeleted = false
                                        }));
                                    }
                                    #endregion

                                    #region Tour Entities

                                    if (quote.TourEntities?.Count > 0)
                                    {
                                        quote.TourEntities.ForEach(a => a.Flag = (a.Type.Contains("Coach") || a.Type.Contains("LDC") ? "DRIVER" : (a.Type.Contains("Guide") || a.Type.Contains("Assistant")) ? "GUIDE" : "GUIDE"));

                                        foreach (var tourEntity in quote.TourEntities)
                                        {
                                            if (Convert.ToInt16(tourEntity.HowMany) > 0 && tourEntity.IsDeleted == false)
                                            {
                                                var CrossPosition = positionList?.Where(a => a.PositionId == tourEntity.PositionID).FirstOrDefault();

                                                int duration = Convert.ToInt32(CrossPosition?.DayNo == 1 ? (CrossPosition?.Duration - 1) : ((CrossPosition?.Duration + CrossPosition?.DayNo) - 2));

                                                if (position.DayNo >= CrossPosition?.DayNo && position.DayNo <= duration)
                                                {
                                                    lstProductRangeDetails = _productRepository.GetProductRangeByParam(new ProductRangeGetReq { ProductIdList = new List<string>() { position.ProductID }, PersonTypeList = new List<string>() { "DRIVER", "GUIDE" } }).ProductRangeDetails;

                                                    var prodrangeres = lstProductRangeDetails?.Where(a => a.ProductRangeName == tourEntity.RoomType.ToUpper() && a.PersonType == tourEntity.Flag).FirstOrDefault();

                                                    lstProdCategoryDetails = _productRepository.GetProductCategoryByParam(new ProductCatGetReq { ProductIdList = new List<string>() { position.ProductID } });

                                                    if (prodrangeres != null)
                                                    {
                                                        var cntromminfo = position.RoomDetailsInfo.Where(a => a.ProductRangeId == prodrangeres.VoyagerProductRange_Id && a.ProductCategoryId == prodrangeres.ProductCategoryId
                                                           && a.CrossPositionId == CrossPosition?.PositionId).Count();

                                                        if (cntromminfo == 0)
                                                        {
                                                            objRoomDetailsInfo = new RoomDetailsInfo
                                                            {
                                                                RoomSequence = roomsequence++,
                                                                CreateDate = DateTime.Now,
                                                                CreateUser = username,
                                                                EditDate = null,
                                                                EditUser = "",
                                                                IsDeleted = false,
                                                                IsSupplement = prodrangeres.AdditionalYN == true ? true : false,
                                                                ProductCategory = lstProdCategoryDetails?.Where(b => b.ProductCategoryId == prodrangeres.ProductCategoryId).FirstOrDefault() != null ?
                                                                        lstProdCategoryDetails?.Where(b => b.ProductCategoryId == prodrangeres.ProductCategoryId).FirstOrDefault().ProductCategoryName : "",
                                                                ProductCategoryId = prodrangeres.ProductCategoryId,
                                                                ProductRange = prodrangeres.ProductRangeName + " (" + prodrangeres.PersonType + ")",
                                                                ProductRangeId = prodrangeres.VoyagerProductRange_Id,
                                                                RoomId = Guid.NewGuid().ToString(),
                                                                CrossPositionId = CrossPosition?.PositionId
                                                            };
                                                            position.RoomDetailsInfo.Add(objRoomDetailsInfo);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    #endregion

                                    #region Supplier
                                    var Supplier = Product.ProductSuppliers.Where(a => a.IsDefault == true).FirstOrDefault();
                                    if (Supplier != null)
                                    {
                                        position.SupplierId = Supplier.Company_Id;
                                        position.SupplierName = Supplier.CompanyName;
                                    }
                                    #endregion

                                    await _MongoContext.mPosition.InsertOneAsync(position);
                                    positionNewList.Add(position);
                                    sequence = sequence + 1;
                                }
                            }
                        }
                        prevnight = Route.Nights;
                    }

                    #region Similar Hotels
                    if (positionNewList?.Count > 0)
                    {
                        foreach (var position1 in positionNewList)
                        {
                            Thread t = new Thread(new ThreadStart(() => _productRepository.SaveSimilarHotels(position1.PositionId, position1.ProductID, "", false)));
                            t.Start();
                        }
                    }
                    #endregion
                }
            }
            return true;
        }
        #endregion

        #region Margins
        public async Task<Margins> GetQRFMarginDetailsByQRFID(MarginGetReq request)
        {
            try
            {
                var builder = Builders<mQuote>.Filter;
                var filter = builder.Where(q => q.QRFID == request.QRFID);
                var result = await _MongoContext.mQuote.Find(filter).Project(r => r.Margins).FirstOrDefaultAsync();

                result = result ?? new Margins();
                if (result.Product == null)
                    result.Product = new Product { ProductProperties = new List<ProductProperties>() };
                if (result.Package == null)
                    result.Package = new Package { PackageProperties = new List<PackageProperties>() };
                if (result.Itemwise == null)
                    result.Itemwise = new Itemwise { ItemProperties = new List<ItemProperties>() };

                if (request.IsCostingMargin)
                {
                    result.Package = new Package { PackageProperties = new List<PackageProperties>() };
                    var resultQRF = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == request.QRFID && a.IsCurrentVersion == true).OrderByDescending(b => b.VersionId).Select(c => c.QRFMargin).FirstOrDefault();

                    result.SelectedMargin = resultQRF.SelectedMargin;
                    if (resultQRF.Package != null)
                    {
                        if (resultQRF.Package.PackageProperties != null)
                        {
                            foreach (var pp in resultQRF.Package.PackageProperties)
                            {
                                result.Package.PackageProperties.Add(new PackageProperties
                                {
                                    PackageID = pp.PackageID,
                                    ComponentName = pp.ComponentName,
                                    SellingPrice = pp.SellingPrice,
                                    MarginUnit = pp.MarginUnit
                                });
                            }
                        }

                        if (resultQRF.Package.MarginComputed != null)
                        {
                            result.Package.MarginComputed.TotalCost = resultQRF.Package.MarginComputed.TotalCost;
                            result.Package.MarginComputed.TotalLeadersCost = resultQRF.Package.MarginComputed.TotalLeadersCost;
                            result.Package.MarginComputed.Upgrade = resultQRF.Package.MarginComputed.Upgrade;
                            result.Package.MarginComputed.MarkupType = resultQRF.Package.MarginComputed.MarkupType;
                        }
                    }
                }

                result.Product.ProductProperties = GetProductTypeDetails(request);
                result.Itemwise.ItemProperties = GetItemwiseDetails(request);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<string> InsertUpdateQRFMarginDetails(MarginSetReq request)
        {
            UpdateResult resultFlag;
            var result = _MongoContext.mQuote.AsQueryable().Where(a => a.QRFID == request.QRFID).ToList();
            mQuote quote = new mQuote();
            if (result != null && result.Count > 0)
            {
                if (request.IsCostingMargin)
                {
                    request.Margins.Package.PackageProperties.ForEach(r =>
                    {
                        r.PackageID = string.IsNullOrEmpty(r.PackageID) ? ObjectId.GenerateNewId().ToString() : r.PackageID;
                    });

                    request.Margins.Product.ProductProperties.ForEach(r =>
                    {
                        r.ProductID = string.IsNullOrEmpty(r.ProductID) ? ObjectId.GenerateNewId().ToString() : r.ProductID;
                    });

                    request.Margins.Itemwise.ItemProperties.ForEach(r =>
                    {
                        r.ItemID = string.IsNullOrEmpty(r.ItemID) ? ObjectId.GenerateNewId().ToString() : r.ItemID;
                    });

                    var position = _MongoContext.mQRFPosition.AsQueryable().Where(r => r.QRFID == request.QRFID && r.IsDeleted == false).OrderBy(a => a.ProductType).ThenBy(b => b.DayNo).ToList();
                    var guesstimate = _MongoContext.mGuesstimate.AsQueryable().Where(r => r.QRFID == request.QRFID && r.IsCurrentVersion == true).FirstOrDefault();
                    foreach (var pos in position)
                    {
                        var KeepAs = guesstimate.GuesstimatePosition.Where(a => a.PositionId == pos.PositionId).Select(b => b.KeepAs).FirstOrDefault();
                        if (KeepAs != null)
                            pos.KeepAs = KeepAs;
                    }


                    var QRFPriceId = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == request.QRFID && a.IsCurrentVersion == true).OrderByDescending(b => b.VersionId).Select(c => c.QRFPrice_Id).FirstOrDefault();
                    var savedMargins = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == request.QRFID && a.IsCurrentVersion == true).OrderByDescending(b => b.VersionId).Select(c => c.QRFMargin).FirstOrDefault();

                    var QRFMargin = new QRFMargins();

                    #region Map quote margin class to Costing margin class
                    if (request.Margins != null)
                    {
                        QRFMargin.CreateUser = savedMargins.CreateUser;
                        QRFMargin.CreateDate = savedMargins.CreateDate;
                        QRFMargin.EditUser = request.Margins.EditUser;
                        QRFMargin.EditDate = request.Margins.EditDate;
                        QRFMargin.SelectedMargin = request.Margins.SelectedMargin;

                        if (request.Margins.Package != null)
                        {
                            if (request.Margins.Package.PackageProperties != null)
                            {
                                foreach (var pp in request.Margins.Package.PackageProperties)
                                {
                                    QRFMargin.Package.PackageProperties.Add(new QRFMarginPackageProperties
                                    {
                                        PackageID = pp.PackageID,
                                        ComponentName = pp.ComponentName,
                                        SellingPrice = pp.SellingPrice,
                                        MarginUnit = pp.MarginUnit
                                    });
                                }
                            }

                            if (request.Margins.Package.MarginComputed != null)
                            {
                                QRFMargin.Package.MarginComputed.TotalCost = request.Margins.Package.MarginComputed.TotalCost;
                                QRFMargin.Package.MarginComputed.TotalLeadersCost = request.Margins.Package.MarginComputed.TotalLeadersCost;
                                QRFMargin.Package.MarginComputed.Upgrade = request.Margins.Package.MarginComputed.Upgrade;
                                QRFMargin.Package.MarginComputed.MarkupType = request.Margins.Package.MarginComputed.MarkupType;
                            }
                        }

                        if (request.Margins.Product != null)
                        {
                            if (request.Margins.Product.ProductProperties != null)
                            {
                                foreach (var pp in request.Margins.Product.ProductProperties)
                                {
                                    QRFMargin.Product.ProductProperties.Add(new QRfMarginProductProperties
                                    {
                                        ProductID = pp.ProductID,
                                        VoyagerProductType_Id = pp.VoyagerProductType_Id,
                                        Prodtype = pp.Prodtype,
                                        SellingPrice = pp.SellingPrice,
                                        MarginUnit = pp.MarginUnit,
                                        HowMany = pp.HowMany
                                    });
                                }
                            }

                            if (request.Margins.Product.MarginComputed != null)
                            {
                                QRFMargin.Product.MarginComputed.TotalCost = request.Margins.Product.MarginComputed.TotalCost;
                                QRFMargin.Product.MarginComputed.TotalLeadersCost = request.Margins.Product.MarginComputed.TotalLeadersCost;
                                QRFMargin.Product.MarginComputed.Upgrade = request.Margins.Product.MarginComputed.Upgrade;
                                QRFMargin.Product.MarginComputed.MarkupType = request.Margins.Product.MarginComputed.MarkupType;
                            }
                        }

                        if (request.Margins.SelectedMargin == "ServiceItem")
                        {
                            if (request.Margins.Itemwise != null)
                            {
                                if (request.Margins.Itemwise.ItemProperties != null)
                                {
                                    foreach (var pp in request.Margins.Itemwise.ItemProperties)
                                    {
                                        QRFMargin.Item.ItemProperties.Add(new QRfMarginItemProperties
                                        {
                                            ItemID = pp.ItemID,
                                            PositionID = pp.PositionID,
                                            ProductName = pp.ProductName,
                                            VoyagerProductType_Id = pp.VoyagerProductType_Id,
                                            Prodtype = pp.Prodtype,
                                            SellingPrice = pp.SellingPrice,
                                            MarginUnit = pp.MarginUnit,
                                            HowMany = pp.HowMany
                                        });
                                    }
                                }

                                if (request.Margins.Itemwise.MarginComputed != null)
                                {
                                    QRFMargin.Item.MarginComputed.TotalCost = request.Margins.Product.MarginComputed.TotalCost;
                                    QRFMargin.Item.MarginComputed.TotalLeadersCost = request.Margins.Product.MarginComputed.TotalLeadersCost;
                                    QRFMargin.Item.MarginComputed.Upgrade = request.Margins.Product.MarginComputed.Upgrade;
                                    QRFMargin.Item.MarginComputed.MarkupType = request.Margins.Product.MarginComputed.MarkupType;
                                }
                            }
                        }
                        else
                        {
                            foreach (var posM in position)
                            {
                                QRFMargin.Item.ItemProperties.Add(new QRfMarginItemProperties
                                {
                                    ItemID = Guid.NewGuid().ToString(),
                                    PositionID = posM.PositionId,
                                    ProductName = posM.ProductName,
                                    VoyagerProductType_Id = posM.ProductTypeId,
                                    Prodtype = posM.ProductType,
                                    SellingPrice = 0,
                                    MarginUnit = "",
                                    HowMany = "",
                                    KeepAs = posM.KeepAs
                                });
                            }
                            if (request.Margins.SelectedMargin == "Package")
                            {
                                if (request.Margins.Package != null)
                                {
                                    if (request.Margins.Package.PackageProperties != null)
                                    {
                                        var marPackInc = request.Margins.Package.PackageProperties.Where(a => a.ComponentName == "Package not including Accommodation"
                                                     || a.ComponentName == "Package including Accommodation").FirstOrDefault();
                                        var marPackSup = request.Margins.Package.PackageProperties.Where(a => a.ComponentName == "Suppliments").FirstOrDefault();
                                        var marPackOpt = request.Margins.Package.PackageProperties.Where(a => a.ComponentName == "Optionals").FirstOrDefault();

                                        marPackInc = marPackInc == null ? new PackageProperties() : marPackInc;
                                        marPackSup = marPackSup == null ? new PackageProperties() : marPackSup;
                                        marPackOpt = marPackOpt == null ? new PackageProperties() : marPackOpt;

                                        foreach (var itemM in QRFMargin.Item.ItemProperties)
                                        {
                                            if (itemM.SellingPrice == 0)
                                            {
                                                if (itemM.KeepAs == "Included")
                                                {
                                                    itemM.SellingPrice = marPackInc.SellingPrice;
                                                    itemM.MarginUnit = marPackInc.MarginUnit;
                                                }
                                                else if (itemM.KeepAs == "Supplement")
                                                {
                                                    itemM.SellingPrice = marPackSup.SellingPrice;
                                                    itemM.MarginUnit = marPackSup.MarginUnit;
                                                }
                                                else if (itemM.KeepAs == "Optional")
                                                {
                                                    itemM.SellingPrice = marPackOpt.SellingPrice;
                                                    itemM.MarginUnit = marPackOpt.MarginUnit;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else if (request.Margins.SelectedMargin == "Product")
                            {
                                if (request.Margins.Product != null)
                                {
                                    if (request.Margins.Product.ProductProperties != null)
                                    {
                                        foreach (var itemM in QRFMargin.Item.ItemProperties)
                                        {
                                            if (itemM.SellingPrice == 0)
                                            {
                                                itemM.SellingPrice = request.Margins.Product.ProductProperties.Where(a => a.VoyagerProductType_Id == itemM.VoyagerProductType_Id).Select(b => b.SellingPrice).FirstOrDefault();
                                                itemM.MarginUnit = request.Margins.Product.ProductProperties.Where(a => a.VoyagerProductType_Id == itemM.VoyagerProductType_Id).Select(b => b.MarginUnit).FirstOrDefault();

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    resultFlag = await _MongoContext.mQRFPrice.UpdateOneAsync(Builders<mQRFPrice>.Filter.Eq("QRFPrice_Id", QRFPriceId),
                                   Builders<mQRFPrice>.Update.Set("QRFMargin", QRFMargin));

                    return resultFlag.MatchedCount > 0 ? "1" : "Margin Details not updated.";
                }
                else
                {
                    Margins margins = result.Select(r => r.Margins).FirstOrDefault();

                    //if (margins != null)
                    //{
                    //    var resultPkg = SetPackage(quote, margins, request);
                    //    var resultProd = SetProduct(quote, margins, request);
                    //    var resultItem = SetItemwise(quote, margins, request);
                    //}
                    //else
                    //{
                    //    request.Margins.CreateDate = DateTime.Now;
                    //    request.Margins.EditDate = null;
                    //    request.Margins.EditUser = "";

                    //    resultFlag = await _MongoContext.mQuote.UpdateOneAsync(Builders<mQuote>.Filter.Eq("QRFID", request.QRFID),
                    //               Builders<mQuote>.Update.Set("Margins", request.Margins).
                    //               Set("CurrentPipeline", "Quote Pipeline").
                    //               Set("CurrentPipelineStep", "Quote").Set("CurrentPipelineSubStep", "Margins"));

                    //    return resultFlag.MatchedCount > 0 ? "1" : "Margin Details not updated.";
                    //}

                    request.Margins.Package.PackageProperties.ForEach(r =>
                    {
                        r.PackageID = string.IsNullOrEmpty(r.PackageID) ? ObjectId.GenerateNewId().ToString() : r.PackageID;
                    });

                    request.Margins.Product.ProductProperties.ForEach(r =>
                    {
                        r.ProductID = string.IsNullOrEmpty(r.ProductID) ? ObjectId.GenerateNewId().ToString() : r.ProductID;
                    });

                    request.Margins.Itemwise.ItemProperties.ForEach(r =>
                    {
                        r.ItemID = string.IsNullOrEmpty(r.ItemID) ? ObjectId.GenerateNewId().ToString() : r.ItemID;
                    });

                    if (margins == null)
                    {
                        request.Margins.EditDate = null;
                        request.Margins.EditUser = "";
                        request.Margins.CreateDate = DateTime.Now;
                    }
                    else
                    {
                        request.Margins.EditDate = DateTime.Now;
                        request.Margins.CreateDate = margins.CreateDate;
                        request.Margins.CreateUser = margins.CreateUser;
                    }
                    request.Margins.CreateUser = request.Margins.CreateUser;
                    request.Margins.CreateDate = request.Margins.CreateDate;
                    request.Margins.EditDate = request.Margins.EditDate;
                    request.Margins.EditUser = request.Margins.EditUser;

                    resultFlag = await _MongoContext.mQuote.UpdateOneAsync(Builders<mQuote>.Filter.Eq("QRFID", request.QRFID),
                               Builders<mQuote>.Update.Set("Margins", request.Margins).
                               Set("CurrentPipeline", "Quote Pipeline").
                               Set("CurrentPipelineStep", "Quote").Set("CurrentPipelineSubStep", "Margins"));

                    return resultFlag.MatchedCount > 0 ? "1" : "Margin Details not updated.";
                }
            }
            else
            {
                return "QRF ID not exist.";
            }
        }

        public async Task<string> SetPackage(mQuote mquote, Margins margins, MarginSetReq request)
        {
            try
            {
                UpdateResult resultFlag;
                if (margins.Package != null && margins.Package.PackageProperties != null && margins.Package.PackageProperties.Count > 0)
                {
                    request.Margins.Package.PackageProperties.ForEach(r =>
                    {
                        r.PackageID = string.IsNullOrEmpty(r.PackageID) ? ObjectId.GenerateNewId().ToString() : r.PackageID;
                    });

                    mquote = await _MongoContext.mQuote.FindOneAndUpdateAsync(Builders<mQuote>.Filter.Eq("QRFID", request.QRFID),
                                                Builders<mQuote>.Update.Set("Margins.Package", request.Margins.Package).
                                                Set("Margins.EditUser", request.Margins.EditUser).
                                                Set("Margins.EditDate", DateTime.Now).
                                                Set("Margins.SelectedMargin", request.Margins.SelectedMargin).
                                                Set("CurrentPipeline", "Quote Pipeline").
                                                Set("CurrentPipelineStep", "Quote").Set("CurrentPipelineSubStep", "Margins"));


                    return mquote != null ? "1" : "Margin Details not updated.";
                }
                else
                {
                    request.Margins.Package.PackageProperties.ForEach(r =>
                    {
                        r.PackageID = ObjectId.GenerateNewId().ToString();
                    });
                    resultFlag = await _MongoContext.mQuote.UpdateOneAsync(Builders<mQuote>.Filter.Eq("QRFID", request.QRFID),
                                               Builders<mQuote>.Update.Set("Margins.Package", request.Margins.Package).
                                               Set("Margins.CreateUser", request.Margins.CreateUser).
                                               Set("Margins.CreateDate", DateTime.Now).
                                               Set("Margins.SelectedMargin", request.Margins.SelectedMargin).
                                               Set("CurrentPipeline", "Quote Pipeline").
                                               Set("CurrentPipelineStep", "Quote").Set("CurrentPipelineSubStep", "Margins"));

                    return resultFlag.MatchedCount > 0 ? "1" : "Margin Details not updated.";
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "Margin Details not updated.";
            }
        }

        public string SetProduct(mQuote quote, Margins margins, MarginSetReq request)
        {
            UpdateResult resultFlag;
            if (margins.Product != null && margins.Product.ProductProperties != null && margins.Product.ProductProperties.Count > 0)
            {
                request.Margins.Product.ProductProperties.ForEach(r =>
                {
                    r.ProductID = string.IsNullOrEmpty(r.ProductID) ? ObjectId.GenerateNewId().ToString() : r.ProductID;
                });

                quote = _MongoContext.mQuote.FindOneAndUpdate(Builders<mQuote>.Filter.Eq("QRFID", request.QRFID),
                                            Builders<mQuote>.Update.Set("Margins.Product", request.Margins.Product).
                                            Set("Margins.EditUser", request.Margins.EditUser).
                                            Set("Margins.EditDate", DateTime.Now).
                                            Set("Margins.SelectedMargin", request.Margins.SelectedMargin).
                                            Set("CurrentPipeline", "Quote Pipeline").
                                            Set("CurrentPipelineStep", "Quote").Set("CurrentPipelineSubStep", "Margins"));
                return quote != null ? "1" : "Margin Details not updated.";
            }
            else
            {
                request.Margins.Product.ProductProperties.ForEach(r =>
                {
                    r.ProductID = ObjectId.GenerateNewId().ToString();
                });
                resultFlag = _MongoContext.mQuote.UpdateOne(Builders<mQuote>.Filter.Eq("QRFID", request.QRFID),
                                           Builders<mQuote>.Update.Set("Margins.Product", request.Margins.Product).
                                           Set("Margins.CreateUser", request.Margins.CreateUser).
                                           Set("Margins.CreateDate", DateTime.Now).
                                           Set("Margins.SelectedMargin", request.Margins.SelectedMargin).
                                           Set("CurrentPipeline", "Quote Pipeline").
                                           Set("CurrentPipelineStep", "Quote").Set("CurrentPipelineSubStep", "Margins"));
                return resultFlag.MatchedCount > 0 ? "1" : "Margin Details not updated.";
            }
        }

        public string SetItemwise(mQuote quote, Margins margins, MarginSetReq request)
        {
            UpdateResult resultFlag;
            if (margins.Itemwise != null && margins.Itemwise.ItemProperties != null && margins.Itemwise.ItemProperties.Count > 0)
            {
                request.Margins.Itemwise.ItemProperties.ForEach(r =>
                {
                    r.ItemID = string.IsNullOrEmpty(r.ItemID) ? ObjectId.GenerateNewId().ToString() : r.ItemID;
                });

                quote = _MongoContext.mQuote.FindOneAndUpdate(Builders<mQuote>.Filter.Eq("QRFID", request.QRFID),
                                            Builders<mQuote>.Update.Set("Margins.Itemwise", request.Margins.Itemwise).
                                            Set("Margins.EditUser", request.Margins.EditUser).
                                            Set("Margins.EditDate", DateTime.Now).
                                            Set("Margins.SelectedMargin", request.Margins.SelectedMargin).
                                            Set("CurrentPipeline", "Quote Pipeline").
                                            Set("CurrentPipelineStep", "Quote").Set("CurrentPipelineSubStep", "Margins"));
                return quote != null ? "1" : "Margin Details not updated.";
            }
            else
            {
                request.Margins.Itemwise.ItemProperties.ForEach(r =>
                {
                    r.ItemID = ObjectId.GenerateNewId().ToString();
                });
                resultFlag = _MongoContext.mQuote.UpdateOne(Builders<mQuote>.Filter.Eq("QRFID", request.QRFID),
                                           Builders<mQuote>.Update.Set("Margins.Itemwise", request.Margins.Itemwise).
                                           Set("Margins.CreateUser", request.Margins.CreateUser).
                                           Set("Margins.CreateDate", DateTime.Now).
                                           Set("Margins.SelectedMargin", request.Margins.SelectedMargin).
                                           Set("CurrentPipeline", "Quote Pipeline").
                                           Set("CurrentPipelineStep", "Quote").Set("CurrentPipelineSubStep", "Margins"));
                return resultFlag.MatchedCount > 0 ? "1" : "Margin Details not updated.";
            }
        }

        public List<Currency> GetActiveCurrencyList(CurrencyGetReq request)
        {
            List<Currency> response = new List<Currency>();
            if (string.IsNullOrEmpty(request.CurrencyUnit))
            {
                response = (from c in _MongoContext.mCurrency.AsQueryable()
                                // where  c.Status=="1"
                            select new Currency { CurrencyCode = c.Currency, CurrencyName = c.Name, SubUnit = c.SubUnit }).ToList();
            }
            else
            {
                response = (from c in _MongoContext.mCurrency.AsQueryable()
                            where c.Currency.ToLower().Contains(request.CurrencyUnit) //&& c.Status=="1"
                            select new Currency { CurrencyCode = c.Currency, CurrencyName = c.Name, SubUnit = c.SubUnit }).ToList();
            }
            return response;
        }

        public List<ProductProperties> GetProductTypeDetails(MarginGetReq request)
        {
            List<ProductProperties> result = new List<ProductProperties>();

            var prodresult = new List<ProductProperties>();
            if (request.IsCostingMargin)
            {
                var prodresultCosting = _MongoContext.mQRFPrice.AsQueryable().Where(r => r.QRFID == request.QRFID && r.IsCurrentVersion == true).OrderByDescending(b => b.VersionId).Select(r => r.QRFMargin.Product != null ? r.QRFMargin.Product.ProductProperties : null).FirstOrDefault();
                foreach (var pp in prodresultCosting)
                {
                    prodresult.Add(new ProductProperties
                    {
                        VoyagerProductType_Id = pp.VoyagerProductType_Id,
                        Prodtype = pp.Prodtype,
                        ProductID = pp.ProductID,
                        SellingPrice = pp.SellingPrice,
                        MarginUnit = pp.MarginUnit,
                        HowMany = pp.HowMany
                    });
                }
            }
            else
            {
                prodresult = _MongoContext.mQuote.AsQueryable().Where(r => r.QRFID == request.QRFID).Select(r => r.Margins.Product != null ? r.Margins.Product.ProductProperties : null).FirstOrDefault();
            }

            if (prodresult != null && prodresult.Count > 0)
            {
                result = (from p in _MongoContext.mProductType.AsQueryable().ToList()
                          join m in prodresult.ToList() on p.VoyagerProductType_Id equals m.VoyagerProductType_Id into ps
                          from pm in ps.DefaultIfEmpty()
                          select new ProductProperties
                          {
                              Prodtype = p.Prodtype,
                              HowMany = (pm != null) ? pm.HowMany : "",
                              ProductID = (pm != null) ? pm.ProductID : "",
                              VoyagerProductType_Id = p.VoyagerProductType_Id,
                              MarginUnit = (pm != null) ? pm.MarginUnit : "",
                              SellingPrice = (pm != null) ? pm.SellingPrice : 0
                          }).ToList();
            }

            if (prodresult == null || prodresult.Count == 0)
            {
                result = _MongoContext.mProductType.AsQueryable().Where(p => p.VoyagerProductType_Id != "").Select(p => new ProductProperties
                {
                    Prodtype = p.Prodtype,
                    VoyagerProductType_Id = p.VoyagerProductType_Id,
                }).ToList();
            }

            foreach (var prod in result)
            {
                if (prod.Prodtype.ToLower() == "hotel" || prod.Prodtype.ToLower() == "overnight ferry")
                {
                    prod.ProductMaster = "1Hotel";
                }
                else if (prod.Prodtype.ToLower() == "ldc" || prod.Prodtype.ToLower() == "coach" || prod.Prodtype.ToLower() == "private transfer" || prod.Prodtype.ToLower() == "scheduled transfer"
                    || prod.Prodtype.ToLower() == "ferry passenger" || prod.Prodtype.ToLower() == "ferry transfer" || prod.Prodtype.ToLower() == "train")
                {
                    prod.ProductMaster = "2Transportation";
                }
                else if (prod.Prodtype.ToLower() == "attractions" || prod.Prodtype.ToLower() == "sightseeing - citytour")
                {
                    prod.ProductMaster = "3Activities";
                }
                else if (prod.Prodtype.ToLower() == "meal")
                {
                    prod.ProductMaster = "4Meals";
                }
                else
                {
                    prod.ProductMaster = "5Others";
                }
            }
            result = result.OrderBy(a => a.ProductMaster).ToList();
            result.ForEach(a => a.ProductMaster = a.ProductMaster.Replace("1Hotel", "Hotel").Replace("2Transportation", "Transportation").Replace("3Activities", "Activities").Replace("4Meals", "Meals").Replace("5Others", "Others"));

            return result ?? new List<ProductProperties>();
        }

        public List<ItemProperties> GetItemwiseDetails(MarginGetReq request)
        {
            List<ItemProperties> result = new List<ItemProperties>();
            try
            {
                var MarginResult = new List<ItemProperties>();
                var PositionResult = new List<mPosition>();

                if (request.IsCostingMargin)
                {
                    var MarginResultCosting = _MongoContext.mQRFPrice.AsQueryable().Where(r => r.QRFID == request.QRFID && r.IsCurrentVersion == true).OrderByDescending(b => b.VersionId).Select(r => r.QRFMargin.Item != null ? r.QRFMargin.Item.ItemProperties : null).FirstOrDefault();
                    foreach (var pp in MarginResultCosting)
                    {
                        MarginResult.Add(new ItemProperties
                        {
                            ItemID = pp.ItemID,
                            PositionID = pp.PositionID,
                            ProductName = pp.ProductName,
                            VoyagerProductType_Id = pp.VoyagerProductType_Id,
                            Prodtype = pp.Prodtype,
                            SellingPrice = pp.SellingPrice,
                            MarginUnit = pp.MarginUnit,
                            HowMany = pp.HowMany
                        });
                    }

                    var PositionResultCosting = _MongoContext.mQRFPosition.AsQueryable().Where(r => r.QRFID == request.QRFID && r.IsDeleted == false).OrderBy(a => a.ProductType).ThenBy(b => b.DayNo).ToList();

                    if (PositionResultCosting != null && PositionResultCosting.Count > 0)
                    {
                        foreach (var pos in PositionResultCosting)
                        {
                            var qrfPos = new mPosition();

                            qrfPos._Id = pos._Id;
                            qrfPos.QRFID = pos.QRFID;
                            qrfPos.PositionId = pos.PositionId;
                            qrfPos.PositionSequence = pos.PositionSequence;
                            qrfPos.ProductType = pos.ProductType;
                            qrfPos.ProductTypeId = pos.ProductTypeId;
                            qrfPos.DayNo = pos.DayNo;
                            qrfPos.RoutingDaysID = pos.RoutingDaysID;
                            qrfPos.StartingFrom = pos.StartingFrom;
                            qrfPos.ProductAttributeType = pos.ProductAttributeType;
                            qrfPos.CountryName = pos.CountryName;
                            qrfPos.CityName = pos.CityName;
                            qrfPos.CityID = pos.CityID;
                            qrfPos.ProductName = pos.ProductName;
                            qrfPos.ProductID = pos.ProductID;
                            qrfPos.BudgetCategoryId = pos.BudgetCategoryId;
                            qrfPos.BudgetCategory = pos.BudgetCategory;
                            qrfPos.SupplierId = pos.SupplierId;
                            qrfPos.SupplierName = pos.SupplierName;
                            qrfPos.StartTime = pos.StartTime;
                            qrfPos.EndTime = pos.EndTime;
                            qrfPos.Duration = Convert.ToInt16(pos.Duration);
                            qrfPos.KeepAs = pos.KeepAs;
                            qrfPos.TLRemarks = pos.TLRemarks;
                            qrfPos.OPSRemarks = pos.OPSRemarks;
                            qrfPos.Status = pos.Status;
                            qrfPos.CreateUser = pos.CreateUser;
                            qrfPos.CreateDate = pos.CreateDate;
                            qrfPos.EditUser = pos.EditUser;
                            qrfPos.EditDate = pos.EditDate;
                            qrfPos.IsDeleted = pos.IsDeleted;

                            qrfPos.StarRating = pos.StarRating;
                            qrfPos.Location = pos.Location;
                            qrfPos.ChainName = pos.ChainName;
                            qrfPos.ChainID = pos.ChainID;
                            qrfPos.MealPlan = pos.MealPlan;
                            qrfPos.EarlyCheckInDate = pos.EarlyCheckInDate;
                            qrfPos.EarlyCheckInTime = pos.EarlyCheckInTime;
                            qrfPos.InterConnectingRooms = pos.InterConnectingRooms;
                            qrfPos.WashChangeRooms = pos.WashChangeRooms;
                            qrfPos.LateCheckOutDate = pos.LateCheckOutDate;
                            qrfPos.LateCheckOutTime = pos.LateCheckOutTime;
                            qrfPos.DeletedFrom = pos.DeletedFrom;
                            qrfPos.TypeOfExcursion = pos.TypeOfExcursion;
                            qrfPos.TypeOfExcursion_Id = pos.TypeOfExcursion_Id;
                            qrfPos.NoOfPaxAdult = pos.NoOfPaxAdult;
                            qrfPos.NoOfPaxChild = pos.NoOfPaxChild;
                            qrfPos.NoOfPaxInfant = pos.NoOfPaxInfant;
                            qrfPos.MealType = pos.MealType;
                            qrfPos.TransferDetails = pos.TransferDetails;
                            qrfPos.ApplyAcrossDays = pos.ApplyAcrossDays;
                            qrfPos.FromPickUpLoc = pos.FromPickUpLoc;
                            qrfPos.FromPickUpLocID = pos.FromPickUpLocID;

                            qrfPos.ToCityName = pos.ToCityName;
                            qrfPos.ToCityID = pos.ToCityID;
                            qrfPos.ToCountryName = pos.ToCountryName;
                            qrfPos.ToDropOffLoc = pos.ToDropOffLoc;
                            qrfPos.ToDropOffLocID = pos.ToDropOffLocID;
                            qrfPos.BuyCurrencyId = pos.BuyCurrencyId;
                            qrfPos.BuyCurrency = pos.BuyCurrency;
                            qrfPos.StandardPrice = pos.StandardPrice;
                            qrfPos.StandardFOC = pos.StandardFOC;

                            foreach (var room in pos.RoomDetailsInfo)
                            {
                                if (!room.IsDeleted)
                                {
                                    var newRoom = new RoomDetailsInfo();

                                    newRoom.RoomId = room.RoomId;
                                    newRoom.RoomSequence = room.RoomSequence;
                                    newRoom.ProductCategoryId = room.ProductCategoryId;
                                    newRoom.ProductRangeId = room.ProductRangeId;
                                    newRoom.ProductCategory = room.ProductCategory;
                                    newRoom.ProductRange = room.ProductRange;
                                    newRoom.IsSupplement = room.IsSupplement;
                                    newRoom.CreateUser = room.CreateUser;
                                    newRoom.CreateDate = room.CreateDate;
                                    newRoom.EditUser = room.EditUser;
                                    newRoom.EditDate = room.EditDate;
                                    newRoom.IsDeleted = room.IsDeleted;

                                    qrfPos.RoomDetailsInfo.Add(newRoom);
                                }
                            }
                            PositionResult.Add(qrfPos);
                        }
                    }
                }
                else
                {
                    MarginResult = _MongoContext.mQuote.AsQueryable().Where(r => r.QRFID == request.QRFID).Select(r => r.Margins.Itemwise != null ? r.Margins.Itemwise.ItemProperties : null).FirstOrDefault();
                    PositionResult = _MongoContext.mPosition.AsQueryable().Where(r => r.QRFID == request.QRFID && r.IsDeleted == false).OrderBy(a => a.ProductType).ThenBy(b => b.DayNo).ToList();
                }

                if (MarginResult != null && MarginResult.Count > 0)
                {
                    #region commented
                    //result = _MongoContext.mPosition.AsQueryable().Where(p => p.QRFID == QRFID && p.IsDeleted == false)
                    //    .Join(prodresult, aPos => aPos.PositionId, bItem => bItem.PositionID, (Position, Itemwise) => new ItemProperties
                    //    {
                    //        PositionID = Position.PositionId,
                    //        ProductName = Position.ProductName + " ( " + Position.StartingFrom + " - " + Position.CityName + " )",
                    //        Prodtype = Position.ProductType,
                    //        VoyagerProductType_Id = Position.ProductTypeId,

                    //        HowMany = (Itemwise != null) ? Itemwise.HowMany : "",
                    //        ItemID = (Itemwise != null) ? Itemwise.ItemID : "",
                    //        MarginUnit = (Itemwise != null) ? Itemwise.MarginUnit : "",
                    //        SellingPrice = (Itemwise != null && Itemwise.SellingPrice != null) ? Itemwise.SellingPrice : null
                    //    }).ToList();
                    #endregion

                    result = (from p in PositionResult
                              join m in MarginResult on p.PositionId equals m.PositionID into ps
                              from pm in ps.DefaultIfEmpty()
                              select new ItemProperties
                              {
                                  PositionID = p.PositionId,
                                  ProductName = p.ProductName + " ( " + p.StartingFrom + " - " + p.CityName + " )",
                                  Prodtype = p.ProductType,
                                  VoyagerProductType_Id = p.ProductTypeId,
                                  HowMany = (pm != null) ? pm.HowMany : "",
                                  ItemID = (pm != null) ? pm.ItemID : "",
                                  MarginUnit = (pm != null) ? pm.MarginUnit : "",
                                  SellingPrice = (pm != null) ? pm.SellingPrice : 0
                              }).ToList();
                }

                if (MarginResult == null || MarginResult.Count == 0)
                {
                    result = PositionResult.Select(p => new ItemProperties
                    {
                        PositionID = p.PositionId,
                        ProductName = p.ProductName + " ( " + p.StartingFrom + " - " + p.CityName + " )",
                        Prodtype = p.ProductType,
                        VoyagerProductType_Id = p.ProductTypeId,
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return result ?? new List<ItemProperties>();
        }

        #endregion

        #region QRF FOC
        public FOCGetResponse GetFOCDetailsForQRF_Id(PaxGetRequest req)
        {
            var response = new FOCGetResponse();
            try
            {
                var result = _MongoContext.mQuote.AsQueryable().Where(m => m.QRFID == req.QRFID).FirstOrDefault();
                if (result != null)
                {
                    List<FOCDetails> listNewFOC = new List<FOCDetails>
                        { new FOCDetails { DateRange = "ALL", PaxSlab = "ALL", FOCSingle = 0, FOCTwin = 0, FOCDouble = 0, FOCTriple = 0 }};
                    List<FOCDetails> listNewFOC1 = new List<FOCDetails>
                        { new FOCDetails { DateRange = "ALL", PaxSlab = "ALL", FOCSingle = 0, FOCTwin = 0, FOCDouble = 0, FOCTriple = 0 }};

                    var FOCdbRows = result.FOCDetails.Where(x => x.IsDeleted == false).ToList();
                    string DateRange, PaxRange;

                    foreach (var departure in result.Departures.Where(d => d.IsDeleted == false).OrderBy(e => e.Date))
                    {
                        foreach (var paxslab in result.PaxSlabDetails.PaxSlabs.Where(p => p.IsDeleted == false))
                        {
                            DateRange = Convert.ToDateTime(departure.Date).ToString("dd-MMM-yy");
                            PaxRange = paxslab.From.ToString() + "-" + paxslab.To.ToString();

                            if (!FOCdbRows.Exists(f => f.DateRange == DateRange && f.PaxSlab == PaxRange))
                            {
                                listNewFOC.Add(new FOCDetails
                                {
                                    DateRange = Convert.ToDateTime(departure.Date).ToString("dd-MMM-yy"),
                                    PaxSlab = paxslab.From.ToString() + "-" + paxslab.To.ToString(),
                                    DivideByCost = paxslab.DivideByCost,
                                    DateRangeId = departure.Departure_Id,
                                    PaxSlabId = paxslab.PaxSlab_Id,
                                    FOCSingle = 0,
                                    FOCTwin = 0,
                                    FOCDouble = 0,
                                    FOCTriple = 0
                                });
                            }
                            else
                            {
                                listNewFOC1.Add(new FOCDetails
                                {
                                    DateRange = Convert.ToDateTime(departure.Date).ToString("dd-MMM-yy"),
                                    PaxSlab = paxslab.From.ToString() + "-" + paxslab.To.ToString(),
                                    DivideByCost = paxslab.DivideByCost,
                                    DateRangeId = departure.Departure_Id,
                                    PaxSlabId = paxslab.PaxSlab_Id,
                                    FOCSingle = 0,
                                    FOCTwin = 0,
                                    FOCDouble = 0,
                                    FOCTriple = 0
                                });
                            }
                        }
                    }

                    if (result.FOCDetails != null && result.FOCDetails.Where(m => m.IsDeleted == false).Count() > 0)
                    {
                        if (result.StandardFOC && FOCdbRows != null && FOCdbRows.Count > 0)
                        {
                            listNewFOC[0].FOCSingle = FOCdbRows.FirstOrDefault().FOCSingle;
                            listNewFOC[0].FOCTwin = FOCdbRows.FirstOrDefault().FOCTwin;
                            listNewFOC[0].FOCDouble = FOCdbRows.FirstOrDefault().FOCDouble;
                            listNewFOC[0].FOCTriple = FOCdbRows.FirstOrDefault().FOCTriple;
                        }
                        response.StandardFOC = result.StandardFOC;
                        response.FOCDetails = listNewFOC.Where(a => a.DateRange == "ALL").ToList();

                        FOCdbRows.FindAll(c => !listNewFOC1.Exists(b => c.PaxSlab == c.PaxSlab && c.DateRange == b.DateRange)).ToList().ForEach(a => a.IsDeleted = true);
                        FOCdbRows = FOCdbRows.Where(a => a.IsDeleted == false).ToList();

                        response.FOCDetails.AddRange(FOCdbRows);
                        response.FOCDetails.AddRange(listNewFOC.Where(a => a.DateRange != "ALL").ToList());
                        //var result1 = productRangeList.Where(a => RangeIdList.Any(b => b == a.VoyagerProductRange_Id)).ToList();
                        response.QRFID = req.QRFID;
                        response.Status = "Success";
                    }
                    else
                    {
                        response.StandardFOC = true;
                        response.FOCDetails = listNewFOC;
                        response.QRFID = req.QRFID;
                        response.Status = "Success";
                    }
                }
                else
                {
                    response.Status = "Invalid QRF";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.Status = "Exception Occured";
            }
            return response;
        }

        public async Task<PaxSetResponse> SetFOCDetailsForQRF_Id(FOCSetRequest req)
        {
            var response = new PaxSetResponse();
            QRFCounterRequest qrfCounterRequest = new QRFCounterRequest { CounterType = _configuration["CounterType:QRFFOC"].ToString() };
            var result = _MongoContext.mQuote.AsQueryable().Where(a => a.QRFID == req.QRFID).ToList();
            if (result != null && result.Count > 0)
            {
                List<FOCDetails> lstFOC = result.Select(r => r.FOCDetails).FirstOrDefault();
                if (lstFOC != null)//if exists or not then update as whole Routing Info List
                {
                    req.FOCDetails.AddRange(lstFOC.Where(x => x.IsDeleted == true));
                    if (req.FOCDetails != null && req.FOCDetails.Count > 0)
                    {
                        req.FOCDetails.FindAll(f => !lstFOC.Exists(r => r.FOC_Id == f.FOC_Id)).ForEach
                       (r =>
                       {
                           r.FOC_Id = _genericRepository.GetNextReferenceNumber(qrfCounterRequest).LastReferenceNumber;
                           r.IsDeleted = r.IsDeleted;
                           r.CreateDate = DateTime.Now;
                           r.CreateUser = r.CreateUser;
                           r.EditUser = null;
                           r.EditDate = null;
                       });

                        req.FOCDetails.FindAll(f => lstFOC.Exists(r => r.FOC_Id == f.FOC_Id)).ForEach
                           (r =>
                           {
                               r.EditDate = DateTime.Now;
                               r.EditUser = r.EditUser;
                               r.IsDeleted = r.IsDeleted;
                               r.CreateUser = lstFOC.Where(l => l.FOC_Id == r.FOC_Id).Select(l => l.CreateUser).FirstOrDefault();
                               r.CreateDate = lstFOC.Where(l => l.FOC_Id == r.FOC_Id).Select(l => l.CreateDate).FirstOrDefault();
                           });

                        var res = await _MongoContext.mQuote.FindOneAndUpdateAsync(Builders<mQuote>.Filter.Eq("QRFID", req.QRFID),
                               Builders<mQuote>.Update.Set("StandardFOC", req.StandardFOC).Set("FOCDetails", req.FOCDetails).
                               Set("CurrentPipeline", "Quote Pipeline").Set("CurrentPipelineStep", "Quote").Set("CurrentPipelineSubStep", "SalesFOC"));

                        response.Status = "Success";
                    }
                    else
                    {
                        response.Status = "No FOCDetails found";
                    }
                }
                else//insert code at 1st time
                {
                    req.FOCDetails.RemoveAll(f => f.IsDeleted == true);
                    if (req.FOCDetails != null && req.FOCDetails.Count > 0)
                    {
                        req.FOCDetails.ForEach(r =>
                        {
                            r.FOC_Id = _genericRepository.GetNextReferenceNumber(qrfCounterRequest).LastReferenceNumber;
                            r.IsDeleted = r.IsDeleted;
                            r.CreateDate = DateTime.Now;
                            r.CreateUser = r.CreateUser;
                            r.EditUser = null;
                            r.EditDate = null;
                        });

                        var resultFlag = await _MongoContext.mQuote.UpdateOneAsync(Builders<mQuote>.Filter.Eq("QRFID", req.QRFID),
                              Builders<mQuote>.Update.Set("FOCDetails", req.FOCDetails).Set("CurrentPipeline", "Quote Pipeline").
                              Set("CurrentPipelineStep", "Quote").Set("CurrentPipelineSubStep", "SalesFOC"));
                        response.Status = "Success";
                    }
                    else
                    {
                        response.Status = "No FOCDetails record to insert";
                    }
                }
            }
            else
            {
                response.Status = "QRF doesn't Exist";
            }

            return response;
        }
        #endregion

        #region TourEntities
        ///below function will work as
        ///1)if Tour Entity is unchecked from UI then existing Tour Entity details mark as IsDeleted to true in mQuote collection MongoDB
        ///2)if TE details already exists then update in mQuote collection
        ///3)if Static TE details not exists then insert into mPosition collection as ProductType=Assistant and IsTourEntity=true and fetch the ProductRange details CityName = "Dubai", ProductName = Static TE 
        ///and insert into mPosition colletion after that it takes already existing Assistant TE positions and update its ProductRanges into mPosition collection.
        ///If Newly selected TE then insert into mQuote->TourEntity collection
        ///4)If staying Room service in dropdown is selected (for static or dynamic TE) then insert/update Rooming Details and Price into Accomodation,Meals positions
        ///HowMany and Duration of Tour Entity should be check for Hotel Positions only
        public async Task<TourEntitiesSetRes> SetTourEntities(TourEntitiesSetReq request)
        {
            TourEntitiesSetRes response = new TourEntitiesSetRes() { QRFID = request.QRFID };
            response.ResponseStatus = new ResponseStatus();
            UpdateResult resultFlag;
            try
            {
                var quote = await _MongoContext.mQuote.FindAsync(a => a.QRFID == request.QRFID).Result.FirstOrDefaultAsync();
                if (quote != null)
                {
                    if (request.TourEntities != null && request.TourEntities.Count > 0)
                    {
                        mQuote objmQuote = new mQuote();
                        List<mPosition> lstPosition = new List<mPosition>();

                        List<string> lstStr = new List<string>() { "Assistant", "Meal", "Hotel" };
                        var positions = _MongoContext.mPosition.AsQueryable().Where(a => a.QRFID == request.QRFID && lstStr.Contains(a.ProductType) && a.IsDeleted == false).ToList();
                        positions.RemoveAll(a => a.IsTourEntity == false && a.ProductType.ToLower() == "assistant");

                        var dynamicres = request.TourEntities.Where(a => a.Flag == "dynamic").ToList();

                        #region if Tour Entity is unchecked from UI then existing Tour Entity details mark as IsDeleted to true in mQuote collection MongoDB
                        var tourentities = quote.TourEntities.Where(a => a.IsDeleted == false).ToList();
                        var tourlist = tourentities.FindAll(a => !request.TourEntities.Exists(b => b.Type.Contains(a.Type)));
                        if (tourlist != null && tourlist.Count > 0)
                        {
                            tourlist.ForEach(a =>
                            {
                                a.EditDate = DateTime.Now; a.EditUser = request.UserName; a.CreateUser = tourentities.Where(b => b.TourEntityID == a.TourEntityID).FirstOrDefault().CreateUser;
                                a.IsDeleted = true;
                                a.CreateDate = tourentities.Where(b => b.TourEntityID == a.TourEntityID).FirstOrDefault().CreateDate;
                                a.PositionID = tourentities.Where(b => b.TourEntityID == a.TourEntityID).FirstOrDefault().PositionID;
                            });

                            foreach (var item in tourlist)
                            {
                                item.Flag = null;
                                objmQuote = await _MongoContext.mQuote.FindOneAndUpdateAsync(m => m.QRFID == request.QRFID && m.TourEntities.Any(a => a.TourEntityID == item.TourEntityID),
                                       Builders<mQuote>.Update.Set(m => m.TourEntities[-1], item));
                            }
                        }
                        #endregion

                        #region if TE details already exists then update 
                        List<TourEntities> lstTEExists = request.TourEntities.FindAll(b => b.TourEntityID != "0" && !string.IsNullOrEmpty(b.TourEntityID));
                        if (lstTEExists != null && lstTEExists.Count > 0)
                        {
                            lstTEExists.ForEach(a =>
                            {
                                a.EditDate = DateTime.Now; a.EditUser = request.UserName; a.CreateUser = tourentities.Where(b => b.TourEntityID == a.TourEntityID).FirstOrDefault().CreateUser;
                                a.CreateDate = tourentities.Where(b => b.TourEntityID == a.TourEntityID).FirstOrDefault().CreateDate;
                                a.PositionID = tourentities.Where(b => b.TourEntityID == a.TourEntityID).FirstOrDefault().PositionID;
                                a.Flag = null;
                            });

                            foreach (var item in lstTEExists)
                            {
                                objmQuote = await _MongoContext.mQuote.FindOneAndUpdateAsync(m => m.QRFID == request.QRFID && m.TourEntities.Any(a => a.TourEntityID == item.TourEntityID),
                                       Builders<mQuote>.Update.Set(m => m.TourEntities[-1], item));
                            }

                            response.ResponseStatus.Status = objmQuote != null ? "Success" : "Failure";
                            response.ResponseStatus.ErrorMessage = objmQuote != null ? "Tour Entity details saved successfully." : "Tour Entities not updated";
                        }
                        #endregion

                        #region if Static TE details not exists then insert into mPosition collection & Static ,Dynamic TE into TourEntities of mQuote collection
                        List<TourEntities> lstTENotExists = request.TourEntities.FindAll(a => a.TourEntityID == "0" || string.IsNullOrEmpty(a.TourEntityID));
                        if (lstTENotExists != null && lstTENotExists.Count > 0)
                        {
                            //1)insert non product type into mPosition
                            var posres = positions.Where(a => a.ProductType == "Assistant" && a.IsTourEntity).ToList();
                            List<TourEntities> lstStaticTE = request.TourEntities.FindAll(a => posres != null && (posres.Count > 0 ? !posres.Exists(b => a.Type == b.ProductName) : true) && !string.IsNullOrEmpty(a.Flag) && a.Flag == "static");

                            if (lstStaticTE != null && lstStaticTE.Count > 0)
                            {
                                List<string> lstType = lstStaticTE.Where(a => !string.IsNullOrEmpty(a.Flag) && a.Flag == "static").Select(a => a.Type).Distinct().ToList();
                                var chkPos = _MongoContext.mPosition.AsQueryable().Where(a => lstType.Contains(a.ProductName) && a.QRFID == request.QRFID).ToList();
                                lstType = chkPos != null && chkPos.Count > 0 ? lstType.FindAll(a => !chkPos.Select(c => c.ProductName).ToList().Exists(b => a == b)) : lstType;

                                if (lstType != null && lstType.Count > 0)
                                {
                                    List<mProducts> lstProducts = _productRepository.GetProductsByNames(new ProductGetReq { ProductName = lstType, CityName = "Dubai" });
                                    List<ProductRangeDetails> lstProductRange = _productRepository.GetProductRangeByParam(new ProductRangeGetReq { ProductIdList = lstProducts.Select(a => a.VoyagerProduct_Id).ToList() }).ProductRangeDetails;
                                    List<ProdCategoryDetails> lstProdCategoryDetails = _productRepository.GetProductCategoryByParam(new ProductCatGetReq { ProductIdList = lstProducts.Select(a => a.VoyagerProduct_Id).ToList() });
                                    List<SupplierData> lstSupplierList = _productRepository.GetSupplierDetails(new ProductSupplierGetReq { ProductIdList = lstProducts.Select(a => a.VoyagerProduct_Id).ToList() }).SupllierList;
                                    var prodcat = new ProdCategoryDetails();

                                    var curids = lstSupplierList.Select(a => a.CurrencyId).ToList();
                                    var currencylist = _MongoContext.mCurrency.AsQueryable().Where(c => curids.Contains(c.VoyagerCurrency_Id)).ToList();

                                    if (lstProducts != null && lstProducts.Count > 0 &&
                                         //lstProductRange != null && lstProductRange.Count > 0 &&
                                         lstProdCategoryDetails != null && lstProdCategoryDetails.Count > 0 &&
                                        lstSupplierList != null && lstSupplierList.Count > 0)
                                    {
                                        mPosition objPosition = new mPosition();
                                        var res = new mProducts();
                                        var resProductRange = new List<ProductRangeDetails>();
                                        var RoomDetailsInfo = new List<RoomDetailsInfo>();
                                        int i = 1;
                                        var newposid = "";
                                        var suppdata = new SupplierData();
                                        var curdata = new mCurrency();
                                        var routingDays = new RoutingDays();

                                        foreach (var m in lstType)
                                        {
                                            i = 1;
                                            res = lstProducts.Where(a => a.ProdName == m).FirstOrDefault();
                                            resProductRange = res != null && lstProductRange != null ? lstProductRange.Where(a => a.ProductId == res.VoyagerProduct_Id).ToList() : new List<ProductRangeDetails>();
                                            suppdata = lstSupplierList.Where(a => a.ProdId == res.VoyagerProduct_Id).FirstOrDefault();
                                            curdata = currencylist.Where(a => a.VoyagerCurrency_Id == suppdata.CurrencyId).FirstOrDefault();

                                            if (res != null
                                                //&& resProductRange != null && resProductRange.Count > 0 
                                                && suppdata != null && curdata != null)
                                            {
                                                newposid = Guid.NewGuid().ToString();

                                                RoomDetailsInfo = resProductRange.Select(a => new RoomDetailsInfo
                                                {
                                                    CreateDate = DateTime.Now,
                                                    CreateUser = "",
                                                    IsSupplement = (a.AdditionalYN == null || a.AdditionalYN == false) ? false : true,
                                                    ProdDesc = a.ProductMenu,
                                                    ProductCategory = lstProdCategoryDetails.Where(b => b.ProductCategoryId == a.ProductCategoryId).FirstOrDefault() != null ?
                                                                      lstProdCategoryDetails.Where(b => b.ProductCategoryId == a.ProductCategoryId).FirstOrDefault().ProductCategoryName : "",
                                                    ProductCategoryId = a.ProductCategoryId,
                                                    ProductRange = a.ProductRangeName + " (" + a.PersonType + ")",
                                                    ProductRangeId = a.VoyagerProductRange_Id,
                                                    RoomId = Guid.NewGuid().ToString(),
                                                    RoomSequence = i++,
                                                    CrossPositionId = newposid
                                                }).ToList();

                                                prodcat = lstProdCategoryDetails.Where(a => a.ProductId == res.VoyagerProduct_Id).FirstOrDefault();
                                                if (prodcat != null)
                                                {
                                                    objPosition = new mPosition();
                                                    if (RoomDetailsInfo != null && RoomDetailsInfo.Count > 0)
                                                    {
                                                        objPosition.RoomDetailsInfo = RoomDetailsInfo.OrderBy(a => a.IsSupplement).ThenBy(a => a.ProductRange).ToList();
                                                    }
                                                    objPosition.CreateDate = DateTime.Now; objPosition.EditDate = null; objPosition.EditUser = "";
                                                    objPosition.PositionId = newposid;
                                                    objPosition.KeepAs = "Included";
                                                    objPosition.Duration = Convert.ToInt32(quote.AgentProductInfo.Duration) + 1;
                                                    objPosition.ProductType = res.ProductType;
                                                    objPosition.ProductName = m;
                                                    objPosition.CityID = res.Resort_Id;
                                                    objPosition.CityName = res.CityName;
                                                    objPosition.CountryName = res.CountryName;
                                                    objPosition.DayNo = 1;
                                                    routingDays = quote.RoutingDays.Where(a => a.DayNo == 1 && a.IsDeleted == false).FirstOrDefault();
                                                    objPosition.RoutingDaysID = routingDays != null ? routingDays.RoutingDaysID : "";
                                                    objPosition.PositionSequence = 1;
                                                    objPosition.ProductID = res.VoyagerProduct_Id;
                                                    objPosition.ProductTypeId = res.ProductType_Id;
                                                    objPosition.QRFID = request.QRFID;
                                                    objPosition.StartingFrom = "Day 1";
                                                    objPosition.Status = "Q";
                                                    objPosition.StandardPrice = true;
                                                    objPosition.BudgetCategory = prodcat.ProductCategoryName;
                                                    objPosition.BudgetCategoryId = prodcat.ProductCategoryId;
                                                    objPosition.SupplierId = suppdata.SupplierId;
                                                    objPosition.SupplierName = suppdata.SupplierName;
                                                    objPosition.BuyCurrencyId = suppdata.CurrencyId;
                                                    objPosition.BuyCurrency = curdata.Currency;
                                                    objPosition.StartTime = "07:00";
                                                    objPosition.EndTime = "19:00";
                                                    objPosition.IsTourEntity = true;
                                                    lstPosition.Add(objPosition);
                                                }
                                            }
                                        }

                                        if (lstPosition != null && lstPosition.Count > 0)
                                        {
                                            await _MongoContext.mPosition.InsertManyAsync(lstPosition);
                                        }
                                    }
                                }
                                lstPosition.AddRange(chkPos);
                            }

                            //if TE details not exists then insert into mQuote->TourEntities collection
                            lstTENotExists.ForEach(a =>
                            {
                                a.TourEntityID = Guid.NewGuid().ToString(); a.CreateDate = DateTime.Now; a.CreateUser = request.UserName; a.EditUser = ""; a.EditDate = null;
                                a.PositionID = string.IsNullOrEmpty(a.PositionID) && lstPosition != null && lstPosition.Count > 0 ? lstPosition.Where(b => b.ProductName == a.Type).FirstOrDefault().PositionId : a.PositionID;
                            });

                            request.TourEntities.ForEach(a =>
                            a.PositionID = string.IsNullOrEmpty(a.PositionID) && lstPosition != null && lstPosition.Count > 0 ? lstPosition.Where(b => b.ProductName == a.Type).FirstOrDefault().PositionId : a.PositionID);

                            resultFlag = await _MongoContext.mQuote.UpdateOneAsync(Builders<mQuote>.Filter.Eq("QRFID", request.QRFID),
                                Builders<mQuote>.Update.PushEach<TourEntities>("TourEntities", lstTENotExists));

                            response.ResponseStatus.Status = resultFlag.ModifiedCount > 0 ? "Success" : "Failure";
                            response.ResponseStatus.ErrorMessage = resultFlag.ModifiedCount > 0 ? "Tour Entity details saved successfully." : "Tour Entities not saved";
                        }
                        //below code is for updating RoomDetailsInfo of Static TE
                        List<TourEntities> lstTEExistsUpd = request.TourEntities.FindAll(a => a.TourEntityID != "0" && !string.IsNullOrEmpty(a.TourEntityID) && (a.Flag == "static" || string.IsNullOrEmpty(a.Flag)));
                        if (lstTEExistsUpd != null && lstTEExistsUpd.Count > 0)
                        {
                            var posid = lstTEExistsUpd.Select(a => a.PositionID).ToList();
                            var chkPos = _MongoContext.mPosition.AsQueryable().Where(a => posid.Contains(a.PositionId) && a.QRFID == request.QRFID && a.ProductType.ToLower() == "assistant" && a.IsTourEntity).ToList();

                            if (chkPos != null && chkPos.Count > 0)
                            {
                                List<ProductRangeDetails> lstProductRange = _productRepository.GetProductRangeByParam(new ProductRangeGetReq { ProductIdList = chkPos.Select(a => a.ProductID).ToList() }).ProductRangeDetails;
                                List<ProdCategoryDetails> lstProdCategoryDetails = _productRepository.GetProductCategoryByParam(new ProductCatGetReq { ProductIdList = chkPos.Select(a => a.ProductID).ToList() });

                                if (lstProductRange != null && lstProductRange.Count > 0 && lstProdCategoryDetails != null && lstProdCategoryDetails.Count > 0)
                                {
                                    var resProductRange = new List<ProductRangeDetails>();
                                    var RoomDetailsInfo = new List<RoomDetailsInfo>();
                                    int i = 1;
                                    foreach (var item in chkPos)
                                    {
                                        var position = chkPos.Where(a => a.PositionId == item.PositionId).FirstOrDefault();
                                        if (position != null)
                                        {
                                            resProductRange = lstProductRange.Where(a => a.ProductId == position.ProductID).ToList();
                                            RoomDetailsInfo = resProductRange.Select(a => new RoomDetailsInfo
                                            {
                                                CreateDate = DateTime.Now,
                                                CreateUser = "",
                                                IsSupplement = (a.AdditionalYN == null || a.AdditionalYN == false) ? false : true,
                                                ProdDesc = a.ProductMenu,
                                                ProductCategory = lstProdCategoryDetails.Where(b => b.ProductCategoryId == a.ProductCategoryId).FirstOrDefault() != null ?
                                                                    lstProdCategoryDetails.Where(b => b.ProductCategoryId == a.ProductCategoryId).FirstOrDefault().ProductCategoryName : "",
                                                ProductCategoryId = a.ProductCategoryId,
                                                ProductRange = a.ProductRangeName + " (" + a.PersonType + ")",
                                                ProductRangeId = a.VoyagerProductRange_Id,
                                                RoomId = Guid.NewGuid().ToString(),
                                                CrossPositionId = item.PositionId
                                            }).ToList();

                                            if (item.RoomDetailsInfo != null)
                                            {
                                                item.RoomDetailsInfo.AddRange(item.RoomDetailsInfo.FindAll(a => !RoomDetailsInfo.Exists(b => a.ProductRangeId == b.ProductRangeId)));
                                            }
                                            else
                                            {
                                                item.RoomDetailsInfo = RoomDetailsInfo;
                                            }
                                            item.RoomDetailsInfo.ForEach(a => a.RoomSequence = i++);
                                            position.RoomDetailsInfo = item.RoomDetailsInfo.OrderBy(a => a.IsSupplement).ThenBy(a => a.ProductRange).ToList();

                                            ReplaceOneResult replaceResult = await _MongoContext.mPosition.ReplaceOneAsync(Builders<mPosition>.Filter.Eq("PositionId", item.PositionId), position);
                                        }
                                    }
                                }
                            }
                            lstPosition.AddRange(chkPos);
                        }
                        #endregion

                        #region If staying Room service in dropdown is selected (for static or dynamic TE) then insert/update Rooming Details & Price into Accomodation,Meals positions
                        List<mPosition> lstPositionNotExists = new List<mPosition>();
                        List<mPosition> lstPositionExists = new List<mPosition>();

                        var accommealpositions = positions.Where(a => a.ProductType == "Hotel" || (a.ProductType == "Meal" && (a.MealType == "Lunch" || a.MealType == "Dinner"))).ToList();

                        if (accommealpositions != null && accommealpositions.Count > 0)//&& dynamicres != null && dynamicres.Count > 0
                        {
                            //If TE is unchecked from UI then delete the services from Acco, Meals Positions and its Price details from mPositionPrice collection
                            var delListPositionIds = tourlist.Select(a => a.PositionID).Distinct().ToList();
                            if (delListPositionIds?.Count > 0)
                            {
                                foreach (var item in accommealpositions)
                                {
                                    var roomids = item.RoomDetailsInfo.Where(a => !string.IsNullOrEmpty(a.CrossPositionId) && delListPositionIds.Contains(a.CrossPositionId)).ToList().Select(a => a.RoomId).ToList();
                                    if (roomids.Count > 0)
                                    {
                                        item.RoomDetailsInfo.Where(a => !string.IsNullOrEmpty(a.CrossPositionId) && delListPositionIds.Contains(a.CrossPositionId)).ToList().
                                        ForEach(a => { a.EditDate = DateTime.Now; a.EditUser = request.UserName; a.IsDeleted = true; });

                                        var objmPosition = await _MongoContext.mPosition.FindOneAndUpdateAsync(m => m.QRFID == request.QRFID && m.PositionId == item.PositionId,
                                            Builders<mPosition>.Update.Set(m => m.RoomDetailsInfo, item.RoomDetailsInfo));

                                        var objmPositionPrice = await _MongoContext.mPositionPrice.UpdateManyAsync(m => m.QRFID == request.QRFID && m.PositionId == item.PositionId && roomids.Contains(m.RoomId),
                                            Builders<mPositionPrice>.Update.Set(m => m.IsDeleted, true).Set(m => m.EditDate, DateTime.Now).Set(m => m.EditUser, request.UserName));
                                    }
                                    positions = positions.Where(a => a.IsDeleted == false).ToList();
                                }
                            }

                            RoomDetailsInfo objRoomDetailsInfo = new RoomDetailsInfo();
                            var roominfo = new List<RoomDetailsInfo>();
                            var lastseq = 0;
                            request.TourEntities.ForEach(a => a.Flag = (a.Type.Contains("Coach") || a.Type.Contains("LDC") ? "DRIVER" : (a.Type.Contains("Guide") || a.Type.Contains("Assistant")) ? "GUIDE" : "GUIDE"));
                            dynamicres = request.TourEntities;
                            var posids = dynamicres.Where(a => !string.IsNullOrEmpty(a.PositionID)).Select(a => a.PositionID).ToList();
                            var tourpositions = _MongoContext.mPosition.AsQueryable().Where(a => a.QRFID == request.QRFID && posids.Contains(a.PositionId) && a.IsDeleted == false).ToList();

                            List<ProductRangeDetails> lstProductRangeDetails = _productRepository.GetProductRangeByParam(new ProductRangeGetReq { ProductIdList = accommealpositions.Select(a => a.ProductID).ToList(), PersonTypeList = new List<string>() { "DRIVER", "GUIDE" } }).ProductRangeDetails;
                            if (lstProductRangeDetails != null && lstProductRangeDetails.Count > 0)
                            {
                                lstPosition = new List<mPosition>();
                                int i = 0;
                                string strchkin, strchkout = "";
                                TimeSpan timein, timeout, postimein, postimeout;
                                var oldroominfo = new List<RoomDetailsInfo>();
                                var mpos = new mPosition();
                                var prodrangeres = new ProductRangeDetails();

                                var positionsTE = _MongoContext.mPosition.AsQueryable().Where(a => a.QRFID == request.QRFID && posids.Contains(a.PositionId) && a.IsDeleted == false).ToList();

                                foreach (var item in accommealpositions)
                                {
                                    var accommealProdRange = lstProductRangeDetails.Where(a => a.ProductId == item.ProductID).ToList();
                                    if (accommealProdRange != null && accommealProdRange.Count > 0)
                                    {
                                        oldroominfo = new List<RoomDetailsInfo>();
                                        oldroominfo = item.RoomDetailsInfo.Where(a => a.IsDeleted == false).ToList();
                                        roominfo = oldroominfo.OrderBy(a => a.RoomSequence).ToList();
                                        var aa = item.ProductType;
                                        var aaa = item.MealType;

                                        lastseq = roominfo.TakeLast(1).FirstOrDefault().RoomSequence;
                                        i = lastseq;
                                        item.RoomDetailsInfo = new List<RoomDetailsInfo>();

                                        foreach (var d in dynamicres)
                                        {
                                            if (!string.IsNullOrEmpty(d.PositionID))
                                            {
                                                if ((d.IsDinner && !string.IsNullOrEmpty(item.MealType) && item.MealType.ToLower() == "dinner")
                                                    || (d.IsLunch && !string.IsNullOrEmpty(item.MealType) && item.MealType.ToLower() == "lunch")
                                                    || (item.ProductType.ToLower() == "hotel"))
                                                {
                                                    mpos = positionsTE.Where(a => a.PositionId == d.PositionID).FirstOrDefault();
                                                    int duration = 0;
                                                    if (item.ProductType.ToLower() == "hotel")
                                                    {
                                                        duration = mpos.DayNo == 1 ? (mpos.Duration - 1) : ((mpos.Duration + mpos.DayNo) - 2);
                                                    }
                                                    else
                                                    {
                                                        duration = mpos.DayNo == 1 ? mpos.Duration : ((mpos.Duration + mpos.DayNo) - 1);
                                                    }

                                                    if (item.DayNo >= mpos.DayNo && item.DayNo <= duration)
                                                    {
                                                        prodrangeres = new ProductRangeDetails();

                                                        if ((d.IsDinner && !string.IsNullOrEmpty(item.MealType) && item.MealType.ToLower() == "dinner")
                                                            || (d.IsLunch && !string.IsNullOrEmpty(item.MealType) && item.MealType.ToLower() == "lunch"))//item.StartingFrom == d.Type.Split(',')[1] && 
                                                        {
                                                            if (!string.IsNullOrEmpty(mpos.StartTime) && !string.IsNullOrEmpty(mpos.EndTime))
                                                            {
                                                                strchkin = mpos.StartTime;
                                                                strchkout = ((mpos.ProductType == "Guide" || mpos.ProductType == "Assistant") && (mpos.EndTime == "09:00" || Convert.ToInt32(mpos.EndTime.Split(":")[0]) >= 24)) ? "23:59" : mpos.EndTime;
                                                                //strchkout = mpos.EndTime;

                                                                TimeSpan.TryParse(strchkin, out timein); //TE Position TimeIn 7
                                                                TimeSpan.TryParse(strchkout, out timeout);//TE Position TimeOut 18 7.30

                                                                TimeSpan.TryParse(item.StartTime, out postimein);//13
                                                                TimeSpan.TryParse(item.EndTime, out postimeout);//13.30

                                                                if ((postimein <= timein || postimein <= timeout) && (postimeout <= timein || postimeout <= timeout))
                                                                {
                                                                    prodrangeres = !string.IsNullOrEmpty(d.RoomType) ? accommealProdRange.Where(a => a.ProductRangeName == "MEAL" && a.PersonType == d.Flag).FirstOrDefault() : null;
                                                                }
                                                                else
                                                                {
                                                                    prodrangeres = null;
                                                                }
                                                            }
                                                        }

                                                        //HowMany & Duration should be check for Hotel Positions only
                                                        if (item.ProductType.ToLower() == "hotel" && tourpositions.Where(a => a.PositionId == d.PositionID).FirstOrDefault().Duration > 1)
                                                        {
                                                            if (Convert.ToInt32(d.HowMany) > 0)
                                                            {
                                                                prodrangeres = !string.IsNullOrEmpty(d.RoomType) ? accommealProdRange.Where(a => a.ProductRangeName == d.RoomType.ToUpper() && a.PersonType == d.Flag).FirstOrDefault() : null;
                                                            }
                                                            else
                                                            {
                                                                var dynamicresdetails = dynamicres.Where(a => a.PositionID == d.PositionID && a.RoomType == d.RoomType && a.Flag == d.Flag && Convert.ToInt32(a.HowMany) > 0).ToList();

                                                                if (dynamicresdetails.Count == 0)
                                                                {
                                                                    var roomserv = roominfo.Where(a => a.ProductRange == d.RoomType.ToUpper() + " (" + d.Flag + ")").FirstOrDefault();
                                                                    if (roomserv != null)
                                                                    {
                                                                        lstPositionNotExists.Add(new mPosition { PositionId = item.PositionId, RoomDetailsInfo = new List<RoomDetailsInfo> { roomserv } });
                                                                    }
                                                                }
                                                            }
                                                        }

                                                        if (prodrangeres != null && !string.IsNullOrEmpty(prodrangeres.VoyagerProductRange_Id))
                                                        {
                                                            var cntromminfo = roominfo.Where(a => a.ProductRangeId == prodrangeres.VoyagerProductRange_Id && a.ProductCategoryId == prodrangeres.ProductCategoryId
                                                            && a.CrossPositionId == d.PositionID).Count();
                                                            if (cntromminfo == 0)
                                                            {
                                                                objRoomDetailsInfo = new RoomDetailsInfo
                                                                {
                                                                    RoomSequence = i++,
                                                                    CreateDate = DateTime.Now,
                                                                    CreateUser = request.UserName,
                                                                    EditDate = null,
                                                                    EditUser = "",
                                                                    IsDeleted = false,
                                                                    IsSupplement = prodrangeres.AdditionalYN == true ? true : false,
                                                                    ProductCategory = roominfo.FirstOrDefault().ProductCategory,
                                                                    ProductCategoryId = prodrangeres.ProductCategoryId,
                                                                    ProductRange = prodrangeres.ProductRangeName + " (" + prodrangeres.PersonType + ")",
                                                                    ProductRangeId = prodrangeres.VoyagerProductRange_Id,
                                                                    RoomId = Guid.NewGuid().ToString(),
                                                                    CrossPositionId = d.PositionID
                                                                };
                                                                item.RoomDetailsInfo.Add(objRoomDetailsInfo);
                                                                roominfo.Add(objRoomDetailsInfo);
                                                            }
                                                            else if (oldroominfo.Where(a => a.ProductRangeId == prodrangeres.VoyagerProductRange_Id && a.ProductCategoryId == prodrangeres.ProductCategoryId).Count() > 0)
                                                            {
                                                                if (lstPositionExists.Where(a => a.PositionId == item.PositionId).Count() == 0)
                                                                {
                                                                    lstPositionExists.Add(item);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        if (item.RoomDetailsInfo != null && item.RoomDetailsInfo.Count > 0)
                                        {
                                            lstPosition.Add(item);
                                        }
                                        //take IsDeleted=true in lstPositionNotExists
                                        var isdeletedrooms = dynamicres.Where(a => a.IsDeleted == true).Select(a => (a.RoomType + " (" + a.Flag + ")")).Distinct().ToList();
                                        var exists = isdeletedrooms != null && isdeletedrooms.Count > 0 ? roominfo.FindAll(a => !string.IsNullOrEmpty(a.CrossPositionId) && isdeletedrooms.Exists(b => a.ProductRange.ToUpper().Trim() == b.ToUpper().Trim())) :
                                            new List<RoomDetailsInfo>();

                                        //take not existing rooming details into notexists variable
                                        var prodrangenm = dynamicres.Where(a => a.IsDeleted == false).Select(a => (a.RoomType + " (" + a.Flag + ")")).Distinct().ToList();
                                        var mealranges = dynamicres.Where(b => b.IsDinner || b.IsLunch).Select(b => "MEAL" + " (" + b.Flag + ")").Distinct().ToList();
                                        prodrangenm.AddRange(mealranges);
                                        var notexists = prodrangenm != null && prodrangenm.Count > 0 ? roominfo.FindAll(a => !string.IsNullOrEmpty(a.CrossPositionId) && !prodrangenm.Exists(b => a.ProductRange.ToUpper().Trim() == b.ToUpper().Trim())) :
                                              new List<RoomDetailsInfo>();
                                        notexists.AddRange(exists);
                                        if (notexists != null && notexists.Count > 0)
                                        {
                                            lstPositionNotExists.Add(new mPosition { PositionId = item.PositionId, RoomDetailsInfo = notexists });
                                        }
                                    }
                                }

                                //insert new rooming details into mPosition
                                if (lstPosition != null && lstPosition.Count > 0)
                                {
                                    foreach (var item in lstPosition)
                                    {
                                        resultFlag = await _MongoContext.mPosition.UpdateOneAsync(Builders<mPosition>.Filter.Eq("PositionId", item.PositionId),
                                       Builders<mPosition>.Update.PushEach<RoomDetailsInfo>("RoomDetailsInfo", item.RoomDetailsInfo).Set("CreateDate", DateTime.Now).Set("CreateUser", request.UserName));
                                    }
                                }

                                // if roomig details not exists then update IsDeleted to true in mPosition
                                if (lstPositionNotExists != null && lstPositionNotExists.Count > 0)
                                {
                                    var lstStrPos = lstPositionNotExists.Select(b => b.PositionId).ToList();
                                    var posres = _MongoContext.mPosition.AsQueryable().Where(a => lstStrPos.Contains(a.PositionId)).ToList();
                                    mPosition position = new mPosition();

                                    foreach (var item in posres)
                                    {
                                        if (item.RoomDetailsInfo != null && item.RoomDetailsInfo.Count > 0)
                                        {
                                            var res = lstPositionNotExists.Where(a => a.PositionId == item.PositionId).Select(a => a.RoomDetailsInfo).FirstOrDefault();
                                            item.RoomDetailsInfo.FindAll(a => res.Exists(b => a.RoomId == b.RoomId)).ForEach(a => { a.IsDeleted = true; a.EditDate = DateTime.Now; a.EditUser = request.UserName; });

                                            position = await _MongoContext.mPosition.FindOneAndUpdateAsync(m => m.QRFID == request.QRFID && m.PositionId == item.PositionId,
                                                   Builders<mPosition>.Update.Set(m => m.RoomDetailsInfo, item.RoomDetailsInfo));
                                        }
                                    }
                                }
                            }
                        }

                        //UPSERT price details into mPositionPrice for Static & Dynamic TE which are added in Meals and Hotels						 
                        if (accommealpositions?.Count > 0)
                        {
                            var lstStrPos = accommealpositions.Select(b => b.PositionId).Distinct().ToList();
                            var posres = _MongoContext.mPosition.AsQueryable().Where(a => lstStrPos.Contains(a.PositionId)).ToList();
                            quote = await _MongoContext.mQuote.FindAsync(a => a.QRFID == request.QRFID).Result.FirstOrDefaultAsync();

                            PositionPriceGetReq objPositionPriceGetReq = new PositionPriceGetReq() { QRFID = request.QRFID };
                            quote.Departures = quote.Departures.Where(a => a.IsDeleted == false).ToList();
                            quote.TourEntities = quote.TourEntities.Where(a => a.IsDeleted == false).ToList();
                            PositionPriceGetRes objPositionPriceGetRes = await GetSetPositionPriceByTourEntity(objPositionPriceGetReq, quote, posres, request.UserName);
                        }
                        #endregion
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "Tour Entity details can not be blank.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "QRFID not exists.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.ErrorMessage = ex.Message;
                response.ResponseStatus.Status = "Failure";
            }
            return response;
        }

        public async Task<TourEntitiesGetRes> GetTourEntities(TourEntitiesGetReq request)
        {
            TourEntitiesGetRes response = new TourEntitiesGetRes() { QRFID = request.QRFID };
            List<TourEntities> lstTourEntities = new List<TourEntities>();
            try
            {
                var quote = await _MongoContext.mQuote.FindAsync(a => a.QRFID == request.QRFID).Result.FirstOrDefaultAsync();
                if (quote != null)
                {
                    PaxGetRequest paxGetRequest = new PaxGetRequest() { QRFID = request.QRFID };
                    PaxGetResponse paxGetResponse = GetPaxSlabDetailsForQRF_Id(paxGetRequest);

                    if (paxGetResponse.Status.ToLower() == "success" && paxGetResponse != null && paxGetResponse.PaxSlabDetails != null && paxGetResponse.PaxSlabDetails.PaxSlabs.Count > 0)
                    {
                        response.PaxSlabDetails.PaxSlabs = paxGetResponse.PaxSlabDetails.PaxSlabs;
                        if (string.IsNullOrEmpty(request.PositionID))
                        {
                            if (!string.IsNullOrEmpty(request.Type))
                            {
                                var tourentity = quote.TourEntities.Where(a => a.Type.Contains(request.Type.Trim()) && a.IsDeleted == false).ToList();
                                lstTourEntities = tourentity;
                            }
                            else
                            {
                                lstTourEntities = quote.TourEntities.Where(a => a.IsDeleted == false).ToList();
                            }
                        }
                        else
                        {
                            //var tourentity = quote.TourEntities.Where(a => a.Type.Contains(request.Type.Trim()) && a.IsDeleted == false).ToList();
                            List<string> lstposIds = request.PositionID.Split(",").ToList();
                            var tourentity = quote.TourEntities.Where(a => lstposIds.Contains(a.PositionID) && a.IsDeleted == false).ToList();
                            lstTourEntities = tourentity;
                        }

                        if (lstTourEntities != null && lstTourEntities.Count > 0)
                        {
                            var lstnotexists = lstTourEntities.FindAll(a => !response.PaxSlabDetails.PaxSlabs.Exists(b => a.PaxSlab == b.From.ToString() + " - " + b.To.ToString()));
                            if (lstnotexists != null && lstnotexists.Count > 0)
                            {
                                lstTourEntities.FindAll(a => lstnotexists.Exists(b => b.TourEntityID == a.TourEntityID)).ForEach(a => a.IsDeleted = true);
                            }
                            response.ResponseStatus.Status = "Success";
                        }
                        else
                        {
                            response.ResponseStatus.Status = "Success";
                            response.ResponseStatus.ErrorMessage = "Tour Entities details not found.";
                        }
                    }
                    else
                    {
                        response.PaxSlabDetails = new PaxSlabDetails();
                        response.PaxSlabDetails.PaxSlabs = new List<PaxSlabs>();
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "Pax details not found.";
                    }
                }
                else
                {
                    response.PaxSlabDetails = new PaxSlabDetails();
                    response.PaxSlabDetails.PaxSlabs = new List<PaxSlabs>();
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "QRFID not exists.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.ErrorMessage = ex.Message;
                response.ResponseStatus.Status = "Failure";
            }
            lstTourEntities = lstTourEntities.Where(a => a.IsDeleted == false).OrderBy(a => a.PaxSlab).ToList();
            response.TourEntities = lstTourEntities;
            return response;

        }

        public async Task<PositionPriceGetRes> GetSetPositionPriceByTourEntity(PositionPriceGetReq request, mQuote result, List<mPosition> positionData, string username)
        {
            PositionPriceGetRes response = new PositionPriceGetRes();
            try
            {
                String supplierId = "";
                mPositionPrice objPricesInfo = new mPositionPrice();
                List<mProductRange> prodrange = new List<mProductRange>();
                var positionids = new List<string>();

                if (result != null && positionData != null)
                {
                    positionids = positionData.Select(a => a.PositionId).ToList();
                    var procatdetails = new List<string>();
                    var procatlist = positionData.Select(a => a.RoomDetailsInfo).ToList();
                    var roomDetailsList = new List<RoomDetailsInfo>();

                    foreach (var item in procatlist)
                    {
                        roomDetailsList.AddRange(item);
                    }
                    List<string> RangeIdList = new List<string>();
                    RangeIdList.AddRange(roomDetailsList.Where(b => b.IsDeleted == false).Select(b => b.ProductRangeId).ToList());

                    var builder = Builders<mProductRange>.Filter;
                    var filter = builder.Where(a => RangeIdList.Contains(a.VoyagerProductRange_Id));
                    prodrange = await _MongoContext.mProductRange.Find(filter).ToListAsync();

                    var supplierids = positionData.Select(a => a.SupplierId).ToList();
                    var currencyidlist = _MongoContext.mProductSupplier.AsQueryable().Where(s => supplierids.Contains(s.Company_Id)).ToList();

                    var curids = currencyidlist.Select(a => a.Currency_Id).ToList();
                    var currencylist = _MongoContext.mCurrency.AsQueryable().Where(c => curids.Contains(c.VoyagerCurrency_Id)).ToList();
                    var TourEntitiesPaxSlab = new List<TourEntities>();
                    var currencyId = "";
                    var currency = "";

                    result.TourEntities.ForEach(a => a.Flag = (a.Type.Contains("Coach") || a.Type.Contains("LDC") ? "DRIVER" : (a.Type.Contains("Guide") || a.Type.Contains("Assistant")) ? "GUIDE" : "GUIDE"));

                    for (int p = 0; p < positionData.Count; p++)
                    {
                        response.StandardPrice = positionData[p].StandardPrice;
                        supplierId = positionData[p].SupplierId;

                        currencyId = currencyidlist.Where(s => s.Company_Id == positionData[p].SupplierId && s.Product_Id == positionData[p].ProductID).Select(a => a.Currency_Id).FirstOrDefault();
                        currency = currencylist.Where(c => c.VoyagerCurrency_Id == currencyId).Select(a => a.Currency).FirstOrDefault();

                        int addDaysToPeriod = 0;
                        addDaysToPeriod = (positionData[p].DayNo - 1) + (positionData[p].Duration - 1);

                        for (int i = 0; i < result.Departures.Count; i++)
                        {
                            positionData[p].RoomDetailsInfo = positionData[p].RoomDetailsInfo.Where(a => !string.IsNullOrEmpty(a.CrossPositionId) == true && a.IsDeleted == false).ToList();

                            if (positionData[p].RoomDetailsInfo.Count > 0)
                            {
                                for (int k = 0; k < positionData[p].RoomDetailsInfo.Count; k++)
                                {
                                    TourEntitiesPaxSlab = new List<TourEntities>();
                                    if (positionData[p].ProductType.ToLower() == "meal")
                                    {
                                        if (!string.IsNullOrEmpty(positionData[p].MealType) && positionData[p].MealType.ToLower() == "lunch")
                                        {
                                            TourEntitiesPaxSlab = result.TourEntities.Where(a => a.IsDeleted == false && a.PositionID == positionData[p].RoomDetailsInfo[k].CrossPositionId
                                                              && positionData[p].RoomDetailsInfo[k].ProductRange.ToUpper() == "MEAL (" + a.Flag + ")" && a.IsLunch).
                                                              Select(a => new TourEntities { TourEntityID = a.TourEntityID, PaxSlab = a.PaxSlab, PaxSlabID = a.PaxSlabID }).ToList();
                                        }
                                        else if (!string.IsNullOrEmpty(positionData[p].MealType) && positionData[p].MealType.ToLower() == "dinner")
                                        {
                                            TourEntitiesPaxSlab = result.TourEntities.Where(a => a.IsDeleted == false && a.PositionID == positionData[p].RoomDetailsInfo[k].CrossPositionId
                                                              && positionData[p].RoomDetailsInfo[k].ProductRange.ToUpper() == "MEAL (" + a.Flag + ")" && a.IsDinner).
                                                              Select(a => new TourEntities { TourEntityID = a.TourEntityID, PaxSlab = a.PaxSlab, PaxSlabID = a.PaxSlabID }).ToList();
                                        }
                                    }
                                    else if (positionData[p].ProductType.ToLower() == "assistant" && positionData[p].IsTourEntity)
                                    {
                                        //&& Convert.ToInt32(a.HowMany) > 0
                                        TourEntitiesPaxSlab = result.TourEntities.Where(a => a.IsDeleted == false && a.PositionID == positionData[p].RoomDetailsInfo[k].CrossPositionId).
                                                              Select(a => new TourEntities { TourEntityID = a.TourEntityID, PaxSlab = a.PaxSlab, PaxSlabID = a.PaxSlabID }).Distinct().ToList();
                                    }
                                    else  //for hotels
                                    {//&& Convert.ToInt32(a.HowMany) > 0
                                        TourEntitiesPaxSlab = result.TourEntities.Where(a => a.IsDeleted == false && a.PositionID == positionData[p].RoomDetailsInfo[k].CrossPositionId
                                                              && (a.RoomType.ToUpper() + " (" + a.Flag + ")") == positionData[p].RoomDetailsInfo[k].ProductRange.ToUpper()
                                                              && Convert.ToInt32(a.HowMany) > 0).
                                                              Select(a => new TourEntities { TourEntityID = a.TourEntityID, PaxSlab = a.PaxSlab, PaxSlabID = a.PaxSlabID }).ToList();
                                    }

                                    if (TourEntitiesPaxSlab != null && TourEntitiesPaxSlab.Count > 0)
                                    {
                                        for (int j = 0; j < TourEntitiesPaxSlab.Count; j++)
                                        {
                                            objPricesInfo = new mPositionPrice
                                            {
                                                QRFID = request.QRFID,
                                                PositionId = positionData[p].PositionId,
                                                DepartureId = result.Departures[i].Departure_Id,
                                                Period = result.Departures[i].Date,
                                                PaxSlabId = TourEntitiesPaxSlab[j].PaxSlabID,
                                                PaxSlab = TourEntitiesPaxSlab[j].PaxSlab,
                                                SupplierId = positionData[p].SupplierId,
                                                Supplier = positionData[p].SupplierName,
                                                RoomId = positionData[p].RoomDetailsInfo[k].RoomId,
                                                IsSupplement = positionData[p].RoomDetailsInfo[k].IsSupplement,
                                                ProductCategoryId = positionData[p].RoomDetailsInfo[k].ProductCategoryId,
                                                ProductRangeId = positionData[p].RoomDetailsInfo[k].ProductRangeId,
                                                ProductCategory = positionData[p].RoomDetailsInfo[k].ProductCategory,
                                                ProductRange = positionData[p].RoomDetailsInfo[k].ProductRange,
                                                BuyCurrencyId = currencyId,
                                                BuyCurrency = currency,
                                                TourEntityId = TourEntitiesPaxSlab[j].TourEntityID
                                            };
                                            if (addDaysToPeriod > 0)
                                                objPricesInfo.ContractPeriod = Convert.ToDateTime(objPricesInfo.ContractPeriod).AddDays(addDaysToPeriod);

                                            objPricesInfo.Type = prodrange.Where(a => a.VoyagerProductRange_Id == objPricesInfo.ProductRangeId).Select(b => b.PersonType).FirstOrDefault();
                                            objPricesInfo.ProductRangeCode = prodrange.Where(a => a.VoyagerProductRange_Id == objPricesInfo.ProductRangeId).Select(b => b.ProductRangeCode).FirstOrDefault();
                                            response.PositionPrice.Add(objPricesInfo);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (response.PositionPrice?.Count > 0)
                    {
                        #region Get Contract Rates By Service
                        ProdContractGetRes prodContractGetRes = new ProdContractGetRes();
                        ProdContractGetReq prodContractGetReq = new ProdContractGetReq
                        {
                            QRFID = request.QRFID,
                            ProductIDList = positionData.Select(a => a.ProductID).ToList(),
                            SupplierId = supplierId
                        };

                        var rangelist = response.PositionPrice.Select(c => c.ProductRangeId).ToList();
                        prodContractGetRes = _productRepository.GetContractRatesByProductID(prodContractGetReq, rangelist);
                        var prodid = "";

                        if (prodContractGetRes != null && prodContractGetRes.ProductContractInfo.Count > 0)
                        {
                            for (int i = 0; i < response.PositionPrice.Count; i++)
                            {
                                prodid = positionData.Where(a => a.PositionId == response.PositionPrice[i].PositionId).Select(a => a.ProductID).FirstOrDefault();
                                if (!string.IsNullOrEmpty(prodid))
                                {
                                    var lstPCInfo = prodContractGetRes.ProductContractInfo.Where(a => a.SupplierId == response.PositionPrice[i].SupplierId && a.ProductId == prodid
                                                  && a.ProductRangeId == response.PositionPrice[i].ProductRangeId
                                                  && (a.FromDate <= response.PositionPrice[i].ContractPeriod && response.PositionPrice[i].ContractPeriod <= a.ToDate)).ToList();

                                    if (lstPCInfo != null && lstPCInfo.Count > 0)
                                    {
                                        foreach (var con in lstPCInfo)
                                        {
                                            char[] dayPattern = con.DayComboPattern.ToCharArray();
                                            int dayNo = (int)Convert.ToDateTime(response.PositionPrice[i].ContractPeriod).DayOfWeek;

                                            if (dayNo == 0)
                                                dayNo = 7;

                                            if (dayPattern[dayNo - 1] == '1')
                                            {
                                                response.PositionPrice[i].ContractId = con.ContractId;
                                                response.PositionPrice[i].ContractPrice = Convert.ToDouble(con.Price);
                                                response.PositionPrice[i].BudgetPrice = Convert.ToDouble(con.Price);
                                                response.PositionPrice[i].BuyCurrencyId = con.CurrencyId;
                                                response.PositionPrice[i].BuyCurrency = con.Currency;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        #endregion

                        #region Save Currency into mPosition 
                        mPosition objPosition = new mPosition();
                        for (int p = 0; p < positionData.Count; p++)
                        {
                            objPosition = new mPosition();
                            objPosition = _MongoContext.mPosition.AsQueryable().Where(a => a.PositionId == positionData[p].PositionId).FirstOrDefault();
                            var pos = response.PositionPrice.Where(a => a.PositionId == positionData[p].PositionId).FirstOrDefault();
                            if (pos != null)
                            {
                                objPosition.BuyCurrencyId = pos.BuyCurrencyId;
                                objPosition.BuyCurrency = pos.BuyCurrency;
                                objPosition.EditDate = DateTime.Now;

                                ReplaceOneResult replaceResult = await _MongoContext.mPosition.ReplaceOneAsync(Builders<mPosition>.Filter.Eq("PositionId", objPosition.PositionId), objPosition);
                            }
                        }
                        #endregion

                        #region Get Saved Data from mPositionPrices and update IsDeleted to True those are deleted services and also replace the ProdRangeName
                        var roomingnmdiff = new List<mPositionPrice>();
                        var resultPosPrices = _MongoContext.mPositionPrice.AsQueryable().Where(a => a.QRFID == result.QRFID && positionids.Contains(a.PositionId) && a.IsDeleted == false
                                                                                && !string.IsNullOrEmpty(a.TourEntityId)).ToList();

                        if (resultPosPrices != null && resultPosPrices.Count > 0)
                        {
                            for (int i = 0; i < response.PositionPrice.Count; i++)
                            {
                                var pospr = resultPosPrices.Where(a => a.QRFID == response.PositionPrice[i].QRFID && a.PositionId == response.PositionPrice[i].PositionId
                                 && a.DepartureId == response.PositionPrice[i].DepartureId && a.PaxSlabId == response.PositionPrice[i].PaxSlabId &&
                                 a.RoomId == response.PositionPrice[i].RoomId).FirstOrDefault();

                                if (pospr != null)
                                {
                                    response.PositionPrice[i].PositionPriceId = pospr.PositionPriceId;
                                    response.PositionPrice[i].BudgetPrice = pospr.BudgetPrice;
                                }
                            }

                            var posprexist = response.PositionPrice.Where(c => !string.IsNullOrEmpty(c.PositionPriceId)).ToList();
                            var roomingids = resultPosPrices.FindAll(a => !posprexist.Exists(b => b.PositionPriceId == a.PositionPriceId));
                            roomingnmdiff = resultPosPrices.FindAll(a => !posprexist.Exists(b => b.ProductRange == a.ProductRange));
                            roomingids.AddRange(roomingnmdiff);
                            if (roomingids != null && roomingids.Count > 0)
                            {
                                UpdateResult resultFlag;
                                foreach (var item in roomingids)
                                {
                                    resultFlag = await _MongoContext.mPositionPrice.UpdateOneAsync(Builders<mPositionPrice>.Filter.Eq("PositionPriceId", item.PositionPriceId),
                                        Builders<mPositionPrice>.Update.Set("IsDeleted", true).Set("EditDate", DateTime.Now).
                                        Set("EditUser", username));
                                }
                            }

                            response.PositionPrice = response.PositionPrice.OrderBy(a => a.ProductRange).ToList();
                        }
                        #endregion

                        #region Set PositionPrice 
                        //the below code is for if from frontend existing Dropdown of Services is changed then deactive the service and after that insert it.dectivation code is above
                        var prposlist = response.PositionPrice.Where(a => string.IsNullOrEmpty(a.PositionPriceId) || a.PositionPriceId == Guid.Empty.ToString()).ToList();
                        if (roomingnmdiff != null && roomingnmdiff.Count > 0)
                        {
                            var posids = roomingnmdiff.Select(a => a.PositionPriceId).ToList();
                            List<mPositionPrice> objPositionPrices = response.PositionPrice.Where(a => posids.Contains(a.PositionPriceId)).ToList();
                            objPositionPrices.ForEach(a => { a.BudgetPrice = 0; a.PositionPriceId = ""; });
                            prposlist.AddRange(objPositionPrices);
                        }
                        if (prposlist != null && prposlist.Count > 0)
                        {
                            foreach (var item in prposlist)
                            {
                                if (string.IsNullOrEmpty(item.PositionPriceId) || item.PositionPriceId == Guid.Empty.ToString())
                                {
                                    objPricesInfo = new mPositionPrice();
                                    item.PositionPriceId = Guid.NewGuid().ToString();
                                    item.CreateDate = DateTime.Now;
                                    item.EditUser = "";
                                    item.EditDate = null;
                                    item.IsDeleted = false;
                                    objPricesInfo = item;
                                    await _MongoContext.mPositionPrice.InsertOneAsync(objPricesInfo);
                                    response.ResponseStatus.Status = "Success";
                                    response.ResponseStatus.ErrorMessage = "Saved Successfully.";
                                }
                            }
                        }

                        var prposlistUpdate = response.PositionPrice.Where(a => !string.IsNullOrEmpty(a.PositionPriceId) && a.PositionPriceId != Guid.Empty.ToString()).ToList();
                        if (prposlistUpdate != null && prposlistUpdate.Count > 0)
                        {
                            var PositionPriceIdlist = prposlistUpdate.Select(a => a.PositionPriceId).ToList();
                            var PositionIdlist = prposlistUpdate.Select(a => a.PositionId).ToList();

                            var prposlistlist = _MongoContext.mPositionPrice.AsQueryable().Where(a => PositionIdlist.Contains(a.PositionId)).Distinct().ToList();
                            var positionlist = _MongoContext.mPosition.AsQueryable().Where(a => PositionIdlist.Contains(a.PositionId)).ToList();

                            var prposlistUpdatelist = prposlistlist.Where(a => PositionPriceIdlist.Contains(a.PositionPriceId)).ToList();
                            mPositionPrice res = new mPositionPrice();
                            var PosPrice = new List<mPositionPrice>();

                            foreach (var item in prposlistUpdatelist)
                            {
                                res = prposlistUpdate.Where(a => a.PositionPriceId == item.PositionPriceId).FirstOrDefault();
                                var posres = positionlist.Where(a => a.PositionId == item.PositionId).FirstOrDefault();
                                if (posres.ProductType.ToLower() == "meal")
                                {
                                    PosPrice = prposlistlist.Where(a => !string.IsNullOrEmpty(a.Type) && a.Type.ToLower() == "adult" && a.PositionId == item.PositionId).ToList();
                                }
                                else
                                {
                                    PosPrice = prposlistlist.Where(a => !string.IsNullOrEmpty(a.Type) && a.Type.ToLower() == "adult" && a.ProductRangeCode == item.ProductRangeCode && a.PositionId == item.PositionId).ToList();
                                }

                                item.EditDate = DateTime.Now;
                                item.EditUser = res.EditUser;
                                item.ContractId = res.ContractId;
                                item.ContractPrice = res.ContractPrice;
                                // item.BudgetPrice = res.BudgetPrice;
                                if (!string.IsNullOrEmpty(item.TourEntityId))
                                {
                                    item.BudgetPrice = PosPrice != null && PosPrice.Count > 0 ? PosPrice.Max(a => a.BudgetPrice) : res.BudgetPrice;
                                }
                                else
                                {
                                    item.BudgetPrice = res.BudgetPrice;
                                }
                                item.BuyCurrencyId = res.BuyCurrencyId;
                                item.BuyCurrency = res.BuyCurrency;
                                item.Period = res.Period;

                                ReplaceOneResult replaceResult = await _MongoContext.mPositionPrice.ReplaceOneAsync(Builders<mPositionPrice>.Filter.Eq("PositionPriceId", item.PositionPriceId), item);
                            }
                        }
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.ErrorMessage = ex.Message;
                response.ResponseStatus.Status = "Failure";
            }
            return response;
        }
        #endregion

        #region Meals
        public async Task<MealSetRes> SetMeals(MealSetReq request)
        {
            MealSetRes response = new MealSetRes() { QRFID = request.QRFID, MealDays = request.MealDays };
            response.ResponseStatus = new ResponseStatus();
            UpdateResult resultFlag;
            try
            {
                var quote = await _MongoContext.mQuote.FindAsync(a => a.QRFID == request.QRFID).Result.FirstOrDefaultAsync();
                if (quote != null)
                {
                    if (request.MealDays != null && request.MealDays.Count > 0)
                    {
                        if (quote.Meals != null && quote.Meals.MealDays != null && quote.Meals.MealDays.Count > 0)
                        {
                            var mealdayinfo = new MealDayInfo();
                            if (request.Flag == "partial")
                            {
                                var posids = new List<string>();
                                request.MealDays.SelectMany(a => a.MealDayInfo).ToList().ForEach(a => posids.Add(a.PositionID));
                                var prodList = _MongoContext.mPosition.AsQueryable().Where(x => posids.Contains(x.PositionId)).ToList();
                                var ProdIdList = prodList.Select(a => a.ProductID).ToList();
                                var mealProductList = _MongoContext.Products.AsQueryable().Where(x => ProdIdList.Contains(x.VoyagerProduct_Id)).Select(y => new { y.VoyagerProduct_Id, y.Address, y.Street, y.Corner, y.PostCode, y.SupplierTel, y.SupplierEmail }).ToList();

                                string FullAddress = "";
                                foreach (var item in request.MealDays)
                                {
                                    var mealdays = quote.Meals.MealDays.Where(a => a.RoutingDaysID == item.RoutingDaysID).FirstOrDefault();

                                    mealdayinfo = item.MealDayInfo.FirstOrDefault();
                                    var prodId = prodList.Where(a => a.ProductID == mealdayinfo.ProductID).FirstOrDefault()?.ProductID;
                                    var mealProduct = mealProductList.Where(a => a.VoyagerProduct_Id == prodId).FirstOrDefault();
                                    if (mealProduct != null)
                                    {
                                        FullAddress = (mealProduct.Address + "," + mealProduct.Street + "," + mealProduct.Corner + "," + mealProduct.PostCode)?.Replace(",,,", ",")?.Replace(",,", ",");
                                        if (mealdays != null)
                                        {
                                            if (quote.Meals.MealDays.Where(a => a.RoutingDaysID == item.RoutingDaysID &&
                                           !a.MealDayInfo.Select(b => b.MealType).ToList().Contains(item.MealDayInfo.FirstOrDefault().MealType)).FirstOrDefault() != null)
                                            {
                                                quote.Meals.MealDays.Where(a => a.RoutingDaysID == item.RoutingDaysID &&
                                                  !a.MealDayInfo.Select(b => b.MealType).ToList().Contains(item.MealDayInfo.FirstOrDefault().MealType)).FirstOrDefault().
                                                  MealDayInfo.Add(new MealDayInfo
                                                  {
                                                      MealType = mealdayinfo.MealType,
                                                      PositionID = mealdayinfo.PositionID,
                                                      StartTime = mealdayinfo.StartTime,
                                                      ProductID =  mealProduct.VoyagerProduct_Id,
                                                      Address =  mealProduct.Address ,
                                                      FullAddress =  FullAddress,
                                                      Telephone = mealProduct.SupplierTel,
                                                      Mail = mealProduct.SupplierEmail ,
                                                      IsDeleted = mealdayinfo.IsDeleted
                                                  });
                                            }
                                            else
                                            {
                                                quote.Meals.MealDays.Where(a => a.RoutingDaysID == item.RoutingDaysID &&
                                                 a.MealDayInfo.Select(b => b.MealType).ToList().Contains(item.MealDayInfo.FirstOrDefault().MealType)).FirstOrDefault().
                                                 MealDayInfo.ForEach(a =>
                                                 {
                                                     a.MealType = mealdayinfo.MealType;
                                                     a.PositionID = mealdayinfo.PositionID;
                                                     a.StartTime = mealdayinfo.StartTime;
                                                     a.ProductID =mealProduct.VoyagerProduct_Id ;
                                                     a.Address = mealProduct.Address;
                                                     a.FullAddress = FullAddress ;
                                                     a.Telephone =  mealProduct.SupplierTel;
                                                     a.Mail = mealProduct.SupplierEmail ;
                                                     a.IsDeleted = mealdayinfo.IsDeleted;
                                                 });
                                            }
                                            request.MealDays = quote.Meals.MealDays;
                                        }
                                        else
                                        {
                                            item.MealDayInfo.ForEach(a =>
                                            {
                                                if (!string.IsNullOrEmpty(a.PositionID))
                                                {
                                                    a.ProductID = mealProduct.VoyagerProduct_Id;
                                                    a.Address = mealProduct.Address;
                                                    a.FullAddress = FullAddress;
                                                    a.Telephone = mealProduct.SupplierTel;
                                                    a.Mail = mealProduct.SupplierEmail;
                                                }
                                            });
                                            quote.Meals.MealDays.Add(item);
                                            request.MealDays = quote.Meals.MealDays;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                List<string> lstPositionIds = new List<string>();
                                request.MealDays.ForEach(x => lstPositionIds.AddRange(x.MealDayInfo.Select(y => y.PositionID).ToList()));
                                var lstPos = _MongoContext.mPosition.AsQueryable().Where(x => lstPositionIds.Contains(x.PositionId)).ToList();
                                var prodids = lstPos.Select(a => a.ProductID).ToList();
                                var mealProduct = _MongoContext.Products.AsQueryable().Where(x => prodids.Contains(x.VoyagerProduct_Id)).Select(y => new { y.VoyagerProduct_Id, y.Address, y.Street, y.Corner, y.PostCode, y.SupplierTel, y.SupplierEmail }).ToList();

                                request.MealDays.ForEach(a => a.MealDayInfo.ForEach(b =>
                                {
                                    if (!string.IsNullOrEmpty(b.PositionID))
                                    {
                                        b.ProductID = lstPos.Where(c => c.PositionId == b.PositionID).FirstOrDefault().ProductID;

                                        var prod = mealProduct.Where(d => d.VoyagerProduct_Id == b.ProductID).FirstOrDefault();
                                        if (prod!=null)
                                        {
                                            b.Address = prod.Address;
                                            b.FullAddress = (prod.Address + "," + prod.Street + "," + prod.Corner + "," + prod.PostCode)?.Replace(",,,", ",")?.Replace(",,", ",");
                                            b.Telephone = prod.SupplierTel;
                                            b.Mail = prod.SupplierEmail; 
                                        } 
                                    }
                                }));
                            }

                            var res = await _MongoContext.mQuote.FindOneAndUpdateAsync(Builders<mQuote>.Filter.Eq("QRFID", request.QRFID),
                                        Builders<mQuote>.Update.Set("Meals.MealDays", request.MealDays)
                                        .Set("Meals.CreateUser", string.IsNullOrEmpty(quote.Meals.CreateUser) ? request.UserName : quote.Meals.CreateUser)
                                        .Set("Meals.CreateDate", quote.Meals.CreateDate)
                                        .Set("Meals.EditUser", request.UserName).Set("Meals.EditDate", DateTime.Now));
                            response.ResponseStatus.Status = res != null ? "Success" : "Failure";
                            response.ResponseStatus.ErrorMessage = res != null ? "" : "RoutingDays not updated";
                        }
                        else
                        {
                            List<string> lstPositionIds = new List<string>();
                            request.MealDays.ForEach(x => lstPositionIds.AddRange(x.MealDayInfo.Select(y => y.PositionID).ToList()));
                            var lstPos = _MongoContext.mPosition.AsQueryable().Where(y => lstPositionIds.Contains(y.PositionId)).ToList();
                            var prodids = lstPos.Select(a => a.ProductID).ToList();
                            var mealProduct = _MongoContext.Products.AsQueryable().Where(x => prodids.Contains(x.VoyagerProduct_Id)).Select(y => new { y.VoyagerProduct_Id, y.Address, y.Street, y.Corner, y.PostCode, y.SupplierTel, y.SupplierEmail }).ToList();

                            request.MealDays.ForEach(a => a.MealDayInfo.ForEach(b =>
                            {
                                if (!string.IsNullOrEmpty(b.PositionID))
                                {
                                    b.ProductID = lstPos.Where(c => c.PositionId == b.PositionID).FirstOrDefault().ProductID;

                                    var prod = mealProduct.Where(d => d.VoyagerProduct_Id == b.ProductID).FirstOrDefault();
                                    if (prod != null)
                                    {
                                        b.Address = prod.Address;
                                        b.FullAddress = (prod.Address + "," + prod.Street + "," + prod.Corner + "," + prod.PostCode)?.Replace(",,,", ",")?.Replace(",,", ",");
                                        b.Telephone = prod.SupplierTel;
                                        b.Mail = prod.SupplierEmail;
                                    }
                                }
                            }));

                            resultFlag = await _MongoContext.mQuote.UpdateOneAsync(Builders<mQuote>.Filter.Eq("QRFID", request.QRFID),
                            Builders<mQuote>.Update.PushEach<MealDays>("Meals.MealDays", request.MealDays).Set("Meals.CreateUser", request.UserName)
                            .Set("Meals.CreateDate", DateTime.Now));

                            response.ResponseStatus.Status = resultFlag.MatchedCount > 0 ? "Success" : "Failure";
                            response.ResponseStatus.ErrorMessage = resultFlag.MatchedCount > 0 ? "Meal details saved successfully." : "Meal details not saved";
                        }
                    }
                    else
                    {
                        resultFlag = await _MongoContext.mQuote.UpdateOneAsync(Builders<mQuote>.Filter.Eq("QRFID", request.QRFID),
                               Builders<mQuote>.Update.Set("Meals.MealDays", request.MealDays).Set("Meals.CreateUser", request.UserName)
                               .Set("Meals.CreateDate", DateTime.Now));

                        response.ResponseStatus.Status = "Success";
                        response.ResponseStatus.ErrorMessage = "Meal details saved successfully.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "QRFID not exists.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.ErrorMessage = ex.Message;
                response.ResponseStatus.Status = "Failure";
            }
            return response;
        }

        public async Task<MealGetRes> GetMeals(MealGetReq request)
        {
            MealGetRes response = new MealGetRes() { QRFID = request.QRFID };
            response.ResponseStatus = new ResponseStatus();
            try
            {
                var quote = await _MongoContext.mQuote.FindAsync(a => a.QRFID == request.QRFID).Result.FirstOrDefaultAsync();
                if (quote != null)
                {
                    if (quote.Meals != null)
                    {
                        response.MealDays = quote.Meals.MealDays;
                        response.ResponseStatus.Status = "Success";
                    }
                    else
                    {
                        quote.Meals = new Meals();
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "Not Found.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "QRFID not exists.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.ErrorMessage = ex.Message;
                response.ResponseStatus.Status = "Failure";
            }
            return response;
        }
        #endregion

        #region Follow Up
        public async Task<FollowUpSetRes> SetFollowUpForQRF(FollowUpSetReq req)
        {
            var response = new FollowUpSetRes();
            try
            {
                var quote = _MongoContext.mQuote.AsQueryable().Where(a => a.QRFID == req.QRFID).FirstOrDefault();
                if (quote != null)
                {
                    if (req.FollowUp != null && req.FollowUp.Count > 0)
                    {
                        if (quote.CurrentPipeline == "Quote Pipeline")
                        {
                            if (quote.FollowUp == null)
                                quote.FollowUp = new List<FollowUp>();

                            quote.FollowUp.AddRange(req.FollowUp);

                            var resultFlag = await _MongoContext.mQuote.UpdateOneAsync(Builders<mQuote>.Filter.Eq("QRFID", req.QRFID),
                                             Builders<mQuote>.Update.Set("FollowUp", quote.FollowUp));
                            response.ResponseStatus.Status = "Success";
                        }
                        else
                        {
                            var qRFPrice = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == req.QRFID && a.IsCurrentVersion == true).FirstOrDefault();
                            if (qRFPrice != null)
                            {
                                if (qRFPrice.FollowUp == null)
                                    qRFPrice.FollowUp = new List<FollowUp>();

                                qRFPrice.FollowUp.AddRange(req.FollowUp);

                                var resultFlag = await _MongoContext.mQRFPrice.UpdateOneAsync(Builders<mQRFPrice>.Filter.Eq("QRFPrice_Id", qRFPrice.QRFPrice_Id),
                                                 Builders<mQRFPrice>.Update.Set("FollowUp", qRFPrice.FollowUp));
                                response.ResponseStatus.Status = "Success";
                            }
                        }
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failed";
                    response.ResponseStatus.ErrorMessage = "Invalid QRF";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.ResponseStatus.Status = "Failed";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }
            return response;
        }

        public FollowUpGetRes GetFollowUpForQRF(FollowUpGetReq req)
        {
            var response = new FollowUpGetRes();
            try
            {
                var quote = _MongoContext.mQuote.AsQueryable().Where(a => a.QRFID == req.QRFID).FirstOrDefault();
                if (quote != null)
                {
                    if (quote.CurrentPipeline == "Quote Pipeline")
                    {
                        if (quote.FollowUp != null)
                        {
                            response.FollowUp = quote.FollowUp;
                            response.FollowUp.ForEach(a => a.CreateDate = Convert.ToDateTime(a.FollowUpTask[0].FollowUpDateTime));
                            response.FollowUp = response.FollowUp.OrderBy(a => a.CreateDate).ToList();
                        }
                    }
                    else
                    {
                        var qRFPrice = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == req.QRFID && a.IsCurrentVersion == true).FirstOrDefault();
                        if (qRFPrice != null)
                        {
                            if (qRFPrice.FollowUp != null)
                            {
                                response.FollowUp = qRFPrice.FollowUp;
                                response.FollowUp.ForEach(a => a.CreateDate = Convert.ToDateTime(a.FollowUpTask[0].FollowUpDateTime));
                                response.FollowUp = response.FollowUp.OrderBy(a => a.CreateDate).ToList();
                            }
                        }
                    }
                    if (response.FollowUp.Count() > 0)
                    {
                        response.ResponseStatus.Status = "Success";
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Success";
                        response.ResponseStatus.ErrorMessage = "No Follow Up Found";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Success";
                    response.ResponseStatus.ErrorMessage = "Invalid QRF";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.ResponseStatus.Status = "Failed";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }
            return response;
        }

        public FollowUpMasterGetRes GetFollowUpMasterData(FollowUpGetReq req)
        {
            var response = new FollowUpMasterGetRes();
            try
            {
                #region FollowUp Task
                var QRFMaster = _MongoContext.mTypeMaster.AsQueryable().Where(a => a.PropertyType.PropertyName == "QRF Masters").Select(b => b.PropertyType).FirstOrDefault();

                if (QRFMaster != null)
                {
                    var Values = QRFMaster.Attribute.Where(a => a.AttributeName == "FollowUpTask").Select(b => b.Values).FirstOrDefault();
                    if (Values != null && Values.Count > 0)
                        response.FollowUpTaskList = Values.OrderBy(c => c.Value).ToList();
                }
                #endregion

                #region Internal User
                CompanyOfficerGetReq internalUserReq = new CompanyOfficerGetReq();
                internalUserReq.CompanyId = req.CompanyId;
                var internalUserRes = _agentRepository.GetCompanyContacts(internalUserReq).Result;

                if (internalUserRes != null)
                {
                    response.InternalUserList = internalUserRes.ContactDetails.Where(a => Convert.ToString(a.STATUS).Trim() == "").ToList();
                }
                #endregion

                #region External User
                var AgentID = _MongoContext.mQuote.AsQueryable().Where(a => a.QRFID == req.QRFID).Select(b => b.AgentInfo.AgentID).FirstOrDefault();

                CompanyOfficerGetReq externalUserReq = new CompanyOfficerGetReq();
                externalUserReq.CompanyId = AgentID;
                var externalUserRes = _agentRepository.GetCompanyContacts(externalUserReq).Result;

                if (externalUserRes != null)
                {
                    response.ExternalUserList = externalUserRes.ContactDetails.Where(a => Convert.ToString(a.STATUS).Trim() == "").ToList(); ;
                }
                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.ResponseStatus.Status = "Failed";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }
            return response;
        }

        public List<FollowUp> GetLatestFollowUpForQRF(string QRFID, string CurrentPipeline)
        {
            var FollowUpList = new List<FollowUp>();
            var FollowUp = new List<FollowUp>();

            if (CurrentPipeline == "Quote Pipeline")
            {
                FollowUp = _MongoContext.mQuote.AsQueryable().Where(a => a.QRFID == QRFID).Select(b => b.FollowUp).FirstOrDefault();
            }
            else
            {
                FollowUp = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == QRFID && a.IsCurrentVersion == true).Select(b => b.FollowUp).FirstOrDefault();
            }
            if (FollowUp != null)
            {
                var Internal = FollowUp.Where(a => a.FollowUpTask.Any(b => b.FollowUpType == "Internal")).OrderByDescending(c => c.CreateDate).FirstOrDefault();
                var External = FollowUp.Where(a => a.FollowUpTask.Any(b => b.FollowUpType == "External")).OrderByDescending(c => c.CreateDate).FirstOrDefault();

                if (Internal != null)
                    FollowUpList.Add(Internal);
                if (External != null)
                    FollowUpList.Add(External);
            }
            return FollowUpList;
        }

        #endregion

        #region UPSERT/DELETE Price,Foc on UPSERT/Delete of Departure Date/Pax Slab
        public async Task<ResponseStatus> UpsertDeletePriceFocOnDatePaxSlabChange(DatePaxDetailsSetRequest req)
        {
            ResponseStatus responseStatus = new ResponseStatus();
            try
            {
                if (req != null)
                {
                    var result = _MongoContext.mQuote.AsQueryable().Where(a => a.QRFID == req.QRFID).FirstOrDefault();
                    if (result != null)
                    {
                        var lstPositions = _MongoContext.mPosition.AsQueryable().Where(a => a.QRFID == req.QRFID && a.IsDeleted == false).ToList();
                        if (lstPositions?.Count > 0)
                        {
                            #region Departure:- Update,Delete and Insert Price and FOC in mPositionPrice and mPositionFOC collection respectively
                            //Point 1):-If departures are deleted then delete the Price and FOC from mPositionPrice and mPositionFOC collection respectively 
                            //Point 2):-If departures are added then add new departures in mPositionPrice ,mPositionFOC collections respectively
                            if (req.DepartureDates?.Count > 0)
                            {
                                var posids = lstPositions.Select(a => a.PositionId).ToList();
                                var mPositionPriceList = _MongoContext.mPositionPrice.AsQueryable().Where(a => posids.Contains(a.PositionId) && a.IsDeleted == false).ToList();
                                var mPositionFOCList = _MongoContext.mPositionFOC.AsQueryable().Where(a => posids.Contains(a.PositionId) && a.IsDeleted == false).ToList();

                                //Point 1) deletion code
                                var deprids = req.DepartureDates.Where(a => a.IsDeleted).Select(a => a.Departure_Id).ToList();
                                if (deprids.Count > 0)
                                {
                                    foreach (var item in lstPositions)
                                    {
                                        var objmPositionPrice = await _MongoContext.mPositionPrice.UpdateManyAsync(m => m.QRFID == req.QRFID && m.PositionId == item.PositionId && deprids.Contains(m.DepartureId),
                                            Builders<mPositionPrice>.Update.Set(m => m.IsDeleted, true).Set(m => m.EditUser, req.UserEmail).Set(m => m.EditDate, DateTime.Now));

                                        var objmPositionFOC = await _MongoContext.mPositionFOC.UpdateManyAsync(m => m.QRFID == req.QRFID && m.PositionId == item.PositionId && deprids.Contains(m.DepartureId),
                                           Builders<mPositionFOC>.Update.Set(m => m.IsDeleted, true).Set(m => m.EditUser, req.UserEmail).Set(m => m.EditDate, DateTime.Now));
                                    }
                                }
                            }

                            //Point 2) Insertion Code                            
                            if (req.DepartureDatesNew.Count > 0)
                            {
                                result.Departures = req.DepartureDatesNew;
                                result.PaxSlabDetails.PaxSlabs = result.PaxSlabDetails.PaxSlabs.Where(a => a.IsDeleted == false).ToList();
                                responseStatus = await InsertPriceFOC(lstPositions, result, req.UserEmail);
                            }
                            #endregion

                            #region PaxSlab:- Update,Delete and Insert Price and FOC in mPositionPrice and mPositionFOC collection respectively and mQuote->TourEntity
                            //Point 1):-If PaxSlab are deleted then marked as delete the Price and FOC in mPositionPrice and mPositionFOC collection respectivly and also delete the pax slab from Tourentities
                            //Point 2):-If PaxSlab are modified then update the Price and FOC in mPositionPrice and mPositionFOC collection respectivly and also update pax slab in mQuote->TourEntity
                            //Point 3):-If PaxSlab are added then add new PaxSlab in mPositionPrice ,mPositionFOC collections respectively
                            if (req.PaxSlabs?.Count > 0)
                            {
                                var posids = lstPositions.Select(a => a.PositionId).ToList();
                                var mPositionPriceList = _MongoContext.mPositionPrice.AsQueryable().Where(a => posids.Contains(a.PositionId) && a.IsDeleted == false).ToList();
                                var mPositionFOCList = _MongoContext.mPositionFOC.AsQueryable().Where(a => posids.Contains(a.PositionId) && a.IsDeleted == false).ToList();

                                //Point 1) deletion code
                                var PaxSlab_Ids = req.PaxSlabs.Where(a => a.IsDeleted).Select(a => a.PaxSlab_Id).ToList();
                                if (PaxSlab_Ids.Count > 0)
                                {
                                    foreach (var item in lstPositions)
                                    {
                                        var objmPositionPrice = await _MongoContext.mPositionPrice.UpdateManyAsync(m => m.QRFID == req.QRFID && m.PositionId == item.PositionId && PaxSlab_Ids.Contains(m.PaxSlabId),
                                            Builders<mPositionPrice>.Update.Set(m => m.IsDeleted, true).Set(m => m.EditUser, req.UserEmail).Set(m => m.EditDate, DateTime.Now));

                                        var objmPositionFOC = await _MongoContext.mPositionFOC.UpdateManyAsync(m => m.QRFID == req.QRFID && m.PositionId == item.PositionId && PaxSlab_Ids.Contains(m.PaxSlabId),
                                           Builders<mPositionFOC>.Update.Set(m => m.IsDeleted, true).Set(m => m.EditUser, req.UserEmail).Set(m => m.EditDate, DateTime.Now));
                                    }
                                    if (result.TourEntities?.Count > 0)
                                    {
                                        result.TourEntities.Where(a => PaxSlab_Ids.Contains(a.PaxSlabID)).ToList().ForEach(a =>
                                        {
                                            a.IsDeleted = true;
                                            a.EditDate = DateTime.Now; a.EditUser = req.UserEmail;
                                        });

                                        var objmQuote = await _MongoContext.mQuote.UpdateOneAsync(m => m.QRFID == req.QRFID, Builders<mQuote>.Update.Set(m => m.TourEntities, result.TourEntities));
                                    }
                                }

                                //Point 2) Updation Code
                                PaxSlab_Ids = req.PaxSlabs.Where(a => a.IsDeleted == false).Select(a => a.PaxSlab_Id).ToList();
                                if (PaxSlab_Ids.Count > 0)
                                {
                                    var listPosPaxIds = mPositionPriceList.Where(a => PaxSlab_Ids.Contains(a.PaxSlabId)).Select(a => new { a.PaxSlabId, a.PositionId }).Distinct().ToList();
                                    listPosPaxIds.AddRange(mPositionFOCList.Where(a => PaxSlab_Ids.Contains(a.PaxSlabId)).Select(a => new { a.PaxSlabId, a.PositionId }).Distinct().ToList());
                                    listPosPaxIds = listPosPaxIds.Distinct().ToList();

                                    foreach (var item in listPosPaxIds)
                                    {
                                        var pax = req.PaxSlabs.Where(a => a.PaxSlab_Id == item.PaxSlabId).FirstOrDefault();
                                        var paxslab = pax.From.ToString() + " - " + pax.To.ToString();

                                        var objmPositionPrice = await _MongoContext.mPositionPrice.UpdateManyAsync(m => m.QRFID == req.QRFID && m.PositionId == item.PositionId && m.PaxSlabId == item.PaxSlabId,
                                            Builders<mPositionPrice>.Update.Set(m => m.PaxSlab, paxslab).Set(m => m.EditUser, req.UserEmail).Set(m => m.EditDate, DateTime.Now));

                                        var objmPositionFOC = await _MongoContext.mPositionFOC.UpdateManyAsync(m => m.QRFID == req.QRFID && m.PositionId == item.PositionId && m.PaxSlabId == item.PaxSlabId,
                                           Builders<mPositionFOC>.Update.Set(m => m.PaxSlab, paxslab).Set(m => m.EditUser, req.UserEmail).Set(m => m.EditDate, DateTime.Now));

                                        if (result.TourEntities?.Count > 0)
                                        {
                                            result.TourEntities.Where(a => a.PaxSlabID == item.PaxSlabId).FirstOrDefault().PaxSlab = paxslab;
                                        }
                                    }
                                    if (result.TourEntities?.Count > 0)
                                    {
                                        var objmQuote = await _MongoContext.mQuote.UpdateOneAsync(m => m.QRFID == req.QRFID, Builders<mQuote>.Update.Set(m => m.TourEntities, result.TourEntities));
                                    }
                                }
                            }

                            //Point 3) Insertion Code
                            if (req.PaxSlabsNew.Count > 0)
                            {
                                result.PaxSlabDetails.PaxSlabs = req.PaxSlabsNew;
                                result.Departures = result.Departures.Where(a => a.IsDeleted == false).ToList();
                                responseStatus = await InsertPriceFOC(lstPositions, result, req.UserEmail);
                            }
                            #endregion
                        }
                        else
                        {
                            responseStatus.Status = "Failure";
                            responseStatus.ErrorMessage = "Details not found in mPosition.";
                        }
                    }
                    else
                    {
                        responseStatus.Status = "Failure";
                        responseStatus.ErrorMessage = "QRFID not exists in mQuote.";
                    }
                }
                else
                {
                    responseStatus.Status = "Failure";
                    responseStatus.ErrorMessage = "PaxDetailsSetRequest can not be null.";
                }
            }
            catch (Exception ex)
            {
                responseStatus.Status = "Failure";
                responseStatus.ErrorMessage = ex.Message;
            }
            return responseStatus;
        }

        public async Task<ResponseStatus> InsertPriceFOC(List<mPosition> lstPositions, mQuote result, string UserEmail)
        {
            ResponseStatus objResponseStatus = new ResponseStatus();
            try
            {
                mPositionPrice objPricesInfo = new mPositionPrice();
                List<mPositionPrice> lstPricesInfo = new List<mPositionPrice>();
                mPositionFOC objFOCInfo = new mPositionFOC();
                List<mPositionFOC> lstFOCInfo = new List<mPositionFOC>();
                List<ProductRangeInfo> prodrange = new List<ProductRangeInfo>();
                var supplierids = new List<string>();
                var curids = new List<string>();
                var currencyidlist = new List<mProductSupplier>();
                var currencylist = new List<mCurrency>();
                var roomingcrossposnone = new List<RoomDetailsInfo>();
                var roomDetailsList = new List<RoomDetailsInfo>();
                List<string> RangeIdList = new List<string>();
                string supplierId = "";
                var currencyId = "";
                var currency = "";

                var positionids = lstPositions.Select(a => a.PositionId).ToList();
                var lstProductList = lstPositions.Select(a => a.ProductID).Distinct().ToList();
                var roomDetailsInfoList = lstPositions.Select(a => a.RoomDetailsInfo).ToList();

                var resultPosPrices = _MongoContext.mPositionPrice.AsQueryable().Where(a => a.QRFID == result.QRFID && positionids.Contains(a.PositionId) && a.IsDeleted == false).ToList();

                foreach (var itemPC in roomDetailsInfoList)
                {
                    roomDetailsList.AddRange(itemPC);
                }

                lstPositions.ForEach(a => RangeIdList.AddRange(a.RoomDetailsInfo.Where(b => b.IsDeleted == false).Select(b => b.ProductRangeId).ToList()));

                prodrange = _MongoContext.mProductRange.AsQueryable().Where(a => RangeIdList.Contains(a.VoyagerProductRange_Id))
                           .Select(a => new ProductRangeInfo
                           {
                               VoyagerProductRange_Id = a.VoyagerProductRange_Id,
                               ProductRangeCode = a.ProductRangeCode,
                               ProductType_Id = a.ProductType_Id,
                               PersonType = a.PersonType,
                               ProductMenu = a.ProductMenu
                           }).ToList();

                supplierids = lstPositions.Select(a => a.SupplierId).ToList();
                currencyidlist = _MongoContext.mProductSupplier.AsQueryable().Where(s => supplierids.Contains(s.Company_Id)).ToList();

                curids = currencyidlist.Select(a => a.Currency_Id).ToList();
                currencylist = _MongoContext.mCurrency.AsQueryable().Where(c => curids.Contains(c.VoyagerCurrency_Id)).ToList();

                result.TourEntities.ForEach(a => a.Flag = ((a.Type.Contains("Coach") || a.Type.Contains("LDC")) ? "DRIVER" : a.Type.Contains("Guide") ? "GUIDE" : a.Flag));

                for (int p = 0; p < lstPositions.Count; p++)
                {
                    supplierId = lstPositions[p].SupplierId;
                    int addDaysToPeriod = 0;
                    addDaysToPeriod = (lstPositions[p].DayNo - 1) + (lstPositions[p].Duration - 1);
                    currencyId = currencyidlist.Where(s => s.Company_Id == supplierId && s.Product_Id == lstPositions[p].ProductID).Select(a => a.Currency_Id).FirstOrDefault();
                    currency = currencylist.Where(c => c.VoyagerCurrency_Id == currencyId).Select(a => a.Currency).FirstOrDefault();

                    roomingcrossposnone = lstPositions[p].RoomDetailsInfo.Where(a => string.IsNullOrEmpty(a.CrossPositionId) && a.IsDeleted == false).ToList();

                    for (int i = 0; i < result.Departures.Count; i++)
                    {
                        //below code for rooming details without having TourEntity Cross PositionId
                        if (roomingcrossposnone != null && roomingcrossposnone.Count > 0)
                        {
                            for (int j = 0; j < result.PaxSlabDetails.PaxSlabs.Count; j++)
                            {
                                for (int k = 0; k < roomingcrossposnone.Count; k++)
                                {
                                    //Price added in objPricesInfo
                                    objPricesInfo = new mPositionPrice
                                    {
                                        QRFID = result.QRFID,
                                        PositionId = lstPositions[p].PositionId,
                                        DepartureId = result.Departures[i].Departure_Id,
                                        Period = result.Departures[i].Date,
                                        ContractPeriod = result.Departures[i].Date,
                                        PaxSlabId = result.PaxSlabDetails.PaxSlabs[j].PaxSlab_Id,
                                        PaxSlab = result.PaxSlabDetails.PaxSlabs[j].From.ToString() + " - " + result.PaxSlabDetails.PaxSlabs[j].To.ToString(),
                                        SupplierId = supplierId,
                                        Supplier = lstPositions[p].SupplierName,
                                        RoomId = roomingcrossposnone[k].RoomId,
                                        IsSupplement = roomingcrossposnone[k].IsSupplement,
                                        ProductCategoryId = roomingcrossposnone[k].ProductCategoryId,
                                        ProductRangeId = roomingcrossposnone[k].ProductRangeId,
                                        ProductCategory = roomingcrossposnone[k].ProductCategory,
                                        ProductRange = roomingcrossposnone[k].ProductRange,
                                        BuyCurrencyId = currencyId,
                                        BuyCurrency = currency,
                                        CreateDate = DateTime.Now,
                                        CreateUser = UserEmail,
                                        EditUser = null,
                                        EditDate = null,
                                        PositionPriceId = Guid.NewGuid().ToString()
                                    };
                                    if (addDaysToPeriod > 0)
                                        objPricesInfo.ContractPeriod = Convert.ToDateTime(objPricesInfo.ContractPeriod).AddDays(addDaysToPeriod);

                                    objPricesInfo.Type = prodrange.Where(a => a.VoyagerProductRange_Id == objPricesInfo.ProductRangeId).Select(b => b.PersonType).FirstOrDefault();
                                    objPricesInfo.ProductRangeCode = prodrange.Where(a => a.VoyagerProductRange_Id == objPricesInfo.ProductRangeId).Select(b => b.ProductRangeCode).FirstOrDefault();
                                    lstPricesInfo.Add(objPricesInfo);

                                    //FOC added in objFOCInfo
                                    objFOCInfo = new mPositionFOC
                                    {
                                        QRFID = result.QRFID,
                                        PositionId = lstPositions[p].PositionId,
                                        DepartureId = result.Departures[i].Departure_Id,
                                        Period = result.Departures[i].Date,
                                        ContractPeriod = result.Departures[i].Date,
                                        PaxSlabId = result.PaxSlabDetails.PaxSlabs[j].PaxSlab_Id,
                                        PaxSlab = result.PaxSlabDetails.PaxSlabs[j].From.ToString() + " - " + result.PaxSlabDetails.PaxSlabs[j].To.ToString(),
                                        CityId = lstPositions[p].CityID,
                                        CityName = lstPositions[p].CityName,
                                        ProductId = lstPositions[p].ProductID,
                                        ProductName = lstPositions[p].ProductName,
                                        SupplierId = supplierId,
                                        Supplier = lstPositions[p].SupplierName,
                                        RoomId = roomingcrossposnone[k].RoomId,
                                        IsSupplement = roomingcrossposnone[k].IsSupplement,
                                        ProductCategoryId = roomingcrossposnone[k].ProductCategoryId,
                                        ProductRangeId = roomingcrossposnone[k].ProductRangeId,
                                        ProductCategory = roomingcrossposnone[k].ProductCategory,
                                        ProductRange = roomingcrossposnone[k].ProductRange,
                                        Quantity = result.PaxSlabDetails.PaxSlabs[j].From,
                                        CreateDate = DateTime.Now,
                                        CreateUser = UserEmail,
                                        EditUser = null,
                                        EditDate = null,
                                        PositionFOCId = Guid.NewGuid().ToString()
                                    };
                                    if (addDaysToPeriod > 0)
                                        objFOCInfo.ContractPeriod = Convert.ToDateTime(objFOCInfo.ContractPeriod).AddDays(addDaysToPeriod);
                                    objFOCInfo.Type = prodrange.Where(a => a.VoyagerProductRange_Id == objFOCInfo.ProductRangeId).Select(b => b.PersonType).FirstOrDefault();
                                    lstFOCInfo.Add(objFOCInfo);
                                }
                            }
                        }
                    }
                }

                //Price
                if (lstPricesInfo?.Count > 0)
                {
                    #region Get Contract Rates By Service
                    ProdContractGetRes prodContractGetRes = new ProdContractGetRes();
                    ProdContractGetReq prodContractGetReq = new ProdContractGetReq
                    {
                        QRFID = result.QRFID,
                        ProductIDList = lstProductList,
                        AgentId = result.AgentInfo.AgentID
                    };

                    var rangelist = lstPricesInfo.Select(c => c.ProductRangeId).ToList();
                    prodContractGetRes = _productRepository.GetContractRatesByProductID(prodContractGetReq, rangelist);
                    var prodid = "";

                    if (prodContractGetRes != null && prodContractGetRes.ProductContractInfo.Count > 0)
                    {
                        for (int i = 0; i < lstPricesInfo.Count; i++)
                        {
                            prodid = lstPositions.Where(a => a.PositionId == lstPricesInfo[i].PositionId).Select(a => a.ProductID).FirstOrDefault();
                            if (!string.IsNullOrEmpty(prodid))
                            {
                                var lstPCInfo = prodContractGetRes.ProductContractInfo.Where(a => a.SupplierId == lstPricesInfo[i].SupplierId && a.ProductId == prodid
                                  && a.ProductRangeId == lstPricesInfo[i].ProductRangeId
                                  && (a.FromDate <= lstPricesInfo[i].ContractPeriod && lstPricesInfo[i].ContractPeriod <= a.ToDate)).ToList();

                                if (lstPCInfo != null && lstPCInfo.Count > 0)
                                {
                                    foreach (var con in lstPCInfo)
                                    {
                                        char[] dayPattern = con.DayComboPattern.ToCharArray();
                                        int dayNo = (int)Convert.ToDateTime(lstPricesInfo[i].ContractPeriod).DayOfWeek;

                                        if (dayNo == 0)
                                            dayNo = 7;

                                        if (dayPattern[dayNo - 1] == '1')
                                        {
                                            lstPricesInfo[i].ContractId = con.ContractId;
                                            lstPricesInfo[i].ContractPrice = Convert.ToDouble(con.Price);
                                            lstPricesInfo[i].BudgetPrice = Convert.ToDouble(con.Price);
                                            lstPricesInfo[i].BuyCurrencyId = con.CurrencyId;
                                            lstPricesInfo[i].BuyCurrency = con.Currency;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    #region GetMarkupValue and Add in Contract price
                    bool IsSalesOfficeUser = _genericRepository.IsSalesOfficeUser(UserEmail);
                    if (IsSalesOfficeUser == true)
                    {
                        var UserCompany_Id = _MongoContext.mUsers.AsQueryable().Where(x => x.UserName.ToLower() == UserEmail.ToLower().Trim()).Select(y => y.Company_Id).FirstOrDefault();
                        var Markup_Id = _MongoContext.mCompanies.AsQueryable().Where(x => x.Company_Id == UserCompany_Id && x.Markups.Any(y => y.Markup_For == "Groups")).FirstOrDefault().Markups.FirstOrDefault().Markup_Id;

                        if (!string.IsNullOrEmpty(Markup_Id))
                        {
                            for (int i = 0; i < lstPricesInfo.Count; i++)
                            {
                                if (!string.IsNullOrEmpty(lstPricesInfo[i].ContractId))
                                {
                                    ProdMarkupsGetReq prodMarkupsGetReq = new ProdMarkupsGetReq();

                                    prodMarkupsGetReq.MarkupsId = Markup_Id;
                                    prodMarkupsGetReq.ProductType = lstPositions.Where(a => a.PositionId == lstPricesInfo[i].PositionId).Select(b => b.ProductType).FirstOrDefault();
                                    var MarkupDetails = _productRepository.GetProdMarkups(prodMarkupsGetReq).Result;

                                    if (MarkupDetails != null)
                                    {
                                        double MarkupValue = Convert.ToDouble(MarkupDetails.PercMarkUp) <= 0 ? Convert.ToDouble(MarkupDetails.FixedMarkUp) : Convert.ToDouble(MarkupDetails.PercMarkUp);

                                        if (MarkupDetails.MARKUPTYPE == "Fixed")
                                        {
                                            double markup = MarkupValue;
                                            if (MarkupDetails.CURRENCY_ID != lstPricesInfo[i].BuyCurrencyId)
                                            {
                                                var rate = _genericRepository.getExchangeRate(MarkupDetails.CURRENCY_ID, lstPricesInfo[i].BuyCurrencyId, result.QRFID);
                                                if (rate != null)
                                                    markup = MarkupValue * Convert.ToDouble(rate.Value);
                                            }
                                            if (markup > 0)
                                                lstPricesInfo[i].BudgetPrice = lstPricesInfo[i].BudgetPrice + Math.Round(markup, 2);
                                        }
                                        else
                                        {
                                            lstPricesInfo[i].BudgetPrice = lstPricesInfo[i].BudgetPrice + (lstPricesInfo[i].BudgetPrice * MarkupValue / 100);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    #region Save Currency into mPosition
                    var objPosition = new mPosition();
                    var PosPriceCur = new mPositionPrice();
                    var modelsPosCur = new WriteModel<mPosition>[lstPositions.ToList().Count];
                    var curposids = lstPositions.Where(a => a.IsDeleted == false).Select(a => a.PositionId).ToList();
                    List<mPosition> lstCurPosition = _MongoContext.mPosition.AsQueryable().Where(a => curposids.Contains(a.PositionId) && a.IsDeleted == false).ToList();

                    for (int p = 0; p < lstPositions.Count; p++)
                    {
                        objPosition = new mPosition();
                        objPosition = lstCurPosition.Where(a => a.PositionId == lstPositions[p].PositionId).FirstOrDefault();
                        PosPriceCur = lstPricesInfo.Where(a => a.PositionId == lstPositions[p].PositionId).FirstOrDefault();
                        objPosition.BuyCurrencyId = PosPriceCur.BuyCurrencyId;
                        objPosition.BuyCurrency = PosPriceCur.BuyCurrency;
                        objPosition.EditDate = DateTime.Now;
                        modelsPosCur[p] = new ReplaceOneModel<mPosition>(new BsonDocument("PositionId", objPosition.PositionId), objPosition) { IsUpsert = true };
                    }
                    var BulkWriteRes = await _MongoContext.mPosition.BulkWriteAsync(modelsPosCur);
                    #endregion

                    #region Set PositionPrice 
                    //the below code is for if from frontend existing Dropdown of Services is changed then deactive the service and after that insert it.dectivation code is above 
                    if (lstPricesInfo?.Count > 0)
                    {
                        await _MongoContext.mPositionPrice.InsertManyAsync(lstPricesInfo);
                        objResponseStatus.Status = "Success";
                        objResponseStatus.ErrorMessage = "Saved Successfully.";
                    }
                    else
                    {
                        objResponseStatus.Status = "Failure";
                        objResponseStatus.ErrorMessage = "PositionPrice details not found for insertion.";
                    }
                    #endregion
                }

                //FOC
                if (lstFOCInfo?.Count > 0)
                {
                    #region Get ContractId from mPositionPrice                       
                    var resultPosPrice = _MongoContext.mPositionPrice.AsQueryable().Where(a => a.QRFID == result.QRFID && positionids.Contains(a.PositionId) && a.IsDeleted == false).ToList();
                    if (resultPosPrice != null && resultPosPrice.Count > 0)
                    {
                        for (int i = 0; i < lstFOCInfo.Count; i++)
                        {
                            var pospr = resultPosPrice.Where(a => a.PositionId == lstFOCInfo[i].PositionId && a.QRFID == lstFOCInfo[i].QRFID
                            && a.DepartureId == lstFOCInfo[i].DepartureId && a.PaxSlabId == lstFOCInfo[i].PaxSlabId
                            && a.RoomId == lstFOCInfo[i].RoomId).FirstOrDefault();

                            if (pospr != null)
                            {
                                lstFOCInfo[i].ContractId = pospr.ContractId;
                            }
                        }
                    }
                    #endregion

                    #region Get FOC Quantity mProductFreePlacePolicy
                    var resultProdFPP = _MongoContext.mProductFreePlacePolicy.AsQueryable().Where(a => lstProductList.Contains(a.Product_Id)).ToList();
                    if (resultProdFPP != null && resultProdFPP.Count > 0)
                    {
                        for (int i = 0; i < lstFOCInfo.Count; i++)
                        {
                            var posFPP = resultProdFPP.Where(a => a.Product_Id == lstFOCInfo[i].ProductId).ToList();
                            if (posFPP != null && posFPP.Count > 0)
                            {
                                for (int j = 0; j < posFPP.Count; j++)
                                {
                                    string[] paxSlab = lstFOCInfo[i].PaxSlab.Split(" - ");
                                    if (
                                        lstFOCInfo[i].ContractId == posFPP[j].ProductContract_Id &&
                                        Convert.ToString(lstFOCInfo[i].ProductRange.Replace("(" + lstFOCInfo[i].Type.ToUpper() + ")", "").ToLower()).Trim() == Convert.ToString(posFPP[j].Subprod.ToLower()).Trim() &&
                                        (posFPP[j].DateMin <= lstFOCInfo[i].Period && lstFOCInfo[i].Period <= posFPP[j].DateMax) &&
                                        (Convert.ToInt16(paxSlab[0]) <= posFPP[j].MinPers && posFPP[j].MinPers <= Convert.ToInt16(paxSlab[0]))
                                        )
                                    {
                                        lstFOCInfo[i].FOCQty = posFPP[j].Quantity;
                                        break;
                                    }
                                }
                            }
                        }

                    }
                    #endregion

                    #region insert new FOC details 
                    if (lstFOCInfo?.Count > 0)
                    {
                        await _MongoContext.mPositionFOC.InsertManyAsync(lstFOCInfo);
                        objResponseStatus.Status = "Success";
                        objResponseStatus.ErrorMessage = "Saved Successfully.";
                    }
                    else
                    {
                        objResponseStatus.Status = "Failure";
                        objResponseStatus.ErrorMessage = "PositionFOC details not found for insertion.";
                    }

                    #endregion
                }
            }
            catch (Exception ex)
            {
                objResponseStatus.Status = "Failure";
                objResponseStatus.ErrorMessage = ex.Message;
            }

            return objResponseStatus;
        }

        public void UpdateBulkPositionPrice(List<WriteModel<mPositionPrice>> lst)
        {
            _MongoContext.mPositionPrice.BulkWrite(lst);
        }
        #endregion

        #region LinkedQRFs
        public async Task<bool> ChcekLinkedQRFsExist(string QRFID)
        {
            var quote = await _MongoContext.mQuote.FindAsync(a => a.QRFID == QRFID).Result.FirstOrDefaultAsync();
            if (quote == null)
                return false;
            if (string.IsNullOrEmpty(quote.LatestChild_QRFID) && string.IsNullOrEmpty(quote.Parent_QRFID))
                return false;
            else
                return true;
        }

        public async Task<List<LinkedQRFsData>> GetLinkedQRFs(LinkedQRFsGetReq request)
        {
            var result = new List<LinkedQRFsData>();
            var quote = await _MongoContext.mQuote.FindAsync(a => a.QRFID == request.QRFID).Result.FirstOrDefaultAsync();
            result.Add(new LinkedQRFsData
            {
                QRFID = quote.QRFID,
                AgentName = quote.AgentInfo.AgentName,
                CurrentPipeline = quote.CurrentPipeline,
                TourName = quote.AgentProductInfo.TourName,
                Duration = quote.AgentProductInfo.Duration,
                Parent_QRFID = quote.Parent_QRFID,
                CreateDate = quote.CreateDate
            });

            var quoteListParent = await _MongoContext.mQuote.FindAsync(a => a.Parent_QRFID == request.QRFID).Result.ToListAsync();
            result.AddRange(quoteListParent.Select(a => new LinkedQRFsData
            {
                QRFID = a.QRFID,
                AgentName = a.AgentInfo.AgentName,
                CurrentPipeline = a.CurrentPipeline,
                TourName = a.AgentProductInfo.TourName,
                Duration = a.AgentProductInfo.Duration,
                Parent_QRFID = a.Parent_QRFID,
                CreateDate = a.CreateDate
            }).ToList());

            if (!string.IsNullOrEmpty(quote.Parent_QRFID))
            {
                var quotePar = await _MongoContext.mQuote.FindAsync(a => a.QRFID == quote.Parent_QRFID).Result.FirstOrDefaultAsync();
                result.Add(new LinkedQRFsData
                {
                    QRFID = quotePar.QRFID,
                    AgentName = quotePar.AgentInfo.AgentName,
                    CurrentPipeline = quotePar.CurrentPipeline,
                    TourName = quotePar.AgentProductInfo.TourName,
                    Duration = quotePar.AgentProductInfo.Duration,
                    Parent_QRFID = quotePar.Parent_QRFID,
                    CreateDate = quotePar.CreateDate
                });

                var quoteParListParent = await _MongoContext.mQuote.FindAsync(a => a.Parent_QRFID == quotePar.QRFID && a.QRFID != request.QRFID).Result.ToListAsync();
                result.AddRange(quoteParListParent.Select(a => new LinkedQRFsData
                {
                    QRFID = a.QRFID,
                    AgentName = a.AgentInfo.AgentName,
                    CurrentPipeline = a.CurrentPipeline,
                    TourName = a.AgentProductInfo.TourName,
                    Duration = a.AgentProductInfo.Duration,
                    Parent_QRFID = a.Parent_QRFID,
                    CreateDate = a.CreateDate
                }).ToList());
            }

            return result = result.Distinct().OrderByDescending(a => a.CreateDate).ToList();
        }
        #endregion

        #region QuoteRejectOpportunity
        public async Task<CommonResponse> SetQuoteRejectOpportunity(QuoteRejectOpportunityReq req)
        {
            var response = new CommonResponse();
            try
            {
                var quote = _MongoContext.mQuote.AsQueryable().Where(a => a.QRFID == req.QRFID).FirstOrDefault();
                if (quote != null)
                {
                    var resultFlag = await _MongoContext.mQuote.UpdateOneAsync(Builders<mQuote>.Filter.Eq("QRFID", req.QRFID),
                                     Builders<mQuote>.Update.Set("CurrentPipeline", "Rejected Pipeline").Set("CurrentPipelineStep", "").Set("CurrentPipelineSubStep", "")
                                     .Set("EditUser", req.EditUser).Set("EditDate", DateTime.Now));

                    response.ResponseStatus.Status = "Success";
                }
                else
                {
                    response.ResponseStatus.Status = "Failed";
                    response.ResponseStatus.ErrorMessage = "Invalid QRF";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.ResponseStatus.Status = "Failed";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }
            return response;
        }
        #endregion

        #region 3rd party QuoteRejectOpportunity
        public async Task<OpportunityPartnerRes> SetPartnerQuoteRejectOpportunity(ManageOpportunityReq req)
        {
            OpportunityPartnerRes response = new OpportunityPartnerRes();

            try
            {
                var quote = _MongoContext.mQuote.AsQueryable().Where(a => a.QRFID == req.OpportunityInfo.OpportunityId).FirstOrDefault();
                if (quote != null)
                {
                    if (quote.Mappings != null && quote.Mappings.Any())
                    {
                        QuoteMappings updateQuoteMapping = new QuoteMappings();
                        if (quote.CurrentPipeline == _configuration.GetValue<string>("PipeLines:Quote") || quote.CurrentPipeline == _configuration.GetValue<string>("PipeLines:Costing")
                            || quote.CurrentPipeline == _configuration.GetValue<string>("PipeLines:CostingApproval") || quote.CurrentPipeline == _configuration.GetValue<string>("PipeLines:Amendment"))
                        {
                            updateQuoteMapping = quote.Mappings.Where(a => a.Application.ToLower() == req.CredentialInfo.Source.ToLower() && a.PartnerEntityType == _configuration.GetValue<string>("MappingDefault:PartnerEntityTypeOpportunity")).FirstOrDefault();
                        }
                        else if (quote.CurrentPipeline == _configuration.GetValue<string>("PipeLines:AgentApproval"))
                        {
                            updateQuoteMapping = quote.Mappings.Where(a => a.Application.ToLower() == req.CredentialInfo.Source.ToLower() && a.PartnerEntityType == _configuration.GetValue<string>("MappingDefault:PartnerEntityTypeQuote")).FirstOrDefault();
                        }
                        if (!string.IsNullOrEmpty(req.OpportunityInfo.Status) && string.Compare(req.OpportunityInfo.Status, "Rejected", true) == 0)
                        {
                            updateQuoteMapping.Status = req.OpportunityInfo.Status;
                            updateQuoteMapping.AdditionalInfoType = _configuration.GetValue<string>("MappingDefault:AdditionalInfoTypeReason");
                            updateQuoteMapping.AdditionalInfo = req.OpportunityInfo.Reason;

                            var resultFlag = await _MongoContext.mQuote.UpdateOneAsync(Builders<mQuote>.Filter.Eq("QRFID", req.OpportunityInfo.OpportunityId),
                                             Builders<mQuote>.Update.Set("CurrentPipeline", "Rejected Pipeline").Set("CurrentPipelineStep", "").Set("CurrentPipelineSubStep", "")
                                             .Set("EditUser", req.CreatedUser).Set("EditDate", DateTime.Now).Set("Mappings", quote.Mappings));

                            response.ResponseStatus.Status = "Success";
                            response.ResponseStatus.ErrorMessage = "Opportunity info saved successfully.";
                        }
                        else
                        {
                            response.ResponseStatus.Status = "Failed";
                            response.ResponseStatus.ErrorMessage = "Action not found.";
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failed";
                        response.ResponseStatus.ErrorMessage = "Opportunity doesn't mapped to any Partner Entity.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failed";
                    response.ResponseStatus.ErrorMessage = "Invalid Opportunity Id";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.ResponseStatus.Status = "Failed";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }

            return response;
        }
        #endregion

        #region 3rd party Search Quote Details

        /// <summary>
        /// GetPartnerQuoteDetails used for getting fetch QRFID VGER-mongodb (mQuote) based on "PartnerEntityCode" and "Application" provided by any 3rd party
        /// </summary>
        /// <param name="request"></param>
        /// <returns>
        /// Fetch QRFID from VGER-mongodb (mQuote)
        /// </returns>
        public async Task<QuoteThirdPartyGetRes> GetPartnerQuoteDetails(QuoteThirdPartyGetReq request)
        {
            QuoteThirdPartyGetRes response = new QuoteThirdPartyGetRes();

            try
            {
                if (string.IsNullOrEmpty(request.QrfID))
                {
                    var data = _MongoContext.mQuote.AsQueryable().Where(a => a.Mappings != null && a.Mappings.Any(b => b.PartnerEntityCode == request.PartnerEntityCode && b.Application.ToLower() == request.Application.ToLower())).FirstOrDefault();
                    response.QRFID = data != null && data.QRFID != null ? data.QRFID : "";
                    response.CurrentPipeline = data != null && data.QRFID != null ? data.CurrentPipeline : "";

                }
                else
                {
                    var data = _MongoContext.mQuote.AsQueryable().Where(a => a.QRFID == request.QrfID).FirstOrDefault();
                    if (data != null && string.Compare(data.CurrentPipeline, _configuration.GetValue<string>("PipeLines:Quote"), true) == 0)
                    {
                        var mapping = data.Mappings.Where(x => x.Application.ToLower() == _configuration.GetValue<string>("Application:Application Name").ToLower() && x.PartnerEntityName.ToLower() == _configuration.GetValue<string>("PartnerEntityName:Quote Pipeline").ToLower()).FirstOrDefault();
                        response.PartnerEntityCode = mapping != null ? mapping.PartnerEntityCode : null;
                        response.PartnerEntityName = mapping != null ? mapping.PartnerEntityName : null;
                        response.ApplicationName = mapping != null ? mapping.Application : null;
                        response.CurrentPipeline = data != null ? data.CurrentPipeline : "";

                    }
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }

        #endregion

        #region QRF Prices
        //public async Task<List<QRFPricesInfo>> GetQRFPrices(QRFPricesGetReq request)
        //{
        //    List<QRFPricesInfo> response = new List<QRFPricesInfo>();
        //    List<MealDetails> resultMeals = new List<MealDetails>();
        //    String supplierId = "";

        //    var builder = Builders<mQuote>.Filter;
        //    var filter = builder.Where(q => q.QRFID == request.QRFId);
        //    mQuote result = await _MongoContext.mQuote.Find(filter).Project(q => new mQuote
        //    {
        //        Departures = q.Departures,
        //        PaxSlabDetails = q.PaxSlabDetails,
        //        AgentPassengerInfo = q.AgentPassengerInfo
        //    }).FirstOrDefaultAsync(); 

        //    if (request.Type.ToLower() == "meal")
        //    {
        //        VenueTypes objVenueTypes = new VenueTypes();
        //        string[] rowid = request.RowId.Split("|");
        //        if (rowid != null && rowid.Count() > 1)
        //        {
        //            resultMeals = _MongoContext.mPosition.AsQueryable().Where(a => a.QRFID == request.QRFId).Select(a => a.Meals).FirstOrDefault();
        //            if (resultMeals != null)
        //            {
        //                objVenueTypes = resultMeals.Where(a => a.MealID == Convert.ToInt64(rowid[0])).FirstOrDefault().VenueTypes.Where(v => v.VenueDetails.VenueDetailsId == Convert.ToInt64(rowid[1])).FirstOrDefault();
        //                if (objVenueTypes != null)
        //                {
        //                    supplierId = objVenueTypes.VenueDetails.SupplementID;
        //                    var resultpersontype = _MongoContext.mProductRange.AsQueryable().Where(a => a.VoyagerProductRange_Id == objVenueTypes.VenueDetails.MealDescriptionID).Select(b => b.PersonType).FirstOrDefault();
        //                    for (int i = 0; i < result.Departures.Count; i++)
        //                    {
        //                        for (int j = 0; j < result.PaxSlabDetails.PaxSlabs.Count; j++)
        //                        {
        //                            var objQRFPricesInfo = new QRFPricesInfo();

        //                            objQRFPricesInfo.QRFPriceId = request.QRFId + "|" + result.Departures[i].Departure_Id + "|" + result.PaxSlabDetails.PaxSlabs[j].PaxSlab_Id + "|" + objVenueTypes.VenueDetails.MealDescriptionID;
        //                            objQRFPricesInfo.Period = Convert.ToDateTime(result.Departures[i].Date).ToString("dd/MM/yyyy");
        //                            objQRFPricesInfo.PaxSlab = result.PaxSlabDetails.PaxSlabs[j].From.ToString() + " - " + result.PaxSlabDetails.PaxSlabs[j].To.ToString();

        //                            objQRFPricesInfo.Category = objVenueTypes.VenueDetails.MealType;
        //                            objQRFPricesInfo.For = objVenueTypes.VenueDetails.MealDescription;
        //                            objQRFPricesInfo.ProductCategoryId = objVenueTypes.VenueDetails.MealTypeID;
        //                            objQRFPricesInfo.ProductRangeId = objVenueTypes.VenueDetails.MealDescriptionID;

        //                            objQRFPricesInfo.Type = resultpersontype;
        //                            response.Add(objQRFPricesInfo);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    else if (request.Type.ToLower() == "activities")
        //    {
        //        ActivitiesProperties activitiesProperties = new ActivitiesProperties();
        //        var resultActivities = _MongoContext.mPosition.AsQueryable().Where(a => a.QRFID == request.QRFId).Select(a => a.Activities.ActivitiesDetails).FirstOrDefault();
        //        if (resultActivities != null)
        //        {
        //            activitiesProperties = resultActivities.Where(a => a.ActivityID == Convert.ToInt64(request.RowId)).FirstOrDefault();

        //            if (activitiesProperties != null)
        //            {
        //                supplierId = activitiesProperties.SupplementID;
        //                string VoyagerProductRange_Id;
        //                QRFPricesInfo objQRFPricesInfo;
        //                var ProdRangeList = _MongoContext.mProductRange.AsQueryable()
        //                    .Where(a => a.Product_Id == activitiesProperties.ProductID && a.ProductCategory_Id == activitiesProperties.TourType_Id
        //                    && a.ProductRangeCode == "TICKET").ToList();
        //                for (int i = 0; i < result.Departures.Count; i++)
        //                {
        //                    for (int j = 0; j < result.PaxSlabDetails.PaxSlabs.Count; j++)
        //                    {
        //                        foreach (AgentPassengerInfo PaxInfo in result.AgentPassengerInfo)
        //                        {
        //                            objQRFPricesInfo = new QRFPricesInfo();

        //                            objQRFPricesInfo.QRFPriceId = request.QRFId + "|" + result.Departures[i].Departure_Id + "|" + result.PaxSlabDetails.PaxSlabs[j].PaxSlab_Id + "|";
        //                            objQRFPricesInfo.Period = Convert.ToDateTime(result.Departures[i].Date).ToString("dd/MM/yyyy");
        //                            objQRFPricesInfo.PaxSlab = result.PaxSlabDetails.PaxSlabs[j].From.ToString() + " - " + result.PaxSlabDetails.PaxSlabs[j].To.ToString();

        //                            objQRFPricesInfo.Category = activitiesProperties.TourType;
        //                            objQRFPricesInfo.For = activitiesProperties.TicketType;
        //                            objQRFPricesInfo.ProductCategoryId = activitiesProperties.TourType_Id;
        //                            objQRFPricesInfo.ProductRangeId = activitiesProperties.TicketType_Id;

        //                            if (PaxInfo.Type == "ADULT")
        //                            {
        //                                VoyagerProductRange_Id = ProdRangeList.Where(a => a.PersonType == "ADULT").Select(b => b.VoyagerProductRange_Id).FirstOrDefault();
        //                                objQRFPricesInfo.ProductRangeId = VoyagerProductRange_Id;
        //                                objQRFPricesInfo.QRFPriceId += VoyagerProductRange_Id;
        //                                objQRFPricesInfo.Type = ProdRangeList.Where(a => a.PersonType == "ADULT").Select(b => b.PersonType).FirstOrDefault();
        //                                response.Add(objQRFPricesInfo);
        //                            }
        //                            else if (PaxInfo.Type == "CHILDWITHBED" || PaxInfo.Type == "CHILDWITHOUTBED" || PaxInfo.Type == "INFANT")
        //                            {
        //                                foreach (int age in PaxInfo.Age)
        //                                {
        //                                    foreach (var prodRange in ProdRangeList.Where(a => a.PersonType == "CHILD" || a.PersonType == "INFANT").ToList())
        //                                    {
        //                                        if (age >= Convert.ToInt32(prodRange.Agemin) && age <= Convert.ToInt32(prodRange.Agemax))
        //                                        {
        //                                            objQRFPricesInfo.ProductRangeId = prodRange.VoyagerProductRange_Id;
        //                                            objQRFPricesInfo.QRFPriceId += prodRange.VoyagerProductRange_Id;

        //                                            objQRFPricesInfo.Type = ProdRangeList.Where(a => a.VoyagerProductRange_Id == prodRange.VoyagerProductRange_Id).Select(b => b.PersonType).FirstOrDefault();
        //                                            if (response.Where(a => a.Period == objQRFPricesInfo.Period && a.PaxSlab == objQRFPricesInfo.PaxSlab && a.Type == objQRFPricesInfo.Type).Count() <= 0)
        //                                            {
        //                                                response.Add(objQRFPricesInfo);
        //                                                break;
        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    else if (request.Type.ToLower() == "accomodation")
        //    {
        //        AccomodationInfo objAccomodationInfo = new AccomodationInfo();
        //        var resultAcco = _MongoContext.mPosition.AsQueryable().Where(a => a.QRFID == request.QRFId).Select(a => a.AccomodationInfo).FirstOrDefault();
        //        if (resultAcco != null)
        //        {
        //            objAccomodationInfo = resultAcco.Where(a => a.AccomodationId == request.RowId).FirstOrDefault();

        //            supplierId = objAccomodationInfo.SupplementID;
        //            string[] RoomType;
        //            string[] RoomTypeID;

        //            //List<string> RangeIdList = new List<string>();
        //            for (int i = 0; i < result.Departures.Count; i++)
        //            {
        //                for (int j = 0; j < result.PaxSlabDetails.PaxSlabs.Count; j++)
        //                {
        //                    for (int k = 0; k < objAccomodationInfo.RoomDetailsInfo.Count; k++)
        //                    {
        //                        if (!(objAccomodationInfo.RoomDetailsInfo[k].IsDeleted))
        //                        {
        //                            var objQRFPricesInfo = new QRFPricesInfo();

        //                            objQRFPricesInfo.QRFPriceId = request.QRFId + "|" + result.Departures[i].Departure_Id + "|" + result.PaxSlabDetails.PaxSlabs[j].PaxSlab_Id + "|" + objAccomodationInfo.RoomDetailsInfo[k].RoomId;
        //                            objQRFPricesInfo.Period = Convert.ToDateTime(result.Departures[i].Date).ToString("dd/MM/yyyy");
        //                            objQRFPricesInfo.PaxSlab = result.PaxSlabDetails.PaxSlabs[j].From.ToString() + " - " + result.PaxSlabDetails.PaxSlabs[j].To.ToString();

        //                            if (!(string.IsNullOrEmpty(objAccomodationInfo.RoomDetailsInfo[k].RoomType)))
        //                            {
        //                                RoomType = objAccomodationInfo.RoomDetailsInfo[k].RoomType.Split(')');
        //                                objQRFPricesInfo.Category = RoomType[0].Replace("(", "").Trim();
        //                                objQRFPricesInfo.For = RoomType[1].Replace("(", "").Trim();
        //                            }
        //                            if (!(string.IsNullOrEmpty(objAccomodationInfo.RoomDetailsInfo[k].RoomTypeID)))
        //                            {
        //                                RoomTypeID = objAccomodationInfo.RoomDetailsInfo[k].RoomTypeID.Split('|');
        //                                objQRFPricesInfo.ProductCategoryId = RoomTypeID[0].Trim();
        //                                objQRFPricesInfo.ProductRangeId = RoomTypeID[1].Trim();
        //                                //RangeIdList.Add(RoomTypeID[1].Trim());
        //                            }
        //                            objQRFPricesInfo.Type = _MongoContext.mProductRange.AsQueryable().Where(a => a.VoyagerProductRange_Id == objQRFPricesInfo.ProductRangeId).Select(b => b.PersonType).FirstOrDefault();
        //                            response.Add(objQRFPricesInfo);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    //try
        //    //{
        //    //    var productRangeList = _MongoContext.mProductRange.AsQueryable().Where(a => a.ProductRangeName == "TICKET").Select(b => b).ToList();
        //    //    var productRangeList1 = _MongoContext.mProductRange.AsQueryable().Where(a => a.ProductRangeName != "TICKET").Select(b => b).ToList();
        //    //    //var result1 = productRangeList.Where(a => RangeIdList.Any(b => b == a.VoyagerProductRange_Id)).ToList();

        //    //}
        //    //catch (Exception e)
        //    //{

        //    //    throw;
        //    //}

        //    #region Get Contract Rates By Service

        //    ProdContractGetRes prodContractGetRes = new ProdContractGetRes();
        //    ProdContractGetReq prodContractGetReq = new ProdContractGetReq();
        //    prodContractGetReq.QRFID = request.QRFId;
        //    prodContractGetReq.ProductID = request.ProductID; //"8a098ef0-2a02-43a5-a713-e877b459093b";
        //    prodContractGetReq.RowId = request.RowId;
        //    prodContractGetReq.Type = request.Type;
        //    prodContractGetReq.SupplierId = supplierId;

        //    prodContractGetRes = _productRepository.GetContractRatesByProductID(prodContractGetReq, response);
        //    if (prodContractGetRes != null && prodContractGetRes.ProductContractInfo.Count > 0)
        //    {
        //        for (int i = 0; i < response.Count; i++)
        //        {
        //            for (int j = 0; j < prodContractGetRes.ProductContractInfo.Count; j++)
        //            {
        //                if (response[i].ProductRangeId == prodContractGetRes.ProductContractInfo[j].ProductRangeId)
        //                {
        //                    if (prodContractGetRes.ProductContractInfo[j].FromDate <= Convert.ToDateTime(response[i].Period) && Convert.ToDateTime(response[i].Period) <= prodContractGetRes.ProductContractInfo[j].ToDate)
        //                    {
        //                        response[i].ContractPrice = prodContractGetRes.ProductContractInfo[j].Price;
        //                        break;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    #endregion

        //    #region Get QT Rate From Saved Data

        //    var resultPosPrice = _MongoContext.mPositionPrice.AsQueryable().Where(a => a.QRFID == request.QRFId).ToList();
        //    if (resultPosPrice != null && resultPosPrice.Count > 0)
        //    {
        //        if (request.Type.ToLower() == "accomodation")
        //        {
        //            var accoQRFPrices = resultPosPrice.Select(r => r.AccomodationQRFPrices).FirstOrDefault();
        //            if (accoQRFPrices != null && accoQRFPrices.Count > 0)
        //            {
        //                List<QRFPricesInfo> QRFPrices = accoQRFPrices.Where(a => a.AccomodationId == request.RowId).Select(r => r.QRFPricesInfo).FirstOrDefault();
        //                if (QRFPrices != null && QRFPrices.Count > 0)
        //                {
        //                    for (int i = 0; i < response.Count; i++)
        //                    {
        //                        for (int j = 0; j < QRFPrices.Count; j++)
        //                        {
        //                            if (response[i].QRFPriceId == QRFPrices[j].QRFPriceId)
        //                            {
        //                                response[i].QTRate = QRFPrices[j].QTRate;
        //                                break;
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        else if (request.Type.ToLower() == "meal")
        //        {
        //            var mealsQRFPrices = resultPosPrice.Select(r => r.MealsQRFPrices).FirstOrDefault();
        //            if (mealsQRFPrices != null && mealsQRFPrices.Count > 0)
        //            {
        //                string[] rowid = request.RowId.Split("|");
        //                List<QRFPricesInfo> QRFPrices = mealsQRFPrices.Where(a => a.MealId == Convert.ToInt64(rowid[0]) && a.VenueDetailsID == Convert.ToInt64(rowid[1])).Select(r => r.QRFPricesInfo).FirstOrDefault();
        //                if (QRFPrices != null && QRFPrices.Count > 0)
        //                {
        //                    for (int i = 0; i < response.Count; i++)
        //                    {
        //                        for (int j = 0; j < QRFPrices.Count; j++)
        //                        {
        //                            if (response[i].QRFPriceId == QRFPrices[j].QRFPriceId)
        //                            {
        //                                response[i].QTRate = QRFPrices[j].QTRate;
        //                                break;
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        else if (request.Type.ToLower() == "activities")
        //        {
        //            var actQRFPrices = resultPosPrice.Select(r => r.ActivitiesQRFPrices).FirstOrDefault();
        //            if (actQRFPrices != null && actQRFPrices.Count > 0)
        //            {
        //                List<QRFPricesInfo> QRFPrices = actQRFPrices.Where(a => a.ActivityId == request.RowId).Select(r => r.QRFPricesInfo).FirstOrDefault();
        //                if (QRFPrices != null && QRFPrices.Count > 0)
        //                {
        //                    for (int i = 0; i < response.Count; i++)
        //                    {
        //                        for (int j = 0; j < QRFPrices.Count; j++)
        //                        {
        //                            if (response[i].QRFPriceId == QRFPrices[j].QRFPriceId)
        //                            {
        //                                response[i].QTRate = QRFPrices[j].QTRate;
        //                                break;
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    #endregion

        //    return response;
        //}

        //public async Task<string> InsertUpdateQRFPrices(QRFPricesSetReq request)
        //{
        //    string result = "";
        //    var resultPrice = await _MongoContext.mPositionPrice.FindAsync(p => p.QRFID == request.QRFID).Result.ToListAsync();
        //    if (request.Type.ToLower() == "accomodation")
        //    {
        //        result = await SetAccomodationPrice(resultPrice, request);
        //    }
        //    else if (request.Type.ToLower() == "meal")
        //    {
        //        result = await SetMealPrice(resultPrice, request);
        //    }
        //    else if (request.Type.ToLower() == "activities")
        //    {
        //        result = await SetActivitiesPrice(resultPrice, request);
        //    }
        //    //_MongoContext.mPositionPrice.AsQueryable().Where(a => a.QRFID == request.QRFID).ToList(); 

        //    return result;
        //}

        //public async Task<string> SetAccomodationPrice(List<mPositionPrice> resultPrice, QRFPricesSetReq request)
        //{
        //    UpdateResult resultFlag;
        //    if (resultPrice != null && resultPrice.Count > 0)
        //    {
        //        List<AccomodationQRFPrices> accoQRFPrices = resultPrice.Select(r => r.AccomodationQRFPrices).FirstOrDefault();

        //        if (accoQRFPrices != null && accoQRFPrices.Count > 0)
        //        {
        //            List<QRFPricesInfo> result = accoQRFPrices.Where(a => a.AccomodationId == request.AccomodationQRFPrices[0].AccomodationId).Select(r => r.QRFPricesInfo).FirstOrDefault();

        //            if (result != null && result.Count > 0)
        //            {
        //                request.AccomodationQRFPrices[0].QRFPricesInfo.FindAll(f => !result.Exists(r => r.QRFPriceId == f.QRFPriceId)).ForEach
        //               (r =>
        //               {
        //                   r.EditUser = "";
        //                   r.EditDate = null;
        //               });

        //                request.AccomodationQRFPrices[0].QRFPricesInfo.FindAll(f => result.Exists(r => r.QRFPriceId == f.QRFPriceId)).ForEach
        //              (r =>
        //              {
        //                  r.CreateDate = (result.Where(l => l.QRFPriceId == r.QRFPriceId).Select(l => l.CreateDate).FirstOrDefault());
        //                  r.CreateUser = (result.Where(l => l.QRFPriceId == r.QRFPriceId).Select(l => l.CreateUser).FirstOrDefault());
        //              });

        //                var res = await _MongoContext.mPositionPrice.FindOneAndUpdateAsync(m => m.QRFID == request.QRFID && m.AccomodationQRFPrices.Any(md => md.AccomodationId == request.AccomodationQRFPrices[0].AccomodationId),
        //                                Builders<mPositionPrice>.Update.Set(m => m.AccomodationQRFPrices[-1].QRFPricesInfo, request.AccomodationQRFPrices[0].QRFPricesInfo));
        //            }
        //            else
        //            {
        //                if (request.AccomodationQRFPrices[0].QRFPricesInfo != null && request.AccomodationQRFPrices[0].QRFPricesInfo.Count > 0)
        //                {
        //                    request.AccomodationQRFPrices[0].QRFPricesInfo.ForEach(r =>
        //                    {
        //                        r.EditUser = "";
        //                        r.EditDate = null;
        //                    });

        //                    resultFlag = await _MongoContext.mPositionPrice.UpdateOneAsync(Builders<mPositionPrice>.Filter.Eq("QRFID", request.QRFID),
        //                         Builders<mPositionPrice>.Update.PushEach<AccomodationQRFPrices>("AccomodationQRFPrices", request.AccomodationQRFPrices));

        //                    return resultFlag.ModifiedCount > 0 ? "1" : "QRF Prices not inserted.";
        //                }
        //                else
        //                {
        //                    return "1";
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (request.AccomodationQRFPrices[0].QRFPricesInfo != null && request.AccomodationQRFPrices[0].QRFPricesInfo.Count > 0)
        //            {
        //                request.AccomodationQRFPrices[0].QRFPricesInfo.ForEach(r =>
        //                {
        //                    r.EditUser = "";
        //                    r.EditDate = null;
        //                });

        //                resultFlag = await _MongoContext.mPositionPrice.UpdateOneAsync(Builders<mPositionPrice>.Filter.Eq("QRFID", request.QRFID),
        //                     Builders<mPositionPrice>.Update.PushEach<AccomodationQRFPrices>("AccomodationQRFPrices", request.AccomodationQRFPrices));

        //                return resultFlag.ModifiedCount > 0 ? "1" : "QRF Prices not inserted.";
        //            }
        //            else
        //            {
        //                return "1";
        //            }
        //        }
        //    }
        //    else
        //    {
        //        if (request.AccomodationQRFPrices != null && request.AccomodationQRFPrices.Count > 0)
        //        {
        //            request.AccomodationQRFPrices[0].QRFPricesInfo.ForEach(r =>
        //            {
        //                r.EditUser = "";
        //                r.EditDate = null;
        //            });
        //            var obj = new mPositionPrice();
        //            obj.QRFID = request.QRFID;
        //            obj.AccomodationQRFPrices = request.AccomodationQRFPrices;

        //            await _MongoContext.mPositionPrice.InsertOneAsync(obj);

        //            return "1";
        //        }
        //        else
        //        {
        //            return "1";
        //        }

        //    }

        //    return "1";
        //}

        //public async Task<string> SetMealPrice(List<mPositionPrice> resultPrice, QRFPricesSetReq request)
        //{
        //    UpdateResult resultFlag;
        //    if (resultPrice != null && resultPrice.Count > 0)
        //    {
        //        List<MealsQRFPrices> mealQRFPrices = resultPrice.Select(r => r.MealsQRFPrices).FirstOrDefault();

        //        if (mealQRFPrices != null && mealQRFPrices.Count > 0)
        //        {
        //            List<QRFPricesInfo> result = mealQRFPrices.Where(a => a.MealId == request.MealsQRFPrices[0].MealId && a.VenueDetailsID == request.MealsQRFPrices[0].VenueDetailsID).Select(r => r.QRFPricesInfo).FirstOrDefault();

        //            if (result != null && result.Count > 0)
        //            {
        //                request.MealsQRFPrices[0].QRFPricesInfo.FindAll(f => !result.Exists(r => r.QRFPriceId == f.QRFPriceId)).ForEach
        //               (r =>
        //               {
        //                   r.EditUser = "";
        //                   r.EditDate = null;
        //                   r.CreateDate = DateTime.Now;
        //               });

        //                request.MealsQRFPrices[0].QRFPricesInfo.FindAll(f => result.Exists(r => r.QRFPriceId == f.QRFPriceId)).ForEach
        //              (r =>
        //              {
        //                  r.CreateDate = (result.Where(l => l.QRFPriceId == r.QRFPriceId).Select(l => l.CreateDate).FirstOrDefault());
        //                  r.CreateUser = (result.Where(l => l.QRFPriceId == r.QRFPriceId).Select(l => l.CreateUser).FirstOrDefault());
        //                  r.EditDate = DateTime.Now;
        //              });

        //                var res = await _MongoContext.mPositionPrice.FindOneAndUpdateAsync(m => m.QRFID == request.QRFID && m.MealsQRFPrices.Any(md => md.MealId == request.MealsQRFPrices[0].MealId && md.VenueDetailsID == request.MealsQRFPrices[0].VenueDetailsID),
        //                                Builders<mPositionPrice>.Update.Set(m => m.MealsQRFPrices[-1].QRFPricesInfo, request.MealsQRFPrices[0].QRFPricesInfo));

        //                return "1";
        //            }
        //            else
        //            {
        //                if (request.MealsQRFPrices[0].QRFPricesInfo != null && request.MealsQRFPrices[0].QRFPricesInfo.Count > 0)
        //                {
        //                    request.MealsQRFPrices[0].QRFPricesInfo.ForEach(r =>
        //                    {
        //                        r.EditUser = "";
        //                        r.EditDate = null;
        //                        r.CreateDate = DateTime.Now;
        //                    });

        //                    resultFlag = await _MongoContext.mPositionPrice.UpdateOneAsync(Builders<mPositionPrice>.Filter.Eq("QRFID", request.QRFID),
        //                         Builders<mPositionPrice>.Update.PushEach<MealsQRFPrices>("MealsQRFPrices", request.MealsQRFPrices));

        //                    return resultFlag.ModifiedCount > 0 ? "1" : "QRF Prices not inserted.";
        //                }
        //                else
        //                {
        //                    return "Request Param Can not be blank.";
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (request.MealsQRFPrices[0].QRFPricesInfo != null && request.MealsQRFPrices[0].QRFPricesInfo.Count > 0)
        //            {
        //                request.MealsQRFPrices[0].QRFPricesInfo.ForEach(r =>
        //                {
        //                    r.EditUser = "";
        //                    r.EditDate = null;
        //                    r.CreateDate = DateTime.Now;
        //                });

        //                resultFlag = await _MongoContext.mPositionPrice.UpdateOneAsync(Builders<mPositionPrice>.Filter.Eq("QRFID", request.QRFID),
        //                     Builders<mPositionPrice>.Update.PushEach<MealsQRFPrices>("MealsQRFPrices", request.MealsQRFPrices));

        //                return resultFlag.ModifiedCount > 0 ? "1" : "QRF Prices not inserted.";
        //            }
        //            else
        //            {
        //                return "Request Param Can not be blank.";
        //            }
        //        }
        //    }
        //    else
        //    {
        //        if (request.MealsQRFPrices != null && request.MealsQRFPrices.Count > 0)
        //        {
        //            request.MealsQRFPrices[0].QRFPricesInfo.ForEach(r =>
        //            {
        //                r.EditUser = "";
        //                r.EditDate = null;
        //                r.CreateDate = DateTime.Now;
        //            });
        //            var obj = new mPositionPrice();
        //            obj.QRFID = request.QRFID;
        //            obj.MealsQRFPrices = request.MealsQRFPrices;

        //            await _MongoContext.mPositionPrice.InsertOneAsync(obj);

        //            return "1";
        //        }
        //        else
        //        {
        //            return "Request Param Can not be blank.";
        //        }
        //    }
        //}

        //public async Task<string> SetActivitiesPrice(List<mPositionPrice> resultPrice, QRFPricesSetReq request)
        //{
        //    UpdateResult resultFlag;
        //    if (resultPrice != null && resultPrice.Count > 0)
        //    {
        //        List<ActivitiesQRFPrices> actQRFPrices = resultPrice.Select(r => r.ActivitiesQRFPrices).FirstOrDefault();

        //        if (actQRFPrices != null && actQRFPrices.Count > 0)
        //        {
        //            List<QRFPricesInfo> result = actQRFPrices.Where(a => a.ActivityId == request.ActivitiesQRFPrices[0].ActivityId).Select(r => r.QRFPricesInfo).FirstOrDefault();

        //            if (result != null && result.Count > 0)
        //            {
        //                request.ActivitiesQRFPrices[0].QRFPricesInfo.FindAll(f => !result.Exists(r => r.QRFPriceId == f.QRFPriceId)).ForEach
        //               (r =>
        //               {
        //                   r.EditUser = "";
        //                   r.EditDate = null;
        //               });

        //                request.ActivitiesQRFPrices[0].QRFPricesInfo.FindAll(f => result.Exists(r => r.QRFPriceId == f.QRFPriceId)).ForEach
        //              (r =>
        //              {
        //                  r.CreateDate = (result.Where(l => l.QRFPriceId == r.QRFPriceId).Select(l => l.CreateDate).FirstOrDefault());
        //                  r.CreateUser = (result.Where(l => l.QRFPriceId == r.QRFPriceId).Select(l => l.CreateUser).FirstOrDefault());
        //              });

        //                var res = await _MongoContext.mPositionPrice.FindOneAndUpdateAsync(m => m.QRFID == request.QRFID && m.ActivitiesQRFPrices.Any(md => md.ActivityId == request.ActivitiesQRFPrices[0].ActivityId),
        //                                Builders<mPositionPrice>.Update.Set(m => m.ActivitiesQRFPrices[-1].QRFPricesInfo, request.ActivitiesQRFPrices[0].QRFPricesInfo));
        //            }
        //            else
        //            {
        //                if (request.ActivitiesQRFPrices[0].QRFPricesInfo != null && request.ActivitiesQRFPrices[0].QRFPricesInfo.Count > 0)
        //                {
        //                    request.ActivitiesQRFPrices[0].QRFPricesInfo.ForEach(r =>
        //                    {
        //                        r.EditUser = "";
        //                        r.EditDate = null;
        //                    });

        //                    resultFlag = await _MongoContext.mPositionPrice.UpdateOneAsync(Builders<mPositionPrice>.Filter.Eq("QRFID", request.QRFID),
        //                         Builders<mPositionPrice>.Update.PushEach("ActivitiesQRFPrices", request.ActivitiesQRFPrices));

        //                    return resultFlag.ModifiedCount > 0 ? "1" : "QRF Prices not inserted.";
        //                }
        //                else
        //                {
        //                    return "1";
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (request.ActivitiesQRFPrices[0].QRFPricesInfo != null && request.ActivitiesQRFPrices[0].QRFPricesInfo.Count > 0)
        //            {
        //                request.ActivitiesQRFPrices[0].QRFPricesInfo.ForEach(r =>
        //                {
        //                    r.EditUser = "";
        //                    r.EditDate = null;
        //                });

        //                resultFlag = await _MongoContext.mPositionPrice.UpdateOneAsync(Builders<mPositionPrice>.Filter.Eq("QRFID", request.QRFID),
        //                     Builders<mPositionPrice>.Update.PushEach("ActivitiesQRFPrices", request.ActivitiesQRFPrices));

        //                return resultFlag.ModifiedCount > 0 ? "1" : "QRF Prices not inserted.";
        //            }
        //            else
        //            {
        //                return "1";
        //            }
        //        }
        //    }
        //    else
        //    {
        //        if (request.ActivitiesQRFPrices != null && request.ActivitiesQRFPrices.Count > 0)
        //        {
        //            request.ActivitiesQRFPrices[0].QRFPricesInfo.ForEach(r =>
        //            {
        //                r.EditUser = "";
        //                r.EditDate = null;
        //            });
        //            var obj = new mPositionPrice();
        //            obj.QRFID = request.QRFID;
        //            obj.ActivitiesQRFPrices = request.ActivitiesQRFPrices;

        //            await _MongoContext.mPositionPrice.InsertOneAsync(obj);

        //            return "1";
        //        }
        //        else
        //        {
        //            return "1";
        //        }
        //    }

        //    return "1";
        //}
        #endregion

        //public async Task<long> UpdateQuoteDetails(QuoteSetReq request)
        //{
        //    try
        //    {
        //        if (!string.IsNullOrEmpty(request.QRFID))
        //        {
        //            if (!string.IsNullOrEmpty(request.CurrentPipeline) && !string.IsNullOrEmpty(request.CurrentPipelineStep))
        //            {
        //                await _MongoContext.mQuote.FindOneAndUpdateAsync(
        //                                           Builders<mQuote>.Filter.Eq("QRFID", request.QRFID),
        //                                           Builders<mQuote>.Update.
        //                                           Set("CurrentPipeline", request.CurrentPipeline).
        //                                           Set("CurrentPipelineStep", request.CurrentPipelineStep).
        //                                           Set("Remarks", request.Remarks).
        //                                           Set("CurrentPipelineSubStep", request.CurrentPipelineSubStep).
        //                                           Set("QuoteResult", request.QuoteResult).
        //                                           Set("Status", request.Status).
        //                                           Set("EditUser", request.EditUser).
        //                                           Set("EditDate", DateTime.Now)
        //                                           );

        //                SaveDefaultGuesstimate(request.QRFID);
        //                SaveDefaultProposal(request.QRFID);

        //                return request.QRFID;
        //            }
        //            else
        //            {
        //                return 0;
        //            }
        //        }
        //        else
        //        {
        //            return 0;
        //        }
        //    }
        //    catch (MongoWriteException)
        //    {
        //        return 0;
        //    }
        //}

        //public void SaveDefaultGuesstimate(string QRFID = 0)
        //{
        //    if (QRFId > 0)
        //    {
        //        _MongoContext.mGuesstimate.DeleteManyAsync(Builders<mGuesstimate>.Filter.Eq("QRFID", QRFId));

        //        var objPositionPricesList = _MongoContext.mPositionPrice.AsQueryable().Where(a => a.QRFID == QRFId && a.IsDeleted == false).ToList();

        //        var objPositions = _MongoContext.mPosition.AsQueryable().Where(a => a.QRFID == QRFId).ToList();

        //        var guesstimate = new mGuesstimate();
        //        guesstimate.GuesstimateId = Guid.NewGuid().ToString();
        //        guesstimate.QRFID = QRFId;
        //        guesstimate.VersionId = 1;
        //        guesstimate.VersionName = "FirstVersion";
        //        guesstimate.VersionDescription = "FirstVersion";
        //        guesstimate.CreateDate = DateTime.Now;
        //        guesstimate.EditUser = "";
        //        guesstimate.EditDate = null;
        //        guesstimate.IsDeleted = false;

        //        foreach (var objPositionPrices in objPositionPricesList)
        //        {
        //            var objPos = objPositions.Where(a => a.PositionId == objPositionPrices.PositionId).FirstOrDefault();

        //            var item = new GuesstimateDetails();
        //            item.GuesstimateDetailId = Guid.NewGuid().ToString();
        //            item.PositionId = objPositionPrices.PositionId;
        //            item.PositionPriceId = objPositionPrices.PositionPriceId;
        //            item.DepartureId = objPositionPrices.DepartureId;
        //            item.Period = objPositionPrices.Period;
        //            item.PaxSlabId = objPositionPrices.PaxSlabId;
        //            item.PaxSlab = objPositionPrices.PaxSlab;
        //            item.Type = objPositionPrices.Type;
        //            item.RoomId = objPositionPrices.RoomId;
        //            item.SupplierId = objPositionPrices.SupplierId;
        //            item.Supplier = objPositionPrices.Supplier;
        //            item.ProductCategoryId = objPositionPrices.ProductCategoryId;
        //            item.ProductRangeId = objPositionPrices.ProductRangeId;
        //            item.ProductRange = objPositionPrices.ProductRange;
        //            item.ProductRangeCode = objPositionPrices.ProductRangeCode;
        //            item.ProductType = objPos.ProductType;
        //            item.KeepAs = objPos.KeepAs;
        //            item.BuyCurrencyId = objPositionPrices.BuyCurrencyId;
        //            item.BuyCurrency = objPositionPrices.BuyCurrency;
        //            item.ContractId = objPositionPrices.ContractId;
        //            item.ContractPrice = objPositionPrices.ContractPrice;
        //            item.BudgetPrice = objPositionPrices.BudgetPrice;
        //            item.BuyPrice = objPositionPrices.BuyPrice;
        //            item.MarkupAmount = objPositionPrices.MarkupAmount;
        //            item.BuyNetPrice = objPositionPrices.BuyNetPrice;
        //            item.SellCurrencyId = objPositionPrices.SellCurrencyId;
        //            item.SellCurrency = objPositionPrices.SellCurrency;
        //            item.SellNetPrice = objPositionPrices.SellNetPrice;
        //            item.TaxAmount = objPositionPrices.TaxAmount;
        //            item.SellPrice = objPositionPrices.SellPrice;
        //            item.ExchangeRateId = objPositionPrices.ExchangeRateId;
        //            item.ExchangeRatio = objPositionPrices.ExchangeRatio;

        //            item.CreateDate = objPositionPrices.CreateDate;
        //            item.CreateUser = objPositionPrices.CreateUser;
        //            item.EditUser = objPositionPrices.EditUser;
        //            item.EditDate = objPositionPrices.EditDate;
        //            item.IsDeleted = objPositionPrices.IsDeleted;

        //            guesstimate.GuesstimateDetails.Add(item);
        //        }

        //        _MongoContext.mGuesstimate.InsertOneAsync(guesstimate);
        //    }
        //    //response.ResponseStatus.Status = "Success";
        //    //response.ResponseStatus.ErrorMessage = "Saved Successfully.";
        //}

        //#region Helper Methods

        //public void SaveDefaultProposal(string QRFID = 0)
        //{
        //    if (QRFId > 0)
        //    {
        //        _MongoContext.mProposal.DeleteManyAsync(Builders<mProposal>.Filter.Eq("QRFID", QRFId));

        //        var proposal = new mProposal();
        //        proposal.QRFID = QRFId;
        //        proposal.ProposalId = Guid.NewGuid().ToString();
        //        proposal.ItineraryId = Guid.NewGuid().ToString();
        //        proposal.Version = 1;
        //        proposal.CreateDate = DateTime.Now;
        //        proposal.EditUser = "";
        //        proposal.EditDate = null;
        //        proposal.IsDeleted = false;

        //        _MongoContext.mProposal.InsertOneAsync(proposal);
        //        SaveDefaultProposalSuggestedItinerary(QRFId, proposal.ItineraryId);
        //    }
        //}

        //public void SaveDefaultProposalSuggestedItinerary(string QRFID = 0, string ItineraryId = "")
        //{
        //    if (QRFId > 0 && !string.IsNullOrEmpty(ItineraryId))
        //    {
        //        _MongoContext.mItinerary.DeleteManyAsync(Builders<mItinerary>.Filter.Eq("QRFID", QRFId));

        //        var itinerary = new mItinerary();
        //        itinerary.QRFID = QRFId;
        //        itinerary.ItineraryID = ItineraryId;
        //        itinerary.Version = 1;
        //        //itinerary.ItineraryDays = ;

        //        _MongoContext.mItinerary.InsertOneAsync(itinerary);
        //    }
        //}

        //#endregion

        #region FollowUp Quote
        //public IQueryable<dynamic> GetFollowUpForQRF_Id(QrfFollowUpRequest request)
        //{
        //    var result = from u in _MongoContext.mQrfFollowUp.AsQueryable()
        //                 where u.QRFID == request.QRFID
        //                 select u.FollowUp.Where(x => x.FollowUpStatus.ToLower() == request.Status.ToLower());

        //    return result;
        //}

        //public IQueryable<dynamic> GetFollowUpForFollowUp_Id(QrfFollowUpRequest request)
        //{
        //    var result = from u in _MongoContext.mQrfFollowUp.AsQueryable()
        //                 where u.QRFID == request.QRFID
        //                 select u;

        //    var res = from u in result.AsQueryable()
        //              select u.FollowUp.Where(x => x.FollowUp_Id == request.FollowUp_Id);

        //    return res;
        //}

        //public async Task<bool> SetFollowUpForQRF_Id(QrfFollowUpSetRequest req)
        //{
        //    try
        //    {
        //        req.FollowUpItem.FollowUpStatus = req.FollowUpItem.FollowUpStatus.ToLower();
        //        var followup = new List<FollowUpItem> { req.FollowUpItem };
        //        var item = new mQrfFollowUp { FollowUp = followup, QRFID = req.QRFID };
        //        var filters = Builders<mQrfFollowUp>.Filter.Where(x => x.QRFID == req.QRFID);
        //        if (_MongoContext.mQrfFollowUp.Find(filters).Count() > 0)
        //        {
        //            var filter = Builders<mQrfFollowUp>.Filter.And(
        //                            Builders<mQrfFollowUp>.Filter.Where(x => x.QRFID == req.QRFID),
        //                            Builders<mQrfFollowUp>.Filter.Eq("FollowUp.FollowUp_Id", req.FollowUpItem.FollowUp_Id));

        //            if (_MongoContext.mQrfFollowUp.Find(filter).Count() > 0)
        //            {
        //                var update = Builders<mQrfFollowUp>.Update.Set(x => x.FollowUp[-1], req.FollowUpItem);
        //                await _MongoContext.mQrfFollowUp.UpdateOneAsync(filter, update);
        //            }
        //            else
        //            {
        //                req.FollowUpItem.FollowUp_Id = ObjectId.GenerateNewId().ToString();
        //                var insert = Builders<mQrfFollowUp>.Update.Push<FollowUpItem>(e => e.FollowUp, req.FollowUpItem);
        //                await _MongoContext.mQrfFollowUp.UpdateOneAsync(filters, insert);
        //            }
        //        }
        //        else
        //        {
        //            item.FollowUp.ForEach(x => x.FollowUp_Id = ObjectId.GenerateNewId().ToString());
        //            await _MongoContext.mQrfFollowUp.InsertOneAsync(item);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        return false;
        //    }
        //    return true;
        //}

        //public FollowUpItem GetFollowUpByQuoteSearchCriteria(QuoteSearchReq request, string strQRFID)
        //{
        //    var followup = new FollowUpItem();

        //    if (!string.IsNullOrWhiteSpace(request.Date) && request.Date.ToLower().Trim() == "follow up date")
        //    {
        //        if (request.From != null && request.To != null)
        //        {
        //            DateTime dtFrom = Convert.ToDateTime(request.From);
        //            DateTime dtTo = Convert.ToDateTime(request.To);

        //            var result = (from u in _MongoContext.mQrfFollowUp.AsQueryable()
        //                          where u.QRFID == strQRFID
        //                          select u.FollowUp.Where(f => f.FollowUpStatus.ToLower() == "active" && (f.ExternalFollowUpDateTime >= dtFrom && f.ExternalFollowUpDateTime <= dtTo)
        //                                                    || (f.InternalFollowUpDateTime >= dtFrom && f.InternalFollowUpDateTime <= dtTo))).Distinct().ToList();

        //            var followupResult = (result != null && result.Count > 0) ? result.FirstOrDefault().Select(f => new FollowUpItem
        //            {
        //                CreateDate = f.CreateDate,
        //                CreateUser = f.CreateUser,
        //                EditDate = f.EditDate,
        //                EditUser = f.EditUser,
        //                ExternalName = (f.ExternalFollowUpDateTime >= dtFrom && f.ExternalFollowUpDateTime <= dtTo) ? f.ExternalName : "",
        //                ExternalFollowUpDateTime = (f.ExternalFollowUpDateTime >= dtFrom && f.ExternalFollowUpDateTime <= dtTo) ? f.ExternalFollowUpDateTime : null,
        //                ExternalStatus = (f.ExternalFollowUpDateTime >= dtFrom && f.ExternalFollowUpDateTime <= dtTo) ? f.ExternalStatus : "",
        //                FollowUp_Id = f.FollowUp_Id,
        //                InternalName = (f.InternalFollowUpDateTime >= dtFrom && f.InternalFollowUpDateTime <= dtTo) ? f.InternalName : "",
        //                InternalFollowUpDateTime = (f.InternalFollowUpDateTime >= dtFrom && f.InternalFollowUpDateTime <= dtTo) ? f.InternalFollowUpDateTime : null,
        //                InternalStatus = (f.InternalFollowUpDateTime >= dtFrom && f.InternalFollowUpDateTime <= dtTo) ? f.InternalStatus : "",
        //                Notes = f.Notes,
        //                Task = f.Task,
        //                FollowUpStatus = f.FollowUpStatus
        //            }).FirstOrDefault() : followup;

        //            return followupResult;
        //        }
        //        else if (!string.IsNullOrEmpty(request.Month) && request.Year > 0)
        //        {
        //            var result = (from u in _MongoContext.mQrfFollowUp.AsQueryable()
        //                          where u.QRFID == strQRFID
        //                          select u.FollowUp.Where(f => f.FollowUpStatus.ToLower() == "active" && (f.ExternalFollowUpDateTime.Value.Year == request.Year && f.ExternalFollowUpDateTime.Value.Month == DateTime.ParseExact(request.Month, "MMMM", System.Globalization.CultureInfo.InvariantCulture).Month)
        //                          || (f.InternalFollowUpDateTime.Value.Year == request.Year && f.InternalFollowUpDateTime.Value.Month == DateTime.ParseExact(request.Month, "MMMM", System.Globalization.CultureInfo.InvariantCulture).Month))).Distinct().ToList();

        //            return (result != null && result.Count > 0) ? result.FirstOrDefault().Select(f => new FollowUpItem
        //            {
        //                CreateDate = f.CreateDate,
        //                CreateUser = f.CreateUser,
        //                EditDate = f.EditDate,
        //                EditUser = f.EditUser,
        //                ExternalName = (f.ExternalFollowUpDateTime.Value.Year == request.Year && f.ExternalFollowUpDateTime.Value.Month == DateTime.ParseExact(request.Month, "MMMM", System.Globalization.CultureInfo.InvariantCulture).Month) ? f.ExternalName : "",
        //                ExternalFollowUpDateTime = (f.ExternalFollowUpDateTime.Value.Year == request.Year && f.ExternalFollowUpDateTime.Value.Month == DateTime.ParseExact(request.Month, "MMMM", System.Globalization.CultureInfo.InvariantCulture).Month) ? f.ExternalFollowUpDateTime : null,
        //                ExternalStatus = (f.ExternalFollowUpDateTime.Value.Year == request.Year && f.ExternalFollowUpDateTime.Value.Month == DateTime.ParseExact(request.Month, "MMMM", System.Globalization.CultureInfo.InvariantCulture).Month) ? f.ExternalStatus : "",
        //                FollowUp_Id = f.FollowUp_Id,
        //                InternalName = (f.InternalFollowUpDateTime.Value.Year == request.Year && f.InternalFollowUpDateTime.Value.Month == DateTime.ParseExact(request.Month, "MMMM", System.Globalization.CultureInfo.InvariantCulture).Month) ? f.InternalName : "",
        //                InternalFollowUpDateTime = (f.InternalFollowUpDateTime.Value.Year == request.Year && f.InternalFollowUpDateTime.Value.Month == DateTime.ParseExact(request.Month, "MMMM", System.Globalization.CultureInfo.InvariantCulture).Month) ? f.InternalFollowUpDateTime : null,
        //                InternalStatus = (f.InternalFollowUpDateTime.Value.Year == request.Year && f.InternalFollowUpDateTime.Value.Month == DateTime.ParseExact(request.Month, "MMMM", System.Globalization.CultureInfo.InvariantCulture).Month) ? f.InternalStatus : "",
        //                Notes = f.Notes,
        //                Task = f.Task,
        //                FollowUpStatus = f.FollowUpStatus
        //            }).FirstOrDefault() : followup;
        //        }
        //    }
        //    else
        //    {

        //        var result = (from u in _MongoContext.mQrfFollowUp.AsQueryable()
        //                      where u.QRFID == strQRFID
        //                      select u.FollowUp.Where(f => f.FollowUpStatus.ToLower() == "active")).Distinct().ToList();

        //        return (result != null && result.Count > 0) ? result.FirstOrDefault().FirstOrDefault() : followup;
        //    }

        //    return null;
        //}
        #endregion

        #region Update ValidForAcceptance field in mQuote and mQRFPrice collection
        /// <summary>
        /// others Pipeline as Costing,Costing Approval, Agent Approval, Amendment Pipeline save the ValidForAcceptance = CurrentDate + 7Days 
        /// in mQRFPrice collection when Proposal Document is generated.
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<ResponseStatus> UpdateValidForAcceptance(QuoteGetReq req)
        {
            ResponseStatus objResponseStatus = new ResponseStatus();
            try
            {
                string ValidForAcceptance = "On or before " + Convert.ToDateTime((DateTime.Now).AddDays(7)).ToString("dd MMM yy");

                //var res = await _MongoContext.mQuote.FindOneAndUpdateAsync(Builders<mQuote>.Filter.Eq("QRFID", req.QRFID),
                //              Builders<mQuote>.Update.Set("ValidForAcceptance", ValidForAcceptance).Set("EditUser", req.UserName)
                //              .Set("EditDate", DateTime.Now));

                var resQRFPrice = await _MongoContext.mQRFPrice.FindOneAndUpdateAsync(Builders<mQRFPrice>.Filter.Where(a => a.QRFID == req.QRFID && a.IsCurrentVersion == true),
                            Builders<mQRFPrice>.Update.Set("ValidForAcceptance", ValidForAcceptance).Set("EditUser", req.UserName)
                            .Set("EditDate", DateTime.Now));

                objResponseStatus.Status = "Success";
            }
            catch (Exception ex)
            {
                objResponseStatus.Status = "Error";
                objResponseStatus.ErrorMessage = ex.Message;
            }
            return objResponseStatus;
        }
        #endregion
        /// <summary>
        /// Get Quote on Qrfid
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        #region GetQrfDocuments
        public async Task<QrfDocumentGetResponse> GetQuoteQrfDocumentsDetails(QrfDocumentGetReq request)
        {
            QrfDocumentGetResponse response = new QrfDocumentGetResponse();
            var QuoteResponse = await this.GetQuoteDetails(request.QrfId);
            if (QuoteResponse != null && !string.IsNullOrEmpty(request.DocumentId))
            {
                var Documents = QuoteResponse.QrfDocuments.Where(x => x.Document_Id == request.DocumentId).ToList();
                response.QrfDocuments = Documents?.ToList();
                response.ResponseStatus.StatusMessage = "Success";
            }
            else
            {
                response.ResponseStatus.Status = "error";
                response.ResponseStatus.StatusMessage = "No Data Found";
            }

            return response;

        }
        /// <summary>
        /// Returns Quote Details
        /// </summary>
        /// <param name="QrfId"></param>
        /// <returns></returns>
        public async Task<mQuote> GetQuoteDetails(string QrfId)
        {
            mQuote mq = new mQuote();
            mq = await _MongoContext.mQuote.FindAsync(a => a.QRFID == QrfId).Result.FirstOrDefaultAsync();
            return mq;

        }
        /// <summary>
        /// Sets QrfDocuments
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<QrfDocumentPostResponse> SaveQRFDocumentsForQrfId(QrfDocumentPostRequest request)
        {
            QrfDocumentPostResponse response = new QrfDocumentPostResponse();
            QrfDocument qrdoc = new QrfDocument();
            if (!string.IsNullOrEmpty(request.QrfId))
            {
                var QuoteDetails = await this.GetQuoteDetails(request.QrfId);
                if (QuoteDetails != null)
                {
                    qrdoc.Document_Id = Guid.NewGuid().ToString();
                    qrdoc.FileName = request.FileName;
                    qrdoc.PhysicalPath = request.PhysicalPath;
                    qrdoc.SavedFileName = request.SavedFileName;
                    qrdoc.VirtualPath = request.VirtualPath;
                    qrdoc.Status = request.Status;
                    qrdoc.CratedUser = request.CratedUser;
                    qrdoc.CreateDate = request.CreateDate;
                    qrdoc.ModifiedDate = request.ModifiedDate;
                    qrdoc.ModifiedUser = request.ModifiedUser;
                    QuoteDetails.QrfDocuments.Add(qrdoc);
                    response.DocumentId = qrdoc.Document_Id;
                    response.QrfId = request.QrfId;
                    response.Responsestatus.Status = "Success";
                    await _MongoContext.mQuote.FindOneAndUpdateAsync(
                                            Builders<mQuote>.Filter.Eq("QRFID", request.QrfId),
                                            Builders<mQuote>.Update.
                                            Set("QrfDocuments", QuoteDetails.QrfDocuments));
                    response.Responsestatus.Status = "Success";

                }
                else
                {
                    response.Responsestatus.Status = "Error";
                    response.Responsestatus.StatusMessage = "No quote Data Found";

                }
            }
            else
            {
                response.Responsestatus.Status = "Error";
                response.Responsestatus.StatusMessage = "Invalid QRfId";

            }
            return response;

        }
        #endregion

        #region MSDynamics for Opportunity Bookins and Quote

        public async Task<mQuote> getQuoteInfo(string QrfId)
        {
            mQuote quoteInfo = _MongoContext.mQuote.AsQueryable().Where(a => a.QRFID == QrfId).FirstOrDefault();

            return quoteInfo;
        }

        public async Task<mQRFPrice> getQuotePriceInfo(string QrfId)
        {
            mQRFPrice quoteInfo = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == QrfId && a.IsCurrentVersion == true).FirstOrDefault();

            return quoteInfo;
        }

        public async Task<Bookings> getBookingInfo(string BookingNo)
        {
            Bookings BookingInfo = _MongoContext.Bookings.AsQueryable().Where(a => a.BookingNumber == BookingNo).FirstOrDefault();

            return BookingInfo;
        }

        #endregion
    }
}