using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class mUsers
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        public string VoyagerUser_Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; }
        public bool IsLocked { get; set; }
        public bool IsAgent { get; set; }
        public bool IsSuppplier { get; set; }
        public bool IsStaff { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime LastLoginDate { get; set; }
        public string Manager { get; set; }
        public string Contact_Id { get; set; }
        public string Company_Id { get; set; }
        public string CreateUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateDate { get; set; }
        public string EditUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime EditDate { get; set; }

        public string Photo { get; set; }

        public List<Integration_App_Keys> App_Keys { get; set; }

    }

    public class Integration_App_Keys
    {
        public string Application_Id { get; set; }
        public string Application_Name { get; set; }
        public string Key { get; set; }
        public string Status { get; set; }
        public string CreateUser { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateDate { get; set; }
        public string EditUser { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime EditDate { get; set; }
    }
}
