using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VGER_WAPI.Helpers;
using VGER_WAPI.Models;
using VGER_WAPI.Providers;
using VGER_WAPI_CLASSES;
using VGER_WAPI_CLASSES.Booking;
//comment before commiting to development
//using Microsoft.Office.Interop.Outlook;
//using OutlookApp = Microsoft.Office.Interop.Outlook.Application;

namespace VGER_WAPI.Repositories
{
    public class OperationsRepository : IOperationsRepository
    {
        #region Private Variable Declaration
        private readonly MongoContext _MongoContext = null;
        private readonly IGenericRepository _genericRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IConfiguration _configuration;
        private readonly IMasterRepository _masterRepository;
        private BookingProviders _bookingProviders = null;
        private readonly IEmailRepository _emailRepository;
        private readonly IPDFRepository _pdfRepository;
        private readonly ICommonRepository _commonRepository;
        private readonly IProductRepository _productRepository;
        private readonly IHotelsDeptRepository _hotelsDeptRepository;
        private DocumentProviders _documentProviders = null;
        private readonly IMSDynamicsRepository _mSDynamicsRepository;
        #endregion

        public OperationsRepository(IOptions<MongoSettings> settings, IConfiguration configuration, IGenericRepository genericRepository, IBookingRepository bookingRepository,
            IMasterRepository masterRepository, IEmailRepository emailRepository, ICommonRepository commonRepository, IPDFRepository pdfRepository, 
            IProductRepository productRepository, IHotelsDeptRepository hotelsDeptRepository, IMSDynamicsRepository mSDynamicsRepository)
        {
            _MongoContext = new MongoContext(settings);
            _genericRepository = genericRepository;
            _configuration = configuration;
            _bookingRepository = bookingRepository;
            _masterRepository = masterRepository;
            _bookingProviders = new BookingProviders(_configuration);
            _emailRepository = emailRepository;
            _commonRepository = commonRepository;
            _pdfRepository = pdfRepository;
            _productRepository = productRepository;
            _hotelsDeptRepository = hotelsDeptRepository;
            _documentProviders = new DocumentProviders(_configuration);
            _mSDynamicsRepository = mSDynamicsRepository;
        }

        #region Ops Booking Search
        public async Task<OpsBookingSearchRes> GetBookingDetails(OpsBookingSearchReq request)
        {
            OpsBookingSearchRes searchResponse = new OpsBookingSearchRes();
            List<OpsBookingsSearchResult> response = new List<OpsBookingsSearchResult>();
            try
            {
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
                if (!string.IsNullOrWhiteSpace(request.AgentTourName))
                {
                    filter = filter & Builders<Bookings>.Filter.Regex(x => x.CustRef, new BsonRegularExpression(new Regex(request.AgentTourName.Trim(), RegexOptions.IgnoreCase)));
                }
                if (!string.IsNullOrWhiteSpace(request.Status))
                {
                    filter = filter & Builders<Bookings>.Filter.Eq(x => x.STATUS, request.Status);
                }
                else
                {
                    filter = filter & Builders<Bookings>.Filter.Nin(x => x.STATUS, StatusIgnoreList.Select(a => a));
                }
                if (!string.IsNullOrWhiteSpace(request.BusinessType))
                {
                    filter = filter & Builders<Bookings>.Filter.Eq(x => x.BusinessType, request.BusinessType);
                }
                if (!string.IsNullOrWhiteSpace(request.Destination))
                {
                    filter = filter & Builders<Bookings>.Filter.Eq(x => x.Preferences.Destination_Id, request.Destination);
                }
                if (!string.IsNullOrWhiteSpace(request.SalesOffice))
                {
                    filter = filter & Builders<Bookings>.Filter.Eq(x => x.AgentInfo.Division_Name, request.SalesOffice);
                }
                if (!string.IsNullOrWhiteSpace(request.FileHandler))
                {
                    filter = filter & Builders<Bookings>.Filter.Eq(x => x.StaffDetails.Staff_OpsUser_Email, request.FileHandler);
                }

                if (!string.IsNullOrWhiteSpace(request.DateType) && request.DateType.ToLower().Trim() == "creation date")
                {
                    DateTime fromdt = new DateTime();
                    DateTime todt = new DateTime();
                    if (request.From != null && request.To != null)
                    {
                        fromdt = request.From.Value;
                        todt = request.To.Value.AddHours(23).AddMinutes(59).AddSeconds(59);
                        filter = filter & Builders<Bookings>.Filter.Where(x => x.AuditTrail.CREA_DT >= fromdt && x.AuditTrail.CREA_DT <= todt);
                    }
                    else if (request.From != null && request.To == null)
                    {
                        filter = filter & Builders<Bookings>.Filter.Where(x => x.AuditTrail.CREA_DT >= request.From);
                    }
                    else if (request.From == null && request.To != null)
                    {
                        todt = request.To.Value.AddHours(23).AddMinutes(59).AddSeconds(59);
                        filter = filter & Builders<Bookings>.Filter.Where(x => x.AuditTrail.CREA_DT <= todt);
                    }
                }

                if (!string.IsNullOrWhiteSpace(request.DateType) && request.DateType.ToLower().Trim() == "travel date")
                {
                    DateTime fromdt = new DateTime();
                    DateTime todt = new DateTime();
                    if (request.From != null && request.To != null)
                    {
                        fromdt = request.From.Value;
                        todt = request.To.Value.AddHours(23).AddMinutes(59).AddSeconds(59);
                        filter = filter & Builders<Bookings>.Filter.Where(x => x.STARTDATE >= fromdt && x.STARTDATE <= todt);
                    }
                    else if (request.From != null && request.To == null)
                    {
                        filter = filter & Builders<Bookings>.Filter.Where(x => x.STARTDATE >= request.From);
                    }
                    else if (request.From == null && request.To != null)
                    {
                        todt = request.To.Value.AddHours(23).AddMinutes(59).AddSeconds(59);
                        filter = filter & Builders<Bookings>.Filter.Where(x => x.STARTDATE <= todt);
                    }
                }
                #endregion

                result = await _MongoContext.Bookings.Find(filter).Sort("{STARTDATE: 1}").ToListAsync();

                List<ServicePercentages> ServicePerc = CalculateCompletionPercentage(result);

                response = result.Skip(request.Start).Take(request.Length).Select(a => new OpsBookingsSearchResult
                {
                    BookingNumber = a.BookingNumber,
                    OutstandingCount = a.Fixes?.Where(b => b.Status.ToUpper() != "CLOSE").Count().ToString(),
                    Status = a.STATUS,
                    Type = a.BusinessType,
                    CompanyContact = a.AgentInfo.Contact_Name,
                    CompanyName = a.AgentInfo.Name,
                    Telephone = a.AgentInfo.Contact_Tel,
                    Email = a.AgentInfo.Contact_Email,
                    TourName = a.CustRef,
                    Duration = a.Duration.ToString(),
                    PaxCount = a.BookingPax.Where(b => b.PERSTYPE == "ADULT").FirstOrDefault()?.PERSONS.ToString() + " Adults",
                    StartDate = (Convert.ToDateTime(a.STARTDATE)).ToString("ddMMM"),
                    EndDate = (Convert.ToDateTime(a.ENDDATE)).ToString("ddMMM"),
                    Destination = a.Preferences.Destination_Name == null ? "" : a.Preferences.Destination_Name,
                    AccomPercent = Convert.ToDecimal(ServicePerc.Where(b => b.BookingNumber == a.BookingNumber).FirstOrDefault()?.AccomPercent),
                    TransportPercent = Convert.ToDecimal(ServicePerc.Where(b => b.BookingNumber == a.BookingNumber).FirstOrDefault()?.TransportPercent),
                    ServicesPercent = Convert.ToDecimal(ServicePerc.Where(b => b.BookingNumber == a.BookingNumber).FirstOrDefault()?.ServicesPercent),
                    SalesOfficerEmail = a.StaffDetails.Staff_SalesUser_Email,
                    FileHandlerEmail = a.StaffDetails.Staff_OpsUser_Email,
                    ProductAccountantEmail = a.StaffDetails.Staff_PAUser_Email,
                    SalesOffice = a.AgentInfo?.Division_Name == null ? "" : a.AgentInfo?.Division_Name
                }).ToList();

                if (result.Count > 0)
                {
                    searchResponse.BookingTotalCount = result.Count();
                }
                searchResponse.Bookings = response;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return searchResponse;
        }
        #endregion

        public async Task<OpsBookingSummaryGetRes> GetOpsBookingSummary(ProductSRPHotelGetReq request)
        {
            OpsBookingSummaryGetRes response = new OpsBookingSummaryGetRes();
            try
            {
                var resBooking = await _MongoContext.Bookings.FindAsync(a => a.BookingNumber == request.QRFID).Result.FirstOrDefaultAsync();
                if (resBooking != null)
                {
                    var posIdList = resBooking.Positions.Select(b => b.Position_Id).ToList();
                    var documentStore = await _MongoContext.mDocumentStore.FindAsync(a => a.DocumentType == "VOUCHER"
                                && posIdList.Contains(a.PositionId)).Result.ToListAsync();

                    int PlacingPendingCnt = resBooking.Positions.Where(a => a.STATUS == "P").Count();
                    int BookingPendingCnt = resBooking.Positions.Where(a => "EOQM".Contains(a.STATUS)).Count();
                    int BookingConfirmedCnt = resBooking.Positions.Where(a => "KBI".Contains(a.STATUS)).Count();
                    int BookingVoucheredCnt = documentStore.Count;
                    List<ServicePercentages> ServicePerc = CalculateCompletionPercentage(new List<Bookings> { resBooking });

                    response = new OpsBookingSummaryGetRes
                    {
                        OpsBookingSummaryDetails = new OpsBookingSummaryDetails
                        {
                            QRFID = resBooking.QRFID,
                            AgentId = resBooking.AgentInfo.Id,
                            AgentName = resBooking.AgentInfo.Name,
                            ContactId = resBooking.AgentInfo.Contact_Id,
                            ContactName = resBooking.AgentInfo.Contact_Name,
                            ContactTel = resBooking.AgentInfo.Contact_Tel,
                            ContactEmail = resBooking.AgentInfo.Contact_Email,
                            SalesOfficerEmail = resBooking.StaffDetails.Staff_SalesUser_Email,
                            CostingOfficerEmail = resBooking.StaffDetails.Staff_SalesSupport_Email,
                            FileHandlerEmail = resBooking.StaffDetails.Staff_OpsUser_Email,
                            ProductAccountantEmail = resBooking.StaffDetails.Staff_PAUser_Email,
                            Destination = resBooking.Preferences?.Destination_Name,
                            Division = resBooking.AgentInfo?.Division_Name,
                            Nationality = resBooking.GuestDetails?.Nationality_Name,
                            TravelReason = resBooking.GuestDetails?.TravelReason,
                            PRIORITY = resBooking.Preferences?.PRIORITY,

                            TourName = resBooking.CustRef,
                            TourType = resBooking.BusinessType,
                            GoAheadDate = resBooking.GoAheadDetails.GoAhead_Date?.ToString("dd/MM/yyyy"),
                            HotelConfirmationDate = resBooking.GoAheadDetails.HotelConfirmation_Date?.ToString("dd/MM/yyyy"),
                            StartDate = resBooking.STARTDATE,
                            EndDate = resBooking.ENDDATE,
                            NoOfDays = Convert.ToInt16(resBooking.Duration) + 1,
                            NoOfNights = Convert.ToInt16(resBooking.Duration),
                            PaymentDueDate = resBooking.BookingDate?.ToString("dd/MM/yyyy"),        //not available
                            BookingFixes = resBooking.Fixes.GroupBy(x => x.FixDescription).Select(x => x.FirstOrDefault()).OrderBy(a => a.Position_StartDate).ToList(),

                            ConfirmationPerc = Convert.ToDecimal(ServicePerc.Where(a => a.BookingNumber == resBooking.BookingNumber).FirstOrDefault()?.TotalPercent),

                            PlacingPendingCnt = PlacingPendingCnt,
                            BookingPendingCnt = BookingPendingCnt,
                            BookingConfirmedCnt = BookingConfirmedCnt - BookingVoucheredCnt,
                            BookingVoucheredCnt = BookingVoucheredCnt,

                            PaxRooms = resBooking.BookingRooms.Where(b => (b.PersonType == null || b.PersonType == "ADULT") && b.Status != "X" && b.ROOMNO != null && b.ROOMNO > 0).Select(a => new BookingRooms
                            {
                                Count = a.ROOMNO,
                                For = "(" + a.Name + ")" + a.SUBPROD,
                                Type = a.SUBPROD,
                                Age = (a.Age.HasValue ? Convert.ToString(a.Age.Value): null)

                            }).ToList(),
                            TourStaffRooms = resBooking.BookingRooms.Where(b => b.PersonType != null && b.PersonType != "ADULT").Select(a => new BookingRooms
                            {
                                Count = a.ROOMNO,
                                For = "(" + a.PersonType + ")" + a.SUBPROD
                            }).ToList(),
                        }
                    };
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

        public async Task<PositionsFromBookingGetRes> GetPositionsFromBooking(PositionsFromBookingGetReq request)
        {
            PositionsFromBookingGetRes response = new PositionsFromBookingGetRes();
            List<Positions> DetailsOfPositions = new List<Positions>();
            try
            {
                if (!string.IsNullOrEmpty(request.BookingNumber))
                {
                    DetailsOfPositions = _MongoContext.Bookings.AsQueryable().Where(x => x.BookingNumber == request.BookingNumber).FirstOrDefault().Positions;

                }
                if (!string.IsNullOrEmpty(request.PositionId))
                {
                    DetailsOfPositions = DetailsOfPositions.Where(x => x.Position_Id == request.PositionId).ToList();

                }
                if (!string.IsNullOrEmpty(request.PositionType))
                {
                    DetailsOfPositions = DetailsOfPositions.Where(x => x.ProductType.Trim().ToUpper() == request.PositionType.Trim().ToUpper()).ToList();
                }

                DetailsOfPositions = DetailsOfPositions.Where(a => a.STATUS.Trim().ToUpper() != "J" && a.STATUS.Trim().ToUpper() != "C" && a.STATUS.Trim().ToUpper() != "X" && a.STATUS != null).ToList();

                if (request.IsPlaceholder != null)
                {
                    var PosProdIds = DetailsOfPositions.Select(x => x.Product_Id).ToList();
                    var ProdIds = _MongoContext.mProducts_Lite.AsQueryable().Where(x => PosProdIds.Contains(x.VoyagerProduct_Id) && x.Placeholder == request.IsPlaceholder).Select(y => y.VoyagerProduct_Id).ToList();
                    DetailsOfPositions = DetailsOfPositions.Where(x => ProdIds.Contains(x.Product_Id)).ToList();
                }

                response.PositionDetails = DetailsOfPositions;
                response.Response.Status = "Success";
                response.Response.StatusMessage = "Positions Retrieved Successfully";

            }
            catch (System.Exception ex)
            {
                response.Response.Status = "Failure";
                response.Response.ErrorMessage = !string.IsNullOrEmpty(ex.Message) ? ex.Message : ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message) ? ex.InnerException.Message : "";
            }
            return response;
        }

        public async Task<BookingRoomGetResponse> GetOpsBookingRoomsDetails(BookingRoomsGetRequest request)
        {
            BookingRoomGetResponse response = new BookingRoomGetResponse();
            List<TemplateBookingRoomsGrid> DetailsOfBookingRooms = new List<TemplateBookingRoomsGrid>();
            try
            {

                if (!string.IsNullOrEmpty(request.BookingNumber))
                {
                    var BookingData = _MongoContext.Bookings.AsQueryable().Where(x => x.BookingNumber == request.BookingNumber).FirstOrDefault();
                    response.QrfId = BookingData.QRFID;
                    DetailsOfBookingRooms = _MongoContext.Bookings.AsQueryable().Where(x => x.BookingNumber == request.BookingNumber).FirstOrDefault().BookingRooms;

                }
                if (!string.IsNullOrEmpty(request.BookingRoom_id))
                {
                    DetailsOfBookingRooms = DetailsOfBookingRooms.Where(x => x.BookingRooms_ID == request.BookingRoom_id).ToList();

                }
                if (!string.IsNullOrEmpty(request.RoomType))
                {
                    DetailsOfBookingRooms = DetailsOfBookingRooms.Where(x => x.SUBPROD.Trim().ToUpper() == request.RoomType.Trim().ToUpper()).ToList();
                }
                DetailsOfBookingRooms = DetailsOfBookingRooms.Where(a => a.Status?.Trim()?.ToUpper() != "X" && a.ROOMNO != null && a.ROOMNO != 0).ToList();

                response.BookingRoomsDetails = DetailsOfBookingRooms.ToList();

                response.Response.Status = "Success";
                response.Response.StatusMessage = "BookingRooms Retrieved Successfully";

            }
            catch (System.Exception ex)
            {
                response.Response.Status = "Failure";
                response.Response.ErrorMessage = !string.IsNullOrEmpty(ex.Message) ? ex.Message : ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message) ? ex.InnerException.Message : "";
            }
            return response;
        }

        public async Task<QrfPackagePriceGetRes> GetQRFPackagePriceForRoomsDetails(QrfPackagePriceGetReq request)
        {
            QrfPackagePriceGetRes response = new QrfPackagePriceGetRes();
            List<mQRFPackagePrice> DetailsQrfPriceForBookingRooms = new List<mQRFPackagePrice>();
            try
            {

                if (!string.IsNullOrEmpty(request.QrfId))
                {
                    //var BookingData = _MongoContext.Bookings.AsQueryable().Where(x => x.BookingNumber == request.BookingNumber).FirstOrDefault();
                    //response.QrfId = BookingData.QRFID;
                    DetailsQrfPriceForBookingRooms = _MongoContext.mQRFPackagePrice.AsQueryable().Where(x => x.QRFID == request.QrfId).ToList();

                }

                DetailsQrfPriceForBookingRooms = DetailsQrfPriceForBookingRooms.Where(a => a.Status?.Trim()?.ToUpper() != "X").ToList();

                response.RoomType = DetailsQrfPriceForBookingRooms.Select(x => new { x.RoomName }).Distinct().Select(x => x.RoomName).ToList();
                response.Response.Status = "Success";
                response.Response.StatusMessage = "BookingRooms in costSheet of Qrdid  Retrieved Successfully";

            }
            catch (System.Exception ex)
            {
                response.Response.Status = "Failure";
                response.Response.ErrorMessage = !string.IsNullOrEmpty(ex.Message) ? ex.Message : ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message) ? ex.InnerException.Message : "";
            }
            return response;
        }

        public async Task<BookingPaxDetailsGetResponse> GetOpsBookingPaxDetails(BookingPaxDetailsGetRequest request)
        {
            BookingPaxDetailsGetResponse response = new BookingPaxDetailsGetResponse();
            List<TemplateBookingPaxGrid> DetailsOfPacBookingDetails = new List<TemplateBookingPaxGrid>();
            try
            {

                if (!string.IsNullOrEmpty(request.BookingNumber))
                {
                    DetailsOfPacBookingDetails = _MongoContext.Bookings.AsQueryable().Where(x => x.BookingNumber == request.BookingNumber).FirstOrDefault().BookingPax;

                }
                if (!string.IsNullOrEmpty(request.PersonType))
                {
                    DetailsOfPacBookingDetails = DetailsOfPacBookingDetails.Where(x => x.PERSTYPE.Trim().ToUpper() == request.PersonType.Trim().ToUpper()).ToList();

                }

                DetailsOfPacBookingDetails = DetailsOfPacBookingDetails.Where(a => a.Status?.Trim()?.ToUpper() != "X" && a.PERSONS > 0).ToList();

                response.bookingPaxDetails = DetailsOfPacBookingDetails.ToList();

                response.Response.Status = "Success";
                response.Response.StatusMessage = "BookingPaxRooms Retrieved Successfully";

            }
            catch (System.Exception ex)
            {
                response.Response.Status = "Failure";
                response.Response.ErrorMessage = !string.IsNullOrEmpty(ex.Message) ? ex.Message : ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message) ? ex.InnerException.Message : "";
            }
            return response;
        }


        public List<ServicePercentages> CalculateCompletionPercentage(List<Bookings> bookings)
        {
            List<ServicePercentages> ServicePerc = new List<ServicePercentages>();
            string[] accomtypes = new string[] { "HOTEL", "OVERNIGHT FERRY" };
            string[] transporttypes = new string[] { "LDC", "COACH", "PRIVATE TRANSFER", "SCHEDULED TRANSFER", "FERRY TRANSFER", "FERRY PASSENGER", "TRAIN", "DOMESTIC FLIGHT" };
            string[] servicestypes = new string[] { "MEAL", "ATTRACTIONS", "SIGHTSEEING - CITYTOUR", "GUIDE", "VISA", "INSURANCE" };
            string statuschecks = "KBI";
            decimal accomList, transportList, servicesList, accomCompleted, transportCompleted, servicesCompleted, accomPerc, transportPerc, servicesPerc, totalPerc;

            try
            {
                for (int i = 0; i < bookings.Count; i++)
                {
                    accomList = bookings[i].Positions.Where(a => accomtypes.Contains(a.ProductType?.ToUpper())).Count();
                    transportList = bookings[i].Positions.Where(a => transporttypes.Contains(a.ProductType?.ToUpper())).Count();
                    servicesList = bookings[i].Positions.Where(a => servicestypes.Contains(a.ProductType?.ToUpper())).Count();

                    accomCompleted = bookings[i].Positions.Where(a => accomtypes.Contains(a.ProductType?.ToUpper()) && statuschecks.Contains(a.STATUS)).Count();
                    transportCompleted = bookings[i].Positions.Where(a => transporttypes.Contains(a.ProductType?.ToUpper()) && statuschecks.Contains(a.STATUS)).Count();
                    servicesCompleted = bookings[i].Positions.Where(a => servicestypes.Contains(a.ProductType?.ToUpper()) && statuschecks.Contains(a.STATUS)).Count();

                    accomPerc = (accomCompleted / accomList) * 100;
                    transportPerc = (transportCompleted / accomList) * 100;
                    servicesPerc = (servicesCompleted / accomList) * 100;
                    totalPerc = (accomPerc + transportPerc + servicesPerc) / 3;

                    ServicePerc.Add(new ServicePercentages { BookingNumber = bookings[i].BookingNumber, AccomPercent = Math.Round(accomPerc, 2), TransportPercent = Math.Round(transportPerc, 2), ServicesPercent = Math.Round(servicesPerc, 2), TotalPercent = Math.Round(totalPerc) });
                }
            }
            catch (Exception ex)
            { }

            return ServicePerc;
        }

        #region RoomingList
        public async Task<BookingRoomingSetResponse> SetRoomingDetails(SetPassengerDetailsReq request)
        {
            var response = new BookingRoomingSetResponse();
            try
            {

                var Bookings = _MongoContext.Bookings.AsQueryable().Where(x => x.BookingNumber == request.Booking_Number).FirstOrDefault();

                if (Bookings != null)
                {
                    foreach (var item in request.PassengerInfo)
                    {
                        if (String.IsNullOrEmpty(item.Passenger_Id))
                        {
                            PassengerDetails newPassenger = new PassengerDetails();
                            newPassenger.Passenger_Id = Guid.NewGuid().ToString();

                            newPassenger.Title = item.Title;
                            newPassenger.FirstName = item.FirstName;
                            newPassenger.LastName = item.LastName;
                            newPassenger.PassengerName_LocalLanguage = item.PassengerName_LocalLanguage;
                            newPassenger.DateOfBirth = item.DateOfBirth;
                            newPassenger.Notes = item.Notes;
                            newPassenger.PassportNumber = item.PassportNumber;
                            newPassenger.PassportIssued = item.PassportIssued;
                            newPassenger.PassportExpiry = item.PassportExpiry;
                            newPassenger.VisaNumber = item.VisaNumber;
                            newPassenger.Sex = item.Sex;
                            newPassenger.ISTourLeader = item.ISTourLeader;
                            newPassenger.DateOfAnniversary = item.DateOfAnniversary;
                            newPassenger.DietaryRequirements = item.DietaryRequirements;
                            newPassenger.SpecialAssistanceRequirements = item.SpecialAssistanceRequirements;
                            newPassenger.RoomType = item.RoomType;
                            newPassenger.RoomAssignment = item.RoomAssignment;
                            newPassenger.PersonType = item.PersonType;
                            newPassenger.PassengerNumber = item.PassengerNumber;
                            newPassenger.AuditTrail.CREA_DT = DateTime.Now;
                            newPassenger.AuditTrail.CREA_US = item.AuditTrail.CREA_US;
                            if (Int32.TryParse(item.Age, out int j))
                            {
                                newPassenger.Age = item?.Age;
                            }
                            Bookings.RoomingList.Add(newPassenger);
                            response.ResponseStatus.Status = "Success";
                            response.ResponseStatus.StatusMessage = "Passenger details Successfully Inserted ";

                        }
                        else
                        {
                            var PassengerInfo = Bookings.RoomingList.Where(x => x.Passenger_Id == item.Passenger_Id).FirstOrDefault();
                            PassengerInfo.Title = item.Title;
                            PassengerInfo.FirstName = item.FirstName;
                            PassengerInfo.LastName = item.LastName;
                            PassengerInfo.PassengerName_LocalLanguage = item.PassengerName_LocalLanguage;
                            PassengerInfo.DateOfBirth = item.DateOfBirth;
                            PassengerInfo.Notes = item.Notes;
                            PassengerInfo.PassportNumber = item.PassportNumber;
                            PassengerInfo.PassportIssued = item.PassportIssued;
                            PassengerInfo.PassportExpiry = item.PassportExpiry;
                            PassengerInfo.VisaNumber = item.VisaNumber;
                            PassengerInfo.Sex = item.Sex;
                            PassengerInfo.ISTourLeader = item.ISTourLeader;
                            PassengerInfo.DietaryRequirements = item.DietaryRequirements;
                            PassengerInfo.SpecialAssistanceRequirements = item.SpecialAssistanceRequirements;
                            PassengerInfo.RoomType = item.RoomType;
                            PassengerInfo.RoomAssignment = item.RoomAssignment;
                            PassengerInfo.PersonType = item.PersonType;
                            PassengerInfo.DateOfAnniversary = item.DateOfAnniversary;
                            PassengerInfo.Age = item.Age;
                            PassengerInfo.PassengerNumber = item.PassengerNumber;
                            PassengerInfo.AuditTrail.MODI_DT = DateTime.Now;
                            PassengerInfo.AuditTrail.MODI_US = item.AuditTrail.MODI_US;
                            response.ResponseStatus.Status = "Success";
                            response.ResponseStatus.StatusMessage = "Passenger details Successfully Updated ";

                        }
                    }
                    await _MongoContext.Bookings.FindOneAndUpdateAsync(a => a.BookingNumber == request.Booking_Number,
                                                    Builders<Bookings>.Update.Set("RoomingList", Bookings.RoomingList));
                }
                else
                {

                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "No bookings Exists";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = !string.IsNullOrEmpty(ex.Message) ? ex.Message : ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message) ? ex.InnerException.Message : "";

            }
            return response;
        }
        public async Task<OpsBookingSetRes> CancelRoomingListUpdate(string BookingNumber)
        {
            OpsBookingSetRes response = new OpsBookingSetRes();
            var Bookings = _MongoContext.Bookings.AsQueryable().Where(x => x.BookingNumber == BookingNumber).FirstOrDefault();
            if(Bookings != null)
            {
                var RoomingList = Bookings.RoomingList.ToList();
                foreach (var t in RoomingList)
                {
                    Bookings.RoomingList.RemoveAll(x => x.PassengerNumber == t.PassengerNumber);
                }
                await _MongoContext.Bookings.FindOneAndUpdateAsync(a => a.BookingNumber == BookingNumber,
                                                    Builders<Bookings>.Update.Set("RoomingList", Bookings.RoomingList));
            }
            else
            {

                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage.Add("No Bookings Exists");
            }
            return response;

        }
        public async Task<BookingRoomingSetResponse> SaveRoomingListPersonDetails(SetPassengerDetailsReq request)
        {
            //List<PassengerDetails> PassInDb = new List<PassengerDetails>();
            var response = new BookingRoomingSetResponse();
            try
            {

                var Bookings = _MongoContext.Bookings.AsQueryable().Where(x => x.BookingNumber == request.Booking_Number).FirstOrDefault();

                if (Bookings != null)
                {
                    List<int> PassInDb = Bookings.RoomingList.Select(x => x.PassengerNumber).ToList();
                    List<int> PassinExcel = request?.PassengerInfo?.Select(x => x.PassengerNumber).ToList();
                    var RemoveddatafromUi = PassInDb.Except(PassinExcel);
                    foreach (var item in request.PassengerInfo)
                    {
                        var PassengerInfo = Bookings.RoomingList.Where(x => x.PassengerNumber == item.PassengerNumber).FirstOrDefault();
                        if (PassengerInfo != null)
                        {
                            PassengerInfo.RoomAssignment = item.RoomAssignment;
                            PassengerInfo.RoomType = item.RoomType;
                            PassengerInfo.PassengerName_LocalLanguage = item.PassengerName_LocalLanguage;
                            PassengerInfo.FirstName = item.FirstName;
                            PassengerInfo.LastName = item.LastName;
                            PassengerInfo.Sex = item.Sex;
                            PassengerInfo.DateOfBirth = item.DateOfBirth;
                            PassengerInfo.PassportNumber = item.PassportNumber;
                            PassengerInfo.PassportIssued = item.PassportIssued;
                            PassengerInfo.PassportExpiry = item.PassportExpiry;
                            PassengerInfo.VisaNumber = item.VisaNumber;
                            if (!string.IsNullOrEmpty(item.Age))
                            {
                                PassengerInfo.Age = item.Age;
                            }
                            PassengerInfo.Notes = item.Notes;
                            PassengerInfo.AuditTrail.MODI_DT = DateTime.Now;
                            PassengerInfo.AuditTrail.MODI_US = item.AuditTrail.MODI_US;

                        }
                        else {
                            PassengerDetails NewPassengerInfo = new PassengerDetails();
                            NewPassengerInfo.RoomAssignment = item.RoomAssignment;
                            NewPassengerInfo.RoomType = item.RoomType;
                            NewPassengerInfo.Passenger_Id = Guid.NewGuid().ToString();
                            NewPassengerInfo.PassengerName_LocalLanguage = item.PassengerName_LocalLanguage;
                            NewPassengerInfo.FirstName = item.FirstName;
                            NewPassengerInfo.LastName = item.LastName;
                            NewPassengerInfo.Sex = item.Sex;
                            NewPassengerInfo.DateOfBirth = item.DateOfBirth;
                            NewPassengerInfo.PassportNumber = item.PassportNumber;
                            NewPassengerInfo.PassportIssued = item.PassportIssued;
                            NewPassengerInfo.PassportExpiry = item.PassportExpiry;
                            NewPassengerInfo.PassengerNumber = item.PassengerNumber;
                            NewPassengerInfo.VisaNumber = item.VisaNumber;
                            if (!string.IsNullOrEmpty(item.Age))
                            {
                                NewPassengerInfo.Age = item.Age;
                            }
                            NewPassengerInfo.Notes = item.Notes;
                            NewPassengerInfo.AuditTrail.MODI_DT = DateTime.Now;
                            NewPassengerInfo.AuditTrail.MODI_US = item.AuditTrail.MODI_US;
                            Bookings.RoomingList.Add(NewPassengerInfo);

                        }
                    }
                    foreach (var x in RemoveddatafromUi)
                    {
                        var PassDataDeletedFromUi = Bookings.RoomingList.Where(t => t.PassengerNumber == x)?.FirstOrDefault();
                        if (PassDataDeletedFromUi != null)
                        {
                            Bookings.RoomingList.RemoveAll(t => t.PassengerNumber == x);
                        }
                    }
                    Bookings.RoomingList.OrderBy(x => x.PassengerNumber);
                    response.ResponseStatus.Status = "Success";
                    response.ResponseStatus.StatusMessage = "Passenger details Successfully Updated ";

                    await _MongoContext.Bookings.FindOneAndUpdateAsync(a => a.BookingNumber == request.Booking_Number,
                                                    Builders<Bookings>.Update.Set("RoomingList", Bookings.RoomingList));
                }
                else
                {

                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "No bookings Exists";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = !string.IsNullOrEmpty(ex.Message) ? ex.Message : ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message) ? ex.InnerException.Message : "";

            }
            return response;

        }
        public async Task<BookingRoomsSetResponse> UpdateBookingRoomsDataAsperExcel(BookingRoomsSetRequest request)
        {
            var response = new BookingRoomsSetResponse();
            try
            {

                var Bookings = _MongoContext.Bookings.AsQueryable().Where(x => x.BookingNumber == request.BookingNumber).FirstOrDefault();

                if (Bookings != null)
                {
                    foreach (var item in request.BookingRoomDetails)
                    {
                        var BookingRoom = Bookings.BookingRooms.Where(x => x.SUBPROD.Trim().ToUpper() == item.RoomName).FirstOrDefault();
                        if (BookingRoom != null)
                        {
                            //PassengerInfo.RoomAssignment = item.RoomAssignment;
                            //PassengerInfo.RoomType = item.RoomType;
                            //PassengerInfo.PassengerName_LocalLanguage = item.PassengerName_LocalLanguage;
                            //PassengerInfo.FirstName = item.FirstName;
                            //PassengerInfo.LastName = item.LastName;
                            //PassengerInfo.Sex = item.Sex;
                            //PassengerInfo.DateOfBirth = item.DateOfBirth;
                            //PassengerInfo.PassportNumber = item.PassportNumber;
                            //PassengerInfo.PassportIssued = item.PassportIssued;
                            //PassengerInfo.PassportExpiry = item.PassportExpiry;
                            //PassengerInfo.VisaNumber = item.VisaNumber;
                            //PassengerInfo.Notes = item.Notes;
                            //PassengerInfo.AuditTrail.MODI_DT = DateTime.Now;
                            //PassengerInfo.AuditTrail.MODI_US = item.AuditTrail.MODI_US;
                            //if (BookingRoom.ROOMNO.HasValue && (BookingRoom.ROOMNO.Value != Convert.ToInt32(item.RoomQuantity)) && String.IsNullOrEmpty(item.Difference))
                            //{
                            //    BookingRoom.ROOMNO = Convert.ToInt32(item.RoomQuantity);
                            //}

                        }
                        else
                        {
                            //TemplateBookingRoomsGrid t = new TemplateBookingRoomsGrid();
                            //t.SUBPROD = item.RoomName;
                            //t.ROOMNO = Convert.ToInt32(item.RoomQuantity);
                            //Bookings.BookingRooms.Add(t);
                        }
                    }
                    response.Response.Status = "Success";
                    response.Response.StatusMessage = "BookingRooms details Successfully Updated ";

                    await _MongoContext.Bookings.FindOneAndUpdateAsync(a => a.BookingNumber == request.BookingNumber,
                                                    Builders<Bookings>.Update.Set("BookingRooms", Bookings.BookingRooms));
                }
                else
                {

                    response.Response.Status = "Failure";
                    response.Response.ErrorMessage = "No bookings Exists";
                }
            }
            catch (Exception ex)
            {
                response.Response.Status = "Failure";
                response.Response.ErrorMessage = !string.IsNullOrEmpty(ex.Message) ? ex.Message : ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message) ? ex.InnerException.Message : "";

            }
            return response;

        }


