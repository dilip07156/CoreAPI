using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    /// <summary>
    /// QuoteGetReq used for taking QRFID and UserName
    /// </summary>
    public class QuoteGetReq
    {
        /// <summary>
        /// QRF ID
        /// </summary>
        public string QRFID { get; set; }

        /// <summary>
        /// User Name
        /// </summary>
        public string UserName { get; set; } = "";

        /// <summary>
        /// Current Pipeline Step
        /// </summary>
        public string CurrentPipelineStep { get; set; }
    }
}
