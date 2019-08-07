using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI.Helpers;
using VGER_WAPI.Models;
using VGER_WAPI.Providers;
using VGER_WAPI_CLASSES;
using VGER_WAPI_CLASSES.Booking;

namespace VGER_WAPI.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        #region Private Variable Declaration
        private readonly MongoContext _MongoContext = null;
        private readonly IUserRepository _userRepository;
        private readonly IEmailRepository _emailRepository;
        private BookingProviders _bookingProviders = null;
        private DocumentProviders _documentProviders = null;
        private readonly IConfiguration _configuration;
        #endregion

        public BookingRepository(IOptions<MongoSettings> settings, IUserRepository userRepository, IEmailRepository emailRepository, IConfiguration configuration)
        {
            _MongoContext = new MongoContext(settings);
            _userRepository = userRepository;
            _emailRepository = emailRepository;
            _configuration = configuration;
            _bookingProviders = new BookingProviders(_configuration);
            _documentProviders = new DocumentProviders(_configuration);
        }

        #region mBookings Collection
        public async Task<BookingSearchRes> GetBookingDetails(BookingSearchReq request)
        {
            BookingSearchRes response = new BookingSearchRes();
            FilterDefinition<mBookings> filter;
            filter = Builders<mBookings>.Filter.Empty;

            var UserCompany_Id = _MongoContext.mUsers.AsQueryable().Where(x => x.VoyagerUser_Id == request.UserId).Select(y => y.Company_Id).FirstOrDefault();
            var AdminRoleId = _MongoContext.mRoles.AsQueryable().Where(x => x.LoweredRoleName == "administrator").Select(y => y.Voyager_Role_Id).FirstOrDefault();
            var AdminRole = _MongoContext.mUsersInRoles.AsQueryable().Where(x => x.UserId == request.UserId && x.RoleId == AdminRoleId).FirstOrDefault();

            if (AdminRole == null)//means user is not an Admin
            {
                var CoreCompany_Id = _MongoContext.mSystem.AsQueryable().Select(y => y.CoreCompany_Id).FirstOrDefault();
                if (UserCompany_Id == CoreCompany_Id)
                {
                    if (!string.IsNullOrWhiteSpace(CoreCompany_Id))
                    {
                        filter = filter & Builders<mBookings>.Filter.Where(x => x.AgentId != CoreCompany_Id);
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(UserCompany_Id))
                    {
                        var lstCompanies = _MongoContext.mCompanies.AsQueryable().Where(x => x.ParentAgent_Id == UserCompany_Id).Select(y => y.Company_Id).ToList();
                        filter = filter & Builders<mBookings>.Filter.Where(x => lstCompanies.Contains(x.AgentId));
                    }
                }
            }

            if (!string.IsNullOrEmpty(request.AgentName))
            {
                filter = filter & Builders<mBookings>.Filter.Where(x => x.AgentName.Trim().ToLower().Contains(request.AgentName.Trim().ToLower()));
            }
            if (!string.IsNullOrWhiteSpace(request.BookingNumber))
            {
                filter = filter & Builders<mBookings>.Filter.Where(x => x.BookingNumber.ToLower().Contains(request.BookingNumber.ToLower()));
            }
            if (!string.IsNullOrWhiteSpace(request.AgentCode))
            {
                filter = filter & Builders<mBookings>.Filter.Where(x => x.AgentCode.ToLower().Contains(request.AgentCode.ToLower()));
            }
            if (!string.IsNullOrWhiteSpace(request.BookingName))
            {
                filter = filter & Builders<mBookings>.Filter.Where(x => x.CUSTREF.Trim().ToLower().Contains(request.BookingName.Trim().ToLower()));
            }
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                filter = filter & Builders<mBookings>.Filter.Where(x => x.Status.ToLower() == request.Status.ToLower());
            }
            if (!string.IsNullOrWhiteSpace(request.DateType) && request.DateType.ToLower().Trim() == "creation date")
            {
                DateTime todt = new DateTime();
                if (!string.IsNullOrWhiteSpace(request.From) && !string.IsNullOrEmpty(request.To))
                {
                    todt = DateTime.Parse(request.To).AddHours(23).AddMinutes(59).AddSeconds(59);
                    filter = filter & Builders<mBookings>.Filter.Where(x => x.CreaDT >= DateTime.Parse(request.From) && x.CreaDT <= todt);
                }
                else if (!string.IsNullOrWhiteSpace(request.From) && string.IsNullOrEmpty(request.To))
                {
                    filter = filter & Builders<mBookings>.Filter.Where(x => x.CreaDT >= DateTime.Parse(request.From));
                }
                else if (string.IsNullOrWhiteSpace(request.From) && !string.IsNullOrEmpty(request.To))
                {
                    todt = DateTime.Parse(request.To).AddHours(23).AddMinutes(59).AddSeconds(59);
                    filter = filter & Builders<mBookings>.Filter.Where(x => x.CreaDT <= todt);
                }
            }

            if (!string.IsNullOrWhiteSpace(request.DateType) && request.DateType.ToLower().Trim() == "travel date")
            {
                DateTime todt = new DateTime();
                if (!string.IsNullOrWhiteSpace(request.From))
                {
                    filter = filter & Builders<mBookings>.Filter.Where(x => x.DEPARTUREDATE >= DateTime.Parse(request.From));
                }
                if (!string.IsNullOrEmpty(request.To))
                {
                    todt = DateTime.Parse(request.To).AddHours(23).AddMinutes(59).AddSeconds(59);
                    filter = filter & Builders<mBookings>.Filter.Where(x => x.ARRIVALDATE <= todt);
                }
            }

            var result = await _MongoContext.mBookings.Find(filter).Sort("{DEPARTUREDATE: 1}").Skip(request.Start).Limit(request.Length).Project(x => new BookingList
            {
                BookingId = x.BookingId,
                BookingReference = x.BookingNumber,
                AgentId = x.AgentId,
                AgentCode = x.AgentCode,
                AgentName = x.AgentName,
                Agentontact = x.AgentContactName,
                BookingName = x.CUSTREF,
                StartDate = x.DEPARTUREDATE != null ? Convert.ToDateTime(x.DEPARTUREDATE.ToString()).ToString("dd/MM/yyyy").Replace('-', '/') : "",
                EndDate = x.ARRIVALDATE != null ? Convert.ToDateTime(x.ARRIVALDATE.ToString()).ToString("dd/MM/yyyy").Replace('-', '/') : "",
                Duration = Convert.ToString(x.Duration),
                Status = x.Status
            }).ToListAsync();

            if (result.Count > 0)
            {
                response.BookingTotalCount = Convert.ToInt32(_MongoContext.mBookings.Find(filter).Count());
            }
            response.Bookings = result;

            return response;
        }

        public List<Attributes> GetBookingStatusList()
        {
            BookingSearchRes response = new BookingSearchRes();
            response.BookingStatusList = _MongoContext.mStatus.AsQueryable().Where(x => x.ForBooking == true || x.ForCompany == true).Select(y => new Attributes { AttributeName = y.Description, Attribute_Id = y.Status }).ToList();

            return response.BookingStatusList;
        }

        public async Task<BookingSearchRes> GetBookingRoomDetails(BookingSearchReq request)
        {
            BookingSearchRes response = new BookingSearchRes();
            FilterDefinition<mBookingRooms> filter;
            filter = Builders<mBookingRooms>.Filter.Empty;
            if (!string.IsNullOrWhiteSpace(request.BookingNumber))
            {
                filter = filter & Builders<mBookingRooms>.Filter.Where(x => x.BookingNumber == request.BookingNumber);
            }
            if (!string.IsNullOrWhiteSpace(request.PositionId))
            {
                filter = filter & Builders<mBookingRooms>.Filter.Where(x => x.Position_Id == request.PositionId);
            }
            var result = await _MongoContext.mBookingRooms.Find(filter).Project(q => new BookingList
            {
                PositionId = q.Position_Id,
                RoomNo = q.ROOMNO == 0 ? "" : q.ROOMNO.ToString(),
                ProductTemplate = q.ProductTemplate,
                PersonType = q.PersonType,
                ProductRangeId = q.ProductRange_Id,
                BookingRoomId = q.BookingRooms_ID
            }).ToListAsync();

            response.BookingRooms = result;
            return response;
        }

        public async Task<BookingSearchRes> GetBookingPositionPricingDetails(BookingSearchReq request)
        {
            BookingSearchRes response = new BookingSearchRes();
            FilterDefinition<mBookingPositionPricing> filter;
            filter = Builders<mBookingPositionPricing>.Filter.Empty;
            if (!string.IsNullOrWhiteSpace(request.BookingNumber))
            {
                filter = filter & Builders<mBookingPositionPricing>.Filter.Where(x => x.BookingNumber == request.BookingNumber);
            }
            if (!string.IsNullOrWhiteSpace(request.PositionId))
            {
                filter = filter & Builders<mBookingPositionPricing>.Filter.Where(x => x.Position_Id == request.PositionId);
            }

            var result = await _MongoContext.mBookingPositionPricing.Find(filter).Project(q => new BookingList
            {
                PositionId = q.Position_Id,
                BookingRoomId = q.BookingRooms_Id,
                ProductTemplate = q.ProductTemplate,
                PersonType = q.PersonType,
                BookingReference = q.BookingNumber
            }).ToListAsync();

            if (request.BookingRoomId != null && request.BookingRoomId.Count > 0)
            {
                // filter = filter & Builders<mBookingPositionPricing>.Filter.Where(x => x.BookingRooms_Id == request.BookingRoomId);

                result = _MongoContext.mBookingPositionPricing.AsQueryable().Where(p => request.BookingRoomId.Contains(p.BookingRooms_Id))
                  .Select(p => new BookingList { BookingRoomId = p.BookingRooms_Id, PositionId = p.Position_Id, ProductTemplate = p.ProductTemplate }).ToList();
            }

            response.BookingPositionPricing = result;
            return response;
        }

        public async Task<BookingDocumentGetRes> GetBookingDocumentDetails(BookingDocumentGetReq request)
        {
            BookingDocumentGetRes response = new BookingDocumentGetRes();

            var document = _MongoContext.mBookingDocuments.AsQueryable().Where(x => x.BookingId == request.Booking_Id && x.Type == request.Type).FirstOrDefault();

            if (document != null)
            {
                response.Document_Id = document.DocumentId;
                response.File_Path = document.FilePath;
                response.FileCreationDate = document.CreateDate != null ? document.CreateDate.ToString("dd MMM yyyy") : DateTime.Now.ToString("dd MMM yyyy");
            }
            else
            {
                return null;
            }

            return response;
        }

        public async Task<BookingDocumentSetRes> SetBookingDocumentDetails(BookingDocumentSetReq request)
        {
            try
            {
                BookingDocumentSetRes response = new BookingDocumentSetRes();
                mBookingDocuments doc = new mBookingDocuments();

                var document = _MongoContext.mBookingDocuments.AsQueryable().Where(x => x.BookingId == request.documents.BookingId && x.Type == request.documents.Type).FirstOrDefault();
                if (document == null)
                {
                    doc.BookingDocumentId = Guid.NewGuid().ToString();
                    doc.BookingId = request.documents.BookingId;
                    doc.BookingNumber = request.documents.BookingNumber;
                    doc.DocumentId = request.documents.DocumentId;
                    doc.Type = request.documents.Type;
                    doc.FilePath = request.documents.FilePath;
                    doc.CreateUser = request.documents.CreateUser;
                    doc.CreateDate = DateTime.Now;

                    await _MongoContext.mBookingDocuments.InsertOneAsync(doc);
                    response.ResponseStatus.Status = "Success";
                    response.ResponseStatus.ErrorMessage = "Saved Successfully.";
                }
                else
                {
                    doc._Id = document._Id;
                    doc.BookingDocumentId = document.BookingDocumentId;
                    doc.BookingId = document.BookingId;
                    doc.BookingNumber = document.BookingNumber;
                    doc.DocumentId = request.documents.DocumentId;
                    doc.Type = document.Type;
                    doc.FilePath = request.documents.FilePath;
                    //doc.CreateUser = document.CreateUser;
                    //doc.CreateDate = document.CreateDate;
                    doc.EditUser = request.documents.CreateUser;
                    doc.EditDate = DateTime.Now;

                    ReplaceOneResult replaceResult = await _MongoContext.mBookingDocuments.ReplaceOneAsync(Builders<mBookingDocuments>.Filter.Eq("BookingDocumentId", doc.BookingDocumentId), doc);
                    response.ResponseStatus.Status = replaceResult.MatchedCount > 0 ? "Success" : "Failure";
                    response.ResponseStatus.ErrorMessage = replaceResult.MatchedCount > 0 ? "Saved Successfully." : "Details not updated.";
                }
                return response;
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        #endregion

        #region Bookings Collection
        public async Task<BookingSearchRes> GetSearchBookingDetails(BookingSearchReq request)
        {
            BookingSearchRes response = new BookingSearchRes() { ResponseStatus = new ResponseStatus() };
            FilterDefinition<Bookings> filter;
            filter = Builders<Bookings>.Filter.Empty;

            try
            {
                var UserCompany_Id = _MongoContext.mUsers.AsQueryable().Where(x => x.VoyagerUser_Id == request.UserId).Select(y => y.Company_Id).FirstOrDefault();
                var AdminRoleId = _MongoContext.mRoles.AsQueryable().Where(x => x.LoweredRoleName == "administrator").Select(y => y.Voyager_Role_Id).FirstOrDefault();
                var AdminRole = _MongoContext.mUsersInRoles.AsQueryable().Where(x => x.UserId == request.UserId && x.RoleId == AdminRoleId).FirstOrDefault();

                if (AdminRole == null)//means user is not an Admin
                {
                    var CoreCompany_Id = _MongoContext.mSystem.AsQueryable().Select(y => y.CoreCompany_Id).FirstOrDefault();
                    if (UserCompany_Id == CoreCompany_Id)
                    {
                        if (!string.IsNullOrWhiteSpace(CoreCompany_Id))
                        {
                            filter = filter & Builders<Bookings>.Filter.Where(x => x.AgentInfo.Id != CoreCompany_Id);
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(UserCompany_Id))
                        {
                            var lstCompanies = _MongoContext.mCompanies.AsQueryable().Where(x => x.ParentAgent_Id == UserCompany_Id).Select(y => y.Company_Id).ToList();
                            filter = filter & Builders<Bookings>.Filter.Where(x => lstCompanies.Contains(x.AgentInfo.Id));
                        }
                    }
                }

                if (!string.IsNullOrEmpty(request.AgentName))
                {
                    filter = filter & Builders<Bookings>.Filter.Where(x => x.AgentInfo.Name.Trim().ToLower().Contains(request.AgentName.Trim().ToLower()));
                }
                if (!string.IsNullOrWhiteSpace(request.BookingNumber))
                {
                    filter = filter & Builders<Bookings>.Filter.Where(x => x.BookingNumber.ToLower().Contains(request.BookingNumber.ToLower()));
                }
                if (!string.IsNullOrWhiteSpace(request.AgentCode))
                {
                    filter = filter & Builders<Bookings>.Filter.Where(x => x.AgentInfo.Code.ToLower().Contains(request.AgentCode.ToLower()));
                }
                if (!string.IsNullOrWhiteSpace(request.BookingName))
                {
                    filter = filter & Builders<Bookings>.Filter.Where(x => x.CustRef.Trim().ToLower().Contains(request.BookingName.Trim().ToLower()));
                }
                if (!string.IsNullOrWhiteSpace(request.Status))
                {
                    filter = filter & Builders<Bookings>.Filter.Where(x => x.STATUS.ToLower() == request.Status.ToLower());
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
                    if (!string.IsNullOrWhiteSpace(request.From))
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

                var result = await _MongoContext.Bookings.Find(filter).Sort("{ENDDATE: 1}").Skip(request.Start).Limit(request.Length).Project(x => new BookingList
                {
                    BookingId = x.Booking_Id,
                    BookingReference = x.BookingNumber,
                    AgentId = x.AgentInfo.Id,
                    AgentCode = x.AgentInfo.Code,
                    AgentName = x.AgentInfo.Name,
                    Agentontact = x.AgentInfo.Contact_Name,
                    BookingName = x.CustRef,
                    StartDate = x.STARTDATE != null ? Convert.ToDateTime(x.STARTDATE.ToString()).ToString("dd/MM/yyyy").Replace('-', '/') : "",
                    EndDate = x.ENDDATE != null ? Convert.ToDateTime(x.ENDDATE.ToString()).ToString("dd/MM/yyyy").Replace('-', '/') : "",
                    // Duration = Convert.ToString(x.Duration),
                    Duration = x.Duration != null ? x.Duration.ToString() : "0",
                    Status = x.STATUS
                }).ToListAsync();

                if (result.Count > 0)
                {
                    response.BookingTotalCount = Convert.ToInt32(_MongoContext.Bookings.Find(filter).Count());
                }
                response.Bookings = result;
            }
            catch (Exception ex)
            {
                response = new BookingSearchRes();
                response.ResponseStatus = new ResponseStatus();
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }

            return response;
        }

        public async Task<BookingInfoRes> GetBookingDetailsByParam(BookingDetailReq request)
        {
            BookingInfoRes res = new BookingInfoRes() { ResponseStatus = new ResponseStatus(), Bookings = new Bookings() };
            FilterDefinition<Bookings> filter;
            filter = Builders<Bookings>.Filter.Empty;

            try
            {
                if (!string.IsNullOrWhiteSpace(request.BookingNumber))
                {
                    filter = filter & Builders<Bookings>.Filter.Where(f => f.BookingNumber == request.BookingNumber);
                    var result = await _MongoContext.Bookings.Find(filter).FirstOrDefaultAsync();

                    UserCookieDetail userdetails = new UserCookieDetail() { UserName = request.UserName, Company_Id = request.VoygerCompany_Id };
                    _userRepository.GetUserCompanyType(ref userdetails);
                    var System = _MongoContext.mSystem.AsQueryable().FirstOrDefault();

                    string AgentId = result.AgentInfo?.Id;

                    var suppliersList = result.Positions?.Count > 0 ? result.Positions.Where(a => a.SupplierInfo != null).SelectMany(a => a.SupplierInfo.Id).ToList() : new List<char>();
                    string Suppliers = suppliersList?.Count > 0 ? string.Join(",", suppliersList) : "";

                    if (userdetails.Company_Id.ToString().ToLower() != System.CoreCompany_Id.ToString().ToLower())
                    {
                        if ((userdetails.IsAgent ?? false))
                        {
                            if (userdetails.Company_Id.Trim().ToLower() != AgentId.Trim().ToLower())
                            {
                                res.Bookings = new Bookings();
                                res.Bookings.CustRef = result.CustRef;
                                res.ResponseStatus = new ResponseStatus() { Status = "Failure", ErrorMessage = "User has not rights." };
                            }
                        }
                        else if ((userdetails.IsSupplier ?? false))
                        {
                            if (!Suppliers.Contains("," + userdetails.Company_Id + ","))
                            {
                                res.Bookings = new Bookings();
                                res.Bookings.CustRef = result.CustRef;
                                res.ResponseStatus = new ResponseStatus() { Status = "Failure", ErrorMessage = "User has not rights." };
                            }
                        }
                    }
                    else
                    {
                        res.ResponseStatus = new ResponseStatus() { Status = "Success" };
                        res.Bookings = result;
                    }
                }
            }
            catch (Exception ex)
            {
                res.Bookings = new Bookings();
                res.ResponseStatus = new ResponseStatus() { Status = "Failure", ErrorMessage = ex.Message };
            }
            return res;
        }

        public async Task<BookingPositionsSetRes> SetBookingPositions(BookingPositionsSetReq request)
        {
            var response = new BookingPositionsSetRes() { ResponseStatus = new ResponseStatus() };
            var position = new Positions();
            var resbooking = new Bookings();
            try
            {
                if (request != null && !string.IsNullOrEmpty(request.BookingNumber))
                {
                    if (request.Position.AuditTrail == null)
                    {
                        request.Position.AuditTrail = new AuditTrail();
                    }

                    resbooking = await _MongoContext.Bookings.FindAsync(a => a.BookingNumber == request.BookingNumber).Result.FirstOrDefaultAsync();
                    if (resbooking != null)
                    {
                        if (string.IsNullOrEmpty(request.PositionId)) request.PositionId = request.Position.Position_Id;
                        if (request.Position != null && !string.IsNullOrEmpty(request.Position.Position_Id))
                        {
                            position = resbooking.Positions.Where(a => a.Position_Id == request.Position.Position_Id).FirstOrDefault();
                        }
                        //else
                        //{
                        //    position = resbooking.Positions.Where(a => a.Position_Id == request.PositionId).FirstOrDefault();
                        //    request.Position = position;
                        //}
                        if (position != null)
                        {
                            //booking startdate enddate logic
                            var activepos = resbooking.Positions.Where(a => a.STATUS.ToLower() != "c").ToList();
                            if (activepos != null && activepos.Count > 0)
                            {
                                resbooking.STARTDATE = activepos.Where(a => a.STARTDATE != null).OrderBy(a => a.STARTDATE).ThenBy(a => a.STARTTIME).FirstOrDefault().STARTDATE;
                                resbooking.ENDDATE = activepos.OrderByDescending(a => a.ENDDATE).ThenByDescending(a => a.ENDTIME).FirstOrDefault().ENDDATE;
                            }

                            request.Position.AuditTrail.MODI_DT = DateTime.Now;
                            request.Position.AuditTrail.MODI_US = request.UserEmailId;

                            var res = await _MongoContext.Bookings.FindOneAndUpdateAsync(a => a.BookingNumber == request.BookingNumber && a.Positions.Any(b => b.Position_Id == request.Position.Position_Id),
                                            Builders<Bookings>.Update.Set(m => m.Positions[-1], request.Position).
                                            Set("STARTDATE", resbooking.STARTDATE).
                                            Set("ENDDATE", resbooking.ENDDATE).
                                            Set("AuditTrail.MODI_DT", DateTime.Now).
                                            Set("AuditTrail.MODI_US", request.UserEmailId));

                            if (res != null)
                            {
                                response.PositionId = request.PositionId;
                                response.ResponseStatus.Status = "Success";
                                response.ResponseStatus.ErrorMessage = "Booking Position details updated successfully.";
                            }
                            else
                            {
                                response.ResponseStatus.Status = "Failure";
                                response.ResponseStatus.ErrorMessage = "Booking Position details not updated.";
                            }
                        }
                        else
                        {
                            //booking startdate enddate logic
                            resbooking.Positions.Add(request.Position);
                            var activepos = resbooking.Positions.Where(a => a.STATUS.ToLower() != "c").ToList();
                            if (activepos != null && activepos.Count > 0)
                            {
                                resbooking.STARTDATE = activepos.Where(a => a.STARTDATE != null).OrderBy(a => a.STARTDATE).ThenBy(a => a.STARTTIME).FirstOrDefault().STARTDATE;
                                resbooking.ENDDATE = activepos.OrderByDescending(a => a.ENDDATE).ThenByDescending(a => a.ENDTIME).FirstOrDefault().ENDDATE;
                            }

                            request.Position.AuditTrail.CREA_DT = DateTime.Now;
                            request.Position.AuditTrail.CREA_US = request.UserEmailId;

                            var resultFlag = await _MongoContext.Bookings.UpdateOneAsync(Builders<Bookings>.Filter.Eq("BookingNumber", request.BookingNumber),
                                            Builders<Bookings>.Update.Push<Positions>("Positions", request.Position).
                                            Set("STARTDATE", resbooking.STARTDATE).
                                            Set("ENDDATE", resbooking.ENDDATE).
                                            Set("AuditTrail.MODI_DT", DateTime.Now).
                                            Set("AuditTrail.MODI_US", request.UserEmailId));

                            if (resultFlag.ModifiedCount > 0)
                            {
                                response.PositionId = request.PositionId;
                                response.ResponseStatus.Status = "Success";
                                response.ResponseStatus.ErrorMessage = "Booking Position details saved successfully.";
                            }
                            else
                            {
                                response.ResponseStatus.Status = "Failure";
                                response.ResponseStatus.ErrorMessage = "Booking Position details not saved.";
                            }
                        }

                        #region Bridge Service
                        //Jira ticket 657
                        if (response.ResponseStatus.Status == "Success")
                        {
                            var resPos = await _bookingProviders.SetBookingPositionDetails(new BookingPosAltSetReq()
                            {
                                BookingNumber = request.BookingNumber,
                                PositionId = request.PositionId,
                                User = request.UserEmailId
                            });
                            if (resPos != null)
                            {
                                response.ResponseStatus = resPos;
                                if (resPos.Status == "Success")
                                {
                                    response.ResponseStatus.Status = "Success";
                                    response.ResponseStatus.ErrorMessage = "Booking Position details updated successfully.";
                                }
                            }
                            else
                            {
                                response.ResponseStatus.Status = "Failure";
                                response.ResponseStatus.ErrorMessage = "Booking Position Details not updated in SQL.";
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "BookingNumber not found in Bookings collection.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "BookingNumber can not be null/Blank.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = !string.IsNullOrEmpty(ex.Message) ? ex.Message : ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message) ? ex.InnerException.Message : "";
            }
            return response;
        }

        public async Task<BookingPositionsSetRes> CancelBookingPositions(BookingCancelPositionSetReq request)
        {
            var response = new BookingPositionsSetRes() { ResponseStatus = new ResponseStatus() };
            var position = new Positions();
            var resbooking = new Bookings();
            try
            {
                if (request != null && !string.IsNullOrEmpty(request.BookingNumber) && !string.IsNullOrEmpty(request.PositionId))
                {
                    resbooking = await _MongoContext.Bookings.FindAsync(a => a.BookingNumber == request.BookingNumber).Result.FirstOrDefaultAsync();
                    if (resbooking != null)
                    {
                        position = resbooking.Positions.Where(a => a.Position_Id == request.PositionId).FirstOrDefault();
                        if (position != null)
                        {
                            //call Jira ticket 485 i.e. send the Cancel Position Booking Email 
                            var objEmailGetReq = new EmailGetReq()
                            {
                                BookingNo = request.BookingNumber,
                                DocumentType = DocType.BOOKXX,
                                PositionId = request.PositionId,
                                UserEmail = request.UserEmailId,
                                PlacerUserId = request.PlacerUserId,
                                SupplierId = position.SupplierInfo.Id,
                                QrfId = resbooking.QRFID
                            };

                            var responseStatusMail = await _emailRepository.GenerateEmail(objEmailGetReq);
                            if (responseStatusMail != null && responseStatusMail.ResponseStatus != null)
                            {
                                if (responseStatusMail.ResponseStatus.Status == "Success")
                                {
                                    position.STATUS = "C";
                                    position.CancellationReason = string.IsNullOrEmpty(request.CancelResoan) ? position.CancellationReason : request.CancelResoan;
                                    position.AuditTrail.MODI_DT = DateTime.Now;
                                    position.AuditTrail.MODI_US = request.UserEmailId;

                                    var poscnt = resbooking.Positions.Count;
                                    var activepos = resbooking.Positions.Where(a => a.STATUS.ToLower() != "c").ToList();

                                    if (activepos != null && activepos.Count > 0)
                                    {
                                        if (position.STARTDATE == resbooking.STARTDATE)
                                            resbooking.STARTDATE = activepos.Where(a => a.STARTDATE != null).OrderBy(a => a.STARTDATE).ThenBy(a => a.STARTTIME).FirstOrDefault().STARTDATE;

                                        if (position.ENDDATE == resbooking.ENDDATE)
                                            resbooking.ENDDATE = activepos.OrderByDescending(a => a.ENDDATE).ThenByDescending(a => a.ENDTIME).FirstOrDefault().ENDDATE;
                                    }

                                    if (request.PageType != "hotels")
                                    {
                                        if (activepos == null || activepos.Count == 0)
                                        {
                                            var cancelcnt = resbooking.Positions.Where(a => a.STATUS.ToLower() == "c").Count();
                                            resbooking.STATUS = cancelcnt == poscnt ? "C" : resbooking.STATUS;
                                        }
                                    }

                                    var res = await _MongoContext.Bookings.FindOneAndUpdateAsync(a => a.BookingNumber == request.BookingNumber && a.Positions.Any(b => b.Position_Id == request.PositionId),
                                                    Builders<Bookings>.Update.Set(m => m.Positions[-1], position).
                                                                              Set("STARTDATE", resbooking.STARTDATE).
                                                                              Set("ENDDATE", resbooking.ENDDATE).
                                                                              Set("STATUS", resbooking.STATUS).
                                                                              Set("AuditTrail.MODI_DT", DateTime.Now).
                                                                              Set("AuditTrail.MODI_US", request.UserEmailId));
                                    if (res != null)
                                    {
                                        #region Bridge service
                                        //Cancel booking in SQL through Bridge service
                                        var resCancelBooking = new ResponseStatus() { Status = "success" };
                                        if (request.PageType != "hotels")
                                        {
                                            if (resbooking.STATUS.ToLower() == "c")
                                            {
                                                resCancelBooking = await _bookingProviders.CancelBookingDetails(new BookingSetReq()
                                                {
                                                    BookingNumber = request.BookingNumber,
                                                    Status = "C",
                                                    User = request.UserEmailId
                                                });
                                            }
                                        }
                                        if (resCancelBooking != null)
                                        {
                                            if (resCancelBooking.Status.ToLower() == "success")
                                            {
                                                //call jira ticket 657
                                                var resPos = await _bookingProviders.SetBookingPositionDetails(new BookingPosAltSetReq()
                                                {
                                                    BookingNumber = request.BookingNumber,
                                                    PositionId = request.PositionId,
                                                    User = request.UserEmailId
                                                });
                                                if (resPos != null)
                                                {
                                                    response.ResponseStatus = resPos;
                                                    //Jira ticket 744
                                                    if (resPos.Status.ToLower() == "success")
                                                    {
                                                        ResponseStatus resDoc = await _documentProviders.SetDocumentsAndCommuncationsLogDetails(new DocumentStoreGetReq
                                                        {
                                                            BookingNumber = request.BookingNumber,
                                                            DocumentType = "BOOK-XX",
                                                            Position_Id = request.PositionId,
                                                            Supplier_Id = position.SupplierInfo.Id
                                                        });
                                                        if (resDoc != null)
                                                        {
                                                            if (resDoc.Status.ToLower() == "success")
                                                            {
                                                                response.ResponseStatus.Status = "Success";
                                                                response.ResponseStatus.ErrorMessage = "Booking Position details Cancelled successfully.";
                                                            }
                                                            else
                                                            {
                                                                response.ResponseStatus.Status = "Failure";
                                                                response.ResponseStatus.ErrorMessage = "Booking Position details not updated in Documents and CommuncationsLog in SQL.";
                                                            }
                                                        }
                                                        else
                                                        {
                                                            response.ResponseStatus.Status = "Failure";
                                                            response.ResponseStatus.ErrorMessage = "Booking Position details not updated in Documents and CommuncationsLog in SQL.";
                                                        }
                                                    }
                                                    else
                                                    {
                                                        response.ResponseStatus.Status = "Failure";
                                                        response.ResponseStatus.ErrorMessage = "Booking Position Details not updated in SQL.";
                                                    }
                                                }
                                                else
                                                {
                                                    response.ResponseStatus.Status = "Failure";
                                                    response.ResponseStatus.ErrorMessage = "Booking Position Details not updated in SQL.";
                                                }
                                            }
                                            else
                                            {
                                                response.ResponseStatus.Status = "Failure";
                                                response.ResponseStatus.ErrorMessage = resCancelBooking.StatusMessage;
                                            }
                                        }
                                        else
                                        {
                                            response.ResponseStatus.Status = "Failure";
                                            response.ResponseStatus.ErrorMessage = "Booking Details not cancelled in SQL.";
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        response.ResponseStatus.Status = "Failure";
                                        response.ResponseStatus.ErrorMessage = "Booking Position details not updated.";
                                    }
                                }
                                else
                                {
                                    response.ResponseStatus.Status = "Failure";
                                    response.ResponseStatus.ErrorMessage = responseStatusMail.ResponseStatus.ErrorMessage;
                                }
                            }
                            else
                            {
                                response.ResponseStatus.Status = "Failure";
                                response.ResponseStatus.ErrorMessage = "Email Not Sent.";
                            }
                        }
                        else
                        {
                            response.ResponseStatus.Status = "Failure";
                            response.ResponseStatus.ErrorMessage = "Position Id not found in Bookings collection.";
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "BookingNumber not found in Bookings collection.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "BookingNumber can not be null/Blank.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = !string.IsNullOrEmpty(ex.Message) ? ex.Message : ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message) ? ex.InnerException.Message : "";
            }
            return response;
        }       

        public async Task<ResponseStatus> SetBookingBackScriptDetails()
        {
            ResponseStatus response = new ResponseStatus();
            try
            {
                var lstBookings = _MongoContext.Bookings.AsQueryable().ToList();
                var lstQRFIDs = lstBookings.Select(a => a.QRFID).Distinct().ToList();
                var lstQRFPrice = _MongoContext.mQRFPrice.AsQueryable().Where(a => lstQRFIDs.Contains(a.QRFID) && a.IsCurrentVersion).ToList();
                var lstQuote = _MongoContext.mQuote.AsQueryable().Where(a => lstQRFIDs.Contains(a.QRFID)).ToList();

                var lstEmalis = new List<string>();
                lstEmalis.AddRange(lstQRFPrice.Select(a => a.CostingOfficer).ToList());
                lstEmalis.AddRange(lstQRFPrice.Select(a => a.SalesOfficer).ToList());
                lstEmalis.AddRange(lstQRFPrice.Select(a => a.ProductAccountant).ToList());

                var lstUsers = _MongoContext.mUsers.AsQueryable().Where(a => lstEmalis.Contains(a.Email)).ToList();
                var lstCompaniesIds = lstUsers.Select(a => a.Company_Id).ToList();
                var lstCompanies = _MongoContext.mCompanies.AsQueryable().Where(a => lstCompaniesIds.Contains(a.Company_Id)).ToList();

                foreach (var item in lstBookings)
                {
                    var qrfprice = lstQRFPrice.Where(a => a.QRFID == item.QRFID).FirstOrDefault();
                    var quote = lstQuote.Where(a => a.QRFID == item.QRFID).FirstOrDefault();

                    if (qrfprice != null)
                    {
                        item.AgentInfo.Division_ID = quote?.AgentProductInfo?.DivisionID;
                        item.AgentInfo.Division_Name = quote?.AgentProductInfo?.Division;
                        item.AgentInfo.ParentCompany_Id = quote?.AgentProductInfo?.DivisionID;
                        item.AgentInfo.ParentCompany_Name = quote?.AgentProductInfo?.Division;

                        //if (string.IsNullOrEmpty(item.AgentInfo?.ParentCompany_Id))
                        //{
                        //    item.AgentInfo.Division_ID = quote.AgentProductInfo.DivisionID;
                        //    item.AgentInfo.Division_Name = quote.AgentProductInfo.Division;
                        //    item.AgentInfo.ParentCompany_Id = quote.AgentProductInfo.DivisionID;
                        //    item.AgentInfo.ParentCompany_Name = quote.AgentProductInfo.Division;
                        //}
                        //else
                        //{
                        //    item.AgentInfo.Division_ID = item.AgentInfo?.ParentCompany_Id;
                        //    item.AgentInfo.Division_Name = item.AgentInfo?.ParentCompany_Name;
                        //}

                        item.BusinessType = qrfprice.AgentProductInfo.Type;

                        var SO = lstUsers.Where(a => a.Email == qrfprice.SalesOfficer).FirstOrDefault();
                        var CO = lstUsers.Where(a => a.Email == qrfprice.CostingOfficer).FirstOrDefault();
                        var PA = lstUsers.Where(a => a.Email == qrfprice.ProductAccountant).FirstOrDefault();

                        if (PA != null)
                        {
                            var PAcompany = lstCompanies.Where(a => a.Company_Id == PA.Company_Id).FirstOrDefault();
                            var contact = PAcompany.ContactDetails.Where(a => a.MAIL.ToLower() == PA.Email.ToLower().Trim()).FirstOrDefault();
                            if (contact != null)
                            {
                                item.StaffDetails.Staff_PAUser_Company_Id = PA.Company_Id;
                                item.StaffDetails.Staff_PAUser_Company_Name = PAcompany?.Name;
                                item.StaffDetails.Staff_PAUser_Email = contact.MAIL.ToLower();
                                item.StaffDetails.Staff_PAUser_Id = PA.VoyagerUser_Id;
                                item.StaffDetails.Staff_PAUser_Name = contact.FIRSTNAME + " " + contact.LastNAME;
                            }
                        }

                        if (CO != null)
                        {
                            var COcompany = lstCompanies.Where(a => a.Company_Id == CO.Company_Id).FirstOrDefault();
                            var contact = COcompany.ContactDetails.Where(a => a.MAIL.ToLower() == CO.Email.ToLower().Trim()).FirstOrDefault();

                            if (contact != null)
                            {
                                item.StaffDetails.Staff_SalesSupport_Email = contact.MAIL.ToLower().Trim();
                                item.StaffDetails.Staff_SalesSupport_Id = CO.VoyagerUser_Id;
                                item.StaffDetails.Staff_SalesSupport_Name = contact.FIRSTNAME + " " + contact.LastNAME;
                            }
                        }

                        if (SO != null)
                        {
                            var SOcompany = lstCompanies.Where(a => a.Company_Id == SO.Company_Id).FirstOrDefault();
                            var contact = SOcompany.ContactDetails.Where(a => a.MAIL.ToLower() == SO.Email.ToLower().Trim()).FirstOrDefault();
                            if (contact != null)
                            {
                                item.StaffDetails.Staff_SalesUser_Company_Id = SO.Company_Id;
                                item.StaffDetails.Staff_SalesUser_Company_Name = SOcompany?.Name;
                                item.StaffDetails.Staff_SalesUser_Email = contact.MAIL.ToLower().Trim();
                                item.StaffDetails.Staff_SalesUser_Id = SO.VoyagerUser_Id;
                                item.StaffDetails.Staff_SalesUser_Name = contact.FIRSTNAME + " " + contact.LastNAME;
                            }
                        }
                    }
                    else
                    {
                        item.AgentInfo.Division_ID = item.AgentInfo?.ParentCompany_Id;
                        item.AgentInfo.Division_Name = item.AgentInfo?.ParentCompany_Name;
                        item.BusinessType = "Ad-hoc";
                    }

                    var res = await _MongoContext.Bookings.FindOneAndUpdateAsync(a => a.BookingNumber == item.BookingNumber,
                                          Builders<Bookings>.Update.Set("StaffDetails", item.StaffDetails).
                                          Set("AgentInfo", item.AgentInfo).
                                          Set("BusinessType", item.BusinessType));
                }
            }
            catch (Exception ex)
            {
                response.Status = "Failure";
                response.ErrorMessage = ex.Message;
            }

            return response;
        }
        #endregion
    }
}