using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI.Helpers;
using VGER_WAPI.Models;
using VGER_WAPI_CLASSES;
using VGER_WAPI_CLASSES.User;

namespace VGER_WAPI.Repositories
{
	public class UserRepository : IUserRepository
	{
		private readonly MongoContext _MongoContext = null;
		public UserRepository(IOptions<MongoSettings> settings)
		{
			_MongoContext = new MongoContext(settings);
		}

		#region Login

		public IQueryable<dynamic> GetLoginDetails(string UserName)
		{
			var result = _MongoContext.mUsers.AsQueryable().Where(x => x.UserName.ToLower() == UserName.ToLower().Trim() && x.IsActive == true);
			return result;
		}

		#endregion

		#region UserDetails

		public UserDetailsResponse GetUserDetails(string UserName)
		{
			var response = new UserDetailsResponse();
			try
			{
				//var UserDetails = (from u in _MongoContext.mUsers.AsQueryable()
				//                   join co in _MongoContext.mCompany.AsQueryable() on u.Company_Id equals co.VoyagerCompany_Id into uco
				//                   join s in _MongoContext.mSystem.AsQueryable() on u.Company_Id equals s.CoreCompany_Id into us
				//                   where u.UserName == UserName
				//                   select new { u.VoyagerUser_Id, u.UserName, u.FirstName, u.LastName, uco.First().Name, us.First().EmergencyPhoneGroups }).FirstOrDefault();
				UserName = UserName.ToLower().Trim();
				var Users = _MongoContext.mUsers.AsQueryable().Where(a => !string.IsNullOrEmpty(a.UserName) && a.UserName.ToLower() == UserName).FirstOrDefault();
				var Contact = _MongoContext.mContacts.AsQueryable().Where(a => !string.IsNullOrEmpty(a.MAIL) && a.MAIL.ToLower() == UserName).FirstOrDefault();
				var Company = _MongoContext.mCompany.AsQueryable().Where(a => Users.Company_Id == a.VoyagerCompany_Id).FirstOrDefault();
				var System = _MongoContext.mSystem.AsQueryable().FirstOrDefault(); //.Where(a => Users.Company_Id == a.CoreCompany_Id)
				var RoleList = _MongoContext.mUsersInRoles.AsQueryable().Where(a => a.UserId == Users.VoyagerUser_Id).Select(a => a.RoleId).ToList();
				var RoleDetails = _MongoContext.mRoles.AsQueryable().Where(a => RoleList.Contains(a.Voyager_Role_Id)).Select(a => new UserRoleDetails
				{
					Voyager_Role_Id = a.Voyager_Role_Id,
					RoleName = a.RoleName,
					LoweredRoleName = a.LoweredRoleName,
					Description = a.Description
				}).ToList();

				response.VoyagerUser_Id = Users.VoyagerUser_Id;
				response.CompanyId = Users.Company_Id;
				response.Photo = (Users.Photo == null) ? "" : Users.Photo;
				response.FirstName = (Contact == null || Contact.FIRSTNAME == null) ? Users.FirstName : Contact.FIRSTNAME;
				response.LastName = (Contact == null || Contact.LastNAME == null) ? Users.FirstName : Contact.LastNAME;
				response.ContactDisplayMessage = (System.EmergencyPhoneGroups == null) ? "" : System.EmergencyPhoneGroups;
				response.CreditAmount = "10000";
				response.BalanceAmount = "20000";
				response.CompanyName = (Company.Name == null) ? "" : Company.Name;
				response.Currency = "USD";
				response.UserRoleDetails = RoleDetails;
				response.ContactId = Users.Contact_Id;
				response.Status = "Success";
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				response.Status = "Error Occured";
			}
			return response;
		}

		public ContactDetailsResponse GetContactsByEmailId(ContactDetailsRequest request)
		{
			var response = new ContactDetailsResponse();
			try
			{
				request.Email = request.Email.ToLower().Trim();
				var Contacts = new mContacts();
				if (request.Users != null && !string.IsNullOrEmpty(request.Users.VoyagerUser_Id))
				{
					Contacts = _MongoContext.mContacts.AsQueryable().Where(a => !string.IsNullOrEmpty(a.MAIL) && a.MAIL.ToLower() == request.Email && a.Systemuser_id == request.Users.VoyagerUser_Id).FirstOrDefault();
				}
				else
				{
					Contacts = _MongoContext.mContacts.AsQueryable().Where(a => !string.IsNullOrEmpty(a.MAIL) && a.MAIL.ToLower() == request.Email).FirstOrDefault();
				}
				response.Contacts = Contacts;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				response.Status = "Error Occured";
			}
			return response;
		}

		public async Task<ContactDetailsResponse> UpdateUserContactDetails(ContactDetailsRequest request)
		{
			mContacts mContact = new mContacts();
			var response = new ContactDetailsResponse();

			try
			{

				if (request != null)
				{

					var resultFlag = await _MongoContext.mContacts.FindOneAndUpdateAsync(
										Builders<mContacts>.Filter.Eq("MAIL", request.Email.ToLower().Trim()),
										Builders<mContacts>.Update.Set("TEL", request.TEL).
														Set("MOBILE", request.MOBILE).
														Set("FAX", request.FAX));
					//.Set("WEB", request.WEB));

					//var Contacts = _MongoContext.mContacts.AsQueryable().Where(a => a.MAIL == request.Email).FirstOrDefault();


					response.Contacts = resultFlag;
					return response;

				}
				else
				{
					return response;

				}

			}
			catch (MongoWriteException)
			{
				//if (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
				//{
				//    // mwx.WriteError.Message contains the duplicate key error message
				//}
				return response;
			}
		}

