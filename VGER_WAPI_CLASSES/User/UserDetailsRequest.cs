using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    /// <summary>
    /// Request Format for getting User Details
    /// </summary>
    public class UserDetailsRequest
    {
        /// <summary>
        /// User Id (Generally Email of the user)
        /// </summary>
        [Required]
        public string UserName;
    }
}
