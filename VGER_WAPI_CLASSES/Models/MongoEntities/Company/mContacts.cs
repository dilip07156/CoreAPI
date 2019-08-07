using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{

    public class CompanyContacts
    {
        public string Contact_Id { get; set; }
        public string Company_Id { get; set; }
        public string Company_Name { get; set; }
        public byte? Default { get; set; }
        public string ActualContact_Id_AsShared { get; set; }
        public string ActualCompany_Id_AsShared { get; set; }
        public string ActualCompany_Name_AsShared { get; set; }
        public string CommonTitle { get; set; }
        public string TITLE { get; set; }
        public string FIRSTNAME { get; set; }
        public string LastNAME { get; set; }
        public string TEL { get; set; }
        public string MOBILE { get; set; }
        public string FAX { get; set; }
        public string MAIL { get; set; }
        public string WEB { get; set; }

        public string DEPARTMENT { get; set; }
        public string StartPage_Id { get; set; }
        public string Start_Page { get; set; }

        public string Systemuser_id { get; set; }
        public string UserName { get; set; }
        public string PasswordText { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public string PasswordQuestion { get; set; }
        public string PasswordAnswer { get; set; }
        public int? PasswordFormat { get; set; }
        public bool? IsApproved { get; set; }
        public bool? IsLockedOut { get; set; }

        public List<UserRoles> Roles { get; set; }
        [BsonIgnoreIfNull(true)]
        public List<ContactMappings> Mappings { get; set; }
        
        [BsonIgnoreIfNull(true)]
        public List<Targets> Targets { get; set; }

        public string STATUS { get; set; }
        public int? ContactOrder { get; set; }
        public bool IsOperationDefault { get; set; }
        public bool IsCentralEmail { get; set; }
        public string CreateUser { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CreateDate { get; set; }
        public string EditUser { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EditDate { get; set; }
    }

    public class UserRoles
    {
        public string UserRole_Id { get; set; }
        public string User_id { get; set; }
        public string UserName { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
        public bool? BackOffice { get; set; }
    }

    public class EmergencyContacts
    {
        public string EmergencyContact_Id { get; set; }
        public string Company_Id { get; set; }
        public string Country_Id { get; set; }
        public string Country { get; set; }
        public string EmergencyNo { get; set; }
        public bool? Default { get; set; }
        public string Contact_Id { get; set; }
        public string ContactName { get; set; }
        public string ContactMail { get; set; }
        public string ContactTel { get; set; }
        public string BusiType { get; set; }
        public string Status { get; set; }
    }

    public class mContacts
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        public string VoyagerContact_Id { get; set; }
        public string Company_Id { get; set; }
        public int? Default { get; set; }
        public string ActualContact_Id_AsShared { get; set; }
        public string ActualCompany_Id_AsShared { get; set; }
        public string CommonTitle { get; set; }
        public string TITLE { get; set; }
        public string FIRSTNAME { get; set; }
        public string LastNAME { get; set; }
        public string TEL { get; set; }
        public string MOBILE { get; set; }
        public string FAX { get; set; }
        public string MAIL { get; set; }
        public string WEB { get; set; }
        public string STATUS { get; set; }
        public string Systemuser_id { get; set; }
        public int? ContactOrder { get; set; }
        public bool IsOperationDefault { get; set; }
        public bool IsCentralEmail { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public string EditUser { get; set; }
        public DateTime? EditDate { get; set; }
        public string CompanyName { get; set; }
    }

    public class ContactMappings
    {
        public string Application_Id { get; set; }
        public string Application { get; set; }
        public string PartnerEntityCode { get; set; }
        public string PartnerEntityName { get; set; }
        public string PartnerEntityType { get; set; }
        public string Action { get; set; }
        public string Status { get; set; }
        public string AdditionalInfoType { get; set; }
        public string AdditionalInfo { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public string EditUser { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EditDate { get; set; }
    }
}
