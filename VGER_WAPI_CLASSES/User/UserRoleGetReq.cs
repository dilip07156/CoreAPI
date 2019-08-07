using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class UserRoleGetReq
    {
        /// <summary>
        /// User ID
        /// </summary>
        public string UserID { get; set; }
		/// <summary>
		/// Company ID
		/// </summary>
		public string CompanyID { get; set; }
		/// <summary>
		/// Contact ID
		/// </summary>
		public string ContactID { get; set; }
	} 
}
