using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VGER_WAPI.Helpers;
using VGER_WAPI.Models;
using VGER_WAPI.Repositories.Master;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Repositories
{
    public class AccomodationRepository : IAccomodationRepository
    {
        #region Private Variable Declaration
        private readonly MongoContext _MongoContext = null;
        #endregion

        public AccomodationRepository(IOptions<MongoSettings> settings)
        {
            _MongoContext = new MongoContext(settings);
        }

        public async Task<List<AccomodationInfo>> GetAccomodationByQRFID(AccomodationGetReq request)
        {
            var builder = Builders<mPosition>.Filter;
            var filter = builder.Where(q => q.QRFID == request.QRFId);
            var result = await _MongoContext.mPosition.Find(filter).Project(r => r.AccomodationInfo).FirstOrDefaultAsync();
            if (result == null)
            {
                return (new List<AccomodationInfo>());
            }
            else
            {
                var accom = result.Where(f => !f.IsDeleted && f.AccomodationSequence > 0).ToList().OrderBy(r => r.AccomodationSequence).ToList();
                accom.ForEach(c => { c.RoomDetailsInfo.RemoveAll(d => d.IsDeleted == true); });
                return accom;
            }
        }

        public async Task<string> InsertUpdateAccomodation(AccomodationSetReq request)
        {
            //QRFCounterRequest qrfCounterRequest = new QRFCounterRequest();
            //qrfCounterRequest.CounterType = _configuration["CounterType:QRFRoute"].ToString();

            UpdateResult resultFlag;
            var result = _MongoContext.mPosition.AsQueryable().Where(a => a.QRFID == request.QRFID).ToList();

            if (result != null && result.Count > 0)//if exists or not then update as whole Accomodation List
            {
                if (request.SaveType == "full")
                {
                    List<AccomodationInfo> lstAccomodationInfo = result.Select(r => r.AccomodationInfo).FirstOrDefault();

                    request.AccomodationInfo.RemoveAll(f => f.AccomodationSequence == 0 && f.AccomodationId == "");
                    if (request.AccomodationInfo != null && request.AccomodationInfo.Count > 0)
                    {
                        request.AccomodationInfo.AddRange(lstAccomodationInfo.Where(f => f.IsDeleted == true).ToList().Distinct());

                        request.AccomodationInfo.FindAll(f => !lstAccomodationInfo.Exists(r => r.AccomodationId == f.AccomodationId)).ForEach
                       (r =>
                       {
                           r.AccomodationId = ObjectId.GenerateNewId().ToString();
                           r.CreateDate = DateTime.Now;
                           r.IsDeleted = (r.AccomodationSequence == 0 ? true : false);
                           r.EditUser = "";
                           r.EditDate = null;
                           r.RoomDetailsInfo.ForEach(d =>
                           {
                               d.RoomId = ObjectId.GenerateNewId().ToString();
                               d.CreateDate = DateTime.Now;
                               d.EditUser = "";
                               d.EditDate = null;
                           });
                       });

                        request.AccomodationInfo.FindAll(f => lstAccomodationInfo.Exists(r => r.AccomodationId == f.AccomodationId)).ForEach
                           (r =>
                           {
                               r.EditDate = DateTime.Now;
                               r.CreateDate = (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.CreateDate).FirstOrDefault());
                               r.CreateUser = (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.CreateUser).FirstOrDefault());
                               r.IsDeleted = (r.AccomodationSequence == 0 ? true : false);
                               r.CityName = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.CityName).FirstOrDefault()) : r.CityName;
                               r.CityID = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.CityID).FirstOrDefault()) : r.CityID;

                               r.StartingFrom = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.StartingFrom).FirstOrDefault()) : r.StartingFrom;
                               r.NoOfNight = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.NoOfNight).FirstOrDefault()) : r.NoOfNight;
                               r.Category = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.Category).FirstOrDefault()) : r.Category;
                               r.StarRating = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.StarRating).FirstOrDefault()) : r.StarRating;
                               r.Location = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.Location).FirstOrDefault()) : r.Location;

                               r.ChainName = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.ChainName).FirstOrDefault()) : r.ChainName;
                               r.ChainID = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.ChainID).FirstOrDefault()) : r.ChainID;
                               r.HotelName = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.HotelName).FirstOrDefault()) : r.HotelName;
                               r.HotelID = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.HotelID).FirstOrDefault()) : r.HotelID;
                               r.SupplierId = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.SupplierId).FirstOrDefault()) : r.SupplierId;

                               r.MealPlan = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.MealPlan).FirstOrDefault()) : r.MealPlan;
                               r.EarlyCheckInDate = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.EarlyCheckInDate).FirstOrDefault()) : r.EarlyCheckInDate;
                               r.EarlyCheckInTime = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.EarlyCheckInTime).FirstOrDefault()) : r.EarlyCheckInTime;
                               r.NumberOfEarlyCheckInRooms = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.NumberOfEarlyCheckInRooms).FirstOrDefault()) : r.NumberOfEarlyCheckInRooms;
                               r.NumberofInterConnectingRooms = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.NumberofInterConnectingRooms).FirstOrDefault()) : r.NumberofInterConnectingRooms;
                               r.NumberOfWashChangeRooms = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.NumberOfWashChangeRooms).FirstOrDefault()) : r.NumberOfWashChangeRooms;
                               r.LateCheckOutDate = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.LateCheckOutDate).FirstOrDefault()) : r.LateCheckOutDate;
                               r.LateCheckOutTime = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.LateCheckOutTime).FirstOrDefault()) : r.LateCheckOutTime;
                               r.NumberOfLateCheckOutRooms = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.NumberOfLateCheckOutRooms).FirstOrDefault()) : r.NumberOfLateCheckOutRooms;
                               r.Supplement = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.Supplement).FirstOrDefault()) : r.Supplement;
                               r.SupplementID = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.SupplementID).FirstOrDefault()) : r.SupplementID;

                               r.KeepAs = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.KeepAs).FirstOrDefault()) : r.KeepAs;
                               r.RemarksForTL = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.RemarksForTL).FirstOrDefault()) : r.RemarksForTL;
                               r.RemarksForOPS = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.RemarksForOPS).FirstOrDefault()) : r.RemarksForOPS;

                               List<RoomDetailsInfo> lstRoomDetailsInfo = lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.RoomDetailsInfo).FirstOrDefault();
                               r.RoomDetailsInfo.RemoveAll(f => f.RoomSequence == 0 && f.RoomId == "");

                               if (r.RoomDetailsInfo != null && r.RoomDetailsInfo.Count > 0)
                               {
                                   r.RoomDetailsInfo.AddRange(lstRoomDetailsInfo.Where(f => f.IsDeleted == true).ToList().Distinct());

                                   r.RoomDetailsInfo.FindAll(f => !lstRoomDetailsInfo.Exists(p => p.RoomId == f.RoomId)).ForEach
                                (p =>
                           {
                               p.RoomId = ObjectId.GenerateNewId().ToString();
                               p.CreateDate = DateTime.Now;
                               p.IsDeleted = (p.RoomSequence == 0 ? true : false);
                               p.EditUser = "";
                               p.EditDate = null;

                           });

                                   r.RoomDetailsInfo.FindAll(f => lstRoomDetailsInfo.Exists(p => p.RoomId == f.RoomId)).ForEach
                          (p =>
                          {
                              p.EditDate = DateTime.Now;
                              p.CreateDate = (lstRoomDetailsInfo.Where(l => l.RoomId == p.RoomId).Select(l => l.CreateDate).FirstOrDefault());
                              p.CreateUser = (lstRoomDetailsInfo.Where(l => l.RoomId == p.RoomId).Select(l => l.CreateUser).FirstOrDefault());
                              p.IsDeleted = p.RoomSequence == 0 ? true : false;
                              p.Rooms = p.RoomSequence == 0 ? (lstRoomDetailsInfo.Where(l => l.RoomId == p.RoomId).Select(l => l.Rooms).FirstOrDefault()) : p.Rooms;
                              p.RoomType = p.RoomSequence == 0 ? (lstRoomDetailsInfo.Where(l => l.RoomId == p.RoomId).Select(l => l.RoomType).FirstOrDefault()) : p.RoomType;
                              p.RoomTypeID = p.RoomSequence == 0 ? (lstRoomDetailsInfo.Where(l => l.RoomId == p.RoomId).Select(l => l.RoomTypeID).FirstOrDefault()) : p.RoomTypeID;
                              p.IsSupplement = p.RoomSequence == 0 ? (lstRoomDetailsInfo.Where(l => l.RoomId == p.RoomId).Select(l => l.IsSupplement).FirstOrDefault()) : p.IsSupplement;
                          });
                               }


                           });

                        var res = await _MongoContext.mPosition.FindOneAndUpdateAsync(Builders<mPosition>.Filter.Eq("QRFID", request.QRFID),
                               Builders<mPosition>.Update.Set("AccomodationInfo", request.AccomodationInfo.Distinct()));

                        return res != null ? "1" : "Accomodation Details not updated.";
                    }
                    else
                    {
                        return "1";
                    }
                }
                else
                {
                    

                    //var res = await _MongoContext.mPosition.UpdateOneAsync(Builders<mPosition>.Filter.Eq("QRFID", request.QRFID),
                    //                   Builders<mPosition>.Update.Push("AccomodationInfo", request.AccomodationInfo));

                    if (string.IsNullOrEmpty(request.AccomodationInfo[0].AccomodationId))
                    {
                        request.AccomodationInfo.ForEach(r =>
                        {
                            r.AccomodationId = ObjectId.GenerateNewId().ToString();
                            r.CreateDate = DateTime.Now;
                            r.EditUser = "";
                            r.EditDate = null;
                            r.RoomDetailsInfo.ForEach(d =>
                            {
                                d.RoomId = ObjectId.GenerateNewId().ToString();
                                d.CreateDate = DateTime.Now;
                                d.EditUser = "";
                                d.EditDate = null;
                            });
                        });
                        var res = await _MongoContext.mPosition.UpdateOneAsync(Builders<mPosition>.Filter.Eq("QRFID", request.QRFID),
                                Builders<mPosition>.Update.Push("AccomodationInfo", request.AccomodationInfo[0]));
                    }
                    else
                    {
                        List<AccomodationInfo> lstAccomodationInfo = result.Select(r => r.AccomodationInfo).FirstOrDefault();

                        request.AccomodationInfo.RemoveAll(f => f.AccomodationSequence == 0 && f.AccomodationId == "");

                        request.AccomodationInfo.FindAll(f => lstAccomodationInfo.Exists(r => r.AccomodationId == f.AccomodationId)).ForEach
                           (r =>
                           {
                               r.EditDate = DateTime.Now;
                               r.CreateDate = (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.CreateDate).FirstOrDefault());
                               r.CreateUser = (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.CreateUser).FirstOrDefault());
                               r.IsDeleted = (r.AccomodationSequence == 0 ? true : false);
                               r.CityName = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.CityName).FirstOrDefault()) : r.CityName;
                               r.CityID = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.CityID).FirstOrDefault()) : r.CityID;

                               r.StartingFrom = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.StartingFrom).FirstOrDefault()) : r.StartingFrom;
                               r.NoOfNight = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.NoOfNight).FirstOrDefault()) : r.NoOfNight;
                               r.Category = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.Category).FirstOrDefault()) : r.Category;
                               r.StarRating = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.StarRating).FirstOrDefault()) : r.StarRating;
                               r.Location = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.Location).FirstOrDefault()) : r.Location;

                               r.ChainName = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.ChainName).FirstOrDefault()) : r.ChainName;
                               r.ChainID = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.ChainID).FirstOrDefault()) : r.ChainID;
                               r.HotelName = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.HotelName).FirstOrDefault()) : r.HotelName;
                               r.HotelID = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.HotelID).FirstOrDefault()) : r.HotelID;
                               r.SupplierId = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.SupplierId).FirstOrDefault()) : r.SupplierId;

                               r.MealPlan = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.MealPlan).FirstOrDefault()) : r.MealPlan;
                               r.EarlyCheckInDate = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.EarlyCheckInDate).FirstOrDefault()) : r.EarlyCheckInDate;
                               r.EarlyCheckInTime = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.EarlyCheckInTime).FirstOrDefault()) : r.EarlyCheckInTime;
                               r.NumberOfEarlyCheckInRooms = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.NumberOfEarlyCheckInRooms).FirstOrDefault()) : r.NumberOfEarlyCheckInRooms;
                               r.NumberofInterConnectingRooms = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.NumberofInterConnectingRooms).FirstOrDefault()) : r.NumberofInterConnectingRooms;
                               r.NumberOfWashChangeRooms = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.NumberOfWashChangeRooms).FirstOrDefault()) : r.NumberOfWashChangeRooms;
                               r.LateCheckOutDate = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.LateCheckOutDate).FirstOrDefault()) : r.LateCheckOutDate;
                               r.LateCheckOutTime = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.LateCheckOutTime).FirstOrDefault()) : r.LateCheckOutTime;
                               r.NumberOfLateCheckOutRooms = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.NumberOfLateCheckOutRooms).FirstOrDefault()) : r.NumberOfLateCheckOutRooms;
                               r.Supplement = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.Supplement).FirstOrDefault()) : r.Supplement;
                               r.SupplementID = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.SupplementID).FirstOrDefault()) : r.SupplementID;

                               r.KeepAs = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.KeepAs).FirstOrDefault()) : r.KeepAs;
                               r.RemarksForTL = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.RemarksForTL).FirstOrDefault()) : r.RemarksForTL;
                               r.RemarksForOPS = r.AccomodationSequence == 0 ? (lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.RemarksForOPS).FirstOrDefault()) : r.RemarksForOPS;

                               List<RoomDetailsInfo> lstRoomDetailsInfo = lstAccomodationInfo.Where(l => l.AccomodationId == r.AccomodationId).Select(l => l.RoomDetailsInfo).FirstOrDefault();
                               r.RoomDetailsInfo.RemoveAll(f => f.RoomSequence == 0 && f.RoomId == "");

                               if (r.RoomDetailsInfo != null && r.RoomDetailsInfo.Count > 0)
                               {
                                   r.RoomDetailsInfo.AddRange(lstRoomDetailsInfo.Where(f => f.IsDeleted == true).ToList().Distinct());

                                   r.RoomDetailsInfo.FindAll(f => !lstRoomDetailsInfo.Exists(p => p.RoomId == f.RoomId)).ForEach
                                (p =>
                                {
                                    p.RoomId = ObjectId.GenerateNewId().ToString();
                                    p.CreateDate = DateTime.Now;
                                    p.IsDeleted = (p.RoomSequence == 0 ? true : false);
                                    p.EditUser = "";
                                    p.EditDate = null;

                                });

                                   r.RoomDetailsInfo.FindAll(f => lstRoomDetailsInfo.Exists(p => p.RoomId == f.RoomId)).ForEach
                          (p =>
                          {
                              p.EditDate = DateTime.Now;
                              p.CreateDate = (lstRoomDetailsInfo.Where(l => l.RoomId == p.RoomId).Select(l => l.CreateDate).FirstOrDefault());
                              p.CreateUser = (lstRoomDetailsInfo.Where(l => l.RoomId == p.RoomId).Select(l => l.CreateUser).FirstOrDefault());
                              p.IsDeleted = p.RoomSequence == 0 ? true : false;
                              p.Rooms = p.RoomSequence == 0 ? (lstRoomDetailsInfo.Where(l => l.RoomId == p.RoomId).Select(l => l.Rooms).FirstOrDefault()) : p.Rooms;
                              p.RoomType = p.RoomSequence == 0 ? (lstRoomDetailsInfo.Where(l => l.RoomId == p.RoomId).Select(l => l.RoomType).FirstOrDefault()) : p.RoomType;
                              p.RoomTypeID = p.RoomSequence == 0 ? (lstRoomDetailsInfo.Where(l => l.RoomId == p.RoomId).Select(l => l.RoomTypeID).FirstOrDefault()) : p.RoomTypeID;
                              p.IsSupplement = p.RoomSequence == 0 ? (lstRoomDetailsInfo.Where(l => l.RoomId == p.RoomId).Select(l => l.IsSupplement).FirstOrDefault()) : p.IsSupplement;
                          });
                               }


                           });

                        var res = await _MongoContext.mPosition.FindOneAndUpdateAsync(m => m.QRFID == request.QRFID && m.AccomodationInfo.Any(md => md.AccomodationId == request.AccomodationInfo[0].AccomodationId),
                                   Builders<mPosition>.Update.Set(m => m.AccomodationInfo[-1], request.AccomodationInfo[0]));
                    }
                    return request.AccomodationInfo[0].AccomodationId;
                }
            }
            else//insert Route details at 1st time
            {
                request.AccomodationInfo.RemoveAll(f => f.AccomodationSequence == 0 && f.AccomodationId == "");

                if (request.AccomodationInfo != null && request.AccomodationInfo.Count > 0)
                {
                    request.AccomodationInfo.ForEach(r =>
                    {
                        r.AccomodationId = ObjectId.GenerateNewId().ToString();
                        r.CreateDate = DateTime.Now;
                        r.EditUser = "";
                        r.EditDate = null;
                        r.RoomDetailsInfo.ForEach(d =>
                        {
                            d.RoomId = ObjectId.GenerateNewId().ToString();
                            d.CreateDate = DateTime.Now;
                            d.EditUser = "";
                            d.EditDate = null;
                        });
                    });

                    mPosition mAccomodation = new mPosition();
                    mAccomodation.QRFID = request.QRFID;
                    mAccomodation.AccomodationInfo = request.AccomodationInfo;
                    await _MongoContext.mPosition.InsertOneAsync(mAccomodation);

                    return "1";
                }
                else
                {
                    return "1";
                }
            }
        }

    }
}
