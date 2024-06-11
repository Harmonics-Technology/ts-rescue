using AutoMapper;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Repositories;
using TimesheetBE.Repositories.Interfaces;
using TimesheetBE.Utilities.Extentions;
using TimesheetBE.Utilities;
using System.Linq;
using TimesheetBE.Models.UtilityModels;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using System.Collections.Generic;
using TimesheetBE.Controllers;
using TimesheetBE.Models;
using TimesheetBE.Services.Interfaces;
using Google.Cloud.Storage.V1;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml.Office2021.DocumentTasks;
using Stripe;
using TimesheetBE.Utilities.Abstrctions;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Office2010.Excel;
using TimesheetBE.Utilities.Constants;
using Microsoft.Extensions.Options;

namespace TimesheetBE.Services
{
    public class ProjectManagementService : IProjectManagementService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectTaskRepository _projectTaskRepository;
        private readonly IProjectSubTaskRepository _projectSubTaskRepository;
        private readonly IUserRepository _userRepository;
        private readonly IProjectTaskAsigneeRepository _projectTaskAsigneeRepository;
        private readonly IProjectTimesheetRepository _projectTimesheetRepository;
        private readonly IMapper _mapper;
        private readonly IConfigurationProvider _configuration;
        private readonly IHttpContextAccessor _httpContext;
        private readonly ITimeSheetService _timeSheetService;
        private readonly IDataExport _dataExport;
        private readonly IEmployeeInformationRepository _employeeInformationRepository;
        private readonly IContractRepository _contractRepository;
        private readonly IControlSettingRepository _controlSettingRepository;
        private readonly INotificationService _notificationService;
        private readonly IEmailHandler _emailHandler;
        private readonly Globals _appSettings;
        private readonly ITimeSheetRepository _timeSheetRepository;

        public ProjectManagementService(IProjectRepository projectRepository, IProjectTaskRepository projectTaskRepository, IProjectSubTaskRepository projectSubTaskRepository, 
            IUserRepository userRepository, IProjectTaskAsigneeRepository projectTaskAsigneeRepository, IProjectTimesheetRepository projectTimesheetRepository, IMapper mapper,
            IConfigurationProvider configuration, IHttpContextAccessor httpContext, ITimeSheetService timeSheetService, IDataExport dataExport, 
            IEmployeeInformationRepository employeeInformationRepository, IContractRepository contractRepository, IControlSettingRepository controlSettingRepository,
            INotificationService notificationService, IEmailHandler emailHandler, IOptions<Globals> appSettings, ITimeSheetRepository timeSheetRepository)
        {
            _projectRepository = projectRepository;
            _projectTaskRepository = projectTaskRepository;
            _projectSubTaskRepository = projectSubTaskRepository;
            _userRepository = userRepository;
            _projectTaskAsigneeRepository = projectTaskAsigneeRepository;
            _projectTimesheetRepository = projectTimesheetRepository;
            _mapper = mapper;
            _configuration = configuration;
            _httpContext = httpContext;
            _timeSheetService = timeSheetService;
            _dataExport = dataExport;
            _employeeInformationRepository = employeeInformationRepository;
            _contractRepository = contractRepository;
            _controlSettingRepository = controlSettingRepository;
            _notificationService = notificationService;
            _emailHandler = emailHandler;
            _appSettings = appSettings.Value;
            _timeSheetRepository = timeSheetRepository;
        }

        //Create a project

