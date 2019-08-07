using Microsoft.Extensions.Configuration;
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
    public class MealRepository : IMealRepository
    {
        #region Private Variable Declaration
        private readonly MongoContext _MongoContext = null;
        private readonly IConfiguration _configuration;
        private readonly IGenericRepository _genericRepository;
        private readonly IQuoteRepository _quoteRepository;
        #endregion

        public MealRepository(IOptions<MongoSettings> settings, IGenericRepository genericRepository, IConfiguration configuration, IQuoteRepository quoteRepository)
        {
            _MongoContext = new MongoContext(settings);
            _configuration = configuration;
            _genericRepository = genericRepository;
            _quoteRepository = quoteRepository;
        }

        public async Task<MealsGetRes> GetMealsDetailsByQRFID(QuoteGetReq request)
        {
            var response = new MealsGetRes();
            List<MealDetails> objMealsProperties = new List<MealDetails>();

            RoutingGetReq req = new RoutingGetReq();
            req.QRFId = request.QRFID;

            var resultQuote = _MongoContext.mQuote.AsQueryable().Where(q => q.QRFID == request.QRFID);

            if (resultQuote != null && resultQuote.Count() > 0)
            {
                var resultPosition = _MongoContext.mPosition.AsQueryable().Where(q => q.QRFID == request.QRFID).FirstOrDefault();
                List<RoutingInfo> lstRoutingInfo = await _quoteRepository.GetQRFRouteDetailsByQRFID(req);

                if (resultPosition != null)
                {
                    var result = resultPosition.Meals;
                    if (result != null && result.Count > 0)
                    {
                        var CityCount = result.GroupBy(m => new { m.CityID }).Select(m => new { m.Key.CityID, DayCount = m.Count() });

                        if ((lstRoutingInfo != null && lstRoutingInfo.Count > 0) && (CityCount != null && CityCount.Count() > 0))
                        {
                            result.ToList().FindAll(f => !lstRoutingInfo.Exists(r => r.ToCityID == f.CityID)).ForEach(c => c.IsDeleted = true);

                            foreach (var item in lstRoutingInfo)
                            {
                                int daycount = CityCount.Where(c => c.CityID == item.ToCityID).Select(c => c.DayCount).FirstOrDefault();
                                if (item.Days > daycount)
                                {
                                    // int i = item.Days - daycount; //5-3=2 
                                    for (int j = daycount; j < item.Days; j++)
                                    {
                                        result.Add(new MealDetails { CityID = item.ToCityID, CityName = item.ToCityName, VenueTypes = new List<VenueTypes>() });
                                    }
                                }
                                else if (item.Days < daycount)
                                {
                                    int i = daycount - item.Days; //5-3=2                                
                                    result.Where(r => r.CityID == item.ToCityID).TakeLast(i).ToList().ForEach(m => { m.IsDeleted = true; });
                                }
                            }
                            TimeSpan timeoutprev;
                            foreach (var m in result)
                            {
                                m.SequenceNo = lstRoutingInfo.Where(r => r.ToCityID == m.CityID).Select(t => t.RouteSequence).FirstOrDefault();
                                AccomodationInfo objAccomodationInfo = resultPosition.AccomodationInfo.Where(a => a.StartingFrom == m.DayID && a.IsDeleted == false).FirstOrDefault();
                                if (objAccomodationInfo != null)
                                {
                                    MealDetails md = CheckDefaultMealPlan(m, timeoutprev, objAccomodationInfo, out TimeSpan newtimeout);
                                    m.Breakfast = md.Breakfast;
                                    m.DefaultPlan = md.DefaultPlan;
                                    m.Dinner = md.Dinner;
                                    m.Lunch = md.Lunch;
                                    timeoutprev = newtimeout;
                                }
                            }
                            result = result.Where(m => m.IsDeleted == false).OrderBy(m => m.SequenceNo).ToList();

                            int k = 1;
                            result.ForEach(m => m.DayID = "Day " + (k++));
                            response.MealDetails = result;
                            response.ResponseStatus.Status = "Success";
                        }
                        else if ((lstRoutingInfo != null && lstRoutingInfo.Count > 0) && CityCount.Count() == 0)
                        {
                            mPosition objPosition = AddMealsDetails(objMealsProperties, lstRoutingInfo, request, resultPosition.AccomodationInfo, "i");
                            result = objPosition.Meals;

                            var resultFlag = await _MongoContext.mPosition.UpdateOneAsync(Builders<mPosition>.Filter.Eq("QRFID", request.QRFID),
                                    Builders<mPosition>.Update.Set("Meals", result));

                            response.ResponseStatus.Status = resultFlag.MatchedCount > 0 ? "Success" : "Failure";
                            response.ResponseStatus.ErrorMessage = resultFlag.MatchedCount > 0 ? "Saved Successfully." : "Meals Details not updated.";
                        }
                        else
                        {
                            TimeSpan timeoutprev;
                            var agentroute = resultQuote.FirstOrDefault().RoutingInfo.FirstOrDefault();
                            result.ForEach(m =>
                            {
                                m.IsDeleted = true; m.EditDate = agentroute.EditDate; m.EditUser = request.UserName;
                                m.SequenceNo = lstRoutingInfo.Where(r => r.ToCityID == m.CityID).Select(t => t.RouteSequence).FirstOrDefault();
                                AccomodationInfo objAccomodationInfo = resultPosition.AccomodationInfo.Where(a => a.StartingFrom == m.DayID && a.IsDeleted == false).FirstOrDefault();
                                if (objAccomodationInfo != null)
                                {
                                    MealDetails md = CheckDefaultMealPlan(m, timeoutprev, objAccomodationInfo, out TimeSpan newtimeout);
                                    m.Breakfast = md.Breakfast;
                                    m.DefaultPlan = md.DefaultPlan;
                                    m.Dinner = md.Dinner;
                                    m.Lunch = md.Lunch;
                                    timeoutprev = newtimeout;
                                }
                            });

                            UpdateResult resultFlag = await _MongoContext.mPosition.UpdateOneAsync(Builders<mPosition>.Filter.Eq("QRFID", request.QRFID),
                                     Builders<mPosition>.Update.Set("Meals", result));

                            response.ResponseStatus.Status = resultFlag.MatchedCount > 0 ? "Success" : "Failure";
                            response.ResponseStatus.ErrorMessage = resultFlag.MatchedCount > 0 ? "Saved Successfully." : "Meals Details not updated.";
                        }
                    }
                    else
                    {
                        UpdateResult resultFlag = null;
                        if (lstRoutingInfo != null && lstRoutingInfo.Count > 0)
                        {
                            mPosition objPosition = AddMealsDetails(objMealsProperties, lstRoutingInfo, request, resultPosition.AccomodationInfo, "i");

                            resultFlag = await _MongoContext.mPosition.UpdateOneAsync(Builders<mPosition>.Filter.Eq("QRFID", request.QRFID),
                                            Builders<mPosition>.Update.Set("Meals", objPosition.Meals));
                            response.MealDetails = objPosition.Meals;
                            response.ResponseStatus.Status = resultFlag.MatchedCount > 0 ? "Success" : "Failure";
                            response.ResponseStatus.ErrorMessage = resultFlag.MatchedCount > 0 ? "Saved Successfully." : "Meals Details not updated.";
                        }
                        else
                        {
                            response.ResponseStatus.ErrorMessage = "No Routing Details found.";
                            response.ResponseStatus.Status = "Failure";
                        }
                    }
                }
                else
                {
                    if (lstRoutingInfo != null && lstRoutingInfo.Count > 0)
                    {
                        mPosition objPosition = AddMealsDetails(objMealsProperties, lstRoutingInfo, request, resultPosition.AccomodationInfo, "i");

                        await _MongoContext.mPosition.InsertOneAsync(objPosition);
                        response.ResponseStatus.Status = "Success";
                        response.MealDetails = objPosition.Meals;
                    }
                    else
                    {
                        response.ResponseStatus.ErrorMessage = "No Routing Details found.";
                        response.ResponseStatus.Status = "Failure";
                    }
                }
            }
            else
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "QRFID is not exists.";
            }
            return response;
        }

        public mPosition AddMealsDetails(List<MealDetails> objMealsProperties, List<RoutingInfo> lstRoutingInfo, QuoteGetReq request, List<AccomodationInfo> lstAcc, string status)
        {
            foreach (var item in lstRoutingInfo)
            {
                int day = item.Days;
                for (int i = 1; i <= day; i++)
                {
                    objMealsProperties.Add(new MealDetails { CityID = item.ToCityID, CityName = item.ToCityName, VenueTypes = new List<VenueTypes>(), DayID = "Day " + (objMealsProperties.Count + 1) });
                }
            }
            QRFCounterRequest qrfCounterRequest = new QRFCounterRequest();
            qrfCounterRequest.CounterType = _configuration["CounterType:Meals"].ToString();
            TimeSpan timeoutprev;

            if (status == "i")
            {
                foreach (var item in objMealsProperties)
                {
                    item.CreateUser = request.UserName;
                    item.CreateDate = DateTime.Now; item.EditDate = null; item.EditUser = "";
                    item.MealID = item.MealID == 0 ? _genericRepository.GetNextReferenceNumber(qrfCounterRequest).LastReferenceNumber : item.MealID;
                    item.SequenceNo = lstRoutingInfo.Where(r => r.ToCityID == item.CityID).Select(t => t.RouteSequence).FirstOrDefault();
                    item.VenueTypes.ForEach(a => { a.VenueDetails.CreateDate = DateTime.Now; a.VenueDetails.EditDate = null; a.VenueDetails.EditUser = ""; });
                    AccomodationInfo objAccomodationInfo = lstAcc.Where(a => a.StartingFrom == item.DayID && a.IsDeleted == false).FirstOrDefault();
                    if (objAccomodationInfo != null)
                    {
                        MealDetails md = CheckDefaultMealPlan(item, timeoutprev, objAccomodationInfo, out TimeSpan newtimeout);
                        item.Breakfast = md.Breakfast;
                        item.DefaultPlan = md.DefaultPlan;
                        item.Dinner = md.Dinner;
                        item.Lunch = md.Lunch;
                        timeoutprev = newtimeout;
                    }
                }
            }

            mPosition objPosition = new mPosition();
            objPosition.Meals = objMealsProperties.OrderBy(m => m.SequenceNo).ToList();
            objPosition.QRFID = request.QRFID;

            return objPosition;
        }

        public MealDetails CheckDefaultMealPlan(MealDetails m, TimeSpan timeoutprev, AccomodationInfo objAccomodationInfo, out TimeSpan newtimeout)
        {
            m.DefaultPlan = objAccomodationInfo.MealPlan;
            if (m.DefaultPlan != null && m.DefaultPlan.ToLower() != "nb")
            {
                var hotelid = objAccomodationInfo.HotelID;
                List<ProductAttributes> lstProdAttr = _MongoContext.mProducts.AsQueryable().Where(p => p.VoyagerProduct_Id == hotelid).Select(p => p.ProductAttributes).FirstOrDefault();
                if (lstProdAttr != null && lstProdAttr.Count > 0)
                {
                    List<ProdAttributeValues> lstprodval = lstProdAttr.Where(p => p.AttributeGroupName == "Times").Select(p => p.AttributeValues).FirstOrDefault();
                    if (lstprodval != null && lstprodval.Count > 0)
                    {
                        TimeSpan timein, timeout;

                        string strchkin = lstprodval.Where(a => a.AttributeName == "Check In").Select(a => a.AttributeValue).FirstOrDefault();
                        string strchkout = lstprodval.Where(a => a.AttributeName == "Check Out").Select(a => a.AttributeValue).FirstOrDefault();
                        TimeSpan.TryParse(strchkin, out timein);
                        TimeSpan.TryParse(strchkout, out timeout);
                        newtimeout = timeout;

                        if (timein != null && timeout != null)
                        {
                            if (m.DefaultPlan.ToLower() == "bb")
                            {
                                bool flag = false;
                                if ((timein >= new TimeSpan(07, 00, 00) && timein <= new TimeSpan(07, 30, 00)) ||
                                    (timeoutprev != null && timeoutprev >= new TimeSpan(07, 30, 00)))
                                {
                                    m.Breakfast = "Included in Hotel";
                                    flag = true;
                                }
                                if (!flag)
                                {
                                    m.DefaultPlan = "";
                                }
                            }
                            else if (m.DefaultPlan.ToLower() == "hb")
                            {
                                bool flag = false;

                                if ((timein >= new TimeSpan(07, 00, 00) && timein <= new TimeSpan(07, 30, 00)) ||
                                    (timeoutprev != null && timeoutprev >= new TimeSpan(07, 30, 00)))
                                {
                                    m.Breakfast = "Included in Hotel";
                                    flag = true;
                                }

                                if ((timein >= new TimeSpan(07, 00, 00) && timein <= new TimeSpan(21, 00, 00))
                                    || (timeoutprev != null && timeoutprev >= new TimeSpan(21, 00, 00)))
                                { m.Dinner = "Included in Hotel"; flag = true; }

                                if (!flag)
                                {
                                    m.DefaultPlan = "";
                                }
                            }
                            else if (m.DefaultPlan.ToLower() == "fb")
                            {
                                bool flag = false;

                                if ((timein >= new TimeSpan(07, 00, 00) && timein <= new TimeSpan(07, 30, 00))
                                    || (timeoutprev != null && timeoutprev >= new TimeSpan(07, 30, 00)))
                                {
                                    m.Breakfast = "Included in Hotel";
                                    flag = true;
                                }
                                if ((timein >= new TimeSpan(07, 00, 00) && timein <= new TimeSpan(13, 30, 00)) ||
                                    (timeoutprev != null && timeoutprev >= new TimeSpan(13, 30, 00)))
                                { m.Lunch = "Included in Hotel"; flag = true; }

                                if ((timein >= new TimeSpan(07, 00, 00) && timein <= new TimeSpan(21, 00, 00)) ||
                                    (timeoutprev != null && timeoutprev >= new TimeSpan(21, 00, 00)))
                                { m.Dinner = "Included in Hotel"; flag = true; }

                                if (!flag)
                                {
                                    m.DefaultPlan = "";
                                }
                            }

                             newtimeout = timeout;
                        }
                    }
                }
            }
            return m;
        }

        public async Task<MealSetRes> SetMealDetailsByID(MealSetReq request)
        {
            MealSetRes MealSetRes = new MealSetRes();
            UpdateResult resultFlag;
            var result = _MongoContext.mQuote.AsQueryable().Where(a => a.QRFID == request.QRFID).ToList();
            if (result != null && result.Count > 0)
            {
                var usernm = request.MealDetails.FirstOrDefault().EditUser;
                var resultposition = _MongoContext.mPosition.AsQueryable().Where(a => a.QRFID == request.QRFID).FirstOrDefault();

                if (resultposition != null)
                {
                    var resultmeal = resultposition;
                    if (resultmeal != null && resultmeal.Meals.Count > 0)
                    {
                        request.MealDetails.FindAll(f => !resultmeal.Meals.Exists(r => r.MealID == f.MealID)).ForEach(m =>
                        {
                            m.EditDate = DateTime.Now;
                            m.EditUser = usernm;
                            m.IsDeleted = true;
                            m.VenueTypes = resultmeal.Meals.Where(d => d.MealID == m.MealID).Select(d => d.VenueTypes).FirstOrDefault();
                        });

                        request.MealDetails.FindAll(f => resultmeal.Meals.Exists(r => r.MealID == f.MealID)).ForEach(m =>
                        {
                            m.EditDate = DateTime.Now;
                            m.EditUser = usernm;
                            m.IsDeleted = false;
                            m.VenueTypes = resultmeal.Meals.Where(d => d.MealID == m.MealID).Select(d => d.VenueTypes).FirstOrDefault();
                        });

                        request.MealDetails.Select(v => v.VenueTypes).FirstOrDefault().ForEach(m => m.VenueTypeName = (m.VenueTypeName == null ? "" : m.VenueTypeName));
                        resultFlag = await _MongoContext.mPosition.UpdateOneAsync(Builders<mPosition>.Filter.Eq("QRFID", request.QRFID),
                                    Builders<mPosition>.Update.Set("Meals", request.MealDetails));

                        MealSetRes.ResponseStatus.Status = resultFlag.MatchedCount > 0 ? "Success" : "Failure";
                        MealSetRes.ResponseStatus.ErrorMessage = resultFlag.MatchedCount > 0 ? "Meals Details Saved Successfully." : "Meals Details not updated.";
                    }
                }
                else
                {
                    QRFCounterRequest qrfCounterRequest = new QRFCounterRequest();
                    qrfCounterRequest.CounterType = _configuration["CounterType:Meals"].ToString();
                    mPosition objPosition = new mPosition();
                    request.MealDetails.ForEach(m =>
                    {
                        m.CreateDate = DateTime.Now; m.EditDate = null; m.EditUser = "";
                        m.MealID = m.MealID == 0 ? _genericRepository.GetNextReferenceNumber(qrfCounterRequest).LastReferenceNumber : m.MealID;
                    });
                    objPosition.QRFID = request.QRFID;
                    objPosition.Meals = request.MealDetails;
                    objPosition.Meals[0].CreateUser = request.MealDetails.FirstOrDefault().CreateUser;
                    objPosition.Meals[0].EditDate = null;
                    objPosition.Meals[0].EditUser = "";
                    await _MongoContext.mPosition.InsertOneAsync(objPosition);
                    MealSetRes.ResponseStatus.Status = "Success";
                    MealSetRes.ResponseStatus.ErrorMessage = "Meals Details Saved Successfully.";
                }
            }
            else
            {
                MealSetRes.ResponseStatus.ErrorMessage = "QRF ID not exist.";
            }
            return MealSetRes;
        }

        public async Task<MealVenueSetRes> SetMealVenueDetailsByID(MealVenueSetReq request)
        {
            MealVenueSetRes mealVenueSetRes = new MealVenueSetRes();
            mProductSupplier mProductSupplier = new mProductSupplier();

            mealVenueSetRes.MealID = request.MealID;
            mealVenueSetRes.QRFID = request.QRFID;
            var result = _MongoContext.mQuote.AsQueryable().Where(a => a.QRFID == request.QRFID).ToList();
            string msg = "";
            long venuedetailsid = request.VenueTypes.VenueDetails.VenueDetailsId;
            mealVenueSetRes.VenueTypes.VenueDetails.VenueDetailsId = venuedetailsid;
            if (result != null && result.Count > 0)
            {
                var resultmeal = _MongoContext.mPosition.AsQueryable().Where(a => a.QRFID == request.QRFID).FirstOrDefault();

                if (resultmeal != null)
                {
                    MealDetails objMealDetails = resultmeal.Meals.Find(m => m.MealID == request.MealID);
                    List<VenueTypes> lstVenueTypes = new List<VenueTypes>();

                    if (!(string.IsNullOrEmpty(request.VenueTypes.VenueDetails.MealTypeID)))
                    {
                        mProductSupplier = _MongoContext.mProductSupplier.AsQueryable().Where(p => p.Product_Id == request.VenueTypes.VenueDetails.MealTypeID && p.DafaultSupplier == true).FirstOrDefault();
                        request.VenueTypes.VenueDetails.SupplementID = mProductSupplier != null ? mProductSupplier.VoyagerProductSupplier_Id : "";
                    }

                    if (objMealDetails != null)
                    {
                        lstVenueTypes = objMealDetails.VenueTypes;
                        if (lstVenueTypes != null && lstVenueTypes.Count > 0)
                        {
                            if (lstVenueTypes.Where(v => v.VenueDetails.VenueDetailsId == venuedetailsid).FirstOrDefault() != null)
                            {
                                request.VenueTypes.VenueDetails.EditDate = DateTime.Now;
                                lstVenueTypes.Where(v => v.VenueDetails.VenueDetailsId == venuedetailsid).FirstOrDefault().VenueDetails = request.VenueTypes.VenueDetails;
                                lstVenueTypes.Where(v => v.VenueDetails.VenueDetailsId == venuedetailsid).FirstOrDefault().VenueTypeName = request.VenueTypes.VenueTypeName;
                                mealVenueSetRes.VenueTypes.VenueDetails.VenueDetailsId = venuedetailsid;
                                msg = "Details Saved Successfully.";
                            }
                            else
                            {
                                QRFCounterRequest qrfCounterRequest = new QRFCounterRequest();
                                qrfCounterRequest.CounterType = _configuration["CounterType:MealsVenue"].ToString();
                                request.VenueTypes.VenueDetails.VenueDetailsId = _genericRepository.GetNextReferenceNumber(qrfCounterRequest).LastReferenceNumber;
                                mealVenueSetRes.VenueTypes.VenueDetails.VenueDetailsId = request.VenueTypes.VenueDetails.VenueDetailsId;

                                request.VenueTypes.VenueDetails.CreateDate = DateTime.Now;
                                request.VenueTypes.VenueDetails.EditDate = null;
                                request.VenueTypes.VenueDetails.EditUser = "";

                                if (!(string.IsNullOrEmpty(request.VenueTypes.VenueDetails.MealTypeID)))
                                {
                                    mProductSupplier = _MongoContext.mProductSupplier.AsQueryable().Where(p => p.Product_Id == request.VenueTypes.VenueDetails.MealTypeID && p.DafaultSupplier == true).FirstOrDefault();
                                }
                                lstVenueTypes.Add(new VenueTypes { VenueDetails = request.VenueTypes.VenueDetails, VenueTypeName = request.VenueTypes.VenueTypeName });
                                msg = "Details Updated Successfully.";
                            }
                        }
                        else
                        {
                            QRFCounterRequest qrfCounterRequest = new QRFCounterRequest();

                            qrfCounterRequest.CounterType = _configuration["CounterType:MealsVenue"].ToString();
                            request.VenueTypes.VenueDetails.VenueDetailsId = _genericRepository.GetNextReferenceNumber(qrfCounterRequest).LastReferenceNumber;
                            mealVenueSetRes.VenueTypes.VenueDetails.VenueDetailsId = request.VenueTypes.VenueDetails.VenueDetailsId;

                            request.VenueTypes.VenueDetails.CreateDate = DateTime.Now;
                            request.VenueTypes.VenueDetails.EditDate = null;
                            request.VenueTypes.VenueDetails.EditUser = "";

                            lstVenueTypes.Add(new VenueTypes { VenueDetails = request.VenueTypes.VenueDetails, VenueTypeName = request.VenueTypes.VenueTypeName, });

                            msg = "Details Saved Successfully.";
                        }
                        await _MongoContext.mPosition.FindOneAndUpdateAsync(m => m.QRFID == request.QRFID && m.Meals.Any(md => md.MealID == request.MealID),
                                Builders<mPosition>.Update.Set(m => m.Meals[-1].VenueTypes, lstVenueTypes));
                    }
                    else
                    {
                        msg = "Meal ID not exist.";
                    }
                }
                else
                {
                    msg = "QRF ID not exist in Meals.";
                }
            }
            else
            {
                msg = "QRF ID not exist.";
            }
            mealVenueSetRes.ResponseStatus.Status = "Success";
            mealVenueSetRes.ResponseStatus.ErrorMessage = msg;
            return mealVenueSetRes;
        }

        public async Task<MealVenueGetRes> GetMealVenueDetailsByID(MealVenueGetReq request)
        {
            MealVenueGetRes MealVenueGetRes = new MealVenueGetRes();
            var result = await _MongoContext.mQuote.FindAsync(m => m.QRFID == request.QRFID);
            if (result != null && result.ToList().Count > 0)
            {
                var resultPosition = _MongoContext.mPosition.AsQueryable().Where(a => a.QRFID == request.QRFID).Select(a => a.Meals).FirstOrDefault();

                if (resultPosition != null)
                {
                    var VenueTypes = resultPosition.Where(m => m.MealID == request.MealID).Select(m => m.VenueTypes).FirstOrDefault();
                    VenueTypes objVenueTypes = (VenueTypes != null && VenueTypes.Count > 0) ? VenueTypes.Where(m => m.VenueDetails.VenueDetailsId == request.VenueDetailsId).FirstOrDefault() : new VenueTypes();
                    MealVenueGetRes.VenueDetailsId = request.VenueDetailsId;
                    MealVenueGetRes.VenueTypes = objVenueTypes;
                    MealVenueGetRes.MealID = request.MealID;
                    MealVenueGetRes.QRFID = request.QRFID;
                }
                else
                {
                    MealVenueGetRes.ResponseStatus.ErrorMessage = "No Data Found";
                }
            }
            else
            {
                MealVenueGetRes.ResponseStatus.ErrorMessage = "No QRFID Found";
            }
            MealVenueGetRes.ResponseStatus.Status = "Success";
            return MealVenueGetRes;
        }
    }
}
