using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VGER_WAPI_CLASSES
{
	public class mSalesPipelineRoles
	{
		[BsonId]
		public ObjectId _Id { get; set; } = ObjectId.GenerateNewId();
		public string SystemCompany_Id { get; set; }
		public string Type { get; set; }
		public List<Values> Values { get; set; } = new List<Values>();
	}

	public class Values
	{		
		public string TypeId { get; set; }
		public string TypeName { get; set; }
		public string RoleId { get; set; }
		public string RoleName { get; set; }
		public string UserId { get; set; }
		public string UserName { get; set; }
		public string Status { get; set; }
		public string CreateUser { get; set; }
		public DateTime CreateDate { get; set; }
		public string EditUser { get; set; }
		public DateTime? EditDate { get; set; }
	}
}
