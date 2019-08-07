using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class SettingsGetReq
	{
		public string Type { get; set; }
		public string DestinationId { get; set; }
		public string RoleId { get; set; }
		public string UserId { get; set; }
		public string LoggedInUserContact_Id { get; set; }
	}

    public class SettingsAutomatedGetReq
    {
        public string QRFId { get; set; }
        public string CompanyId { get; set; } 
        public string UserRole { get; set; }
    }
}
