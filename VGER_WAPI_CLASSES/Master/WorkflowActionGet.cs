using System.Collections.Generic; 

namespace VGER_WAPI_CLASSES
{
    public class WorkflowActionGetReq
    {
        public string ModuleParent { get; set; }
        public string Module { get; set; }
        public string Action { get; set; }
        public List<OpsKeyValue> OpsKeyValue { get; set; }

    }

    public class WorkflowActionGetRes
    {
        public List<Workflow_Actions> WorkflowActions { get; set; } = new List<Workflow_Actions>();
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
    }

    public class ExecuteWorkflowActionRes
    {
        public Bookings Bookings { get; set; } = new Bookings();
        public OPSWorkflowResponseStatus ResponseStatus { get; set; } = new OPSWorkflowResponseStatus();
    }

    public class ExecuteWorkflowActionReq
    {
        public List<OpsKeyValue> OpsKeyValue { get; set; }
        public List<string> PositionIds { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmailId { get; set; }
        public string DocType { get; set; }
        public string Module { get; set; }
        public bool IsSendEmail { get; set; }
        public bool IsSaveDocStore { get; set; }
    } 
}