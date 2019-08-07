using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    /// <summary>
    /// Response Format for User Details
    /// </summary>
    public class UserDetailsResponse
    {
        /// <summary>
        /// First Name of the User
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// Last Name of the User
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// Status message of the request
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// Status message of the request
        /// </summary>
        public string ErrorMessage { get; set; }
        /// <summary>
        /// Name of the User Company
        /// </summary>
        public string CompanyName { get; set; }
        /// <summary>
        /// Id of the User Company
        /// </summary>
        public string CompanyId { get; set; }
        /// <summary>
        /// Contact Number
        /// </summary>
        public string ContactDisplayMessage { get; set; }
        /// <summary>
        /// Currency for the Balance and Credit Amount
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// Balance Amount Availabe in the Voyager for the User
        /// </summary>
        public string BalanceAmount { get; set; }
        /// <summary>
        /// Credit Limit after for the user
        /// </summary>
        public string CreditAmount { get; set; }
        /// <summary>
        /// VoyagerUser_Id
        /// </summary>
        public string VoyagerUser_Id { get; set; }
        /// <summary>
        /// SystemCompany_Id
        /// </summary>
        public string SystemCompany_Id { get; set; }
        /// <summary>
        /// List of Roles assigned to User in Voyager
        /// </summary>
        public List<UserRoleDetails> UserRoleDetails { get; set; }
        public string Photo { get; set; }

		/// <summary>
		/// Contact Id of Logged In User
		/// </summary>
		public string ContactId { get; set; }
    }
    /// <summary>
    /// Roles assigned to User in Voyager
    /// </summary>
    public class UserRoleDetails {
        /// <summary>
        /// Unique Id of the Role
        /// </summary>
        public string Voyager_Role_Id { get; set; }
        /// <summary>
        /// Role Name
        /// </summary>
        public string RoleName { get; set; }
        /// <summary>
        /// Role Name in all small letter
        /// </summary>
        public string LoweredRoleName { get; set; }
        /// <summary>
        /// Description of the Role
        /// </summary>
        public string Description { get; set; }
    }

    public class UserCookieDetail
    {
        public string Contact_Id { get; set; }
        public string UserName { get; set; }
        public string Company_Id { get; set; }
        public string CompanyName { get; set; }
        public string RoleName { get; set; }
        public bool? IsSupplier { get; set; }
        public bool? IsAgent { get; set; }
    }

}
