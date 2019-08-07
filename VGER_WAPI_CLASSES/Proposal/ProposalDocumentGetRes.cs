using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI_CLASSES
{
    /// <summary>
    /// Proposal Document service Get Response class
    /// </summary>
    public class ProposalDocumentGetRes
    {
        /// <summary>
        /// System Phone
        /// </summary>
        public string SystemPhone { get; set; }
        
        /// <summary>
        /// System Email
        /// </summary>
        public string SystemEmail { get; set; }

        /// <summary>
        /// System Website
        /// </summary>
        public string SystemWebsite { get; set; }

        /// <summary>
        /// QRF Form Details
        /// </summary>
        public mQRFPrice QRFQuote { get; set; } = new mQRFPrice();

        /// <summary>
        /// Proposal Details
        /// </summary>
        public mProposal Proposal { get; set; } = new mProposal();

        /// <summary>
        /// Itenerary Details
        /// </summary>
        public mItinerary Itinerary { get; set; } = new mItinerary();

        /// <summary>
        /// Images Details Collection
        /// </summary>
        public List<Images> ProductImages { get; set; } = new List<Images>();

        /// <summary>
        /// Product and Hotel images from Generic Images Collection
        /// </summary>
        public List<mGenericImages> GenericImages { get; set; } = new List<mGenericImages>();

        /// <summary>
        /// Response Details
        /// </summary>
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
    }

    /// <summary>
    /// Images Details
    /// </summary>
    public class Images
    {
        /// <summary>
        /// Image Indentifier
        /// </summary>
        public string ImageIdentifier { get; set; }

        /// <summary>
        /// Image Name
        /// </summary>
        public string ImageName { get; set; }

        /// <summary>
        /// Image URL
        /// </summary>
        public string ImageURL { get; set; }

    }
}
