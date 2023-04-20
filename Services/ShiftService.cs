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

namespace TimesheetBE.Services
{
    public class ShiftService : IShiftService
    {
        private readonly IShiftRepository _shiftRepository;
        private readonly IEmployeeInformationRepository _employeeInformationRepository;
        private readonly ICustomLogger<ShiftService> _logger;
        private readonly IMapper _mapper;
        private readonly IConfigurationProvider _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserRepository _userRepository;
        private readonly ISwapRepository _swapRepository;
        public ShiftService(IShiftRepository shiftRepository, IEmployeeInformationRepository employeeInformationRepository, ICustomLogger<ShiftService> logger, 
            IMapper mapper, IConfigurationProvider configuration, IHttpContextAccessor httpContextAccessor, IUserRepository userRepository, ISwapRepository swapRepository)
        {
            _shiftRepository = shiftRepository;
            _employeeInformationRepository = employeeInformationRepository;
            _mapper = mapper;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _userRepository = userRepository;
            _swapRepository = swapRepository;
        }

        public async Task<StandardResponse<ShiftView>> CreateShift(ShiftModel model)
        {
            try
            {
                var userInfo = _userRepository.Query().Include(x => x.EmployeeInformation).FirstOrDefault(x => x.Id == model.UserId);

                if(userInfo?.EmployeeInformation?.EmployeeType.ToLower() != "shift")
                    return StandardResponse<ShiftView>.Error("This user is not a shift user");

                var mappedShift = _mapper.Map<Shift>(model);
                var createdShift = _shiftRepository.CreateAndReturn(mappedShift);

                if (model.RepeatStopDate.HasValue)
                {
                    var getAllStartDayDayOfWeek = GetWeekdayInRange(model.Start, model.RepeatStopDate, model.Start.DayOfWeek);
                    foreach(var date in getAllStartDayDayOfWeek)
                    {
                        if (_shiftRepository.Query().Any(x => x.Start.Date == date.Date)) continue;
                        var startHour = TimeSpan.Parse(model.Start.ToString("H:mm")).TotalHours;
                        var endHour = TimeSpan.Parse(model.End.ToString("H:mm")).TotalHours;

                        //create shift for each week if shift is repeated till the end date

                        var shift = new Shift
                        {
                            UserId = model.UserId,
                            Start = date.Date.AddHours(startHour),
                            End = date.Date.AddHours(endHour),
                            Hours = model.Hours,
                            Title = model.Title,
                            Color = model.Color,
                            RepeatQuery = model.RepeatQuery,
                            Note = model.Note,
                        };

                        _shiftRepository.CreateAndReturn(shift);
                    }
                }

                var mappedShiftView = _mapper.Map<ShiftView>(createdShift);
                return StandardResponse<ShiftView>.Ok(mappedShiftView);
            }
            catch (Exception ex)
            {
                return _logger.Error<ShiftView>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<List<ShiftView>>> ListUsersShift(UsersShiftModel model, bool? isPublished = null)
        {
            try
            {
                var shifts = _shiftRepository.Query().Where(x => x.Start.Date >= model.StartDate && x.End.Date >= model.StartDate && x.Start.Date <= model.EndDate && x.End.Date <= model.EndDate).OrderBy(x => x.Start);

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
            var shifts = _shiftRepository.Query().Where(x => x.UserId == user.Id && x.Start >= StartDate && x.End >= StartDate && x.Start <= EndDate && x.End <= EndDate);
            
            var shiftHours = shifts.Sum(x => x.Hours);

            var mappedShift = shifts.ProjectTo<ShiftView>(_configuration).ToList();

            return new ShiftUsersListView { UserId = user.Id, FullName = user.FullName, TotalHours = shiftHours };

        }

        public async Task<StandardResponse<PagedCollection<UsersShiftView>>> GetUsersShift(PagingOptions pagingOptions, UsersShiftModel model)
        {
            try
            {
                var allUsers = _userRepository.Query().Include(u => u.EmployeeInformation).Where(user => user.Role.ToLower() == "team member" || user.Role.ToLower() == "internal supervisor" || user.Role.ToLower() == "internal admin");

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

        public async Task<StandardResponse<bool>>  PublishShifts(DateTime startDate, DateTime endDate)
        {
            try
            {
                var shifts = _shiftRepository.Query().Where(x => x.Start.Date >= startDate && x.End.Date >= startDate && x.Start.Date <= endDate 
                && x.End.Date <= endDate && x.IsPublished == false).ToList();

                foreach(var shift in shifts)
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
                var shift = _shiftRepository.Query().Include(x => x.User).FirstOrDefault(x => x.Id == model.ShiftId);
                if(shift == null)
                    return StandardResponse<bool>.NotFound("Shift not found");

                var shiftToSwap = _shiftRepository.Query().Include(x => x.User).FirstOrDefault(x => x.Id == model.ShiftToSwapId);
                if (shift == null)
                    return StandardResponse<bool>.NotFound("Shift not found");

                _swapRepository.CreateAndReturn(new Swap
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
                shift.DateModified = DateTime.Now;
                shiftToSwap.DateModified = DateTime.Now;

                _shiftRepository.Update(shift);

                _shiftRepository.Update(shiftToSwap);

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
                var swaps = _swapRepository.Query().Include(x => x.Shift).Include(x => x.ShiftToSwap).Include(x => x.Swapee).Include(x => x.Swapper)
                    .Where(x => x.SwapperId == userId || x.SwapeeId == userId).OrderByDescending(x => x.DateModified); 

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

        public async Task<StandardResponse<PagedCollection<SwapView>>> GetAllSwapShifts(PagingOptions pagingOptions)
        {
            try
            {
                var swaps = _swapRepository.Query().Include(x => x.Shift).Include(x => x.ShiftToSwap).Include(x => x.Swapee).Include(x => x.Swapper).OrderByDescending(x => x.DateModified);

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

        public async Task<StandardResponse<bool>> ApproveSwap(Guid id, int action)
        {
            try
            {
                var swap = _swapRepository.Query().FirstOrDefault(x => x.Id == id);
                var shift = _shiftRepository.Query().FirstOrDefault(x => x.Id == swap.ShiftId);
                var shiftToSwap = _shiftRepository.Query().FirstOrDefault(x => x.Id == swap.ShiftToSwapId);
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
