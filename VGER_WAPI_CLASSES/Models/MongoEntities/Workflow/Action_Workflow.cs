using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace VGER_WAPI_CLASSES
{
    public class Workflow_Actions
    {
        [BsonId]
        public ObjectId _Id { get; set; }

        public string Module { get; set; }
        public string ModuleParent { get; set; }
        public string Action { get; set; }
        public List<Workflow_ModificationType> ModificationType { get; set; }
        public string SubType { get; set; }
        public List<Workflow_Steps> Steps { get; set; }
        public List<Workflow_Steps> BridgeSteps { get; set; }
    }

    public class Workflow_ModificationType
    {
        public string field { get; set; }
    }

    public class Workflow_Steps
    {
        public int Order { get; set; }
        public string StepName { get; set; }
        public string StepNameParent { get; set; }
        public List<Workflow_ModificationType> ModificationType { get; set; }
        public string FunctionName { get; set; }
        public string FunctionType { get; set; }
        public bool? IsDbCommit { get; set; }

    }
}
