using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    /// <summary>
    /// Response format of Valid Login 
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// Authentication Token generated after valid Login Attempt
        /// </summary>
        public string Token {get; set;}

        /// <summary>
        /// Token Validity Duration (in Minutes)
        /// </summary>
        public string Expiry { get; set; }
        /// <summary>
        /// Status message of the Login Attempt
        /// </summary>
        public string Message { get; set; }
    }

    /// <summary>
    /// Response format of Valid Login 
    /// </summary>
    public class IntegrationLoginResponse
    {
        /// <summary>
        /// Authentication Token generated after valid Login Attempt
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Token Validity Duration (in Minutes)
        /// </summary>
        public string Expiry { get; set; }
        /// <summary>
        /// Status message of the Login Attempt
        /// </summary>
        public string Message { get; set; }

        public mUsers UserInfo { get; set; }
    }
}
