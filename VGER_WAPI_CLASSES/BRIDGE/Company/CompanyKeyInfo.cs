using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class CompanyKeyInfo
    {
    }

    /// <summary>
    /// Request Object for Set Key Info for Company
    /// </summary>
    public class SetCompanyKeyInfo_RQ
    {
        /// <summary>
        /// Unique Id of the Company Record
        /// </summary>
        public string Company_Id { get; set; }
        /// <summary>
        /// Requesting User
        /// </summary>
        public string User { get; set; }
    }
}
