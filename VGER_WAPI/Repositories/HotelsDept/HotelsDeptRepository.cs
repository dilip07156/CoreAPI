using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VGER_WAPI.Helpers;
using VGER_WAPI.Models;
using VGER_WAPI.Providers;
using VGER_WAPI_CLASSES;
//comment before commiting to development
//using Microsoft.Office.Interop.Outlook;
//using OutlookApp = Microsoft.Office.Interop.Outlook.Application;

namespace VGER_WAPI.Repositories
{
    public class HotelsDeptRepository : IHotelsDeptRepository
    {
        #region Private Variable Declaration
        private readonly MongoContext _MongoContext = null;
        private readonly IGenericRepository _genericRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration _configuration;
        private readonly IEmailRepository _emailRepository;
        private BookingProviders _bookingProviders = null;
        private CompanyProviders _companyProviders = null;
        #endregion

        public HotelsDeptRepository(IOptions<MongoSettings> settings, IConfiguration configuration, IHostingEnvironment env, IGenericRepository genericRepository, IEmailRepository emailRepository, IBookingRepository bookingRepository)
        {
            _MongoContext = new MongoContext(settings);
            _genericRepository = genericRepository;
            _configuration = configuration;
            _env = env;
            _bookingProviders = new BookingProviders(_configuration);
            _bookingRepository = bookingRepository;
            _companyProviders = new CompanyProviders(_configuration);
            _emailRepository = emailRepository;
        }

