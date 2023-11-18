using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimesheetBE.Controllers;
using TimesheetBE.Models;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Models.IdentityModels;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Repositories.Interfaces;
using TimesheetBE.Services.Interfaces;
using TimesheetBE.Utilities;
using TimesheetBE.Utilities.Abstrctions;
using TimesheetBE.Utilities.Constants;
using TimesheetBE.Utilities.Extentions;

namespace TimesheetBE.Services
{
    public class ShiftService : IShiftService
    {
        private readonly IShiftRepository _shiftRepository;
        private readonly IShiftTypeRepository _shiftTypeRepository;
        private readonly IEmployeeInformationRepository _employeeInformationRepository;
        private readonly ICustomLogger<ShiftService> _logger;
        private readonly IMapper _mapper;
        private readonly IConfigurationProvider _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserRepository _userRepository;
        private readonly ISwapRepository _swapRepository;
        private readonly IEmailHandler _emailHandler;
        private readonly IControlSettingRepository _controlSettingRepository;
        public ShiftService(IShiftRepository shiftRepository, IEmployeeInformationRepository employeeInformationRepository, ICustomLogger<ShiftService> logger, 
            IMapper mapper, IConfigurationProvider configuration, IHttpContextAccessor httpContextAccessor, IUserRepository userRepository, ISwapRepository swapRepository, IEmailHandler emailHandler,
             IShiftTypeRepository shiftTypeRepository, IControlSettingRepository controlSettingRepository)
        {
            _shiftRepository = shiftRepository;
            _shiftTypeRepository = shiftTypeRepository;
            _employeeInformationRepository = employeeInformationRepository;
            _mapper = mapper;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _userRepository = userRepository;
            _swapRepository = swapRepository;
            _emailHandler = emailHandler;
            _controlSettingRepository = controlSettingRepository;
        }

