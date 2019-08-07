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
    public class PositionRepository : IPositionRepository
    {
        #region Private Variable Declaration
        private readonly MongoContext _MongoContext = null;
        private readonly IQuoteRepository _quoteRepository;
        private readonly IProductRepository _productRepository;
        private readonly IMasterRepository _masterRepository;
        private readonly IGenericRepository _genericRepository;
        #endregion

        private readonly IMongoDatabase _thisdatabase = null;

        public PositionRepository(IOptions<MongoSettings> settings, IQuoteRepository quoteRepository, IProductRepository productRepository, IMasterRepository masterRepository, IGenericRepository genericRepository)
        {

            var client = new MongoClient(settings.Value.ConnectionString);
            if (client != null)
            {

                _thisdatabase = client.GetDatabase(settings.Value.Database);
            }
            _MongoContext = new MongoContext(settings);
            _quoteRepository = quoteRepository;
            _productRepository = productRepository;
            _masterRepository = masterRepository;
            _genericRepository = genericRepository;
        }

        #region Get Set Position
        public async Task<PositionGetRes> GetPosition(PositionGetReq request)
        {
            var response = new PositionGetRes();
            request.ProductType = request.ProductType ?? new List<ProductType>();

            try
            {
                var resultQuote = await _MongoContext.mQuote.FindAsync(m => m.QRFID == request.QRFID).Result.FirstOrDefaultAsync();

                if (resultQuote != null)
                {
                    #region Routing Info
                    RoutingGetReq routingGetReq = new RoutingGetReq { QRFID = request.QRFID };
                    List<RoutingInfo> lstRoutingInfo = await _quoteRepository.GetQRFRouteDetailsByQRFID(routingGetReq);
                    response.RoutingInfo = lstRoutingInfo != null && lstRoutingInfo.Count > 0 ? lstRoutingInfo : (new List<RoutingInfo>());
                    #endregion

                    #region Routing Days 
                    RoutingDaysGetReq req = new RoutingDaysGetReq { QRFID = request.QRFID };
                    RoutingDaysGetRes res = await _quoteRepository.GetQRFRoutingDays(req);
                    List<AttributeValues> DaysList = new List<AttributeValues>();
                    if (res != null && res.ResponseStatus.Status.ToLower() == "success")
                    {
                        if (res.RoutingDays != null && res.RoutingDays.Count > 0)
                        {
                            foreach (var item in res.RoutingDays)
                            {
                                DaysList.Add(new AttributeValues { AttributeValue_Id = item.RoutingDaysID, Value = item.Days, CityId = item.FromCityID, CityName = item.FromCityName });
                            }
                        }
                    }
                    response.DaysList = DaysList;
                    response.RoutingDays = res.RoutingDays;
                    #endregion

                    List<mPosition> resultPosition = new List<mPosition>();
                    List<mQRFPosition> resultQrfPosition = new List<mQRFPosition>();

                    if (request.ProductType.Count == 0 && string.IsNullOrEmpty(request.PositionId))
                    {
                        if (request.IsClone == true)
                        {
                            resultQrfPosition = _MongoContext.mQRFPosition.AsQueryable().Where(q => q.QRFID == request.QRFID && q.IsDeleted == false && q.IsTourEntity == false).ToList();
                        }
                        else
                        {
                            resultPosition = _MongoContext.mPosition.AsQueryable().Where(q => q.QRFID == request.QRFID && q.IsDeleted == false && q.IsTourEntity == false).ToList();
                        }
                    }
                    else if (request.ProductType.Count > 0 && string.IsNullOrEmpty(request.PositionId))
                    {
                        List<string> lstStr = request.ProductType.Select(a => a.ProdType).ToList();

                        if (!string.IsNullOrEmpty(request.Type) && (request.Type == "meal" || request.Type == "transfer"))
                        {
                            if (request.IsClone == true)
                            {
                                resultQrfPosition = _MongoContext.mQRFPosition.AsQueryable().Where(q => q.QRFID == request.QRFID && lstStr.Contains(q.ProductType) && q.IsDeleted == false && q.IsTourEntity == false).Select(q => q).ToList();
                                response.mPosition = resultPosition;
                                response.DaysList = DaysList;
                                response.RoutingDays = res.RoutingDays;
                            }
                            else
                            {
                                resultPosition = _MongoContext.mPosition.AsQueryable().Where(q => q.QRFID == request.QRFID && lstStr.Contains(q.ProductType) && q.IsDeleted == false && q.IsTourEntity == false).Select(q => q).ToList();
                                if (request.Type == "meal")
                                {
                                    response = GetMealGridDetails(response.RoutingDays, resultPosition, req.QRFID);
                                }
                                else if (request.Type == "transfer")
                                {
                                    response = GetTransferGridDetails(response.RoutingDays, resultPosition, req.QRFID);
                                }
                                response.mPosition = resultPosition;
                                response.DaysList = DaysList;
                                response.RoutingDays = res.RoutingDays;
                            }
                        }
                        else if (!string.IsNullOrEmpty(request.Type) && request.Type == "bus")
                        {
                            List<string> alllist = new List<string>();
                            List<string> deflist = new List<string> { "Meal", "Attractions", "Sightseeing - CityTour", "Overnight Ferry", "Train" };
                            alllist.AddRange(lstStr);
                            alllist.AddRange(deflist);
                            resultPosition = _MongoContext.mPosition.AsQueryable().Where(q => q.QRFID == request.QRFID && alllist.Contains(q.ProductType) && q.IsDeleted == false && q.IsTourEntity == false).ToList();
                            var buspos = resultPosition.Where(a => lstStr.Contains(a.ProductType)).ToList();
                            var defpos = resultPosition.Where(a => deflist.Contains(a.ProductType) && a.TransferDetails == "With Transfer").ToList();
                            var i = 1;
                            if (defpos != null && defpos.Count > 0)
                            {
                                mPosition objPosition = new mPosition();
                                var dayno = 0;
                                var prodtypeid = "";
                                var dropofflocnm = "";
                                var dropofflocid = "";
                                MasterTypeRequest objMasterTypeRequest = new MasterTypeRequest();
                                objMasterTypeRequest.Property = "QRF Masters";
                                objMasterTypeRequest.Name = "PickUpDropOffLocations";
                                IQueryable<dynamic> objMaster = _masterRepository.GetGenericMasterForTypeByName(objMasterTypeRequest);
                                if (objMaster != null && objMaster.Count() > 0)
                                {
                                    List<Properties> PropertyList = new List<Properties>();
                                    var Property = new Properties();
                                    foreach (var x in objMaster)
                                    {
                                        Property.Attribute = x;
                                    }
                                    PropertyList.Add(Property);
                                    var attrlst = PropertyList.Select(a => a.Attribute).ToList().FirstOrDefault();
                                    if (attrlst != null && attrlst.Count > 0)
                                    {
                                        var attrvalue = attrlst[0].Values;
                                        if (attrvalue != null && attrvalue.Count > 0)
                                        {
                                            var attr = attrvalue.Where(a => a.Value.ToLower() == "the hotel").FirstOrDefault();
                                            if (attr != null)
                                            {
                                                dropofflocnm = attr.Value;
                                                dropofflocid = attr.AttributeValue_Id;
                                            }
                                        }
                                    }
                                }
                                if (buspos == null || buspos.Count == 0)
                                {
                                    mProductType objProdType = _productRepository.GetProductTypeByProdType(new ProdTypeGetReq { ProdType = "Coach" }).FirstOrDefault();
                                    prodtypeid = objProdType.VoyagerProductType_Id;
                                }
                                else
                                {
                                    prodtypeid = buspos.FirstOrDefault().ProductTypeId;
                                }

                                string[] citynm;
                                string[] tocitynm;
                                foreach (var item in defpos)
                                {
                                    if (buspos.Where(a => a.ForPositionId == item.PositionId).Count() == 0)
                                    {
                                        citynm = DaysList.Where(b => b.AttributeValue_Id == item.RoutingDaysID).FirstOrDefault().CityName.Split(",");
                                        objPosition = new mPosition();
                                        objPosition.CityID = DaysList.Where(b => b.AttributeValue_Id == item.RoutingDaysID).FirstOrDefault().CityId;
                                        objPosition.CityName = citynm[0].Trim();
                                        objPosition.CountryName = citynm[1].Trim();
                                        objPosition.DayNo = item.DayNo;
                                        objPosition.StartingFrom = item.StartingFrom;
                                        objPosition.PositionSequence = 0;
                                        objPosition.QRFID = item.QRFID;
                                        objPosition.ProductType = "Coach";
                                        objPosition.ProductTypeId = prodtypeid;
                                        objPosition.RoutingDaysID = item.RoutingDaysID;
                                        objPosition.StartTime = item.StartTime;
                                        objPosition.EndTime = "";
                                        objPosition.ForPositionId = item.PositionId;
                                        objPosition.ToDropOffLoc = dropofflocnm;
                                        objPosition.ToDropOffLocID = dropofflocid;
                                        dayno = item.DayNo + item.Duration;
                                        objPosition.Duration = item.Duration;
                                        objPosition.TLRemarks = item.TLRemarks;
                                        objPosition.OPSRemarks = item.OPSRemarks;

                                        if (dayno <= DaysList.Count)
                                        {
                                            tocitynm = DaysList.Where(b => b.Value == "Day " + dayno.ToString()).FirstOrDefault().CityName.Split(",");

                                            objPosition.ToCityID = DaysList.Where(b => b.Value == "Day " + dayno.ToString()).FirstOrDefault().CityId;
                                            objPosition.ToCityName = tocitynm[0].Trim();
                                            objPosition.ToCountryName = tocitynm[1].Trim();
                                        }
                                        buspos.Add(objPosition);
                                    }
                                }
                            }

                            buspos = buspos.OrderBy(a => a.DayNo).ToList();
                            buspos.ForEach(a => a.PositionSequence = i++);
                            resultPosition = buspos;
                        }
                        else
                        {
                            if (request.IsClone == true)
                            {
                                resultQrfPosition = _MongoContext.mQRFPosition.AsQueryable().Where(q => q.QRFID == request.QRFID && lstStr.Contains(q.ProductType) && q.IsDeleted == false && q.IsTourEntity == false).ToList();
                            }
                            else
                            {
                                resultPosition = _MongoContext.mPosition.AsQueryable().Where(q => q.QRFID == request.QRFID && lstStr.Contains(q.ProductType) && q.IsDeleted == false && q.IsTourEntity == false).ToList();
                            }
                        }
                    }
                    else if (request.ProductType.Count > 0 && !string.IsNullOrEmpty(request.PositionId))
                    {
                        List<string> lstStr = new List<string>();
                        if (request.IsClone == true)
                        {
                            lstStr = request.ProductType.Select(a => a.ProdType).ToList();
                            resultQrfPosition = _MongoContext.mQRFPosition.AsQueryable().Where(q => q.QRFID == request.QRFID && lstStr.Contains(q.ProductType) && q.PositionId == request.PositionId && q.IsDeleted == false && q.IsTourEntity == false).ToList();
                        }
                        else
                        {
                            lstStr = request.ProductType.Select(a => a.ProdType).ToList();
                            resultPosition = _MongoContext.mPosition.AsQueryable().Where(q => q.QRFID == request.QRFID && lstStr.Contains(q.ProductType) && q.PositionId == request.PositionId && q.IsDeleted == false && q.IsTourEntity == false).ToList();
                        }

                        if (lstStr.Contains("Meal"))
                        {
                            if (!string.IsNullOrEmpty(request.PositionId) && request.PositionId != "0")
                            {
                                var prodid = "";
                                if (request.IsClone == true)
                                {
                                    prodid = resultQrfPosition.Where(a => a.PositionId == request.PositionId).FirstOrDefault().ProductID;
                                }
                                else
                                {
                                    prodid = resultPosition.Where(a => a.PositionId == request.PositionId).FirstOrDefault().ProductID;
                                }

                                var Placeholder = _MongoContext.mProducts_Lite.AsQueryable().Where(a => a.VoyagerProduct_Id == prodid).FirstOrDefault().Placeholder;
                                response.IsPlaceHolder = Placeholder;
                            }
                            response.AgentPassengerInfo = resultQuote.AgentPassengerInfo;
                            response.AgentPassengerInfo = response.AgentPassengerInfo.Where(a => a.count > 0).ToList();
                        }
                    }
                    else if (request.ProductType.Count == 0 && !string.IsNullOrEmpty(request.PositionId) && !string.IsNullOrEmpty(request.Type) && request.Type == "all")
                    {
                        if (request.IsClone == true)
                        {
                            resultQrfPosition = _MongoContext.mQRFPosition.AsQueryable().Where(q => q.QRFID == request.QRFID && q.PositionId == request.PositionId && q.IsTourEntity == false).ToList();
                        }
                        else
                        {
                            resultPosition = _MongoContext.mPosition.AsQueryable().Where(q => q.QRFID == request.QRFID && q.PositionId == request.PositionId && q.IsTourEntity == false).ToList();
                        }
                    }
                    else if (request.ProductType.Count == 0 && !string.IsNullOrEmpty(request.PositionId))
                    {
                        if (request.IsClone == true)
                        {
                            resultQrfPosition = _MongoContext.mQRFPosition.AsQueryable().Where(q => q.QRFID == request.QRFID && q.PositionId == request.PositionId && q.IsDeleted == false && q.IsTourEntity == false).ToList();
                        }
                        else
                        {
                            resultPosition = _MongoContext.mPosition.AsQueryable().Where(q => q.QRFID == request.QRFID && q.PositionId == request.PositionId && q.IsDeleted == false && q.IsTourEntity == false).ToList();
                        }
                    }

                    if (resultPosition != null && resultPosition.Count > 0)
                    {
                        resultPosition.ForEach(c => { c.RoomDetailsInfo.RemoveAll(d => d.IsDeleted == true); });
                        response.mPosition = resultPosition;
                    }
                    else
                    {
                        response.mPosition = new List<mPosition>();
                    }

                    if (resultQrfPosition != null && resultQrfPosition.Count > 0)
                    {
                        resultQrfPosition.ForEach(c => { c.RoomDetailsInfo.RemoveAll(d => d.IsDeleted == true); });
                        foreach (var qrfpos in resultQrfPosition)
                        {
                            var pos = new mPosition();

                            pos.ApplyAcrossDays = qrfpos.ApplyAcrossDays;
                            pos.BudgetCategory = qrfpos.BudgetCategory;
                            pos.BudgetCategoryId = qrfpos.BudgetCategoryId;
                            pos.BuyCurrencyId = qrfpos.BuyCurrencyId;
                            pos.BuyCurrency = qrfpos.BuyCurrency;
                            pos.ChainID = qrfpos.ChainID;
                            pos.ChainName = qrfpos.ChainName;
                            pos.CountryName = qrfpos.CountryName;
                            pos.CityName = qrfpos.CityName;
                            pos.CityID = qrfpos.CityID;
                            pos.CreateDate = qrfpos.CreateDate;
                            pos.CreateUser = qrfpos.CreateUser;
                            pos.DayNo = qrfpos.DayNo;
                            pos.DeletedFrom = qrfpos.DeletedFrom;
                            pos.Duration = qrfpos.Duration != 0 ? Convert.ToInt32(qrfpos.Duration) : 0;
                            pos.EarlyCheckInDate = qrfpos.EarlyCheckInDate;
                            pos.EarlyCheckInTime = qrfpos.EarlyCheckInTime;
                            pos.EditDate = qrfpos.EditDate;
                            pos.EditUser = qrfpos.EditUser;
                            pos.EndTime = qrfpos.EndTime;
                            pos.FromPickUpLoc = qrfpos.FromPickUpLoc;
                            pos.FromPickUpLocID = qrfpos.FromPickUpLocID;
                            pos.InterConnectingRooms = qrfpos.InterConnectingRooms;
                            pos.IsDeleted = qrfpos.IsDeleted;
                            pos.KeepAs = qrfpos.KeepAs;
                            pos.LateCheckOutDate = qrfpos.LateCheckOutDate;
                            pos.LateCheckOutTime = qrfpos.LateCheckOutTime;
                            pos.Location = qrfpos.Location;
                            pos.MealPlan = qrfpos.MealPlan;
                            pos.MealType = qrfpos.MealType;
                            pos.NoOfPaxAdult = qrfpos.NoOfPaxAdult;
                            pos.NoOfPaxChild = qrfpos.NoOfPaxChild;
                            pos.NoOfPaxInfant = qrfpos.NoOfPaxInfant;
                            pos.OPSRemarks = qrfpos.OPSRemarks;
                            pos.PositionId = qrfpos.PositionId;
                            pos.PositionSequence = qrfpos.PositionSequence;
                            pos.ProductAttributeType = qrfpos.ProductAttributeType;
                            pos.ProductID = qrfpos.ProductID;
                            pos.ProductName = qrfpos.ProductName;
                            pos.ProductType = qrfpos.ProductType;
                            pos.ProductTypeId = qrfpos.ProductTypeId;
                            pos.QRFID = qrfpos.QRFID;
                            pos.RoutingDaysID = qrfpos.RoutingDaysID;
                            pos.StandardFOC = qrfpos.StandardFOC;
                            pos.StandardPrice = qrfpos.StandardPrice;
                            pos.StarRating = qrfpos.StarRating;
                            pos.StartingFrom = qrfpos.StartingFrom;
                            pos.StartTime = qrfpos.StartTime;
                            pos.Status = qrfpos.Status;
                            pos.SupplierId = qrfpos.SupplierId;
                            pos.SupplierName = qrfpos.SupplierName;
                            pos.TLRemarks = qrfpos.TLRemarks;
                            pos.ToCityID = qrfpos.ToCityID;
                            pos.ToCityName = qrfpos.ToCityName;
                            pos.ToCountryName = qrfpos.ToCountryName;
                            pos.ToDropOffLoc = qrfpos.ToDropOffLoc;
                            pos.ToDropOffLocID = qrfpos.ToDropOffLocID;
                            pos.TransferDetails = qrfpos.TransferDetails;
                            pos.TypeOfExcursion = qrfpos.TypeOfExcursion;
                            pos.TypeOfExcursion_Id = qrfpos.TypeOfExcursion_Id;
                            pos.WashChangeRooms = qrfpos.WashChangeRooms;
                            pos.IsCityPermit = qrfpos.IsCityPermit;
                            pos.IsParkingCharges = qrfpos.IsParkingCharges;
                            pos.IsRoadTolls = qrfpos.IsRoadTolls;

                            foreach (var qrfroom in qrfpos.RoomDetailsInfo)
                            {
                                var room = new RoomDetailsInfo();
                                room.CreateDate = qrfroom.CreateDate;
                                room.CreateUser = qrfroom.CreateUser;
                                room.EditDate = qrfroom.EditDate;
                                room.EditUser = qrfroom.EditUser;
                                room.IsSupplement = qrfroom.IsSupplement;
                                room.IsDeleted = qrfroom.IsDeleted;
                                room.ProductCategory = qrfroom.ProductCategory;
                                room.ProductCategoryId = qrfroom.ProductCategoryId;
                                room.ProductRange = qrfroom.ProductRange;
                                room.ProductRangeId = qrfroom.ProductRangeId;
                                room.RoomId = qrfroom.RoomId;
                                room.RoomSequence = qrfroom.RoomSequence;

                                pos.RoomDetailsInfo.Add(room);
                            }
                            resultPosition.Add(pos);
                        }
                        response.mPosition = resultPosition;
                    }
                    //else
                    //{
                    //    response.mPosition = new List<mPosition>();
                    //}

                    if ((lstRoutingInfo == null || lstRoutingInfo.Count == 0) && resultPosition.Count > 0)
                    {
                        resultPosition.ForEach(p => { p.IsDeleted = true; p.DeletedFrom = "NoRoutingFound-FromService"; });
                        PositionSetReq positionSetReq = new PositionSetReq { SaveType = "complete", mPosition = resultPosition };
                        PositionSetRes objPositionSetRes = SetPosition(positionSetReq).Result;
                        if (objPositionSetRes != null && response.ResponseStatus.Status.ToLower() == "success")
                        {
                            response.ResponseStatus.Status = "Failure";
                            response.ResponseStatus.ErrorMessage = "No Routing Details found.";
                        }
                        else
                        {
                            response.ResponseStatus.ErrorMessage = "Details not updated.";
                            response.ResponseStatus.Status = "Failure";
                        }
                        response.DaysList = new List<AttributeValues>();
                        response.RoutingInfo = new List<RoutingInfo>();
                        response.RoutingInfo = new List<RoutingInfo>();
                        response.mPosition = new List<mPosition>();
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Success";
                    }
                }
                else
                {
                    response.DaysList = new List<AttributeValues>();
                    response.RoutingInfo = new List<RoutingInfo>();
                    response.mPosition = new List<mPosition>();
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "QRFID is not exists.";
                }

                response.ProductType = request.ProductType;
                response.QRFID = request.QRFID;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return response;
        }

        public async Task<PositionSetRes> SetPosition(PositionSetReq request)
        {
            PositionSetRes response = new PositionSetRes();
            UpdateResult resultFlag = null;
            string LoginUser = request.mPosition[0].EditUser;
            try
            {
                if (request.IsClone == true)
                {
                    var resultQRFQuote = await _MongoContext.mQRFPrice.FindAsync(m => m.QRFID == request.mPosition[0].QRFID && m.IsCurrentVersion).Result.FirstOrDefaultAsync();

                    if (resultQRFQuote != null)
                    {
                        resultFlag = await _MongoContext.mQRFPrice.UpdateOneAsync(Builders<mQRFPrice>.Filter.Eq("QRFPrice_Id", resultQRFQuote.QRFPrice_Id),
                               Builders<mQRFPrice>.Update.Set("RegenerateItinerary", true).Set("EditUser", request.mPosition[0].CreateUser).Set("EditDate", DateTime.Now));

                        resultFlag = await _MongoContext.mQuote.UpdateOneAsync(Builders<mQuote>.Filter.Eq("QRFID", request.mPosition[0].QRFID),
                               Builders<mQuote>.Update.Set("RegenerateItinerary", true).Set("EditUser", request.mPosition[0].CreateUser).Set("EditDate", DateTime.Now));

                        List<mQRFPosition> reqQRFPositionList = ConvertPositionToQRFPosition(request.mPosition);
                        // var mealtype = "";
                        var ProductTypeList = reqQRFPositionList != null && reqQRFPositionList.Count > 0 ? reqQRFPositionList.Select(a => a.ProductType).Distinct().ToList() : new List<string>();

                        var resultQRFposition = _MongoContext.mQRFPosition.AsQueryable().Where(a => a.QRFID == reqQRFPositionList[0].QRFID && a.IsTourEntity == false).ToList();

                        List<string> RangeIdList = new List<string>();
                        List<string> prodcatIdList = new List<string>();

                        if (!string.IsNullOrEmpty(request.SaveType) && request.SaveType.ToLower() == "complete" && (reqQRFPositionList[0].ProductType.ToLower() == "meal" ||
                            reqQRFPositionList[0].ProductType.ToLower() == "private transfer" || reqQRFPositionList[0].ProductType.ToLower() == "scheduled transfer" ||
                            reqQRFPositionList[0].ProductType.ToLower() == "ferry transfer" || reqQRFPositionList[0].ProductType.ToLower() == "ferry passenger"))
                        {
                            if (reqQRFPositionList[0].ProductType.ToLower() == "meal")
                            {
                                resultQRFposition.Where(a => a.ProductType == "Meal" && a.IsDeleted == false).ToList().
                                    ForEach(a =>
                                    {
                                        var rooms = a.RoomDetailsInfo.Where(b => b.IsDeleted == false).ToList();
                                        RangeIdList.AddRange(rooms.Select(b => b.ProductRangeId).ToList());
                                        prodcatIdList.AddRange(rooms.Select(b => b.ProductCategoryId).ToList());
                                    });
                            }
                            else
                            {
                                var prodtypelist = reqQRFPositionList.Select(a => a.ProductType).ToList();
                                resultQRFposition.Where(a => prodtypelist.Contains(a.ProductType) && a.IsDeleted == false).ToList().
                                    ForEach(a =>
                                    {
                                        var rooms = a.RoomDetailsInfo.Where(b => b.IsDeleted == false).ToList();
                                        RangeIdList.AddRange(rooms.Select(b => b.ProductRangeId).ToList());
                                        prodcatIdList.AddRange(rooms.Select(b => b.ProductCategoryId).ToList());
                                    });
                            }
                        }
                        else
                        {
                            reqQRFPositionList.ForEach(a =>
                            {
                                var rooms = a.RoomDetailsInfo.Where(b => b.IsDeleted == false).ToList();
                                RangeIdList.AddRange(rooms.Select(b => b.ProductRangeId).ToList());
                                prodcatIdList.AddRange(rooms.Select(b => b.ProductCategoryId).ToList());
                            });
                        }

                        var lstProductCategories = _MongoContext.Products.AsQueryable().Where(a => a.ProductCategories.Any(b => prodcatIdList.Contains(b.ProductCategory_Id)))
                                     .SelectMany(a => a.ProductCategories).ToList();
                        var ProdRangeList = lstProductCategories.SelectMany(a => a.ProductRanges).Where(a => RangeIdList.Contains(a.ProductRange_Id)).Select(a => new ProductRangeInfo
                        {
                            VoyagerProductRange_Id = a.ProductRange_Id,
                            ProductRangeCode = a.ProductTemplateCode,
                            ProductType_Id = a.PersonType_Id,
                            PersonType = a.PersonType,
                            ProductMenu = a.ProductMenu
                        }).ToList();

                        //var ProdRangeList = _MongoContext.mProductRange.AsQueryable().Where(a => RangeIdList.Contains(a.VoyagerProductRange_Id))
                        //.Select(a => new ProductRangeInfo
                        //{
                        //    VoyagerProductRange_Id = a.VoyagerProductRange_Id,
                        //    ProductRangeCode = a.ProductRangeCode,
                        //    ProductType_Id = a.ProductType_Id,
                        //    PersonType = a.PersonType,
                        //    ProductMenu = a.ProductMenu
                        //}).ToList();

                        var TypeIdList = ProdRangeList.Select(b => b.ProductType_Id).ToList();
                        var mServiceDuration = _MongoContext.mServiceDuration.AsQueryable().Where(a => TypeIdList.Contains(a.ProductTemplate_Id)).ToList();

                        int EndTime = 0;
                        foreach (var position in reqQRFPositionList)
                        {
                            if (position.StartTime.Contains(':'))
                            {
                                var st = position.StartTime.Split(':');
                                if (st[0].Length == 1)
                                {
                                    position.StartTime = "0" + st[0] + ":" + st[1];
                                }
                            }
                            if (position.ProductType.ToLower() == "guide" || position.ProductType.ToLower() == "assistant")
                            {
                                position.EndTime = (position.StartTime.Length >= 5 && position.StartTime.Contains(':')) ?
                                        (Convert.ToInt32(position.StartTime.Split(':')[0]) + 4).ToString() + ":" + position.StartTime.Split(':')[1]
                                        : position.StartTime + 4;
                            }
                            else
                            {
                                var RLFilter = position.RoomDetailsInfo.Where(b => b.IsDeleted == false).Select(a => a.ProductRangeId).ToList();
                                var SDFilter = ProdRangeList.Where(a => RLFilter.Contains(a.VoyagerProductRange_Id)).Select(a => a.ProductType_Id).ToList();
                                var ServiceDuration = mServiceDuration.Where(a => SDFilter.Contains(a.ProductTemplate_Id)).OrderByDescending(a => a.Duration).FirstOrDefault();
                                if (ServiceDuration != null)
                                {
                                    EndTime = ServiceDuration.Duration;
                                    position.EndTime = (position.StartTime.Length >= 5 && position.StartTime.Contains(':')) ?
                                        (Convert.ToInt32(position.StartTime.Split(':')[0]) + EndTime).ToString() + ":" + position.StartTime.Split(':')[1]
                                        : position.StartTime;
                                }
                            }

                            string[] enddt = position.EndTime.Split(":"); 
                            int endTimeHH = Convert.ToInt32(enddt[0]);
                            if (endTimeHH >= 24)
                            {
                                int endtime = endTimeHH - 24;
                                position.EndTime = (endtime >= 9 ? endtime.ToString() : "0" + endtime) + ":" + enddt[1];
                            }
                        }

                        if (resultQRFposition != null && resultQRFposition.Count > 0)
                        {
                            reqQRFPositionList.RemoveAll(f => f.PositionSequence == 0 && string.IsNullOrEmpty(f.PositionId));

                            if (reqQRFPositionList != null && reqQRFPositionList.Count > 0)
                            {
                                mQRFPosition objQRFPosition = new mQRFPosition();

                                foreach (var item in reqQRFPositionList)
                                {
                                    //item.RoomDetailsInfo.OrderBy(a => a.IsSupplement).ThenBy(a => a.ProductRange).ToList().ForEach(a => a.RoomSequence = i++);
                                    if (item.ProductType.ToLower() != "hotel" && item.ProductType.ToLower() != "overnight ferry")
                                    {
                                        item.RoomDetailsInfo = item.RoomDetailsInfo.OrderBy(a => a.IsSupplement).ThenBy(a => a.ProductRange).ToList();
                                    }

                                    //if (!string.IsNullOrEmpty(item.MealType))
                                    //{
                                    //    mealtype = item.MealType.ToLower();
                                    //    item.StartTime = mealtype == "early morning tea" ? "06:00" : mealtype == "breakfast" ? "07:00" : mealtype == "brunch" ? "11:00" : mealtype == "lunch" ? "13:00" : mealtype == "tea" ? "15:00" : mealtype == "dinner" ? "20:00" : item.StartTime;
                                    //    item.EndTime = mealtype == "early morning tea" ? "06:30" : mealtype == "breakfast" ? "07:30" : mealtype == "brunch" ? "11:30" : mealtype == "lunch" ? "13:30" : mealtype == "tea" ? "15:30" : mealtype == "dinner" ? "21:00" : item.EndTime;
                                    //}
                                    if (string.IsNullOrEmpty(item.PositionId) || item.PositionId == Guid.Empty.ToString())
                                    {
                                        objQRFPosition = new mQRFPosition();
                                        item.PositionId = Guid.NewGuid().ToString();
                                        item.CreateDate = DateTime.Now;
                                        item.EditUser = "";
                                        item.EditDate = null;
                                        item.ProductTypeId = item.ProductTypeId;
                                        if (item.RoomDetailsInfo != null && item.RoomDetailsInfo.Count > 0)
                                        {
                                            item.RoomDetailsInfo.ForEach(p =>
                                            {
                                                p.RoomId = Guid.NewGuid().ToString();
                                                p.ProdDesc = ProdRangeList.Where(a => a.VoyagerProductRange_Id == p.ProductRangeId).Count() > 0 ?
                                                             ProdRangeList.Where(a => a.VoyagerProductRange_Id == p.ProductRangeId).FirstOrDefault().ProductMenu : "";
                                            });
                                        }
                                        objQRFPosition = item;
                                        await _MongoContext.mQRFPosition.InsertOneAsync(objQRFPosition);
                                        response.ResponseStatus.Status = "Success";
                                        response.ResponseStatus.ErrorMessage = "Saved Successfully.";

                                        if (item.ProductType.ToLower() == "hotel")
                                        {
                                            await _productRepository.SaveSimilarHotels(item.PositionId, item.ProductID, item.EditUser, true);
                                        }
                                    }
                                    else if (item.ProductType != null && item.Status == "isactive") //&& item.ProductType.ToLower() == "meal"
                                    {
                                        resultFlag = await _MongoContext.mQRFPosition.UpdateOneAsync(Builders<mQRFPosition>.Filter.Eq("PositionId", item.PositionId),
                                            Builders<mQRFPosition>.Update.Set("IsDeleted", item.IsDeleted).Set("EditDate", DateTime.Now).Set("EditUser", item.EditUser));

                                        response.ResponseStatus.Status = resultFlag.MatchedCount > 0 ? "Success" : "Failure";
                                        response.ResponseStatus.ErrorMessage = resultFlag.MatchedCount > 0 ? "Saved Successfully." : "Details not updated.";
                                    }
                                    else
                                    {
                                        //resultFlag = await _MongoContext.mPosition.UpdateOneAsync(Builders<mPosition>.Filter.Eq("PositionId", item.PositionId), Builders<mPosition>.Update.Set("mPosition", item));
                                        var position = resultQRFposition.Where(a => a.PositionId == item.PositionId).FirstOrDefault();
                                        item._Id = position._Id;
                                        item.StandardPrice = position.StandardPrice;
                                        item.StandardFOC = position.StandardFOC;
                                        item.BuyCurrency = position.BuyCurrency;
                                        item.BuyCurrencyId = position.BuyCurrencyId;
                                        item.EditDate = item.IsDeleted ? position.EditDate : DateTime.Now;
                                        item.CreateDate = position.CreateDate;
                                        item.CreateUser = position.CreateUser;

                                        if (item.RoomDetailsInfo != null)
                                        {
                                            item.RoomDetailsInfo.ForEach(p => { p.RoomId = (string.IsNullOrEmpty(p.RoomId) || p.RoomId == "0") ? Guid.NewGuid().ToString() : p.RoomId; });
                                            item.RoomDetailsInfo.FindAll(a => position.RoomDetailsInfo.Exists(b => a.RoomId == b.RoomId)).
                                                ForEach(a =>
                                                {
                                                    //a.CrossPaxSlab = position.RoomDetailsInfo.Where(c => c.RoomId == a.RoomId).FirstOrDefault().CrossPaxSlab;
                                                    a.CrossPositionId = position.RoomDetailsInfo.Where(c => c.RoomId == a.RoomId).FirstOrDefault().CrossPositionId;
                                                });
                                            item.RoomDetailsInfo.AddRange(position.RoomDetailsInfo.Where(p => p.IsDeleted).ToList());

                                            item.RoomDetailsInfo.ForEach(a => a.ProdDesc =
                                                                        ProdRangeList.Where(b => b.VoyagerProductRange_Id == a.ProductRangeId).Count() > 0 ?
                                                                        ProdRangeList.Where(b => b.VoyagerProductRange_Id == a.ProductRangeId).FirstOrDefault().ProductMenu : "");

                                            if (item.ProductType.ToLower() != "hotel" && item.ProductType.ToLower() != "overnight ferry")
                                            {
                                                item.RoomDetailsInfo = item.RoomDetailsInfo.OrderBy(a => a.IsSupplement).ThenBy(a => a.ProductRange).ToList();
                                            }

                                            reqQRFPositionList.Where(a => a.PositionId == item.PositionId).FirstOrDefault().RoomDetailsInfo = item.RoomDetailsInfo.Where(a => !a.IsDeleted).ToList();
                                        }

                                        //_MongoContext.mPosition.FindOneAndReplace(Builders<mPosition>.Filter.Eq("PositionId", item.PositionId), item);
                                        ReplaceOneResult replaceResult = await _MongoContext.mQRFPosition.ReplaceOneAsync(Builders<mQRFPosition>.Filter.Eq("PositionId", item.PositionId), item);

                                        response.ResponseStatus.Status = replaceResult.MatchedCount > 0 ? "Success" : "Failure";
                                        response.ResponseStatus.ErrorMessage = replaceResult.MatchedCount > 0 ? "Saved Successfully." : "Details not updated.";

                                        if (item.ProductType.ToLower() == "hotel")
                                        {
                                            await _productRepository.SaveSimilarHotels(item.PositionId, item.ProductID, item.EditUser, true);
                                        }
                                    }
                                }
                                //var posresult = reqQRFPositionList.FindAll(a => a.ProductType != "Meal" && !GetProductType("transfer").Exists(b => a.ProductType == b.ProdType)); 
                            }
                            else
                            {
                                response.ResponseStatus.Status = "Success";
                                response.ResponseStatus.ErrorMessage = "Details Saved Successfully.";
                            }
                            response.mPosition = ConvertQRFPositionToPosition(reqQRFPositionList);
                        }
                        else
                        {
                            List<mQRFPosition> objQRFPosition = new List<mQRFPosition>();
                            foreach (var m in reqQRFPositionList)
                            {
                                if (m.ProductType.ToLower() != "hotel" && m.ProductType.ToLower() != "overnight ferry")
                                {
                                    m.RoomDetailsInfo = m.RoomDetailsInfo.OrderBy(a => a.IsSupplement).ThenBy(a => a.ProductRange).ToList();
                                }

                                //if (!string.IsNullOrEmpty(m.MealType))
                                //{
                                //    mealtype = m.MealType.ToLower();
                                //    m.StartTime = mealtype == "early morning tea" ? "06:00" : mealtype == "breakfast" ? "07:00" : mealtype == "brunch" ? "11:00" : mealtype == "lunch" ? "13:00" : mealtype == "tea" ? "15:00" : mealtype == "dinner" ? "20:00" : m.StartTime;
                                //    m.EndTime = mealtype == "early morning tea" ? "06:30" : mealtype == "breakfast" ? "07:30" : mealtype == "brunch" ? "11:30" : mealtype == "lunch" ? "13:30" : mealtype == "tea" ? "15:30" : mealtype == "dinner" ? "21:00" : m.EndTime;
                                //}
                                m.CreateDate = DateTime.Now; m.EditDate = null; m.EditUser = "";
                                m.PositionId = Guid.NewGuid().ToString();
                                if (m.RoomDetailsInfo != null && m.RoomDetailsInfo.Count > 0)
                                    m.RoomDetailsInfo.ForEach(d =>
                                    {
                                        d.RoomId = Guid.NewGuid().ToString(); d.CreateDate = DateTime.Now;
                                        d.ProdDesc = ProdRangeList.Where(b => b.VoyagerProductRange_Id == d.ProductRangeId).Count() > 0 ?
                                                                        ProdRangeList.Where(b => b.VoyagerProductRange_Id == d.ProductRangeId).FirstOrDefault().ProductMenu : "";
                                    });

                                objQRFPosition.Add(m);
                            }

                            await _MongoContext.mQRFPosition.InsertManyAsync(objQRFPosition);
                            foreach (var m in objQRFPosition)
                            {
                                if (m.ProductType.ToLower() == "hotel")
                                {
                                    await _productRepository.SaveSimilarHotels(m.PositionId, m.ProductID, m.EditUser, false);
                                }
                            }
                            response.mPosition = ConvertQRFPositionToPosition(objQRFPosition);

                            // var posresult = request.mPosition.FindAll(a => a.ProductType != "Meal" && !GetProductType("transfer").Exists(b => a.ProductType == b.ProdType)); 
                        }

                        if (response.mPosition != null && response.mPosition.Count > 0)
                        {
                            var delPositionIds = response.mPosition.Where(a => a.IsDeleted == true).Select(a => a.PositionId).ToList();
                            if (delPositionIds?.Count > 0)
                            {
                                bool delFlag = await _genericRepository.DeletePositionPriceFOC(delPositionIds, response.mPosition.FirstOrDefault().EditUser, true);
                            }

                            PositionPriceFOCSetRes res = await SetAllPositionPriceFOC(new PositionPriceFOCSetReq
                            {
                                PositionId = response.mPosition != null && response.mPosition.Count == 1 ? response.mPosition.FirstOrDefault().PositionId : "",
                                QRFID = resultQRFQuote.QRFID,
                                IsFOC = request.FOC == "foc" ? true : false,
                                IsPrice = request.Price == "price" ? true : false,
                                ProductRangeInfo = ProdRangeList,
                                ProductTypeList = ProductTypeList,
                                IsClone = request.IsClone,
                                LoginUserId = LoginUser
                            });
                            if (res != null && res.PositionPrice != null && res.ResponseStatus != null && res.ResponseStatus.Status == "Success")
                            {
                                response.ResponseStatus.Status = "Success";
                                response.ResponseStatus.ErrorMessage = "Details Saved Successfully.";
                            }
                        }

                        //if (request.mPosition.Count == 1 && request.mPosition[0].ProductType == "Meal" && request.mPosition[0].ApplyAcrossDays == true)
                        //{
                        //    List<mPosition> lstPosition = await SetPositionApplyAcross(request.mPosition[0], resultQRFQuote.RoutingDays);
                        //    lstPosition.Add(request.mPosition[0]);
                        //    response.PositionDetails = lstPosition.Select(a => new PositionDetails { Days = a.StartingFrom, PositionID = a.PositionId, RoutingDaysID = a.RoutingDaysID }).ToList();
                        //}
                    }
                    else
                    {
                        response.mPosition = new List<mPosition>();
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "QRF ID not exist.";
                    }
                }
                else
                {
                    var resultQuote = await _MongoContext.mQuote.FindAsync(m => m.QRFID == request.mPosition[0].QRFID).Result.FirstOrDefaultAsync();

                    if (resultQuote != null)
                    {
                        if (resultQuote.Departures?.Count > 0)
                        {
                            if (resultQuote.PaxSlabDetails != null && resultQuote.PaxSlabDetails.PaxSlabs?.Count > 0)
                            {
                                var ProductTypeList = request.mPosition != null && request.mPosition.Count > 0 ? request.mPosition.Select(a => a.ProductType).Distinct().ToList() : new List<string>();
                                var resultposition = _MongoContext.mPosition.AsQueryable().Where(a => a.QRFID == request.mPosition[0].QRFID && ProductTypeList.Contains(a.ProductType) && a.IsDeleted == false && a.IsTourEntity == false).ToList();

                                #region Set RegenerateItinerary=true
                                resultFlag = await _MongoContext.mQuote.UpdateOneAsync(Builders<mQuote>.Filter.Eq("QRFID", request.mPosition[0].QRFID),
                                       Builders<mQuote>.Update.Set("RegenerateItinerary", true).Set("EditUser", request.mPosition[0].CreateUser).Set("EditDate", DateTime.Now));
                                #endregion

                                #region fetch RangeId details of request Positions from Products collection and update the EndTime for positions into mPosition collection
                                List<string> RangeIdList = new List<string>();
                                List<string> prodcatIdList = new List<string>();

                                if (!string.IsNullOrEmpty(request.SaveType) && request.SaveType.ToLower() == "complete" && (request.mPosition[0].ProductType.ToLower() == "meal" ||
                                    request.mPosition[0].ProductType.ToLower() == "private transfer" || request.mPosition[0].ProductType.ToLower() == "scheduled transfer" ||
                                    request.mPosition[0].ProductType.ToLower() == "ferry transfer" || request.mPosition[0].ProductType.ToLower() == "ferry passenger"))
                                {
                                    if (request.mPosition[0].ProductType.ToLower() == "meal")
                                    {
                                        resultposition.Where(a => a.ProductType == "Meal" && a.IsDeleted == false).ToList().
                                            ForEach(a =>
                                            {
                                                var rooms = a.RoomDetailsInfo.Where(b => b.IsDeleted == false).ToList();
                                                RangeIdList.AddRange(rooms.Select(b => b.ProductRangeId).ToList());
                                                prodcatIdList.AddRange(rooms.Select(b => b.ProductCategoryId).ToList());
                                            });
                                    }
                                    else
                                    {
                                        // var prodtypelist = request.mPosition.Select(a => a.ProductType).ToList();
                                        resultposition.Where(a => ProductTypeList.Contains(a.ProductType) && a.IsDeleted == false).ToList().
                                            ForEach(a =>
                                            {
                                                var rooms = a.RoomDetailsInfo.Where(b => b.IsDeleted == false).ToList();
                                                RangeIdList.AddRange(rooms.Select(b => b.ProductRangeId).ToList());
                                                prodcatIdList.AddRange(rooms.Select(b => b.ProductCategoryId).ToList());
                                            });
                                    }
                                }
                                else
                                {
                                    request.mPosition.ForEach(a =>
                                    {
                                        var rooms = a.RoomDetailsInfo.Where(b => b.IsDeleted == false).ToList();
                                        RangeIdList.AddRange(rooms.Select(b => b.ProductRangeId).ToList());
                                        prodcatIdList.AddRange(rooms.Select(b => b.ProductCategoryId).ToList());
                                    });
                                }

                                var lstProductCategories = _MongoContext.Products.AsQueryable().Where(a => a.ProductCategories.Any(b => prodcatIdList.Contains(b.ProductCategory_Id)))
                                             .SelectMany(a => a.ProductCategories).ToList();
                                var ProdRangeList = lstProductCategories.SelectMany(a => a.ProductRanges).Where(a => RangeIdList.Contains(a.ProductRange_Id)).Select(a => new ProductRangeInfo
                                {
                                    VoyagerProductRange_Id = a.ProductRange_Id,
                                    ProductRangeCode = a.ProductTemplateCode,
                                    ProductType_Id = a.PersonType_Id,
                                    PersonType = a.PersonType,
                                    ProductMenu = a.ProductMenu
                                }).ToList();

                                //var ProdRangeList = _MongoContext.mProductRange.AsQueryable().Where(a => RangeIdList.Contains(a.VoyagerProductRange_Id))
                                //.Select(a => new ProductRangeInfo
                                //{
                                //    VoyagerProductRange_Id = a.VoyagerProductRange_Id,
                                //    ProductRangeCode = a.ProductRangeCode,
                                //    ProductType_Id = a.ProductType_Id,
                                //    PersonType = a.PersonType,
                                //    ProductMenu = a.ProductMenu
                                //}).ToList();

                                var TypeIdList = ProdRangeList.Select(b => b.ProductType_Id).ToList();
                                var mServiceDuration = _MongoContext.mServiceDuration.AsQueryable().Where(a => TypeIdList.Contains(a.ProductTemplate_Id)).ToList();

                                int EndTime = 0;
                                foreach (var position in request.mPosition)
                                {
                                    if (position.StartTime.Contains(':'))
                                    {
                                        var st = position.StartTime.Split(':');
                                        if (st[0].Length == 1)
                                        {
                                            position.StartTime = "0" + st[0] + ":" + st[1];
                                        }
                                    }
                                    if (position.ProductType.ToLower() == "guide" || position.ProductType.ToLower() == "assistant" || position.ProductType.ToLower() == "train" ||
                                        position.ProductType.ToLower() == "overnight ferry" || position.ProductType.ToLower() == "domestic flight" ||
                                        position.ProductType.ToLower() == "visa" || position.ProductType.ToLower() == "insurance" || position.ProductType.ToLower() == "other" ||
                                        position.ProductType.ToLower() == "fee")
                                    {
                                        if (position.StartTime.Contains(':'))
                                        {
                                            var st = position.StartTime.Split(':');
                                            position.EndTime = (Convert.ToInt32(st[0]) + 4).ToString() + ":" + st[1];
                                        }
                                        else
                                        {
                                            position.EndTime = position.StartTime;
                                        }
                                        //position.EndTime = (position.StartTime.Length >= 5 && position.StartTime.Contains(':')) ?
                                        //    (Convert.ToInt32(position.StartTime.Split(':')[0]) + 4).ToString() + ":" + position.StartTime.Split(':')[1]
                                        //    : position.StartTime + 4;
                                    }
                                    else
                                    {
                                        var RLFilter = position.RoomDetailsInfo.Where(b => b.IsDeleted == false).Select(a => a.ProductRangeId).ToList();
                                        var SDFilter = ProdRangeList.Where(a => RLFilter.Contains(a.VoyagerProductRange_Id)).Select(a => a.ProductType_Id).ToList();
                                        var ServiceDuration = mServiceDuration.Where(a => SDFilter.Contains(a.ProductTemplate_Id)).OrderByDescending(a => a.Duration).FirstOrDefault();
                                        if (ServiceDuration != null)
                                        {
                                            EndTime = ServiceDuration.Duration;
                                            if (position.StartTime.Contains(':'))
                                            {
                                                var st = position.StartTime.Split(':');
                                                position.EndTime = (Convert.ToInt32(st[0]) + EndTime).ToString() + ":" + st[1];
                                            }
                                            else
                                            {
                                                position.EndTime = position.StartTime;
                                            }

                                            //position.EndTime = (position.StartTime.Length >= 5 && position.StartTime.Contains(':')) ?
                                            //    (Convert.ToInt32(position.StartTime.Split(':')[0]) + EndTime).ToString() + ":" + position.StartTime.Split(':')[1]
                                            //    : position.StartTime;
                                        }
                                    }

                                    string[] enddt = position.EndTime.Split(":");
                                    int endTimeHH = Convert.ToInt32(enddt[0]);
                                    if (endTimeHH >= 24)
                                    {
                                        int endtime = endTimeHH - 24;
                                        position.EndTime = (endtime >= 9 ? endtime.ToString() : "0" + endtime) + ":" + enddt[1];
                                    }
                                }
                                #endregion

                                #region Update/Insert the position details into mPosition collection
                                if (resultposition != null && resultposition.Count > 0)
                                {
                                    request.mPosition.RemoveAll(f => f.PositionSequence == 0 && string.IsNullOrEmpty(f.PositionId));

                                    if (request.mPosition != null && request.mPosition.Count > 0)
                                    {
                                        mPosition objPosition = new mPosition();

                                        foreach (var item in request.mPosition)
                                        {
                                            if (item.ProductType.ToLower() != "hotel" && item.ProductType.ToLower() != "overnight ferry")
                                            {
                                                item.RoomDetailsInfo = item.RoomDetailsInfo.OrderBy(a => a.IsSupplement).ThenBy(a => a.ProductRange).ToList();
                                            }
                                            if (string.IsNullOrEmpty(item.PositionId) || item.PositionId == Guid.Empty.ToString())
                                            {
                                                objPosition = new mPosition();
                                                item.PositionId = Guid.NewGuid().ToString();
                                                item.CreateDate = DateTime.Now;
                                                item.EditUser = "";
                                                item.EditDate = null;
                                                item.ProductTypeId = item.ProductTypeId;
                                                if (item.RoomDetailsInfo != null && item.RoomDetailsInfo.Count > 0)
                                                {
                                                    item.RoomDetailsInfo.ForEach(p =>
                                                    {
                                                        p.RoomId = Guid.NewGuid().ToString();
                                                        p.ProdDesc = ProdRangeList.Where(a => a.VoyagerProductRange_Id == p.ProductRangeId).Count() > 0 ?
                                                                     ProdRangeList.Where(a => a.VoyagerProductRange_Id == p.ProductRangeId).FirstOrDefault().ProductMenu : "";
                                                    });
                                                }
                                                objPosition = item;
                                                await _MongoContext.mPosition.InsertOneAsync(objPosition);
                                                response.ResponseStatus.Status = "Success";
                                                response.ResponseStatus.ErrorMessage = "Saved Successfully.";
                                                if (item.ProductType.ToLower() == "hotel")
                                                {
                                                    await _productRepository.SaveSimilarHotels(item.PositionId, item.ProductID, item.EditUser, false);
                                                }
                                            }
                                            else if (item.ProductType != null && item.Status == "isactive") //&& item.ProductType.ToLower() == "meal"
                                            {
                                                resultFlag = await _MongoContext.mPosition.UpdateOneAsync(Builders<mPosition>.Filter.Eq("PositionId", item.PositionId),
                                                    Builders<mPosition>.Update.Set("IsDeleted", item.IsDeleted).Set("EditDate", DateTime.Now).Set("EditUser", item.EditUser));

                                                item.StartTime = resultposition.Where(a => a.PositionId == item.PositionId).FirstOrDefault().StartTime;
                                                response.ResponseStatus.Status = resultFlag.MatchedCount > 0 ? "Success" : "Failure";
                                                response.ResponseStatus.ErrorMessage = resultFlag.MatchedCount > 0 ? "Saved Successfully." : "Details not updated.";
                                            }
                                            else
                                            {
                                                var position = resultposition.Where(a => a.PositionId == item.PositionId).FirstOrDefault();
                                                item._Id = position._Id;
                                                item.StandardPrice = position.StandardPrice;
                                                item.StandardFOC = position.StandardFOC;
                                                item.BuyCurrency = position.BuyCurrency;
                                                item.BuyCurrencyId = position.BuyCurrencyId;
                                                item.EditDate = item.IsDeleted ? position.EditDate : DateTime.Now;
                                                item.CreateDate = position.CreateDate;
                                                item.CreateUser = position.CreateUser;
                                                item.AlternateHotels = position.AlternateHotels;
                                                item.AlternateHotelsParameter = position.AlternateHotelsParameter;

                                                if (item.RoomDetailsInfo != null)
                                                {
                                                    item.RoomDetailsInfo.ForEach(p => { p.RoomId = (string.IsNullOrEmpty(p.RoomId) || p.RoomId == "0") ? Guid.NewGuid().ToString() : p.RoomId; });
                                                    item.RoomDetailsInfo.FindAll(a => position.RoomDetailsInfo.Exists(b => a.RoomId == b.RoomId)).
                                                        ForEach(a =>
                                                        {
                                                            //a.CrossPaxSlab = position.RoomDetailsInfo.Where(c => c.RoomId == a.RoomId).FirstOrDefault().CrossPaxSlab;
                                                            a.CrossPositionId = position.RoomDetailsInfo.Where(c => c.RoomId == a.RoomId).FirstOrDefault().CrossPositionId;
                                                        });
                                                    item.RoomDetailsInfo.AddRange(position.RoomDetailsInfo.Where(p => p.IsDeleted).ToList());

                                                    item.RoomDetailsInfo.ForEach(a => a.ProdDesc =
                                                                                ProdRangeList.Where(b => b.VoyagerProductRange_Id == a.ProductRangeId).Count() > 0 ?
                                                                                ProdRangeList.Where(b => b.VoyagerProductRange_Id == a.ProductRangeId).FirstOrDefault().ProductMenu : "");

                                                    if (item.ProductType.ToLower() != "hotel" && item.ProductType.ToLower() != "overnight ferry")
                                                    {
                                                        item.RoomDetailsInfo = item.RoomDetailsInfo.OrderBy(a => a.IsSupplement).ThenBy(a => a.ProductRange).ToList();
                                                    }

                                                    request.mPosition.Where(a => a.PositionId == item.PositionId).FirstOrDefault().RoomDetailsInfo = item.RoomDetailsInfo.Where(a => !a.IsDeleted).ToList();
                                                }

                                                ReplaceOneResult replaceResult = await _MongoContext.mPosition.ReplaceOneAsync(Builders<mPosition>.Filter.Eq("PositionId", item.PositionId), item);

                                                response.ResponseStatus.Status = replaceResult.MatchedCount > 0 ? "Success" : "Failure";
                                                response.ResponseStatus.ErrorMessage = replaceResult.MatchedCount > 0 ? "Saved Successfully." : "Details not updated.";

                                                if (item.ProductType.ToLower() == "hotel")
                                                {
                                                    await _productRepository.SaveSimilarHotels(item.PositionId, item.ProductID, item.EditUser, false);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        response.ResponseStatus.Status = "Success";
                                        response.ResponseStatus.ErrorMessage = "Details Saved Successfully.";
                                    }
                                    response.mPosition = request.mPosition;
                                }
                                else
                                {
                                    List<mPosition> objPosition = new List<mPosition>();
                                    foreach (var m in request.mPosition)
                                    {
                                        if (m.ProductType.ToLower() != "hotel" && m.ProductType.ToLower() != "overnight ferry")
                                        {
                                            m.RoomDetailsInfo = m.RoomDetailsInfo.OrderBy(a => a.IsSupplement).ThenBy(a => a.ProductRange).ToList();
                                        }
                                        m.CreateDate = DateTime.Now; m.EditDate = null; m.EditUser = "";
                                        m.PositionId = Guid.NewGuid().ToString();
                                        if (m.RoomDetailsInfo != null && m.RoomDetailsInfo.Count > 0)
                                            m.RoomDetailsInfo.ForEach(d =>
                                            {
                                                d.RoomId = Guid.NewGuid().ToString(); d.CreateDate = DateTime.Now;
                                                d.ProdDesc = ProdRangeList.Where(b => b.VoyagerProductRange_Id == d.ProductRangeId).Count() > 0 ?
                                                                                ProdRangeList.Where(b => b.VoyagerProductRange_Id == d.ProductRangeId).FirstOrDefault().ProductMenu : "";
                                            });

                                        objPosition.Add(m);
                                    }

                                    await _MongoContext.mPosition.InsertManyAsync(objPosition);

                                    foreach (var m in objPosition)
                                    {
                                        if (m.ProductType.ToLower() == "hotel")
                                        {
                                            await _productRepository.SaveSimilarHotels(m.PositionId, m.ProductID, m.EditUser, false);
                                        }
                                    }
                                    response.mPosition = objPosition;
                                    response.ResponseStatus.Status = "Success";
                                    response.ResponseStatus.ErrorMessage = "Details Saved Successfully.";
                                }
                                #endregion

                                #region If positions are deleted then delete its PRICE and FOC and UPSERT the Position Price/FOC into  mPositionPrice/mPositionFOC collection respectivly
                                if (response.mPosition != null && response.mPosition.Count > 0)
                                {
                                    var delPositionIds = response.mPosition.Where(a => a.IsDeleted == true).Select(a => a.PositionId).ToList();
                                    if (delPositionIds?.Count > 0)
                                    {
                                        bool delFlag = await _genericRepository.DeletePositionPriceFOC(delPositionIds, response.mPosition.FirstOrDefault().EditUser);
                                    }

                                    PositionPriceFOCSetRes res = await SetAllPositionPriceFOC(new PositionPriceFOCSetReq
                                    {
                                        PositionId = response.mPosition != null && response.mPosition.Count == 1 ? response.mPosition.FirstOrDefault().PositionId : "",
                                        QRFID = resultQuote.QRFID,
                                        IsFOC = request.FOC == "foc" ? true : false,
                                        IsPrice = request.Price == "price" ? true : false,
                                        ProductRangeInfo = ProdRangeList,
                                        ProductTypeList = ProductTypeList,
                                        LoginUserId = LoginUser
                                    });

                                    if (res != null && res.PositionPrice != null && res.ResponseStatus?.Status == "Success")
                                    {
                                        response.ResponseStatus.Status = "Success";
                                        response.ResponseStatus.ErrorMessage = "Details Saved Successfully.";
                                    }
                                }
                                #endregion

                                #region If ApplyAcrossDays = true for Meal ProductType then copies the position details to other days
                                if (request.mPosition.Count == 1 && request.mPosition[0].ProductType == "Meal" && request.mPosition[0].ApplyAcrossDays == true)
                                {
                                    List<mPosition> lstPosition = await SetPositionApplyAcross(request.mPosition[0], resultQuote.RoutingDays);
                                    lstPosition.Add(request.mPosition[0]);
                                    response.PositionDetails = lstPosition.Select(a => new PositionDetails { Days = a.StartingFrom, PositionID = a.PositionId, RoutingDaysID = a.RoutingDaysID, ProductID = a.ProductID }).ToList();
                                }
                                #endregion

                                #region Guide/ LDC/ Coach positions handled in Tour Entity 
                                //1)If we delete Guide/ LDC/ Coach positions then delete its Product Range from Hotels and Meals Positions
                                //2)if we decrement the duration from Guide/ LDC/ Coach positions, then removed the DRIVER and GUIDE from MEALs and Acco positions 

                                List<string> lstHMProdType = new List<string>() { "Hotel", "Meal" };
                                var builderPT = Builders<mProductType>.Filter;
                                var filterPT = builderPT.Where(q => q.ChargeBasis == "PUPD");
                                var resultPT = await _MongoContext.mProductType.Find(filterPT).Project(q => new mProductType { Prodtype = q.Prodtype, Name = q.Name }).ToListAsync();

                                if (resultPT != null && resultPT.Count > 0)
                                {
                                    List<string> lstStr = resultPT.Select(a => a.Prodtype).ToList();
                                    var resultPos = _MongoContext.mPosition.AsQueryable().Where(a => a.QRFID == request.mPosition[0].QRFID &&
                                               ((lstStr.Contains(a.ProductType)) || (lstHMProdType.Contains(a.ProductType) && a.IsDeleted == false)) && a.IsTourEntity == false).ToList();

                                    if (resultPos?.Count > 0)
                                    {
                                        #region 1)If we delete PUPD chargeBasis ProductType positions then delete its Product Range from Hotels and Meals Positions
                                        var posids = resultPos.Where(a => lstStr.Contains(a.ProductType) && a.IsDeleted).Select(a => a.PositionId).ToList();
                                        if (posids?.Count > 0)
                                        {
                                            var accomealspos = resultPos.Where(a => lstHMProdType.Contains(a.ProductType)).ToList();
                                            if (accomealspos?.Count > 0)
                                            {
                                                foreach (var item in accomealspos)
                                                {
                                                    item.RoomDetailsInfo.Where(a => posids.Contains(a.CrossPositionId)).ToList().ForEach(a =>
                                                    {
                                                        a.IsDeleted = true;
                                                        a.EditDate = DateTime.Now; a.EditUser = request.mPosition[0].CreateUser;
                                                    });
                                                    resultFlag = await _MongoContext.mPosition.UpdateOneAsync(Builders<mPosition>.Filter.Eq("PositionId", item.PositionId),
                                                    Builders<mPosition>.Update.Set("RoomDetailsInfo", item.RoomDetailsInfo).Set("EditUser", request.mPosition[0].CreateUser).Set("EditDate", DateTime.Now));
                                                }
                                            }
                                        }
                                        #endregion

                                        #region 2)if we decrement the duration from Guide/ LDC/ Coach positions, then removed the DRIVER and GUIDE from MEALs and Acco positions                                
                                        var PosList = resultposition.Where(a => lstStr.Contains(a.ProductType) && a.IsDeleted == false).Distinct().ToList();
                                        if (PosList?.Count > 0)
                                        {
                                            var mealacco = resultPos.Where(a => lstHMProdType.Contains(a.ProductType)).Distinct().ToList();

                                            //fetch the PUPD ChargeBasis product types from mPosition collection
                                            var mPos = resultPos.Where(a => lstStr.Contains(a.ProductType) && a.IsDeleted == false).Distinct().ToList();
                                            foreach (var objPos in PosList)
                                            {
                                                var pos = mPos.Where(a => a.PositionId == objPos.PositionId).FirstOrDefault();
                                                //check if Position of old durtion is > than same Position of updated duration
                                                if (objPos.Duration > pos?.Duration)
                                                {
                                                    int? dur = objPos.Duration - pos.Duration;
                                                    int duration = pos.DayNo == 1 ? (pos.Duration - 1) : pos.Duration;
                                                    foreach (var item in mealacco)
                                                    {
                                                        if (!(item.DayNo >= pos.DayNo && item.DayNo <= duration))
                                                        {
                                                            var roomids = item.RoomDetailsInfo.FindAll(a => a.CrossPositionId == objPos.PositionId).Select(b => b.RoomId).ToList();
                                                            if (roomids?.Count > 0)
                                                            {
                                                                int removedcnt = item.RoomDetailsInfo.RemoveAll(a => roomids.Contains(a.RoomId));
                                                                if (removedcnt > 0)
                                                                {
                                                                    resultFlag = await _MongoContext.mPosition.UpdateOneAsync(Builders<mPosition>.Filter.Eq("PositionId", item.PositionId),
                                                                                 Builders<mPosition>.Update.Set("RoomDetailsInfo", item.RoomDetailsInfo).Set("EditUser", request.mPosition[0].CreateUser).Set("EditDate", DateTime.Now));

                                                                    resultFlag = await _MongoContext.mPositionPrice.UpdateManyAsync(Builders<mPositionPrice>.Filter.Where(a => a.PositionId == item.PositionId && roomids.Contains(a.RoomId)),
                                                                                Builders<mPositionPrice>.Update.Set("IsDeleted", true).Set("EditUser", request.mPosition[0].CreateUser).Set("EditDate", DateTime.Now));
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        #endregion
                                    }
                                }
                                #endregion

                            }
                            else
                            {
                                response.mPosition = new List<mPosition>();
                                response.ResponseStatus.Status = "Failure";
                                response.ResponseStatus.ErrorMessage = "Please Enter Pax Slab Details.";
                            }
                        }
                        else
                        {
                            response.mPosition = new List<mPosition>();
                            response.ResponseStatus.Status = "Failure";
                            response.ResponseStatus.ErrorMessage = "Please Enter Departures Details.";
                        }
                    }
                    else
                    {
                        response.mPosition = new List<mPosition>();
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "QRF ID not exist.";
                    }
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }
            return response;
        }

        public async Task<bool> UpdateItineraryRecords(string QRFId, List<mPosition> resultposition)
        {
            bool flag = false;
            var resultItinerary = _MongoContext.mItinerary.AsQueryable().Where(a => a.QRFID == QRFId).FirstOrDefault();

            if (resultItinerary != null)
            {
                List<string> listRangeId = new List<string>();
                mProductRange productRange;
                string ProductRangeName = "";
                //resultposition = resultposition.Where(q => q.ProductType.ToLower() != "assistant").ToList();
                resultposition = resultposition.Where(q => q.IsTourEntity == false).ToList();
                resultposition.ForEach(a => listRangeId.AddRange(a.RoomDetailsInfo.Select(b => b.ProductRangeId).ToList()));
                var ProdRangeLst = _MongoContext.mProductRange.AsQueryable().Where(a => listRangeId.Contains(a.VoyagerProductRange_Id));
                var products = _MongoContext.mProducts.AsQueryable().ToList();

                foreach (var a in resultItinerary.ItineraryDays)
                {
                    foreach (var pos in a.ItineraryDescription)
                    {
                        foreach (var item in resultposition)
                        {
                            if (pos.PositionId == item.PositionId)
                            {
                                //Update Position in Itinerary
                                pos.TLRemarks = item.TLRemarks;
                                pos.OPSRemarks = item.OPSRemarks;
                                pos.IsDeleted = item.IsDeleted;

                                pos.ProductId = string.IsNullOrEmpty(item.ProductID) ? pos.ProductId : item.ProductID;
                                pos.ProductName = string.IsNullOrEmpty(item.ProductName) ? pos.ProductName : item.ProductName;
                                pos.StartTime = string.IsNullOrEmpty(item.StartTime) ? pos.StartTime : item.StartTime;
                                pos.EndTime = string.IsNullOrEmpty(item.EndTime) ? pos.EndTime : item.EndTime;
                                pos.City = string.IsNullOrEmpty(item.CityName) ? pos.City : item.CityName;
                                pos.KeepAs = string.IsNullOrEmpty(item.KeepAs) ? pos.KeepAs : item.KeepAs;
                                pos.ProductDescription = products != null ? products.Where(x => x.VoyagerProduct_Id == item.ProductID).Select(y => y.ProdDesc).FirstOrDefault() : "";
                                foreach (var room in item.RoomDetailsInfo)
                                {
                                    productRange = ProdRangeLst.Where(x => x.VoyagerProductRange_Id == room.ProductRangeId).FirstOrDefault();
                                    if (productRange != null) ProductRangeName = productRange.ProductRangeName;

                                    pos.RoomDetails.Add(new RoomInfo
                                    {
                                        ProductRangeId = room.ProductRangeId,
                                        ProductRange = room.ProductRange,
                                        ProdDesc = room.ProdDesc,
                                        RangeDesc = ProductRangeName
                                    });
                                }

                                pos.EditDate = DateTime.Now;
                                pos.EditUser = item.CreateUser;
                            }
                            else
                            {
                                //Add Position in Itinerary
                                ItineraryDescriptionInfo newobj = new ItineraryDescriptionInfo();

                                newobj.PositionId = item.PositionId;
                                newobj.City = item.CityName;
                                newobj.ProductType = item.ProductType;
                                newobj.Type = "Service";
                                newobj.ProductId = item.ProductID;
                                newobj.ProductName = item.ProductName;
                                newobj.StartTime = item.StartTime;
                                newobj.EndTime = item.EndTime;
                                newobj.NumberOfPax = item.NoOfPaxAdult;
                                newobj.KeepAs = item.KeepAs;
                                newobj.ProductDescription = products != null ? products.Where(x => x.VoyagerProduct_Id == item.ProductID).Select(y => y.ProdDesc).FirstOrDefault() : "";
                                newobj.Duration = item.Duration;
                                newobj.TLRemarks = item.TLRemarks;
                                newobj.OPSRemarks = item.OPSRemarks;
                                newobj.Supplier = item.SupplierName;
                                newobj.Allocation = null;
                                newobj.IsRoutingMatrix = false;
                                newobj.IsDeleted = item.IsDeleted;
                                newobj.CreateDate = DateTime.Now;
                                newobj.CreateUser = item.CreateUser;

                                a.ItineraryDescription.Add(newobj);
                            }
                        }
                    }
                }
                await _MongoContext.mItinerary.UpdateOneAsync(Builders<mItinerary>.Filter.Eq("ItineraryID", resultItinerary.ItineraryID),
                          Builders<mItinerary>.Update.Set("ItineraryDays", resultItinerary.ItineraryDays));
                flag = true;
            }

            return flag;
        }

        public List<ProductType> GetProductType(string strProdType)
        {
            List<ProductType> lst = new List<ProductType>();
            if (strProdType == "transfer")
            {
                lst.Add(new ProductType { ProdType = "Scheduled Transfer" });
                lst.Add(new ProductType { ProdType = "Private Transfer" });
                lst.Add(new ProductType { ProdType = "Ferry Passenger" });
                lst.Add(new ProductType { ProdType = "Ferry Transfer" });
            }
            return lst;
        }
        #endregion

        #region Meal 
        public PositionGetRes GetMealGridDetails(List<RoutingDays> RoutingDays, List<mPosition> mPosition, string QRFID)
        {
            PositionGetRes response = new PositionGetRes() { QRFID = QRFID, mPosition = mPosition, RoutingDays = RoutingDays };
            var objresultPosition = new List<mPosition>();
            List<MealDetails> lstMealDetails = new List<MealDetails>();

            List<mPosition> lstAccoPosition = _MongoContext.mPosition.AsQueryable().Where(q => q.QRFID == QRFID && q.ProductType == "Hotel" && q.IsDeleted == false).Select(q => q).ToList();
            List<mPosition> AccomodationDetails = lstAccoPosition != null && lstAccoPosition.Count > 0 ? lstAccoPosition : new List<mPosition>();

            //List<mProductLevelAttribute> ProductLevelAttribute = new List<mProductLevelAttribute>();
            //if (lstAccoPosition != null && lstAccoPosition.Count > 0)
            //{
            //    var prodlist = lstAccoPosition.Select(p => p.ProductID).Distinct().ToList();
            //    var product = _MongoContext.mProductLevelAttribute.AsQueryable().Where(p => prodlist.Contains(p.Product_Id)
            //                  && (p.AttributeName.ToLower() == "check in" || p.AttributeName.ToLower() == "check out")).ToList();
            //    ProductLevelAttribute = product != null && product.Count > 0 ? product : new List<mProductLevelAttribute>();
            //}

            foreach (var item in RoutingDays)
            {
                objresultPosition = mPosition.Where(a => a.RoutingDaysID == item.RoutingDaysID).ToList();
                if (objresultPosition != null && objresultPosition.Count > 0)
                {
                    foreach (var pos in objresultPosition)
                    {
                        if (lstMealDetails.Where(a => a.RoutingDaysID == item.RoutingDaysID).Count() > 0)
                        {
                            AddMealList(pos, lstMealDetails, item);
                        }
                        else
                        {
                            lstMealDetails.Add(AddMealDetails(pos, item));
                        }
                    }
                }
                else
                {
                    lstMealDetails.Add(AddMealDetails(null, item));
                }
            }

            lstMealDetails = lstMealDetails.OrderBy(m => m.DayNo).ToList();
            //lstMealDetails = CheckDefaultMealPlan(lstAccoPosition, ProductLevelAttribute, lstMealDetails);
            lstMealDetails = CheckDefaultMealPlan(lstAccoPosition, lstMealDetails);
            response.MealDetails = lstMealDetails;
            return response;
        }

        public MealDetails AddMealDetails(mPosition objresultPosition, RoutingDays item)
        {
            objresultPosition = objresultPosition == null ? new mPosition() { MealType = "", MealPlan = "", PositionId = "", IsDeleted = false } : objresultPosition;

            MealDetails objMealDetails = new MealDetails()
            {
                DefaultPlan = objresultPosition.MealPlan,
                BreakfastId = objresultPosition.MealType.ToLower() == "breakfast" ? objresultPosition.PositionId : "",
                EarlyMorningTeaId = objresultPosition.MealType.ToLower() == "early morning tea" ? objresultPosition.PositionId : "",
                BrunchId = objresultPosition.MealType.ToLower() == "brunch" ? objresultPosition.PositionId : "",
                TeaId = objresultPosition.MealType.ToLower() == "tea" ? objresultPosition.PositionId : "",
                DinnerId = objresultPosition.MealType.ToLower() == "dinner" ? objresultPosition.PositionId : "",
                LunchId = objresultPosition.MealType.ToLower() == "lunch" ? objresultPosition.PositionId : "",
                IsBreakfast = objresultPosition.MealType.ToLower() == "breakfast" && !objresultPosition.IsDeleted ? true : false,
                IsBrunch = objresultPosition.MealType.ToLower() == "brunch" && !objresultPosition.IsDeleted ? true : false,
                IsDinner = objresultPosition.MealType.ToLower() == "dinner" && !objresultPosition.IsDeleted ? true : false,
                IsEarlyMorningTea = objresultPosition.MealType.ToLower() == "early morning tea" && !objresultPosition.IsDeleted ? true : false,
                IsLunch = objresultPosition.MealType.ToLower() == "lunch" && !objresultPosition.IsDeleted ? true : false,
                IsTea = objresultPosition.MealType.ToLower() == "tea" && !objresultPosition.IsDeleted ? true : false,
                DayID = item.Days,
                DayNo = item.DayNo,
                IsDeleted = false,
                PositionSequence = item.RouteSequence,
                RoutingCity = item.GridLabel, //item.FromCityName.Trim(),
                RoutingDaysID = item.RoutingDaysID
            };
            return objMealDetails;
        }

        public List<MealDetails> AddMealList(mPosition objresultPosition, List<MealDetails> lstMealDetails, RoutingDays item)
        {
            lstMealDetails.Where(m => m.RoutingDaysID == item.RoutingDaysID).ToList().ForEach(m =>
            {
                m.DefaultPlan = objresultPosition.MealPlan;
                if (objresultPosition.MealType.ToLower() == "breakfast" && string.IsNullOrEmpty(m.BreakfastId))
                {
                    m.BreakfastId = objresultPosition.PositionId;
                    m.IsBreakfast = !objresultPosition.IsDeleted ? true : false;
                }

                if (objresultPosition.MealType.ToLower() == "early morning tea" && string.IsNullOrEmpty(m.EarlyMorningTeaId))
                {
                    m.EarlyMorningTeaId = objresultPosition.PositionId;
                    m.IsEarlyMorningTea = !objresultPosition.IsDeleted ? true : false;
                }

                if (objresultPosition.MealType.ToLower() == "brunch" && string.IsNullOrEmpty(m.BrunchId))
                {
                    m.BrunchId = objresultPosition.PositionId;
                    m.IsBrunch = !objresultPosition.IsDeleted ? true : false;
                }

                if (objresultPosition.MealType.ToLower() == "tea" && string.IsNullOrEmpty(m.TeaId))
                {
                    m.TeaId = objresultPosition.PositionId;
                    m.IsTea = !objresultPosition.IsDeleted ? true : false;
                }

                if (objresultPosition.MealType.ToLower() == "dinner" && string.IsNullOrEmpty(m.DinnerId))
                {
                    m.DinnerId = objresultPosition.PositionId;
                    m.IsDinner = !objresultPosition.IsDeleted ? true : false;
                }

                if (objresultPosition.MealType.ToLower() == "lunch" && string.IsNullOrEmpty(m.LunchId))
                {
                    m.LunchId = objresultPosition.PositionId;
                    m.IsLunch = !objresultPosition.IsDeleted ? true : false;
                }
            });

            return lstMealDetails;
        }

        //public List<MealDetails> CheckDefaultMealPlan(List<mPosition> lstAccoPosition, List<mProductLevelAttribute> lstProduct, List<MealDetails> lstMealDetails)
        public List<MealDetails> CheckDefaultMealPlan(List<mPosition> lstAccoPosition, List<MealDetails> lstMealDetails)
        {
            if (lstAccoPosition != null && lstAccoPosition.Count > 0)
            {
                lstAccoPosition = lstAccoPosition.Where(a => a.IsDeleted == false).OrderBy(a => a.DayNo).ToList();

                lstAccoPosition = lstAccoPosition.GroupBy(a => a.DayNo).Select(a => a.FirstOrDefault()).ToList();

                var prodlist = lstAccoPosition.Select(p => p.ProductID).Distinct().ToList();
                //List<mProductLevelAttribute> lstProdAttr = new List<mProductLevelAttribute>();

                if (prodlist != null && prodlist.Count > 0)
                {
                    TimeSpan? timeoutprev = null; TimeSpan? newtimeout = null;
                    string strPrevPlan = "";

                    mPosition res = new mPosition();
                    int nextday = 0;
                    int curday = 0;
                    int i = 0;

                    string strchkin = "";
                    string strchkout = "";

                    //1st time take curday & nextday in variable
                    res = lstAccoPosition[0];
                    curday = lstAccoPosition[0].DayNo;
                    nextday = lstAccoPosition[0].DayNo + lstAccoPosition[0].Duration;

                    foreach (var item in lstMealDetails)
                    {
                        if (item.DayNo >= curday) //if meal day >= accom day  
                        {
                            if (item.DayNo <= nextday)
                            {
                                if (item.DayNo == nextday)
                                {
                                    i++;
                                    if (lstAccoPosition.Count > i)
                                    {
                                        res = lstAccoPosition[i];
                                        curday = lstAccoPosition[i].DayNo;
                                        nextday = lstAccoPosition[i].DayNo + lstAccoPosition[i].Duration;
                                    }
                                }
                                else
                                {
                                    //res = lstAccoPosition.Where(a => a.DayNo >= item.DayNo && a.Duration <= item.DayNo).FirstOrDefault();
                                    res = lstAccoPosition[i];
                                }
                            }
                            else
                            {
                                res = null;
                            }
                        }
                        else
                        {
                            res = null;
                        }

                        if (newtimeout != null)
                        {
                            timeoutprev = newtimeout;
                        }

                        if (res != null && !string.IsNullOrEmpty(res.MealPlan))
                        {
                            TimeSpan timein, timeout;
                            if (item.DayNo >= curday) //if meal day >= accom day
                            {
                                if (res.MealPlan.ToLower() == "nb") //if No board then Check only CheckOut Time for the Meal day(item.DayNo)
                                {
                                    bool flag = false;
                                    if (newtimeout != null && timeoutprev != null && !string.IsNullOrEmpty(strPrevPlan))
                                    {
                                        item.DefaultPlan = strPrevPlan;
                                        if (strPrevPlan.ToLower() == "fb")
                                        {
                                            if (timeoutprev != null && timeoutprev >= new TimeSpan(07, 30, 00) && string.IsNullOrEmpty(item.BreakfastId))
                                            {
                                                item.Breakfast = "Included in Hotel"; flag = true;
                                            }
                                            //if (timeoutprev != null && timeoutprev >= new TimeSpan(13, 30, 00))                                            
                                            if (string.IsNullOrEmpty(item.LunchId))
                                            {
                                                item.Lunch = "Included in Hotel"; flag = true;
                                            }

                                            if (timeoutprev != null && timeoutprev >= new TimeSpan(21, 00, 00) && string.IsNullOrEmpty(item.DinnerId))
                                            { item.Dinner = "Included in Hotel"; flag = true; }

                                        }
                                        else if (strPrevPlan.ToLower() == "bb")
                                        {
                                            if (timeoutprev != null && timeoutprev >= new TimeSpan(07, 30, 00) && string.IsNullOrEmpty(item.BreakfastId))
                                            {
                                                item.Breakfast = "Included in Hotel"; flag = true;
                                            }
                                        }
                                        else if (strPrevPlan.ToLower() == "hb")
                                        {
                                            if (timeoutprev != null && timeoutprev >= new TimeSpan(07, 30, 00) && string.IsNullOrEmpty(item.BreakfastId))
                                            {
                                                item.Breakfast = "Included in Hotel"; flag = true;
                                            }
                                            if (timeoutprev != null && timeoutprev >= new TimeSpan(21, 00, 00) && string.IsNullOrEmpty(item.DinnerId))
                                            { item.Dinner = "Included in Hotel"; flag = true; }
                                        }
                                        else if (strPrevPlan.ToLower() == "hl")
                                        {
                                            if (timeoutprev != null && timeoutprev >= new TimeSpan(07, 30, 00) && string.IsNullOrEmpty(item.BreakfastId))
                                            {
                                                item.Breakfast = "Included in Hotel"; flag = true;
                                            }
                                            //if (timeoutprev != null && timeoutprev >= new TimeSpan(13, 30, 00))                                            
                                            if (string.IsNullOrEmpty(item.LunchId))
                                            {
                                                item.Lunch = "Included in Hotel"; flag = true;
                                            }
                                        }
                                        else if (strPrevPlan.ToLower() == "hd")
                                        {
                                            if (timeoutprev != null && timeoutprev >= new TimeSpan(07, 30, 00) && string.IsNullOrEmpty(item.BreakfastId))
                                            {
                                                item.Breakfast = "Included in Hotel"; flag = true;
                                            }
                                            if (timeoutprev != null && timeoutprev >= new TimeSpan(21, 00, 00) && string.IsNullOrEmpty(item.DinnerId))
                                            { item.Dinner = "Included in Hotel"; flag = true; }
                                        }
                                    }

                                    if (!flag)
                                    {
                                        item.DefaultPlan = "";
                                    }
                                    newtimeout = null;
                                    timeoutprev = null;
                                }
                                else //if MealPlan is Full Board,Half Board,BreakFast Board then it will check follow code
                                {
                                    //lstProdAttr = lstProduct.Where(p => p.Product_Id == res.ProductID).ToList();
                                    //if (lstProdAttr != null && lstProdAttr.Count > 0)
                                    //{
                                    //    strchkin = lstProdAttr.Where(a => a.AttributeName == "Check In").Select(a => a.AttributeValue).FirstOrDefault();
                                    //    strchkout = lstProdAttr.Where(a => a.AttributeName == "Check Out").Select(a => a.AttributeValue).FirstOrDefault();
                                    //}
                                    ////if CheckOut & CheckIn time is defined in mProductLevelAttribute collection then set def times below 
                                    //if (lstProdAttr == null || lstProdAttr.Count == 0 || string.IsNullOrEmpty(strchkin) || string.IsNullOrEmpty(strchkout))
                                    //{
                                    //    strchkin = "18:00";
                                    //    strchkout = "09:00";
                                    //}
                                    //else
                                    //{
                                    //    //returns true if string contains letter 
                                    //    int cntStrchkin = Regex.Matches(strchkin, @"[a-zA-Z]").Count;
                                    //    if (cntStrchkin > 0)
                                    //    {
                                    //        strchkin = "18:00";
                                    //    }
                                    //    int cntstrchkout = Regex.Matches(strchkout, @"[a-zA-Z]").Count;
                                    //    if (cntstrchkout > 0)
                                    //    {
                                    //        strchkout = "09:00";
                                    //    }
                                    //}
                                    //if (res.MealPlan.ToLower() == "bb")
                                    //{
                                    //    strchkin = res.StartTime;
                                    //    strchkout = res.EndTime;
                                    //}

                                    strchkin = res.StartTime;
                                    strchkout = res.EndTime;
                                    if (string.IsNullOrEmpty(strchkin))
                                    {
                                        strchkin = "18:00";
                                    }
                                    if (string.IsNullOrEmpty(strchkout))
                                    {
                                        strchkout = "09:00";
                                    }

                                    TimeSpan.TryParse(strchkin, out timein);
                                    TimeSpan.TryParse(strchkout, out timeout);

                                    if (timein != null && timeout != null)
                                    {
                                        if (res.MealPlan.ToLower() == "bb") //BB has only Breakfast
                                        {
                                            bool flag = false;
                                            item.DefaultPlan = "BB";
                                            strPrevPlan = "BB";
                                            //below condition checks if Hotel is checkout on last nthday then checks only Checkout time & set Included in Hotel
                                            if (item.DayNo == nextday && item.DayNo != res.DayNo)
                                            {
                                                if (timeoutprev != null && timeoutprev >= new TimeSpan(07, 30, 00) && string.IsNullOrEmpty(item.BreakfastId))
                                                {
                                                    strPrevPlan = "";
                                                    item.Breakfast = "Included in Hotel";
                                                    flag = true;
                                                }
                                            }
                                            else if (item.DayNo > res.DayNo && item.DayNo < nextday && string.IsNullOrEmpty(item.BreakfastId))//this condition checks if Hotel is booked between nth day to (nth-1) day then set Included in Hotel
                                            {
                                                item.Breakfast = "Included in Hotel";
                                                flag = true;
                                            }
                                            else
                                            {
                                                //below condition if Hotel booked for nth day then 1st time it will checks below condn
                                                if (((timein >= new TimeSpan(07, 00, 00) && timein <= new TimeSpan(07, 30, 00)) ||
                                                (timeoutprev != null && timeoutprev >= new TimeSpan(07, 30, 00))) && string.IsNullOrEmpty(item.BreakfastId))
                                                {
                                                    item.Breakfast = "Included in Hotel";
                                                    flag = true;
                                                }
                                            }
                                            if (!flag)
                                            {
                                                item.DefaultPlan = "";
                                            }
                                        }
                                        else if (res.MealPlan.ToLower() == "hb") //HB has Breakfast,Dinner
                                        {
                                            bool flag = false;
                                            item.DefaultPlan = "HB";
                                            strPrevPlan = "HB";
                                            //below condition checks if Hotel is checkout on last nthday then checks only Checkout time & set Included in Hotel
                                            if (item.DayNo == nextday && item.DayNo != res.DayNo)
                                            {
                                                strPrevPlan = "";
                                                if (timeoutprev != null && timeoutprev >= new TimeSpan(07, 30, 00) && string.IsNullOrEmpty(item.BreakfastId))
                                                {
                                                    item.Breakfast = "Included in Hotel";
                                                    flag = true;
                                                }

                                                if (timeoutprev != null && timeoutprev >= new TimeSpan(21, 00, 00) && string.IsNullOrEmpty(item.DinnerId))
                                                { item.Dinner = "Included in Hotel"; flag = true; }
                                            }
                                            else if (item.DayNo > res.DayNo && item.DayNo < nextday)//this condition checks if Hotel is booked between nth day to (nth-1) day then set Included in Hotel
                                            {
                                                item.Breakfast = string.IsNullOrEmpty(item.BreakfastId) ? "Included in Hotel" : "";
                                                item.Dinner = string.IsNullOrEmpty(item.DinnerId) ? "Included in Hotel" : "";
                                                flag = true;
                                            }
                                            else
                                            { //below condition if Hotel booked for nth day then 1st time it will checks below condn
                                                if (((timein >= new TimeSpan(07, 00, 00) && timein <= new TimeSpan(07, 30, 00)) ||
                                                (timeoutprev != null && timeoutprev >= new TimeSpan(07, 30, 00))) && string.IsNullOrEmpty(item.BreakfastId))
                                                {
                                                    item.Breakfast = "Included in Hotel";
                                                    flag = true;
                                                }

                                                if (((timein >= new TimeSpan(07, 00, 00) && timein <= new TimeSpan(21, 00, 00))
                                                    || (timeoutprev != null && timeoutprev >= new TimeSpan(21, 00, 00))) && string.IsNullOrEmpty(item.DinnerId))
                                                { item.Dinner = "Included in Hotel"; flag = true; }
                                            }
                                            //flag taken for checking if above if conditions are not true then set DefaultPlan blank
                                            if (!flag)
                                            {
                                                item.DefaultPlan = "";
                                            }
                                        }
                                        else if (res.MealPlan.ToLower() == "fb")
                                        {
                                            bool flag = false;
                                            item.DefaultPlan = "FB";
                                            strPrevPlan = "FB";
                                            //below condition checks if Hotel is checkout on last nthday then checks only Checkout time & set Included in Hotel
                                            if (item.DayNo == nextday && item.DayNo != res.DayNo) //this is checking for last day checkout 
                                            {
                                                strPrevPlan = "";
                                                if (timeoutprev != null && timeoutprev >= new TimeSpan(07, 30, 00) && string.IsNullOrEmpty(item.BreakfastId))
                                                {
                                                    item.Breakfast = "Included in Hotel";
                                                    flag = true;
                                                }
                                                //if (timeoutprev != null && timeoutprev >= new TimeSpan(13, 30, 00))
                                                //{ item.Lunch = "Included in Hotel"; flag = true; }
                                                item.Lunch = string.IsNullOrEmpty(item.LunchId) ? "Included in Hotel" : "";
                                                flag = true;

                                                if (timeoutprev != null && timeoutprev >= new TimeSpan(21, 00, 00) && string.IsNullOrEmpty(item.DinnerId))
                                                { item.Dinner = "Included in Hotel"; flag = true; }
                                            }
                                            else if (item.DayNo > res.DayNo && item.DayNo < nextday)//this condition checks if Hotel is booked between nth day to (nth-1) day then set Included in Hotel
                                            {
                                                item.Breakfast = string.IsNullOrEmpty(item.BreakfastId) ? "Included in Hotel" : "";
                                                item.Lunch = string.IsNullOrEmpty(item.LunchId) ? "Included in Hotel" : "";
                                                item.Dinner = string.IsNullOrEmpty(item.DinnerId) ? "Included in Hotel" : "";
                                                flag = true;
                                            }
                                            else
                                            { //below condition if Hotel booked for nth day then 1st time it will checks below condn
                                                if (((timein >= new TimeSpan(07, 00, 00) && timein <= new TimeSpan(07, 30, 00))
                                               || (timeoutprev != null && timeoutprev >= new TimeSpan(07, 30, 00)) && string.IsNullOrEmpty(item.BreakfastId)))
                                                {
                                                    item.Breakfast = "Included in Hotel";
                                                    flag = true;
                                                }
                                                //if ((timein >= new TimeSpan(07, 00, 00) && timein <= new TimeSpan(13, 30, 00)) ||
                                                //    (timeoutprev != null && timeoutprev >= new TimeSpan(13, 30, 00)))
                                                //{ item.Lunch = "Included in Hotel"; flag = true; }
                                                item.Lunch = string.IsNullOrEmpty(item.LunchId) ? "Included in Hotel" : "";
                                                flag = true;

                                                if (((timein >= new TimeSpan(07, 00, 00) && timein <= new TimeSpan(21, 00, 00)) ||
                                                    (timeoutprev != null && timeoutprev >= new TimeSpan(21, 00, 00))) && string.IsNullOrEmpty(item.DinnerId))
                                                { item.Dinner = "Included in Hotel"; flag = true; }
                                            }

                                            if (!flag)
                                            {
                                                item.DefaultPlan = "";
                                            }
                                        }
                                        else if (res.MealPlan.ToLower() == "hl") //HL has Lunch
                                        {
                                            bool flag = false;
                                            item.DefaultPlan = "HL";
                                            strPrevPlan = "HL";
                                            //below condition checks if Hotel is checkout on last nthday then checks only Checkout time & set Included in Hotel
                                            if (item.DayNo == nextday && item.DayNo != res.DayNo)
                                            {
                                                strPrevPlan = "";
                                                if (timeoutprev != null && timeoutprev >= new TimeSpan(07, 30, 00) && string.IsNullOrEmpty(item.BreakfastId))
                                                {
                                                    item.Breakfast = "Included in Hotel";
                                                    flag = true;
                                                }

                                                item.Lunch = string.IsNullOrEmpty(item.LunchId) ? "Included in Hotel" : "";
                                                flag = true;
                                            }
                                            else if (item.DayNo > res.DayNo && item.DayNo < nextday)//this condition checks if Hotel is booked between nth day to (nth-1) day then set Included in Hotel
                                            {
                                                item.Breakfast = string.IsNullOrEmpty(item.BreakfastId) ? "Included in Hotel" : "";
                                                item.Lunch = string.IsNullOrEmpty(item.LunchId) ? "Included in Hotel" : "";
                                                flag = true;
                                            }
                                            else
                                            {
                                                if (((timein >= new TimeSpan(07, 00, 00) && timein <= new TimeSpan(07, 30, 00))
                                                || (timeoutprev != null && timeoutprev >= new TimeSpan(07, 30, 00)) && string.IsNullOrEmpty(item.BreakfastId)))
                                                {
                                                    item.Breakfast = "Included in Hotel";
                                                    flag = true;
                                                }
                                                //below condition if Hotel booked for nth day then 1st time it will checks below condn
                                                item.Lunch = string.IsNullOrEmpty(item.LunchId) ? "Included in Hotel" : "";
                                                flag = true;
                                            }
                                            //flag taken for checking if above if conditions are not true then set DefaultPlan blank
                                            if (!flag)
                                            {
                                                item.DefaultPlan = "";
                                            }
                                        }
                                        else if (res.MealPlan.ToLower() == "hd") //HD has Dinner
                                        {
                                            bool flag = false;
                                            item.DefaultPlan = "HD";
                                            strPrevPlan = "HD";
                                            //below condition checks if Hotel is checkout on last nthday then checks only Checkout time & set Included in Hotel
                                            if (item.DayNo == nextday && item.DayNo != res.DayNo)
                                            {
                                                strPrevPlan = "";
                                                if (timeoutprev != null && timeoutprev >= new TimeSpan(07, 30, 00) && string.IsNullOrEmpty(item.BreakfastId))
                                                {
                                                    item.Breakfast = "Included in Hotel";
                                                    flag = true;
                                                }

                                                if (timeoutprev != null && timeoutprev >= new TimeSpan(21, 00, 00) && string.IsNullOrEmpty(item.DinnerId))
                                                { item.Dinner = "Included in Hotel"; flag = true; }
                                            }
                                            else if (item.DayNo > res.DayNo && item.DayNo < nextday)//this condition checks if Hotel is booked between nth day to (nth-1) day then set Included in Hotel
                                            {
                                                item.Breakfast = string.IsNullOrEmpty(item.BreakfastId) ? "Included in Hotel" : "";
                                                item.Dinner = string.IsNullOrEmpty(item.DinnerId) ? "Included in Hotel" : "";
                                                flag = true;
                                            }
                                            else
                                            {
                                                //below condition if Hotel booked for nth day then 1st time it will checks below condn
                                                if (((timein >= new TimeSpan(07, 00, 00) && timein <= new TimeSpan(07, 30, 00))
                                                || (timeoutprev != null && timeoutprev >= new TimeSpan(07, 30, 00)) && string.IsNullOrEmpty(item.BreakfastId)))
                                                {
                                                    item.Breakfast = "Included in Hotel";
                                                    flag = true;
                                                }
                                                if (((timein >= new TimeSpan(07, 00, 00) && timein <= new TimeSpan(21, 00, 00)) ||
                                                     (timeoutprev != null && timeoutprev >= new TimeSpan(21, 00, 00))) && string.IsNullOrEmpty(item.DinnerId))
                                                { item.Dinner = "Included in Hotel"; flag = true; }
                                            }
                                            //flag taken for checking if above if conditions are not true then set DefaultPlan blank
                                            if (!flag)
                                            {
                                                item.DefaultPlan = "";
                                            }
                                        }
                                        newtimeout = timeout;
                                    }
                                }
                            }
                            else
                            {
                                newtimeout = null;
                                timeoutprev = null;
                            }
                        }
                    }
                }
            }
            return lstMealDetails;
        }

        //public async Task<List<mPosition>> SetPositionApplyAcross(mPosition objPosition, List<RoutingDays> RoutingDays)
        //{
        //    List<mPosition> lstPosition = new List<mPosition>();
        //    try
        //    {
        //        var mealposition = _MongoContext.mPosition.AsQueryable().Where(a => a.QRFID == objPosition.QRFID && a.ProductType.ToLower() == "meal" && a.IsDeleted == false).ToList();
        //        var resultposition = mealposition.Where(a => a.MealType.ToLower() == objPosition.MealType.ToLower()).ToList();

        //        PositionGetRes response = GetMealGridDetails(RoutingDays, mealposition, objPosition.QRFID);
        //        if (response != null && response.MealDetails != null && response.MealDetails.Count > 0)
        //        {
        //            foreach (var item in response.MealDetails)
        //            {
        //                if (item.Breakfast != null && item.Breakfast.ToLower() == "included in hotel" && objPosition.MealType != null && objPosition.MealType == "Breakfast")
        //                {
        //                    resultposition.Add(new mPosition { DayNo = item.DayNo, RoutingDaysID = item.RoutingDaysID, StartingFrom = item.DayID, MealType = "Breakfast", ProductType = "Meal" });
        //                }
        //                else if (item.Brunch != null && item.Brunch.ToLower() == "included in hotel" && objPosition.MealType != null && objPosition.MealType == "Brunch")
        //                {
        //                    resultposition.Add(new mPosition { DayNo = item.DayNo, RoutingDaysID = item.RoutingDaysID, StartingFrom = item.DayID, MealType = "Brunch", ProductType = "Meal" });
        //                }
        //                else if (item.Dinner != null && item.Dinner.ToLower() == "included in hotel" && objPosition.MealType != null && objPosition.MealType == "Dinner")
        //                {
        //                    resultposition.Add(new mPosition { DayNo = item.DayNo, RoutingDaysID = item.RoutingDaysID, StartingFrom = item.DayID, MealType = "Dinner", ProductType = "Meal" });
        //                }
        //                else if (item.EarlyMorningTea != null && item.EarlyMorningTea.ToLower() == "included in hotel" && objPosition.MealType != null && objPosition.MealType == "Early Morning Tea")
        //                {
        //                    resultposition.Add(new mPosition { DayNo = item.DayNo, RoutingDaysID = item.RoutingDaysID, StartingFrom = item.DayID, MealType = "Early Morning Tea", ProductType = "Meal" });
        //                }
        //                else if (item.Lunch != null && item.Lunch.ToLower() == "included in hotel" && objPosition.MealType != null && objPosition.MealType == "Lunch")
        //                {
        //                    resultposition.Add(new mPosition { DayNo = item.DayNo, RoutingDaysID = item.RoutingDaysID, StartingFrom = item.DayID, MealType = "Lunch", ProductType = "Meal" });
        //                }
        //                else if (item.Tea != null && item.Tea.ToLower() == "included in hotel" && objPosition.MealType != null && objPosition.MealType == "Tea")
        //                {
        //                    resultposition.Add(new mPosition { DayNo = item.DayNo, RoutingDaysID = item.RoutingDaysID, StartingFrom = item.DayID, MealType = "Tea", ProductType = "Meal" });
        //                }
        //            }
        //        }

        //        #region insert If Position's not added
        //        RoutingDaysGetReq req = new RoutingDaysGetReq { QRFID = objPosition.QRFID };
        //        RoutingDaysGetRes res = await _quoteRepository.GetQRFRoutingDays(req);
        //        if (res != null && res.ResponseStatus.Status.ToLower() == "success" && res.RoutingDays != null && res.RoutingDays.Count > 0)
        //        {
        //            if (resultposition != null && resultposition.Count > 0)
        //            {
        //                res.RoutingDays = res.RoutingDays.Where(a => (a.Days != objPosition.StartingFrom && a.RoutingDaysID != objPosition.RoutingDaysID) &&
        //                                  !resultposition.Exists(b => a.Days == b.StartingFrom && a.RoutingDaysID == b.RoutingDaysID)).ToList();
        //            }
        //            else
        //            {
        //                res.RoutingDays = res.RoutingDays.Where(a => a.Days != objPosition.StartingFrom && a.RoutingDaysID != objPosition.RoutingDaysID).ToList();
        //            }

        //            if (res.RoutingDays != null && res.RoutingDays.Count > 0)
        //            {
        //                objPosition.RoomDetailsInfo.ForEach(d => { d.CreateDate = DateTime.Now; d.EditDate = null; d.EditUser = ""; });
        //                mPosition newposition = new mPosition();

        //                foreach (var item in res.RoutingDays)
        //                {
        //                    newposition = new mPosition()
        //                    {
        //                        //ApplyAcrossDays = false,
        //                        BudgetCategory = objPosition.BudgetCategory,
        //                        BudgetCategoryId = objPosition.BudgetCategoryId,
        //                        BuyCurrency = objPosition.BuyCurrency,
        //                        BuyCurrencyId = objPosition.BuyCurrencyId,
        //                        CityID = objPosition.CityID,
        //                        CityName = objPosition.CityName,
        //                        CountryName = objPosition.CountryName.Trim(),
        //                        CreateDate = DateTime.Now,
        //                        CreateUser = objPosition.CreateUser,
        //                        KeepAs = objPosition.KeepAs,
        //                        MealPlan = objPosition.MealPlan,
        //                        MealType = objPosition.MealType,
        //                        OPSRemarks = objPosition.OPSRemarks,
        //                        ProductID = objPosition.ProductID,
        //                        ProductName = objPosition.ProductName,
        //                        ProductType = objPosition.ProductType,
        //                        ProductTypeId = objPosition.ProductTypeId,
        //                        QRFID = objPosition.QRFID,
        //                        RoomDetailsInfo = objPosition.RoomDetailsInfo,
        //                        StandardFOC = objPosition.StandardFOC,
        //                        StandardPrice = objPosition.StandardPrice,
        //                        SupplierId = objPosition.SupplierId,
        //                        SupplierName = objPosition.SupplierName,
        //                        TLRemarks = objPosition.TLRemarks,
        //                        StartTime = objPosition.StartTime,
        //                        EndTime = objPosition.EndTime,
        //                        StartingFrom = item.Days,
        //                        EditDate = null,
        //                        EditUser = "",
        //                        PositionId = Guid.NewGuid().ToString(),
        //                        PositionSequence = item.RouteSequence,
        //                        DayNo = item.DayNo,
        //                        RoutingDaysID = item.RoutingDaysID,
        //                        Status = objPosition.Status,
        //                        TransferDetails = objPosition.TransferDetails
        //                    };

        //                    lstPosition.Add(newposition);
        //                }

        //                await _MongoContext.mPosition.InsertManyAsync(lstPosition);
        //            }
        //        }
        //        #endregion
        //        if (lstPosition != null && lstPosition.Count > 0)
        //        {
        //            #region insert extra added Price & FOC    
        //            List<string> lstStr = new List<string>();
        //            lstStr.Add(objPosition.PositionId);
        //            lstStr.AddRange(lstPosition.Select(a => a.PositionId).ToList());

        //            var positionprice = _MongoContext.mPositionPrice.AsQueryable().Where(a => lstStr.Contains(a.PositionId));
        //            var objpositionprice = positionprice.Where(a => a.PositionId == objPosition.PositionId).ToList();

        //            if (objpositionprice != null && objpositionprice.Count > 0)
        //            {
        //                List<mPositionPrice> lstPositionPrice = new List<mPositionPrice>();
        //                foreach (var item in lstPosition)
        //                {
        //                    lstPositionPrice = new List<mPositionPrice>();
        //                    var posprice = positionprice.Where(a => a.PositionId == item.PositionId).ToList();
        //                    lstPositionPrice = objpositionprice.FindAll(a => !posprice.Exists(b => a.RoomId == b.RoomId));

        //                    if (lstPositionPrice != null && lstPositionPrice.Count > 0)
        //                    {
        //                        lstPositionPrice.ForEach(a => { a.PositionId = item.PositionId; a._Id = ObjectId.Empty; a.PositionPriceId = Guid.NewGuid().ToString(); });
        //                        await _MongoContext.mPositionPrice.InsertManyAsync(lstPositionPrice);
        //                    }
        //                }
        //            }

        //            var positionfoc = _MongoContext.mPositionFOC.AsQueryable().Where(a => lstStr.Contains(a.PositionId));
        //            var objpositionfoc = positionfoc.Where(a => a.PositionId == objPosition.PositionId).ToList();
        //            if (objpositionfoc != null && objpositionfoc.Count > 0)
        //            {
        //                List<mPositionFOC> lstPositionFOC = new List<mPositionFOC>();
        //                foreach (var item in lstPosition)
        //                {
        //                    lstPositionFOC = new List<mPositionFOC>();
        //                    var posfoc = objpositionfoc.Where(a => a.PositionId == item.PositionId).ToList();
        //                    lstPositionFOC = objpositionfoc.FindAll(a => !posfoc.Exists(b => a.RoomId == b.RoomId));
        //                    if (lstPositionFOC != null && lstPositionFOC.Count > 0)
        //                    {
        //                        lstPositionFOC.ForEach(a => { a.PositionId = item.PositionId; a._Id = ObjectId.Empty; a.PositionFOCId = Guid.NewGuid().ToString(); });
        //                        await _MongoContext.mPositionFOC.InsertManyAsync(lstPositionFOC);
        //                    }
        //                }
        //            }
        //            #endregion
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }
        //    return lstPosition;
        //}

        public async Task<List<mPosition>> SetPositionApplyAcross(mPosition objPosition, List<RoutingDays> RoutingDays)
        {
            List<mPosition> lstPosition = new List<mPosition>();
            try
            {
                var mealposition = _MongoContext.mPosition.AsQueryable().Where(a => a.QRFID == objPosition.QRFID && a.ProductType.ToLower() == "meal" && a.IsDeleted == false).ToList();
                var resultposition = mealposition.Where(a => a.MealType.ToLower() == objPosition.MealType.ToLower()).ToList();

                PositionGetRes response = GetMealGridDetails(RoutingDays, mealposition, objPosition.QRFID);
                if (response != null && response.MealDetails != null && response.MealDetails.Count > 0)
                {
                    foreach (var item in response.MealDetails)
                    {
                        if (item.Breakfast != null && item.Breakfast.ToLower() == "included in hotel" && objPosition.MealType != null && objPosition.MealType == "Breakfast")
                        {
                            resultposition.Add(new mPosition { DayNo = item.DayNo, RoutingDaysID = item.RoutingDaysID, StartingFrom = item.DayID, MealType = "Breakfast", ProductType = "Meal" });
                        }
                        else if (item.Brunch != null && item.Brunch.ToLower() == "included in hotel" && objPosition.MealType != null && objPosition.MealType == "Brunch")
                        {
                            resultposition.Add(new mPosition { DayNo = item.DayNo, RoutingDaysID = item.RoutingDaysID, StartingFrom = item.DayID, MealType = "Brunch", ProductType = "Meal" });
                        }
                        else if (item.Dinner != null && item.Dinner.ToLower() == "included in hotel" && objPosition.MealType != null && objPosition.MealType == "Dinner")
                        {
                            resultposition.Add(new mPosition { DayNo = item.DayNo, RoutingDaysID = item.RoutingDaysID, StartingFrom = item.DayID, MealType = "Dinner", ProductType = "Meal" });
                        }
                        else if (item.EarlyMorningTea != null && item.EarlyMorningTea.ToLower() == "included in hotel" && objPosition.MealType != null && objPosition.MealType == "Early Morning Tea")
                        {
                            resultposition.Add(new mPosition { DayNo = item.DayNo, RoutingDaysID = item.RoutingDaysID, StartingFrom = item.DayID, MealType = "Early Morning Tea", ProductType = "Meal" });
                        }
                        else if (item.Lunch != null && item.Lunch.ToLower() == "included in hotel" && objPosition.MealType != null && objPosition.MealType == "Lunch")
                        {
                            resultposition.Add(new mPosition { DayNo = item.DayNo, RoutingDaysID = item.RoutingDaysID, StartingFrom = item.DayID, MealType = "Lunch", ProductType = "Meal" });
                        }
                        else if (item.Tea != null && item.Tea.ToLower() == "included in hotel" && objPosition.MealType != null && objPosition.MealType == "Tea")
                        {
                            resultposition.Add(new mPosition { DayNo = item.DayNo, RoutingDaysID = item.RoutingDaysID, StartingFrom = item.DayID, MealType = "Tea", ProductType = "Meal" });
                        }
                    }
                }

                #region insert If Position's not added
                RoutingDaysGetReq req = new RoutingDaysGetReq { QRFID = objPosition.QRFID };
                RoutingDaysGetRes res = await _quoteRepository.GetQRFRoutingDays(req);
                if (res != null && res.ResponseStatus.Status.ToLower() == "success" && res.RoutingDays != null && res.RoutingDays.Count > 0)
                {
                    if (resultposition != null && resultposition.Count > 0)
                    {
                        res.RoutingDays = res.RoutingDays.Where(a => (a.Days != objPosition.StartingFrom && a.RoutingDaysID != objPosition.RoutingDaysID) &&
                                          !resultposition.Exists(b => a.Days == b.StartingFrom && a.RoutingDaysID == b.RoutingDaysID)).ToList();
                    }
                    else
                    {
                        res.RoutingDays = res.RoutingDays.Where(a => a.Days != objPosition.StartingFrom && a.RoutingDaysID != objPosition.RoutingDaysID).ToList();
                    }

                    if (res.RoutingDays != null && res.RoutingDays.Count > 0)
                    {
                        var countrynameList = res.RoutingDays.Select(a => a.FromCityName.Split(',')[1].Trim()).Distinct().ToList();
                        var cityids = res.RoutingDays.Select(a => a.FromCityID).ToList();
                        var lstResorts = _MongoContext.mResort.AsQueryable().Where(x => x.KeyResort == true && x.ResortType.ToLower() == "city" && countrynameList.Contains(x.ParentResortName)).ToList();

                        var lstResortsRoutes = _MongoContext.mResort.AsQueryable().Where(x => x.ResortType.ToLower() == "city" && cityids.Contains(x.Voyager_Resort_Id)).ToList();

                        //var lstProducts = _MongoContext.mProducts.AsQueryable().Where(x => x.ProdName.ToLower() == objPosition.ProductName.ToLower() && cityids.Contains(x.Resort_Id)).ToList();

                        var resortscityids = lstResorts.Select(a => a.Voyager_Resort_Id).Distinct().ToList();
                        var lstProducts = _MongoContext.mProducts_Lite.AsQueryable().Where(x => x.ProductType.ToLower() == "meal" && x.ProdName.ToLower() == objPosition.ProductName.ToLower() && x.Placeholder == true
                                         && resortscityids.Contains(x.CityId)).ToList();

                        var prodids = lstProducts.Select(a => a.VoyagerProduct_Id).ToList();
                        List<ProdCategoryDetails> lstProdCategoryDetails = new List<ProdCategoryDetails>();
                        var prodSupplierGetRes = new ProductSupplierGetRes();
                        var productRangeGetRes = new ProductRangeGetRes();

                        if (prodids?.Count > 0)
                        {
                            lstProdCategoryDetails = _productRepository.GetProductCategoryByParam(new ProductCatGetReq { ProductIdList = prodids });
                            prodSupplierGetRes = _productRepository.GetSupplierDetails(new ProductSupplierGetReq() { ProductIdList = prodids });
                            productRangeGetRes = _productRepository.GetProductRangeByParam(new ProductRangeGetReq() { ProductIdList = prodids });

                            //objPosition.RoomDetailsInfo.ForEach(d => { d.CreateDate = DateTime.Now; d.EditDate = null; d.EditUser = ""; });
                            mPosition newposition = new mPosition();
                            List<RoomDetailsInfo> lstRoomDetailsInfo = new List<RoomDetailsInfo>();
                            List<ProductRangeDetails> lstProductRangeDetails = new List<ProductRangeDetails>();
                            ProductRangeDetails objProductRangeDetails = new ProductRangeDetails();
                            List<ResponseStatus> lstResponseStatus = new List<ResponseStatus>();

                            foreach (var item in res.RoutingDays)
                            {
                                var countryName = lstResortsRoutes?.Where(a => a.Voyager_Resort_Id == item.FromCityID).FirstOrDefault()?.ParentResortName;
                                var product = lstProducts?.Where(a => a.CityId == item.FromCityID)?.FirstOrDefault();
                                product = product == null ? lstProducts.Where(a => a.CountryName == countryName).FirstOrDefault() : product;

                                if (!string.IsNullOrEmpty(countryName) && product != null)
                                {
                                    newposition = new mPosition()
                                    {
                                        //ApplyAcrossDays = false,
                                        //CityID = objPosition.CityID,
                                        //CityName = objPosition.CityName,
                                        //CountryName = objPosition.CountryName.Trim(),
                                        //BudgetCategory = objPosition.BudgetCategory,
                                        //BudgetCategoryId = objPosition.BudgetCategoryId,
                                        //RoomDetailsInfo = objPosition.RoomDetailsInfo,
                                        //SupplierId = objPosition.SupplierId,
                                        //SupplierName = objPosition.SupplierName,
                                        //ProductID = objPosition.ProductID,
                                        //ProductName = objPosition.ProductName,
                                        //ProductType = objPosition.ProductType,
                                        //ProductTypeId = objPosition.ProductTypeId,

                                        CityID = item.FromCityID,
                                        CityName = item.FromCityName.Split(",")[0].Trim(),
                                        CountryName = countryName,
                                        BuyCurrency = objPosition.BuyCurrency,
                                        BuyCurrencyId = objPosition.BuyCurrencyId,
                                        CreateDate = DateTime.Now,
                                        CreateUser = objPosition.CreateUser,
                                        KeepAs = objPosition.KeepAs,
                                        MealPlan = objPosition.MealPlan,
                                        MealType = objPosition.MealType,
                                        OPSRemarks = objPosition.OPSRemarks,
                                        QRFID = objPosition.QRFID,
                                        StandardFOC = objPosition.StandardFOC,
                                        StandardPrice = objPosition.StandardPrice,
                                        TLRemarks = objPosition.TLRemarks,
                                        StartTime = objPosition.StartTime,
                                        EndTime = objPosition.EndTime,
                                        StartingFrom = item.Days,
                                        EditDate = null,
                                        EditUser = "",
                                        PositionId = Guid.NewGuid().ToString(),
                                        PositionSequence = item.RouteSequence,
                                        DayNo = item.DayNo,
                                        RoutingDaysID = item.RoutingDaysID,
                                        Status = objPosition.Status,
                                        TransferDetails = objPosition.TransferDetails
                                    };

                                    newposition.ProductID = product.VoyagerProduct_Id;
                                    newposition.ProductName = product.ProdName;
                                    newposition.ProductType = product.ProductType;
                                    newposition.ProductTypeId = product.ProductType_Id;

                                    var prodMealType = lstProdCategoryDetails?.Where(a => a.ProductId == product.VoyagerProduct_Id)?.FirstOrDefault();
                                    if (prodMealType != null)
                                    {
                                        newposition.BudgetCategory = prodMealType.ProductCategoryName;
                                        newposition.BudgetCategoryId = prodMealType.ProductCategoryId;

                                        var prodSupplier = prodSupplierGetRes?.SupllierList?.Where(a => a.ProdId == product.VoyagerProduct_Id).FirstOrDefault();
                                        if (prodSupplier != null)
                                        {
                                            newposition.SupplierId = prodSupplier.SupplierId;
                                            newposition.SupplierName = prodSupplier.SupplierName;

                                            lstProductRangeDetails = new List<ProductRangeDetails>();
                                            lstProductRangeDetails = productRangeGetRes?.ProductRangeDetails?.Where(a => a.ProductCategoryId == prodMealType.ProductCategoryId).
                                                Select(a => new ProductRangeDetails
                                                {
                                                    AdditionalYN = a.AdditionalYN,
                                                    AgeRange = a.AgeRange,
                                                    PersonType = a.PersonType,
                                                    ProductCategoryId = a.ProductCategoryId,
                                                    ProductCategoryName = a.ProductCategoryName,
                                                    ProductId = a.ProductId,
                                                    ProductMenu = a.ProductMenu,
                                                    ProductRangeCode = a.ProductRangeCode,
                                                    ProductRangeName = a.ProductRangeName,
                                                    VoyagerProductRange_Id = a.VoyagerProductRange_Id
                                                }).ToList();

                                            if (lstProductRangeDetails?.Count > 0)
                                            {
                                                lstProductRangeDetails.ForEach(a => a.ProductRangeCode = a.ProductRangeCode + " (" + a.PersonType + (a.AgeRange == null ? "" : " | " + a.AgeRange) + ") ");
                                                lstRoomDetailsInfo = new List<RoomDetailsInfo>();

                                                foreach (var oldRooms in objPosition.RoomDetailsInfo)
                                                {
                                                    objProductRangeDetails = new ProductRangeDetails();
                                                    objProductRangeDetails = lstProductRangeDetails.Where(a => a.ProductRangeCode.Trim() == oldRooms.ProductRange.Trim()).FirstOrDefault();
                                                    if (objProductRangeDetails != null)
                                                    {
                                                        lstRoomDetailsInfo.Add(new RoomDetailsInfo
                                                        {
                                                            CreateDate = DateTime.Now,
                                                            CreateUser = objPosition.CreateUser,
                                                            IsSupplement = Convert.ToBoolean(objProductRangeDetails.AdditionalYN),
                                                            ProdDesc = objProductRangeDetails.ProductMenu,
                                                            ProductCategory = prodMealType.ProductCategoryName,
                                                            ProductCategoryId = prodMealType.ProductCategoryId,
                                                            ProductRange = objProductRangeDetails.ProductRangeCode,
                                                            ProductRangeId = objProductRangeDetails.VoyagerProductRange_Id,
                                                            RoomId = Guid.NewGuid().ToString()
                                                        });
                                                    }
                                                    else
                                                    {
                                                        lstResponseStatus.Add(new ResponseStatus()
                                                        {
                                                            Status = "Error",
                                                            ErrorMessage = oldRooms.ProductRange + " for ProductId " + product.VoyagerProduct_Id + "(" + prodMealType.ProductCategoryId + ")" + " of " + newposition.CityName + ", " + newposition.CountryName + " " + " not found in the mProductRange collection."
                                                        });
                                                    }
                                                }

                                                if (lstRoomDetailsInfo?.Count > 0)
                                                {
                                                    lstRoomDetailsInfo = lstRoomDetailsInfo.OrderBy(a => a.IsSupplement).ThenBy(a => a.ProductRange).ToList();
                                                    int i = 1;
                                                    lstRoomDetailsInfo.ForEach(a => a.RoomSequence = i++);

                                                    newposition.RoomDetailsInfo = lstRoomDetailsInfo;
                                                    lstPosition.Add(newposition);
                                                }
                                            }
                                            else
                                            {
                                                lstResponseStatus.Add(new ResponseStatus() { Status = "Error", ErrorMessage = "ProductId " + product.VoyagerProduct_Id + "(" + prodMealType.ProductCategoryId + ")" + " of " + newposition.CityName + ", " + newposition.CountryName + " " + " not found in the mProductRange collection." });
                                            }
                                        }
                                        else
                                        {
                                            lstResponseStatus.Add(new ResponseStatus() { Status = "Error", ErrorMessage = "ProductId " + product.VoyagerProduct_Id + " of " + newposition.CityName + ", " + newposition.CountryName + " " + " not found in the mProductSupplier collection." });
                                        }
                                    }
                                    else
                                    {
                                        lstResponseStatus.Add(new ResponseStatus() { Status = "Error", ErrorMessage = "ProductId " + product.VoyagerProduct_Id + " of " + newposition.CityName + ", " + newposition.CountryName + " " + " not found in the mProductCategory collection." });
                                    }
                                }
                                else
                                {
                                    lstResponseStatus.Add(new ResponseStatus() { Status = "Error", ErrorMessage = newposition.CityName + ", " + newposition.CountryName + " " + " not found in the mProduct collection." });
                                }
                            }

                            if (lstPosition?.Count > 0)
                            {
                                await _MongoContext.mPosition.InsertManyAsync(lstPosition);
                            }
                        }
                    }
                }
                #endregion

                #region insert Price & FOC for New Positions
                if (lstPosition != null && lstPosition.Count > 0)
                {
                    List<string> RangeIdList = new List<string>();
                    lstPosition.ForEach(a => RangeIdList.AddRange(a.RoomDetailsInfo.Where(b => b.IsDeleted == false).Select(b => b.ProductRangeId).ToList()));

                    if (RangeIdList != null && RangeIdList.Count > 0)
                    {
                        var ProdRangeList = _MongoContext.mProductRange.AsQueryable().Where(a => RangeIdList.Contains(a.VoyagerProductRange_Id))
                        .Select(a => new ProductRangeInfo
                        {
                            VoyagerProductRange_Id = a.VoyagerProductRange_Id,
                            ProductRangeCode = a.ProductRangeCode,
                            ProductType_Id = a.ProductType_Id,
                            PersonType = a.PersonType,
                            ProductMenu = a.ProductMenu
                        }).ToList();

                        PositionPriceFOCSetRes objPositionPriceFOCSetRes = await SetAllPositionPriceFOC(new PositionPriceFOCSetReq
                        {
                            IsClone = false,
                            IsFOC = true,
                            IsPrice = true,
                            LoginUserId = objPosition.CreateUser,
                            QRFID = objPosition.QRFID,
                            PositionIdList = lstPosition.Select(a => a.PositionId).ToList(),
                            ProductRangeInfo = ProdRangeList
                        });
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return lstPosition;
        }
        #endregion

        #region Transfer
        public PositionGetRes GetTransferGridDetails(List<RoutingDays> RoutingDays, List<mPosition> mPosition, string QRFID)
        {
            PositionGetRes response = new PositionGetRes() { QRFID = QRFID, mPosition = mPosition, RoutingDays = RoutingDays };

            var objresultPosition = new List<mPosition>();
            List<TransferDetails> lstTransferDetails = new List<TransferDetails>();

            foreach (var item in RoutingDays)
            {
                objresultPosition = mPosition.Where(a => a.RoutingDaysID == item.RoutingDaysID).ToList();
                if (objresultPosition != null && objresultPosition.Count > 0)
                {
                    foreach (var pos in objresultPosition)
                    {
                        if (lstTransferDetails.Where(a => a.TransferProperties.RoutingDaysID == item.RoutingDaysID).Count() > 0)
                        {
                            AddTransferList(pos, lstTransferDetails, item);
                        }
                        else
                        {
                            lstTransferDetails.Add(AddTransferDetails(pos, item));
                        }
                    }
                }
                else
                {
                    lstTransferDetails.Add(AddTransferDetails(null, item));
                }
            }

            lstTransferDetails = lstTransferDetails.OrderBy(m => m.TransferProperties.DayID).ToList();
            response.TransferDetails = lstTransferDetails;
            return response;
        }

        public TransferDetails AddTransferDetails(mPosition objresultPosition, RoutingDays item)
        {
            TransferProperties objTransferProperties = new TransferProperties()
            {
                RoutingCity = item.GridLabel,
                DayName = item.Days,
                DayID = item.DayNo,
                PositionSequence = item.RouteSequence,
                RoutingDaysID = item.RoutingDaysID
            };
            objresultPosition = objresultPosition == null ? new mPosition() { ProductType = "", PositionId = "", IsDeleted = false } : objresultPosition;

            TransferDetails objTransferDetails = new TransferDetails()
            {
                TransferProperties = objTransferProperties,
                PCTID = objresultPosition.ProductType.ToLower() == "private transfer" ? objresultPosition.PositionId : "",
                STID = objresultPosition.ProductType.ToLower() == "scheduled transfer" ? objresultPosition.PositionId : "",
                FPID = objresultPosition.ProductType.ToLower() == "ferry passenger" ? objresultPosition.PositionId : "",
                FTID = objresultPosition.ProductType.ToLower() == "ferry transfer" ? objresultPosition.PositionId : "",
                IsPCT = objresultPosition.ProductType.ToLower() == "private transfer" && !objresultPosition.IsDeleted ? true : false,
                IsST = objresultPosition.ProductType.ToLower() == "scheduled transfer" && !objresultPosition.IsDeleted ? true : false,
                IsFP = objresultPosition.ProductType.ToLower() == "ferry passenger" && !objresultPosition.IsDeleted ? true : false,
                IsFT = objresultPosition.ProductType.ToLower() == "ferry transfer" && !objresultPosition.IsDeleted ? true : false
            };
            return objTransferDetails;
        }

        public List<TransferDetails> AddTransferList(mPosition objresultPosition, List<TransferDetails> lstTransferDetails, RoutingDays item)
        {
            lstTransferDetails.Where(m => m.TransferProperties.RoutingDaysID == item.RoutingDaysID).ToList().ForEach(m =>
            {
                if (objresultPosition.ProductType.ToLower() == "private transfer" && string.IsNullOrEmpty(m.PCTID))
                {
                    m.PCTID = objresultPosition.PositionId;
                    m.IsPCT = !objresultPosition.IsDeleted ? true : false;
                }

                if (objresultPosition.ProductType.ToLower() == "scheduled transfer" && string.IsNullOrEmpty(m.STID))
                {
                    m.STID = objresultPosition.PositionId;
                    m.IsST = !objresultPosition.IsDeleted ? true : false;
                }

                if (objresultPosition.ProductType.ToLower() == "ferry passenger" && string.IsNullOrEmpty(m.FPID))
                {
                    m.FPID = objresultPosition.PositionId;
                    m.IsFP = !objresultPosition.IsDeleted ? true : false;
                }

                if (objresultPosition.ProductType.ToLower() == "ferry transfer" && string.IsNullOrEmpty(m.FTID))
                {
                    m.FTID = objresultPosition.PositionId;
                    m.IsFT = !objresultPosition.IsDeleted ? true : false;
                }
            });
            return lstTransferDetails;
        }
        #endregion

        #region  Price FOC Set for All Positions
        /// <summary>
        /// this will work on Save button of position page i.e. all position prices and FOC get saved here
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<PositionPriceFOCSetRes> SetAllPositionPriceFOC(PositionPriceFOCSetReq request)
        {
            PositionPriceFOCSetRes response = new PositionPriceFOCSetRes();
            response.ResponseStatus = new ResponseStatus();
            response.PositionFOC = new List<mPositionFOC>();
            response.PositionPrice = new List<mPositionPrice>();

            try
            {
                if (request.IsClone)
                {
                    response.PositionFOCQRF = new List<mQRFPositionFOC>();
                    response.PositionPriceQRF = new List<mPositionPriceQRF>();

                    mPositionPriceQRF objPricesInfo = new mPositionPriceQRF();
                    mQRFPositionFOC objFOCInfo = new mQRFPositionFOC();
                    mQRFPrice result = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == request.QRFID && a.IsCurrentVersion).FirstOrDefault();

                    if (result != null)
                    {
                        result.PaxSlabDetails.QRFPaxSlabs = result.PaxSlabDetails.QRFPaxSlabs.Where(a => a.IsDeleted == false).ToList();
                        result.Departures = result.Departures.Where(a => a.IsDeleted == false).ToList();
                        List<mQRFPosition> lstPositions = new List<mQRFPosition>();

                        if (!string.IsNullOrEmpty(request.PositionId))
                        {
                            lstPositions = _MongoContext.mQRFPosition.AsQueryable().Where(a => a.PositionId == request.PositionId && a.IsDeleted == false).ToList();
                        }
                        else if (request.ProductTypeList != null && request.ProductTypeList.Count > 0)
                        {
                            lstPositions = _MongoContext.mQRFPosition.AsQueryable().Where(a => a.QRFID == result.QRFID && request.ProductTypeList.Contains(a.ProductType) && a.IsDeleted == false).ToList();
                        }
                        else
                        {
                            lstPositions = _MongoContext.mQRFPosition.AsQueryable().Where(a => a.QRFID == result.QRFID && a.IsDeleted == false).ToList();
                        }

                        if (lstPositions != null && lstPositions.Count > 0)
                        {
                            var usernm = !string.IsNullOrEmpty(lstPositions[0].EditUser) ? lstPositions[0].EditUser : lstPositions[0].CreateUser;
                            var lstProductList = new List<string>();
                            string supplierId = "";
                            List<ProductRangeInfo> prodrange = new List<ProductRangeInfo>();
                            var prorangelist = new List<string>();
                            var currencyId = "";
                            var currency = "";
                            var roomingcrosspos = new List<QRFRoomDetailsInfo>();
                            var supplierids = new List<string>();
                            var curids = new List<string>();
                            var currencyidlist = new List<ProductSupplierInfo>();
                            // var currencyidlist = new List<mProductSupplier>();
                            // var currencylist = new List<mCurrency>();
                            var roomingcrossposnone = new List<QRFRoomDetailsInfo>();
                            var roomDetailsList = new List<QRFRoomDetailsInfo>();
                            var lstProductSupplier = new List<ProductSupplier>();

                            var positionids = lstPositions.Select(a => a.PositionId).ToList();
                            lstProductList = lstPositions.Select(a => a.ProductID).ToList();
                            var procatdetails = lstPositions.Select(a => a.RoomDetailsInfo).ToList();

                            var resultPosPrices = _MongoContext.mPositionPriceQRF.AsQueryable().Where(a => a.QRFID == result.QRFID && positionids.Contains(a.PositionId) && a.IsDeleted == false).ToList();

                            foreach (var item in procatdetails)
                            {
                                roomDetailsList.AddRange(item);
                            }

                            var prodids = lstPositions.Select(b => b.ProductID).ToList();
                            prodrange = request.ProductRangeInfo;

                            if (request.IsPrice)
                            {
                                supplierids = lstPositions.Select(a => a.SupplierId).ToList();

                                currencyidlist = _MongoContext.Products.AsQueryable().Where(p => lstProductList.Contains(p.VoyagerProduct_Id)).
                                   Select(a => new ProductSupplierInfo
                                   {
                                       Product_Id = a.VoyagerProduct_Id,
                                       ProductSupplier = a.ProductSuppliers
                                   }).ToList();

                                //currencyidlist = _MongoContext.mProductSupplier.AsQueryable().Where(s => supplierids.Contains(s.Company_Id)).ToList();
                                //curids = currencyidlist.Select(a => a.Currency_Id).ToList();
                                //currencylist = _MongoContext.mCurrency.AsQueryable().Where(c => curids.Contains(c.VoyagerCurrency_Id)).ToList();
                            }
                            result.TourEntities.ForEach(a => a.Flag = (a.Type.Contains("Coach") || a.Type.Contains("LDC") ? "DRIVER" : a.Type.Contains("Guide") ? "GUIDE" : a.Flag));

                            for (int p = 0; p < lstPositions.Count; p++)
                            {
                                supplierId = lstPositions[p].SupplierId;
                                int addDaysToPeriod = 0;
                                addDaysToPeriod = (lstPositions[p].DayNo - 1) + (lstPositions[p].Duration - 1);

                                roomingcrossposnone = lstPositions[p].RoomDetailsInfo.Where(a => string.IsNullOrEmpty(a.CrossPositionId) && a.IsDeleted == false).ToList();

                                if (request.IsPrice)
                                {
                                    response.StandardPrice = lstPositions[p].StandardPrice;
                                    //currencyId = currencyidlist.Where(s => s.Company_Id == supplierId && s.Product_Id == lstPositions[p].ProductID).Select(a => a.Currency_Id).FirstOrDefault();
                                    //currency = currencylist.Where(c => c.VoyagerCurrency_Id == currencyId).Select(a => a.Currency).FirstOrDefault();

                                    lstProductSupplier = currencyidlist.Where(a => a.Product_Id == lstPositions[p].ProductID).FirstOrDefault().ProductSupplier;
                                    var objProductSupplier = lstProductSupplier.Where(a => a.Company_Id == supplierId).FirstOrDefault();

                                    currencyId = objProductSupplier.CurrencyId;
                                    currency = objProductSupplier.CurrencyName;

                                    roomingcrosspos = lstPositions[p].RoomDetailsInfo.Where(a => !string.IsNullOrEmpty(a.CrossPositionId) && a.IsDeleted == false).
                                    Select(a => new QRFRoomDetailsInfo
                                    {
                                        IsDeleted = a.IsDeleted,
                                        IsSupplement = a.IsSupplement,
                                        ProdDesc = a.ProdDesc,
                                        ProductCategory = a.ProductCategory,
                                        ProductCategoryId = a.ProductCategoryId,
                                        ProductRange = a.ProductRange,
                                        ProductRangeId = a.ProductRangeId,
                                        RoomId = a.RoomId,
                                        RoomSequence = a.RoomSequence
                                    }).Distinct().ToList();
                                }
                                if (request.IsFOC)
                                {
                                    response.StandardFOC = lstPositions[p].StandardFOC;
                                }

                                for (int i = 0; i < result.Departures.Count; i++)
                                {
                                    if (roomingcrossposnone != null && roomingcrossposnone.Count > 0)
                                    {
                                        for (int j = 0; j < result.PaxSlabDetails.QRFPaxSlabs.Count; j++)
                                        {
                                            for (int k = 0; k < roomingcrossposnone.Count; k++)
                                            {
                                                if (request.IsPrice)
                                                {
                                                    //Price added in objPricesInfo
                                                    objPricesInfo = new mPositionPriceQRF
                                                    {
                                                        QRFID = result.QRFID,
                                                        PositionId = lstPositions[p].PositionId,
                                                        DepartureId = result.Departures[i].Departure_Id,
                                                        Period = result.Departures[i].Date,
                                                        ContractPeriod = result.Departures[i].Date,
                                                        PaxSlabId = result.PaxSlabDetails.QRFPaxSlabs[j].PaxSlab_Id,
                                                        PaxSlab = result.PaxSlabDetails.QRFPaxSlabs[j].From.ToString() + " - " + result.PaxSlabDetails.QRFPaxSlabs[j].To.ToString(),
                                                        SupplierId = supplierId,
                                                        Supplier = lstPositions[p].SupplierName,
                                                        RoomId = roomingcrossposnone[k].RoomId,
                                                        IsSupplement = roomingcrossposnone[k].IsSupplement,
                                                        ProductCategoryId = roomingcrossposnone[k].ProductCategoryId,
                                                        ProductRangeId = roomingcrossposnone[k].ProductRangeId,
                                                        ProductCategory = roomingcrossposnone[k].ProductCategory,
                                                        ProductRange = roomingcrossposnone[k].ProductRange,
                                                        BuyCurrencyId = currencyId,
                                                        BuyCurrency = currency
                                                    };
                                                    if (addDaysToPeriod > 0)
                                                        objPricesInfo.ContractPeriod = Convert.ToDateTime(objPricesInfo.ContractPeriod).AddDays(addDaysToPeriod);

                                                    objPricesInfo.Type = prodrange.Where(a => a.VoyagerProductRange_Id == objPricesInfo.ProductRangeId).Select(b => b.PersonType).FirstOrDefault();
                                                    objPricesInfo.ProductRangeCode = prodrange.Where(a => a.VoyagerProductRange_Id == objPricesInfo.ProductRangeId).Select(b => b.ProductRangeCode).FirstOrDefault();
                                                    response.PositionPriceQRF.Add(objPricesInfo);
                                                }

                                                if (request.IsFOC)
                                                {
                                                    //FOC added in objFOCInfo
                                                    objFOCInfo = new mQRFPositionFOC
                                                    {
                                                        QRFID = result.QRFID,
                                                        PositionId = lstPositions[p].PositionId,
                                                        DepartureId = result.Departures[i].Departure_Id,
                                                        Period = result.Departures[i].Date,
                                                        ContractPeriod = result.Departures[i].Date,
                                                        PaxSlabId = result.PaxSlabDetails.QRFPaxSlabs[j].PaxSlab_Id,
                                                        PaxSlab = result.PaxSlabDetails.QRFPaxSlabs[j].From.ToString() + " - " + result.PaxSlabDetails.QRFPaxSlabs[j].To.ToString(),
                                                        CityId = lstPositions[p].CityID,
                                                        CityName = lstPositions[p].CityName,
                                                        ProductId = lstPositions[p].ProductID,
                                                        ProductName = lstPositions[p].ProductName,
                                                        SupplierId = supplierId,
                                                        Supplier = lstPositions[p].SupplierName,
                                                        RoomId = roomingcrossposnone[k].RoomId,
                                                        IsSupplement = roomingcrossposnone[k].IsSupplement,
                                                        ProductCategoryId = roomingcrossposnone[k].ProductCategoryId,
                                                        ProductRangeId = roomingcrossposnone[k].ProductRangeId,
                                                        ProductCategory = roomingcrossposnone[k].ProductCategory,
                                                        ProductRange = roomingcrossposnone[k].ProductRange,
                                                        Quantity = result.PaxSlabDetails.QRFPaxSlabs[j].From
                                                    };
                                                    if (addDaysToPeriod > 0)
                                                        objFOCInfo.ContractPeriod = Convert.ToDateTime(objFOCInfo.ContractPeriod).AddDays(addDaysToPeriod);
                                                    objFOCInfo.Type = prodrange.Where(a => a.VoyagerProductRange_Id == objFOCInfo.ProductRangeId).Select(b => b.PersonType).FirstOrDefault();
                                                    response.PositionFOCQRF.Add(objFOCInfo);
                                                }
                                            }
                                        }
                                    }

                                    var TourEntitiesPaxSlab = new List<TourEntities>();
                                    var roomdetails = new QRFRoomDetailsInfo();
                                    if (request.IsPrice)
                                    {
                                        if (roomingcrosspos != null && roomingcrosspos.Count > 0)
                                        {
                                            for (int k = 0; k < roomingcrosspos.Count; k++)
                                            {
                                                roomdetails = lstPositions[p].RoomDetailsInfo.Where(a => a.IsDeleted == false && a.RoomId == roomingcrosspos[k].RoomId).FirstOrDefault();
                                                if (!string.IsNullOrEmpty(roomdetails.CrossPositionId))
                                                {
                                                    TourEntitiesPaxSlab = new List<TourEntities>();
                                                    if (lstPositions[p].ProductType.ToLower() == "meal")
                                                    {
                                                        if (!string.IsNullOrEmpty(lstPositions[p].MealType) && lstPositions[p].MealType.ToLower() == "lunch")
                                                        {
                                                            TourEntitiesPaxSlab = result.TourEntities.Where(a => a.IsDeleted == false && a.PositionID == roomdetails.CrossPositionId
                                                                              && roomdetails.ProductRange.ToUpper() == "MEAL (" + a.Flag + ")" && a.IsLunch).
                                                                              Select(a => new TourEntities { TourEntityID = a.TourEntityID, PaxSlab = a.PaxSlab, PaxSlabID = a.PaxSlabID }).ToList();
                                                        }
                                                        else if (!string.IsNullOrEmpty(lstPositions[p].MealType) && lstPositions[p].MealType.ToLower() == "dinner")
                                                        {
                                                            TourEntitiesPaxSlab = result.TourEntities.Where(a => a.IsDeleted == false && a.PositionID == roomdetails.CrossPositionId
                                                                              && roomdetails.ProductRange.ToUpper() == "MEAL (" + a.Flag + ")" && a.IsDinner).
                                                                              Select(a => new TourEntities { TourEntityID = a.TourEntityID, PaxSlab = a.PaxSlab, PaxSlabID = a.PaxSlabID }).ToList();
                                                        }
                                                    }
                                                    else if (lstPositions[p].ProductType.ToLower() == "assistant" && lstPositions[p].IsTourEntity)
                                                    {
                                                        //&& Convert.ToInt32(a.HowMany) > 0 
                                                        TourEntitiesPaxSlab = result.TourEntities.Where(a => a.IsDeleted == false && a.PositionID == roomdetails.CrossPositionId).
                                                                              Select(a => new TourEntities { TourEntityID = a.TourEntityID, PaxSlab = a.PaxSlab, PaxSlabID = a.PaxSlabID }).Distinct().ToList();
                                                    }
                                                    else
                                                    { //&& Convert.ToInt32(a.HowMany) > 0
                                                        TourEntitiesPaxSlab = result.TourEntities.Where(a => a.IsDeleted == false && a.PositionID == roomdetails.CrossPositionId
                                                                              && (a.RoomType.ToUpper() + " (" + a.Flag + ")") == roomdetails.ProductRange.ToUpper()
                                                                              && Convert.ToInt32(a.HowMany) > 0).
                                                                              Select(a => new TourEntities { TourEntityID = a.TourEntityID, PaxSlab = a.PaxSlab, PaxSlabID = a.PaxSlabID }).ToList();
                                                    }

                                                    if (TourEntitiesPaxSlab != null && TourEntitiesPaxSlab.Count > 0)
                                                    {
                                                        for (int j = 0; j < TourEntitiesPaxSlab.Count; j++)
                                                        {
                                                            objPricesInfo = new mPositionPriceQRF
                                                            {
                                                                QRFID = result.QRFID,
                                                                PositionId = lstPositions[p].PositionId,
                                                                DepartureId = result.Departures[i].Departure_Id,
                                                                Period = result.Departures[i].Date,
                                                                ContractPeriod = result.Departures[i].Date,
                                                                PaxSlabId = TourEntitiesPaxSlab[j].PaxSlabID,
                                                                PaxSlab = TourEntitiesPaxSlab[j].PaxSlab,
                                                                SupplierId = lstPositions[p].SupplierId,
                                                                Supplier = lstPositions[p].SupplierName,
                                                                RoomId = roomingcrosspos[k].RoomId,
                                                                IsSupplement = roomingcrosspos[k].IsSupplement,
                                                                ProductCategoryId = roomingcrosspos[k].ProductCategoryId,
                                                                ProductRangeId = roomingcrosspos[k].ProductRangeId,
                                                                ProductCategory = roomingcrosspos[k].ProductCategory,
                                                                ProductRange = roomingcrosspos[k].ProductRange,
                                                                BuyCurrencyId = currencyId,
                                                                BuyCurrency = currency,
                                                                TourEntityId = TourEntitiesPaxSlab[j].TourEntityID
                                                            };
                                                            if (addDaysToPeriod > 0)
                                                                objPricesInfo.ContractPeriod = Convert.ToDateTime(objPricesInfo.ContractPeriod).AddDays(addDaysToPeriod);

                                                            objPricesInfo.Type = prodrange.Where(a => a.VoyagerProductRange_Id == objPricesInfo.ProductRangeId).Select(b => b.PersonType).FirstOrDefault();
                                                            objPricesInfo.ProductRangeCode = prodrange.Where(a => a.VoyagerProductRange_Id == objPricesInfo.ProductRangeId).Select(b => b.ProductRangeCode).FirstOrDefault();
                                                            response.PositionPriceQRF.Add(objPricesInfo);
                                                        }
                                                    }

                                                }

                                            }
                                        }
                                    }
                                }
                            }

                            if (request.IsPrice)
                            {
                                #region Get Contract Rates By Service
                                ProdContractGetRes prodContractGetRes = new ProdContractGetRes();
                                ProdContractGetReq prodContractGetReq = new ProdContractGetReq
                                {
                                    QRFID = result.QRFID,
                                    ProductIDList = lstProductList,
                                    AgentId = result.AgentInfo.AgentID
                                };

                                var rangelist = response.PositionPriceQRF.Select(c => c.ProductRangeId).Distinct().ToList();
                                prodContractGetRes = _productRepository.GetContractRatesByProductID(prodContractGetReq, rangelist);
                                var prodid = "";

                                if (prodContractGetRes != null && prodContractGetRes.ProductContractInfo.Count > 0)
                                {
                                    for (int i = 0; i < response.PositionPriceQRF.Count; i++)
                                    {
                                        prodid = lstPositions.Where(a => a.PositionId == response.PositionPriceQRF[i].PositionId).Select(a => a.ProductID).FirstOrDefault();
                                        if (!string.IsNullOrEmpty(prodid))
                                        {
                                            var lstPCInfo = prodContractGetRes.ProductContractInfo.Where(a => a.SupplierId == response.PositionPriceQRF[i].SupplierId && a.ProductId == prodid
                                              && a.ProductRangeId == response.PositionPriceQRF[i].ProductRangeId
                                              && (a.FromDate <= response.PositionPriceQRF[i].ContractPeriod && response.PositionPriceQRF[i].ContractPeriod <= a.ToDate)).ToList();

                                            if (lstPCInfo != null && lstPCInfo.Count > 0)
                                            {
                                                foreach (var con in lstPCInfo)
                                                {
                                                    if (con.DayComboPattern == null || con.DayComboPattern == "")
                                                        con.DayComboPattern = "1111111";
                                                    char[] dayPattern = con.DayComboPattern.ToCharArray();
                                                    int dayNo = (int)Convert.ToDateTime(response.PositionPriceQRF[i].ContractPeriod).DayOfWeek;

                                                    if (dayNo == 0)
                                                        dayNo = 7;

                                                    if (dayPattern[dayNo - 1] == '1')
                                                    {
                                                        response.PositionPriceQRF[i].ContractId = con.ContractId;
                                                        response.PositionPriceQRF[i].ContractPrice = Convert.ToDouble(con.Price);
                                                        response.PositionPriceQRF[i].BudgetPrice = Convert.ToDouble(con.Price);
                                                        response.PositionPriceQRF[i].BuyCurrencyId = con.CurrencyId;
                                                        response.PositionPriceQRF[i].BuyCurrency = con.Currency;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                #endregion

                                #region GetMarkupValue and Add in Contract price

                                bool IsSalesOfficeUser = _genericRepository.IsSalesOfficeUser(request.LoginUserId);

                                if (IsSalesOfficeUser == true)
                                {
                                    var UserCompany_Id = _MongoContext.mUsers.AsQueryable().Where(x => x.UserName.ToLower() == request.LoginUserId.ToLower().Trim()).Select(y => y.Company_Id).FirstOrDefault();
                                    var Markup_Id = _MongoContext.mCompanies.AsQueryable().Where(x => x.Company_Id == UserCompany_Id && x.Markups.Any(y => y.Markup_For == "Groups")).FirstOrDefault().Markups.FirstOrDefault().Markup_Id;

                                    if (!string.IsNullOrEmpty(Markup_Id))
                                    {
                                        for (int i = 0; i < response.PositionPriceQRF.Count; i++)
                                        {
                                            if (!string.IsNullOrEmpty(response.PositionPriceQRF[i].ContractId))
                                            {
                                                ProdMarkupsGetReq prodMarkupsGetReq = new ProdMarkupsGetReq();

                                                prodMarkupsGetReq.MarkupsId = Markup_Id;
                                                prodMarkupsGetReq.ProductType = lstPositions.Where(a => a.PositionId == response.PositionPriceQRF[i].PositionId).Select(b => b.ProductType).FirstOrDefault();
                                                var MarkupDetails = _productRepository.GetProdMarkups(prodMarkupsGetReq).Result;

                                                if (MarkupDetails != null)
                                                {
                                                    double MarkupValue = Convert.ToDouble(MarkupDetails.PercMarkUp) <= 0 ? Convert.ToDouble(MarkupDetails.FixedMarkUp) : Convert.ToDouble(MarkupDetails.PercMarkUp);

                                                    if (MarkupDetails.MARKUPTYPE == "Fixed")
                                                    {
                                                        double markup = MarkupValue;
                                                        if (MarkupDetails.CURRENCY_ID != response.PositionPriceQRF[i].BuyCurrencyId)
                                                        {
                                                            var rate = _genericRepository.getExchangeRate(MarkupDetails.CURRENCY_ID, response.PositionPriceQRF[i].BuyCurrencyId, request.QRFID);
                                                            if (rate != null)
                                                                markup = MarkupValue * Convert.ToDouble(rate.Value);
                                                        }
                                                        if (markup > 0)
                                                            response.PositionPriceQRF[i].BudgetPrice = response.PositionPriceQRF[i].BudgetPrice + Math.Round(markup, 2);
                                                    }
                                                    else
                                                    {
                                                        response.PositionPriceQRF[i].BudgetPrice = response.PositionPriceQRF[i].BudgetPrice + (response.PositionPriceQRF[i].BudgetPrice * MarkupValue / 100);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                #endregion

                                #region Save Currency into mPosition
                                var objPosition = new mQRFPosition();
                                var posPr = new mPositionPriceQRF();

                                var modelsPosCur = new WriteModel<mQRFPosition>[lstPositions.ToList().Count];
                                var curposids = lstPositions.Where(a => a.IsDeleted == false).Select(a => a.PositionId).ToList();
                                List<mQRFPosition> lstCurPosition = _MongoContext.mQRFPosition.AsQueryable().Where(a => curposids.Contains(a.PositionId) && a.IsDeleted == false).ToList();

                                for (int p = 0; p < lstPositions.Count; p++)
                                {
                                    objPosition = new mQRFPosition();
                                    objPosition = lstCurPosition.Where(a => a.PositionId == lstPositions[p].PositionId).FirstOrDefault();
                                    posPr = response.PositionPriceQRF.Where(a => a.PositionId == lstPositions[p].PositionId).FirstOrDefault();
                                    objPosition.BuyCurrencyId = posPr.BuyCurrencyId;
                                    objPosition.BuyCurrency = posPr.BuyCurrency;
                                    objPosition.EditDate = DateTime.Now;
                                    modelsPosCur[p] = new ReplaceOneModel<mQRFPosition>(new BsonDocument("PositionId", objPosition.PositionId), objPosition) { IsUpsert = true };
                                }
                                var BulkWriteRes = await _MongoContext.mQRFPosition.BulkWriteAsync(modelsPosCur);
                                #endregion

                                #region Get Saved Data from mPositionPrices and update IsDeleted to True those are deleted services and also replace the ProdRangeName
                                var roomingnmdiff = new List<mPositionPriceQRF>();

                                if (resultPosPrices != null && resultPosPrices.Count > 0)
                                {
                                    if (response != null && response.PositionPriceQRF.Count > 0)
                                    {
                                        var PosPrice = new List<mPositionPriceQRF>();
                                        for (int i = 0; i < response.PositionPriceQRF.Count; i++)
                                        {
                                            var pospr = resultPosPrices.Where(a => a.QRFID == response.PositionPriceQRF[i].QRFID && a.PositionId == response.PositionPriceQRF[i].PositionId
                                             && a.DepartureId == response.PositionPriceQRF[i].DepartureId && a.PaxSlabId == response.PositionPriceQRF[i].PaxSlabId &&
                                             a.RoomId == response.PositionPriceQRF[i].RoomId && a.ProductRangeId == response.PositionPriceQRF[i].ProductRangeId).FirstOrDefault();
                                            if (pospr != null)
                                            {
                                                response.PositionPriceQRF[i].PositionPriceId = pospr.PositionPriceId;
                                                response.PositionPriceQRF[i].BudgetPrice = pospr.BudgetPrice;
                                            }
                                        }

                                        var posprexist = response.PositionPriceQRF.Where(c => !string.IsNullOrEmpty(c.PositionPriceId)).ToList();
                                        var roomingids = resultPosPrices.FindAll(a => !posprexist.Exists(b => b.PositionPriceId == a.PositionPriceId));
                                        roomingnmdiff = resultPosPrices.FindAll(a => !posprexist.Exists(b => b.ProductRange == a.ProductRange));
                                        roomingids.AddRange(roomingnmdiff);
                                        if (roomingids != null && roomingids.Count > 0)
                                        {
                                            //UpdateResult resultFlag;
                                            //foreach (var item in roomingids)
                                            //{
                                            //    resultFlag = await _MongoContext.mPositionPriceQRF.UpdateOneAsync(Builders<mPositionPriceQRF>.Filter.Eq("PositionPriceId", item.PositionPriceId),
                                            //        Builders<mPositionPriceQRF>.Update.Set("IsDeleted", true).Set("EditDate", DateTime.Now).
                                            //        Set("EditUser", usernm));
                                            //}
                                            var posPrids = roomingids.Select(b => b.PositionPriceId).ToList();
                                            var filter = Builders<mPositionPriceQRF>.Filter.Where(a => posPrids.Contains(a.PositionPriceId));
                                            var update = Builders<mPositionPriceQRF>.Update.Set(a => a.IsDeleted, true)
                                                                                        .Set(a => a.EditUser, usernm)
                                                                                        .Set(a => a.EditDate, DateTime.Now);
                                            UpdateResult upResult = await _MongoContext.mPositionPriceQRF.UpdateManyAsync(filter, update);
                                        }
                                    }

                                    response.PositionPriceQRF = response.PositionPriceQRF.OrderBy(a => a.ProductRange).ToList();
                                }
                                #endregion

                                #region Set PositionPrice
                                if (response != null && response.PositionPriceQRF != null && response.PositionPriceQRF.Count > 0)
                                {
                                    //the below code is for if from frontend existing Dropdown of Services is changed then deactive the service and after that insert it.dectivation code is above
                                    var prposlist = response.PositionPriceQRF.Where(a => string.IsNullOrEmpty(a.PositionPriceId) || a.PositionPriceId == Guid.Empty.ToString()).ToList();
                                    if (roomingnmdiff != null && roomingnmdiff.Count > 0)
                                    {
                                        var posids = roomingnmdiff.Select(a => a.PositionPriceId).ToList();
                                        List<mPositionPriceQRF> objPositionPrices = response.PositionPriceQRF.Where(a => posids.Contains(a.PositionPriceId)).ToList();
                                        objPositionPrices.ForEach(a => { a.BudgetPrice = 0; a.PositionPriceId = ""; });
                                        prposlist.AddRange(objPositionPrices);
                                    }
                                    if (prposlist != null && prposlist.Count > 0)
                                    {
                                        var lstPricesInfo = new List<mPositionPriceQRF>();
                                        foreach (var item in prposlist)
                                        {
                                            if (string.IsNullOrEmpty(item.PositionPriceId) || item.PositionPriceId == Guid.Empty.ToString())
                                            {
                                                objPricesInfo = new mPositionPriceQRF();
                                                item.PositionPriceId = Guid.NewGuid().ToString();
                                                item.CreateUser = usernm;
                                                item.CreateDate = DateTime.Now;
                                                item.EditUser = "";
                                                item.EditDate = null;
                                                item.IsDeleted = false;
                                                objPricesInfo = item;
                                                lstPricesInfo.Add(objPricesInfo);
                                            }
                                        }
                                        await _MongoContext.mPositionPriceQRF.InsertManyAsync(lstPricesInfo);
                                        response.ResponseStatus.Status = "Success";
                                        response.ResponseStatus.ErrorMessage = "Saved Successfully.";
                                    }

                                    var prposlistUpdate = response.PositionPriceQRF.Where(a => !string.IsNullOrEmpty(a.PositionPriceId) && a.PositionPriceId != Guid.Empty.ToString()).ToList();

                                    if (prposlistUpdate != null && prposlistUpdate.Count > 0)
                                    {
                                        var PositionIdlist = prposlistUpdate.Select(a => a.PositionId).ToList();
                                        var positionlist = _MongoContext.mQRFPosition.AsQueryable().Where(a => PositionIdlist.Contains(a.PositionId) && a.IsDeleted == false).ToList();

                                        var PositionPriceIdlist = prposlistUpdate.Select(a => a.PositionPriceId).ToList();
                                        var prposlistUpdatelist = _MongoContext.mPositionPriceQRF.AsQueryable().Where(a => PositionPriceIdlist.Contains(a.PositionPriceId) && a.IsDeleted == false);

                                        mPositionPriceQRF res = new mPositionPriceQRF();
                                        var models = new WriteModel<mPositionPriceQRF>[prposlistUpdatelist.ToList().Count];
                                        int j = 0;
                                        foreach (var item in prposlistUpdatelist)
                                        {
                                            res = prposlistUpdate.Where(a => a.PositionPriceId == item.PositionPriceId).FirstOrDefault();

                                            item.EditDate = DateTime.Now;
                                            item.EditUser = usernm;
                                            item.ContractId = res.ContractId;
                                            item.ContractPrice = res.ContractPrice;
                                            item.BudgetPrice = res.BudgetPrice;
                                            item.Type = res.Type;
                                            item.BuyCurrencyId = res.BuyCurrencyId;
                                            item.BuyCurrency = res.BuyCurrency;
                                            item.Period = res.Period;
                                            models[j] = new ReplaceOneModel<mPositionPriceQRF>(new BsonDocument("PositionPriceId", item.PositionPriceId), item) { IsUpsert = true };
                                            j = j + 1;
                                        }
                                        var lst = new List<WriteModel<mPositionPriceQRF>>();
                                        for (int i = 0; i < prposlistUpdatelist.Count(); i = i + 100)
                                        {
                                            if (i + 100 > prposlistUpdatelist.Count())
                                            {
                                                lst = models.ToList().GetRange(i, prposlistUpdatelist.Count() - i);
                                            }
                                            else
                                            {
                                                lst = models.ToList().GetRange(i, 100);
                                            }
                                            Thread t = new Thread(new ThreadStart(() => UpdateBulkQRFPositionPrice(lst)));
                                            t.Start();
                                        }
                                        response.ResponseStatus.Status = BulkWriteRes.MatchedCount > 0 ? "Success" : "Error";
                                        response.ResponseStatus.ErrorMessage = BulkWriteRes.MatchedCount > 0 ? "Saved Successfully." : "Not saved.";
                                    }
                                }
                                #endregion
                            }

                            if (request.IsFOC)
                            {
                                #region Get ContractId from mPositionPrice                       
                                var resultPosPrice = _MongoContext.mPositionPriceQRF.AsQueryable().Where(a => a.QRFID == result.QRFID && positionids.Contains(a.PositionId) && a.IsDeleted == false).ToList();
                                if (resultPosPrice != null && resultPosPrice.Count > 0)
                                {
                                    if (response != null && response.PositionFOCQRF.Count > 0)
                                    {
                                        for (int i = 0; i < response.PositionFOCQRF.Count; i++)
                                        {
                                            var pospr = resultPosPrice.Where(a => a.PositionId == response.PositionFOCQRF[i].PositionId && a.QRFID == response.PositionFOCQRF[i].QRFID
                                            && a.DepartureId == response.PositionFOCQRF[i].DepartureId && a.PaxSlabId == response.PositionFOCQRF[i].PaxSlabId
                                            && a.RoomId == response.PositionFOCQRF[i].RoomId).FirstOrDefault();

                                            if (pospr != null)
                                            {
                                                response.PositionFOCQRF[i].ContractId = pospr.ContractId;
                                            }
                                        }
                                    }
                                }
                                #endregion

                                #region Get FOC Quantity mProductFreePlacePolicy
                                var resultProdFPP = _MongoContext.mProductFreePlacePolicy.AsQueryable().Where(a => prodids.Contains(a.Product_Id)).ToList();
                                if (resultProdFPP != null && resultProdFPP.Count > 0)
                                {
                                    if (response != null && response.PositionFOCQRF.Count > 0)
                                    {
                                        for (int i = 0; i < response.PositionFOCQRF.Count; i++)
                                        {
                                            var posFPP = resultProdFPP.Where(a => a.Product_Id == response.PositionFOCQRF[i].ProductId).ToList();
                                            if (posFPP != null && posFPP.Count > 0)
                                            {
                                                for (int j = 0; j < posFPP.Count; j++)
                                                {
                                                    string[] paxSlab = response.PositionFOCQRF[i].PaxSlab.Split(" - ");
                                                    if (
                                                        response.PositionFOCQRF[i].ContractId == posFPP[j].ProductContract_Id &&
                                                        Convert.ToString(response.PositionFOCQRF[i].ProductRange.Replace("(" + response.PositionFOCQRF[i].Type.ToUpper() + ")", "").ToLower()).Trim() == Convert.ToString(posFPP[j].Subprod.ToLower()).Trim() &&
                                                        (posFPP[j].DateMin <= response.PositionFOCQRF[i].Period && response.PositionFOCQRF[i].Period <= posFPP[j].DateMax) &&
                                                        (Convert.ToInt16(paxSlab[0]) <= posFPP[j].MinPers && posFPP[j].MinPers <= Convert.ToInt16(paxSlab[0]))
                                                        )
                                                    {
                                                        response.PositionFOCQRF[i].FOCQty = posFPP[j].Quantity;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                #endregion

                                #region Get Saved Data from mPositionFOC and update IsDeleted to True for those are deleted services
                                var roomingnmdiff = new List<mQRFPositionFOC>();
                                var resultPosFOC = _MongoContext.mQRFPositionFOC.AsQueryable().Where(a => a.QRFID == result.QRFID && positionids.Contains(a.PositionId) && a.IsDeleted == false).ToList();
                                if (resultPosFOC != null && resultPosFOC.Count > 0)
                                {
                                    if (response != null && response.PositionFOCQRF.Count > 0)
                                    {
                                        foreach (mQRFPositionFOC posFOC in response.PositionFOCQRF)
                                        {
                                            var resultFOC = resultPosFOC.Where(a => a.PositionId == posFOC.PositionId && a.QRFID == posFOC.QRFID
                                                                                        && a.DepartureId == posFOC.DepartureId && a.PaxSlabId == posFOC.PaxSlabId
                                                                                        && a.RoomId == posFOC.RoomId && a.ProductRangeId == posFOC.ProductRangeId).FirstOrDefault();
                                            if (resultFOC != null)
                                            {
                                                posFOC.PositionFOCId = resultFOC.PositionFOCId;
                                                posFOC.FOCQty = resultFOC.FOCQty;
                                            }
                                        }

                                        //for (int i = 0; i < response.PositionFOCQRF.Count; i++)
                                        //{
                                        //    var resultFOC = resultPosFOC.Where(a => a.PositionId == response.PositionFOCQRF[i].PositionId && a.QRFID == response.PositionFOCQRF[i].QRFID
                                        //    && a.DepartureId == response.PositionFOCQRF[i].DepartureId && a.PaxSlabId == response.PositionFOCQRF[i].PaxSlabId
                                        //    && a.RoomId == response.PositionFOCQRF[i].RoomId && a.ProductRangeId == response.PositionFOCQRF[i].ProductRangeId).FirstOrDefault();

                                        //    if (resultFOC != null)
                                        //    {
                                        //        response.PositionFOCQRF[i].PositionFOCId = resultFOC.PositionFOCId;
                                        //        response.PositionFOCQRF[i].FOCQty = resultFOC.FOCQty;
                                        //    }
                                        //}
                                        var posprexist = response.PositionFOCQRF.Where(c => !string.IsNullOrEmpty(c.PositionFOCId)).ToList();
                                        var roomingids = resultPosFOC.FindAll(a => !posprexist.Exists(b => b.PositionFOCId == a.PositionFOCId));
                                        roomingnmdiff = resultPosFOC.FindAll(a => !posprexist.Exists(b => b.ProductRange == a.ProductRange));
                                        roomingids.AddRange(roomingnmdiff);

                                        if (roomingids != null && roomingids.Count > 0)
                                        {
                                            //UpdateResult resultFlag;
                                            //foreach (var item in roomingids)
                                            //{
                                            //    resultFlag = await _MongoContext.mQRFPositionFOC.UpdateOneAsync(Builders<mQRFPositionFOC>.Filter.Eq("PositionFOCId", item.PositionFOCId),
                                            //        Builders<mQRFPositionFOC>.Update.Set("IsDeleted", true).Set("EditDate", DateTime.Now).
                                            //        Set("EditUser", usernm));
                                            //}
                                            var posPrids = roomingids.Select(b => b.PositionFOCId).ToList();
                                            var filter = Builders<mQRFPositionFOC>.Filter.Where(a => posPrids.Contains(a.PositionFOCId));
                                            var update = Builders<mQRFPositionFOC>.Update.Set(a => a.IsDeleted, true)
                                                                                        .Set(a => a.EditUser, usernm)
                                                                                        .Set(a => a.EditDate, DateTime.Now);
                                            UpdateResult upResult = await _MongoContext.mQRFPositionFOC.UpdateManyAsync(filter, update);
                                        }
                                    }
                                }
                                #endregion

                                #region On Position Save button set All FOC
                                if (response.PositionFOCQRF != null && response.PositionFOCQRF.Count > 0)
                                {
                                    var resfoc = response.PositionFOCQRF.Where(a => string.IsNullOrEmpty(a.PositionFOCId) || a.PositionFOCId == Guid.Empty.ToString()).ToList();
                                    if (roomingnmdiff != null && roomingnmdiff.Count > 0)
                                    {
                                        var posids = roomingnmdiff.Select(a => a.PositionFOCId).ToList();
                                        List<mQRFPositionFOC> objmPositionFOC = response.PositionFOCQRF.Where(a => posids.Contains(a.PositionFOCId)).ToList();
                                        objmPositionFOC.ForEach(a => { a.FOCQty = 0; a.PositionFOCId = ""; });
                                        resfoc.AddRange(objmPositionFOC);
                                    }

                                    if (resfoc != null && resfoc.Count > 0)
                                    {
                                        var lstFOCInfo = new List<mQRFPositionFOC>();
                                        foreach (var item in resfoc)
                                        {
                                            if (string.IsNullOrEmpty(item.PositionFOCId) || item.PositionFOCId == Guid.Empty.ToString())
                                            {
                                                objFOCInfo = new mQRFPositionFOC();
                                                item.PositionFOCId = Guid.NewGuid().ToString();
                                                item.CreateUser = usernm;
                                                item.CreateDate = DateTime.Now;
                                                item.EditUser = "";
                                                item.EditDate = null;
                                                item.IsDeleted = false;
                                                objFOCInfo = item;
                                                lstFOCInfo.Add(objFOCInfo);
                                            }
                                        }
                                        await _MongoContext.mQRFPositionFOC.InsertManyAsync(lstFOCInfo);
                                        response.ResponseStatus.Status = "Success";
                                        response.ResponseStatus.ErrorMessage = "Saved Successfully.";
                                    }
                                    else
                                    {
                                        response.ResponseStatus.Status = "Success";
                                        response.ResponseStatus.ErrorMessage = "Saved Successfully.";
                                    }
                                }
                                #endregion
                            }
                        }
                    }
                    else
                    {
                        response.ResponseStatus.ErrorMessage = "QRF ID not exist.";
                        response.ResponseStatus.Status = "Failure";
                    }
                }
                else
                {
                    mPositionPrice objPricesInfo = new mPositionPrice();
                    mPositionFOC objFOCInfo = new mPositionFOC();
                    mQuote result = _MongoContext.mQuote.AsQueryable().Where(a => a.QRFID == request.QRFID).FirstOrDefault();

                    if (result != null)
                    {
                        result.PaxSlabDetails.PaxSlabs = result.PaxSlabDetails.PaxSlabs.Where(a => a.IsDeleted == false).ToList();
                        result.Departures = result.Departures.Where(a => a.IsDeleted == false).ToList();
                        List<mPosition> lstPositions = new List<mPosition>();

                        if (!string.IsNullOrEmpty(request.PositionId))
                        {
                            lstPositions = _MongoContext.mPosition.AsQueryable().Where(a => a.PositionId == request.PositionId && a.IsDeleted == false).ToList();
                        }
                        else if (request.ProductTypeList != null && request.ProductTypeList.Count > 0)
                        {
                            lstPositions = _MongoContext.mPosition.AsQueryable().Where(a => a.QRFID == result.QRFID && request.ProductTypeList.Contains(a.ProductType) && a.IsDeleted == false).ToList();
                        }
                        else if (request.PositionIdList != null && request.PositionIdList.Count > 0)
                        {
                            lstPositions = _MongoContext.mPosition.AsQueryable().Where(a => a.QRFID == result.QRFID && request.PositionIdList.Contains(a.PositionId) && a.IsDeleted == false).ToList();
                        }
                        else
                        {
                            lstPositions = _MongoContext.mPosition.AsQueryable().Where(a => a.QRFID == result.QRFID && a.IsDeleted == false).ToList();
                        }

                        if (lstPositions != null && lstPositions.Count > 0)
                        {
                            var usernm = !string.IsNullOrEmpty(lstPositions[0].EditUser) ? lstPositions[0].EditUser : lstPositions[0].CreateUser;
                            var lstProductList = new List<string>();
                            string supplierId = "";
                            List<ProductRangeInfo> prodrange = new List<ProductRangeInfo>();
                            var prorangelist = new List<string>();
                            var currencyId = "";
                            var currency = "";
                            var roomingcrosspos = new List<RoomDetailsInfo>();
                            var supplierids = new List<string>();
                            var curids = new List<string>();
                            //var currencyidlist = new List<mProductSupplier>();
                            var currencyidlist = new List<ProductSupplierInfo>();
                            var currencylist = new List<mCurrency>();
                            var roomingcrossposnone = new List<RoomDetailsInfo>();
                            var roomDetailsList = new List<RoomDetailsInfo>();
                            var lstProductSupplier = new List<ProductSupplier>();

                            var positionids = lstPositions.Select(a => a.PositionId).ToList();
                            lstProductList = lstPositions.Select(a => a.ProductID).ToList();
                            var procatdetails = lstPositions.Select(a => a.RoomDetailsInfo).ToList();

                            var resultPosPrices = _MongoContext.mPositionPrice.AsQueryable().Where(a => a.QRFID == result.QRFID && positionids.Contains(a.PositionId) && a.IsDeleted == false).ToList();

                            foreach (var item in procatdetails)
                            {
                                roomDetailsList.AddRange(item);
                            }

                            var prodids = lstPositions.Select(b => b.ProductID).ToList();
                            prodrange = request.ProductRangeInfo;

                            if (request.IsPrice)
                            {
                                supplierids = lstPositions.Select(a => a.SupplierId).ToList();
                                currencyidlist = _MongoContext.Products.AsQueryable().Where(p => lstProductList.Contains(p.VoyagerProduct_Id)).
                                    Select(a => new ProductSupplierInfo
                                    {
                                        Product_Id = a.VoyagerProduct_Id,
                                        ProductSupplier = a.ProductSuppliers
                                    }).ToList();

                                //currencyidlist = _MongoContext.mProductSupplier.AsQueryable().Where(s => supplierids.Contains(s.Company_Id)).ToList();
                                //currencyidlist = _MongoContext.mProductSupplier.AsQueryable().Where(s => supplierids.Contains(s.Company_Id)).
                                //                Select(a => new SupplierInfo
                                //                {
                                //                    Company_Id = a.Company_Id,
                                //                    Currency_Id = a.Currency_Id,
                                //                    Product_Id = a.Product_Id
                                //                }).ToList();

                                //curids = currencyidlist.Select(a => a.Currency_Id).ToList();
                                //currencylist = _MongoContext.mCurrency.AsQueryable().Where(c => curids.Contains(c.VoyagerCurrency_Id)).ToList();
                            }
                            result.TourEntities.ForEach(a => a.Flag = (a.Type.Contains("Coach") || a.Type.Contains("LDC") ? "DRIVER" : a.Type.Contains("Guide") ? "GUIDE" : "GUIDE"));

                            for (int p = 0; p < lstPositions.Count; p++)
                            {
                                supplierId = lstPositions[p].SupplierId;
                                int addDaysToPeriod = 0;
                                addDaysToPeriod = (lstPositions[p].DayNo - 1) + (lstPositions[p].Duration - 1);

                                roomingcrossposnone = lstPositions[p].RoomDetailsInfo.Where(a => string.IsNullOrEmpty(a.CrossPositionId) && a.IsDeleted == false).ToList();

                                if (request.IsPrice)
                                {
                                    response.StandardPrice = lstPositions[p].StandardPrice;
                                    //currencyId = currencyidlist.Where(s => s.Company_Id == supplierId && s.Product_Id == lstPositions[p].ProductID).Select(a => a.Currency_Id).FirstOrDefault();
                                    //currency = currencylist.Where(c => c.VoyagerCurrency_Id == currencyId).Select(a => a.Currency).FirstOrDefault();

                                    lstProductSupplier = currencyidlist.Where(a => a.Product_Id == lstPositions[p].ProductID).FirstOrDefault().ProductSupplier;
                                    var objProductSupplier = lstProductSupplier.Where(a => a.Company_Id == supplierId).FirstOrDefault();
                                    currencyId = objProductSupplier.CurrencyId;
                                    currency = objProductSupplier.CurrencyName;

                                    roomingcrosspos = lstPositions[p].RoomDetailsInfo.Where(a => !string.IsNullOrEmpty(a.CrossPositionId) && a.IsDeleted == false).
                                    Select(a => new RoomDetailsInfo
                                    {
                                        IsDeleted = a.IsDeleted,
                                        IsSupplement = a.IsSupplement,
                                        ProdDesc = a.ProdDesc,
                                        ProductCategory = a.ProductCategory,
                                        ProductCategoryId = a.ProductCategoryId,
                                        ProductRange = a.ProductRange,
                                        ProductRangeId = a.ProductRangeId,
                                        RoomId = a.RoomId,
                                        RoomSequence = a.RoomSequence
                                    }).Distinct().ToList();
                                }
                                if (request.IsFOC)
                                {
                                    response.StandardFOC = lstPositions[p].StandardFOC;
                                }

                                for (int i = 0; i < result.Departures.Count; i++)
                                {
                                    if (roomingcrossposnone != null && roomingcrossposnone.Count > 0)
                                    {
                                        for (int j = 0; j < result.PaxSlabDetails.PaxSlabs.Count; j++)
                                        {
                                            for (int k = 0; k < roomingcrossposnone.Count; k++)
                                            {
                                                if (request.IsPrice)
                                                {
                                                    //Price added in objPricesInfo
                                                    objPricesInfo = new mPositionPrice
                                                    {
                                                        QRFID = result.QRFID,
                                                        PositionId = lstPositions[p].PositionId,
                                                        DepartureId = result.Departures[i].Departure_Id,
                                                        Period = result.Departures[i].Date,
                                                        ContractPeriod = result.Departures[i].Date,
                                                        PaxSlabId = result.PaxSlabDetails.PaxSlabs[j].PaxSlab_Id,
                                                        PaxSlab = result.PaxSlabDetails.PaxSlabs[j].From.ToString() + " - " + result.PaxSlabDetails.PaxSlabs[j].To.ToString(),
                                                        SupplierId = supplierId,
                                                        Supplier = lstPositions[p].SupplierName,
                                                        RoomId = roomingcrossposnone[k].RoomId,
                                                        IsSupplement = roomingcrossposnone[k].IsSupplement,
                                                        ProductCategoryId = roomingcrossposnone[k].ProductCategoryId,
                                                        ProductRangeId = roomingcrossposnone[k].ProductRangeId,
                                                        ProductCategory = roomingcrossposnone[k].ProductCategory,
                                                        ProductRange = roomingcrossposnone[k].ProductRange,
                                                        BuyCurrencyId = currencyId,
                                                        BuyCurrency = currency
                                                    };
                                                    if (addDaysToPeriod > 0)
                                                        objPricesInfo.ContractPeriod = Convert.ToDateTime(objPricesInfo.ContractPeriod).AddDays(addDaysToPeriod);

                                                    objPricesInfo.Type = prodrange.Where(a => a.VoyagerProductRange_Id == objPricesInfo.ProductRangeId).Select(b => b.PersonType).FirstOrDefault();
                                                    objPricesInfo.ProductRangeCode = prodrange.Where(a => a.VoyagerProductRange_Id == objPricesInfo.ProductRangeId).Select(b => b.ProductRangeCode).FirstOrDefault();
                                                    response.PositionPrice.Add(objPricesInfo);
                                                }

                                                if (request.IsFOC)
                                                {
                                                    //FOC added in objFOCInfo
                                                    objFOCInfo = new mPositionFOC
                                                    {
                                                        QRFID = result.QRFID,
                                                        PositionId = lstPositions[p].PositionId,
                                                        DepartureId = result.Departures[i].Departure_Id,
                                                        Period = result.Departures[i].Date,
                                                        ContractPeriod = result.Departures[i].Date,
                                                        PaxSlabId = result.PaxSlabDetails.PaxSlabs[j].PaxSlab_Id,
                                                        PaxSlab = result.PaxSlabDetails.PaxSlabs[j].From.ToString() + " - " + result.PaxSlabDetails.PaxSlabs[j].To.ToString(),
                                                        CityId = lstPositions[p].CityID,
                                                        CityName = lstPositions[p].CityName,
                                                        ProductId = lstPositions[p].ProductID,
                                                        ProductName = lstPositions[p].ProductName,
                                                        SupplierId = supplierId,
                                                        Supplier = lstPositions[p].SupplierName,
                                                        RoomId = roomingcrossposnone[k].RoomId,
                                                        IsSupplement = roomingcrossposnone[k].IsSupplement,
                                                        ProductCategoryId = roomingcrossposnone[k].ProductCategoryId,
                                                        ProductRangeId = roomingcrossposnone[k].ProductRangeId,
                                                        ProductCategory = roomingcrossposnone[k].ProductCategory,
                                                        ProductRange = roomingcrossposnone[k].ProductRange,
                                                        Quantity = result.PaxSlabDetails.PaxSlabs[j].From
                                                    };
                                                    if (addDaysToPeriod > 0)
                                                        objFOCInfo.ContractPeriod = Convert.ToDateTime(objFOCInfo.ContractPeriod).AddDays(addDaysToPeriod);
                                                    objFOCInfo.Type = prodrange.Where(a => a.VoyagerProductRange_Id == objFOCInfo.ProductRangeId).Select(b => b.PersonType).FirstOrDefault();
                                                    response.PositionFOC.Add(objFOCInfo);
                                                }
                                            }
                                        }
                                    }

                                    var TourEntitiesPaxSlab = new List<TourEntities>();
                                    var roomdetails = new RoomDetailsInfo();
                                    if (request.IsPrice)
                                    {
                                        if (roomingcrosspos != null && roomingcrosspos.Count > 0)
                                        {
                                            for (int k = 0; k < roomingcrosspos.Count; k++)
                                            {
                                                roomdetails = lstPositions[p].RoomDetailsInfo.Where(a => a.IsDeleted == false && a.RoomId == roomingcrosspos[k].RoomId).FirstOrDefault();
                                                if (!string.IsNullOrEmpty(roomdetails.CrossPositionId))
                                                {
                                                    TourEntitiesPaxSlab = new List<TourEntities>();
                                                    if (lstPositions[p].ProductType.ToLower() == "meal")
                                                    {
                                                        if (!string.IsNullOrEmpty(lstPositions[p].MealType) && lstPositions[p].MealType.ToLower() == "lunch")
                                                        {
                                                            TourEntitiesPaxSlab = result.TourEntities.Where(a => a.IsDeleted == false && a.PositionID == roomdetails.CrossPositionId
                                                                              && roomdetails.ProductRange.ToUpper() == "MEAL (" + a.Flag + ")" && a.IsLunch).
                                                                              Select(a => new TourEntities { TourEntityID = a.TourEntityID, PaxSlab = a.PaxSlab, PaxSlabID = a.PaxSlabID }).ToList();
                                                        }
                                                        else if (!string.IsNullOrEmpty(lstPositions[p].MealType) && lstPositions[p].MealType.ToLower() == "dinner")
                                                        {
                                                            TourEntitiesPaxSlab = result.TourEntities.Where(a => a.IsDeleted == false && a.PositionID == roomdetails.CrossPositionId
                                                                              && roomdetails.ProductRange.ToUpper() == "MEAL (" + a.Flag + ")" && a.IsDinner).
                                                                              Select(a => new TourEntities { TourEntityID = a.TourEntityID, PaxSlab = a.PaxSlab, PaxSlabID = a.PaxSlabID }).ToList();
                                                        }
                                                    }
                                                    else if (lstPositions[p].ProductType.ToLower() == "assistant" && lstPositions[p].IsTourEntity)
                                                    {
                                                        //&& Convert.ToInt32(a.HowMany) > 0
                                                        TourEntitiesPaxSlab = result.TourEntities.Where(a => a.IsDeleted == false && a.PositionID == roomdetails.CrossPositionId).
                                                                              Select(a => new TourEntities { TourEntityID = a.TourEntityID, PaxSlab = a.PaxSlab, PaxSlabID = a.PaxSlabID }).Distinct().ToList();
                                                    }
                                                    else
                                                    {
                                                        TourEntitiesPaxSlab = result.TourEntities.Where(a => a.IsDeleted == false && a.PositionID == roomdetails.CrossPositionId
                                                                              && (a.RoomType.ToUpper() + " (" + a.Flag + ")") == roomdetails.ProductRange.ToUpper()
                                                                              && Convert.ToInt32(a.HowMany) > 0).
                                                                              Select(a => new TourEntities { TourEntityID = a.TourEntityID, PaxSlab = a.PaxSlab, PaxSlabID = a.PaxSlabID }).ToList();
                                                    }

                                                    if (TourEntitiesPaxSlab != null && TourEntitiesPaxSlab.Count > 0)
                                                    {
                                                        for (int j = 0; j < TourEntitiesPaxSlab.Count; j++)
                                                        {
                                                            objPricesInfo = new mPositionPrice
                                                            {
                                                                QRFID = result.QRFID,
                                                                PositionId = lstPositions[p].PositionId,
                                                                DepartureId = result.Departures[i].Departure_Id,
                                                                Period = result.Departures[i].Date,
                                                                ContractPeriod = result.Departures[i].Date,
                                                                PaxSlabId = TourEntitiesPaxSlab[j].PaxSlabID,
                                                                PaxSlab = TourEntitiesPaxSlab[j].PaxSlab,
                                                                SupplierId = lstPositions[p].SupplierId,
                                                                Supplier = lstPositions[p].SupplierName,
                                                                RoomId = roomingcrosspos[k].RoomId,
                                                                IsSupplement = roomingcrosspos[k].IsSupplement,
                                                                ProductCategoryId = roomingcrosspos[k].ProductCategoryId,
                                                                ProductRangeId = roomingcrosspos[k].ProductRangeId,
                                                                ProductCategory = roomingcrosspos[k].ProductCategory,
                                                                ProductRange = roomingcrosspos[k].ProductRange,
                                                                BuyCurrencyId = currencyId,
                                                                BuyCurrency = currency,
                                                                TourEntityId = TourEntitiesPaxSlab[j].TourEntityID
                                                            };
                                                            if (addDaysToPeriod > 0)
                                                                objPricesInfo.ContractPeriod = Convert.ToDateTime(objPricesInfo.ContractPeriod).AddDays(addDaysToPeriod);

                                                            objPricesInfo.Type = prodrange.Where(a => a.VoyagerProductRange_Id == objPricesInfo.ProductRangeId).Select(b => b.PersonType).FirstOrDefault();
                                                            objPricesInfo.ProductRangeCode = prodrange.Where(a => a.VoyagerProductRange_Id == objPricesInfo.ProductRangeId).Select(b => b.ProductRangeCode).FirstOrDefault();
                                                            response.PositionPrice.Add(objPricesInfo);
                                                        }
                                                    }

                                                }

                                            }
                                        }
                                    }
                                }
                            }

                            if (request.IsPrice)
                            {
                                #region Get Contract Rates By Service
                                ProdContractGetRes prodContractGetRes = new ProdContractGetRes();
                                ProdContractGetReq prodContractGetReq = new ProdContractGetReq
                                {
                                    QRFID = result.QRFID,
                                    ProductIDList = lstProductList,
                                    AgentId = result.AgentInfo.AgentID
                                };

                                var rangelist = response.PositionPrice.Select(c => c.ProductRangeId).Distinct().ToList();
                                prodContractGetRes = _productRepository.GetContractRatesByProductID(prodContractGetReq, rangelist);
                                var prodid = "";

                                if (prodContractGetRes != null && prodContractGetRes.ProductContractInfo.Count > 0)
                                {
                                    for (int i = 0; i < response.PositionPrice.Count; i++)
                                    {
                                        prodid = lstPositions.Where(a => a.PositionId == response.PositionPrice[i].PositionId).Select(a => a.ProductID).FirstOrDefault();
                                        if (!string.IsNullOrEmpty(prodid))
                                        {
                                            var lstPCInfo = prodContractGetRes.ProductContractInfo.Where(a => a.SupplierId == response.PositionPrice[i].SupplierId && a.ProductId == prodid
                                              && a.ProductRangeId == response.PositionPrice[i].ProductRangeId
                                              && (a.FromDate <= response.PositionPrice[i].ContractPeriod && response.PositionPrice[i].ContractPeriod <= a.ToDate)).ToList();

                                            if (lstPCInfo != null && lstPCInfo.Count > 0)
                                            {
                                                foreach (var con in lstPCInfo)
                                                {
                                                    char[] dayPattern = con.DayComboPattern.ToCharArray();
                                                    int dayNo = (int)Convert.ToDateTime(response.PositionPrice[i].ContractPeriod).DayOfWeek;

                                                    if (dayNo == 0)
                                                        dayNo = 7;

                                                    if (dayPattern[dayNo - 1] == '1')
                                                    {
                                                        response.PositionPrice[i].ContractId = con.ContractId;
                                                        response.PositionPrice[i].ContractPrice = Convert.ToDouble(con.Price);
                                                        response.PositionPrice[i].BudgetPrice = Convert.ToDouble(con.Price);
                                                        response.PositionPrice[i].BuyCurrencyId = con.CurrencyId;
                                                        response.PositionPrice[i].BuyCurrency = con.Currency;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                #endregion

                                #region GetMarkupValue and Add in Contract price

                                bool IsSalesOfficeUser = _genericRepository.IsSalesOfficeUser(request.LoginUserId);

                                if (IsSalesOfficeUser == true)
                                {
                                    var UserCompany_Id = _MongoContext.mUsers.AsQueryable().Where(x => x.UserName.ToLower() == request.LoginUserId.ToLower().Trim()).Select(y => y.Company_Id).FirstOrDefault();
                                    var Markup_Id = _MongoContext.mCompanies.AsQueryable().Where(x => x.Company_Id == UserCompany_Id && x.Markups.Any(y => y.Markup_For == "Groups")).FirstOrDefault().Markups.FirstOrDefault().Markup_Id;

                                    if (!string.IsNullOrEmpty(Markup_Id))
                                    {
                                        for (int i = 0; i < response.PositionPrice.Count; i++)
                                        {
                                            if (!string.IsNullOrEmpty(response.PositionPrice[i].ContractId))
                                            {
                                                ProdMarkupsGetReq prodMarkupsGetReq = new ProdMarkupsGetReq();

                                                prodMarkupsGetReq.MarkupsId = Markup_Id;
                                                prodMarkupsGetReq.ProductType = lstPositions.Where(a => a.PositionId == response.PositionPrice[i].PositionId).Select(b => b.ProductType).FirstOrDefault();
                                                var MarkupDetails = _productRepository.GetProdMarkups(prodMarkupsGetReq).Result;

                                                if (MarkupDetails != null)
                                                {
                                                    double MarkupValue = Convert.ToDouble(MarkupDetails.PercMarkUp) <= 0 ? Convert.ToDouble(MarkupDetails.FixedMarkUp) : Convert.ToDouble(MarkupDetails.PercMarkUp);

                                                    if (MarkupDetails.MARKUPTYPE == "Fixed")
                                                    {
                                                        double markup = MarkupValue;
                                                        if (MarkupDetails.CURRENCY_ID != response.PositionPrice[i].BuyCurrencyId)
                                                        {
                                                            var rate = _genericRepository.getExchangeRate(MarkupDetails.CURRENCY_ID, response.PositionPrice[i].BuyCurrencyId, request.QRFID);
                                                            if (rate != null)
                                                                markup = MarkupValue * Convert.ToDouble(rate.Value);
                                                        }
                                                        if (markup > 0)
                                                            response.PositionPrice[i].BudgetPrice = response.PositionPrice[i].BudgetPrice + Math.Round(markup, 2);
                                                    }
                                                    else
                                                    {
                                                        response.PositionPrice[i].BudgetPrice = response.PositionPrice[i].BudgetPrice + (response.PositionPrice[i].BudgetPrice * MarkupValue / 100);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                #endregion

                                #region Save Currency into mPosition
                                var objPosition = new mPosition();
                                var PosPriceCur = new mPositionPrice();

                                var modelsPosCur = new WriteModel<mPosition>[lstPositions.ToList().Count];
                                var curposids = lstPositions.Where(a => a.IsDeleted == false).Select(a => a.PositionId).ToList();
                                List<mPosition> lstCurPosition = _MongoContext.mPosition.AsQueryable().Where(a => curposids.Contains(a.PositionId) && a.IsDeleted == false).ToList();

                                for (int p = 0; p < lstPositions.Count; p++)
                                {
                                    objPosition = new mPosition();
                                    objPosition = lstCurPosition.Where(a => a.PositionId == lstPositions[p].PositionId).FirstOrDefault();
                                    PosPriceCur = response.PositionPrice.Where(a => a.PositionId == lstPositions[p].PositionId).FirstOrDefault();
                                    objPosition.BuyCurrencyId = PosPriceCur.BuyCurrencyId;
                                    objPosition.BuyCurrency = PosPriceCur.BuyCurrency;
                                    objPosition.EditDate = DateTime.Now;
                                    modelsPosCur[p] = new ReplaceOneModel<mPosition>(new BsonDocument("PositionId", objPosition.PositionId), objPosition) { IsUpsert = true };
                                }
                                var BulkWriteRes = await _MongoContext.mPosition.BulkWriteAsync(modelsPosCur);
                                #endregion

                                #region Get Saved Data from mPositionPrices and update IsDeleted to True those are deleted services and also replace the ProdRangeName
                                var roomingnmdiff = new List<mPositionPrice>();

                                if (resultPosPrices != null && resultPosPrices.Count > 0)
                                {
                                    if (response != null && response.PositionPrice.Count > 0)
                                    {
                                        //var PosPrice = new List<mPositionPrice>();
                                        foreach (mPositionPrice PosPrice in response.PositionPrice)
                                        {
                                            var resultFOC = resultPosPrices.Where(a => a.PositionId == PosPrice.PositionId && a.QRFID == PosPrice.QRFID
                                                                                        && a.DepartureId == PosPrice.DepartureId && a.PaxSlabId == PosPrice.PaxSlabId
                                                                                        && a.RoomId == PosPrice.RoomId && a.ProductRangeId == PosPrice.ProductRangeId).FirstOrDefault();
                                            if (resultFOC != null)
                                            {
                                                PosPrice.PositionPriceId = resultFOC.PositionPriceId;
                                                PosPrice.BudgetPrice = resultFOC.BudgetPrice;
                                            }
                                        }

                                        //for (int i = 0; i < response.PositionPrice.Count; i++)
                                        //{
                                        //    var pospr = resultPosPrices.Where(a => a.QRFID == response.PositionPrice[i].QRFID && a.PositionId == response.PositionPrice[i].PositionId
                                        //     && a.DepartureId == response.PositionPrice[i].DepartureId && a.PaxSlabId == response.PositionPrice[i].PaxSlabId &&
                                        //     a.RoomId == response.PositionPrice[i].RoomId && a.ProductRangeId == response.PositionPrice[i].ProductRangeId).FirstOrDefault();
                                        //    if (pospr != null)
                                        //    {
                                        //        response.PositionPrice[i].PositionPriceId = pospr.PositionPriceId;
                                        //        response.PositionPrice[i].BudgetPrice = pospr.BudgetPrice;
                                        //    }
                                        //}

                                        var posprexist = response.PositionPrice.Where(c => !string.IsNullOrEmpty(c.PositionPriceId)).ToList();
                                        var roomingids = resultPosPrices.FindAll(a => !posprexist.Exists(b => b.PositionPriceId == a.PositionPriceId));
                                        roomingnmdiff = resultPosPrices.FindAll(a => !posprexist.Exists(b => b.ProductRange == a.ProductRange));
                                        roomingids.AddRange(roomingnmdiff);
                                        if (roomingids != null && roomingids.Count > 0)
                                        {
                                            //UpdateResult resultFlag;
                                            //foreach (var item in roomingids)
                                            //{
                                            //    resultFlag = await _MongoContext.mPositionPrice.UpdateOneAsync(Builders<mPositionPrice>.Filter.Eq("PositionPriceId", item.PositionPriceId),
                                            //        Builders<mPositionPrice>.Update.Set("IsDeleted", true).Set("EditDate", DateTime.Now).
                                            //        Set("EditUser", usernm));
                                            //}
                                            var posPrids = roomingids.Select(b => b.PositionPriceId).ToList();
                                            var filter = Builders<mPositionPrice>.Filter.Where(a => posPrids.Contains(a.PositionPriceId));
                                            var update = Builders<mPositionPrice>.Update.Set(a => a.IsDeleted, true)
                                                                                        .Set(a => a.EditUser, usernm)
                                                                                        .Set(a => a.EditDate, DateTime.Now);
                                            UpdateResult upResult = await _MongoContext.mPositionPrice.UpdateManyAsync(filter, update);
                                        }
                                    }

                                    response.PositionPrice = response.PositionPrice.OrderBy(a => a.ProductRange).ToList();
                                }
                                #endregion

                                #region Set PositionPrice
                                if (response != null && response.PositionPrice != null && response.PositionPrice.Count > 0)
                                {
                                    //the below code is for if from frontend existing Dropdown of Services is changed then deactive the service and after that insert it.dectivation code is above
                                    var prposlist = response.PositionPrice.Where(a => string.IsNullOrEmpty(a.PositionPriceId) || a.PositionPriceId == Guid.Empty.ToString()).ToList();
                                    if (roomingnmdiff != null && roomingnmdiff.Count > 0)
                                    {
                                        var posids = roomingnmdiff.Select(a => a.PositionPriceId).ToList();
                                        List<mPositionPrice> objPositionPrices = response.PositionPrice.Where(a => posids.Contains(a.PositionPriceId)).ToList();
                                        objPositionPrices.ForEach(a => { a.BudgetPrice = 0; a.PositionPriceId = ""; });
                                        prposlist.AddRange(objPositionPrices);
                                    }
                                    if (prposlist != null && prposlist.Count > 0)
                                    {
                                        var lstAddPricesInfo = new List<mPositionPrice>();
                                        foreach (var item in prposlist)
                                        {
                                            if (string.IsNullOrEmpty(item.PositionPriceId) || item.PositionPriceId == Guid.Empty.ToString())
                                            {
                                                objPricesInfo = new mPositionPrice();
                                                item.PositionPriceId = Guid.NewGuid().ToString();
                                                item.CreateUser = usernm;
                                                item.CreateDate = DateTime.Now;
                                                item.EditUser = "";
                                                item.EditDate = null;
                                                item.IsDeleted = false;
                                                objPricesInfo = item;
                                                lstAddPricesInfo.Add(objPricesInfo);
                                            }
                                        }

                                        await _MongoContext.mPositionPrice.InsertManyAsync(lstAddPricesInfo);
                                        response.ResponseStatus.Status = "Success";
                                        response.ResponseStatus.ErrorMessage = "Saved Successfully.";
                                    }

                                    var prposlistUpdate = response.PositionPrice.Where(a => !string.IsNullOrEmpty(a.PositionPriceId) && a.PositionPriceId != Guid.Empty.ToString()).ToList();

                                    if (prposlistUpdate != null && prposlistUpdate.Count > 0)
                                    {
                                        var PositionIdlist = prposlistUpdate.Select(a => a.PositionId).ToList();
                                        //var positionlist = _MongoContext.mPosition.AsQueryable().Where(a => PositionIdlist.Contains(a.PositionId)).ToList();

                                        var PositionPriceIdlist = prposlistUpdate.Select(a => a.PositionPriceId).ToList();
                                        var prposlistUpdatelist = _MongoContext.mPositionPrice.AsQueryable().Where(a => PositionPriceIdlist.Contains(a.PositionPriceId) && a.IsDeleted == false);

                                        mPositionPrice res = new mPositionPrice();
                                        var PosPrice = new List<mPositionPrice>();

                                        var models = new WriteModel<mPositionPrice>[prposlistUpdatelist.ToList().Count];
                                        int j = 0;
                                        foreach (var item in prposlistUpdatelist)
                                        {
                                            res = prposlistUpdate.Where(a => a.PositionPriceId == item.PositionPriceId).FirstOrDefault();

                                            item.EditDate = DateTime.Now;
                                            item.EditUser = usernm;
                                            item.ContractId = res.ContractId;
                                            item.ContractPrice = res.ContractPrice;
                                            item.BudgetPrice = res.BudgetPrice;
                                            item.Type = res.Type;
                                            item.BuyCurrencyId = res.BuyCurrencyId;
                                            item.BuyCurrency = res.BuyCurrency;
                                            item.Period = res.Period;
                                            //models[j] = new ReplaceOneModel<VGER_WAPI_CLASSES.mPositionPrice>(new BsonDocument("PositionPriceId", item.PositionPriceId), item) { IsUpsert = true }; 
                                            models[j] = new ReplaceOneModel<VGER_WAPI_CLASSES.mPositionPrice>(new BsonDocument("PositionPriceId", item.PositionPriceId), item);
                                            j = j + 1;
                                        }
                                        var lst = new List<WriteModel<mPositionPrice>>();
                                        int cnt = 100;
                                        int totalPosCnt = prposlistUpdatelist.Count();
                                        for (int i = 0; i < totalPosCnt; i = i + cnt)
                                        {
                                            if ((i + cnt) > totalPosCnt)
                                            {
                                                lst = models.ToList().GetRange(i, totalPosCnt - i);
                                            }
                                            else
                                            {
                                                lst = models.ToList().GetRange(i, cnt);
                                            }
                                            Thread t = new Thread(new ThreadStart(() => UpdateBulkPositionPrice(lst)));
                                            t.Start();
                                        }
                                    }
                                }
                                #endregion
                            }

                            if (request.IsFOC)
                            {
                                #region Get ContractId from mPositionPrice                       
                                var resultPosPrice = _MongoContext.mPositionPrice.AsQueryable().Where(a => a.QRFID == result.QRFID && positionids.Contains(a.PositionId) && a.IsDeleted == false).ToList();
                                if (resultPosPrice != null && resultPosPrice.Count > 0)
                                {
                                    if (response != null && response.PositionFOC.Count > 0)
                                    {
                                        for (int i = 0; i < response.PositionFOC.Count; i++)
                                        {
                                            var pospr = resultPosPrice.Where(a => a.PositionId == response.PositionFOC[i].PositionId && a.QRFID == response.PositionFOC[i].QRFID
                                            && a.DepartureId == response.PositionFOC[i].DepartureId && a.PaxSlabId == response.PositionFOC[i].PaxSlabId
                                            && a.RoomId == response.PositionFOC[i].RoomId).FirstOrDefault();

                                            if (pospr != null)
                                            {
                                                response.PositionFOC[i].ContractId = pospr.ContractId;
                                            }
                                        }
                                    }
                                }
                                #endregion

                                #region Get FOC Quantity mProductFreePlacePolicy
                                var resultProdFPP = _MongoContext.mProductFreePlacePolicy.AsQueryable().Where(a => prodids.Contains(a.Product_Id)).ToList();
                                if (resultProdFPP != null && resultProdFPP.Count > 0)
                                {
                                    if (response != null && response.PositionFOC.Count > 0)
                                    {
                                        for (int i = 0; i < response.PositionFOC.Count; i++)
                                        {
                                            var posFPP = resultProdFPP.Where(a => a.Product_Id == response.PositionFOC[i].ProductId).ToList();
                                            if (posFPP != null && posFPP.Count > 0)
                                            {
                                                for (int j = 0; j < posFPP.Count; j++)
                                                {
                                                    string[] paxSlab = response.PositionFOC[i].PaxSlab.Split(" - ");
                                                    if (
                                                        response.PositionFOC[i].ContractId == posFPP[j].ProductContract_Id &&
                                                        Convert.ToString(response.PositionFOC[i].ProductRange.Replace("(" + response.PositionFOC[i].Type.ToUpper() + ")", "").ToLower()).Trim() == Convert.ToString(posFPP[j].Subprod.ToLower()).Trim() &&
                                                        (posFPP[j].DateMin <= response.PositionFOC[i].Period && response.PositionFOC[i].Period <= posFPP[j].DateMax) &&
                                                        (Convert.ToInt16(paxSlab[0]) <= posFPP[j].MinPers && posFPP[j].MinPers <= Convert.ToInt16(paxSlab[0]))
                                                        )
                                                    {
                                                        response.PositionFOC[i].FOCQty = posFPP[j].Quantity;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                #endregion

                                #region Get Saved Data from mPositionFOC and update IsDeleted to True for those are deleted services
                                var roomingnmdiff = new List<mPositionFOC>();
                                var resultPosFOC = _MongoContext.mPositionFOC.AsQueryable().Where(a => a.QRFID == result.QRFID && positionids.Contains(a.PositionId) && a.IsDeleted == false).ToList();
                                if (resultPosFOC != null && resultPosFOC.Count > 0)
                                {
                                    if (response != null && response.PositionFOC.Count > 0)
                                    {
                                        foreach (mPositionFOC posFOC in response.PositionFOC)
                                        {
                                            var resultFOC = resultPosFOC.Where(a => a.PositionId == posFOC.PositionId && a.QRFID == posFOC.QRFID
                                                                                        && a.DepartureId == posFOC.DepartureId && a.PaxSlabId == posFOC.PaxSlabId
                                                                                        && a.RoomId == posFOC.RoomId && a.ProductRangeId == posFOC.ProductRangeId).FirstOrDefault();
                                            if (resultFOC != null)
                                            {
                                                posFOC.PositionFOCId = resultFOC.PositionFOCId;
                                                posFOC.FOCQty = resultFOC.FOCQty;
                                            }
                                        }
                                        /*for (int i = 0; i < response.PositionFOC.Count; i++)
                                        {
                                            var resultFOC = resultPosFOC.Where(a => a.PositionId == response.PositionFOC[i].PositionId && a.QRFID == response.PositionFOC[i].QRFID
                                            && a.DepartureId == response.PositionFOC[i].DepartureId && a.PaxSlabId == response.PositionFOC[i].PaxSlabId
                                            && a.RoomId == response.PositionFOC[i].RoomId && a.ProductRangeId == response.PositionPrice[i].ProductRangeId).FirstOrDefault();

                                            if (resultFOC != null)
                                            {
                                                response.PositionFOC[i].PositionFOCId = resultFOC.PositionFOCId;
                                                response.PositionFOC[i].FOCQty = resultFOC.FOCQty;
                                            }
                                        }*/
                                        var posprexist = response.PositionFOC.Where(c => !string.IsNullOrEmpty(c.PositionFOCId)).ToList();
                                        var roomingids = resultPosFOC.FindAll(a => !posprexist.Exists(b => b.PositionFOCId == a.PositionFOCId));
                                        roomingnmdiff = resultPosFOC.FindAll(a => !posprexist.Exists(b => b.ProductRange == a.ProductRange));
                                        roomingids.AddRange(roomingnmdiff);

                                        if (roomingids != null && roomingids.Count > 0)
                                        {
                                            var posFOCids = roomingids.Select(b => b.PositionFOCId).ToList();
                                            var filter = Builders<mPositionFOC>.Filter.Where(a => posFOCids.Contains(a.PositionFOCId));
                                            var update = Builders<mPositionFOC>.Update.Set(a => a.IsDeleted, true)
                                                                                        .Set(a => a.EditUser, usernm)
                                                                                        .Set(a => a.EditDate, DateTime.Now);
                                            UpdateResult upResult = await _MongoContext.mPositionFOC.UpdateManyAsync(filter, update);

                                            //UpdateResult resultFlag;
                                            //foreach (var item in roomingids)
                                            //{
                                            //    resultFlag = await _MongoContext.mPositionFOC.UpdateOneAsync(Builders<mPositionFOC>.Filter.Eq("PositionFOCId", item.PositionFOCId),
                                            //        Builders<mPositionFOC>.Update.Set("IsDeleted", true).Set("EditDate", DateTime.Now).
                                            //        Set("EditUser", usernm));
                                            //}
                                        }
                                    }
                                }
                                #endregion

                                #region On Position Save button set All FOC
                                if (response.PositionFOC != null && response.PositionFOC.Count > 0)
                                {
                                    var resfoc = response.PositionFOC.Where(a => string.IsNullOrEmpty(a.PositionFOCId) || a.PositionFOCId == Guid.Empty.ToString()).ToList();
                                    if (roomingnmdiff != null && roomingnmdiff.Count > 0)
                                    {
                                        var posids = roomingnmdiff.Select(a => a.PositionFOCId).ToList();
                                        List<mPositionFOC> objmPositionFOC = response.PositionFOC.Where(a => posids.Contains(a.PositionFOCId)).ToList();
                                        objmPositionFOC.ForEach(a => { a.FOCQty = 0; a.PositionFOCId = ""; });
                                        resfoc.AddRange(objmPositionFOC);
                                    }

                                    if (resfoc != null && resfoc.Count > 0)
                                    {
                                        var lstFOCInfo = new List<mPositionFOC>();
                                        foreach (var item in resfoc)
                                        {
                                            if (string.IsNullOrEmpty(item.PositionFOCId) || item.PositionFOCId == Guid.Empty.ToString())
                                            {
                                                objFOCInfo = new mPositionFOC();
                                                item.PositionFOCId = Guid.NewGuid().ToString();
                                                item.CreateUser = usernm;
                                                item.CreateDate = DateTime.Now;
                                                item.EditUser = "";
                                                item.EditDate = null;
                                                item.IsDeleted = false;
                                                objFOCInfo = item;
                                                lstFOCInfo.Add(objFOCInfo);
                                            }
                                        }
                                        await _MongoContext.mPositionFOC.InsertManyAsync(lstFOCInfo);
                                        response.ResponseStatus.Status = "Success";
                                        response.ResponseStatus.ErrorMessage = "Saved Successfully.";
                                    }
                                    else
                                    {
                                        response.ResponseStatus.Status = "Success";
                                        response.ResponseStatus.ErrorMessage = "Saved Successfully.";
                                    }
                                }
                                #endregion
                            }
                        }
                    }
                    else
                    {
                        response.ResponseStatus.ErrorMessage = "QRF ID not exist.";
                        response.ResponseStatus.Status = "Failure";
                    }
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }

            return response;
        }

        public void UpdateBulkPositionPrice(List<WriteModel<mPositionPrice>> lst)
        {
            _MongoContext.mPositionPrice.BulkWrite(lst);
        }

        public void UpdateBulkQRFPositionPrice(List<WriteModel<mPositionPriceQRF>> lst)
        {
            _MongoContext.mPositionPriceQRF.BulkWrite(lst);
        }

        public async Task<bool> SetAllPriceFOCByQRFID(string QRFID, string UserName)
        {
            List<string> RangeIdList = new List<string>();
            List<string> prodcatIdList = new List<string>();
            var resultposition = _MongoContext.mPosition.AsQueryable().Where(a => a.QRFID == QRFID && a.ProductType == "Hotel" && a.IsTourEntity == false && a.IsDeleted == false).ToList();
            if (resultposition?.Count > 0)
            {
                resultposition.ForEach(a =>
                {
                    var rooms = a.RoomDetailsInfo.Where(b => b.IsDeleted == false).ToList();
                    RangeIdList.AddRange(rooms.Select(b => b.ProductRangeId).ToList());
                    prodcatIdList.AddRange(rooms.Select(b => b.ProductCategoryId).ToList());
                });
                //var ProdRangeList = _MongoContext.mProductRange.AsQueryable().Where(a => RangeIdList.Contains(a.VoyagerProductRange_Id))
                //           .Select(a => new ProductRangeInfo
                //           {
                //               VoyagerProductRange_Id = a.VoyagerProductRange_Id,
                //               ProductRangeCode = a.ProductRangeCode,
                //               ProductType_Id = a.ProductType_Id,
                //               PersonType = a.PersonType,
                //               ProductMenu = a.ProductMenu
                //           }).ToList(); 
                var lstProductCategories = _MongoContext.Products.AsQueryable().Where(a => a.ProductCategories.Any(b => prodcatIdList.Contains(b.ProductCategory_Id)))
                                  .SelectMany(a => a.ProductCategories).ToList();
                var ProdRangeList = lstProductCategories.SelectMany(a => a.ProductRanges).Where(a => RangeIdList.Contains(a.ProductRange_Id)).Select(a => new ProductRangeInfo
                {
                    VoyagerProductRange_Id = a.ProductRange_Id,
                    ProductRangeCode = a.ProductTemplateCode,
                    ProductType_Id = a.PersonType_Id,
                    PersonType = a.PersonType,
                    ProductMenu = a.ProductMenu
                }).ToList();

                PositionPriceFOCSetRes res = await SetAllPositionPriceFOC(new PositionPriceFOCSetReq
                {
                    PositionId = "",
                    QRFID = QRFID,
                    IsFOC = true,
                    IsPrice = true,
                    ProductRangeInfo = ProdRangeList,
                    ProductTypeList = new List<string>() { "Hotel" },
                    IsClone = false,
                    LoginUserId = UserName
                });
            }
            return true;
        }
        #endregion

        #region Prices
        public async Task<PositionPriceGetRes> GetPositionPrice(PositionPriceGetReq request)
        {
            PositionPriceGetRes response = new PositionPriceGetRes();

            response.ResponseStatus = new ResponseStatus();
            response.PositionPrice = new List<mPositionPrice>();

            try
            {
                bool IsSalesOfficeUser = _genericRepository.IsSalesOfficeUser(request.LoginUser);
                response.IsSalesOfficeUser = IsSalesOfficeUser;
                if (request.IsClone)
                {
                    var builder = Builders<mPositionPriceQRF>.Filter;
                    var respos = _MongoContext.mQRFPosition.AsQueryable().Where(a => a.PositionId == request.PositionId && a.IsDeleted == false).FirstOrDefault();
                    if (respos != null)
                    {
                        response.StandardPrice = respos.StandardPrice;
                        var filter = builder.Where(q => q.PositionId == request.PositionId && q.IsDeleted == false);
                        List<mPositionPriceQRF> result = await _MongoContext.mPositionPriceQRF.Find(filter).ToListAsync();
                        if (result != null && result.Count > 0)
                        {
                            response.ResponseStatus.Status = "Success";
                            if (respos.ProductType.ToLower() == "hotel" || respos.ProductType.ToLower() == "overnight ferry")
                            {
                                var roomsuppliment = new List<mPositionPriceQRF>();
                                var roomservice = new List<mPositionPriceQRF>();

                                roomservice = result.Where(a => a.IsSupplement == false).ToList().
                                     OrderBy(a => a.ProductRange.Contains("SINGLE") ? "A" : a.ProductRange.Contains("DOUBLE") ? "B" : a.ProductRange.Contains("TWIN") ? "C" : a.ProductRange.Contains("TRIPLE") ? "D" :
                                                        a.ProductRange.Contains("QUAD") ? "E" : a.ProductRange.Contains("TSU") ? "F" :
                                                        a.ProductRange.ToLower().Contains("child + bed") ? "G" : a.ProductRange.ToLower().Contains("child - bed") ? "H" :
                                                        a.ProductRange.ToLower().Contains("infant") ? "I" : "J").ThenBy(a => a.ProductRange).ToList();

                                roomsuppliment = result.Where(a => a.IsSupplement == true).OrderBy(a => a.ProductRange).ToList();
                                roomservice.AddRange(roomsuppliment);

                                response.PositionPrice = ConvertQRFPosPriceToPosPrice(roomservice);
                            }
                            else
                            {
                                response.PositionPrice = ConvertQRFPosPriceToPosPrice(result.OrderBy(a => a.IsSupplement).ThenBy(a => a.ProductRange).ToList());
                            }
                        }
                        else
                        {
                            response.ResponseStatus.Status = "Error";
                            response.ResponseStatus.ErrorMessage = "Position Id not found.";
                        }
                    }
                }
                else
                {
                    var builder = Builders<mPositionPrice>.Filter;
                    var respos = _MongoContext.mPosition.AsQueryable().Where(a => a.PositionId == request.PositionId && a.IsDeleted == false).FirstOrDefault();
                    if (respos != null)
                    {
                        response.StandardPrice = respos.StandardPrice;
                        var filter = builder.Where(q => q.PositionId == request.PositionId && q.IsDeleted == false);
                        List<mPositionPrice> result = await _MongoContext.mPositionPrice.Find(filter).ToListAsync();
                        if (result != null && result.Count > 0)
                        {
                            response.ResponseStatus.Status = "Success";
                            if (respos.ProductType.ToLower() == "hotel" || respos.ProductType.ToLower() == "overnight ferry")
                            {
                                var roomsuppliment = new List<mPositionPrice>();
                                var roomservice = new List<mPositionPrice>();

                                roomservice = result.Where(a => a.IsSupplement == false).ToList().
                                     OrderBy(a => a.ProductRange.Contains("SINGLE") ? "A" : a.ProductRange.Contains("DOUBLE") ? "B" : a.ProductRange.Contains("TWIN") ? "C" : a.ProductRange.Contains("TRIPLE") ? "D" :
                                                        a.ProductRange.Contains("QUAD") ? "E" : a.ProductRange.Contains("TSU") ? "F" :
                                                        a.ProductRange.ToLower().Contains("child + bed") ? "G" : a.ProductRange.ToLower().Contains("child - bed") ? "H" :
                                                        a.ProductRange.ToLower().Contains("infant") ? "I" : "J").ThenBy(a => a.ProductRange).ToList();

                                roomsuppliment = result.Where(a => a.IsSupplement == true).OrderBy(a => a.ProductRange).ToList();
                                roomservice.AddRange(roomsuppliment);

                                response.PositionPrice = roomservice;
                            }
                            else
                            {
                                response.PositionPrice = result.OrderBy(a => a.IsSupplement).ThenBy(a => a.ProductRange).ToList();
                            }
                        }
                        else
                        {
                            response.ResponseStatus.Status = "Error";
                            response.ResponseStatus.ErrorMessage = "Position Id not found.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// this will work for Single Position Price Save button
        /// </summary>
        /// <param name="request">PositionPriceSetReq</param>
        /// <returns></returns>
        public async Task<PositionPriceSetRes> SetPositionPrice(PositionPriceSetReq request)
        {
            PositionPriceSetRes response = new PositionPriceSetRes();
            try
            {
                if (request.IsClone == true)
                {
                    List<mPositionPriceQRF> PositionPriceQRF = new List<mPositionPriceQRF>();
                    PositionPriceQRF = ConvertPosPriceToQRFPosPrice(request.PositionPrice);

                    mPositionPriceQRF objPositionPrices;

                    if (request.PositionPrice != null && request.PositionPrice.Count > 0)
                    {
                        var resultFlag = await _MongoContext.mQRFPosition.UpdateOneAsync(Builders<mQRFPosition>.Filter.Eq("PositionId", request.PositionPrice[0].PositionId),
                                            Builders<mQRFPosition>.Update.Set("StandardPrice", request.StandardPrice).Set("EditDate", DateTime.Now).Set("EditUser", request.PositionPrice[0].EditUser));

                        var position = _MongoContext.mQRFPosition.AsQueryable().Where(a => a.PositionId == request.PositionPrice[0].PositionId && a.IsDeleted == false).FirstOrDefault();
                        if (position != null)
                        {
                            response.PositionId = position.PositionId;
                            response.ProductId = position.ProductID;
                            response.PositionName = position.ProductType + "(" + position.CityName + "," + position.StartingFrom + "," + position.Duration.ToString() + "D" + ")";
                        }
                    }
                    foreach (var item in PositionPriceQRF)
                    {
                        if (string.IsNullOrEmpty(item.PositionPriceId) || item.PositionPriceId == Guid.Empty.ToString())
                        {
                            objPositionPrices = new mPositionPriceQRF();
                            item.PositionPriceId = Guid.NewGuid().ToString();
                            item.CreateDate = DateTime.Now;
                            item.EditUser = "";
                            item.EditDate = null;
                            item.IsDeleted = false;
                            objPositionPrices = item;
                            await _MongoContext.mPositionPriceQRF.InsertOneAsync(objPositionPrices);
                            response.ResponseStatus.Status = "Success";
                            response.ResponseStatus.ErrorMessage = "Saved Successfully.";
                        }
                        else
                        {
                            objPositionPrices = _MongoContext.mPositionPriceQRF.AsQueryable().Where(a => a.PositionPriceId == item.PositionPriceId).FirstOrDefault();
                            if (objPositionPrices != null)
                            {
                                objPositionPrices.EditDate = DateTime.Now;
                                objPositionPrices.EditUser = item.EditUser;
                                objPositionPrices.BudgetPrice = item.BudgetPrice;

                                ReplaceOneResult replaceResult = await _MongoContext.mPositionPriceQRF.ReplaceOneAsync(Builders<mPositionPriceQRF>.Filter.Eq("PositionPriceId", item.PositionPriceId), objPositionPrices);
                                response.ResponseStatus.Status = replaceResult.MatchedCount > 0 ? "Success" : "Failure";
                                response.ResponseStatus.ErrorMessage = replaceResult.MatchedCount > 0 ? "Saved Successfully." : "Details not updated.";
                            }
                        }
                    }
                }
                else
                {
                    mPositionPrice objPositionPrices;

                    if (request.PositionPrice != null && request.PositionPrice.Count > 0)
                    {
                        var resultFlag = await _MongoContext.mPosition.UpdateOneAsync(Builders<mPosition>.Filter.Eq("PositionId", request.PositionPrice[0].PositionId),
                                            Builders<mPosition>.Update.Set("StandardPrice", request.StandardPrice).Set("EditDate", DateTime.Now).Set("EditUser", request.PositionPrice[0].EditUser));

                        var position = _MongoContext.mPosition.AsQueryable().Where(a => a.PositionId == request.PositionPrice[0].PositionId && a.IsDeleted == false).FirstOrDefault();
                        if (position != null)
                        {
                            response.PositionId = position.PositionId;
                            response.ProductId = position.ProductID;
                            response.PositionName = position.ProductType + "(" + position.CityName + "," + position.StartingFrom + "," + position.Duration.ToString() + "D" + ")";
                        }
                    }
                    foreach (var item in request.PositionPrice)
                    {
                        if (string.IsNullOrEmpty(item.PositionPriceId) || item.PositionPriceId == Guid.Empty.ToString())
                        {
                            objPositionPrices = new mPositionPrice();
                            item.PositionPriceId = Guid.NewGuid().ToString();
                            item.CreateDate = DateTime.Now;
                            item.EditUser = "";
                            item.EditDate = null;
                            item.IsDeleted = false;
                            objPositionPrices = item;
                            await _MongoContext.mPositionPrice.InsertOneAsync(objPositionPrices);
                            response.ResponseStatus.Status = "Success";
                            response.ResponseStatus.ErrorMessage = "Saved Successfully.";
                        }
                        else
                        {
                            objPositionPrices = _MongoContext.mPositionPrice.AsQueryable().Where(a => a.PositionPriceId == item.PositionPriceId).FirstOrDefault();
                            if (objPositionPrices != null)
                            {
                                objPositionPrices.EditDate = DateTime.Now;
                                objPositionPrices.EditUser = item.EditUser;
                                objPositionPrices.BudgetPrice = item.BudgetPrice;

                                ReplaceOneResult replaceResult = await _MongoContext.mPositionPrice.ReplaceOneAsync(Builders<mPositionPrice>.Filter.Eq("PositionPriceId", item.PositionPriceId), objPositionPrices);
                                response.ResponseStatus.Status = replaceResult.MatchedCount > 0 ? "Success" : "Failure";
                                response.ResponseStatus.ErrorMessage = replaceResult.MatchedCount > 0 ? "Saved Successfully." : "Details not updated.";
                            }
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
        #endregion

        #region FOC
        //public async Task<PositionFOCGetRes> GetPositionFOC(PositionFOCGetReq request)
        //{
        //    PositionFOCGetRes response = new PositionFOCGetRes();
        //    String supplierId = "";
        //    mPositionFOC objFOCInfo = new mPositionFOC();

        //    var builder = Builders<mQuote>.Filter;
        //    var filter = builder.Where(q => q.QRFID == request.QRFId);
        //    mQuote result = await _MongoContext.mQuote.Find(filter).Project(q => new mQuote
        //    {
        //        Departures = q.Departures,
        //        PaxSlabDetails = q.PaxSlabDetails,
        //        AgentPassengerInfo = q.AgentPassengerInfo
        //    }).FirstOrDefaultAsync();

        //    var positionData = _MongoContext.mPosition.AsQueryable().Where(a => a.PositionId == request.PositionId).FirstOrDefault();
        //    if (positionData != null)
        //    {
        //        response.StandardFOC = positionData.StandardFOC;

        //        supplierId = positionData.SupplierId;
        //        var procatlist = positionData.RoomDetailsInfo.Select(p => p.ProductCategoryId).ToList();
        //        List<mProductRange> prodrange = new List<mProductRange>();
        //        prodrange = _MongoContext.mProductRange.AsQueryable().Where(a => a.Product_Id == positionData.ProductID && procatlist.Contains(a.ProductCategory_Id)).ToList();

        //        var currencyId = _MongoContext.mProductSupplier.AsQueryable().Where(s => s.Company_Id == positionData.SupplierId).Select(a => a.Currency_Id).FirstOrDefault();
        //        var currency = _MongoContext.mCurrency.AsQueryable().Where(c => c.VoyagerCurrency_Id == currencyId).Select(a => a.Currency).FirstOrDefault();

        //        for (int i = 0; i < result.Departures.Count; i++)
        //        {
        //            for (int j = 0; j < result.PaxSlabDetails.PaxSlabs.Count; j++)
        //            {
        //                for (int k = 0; k < positionData.RoomDetailsInfo.Count; k++)
        //                {
        //                    if ((!(positionData.RoomDetailsInfo[k].IsDeleted)) && (!(result.Departures[i].IsDeleted)) && (!(result.PaxSlabDetails.PaxSlabs[j].IsDeleted)))
        //                    {
        //                        objFOCInfo = new mPositionFOC
        //                        {
        //                            QRFID = request.QRFId,
        //                            PositionId = request.PositionId,
        //                            DepartureId = result.Departures[i].Departure_Id,
        //                            Period = result.Departures[i].Date,
        //                            PaxSlabId = result.PaxSlabDetails.PaxSlabs[j].PaxSlab_Id,
        //                            PaxSlab = result.PaxSlabDetails.PaxSlabs[j].From.ToString() + " - " + result.PaxSlabDetails.PaxSlabs[j].To.ToString(),
        //                            CityId = positionData.CityID,
        //                            CityName = positionData.CityName,
        //                            ProductId = positionData.ProductID,
        //                            ProductName = positionData.ProductName,
        //                            SupplierId = positionData.SupplierId,
        //                            Supplier = positionData.SupplierName,
        //                            RoomId = positionData.RoomDetailsInfo[k].RoomId,
        //                            IsSupplement = positionData.RoomDetailsInfo[k].IsSupplement,
        //                            ProductCategoryId = positionData.RoomDetailsInfo[k].ProductCategoryId,
        //                            ProductRangeId = positionData.RoomDetailsInfo[k].ProductRangeId,
        //                            ProductCategory = positionData.RoomDetailsInfo[k].ProductCategory,
        //                            ProductRange = positionData.RoomDetailsInfo[k].ProductRange,
        //                            Quantity = result.PaxSlabDetails.PaxSlabs[j].From,
        //                        };
        //                        objFOCInfo.Type = prodrange.Where(a => a.VoyagerProductRange_Id == objFOCInfo.ProductRangeId).Select(b => b.PersonType).FirstOrDefault();
        //                        response.PositionFOC.Add(objFOCInfo);
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    #region Get ContractId from mPositionPrice
        //    var resultPosPrice = _MongoContext.mPositionPrice.AsQueryable().Where(a => a.PositionId == request.PositionId).ToList();
        //    if (resultPosPrice != null && resultPosPrice.Count > 0)
        //    {
        //        if (response != null && response.PositionFOC.Count > 0)
        //        {
        //            for (int i = 0; i < response.PositionFOC.Count; i++)
        //            {
        //                for (int j = 0; j < resultPosPrice.Count; j++)
        //                {
        //                    if (response.PositionFOC[i].QRFID == resultPosPrice[j].QRFID &&
        //                        response.PositionFOC[i].PositionId == resultPosPrice[j].PositionId &&
        //                        response.PositionFOC[i].DepartureId == resultPosPrice[j].DepartureId &&
        //                        response.PositionFOC[i].PaxSlabId == resultPosPrice[j].PaxSlabId &&
        //                        response.PositionFOC[i].RoomId == resultPosPrice[j].RoomId)
        //                    {
        //                        response.PositionFOC[i].ContractId = resultPosPrice[j].ContractId;
        //                        break;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    #endregion

        //    #region Get FOC Quantity mProductFreePlacePolicy
        //    var resultProdFPP = _MongoContext.mProductFreePlacePolicy.AsQueryable().Where(a => a.Product_Id == request.ProductID).ToList();
        //    if (resultProdFPP != null && resultProdFPP.Count > 0)
        //    {
        //        if (response != null && response.PositionFOC.Count > 0)
        //        {
        //            for (int i = 0; i < response.PositionFOC.Count; i++)
        //            {
        //                for (int j = 0; j < resultProdFPP.Count; j++)
        //                {
        //                    string[] paxSlab = response.PositionFOC[i].PaxSlab.Split(" - ");
        //                    if (
        //                        response.PositionFOC[i].ContractId == resultProdFPP[j].ProductContract_Id &&
        //                        Convert.ToString(response.PositionFOC[i].ProductRange.ToLower()).Trim() == Convert.ToString(resultProdFPP[j].Subprod.ToLower()).Trim() &&
        //                        (resultProdFPP[j].DateMin <= response.PositionFOC[i].Period && response.PositionFOC[i].Period <= resultProdFPP[j].DateMax) &&
        //                        (Convert.ToInt16(paxSlab[0]) <= resultProdFPP[j].MinPers && resultProdFPP[j].MinPers <= Convert.ToInt16(paxSlab[0]))
        //                        )
        //                    {
        //                        response.PositionFOC[i].FOCQty = resultProdFPP[j].Quantity;
        //                        break;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    #endregion

        //    #region Get Saved Data from mPositionFOC
        //    var resultPosFOC = _MongoContext.mPositionFOC.AsQueryable().Where(a => a.PositionId == request.PositionId).ToList();
        //    if (resultPosFOC != null && resultPosFOC.Count > 0)
        //    {
        //        if (response != null && response.PositionFOC.Count > 0)
        //        {
        //            for (int i = 0; i < response.PositionFOC.Count; i++)
        //            {
        //                for (int j = 0; j < resultPosFOC.Count; j++)
        //                {
        //                    if (response.PositionFOC[i].QRFID == resultPosFOC[j].QRFID &&
        //                        response.PositionFOC[i].PositionId == resultPosFOC[j].PositionId &&
        //                        response.PositionFOC[i].DepartureId == resultPosFOC[j].DepartureId &&
        //                        response.PositionFOC[i].PaxSlabId == resultPosFOC[j].PaxSlabId &&
        //                        response.PositionFOC[i].RoomId == resultPosFOC[j].RoomId)
        //                    {
        //                        response.PositionFOC[i].PositionFOCId = resultPosFOC[j].PositionFOCId;
        //                        response.PositionFOC[i].FOCQty = resultPosFOC[j].FOCQty;
        //                        break;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    #endregion

        //    return response;
        //}

        public async Task<PositionFOCGetRes> GetPositionFOC(PositionFOCGetReq request)
        {
            PositionFOCGetRes response = new PositionFOCGetRes();

            response.ResponseStatus = new ResponseStatus();
            response.PositionFOC = new List<mPositionFOC>();

            try
            {
                if (request.IsClone)
                {
                    var builder = Builders<mQRFPositionFOC>.Filter;
                    var respos = _MongoContext.mQRFPosition.AsQueryable().Where(a => a.PositionId == request.PositionId && a.IsDeleted == false).FirstOrDefault();
                    if (respos != null)
                    {
                        response.StandardFOC = respos.StandardFOC;
                        var filter = builder.Where(q => q.PositionId == request.PositionId && q.IsDeleted == false);
                        List<mQRFPositionFOC> result = await _MongoContext.mQRFPositionFOC.Find(filter).ToListAsync();
                        if (result != null && result.Count > 0)
                        {
                            response.ResponseStatus.Status = "Success";
                            if (respos.ProductType.ToLower() == "hotel" || respos.ProductType.ToLower() == "overnight ferry")
                            {
                                var roomsuppliment = new List<mQRFPositionFOC>();
                                var roomservice = new List<mQRFPositionFOC>();

                                roomservice = result.Where(a => a.IsSupplement == false).ToList().
                                     OrderBy(a => a.ProductRange.Contains("SINGLE") ? "A" : a.ProductRange.Contains("DOUBLE") ? "B" : a.ProductRange.Contains("TWIN") ? "C" : a.ProductRange.Contains("TRIPLE") ? "D" :
                                                        a.ProductRange.Contains("QUAD") ? "E" : a.ProductRange.Contains("TSU") ? "F" :
                                                        a.ProductRange.ToLower().Contains("child + bed") ? "G" : a.ProductRange.ToLower().Contains("child - bed") ? "H" :
                                                        a.ProductRange.ToLower().Contains("infant") ? "I" : "J").ThenBy(a => a.ProductRange).ToList();

                                roomsuppliment = result.Where(a => a.IsSupplement == true).OrderBy(a => a.ProductRange).ToList();
                                roomservice.AddRange(roomsuppliment);

                                response.PositionFOC = ConvertQRFPosFOCToPosFOC(roomservice);
                            }
                            else
                            {
                                response.PositionFOC = ConvertQRFPosFOCToPosFOC(result.OrderBy(a => a.IsSupplement).ThenBy(a => a.ProductRange).ToList());
                            }
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Error";
                        response.ResponseStatus.ErrorMessage = "Position Id not found.";
                    }
                }
                else
                {
                    var builder = Builders<mPositionFOC>.Filter;
                    var respos = _MongoContext.mPosition.AsQueryable().Where(a => a.PositionId == request.PositionId && a.IsDeleted == false).FirstOrDefault();
                    if (respos != null)
                    {
                        response.StandardFOC = respos.StandardFOC;
                        var filter = builder.Where(q => q.PositionId == request.PositionId && q.IsDeleted == false);
                        List<mPositionFOC> result = await _MongoContext.mPositionFOC.Find(filter).ToListAsync();
                        if (result != null && result.Count > 0)
                        {
                            response.ResponseStatus.Status = "Success";
                            if (respos.ProductType.ToLower() == "hotel" || respos.ProductType.ToLower() == "overnight ferry")
                            {
                                var roomsuppliment = new List<mPositionFOC>();
                                var roomservice = new List<mPositionFOC>();

                                roomservice = result.Where(a => a.IsSupplement == false).ToList().
                                     OrderBy(a => a.ProductRange.Contains("SINGLE") ? "A" : a.ProductRange.Contains("DOUBLE") ? "B" : a.ProductRange.Contains("TWIN") ? "C" : a.ProductRange.Contains("TRIPLE") ? "D" :
                                                        a.ProductRange.Contains("QUAD") ? "E" : a.ProductRange.Contains("TSU") ? "F" :
                                                        a.ProductRange.ToLower().Contains("child + bed") ? "G" : a.ProductRange.ToLower().Contains("child - bed") ? "H" :
                                                        a.ProductRange.ToLower().Contains("infant") ? "I" : "J").ThenBy(a => a.ProductRange).ToList();

                                roomsuppliment = result.Where(a => a.IsSupplement == true).OrderBy(a => a.ProductRange).ToList();
                                roomservice.AddRange(roomsuppliment);

                                response.PositionFOC = roomservice;
                            }
                            else
                            {
                                response.PositionFOC = result.OrderBy(a => a.IsSupplement).ThenBy(a => a.ProductRange).ToList();
                            }
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Error";
                        response.ResponseStatus.ErrorMessage = "Position Id not found.";
                    }
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// this will work for Single Position FOC Save button
        /// </summary>
        /// <param name="request">PositionFOCSetReq</param>
        /// <returns></returns>
        public async Task<PositionFOCSetRes> SetPositionFOC(PositionFOCSetReq request)
        {
            PositionFOCSetRes response = new PositionFOCSetRes();
            try
            {
                if (request.IsClone)
                {
                    List<mQRFPositionFOC> QRFPositionFOC = new List<mQRFPositionFOC>();
                    QRFPositionFOC = ConvertPosFOCToQRFPosFOC(request.PositionFOC);


                    mQRFPositionFOC objPositionFOC;

                    if (request.PositionFOC.Count > 0)
                    {
                        var resultFlag = await _MongoContext.mQRFPosition.UpdateOneAsync(Builders<mQRFPosition>.Filter.Eq("PositionId", request.PositionFOC[0].PositionId),
                                            Builders<mQRFPosition>.Update.Set("StandardFOC", request.StandardFOC).Set("EditDate", DateTime.Now).Set("EditUser", request.PositionFOC[0].EditUser));
                    }
                    foreach (var item in QRFPositionFOC)
                    {
                        if (string.IsNullOrEmpty(item.PositionFOCId) || item.PositionFOCId == Guid.Empty.ToString())
                        {
                            objPositionFOC = new mQRFPositionFOC();
                            item.PositionFOCId = Guid.NewGuid().ToString();
                            item.CreateDate = DateTime.Now;
                            item.EditUser = "";
                            item.EditDate = null;
                            item.IsDeleted = false;
                            objPositionFOC = item;
                            await _MongoContext.mQRFPositionFOC.InsertOneAsync(objPositionFOC);
                            response.ResponseStatus.Status = "Success";
                            response.ResponseStatus.ErrorMessage = "Saved Successfully.";
                        }
                        else
                        {
                            objPositionFOC = _MongoContext.mQRFPositionFOC.AsQueryable().Where(a => a.PositionFOCId == item.PositionFOCId).FirstOrDefault();
                            objPositionFOC.EditDate = DateTime.Now;
                            objPositionFOC.EditUser = item.EditUser;
                            objPositionFOC.FOCQty = item.FOCQty;

                            ReplaceOneResult replaceResult = await _MongoContext.mQRFPositionFOC.ReplaceOneAsync(Builders<mQRFPositionFOC>.Filter.Eq("PositionFOCId", item.PositionFOCId), objPositionFOC);
                            response.ResponseStatus.Status = replaceResult.MatchedCount > 0 ? "Success" : "Failure";
                            response.ResponseStatus.ErrorMessage = replaceResult.MatchedCount > 0 ? "Saved Successfully." : "Details not updated.";
                        }
                    }
                }
                else
                {
                    mPositionFOC objPositionFOC;

                    if (request.PositionFOC.Count > 0)
                    {
                        var resultFlag = await _MongoContext.mPosition.UpdateOneAsync(Builders<mPosition>.Filter.Eq("PositionId", request.PositionFOC[0].PositionId),
                                            Builders<mPosition>.Update.Set("StandardFOC", request.StandardFOC).Set("EditDate", DateTime.Now).Set("EditUser", request.PositionFOC[0].EditUser));
                    }
                    foreach (var item in request.PositionFOC)
                    {
                        if (string.IsNullOrEmpty(item.PositionFOCId) || item.PositionFOCId == Guid.Empty.ToString())
                        {
                            objPositionFOC = new mPositionFOC();
                            item.PositionFOCId = Guid.NewGuid().ToString();
                            item.CreateDate = DateTime.Now;
                            item.EditUser = "";
                            item.EditDate = null;
                            item.IsDeleted = false;
                            objPositionFOC = item;
                            await _MongoContext.mPositionFOC.InsertOneAsync(objPositionFOC);
                            response.ResponseStatus.Status = "Success";
                            response.ResponseStatus.ErrorMessage = "Saved Successfully.";
                        }
                        else
                        {
                            objPositionFOC = _MongoContext.mPositionFOC.AsQueryable().Where(a => a.PositionFOCId == item.PositionFOCId).FirstOrDefault();
                            objPositionFOC.EditDate = DateTime.Now;
                            objPositionFOC.EditUser = item.EditUser;
                            objPositionFOC.FOCQty = item.FOCQty;

                            ReplaceOneResult replaceResult = await _MongoContext.mPositionFOC.ReplaceOneAsync(Builders<mPositionFOC>.Filter.Eq("PositionFOCId", item.PositionFOCId), objPositionFOC);
                            response.ResponseStatus.Status = replaceResult.MatchedCount > 0 ? "Success" : "Failure";
                            response.ResponseStatus.ErrorMessage = replaceResult.MatchedCount > 0 ? "Saved Successfully." : "Details not updated.";
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
        #endregion

        #region Get Dynamic Tour Entity
        public async Task<TourEntitiesGetRes> GetDynamicTourEntities(TourEntitiesGetReq request)
        {
            TourEntitiesGetRes response = new TourEntitiesGetRes() { QRFID = request.QRFID, ResponseStatus = new ResponseStatus(), DynamicTourEntity = new List<DynamicTourEntity>() };
            try
            {
                var builderPT = Builders<mProductType>.Filter;
                var filterPT = builderPT.Where(q => q.ChargeBasis == "PUPD");

                var resultPT = await _MongoContext.mProductType.Find(filterPT).Project(q => new mProductType { Prodtype = q.Prodtype, Name = q.Name }).ToListAsync();

                if (resultPT != null && resultPT.Count > 0)
                {
                    List<string> lstStr = resultPT.Select(a => a.Prodtype).ToList();
                    var builder = Builders<mPosition>.Filter;
                    var filter = builder.Where(q => q.QRFID == request.QRFID && lstStr.Contains(q.ProductType) && q.IsDeleted == false);

                    var result = await _MongoContext.mPosition.Find(filter).Project(q => new DynamicTourEntity
                    {
                        PositionID = q.PositionId,
                        ProductID = q.ProductID,
                        ProductName = q.ProductName,
                        CityName = q.CityName,
                        Duration = q.Duration.ToString() + "D",
                        ProductType = q.ProductType,
                        ProductTypeID = q.ProductTypeId,
                        StartDay = q.StartingFrom,
                        Status = q.IsDeleted,
                        EditUser = q.EditUser,
                        IsTourEntity = q.IsTourEntity
                    }).ToListAsync();

                    // var resdeleted = result.Where(a => a.Status == true).Select(a => a.PositionID);
                    List<TourEntities> resquote = new List<TourEntities>();
                    var quote = _MongoContext.mQuote.AsQueryable().Where(a => a.QRFID == request.QRFID).FirstOrDefault();
                    if (quote != null)
                    {
                        quote.TourEntities = quote.TourEntities.Where(a => a.IsDeleted == false).ToList();
                        resquote = quote.TourEntities.FindAll(a => a.IsDeleted == false && (!result.Exists(b => a.PositionID == b.PositionID) || result.Exists(c => c.Status == true && a.PositionID == c.PositionID)));

                        if (resquote != null && resquote.Count > 0)
                        {
                            foreach (var item in resquote)
                            {
                                item.IsDeleted = true;
                                item.EditDate = DateTime.Now;
                                item.EditUser = (string.IsNullOrEmpty(item.PositionID) || result.Where(a => a.PositionID == item.PositionID).FirstOrDefault() == null) ? "" : result.Where(a => a.PositionID == item.PositionID).FirstOrDefault().EditUser;
                                await _MongoContext.mQuote.FindOneAndUpdateAsync(m => m.QRFID == request.QRFID && m.TourEntities.Any(a => a.TourEntityID == item.TourEntityID),
                                             Builders<mQuote>.Update.Set(m => m.TourEntities[-1], item));
                            }
                        }

                        var quoteupdated = _MongoContext.mQuote.AsQueryable().Where(a => a.QRFID == request.QRFID).FirstOrDefault();
                        var pos = quoteupdated.TourEntities.Where(a => a.IsDeleted == false && result.Select(b => b.PositionID).ToList().Contains(a.PositionID)).ToList();

                        if (pos != null && pos.Count > 0)
                        {
                            var restour = result.Select(b => (b.ProductType.ToLower() == "assistant" && b.IsTourEntity) ? b.ProductName : (b.ProductType + "(" + b.CityName + "," + b.StartDay + "," + b.Duration + ")")).ToList();
                            var nmchanged = pos.Where(a => !restour.Exists(b => b == a.Type)).ToList();

                            DynamicTourEntity newposnm = new DynamicTourEntity();
                            foreach (var item in nmchanged)
                            {
                                newposnm = result.Where(a => a.PositionID == item.PositionID).FirstOrDefault();
                                item.Type = (newposnm.ProductType.ToLower() == "assistant" && newposnm.IsTourEntity) ? newposnm.ProductName : (newposnm.ProductType + "(" + newposnm.CityName + "," + newposnm.StartDay + "," + newposnm.Duration + ")");
                                item.EditDate = DateTime.Now;
                                item.EditUser = string.IsNullOrEmpty(item.PositionID) ? "" : result.Where(a => a.PositionID == item.PositionID).FirstOrDefault().EditUser;
                                await _MongoContext.mQuote.FindOneAndUpdateAsync(m => m.QRFID == request.QRFID && m.TourEntities.Any(a => a.TourEntityID == item.TourEntityID),
                                             Builders<mQuote>.Update.Set(m => m.TourEntities[-1], item));
                            }
                        }

                        List<string> lstStatic = new List<string>() { "Tour Leader", "Assistant Tour Leader", "Chef", "Assistant Chef", "MOS", "Others" };
                        List<string> lstStaticothers = new List<string>() { "Chef Helper", "Actor", "Musician", "Photographer", "Cinematographer", "Local Coach Driver", "Caravan Driver", "Others" };
                        var res = new DynamicTourEntity();
                        var staticRes = result.Where(a => a.ProductType.ToLower() == "assistant" && a.IsTourEntity && a.Status == false).ToList();

                        //in below code Status field is used to check for if static TE already exists in DB
                        foreach (var item in lstStatic)
                        {
                            if (item.ToLower() == "others")
                            {
                                foreach (var itemothers in lstStaticothers)
                                {
                                    res = staticRes.Where(a => a.ProductType.ToLower() == "assistant" && a.ProductName.ToLower() == itemothers.ToLower()).FirstOrDefault();
                                    if (res == null || (res != null && quote.TourEntities.Where(a => a.PositionID == res.PositionID).FirstOrDefault() == null))
                                    {
                                        staticRes.Add(new DynamicTourEntity { ProductName = itemothers, ProductType = "Assistant", IsOther = true, IsTourEntity = true });
                                    }
                                    else
                                    {
                                        staticRes.Where(a => a.ProductType.ToLower() == "assistant" && a.ProductName.ToLower() == item.ToLower()).FirstOrDefault().Status = true;
                                        staticRes.Where(a => a.ProductType.ToLower() == "assistant" && a.ProductName.ToLower() == item.ToLower()).FirstOrDefault().IsOther = true;
                                    }
                                }
                            }
                            else
                            {
                                res = staticRes.Where(a => a.ProductType.ToLower() == "assistant" && a.ProductName.ToLower() == item.ToLower()).FirstOrDefault();
                                if (res == null || (res != null && quote.TourEntities.Where(a => a.PositionID == res.PositionID).FirstOrDefault() == null))
                                {
                                    staticRes.Add(new DynamicTourEntity { ProductName = item, ProductType = "Assistant", IsTourEntity = true });
                                }
                                else
                                {
                                    staticRes.Where(a => a.ProductType.ToLower() == "assistant" && a.ProductName.ToLower() == item.ToLower()).FirstOrDefault().Status = true;
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(request.GetType) && request.GetType.ToLower() == "allowances")
                        {
                            result = staticRes.Where(a => a.Status == true).ToList();
                        }
                        else
                        {
                            result.RemoveAll(a => a.IsTourEntity && a.ProductType.ToLower() == "assistant");
                            staticRes.AddRange(result.Where(a => a.Status == false).ToList());
                            result = staticRes;
                        }
                    }

                    if (result != null)
                    {
                        response.ResponseStatus.ErrorMessage = "";
                        response.ResponseStatus.Status = "Success";
                        response.DynamicTourEntity = result;
                    }
                    else
                    {
                        response.ResponseStatus.ErrorMessage = "Details not found";
                        response.ResponseStatus.Status = "Failure";
                    }
                }
                else
                {
                    response.ResponseStatus.ErrorMessage = "No Product Types found in ProductType Master.";
                    response.ResponseStatus.Status = "Failure";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.ErrorMessage = ex.Message;
                response.ResponseStatus.Status = "Failure";
            }
            return response;
        }
        #endregion

        #region Get QuickPick Activities
        public async Task<PosQuicePickGetRes> GetQuickPickActivities(PositionGetReq request)
        {
            var response = new PosQuicePickGetRes();
            request.ProductType = request.ProductType ?? new List<ProductType>();

            try
            {
                var resultQuote = await _MongoContext.mQuote.FindAsync(m => m.QRFID == request.QRFID);
                var resPosition = await _MongoContext.mPosition.FindAsync(m => m.QRFID == request.QRFID && (m.ProductType == "Attractions" || m.ProductType == "Sightseeing - CityTour") && m.IsDeleted == false);

                var PosList = resPosition.ToList().Select(a => new { a.StartingFrom, a.RoutingDaysID, a.CityName, a.ProductName, a.StartTime, a.PositionId }).ToList();
                if (resultQuote != null && resultQuote.ToList().Count > 0)
                {
                    #region Routing Days 
                    RoutingDaysGetRes RoutingRes = await _quoteRepository.GetQRFRoutingDays(new RoutingDaysGetReq { QRFID = request.QRFID });
                    #endregion

                    if (RoutingRes.RoutingDays != null && RoutingRes.RoutingDays.Count > 0)
                    {
                        string[] City = new string[2];
                        List<ProductSearchDetails> result = new List<ProductSearchDetails>();
                        List<ProductSearchDetails> finalresult = new List<ProductSearchDetails>();
                        Dictionary<string, List<string>> countryCol = new Dictionary<string, List<string>>();
                        List<string> countrylist = new List<string>();
                        List<string> citylist = new List<string>();

                        foreach (var item in RoutingRes.RoutingDays)
                        {
                            if (!string.IsNullOrEmpty(item.FromCityName) && item.FromCityName.Contains(',')) City = item.FromCityName.Split(',');
                            if (!countrylist.Contains(City[1].Trim())) countrylist.Add(City[1].Trim());
                        }

                        foreach (var item in countrylist)
                        {
                            foreach (var item2 in RoutingRes.RoutingDays)
                            {
                                if (!string.IsNullOrEmpty(item2.FromCityName) && item2.FromCityName.Contains(',')) City = item2.FromCityName.Split(',');
                                if (!citylist.Contains(City[0].Trim())) citylist.Add(City[0].Trim());
                            }
                            countryCol.Add(item, citylist);
                        }

                        foreach (var item in countryCol)
                        {
                            ProductSearchReq ProdRequest = new ProductSearchReq
                            {
                                ProdType = "Attractions,Sightseeing - CityTour".Split(',').ToList(),
                                CityList = item.Value,
                                CountryName = item.Key,
                                ProductAttributeName = "PickListProduct",
                                ProductAttributeValue = "Y"
                            };

                            result.AddRange(await _productRepository.GetProductDetailsByCountryCityProdType(ProdRequest));
                        }

                        foreach (var item in RoutingRes.RoutingDays)
                        {
                            if (!string.IsNullOrEmpty(item.FromCityName) && item.FromCityName.Contains(',')) City = item.FromCityName.Split(',');
                            if (City.Length > 0 && response.PosQuickPickList.Where(a => a.CityName == item.FromCityName).Count() < 1)
                            {
                                finalresult = result.Where(a => a.ProdLocation.CountryName == City[1].Trim() && a.ProdLocation.CityName == City[0].Trim()).ToList();
                                response.PosQuickPickList.Add(new PosQuickPickList
                                {
                                    CityID = item.FromCityID,
                                    CityName = item.FromCityName,
                                    PosQuickPickProductList = finalresult.Select(b => new PosQuickPickProductList
                                    {
                                        ProdId = b.VoyagerProduct_Id,
                                        ProdName = b.ProdName,
                                        ProdCode = b.ProdCode,
                                        ProdType = b.ProdType,
                                        ProdTypeId = b.ProdTypeId,
                                        ActivityDayNo = item.RoutingDaysID,
                                        DayName = item.Days,
                                        SupplierID = "",
                                        SupplierName = ""
                                    }).ToList()
                                });
                            }
                        }

                        foreach (var posItem in PosList)
                        {
                            response.PosQuickPickList
                                .Where(a => a.CityName.Split(',')[0] == posItem.CityName).ToList()
                                    .ForEach(b => b.PosQuickPickProductList
                                        .Where(a => a.ProdName == posItem.ProductName).ToList()
                                            .ForEach(a =>
                                            {
                                                a.IsSelected = true;
                                                a.StartTime = posItem.StartTime;
                                                a.PositionId = posItem.PositionId;
                                                a.ActivityDayNo = posItem.RoutingDaysID;
                                                a.DayName = posItem.StartingFrom;
                                            }));
                        }

                        response.ResponseStatus.Status = "Success";
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "Routing details not exists";
                    }
                }
                else
                {
                    response.PosQuickPickList = new List<PosQuickPickList>();
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "QRFID is not exists.";
                }

                response.QRFID = request.QRFID;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return response;
        }
        #endregion

        #region Set DefaultMealPlan Accomodation
        public async Task<PositionDefMealSetRes> SetDefaultMealPlan(PositionDefMealSetReq request)
        {
            PositionDefMealSetRes response = new PositionDefMealSetRes();
            try
            {
                var resultQuote = await _MongoContext.mQuote.FindAsync(m => m.QRFID == request.QRFID).Result.FirstOrDefaultAsync();

                if (resultQuote != null)
                {
                    var resultPosition = _MongoContext.mPosition.AsQueryable().Where(q => q.QRFID == request.QRFID && q.IsDeleted == false && q.ProductType.ToLower() == request.ProductType.ToLower()).ToList();

                    if (resultPosition != null && resultPosition.Count > 0)
                    {
                        var posids = resultPosition.Select(a => a.PositionId).ToList();
                        UpdateResult resultFlag;
                        foreach (var item in posids)
                        {
                            resultFlag = await _MongoContext.mPosition.UpdateOneAsync(Builders<mPosition>.Filter.Where(a => a.QRFID == request.QRFID && a.PositionId == item),
                                          Builders<mPosition>.Update.Set("MealPlan", request.MealType).Set("EditDate", DateTime.Now).Set("EditUser", request.UserName));
                        }

                        resultFlag = await _MongoContext.mQuote.UpdateOneAsync(Builders<mQuote>.Filter.Where(a => a.QRFID == request.QRFID),
                                          Builders<mQuote>.Update.Set("DefaultMealPlan", request.MealType).Set("EditDate", DateTime.Now).Set("EditUser", request.UserName));

                        response.ResponseStatus.Status = resultFlag.MatchedCount > 0 ? "Success" : "Failure";
                        response.ResponseStatus.ErrorMessage = resultFlag.MatchedCount > 0 ? "Updated Successfully." : "Details not updated.";
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "No Position details found.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "QRFID is not exists.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }
            return response;
        }
        #endregion

        #region Helper

        public List<mQRFPosition> ConvertPositionToQRFPosition(List<mPosition> position)
        {
            List<mQRFPosition> qrfPosition = new List<mQRFPosition>();

            foreach (var pos in position)
            {
                var qrfpos = new mQRFPosition();

                qrfpos.ApplyAcrossDays = pos.ApplyAcrossDays;
                qrfpos.BudgetCategory = pos.BudgetCategory;
                qrfpos.BudgetCategoryId = pos.BudgetCategoryId;
                qrfpos.BuyCurrencyId = pos.BuyCurrencyId;
                qrfpos.BuyCurrency = pos.BuyCurrency;
                qrfpos.ChainID = pos.ChainID;
                qrfpos.ChainName = pos.ChainName;
                qrfpos.CountryName = pos.CountryName;
                qrfpos.CityName = pos.CityName;
                qrfpos.CityID = pos.CityID;
                qrfpos.CreateDate = pos.CreateDate;
                qrfpos.CreateUser = pos.CreateUser;
                qrfpos.DayNo = pos.DayNo;
                qrfpos.DeletedFrom = pos.DeletedFrom;
                qrfpos.Duration = pos.Duration != 0 ? Convert.ToInt32(pos.Duration) : 0;
                qrfpos.EarlyCheckInDate = pos.EarlyCheckInDate;
                qrfpos.EarlyCheckInTime = pos.EarlyCheckInTime;
                qrfpos.EditDate = pos.EditDate;
                qrfpos.EditUser = pos.EditUser;
                qrfpos.EndTime = pos.EndTime;
                qrfpos.FromPickUpLoc = pos.FromPickUpLoc;
                qrfpos.FromPickUpLocID = pos.FromPickUpLocID;
                qrfpos.InterConnectingRooms = pos.InterConnectingRooms;
                qrfpos.IsDeleted = pos.IsDeleted;
                qrfpos.KeepAs = pos.KeepAs;
                qrfpos.LateCheckOutDate = pos.LateCheckOutDate;
                qrfpos.LateCheckOutTime = pos.LateCheckOutTime;
                qrfpos.Location = pos.Location;
                qrfpos.MealPlan = pos.MealPlan;
                qrfpos.MealType = pos.MealType;
                qrfpos.NoOfPaxAdult = pos.NoOfPaxAdult;
                qrfpos.NoOfPaxChild = pos.NoOfPaxChild;
                qrfpos.NoOfPaxInfant = pos.NoOfPaxInfant;
                qrfpos.OPSRemarks = pos.OPSRemarks;
                qrfpos.PositionId = pos.PositionId;
                qrfpos.PositionSequence = pos.PositionSequence;
                qrfpos.ProductAttributeType = pos.ProductAttributeType;
                qrfpos.ProductID = pos.ProductID;
                qrfpos.ProductName = pos.ProductName;
                qrfpos.ProductType = pos.ProductType;
                qrfpos.ProductTypeId = pos.ProductTypeId;
                qrfpos.QRFID = pos.QRFID;
                qrfpos.RoutingDaysID = pos.RoutingDaysID;
                qrfpos.StandardFOC = pos.StandardFOC;
                qrfpos.StandardPrice = pos.StandardPrice;
                qrfpos.StarRating = pos.StarRating;
                qrfpos.StartingFrom = pos.StartingFrom;
                qrfpos.StartTime = pos.StartTime;
                qrfpos.Status = pos.Status;
                qrfpos.SupplierId = pos.SupplierId;
                qrfpos.SupplierName = pos.SupplierName;
                qrfpos.TLRemarks = pos.TLRemarks;
                qrfpos.ToCityID = pos.ToCityID;
                qrfpos.ToCityName = pos.ToCityName;
                qrfpos.ToCountryName = pos.ToCountryName;
                qrfpos.ToDropOffLoc = pos.ToDropOffLoc;
                qrfpos.ToDropOffLocID = pos.ToDropOffLocID;
                qrfpos.TransferDetails = pos.TransferDetails;
                qrfpos.TypeOfExcursion = pos.TypeOfExcursion;
                qrfpos.TypeOfExcursion_Id = pos.TypeOfExcursion_Id;
                qrfpos.WashChangeRooms = pos.WashChangeRooms;
                qrfpos.IsCityPermit = pos.IsCityPermit;
                qrfpos.IsParkingCharges = pos.IsParkingCharges;
                qrfpos.IsRoadTolls = pos.IsRoadTolls;

                foreach (var qrfroom in pos.RoomDetailsInfo)
                {
                    var room = new QRFRoomDetailsInfo();
                    room.CreateDate = qrfroom.CreateDate;
                    room.CreateUser = qrfroom.CreateUser;
                    room.EditDate = qrfroom.EditDate;
                    room.EditUser = qrfroom.EditUser;
                    room.IsSupplement = qrfroom.IsSupplement;
                    room.CrossPositionId = qrfroom.CrossPositionId;
                    //room.CrossPosition = qrfroom.CrossPosition;
                    room.ProdDesc = qrfroom.ProdDesc;
                    room.IsDeleted = qrfroom.IsDeleted;
                    room.ProductCategory = qrfroom.ProductCategory;
                    room.ProductCategoryId = qrfroom.ProductCategoryId;
                    room.ProductRange = qrfroom.ProductRange;
                    room.ProductRangeId = qrfroom.ProductRangeId;
                    room.RoomId = qrfroom.RoomId;
                    room.RoomSequence = qrfroom.RoomSequence;

                    qrfpos.RoomDetailsInfo.Add(room);
                }
                qrfPosition.Add(qrfpos);
            }
            return qrfPosition;
        }

        public List<mPosition> ConvertQRFPositionToPosition(List<mQRFPosition> qrfposition)
        {
            List<mPosition> Position = new List<mPosition>();

            foreach (var qrfpos in qrfposition)
            {
                var pos = new mPosition();

                pos.ApplyAcrossDays = qrfpos.ApplyAcrossDays;
                pos.BudgetCategory = qrfpos.BudgetCategory;
                pos.BudgetCategoryId = qrfpos.BudgetCategoryId;
                pos.BuyCurrencyId = qrfpos.BuyCurrencyId;
                pos.BuyCurrency = qrfpos.BuyCurrency;
                pos.ChainID = qrfpos.ChainID;
                pos.ChainName = qrfpos.ChainName;
                pos.CountryName = qrfpos.CountryName;
                pos.CityName = qrfpos.CityName;
                pos.CityID = qrfpos.CityID;
                pos.CreateDate = qrfpos.CreateDate;
                pos.CreateUser = qrfpos.CreateUser;
                pos.DayNo = qrfpos.DayNo;
                pos.DeletedFrom = qrfpos.DeletedFrom;
                pos.Duration = qrfpos.Duration != 0 ? Convert.ToInt32(qrfpos.Duration) : 0;
                pos.EarlyCheckInDate = qrfpos.EarlyCheckInDate;
                pos.EarlyCheckInTime = qrfpos.EarlyCheckInTime;
                pos.EditDate = qrfpos.EditDate;
                pos.EditUser = qrfpos.EditUser;
                pos.EndTime = qrfpos.EndTime;
                pos.FromPickUpLoc = qrfpos.FromPickUpLoc;
                pos.FromPickUpLocID = qrfpos.FromPickUpLocID;
                pos.InterConnectingRooms = qrfpos.InterConnectingRooms;
                pos.IsDeleted = qrfpos.IsDeleted;
                pos.KeepAs = qrfpos.KeepAs;
                pos.LateCheckOutDate = qrfpos.LateCheckOutDate;
                pos.LateCheckOutTime = qrfpos.LateCheckOutTime;
                pos.Location = qrfpos.Location;
                pos.MealPlan = qrfpos.MealPlan;
                pos.MealType = qrfpos.MealType;
                pos.NoOfPaxAdult = qrfpos.NoOfPaxAdult;
                pos.NoOfPaxChild = qrfpos.NoOfPaxChild;
                pos.NoOfPaxInfant = qrfpos.NoOfPaxInfant;
                pos.OPSRemarks = qrfpos.OPSRemarks;
                pos.PositionId = qrfpos.PositionId;
                pos.PositionSequence = qrfpos.PositionSequence;
                pos.ProductAttributeType = qrfpos.ProductAttributeType;
                pos.ProductID = qrfpos.ProductID;
                pos.ProductName = qrfpos.ProductName;
                pos.ProductType = qrfpos.ProductType;
                pos.ProductTypeId = qrfpos.ProductTypeId;
                pos.QRFID = qrfpos.QRFID;
                pos.RoutingDaysID = qrfpos.RoutingDaysID;
                pos.StandardFOC = qrfpos.StandardFOC;
                pos.StandardPrice = qrfpos.StandardPrice;
                pos.StarRating = qrfpos.StarRating;
                pos.StartingFrom = qrfpos.StartingFrom;
                pos.StartTime = qrfpos.StartTime;
                pos.Status = qrfpos.Status;
                pos.SupplierId = qrfpos.SupplierId;
                pos.SupplierName = qrfpos.SupplierName;
                pos.TLRemarks = qrfpos.TLRemarks;
                pos.ToCityID = qrfpos.ToCityID;
                pos.ToCityName = qrfpos.ToCityName;
                pos.ToCountryName = qrfpos.ToCountryName;
                pos.ToDropOffLoc = qrfpos.ToDropOffLoc;
                pos.ToDropOffLocID = qrfpos.ToDropOffLocID;
                pos.TransferDetails = qrfpos.TransferDetails;
                pos.TypeOfExcursion = qrfpos.TypeOfExcursion;
                pos.TypeOfExcursion_Id = qrfpos.TypeOfExcursion_Id;
                pos.WashChangeRooms = qrfpos.WashChangeRooms;

                foreach (var qrfroom in qrfpos.RoomDetailsInfo)
                {
                    var room = new RoomDetailsInfo();
                    room.CreateDate = qrfroom.CreateDate;
                    room.CreateUser = qrfroom.CreateUser;
                    room.EditDate = qrfroom.EditDate;
                    room.EditUser = qrfroom.EditUser;
                    room.IsSupplement = qrfroom.IsSupplement;
                    room.CrossPositionId = qrfroom.CrossPositionId;
                    //  room.CrossPosition = qrfroom.CrossPosition;
                    room.ProdDesc = qrfroom.ProdDesc;
                    room.IsDeleted = qrfroom.IsDeleted;
                    room.ProductCategory = qrfroom.ProductCategory;
                    room.ProductCategoryId = qrfroom.ProductCategoryId;
                    room.ProductRange = qrfroom.ProductRange;
                    room.ProductRangeId = qrfroom.ProductRangeId;
                    room.RoomId = qrfroom.RoomId;
                    room.RoomSequence = qrfroom.RoomSequence;

                    pos.RoomDetailsInfo.Add(room);
                }
                Position.Add(pos);
            }
            return Position;
        }

        public List<mPositionPrice> ConvertQRFPosPriceToPosPrice(List<mPositionPriceQRF> qrfpositionPrice)
        {
            List<mPositionPrice> PositionPice = new List<mPositionPrice>();

            foreach (var price in qrfpositionPrice)
            {
                PositionPice.Add(new mPositionPrice
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
            return PositionPice;
        }

        public List<mPositionPriceQRF> ConvertPosPriceToQRFPosPrice(List<mPositionPrice> positionPrice)
        {
            List<mPositionPriceQRF> PositionQRF = new List<mPositionPriceQRF>();

            foreach (var price in positionPrice)
            {
                PositionQRF.Add(new mPositionPriceQRF
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
            return PositionQRF;
        }

        public List<mPositionFOC> ConvertQRFPosFOCToPosFOC(List<mQRFPositionFOC> qrfpositionFOC)
        {
            List<mPositionFOC> PositionFOC = new List<mPositionFOC>();

            foreach (var foc in qrfpositionFOC)
            {
                PositionFOC.Add(new mPositionFOC
                {
                    _Id = foc._Id,
                    PositionFOCId = foc.PositionFOCId,
                    QRFID = foc.QRFID,
                    Period = foc.Period,
                    PositionId = foc.PositionId,
                    DepartureId = foc.DepartureId,
                    PaxSlabId = foc.PaxSlabId,
                    PaxSlab = foc.PaxSlab,
                    ContractPeriod = foc.ContractPeriod,
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
            return PositionFOC;
        }

        public List<mQRFPositionFOC> ConvertPosFOCToQRFPosFOC(List<mPositionFOC> positionFOC)
        {
            List<mQRFPositionFOC> qrfPositionFOC = new List<mQRFPositionFOC>();

            foreach (var foc in positionFOC)
            {
                qrfPositionFOC.Add(new mQRFPositionFOC
                {
                    _Id = foc._Id,
                    PositionFOCId = foc.PositionFOCId,
                    QRFID = foc.QRFID,
                    Period = foc.Period,
                    PositionId = foc.PositionId,
                    DepartureId = foc.DepartureId,
                    ContractPeriod = foc.ContractPeriod,
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
            return qrfPositionFOC;
        }
        #endregion

        #region PositionRoomDetails
        public async Task<PositionRoomsGetRes> GetPositionRoomDetails(PositionRoomsGetReq request)
        {
            PositionRoomsGetRes response = new PositionRoomsGetRes()
            {
                ProductRangeDetails = new List<ProductRangeDetails>(),
                ProductId = request.ProductId,
                ResponseStatus = new ResponseStatus(),
                PositionId = request.PositionId,
                RoomDetailsInfo = new List<RoomDetailsInfo>()
            };
            try
            {
                var builder = Builders<mPosition>.Filter;
                var filter = builder.Where(q => q.QRFID == request.QRFId && q.PositionId == request.PositionId && q.IsDeleted == false);
                var result = await _MongoContext.mPosition.Find(filter).FirstOrDefaultAsync();

                if (result != null)
                {
                    ProductRangeGetRes res = _productRepository.GetProductRangeByParam(new ProductRangeGetReq { ProductCatId = result.BudgetCategoryId, ProductId = result.ProductID, QRFID = result.QRFID });
                    if (res != null && res.ProductRangeDetails != null && res.ProductRangeDetails.Count > 0)
                    {
                        response.ProductRangeDetails = res.ProductRangeDetails;
                        response.ProductCatId = result.BudgetCategoryId;
                        response.ProductId = result.ProductID;
                        response.SupplierId = result.SupplierId;
                        response.SupplierName = result.SupplierName;
                        response.RoomDetailsInfo = result.RoomDetailsInfo.Where(a => a.IsDeleted == false).ToList();
                        response.ResponseStatus.Status = "Success";
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Error";
                        response.ResponseStatus.ErrorMessage = "Product Range not found.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Error";
                    response.ResponseStatus.ErrorMessage = "PositionID not exists in mPosition collection.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Error";
                response.ResponseStatus.ErrorMessage = string.IsNullOrEmpty(ex.Message) ? ex.InnerException != null ? ex.InnerException.Message : ex.Message : "";
            }
            return response;
        }

        public async Task<PositionRoomsSetRes> SetPositionRoomDetails(PositionRoomsSetReq request)
        {
            UpdateResult resultFlag = null;
            PositionRoomsSetRes response = new PositionRoomsSetRes()
            {
                QRFId = request.QRFId,
                PositionId = request.PositionId,
                ProductId = request.ProductId,
                ResponseStatus = new ResponseStatus(),
                RoomDetailsInfo = new List<RoomDetailsInfo>()
            };

            try
            {
                var builder = Builders<mPosition>.Filter;
                var filter = builder.Where(q => q.QRFID == request.QRFId && q.PositionId == request.PositionId && q.IsDeleted == false);
                var position = await _MongoContext.mPosition.Find(filter).FirstOrDefaultAsync();

                if (position != null)
                {
                    List<string> RangeIdList = new List<string>();
                    RangeIdList.AddRange(request.RoomDetailsInfo.Where(a => a.IsDeleted == false).Select(a => a.ProductRangeId).ToList());
                    if (RangeIdList != null && RangeIdList.Count > 0)
                    {
                        var ProdRangeList = _MongoContext.mProductRange.AsQueryable().Where(a => RangeIdList.Contains(a.VoyagerProductRange_Id))
                        .Select(a => new ProductRangeInfo
                        {
                            VoyagerProductRange_Id = a.VoyagerProductRange_Id,
                            ProductRangeCode = a.ProductRangeCode,
                            ProductType_Id = a.ProductType_Id,
                            PersonType = a.PersonType,
                            ProductMenu = a.ProductMenu
                        }).ToList();

                        request.RoomDetailsInfo.ForEach(p => { p.RoomId = (string.IsNullOrEmpty(p.RoomId) || p.RoomId == "0") ? Guid.NewGuid().ToString() : p.RoomId; });
                        request.RoomDetailsInfo.FindAll(a => position.RoomDetailsInfo.Exists(b => a.RoomId == b.RoomId)).
                            ForEach(a =>
                            {
                                a.CrossPositionId = position.RoomDetailsInfo.Where(c => c.RoomId == a.RoomId).FirstOrDefault().CrossPositionId;
                            });
                        request.RoomDetailsInfo.AddRange(position.RoomDetailsInfo.Where(p => p.IsDeleted).ToList());

                        request.RoomDetailsInfo.ForEach(a =>
                        {
                            a.ProdDesc = ProdRangeList.Where(b => b.VoyagerProductRange_Id == a.ProductRangeId).Count() > 0 ?
                                  ProdRangeList.Where(b => b.VoyagerProductRange_Id == a.ProductRangeId).FirstOrDefault().ProductMenu : "";
                            a.ProductCategory = position.BudgetCategory;
                            a.ProductCategoryId = position.BudgetCategoryId;
                        });

                        request.RoomDetailsInfo = request.RoomDetailsInfo.OrderBy(a => a.IsSupplement).ThenBy(a => a.ProductRange).ToList();
                        resultFlag = await _MongoContext.mPosition.UpdateOneAsync(Builders<mPosition>.Filter.Eq("PositionId", request.PositionId),
                                                 Builders<mPosition>.Update.Set("RoomDetailsInfo", request.RoomDetailsInfo).Set("EditDate", DateTime.Now).Set("EditUser", request.EditUser));

                        var ProductTypeList = new List<string>();
                        ProductTypeList.Add(position.ProductType);
                        response.RoomDetailsInfo = request.RoomDetailsInfo.Where(a => a.IsDeleted == false).ToList();
                        PositionPriceFOCSetRes res = await SetAllPositionPriceFOC(new PositionPriceFOCSetReq
                        {
                            PositionId = request.PositionId,
                            QRFID = request.QRFId,
                            IsFOC = false,
                            IsPrice = true,
                            ProductRangeInfo = ProdRangeList,
                            ProductTypeList = ProductTypeList,
                            LoginUserId = request.EditUser
                        });
                        if (res != null && res.PositionPrice != null && res.ResponseStatus != null && res.ResponseStatus.Status == "Success")
                        {
                            response.ResponseStatus.Status = "Success";
                            response.ResponseStatus.ErrorMessage = "Details Saved Successfully.";
                        }
                        else
                        {
                            response.ResponseStatus.Status = "Error";
                            response.ResponseStatus.ErrorMessage = "Details not Saved.";
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Error";
                        response.ResponseStatus.ErrorMessage = "Product Range Details not exists in mPosition collection.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Error";
                    response.ResponseStatus.ErrorMessage = "PositionID not exists in mPosition collection.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Error";
                response.ResponseStatus.ErrorMessage = string.IsNullOrEmpty(ex.Message) ? ex.InnerException != null ? ex.InnerException.Message : ex.Message : "";
            }
            return response;
        }
        #endregion
    }
}