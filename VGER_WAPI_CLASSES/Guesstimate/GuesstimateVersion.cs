using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VGER_WAPI_CLASSES
{
    public class GuesstimateVersion
    {
        public string QRFID { get; set; }
        public string GuesstimateId { get; set; }
        public int VersionId { get; set; }
        public string VersionName { get; set; }
        public string VersionDescription { get; set; }
        public bool IsCurrentVersion { get; set; }

        public DateTime VersionCreateDate { get; set; }
    }
}