        public async Task<StandardResponse<bool>> CreateProject(ProjectModel model)
        {
            try
            {
                var superAdmin = _userRepository.Query().FirstOrDefault(x => x.Id == model.SuperAdminId);

                Guid UserId = _httpContext.HttpContext.User.GetLoggedInUserId<Guid>();

                var loggedInUser = _userRepository.Query().FirstOrDefault(x => x.Id == model.SuperAdminId);

                if (superAdmin == null) return StandardResponse<bool>.NotFound("user not found");

                var project = _mapper.Map<Project>(model);
                project.BudgetSpent = 0;

                project = _projectRepository.CreateAndReturn(project);

                model.AssignedUsers.ForEach(id =>
                {
                    var assignee = new ProjectTaskAsignee { UserId = id, ProjectId = project.Id };
                    _projectTaskAsigneeRepository.CreateAndReturn(assignee);
                });
                await _notificationService.SendNotification(new NotificationModel { UserId = project.SuperAdminId, Title = "Project Creation", Type = "Notification", Message = $"{project.Name} has been successfully created" });

                List<KeyValuePair<string, string>> EmailParameters = new()
                {
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_LOGO_URL, _appSettings.LOGO),
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, loggedInUser.FirstName),
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_PROJECT, model.Name)
                };

                var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.PROJECT_CREATION_FILENAME, EmailParameters);
                var SendEmail = _emailHandler.SendEmail(loggedInUser.Email, "PROJECT CREATION NOTIFICATION", EmailTemplate, "");

                foreach(var user in model.AssignedUsers)
                {
                    var assignee = _userRepository.Query().FirstOrDefault(x => x.Id == user);
                    if (assignee == null) continue;
                    List<KeyValuePair<string, string>> EmailParam = new()
                {
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_LOGO_URL, _appSettings.LOGO),
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, loggedInUser.FirstName),
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_URL, $"{Globals.FrontEndBaseUrl}TeamMember/project-management")
                };

                    var ProjectAssigneeEmailTemplate = _emailHandler.ComposeFromTemplate(Constants.PROJECT_ASSIGNEE_FILENAME, EmailParam);
                    var SendEmailToProjectAssignee = _emailHandler.SendEmail(loggedInUser.Email, "PROJECT ASSIGNMENT", EmailTemplate, "");
                }
                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception e)
            {
                return StandardResponse<bool>.Error("Error creating Project");
            }
        }

        public async Task<StandardResponse<bool>> UpdateProject(ProjectModel model)
        {
            try
            {
                var project = _projectRepository.Query().Include(x => x.Assignees).FirstOrDefault(x => x.Id == model.Id);

                if (project == null) return StandardResponse<bool>.Failed("Project not fount");

                project.Name = model.Name;
                project.StartDate = model.StartDate;
                project.EndDate = model.EndDate;
                project.Duration = model.Duration;
                project.Budget = model.Budget;
                project.Note = model.Note;
                project.DocumentURL = model.DocumentURL;
                project.BudgetThreshold = model.BudgetThreshold.HasValue ? model.BudgetThreshold.Value : null;
                project.ProjectManagerId = model.ProjectManagerId.HasValue ? model.ProjectManagerId.Value : null;
                foreach(var user in project.Assignees)
                {
                    if(!model.AssignedUsers.Any(x => x == user.UserId))
                    {
                        var assignees = _projectTaskAsigneeRepository.Query().Where(x => x.UserId == user.UserId && x.ProjectId == project.Id).ToList();

                        assignees.ForEach(x =>
                        {
                            x.Disabled = true;
                            _projectTaskAsigneeRepository.Update(x);
                        });
                    }
                }

                foreach (var id in model.AssignedUsers)
                {
                    if (project.Assignees.Any(x => x.UserId == id && x.Disabled == false)) continue;

                    if (project.Assignees.Any(x => x.UserId == id && x.Disabled == true))
                    {
                        var existingAssignee = _projectTaskAsigneeRepository.Query().FirstOrDefault(x => x.UserId == id && x.ProjectId == project.Id && x.ProjectTaskId == null);

                        existingAssignee.Disabled = false;

                        _projectTaskAsigneeRepository.Update(existingAssignee);
                    }
                    else
                    {
                        var assignee = new ProjectTaskAsignee { UserId = id, ProjectId = project.Id };

                        _projectTaskAsigneeRepository.CreateAndReturn(assignee);
                    }
                }

                //remove assigned users

                //if (project.Assignees.Any())
                //{
                //    foreach (var assignee in project.Assignees)
                //    {
                //        if (_projectSubTaskRepository.Query().Any(x => x.ProjectTaskAsigneeId == assignee.Id)) return StandardResponse<bool>.Failed("You cant update this task because one of the assignee is assigned to a subtasks");
                //        //if (_projectTimesheetRepository.Query().Any(x => x.ProjectTaskAsigneeId == assignee.Id)) return StandardResponse<bool>.Failed("You cant update this task because one of the assignee has filled their timesheet");
                //        _projectTaskAsigneeRepository.Delete(assignee);
                //    }
                //}

                //if (!model.AssignedUsers.Any()) return StandardResponse<bool>.Failed("Kindly add an assignee for this tasks");

                project.DateModified = DateTime.Now;

                project = _projectRepository.Update(project);

                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception e)
            {
                return StandardResponse<bool>.Error("Error creating Project");
            }
        }

        public async Task<StandardResponse<bool>> CreateTask(ProjectTaskModel model)
        {
            try
            {
                var superAdmin = _userRepository.Query().FirstOrDefault(x => x.Id == model.SuperAdminId);

                Guid UserId = _httpContext.HttpContext.User.GetLoggedInUserId<Guid>();

                var loggedInUser = _userRepository.Query().FirstOrDefault(x => x.Id == model.SuperAdminId);

                if (superAdmin == null) return StandardResponse<bool>.NotFound("user not found");

                //if (model.Category.HasValue && (int)model.Category == 0) return StandardResponse<bool>.Failed("Enter a valid category");

                if (!model.IsOperationalTask && (int)model.TaskPriority == 0) return StandardResponse<bool>.Failed("Select a task priority");

                if (model.ProjectId.HasValue)
                {
                    var project = _projectRepository.Query().FirstOrDefault(x => x.Id == model.ProjectId.Value);
                    if (project == null) return StandardResponse<bool>.NotFound("The project does not exist");
                }
                
                var task = _mapper.Map<ProjectTask>(model);
                //task.Category = model.Category.HasValue ? model.Category.ToString() : null;
                if (model.IsOperationalTask)
                {
                    task.OperationalTaskStatus = "To Do";
                    task.CreatedByUserId = UserId;
                }
                task.TaskPriority = model.TaskPriority == null ? null : model.TaskPriority.ToString();
                task.DurationInHours = model.DurationInHours == null && model.IsOperationalTask == true ? null : (model.EndDate - model.StartDate).TotalHours / 3;
                if (model.TrackedByHours == true) task.DurationInHours = model.DurationInHours.Value;

                if (task.ProjectId != null)
                {
                    var project = _projectRepository.Query().FirstOrDefault(x => x.Id == task.ProjectId);

                    if (task.EndDate.Date > project.EndDate.Date)
                    {
                        var diff = (task.EndDate.Date - project.EndDate.Date).TotalDays;

                        project.EndDate = project.EndDate.AddDays(diff);

                        _projectRepository.Update(project);
                    }
                }

                task = _projectTaskRepository.CreateAndReturn(task);

                model.AssignedUsers.ForEach(id =>
                {
                    var assignee = new ProjectTaskAsignee();
                    if(model.IsOperationalTask == false)
                    {
                        var budget = (decimal)(_timeSheetService.GetTeamMemberPayPerHour(id, DateTime.Now) * task.DurationInHours);
                        if (model.ProjectId.HasValue) assignee = new ProjectTaskAsignee { UserId = id, ProjectId = model.ProjectId, ProjectTaskId = task.Id, Budget = budget == 0 ? null : budget };
                    }
                    else
                    {
                        assignee = new ProjectTaskAsignee { UserId = id, ProjectTaskId = task.Id };
                    }
                    _projectTaskAsigneeRepository.CreateAndReturn(assignee);
                });

                await _notificationService.SendNotification(new NotificationModel { UserId = task.SuperAdminId, Title = "Task Creation", Type = "Notification", Message = $"{task.Name} has been successfully created" });

                List<KeyValuePair<string, string>> EmailParameters = new()
                {
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_LOGO_URL, _appSettings.LOGO),
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, loggedInUser.FirstName),
                };

                var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.TASK_CREATION_FILENAME, EmailParameters);
                var SendEmail = _emailHandler.SendEmail(loggedInUser.Email, "TASK CREATION NOTIFICATION", EmailTemplate, "");

                foreach (var user in model.AssignedUsers)
                {
                    var assignee = _userRepository.Query().FirstOrDefault(x => x.Id == user);
                    if (assignee == null) continue;
                    List<KeyValuePair<string, string>> EmailParam = new()
                    {
                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_LOGO_URL, _appSettings.LOGO),
                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, assignee.FirstName),
                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_TASK_NAME, task.Name),
                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_URL, $"{Globals.FrontEndBaseUrl}TeamMember/project-management")
                    };

                    var TaskAssigneeEmailTemplate = _emailHandler.ComposeFromTemplate(Constants.PROJECT_TASK_ASSIGNEE_FILENAME, EmailParam);
                    var SendEmailToProjectAssignee = _emailHandler.SendEmail(assignee.Email, "PROJECT TASK ASSIGNMENT", TaskAssigneeEmailTemplate, "");
                }
                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception e)
            {
                return StandardResponse<bool>.Error("Error creating task");
            }
        }

        public async Task<StandardResponse<bool>> UpdateTask(ProjectTaskModel model)
        {
            try
            {
                if (model.ProjectId.HasValue)
                {
                    var project = _projectRepository.Query().FirstOrDefault(x => x.Id == model.ProjectId.Value);
                    if (project == null) return StandardResponse<bool>.NotFound("The project does not exist");
                }

                var task = _projectTaskRepository.Query().Include(x => x.Assignees).FirstOrDefault(x => x.Id == model.Id);

                if (task == null) return StandardResponse<bool>.Failed("Task not found");

                if (!task.IsOperationalTask && (int)model.TaskPriority == 0) return StandardResponse<bool>.Failed("Select a task priority");

                task.Name = model.Name;
                task.TrackedByHours = model.TrackedByHours;
                task.StartDate = model.StartDate;
                task.EndDate = model.EndDate;
                task.TaskPriority = model.TaskPriority == null ? null : model.TaskPriority.ToString();
                task.Note = model.Note;
                task.DurationInHours = model.DurationInHours == null && model.IsOperationalTask == true ? null : (model.EndDate - model.StartDate).TotalHours / 3;
                if (model.TrackedByHours == true) task.DurationInHours = model.DurationInHours.Value;

                if(task.IsOperationalTask == true)
                {
                    task.OperationalTaskStatus = model.OperationalTaskStatus;
                    task.IsAssignedToMe = model.IsAssignedToMe;
                    task.IsCompleted = model.OperationalTaskStatus.ToLower() == "completed" ? true : false;
                    task.Department = model.Department;
                }
                if (task.ProjectId != null)
                {
                    var project = _projectRepository.Query().FirstOrDefault(x => x.Id == task.ProjectId);

                    if (task.EndDate.Date > project.EndDate.Date)
                    {
                        var diff = (task.EndDate.Date - project.EndDate.Date).TotalDays;

                        project.EndDate = project.EndDate.AddDays(diff);

                        _projectRepository.Update(project);
                    }
                }

                foreach (var user in task.Assignees)
                {
                    if (!model.AssignedUsers.Any(x => x == user.UserId))
                    {
                        var assignees = _projectTaskAsigneeRepository.Query().Where(x => x.UserId == user.UserId && x.ProjectTaskId == task.Id).ToList();

                        assignees.ForEach(x =>
                        {
                            x.Disabled = true;
                            _projectTaskAsigneeRepository.Update(x);
                        });
                    }
                }

                foreach (var id in model.AssignedUsers)
                {
                    if (task.Assignees.Any(x => x.UserId == id && x.Disabled == false)) continue;

                    if (task.Assignees.Any(x => x.UserId == id && x.Disabled == true))
                    {
                        var existingAssignee = _projectTaskAsigneeRepository.Query().FirstOrDefault(x => x.UserId == id && x.ProjectTaskId == task.Id);

                        existingAssignee.Disabled = false;

                        _projectTaskAsigneeRepository.Update(existingAssignee);
                    }
                    else
                    {
                        var assignee = new ProjectTaskAsignee();
                        if(task.IsOperationalTask == false)
                        {
                            var budget = (decimal)(_timeSheetService.GetTeamMemberPayPerHour(id, DateTime.Now) * task.DurationInHours);
                            if (model.ProjectId.HasValue) assignee = new ProjectTaskAsignee { UserId = id, ProjectId = model.ProjectId, ProjectTaskId = task.Id, Budget = budget };
                        }
                        else
                        {
                            assignee = new ProjectTaskAsignee { UserId = id, ProjectTaskId = task.Id };
                        }
                        
                        _projectTaskAsigneeRepository.CreateAndReturn(assignee);
                    }
                }

                task.DateModified = DateTime.Now;

                task = _projectTaskRepository.Update(task);

                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception e)
            {
                return StandardResponse<bool>.Error("Error creating task");
            }
        }

        public async Task<StandardResponse<bool>> CreateSubTask(ProjectSubTaskModel model)
        {
            try
            {
                if ((int)model.TaskPriority == 0) return StandardResponse<bool>.Failed("Select a task priority");

                var task = _projectTaskRepository.Query().FirstOrDefault(x => x.Id == model.ProjectTaskId);

                if(task == null) return StandardResponse<bool>.NotFound("Task not found");

                var subTask = _mapper.Map<ProjectSubTask>(model);

                subTask.TaskPriority = model.TaskPriority.ToString();

                subTask.DurationInHours = (model.EndDate - model.StartDate).TotalHours / 3;

                if (model.TrackedByHours) task.DurationInHours = model.DurationInHours.Value;

                //if (subTask.DurationInHours.Value > task.DurationInHours) return StandardResponse<bool>.NotFound("Subtask duration cannot be greater than the task duration");

                if(model.EndDate.Date > task.EndDate.Date)
                {
                    var dateDifference = (model.EndDate.Date - task.EndDate.Date).TotalDays;

                    task.EndDate = task.EndDate.AddDays(dateDifference);

                    if(task.TrackedByHours == false) task.DurationInHours = (task.EndDate.Date - task.StartDate.Date).TotalHours / 3;

                    if(task.ProjectId != null)
                    {
                        var project = _projectRepository.Query().FirstOrDefault(x => x.Id == task.ProjectId);

                        if (task.EndDate.Date > project.EndDate.Date)
                        {
                            var diff = (task.EndDate.Date - project.EndDate.Date).TotalDays;

                            project.EndDate = project.EndDate.AddDays(diff);

                            _projectRepository.Update(project);
                        }
                    }

                    _projectTaskRepository.Update(task);
                }

                var assignee = _projectTaskAsigneeRepository.Query().FirstOrDefault(x => x.Id == model.ProjectTaskAsigneeId);

                var assignedUser = _userRepository.Query().FirstOrDefault(x => x.Id == assignee.UserId);

                List<KeyValuePair<string, string>> EmailParam = new()
                    {
                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_LOGO_URL, _appSettings.LOGO),
                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, assignedUser.FirstName),
                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_SUBTASKNAME, subTask.Name),
                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_URL, $"{Globals.FrontEndBaseUrl}TeamMember/project-management")
                    };

                var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.PROJECT_SUBTASK_ASSIGNEE_FILENAME, EmailParam);
                var SendEmailToProjectAssignee = _emailHandler.SendEmail(assignedUser.Email, "PROJECT SUBTASK ASSIGNMENT", EmailTemplate, "");

                subTask = _projectSubTaskRepository.CreateAndReturn(subTask);

                await _notificationService.SendNotification(new NotificationModel { UserId = task.SuperAdminId, Title = "Subtask Created", Type = "Notification", Message = $"{subTask.Name} has been successfully created" });

                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception e)
            {
                return StandardResponse<bool>.Error("Error creating subtask");
            }
        }

        public async Task<StandardResponse<bool>> UpdateSubTask(ProjectSubTaskModel model)
        {
            try
            {
                var subtask = _projectSubTaskRepository.Query().FirstOrDefault(x => x.Id == model.Id);
                if(subtask == null) return StandardResponse<bool>.Failed("Sub task not found");
                if ((int)model.TaskPriority == 0) return StandardResponse<bool>.Failed("Select a task priority");

                var task = _projectTaskRepository.Query().FirstOrDefault(x => x.Id == model.ProjectTaskId);

                if (task == null) return StandardResponse<bool>.NotFound("Task not found");

                subtask.ProjectTaskId = model.ProjectTaskId;
                subtask.Name = model.Name;
                subtask.ProjectTaskAsigneeId = model.ProjectTaskAsigneeId;
                subtask.StartDate = model.StartDate;
                subtask.EndDate = model.EndDate;
                subtask.Duration = model.Duration;

                subtask.TaskPriority = model.TaskPriority.ToString();

                subtask.DurationInHours = (model.EndDate - model.StartDate).TotalHours / 3;

                if (model.TrackedByHours) task.DurationInHours = model.DurationInHours.Value;

                //if (subtask.DurationInHours.Value > task.DurationInHours) return StandardResponse<bool>.NotFound("Subtask duration cannot be greater than the task duration");

                if (model.EndDate.Date > task.EndDate.Date)
                {
                    var dateDifference = (model.EndDate.Date - task.EndDate.Date).TotalDays;

                    task.EndDate = task.EndDate.AddDays(dateDifference);

                    if (task.TrackedByHours == false) task.DurationInHours = (task.EndDate.Date - task.StartDate.Date).TotalHours / 3;

                    if (task.ProjectId != null)
                    {
                        var project = _projectRepository.Query().FirstOrDefault(x => x.Id == task.ProjectId);

                        if (task.EndDate.Date > project.EndDate.Date)
                        {
                            var diff = (task.EndDate.Date - project.EndDate.Date).TotalDays;

                            project.EndDate = project.EndDate.AddDays(diff);

                            _projectRepository.Update(project);
                        }
                    }

                    _projectTaskRepository.Update(task);
                }

                subtask.DateModified = DateTime.Now;

                _projectSubTaskRepository.Update(subtask);

                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception e)
            {
                return StandardResponse<bool>.Error("Error creating subtask");
            }
        }

        public async Task<StandardResponse<bool>> FillTimesheetForProject(ProjectTimesheetModel model)
        {
            try
            {
                var loggedInUserId = _httpContext.HttpContext.User.GetLoggedInUserId<Guid>();
                //var assignee = _projectTaskAsigneeRepository.Query().FirstOrDefault(x => x.UserId == loggedInUserId && x.ProjectId == model.ProjectId && x.ProjectTaskId == model.ProjectTaskId);

                var assignee = _projectTaskAsigneeRepository.Query().Include(x => x.User).FirstOrDefault(x => x.UserId == loggedInUserId && x.Id == model.ProjectTaskAsigneeId);

                if (assignee == null || assignee?.Disabled == true) return StandardResponse<bool>.NotFound("You are not assigned to this project");

                var settings = _controlSettingRepository.Query().FirstOrDefault(x => x.SuperAdminId == assignee.User.SuperAdminId);

                var contract = _contractRepository.Query().FirstOrDefault(x => x.EmployeeInformationId == assignee.User.EmployeeInformationId && x.StatusId == (int)Statuses.ACTIVE);

                if(contract == null) return StandardResponse<bool>.NotFound("You do not have an active contract");

                if (model.ProjectTimesheets.Any(x => x.StartDate.Date > DateTime.Today.Date && !settings.AllowUsersTofillFutureTimesheet))
                    return StandardResponse<bool>.Failed("You cant fill timesheet for future date");

                if (model.ProjectTimesheets.Any(x => contract.StartDate.Date > x.StartDate.Date || x.EndDate.Date > contract.EndDate.Date)) 
                    return StandardResponse<bool>.Failed("You cant fill timesheet for days that does not fall between your contract start date and end date. Try again with appropriate date");

                if(model.ProjectTimesheets.Any(x => x.StartDate.Date != x.EndDate.Date)) return StandardResponse<bool>.Failed("Invalid start date or end date and time");

                if(model.ProjectTimesheets.Any(x => x.StartDate.DayOfWeek == DayOfWeek.Saturday || x.StartDate.DayOfWeek == DayOfWeek.Sunday))
                    return StandardResponse<bool>.Failed("You cannot fill time sheet for weekends");

                if (model.ProjectId.HasValue)
                {
                    var project = _projectRepository.Query().FirstOrDefault(x => x.Id == model.ProjectId);

                    if (project == null) return StandardResponse<bool>.NotFound("The project does not exist");

                    if (model.ProjectTimesheets.Any(x => x.StartDate.Date > project.EndDate.Date || project.StartDate.Date > x.StartDate.Date))
                        return StandardResponse<bool>.Failed("You cant fill timesheet before a project start date or beyond the project endate");

                    if (project.IsCompleted) return StandardResponse<bool>.Failed("Project has been completed");
                }

                var task = _projectTaskRepository.Query().FirstOrDefault(x => x.Id == model.ProjectTaskId);

                if (task == null) return StandardResponse<bool>.NotFound("Task not found");

                if (task.IsCompleted) return StandardResponse<bool>.NotFound("Task has been completed");

                if(_projectSubTaskRepository.Query().Any(x => x.ProjectTaskId == model.ProjectTaskId) && !model.ProjectSubTaskId.HasValue) return StandardResponse<bool>.Failed("Please enter the subtask for the selected task");

                if (model.ProjectSubTaskId.HasValue)
                {
                    var subTask = _projectSubTaskRepository.Query().FirstOrDefault(x => x.Id == model.ProjectSubTaskId.Value);
                    if (subTask == null) return StandardResponse<bool>.NotFound("subtask not found");
                }

                foreach(var newTimesheet in model.ProjectTimesheets)
                {
                    var project = _projectRepository.Query().FirstOrDefault(x => x.Id == model.ProjectId);

                    var timesheet = _mapper.Map<ProjectTimesheet>(model);

                    timesheet.StartDate = newTimesheet.StartDate;

                    timesheet.EndDate = newTimesheet.EndDate;

                    timesheet.PercentageOfCompletion = newTimesheet.PercentageOfCompletion;

                    timesheet.Billable = newTimesheet.Billable;

                    timesheet.TotalHours = (newTimesheet.EndDate - newTimesheet.StartDate).TotalHours;

                    timesheet.AmountEarned = (decimal)(_timeSheetService.GetTeamMemberPayPerHour(assignee.UserId, newTimesheet.StartDate) * timesheet.TotalHours);

                    timesheet.StatusId = (int)Statuses.PENDING;
                    timesheet.ProjectTaskAsigneeId = assignee.Id;

                    if (project != null)
                    {
                        //timesheet.AmountEarned = (decimal)(_timeSheetService.GetTeamMemberPayPerHour(assignee.UserId) * timesheet.TotalHours);

                        //timesheet.StatusId = (int)Statuses.PENDING;

                        //assignee.HoursLogged += (newTimesheet.EndDate - newTimesheet.StartDate).TotalHours;

                        //if (assignee.Budget != null && newTimesheet.Billable) assignee.BudgetSpent += timesheet.AmountEarned;

                        //_projectTaskAsigneeRepository.Update(assignee);

                        var updateTimesheet = await _timeSheetService.AddProjectManagementTimeSheet(assignee.UserId, newTimesheet.StartDate, newTimesheet.EndDate);

                        //if (newTimesheet.Billable) project.BudgetSpent += timesheet.AmountEarned;

                        //project.HoursSpent += timesheet.TotalHours;

                        //_projectRepository.Update(project);

                        if (timesheet.ProjectTaskId.HasValue && timesheet.ProjectSubTaskId.HasValue)
                        {
                            var subtask = _projectSubTaskRepository.Query().FirstOrDefault(x => x.Id == timesheet.ProjectSubTaskId.Value);

                            var subtaskTask = _projectTaskRepository.Query().FirstOrDefault(x => x.Id == timesheet.ProjectTaskId.Value);

                            subtask.PercentageOfCompletion = timesheet.PercentageOfCompletion;

                            subtask.DateModified = DateTime.Now;

                            var subTaskUpdate = _projectSubTaskRepository.Update(subtask);

                            subtaskTask.PercentageOfCompletion = (double)GetTaskPercentageOfCompletion(subtaskTask.Id) * 100;

                            subtaskTask.DateModified = DateTime.Now;

                            _projectTaskRepository.Update(subtaskTask);

                        }
                        if (timesheet.ProjectTaskId.HasValue && !timesheet.ProjectSubTaskId.HasValue)
                        {
                            var thisTask = _projectTaskRepository.Query().FirstOrDefault(x => x.Id == timesheet.ProjectTaskId.Value);

                            thisTask.PercentageOfCompletion = timesheet.PercentageOfCompletion;

                            thisTask.DateModified = DateTime.Now;

                            _projectTaskRepository.Update(thisTask);
                        }
                    }

                    timesheet = _projectTimesheetRepository.CreateAndReturn(timesheet);
                }

                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception e)
            {
                return StandardResponse<bool>.Error("Error Filling timesheet");
            }
        }

        public async Task<StandardResponse<bool>> TreatTimesheet(ProjectTimesheetApprovalModel model)
        {
            try
            {
                var employee = _userRepository.Query().FirstOrDefault(x => x.EmployeeInformationId == model.EmployeeInformationId);

                var superAdmin = _userRepository.Query().FirstOrDefault(x => x.Id == employee.SuperAdminId);

                if(employee == null) return StandardResponse<bool>.NotFound("User not found");

                if (superAdmin == null) return StandardResponse<bool>.NotFound("Super admin not found");

                List<ProjectTimesheet> timesheets = new();

                //if(model.StartDate.HasValue && model.EndDate.HasValue)
                //{
                //    timesheets = _projectTimesheetRepository.Query().Include(x => x.ProjectTaskAsignee).Where(x => x.StartDate.Date >= model.StartDate.Value.Date && 
                //    x.EndDate.Date <= model.EndDate.Value.Date && x.ProjectTaskAsignee.UserId == employee.Id).ToList();
                //}

                if(model.Dates != null)
                {
                    model.Dates.ForEach(date =>
                    {
                        var timesheet = _projectTimesheetRepository.Query().Include(x => x.ProjectTaskAsignee).Where(x => x.StartDate.Date == date.Date &&
                        x.EndDate.Date == date.Date && x.ProjectTaskAsignee.UserId == employee.Id && x.StatusId == (int)Statuses.PENDING).ToList();

                        timesheets.AddRange(timesheet);
                    });
                }
                

                if (model.TimesheetId.HasValue)
                {
                    var timesheet = _projectTimesheetRepository.Query().FirstOrDefault(x => x.Id == model.TimesheetId.Value);
                    if (timesheet == null) return StandardResponse<bool>.NotFound("Timesheet not found");
                    timesheets.Add(timesheet);
                }
                foreach(var timesheet in timesheets)
                {
                    //var assignee = _projectTaskAsigneeRepository.Query().FirstOrDefault(x => x.Id == timesheet.ProjectTaskAsigneeId);

                    if (model.Approve)
                    {
                        timesheet.StatusId = (int)Statuses.APPROVED;

                        if (timesheet.ProjectId.HasValue)
                        {
                            var assignee = _projectTaskAsigneeRepository.Query().FirstOrDefault(x => x.Id == timesheet.ProjectTaskAsigneeId);

                            var projectassignee = _projectTaskAsigneeRepository.Query().FirstOrDefault(x => x.UserId == assignee.UserId && x.ProjectId == timesheet.ProjectId && x.ProjectTaskId == null);

                            var updateTimesheet = await _timeSheetService.TreatProjectManagementTimeSheet(assignee.UserId, model.Approve, timesheet.StartDate, timesheet.EndDate, model.Reason);

                            var project = _projectRepository.Query().FirstOrDefault(x => x.Id == timesheet.ProjectId.Value);

                            timesheet.AmountEarned = (decimal)(_timeSheetService.GetTeamMemberPayPerHour(assignee.UserId, timesheet.StartDate) * timesheet.TotalHours);

                            timesheet.IsApproved = true;

                            assignee.HoursLogged += (timesheet.EndDate - timesheet.StartDate).TotalHours;

                            projectassignee.HoursLogged += (timesheet.EndDate - timesheet.StartDate).TotalHours;

                            if (assignee.Budget != null && timesheet.Billable)
                            {
                                assignee.BudgetSpent = assignee.BudgetSpent == null ? timesheet.AmountEarned : assignee.BudgetSpent += timesheet.AmountEarned;
                            }
                            assignee.DateModified = DateTime.Now;

                            projectassignee.DateModified = DateTime.Now;

                            _projectTaskAsigneeRepository.Update(assignee);

                            _projectTaskAsigneeRepository.Update(projectassignee);

                            if (timesheet.Billable) project.BudgetSpent += timesheet.AmountEarned;

                            project.HoursSpent += timesheet.TotalHours;

                            project.DateModified = DateTime.Now;

                            _projectRepository.Update(project);

                            if(project.BudgetThreshold != null && project.BudgetSpent >= project.BudgetThreshold.Value)
                            {
                                await _notificationService.SendNotification(new NotificationModel { UserId = superAdmin.Id, Title = "Budget Threshold Warning", Type = "Notification", Message = $"{project.Name} has reached set threshold. Action is required" });

                                List<KeyValuePair<string, string>> EmailParameters = new()
                                {
                                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_LOGO_URL, _appSettings.LOGO),
                                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, superAdmin.FullName),
                                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_PROJECT, project.Name),
                                };

                                var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.BUDGET_THRESHOLD_NOTIFICATION_FILENAME, EmailParameters);
                                var SendEmail = _emailHandler.SendEmail(superAdmin.Email, "BUDGET THRESHOLD WARNING", EmailTemplate, "");
                            }
                        }

                        //if (timesheet.ProjectTaskId.HasValue && timesheet.ProjectSubTaskId.HasValue)
                        //{
                        //    var subtask = _projectSubTaskRepository.Query().FirstOrDefault(x => x.Id == timesheet.ProjectSubTaskId.Value);

                        //    var task = _projectTaskRepository.Query().FirstOrDefault(x => x.Id == timesheet.ProjectTaskId.Value);

                        //    subtask.PercentageOfCompletion = timesheet.PercentageOfCompletion;

                        //    subtask.DateModified = DateTime.Now;

                        //    var subTaskUpdate = _projectSubTaskRepository.Update(subtask);
                            
                        //    task.PercentageOfCompletion = (double)GetTaskPercentageOfCompletion(task.Id) * 100;

                        //    task.DateModified = DateTime.Now;

                        //    _projectTaskRepository.Update(task);

                        //}
                        //if (timesheet.ProjectTaskId.HasValue && !timesheet.ProjectSubTaskId.HasValue)
                        //{
                        //    var task = _projectTaskRepository.Query().FirstOrDefault(x => x.Id == timesheet.ProjectTaskId.Value);

                        //    task.PercentageOfCompletion = timesheet.PercentageOfCompletion;

                        //    task.DateModified = DateTime.Now;

                        //    _projectTaskRepository.Update(task);
                        //}
                    }
                    else
                    {
                        timesheet.StatusId = (int)Statuses.REJECTED;
                        timesheet.Reason = model.Reason;
                    }

                    timesheet.DateModified = DateTime.Now;

                    _projectTimesheetRepository.Update(timesheet);
                }
                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception e)
            {
                return StandardResponse<bool>.Error("Error Treating timesheet");
            }
        }

        public async Task<StandardResponse<bool>> UpdateFilledTimesheet(UpdateProjectTimesheet model)
        {
            try
            {
                var loggedInUserId = _httpContext.HttpContext.User.GetLoggedInUserId<Guid>();

                var employee = _userRepository.Query().FirstOrDefault(x => x.EmployeeInformationId == model.EmployeeInformationId);

                var superAdmin = _userRepository.Query().FirstOrDefault(x => x.Id == employee.SuperAdminId);

                if (employee == null) return StandardResponse<bool>.NotFound("User not found");

                if (superAdmin == null) return StandardResponse<bool>.NotFound("Super admin not found");

                var assignee = _projectTaskAsigneeRepository.Query().Include(x => x.User).FirstOrDefault(x => x.UserId == loggedInUserId && x.Id == model.ProjectTaskAsigneeId);

                if (assignee == null || assignee?.Disabled == true) return StandardResponse<bool>.NotFound("You are not assigned to this project");

                var timesheet = _projectTimesheetRepository.Query().FirstOrDefault(x => x.Id == model.Id);

                if (timesheet == null) return StandardResponse<bool>.NotFound("Timesheet not found");

                if(timesheet.IsApproved) return StandardResponse<bool>.NotFound("Timesheet has already been approved");

                timesheet.StartDate = model.StartDate;

                timesheet.EndDate = model.EndDate;

                timesheet.PercentageOfCompletion = model.PercentageOfCompletion;

                timesheet.Billable = model.Billable;

                timesheet.TotalHours = (model.EndDate - model.StartDate).TotalHours;

                timesheet.AmountEarned = (decimal)(_timeSheetService.GetTeamMemberPayPerHour(employee.Id, model.StartDate) * timesheet.TotalHours);

                timesheet.StatusId = (int)Statuses.PENDING;
                timesheet.ProjectTaskAsigneeId = assignee.Id;

                var project = _projectRepository.Query().FirstOrDefault(x => x.Id == timesheet.ProjectId);

                if (project != null)
                {

                    var updateTimesheet = await _timeSheetService.AddProjectManagementTimeSheet(assignee.UserId, model.StartDate, model.EndDate);
                    
                }

                timesheet.IsEdited = true;
                timesheet.DateModified = DateTime.Now;
                _projectTimesheetRepository.Update(timesheet);
                
                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception e)
            {
                return StandardResponse<bool>.Error("Error Treating timesheet");
            }
        }

        public async Task<StandardResponse<PagedCollection<ProjectView>>> ListProject(PagingOptions pagingOptions, Guid superAdminId, ProjectStatus? status = null, Guid? userId = null, string search = null)
        {
            try
            {
                var superAdmin = _userRepository.Query().FirstOrDefault(x => x.Id == superAdminId);

                if (superAdmin == null) return StandardResponse<PagedCollection<ProjectView>>.NotFound("User not found");

                var projects = _projectRepository.Query().Include(x => x.Assignees).ThenInclude(x => x.User).Where(x => x.SuperAdminId == superAdminId).OrderByDescending(x => x.DateModified);

                if(userId != null)
                {
                    projects = projects.Where(x => x.Assignees.Any(x => x.UserId == userId)).OrderByDescending(x => x.DateModified);
                }

                if(!string.IsNullOrEmpty(search))
                {
                    projects = projects.Where(x => x.Name.ToLower().Contains(search.ToLower())).OrderByDescending(x => x.DateModified);
                }

                if (status.HasValue && status == ProjectStatus.NotStarted)
                {
                    projects = projects.Where(x => x.StartDate > DateTime.Now && x.IsCompleted == false).OrderByDescending(x => x.DateModified);
                }else if (status.HasValue && status.Value == ProjectStatus.InProgress)
                {
                    projects = projects.Where(x => DateTime.Now > x.StartDate && x.IsCompleted == false).OrderByDescending(x => x.DateModified);
                }else if(status.HasValue && status.Value == ProjectStatus.Completed)
                {
                    projects = projects.Where(x => x.IsCompleted == true).OrderByDescending(x => x.DateModified);
                }

                var pageProjects = projects.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value).OrderByDescending(x => x.DateModified);

                var mappedProjects = pageProjects.ProjectTo<ProjectView>(_configuration).ToList();

                foreach (var project in mappedProjects)
                {
                    var progress = GetProjectPercentageOfCompletion(project.Id);
                    if (project.IsCompleted)
                    {
                        progress = 100;
                    }
                    project.Progress = progress;
                    project.Assignees = project.Assignees.Where(x => x.Disabled == false).ToList();
                }

                var pagedCollection = PagedCollection<ProjectView>.Create(Link.ToCollection(nameof(ProjectManagementController.ListProject)), mappedProjects.ToArray(), projects.Count(), pagingOptions);

                return StandardResponse<PagedCollection<ProjectView>>.Ok(pagedCollection);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<ProjectView>>.Error("Error listing Project");
            }
        }

        public async Task<StandardResponse<PagedCollection<ProjectTaskView>>> ListTasks(PagingOptions pagingOptions, Guid superAdminId, Guid? projectId = null, ProjectStatus? status = null, Guid? userId = null, string search = null)
        {
            try
            {
                var superAdmin = _userRepository.Query().FirstOrDefault(x => x.Id == superAdminId);

                if (superAdmin == null) return StandardResponse<PagedCollection<ProjectTaskView>>.NotFound("User not found");

                var tasks = _projectTaskRepository.Query().Include(x => x.SubTasks).Include(x => x.Assignees).
                    ThenInclude(x => x.User).Where(x => x.SuperAdminId == superAdminId && x.ProjectId != null && x.IsOperationalTask == false).OrderByDescending(x => x.DateModified);
                //var tasks = _projectTaskRepository.Query().Include(x => x.Assignees).ThenInclude(x => x.User).Where(x => x.SuperAdminId == superAdminId && x.ProjectId == projectId);

                if (projectId.HasValue)
                {
                    tasks = tasks.Where(x => x.ProjectId == projectId).OrderByDescending(x => x.DateModified);
                }

                //if (!projectId.HasValue)
                //{
                //    tasks = tasks.Where(x => x.Category == null).OrderByDescending(x => x.DateModified);
                //}


                if (userId != null)
                {
                    tasks = tasks.Where(x => x.Assignees.Any(x => x.UserId == userId)).OrderByDescending(x => x.DateModified);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    tasks = tasks.Where(x => x.Name.ToLower().Contains(search.ToLower())).OrderByDescending(x => x.DateModified);
                }

                if (status.HasValue && status == ProjectStatus.NotStarted)
                {
                    tasks = tasks.Where(x => x.StartDate > DateTime.Now).OrderByDescending(x => x.DateModified);
                }
                else if (status.HasValue && status.Value == ProjectStatus.InProgress)
                {
                    tasks = tasks.Where(x => DateTime.Now > x.StartDate && x.IsCompleted == false).OrderByDescending(x => x.DateModified);
                }
                else if (status.HasValue && status.Value == ProjectStatus.Completed)
                {
                    tasks = tasks.Where(x => x.IsCompleted == true).OrderByDescending(x => x.DateModified); 
                }

                var pagedTasks = tasks.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedTasks = pagedTasks.ProjectTo<ProjectTaskView>(_configuration).ToList();

                foreach (var task in mappedTasks)
                {
                    var hours = GetHoursSpentOnTask(task.Id);
                    task.HoursSpent = hours;
                    if (task.IsCompleted) task.PercentageOfCompletion = 100;
                    task.Assignees = task.Assignees.Where(x => x.Disabled == false).ToList();
                    //task.Progress = GetTaskPercentageOfCompletion(task.Id);
                }

                var pagedCollection = PagedCollection<ProjectTaskView>.Create(Link.ToCollection(nameof(ProjectManagementController.ListTasks)), mappedTasks.ToArray(), tasks.Count(), pagingOptions);

                return StandardResponse<PagedCollection<ProjectTaskView>>.Ok(pagedCollection);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<ProjectTaskView>>.Error("Error listing tasks");
            }
        }

        public async Task<StandardResponse<PagedCollection<ProjectTaskView>>> ListOperationalTasks(PagingOptions pagingOptions, Guid superAdminId, string? status = null, Guid? userId = null, string search = null)
        {
            try
            {
                var user = _userRepository.Query().FirstOrDefault(x => x.Id == superAdminId);

                if (user == null) return StandardResponse<PagedCollection<ProjectTaskView>>.NotFound("User not found");

                var tasks = _projectTaskRepository.Query().Include(x => x.SubTasks).Include(x => x.Assignees).
                    ThenInclude(x => x.User).Include(x => x.CreatedByUser).Where(x => x.SuperAdminId == superAdminId && x.ProjectId == null && x.IsOperationalTask == true).OrderByDescending(x => x.DateModified);
                //var tasks = _projectTaskRepository.Query().Include(x => x.Assignees).ThenInclude(x => x.User).Where(x => x.SuperAdminId == superAdminId && x.ProjectId == projectId);

                if (userId != null)
                {
                    tasks = tasks.Where(x => x.Assignees.Any(x => x.UserId == userId)).OrderByDescending(x => x.DateModified);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    tasks = tasks.Where(x => x.Name.ToLower().Contains(search.ToLower())).OrderByDescending(x => x.DateModified);
                }

                if (status != null && status.ToLower() == "to do")
                {
                    tasks = tasks.Where(x => x.OperationalTaskStatus.ToLower() == "to do").OrderByDescending(x => x.DateModified);
                }
                else if (status != null && status.ToLower() == "in progress")
                {
                    tasks = tasks.Where(x => x.OperationalTaskStatus.ToLower() == "in progress").OrderByDescending(x => x.DateModified);
                }
                else if (status != null && status.ToLower() == "done")
                {
                    tasks = tasks.Where(x => x.OperationalTaskStatus.ToLower() == "done").OrderByDescending(x => x.DateModified);
                }

                var pagedTasks = tasks.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value).OrderByDescending(x => x.DateModified);

                var mappedTasks = pagedTasks.ProjectTo<ProjectTaskView>(_configuration).ToList();

                foreach (var task in mappedTasks)
                {
                    var hours = GetHoursSpentOnTask(task.Id);
                    task.HoursSpent = hours;
                    if (task.IsCompleted) task.PercentageOfCompletion = 100;
                    //task.Progress = GetTaskPercentageOfCompletion(task.Id);
                    task.Assignees = task.Assignees.Where(x => x.Disabled == false).ToList();
                }

                var pagedCollection = PagedCollection<ProjectTaskView>.Create(Link.ToCollection(nameof(ProjectManagementController.ListOperationalTasks)), mappedTasks.ToArray(), tasks.Count(), pagingOptions);

                return StandardResponse<PagedCollection<ProjectTaskView>>.Ok(pagedCollection);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<ProjectTaskView>>.Error("Error listing tasks");
            }
        }

        public async Task<StandardResponse<PagedCollection<ProjectTaskAsigneeView>>> GetUserTasks(PagingOptions pagingOptions, Guid userId, Guid projectId)
        {
            try
            {
                var user = _userRepository.Query().FirstOrDefault(x => x.Id == userId);

                if (user == null) return StandardResponse<PagedCollection<ProjectTaskAsigneeView>>.NotFound("User not found");

                var assigneeTasks = _projectTaskAsigneeRepository.Query().Include(x => x.ProjectTask).Include(x => x.SubTasks).ThenInclude(x => x.ProjectTimesheets).Where(x => x.ProjectId == projectId && x.UserId == userId && x.ProjectTaskId != null).OrderByDescending(x => x.DateModified);

                //var assigneeTasks = _projectTaskAsigneeRepository.Query().Include(x => x.ProjectTask).Where(x => x.ProjectId == projectId && x.UserId == userId);

                var pagedTasks = assigneeTasks.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value).OrderByDescending(x => x.DateModified);

                var mappedTasks = pagedTasks.ProjectTo<ProjectTaskAsigneeView>(_configuration).ToList();

                var pagedCollection = PagedCollection<ProjectTaskAsigneeView>.Create(Link.ToCollection(nameof(ProjectManagementController.GetUserTasks)), mappedTasks.ToArray(), assigneeTasks.Count(), pagingOptions);

                return StandardResponse<PagedCollection<ProjectTaskAsigneeView>>.Ok(pagedCollection);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<ProjectTaskAsigneeView>>.Error("Error listing user tasks");
            }
        }

        public async Task<StandardResponse<PagedCollection<ProjectSubTaskView>>> ListSubTasks(PagingOptions pagingOptions, Guid? taskId = null, ProjectStatus? status = null, string search = null)
        {
            try
            {
                var task = _projectTaskRepository.Query().FirstOrDefault(x => x.Id == taskId);

                if (task == null) return StandardResponse<PagedCollection<ProjectSubTaskView>>.NotFound("Task not found");

                var subtasks = _projectSubTaskRepository.Query().Include(x => x.ProjectTask).Include(x => x.ProjectTaskAsignee).AsQueryable().OrderByDescending(x => x.DateModified);

                //var subtasks = _projectSubTaskRepository.Query().Where(x => x.ProjectTaskId == taskId);
                //var sublist = subtasks.ToList();

                if (taskId.HasValue)
                {
                    subtasks = subtasks.Where(x => x.ProjectTaskId == taskId).OrderByDescending(x => x.DateModified);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    subtasks = subtasks.Where(x => x.Name.ToLower().Contains(search.ToLower())).OrderByDescending(x => x.DateModified);
                }

                if (status.HasValue && status == ProjectStatus.NotStarted)
                {
                    subtasks = subtasks.Where(x => x.StartDate > DateTime.Now).OrderByDescending(x => x.DateModified);
                }
                else if (status.HasValue && status.Value == ProjectStatus.InProgress)
                {
                    subtasks = subtasks.Where(x => DateTime.Now > x.StartDate && DateTime.Now < x.EndDate).OrderByDescending(x => x.DateModified);
                }
                else if (status.HasValue && status.Value == ProjectStatus.Completed)
                {
                    subtasks = subtasks.Where(x => x.IsCompleted == true).OrderByDescending(x => x.DateModified);
                }

                var pagedTasks = subtasks.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value).OrderByDescending(x => x.DateModified).AsNoTracking();

                var mappedTasks = pagedTasks.ProjectTo<ProjectSubTaskView>(_configuration).AsNoTracking();

                var pagedCollection = PagedCollection<ProjectSubTaskView>.Create(Link.ToCollection(nameof(ProjectManagementController.ListSubTasks)), mappedTasks.ToArray(), subtasks.Count(), pagingOptions);

                return StandardResponse<PagedCollection<ProjectSubTaskView>>.Ok(pagedCollection);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<ProjectSubTaskView>>.Error("Error listing subtasks");
            }
        }

        public async Task<StandardResponse<PagedCollection<ProjectTaskAsigneeView>>> ListProjectAssigneeDetail(PagingOptions pagingOptions, Guid projectId, string search = null)
        {
            try
            {
                var assignees = _projectTaskAsigneeRepository.Query().Include(x => x.User).Include(x => x.ProjectTask).Where(x => x.ProjectId == projectId).OrderByDescending(x => x.DateCreated);

                if (!string.IsNullOrEmpty(search))
                {
                    assignees = assignees.Where(x => x.ProjectTask.Name.ToLower().Contains(search.ToLower()) || (x.User.FirstName.ToLower() + " " + x.User.LastName.ToLower()).Contains(search.ToLower())).OrderByDescending(x => x.DateCreated); 
                }

                var pagedAssignee = assignees.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value).AsNoTracking();

                var mappedAssignee = pagedAssignee.ProjectTo<ProjectTaskAsigneeView>(_configuration).AsNoTracking();

                var pagedCollection = PagedCollection<ProjectTaskAsigneeView>.Create(Link.ToCollection(nameof(ProjectManagementController.ListProjectAssigneeDetail)), mappedAssignee.ToArray(), assignees.Count(), pagingOptions);

                return StandardResponse<PagedCollection<ProjectTaskAsigneeView>>.Ok(pagedCollection);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<ProjectTaskAsigneeView>>.Error("Error listing assignee");
            }
        }

        public async Task<StandardResponse<ProjectView>> GetProject(Guid projectId)
        {
            try
            {
                var project = _projectRepository.Query().Include(x => x.Assignees.Where(x => x.Disabled == false)).ThenInclude(x => x.User).
                    ThenInclude(x => x.EmployeeInformation).FirstOrDefault(x => x.Id == projectId);

                if (project == null) return StandardResponse<ProjectView>.NotFound("Project not found");

                var mappedProject = _mapper.Map<ProjectView>(project);

                mappedProject.Progress = GetProjectPercentageOfCompletion(projectId);
                if (mappedProject.IsCompleted) mappedProject.Progress = 100;

                var metrics = new ProjectMetrics { TotalBudget = project.Budget, TotalBudgetSpent = project.BudgetSpent, CurrentBalance = (project.Budget - project.BudgetSpent), TotalHourSpent = project.HoursSpent };

                mappedProject.ProjectMetrics = metrics;
                return StandardResponse<ProjectView>.Ok(mappedProject);
            }
            catch (Exception e)
            {
                return StandardResponse<ProjectView>.Error("Error getting Project");
            }
        }

        public async Task<StandardResponse<ProjectTaskView>> GetTask(Guid taskId)
        {
            try
            {
                var task = _projectTaskRepository.Query().Include(x => x.Assignees.Where(x => x.Disabled == false)).ThenInclude(x => x.User).FirstOrDefault(x => x.Id == taskId);

                if (task == null) return StandardResponse<ProjectTaskView>.NotFound("Task not found");

                var mappedTasked = _mapper.Map<ProjectTaskView>(task);
                mappedTasked.HoursSpent = GetHoursSpentOnTask(taskId);
                if (mappedTasked.IsCompleted) mappedTasked.PercentageOfCompletion = 100;
                //mappedTasked.Progress = GetTaskPercentageOfCompletion(taskId);

                return StandardResponse<ProjectTaskView>.Ok(mappedTasked);
            }
            catch (Exception e)
            {
                return StandardResponse<ProjectTaskView>.Error("Error getting task");
            }
        }

        public async Task<StandardResponse<ProjectSubTaskView>> GetSubTask(Guid subTaskId)
        {
            try
            {
                var subTask = _projectSubTaskRepository.Query().Include(x => x.ProjectTaskAsignee).FirstOrDefault(x => x.Id == subTaskId);
                //var subTask = _projectSubTaskRepository.Query().FirstOrDefault(x => x.Id == subTaskId);

                if (subTask == null) return StandardResponse<ProjectSubTaskView>.NotFound("Subtask not found");

                var mappedSubTask = _mapper.Map<ProjectSubTaskView>(subTask);

                return StandardResponse<ProjectSubTaskView>.Ok(mappedSubTask);
            }
            catch (Exception e)
            {
                return StandardResponse<ProjectSubTaskView>.Error("Error getting subtask");
            }
        }

        public async Task<StandardResponse<ProjectProgressCountView>> GetStatusCountForProject(Guid superAdminId, Guid? userId = null)
        {
            try
            {
                var projects = _projectRepository.Query().Include(x => x.Assignees.Where(x => x.Disabled == false)).Where(x => x.SuperAdminId == superAdminId);

                if (userId.HasValue)
                {
                    projects = projects.Where(x => x.Assignees.Any(x => x.UserId == userId));
                }

                var notStarted = projects.Where(x => x.StartDate > DateTime.Now && x.IsCompleted == false).Count(); 

                var inProgress = projects.Where(x => DateTime.Now > x.StartDate && x.IsCompleted == false).Count();

                var completed = projects.Where(x => x.IsCompleted == true).Count();

                var response = new ProjectProgressCountView { NotStarted = notStarted, InProgress = inProgress, Completed = completed };

                return StandardResponse<ProjectProgressCountView>.Ok(response);
            }
            catch (Exception e)
            {
                return StandardResponse<ProjectProgressCountView>.Error("Error getting response");
            }
        }

        public async Task<StandardResponse<ProjectProgressCountView>> GetStatusCountForOperationalTask(Guid superAdminId, Guid? userId = null)
        {
            try
            {
                var tasks = _projectTaskRepository.Query().Include(x => x.Assignees.Where(x => x.Disabled == false)).Where(x => x.SuperAdminId == superAdminId && 
                x.IsOperationalTask == true);

                if (userId.HasValue)
                {
                    tasks = tasks.Where(x => x.Assignees.Any(x => x.UserId == userId));
                }

                var notStarted = tasks.Where(x => x.OperationalTaskStatus.ToLower() == "to do").Count();

                var inProgress = tasks.Where(x => x.OperationalTaskStatus.ToLower() == "in progress").Count();

                var completed = tasks.Where(x => x.OperationalTaskStatus.ToLower() == "completed").Count();

                var response = new ProjectProgressCountView { NotStarted = notStarted, InProgress = inProgress, Completed = completed };

                return StandardResponse<ProjectProgressCountView>.Ok(response);
            }
            catch (Exception e)
            {
                return StandardResponse<ProjectProgressCountView>.Error("Error getting response");
            }
        }

        public async Task<StandardResponse<ProjectTimesheetListView>> ListUserProjectTimesheet(Guid employeeId, DateTime startDate, DateTime endDate, Guid? projectId = null)
        {
            try
            {
                var employee = _employeeInformationRepository.Query().FirstOrDefault(x => x.Id == employeeId);

                if (employee == null) return StandardResponse<ProjectTimesheetListView>.NotFound("Employee not found");
                var projectTimesheets = _projectTimesheetRepository.Query().Include(x => x.Project).Include(x => x.ProjectTaskAsignee).ThenInclude(x => x.User).Include(x => x.ProjectTask)
                    .Include(x => x.ProjectSubTask).Where(x => x.ProjectTaskAsignee.User.EmployeeInformationId == employeeId && x.StartDate.Date >= startDate.Date && x.EndDate.Date <= endDate);

                if (projectId.HasValue)
                {
                    projectTimesheets = projectTimesheets.Where(x =>  x.ProjectId == projectId.Value);   
                }

                var mappedTaskTimesheet = projectTimesheets.ProjectTo<ProjectTimesheetView>(_configuration).ToList();

                var totalHousBillable = mappedTaskTimesheet.Where(x => x.Billable == true).Sum(x => x.TotalHours);

                var totalHousNonBillable = mappedTaskTimesheet.Where(x => x.Billable == false).Sum(x => x.TotalHours);

                var response = new ProjectTimesheetListView { ProjectTimesheets = mappedTaskTimesheet, Billable = totalHousBillable, NonBillable = totalHousNonBillable };

                return StandardResponse<ProjectTimesheetListView>.Ok(response);
            }
            catch (Exception e)
            {
                return StandardResponse<ProjectTimesheetListView>.Error("Error listing timesheets");
            }
        }

        public async Task<StandardResponse<ProjectTimesheetListView>> ListSupervisorProjectTimesheet(Guid supervisorId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var user = _userRepository.Query().FirstOrDefault(x => x.Id == supervisorId);

                if (user == null) return StandardResponse<ProjectTimesheetListView>.NotFound("supervisor not found");
                var projectTimesheets = _projectTimesheetRepository.Query().Include(x => x.Project).Include(x => x.ProjectTaskAsignee).ThenInclude(x => x.User).ThenInclude(x => x.EmployeeInformation).Include(x => x.ProjectTask)
                    .Include(x => x.ProjectSubTask).Where(x => x.ProjectTaskAsignee.User.EmployeeInformation.SupervisorId == supervisorId && x.StartDate.Date >= startDate.Date && x.EndDate.Date <= endDate);

                var mappedTaskTimesheet = projectTimesheets.ProjectTo<ProjectTimesheetView>(_configuration).ToList();

                var totalHousBillable = mappedTaskTimesheet.Where(x => x.Billable == true).Sum(x => x.TotalHours);

                var totalHousNonBillable = mappedTaskTimesheet.Where(x => x.Billable == false).Sum(x => x.TotalHours);

                var response = new ProjectTimesheetListView { ProjectTimesheets = mappedTaskTimesheet, Billable = totalHousBillable, NonBillable = totalHousNonBillable };

                return StandardResponse<ProjectTimesheetListView>.Ok(response);
            }
            catch (Exception e)
            {
                return StandardResponse<ProjectTimesheetListView>.Error("Error listing timesheets");
            }
        }

        public async Task<StandardResponse<PagedCollection<ProjectTaskAsigneeView>>> ListProjectAssigneeTasks(PagingOptions pagingOptions, Guid superAdminId, Guid projectId, string search = null)
        {
            try
            {
                var superAdmin = _userRepository.Query().FirstOrDefault(x => x.Id == superAdminId);

                if (superAdmin == null) return StandardResponse<PagedCollection<ProjectTaskAsigneeView>>.NotFound("super admin not found");

                var assigneeTasks = _projectTaskAsigneeRepository.Query().Include(x => x.User).Include(x => x.ProjectTask).Where(x => x.ProjectId == projectId & x.ProjectTaskId != null).OrderByDescending(x => x.DateModified);

                if (!string.IsNullOrEmpty(search))
                {
                    assigneeTasks = assigneeTasks.Where(x => (x.User.FirstName + " " + x.User.LastName).ToLower().Contains(search.ToLower()) || x.ProjectTask.Name.ToLower().Contains(search.ToLower())).OrderByDescending(x => x.DateModified);
                }

                var pagedAssigneeTasks = assigneeTasks.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedassignedTasks = pagedAssigneeTasks.ProjectTo<ProjectTaskAsigneeView>(_configuration);

                var pagedCollection = PagedCollection<ProjectTaskAsigneeView>.Create(Link.ToCollection(nameof(ProjectManagementController.ListProjectAssigneeTasks)), mappedassignedTasks.ToArray(), assigneeTasks.Count(), pagingOptions);

                return StandardResponse<PagedCollection<ProjectTaskAsigneeView>>.Ok(pagedCollection);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<ProjectTaskAsigneeView>>.Error("Error listing assigned tasks");
            }
        }

        //public async Task<StandardResponse<ProjectTaskListView>> ListProjectTask(PagingOptions pagingOptions, Guid superAdminId, string search)
        //{
        //    try
        //    {
        //        var user = _userRepository.Query().FirstOrDefault(x => x.Id == superAdminId);

        //        if (user == null) return StandardResponse<ProjectTaskListView>.NotFound("User not found");

        //        var tasks = _projectTaskRepository.Query().Where(x => x.SuperAdminId == superAdminId);

        //        if (!string.IsNullOrEmpty(search))
        //        {
        //            tasks = tasks.Where(x => x.Name.ToLower().Contains(search.ToLower()));
        //        }

        //        var pageTasks = tasks.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

        //        var mappedTasks = pageTasks.ProjectTo<ProjectTaskView>(_configuration).ToList();

        //        foreach (var task in mappedTasks)
        //        {

        //        }

        //        //var projectLists = new ProjectListView
        //        //{
        //        //    NotStarted = projects.Where(x => x.StartDate > DateTime.Now).Count(),
        //        //    InProgress = projects.Where(x => DateTime.Now > x.StartDate && DateTime.Now < x.EndDate).Count(),
        //        //    Completed = projects.Where(x => DateTime.Now > x.EndDate).Count(),
        //        //    projects = mappedProjects
        //        //};

        //        return StandardResponse<ProjectListView>.Ok(projectLists);
        //    }
        //    catch (Exception e)
        //    {
        //        return StandardResponse<ProjectListView>.Error("Error listing Project");
        //    }
        //}

        public async Task<StandardResponse<BudgetSummaryReportView>> GetSummaryReport(Guid superAdminId, DateFilter dateFilter)
        {
            try
            {
                var users = _userRepository.Query().Where(x => x.SuperAdminId == superAdminId).Count();

                var projectTimesheet = _projectTimesheetRepository.Query().Include(x => x.Project).Where(x => x.Project.SuperAdminId == superAdminId);

                if (dateFilter.StartDate.HasValue) projectTimesheet = projectTimesheet.Where(x => x.DateCreated.Date >= dateFilter.StartDate.Value.Date);

                if (dateFilter.EndDate.HasValue) projectTimesheet = projectTimesheet.Where(x => x.DateCreated.Date <= dateFilter.EndDate.Value);

                var totalHours = projectTimesheet.Sum(x => x.TotalHours);

                var billlable = projectTimesheet.Where(x => x.Billable == true).Sum(x => x.TotalHours);

                var nonBillable = projectTimesheet.Where(x => x.Billable != true).Sum(x => x.TotalHours);

                var amount = projectTimesheet.Where(x => x.Billable == true).Sum(x => x.AmountEarned);

                var budgetRecord = new BudgetSummaryReportView { NoOfUsers = users, TotalHours = totalHours, BillableHours = billlable, NonBillableHours = nonBillable, Amount = amount };

                return StandardResponse<BudgetSummaryReportView>.Ok(budgetRecord);
            }
            catch (Exception e)
            {
                return StandardResponse<BudgetSummaryReportView>.Error("Error getting summary");
            }
        }

        public StandardResponse<byte[]> ExportSummaryReportRecord(BudgetRecordDownloadModel model, DateFilter dateFilter, Guid superAdminId)
        {
            try
            {
                var users = _userRepository.Query().Where(x => x.SuperAdminId == superAdminId).Count();

                var projectTimesheet = _projectTimesheetRepository.Query().Include(x => x.Project).Where(x => x.Project.SuperAdminId == superAdminId);

                if (dateFilter.StartDate.HasValue) projectTimesheet = projectTimesheet.Where(x => x.DateCreated.Date >= dateFilter.StartDate.Value.Date);

                if(dateFilter.EndDate.HasValue) projectTimesheet = projectTimesheet.Where(x => x.DateCreated.Date <=  dateFilter.EndDate.Value);

                var totalHours = projectTimesheet.Sum(x => x.TotalHours);

                var billlable = projectTimesheet.Where(x => x.Billable == true).Sum(x => x.TotalHours);

                var nonBillable = projectTimesheet.Where(x => x.Billable != true).Sum(x => x.TotalHours);

                var amount =projectTimesheet.Sum(x => x.AmountEarned);

                var budgetRecord = new BudgetSummaryReportView { NoOfUsers = users, TotalHours = totalHours, BillableHours = billlable, NonBillableHours = nonBillable, Amount =  amount };

                var workbook = _dataExport.ExportBudgetSummaryReport(budgetRecord, model.rowHeaders);

                return StandardResponse<byte[]>.Ok(workbook);
            }
            catch (Exception e)
            {
                return StandardResponse<byte[]>.Error("Error downloading report");
            }
        }

        public async Task<StandardResponse<bool>> MarkProjectOrTaskAsCompleted(MarkAsCompletedModel model)
        {
            try
            {
                if ((int)model.Type == 0) return StandardResponse<bool>.Failed("Enter a valid task type");

                switch (model.Type)
                {
                    case TaskType.Project:

                        var project = _projectRepository.Query().FirstOrDefault(x => x.Id == model.TaskId);

                        if (project == null) return StandardResponse<bool>.Failed("Project not found");

                        project.IsCompleted = true;

                        _projectRepository.Update(project);

                        await _notificationService.SendNotification(new NotificationModel { UserId = project.SuperAdminId, Title = "Project Completed", Type = "Notification", Message = $"{project.Name} has been successfully marked as completed" });

                        break;

                    case TaskType.Task:

                        var task = _projectTaskRepository.Query().FirstOrDefault(x => x.Id == model.TaskId);

                        if (task == null) return StandardResponse<bool>.Failed("Task not found");

                        task.IsCompleted = true;

                        _projectTaskRepository.Update(task);

                        await _notificationService.SendNotification(new NotificationModel { UserId = task.SuperAdminId, Title = "Task Completed", Type = "Notification", Message = $"{task.Name} has been successfully marked as completed" });

                        break;

                    case TaskType.Subtask:

                        var subTask = _projectSubTaskRepository.Query().Include(x => x.ProjectTask).FirstOrDefault(x => x.Id == model.TaskId);

                        if (subTask == null) return StandardResponse<bool>.Failed("Subtask not found");

                        subTask.IsCompleted = true;

                        _projectSubTaskRepository.Update(subTask);

                        await _notificationService.SendNotification(new NotificationModel { UserId = subTask.ProjectTask.SuperAdminId, Title = "Subtask Completed", Type = "Notification", Message = $"{subTask.Name} has been successfully marked as completed" });

                        break;

                    default:

                        return StandardResponse<bool>.Failed("An error occured");

                        break;

                }
                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception e)
            {
                return StandardResponse<bool>.Error("An error occured");
            }
        }

        public double? GetProjectPercentageOfCompletion(Guid projectId)
        {
           
            var tasks = _projectTaskRepository.Query().Where(x => x.ProjectId == projectId).ToList();

            double? projectProgress = 0;

            if(tasks.Count() > 0)
            {
                var totalHoursForTask = tasks.Sum(x => x.DurationInHours);
                foreach(var task in tasks)
                {
                    projectProgress += (task.DurationInHours / totalHoursForTask) * task.PercentageOfCompletion;

                    //double? subTaskProgress = 0;
                    //var subTasks = _projectSubTaskRepository.Query().Where(x => x.ProjectTaskId == task.Id).ToList();



                    //if (subTasks.Count() > 0)
                    //{
                    //    var totalHoursForSubTasks = subTasks.Sum(x => x.DurationInHours);
                    //    foreach (var subTask in subTasks)
                    //    {
                    //        subTaskProgress += (subTask.DurationInHours / totalHoursForSubTasks) * (subTask.PercentageOfCompletion / 100);
                    //        //var timeSheets = _projectTimesheetRepository.Query().Where(x => x.ProjectSubTaskId == subTask.Id).ToList();
                    //        //foreach (var timeSheet in timeSheets)
                    //        //{
                    //        //    subTaskProgress += (subTask.DurationInHours / totalHoursForSubTasks) * (subTask.PercentageOfCompletion / 100);
                    //        //}
                    //    }

                    //    projectProgress += subTaskProgress * (task.DurationInHours / totalHoursForTask);
                    //}

                    //var timeSheetsForTasks = _projectTimesheetRepository.Query().Where(x => x.ProjectTaskId == task.Id && x.ProjectSubTaskId == null).ToList();

                    //if (timeSheetsForTasks.Count() > 0)
                    //{
                    //    foreach (var timeSheet in timeSheetsForTasks)
                    //    {
                    //        projectProgress += (timeSheet.TotalHours / totalHoursForTask) * (task.PercentageOfCompletion / 100);
                    //    }
                    //}
                }
            }
            return projectProgress;


        }

        public double? GetTaskPercentageOfCompletion(Guid taskId)
        {
            var task = _projectTaskRepository.Query().FirstOrDefault(x => x.Id == taskId);
            double? taskProgress = 0;
            var subTasks = _projectSubTaskRepository.Query().Where(x => x.ProjectTaskId == taskId).ToList();

            //Get all subtasks in a 

            if (subTasks.Count() > 0)
            {
                var totalHoursForSubTasks = subTasks.Sum(x => x.DurationInHours);
                foreach (var subTask in subTasks)
                {
                    taskProgress += (subTask.DurationInHours / totalHoursForSubTasks) * (subTask.PercentageOfCompletion / 100);
                    //var timeSheets = _projectTimesheetRepository.Query().Where(x => x.ProjectSubTaskId == subTask.Id).ToList();
                    //foreach (var timeSheet in timeSheets)
                    //{
                    //    taskProgress += (timeSheet.DurationInHours / totalHoursForSubTasks) * (subTask.PercentageOfCompletion / 100);
                    //}
                }
            }
            else
            {
                taskProgress += task.PercentageOfCompletion / 100;
            }



            //var timeSheetsForTasks = _projectTimesheetRepository.Query().Where(x => x.ProjectTaskId == taskId && x.ProjectSubTaskId == null).ToList();

            //if (timeSheetsForTasks.Count() > 0)
            //{
            //    foreach (var timeSheet in timeSheetsForTasks)
            //    {
            //        taskProgress += (timeSheet.TotalHours / task.DurationInHours) * task.PercentageOfCompletion / 100;
            //    }
            //}
            return taskProgress;
        }

        public double GetHoursSpentOnTask(Guid taskId)
        {
            double totalHours = 0;
            var task = _projectTaskRepository.Query().FirstOrDefault(x => x.Id == taskId);

            var subTasks = _projectSubTaskRepository.Query().Where(x => x.ProjectTaskId == taskId).ToList();

            if (subTasks.Count > 0)
            {
                foreach (var subTask in subTasks)
                {
                    var timeSheets = _projectTimesheetRepository.Query().Where(x => x.ProjectSubTaskId == subTask.Id).ToList();
                    foreach (var timeSheet in timeSheets)
                    {
                        totalHours += (timeSheet.EndDate - timeSheet.StartDate).TotalHours;
                    }
                }
            }

            var timeSheetsForTasks = _projectTimesheetRepository.Query().Where(x => x.ProjectTaskId == task.Id && x.ProjectSubTaskId == null).ToList();
            foreach (var timeSheet in timeSheetsForTasks)
            {
                totalHours += (timeSheet.EndDate - timeSheet.StartDate).TotalHours;
            }

            return totalHours;
        }

        public async Task<StandardResponse<PagedCollection<ResourceCapacityView>>> GetResourcesCapacityOverview(PagingOptions pagingOptions, Guid superAdminId, DateFilter dateFilter = null)
        {
            try
            {
                //var resources = _userRepository.Query().Include(x => x.EmployeeInformation).Where(x => x.Role.ToLower() == "team member" && x.SuperAdminId == superAdminId).Join(_projectTaskAsigneeRepository.Query(), user => user.Id, assignee => assignee.UserId, (user, assignee)
                //    => new { User = user, Assignee = assignee }).ToList();

                var users = _userRepository.Query().Include(x => x.EmployeeInformation).Where(x => x.Role.ToLower() == "team member" && x.SuperAdminId == superAdminId && x.IsActive == true).OrderByDescending(u => u.DateCreated);

                if (dateFilter.StartDate.HasValue)
                    users = users.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

                if (dateFilter.EndDate.HasValue)
                    users = users.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);

                var pageUsers = users.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value).ToList();

                var resourcesOverview = new List<ResourceCapacityView>();

                foreach (var user in pageUsers)
                {
                    var assigneeProjectCount = _projectTaskAsigneeRepository.Query().Count(x => x.ProjectId != null && x.ProjectTaskId == null && x.UserId == user.Id);

                    var assigneeTaskCount = _projectTaskAsigneeRepository.Query().Count(x => x.ProjectTaskId != null && x.UserId == user.Id);

                    var assigneeTaskCompleted = _projectTaskAsigneeRepository.Query().Include(x => x.ProjectTask).Count(x => x.ProjectTask.IsCompleted && x.UserId == user.Id);

                    var overview = new ResourceCapacityView
                    {
                        UserId = user.Id,
                        Name = user.FullName,
                        JobTitle = user.EmployeeInformation.JobTitle,
                        TotalNumberOfTask = assigneeTaskCount,
                        TotalNumberOfProject = assigneeProjectCount,
                        NoOfTaskCompleted = assigneeTaskCompleted
                    };

                    resourcesOverview.Add(overview);
                }

                var pagedCollection = PagedCollection<ResourceCapacityView>.Create(Link.ToCollection(nameof(ProjectManagementController.GetResourcesCapacityOverview)), resourcesOverview.ToArray(), users.Count(), pagingOptions);

                return StandardResponse<PagedCollection<ResourceCapacityView>>.Ok(pagedCollection);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<ResourceCapacityView>>.Error("Error listing resource overview");
            }


        }

        public async Task<StandardResponse<PagedCollection<ResourceCapacityDetailView>>> GetResourceDetails(PagingOptions pagingOptions, Guid userId, Guid? projectId = null, ProjectStatus? status = null, string search = null)
        {
            try
            {
                var assignedTasks = _projectTaskAsigneeRepository.Query().Include(x => x.Project).Include(x => x.ProjectTask).
                    Where(x => x.ProjectTaskId != null && x.ProjectId != null && x.UserId == userId).OrderByDescending(x => x.DateModified);

                if(projectId != null)
                {
                    assignedTasks = assignedTasks.Where(x => x.ProjectId == projectId).OrderByDescending(x => x.DateModified);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    assignedTasks = assignedTasks.Where(x => x.ProjectTask.Name.Contains(search)).OrderByDescending(x => x.DateModified);
                }

                if (status.HasValue && status == ProjectStatus.NotStarted)
                {
                    assignedTasks = assignedTasks.Where(x => x.ProjectTask.StartDate > DateTime.Now).OrderByDescending(x => x.DateModified);
                }
                else if (status.HasValue && status.Value == ProjectStatus.InProgress)
                {
                    assignedTasks = assignedTasks.Where(x => DateTime.Now > x.ProjectTask.StartDate && x.ProjectTask.IsCompleted == false).OrderByDescending(x => x.DateModified);
                }
                else if (status.HasValue && status.Value == ProjectStatus.Completed)
                {
                    assignedTasks = assignedTasks.Where(x => x.ProjectTask.IsCompleted == true).OrderByDescending(x => x.DateModified);
                }

                var pagedResult = assignedTasks.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value).ToList();

                var resourcesOverview = new List<ResourceCapacityDetailView>();

                foreach(var result in pagedResult)
                {
                    var record = new ResourceCapacityDetailView
                    {
                        TaskName = result.ProjectTask.Name,
                        ProjectName = result.Project.Name,
                        TotalHours = result.HoursLogged,
                        StartDate = result.ProjectTask.StartDate,
                        EndDate = result.ProjectTask.EndDate,
                        Status = result.ProjectTask.GetStatus()
                    };

                    resourcesOverview.Add(record);
                }

                var pagedCollection = PagedCollection<ResourceCapacityDetailView>.Create(Link.ToCollection(nameof(ProjectManagementController.GetResourceDetails)), resourcesOverview.ToArray(), assignedTasks.Count(), pagingOptions);

                return StandardResponse<PagedCollection<ResourceCapacityDetailView>>.Ok(pagedCollection);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<ResourceCapacityDetailView>>.Error("Error viewing resource detail");
            }


        }

        public async Task<StandardResponse<bool>> UpdateTaskProgress(Guid taskId, double percentageOfCompletion)
        {
            try
            {
                var task = _projectTaskRepository.Query().FirstOrDefault(x => x.Id == taskId);

                if(task == null) return StandardResponse<bool>.Failed("Task not found");

                task.PercentageOfCompletion = percentageOfCompletion;

                _projectTaskRepository.Update(task);

                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception e)
            {
                return StandardResponse<bool>.Error("An error occured");
            }
        }

        public async Task<StandardResponse<bool>> UpdateSubtaskProgress(Guid subTaskId, double percentageOfCompletion)
        {
            try
            {
                var subtask = _projectSubTaskRepository.Query().FirstOrDefault(x => x.Id == subTaskId);

                if (subtask == null) return StandardResponse<bool>.Failed("Subtask not found");

                subtask.PercentageOfCompletion = percentageOfCompletion;

                _projectSubTaskRepository.Update(subtask);

                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception e)
            {
                return StandardResponse<bool>.Error("An error occured");
            }
        }
    }
}
