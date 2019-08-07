using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class mRoles
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        public string Voyager_Role_Id { get; set; }
        public string ApplicationId { get; set; } 
        public string RoleName { get; set; }
        public string LoweredRoleName { get; set; }
        public string Description { get; set; }
        public bool Web { get; set; }
        public bool BAckoffice { get; set; }
        public string CreateUser { get; set; }
        public DateTime CreateDate { get; set; }
        public string EditUser { get; set; }
        public DateTime? EditDate { get; set; }
    }
}
