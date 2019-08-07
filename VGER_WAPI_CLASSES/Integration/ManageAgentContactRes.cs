using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class ManageAgentContactRes
    {
        public ManageAgentContactRes()
        {
            CompanyInfo = new mCompanies();
            ResponseStatus = new ResponseStatus();
        }

        /// <summary>
        /// mCompanies info is set after record is updated or saved as return.
        /// </summary>
        public mCompanies CompanyInfo { get; set; }

        /// <summary>
        /// Response Status for Success, Failure Duplicate etc with Message.
        /// </summary>
        public ResponseStatus ResponseStatus { get; set; }
    }
}