        public async Task<BookingRoomingGetResponse> GetRoomingDetails(BookingRoomingGetRequest request)
        {
            var response = new BookingRoomingGetResponse();
            List<PassengerDetails> DetailsOfPassengers = new List<PassengerDetails>();
            try
            {
                if (!string.IsNullOrEmpty(request.Booking_Number))
                {
                    DetailsOfPassengers = _MongoContext.Bookings.AsQueryable().Where(x => x.BookingNumber == request.Booking_Number).FirstOrDefault().RoomingList;

                }
                if (!string.IsNullOrEmpty(request.Passenger_Id))
                {

                    //DetailsOfPassengers = _MongoContext.Bookings.AsQueryable().Where(x => x.RoomingList.Any(y => y.Passenger_Id == request.Passenger_Id)).FirstOrDefault().RoomingList;
                    DetailsOfPassengers = DetailsOfPassengers.Where(x => x.Passenger_Id == request.Passenger_Id).ToList();
                }
                response.Passengers = DetailsOfPassengers;
                response.Response.Status = "Success";

            }
            catch (Exception ex)
            {
                response.Response.Status = "Failure";
                response.Response.ErrorMessage = !string.IsNullOrEmpty(ex.Message) ? ex.Message : ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message) ? ex.InnerException.Message : "";
            }
            return response;
        }

        public async Task<BookingRoomHotelsGetRes> GetRoomingDetailsForHotels(BookingRoomingGetRequest request)
        {
            var response = new BookingRoomHotelsGetRes() { SendRoomingListToHotelVm = new List<SendRoomingListToHotelVm>(), Response = new ResponseStatus() };
            try
            {
                if (!string.IsNullOrWhiteSpace(request.Booking_Number))
                {
                    var ExcludeList = new List<string>() { "-", "X", "J", "C" };

                    var booking = _MongoContext.Bookings.AsQueryable().Where(x => x.BookingNumber == request.Booking_Number).FirstOrDefault();

                    var lstPos = booking.Positions.Where(a => !ExcludeList.Contains(a.STATUS) && a.ProductType.ToLower() == "hotel").ToList();
                    var lstPosStatus = lstPos.Select(a => a.STATUS).ToList();
                    var lstStatus = await _MongoContext.mStatus.Find(a => lstPosStatus.Contains(a.Status)).ToListAsync();

                    var PosProdIds = lstPos.Select(x => x.Product_Id).ToList();
                    var products_Lites = _MongoContext.mProducts_Lite.AsQueryable().Where(x => PosProdIds.Contains(x.VoyagerProduct_Id)).ToList();

                    foreach (var item in lstPos)
                    {
                        response.SendRoomingListToHotelVm.Add(new SendRoomingListToHotelVm()
                        {
                            Location = item.City + "," + item.Country,
                            ProductName = item.Product_Name,
                            StartDate = item.STARTDATE.HasValue ? item.STARTDATE.Value.ToString("dd MMM") : string.Empty,
                            PositionId = item.Position_Id,
                            Status = item.STATUS,
                            FullFormStatus = lstStatus.Where(a => a.Status?.ToLower() == item.STATUS?.ToLower())?.FirstOrDefault()?.Description,
                            PlaceHolder = products_Lites.Where(a => a.VoyagerProduct_Id == item.Product_Id)?.FirstOrDefault()?.Placeholder
                        });
                    }
                    response.Response.Status = "Success";
                }
                else
                {
                    response.Response.Status = "Failure";
                    response.Response.ErrorMessage = "Booking No can not null/empty.";
                }
            }
            catch (Exception ex)
            {
                response.Response.Status = "Failure";
                response.Response.ErrorMessage = !string.IsNullOrEmpty(ex.Message) ? ex.Message : ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message) ? ex.InnerException.Message : "";
            }
            return response;
        }
        #endregion

        #region View Service Status->Itinerary
        public async Task<OpsBookingItineraryGetRes> GetBookingItineraryDetails(OpsBookingItineraryGetReq request)
        {
            OpsBookingItineraryGetRes objOpsBookingItineraryGetRes = new OpsBookingItineraryGetRes()
            {
                ResponseStatus = new ResponseStatus(),
                OpsItineraryDetails = new OpsItineraryDetails()
            };

            try
            {
                if (!string.IsNullOrEmpty(request.BookingNumber))
                {
                    List<OpsItinenraryDays> lstOpsItinenraryDays = new List<OpsItinenraryDays>();
                    List<OpsItineraryDayDetails> lstOpsItineraryDayDetails = new List<OpsItineraryDayDetails>();
                    List<ItineraryDetails> lstItineraryDetails = new List<ItineraryDetails>();

                    FilterDefinition<Bookings> filter = Builders<Bookings>.Filter.Empty;
                    filter = filter & Builders<Bookings>.Filter.Eq(x => x.BookingNumber, request.BookingNumber);
                    var objBooking = await _MongoContext.Bookings.Find(filter).FirstOrDefaultAsync();

                    if (objBooking != null)
                    {
                        objBooking.ItineraryDetails = objBooking.ItineraryDetails == null ? new List<ItineraryDetails>() : objBooking.ItineraryDetails;

                        if (request.BookingRemark == "noremarks")
                        {
                            lstItineraryDetails = objBooking.ItineraryDetails.Where(a => !string.IsNullOrEmpty(a.Position_Id)).ToList();
                        }
                        else if (request.BookingRemark == "withremarks")
                        {
                            lstItineraryDetails = objBooking.ItineraryDetails;
                        }
                        else if (request.BookingRemark == "onlyremarks")
                        {
                            lstItineraryDetails = objBooking.ItineraryDetails.Where(a => string.IsNullOrEmpty(a.Position_Id)).ToList();
                        }

                        var ExcludeList = new List<string>() { "-", "X", "J" };

                        var posIds = lstItineraryDetails.Select(a => a.Position_Id).ToList();
                        var lstPos = objBooking.Positions.Where(a => posIds.Contains(a.Position_Id) && !ExcludeList.Contains(a.STATUS)).ToList();
                        var lstPosStatus = lstPos.Select(a => a.STATUS).ToList();
                        var lstStatus = await _MongoContext.mStatus.Find(a => lstPosStatus.Contains(a.Status)).ToListAsync();

                        lstItineraryDetails = lstItineraryDetails.Where(a => lstPos.Select(b => b.Position_Id).ToList().Contains(a.Position_Id)).ToList();

                        if (!string.IsNullOrWhiteSpace(request.DayName))
                        {
                            lstItineraryDetails = lstItineraryDetails.Where(a => "Day " + a.DayNo == request.DayName).ToList();
                        }
                        if (!string.IsNullOrWhiteSpace(request.ProductType))
                        {
                            lstItineraryDetails = lstItineraryDetails.Where(a => a.ProductType == request.ProductType).ToList();
                        }
                        if (!string.IsNullOrWhiteSpace(request.Status))
                        {
                            lstItineraryDetails = lstItineraryDetails.Where(a => a.Status == request.Status).ToList();
                        }

                        var NoOfPax = objBooking.BookingPax.Where(a => a.PERSTYPE.ToUpper() == "ADULT").Sum(a => a.PERSONS).ToString();
                        foreach (var item in lstItineraryDetails)
                        {
                            var dayname = "Day " + item.DayNo;
                            var objOpsItineraryDayDetails = new OpsItineraryDayDetails()
                            {
                                PositionId = item.Position_Id,
                                DayName = dayname,
                                CityId = item.City_Id,
                                CityName = item.CityName,
                                CountryName = item.CountryName,
                                CountryId = item.Country_Id,
                                ProductName = item.Description,
                                ProductType = item.ProductType == null ? "" : item.ProductType.Trim().ToUpper(),
                                STARTDateLongFormat = item.STARTDATE?.ToString("dd MMM yyyy"),
                                STARTDayOfWeek = item.STARTDATE?.ToString("dddd"),
                                STARTDATE = item.STARTDATE,
                                STARTTIME = item.STARTTIME,
                                ENDDATE = item.ENDDATE,
                                ENDTIME = item.ENDTIME,
                                Status = lstPos?.Where(a => a.Position_Id == item.Position_Id)?.FirstOrDefault()?.STATUS?.Trim().ToUpper(),
                                StatusDescription = lstStatus?.Where(a => a.Status == lstPos?.Where(b => b.Position_Id == item.Position_Id)?.FirstOrDefault()?.STATUS?.Trim().ToUpper()).FirstOrDefault()?.Description,
                                Supplier = objBooking.Positions.Where(a => a.Position_Id == item.Position_Id).FirstOrDefault()?.SupplierInfo?.Name,
                                NoOfPax = NoOfPax,
                                Price = "",
                                Allocation = "No"
                            };
                            if (lstOpsItinenraryDays.Where(a => a.DayName == dayname).Count() == 0)
                            {
                                lstOpsItinenraryDays.Add(new OpsItinenraryDays()
                                {
                                    DayNo = item.DayNo,
                                    DayName = dayname,
                                    OpsItineraryDayDetails = new List<OpsItineraryDayDetails>() { objOpsItineraryDayDetails }
                                });
                            }
                            else
                            {
                                lstOpsItinenraryDays.Where(a => a.DayName == dayname).ToList().ForEach(a =>
                                {
                                    a.OpsItineraryDayDetails.Add(objOpsItineraryDayDetails);
                                    a.OpsItineraryDayDetails = a.OpsItineraryDayDetails.OrderBy(b => b.STARTTIME).ToList();
                                });
                                //lstOpsItinenraryDays.Where(a => a.DayName == dayname).FirstOrDefault().OpsItineraryDayDetails.Add(objOpsItineraryDayDetails);
                                //lstOpsItinenraryDays.Where(a => a.DayName == dayname).FirstOrDefault().OpsItineraryDayDetails =
                                //    lstOpsItinenraryDays.Where(a => a.DayName == dayname).FirstOrDefault().OpsItineraryDayDetails.OrderBy(a => a.STARTTIME).ToList();
                            }
                        }

                        var qrfId = objBooking.QRFID;
                        if (!string.IsNullOrEmpty(qrfId))
                        {
                            var routingDays = _MongoContext.mQuote.AsQueryable().Where(x => x.QRFID == qrfId).FirstOrDefault()?.RoutingDays;
                            if (routingDays != null && routingDays.Count > 0)
                            {
                                lstOpsItinenraryDays.ForEach(a =>
                                a.CityNames = routingDays.Where(x => x.DayNo == a.DayNo).Select(y => y.GridLabel?.Trim()).FirstOrDefault());

                            }
                            else
                            {
                                lstOpsItinenraryDays.ForEach(a =>
                                a.CityNames = string.Join(",", a.OpsItineraryDayDetails.Where(b => !string.IsNullOrEmpty(b.CityName)).Select(b => b.CityName.Trim()).Distinct().ToList()));
                            }
                        }
                        else
                        {
                            lstOpsItinenraryDays.ForEach(a =>
                            a.CityNames = string.Join(",", a.OpsItineraryDayDetails.Where(b => !string.IsNullOrEmpty(b.CityName)).Select(b => b.CityName.Trim()).Distinct().ToList()));
                        }

                        lstOpsItinenraryDays = lstOpsItinenraryDays.OrderBy(a => a.DayNo).ToList();
                        objOpsBookingItineraryGetRes.OpsItineraryDetails.OpsItinenraryDays = lstOpsItinenraryDays;

                        //Days will contains list of All Day of given BookingNumber
                        objOpsBookingItineraryGetRes.OpsItineraryDetails.Days = lstOpsItinenraryDays.Select(a => a.DayName).Distinct().ToList();

                        //ProductType will contains list of All ProductTypes of given BookingNumber Itineraries
                        objOpsBookingItineraryGetRes.OpsItineraryDetails.ServiceType = lstItineraryDetails.Select(a => a.ProductType.Trim()).Distinct().OrderBy(a => a).ToList();

                        //Status will contain the list of all Itinerary Status of given BookingNumber
                        objOpsBookingItineraryGetRes.OpsItineraryDetails.BookingStaus = lstPosStatus.Distinct().OrderBy(a => a).ToList();
                    }
                    else
                    {
                        objOpsBookingItineraryGetRes.ResponseStatus.Status = "Failure";
                        objOpsBookingItineraryGetRes.ResponseStatus.ErrorMessage = "Booking Number can not be Null/Blank.";
                    }
                }
                else
                {
                    objOpsBookingItineraryGetRes.ResponseStatus.Status = "Failure";
                    objOpsBookingItineraryGetRes.ResponseStatus.ErrorMessage = "Booking Number can not be Null/Blank.";
                }
            }
            catch (Exception ex)
            {
                objOpsBookingItineraryGetRes.ResponseStatus.Status = "Failure";
                objOpsBookingItineraryGetRes.ResponseStatus.ErrorMessage = ex.Message;
            }
            return objOpsBookingItineraryGetRes;
        }

        public async Task<OpsBookingItineraryGetRes> GetPositionTypeByPositionId(OpsBookingItineraryGetReq request)
        {
            OpsBookingItineraryGetRes objOpsBookingItineraryGetRes = new OpsBookingItineraryGetRes()
            {
                ResponseStatus = new ResponseStatus(),
                OpsItineraryDetails = new OpsItineraryDetails()
            };

            try
            {
                if (!string.IsNullOrEmpty(request.BookingNumber))
                {
                    OpsPositionDetails obj = new OpsPositionDetails();

                    var booking = _MongoContext.Bookings.AsQueryable().Where(x => x.BookingNumber == request.BookingNumber).FirstOrDefault();
                    if (booking != null)
                    {
                        var systemCompanyId = booking.SystemCompany_Id;
                        var objPosition = booking.Positions;
                        if (objPosition != null && !string.IsNullOrEmpty(request.PositionId))
                        {
                            var position = objPosition.Where(x => x.Position_Id == request.PositionId).FirstOrDefault();
                            if (position != null)
                            {
                                var resSRP = await _MongoContext.mProducts_Lite.FindAsync(a => a.VoyagerProduct_Id == position.Product_Id).Result.FirstOrDefaultAsync();
                                obj.PositionType = position.PositionType;
                                if (resSRP != null)
                                {
                                    var PositionStatus = _MongoContext.mStatus.AsQueryable().Where(a => a.Status == position.STATUS).Select(a => a.Description).FirstOrDefault();
                                    obj.Placeholder = resSRP.Placeholder;
                                    obj.PositionStatus = PositionStatus;
                                    obj.systemCompanyId = systemCompanyId;
                                }

                            }
                            objOpsBookingItineraryGetRes.OpsItineraryDetails.OpsPositions.Add(obj);
                        }
                        else
                        {
                            objOpsBookingItineraryGetRes.ResponseStatus.Status = "Failure";
                            objOpsBookingItineraryGetRes.ResponseStatus.ErrorMessage = "Position Details can not be Null/Blank.";
                        }
                    }
                }
                else
                {
                    objOpsBookingItineraryGetRes.ResponseStatus.Status = "Failure";
                    objOpsBookingItineraryGetRes.ResponseStatus.ErrorMessage = "Booking Number can not be Null/Blank.";
                }
            }
            catch (Exception ex)
            {
                objOpsBookingItineraryGetRes.ResponseStatus.Status = "Failure";
                objOpsBookingItineraryGetRes.ResponseStatus.ErrorMessage = ex.Message;
            }
            return objOpsBookingItineraryGetRes;
        }
        #endregion

        #region Get Service for Opeartion 
        //For Opeartion Header
        public async Task<OperationHeaderInfo> GetOperationHeaderDetails(OpsHeaderGetReq request)
        {
            OperationHeaderInfo objOperationHeaderInfo = new OperationHeaderInfo() { ResponseStatus = new ResponseStatus() };
            var objBooking = new Bookings();
            try
            {
                if (!string.IsNullOrEmpty(request.BookingNumber))
                {
                    FilterDefinition<Bookings> filter = Builders<Bookings>.Filter.Empty;
                    filter = filter & Builders<Bookings>.Filter.Eq(x => x.BookingNumber, request.BookingNumber);
                    objBooking = await _MongoContext.Bookings.Find(filter).FirstOrDefaultAsync();
                }
                else if (!string.IsNullOrEmpty(request.PositionId))
                {
                    objBooking = await _MongoContext.Bookings.Find(a => a.Positions.Any(b => b.Position_Id == request.PositionId)).FirstOrDefaultAsync();
                }
                else
                {
                    objBooking = request.Bookings;
                }
                if (objBooking != null)
                {
                    objOperationHeaderInfo.TourName = objBooking.CustRef;
                    objOperationHeaderInfo.BookingNumber = objBooking.BookingNumber;
                    objOperationHeaderInfo.NoOfNights = Convert.ToInt16(objBooking.Duration);
                    objOperationHeaderInfo.NoOfDays = Convert.ToInt16(objBooking.Duration) + 1;
                    objOperationHeaderInfo.Destination = objBooking.Destination;
                    objOperationHeaderInfo.StartDate = objBooking.STARTDATE;
                    objOperationHeaderInfo.EndDate = objBooking.ENDDATE;
                    objOperationHeaderInfo.SalesOfficerEmail = objBooking.StaffDetails.Staff_SalesUser_Email;
                    objOperationHeaderInfo.CostingOfficerEmail = objBooking.StaffDetails.Staff_SalesSupport_Email;
                    objOperationHeaderInfo.FileHandlerEmail = objBooking.StaffDetails.Staff_OpsUser_Email;
                    objOperationHeaderInfo.ProductAccountantEmail = objBooking.StaffDetails.Staff_PAUser_Email;
                    objOperationHeaderInfo.AgentName = objBooking.AgentInfo?.Name;
                    if (!string.IsNullOrEmpty(request.PositionId))
                    {
                        var objPos = objBooking.Positions.Where(a => a.Position_Id == request.PositionId).FirstOrDefault();
                        objOperationHeaderInfo.ProductType = objPos.ProductType;
                        objOperationHeaderInfo.UIProductType = GetUIProductType(objOperationHeaderInfo.ProductType.ToUpper());
                    }

                    objOperationHeaderInfo.ResponseStatus.Status = "Success";
                }
                else
                {
                    objOperationHeaderInfo.ResponseStatus.Status = "Failure";
                    objOperationHeaderInfo.ResponseStatus.ErrorMessage = "Booking details not found.";
                }
            }
            catch (Exception ex)
            {
                objOperationHeaderInfo.ResponseStatus.Status = "Failure";
                objOperationHeaderInfo.ResponseStatus.ErrorMessage = ex.Message;
            }

            return objOperationHeaderInfo;
        }

        //Get Ops Position View details by ProductType
        public async Task<OpsProductTypeGetRes> GetOpsProductTypeDetails(OpsProductTypeGetReq request)
        {
            OpsProductTypeCommonFields objOpsProductTypeCommonFields = new OpsProductTypeCommonFields()
            {
                BookingNumber = request.BookingNumber,
                ProductType = request.ProductType,
                PositionId = request.PositionId

            };

            OpsProductTypeGetRes objOpsProductTypeGetRes = new OpsProductTypeGetRes()
            {
                DayList = new List<AttributeValues>(),
                OpsProductTypeCommonFields = objOpsProductTypeCommonFields,
                ResponseStatus = new ResponseStatus()
            };

            try
            {
                if (!string.IsNullOrEmpty(request.BookingNumber) && request.ProductType?.Count() > 0)
                {
                    FilterDefinition<Bookings> filter = Builders<Bookings>.Filter.Empty;
                    filter = filter & Builders<Bookings>.Filter.Eq(x => x.BookingNumber, request.BookingNumber);
                    var objBooking = await _MongoContext.Bookings.Find(filter).FirstOrDefaultAsync();

                    if (objBooking != null)
                    {
                        Positions objPos;
                        var prodtype = request.ProductType.ToLower().Split(",");
                        bool flag = false;
                        var dur = (objBooking.ENDDATE.Value - objBooking.STARTDATE.Value).TotalDays;

                        if (!string.IsNullOrEmpty(objBooking.QRFID))
                        {
                            FilterDefinition<mQuote> filterQuote = Builders<mQuote>.Filter.Empty;
                            filterQuote = filterQuote & Builders<mQuote>.Filter.Where(x => x.QRFID == objBooking.QRFID);
                            var objQuote = await _MongoContext.mQuote.Find(filterQuote).FirstOrDefaultAsync();
                            if (objQuote != null)
                            {
                                var routingDays = objQuote.RoutingDays;
                                for (int i = 1; i <= dur; i++)
                                {
                                    var routeDay = routingDays.Where(a => a.DayNo == i).FirstOrDefault();
                                    if (routeDay != null)
                                    {
                                        objOpsProductTypeGetRes.DayList.Add(new AttributeValues
                                        {
                                            Value = routeDay.Days,
                                            CityName = routeDay.GridLabel,
                                            CityId = routeDay.GridLabelIds,
                                            SequenceNo = routeDay.DayNo
                                        });
                                    }
                                    else
                                    {
                                        objOpsProductTypeGetRes.DayList.Add(new AttributeValues
                                        {
                                            Value = "Day " + i,
                                            CityName = "",
                                            CityId = "",
                                            SequenceNo = i
                                        });
                                    }
                                }
                            }
                            else
                            {
                                flag = true;
                            }
                        }
                        else
                        {
                            flag = true;
                        }

                        if (flag)
                        {
                            //below forloop will create the Days List with Routing cities 
                            //dur=4 1 2
                            //pos=2 3 4
                            var posList = objBooking.Positions.OrderBy(a => a.STARTDATE).ToList();//&& prodtype.Contains(a.ProductType.ToLower())
                            for (int i = 0; i < posList.Count; i++)
                            {
                                var dayno = ((objBooking.Positions[i].STARTDATE - objBooking.STARTDATE).Value.Days + 1).ToString();
                                if (objOpsProductTypeGetRes.DayList.Where(a => a.Value == "Day " + dayno).FirstOrDefault() != null)
                                {
                                    if (!objOpsProductTypeGetRes.DayList.Where(a => a.Value == "Day " + dayno).FirstOrDefault().CityName.Contains(posList[i].City.Trim()))
                                    {
                                        objOpsProductTypeGetRes.DayList.Where(a => a.Value == "Day " + dayno).FirstOrDefault().CityName += ", " + posList[i].City.Trim();
                                        objOpsProductTypeGetRes.DayList.Where(a => a.Value == "Day " + dayno).FirstOrDefault().CityId += ", " + posList[i].City_Id;
                                    }
                                }
                                else
                                {
                                    objOpsProductTypeGetRes.DayList.Add(new AttributeValues
                                    {
                                        Value = "Day " + dayno,
                                        CityName = posList[i].City.Trim(),
                                        CityId = posList[i].City_Id,
                                        SequenceNo = Convert.ToInt32(dayno)
                                    });
                                }
                            }

                            for (int i = 1; i <= dur; i++)
                            {
                                var day = objOpsProductTypeGetRes.DayList.Where(a => a.SequenceNo == i).FirstOrDefault();
                                if (day == null)
                                {
                                    objOpsProductTypeGetRes.DayList.Add(new AttributeValues
                                    {
                                        Value = "Day " + i,
                                        CityName = "",
                                        CityId = "",
                                        SequenceNo = i
                                    });
                                }
                            }
                        }

                        objOpsProductTypeGetRes.DayList = objOpsProductTypeGetRes?.DayList?.OrderBy(a => a.SequenceNo).ToList();

                        //If PositionId is passed then get the details of Position
                        //If PositionId is not passed then get the 1st position details as order by STARTDATE ascending
                        if (string.IsNullOrEmpty(objOpsProductTypeGetRes.OpsProductTypeCommonFields.PositionId))
                        {
                            objPos = objBooking.Positions.Where(a => prodtype.Contains(a.ProductType.ToLower())).OrderBy(a => a.STARTDATE).FirstOrDefault();
                            if (objPos != null)
                            {
                                var dayno = ((objPos.STARTDATE - objBooking.STARTDATE).Value.Days + 1).ToString();
                                objOpsProductTypeGetRes.OpsProductTypeCommonFields.DayName = "Day " + dayno;
                                objOpsProductTypeGetRes.OpsProductTypeCommonFields.PositionId = objPos.Position_Id;
                                objOpsProductTypeGetRes.Position = objPos;
                            }
                            else
                            {
                                objOpsProductTypeGetRes.OpsProductTypeCommonFields.DayName = objOpsProductTypeGetRes.DayList?.FirstOrDefault()?.Value;
                                objOpsProductTypeGetRes.Position = new Positions();
                            }
                        }
                        else
                        {
                            objPos = objBooking.Positions.Where(a => prodtype.Contains(a.ProductType.ToLower()) && a.Position_Id == request.PositionId).OrderBy(a => a.STARTDATE).FirstOrDefault();
                            var dayno = ((objPos.STARTDATE - objBooking.STARTDATE).Value.Days + 1).ToString();
                            if (objPos != null)
                            {
                                objOpsProductTypeGetRes.OpsProductTypeCommonFields.DayName = "Day " + dayno;
                                objOpsProductTypeGetRes.Position = objPos;
                            }
                            else
                            {
                                objOpsProductTypeGetRes.OpsProductTypeCommonFields.DayName = objOpsProductTypeGetRes.DayList?.FirstOrDefault()?.Value;
                                objOpsProductTypeGetRes.Position = new Positions();
                            }
                        }
                        OpsProductTypeDetails objOpsProductTypeDetails = await GetOPSProductTypeDetailsByPosition(objOpsProductTypeGetRes.Position);
                        objOpsProductTypeGetRes.OpsProductTypeDetails = objOpsProductTypeDetails;
                        objOpsProductTypeGetRes.ScheduleDetailsList = GetScheduleDetailsList(objBooking, objPos.STARTDATE.Value);
                        objOpsProductTypeGetRes.OpsProductTypeCommonFields.BookingId = objBooking.Booking_Id;
                        objOpsProductTypeGetRes.OpsProductTypeCommonFields.IsRealSupplier = objBooking.SystemCompany_Id == objPos.SupplierInfo.Id ? false : true;
                        objOpsProductTypeGetRes.SpecificDayItineraryDetails = objBooking.ItineraryDetails.Where(a => a.STARTDATE == objPos.STARTDATE).ToList();
                        objOpsProductTypeGetRes.OpsProductTypeCommonFields.SystemCompany_Id = objBooking.SystemCompany_Id;

                        objOpsProductTypeGetRes.ResponseStatus.Status = "Success";

                        if (!string.IsNullOrEmpty(objPos?.ProductType_Id))
                        {
                            objOpsProductTypeGetRes.OpsProductTypeCommonFields.ChargeBasis = _MongoContext.mProductType.AsQueryable().Where(a => a.VoyagerProductType_Id == objPos.ProductType_Id).FirstOrDefault()?.ChargeBasis;
                        }
                    }
                    else
                    {
                        objOpsProductTypeGetRes.ResponseStatus.Status = "Failure";
                        objOpsProductTypeGetRes.ResponseStatus.ErrorMessage = "Booking details not found.";
                    }
                }
                else
                {
                    objOpsProductTypeGetRes.ResponseStatus.Status = "Failure";
                    objOpsProductTypeGetRes.ResponseStatus.ErrorMessage = "BookingNumber/ProductType can not be null/blank.";
                }
            }
            catch (Exception ex)
            {
                objOpsProductTypeGetRes.ResponseStatus.Status = "Failure";
                objOpsProductTypeGetRes.ResponseStatus.ErrorMessage = ex.Message;
            }
            return objOpsProductTypeGetRes;
        }

        //Get Ops Position View details by PositionId
        public async Task<OpsProductTypeGetRes> GetProdTypePositionByParam(OpsProdTypePositionGetReq request)
        {
            OpsProductTypeCommonFields objOpsProductTypeCommonFields = new OpsProductTypeCommonFields()
            {
                BookingNumber = request.BookingNumber,
                DayName = request.DayName,
                ProductType = request.ProductType
            };

            OpsProductTypeGetRes objOpsProductTypeGetRes = new OpsProductTypeGetRes()
            {
                DayList = new List<AttributeValues>(),
                OpsProductTypeCommonFields = objOpsProductTypeCommonFields,
                ResponseStatus = new ResponseStatus()
            };

            //OpsProductTypeGetRes objOpsProductTypeGetRes = new OpsProductTypeGetRes();

            try
            {
                if (!string.IsNullOrEmpty(request.BookingNumber) && ((!string.IsNullOrEmpty(request.DayName) && !string.IsNullOrEmpty(request.ProductType)) || !string.IsNullOrEmpty(request.PositionId)))
                {
                    FilterDefinition<Bookings> filter = Builders<Bookings>.Filter.Empty;
                    filter = filter & Builders<Bookings>.Filter.Eq(x => x.BookingNumber, request.BookingNumber);
                    var objBooking = await _MongoContext.Bookings.Find(filter).FirstOrDefaultAsync();

                    if (objBooking != null)
                    {
                        #region Get the Position by ProductType and DayName
                        var prodtype = request.ProductType?.ToLower()?.Split(",");
                        if (!string.IsNullOrEmpty(request.PositionId) || (prodtype != null && prodtype.Count() > 0))
                        {
                            Positions objPos = new Positions();
                            if (prodtype != null && prodtype.Count() > 0)
                            {
                                objPos = objBooking.Positions.Where(a => prodtype.Contains(a.ProductType.ToLower()) && "Day " + ((a.STARTDATE - objBooking.STARTDATE).Value.Days + 1).ToString() == request.DayName)
                                    .OrderBy(a => a.STARTDATE).FirstOrDefault();
                            }
                            else if (!string.IsNullOrEmpty(request.PositionId))
                            {
                                objPos = objBooking.Positions.Where(a => a.Position_Id == request.PositionId).FirstOrDefault();
                            }
                            OpsProductTypeDetails objOpsProductTypeDetails = await GetOPSProductTypeDetailsByPosition(objPos);

                            objOpsProductTypeGetRes.OpsProductTypeDetails = objOpsProductTypeDetails;
                            objOpsProductTypeGetRes.ScheduleDetailsList = GetScheduleDetailsList(objBooking, objPos.STARTDATE.Value);
                            objOpsProductTypeGetRes.OpsProductTypeCommonFields.PositionId = objPos?.Position_Id;
                            objOpsProductTypeGetRes.OpsProductTypeCommonFields.ProductType = objPos?.ProductType;
                            objOpsProductTypeGetRes.OpsProductTypeCommonFields.DayName = "Day " + ((objPos?.STARTDATE - objBooking.STARTDATE).Value.Days + 1);
                            objOpsProductTypeGetRes.OpsProductTypeCommonFields.IsRealSupplier = objBooking.SystemCompany_Id == objPos.SupplierInfo.Id ? false : true;
                            objOpsProductTypeGetRes.OpsProductTypeCommonFields.SystemCompany_Id = objBooking.SystemCompany_Id;
                            objOpsProductTypeGetRes.Position = objPos;
                            objOpsProductTypeGetRes.SpecificDayItineraryDetails = objBooking.ItineraryDetails.Where(a => a.STARTDATE == objPos.STARTDATE).ToList();

                            if (!string.IsNullOrEmpty(objPos?.ProductType_Id))
                            {
                                objOpsProductTypeGetRes.OpsProductTypeCommonFields.ChargeBasis = _MongoContext.mProductType.AsQueryable().Where(a => a.VoyagerProductType_Id == objPos.ProductType_Id).FirstOrDefault()?.ChargeBasis;
                            }
                        }
                        else
                        {
                            objOpsProductTypeGetRes.ResponseStatus.Status = "Failure";
                            objOpsProductTypeGetRes.ResponseStatus.ErrorMessage = "Product Type can not be null/blank.";
                        }
                        #endregion

                        objOpsProductTypeGetRes.OpsProductTypeCommonFields.BookingId = objBooking.Booking_Id;
                    }
                    else
                    {
                        objOpsProductTypeGetRes.ResponseStatus.Status = "Failure";
                        objOpsProductTypeGetRes.ResponseStatus.ErrorMessage = "Booking details not found.";
                    }
                }
                else
                {
                    objOpsProductTypeGetRes.ResponseStatus.Status = "Failure";
                    objOpsProductTypeGetRes.ResponseStatus.ErrorMessage = "BookingNumber/DayName/ProductType parameters can not be blank.";
                }
            }
            catch (Exception ex)
            {
                objOpsProductTypeGetRes.ResponseStatus.Status = "Failure";
                objOpsProductTypeGetRes.ResponseStatus.ErrorMessage = ex.Message;
            }
            return objOpsProductTypeGetRes;
        }

        //Internal Common function for GetProdTypePositionByParam & GetOpsProductTypeDetails
        public async Task<OpsProductTypeDetails> GetOPSProductTypeDetailsByPosition(Positions position)
        {
            OpsProductTypeDetails objOpsProductTypeDetails = new OpsProductTypeDetails();
            try
            {
                #region ProductSRPDetails
                if (position != null)
                {
                    var resSRP = await _MongoContext.mProducts_Lite.FindAsync(a => a.VoyagerProduct_Id == position.Product_Id).Result.FirstOrDefaultAsync();
                    if (resSRP != null)
                    {
                        var PositionStatus = _MongoContext.mStatus.AsQueryable().Where(a => a.Status == position.STATUS).Select(a => a.Description).FirstOrDefault();
                        objOpsProductTypeDetails.ProductSRPDetails = new ProductSRPDetails
                        {
                            Address = resSRP.Address,
                            BdgPriceCategory = resSRP.BdgPriceCategory,
                            Chain = resSRP.Chain,
                            CityName = resSRP.CityName,
                            CountryName = resSRP.CountryName,
                            CreateDate = resSRP.CreateDate,
                            CreateUser = resSRP.CreateUser,
                            DefaultSupplierId = resSRP.DefaultSupplierId,
                            DefaultSupplier = resSRP.DefaultSupplier,
                            EditDate = resSRP.EditDate,
                            EditUser = resSRP.EditUser,
                            HotelImageURL = resSRP.HotelImageURL,
                            HotelType = resSRP.HotelType,
                            Location = resSRP.Location,
                            Placeholder = resSRP.Placeholder,
                            PostCode = resSRP.PostCode,
                            ProdDesc = resSRP.ProdDesc,
                            ProdName = resSRP.ProdName,
                            ProductCode = resSRP.ProductCode,
                            ProductFacilities = resSRP.ProductFacilities,
                            ProductType = resSRP.ProductType,
                            ProductType_Id = resSRP.ProductType_Id,
                            Rooms = resSRP.Rooms,
                            StarRating = resSRP.StarRating,
                            Status = resSRP.Status,
                            Street = resSRP.Street,
                            VoyagerProduct_Id = resSRP.VoyagerProduct_Id,
                            PositionStatus = PositionStatus,
                            PositionStatusSCode = position.STATUS,
                            TotalAmount = Convert.ToDecimal(position.Pricing?.Where(a => a.Status == "A").FirstOrDefault()?.BuyingPrice),
                            AmountCurrency = position.Pricing?.Where(a => a.Status == "A").FirstOrDefault()?.BuyCurrency_Name,
                            SupplierName = position.SupplierInfo?.Name,
                            SupplierContactName = position.SupplierInfo?.Contact_Name,
                            SupplierContactTel = position.SupplierInfo?.Contact_Tel,
                            SupplierContactEmail = position.SupplierInfo?.Contact_Email,
                        };

                        objOpsProductTypeDetails.ResponseStatus.Status = "Success";
                    }
                    else
                    {
                        objOpsProductTypeDetails.ResponseStatus.Status = "Failure";
                        objOpsProductTypeDetails.ResponseStatus.ErrorMessage = "Product_Id:- " + position.Product_Id + " not found in mProducts_Lite.";
                        //objOpsProductTypeDetails.ProductSRPDetails = new ProductSRPDetails();
                    }
                }
                else
                {
                    objOpsProductTypeDetails.ResponseStatus.Status = "Failure";
                    objOpsProductTypeDetails.ResponseStatus.ErrorMessage = "PositionId can not be null/blank.";
                    objOpsProductTypeDetails.ProductSRPDetails = new ProductSRPDetails();
                }
                #endregion
            }
            catch (Exception ex)
            {
                objOpsProductTypeDetails.ResponseStatus.Status = "Failure";
                objOpsProductTypeDetails.ResponseStatus.ErrorMessage = ex.Message;
            }
            return objOpsProductTypeDetails;
        }

        public List<ScheduleDetails> GetScheduleDetailsList(Bookings booking, DateTime PositionDate)
        {
            List<ScheduleDetails> ScheduleDetailsList = new List<ScheduleDetails>();
            try
            {
                ScheduleDetailsList = booking.Positions.Where(a => a.STARTDATE == PositionDate).OrderBy(a => a.STARTTIME)
                                        .Select(a => new ScheduleDetails
                                        {
                                            PositionId = a.Position_Id,
                                            ProductName = a.Product_Name,
                                            ProductType = a.ProductType,
                                            StartTime = a.STARTTIME,
                                            EndTime = a.ENDTIME,
                                            StartDate = a.STARTDATE.Value
                                        }).ToList();

                return ScheduleDetailsList;
            }
            catch (Exception ex)
            {
                return new List<ScheduleDetails>();
            }
        }

        public string GetUIProductType(string prodType)
        {
            string UIProdType = "";
            switch (prodType)
            {
                case "HOTEL":
                    UIProdType = "Accommodation";
                    break;
                case "MEAL":
                    UIProdType = "Meals";
                    break;
                case "ATTRACTIONS":
                case "SIGHTSEEING - CITYTOUR":
                    UIProdType = "Activities";
                    break;
                case "LDC":
                case "COACH":
                    UIProdType = "Bus";
                    break;
                case "OVERNIGHT FERRY":
                    UIProdType = "Cruise";
                    break;
                case "TRAIN":
                    UIProdType = "Rail";
                    break;
                case "PRIVATE TRANSFER":
                case "SCHEDULED TRANSFER":
                case "FERRY TRANSFER":
                case "FERRY PASSENGER":
                    UIProdType = "Transfers";
                    break;
                case "DOMESTIC FLIGHT":
                    UIProdType = "Flights";
                    break;
                case "GUIDE":
                case "ASSISTANT":
                    UIProdType = "Local Guide";
                    break;
                case "VISA":
                case "INSURANCE":
                case "OTHER":
                case "FEE":
                    UIProdType = "Others";
                    break;
                case "ASSISTANTTE":
                    UIProdType = "Tour Entities";
                    break;
                default:
                    UIProdType = "";
                    break;
            }

            return UIProdType;
        }

        public async Task<OpsProdRangePersTypeGetRes> GetPersonTypeByProductRange(OpsProdRangePersTypeGetReq request)
        {
            OpsProdRangePersTypeGetRes objOpsProductTypeGetRes = new OpsProdRangePersTypeGetRes();
            try
            {
                if (!string.IsNullOrEmpty(request.ProductId) && !string.IsNullOrEmpty(request.ProdCategory) && !string.IsNullOrEmpty(request.ProdRangeId))
                {
                    var objResponse = _MongoContext.Products.AsQueryable().Where(a => a.VoyagerProduct_Id == request.ProductId).FirstOrDefault()?
                        .ProductCategories?.Where(a => a.ProductCategoryName == request.ProdCategory).FirstOrDefault()?
                        .ProductRanges?.Where(a => a.ProductRange_Id == request.ProdRangeId)
                        .Select(a => new OpsProdRangePersTypeGetRes { PersonType = a.PersonType, PersonTypeId = a.PersonType_Id }).FirstOrDefault();

                    if (objResponse != null)
                    {
                        objOpsProductTypeGetRes = objResponse;
                        objOpsProductTypeGetRes.ResponseStatus.Status = "Success";
                    }
                    else
                    {
                        objOpsProductTypeGetRes.ResponseStatus.Status = "Failure";
                        objOpsProductTypeGetRes.ResponseStatus.ErrorMessage = "Product details not found.";
                    }
                }
                else
                {
                    objOpsProductTypeGetRes.ResponseStatus.Status = "Failure";
                    objOpsProductTypeGetRes.ResponseStatus.ErrorMessage = "ProductId/ProdCategory/ProdRangeId parameters can not be blank.";
                }
            }
            catch (Exception ex)
            {
                objOpsProductTypeGetRes.ResponseStatus.Status = "Failure";
                objOpsProductTypeGetRes.ResponseStatus.ErrorMessage = ex.Message;
            }
            return objOpsProductTypeGetRes;
        }
        #endregion

        #region Booking Position FOC

        public async Task<OpsFOCSetRes> SetBookingPositionFOC(OpsFOCSetReq request)
        {
            OpsFOCSetRes response = new OpsFOCSetRes();
            List<PositionFOC> lstFOC = new List<PositionFOC>();
            try
            {
                if (!string.IsNullOrWhiteSpace(request.BookingNo))
                {
                    var booking = _MongoContext.Bookings.AsQueryable().Where(x => x.BookingNumber == request.BookingNo).FirstOrDefault();

                    if (booking != null)
                    {
                        var position = booking.Positions?.Where(y => y.Position_Id == request.Position_Id).FirstOrDefault();

                        if (position != null && !string.IsNullOrWhiteSpace(request.Position_Id) && request.PositionFoc != null & request.PositionFoc.Count > 0)
                        {
                            request.PositionFoc.RemoveAll(x => x.BuyQuantity == null && x.GetQuantity == null);
                            position.PositionFOC = request.PositionFoc.Distinct().ToList();
                        }
                        else
                            position.PositionFOC = new List<PositionFOC>();

                        await _MongoContext.Bookings.UpdateOneAsync(Builders<Bookings>.Filter.Eq("BookingNumber", request.BookingNo),
                            Builders<Bookings>.Update.Set("Positions", booking.Positions));
                    }
                    response.ResponseStatus.Status = "Success";
                    response.ResponseStatus.StatusMessage = "Record Saved Successfully.";
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Position Id not found";
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

        #region Financials

        public async Task<OpsFinancialsGetRes> GetOpsFinancialDetails(OpsFinancialsGetReq request)
        {
            OpsFinancialsGetRes objOpsFinancialsGetRes = new OpsFinancialsGetRes();
            List<FinancialDetail> lstFinancialDetail = new List<FinancialDetail>();
            try
            {
                if (!string.IsNullOrWhiteSpace(request.BookingNumber))
                {
                    var position = _MongoContext.Bookings.AsQueryable().Where(x => x.BookingNumber == request.BookingNumber).FirstOrDefault()?.Positions?.Where(y => y.Position_Id == request.PositionId).FirstOrDefault();
                    var pricing = position.Pricing?.Where(z => z.Status.ToUpper() == "A").FirstOrDefault();
                    var pricingDetail = pricing?.PricingDetail;
                    var bookingRoomsAndPrices = position?.BookingRoomsAndPrices;
                    decimal? buyprice;

                    if (pricing != null)
                    {
                        objOpsFinancialsGetRes.TotalBuyCurrency = pricing.BuyCurrency_Name;
                        objOpsFinancialsGetRes.TotalBuyPrice = Convert.ToString(pricing.BuyingPrice);
                        objOpsFinancialsGetRes.TotalSellCurrency = pricing.SellCurrency_Name;
                        objOpsFinancialsGetRes.TotalSellPrice = Convert.ToString(pricing.Gross_SellPrice);
                        objOpsFinancialsGetRes.TotalGPPercent = Convert.ToString(pricing.Gross_Margin_Perc);
                        objOpsFinancialsGetRes.TotalGPAmount = Convert.ToString((pricing.Gross_SellPrice) - (pricing.Net_SellPrice));
                    }

                    if (pricingDetail != null && pricingDetail.Count > 0)
                    {
                        foreach (var p in pricingDetail)
                        {
                            buyprice = bookingRoomsAndPrices != null ? bookingRoomsAndPrices.Where(x => x.RoomName == p.ProductRange_Name).Select(y => y.BuyPrice).FirstOrDefault() : 0;
                            var objFinancial = new FinancialDetail()
                            {
                                Date = Convert.ToDateTime(p.PriceDate).ToString("dd MMM yy"),
                                Item = p.ProductRange_Name,
                                Quantity = Convert.ToString(p.Units),
                                ChargeBy = bookingRoomsAndPrices != null ? bookingRoomsAndPrices.Where(x => x.RoomName == p.ProductRange_Name).Select(y => y.PersonType).FirstOrDefault() : "",
                                Buy = p.BuyCurrency,
                                Rate = Convert.ToString(buyprice),
                                Total = Convert.ToString(buyprice * p.Units),
                                Basis = "%",
                                Value = Convert.ToString(p.Gross_Margin_Perc),
                                Sell = p.SellCurrency,
                                SValue = Convert.ToString(p.Gross_SellPrice),
                                GPPercent = Convert.ToString(p.Gross_Margin_Perc),
                                GPAmount = Convert.ToString((p.Gross_SellPrice) - (p.Net_SellPrice)),
                            };
                            lstFinancialDetail.Add(objFinancial);
                        }

                        objOpsFinancialsGetRes.BookingNumber = request.BookingNumber;
                        objOpsFinancialsGetRes.PositionId = request.PositionId;
                        objOpsFinancialsGetRes.FinancialDetail = lstFinancialDetail.OrderBy(x => x.Date).ThenBy(x => x.Item).ToList();
                    }
                    else
                    {
                        objOpsFinancialsGetRes.ResponseStatus.Status = "Failure";
                        objOpsFinancialsGetRes.ResponseStatus.ErrorMessage = "Record not found.";
                    }
                }
                else
                {
                    objOpsFinancialsGetRes.ResponseStatus.Status = "Failure";
                    objOpsFinancialsGetRes.ResponseStatus.ErrorMessage = "Booking Number not found.";
                }
            }
            catch (Exception ex)
            {
                objOpsFinancialsGetRes.ResponseStatus.Status = "Failure";
                objOpsFinancialsGetRes.ResponseStatus.ErrorMessage = ex.Message;
            }
            return objOpsFinancialsGetRes;
        }

        #endregion

        #region Itinerary Builder

        public async Task<OpsBookingItineraryGetRes> GetItineraryBuilderDetails(OpsBookingItineraryGetReq request)
        {
            OpsBookingItineraryGetRes objOpsBookingItineraryGetRes = new OpsBookingItineraryGetRes()
            {
                ResponseStatus = new ResponseStatus(),
                OpsItineraryDetails = new OpsItineraryDetails()
            };

            try
            {
                if (!string.IsNullOrEmpty(request.BookingNumber))
                {
                    List<OpsItinenraryDays> lstOpsItinenraryDays = new List<OpsItinenraryDays>();
                    List<OpsItineraryDayDetails> lstOpsItineraryDayDetails = new List<OpsItineraryDayDetails>();
                    List<ItineraryDetails> lstItineraryDetails = new List<ItineraryDetails>();
                    List<Positions> lstPositionDetails = new List<Positions>();
                    List<Products> products = new List<Products>();
                    List<Positions> lstPositions = new List<Positions>();

                    FilterDefinition<Bookings> filter = Builders<Bookings>.Filter.Empty;
                    filter = filter & Builders<Bookings>.Filter.Eq(x => x.BookingNumber, request.BookingNumber);
                    var objBooking = await _MongoContext.Bookings.Find(filter).FirstOrDefaultAsync();

                    if (objBooking != null)
                    {
                        objBooking.ItineraryDetails = objBooking.ItineraryDetails == null ? new List<ItineraryDetails>() : objBooking.ItineraryDetails;
                        lstItineraryDetails = objBooking.ItineraryDetails;

                        var PosIds = lstItineraryDetails?.Where(a => !string.IsNullOrWhiteSpace(a.Position_Id)).Select(b => b.Position_Id).ToList();
                        if (PosIds != null && PosIds.Count > 0)
                            lstPositions = objBooking.Positions?.AsQueryable().Where(a => PosIds.Contains(a.Position_Id)).ToList();

                        var lstProdIds = lstPositions?.Select(a => a.Product_Id).ToList();
                        if (lstProdIds.Count > 0 && lstProdIds != null)
                            products = _MongoContext.Products.AsQueryable().Where(a => lstProdIds.Contains(a.VoyagerProduct_Id)).ToList();

                        if (lstPositions != null && lstPositions.Count > 0)
                        {
                            objOpsBookingItineraryGetRes.OpsItineraryDetails.OpsPositions = lstPositions.Select(a => new OpsPositionDetails()
                            {
                                Position_Id = a.Position_Id,
                                STARTTIME = a.STARTTIME,
                                ENDTIME = a.ENDTIME,
                                HOTELMEALPLAN = a.HOTELMEALPLAN,
                                BreakFastType = a.BreakFastType,
                                Porterage = a.Porterage ?? false,
                                VoucherNote = a.VoucherNote,
                                STARTLOC = a.STARTLOC,
                                ENDLOC = a.ENDLOC,
                                DriverName = a.DriverName,
                                DriverContactNumber = a.DriverContactNumber,
                                LicencePlate = a.LicencePlate,
                                Menu = a.Menu,
                                MealStyle = a.MealStyle,
                                Course = a.Course,
                                TicketLocation = a.TicketLocation,
                                GuidePurchaseTicket = a.GuidePurchaseTicket,
                                TrainNumber = a.TrainNumber,
                                ProductType = a.ProductType,
                                ProdDescription = products?.Where(x => x.VoyagerProduct_Id == a.Product_Id).FirstOrDefault()?.ProductDescription?.Where(y => y.DescType?.ToLower() == "description").Select(z => z.Description).FirstOrDefault()
                            }).ToList();
                        }

                        if (!string.IsNullOrWhiteSpace(request.DayName))
                        {
                            lstItineraryDetails = lstItineraryDetails.Where(a => "Day " + a.DayNo == request.DayName).ToList();
                        }
                        if (!string.IsNullOrWhiteSpace(request.ProductType))
                        {
                            lstItineraryDetails = lstItineraryDetails.Where(a => a.ProductType?.Trim()?.ToLower() == request.ProductType?.Trim()?.ToLower()).ToList();
                        }

                        var NoOfPax = objBooking.BookingPax.Where(a => a.PERSTYPE.ToUpper() == "ADULT").Sum(a => a.PERSONS).ToString();
                        foreach (var item in lstItineraryDetails)
                        {
                            var dayname = "Day " + item.DayNo;
                            var objOpsItineraryDayDetails = new OpsItineraryDayDetails()
                            {
                                DayNo = item.DayNo,
                                ItineraryDetailId = item.ItineraryDetail_Id,
                                PositionId = item.Position_Id,
                                DayName = dayname,
                                CityId = item.City_Id,
                                CityName = item.CityName,
                                CountryName = item.CountryName,
                                CountryId = item.Country_Id,
                                ProductName = item.Description,
                                ProductType = item.ProductType == null ? "" : item.ProductType.Trim().ToUpper(),
                                STARTDateLongFormat = item.STARTDATE?.ToString("dd MMM yyyy"),
                                STARTDayOfWeek = item.STARTDATE?.ToString("dddd"),
                                STARTDATE = item.STARTDATE,
                                STARTTIME = item.STARTTIME,
                                ENDDATE = item.ENDDATE,
                                ENDTIME = item.ENDTIME,
                                Supplier = objBooking.Positions.Where(a => a.Position_Id == item.Position_Id).FirstOrDefault()?.SupplierInfo?.Name,
                                NoOfPax = NoOfPax,
                                Price = "",
                                Allocation = "",
                                TLRemarks = item.TLRemarks,
                                OPSRemarks = item.OPSRemarks,
                                IsDeleted = item.IsDeleted,
                                ItineraryRemarks = item.ItineraryRemarks,
                                UniqueIdentityValue = item.UniqueIdentityValue
                            };
                            if (lstOpsItinenraryDays.Where(a => a.DayName == dayname).Count() == 0)
                            {
                                lstOpsItinenraryDays.Add(new OpsItinenraryDays()
                                {
                                    DayNo = item.DayNo,
                                    DayName = dayname,
                                    OpsItineraryDayDetails = new List<OpsItineraryDayDetails>() { objOpsItineraryDayDetails }
                                });
                            }
                            else
                            {
                                lstOpsItinenraryDays.Where(a => a.DayName == dayname).ToList().ForEach(a =>
                                {
                                    a.OpsItineraryDayDetails.Add(objOpsItineraryDayDetails);
                                    a.OpsItineraryDayDetails = a.OpsItineraryDayDetails.OrderBy(b => b.STARTTIME).ToList();
                                });
                            }
                        }

                        var qrfId = objBooking.QRFID;
                        if (!string.IsNullOrEmpty(qrfId))
                        {
                            var routingDays = _MongoContext.mQuote.AsQueryable().Where(x => x.QRFID == qrfId).FirstOrDefault()?.RoutingDays;
                            if (routingDays != null && routingDays.Count > 0)
                            {
                                lstOpsItinenraryDays.ForEach(a =>
                                a.CityNames = routingDays.Where(x => x.DayNo == a.DayNo).Select(y => y.GridLabel?.Trim()).FirstOrDefault());

                            }
                            else
                            {
                                lstOpsItinenraryDays.ForEach(a =>
                                a.CityNames = string.Join(",", a.OpsItineraryDayDetails.Where(b => !string.IsNullOrEmpty(b.CityName)).Select(b => b.CityName.Trim()).Distinct().ToList()));
                            }
                        }
                        else
                        {
                            lstOpsItinenraryDays.ForEach(a =>
                            a.CityNames = string.Join(",", a.OpsItineraryDayDetails.Where(b => !string.IsNullOrEmpty(b.CityName)).Select(b => b.CityName.Trim()).Distinct().ToList()));
                        }

                        lstOpsItinenraryDays = lstOpsItinenraryDays.OrderBy(a => a.DayNo).ToList();
                        objOpsBookingItineraryGetRes.OpsItineraryDetails.OpsItinenraryDays = lstOpsItinenraryDays;

                        //Days will contains list of All Day of given BookingNumber
                        objOpsBookingItineraryGetRes.OpsItineraryDetails.Days = objBooking.ItineraryDetails.OrderBy(a => a.DayNo).Select(a => "Day " + a.DayNo).Distinct().ToList();

                        //City will contains list of All Days city of given BookingNumber
                        objOpsBookingItineraryGetRes.OpsItineraryDetails.Cities = objBooking.ItineraryDetails.OrderBy(a => a.CityName).Select(a => a.CityName).Distinct().ToList();

                        //ProductType will contains list of All ProductTypes of given BookingNumber Itineraries
                        objOpsBookingItineraryGetRes.OpsItineraryDetails.ServiceType = objBooking.ItineraryDetails.OrderBy(a => a.ProductType).Where(a => !string.IsNullOrWhiteSpace(a.ProductType)).Select(a => a.ProductType.Trim()).Distinct().OrderBy(a => a).ToList();
                    }
                    else
                    {
                        objOpsBookingItineraryGetRes.ResponseStatus.Status = "Failure";
                        objOpsBookingItineraryGetRes.ResponseStatus.ErrorMessage = "Booking can not be Null/Blank.";
                    }
                }
                else
                {
                    objOpsBookingItineraryGetRes.ResponseStatus.Status = "Failure";
                    objOpsBookingItineraryGetRes.ResponseStatus.ErrorMessage = "Booking Number can not be Null/Blank.";
                }
            }
            catch (Exception ex)
            {
                objOpsBookingItineraryGetRes.ResponseStatus.Status = "Failure";
                objOpsBookingItineraryGetRes.ResponseStatus.ErrorMessage = ex.Message;
            }
            return objOpsBookingItineraryGetRes;
        }

        public async Task<OpsBookingItineraryGetRes> GetItineraryBuilderPositionDetailById(OpsBookingItineraryGetReq request)
        {
            OpsBookingItineraryGetRes objOpsBookingItineraryGetRes = new OpsBookingItineraryGetRes()
            {
                ResponseStatus = new ResponseStatus(),
                ItineraryDetails = new ItineraryDetails()
            };

            try
            {
                ItineraryDetails pos = new ItineraryDetails();
                if (!string.IsNullOrWhiteSpace(request.BookingNumber))
                {
                    FilterDefinition<Bookings> filter = Builders<Bookings>.Filter.Empty;
                    filter = filter & Builders<Bookings>.Filter.Eq(x => x.BookingNumber, request.BookingNumber);
                    var objBooking = await _MongoContext.Bookings.Find(filter).FirstOrDefaultAsync();

                    if (objBooking != null)
                    {
                        if (!string.IsNullOrWhiteSpace(request.PositionId))
                        {
                            pos = objBooking.ItineraryDetails.Where(x => x.Position_Id == request.PositionId).FirstOrDefault();
                        }
                        if (!string.IsNullOrWhiteSpace(request.ItineraryDetailId))
                        {
                            pos = objBooking.ItineraryDetails.Where(x => x.ItineraryDetail_Id == request.ItineraryDetailId).FirstOrDefault();
                        }

                        objOpsBookingItineraryGetRes.ItineraryDetails = pos;
                    }
                    else
                    {
                        objOpsBookingItineraryGetRes.ResponseStatus.Status = "Failure";
                        objOpsBookingItineraryGetRes.ResponseStatus.ErrorMessage = "Booking can not be Null/Blank.";
                    }
                }
                else
                {
                    objOpsBookingItineraryGetRes.ResponseStatus.Status = "Failure";
                    objOpsBookingItineraryGetRes.ResponseStatus.ErrorMessage = "Booking Number can not be Null/Blank.";
                }
            }
            catch (Exception ex)
            {
                objOpsBookingItineraryGetRes.ResponseStatus.Status = "Failure";
                objOpsBookingItineraryGetRes.ResponseStatus.ErrorMessage = ex.Message;
            }
            return objOpsBookingItineraryGetRes;
        }

        public async Task<OpsBookingItinerarySetRes> SetRemarksForItineraryBuilderDetails(OpsBookingItinerarySetReq request)
        {
            OpsBookingItinerarySetRes objOpsBookingItinerarySetRes = new OpsBookingItinerarySetRes()
            {
                ResponseStatus = new ResponseStatus(),
            };
            try
            {
                if (!string.IsNullOrWhiteSpace(request.BookingNumber))
                {
                    FilterDefinition<Bookings> filter = Builders<Bookings>.Filter.Empty;
                    filter = filter & Builders<Bookings>.Filter.Eq(x => x.BookingNumber, request.BookingNumber);
                    var objBooking = await _MongoContext.Bookings.Find(filter).FirstOrDefaultAsync();

                    if (objBooking != null)
                    {
                        if (request.Type == "NewElement")
                        {
                            if (!string.IsNullOrWhiteSpace(request.ItineraryDetails.ItineraryDetail_Id))
                            {
                                //update
                                var itinerary = objBooking.ItineraryDetails.Where(x => x.ItineraryDetail_Id == request.ItineraryDetails.ItineraryDetail_Id).FirstOrDefault();
                                itinerary.STARTTIME = request.ItineraryDetails.STARTTIME;
                                itinerary.ENDTIME = request.ItineraryDetails.ENDTIME;
                                itinerary.Description = request.ItineraryDetails.Description;
                                itinerary.CityName = request.ItineraryDetails.CityName;
                                objOpsBookingItinerarySetRes.ItineraryDetailId = itinerary.ItineraryDetail_Id;
                            }
                            else
                            {
                                //add
                                var newId = Guid.NewGuid().ToString();
                                objBooking.ItineraryDetails.Add(new ItineraryDetails
                                {
                                    ItineraryDetail_Id = newId,
                                    Booking_Id = objBooking.Booking_Id,
                                    Position_Id = Guid.NewGuid().ToString(),
                                    ProductType = "",
                                    CityName = request.ItineraryDetails.CityName,
                                    STARTTIME = request.ItineraryDetails.STARTTIME,
                                    ENDTIME = request.ItineraryDetails.ENDTIME,
                                    Description = request.ItineraryDetails.Description,
                                    DayNo = request.ItineraryDetails.DayNo
                                });
                                objOpsBookingItinerarySetRes.ItineraryDetailId = newId;
                            }
                        }
                        else if (!string.IsNullOrWhiteSpace(request.ItineraryDetails.ItineraryDetail_Id))
                        {
                            var itinerary = objBooking.ItineraryDetails.Where(x => x.ItineraryDetail_Id == request.ItineraryDetails.ItineraryDetail_Id).FirstOrDefault();
                            itinerary.OPSRemarks = request.ItineraryDetails.OPSRemarks;
                            itinerary.TLRemarks = request.ItineraryDetails.TLRemarks;

                            if (request.Type == "UpdateDescription")
                                itinerary.Description = request.ItineraryDetails.Description;

                            if (request.Type == "UpdateDeleted")
                                itinerary.IsDeleted = request.ItineraryDetails.IsDeleted;
                            objOpsBookingItinerarySetRes.ItineraryDetailId = request.ItineraryDetails.ItineraryDetail_Id;
                        }
                        await _MongoContext.Bookings.UpdateOneAsync(Builders<Bookings>.Filter.Eq("BookingNumber", objBooking.BookingNumber),
                          Builders<Bookings>.Update.Set("ItineraryDetails", objBooking.ItineraryDetails));
                        objOpsBookingItinerarySetRes.ResponseStatus.Status = "Success";
                    }
                    else
                    {
                        objOpsBookingItinerarySetRes.ResponseStatus.Status = "Failure";
                        objOpsBookingItinerarySetRes.ResponseStatus.ErrorMessage = "Booking Detail can not be Null/Blank.";
                    }
                }
                else
                {
                    objOpsBookingItinerarySetRes.ResponseStatus.Status = "Failure";
                    objOpsBookingItinerarySetRes.ResponseStatus.ErrorMessage = "Booking Number can not be Null/Blank.";
                }
            }
            catch (Exception ex)
            {
                objOpsBookingItinerarySetRes.ResponseStatus.Status = "Failure";
                objOpsBookingItinerarySetRes.ResponseStatus.ErrorMessage = ex.Message;
            }
            return objOpsBookingItinerarySetRes;
        }

        public async Task<OpsBookingItinerarySetRes> SetItineraryBuilderDetails(OpsBookingItinerarySetReq request)
        {
            OpsBookingItinerarySetRes objOpsBookingItinerarySetRes = new OpsBookingItinerarySetRes()
            {
                ResponseStatus = new ResponseStatus(),
            };
            try
            {
                if (!string.IsNullOrWhiteSpace(request.BookingNumber))
                {
                    FilterDefinition<Bookings> filter = Builders<Bookings>.Filter.Empty;
                    filter = filter & Builders<Bookings>.Filter.Eq(x => x.BookingNumber, request.BookingNumber);
                    var objBooking = await _MongoContext.Bookings.Find(filter).FirstOrDefaultAsync();

                    if (objBooking != null)
                    {
                        var itineraryDetails = objBooking.ItineraryDetails;

                        foreach (var oldobj in itineraryDetails)
                        {
                            foreach (var newobj in request.lstItineraryDetails)
                            {
                                if (oldobj.ItineraryDetail_Id == newobj.ItineraryDetail_Id)
                                {
                                    oldobj.ItineraryRemarks = newobj.ItineraryRemarks;
                                }
                            }
                        }
                        await _MongoContext.Bookings.UpdateOneAsync(Builders<Bookings>.Filter.Eq("BookingNumber", objBooking.BookingNumber),
                      Builders<Bookings>.Update.Set("ItineraryDetails", objBooking.ItineraryDetails));
                        objOpsBookingItinerarySetRes.ResponseStatus.Status = "Success";

                    }
                    else
                    {
                        objOpsBookingItinerarySetRes.ResponseStatus.Status = "Failure";
                        objOpsBookingItinerarySetRes.ResponseStatus.ErrorMessage = "Booking details can not be Null/Blank.";
                    }
                }
                else
                {
                    objOpsBookingItinerarySetRes.ResponseStatus.Status = "Failure";
                    objOpsBookingItinerarySetRes.ResponseStatus.ErrorMessage = "Booking Number can not be Null/Blank.";
                }
            }
            catch (Exception ex)
            {
                objOpsBookingItinerarySetRes.ResponseStatus.Status = "Failure";
                objOpsBookingItinerarySetRes.ResponseStatus.ErrorMessage = ex.Message;
            }
            return objOpsBookingItinerarySetRes;
        }
        #endregion

        #region Booking Workflow        
        public async Task<OpsBookingSetRes> SetBookingByWorkflow(OpsBookingSetReq request)
        {
            OpsBookingSetRes response = new OpsBookingSetRes();
            WorkflowActionGetReq workflowActionGetReq = new WorkflowActionGetReq();
            WorkflowActionGetRes workflowActionGetRes = new WorkflowActionGetRes();
            try
            {
                var booking = _MongoContext.Bookings.AsQueryable().Where(x => x.BookingNumber == request.BookingNumber).FirstOrDefault();
                if (booking != null)
                {
                    response.ResponseStatus = await ValidateBooking(request, booking);
                    if (response?.ResponseStatus?.Status?.ToLower() == "success")
                    {
                        GetDocTypeByWorkflowReq getDocTypeByWorkflowReq = new GetDocTypeByWorkflowReq();
                        getDocTypeByWorkflowReq.ModuleParent = request.ModuleParent;
                        getDocTypeByWorkflowReq.Module = request.Module;
                        getDocTypeByWorkflowReq.Action = request.Action;
                        getDocTypeByWorkflowReq.OpsKeyValue = request.OpsKeyValue;
                        if (request.PositionIds?.Count > 0)
                        {
                            getDocTypeByWorkflowReq.PositionStatus = booking.Positions.Where(a => a.Position_Id == request.PositionIds[0]).FirstOrDefault()?.STATUS;
                        }
                        DocTypeDetails docTypeDetails = GetDocTypeByWorkflow(getDocTypeByWorkflowReq);
                        request.DocType = docTypeDetails.DocType;
                        request.IsSendEmail = docTypeDetails.IsSendEmail;
                        request.IsSaveDocStore = docTypeDetails.IsSaveDocStore;

                        if (request.PositionIds == null)
                        {
                            request.PositionIds = new List<string>();
                            var keyValue = request.OpsKeyValue.Where(a => a.Key == "TableCancelBooking" || a.Key == "TableRoomsingListToHotel").FirstOrDefault();
                            if (keyValue != null)
                            {
                                var values = JsonConvert.DeserializeObject<List<SendRoomingListToHotelVm>>(keyValue.Value.ToString());
                                var posIds = values.Where(x => x.IsSelected == true).Select(x => x.PositionId).ToList();

                                request.PositionIds.AddRange(posIds);
                            }
                            keyValue = request.OpsKeyValue.Where(a => a.Key == "BookingPositionDwdVoucher").FirstOrDefault();
                            if (keyValue != null)
                            {
                                List<string> lstStatus = new List<string>() { "K", "B" };
                                request.PositionIds.AddRange(booking.Positions.Where(a => lstStatus.Contains(a.STATUS)).Select(a => a.Position_Id).ToList());
                            }
                            var keyValueForPositions = request.OpsKeyValue.Where(a => a.Key == "Positions").FirstOrDefault();
                            if (keyValueForPositions != null)
                            {
                                var values = JsonConvert.DeserializeObject<List<OpsBookingPositionDetails>>(keyValueForPositions.Value.ToString());
                                if (values.Any(x => x.IsSelected == true))
                                {
                                    var posIds = values.Where(x => x.IsSelected == true).Select(x => x.Position_ID).ToList();
                                    request.PositionIds.AddRange(posIds);
                                }
                                else
                                {
                                    var posIds = values.Select(x => x.Position_ID).ToList();
                                    request.PositionIds.AddRange(posIds);
                                }


                            }
                        }

                        ExecuteWorkflowActionRes exeWorkflowActionGetRes = await ExecuteWorkflowAction(request, booking);
                        response.ResponseStatus = exeWorkflowActionGetRes.ResponseStatus;

                        //if (request.ModuleParent.ToLower() == "booking" && request.Module.ToLower() == "booking" && request.Action.ToLower() == "hotelconfirm")
                        //{
                        //    response.ResponseStatus.Status = exeWorkflowActionGetRes.ResponseStatus.Status;
                        //    response.ResponseStatus.ErrorMessage = exeWorkflowActionGetRes.ResponseStatus.ErrorMessage;
                        //}
                        //else
                        //{
                        //    if (exeWorkflowActionGetRes?.ResponseStatus?.Status?.ToLower() == "success")
                        //    {
                        //        ReplaceOneResult replaceResult = await _MongoContext.Bookings.ReplaceOneAsync(Builders<Bookings>.Filter.Eq("BookingNumber", request.BookingNumber), booking);
                        //        if (replaceResult.MatchedCount > 0)
                        //        {
                        //            if (request.PositionIds?.Count > 0)
                        //            {
                        //                //call the bridge SetPosition function for update details in Postions SQL table
                        //                ResponseStatus responseStatus = await _bookingProviders.SetPositionDetails(new BookingPosSetReq() { BookingNumber = request.BookingNumber, PositionIds = request.PositionIds, UserEmail = request.UserEmailId });

                        //                if (!string.IsNullOrWhiteSpace(responseStatus?.Status))
                        //                {
                        //                    response.ResponseStatus.Status = responseStatus.Status;
                        //                    response.ResponseStatus.ErrorMessage = responseStatus.StatusMessage;
                        //                }
                        //                else
                        //                {
                        //                    response.ResponseStatus.Status = "Failure";
                        //                    response.ResponseStatus.ErrorMessage = "Booking Position details not updated at Bridge Level.";
                        //                }
                        //            }
                        //        }
                        //        else
                        //        {
                        //            response.ResponseStatus.Status = "Failure";
                        //            response.ResponseStatus.ErrorMessage = "Booking Position details not updated.";
                        //        }
                        //    }
                        //    else
                        //    {
                        //        response.ResponseStatus.Status = exeWorkflowActionGetRes.ResponseStatus.Status;
                        //        response.ResponseStatus.ErrorMessage = exeWorkflowActionGetRes.ResponseStatus.ErrorMessage;
                        //    }
                        //}
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage.Add("Booking Number " + request.BookingNumber + " not found in monogodb");
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage.Add("SetBookingByWorkflow:- " + ex.Message);
            }
            return response;
        }

        public async Task<ExecuteWorkflowActionRes> ExecuteWorkflowAction(OpsBookingSetReq request, Bookings booking)
        {
            WorkflowActionGetRes workflowActionGetRes = new WorkflowActionGetRes();
            ExecuteWorkflowActionRes response = new ExecuteWorkflowActionRes() { ResponseStatus = new OPSWorkflowResponseStatus() { Status = "Success", ErrorMessage = new List<string>() } };

            try
            {
                workflowActionGetRes = _masterRepository.GetWorkflowAction(new WorkflowActionGetReq()
                {
                    Action = request.Action,
                    Module = request.Module,
                    ModuleParent = request.ModuleParent
                }).Result;

                if (workflowActionGetRes?.WorkflowActions?.Count > 0)
                {
                    List<Workflow_Steps> StepsList = new List<Workflow_Steps>();
                    List<Workflow_Steps> BridgeStepsList = new List<Workflow_Steps>();

                    ExecuteWorkflowActionReq objExeWorkflowActionReq = new ExecuteWorkflowActionReq()
                    {
                        OpsKeyValue = request.OpsKeyValue,
                        PositionIds = request.PositionIds,
                        UserId = request.UserId,
                        UserName = request.UserName,
                        UserEmailId = request.UserEmailId,
                        DocType = request.DocType,
                        Module = request.Module,
                        IsSendEmail = request.IsSendEmail,
                        IsSaveDocStore = request.IsSaveDocStore
                    };

                    foreach (var item in workflowActionGetRes.WorkflowActions)
                    {
                        if (item.Steps != null)
                        {
                            StepsList.AddRange(item.Steps.Where(b => b.FunctionType == "Core"));
                        }

                        if (item.BridgeSteps != null)
                        {
                            BridgeStepsList.AddRange(item.BridgeSteps.Where(b => b.FunctionType == "Core"));
                        }
                    }

                    foreach (var keyValue in request.OpsKeyValue)
                    {
                        var Steps = workflowActionGetRes.WorkflowActions.Where(a => a.ModificationType.Any(b => b.field == keyValue.Key))
                                    .Select(c => c.Steps).FirstOrDefault();
                        Steps = Steps?.Where(a => a.ModificationType.Any(b => b.field == keyValue.Key)).ToList();
                        Steps?.RemoveAll(a => StepsList.Any(b => b.FunctionName == a.FunctionName));
                        if (Steps != null) StepsList.AddRange(Steps);

                        var BridgeSteps = workflowActionGetRes.WorkflowActions.Where(a => a.ModificationType.Any(b => b.field == keyValue.Key))
                                   .Select(c => c.BridgeSteps).FirstOrDefault();
                        BridgeSteps = BridgeSteps?.Where(a => a.ModificationType.Any(b => b.field == keyValue.Key)).ToList();
                        BridgeSteps?.RemoveAll(a => BridgeStepsList.Any(b => b.FunctionName == a.FunctionName));
                        if (BridgeSteps != null) BridgeStepsList.AddRange(BridgeSteps);
                    }
                    StepsList = StepsList?.OrderBy(a => a.Order).ToList();
                    BridgeStepsList = BridgeStepsList?.OrderBy(a => a.Order).ToList();

                    OPSWorkflowResponseStatus responseStatus = new OPSWorkflowResponseStatus();
                    foreach (var steps in StepsList)
                    {
                        MethodInfo addMethod = this.GetType().GetMethod(steps.FunctionName);
                        if (addMethod != null)
                        {
                            object result = addMethod.Invoke(this, new object[] { booking, objExeWorkflowActionReq });
                            responseStatus = (OPSWorkflowResponseStatus)result;

                            if (steps.IsDbCommit == true)
                            {
                                if (responseStatus?.Status?.ToLower() == "success")
                                {
                                    responseStatus = CommitBooking(ref booking);
                                }
                            }

                            if (responseStatus?.Status?.ToLower() != "success")
                            {
                                if (responseStatus?.ErrorMessage?.Count > 0)
                                {
                                    response.ResponseStatus.Status = responseStatus.Status;
                                    response.ResponseStatus.ErrorMessage.AddRange(responseStatus.ErrorMessage);
                                }
                            }
                        }
                    }

                    if (responseStatus?.Status?.ToLower() == "success")
                    {
                        ReplaceOneResult replaceResult = null;
                        if (StepsList.Where(a => a.IsDbCommit == true).ToList().Count > 0)
                        {
                            replaceResult = await _MongoContext.Bookings.ReplaceOneAsync(Builders<Bookings>.Filter.Eq("BookingNumber", request.BookingNumber), booking);
                        }

                        if (BridgeStepsList?.Count > 0)
                        {
                            foreach (var item in request.PositionIds)
                            {
                                BridgeBookingReq bridgeBookingReq = new BridgeBookingReq();
                                bridgeBookingReq.BookingNumber = request.BookingNumber;
                                bridgeBookingReq.PositionId = item;
                                bridgeBookingReq.User = request.UserEmailId;
                                bridgeBookingReq.SupplierId = booking.Positions.Where(a => a.Position_Id == item).FirstOrDefault()?.SupplierInfo?.Id;
                                bridgeBookingReq.DocType = request.DocType;

                                foreach (var bridgesteps in BridgeStepsList)
                                {
                                    MethodInfo addMethod = this.GetType().GetMethod(bridgesteps.FunctionName);
                                    if (addMethod != null)
                                    {
                                        object result = addMethod.Invoke(this, new object[] { bridgeBookingReq });
                                        var responseStatus1 = (Task<ResponseStatus>)result;

                                        if (responseStatus1.Result != null && responseStatus1.Result.Status?.ToLower() != "success")
                                        {
                                            if (!string.IsNullOrWhiteSpace(responseStatus1.Result.ErrorMessage))
                                            {
                                                response.ResponseStatus.Status = responseStatus1.Result.Status;
                                                response.ResponseStatus.ErrorMessage.Add(responseStatus1.Result.ErrorMessage);
                                            }
                                        }
                                    }
                                }

                                if (!string.IsNullOrWhiteSpace(responseStatus?.Status))
                                {
                                    response.ResponseStatus = responseStatus;
                                }
                                else
                                {
                                    response.ResponseStatus.Status = "Failure";
                                    response.ResponseStatus.ErrorMessage.Add("Booking Position details not updated at Bridge Level.");
                                }
                            }
                        }
                        else
                        {
                            response.ResponseStatus = responseStatus;
                        }
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage.Add("Workflow Actions not found in database");
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage.Add("ExecuteWorkflowAction:- An Error Occurs - " + ex.Message);
            }

            return response;
        }

        /// <summary>
        /// In this function configure the DocType for WorkFlow Actions
        /// e.g. for Book Confirmation,Booking Cancel
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public DocTypeDetails GetDocTypeByWorkflow(GetDocTypeByWorkflowReq request)
        {
            DocTypeDetails docTypeDetails = new DocTypeDetails();
            bool IsSendEmail = true;
            bool IsSaveDocStore = false;
            var docType = "";

            if (request.ModuleParent.ToLower() == "booking" && request.Module.ToLower() == "position" && request.Action.ToLower() == "book")
            {
                docType = DocType.BOOKKK;
            }
            else if (request.ModuleParent.ToLower() == "booking" && request.Module.ToLower() == "position" && request.Action.ToLower() == "cancel")
            {
                docType = DocType.BOOKXX;
            }
            else if (request.ModuleParent.ToLower() == "booking" && request.Module.ToLower() == "booking" && request.Action.ToLower() == "hotelconfirm")
            {
                docType = DocType.OPSHOTELCONFIRM;
            }
            else if (request.ModuleParent.ToLower() == "position" && request.Module.ToLower() == "supplier" && request.Action.ToLower() == "changesupplier_placeholder")
            {
                List<string> changeSupplierStatus = new List<string>() { "o", "k", "B", "i", "m" };
                if (changeSupplierStatus.Contains(request.PositionStatus.ToLower()))
                {
                    docType = DocType.BOOKKK;
                }
                else
                {
                    docType = DocType.BOOKREQ;
                }
            }
            else if (request.ModuleParent.ToLower() == "position" && request.Module.ToLower() == "supplier" && request.Action.ToLower() == "changesupplier_real")
            {
                List<string> changeSupplierStatus = new List<string>() { "o", "k", "B", "i", "m" };
                if (changeSupplierStatus.Contains(request.PositionStatus.ToLower()))
                {
                    docType = DocType.BOOKKK;
                }
                else
                {
                    docType = DocType.BOOKREQ;
                }
            }
            else if (request.ModuleParent.ToLower() == "booking" && (request.Module.ToLower() == "position" && request.Action.ToLower() == "raisevoucher")
                || (request.Module.ToLower() == "booking" && request.Action.ToLower() == "downloadvoucher"))
            {
                docType = DocType.OPSVOUCHER;
                IsSendEmail = false;
            }
            else if (request.ModuleParent.ToLower() == "booking" && request.Module.ToLower() == "position" && request.Action.ToLower() == "sendroominglisttohotel")
            {
                docType = DocType.OPSROOMING;
            }
            else if (request.ModuleParent.ToLower() == "booking" && request.Module.ToLower() == "position" && request.Action.ToLower() == "fullitinerary")
            {
                docType = DocType.OPSFULLITINERARY;
                IsSendEmail = false;
            }
            else if (request.ModuleParent.ToLower() == "booking" && request.Module.ToLower() == "booking" && request.Action.ToLower() == "changeroomcount")
            {
                docType = DocType.OPSPOSAMEND;
            }

            else if (request.ModuleParent.ToLower() == "booking" && request.Module.ToLower() == "position" && request.Action.ToLower() == "save")
            {
                var keyValue = request.OpsKeyValue.Where(a => a.Key == "SendBookAmend").FirstOrDefault();
                bool IsBookAmend = Convert.ToBoolean(keyValue?.Value);
                if (IsBookAmend)
                {
                    docType = DocType.OPSPOSAMEND;
                    keyValue = request.OpsKeyValue.Where(a => a.Key == "ConfirmBookAmend").FirstOrDefault();
                    IsBookAmend = Convert.ToBoolean(keyValue?.Value);

                    if (IsBookAmend)
                    {
                        IsSendEmail = true;
                    }
                    else
                    {
                        IsSaveDocStore = true;
                        IsSendEmail = false;
                    }
                }
            }


            docTypeDetails.DocType = docType;
            docTypeDetails.IsSendEmail = IsSendEmail;
            docTypeDetails.IsSaveDocStore = IsSaveDocStore;

            return docTypeDetails;
        }

        #region Booking Workflow Functions
        public OPSWorkflowResponseStatus UpdatePositionKeyDetails(ref Bookings booking, ExecuteWorkflowActionReq request)
        {
            OPSWorkflowResponseStatus response = new OPSWorkflowResponseStatus() { Status = "Success" };
            try
            {
                if (booking != null && request.OpsKeyValue?.Count > 0)
                {
                    if (request.PositionIds?.Count > 0)
                    {
                        var KeyValueList = request.OpsKeyValue;
                        var keyValue = new OpsKeyValue();
                        string cancelReason = "";
                        string additionalInfo = "";
                        bool auditStatusPosFlag = false;
                        bool auditBookingFlag = false;

                        var positionList = booking.Positions.Where(x => request.PositionIds.Contains(x.Position_Id)).ToList();

                        if (positionList?.Count > 0)
                        {
                            foreach (var position in positionList)
                            {
                                auditStatusPosFlag = false;

                                #region Common Fields
                                keyValue = KeyValueList.Where(a => a.Key == "Status").FirstOrDefault();
                                if (keyValue != null)
                                {
                                    position.STATUS = Convert.ToString(keyValue.Value);
                                    auditStatusPosFlag = true;
                                }
                                #endregion

                                #region CancelBooking
                                keyValue = KeyValueList.Where(a => a.Key == "TableCancelBooking").FirstOrDefault();
                                if (keyValue != null)
                                {
                                    keyValue = KeyValueList.Where(a => a.Key == "CancelReason").FirstOrDefault();
                                    if (keyValue != null)
                                        cancelReason = Convert.ToString(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "AdditionalInfo").FirstOrDefault();
                                    if (keyValue != null)
                                        additionalInfo = Convert.ToString(keyValue.Value);

                                    position.CancellationReason = cancelReason + (!string.IsNullOrWhiteSpace(additionalInfo) ? " | " + additionalInfo : "");
                                    position.CancellationDate = DateTime.Now;
                                    position.CancellationUser = request.UserEmailId;
                                }
                                #endregion

                                #region CancelPosition
                                keyValue = KeyValueList.Where(a => a.Key == "CancelReason").FirstOrDefault();
                                if (keyValue != null)
                                {
                                    cancelReason = Convert.ToString(keyValue.Value);

                                    position.CancellationReason = cancelReason;
                                    position.CancellationDate = DateTime.Now;
                                    position.CancellationUser = request.UserEmailId;
                                }
                                #endregion

                                #region Save Position
                                keyValue = KeyValueList.Where(a => a.Key == "saveposition").FirstOrDefault();
                                if (keyValue != null)
                                {
                                    keyValue = KeyValueList.Where(a => a.Key == "FileHandler").FirstOrDefault();
                                    if (keyValue != null)
                                        position.HotelPLacer_Name = Convert.ToString(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "FileHandlerID").FirstOrDefault();
                                    if (keyValue != null)
                                        position.HotelPLacer_ID = Convert.ToString(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "PositionType").FirstOrDefault();
                                    if (keyValue != null)
                                        position.PositionType = Convert.ToString(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "PaxType").FirstOrDefault();
                                    if (keyValue != null)
                                        position.StandardRooming = Convert.ToBoolean(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "StartDate").FirstOrDefault();
                                    if (keyValue != null)
                                    {
                                        position.STARTDATE = _genericRepository.ConvertStringToDateTime(Convert.ToString(keyValue.Value));
                                        var activepos = booking.Positions.Where(a => a.STATUS.ToLower() != "c").ToList();
                                        if (activepos != null && activepos.Count > 0)
                                        {
                                            auditBookingFlag = true;
                                            booking.STARTDATE = activepos.Where(a => a.STARTDATE != null).OrderBy(a => a.STARTDATE).ThenBy(a => a.STARTTIME).FirstOrDefault().STARTDATE;
                                        }
                                    }

                                    keyValue = KeyValueList.Where(a => a.Key == "StartTime").FirstOrDefault();
                                    if (keyValue != null)
                                        position.STARTTIME = Convert.ToString(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "StartLocation").FirstOrDefault();
                                    if (keyValue != null)
                                        position.STARTLOC = Convert.ToString(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "EndDate").FirstOrDefault();
                                    if (keyValue != null)
                                    {
                                        position.ENDDATE = _genericRepository.ConvertStringToDateTime(Convert.ToString(keyValue.Value));
                                        var activepos = booking.Positions.Where(a => a.STATUS.ToLower() != "c").ToList();
                                        if (activepos != null && activepos.Count > 0)
                                        {
                                            auditBookingFlag = true;
                                            booking.ENDDATE = activepos.OrderByDescending(a => a.ENDDATE).ThenByDescending(a => a.ENDTIME).FirstOrDefault().ENDDATE;
                                        }
                                    }

                                    keyValue = KeyValueList.Where(a => a.Key == "EndTime").FirstOrDefault();
                                    if (keyValue != null)
                                        position.ENDTIME = Convert.ToString(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "EndLocation").FirstOrDefault();
                                    if (keyValue != null)
                                        position.ENDLOC = Convert.ToString(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "OptionDate").FirstOrDefault();
                                    if (keyValue != null)
                                        position.OPTIONDATE = _genericRepository.ConvertStringToDateTime(Convert.ToString(keyValue.Value));

                                    keyValue = KeyValueList.Where(a => a.Key == "ConfirmDate").FirstOrDefault();
                                    if (keyValue != null)
                                        position.ConfirmDate = _genericRepository.ConvertStringToDateTime(Convert.ToString(keyValue.Value));

                                    keyValue = KeyValueList.Where(a => a.Key == "CancellationDate").FirstOrDefault();
                                    if (keyValue != null)
                                        position.CancellationDate = _genericRepository.ConvertStringToDateTime(Convert.ToString(keyValue.Value));

                                    #region Hotel
                                    keyValue = KeyValueList.Where(a => a.Key == "BoardBasis").FirstOrDefault();
                                    if (keyValue != null)
                                        position.HOTELMEALPLAN = Convert.ToString(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "BreakfastType").FirstOrDefault();
                                    if (keyValue != null)
                                        position.BreakFastType = Convert.ToString(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "Porterage").FirstOrDefault();
                                    if (keyValue != null)
                                        position.Porterage = Convert.ToBoolean(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "VoucherNote").FirstOrDefault();
                                    if (keyValue != null)
                                        position.VoucherNote = Convert.ToString(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "CancellationPolicy").FirstOrDefault();
                                    if (keyValue != null)
                                        position.CancellationPolicy = Convert.ToString(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "CityTaxAdvise").FirstOrDefault();
                                    if (keyValue != null)
                                        position.CityTaxAdvise = Convert.ToString(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "TLRemarks").FirstOrDefault();
                                    if (keyValue != null)
                                        position.TLRemarks = Convert.ToString(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "OpsRemarks").FirstOrDefault();
                                    if (keyValue != null)
                                        position.OPSRemarks = Convert.ToString(keyValue.Value);

                                    #endregion

                                    #region Bus
                                    keyValue = KeyValueList.Where(a => a.Key == "DriverName").FirstOrDefault();
                                    if (keyValue != null)
                                        position.DriverName = Convert.ToString(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "DriverContact").FirstOrDefault();
                                    if (keyValue != null)
                                        position.DriverContactNumber = Convert.ToString(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "DriverLanguages").FirstOrDefault();
                                    if (keyValue != null)
                                        position.DriverLanguage = Convert.ToString(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "DriverLicenceNumber").FirstOrDefault();
                                    if (keyValue != null)
                                        position.DriverLicenceNumber = Convert.ToString(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "MealsIncluded").FirstOrDefault();
                                    if (keyValue != null)
                                        position.MealsIncluded = Convert.ToBoolean(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "NumberofDriverRooms").FirstOrDefault();
                                    if (keyValue != null)
                                        position.NumberOfDriverRooms = Convert.ToInt16(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "VehicleRegistration").FirstOrDefault();
                                    if (keyValue != null)
                                        position.VehicleRegistration = Convert.ToString(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "ManufacturedDate").FirstOrDefault();
                                    if (keyValue != null)
                                        position.ManufacturedDate = _genericRepository.ConvertStringToDateTime(Convert.ToString(keyValue.Value));

                                    //keyValue = KeyValueList.Where(a => a.Key == "SafetyCertificateDate").FirstOrDefault();
                                    //if (keyValue != null)
                                    //    position.s = Convert.ToString(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "Parking").FirstOrDefault();
                                    if (keyValue != null)
                                        position.Parking = Convert.ToBoolean(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "CityPermit").FirstOrDefault();
                                    if (keyValue != null)
                                        position.CityPermits = Convert.ToBoolean(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "RoadTolls").FirstOrDefault();
                                    if (keyValue != null)
                                        position.RoadTolls = Convert.ToBoolean(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "AC").FirstOrDefault();
                                    if (keyValue != null)
                                        position.AC = Convert.ToBoolean(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "WC").FirstOrDefault();
                                    if (keyValue != null)
                                        position.WC = Convert.ToBoolean(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "Safety").FirstOrDefault();
                                    if (keyValue != null)
                                        position.Safety = Convert.ToBoolean(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "GPS").FirstOrDefault();
                                    if (keyValue != null)
                                        position.GPS = Convert.ToBoolean(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "AV").FirstOrDefault();
                                    if (keyValue != null)
                                        position.AV = Convert.ToBoolean(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "Itinerary").FirstOrDefault();
                                    if (keyValue != null)
                                        position.Itinerary = Convert.ToString(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "TypeofRoom").FirstOrDefault();
                                    if (keyValue != null)
                                        position.TypeOfRoom = Convert.ToString(keyValue.Value);
                                    #endregion

                                    #region Meal
                                    keyValue = KeyValueList.Where(a => a.Key == "Floor").FirstOrDefault();
                                    if (keyValue != null)
                                        position.Floor = Convert.ToString(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "CoachParking").FirstOrDefault();
                                    if (keyValue != null)
                                        position.CoachParkingAvailable = Convert.ToBoolean(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "MealStyle").FirstOrDefault();
                                    if (keyValue != null)
                                        position.MealStyle = Convert.ToString(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "Courses").FirstOrDefault();
                                    if (keyValue != null)
                                        position.Course = Convert.ToString(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "Tea").FirstOrDefault();
                                    if (keyValue != null)
                                        position.Tea = Convert.ToBoolean(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "Dessert").FirstOrDefault();
                                    if (keyValue != null)
                                        position.Dessert = Convert.ToBoolean(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "Water").FirstOrDefault();
                                    if (keyValue != null)
                                        position.Water = Convert.ToBoolean(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "Bread").FirstOrDefault();
                                    if (keyValue != null)
                                        position.Bread = Convert.ToBoolean(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "MealMenu").FirstOrDefault();
                                    if (keyValue != null)
                                        position.Menu = Convert.ToString(keyValue.Value);

                                    #endregion

                                    #region Attraction
                                    keyValue = KeyValueList.Where(a => a.Key == "TicketLocation").FirstOrDefault();
                                    if (keyValue != null)
                                        position.TicketLocation = Convert.ToString(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "TrainNumber").FirstOrDefault();
                                    if (keyValue != null)
                                        position.TrainNumber = Convert.ToString(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "GuidePurchaseTicket").FirstOrDefault();
                                    if (keyValue != null)
                                        position.GuidePurchaseTicket = Convert.ToBoolean(keyValue.Value);

                                    #endregion

                                    #region Guide
                                    keyValue = KeyValueList.Where(a => a.Key == "GuideName").FirstOrDefault();
                                    if (keyValue != null)
                                        position.DriverName = Convert.ToString(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "GuideContact").FirstOrDefault();
                                    if (keyValue != null)
                                        position.DriverContactNumber = Convert.ToString(keyValue.Value);

                                    keyValue = KeyValueList.Where(a => a.Key == "GuideTickets").FirstOrDefault();
                                    if (keyValue != null)
                                        position.TicketsIncluded = Convert.ToBoolean(keyValue.Value);

                                    #endregion

                                    #region Flight
                                    keyValue = KeyValueList.Where(a => a.Key == "FlightNumber").FirstOrDefault();
                                    if (keyValue != null)
                                        position.FlightNumber = Convert.ToString(keyValue.Value);
                                    #endregion

                                    //keyValue = KeyValueList.Where(a => a.Key == "TableRoomsAndRates").FirstOrDefault();
                                    //if (keyValue != null)
                                    //{
                                    //    var data = System.Web.HttpUtility.UrlDecode(Convert.ToString(keyValue.Value));
                                    //    var dict = System.Web.HttpUtility.ParseQueryString(data);
                                    //    var json = Newtonsoft.Json.JsonConvert.SerializeObject(dict.AllKeys.ToDictionary(k => k, k => dict[k]));
                                    //    Positions BookingRoomsAndPrices = Newtonsoft.Json.JsonConvert.DeserializeObject<Positions>(json);
                                    //}
                                }
                                #endregion

                                #region Change Supplier Placeholder
                                keyValue = KeyValueList.Where(a => a.Key == "TableChangeSuppPlace").FirstOrDefault();
                                if (keyValue != null)
                                {
                                    keyValue = KeyValueList.Where(a => a.Key == "ProductId").FirstOrDefault();
                                    if (keyValue != null && !string.IsNullOrEmpty(Convert.ToString(keyValue.Value)))
                                    {
                                        var Product = _MongoContext.Products.AsQueryable().Where(a => a.VoyagerProduct_Id == Convert.ToString(keyValue.Value)).FirstOrDefault();
                                        if (Product != null)
                                        {
                                            position.Product_Id = Product.VoyagerProduct_Id;
                                            position.ProductCode = Product.ProductCode;
                                            position.Product_Name = Product.ProductName;
                                            position.Country_Id = Product.CountryId;
                                            position.Country = Product.CountryName;
                                            position.City_Id = Product.CityId;
                                            position.City = Product.CityName;
                                            position.GRIDINFO = string.Format("{0} {1} {2} - {3} {4}", position.OrderNr, position.ProductType, position.STARTDATE?.ToString("dd/MM/yy"), position.ENDDATE?.ToString("dd/MM/yy"), Product.ProductName);

                                            #region SupplierInfo
                                            keyValue = KeyValueList.Where(a => a.Key == "SupplierId").FirstOrDefault();
                                            if (keyValue != null && !string.IsNullOrEmpty(Convert.ToString(keyValue.Value)))
                                            {
                                                var Supplier = _MongoContext.mCompanies.AsQueryable().Where(a => a.Company_Id == Convert.ToString(keyValue.Value)).FirstOrDefault();
                                                if (Supplier != null)
                                                {
                                                    position.BookingSeason.ForEach(a => a.Supplier_Id = Supplier.Company_Id);

                                                    position.SupplierInfo.Id = Supplier.Company_Id;
                                                    position.SupplierInfo.Name = Supplier.Name;
                                                    position.SupplierInfo.Code = Supplier.CompanyCode;
                                                    position.SupplierInfo.ISSUBAGENT = null;
                                                    position.SupplierInfo.ParentCompany_Id = null;
                                                    position.SupplierInfo.ParentCompany_Name = null;
                                                    position.SupplierInfo.Division_ID = null;
                                                    position.SupplierInfo.Division_Name = null;

                                                    var ProdContact = Supplier.Products.Where(a => a.Product_Id == Product.VoyagerProduct_Id).FirstOrDefault();
                                                    if (ProdContact?.Contact_Group_Id != null)
                                                    {
                                                        position.SupplierInfo.Contact_Id = ProdContact.Contact_Group_Id;
                                                        position.SupplierInfo.Contact_Name = ProdContact.Contact_Group_Name;
                                                        position.SupplierInfo.Contact_Email = ProdContact.Contact_Group_Email;
                                                        position.SupplierInfo.Contact_SendType = ProdContact.ContactVia;
                                                    }
                                                    else
                                                    {
                                                        var Contact = Supplier.ContactDetails.Where(a => a.IsOperationDefault).FirstOrDefault();
                                                        if (Contact != null)
                                                        {
                                                            position.SupplierInfo.Contact_Id = Contact.Contact_Id;
                                                            position.SupplierInfo.Contact_Name = Contact.FIRSTNAME + " " + Contact.LastNAME;
                                                            position.SupplierInfo.Contact_Email = Contact.MAIL;
                                                            position.SupplierInfo.Contact_Tel = Contact.TEL;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    position.BookingSeason.ForEach(a => a.Supplier_Id = "");
                                                    position.SupplierInfo = new BookingCompanyInfo();
                                                }
                                            }
                                            #endregion

                                            #region BookingRoomsAndPrices
                                            #region ProductTemplate
                                            var productTemplatesList = _masterRepository.GetProductTemplates(new ProductTemplatesGetReq());
                                            #endregion

                                            position.BookingRoomsAndPrices.ForEach(a => a.Status = "X");
                                            keyValue = KeyValueList.Where(a => a.Key == "ProductCategoryId").FirstOrDefault();
                                            if (keyValue != null && !string.IsNullOrEmpty(Convert.ToString(keyValue.Value)))
                                            {
                                                var Category = Product.ProductCategories.Where(a => a.ProductCategory_Id == Convert.ToString(keyValue.Value)).FirstOrDefault();
                                                if (Category != null)
                                                {
                                                    var ProductContractsBuy = _productRepository.GetProductContracts(new ProductContractsGetReq
                                                    {
                                                        SupplierId = position.SupplierInfo.Id,
                                                        ProductId = position.Product_Id,
                                                        BuySellType = "Buy",
                                                        AgentId = booking.AgentInfo?.Id
                                                    })?.Result?.ProductContract;
                                                    var ProductContractsSell = _productRepository.GetProductContracts(new ProductContractsGetReq
                                                    {
                                                        SupplierId = position.SupplierInfo.Id,
                                                        ProductId = position.Product_Id,
                                                        BuySellType = "Sell",
                                                        AgentId = booking.AgentInfo?.Id
                                                    })?.Result?.ProductContract;

                                                    if (ProductContractsBuy == null)
                                                    {
                                                        ProductContractsBuy = _productRepository.GetProductContracts(new ProductContractsGetReq
                                                        {
                                                            SupplierId = position.SupplierInfo.Id,
                                                            ProductId = position.Product_Id,
                                                            BuySellType = "Buy"
                                                        })?.Result?.ProductContract;
                                                    }
                                                    if (ProductContractsSell == null)
                                                    {
                                                        ProductContractsSell = _productRepository.GetProductContracts(new ProductContractsGetReq
                                                        {
                                                            SupplierId = position.SupplierInfo.Id,
                                                            ProductId = position.Product_Id,
                                                            BuySellType = "Sell"
                                                        })?.Result?.ProductContract;
                                                    }

                                                    if (position.ProductType?.ToLower() == "hotel" || position.ProductType?.ToLower() == "apartments" || position.ProductType?.ToLower() == "overnight ferry")
                                                    {
                                                        var BookingRoomList = position.BookingRoomsAndPrices.Where(a => a.PersonType == "ADULT").ToList();
                                                        var BookingRoomListNew = new List<BookingRoomsAndPrices>();
                                                        foreach (var BookingRoom in BookingRoomList)
                                                        {
                                                            var Range = Category.ProductRanges.Where(a => a.ProductTemplateCode == BookingRoom.RoomShortCode && a.PersonType == BookingRoom.PersonType).FirstOrDefault();

                                                            if (Range == null)
                                                            {
                                                                var productTemplateList = productTemplatesList?.Where(a => a.ParentSubProd == BookingRoom.RoomShortCode).ToList();
                                                                if (productTemplateList?.Count > 0)
                                                                    Range = Category.ProductRanges.Where(a => productTemplateList.Any(b => b.VoyagerProductTemplate_Id == a.ProductTemplate_Id)).FirstOrDefault();
                                                            }

                                                            var BuyContractPrices = ProductContractsBuy?.PricePeriods?.Where(a => a.Period_Start_Date <= position.STARTDATE && a.Period_End_Date >= position.STARTDATE)
                                                               .OrderBy(a => a.RateType_Name == "Special" ? "A" : a.RateType_Name == "Exception " ? "B" : a.RateType_Name == "Normal" ? "C" : "D").ThenBy(a => a.RateType_Name)
                                                               .FirstOrDefault()?.Prices.Where(a => a.ProductRange_ID == Range?.ProductRange_Id).FirstOrDefault();

                                                            var SellContractPrices = ProductContractsSell?.PricePeriods?.Where(a => a.Period_Start_Date <= position.STARTDATE && a.Period_End_Date >= position.STARTDATE)
                                                            .OrderBy(a => a.RateType_Name == "Special" ? "A" : a.RateType_Name == "Exception " ? "B" : a.RateType_Name == "Normal" ? "C" : "D").ThenBy(a => a.RateType_Name)
                                                            .FirstOrDefault()?.Prices.Where(a => a.ProductRange_ID == Range?.ProductRange_Id).FirstOrDefault();

                                                            int Occupancy = Range?.ProductTemplateName == "DOUBLE" || Range?.ProductTemplateName == "TWIN" ? 2 :
                                                                (Range?.ProductTemplateName == "TRIPLE") ? 3 : (Range?.ProductTemplateName == "QUAD") ? 4 : 1;

                                                            position.BookingRoomsAndPrices.Add(new BookingRoomsAndPrices
                                                            {
                                                                BookingRooms_Id = Guid.NewGuid().ToString(),
                                                                PositionPricing_Id = Guid.NewGuid().ToString(),

                                                                ApplyMarkup = BookingRoom.ApplyMarkup,
                                                                BudgetPrice = BookingRoom.BudgetPrice,
                                                                BuyCurrency_Id = BookingRoom.BuyCurrency_Id,
                                                                BuyCurrency_Name = BookingRoom.BuyCurrency_Name,
                                                                BuyPrice = BookingRoom.BuyPrice,
                                                                ChargeBasis = BookingRoom.ChargeBasis,
                                                                ConfirmedReqPrice = BookingRoom.ConfirmedReqPrice,
                                                                EndDate = BookingRoom.EndDate,
                                                                ExcludeFromInvoice = BookingRoom.ExcludeFromInvoice,
                                                                MealPlan = BookingRoom.MealPlan,
                                                                MealPlan_Id = BookingRoom.MealPlan_Id,
                                                                PersonType = BookingRoom.PersonType,
                                                                PersonType_Id = BookingRoom.PersonType_Id,
                                                                RequestedPrice = BookingRoom.RequestedPrice,
                                                                Req_Count = BookingRoom.Req_Count,
                                                                RoomsAndPricesAllocation = BookingRoom.RoomsAndPricesAllocation,
                                                                StartDate = BookingRoom.StartDate,
                                                                STATUS_DT = BookingRoom.STATUS_DT,
                                                                STATUS_US = BookingRoom.STATUS_US,
                                                                CREA_DT = DateTime.Now,
                                                                CREA_US = request.UserEmailId,

                                                                Booking_Id = booking.Booking_Id,
                                                                Position_Id = position.Position_Id,
                                                                Category_Id = Category.ProductCategory_Id,
                                                                CategoryName = Category.ProductCategoryName,
                                                                ProductRange_Id = Range.ProductRange_Id,
                                                                RoomName = Range.ProductTemplateName,
                                                                ProductTemplate_Id = Range.ProductTemplate_Id,
                                                                RoomShortCode = Range.ProductTemplateCode,
                                                                Capacity = Range.Quantity,
                                                                AllocationUsed = null,
                                                                Allocation_Id = null,
                                                                Age = null,
                                                                CrossBookingPax_Id = null,
                                                                CrossPosition_Id = null,
                                                                IsRecursive = true,
                                                                OneOffDate = null,
                                                                ParentBookingRooms_Id = null,

                                                                Action = ProductContractsBuy == null ? "R" : "N",
                                                                BuyContract_Id = ProductContractsBuy?.BuyContract_Id,
                                                                BuyPositionPrice_Id = BuyContractPrices?.ProductPrice_ID,
                                                                SellContract_Id = ProductContractsSell?.BuyContract_Id,
                                                                SellPositionPrice_Id = SellContractPrices?.ProductPrice_ID,
                                                                SellContractCurrency_Id = ProductContractsBuy?.Contract_Currency_Id,
                                                                SellContractCurrency_Name = ProductContractsBuy?.Contract_Currency,
                                                                ContractedSellPrice = SellContractPrices?.Contract_Price,
                                                                ContractedBuyPrice = BuyContractPrices?.Contract_Price,
                                                                BookingSeason_Id = position.BookingSeason.FirstOrDefault()?.BookingSeason_ID,
                                                                InvForPax = position.PositionType == "Core" ? null : "N",
                                                                InvNumber = position.PositionType == "Core" ? null : (BookingRoom.ChargeBasis != "PRPN" ? BookingRoom.Req_Count : (BookingRoom.Req_Count * Occupancy)),

                                                                AuditTrail = new AuditTrail
                                                                {
                                                                    CREA_DT = DateTime.Now,
                                                                    CREA_US = request.UserEmailId
                                                                },

                                                                IsAdditionalYN = Convert.ToBoolean(Range.AdditionalYn),
                                                                Status = null,
                                                            });
                                                            //position.BookingRoomsAndPrices.Add(BookingRoom);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        keyValue = KeyValueList.Where(a => a.Key == "ProductRangeId").FirstOrDefault();
                                                        if (keyValue != null && !string.IsNullOrEmpty(Convert.ToString(keyValue.Value)))
                                                        {
                                                            var Range = Category.ProductRanges.Where(a => a.ProductRange_Id == Convert.ToString(keyValue.Value)).FirstOrDefault();
                                                            if (Range != null)
                                                            {
                                                                var BuyContractPrices = ProductContractsBuy?.PricePeriods?.Where(a => a.Period_Start_Date <= position.STARTDATE && a.Period_End_Date >= position.STARTDATE)
                                                                .OrderBy(a => a.RateType_Name == "Special" ? "A" : a.RateType_Name == "Exception " ? "B" : a.RateType_Name == "Normal" ? "C" : "D").ThenBy(a => a.RateType_Name)
                                                                .FirstOrDefault()?.Prices.Where(a => a.ProductRange_ID == Range.ProductRange_Id).FirstOrDefault();

                                                                var SellContractPrices = ProductContractsSell?.PricePeriods?.Where(a => a.Period_Start_Date <= position.STARTDATE && a.Period_End_Date >= position.STARTDATE)
                                                                .OrderBy(a => a.RateType_Name == "Special" ? "A" : a.RateType_Name == "Exception " ? "B" : a.RateType_Name == "Normal" ? "C" : "D").ThenBy(a => a.RateType_Name)
                                                                .FirstOrDefault()?.Prices.Where(a => a.ProductRange_ID == Range.ProductRange_Id).FirstOrDefault();

                                                                int Occupancy = Range?.ProductTemplateName == "DOUBLE" || Range?.ProductTemplateName == "TWIN" ? 2 :
                                                                    (Range?.ProductTemplateName == "TRIPLE") ? 3 : (Range?.ProductTemplateName == "QUAD") ? 4 : 1;

                                                                var BookingRoom = position.BookingRoomsAndPrices.Where(a => a.RoomShortCode == Range.ProductTemplateCode && a.PersonType == Range.PersonType).FirstOrDefault();

                                                                if (BookingRoom == null)
                                                                {
                                                                    var productTemplate = productTemplatesList.Where(a => a.VoyagerProductTemplate_Id == Range.ProductTemplate_Id).FirstOrDefault();
                                                                    if (productTemplate != null)
                                                                    {
                                                                        BookingRoom = position.BookingRoomsAndPrices.Where(a => a.RoomShortCode == productTemplate.ParentSubProd && a.PersonType == Range.PersonType).FirstOrDefault();
                                                                    }
                                                                }

                                                                if (BookingRoom != null)
                                                                {
                                                                    position.BookingRoomsAndPrices.Add(new BookingRoomsAndPrices
                                                                    {
                                                                        BookingRooms_Id = Guid.NewGuid().ToString(),
                                                                        PositionPricing_Id = Guid.NewGuid().ToString(),

                                                                        ApplyMarkup = BookingRoom.ApplyMarkup,
                                                                        BudgetPrice = BookingRoom.BudgetPrice,
                                                                        BuyCurrency_Id = BookingRoom.BuyCurrency_Id,
                                                                        BuyCurrency_Name = BookingRoom.BuyCurrency_Name,
                                                                        BuyPrice = BookingRoom.BuyPrice,
                                                                        ChargeBasis = BookingRoom.ChargeBasis,
                                                                        ConfirmedReqPrice = BookingRoom.ConfirmedReqPrice,
                                                                        EndDate = BookingRoom.EndDate,
                                                                        ExcludeFromInvoice = BookingRoom.ExcludeFromInvoice,
                                                                        MealPlan = BookingRoom.MealPlan,
                                                                        MealPlan_Id = BookingRoom.MealPlan_Id,
                                                                        PersonType = BookingRoom.PersonType,
                                                                        PersonType_Id = BookingRoom.PersonType_Id,
                                                                        RequestedPrice = BookingRoom.RequestedPrice,
                                                                        Req_Count = BookingRoom.Req_Count,
                                                                        RoomsAndPricesAllocation = BookingRoom.RoomsAndPricesAllocation,
                                                                        StartDate = BookingRoom.StartDate,
                                                                        STATUS_DT = BookingRoom.STATUS_DT,
                                                                        STATUS_US = BookingRoom.STATUS_US,
                                                                        CREA_DT = DateTime.Now,
                                                                        CREA_US = request.UserEmailId,

                                                                        Booking_Id = booking.Booking_Id,
                                                                        Position_Id = position.Position_Id,
                                                                        Category_Id = Category.ProductCategory_Id,
                                                                        CategoryName = Category.ProductCategoryName,
                                                                        ProductRange_Id = Range.ProductRange_Id,
                                                                        RoomName = Range.ProductTemplateName,
                                                                        ProductTemplate_Id = Range.ProductTemplate_Id,
                                                                        RoomShortCode = Range.ProductTemplateCode,
                                                                        Capacity = Range.Quantity,
                                                                        AllocationUsed = null,
                                                                        Allocation_Id = null,
                                                                        Age = null,
                                                                        CrossBookingPax_Id = null,
                                                                        CrossPosition_Id = null,
                                                                        IsRecursive = true,
                                                                        OneOffDate = null,
                                                                        ParentBookingRooms_Id = null,

                                                                        Action = ProductContractsBuy == null ? "R" : "N",
                                                                        BuyContract_Id = ProductContractsBuy?.BuyContract_Id,
                                                                        BuyPositionPrice_Id = BuyContractPrices?.ProductPrice_ID,
                                                                        SellContract_Id = ProductContractsSell?.BuyContract_Id,
                                                                        SellPositionPrice_Id = SellContractPrices?.ProductPrice_ID,
                                                                        SellContractCurrency_Id = ProductContractsBuy?.Contract_Currency_Id,
                                                                        SellContractCurrency_Name = ProductContractsBuy?.Contract_Currency,
                                                                        ContractedSellPrice = SellContractPrices?.Contract_Price,
                                                                        ContractedBuyPrice = BuyContractPrices?.Contract_Price,
                                                                        BookingSeason_Id = position.BookingSeason?.FirstOrDefault()?.BookingSeason_ID,
                                                                        InvForPax = position.PositionType == "Core" ? null : "N",
                                                                        InvNumber = position.PositionType == "Core" ? null : (BookingRoom.ChargeBasis != "PRPN" ? BookingRoom.Req_Count : (BookingRoom.Req_Count * Occupancy)),

                                                                        AuditTrail = new AuditTrail
                                                                        {
                                                                            CREA_DT = DateTime.Now,
                                                                            CREA_US = request.UserEmailId
                                                                        },

                                                                        IsAdditionalYN = Convert.ToBoolean(Range.AdditionalYn),
                                                                        Status = null
                                                                    });
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            #endregion
                                        }
                                        else
                                        {
                                            response.Status = "Failure";
                                            response.ErrorMessage.Add("UpdatePositionKeyDetails:-Bookings->Position->Product details not found.");
                                        }
                                    }
                                    else
                                    {
                                        response.Status = "Failure";
                                        response.ErrorMessage.Add("UpdatePositionKeyDetails:-Bookings->Position->ProductId not found.");
                                    }
                                }
                                #endregion

                                #region Change Supplier Real
                                keyValue = KeyValueList.Where(a => a.Key == "TableChangeSuppReal").FirstOrDefault();
                                if (keyValue != null)
                                {
                                    #region SupplierInfo
                                    keyValue = KeyValueList.Where(a => a.Key == "SupplierId").FirstOrDefault();
                                    if (keyValue != null && !string.IsNullOrEmpty(Convert.ToString(keyValue.Value)))
                                    {
                                        var Supplier = _MongoContext.mCompanies.AsQueryable().Where(a => a.Company_Id == Convert.ToString(keyValue.Value)).FirstOrDefault();
                                        var ProdSupplier = _MongoContext.Products.AsQueryable().Where(a => a.VoyagerProduct_Id == position.Product_Id).FirstOrDefault()?.ProductSuppliers?.Where(b => b.Company_Id == Convert.ToString(keyValue.Value)).FirstOrDefault();

                                        if (ProdSupplier != null)
                                        {
                                            position.BookingSeason.ForEach(a => a.Supplier_Id = ProdSupplier.Company_Id);

                                            position.SupplierInfo.Id = ProdSupplier.Company_Id;
                                            position.SupplierInfo.Name = ProdSupplier.CompanyName;
                                            position.SupplierInfo.Code = Supplier?.CompanyCode;
                                            position.SupplierInfo.ISSUBAGENT = null;
                                            position.SupplierInfo.ParentCompany_Id = null;
                                            position.SupplierInfo.ParentCompany_Name = null;
                                            position.SupplierInfo.Division_ID = null;
                                            position.SupplierInfo.Division_Name = null;

                                            var ProdContact = Supplier?.Products.Where(a => a.Product_Id == position.Product_Id).FirstOrDefault();
                                            if (ProdContact?.Contact_Group_Id != null)
                                            {
                                                position.SupplierInfo.Contact_Id = ProdContact.Contact_Group_Id;
                                                position.SupplierInfo.Contact_Name = ProdContact.Contact_Group_Name;
                                                position.SupplierInfo.Contact_Email = ProdContact.Contact_Group_Email;
                                                position.SupplierInfo.Contact_SendType = ProdContact.ContactVia;
                                            }
                                            else
                                            {
                                                var Contact = Supplier?.ContactDetails.Where(a => a.IsOperationDefault).FirstOrDefault();
                                                if (Contact != null)
                                                {
                                                    position.SupplierInfo.Contact_Id = Contact.Contact_Id;
                                                    position.SupplierInfo.Contact_Name = Contact.FIRSTNAME + " " + Contact.LastNAME;
                                                    position.SupplierInfo.Contact_Email = Contact.MAIL;
                                                    position.SupplierInfo.Contact_Tel = Contact.TEL;
                                                }
                                            }

                                            #region Contract Price
                                            var ProductContractsBuy = _productRepository.GetProductContracts(new ProductContractsGetReq
                                            {
                                                SupplierId = position.SupplierInfo.Id,
                                                ProductId = position.Product_Id,
                                                BuySellType = "Buy",
                                                AgentId = booking.AgentInfo?.Id
                                            })?.Result?.ProductContract;
                                            var ProductContractsSell = _productRepository.GetProductContracts(new ProductContractsGetReq
                                            {
                                                SupplierId = position.SupplierInfo.Id,
                                                ProductId = position.Product_Id,
                                                BuySellType = "Sell",
                                                AgentId = booking.AgentInfo?.Id
                                            })?.Result?.ProductContract;

                                            if (ProductContractsBuy == null)
                                            {
                                                ProductContractsBuy = _productRepository.GetProductContracts(new ProductContractsGetReq
                                                {
                                                    SupplierId = position.SupplierInfo.Id,
                                                    ProductId = position.Product_Id,
                                                    BuySellType = "Buy"
                                                })?.Result?.ProductContract;
                                            }
                                            if (ProductContractsSell == null)
                                            {
                                                ProductContractsSell = _productRepository.GetProductContracts(new ProductContractsGetReq
                                                {
                                                    SupplierId = position.SupplierInfo.Id,
                                                    ProductId = position.Product_Id,
                                                    BuySellType = "Sell"
                                                })?.Result?.ProductContract;
                                            }

                                            foreach (var BookingRoom in position.BookingRoomsAndPrices)
                                            {
                                                var BuyContractPrices = ProductContractsBuy?.PricePeriods?.Where(a => a.Period_Start_Date <= position.STARTDATE && a.Period_End_Date >= position.STARTDATE)
                                                               .OrderBy(a => a.RateType_Name == "Special" ? "A" : a.RateType_Name == "Exception " ? "B" : a.RateType_Name == "Normal" ? "C" : "D").ThenBy(a => a.RateType_Name)
                                                               .FirstOrDefault()?.Prices.Where(a => a.ProductRange_ID == BookingRoom.ProductRange_Id).FirstOrDefault();

                                                var SellContractPrices = ProductContractsSell?.PricePeriods?.Where(a => a.Period_Start_Date <= position.STARTDATE && a.Period_End_Date >= position.STARTDATE)
                                                .OrderBy(a => a.RateType_Name == "Special" ? "A" : a.RateType_Name == "Exception " ? "B" : a.RateType_Name == "Normal" ? "C" : "D").ThenBy(a => a.RateType_Name)
                                                .FirstOrDefault()?.Prices.Where(a => a.ProductRange_ID == BookingRoom.ProductRange_Id).FirstOrDefault();

                                                BookingRoom.Action = ProductContractsBuy == null ? "R" : "N";
                                                BookingRoom.BuyContract_Id = ProductContractsBuy?.BuyContract_Id;
                                                BookingRoom.SellContract_Id = ProductContractsSell?.BuyContract_Id;
                                                BookingRoom.BuyCurrency_Id = ProductContractsBuy?.Contract_Currency_Id == null ? ProdSupplier?.CurrencyId : ProductContractsBuy.Contract_Currency_Id;
                                                BookingRoom.BuyCurrency_Name = ProductContractsBuy?.Contract_Currency == null ? ProdSupplier?.CurrencyName : ProductContractsBuy.Contract_Currency;

                                                BookingRoom.BuyPositionPrice_Id = BuyContractPrices?.ProductPrice_ID;
                                                BookingRoom.SellPositionPrice_Id = SellContractPrices?.ProductPrice_ID;
                                                BookingRoom.SellContractCurrency_Id = ProductContractsBuy?.Contract_Currency_Id;
                                                BookingRoom.SellContractCurrency_Name = ProductContractsBuy?.Contract_Currency;
                                                BookingRoom.ContractedSellPrice = SellContractPrices?.Contract_Price;
                                                BookingRoom.ContractedBuyPrice = BuyContractPrices?.Contract_Price == null ? BookingRoom.ContractedBuyPrice : BuyContractPrices?.Contract_Price;
                                                BookingRoom.BuyPrice = BuyContractPrices?.Contract_Price == null ? BookingRoom.BuyPrice : BuyContractPrices?.Contract_Price;
                                            }
                                            #endregion
                                        }
                                    }
                                    #endregion
                                }
                                #endregion

                                #region Confirm Supplier

                                keyValue = KeyValueList.Where(a => a.Key == "SupplierConfirmation").FirstOrDefault();
                                if (keyValue != null)
                                    position.Supplier_Confirmation = Convert.ToString(keyValue.Value);

                                #endregion

                                #region Position AuditTrail
                                position.AuditTrail.MODI_US = request.UserEmailId;
                                position.AuditTrail.MODI_DT = DateTime.Now;
                                if (auditStatusPosFlag)
                                {
                                    position.AuditTrail.STATUS_US = request.UserEmailId;
                                    position.AuditTrail.STATUS_DT = DateTime.Now;
                                }
                                #endregion
                            }

                            #region Booking->AuditTrail
                            if (auditBookingFlag)
                            {
                                booking.AuditTrail.MODI_DT = DateTime.Now;
                                booking.AuditTrail.MODI_US = request.UserEmailId;
                            }
                            #endregion
                        }
                        else
                        {
                            response.Status = "Failure";
                            response.ErrorMessage.Add("UpdatePositionKeyDetails:-Bookings->Position details not found.");
                        }
                    }
                    else
                    {
                        response.Status = "Failure";
                        response.ErrorMessage.Add("UpdatePositionKeyDetails:-PositionIds can not be null.");
                    }
                }
                else
                {
                    response.Status = "Failure";
                    response.ErrorMessage.Add("UpdatePositionKeyDetails:-Bookings/OpsKeyValue can not be null.");
                }
            }
            catch (Exception ex)
            {
                response.Status = "Failure";
                response.ErrorMessage.Add("UpdatePositionKeyDetails:- " + ex.Message);
            }

            return response;
        }

        public OPSWorkflowResponseStatus UpdateBookingRoomsAndPrices(ref Bookings booking, ExecuteWorkflowActionReq request)
        {
            OPSWorkflowResponseStatus response = new OPSWorkflowResponseStatus() { Status = "Success" };
            try
            {
                if (booking != null && request.OpsKeyValue?.Count > 0)
                {
                    if (request.PositionIds?.Count > 0)
                    {
                        string ChargeBasis, CategoryName;
                        ProductCategory productCategory;
                        ProductRange productRange;
                        var KeyValueList = request.OpsKeyValue;
                        var keyValue = new OpsKeyValue();
                        BookingRoomsAndPrices PosPrice;
                        BudgetSupplements PosBudgSupp;
                        List<OpsPositionRoomPrice> PosRoomsAndRates;
                        List<OpsBudgetSupplements> PosBudgetSupp;
                        List<RoomsAndPricesAllocation> defAllocation;
                        ContractPrices BuyContractPrices;
                        ContractPrices SellContractPrices;
                        int Occupancy;
                        var positionList = booking.Positions.Where(x => request.PositionIds.Contains(x.Position_Id)).ToList();
                        string AgentId = booking?.AgentInfo?.Id;

                        if (positionList?.Count > 0)
                        {
                            foreach (var position in positionList)
                            {
                                var Product = _MongoContext.Products.AsQueryable().Where(a => a.VoyagerProduct_Id == position.Product_Id).FirstOrDefault();
                                var DefMealPlan = _MongoContext.mDefMealPlan.AsQueryable().Where(a => a.MealPlan == position.HOTELMEALPLAN).FirstOrDefault();
                                var ProductContractsBuy = _productRepository.GetProductContracts(new ProductContractsGetReq
                                {
                                    SupplierId = position?.SupplierInfo?.Id,
                                    ProductId = position.Product_Id,
                                    BuySellType = "Buy",
                                    AgentId = AgentId
                                })?.Result?.ProductContract;
                                var ProductContractsSell = _productRepository.GetProductContracts(new ProductContractsGetReq
                                {
                                    SupplierId = position?.SupplierInfo?.Id,
                                    ProductId = position.Product_Id,
                                    BuySellType = "Sell",
                                    AgentId = AgentId
                                })?.Result?.ProductContract;
                                //var ProductContractsBuy = _MongoContext.ProductContracts.AsQueryable().Where(a => a.Supplier_Id == position.SupplierInfo.Id
                                //    && a.Product_Id == position.Product_Id && a.BuySellType == "Buy" && a.ForAgent_Id == AgentId).FirstOrDefault();
                                //var ProductContractsSell = _MongoContext.ProductContracts.AsQueryable().Where(a => a.Supplier_Id == position.SupplierInfo.Id
                                //    && a.Product_Id == position.Product_Id && a.BuySellType == "Sell" && a.ForAgent_Id == AgentId).FirstOrDefault();

                                if (ProductContractsBuy == null)
                                {
                                    ProductContractsBuy = _productRepository.GetProductContracts(new ProductContractsGetReq
                                    {
                                        SupplierId = position?.SupplierInfo?.Id,
                                        ProductId = position.Product_Id,
                                        BuySellType = "Buy"
                                    })?.Result?.ProductContract;
                                }
                                if (ProductContractsSell == null)
                                {
                                    ProductContractsSell = _productRepository.GetProductContracts(new ProductContractsGetReq
                                    {
                                        SupplierId = position?.SupplierInfo?.Id,
                                        ProductId = position.Product_Id,
                                        BuySellType = "Sell"
                                    })?.Result?.ProductContract;
                                }

                                #region Save Position
                                keyValue = KeyValueList.Where(a => a.Key == "saveposition").FirstOrDefault();
                                if (keyValue != null)
                                {
                                    #region Rooms and prices main section
                                    ChargeBasis = Convert.ToString(request.OpsKeyValue.Where(a => a.Key == "ChargeBasis").FirstOrDefault()?.Value);
                                    keyValue = request.OpsKeyValue.Where(a => a.Key == "TableRoomsAndRates").FirstOrDefault();
                                    if (keyValue != null)
                                    {
                                        PosRoomsAndRates = JsonConvert.DeserializeObject<List<OpsPositionRoomPrice>>(keyValue.Value.ToString());

                                        foreach (var RoomRate in PosRoomsAndRates)
                                        {
                                            if (RoomRate.ProductRange != null && RoomRate.ProductRange.Contains('(') && RoomRate.ProductRange.Contains(')'))
                                            {
                                                CategoryName = RoomRate.ProductRange.Substring((RoomRate.ProductRange.IndexOf("(") + 1), (RoomRate.ProductRange.IndexOf(")") - 1));
                                            }
                                            else
                                            {
                                                CategoryName = RoomRate.CategoryName;
                                            }
                                            productCategory = Product?.ProductCategories?.Where(a => a.ProductCategoryName == CategoryName)?.FirstOrDefault();
                                            productRange = productCategory?.ProductRanges?.Where(a => a.ProductRange_Id == RoomRate.ProductRangeID)?.FirstOrDefault();
                                            PosPrice = position.BookingRoomsAndPrices.Where(a => a.BookingRooms_Id == RoomRate.BookingRooms_Id && a.PositionPricing_Id == RoomRate.PositionPricing_Id)?.FirstOrDefault();

                                            if (PosPrice == null)
                                            {
                                                BuyContractPrices = ProductContractsBuy?.PricePeriods.Where(a => a.Period_Start_Date <= position.STARTDATE && a.Period_End_Date >= position.STARTDATE)
                                                .OrderBy(a => a.RateType_Name == "Special" ? "A" : a.RateType_Name == "Exception " ? "B" : a.RateType_Name == "Normal" ? "C" : "D").ThenBy(a => a.RateType_Name)
                                                .FirstOrDefault()?.Prices.Where(a => a.ProductRange_ID == RoomRate.ProductRangeID).FirstOrDefault();

                                                SellContractPrices = ProductContractsSell?.PricePeriods.Where(a => a.Period_Start_Date <= position.STARTDATE && a.Period_End_Date >= position.STARTDATE)
                                                .OrderBy(a => a.RateType_Name == "Special" ? "A" : a.RateType_Name == "Exception " ? "B" : a.RateType_Name == "Normal" ? "C" : "D").ThenBy(a => a.RateType_Name)
                                                .FirstOrDefault()?.Prices.Where(a => a.ProductRange_ID == RoomRate.ProductRangeID).FirstOrDefault();

                                                Occupancy = productRange?.ProductTemplateName == "DOUBLE" || productRange?.ProductTemplateName == "TWIN" ? 2 :
                                                    (productRange?.ProductTemplateName == "TRIPLE") ? 3 : (productRange?.ProductTemplateName == "QUAD") ? 4 : 1;

                                                PosPrice = new BookingRoomsAndPrices
                                                {
                                                    BookingRooms_Id = RoomRate.BookingRooms_Id,
                                                    PositionPricing_Id = RoomRate.PositionPricing_Id,
                                                    Booking_Id = booking.Booking_Id,
                                                    Position_Id = position.Position_Id,
                                                    //ChargeBasis = ChargeBasis,
                                                    IsRecursive = true,
                                                    MealPlan_Id = DefMealPlan?.MealPlan_Id,
                                                    MealPlan = DefMealPlan?.MealPlan,
                                                    StartDate = position.STARTDATE,
                                                    EndDate = position.ENDDATE,
                                                    BuyCurrency_Id = position.BuyCurrency_Id,
                                                    BuyCurrency_Name = RoomRate.BuyCurrency_Name != null ? RoomRate.BuyCurrency_Name : position.BuyCurrency_Name,
                                                    Action = ProductContractsBuy == null ? "R" : "N",
                                                    BuyContract_Id = ProductContractsBuy?.BuyContract_Id,
                                                    BuyPositionPrice_Id = BuyContractPrices?.ProductPrice_ID,
                                                    SellContract_Id = ProductContractsSell?.BuyContract_Id,
                                                    SellPositionPrice_Id = SellContractPrices?.ProductPrice_ID,
                                                    SellContractCurrency_Id = ProductContractsBuy?.Contract_Currency_Id,
                                                    SellContractCurrency_Name = ProductContractsBuy?.Contract_Currency,
                                                    ContractedSellPrice = SellContractPrices?.Contract_Price,
                                                    ContractedBuyPrice = BuyContractPrices?.Contract_Price,
                                                    BookingSeason_Id = position.BookingSeason.FirstOrDefault()?.BookingSeason_ID,
                                                    InvForPax = position.PositionType == "Core" ? null : "N",
                                                    InvNumber = position.PositionType == "Core" ? null : (ChargeBasis != "PRPN" ? RoomRate.Req_Count : (RoomRate.Req_Count * Occupancy)),

                                                    AuditTrail = new AuditTrail
                                                    {
                                                        CREA_DT = DateTime.Now,
                                                        CREA_US = request.UserEmailId
                                                    }
                                                };
                                                defAllocation = position.BookingRoomsAndPrices.FirstOrDefault()?.RoomsAndPricesAllocation;
                                                PosPrice.RoomsAndPricesAllocation = defAllocation.Select(a => new RoomsAndPricesAllocation
                                                {
                                                    BookingRoomDetail_ID = Guid.NewGuid().ToString(),
                                                    AllocationDate = a.AllocationDate,
                                                    AuditTrail = new AuditTrail
                                                    {
                                                        CREA_DT = DateTime.Now,
                                                        CREA_US = request.UserEmailId,
                                                    }
                                                }).ToList();

                                                position.BookingRoomsAndPrices.Add(PosPrice);
                                            }

                                            PosPrice.RoomName = productRange?.ProductTemplateName;
                                            PosPrice.RoomShortCode = productRange?.ProductTemplateCode;
                                            PosPrice.CategoryName = CategoryName;
                                            PosPrice.Category_Id = productCategory?.DefProductCategory_Id;
                                            PosPrice.ProductTemplate_Id = productRange?.ProductTemplate_Id;
                                            PosPrice.RoomShortCode = productRange?.ProductTemplateCode;
                                            PosPrice.Capacity = productRange?.Quantity;
                                            PosPrice.ChargeBasis = String.IsNullOrEmpty(ChargeBasis) ? RoomRate.ChargeBasis : ChargeBasis;
                                            PosPrice.ProductRange_Id = RoomRate.ProductRangeID;
                                            PosPrice.PersonType = RoomRate.PersonType;
                                            PosPrice.PersonType_Id = RoomRate.PersonTypeID;
                                            PosPrice.Req_Count = RoomRate.Req_Count;
                                            PosPrice.BudgetPrice = RoomRate.BudgetPrice;
                                            PosPrice.RequestedPrice = RoomRate.RequestedPrice;
                                            PosPrice.BuyPrice = RoomRate.BuyPrice;
                                            PosPrice.ConfirmedReqPrice = RoomRate.ConfirmedReqPrice;
                                            PosPrice.ExcludeFromInvoice = RoomRate.ExcludeFromInvoice;
                                            PosPrice.ApplyMarkup = RoomRate.ApplyMarkup;

                                            if (RoomRate.IsAdditionalYN != null)
                                            {
                                                PosPrice.IsAdditionalYN = RoomRate.IsAdditionalYN.Value;
                                            }
                                            else
                                            {
                                                PosPrice.IsAdditionalYN = false;
                                            }
                                            PosPrice.Status = RoomRate.Status;
                                            PosPrice.Age = RoomRate?.Age;
                                            if (PosPrice.RoomsAndPricesAllocation != null)
                                            {
                                                PosPrice?.RoomsAndPricesAllocation?.ForEach(a =>
                                                {
                                                    a.OnReqQty = RoomRate.OnReqQty;
                                                    a.OnAllocQty = RoomRate.OnAllocQty;
                                                    a.OnFreeSellQty = RoomRate.OnFreeSellQty;
                                                    a.AuditTrail.MODI_DT = DateTime.Now;
                                                    a.AuditTrail.MODI_US = request.UserEmailId;
                                                });
                                            }

                                        }
                                    }
                                    #endregion

                                    #region Rooms and prices Additional Supplements
                                    keyValue = request.OpsKeyValue.Where(a => a.Key == "TableAdditionalSuppliments").FirstOrDefault();
                                    if (keyValue != null)
                                    {
                                        PosRoomsAndRates = JsonConvert.DeserializeObject<List<OpsPositionRoomPrice>>(keyValue.Value.ToString());

                                        foreach (var RoomRate in PosRoomsAndRates)
                                        {
                                            if (RoomRate.ProductRange != null && RoomRate.ProductRange.Contains('(') && RoomRate.ProductRange.Contains(')'))
                                            {
                                                CategoryName = RoomRate.ProductRange.Substring((RoomRate.ProductRange.IndexOf("(") + 1), (RoomRate.ProductRange.IndexOf(")") - 1));
                                            }
                                            else
                                            {
                                                CategoryName = RoomRate.CategoryName;
                                            }
                                            //CategoryName = CategoryName == null ? RoomRate.CategoryName : null;
                                            productCategory = Product?.ProductCategories?.Where(a => a.ProductCategoryName == CategoryName)?.FirstOrDefault();
                                            productRange = productCategory?.ProductRanges?.Where(a => a.ProductRange_Id == RoomRate.ProductRangeID)?.FirstOrDefault();
                                            PosPrice = position?.BookingRoomsAndPrices?.Where(a => a.BookingRooms_Id == RoomRate.BookingRooms_Id && a.PositionPricing_Id == RoomRate.PositionPricing_Id)?.FirstOrDefault();

                                            if (PosPrice == null)
                                            {
                                                BuyContractPrices = ProductContractsBuy?.PricePeriods.Where(a => a.Period_Start_Date <= position.STARTDATE && a.Period_End_Date >= position.STARTDATE)
                                                .OrderBy(a => a.RateType_Name == "Special" ? "A" : a.RateType_Name == "Exception " ? "B" : a.RateType_Name == "Normal" ? "C" : "D").ThenBy(a => a.RateType_Name)
                                                .FirstOrDefault()?.Prices.Where(a => a.ProductRange_ID == RoomRate.ProductRangeID).FirstOrDefault();

                                                SellContractPrices = ProductContractsSell?.PricePeriods.Where(a => a.Period_Start_Date <= position.STARTDATE && a.Period_End_Date >= position.STARTDATE)
                                                .OrderBy(a => a.RateType_Name == "Special" ? "A" : a.RateType_Name == "Exception " ? "B" : a.RateType_Name == "Normal" ? "C" : "D").ThenBy(a => a.RateType_Name)
                                                .FirstOrDefault()?.Prices.Where(a => a.ProductRange_ID == RoomRate.ProductRangeID).FirstOrDefault();

                                                Occupancy = productRange?.ProductTemplateName == "DOUBLE" || productRange?.ProductTemplateName == "TWIN" ? 2 :
                                                    (productRange?.ProductTemplateName == "TRIPLE") ? 3 : (productRange?.ProductTemplateName == "QUAD") ? 4 : 1;

                                                PosPrice = new BookingRoomsAndPrices
                                                {
                                                    BookingRooms_Id = RoomRate.BookingRooms_Id,
                                                    PositionPricing_Id = RoomRate.PositionPricing_Id,
                                                    Booking_Id = booking.Booking_Id,
                                                    Position_Id = position.Position_Id,
                                                    //ChargeBasis = ChargeBasis,
                                                    IsRecursive = true,
                                                    MealPlan_Id = DefMealPlan?.MealPlan_Id,
                                                    MealPlan = DefMealPlan?.MealPlan,
                                                    StartDate = position.STARTDATE,
                                                    EndDate = position.ENDDATE,
                                                    BuyCurrency_Id = position.BuyCurrency_Id,
                                                    BuyCurrency_Name = RoomRate.BuyCurrency_Name != null ? RoomRate.BuyCurrency_Name : position.BuyCurrency_Name,
                                                    Action = ProductContractsBuy == null ? "R" : "N",
                                                    BuyContract_Id = ProductContractsBuy?.BuyContract_Id,
                                                    BuyPositionPrice_Id = BuyContractPrices?.ProductPrice_ID,
                                                    SellContract_Id = ProductContractsSell?.BuyContract_Id,
                                                    SellPositionPrice_Id = SellContractPrices?.ProductPrice_ID,
                                                    SellContractCurrency_Id = ProductContractsBuy?.Contract_Currency_Id,
                                                    SellContractCurrency_Name = ProductContractsBuy?.Contract_Currency,
                                                    ContractedSellPrice = SellContractPrices?.Contract_Price,
                                                    ContractedBuyPrice = BuyContractPrices?.Contract_Price,
                                                    BookingSeason_Id = position.BookingSeason.FirstOrDefault()?.BookingSeason_ID,
                                                    InvForPax = position.PositionType == "Core" ? null : "N",
                                                    InvNumber = position.PositionType == "Core" ? null : (ChargeBasis != "PRPN" ? RoomRate.Req_Count : (RoomRate.Req_Count * Occupancy)),
                                                    AuditTrail = new AuditTrail
                                                    {
                                                        CREA_DT = DateTime.Now,
                                                        CREA_US = request.UserEmailId
                                                    }
                                                };

                                                position.BookingRoomsAndPrices.Add(PosPrice);
                                            }

                                            PosPrice.RoomName = productRange?.ProductTemplateName;
                                            PosPrice.RoomShortCode = productRange?.ProductTemplateCode;
                                            PosPrice.CategoryName = CategoryName;
                                            PosPrice.Category_Id = productCategory?.DefProductCategory_Id;
                                            PosPrice.ProductTemplate_Id = productRange?.ProductTemplate_Id;
                                            PosPrice.RoomShortCode = productRange?.ProductTemplateCode;
                                            PosPrice.Capacity = productRange?.Quantity;
                                            PosPrice.ChargeBasis = ChargeBasis;
                                            PosPrice.ProductRange_Id = RoomRate.ProductRangeID;
                                            PosPrice.PersonType = RoomRate.PersonType;
                                            PosPrice.PersonType_Id = RoomRate.PersonTypeID;
                                            PosPrice.Req_Count = RoomRate.Req_Count;
                                            PosPrice.Age = RoomRate.Age;
                                            PosPrice.BudgetPrice = RoomRate.BudgetPrice;
                                            PosPrice.RequestedPrice = RoomRate.RequestedPrice;
                                            PosPrice.BuyPrice = RoomRate.BuyPrice;
                                            PosPrice.OneOffDate = _genericRepository.ConvertStringToDateTime(RoomRate.OneOffDate);
                                            PosPrice.ConfirmedReqPrice = RoomRate.ConfirmedReqPrice;
                                            PosPrice.ExcludeFromInvoice = RoomRate.ExcludeFromInvoice;
                                            PosPrice.ApplyMarkup = RoomRate.ApplyMarkup;

                                            if (RoomRate.IsAdditionalYN != null)
                                            {
                                                PosPrice.IsAdditionalYN = RoomRate.IsAdditionalYN.Value;
                                            }
                                            else
                                            {
                                                PosPrice.IsAdditionalYN = true;
                                            }
                                            PosPrice.Status = RoomRate.Status;
                                        }
                                    }
                                    #endregion

                                    #region Rooms and prices Budget Suppliments
                                    keyValue = request.OpsKeyValue.Where(a => a.Key == "TableBudgetSuppliments").FirstOrDefault();
                                    if (keyValue != null)
                                    {
                                        PosBudgetSupp = JsonConvert.DeserializeObject<List<OpsBudgetSupplements>>(keyValue.Value.ToString());

                                        foreach (var BudgSupp in PosBudgetSupp)
                                        {
                                            PosPrice = position.BookingRoomsAndPrices?.Where(a => a.BookingRooms_Id == BudgSupp.BookingRooms_Id).FirstOrDefault();
                                            PosBudgSupp = position.BudgetSupplements?.Where(a => a.BudgetSupplement_Id == BudgSupp.BudgetSupplement_Id).FirstOrDefault();
                                            if (PosBudgSupp == null)
                                            {
                                                PosBudgSupp = new BudgetSupplements
                                                {
                                                    BudgetSupplement_Id = BudgSupp.BudgetSupplement_Id,
                                                    CREA_DT = DateTime.Now,
                                                    CREA_US = request.UserEmailId,
                                                };
                                                if (position.BudgetSupplements == null)
                                                    position.BudgetSupplements = new List<BudgetSupplements>();
                                                position.BudgetSupplements.Add(PosBudgSupp);
                                            }

                                            PosBudgSupp.BookingRooms_Id = BudgSupp.BookingRooms_Id;
                                            PosBudgSupp.RoomShortCode = BudgSupp.ProductRange;
                                            PosBudgSupp.BudgetSupplementAmount = BudgSupp.BudgetSupplementAmount;
                                            PosBudgSupp.BudgetSupplementReason = BudgSupp.BudgetSupplementReason;
                                            PosBudgSupp.AgentConfirmed = BudgSupp.AgentConfirmed;
                                            PosBudgSupp.ApplyMarkUp = BudgSupp.ApplyMarkUp;
                                            PosBudgSupp.PersonType = PosPrice?.PersonType;
                                            PosBudgSupp.BudgetSuppCurrencyId = position.BuyCurrency_Id;
                                            PosBudgSupp.BudgetSuppCurrencyName = position.BuyCurrency_Name;
                                            PosBudgSupp.SupplementNumber = PosPrice?.InvNumber;
                                            PosBudgSupp.MODI_DT = DateTime.Now;
                                            PosBudgSupp.MODI_US = request.UserEmailId;
                                            PosBudgSupp.status = BudgSupp.status;
                                            
                                            if (PosPrice?.InvForPax == "N")
                                            {
                                                PosBudgSupp.SupplementFor = "Number";
                                            }
                                            else if (PosPrice?.InvForPax == "P")
                                            {
                                                PosBudgSupp.SupplementFor = "Paying Pax";
                                            }
                                            else if (PosPrice?.InvForPax == "N")
                                            {
                                                PosBudgSupp.SupplementFor = "Unit";
                                            }
                                            else
                                            {
                                                PosBudgSupp.SupplementFor = "All Pax";
                                            }
                                        }
                                    }
                                    #endregion
                                }
                                #endregion

                                #region Position AuditTrail
                                position.AuditTrail.MODI_US = request.UserEmailId;
                                position.AuditTrail.MODI_DT = DateTime.Now;
                                #endregion
                            }
                        }
                        else
                        {
                            response.Status = "Failure";
                            response.ErrorMessage.Add("UpdateBookingRoomsAndPrices:-Bookings->Position details not found.");
                        }
                    }
                    else
                    {
                        response.Status = "Failure";
                        response.ErrorMessage.Add("UpdateBookingRoomsAndPrices:-PositionIds can not be null.");
                    }
                }
                else
                {
                    response.Status = "Failure";
                    response.ErrorMessage.Add("UpdateBookingRoomsAndPrices:-Bookings/OpsKeyValue can not be null.");
                }
            }
            catch (Exception ex)
            {
                response.Status = "Failure";
                response.ErrorMessage.Add("UpdateBookingRoomsAndPrices:- " + ex.Message);
            }

            return response;
        }

        public OPSWorkflowResponseStatus UpdatePurchaseFOC(ref Bookings booking, ExecuteWorkflowActionReq request)
        {
            OPSWorkflowResponseStatus response = new OPSWorkflowResponseStatus() { Status = "Success" };
            try
            {
                if (booking != null && request.OpsKeyValue?.Count > 0)
                {
                    if (request.PositionIds?.Count > 0)
                    {
                        var KeyValueList = request.OpsKeyValue;
                        var keyValue = new OpsKeyValue();
                        PositionFOC PosFOC;
                        List<OpsPositionFOC> PosFOCList;

                        var positionList = booking.Positions.Where(x => request.PositionIds.Contains(x.Position_Id)).ToList();

                        if (positionList?.Count > 0)
                        {
                            foreach (var position in positionList)
                            {
                                #region Save Position
                                keyValue = KeyValueList.Where(a => a.Key == "saveposition").FirstOrDefault();
                                if (keyValue != null)
                                {
                                    #region Position FOC
                                    keyValue = request.OpsKeyValue.Where(a => a.Key == "TableOpsPosFOC").FirstOrDefault();
                                    if (keyValue != null)
                                    {
                                        PosFOCList = JsonConvert.DeserializeObject<List<OpsPositionFOC>>(keyValue.Value.ToString());

                                        foreach (var foc in PosFOCList)
                                        {
                                            PosFOC = position.PositionFOC.Where(a => a.PositionFOC_Id == foc.PositionFOC_Id).FirstOrDefault();
                                            if (PosFOC == null)
                                            {
                                                PosFOC = new PositionFOC
                                                {
                                                    PositionFOC_Id = foc.PositionFOC_Id,
                                                    AuditTrail = new AuditTrail
                                                    {
                                                        CREA_DT = DateTime.Now,
                                                        CREA_US = request.UserEmailId
                                                    }
                                                };
                                                position.PositionFOC.Add(PosFOC);
                                            }

                                            PosFOC.BuyBookingRooms_ID = foc.BuyBookingRoomsId;
                                            PosFOC.BuyRoomShortCode = foc.BuyRoomShortCode;
                                            PosFOC.BuyQuantity = foc.BuyQuantity;
                                            PosFOC.GetBookingRooms_ID = foc.GetBookingRoomsId;
                                            PosFOC.GetRoomShortCode = foc.GetRoomShortCode;
                                            PosFOC.GetQuantity = foc.GetQuantity;
                                            PosFOC.AuditTrail.MODI_DT = DateTime.Now;
                                            PosFOC.AuditTrail.MODI_US = request.UserEmailId;
                                        }
                                    }
                                    #endregion
                                }
                                #endregion

                                #region Position AuditTrail
                                position.AuditTrail.MODI_US = request.UserEmailId;
                                position.AuditTrail.MODI_DT = DateTime.Now;
                                #endregion
                            }
                        }
                        else
                        {
                            response.Status = "Failure";
                            response.ErrorMessage.Add("UpdatePurchaseFOC:-Bookings->Position details not found.");
                        }
                    }
                    else
                    {
                        response.Status = "Failure";
                        response.ErrorMessage.Add("UpdatePurchaseFOC:-PositionIds can not be null.");
                    }
                }
                else
                {
                    response.Status = "Failure";
                    response.ErrorMessage.Add("UpdatePurchaseFOC:-Bookings/OpsKeyValue can not be null.");
                }
            }
            catch (Exception ex)
            {
                response.Status = "Failure";
                response.ErrorMessage.Add("UpdatePurchaseFOC:- " + ex.Message);
            }

            return response;
        }

        public OPSWorkflowResponseStatus GenerateDocAndUpdCommsLog(ref Bookings booking, ExecuteWorkflowActionReq request)
        {
            OPSWorkflowResponseStatus response = new OPSWorkflowResponseStatus() { Status = "Success" };
            if (!string.IsNullOrWhiteSpace(request.DocType))
            {
                try
                {
                    EmailGetReq objEmailGetReq = new EmailGetReq()
                    {
                        BookingNo = booking.BookingNumber,
                        DocumentType = request.DocType,
                        PlacerUserId = request.UserId,
                        UserEmail = request.UserEmailId,
                        QrfId = booking.QRFID,
                        SystemCompany_Id = booking.SystemCompany_Id,
                        UserName = request.UserName,
                        Module = "ops",
                        IsSendEmail = request.IsSendEmail,
                        IsSaveDocStore = request.IsSaveDocStore
                    };

                    if (request.PositionIds?.Count > 0)
                    {
                        if (request.PositionIds.Count == 1)
                        {
                            objEmailGetReq.PositionId = request.PositionIds.FirstOrDefault();
                            objEmailGetReq.SupplierId = booking.Positions.Where(a => a.Position_Id == objEmailGetReq.PositionId)?.FirstOrDefault()?.SupplierInfo.Id;
                        }
                        else if (request.PositionIds.Count > 1)
                        {
                            objEmailGetReq.PositionIds = request.PositionIds;
                        }
                    }

                    var responseStatusMail = _emailRepository.GenerateEmail(objEmailGetReq).Result;
                    response.Status = responseStatusMail?.ResponseStatus?.Status;
                    response.ErrorMessage = responseStatusMail?.ResponseStatus?.ErrorMessage?.Split("|").ToList();
                }
                catch (Exception ex)
                {
                    response.Status = "Failure";
                    response.ErrorMessage.Add("GenerateDocAndUpdCommsLog:- " + ex.Message);
                }
            }
            return response;
        }

        public OPSWorkflowResponseStatus GenerateDocAndUpdCommsLogCanSupp(ref Bookings booking, ExecuteWorkflowActionReq request)
        {
            OPSWorkflowResponseStatus response = new OPSWorkflowResponseStatus() { Status = "Success" };
            try
            {
                var PositionStatus = booking.Positions.Where(a => a.Position_Id == request.PositionIds[0]).FirstOrDefault()?.STATUS;
                List<string> changeSupplierStatus = new List<string>() { "k", "B", "i", "m" };
                if (changeSupplierStatus.Contains(PositionStatus.ToLower()))
                {
                    request.DocType = DocType.BOOKXX;
                    response = GenerateDocAndUpdCommsLog(ref booking, request);
                }
            }
            catch (Exception ex)
            {
                response.Status = "Failure";
                response.ErrorMessage.Add("GenerateDocAndUpdCommsLog:- " + ex.Message);
            }
            return response;
        }

        public OPSWorkflowResponseStatus SendHotelReservationRequest(ref Bookings booking, ExecuteWorkflowActionReq request)
        {
            OPSWorkflowResponseStatus response = new OPSWorkflowResponseStatus() { Status = "Success" };
            try
            {
                if (request.DocType == DocType.BOOKREQ)
                {
                    HotelReservationRequestEmail requestHR = new HotelReservationRequestEmail();
                    requestHR.BookingNumber = booking.BookingNumber;
                    requestHR.PositionId = request.PositionIds[0];
                    requestHR.PlacerEmail = request.UserEmailId;
                    requestHR.PlacerUserId = request.UserId;
                    requestHR.SendType = "opschangeproduct";
                    var responseHR = _hotelsDeptRepository.SendHotelReservationRequestEmail(requestHR)?.Result;
                    response.Status = responseHR?.ResponseStatus?.Status;
                }
                else
                {
                    response = GenerateDocAndUpdCommsLog(ref booking, request);
                }
            }
            catch (Exception ex)
            {
                response.Status = "Failure";
                response.ErrorMessage.Add("SendHotelReservationRequest:- " + ex.Message);
            }
            return response;
        }

        public OPSWorkflowResponseStatus SyncBookingStatus(ref Bookings booking, ExecuteWorkflowActionReq request)
        {
            OPSWorkflowResponseStatus response = new OPSWorkflowResponseStatus() { Status = "Success" };
            try
            {
                if (booking != null)
                {
                    //If the Currenct booking Status = 'N' OR 'T' then Booking Status will not be Synced by Position if Booking is in Quote and Template stage.
                    if (booking.STATUS.ToLower() == "n" || booking.STATUS.ToLower() == "t")
                    {
                        response.Status = "Success";
                        return response;
                    }
                    else
                    {
                        bool IsStatusChanged = true;
                        List<string> lstPosStatus = new List<string>() { "k", "b" };
                        var activepos = booking.Positions.Where(a => a.STATUS.ToLower() != "c").ToList();//active positions excluding cancel positions

                        int totalPosCnt = booking.Positions.Count;                                       //total positions count
                        int activePosCnt = activepos.Count;                                              //active positions count            
                        int cancelcnt = booking.Positions.Where(a => a.STATUS.ToLower() == "c").Count(); //cancel positions count
                        int ttlPosOStatus = activepos.Where(a => a.STATUS.ToLower() == "o").Count();     //O status positions count
                        int ttlPosMStatus = activepos.Where(a => a.STATUS.ToLower() == "m").Count();     //M status positions count 

                        //If all the position status are in 'K' or 'B'
                        var posKB = activepos.Where(a => lstPosStatus.Contains(a.STATUS.ToLower())).ToList();
                        int posKBcnt = posKB.Count;

                        if (posKBcnt == activePosCnt)
                        {
                            int posIStatus = activepos.Where(a => a.InvoiceStatus.ToLower() == "i").Count();
                            int posBStatus = activepos.Where(a => a.InvoiceStatus.ToLower() != "i" && a.STATUS.ToLower() == "b").Count();
                            int posKStatus = activepos.Where(a => a.InvoiceStatus.ToLower() != "i" && a.STATUS.ToLower() == "k").Count();

                            booking.STATUS = posIStatus == activePosCnt ? "I" : posBStatus == activePosCnt ? "B" : posKStatus == activePosCnt ? "K" : booking.STATUS;
                        }
                        //If all of the position Status is in 'O'
                        else if (activePosCnt == ttlPosOStatus)
                        {
                            booking.STATUS = "O";
                        }
                        //If any of the position Status is in 'M'
                        else if (ttlPosMStatus > 0)
                        {
                            booking.STATUS = "M";
                        }
                        //If all of the position Status is in 'C'
                        else if (totalPosCnt == cancelcnt)
                        {
                            booking.STATUS = "C";
                        }
                        else
                        {
                            IsStatusChanged = false;
                        }

                        if (IsStatusChanged)
                        {
                            booking.AuditTrail.STATUS_DT = DateTime.Now;
                            booking.AuditTrail.STATUS_US = request.UserEmailId;
                        }
                    }
                }
                else
                {
                    response.Status = "Failure";
                    response.ErrorMessage.Add("SyncBookingStatus:-Bookings/OpsKeyValue can not be null.");
                }
            }
            catch (Exception ex)
            {
                response.Status = "Failure";
                response.ErrorMessage.Add("SyncBookingStatus:- " + ex.Message);
            }
            return response;
        }

        public OPSWorkflowResponseStatus GeneratePDF(ref Bookings booking, ExecuteWorkflowActionReq request)
        {
            OPSWorkflowResponseStatus response = new OPSWorkflowResponseStatus() { Status = "Success" };
            try
            {
                PDFGetReq objPDFGetReq = new PDFGetReq()
                {
                    BookingNo = booking.BookingNumber,
                    DocumentType = request.DocType,
                    UserId = request.UserId,
                    UserEmail = request.UserEmailId,
                    UserName = request.UserName,
                    PositionIds = request.PositionIds,
                    Module = request.Module,
                    IsSendEmail = request.IsSendEmail,
                    QRFID = booking.QRFID,
                    SystemCompany_Id = booking.SystemCompany_Id
                };

                var responseStatusMail = _pdfRepository.GeneratePDF(objPDFGetReq).Result;
                response.Status = responseStatusMail?.ResponseStatusMessage.Status;
                response.ErrorMessage = responseStatusMail?.ResponseStatusMessage.ErrorMessage;
                response.DocumentDetails = responseStatusMail?.PDFTemplateGetRes.Select(a => a.DocumentDetails).ToList();
            }
            catch (Exception ex)
            {
                response.Status = "Failure";
                response.ErrorMessage.Add("GeneratePDF:- " + ex.Message);
            }
            return response;
        }

        public OPSWorkflowResponseStatus CommitBooking(ref Bookings booking)
        {
            OPSWorkflowResponseStatus response = new OPSWorkflowResponseStatus() { Status = "Success" };
            try
            {
                var replaceResult = _MongoContext.Bookings.ReplaceOneAsync(Builders<Bookings>.Filter.Eq("BookingNumber", booking.BookingNumber), booking).Result;
                if (replaceResult.ModifiedCount < 1)
                {
                    response.Status = "Error";
                }
            }
            catch (Exception ex)
            {
                response.Status = "Failure";
                response.ErrorMessage.Add("CommitBooking:- " + ex.Message);
            }
            return response;
        }

        public OPSWorkflowResponseStatus UpdateBookingRoomsAndPricesForPosition(ref Bookings booking, ExecuteWorkflowActionReq request)
        {
            OPSWorkflowResponseStatus response = new OPSWorkflowResponseStatus() { Status = "Success" };
            List<OpsKeyValue> OpsKeyValue = new List<OpsKeyValue>();
            BookingRoomsAndPrices bookingRoomAndPrices = new BookingRoomsAndPrices();
            List<OpsBookingRoomsDetails> BookingDetails = new List<OpsBookingRoomsDetails>();
            List<OpsBookingPaxDetails> BookingPaxDetails = new List<OpsBookingPaxDetails>();
            List<OpsBookingPaxDetails> BookingDetailsPax = new List<OpsBookingPaxDetails>();
            List<OpsBookingPaxDetails> BookingDetailsPaxForRemoved = new List<OpsBookingPaxDetails>();
            BookingRoomsAndPrices BookingRoomsAndPrice = new BookingRoomsAndPrices();
            List<TemplateBookingRoomsGrid> BookingRoomListData = new List<TemplateBookingRoomsGrid>();
            Products product = new Products();
            ProductCategory productCategories = new ProductCategory();
            List<OpsBookingRoomsDetails> RemovedBookingRoomsFromUi = new List<OpsBookingRoomsDetails>();
            BookingRoomsAndPrices RemoveRoom = new BookingRoomsAndPrices();

            try
            {
                if (booking != null && request.OpsKeyValue?.Count > 0)
                {
                    var KeyValueList = request.OpsKeyValue;
                    var keyValueForPositions = new OpsKeyValue();
                    var keyValueForBookingRooms = new OpsKeyValue();
                    var keyValueForBookingPax = new OpsKeyValue();
                    keyValueForPositions = KeyValueList.Where(x => x.Key == "Positions").FirstOrDefault();
                    keyValueForBookingRooms = KeyValueList.Where(x => x.Key == "BookingRooms").FirstOrDefault();
                    keyValueForBookingPax = KeyValueList.Where(x => x.Key == "BookingPax").FirstOrDefault();
                    var PositionsFromUi = JsonConvert.DeserializeObject<List<OpsBookingPositionDetails>>(keyValueForPositions.Value.ToString());
                    var ListOfPos = PositionsFromUi.Select(x => x.Position_ID).ToList();
                    if (ListOfPos?.Count > 0)
                    {
                        var positionList = booking.Positions.Where(x => ListOfPos.Contains(x.Position_Id)).ToList();
                        if (positionList != null && positionList.Any())
                        {
                            foreach (var pos in positionList)
                            {

                                List<OpsPositionRoomPrice> PostionBookingRooms = new List<OpsPositionRoomPrice>();
                                List<OpsPositionRoomPrice> PostionBookingRoomsForSupplements = new List<OpsPositionRoomPrice>();
                                List<BookingRoomsAndPrices> NewListForSuppliments = new List<BookingRoomsAndPrices>();
                                List<BookingRoomsAndPrices> BookingRoomsAndPrices = new List<BookingRoomsAndPrices>();
                                var Charge = _MongoContext.mProductType.AsQueryable().Where(x => x.VoyagerProductType_Id == pos.ProductType_Id).FirstOrDefault()?.ChargeBasis;
                                if (Charge == "PRPN")
                                {

                                    product = _MongoContext.Products.AsQueryable().Where(a => a.VoyagerProduct_Id == pos.Product_Id).FirstOrDefault();

                                    if (keyValueForBookingRooms != null)
                                    {
                                        BookingDetails = JsonConvert.DeserializeObject<List<OpsBookingRoomsDetails>>(keyValueForBookingRooms.Value.ToString());
                                        RemovedBookingRoomsFromUi = BookingDetails.Where(x => x.Status == "X").ToList();
                                        BookingDetails = BookingDetails.Where(x => x.Status != "X").ToList();
                                        BookingRoomsAndPrices = booking.Positions.Where(x => x.Position_Id == pos.Position_Id)?.FirstOrDefault()?.BookingRoomsAndPrices;
                                        foreach (var BookingDetailsInTemplate in BookingDetails)
                                        {
                                            if (BookingDetailsInTemplate.RoomType != null && (BookingDetailsInTemplate.RoomType == "SINGLE" || BookingDetailsInTemplate.RoomType == "DOUBLE" || BookingDetailsInTemplate.RoomType == "QUAD" || BookingDetailsInTemplate.RoomType == "TRIPLE" || BookingDetailsInTemplate.RoomType == "TWIN" || BookingDetailsInTemplate.RoomType == "TSU"))
                                            {
                                                BookingRoomsAndPrice = BookingRoomsAndPrices.Where(x => x.RoomShortCode != null && x.RoomShortCode.ToUpper() == BookingDetailsInTemplate?.RoomType?.ToUpper() && x.PersonType != null && x.PersonType == "ADULT")?.FirstOrDefault();
                                            }
                                            else
                                            {
                                                if (BookingDetailsInTemplate.RoomType?.Trim()?.ToUpper() == "CHILDWITHBED" && BookingDetailsInTemplate.Age != null)
                                                {
                                                    BookingRoomsAndPrice = BookingRoomsAndPrices.Where(x => x.PersonType != null && x.PersonType == "Child + Bed" && x.Age != null && x.Age == BookingDetailsInTemplate.Age)?.FirstOrDefault();
                                                    if (BookingRoomsAndPrice == null)
                                                    {
                                                        BookingRoomsAndPrice = BookingRoomsAndPrices.Where(x => x.PersonType != null && x.PersonType == "CHILD" && x.Age != null && x.Age == BookingDetailsInTemplate.Age)?.FirstOrDefault();
                                                    }
                                                }
                                                if (BookingDetailsInTemplate.RoomType?.Trim()?.ToUpper() == "CHILDWITHOUTBED" && BookingDetailsInTemplate.Age != null)
                                                {
                                                    BookingRoomsAndPrice = BookingRoomsAndPrices.Where(x => x.PersonType != null && x.PersonType == "Child - Bed" && x.Age != null && x.Age == BookingDetailsInTemplate.Age)?.FirstOrDefault();
                                                    if (BookingRoomsAndPrice == null)
                                                    {
                                                        BookingRoomsAndPrice = BookingRoomsAndPrices.Where(x => x.PersonType != null && x.PersonType == "CHILD" && x.Age != null && x.Age == BookingDetailsInTemplate.Age)?.FirstOrDefault();
                                                    }
                                                }
                                                if (BookingDetailsInTemplate.RoomType?.Trim()?.ToUpper() == "INFANT" && BookingDetailsInTemplate.Age != null)
                                                {
                                                    BookingRoomsAndPrice = BookingRoomsAndPrices.Where(x => x.PersonType != null && x.PersonType == "INFANT" && x.Age != null && x.Age == BookingDetailsInTemplate.Age)?.FirstOrDefault();
                                                }
                                            }
                                            if (BookingRoomsAndPrice != null)
                                            {
                                                BookingRoomsAndPrice.Req_Count = BookingDetailsInTemplate.NewLevel;
                                                BookingRoomsAndPrice.Status = null;
                                            }
                                            else
                                            {
                                                BookingRoomsAndPrices bookingpriceroom = new BookingRoomsAndPrices();

                                                bookingpriceroom.Booking_Id = booking.Booking_Id;

                                                var CategoryId = pos?.BookingRoomsAndPrices?.FirstOrDefault()?.Category_Id;
                                                if (CategoryId != null)
                                                {
                                                    productCategories = product?.ProductCategories?.Where(x => x.DefProductCategory_Id == CategoryId).FirstOrDefault();
                                                }
                                                if (BookingDetailsInTemplate?.RoomType?.ToUpper() != "CHILDWITHBED" && BookingDetailsInTemplate?.RoomType?.ToUpper() != "CHILDWITHOUTBED" && BookingDetailsInTemplate?.RoomType?.ToUpper() != "INFANT")
                                                {
                                                    bookingpriceroom.ProductRange_Id = productCategories?.ProductRanges?.Where(x => x.ProductTemplateCode?.ToUpper() == BookingDetailsInTemplate?.RoomType?.ToUpper() && x.PersonType?.ToUpper() == "ADULT").FirstOrDefault()?.ProductRange_Id;

                                                }
                                                else
                                                {
                                                    if (BookingDetailsInTemplate?.RoomType?.ToUpper() == "CHILDWITHBED")
                                                    {
                                                        var ProductforChildWithBed = productCategories?.ProductRanges?.Where(x => x.PersonType != null && (x.PersonType == _configuration.GetValue<string>("GetRoomTypes:CHILDWITHBED")) && x.ProductTemplateCode == "CHILD");
                                                        foreach (var p in ProductforChildWithBed)
                                                        {
                                                            if (p.Agemin != null && p.Agemax != null && BookingDetailsInTemplate.Age != null)
                                                            {
                                                                var MinAge = Convert.ToInt32(p.Agemin);
                                                                var MaxAge = Convert.ToInt32(p.Agemax);
                                                                if (bookingpriceroom.ProductRange_Id == null)
                                                                    bookingpriceroom.ProductRange_Id = MinAge <= BookingDetailsInTemplate.Age.Value && BookingDetailsInTemplate.Age.Value <= MaxAge ? p?.ProductRange_Id : null;
                                                            }

                                                        }
                                                        if (bookingpriceroom.ProductRange_Id == null)
                                                        {
                                                            var AlternatePersonTypeInMdefpersontype = _MongoContext.mDefPersonType.AsQueryable().Where(x => x.PersonType == _configuration.GetValue<string>("GetRoomTypes:CHILDWITHBED"))?.FirstOrDefault()?.Alaernate_PersonType;
                                                            var ProductInfo = productCategories?.ProductRanges?.Where(x => x.PersonType != null && AlternatePersonTypeInMdefpersontype != null && x.PersonType == AlternatePersonTypeInMdefpersontype && x.ProductTemplateCode == AlternatePersonTypeInMdefpersontype);
                                                            foreach (var p in ProductInfo)
                                                            {
                                                                if (p.Agemin != null && p.Agemax != null && BookingDetailsInTemplate.Age != null)
                                                                {
                                                                    var AgeMin = Convert.ToInt32(p.Agemin);
                                                                    var AgeMax = Convert.ToInt32(p.Agemax);
                                                                    if (bookingpriceroom.ProductRange_Id == null)
                                                                        bookingpriceroom.ProductRange_Id = AgeMin <= BookingDetailsInTemplate.Age.Value && BookingDetailsInTemplate.Age.Value <= AgeMax ? p?.ProductRange_Id : null;
                                                                }
                                                            }
                                                        }

                                                        if (bookingpriceroom.ProductRange_Id == null)
                                                        {
                                                            var AlternatePersonTypeInMdefpersontype = _MongoContext.mDefPersonType.AsQueryable().Where(x => x.PersonType == _configuration.GetValue<string>("GetRoomTypes:CHILDWITHBED"))?.FirstOrDefault()?.Alaernate_PersonType;
                                                            bookingpriceroom.ProductRange_Id = productCategories?.ProductRanges?.Where(x => x.PersonType != null && AlternatePersonTypeInMdefpersontype != null && x.PersonType == AlternatePersonTypeInMdefpersontype && x.ProductTemplateCode == AlternatePersonTypeInMdefpersontype)?.FirstOrDefault()?.ProductRange_Id;
                                                        }

                                                        if (bookingpriceroom.ProductRange_Id != null)
                                                        {
                                                            var IsYnforCHILDWITHBED = productCategories?.ProductRanges?.Where(x => x.ProductRange_Id == bookingpriceroom.ProductRange_Id)?.FirstOrDefault()?.AdditionalYn;
                                                            if (IsYnforCHILDWITHBED != null && IsYnforCHILDWITHBED.HasValue)
                                                            {
                                                                bookingpriceroom.IsAdditionalYN = IsYnforCHILDWITHBED.Value;
                                                            }
                                                        }
                                                    }
                                                    if (BookingDetailsInTemplate?.RoomType?.ToUpper() == "CHILDWITHOUTBED")
                                                    {
                                                        var ProductForChildWithOutBed = productCategories?.ProductRanges?.Where(x => x.PersonType != null && (x.PersonType?.ToUpper() == _configuration.GetValue<string>("GetRoomTypes:CHILDWITHOUTBED") && x.ProductTemplateCode == "CHILD"));
                                                        foreach (var p in ProductForChildWithOutBed)
                                                        {
                                                            if (p.Agemax != null && p.Agemin != null && BookingDetailsInTemplate.Age != null)
                                                            {
                                                                var MinAge = Convert.ToInt32(p.Agemin);
                                                                var MaxAge = Convert.ToInt32(p.Agemax);
                                                                if (bookingpriceroom.ProductRange_Id == null)
                                                                    bookingpriceroom.ProductRange_Id = MinAge <= BookingDetailsInTemplate.Age.Value && BookingDetailsInTemplate.Age.Value <= MaxAge ? p?.ProductRange_Id : null;

                                                            }
                                                        }
                                                        if (bookingpriceroom.ProductRange_Id == null)
                                                        {
                                                            var AlternatePersonTypeInMdefpersontype = _MongoContext.mDefPersonType.AsQueryable().Where(x => x.PersonType == _configuration.GetValue<string>("GetRoomTypes:CHILDWITHOUTBED"))?.FirstOrDefault()?.Alaernate_PersonType;
                                                            var ProductInfo = productCategories?.ProductRanges?.Where(x => x.PersonType != null && AlternatePersonTypeInMdefpersontype != null && x.PersonType == AlternatePersonTypeInMdefpersontype && x.ProductTemplateCode == AlternatePersonTypeInMdefpersontype);
                                                            foreach (var p in ProductInfo)
                                                            {
                                                                if (p.Agemin != null && p.Agemax != null && BookingDetailsInTemplate.Age != null)
                                                                {
                                                                    var AgeMin = Convert.ToInt32(p.Agemin);
                                                                    var AgeMax = Convert.ToInt32(p.Agemax);
                                                                    if (bookingpriceroom.ProductRange_Id == null)
                                                                        bookingpriceroom.ProductRange_Id = AgeMin <= BookingDetailsInTemplate.Age.Value && BookingDetailsInTemplate.Age.Value <= AgeMax ? p?.ProductRange_Id : null;
                                                                }

                                                            }
                                                        }

                                                        if (bookingpriceroom.ProductRange_Id == null)
                                                        {
                                                            var AlternatePersonTypeInMdefpersontype = _MongoContext.mDefPersonType.AsQueryable().Where(x => x.PersonType == _configuration.GetValue<string>("GetRoomTypes:CHILDWITHOUTBED"))?.FirstOrDefault()?.Alaernate_PersonType;
                                                            bookingpriceroom.ProductRange_Id = productCategories?.ProductRanges?.Where(x => x.PersonType != null && AlternatePersonTypeInMdefpersontype != null && x.PersonType == AlternatePersonTypeInMdefpersontype && x.ProductTemplateCode == AlternatePersonTypeInMdefpersontype)?.FirstOrDefault()?.ProductRange_Id;

                                                        }

                                                        if (bookingpriceroom.ProductRange_Id != null)
                                                        {
                                                            var IsYnforCHILDWITHOUTBED = productCategories?.ProductRanges?.Where(x => x.ProductRange_Id == bookingpriceroom.ProductRange_Id)?.FirstOrDefault()?.AdditionalYn;
                                                            if (IsYnforCHILDWITHOUTBED != null && IsYnforCHILDWITHOUTBED.HasValue)
                                                            {
                                                                bookingpriceroom.IsAdditionalYN = IsYnforCHILDWITHOUTBED.Value;
                                                            }
                                                        }
                                                    }

                                                    if (BookingDetailsInTemplate?.RoomType?.ToUpper() == "INFANT")
                                                    {
                                                        bookingpriceroom.ProductRange_Id = productCategories?.ProductRanges?.Where(x => x.PersonType != null && x.PersonType?.ToUpper() == "INFANT").FirstOrDefault()?.ProductRange_Id;
                                                        if (bookingpriceroom.ProductRange_Id != null)
                                                        {
                                                            var IsYnforINFANT = productCategories?.ProductRanges?.Where(x => x.ProductRange_Id == bookingpriceroom.ProductRange_Id)?.FirstOrDefault()?.AdditionalYn;
                                                            if (IsYnforINFANT != null && IsYnforINFANT.HasValue)
                                                            {
                                                                bookingpriceroom.IsAdditionalYN = IsYnforINFANT.Value;
                                                            }
                                                        }
                                                    }
                                                }
                                                bookingpriceroom.CategoryName = product?.ProductCategories?.Where(x => x.DefProductCategory_Id == CategoryId)?.FirstOrDefault()?.ProductCategoryName;
                                                bookingpriceroom.BookingRooms_Id = Guid.NewGuid().ToString();
                                                bookingpriceroom.PositionPricing_Id = Guid.NewGuid().ToString();
                                                bookingpriceroom.BudgetPrice = 0.00M;
                                                bookingpriceroom.RequestedPrice = 0.00M;
                                                bookingpriceroom.BuyPrice = 0.00M;
                                                bookingpriceroom.Position_Id = pos.Position_Id;
                                                bookingpriceroom.ChargeBasis = _MongoContext.mProductType.AsQueryable().Where(x => x.VoyagerProductType_Id == pos.ProductType_Id)?.FirstOrDefault()?.ChargeBasis;
                                                bookingpriceroom.Req_Count = BookingDetailsInTemplate.NewLevel;
                                                bookingpriceroom.BuyCurrency_Name = BookingRoomsAndPrices.Where(x => x.BuyCurrency_Name != null && x.Status != "X")?.FirstOrDefault()?.BuyCurrency_Name;
                                                if (BookingDetailsInTemplate?.RoomType?.ToUpper() == "CHILDWITHBED" || BookingDetailsInTemplate?.RoomType?.ToUpper() == "CHILDWITHOUTBED" || BookingDetailsInTemplate?.RoomType?.ToUpper() == "INFANT")
                                                {
                                                    //var productTypeFromProductRanges = productCategories.ProductRanges.Where(x => x.ProductRange_Id == bookingpriceroom.ProductRange_Id)?.FirstOrDefault();
                                                    //if()
                                                    //bookingpriceroom.PersonType_Id = BookingDetailsInTemplate.RoomType.ToUpper() == "CHILDWITHBED" ? _MongoContext.mDefPersonType.AsQueryable().Where(a => a.PersonType.ToUpper() == "CHILD + BED").FirstOrDefault().defPersonType_Id : BookingDetailsInTemplate.RoomType.ToUpper() == "CHILDWITHOUTBED" ? _MongoContext.mDefPersonType.AsQueryable().Where(a => a.PersonType.ToUpper() == "CHILD - BED").FirstOrDefault().defPersonType_Id : BookingDetailsInTemplate.RoomType.ToUpper() == "INFANT" ? _MongoContext.mDefPersonType.AsQueryable().Where(a => a.PersonType.ToUpper() == "INFANT").FirstOrDefault().defPersonType_Id : null;
                                                    //if (BookingDetailsInTemplate?.RoomType?.ToUpper() == "INFANT")
                                                    //{
                                                    //    bookingpriceroom.PersonType = "INFANT";
                                                    //}
                                                    //else
                                                    //{
                                                    //    if (BookingDetailsInTemplate?.RoomType?.ToUpper() == "CHILDWITHBED")
                                                    //    {
                                                    //        bookingpriceroom.PersonType = "Child + Bed";
                                                    //    }
                                                    //    if (BookingDetailsInTemplate?.RoomType?.ToUpper() == "CHILDWITHOUTBED")
                                                    //    {
                                                    //        bookingpriceroom.PersonType = "Child - Bed";
                                                    //    }
                                                    //}

                                                    var productTypeFromProductRanges = productCategories?.ProductRanges?.Where(x => x.ProductRange_Id == bookingpriceroom.ProductRange_Id)?.FirstOrDefault();
                                                    if (productTypeFromProductRanges != null)
                                                    {
                                                        var ifPersonTypeExist = _MongoContext.mDefPersonType.AsQueryable().Where(a => a.defPersonType_Id == productTypeFromProductRanges.PersonType_Id)?.FirstOrDefault();
                                                        if (ifPersonTypeExist != null)
                                                        {
                                                            bookingpriceroom.PersonType_Id = ifPersonTypeExist.defPersonType_Id;
                                                            bookingpriceroom.PersonType = ifPersonTypeExist.PersonType;
                                                        }
                                                        else
                                                        {
                                                            var alternatepersontype = _MongoContext.mDefPersonType.AsQueryable().Where(a => a.Alaernate_PersonType == productTypeFromProductRanges.PersonType_Id)?.FirstOrDefault();
                                                            if (alternatepersontype != null)
                                                            {
                                                                bookingpriceroom.PersonType_Id = alternatepersontype.AlternateType_Id;
                                                                bookingpriceroom.PersonType = alternatepersontype.Alaernate_PersonType;

                                                            }
                                                        }
                                                    }


                                                }
                                                else
                                                {
                                                    var productTypeFromProductRangesForAdult = productCategories?.ProductRanges?.Where(x => x.ProductRange_Id == bookingpriceroom.ProductRange_Id)?.FirstOrDefault();
                                                    if (productTypeFromProductRangesForAdult != null)
                                                    {
                                                        var ifPersonTypeExist = _MongoContext.mDefPersonType.AsQueryable().Where(a => a.defPersonType_Id == productTypeFromProductRangesForAdult.PersonType_Id)?.FirstOrDefault();
                                                        if (ifPersonTypeExist != null)
                                                        {
                                                            bookingpriceroom.PersonType_Id = ifPersonTypeExist.defPersonType_Id;
                                                            bookingpriceroom.PersonType = ifPersonTypeExist.PersonType;
                                                        }
                                                        else
                                                        {
                                                            var alternatepersonTypeid = _MongoContext.mDefPersonType.AsQueryable().Where(a => a.Alaernate_PersonType == productTypeFromProductRangesForAdult.PersonType_Id)?.FirstOrDefault();
                                                            if (alternatepersonTypeid != null)
                                                            {
                                                                bookingpriceroom.PersonType_Id = alternatepersonTypeid.AlternateType_Id;
                                                                bookingpriceroom.PersonType = alternatepersonTypeid.Alaernate_PersonType;
                                                            }

                                                        }
                                                    }
                                                    //bookingpriceroom.PersonType_Id = _MongoContext.mDefPersonType.AsQueryable().Where(a => a.PersonType.ToUpper() == "ADULT")?.FirstOrDefault()?.defPersonType_Id;
                                                    //bookingpriceroom.PersonType = "ADULT";
                                                }
                                                bookingpriceroom.ApplyMarkup = true;
                                                bookingpriceroom.ExcludeFromInvoice = false;
                                                bookingpriceroom.ConfirmedReqPrice = false;

                                                bookingpriceroom.Req_Count = BookingDetailsInTemplate.NewLevel;

                                                if (BookingDetailsInTemplate.Age.HasValue)
                                                {
                                                    bookingpriceroom.Age = BookingDetailsInTemplate.Age.Value;
                                                }
                                                bookingpriceroom.Status = null;
                                                if ((BookingDetailsInTemplate?.RoomType?.ToUpper() == "CHILDWITHBED" || BookingDetailsInTemplate?.RoomType?.ToUpper() == "CHILDWITHOUTBED" || BookingDetailsInTemplate?.RoomType?.ToUpper() == "INFANT") && BookingDetailsInTemplate.Age != null)
                                                {
                                                    bookingpriceroom.Age = BookingDetailsInTemplate.Age.Value;
                                                }
                                                if (BookingDetailsInTemplate?.RoomType?.ToUpper() == "CHILDWITHBED" || BookingDetailsInTemplate?.RoomType?.ToUpper() == "CHILDWITHOUTBED" || BookingDetailsInTemplate?.RoomType?.ToUpper() == "INFANT" || BookingDetailsInTemplate?.RoomType?.ToUpper() == "SINGLE" || BookingDetailsInTemplate?.RoomType?.ToUpper() == "DOUBLE" || BookingDetailsInTemplate?.RoomType?.ToUpper() == "TWIN" || BookingDetailsInTemplate?.RoomType?.ToUpper() == "TRIPLE" || BookingDetailsInTemplate?.RoomType?.ToUpper() == "QUAD" || BookingDetailsInTemplate?.RoomType?.ToUpper() == "TSU")
                                                {
                                                    if (bookingpriceroom.IsAdditionalYN)
                                                    {
                                                        if (bookingpriceroom.ProductRange_Id != null)
                                                        {
                                                            NewListForSuppliments.Add(bookingpriceroom);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (bookingpriceroom.ProductRange_Id != null)
                                                        {
                                                            BookingRoomsAndPrices.Add(bookingpriceroom);
                                                        }
                                                    }
                                                }


                                            }
                                        }
                                        //Logic to Remove Rooms
                                        foreach (var removedroomtbr in RemovedBookingRoomsFromUi)
                                        {
                                            if (removedroomtbr.RoomType?.ToUpper() != "CHILDWITHBED" && removedroomtbr.RoomType?.ToUpper() != "CHILDWITHOUTBED" && removedroomtbr.RoomType?.ToUpper() != "INFANT")
                                            {
                                                var Bookingpriceroom = BookingRoomsAndPrices.Where(x => x.RoomShortCode != null && x.RoomShortCode?.ToUpper() == removedroomtbr?.RoomType?.ToUpper() && x.PersonType != null && x.PersonType == "ADULT" && x.Status != "X" && x.Status != "-")?.FirstOrDefault();
                                                if (Bookingpriceroom != null)
                                                {
                                                    Bookingpriceroom.Status = "X";
                                                }
                                            }
                                            else
                                            {
                                                if (removedroomtbr?.RoomType?.ToUpper() == "CHILDWITHBED")
                                                {
                                                    RemoveRoom = BookingRoomsAndPrices.Where(x => x.PersonType != null && x.PersonType == _configuration.GetValue<string>("GetRoomTypes:CHILDWITHBED") && x.Age != null && removedroomtbr.Age != null && x.Age.Value == removedroomtbr.Age && x.Status != "X" && x.Status != "-")?.FirstOrDefault();
                                                    if (RemoveRoom == null)
                                                    {
                                                        RemoveRoom = BookingRoomsAndPrices.Where(x => x.PersonType != null && x.PersonType == "CHILD" && x.Age != null && removedroomtbr.Age != null && x.Age.Value == removedroomtbr.Age && x.Status != "X" && x.Status != "-")?.FirstOrDefault();
                                                    }
                                                    if (RemoveRoom != null)
                                                    {
                                                        RemoveRoom.Status = "X";
                                                    }
                                                }
                                                if (removedroomtbr?.RoomType?.ToUpper() == "CHILDWITHOUTBED")
                                                {
                                                    RemoveRoom = BookingRoomsAndPrices.Where(x => x.PersonType != null && x.PersonType == _configuration.GetValue<string>("GetRoomTypes:CHILDWITHOUTBED") && x.Age != null && removedroomtbr.Age != null && x.Age.Value == removedroomtbr.Age && x.Status != "X" && x.Status != "-")?.FirstOrDefault();
                                                    if (RemoveRoom == null)
                                                    {
                                                        RemoveRoom = BookingRoomsAndPrices.Where(x => x.PersonType != null && x.PersonType == "CHILD" && x.Age != null && removedroomtbr.Age != null && x.Age.Value == removedroomtbr.Age && x.Status != "X" && x.Status != "-")?.FirstOrDefault();
                                                    }
                                                    if (RemoveRoom != null)
                                                    {
                                                        RemoveRoom.Status = "X";
                                                    }
                                                }
                                                if (removedroomtbr?.RoomType?.ToUpper() == "INFANT")
                                                {
                                                    RemoveRoom = BookingRoomsAndPrices.Where(x => x.PersonType != null && x.PersonType == _configuration.GetValue<string>("GetRoomTypes:INFANT") && x.Age != null && removedroomtbr.Age != null && x.Age.Value == removedroomtbr.Age && x.Status != "X" && x.Status != "-")?.FirstOrDefault();
                                                    if (RemoveRoom != null)
                                                    {
                                                        RemoveRoom.Status = "X";
                                                    }
                                                }

                                            }
                                        }


                                        //Logic to Convert in Required Type
                                        foreach (var Rooms in BookingRoomsAndPrices)
                                        {
                                            OpsPositionRoomPrice ops = new OpsPositionRoomPrice();
                                            ops.BookingRooms_Id = Rooms.BookingRooms_Id;
                                            ops.Booking_Id = Rooms.Booking_Id;
                                            ops.PositionPricing_Id = Rooms.PositionPricing_Id;
                                            ops.Position_Id = Rooms.Position_Id;
                                            ops.PersonType = Rooms.PersonType;
                                            ops.PersonTypeID = Rooms.PersonType_Id;
                                            var CategoryId = pos?.BookingRoomsAndPrices?.FirstOrDefault()?.Category_Id;
                                            if (CategoryId != null && Rooms.ProductRange_Id != null)
                                            {
                                                productCategories = product?.ProductCategories?.Where(x => x.DefProductCategory_Id == CategoryId).FirstOrDefault();
                                                var productrange = productCategories?.ProductRanges.Where(x => x.ProductRange_Id == Rooms.ProductRange_Id)?.FirstOrDefault()?.ProductTemplateCode;
                                                if (productrange != null)
                                                {
                                                    ops.ProductRange = productrange;
                                                }
                                            }

                                            ops.BudgetPrice = Rooms.BudgetPrice;
                                            if (Rooms.ConfirmedReqPrice != null && Rooms.ConfirmedReqPrice.HasValue)
                                                ops.ConfirmedReqPrice = Rooms.ConfirmedReqPrice.Value;
                                            ops.RequestedPrice = Rooms.RequestedPrice;
                                            ops.BuyPrice = Rooms.BuyPrice;

                                            ops.ProductRangeID = Rooms.ProductRange_Id;
                                            if (Rooms.ApplyMarkup != null && Rooms.ApplyMarkup.HasValue)
                                                ops.ApplyMarkup = Rooms.ApplyMarkup.Value;
                                            if (Rooms.ExcludeFromInvoice != null && Rooms.ExcludeFromInvoice.HasValue)
                                                ops.ExcludeFromInvoice = Rooms.ExcludeFromInvoice.Value;
                                            ops.Req_Count = Rooms.Req_Count;
                                            ops.Age = Rooms.Age;
                                            ops.Status = Rooms.Status;
                                            ops.ChargeBasis = Rooms.ChargeBasis;
                                            ops.OnReqQty = Rooms.Req_Count;
                                            ops.CategoryName = Rooms.CategoryName;
                                            ops.IsAdditionalYN = Rooms.IsAdditionalYN;
                                            ops.BuyCurrency_Name = Rooms.BuyCurrency_Name;

                                            PostionBookingRooms.Add(ops);
                                        }

                                        foreach (var RoomsNew in NewListForSuppliments)
                                        {
                                            OpsPositionRoomPrice ops = new OpsPositionRoomPrice();
                                            ops.BookingRooms_Id = RoomsNew.BookingRooms_Id;
                                            ops.Booking_Id = RoomsNew.Booking_Id;
                                            ops.PositionPricing_Id = RoomsNew.PositionPricing_Id;
                                            ops.Position_Id = RoomsNew.Position_Id;
                                            ops.PersonType = RoomsNew.PersonType;
                                            ops.PersonTypeID = RoomsNew.PersonType_Id;
                                            var CategoryId = pos?.BookingRoomsAndPrices?.FirstOrDefault()?.Category_Id;
                                            if (CategoryId != null && RoomsNew.ProductRange_Id != null)
                                            {
                                                productCategories = product?.ProductCategories?.Where(x => x.DefProductCategory_Id == CategoryId).FirstOrDefault();
                                                var productrange = productCategories?.ProductRanges.Where(x => x.ProductRange_Id == RoomsNew.ProductRange_Id)?.FirstOrDefault()?.ProductTemplateCode;
                                                if (productrange != null)
                                                {
                                                    ops.ProductRange = productrange;
                                                }
                                            }

                                            ops.BudgetPrice = RoomsNew.BudgetPrice;
                                            if (RoomsNew.ConfirmedReqPrice != null && RoomsNew.ConfirmedReqPrice.HasValue)
                                                ops.ConfirmedReqPrice = RoomsNew.ConfirmedReqPrice.Value;
                                            ops.RequestedPrice = RoomsNew.RequestedPrice;
                                            ops.BuyPrice = RoomsNew.BuyPrice;

                                            ops.ProductRangeID = RoomsNew.ProductRange_Id;
                                            if (RoomsNew.ApplyMarkup != null && RoomsNew.ApplyMarkup.HasValue)
                                                ops.ApplyMarkup = RoomsNew.ApplyMarkup.Value;
                                            if (RoomsNew.ExcludeFromInvoice != null && RoomsNew.ExcludeFromInvoice.HasValue)
                                                ops.ExcludeFromInvoice = RoomsNew.ExcludeFromInvoice.Value;
                                            ops.Req_Count = RoomsNew.Req_Count;
                                            ops.Age = RoomsNew.Age;
                                            ops.Status = RoomsNew.Status;
                                            ops.ChargeBasis = RoomsNew.ChargeBasis;
                                            ops.OnReqQty = RoomsNew.Req_Count;
                                            ops.CategoryName = RoomsNew.CategoryName;
                                            ops.IsAdditionalYN = RoomsNew.IsAdditionalYN;
                                            ops.BuyCurrency_Name = RoomsNew.BuyCurrency_Name;
                                            PostionBookingRoomsForSupplements.Add(ops);
                                        }


                                    }

                                }

                                if (Charge == "PP")
                                {
                                    BookingRoomsAndPrices removepaxfromui = new BookingRoomsAndPrices();
                                    product = _MongoContext.Products.AsQueryable().Where(a => a.VoyagerProduct_Id == pos.Product_Id).FirstOrDefault();
                                    if (keyValueForBookingPax != null)
                                    {
                                        BookingDetailsPax = JsonConvert.DeserializeObject<List<OpsBookingPaxDetails>>(keyValueForBookingPax.Value.ToString());
                                        BookingDetailsPaxForRemoved = BookingDetailsPax.Where(x => x.PassengerQty == 0).ToList();
                                        BookingDetailsPax = BookingDetailsPax.Where(x => x.PassengerQty != 0).ToList();
                                        BookingRoomsAndPrices = booking.Positions.Where(x => x.Position_Id == pos.Position_Id)?.FirstOrDefault()?.BookingRoomsAndPrices;
                                        foreach (var bpax in BookingDetailsPax)
                                        {
                                            //var fetchedPosDetails = booking.Positions.Where(x => x.Position_Id == pos.Position_Id).FirstOrDefault();
                                            if (bpax?.PassengerType?.ToUpper() == "ADULT")
                                            {
                                                bookingRoomAndPrices = BookingRoomsAndPrices.Where(x => x.PersonType?.ToUpper() == bpax?.PassengerType?.ToUpper()).FirstOrDefault();
                                            }
                                            else
                                            {
                                                if (bpax?.PassengerType?.ToUpper() == "CHILD + BED" || bpax?.PassengerType?.ToUpper() == "CHILD - BED" || bpax?.PassengerType?.ToUpper() == "INFANT" || bpax?.PassengerType?.ToUpper() == "DRIVER" || bpax?.PassengerType?.ToUpper() == "GUIDE")
                                                {
                                                    if (bpax?.PassengerType?.ToUpper() == "CHILD + BED")
                                                    {
                                                        bookingRoomAndPrices = BookingRoomsAndPrices.Where(x => x.PersonType?.ToUpper() == bpax?.PassengerType?.ToUpper() && x.Age != null && x.Age.Value == Convert.ToInt32(bpax?.PassengerAge)).FirstOrDefault();
                                                        if (bookingRoomAndPrices == null)
                                                        {
                                                            bookingRoomAndPrices = BookingRoomsAndPrices.Where(x => x.PersonType?.ToUpper() == "CHILD" && x.Age != null && x.Age.Value == Convert.ToInt32(bpax?.PassengerAge)).FirstOrDefault();
                                                        }
                                                    }
                                                    if (bpax?.PassengerType?.ToUpper() == "CHILD - BED")
                                                    {
                                                        bookingRoomAndPrices = BookingRoomsAndPrices.Where(x => x.PersonType?.ToUpper() == bpax?.PassengerType?.ToUpper() && x.Age != null && x.Age.Value == Convert.ToInt32(bpax?.PassengerAge)).FirstOrDefault();
                                                        if (bookingRoomAndPrices == null)
                                                        {
                                                            bookingRoomAndPrices = BookingRoomsAndPrices.Where(x => x.PersonType?.ToUpper() == "CHILD" && x.Age != null && x.Age.Value == Convert.ToInt32(bpax?.PassengerAge)).FirstOrDefault();
                                                        }
                                                    }
                                                    if (bpax?.PassengerType?.ToUpper() == "INFANT")
                                                    {
                                                        bookingRoomAndPrices = BookingRoomsAndPrices.Where(x => x.PersonType?.ToUpper() == bpax?.PassengerType?.ToUpper() && x.Age != null && x.Age.Value == Convert.ToInt32(bpax?.PassengerAge)).FirstOrDefault();
                                                    }
                                                    if (bpax?.PassengerType?.ToUpper() == "DRIVER")
                                                    {
                                                        bookingRoomAndPrices = BookingRoomsAndPrices.Where(x => x.PersonType?.ToUpper() == bpax?.PassengerType?.ToUpper()).FirstOrDefault();
                                                    }
                                                    if (bpax?.PassengerType?.ToUpper() == "GUIDE")
                                                    {
                                                        bookingRoomAndPrices = BookingRoomsAndPrices.Where(x => x.PersonType?.ToUpper() == bpax?.PassengerType?.ToUpper()).FirstOrDefault();
                                                    }

                                                }

                                            }
                                            if (bookingRoomAndPrices != null)
                                            {
                                                if (bpax.PassengerQty.HasValue && bpax.PassengerQty.Value > 0)
                                                {

                                                    bookingRoomAndPrices.Status = null;
                                                    bookingRoomAndPrices.Req_Count = bpax?.PassengerQty;
                                                }

                                            }
                                            else
                                            {
                                                if (bpax.PassengerQty.HasValue && bpax.PassengerQty.Value > 0)
                                                {
                                                    BookingRoomsAndPrices bookingpriceroom = new BookingRoomsAndPrices();
                                                    bookingpriceroom.Booking_Id = booking.Booking_Id;
                                                    var CategoryId = pos?.BookingRoomsAndPrices?.FirstOrDefault()?.Category_Id;
                                                    if (CategoryId != null)
                                                    {
                                                        productCategories = product.ProductCategories.Where(x => x.DefProductCategory_Id == CategoryId).FirstOrDefault();
                                                    }
                                                    if (bpax?.PassengerType?.ToUpper() == "ADULT")
                                                    {
                                                        bookingpriceroom.ProductRange_Id = productCategories?.ProductRanges?.Where(x => x.PersonType?.ToUpper() == "ADULT").FirstOrDefault()?.ProductRange_Id;

                                                    }
                                                    else
                                                    {
                                                        if (bpax?.PassengerType.Trim() == "CHILD + BED")
                                                        {

                                                            var ChildWithBedProduct = productCategories?.ProductRanges?.Where(x => x.PersonType != null && (x.PersonType == "Child + Bed"));
                                                            foreach (var p in ChildWithBedProduct)
                                                            {
                                                                if (p.Agemin != null && p.Agemax != null && !string.IsNullOrEmpty(bpax?.PassengerAge))
                                                                {
                                                                    var MinAge = Convert.ToInt32(p.Agemin);
                                                                    var MaxAge = Convert.ToInt32(p.Agemax);
                                                                    var passAge = Convert.ToInt32(bpax?.PassengerAge);
                                                                    if (bookingpriceroom.ProductRange_Id == null)
                                                                        bookingpriceroom.ProductRange_Id = MinAge <= passAge && passAge <= MaxAge ? p?.ProductRange_Id : null;
                                                                }

                                                            }
                                                            if (bookingpriceroom.ProductRange_Id == null)
                                                            {
                                                                var AlternatePersonTypeInMdefpersontype = _MongoContext.mDefPersonType.AsQueryable().Where(x => x.PersonType == _configuration.GetValue<string>("GetRoomTypes:CHILDWITHBED"))?.FirstOrDefault()?.Alaernate_PersonType;
                                                                var FoundProduct = productCategories?.ProductRanges?.Where(x => x.PersonType != null && AlternatePersonTypeInMdefpersontype != null && x.PersonType == AlternatePersonTypeInMdefpersontype);
                                                                foreach (var p in FoundProduct)
                                                                {
                                                                    if (p.Agemin != null && p.Agemax != null && !string.IsNullOrEmpty(bpax?.PassengerAge))
                                                                    {
                                                                        var AgeMin = Convert.ToInt32(p.Agemin);
                                                                        var AgeMax = Convert.ToInt32(p.Agemax);
                                                                        var passAge = Convert.ToInt32(bpax?.PassengerAge);
                                                                        if (bookingpriceroom.ProductRange_Id == null)
                                                                            bookingpriceroom.ProductRange_Id = AgeMin <= passAge && passAge <= AgeMax ? p?.ProductRange_Id : null;
                                                                    }

                                                                }

                                                            }
                                                            if (bookingpriceroom.ProductRange_Id == null)
                                                            {
                                                                var AlternatePersonTypeInMdefpersontype = _MongoContext.mDefPersonType.AsQueryable().Where(x => x.PersonType == _configuration.GetValue<string>("GetRoomTypes:CHILDWITHBED"))?.FirstOrDefault()?.Alaernate_PersonType;
                                                                bookingpriceroom.ProductRange_Id = productCategories?.ProductRanges?.Where(x => x.PersonType != null && AlternatePersonTypeInMdefpersontype != null && x.PersonType == AlternatePersonTypeInMdefpersontype).FirstOrDefault()?.ProductRange_Id;

                                                            }
                                                            if (bookingpriceroom.ProductRange_Id != null)
                                                            {
                                                                var IsYnforCHILDWITHBED = productCategories.ProductRanges.Where(x => x.ProductRange_Id == bookingpriceroom.ProductRange_Id)?.FirstOrDefault()?.AdditionalYn;
                                                                if (IsYnforCHILDWITHBED != null && IsYnforCHILDWITHBED.HasValue)
                                                                {
                                                                    bookingpriceroom.IsAdditionalYN = IsYnforCHILDWITHBED.Value;
                                                                }
                                                            }

                                                        }
                                                        if (bpax?.PassengerType.Trim() == "CHILD - BED")
                                                        {
                                                            var ChildWithOutBedProduct = productCategories?.ProductRanges?.Where(x => x.PersonType != null && (x.PersonType == "Child - Bed"));
                                                            foreach (var p in ChildWithOutBedProduct)
                                                            {
                                                                if (p.Agemin != null && p.Agemax != null && !string.IsNullOrEmpty(bpax?.PassengerAge))
                                                                {
                                                                    var MinAge = Convert.ToInt32(p.Agemin);
                                                                    var MaxAge = Convert.ToInt32(p.Agemax);
                                                                    var passAge = Convert.ToInt32(bpax?.PassengerAge);
                                                                    if (bookingpriceroom.ProductRange_Id == null)
                                                                        bookingpriceroom.ProductRange_Id = MinAge <= passAge && passAge <= MaxAge ? p?.ProductRange_Id : null;
                                                                }

                                                            }
                                                            if (bookingpriceroom.ProductRange_Id == null)
                                                            {
                                                                var AlternatePersonTypeInMdefpersontype = _MongoContext.mDefPersonType.AsQueryable().Where(x => x.PersonType == _configuration.GetValue<string>("GetRoomTypes:CHILDWITHOUTBED"))?.FirstOrDefault()?.Alaernate_PersonType;
                                                                var FoundProduct = productCategories?.ProductRanges?.Where(x => x.PersonType != null && AlternatePersonTypeInMdefpersontype != null && x.PersonType == AlternatePersonTypeInMdefpersontype);
                                                                foreach (var p in FoundProduct)
                                                                {
                                                                    if (p.Agemin != null && p.Agemax != null && !string.IsNullOrEmpty(bpax?.PassengerAge))
                                                                    {
                                                                        var AgeMin = Convert.ToInt32(p.Agemin);
                                                                        var AgeMax = Convert.ToInt32(p.Agemax);
                                                                        var passAge = Convert.ToInt32(bpax?.PassengerAge);
                                                                        if (bookingpriceroom.ProductRange_Id == null)
                                                                            bookingpriceroom.ProductRange_Id = AgeMin <= passAge && passAge <= AgeMax ? p?.ProductRange_Id : null;
                                                                    }

                                                                }

                                                            }
                                                            if (bookingpriceroom.ProductRange_Id == null)
                                                            {
                                                                var AlternatePersonTypeInMdefpersontype = _MongoContext.mDefPersonType.AsQueryable().Where(x => x.PersonType == _configuration.GetValue<string>("GetRoomTypes:CHILDWITHOUTBED"))?.FirstOrDefault()?.Alaernate_PersonType;
                                                                bookingpriceroom.ProductRange_Id = productCategories?.ProductRanges?.Where(x => x.PersonType != null && AlternatePersonTypeInMdefpersontype != null && x.PersonType == AlternatePersonTypeInMdefpersontype).FirstOrDefault()?.ProductRange_Id;

                                                            }
                                                            if (bookingpriceroom.ProductRange_Id != null)
                                                            {
                                                                var IsYnforCHILDWITHBED = productCategories.ProductRanges.Where(x => x.ProductRange_Id == bookingpriceroom.ProductRange_Id)?.FirstOrDefault()?.AdditionalYn;
                                                                if (IsYnforCHILDWITHBED != null && IsYnforCHILDWITHBED.HasValue)
                                                                {
                                                                    bookingpriceroom.IsAdditionalYN = IsYnforCHILDWITHBED.Value;
                                                                }
                                                            }
                                                        }

                                                        if (bpax?.PassengerType.Trim() == "INFANT")
                                                        {
                                                            bookingpriceroom.ProductRange_Id = productCategories?.ProductRanges?.Where(x => x.PersonType != null && x.PersonType == bpax?.PassengerType.Trim()).FirstOrDefault()?.ProductRange_Id;
                                                            if (bookingpriceroom.ProductRange_Id != null)
                                                            {
                                                                var IsYnforINFANT = productCategories?.ProductRanges?.Where(x => x.ProductRange_Id == bookingpriceroom.ProductRange_Id)?.FirstOrDefault()?.AdditionalYn;
                                                                if (IsYnforINFANT != null && IsYnforINFANT.HasValue)
                                                                {
                                                                    bookingpriceroom.IsAdditionalYN = IsYnforINFANT.Value;
                                                                }
                                                            }
                                                        }
                                                        if (bpax?.PassengerType.Trim() == "DRIVER")
                                                        {
                                                            bookingpriceroom.ProductRange_Id = productCategories?.ProductRanges?.Where(x => x.PersonType != null && x.PersonType == bpax?.PassengerType.Trim()).FirstOrDefault()?.ProductRange_Id;

                                                        }
                                                        if (bpax?.PassengerType.Trim() == "GUIDE")
                                                        {
                                                            bookingpriceroom.ProductRange_Id = productCategories?.ProductRanges?.Where(x => x.PersonType != null && x.PersonType == bpax?.PassengerType.Trim()).FirstOrDefault()?.ProductRange_Id;

                                                        }
                                                    }
                                                    bookingpriceroom.CategoryName = product?.ProductCategories?.Where(x => x.DefProductCategory_Id == CategoryId)?.FirstOrDefault()?.ProductCategoryName;
                                                    bookingpriceroom.BookingRooms_Id = Guid.NewGuid().ToString();
                                                    bookingpriceroom.PositionPricing_Id = Guid.NewGuid().ToString();
                                                    bookingpriceroom.BudgetPrice = 0.00M;
                                                    bookingpriceroom.RequestedPrice = 0.00M;
                                                    bookingpriceroom.BuyPrice = 0.00M;
                                                    bookingpriceroom.ChargeBasis = _MongoContext.mProductType.AsQueryable().Where(x => x.VoyagerProductType_Id == pos.ProductType_Id)?.FirstOrDefault()?.ChargeBasis;
                                                    bookingpriceroom.Req_Count = bpax.PassengerQty;
                                                    bookingpriceroom.BuyCurrency_Name = BookingRoomsAndPrices.Where(x => x.BuyCurrency_Name != null && x.Status != "X")?.FirstOrDefault()?.BuyCurrency_Name;
                                                    if (bpax?.PassengerType.ToUpper() != "ADULT")
                                                    {
                                                        var productTypeFromProductRanges = productCategories?.ProductRanges.Where(x => x.ProductRange_Id == bookingpriceroom.ProductRange_Id)?.FirstOrDefault();
                                                        if (productTypeFromProductRanges != null)
                                                        {
                                                            var ifPersonTypeExist = _MongoContext.mDefPersonType.AsQueryable().Where(a => a.defPersonType_Id == productTypeFromProductRanges.PersonType_Id)?.FirstOrDefault();
                                                            if (ifPersonTypeExist != null)
                                                            {
                                                                bookingpriceroom.PersonType_Id = ifPersonTypeExist.defPersonType_Id;
                                                                bookingpriceroom.PersonType = ifPersonTypeExist.PersonType;
                                                            }
                                                            else
                                                            {
                                                                var alternatepersontype = _MongoContext.mDefPersonType.AsQueryable().Where(a => a.Alaernate_PersonType == productTypeFromProductRanges.PersonType_Id)?.FirstOrDefault();
                                                                if (alternatepersontype != null)
                                                                {
                                                                    bookingpriceroom.PersonType_Id = alternatepersontype.AlternateType_Id;
                                                                    bookingpriceroom.PersonType = alternatepersontype.Alaernate_PersonType;

                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var productTypeFromProductRangesForAdult = productCategories?.ProductRanges?.Where(x => x.ProductRange_Id == bookingpriceroom.ProductRange_Id)?.FirstOrDefault();
                                                        if (productTypeFromProductRangesForAdult != null)
                                                        {
                                                            var ifPersonTypeExist = _MongoContext.mDefPersonType.AsQueryable().Where(a => a.defPersonType_Id == productTypeFromProductRangesForAdult.PersonType_Id)?.FirstOrDefault();
                                                            if (ifPersonTypeExist != null)
                                                            {
                                                                bookingpriceroom.PersonType_Id = ifPersonTypeExist.defPersonType_Id;
                                                                bookingpriceroom.PersonType = ifPersonTypeExist.PersonType;
                                                            }
                                                            else
                                                            {
                                                                var alternatepersonTypeid = _MongoContext.mDefPersonType.AsQueryable().Where(a => a.Alaernate_PersonType == productTypeFromProductRangesForAdult.PersonType_Id)?.FirstOrDefault();
                                                                if (alternatepersonTypeid != null)
                                                                {
                                                                    bookingpriceroom.PersonType_Id = alternatepersonTypeid.AlternateType_Id;
                                                                    bookingpriceroom.PersonType = alternatepersonTypeid.Alaernate_PersonType;
                                                                }

                                                            }
                                                        }
                                                        //bookingpriceroom.PersonType_Id = _MongoContext.mDefPersonType.AsQueryable().Where(a => a.PersonType.ToUpper() == "ADULT")?.FirstOrDefault()?.defPersonType_Id;
                                                        //bookingpriceroom.PersonType = "ADULT";
                                                    }
                                                    bookingpriceroom.ApplyMarkup = true;
                                                    bookingpriceroom.ExcludeFromInvoice = false;
                                                    bookingpriceroom.ConfirmedReqPrice = false;
                                                    bookingpriceroom.Req_Count = bpax.PassengerQty;
                                                    if (!string.IsNullOrEmpty(bpax.PassengerAge))
                                                    {
                                                        bookingpriceroom.Age = Convert.ToInt32(bpax.PassengerAge);
                                                    }
                                                    bookingpriceroom.Status = null;
                                                    if (bpax?.PassengerType.Trim() == "CHILD + BED" || bpax?.PassengerType.Trim() == "CHILD - BED" || bpax?.PassengerType.Trim() == "INFANT")
                                                    {
                                                        if (bookingpriceroom.IsAdditionalYN)
                                                        {
                                                            if (bookingpriceroom.ProductRange_Id != null)
                                                            {
                                                                NewListForSuppliments.Add(bookingpriceroom);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (bookingpriceroom.ProductRange_Id != null)
                                                            {
                                                                BookingRoomsAndPrices.Add(bookingpriceroom);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (bookingpriceroom.ProductRange_Id != null)
                                                        {
                                                            BookingRoomsAndPrices.Add(bookingpriceroom);
                                                        }
                                                    }


                                                }
                                            }

                                        }
                                        // logic to remove deleted from pax

                                        foreach (var removefrompax in BookingDetailsPaxForRemoved)
                                        {
                                            if (removefrompax?.PassengerType?.ToUpper() == "CHILD + BED")
                                            {
                                                removepaxfromui = BookingRoomsAndPrices.Where(x => x.PersonType == _configuration.GetValue<string>("GetRoomTypes:CHILDWITHBED") && removefrompax.PassengerAge != null && x.Age != null && x.Age.Value == Convert.ToInt32(removefrompax.PassengerAge) && x.Status != "X" && x.Status != "-")?.FirstOrDefault();
                                                if (removepaxfromui == null)
                                                {
                                                    if (!String.IsNullOrEmpty(removefrompax.PassengerAge))
                                                    {
                                                        removepaxfromui = BookingRoomsAndPrices.Where(x => x.PersonType == "CHILD" && removefrompax.PassengerAge != null && x.Age != null && x.Age.Value == Convert.ToInt32(removefrompax.PassengerAge) && x.Status != "X" && x.Status != "-")?.FirstOrDefault();
                                                    }
                                                    else
                                                    {

                                                        removepaxfromui = BookingRoomsAndPrices.Where(x => x.PersonType == "CHILD" && x.Status != "X" && x.Status != "-")?.FirstOrDefault();
                                                    }

                                                }
                                                if (removepaxfromui != null)
                                                {
                                                    removepaxfromui.Status = "X";
                                                }
                                            }
                                            if (removefrompax?.PassengerType?.ToUpper() == "CHILD - BED")
                                            {
                                                removepaxfromui = BookingRoomsAndPrices.Where(x => x.PersonType == _configuration.GetValue<string>("GetRoomTypes:CHILDWITHOUTBED") && x.Age != null && x.Age.Value == Convert.ToInt32(removefrompax.PassengerAge) && x.Status != "X" && x.Status != "-")?.FirstOrDefault();
                                                if (removepaxfromui == null)
                                                {
                                                    if (!String.IsNullOrEmpty(removefrompax.PassengerAge))
                                                    {
                                                        removepaxfromui = BookingRoomsAndPrices.Where(x => x.PersonType == "CHILD" && removefrompax.PassengerAge != null && x.Age != null && x.Age.Value == Convert.ToInt32(removefrompax.PassengerAge) && x.Status != "X" && x.Status != "-")?.FirstOrDefault();
                                                    }
                                                    else
                                                    {

                                                        removepaxfromui = BookingRoomsAndPrices.Where(x => x.PersonType == "CHILD" && x.Status != "X" && x.Status != "-")?.FirstOrDefault();
                                                    }

                                                }
                                                if (removepaxfromui != null)
                                                {
                                                    removepaxfromui.Status = "X";
                                                }
                                            }
                                            if (removefrompax?.PassengerType?.ToUpper() == "INFANT")
                                            {
                                                removepaxfromui = BookingRoomsAndPrices.Where(x => x.PersonType == _configuration.GetValue<string>("GetRoomTypes:INFANT") && x.Age != null && x.Age.Value == Convert.ToInt32(removefrompax.PassengerAge) && x.Status != "X" && x.Status != "-")?.FirstOrDefault();
                                            }
                                            if (removefrompax?.PassengerType?.ToUpper() == "ADULT")
                                            {
                                                removepaxfromui = BookingRoomsAndPrices.Where(x => x.PersonType == "ADULT" && x.Status != "X" && x.Status != "-").FirstOrDefault();
                                            }

                                        }
                                        //Converting in Req format
                                        foreach (var Rooms in BookingRoomsAndPrices)
                                        {
                                            OpsPositionRoomPrice ops = new OpsPositionRoomPrice();
                                            ops.BookingRooms_Id = Rooms.BookingRooms_Id;
                                            ops.Booking_Id = Rooms.Booking_Id;
                                            ops.PositionPricing_Id = Rooms.PositionPricing_Id;
                                            ops.Position_Id = Rooms.Position_Id;
                                            ops.PersonType = Rooms.PersonType;
                                            ops.PersonTypeID = Rooms.PersonType_Id;
                                            var CategoryId = pos?.BookingRoomsAndPrices?.FirstOrDefault()?.Category_Id;
                                            if (CategoryId != null && Rooms.ProductRange_Id != null)
                                            {
                                                productCategories = product.ProductCategories.Where(x => x.DefProductCategory_Id == CategoryId).FirstOrDefault();
                                                var productrange = productCategories?.ProductRanges.Where(x => x.ProductRange_Id == Rooms.ProductRange_Id)?.FirstOrDefault()?.ProductTemplateCode;
                                                if (productrange != null)
                                                {
                                                    ops.ProductRange = productrange;
                                                }
                                            }

                                            ops.BudgetPrice = Rooms.BudgetPrice;
                                            if (Rooms.ConfirmedReqPrice != null && Rooms.ConfirmedReqPrice.HasValue)
                                                ops.ConfirmedReqPrice = Rooms.ConfirmedReqPrice.Value;
                                            ops.RequestedPrice = Rooms.RequestedPrice;
                                            ops.BuyPrice = Rooms.BuyPrice;

                                            ops.ProductRangeID = Rooms.ProductRange_Id;
                                            if (Rooms.ApplyMarkup != null && Rooms.ApplyMarkup.HasValue)
                                                ops.ApplyMarkup = Rooms.ApplyMarkup.Value;
                                            if (Rooms.ExcludeFromInvoice != null && Rooms.ExcludeFromInvoice.HasValue)
                                                ops.ExcludeFromInvoice = Rooms.ExcludeFromInvoice.Value;
                                            ops.Req_Count = Rooms.Req_Count;
                                            ops.Age = Rooms.Age;
                                            ops.Status = Rooms.Status;
                                            ops.ChargeBasis = Rooms.ChargeBasis;
                                            ops.OnReqQty = Rooms.Req_Count;
                                            ops.CategoryName = Rooms.CategoryName;
                                            ops.IsAdditionalYN = Rooms.IsAdditionalYN;
                                            ops.BuyCurrency_Name = Rooms.BuyCurrency_Name;
                                            PostionBookingRooms.Add(ops);
                                        }

                                        foreach (var RoomsNew in NewListForSuppliments)
                                        {
                                            OpsPositionRoomPrice ops = new OpsPositionRoomPrice();
                                            ops.BookingRooms_Id = RoomsNew.BookingRooms_Id;
                                            ops.Booking_Id = RoomsNew.Booking_Id;
                                            ops.PositionPricing_Id = RoomsNew.PositionPricing_Id;
                                            ops.Position_Id = RoomsNew.Position_Id;
                                            ops.PersonType = RoomsNew.PersonType;
                                            ops.PersonTypeID = RoomsNew.PersonType_Id;
                                            var CategoryId = pos?.BookingRoomsAndPrices?.FirstOrDefault()?.Category_Id;
                                            if (CategoryId != null && RoomsNew.ProductRange_Id != null)
                                            {
                                                productCategories = product.ProductCategories.Where(x => x.DefProductCategory_Id == CategoryId).FirstOrDefault();
                                                var productrange = productCategories?.ProductRanges.Where(x => x.ProductRange_Id == RoomsNew.ProductRange_Id)?.FirstOrDefault()?.ProductTemplateCode;
                                                if (productrange != null)
                                                {
                                                    ops.ProductRange = productrange;
                                                }
                                            }

                                            ops.BudgetPrice = RoomsNew.BudgetPrice;
                                            if (RoomsNew.ConfirmedReqPrice != null && RoomsNew.ConfirmedReqPrice.HasValue)
                                                ops.ConfirmedReqPrice = RoomsNew.ConfirmedReqPrice.Value;
                                            ops.RequestedPrice = RoomsNew.RequestedPrice;
                                            ops.BuyPrice = RoomsNew.BuyPrice;

                                            ops.ProductRangeID = RoomsNew.ProductRange_Id;
                                            if (RoomsNew.ApplyMarkup != null && RoomsNew.ApplyMarkup.HasValue)
                                                ops.ApplyMarkup = RoomsNew.ApplyMarkup.Value;
                                            if (RoomsNew.ExcludeFromInvoice != null && RoomsNew.ExcludeFromInvoice.HasValue)
                                                ops.ExcludeFromInvoice = RoomsNew.ExcludeFromInvoice.Value;
                                            ops.Req_Count = RoomsNew.Req_Count;
                                            ops.Age = RoomsNew.Age;
                                            ops.Status = RoomsNew.Status;
                                            ops.ChargeBasis = RoomsNew.ChargeBasis;
                                            ops.OnReqQty = RoomsNew.Req_Count;
                                            ops.CategoryName = RoomsNew.CategoryName;
                                            ops.IsAdditionalYN = RoomsNew.IsAdditionalYN;
                                            ops.BuyCurrency_Name = RoomsNew.BuyCurrency_Name;
                                            PostionBookingRoomsForSupplements.Add(ops);
                                        }


                                    }

                                }
                                if (Charge == "PRPN" || Charge == "PP")
                                {
                                    ExecuteWorkflowActionReq request1 = new ExecuteWorkflowActionReq();
                                    request1.OpsKeyValue = new List<OpsKeyValue>();
                                    request1.PositionIds = new List<string>();
                                    request1.Module = request.Module;
                                    string posid = pos.Position_Id.ToString();
                                    request1.PositionIds.Add(posid);
                                    request1.UserEmailId = request.UserEmailId;
                                    request1.UserId = request.UserId;
                                    request1.UserName = request.UserName;
                                    // request1.OpsKeyValue = request.OpsKeyValue;
                                    var Serializeddata = JsonConvert.SerializeObject(PostionBookingRooms);
                                    var SerializedDataForSupplements = JsonConvert.SerializeObject(PostionBookingRoomsForSupplements);
                                    request1.OpsKeyValue.Add(new OpsKeyValue { Key = "TableRoomsAndRates", Value = Serializeddata });
                                    if (SerializedDataForSupplements != null && SerializedDataForSupplements.Any())
                                    {
                                        request1.OpsKeyValue.Add(new OpsKeyValue { Key = "TableAdditionalSuppliments", Value = SerializedDataForSupplements });
                                    }
                                    request1.OpsKeyValue.Add(new OpsKeyValue { Key = "saveposition", Value = "saveposition" });
                                    response = this.UpdateBookingRoomsAndPrices(ref booking, request1);
                                    if (response.Status == "Failure")
                                    {
                                        response.ErrorMessage.Add("Unable To Update Booking  Rooms and Prices");
                                        response.Status = "Failure";
                                        break;
                                    }
                                    else
                                    {
                                        request1.OpsKeyValue.Add(new OpsKeyValue { Key = "PaxType", Value = "true" });
                                        response = this.UpdatePositionKeyDetails(ref booking, request1);
                                        if (response.Status == "Success")
                                        {
                                            request1.OpsKeyValue.Add(new OpsKeyValue { Key = "Status", Value = "M" });
                                            if (pos.STATUS == "K" || pos.STATUS == "B" || pos.STATUS == "I")
                                            {
                                                response = this.UpdatePositionKeyDetails(ref booking, request1);

                                                if (response.Status == "Failure")
                                                {
                                                    response.ErrorMessage.Add("Unable To Update Key PostionDetail Status for Position");
                                                    response.Status = "Failure";
                                                    break;
                                                }

                                            }
                                        }
                                        else
                                        {
                                            response.ErrorMessage.Add("Unable To Update position key StandardRooming");
                                            response.Status = "Failure";
                                            break;
                                        }
                                    }
                                }
                            }
                            if (response.Status == "Success")
                            {
                                if (request.PositionIds != null)
                                {
                                    GetDocTypeByWorkflowReq requestforDoc = new GetDocTypeByWorkflowReq();
                                    requestforDoc.Module = request.Module;
                                    requestforDoc.ModuleParent = request.Module;
                                    requestforDoc.Action = "changeroomcount";
                                    var DocType = this.GetDocTypeByWorkflow(requestforDoc);
                                    if (DocType != null)
                                    {
                                        ExecuteWorkflowActionReq requestforSendEmail = new ExecuteWorkflowActionReq();
                                        requestforSendEmail.IsSendEmail = request.IsSendEmail;
                                        requestforSendEmail.UserEmailId = request.UserEmailId;
                                        requestforSendEmail.UserId = request.UserId;
                                        requestforSendEmail.UserName = request.UserName;
                                        requestforSendEmail.PositionIds = new List<string>();
                                        requestforSendEmail.PositionIds = request.PositionIds;
                                        requestforSendEmail.DocType = DocType.DocType;
                                        request.DocType = DocType.DocType;
                                        var EmailResult = this.GenerateDocAndUpdCommsLog(ref booking, requestforSendEmail);
                                        if (response.Status == "Failure")
                                        {
                                            response.ErrorMessage.Add("Unable To Send Mail Related to Changes in BookingRooms and Prices");
                                            response.Status = "Failure";

                                        }




                                    }
                                }

                            }

                        }
                    }

                }
            }
            catch (Exception e)
            {
                response.Status = "Failure";
                response.ErrorMessage.Add("UpdateBookingRoomsAndPrices:- " + e.Message);

            }
            return response;
        }

        public OPSWorkflowResponseStatus AddPosition(ref Bookings booking, ExecuteWorkflowActionReq request)
        {
            OPSWorkflowResponseStatus response = new OPSWorkflowResponseStatus() { Status = "Success" };
            try
            {
                BookingRoomsAndPrices PosPrice;
                if (booking != null && request.OpsKeyValue?.Count > 0)
                {
                    var KeyValueList = request.OpsKeyValue;
                    var keyValue = new OpsKeyValue();

                    #region Add Position
                    keyValue = KeyValueList.Where(a => a.Key == "TableAddPosition").FirstOrDefault();
                    if (keyValue != null && Convert.ToBoolean(keyValue.Value) == true)
                    {
                        if (!string.IsNullOrEmpty(request.PositionIds[0]))
                        {
                            var position = new Positions();
                            position.Position_Id = request.PositionIds[0];

                            keyValue = KeyValueList.Where(a => a.Key == "PositionType").FirstOrDefault();
                            if (keyValue != null)
                                position.PositionType = Convert.ToString(keyValue.Value);

                            keyValue = KeyValueList.Where(a => a.Key == "PositionStartDate").FirstOrDefault();
                            if (keyValue != null)
                            {
                                position.STARTDATE = _genericRepository.ConvertStringToDateTime(Convert.ToString(keyValue.Value));
                            }

                            keyValue = KeyValueList.Where(a => a.Key == "NoOfDays").FirstOrDefault();
                            if (keyValue != null)
                                position.DURATION = Convert.ToString(keyValue.Value);

                            keyValue = KeyValueList.Where(a => a.Key == "StartTime").FirstOrDefault();
                            if (keyValue != null)
                                position.STARTTIME = Convert.ToString(keyValue.Value);

                            keyValue = KeyValueList.Where(a => a.Key == "EndTime").FirstOrDefault();
                            if (keyValue != null)
                                position.ENDTIME = Convert.ToString(keyValue.Value);

                            if (position.PositionType?.ToLower() == "core")
                                position.StandardRooming = true;
                            else
                                position.StandardRooming = false;

                            var MaxOrderPos = booking.Positions.OrderByDescending(a => a.OrderNr).FirstOrDefault();
                            if (MaxOrderPos != null) position.OrderNr = Convert.ToString(Convert.ToInt32(MaxOrderPos.OrderNr) + 10);
                            else position.OrderNr = "10";

                            position.STATUS = "E";
                            position.InvoiceStatus = "T";
                            position.HOTELMEALPLAN = "BB";
                            position.WashChangeRoom = 0;
                            position.NumberOfDriverRooms = 0;

                            position.IsLocked = false;
                            position.IsSendToHotel = false;
                            position.Special_Requests = "";
                            position.Porterage = false;
                            position.MealsIncluded = false;
                            position.TicketsIncluded = false;
                            position.Parking = false;
                            position.CityPermits = false;
                            position.RoadTolls = false;
                            position.AC = false;
                            position.WC = false;
                            position.GPS = false;
                            position.AV = false;
                            position.CoachParkingAvailable = false;
                            position.Tea = false;
                            position.Dessert = false;
                            position.Water = false;
                            position.Bread = false;

                            keyValue = KeyValueList.Where(a => a.Key == "ProductId").FirstOrDefault();
                            if (keyValue != null && !string.IsNullOrEmpty(Convert.ToString(keyValue.Value)))
                            {
                                var Product = _MongoContext.Products.AsQueryable().Where(a => a.VoyagerProduct_Id == Convert.ToString(keyValue.Value)).FirstOrDefault();
                                if (Product != null)
                                {
                                    position.Product_Id = Product.VoyagerProduct_Id;
                                    position.ProductType_Id = Product.ProductType_Id;
                                    position.ProductType = Product.ProductType;
                                    position.ProductCode = Product.ProductCode;
                                    position.Product_Name = Product.ProductName;
                                    position.Country_Id = Product.CountryId;
                                    position.Country = Product.CountryName;
                                    position.City_Id = Product.CityId;
                                    position.City = Product.CityName;
                                    position.GRIDINFO = string.Format("{0} {1} {2} - {3} {4}", position.OrderNr, position.ProductType, position.STARTDATE?.ToString("dd/MM/yy"), position.ENDDATE?.ToString("dd/MM/yy"), Product.ProductName);
                                    position.Attributes = Product.HotelAdditionalInfo;
                                    position.HotelStarRating = Convert.ToInt16(Product.HotelAdditionalInfo?.StarRating?.Substring(0, 1));

                                    if (position.ProductType?.ToLower() == "hotel" || position.ProductType?.ToLower() == "apartments" || position.ProductType?.ToLower() == "overnight ferry")
                                    {
                                        position.ENDDATE = Convert.ToDateTime(position.STARTDATE).AddDays(Convert.ToDouble(position.DURATION));
                                    }
                                    else
                                    {
                                        position.ENDDATE = Convert.ToDateTime(position.STARTDATE).AddDays(Convert.ToDouble(position.DURATION) - 1);
                                    }

                                    #region SupplierInfo
                                    keyValue = KeyValueList.Where(a => a.Key == "SupplierId").FirstOrDefault();
                                    var SupplierId = Convert.ToString(keyValue?.Value);
                                    if (string.IsNullOrEmpty(SupplierId))
                                    {
                                        SupplierId = Product.ProductSuppliers.Where(a => a.IsDefault == true).FirstOrDefault()?.Company_Id;
                                    }
                                    var Supplier = _MongoContext.mCompanies.AsQueryable().Where(a => a.Company_Id == SupplierId).FirstOrDefault();
                                    var ProdSupplier = Product.ProductSuppliers.Where(a => a.Company_Id == SupplierId).FirstOrDefault();

                                    position.SupplierInfo = new BookingCompanyInfo();
                                    if (Supplier != null)
                                    {
                                        position.SupplierInfo.Id = Supplier.Company_Id;
                                        position.SupplierInfo.Name = Supplier.Name;
                                        position.SupplierInfo.Code = Supplier.CompanyCode;
                                        position.SupplierInfo.ISSUBAGENT = null;
                                        position.SupplierInfo.ParentCompany_Id = null;
                                        position.SupplierInfo.ParentCompany_Name = null;
                                        position.SupplierInfo.Division_ID = null;
                                        position.SupplierInfo.Division_Name = null;

                                        var ProdContact = Supplier.Products.Where(a => a.Product_Id == Product.VoyagerProduct_Id).FirstOrDefault();
                                        if (ProdContact?.Contact_Group_Id != null)
                                        {
                                            position.SupplierInfo.Contact_Id = ProdContact.Contact_Group_Id;
                                            position.SupplierInfo.Contact_Name = ProdContact.Contact_Group_Name;
                                            position.SupplierInfo.Contact_Email = ProdContact.Contact_Group_Email;
                                            position.SupplierInfo.Contact_SendType = ProdContact.ContactVia;
                                        }
                                        else
                                        {
                                            var Contact = Supplier.ContactDetails.Where(a => a.IsOperationDefault).FirstOrDefault();
                                            if (Contact != null)
                                            {
                                                position.SupplierInfo.Contact_Id = Contact.Contact_Id;
                                                position.SupplierInfo.Contact_Name = Contact.FIRSTNAME + " " + Contact.LastNAME;
                                                position.SupplierInfo.Contact_Email = Contact.MAIL;
                                                position.SupplierInfo.Contact_Tel = Contact.TEL;
                                            }
                                        }
                                        position.BuyCurrency_Id = ProdSupplier?.CurrencyId;
                                        position.BuyCurrency_Name = ProdSupplier?.CurrencyName;
                                        position.ExchangeRate_ID = booking.ExchangeRateSnapshot?.ExchangeRateSnapshot_ID;
                                        position.ExchangeRateDetail_ID = booking.ExchangeRateSnapshot?.ExchangeRateDetail.Where(a => a.Currency_Id == position.BuyCurrency_Id).FirstOrDefault()?.ExchangeRateDetailSnapshot_Id;

                                        var rate = _genericRepository.getExchangeRateFromBooking(booking.ExchangeRateSnapshot?.Currency_Id, position.BuyCurrency_Id, booking.BookingNumber);
                                        if (rate != null)
                                            position.ExchangeRate = Convert.ToDecimal(rate.Value);

                                        rate = _genericRepository.getExchangeRateFromBooking(booking.SellCurrency_Id, position.BuyCurrency_Id, booking.BookingNumber);
                                        if (rate != null)
                                            position.ExchangeRateSell = Convert.ToDecimal(rate.Value);

                                        #region Booking Season
                                        position.BookingSeason = new List<BookingSeason>();
                                        position.BookingSeason.Add(new BookingSeason()
                                        {
                                            BookingSeason_ID = Guid.NewGuid().ToString(),
                                            Booking_Id = booking.Booking_Id,
                                            STARTDATE = Convert.ToString(position.STARTDATE),
                                            ENDDATE = Convert.ToString(position.ENDDATE),
                                            Season = "GROUPS Default",
                                            Position_Id = position.Position_Id,
                                            WEEKDAY = "Daily",
                                            PPBusiTypes = "G",
                                            Supplier_Id = position.SupplierInfo.Id
                                        });
                                        #endregion

                                        #region Commercial
                                        position.Commercials = new List<BookingCommercials>();
                                        position.Commercials.Add(new BookingCommercials()
                                        {
                                            Markup_Id = booking.Commercials?.Markup_Id,
                                            Markup_Name = booking.Commercials?.Markup_Name,
                                            MarkupDetail_Id = booking.Commercials?.MarkupDetail_Id,
                                            MarkupPercAmt = booking.Commercials?.MarkupPercAmt,
                                            MarkupCurrency = booking.Commercials?.MarkupCurrency,
                                            MarkupCurrency_Id = booking.Commercials?.MarkupCurrency_Id,
                                        });
                                        #endregion
                                    }

                                    #endregion

                                    #region BookingRoomsAndPrices
                                    position.BookingRoomsAndPrices = new List<BookingRoomsAndPrices>();
                                    var DefMealPlan = _MongoContext.mDefMealPlan.AsQueryable().Where(a => a.MealPlan == position.HOTELMEALPLAN).FirstOrDefault();
                                    var ChargeBasis = _MongoContext.mProductType.AsQueryable().Where(a => a.VoyagerProductType_Id == position.ProductType_Id).FirstOrDefault()?.ChargeBasis;
                                    int? Req_Count = 1;

                                    keyValue = KeyValueList.Where(a => a.Key == "ProductCategoryId").FirstOrDefault();
                                    if (keyValue != null && !string.IsNullOrEmpty(Convert.ToString(keyValue.Value)))
                                    {
                                        var Category = Product.ProductCategories.Where(a => a.ProductCategory_Id == Convert.ToString(keyValue.Value)).FirstOrDefault();
                                        if (Category != null)
                                        {
                                            var ProductContractsBuy = _productRepository.GetProductContracts(new ProductContractsGetReq
                                            {
                                                SupplierId = position.SupplierInfo.Id,
                                                ProductId = position.Product_Id,
                                                BuySellType = "Buy",
                                                AgentId = booking.AgentInfo?.Id
                                            })?.Result?.ProductContract;
                                            var ProductContractsSell = _productRepository.GetProductContracts(new ProductContractsGetReq
                                            {
                                                SupplierId = position.SupplierInfo.Id,
                                                ProductId = position.Product_Id,
                                                BuySellType = "Sell",
                                                AgentId = booking.AgentInfo?.Id
                                            })?.Result?.ProductContract;

                                            if (ProductContractsBuy == null)
                                            {
                                                ProductContractsBuy = _productRepository.GetProductContracts(new ProductContractsGetReq
                                                {
                                                    SupplierId = position.SupplierInfo.Id,
                                                    ProductId = position.Product_Id,
                                                    BuySellType = "Buy"
                                                })?.Result?.ProductContract;
                                            }
                                            if (ProductContractsSell == null)
                                            {
                                                ProductContractsSell = _productRepository.GetProductContracts(new ProductContractsGetReq
                                                {
                                                    SupplierId = position.SupplierInfo.Id,
                                                    ProductId = position.Product_Id,
                                                    BuySellType = "Sell"
                                                })?.Result?.ProductContract;
                                            }

                                            if (ChargeBasis == "PRPN")
                                            {
                                                foreach (var BookingRoom in booking.BookingRooms)
                                                {
                                                    Req_Count = BookingRoom.ROOMNO;
                                                    var Range = Category.ProductRanges.Where(a => a.ProductTemplateCode == BookingRoom.SUBPROD && a.PersonType == "ADULT").FirstOrDefault();
                                                    if (Range != null)
                                                    {
                                                        var BuyContractPrices = ProductContractsBuy?.PricePeriods?.Where(a => a.Period_Start_Date <= position.STARTDATE && a.Period_End_Date >= position.STARTDATE)
                                                        .OrderBy(a => a.RateType_Name == "Special" ? "A" : a.RateType_Name == "Exception " ? "B" : a.RateType_Name == "Normal" ? "C" : "D").ThenBy(a => a.RateType_Name)
                                                        .FirstOrDefault()?.Prices.Where(a => a.ProductRange_ID == Range.ProductRange_Id).FirstOrDefault();

                                                        var SellContractPrices = ProductContractsSell?.PricePeriods?.Where(a => a.Period_Start_Date <= position.STARTDATE && a.Period_End_Date >= position.STARTDATE)
                                                        .OrderBy(a => a.RateType_Name == "Special" ? "A" : a.RateType_Name == "Exception " ? "B" : a.RateType_Name == "Normal" ? "C" : "D").ThenBy(a => a.RateType_Name)
                                                        .FirstOrDefault()?.Prices.Where(a => a.ProductRange_ID == Range.ProductRange_Id).FirstOrDefault();

                                                        int Occupancy = Range?.ProductTemplateName == "DOUBLE" || Range?.ProductTemplateName == "TWIN" ? 2 :
                                                            (Range?.ProductTemplateName == "TRIPLE") ? 3 : (Range?.ProductTemplateName == "QUAD") ? 4 : 1;

                                                        PosPrice = new BookingRoomsAndPrices
                                                        {
                                                            BookingRooms_Id = Guid.NewGuid().ToString(),
                                                            PositionPricing_Id = Guid.NewGuid().ToString(),
                                                            Booking_Id = booking.Booking_Id,
                                                            Position_Id = position.Position_Id,
                                                            ChargeBasis = ChargeBasis,
                                                            IsRecursive = true,
                                                            MealPlan_Id = DefMealPlan?.MealPlan_Id,
                                                            MealPlan = DefMealPlan?.MealPlan,
                                                            StartDate = position.STARTDATE,
                                                            EndDate = position.ENDDATE,
                                                            BuyCurrency_Id = position.BuyCurrency_Id,
                                                            BuyCurrency_Name = position.BuyCurrency_Name,
                                                            Action = ProductContractsBuy == null ? "R" : "N",
                                                            BuyContract_Id = ProductContractsBuy?.BuyContract_Id,
                                                            BuyPositionPrice_Id = BuyContractPrices?.ProductPrice_ID,
                                                            SellContract_Id = ProductContractsSell?.BuyContract_Id,
                                                            SellPositionPrice_Id = SellContractPrices?.ProductPrice_ID,
                                                            SellContractCurrency_Id = ProductContractsBuy?.Contract_Currency_Id,
                                                            SellContractCurrency_Name = ProductContractsBuy?.Contract_Currency,
                                                            ContractedSellPrice = SellContractPrices?.Contract_Price,
                                                            ContractedBuyPrice = BuyContractPrices?.Contract_Price,
                                                            BookingSeason_Id = position.BookingSeason.FirstOrDefault()?.BookingSeason_ID,
                                                            InvForPax = position.PositionType == "Core" ? null : "N",
                                                            InvNumber = position.PositionType == "Core" ? null : (ChargeBasis != "PRPN" ? Req_Count : (Req_Count * Occupancy)),

                                                            AuditTrail = new AuditTrail
                                                            {
                                                                CREA_DT = DateTime.Now,
                                                                CREA_US = request.UserEmailId
                                                            }
                                                        };

                                                        int duration = Convert.ToInt16(position.DURATION);
                                                        DateTime allocationDate = Convert.ToDateTime(position.STARTDATE);
                                                        PosPrice.RoomsAndPricesAllocation = new List<RoomsAndPricesAllocation>();
                                                        for (int i = 0; i < duration; i++)
                                                        {
                                                            PosPrice.RoomsAndPricesAllocation.Add(new RoomsAndPricesAllocation
                                                            {
                                                                BookingRoomDetail_ID = Guid.NewGuid().ToString(),
                                                                AllocationDate = allocationDate.AddDays(i),
                                                                OnReqQty = Req_Count,
                                                                AuditTrail = new AuditTrail
                                                                {
                                                                    CREA_DT = DateTime.Now,
                                                                    CREA_US = request.UserEmailId,
                                                                }
                                                            });
                                                        }

                                                        PosPrice.RoomName = Range?.ProductTemplateName;
                                                        PosPrice.RoomShortCode = Range?.ProductTemplateCode;
                                                        PosPrice.CategoryName = Category.ProductCategoryName;
                                                        PosPrice.Category_Id = Category?.DefProductCategory_Id;
                                                        PosPrice.ProductTemplate_Id = Range?.ProductTemplate_Id;
                                                        PosPrice.RoomShortCode = Range?.ProductTemplateCode;
                                                        PosPrice.Capacity = Range?.Quantity;
                                                        PosPrice.ProductRange_Id = Range.ProductRange_Id;
                                                        PosPrice.PersonType = Range.PersonType;
                                                        PosPrice.PersonType_Id = Range.PersonType_Id;
                                                        PosPrice.Req_Count = Req_Count;
                                                        PosPrice.BudgetPrice = BuyContractPrices?.Contract_Price;
                                                        PosPrice.RequestedPrice = BuyContractPrices?.Contract_Price;
                                                        PosPrice.BuyPrice = BuyContractPrices?.Contract_Price;
                                                        PosPrice.ConfirmedReqPrice = BuyContractPrices == null ? false : true;
                                                        PosPrice.ExcludeFromInvoice = false;
                                                        PosPrice.ApplyMarkup = true;
                                                        PosPrice.IsAdditionalYN = false;
                                                        PosPrice.Status = null;
                                                        position.STATUS = ProductContractsBuy != null ? "O" : position.STATUS;

                                                        position.BookingRoomsAndPrices.Add(PosPrice);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (ChargeBasis == "PP")
                                                    Req_Count = Convert.ToInt16(booking.BookingPax.Where(a => a.PERSTYPE == "ADULT").FirstOrDefault()?.PERSONS);
                                                else
                                                    Req_Count = 1;
                                                keyValue = KeyValueList.Where(a => a.Key == "ProductRangeId").FirstOrDefault();
                                                if (keyValue != null && !string.IsNullOrEmpty(Convert.ToString(keyValue.Value)))
                                                {
                                                    var Range = Category.ProductRanges.Where(a => a.ProductRange_Id == Convert.ToString(keyValue.Value)).FirstOrDefault();
                                                    if (Range != null)
                                                    {
                                                        var BuyContractPrices = ProductContractsBuy?.PricePeriods?.Where(a => a.Period_Start_Date <= position.STARTDATE && a.Period_End_Date >= position.STARTDATE)
                                                        .OrderBy(a => a.RateType_Name == "Special" ? "A" : a.RateType_Name == "Exception " ? "B" : a.RateType_Name == "Normal" ? "C" : "D").ThenBy(a => a.RateType_Name)
                                                        .FirstOrDefault()?.Prices.Where(a => a.ProductRange_ID == Range.ProductRange_Id).FirstOrDefault();

                                                        var SellContractPrices = ProductContractsSell?.PricePeriods?.Where(a => a.Period_Start_Date <= position.STARTDATE && a.Period_End_Date >= position.STARTDATE)
                                                        .OrderBy(a => a.RateType_Name == "Special" ? "A" : a.RateType_Name == "Exception " ? "B" : a.RateType_Name == "Normal" ? "C" : "D").ThenBy(a => a.RateType_Name)
                                                        .FirstOrDefault()?.Prices.Where(a => a.ProductRange_ID == Range.ProductRange_Id).FirstOrDefault();

                                                        int Occupancy = Range?.ProductTemplateName == "DOUBLE" || Range?.ProductTemplateName == "TWIN" ? 2 :
                                                            (Range?.ProductTemplateName == "TRIPLE") ? 3 : (Range?.ProductTemplateName == "QUAD") ? 4 : 1;

                                                        PosPrice = new BookingRoomsAndPrices
                                                        {
                                                            BookingRooms_Id = Guid.NewGuid().ToString(),
                                                            PositionPricing_Id = Guid.NewGuid().ToString(),
                                                            Booking_Id = booking.Booking_Id,
                                                            Position_Id = position.Position_Id,
                                                            ChargeBasis = ChargeBasis,
                                                            IsRecursive = true,
                                                            MealPlan_Id = DefMealPlan?.MealPlan_Id,
                                                            MealPlan = DefMealPlan?.MealPlan,
                                                            StartDate = position.STARTDATE,
                                                            EndDate = position.ENDDATE,
                                                            BuyCurrency_Id = position.BuyCurrency_Id,
                                                            BuyCurrency_Name = position.BuyCurrency_Name,
                                                            Action = ProductContractsBuy == null ? "R" : "N",
                                                            BuyContract_Id = ProductContractsBuy?.BuyContract_Id,
                                                            BuyPositionPrice_Id = BuyContractPrices?.ProductPrice_ID,
                                                            SellContract_Id = ProductContractsSell?.BuyContract_Id,
                                                            SellPositionPrice_Id = SellContractPrices?.ProductPrice_ID,
                                                            SellContractCurrency_Id = ProductContractsBuy?.Contract_Currency_Id,
                                                            SellContractCurrency_Name = ProductContractsBuy?.Contract_Currency,
                                                            ContractedSellPrice = SellContractPrices?.Contract_Price,
                                                            ContractedBuyPrice = BuyContractPrices?.Contract_Price,
                                                            BookingSeason_Id = position.BookingSeason.FirstOrDefault()?.BookingSeason_ID,
                                                            InvForPax = position.PositionType == "Core" ? null : "N",
                                                            InvNumber = position.PositionType == "Core" ? null : (ChargeBasis != "PRPN" ? Req_Count : (Req_Count * Occupancy)),

                                                            AuditTrail = new AuditTrail
                                                            {
                                                                CREA_DT = DateTime.Now,
                                                                CREA_US = request.UserEmailId
                                                            }
                                                        };

                                                        int duration = Convert.ToInt16(position.DURATION);
                                                        DateTime allocationDate = Convert.ToDateTime(position.STARTDATE);
                                                        PosPrice.RoomsAndPricesAllocation = new List<RoomsAndPricesAllocation>();
                                                        for (int i = 0; i < duration; i++)
                                                        {
                                                            PosPrice.RoomsAndPricesAllocation.Add(new RoomsAndPricesAllocation
                                                            {
                                                                BookingRoomDetail_ID = Guid.NewGuid().ToString(),
                                                                AllocationDate = allocationDate.AddDays(i),
                                                                OnReqQty = Req_Count,
                                                                AuditTrail = new AuditTrail
                                                                {
                                                                    CREA_DT = DateTime.Now,
                                                                    CREA_US = request.UserEmailId,
                                                                }
                                                            });
                                                        }

                                                        PosPrice.RoomName = Range?.ProductTemplateName;
                                                        PosPrice.RoomShortCode = Range?.ProductTemplateCode;
                                                        PosPrice.CategoryName = Category.ProductCategoryName;
                                                        PosPrice.Category_Id = Category?.DefProductCategory_Id;
                                                        PosPrice.ProductTemplate_Id = Range?.ProductTemplate_Id;
                                                        PosPrice.RoomShortCode = Range?.ProductTemplateCode;
                                                        PosPrice.Capacity = Range?.Quantity;
                                                        PosPrice.ProductRange_Id = Range.ProductRange_Id;
                                                        PosPrice.PersonType = Range.PersonType;
                                                        PosPrice.PersonType_Id = Range.PersonType_Id;
                                                        PosPrice.Req_Count = Req_Count;
                                                        PosPrice.BudgetPrice = BuyContractPrices?.Contract_Price;
                                                        PosPrice.RequestedPrice = BuyContractPrices?.Contract_Price;
                                                        PosPrice.BuyPrice = BuyContractPrices?.Contract_Price;
                                                        PosPrice.ConfirmedReqPrice = BuyContractPrices == null ? false : true;
                                                        PosPrice.ExcludeFromInvoice = false;
                                                        PosPrice.ApplyMarkup = true;
                                                        PosPrice.IsAdditionalYN = false;
                                                        PosPrice.Status = null;
                                                        position.STATUS = ProductContractsBuy != null ? "O" : position.STATUS;

                                                        position.BookingRoomsAndPrices.Add(PosPrice);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    #endregion

                                    #region Position AuditTrail
                                    position.AuditTrail = new AuditTrail();
                                    position.AuditTrail.CREA_US = request.UserEmailId;
                                    position.AuditTrail.CREA_DT = DateTime.Now;
                                    #endregion

                                    booking.Positions.Add(position);

                                    var activepos = booking.Positions.Where(a => a.STATUS.ToLower() != "c").ToList();
                                    if (activepos != null && activepos.Count > 0)
                                    {
                                        booking.STARTDATE = activepos.Where(a => a.STARTDATE != null).OrderBy(a => a.STARTDATE).ThenBy(a => a.STARTTIME).FirstOrDefault().STARTDATE;
                                        booking.ENDDATE = activepos.OrderByDescending(a => a.ENDDATE).ThenByDescending(a => a.ENDTIME).FirstOrDefault().ENDDATE;
                                    }
                                }
                                else
                                {
                                    response.Status = "Failure";
                                    response.ErrorMessage.Add("AddPosition:-Bookings->Position->Product details not found.");
                                }
                            }
                            else
                            {
                                response.Status = "Failure";
                                response.ErrorMessage.Add("AddPosition:-Bookings->Position->ProductId not found.");
                            }
                        }
                        else
                        {
                            response.Status = "Failure";
                            response.ErrorMessage.Add("AddPosition:-PositionId can not be null.");
                        }
                    }
                    #endregion   
                }
                else
                {
                    response.Status = "Failure";
                    response.ErrorMessage.Add("AddPosition:-Bookings/OpsKeyValue can not be null.");
                }
            }
            catch (Exception ex)
            {
                response.Status = "Failure";
                response.ErrorMessage.Add("AddPosition:- " + ex.Message);
            }

            return response;
        }

        public OPSWorkflowResponseStatus UpdateBookingInfoInMSCRM(ref Bookings booking, ExecuteWorkflowActionReq request)
        {
            OPSWorkflowResponseStatus response = new OPSWorkflowResponseStatus() { Status = "Success" };
            try
            {
                if (!string.IsNullOrEmpty(booking.BookingNumber) && !string.IsNullOrEmpty(booking.AuditTrail.CREA_US) && booking.Mappings != null && booking.Mappings.Any())
                {
                    var KeyValueList = request.OpsKeyValue;
                    var keyValueForPospayment = new OpsKeyValue();
                    if (request.PositionIds?.Count > 0)
                    {
                        var position = booking.Positions.Where(x => request.PositionIds.Contains(x.Position_Id)).FirstOrDefault();
                        if (position != null)
                        {
                            var SystemCompanyId = booking.SystemCompany_Id;
                            int DefaultSupplierTerm = _MongoContext.mSystem.AsQueryable().Where(a => a.CoreCompany_Id == SystemCompanyId).Select(a => a.DefaultSupplierTerm).FirstOrDefault();
                            position.PaymentSchedule?.Where(a => a.Status?.ToLower() != "p").ToList().ForEach(a => a.Status = "X");
                            List<PaymentSchedule> paymentSchedulePaidList = position.PaymentSchedule?.Where(a => a.Status?.ToLower() == "p").ToList();

                            if (paymentSchedulePaidList?.Count > 0)
                            {
                                //Identify the sum of amount paid
                                decimal ttlAmtPaid = paymentSchedulePaidList.Sum(a => a.Amount);

                                //Identify the sum of LPO 
                                decimal ttlLPO = Convert.ToDecimal(position.Pricing?.Sum(a => a.BuyingPrice));

                                //Variable PaymentAmount
                                decimal variablePayAmt = ttlLPO - ttlAmtPaid;

                                PaymentSchedule objPaymentSchedule = new PaymentSchedule();
                                objPaymentSchedule.BookingPaymentSchedule_Id = Guid.NewGuid().ToString();
                                objPaymentSchedule.Company_Id = position.SupplierInfo.Id;
                                objPaymentSchedule.Company_Name = position.SupplierInfo.Name;
                                objPaymentSchedule.Position_Id = position.Position_Id;

                                var curDT = DateTime.Now;
                                DefaultSupplierTerm = (DefaultSupplierTerm == 0 ? -30 : DefaultSupplierTerm);
                                var defPaymenttermDT = position.STARTDATE.Value.AddDays(DefaultSupplierTerm);

                                if (curDT < defPaymenttermDT)
                                    objPaymentSchedule.PaymentDueDate = curDT;
                                else
                                    objPaymentSchedule.PaymentDueDate = defPaymenttermDT;

                                objPaymentSchedule.Amount = variablePayAmt;
                                objPaymentSchedule.Currency_Id = position.BuyCurrency_Id;
                                objPaymentSchedule.Currency_Name = position.BuyCurrency_Name;
                                objPaymentSchedule.Status = "N";
                                objPaymentSchedule.VoucherReleased = true;
                                objPaymentSchedule.CREA_DT = DateTime.Now;
                                objPaymentSchedule.CREA_US = request.UserEmailId;
                                position.PaymentSchedule.Add(objPaymentSchedule);
                            }
                            keyValueForPospayment = KeyValueList.Where(x => x.Key == "TableOpsPosPaymentSchedule").FirstOrDefault();
                            if (keyValueForPospayment != null)
                            {
                                var PaymentScheduleDetails = JsonConvert.DeserializeObject<List<PaymentSchedule>>(keyValueForPospayment.Value.ToString());
                                if (PaymentScheduleDetails?.Count > 0)
                                {
                                    position.PaymentSchedule.AddRange(PaymentScheduleDetails);
                                }
                            }
                        }
                        else
                        {
                            response.Status = "Failure";
                            response.ErrorMessage.Add("UpdatePaymentSchedule:-PositionId not exists in Bookings collection.");
                        }
                    }
                    else
                    {
                        response.Status = "Failure";
                        response.ErrorMessage.Add("UpdatePaymentSchedule:-PositionIds can not be null/empty.");
                    }
                }
                else
                {
                    response.Status = "Failure";
                    response.ErrorMessage.Add("UpdatePaymentSchedule:-Booking/OpsKeyValue can not be null/empty.");
                }
            }
            catch (Exception ex)
            {
                response.Status = "Failure";
                response.ErrorMessage.Add("while Calling Patch Booking API for MSCRM:- \n" + ex.Message);
            }
            return response;
        }

        #endregion

        #region Bridge Functions
        public async Task<ResponseStatus> BridgeUpdateBooking(BridgeBookingReq request)
        {
            ResponseStatus response = new ResponseStatus() { Status = "Success" };
            try
            {
                if (request != null)
                {
                    response = await _bookingProviders.SetBookingDetails(new BookingSetReq()
                    {
                        BookingNumber = request.BookingNumber,
                        User = request.User
                    });
                }
                else
                {
                    response.Status = "Failure";
                    response.ErrorMessage = "BridgeUpdateBooking:-request can not be null.";
                }
            }
            catch (Exception ex)
            {
                response.Status = "Failure";
                response.ErrorMessage = "BridgeUpdateBooking:- " + ex.Message;
            }
            return response;
        }

        public async Task<ResponseStatus> BridgeUpdateBookingPosition(BridgeBookingReq request)
        {
            ResponseStatus response = new ResponseStatus() { Status = "Success" };
            try
            {
                if (request != null)
                {
                    response = await _bookingProviders.SetBookingPositionDetails(new BookingPosAltSetReq()
                    {
                        BookingNumber = request.BookingNumber,
                        PositionId = request.PositionId,
                        User = request.User,
                        ModuleType = "ops"
                    });
                }
                else
                {
                    response.Status = "Failure";
                    response.ErrorMessage = "BridgeUpdateBookingPosition:-request can not be null.";
                }
            }
            catch (Exception ex)
            {
                response.Status = "Failure";
                response.ErrorMessage = "BridgeUpdateBookingPosition:- " + ex.Message;
            }
            return response;
        }

        public async Task<ResponseStatus> BridgeUpdateDocsAndComsLog(BridgeBookingReq request)
        {
            ResponseStatus response = new ResponseStatus() { Status = "Success" };
            try
            {
                if (request != null)
                {
                    ResponseStatus resDoc = await _documentProviders.SetDocumentsAndCommuncationsLogDetails(new DocumentStoreGetReq
                    {
                        BookingNumber = request.BookingNumber,
                        DocumentType = request.DocType,
                        Position_Id = request.PositionId,
                        Supplier_Id = request.SupplierId
                    });
                }
                else
                {
                    response.Status = "Failure";
                    response.ErrorMessage = "BridgeUpdateBookingPosition:-request can not be null.";
                }
            }
            catch (Exception ex)
            {
                response.Status = "Failure";
                response.ErrorMessage = "BridgeUpdateBookingPosition:- " + ex.Message;
            }
            return response;
        }

        public OPSWorkflowResponseStatus UpdateTemplateBookingRooms(ref Bookings booking, ExecuteWorkflowActionReq request)
        {
            OPSWorkflowResponseStatus response = new OPSWorkflowResponseStatus();
            List<OpsBookingRoomsDetails> RoomsCountData;
            List<FilterRemovedData> RoomsinDb1;
            List<FilterRemovedData> RoomsinMaterialization1;
            List<string> RoomsinDb;
            List<string> RoomsinMaterialization;
            TemplateBookingRoomsGrid IsRoomPresentInDb;
            TemplateBookingRoomsGrid RoomToBeRemoved;
            TemplateBookingRoomsGrid RoomToBeRemovedForChild;
            try
            {
                var keyValue = new OpsKeyValue();
                if (booking != null && request.OpsKeyValue?.Count > 0)
                {
                    var BookingRoomListData = booking.BookingRooms?.Where(a => a.Status?.Trim()?.ToUpper() != "X" && a.ROOMNO != null && a.ROOMNO != 0).ToList();
                    var KeyValueList = request.OpsKeyValue;
                    keyValue = KeyValueList.Where(a => a.Key == "BookingRooms").FirstOrDefault();
                    if (keyValue != null && BookingRoomListData != null && BookingRoomListData.Any())
                    {
                        RoomsCountData = JsonConvert.DeserializeObject<List<OpsBookingRoomsDetails>>(keyValue.Value.ToString());
                        RoomsCountData = RoomsCountData.Where(x => x.Status != "X").ToList();
                        if (RoomsCountData != null && RoomsCountData.Any())
                        {

                            foreach (var Room in RoomsCountData)
                            {
                                if (!string.IsNullOrEmpty(Room.BookingRooms_Id))
                                {
                                    var RoomDetail = BookingRoomListData.Where(a => a.BookingRooms_ID == Room?.BookingRooms_Id)?.FirstOrDefault();
                                    if (RoomDetail != null)
                                    {
                                        RoomDetail.ROOMNO = Room?.NewLevel;
                                    }

                                }
                                else
                                {
                                    if (Room.Age != null && Room.Age.HasValue && (Room.RoomType?.Trim()?.ToUpper() == "CHILDWITHBED" || Room.RoomType?.Trim()?.ToUpper() == "CHILDWITHOUTBED" || Room.RoomType?.Trim()?.ToUpper() == "INFANT"))
                                    {
                                        IsRoomPresentInDb = booking.BookingRooms.Where(x => x.SUBPROD?.Trim()?.ToLower() == Room.RoomType?.Trim()?.ToLower() && x.Age != null && x.Age == Room.Age.Value).FirstOrDefault();
                                    }
                                    else
                                    {
                                        IsRoomPresentInDb = booking.BookingRooms.Where(x => x.SUBPROD?.Trim()?.ToLower() == Room.RoomType?.Trim()?.ToLower()).FirstOrDefault();
                                    }
                                    if (IsRoomPresentInDb != null)
                                    {
                                        IsRoomPresentInDb.Status = null;
                                        IsRoomPresentInDb.ROOMNO = Room.NewLevel;
                                    }
                                    else
                                    {
                                        TemplateBookingRoomsGrid NewRoom = new TemplateBookingRoomsGrid();
                                        NewRoom.BookingRooms_ID = Guid.NewGuid().ToString();
                                        NewRoom.Booking_Id = booking.Booking_Id;
                                        NewRoom.ROOMNO = Room?.NewLevel;

                                        NewRoom.ProductTemplate_Id = (Room.RoomType == "CHILDWITHBED" || Room.RoomType == "CHILDWITHOUTBED" || Room.RoomType == "INFANT") ? _MongoContext.mProductTemplates.AsQueryable().Where(m => m.Name != null && m.Name == "CHILD")?.FirstOrDefault()?.VoyagerProductTemplate_Id : _MongoContext.mProductTemplates.AsQueryable().Where(m => m.Name != null && m.Name.ToLower() == Room.RoomType.ToLower())?.FirstOrDefault()?.VoyagerProductTemplate_Id;
                                        NewRoom.PersonType_Id = Room.RoomType?.ToUpper() == "CHILDWITHBED" ? _MongoContext.mDefPersonType.AsQueryable().Where(a => a.PersonType != null && a.PersonType.ToUpper() == "CHILD + BED").FirstOrDefault()?.defPersonType_Id : Room.RoomType?.ToUpper() == "CHILDWITHOUTBED" ? _MongoContext.mDefPersonType.AsQueryable().Where(a => a.PersonType != null && a.PersonType.ToUpper() == "CHILD - BED").FirstOrDefault()?.defPersonType_Id : Room.RoomType?.ToUpper() == "INFANT" ? _MongoContext.mDefPersonType.AsQueryable().Where(a => a.PersonType != null && a.PersonType.ToUpper() == "INFANT").FirstOrDefault().defPersonType_Id : null;
                                        var CategoryID = booking.BookingRooms.FirstOrDefault().Category_Id;
                                        NewRoom.Category_Id = CategoryID != null ? CategoryID : _MongoContext.mProdCatDef.AsQueryable().Where(a => a.Name != null && a.Name == "Standard")?.FirstOrDefault()?.VoyagerDefProductCategoryId;
                                        NewRoom.SUBPROD = Room?.RoomType;
                                        var Name = _MongoContext.mProdCatDef.AsQueryable().Where(a => a.VoyagerDefProductCategoryId == NewRoom.Category_Id)?.FirstOrDefault()?.Name;
                                        NewRoom.Name = Name != null ? Name : _MongoContext.mProdCatDef.AsQueryable().Where(a => a.Name != null && a.Name == "Standard")?.FirstOrDefault()?.Name;
                                        NewRoom.ID = null;
                                        NewRoom.Age = Room?.Age;
                                        NewRoom.Status = string.Empty;
                                        booking.BookingRooms.Add(NewRoom);
                                    }

                                }
                            }

                        }
                        if (RoomsCountData != null && RoomsCountData.Any())
                        {
                            var BookingRoomListDataForNotChild = BookingRoomListData.Where(x => x.SUBPROD != "CHILDWITHBED" && x.SUBPROD != "CHILDWITHOUTBED" && x.SUBPROD != "INFANT").ToList();
                            RoomsinDb = BookingRoomListDataForNotChild.Select(x => x.SUBPROD).ToList();
                            RoomsinMaterialization = RoomsCountData.Where(x => x.RoomType != "CHILDWITHBED" && x.RoomType != "CHILDWITHOUTBED" && x.RoomType != "INFANT").Select(x => x.RoomType).ToList();
                            var RoomInDbNotinMaterialization = RoomsinDb.Except(RoomsinMaterialization);

                            if (RoomInDbNotinMaterialization != null && RoomInDbNotinMaterialization.Any())
                            {
                                foreach (var MissingRoomsinMaterialization in RoomInDbNotinMaterialization)
                                {
                                    RoomToBeRemoved = BookingRoomListData.Where(x => x.SUBPROD?.Trim()?.ToLower() == MissingRoomsinMaterialization?.Trim()?.ToLower()).FirstOrDefault();
                                    if (RoomToBeRemoved != null)
                                    {
                                        RoomToBeRemoved.Status = "X";
                                    }
                                }


                            }
                        }
                        if (RoomsCountData != null && RoomsCountData.Any() && BookingRoomListData != null && BookingRoomListData.Any())
                        {
                            var BookingRoomsDataforChild = BookingRoomListData.Where(x => x.SUBPROD?.Trim()?.ToUpper() == "CHILDWITHBED" || x.SUBPROD?.Trim()?.ToUpper() == "CHILDWITHOUTBED" || x.SUBPROD?.Trim()?.ToUpper() == "INFANT").ToList();
                            var RoomsDataForChild = RoomsCountData.Where(x => x.RoomType?.Trim()?.ToUpper() == "CHILDWITHBED" || x.RoomType?.Trim()?.ToUpper() == "CHILDWITHOUTBED" || x.RoomType?.Trim()?.ToUpper() == "INFANT").ToList();
                            if (BookingRoomsDataforChild != null && BookingRoomsDataforChild.Any() && RoomsDataForChild != null)
                            {
                                RoomsinDb1 = BookingRoomsDataforChild.Select(x => new FilterRemovedData { Age = x.Age.HasValue ? x.Age.Value.ToString() : null, RoomName = x.SUBPROD }).ToList();
                                RoomsinMaterialization1 = RoomsDataForChild.Select(x => new FilterRemovedData { RoomName = x.RoomType, Age = x.Age.HasValue ? x.Age.Value.ToString() : null }).ToList();
                                //var RoomInDbNotinMaterializationForChild = RoomsinDb1.Except(RoomsinMaterialization1);
                                foreach (var room in RoomsinMaterialization1)
                                {
                                    RoomsinDb1.RemoveAll(a => a.RoomName == room.RoomName && a.Age == room.Age);

                                }
                                var RoomInDbNotinMaterializationForChild = RoomsinDb1;
                                if (RoomInDbNotinMaterializationForChild != null && RoomInDbNotinMaterializationForChild.Any())
                                {
                                    foreach (FilterRemovedData MissingRoomsinMaterialization in RoomInDbNotinMaterializationForChild)
                                    {
                                        if (!string.IsNullOrEmpty(MissingRoomsinMaterialization.Age))
                                        {
                                            RoomToBeRemovedForChild = BookingRoomListData.Where(x => x.SUBPROD?.Trim()?.ToLower() == MissingRoomsinMaterialization.RoomName?.Trim()?.ToLower() && x.Age.Value.ToString() == MissingRoomsinMaterialization.Age).FirstOrDefault();
                                            if (RoomToBeRemovedForChild != null)
                                            {
                                                RoomToBeRemovedForChild.Status = "X";
                                            }
                                        }
                                    }


                                }
                            }
                        }
                    }

                }
                response.Status = "Success";
            }
            catch (Exception ex)
            {
                response.Status = "Failure";
                response.ErrorMessage.Add(ex.Message);
            }

            return response;
        }

        public OPSWorkflowResponseStatus UpdateTemplateBookingPax(ref Bookings booking, ExecuteWorkflowActionReq request)
        {
            OPSWorkflowResponseStatus response = new OPSWorkflowResponseStatus();
            List<OpsBookingPaxDetails> RoomsPaxCountData;

            TemplateBookingPaxGrid IsRoomPaxPresentInDb;
            try
            {
                var keyValue = new OpsKeyValue();
                if (booking != null && request.OpsKeyValue?.Count > 0)
                {
                    var BookingPaxListData = booking.BookingPax?.Where(a => a.Status?.Trim()?.ToUpper() != "X" && a.PERSONS > 0).ToList();
                    var KeyValueList = request.OpsKeyValue;
                    keyValue = KeyValueList.Where(a => a.Key == "BookingPax").FirstOrDefault();
                    if (keyValue != null && BookingPaxListData != null && BookingPaxListData.Any())
                    {
                        RoomsPaxCountData = JsonConvert.DeserializeObject<List<OpsBookingPaxDetails>>(keyValue.Value.ToString());
                        if (RoomsPaxCountData != null && RoomsPaxCountData.Any())
                        {

                            foreach (var RoomPax in RoomsPaxCountData)
                            {
                                if (!string.IsNullOrEmpty(RoomPax.BookingPax_ID))
                                {
                                    var RoomPaxDetail = BookingPaxListData.Where(a => a.BookingPax_Id == RoomPax?.BookingPax_ID)?.FirstOrDefault();
                                    if (RoomPaxDetail != null)
                                    {
                                        if (RoomPax.PassengerQty.Value > 0)
                                        {
                                            if (RoomPax.PassengerQty != null && RoomPax.PassengerQty.HasValue) { RoomPaxDetail.PERSONS = RoomPax.PassengerQty.Value; }
                                        }
                                        else
                                        {
                                            RoomPaxDetail.Status = "X";
                                        }
                                    }

                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(RoomPax.PassengerAge) && RoomPax.PassengerType.Trim().ToUpper() != "ADULT")
                                    {
                                        IsRoomPaxPresentInDb = booking.BookingPax.Where(x => x.PERSTYPE != null && x.PERSTYPE.Trim().ToLower() == RoomPax.PassengerType.Trim().ToLower() && x.AGE == Convert.ToInt32(RoomPax.PassengerAge))?.FirstOrDefault();
                                    }
                                    else
                                    {
                                        IsRoomPaxPresentInDb = booking.BookingPax.Where(x => x.PERSTYPE != null && x.PERSTYPE?.Trim()?.ToLower() == RoomPax.PassengerType.Trim().ToLower())?.FirstOrDefault();
                                    }
                                    if (IsRoomPaxPresentInDb != null)
                                    {
                                        if (RoomPax.PassengerQty != null && RoomPax.PassengerQty.HasValue && RoomPax.PassengerQty.Value > 0)
                                        {
                                            IsRoomPaxPresentInDb.Status = null;

                                            if (RoomPax.PassengerQty != null && RoomPax.PassengerQty.HasValue)
                                            {
                                                IsRoomPaxPresentInDb.PERSONS = RoomPax.PassengerQty.Value;
                                            }
                                        }
                                        else
                                        {
                                            IsRoomPaxPresentInDb.Status = "X";
                                        }
                                    }
                                    else
                                    {
                                        if (RoomPax.PassengerQty != null && RoomPax.PassengerQty.HasValue && RoomPax.PassengerQty.Value > 0)
                                        {
                                            TemplateBookingPaxGrid NewPax = new TemplateBookingPaxGrid();
                                            NewPax.BookingPax_Id = Guid.NewGuid().ToString();
                                            NewPax.PERSTYPE = RoomPax?.PassengerType;
                                            NewPax.Booking_Id = booking?.Booking_Id;
                                            NewPax.PersonType_Id = RoomPax.PassengerType?.Trim()?.ToUpper() == "CHILD + BED" ? _MongoContext.mDefPersonType.AsQueryable().Where(a => a.PersonType != null && a.PersonType.ToUpper() == "CHILD + BED")?.FirstOrDefault()?.defPersonType_Id : RoomPax?.PassengerType?.ToUpper() == "CHILD - BED" ? _MongoContext.mDefPersonType.AsQueryable().Where(a => a.PersonType != null && a.PersonType.ToUpper() == "CHILD - BED")?.FirstOrDefault()?.defPersonType_Id : RoomPax?.PassengerType?.ToUpper() == "INFANT" ? _MongoContext.mDefPersonType.AsQueryable().Where(a => a.PersonType != null && a.PersonType.ToUpper() == "INFANT")?.FirstOrDefault()?.defPersonType_Id : null;
                                            if (RoomPax.PassengerQty.HasValue) { NewPax.PERSONS = RoomPax.PassengerQty.Value; }
                                            if (RoomPax.PassengerType?.Trim()?.ToUpper() == "CHILD + BED" || RoomPax.PassengerType?.Trim()?.ToUpper() == "CHILD - BED" || RoomPax.PassengerType?.Trim()?.ToUpper() == "INFANT")
                                            {
                                                NewPax.AGE = Convert.ToInt32(RoomPax.PassengerAge);
                                            }
                                            booking.BookingPax.Add(NewPax);
                                        }

                                    }

                                }
                            }

                        }

                    }
                }
                response.Status = "Success";
            }
            catch (Exception ex)
            {
                response.Status = "Failure";
                response.ErrorMessage.Add(ex.Message);
            }

            return response;
        }
        #endregion

        #region Validate Booking
        public async Task<OPSWorkflowResponseStatus> ValidateBooking(OpsBookingSetReq request, Bookings booking)
        {
            OPSWorkflowResponseStatus response = new OPSWorkflowResponseStatus() { ErrorMessage = new List<string>() };
            try
            {
                if (booking != null)
                {
                    response.Status = "Success";
                    if (request.ModuleParent?.ToLower() == "booking" && request.Module?.ToLower() == "position" && request.Action?.ToLower() == "book")
                    {
                        var posId = request.PositionIds?.FirstOrDefault();
                        var pos = booking.Positions?.Where(a => a.Position_Id == posId).FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(pos?.Product_Id))
                        {
                            FilterDefinition<Products> filterProd = Builders<Products>.Filter.Empty;
                            filterProd = filterProd & Builders<Products>.Filter.Eq(x => x.VoyagerProduct_Id, pos.Product_Id);
                            var objProduct = await _MongoContext.Products.Find(filterProd).FirstOrDefaultAsync();

                            if (objProduct?.Placeholder == true)
                            {
                                response.ErrorMessage.Add("Position Cannot be confirmed as an " + pos.Product_Name + " is not a valid product. Please switch to a valid product to continue.");
                            }
                            var chkrooms = pos.BookingRoomsAndPrices?.Where(a => a.BuyPrice == null || a.ConfirmedReqPrice == false || a.ConfirmedReqPrice == null).ToList().Count;
                            if (chkrooms > 0)
                            {
                                response.ErrorMessage.Add("Position cannot be confirmed as BUY RATES have not been entered.");
                            }

                            if (!_commonRepository.ValidateEmailCustom(pos.SupplierInfo?.Contact_Email))
                            {
                                response.ErrorMessage.Add("Position cannot be confirmed as there is an invalid EMAIL address for this SUPPLIER.");
                            }

                            var system = _MongoContext.mSystem.AsQueryable().Where(a => a.CoreCompany_Id == pos.SupplierInfo.Id).ToList();
                            if (system.Count > 0)
                            {
                                response.ErrorMessage.Add("Position Cannot be confirmed as " + pos.SupplierInfo.Name + " is not a valid supplier.");
                            }
                            else
                            {
                                FilterDefinition<mCompanies> filterComp = Builders<mCompanies>.Filter.Empty;
                                filterComp = filterComp & Builders<mCompanies>.Filter.Where(x => x.Company_Id == pos.SupplierInfo.Id && x.Issupplier == true);
                                var objCompany = await _MongoContext.mCompanies.Find(filterComp).FirstOrDefaultAsync();
                                if (objCompany == null)
                                {
                                    response.ErrorMessage.Add("Position Cannot be confirmed as " + pos.SupplierInfo.Name + "  is not a valid supplier.");
                                }
                            }

                            if (response.ErrorMessage?.Count > 0)
                            {
                                response.Status = "Failure";
                            }
                        }
                        else
                        {
                            response.Status = "Failure";
                            response.ErrorMessage.Add("Product_Id con not be null/blank in Bookings->Positions collection for PositionId " + posId + ".");
                        }
                    }
                }
                else
                {
                    response.Status = "Failure";
                    response.ErrorMessage.Add("BookingNumber/PositionId can not be Null/Blank.");
                }
            }
            catch (Exception ex)
            {
                response.Status = "Failure";
                response.ErrorMessage.Add("An error occurs:- " + ex.Message);
            }
            return response;
        }
        #endregion
        #endregion

        #region Payment Schedule
        public async Task<PaymentScheduleGetRes> GetPositionPaymentSchedule(PaymentScheduleGetReq request)
        {
            PaymentScheduleGetRes response = new PaymentScheduleGetRes() { PaymentSchedule = new List<PaymentSchedule>(), ResponseStatus = new ResponseStatus() };
            try
            {
                if (!string.IsNullOrWhiteSpace(request.BookingNumber) && !string.IsNullOrWhiteSpace(request.PositionId))
                {
                    var booking = await _MongoContext.Bookings.FindAsync(x => x.BookingNumber == request.BookingNumber).Result.FirstOrDefaultAsync();
                    if (booking != null)
                    {
                        var position = booking.Positions?.Where(a => a.Position_Id == request.PositionId).FirstOrDefault();
                        if (position != null)
                        {
                            response.PaymentSchedule = position.PaymentSchedule?.Where(a => a.Status?.ToLower().Trim() != "x").ToList();
                            response.ResponseStatus.Status = "Success";
                        }
                        else
                        {
                            response.ResponseStatus.Status = "Failure";
                            response.ResponseStatus.ErrorMessage = "PositionId " + request.PositionId + " not found in monogodb";
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "Booking Number " + request.BookingNumber + " not found in monogodb";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Booking Number/PositionId can not be null/Empty.";
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
    }
}