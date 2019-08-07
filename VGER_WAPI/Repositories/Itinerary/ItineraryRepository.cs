using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI.Helpers;
using VGER_WAPI.Models;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Repositories
{
	public class ItineraryRepository : IItineraryRepository
	{
		#region Private Variable Declaration
		private readonly MongoContext _MongoContext = null;
		private readonly IQuoteRepository _quoteRepository;
		private readonly IPositionRepository _positionRepository;
		private readonly IQRFSummaryRepository _qrfSummaryRepository;
		#endregion

		public ItineraryRepository(IOptions<MongoSettings> settings, IQuoteRepository quoteRepository, IPositionRepository positionRepository, IQRFSummaryRepository qrfSummaryRepository)
		{
			_MongoContext = new MongoContext(settings);
			_quoteRepository = quoteRepository;
			_positionRepository = positionRepository;
			_qrfSummaryRepository = qrfSummaryRepository;
		}

		#region Itinerary
		public async Task<mItinerary> GetItinerary(ItineraryGetReq request)
		{
			try
			{
				mItinerary response = new mItinerary();
				response = _MongoContext.mItinerary.AsQueryable().Where(a => a.QRFID == request.QRFID).FirstOrDefault();
				response.ItineraryDays.ForEach(b => b.ItineraryDescription = b.ItineraryDescription.OrderBy(c => c.StartTime).ToList());
				return response;
			}
			catch (Exception ex)
			{
				throw;
			}
		}

		public async Task<mItinerary> GetItineraryDetails(ItineraryGetReq request)
		{
			try
			{
				mItinerary response = new mItinerary();
				bool IsDefault = false;
				string pagename = string.IsNullOrEmpty(request.Page) ? "" : request.Page.ToLower() ?? "";
				response = _MongoContext.mItinerary.AsQueryable().Where(a => a.QRFID == request.QRFID).FirstOrDefault();

				string ItineraryId;
				if (response == null || string.IsNullOrEmpty(response.ItineraryID))
					ItineraryId = Guid.NewGuid().ToString();
				else
					ItineraryId = response.ItineraryID;

				if (pagename == "qrfsummary")
				{
					if (response == null)
					{
						IsDefault = await _qrfSummaryRepository.SaveDefaultItinerary(request.editUser, request.QRFID, ItineraryId, false);
					}
					else
					{
						var RegenerateItinerary = _MongoContext.mQuote.AsQueryable().Where(a => a.QRFID == request.QRFID).Select(b => b.RegenerateItinerary).FirstOrDefault();
						if (RegenerateItinerary)
						{
							IsDefault = await _qrfSummaryRepository.SaveDefaultItinerary(request.editUser, request.QRFID, ItineraryId, false);
						}
						else
						{
							IsDefault = true;
						}
					}
				}
				else
				{
					if (response == null)
					{
						IsDefault = await _qrfSummaryRepository.SaveDefaultItinerary(request.editUser, request.QRFID, ItineraryId, true);
					}
					else
					{
						var RegenerateItinerary = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == request.QRFID && a.IsCurrentVersion).Select(b => b.RegenerateItinerary).FirstOrDefault();
						if (RegenerateItinerary)
						{
							IsDefault = await _qrfSummaryRepository.SaveDefaultItinerary(request.editUser, request.QRFID, ItineraryId, true);
						}
						else
						{
							IsDefault = true;
						}
					}
				}

				if (IsDefault == true)
					response = _MongoContext.mItinerary.AsQueryable().Where(a => a.QRFID == request.QRFID).FirstOrDefault();
				else
					response = null;

				response.ItineraryDays.ForEach(b => b.ItineraryDescription = b.ItineraryDescription.OrderBy(c => c.StartTime).ToList());
				return response;
			}
			catch (Exception ex)
			{
				throw;
			}
		}

		public async Task<ItinerarySetRes> SetItinerary(ItinerarySetReq request)
		{
			ItinerarySetRes response = new ItinerarySetRes();
			try
			{
				mItinerary itinerary;

				//To enter new itinerary element in existing itinerary days
				if (request.IsExtraItineraryElement == false)
				{
					if (request.IsNewVersion)
					{
						//Add
						itinerary = _MongoContext.mItinerary.AsQueryable().Where(x => x.ItineraryID == request.itinerary.ItineraryID).FirstOrDefault();

						itinerary._Id = ObjectId.Empty;
						itinerary.ItineraryID = Guid.NewGuid().ToString();
						itinerary.Version = itinerary.Version + 1;
						itinerary.CreateUser = itinerary.CreateUser;
						itinerary.CreateDate = DateTime.Now;

						await _MongoContext.mItinerary.InsertOneAsync(itinerary);
						response.ResponseStatus.Status = "Success";
						response.ResponseStatus.ErrorMessage = "Saved Successfully.";
					}
					else
					{
						//Update
						itinerary = _MongoContext.mItinerary.AsQueryable().Where(x => x.ItineraryID == request.itinerary.ItineraryID).FirstOrDefault();

						foreach (var days in itinerary.ItineraryDays)
						{
							foreach (var daysReq in request.itinerary.ItineraryDays)
							{
								if (days.ItineraryDaysId == daysReq.ItineraryDaysId)
								{
									foreach (var desc in days.ItineraryDescription)
									{
										foreach (var descReq in daysReq.ItineraryDescription)
										{
											if (desc.PositionId == descReq.PositionId)
											{
												desc.IsDeleted = descReq.IsDeleted;
												desc.ProductName = string.IsNullOrEmpty(descReq.ProductName) ? desc.ProductName : descReq.ProductName;
												desc.StartTime = string.IsNullOrEmpty(descReq.StartTime) ? desc.StartTime : descReq.StartTime;
												desc.EndTime = string.IsNullOrEmpty(descReq.EndTime) ? desc.EndTime : descReq.EndTime;
												desc.City = string.IsNullOrEmpty(descReq.City) ? desc.City : descReq.City;
												desc.TLRemarks = descReq.TLRemarks;
												desc.OPSRemarks = descReq.OPSRemarks;
												desc.EditDate = DateTime.Now;
												desc.EditUser = !string.IsNullOrWhiteSpace(descReq.EditUser) ? descReq.EditUser : request.itinerary.EditUser;
												break;
											}
										}
									}
									break;
								}
							}
						}

						ReplaceOneResult replaceResult = await _MongoContext.mItinerary.ReplaceOneAsync(Builders<mItinerary>.Filter.Eq("ItineraryID", itinerary.ItineraryID), itinerary);
						response.ResponseStatus.Status = replaceResult.MatchedCount > 0 ? "Success" : "Failure";
						response.ResponseStatus.ErrorMessage = replaceResult.MatchedCount > 0 ? "Saved Successfully." : "Details not updated.";
					}
				}
				else
				{
					//IsExtraItineraryElement is true

					itinerary = _MongoContext.mItinerary.AsQueryable().Where(x => x.QRFID == request.itinerary.QRFID && x.ItineraryID == request.itinerary.ItineraryID).FirstOrDefault();

					if (itinerary != null)
					{

						foreach (var day in itinerary.ItineraryDays)
						{
							foreach (var reqDay in request.itinerary.ItineraryDays)
							{
								if (reqDay.ItineraryDaysId == day.ItineraryDaysId)
								{
									day.ItineraryDescription.Add(new ItineraryDescriptionInfo
									{
										PositionId = Guid.NewGuid().ToString(),
										City = reqDay.ItineraryDescription[0].City,
										ProductType = reqDay.ItineraryDescription[0].ProductType == null ? "" : reqDay.ItineraryDescription[0].ProductType,
										StartTime = reqDay.ItineraryDescription[0].StartTime,
										EndTime = reqDay.ItineraryDescription[0].EndTime,
										Type = reqDay.ItineraryDescription[0].Type == null ? "" : reqDay.ItineraryDescription[0].Type,
										ProductName = reqDay.ItineraryDescription[0].ProductName == null ? "" : reqDay.ItineraryDescription[0].ProductName,
										NumberOfPax = reqDay.ItineraryDescription[0].NumberOfPax,
										KeepAs = reqDay.ItineraryDescription[0].KeepAs == null ? "" : reqDay.ItineraryDescription[0].KeepAs,
										IsDeleted = reqDay.ItineraryDescription[0].IsDeleted,
										CreateDate = DateTime.Now,
										CreateUser = !string.IsNullOrWhiteSpace(request.itinerary.CreateUser) ? request.itinerary.CreateUser : itinerary.CreateUser
									});
								}
							}
						}
						var resultFlag = await _MongoContext.mItinerary.UpdateOneAsync(Builders<mItinerary>.Filter.Eq("ItineraryID", itinerary.ItineraryID),
						Builders<mItinerary>.Update.Set("ItineraryDays", itinerary.ItineraryDays));

						response.ResponseStatus.Status = "Success";
						response.ResponseStatus.ErrorMessage = "Saved Successfully.";
					}
					else
					{
						response.ResponseStatus.Status = "Error";
						response.ResponseStatus.ErrorMessage = "No records to insert.";
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

		public async Task<ItinerarySetRes> EnableDisablePosition(ItinerarySetReq request)
		{
			ItinerarySetRes response = new ItinerarySetRes();
			try
			{
				//Update Itinerary Description (Position)
				mItinerary itinerary; mQRFPosition qrfPosition; mPosition position; mQuote quote;
				itinerary = _MongoContext.mItinerary.AsQueryable().Where(x => x.ItineraryID == request.ItineraryId).FirstOrDefault();
				foreach (var i in itinerary.ItineraryDays)
				{
					if (i.ItineraryDaysId == request.ItineraryDaysId)
					{
						foreach (var desc in i.ItineraryDescription)
						{
							if (desc.PositionId == request.PositionId)
							{
								desc.IsDeleted = request.IsDeleted;
							}
						}
					}
				}

				// Update Itinerary Meal Array
				foreach (var i in itinerary.ItineraryDays)
				{
					if (i.ItineraryDaysId == request.ItineraryDaysId)
					{
						foreach (var meal in i.Meal)
						{
							if (meal.PositionId == request.PositionId)
							{
								meal.IsDeleted = request.IsDeleted;
							}
						}
					}
				}

				//Update Itinerary Hotel Array
				foreach (var i in itinerary.ItineraryDays)
				{
					if (i.ItineraryDaysId == request.ItineraryDaysId)
					{
						foreach (var hotel in i.Hotel)
						{
							if (hotel.PositionId == request.PositionId)
							{
								hotel.IsDeleted = request.IsDeleted;
							}
						}
					}
				}

				await _MongoContext.mItinerary.UpdateOneAsync(Builders<mItinerary>.Filter.Eq("ItineraryID", request.ItineraryId),
							Builders<mItinerary>.Update.Set("ItineraryDays", itinerary.ItineraryDays).Set("EditDate", DateTime.Now));//.Set("EditUser", request.EditUser)

				#region Set RegenerateItinerary
				await _MongoContext.mQuote.UpdateOneAsync(Builders<mQuote>.Filter.Eq("QRFID", request.QRFId),
							   Builders<mQuote>.Update.Set("RegenerateItinerary", true).Set("EditUser", request.itinerary.EditUser).Set("EditDate", DateTime.Now));

				var resultQRFQuote = await _MongoContext.mQRFPrice.FindAsync(m => m.QRFID == request.QRFId && m.IsCurrentVersion).Result.FirstOrDefaultAsync();

				if (resultQRFQuote != null)
				{
					await _MongoContext.mQRFPrice.UpdateOneAsync(Builders<mQRFPrice>.Filter.Eq("QRFPrice_Id", resultQRFQuote.QRFPrice_Id),
						  Builders<mQRFPrice>.Update.Set("RegenerateItinerary", true).Set("EditUser", request.itinerary.EditUser).Set("EditDate", DateTime.Now));
				}
				#endregion

				//Update Quote Meal Array
				quote = _MongoContext.mQuote.AsQueryable().Where(x => x.QRFID == request.QRFId).FirstOrDefault();
				if (quote != null && quote.Meals != null && quote.Meals.MealDays != null)
				{
					foreach (var meal in quote.Meals.MealDays)
					{
						foreach (var info in meal.MealDayInfo)
						{
							if (info.PositionID == request.PositionId)
							{
								info.IsDeleted = request.IsDeleted;
							}
						}
					}
					await _MongoContext.mQuote.UpdateOneAsync(Builders<mQuote>.Filter.Eq("QRFID", request.QRFId),
						Builders<mQuote>.Update.Set("Meals", quote.Meals));
				}

				//Update Position
				position = _MongoContext.mPosition.AsQueryable().Where(x => x.QRFID == request.QRFId && x.PositionId == request.PositionId).FirstOrDefault();
				if (position != null)
				{
					position.IsDeleted = request.IsDeleted;
					await _MongoContext.mPosition.ReplaceOneAsync(Builders<mPosition>.Filter.Eq("PositionId", request.PositionId), position);
				}

				//Update QRF Position
				qrfPosition = _MongoContext.mQRFPosition.AsQueryable().Where(x => x.QRFID == request.QRFId && x.PositionId == request.PositionId).FirstOrDefault();
				if (qrfPosition != null)
				{
					qrfPosition.IsDeleted = request.IsDeleted;
					await _MongoContext.mQRFPosition.ReplaceOneAsync(Builders<mQRFPosition>.Filter.Eq("PositionId", request.PositionId), qrfPosition);
				}

				response.ResponseStatus.Status = "Success";
				response.ResponseStatus.ErrorMessage = "Itinerary and QrfPosition updated Successfully.";

			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				response.ResponseStatus.Status = "Failure";
				response.ResponseStatus.ErrorMessage = ex.Message;
			}
			return response;
		}

		public async Task<ItinerarySetRes> SaveRemarks(ItinerarySetReq request)
		{
			ItinerarySetRes response = new ItinerarySetRes();
			try
			{
				//Update Itinerary Description (Position)
				mItinerary itinerary; mPosition position;
				itinerary = _MongoContext.mItinerary.AsQueryable().Where(x => x.ItineraryID == request.ItineraryId).FirstOrDefault();
				foreach (var i in itinerary.ItineraryDays)
				{
					if (i.ItineraryDaysId == request.ItineraryDaysId)
					{
						foreach (var desc in i.ItineraryDescription)
						{
							if (desc.PositionId == request.PositionId)
							{
								desc.TLRemarks = request.TLRemarks;
								desc.OPSRemarks = request.OPSRemarks;
							}
						}
					}
				}
				await _MongoContext.mItinerary.UpdateOneAsync(Builders<mItinerary>.Filter.Eq("ItineraryID", request.ItineraryId),
							Builders<mItinerary>.Update.Set("ItineraryDays", itinerary.ItineraryDays).Set("EditDate", DateTime.Now));//.Set("EditUser", request.EditUser)

				//Update Position
				position = _MongoContext.mPosition.AsQueryable().Where(x => x.QRFID == request.QRFId && x.PositionId == request.PositionId).FirstOrDefault();
				if (position != null)
				{
					position.TLRemarks = request.TLRemarks;
					position.OPSRemarks = request.OPSRemarks;
					await _MongoContext.mPosition.ReplaceOneAsync(Builders<mPosition>.Filter.Eq("PositionId", request.PositionId), position);
				}

				response.ResponseStatus.Status = "Success";
				response.ResponseStatus.ErrorMessage = "Itinerary and Position updated Successfully.";

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

		#region QRFPosition

		public async Task<ItineraryGetRes> GetQRFPosition(ItineraryGetReq request)
		{
			var response = new ItineraryGetRes();
			request.ProductType = request.ProductType ?? new List<ProductType>();

			try
			{
				var resultQuote = await _MongoContext.mQuote.FindAsync(m => m.QRFID == request.QRFID);

				if (resultQuote != null && resultQuote.ToList().Count > 0)
				{
					//#region Routing Info
					//RoutingGetReq routingGetReq = new RoutingGetReq { QRFId = request.QRFId };
					//List<RoutingInfo> lstRoutingInfo = await _quoteRepository.GetQRFRouteDetailsByQRFID(routingGetReq);
					//response.RoutingInfo = lstRoutingInfo != null && lstRoutingInfo.Count > 0 ? lstRoutingInfo : (new List<RoutingInfo>());
					//#endregion

					//#region Routing Days 
					//RoutingDaysGetReq req = new RoutingDaysGetReq { QRFID = request.QRFId };
					//RoutingDaysGetRes res = await _quoteRepository.GetQRFRoutingDays(req);
					//List<AttributeValues> DaysList = new List<AttributeValues>();
					//if (res != null && res.ResponseStatus.Status.ToLower() == "success")
					//{
					//    if (res.RoutingDays != null && res.RoutingDays.Count > 0)
					//    {
					//        foreach (var item in res.RoutingDays)
					//        {
					//            DaysList.Add(new AttributeValues { AttributeValue_Id = item.RoutingDaysID, Value = item.Days });
					//        }
					//    }
					//}
					//response.DaysList = DaysList;
					//response.RoutingDays = res.RoutingDays;
					//#endregion

					List<mQRFPosition> resultPosition = new List<mQRFPosition>();
					if (request.ProductType.Count == 0 && string.IsNullOrEmpty(request.PositionId))
					{
						resultPosition = _MongoContext.mQRFPosition.AsQueryable().Where(q => q.QRFID == request.QRFID && q.IsDeleted == false).Select(q => q).ToList();
					}
					else if (request.ProductType.Count > 0 && string.IsNullOrEmpty(request.PositionId))
					{
						List<string> lstStr = request.ProductType.Select(a => a.ProdType).ToList();

						if (!string.IsNullOrEmpty(request.Type) && (request.Type == "meal" || request.Type == "transfer"))
						{
							resultPosition = _MongoContext.mQRFPosition.AsQueryable().Where(q => q.QRFID == request.QRFID && lstStr.Contains(q.ProductType) && q.IsDeleted == false).Select(q => q).ToList();
							//response = GetMealProducts(req.QRFID, request.Type);
							response.mQRFPosition = resultPosition;
							//response.DaysList = DaysList;
							//response.RoutingDays = res.RoutingDays;
						}
						else
						{
							resultPosition = _MongoContext.mQRFPosition.AsQueryable().Where(q => q.QRFID == request.QRFID && lstStr.Contains(q.ProductType) && q.IsDeleted == false).Select(q => q).ToList();
						}
					}
					else if (request.ProductType.Count > 0 && !string.IsNullOrEmpty(request.PositionId))
					{
						List<string> lstStr = request.ProductType.Select(a => a.ProdType).ToList();
						resultPosition = _MongoContext.mQRFPosition.AsQueryable().Where(q => q.QRFID == request.QRFID && lstStr.Contains(q.ProductType) && q.PositionId == request.PositionId && q.IsDeleted == false).
							Select(q => q).ToList();
					}
					else if (request.ProductType.Count == 0 && !string.IsNullOrEmpty(request.PositionId) && !string.IsNullOrEmpty(request.Type) && request.Type == "all")
					{
						resultPosition = _MongoContext.mQRFPosition.AsQueryable().Where(q => q.QRFID == request.QRFID && q.PositionId == request.PositionId).Select(q => q).ToList();
					}
					else if (request.ProductType.Count == 0 && !string.IsNullOrEmpty(request.PositionId))
					{
						resultPosition = _MongoContext.mQRFPosition.AsQueryable().Where(q => q.QRFID == request.QRFID && q.PositionId == request.PositionId && q.IsDeleted == false).
							Select(q => q).ToList();
					}

					if (resultPosition != null && resultPosition.Count > 0)
					{
						resultPosition.ForEach(c => { c.RoomDetailsInfo.RemoveAll(d => d.IsDeleted == true); });
						response.mQRFPosition = resultPosition;
					}
					else
					{
						response.mQRFPosition = new List<mQRFPosition>();
					}

					//if ((lstRoutingInfo == null || lstRoutingInfo.Count == 0) && resultPosition.Count > 0)
					//{
					//    resultPosition.ForEach(p => { p.IsDeleted = true; p.DeletedFrom = "NoRoutingFound-FromService"; });
					//    PositionSetReq positionSetReq = new PositionSetReq { SaveType = "complete", mqrf = resultPosition };
					//    PositionSetRes objPositionSetRes = SetPosition(positionSetReq).Result;
					//    if (objPositionSetRes != null && response.ResponseStatus.Status.ToLower() == "success")
					//    {
					//        response.ResponseStatus.Status = "Failure";
					//        response.ResponseStatus.ErrorMessage = "No Routing Details found.";
					//    }
					//    else
					//    {
					//        response.ResponseStatus.ErrorMessage = "Details not updated.";
					//        response.ResponseStatus.Status = "Failure";
					//    }
					//    response.DaysList = new List<AttributeValues>();
					//    response.RoutingInfo = new List<RoutingInfo>();
					//    response.RoutingInfo = new List<RoutingInfo>();
					//    response.mQRFPosition = new List<mQRFPosition>();
					//}
					//else
					//{
					//    response.ResponseStatus.Status = "Success";
					//}
				}
				else
				{
					//response.DaysList = new List<AttributeValues>();
					//response.RoutingInfo = new List<RoutingInfo>();
					response.mQRFPosition = new List<mQRFPosition>();
					response.ResponseStatus.Status = "Failure";
					response.ResponseStatus.ErrorMessage = "QRFID is not exists.";
				}
				response.ResponseStatus.Status = "success";
				response.ProductType = request.ProductType;
				response.QRFID = request.QRFID;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			return response;
		}

		#endregion
	}
}
