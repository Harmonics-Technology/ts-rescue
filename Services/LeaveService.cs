using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        private readonly ILeaveConfigurationRepository _leaveConfigurationRepository;
        private readonly UserManager<User> _userManager;
        private readonly IControlSettingRepository _controlSettingRepository;
        private readonly IContractRepository _contractRepository;
        public LeaveService(ILeaveTypeRepository leaveTypeRepository, ILeaveRepository leaveRepository, IMapper mapper, IConfigurationProvider configuration,
            ICustomLogger<LeaveService> logger, IHttpContextAccessor httpContextAccessor, IEmployeeInformationRepository employeeInformationRepository, 
            ITimeSheetRepository timeSheetRepository, IEmailHandler emailHandler, IUserRepository userRepository, INotificationRepository notificationRepository,
            ILeaveConfigurationRepository leaveConfigurationRepository, UserManager<User> userManager, IControlSettingRepository controlSettingRepository,
            IContractRepository contractRepository)
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
            _leaveConfigurationRepository = leaveConfigurationRepository;
            _userManager = userManager;
            _controlSettingRepository = controlSettingRepository;
            _contractRepository = contractRepository;
        }

        //Add Leave Configuration
        public async Task<StandardResponse<LeaveConfigurationView>> AddLeaveConfiguration(LeaveConfigurationModel model)
        {
            try
            {
                Guid UserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();

                var mappedLeaveConfiguration = _mapper.Map<LeaveConfiguration>(model);
                var createdModel = _leaveConfigurationRepository.CreateAndReturn(mappedLeaveConfiguration);

                var superAdmin = _userRepository.Query().FirstOrDefault(x => x.Id == model.SuperAdminId);

                superAdmin.LeaveConfigurationId = createdModel.Id;

                var up = _userManager.UpdateAsync(superAdmin).Result;
                if (!up.Succeeded)
                    return StandardResponse<LeaveConfigurationView>.Failed(up.Errors.FirstOrDefault().Description);

                var mappedView = _mapper.Map<LeaveConfigurationView>(createdModel);
                return StandardResponse<LeaveConfigurationView>.Ok(mappedView);
            }
            catch (Exception ex)
            {
                return _logger.Error<LeaveConfigurationView>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<LeaveConfigurationView>> GetLeaveConfiguration(Guid superAdminId)
        {
            try
            {
                var leaveConfiguration = _leaveConfigurationRepository.Query().FirstOrDefault(x => x.SuperAdminId == superAdminId);
                if (leaveConfiguration == null)
                    return StandardResponse<LeaveConfigurationView>.NotFound("Configuration not found");
                var mappedView = _mapper.Map<LeaveConfigurationView>(leaveConfiguration);
                return StandardResponse<LeaveConfigurationView>.Ok(mappedView);
            }
            catch (Exception ex)
            {
                return _logger.Error<LeaveConfigurationView>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<bool>> UpdateLeaveConfiguration(LeaveConfigurationModel model)
        {
            try
            {
                Guid UserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();

                var user = _userRepository.Query().FirstOrDefault(x => x.Id == UserId);

                var superAdminSettings = _controlSettingRepository.Query().FirstOrDefault(x => x.SuperAdminId == model.SuperAdminId);

                if (!superAdminSettings.AdminLeaveManagement && user.Role.ToLower() != "super admin") return StandardResponse<bool>.Failed("Leave configuration is disabled for admins");

                var leaveConfiguration = _leaveConfigurationRepository.Query().FirstOrDefault(x => x.Id == model.Id);
                if (leaveConfiguration == null)
                    return StandardResponse<bool>.NotFound("Configuration not found");

                if(model.EligibleLeaveDays.HasValue) leaveConfiguration.EligibleLeaveDays = model.EligibleLeaveDays.Value;
                if(model.IsStandardEligibleDays.HasValue) leaveConfiguration.IsStandardEligibleDays = model.IsStandardEligibleDays.Value;
                
                _leaveConfigurationRepository.Update(leaveConfiguration);

                return StandardResponse<bool>.Ok();
            }
            catch (Exception ex)
            {
                return _logger.Error<bool>(_logger.GetMethodName(), ex);
            }
        }

        //Create leave type
        public async Task<StandardResponse<LeaveTypeView>> AddLeaveType(LeaveTypeModel model)
        {
            try
            {
                Guid UserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();

                var user = _userRepository.Query().FirstOrDefault(x => x.Id == UserId);

                var superAdminSettings = _controlSettingRepository.Query().FirstOrDefault(x => x.SuperAdminId == model.superAdminId);

                if (!superAdminSettings.AdminLeaveManagement && user.Role.ToLower() != "super admin") return StandardResponse<LeaveTypeView>.Failed("Leave type configuration is disabled for admins");
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
                Guid UserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();

                var user = _userRepository.Query().FirstOrDefault(x => x.Id == UserId);

                var superAdminSettings = _controlSettingRepository.Query().FirstOrDefault(x => x.SuperAdminId == model.superAdminId);

                if (!superAdminSettings.AdminLeaveManagement && user.Role.ToLower() != "super admin") return StandardResponse<LeaveTypeView>.Failed("Leave type configuration is disabled for admins");

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
                Guid UserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();

                var user = _userRepository.Query().FirstOrDefault(x => x.Id == UserId);

                if(user.Role.ToLower() != "super admin")
                {
                    var superAdminSettings = _controlSettingRepository.Query().FirstOrDefault(x => x.SuperAdminId == user.SuperAdminId);

                    if (!superAdminSettings.AdminLeaveManagement) return StandardResponse<bool>.Failed("Leave type configuration is disabled for admins");
                }

                
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

        public async Task<StandardResponse<PagedCollection<LeaveTypeView>>> LeaveTypes(PagingOptions pagingOptions, Guid superAdminId)
        {
            try
            {
                var leaveTypes = _leaveTypeRepository.Query().Where(x => x.superAdminId == superAdminId);

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

                var configuration = _leaveConfigurationRepository.Query().FirstOrDefault(x => x.SuperAdminId == employeeInformation.User.SuperAdminId);

                if (configuration == null) return StandardResponse<LeaveView>.Failed("Leave configuration not available");

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
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_LEAVEDAYSAPPLIED, model.NoOfLeaveDaysApplied.ToString())
                };

                var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.REQUEST_FOR_LEAVE_FILENAME, EmailParameters);
                var SendEmail = _emailHandler.SendEmail(employeeInformation.Supervisor.Email, "Leave Request Notification", EmailTemplate, "");

                var noOfLeaveDaysEligible = GetEligibleLeaveDays(employeeInformation.Id);
                var noOfLeaveDaysLeft = noOfLeaveDaysEligible - employeeInformation.NumberOfEligibleLeaveDaysTaken;

                _notificationRepository.CreateAndReturn(new Notification { UserId = employeeInformation.UserId, Type = "Leave Request", Message = $"Your leave request has been sent for approval. You have {noOfLeaveDaysLeft} days left for the year", IsRead = false });

                var mappedLeaveView = _mapper.Map<LeaveView>(createdLeave);
                return StandardResponse<LeaveView>.Ok(mappedLeaveView);
            }
            catch (Exception ex)
            {
                return _logger.Error<LeaveView>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<bool>> UpdateLeave(LeaveModel model)
        {
            try
            {
                var leave = _leaveRepository.Query().FirstOrDefault(x => x.Id == model.Id);

                if (leave == null) return StandardResponse<bool>.Failed("Leave not found");

                //if(leave.StatusId == (int)Statuses.APPROVED) return StandardResponse<bool>.Failed("You cant update this leave as it as already been approved");

                leave.LeaveTypeId = model.LeaveTypeId;
                leave.StartDate = model.StartDate;
                leave.EndDate = model.EndDate;
                leave.ReasonForLeave = model.ReasonForLeave;
                leave.WorkAssigneeId = model.WorkAssigneeId.HasValue ? model.WorkAssigneeId.Value : null;

                _leaveRepository.Update(leave);


                var employeeInformation = _employeeInformationRepository.Query().Include(x => x.User).Include(x => x.Supervisor).FirstOrDefault(x => x.Id == model.EmployeeInformationId);


                var noOfLeaveDaysEligible = GetEligibleLeaveDays(employeeInformation.Id);
                var noOfLeaveDaysLeft = noOfLeaveDaysEligible - employeeInformation.NumberOfEligibleLeaveDaysTaken;

                _notificationRepository.CreateAndReturn(new Notification { UserId = employeeInformation.UserId, Message = $"Your leave request has been updaated. You have {noOfLeaveDaysLeft} days left for the year", IsRead = false });

                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                return _logger.Error<bool>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<bool>> CancelLeave(Guid leaveId)
        {
            try
            {
                var leave = _leaveRepository.Query().FirstOrDefault(x => x.Id == leaveId);

                if (leave == null) return StandardResponse<bool>.Failed("Leave not found");

                if(leave.StatusId == (int)Statuses.PENDING)
                {
                    leave.StatusId = (int)Statuses.CANCELED;

                    leave.IsCanceled = true;

                    _leaveRepository.Update(leave);

                    return StandardResponse<bool>.Ok(true);

                }

                if(leave.StatusId == (int)Statuses.APPROVED && DateTime.Now.Date >= leave.StartDate.Date) return StandardResponse<bool>.Failed("You cannot cancel a leave you started");

                leave.IsCanceled = true;

                leave.StatusId = (int)Statuses.REVIEWING;

                _leaveRepository.Update(leave);

                var employeeInformation = _employeeInformationRepository.Query().Include(x => x.User).Include(x => x.Supervisor).FirstOrDefault(x => x.Id == leave.EmployeeInformationId);

                List<KeyValuePair<string, string>> EmailParameters = new()
                {
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, employeeInformation.User.FirstName),
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_COWORKER, employeeInformation.Supervisor.FirstName),
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_LEAVESTARTDATE, leave.StartDate.Date.ToString()),
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_LEAVEENDDATE, leave.EndDate.Date.ToString()),
                };

                var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.LEAVE_CANCELLATION_FILENAME, EmailParameters);
                var SendEmail = _emailHandler.SendEmail(employeeInformation.Supervisor.Email, "Leave Cancelation Request Notification", EmailTemplate, "");

                var noOfLeaveDaysEligible = GetEligibleLeaveDays(employeeInformation.Id);
                var noOfLeaveDaysLeft = noOfLeaveDaysEligible - employeeInformation.NumberOfEligibleLeaveDaysTaken;

                _notificationRepository.CreateAndReturn(new Notification { UserId = employeeInformation.UserId, Type= "Leave Cancelation Request", Message = $"Your leave cancelation request has been sent for approval.", IsRead = false });

                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                return _logger.Error<bool>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<PagedCollection<LeaveView>>> ListLeaves(PagingOptions pagingOptions, Guid? superAdminId, Guid? supervisorId = null, Guid? employeeInformationId = null, string search = null, DateFilter dateFilter = null)
        {
            try
            {
                var leaves = _leaveRepository.Query().Include(x => x.LeaveType).Include(x => x.EmployeeInformation).ThenInclude(x => x.User).OrderByDescending(x => x.DateCreated);
                if (superAdminId.HasValue)
                    leaves = leaves.Where(x => x.EmployeeInformation.User.SuperAdminId == superAdminId).OrderByDescending(u => u.DateCreated);

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
            catch (Exception ex)
            {
                return StandardResponse<PagedCollection<LeaveView>>.Error("Error listing leave");
            }
        }

        public async Task<StandardResponse<PagedCollection<LeaveView>>> ListAllPendingLeaves(PagingOptions pagingOptions, Guid superAdminId, Guid? supervisorId = null, Guid? employeeId = null)
        {
            try
            {
                var leaves = _leaveRepository.Query().Include(x => x.LeaveType).Include(x => x.EmployeeInformation).ThenInclude(x => x.User).
                    Where(x => (x.StatusId == (int)Statuses.PENDING || (x.StatusId == (int)Statuses.APPROVED && x.StartDate.Date >= DateTime.Now.Date)) 
                    && x.EmployeeInformation.User.SuperAdminId == superAdminId).OrderByDescending(x => x.DateCreated);

                if (supervisorId.HasValue && supervisorId != null)
                {
                    leaves = leaves.Where(x => x.EmployeeInformation.SupervisorId == supervisorId.Value).OrderByDescending(x => x.DateCreated);
                }

                if (employeeId.HasValue)
                {
                    leaves = leaves.Where(x => x.EmployeeInformationId == employeeId.Value).OrderByDescending(x => x.DateCreated);
                }

                var pagedLeaves = leaves.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedLeaves = pagedLeaves.ProjectTo<LeaveView>(_configuration).ToArray();

                foreach (var leave in mappedLeaves)
                {
                    leave.LeaveDaysEarned = GetEligibleLeaveDays(leave?.EmployeeInformationId);
                }
                var pagedCollection = PagedCollection<LeaveView>.Create(Link.ToCollection(nameof(LeaveController.ListAllPendingLeaves)), mappedLeaves, pagedLeaves.Count(), pagingOptions);

                return StandardResponse<PagedCollection<LeaveView>>.Ok(pagedCollection);
            }
            catch (Exception ex)
            {
                return StandardResponse<PagedCollection<LeaveView>>.Error("Error listing leave");
            }
        }

        public async Task<StandardResponse<PagedCollection<LeaveView>>> ListLeaveHistory(PagingOptions pagingOptions, Guid superAdminId, Guid? supervisorId = null, Guid? employeeId = null)
        {
            try
            {
                var leaves = _leaveRepository.Query().Include(x => x.LeaveType).Include(x => x.EmployeeInformation).ThenInclude(x => x.User).
                    Where(x => x.EmployeeInformation.User.SuperAdminId == superAdminId && (x.StatusId != (int)Statuses.PENDING && x.StatusId != (int)Statuses.REVIEWING) || 
                    (x.StatusId == (int)Statuses.APPROVED && x.StartDate.Date >= DateTime.Now.Date)).OrderByDescending(x => x.DateCreated);

                if (supervisorId.HasValue && supervisorId != null)
                {
                    leaves = leaves.Where(x => x.EmployeeInformation.SupervisorId == supervisorId.Value).OrderByDescending(x => x.DateCreated);
                }

                if (employeeId.HasValue)
                {
                    leaves = leaves.Where(x => x.EmployeeInformationId == employeeId.Value).OrderByDescending(x => x.DateCreated);
                }

                var pagedLeaves = leaves.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedLeaves = pagedLeaves.ProjectTo<LeaveView>(_configuration).ToArray();

                foreach (var leave in mappedLeaves)
                {
                    leave.LeaveDaysEarned = GetEligibleLeaveDays(leave?.EmployeeInformationId);
                }
                var pagedCollection = PagedCollection<LeaveView>.Create(Link.ToCollection(nameof(LeaveController.ListLeaveHistory)), mappedLeaves, pagedLeaves.Count(), pagingOptions);

                return StandardResponse<PagedCollection<LeaveView>>.Ok(pagedCollection);
            }
            catch (Exception ex)
            {
                return StandardResponse<PagedCollection<LeaveView>>.Error("Error listing leave");
            }
        }

        public async Task<StandardResponse<PagedCollection<LeaveView>>> ListCanceledLeave(PagingOptions pagingOptions, Guid superAdminId, Guid? supervisorId = null, Guid? employeeId = null)
        {
            try
            {
                var leaves = _leaveRepository.Query().Include(x => x.LeaveType).Include(x => x.EmployeeInformation).ThenInclude(x => x.User).
                    Where(x => x.EmployeeInformation.User.SuperAdminId == superAdminId && x.IsCanceled == true && x.StatusId == (int)Statuses.REVIEWING).OrderByDescending(x => x.DateCreated).AsQueryable();

                if (supervisorId.HasValue && supervisorId != null)
                {
                    leaves = leaves.Where(x => x.EmployeeInformation.SupervisorId == supervisorId.Value).OrderByDescending(x => x.DateCreated);
                }

                if (employeeId.HasValue)
                {
                    leaves = leaves.Where(x => x.EmployeeInformationId == employeeId.Value).OrderByDescending(x => x.DateCreated).AsQueryable();
                }

                var pagedLeaves = leaves.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedLeaves = pagedLeaves.ProjectTo<LeaveView>(_configuration).ToArray();

                foreach (var leave in mappedLeaves)
                {
                    leave.LeaveDaysEarned = GetEligibleLeaveDays(leave?.EmployeeInformationId);
                }
                var pagedCollection = PagedCollection<LeaveView>.Create(Link.ToCollection(nameof(LeaveController.ListLeaveHistory)), mappedLeaves, pagedLeaves.Count(), pagingOptions);

                return StandardResponse<PagedCollection<LeaveView>>.Ok(pagedCollection);
            }
            catch (Exception ex)
            {
                return StandardResponse<PagedCollection<LeaveView>>.Error("Error listing leave");
            }
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

                        _notificationRepository.CreateAndReturn(new Notification { UserId = leave.EmployeeInformation.UserId, Type = "Leave Approved", Message = $"Your leave request has been approved.", IsRead = false });

                        //sent to assignee
                        var EmailTemplateForAssignee = _emailHandler.ComposeFromTemplate(Constants.LEAVE_APPROVAL_WORK_ASSIGNEE_FILENAME, EmailParameters);
                        var SendEmailToAssignee = _emailHandler.SendEmail(assignee.Email, "Leave Approval Notification", EmailTemplate, "");

                        _notificationRepository.CreateAndReturn(new Notification { UserId = assignee.Id, Type = "Work Assignee Notification", Message = $"You have been assigned {leave.EmployeeInformation.User.FullName} tasks.", IsRead = false });

                        return StandardResponse<bool>.Ok(true);
                        break;
                    case LeaveStatuses.Declined:
                        leave.StatusId = (int)Statuses.DECLINED;
                        _leaveRepository.Update(leave);
                        return StandardResponse<bool>.Ok(true);
                        break; 
                    case LeaveStatuses.Canceled:
                        leave.StatusId = (int)Statuses.CANCELED;
                        _leaveRepository.Update(leave);

                        List<KeyValuePair<string, string>> Emailvariables = new()
                        {
                            new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, supervisor.FullName),
                            new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_COWORKER, leave.EmployeeInformation.User.FirstName),
                            new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_LEAVESTARTDATE, leave.StartDate.Date.ToString()),
                            new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_LEAVEENDDATE, leave.EndDate.Date.ToString()),
                        };

                        var EmailTemplateForCancelation = _emailHandler.ComposeFromTemplate(Constants.LEAVE_CANCELLATION_Approval_FILENAME, Emailvariables);
                        var SendCancelationEmail = _emailHandler.SendEmail(leave.EmployeeInformation.User.Email, "Leave Cancelation Approval Notification", EmailTemplateForCancelation, "");

                        _notificationRepository.CreateAndReturn(new Notification { UserId = leave.EmployeeInformation.UserId, Type = "Leave Cancelation Approved", Message = $"Your leave cancelation request has been approved.", IsRead = false });
                        return StandardResponse<bool>.Ok(true);
                        break;
                    case LeaveStatuses.DeclineCancelation:
                        leave.StatusId = (int)Statuses.REJECTED;
                        leave.IsCanceled = false;
                        _leaveRepository.Update(leave);

                        List<KeyValuePair<string, string>> DeclineCancellationEmailvariables = new()
                        {
                            new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, supervisor.FullName),
                            new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_COWORKER, leave.EmployeeInformation.User.FirstName),
                            new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_LEAVESTARTDATE, leave.StartDate.Date.ToString()),
                            new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_LEAVEENDDATE, leave.EndDate.Date.ToString()),
                        };

                        var EmailTemplateForDeclinedCancelation = _emailHandler.ComposeFromTemplate(Constants.LEAVE_CANCELLATION_DECLINEDl_FILENAME, DeclineCancellationEmailvariables);
                        var SendDeclinedCancelationEmail = _emailHandler.SendEmail(leave.EmployeeInformation.User.Email, "Leave Cancelation Declined Notification", EmailTemplateForDeclinedCancelation, "");

                        _notificationRepository.CreateAndReturn(new Notification { UserId = leave.EmployeeInformation.UserId, Type = "Leave Cancelation Declined", Message = $"Your leave request cancelation has been declined.", IsRead = false });
                        return StandardResponse<bool>.Ok(true);
                        break;

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

            //var contract = _contractRepository.Query().FirstOrDefault(x => x.EmployeeInformationId == employeeInformationId && x.StatusId == (int)Statuses.ACTIVE);

            //if (contract == null) return 0;

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
