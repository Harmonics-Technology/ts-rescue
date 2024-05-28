using AutoMapper;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Repositories;
using TimesheetBE.Repositories.Interfaces;
using TimesheetBE.Services.Interfaces;
using TimesheetBE.Utilities.Extentions;
using TimesheetBE.Utilities;
using Microsoft.EntityFrameworkCore;
using TimesheetBE.Controllers;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Models;
using Stripe;
using AutoMapper.QueryableExtensions;

namespace TimesheetBE.Services
{
    public class TrainingService : ITrainingService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITrainingRepository _trainingRepository;
        private readonly ITrainingAssigneeRepository _trainingAssigneeRepository;
        private readonly ITrainingFileRepository _trainingFileRepository;
        private readonly IMapper _mapper;
        private readonly IConfigurationProvider _configuration;
        private readonly IHttpContextAccessor _httpContext;
        private readonly INotificationService _notificationService;
        private readonly ITrainingVideoProgressLogRepository _trainingVideoProgressLogRepository;
        public TrainingService(IUserRepository userRepository, ITrainingRepository trainingRepository, ITrainingAssigneeRepository trainingAssigneeRepository, 
            ITrainingFileRepository trainingFileRepository, IMapper mapper, IConfigurationProvider configuration, IHttpContextAccessor httpContext, 
            INotificationService notificationService, ITrainingVideoProgressLogRepository trainingVideoProgressLogRepository)
        {
            _userRepository = userRepository;
            _trainingRepository = trainingRepository;
            _trainingAssigneeRepository = trainingAssigneeRepository;
            _trainingFileRepository = trainingFileRepository;
            _mapper = mapper;
            _configuration = configuration;
            _httpContext = httpContext;
            _notificationService = notificationService;
            _trainingVideoProgressLogRepository = trainingVideoProgressLogRepository;
        }

        public async Task<StandardResponse<bool>> AddTraining(TrainingModel model)
        {
            try
            {
                var superAdmin = _userRepository.Query().FirstOrDefault(x => x.Id == model.SuperAdminId);

                if (superAdmin == null) return StandardResponse<bool>.NotFound("user not found");

                var training = _mapper.Map<Training>(model);

                training = _trainingRepository.CreateAndReturn(training);

                if (model.TrainingFiles == null) return StandardResponse<bool>.Failed("Kindly add training files.");

                model.TrainingFiles.ForEach(x =>
                {
                    var file = new TrainingFile { TrainingId = training.Id, Title = x.Title, Category = x.Category, FileUrl = x.FileUrl };
                    _trainingFileRepository.CreateAndReturn(file);
                });

                if (!training.IsAllParticipant)
                {
                    if(model.AssignedUsers == null) return StandardResponse<bool>.Failed("Kindly assign participant for this training");

                    var trainingFiles = _trainingFileRepository.Query().Where(x => x.TrainingId == training.Id).ToList();

                    model.AssignedUsers.ForEach(id =>
                    {
                        var trainingAssignee = new TrainingAssignee { UserId = id, TrainingId = training.Id, TrainingFileId = null, DateCompleted = null };
                        _trainingAssigneeRepository.CreateAndReturn(trainingAssignee);

                        trainingFiles.ForEach(file =>
                        {
                            var assignee = new TrainingAssignee { UserId = id, TrainingId = training.Id, TrainingFileId = file.Id, DateCompleted = null };
                            _trainingAssigneeRepository.CreateAndReturn(assignee);
                        });
                    });
                }
                else
                {
                    var teamMembers = _userRepository.Query().Where(x => x.SuperAdminId == training.SuperAdminId && x.Role.ToLower() == "team member").ToList();

                    var trainingFiles = _trainingFileRepository.Query().Where(x => x.TrainingId == training.Id).ToList();

                    foreach (var teamMember in teamMembers)
                    {
                        var trainingAssignee = new TrainingAssignee { UserId = teamMember.Id, TrainingId = training.Id, TrainingFileId = null, DateCompleted = null };
                        _trainingAssigneeRepository.CreateAndReturn(trainingAssignee);

                        trainingFiles.ForEach(file =>
                        {
                            var assignee = new TrainingAssignee { UserId = teamMember.Id, TrainingId = training.Id, TrainingFileId = file.Id, DateCompleted = null };
                            _trainingAssigneeRepository.CreateAndReturn(assignee);
                        });
                    }
                }

                

                await _notificationService.SendNotification(new NotificationModel { UserId = training.SuperAdminId, Title = "New Training Added", Type = "Notification", Message = $"{training.Name} has been successfully created" });
                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception e)
            {
                return StandardResponse<bool>.Error("Error creating Training");
            }
        }

