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
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Repositories.Interfaces;
using TimesheetBE.Services.Interfaces;
using TimesheetBE.Utilities;
using TimesheetBE.Utilities.Abstrctions;
using TimesheetBE.Utilities.Constants;

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
        private readonly ITimeSheetRepository _timeSheetRepository;
        private readonly IEmailHandler _emailHandler;
        private readonly IUserRepository _userRepository;
        private readonly INotificationRepository _notificationRepository;
        public LeaveService(ILeaveTypeRepository leaveTypeRepository, ILeaveRepository leaveRepository, IMapper mapper, IConfigurationProvider configuration,
            ICustomLogger<LeaveService> logger, IHttpContextAccessor httpContextAccessor, IEmployeeInformationRepository employeeInformationRepository, 
            ITimeSheetRepository timeSheetRepository, IEmailHandler emailHandler, IUserRepository userRepository, INotificationRepository notificationRepository)
        {
            _leaveTypeRepository = leaveTypeRepository;
            _leaveRepository = leaveRepository;
            _mapper = mapper;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _employeeInformationRepository = employeeInformationRepository;
            _timeSheetRepository = timeSheetRepository;
            _emailHandler = emailHandler;
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
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
                var employeeInformation = _employeeInformationRepository.Query().Include(x => x.User).Include(x => x.Supervisor).FirstOrDefault(x => x.Id == model.EmployeeInformationId);
                var assignee = _userRepository.Query().FirstOrDefault(x => x.Id == model.WorkAssigneeId);

                var mappedLeave = _mapper.Map<Leave>(model);
                mappedLeave.StatusId = (int)Statuses.PENDING;
                var createdLeave = _leaveRepository.CreateAndReturn(mappedLeave);

                List<KeyValuePair<string, string>> EmailParameters = new()
                {
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, employeeInformation.User.FirstName),
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_COWORKER, employeeInformation.Supervisor.FirstName),
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_LEAVESTARTDATE, model.StartDate.Date.ToString()),
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_LEAVEENDDATE, model.EndDate.Date.ToString()),
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_WORK_ASSIGNEE, assignee.FullName),
                };

                var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.REQUEST_FOR_LEAVE_FILENAME, EmailParameters);
                var SendEmail = _emailHandler.SendEmail(employeeInformation.Supervisor.Email, "Leave Request Notification", EmailTemplate, "");

                var noOfLeaveDaysEligible = GetEligibleLeaveDays(employeeInformation.Id);
                var noOfLeaveDaysLeft = noOfLeaveDaysEligible - employeeInformation.NumberOfEligibleLeaveDaysTaken;

                _notificationRepository.CreateAndReturn(new Notification { UserId = employeeInformation.UserId, Message = $"Your leave request has been sent for approval. You have {noOfLeaveDaysLeft} days left for the year", IsRead = false });

                var mappedLeaveView = _mapper.Map<LeaveView>(createdLeave);
                return StandardResponse<LeaveView>.Ok(mappedLeaveView);
            }
            catch (Exception ex)
            {
                return _logger.Error<LeaveView>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<PagedCollection<LeaveView>>> ListLeaves(PagingOptions pagingOptions, Guid? supervisorId = null, Guid? employeeInformationId = null, string search = null, DateFilter dateFilter = null)
        {
            var leaves = _leaveRepository.Query().Include(x => x.LeaveType).Include(x => x.EmployeeInformation).ThenInclude(x => x.User).OrderByDescending(x => x.DateCreated);
            if (supervisorId.HasValue)
                leaves = leaves.Where(x => x.EmployeeInformation.SupervisorId == supervisorId).OrderByDescending(u => u.DateCreated);

            if (employeeInformationId.HasValue)
                leaves = leaves.Where(x => x.EmployeeInformationId == employeeInformationId).OrderByDescending(u => u.DateCreated);

            if (dateFilter.StartDate.HasValue)
                leaves = leaves.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

            if (dateFilter.EndDate.HasValue)
                leaves = leaves.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);

            if (!string.IsNullOrEmpty(search))
                leaves = leaves.Where(x => x.LeaveType.Name.Contains(search)).OrderByDescending(u => u.DateCreated);

            var pagedLeaves = leaves.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);
            

            var mappedLeaves = pagedLeaves.ProjectTo<LeaveView>(_configuration).ToArray();

            foreach (var leave in mappedLeaves)
            {
                leave.LeaveDaysEarned = GetEligibleLeaveDays(leave?.EmployeeInformationId);
            }
            var pagedCollection = PagedCollection<LeaveView>.Create(Link.ToCollection(nameof(LeaveController.ListLeaves)), mappedLeaves, pagedLeaves.Count(), pagingOptions);

            return StandardResponse<PagedCollection<LeaveView>>.Ok(pagedCollection);
        }

        public async Task<StandardResponse<bool>> TreatLeave(Guid leaveId, LeaveStatuses status)
        {
            try
            {
                if ((int)status == 0)
                    return StandardResponse<bool>.Failed("Invalid action");

                var leave = _leaveRepository.Query().Include(x => x.EmployeeInformation).ThenInclude(x => x.User).FirstOrDefault(x => x.Id == leaveId);
                var supervisor = _userRepository.Query().FirstOrDefault(x => x.Id == leave.EmployeeInformation.SupervisorId);
                var assignee = _userRepository.Query().FirstOrDefault(x => x.Id == leave.WorkAssigneeId);
                //var nOfDaysApplied = (leave.EndDate.Date - leave.StartDate.Date).Days;
                switch (status)
                {
                    case LeaveStatuses.Approved:
                        leave.StatusId = (int)Statuses.APPROVED;
                        leave.ApprovalDate = DateTime.Now;
                        _leaveRepository.Update(leave);
                        List<KeyValuePair<string, string>> EmailParameters = new()
                        {
                            new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, supervisor.FullName),
                            new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_COWORKER, leave.EmployeeInformation.User.FirstName),
                            new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_LEAVESTARTDATE, leave.StartDate.Date.ToString()),
                            new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_LEAVEENDDATE, leave.EndDate.Date.ToString()),
                        };

                        List<KeyValuePair<string, string>> EmailParams = new()
                        {
                            new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, leave.EmployeeInformation.User.FullName),
                            new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_COWORKER, assignee.FirstName),
                            new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_LEAVESTARTDATE, leave.StartDate.Date.ToString()),
                            new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_LEAVEENDDATE, leave.EndDate.Date.ToString()),
                        };

                        var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.LEAVE_APPROVAL_FILENAME, EmailParameters);
                        var SendEmail = _emailHandler.SendEmail(leave.EmployeeInformation.User.Email, "Leave Approval Notification", EmailTemplate, "");

                        //sent to assignee
                        var EmailTemplateForAssignee = _emailHandler.ComposeFromTemplate(Constants.LEAVE_APPROVAL_WORK_ASSIGNEE_FILENAME, EmailParameters);
                        var SendEmailToAssignee = _emailHandler.SendEmail(assignee.Email, "Leave Approval Notification", EmailTemplate, "");

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

        public int GetEligibleLeaveDays(Guid? employeeInformationId)
        {
            var employee = _employeeInformationRepository.Query().FirstOrDefault(x => x.Id == employeeInformationId);
            var firstTimeSheetInTheYear = _timeSheetRepository.Query().FirstOrDefault(x => x.EmployeeInformationId == employeeInformationId && x.Date.Year == DateTime.Now.Year);
            var lastTimeSheetInTheYear = _timeSheetRepository.Query().Where(x => x.EmployeeInformationId == employeeInformationId).OrderBy(x => x.Date).LastOrDefault();
            if (firstTimeSheetInTheYear == null) return 0;
            if(lastTimeSheetInTheYear == null) return 0;
            var noOfMonthWorked = (int)((lastTimeSheetInTheYear.Date.Year - firstTimeSheetInTheYear.Date.Year) * 12) + lastTimeSheetInTheYear.Date.Month - firstTimeSheetInTheYear.Date.Month;
            if (employee == null) return 0;
            if (employee.NumberOfDaysEligible == null) return 0;

            var noOfDays = (employee.NumberOfDaysEligible / 12) * noOfMonthWorked;
            return (int)noOfDays;
        }

    }
}