		public async Task<ContactDetailsResponse> UpdateUserPassword(ContactDetailsRequest request)
		{
			mContacts mContact = new mContacts();
			var response = new ContactDetailsResponse();

			try
			{

				if (request != null)
				{

					string HashedPassword = Encrypt.Sha256encrypt(request.Users.Password);



					var resultUSer = _MongoContext.mUsers.FindOneAndUpdate(

						Builders<mUsers>.Filter.Eq("VoyagerUser_Id", request.Users.VoyagerUser_Id),
						Builders<mUsers>.Update.Set("Password", HashedPassword));


					var resultFlag = await _MongoContext.mContacts.FindOneAndUpdateAsync(
										Builders<mContacts>.Filter.Eq("MAIL", request.Email.ToLower().Trim()),
										Builders<mContacts>.Update.Set("TEL", request.TEL).
														Set("MOBILE", request.MOBILE).
														Set("FAX", request.FAX).
														Set("WEB", request.WEB));

					//var Contacts = _MongoContext.mContacts.AsQueryable().Where(a => a.MAIL == request.Email).FirstOrDefault();


					response.Contacts = resultFlag;
					return response;

				}
				else
				{
					return response;

				}

			}
			catch (MongoWriteException)
			{
				//if (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
				//{
				//    // mwx.WriteError.Message contains the duplicate key error message
				//}
				return response;
			}
		}

		public ContactDetailsResponse UpdateUserDetails(ContactDetailsRequest request)
		{
			mContacts mContact = new mContacts();
			var response = new ContactDetailsResponse();

			try
			{

				if (request != null)
				{
					var resultUSer = _MongoContext.mUsers.FindOneAndUpdate(

						Builders<mUsers>.Filter.Eq("VoyagerUser_Id", request.Users.VoyagerUser_Id),
						Builders<mUsers>.Update.Set("Photo", request.Users.Photo));


					//var resultFlag = await _MongoContext.mContacts.FindOneAndUpdateAsync(
					//                    Builders<mContacts>.Filter.Eq("MAIL", request.Email),
					//                    Builders<mContacts>.Update.Set("TEL", request.TEL).
					//                                    Set("MOBILE", request.MOBILE).
					//                                    Set("FAX", request.FAX).
					//                                    Set("WEB", request.WEB));

					var Contacts = _MongoContext.mContacts.AsQueryable().Where(a => a.Systemuser_id == request.Users.VoyagerUser_Id).FirstOrDefault();


					response.Contacts = Contacts;
					return response;

				}
				else
				{
					return response;

				}

			}
			catch (MongoWriteException)
			{
				//if (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
				//{
				//    // mwx.WriteError.Message contains the duplicate key error message
				//}
				return response;
			}
		}

		public UserDetailsResponse ResetUserPassword(LoginRequest request)
		{
			mContacts mContact = new mContacts();
			var response = new UserDetailsResponse();

			try
			{
				if (request != null)
				{
					string HashedPassword = Encrypt.Sha256encrypt(request.Password);
					var objCompanies = _MongoContext.mCompanies.AsQueryable().Where(x => x.Company_Id == request.CompanyId).FirstOrDefault();
					if (objCompanies != null)
					{
						var resultUSer = _MongoContext.mUsers.FindOneAndUpdate(Builders<mUsers>.Filter.Eq("UserName", request.UserName.ToLower().Trim()), Builders<mUsers>.Update.Set("Password", HashedPassword));
						var contacts = objCompanies.ContactDetails;
						if (contacts != null && contacts.Count > 0)
						{
							response.VoyagerUser_Id = resultUSer.VoyagerUser_Id;
							var usercontact = contacts.Where(a => a.Contact_Id == request.ContactId && a.UserName.ToLower() == request.UserName.ToLower().Trim()).FirstOrDefault();
							if (usercontact != null)
							{
								response.SystemCompany_Id = objCompanies.SystemCompany_Id;
								usercontact.PasswordText = request.Password;
								usercontact.Password = HashedPassword;

								mCompanies objmCompanies = _MongoContext.mCompanies.FindOneAndUpdate(Builders<mCompanies>.Filter.Eq("Company_Id", request.CompanyId), Builders<mCompanies>.Update.Set("ContactDetails", contacts));
								if (objmCompanies != null)
								{
									response.ErrorMessage = "Updated Password.";
									response.Status = "Success";
								}
								else
								{
									response.ErrorMessage = "Password Not Updated.";
									response.Status = "Error";
								}
							}
							else
							{
								response.Status = "Error";
								response.ErrorMessage = "ContactId/UserName not exists in mCompanies->ContactDetails.";
							}
						}
						else
						{
							response.Status = "Error";
							response.ErrorMessage = "ContactDetails not exists in mCompanies.";
						}
					}
					else
					{
						response.Status = "Error";
						response.ErrorMessage = "CompanyId not exists in mCompanies.";
					}
				}
				else
				{
					response.Status = "Error";
					response.ErrorMessage = "request not found.";
				}
			}
			catch (Exception ex)
			{
				response.Status = "Error";
				response.ErrorMessage = ex.Message;
			}
			return response;
		}

		public UserByRoleGetRes GetUsersByRole(UserByRoleGetReq request)
		{
			var response = new UserByRoleGetRes();
			try
			{
				var result = (from r in _MongoContext.mRoles.AsQueryable()
							  join ur in _MongoContext.mUsersInRoles.AsQueryable() on r.Voyager_Role_Id equals ur.RoleId
							  join u in _MongoContext.mUsers.AsQueryable() on ur.UserId equals u.VoyagerUser_Id
							  where request.RoleName.Contains(r.RoleName)
							  select new UserDetails
							  {
								  UserId = u.VoyagerUser_Id,
								  UserRoleId = r.Voyager_Role_Id,
								  UserRole = r.RoleName,
								  FirstName = u.FirstName,
								  LastName = u.LastName,
								  Email = u.Email,
								  CompanyId = u.Company_Id,
								  ContactId = u.Contact_Id
							  }).Distinct().ToList();
				response.Users = result.OrderBy(a => a.Email).ToList();
				response.ResponseStatus.Status = "Success";
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				response.ResponseStatus.Status = "Failure";
				response.ResponseStatus.ErrorMessage = ex.Message;
			}
			return response;
		}

