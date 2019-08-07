using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    /// <summary>
    /// Request Format for Login Attempt
    /// </summary>
    public class LoginRequest 
    {
        /// <summary>
        /// Login User Id (Generally Email of the user)
        /// </summary>
        [Required]
        public string UserName { get; set; }
        /// <summary>
        /// user password
        /// </summary>
        [Required]
        public string Password { get; set; }

        public string CompanyId { get; set; }

        public string ContactId { get; set; }

        public string Email { get; set; }
    }

    public class IntegrationLoginRequest
    {
        [Required]
        public string Source { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Module { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Operation { get; set; }//Action
        [Required]
        public string Key { get; set; }
        [Required]
        public string User { get; set; }
    }
}
