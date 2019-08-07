using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VGER_WAPI.Helpers;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI.Helpers;
using VGER_WAPI.Models;
using VGER_WAPI_CLASSES;
using MongoDB.Driver;

namespace VGER_WAPI.Repositories
{
	public class SupplierRepository : ISupplierRepository
	{
		#region Private Variable Declaration
		private readonly MongoContext _MongoContext = null;
		#endregion

		#region Supplier
		public SupplierRepository(IOptions<MongoSettings> settings)
		{
			_MongoContext = new MongoContext(settings);
		}

		public async Task<SupplierGetRes> GetSupplierData(SupplierGetReq request)
		{
			SupplierGetRes response = new SupplierGetRes();
			try
			{
				var UserCompany_Id = _MongoContext.mUsers.AsQueryable().Where(x => x.VoyagerUser_Id == request.UserId).Select(y => y.Company_Id).FirstOrDefault();
				var AdminRoleId = _MongoContext.mRoles.AsQueryable().Where(x => x.LoweredRoleName == "administrator").Select(y => y.Voyager_Role_Id).FirstOrDefault();
				var AdminRole = _MongoContext.mUsersInRoles.AsQueryable().Where(x => x.UserId == request.UserId && x.RoleId == AdminRoleId).FirstOrDefault();

				FilterDefinition<mCompanies> filter;
				filter = Builders<mCompanies>.Filter.Empty;
				if (AdminRole == null)//means user is not an Admin
				{
					var CoreCompany_Id = _MongoContext.mSystem.AsQueryable().Select(y => y.CoreCompany_Id).FirstOrDefault();
					if (UserCompany_Id == CoreCompany_Id)
					{
						if (!string.IsNullOrWhiteSpace(CoreCompany_Id))
						{
							filter = filter & Builders<mCompanies>.Filter.Where(x => x.Company_Id != CoreCompany_Id);
						}
					}
					else
					{
						if (!string.IsNullOrWhiteSpace(UserCompany_Id))
						{
							filter = filter & Builders<mCompanies>.Filter.Where(x => x.ParentAgent_Id == UserCompany_Id);
						}
					}
				}

				if (!string.IsNullOrWhiteSpace(request.SupplierName))
				{
					filter = filter & Builders<mCompanies>.Filter.Where(x => x.Name.Trim().ToLower().Contains(request.SupplierName.Trim().ToLower()) && x.Issupplier == true);
				}
				if (!string.IsNullOrWhiteSpace(request.SupplierCode))
				{
					filter = filter & Builders<mCompanies>.Filter.Where(x => x.CompanyCode.ToLower().Contains(request.SupplierCode.Trim().ToLower()) && x.Issupplier == true);
				}
				if (request.CountryId.HasValue && !(request.CountryId.Value == Guid.Empty))
				{
					filter = filter & Builders<mCompanies>.Filter.Where(x => x.Country_Id == Convert.ToString(request.CountryId));
				}
				if (request.CityId.HasValue && !(request.CityId.Value == Guid.Empty))
				{
					filter = filter & Builders<mCompanies>.Filter.Where(x => x.City_Id == Convert.ToString(request.CityId));
				}
				if (!string.IsNullOrWhiteSpace(request.ProductType))
				{
					filter = filter & Builders<mCompanies>.Filter.Where(x => x.Products.Any(y => y.ProductType.Contains(request.ProductType)));
				}
				if (!string.IsNullOrWhiteSpace(request.Status) && request.Status.ToLower() == "inactive")
				{
					filter = filter & Builders<mCompanies>.Filter.Where(x => x.STATUS != "" && x.STATUS != " ");
				}
				else
				{
					filter = filter & Builders<mCompanies>.Filter.Where(x => x.STATUS == "" || x.STATUS == " ");
				}

				filter = filter & Builders<mCompanies>.Filter.Where(x => x.Issupplier == true);

				var result = await _MongoContext.mCompanies.Find(filter).Sort("{Name: 1}").Skip(request.Start).Limit(request.Length).Project(x => new SupplierList
				{
					CompanyId = x.Company_Id,
					CityId = x.City_Id,
					City = x.CityName,
					CountryId = x.Country_Id,
					Country = x.CountryName,
					Code = x.CompanyCode,
					Name = x.Name,
					Status = x.STATUS,
					IsSupplier = x.Issupplier

				}).ToListAsync();

				if (result.Count > 0)
				{
					response.SupplierTotalCount = Convert.ToInt32(_MongoContext.mCompanies.Find(filter).Count());
				}
				response.SupplierList = result;

				//response.SupplierList = result.OrderBy(x => x.Name).ToList();

				return response;
			}
			catch (Exception ex)
			{
				response.ResponseStatus.Status = "Failure";
			}
			return response;
		}

