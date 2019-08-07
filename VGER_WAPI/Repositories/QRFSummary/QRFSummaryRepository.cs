using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VGER_WAPI.Helpers;
using VGER_WAPI.Models;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Repositories
{
	public class QRFSummaryRepository : IQRFSummaryRepository
	{
		#region Private Variable Declaration
		private readonly MongoContext _MongoContext = null;
		private readonly IQuoteRepository _quoteRepository;
		private readonly IPositionRepository _positionRepository;
		private readonly IGenericRepository _genericRepository;
		private readonly IHostingEnvironment _env;
		private readonly IConfiguration _configuration;
		private readonly IEmailRepository _emailRepository;
		private readonly IMasterRepository _masterRepository;
		#endregion

		public QRFSummaryRepository(IConfiguration configuration, IOptions<MongoSettings> settings, IQuoteRepository quoteRepository,
			IPositionRepository positionRepository, IGenericRepository genericRepository, IEmailRepository emailRepository, IMasterRepository masterRepository, IHostingEnvironment env)
		{
			_MongoContext = new MongoContext(settings);
			_quoteRepository = quoteRepository;
			_positionRepository = positionRepository;
			_genericRepository = genericRepository;
			_emailRepository = emailRepository;
			_masterRepository = masterRepository;
			_env = env;
			_configuration = configuration;
		}

		public async Task<QRFSummaryGetRes> GetQRFSummary(QRFSummaryGetReq request)
		{
			var response = new QRFSummaryGetRes();
			List<SummaryDetailsInfo> summary = new List<SummaryDetailsInfo>();
			List<Attributes> DaysList = new List<Attributes>();
			List<OriginalItineraryDetailsInfo> originalDetails = new List<OriginalItineraryDetailsInfo>();
			string ProductRangeName = "";
			mProductRange productRange;

			RoutingDaysGetReq getRequest = new RoutingDaysGetReq();
			getRequest.QRFID = request.QRFID;

			if (request.IsCosting)
			{
				List<ProductDescription> prodDesc = new List<ProductDescription>();
				string desc = "";
				var resultQuote = _MongoContext.mQRFPrice.AsQueryable().Where(q => q.QRFID == request.QRFID && q.IsCurrentVersion == true).FirstOrDefault();
				var date = resultQuote.Departures.Where(y => y.IsDeleted == false).Select(x => x.Date).Min();
				int quotePaxAdultCount = resultQuote.AgentPassengerInfo.Where(x => x.Type == "ADULT").Select(b => b.count).FirstOrDefault();
				if (resultQuote != null)
				{
					RoutingDaysGetRes routingDays = await _quoteRepository.GetQRFRoutingDays(getRequest);
					//var resultPosition = _MongoContext.mQRFPosition.AsQueryable().Where(q => q.QRFID == request.QRFID && q.ProductType.ToLower() != "assistant").ToList();
					var resultPosition = _MongoContext.mQRFPosition.AsQueryable().Where(q => q.QRFID == request.QRFID && q.IsTourEntity == false).ToList();
					List<string> listRangeId = new List<string>();
					resultPosition.ForEach(a => listRangeId.AddRange(a.RoomDetailsInfo.Select(b => b.ProductRangeId).ToList()));
					var ProdRangeList = _MongoContext.mProductRange.AsQueryable().Where(a => listRangeId.Contains(a.VoyagerProductRange_Id));

					int counter = 1;
					DateTime startDate = Convert.ToDateTime(date);
					var totalDays = routingDays.RoutingDays.Count();
					var mealDays = resultQuote.Meals?.MealDays;
					mealDays = mealDays == null ? new List<MealDays>() : mealDays;

					#region for Accomo Positions
					int day = 0;
					int prevnight = 0;
					List<RoutingInfoCity> lstRoutingInfoCity = new List<RoutingInfoCity>();
					var hotelPos = resultPosition.Where(a => a.ProductType.ToLower() == "hotel").ToList();
					var hotelpos = new mQRFPosition();
					var hotelposid = "";
					foreach (var Route in resultQuote.RoutingInfo.Where(a => a.Nights > 0))
					{
						if (day == 0) { day = 1; }
						else { day = prevnight + day; }

						hotelposid = "";
						hotelpos = hotelPos.Where(a => a.DayNo == day).FirstOrDefault();
						if (hotelpos != null)
						{
							hotelposid = hotelpos.PositionId;
						}

						RoutingInfoCity city = new RoutingInfoCity();
						var rday1 = routingDays.RoutingDays.Where(a => a.DayNo == day && a.IsDeleted == false).FirstOrDefault();
						if (rday1 != null)
							city.DayId = routingDays.RoutingDays.Where(a => a.DayNo == day && a.IsDeleted == false).FirstOrDefault().RoutingDaysID;
						else
							city.DayId = "";
						city.DayNo = day;
						city.DayName = "Day " + day.ToString();
						city.Duration = Route.Nights;
						city.CityID = Route.ToCityID;
						city.CityName = Route.ToCityName;
						city.PositionId = hotelposid;
						lstRoutingInfoCity.Add(city);
						prevnight = Route.Nights;

						//lstRoutingInfoCity.Add(new RoutingInfoCity
						//{
						//    DayId = routingDays.RoutingDays.Where(a => a.DayNo == day && a.IsDeleted == false).FirstOrDefault().RoutingDaysID,
						//    DayNo = day,
						//    DayName = "Day " + day.ToString(),
						//    Duration = Route.Nights,
						//    CityID = Route.ToCityID,
						//    CityName = Route.ToCityName,
						//    PositionId = hotelposid
						//});
						//prevnight = Route.Nights;
					}
					response.RoutingInfoCity = lstRoutingInfoCity;
					#endregion

					foreach (var item in routingDays.RoutingDays)
					{
						if (!item.IsDeleted)
						{
							List<Meal> meal = new List<Meal>();

							foreach (var mday in mealDays)
							{
								if (mday.DayName == item.Days)
								{
									foreach (var info in mday.MealDayInfo) //foreach (var info in mday.MealDayInfo.Where(x => string.IsNullOrEmpty(x.PositionID)))
									{
										Meal m = new Meal();
										m.MealType = info.MealType;
										m.MealTime = info.StartTime;
										m.PositionId = info.PositionID;
										m.ProductID = info.ProductID;
										m.Address = info.Address;
										m.FullAddress = info.FullAddress;
										m.Telephone = info.Telephone;
										m.Mail = info.Mail;
										m.IsDeleted = info.IsDeleted;
										meal.Add(m);
									}
								}
							}
							var result = resultPosition.Where(x => x.RoutingDaysID == item.RoutingDaysID).OrderBy(y => y.StartTime).ToList();
							var productType = _MongoContext.mProductType.AsQueryable().ToList();
							var ProdIdList = result.Select(y => y.ProductID).ToList();
							var products = _MongoContext.Products.AsQueryable().Where(x => ProdIdList.Contains(x.VoyagerProduct_Id)).Select(y => new { y.VoyagerProduct_Id, y.ProductDescription });

							if (counter != 1)
							{
								date = startDate.AddDays(1);
								startDate = Convert.ToDateTime(date);
							}

							var objSummary = new SummaryDetailsInfo();
							objSummary.Day = item.Days;
							objSummary.OriginalItineraryDate = date;
							objSummary.OriginalItineraryDay = date.Value.DayOfWeek.ToString();
							objSummary.PlaceOfService = item.GridLabel;

							//if (counter == totalDays)
							//	objSummary.PlaceOfService = !string.IsNullOrEmpty(item.GridLabel) && item.GridLabel.Contains(',') ? item.GridLabel.Split(',').Last() : item.GridLabel;
							//else
							//	objSummary.PlaceOfService = !string.IsNullOrEmpty(item.GridLabel) && item.GridLabel.Contains(',') ? item.GridLabel.Split(',')[0] : "";

							//objSummary.PlaceOfService = !string.IsNullOrEmpty(item.FromCityName) && item.FromCityName.Contains(',') ? item.FromCityName.Split(',')[0] : "";
							//objSummary.CountryName = !string.IsNullOrEmpty(item.FromCityName) && item.FromCityName.Contains(',') ? item.FromCityName.Split(',')[1] : "";
							objSummary.ToCityName = !string.IsNullOrEmpty(item.ToCityName) && item.ToCityName.Contains(',') ? item.ToCityName.Split(',')[0] : "";
							//objSummary.ToCountryName = !string.IsNullOrEmpty(item.ToCityName) && item.ToCityName.Contains(',') ? item.ToCityName.Split(',')[1] : "";
							objSummary.IncludedMeals = meal;
							objSummary.RoutingCityIds = item.GridLabelIds;

							if (counter == 1)
							{
								objSummary.OriginalItineraryName = "Arrive at " + item.GridLabel;
							}
							else if (counter == totalDays)
							{
								var cityname = !string.IsNullOrEmpty(item.GridLabel) && item.GridLabel.Contains(',') ? item.GridLabel.Split(',').Last() : item.GridLabel;
								objSummary.OriginalItineraryName = "Depart from " + cityname;
							}
							else
							{
								objSummary.OriginalItineraryName = item.GridLabel;
							}

							foreach (var position in result)
							{
								desc = "";
								prodDesc = (products != null && !string.IsNullOrEmpty(position.ProductID)) ? products.Where(x => x.VoyagerProduct_Id == position.ProductID).FirstOrDefault()?.ProductDescription : null; //.Select(y => y.ProductDescription.Where(z => z.DescType == "Description").Select(c => c.Description).FirstOrDefault()).FirstOrDefault() : "";
								if (prodDesc != null && prodDesc.Count > 0)
									desc = prodDesc.Where(x => x.Description?.ToLower() == "description").Select(y => y.Description).FirstOrDefault();
								var obj = new OriginalItineraryDetailsInfo();
								obj.PositionId = position.PositionId;
								obj.TLRemarks = position.TLRemarks;
								obj.OPSRemarks = position.OPSRemarks;
								obj.Supplier = position.SupplierName;
								obj.SupplierId = position.SupplierId;
								obj.ProductId = position.ProductID;
								obj.NumberOfPax = quotePaxAdultCount;
								obj.OriginalItineraryDescription = position.ProductName;
								obj.ProductType = position.ProductType;
								obj.IsDeleted = position.IsDeleted;
								obj.KeepAs = position.KeepAs;
								obj.StartTime = position.StartTime;
								obj.EndTime = position.EndTime;
								obj.ProductCategoryId = position.BudgetCategoryId;
								obj.ProductCategory = position.BudgetCategory;
								obj.ProductTypeChargeBasis = productType.Where(x => x.VoyagerProductType_Id == position.ProductTypeId).Select(y => y.ChargeBasis).FirstOrDefault();
								obj.BuyCurrency = position.BuyCurrency;
								obj.MealType = position.MealType;
								obj.CityName = position.CityName;
								obj.CountryName = position.CountryName;
								obj.Stars = position.StarRating;
								obj.ProductId = position.ProductID;
								obj.Duration = position.Duration;
								obj.ProductDescription = desc != null ? desc : ""; //products != null ? products.Where(x => x.VoyagerProduct_Id == position.ProductID).Select(y => y.ProductDescription.Where(z => z.DescType == "Description").Select(c => c.Description).FirstOrDefault()).FirstOrDefault() : "";
								obj.StandardPrice = position.StandardPrice;

								foreach (var room in position.RoomDetailsInfo)
								{
									productRange = ProdRangeList.Where(a => a.VoyagerProductRange_Id == room.ProductRangeId).FirstOrDefault();
									if (productRange != null) ProductRangeName = productRange.ProductRangeName;
									if (productRange != null && !string.IsNullOrWhiteSpace(productRange.ProductMenu)) ProductRangeName = ProductRangeName + " - " + productRange.ProductMenu;
									obj.RoomDetails.Add(new RoomInfo
									{
										ProductRangeId = room.ProductRangeId,
										ProductRange = room.ProductRange,
										ProdDesc = room.ProdDesc,
										RangeDesc = ProductRangeName
									});
								}

								objSummary.OriginalItineraryDetails.Add(obj);
							}
							summary.Add(objSummary);
							counter++;
						}
						response.ResponseStatus.Status = "Success";
						response.SummaryDetailsInfo = summary.ToList();
						response.QRFID = resultQuote.QRFID;
					}
				}
				else
				{
					response.ResponseStatus.Status = "Failure";
					response.ResponseStatus.ErrorMessage = "QRFID is not exists.";
				}
				return response;
			}
			else
			{
				List<ProductDescription> prodDesc = new List<ProductDescription>();
				string desc = "";
				var resultQuote = _MongoContext.mQuote.AsQueryable().Where(q => q.QRFID == request.QRFID).FirstOrDefault();
				var date = resultQuote.Departures.Where(y => y.IsDeleted == false).Select(x => x.Date).Min();
				int quotePaxAdultCount = resultQuote.AgentPassengerInfo.Where(x => x.Type == "ADULT").Select(b => b.count).FirstOrDefault();
				if (resultQuote != null)
				{
					RoutingDaysGetRes routingDays = await _quoteRepository.GetQRFRoutingDays(getRequest);
					//var resultPosition = _MongoContext.mPosition.AsQueryable().Where(q => q.QRFID == request.QRFID && q.ProductType.ToLower() != "assistant").ToList();
					var resultPosition = _MongoContext.mPosition.AsQueryable().Where(q => q.QRFID == request.QRFID && q.IsTourEntity == false).ToList();

					List<string> listRangeId = new List<string>();
					resultPosition.ForEach(a => listRangeId.AddRange(a.RoomDetailsInfo.Select(b => b.ProductRangeId).ToList()));
					var ProdRangeList = _MongoContext.mProductRange.AsQueryable().Where(a => listRangeId.Contains(a.VoyagerProductRange_Id)).ToList();

					int counter = 1;
					DateTime startDate = Convert.ToDateTime(date);
					var totalDays = routingDays.RoutingDays.Count();
					var mealDays = resultQuote.Meals?.MealDays;
					mealDays = mealDays == null ? new List<MealDays>() : mealDays;

					#region for Accomo Positions
					int day = 0;
					int prevnight = 0;
					List<RoutingInfoCity> lstRoutingInfoCity = new List<RoutingInfoCity>();
					var hotelPos = resultPosition.Where(a => a.ProductType.ToLower() == "hotel").ToList();
					var hotelpos = new mPosition();
					var hotelposid = "";
					foreach (var Route in resultQuote.RoutingInfo.Where(a => a.Nights > 0))
					{
						if (day == 0) { day = 1; }
						else { day = prevnight + day; }

						hotelposid = "";
						hotelpos = hotelPos.Where(a => a.DayNo == day).FirstOrDefault();
						if (hotelpos != null)
						{
							hotelposid = hotelpos.PositionId;
						}

						RoutingInfoCity city = new RoutingInfoCity();
						var rday1 = routingDays.RoutingDays.Where(a => a.DayNo == day && a.IsDeleted == false).FirstOrDefault();
						if (rday1 != null)
							city.DayId = routingDays.RoutingDays.Where(a => a.DayNo == day && a.IsDeleted == false).FirstOrDefault().RoutingDaysID;
						else
							city.DayId = "";
						city.DayNo = day;
						city.DayName = "Day " + day.ToString();
						city.Duration = Route.Nights;
						city.CityID = Route.ToCityID;
						city.CityName = Route.ToCityName;
						city.PositionId = hotelposid;
						lstRoutingInfoCity.Add(city);
						prevnight = Route.Nights;

						//lstRoutingInfoCity.Add(new RoutingInfoCity
						//{
						//    DayId = routingDays.RoutingDays.Where(a => a.DayNo == day && a.IsDeleted == false).FirstOrDefault().RoutingDaysID,
						//    DayNo = day,
						//    DayName = "Day " + day.ToString(),
						//    Duration = Route.Nights,
						//    CityID = Route.ToCityID,
						//    CityName = Route.ToCityName,
						//    PositionId = hotelposid
						//});
						//prevnight = Route.Nights;
					}
					response.RoutingInfoCity = lstRoutingInfoCity;
					#endregion

					foreach (var item in routingDays.RoutingDays)
					{
						if (!item.IsDeleted)
						{
							List<Meal> meal = new List<Meal>();

							foreach (var mday in mealDays)
							{
								if (mday.DayName == item.Days)
								{
									foreach (var info in mday.MealDayInfo) //foreach (var info in mday.MealDayInfo.Where(x => string.IsNullOrEmpty(x.PositionID)))
									{
										Meal m = new Meal();
										m.MealType = info.MealType;
										m.MealTime = info.StartTime;
										m.PositionId = info.PositionID;
										m.ProductID = info.ProductID;
										m.Address = info.Address;
										m.FullAddress = info.FullAddress;
										m.Telephone = info.Telephone;
										m.Mail = info.Mail;
										m.IsDeleted = info.IsDeleted;
										meal.Add(m);
									}
								}
							}
							var result = resultPosition.Where(x => x.RoutingDaysID == item.RoutingDaysID).OrderBy(y => y.StartTime).ToList();
							var productType = _MongoContext.mProductType.AsQueryable().ToList();
							var ProdIdList = result.Select(y => y.ProductID).ToList();
							var products = _MongoContext.Products.AsQueryable().Where(x => ProdIdList.Contains(x.VoyagerProduct_Id)).Select(y => new { y.VoyagerProduct_Id, y.ProductDescription });

							if (counter != 1)
							{
								date = startDate.AddDays(1);
								startDate = Convert.ToDateTime(date);
							}

							var objSummary = new SummaryDetailsInfo();
							objSummary.Day = item.Days;
							objSummary.OriginalItineraryDate = date;
							objSummary.OriginalItineraryDay = date.Value.DayOfWeek.ToString();
							objSummary.PlaceOfService = item.GridLabel;

							//if (counter == totalDays)
							//	objSummary.PlaceOfService = !string.IsNullOrEmpty(item.GridLabel) && item.GridLabel.Contains(',') ? item.GridLabel.Split(',').Last() : item.GridLabel;
							//else
							//	objSummary.PlaceOfService = !string.IsNullOrEmpty(item.GridLabel) && item.GridLabel.Contains(',') ? item.GridLabel.Split(',')[0] : item.GridLabel;

							//objSummary.PlaceOfService = !string.IsNullOrEmpty(item.FromCityName) && item.FromCityName.Contains(',') ? item.FromCityName.Split(',')[0] : "";
							//objSummary.CountryName = !string.IsNullOrEmpty(item.FromCityName) && item.FromCityName.Contains(',') ? item.FromCityName.Split(',')[1] : "";
							objSummary.ToCityName = !string.IsNullOrEmpty(item.ToCityName) && item.ToCityName.Contains(',') ? item.ToCityName.Split(',')[0] : "";
							//objSummary.ToCountryName = !string.IsNullOrEmpty(item.ToCityName) && item.ToCityName.Contains(',') ? item.ToCityName.Split(',')[1] : "";
							objSummary.IncludedMeals = meal;
							objSummary.RoutingCityIds = item.GridLabelIds;

							if (counter == 1)
							{
								objSummary.OriginalItineraryName = "Arrive at " + item.GridLabel;
							}
							else if (counter == totalDays)
							{
								var cityname = !string.IsNullOrEmpty(item.GridLabel) && item.GridLabel.Contains(',') ? item.GridLabel.Split(',').Last() : item.GridLabel;
								objSummary.OriginalItineraryName = "Depart from " + cityname;
							}
							else
							{
								objSummary.OriginalItineraryName = item.GridLabel;
							}

							foreach (var position in result)
							{
								desc = "";
								prodDesc = (products != null && !string.IsNullOrEmpty(position.ProductID)) ? products.Where(x => x.VoyagerProduct_Id == position.ProductID).FirstOrDefault()?.ProductDescription : null; //.Select(y => y.ProductDescription.Where(z => z.DescType == "Description").Select(c => c.Description).FirstOrDefault()).FirstOrDefault() : "";
								if (prodDesc != null && prodDesc.Count > 0)
									desc = prodDesc.Where(x => x.Description?.ToLower() == "description").Select(y => y.Description).FirstOrDefault();
								var obj = new OriginalItineraryDetailsInfo();
								obj.PositionId = position.PositionId;
								obj.TLRemarks = position.TLRemarks;
								obj.OPSRemarks = position.OPSRemarks;
								obj.Supplier = position.SupplierName;
								obj.SupplierId = position.SupplierId;
								obj.ProductId = position.ProductID;
								obj.NumberOfPax = quotePaxAdultCount;
								obj.OriginalItineraryDescription = position.ProductName;
								obj.ProductType = position.ProductType;
								obj.IsDeleted = position.IsDeleted;
								obj.KeepAs = position.KeepAs;
								obj.StartTime = position.StartTime;
								obj.EndTime = position.EndTime;
								obj.ProductCategoryId = position.BudgetCategoryId;
								obj.ProductCategory = position.BudgetCategory;
								obj.ProductTypeChargeBasis = productType.Where(x => x.VoyagerProductType_Id == position.ProductTypeId).Select(y => y.ChargeBasis).FirstOrDefault();
								obj.BuyCurrency = position.BuyCurrency;
								obj.MealType = position.MealType;
								obj.CityName = position.CityName;
								obj.CountryName = position.CountryName;
								obj.Stars = position.StarRating;
								obj.ProductId = position.ProductID;
								obj.Duration = position.Duration;
								obj.ProductDescription = desc != null ? desc : ""; //products != null ? products.Where(x => x.VoyagerProduct_Id == position.ProductID).Select(y => y.ProductDescription.Where(z => z.DescType == "Description").Select(c => c.Description).FirstOrDefault()).FirstOrDefault() : "";
								obj.StandardPrice = position.StandardPrice;

								foreach (var room in position.RoomDetailsInfo)
								{
									productRange = ProdRangeList.Where(a => a.VoyagerProductRange_Id == room.ProductRangeId).FirstOrDefault();
									if (productRange != null) ProductRangeName = productRange.ProductRangeName;
									if (productRange != null && !string.IsNullOrWhiteSpace(productRange.ProductMenu)) ProductRangeName = ProductRangeName + " - " + productRange.ProductMenu;
									obj.RoomDetails.Add(new RoomInfo
									{
										ProductRangeId = room.ProductRangeId,
										ProductRange = room.ProductRange,
										ProdDesc = room.ProdDesc,
										RangeDesc = ProductRangeName
									});
								}

								objSummary.OriginalItineraryDetails.Add(obj);
							}
							summary.Add(objSummary);
							counter++;
						}
						response.ResponseStatus.Status = "Success";
						response.SummaryDetailsInfo = summary.ToList();
						response.QRFID = resultQuote.QRFID;
					}
				}
				else
				{
					response.ResponseStatus.Status = "Failure";
					response.ResponseStatus.ErrorMessage = "QRFID is not exists.";
				}
				return response;
			}
		}

		public async Task<QuoteSetRes> SubmitQuote(QuoteSetReq request)
		{
			QuoteSetRes quoteSetRes = new QuoteSetRes();
			try
			{
				if (!string.IsNullOrEmpty(request.QRFID))
				{
					var resQuote = await _MongoContext.mQuote.FindAsync(a => a.QRFID == request.QRFID).Result.FirstOrDefaultAsync();
					if (resQuote != null)
					{
						if (!string.IsNullOrEmpty(request.EnquiryPipeline))
						{
							var EnquiryPipeline = "Costing Pipeline";
							var VersionName = "Default";
							var VersionDesc = "Generated on Submit Quote";
							if (request.IsCopyQuote)
							{
								EnquiryPipeline = request.EnquiryPipeline;
								VersionDesc = "Generated on Copy Quote";
								VersionName = "CopyQuote";
							}

							//This method is use to delete old costing data for QRF 
							await DeleteOldQRFData(request.QRFID);

							//This method clone all position of quote into QRFPosition with Price and FOC
							await SaveDefaultQRFPosition(request.QRFID);

							//This method creates Guesstimate
							await SaveDefaultGuesstimate(request.QRFID);

							//This method clone Quote data into QRFPrice
							string qrfpriceid = await SaveQRFPrice(VersionName, VersionDesc, request.QRFID, request.PlacerEmail);
							if (string.IsNullOrEmpty(qrfpriceid)) return null;

							//In Copy Quote, Proposal and Itinerary are copied from old quote so no need to create it again.
							//In Copy Quote, no need to send mail and add followup.
							if (!request.IsCopyQuote)
							{
								//This method Creates Proposal and Itinerary
								bool proposal = await SaveDefaultProposal(request.QRFID, request.PlacerEmail);
								if (!proposal) return null;

								#region Add Followup 
								request.PlacerEmail = request.PlacerEmail.ToLower().Trim();
								var CompanyList = _MongoContext.mCompanies.AsQueryable();
								var FromUserContacts = CompanyList.Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a => !string.IsNullOrEmpty(a.MAIL) && a.MAIL.ToLower() == request.PlacerEmail))?.FirstOrDefault()?.ContactDetails;
								var FromUser = FromUserContacts.Where(a => a.MAIL.ToLower() == request.PlacerEmail).FirstOrDefault();
								var ToUserContacts = CompanyList.Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a => !string.IsNullOrEmpty(a.MAIL) && a.MAIL.ToLower() == request.PlacerEmail))?.FirstOrDefault()?.ContactDetails;
								var ToUser = ToUserContacts.Where(a => a.MAIL.ToLower() == request.PlacerEmail).FirstOrDefault();

								FollowUpSetRes response = new FollowUpSetRes();
								FollowUpSetReq followUprequest = new FollowUpSetReq();
								followUprequest.QRFID = request.QRFID;

								FollowUpTask task = new FollowUpTask();
								task.Task = "Costing Requested";
								task.FollowUpType = "Internal";
								task.FollowUpDateTime = DateTime.Now;

								task.FromEmail = request.PlacerEmail;
								if (FromUser != null)
								{
									task.FromName = FromUser.CommonTitle + " " + FromUser.FIRSTNAME + " " + FromUser.LastNAME;
									task.FromContact_Id = FromUser.Contact_Id;
								}

								task.ToEmail = request.CostingOfficer;
								if (ToUser != null)
								{
									task.ToName = ToUser.CommonTitle + " " + ToUser.FIRSTNAME + " " + ToUser.LastNAME;
									task.ToContact_Id = ToUser.Contact_Id;
								}

								task.Status = "Requested";
								task.Notes = "Costing Requested";

								var FollowUpTaskList = new List<FollowUpTask>();
								FollowUpTaskList.Add(task);

								followUprequest.FollowUp.Add(new FollowUp
								{
									FollowUp_Id = Guid.NewGuid().ToString(),
									FollowUpTask = FollowUpTaskList,
									CreateUser = request.PlacerEmail,
									CreateDate = DateTime.Now
								});
								await _quoteRepository.SetFollowUpForQRF(followUprequest);

								#endregion

								UpdateQuote(request, EnquiryPipeline, resQuote);

								#region Send Email 
								var objEmailGetReq = new EmailGetReq()
								{
									UserEmail = request.PlacerEmail,
									UserName = request.PlacerUser,
									QrfId = request.QRFID,
									QRFPriceId = qrfpriceid,
									Remarks = request.Remarks,
									DocumentType = DocType.SALESSUBMITQUOTE,
									EnquiryPipeline = request.EnquiryPipeline
								};
								var responseStatusMail = await _emailRepository.GenerateEmail(objEmailGetReq);
								if (responseStatusMail == null || responseStatusMail.ResponseStatus == null || string.IsNullOrEmpty(responseStatusMail.ResponseStatus.Status))
								{
									responseStatusMail.ResponseStatus = new ResponseStatus();
									responseStatusMail.ResponseStatus.Status = "Error";
									responseStatusMail.ResponseStatus.ErrorMessage = "Mail not sent.";
								}
								#endregion
							}
							else
								UpdateQuote(request, EnquiryPipeline, resQuote);

							//return request.QRFID;
							quoteSetRes.QRFID = request.QRFID;
							quoteSetRes.ResponseStatus.Status = "Success";
						}
						else
						{
							quoteSetRes.ResponseStatus.Status = "Error";
						}
					}
					else
					{
						quoteSetRes.ResponseStatus.Status = "Error";
					}
				}
				else
				{
					quoteSetRes.ResponseStatus.Status = "Error";
				}
			}
			catch (Exception ex)
			{
				quoteSetRes.ResponseStatus.Status = "Error";
			}
			return quoteSetRes;
		}

		#region Helper Methods for binding defaults

		public async void UpdateQuote(QuoteSetReq request, string EnquiryPipeline, mQuote resQuote)
		{
			await _MongoContext.mQuote.FindOneAndUpdateAsync(
												   Builders<mQuote>.Filter.Eq("QRFID", request.QRFID),
												   Builders<mQuote>.Update.
												   Set("CurrentPipeline", EnquiryPipeline).
												   Set("CurrentPipelineStep", "Itinerary").
												   Set("Remarks", request.Remarks).
												   Set("CurrentPipelineSubStep", "").
												   Set("QuoteResult", "Success").
												   Set("Status", "NewCostingPipeline").
												   Set("CostingOfficer", request.CostingOfficer).
												   Set("EditUser", request.PlacerEmail).
												   Set("EditDate", DateTime.Now).
												   Set("ValidForAcceptance", "On or before " + resQuote.CreateDate.AddDays(7).ToString("dd MMM yy"))
												   );

			await _MongoContext.mQRFPrice.FindOneAndUpdateAsync(
												   Builders<mQRFPrice>.Filter.Eq("QRFID", request.QRFID),
												   Builders<mQRFPrice>.Update.
												   Set("CostingOfficer", request.CostingOfficer).
												   Set("EditUser", request.PlacerEmail).
												   Set("EditDate", DateTime.Now).
												   Set("ValidForAcceptance", "On or before " + resQuote.CreateDate.AddDays(7).ToString("dd MMM yy")));
		}

		public async Task<bool> SaveDefaultProposal(string QRFID, string editUser)
		{
			try
			{
				if (!string.IsNullOrEmpty(QRFID))
				{
					await _MongoContext.mProposal.DeleteManyAsync(Builders<mProposal>.Filter.Eq("QRFID", QRFID));

					var ItineraryId = _MongoContext.mItinerary.AsQueryable().Where(a => a.QRFID == QRFID).Select(b => b.ItineraryID).FirstOrDefault();

					if (string.IsNullOrEmpty(ItineraryId))
						ItineraryId = Guid.NewGuid().ToString();

					var proposal = new mProposal();
					proposal.QRFID = QRFID;
					proposal.ProposalId = Guid.NewGuid().ToString();
					proposal.ItineraryId = ItineraryId;
					proposal.Version = 1;
					proposal.CreateUser = "Default";
					proposal.CreateDate = DateTime.Now;
					proposal.EditUser = editUser;
					proposal.EditDate = DateTime.Now;
					proposal.IsDeleted = false;

					await _MongoContext.mProposal.InsertOneAsync(proposal);
					await SaveDefaultItinerary(editUser, QRFID, proposal.ItineraryId);
				}
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public async Task<bool> SaveDefaultItinerary(string editUser, string QRFID, string ItineraryId = "", bool IsCosting = false)
		{

			try
			{
				//if (string.IsNullOrEmpty(ItineraryId))
				//{
				//    //ItineraryId = _MongoContext.mProposal.AsQueryable().Where(a => a.QRFID == QRFID).Select(b => b.ItineraryId).FirstOrDefault();
				//    ItineraryId = Guid.NewGuid().ToString();
				//}
				if (!string.IsNullOrEmpty(QRFID) && !string.IsNullOrEmpty(ItineraryId))
				{
					var lastElement = "";

					//await _MongoContext.mItinerary.DeleteManyAsync(Builders<mItinerary>.Filter.Eq("QRFID", QRFID));
					QRFSummaryGetReq qrfSummaryGetReq = new QRFSummaryGetReq();
					QRFSummaryGetRes qrfSummaryGetRes = new QRFSummaryGetRes();
					qrfSummaryGetReq.QRFID = QRFID;
					qrfSummaryGetReq.IsCosting = IsCosting;
					qrfSummaryGetRes = GetQRFSummary(qrfSummaryGetReq).Result;

					var existingitinerary = _MongoContext.mItinerary.AsQueryable().Where(x => x.QRFID == QRFID).FirstOrDefault();

					bool IsLdcCoachExist = false;

					foreach (var summary in qrfSummaryGetRes.SummaryDetailsInfo)
					{
						foreach (var details in summary.OriginalItineraryDetails)
						{
							if (details.ProductType != null && (details.ProductType.ToLower() == "coach" || details.ProductType.ToLower() == "ldc"))
							{
								IsLdcCoachExist = true;
								break;
							}
						}
					}

					var itinerary = new mItinerary();
					itinerary.QRFID = QRFID;
					itinerary.ItineraryID = ItineraryId;
					itinerary.Version = 1;
					itinerary.CreateUser = "Default";
					itinerary.CreateDate = DateTime.Now;
					itinerary.EditUser = editUser;
					itinerary.EditDate = DateTime.Now;
					itinerary.IsDeleted = false;

					var qrfAgentRoom = _MongoContext.mQRFPrice.AsQueryable().AsQueryable().Where(x => x.QRFID == QRFID && x.IsCurrentVersion == true).Select(y => y.QRFAgentRoom).ToList();

					List<string> cityNamesList = qrfSummaryGetRes.SummaryDetailsInfo.Select(x => x.PlaceOfService).ToList();
					List<string> ToCityNamesList = qrfSummaryGetRes.SummaryDetailsInfo.Select(x => x.ToCityName).ToList();

					List<Attributes> countryNamesList = _masterRepository.GetCountryNameByCityName(cityNamesList);
					List<Attributes> TocountryNamesList = _masterRepository.GetCountryNameByCityName(ToCityNamesList);

					foreach (var summary in qrfSummaryGetRes.SummaryDetailsInfo)
					{
						var hotels = summary.OriginalItineraryDetails.Where(x => x.ProductType.ToUpper() == "HOTEL").Select(y => new { y.OriginalItineraryDescription, y.ProductCategory, y.CityName, y.CountryName, y.Stars, y.ProductId, y.Duration, y.PositionId, y.IsDeleted });
						var ProdIdList = summary.OriginalItineraryDetails.Where(x => x.ProductType.ToUpper() == "HOTEL").Select(y => y.ProductId).ToList();
						var StarList = _MongoContext.mProductHotelAdditionalInfo.AsQueryable().Where(x => ProdIdList.Contains(x.ProductId)).Select(y => new { y.ProductId, y.StarRating }).ToList();
						var productList = _MongoContext.Products.AsQueryable().Where(x => ProdIdList.Contains(x.VoyagerProduct_Id)).Select(y => new { y.VoyagerProduct_Id, y.ProductDescription, y.Address, y.Street, y.PostCode, y.SupplierTel, y.Lat, y.Long, y.ProductFacilities, y.ProductResources, y.SupplierEmail });

						var positionIdList = summary.OriginalItineraryDetails.Where(x => x.ProductType.ToUpper() == "HOTEL").Select(y => y.PositionId).ToList();

						var positionList = new List<mPosition>();
						var qrfpositionList = new List<mQRFPosition>();


						if (IsCosting == true)
							qrfpositionList = _MongoContext.mQRFPosition.AsQueryable().Where(x => positionIdList.Contains(x.PositionId)).ToList();
						else
							positionList = _MongoContext.mPosition.AsQueryable().Where(x => positionIdList.Contains(x.PositionId)).ToList();

						var day = new ItineraryDaysInfo();
						ItineraryDescriptionInfo description;
						day.ItineraryDaysId = Guid.NewGuid().ToString();
						day.Day = summary.Day;
						day.Date = summary.OriginalItineraryDate;
						day.DayOfWeek = summary.OriginalItineraryDay;
						day.City = summary.PlaceOfService;
						//day.Country = summary.CountryName;
						day.Country = countryNamesList.Where(x => x.Attribute_Id == summary.PlaceOfService).Select(x => x.AttributeName)?.FirstOrDefault()?.ToString() ?? "";
						day.ItineraryName = summary.OriginalItineraryName;
						try
						{
							if (IsLdcCoachExist)
								day.RoutingMatrix = GetRoutingMatrixForItinerary(summary.RoutingCityIds);
						}
						catch (Exception e)
						{
						}
						day.ToCityName = summary.ToCityName;
						//day.ToCountryName = summary.ToCountryName;
						day.ToCountryName = TocountryNamesList.Where(x => x.Attribute_Id == summary.ToCityName).Select(x => x.AttributeName)?.FirstOrDefault()?.ToString() ?? "";
						day.Meal = summary.IncludedMeals;

						int totalNoOfRooms = 0; int count = 0;
						foreach (var room in qrfAgentRoom)
						{
							for (int i = 0; i < room.Count; i++)
							{
								count = count + room[i].RoomCount == null ? 0 : Convert.ToInt32(room[i].RoomCount);
								totalNoOfRooms = totalNoOfRooms + count;
							}
						}

						List<Hotel> hotel = new List<Hotel>();
						foreach (var k in hotels)
						{
							var product = productList.Where(a => a.VoyagerProduct_Id == k.ProductId).FirstOrDefault();
							var AlternateHotels = new List<AlternateServices>();
							var AlternateHotelsParameter = new AlternateServiesParameter();

							if (IsCosting == true)
							{
								AlternateHotels = qrfpositionList.Where(a => a.PositionId == k.PositionId).Select(b => b.AlternateHotels).FirstOrDefault();
								AlternateHotelsParameter = qrfpositionList.Where(a => a.PositionId == k.PositionId).Select(b => b.AlternateHotelsParameter).FirstOrDefault();
							}
							else
							{
								AlternateHotels = positionList.Where(a => a.PositionId == k.PositionId).Select(b => b.AlternateHotels).FirstOrDefault();
								AlternateHotelsParameter = positionList.Where(a => a.PositionId == k.PositionId).Select(b => b.AlternateHotelsParameter).FirstOrDefault();
							}

							var star = StarList.Where(a => a.ProductId == k.ProductId).FirstOrDefault();
							Hotel h = new Hotel();

							h.PositionId = k.PositionId;
							h.HotelName = k.OriginalItineraryDescription;
							h.ProdCategory = k.ProductCategory;
							h.Location = k.CityName + "," + k.CountryName;
							h.Stars = star != null && !string.IsNullOrEmpty(star.StarRating) && star.StarRating.Contains(' ') ? Convert.ToInt32(star.StarRating.Split(' ')[0]) : 0;
							h.Duration = k.Duration;
							h.ProdDesc = product.ProductDescription.Where(x => x.DescType?.ToLower() == "description").FirstOrDefault().ToString();
							h.Address = product.Address;
							h.FullAddress = product.Address?.Trim() + ((string.IsNullOrWhiteSpace(product.Address) == false) ? ", " : "") + product.Street?.Trim() + ((string.IsNullOrWhiteSpace(product.Street) == false) ? ", " : "") + product.PostCode?.Trim();
							h.Telephone = product.SupplierTel;
							h.Lat = product.Lat;
							h.Long = product.Long;
							h.ProdFacilities = product.ProductFacilities.Select(x => new ArrProductFacilities { FacilityDescription = x.FacilityDesc, Facility_Id = x.FacilityId, ProductFacility_Id = x.ProductFacilityId, Product_Id = k.ProductId }).ToList();
							h.ProdResources = product.ProductResources.Select(x => new ArrProductResources
							{
								ProductResource_Id = x.ProductResource_Id,
								ResourceType = x.ResourceType,
								ResourceType_Id = x.ResourceType_Id,
								OrderNr = x.OrderNr,
								ImageSRC = x.ImageSRC,
								Product_Id = k.ProductId,
								Name = x.Description
							}).ToList();
							h.Mail = product.SupplierEmail;
							h.TotalNumberOfRooms = totalNoOfRooms == 0 ? "" : Convert.ToString(totalNoOfRooms);
							h.IsDeleted = k.IsDeleted;
							h.AlternateHotels = AlternateHotels;
							h.AlternateHotelsParameter = AlternateHotelsParameter;

							hotel.Add(h);
						}
						day.Hotel = hotel;

						//bool flag = false;
						foreach (var details in summary.OriginalItineraryDetails.OrderBy(x => x.StartTime))
						{
							//if (details.ProductType == "Hotel") { flag = true; }
							description = new ItineraryDescriptionInfo();
							description.PositionId = details.PositionId;
							description.City = details.CityName;
							description.ProductType = details.ProductType;
							description.Type = details.ProductType != null ? "Service" : "Extra";
							description.ProductId = details.ProductId;
							description.ProductName = details.OriginalItineraryDescription;
							description.StartTime = details.StartTime;
							description.EndTime = details.EndTime;
							description.CreateDate = DateTime.Now;
							description.NumberOfPax = details.NumberOfPax;
							description.KeepAs = details.KeepAs;
							description.ProductDescription = details.ProductDescription;
							description.IsDeleted = details.IsDeleted;
							description.Duration = details.Duration;
							description.TLRemarks = details.TLRemarks;
							description.OPSRemarks = details.OPSRemarks;
							description.Supplier = details.Supplier;
							description.Allocation = details.Allocation;
							foreach (var room in details.RoomDetails)
							{
								description.RoomDetails.Add(new RoomInfo
								{
									ProductRangeId = room.ProductRangeId,
									ProductRange = room.ProductRange,
									ProdDesc = room.ProdDesc,
									RangeDesc = room.RangeDesc
								});
							}

							day.ItineraryDescription.Add(description);
						}

						if (summary.IncludedMeals != null)
						{
							if (existingitinerary != null && existingitinerary.ItineraryDays != null)
							{
								var iDesc = existingitinerary.ItineraryDays.Where(x => x.Day == summary.Day).Select(y => y.ItineraryDescription).FirstOrDefault();
								var meals = existingitinerary.ItineraryDays.Where(x => x.Day == summary.Day).Select(y => y.Meal).FirstOrDefault();
								foreach (var meal in summary.IncludedMeals.Where(x => string.IsNullOrEmpty(x.PositionId)))
								{
									if (meals != null && meals.Count > 0)
									{
										foreach (var m in meals)
										{
											if (m.MealType == meal.MealType)
											{
												var obj = iDesc?.Where(x => x.StartTime == meal.MealTime).FirstOrDefault();
												if (obj != null)
												{
													day.ItineraryDescription.Add(obj);
												}
												else
												{
													day.ItineraryDescription.Add(new ItineraryDescriptionInfo { PositionId = Guid.NewGuid().ToString(), Type = "Extra", StartTime = meal.MealTime, ProductName = meal.MealType + " in Hotel", CreateDate = DateTime.Now });
												}
											}
										}
									}
								}
							}
							else
							{
								foreach (var meal in summary.IncludedMeals.Where(x => string.IsNullOrEmpty(x.PositionId)))
								{
									day.ItineraryDescription.Add(new ItineraryDescriptionInfo { PositionId = Guid.NewGuid().ToString(), Type = "Extra", StartTime = meal.MealTime, ProductName = meal.MealType + " in Hotel", CreateDate = DateTime.Now });
								}
							}
						}

						//if (flag == true)
						//{
						//    var lastElement = "";
						//    if (summary.OriginalItineraryName.Contains(','))
						//        lastElement = summary.OriginalItineraryName.Split(',').Last();
						//    else
						//        lastElement = summary.OriginalItineraryName.Split(' ').Last();
						//    day.ItineraryDescription.Add(new ItineraryDescriptionInfo { PositionId = Guid.NewGuid().ToString(), Type = "Extra", StartTime = "22:00", ProductName = "Overnight in " + lastElement, CreateDate = DateTime.Now });
						//}
						itinerary.ItineraryDays.Add(day);
					}

					//To add existing add new itinerary element positions into new itinerary					
					if (existingitinerary != null)
					{
						foreach (var edays in existingitinerary.ItineraryDays)
						{
							foreach (var qdays in qrfSummaryGetRes.SummaryDetailsInfo)
							{
								foreach (var newobj in itinerary.ItineraryDays)
								{
									if (edays.Day == qdays.Day)
									{
										foreach (var epos in edays.ItineraryDescription)
										{
											if (epos.Type.ToLower() == "extra" && !string.IsNullOrEmpty(epos.EndTime) && newobj.Day == edays.Day && epos.IsRoutingMatrix == false)
											{
												ItineraryDescriptionInfo obj = new ItineraryDescriptionInfo();
												obj.City = epos.City;
												obj.PositionId = epos.PositionId;
												obj.ProductType = epos.ProductType;
												obj.Type = epos.Type;
												obj.StartTime = epos.StartTime;
												obj.EndTime = epos.EndTime;
												obj.ProductName = epos.ProductName;
												obj.NumberOfPax = epos.NumberOfPax;
												obj.KeepAs = epos.KeepAs;
												obj.IsDeleted = epos.IsDeleted;
												obj.CreateDate = epos.CreateDate;
												obj.CreateUser = epos.CreateUser;
												obj.EditDate = epos.EditDate;
												obj.EditUser = epos.EditUser;
												obj.IsRoutingMatrix = epos.IsRoutingMatrix;
												newobj.ItineraryDescription.Add(obj);
											}
										}
									}
								}
							}
						}
					}

					#region Remarks
					// Overnight in Hotel & Meal under own arrangements starts here
					int dayno = 0;
					int? dayduration = 0;
					int? hoteldayduration = 0;
					bool isBreakfastExist = false;
					bool isLunchExist = false;
					bool isDinnerExist = false;

					var lastdayno = Convert.ToInt32(itinerary.ItineraryDays.LastOrDefault().Day.Replace("Day ", ""));


					foreach (var item in itinerary.ItineraryDays)
					{
						dayno = Convert.ToInt32(item.Day.Replace("Day ", ""));

						var existingremarks = existingitinerary?.ItineraryDays.Where(x => x.Day == item.Day).Select(x => x.ItineraryDescription).FirstOrDefault();

						// Meal under own arrangements remarks Starts here  (Dev: Anand Desai)
						#region "Meal under own arrangements remarks"
						var meals = item.Meal.Where(x => x.IsDeleted == false).ToList(); // Taking Meal Array for the Day

						// Assigning default values to variables
						string mealComment = " under own arrangements";
						isBreakfastExist = false;
						isLunchExist = false;
						isDinnerExist = false;

						// If Meal Array in Not Empty then only perform look up. Otherwise IsExist flag remains false
						if (meals.Count > 0)
						{
							// Checking if breakfast exists in the Meal Array
							var breakfast = meals.Where(x => x.MealType.ToUpper() == "BREAKFAST").Select(x => x).FirstOrDefault();
							// Setting IsExist flag to true if breakfast exists
							if (breakfast != null)
								isBreakfastExist = true;

							// Checking if lunch exists in the Meal Array
							var lunch = meals.Where(x => x.MealType.ToUpper() == "LUNCH").Select(x => x).FirstOrDefault();
							// Setting IsExist flag to true if lunch exists
							if (lunch != null)
								isLunchExist = true;

							// Checking if dinner exists in the Meal Array
							var dinner = meals.Where(x => x.MealType.ToUpper() == "DINNER").Select(x => x).FirstOrDefault();
							// Setting IsExist flag to true if dinner exists
							if (dinner != null)
								isDinnerExist = true;
						}

						// Adding Meal Comments if Flag remains Flase
						if (!isBreakfastExist && dayno != 1)
						{
							var obj = existingremarks?.Where(x => x.Type == "Extra" && x.StartTime == "08:00").FirstOrDefault();
							if (obj != null)
							{
								item.ItineraryDescription.Add(obj);
							}
							else
							{
								item.ItineraryDescription.Add(new ItineraryDescriptionInfo { PositionId = Guid.NewGuid().ToString(), Type = "Extra", StartTime = "08:00", ProductName = "Breakfast" + mealComment, CreateDate = DateTime.Now });
							}
						}

						if (!isLunchExist)
						{
							var obj = existingremarks?.Where(x => x.Type == "Extra" && x.StartTime == "13:00").FirstOrDefault();
							if (obj != null)
							{
								item.ItineraryDescription.Add(obj);
							}
							else
							{
								item.ItineraryDescription.Add(new ItineraryDescriptionInfo { PositionId = Guid.NewGuid().ToString(), Type = "Extra", StartTime = "13:00", ProductName = "Lunch" + mealComment, CreateDate = DateTime.Now });
							}
						}

						if (!isDinnerExist)
						{
							var obj = existingremarks?.Where(x => x.Type == "Extra" && x.StartTime == "20:00").FirstOrDefault();
							if (obj != null)
							{
								item.ItineraryDescription.Add(obj);
							}
							else
							{
								item.ItineraryDescription.Add(new ItineraryDescriptionInfo { PositionId = Guid.NewGuid().ToString(), Type = "Extra", StartTime = "20:00", ProductName = "Dinner" + mealComment, CreateDate = DateTime.Now });
							}
						}

						#endregion
						// Meal under own arrangements remarks ends here
						if (dayduration == 0)
						{
							var pos = item.ItineraryDescription.Where(a => a.ProductType.ToLower() == "hotel" && a.IsDeleted == false).FirstOrDefault();
							if (pos != null && pos.Duration > 0)
							{
								dayduration = pos.Duration;
								if (dayduration == 1)
								{
									dayduration = dayno;
								}
								else
								{
									dayduration = (dayno == 1) ? dayduration : ((dayno + dayduration) - 1);
								}
							}
						}

						if (dayno <= dayduration)
						{
							lastElement = "";
							if (item.ItineraryName.Contains(','))
								lastElement = item.ItineraryName.Split(',').Last();
							else if (item.ItineraryName.Contains("Arrive at"))
								lastElement = item.ItineraryName.Replace("Arrive at", "");
							else
								lastElement = item.ItineraryName;
							item.Desc = "Overnight in " + lastElement;
							dayduration = (dayno == dayduration) ? 0 : dayduration;
						}
						else
						{
							item.Desc = "Overnight under own arrangement";
						}

						#region Itinerary - Auto Remarks - No Hotel Stay Set for the Date
						if (lastdayno != dayno)
						{
							if (hoteldayduration == 0)
							{
								var posHotel = qrfSummaryGetRes.RoutingInfoCity.Where(a => a.DayName == item.Day && string.IsNullOrEmpty(a.PositionId)).FirstOrDefault();

								if (posHotel != null && posHotel.Duration > 0)
								{
									hoteldayduration = posHotel.Duration;
									if (dayduration == 1)
									{
										hoteldayduration = dayno;
									}
									else
									{
										hoteldayduration = (dayno == 1) ? hoteldayduration : ((dayno + hoteldayduration) - 1);
									}
								}
							}

							//if (dayno <= hoteldayduration)
							//{
							//	item.ItineraryDescription.Add(new ItineraryDescriptionInfo
							//	{
							//		PositionId = Guid.NewGuid().ToString(),
							//		Type = "Extra",
							//		StartTime = "22:00",
							//		ProductName = "Overnight under own arrangement",
							//		CreateDate = DateTime.Now
							//	});
							//	hoteldayduration = (dayno == hoteldayduration) ? 0 : hoteldayduration;
							//}
						}
						#endregion
					}

					foreach (var a in itinerary.ItineraryDays)
					{
						var existingremarks = existingitinerary?.ItineraryDays.Where(x => x.Day == a.Day).Select(x => x.ItineraryDescription).FirstOrDefault();

						if (!string.IsNullOrEmpty(a.Desc))
						{
							var pos = existingremarks?.Where(x => x.ProductType.ToLower() == "hotel").FirstOrDefault();
							var obj = existingremarks?.Where(x => x.Type == "Extra" && x.StartTime == "22:00").FirstOrDefault();
							if (obj != null && pos != null)
							{
								if (pos.IsDeleted == true)
								{
									//if (obj.ProductName == "Overnight under own arrangement")
									//{
										a.ItineraryDescription.Add(new ItineraryDescriptionInfo { PositionId = Guid.NewGuid().ToString(), Type = "Extra", StartTime = "22:00", ProductName = a.Desc, CreateDate = DateTime.Now });
									//}
									//else
									//	a.ItineraryDescription.Add(new ItineraryDescriptionInfo { PositionId = obj.PositionId, Type = "Extra", StartTime = "22:00", ProductName = obj.ProductName, CreateDate = DateTime.Now, IsDeleted = obj.IsDeleted });
								}
								else
								{
									//if (obj.ProductName == "Overnight under own arrangement")
									//{
										a.ItineraryDescription.Add(new ItineraryDescriptionInfo { PositionId = Guid.NewGuid().ToString(), Type = "Extra", StartTime = "22:00", ProductName = a.Desc, CreateDate = DateTime.Now });
									//}
									//else
									//	a.ItineraryDescription.Add(new ItineraryDescriptionInfo { PositionId = obj.PositionId, Type = "Extra", StartTime = "22:00", ProductName = obj.ProductName, CreateDate = DateTime.Now, IsDeleted = obj.IsDeleted });
								}
							}
							else
							{
								a.ItineraryDescription.Add(new ItineraryDescriptionInfo { PositionId = Guid.NewGuid().ToString(), Type = "Extra", StartTime = "22:00", ProductName = a.Desc, CreateDate = DateTime.Now });
							}
						}
					}
					// Overnight in Hotel & Meal under own arrangements ends here

					//Check In and Check Out Remarks starts here (Dev: Anand Desai)
					#region "Check In and Check Out Remarks"
					// Taking All Hotel Positions into List
					var qrfHotels = _MongoContext.mPosition.AsQueryable().Where(x => x.QRFID == QRFID && x.ProductType == "Hotel" && x.IsDeleted == false).Select(y => y).ToList();

					// Looping through List of Hotel Positions
					foreach (mPosition hotel in qrfHotels)
					{
						// Declaring and Assigning default values to variables
						int hotelStart = hotel.DayNo; // hotelStart is Check In Day
						int hotelDuration = (hotel.Duration < 1 ? 1 : hotel.Duration); // Sum of hotelStart and Duration will be Check Out Day
						string hotelStartTime = (hotel.StartTime ?? "18:00"); //If Start Time is not Defined for Hotel then take default check in at 18:00
						string hotelEndTime = (hotel.EndTime ?? "09:00"); //If End Time is not Defined for Hotel then take default check out at 09:00
						string hotelName = (hotel.ProductName ?? "Hotel").ToString().Trim();

						// If Hotel is added for valid day then only perform lookup
						if (hotelStart != 0)
						{
							// Taking Start Day of the Hotel
							ItineraryDaysInfo itinStartDay = itinerary.ItineraryDays.Where(x => x.Day == ("Day " + Convert.ToString(hotelStart))).Select(x => x).FirstOrDefault();
							// Taking End Day of the Hotel
							ItineraryDaysInfo itinEndDay = itinerary.ItineraryDays.Where(x => x.Day == ("Day " + Convert.ToString(hotelStart + hotelDuration))).Select(x => x).FirstOrDefault();

							// If Start Day is valid day then add Check in Remarks at Hotel Start Time
							//if (itinStartDay != null)
							//itinStartDay.ItineraryDescription.Add(new ItineraryDescriptionInfo { PositionId = Guid.NewGuid().ToString(), Type = "Extra", StartTime = hotelStartTime, ProductName = "Check-in into " + hotelName, CreateDate = DateTime.Now });
							// If End Day is valid day then add Check out Remarks at Hotel End Time
							if (itinEndDay != null)
							{
								var DayNo = "Day " + Convert.ToString(hotelStart + hotelDuration);
								var obj1 = existingitinerary?.ItineraryDays?.Where(x => x.Day == DayNo).Select(x => x.ItineraryDescription).FirstOrDefault();
								var obj = obj1?.Where(x => x.Type == "Extra" && x.StartTime == hotelEndTime).FirstOrDefault();
								if (obj != null)
								{
									itinEndDay.ItineraryDescription.Add(obj);
								}
								else
								{
									itinEndDay.ItineraryDescription.Add(new ItineraryDescriptionInfo { PositionId = Guid.NewGuid().ToString(), Type = "Extra", StartTime = hotelEndTime, ProductName = "Check-out from " + hotelName, CreateDate = DateTime.Now });
								}
							}
						}
					}
					#endregion
					//Check In and Check Out Remarks ends here

					//To remove last element in last day
					if (itinerary.ItineraryDays.Count > 0)
					{
						var lastDay = itinerary.ItineraryDays.Last();
						var lastelement = lastDay.ItineraryDescription.Where(x => x.StartTime == "22:00").FirstOrDefault();
						lastDay.ItineraryDescription.Remove(lastelement);
					}
					#endregion

					itinerary.ItineraryDays.ForEach(b => b.ItineraryDescription = b.ItineraryDescription.OrderBy(c => c.StartTime).ToList());

					#region routing details
					try
					{
						if (IsLdcCoachExist)
						{
							for (int i = 0; i < itinerary.ItineraryDays.Count; i++)
							{
								var positions = itinerary.ItineraryDays[i].ItineraryDescription.Where(a => a.Type == "Service" && a.IsDeleted == false && (a.ProductType == "Hotel" || a.ProductType == "Apartments" || a.ProductType == "Meal" || a.ProductType == "Attractions" || a.ProductType.ToLower() == "sightseeing - citytour")).ToList();

								for (int j = 0; j < positions.Count; j++)
								{
									if (i == 0 && j == 0)
									{
										//do nothing
									}
									else if (j == 0)
									{
										var hotelPrevDay = itinerary.ItineraryDays[i - 1].ItineraryDescription.Where(a => a.Type == "Service" && a.IsDeleted == false && a.ProductType == "Hotel").OrderByDescending(c => c.StartTime).FirstOrDefault();

										if (hotelPrevDay != null && (hotelPrevDay.ProductId != positions[j].ProductId))
										{
											string RoutingMatrix = "";
											DistanceMatrixGetRes response;
											response = _genericRepository.GetDistanceMatrixForProduct(hotelPrevDay.ProductId, positions[j].ProductId).Result;
											if (response != null)
											{
												if (response.status == "OK")
												{
													if (hotelPrevDay.City == positions[j].City)
														RoutingMatrix = "From " + hotelPrevDay.ProductName + " to " + positions[j].ProductName + " (" + response.Rows[0].Elements[0].distance.text + " - " + response.Rows[0].Elements[0].duration.text + ")";
													else
														RoutingMatrix = "From " + hotelPrevDay.ProductName + ", " + hotelPrevDay.City + " to " + positions[j].ProductName + ", " + positions[j].City + " (" + response.Rows[0].Elements[0].distance.text + " - " + response.Rows[0].Elements[0].duration.text + ")";

													var newDesc = new ItineraryDescriptionInfo();
													newDesc.PositionId = Guid.NewGuid().ToString();
													newDesc.Type = "Extra";
													newDesc.StartTime = positions[j].StartTime;
													newDesc.EndTime = positions[j].StartTime;
													newDesc.City = positions[j].City;
													newDesc.CreateDate = DateTime.Now;
													newDesc.ProductName = RoutingMatrix;
													newDesc.IsRoutingMatrix = true;

													var index = itinerary.ItineraryDays[i].ItineraryDescription.FindIndex(x => x.PositionId == positions[j].PositionId);
													itinerary.ItineraryDays[i].ItineraryDescription.Insert(index, newDesc);
												}
											}
										}
									}
									else if (j != 0 && (positions[j - 1].ProductId != positions[j].ProductId))
									{
										string RoutingMatrix = "";
										DistanceMatrixGetRes response;
										response = _genericRepository.GetDistanceMatrixForProduct(positions[j - 1].ProductId, positions[j].ProductId).Result;
										if (response != null)
										{
											if (response.status == "OK")
											{
												if (positions[j - 1].City == positions[j].City)
													RoutingMatrix = "From " + positions[j - 1].ProductName + " to " + positions[j].ProductName + " (" + response.Rows[0].Elements[0].distance.text + " - " + response.Rows[0].Elements[0].duration.text + ")";
												else
													RoutingMatrix = "From " + positions[j - 1].ProductName + ", " + positions[j - 1].City + " to " + positions[j].ProductName + ", " + positions[j].City + " (" + response.Rows[0].Elements[0].distance.text + " - " + response.Rows[0].Elements[0].duration.text + ")";

												var newDesc = new ItineraryDescriptionInfo();
												newDesc.PositionId = Guid.NewGuid().ToString();
												newDesc.Type = "Extra";
												newDesc.StartTime = positions[j].StartTime;
												newDesc.EndTime = positions[j].StartTime;
												newDesc.City = positions[j].City;
												newDesc.CreateDate = DateTime.Now;
												newDesc.ProductName = RoutingMatrix;
												newDesc.IsRoutingMatrix = true;

												var index = itinerary.ItineraryDays[i].ItineraryDescription.FindIndex(x => x.PositionId == positions[j].PositionId);
												itinerary.ItineraryDays[i].ItineraryDescription.Insert(index, newDesc);
											}
										}
									}
								}
							}
						}
					}
					catch (Exception e)
					{
					}
					#endregion


					await _MongoContext.mItinerary.DeleteManyAsync(Builders<mItinerary>.Filter.Eq("QRFID", QRFID));
					await _MongoContext.mItinerary.InsertOneAsync(itinerary);

					#region Update RegenerateItinerary flag in mQuote
					await _MongoContext.mQRFPrice.UpdateManyAsync(Builders<mQRFPrice>.Filter.Eq("QRFID", QRFID),
							   Builders<mQRFPrice>.Update.Set("RegenerateItinerary", false).Set("EditUser", editUser).Set("EditDate", DateTime.Now));

					await _MongoContext.mQuote.UpdateOneAsync(Builders<mQuote>.Filter.Eq("QRFID", QRFID),
						   Builders<mQuote>.Update.Set("RegenerateItinerary", false).Set("EditUser", editUser).Set("EditDate", DateTime.Now));
					#endregion
				}
				return true;
			}
			catch (Exception ex)
			{
				#region Update RegenerateItinerary flag in mQuote
				await _MongoContext.mQRFPrice.UpdateManyAsync(Builders<mQRFPrice>.Filter.Eq("QRFID", QRFID),
						   Builders<mQRFPrice>.Update.Set("RegenerateItinerary", true).Set("EditUser", editUser).Set("EditDate", DateTime.Now));

				await _MongoContext.mQuote.UpdateOneAsync(Builders<mQuote>.Filter.Eq("QRFID", QRFID),
					   Builders<mQuote>.Update.Set("RegenerateItinerary", true).Set("EditUser", editUser).Set("EditDate", DateTime.Now));
				#endregion
				throw;
			}
		}

		public async Task<bool> DeleteOldQRFData(string QRFID)
		{
			await _MongoContext.mQRFPosition.DeleteManyAsync(Builders<mQRFPosition>.Filter.Eq("QRFID", QRFID));

			await _MongoContext.mPositionPriceQRF.DeleteManyAsync(Builders<mPositionPriceQRF>.Filter.Eq("QRFID", QRFID));

			await _MongoContext.mQRFPositionFOC.DeleteManyAsync(Builders<mQRFPositionFOC>.Filter.Eq("QRFID", QRFID));

			await _MongoContext.mGuesstimate.DeleteManyAsync(Builders<mGuesstimate>.Filter.Eq("QRFID", QRFID));

			await _MongoContext.mQRFPrice.DeleteManyAsync(Builders<mQRFPrice>.Filter.Eq("QRFID", QRFID));

			await _MongoContext.mQRFPositionTotalCost.DeleteManyAsync(Builders<mQRFPositionTotalCost>.Filter.Eq("QRFID", QRFID));

			await _MongoContext.mQRFPositionPrice.DeleteManyAsync(Builders<mQRFPositionPrice>.Filter.Eq("QRFID", QRFID));

			await _MongoContext.mQRFPackagePrice.DeleteManyAsync(Builders<mQRFPackagePrice>.Filter.Eq("QRFID", QRFID));

			await _MongoContext.mQRFNonPackagedPrice.DeleteManyAsync(Builders<mQRFNonPackagedPrice>.Filter.Eq("QRFID", QRFID));

			return true;
		}

		public async Task<bool> SaveDefaultQRFPosition(string QRFID)
		{
			try
			{
				#region Position
				var quote = _MongoContext.mQuote.AsQueryable().Where(a => a.QRFID == QRFID).FirstOrDefault();

				var position = _MongoContext.mPosition.AsQueryable().Where(a => a.QRFID == QRFID).ToList();

				List<RoomDetailsInfo> RoomDetailsInfo = new List<RoomDetailsInfo>();
				foreach (var pos in position)
				{
					RoomDetailsInfo.AddRange(pos.RoomDetailsInfo);
				}
				var rangeIds = RoomDetailsInfo.Select(a => a.ProductRangeId).ToList();

				var ProductRange = _MongoContext.mProductRange.AsQueryable().Where(a => rangeIds.Contains(a.VoyagerProductRange_Id)).ToList();

				int ADULTCount = quote.AgentPassengerInfo.Where(a => a.Type == "ADULT").Select(b => b.count).FirstOrDefault();
				int INFANTCount = quote.AgentPassengerInfo.Where(a => a.Type == "INFANT").Select(b => b.count).FirstOrDefault();
				int CHILDWITHBEDCount = quote.AgentPassengerInfo.Where(a => a.Type == "CHILDWITHBED").Select(b => b.count).FirstOrDefault();
				int CHILDWITHOUTBEDCount = quote.AgentPassengerInfo.Where(a => a.Type == "CHILDWITHOUTBED").Select(b => b.count).FirstOrDefault();

				if (position != null && position.Count > 0)
				{
					foreach (var pos in position)
					{
						var qrfPos = new mQRFPosition();

						qrfPos._Id = pos._Id;
						qrfPos.QRFID = pos.QRFID;
						qrfPos.PositionId = pos.PositionId;
						qrfPos.PositionSequence = pos.PositionSequence;
						qrfPos.ProductType = pos.ProductType;
						qrfPos.ProductTypeId = pos.ProductTypeId;
						qrfPos.DayNo = pos.DayNo;
						qrfPos.RoutingDaysID = pos.RoutingDaysID;
						qrfPos.StartingFrom = pos.StartingFrom;
						qrfPos.ProductAttributeType = pos.ProductAttributeType;
						qrfPos.CountryName = pos.CountryName;
						qrfPos.CityName = pos.CityName;
						qrfPos.CityID = pos.CityID;
						qrfPos.ProductName = pos.ProductName;
						qrfPos.ProductID = pos.ProductID;
						qrfPos.BudgetCategoryId = pos.BudgetCategoryId;
						qrfPos.BudgetCategory = pos.BudgetCategory;
						qrfPos.SupplierId = pos.SupplierId;
						qrfPos.SupplierName = pos.SupplierName;
						qrfPos.StartTime = pos.StartTime;
						qrfPos.EndTime = pos.EndTime;
						qrfPos.Duration = pos.Duration;
						qrfPos.KeepAs = pos.KeepAs;
						qrfPos.TLRemarks = pos.TLRemarks;
						qrfPos.OPSRemarks = pos.OPSRemarks;
						qrfPos.Status = pos.Status;
						qrfPos.CreateUser = pos.CreateUser;
						qrfPos.CreateDate = pos.CreateDate;
						qrfPos.EditUser = pos.EditUser;
						qrfPos.EditDate = pos.EditDate;
						qrfPos.IsDeleted = pos.IsDeleted;

						qrfPos.StarRating = pos.StarRating;
						qrfPos.Location = pos.Location;
						qrfPos.ChainName = pos.ChainName;
						qrfPos.ChainID = pos.ChainID;
						qrfPos.MealPlan = pos.MealPlan;
						qrfPos.EarlyCheckInDate = pos.EarlyCheckInDate;
						qrfPos.EarlyCheckInTime = pos.EarlyCheckInTime;
						qrfPos.InterConnectingRooms = pos.InterConnectingRooms;
						qrfPos.WashChangeRooms = pos.WashChangeRooms;
						qrfPos.LateCheckOutDate = pos.LateCheckOutDate;
						qrfPos.LateCheckOutTime = pos.LateCheckOutTime;
						qrfPos.DeletedFrom = pos.DeletedFrom;
						qrfPos.TypeOfExcursion = pos.TypeOfExcursion;
						qrfPos.TypeOfExcursion_Id = pos.TypeOfExcursion_Id;
						qrfPos.NoOfPaxAdult = pos.NoOfPaxAdult;
						qrfPos.NoOfPaxChild = pos.NoOfPaxChild;
						qrfPos.NoOfPaxInfant = pos.NoOfPaxInfant;
						qrfPos.MealType = pos.MealType;
						qrfPos.TransferDetails = pos.TransferDetails;
						qrfPos.ApplyAcrossDays = pos.ApplyAcrossDays;
						qrfPos.FromPickUpLoc = pos.FromPickUpLoc;
						qrfPos.FromPickUpLocID = pos.FromPickUpLocID;

						qrfPos.ToCityName = pos.ToCityName;
						qrfPos.ToCityID = pos.ToCityID;
						qrfPos.ToCountryName = pos.ToCountryName;
						qrfPos.ToDropOffLoc = pos.ToDropOffLoc;
						qrfPos.ToDropOffLocID = pos.ToDropOffLocID;
						qrfPos.BuyCurrencyId = pos.BuyCurrencyId;
						qrfPos.BuyCurrency = pos.BuyCurrency;
						qrfPos.StandardPrice = pos.StandardPrice;
						qrfPos.StandardFOC = pos.StandardFOC;
						qrfPos.IsCityPermit = pos.IsCityPermit;
						qrfPos.IsParkingCharges = pos.IsParkingCharges;
						qrfPos.IsRoadTolls = pos.IsRoadTolls;
						qrfPos.AlternateHotels = pos.AlternateHotels;
						qrfPos.AlternateHotelsParameter = pos.AlternateHotelsParameter;
						qrfPos.IsTourEntity = pos.IsTourEntity;

						foreach (var room in pos.RoomDetailsInfo)
						{
							if (!room.IsDeleted)
							{
								var newRoom = new QRFRoomDetailsInfo();

								var personType = ProductRange.Where(a => a.VoyagerProductRange_Id == room.ProductRangeId).Select(b => b.PersonType).FirstOrDefault();
								var ProductRangeCode = ProductRange.Where(a => a.VoyagerProductRange_Id == room.ProductRangeId).Select(b => b.ProductRangeCode).FirstOrDefault();
								var ChargeBasis = "PUPD";
								if (personType == "ADULT")
									ChargeBasis = "PP";
								else if (personType == "SINGLE" || personType == "DOUBLE" || personType == "TWIN" || personType == "TRIPLE" || personType == "QUAD" || personType == "TSU")
									ChargeBasis = "PRPN";

								int count = 1;
								if (personType == "ADULT") count = ADULTCount;
								else if (personType == "INFANT") count = INFANTCount;
								else if (personType == "CHILDWITHBED") count = CHILDWITHBEDCount;
								else if (personType == "CHILDWITHOUTBED") count = CHILDWITHOUTBEDCount;

								count = (count < 1) ? 1 : count;
								newRoom.RoomId = room.RoomId;
								newRoom.RoomSequence = room.RoomSequence;
								newRoom.ProductCategoryId = room.ProductCategoryId;
								newRoom.ProductRangeId = room.ProductRangeId;
								newRoom.ProductCategory = room.ProductCategory;
								newRoom.ProductRange = room.ProductRange;
								newRoom.IsSupplement = room.IsSupplement;
								newRoom.CrossPositionId = room.CrossPositionId;
								// newRoom.CrossPosition = room.CrossPosition;
								//newRoom.Count = count;
								if (ProductRangeCode == "SINGLE" || ProductRangeCode == "DOUBLE" || ProductRangeCode == "TWIN" || ProductRangeCode == "TRIPLE" || ProductRangeCode == "QUAD" || ProductRangeCode == "TSU")
									newRoom.Count = (quote.AgentRoom.Where(x => x.RoomTypeName.ToUpper() == ProductRangeCode.ToUpper()).Select(x => x.RoomCount).FirstOrDefault() == null) ? count : Convert.ToInt32(quote.AgentRoom.Where(x => x.RoomTypeName.ToUpper() == ProductRangeCode.ToUpper()).Select(x => x.RoomCount).FirstOrDefault());
								else
									newRoom.Count = count;
								newRoom.ChargeBasis = ChargeBasis;
								newRoom.CreateUser = room.CreateUser;
								newRoom.CreateDate = room.CreateDate;
								newRoom.EditUser = room.EditUser;
								newRoom.EditDate = room.EditDate;
								newRoom.IsDeleted = room.IsDeleted;

								qrfPos.RoomDetailsInfo.Add(newRoom);
							}
						}

						await _MongoContext.mQRFPosition.InsertOneAsync(qrfPos);

						#region Price

						var positionPrice = _MongoContext.mPositionPrice.AsQueryable().Where(a => a.PositionId == pos.PositionId).ToList();
						if (positionPrice.Count > 0)
						{
							List<mPositionPriceQRF> positionPriceQRF = new List<mPositionPriceQRF>();
							foreach (var price in positionPrice)
							{
								positionPriceQRF.Add(new mPositionPriceQRF
								{
									_Id = price._Id,
									PositionPriceId = price.PositionPriceId,
									QRFID = price.QRFID,
									Period = price.Period,
									PositionId = price.PositionId,
									DepartureId = price.DepartureId,
									PaxSlabId = price.PaxSlabId,
									PaxSlab = price.PaxSlab,
									ContractPeriod = price.ContractPeriod,
									Type = price.Type,
									RoomId = price.RoomId,
									IsSupplement = price.IsSupplement,
									SupplierId = price.SupplierId,
									Supplier = price.Supplier,
									ProductCategoryId = price.ProductCategoryId,
									ProductCategory = price.ProductCategory,
									ProductRangeId = price.ProductRangeId,
									ProductRange = price.ProductRange,
									ProductRangeCode = price.ProductRangeCode,
									TourEntityId = price.TourEntityId,

									BuyCurrencyId = price.BuyCurrencyId,
									BuyCurrency = price.BuyCurrency,
									ContractId = price.ContractId,
									ContractPrice = price.ContractPrice,
									BudgetPrice = price.BudgetPrice,
									BuyPrice = price.BuyPrice,
									MarkupAmount = price.MarkupAmount,
									BuyNetPrice = price.BuyNetPrice,

									SellCurrencyId = price.SellCurrencyId,
									SellCurrency = price.SellCurrency,
									SellNetPrice = price.SellNetPrice,
									TaxAmount = price.TaxAmount,
									SellPrice = price.SellPrice,
									ExchangeRateId = price.ExchangeRateId,
									ExchangeRatio = price.ExchangeRatio,

									CreateUser = price.CreateUser,
									CreateDate = price.CreateDate,
									EditUser = price.EditUser,
									EditDate = price.EditDate,
									IsDeleted = price.IsDeleted
								});
							}
							await _MongoContext.mPositionPriceQRF.InsertManyAsync(positionPriceQRF);
						}
						#endregion

						#region FOC

						var positionFOC = _MongoContext.mPositionFOC.AsQueryable().Where(a => a.PositionId == pos.PositionId).ToList();
						if (positionFOC.Count > 0)
						{
							List<mQRFPositionFOC> positionFOCQRF = new List<mQRFPositionFOC>();
							foreach (var foc in positionFOC)
							{
								positionFOCQRF.Add(new mQRFPositionFOC
								{
									_Id = foc._Id,
									PositionFOCId = foc.PositionFOCId,
									QRFID = foc.QRFID,
									Period = foc.Period,
									ContractPeriod = foc.ContractPeriod,
									PositionId = foc.PositionId,
									DepartureId = foc.DepartureId,
									PaxSlabId = foc.PaxSlabId,
									PaxSlab = foc.PaxSlab,
									Type = foc.Type,
									CityId = foc.CityId,
									CityName = foc.CityName,
									ProductId = foc.ProductId,
									ProductName = foc.ProductName,
									RoomId = foc.RoomId,
									IsSupplement = foc.IsSupplement,
									SupplierId = foc.SupplierId,
									Supplier = foc.Supplier,
									ProductCategoryId = foc.ProductCategoryId,
									ProductCategory = foc.ProductCategory,
									ProductRangeId = foc.ProductRangeId,
									ProductRange = foc.ProductRange,
									ContractId = foc.ContractId,
									Quantity = foc.Quantity,
									FOCQty = foc.FOCQty,
									CreateUser = foc.CreateUser,
									CreateDate = foc.CreateDate,
									EditUser = foc.EditUser,
									EditDate = foc.EditDate,
									IsDeleted = foc.IsDeleted
								});
							}
							await _MongoContext.mQRFPositionFOC.InsertManyAsync(positionFOCQRF);
						}
						#endregion

					}
				}


				#endregion
			}
			catch (Exception ex)
			{
				throw;
			}
			return true;
		}

		public async Task<bool> SaveDefaultGuesstimate(string QRFID, string VersionName = "Default", string VersionDescription = "Deafult", string UserName = "")
		{
			if (!string.IsNullOrEmpty(QRFID))
			{
				var objPositionPricesList = _MongoContext.mPositionPriceQRF.AsQueryable().Where(a => a.QRFID == QRFID && a.IsDeleted == false).ToList();

				#region Get Summary Data
				var request = new QRFSummaryGetReq();
				var response = new QRFSummaryGetRes();
				request.QRFID = QRFID;
				if (VersionName == "Default")
					request.IsCosting = false;
				else
					request.IsCosting = true;

				response = await GetQRFSummary(request);

				#endregion

				var guesstimate = new mGuesstimate();
				guesstimate.GuesstimateId = Guid.NewGuid().ToString();
				guesstimate.QRFID = QRFID;
				if (VersionName == "Default")
				{
					guesstimate.VersionId = 1;
					guesstimate.VersionName = "FirstVersion";
					guesstimate.VersionDescription = "FirstVersion";
				}
				else
				{
					var guesstimateOld = _MongoContext.mGuesstimate.AsQueryable().Where(a => a.QRFID == QRFID && a.IsCurrentVersion == true).FirstOrDefault();

					guesstimateOld.IsCurrentVersion = false;
					guesstimateOld.EditUser = UserName;
					guesstimateOld.EditDate = DateTime.Now;
					ReplaceOneResult replaceResult = await _MongoContext.mGuesstimate.ReplaceOneAsync(Builders<mGuesstimate>.Filter.Eq("GuesstimateId", guesstimateOld.GuesstimateId), guesstimateOld);

					guesstimate.VersionId = guesstimateOld.VersionId + 1;
					guesstimate.VersionName = VersionName;
					guesstimate.VersionDescription = VersionDescription;
				}
				guesstimate.IsCurrentVersion = true;
				guesstimate.CreateDate = DateTime.Now;
				guesstimate.EditUser = "";
				guesstimate.EditDate = null;
				guesstimate.IsDeleted = false;

				foreach (var objSumm in response.SummaryDetailsInfo)
				{
					foreach (var objPosition in objSumm.OriginalItineraryDetails.Where(a => a.IsDeleted == false))
					{
						var itemPos = new GuesstimatePosition();

						itemPos.GuesstimatePositionId = Guid.NewGuid().ToString();
						itemPos.PositionId = objPosition.PositionId;
						itemPos.ProductId = objPosition.ProductId;
						itemPos.DefaultSupplierId = objPosition.SupplierId;
						itemPos.DefaultSupplier = objPosition.Supplier;
						itemPos.ActiveSupplierId = objPosition.SupplierId;
						itemPos.ActiveSupplier = objPosition.Supplier;
						itemPos.Day = objSumm.Day;
						itemPos.PlaceOfService = objSumm.OriginalItineraryName;
						itemPos.OriginalItineraryDate = objSumm.OriginalItineraryDate;
						itemPos.OriginalItineraryDay = objSumm.OriginalItineraryDay;
						itemPos.ProductCategory = objPosition.ProductCategory;
						itemPos.ProductCategoryId = objPosition.ProductCategoryId;
						itemPos.KeepZero = false;
						itemPos.KeepAs = objPosition.KeepAs;
						itemPos.StartTime = objPosition.StartTime;
						itemPos.EndTime = objPosition.EndTime;
						itemPos.ProductType = objPosition.ProductType;
						itemPos.OriginalItineraryDescription = objPosition.OriginalItineraryDescription;
						itemPos.BuyCurrency = objPosition.BuyCurrency;
						itemPos.ProductTypeChargeBasis = objPosition.ProductTypeChargeBasis;
						itemPos.StandardPrice = objPosition.StandardPrice;

						foreach (var objPositionPrices in objPositionPricesList)
						{
							if (objPosition.PositionId == objPositionPrices.PositionId)
							{
								var item = new GuesstimatePrice();
								item.GuesstimatePriceId = Guid.NewGuid().ToString();
								item.PositionId = objPositionPrices.PositionId;
								item.PositionPriceId = objPositionPrices.PositionPriceId;
								item.DepartureId = objPositionPrices.DepartureId;
								item.Period = objPositionPrices.Period;
								item.PaxSlabId = objPositionPrices.PaxSlabId;
								item.PaxSlab = objPositionPrices.PaxSlab;
								item.Type = objPositionPrices.Type;
								item.RoomId = objPositionPrices.RoomId;
								item.IsSupplement = objPositionPrices.IsSupplement;
								item.SupplierId = objPositionPrices.SupplierId;
								item.Supplier = objPositionPrices.Supplier;
								item.ProductCategoryId = objPositionPrices.ProductCategoryId;
								item.ProductCategory = objPositionPrices.ProductCategory;
								item.ProductRangeId = objPositionPrices.ProductRangeId;
								item.ProductRange = objPositionPrices.ProductRange;
								item.ProductRangeCode = objPositionPrices.ProductRangeCode;
								item.ProductType = objPosition.ProductType;
								item.KeepAs = objPosition.KeepAs;
								item.BuyCurrencyId = objPositionPrices.BuyCurrencyId;
								item.BuyCurrency = objPositionPrices.BuyCurrency;
								item.ContractId = objPositionPrices.ContractId;
								item.ContractPrice = objPositionPrices.ContractPrice;
								item.BudgetPrice = objPositionPrices.BudgetPrice;
								item.BuyPrice = objPositionPrices.BuyPrice;
								item.MarkupAmount = objPositionPrices.MarkupAmount;
								item.BuyNetPrice = objPositionPrices.BuyNetPrice;
								item.SellCurrencyId = objPositionPrices.SellCurrencyId;
								item.SellCurrency = objPositionPrices.SellCurrency;
								item.SellNetPrice = objPositionPrices.SellNetPrice;
								item.TaxAmount = objPositionPrices.TaxAmount;
								item.SellPrice = objPositionPrices.SellPrice;
								item.ExchangeRateId = objPositionPrices.ExchangeRateId;
								item.ExchangeRatio = objPositionPrices.ExchangeRatio;

								item.CreateDate = objPositionPrices.CreateDate;
								item.CreateUser = objPositionPrices.CreateUser;
								item.EditUser = objPositionPrices.EditUser;
								item.EditDate = objPositionPrices.EditDate;
								item.IsDeleted = objPositionPrices.IsDeleted;

								itemPos.GuesstimatePrice.Add(item);
							}
						}
						guesstimate.GuesstimatePosition.Add(itemPos);
					}
				}

				await _MongoContext.mGuesstimate.InsertOneAsync(guesstimate);

			}
			return true;
		}

		/// <summary>
		/// This method get All QRF data from mQuote and clone it to mQRFPrice
		/// </summary>
		/// <param name="VersionName"></param>
		/// <param name="VersionDescription"></param>
		/// <param name="QRFID"></param>
		/// <param name="UserName"></param>
		/// <returns></returns>
		public async Task<string> SaveQRFPrice(string VersionName, string VersionDescription, string QRFID, string UserName = "")
		{
			try
			{
				var quote = _MongoContext.mQuote.AsQueryable().Where(a => a.QRFID == QRFID).FirstOrDefault();
				var position = _MongoContext.mQRFPosition.AsQueryable().Where(a => a.QRFID == QRFID).ToList();

				if (quote != null)
				{
					var qrfPrice = new mQRFPrice();

					qrfPrice.QRFPrice_Id = Guid.NewGuid().ToString();
					qrfPrice.QRFID = QRFID;

					if (VersionName == "Default")
					{
						qrfPrice.VersionId = 1;
						qrfPrice.SalesOfficer = quote.SalesPerson;
						qrfPrice.CostingOfficer = quote.CostingOfficer;
					}
					else
					{
						var QRFPriceOld = new mQRFPrice();
						if (VersionName == "CopyQuote")
						{
							QRFPriceOld = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == quote.Parent_QRFID && a.IsCurrentVersion == true).OrderByDescending(b => b.VersionId).FirstOrDefault();
							if (QRFPriceOld == null)
								QRFPriceOld = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == quote.Parent_QRFID).OrderByDescending(b => b.VersionId).FirstOrDefault();
						}
						else
						{
							QRFPriceOld = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == QRFID && a.IsCurrentVersion == true).OrderByDescending(b => b.VersionId).FirstOrDefault();
							if (QRFPriceOld == null)
								QRFPriceOld = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == QRFID).OrderByDescending(b => b.VersionId).FirstOrDefault();
						}

						if (QRFPriceOld != null)
						{
							qrfPrice.VersionId = QRFPriceOld.VersionId + 1;
							qrfPrice.SalesOfficer = QRFPriceOld.SalesOfficer;
							qrfPrice.CostingOfficer = QRFPriceOld.CostingOfficer;
							qrfPrice.ProductAccountant = QRFPriceOld.ProductAccountant;
							qrfPrice.FollowUp = QRFPriceOld.FollowUp;
						}
					}
					qrfPrice.VersionName = VersionName;
					qrfPrice.VersionDescription = VersionDescription;
					qrfPrice.IsCurrentVersion = true;
					qrfPrice.CreateUser = UserName;
					qrfPrice.QRFCurrency_Id = quote.AgentProductInfo.BudgetCurrencyID;
					qrfPrice.QRFCurrency = quote.AgentProductInfo.BudgetCurrency;
					qrfPrice.PercentSoldSupplement = 100;
					qrfPrice.ValidForTravel = quote.ValidForTravel;
					qrfPrice.ValidForAcceptance = quote.ValidForAcceptance;
					qrfPrice.FollowUpCostingOfficer = DateTime.Now;
					qrfPrice.FollowUpWithClient = DateTime.Now.AddDays(7);
					qrfPrice.SystemCompany_Id = quote.SystemCompany_Id;
					qrfPrice.RegenerateItinerary = quote.RegenerateItinerary;
					qrfPrice.Mappings = quote.Mappings;

					#region Set AgentInfo
					if (quote.AgentInfo != null)
					{
						qrfPrice.AgentInfo.AgentID = quote.AgentInfo.AgentID;
						qrfPrice.AgentInfo.AgentName = quote.AgentInfo.AgentName;
						qrfPrice.AgentInfo.ContactPerson = quote.AgentInfo.ContactPerson;
						qrfPrice.AgentInfo.ContactPersonID = quote.AgentInfo.ContactPersonID;
						qrfPrice.AgentInfo.EmailAddress = quote.AgentInfo.EmailAddress;
						qrfPrice.AgentInfo.MobileNo = quote.AgentInfo.MobileNo;

					}
					#endregion

					#region Set AgentProductInfo
					if (quote.AgentProductInfo != null)
					{
						qrfPrice.AgentProductInfo.Type = quote.AgentProductInfo.Type;
						qrfPrice.AgentProductInfo.TypeID = quote.AgentProductInfo.TypeID;
						qrfPrice.AgentProductInfo.Division = quote.AgentProductInfo.Division;
						qrfPrice.AgentProductInfo.DivisionID = quote.AgentProductInfo.DivisionID;
						qrfPrice.AgentProductInfo.Product = quote.AgentProductInfo.Product;
						qrfPrice.AgentProductInfo.ProductID = quote.AgentProductInfo.ProductID;
						qrfPrice.AgentProductInfo.PurposeofTravel = quote.AgentProductInfo.PurposeofTravel;
						qrfPrice.AgentProductInfo.PurposeofTravelID = quote.AgentProductInfo.PurposeofTravelID;
						qrfPrice.AgentProductInfo.Destination = quote.AgentProductInfo.Destination;
						qrfPrice.AgentProductInfo.DestinationID = quote.AgentProductInfo.DestinationID;
						qrfPrice.AgentProductInfo.Priority = quote.AgentProductInfo.Priority;
						qrfPrice.AgentProductInfo.TourName = quote.AgentProductInfo.TourName;
						qrfPrice.AgentProductInfo.TourCode = quote.AgentProductInfo.TourCode;
						qrfPrice.AgentProductInfo.Duration = quote.AgentProductInfo.Duration;
						qrfPrice.AgentProductInfo.BudgetCurrency = quote.AgentProductInfo.BudgetCurrency;
						qrfPrice.AgentProductInfo.BudgetCurrencyID = quote.AgentProductInfo.BudgetCurrencyID;
						qrfPrice.AgentProductInfo.BudgetCurrencyCode = quote.AgentProductInfo.BudgetCurrencyCode;
						qrfPrice.AgentProductInfo.BudgetAmount = quote.AgentProductInfo.BudgetAmount;
						qrfPrice.AgentProductInfo.CostingType = quote.AgentProductInfo.CostingType;
					}
					#endregion

					#region Set AgentPassangerInfo
					if (quote.AgentPassengerInfo != null && quote.AgentPassengerInfo.Count > 0)
					{
						foreach (var passInfo in quote.AgentPassengerInfo)
						{
							qrfPrice.AgentPassengerInfo.Add(new QRFAgentPassengerInfo
							{
								Type = passInfo.Type,
								count = passInfo.count,
								Age = passInfo.Age
							});
						}
					}
					#endregion

					#region Set AgentRoom
					if (quote.AgentRoom != null && quote.AgentRoom.Count > 0)
					{
						foreach (var room in quote.AgentRoom)
						{
							qrfPrice.QRFAgentRoom.Add(new QRFAgentRoom
							{
								RoomTypeID = room.RoomTypeID,
								RoomTypeName = room.RoomTypeName,
								RoomCount = room.RoomCount
							});
						}
					}
					#endregion

					#region Set Meals
					qrfPrice.Meals = quote.Meals;
					#endregion

					#region Set RoutingInfo
					if (quote.RoutingInfo != null && quote.RoutingInfo.Count > 0)
					{
						foreach (var passInfo in quote.RoutingInfo)
						{
							qrfPrice.RoutingInfo.Add(new RoutingInfo
							{
								CreateDate = passInfo.CreateDate,
								CreateUser = passInfo.CreateUser,
								Days = passInfo.Days,
								EditDate = passInfo.EditDate,
								EditUser = passInfo.EditUser,
								FromCityID = passInfo.FromCityID,
								FromCityName = passInfo.FromCityName,
								IsDeleted = passInfo.IsDeleted,
								IsLocalGuide = passInfo.IsLocalGuide,
								Nights = passInfo.Nights,
								RouteID = passInfo.RouteID,
								RouteSequence = passInfo.RouteSequence,
								ToCityID = passInfo.ToCityID,
								ToCityName = passInfo.ToCityName,
							});
						}
					}
					#endregion

					#region Set RoutingDays
					if (quote.RoutingDays != null && quote.RoutingDays.Count > 0)
					{
						foreach (var passInfo in quote.RoutingDays)
						{
							qrfPrice.RoutingDays.Add(new RoutingDays
							{
								CreateDate = passInfo.CreateDate,
								CreateUser = passInfo.CreateUser,
								Days = passInfo.Days,
								EditDate = passInfo.EditDate,
								EditUser = passInfo.EditUser,
								FromCityID = passInfo.FromCityID,
								FromCityName = passInfo.FromCityName,
								IsDeleted = passInfo.IsDeleted,
								DayNo = passInfo.DayNo,
								RoutingDaysID = passInfo.RoutingDaysID,
								RouteID = passInfo.RouteID,
								RouteSequence = passInfo.RouteSequence,
								ToCityID = passInfo.ToCityID,
								ToCityName = passInfo.ToCityName,
								GridLabel = passInfo.GridLabel,
								GridLabelIds = passInfo.GridLabelIds,
							});
						}
					}
					#endregion

					#region Set Departures

					if (quote.Departures != null && quote.Departures.Count > 0)
					{
						foreach (var dep in quote.Departures.Where(x => !x.IsDeleted))
						{
							qrfPrice.Departures.Add(new QRFDepartureDates
							{
								Departure_Id = dep.Departure_Id,
								Date = dep.Date,
								NoOfDep = dep.NoOfDep,
								PaxPerDep = dep.PaxPerDep,
								Warning = dep.Warning,
								IsDeleted = dep.IsDeleted,
								CreateUser = dep.CreateUser,
								CreateDate = dep.CreateDate,
								EditUser = dep.EditUser,
								EditDate = dep.EditDate
							});
						}
					}

					#endregion

					#region Set PaxSlab

					if (quote.PaxSlabDetails != null)
					{
						qrfPrice.PaxSlabDetails.HotelFlag = quote.PaxSlabDetails.HotelFlag;
						qrfPrice.PaxSlabDetails.CreateUser = quote.PaxSlabDetails.CreateUser;
						qrfPrice.PaxSlabDetails.CreateDate = quote.PaxSlabDetails.CreateDate;
						qrfPrice.PaxSlabDetails.EditUser = quote.PaxSlabDetails.EditUser;
						qrfPrice.PaxSlabDetails.EditDate = quote.PaxSlabDetails.EditDate;

						if (quote.PaxSlabDetails.PaxSlabs != null && quote.PaxSlabDetails.PaxSlabs.Count > 0)
						{
							foreach (var pax in quote.PaxSlabDetails.PaxSlabs.Where(x => !x.IsDeleted))
							{
								qrfPrice.PaxSlabDetails.QRFPaxSlabs.Add(new QRFPaxSlabs
								{
									PaxSlab_Id = pax.PaxSlab_Id,
									From = pax.From,
									To = pax.To,
									DivideByCost = pax.DivideByCost,
									Category = pax.Category,
									Category_Id = pax.Category_Id,
									CoachType = pax.CoachType,
									CoachType_Id = pax.CoachType_Id,
									Brand = pax.Brand,
									Brand_Id = pax.Brand_Id,
									HowMany = pax.HowMany,
									BudgetAmount = pax.BudgetAmount,
									IsDeleted = pax.IsDeleted,
									DeleteUser = pax.DeleteUser,
									DeleteDate = pax.DeleteDate,
									CreateDate = pax.CreateDate,
									EditDate = pax.EditDate
								});
							}
						}

						if (quote.PaxSlabDetails.HotelCategories != null && quote.PaxSlabDetails.HotelCategories.Count > 0)
						{
							foreach (var hotCat in quote.PaxSlabDetails.HotelCategories)
							{
								qrfPrice.PaxSlabDetails.HotelCategories.Add(new QRFHotelCategories
								{
									VoyagerDefProductCategoryId = hotCat.VoyagerDefProductCategoryId,
									Name = hotCat.Name
								});
							}
						}

						if (quote.PaxSlabDetails.HotelChain != null && quote.PaxSlabDetails.HotelChain.Count > 0)
						{
							foreach (var hotChain in quote.PaxSlabDetails.HotelChain)
							{
								qrfPrice.PaxSlabDetails.HotelChain.Add(new QRFHotelChain
								{
									AttributeId = hotChain.AttributeId,
									Name = hotChain.Name
								});
							}
						}
					}

					#endregion

					#region Set FOC
					if (quote.FOCDetails != null && quote.FOCDetails.Count > 0)
					{
						foreach (var foc in quote.FOCDetails)
						{
							qrfPrice.QRFSalesFOC.Add(new QRFFOCDetails
							{
								FOC_Id = foc.FOC_Id,
								DateRangeId = foc.DateRangeId,
								DateRange = foc.DateRange,
								PaxSlab = foc.PaxSlab,
								PaxSlabId = foc.PaxSlabId,
								FOCSingle = foc.FOCSingle,
								FOCTwin = foc.FOCTwin,
								FOCDouble = foc.FOCDouble,
								FOCTriple = foc.FOCTriple,
								IsDeleted = foc.IsDeleted,
								CreateUser = foc.CreateUser,
								CreateDate = foc.CreateDate,
								EditUser = foc.EditUser,
								EditDate = foc.EditDate
							});
						}
					}
					#endregion

					#region Set Margins
					if (VersionName != "Default" && VersionName != "CopyQuote")
					{
						var savedMargins = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == QRFID && a.IsCurrentVersion == true).OrderByDescending(b => b.VersionId).Select(c => c.QRFMargin).FirstOrDefault();

						if (savedMargins != null)
						{
							qrfPrice.QRFMargin.CreateUser = savedMargins.CreateUser;
							qrfPrice.QRFMargin.CreateDate = savedMargins.CreateDate;
							qrfPrice.QRFMargin.EditUser = UserName;
							qrfPrice.QRFMargin.EditDate = DateTime.Now;
							qrfPrice.QRFMargin.SelectedMargin = savedMargins.SelectedMargin;
							qrfPrice.QRFMargin.Package = savedMargins.Package;
							qrfPrice.QRFMargin.Product = savedMargins.Product;
							qrfPrice.QRFMargin.Item = savedMargins.Item;
						}
					}
					else
					{
						if (quote.Margins != null)
						{
							qrfPrice.QRFMargin.CreateUser = quote.Margins.CreateUser;
							qrfPrice.QRFMargin.CreateDate = quote.Margins.CreateDate;
							qrfPrice.QRFMargin.SelectedMargin = quote.Margins.SelectedMargin;

							if (quote.Margins.Package != null)
							{
								if (quote.Margins.Package.PackageProperties != null)
								{
									foreach (var pp in quote.Margins.Package.PackageProperties)
									{
										qrfPrice.QRFMargin.Package.PackageProperties.Add(new QRFMarginPackageProperties
										{
											PackageID = pp.PackageID,
											ComponentName = pp.ComponentName,
											SellingPrice = pp.SellingPrice,
											MarginUnit = pp.MarginUnit
										});
									}
								}

								if (quote.Margins.Package.MarginComputed != null)
								{
									qrfPrice.QRFMargin.Package.MarginComputed.TotalCost = quote.Margins.Package.MarginComputed.TotalCost;
									qrfPrice.QRFMargin.Package.MarginComputed.TotalLeadersCost = quote.Margins.Package.MarginComputed.TotalLeadersCost;
									qrfPrice.QRFMargin.Package.MarginComputed.Upgrade = quote.Margins.Package.MarginComputed.Upgrade;
									qrfPrice.QRFMargin.Package.MarginComputed.MarkupType = quote.Margins.Package.MarginComputed.MarkupType;
								}
							}

							if (quote.Margins.Product != null)
							{
								if (quote.Margins.Product.ProductProperties != null)
								{
									foreach (var pp in quote.Margins.Product.ProductProperties)
									{
										qrfPrice.QRFMargin.Product.ProductProperties.Add(new QRfMarginProductProperties
										{
											ProductID = pp.ProductID,
											VoyagerProductType_Id = pp.VoyagerProductType_Id,
											Prodtype = pp.Prodtype,
											SellingPrice = pp.SellingPrice,
											MarginUnit = pp.MarginUnit,
											HowMany = pp.HowMany
										});
									}
								}

								if (quote.Margins.Product.MarginComputed != null)
								{
									qrfPrice.QRFMargin.Product.MarginComputed.TotalCost = quote.Margins.Product.MarginComputed.TotalCost;
									qrfPrice.QRFMargin.Product.MarginComputed.TotalLeadersCost = quote.Margins.Product.MarginComputed.TotalLeadersCost;
									qrfPrice.QRFMargin.Product.MarginComputed.Upgrade = quote.Margins.Product.MarginComputed.Upgrade;
									qrfPrice.QRFMargin.Product.MarginComputed.MarkupType = quote.Margins.Product.MarginComputed.MarkupType;
								}
							}

							if (quote.Margins.SelectedMargin == "ServiceItem")
							{
								if (quote.Margins.Itemwise != null)
								{
									if (quote.Margins.Itemwise.ItemProperties != null)
									{
										foreach (var pp in quote.Margins.Itemwise.ItemProperties)
										{
											qrfPrice.QRFMargin.Item.ItemProperties.Add(new QRfMarginItemProperties
											{
												ItemID = pp.ItemID,
												PositionID = pp.PositionID,
												ProductName = pp.ProductName,
												VoyagerProductType_Id = pp.VoyagerProductType_Id,
												Prodtype = pp.Prodtype,
												SellingPrice = pp.SellingPrice,
												MarginUnit = pp.MarginUnit,
												HowMany = pp.HowMany
											});
										}
									}

									if (quote.Margins.Itemwise.MarginComputed != null)
									{
										qrfPrice.QRFMargin.Item.MarginComputed.TotalCost = quote.Margins.Product.MarginComputed.TotalCost;
										qrfPrice.QRFMargin.Item.MarginComputed.TotalLeadersCost = quote.Margins.Product.MarginComputed.TotalLeadersCost;
										qrfPrice.QRFMargin.Item.MarginComputed.Upgrade = quote.Margins.Product.MarginComputed.Upgrade;
										qrfPrice.QRFMargin.Item.MarginComputed.MarkupType = quote.Margins.Product.MarginComputed.MarkupType;
									}
								}
							}
							else
							{
								foreach (var posM in position)
								{
									qrfPrice.QRFMargin.Item.ItemProperties.Add(new QRfMarginItemProperties
									{
										ItemID = Guid.NewGuid().ToString(),
										PositionID = posM.PositionId,
										ProductName = posM.ProductName,
										VoyagerProductType_Id = posM.ProductTypeId,
										Prodtype = posM.ProductType,
										SellingPrice = 0,
										MarginUnit = "",
										HowMany = "",
										KeepAs = posM.KeepAs
									});
								}
								if (quote.Margins.SelectedMargin == "Package")
								{
									if (quote.Margins.Package != null)
									{
										if (quote.Margins.Package.PackageProperties != null)
										{
											var marPackInc = quote.Margins.Package.PackageProperties.Where(a => a.ComponentName == "Package not including Accommodation"
														 || a.ComponentName == "Package including Accommodation").FirstOrDefault();
											var marPackSup = quote.Margins.Package.PackageProperties.Where(a => a.ComponentName == "Suppliments").FirstOrDefault();
											var marPackOpt = quote.Margins.Package.PackageProperties.Where(a => a.ComponentName == "Optionals").FirstOrDefault();

											marPackInc = marPackInc == null ? new PackageProperties() : marPackInc;
											marPackSup = marPackSup == null ? new PackageProperties() : marPackSup;
											marPackOpt = marPackOpt == null ? new PackageProperties() : marPackOpt;

											foreach (var itemM in qrfPrice.QRFMargin.Item.ItemProperties)
											{
												if (itemM.SellingPrice == 0)
												{
													if (itemM.KeepAs == "Included")
													{
														itemM.SellingPrice = marPackInc.SellingPrice;
														itemM.MarginUnit = marPackInc.MarginUnit;
													}
													else if (itemM.KeepAs == "Supplement")
													{
														itemM.SellingPrice = marPackSup.SellingPrice;
														itemM.MarginUnit = marPackSup.MarginUnit;
													}
													else if (itemM.KeepAs == "Optional")
													{
														itemM.SellingPrice = marPackOpt.SellingPrice;
														itemM.MarginUnit = marPackOpt.MarginUnit;
													}
												}
											}
										}
									}
								}
								else if (quote.Margins.SelectedMargin == "Product")
								{
									if (quote.Margins.Product != null)
									{
										if (quote.Margins.Product.ProductProperties != null)
										{
											foreach (var itemM in qrfPrice.QRFMargin.Item.ItemProperties)
											{
												if (itemM.SellingPrice == 0)
												{
													itemM.SellingPrice = quote.Margins.Product.ProductProperties.Where(a => a.VoyagerProductType_Id == itemM.VoyagerProductType_Id).Select(b => b.SellingPrice).FirstOrDefault();
													itemM.MarginUnit = quote.Margins.Product.ProductProperties.Where(a => a.VoyagerProductType_Id == itemM.VoyagerProductType_Id).Select(b => b.MarginUnit).FirstOrDefault();

												}
											}
										}
									}
								}
							}
						}
					}
					#endregion

					#region Follow Up
					if (VersionName == "Default")
					{
						qrfPrice.FollowUp = quote.FollowUp;
					}
					#endregion

					#region Set ExchangeRates

					qrfPrice.ExchangeRateSnapshot = quote.ExchangeRateSnapshot;

					var FromCurrencyId = quote.AgentProductInfo.BudgetCurrencyID;
					var FromCurrency = quote.AgentProductInfo.BudgetCurrencyCode;

					var posn = _MongoContext.mPosition.AsQueryable().Where(a => a.QRFID == QRFID).ToList();

					List<string> posCurrencyIdList = new List<string>();

					foreach (var pn in posn)
					{
						var ToCurrencyId = pn.BuyCurrencyId;
						var ToCurrency = pn.BuyCurrency;

						if (!(string.IsNullOrEmpty(FromCurrencyId) || string.IsNullOrEmpty(ToCurrencyId)))
						{
							if (!(posCurrencyIdList.Contains(ToCurrencyId)))
							{
								var rateDetail = _genericRepository.getExchangeRate(FromCurrencyId, ToCurrencyId, QRFID);

								if (!(string.IsNullOrEmpty(rateDetail.AttributeValue_Id)))
								{
									qrfPrice.QRFExchangeRates.Add(new QRFExchangeRates
									{
										QRFExchangeRatesID = Guid.NewGuid().ToString(),
										QRFID = QRFID,
										ExchamgeRateDetailID = rateDetail.AttributeValue_Id,
										AsOnDate = quote.CreateDate,
										FromCurrencyId = FromCurrencyId,
										FromCurrency = FromCurrency,
										ToCurrencyId = ToCurrencyId,
										ToCurrency = ToCurrency,
										ExchangeRate = Convert.ToDouble(rateDetail.Value),
										CreateUser = "Default",
										CreateDate = DateTime.Now
									});
								}
								posCurrencyIdList.Add(ToCurrencyId);
							}
						}
					}

					#endregion

					#region Set Guesstimate

					var guesstimate = _MongoContext.mGuesstimate.AsQueryable().Where(a => a.QRFID == QRFID && a.IsCurrentVersion == true).OrderByDescending(b => b.VersionId).FirstOrDefault();

					qrfPrice.Guesstimate = guesstimate;

					#endregion

					await _MongoContext.mQRFPrice.InsertOneAsync(qrfPrice);

					#region Set Itinerary
					if (VersionName != "Default")
					{
						var Itinerary = _MongoContext.mItinerary.AsQueryable().Where(a => a.QRFID == QRFID).FirstOrDefault();

						foreach (var Idays in Itinerary.ItineraryDays)
						{
							foreach (var Idesc in Idays.ItineraryDescription)
							{
								var KeepAs = guesstimate.GuesstimatePosition.Where(a => a.PositionId == Idesc.PositionId).Select(b => b.KeepAs).FirstOrDefault();
								if (KeepAs != null)
									Idesc.KeepAs = KeepAs;
							}
						}
						ReplaceOneResult replaceResult = await _MongoContext.mItinerary.ReplaceOneAsync(Builders<mItinerary>.Filter.Eq("ItineraryID", Itinerary.ItineraryID), Itinerary);
					}
					#endregion

					if (VersionName != "Default")
					{
						List<mQRFPrice> qrfPriceList;
						qrfPriceList = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == QRFID && a.QRFPrice_Id != qrfPrice.QRFPrice_Id && a.IsCurrentVersion == true).ToList();

						foreach (var objqrfPrice in qrfPriceList)
						{
							objqrfPrice.IsCurrentVersion = false;
							objqrfPrice.EditUser = UserName;
							objqrfPrice.EditDate = DateTime.Now;
							ReplaceOneResult replaceResult = await _MongoContext.mQRFPrice.ReplaceOneAsync(Builders<mQRFPrice>.Filter.Eq("QRFPrice_Id", objqrfPrice.QRFPrice_Id), objqrfPrice);
						}
					}

					bool flag = await SaveQRFCost(qrfPrice.QRFPrice_Id, QRFID);

					if (flag) return qrfPrice.QRFPrice_Id; else return null;
				}
				return null;
			}
			catch (Exception e)
			{
				return null;
			}
		}

		/// <summary>
		/// This method save all Costing related data
		/// </summary>
		/// <param name="QRFPriceId"></param>
		/// <param name="QRFID"></param>
		/// <returns></returns>
		public async Task<bool> SaveQRFCost(string QRFPriceId, string QRFID)
		{
			try
			{
				//In this method we save data in 4 collections mQRFPositionTotalCost, mQRFPositionPrice, mQRFPackagePrice, mQRFNonPackagedPrice for Costsheet
				//We take all position data of Quote and save it in mQRFPositionTotalCost. There will be one record for each position in this collection and each position will
				//contain total cost for that perticular position for its all rooms(range).
				//We take all position prices data from Guesstimate in save it in mQRFPositionPrice. This will contain one record for each position room(range).
				#region save mQRFPositionTotalCost, mQRFPositionPrice

				//In the following we get data from mQRFPrice(quote data), position and guesstimate
				var quote = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFPrice_Id == QRFPriceId).FirstOrDefault();

				var position = _MongoContext.mQRFPosition.AsQueryable().Where(a => a.QRFID == QRFID).ToList();

				var guesstimate = _MongoContext.mGuesstimate.AsQueryable().Where(a => a.QRFID == QRFID && a.IsCurrentVersion).OrderByDescending(b => b.VersionId).FirstOrDefault();

				//We take margin data from QRFMargin.Item, as margin is calculated for each position seperately
				var margin = quote.QRFMargin.Item;
				var exchangeRateList = quote.QRFExchangeRates;
				var currencyList = _MongoContext.mCurrency.AsQueryable();
				//var ExchangeRateId = _MongoContext.mExchangeRate.AsQueryable().Where(a => a.DateMin <= DateTime.Now && DateTime.Now <= a.DateMax).Select(b => b.ExchangeRateId).FirstOrDefault();
				//var ExchangeRateDetailList = _MongoContext.mExchangeRateDetail.AsQueryable().Where(a => a.ExchangeRate_Id == ExchangeRateId).ToList();

				var BaseCurrency = quote.ExchangeRateSnapshot;
				var ExchangeRateDetailList = BaseCurrency?.ExchangeRateDetail;
				var ExchangeRateId = BaseCurrency?.ExchangeRate_id;

				if (ExchangeRateDetailList == null || ExchangeRateDetailList?.Count == 0)
				{
					BaseCurrency = _MongoContext.mExchangeRate.AsQueryable().Where(a => a.DateMin <= DateTime.Now && DateTime.Now <= a.DateMax).Select(a => new ExchangeRateSnapshot
					{
						Currency_Id = a.Currency_Id,
						REFCUR = a.RefCur,
						ExchangeRate_id = a.ExchangeRateId,
						DATEMAX = a.DateMax,
						DATEMIN = a.DateMin,
						EXRATE = a.ExRate,
						VATRATE = a.VatRate,
						CREA_DT = a.CreateDate
					}).FirstOrDefault();
					ExchangeRateId = BaseCurrency.ExchangeRate_id;

					ExchangeRateDetailList = _MongoContext.mExchangeRateDetail.AsQueryable().Where(a => a.ExchangeRate_Id == BaseCurrency.ExchangeRate_id)
						.Select(a => new ExchangeRateDetailSnapshot
						{
							Currency_Id = a.Currency_Id,
							CURRENCY = a.CURRENCY,
							RATE = a.RATE,
							ROUNDTO = a.ROUNDTO
						}).ToList();
				}

				var selMarginType = (quote.QRFMargin.SelectedMargin ?? "Package");
				var pckMargin = quote.QRFMargin.Package;
				double pckMarkup = 0.0;
				double pckMarkupPerPosition = 0.0;
				double pckExchangeRate = 1.00;
				double pckExchangeRateFrom = 1.00;
				double pckExchangeRateTo = 1.00;
				string pckMarkupUnit = (pckMargin.PackageProperties.FirstOrDefault()?.MarginUnit ?? "%");
				string pckMarkupUnitId = "";

				//If margin is PACKAGE and fixed value then it should be divided across all position
				if (selMarginType.ToUpper() == "PACKAGE" && pckMarkupUnit != "%")
				{
					pckMarkup = Convert.ToDouble(pckMargin.PackageProperties.FirstOrDefault().SellingPrice);
					pckMarkupPerPosition = pckMarkup / (position.Count == 0 ? 1 : position.Count);
					pckExchangeRate = exchangeRateList.Where(a => a.FromCurrency == quote.AgentProductInfo.BudgetCurrencyCode && a.ToCurrency == pckMarkupUnit).Select(b => b.ExchangeRate).FirstOrDefault();

					if (pckExchangeRate == 0)
					{
						pckMarkupUnitId = currencyList.Where(a => a.Currency == pckMarkupUnit).Select(a => a.VoyagerCurrency_Id).FirstOrDefault();
						/* if (quote.AgentProductInfo.BudgetCurrencyID != pckMarkupUnitId)
                         { 
                             AttributeValues curAttr = _genericRepository.getExchangeRate(quote.AgentProductInfo.BudgetCurrencyID, pckMarkupUnitId, QRFID);
                             pckExchangeRate = Convert.ToDouble(curAttr.Value ?? "1.00");
                         }
                         */
						pckExchangeRateFrom = Convert.ToDouble(ExchangeRateDetailList.Where(a => a.Currency_Id == quote.AgentProductInfo.BudgetCurrencyID).Select(a => a.RATE).FirstOrDefault());
						if (pckExchangeRateFrom == 0.00)
							pckExchangeRateFrom = 1.00;
						pckExchangeRateTo = Convert.ToDouble(ExchangeRateDetailList.Where(a => a.Currency_Id == pckMarkupUnitId).Select(a => a.RATE).FirstOrDefault());
						if (pckExchangeRateTo == 0.00)
							pckExchangeRateTo = 1.00;
						pckExchangeRate = pckExchangeRateTo / pckExchangeRateFrom;
					}
				}

				var qrfSalesFOCList = quote.QRFSalesFOC;

				//List<CommsPositions> lstCommsPositions = new List<CommsPositions>();

				//Now Cost is calculated per departure, per pax, per position. So if there is 2 departure, 3 pax and 10 position then there would be 60 records in mQRFPositionTotalCost
				for (int i = 0; i < quote.Departures.Count; i++)
				{
					for (int j = 0; j < quote.PaxSlabDetails.QRFPaxSlabs.Count; j++)
					{
						for (int p = 0; p < position.Count; p++)
						{
							//Here we take prices from guesstimate
							var guessPos = guesstimate.GuesstimatePosition.Where(a => a.PositionId == position[p].PositionId).FirstOrDefault();

							//We do not calculate prices for deleted Departure, Pax and Position
							if ((!(position[p].IsDeleted))
								&& (!(quote.Departures[i].IsDeleted))
								&& (!(quote.PaxSlabDetails.QRFPaxSlabs[j].IsDeleted))
								&& guessPos != null
							   )
							{
								double markup = margin.ItemProperties.Where(a => a.PositionID == position[p].PositionId).Select(b => b.SellingPrice).FirstOrDefault();
								string markupUnit = margin.ItemProperties.Where(a => a.PositionID == position[p].PositionId).Select(b => b.MarginUnit).FirstOrDefault();
								string markupUnitId = "";
								double itmExchangeRate = 1.00;
								double itmExchangeRateFrom = 1.00;
								double itmExchangeRateTo = 1.00;

								//If margin is non-percentage and non-package then following code convert margin in base currency
								if (markupUnit != "%" && selMarginType.ToUpper() != "PACKAGE")
								{
									markupUnitId = currencyList.Where(a => a.Currency == markupUnit).Select(a => a.VoyagerCurrency_Id).FirstOrDefault();
									/*if (quote.AgentProductInfo.BudgetCurrencyID != markupUnitId)
                                    {
                                        AttributeValues itmAttr = _genericRepository.getExchangeRate(quote.AgentProductInfo.BudgetCurrencyID, markupUnitId, QRFID);
                                        markup = markup / Convert.ToDouble(itmAttr.Value ?? "1.00");
                                    }*/
									itmExchangeRateFrom = Convert.ToDouble(ExchangeRateDetailList.Where(a => a.Currency_Id == quote.AgentProductInfo.BudgetCurrencyID).Select(a => a.RATE).FirstOrDefault());
									itmExchangeRateTo = Convert.ToDouble(ExchangeRateDetailList.Where(a => a.Currency_Id == markupUnitId).Select(a => a.RATE).FirstOrDefault());

									if (itmExchangeRateFrom == 0.00)
										itmExchangeRateFrom = 1.00;

									if (itmExchangeRateTo == 0.00)
										itmExchangeRateTo = 1.00;
									itmExchangeRate = itmExchangeRateTo / itmExchangeRateFrom;
									markup = markup / itmExchangeRate;

								}
								double exchangeRate = exchangeRateList.Where(a => a.FromCurrency == quote.AgentProductInfo.BudgetCurrencyCode && a.ToCurrency == guessPos.BuyCurrency).Select(b => b.ExchangeRate).FirstOrDefault();

								if (exchangeRate == 0) exchangeRate = 1;

								var objTotalCost = new mQRFPositionTotalCost();

								#region FOC Calculation
								var qrfSalesFOC = qrfSalesFOCList.Where(a => a.DateRangeId == quote.Departures[i].Departure_Id && a.PaxSlabId == quote.PaxSlabDetails.QRFPaxSlabs[j].PaxSlab_Id).FirstOrDefault();
								double focCount = 0;
								double focValue = 0;
								double priceForFOCCalc = 0;
								//We calculate FOC for Included position only...Following Calculation is for Hotels only, for other Position FOC calculation is different
								if (qrfSalesFOC != null && position[p].KeepAs.ToString().ToLower() == "included")
								{
									//Sums the total FOC count
									focCount = qrfSalesFOC.FOCSingle + qrfSalesFOC.FOCDouble + qrfSalesFOC.FOCTriple + qrfSalesFOC.FOCTwin;
									if (position[p].ProductType.ToUpper() == "HOTEL")
									{
										var guessPosPriceForFOC = guessPos.GuesstimatePrice.Where(a => a.DepartureId == quote.Departures[i].Departure_Id
															   && a.PaxSlabId == quote.PaxSlabDetails.QRFPaxSlabs[j].PaxSlab_Id && a.SupplierId == guessPos.ActiveSupplierId
															   && a.IsDeleted == false).ToList();

										if (guessPosPriceForFOC != null)
										{
											if (qrfSalesFOC.FOCSingle > 0)
											{
												foreach (var range in position[p].RoomDetailsInfo)
												{
													priceForFOCCalc = 0;
													if ((!range.IsDeleted) && range.ProductRange == "SINGLE (ADULT)")
													{
														var guessPosPriceSin = guessPosPriceForFOC.Where(a => a.RoomId == range.RoomId).FirstOrDefault();

														priceForFOCCalc = (guessPosPriceSin.BudgetPrice * Convert.ToDouble(position[p].Duration)) / exchangeRate;

														focValue = focValue + (priceForFOCCalc / quote.PaxSlabDetails.QRFPaxSlabs[j].DivideByCost * qrfSalesFOC.FOCSingle);
													}
												}
											}
											if (qrfSalesFOC.FOCDouble > 0)
											{
												foreach (var range in position[p].RoomDetailsInfo)
												{
													priceForFOCCalc = 0;
													if ((!range.IsDeleted) && range.ProductRange == "DOUBLE (ADULT)")
													{
														var guessPosPriceSin = guessPosPriceForFOC.Where(a => a.RoomId == range.RoomId).FirstOrDefault();

														priceForFOCCalc = (guessPosPriceSin.BudgetPrice * Convert.ToDouble(position[p].Duration)) / exchangeRate;

														focValue = focValue + (priceForFOCCalc / quote.PaxSlabDetails.QRFPaxSlabs[j].DivideByCost * qrfSalesFOC.FOCDouble);
													}
												}
											}
											if (qrfSalesFOC.FOCTriple > 0)
											{
												foreach (var range in position[p].RoomDetailsInfo)
												{
													priceForFOCCalc = 0;
													if ((!range.IsDeleted) && range.ProductRange == "TRIPLE (ADULT)")
													{
														var guessPosPriceSin = guessPosPriceForFOC.Where(a => a.RoomId == range.RoomId).FirstOrDefault();

														priceForFOCCalc = (guessPosPriceSin.BudgetPrice * Convert.ToDouble(position[p].Duration)) / exchangeRate;

														focValue = focValue + (priceForFOCCalc / quote.PaxSlabDetails.QRFPaxSlabs[j].DivideByCost * qrfSalesFOC.FOCTriple);
													}
												}
											}
											if (qrfSalesFOC.FOCTwin > 0)
											{
												foreach (var range in position[p].RoomDetailsInfo)
												{
													priceForFOCCalc = 0;
													if ((!range.IsDeleted) && range.ProductRange == "TWIN (ADULT)")
													{
														var guessPosPriceSin = guessPosPriceForFOC.Where(a => a.RoomId == range.RoomId).FirstOrDefault();

														priceForFOCCalc = (guessPosPriceSin.BudgetPrice * Convert.ToDouble(position[p].Duration)) / exchangeRate;

														focValue = focValue + (priceForFOCCalc / quote.PaxSlabDetails.QRFPaxSlabs[j].DivideByCost * qrfSalesFOC.FOCTwin);
													}
												}
											}
										}
									}
								}
								#endregion

								objTotalCost.QRFCostForPositionID = Guid.NewGuid().ToString();
								objTotalCost.QRFID = QRFID;
								objTotalCost.QRFPrice_Id = QRFPriceId;
								objTotalCost.PositionId = position[p].PositionId;
								objTotalCost.ProductId = position[p].ProductID;
								objTotalCost.ProductName = position[p].ProductName;
								objTotalCost.PositionType = position[p].ProductType;
								objTotalCost.PositionKeepAs = guessPos.KeepAs;

								objTotalCost.Departure_Id = quote.Departures[i].Departure_Id;
								objTotalCost.DepartureDate = quote.Departures[i].Date;
								objTotalCost.PaxSlab_Id = quote.PaxSlabDetails.QRFPaxSlabs[j].PaxSlab_Id;
								objTotalCost.PaxSlab = quote.PaxSlabDetails.QRFPaxSlabs[j].From + "-" + quote.PaxSlabDetails.QRFPaxSlabs[j].To;

								objTotalCost.BuyCurrencyId = guessPos.GuesstimatePrice.Where(a => a.SupplierId == guessPos.ActiveSupplierId).FirstOrDefault()?.BuyCurrencyId;
								objTotalCost.BuyCurrency = guessPos.BuyCurrency;
								objTotalCost.QRFCurrency_Id = quote.AgentProductInfo.BudgetCurrencyID;
								objTotalCost.QRFCurrency = quote.AgentProductInfo.BudgetCurrency;

								objTotalCost.TotalBuyPrice = 0;
								objTotalCost.TotalSellPrice = 0;
								objTotalCost.ProfitAmount = 0;
								objTotalCost.ProfitPercentage = 0;

								objTotalCost.Status = null;
								objTotalCost.Create_Date = DateTime.Now;
								objTotalCost.Create_User = "Default";

								//For each Range, prices are calculated seperately
								foreach (var range in position[p].RoomDetailsInfo)
								{
									if (!(range.IsDeleted))
									{
										//Get the price from Guesstimate for each range of Active Supplier and current position
										var guessPosPrice = guessPos.GuesstimatePrice.Where(a => a.DepartureId == quote.Departures[i].Departure_Id
														   && a.PaxSlabId == quote.PaxSlabDetails.QRFPaxSlabs[j].PaxSlab_Id
														   && a.RoomId == range.RoomId && a.SupplierId == guessPos.ActiveSupplierId).FirstOrDefault();

										if (guessPosPrice != null)
										{
											var objPosPrice = new mQRFPositionPrice();

											double TotalBuyPriceWithExchRate = 0;

											objPosPrice.QRFPositionPriceID = Guid.NewGuid().ToString();
											objPosPrice.QRFID = QRFID;
											objPosPrice.QRFPrice_Id = QRFPriceId;
											objPosPrice.QRFCostForPositionID = objTotalCost.QRFCostForPositionID;
											objPosPrice.PositionId = position[p].PositionId;
											objPosPrice.ProductRange_Id = range.ProductRangeId;
											objPosPrice.ProductRange = range.ProductRange;
											objPosPrice.PersoneType = guessPosPrice.Type;
											objPosPrice.Age = 0;
											objPosPrice.IsAdditional = range.IsSupplement;
											objPosPrice.BuyCurrency = guessPos.BuyCurrency;
											//Budget Price is always = Price entered by user * duration(no. of nights)
											objPosPrice.TotalBuyPrice = guessPosPrice.BudgetPrice * Convert.ToDouble(position[p].Duration);
											TotalBuyPriceWithExchRate = objPosPrice.TotalBuyPrice / exchangeRate;

											//If position is Hotel then FOC is added as value calculated above in FOC section else FOC = TotalBuyPriceWithExchRate/Pax * focCount
											objPosPrice.FOCInBuyCurrency = 0;
											if (position[p].ProductType.ToUpper() == "HOTEL")
											{
												if (objPosPrice.IsAdditional == true)
												{
													if (objPosPrice.PersoneType == "ADULT")
													{
														objPosPrice.FOCInBuyCurrency = (TotalBuyPriceWithExchRate / quote.PaxSlabDetails.QRFPaxSlabs[j].DivideByCost) * focCount;
													}
												}
												else
												{
													if (objPosPrice.PersoneType == "ADULT")
													{
														objPosPrice.FOCInBuyCurrency = focValue;
													}
												}
											}
											else
											{
												if (objPosPrice.PersoneType == "ADULT")
												{
													objPosPrice.FOCInBuyCurrency = (TotalBuyPriceWithExchRate / quote.PaxSlabDetails.QRFPaxSlabs[j].DivideByCost) * focCount;
												}
											}

											//Margin is calculated after adding FOC value to TotalBuyPriceWithExchRate
											if (markupUnit == "%")
												objPosPrice.TotalMarkup = (TotalBuyPriceWithExchRate + objPosPrice.FOCInBuyCurrency) * markup / 100;
											else
											{
												if (selMarginType.ToUpper() != "PACKAGE")
													objPosPrice.TotalMarkup = markup;
												else
													objPosPrice.TotalMarkup = pckMarkupPerPosition / pckExchangeRate;
											}

											objPosPrice.QRFCurrency = quote.AgentProductInfo.BudgetCurrency;
											//TotalSellPrice = TotalBuyPriceWithExchRate + TotalMarkup + FOCInBuyCurrency
											objPosPrice.TotalSellPrice = TotalBuyPriceWithExchRate + objPosPrice.TotalMarkup + objPosPrice.FOCInBuyCurrency;
											objPosPrice.TotalSellPriceInBuyCurrency = objPosPrice.TotalSellPrice * exchangeRate;

											objPosPrice.Status = null;
											objPosPrice.Create_Date = DateTime.Now;
											objPosPrice.Create_User = "Default";

											//For current Range, price is Added in mQRFPositionPrice 
											await _MongoContext.mQRFPositionPrice.InsertOneAsync(objPosPrice);

											objTotalCost.TotalBuyPrice = objTotalCost.TotalBuyPrice + objPosPrice.TotalBuyPrice;
											objTotalCost.FOCInBuyCurrency = objTotalCost.FOCInBuyCurrency + objPosPrice.FOCInBuyCurrency;
											objTotalCost.TotalSellPrice = objTotalCost.TotalSellPrice + objPosPrice.TotalSellPrice;
										}
									}
								}

								//Add FOC value if Room is missing to equal SINGLE SUPP across all pax slab
								if (position[p].ProductType.ToUpper() == "HOTEL" && focValue > 0)
								{
									foreach (var room in quote.QRFAgentRoom)
									{
										var IsRoomExist = position[p].RoomDetailsInfo.FindAll(a => a.ProductRange.ToUpper().Replace("(ADULT)", "").Trim() == room.RoomTypeName.ToUpper().Trim() && a.IsSupplement == false).Count > 0 ? true : false;

										if (!IsRoomExist)
										{
											var objPosPrice = new mQRFPositionPrice();

											objPosPrice.QRFPositionPriceID = Guid.NewGuid().ToString();
											objPosPrice.QRFID = QRFID;
											objPosPrice.QRFPrice_Id = QRFPriceId;
											objPosPrice.QRFCostForPositionID = objTotalCost.QRFCostForPositionID;
											objPosPrice.PositionId = position[p].PositionId;
											objPosPrice.ProductRange_Id = "";
											objPosPrice.ProductRange = room.RoomTypeName.ToUpper();
											objPosPrice.PersoneType = "ADULT";
											objPosPrice.Age = 0;
											objPosPrice.IsAdditional = false;
											objPosPrice.BuyCurrency = guessPos.BuyCurrency;
											objPosPrice.TotalBuyPrice = 0;

											objPosPrice.FOCInBuyCurrency = focValue;
											if (markupUnit == "%")
												objPosPrice.TotalMarkup = (objPosPrice.FOCInBuyCurrency) * markup / 100;
											else
											{
												if (selMarginType.ToUpper() != "PACKAGE")
													objPosPrice.TotalMarkup = markup;
												else
													objPosPrice.TotalMarkup = pckMarkupPerPosition / pckExchangeRate;
											}

											objPosPrice.QRFCurrency = quote.AgentProductInfo.BudgetCurrency;
											objPosPrice.TotalSellPrice = 0 + objPosPrice.TotalMarkup + objPosPrice.FOCInBuyCurrency;
											objPosPrice.TotalSellPriceInBuyCurrency = objPosPrice.TotalSellPrice * exchangeRate;

											objPosPrice.Status = null;
											objPosPrice.Create_Date = DateTime.Now;
											objPosPrice.Create_User = "Default";

											await _MongoContext.mQRFPositionPrice.InsertOneAsync(objPosPrice);

											objTotalCost.TotalBuyPrice = objTotalCost.TotalBuyPrice + objPosPrice.TotalBuyPrice;
											objTotalCost.FOCInBuyCurrency = objTotalCost.FOCInBuyCurrency + objPosPrice.FOCInBuyCurrency;
											objTotalCost.TotalSellPrice = objTotalCost.TotalSellPrice + objPosPrice.TotalSellPrice;
										}
									}
								}

								objTotalCost.ProfitAmount = objTotalCost.TotalSellPrice - (objTotalCost.TotalBuyPrice / exchangeRate) - objTotalCost.FOCInBuyCurrency;

								if (objTotalCost.TotalSellPrice > 0)
								{
									if (objTotalCost.TotalSellPrice - objTotalCost.ProfitAmount > 0)
										objTotalCost.ProfitPercentage = objTotalCost.ProfitAmount * 100 / (objTotalCost.TotalSellPrice - objTotalCost.ProfitAmount);
									else
										objTotalCost.ProfitPercentage = 100;
								}
								//For current Positionm, price is Added in mQRFPositionPrice 
								await _MongoContext.mQRFPositionTotalCost.InsertOneAsync(objTotalCost);

								//#region breakup  
								//double rate = 0;
								//var FromCurrencyRate = ExchangeRateDetailList.Where(a => a.Currency_Id == BaseCurrency.Currency_Id).FirstOrDefault();
								//var ToCurrencyRateBuy = ExchangeRateDetailList.Where(a => a.Currency_Id == guessPos.BuyCurrencyId).FirstOrDefault();
								//var posTotalBuyPriceToCur = objTotalCost.TotalBuyPrice;
								//var posTotalSellPriceToCur = objTotalCost.TotalSellPrice;

								//if (!(FromCurrencyRate == null || ToCurrencyRateBuy == null))
								//{
								//    rate = Math.Round(Convert.ToDouble(ToCurrencyRateBuy.RATE / FromCurrencyRate.RATE), 4);

								//    if (rate > 0)
								//    {
								//        posTotalBuyPriceToCur = posTotalBuyPriceToCur / rate;
								//        guessPos.BuyCurrency = BaseCurrency.REFCUR;
								//    }
								//}
								//var ToCurrencyRateSell = ExchangeRateDetailList.Where(a => a.Currency_Id == quote.AgentProductInfo.BudgetCurrencyID).FirstOrDefault();
								//if (!(FromCurrencyRate == null || ToCurrencyRateSell == null))
								//{
								//    rate = Math.Round(Convert.ToDouble(ToCurrencyRateSell.RATE / FromCurrencyRate.RATE), 4);

								//    if (rate > 0)
								//    {
								//        posTotalSellPriceToCur = posTotalSellPriceToCur / rate;
								//        if (posTotalSellPriceToCur < 0)//for negative numbers
								//        {
								//            posTotalSellPriceToCur = Math.Floor(posTotalSellPriceToCur);
								//        }
								//        else if (posTotalSellPriceToCur > 0)//for positive numbers
								//        {
								//            posTotalSellPriceToCur = Math.Ceiling(posTotalSellPriceToCur);
								//        }
								//        //pos.ProfitAmount = pos.ProfitAmount / rate; 
								//    }
								//}

								//lstCommsPositions.Add(new CommsPositions()
								//{
								//    GridInfo = position[p].ProductName,
								//    PositionId = position[p].PositionId,
								//    PositionType = position[p].KeepAs,
								//    ProductType = position[p].ProductType,
								//    Calculation = new CommercialReport()
								//    {
								//        Purchasing = new CommsPurchasing()
								//        {
								//            Currency = guessPos.BuyCurrency,
								//            Budget = objTotalCost.TotalBuyPrice,
								//            Actual = objTotalCost.TotalBuyPrice
								//        },
								//        Profitability = new CommsProfitability()
								//        {
								//            Currency = BaseCurrency.REFCUR,
								//            Purchase = posTotalBuyPriceToCur,
								//            Sell = posTotalSellPriceToCur,
								//            Margin = posTotalSellPriceToCur - objTotalCost.TotalBuyPrice,
								//            ProfitPercentage = objTotalCost.ProfitPercentage
								//        }
								//    }
								//});
								//#endregion
							}
						}
					}
				}
				#endregion

				#region save mQRFPackagePrice

				//To calculate Package price we get data from mQRFPositionTotalCost, mQRFPositionPrice which we saved in previous section

				var qrfPositionTotalCost = _MongoContext.mQRFPositionTotalCost.AsQueryable().Where(a => a.QRFPrice_Id == QRFPriceId && a.PositionKeepAs == "Included").ToList();

				var qrfPositionPrice = _MongoContext.mQRFPositionPrice.AsQueryable().Where(a => a.QRFPrice_Id == QRFPriceId).ToList();

				var currFromExchangeRate = _MongoContext.mExchangeRate.AsQueryable().Where(a => a.DateMin <= DateTime.Now && DateTime.Now <= a.DateMax).FirstOrDefault();

				var prorangelist = qrfPositionPrice.Select(c => c.ProductRange_Id).ToList();
				List<mProductRange> prodRange = new List<mProductRange>();
				prodRange = _MongoContext.mProductRange.AsQueryable().Where(a => prorangelist.Contains(a.VoyagerProductRange_Id)).ToList();

				//In package price, we consider only those Positions which are Included
				//Package price is calculated for per Departure, per Pax and rooms we selected in Quote like SINGLE, DOUBLE, CHILD etc
				if (qrfPositionTotalCost != null)
				{
					if (qrfPositionTotalCost.Count > 0)
					{
						for (int i = 0; i < quote.Departures.Count; i++)
						{
							for (int j = 0; j < quote.PaxSlabDetails.QRFPaxSlabs.Count; j++)
							{
								if ((!(quote.Departures[i].IsDeleted)) && (!(quote.PaxSlabDetails.QRFPaxSlabs[j].IsDeleted)))
								{
									double buyPriceExcHotel = 0;
									double sellPriceExcHotel = 0;
									double buyPriceExcHotelCHILD = 0;
									double sellPriceExcHotelCHILD = 0;

									var totalCost = qrfPositionTotalCost.Where(a => a.PaxSlab_Id == quote.PaxSlabDetails.QRFPaxSlabs[j].PaxSlab_Id && a.Departure_Id == quote.Departures[i].Departure_Id).ToList();

									if (totalCost != null)
									{
										if (totalCost.Count > 0)
										{
											double exchangeRate = exchangeRateList.Where(a => a.FromCurrency == quote.AgentProductInfo.BudgetCurrencyCode && a.ToCurrency == currFromExchangeRate.RefCur).Select(b => b.ExchangeRate).FirstOrDefault();
											if (exchangeRate == 0) exchangeRate = 1;

											//To calculate Package price, we devide position into Hotel and Non-Hotel
											var totalCostExcHotel = totalCost.Where(a => a.PositionType != "Hotel").ToList();
											var totalCostIncHotel = totalCost.Where(a => a.PositionType == "Hotel").ToList();

											foreach (var tc in totalCostExcHotel)
											{
												var posPrice = qrfPositionPrice.Where(a => a.QRFCostForPositionID == tc.QRFCostForPositionID).ToList();

												foreach (var pp in posPrice)
												{
													if (pp.PersoneType == "UNIT" || pp.PersoneType == "DRIVER" || pp.PersoneType == "GUIDE")
													{
														buyPriceExcHotel = buyPriceExcHotel + (pp.TotalBuyPrice / quote.PaxSlabDetails.QRFPaxSlabs[j].DivideByCost);
														sellPriceExcHotel = sellPriceExcHotel + (pp.TotalSellPrice / quote.PaxSlabDetails.QRFPaxSlabs[j].DivideByCost);
													}
													else if (pp.PersoneType == "ADULT")
													{
														buyPriceExcHotel = buyPriceExcHotel + pp.TotalBuyPrice;
														sellPriceExcHotel = sellPriceExcHotel + pp.TotalSellPrice;
													}
												}
											}

											foreach (var tc in totalCostExcHotel)
											{
												var posPrice = qrfPositionPrice.Where(a => a.QRFCostForPositionID == tc.QRFCostForPositionID).ToList();

												foreach (var pp in posPrice)
												{
													if (pp.PersoneType == "CHILD")
													{
														buyPriceExcHotelCHILD = buyPriceExcHotelCHILD + pp.TotalBuyPrice;
														sellPriceExcHotelCHILD = sellPriceExcHotelCHILD + pp.TotalSellPrice;
													}
												}
											}

											foreach (var tc in totalCostIncHotel)
											{
												var posPrice = qrfPositionPrice.Where(a => a.QRFCostForPositionID == tc.QRFCostForPositionID).ToList();

												foreach (var pp in posPrice)
												{
													if (pp.IsAdditional == true)
													{
														if (pp.PersoneType == "UNIT" || pp.PersoneType == "DRIVER" || pp.PersoneType == "GUIDE")
														{
															buyPriceExcHotel = buyPriceExcHotel + (pp.TotalBuyPrice / quote.PaxSlabDetails.QRFPaxSlabs[j].DivideByCost);
															sellPriceExcHotel = sellPriceExcHotel + (pp.TotalSellPrice / quote.PaxSlabDetails.QRFPaxSlabs[j].DivideByCost);
														}
														else if (pp.PersoneType == "ADULT")
														{
															buyPriceExcHotel = buyPriceExcHotel + pp.TotalBuyPrice;
															sellPriceExcHotel = sellPriceExcHotel + pp.TotalSellPrice;
														}
													}
												}
											}

											if (quote.QRFAgentRoom.Count == 0)
											{
												quote.QRFAgentRoom.Add(new QRFAgentRoom() { RoomTypeName = "Single", RoomCount = 1 });
												quote.QRFAgentRoom.Add(new QRFAgentRoom() { RoomTypeName = "Double", RoomCount = 1 });
												quote.QRFAgentRoom.Add(new QRFAgentRoom() { RoomTypeName = "Twin", RoomCount = 1 });
												quote.QRFAgentRoom.Add(new QRFAgentRoom() { RoomTypeName = "Triple", RoomCount = 1 });
											}

											//For SINGLE, DOUBLE, TWIN......
											foreach (var room in quote.QRFAgentRoom)
											{
												var qrfPackagePrice = new mQRFPackagePrice();
												qrfPackagePrice.QRFPackagePriceId = Guid.NewGuid().ToString();
												qrfPackagePrice.QRFID = QRFID;
												qrfPackagePrice.QRFPrice_Id = QRFPriceId;
												qrfPackagePrice.Departure_Id = quote.Departures[i].Departure_Id;
												qrfPackagePrice.DepartureDate = quote.Departures[i].Date;
												qrfPackagePrice.PaxSlab_Id = totalCost[0].PaxSlab_Id;
												qrfPackagePrice.PaxSlab = totalCost[0].PaxSlab;
												qrfPackagePrice.BuyCurrencyId = currFromExchangeRate.Currency_Id;
												qrfPackagePrice.BuyCurrency = currFromExchangeRate.RefCur;
												qrfPackagePrice.QRFCurrency_Id = totalCost[0].QRFCurrency_Id;
												qrfPackagePrice.QRFCurrency = totalCost[0].QRFCurrency;
												qrfPackagePrice.Status = null;
												qrfPackagePrice.Create_Date = DateTime.Now;
												qrfPackagePrice.Create_User = "Default";
												qrfPackagePrice.RoomName = room.RoomTypeName.ToUpper();

												foreach (var tc in totalCostIncHotel)
												{
													var posPrice = qrfPositionPrice.Where(a => a.QRFCostForPositionID == tc.QRFCostForPositionID).ToList();

													foreach (var pp in posPrice)
													{
														if (pp.IsAdditional == false)
														{
															if (pp.ProductRange.ToUpper().Replace("(" + pp.PersoneType.ToUpper() + ")", "").Trim() == room.RoomTypeName.ToUpper().Trim())
															{
																qrfPackagePrice.BuyPrice = qrfPackagePrice.BuyPrice + pp.TotalBuyPrice;
																qrfPackagePrice.SellPrice = qrfPackagePrice.SellPrice + pp.TotalSellPrice;
															}
														}
													}
												}

												qrfPackagePrice.BuyPrice = qrfPackagePrice.BuyPrice + buyPriceExcHotel;
												qrfPackagePrice.SellPrice = qrfPackagePrice.SellPrice + sellPriceExcHotel;

												qrfPackagePrice.ProfitAmount = qrfPackagePrice.SellPrice - (qrfPackagePrice.BuyPrice * exchangeRate);//Excahnge rate pending
												if (qrfPackagePrice.SellPrice > 0)
												{
													if (qrfPackagePrice.SellPrice - qrfPackagePrice.ProfitAmount > 0)
														qrfPackagePrice.ProfitPercentage = qrfPackagePrice.ProfitAmount * 100 / (qrfPackagePrice.SellPrice - qrfPackagePrice.ProfitAmount);
													else
														qrfPackagePrice.ProfitPercentage = 100;
												}
												await _MongoContext.mQRFPackagePrice.InsertOneAsync(qrfPackagePrice);
											}

											//For CHILD, INFANT....
											var agentPassengerInfo = quote.AgentPassengerInfo.Where(a => a.Type != "ADULT").ToList();

											foreach (var passInfo in agentPassengerInfo)
											{
												if (passInfo.Type == "INFANT")
												{
													if (passInfo.count > 0)
													{
														var qrfPackagePrice = new mQRFPackagePrice();
														qrfPackagePrice.QRFPackagePriceId = Guid.NewGuid().ToString();
														qrfPackagePrice.QRFID = QRFID;
														qrfPackagePrice.QRFPrice_Id = QRFPriceId;
														qrfPackagePrice.Departure_Id = quote.Departures[i].Departure_Id;
														qrfPackagePrice.DepartureDate = quote.Departures[i].Date;
														qrfPackagePrice.PaxSlab_Id = totalCost[0].PaxSlab_Id;
														qrfPackagePrice.PaxSlab = totalCost[0].PaxSlab;
														qrfPackagePrice.BuyCurrencyId = currFromExchangeRate.Currency_Id;
														qrfPackagePrice.BuyCurrency = currFromExchangeRate.RefCur;
														qrfPackagePrice.QRFCurrency_Id = totalCost[0].QRFCurrency_Id;
														qrfPackagePrice.QRFCurrency = totalCost[0].QRFCurrency;
														qrfPackagePrice.Status = null;
														qrfPackagePrice.Create_Date = DateTime.Now;
														qrfPackagePrice.Create_User = "Default";
														qrfPackagePrice.RoomName = passInfo.Type;

														foreach (var tc in totalCostIncHotel)
														{
															var posPrice = qrfPositionPrice.Where(a => a.QRFCostForPositionID == tc.QRFCostForPositionID).ToList();

															foreach (var pp in posPrice)
															{
																if (pp.PersoneType == passInfo.Type.ToUpper())
																{
																	qrfPackagePrice.BuyPrice = qrfPackagePrice.BuyPrice + pp.TotalBuyPrice;
																	qrfPackagePrice.SellPrice = qrfPackagePrice.SellPrice + pp.TotalSellPrice;
																}
															}
														}

														qrfPackagePrice.BuyPrice = qrfPackagePrice.BuyPrice + buyPriceExcHotelCHILD;
														qrfPackagePrice.SellPrice = qrfPackagePrice.SellPrice + sellPriceExcHotelCHILD;

														qrfPackagePrice.ProfitAmount = qrfPackagePrice.SellPrice - (qrfPackagePrice.BuyPrice * exchangeRate);//Excahnge rate pending
														if (qrfPackagePrice.SellPrice > 0)
														{
															if (qrfPackagePrice.SellPrice - qrfPackagePrice.ProfitAmount > 0)
																qrfPackagePrice.ProfitPercentage = qrfPackagePrice.ProfitAmount * 100 / (qrfPackagePrice.SellPrice - qrfPackagePrice.ProfitAmount);
															else
																qrfPackagePrice.ProfitPercentage = 100;
														}
														await _MongoContext.mQRFPackagePrice.InsertOneAsync(qrfPackagePrice);
													}
												}
												else
												{
													string childType = "";
													if (passInfo.Type == "CHILDWITHBED")
														childType = "Child + Bed";
													else if (passInfo.Type == "CHILDWITHOUTBED")
														childType = "Child - Bed";

													if (passInfo.count > 0)
													{
														foreach (var data in passInfo.Age)
														{
															var qrfPackagePrice = new mQRFPackagePrice();
															qrfPackagePrice.QRFPackagePriceId = Guid.NewGuid().ToString();
															qrfPackagePrice.QRFID = QRFID;
															qrfPackagePrice.QRFPrice_Id = QRFPriceId;
															qrfPackagePrice.Departure_Id = quote.Departures[i].Departure_Id;
															qrfPackagePrice.DepartureDate = quote.Departures[i].Date;
															qrfPackagePrice.PaxSlab_Id = totalCost[0].PaxSlab_Id;
															qrfPackagePrice.PaxSlab = totalCost[0].PaxSlab;
															qrfPackagePrice.BuyCurrencyId = currFromExchangeRate.Currency_Id;
															qrfPackagePrice.BuyCurrency = currFromExchangeRate.RefCur;
															qrfPackagePrice.QRFCurrency_Id = totalCost[0].QRFCurrency_Id;
															qrfPackagePrice.QRFCurrency = totalCost[0].QRFCurrency;
															qrfPackagePrice.Status = null;
															qrfPackagePrice.Create_Date = DateTime.Now;
															qrfPackagePrice.Create_User = "Default";
															qrfPackagePrice.RoomName = passInfo.Type;
															qrfPackagePrice.Age = data;

															foreach (var tc in totalCostIncHotel)
															{
																bool flagChildFound = false;
																var posPrice = qrfPositionPrice.Where(a => a.QRFCostForPositionID == tc.QRFCostForPositionID).ToList();

																foreach (var pp in posPrice)
																{
																	if (pp.PersoneType == childType)
																	{
																		var range = prodRange.Where(a => a.VoyagerProductRange_Id == pp.ProductRange_Id).FirstOrDefault();
																		if (Convert.ToInt32(range.Agemin) <= data && data <= Convert.ToInt32(range.Agemax))
																		{
																			flagChildFound = true;
																			qrfPackagePrice.BuyPrice = qrfPackagePrice.BuyPrice + pp.TotalBuyPrice;
																			qrfPackagePrice.SellPrice = qrfPackagePrice.SellPrice + pp.TotalSellPrice;
																		}
																	}
																}
																if (!flagChildFound)
																{
																	foreach (var pp in posPrice)
																	{
																		if (pp.PersoneType == "CHILD")
																		{
																			var range = prodRange.Where(a => a.VoyagerProductRange_Id == pp.ProductRange_Id).FirstOrDefault();
																			if (Convert.ToInt32(range.Agemin) <= data && data <= Convert.ToInt32(range.Agemax))
																			{
																				qrfPackagePrice.BuyPrice = qrfPackagePrice.BuyPrice + pp.TotalBuyPrice;
																				qrfPackagePrice.SellPrice = qrfPackagePrice.SellPrice + pp.TotalSellPrice;
																			}
																		}
																	}
																}
															}

															qrfPackagePrice.BuyPrice = qrfPackagePrice.BuyPrice + buyPriceExcHotelCHILD;
															qrfPackagePrice.SellPrice = qrfPackagePrice.SellPrice + sellPriceExcHotelCHILD;

															qrfPackagePrice.ProfitAmount = qrfPackagePrice.SellPrice - (qrfPackagePrice.BuyPrice * exchangeRate);//Excahnge rate pending
															if (qrfPackagePrice.SellPrice > 0)
															{
																if (qrfPackagePrice.SellPrice - qrfPackagePrice.ProfitAmount > 0)
																	qrfPackagePrice.ProfitPercentage = qrfPackagePrice.ProfitAmount * 100 / (qrfPackagePrice.SellPrice - qrfPackagePrice.ProfitAmount);
																else
																	qrfPackagePrice.ProfitPercentage = 100;
															}
															await _MongoContext.mQRFPackagePrice.InsertOneAsync(qrfPackagePrice);
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
				#endregion

				//In non-package price, we consider only those Positions which are Optional and supplement
				//Package price is calculated for per Departure, only 1 Pax and ADULT, CHILD etc
				#region save mQRFNonPackagedPrice
				var qrfPositionTotalCostOp = _MongoContext.mQRFPositionTotalCost.AsQueryable().Where(a => a.QRFPrice_Id == QRFPriceId && a.PositionKeepAs != "Included").ToList();

				int paxslabCount = 0;
				for (int i = 0; i < quote.Departures.Count; i++)
				{
					paxslabCount = 0;
					for (int j = 0; j < quote.PaxSlabDetails.QRFPaxSlabs.Count; j++)
					{
						if ((!(quote.Departures[i].IsDeleted)) && (!(quote.PaxSlabDetails.QRFPaxSlabs[j].IsDeleted)))
						{
							paxslabCount = paxslabCount + 1;
							if (paxslabCount == 1) // Save Non packaged price only for 1 pax per departures
							{
								var totalCost = qrfPositionTotalCostOp.Where(a => a.PaxSlab_Id == quote.PaxSlabDetails.QRFPaxSlabs[j].PaxSlab_Id && a.Departure_Id == quote.Departures[i].Departure_Id).ToList();

								foreach (var tc in totalCost)
								{
									double exchangeRate = exchangeRateList.Where(a => a.FromCurrency == quote.AgentProductInfo.BudgetCurrencyCode && a.ToCurrency == tc.BuyCurrency).Select(b => b.ExchangeRate).FirstOrDefault();
									if (exchangeRate == 0) exchangeRate = 1;
									var posPrice = qrfPositionPrice.Where(a => a.QRFCostForPositionID == tc.QRFCostForPositionID).ToList();

									// var disProductType = posPrice.Select(a => a.PersoneType).Distinct().ToList();

									//ADULT
									var qrfNonPackagePrice = new mQRFNonPackagedPrice();
									qrfNonPackagePrice.QRFSupplementPriceID = Guid.NewGuid().ToString();
									qrfNonPackagePrice.QRFID = QRFID;
									qrfNonPackagePrice.QRFPrice_Id = QRFPriceId;
									qrfNonPackagePrice.PositionId = tc.PositionId;
									qrfNonPackagePrice.PositionType = tc.PositionType;
									qrfNonPackagePrice.ProductId = tc.ProductId;
									qrfNonPackagePrice.ProductName = tc.ProductName;
									qrfNonPackagePrice.PositionKeepAs = tc.PositionKeepAs;
									qrfNonPackagePrice.Departure_Id = quote.Departures[i].Departure_Id;
									qrfNonPackagePrice.DepartureDate = quote.Departures[i].Date;
									qrfNonPackagePrice.PaxSlab_Id = totalCost[0].PaxSlab_Id;
									qrfNonPackagePrice.PaxSlab = totalCost[0].PaxSlab;
									qrfNonPackagePrice.BuyCurrencyId = tc.BuyCurrencyId;
									qrfNonPackagePrice.BuyCurrency = tc.BuyCurrency;
									qrfNonPackagePrice.QRFCurrency_Id = tc.QRFCurrency_Id;
									qrfNonPackagePrice.QRFCurrency = tc.QRFCurrency;
									qrfNonPackagePrice.Status = null;
									qrfNonPackagePrice.Create_Date = DateTime.Now;
									qrfNonPackagePrice.Create_User = "Default";
									qrfNonPackagePrice.RoomName = "ADULT";

									foreach (var pp in posPrice)
									{
										if (pp.PersoneType == "ADULT")
										{
											qrfNonPackagePrice.BuyPrice = qrfNonPackagePrice.BuyPrice + pp.TotalBuyPrice;
											qrfNonPackagePrice.SellPrice = qrfNonPackagePrice.SellPrice + pp.TotalSellPrice;
										}
										else if (pp.PersoneType == "UNIT" || pp.PersoneType == "DRIVER" || pp.PersoneType == "GUIDE")
										{
											qrfNonPackagePrice.BuyPrice = (qrfNonPackagePrice.BuyPrice / quote.PaxSlabDetails.QRFPaxSlabs[j].DivideByCost) + pp.TotalBuyPrice;
											qrfNonPackagePrice.SellPrice = (qrfNonPackagePrice.SellPrice / quote.PaxSlabDetails.QRFPaxSlabs[j].DivideByCost) + pp.TotalSellPrice;
										}
									}

									qrfNonPackagePrice.ProfitAmount = qrfNonPackagePrice.SellPrice - (qrfNonPackagePrice.BuyPrice * exchangeRate);
									if (qrfNonPackagePrice.SellPrice > 0)
									{
										if (qrfNonPackagePrice.SellPrice - qrfNonPackagePrice.ProfitAmount > 0)
											qrfNonPackagePrice.ProfitPercentage = qrfNonPackagePrice.ProfitAmount * 100 / (qrfNonPackagePrice.SellPrice - qrfNonPackagePrice.ProfitAmount);
										else
											qrfNonPackagePrice.ProfitPercentage = 100;
									}

									await _MongoContext.mQRFNonPackagedPrice.InsertOneAsync(qrfNonPackagePrice);

									//For CHILD, INFANT....
									var agentPassengerInfo = quote.AgentPassengerInfo.Where(a => a.Type != "ADULT").ToList();
									foreach (var passInfo in agentPassengerInfo)
									{
										if (passInfo.Type == "INFANT")
										{
											if (passInfo.count > 0)
											{
												qrfNonPackagePrice = new mQRFNonPackagedPrice();
												qrfNonPackagePrice.QRFSupplementPriceID = Guid.NewGuid().ToString();
												qrfNonPackagePrice.QRFID = QRFID;
												qrfNonPackagePrice.QRFPrice_Id = QRFPriceId;
												qrfNonPackagePrice.PositionId = tc.PositionId;
												qrfNonPackagePrice.PositionType = tc.PositionType;
												qrfNonPackagePrice.PositionKeepAs = tc.PositionKeepAs;
												qrfNonPackagePrice.Departure_Id = quote.Departures[i].Departure_Id;
												qrfNonPackagePrice.DepartureDate = quote.Departures[i].Date;
												qrfNonPackagePrice.PaxSlab_Id = totalCost[0].PaxSlab_Id;
												qrfNonPackagePrice.PaxSlab = totalCost[0].PaxSlab;
												qrfNonPackagePrice.BuyCurrencyId = tc.BuyCurrencyId;
												qrfNonPackagePrice.BuyCurrency = tc.BuyCurrency;
												qrfNonPackagePrice.QRFCurrency_Id = tc.QRFCurrency_Id;
												qrfNonPackagePrice.QRFCurrency = tc.QRFCurrency;
												qrfNonPackagePrice.Status = null;
												qrfNonPackagePrice.Create_Date = DateTime.Now;
												qrfNonPackagePrice.Create_User = "Default";
												qrfNonPackagePrice.RoomName = "INFANT";

												foreach (var pp in posPrice)
												{
													if (pp.PersoneType == "INFANT")
													{
														qrfNonPackagePrice.BuyPrice = qrfNonPackagePrice.BuyPrice + pp.TotalBuyPrice;
														qrfNonPackagePrice.SellPrice = qrfNonPackagePrice.SellPrice + pp.TotalSellPrice;
													}
												}

												qrfNonPackagePrice.ProfitAmount = qrfNonPackagePrice.SellPrice - (qrfNonPackagePrice.BuyPrice * exchangeRate);
												if (qrfNonPackagePrice.SellPrice > 0)
												{
													if (qrfNonPackagePrice.SellPrice - qrfNonPackagePrice.ProfitAmount > 0)
														qrfNonPackagePrice.ProfitPercentage = qrfNonPackagePrice.ProfitAmount * 100 / (qrfNonPackagePrice.SellPrice - qrfNonPackagePrice.ProfitAmount);
													else
														qrfNonPackagePrice.ProfitPercentage = 100;
												}

												await _MongoContext.mQRFNonPackagedPrice.InsertOneAsync(qrfNonPackagePrice);
											}
										}
										else
										{
											string childType = "";
											if (passInfo.Type == "CHILDWITHBED")
												childType = "Child + Bed";
											else if (passInfo.Type == "CHILDWITHOUTBED")
												childType = "Child - Bed";

											if (passInfo.count > 0)
											{
												foreach (var data in passInfo.Age)
												{
													bool flagChildFound = false;
													qrfNonPackagePrice = new mQRFNonPackagedPrice();
													qrfNonPackagePrice.QRFSupplementPriceID = Guid.NewGuid().ToString();
													qrfNonPackagePrice.QRFID = QRFID;
													qrfNonPackagePrice.QRFPrice_Id = QRFPriceId;
													qrfNonPackagePrice.PositionId = tc.PositionId;
													qrfNonPackagePrice.PositionType = tc.PositionType;
													qrfNonPackagePrice.PositionKeepAs = tc.PositionKeepAs;
													qrfNonPackagePrice.Departure_Id = quote.Departures[i].Departure_Id;
													qrfNonPackagePrice.DepartureDate = quote.Departures[i].Date;
													qrfNonPackagePrice.PaxSlab_Id = totalCost[0].PaxSlab_Id;
													qrfNonPackagePrice.PaxSlab = totalCost[0].PaxSlab;
													qrfNonPackagePrice.BuyCurrencyId = tc.BuyCurrencyId;
													qrfNonPackagePrice.BuyCurrency = tc.BuyCurrency;
													qrfNonPackagePrice.QRFCurrency_Id = tc.QRFCurrency_Id;
													qrfNonPackagePrice.QRFCurrency = tc.QRFCurrency;
													qrfNonPackagePrice.Status = null;
													qrfNonPackagePrice.Create_Date = DateTime.Now;
													qrfNonPackagePrice.Create_User = "Default";
													qrfNonPackagePrice.RoomName = passInfo.Type;
													qrfNonPackagePrice.Age = data;

													foreach (var pp in posPrice)
													{
														if (pp.PersoneType == childType)
														{
															var range = prodRange.Where(a => a.VoyagerProductRange_Id == pp.ProductRange_Id).FirstOrDefault();
															if (Convert.ToInt32(range.Agemin) <= data && data <= Convert.ToInt32(range.Agemax))
															{
																flagChildFound = true;
																qrfNonPackagePrice.BuyPrice = qrfNonPackagePrice.BuyPrice + pp.TotalBuyPrice;
																qrfNonPackagePrice.SellPrice = qrfNonPackagePrice.SellPrice + pp.TotalSellPrice;
															}
														}
													}

													if (!flagChildFound)
													{
														foreach (var pp in posPrice)
														{
															if (pp.PersoneType == "CHILD")
															{
																var range = prodRange.Where(a => a.VoyagerProductRange_Id == pp.ProductRange_Id).FirstOrDefault();
																if (Convert.ToInt32(range.Agemin) <= data && data <= Convert.ToInt32(range.Agemax))
																{
																	qrfNonPackagePrice.BuyPrice = qrfNonPackagePrice.BuyPrice + pp.TotalBuyPrice;
																	qrfNonPackagePrice.SellPrice = qrfNonPackagePrice.SellPrice + pp.TotalSellPrice;
																}
															}
														}
													}

													qrfNonPackagePrice.ProfitAmount = qrfNonPackagePrice.SellPrice - (qrfNonPackagePrice.BuyPrice * exchangeRate);
													if (qrfNonPackagePrice.SellPrice > 0)
													{
														if (qrfNonPackagePrice.SellPrice - qrfNonPackagePrice.ProfitAmount > 0)
															qrfNonPackagePrice.ProfitPercentage = qrfNonPackagePrice.ProfitAmount * 100 / (qrfNonPackagePrice.SellPrice - qrfNonPackagePrice.ProfitAmount);
														else
															qrfNonPackagePrice.ProfitPercentage = 100;
													}

													await _MongoContext.mQRFNonPackagedPrice.InsertOneAsync(qrfNonPackagePrice);

												}
											}
										}
									}
								}
							}
						}
					}
				}

				#endregion

                #region RoundSellingPriceInCostsheet as it rounds the value in int and save it into mQRFPackagePrice,mQRFNonPackagedPrice,mQRFPositionTotalCost collection
                Thread t = new Thread(new ThreadStart(() => RoundSellingPriceInCostsheet(QRFPriceId, quote.EditUser)));
                t.Start();
                #endregion

				return true;
			}
			catch (Exception e)
			{
				return false;
			}
		}

		public async Task<ResponseStatus> SaveCommercialPrice(string QRFID, string UserEmail, string QRFPriceID)
		{
			ResponseStatus responseStatus = new ResponseStatus();
			try
			{
				var objQRFPrice = await _MongoContext.mQRFPrice.FindAsync(a => a.QRFID == QRFID && a.QRFPrice_Id == QRFPriceID).Result.FirstOrDefaultAsync();
				if (objQRFPrice != null)
				{
					objQRFPrice.Departures = objQRFPrice.Departures.Where(a => a.IsDeleted == false).ToList();
					objQRFPrice.PaxSlabDetails.QRFPaxSlabs = objQRFPrice.PaxSlabDetails.QRFPaxSlabs.Where(a => a.IsDeleted == false).ToList();

					var objGuesstimate = _MongoContext.mGuesstimate.AsQueryable().Where(a => a.QRFID == QRFID && a.GuesstimateId == objQRFPrice.Guesstimate.GuesstimateId).FirstOrDefault();

					if (objGuesstimate != null)
					{
						var posids = objGuesstimate.GuesstimatePosition.Select(a => a.PositionId).ToList();
						var lstQRFPosition = _MongoContext.mQRFPosition.AsQueryable().Where(a => a.QRFID == QRFID && posids.Contains(a.PositionId)).ToList();

						Commercial objCommercial = new Commercial();
						List<CommsPositions> lstCommsPositions = new List<CommsPositions>();
						var lstCommercialSlabs = new List<CommercialSlabs>();

						var positions = objGuesstimate.GuesstimatePosition;
						var BaseCurrency = objQRFPrice.ExchangeRateSnapshot;
						var ExchangeRateDetailList = BaseCurrency?.ExchangeRateDetail;

						if (ExchangeRateDetailList == null || ExchangeRateDetailList?.Count == 0)
						{
							BaseCurrency = _MongoContext.mExchangeRate.AsQueryable().Where(a => a.DateMin <= DateTime.Now && DateTime.Now <= a.DateMax).Select(a => new ExchangeRateSnapshot
							{
								Currency_Id = a.Currency_Id,
								REFCUR = a.RefCur,
								ExchangeRate_id = a.ExchangeRateId,
								DATEMAX = a.DateMax,
								DATEMIN = a.DateMin,
								EXRATE = a.ExRate,
								VATRATE = a.VatRate,
								CREA_DT = a.CreateDate
							}).FirstOrDefault();

							ExchangeRateDetailList = _MongoContext.mExchangeRateDetail.AsQueryable().Where(a => a.ExchangeRate_Id == BaseCurrency.ExchangeRate_id)
								.Select(a => new ExchangeRateDetailSnapshot
								{
									Currency_Id = a.Currency_Id,
									CURRENCY = a.CURRENCY,
									RATE = a.RATE,
									ROUNDTO = a.ROUNDTO
								}).ToList();
						}

						objCommercial.QRFID = QRFID;
						objCommercial.CreateDate = DateTime.Now;
						objCommercial.CreateUser = UserEmail;
						objCommercial.Mode = "Costsheet";
						objCommercial.IsCurrentVersion = true;
						objCommercial.BaseCurrency = BaseCurrency.REFCUR;
						objCommercial.InvoiceCurrency = objQRFPrice.QRFCurrency;

						List<CommercialDepartures> lstCommercialDepartures = new List<CommercialDepartures>();
						for (int i = 0; i < objQRFPrice.Departures.Count; i++)
						{
							lstCommercialSlabs = new List<CommercialSlabs>();
							for (int j = 0; j < objQRFPrice.PaxSlabDetails.QRFPaxSlabs.Count; j++)
							{
								lstCommsPositions = new List<CommsPositions>();
								//calculate Breakup Price details for each positions
								for (int k = 0; k < positions.Count; k++)
								{
									var totalPosBudgetPrice = Convert.ToDouble(positions[k]?.GuesstimatePrice?.Select(a => a.BudgetPrice).Sum());
									var totalPosContractPrice = Convert.ToDouble(positions[k]?.GuesstimatePrice?.Select(a => a.ContractPrice).Sum());
									var totalPosBuyPrice = Convert.ToDouble(positions[k]?.GuesstimatePrice?.Select(a => a.BuyPrice).Sum());
									var Purchase = 0.0;
									var Sell = 0.0;
									var rate = 0.0;
									var ProfitPercentage = 0.0;
									var Margin = 0.0;
									var pos = lstQRFPosition.Where(a => a.PositionId == positions[k].PositionId).FirstOrDefault();

									var FromCurrencyRate = ExchangeRateDetailList.Where(a => a.Currency_Id == BaseCurrency.Currency_Id).FirstOrDefault();
									var ToCurrencyRateBuy = ExchangeRateDetailList.Where(a => a.CURRENCY == positions[k].BuyCurrency).FirstOrDefault();

									if (!(FromCurrencyRate == null || ToCurrencyRateBuy == null))
									{
										rate = Math.Round(Convert.ToDouble(ToCurrencyRateBuy.RATE / FromCurrencyRate.RATE), 4);

										if (rate > 0)
										{
											Purchase = (totalPosContractPrice == 0 ? totalPosBuyPrice : totalPosContractPrice) / rate;
										}
									}
									var ToCurrencyRateSell = ExchangeRateDetailList.Where(a => a.Currency_Id == objQRFPrice.AgentProductInfo.BudgetCurrencyID).FirstOrDefault();
									if (!(FromCurrencyRate == null || ToCurrencyRateSell == null))
									{
										rate = Math.Round(Convert.ToDouble(ToCurrencyRateSell.RATE / FromCurrencyRate.RATE), 4);

										if (rate > 0)
										{
											Sell = totalPosContractPrice / rate;
											if (Sell < 0)//for negative numbers
											{
												Sell = Math.Floor(Sell);
											}
											else if (Sell > 0)//for positive numbers
											{
												Sell = Math.Ceiling(Sell);
											}
										}
									}
									Margin = Sell - Purchase;
									ProfitPercentage = Margin * 100 / Sell;

									lstCommsPositions.Add(new CommsPositions()
									{
										GridInfo = positions[k].OriginalItineraryDescription,
										PositionId = positions[k].PositionId,
										PositionType = pos.KeepAs,
										ProductType = pos.ProductType,
										Calculation = new CommercialReport()
										{
											Purchasing = new CommsPurchasing()
											{
												Currency = positions[k].BuyCurrency,
												Budget = totalPosBudgetPrice,
												Actual = totalPosContractPrice == 0 ? totalPosBuyPrice : totalPosContractPrice
											},
											Profitability = new CommsProfitability()
											{
												Currency = objQRFPrice.ExchangeRateSnapshot.REFCUR,
												Purchase = Purchase,
												Sell = Sell,
												Margin = Margin,
												ProfitPercentage = ProfitPercentage
											}
										}
									});
								}
								var consiladatedCommsProfitability = lstCommsPositions.GroupBy(a => a.ProductType).Select(b => new TotalForProductType
								{
									ProductType = b.First().ProductType,
									Total = new CommsProfitability()
									{
										Currency = objQRFPrice.ExchangeRateSnapshot.REFCUR,
										Sell = b.Sum(c => c.Calculation.Profitability.Sell),
										Purchase = b.Sum(c => c.Calculation.Profitability.Purchase),
										Margin = b.Sum(c => c.Calculation.Profitability.Margin),
										ProfitPercentage = b.Sum(c => c.Calculation.Profitability.ProfitPercentage),
									}
								}).ToList();

								//calculate consolidated Price details by ProductType Wise such as Hotel,LDC,Coach,Meal
								lstCommercialSlabs.Add(new CommercialSlabs()
								{
									PaxSlabId = objQRFPrice.PaxSlabDetails.QRFPaxSlabs[j].PaxSlab_Id,
									PaxSlab = objQRFPrice.PaxSlabDetails.QRFPaxSlabs[j].From.ToString() + " - " + objQRFPrice.PaxSlabDetails.QRFPaxSlabs[j].To.ToString(),
									Breakup = lstCommsPositions,
									Consolidated = consiladatedCommsProfitability
								});

							}

							lstCommercialDepartures.Add(new CommercialDepartures()
							{
								DepartureId = objQRFPrice.Departures[i].Departure_Id,
								StartDate = Convert.ToDateTime(objQRFPrice.Departures[i].Date),
								EndDate = Convert.ToDateTime(objQRFPrice.Departures[i].Date),
								Slabs = lstCommercialSlabs
							});
						}

						responseStatus.Status = "Success";
					}
				}

			}
			catch (Exception ex)
			{
				responseStatus.Status = "Failure";
				responseStatus.ErrorMessage = ex.Message;
			}
			return responseStatus;
		}

		public string GetRoutingMatrixForItinerary(string CityIds = "")
		{
			DistanceMatrixGetRes response;
			string RoutingMatrix = "";
			if (CityIds.Contains(','))
			{
				string[] Ids = CityIds.Split(",");
				for (int i = 0; i < Ids.Length - 1; i++)
				{
					if (Ids[i].Trim() != Ids[i + 1].Trim())
					{
						response = _genericRepository.GetDistanceMatrixForCity(Ids[i].Trim(), Ids[i + 1].Trim()).Result;
						if (response != null)
						{
							if (response.status == "OK")
							{
								if (i == Ids.Length - 2)
									RoutingMatrix = RoutingMatrix + response.OriginCity + " - " + response.DestinationCity + " (" + response.Rows[0].Elements[0].distance.text + " - " + response.Rows[0].Elements[0].duration.text + ")";
								else
									RoutingMatrix = RoutingMatrix + response.OriginCity + " - " + response.DestinationCity + " (" + response.Rows[0].Elements[0].distance.text + " - " + response.Rows[0].Elements[0].duration.text + ")" + "|";
							}
						}
					}
				}
				return RoutingMatrix;
			}
			else
				return "";
		}

		public async Task<ResponseStatus> RoundSellingPriceInCostsheet(string QRFPriceId, string UserEmail)
		{
			ResponseStatus objResponseStatus = new ResponseStatus();
			try
			{
				var QrfPackagePrice = _MongoContext.mQRFPackagePrice.AsQueryable().Where(a => a.QRFPrice_Id == QRFPriceId && a.SellPrice != 0).ToList();
				var QrfNonPackagePrice = _MongoContext.mQRFNonPackagedPrice.AsQueryable().Where(a => a.QRFPrice_Id == QRFPriceId && a.SellPrice != 0).ToList();
				var QRFPositionTotalCost = _MongoContext.mQRFPositionTotalCost.AsQueryable().Where(a => a.QRFPrice_Id == QRFPriceId && a.TotalSellPrice != 0).ToList();

				foreach (var item in QrfPackagePrice)
				{
					if ((item.SellPrice % 1) != 0)
					{
						if (item.SellPrice < 0)//for negative numbers
						{
							item.SellPrice = Math.Floor(item.SellPrice);
						}
						else if (item.SellPrice > 0)//for positive numbers
						{
							item.SellPrice = Math.Ceiling(item.SellPrice);
						}
						await _MongoContext.mQRFPackagePrice.FindOneAndUpdateAsync(
										   Builders<mQRFPackagePrice>.Filter.Eq("QRFPackagePriceId", item.QRFPackagePriceId),
										   Builders<mQRFPackagePrice>.Update.
										   Set("SellPrice", item.SellPrice).
										   Set("Edit_Date", DateTime.Now).
										   Set("Edit_User", UserEmail));
					}
				}

				foreach (var item in QrfNonPackagePrice)
				{
					if ((item.SellPrice % 1) != 0)
					{
						if (item.SellPrice < 0)//for negative numbers
						{
							item.SellPrice = Math.Floor(item.SellPrice);
						}
						else if (item.SellPrice > 0)//for positive numbers
						{
							item.SellPrice = Math.Ceiling(item.SellPrice);
						}
						await _MongoContext.mQRFNonPackagedPrice.FindOneAndUpdateAsync(
										   Builders<mQRFNonPackagedPrice>.Filter.Eq("QRFSupplementPriceID", item.QRFSupplementPriceID),
										   Builders<mQRFNonPackagedPrice>.Update.
										   Set("SellPrice", item.SellPrice).
										   Set("Edit_Date", DateTime.Now).
									   Set("Edit_User", UserEmail));
					}
				}

				foreach (var item in QRFPositionTotalCost)
				{
					if ((item.TotalSellPrice % 1) != 0)
					{
						if (item.TotalSellPrice < 0)//for negative numbers
						{
							item.TotalSellPrice = Math.Floor(item.TotalSellPrice);
						}
						else if (item.TotalSellPrice > 0)//for positive numbers
						{
							item.TotalSellPrice = Math.Ceiling(item.TotalSellPrice);
						}
						await _MongoContext.mQRFPositionTotalCost.FindOneAndUpdateAsync(
										   Builders<mQRFPositionTotalCost>.Filter.Eq("QRFCostForPositionID", item.QRFCostForPositionID),
										   Builders<mQRFPositionTotalCost>.Update.
										   Set("TotalSellPrice", item.TotalSellPrice).
										   Set("Edit_Date", DateTime.Now).
										   Set("Edit_User", UserEmail));
					}
				}
				objResponseStatus.Status = "Success";
			}
			catch (Exception ex)
			{
				objResponseStatus.Status = "Failure";
				objResponseStatus.ErrorMessage = "An Error Occurs:- " + ex.Message;
			}
			return objResponseStatus;
		}

		#endregion

		#region CopyQuote
		public async Task<GetQRFForCopyQuoteRes> GetQRFDataForCopyQuote(QuoteAgentGetReq request)
		{
			var response = new GetQRFForCopyQuoteRes();
			try
			{
				var quote = _MongoContext.mQuote.AsQueryable().Where(a => a.QRFID == request.QRFID).FirstOrDefault();

				if (quote != null)
				{
					response.QRFID = request.QRFID;
					if (quote.CurrentPipeline == "Quote Pipeline")
					{
						response.TourName = quote.AgentProductInfo.TourName;
						response.AgentInfo = quote.AgentInfo;
						response.ExisitingDepatures = quote.Departures.Where(a => a.IsDeleted != true).ToList().Select(a => new ExisitingDepatures { DepatureDate = a.Date, DepatureId = a.Departure_Id, PPTwin = 0 }).OrderBy(a => a.DepatureDate).ToList();
						response.ResponseStatus.Status = "Success";
					}
					else if (quote.CurrentPipeline == "Costing Pipeline" || quote.CurrentPipeline == "Agent Approval Pipeline")
					{
						var qRFPrice = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == request.QRFID && a.IsCurrentVersion).FirstOrDefault();
						if (qRFPrice != null)
						{
							double SellPrice = 0;
							string Currency = "";
							bool flag = false;
							int? totalRoomCnt = 0;
							string[] paxslab = new string[] { };

							response.TourName = qRFPrice.AgentProductInfo.TourName;
							response.AgentInfo = qRFPrice.AgentInfo;
							response.ExisitingDepatures = qRFPrice.Departures.Where(a => a.IsDeleted != true).ToList().Select(a => new ExisitingDepatures { DepatureDate = a.Date, DepatureId = a.Departure_Id, PPTwin = 0 }).OrderBy(a => a.DepatureDate).ToList();


							foreach (var item in qRFPrice.QRFAgentRoom)
							{
								if (!string.IsNullOrEmpty(item.RoomTypeName))
								{
									totalRoomCnt = totalRoomCnt + (item.RoomTypeName.ToLower() == "single" ? item.RoomCount :
																   item.RoomTypeName.ToLower() == "twin" ? (item.RoomCount * 2) :
																   item.RoomTypeName.ToLower() == "double" ? (item.RoomCount * 2) :
																   item.RoomTypeName.ToLower() == "quad" ? (item.RoomCount * 4) :
																   item.RoomTypeName.ToLower() == "triple" ? (item.RoomCount * 3) :
																   item.RoomTypeName.ToLower() == "tsu" ? item.RoomCount : 0);
								}
							}

							var resQRFPackagePrice = await _MongoContext.mQRFPackagePrice.FindAsync(m => m.QRFID == request.QRFID && m.QRFPrice_Id == qRFPrice.QRFPrice_Id).Result.ToListAsync();

							resQRFPackagePrice = resQRFPackagePrice.OrderBy(a => a.RoomName.ToLower() == "twin" ? "A" :
																						 a.RoomName.ToLower() == "double" ? "B" :
																						 a.RoomName.ToLower() == "triple" ? "C" :
																						 a.RoomName.ToLower() == "single" ? "D" :
																						 a.RoomName.ToLower() == "quad" ? "E" :
																						 a.RoomName.ToLower() == "tsu" ? "F" : "G").ToList();
							bool chkPrice = false;
							foreach (var itemDeptDates in response.ExisitingDepatures)
							{
								foreach (var itemQRFPkgPr in resQRFPackagePrice)
								{
									paxslab = itemQRFPkgPr.PaxSlab.Split('-');
									//check if totalRoomCnt falls in PaxSlab Range then take it by following RoomName order
									if (paxslab.Length > 0 && totalRoomCnt >= Convert.ToInt32(paxslab[0]) && totalRoomCnt <= Convert.ToInt32(paxslab[1])
										&& itemDeptDates.DepatureId == itemQRFPkgPr.Departure_Id)
									{
										if (!string.IsNullOrEmpty(itemQRFPkgPr.RoomName))
										{
											chkPrice = true;
											if (itemQRFPkgPr.RoomName.ToLower() == "twin")
											{
												SellPrice = itemQRFPkgPr.SellPrice;
												Currency = itemQRFPkgPr.BuyCurrency;
												break;
											}
											else if (itemQRFPkgPr.RoomName.ToLower() == "double")
											{
												SellPrice = itemQRFPkgPr.SellPrice;
												Currency = itemQRFPkgPr.BuyCurrency;
												break;
											}
											else if (itemQRFPkgPr.RoomName.ToLower() == "triple")
											{
												SellPrice = itemQRFPkgPr.SellPrice;
												Currency = itemQRFPkgPr.BuyCurrency;
												break;
											}
											else if (itemQRFPkgPr.RoomName.ToLower() == "single")
											{
												SellPrice = itemQRFPkgPr.SellPrice;
												Currency = itemQRFPkgPr.BuyCurrency;
												break;
											}
											else if (itemQRFPkgPr.RoomName.ToLower() == "quad")
											{
												SellPrice = itemQRFPkgPr.SellPrice;
												Currency = itemQRFPkgPr.BuyCurrency;
												break;
											}
											else if (itemQRFPkgPr.RoomName.ToLower() == "tsu")
											{
												SellPrice = itemQRFPkgPr.SellPrice;
												Currency = itemQRFPkgPr.BuyCurrency;
												break;
											}
										}
									}
								}

								if (!chkPrice)
								{
									SellPrice = resQRFPackagePrice.FirstOrDefault().SellPrice;
									Currency = resQRFPackagePrice.FirstOrDefault().BuyCurrency;
								}
								chkPrice = false;

								flag = SellPrice == 0 ? true : false;
								if (flag)
								{
									var resQRFPkgPr = resQRFPackagePrice.FirstOrDefault();
									if (resQRFPkgPr != null)
									{
										SellPrice = resQRFPkgPr.SellPrice;
										Currency = resQRFPkgPr.BuyCurrency;
									}
								}
								response.ExisitingDepatures.Where(a => a.DepatureId == itemDeptDates.DepatureId).FirstOrDefault().PPTwin = SellPrice;
								response.ExisitingDepatures.Where(a => a.DepatureId == itemDeptDates.DepatureId).FirstOrDefault().Currency = Currency;
							}

						}
						response.ResponseStatus.Status = "Success";
					}
					else
					{
						response.ResponseStatus.ErrorMessage = "QRF data not available.";
						response.ResponseStatus.Status = "Error";
					}
				}
				else
				{
					response.ResponseStatus.ErrorMessage = "QRF data not available.";
					response.ResponseStatus.Status = "Error";
				}
			}
			catch (Exception e)
			{
				response.ResponseStatus.ErrorMessage = e.Message;
				response.ResponseStatus.Status = "Error";
			}
			return response;
		}

		public async Task<SetCopyQuoteRes> SetCopyQuote(SetCopyQuoteReq request)
		{
			var response = new SetCopyQuoteRes();
			try
			{
				var quote = _MongoContext.mQuote.AsQueryable().Where(a => a.QRFID == request.QRFID).FirstOrDefault();
				var PositionList = _MongoContext.mPosition.AsQueryable().Where(a => a.QRFID == request.QRFID).ToList();
				var PositionPricesList = _MongoContext.mPositionPrice.AsQueryable().Where(a => a.QRFID == request.QRFID).ToList();
				var PositionFOCList = _MongoContext.mPositionFOC.AsQueryable().Where(a => a.QRFID == request.QRFID).ToList();
				var Itinerary = _MongoContext.mItinerary.AsQueryable().Where(a => a.QRFID == request.QRFID && a.IsDeleted == false).FirstOrDefault();
				var FOCDetailsList = quote.FOCDetails;
				var DepartureDates = new DepartureDates();

				#region Take data from Costing to quote if exist
				if (quote.CurrentPipeline == "Costing Pipeline" || quote.CurrentPipeline == "Agent Approval Pipeline")
				{
					//Getting costing data of Positions and Prices
					var qrfprice = _MongoContext.mQRFPrice.AsQueryable().Where(x => x.QRFID == request.QRFID && x.IsCurrentVersion == true).FirstOrDefault();
					var guesstimate = _MongoContext.mGuesstimate.AsQueryable().Where(x => x.QRFID == request.QRFID && x.IsCurrentVersion == true).FirstOrDefault();

					//Updating KeepAs of Positions
					foreach (var g in guesstimate.GuesstimatePosition)
					{
						foreach (var pos in PositionList)
						{
							if (pos.PositionId == g.PositionId)
							{
								pos.KeepAs = g.KeepAs;
							}
						}
					}

					//Update Budgetprice of PositionPrice
					foreach (var g in guesstimate.GuesstimatePosition)
					{
						var prices = g.GuesstimatePrice.Where(x => x.SupplierId == g.ActiveSupplierId).ToList();

						PositionList.Where(a => a.PositionId == g.PositionId).ToList().ForEach(b => { b.SupplierId = g.ActiveSupplierId; b.SupplierName = g.ActiveSupplier; });

						foreach (var price in prices)
						{
							foreach (var posprice in PositionPricesList)
							{
								if (price.PositionPriceId == posprice.PositionPriceId)
								{
									posprice.BudgetPrice = price.BudgetPrice;
									posprice.SupplierId = g.ActiveSupplierId;
									posprice.Supplier = g.ActiveSupplier;
								}
							}
						}
					}

					#region Margin
					//Update Margins of Quote
					//Package
					//1. Package Properties
					List<PackageProperties> lstPkg = new List<PackageProperties>();
					foreach (var pprop in qrfprice.QRFMargin.Package.PackageProperties)
					{
						PackageProperties qprop = new PackageProperties();
						qprop.ComponentName = pprop.ComponentName;
						qprop.SellingPrice = pprop.SellingPrice;
						qprop.MarginUnit = pprop.MarginUnit;
						lstPkg.Add(qprop);
					}
					quote.Margins.Package = new Package();
					quote.Margins.Package.PackageProperties = lstPkg;

					//2. Margin Computed
					quote.Margins.Package.MarginComputed.TotalCost = qrfprice.QRFMargin.Package.MarginComputed.TotalCost;
					quote.Margins.Package.MarginComputed.TotalLeadersCost = qrfprice.QRFMargin.Package.MarginComputed.TotalLeadersCost;
					quote.Margins.Package.MarginComputed.Upgrade = qrfprice.QRFMargin.Package.MarginComputed.Upgrade;
					quote.Margins.Package.MarginComputed.MarkupType = qrfprice.QRFMargin.Package.MarginComputed.MarkupType;

					//Product
					//1. Product Properties
					List<ProductProperties> lstProd = new List<ProductProperties>();
					foreach (var pprop in qrfprice.QRFMargin.Product.ProductProperties)
					{
						ProductProperties qprop = new ProductProperties();
						qprop.VoyagerProductType_Id = pprop.VoyagerProductType_Id;
						qprop.Prodtype = pprop.Prodtype;
						qprop.SellingPrice = pprop.SellingPrice;
						qprop.MarginUnit = pprop.MarginUnit;
						qprop.HowMany = pprop.HowMany;
						lstProd.Add(qprop);
					}
					quote.Margins.Product = new Product();
					quote.Margins.Product.ProductProperties = lstProd;

					//2. Margin Computed
					quote.Margins.Product.MarginComputed.TotalCost = qrfprice.QRFMargin.Product.MarginComputed.TotalCost;
					quote.Margins.Product.MarginComputed.TotalLeadersCost = qrfprice.QRFMargin.Product.MarginComputed.TotalLeadersCost;
					quote.Margins.Product.MarginComputed.Upgrade = qrfprice.QRFMargin.Product.MarginComputed.Upgrade;
					quote.Margins.Product.MarginComputed.MarkupType = qrfprice.QRFMargin.Product.MarginComputed.MarkupType;

					//Item
					//1. Item Properties
					List<ItemProperties> lstItem = new List<ItemProperties>();
					foreach (var pprop in qrfprice.QRFMargin.Item.ItemProperties)
					{
						ItemProperties qprop = new ItemProperties();
						qprop.PositionID = pprop.PositionID;
						qprop.ProductName = pprop.ProductName;
						qprop.VoyagerProductType_Id = pprop.VoyagerProductType_Id;
						qprop.Prodtype = pprop.Prodtype;
						qprop.SellingPrice = pprop.SellingPrice;
						qprop.MarginUnit = pprop.MarginUnit;
						qprop.HowMany = pprop.HowMany;
						lstItem.Add(qprop);
					}
					quote.Margins.Itemwise = new Itemwise();
					quote.Margins.Itemwise.ItemProperties = lstItem;

					//2. Margin Computed
					quote.Margins.Itemwise.MarginComputed.TotalCost = qrfprice.QRFMargin.Item.MarginComputed.TotalCost;
					quote.Margins.Itemwise.MarginComputed.TotalLeadersCost = qrfprice.QRFMargin.Item.MarginComputed.TotalLeadersCost;
					quote.Margins.Itemwise.MarginComputed.Upgrade = qrfprice.QRFMargin.Item.MarginComputed.Upgrade;
					quote.Margins.Itemwise.MarginComputed.MarkupType = qrfprice.QRFMargin.Item.MarginComputed.MarkupType;

					#endregion
				}
				#endregion

				if (quote != null)
				{
					QRFCounterRequest qrfCounterRequest = new QRFCounterRequest { CounterType = _configuration["CounterType:QRF"].ToString() };
					QRFCounterRequest qrfCounterRequestDep = new QRFCounterRequest { CounterType = _configuration["CounterType:QRFDeparture"].ToString() };
					QRFCounterRequest qrfCounterRequestFOC = new QRFCounterRequest { CounterType = _configuration["CounterType:QRFFOC"].ToString() };

					#region Quote
					quote._Id = new ObjectId();
					quote.QRFID = _genericRepository.GetNextReferenceNumber(qrfCounterRequest).LastReferenceNumber.ToString();
					quote.Parent_QRFID = request.QRFID;
					quote.Remarks = "Quote copied from " + request.QRFID + " by " + request.CreateUser;
					quote.AgentProductInfo.TourName = request.TourName;

					quote.AgentInfo.AgentID = request.AgentId;
					quote.AgentInfo.AgentName = _quoteRepository.GetAgentCompaniesByID(request.AgentId).Name;
					quote.AgentInfo.ContactPerson = _quoteRepository.GetContactsForAgentCompanyByID(request.ContactPerson) == null ? "" : _quoteRepository.GetContactsForAgentCompanyByID(request.ContactPerson).FullName;
					quote.AgentInfo.ContactPersonID = request.ContactPerson;
					quote.AgentInfo.MobileNo = request.MobileNo;
					quote.AgentInfo.EmailAddress = request.Email;

					quote.Departures = new List<DepartureDates>();
					quote.FOCDetails = new List<FOCDetails>();

					foreach (var extdep in request.CopyQuoteDepartures)
					{
						DepartureDates = new DepartureDates();
						DepartureDates.Departure_Id = _genericRepository.GetNextReferenceNumber(qrfCounterRequestDep).LastReferenceNumber;
						DepartureDates.Date = extdep.NewDepartureDate;
						DepartureDates.NoOfDep = 1;
						DepartureDates.PaxPerDep = 1;
						DepartureDates.Warning = null;
						DepartureDates.IsDeleted = false;
						DepartureDates.CreateDate = DateTime.Now;
						DepartureDates.CreateUser = request.CreateUser;
						quote.Departures.Add(DepartureDates);

						var FOCList = FOCDetailsList.Where(a => a.DateRangeId == extdep.DepartureId).ToList();
						if (FOCList?.Count > 0)
						{
							FOCList.ForEach(a =>
							{
								a.FOC_Id = _genericRepository.GetNextReferenceNumber(qrfCounterRequestFOC).LastReferenceNumber;
								a.DateRangeId = DepartureDates.Departure_Id;
								a.DateRange = extdep.NewDepartureDate.ToString("dd-MMM-yy");
								a.CreateDate = DateTime.Now;
								a.CreateUser = request.CreateUser;
								a.EditDate = null;
								a.EditUser = null;
							});
							quote.FOCDetails.AddRange(FOCList);
						}
					}

					quote.TourEntities?.ForEach(a =>
					{
						a.TourEntityID = Guid.NewGuid().ToString();
						a.CreateDate = DateTime.Now;
						a.CreateUser = request.CreateUser;
						a.EditDate = null;
						a.EditUser = null;
					});

					if (quote.Meals != null)
					{
						quote.Meals.CreateDate = DateTime.Now;
						quote.Meals.CreateUser = request.CreateUser;
						quote.Meals.EditDate = null;
						quote.Meals.EditUser = null;
					}

					quote.FollowUp = null;
					quote.CreateDate = DateTime.Now;
					quote.CreateUser = request.CreateUser;
					quote.EditDate = null;
					quote.EditUser = null;

					#endregion

					#region Positions/Pice/FOC

					if (PositionList?.Count > 0)
					{
						foreach (var position in PositionList)
						{
							var NewPositionId = Guid.NewGuid().ToString();
							foreach (var extdep in request.CopyQuoteDepartures)
							{
								#region PositionPrices
								var dep = quote.Departures.Where(a => a.Date == extdep.NewDepartureDate).FirstOrDefault();
								var PricesList = new List<mPositionPrice>();
								PricesList.AddRange(PositionPricesList?.Where(a => a.PositionId == position.PositionId && a.DepartureId == extdep.DepartureId).ToList().Select(price => new mPositionPrice
								{
									_Id = new ObjectId(),
									PositionPriceId = Guid.NewGuid().ToString(),
									QRFID = quote.QRFID,
									Period = dep.Date,
									PositionId = NewPositionId,
									DepartureId = dep.Departure_Id,
									PaxSlabId = price.PaxSlabId,
									PaxSlab = price.PaxSlab,
									ContractPeriod = price.ContractPeriod,
									Type = price.Type,
									RoomId = price.RoomId,
									IsSupplement = price.IsSupplement,
									SupplierId = price.SupplierId,
									Supplier = price.Supplier,
									ProductCategoryId = price.ProductCategoryId,
									ProductCategory = price.ProductCategory,
									ProductRangeId = price.ProductRangeId,
									ProductRange = price.ProductRange,
									ProductRangeCode = price.ProductRangeCode,
									TourEntityId = price.TourEntityId,

									BuyCurrencyId = price.BuyCurrencyId,
									BuyCurrency = price.BuyCurrency,
									ContractId = price.ContractId,
									ContractPrice = price.ContractPrice,
									BudgetPrice = price.BudgetPrice,
									BuyPrice = price.BuyPrice,
									MarkupAmount = price.MarkupAmount,
									BuyNetPrice = price.BuyNetPrice,

									SellCurrencyId = price.SellCurrencyId,
									SellCurrency = price.SellCurrency,
									SellNetPrice = price.SellNetPrice,
									TaxAmount = price.TaxAmount,
									SellPrice = price.SellPrice,
									ExchangeRateId = price.ExchangeRateId,
									ExchangeRatio = price.ExchangeRatio,

									CreateDate = DateTime.Now,
									CreateUser = request.CreateUser,
									EditDate = null,
									EditUser = null,
									IsDeleted = price.IsDeleted
								}).ToList());

								if (PricesList?.Count > 0)
									await _MongoContext.mPositionPrice.InsertManyAsync(PricesList);
								#endregion

								#region PositionFOC
								var FOCList = new List<mPositionFOC>();
								FOCList.AddRange(PositionFOCList?.Where(a => a.PositionId == position.PositionId && a.DepartureId == extdep.DepartureId).ToList().Select(foc => new mPositionFOC
								{
									_Id = new ObjectId(),
									PositionFOCId = Guid.NewGuid().ToString(),
									QRFID = quote.QRFID,
									Period = dep.Date,
									ContractPeriod = foc.ContractPeriod,
									PositionId = NewPositionId,
									DepartureId = dep.Departure_Id,
									PaxSlabId = foc.PaxSlabId,
									PaxSlab = foc.PaxSlab,
									Type = foc.Type,
									CityId = foc.CityId,
									CityName = foc.CityName,
									ProductId = foc.ProductId,
									ProductName = foc.ProductName,
									RoomId = foc.RoomId,
									IsSupplement = foc.IsSupplement,
									SupplierId = foc.SupplierId,
									Supplier = foc.Supplier,
									ProductCategoryId = foc.ProductCategoryId,
									ProductCategory = foc.ProductCategory,
									ProductRangeId = foc.ProductRangeId,
									ProductRange = foc.ProductRange,
									ContractId = foc.ContractId,
									Quantity = foc.Quantity,
									FOCQty = foc.FOCQty,
									CreateDate = DateTime.Now,
									CreateUser = request.CreateUser,
									EditDate = null,
									EditUser = null,
									IsDeleted = foc.IsDeleted
								}).ToList());

								if (FOCList?.Count > 0)
									await _MongoContext.mPositionFOC.InsertManyAsync(FOCList);

								#endregion

								#region Update PositionId in Markup
								quote.Margins?.Itemwise?.ItemProperties?.Where(a => a.PositionID == position.PositionId).ToList()?.ForEach(a => { a.PositionID = NewPositionId; });
								#endregion
							}

							#region TourEntities/Meal in mQuote
							if (quote.TourEntities?.Count > 0)
							{
								quote.TourEntities.ForEach(a => { if (a.PositionID == position.PositionId) { a.PositionID = NewPositionId; } });
							}
							if (quote.Meals?.MealDays?.Count > 0)
							{
								quote.Meals?.MealDays?.ForEach(a => { a.MealDayInfo?.ForEach(b => { if (b.PositionID == position.PositionId) { b.PositionID = NewPositionId; } }); });
							}
							#endregion

							#region Itinerary
							if (Itinerary != null)
							{
								Itinerary.ItineraryDays?.ForEach(a => { a.ItineraryDescription?.ForEach(b => { if (b.PositionId == position.PositionId) { b.PositionId = NewPositionId; } }); });
								Itinerary.ItineraryDays?.ForEach(a => { a.Hotel?.ForEach(b => { if (b.PositionId == position.PositionId) { b.PositionId = NewPositionId; } }); });
								Itinerary.ItineraryDays?.ForEach(a => { a.Meal?.ForEach(b => { if (b.PositionId == position.PositionId) { b.PositionId = NewPositionId; } }); });
							}
							#endregion
							position._Id = new ObjectId();
							position.QRFID = quote.QRFID;
							position.PositionId = NewPositionId;
							position.CreateDate = DateTime.Now;
							position.CreateUser = request.CreateUser;
							position.EditDate = null;
							position.EditUser = null;
							await _MongoContext.mPosition.InsertOneAsync(position);
						}
					}
					#endregion
					await _MongoContext.mQuote.InsertOneAsync(quote);
					#region Itinerary
					if (Itinerary != null)
					{
						Itinerary._Id = new ObjectId();
						Itinerary.QRFID = quote.QRFID;
						Itinerary.ItineraryID = Guid.NewGuid().ToString();
						Itinerary.Version = 1;
						Itinerary.CreateDate = DateTime.Now;
						Itinerary.CreateUser = request.CreateUser;
						Itinerary.EditDate = null;
						Itinerary.EditUser = null;
						await _MongoContext.mItinerary.InsertOneAsync(Itinerary);
					}
					#endregion

					if (quote.CurrentPipeline == "Quote Pipeline")
					{
						//await SaveDefaultItinerary(request.CreateUser, quote.QRFID, Guid.NewGuid().ToString(), false);
						response.QRFID = quote.QRFID;
						response.ResponseStatus.Status = "Success";
					}
					else if (quote.CurrentPipeline == "Costing Pipeline" || quote.CurrentPipeline == "Agent Approval Pipeline")
					{
						#region Proposal
						var proposal = _MongoContext.mProposal.AsQueryable().Where(a => a.QRFID == request.QRFID && a.IsDeleted == false).FirstOrDefault();

						if (proposal != null)
						{
							proposal._Id = new ObjectId();
							proposal.QRFID = quote.QRFID;
							proposal.ProposalId = Guid.NewGuid().ToString();
							proposal.ItineraryId = Itinerary?.ItineraryID;
							proposal.Version = 1;
							proposal.CreateDate = DateTime.Now;
							proposal.CreateUser = request.CreateUser;
							proposal.EditDate = null;
							proposal.EditUser = null;

							await _MongoContext.mProposal.InsertOneAsync(proposal);
						}
						#endregion
						QuoteSetReq req = new QuoteSetReq();
						req.QRFID = quote.QRFID;
						req.CostingOfficer = quote.CostingOfficer;
						req.EnquiryPipeline = quote.CurrentPipeline;
						req.IsApproveQuote = false;
						req.IsUI = false;
						req.IsCopyQuote = true;
						req.PlacerEmail = request.CreateUser;
						req.Remarks = "Quote copied from " + request.QRFID + " by " + request.CreateUser;
						await SubmitQuote(req);
						response.QRFID = quote.QRFID;
						response.ResponseStatus.Status = "Success";
					}
				}
				else
				{
					response.ResponseStatus.ErrorMessage = "QRF data not available.";
					response.ResponseStatus.Status = "Error";
				}

			}
			catch (Exception e)
			{
				response.ResponseStatus.ErrorMessage = e.Message;
				response.ResponseStatus.Status = "Error";
			}
			return response;
		}
		#endregion


	}
}
