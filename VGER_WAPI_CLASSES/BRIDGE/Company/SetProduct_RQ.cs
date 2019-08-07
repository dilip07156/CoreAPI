using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class SetProduct_RQ
    {       
        /// <summary>
        /// Product Supplier Id
        /// </summary>
        public string ProductSupplier_Id { get; set; }
        /// <summary>
        /// Requesting User
        /// </summary>
        public string User { get; set; }
    }
}
