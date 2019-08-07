using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    /// <summary>
    /// Request Object for Set Key Info for Company
    /// </summary>
    public class SetCompanyPaymentTerms_RQ
    {
        /// <summary>
        /// Unique Id of the Company Record
        /// </summary>
        public string PaymentTerms_Id { get; set; }
        /// <summary>
        /// Requesting User
        /// </summary>
        public string User { get; set; }
    }
}
