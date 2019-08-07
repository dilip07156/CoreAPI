using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    /// <summary>
    /// Request Object for Set Key Info for Company Account
    /// </summary>
    public class CompanyPaymentSetReq
    {
        /// <summary>
        /// Unique Id of the Company Record
        /// </summary>
        public string Company_Id { get; set; }

        /// <summary>
        /// Unique Id of the CompanyAccount Record
        /// </summary>
        public string CompanyAccountDetail_Id { get; set; }

        /// <summary>
        /// Requesting User
        /// </summary>
        public string User { get; set; }
    }
}
