﻿using AutoMapper;
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
        public ProjectManagementService(IProjectRepository projectRepository, IProjectTaskRepository projectTaskRepository, IProjectSubTaskRepository projectSubTaskRepository, 
            IUserRepository userRepository, IProjectTaskAsigneeRepository projectTaskAsigneeRepository, IProjectTimesheetRepository projectTimesheetRepository, IMapper mapper,
            IConfigurationProvider configuration, IHttpContextAccessor httpContext, ITimeSheetService timeSheetService, IDataExport dataExport)
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
        }

        //Create a project

        public async Task<StandardResponse<bool>> CreateProject(ProjectModel model)
        {
            try
            {
                var project = _mapper.Map<Project>(model);
                project.BudgetSpent = 0;

                project = _projectRepository.CreateAndReturn(project);

                model.AssignedUsers.ForEach(id =>
                {
                    var assignee = new ProjectTaskAsignee { UserId = id, ProjectId = project.Id };
                    _projectTaskAsigneeRepository.CreateAndReturn(assignee);
                });
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

                project = _projectRepository.Update(project);

                //model.AssignedUsers.ForEach(id =>
                //{
                //    var assignee = new ProjectTaskAsignee { UserId = id, ProjectId = project.Id };
                //    _projectTaskAsigneeRepository.CreateAndReturn(assignee);
                //});
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
                if (model.Category.HasValue && (int)model.Category == 0) return StandardResponse<bool>.Failed("Enter a valid category");

                if ((int)model.TaskPriority == 0) return StandardResponse<bool>.Failed("Select a task priority");

                if (model.ProjectId.HasValue)
                {
                    var project = _projectRepository.Query().FirstOrDefault(x => x.Id == model.ProjectId.Value);
                    if (project == null) return StandardResponse<bool>.NotFound("The project does not exist");
                }
                

                var task = _mapper.Map<ProjectTask>(model);
                task.Category = model.Category.HasValue ? model.Category.ToString() : null;
                task.TaskPriority = model.TaskPriority.ToString();
                task.DurationInHours = (model.EndDate - model.StartDate).TotalHours / 3;
                if (model.TrackedByHours) task.DurationInHours = model.DurationInHours.Value;

                task = _projectTaskRepository.CreateAndReturn(task);

                model.AssignedUsers.ForEach(id =>
                {
                    var assignee = new ProjectTaskAsignee();
                    var budget = (decimal)(_timeSheetService.GetTeamMemberPayPerHour(id) * task.DurationInHours);
                    if (model.ProjectId.HasValue) assignee = new ProjectTaskAsignee { UserId = id, ProjectId = model.ProjectId, ProjectTaskId = task.Id, Budget = budget };
                    if (!model.ProjectId.HasValue) assignee = new ProjectTaskAsignee { UserId = id, ProjectTaskId = task.Id };
                    _projectTaskAsigneeRepository.CreateAndReturn(assignee);
                });
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

                if (model.Category.HasValue && (int)model.Category == 0) return StandardResponse<bool>.Failed("Enter a valid category");

                if ((int)model.TaskPriority == 0) return StandardResponse<bool>.Failed("Select a task priority");

                task.Name = model.Name;
                task.TrackedByHours = model.TrackedByHours;
                task.StartDate = model.StartDate;
                task.EndDate = model.EndDate;
                task.Category = model.Category.HasValue ? model.Category.ToString() : null;
                task.TaskPriority = model.TaskPriority.ToString();
                task.DurationInHours = (model.EndDate - model.StartDate).TotalHours / 3;
                if (model.TrackedByHours) task.DurationInHours = model.DurationInHours.Value;


                //remove assigned users

                //if (task.Assignees.Any())
                //{
                //    foreach(var assignee in task.Assignees)
                //    {
                //        if (_projectSubTaskRepository.Query().Any(x => x.ProjectTaskAsigneeId == assignee.Id)) return StandardResponse<bool>.Failed("You cant update this task because one of the assignee is assigned to a subtasks");
                //        if (_projectTimesheetRepository.Query().Any(x => x.ProjectTaskAsigneeId == assignee.Id)) return StandardResponse<bool>.Failed("You cant update this task because one of the assignee has filled their timesheet");
                //        _projectTaskAsigneeRepository.Delete(assignee);
                //    }
                //}

                //if(!model.AssignedUsers.Any()) return StandardResponse<bool>.Failed("Kindly add an assignee for this tasks");

                //model.AssignedUsers.ForEach(id =>
                //{
                //    var assignee = new ProjectTaskAsignee();
                //    var budget = (decimal)(_timeSheetService.GetTeamMemberPayPerHour(id) * task.DurationInHours);
                //    if (model.ProjectId.HasValue) assignee = new ProjectTaskAsignee { UserId = id, ProjectId = model.ProjectId, ProjectTaskId = task.Id, Budget = budget };
                //    if (!model.ProjectId.HasValue) assignee = new ProjectTaskAsignee { UserId = id, ProjectTaskId = task.Id };
                //    _projectTaskAsigneeRepository.CreateAndReturn(assignee);
                //});

                

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

                if (subTask.DurationInHours.Value > task.DurationInHours) return StandardResponse<bool>.NotFound("Subtask duration cannot be greater than the task duration");

                subTask = _projectSubTaskRepository.CreateAndReturn(subTask);

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

                if (subtask.DurationInHours.Value > task.DurationInHours) return StandardResponse<bool>.NotFound("Subtask duration cannot be greater than the task duration");

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
                var assignee = _projectTaskAsigneeRepository.Query().FirstOrDefault(x => x.Id == model.ProjectTaskAsigneeId);

                if (assignee == null) return StandardResponse<bool>.NotFound("You are not assigned to this project");

                var project = _projectRepository.Query().FirstOrDefault(x => x.Id == model.ProjectId);

                if (project == null) return StandardResponse<bool>.NotFound("The project does not exist");

                var task = _projectTaskRepository.Query().FirstOrDefault(x => x.Id == model.ProjectTaskId);

                if (task == null) return StandardResponse<bool>.NotFound("Task not found");

                if(model.ProjectSubTaskId.HasValue)
                {
                    var subTask = _projectSubTaskRepository.Query().FirstOrDefault(x => x.Id == model.ProjectSubTaskId.Value);
                    if (subTask == null) return StandardResponse<bool>.NotFound("subtask not found");
                }

                foreach(var newTimesheet in model.ProjectTimesheets)
                {
                    var timesheet = _mapper.Map<ProjectTimesheet>(model);

                    timesheet.TotalHours = (newTimesheet.EndDate - newTimesheet.StartDate).TotalHours;

                    timesheet.AmountEarned = (decimal)(_timeSheetService.GetTeamMemberPayPerHour(assignee.UserId) * timesheet.TotalHours);

                    timesheet.StatusId = (int)Statuses.PENDING;

                    timesheet = _projectTimesheetRepository.CreateAndReturn(timesheet);

                    assignee.HoursLogged += (newTimesheet.EndDate - newTimesheet.StartDate).TotalHours;

                    if (assignee.Budget != null && newTimesheet.Billable) assignee.BudgetSpent += timesheet.AmountEarned;

                    _projectTaskAsigneeRepository.Update(assignee);

                    var updateTimesheet = await _timeSheetService.AddProjectManagementTimeSheet(assignee.UserId, newTimesheet.StartDate, newTimesheet.EndDate);

                    if (newTimesheet.Billable) project.BudgetSpent += timesheet.AmountEarned;

                    project.HoursSpent += timesheet.TotalHours;

                    _projectRepository.Update(project);
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
                var timesheet = _projectTimesheetRepository.Query().FirstOrDefault(x => x.Id == model.TimesheetId);

                if (timesheet == null) return StandardResponse<bool>.NotFound("Timesheet not found");

                var assignee = _projectTaskAsigneeRepository.Query().FirstOrDefault(x => x.Id == timesheet.ProjectTaskAsigneeId);

                if (model.Approve)
                {
                    timesheet.StatusId = (int)Statuses.APPROVED;
                }
                else
                {
                    timesheet.StatusId = (int)Statuses.REJECTED;
                    timesheet.Reason = model.Reason;
                }

                var updateTimesheet = await _timeSheetService.TreatProjectManagementTimeSheet(assignee.UserId, model.Approve, timesheet.StartDate, timesheet.EndDate, model.Reason);

                _projectTimesheetRepository.Update(timesheet);

                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception e)
            {
                return StandardResponse<bool>.Error("Error Treating timesheet");
            }
        }


        public async Task<StandardResponse<PagedCollection<ProjectView>>> ListProject(PagingOptions pagingOptions, Guid superAdminId, ProjectStatus? status, Guid? userId = null, string search = null)
        {
            try
            {
                var superAdmin = _userRepository.Query().FirstOrDefault(x => x.Id == superAdminId);

                if (superAdmin == null) return StandardResponse<PagedCollection<ProjectView>>.NotFound("User not found");

                var projects = _projectRepository.Query().Include(x => x.Assignees).ThenInclude(x => x.User).Where(x => x.SuperAdminId == superAdminId);

                if(userId != null)
                {
                    projects = projects.Where(x => x.Assignees.Any(x => x.UserId == userId));
                }

                if(!string.IsNullOrEmpty(search))
                {
                    projects = projects.Where(x => x.Name.ToLower().Contains(search.ToLower()));
                }

                if (status.HasValue && status == ProjectStatus.NotStarted)
                {
                    projects = projects.Where(x => x.StartDate > DateTime.Now);
                }else if (status.HasValue && status.Value == ProjectStatus.InProgress)
                {
                    projects = projects.Where(x => DateTime.Now > x.StartDate && DateTime.Now < x.EndDate);
                }else if(status.HasValue && status.Value == ProjectStatus.Completed)
                {
                    projects = projects.Where(x => x.IsCompleted == true);
                }

                var pageProjects = projects.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedProjects = pageProjects.ProjectTo<ProjectView>(_configuration).ToList();

                foreach (var project in mappedProjects)
                {
                    var progress = GetProjectPercentageOfCompletion(project.Id);
                    project.Progress = progress;
                }

                var pagedCollection = PagedCollection<ProjectView>.Create(Link.ToCollection(nameof(ProjectManagementController.ListProject)), mappedProjects.ToArray(), projects.Count(), pagingOptions);

                return StandardResponse<PagedCollection<ProjectView>>.Ok(pagedCollection);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<ProjectView>>.Error("Error listing Project");
            }
        }

        public async Task<StandardResponse<PagedCollection<ProjectTaskView>>> ListTasks(PagingOptions pagingOptions, Guid superAdminId, Guid? projectId, ProjectStatus? status, Guid? userId = null, string search = null)
        {
            try
            {
                var superAdmin = _userRepository.Query().FirstOrDefault(x => x.Id == superAdminId);

                if (superAdmin == null) return StandardResponse<PagedCollection<ProjectTaskView>>.NotFound("User not found");

                var tasks = _projectTaskRepository.Query().Include(x => x.SubTasks).Include(x => x.Assignees).ThenInclude(x => x.User).Where(x => x.SuperAdminId == superAdminId);
                //var tasks = _projectTaskRepository.Query().Include(x => x.Assignees).ThenInclude(x => x.User).Where(x => x.SuperAdminId == superAdminId && x.ProjectId == projectId);

                if (projectId.HasValue)
                {
                    tasks = tasks.Where(x => x.ProjectId == projectId);
                }

                if (!projectId.HasValue)
                {
                    tasks = tasks.Where(x => x.Category == null);
                }

                if (userId != null)
                {
                    tasks = tasks.Where(x => x.Assignees.Any(x => x.UserId == userId));
                }

                if (!string.IsNullOrEmpty(search))
                {
                    tasks = tasks.Where(x => x.Name.ToLower().Contains(search.ToLower()));
                }

                if (status.HasValue && status == ProjectStatus.NotStarted)
                {
                    tasks = tasks.Where(x => x.StartDate > DateTime.Now);
                }
                else if (status.HasValue && status.Value == ProjectStatus.InProgress)
                {
                    tasks = tasks.Where(x => DateTime.Now > x.StartDate && DateTime.Now < x.EndDate);
                }
                else if (status.HasValue && status.Value == ProjectStatus.Completed)
                {
                    tasks = tasks.Where(x => x.IsCompleted == true);
                }

                var pagedTasks = tasks.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedTasks = pagedTasks.ProjectTo<ProjectTaskView>(_configuration).ToList();

                foreach (var task in mappedTasks)
                {
                    var hours = GetHoursSpentOnTask(task.Id);
                    task.HoursSpent = hours;
                    task.Progress = GetTaskPercentageOfCompletion(task.Id);
                }

                var pagedCollection = PagedCollection<ProjectTaskView>.Create(Link.ToCollection(nameof(ProjectManagementController.ListTasks)), mappedTasks.ToArray(), tasks.Count(), pagingOptions);

                return StandardResponse<PagedCollection<ProjectTaskView>>.Ok(pagedCollection);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<ProjectTaskView>>.Error("Error listing tasks");
            }
        }

        public async Task<StandardResponse<PagedCollection<ProjectTaskView>>> ListOperationalTasks(PagingOptions pagingOptions, Guid superAdminId, ProjectStatus? status, Guid? userId = null, string search = null)
        {
            try
            {
                var user = _userRepository.Query().FirstOrDefault(x => x.Id == superAdminId);

                if (user == null) return StandardResponse<PagedCollection<ProjectTaskView>>.NotFound("User not found");

                var tasks = _projectTaskRepository.Query().Include(x => x.SubTasks).Include(x => x.Assignees).ThenInclude(x => x.User).Where(x => x.SuperAdminId == superAdminId && x.ProjectId == null);
                //var tasks = _projectTaskRepository.Query().Include(x => x.Assignees).ThenInclude(x => x.User).Where(x => x.SuperAdminId == superAdminId && x.ProjectId == projectId);

                if (userId != null)
                {
                    tasks = tasks.Where(x => x.Assignees.Any(x => x.UserId == userId));
                }

                if (!string.IsNullOrEmpty(search))
                {
                    tasks = tasks.Where(x => x.Name.ToLower().Contains(search.ToLower()));
                }

                if (status.HasValue && status == ProjectStatus.NotStarted)
                {
                    tasks = tasks.Where(x => x.StartDate > DateTime.Now);
                }
                else if (status.HasValue && status.Value == ProjectStatus.InProgress)
                {
                    tasks = tasks.Where(x => DateTime.Now > x.StartDate && DateTime.Now < x.EndDate);
                }
                else if (status.HasValue && status.Value == ProjectStatus.Completed)
                {
                    tasks = tasks.Where(x => x.IsCompleted == true);
                }

                var pagedTasks = tasks.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedTasks = pagedTasks.ProjectTo<ProjectTaskView>(_configuration).ToList();

                foreach (var task in mappedTasks)
                {
                    var hours = GetHoursSpentOnTask(task.Id);
                    task.HoursSpent = hours;
                    task.Progress = GetTaskPercentageOfCompletion(task.Id);
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

                var assigneeTasks = _projectTaskAsigneeRepository.Query().Include(x => x.ProjectTask).Include(x => x.SubTasks).ThenInclude(x => x.ProjectTimesheets).Where(x => x.ProjectId == projectId && x.UserId == userId && x.ProjectTaskId != null);

                //var assigneeTasks = _projectTaskAsigneeRepository.Query().Include(x => x.ProjectTask).Where(x => x.ProjectId == projectId && x.UserId == userId);

                var pagedTasks = assigneeTasks.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedTasks = pagedTasks.ProjectTo<ProjectTaskAsigneeView>(_configuration).ToList();

                var pagedCollection = PagedCollection<ProjectTaskAsigneeView>.Create(Link.ToCollection(nameof(ProjectManagementController.GetUserTasks)), mappedTasks.ToArray(), assigneeTasks.Count(), pagingOptions);

                return StandardResponse<PagedCollection<ProjectTaskAsigneeView>>.Ok(pagedCollection);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<ProjectTaskAsigneeView>>.Error("Error listing user tasks");
            }
        }

        public async Task<StandardResponse<PagedCollection<ProjectSubTaskView>>> ListSubTasks(PagingOptions pagingOptions, Guid? taskId, ProjectStatus? status, string search = null)
        {
            try
            {
                var task = _projectTaskRepository.Query().FirstOrDefault(x => x.Id == taskId);

                if (task == null) return StandardResponse<PagedCollection<ProjectSubTaskView>>.NotFound("Task not found");

                var subtasks = _projectSubTaskRepository.Query().Include(x => x.ProjectTask).Include(x => x.ProjectTimesheets).Include(x => x.ProjectTaskAsignee).AsQueryable();

                //var subtasks = _projectSubTaskRepository.Query().Where(x => x.ProjectTaskId == taskId);
                //var sublist = subtasks.ToList();

                if (taskId.HasValue)
                {
                    subtasks = subtasks.Where(x => x.ProjectTaskId == taskId);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    subtasks = subtasks.Where(x => x.Name.ToLower().Contains(search.ToLower()));
                }

                if (status.HasValue && status == ProjectStatus.NotStarted)
                {
                    subtasks = subtasks.Where(x => x.StartDate > DateTime.Now);
                }
                else if (status.HasValue && status.Value == ProjectStatus.InProgress)
                {
                    subtasks = subtasks.Where(x => DateTime.Now > x.StartDate && DateTime.Now < x.EndDate);
                }
                else if (status.HasValue && status.Value == ProjectStatus.Completed)
                {
                    subtasks = subtasks.Where(x => x.IsCompleted == true);
                }

                var pagedTasks = subtasks.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value).AsNoTracking();

                var mappedTasks = pagedTasks.ProjectTo<ProjectSubTaskView>(_configuration).AsNoTracking();

                var pagedCollection = PagedCollection<ProjectSubTaskView>.Create(Link.ToCollection(nameof(ProjectManagementController.ListSubTasks)), mappedTasks.ToArray(), subtasks.Count(), pagingOptions);

                return StandardResponse<PagedCollection<ProjectSubTaskView>>.Ok(pagedCollection);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<ProjectSubTaskView>>.Error("Error listing subtasks");
            }
        }

        public async Task<StandardResponse<ProjectView>> GetProject(Guid projectId)
        {
            try
            {
                var project = _projectRepository.Query().Include(x => x.Assignees).ThenInclude(x => x.User).FirstOrDefault(x => x.Id == projectId);

                if (project == null) return StandardResponse<ProjectView>.NotFound("Project not found");

                var mappedProject = _mapper.Map<ProjectView>(project);

                mappedProject.Progress = GetProjectPercentageOfCompletion(projectId);

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
                var task = _projectTaskRepository.Query().Include(x => x.Assignees).ThenInclude(x => x.User).FirstOrDefault(x => x.Id == taskId);

                if (task == null) return StandardResponse<ProjectTaskView>.NotFound("Task not found");

                var mappedTasked = _mapper.Map<ProjectTaskView>(task);
                mappedTasked.HoursSpent = GetHoursSpentOnTask(taskId);
                mappedTasked.Progress = GetTaskPercentageOfCompletion(taskId);

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

        public async Task<StandardResponse<ProjectProgressCountView>> GetStatusCountForProject(Guid superAdminId, Guid? userId)
        {
            try
            {
                var projects = _projectRepository.Query().Include(x => x.Assignees).Where(x => x.SuperAdminId == superAdminId);

                if (userId.HasValue)
                {
                    projects = projects.Where(x => x.Assignees.Any(x => x.UserId == userId));
                }

                var notStarted = projects.Where(x => x.StartDate > DateTime.Now).Count(); 

                var inProgress = projects.Where(x => DateTime.Now > x.StartDate).Count();

                var completed = projects.Where(x => x.IsCompleted == true).Count();

                var response = new ProjectProgressCountView { NotStarted = notStarted, InProgress = inProgress, Completed = completed };

                return StandardResponse<ProjectProgressCountView>.Ok(response);
            }
            catch (Exception e)
            {
                return StandardResponse<ProjectProgressCountView>.Error("Error getting response");
            }
        }

        public async Task<StandardResponse<ProjectTimesheetListView>> ListUserProjectTimesheet(Guid userId, DateTime startDate, DateTime endDate, Guid? projectId)
        {
            try
            {
                var user = _userRepository.Query().FirstOrDefault(x => x.Id == userId);

                if (user == null) return StandardResponse<ProjectTimesheetListView>.NotFound("user not found");
                var projectTimesheets = _projectTimesheetRepository.Query().Include(x => x.Project).Include(x => x.ProjectTaskAsignee).ThenInclude(x => x.User).Include(x => x.ProjectTask)
                    .Include(x => x.ProjectSubTask).Where(x => x.ProjectTaskAsignee.UserId == userId && x.StartDate.Date >= startDate.Date && x.EndDate.Date <= endDate);

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

                var assigneeTasks = _projectTaskAsigneeRepository.Query().Include(x => x.User).Include(x => x.ProjectTask).Where(x => x.ProjectId == projectId & x.ProjectTaskId != null);

                if (!string.IsNullOrEmpty(search))
                {
                    assigneeTasks = assigneeTasks.Where(x => (x.User.FirstName + " " + x.User.LastName).ToLower().Contains(search.ToLower()) || x.ProjectTask.Name.ToLower().Contains(search.ToLower()));
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

                var amount = projectTimesheet.Sum(x => x.AmountEarned);

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

                        break;

                    case TaskType.Task:

                        var task = _projectTaskRepository.Query().FirstOrDefault(x => x.Id == model.TaskId);

                        if (task == null) return StandardResponse<bool>.Failed("Task not found");

                        task.IsCompleted = true;

                        _projectTaskRepository.Update(task);

                        break;

                    case TaskType.Subtask:

                        var subTask = _projectSubTaskRepository.Query().FirstOrDefault(x => x.Id == model.TaskId);

                        if (subTask == null) return StandardResponse<bool>.Failed("Subtask not found");

                        subTask.IsCompleted = true;

                        _projectSubTaskRepository.Update(subTask);

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



        private double? GetProjectPercentageOfCompletion(Guid projectId)
        {
           
            var tasks = _projectTaskRepository.Query().Where(x => x.ProjectId == projectId).ToList();

            double? projectProgress = 0;

            if(tasks.Count() > 0)
            {
                var totalHoursForTask = tasks.Sum(x => x.DurationInHours);
                foreach(var task in tasks)
                {
                    double? subTaskProgress = 0;
                    var subTasks = _projectSubTaskRepository.Query().Where(x => x.ProjectTaskId == task.Id).ToList();



                    if (subTasks.Count() > 0)
                    {
                        var totalHoursForSubTasks = subTasks.Sum(x => x.DurationInHours);
                        foreach (var subTask in subTasks)
                        {
                            var timeSheets = _projectTimesheetRepository.Query().Where(x => x.ProjectSubTaskId == subTask.Id).ToList();
                            foreach (var timeSheet in timeSheets)
                            {
                                subTaskProgress += (subTask.DurationInHours / totalHoursForSubTasks) * (timeSheet.PercentageOfCompletion / 100);
                            }
                        }

                        projectProgress += subTaskProgress * (task.DurationInHours / totalHoursForTask);
                    }

                    var timeSheetsForTasks = _projectTimesheetRepository.Query().Where(x => x.ProjectTaskId == task.Id && x.ProjectSubTaskId == null).ToList();

                    if (timeSheetsForTasks.Count() > 0)
                    {
                        foreach (var timeSheet in timeSheetsForTasks)
                        {
                            projectProgress += (task.DurationInHours / totalHoursForTask) * (timeSheet.PercentageOfCompletion / 100);
                        }
                    }
                }
            }
            return projectProgress;


        }

        private double? GetTaskPercentageOfCompletion(Guid taskId)
        {

            double? taskProgress = 0;
            var subTasks = _projectSubTaskRepository.Query().Where(x => x.ProjectTaskId == taskId).ToList();

            if (subTasks.Count() > 0)
            {
                var totalHoursForSubTasks = subTasks.Sum(x => x.DurationInHours);
                foreach (var subTask in subTasks)
                {
                    var timeSheets = _projectTimesheetRepository.Query().Where(x => x.ProjectSubTaskId == subTask.Id).ToList();
                    foreach (var timeSheet in timeSheets)
                    {
                        taskProgress += (subTask.DurationInHours / totalHoursForSubTasks) * (timeSheet.PercentageOfCompletion / 100);
                    }
                }
            }

            var timeSheetsForTasks = _projectTimesheetRepository.Query().Where(x => x.ProjectTaskId == taskId && x.ProjectSubTaskId == null).ToList();

            if (timeSheetsForTasks.Count() > 0)
            {
                foreach (var timeSheet in timeSheetsForTasks)
                {
                    taskProgress += timeSheet.PercentageOfCompletion / 100;
                }
            }
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

    }
}
