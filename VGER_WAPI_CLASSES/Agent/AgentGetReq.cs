using System;

namespace VGER_WAPI_CLASSES
{
    public class AgentGetReq
    {
        public string UserId { get; set; }
        public string ContactId { get; set; }
        public string CompanyId { get; set; }
        public string AgentReference { get; set; }
        public string AgentId { get; set; }
        public string AgentName { get; set; }
        public Guid? CountryId { get; set; }
        public string Status { get; set; }
        public string UserName { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public string ActionType { get; set; }
    }
}
