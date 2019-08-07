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
    public class ActivitiesRepository : IActivitiesRepository
    {
        #region Private Variable Declaration
        private readonly MongoContext _MongoContext = null;
        private readonly IConfiguration _configuration;
        private readonly IGenericRepository _genericRepository;
        private readonly IQuoteRepository _quoteRepository;
        #endregion

        public ActivitiesRepository(IOptions<MongoSettings> settings, IGenericRepository genericRepository, IConfiguration configuration, IQuoteRepository quoteRepository)
        {
            _MongoContext = new MongoContext(settings);
            _configuration = configuration;
            _genericRepository = genericRepository;
            _quoteRepository = quoteRepository;
        }

        public async Task<ActivitiesGetRes> GetActivitiesDetailsByQRFID(QuoteGetReq request)
        {
            var response = new ActivitiesGetRes();
            try
            {
                List<ActivitiesProperties> objActivitiesProperties = new List<ActivitiesProperties>();
                RoutingGetReq req = new RoutingGetReq { QRFId = request.QRFID };
                var resultQuote = _MongoContext.mQuote.AsQueryable().Where(q => q.QRFID == request.QRFID);

                if (resultQuote != null && resultQuote.Count() > 0)
                {
                    var resultPosition = _MongoContext.mPosition.AsQueryable().Where(q => q.QRFID == request.QRFID).FirstOrDefault();
                    List<RoutingInfo> lstRoutingInfo = await _quoteRepository.GetQRFRouteDetailsByQRFID(req);

                    if (resultPosition != null)
                    {
                        var result = resultPosition.Activities;
                        if (result != null && result.ActivitiesDetails != null)
                        {
                            List<string> daysList = new List<string>();
                            int day = 0;
                            foreach (var item in lstRoutingInfo)
                            {
                                day = item.Days;
                                for (int i = 1; i <= day; i++)
                                    daysList.Add("Day " + (daysList.Count + 1));
                            }
                            response.DaysList = daysList;
                            response.ActivitiesDetails = result.ActivitiesDetails.Where(q => q.IsDeleted == false).ToList();

                            response.ResponseStatus.Status = "Success";
                        }
                        else
                        {
                            GetNewActivityDetails(lstRoutingInfo, resultQuote.FirstOrDefault(), ref response);
                        }
                    }
                    else
                    {
                        GetNewActivityDetails(lstRoutingInfo, resultQuote.FirstOrDefault(), ref response);
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
                response.ResponseStatus.ErrorMessage = "Error occured : " + ex.Message.ToString();
            }
            return response;
        }

        public void GetNewActivityDetails(List<RoutingInfo> lstRoutingInfo, mQuote resultQuote, ref ActivitiesGetRes response)
        {
            List<ActivitiesProperties> objActivitiesProperties = new List<ActivitiesProperties>();
            if (lstRoutingInfo != null && lstRoutingInfo.Count > 0)
            {
                List<string> daysList = new List<string>();
                int day = 0;
                foreach (var item in lstRoutingInfo)
                {
                    day = item.Days;
                    for (int i = 1; i <= day; i++)
                        daysList.Add("Day " + (daysList.Count + 1));
                }
                objActivitiesProperties.Add(new ActivitiesProperties
                {
                    CreateDate = DateTime.Now,
                    EditDate = null,
                    EditUser = "",
                    ActivityID = 0,
                    StartTime = "10:00",
                    NoOfPaxAdult = resultQuote.AgentPassengerInfo.Where(a => a.Type == "ADULT").Select(b => b.count).FirstOrDefault(),
                    NoOfPaxChild = resultQuote.AgentPassengerInfo.Where(a => a.Type == "CHILDWITHBED").Select(b => b.count).FirstOrDefault() + resultQuote.AgentPassengerInfo.Where(a => a.Type == "CHILDWITHOUTBED").Select(b => b.count).FirstOrDefault(),
                    NoOfPaxInfant = resultQuote.AgentPassengerInfo.Where(a => a.Type == "INFANT").Select(b => b.count).FirstOrDefault(),
                });
                response.DaysList = daysList;
                response.ResponseStatus.Status = "Success";
                response.ActivitiesDetails = objActivitiesProperties;
            }
            else
            {
                response.ResponseStatus.ErrorMessage = "No Routing Details found.";
                response.ResponseStatus.Status = "Failure";
            }
        }

        public async Task<ActivitiesSetRes> SetActivitiesDetails(ActivitiesSetReq request)
        {
            ActivitiesSetRes ActivitiesSetRes = new ActivitiesSetRes();
            UpdateResult resultFlag;
            try
            {

                var result = _MongoContext.mQuote.AsQueryable().Where(a => a.QRFID == request.QRFID).ToList();
                if (result != null && result.Count > 0)
                {
                    var resultPosition = _MongoContext.mPosition.AsQueryable().Where(a => a.QRFID == request.QRFID).FirstOrDefault();
                    
                    if (resultPosition != null)
                    {
                        QRFCounterRequest qrfCounterRequest = new QRFCounterRequest();
                        qrfCounterRequest.CounterType = _configuration["CounterType:Activities"].ToString();

                        var resultActivities = resultPosition.Activities;
                        if (resultActivities != null && resultActivities.ActivitiesDetails != null && resultActivities.ActivitiesDetails.Count > 0)
                        {
                            request.ActivitiesProperties.FindAll(f => !resultActivities.ActivitiesDetails.Exists(r => r.ActivityID == f.ActivityID)).ForEach(m =>
                            {
                                m.ActivityID = m.ActivityID == 0 ? _genericRepository.GetNextReferenceNumber(qrfCounterRequest).LastReferenceNumber : m.ActivityID;
                                m.CreateDate = DateTime.Now;
                                m.EditDate = null;
                            });

                            request.ActivitiesProperties.FindAll(f => resultActivities.ActivitiesDetails.Exists(r => r.ActivityID == f.ActivityID)).ForEach(m =>
                            {
                                m.EditDate = DateTime.Now;
                            });

                            if (request.SaveType.ToLower() == "complete")
                            {
                                resultFlag = await _MongoContext.mPosition.UpdateOneAsync(Builders<mPosition>.Filter.Eq("QRFID", request.QRFID),
                                           Builders<mPosition>.Update.Set("Activities.ActivitiesDetails", request.ActivitiesProperties));

                                ActivitiesSetRes.ResponseStatus.Status = resultFlag.MatchedCount > 0 ? "Success" : "Failure";
                                ActivitiesSetRes.ResponseStatus.ErrorMessage = resultFlag.MatchedCount > 0 ? "Saved Successfully." : "Activity Details not updated.";
                            }
                            else if (request.SaveType.ToLower() == "partial")
                            {
                                await _MongoContext.mPosition.FindOneAndUpdateAsync(m => m.QRFID == request.QRFID,
                                    Builders<mPosition>.Update.Set(m => m.Activities.ActivitiesDetails[-1], request.ActivitiesProperties[0]));

                                ActivitiesSetRes.ResponseStatus.Status = "Success";
                                ActivitiesSetRes.ResponseStatus.ErrorMessage = "Saved Successfully";
                            }
                        }
                        else
                        {
                            mPosition objPosition = new mPosition();
                            request.ActivitiesProperties.ForEach(m =>
                            {
                                m.CreateDate = DateTime.Now; m.EditDate = null; m.EditUser = "";
                                m.ActivityID = m.ActivityID == 0 ? _genericRepository.GetNextReferenceNumber(qrfCounterRequest).LastReferenceNumber : m.ActivityID;
                            });
                            objPosition.QRFID = request.QRFID;
                            objPosition.Activities.ActivitiesDetails = request.ActivitiesProperties;
                            objPosition.Activities.CreateDate = DateTime.Now;
                            objPosition.Activities.CreateUser = request.ActivitiesProperties.FirstOrDefault().CreateUser;
                            objPosition.Activities.EditDate = null;
                            objPosition.Activities.EditUser = "";

                            resultFlag = await _MongoContext.mPosition.UpdateOneAsync(Builders<mPosition>.Filter.Eq("QRFID", request.QRFID),
                                   Builders<mPosition>.Update.Set("Activities.ActivitiesDetails", request.ActivitiesProperties));

                            ActivitiesSetRes.ResponseStatus.Status = resultFlag.MatchedCount > 0 ? "Success" : "Failure";
                            ActivitiesSetRes.ResponseStatus.ErrorMessage = resultFlag.MatchedCount > 0 ? "Saved Successfully." : "Activity Details not updated.";
                        }
                    }
                    else //first time insert
                    {
                        QRFCounterRequest qrfCounterRequest = new QRFCounterRequest();
                        qrfCounterRequest.CounterType = _configuration["CounterType:Activities"].ToString();

                        mPosition objPosition = new mPosition();
                        request.ActivitiesProperties.ForEach(m =>
                        {
                            m.CreateDate = DateTime.Now; m.EditDate = null; m.EditUser = "";
                            m.ActivityID = m.ActivityID == 0 ? _genericRepository.GetNextReferenceNumber(qrfCounterRequest).LastReferenceNumber : m.ActivityID;
                        });
                        objPosition.QRFID = request.QRFID;
                        objPosition.Activities.ActivitiesDetails = request.ActivitiesProperties;
                        objPosition.Activities.CreateDate = DateTime.Now;
                        objPosition.Activities.CreateUser = request.ActivitiesProperties.FirstOrDefault().CreateUser;
                        objPosition.Activities.EditDate = null;
                        objPosition.Activities.EditUser = "";
                        await _MongoContext.mPosition.InsertOneAsync(objPosition);
                        ActivitiesSetRes.ResponseStatus.Status = "Success";
                    }

                    if (request.SaveType.ToLower() == "partial" && request.ActivitiesProperties.Count > 0) ActivitiesSetRes.ActivityId = request.ActivitiesProperties[0].ActivityID;
                }
                else
                {
                    ActivitiesSetRes.ResponseStatus.ErrorMessage = "QRF ID not exist.";
                }
            }
            catch (Exception ex)
            {
                ActivitiesSetRes.ResponseStatus.ErrorMessage = ex.StackTrace;
            }
            return ActivitiesSetRes;
        }
    }
}