        public async Task<StandardResponse<bool>> UpdateTraining(TrainingModel model)
        {
            try
            {
                var training = _trainingRepository.Query().Include(x => x.Assignees).Include(x => x.Files).FirstOrDefault(x => x.Id == model.Id);

                if (training == null) return StandardResponse<bool>.Failed("Training not found");

                training.Name = model.Name;
                training.Note = model.Note;

                foreach(var user in training.Assignees)
                {
                    if (model.AssignedUsers.Any(id => user.Id == id)) continue;
                    if (model.AssignedUsers.Any(id => user.Id != id))
                    {
                        _trainingAssigneeRepository.Delete(user);
                    }
                }

                foreach(var user in model.AssignedUsers)
                {
                    if (training.Assignees.Any(x => x.Id == user)) continue;

                    var assignee = new TrainingAssignee { UserId = user, TrainingId = training.Id };

                    _trainingAssigneeRepository.CreateAndReturn(assignee);

                }
                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception e)
            {
                return StandardResponse<bool>.Error("Error updating training");
            }
        }

        public async Task<StandardResponse<PagedCollection<TrainingView>>> ListTraining(PagingOptions pagingOptions, Guid superAdminId, Guid? trainingId = null, string search = null, DateFilter dateFilter = null)
        {
            try
            {
                var superAdmin = _userRepository.Query().FirstOrDefault(x => x.Id == superAdminId);

                if (superAdmin == null) return StandardResponse<PagedCollection<TrainingView>>.NotFound("User not found");

                var trainings = _trainingRepository.Query().Include(x => x.Assignees.Where(x => x.TrainingFile == null)).Where(x => x.SuperAdminId == superAdminId).OrderByDescending(x => x.DateCreated);

                if (dateFilter.StartDate.HasValue)
                    trainings = trainings.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

                if (dateFilter.EndDate.HasValue)
                    trainings = trainings.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);

                if (!string.IsNullOrEmpty(search))
                {
                    trainings = trainings.Where(x => x.Name.ToLower().Contains(search.ToLower())).OrderByDescending(u => u.DateCreated);
                }

                if(trainingId != null)
                {
                    trainings = trainings.Where(u => u.Id == trainingId).OrderByDescending(u => u.DateCreated);
                }

                var pagedTraining = trainings.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedTrainings = pagedTraining.ProjectTo<TrainingView>(_configuration).ToList();

                foreach (var training in mappedTrainings)
                {
                    var progress = GetTrainingPercentageOfCompletion(training.Id);
                    training.Progress = progress;
                    training.Assignees = training.Assignees.Where(x => x.TrainingFileId == null).ToList();
                }

                var pagedCollection = PagedCollection<TrainingView>.Create(Link.ToCollection(nameof(TrainingController.ListTraining)), mappedTrainings.ToArray(), trainings.Count(), pagingOptions);

                return StandardResponse<PagedCollection<TrainingView>>.Ok(pagedCollection);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<TrainingView>>.Error("Error listing Project");
            }
        }

        public async Task<StandardResponse<PagedCollection<TrainingAssigneeView>>> ListTrainingStatus(PagingOptions pagingOptions, Guid trainingId)
        {
            try
            {
                var training = _trainingRepository.Query().FirstOrDefault(x => x.Id == trainingId);

                if (training == null) return StandardResponse<PagedCollection<TrainingAssigneeView>>.NotFound("Training not found");

                var usersTrainingStaus = _trainingAssigneeRepository.Query().Include(x => x.User).Where(x => x.TrainingId == trainingId && x.TrainingFileId == null).OrderByDescending(x => x.DateCreated);

                var pagedTrainingStatus = usersTrainingStaus.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedUsersTrainingStaus = pagedTrainingStatus.ProjectTo<TrainingAssigneeView>(_configuration);

                var pagedCollection = PagedCollection<TrainingAssigneeView>.Create(Link.ToCollection(nameof(TrainingController.ListTrainingStatus)), mappedUsersTrainingStaus.ToArray(), usersTrainingStaus.Count(), pagingOptions);

                return StandardResponse<PagedCollection<TrainingAssigneeView>>.Ok(pagedCollection);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<TrainingAssigneeView>>.Error("Error listing training status");
            }
        }

        public async Task<StandardResponse<bool>> DeleteTraining(Guid trainingId)
        {
            try
            {
                var training = _trainingRepository.Query().FirstOrDefault(x => x.Id == trainingId);

                if (training == null) return StandardResponse<bool>.NotFound("Training not found");

                var trainingFiles = _trainingFileRepository.Query().Where(x => x.TrainingId == trainingId).ToList();

                var trainingAssignees = _trainingAssigneeRepository.Query().Where(x => x.TrainingId == trainingId).ToList();

                foreach( var trainingAssignee in trainingAssignees)
                {
                    _trainingAssigneeRepository.Delete(trainingAssignee);
                }

                foreach( var trainingFile in trainingFiles)
                {
                    _trainingFileRepository.Delete(trainingFile);
                }

                _trainingRepository.Delete(training);

                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception e)
            {
                return StandardResponse<bool>.Error("Error deleting training");
            }
        }

        public async Task<StandardResponse<bool>> DeleteTrainingFile(Guid fileId)
        {
            try
            {
                var file = _trainingFileRepository.Query().FirstOrDefault(x => x.Id == fileId);

                if (file == null) return StandardResponse<bool>.NotFound("File not found");

                var assignedUsersWithFile = _trainingAssigneeRepository.Query().Where(x => x.TrainingFileId == fileId).ToList();

                foreach(var assignedUser in assignedUsersWithFile)
                {
                    _trainingAssigneeRepository.Delete(assignedUser);
                }

                _trainingFileRepository.Delete(file);
                return StandardResponse<bool>.Ok(true);
            }
            catch(Exception e)
            {
                return StandardResponse<bool>.Error("Error deleting file");
            }
        }

        public async Task<StandardResponse<bool>> AddNewFile(NewTrainingFileModel model)
        {
            try
            {
                var training = _trainingRepository.Query().FirstOrDefault(x => x.Id == model.TrainingId);

                if (training == null) return StandardResponse<bool>.NotFound("Training not found");

                var file = new TrainingFile { TrainingId = training.Id, Category = model.Category, FileUrl = model.FileURL, Title = model.Title };

                _trainingFileRepository.CreateAndReturn(file);

                var assignedUsers = _trainingAssigneeRepository.Query().Where(x => x.TrainingId == training.Id && x.TrainingFileId == null).ToList();

                foreach(var  user in assignedUsers)
                {
                    var assignee = new TrainingAssignee { UserId = user.UserId, TrainingId = training.Id, TrainingFileId = file.Id };
                    _trainingAssigneeRepository.CreateAndReturn(assignee);
                }
                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception e)
            {
                return StandardResponse<bool>.Error("Error adding new file");
            }
        }

        public async Task<StandardResponse<bool>> DeleteAssignedUser(Guid userId, Guid trainingId)
        {
            try
            {
                var assignee = _trainingAssigneeRepository.Query().FirstOrDefault(x => x.UserId == userId && x.TrainingId == trainingId && x.TrainingFileId == null);

                if (assignee == null) return StandardResponse<bool>.NotFound("Assignee not found");

                var assigneeFileRef = _trainingAssigneeRepository.Query().Where(x => x.UserId == userId && x.TrainingId == trainingId && x.TrainingFileId != null).ToList();

                foreach(var assigneeRef in assigneeFileRef)
                {
                    _trainingAssigneeRepository.Delete(assigneeRef);
                }

                _trainingAssigneeRepository.Delete(assignee);

                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception e)
            {
                return StandardResponse<bool>.Error("Error deleting assignee");
            }
        }

        public async Task<StandardResponse<bool>> AssignNewUser(Guid userId, Guid trainingId)
        {
            try
            {
                var user = _userRepository.Query().FirstOrDefault(x => x.Id == userId);

                if (user == null) return StandardResponse<bool>.NotFound("User not found");

                var training = _trainingRepository.Query().FirstOrDefault(x => x.Id == trainingId);

                if (training == null) return StandardResponse<bool>.NotFound("Training not found");

                var assignee = new TrainingAssignee { UserId = userId, TrainingId = training.Id, TrainingFileId = null };

                _trainingAssigneeRepository.CreateAndReturn(assignee);

                var trainingFiles = _trainingFileRepository.Query().Where(x => x.TrainingId == training.Id).ToList();

                trainingFiles.ForEach(file =>
                {
                    var assigneeFileRef = new TrainingAssignee { UserId = userId, TrainingId = training.Id, TrainingFileId = file.Id };
                    _trainingAssigneeRepository.CreateAndReturn(assigneeFileRef);   
                });

                return StandardResponse<bool>.Ok(true);
            }
            catch(Exception ex)
            {
                return StandardResponse<bool>.Error("Error adding assignee");
            }
        }

        public async Task<StandardResponse<bool>> StartTraining(Guid userId, Guid trainingId, Guid fileId)
        {
            try
            {
                var assigneeTraining = _trainingAssigneeRepository.Query().FirstOrDefault(x => x.TrainingId == trainingId && x.UserId == userId && x.TrainingFileId == null);

                if (assigneeTraining == null) return StandardResponse<bool>.NotFound("Training not found");

                var assigneeTrainingFile = _trainingAssigneeRepository.Query().FirstOrDefault(x => x.TrainingId == trainingId && x.UserId == userId && x.TrainingFileId == fileId);

                if (assigneeTrainingFile == null) return StandardResponse<bool>.NotFound("Training file not found for user");

                if(assigneeTraining.IsStarted == false)
                {
                    assigneeTraining.IsStarted = true;

                    _trainingAssigneeRepository.Update(assigneeTraining);
                }

                assigneeTrainingFile.IsStarted = true;

                _trainingAssigneeRepository.Update(assigneeTrainingFile);

                return StandardResponse<bool>.Ok(true);
            }
            catch(Exception ex)
            {
                return StandardResponse<bool>.Error("Error starting training");
            }
        }

        public async Task<StandardResponse<bool>> CompleteTraining(Guid userId, Guid trainingId, Guid fileId)
        {
            try
            {
                var assigneeTrainingFile = _trainingAssigneeRepository.Query().FirstOrDefault(x => x.TrainingId == trainingId && x.UserId == userId && x.TrainingFileId == fileId);

                if (assigneeTrainingFile == null) return StandardResponse<bool>.NotFound("Training file not found for user");

                assigneeTrainingFile.IsCompleted = true;

                assigneeTrainingFile.DateCompleted = DateTime.Now;

                _trainingAssigneeRepository.Update(assigneeTrainingFile);

                var isAllCompleted = _trainingAssigneeRepository.Query().Any(x => x.TrainingId == trainingId && x.UserId == userId && x.TrainingFileId != null && x.IsCompleted == false);

                if (!isAllCompleted)
                {
                    var assigneeTraining = _trainingAssigneeRepository.Query().FirstOrDefault(x => x.TrainingId == trainingId && x.UserId == userId && x.TrainingFileId == null);

                    assigneeTraining.IsCompleted = true;

                    assigneeTraining.DateCompleted = DateTime.Now;

                    _trainingAssigneeRepository.Update(assigneeTraining);
                }

                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                return StandardResponse<bool>.Error("Error completing training");
            }
        }

        public async Task<StandardResponse<PagedCollection<TrainingMaterialView>>> ListUserTrainings(PagingOptions pagingOptions, Guid userId)
        {
            try
            {
                var user = _userRepository.Query().FirstOrDefault(x => x.Id == userId);

                if (user == null) return StandardResponse<PagedCollection<TrainingMaterialView>>.NotFound("user not found");

                var assignedTrainings = _trainingAssigneeRepository.Query().Include(x => x.Training).Where(x => x.UserId == userId && x.TrainingFileId == null).OrderByDescending(x => x.DateCreated);

                var mappedAssignedTrainings = assignedTrainings.ProjectTo<TrainingAssigneeView>(_configuration).ToList();

                var userAssignedTrainings = new List<TrainingMaterialView>();

                foreach (var training in mappedAssignedTrainings)
                {
                    var userAssignedTraining = new TrainingMaterialView
                    {
                        TrainingId = training.TrainingId,
                        Name = training.Training.Name,
                        NoOfTrainingFile = _trainingAssigneeRepository.Query().Count(x => x.UserId == training.UserId && x.TrainingId == training.TrainingId && x.TrainingFileId != null),
                        Progress = GetUserTrainingProgress(training.TrainingId, training.UserId),
                        DateCompleted = training.DateCompleted,
                        Status = training.Status
                    };

                    userAssignedTrainings.Add(userAssignedTraining);
                }

                var pagedCollection = PagedCollection<TrainingMaterialView>.Create(Link.ToCollection(nameof(TrainingController.ListUserTrainings)), userAssignedTrainings.ToArray(), assignedTrainings.Count(), pagingOptions);

                return StandardResponse<PagedCollection<TrainingMaterialView>>.Ok(pagedCollection);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<TrainingMaterialView>>.Error("Error listing user training");
            }
        }

        public async Task<StandardResponse<List<TrainingAssigneeView>>> ListUserTrainingMaterials(Guid userId, Guid trainingId)
        {
            try
            {
                var user = _userRepository.Query().FirstOrDefault(x => x.Id == userId);

                if (user == null) return StandardResponse<List<TrainingAssigneeView>>.NotFound("user not found");

                var assignedTrainings = _trainingAssigneeRepository.Query().Include(x => x.TrainingFile).Where(x => x.UserId == userId && x.TrainingId == trainingId && x.TrainingFileId != null).OrderByDescending(x => x.DateCreated);

                var mappedAssignedTrainings = _mapper.Map<List<TrainingAssigneeView>>(assignedTrainings);

                return StandardResponse<List<TrainingAssigneeView>>.Ok(mappedAssignedTrainings);
            }
            catch (Exception e)
            {
                return StandardResponse<List<TrainingAssigneeView>>.Error("Error listing training materials");
            }
        }

        public async Task<StandardResponse<bool>> CreateOrUpdateVideoRecordProgress(TrainingVideoProgressLogModel model)
        {
            try
            {
                var assigneeTraining = _trainingAssigneeRepository.Query().FirstOrDefault(x => x.TrainingId == model.TrainingId && x.UserId == model.UserId && x.TrainingFileId == model.TrainingFileId);

                if (assigneeTraining == null) return StandardResponse<bool>.NotFound("Training not found");

                assigneeTraining.LastRecordedProgress = model.LastRecordedProgress;

                _trainingAssigneeRepository.Update(assigneeTraining);

                //var trainingRecord = _trainingVideoProgressLogRepository.Query().FirstOrDefault(x => x.TrainingId == model.TrainingId && x.UserId == model.UserId && x.TrainingFileId == model.TrainingFileId);

                //if(trainingRecord == null)
                //{
                //    trainingRecord = _mapper.Map<TrainingVideoProgressLog>(model);

                //    _trainingVideoProgressLogRepository.CreateAndReturn(trainingRecord);
                //}
                //else
                //{
                //    trainingRecord.LastRecordedProgress = model.LastRecordedProgress;

                //    _trainingVideoProgressLogRepository.Update(trainingRecord);
                //}                

                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                return StandardResponse<bool>.Error("Error creating training progress record");
            }
        }

        public async Task<StandardResponse<string>> GetUserVideoLastRecordedProgress(Guid userId, Guid trainingId, Guid fileId)
        {
            try
            {
                var trainingProgress = _trainingVideoProgressLogRepository.Query().FirstOrDefault(x => x.TrainingId == trainingId && x.UserId == userId && x.TrainingFileId == fileId);

                if (trainingProgress == null) return StandardResponse<string>.NotFound("No recorded progress found");

                return StandardResponse<string>.Ok(trainingProgress.LastRecordedProgress);
            }
            catch (Exception ex)
            {
                return StandardResponse<string>.Error("Error fetching training record");
            }
        }

        private double? GetTrainingPercentageOfCompletion(Guid trainingId)
        {
            var assignedUsers = _trainingAssigneeRepository.Query().Where(x => x.TrainingId == trainingId && x.TrainingFileId == null).ToList();

            double? percentageOfCompletion = 0;

            double? usersWithCompletedTrainingCount = 0;

            foreach(var user in assignedUsers)
            {
                if (_trainingAssigneeRepository.Query().Any(x => x.UserId == user.UserId && x.TrainingId == trainingId && x.TrainingFileId != null && x.IsCompleted == false)) continue;

                usersWithCompletedTrainingCount++;
            }

            percentageOfCompletion = (float)usersWithCompletedTrainingCount / (float)assignedUsers.Count();

            return percentageOfCompletion * 100;
        }

        private double GetUserTrainingProgress(Guid trainingId, Guid userId)
        {
            double percentageOfCompletion = 0;

            var userTrainingFilesCount = _trainingAssigneeRepository.Query().Count(x => x.UserId == userId && x.TrainingId == trainingId && x.TrainingFileId != null);

            var userCompletedTrainingFilesCount = _trainingAssigneeRepository.Query().Count(x => x.UserId == userId && x.TrainingId == trainingId && x.TrainingFileId != null && x.IsCompleted == true);

            percentageOfCompletion = (float)userCompletedTrainingFilesCount /(float)userTrainingFilesCount;

            return percentageOfCompletion * 100;
        }
    }
}
