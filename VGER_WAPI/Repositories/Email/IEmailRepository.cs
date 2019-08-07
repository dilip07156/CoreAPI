using System.Threading.Tasks;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Repositories
{
    public interface IEmailRepository
    {
        Task<EmailGetRes> GenerateEmail(EmailGetReq request);

        Task<EmailGetRes> SendEmail(EmailTemplateGetRes emailContent, string config = "",bool IsLog=false);

        mMailServerConfiguration GetSmtpCredentials(string emailId, string typeconfig = "");

        string GetPath(string documentType);
    }
}
