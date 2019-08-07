using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_DISTRIBUTION.Models.DISTRO
{
    public class LoginRequest
    {
        /// <summary>
        /// Login user name, (email Id)
        /// </summary>
        [Required]
        public string UserName { get; set; }
        /// <summary>
        /// user password
        /// </summary>
        [Required]
        public string Password { get; set; }
    }
}
