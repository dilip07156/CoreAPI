using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class LinkedQRFsGetRes
    {
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
        public List<LinkedQRFsData> LinkedQRFsDataList { get; set; } = new List<LinkedQRFsData>();

    }

    public class LinkedQRFsData
    {
        public string QRFID { get; set; }

        public string Parent_QRFID { get; set; }
        public string CurrentPipeline { get; set; }
        public string AgentName { get; set; }
        public int? Duration { get; set; }
        public string TourName { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateDate { get; set; }
    }
}