		public async Task<SupplierGetRes> GetNoOfBookingsForSupplier(SupplierGetReq request)
		{
			try
			{
				SupplierGetRes res = new SupplierGetRes();
				List<Bookings> response = new List<Bookings>();
				var prodidlist = request.bookingCount.Select(a => a.ProductId).ToList();

				response = _MongoContext.Bookings.AsQueryable().Where(x => x.Positions != null && x.Positions.Count() > 0
				&& x.Positions.Any(z => z.SupplierInfo != null && prodidlist.Contains(z.Product_Id))).ToList();

				var count = 0;
				foreach (var a in response)
				{
					var bookingres = request.bookingCount.Where(b => a.Positions.Select(c => c.Product_Id).ToList().Contains(b.ProductId) &&
					   a.Positions.Any(c => c.SupplierInfo.Id == b.SupplierId)).FirstOrDefault();

					if (bookingres != null)
					{
						count++;
						request.bookingCount.Where(b => b.SupplierId == bookingres.SupplierId && b.ProductId == bookingres.ProductId).FirstOrDefault().TotalCount = count;
					}
				}
				res.BookingCount = request.bookingCount;
				return res;
			}
			catch (Exception ex)
			{
				throw;
			}
		}

		public async Task<SupplierSetRes> SetSupplierProduct(SupplierSetReq request)
		{
			SupplierSetRes response = new SupplierSetRes();
			try
			{
				var company = _MongoContext.mCompanies.AsQueryable().Where(x => x.Company_Id == request.Company_Id).FirstOrDefault();
				if (company != null && request.Product != null)
				{
					var product = company.Products.Where(x => x.ProductSupplier_Id == request.ProductSupplier_Id).FirstOrDefault();
					if (product != null)
					{
						if (request.Product.SalesAgent != null && request.Product.SalesAgent.Count > 0 && request.IsAddSalesAgent)
						{
							ProductSupplierSalesAgent newAgent = new ProductSupplierSalesAgent();
							newAgent.ProductSupplierSalesAgent_Id = Guid.NewGuid().ToString();
							newAgent.Company_Id = request.Product.SalesAgent[0].Company_Id;
							newAgent.Company_Code = request.Product.SalesAgent[0].Company_Code;
							newAgent.Company_Name = request.Product.SalesAgent[0].Company_Name;
							product.EditUser = request.EditUser;
							product.EditDate = DateTime.Now;
							product.SalesAgent.Add(newAgent);
							response.SalesAgentId = newAgent.ProductSupplierSalesAgent_Id;
						}
						else if (request.IsRemoveSalesAgent && !string.IsNullOrEmpty(request.ProductSupplierSalesAgent_Id))
						{
							product.SalesAgent.RemoveAll(x => x.ProductSupplierSalesAgent_Id == request.ProductSupplierSalesAgent_Id);
							product.EditUser = request.EditUser;
							product.EditDate = DateTime.Now;
							response.SalesAgentId = request.ProductSupplierSalesAgent_Id;
						}
						else if (request.IsProduct)
						{
							string salesEmail = string.Empty, salesName = string.Empty, fitEmail = string.Empty, fitName = string.Empty, groupEmail = string.Empty, groupName = string.Empty, financeEmail = string.Empty, financeName = string.Empty, emergencyEmail = string.Empty, emergencyName = string.Empty, complaintEmail = string.Empty, complaintName = string.Empty;

							var currencyname = _MongoContext.mCurrency.AsQueryable().Where(x => x.VoyagerCurrency_Id == request.Product.CurrencyId).Select(x => x.Name).FirstOrDefault();
							var contactdetails = company.ContactDetails.ToList();
							if (contactdetails != null && contactdetails.Count > 0)
							{
								salesName = contactdetails.Where(x => x.Contact_Id == request.Product.Contact_Sales_Id).Select(x => x.FIRSTNAME + " " + x.LastNAME).FirstOrDefault();
								salesEmail = contactdetails.Where(x => x.Contact_Id == request.Product.Contact_Sales_Id).Select(x => x.MAIL).FirstOrDefault();
								fitName = contactdetails.Where(x => x.Contact_Id == request.Product.Contact_FIT_Id).Select(x => x.FIRSTNAME + " " + x.LastNAME).FirstOrDefault();
								fitEmail = contactdetails.Where(x => x.Contact_Id == request.Product.Contact_FIT_Id).Select(x => x.MAIL).FirstOrDefault();
								groupName = contactdetails.Where(x => x.Contact_Id == request.Product.Contact_Group_Id).Select(x => x.FIRSTNAME + " " + x.LastNAME).FirstOrDefault();
								groupEmail = contactdetails.Where(x => x.Contact_Id == request.Product.Contact_Group_Id).Select(x => x.MAIL).FirstOrDefault();
								financeName = contactdetails.Where(x => x.Contact_Id == request.Product.Contact_Finance_Id).Select(x => x.FIRSTNAME + " " + x.LastNAME).FirstOrDefault();
								financeEmail = contactdetails.Where(x => x.Contact_Id == request.Product.Contact_Finance_Id).Select(x => x.MAIL).FirstOrDefault();
								emergencyName = contactdetails.Where(x => x.Contact_Id == request.Product.Contact_Emergency_Id).Select(x => x.FIRSTNAME + " " + x.LastNAME).FirstOrDefault();
								emergencyEmail = contactdetails.Where(x => x.Contact_Id == request.Product.Contact_Emergency_Id).Select(x => x.MAIL).FirstOrDefault();
								complaintName = contactdetails.Where(x => x.Contact_Id == request.Product.Contact_Complaints_Id).Select(x => x.FIRSTNAME + " " + x.LastNAME).FirstOrDefault();
								complaintEmail = contactdetails.Where(x => x.Contact_Id == request.Product.Contact_Complaints_Id).Select(x => x.MAIL).FirstOrDefault();
							}

							product.SupplierStatus = request.Product.SupplierStatus;
							product.CurrencyId = request.Product.CurrencyId;
							product.CurrencyName = currencyname;
							product.ActiveFrom = request.Product.ActiveFrom;
							product.ActiveTo = request.Product.ActiveTo;
							product.IsPreferred = request.Product.IsPreferred;
							product.IsDefault = request.Product.IsDefault;
							product.Contact_Sales_Id = request.Product.Contact_Sales_Id;
							product.Contact_Sales_Name = salesName;
							product.Contact_Sales_Email = salesEmail;
							product.Contact_FIT_Id = request.Product.Contact_FIT_Id;
							product.Contact_FIT_Name = fitName;
							product.Contact_FIT_Email = fitEmail;
							product.Contact_Group_Id = request.Product.Contact_Group_Id;
							product.Contact_Group_Name = groupName;
							product.Contact_Group_Email = groupEmail;
							product.Contact_Finance_Id = request.Product.Contact_Finance_Id;
							product.Contact_Finance_Name = financeName;
							product.Contact_Finance_Email = financeEmail;
							product.Contact_Emergency_Id = request.Product.Contact_Emergency_Id;
							product.Contact_Emergency_Name = emergencyName;
							product.Contact_Emergency_Email = emergencyEmail;
							product.Contact_Complaints_Id = request.Product.Contact_Complaints_Id;
							product.Contact_Complaints_Name = complaintName;
							product.Contact_Complaints_Email = complaintEmail;
							product.EditUser = request.EditUser;
							product.EditDate = DateTime.Now;

							if (request.Product.IsDefault == false)
							{
								product.OperatingMarket.RemoveAll(x => x.ProductSupplierOperatingMkt_Id != null);
								foreach (var a in request.Product.OperatingMarket)
								{
									ProductSupplierOperatingMarket op = new ProductSupplierOperatingMarket();
									op.ProductSupplierOperatingMkt_Id = Guid.NewGuid().ToString();
									op.BusinessRegion_Id = a.BusinessRegion_Id;
									op.BusinessRegion = a.BusinessRegion;
									product.OperatingMarket.Add(op);
								}

								product.SalesMarket.RemoveAll(x => x.ProductSupplierSalesMkt_Id != null);
								foreach (var a in request.Product.SalesMarket)
								{
									ProductSupplierSalesMarket sm = new ProductSupplierSalesMarket();
									sm.ProductSupplierSalesMkt_Id = Guid.NewGuid().ToString();
									sm.BusinessRegion_Id = a.BusinessRegion_Id;
									sm.BusinessRegion = a.BusinessRegion;
									product.SalesMarket.Add(sm);
								}
							}
							else
							{
								product.OperatingMarket = new List<ProductSupplierOperatingMarket>();
								product.SalesMarket = new List<ProductSupplierSalesMarket>();
								product.SalesAgent = new List<ProductSupplierSalesAgent>();
							}
						}
					}

					await _MongoContext.mCompanies.UpdateOneAsync(Builders<mCompanies>.Filter.Eq("Company_Id", company.Company_Id),
						Builders<mCompanies>.Update.Set("Products", company.Products)
						.Set("EditUser", request.EditUser).Set("EditDate", DateTime.Now));
				}
				else
				{
					response.ResponseStatus.Status = "Failure";
					response.ResponseStatus.ErrorMessage = "No company found";
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

		public async Task<SupplierGetRes> EnableDisableSupplierProduct(SupplierGetReq request)
		{
			SupplierGetRes response = new SupplierGetRes();
			try
			{
				var supplier = _MongoContext.mCompanies.AsQueryable().Where(x => x.Company_Id == request.CompanyId).FirstOrDefault();
				foreach (var p in supplier.Products)
				{
					if (p.ProductSupplier_Id == request.ProductSupplierId)
					{
						p.SupplierStatus = request.Status;
						p.EditUser = request.EditUser;
						p.EditDate = DateTime.Now;
					}
				}

				await _MongoContext.mCompanies.UpdateOneAsync(Builders<mCompanies>.Filter.Eq("Company_Id", supplier.Company_Id),
						   Builders<mCompanies>.Update.Set("Products", supplier.Products)
						   .Set("EditUser", request.EditUser).Set("EditDate", DateTime.Now));

				response.ResponseStatus.Status = "Success";
				response.ResponseStatus.StatusMessage = "Details updated successfully.";
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

		public async Task<TaxRegestrationDetails_Res> GetTaxRegestrationDetails(TaxRegestrationDetails_Req request)
		{
			TaxRegestrationDetails_Res response = new TaxRegestrationDetails_Res();
			List<TaxRegestrationDetails> lstTaxRegDetails = new List<TaxRegestrationDetails>();
			ResponseStatus responseStatus = new ResponseStatus();
			try
			{
				if (!String.IsNullOrEmpty(request.Company_Id))
				{
					var company = _MongoContext.mCompanies.AsQueryable().Where(x => x.Company_Id == request.Company_Id).FirstOrDefault();

					if (company != null)
					{
						if (String.IsNullOrEmpty(request.TaxReg_Id))
						{
							lstTaxRegDetails = company.TaxRegestrationDetails.ToList();
						}
						else
						{
							lstTaxRegDetails = company.TaxRegestrationDetails.Where(x => x.TaxReg_Id == request.TaxReg_Id).ToList();
						}
						if (lstTaxRegDetails != null && lstTaxRegDetails.Any() && lstTaxRegDetails.Count > 0)
						{
							response.TaxRegestrationDetails = lstTaxRegDetails;
							response.ResponseStatus.StatusMessage = "Success";
						}
						else
						{
							response.ResponseStatus.Status = "error";
							response.ResponseStatus.StatusMessage = "No Data Found";
						}
					}
					else
					{
						response.ResponseStatus.Status = "error";
						response.ResponseStatus.StatusMessage = "Invalid Data";
					}
				}
				else
				{
					response.ResponseStatus.Status = "error";
					response.ResponseStatus.StatusMessage = "No CompanyId found";
				}
			}
			catch (Exception e)
			{
				response.ResponseStatus.Status = "Failure";
			}
			return response;
		}

		#region Get Product Supplier Sales/Operating Market list

		public List<ProductSupplierSalesMarket> GetProductSupplierSalesMkt()
		{
			try
			{
				var result = (from p in _MongoContext.mProductSupplierSalesMkt.AsQueryable()
							  join b in _MongoContext.mBusinessRegions.AsQueryable()
							  on p.BusinessRegion_Id equals b.BusinessRegion_Id
							  select new ProductSupplierSalesMarket
							  {
								  ProductSupplierSalesMkt_Id = p.ProductSupplierSalesMkt_Id,
								  BusinessRegion_Id = p.BusinessRegion_Id,
								  BusinessRegion = b.BusinessRegion,
							  }).Distinct().ToList();

				return result;
			}
			catch (Exception ex)
			{
				throw;
			}
		}

		public List<ProductSupplierOperatingMarket> GetProductSupplierOperatingMkt()
		{
			var result = (from p in _MongoContext.mProductSupplierOperatingMkt.AsQueryable()
						  join b in _MongoContext.mBusinessRegions.AsQueryable()
						  on p.BusinessRegion_Id equals b.BusinessRegion_Id
						  select new ProductSupplierOperatingMarket
						  {
							  ProductSupplierOperatingMkt_Id = p.ProductSupplierOperatingMkt_Id,
							  BusinessRegion_Id = p.BusinessRegion_Id,
							  BusinessRegion = b.BusinessRegion,
						  }).Distinct().ToList();

			return result;
		}

		public List<mBusinessRegions> GetBusinessRegions()
		{
			var list = _MongoContext.mBusinessRegions.AsQueryable().Distinct().ToList();
			list = list.OrderBy(x => x.BusinessRegion).ToList();
			return list;
		}

		#endregion

		#region Mappings

		public List<mApplications> GetApplications()
		{
			try
			{
				return _MongoContext.mApplications.AsQueryable().Distinct().OrderBy(x => x.Application_Name).ToList();
			}
			catch (Exception ex)
			{
				throw;
			}
		}

		public List<Mappings> GetSupplierMappings(SupplierGetReq request)
		{
			List<Mappings> mappings = new List<Mappings>();
			try
			{
				if (!string.IsNullOrWhiteSpace(request.Id))
				{
					if (!string.IsNullOrWhiteSpace(request.PageName) && request.PageName.ToLower() == "product")
						mappings = _MongoContext.Products.AsQueryable().Where(x => x.VoyagerProduct_Id == request.Id).FirstOrDefault()?.Mappings?.OrderBy(x => x.Application).ToList();
					else
						mappings = _MongoContext.mCompanies.AsQueryable().Where(x => x.Company_Id == request.Id).FirstOrDefault()?.Mappings?.OrderBy(x => x.Application).ToList();
				}
			}
			catch (Exception ex)
			{
				throw;
			}
			return mappings;
		}

		public async Task<SupplierSetRes> SetSupplierMapping(SupplierSetReq request)
		{
			SupplierSetRes response = new SupplierSetRes();
			List<Mappings> lstMappings = new List<Mappings>();
			try
			{
				Products product = new Products();
				mCompanies company = new mCompanies();

				if (!string.IsNullOrWhiteSpace(request.Id))
				{
					if (!string.IsNullOrWhiteSpace(request.PageName) && request.PageName.ToLower() == "product")
						product = await _MongoContext.Products.FindAsync(x => x.VoyagerProduct_Id == request.Id).Result.FirstOrDefaultAsync();
					else
						company = await _MongoContext.mCompanies.FindAsync(x => x.Company_Id == request.Id).Result.FirstOrDefaultAsync();

					if (product != null || company != null)
					{
						request.SupplierMappings.RemoveAll(x => string.IsNullOrWhiteSpace(x.Application_Id) && string.IsNullOrWhiteSpace(x.PartnerEntityCode) && string.IsNullOrWhiteSpace(x.PartnerEntityName));

						foreach (var val in request.SupplierMappings)
						{
                            var foundMapped = company != null && !string.IsNullOrEmpty(company.Company_Id) && company.Mappings != null && company.Mappings.Any() ? company.Mappings.Where(a => a.PartnerEntityCode == val.PartnerEntityCode && a.PartnerEntityType.ToLower() == "ACCOUNT".ToLower() && string.IsNullOrEmpty(a.Status)).FirstOrDefault(): null;
                            if (foundMapped == null)
                            {
                                Mappings mobj = new Mappings()
                                {
                                    Application_Id = val.Application_Id,
                                    Application = val.Application,
                                    PartnerEntityCode = val.PartnerEntityCode,
                                    PartnerEntityName = val.PartnerEntityName,
                                    PartnerEntityType = string.Empty,
                                    Action = string.Empty,
                                    Status = string.Empty,
                                    AdditionalInfoType = string.Empty,
                                    AdditionalInfo = string.Empty,
                                    CreateDate = val.CreateDate,
                                    CreateUser = val.CreateUser
                                };
                                lstMappings.Add(mobj);
                            }
                            else
                            {
                                foundMapped.EditDate = DateTime.Now;
                                foundMapped.EditUser = val.CreateUser;
                                lstMappings.Add(foundMapped);
                            }
                            
						}

						if (product != null && !string.IsNullOrWhiteSpace(product.VoyagerProduct_Id))
						{
							product.Mappings = lstMappings.Distinct().ToList();
							await _MongoContext.Products.UpdateOneAsync(Builders<Products>.Filter.Eq("VoyagerProduct_Id", request.Id), Builders<Products>.Update.Set("Mappings", product.Mappings));
							response.ResponseStatus.Status = "Success";
							response.ResponseStatus.StatusMessage = "Record Saved Successfully.";
						}
						else if (company != null && !string.IsNullOrWhiteSpace(company.Company_Id))
						{
							company.Mappings = lstMappings.Distinct().ToList();
							await _MongoContext.mCompanies.UpdateOneAsync(Builders<mCompanies>.Filter.Eq("Company_Id", request.Id), Builders<mCompanies>.Update.Set("Mappings", company.Mappings));
							response.ResponseStatus.Status = "Success";
							response.ResponseStatus.StatusMessage = "Record Saved Successfully.";
						}
						else
						{
							response.ResponseStatus.Status = "Failure";
							response.ResponseStatus.ErrorMessage = "Details not found";
						}						
					}
					else
					{
						response.ResponseStatus.Status = "Failure";
						response.ResponseStatus.ErrorMessage = "Details not found";
					}
				}
				else
				{
					response.ResponseStatus.Status = "Failure";
					response.ResponseStatus.ErrorMessage = "Id not found";
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
	}
}