		public UserByRoleGetRes GetRoleIdByRoleName(UserByRoleGetReq request)
		{
			var response = new UserByRoleGetRes();
			try
			{
				var result = (from r in _MongoContext.mRoles.AsQueryable()
							  where request.RoleName.Contains(r.RoleName)
							  select new UserDetails
							  {
								  UserRoleId = r.Voyager_Role_Id,
								  UserRole = r.RoleName
							  }).Distinct().ToList();

				response.Users = result;
				response.ResponseStatus.Status = "Success";
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				response.ResponseStatus.Status = "Failure";
				response.ResponseStatus.ErrorMessage = ex.Message;
			}
			return response;
		}

		public UserByRoleGetRes GetUserDetailsByRole(UserByRoleGetReq request)
		{
			var response = new UserByRoleGetRes();
			try
			{
				var result = (from r in _MongoContext.mRoles.AsQueryable()
							  join ur in _MongoContext.mUsersInRoles.AsQueryable() on r.Voyager_Role_Id equals ur.RoleId
							  join u in _MongoContext.mUsers.AsQueryable() on ur.UserId equals u.VoyagerUser_Id
							  where request.RoleName.Contains(r.RoleName)
							  select new UserDetails
							  {
								  Email = u.Email,
								  UserId = u.VoyagerUser_Id,
								  CompanyId = u.Company_Id,
								  ContactId = u.Contact_Id
							  }).Distinct().ToList();

				var lstCompany = result.Select(x => x.CompanyId);

				var companies = _MongoContext.mCompanies.AsQueryable().Where(x => lstCompany.Contains(x.Company_Id)).ToList();

				foreach (var r in result)
				{
					var name = companies.Where(x => x.Company_Id == r.CompanyId).Select(x => x.ContactDetails.Where(y => y.Contact_Id == r.ContactId).FirstOrDefault()).FirstOrDefault();
					r.FirstName = name != null ? name.FIRSTNAME : "";
					r.LastName = name != null ? name.LastNAME : "";
				}

				response.Users = result.OrderBy(a => a.Email).ToList();
				response.ResponseStatus.Status = "Success";
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				response.ResponseStatus.Status = "Failure";
				response.ResponseStatus.ErrorMessage = ex.Message;
			}
			return response;
		}
		public RoleGetRes GetRoles()
		{
			var response = new RoleGetRes();
			try
			{
				var result = (from r in _MongoContext.mRoles.AsQueryable()
							  select new RoleDetails
							  {
								  RoleId = r.Voyager_Role_Id,
								  RoleName = r.RoleName,
							  }).Distinct().ToList();
				response.RoleDetails = result;
				response.ResponseStatus.Status = "Success";
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				response.ResponseStatus.Status = "Failure";
				response.ResponseStatus.ErrorMessage = ex.Message;
			}
			return response;
		}



