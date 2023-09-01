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
        public ProjectManagementService(IProjectRepository projectRepository, IProjectTaskRepository projectTaskRepository, IProjectSubTaskRepository projectSubTaskRepository, 
            IUserRepository userRepository, IProjectTaskAsigneeRepository projectTaskAsigneeRepository, IProjectTimesheetRepository projectTimesheetRepository, IMapper mapper,
            IConfigurationProvider configuration, IHttpContextAccessor httpContext)
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
        }

        //Create a project

        public async Task<StandardResponse<bool>> CreateProject(ProjectModel model)
        {
            try
            {
                var project = _mapper.Map<Project>(model);

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

        public async Task<StandardResponse<bool>> CreateTask(ProjectTaskModel model)
        {
            try
            {
                if ((int)model.Category == 0 && model.Category.HasValue) return StandardResponse<bool>.Failed("Enter a valid category");

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
                    if(model.ProjectId.HasValue) assignee = new ProjectTaskAsignee { UserId = id, ProjectId = model.ProjectId, ProjectTaskId = task.Id };
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

        public async Task<StandardResponse<bool>> CreateSubTask(ProjectSubTaskModel model)
        {
            try
            {
                if ((int)model.TaskPriority == 0) return StandardResponse<bool>.Failed("Select a task priority");

                var task = _projectTaskRepository.Query().FirstOrDefault(x => x.Id == model.ProjectTaskId);

                if(task == null) return StandardResponse<bool>.NotFound("Task not found");

                var subTask = _mapper.Map<ProjectSubTask>(model);

                subTask.TaskPriority = model.TaskPriority.ToString();

                subTask.DurationInHours = model.DurationInHours.Value;

                if (!model.TrackedByHours && !model.DurationInHours.HasValue)
                {
                    subTask.DurationInHours = (model.EndDate - model.StartDate).TotalHours / 3;
                }

                if (subTask.DurationInHours.Value > task.DurationInHours) return StandardResponse<bool>.NotFound("Subtask duration cannot be greater than the task duration");

                subTask = _projectSubTaskRepository.CreateAndReturn(subTask);

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
                var assignee = _projectTaskAsigneeRepository.Query().FirstOrDefault(x => x.Id == model.UserId);

                if (assignee == null) return StandardResponse<bool>.NotFound("You are not assigned to this project");

                var project = _projectRepository.Query().FirstOrDefault(x => x.Id == model.ProjectId);

                if (project == null) return StandardResponse<bool>.NotFound("The project does not exist");

                var task = _projectTaskRepository.Query().FirstOrDefault(x => x.Id == model.TaskId);

                if (task == null) return StandardResponse<bool>.NotFound("Task not found");

                if(model.SubTaskId.HasValue)
                {
                    var subTask = _projectSubTaskRepository.Query().FirstOrDefault(x => x.Id == model.SubTaskId.Value);
                }

                var timesheet = _mapper.Map<ProjectTimesheet>(model);

                timesheet = _projectTimesheetRepository.CreateAndReturn(timesheet);

                assignee.HoursLogged += (model.StartDate - model.EndDate).TotalHours;

                _projectTaskAsigneeRepository.Update(assignee);

                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception e)
            {
                return StandardResponse<bool>.Error("Error Filling timesheet");
            }
        }
        public async Task<StandardResponse<PagedCollection<ProjectView>>> ListProject(PagingOptions pagingOptions, Guid superAdminId, ProjectStatus? status, string search = null)
        {
            try
            {
                var user = _userRepository.Query().FirstOrDefault(x => x.Id == superAdminId);

                if (user == null) return StandardResponse<PagedCollection<ProjectView>>.NotFound("User not found");

                var projects = _projectRepository.Query().Include(x => x.Assignees).ThenInclude(x => x.User).Where(x => x.SuperAdminId == superAdminId);

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

                //var projectProgressList = new List<ProjectListView>();

                //foreach(var project in mappedProjects)
                //{
                //    var record = new ProjectListView
                //    {
                //        NotStarted = projects.Where(x => x.StartDate > DateTime.Now).Count(),
                //        InProgress = projects.Where(x => DateTime.Now > x.StartDate && DateTime.Now < x.EndDate).Count(),
                //        Completed = projects.Where(x => DateTime.Now > x.EndDate).Count(),
                //        ProjectView = project,
                //        Progress = GetProjectPercentageOfCompletion(project.Id)
                //    };
                //    projectProgressList.Add(record);
                //}

                //var projectLists = new ProjectListView
                //{
                //    NotStarted = projects.Where(x => x.StartDate > DateTime.Now).Count(),
                //    InProgress = projects.Where(x => DateTime.Now > x.StartDate && DateTime.Now < x.EndDate).Count(),
                //    Completed = projects.Where(x => DateTime.Now > x.EndDate).Count(),
                //    projects = projectProgressList
                //};

                var pagedCollection = PagedCollection<ProjectView>.Create(Link.ToCollection(nameof(ProjectManagementController.ListProject)), mappedProjects.ToArray(), projects.Count(), pagingOptions);

                return StandardResponse<PagedCollection<ProjectView>>.Ok(pagedCollection);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<ProjectView>>.Error("Error listing Project");
            }
        }

        public async Task<StandardResponse<PagedCollection<ProjectTaskView>>> ListTasks(PagingOptions pagingOptions, Guid superAdminId, Guid projectId, ProjectStatus? status, string search = null)
        {
            try
            {
                var user = _userRepository.Query().FirstOrDefault(x => x.Id == superAdminId);

                if (user == null) return StandardResponse<PagedCollection<ProjectTaskView>>.NotFound("User not found");

                var tasks = _projectTaskRepository.Query().Include(x => x.SubTasks).Include(x => x.Assignees).ThenInclude(x => x.User).Where(x => x.SuperAdminId == superAdminId && x.ProjectId == projectId);

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
                }

                var pagedCollection = PagedCollection<ProjectTaskView>.Create(Link.ToCollection(nameof(ProjectManagementController.ListTasks)), mappedTasks.ToArray(), tasks.Count(), pagingOptions);

                return StandardResponse<PagedCollection<ProjectTaskView>>.Ok(pagedCollection);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<ProjectTaskView>>.Error("Error listing tasks");
            }
        }

        public async Task<StandardResponse<PagedCollection<ProjectSubTaskView>>> ListSubTasks(PagingOptions pagingOptions, Guid taskId, ProjectStatus? status, string search = null)
        {
            try
            {
                var task = _projectTaskRepository.Query().FirstOrDefault(x => x.Id == taskId);

                if (task == null) return StandardResponse<PagedCollection<ProjectSubTaskView>>.NotFound("User not found");

                var subtasks = _projectSubTaskRepository.Query().Include(x => x.ProjectTimesheets).Include(x => x.Assignee).Where(x => x.ProjectTaskId == taskId);

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

                var pagedTasks = subtasks.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedTasks = pagedTasks.ProjectTo<ProjectSubTaskView>(_configuration).ToList();


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
                var project = _projectRepository.Query().Include(x => x.Assignees).FirstOrDefault(x => x.Id == projectId);

                if (project == null) return StandardResponse<ProjectView>.NotFound("Project not found");

                var mappedProject = _mapper.Map<ProjectView>(project);
                mappedProject.Progress = GetProjectPercentageOfCompletion(projectId);

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
                var task = _projectTaskRepository.Query().Include(x => x.Assignees).FirstOrDefault(x => x.Id == taskId);

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
                var subTask = _projectSubTaskRepository.Query().Include(x => x.Assignee).FirstOrDefault(x => x.Id == subTaskId);

                if (subTask == null) return StandardResponse<ProjectSubTaskView>.NotFound("Subtask not found");

                var mappedSubTask = _mapper.Map<ProjectSubTaskView>(subTask);

                return StandardResponse<ProjectSubTaskView>.Ok(mappedSubTask);
            }
            catch (Exception e)
            {
                return StandardResponse<ProjectSubTaskView>.Error("Error getting subtask");
            }
        }

        public async Task<StandardResponse<ProjectProgressCountView>> GetStatusCountForProject(Guid superAdminId)
        {
            try
            {
                var projects = _projectRepository.Query().Include(x => x.Assignees).Where(x => x.SuperAdminId == superAdminId);

                var notStarted = projects.Where(x => x.StartDate > DateTime.Now).Count(); 

                var inProgress = projects.Where(x => x.StartDate > DateTime.Now).Count();

                var completed = projects.Where(x => x.IsCompleted == true).Count();

                var response = new ProjectProgressCountView { NotStarted = notStarted, InProgress = inProgress, Completed = completed };

                return StandardResponse<ProjectProgressCountView>.Ok(response);
            }
            catch (Exception e)
            {
                return StandardResponse<ProjectProgressCountView>.Error("Error getting response");
            }
        }

        public async Task<StandardResponse<PagedCollection<ProjectView>>> ListNotStartedProject(PagingOptions pagingOptions, Guid superAdminId, string search = null)
        {
            try
            {
                var user = _userRepository.Query().FirstOrDefault(x => x.Id == superAdminId);

                if (user == null) return StandardResponse<PagedCollection<ProjectView>>.NotFound("User not found");

                var projects = _projectRepository.Query().Where(x => x.SuperAdminId == superAdminId && x.StartDate > DateTime.Now);

                if (!string.IsNullOrEmpty(search))
                {
                    projects = projects.Where(x => x.Name.ToLower().Contains(search.ToLower()));
                }

                var pageProjects = projects.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedProjects = pageProjects.ProjectTo<ProjectView>(_configuration);
                foreach (var project in mappedProjects)
                {
                    var progress = GetProjectPercentageOfCompletion(project.Id);
                    project.Progress = progress;
                }

                var pagedCollection = PagedCollection<ProjectView>.Create(Link.ToCollection(nameof(TimeSheetController.GetTeamMemberRecentTimeSheet)), mappedProjects.ToArray(), projects.Count(), pagingOptions);

                return StandardResponse<PagedCollection<ProjectView>>.Ok(pagedCollection);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<ProjectView>>.Error("Error listing Project");
            }
        }

        public async Task<StandardResponse<PagedCollection<ProjectView>>> ListInProgressProject(PagingOptions pagingOptions, Guid superAdminId, string search = null)
        {
            try
            {
                var user = _userRepository.Query().FirstOrDefault(x => x.Id == superAdminId);

                if (user == null) return StandardResponse<PagedCollection<ProjectView>>.NotFound("User not found");

                var projects = _projectRepository.Query().Where(x => x.SuperAdminId == superAdminId && DateTime.Now > x.StartDate && DateTime.Now < x.EndDate);

                if (!string.IsNullOrEmpty(search))
                {
                    projects = projects.Where(x => x.Name.ToLower().Contains(search.ToLower()));
                }

                var pageProjects = projects.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedProjects = pageProjects.ProjectTo<ProjectView>(_configuration);

                foreach( var project in mappedProjects)
                {
                    var progress = GetProjectPercentageOfCompletion(project.Id);
                    project.Progress = progress;
                }

                var pagedCollection = PagedCollection<ProjectView>.Create(Link.ToCollection(nameof(TimeSheetController.GetTeamMemberRecentTimeSheet)), mappedProjects.ToArray(), projects.Count(), pagingOptions);

                return StandardResponse<PagedCollection<ProjectView>>.Ok(pagedCollection);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<ProjectView>>.Error("Error listing Project");
            }
        }
        public async Task<StandardResponse<PagedCollection<ProjectView>>> ListCompletedProject(PagingOptions pagingOptions, Guid superAdminId, string search = null)
        {
            try
            {
                var user = _userRepository.Query().FirstOrDefault(x => x.Id == superAdminId);

                if (user == null) return StandardResponse<PagedCollection<ProjectView>>.NotFound("User not found");

                var projects = _projectRepository.Query().Where(x => x.SuperAdminId == superAdminId && x.IsCompleted == true);

                if (!string.IsNullOrEmpty(search))
                {
                    projects = projects.Where(x => x.Name.ToLower().Contains(search.ToLower()));
                }

                var pageProjects = projects.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedProjects = pageProjects.ProjectTo<ProjectView>(_configuration);
                foreach (var project in mappedProjects)
                {
                    var progress = GetProjectPercentageOfCompletion(project.Id);
                    project.Progress = progress;
                }

                var pagedCollection = PagedCollection<ProjectView>.Create(Link.ToCollection(nameof(TimeSheetController.GetTeamMemberRecentTimeSheet)), mappedProjects.ToArray(), projects.Count(), pagingOptions);

                return StandardResponse<PagedCollection<ProjectView>>.Ok(pagedCollection);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<ProjectView>>.Error("Error listing Project");
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
                            var timeSheets = _projectTimesheetRepository.Query().Where(x => x.SubTaskId == subTask.Id).ToList();
                            foreach(var timeSheet in timeSheets)
                            {
                                subTaskProgress += (subTask.DurationInHours / totalHoursForSubTasks) * (timeSheet.PercentageOfCompletion / 100);
                            }
                        }

                        projectProgress += subTaskProgress * (task.DurationInHours / totalHoursForTask);
                    }

                    var timeSheetsForTasks = _projectTimesheetRepository.Query().Where(x => x.TaskId == task.Id && x.SubTaskId == null).ToList();

                    if(timeSheetsForTasks.Count() > 0)
                    {
                        foreach (var timeSheet in timeSheetsForTasks)
                        {
                            projectProgress += (task.DurationInHours / totalHoursForTask) * (timeSheet.PercentageOfCompletion / 100);
                        }
                    }
                    
                    //else
                    //{
                    //    var timeSheets = _projectTimesheetRepository.Query().Where(x => x.TaskId == task.Id).ToList();
                    //    foreach (var timeSheet in timeSheets)
                    //    {
                    //        projectProgress += (timeSheet.PercentageOfCompletion / 100) * (task.DurationInHours / totalHoursForTask);
                    //    }
                    //}
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
                    var timeSheets = _projectTimesheetRepository.Query().Where(x => x.SubTaskId == subTask.Id).ToList();
                    foreach (var timeSheet in timeSheets)
                    {
                        taskProgress += (subTask.DurationInHours / totalHoursForSubTasks) * (timeSheet.PercentageOfCompletion / 100);
                    }
                }
            }

            var timeSheetsForTasks = _projectTimesheetRepository.Query().Where(x => x.TaskId == taskId && x.SubTaskId == null).ToList();

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

            if(subTasks.Count > 0)
            {
                foreach (var subTask in subTasks)
                {
                    var timeSheets = _projectTimesheetRepository.Query().Where(x => x.SubTaskId == subTask.Id).ToList();
                    foreach (var timeSheet in timeSheets)
                    {
                        totalHours += (timeSheet.EndDate - timeSheet.StartDate).TotalHours;
                    }
                }
            }

            var timeSheetsForTasks = _projectTimesheetRepository.Query().Where(x => x.TaskId == task.Id && x.SubTaskId == null).ToList();
            foreach (var timeSheet in timeSheetsForTasks)
            {
                totalHours += (timeSheet.EndDate - timeSheet.StartDate).TotalHours;
            }

            return totalHours;
        }

    }
}