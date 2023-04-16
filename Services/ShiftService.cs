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
        public ShiftService(IShiftRepository shiftRepository, IEmployeeInformationRepository employeeInformationRepository, ICustomLogger<ShiftService> logger, 
            IMapper mapper, IConfigurationProvider configuration, IHttpContextAccessor httpContextAccessor, IUserRepository userRepository)
        {
            _shiftRepository = shiftRepository;
            _employeeInformationRepository = employeeInformationRepository;
            _mapper = mapper;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _userRepository = userRepository;
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

                var mappedShiftView = _mapper.Map<ShiftView>(createdShift);
                return StandardResponse<ShiftView>.Ok(mappedShiftView);
            }
            catch (Exception ex)
            {
                return _logger.Error<ShiftView>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<List<ShiftView>>> ListUsersShift(UsersShiftModel model)
        {
            try
            {
                //var allUsers = _userRepository.Query().Include(u => u.EmployeeInformation).Where(user => user.Role.ToLower() == "team member" || user.Role.ToLower() == "internal supervisor" || user.Role.ToLower() == "internal admin");

                //if (!string.IsNullOrEmpty(search))
                //{
                //    allUsers = allUsers.Where(user => user.FirstName.ToLower().Contains(search.ToLower()) || user.LastName.ToLower().Contains(search.ToLower())
                //    || (user.FirstName.ToLower() + " " + user.LastName.ToLower()).Contains(search.ToLower()));
                //}

                var shifts = _shiftRepository.Query().Where(x => x.Start >= model.StartDate && x.End >= model.StartDate && x.Start <= model.EndDate && x.End <= model.EndDate).OrderBy(x => x.Start);

                //var allShift = new List<ShiftView>();
                //foreach(var shift in shifts)
                //{

                //}

                var mappedShift = shifts.ProjectTo<ShiftView>(_configuration).ToList();
                return StandardResponse<List<ShiftView>>.Ok(mappedShift);

                //var usersShifts = new List<UsersShiftView>();

                //foreach (var shift in shifts)
                //{

                //}

                //if (filterUserId.HasValue)
                //{
                //    allUsers = allUsers.Where(user => user.Id == filterUserId.Value);
                //}

                //var pageUsers = allUsers.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value).ToList().AsQueryable();

                //var usersShifts = new List<UsersShiftView>();

                //foreach (var user in pageUsers)
                //{
                //    if (user.IsActive == false) continue;
                //    if (user.EmailConfirmed == false) continue;
                //    var userShift = GetUsersShift(user, model);

                //    usersShifts.Add(userShift);
                //}

                //var pagedCollection = PagedCollection<UsersShiftView>.Create(Link.ToCollection(nameof(ShiftController.ListUsersShift)), usersShifts.ToArray(), allUsers.Count(), pagingOptions);
                //return StandardResponse<PagedCollection<UsersShiftView>>.Ok(pagedCollection);

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
    }
}
