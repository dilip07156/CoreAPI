using System;
using System.Collections.Generic;
using System.Text;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI_CLASSES
{
    public class UserByRoleGetRes
    {
        public List<UserDetails> Users { get; set; } = new List<UserDetails>();
		public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
    }

    public class UserDetails
    {
        public string UserId { get; set; }
        public string UserRoleId { get; set; }
        public string UserRole { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
		public string CompanyId { get; set; }
		public string ContactId { get; set; }
	}
}
