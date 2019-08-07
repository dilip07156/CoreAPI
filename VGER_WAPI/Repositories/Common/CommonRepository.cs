using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VGER_WAPI.Helpers;
using VGER_WAPI.Models;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Repositories
{
    public class CommonRepository : ICommonRepository
    {
        #region Private Variable Declaration
        private readonly MongoContext _MongoContext = null;
        private readonly ISettingsRepository _settingRepository;
        private readonly IConfiguration _configuration;
        #endregion

        public CommonRepository(IOptions<MongoSettings> settings, ISettingsRepository settingRepository, IConfiguration configuration)
        {
            _MongoContext = new MongoContext(settings);
            _configuration = configuration;
            _settingRepository = settingRepository;
        }

        public async Task<Integration_Search_Data> GetIntegrationCredentials(string ApplicationName, string VoyagerUser_Id)
        {
            var App_Keys_List = _MongoContext.mUsers.AsQueryable().Where(x => x.App_Keys != null && x.App_Keys.Any() && x.VoyagerUser_Id == VoyagerUser_Id).ToList();
            App_Keys_List = App_Keys_List.Where(a => a.App_Keys.Select(b => b.Application_Name.ToLower()).ToArray().Contains(ApplicationName.ToLower())).ToList();

            Integration_Search_Data IntegrationSearchDataList = new Integration_Search_Data();

            if (App_Keys_List != null && App_Keys_List.Any())
            {
                foreach (var item in App_Keys_List.ToList())
                {

                    IntegrationSearchDataList = item.App_Keys.Where(a => a.Status == null || a.Status == "").Select(b => new Integration_Search_Data
                    {
                        Application_Id = b.Application_Id,
                        Application_Name = b.Application_Name,
                        Keys = _settingRepository.GenerateCustomIntegrationKeyForSource(b.Application_Id, item.Email),
                        UserKey = _settingRepository.GenerateCustomIntegrationKeyForSource(b.Key, item.Email),
                        Status = b.Status,
                        UserId = item.VoyagerUser_Id,
                        UserName = item.UserName
                    }).FirstOrDefault();

                }
            }

            return IntegrationSearchDataList;
        }

        public async Task<Integration_Search_Data> GetIntegrationCredentialsByUser(string ApplicationName, string VoyagerUser)
        {
            var App_Keys_List = _MongoContext.mUsers.AsQueryable().Where(x => x.App_Keys != null && x.App_Keys.Any() && x.Email == VoyagerUser).ToList();
            App_Keys_List = App_Keys_List.Where(a => a.App_Keys.Select(b => b.Application_Name.ToLower()).ToArray().Contains(ApplicationName.ToLower())).ToList();

            Integration_Search_Data IntegrationSearchDataList = new Integration_Search_Data();

            if (App_Keys_List != null && App_Keys_List.Any())
            {
                foreach (var item in App_Keys_List.ToList())
                {

                    IntegrationSearchDataList = item.App_Keys.Where(a => a.Status == null || a.Status == "").Select(b => new Integration_Search_Data
                    {
                        Application_Id = b.Application_Id,
                        Application_Name = b.Application_Name,
                        Keys = _settingRepository.GenerateCustomIntegrationKeyForSource(b.Application_Id, item.Email),
                        UserKey = _settingRepository.GenerateCustomIntegrationKeyForSource(b.Key, item.Email),
                        Status = b.Status,
                        UserId = item.VoyagerUser_Id,
                        UserName = item.UserName
                    }).FirstOrDefault();

                }
            }

            return IntegrationSearchDataList;
        }

        public bool ValidateEmail(string email)
        {
            if (!string.IsNullOrWhiteSpace(email))
            {
                Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
                Match match = regex.Match(email);
                if (match.Success)
                    return true;
            }
            return false;
        }

        public bool ValidateEmailCustom(string email)
        {
            email = string.IsNullOrWhiteSpace(email) ? "" : email;
            if (!email.Contains("@") || !email.Contains("."))
            {
                return false;
            }
            return true;
        }
    }
}
