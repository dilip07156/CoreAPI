using Microsoft.AspNetCore.Hosting;
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
using VGER_WAPI.Providers;
using VGER_WAPI.Proxy;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Repositories
{
    public class HandoverRepository : IHandoverRepository
    {
        #region Private Variable Declaration  
        private readonly MongoContext _MongoContext = null;
        private readonly ICostsheetRepository _costsheetRepository;
        private readonly IUserRepository _userRepository;
        private readonly IGenericRepository _genericRepository;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration _configuration;
        private readonly IAgentApprovalRepository _agentApprovalRepository;
        private readonly IEmailRepository _emailRepository;
        private readonly IQuoteRepository _quoteRepository;
        private BridgePushDataProviders _bridgePushDataProviders = null;
        private ServiceProxy serviceProxy;
        #endregion

        public HandoverRepository(IConfiguration configuration, IOptions<MongoSettings> settings, IHostingEnvironment env,
            IGenericRepository genericRepository, ICostsheetRepository costsheetRepository, IUserRepository userRepository, IAgentApprovalRepository agentApprovalRepository,
            IEmailRepository emailRepository, IQuoteRepository quoteRepository)
        {
            _MongoContext = new MongoContext(settings);
            _costsheetRepository = costsheetRepository;
            _userRepository = userRepository;
            _genericRepository = genericRepository;
            _env = env;
            _configuration = configuration;
            _agentApprovalRepository = agentApprovalRepository;
            _emailRepository = emailRepository;
            _quoteRepository = quoteRepository;
            _bridgePushDataProviders = new BridgePushDataProviders(_configuration);
            serviceProxy = new ServiceProxy(_configuration);
        }

        #region  AttachToMaster        
        public async Task<GoAheadGetRes> GetGoAhead(GoAheadGetReq request)
        {
            GoAheadGetRes response = new GoAheadGetRes();
            List<CostsheetVersion> lstCostsheetVersion = new List<CostsheetVersion>();

            var resQRFPrice = _MongoContext.mQRFPrice.AsQueryable().Where(x => x.QRFID == request.QRFID && x.IsCurrentVersion == true).OrderByDescending(y => y.VersionId).FirstOrDefault();
            if (resQRFPrice != null)
            {
                var depatures = resQRFPrice.Departures.Where(a => a.IsDeleted == false).ToList();
                lstCostsheetVersion = _costsheetRepository.GetCostsheetVersions(new CostsheetGetReq { QRFID = request.QRFID });

                List<UserSystemContactDetails> lstUserSystemContactDetails = _userRepository.GetActiveUserSystemContactDetailsByRole("Groups");
                response.UserSystemContactDetails = lstUserSystemContactDetails;

                var resultGoAhead = await _MongoContext.mGoAhead.FindAsync(m => m.QRFID == request.QRFID && m.IsDeleted == false).Result.FirstOrDefaultAsync();

                if (resultGoAhead != null)
                {
                    if (lstCostsheetVersion.Where(a => a.QRFPriceId == resultGoAhead.QRFPriceId && a.VersionId == resultGoAhead.VersionId).FirstOrDefault() != null && !string.IsNullOrEmpty(resultGoAhead.QRFPriceId))
                    {
                        lstCostsheetVersion.ForEach(a => a.IsCurrentVersion = false);
                        lstCostsheetVersion.Where(a => a.QRFPriceId == resultGoAhead.QRFPriceId && a.VersionId == resultGoAhead.VersionId).FirstOrDefault().IsCurrentVersion = true;
                    }
                    response.mGoAhead = resultGoAhead;
                    //if date not exists then mark as delete
                    // response.mGoAhead.Depatures.FindAll(a => !depatures.Exists(b => b.Departure_Id == a.DepatureId)).ForEach(a => a.IsDeleted = true);

                    //if date is added then add it
                    var extradates = depatures.FindAll(a => !response.mGoAhead.Depatures.Exists(b => b.DepatureId == a.Departure_Id));
                    foreach (var item in extradates)
                    {
                        response.mGoAhead.Depatures.Add(new Depatures { ChildInfo = new List<ChildInfo>(), DepatureDate = item.Date, DepatureId = item.Departure_Id, PassengerRoomInfo = new List<PassengerRoomInfo>() });
                    }

                    response.mGoAhead.Depatures.FindAll(a => depatures.Exists(b => b.Departure_Id == a.DepatureId))
                        .ForEach(a => a.DepatureDate = depatures.Where(b => b.Departure_Id == a.DepatureId).FirstOrDefault().Date);

                    response.mGoAhead.Depatures = response.mGoAhead.Depatures.Where(a => a.IsDeleted == false).OrderBy(a => a.DepatureDate).ToList();
                }
                else
                {
                    response.mGoAhead = new mGoAhead();
                    response.mGoAhead.Depatures = depatures.Select(a => new Depatures
                    {
                        ChildInfo = new List<ChildInfo>(),
                        DepatureDate = a.Date,
                        DepatureId = a.Departure_Id,
                        IsCreate = false,
                        IsMaterialised = false,
                        PassengerRoomInfo = new List<PassengerRoomInfo>()

                    }).ToList();
                }
            }
            else
            {
                response.mGoAhead = new mGoAhead();
            }
            var qrfpriceids = lstCostsheetVersion.Select(a => a.QRFPriceId).ToList();
            var lstQRFPackagePrice = _MongoContext.mQRFPackagePrice.AsQueryable().Where(a => a.QRFID == request.QRFID && qrfpriceids.Contains(a.QRFPrice_Id) && (a.RoomName == "DOUBLE" || a.RoomName == "TWIN")).OrderBy(a => a.DepartureDate).ThenBy(a => a.PaxSlab).ToList();
            var twinlist = new List<mQRFPackagePrice>();
            var qrfpckgpriceilist = new List<mQRFPackagePrice>();
            foreach (var item in lstCostsheetVersion)
            {
                qrfpckgpriceilist = lstQRFPackagePrice.Where(b => b.QRFPrice_Id == item.QRFPriceId).ToList();
                twinlist = qrfpckgpriceilist.Where(a => a.RoomName.ToLower() == "twin").ToList();
                if (twinlist == null || twinlist.Count == 0)
                {
                    twinlist = qrfpckgpriceilist.Where(a => a.RoomName.ToLower() == "double").ToList();
                }
                if (twinlist != null && twinlist.Count > 0)
                {
                    item.QRFPackagePriceList = twinlist;
                    item.QRFPkgDepartureList = twinlist.Select(a => a.DepartureDate).Distinct().ToList();
                }
                else
                {
                    item.QRFPackagePriceList = new List<mQRFPackagePrice>();
                    item.QRFPkgDepartureList = new List<DateTime?>();
                }
            }
            response.CostsheetVersion = lstCostsheetVersion;
            return response;
        }

        public async Task<GetGoAheadDepatureRes> GetGoAheadDepature(GoAheadGetReq request)
        {
            GetGoAheadDepatureRes response = new GetGoAheadDepatureRes();
            var resQRFPrice = _MongoContext.mQRFPrice.AsQueryable().Where(x => x.QRFID == request.QRFID && x.IsCurrentVersion == true).OrderByDescending(y => y.VersionId).FirstOrDefault();

            if (resQRFPrice != null)
            {
                var resultGoAhead = await _MongoContext.mGoAhead.FindAsync(m => m.QRFID == request.QRFID && m.IsDeleted == false).Result.FirstOrDefaultAsync();
                var res = resultGoAhead.Depatures.Where(a => a.DepatureId == request.DepatureId).FirstOrDefault();
                resultGoAhead.Depatures = new List<Depatures>();
                resultGoAhead.Depatures.Add(res);

                List<PassengerRoomInfo> lstPassengerRoomInfo = new List<PassengerRoomInfo>();
                List<QRFAgentRoom> lstAgentRoom = new List<QRFAgentRoom>();

                List<ChildInfo> ChildInfo = new List<ChildInfo>();
                List<ChildInfo> ChildInfolst = new List<ChildInfo>();
                List<ChildInfo> ChildInfoChk = new List<ChildInfo>();

                bool childplusbed = false;
                bool childwithoutbed = false;
                bool infant = false;

                foreach (var item in resultGoAhead.Depatures)
                {
                    //if (item.IsMaterialised)
                    //{
                    lstPassengerRoomInfo = new List<PassengerRoomInfo>();
                    ChildInfo = new List<ChildInfo>();

                    //if new room type's are added in mQuote then add it
                    lstAgentRoom = resQRFPrice.QRFAgentRoom.FindAll(a => a.RoomCount > 0 && !item.PassengerRoomInfo.Select(b => b.RoomTypeID).ToList().Contains(a.RoomTypeID));
                    lstPassengerRoomInfo = lstAgentRoom.Select(a => new PassengerRoomInfo { RoomCount = a.RoomCount, RoomTypeID = a.RoomTypeID, RoomTypeName = a.RoomTypeName }).ToList();
                    item.PassengerRoomInfo.AddRange(lstPassengerRoomInfo);
                    item.PassengerRoomInfo = item.PassengerRoomInfo.Where(a => a.IsDeleted == false).ToList();

                    //if room type's are removed from mQuote then remove it
                    lstPassengerRoomInfo = item.PassengerRoomInfo.FindAll(a => !resQRFPrice.QRFAgentRoom.Select(b => b.RoomTypeID).ToList().Contains(a.RoomTypeID));
                    if (lstPassengerRoomInfo != null && lstPassengerRoomInfo.Count > 0)
                    {
                        item.PassengerRoomInfo.FindAll(a => lstPassengerRoomInfo.Select(b => b.RoomTypeID).ToList().Contains(a.RoomTypeID)).ForEach(a =>
                        {
                            a.DeleteDate = DateTime.Now;
                            a.IsDeleted = true; a.DeleteBy = resultGoAhead.CreateUser;
                        });
                    }

                    foreach (var child in resQRFPrice.AgentPassengerInfo)
                    {
                        if (child.Type.ToLower() != "adult")
                        {
                            if (child.Type.ToLower() == "infant" && child.count > 0)
                            {
                                infant = true;
                                ChildInfo.Add(new ChildInfo { Type = child.Type, Age = 1, Number = child.count });
                            }
                            else if (child.Age != null && child.Age.Count > 0)
                            {
                                foreach (var childage in child.Age)
                                {
                                    if (childage > 0)
                                    {
                                        if (child.Type.ToLower() == "childwithbed")
                                        {
                                            childplusbed = true;
                                        }
                                        if (child.Type.ToLower() == "childwithoutbed")
                                        {
                                            childwithoutbed = true;
                                        }
                                        ChildInfo.Add(new ChildInfo { Type = child.Type, Age = childage, Number = 1 });
                                    }
                                }
                            }
                        }
                    }

                    item.ChildInfo = item.ChildInfo.Where(a => a.IsDeleted == false).ToList();
                    //if new Age are added in mQuote then add it
                    ChildInfolst = ChildInfo.FindAll(a => !item.ChildInfo.Select(b => b.Age).ToList().Contains(a.Age));
                    if (ChildInfolst != null && ChildInfolst.Count > 0)
                    {
                        ChildInfolst.RemoveAll(a => a.Type.ToLower() == "infant");
                    }
                    var infantlst = ChildInfo.FindAll(a => a.Type.ToLower() == "infant" && !item.ChildInfo.Where(c => c.Type.ToLower() == "infant").Select(b => b.Number).ToList().Contains(a.Number));
                    ChildInfolst.AddRange(infantlst);
                    ChildInfolst.ForEach(a => a.ChildInfoId = Guid.NewGuid().ToString());
                    item.ChildInfo.AddRange(ChildInfolst);

                    item.PassengerRoomInfo = item.PassengerRoomInfo.Where(a => a.IsDeleted == false).ToList();
                    item.PassengerRoomInfo.ForEach(a =>
                    {
                        a.PaxCount = (a.PaxCount == null || a.PaxCount == 0) ? ((a.RoomTypeName.ToLower() == "single" ? 1 : a.RoomTypeName.ToLower() == "double" ? 2 :
                                        a.RoomTypeName.ToLower() == "triple" ? 3 : a.RoomTypeName.ToLower() == "quad" ? 4 :
                                        a.RoomTypeName.ToLower() == "twin" ? 2 : a.RoomTypeName.ToLower() == "tsu" ? 1 : 1) * (a.RoomCount)) : a.PaxCount;
                    });
                    item.ChildInfo = item.ChildInfo.Where(a => a.IsDeleted == false).ToList();
                    //}
                    //else
                    //{
                    //    item.ChildInfo.ForEach(a => { a.DeleteDate = DateTime.Now; a.DeleteBy = resultGoAhead.CreateUser; a.IsDeleted = true; });
                    //    item.PassengerRoomInfo.ForEach(a => { a.DeleteDate = DateTime.Now; a.DeleteBy = resultGoAhead.CreateUser; a.IsDeleted = true; });

                    //    item.PassengerRoomInfo = item.PassengerRoomInfo.Where(a => a.IsDeleted == false).ToList();
                    //    item.ChildInfo = item.ChildInfo.Where(a => a.IsDeleted == false).ToList();
                    //}

                    item.PassengerRoomInfo = item.PassengerRoomInfo.OrderBy(a => a.RoomTypeName.ToLower() == "single" ? "A" : a.RoomTypeName.ToLower() == "double" ? "B" :
                       a.RoomTypeName.ToLower() == "twin" ? "C" : a.RoomTypeName.ToLower() == "triple" ? "D" : a.RoomTypeName.ToLower() == "quad" ? "E" :
                       a.RoomTypeName.ToLower() == "tsu" ? "F" : "G").ToList();
                }

                List<AttributeValues> ChildTypeList = new List<AttributeValues>();
                ChildTypeList.Add(new AttributeValues { AttributeValue_Id = "CHILDWITHBED", Value = "CHILD + BED", Flag = childplusbed });
                ChildTypeList.Add(new AttributeValues { AttributeValue_Id = "CHILDWITHOUTBED", Value = "CHILD - BED", Flag = childwithoutbed });
                ChildTypeList.Add(new AttributeValues { AttributeValue_Id = "INFANT", Value = "INFANT", Flag = infant });
                response.ChildTypeList = ChildTypeList;

                response.mGoAhead = resultGoAhead;
            }
            else
            {
                response.mGoAhead = new mGoAhead();
            }

            return response;
        }

        public async Task<GoAheadSetRes> SetGoAhead(GoAheadSetReq request)
        {
            GoAheadSetRes response = new GoAheadSetRes();
            List<CostsheetVersion> lstCostsheetVersion = new List<CostsheetVersion>();

            var resQRFPrice = _MongoContext.mQRFPrice.AsQueryable().Where(x => x.QRFID == request.mGoAhead.QRFID && x.IsCurrentVersion == true).OrderByDescending(y => y.VersionId).FirstOrDefault();
            if (resQRFPrice != null)
            {
                var resultGoAhead = await _MongoContext.mGoAhead.FindAsync(m => m.QRFID == request.mGoAhead.QRFID && m.IsDeleted == false).Result.FirstOrDefaultAsync();
                if (resultGoAhead != null)
                {
                    var objDepatures = new Depatures();
                    List<PassengerRoomInfo> lstPassengerRoomInfo = new List<PassengerRoomInfo>();
                    List<QRFAgentRoom> lstAgentRoom = new List<QRFAgentRoom>();

                    List<ChildInfo> ChildInfo = new List<ChildInfo>();
                    List<ChildInfo> ChildInfolst = new List<ChildInfo>();
                    List<ChildInfo> ChildInfoChk = new List<ChildInfo>();
                    var departure = new Depatures();

                    request.mGoAhead._Id = resultGoAhead._Id;
                    request.mGoAhead.EditDate = DateTime.Now;

                    foreach (var item in request.mGoAhead.Depatures)
                    {
                        objDepatures = new Depatures();
                        ChildInfo = new List<ChildInfo>();
                        lstPassengerRoomInfo = new List<PassengerRoomInfo>();

                        objDepatures = resultGoAhead.Depatures.Where(a => a.DepatureId == item.DepatureId).FirstOrDefault();
                        if (objDepatures != null && (!string.IsNullOrEmpty(objDepatures.ConfirmMessage) || !string.IsNullOrEmpty(objDepatures.OpsBookingNumber)))
                        {
                            item.Confirmed_Date = objDepatures.Confirmed_Date;
                            item.Confirmed_User = objDepatures.Confirmed_User;
                            item.ConfirmMessage = objDepatures.ConfirmMessage;
                            item.ConfirmStatus = objDepatures.ConfirmStatus;
                            item.IsCreate = objDepatures.IsCreate;
                            item.IsMaterialised = objDepatures.IsMaterialised;
                            item.OpsBookingNumber = objDepatures.OpsBookingNumber;
                            item.IsDeleted = objDepatures.IsDeleted;
                            item.ChildInfo = objDepatures.ChildInfo;
                            item.DepatureDate = objDepatures.DepatureDate;
                            item.PassengerRoomInfo = objDepatures.PassengerRoomInfo;
                        }
                        else
                        {
                            if (item.IsMaterialised)
                            {
                                if (resultGoAhead.Depatures != null && resultGoAhead.Depatures.Count > 0)
                                {
                                    ChildInfoChk = resultGoAhead.Depatures.Where(a => a.DepatureId == item.DepatureId).Select(a => a.ChildInfo).FirstOrDefault();
                                    item.ChildInfo = ChildInfoChk != null ? resultGoAhead.Depatures.Where(a => a.DepatureId == item.DepatureId).Select(a => a.ChildInfo).FirstOrDefault().Select(a => new ChildInfo
                                    {
                                        Age = a.Age,
                                        ChildInfoId = a.ChildInfoId,
                                        Number = a.Number,
                                        Type = a.Type,
                                        IsDeleted = a.IsDeleted,
                                        DeleteBy = a.DeleteBy,
                                        DeleteDate = a.DeleteDate
                                    }).ToList() : new List<VGER_WAPI_CLASSES.ChildInfo>();
                                }
                                departure = resultGoAhead.Depatures.Where(a => a.DepatureId == item.DepatureId && a.IsDeleted == false).FirstOrDefault();
                                if (departure != null)
                                {
                                    item.PassengerRoomInfo = departure.PassengerRoomInfo;
                                    item.PassengerRoomInfo = item.PassengerRoomInfo.Where(a => a.IsDeleted == false).ToList();

                                    //if new room type's are added in mQuote then add it
                                    lstAgentRoom = resQRFPrice.QRFAgentRoom.FindAll(a => a.RoomCount > 0 && !item.PassengerRoomInfo.Select(b => b.RoomTypeID).ToList().Contains(a.RoomTypeID));
                                    lstPassengerRoomInfo = lstAgentRoom.Select(a => new PassengerRoomInfo { RoomCount = a.RoomCount, RoomTypeID = a.RoomTypeID, RoomTypeName = a.RoomTypeName }).ToList();
                                    item.PassengerRoomInfo.AddRange(lstPassengerRoomInfo);

                                    //if room type's are removed from mQuote then remove it
                                    lstPassengerRoomInfo = item.PassengerRoomInfo.FindAll(a => !resQRFPrice.QRFAgentRoom.Select(b => b.RoomTypeID).ToList().Contains(a.RoomTypeID));
                                    if (lstPassengerRoomInfo != null && lstPassengerRoomInfo.Count > 0)
                                    {
                                        item.PassengerRoomInfo.FindAll(a => lstPassengerRoomInfo.Select(b => b.RoomTypeID).ToList().Contains(a.RoomTypeID)).ForEach(a =>
                                        {
                                            a.DeleteDate = DateTime.Now;
                                            a.IsDeleted = true; a.DeleteBy = request.mGoAhead.CreateUser;
                                        });
                                    }
                                }

                                foreach (var child in resQRFPrice.AgentPassengerInfo)
                                {
                                    if (child.Type.ToLower() != "adult")
                                    {
                                        if (child.Type.ToLower() == "infant" && child.count > 0)
                                        {
                                            ChildInfo.Add(new ChildInfo { Type = child.Type, Age = 1, Number = child.count });
                                        }
                                        else if (child.Age != null && child.Age.Count > 0)
                                        {
                                            foreach (var childage in child.Age)
                                            {
                                                if (childage > 0)
                                                {
                                                    ChildInfo.Add(new ChildInfo { Type = child.Type, Age = childage, Number = 1 });
                                                }
                                            }
                                        }
                                    }
                                }
                                item.ChildInfo = item.ChildInfo.Where(a => a.IsDeleted == false).ToList();
                                //if new Age are added in mQuote then add it
                                ChildInfolst = ChildInfo.FindAll(a => !item.ChildInfo.Select(b => b.Age).ToList().Contains(a.Age));
                                if (ChildInfolst != null && ChildInfolst.Count > 0)
                                {
                                    ChildInfolst.RemoveAll(a => a.Type.ToLower() == "infant");
                                }
                                var infantlst = ChildInfo.FindAll(a => a.Type.ToLower() == "infant" && !item.ChildInfo.Where(c => c.Type.ToLower() == "infant").Select(b => b.Number).ToList().Contains(a.Number));
                                ChildInfolst.AddRange(infantlst);
                                ChildInfolst.ForEach(a => a.ChildInfoId = Guid.NewGuid().ToString());
                                item.ChildInfo.AddRange(ChildInfolst);
                            }
                            else
                            {
                                item.PassengerRoomInfo.ForEach(a => { a.DeleteDate = DateTime.Now; a.DeleteBy = request.mGoAhead.CreateUser; a.IsDeleted = true; });
                                item.ChildInfo.ForEach(a => { a.DeleteDate = DateTime.Now; a.DeleteBy = request.mGoAhead.CreateUser; a.IsDeleted = true; });
                            }
                            item.PassengerRoomInfo = item.PassengerRoomInfo.OrderBy(a => a.RoomTypeName.ToLower() == "single" ? "A" : a.RoomTypeName.ToLower() == "double" ? "B" :
                                 a.RoomTypeName.ToLower() == "twin" ? "C" : a.RoomTypeName.ToLower() == "triple" ? "D" : a.RoomTypeName.ToLower() == "quad" ? "E" :
                                 a.RoomTypeName.ToLower() == "tsu" ? "F" : "G").ToList();
                        }
                    }

                    request.mGoAhead.Depatures.AddRange(resultGoAhead.Depatures.Where(a => a.IsDeleted == true));

                    request.mGoAhead.Depatures.FindAll(a => resQRFPrice.Departures.Exists(b => b.Departure_Id == a.DepatureId))
                        .ForEach(a => a.DepatureDate = resQRFPrice.Departures.Where(b => b.Departure_Id == a.DepatureId).FirstOrDefault().Date);
                    if (request.mGoAhead.Depatures.Where(a => a.DepatureId == request.DepatureId).FirstOrDefault() != null)
                    {
                        request.mGoAhead.Depatures.Where(a => a.DepatureId == request.DepatureId).FirstOrDefault().IsMaterialised = false;
                    }

                    ReplaceOneResult replaceResult = await _MongoContext.mGoAhead.ReplaceOneAsync(Builders<mGoAhead>.Filter.Eq("QRFID", request.mGoAhead.QRFID), request.mGoAhead);

                    response.ResponseStatus.Status = replaceResult.MatchedCount > 0 ? "Success" : "Failure";
                    response.ResponseStatus.ErrorMessage = replaceResult.MatchedCount > 0 ? "Saved Successfully." : "Details not updated.";
                }
                else
                {
                    request.mGoAhead.GoAheadId = Guid.NewGuid().ToString();
                    request.mGoAhead.EditUser = "";
                    request.mGoAhead.Depatures.FindAll(a => resQRFPrice.Departures.Exists(b => b.Departure_Id == a.DepatureId))
                         .ForEach(a => a.DepatureDate = resQRFPrice.Departures.Where(b => b.Departure_Id == a.DepatureId).FirstOrDefault().Date);

                    if (request.mGoAhead.Depatures.Where(a => a.DepatureId == request.DepatureId).FirstOrDefault() != null)
                    {
                        request.mGoAhead.Depatures.Where(a => a.DepatureId == request.DepatureId).FirstOrDefault().IsMaterialised = false;
                    }

                    await _MongoContext.mGoAhead.InsertOneAsync(request.mGoAhead);

                    response.ResponseStatus.ErrorMessage = "Details Saved Successfully.";
                    response.ResponseStatus.Status = "Success";
                }

                if (response.ResponseStatus.Status == "Success")
                {
                    if (request.mGoAhead.IsQRF)
                    {
                        await _agentApprovalRepository.AmendmentQuote(new AmendmentQuoteReq { QRFID = request.mGoAhead.QRFID, EditUser = request.mGoAhead.EditUser });
                    }
                }
            }
            else
            {
                response.ResponseStatus.ErrorMessage = "QRFID not exists.";
                response.ResponseStatus.Status = "Error";
            }
            response.mGoAhead = request.mGoAhead;
            return response;
        }

        public async Task<SetMaterialisationRes> SetMaterialisation(SetMaterialisationReq request)
        {
            SetMaterialisationRes response = new SetMaterialisationRes();

            var resQRFPrice = _MongoContext.mQRFPrice.AsQueryable().Where(x => x.QRFID == request.mGoAhead.QRFID && x.IsCurrentVersion == true).OrderByDescending(y => y.VersionId).FirstOrDefault();
            if (resQRFPrice != null)
            {
                var resultGoAhead = await _MongoContext.mGoAhead.FindAsync(m => m.QRFID == request.mGoAhead.QRFID && m.GoAheadId == request.mGoAhead.GoAheadId && m.IsDeleted == false).Result.FirstOrDefaultAsync();
                if (resultGoAhead != null)
                {
                    var departures = resultGoAhead.Depatures.Where(a => a.DepatureId == request.mGoAhead.Depatures[0].DepatureId).FirstOrDefault();
                    if (departures != null)
                    {
                        resultGoAhead.Depatures.Where(a => a.DepatureId == request.mGoAhead.Depatures[0].DepatureId).FirstOrDefault().PassengerRoomInfo = request.mGoAhead.Depatures[0].PassengerRoomInfo;
                        request.mGoAhead.Depatures[0].ChildInfo.ForEach(a => a.ChildInfoId = (string.IsNullOrEmpty(a.ChildInfoId) || a.ChildInfoId == "0") ? Guid.NewGuid().ToString() : a.ChildInfoId);
                        resultGoAhead.Depatures.Where(a => a.DepatureId == request.mGoAhead.Depatures[0].DepatureId).FirstOrDefault().ChildInfo = request.mGoAhead.Depatures[0].ChildInfo;
                        resultGoAhead.Depatures.Where(a => a.DepatureId == request.mGoAhead.Depatures[0].DepatureId).FirstOrDefault().IsMaterialised = true;
                        resultGoAhead.EditDate = DateTime.Now;
                        resultGoAhead.EditUser = request.mGoAhead.EditUser;

                        resultGoAhead.Depatures.FindAll(a => resQRFPrice.Departures.Exists(b => b.Departure_Id == a.DepatureId))
                       .ForEach(a => a.DepatureDate = resQRFPrice.Departures.Where(b => b.Departure_Id == a.DepatureId).FirstOrDefault().Date);

                        ReplaceOneResult replaceResult = await _MongoContext.mGoAhead.ReplaceOneAsync(Builders<mGoAhead>.Filter.Eq("QRFID", request.mGoAhead.QRFID), resultGoAhead);
                        response.ResponseStatus.Status = replaceResult.MatchedCount > 0 ? "Success" : "Failure";
                        response.ResponseStatus.ErrorMessage = replaceResult.MatchedCount > 0 ? "Saved Successfully." : "Details not updated.";
                    }
                    else
                    {
                        response.ResponseStatus.ErrorMessage = "Depatures Details not exists.";
                        response.ResponseStatus.Status = "Error";
                    }
                }
                else
                {
                    response.ResponseStatus.ErrorMessage = "mGoAhead Details not exists.";
                    response.ResponseStatus.Status = "Error";
                }
            }
            else
            {
                response.ResponseStatus.ErrorMessage = "QRFID not exists.";
                response.ResponseStatus.Status = "Error";
            }
            response.mGoAhead = request.mGoAhead;

            return response;
        }
        #endregion

        #region Handover
        //below function used for to update the ConfirmMessage column for the given DepartureIds
        public async Task<ConfirmBookingSetRes> SetGoAheadConfirmMessage(ConfirmBookingSetReq request)
        {
            ConfirmBookingSetRes response = new ConfirmBookingSetRes() { QRFID = request.QRFID, ResponseStatus = new ResponseStatus() };
            try
            {
                var resQRFPrice = _MongoContext.mQRFPrice.AsQueryable().Where(x => x.QRFID == request.QRFID && x.IsCurrentVersion == true).OrderByDescending(y => y.VersionId).FirstOrDefault();

                if (resQRFPrice != null)
                {
                    var resultGoAhead = _MongoContext.mGoAhead.Find(m => m.QRFID == request.QRFID && m.IsDeleted == false).FirstOrDefault();
                    if (resultGoAhead != null)
                    {
                        if (request?.DepatureId?.Count > 0)
                        {
                            var deptids = resultGoAhead.Depatures.Where(a => !string.IsNullOrEmpty(a.ConfirmMessage) && a.ConfirmMessage.ToLower() != "success" && request.DepatureId.Contains(a.DepatureId)).
                                          Select(a => a.DepatureId).ToList();
                            if (deptids?.Count > 0)
                            {
                                mGoAhead mGoAheadResult = new mGoAhead();
                                foreach (var item in deptids)
                                {
                                    mGoAheadResult = await _MongoContext.mGoAhead.FindOneAndUpdateAsync(a => a.QRFID == request.QRFID
                                                                                        && a.Depatures.Any(b => b.DepatureId == item),
                                                                                        Builders<mGoAhead>.Update.Set(c => c.Depatures[-1].ConfirmMessage, ""));
                                }
                                response.ResponseStatus.Status = "Success";
                            }
                            else
                            {
                                response.ResponseStatus.Status = "Success";
                            }
                        }
                        else
                        {
                            response.ResponseStatus.ErrorMessage = "DepatureId can not be null/blank.";
                            response.ResponseStatus.Status = "Error";
                        }
                    }
                    else
                    {
                        response.ResponseStatus.ErrorMessage = "mGoAhead details not exists.";
                        response.ResponseStatus.Status = "Error";
                    }
                }
                else
                {
                    response.ResponseStatus.ErrorMessage = "QRFID not exists.";
                    response.ResponseStatus.Status = "Error";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.ErrorMessage = ex.Message;
                response.ResponseStatus.Status = "Error";
            }
            return response;
        }

        //the function used for to to generate the Booking No and send the BookingNo, DepartureId to GoAheadQuotes MPUSH function
        public async Task<ConfirmBookingGetRes> GoAheadQuotes(ConfirmBookingGetReq request)
        {
            ConfirmBookingGetRes response = new ConfirmBookingGetRes() { QRFID = request.QRFID, ResponseStatus = new ResponseStatus() };
            PushBookingsSetReq pushBookingsSetReq = new PushBookingsSetReq();
            try
            {
                var resQRFPrice = _MongoContext.mQRFPrice.AsQueryable().Where(x => x.QRFID == request.QRFID && x.IsCurrentVersion == true).OrderByDescending(y => y.VersionId).FirstOrDefault();

                if (resQRFPrice != null)
                {
                    var resultGoAhead = _MongoContext.mGoAhead.Find(m => m.QRFID == request.QRFID && m.IsDeleted == false).FirstOrDefault();
                    if (resultGoAhead != null)
                    {
                        if (resultGoAhead.Depatures != null && resultGoAhead.Depatures.Count > 0)
                        {
                            var departureslist = resultGoAhead.Depatures.Where(a => a.IsDeleted == false && request.DepatureId.Contains(a.DepatureId) &&
                            ((a.IsCreate == true && a.IsMaterialised == true && a.ConfirmMessage != "Success") || (a.IsCreate == false && !string.IsNullOrEmpty(a.ConfirmMessage) &&
                            a.ConfirmMessage.StartsWith("Failure")))).ToList();

                            if (departureslist != null && departureslist.Count > 0)
                            {
                                #region Add Followup
                                request.UserName = request.UserName.ToLower().Trim();
                                var CompanyList = _MongoContext.mCompanies.AsQueryable();
                                var FromUserContacts = CompanyList.Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a => !string.IsNullOrEmpty(a.MAIL) && a.MAIL.ToLower() == request.UserName)).FirstOrDefault()?.ContactDetails;
                                var FromUser = FromUserContacts?.Where(a => a.MAIL.ToLower() == request.UserName).FirstOrDefault();

                                FollowUpSetReq followUprequest = new FollowUpSetReq();
                                followUprequest.QRFID = request.QRFID;

                                FollowUpTask task = new FollowUpTask();
                                task.Task = "Confirm Quote";
                                task.FollowUpType = "Internal";
                                task.FollowUpDateTime = DateTime.Now;

                                task.FromEmail = request.UserName;
                                if (FromUser != null)
                                {
                                    task.FromName = FromUser.CommonTitle + " " + FromUser.FIRSTNAME + " " + FromUser.LastNAME;
                                    task.FromContact_Id = FromUser.Contact_Id;
                                }

                                task.ToEmail = request.UserName;
                                if (FromUser != null)
                                {
                                    task.ToName = FromUser.CommonTitle + " " + FromUser.FIRSTNAME + " " + FromUser.LastNAME;
                                    task.ToContact_Id = FromUser.Contact_Id;
                                }

                                task.Status = "Confirmed";
                                task.Notes = "Confirm Quote";

                                var FollowUpTaskList = new List<FollowUpTask>();
                                FollowUpTaskList.Add(task);

                                followUprequest.FollowUp.Add(new FollowUp
                                {
                                    FollowUp_Id = Guid.NewGuid().ToString(),
                                    FollowUpTask = FollowUpTaskList,
                                    CreateUser = request.UserName,
                                    CreateDate = DateTime.Now
                                });
                                await _quoteRepository.SetFollowUpForQRF(followUprequest);
                                #endregion

                                var resBookingNum = new GetCompany_RS();
                                PushBookingsSetRes objPushBookingsSetRes = new PushBookingsSetRes();
                                foreach (var item in departureslist)
                                {
                                    resBookingNum = new GetCompany_RS();
                                    // Generating next BookingNumber (Calling directly SQL table instead of Mongo Objet to avoid duplicate generation of BookingNumbers due to timelag in MongoPush.                                
                                    resBookingNum = await _bridgePushDataProviders.GetLatestSQLReferenceNumber(new GetCompany_RQ { Type = "GROUP" });
                                    if (resBookingNum?.ResponseStatus?.Status.ToLower() == "success" && resBookingNum.ReferenceNumber > 0)
                                    {
                                        pushBookingsSetReq = new PushBookingsSetReq() { BookingNumber = resBookingNum.ReferenceNumber.ToString(), QRFID = request.QRFID, DepartureId = item.DepatureId.ToString(), UserName = request.UserName, GoAheadId = resultGoAhead.GoAheadId };

                                        //below code is synchronous
                                        objPushBookingsSetRes = new PushBookingsSetRes();
                                        objPushBookingsSetRes = await serviceProxy.PostData(_configuration.GetValue<string>("Handover:GoAheadQuotes"), pushBookingsSetReq, typeof(PushBookingsSetRes));

                                        //below code is concurrent synchronous (parallel threading)
                                        //ServiceProxy.ServiceCall(_configuration.GetValue<string>("Handover:GoAheadQuotes"), pushBookingsSetReq, _configuration.GetValue<string>("MongoPushUrl"));
                                    }
                                }
                                response.ResponseStatus.Status = "Success";
                            }
                            else
                            {
                                response.ResponseStatus.ErrorMessage = "mGoAhead Departure details not exists.";
                                response.ResponseStatus.Status = "Error";
                            }
                        }
                        else
                        {
                            response.ResponseStatus.ErrorMessage = "mGoAhead Departure details not exists.";
                            response.ResponseStatus.Status = "Error";
                        }
                    }
                    else
                    {
                        response.ResponseStatus.ErrorMessage = "mGoAhead details not exists.";
                        response.ResponseStatus.Status = "Error";
                    }
                }
                else
                {
                    response.ResponseStatus.ErrorMessage = "QRFID not exists.";
                    response.ResponseStatus.Status = "Error";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.ErrorMessage = ex.Message;
                response.ResponseStatus.Status = "Error";
            }
            return response;
        }

        //GetGoAheadDeparturesDetails function will fetch the active departure details
        public async Task<HandoverGetRes> GetGoAheadDeparturesDetails(GoAheadGetReq request)
        {
            var response = new HandoverGetRes() { QRFID = request.QRFID, ResponseStatus = new ResponseStatus() };
            var resQRFPrice = _MongoContext.mQRFPrice.AsQueryable().Where(x => x.QRFID == request.QRFID && x.IsCurrentVersion == true).OrderByDescending(y => y.VersionId).FirstOrDefault();

            if (resQRFPrice != null)
            {
                var resultGoAhead = await _MongoContext.mGoAhead.FindAsync(m => m.QRFID == request.QRFID && m.IsDeleted == false).Result.FirstOrDefaultAsync();
                if (resultGoAhead != null)
                {
                    response.Depatures = resultGoAhead.Depatures.Where(a => a.IsDeleted == false && ((a.IsCreate == true && a.IsMaterialised == true)
                    || (a.IsCreate == false && !string.IsNullOrEmpty(a.ConfirmMessage) && !string.IsNullOrEmpty(a.ConfirmMessage) && a.ConfirmMessage.StartsWith("Failure")))).ToList();

                    response.Depatures.FindAll(a => resQRFPrice.Departures.Exists(b => b.Departure_Id == a.DepatureId))
                       .ForEach(a => a.DepatureDate = resQRFPrice.Departures.Where(b => b.Departure_Id == a.DepatureId).FirstOrDefault().Date);

                    response.Depatures = response.Depatures.OrderBy(a => a.DepatureDate).ToList();
                    response.ResponseStatus.Status = "Success";
                }
                else
                {
                    response.ResponseStatus.ErrorMessage = "QRFID not exists in mGoAhead.";
                    response.ResponseStatus.Status = "Error";
                }
            }
            else
            {
                response.ResponseStatus.ErrorMessage = "QRFID not exists.";
                response.ResponseStatus.Status = "Error";
            }
            return response;
        }
        #endregion

        #region Add New Departures
        //GetGoAheadExistDepartures function will fetch the existing detaprtures
        public async Task<GoAheadNewDeptGetRes> GetGoAheadExistDepartures(GoAheadGetReq request)
        {
            double SellPrice = 0;
            string Currency = "";
            bool flag = false;

            var response = new GoAheadNewDeptGetRes()
            {
                QRFID = request.QRFID,
                GoAheadId = request.GoAheadId,
                ExisitingDepatures = new List<ExisitingDepatures>(),
                NewDepatures = new List<NewDepatures>(),
                ResponseStatus = new ResponseStatus()
            };
            try
            {
                var resultGoAhead = await _MongoContext.mGoAhead.FindAsync(m => m.QRFID == request.QRFID && m.GoAheadId == request.GoAheadId && m.IsDeleted == false).Result.FirstOrDefaultAsync();
                if (resultGoAhead != null)
                {
                    var deptDates = resultGoAhead.Depatures.Where(a => a.IsDeleted == false).ToList();

                    var resQRFPackagePrice = await _MongoContext.mQRFPackagePrice.FindAsync(m => m.QRFID == request.QRFID && m.QRFPrice_Id == resultGoAhead.QRFPriceId).Result.ToListAsync();

                    var resQRFPackagePriceDeparture = new List<mQRFPackagePrice>();
                    if (resQRFPackagePrice?.Count > 0)
                    {
                        response.ExisitingDepatures = deptDates.Select(a => new ExisitingDepatures { DepatureDate = a.DepatureDate, DepatureId = a.DepatureId, PPTwin = 0 }).OrderBy(a => a.DepatureDate).ToList();

                        //calculate the Price for each Departures
                        foreach (var itemDeptDates in deptDates)
                        {
                            SellPrice = 0;
                            Currency = "";
                            resQRFPackagePriceDeparture = new List<mQRFPackagePrice>();

                            var dept = resultGoAhead.Depatures.Where(a => a.DepatureId == itemDeptDates.DepatureId).FirstOrDefault();

                            //if departure has PassengerRoomInfo then calculate price by RoomTypeName
                            if (dept?.PassengerRoomInfo?.Count > 0)
                            {
                                //calculate RoomCount
                                int? totalRoomCnt = 0;
                                string[] paxslab = new string[] { };

                                foreach (var item in dept.PassengerRoomInfo)
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

                                resQRFPackagePrice = resQRFPackagePrice.OrderBy(a => a.RoomName.ToLower() == "twin" ? "A" :
                                                                                     a.RoomName.ToLower() == "double" ? "B" :
                                                                                     a.RoomName.ToLower() == "triple" ? "C" :
                                                                                     a.RoomName.ToLower() == "single" ? "D" :
                                                                                     a.RoomName.ToLower() == "quad" ? "E" :
                                                                                     a.RoomName.ToLower() == "tsu" ? "F" : "G").ToList();

                                resQRFPackagePriceDeparture = resQRFPackagePrice.Where(a => a.Departure_Id == itemDeptDates.DepatureId).ToList();
                                if (resQRFPackagePriceDeparture?.Count > 0)
                                {
                                    foreach (var itemQRFPkgPr in resQRFPackagePriceDeparture)
                                    {
                                        paxslab = itemQRFPkgPr.PaxSlab.Split('-');
                                        //check if totalRoomCnt falls in PaxSlab Range then take it by following RoomName order
                                        if (paxslab.Length > 0 && totalRoomCnt >= Convert.ToInt32(paxslab[0]) && totalRoomCnt <= Convert.ToInt32(paxslab[1]))
                                        {
                                            if (!string.IsNullOrEmpty(itemQRFPkgPr.RoomName))
                                            {
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
                                }

                                flag = (SellPrice == 0 || string.IsNullOrEmpty(Currency)) ? true : false;
                            }
                            else
                            {
                                flag = true;
                            }

                            //if departure doesn't have PassengerRoomInfo then take 1st SellPrice of mQRFPackagePrice
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

                        response.ResponseStatus.Status = "Success";
                    }
                    else
                    {
                        response.ResponseStatus.ErrorMessage = "DepatureId " + request.DepatureId.ToString() + " not exists in mQRFPackagePrice.";
                        response.ResponseStatus.Status = "Error";
                    }
                }
                else
                {
                    response.ResponseStatus.ErrorMessage = "QRFID not exists in mGoAhead.";
                    response.ResponseStatus.Status = "Error";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.ErrorMessage = ex.Message;
                response.ResponseStatus.Status = "Error";
            }

            return response;
        }

        public async Task<GoAheadNewDeptSetRes> SetGoAheadNewDepartures(GoAheadNewDeptSetReq request)
        {
            var response = new GoAheadNewDeptSetRes()
            {
                QRFID = request.QRFID,
                GoAheadId = request.GoAheadId,
                ResponseStatus = new ResponseStatus()
            };
            try
            {
                if (request?.NewDepatures?.Count > 0)
                {
                    if (request?.ExisitingDepatures != null)
                    {
                        var resultGoAhead = await _MongoContext.mGoAhead.FindAsync(m => m.QRFID == request.QRFID && m.GoAheadId == request.GoAheadId && m.IsDeleted == false).Result.FirstOrDefaultAsync();
                        if (resultGoAhead != null)
                        {
                            //the below code will check again if DeparturesDate are exists in mGoAhead->Departures list , if exists then it will remove from the request.NewDepatures list
                            var deptDates = resultGoAhead.Depatures.Where(a => a.IsDeleted == false).Select(a => a.DepatureDate.Value.ToString("dd/MM/yyyy")).ToList();

                            var existsDates = request.NewDepatures.FindAll(a => deptDates.Contains(a.DepatureDate.Value.ToString("dd/MM/yyyy"))).Select(a => a.DepatureDate).ToList();

                            request.NewDepatures.RemoveAll(a => existsDates.Contains(a.DepatureDate));

                            var resStatus = new ResponseStatus();
                            var lstResStatus = new List<ResponseStatus>();

                            List<Depatures> lstDepatures = new List<Depatures>();
                            QRFCounterRequest qrfCounterRequest = new QRFCounterRequest { CounterType = _configuration["CounterType:QRFDeparture"].ToString() };

                            lstDepatures = request.NewDepatures.Select(a => new Depatures
                            {
                                DepatureDate = a.DepatureDate,
                                DepatureId = _genericRepository.GetNextReferenceNumber(qrfCounterRequest).LastReferenceNumber,
                                CreateDate = DateTime.Now,
                                CreateUser = request.UserEmail,
                                Confirmed_Date = null
                            }).ToList();

                            //step 1:-Add new departures in mGoAhead->Departures
                            UpdateResult resultFlag = await _MongoContext.mGoAhead.UpdateOneAsync(Builders<mGoAhead>.Filter.Eq("GoAheadId", request.GoAheadId),
                                                        Builders<mGoAhead>.Update.PushEach<Depatures>("Depatures", lstDepatures).
                                                        Set("EditUser", request.UserEmail).
                                                        Set("EditDate", DateTime.Now));

                            //step 2:-Add new departures in mQRFPrice->Guesstimate->GuesstimatePosition->GuesstimatePrice
                            //for this 1st copy the selected existing departure details and then copy and add
                            if (resultFlag?.ModifiedCount > 0)
                            {
                                var lstGuesstimatePosition = _MongoContext.mQRFPrice.AsQueryable().Where(m => m.QRFID == request.QRFID && m.QRFPrice_Id == resultGoAhead.QRFPriceId && m.IsDeleted == false).
                                                             FirstOrDefault()?.Guesstimate?.GuesstimatePosition;
                                if (lstGuesstimatePosition?.Count > 0)
                                {
                                    var lstExistGuesstimatePr = new List<GuesstimatePrice>();
                                    var lstEachExistGuesstimatePr = new List<GuesstimatePrice>();
                                    var newGuesstimatePr = new List<GuesstimatePrice>();
                                    UpdateResult resultFlagGuestPr;

                                    foreach (var item in lstGuesstimatePosition)
                                    {
                                        lstExistGuesstimatePr = new List<GuesstimatePrice>();
                                        lstExistGuesstimatePr = item.GuesstimatePrice?.Where(a => a.DepartureId == request.ExisitingDepatures.DepatureId).ToList();
                                        if (lstExistGuesstimatePr?.Count > 0)
                                        {
                                            newGuesstimatePr = new List<GuesstimatePrice>();
                                            foreach (var itemPr in lstDepatures)
                                            {
                                                lstEachExistGuesstimatePr = new List<GuesstimatePrice>();

                                                lstEachExistGuesstimatePr.AddRange(lstExistGuesstimatePr.Select(a => new GuesstimatePrice
                                                {
                                                    BudgetPrice = a.BudgetPrice,
                                                    BuyCurrency = a.BuyCurrency,
                                                    BuyCurrencyId = a.BuyCurrencyId,
                                                    BuyNetPrice = a.BuyNetPrice,
                                                    BuyPrice = a.BuyPrice,
                                                    ContractId = a.ContractId,
                                                    ContractPrice = a.ContractPrice,
                                                    //CreateDate = a.CreateDate,
                                                    //CreateUser = a.CreateUser,
                                                    //DepartureId = a.DepartureId,
                                                    //Period = a.Period,
                                                    //GuesstimatePriceId = a.GuesstimatePriceId,
                                                    EditDate = a.EditDate,
                                                    EditUser = a.EditUser,
                                                    ExchangeRateId = a.ExchangeRateId,
                                                    ExchangeRatio = a.ExchangeRatio,
                                                    IsDeleted = a.IsDeleted,
                                                    IsSupplement = a.IsSupplement,
                                                    KeepAs = a.KeepAs,
                                                    MarkupAmount = a.MarkupAmount,
                                                    PaxSlab = a.PaxSlab,
                                                    PaxSlabId = a.PaxSlabId,
                                                    PositionId = a.PositionId,
                                                    PositionPriceId = a.PositionPriceId,
                                                    ProductCategory = a.ProductCategory,
                                                    ProductCategoryId = a.ProductCategoryId,
                                                    ProductRange = a.ProductRange,
                                                    ProductRangeCode = a.ProductRangeCode,
                                                    ProductRangeId = a.ProductRangeId,
                                                    ProductType = a.ProductType,
                                                    RoomId = a.RoomId,
                                                    SellCurrency = a.SellCurrency,
                                                    SellCurrencyId = a.SellCurrencyId,
                                                    SellNetPrice = a.SellNetPrice,
                                                    SellPrice = a.SellPrice,
                                                    Supplier = a.Supplier,
                                                    SupplierId = a.SupplierId,
                                                    TaxAmount = a.TaxAmount,
                                                    Type = a.Type,
                                                    GuesstimatePriceId = Guid.NewGuid().ToString(),
                                                    Period = itemPr.DepatureDate,
                                                    DepartureId = itemPr.DepatureId,
                                                    CreateDate = DateTime.Now,
                                                    CreateUser = request.UserEmail
                                                }));
                                                //lstEachExistGuesstimatePr.ForEach(a =>
                                                //{
                                                //    a.GuesstimatePriceId = Guid.NewGuid().ToString();
                                                //    a.Period = itemPr.DepatureDate;
                                                //    a.DepartureId = itemPr.DepatureId; a.CreateDate = DateTime.Now; a.CreateUser = request.UserEmail;
                                                //});
                                                newGuesstimatePr.AddRange(lstEachExistGuesstimatePr);
                                            }

                                            if (newGuesstimatePr?.Count > 0)
                                            {
                                                resultFlagGuestPr = await _MongoContext.mQRFPrice.UpdateOneAsync(m => m.QRFID == request.QRFID && m.QRFPrice_Id == resultGoAhead.QRFPriceId
                                                && m.Guesstimate.GuesstimatePosition.Any(a => a.GuesstimatePositionId == item.GuesstimatePositionId),
                                                        Builders<mQRFPrice>.Update.PushEach<GuesstimatePrice>(m => m.Guesstimate.GuesstimatePosition[-1].GuesstimatePrice, newGuesstimatePr).
                                                        Set("EditUser", request.UserEmail).
                                                        Set("EditDate", DateTime.Now));

                                                if (resultFlagGuestPr.ModifiedCount == 0)
                                                {
                                                    resStatus = new ResponseStatus();
                                                    resStatus.Status = "Error";
                                                    resStatus.ErrorMessage = "New departures not added in mQRFPrice->Guesstimate->GuesstimatePosition for GuesstimatePositionId: " + item.GuesstimatePositionId + " ";
                                                    lstResStatus.Add(resStatus);
                                                }
                                            }
                                        }
                                    }

                                    //step 3:-Add new departures in mQRFPositionTotalCost
                                    //step 4:-Add new departures in mQRFPositionPrice
                                    List<mQRFPositionTotalCost> lstExistQRFPositionTotalCost = new List<mQRFPositionTotalCost>();
                                    List<mQRFPositionTotalCost> lstNewQRFPositionTotalCost = new List<mQRFPositionTotalCost>();

                                    List<mQRFPositionPrice> lstExistQRFPositionPrice = new List<mQRFPositionPrice>();
                                    List<mQRFPositionPrice> lstNewQRFPositionPrice = new List<mQRFPositionPrice>();
                                    var newQRFCostForPositionID = "";

                                    var lstQRFPositionTotalCost = _MongoContext.mQRFPositionTotalCost.AsQueryable().Where(m => m.QRFID == request.QRFID && m.QRFPrice_Id == resultGoAhead.QRFPriceId
                                                                    && m.Departure_Id == request.ExisitingDepatures.DepatureId).ToList();

                                    if (lstQRFPositionTotalCost?.Count > 0)
                                    {
                                        var lstQRFPositionTotalCostIds = lstQRFPositionTotalCost.Select(a => a.QRFCostForPositionID).ToList();
                                        var lstQRFPositionPrice = _MongoContext.mQRFPositionPrice.AsQueryable().Where(m => m.QRFID == request.QRFID && m.QRFPrice_Id == resultGoAhead.QRFPriceId
                                                                   && lstQRFPositionTotalCostIds.Contains(m.QRFCostForPositionID)).ToList();

                                        foreach (var itemDept in lstDepatures)
                                        {
                                            lstExistQRFPositionTotalCost = new List<mQRFPositionTotalCost>();
                                            lstExistQRFPositionTotalCost.AddRange(lstQRFPositionTotalCost);
                                            lstNewQRFPositionPrice = new List<mQRFPositionPrice>();

                                            foreach (var item in lstExistQRFPositionTotalCost)
                                            {
                                                item._Id = ObjectId.Empty;
                                                item.DepartureDate = itemDept.DepatureDate;
                                                item.Departure_Id = itemDept.DepatureId;
                                                item.Create_Date = DateTime.Now;
                                                item.Create_User = request.UserEmail;

                                                newQRFCostForPositionID = Guid.NewGuid().ToString();
                                                lstExistQRFPositionPrice = new List<mQRFPositionPrice>();
                                                lstExistQRFPositionPrice = lstQRFPositionPrice.Where(a => a.QRFCostForPositionID == item.QRFCostForPositionID).ToList();
                                                lstExistQRFPositionPrice.ForEach(a => { a.QRFPositionPriceID = Guid.NewGuid().ToString(); a.QRFCostForPositionID = newQRFCostForPositionID; a._Id = ObjectId.Empty; });
                                                lstNewQRFPositionPrice.AddRange(lstExistQRFPositionPrice);

                                                item.QRFCostForPositionID = newQRFCostForPositionID;
                                            }
                                            //lstNewQRFPositionTotalCost.AddRange(lstExistQRFPositionTotalCost);
                                            await _MongoContext.mQRFPositionTotalCost.InsertManyAsync(lstExistQRFPositionTotalCost);
                                            if (lstNewQRFPositionPrice?.Count > 0)
                                            {
                                                await _MongoContext.mQRFPositionPrice.InsertManyAsync(lstNewQRFPositionPrice);
                                            }
                                        }

                                        //step 5:-Add new departures in mQRFNonPackagedPrice
                                        List<mQRFNonPackagedPrice> lstExistQRFNonPackagedPrice = new List<mQRFNonPackagedPrice>();
                                        //List<mQRFNonPackagedPrice> lstNewQRFNonPackagedPrice = new List<mQRFNonPackagedPrice>();

                                        var lstQRFNonPackagedPrice = _MongoContext.mQRFNonPackagedPrice.AsQueryable().Where(m => m.QRFID == request.QRFID && m.QRFPrice_Id == resultGoAhead.QRFPriceId
                                                                  && m.Departure_Id == request.ExisitingDepatures.DepatureId).ToList();

                                        if (lstQRFNonPackagedPrice?.Count > 0)
                                        {
                                            foreach (var itemDept in lstDepatures)
                                            {
                                                lstExistQRFNonPackagedPrice = new List<mQRFNonPackagedPrice>();
                                                lstExistQRFNonPackagedPrice.AddRange(lstQRFNonPackagedPrice);

                                                foreach (var item in lstExistQRFNonPackagedPrice)
                                                {
                                                    item._Id = ObjectId.Empty;
                                                    item.QRFSupplementPriceID = Guid.NewGuid().ToString();
                                                    item.DepartureDate = itemDept.DepatureDate;
                                                    item.Departure_Id = itemDept.DepatureId;
                                                    item.Create_Date = DateTime.Now;
                                                    item.Create_User = request.UserEmail;
                                                }
                                                //lstNewQRFNonPackagedPrice.AddRange(lstExistQRFNonPackagedPrice);
                                                await _MongoContext.mQRFNonPackagedPrice.InsertManyAsync(lstExistQRFNonPackagedPrice);
                                            }
                                        }

                                        //step 6:-Add new departures in mQRFPackagePrice
                                        //List<mQRFPackagePrice> lstExistQRFPackagePrice = new List<mQRFPackagePrice>();
                                        //List<mQRFPackagePrice> lstNewQRFPackagePrice = new List<mQRFPackagePrice>();

                                        //var lstQRFPackagePrice = _MongoContext.mQRFPackagePrice.AsQueryable().Where(m => m.QRFID == request.QRFID && m.QRFPrice_Id == resultGoAhead.QRFPriceId
                                        //                          && m.Departure_Id == request.ExisitingDepatures.DepatureId).ToList();

                                        //if (lstQRFPackagePrice?.Count > 0)
                                        //{
                                        //    foreach (var itemDept in lstDepatures)
                                        //    {
                                        //        lstExistQRFPackagePrice = new List<mQRFPackagePrice>();
                                        //        lstExistQRFPackagePrice.AddRange(lstQRFPackagePrice);

                                        //        foreach (var item in lstExistQRFPackagePrice)
                                        //        {
                                        //            item._Id = ObjectId.Empty;
                                        //            item.QRFPackagePriceId = Guid.NewGuid().ToString();
                                        //            item.DepartureDate = itemDept.DepatureDate;
                                        //            item.Departure_Id = itemDept.DepatureId;
                                        //            item.Create_Date = DateTime.Now;
                                        //            item.Create_User = request.UserEmail;
                                        //        } 
                                        //        await _MongoContext.mQRFPackagePrice.InsertManyAsync(lstExistQRFPackagePrice);
                                        //    }
                                        //}

                                        response.ResponseStatus.Status = "Success";
                                        response.ResponseStatus.ErrorMessage = "New Departures Added Successfully.";
                                    }
                                    else
                                    {
                                        response.ResponseStatus.ErrorMessage = "QRFID not exists in mQRFPositionTotalCost";
                                        response.ResponseStatus.Status = "Error";
                                    }
                                }
                                else
                                {
                                    response.ResponseStatus.ErrorMessage = "GuesstimatePosition not exists in mQRFPrice For QRFID : " + request.QRFID;
                                    response.ResponseStatus.Status = "Error";
                                }
                            }
                            else
                            {
                                response.ResponseStatus.ErrorMessage = "New Depatures not updated in mGoAhead.";
                                response.ResponseStatus.Status = "Error";
                            }
                        }
                        else
                        {
                            response.ResponseStatus.ErrorMessage = "QRFID not exists in mGoAhead.";
                            response.ResponseStatus.Status = "Error";
                        }
                    }
                    else
                    {
                        response.ResponseStatus.ErrorMessage = "Existing Departures can not be null.";
                        response.ResponseStatus.Status = "Error";
                    }
                }
                else
                {
                    response.ResponseStatus.ErrorMessage = "New Departures can not be null.";
                    response.ResponseStatus.Status = "Error";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.ErrorMessage = ex.Message;
                response.ResponseStatus.Status = "Error";
            }
            return response;
        }
        #endregion
    }
}