		public async Task<CommonResponse> UpdateUserForQuote(UpdateUserGetReq request)
		{
			CommonResponse response = new CommonResponse();
			try
			{
				if (request.ModuleName == "ops")
				{
					var resBooking = await _MongoContext.Bookings.FindAsync(a => a.BookingNumber == request.BookingNumber).Result.FirstOrDefaultAsync();
					var UserList = await _MongoContext.mUsers.AsQueryable().ToListAsync();
					//var CopmanyList = await _MongoContext.mCompanies.AsQueryable().ToListAsync();

					if (resBooking != null)
					{
						var SalesUser = UserList.Where(a => a.Email == request.SalesOfficer).FirstOrDefault();
						var CostingUser = UserList.Where(a => a.Email == request.CostingOfficer).FirstOrDefault();
						var PAUser = UserList.Where(a => a.Email == request.ProductAccountant).FirstOrDefault();
						var OpsUser = UserList.Where(a => a.Email == request.FileHandler).FirstOrDefault();

						if (SalesUser != null)
						{
							resBooking.StaffDetails.Staff_SalesUser_Id = SalesUser.VoyagerUser_Id;
							resBooking.StaffDetails.Staff_SalesUser_Name = SalesUser.FirstName + " " + SalesUser.LastName;
							resBooking.StaffDetails.Staff_SalesUser_Email = SalesUser.Email;
							resBooking.StaffDetails.Staff_SalesUser_Company_Id = SalesUser.Company_Id;
						}
						if (CostingUser != null)
						{
							resBooking.StaffDetails.Staff_SalesSupport_Id = CostingUser.VoyagerUser_Id;
							resBooking.StaffDetails.Staff_SalesSupport_Name = CostingUser.FirstName + " " + CostingUser.LastName;
							resBooking.StaffDetails.Staff_SalesSupport_Email = CostingUser.Email;
						}
						if (OpsUser != null)
						{
							resBooking.StaffDetails.Staff_OpsUser_Id = OpsUser.VoyagerUser_Id;
							resBooking.StaffDetails.Staff_OpsUser_Name = OpsUser.FirstName + " " + OpsUser.LastName;
							resBooking.StaffDetails.Staff_OpsUser_Email = OpsUser.Email;
							resBooking.StaffDetails.Staff_OpsUser_Company_Id = OpsUser.Company_Id;
						}
						if (PAUser != null)
						{
							resBooking.StaffDetails.Staff_PAUser_Id = PAUser.VoyagerUser_Id;
							resBooking.StaffDetails.Staff_PAUser_Name = PAUser.FirstName + " " + PAUser.LastName;
							resBooking.StaffDetails.Staff_PAUser_Email = PAUser.Email;
							resBooking.StaffDetails.Staff_PAUser_Company_Id = PAUser.Company_Id;
						}

						List<string> UserCompanyIds = new List<string>();
						UserCompanyIds.Add(SalesUser?.Company_Id);
						UserCompanyIds.Add(OpsUser?.Company_Id);
						UserCompanyIds.Add(PAUser?.Company_Id);
						var CopmanyList = await _MongoContext.mCompanies.Find(a => UserCompanyIds.Contains(a.Company_Id)).ToListAsync();
						if (string.IsNullOrEmpty(SalesUser.Company_Id))
							resBooking.StaffDetails.Staff_SalesUser_Company_Name = CopmanyList.Where(a => a.Company_Id == SalesUser.Company_Id).Select(a => a.Name).FirstOrDefault();
						if (string.IsNullOrEmpty(OpsUser.Company_Id))
							resBooking.StaffDetails.Staff_OpsUser_Company_Name = CopmanyList.Where(a => a.Company_Id == OpsUser.Company_Id).Select(a => a.Name).FirstOrDefault();
						if (string.IsNullOrEmpty(PAUser.Company_Id))
							resBooking.StaffDetails.Staff_PAUser_Company_Name = CopmanyList.Where(a => a.Company_Id == PAUser.Company_Id).Select(a => a.Name).FirstOrDefault();

						resBooking.AgentInfo.Contact_Id = request.ContactPersonID;
						resBooking.AgentInfo.Contact_Name = request.ContactPerson;
						resBooking.AgentInfo.Contact_Email = request.EmailAddress;
						resBooking.AgentInfo.Contact_Tel = request.MobileNo;

						ReplaceOneResult replaceResult = await _MongoContext.Bookings.ReplaceOneAsync(Builders<Bookings>.Filter.Eq("BookingNumber", request.BookingNumber), resBooking);
						response.ResponseStatus.Status = replaceResult.MatchedCount > 0 ? "Success" : "Failure";
						response.ResponseStatus.ErrorMessage = replaceResult.MatchedCount > 0 ? "Users updated Successfully." : "Users not updated.";
					}
				}
				else
				{
					var quote = _MongoContext.mQuote.AsQueryable().Where(a => a.QRFID == request.QRFID).FirstOrDefault();

					quote.SalesPerson = request.SalesOfficer;
					quote.CostingOfficer = request.CostingOfficer;
					//quote.ProductAccountant = request.ProductAccountant;
					quote.AgentInfo.ContactPersonID = request.ContactPersonID;
					quote.AgentInfo.ContactPerson = request.ContactPerson;
					quote.AgentInfo.EmailAddress = request.EmailAddress;
					quote.AgentInfo.MobileNo = request.MobileNo;

					quote.EditUser = request.EditUser;
					quote.EditDate = DateTime.Now;

					ReplaceOneResult replaceResult = await _MongoContext.mQuote.ReplaceOneAsync(Builders<mQuote>.Filter.Eq("QRFID", quote.QRFID), quote);
					response.ResponseStatus.Status = replaceResult.MatchedCount > 0 ? "Success" : "Failure";
					response.ResponseStatus.ErrorMessage = replaceResult.MatchedCount > 0 ? "Users updated Successfully." : "Users not updated.";

					if (response.ResponseStatus.Status == "Success")
					{
						var QRFPriceList = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == request.QRFID).ToList();

						foreach (var QRFPrice in QRFPriceList)
						{
							QRFPrice.SalesOfficer = request.SalesOfficer;
							QRFPrice.CostingOfficer = request.CostingOfficer;
							QRFPrice.ProductAccountant = request.ProductAccountant;
							QRFPrice.AgentInfo.ContactPersonID = request.ContactPersonID;
							QRFPrice.AgentInfo.ContactPerson = request.ContactPerson;
							QRFPrice.AgentInfo.EmailAddress = request.EmailAddress;
							QRFPrice.AgentInfo.MobileNo = request.MobileNo;

							QRFPrice.EditUser = request.EditUser;
							QRFPrice.EditDate = DateTime.Now;

							ReplaceOneResult replaceResultNew = await _MongoContext.mQRFPrice.ReplaceOneAsync(Builders<mQRFPrice>.Filter.Eq("QRFPrice_Id", QRFPrice.QRFPrice_Id), QRFPrice);
							response.ResponseStatus.Status = replaceResultNew.MatchedCount > 0 ? "Success" : "Failure";
							response.ResponseStatus.ErrorMessage = replaceResultNew.MatchedCount > 0 ? "Users updated Successfully." : "Users not updated.";
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				response.ResponseStatus.Status = "Failure";
				response.ResponseStatus.ErrorMessage = ex.Message;
			}
			return response;
		}

		public async Task<CommonResponse> UpdateFollowUp(UpdateUserGetReq request)
		{
			CommonResponse response = new CommonResponse();
			try
			{
				var QRFPrice = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == request.QRFID).ToList();

				foreach (var obj in QRFPrice)
				{
					obj.FollowUpCostingOfficer = Convert.ToDateTime(request.FollowUpCostingOfficer);
					obj.FollowUpWithClient = Convert.ToDateTime(request.FollowUpWithClient);
					obj.EditUser = request.EditUser;
					obj.EditDate = DateTime.Now;

					ReplaceOneResult replaceResultNew = await _MongoContext.mQRFPrice.ReplaceOneAsync(Builders<mQRFPrice>.Filter.Eq("QRFPrice_Id", obj.QRFPrice_Id), obj);
					response.ResponseStatus.Status = replaceResultNew.MatchedCount > 0 ? "Success" : "Failure";

					if (response.ResponseStatus.Status.ToLower() == "success")
						response.ResponseStatus.StatusMessage = "Followup details updated successfully.";
					else
						response.ResponseStatus.ErrorMessage = "Followup details not updated.";
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				response.ResponseStatus.Status = "Failure";
				response.ResponseStatus.ErrorMessage = ex.Message;
			}
			return response;
		}

		#endregion

		#region User Role
		/// <summary>
		/// GetUserRoleDetails service if User is blank/Null then it will return all Role Details from master mRoles
		/// If UserId is passed then it will return user roles along with all roles from master mRoles
		/// If userId doesn't have any roles then it will return all the roles from master mRoles
		/// if user Id is not valid then it returns Error as status
		/// </summary>
		/// <param name="request">UserRoleGetReq takes UserID </param>
		/// <returns>it returns User Role Details</returns>
		public async Task<UserRoleGetRes> GetUserRoleDetails(UserRoleGetReq request)
		{
			var userrolelist = new List<UserRoles>();
			var response = new UserRoleGetRes() { UserId = request.UserID, ResponseStatus = new ResponseStatus() };
			try
			{
				var rolelist = _MongoContext.mRoles.AsQueryable().ToList();
				if (rolelist != null && rolelist.Count > 0)
				{
					if (!string.IsNullOrEmpty(request.UserID))
					{
						//var resUser = await _MongoContext.mUsers.FindAsync(a => a.VoyagerUser_Id == request.UserID).Result.FirstOrDefaultAsync();
						//if (resUser != null)
						//{
						//var userrolelist = _MongoContext.mUsersInRoles.AsQueryable().Where(a => a.UserId == request.UserID).ToList();

						if (!string.IsNullOrWhiteSpace(request.CompanyID) && !string.IsNullOrWhiteSpace(request.ContactID))
						{
							userrolelist = _MongoContext.mCompanies.AsQueryable().Where(x => x.Company_Id == request.CompanyID).FirstOrDefault()?.ContactDetails?.Where(x => x.Contact_Id == request.ContactID).FirstOrDefault()?.Roles?.ToList();
						}
						else {
							response.ResponseStatus.ErrorMessage = "Contact does not exists.";
					     	response.ResponseStatus.Status = "Error";
						}

						if (userrolelist != null && userrolelist.Count > 0)
							{
								var notexistsroles = rolelist.Where(a => !userrolelist.Select(b => b.RoleId).ToList().Contains(a.Voyager_Role_Id))
									.Select(a => new UserRolesDetails
									{
										IsRoled = false,
										RoleId = a.Voyager_Role_Id,
										RoleName = a.RoleName,
										UserId = request.UserID
									}).ToList();

								notexistsroles.AddRange(userrolelist.Select(a => new UserRolesDetails
								{
									IsRoled = true,
									RoleId = a.RoleId,
									RoleName = rolelist.Where(x => x.Voyager_Role_Id == a.RoleId).FirstOrDefault()?.RoleName,
									UserId = a.User_id
								}).ToList());
								response.UserRolesDetails = notexistsroles;
							}
							else
							{
								response.UserRolesDetails = rolelist.Select(a => new UserRolesDetails
								{
									UserId = request.UserID,
									IsRoled = false,
									RoleId = a.Voyager_Role_Id,
									RoleName = a.RoleName
								}).ToList();
							}

							response.ResponseStatus.Status = "Success";

						//}
						//else
						//{
						//	response.ResponseStatus.ErrorMessage = "UserID not exists.";
						//	response.ResponseStatus.Status = "Error";
						//}
					}
					else
					{
						response.UserRolesDetails = rolelist.Select(a => new UserRolesDetails
						{
							UserId = "",
							IsRoled = false,
							RoleId = a.Voyager_Role_Id,
							RoleName = a.RoleName
						}).ToList();
						response.ResponseStatus.Status = "Success";
					}
				}
				else
				{
					response.ResponseStatus.ErrorMessage = "Role details not found.";
					response.ResponseStatus.Status = "Error";
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				response.ResponseStatus.Status = "Failure";
				response.ResponseStatus.ErrorMessage = ex.Message;
			}
			return response;
		}

		/// <summary>
		/// SetUserRoleDetails service set the userroles in mUsersInRoles collection
		/// If userId already have roles then 1st it will delete all roles of that user and then insert user roles details
		/// if user id not valid then it gives Error Message
		/// </summary>
		/// <param name="request">takes UserId,UserName,EditUser and UserRoles details</param>
		/// <returns>If UserRoles updated/inserted succesfully then it will Return Success else Error</returns>
		public async Task<UserRoleSetRes> SetUserRoleDetails(UserRoleSetReq request)
		{
			var response = new UserRoleSetRes() { ResponseStatus = new ResponseStatus() };
			try
			{
				if (request.UserRoleDetailsList != null && request.UserRoleDetailsList.Count > 0)
				{
					if (!string.IsNullOrEmpty(request.UserId))
					{
						var resUser = await _MongoContext.mUsers.FindAsync(a => a.VoyagerUser_Id == request.UserId).Result.FirstOrDefaultAsync();
						var company = _MongoContext.mCompanies.AsQueryable().Where(x => x.Company_Id == request.CompanyId).FirstOrDefault();
						var resContact = company.ContactDetails.Where(x => x.Contact_Id == request.ContactId).FirstOrDefault();
						var roles = _MongoContext.mRoles.AsQueryable().ToList();

						if (resUser != null)
						{
							var userroles = request.UserRoleDetailsList.Where(x => x.IsRoled == true).Select(a => new mUsersInRoles
							{
								UserId = request.UserId,
								CreateDate = DateTime.Now,
								CreateUser = request.EditUser,
								RoleId = a.RoleId,
								UserName = request.UserName,
								RoleName = a.RoleName
							});

							var userrolelist = _MongoContext.mUsersInRoles.AsQueryable().Where(a => a.UserId == request.UserId).ToList();
							if (userrolelist != null && userrolelist.Count > 0)
							{
								await _MongoContext.mUsersInRoles.DeleteManyAsync(Builders<mUsersInRoles>.Filter.Eq("UserId", request.UserId));
								if (resContact.Roles != null && resContact.Roles.Count > 0)
								{
									resContact.Roles = null;

									await _MongoContext.mCompanies.UpdateOneAsync(Builders<mCompanies>.Filter.Eq("Company_Id", request.CompanyId),
									Builders<mCompanies>.Update.Set("ContactDetails", company.ContactDetails)
									.Set("EditUser", request.EditUser).Set("EditDate", DateTime.Now));
								}

								if (userroles.Count() > 0)
								{
									await _MongoContext.mUsersInRoles.InsertManyAsync(userroles);

									var users = new List<mUsersInRoles>();
									if (!string.IsNullOrWhiteSpace(request.UserName))
										users = _MongoContext.mUsersInRoles.AsQueryable().Where(x => x.UserName.ToLower() == request.UserName.ToLower().Trim()).ToList();
									List<UserRoles> lstuserroles = new List<UserRoles>();
									foreach (var u in users)
									{
										var role = new UserRoles();
										role.UserRole_Id = u._Id.ToString();
										role.User_id = u.UserId;
										role.RoleId = u.RoleId;
										role.UserName = u.UserName;
										role.RoleName = u.RoleName;
										role.Description = roles.Where(x => x.Voyager_Role_Id == u.RoleId).Select(x => x.Description).FirstOrDefault();
										role.BackOffice = roles.Where(x => x.Voyager_Role_Id == u.RoleId).Select(x => x.BAckoffice).FirstOrDefault();
										lstuserroles.Add(role);
									}
									resContact.Roles = lstuserroles;

									await _MongoContext.mCompanies.UpdateOneAsync(Builders<mCompanies>.Filter.Eq("Company_Id", request.CompanyId),
									Builders<mCompanies>.Update.Set("ContactDetails", company.ContactDetails)
									.Set("EditUser", request.EditUser).Set("EditDate", DateTime.Now));
								}
							}
							else
							{
								await _MongoContext.mUsersInRoles.InsertManyAsync(userroles);

								var users = _MongoContext.mUsersInRoles.AsQueryable().Where(x => x.UserName.ToLower() == request.UserName.ToLower().Trim()).ToList();
								List<UserRoles> lstuserroles = new List<UserRoles>();
								foreach (var u in users)
								{
									var role = new UserRoles();
									role.UserRole_Id = u._Id.ToString();
									role.User_id = u.UserId;
									role.RoleId = u.RoleId;
									role.UserName = u.UserName;
									role.RoleName = u.RoleName;
									role.Description = roles.Where(x => x.Voyager_Role_Id == u.RoleId).Select(x => x.Description).FirstOrDefault();
									role.BackOffice = roles.Where(x => x.Voyager_Role_Id == u.RoleId).Select(x => x.BAckoffice).FirstOrDefault();
									lstuserroles.Add(role);
								}
								resContact.Roles = lstuserroles;

								await _MongoContext.mCompanies.UpdateOneAsync(Builders<mCompanies>.Filter.Eq("Company_Id", request.CompanyId),
								Builders<mCompanies>.Update.Set("ContactDetails", company.ContactDetails)
								.Set("EditUser", request.EditUser).Set("EditDate", DateTime.Now));

							}
							response.ResponseStatus.Status = "Success";
							response.ResponseStatus.ErrorMessage = "User Role Details Updated Successfully.";
						}
						else
						{
							response.ResponseStatus.ErrorMessage = "User ID not exists.";
							response.ResponseStatus.Status = "Error";
						}
					}
					else
					{
						response.ResponseStatus.ErrorMessage = "UserID can not be Null/Blank.";
						response.ResponseStatus.Status = "Error";
					}
				}
				else
				{
					response.ResponseStatus.ErrorMessage = "User Role Details can not be Null/Empty.";
					response.ResponseStatus.Status = "Error";
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				response.ResponseStatus.Status = "Failure";
				response.ResponseStatus.ErrorMessage = ex.Message;
			}
			return response;
		}

		public async Task<UserSetRes> CreateUser(UserSetReq request)
		{
			UserSetRes response = new UserSetRes();
			try
			{
				var muser = _MongoContext.mUsers.AsQueryable().Where(x => x.Email.ToLower() == request.User.Email.Trim().ToLower()).FirstOrDefault();
				//var existinguser = _MongoContext.mUsers.AsQueryable().Where(x => x.Contact_Id == request.User.Contact_Id && x.Email == request.User.Email).FirstOrDefault();
				if (muser != null)
				{
					response.UserId = muser.VoyagerUser_Id;
					response.Email = muser.Email;
					response.UserName = muser.UserName;
					response.ResponseStatus.Status = "Error";
					response.ResponseStatus.ErrorMessage = "User is already created with same Email Id";
				}
				else
				{
					//Add
					mUsers user = new mUsers();
					user.VoyagerUser_Id = Guid.NewGuid().ToString();
					user.UserName = request.User.UserName ?? request.User.Email;
					user.FirstName = request.User.FirstName;
					user.LastName = request.User.LastName;
					user.Email = request.User.Email;
					user.Password = request.User.Password;
					user.IsActive = request.User.IsActive;
					user.IsLocked = request.User.IsLocked;
					user.IsAgent = request.User.IsAgent;
					user.IsSuppplier = request.User.IsSuppplier;
					user.IsStaff = request.User.IsStaff;
					user.LastLoginDate = request.User.LastLoginDate;
					user.Manager = request.User.Manager;
					user.Contact_Id = request.User.Contact_Id;
					user.Company_Id = request.User.Company_Id;
					user.CreateUser = request.User.CreateUser;
					user.CreateDate = DateTime.Now;
					user.Photo = request.User.Photo;

					response.UserId = user.VoyagerUser_Id;
					response.UserName = user.UserName;

					await _MongoContext.mUsers.InsertOneAsync(user);

					if (!string.IsNullOrEmpty(request.User.Company_Id) && !string.IsNullOrEmpty(request.User.Contact_Id))
					{
						var result = _MongoContext.mCompanies.AsQueryable().Where(a => a.Company_Id == request.User.Company_Id && a.ContactDetails.Any(b => b.Contact_Id == request.User.Contact_Id)).FirstOrDefault();
						if (result != null)
						{
							var compresult = await _MongoContext.mCompanies.FindOneAndUpdateAsync(m => m.Company_Id == request.User.Company_Id && m.ContactDetails.Any(a => a.Contact_Id == request.User.Contact_Id),
											Builders<mCompanies>.Update.Set(m => m.ContactDetails[-1].Systemuser_id, user.VoyagerUser_Id));
						}
					}
					response.ResponseStatus.Status = "Success";
					response.ResponseStatus.ErrorMessage = "Details saved successfully";
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				response.ResponseStatus.Status = "Failure";
				response.ResponseStatus.ErrorMessage = ex.Message;
			}
			return response;
		}

		public async Task<UserSetRes> EnableDisableUser(UserSetReq request)
		{
			UserSetRes response = new UserSetRes();
			try
			{
				var user = _MongoContext.mUsers.AsQueryable().Where(x => x.VoyagerUser_Id == request.User.VoyagerUser_Id).FirstOrDefault();
				if (user != null)
				{
					user.IsActive = request.User.IsActive;

					_MongoContext.mUsers.FindOneAndUpdate(Builders<mUsers>.Filter.Eq("VoyagerUser_Id", user.VoyagerUser_Id),
															Builders<mUsers>.Update.Set("IsActive", user.IsActive));

					//ReplaceOneResult replaceResult = await _MongoContext.mUsers.ReplaceOneAsync(Builders<mUsers>.Filter.Eq("VoyagerUser_Id", user.VoyagerUser_Id), user);
					response.ResponseStatus.Status = "Success";
					response.ResponseStatus.ErrorMessage = "Details updated successfully.";
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				response.ResponseStatus.Status = "Failure";
				response.ResponseStatus.ErrorMessage = ex.Message;
			}
			return response;
		}

		public async Task<UserSetRes> UpdateUser(UserSetReq request)
		{
			UserSetRes response = new UserSetRes();
			try
			{
				var existinguser = _MongoContext.mUsers.AsQueryable().Where(x => x.VoyagerUser_Id == request.User.VoyagerUser_Id).FirstOrDefault();
				if (existinguser != null)
				{

					if (request.User.LastLoginDate != null && request.User.LastLoginDate != DateTime.MinValue && !string.IsNullOrWhiteSpace(request.User.VoyagerUser_Id))
					{
						_MongoContext.mUsers.FindOneAndUpdate(Builders<mUsers>.Filter.Eq("VoyagerUser_Id", request.User.VoyagerUser_Id),
						Builders<mUsers>.Update.Set("LastLoginDate", request.User.LastLoginDate));
					}
					else
					{
						//Update
						request.User.UserName = request.User.UserName ?? existinguser.UserName;
						request.User.FirstName = request.User.FirstName ?? existinguser.FirstName;
						request.User.LastName = request.User.LastName ?? existinguser.LastName;
						request.User.Email = request.User.Email ?? existinguser.Email;
						request.User.EditUser = request.User.EditUser;
						request.User.EditDate = DateTime.Now;

						//string HashedPassword = "";
						//string password = request.User.Password;
						//if (!string.IsNullOrEmpty(password)) HashedPassword = Encrypt.Sha256encrypt(password);

						//request.User.Password = HashedPassword ?? existinguser.Password;

						var resultUSer = _MongoContext.mUsers.FindOneAndUpdate(Builders<mUsers>.Filter.Eq("VoyagerUser_Id", request.User.VoyagerUser_Id),
						   Builders<mUsers>.Update.Set("UserName", request.User.UserName).Set("FirstName", request.User.FirstName)
						   .Set("LastName", request.User.LastName).Set("Email", request.User.Email).Set("EditUser", request.User.EditUser).Set("EditDate", request.User.EditDate));

						response.UserId = existinguser.VoyagerUser_Id;
						response.ResponseStatus.Status = "success";
						response.ResponseStatus.ErrorMessage = "User is updated";
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				response.ResponseStatus.Status = "Failure";
				response.ResponseStatus.ErrorMessage = ex.Message;
			}
			return response;
		}

		public async Task<bool> CheckExistingEmail(string emailId)
		{
			try
			{
				emailId = emailId.ToLower().Trim();
				if (emailId != null)
				{
					bool isDuplicateInCompanies = false;

					var isDuplicateInUser = _MongoContext.mUsers.AsQueryable().Where(x => x.Email.ToLower() == emailId).FirstOrDefault();
					var companies = _MongoContext.mCompanies.AsQueryable();
					var contactlist = companies.Where(a => a.ContactDetails != null && a.ContactDetails.Count > 0
										  && a.ContactDetails.Any(b => !string.IsNullOrEmpty(b.MAIL) && b.MAIL.ToLower() == emailId)).FirstOrDefault();

					if (contactlist != null)
					{
						isDuplicateInCompanies = true;
					}

					//var contactlist = _MongoContext.mCompanies.AsQueryable().Select(x => x.ContactDetails).ToList();
					//foreach (var c in contactlist)
					//{
					//    isDuplicateInCompanies = c.Select(x => x).Any(x => !string.IsNullOrEmpty(x.MAIL) && x.MAIL.ToLower().Trim() == emailId);
					//    if (isDuplicateInCompanies)
					//    {
					//        break;
					//    }
					//}

					if (isDuplicateInUser != null || isDuplicateInCompanies == true)
						return true;
					else
						return false;
				}
				return false;
			}
			catch (Exception ex)
			{
				throw;
			}
		}

		#endregion

		#region mContacts
		public List<UserSystemContactDetails> GetActiveUserSystemContactDetailsByRole(string RoleName)
		{
			List<UserSystemContactDetails> lstUserSystemContactDetails = new List<UserSystemContactDetails>();
			var compnyids = _MongoContext.mSystem.AsQueryable().Select(a => a.CoreCompany_Id).Distinct().ToList();
			if (compnyids != null && compnyids.Count > 0)
			{
				var userRolesList = _MongoContext.mUsersInRoles.AsQueryable().Where(a => a.RoleName.ToLower() == RoleName.ToLower()).Select(a => a.UserId).ToList();
				if (userRolesList != null && userRolesList.Count > 0)
				{
					var users = _MongoContext.mUsers.AsQueryable().Where(a => userRolesList.Contains(a.VoyagerUser_Id)).Select(a => a.Contact_Id).ToList();
					if (users != null && users.Count > 0)
					{
						List<string> strDeactive = new List<string> { "X", "-", "x" };
						lstUserSystemContactDetails = _MongoContext.mContacts.AsQueryable().Where(a => compnyids.Contains(a.Company_Id) && !strDeactive.Contains(a.STATUS) && users.Contains(a.VoyagerContact_Id)).Select(a => new UserSystemContactDetails { VoygerContactId = a.VoyagerContact_Id, FirstName = a.FIRSTNAME, LastName = a.LastNAME, IsOperationDefault = a.IsOperationDefault }).Distinct().ToList();
					}
				}
			}

			return lstUserSystemContactDetails;
		}
		#endregion

		#region GetIntegrationLoginDetails

		/// <summary>
		/// Method to get user details with provided "applicationKey", "userKey" value by the source like 'CRM' etc
		/// </summary>
		/// <param>applicationKey, userKey, source</param>
		/// <returns></returns>
		public IQueryable<dynamic> GetIntegrationLoginDetails(string applicationKey, string userKey, string source)
		{
			if (!string.IsNullOrEmpty(applicationKey) && !string.IsNullOrEmpty(userKey))
			{

				if (!string.IsNullOrEmpty(source))
				{
					var selectedUser = GetintegrationSignatureStrOne(applicationKey, source);
					if (selectedUser != null)
					{
						var str2 = GetintegrationSignatureStrTwo(selectedUser, userKey, source);
						if (selectedUser.Where(a => a.Email == str2).Any())
						{
							return selectedUser;
						}
					}
				}
			}
			return null;
		}

		#endregion

		#region CommonIntegrationSignature

		/// <summary>
		/// Method to get mUser data with provided "integrationSignature" value by the source like 'CRM' etc
		/// </summary>
		/// <param>integrationSignature, source</param>
		/// <returns>mUser</returns>
		/// 
		public IQueryable<mUsers> GetintegrationSignatureStrOne(string integrationSignatureStrOne, string source)
		{
			var applicationId = _MongoContext.mApplications.AsQueryable().Where(a => a.Application_Name.ToLower() == source.ToLower()).Select(x => x.Application_Id).FirstOrDefault();

			string str1 = Encrypt.DecryptData(applicationId, integrationSignatureStrOne.Replace("|", "/"));
			var result = _MongoContext.mUsers.AsQueryable().Where(a => a.Email == str1);

			return result;
		}

		/// <summary>
		/// Method to get Str 2 data with provided "userKey" as "integrationSignatureStrTwo" value by the source like 'CRM' etc
		/// </summary>
		/// <param>selectedUser, integrationSignatureStrTwo, source</param>
		/// <returns>Str 2</returns>
		/// 
		public string GetintegrationSignatureStrTwo(IQueryable<mUsers> selectedUser, string integrationSignatureStrTwo, string source)
		{
			var appKeyList = selectedUser.Where(a => a.App_Keys.Any(b => b.Application_Name.ToLower() == source.ToLower() && b.Status == "")).Select(x => x.App_Keys).FirstOrDefault();
			var userKey = (appKeyList != null && appKeyList.Any()) ? appKeyList.Where(a => a.Application_Name.ToLower() == source.ToLower() && a.Status == "").FirstOrDefault() : null;
			string str2 = (userKey != null && !string.IsNullOrEmpty(userKey.Key)) ? Encrypt.DecryptData(userKey.Key, integrationSignatureStrTwo.Replace("|", "/")) : "";

			return str2;
		}

		#endregion

		public void GetUserCompanyType(ref UserCookieDetail userdetails)
		{
			string UserName = userdetails.UserName.ToLower().Trim();
			var Users = _MongoContext.mUsers.AsQueryable().Where(a => a.UserName.ToLower() == UserName).FirstOrDefault();
			var Company = _MongoContext.mCompany.AsQueryable().Where(a => Users.Company_Id == a.VoyagerCompany_Id).FirstOrDefault();

			if ((Company.Issupplier ?? false))
				userdetails.IsSupplier = true;
			if ((Company.Iscustomer ?? false) || (Company.Issubagent ?? false))
				userdetails.IsAgent = true;

			//res.UserName = UserName;
			//res.RoleName = userdetails.RoleName;
			//res.CompanyName = userdetails.CompanyName;
			userdetails.Company_Id = Company.VoyagerCompany_Id;
			userdetails.Contact_Id = Users.Contact_Id;
			// return res;
		}
	}
}
