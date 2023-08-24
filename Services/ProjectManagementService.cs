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

namespace TimesheetBE.Services
{
    public class ProjectManagementService
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

                model.Assignees.ForEach(id =>
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

                task = _projectTaskRepository.CreateAndReturn(task);

                model.Assignees.ForEach(id =>
                {
                    var assignee = new ProjectTaskAsignee();
                    if(model.ProjectId.HasValue) assignee = new ProjectTaskAsignee { UserId = id, ProjectId = model.ProjectId, TaskId = task.Id };
                    if (!model.ProjectId.HasValue) assignee = new ProjectTaskAsignee { UserId = id, TaskId = task.Id };
                    _projectTaskAsigneeRepository.CreateAndReturn(assignee);
                });
                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception e)
            {
                return StandardResponse<bool>.Error("Error creating Project");
            }
        }

        public async Task<StandardResponse<bool>> CreateSubTask(ProjectSubTaskModel model)
        {
            try
            {
                if ((int)model.TaskPriority == 0) return StandardResponse<bool>.Failed("Select a task priority");

                var task = _projectTaskRepository.Query().FirstOrDefault(x => x.Id == model.TaskId);

                if(task == null) return StandardResponse<bool>.NotFound("Task not found");

                var subTask = _mapper.Map<ProjectSubTask>(model);

                subTask.TaskPriority = model.TaskPriority.ToString();

                subTask = _projectSubTaskRepository.CreateAndReturn(subTask);

                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception e)
            {
                return StandardResponse<bool>.Error("Error creating Project");
            }
        }

        public async Task<StandardResponse<ProjectListView>> ListProject(PagingOptions pagingOptions, Guid superAdminId, string search)
        {
            try
            {
                var user = _userRepository.Query().FirstOrDefault(x => x.Id == superAdminId);

                if (user == null) return StandardResponse<ProjectListView>.NotFound("User not found");

                var projects = _projectRepository.Query().Where(x => x.SuperAdminId == superAdminId);

                if(!string.IsNullOrEmpty(search))
                {
                    projects = projects.Where(x => x.Name.ToLower().Contains(search.ToLower()));
                }

                var pageProjects = projects.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedProjects = pageProjects.ProjectTo<ProjectView>(_configuration).ToList();

                var projectLists = new ProjectListView
                {
                    NotStarted = projects.Where(x => x.StartDate > DateTime.Now).Count(),
                    InProgress = projects.Where(x => DateTime.Now > x.StartDate && DateTime.Now < x.EndDate).Count(),
                    Completed = projects.Where(x => DateTime.Now > x.EndDate).Count(),
                    projects = mappedProjects
                };

                return StandardResponse<ProjectListView>.Ok(projectLists);
            }
            catch (Exception e)
            {
                return StandardResponse<ProjectListView>.Error("Error listing Project");
            }
        }

        //public async Task<StandardResponse<ProjectListView>> ListProjectTask(PagingOptions pagingOptions, Guid superAdminId, string search)
        //{
        //    try
        //    {
        //        var user = _userRepository.Query().FirstOrDefault(x => x.Id == superAdminId);

        //        if (user == null) return StandardResponse<ProjectListView>.NotFound("User not found");

        //        var tasks = _projectTaskRepository.Query().Where(x => x.SuperAdminId == superAdminId);

        //        if (!string.IsNullOrEmpty(search))
        //        {
        //            tasks = tasks.Where(x => x.Name.ToLower().Contains(search.ToLower()));
        //        }

        //        var pageTasks = tasks.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

        //        var mappedTasks = pageTasks.ProjectTo<ProjectTaskView>(_configuration).ToList();

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

    }
}