        #region Hotels Search  
        public async Task<HotelsDeptSearchRes> GetHotelsByBookingDetails(BookingSearchReq request)
        {
            HotelsDeptSearchRes searchResponse = new HotelsDeptSearchRes();
            List<HotelSearchResult> response = new List<HotelSearchResult>();
            try
            {
                //var bookingList = _MongoContext.Bookings.AsQueryable();
                FilterDefinition<Bookings> filter = Builders<Bookings>.Filter.Empty;
                List<Bookings> result = new List<Bookings>();
                string[] StatusIgnoreList = new string[] { "N", "J", "I", "B", "C", "-", "^", "L", "S", "X", "T" };

                #region create filter for query
                if (!string.IsNullOrEmpty(request.AgentName))
                {
                    filter = filter & Builders<Bookings>.Filter.Regex(x => x.AgentInfo.Name, new BsonRegularExpression(new Regex(request.AgentName.Trim(), RegexOptions.IgnoreCase)));
                }
                if (!string.IsNullOrWhiteSpace(request.BookingNumber))
                {
                    filter = filter & Builders<Bookings>.Filter.Regex(x => x.BookingNumber, new BsonRegularExpression(new Regex(request.BookingNumber.Trim(), RegexOptions.IgnoreCase)));
                }
                if (!string.IsNullOrWhiteSpace(request.AgentCode))
                {
                    filter = filter & Builders<Bookings>.Filter.Regex(x => x.AgentInfo.Code, new BsonRegularExpression(new Regex(request.AgentCode.Trim(), RegexOptions.IgnoreCase)));
                }
                if (!string.IsNullOrWhiteSpace(request.BookingName))
                {
                    filter = filter & Builders<Bookings>.Filter.Regex(x => x.CustRef, new BsonRegularExpression(new Regex(request.BookingName.Trim(), RegexOptions.IgnoreCase)));
                }
                if (!string.IsNullOrWhiteSpace(request.Status))
                {
                    filter = filter & Builders<Bookings>.Filter.Eq(x => x.STATUS, request.Status);
                }
                else
                {
                    filter = filter & Builders<Bookings>.Filter.Nin(x => x.STATUS, StatusIgnoreList.Select(a => a));
                }
                if (!string.IsNullOrWhiteSpace(request.DateType) && request.DateType.ToLower().Trim() == "creation date")
                { 
                    if (!string.IsNullOrWhiteSpace(request.From) && !string.IsNullOrEmpty(request.To))
                    {
                        var strFromDT = request.From.Split("/");
                        var strToDT = request.To.Split("/");
                        if (strFromDT?.Count() >= 3 && strToDT?.Count() >= 3)
                        {
                            DateTime fromDT = new DateTime(Convert.ToInt32(strFromDT[2]), Convert.ToInt32(strFromDT[1]), Convert.ToInt32(strFromDT[0]));
                            DateTime toDT = new DateTime(Convert.ToInt32(strToDT[2]), Convert.ToInt32(strToDT[1]), Convert.ToInt32(strToDT[0]));
                            toDT = toDT.AddHours(23).AddMinutes(59).AddSeconds(59);
                            filter = filter & Builders<Bookings>.Filter.Where(x => x.AuditTrail.CREA_DT >= fromDT && x.AuditTrail.CREA_DT <= toDT);
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(request.From) && string.IsNullOrEmpty(request.To))
                    {
                        var strFromDT = request.From.Split("/"); 
                        if (strFromDT?.Count() >= 3)
                        {
                            DateTime fromDT = new DateTime(Convert.ToInt32(strFromDT[2]), Convert.ToInt32(strFromDT[1]), Convert.ToInt32(strFromDT[0]));                            
                            filter = filter & Builders<Bookings>.Filter.Where(x => x.AuditTrail.CREA_DT >= fromDT);
                        } 
                    }
                    else if (string.IsNullOrWhiteSpace(request.From) && !string.IsNullOrEmpty(request.To))
                    {
                        var strToDT = request.To.Split("/");
                        if (strToDT?.Count() >= 3)
                        {
                            DateTime toDT = new DateTime(Convert.ToInt32(strToDT[2]), Convert.ToInt32(strToDT[1]), Convert.ToInt32(strToDT[0]));
                            toDT = toDT.AddHours(23).AddMinutes(59).AddSeconds(59);
                            filter = filter & Builders<Bookings>.Filter.Where(x => x.AuditTrail.CREA_DT <= toDT);
                        } 
                    }
                }

                if (!string.IsNullOrWhiteSpace(request.DateType) && request.DateType.ToLower().Trim() == "travel date")
                {
                   if (!string.IsNullOrWhiteSpace(request.From) )
                    {
                        var strFromDT = request.From.Split("/");
                        if (strFromDT?.Count() >= 3)
                        {
                            DateTime fromDT = new DateTime(Convert.ToInt32(strFromDT[2]), Convert.ToInt32(strFromDT[1]), Convert.ToInt32(strFromDT[0]));
                            filter = filter & Builders<Bookings>.Filter.Where(x => x.STARTDATE >= fromDT);
                        }
                    }
                    else if (!string.IsNullOrEmpty(request.To))
                    {
                        var strToDT = request.To.Split("/");
                        if (strToDT?.Count() >= 3)
                        {
                            DateTime toDT = new DateTime(Convert.ToInt32(strToDT[2]), Convert.ToInt32(strToDT[1]), Convert.ToInt32(strToDT[0]));
                            toDT = toDT.AddHours(23).AddMinutes(59).AddSeconds(59);
                            filter = filter & Builders<Bookings>.Filter.Where(x => x.ENDDATE <= toDT);
                        }
                    } 
                }
                DateTime curDt = DateTime.Now;
                var curNewdate = new DateTime(curDt.Year, curDt.Month, curDt.Day, 0, 0, 0);
                filter = filter & Builders<Bookings>.Filter.Where(x => x.ENDDATE >= curNewdate);
                #endregion

                result = await _MongoContext.Bookings.Find(filter).ToListAsync();

                foreach (var item in result)
                {
                    item.Positions = item.Positions.Where(a => a.ProductType.Trim() == "Hotel" && !StatusIgnoreList.Contains(a.STATUS)).OrderBy(a => a.STARTDATE).ToList();
                    if (item.Positions.Count == 0)
                    {
                        item.STATUS = "J";
                    }
                }
                result = result.Where(a => a.STATUS != "J").OrderBy(x => x.STARTDATE).ToList();

                response = result.Skip(request.Start).Take(request.Length).Select(a => new HotelSearchResult
                {
                    AgentName = a.AgentInfo?.Name,
                    BookingName = a.CustRef,
                    BookingNumber = a.BookingNumber,
                    BookingRooms = a.BookingRooms,
                    ContactName = a.AgentInfo?.Contact_Name,
                    Duration = Convert.ToString(a.Duration),
                    Status = a.STATUS,
                    StartDate = (Convert.ToDateTime(a.STARTDATE)).ToString("dd/MM/yyyy").Replace('-', '/'),
                    EndDate = (Convert.ToDateTime(a.ENDDATE)).ToString("dd/MM/yyyy").Replace('-', '/')
                }).ToList();

                searchResponse.BookingsDetails = response;

                if (result.Count > 0)
                {
                    searchResponse.HotelsTotalCount = result.Count();
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return searchResponse;
        }
        #endregion

        public async Task<HotelsByBookingGetRes> GetProductHotelDetails(ProductSRPHotelGetReq request)
        {
            HotelsByBookingGetRes response = new HotelsByBookingGetRes() { QRFID = request.QRFID, ResponseStatus = new ResponseStatus() };
            try
            {
                var resBooking = await _MongoContext.Bookings.FindAsync(a => a.BookingNumber == request.QRFID).Result.FirstOrDefaultAsync();
                if (resBooking != null)
                {
                    List<string> lstProdId = resBooking.Positions.Select(a => a.Product_Id).ToList();
                    var resSRP = await _MongoContext.mProducts_Lite.FindAsync(a => lstProdId.Contains(a.VoyagerProduct_Id)).Result.ToListAsync();

                    string[] StatusIgnoreList = new string[] { "N", "J", "I", "B", "C", "-", "^", "L", "S", "X", "T" };
                    resBooking.Positions = resBooking.Positions.Where(a => a.ProductType.Trim() == "Hotel" && !StatusIgnoreList.Contains(a.STATUS)).OrderBy(a => a.STARTDATE).ToList();

                    //resBooking.Positions.ForEach(a => a.AlternateServices = a.AlternateServices.OrderBy(b => b.Product_Name).ToList());
                    var posStatusList = resBooking.Positions.Select(a => a.STATUS).ToList();
                    var posStatusDescList = new List<mStatus>();
                    if (posStatusList?.Count > 0)
                    {
                        posStatusDescList = _MongoContext.mStatus.AsQueryable().Where(a => posStatusList.Contains(a.Status)).ToList();
                    }

                    resBooking.Positions.ForEach(a =>
                    {
                        a.AlternateServices = a.AlternateServices
                                            .Where(b => b.IsBlackListed == null || b.IsBlackListed == false || (b.IsBlackListed == true && b.Requested_On != null && b.Requested_On > Convert.ToDateTime("01-01-2000")))
                                            .OrderBy(b => b.Product_Name)
                                            .ToList();
                        a.STATUS = posStatusDescList?.Where(b => b.Status.ToLower() == a.STATUS.ToLower()).FirstOrDefault()?.Description;
                    });

                    response.Bookings = resBooking;
                    response.ProductSRPDetails = resSRP.Select(a => new ProductSRPDetails
                    {
                        Address = a.Address,
                        BdgPriceCategory = a.BdgPriceCategory,
                        Chain = a.Chain,
                        CityName = a.CityName,
                        CountryName = a.CountryName,
                        CreateDate = a.CreateDate,
                        CreateUser = a.CreateUser,
                        DefaultSupplierId = a.DefaultSupplierId,
                        DefaultSupplier = a.DefaultSupplier,
                        EditDate = a.EditDate,
                        EditUser = a.EditUser,
                        HotelImageURL = a.HotelImageURL,
                        HotelType = a.HotelType,
                        Location = a.Location,
                        Placeholder = a.Placeholder,
                        PostCode = a.PostCode,
                        ProdDesc = a.ProdDesc,
                        ProdName = a.ProdName,
                        ProductCode = a.ProductCode,
                        ProductFacilities = a.ProductFacilities,
                        ProductType = a.ProductType,
                        ProductType_Id = a.ProductType_Id,
                        Rooms = a.Rooms,
                        StarRating = a.StarRating,
                        Status = a.Status,
                        Street = a.Street,
                        VoyagerProduct_Id = a.VoyagerProduct_Id
                    }).ToList();
                }
                else
                {
                    response.ResponseStatus.ErrorMessage = "Booking Number not exists in Bookings.";
                    response.ResponseStatus.Status = "Error";
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return response;
        }

        public async Task<HotelAlternateServicesGetRes> GetAlternateServicesByBooking(HotelAlternateServicesGetReq request)
        {
            HotelAlternateServicesGetRes response = new HotelAlternateServicesGetRes();
            try
            {
                var resBooking = await _MongoContext.Bookings.FindAsync(a => a.BookingNumber == request.BookingNumber).Result.FirstOrDefaultAsync();
                if (resBooking != null)
                {
                    response.AlternateServices = resBooking.Positions.Where(a => a.Position_Id == request.PositionId).FirstOrDefault().AlternateServices;
                }
                else
                {
                    response.ResponseStatus.ErrorMessage = "Booking Number not exists in Bookings.";
                    response.ResponseStatus.Status = "Error";
                }
            }
            catch (System.Exception ex)
            {
                throw;
            }
            return response;
        }

        //the below function used for SendHotelRequest,Remind,TestEmail
        public async Task<HotelReservationEmailRes> SendHotelReservationRequestEmail(HotelReservationRequestEmail request)
        {
            HotelReservationEmailRes response = new HotelReservationEmailRes();
            try
            {
                var resBooking = await _MongoContext.Bookings.FindAsync(a => a.BookingNumber == request.BookingNumber).Result.FirstOrDefaultAsync();
                if (resBooking != null)
                {
                    var Position = resBooking.Positions.Where(a => a.Position_Id == request.PositionId).FirstOrDefault();
                    if (Position != null)
                    {
                        #region Send Email (SendHotelRequest:-update bookings in mongodb and call bridge service)   
                        string altsvcid = Guid.NewGuid().ToString();
                        var mUser = await _MongoContext.mUsers.FindAsync(a => a.VoyagerUser_Id == request.PlacerUserId).Result.FirstOrDefaultAsync();
                        var mCompany = await _MongoContext.mCompanies.FindAsync(a => a.Company_Id == mUser.Company_Id).Result.FirstOrDefaultAsync();
                        var resContact = mCompany.ContactDetails.Where(a => a.Contact_Id == mUser.Contact_Id).FirstOrDefault();

                        if (resContact != null)
                        {
                            var doctype = "";
                            if (request.SendType.ToLower() == "hotelrequests" || request.SendType.ToLower() == "opschangeproduct")
                                doctype = DocType.BOOKREQ;
                            else if (request.SendType.ToLower() == "remind")
                                doctype = DocType.REMIND;
                            else if (request.SendType.ToLower() == "testemail")
                                doctype = DocType.TESTEMAIL;

                            #region Send Mail:-HotelRequest/Remind/TestEmail
                            var objEmailGetReq = new EmailGetReq()
                            {
                                AlternateServiceId = request.AltSvcId,
                                BookingNo = request.BookingNumber,
                                DocumentType = doctype,
                                PositionId = request.PositionId,
                                PlacerUserId = request.PlacerUserId,
                                UserEmail = request.PlacerEmail,
                                SupplierId = Position?.SupplierInfo?.Id,
                                QrfId = resBooking?.QRFID,
                                WebURLInitial = request.WebURLInitial,
                                PosAlternateServiceId = altsvcid
                            };
                            if (request.SendType.ToLower() == "opschangeproduct")
                                objEmailGetReq.Source = request.SendType.ToLower();
                            var responseStatusMail = await _emailRepository.GenerateEmail(objEmailGetReq);
                            if (responseStatusMail == null || responseStatusMail.ResponseStatus == null || string.IsNullOrEmpty(responseStatusMail.ResponseStatus.Status))
                            {
                                responseStatusMail.ResponseStatus = new ResponseStatus();
                                responseStatusMail.ResponseStatus.Status = "Error";
                                responseStatusMail.ResponseStatus.ErrorMessage = "Mail not sent.";
                            }
                            #endregion

                            if (request.SendType == "hotelrequests")
                            {
                                List<string> lstProductId = new List<string>();

                                lstProductId = Position.AlternateServices.Select(a => a.Product_Id).ToList();
                                lstProductId.Add(Position.Product_Id);

                                var ProductSrpList = _MongoContext.mProducts_Lite.AsQueryable().Where(a => lstProductId.Contains(a.VoyagerProduct_Id)).ToList();
                                var ProductSrp = ProductSrpList.Where(a => a.VoyagerProduct_Id == Position.Product_Id).FirstOrDefault();

                                //the below functionality used for saving Position's Supplier as AlternateService 
                                if (ProductSrp != null && ProductSrp.Placeholder != true && Position.AlternateServices.Where(a => a.Product_Id == Position.Product_Id
                                    && a.SupplierInfo != null && a.SupplierInfo?.Id == Position.SupplierInfo?.Id).Count() < 1)
                                {
                                    Position.AlternateServices.Add(new AlternateServices
                                    {
                                        AlternateServies_Id = altsvcid,
                                        SortOrder = 1,
                                        IsBlackListed = false,
                                        Product_Id = Position.Product_Id,
                                        Product_Name = Position.Product_Name,
                                        Country_Id = Position.Country_Id,
                                        Country = Position.Country,
                                        City_Id = Position.City_Id,
                                        City = Position.City,
                                        Requested_On = null,
                                        Attributes = Position.Attributes,
                                        SupplierInfo = Position.SupplierInfo,
                                        Request_RoomsAndPrices = Position.BookingRoomsAndPrices,
                                        AuditTrail = Position.AuditTrail
                                    });
                                }

                                foreach (var AltSvc in Position.AlternateServices.Where(a => a.IsBlackListed == null || a.IsBlackListed == false && (a.Requested_On == null || a.Requested_On < Convert.ToDateTime("01-01-2000"))))
                                {
                                    if (AltSvc != null && AltSvc.SupplierInfo != null && !string.IsNullOrEmpty(AltSvc.SupplierInfo.Contact_Name) && !string.IsNullOrEmpty(AltSvc.SupplierInfo.Contact_Email))
                                    {
                                        AltSvc.Requested_On = DateTime.Now;
                                        AltSvc.Request_Status = "Sent";
                                        AltSvc.Availability_Status = "PENDING";
                                    }
                                }

                                //update positions in Bookings mongodb
                                var resbook = await _MongoContext.Bookings.FindOneAndUpdateAsync(a => a.BookingNumber == request.BookingNumber &&
                                      a.Positions.Any(b => b.Position_Id == request.PositionId), Builders<Bookings>.Update.Set(a => a.Positions[-1], Position));

                                if (responseStatusMail != null && responseStatusMail?.ResponseStatus?.Status?.ToLower() != "error")
                                {
                                    if (resbook != null)
                                    {
                                        //The below Bridge service will UPSERT the details into documents, communicationlogs table in sql
                                        //and Update the fileds in Positionrequests table in sql
                                        ResponseStatus responseStatus = await _bookingProviders.SetBookingAlternateServiceEmailDetails(new BookingPosAltSetReq()
                                        {
                                            BookingNumber = request.BookingNumber,
                                            PositionId = request.PositionId,
                                            User = request.PlacerEmail,
                                            DocumentType = DocType.BOOKREQ
                                        });

                                        if (responseStatus != null && responseStatus.Status != null)
                                        {
                                            if (responseStatus.Status == "Success")
                                            {
                                                response.ResponseStatus.Status = "Success";
                                                response.ResponseStatus.ErrorMessage = "Mail Send Successfully.";
                                            }
                                            else
                                            {
                                                response.ResponseStatus.Status = "Failure";
                                                response.ResponseStatus.ErrorMessage = responseStatus.StatusMessage;
                                            }
                                        }
                                        else
                                        {
                                            response.ResponseStatus.Status = "Failure";
                                            response.ResponseStatus.ErrorMessage = "Email Details not updated in OPS DB.";
                                        }
                                    }
                                    else
                                    {
                                        response.ResponseStatus.Status = "Failure";
                                        response.ResponseStatus.ErrorMessage = "Booking Positions not updated in mongodb.";
                                    }
                                }
                                else
                                {
                                    response.ResponseStatus.ErrorMessage = "Supplier details not found to send the mail.";
                                    response.ResponseStatus.Status = "Error";
                                }
                            }
                            else
                            {
                                if (responseStatusMail?.ResponseStatus?.Status == "Success")
                                {
                                    response.ResponseStatus.ErrorMessage = "Mail Sent Successfully.";
                                    response.ResponseStatus.Status = "Success";
                                }
                                else
                                {
                                    response.ResponseStatus.ErrorMessage = "Mail not Sent.";
                                    response.ResponseStatus.Status = "Error";
                                }
                            }
                        }
                        else
                        {
                            response.ResponseStatus.ErrorMessage = "Hotel placer contact details not exists.";
                            response.ResponseStatus.Status = "Error";
                        }
                        #endregion
                    }
                    else
                    {
                        response.ResponseStatus.ErrorMessage = "Hotel not exists in booking.";
                        response.ResponseStatus.Status = "Error";
                    }
                }
                else
                {
                    response.ResponseStatus.ErrorMessage = "Booking Number not exists.";
                    response.ResponseStatus.Status = "Error";
                }
            }
            catch (System.Exception ex)
            {
                response.ResponseStatus.ErrorMessage = ex.Message;
                response.ResponseStatus.Status = "Error";
            }
            return response;
        }

        public async Task<AvailabilityRequestDetailsGetRes> GetHotelAvailabilityRequestDetails(AvailabilityRequestDetailsGetReq request)
        {
            AvailabilityRequestDetailsGetRes response = new AvailabilityRequestDetailsGetRes { CostingGetProperties = new CostingGetProperties() };
            try
            {
                mQRFPrice resQRFPrice = null;
                string resQRFID = "";
                List<mProducts_Lite> resSRP = null;
                string[] StatusIgnoreList = new string[] { "N", "J", "I", "B", "C", "-", "^", "L", "S", "X", "T" };

                #region get booking & mQRFPrice to fetch QRFID
                var resBooking = _MongoContext.Bookings.AsQueryable().Where(a => a.BookingNumber == request.BookingNumber).FirstOrDefault();
                var position = resBooking.Positions.Where(a => a.Position_Id == request.PositionId && a.ProductType.Trim() == "Hotel" && !StatusIgnoreList.Contains(a.STATUS)).FirstOrDefault();
                var AltSvc = position.AlternateServices.Where(a => a.AlternateServies_Id == request.AltSvcId).FirstOrDefault();

                if (!string.IsNullOrEmpty(request.QRFID))
                {
                    resQRFPrice = await _MongoContext.mQRFPrice.FindAsync(m => m.QRFID == request.QRFID && m.IsDeleted == false).Result.FirstOrDefaultAsync();
                }

                if (string.IsNullOrEmpty(request.QRFID) || resQRFPrice == null)
                {
                    resQRFID = resBooking.QRFID;
                    if (!string.IsNullOrEmpty(resQRFID))
                    {
                        request.QRFID = resQRFID;
                        resQRFPrice = await _MongoContext.mQRFPrice.FindAsync(m => m.QRFID == request.QRFID && m.IsDeleted == false).Result.FirstOrDefaultAsync();
                    }
                    else
                    {
                        request.QRFID = "";
                    }
                }
                #endregion

                #region get currency list
                var CurrencyRes = _MongoContext.mCurrency.AsQueryable().Select(c => new Currency { CurrencyCode = c.Currency, CurrencyName = c.Name, CurrencyId = c.VoyagerCurrency_Id }).ToList();
                if (CurrencyRes != null) response.CurrencyList = CurrencyRes;

                #endregion

                #region assign page header values from mQRFPrice
                if (resQRFPrice != null)
                {
                    response.CostingGetProperties.QRFID = resQRFPrice.QRFID;
                    response.CostingGetProperties.VersionId = resQRFPrice.VersionId;
                    response.CostingGetProperties.VersionName = resQRFPrice.VersionName;
                    response.CostingGetProperties.VersionDescription = resQRFPrice.VersionDescription;
                    response.CostingGetProperties.IsCurrentVersion = resQRFPrice.IsCurrentVersion;
                    response.CostingGetProperties.SalesOfficer = resQRFPrice.SalesOfficer;
                    response.CostingGetProperties.CostingOfficer = resQRFPrice.CostingOfficer;
                    response.CostingGetProperties.ProductAccountant = resQRFPrice.ProductAccountant;
                    response.CostingGetProperties.ValidForAcceptance = resQRFPrice.ValidForAcceptance;
                    response.CostingGetProperties.ValidForTravel = resQRFPrice.ValidForTravel;
                    response.CostingGetProperties.AgentInfo = resQRFPrice.AgentInfo;
                    response.CostingGetProperties.AgentProductInfo = resQRFPrice.AgentProductInfo;
                    response.CostingGetProperties.AgentPassengerInfo = resQRFPrice.AgentPassengerInfo;
                    response.CostingGetProperties.AgentRoom = resQRFPrice.QRFAgentRoom;
                    response.CostingGetProperties.DepartureDates = resQRFPrice.Departures;
                    response.CostingGetProperties.Document_Id = "";
                }
                #endregion

                #region assign responses of ProductSRPDetails, ReservationRequestDetails
                if (resBooking != null)
                {
                    if (position != null)
                    {
                        #region get and assign product srp details of the product
                        resSRP = _MongoContext.mProducts_Lite.AsQueryable().Where(a => a.VoyagerProduct_Id == AltSvc.Product_Id).ToList();
                        response.ProductSRPDetails = resSRP.Select(a => new ProductSRPDetails
                        {
                            Address = a.Address,
                            BdgPriceCategory = a.BdgPriceCategory,
                            Chain = a.Chain,
                            CityName = a.CityName,
                            CountryName = a.CountryName,
                            CreateDate = a.CreateDate,
                            CreateUser = a.CreateUser,
                            DefaultSupplier = a.DefaultSupplier,
                            EditDate = a.EditDate,
                            EditUser = a.EditUser,
                            HotelImageURL = a.HotelImageURL,
                            HotelType = a.HotelType,
                            Location = a.Location,
                            Placeholder = a.Placeholder,
                            PostCode = a.PostCode,
                            ProdDesc = a.ProdDesc,
                            ProdName = a.ProdName,
                            ProductCode = a.ProductCode,
                            ProductFacilities = a.ProductFacilities,
                            ProductType = a.ProductType,
                            ProductType_Id = a.ProductType_Id,
                            Rooms = a.Rooms,
                            StarRating = a.StarRating,
                            Status = a.Status,
                            Street = a.Street,
                            VoyagerProduct_Id = a.VoyagerProduct_Id
                        }).ToList();
                        #endregion

                        #region get and assign Reservation Request Details
                        response.ReservationRequestDetails = new ReservationRequestDetails
                        {
                            Check_In = Convert.ToDateTime(position.STARTDATE).ToString("MMMM dd"),
                            Check_Out_Date = Convert.ToDateTime(position.ENDDATE).ToString("MMMM dd"),
                            Nights = position.DURATION,
                            Nationality = resBooking.GuestDetails.Nationality_Name,
                            Board_Basis = position.HOTELMEALPLAN,
                            Stars = position.HotelStarRating != null && position.HotelStarRating > 0 ? position.HotelStarRating : 0,
                            Location = position.Attributes != null ? position.Attributes.Location : "",
                            Category = position.Attributes != null ? position.Attributes.BdgPriceCategory : ""
                        };
                        #endregion

                        #region get and assign Alternate Services Rooms And Prices
                        if (AltSvc != null)
                        {
                            if (AltSvc.Availability_Status == null || (AltSvc.Availability_Status != null && AltSvc.Availability_Status.ToLower() == "pending"))
                            {
                                response.ResponseStatus.ErrorMessage = "Not yet updated";
                                response.ResponseStatus.Status = "pending";
                            }
                            else
                            {
                                response.ResponseStatus.ErrorMessage = "Already updated";
                                response.ResponseStatus.Status = "done";
                            }

                            //if (AltSvc.Availability_Status == null || (AltSvc.Availability_Status != null && AltSvc.Availability_Status.ToLower() == "pending"))
                            //{
                            var resCompany = await _MongoContext.mCompanies.FindAsync(a => a.Company_Id == AltSvc.SupplierInfo.Id).Result.FirstOrDefaultAsync();
                            var companyContact = resCompany?.ContactDetails?.Where(a => a.Contact_Id.ToLower() == AltSvc.SupplierInfo.Contact_Id.ToLower()).FirstOrDefault();
                            if (companyContact == null) companyContact = new CompanyContacts();

                            response.UpdateReqDetails.OptionDate = AltSvc.OptionDate;
                            response.UpdateReqDetails.CancellationDeadline = Convert.ToInt32(AltSvc.CancellationDeadline);
                            response.UpdateReqDetails.ReservationsEmail = AltSvc.SupplierInfo.Contact_Email;
                            response.UpdateReqDetails.Telephone = AltSvc.SupplierInfo.Contact_Tel;
                            response.UpdateReqDetails.Title = companyContact.CommonTitle;
                            response.UpdateReqDetails.FirstName = companyContact.FIRSTNAME;
                            response.UpdateReqDetails.LastName = companyContact.LastNAME;
                            response.UpdateReqDetails.Availability = AltSvc.Availability_Status;
                            response.UpdateReqDetails.SupplierId = AltSvc.SupplierInfo.Id;
                            response.AltSvcRoomsAndPrices = AltSvc.Request_RoomsAndPrices.Select(a => new AltSvcRoomsAndPrices
                            {
                                BookingRoomsId = a.BookingRooms_Id,
                                PersonType = a.PersonType,
                                RoomName = a.RoomName,
                                Quantity = Convert.ToString(a.Req_Count),
                                CurrencyId = a.BuyCurrency_Id,
                                CurrencyCode = a.BuyCurrency_Name,
                                RoomRate = Convert.ToString(a.BuyPrice)
                            }).ToList();

                            if (AltSvc.BudgetSupplements == null || AltSvc.BudgetSupplements.Count < 1)
                            {
                                #region BudgetSupplement
                                foreach (var AltSvcRoom in AltSvc.Request_RoomsAndPrices)
                                {
                                    var BookingRoom = AltSvc.Request_RoomsAndPrices.Where(a => a.BookingRooms_Id == AltSvcRoom.BookingRooms_Id).FirstOrDefault();
                                    if (BookingRoom != null)
                                    {
                                        var positionRoom = position.BookingRoomsAndPrices.Where(a => a.RoomName == BookingRoom.RoomName && a.PersonType == BookingRoom.PersonType).FirstOrDefault();
                                        if (positionRoom != null)
                                        {
                                            decimal? BudgetSupplementAmount = 0;
                                            if (BookingRoom.BuyCurrency_Id == positionRoom.BuyCurrency_Id)
                                            {
                                                BudgetSupplementAmount = BookingRoom.BuyPrice - positionRoom.BudgetPrice;
                                            }
                                            else
                                            {
                                                var BdgCurrency = resBooking.ExchangeRateSnapshot.ExchangeRateDetail.Where(a => a.Currency_Id == positionRoom.BuyCurrency_Id).FirstOrDefault();
                                                var BuyCurrency = resBooking.ExchangeRateSnapshot.ExchangeRateDetail.Where(a => a.Currency_Id == BookingRoom.BuyCurrency_Id).FirstOrDefault();

                                                if (BdgCurrency != null && BuyCurrency != null)
                                                {
                                                    decimal Rate = Convert.ToDecimal(BuyCurrency.RATE / BdgCurrency.RATE);
                                                    BookingRoom.BuyPrice = (BookingRoom.BuyPrice / Rate);
                                                }
                                                BudgetSupplementAmount = BookingRoom.BuyPrice - positionRoom.BudgetPrice;
                                            }

                                            if (BudgetSupplementAmount > 0)
                                            {
                                                var budgetSuppNew = new BudgetSupplements();
                                                budgetSuppNew.BudgetSupplement_Id = Guid.NewGuid().ToString();
                                                budgetSuppNew.AlternateServies_Id = AltSvc.AlternateServies_Id;
                                                budgetSuppNew.BookingRooms_Id = AltSvcRoom.BookingRooms_Id;
                                                budgetSuppNew.PositionPricing_Id = AltSvcRoom.PositionPricing_Id;
                                                budgetSuppNew.RoomShortCode = AltSvcRoom.RoomShortCode;
                                                budgetSuppNew.PersonType = AltSvcRoom.PersonType;
                                                budgetSuppNew.BudgetSupplementAmount = Math.Round(Convert.ToDecimal(BudgetSupplementAmount), 2);
                                                budgetSuppNew.BudgetSupplementReason = null;
                                                budgetSuppNew.BudgetSuppCurrencyId = positionRoom?.BuyCurrency_Id;
                                                budgetSuppNew.BudgetSuppCurrencyName = positionRoom?.BuyCurrency_Name;
                                                budgetSuppNew.ApplyMarkUp = false;
                                                budgetSuppNew.AgentConfirmed = false;
                                                budgetSuppNew.SupplementFor = "All Pax";
                                                budgetSuppNew.status = null;
                                                budgetSuppNew.CREA_DT = DateTime.Now;
                                                budgetSuppNew.MODI_DT = null;
                                                budgetSuppNew.CREA_US = request.UserEmailId;
                                                budgetSuppNew.MODI_US = null;

                                                if (AltSvc.BudgetSupplements == null)
                                                {
                                                    AltSvc.BudgetSupplements = new List<BudgetSupplements>();
                                                }
                                                AltSvc.BudgetSupplements.Add(budgetSuppNew);
                                            }
                                        }
                                    }
                                }
                                #endregion
                            }
                            response.BudgetSupplements = AltSvc.BudgetSupplements;
                            //}
                        }
                        #endregion
                    }
                    else
                    {
                        response.ResponseStatus.ErrorMessage = "Position not exists in Bookings.";
                        response.ResponseStatus.Status = "Error";
                    }
                }
                else
                {
                    response.ResponseStatus.ErrorMessage = "Booking Number not exists in Bookings.";
                    response.ResponseStatus.Status = "Error";
                }
                #endregion
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return response;
        }

        //Unavaliable, Avaliable Email
        public async Task<HotelReservationEmailRes> UpdateHotelAvailabilityRequest(AvailabilityRequestDetailsSetReq request)
        {
            HotelReservationEmailRes response = new HotelReservationEmailRes();
            Bookings resBooking = null;
            Products resProduct = null;
            mCompanies resCompany = null;
            CompanyContacts companyContact = null;
            BookingRoomsAndPrices BookingRoom = null;
            BookingRoomsAndPrices positionRoom = null;
            BudgetSupplements BudgetSupplements = null;
            string NewContactId = Guid.NewGuid().ToString();
            AltSvcRoomsAndPrices TwinRoom = null;
            var ProdSupp = new ProductSupplier();

            Bookings objBooking = null;
            Products objProduct = null;
            mCompanies objCompany = null;

            var mailRes = new ResponseStatus();
            mailRes.Status = "Success";
            EmailGetRes responseStatusMail = new EmailGetRes() { ResponseStatus = mailRes };

            try
            {
                #region get booking & mQRFPrice to fetch QRFID
                resBooking = await _MongoContext.Bookings.FindAsync(a => a.BookingNumber == request.BookingNumber).Result.FirstOrDefaultAsync();
                if (resBooking != null)
                {
                    var position = resBooking.Positions.Where(a => a.Position_Id == request.PositionId).FirstOrDefault();

                    if (position != null && position.AlternateServices.Count > 0)
                    {
                        var AltSvc = position.AlternateServices.Where(a => a.AlternateServies_Id == request.AltSvcId).FirstOrDefault();
                        if (AltSvc != null && AltSvc.SupplierInfo != null)
                        {
                            resCompany = await _MongoContext.mCompanies.FindAsync(a => a.Company_Id == AltSvc.SupplierInfo.Id).Result.FirstOrDefaultAsync();
                            resProduct = await _MongoContext.Products.FindAsync(a => a.VoyagerProduct_Id == AltSvc.Product_Id).Result.FirstOrDefaultAsync();
                            if (resCompany != null && resProduct != null)
                            {
                                #region create new contact row in mCompanies.ContactDetails
                                if (request.Status != "UNAVAILABLE")
                                {
                                    if (resCompany.ContactDetails == null) resCompany.ContactDetails = new List<CompanyContacts>();

                                    request.UpdateReqDetails.ReservationsEmail = request.UpdateReqDetails.ReservationsEmail?.ToLower().Trim();

                                    companyContact = resCompany.ContactDetails.Where(a => a.MAIL?.ToLower() == request.UpdateReqDetails.ReservationsEmail).FirstOrDefault();
                                    if (companyContact != null)
                                    {
                                        NewContactId = companyContact.Contact_Id;
                                        companyContact.CommonTitle = request.UpdateReqDetails.Title;
                                        companyContact.FIRSTNAME = request.UpdateReqDetails.FirstName;
                                        companyContact.LastNAME = request.UpdateReqDetails.LastName;
                                        companyContact.TEL = request.UpdateReqDetails.Telephone;
                                        companyContact.MAIL = request.UpdateReqDetails.ReservationsEmail;
                                        companyContact.EditDate = DateTime.Now;
                                        companyContact.EditUser = request.Caller == "ui" ? request.UserEmailId : "HotelAvailabilityRequestUpdate";
                                    }
                                    else
                                    {
                                        resCompany.ContactDetails.Add(new CompanyContacts()
                                        {
                                            Contact_Id = NewContactId,
                                            Company_Id = resCompany.Company_Id,
                                            Company_Name = resCompany.Name,
                                            Default = 0,
                                            CommonTitle = request.UpdateReqDetails.Title,
                                            FIRSTNAME = request.UpdateReqDetails.FirstName,
                                            LastNAME = request.UpdateReqDetails.LastName,
                                            TEL = request.UpdateReqDetails.Telephone,
                                            MAIL = request.UpdateReqDetails.ReservationsEmail,
                                            CreateDate = DateTime.Now,
                                            CreateUser = request.Caller == "ui" ? request.UserEmailId : "HotelAvailabilityRequestUpdate"
                                        });
                                    }

                                    objCompany = await _MongoContext.mCompanies.FindOneAndUpdateAsync(a => a.Company_Id == AltSvc.SupplierInfo.Id,
                                           Builders<mCompanies>.Update.Set(a => a.ContactDetails, resCompany.ContactDetails));
                                }
                                #endregion

                                #region update details in Booking.Positions.AlternateServices
                                if (request.Status != "UNAVAILABLE" && request.UpdateReqDetails != null)
                                {
                                    AltSvc.Availability_Status = request.Status == "SAVE" ? AltSvc.Availability_Status : request.Status;
                                    AltSvc.OptionDate = request.UpdateReqDetails.OptionDate;
                                    AltSvc.CancellationDeadline = request.UpdateReqDetails.CancellationDeadline?.ToString();
                                    AltSvc.SupplierInfo.Contact_Id = NewContactId;
                                    AltSvc.SupplierInfo.Contact_Name = request.UpdateReqDetails.FirstName + " " + request.UpdateReqDetails.LastName;
                                    AltSvc.SupplierInfo.Contact_Email = request.UpdateReqDetails.ReservationsEmail;
                                    AltSvc.SupplierInfo.Contact_Tel = request.UpdateReqDetails.Telephone;

                                    if (request.Status != "SAVE")
                                    {
                                        TwinRoom = request.AltSvcRoomsAndPrices.Where(a => a.RoomName == "TWIN").FirstOrDefault();
                                        if (TwinRoom != null && !string.IsNullOrEmpty(TwinRoom.RoomRate))
                                            AltSvc.PPTwin_Rate = Convert.ToDecimal(TwinRoom.RoomRate);

                                        foreach (var AltSvcRoom in request.AltSvcRoomsAndPrices)
                                        {
                                            BookingRoom = AltSvc.Request_RoomsAndPrices.Where(a => a.BookingRooms_Id == AltSvcRoom.BookingRoomsId).FirstOrDefault();
                                            BookingRoom.BuyCurrency_Id = AltSvcRoom.CurrencyId;
                                            BookingRoom.BuyCurrency_Name = AltSvcRoom.CurrencyCode;
                                            BookingRoom.BuyPrice = Convert.ToDecimal(AltSvcRoom.RoomRate);
                                            BookingRoom.BuyCurrency_Id = AltSvcRoom.CurrencyId;
                                        }
                                        #region BudgetSupplement
                                        //Deleting Records
                                        if (AltSvc.BudgetSupplements != null)
                                        {
                                            if (request.BudgetSupplements != null)
                                            {
                                                foreach (var BdgSupp in AltSvc.BudgetSupplements)
                                                {
                                                    BudgetSupplements = request.BudgetSupplements.Where(a => a.BudgetSupplement_Id == BdgSupp.BudgetSupplement_Id).FirstOrDefault();
                                                    if (BudgetSupplements == null)
                                                    {
                                                        BdgSupp.status = "X";
                                                        BdgSupp.MODI_DT = DateTime.Now;
                                                        BdgSupp.MODI_US = request.UserEmailId;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                foreach (var BdgSupp in AltSvc.BudgetSupplements)
                                                {
                                                    BdgSupp.status = "X";
                                                    BdgSupp.MODI_DT = DateTime.Now;
                                                    BdgSupp.MODI_US = request.UserEmailId;
                                                }
                                            }
                                        }

                                        //Adding/Updating records
                                        if (request.BudgetSupplements != null)
                                        {
                                            foreach (var BdgSupp in request.BudgetSupplements)
                                            {
                                                BudgetSupplements = AltSvc.BudgetSupplements?.Where(a => a.BudgetSupplement_Id == BdgSupp.BudgetSupplement_Id).FirstOrDefault();

                                                if (BudgetSupplements == null)
                                                {
                                                    BookingRoom = AltSvc.Request_RoomsAndPrices.Where(a => a.BookingRooms_Id == BdgSupp.BookingRooms_Id).FirstOrDefault();
                                                    positionRoom = position.BookingRoomsAndPrices.Where(a => a.RoomName == BookingRoom?.RoomName && a.PersonType == BookingRoom?.PersonType).FirstOrDefault();

                                                    var budgetSuppNew = new BudgetSupplements();
                                                    budgetSuppNew.BudgetSupplement_Id = Guid.NewGuid().ToString();
                                                    budgetSuppNew.AlternateServies_Id = AltSvc.AlternateServies_Id;
                                                    budgetSuppNew.BookingRooms_Id = BookingRoom?.BookingRooms_Id;
                                                    budgetSuppNew.PositionPricing_Id = BookingRoom?.PositionPricing_Id;
                                                    budgetSuppNew.RoomShortCode = BookingRoom?.RoomShortCode;
                                                    budgetSuppNew.PersonType = BookingRoom?.PersonType;
                                                    budgetSuppNew.BudgetSupplementAmount = BdgSupp.BudgetSupplementAmount;
                                                    budgetSuppNew.BudgetSupplementReason = BdgSupp.BudgetSupplementReason;
                                                    budgetSuppNew.BudgetSuppCurrencyId = positionRoom?.BuyCurrency_Id;
                                                    budgetSuppNew.BudgetSuppCurrencyName = positionRoom?.BuyCurrency_Name;
                                                    budgetSuppNew.ApplyMarkUp = BdgSupp.ApplyMarkUp;
                                                    budgetSuppNew.AgentConfirmed = BdgSupp.AgentConfirmed;
                                                    budgetSuppNew.SupplementFor = "All Pax";
                                                    budgetSuppNew.status = null;
                                                    budgetSuppNew.CREA_DT = DateTime.Now;
                                                    budgetSuppNew.MODI_DT = null;
                                                    budgetSuppNew.CREA_US = request.UserEmailId;
                                                    budgetSuppNew.MODI_US = null;

                                                    if (AltSvc.BudgetSupplements == null)
                                                    {
                                                        AltSvc.BudgetSupplements = new List<BudgetSupplements>();
                                                    }
                                                    AltSvc.BudgetSupplements.Add(budgetSuppNew);
                                                }
                                                else
                                                {
                                                    BudgetSupplements.BudgetSupplementAmount = BdgSupp.BudgetSupplementAmount;
                                                    BudgetSupplements.BudgetSupplementReason = BdgSupp.BudgetSupplementReason;
                                                    BudgetSupplements.ApplyMarkUp = BdgSupp.ApplyMarkUp;
                                                    BudgetSupplements.AgentConfirmed = BdgSupp.AgentConfirmed;
                                                    BudgetSupplements.MODI_DT = DateTime.Now;
                                                    BudgetSupplements.MODI_US = request.UserEmailId;
                                                }
                                            }
                                        }
                                        #endregion
                                    }
                                }
                                else
                                {
                                    AltSvc.Availability_Status = request.Status == "SAVE" ? AltSvc.Availability_Status : request.Status;
                                }

                                objBooking = await _MongoContext.Bookings.FindOneAndUpdateAsync(a => a.BookingNumber == request.BookingNumber &&
                                          a.Positions.Any(b => b.Position_Id == request.PositionId), Builders<Bookings>.Update.Set(a => a.Positions[-1], position));
                                #endregion

                                #region update contact in Products.ProductSuppliers
                                if (request.Status != "UNAVAILABLE")
                                {
                                    ProdSupp = resProduct.ProductSuppliers.Where(a => a.Company_Id == AltSvc.SupplierInfo.Id).FirstOrDefault();
                                    ProdSupp.Contact_Group_Id = NewContactId;
                                    ProdSupp.Contact_Group_Name = request.UpdateReqDetails.FirstName + " " + request.UpdateReqDetails.LastName;
                                    ProdSupp.Contact_Group_Email = request.UpdateReqDetails.ReservationsEmail;

                                    objProduct = await _MongoContext.Products.FindOneAndUpdateAsync(a => a.VoyagerProduct_Id == AltSvc.Product_Id
                                              && a.ProductSuppliers.Any(b => b.Company_Id == AltSvc.SupplierInfo.Id),
                                              Builders<Products>.Update.Set(a => a.ProductSuppliers[-1], ProdSupp));

                                }
                                #endregion

                                #region Unavaliable Email
                                //Jira ticket 699                                
                                if (request.Status == "UNAVAILABLE" && request.Caller == "email")
                                {
                                    var objEmail = _MongoContext.mDocumentStore.AsQueryable().Where(a => a.BookingNumber == request.BookingNumber && a.PositionId == request.PositionId
                                                        && a.AlternateServiceId == request.AltSvcId && a.DocumentType == DocType.BOOKREQ).OrderByDescending(a => a.SendDate).FirstOrDefault();
                                    if (objEmail != null)
                                    {
                                        var objEmailGetReq = new EmailGetReq()
                                        {
                                            AlternateServiceId = request.AltSvcId,
                                            BookingNo = request.BookingNumber,
                                            DocumentType = DocType.HOTELNOTAVAILABLE,
                                            PositionId = request.PositionId,
                                            PlacerUserId = objEmail.VoyagerUser_Id,
                                            UserEmail = objEmail.From,
                                            SupplierId = position.SupplierInfo.Id,
                                            QrfId = resBooking.QRFID

                                        };
                                        responseStatusMail = await _emailRepository.GenerateEmail(objEmailGetReq);
                                        if (responseStatusMail == null || responseStatusMail.ResponseStatus == null || string.IsNullOrEmpty(responseStatusMail.ResponseStatus.Status))
                                        {
                                            responseStatusMail.ResponseStatus = new ResponseStatus();
                                            responseStatusMail.ResponseStatus.Status = "Error";
                                            responseStatusMail.ResponseStatus.ErrorMessage = "Mail not sent.";
                                        }
                                    }
                                    else
                                    {
                                        responseStatusMail.ResponseStatus.Status = "Error";
                                        responseStatusMail.ResponseStatus.ErrorMessage = "Mail not sent for Hotel Request.";
                                    }
                                }
                                #endregion

                                #region Avaliable Email
                                //Jira ticket 724   
                                if (request.Status == "AVAILABLE" && request.Caller == "email")
                                {
                                    var objEmail = _MongoContext.mDocumentStore.AsQueryable().Where(a => a.BookingNumber == request.BookingNumber && a.PositionId == request.PositionId
                                                        && a.AlternateServiceId == request.AltSvcId && a.DocumentType == DocType.BOOKREQ).OrderByDescending(a => a.SendDate).FirstOrDefault();
                                    if (objEmail != null)
                                    {
                                        var objEmailGetReq = new EmailGetReq()
                                        {
                                            AlternateServiceId = request.AltSvcId,
                                            BookingNo = request.BookingNumber,
                                            DocumentType = DocType.HOTELAVAILABLE,
                                            PositionId = request.PositionId,
                                            PlacerUserId = objEmail.VoyagerUser_Id,
                                            UserEmail = objEmail.From,
                                            SupplierId = position.SupplierInfo.Id,
                                            QrfId = resBooking.QRFID
                                        };
                                        responseStatusMail = await _emailRepository.GenerateEmail(objEmailGetReq);
                                        if (responseStatusMail == null || responseStatusMail.ResponseStatus == null || string.IsNullOrEmpty(responseStatusMail.ResponseStatus.Status))
                                        {
                                            responseStatusMail.ResponseStatus = new ResponseStatus();
                                            responseStatusMail.ResponseStatus.Status = "Error";
                                            responseStatusMail.ResponseStatus.ErrorMessage = "Mail not sent.";
                                        }
                                    }
                                    else
                                    {
                                        responseStatusMail.ResponseStatus.Status = "Error";
                                        responseStatusMail.ResponseStatus.ErrorMessage = "Mail not sent for Hotel Request.";
                                    }
                                }
                                #endregion

                                #region Bridge Services   
                                if (objBooking != null)
                                {
                                    if (responseStatusMail != null && !string.IsNullOrEmpty(responseStatusMail.ResponseStatus.Status) && responseStatusMail.ResponseStatus.Status.ToLower() == "success")
                                    {
                                        ResponseStatus rsCompany = new ResponseStatus();
                                        ResponseStatus rsProdSupp = new ResponseStatus();
                                        if (request.Status != "UNAVAILABLE")
                                        {
                                            //The below Bridge service will UPSERT the Conatct details in Conatcts table in SQL
                                            rsCompany = await _companyProviders.SetCompanyContact(new SetCompanyContact_RQ()
                                            {
                                                Contact_Id = NewContactId,
                                                User = request.UserEmailId
                                            });

                                            //2)for updating supplier info into ProductSupplier SQL table
                                            //The below Bridge service will UPSERT the Conatct details in Conatcts table in SQL
                                            rsProdSupp = await _companyProviders.SetProductSuppliers(new SetProduct_RQ()
                                            {
                                                ProductSupplier_Id = ProdSupp.ProductSupplier_Id,
                                                User = request.UserEmailId
                                            });
                                        }

                                        //3)The below Bridge service will UPSERT the contacting details & Status of Alternate service in PositionRequests table in SQL
                                        ResponseStatus rsAltServ = await _bookingProviders.SetBookingAlternateServices(new BookingPosAltSetReq()
                                        {
                                            BookingNumber = request.BookingNumber,
                                            PositionId = request.PositionId,
                                            User = request.UserEmailId
                                        });

                                        if (request.Status != "UNAVAILABLE" && rsCompany != null && rsProdSupp != null && rsAltServ != null && rsCompany.Status == "Success" && rsProdSupp.Status == "Success" && rsAltServ.Status == "Success")
                                        {
                                            response.ResponseStatus.ErrorMessage = "Request Updated Successfully.";
                                            response.ResponseStatus.Status = "Success";
                                        }
                                        else if (request.Status == "UNAVAILABLE" && rsAltServ != null && rsAltServ.Status == "Success")
                                        {
                                            response.ResponseStatus.ErrorMessage = "Request Updated Successfully.";
                                            response.ResponseStatus.Status = "Success";
                                        }
                                        else
                                        {
                                            response.ResponseStatus.ErrorMessage = "Request Not Updated.";
                                            response.ResponseStatus.Status = "Error";
                                        }
                                    }
                                    else
                                    {
                                        response.ResponseStatus.ErrorMessage = responseStatusMail.ResponseStatus.ErrorMessage;
                                        response.ResponseStatus.Status = "Error";
                                    }
                                }
                                else
                                {
                                    response.ResponseStatus.ErrorMessage = "Request Not Updated.";
                                    response.ResponseStatus.Status = "Error";
                                }
                                #endregion
                            }
                            else
                            {
                                response.ResponseStatus.ErrorMessage = "Company not exists.";
                                response.ResponseStatus.Status = "Error";
                            }
                        }
                        else
                        {
                            response.ResponseStatus.ErrorMessage = "Alternate hotel not exists.";
                            response.ResponseStatus.Status = "Error";
                        }
                    }
                    else
                    {
                        response.ResponseStatus.ErrorMessage = "Position not exists.";
                        response.ResponseStatus.Status = "Error";
                    }
                }
                else
                {
                    response.ResponseStatus.ErrorMessage = "Booking not found.";
                    response.ResponseStatus.Status = "Error";
                }
                #endregion

            }
            catch (System.Exception ex)
            {
                response.ResponseStatus.ErrorMessage = ex.Message;
                response.ResponseStatus.Status = "Error";
            }

            return response;
        }

        public async Task<BudgetSupplementGetRes> GetBudgetSupplement(BudgetSupplementGetReq request)
        {
            BudgetSupplementGetRes response = new BudgetSupplementGetRes();
            Bookings resBooking = null;
            BookingRoomsAndPrices BookingRoom = null;

            resBooking = await _MongoContext.Bookings.FindAsync(a => a.BookingNumber == request.BookingNumber).Result.FirstOrDefaultAsync();

            if (resBooking != null)
            {
                var position = resBooking.Positions.Where(a => a.Position_Id == request.PositionId).FirstOrDefault();

                if (position != null && position.AlternateServices.Count > 0)
                {
                    var AltSvc = position.AlternateServices.Where(a => a.AlternateServies_Id == request.AltSvcId).FirstOrDefault();
                    if (AltSvc != null)
                    {
                        foreach (var AltSvcRoom in request.AltSvcRoomsAndPrices)
                        {
                            BookingRoom = AltSvc.Request_RoomsAndPrices.Where(a => a.BookingRooms_Id == AltSvcRoom.BookingRoomsId).FirstOrDefault();

                            #region BudgetSupplement
                            if (BookingRoom != null)
                            {
                                var positionRoom = position.BookingRoomsAndPrices.Where(a => a.RoomName == BookingRoom.RoomName && a.PersonType == BookingRoom.PersonType).FirstOrDefault();
                                if (positionRoom != null)
                                {
                                    decimal? BudgetSupplementAmount = 0;
                                    if (AltSvcRoom.CurrencyId == positionRoom.BuyCurrency_Id)
                                    {
                                        BudgetSupplementAmount = Convert.ToDecimal(AltSvcRoom.RoomRate) - positionRoom.BudgetPrice;
                                    }
                                    else
                                    {
                                        var BdgCurrency = resBooking.ExchangeRateSnapshot.ExchangeRateDetail.Where(a => a.Currency_Id == positionRoom.BuyCurrency_Id).FirstOrDefault();
                                        var BuyCurrency = resBooking.ExchangeRateSnapshot.ExchangeRateDetail.Where(a => a.Currency_Id == AltSvcRoom.CurrencyId).FirstOrDefault();

                                        if (BdgCurrency != null && BuyCurrency != null)
                                        {
                                            decimal Rate = Convert.ToDecimal(BuyCurrency.RATE / BdgCurrency.RATE);
                                            AltSvcRoom.RoomRate = (Convert.ToDecimal(AltSvcRoom.RoomRate) / Rate).ToString();
                                        }
                                        BudgetSupplementAmount = Convert.ToDecimal(AltSvcRoom.RoomRate) - positionRoom.BudgetPrice;
                                    }

                                    if (BudgetSupplementAmount > 0)
                                    {
                                        var BudgetSupplements = AltSvc.BudgetSupplements?.Where(a => a.RoomShortCode == positionRoom.RoomShortCode && a.PersonType == positionRoom.PersonType).FirstOrDefault();
                                        if (BudgetSupplements == null)
                                        {
                                            var budgetSuppNew = new BudgetSupplements();
                                            budgetSuppNew.BudgetSupplement_Id = Guid.NewGuid().ToString();
                                            budgetSuppNew.AlternateServies_Id = AltSvc.AlternateServies_Id;
                                            budgetSuppNew.BookingRooms_Id = BookingRoom?.BookingRooms_Id;
                                            budgetSuppNew.PositionPricing_Id = BookingRoom?.PositionPricing_Id;
                                            budgetSuppNew.RoomShortCode = BookingRoom?.RoomShortCode;
                                            budgetSuppNew.PersonType = BookingRoom?.PersonType;
                                            budgetSuppNew.BudgetSupplementAmount = Math.Round(Convert.ToDecimal(BudgetSupplementAmount), 2);
                                            budgetSuppNew.BudgetSupplementReason = null;
                                            budgetSuppNew.BudgetSuppCurrencyId = positionRoom?.BuyCurrency_Id;
                                            budgetSuppNew.BudgetSuppCurrencyName = positionRoom?.BuyCurrency_Name;
                                            budgetSuppNew.ApplyMarkUp = false;
                                            budgetSuppNew.AgentConfirmed = false;
                                            budgetSuppNew.SupplementFor = "All Pax";
                                            budgetSuppNew.status = null;
                                            budgetSuppNew.CREA_DT = DateTime.Now;
                                            budgetSuppNew.MODI_DT = null;
                                            budgetSuppNew.CREA_US = request.UserEmailId;
                                            budgetSuppNew.MODI_US = null;

                                            if (AltSvc.BudgetSupplements == null)
                                            {
                                                AltSvc.BudgetSupplements = new List<BudgetSupplements>();
                                            }
                                            response.BudgetSupplements.Add(budgetSuppNew);
                                        }
                                        else
                                        {
                                            BudgetSupplements.BudgetSupplementAmount = Math.Round(Convert.ToDecimal(BudgetSupplementAmount), 2);
                                            BudgetSupplements.MODI_DT = DateTime.Now;
                                            BudgetSupplements.MODI_US = request.UserEmailId;
                                            response.BudgetSupplements.Add(BudgetSupplements);
                                        }
                                    }
                                }
                            }
                            response.ResponseStatus.Status = "Success";
                            #endregion
                        }
                    }
                    else
                    {
                        response.ResponseStatus.ErrorMessage = "Alternate hotel not exists.";
                        response.ResponseStatus.Status = "Error";
                    }
                }
                else
                {
                    response.ResponseStatus.ErrorMessage = "Position not exists.";
                    response.ResponseStatus.Status = "Error";
                }
            }
            else
            {
                response.ResponseStatus.ErrorMessage = "Booking not found.";
                response.ResponseStatus.Status = "Error";
            }
            return response;
        }

        public async Task<CommonResponse> SetBudgetSupplement(BudgetSupplementSetReq request)
        {
            CommonResponse response = new CommonResponse();
            Bookings resBooking = null;
            BookingRoomsAndPrices BookingRoom = null;
            BookingRoomsAndPrices positionRoom = null;
            BudgetSupplements BudgetSupplements = null;

            resBooking = await _MongoContext.Bookings.FindAsync(a => a.BookingNumber == request.BookingNumber).Result.FirstOrDefaultAsync();

            if (resBooking != null)
            {
                var position = resBooking.Positions.Where(a => a.Position_Id == request.PositionId).FirstOrDefault();

                if (position != null && position.AlternateServices.Count > 0)
                {
                    var AltSvc = position.AlternateServices.Where(a => a.AlternateServies_Id == request.AltSvcId).FirstOrDefault();
                    if (AltSvc != null)
                    {
                        #region BudgetSupplement
                        //Deleting Records
                        if (AltSvc.BudgetSupplements != null)
                        {
                            if (request.BudgetSupplements != null)
                            {
                                foreach (var BdgSupp in AltSvc.BudgetSupplements)
                                {
                                    BudgetSupplements = request.BudgetSupplements.Where(a => a.BudgetSupplement_Id == BdgSupp.BudgetSupplement_Id).FirstOrDefault();
                                    if (BudgetSupplements == null)
                                    {
                                        BdgSupp.status = "X";
                                        BdgSupp.MODI_DT = DateTime.Now;
                                        BdgSupp.MODI_US = request.UserEmailId;
                                    }
                                }
                            }
                            else
                            {
                                foreach (var BdgSupp in AltSvc.BudgetSupplements)
                                {
                                    BdgSupp.status = "X";
                                    BdgSupp.MODI_DT = DateTime.Now;
                                    BdgSupp.MODI_US = request.UserEmailId;
                                }
                            }
                        }

                        //Adding/Updating records
                        if (request.BudgetSupplements != null)
                        {
                            foreach (var BdgSupp in request.BudgetSupplements)
                            {
                                BudgetSupplements = AltSvc.BudgetSupplements?.Where(a => a.BudgetSupplement_Id == BdgSupp.BudgetSupplement_Id).FirstOrDefault();
                                if (BudgetSupplements == null)
                                {
                                    BookingRoom = AltSvc.Request_RoomsAndPrices.Where(a => a.BookingRooms_Id == BdgSupp.BookingRooms_Id).FirstOrDefault();
                                    positionRoom = position.BookingRoomsAndPrices.Where(a => a.RoomName == BookingRoom?.RoomName && a.PersonType == BookingRoom?.PersonType).FirstOrDefault();

                                    var budgetSuppNew = new BudgetSupplements();
                                    budgetSuppNew.BudgetSupplement_Id = Guid.NewGuid().ToString();
                                    budgetSuppNew.AlternateServies_Id = AltSvc.AlternateServies_Id;
                                    budgetSuppNew.BookingRooms_Id = BookingRoom?.BookingRooms_Id;
                                    budgetSuppNew.PositionPricing_Id = BookingRoom?.PositionPricing_Id;
                                    budgetSuppNew.RoomShortCode = BookingRoom?.RoomShortCode;
                                    budgetSuppNew.PersonType = BookingRoom?.PersonType;
                                    budgetSuppNew.BudgetSupplementAmount = BdgSupp.BudgetSupplementAmount;
                                    budgetSuppNew.BudgetSupplementReason = BdgSupp.BudgetSupplementReason;
                                    budgetSuppNew.BudgetSuppCurrencyId = positionRoom?.BuyCurrency_Id;
                                    budgetSuppNew.BudgetSuppCurrencyName = positionRoom?.BuyCurrency_Name;
                                    budgetSuppNew.ApplyMarkUp = BdgSupp.ApplyMarkUp;
                                    budgetSuppNew.AgentConfirmed = BdgSupp.AgentConfirmed;
                                    budgetSuppNew.SupplementFor = "All Pax";
                                    budgetSuppNew.status = null;
                                    budgetSuppNew.CREA_DT = DateTime.Now;
                                    budgetSuppNew.MODI_DT = null;
                                    budgetSuppNew.CREA_US = request.UserEmailId;
                                    budgetSuppNew.MODI_US = null;

                                    if (AltSvc.BudgetSupplements == null)
                                    {
                                        AltSvc.BudgetSupplements = new List<BudgetSupplements>();
                                    }
                                    AltSvc.BudgetSupplements.Add(budgetSuppNew);
                                }
                                else
                                {
                                    BudgetSupplements.BudgetSupplementAmount = BdgSupp.BudgetSupplementAmount;
                                    BudgetSupplements.BudgetSupplementReason = BdgSupp.BudgetSupplementReason;
                                    BudgetSupplements.ApplyMarkUp = BdgSupp.ApplyMarkUp;
                                    BudgetSupplements.AgentConfirmed = BdgSupp.AgentConfirmed;
                                    BudgetSupplements.MODI_DT = DateTime.Now;
                                    BudgetSupplements.MODI_US = request.UserEmailId;
                                }
                            }
                        }

                        var res = await _MongoContext.Bookings.FindOneAndUpdateAsync(a => a.BookingNumber == request.BookingNumber &&
                                          a.Positions.Any(b => b.Position_Id == request.PositionId), Builders<Bookings>.Update.Set(a => a.Positions[-1], position));
                        if (res != null)
                            response.ResponseStatus.Status = "Success";
                        else
                            response.ResponseStatus.Status = "Error";
                        #endregion
                    }
                    else
                    {
                        response.ResponseStatus.ErrorMessage = "Alternate hotel not exists.";
                        response.ResponseStatus.Status = "Error";
                    }
                }
                else
                {
                    response.ResponseStatus.ErrorMessage = "Position not exists.";
                    response.ResponseStatus.Status = "Error";
                }
            }
            else
            {
                response.ResponseStatus.ErrorMessage = "Booking not found.";
                response.ResponseStatus.Status = "Error";
            }
            return response;
        }

        public async Task<ActivateHotelDetailsGetRes> GetHotelActivationDetails(AvailabilityRequestDetailsGetReq request)
        {
            ActivateHotelDetailsGetRes response = new ActivateHotelDetailsGetRes();
            try
            {
                List<mProducts_Lite> resSRP = null;
                string RoomingDetails = "", PositionTwinRate = "";
                BookingRoomsAndPrices TwinRoom;
                string[] StatusIgnoreList = new string[] { "N", "J", "I", "B", "C", "-", "^", "L", "S", "X", "T" };

                var resBooking = _MongoContext.Bookings.AsQueryable().Where(a => a.BookingNumber == request.BookingNumber).FirstOrDefault();
                var position = resBooking.Positions.Where(a => a.Position_Id == request.PositionId && a.ProductType.Trim() == "Hotel" && !StatusIgnoreList.Contains(a.STATUS)).FirstOrDefault();
                var AltSvc = position.AlternateServices.Where(a => a.AlternateServies_Id == request.AltSvcId).FirstOrDefault();

                #region assign responses of ProductSRPDetails, ReservationRequestDetails
                if (resBooking != null)
                {
                    if (position != null)
                    {
                        #region get and assign product srp details of the product
                        resSRP = _MongoContext.mProducts_Lite.AsQueryable().Where(a => a.VoyagerProduct_Id == AltSvc.Product_Id || a.VoyagerProduct_Id == position.Product_Id).ToList();
                        response.PosProductSRPDetails = resSRP.Where(a => a.VoyagerProduct_Id == position.Product_Id).Select(a => new ProductSRPDetails
                        {
                            Address = a.Address,
                            BdgPriceCategory = a.BdgPriceCategory,
                            Chain = a.Chain,
                            CityName = a.CityName,
                            CountryName = a.CountryName,
                            CreateDate = a.CreateDate,
                            CreateUser = a.CreateUser,
                            DefaultSupplier = a.DefaultSupplier,
                            EditDate = a.EditDate,
                            EditUser = a.EditUser,
                            HotelImageURL = a.HotelImageURL,
                            HotelType = a.HotelType,
                            Location = a.Location,
                            Placeholder = a.Placeholder,
                            PostCode = a.PostCode,
                            ProdDesc = a.ProdDesc,
                            ProdName = a.ProdName,
                            ProductCode = a.ProductCode,
                            ProductFacilities = a.ProductFacilities,
                            ProductType = a.ProductType,
                            ProductType_Id = a.ProductType_Id,
                            Rooms = a.Rooms,
                            StarRating = a.StarRating,
                            Status = a.Status,
                            Street = a.Street,
                            VoyagerProduct_Id = a.VoyagerProduct_Id
                        }).ToList();
                        response.AltProductSRPDetails = resSRP.Where(a => a.VoyagerProduct_Id == AltSvc.Product_Id).Select(a => new ProductSRPDetails
                        {
                            Address = a.Address,
                            BdgPriceCategory = a.BdgPriceCategory,
                            Chain = a.Chain,
                            CityName = a.CityName,
                            CountryName = a.CountryName,
                            CreateDate = a.CreateDate,
                            CreateUser = a.CreateUser,
                            DefaultSupplier = a.DefaultSupplier,
                            EditDate = a.EditDate,
                            EditUser = a.EditUser,
                            HotelImageURL = a.HotelImageURL,
                            HotelType = a.HotelType,
                            Location = a.Location,
                            Placeholder = a.Placeholder,
                            PostCode = a.PostCode,
                            ProdDesc = a.ProdDesc,
                            ProdName = a.ProdName,
                            ProductCode = a.ProductCode,
                            ProductFacilities = a.ProductFacilities,
                            ProductType = a.ProductType,
                            ProductType_Id = a.ProductType_Id,
                            Rooms = a.Rooms,
                            StarRating = a.StarRating,
                            Status = a.Status,
                            Street = a.Street,
                            VoyagerProduct_Id = a.VoyagerProduct_Id
                        }).ToList();
                        #endregion

                        #region get and assign Reservation Request Details
                        response.ReservationRequestDetails = new ReservationRequestDetails
                        {
                            Check_In = Convert.ToDateTime(position.STARTDATE).ToString("MMMM dd"),
                            Check_Out_Date = Convert.ToDateTime(position.ENDDATE).ToString("MMMM dd"),
                            Nights = position.DURATION,
                            Nationality = resBooking.GuestDetails.Nationality_Name,
                            Board_Basis = position.HOTELMEALPLAN,
                            Stars = position.HotelStarRating != null && position.HotelStarRating > 0 ? position.HotelStarRating : 0,
                            Location = position.Attributes != null ? position.Attributes.Location : "",
                            Category = position.Attributes != null ? position.Attributes.BdgPriceCategory : ""
                        };
                        #endregion

                        #region get and assign Position Product Details
                        foreach (var Room in position.BookingRoomsAndPrices)
                        {
                            if (Room != null && !string.IsNullOrEmpty(Room.RoomName) && !string.IsNullOrEmpty(Room.PersonType))
                            {
                                if (RoomingDetails != "") RoomingDetails += "+ ";
                                if (Room.RoomName.ToLower() == "child")
                                    RoomingDetails += (Room.Req_Count + "(" + Room.PersonType + ")");
                                else
                                    RoomingDetails += (Room.Req_Count + Room.RoomName);
                            }
                        }

                        TwinRoom = position.BookingRoomsAndPrices.Where(a => a.RoomName == "TWIN").FirstOrDefault();
                        if (TwinRoom != null && TwinRoom.BuyPrice != null && TwinRoom.BuyPrice > 0)
                            PositionTwinRate = TwinRoom.BuyCurrency_Name + " " + TwinRoom.BuyPrice.ToString();

                        var mStatus = await _MongoContext.mStatus.FindAsync(a => a.Status == position.STATUS).Result.FirstOrDefaultAsync();
                        response.PositionProductDetails = new PositionProductDetails
                        {
                            ProductName = position.Product_Name,
                            ProductCity = position.City,
                            ProductCountry = position.Country,
                            PositionRooms = RoomingDetails,
                            PositionTwinRate = PositionTwinRate,
                            PositionStatus = mStatus?.Description
                        };
                        #endregion

                        #region get and assign Alternate Services Rooms And Prices
                        if (AltSvc != null)
                        {
                            response.UpdateReqDetails.OptionDate = AltSvc.OptionDate;
                            response.UpdateReqDetails.CancellationDeadline = Convert.ToInt32(AltSvc.CancellationDeadline);
                            response.UpdateReqDetails.Availability = AltSvc.Availability_Status;
                            response.AltSvcRoomsAndPrices = AltSvc.Request_RoomsAndPrices.Select(a => new AltSvcRoomsAndPrices
                            {
                                BookingRoomsId = a.BookingRooms_Id,
                                RoomName = a.RoomName,
                                PersonType = a.PersonType,
                                Quantity = Convert.ToString(a.Req_Count),
                                CurrencyCode = a.BuyCurrency_Name,
                                RoomRate = Convert.ToString(a.BuyPrice)
                            }).ToList();
                        }
                        #endregion

                        response.ResponseStatus.Status = "Success";
                    }
                    else
                    {
                        response.ResponseStatus.ErrorMessage = "Position not exists in Bookings.";
                        response.ResponseStatus.Status = "Error";
                    }
                }
                else
                {
                    response.ResponseStatus.ErrorMessage = "Booking Number not exists in Bookings.";
                    response.ResponseStatus.Status = "Error";
                }
                #endregion
                return response;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                return response;
            }
        }

        public async Task<HotelReservationEmailRes> UpdateHotelActivationDetails(AvailabilityRequestDetailsGetReq request)
        {
            HotelReservationEmailRes response = new HotelReservationEmailRes();
            string NewPosId = null;
            try
            {
                bool isPosCancelled = false;
                EmailGetRes responseStatusMail = null;
                string[] StatusIgnoreList = new string[] { "N", "J", "I", "B", "C", "-", "^", "L", "S", "X", "T" };
                var resBooking = await _MongoContext.Bookings.Find(a => a.BookingNumber == request.BookingNumber).FirstOrDefaultAsync();
                var position = resBooking.Positions.Where(a => a.Position_Id == request.PositionId && a.ProductType.Trim() == "Hotel" && !StatusIgnoreList.Contains(a.STATUS)).FirstOrDefault();
                if (position != null)
                {
                    position.OPTIONDATE = !string.IsNullOrEmpty(request.OptionDate) ? (DateTime?)Convert.ToDateTime(request.OptionDate) : null;
                    if (!string.IsNullOrEmpty(request.CancellationDeadline))
                    {
                        var cancelTimeSpan = position.STARTDATE.Value.AddDays(-Convert.ToInt32(request.CancellationDeadline));
                        position.CancellationDate = cancelTimeSpan;
                    }
                    var objBooking = await _MongoContext.Bookings.FindOneAndUpdateAsync(a => a.BookingNumber == request.BookingNumber &&
                                          a.Positions.Any(b => b.Position_Id == request.PositionId), Builders<Bookings>.Update.Set(a => a.Positions[-1], position));
                    if (objBooking != null)
                    {
                        var AltSvc = position.AlternateServices.Where(a => a.AlternateServies_Id == request.AltSvcId).FirstOrDefault();
                        string GridInfo = string.Format("{0} {1} {2} - {3} {4}", position.OrderNr, position.ProductType, position.STARTDATE?.ToString("dd/MM/yy"), position.ENDDATE?.ToString("dd/MM/yy"), AltSvc.Product_Name);

                        #region return null response if Booking, Position or Alt Svc is null
                        if (resBooking == null || position == null || AltSvc == null)
                        {
                            response.ResponseStatus.ErrorMessage = "Booking, Position or Alt Svc can not be null";
                            response.ResponseStatus.Status = "Failure";
                            return response;
                        }
                        #endregion

                        #region Main Crud functionality
                        if ("QPEOIBKM!".Contains(position.STATUS))
                        {
                            //If supplier position is not locked then only switch the product else do not switch
                            if (position.IsSuppPosLocked == null || position.IsSuppPosLocked == false)
                            {
                                if ("QPE".Contains(position.STATUS))
                                {
                                    //if status in [Q,P,E] : Directly Update Position From Alt Svc in mongo
                                    resBooking = DirectUpdatePositionFromAltSvc(request.BookingNumber, request.MailType, position, AltSvc, request.UserEmailId, GridInfo);
                                    if (resBooking == null)
                                    {
                                        response.ResponseStatus.ErrorMessage = "Unable to switch Hotel";
                                        response.ResponseStatus.Status = "Failure";
                                    }
                                    else
                                    {
                                        response.ResponseStatus.ErrorMessage = "Hotels switched successfully";
                                        response.ResponseStatus.Status = "Success";
                                    }
                                }
                                else if (position.STATUS == "O")
                                {
                                    //if status in [O] : 
                                    //1)Send BOOK_OPTXX document email. 2) Directly Update Position From Alt Svc in mongo.
                                    //Send_BOOK_OPTXX();
                                    responseStatusMail = await _emailRepository.GenerateEmail(new EmailGetReq { BookingNo = request.BookingNumber, PositionId = request.PositionId, DocumentType = DocType.BOOKOPTXX, PlacerUserId = request.PlacerUserId, UserEmail = request.UserEmailId });

                                    resBooking = DirectUpdatePositionFromAltSvc(request.BookingNumber, request.MailType, position, AltSvc, request.UserEmailId, GridInfo);
                                }
                                else if ("IBKM!".Contains(position.STATUS))
                                {
                                    //if status in [I,B,K,M,!] : 
                                    //1)Cancel existing position. 1.a) Send BOOK_XX document email. 2) Add New Position From Alt Svc in mongo.
                                    isPosCancelled = await CancelPositionInBooking(request.BookingNumber, request.PositionId, request.UserEmailId, request.PlacerUserId, request.PageType);
                                    if (isPosCancelled)
                                    {
                                        BookingsSetRes res = new BookingsSetRes();
                                        res = await AddPositionFromAltSvc(request.BookingNumber, request.MailType, request.PositionId, request.AltSvcId, request.UserEmailId);
                                        resBooking = res.resBooking;
                                        NewPosId = res.PositionId;
                                    }
                                    if (isPosCancelled == false || resBooking == null)
                                    {
                                        response.ResponseStatus.ErrorMessage = "Current Position is not yet Cancelled";
                                        response.ResponseStatus.Status = "Failure";
                                    }
                                    else
                                    {
                                        response.ResponseStatus.ErrorMessage = "Product switched successfully";
                                        response.ResponseStatus.Status = "Success";
                                    }
                                }

                                if (request.MailType == "A")
                                {
                                    //Send_BOOK_KK();
                                    responseStatusMail = await _emailRepository.GenerateEmail(new EmailGetReq { BookingNo = request.BookingNumber, PositionId = request.PositionId, DocumentType = DocType.BOOKKK, PlacerUserId = request.PlacerUserId, UserEmail = request.UserEmailId });
                                }
                                else if (request.MailType == "B")
                                {
                                    //Send_BOOK_PROV();
                                    responseStatusMail = await _emailRepository.GenerateEmail(new EmailGetReq { BookingNo = request.BookingNumber, PositionId = NewPosId, DocumentType = DocType.BOOKPROV, PlacerUserId = request.PlacerUserId, UserEmail = request.UserEmailId });
                                }
                            }
                            else if (position.IsSuppPosLocked == true)
                            {
                                response.ResponseStatus.ErrorMessage = "The service has SUPPLIER INVOICE attached to it and cannot be switched";
                                response.ResponseStatus.Status = "Failure";
                            }
                        }
                        else
                        {
                            response.ResponseStatus.ErrorMessage = "The service is in invalid status";
                            response.ResponseStatus.Status = "Failure";
                        }
                        #endregion
                    }
                    else
                    {
                        response.ResponseStatus.ErrorMessage = "CancellationDeadline and OPTIONDATE not updated in mongodb for " + request.PositionId;
                        response.ResponseStatus.Status = "Failure";
                    }
                }
                else
                {
                    response.ResponseStatus.ErrorMessage = "Position details not found for " + request.PositionId;
                    response.ResponseStatus.Status = "Failure";
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return response;
        }

        public Bookings DirectUpdatePositionFromAltSvc(string BookingNumber, string SwitchType, Positions position, AlternateServices AltSvc, string UserEmailId, string GridInfo)
        {
            Bookings booking = null;
            BookingRoomsAndPrices altRoom = null;
            try
            {
                var product = _MongoContext.mProducts_Lite.Find(a => a.VoyagerProduct_Id == AltSvc.Product_Id).FirstOrDefault();
                var bookingSeason = position.BookingSeason.Where(a => a.Position_Id == position.Position_Id).FirstOrDefault();

                #region update position product info
                position.Product_Id = AltSvc.Product_Id;
                position.ProductCode = product.ProductCode;
                position.Product_Name = AltSvc.Product_Name;
                position.OPTIONDATE = AltSvc.OptionDate;
                position.CancelDeadline = position.STARTDATE?.AddDays(-Convert.ToInt32(AltSvc.CancellationDeadline));
                position.SupplierInfo = AltSvc.SupplierInfo;
                bookingSeason.Supplier_Id = AltSvc.SupplierInfo.Id;
                position.AuditTrail.MODI_US = UserEmailId;
                position.AuditTrail.MODI_DT = DateTime.Now;
                position.GRIDINFO = GridInfo;

                if (SwitchType == "A") position.STATUS = "K";
                else if (SwitchType == "B") position.STATUS = "O";
                else if (SwitchType == "C") position.STATUS = "E";

                #endregion

                #region update position rooms pricing info
                foreach (var room in position.BookingRoomsAndPrices)
                {
                    altRoom = AltSvc.Request_RoomsAndPrices.Where(a => a.ProductTemplate_Id == room.ProductTemplate_Id && a.PersonType_Id == room.PersonType_Id).FirstOrDefault();
                    if (altRoom != null)
                    {
                        room.ProductRange_Id = altRoom.ProductRange_Id;
                        room.Category_Id = altRoom.Category_Id;
                        room.CategoryName = altRoom.CategoryName;
                        room.BuyCurrency_Id = altRoom.BuyCurrency_Id;
                        room.BuyCurrency_Name = altRoom.BuyCurrency_Name;
                        room.BuyPrice = altRoom.BuyPrice;
                        room.ConfirmedReqPrice = false;
                    }
                }
                #endregion

                booking = _MongoContext.Bookings.FindOneAndUpdate(a => a.BookingNumber == BookingNumber &&
                            a.Positions.Any(b => b.Position_Id == position.Position_Id), Builders<Bookings>.Update.Set(a => a.Positions[-1], position));

                PositionInsertUpdateBridge(BookingNumber, position.Position_Id, UserEmailId);
                return booking;
            }
            catch (Exception ex)
            {
                return booking;
            }
        }

        public async Task<BookingsSetRes> AddPositionFromAltSvc(string BookingNumber, string SwitchType, string PositionId, string AltSvcId, string UserEmailId)
        {
            try
            {
                BookingsSetRes setResponse = new BookingsSetRes();

                int OrderNo = 0;
                string GridInfo, NewPosId = Guid.NewGuid().ToString();
                var resBooking = _MongoContext.Bookings.Find(a => a.BookingNumber == BookingNumber).FirstOrDefault();
                Positions NewPosition, OldPosition = resBooking.Positions.Where(a => a.Position_Id == PositionId && a.ProductType.Trim() == "Hotel").FirstOrDefault();
                var AltSvc = OldPosition.AlternateServices.Where(a => a.AlternateServies_Id == AltSvcId).FirstOrDefault();

                if (OldPosition.STATUS == "C")
                {
                    var AltSvcTwinPrice = AltSvc.Request_RoomsAndPrices.Where(a => a.RoomName == "TWIN").FirstOrDefault();
                    var product = _MongoContext.mProducts_Lite.Find(a => a.VoyagerProduct_Id == AltSvc.Product_Id).FirstOrDefault();
                    AuditTrail AuditTrail = new AuditTrail { CREA_DT = DateTime.Now, MODI_DT = null, CREA_US = UserEmailId, MODI_US = null, STATUS_DT = DateTime.Now, STATUS_US = UserEmailId };
                    var MaxOrderPos = resBooking.Positions.OrderByDescending(a => a.OrderNr).FirstOrDefault();
                    if (MaxOrderPos != null) OrderNo = Convert.ToInt32(MaxOrderPos.OrderNr);
                    OrderNo += 10;
                    GridInfo = string.Format("{0} {1} {2} - {3} {4}", OrderNo, OldPosition.ProductType, OldPosition.STARTDATE?.ToString("dd/MM/yy"), OldPosition.ENDDATE?.ToString("dd/MM/yy"), AltSvc.Product_Name);
                    //20 Hotel 07/11/18 - 08/11/18 * 5 Star Standard Hotel

                    OldPosition.BookingSeason.ForEach(a => a.BookingSeason_ID = Guid.NewGuid().ToString());
                    OldPosition.AlternateServices?.ForEach(a =>
                    {
                        a.AlternateServies_Id = Guid.NewGuid().ToString();
                        a.Request_RoomsAndPrices?.ForEach(b =>
                        {
                            b.BookingRooms_Id = Guid.NewGuid().ToString();
                            b.PositionPricing_Id = Guid.NewGuid().ToString();
                        });
                    });

                    string PostionStatus = "P";
                    if (SwitchType == "A") PostionStatus = "K";
                    else if (SwitchType == "B") PostionStatus = "O";
                    else if (SwitchType == "C") PostionStatus = "E";

                    NewPosition = new Positions
                    {
                        #region Common fields
                        Position_Id = NewPosId,
                        PositionType = OldPosition.PositionType,
                        GRIDINFO = GridInfo,
                        STATUS = PostionStatus,
                        OrderNr = OrderNo.ToString(),
                        InvoiceStatus = OldPosition.InvoiceStatus,
                        ProductType_Id = OldPosition.ProductType_Id,
                        ProductType = OldPosition.ProductType,
                        Product_Id = AltSvc.Product_Id,
                        ProductCode = product.ProductCode,
                        Product_Name = AltSvc.Product_Name,
                        Country_Id = AltSvc.Country_Id,
                        Country = AltSvc.Country,
                        City_Id = AltSvc.City_Id,
                        City = AltSvc.City,
                        Attributes = AltSvc.Attributes,
                        PositionFOC = OldPosition.PositionFOC,
                        STARTDATE = OldPosition.STARTDATE,
                        STARTTIME = OldPosition.STARTTIME,
                        STARTLOC = OldPosition.STARTLOC,
                        ENDDATE = OldPosition.ENDDATE,
                        ENDTIME = OldPosition.ENDTIME,
                        ENDLOC = OldPosition.ENDLOC,
                        DURATION = OldPosition.DURATION,
                        BuyCurrency_Id = AltSvcTwinPrice?.BuyCurrency_Id,
                        BuyCurrency_Name = AltSvcTwinPrice?.BuyCurrency_Name,
                        ExchangeRate_ID = OldPosition.ExchangeRate_ID,
                        ExchangeRateDetail_ID = OldPosition.ExchangeRateDetail_ID,
                        ExchangeRate = OldPosition.ExchangeRate,
                        ExchangeRateSell = OldPosition.ExchangeRateSell,
                        SupplierInfo = AltSvc.SupplierInfo,
                        Supplier_Confirmation = null,
                        SupplierInvoice_Id = null,
                        HotelPLacer_ID = OldPosition.HotelPLacer_ID,
                        HotelPLacer_Name = OldPosition.HotelPLacer_Name,
                        Commercials = OldPosition.Commercials,
                        StandardRooming = OldPosition.StandardRooming,
                        IsLocked = OldPosition.IsLocked,
                        IsSendToHotel = OldPosition.IsSendToHotel,
                        Special_Requests = OldPosition.Special_Requests,
                        EmptyLegs = OldPosition.EmptyLegs,
                        Porterage = OldPosition.Porterage,
                        MealsIncluded = OldPosition.MealsIncluded,
                        TicketsIncluded = OldPosition.TicketsIncluded,
                        HotelStarRating = Convert.ToInt32(AltSvc.Attributes?.StarRating?.Substring(0, 1)),     //check jira
                        HOTELMEALPLAN = OldPosition.HOTELMEALPLAN,
                        EarlyCheckIn = OldPosition.EarlyCheckIn,
                        WashChangeRoom = OldPosition.WashChangeRoom,
                        OPTIONDATE = OldPosition.OPTIONDATE,
                        CancelDeadline = OldPosition.STARTDATE?.AddDays(-Convert.ToInt32(AltSvc.CancellationDeadline)),
                        CancellationDate = OldPosition.CancellationDate,
                        CancellationUser = OldPosition.CancellationUser,
                        CancellationReason = OldPosition.CancellationReason,
                        FlightNumber = OldPosition.FlightNumber,
                        DriverName = OldPosition.DriverName,
                        DriverContactNumber = OldPosition.DriverContactNumber,
                        DriverLicenceNumber = OldPosition.DriverLicenceNumber,
                        SwitchedPosition_Id = OldPosition.Position_Id,
                        TotalSAPAmount = OldPosition.TotalSAPAmount,
                        AlternateServices = OldPosition.AlternateServices,
                        AuditTrail = AuditTrail,
                        #endregion

                        #region BookingSeason
                        BookingSeason = OldPosition.BookingSeason.Select(a => new BookingSeason
                        {
                            BookingSeason_ID = a.BookingSeason_ID,
                            Booking_Id = resBooking.Booking_Id,
                            STARTDATE = resBooking.STARTDATE?.ToString("dd-MMM-yyyy"),
                            ENDDATE = resBooking.ENDDATE?.ToString("dd-MMM-yyyy"),
                            Season = "GROUPS Default",
                            Position_Id = NewPosId,
                            WEEKDAY = "Daily",
                            PPBusiTypes = "G",
                            Supplier_Id = OldPosition.SupplierInfo.Id,
                            SalesQuotationPrice_Id = null
                        }).ToList(),
                        #endregion

                        #region BookingRoomsAndPrices
                        BookingRoomsAndPrices = AltSvc.Request_RoomsAndPrices.Select(a => new BookingRoomsAndPrices
                        {
                            BookingRooms_Id = a.BookingRooms_Id,
                            PositionPricing_Id = a.PositionPricing_Id,
                            Req_Count = a.Req_Count,
                            ChargeBasis = a.ChargeBasis,
                            Status = a.Status,
                            ProductRange_Id = a.ProductRange_Id,
                            Category_Id = a.Category_Id,
                            CategoryName = a.CategoryName,
                            ProductTemplate_Id = a.ProductTemplate_Id,
                            RoomShortCode = a.RoomShortCode,
                            RoomName = a.RoomName,
                            Capacity = a.Capacity,
                            PersonType_Id = a.PersonType_Id,
                            PersonType = a.PersonType,
                            Age = a.Age,
                            ApplyMarkup = a.ApplyMarkup,
                            ExcludeFromInvoice = a.ExcludeFromInvoice,
                            AllocationUsed = a.AllocationUsed,
                            Allocation_Id = a.Allocation_Id,
                            CrossPosition_Id = a.CrossPosition_Id,
                            CrossBookingPax_Id = a.CrossBookingPax_Id,
                            IsRecursive = a.IsRecursive,
                            OneOffDate = a.OneOffDate,
                            ParentBookingRooms_Id = a.ParentBookingRooms_Id,
                            MealPlan_Id = a.MealPlan_Id,
                            MealPlan = a.MealPlan,
                            ConfirmedReqPrice = false,
                            StartDate = a.StartDate,
                            EndDate = a.EndDate,
                            BuyCurrency_Id = a.BuyCurrency_Id,
                            BuyCurrency_Name = a.BuyCurrency_Name,
                            Action = a.Action,
                            BuyContract_Id = a.BuyContract_Id,
                            BuyPositionPrice_Id = a.BuyPositionPrice_Id,
                            SellContract_Id = a.SellContract_Id,
                            SellPositionPrice_Id = a.SellPositionPrice_Id,
                            SellContractCurrency_Id = a.SellContractCurrency_Id,
                            SellContractCurrency_Name = a.SellContractCurrency_Name,
                            ContractedSellPrice = a.ContractedSellPrice,
                            BudgetPrice = a.BudgetPrice,
                            RequestedPrice = a.RequestedPrice,
                            BuyPrice = a.BuyPrice,
                            ContractedBuyPrice = a.ContractedBuyPrice,
                            BookingSeason_Id = OldPosition.BookingSeason.Where(b => b.Position_Id == OldPosition.Position_Id).Select(b => b.BookingSeason_ID).FirstOrDefault(),
                            InvForPax = a.InvForPax,
                            InvNumber = a.InvNumber,
                            AuditTrail = AuditTrail
                        }).ToList()
                        #endregion
                    };

                    BookingPositionsSetRes response = new BookingPositionsSetRes();
                    try
                    {
                        response = await _bookingRepository.SetBookingPositions(new BookingPositionsSetReq { BookingNumber = BookingNumber, UserEmailId = UserEmailId, Position = NewPosition });
                        PositionInsertUpdateBridge(BookingNumber, PositionId, UserEmailId);
                        if (response?.ResponseStatus?.Status?.ToLower() == "success")
                        {
                            setResponse.PositionId = response.PositionId;
                            resBooking = _MongoContext.Bookings.Find(a => a.BookingNumber == BookingNumber).FirstOrDefault();
                            setResponse.resBooking = resBooking;
                        }
                        else
                        {
                            setResponse.resBooking = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
                    }
                }
                else
                {
                    return null;
                }
                return setResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> CancelPositionInBooking(string BookingNumber, string PositionId, string UserEmailId, string PlacerUserId, string PageType)
        {
            BookingPositionsSetRes response = new BookingPositionsSetRes();
            try
            {
                response = await _bookingRepository.CancelBookingPositions(new BookingCancelPositionSetReq { BookingNumber = BookingNumber, UserEmailId = UserEmailId, PositionId = PositionId, PlacerUserId = PlacerUserId, PageType = PageType });
                if (response?.ResponseStatus?.Status?.ToLower() == "success")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool PositionInsertUpdateBridge(string BookingNumber, string PositionId, string UserEmailId)
        {
            try
            {
                //call jira ticket 657
                var resPos = _bookingProviders.SetBookingPositionDetails(new BookingPosAltSetReq()
                {
                    BookingNumber = BookingNumber,
                    PositionId = PositionId,
                    User = UserEmailId
                }).Result;

                if (resPos?.Status?.ToLower() == "success")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}