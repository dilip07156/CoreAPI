using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class SetCompany_RQ
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
