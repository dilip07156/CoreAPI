using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class ProposalGetReq
    {
        public string QRFID { get; set; }
        public string ProposalId { get; set; }
        public string Section { get; set; }
		public string DocType { get; set; }
	}
}
