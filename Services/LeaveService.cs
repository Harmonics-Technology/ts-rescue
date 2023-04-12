using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TimesheetBE.Controllers;
using TimesheetBE.Models;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Repositories.Interfaces;
using TimesheetBE.Services.Interfaces;
using TimesheetBE.Utilities;
using TimesheetBE.Utilities.Abstrctions;

namespace TimesheetBE.Services
{
    public class LeaveService : ILeaveService
    {
        private readonly ILeaveTypeRepository _leaveTypeRepository;
        private readonly ILeaveRepository _leaveRepository;
        private readonly IEmployeeInformationRepository _employeeInformationRepository;
        private readonly ICustomLogger<LeaveService> _logger;
        private readonly IMapper _mapper;
        private readonly IConfigurationProvider _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public LeaveService(ILeaveTypeRepository leaveTypeRepository, ILeaveRepository leaveRepository, IMapper mapper, IConfigurationProvider configuration,
            ICustomLogger<LeaveService> logger, IHttpContextAccessor httpContextAccessor, IEmployeeInformationRepository employeeInformationRepository)
        {
            _leaveTypeRepository = leaveTypeRepository;
            _leaveRepository = leaveRepository;
            _mapper = mapper;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _employeeInformationRepository = employeeInformationRepository;
        }

        //Create leave type
        public async Task<StandardResponse<LeaveTypeView>> AddLeaveType(LeaveTypeModel model)
        {
            try
            {
                var mappedLeaveTypeInput = _mapper.Map<LeaveType>(model);
                var createdLeaveType = _leaveTypeRepository.CreateAndReturn(mappedLeaveTypeInput);
                var mappedLeaveType = _mapper.Map<LeaveTypeView>(createdLeaveType);
                return StandardResponse<LeaveTypeView>.Ok(mappedLeaveType);
            }
            catch (Exception ex)
            {
                return _logger.Error<LeaveTypeView>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<LeaveTypeView>> UpdateLeaveType(Guid id, LeaveTypeModel model)
        {
            try
            {
                var leaveType = _leaveTypeRepository.Query().FirstOrDefault(x => x.Id == id);
                if (leaveType == null)
                    return StandardResponse<LeaveTypeView>.NotFound("Leave type not found");
                leaveType.Name = model.Name;
                leaveType.LeaveTypeIcon = model.LeaveTypeIcon;
                leaveType.DateModified = DateTime.Now;
                var updateLeaveType = _leaveTypeRepository.Update(leaveType);
                var mappedLeaveType = _mapper.Map<LeaveTypeView>(updateLeaveType);
                return StandardResponse<LeaveTypeView>.Ok(mappedLeaveType);
            }
            catch (Exception ex)
            {
                return _logger.Error<LeaveTypeView>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<bool>> DeleteLeaveType(Guid id)
        {
            try
            {
                var leaveType = _leaveTypeRepository.Query().FirstOrDefault(x => x.Id == id);
                if (leaveType == null)
                    return StandardResponse<bool>.NotFound("Leave type not found");
                _leaveTypeRepository.Delete(leaveType);
                return StandardResponse<bool>.Ok();
            }
            catch (Exception ex)
            {
                return _logger.Error<bool>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<PagedCollection<LeaveTypeView>>> LeaveTypes(PagingOptions pagingOptions)
        {
            try
            {
                var leaveTypes = _leaveTypeRepository.Query();

                var pagedLeaveTypes = leaveTypes.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedLeaveTypes = leaveTypes.AsQueryable().ProjectTo<LeaveTypeView>(_configuration).ToArray();

                var pagedCollection = PagedCollection<LeaveTypeView>.Create(Link.ToCollection(nameof(LeaveController.LeaveTypes)), mappedLeaveTypes, leaveTypes.Count(), pagingOptions);

                return StandardResponse<PagedCollection<LeaveTypeView>>.Ok(pagedCollection);
            }
            catch (Exception ex)
            {
                return _logger.Error<PagedCollection<LeaveTypeView>>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<LeaveView>> CreateLeave(LeaveModel model)
        {
            try
            {
                var employeeInformation = _employeeInformationRepository.Query().FirstOrDefault(x => x.Id == model.EmployeeInformationId);
                if (employeeInformation.IsEligibleForLeave == false)
                    return StandardResponse<LeaveView>.NotFound("You are not eligible for leave");
                var mappedLeave = _mapper.Map<Leave>(model);
                mappedLeave.StatusId = (int)Statuses.PENDING;
                var createdLeave = _leaveRepository.CreateAndReturn(mappedLeave);
                var mappedLeaveView = _mapper.Map<LeaveView>(createdLeave);
                return StandardResponse<LeaveView>.Ok(mappedLeaveView);
            }
            catch (Exception ex)
            {
                return _logger.Error<LeaveView>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<PagedCollection<LeaveView>>> ListLeaves(PagingOptions pagingOptions, string search = null, DateFilter dateFilter = null)
        {
            var leaves = _leaveRepository.Query().Include(x => x.EmployeeInformation).ThenInclude(x => x.User).OrderByDescending(x => x.DateCreated);

            if (dateFilter.StartDate.HasValue)
                leaves = leaves.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

            if (dateFilter.EndDate.HasValue)
                leaves = leaves.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);

            if (!string.IsNullOrEmpty(search))
                leaves = leaves.Where(x => x.LeaveType.Name.Contains(search)).OrderByDescending(u => u.DateCreated);

            var pagedLeaves = leaves.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

            var mappedLeaves = pagedLeaves.ProjectTo<LeaveView>(_configuration).ToArray();
            var pagedCollection = PagedCollection<LeaveView>.Create(Link.ToCollection(nameof(LeaveController.ListLeaves)), mappedLeaves, pagedLeaves.Count(), pagingOptions);

            return StandardResponse<PagedCollection<LeaveView>>.Ok(pagedCollection);
        }

        public async Task<StandardResponse<bool>> TreatLeave(Guid leaveId, LeaveStatuses status)
        {
            try
            {
                if ((int)status == 0)
                    return StandardResponse<bool>.Failed("Invalid action");

                var leave = _leaveRepository.Query().FirstOrDefault(x => x.Id == leaveId);
                switch (status)
                {
                    case LeaveStatuses.Approved:
                        leave.StatusId = (int)Statuses.APPROVED;
                        _leaveRepository.Update(leave);
                        return StandardResponse<bool>.Ok(true);
                        break;
                    case LeaveStatuses.Declined:
                        leave.StatusId = (int)Statuses.DECLINED;
                        _leaveRepository.Update(leave);
                        return StandardResponse<bool>.Ok(true);
                    default:
                        return StandardResponse<bool>.Failed("An error occured");

                }

                return StandardResponse<bool>.Failed("An error occured");
            }
            catch (Exception ex)
            {
                return _logger.Error<bool>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<bool>> DeleteLeave(Guid id)
        {
            try
            {
                var leave = _leaveRepository.Query().FirstOrDefault(x => x.Id == id);
                if (leave == null)
                    return StandardResponse<bool>.NotFound("Leave type not found");
                _leaveRepository.Delete(leave);
                return StandardResponse<bool>.Ok();
            }
            catch (Exception ex)
            {
                return _logger.Error<bool>(_logger.GetMethodName(), ex);
            }
        }

    }
}
