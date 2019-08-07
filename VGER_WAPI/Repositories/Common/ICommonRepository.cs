using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Repositories
{
    public interface ICommonRepository
    {
        Task<Integration_Search_Data> GetIntegrationCredentials(string ApplicationName, string VoyagerUser_Id);
        Task<Integration_Search_Data> GetIntegrationCredentialsByUser(string ApplicationName, string VoyagerUser);
        bool ValidateEmail(string email);
        bool ValidateEmailCustom(string email);
    }
}