        public async Task<StandardResponse<ShiftTypeView>> CreateShiftType(ShiftTypeModel model)
        {
            try
            {
                Guid UserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();

                var user = _userRepository.Query().FirstOrDefault(x => x.Id == UserId);

                if (user.Role.ToLower() != "super admin")
                {
                    var superAdminSettings = _controlSettingRepository.Query().FirstOrDefault(x => x.SuperAdminId == user.SuperAdminId);

                    if (!superAdminSettings.AdminLeaveManagement) return StandardResponse<ShiftTypeView>.Failed("Shift type configuration is disabled for admins");
                }

                var mappedShiftType = _mapper.Map<ShiftType>(model);
                var createdShiftType = _shiftTypeRepository.CreateAndReturn(mappedShiftType);
                var mappedShiftTypeView = _mapper.Map<ShiftTypeView>(createdShiftType);
                return StandardResponse<ShiftTypeView>.Ok(mappedShiftTypeView);
            }
            catch (Exception ex)
            {
                return _logger.Error<ShiftTypeView>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<List<ShiftTypeView>>> ListShiftTypes(Guid superAdminId)
        {
            try
            {
                var shiftTypes = _shiftTypeRepository.Query().Where(x => x.SuperAdminId == superAdminId);
                var mappedShiftTypes = shiftTypes.ProjectTo<ShiftTypeView>(_configuration).ToList();
                return StandardResponse<List<ShiftTypeView>>.Ok(mappedShiftTypes);
            }
            catch (Exception ex)
            {
                return _logger.Error<List<ShiftTypeView>>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<bool>> UpdateShiftType(ShiftTypeModel model)
        {
            try
            {
                Guid UserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();

                var user = _userRepository.Query().FirstOrDefault(x => x.Id == UserId);

                if (user.Role.ToLower() != "super admin")
                {
                    var superAdminSettings = _controlSettingRepository.Query().FirstOrDefault(x => x.SuperAdminId == user.SuperAdminId);

                    if (!superAdminSettings.AdminLeaveManagement) return StandardResponse<bool>.Failed("Shift type configuration is disabled for admins");
                }

                var shiftType = _shiftTypeRepository.Query().FirstOrDefault(x => x.Id == model.Id);

                if (shiftType == null) return StandardResponse<bool>.NotFound("shift type was not found");

                shiftType.Name = model.Name;
                shiftType.Duration = model.Duration;
                shiftType.Color = model.Color;
                shiftType.Start = model.Start;
                shiftType.End = model.End;

                _shiftTypeRepository.Update(shiftType);
                return StandardResponse<bool>.Ok();
            }
            catch (Exception ex)
            {
                return _logger.Error<bool>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<bool>> DeleteShiftType(Guid id)
        {
            try
            {
                Guid UserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();

                var user = _userRepository.Query().FirstOrDefault(x => x.Id == UserId);

                if (user.Role.ToLower() != "super admin")
                {
                    var superAdminSettings = _controlSettingRepository.Query().FirstOrDefault(x => x.SuperAdminId == user.SuperAdminId);

                    if (!superAdminSettings.AdminLeaveManagement) return StandardResponse<bool>.Failed("Shift type configuration is disabled for admins");
                }

                var shiftType = _shiftTypeRepository.Query().FirstOrDefault(x => x.Id == id);

                if (shiftType == null) return StandardResponse<bool>.NotFound("shift type was not found");

                _shiftTypeRepository.Delete(shiftType);

                return StandardResponse<bool>.Ok();
            }
            catch (Exception ex)
            {
                return _logger.Error<bool>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<bool>> CreateShift(ShiftModel model)
        {
            try
            {
                var superAdmin = _userRepository.Query().Include(x => x.EmployeeInformation).FirstOrDefault(x => x.Id == model.SuperAdminId);

                if (superAdmin == null) return StandardResponse<bool>.Error("Super admin not found");

                var userInfo = _userRepository.Query().Include(x => x.EmployeeInformation).FirstOrDefault(x => x.Id == model.UserId);

                if(userInfo?.EmployeeInformation?.EmployeeType.ToLower() != "shift")
                    return StandardResponse<bool>.Error("This user is not a shift user");

                var shiftType = _shiftTypeRepository.Query().FirstOrDefault(x => x.Id == model.ShiftTypeId);

                if (shiftType == null) return StandardResponse<bool>.Error("Shift type was not found");

                var shiftTypeStartSplit = Array.ConvertAll(shiftType.Start.Split(':'), p => p.Trim());
                var shiftTypeEndSplit = Array.ConvertAll(shiftType.End.Split(':'), p => p.Trim());

                var mappedShift = _mapper.Map<Shift>(model);

                mappedShift.Start = model.Start.Date.AddHours(Convert.ToInt32(shiftTypeStartSplit[0])).AddMinutes(Convert.ToInt32(shiftTypeStartSplit[1]));

                mappedShift.End = model.Start.Date.AddHours(Convert.ToInt32(shiftTypeEndSplit[0])).AddMinutes(Convert.ToInt32(shiftTypeEndSplit[1]));

                mappedShift.Title = shiftType.Name;

                mappedShift.Color = shiftType.Color;

                mappedShift.Hours = shiftType.Duration;

                var createdShift = _shiftRepository.CreateAndReturn(mappedShift);

                if (model.RepeatStopDate.HasValue)
                {
                    var getAllStartDayDayOfWeek = GetWeekdayInRange(model.Start, model.RepeatStopDate, model.Start.DayOfWeek);
                    foreach(var date in getAllStartDayDayOfWeek)
                    {
                        if (_shiftRepository.Query().Any(x => x.Start.Date == date.Date)) continue;
                        //var startHour = TimeSpan.Parse(model.Start.ToString("H:mm")).TotalHours;
                        //var endHour = TimeSpan.Parse(model.End.ToString("H:mm")).TotalHours;

                        //create shift for each week if shift is repeated till the end date

                        var shift = new Shift
                        {
                            UserId = model.UserId,
                            Start = date.Date.AddHours(Convert.ToInt32(shiftTypeStartSplit[0])).AddMinutes(Convert.ToInt32(shiftTypeStartSplit[1])),
                            End = date.Date.AddHours(Convert.ToInt32(shiftTypeEndSplit[0])).AddMinutes(Convert.ToInt32(shiftTypeEndSplit[1])),
                            Hours = shiftType.Duration,
                            Title = shiftType.Name,
                            Color = shiftType.Color,
                            RepeatQuery = model.RepeatQuery,
                            Note = model.Note,
                        };

                        _shiftRepository.CreateAndReturn(shift);
                    }
                    return StandardResponse<bool>.Ok(true);
                }

                //var mappedShiftView = _mapper.Map<ShiftView>(createdShift);
                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                return _logger.Error<bool>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<List<ShiftView>>> ListUsersShift(UsersShiftModel model, bool? isPublished = null)
        {
            try
            {
                var shifts = _shiftRepository.Query().Where(x => x.Start.Date >= model.StartDate && x.End.Date <= model.EndDate && x.User.SuperAdminId == model.SuperAdminId).OrderBy(x => x.Start);

                if(isPublished.HasValue && isPublished == true)
                {
                    shifts = shifts.Where(x => x.IsPublished == true).OrderBy(x => x.Start);
                }   

                if (model.UserId.HasValue)
                {
                    shifts = shifts.Where(x => x.UserId == model.UserId).OrderBy(x => x.Start);
                }

                var mappedShift = shifts.ProjectTo<ShiftView>(_configuration).ToList();
                return StandardResponse<List<ShiftView>>.Ok(mappedShift);

            }
            catch (Exception ex)
            {
                return _logger.Error<List<ShiftView>>(_logger.GetMethodName(), ex);
            }
        }

        public ShiftUsersListView GetUsersAndTotalHours(User user, DateTime StartDate, DateTime EndDate)
        {
            var shifts = _shiftRepository.Query().Where(x => x.UserId == user.Id && x.Start.Date >= StartDate.Date && x.End.Date <= EndDate.Date).ToList();
            
            var shiftHours = shifts.Sum(x => x.Hours);

            //var mappedShift = shifts.ProjectTo<ShiftView>(_configuration).ToList();

            return new ShiftUsersListView { UserId = user.Id, FullName = user.FullName, TotalHours = shiftHours };

        }

        public async Task<StandardResponse<PagedCollection<UsersShiftView>>> GetUsersShift(PagingOptions pagingOptions, UsersShiftModel model)
        {
            try
            {
                var allUsers = _userRepository.Query().Include(u => u.EmployeeInformation).Where(user => (user.Role.ToLower() == "team member" || user.Role.ToLower() == "internal supervisor" || user.Role.ToLower() == "internal admin") && user.SuperAdminId == model.SuperAdminId);

                allUsers = allUsers.Where(x => x.EmployeeInformation.EmployeeType.ToLower() == "shift");
                if (model.UserId.HasValue)
                {
                    allUsers = allUsers.Where(user => user.Id == model.UserId.Value);
                }

                var pageUsers = allUsers.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value).ToList().AsQueryable();

                var usersShifts = new List<UsersShiftView>();

                foreach (var user in pageUsers)
                {
                    if (user.IsActive == false) continue;
                    if (user.EmailConfirmed == false) continue;
                    var userShift = GetUsersShift(user, model);

                    usersShifts.Add(userShift);
                }

                var pagedCollection = PagedCollection<UsersShiftView>.Create(Link.ToCollection(nameof(ShiftController.ListUsersShift)), usersShifts.ToArray(), allUsers.Count(), pagingOptions);
                return StandardResponse<PagedCollection<UsersShiftView>>.Ok(pagedCollection);

            }
            catch (Exception ex)
            {
                return _logger.Error<PagedCollection<UsersShiftView>>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<PagedCollection<ShiftView>>> GetUserShift(PagingOptions pagingOptions, UsersShiftModel model)
        {
            try
            {
                var shifts = _shiftRepository.Query().Where(x => x.UserId == model.UserId && x.Start.Date >= model.StartDate && x.End.Date >= model.StartDate && x.Start.Date <= model.EndDate && x.End.Date <= model.EndDate && x.IsPublished == true).OrderBy(x => x.Start);

                var pageShifts = shifts.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedShift = pageShifts.ProjectTo<ShiftView>(_configuration).ToArray();

                var pagedCollection = PagedCollection<ShiftView>.Create(Link.ToCollection(nameof(ShiftController.ListUsersShift)), mappedShift, shifts.Count(), pagingOptions);

                return StandardResponse<PagedCollection<ShiftView>>.Ok(pagedCollection);

            }
            catch (Exception ex)
            {
                return _logger.Error<PagedCollection<ShiftView>>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<bool>> DeleteShift(Guid id)
        {
            try
            {
                var shift = _shiftRepository.Query().FirstOrDefault(x => x.Id == id);
                if (shift == null)
                    return StandardResponse<bool>.NotFound("Shift not found");
                _shiftRepository.Delete(shift);
                return StandardResponse<bool>.Ok();
            }
            catch (Exception ex)
            {
                return _logger.Error<bool>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<bool>>  PublishShifts(DateTime startDate, DateTime endDate, Guid superAdminId)
        {
            try
            {
                //var shifts = _shiftRepository.Query().Where(x => x.Start.Date >= startDate && x.End.Date >= startDate && x.Start.Date <= endDate 
                //&& x.End.Date <= endDate && x.IsPublished == false).ToList();

                var shifts = _shiftRepository.Query().Where(x => x.Start.Date >= startDate && x.End.Date <= endDate && x.SuperAdminId == superAdminId && x.IsPublished == false).ToList();

                foreach (var shift in shifts)
                {
                    shift.IsPublished = true;
                    shift.DateModified = DateTime.Now;
                    _shiftRepository.Update(shift);
                }
                return StandardResponse<bool>.Ok();
            }
            catch (Exception ex)
            {
                return _logger.Error<bool>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<bool>> SwapShift(ShiftSwapModel model)
        {
            try
            {
                var superAdminSettings = _controlSettingRepository.Query().FirstOrDefault(x => x.SuperAdminId == model.SuperAdminId);

                if(!superAdminSettings.AllowShiftSwapRequest) return StandardResponse<bool>.NotFound("Shift swap is disabled for admins");

                var shift = _shiftRepository.Query().Include(x => x.User).FirstOrDefault(x => x.Id == model.ShiftId);
                if(shift == null)
                    return StandardResponse<bool>.NotFound("Shift not found");

                var shiftToSwap = _shiftRepository.Query().Include(x => x.User).FirstOrDefault(x => x.Id == model.ShiftToSwapId);
                if (shiftToSwap == null)
                    return StandardResponse<bool>.NotFound("Shift not found");

                var swap = _swapRepository.CreateAndReturn(new Swap
                {
                    SwapperId = shiftToSwap.UserId,
                    SwapeeId = shift.UserId,
                    ShiftId = shift.Id,
                    ShiftToSwapId = shiftToSwap.Id,
                    StatusId = (int)Statuses.PENDING,
                });

                //shift.SwapStatusId = (int)Statuses.PENDING;
                //shift.ShiftSwappedId = model.ShiftToSwapId; 

                //shiftToSwap.SwapStatusId = (int)Statuses.PENDING;
                //shiftToSwap.ShiftToSwapId = model.ShiftId;
                //shift.SwapId = swap.Id;
                //shiftToSwap.SwapId = swap.Id;
                shift.DateModified = DateTime.Now;
                shiftToSwap.DateModified = DateTime.Now;
            

                _shiftRepository.Update(shift);

                _shiftRepository.Update(shiftToSwap);

                List<KeyValuePair<string, string>> EmailParameters = new()
                {
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, shiftToSwap.User.FullName),
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_COWORKER, shift.User.FirstName),
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_SHIFTDATE, shiftToSwap.Start.ToString()),
                };

                var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.REQUEST_FOR_LEAVE_FILENAME, EmailParameters);
                var SendEmail = _emailHandler.SendEmail(shift.User.Email, "Request for Shift", EmailTemplate, "");

                return StandardResponse<bool>.Ok();
            }
            catch (Exception ex)
            {
                return _logger.Error<bool>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<PagedCollection<SwapView>>> GetUserSwapShifts(PagingOptions pagingOptions, Guid userId)
        {
            try
            {
                //var shifts = _shiftRepository.Query().Include(x => x.ShiftToSwap).ThenInclude(x => x.User).Include(x => x.ShiftSwapped).ThenInclude(x => x.User).Where(x => x.UserId == userId && x.SwapStatusId  != null).OrderByDescending(x => x.DateModified);
                var swaps = _swapRepository.Query().Include(x => x.Shift).Include(x => x.ShiftToSwap).Where(x => x.SwapperId == userId || x.SwapeeId == userId).OrderByDescending(x => x.DateModified); 
                var waps = swaps.ToList();

                var pageSwaps = swaps.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedSwaps = pageSwaps.ProjectTo<SwapView>(_configuration).ToArray();

                var pagedCollection = PagedCollection<SwapView>.Create(Link.ToCollection(nameof(ShiftController.GetUserSwapShifts)), mappedSwaps, swaps.Count(), pagingOptions);

                return StandardResponse<PagedCollection<SwapView>>.Ok(pagedCollection);

            }
            catch (Exception ex)
            {
                return _logger.Error<PagedCollection<SwapView>>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<PagedCollection<SwapView>>> GetAllSwapShifts(PagingOptions pagingOptions, Guid superAdminId)
        {
            try
            {
                var swaps = _swapRepository.Query().Include(x => x.Shift).ThenInclude(x => x.User).Include(x => x.ShiftToSwap)
                    .Where(x => x.Shift.User.SuperAdminId == superAdminId).OrderByDescending(x => x.DateModified);

                var pageSwaps = swaps.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedSwaps = pageSwaps.ProjectTo<SwapView>(_configuration).ToArray();

                var pagedCollection = PagedCollection<SwapView>.Create(Link.ToCollection(nameof(ShiftController.GetAllSwapShifts)), mappedSwaps, swaps.Count(), pagingOptions);

                return StandardResponse<PagedCollection<SwapView>>.Ok(pagedCollection);

            }
            catch (Exception ex)
            {
                return _logger.Error<PagedCollection<SwapView>>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<bool>> ApproveSwap(Guid id, int action, Guid superAdminId)
        {
            try
            {
                var superAdminSettings = _controlSettingRepository.Query().FirstOrDefault(x => x.SuperAdminId == superAdminId);

                Guid UserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();

                var user = _userRepository.Query().FirstOrDefault(x => x.Id == UserId);

                if(user.Role.ToLower() != "super admin" && !superAdminSettings.AllowShiftSwapApproval) return StandardResponse<bool>.NotFound("Swap approval disabled for admins");
                var swap = _swapRepository.Query().FirstOrDefault(x => x.Id == id);
                var shift = _shiftRepository.Query().Include(x => x.User).FirstOrDefault(x => x.Id == swap.ShiftId);
                var shiftToSwap = _shiftRepository.Query().Include(x => x.User).FirstOrDefault(x => x.Id == swap.ShiftToSwapId);
                var supervisor = _employeeInformationRepository.Query().Include(x => x.Supervisor).FirstOrDefault(x => x.UserId == shiftToSwap.UserId);
                if (shift == null)
                    return StandardResponse<bool>.NotFound("Shift not found");
                if(action == 1 && swap.StatusId == (int)Statuses.PENDING)
                {
                    swap.StatusId = (int)Statuses.APPROVED;
                    //shiftToSwap.SwapStatusId = (int)Statuses.APPROVED;
                    
                    swap.DateModified = DateTime.Now;
                    //shift.DateModified = DateTime.Now;
                    //shiftToSwap.DateModified = DateTime.Now;

                }
                else if(action == 2 && swap.StatusId == (int)Statuses.APPROVED)
                {
                    swap.IsApproved = true;
                    shift.Start = shiftToSwap.Start;
                    shift.End = shiftToSwap.End;
                    shift.Hours = shiftToSwap.Hours;
                    shift.Title = shiftToSwap.Title;
                    shift.Color = shiftToSwap.Color;
                    shift.RepeatQuery = shiftToSwap.RepeatQuery;
                    shift.Note = shiftToSwap.Note;
                    shift.DateModified = DateTime.Now;

                    shiftToSwap.Start = shift.Start;
                    shiftToSwap.End = shift.End;
                    shiftToSwap.Hours = shift.Hours;
                    shiftToSwap.Title = shift.Title;
                    shiftToSwap.Color = shift.Color;
                    shiftToSwap.RepeatQuery = shift.RepeatQuery;
                    shiftToSwap.Note = shift.Note;
                    shiftToSwap.DateModified = DateTime.Now;
                }
                else
                {
                    swap.StatusId = (int)Statuses.DECLINED;
                    swap.DateModified = DateTime.Now;
                }


                _swapRepository.Update(swap);
                _shiftRepository.Update(shift);
                _shiftRepository.Update(shiftToSwap);
                if(action == 2 && swap.StatusId == (int)Statuses.APPROVED)
                {
                    List<KeyValuePair<string, string>> EmailParameters = new()
                    {
                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, shiftToSwap.User.FirstName),
                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_COWORKER, shift.User.FirstName),
                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_SHIFTSTARTTIME, shift.Start.ToString()),
                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_SHIFTENDTIME, shift.End.ToString()),
                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_MANAGER, supervisor.Supervisor.FullName),
                    };

                    List<KeyValuePair<string, string>> EmailParams = new()
                    {
                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, shift.User.FirstName),
                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_SHIFTSTARTTIME, shiftToSwap.Start.ToString()),
                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_SHIFTENDTIME, shiftToSwap.End.ToString()),
                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_MANAGER, supervisor.Supervisor.FullName),
                    };

                    var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.REQUEST_FOR_LEAVE_FILENAME, EmailParameters);
                    var SendEmail = _emailHandler.SendEmail(shiftToSwap.User.Email, "Request for Shift", EmailTemplate, "");
                }
                return StandardResponse<bool>.Ok();
            }
            catch (Exception ex)
            {
                return _logger.Error<bool>(_logger.GetMethodName(), ex);
            }
        }

        private UsersShiftView GetUsersShift(User user, UsersShiftModel model)
        {
            var shifts = _shiftRepository.Query().Where(x => x.UserId == user.Id && x.Start >= model.StartDate && x.End >= model.StartDate && x.Start <= model.EndDate && x.End <= model.EndDate && x.IsPublished == true);

            var shiftHours = shifts.Sum(x => x.Hours);

            var mappedShift = shifts.ProjectTo<ShiftView>(_configuration).ToList();

            return new UsersShiftView { UserId = user.Id, FullName = user.FullName, TotalHours = shiftHours, StartDate = model.StartDate, EndDate = model.EndDate };

        }

        private List<DateTime> GetWeekdayInRange(DateTime from, DateTime? to, DayOfWeek day)
        {
            const int daysInWeek = 7;
            var result = new List<DateTime>();
            var daysToAdd = ((int)day - (int)from.DayOfWeek + daysInWeek) % daysInWeek;

            do
            {
                from = from.AddDays(daysToAdd);
                result.Add(from);
                daysToAdd = daysInWeek;
            } while (from < to);

            return result;
        }
    }
}
