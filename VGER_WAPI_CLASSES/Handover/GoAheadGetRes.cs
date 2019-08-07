using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace VGER_WAPI_CLASSES
{
    public class GoAheadGetRes
    {
        public GoAheadGetRes()
        {
            CostsheetVersion = new List<CostsheetVersion>();
            UserSystemContactDetails = new List<UserSystemContactDetails>();
            mGoAhead = new mGoAhead();
            ResponseStatus = new ResponseStatus();
        }
        public mGoAhead mGoAhead { get; set; }
        public List<CostsheetVersion> CostsheetVersion { get; set; }
        public List<UserSystemContactDetails> UserSystemContactDetails { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }

    public class GoAheadNewDeptGetRes
    {
        public GoAheadNewDeptGetRes()
        {
            ExisitingDepatures = new List<ExisitingDepatures>();
            NewDepatures = new List<NewDepatures>();
            ResponseStatus = new ResponseStatus();
        }
        public string GoAheadId { get; set; }
        public string QRFID { get; set; }
        public string ExistDepartureId { get; set; }
        public List<ExisitingDepatures> ExisitingDepatures { get; set; }
        public List<NewDepatures> NewDepatures { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }

    public class ExisitingDepatures
    {
        public long DepatureId { get; set; }
        public DateTime? DepatureDate { get; set; } 
        public double PPTwin { get; set; }
        public string Currency { get; set; }
    }

    public class NewDepatures
    { 
        public DateTime? DepatureDate { get; set; }
    }
}
