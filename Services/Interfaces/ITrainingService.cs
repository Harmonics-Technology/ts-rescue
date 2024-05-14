using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Utilities;

namespace TimesheetBE.Services.Interfaces
{
    public interface ITrainingService
    {
        Task<StandardResponse<bool>> AddTraining(TrainingModel model);
        Task<StandardResponse<bool>> UpdateTraining(TrainingModel model);
        Task<StandardResponse<PagedCollection<TrainingView>>> ListTraining(PagingOptions pagingOptions, Guid superAdminId, Guid? trainingId = null, string search = null, DateFilter dateFilter = null);
        Task<StandardResponse<PagedCollection<TrainingAssigneeView>>> ListTrainingStatus(PagingOptions pagingOptions, Guid trainingId);
        Task<StandardResponse<bool>> DeleteTrainingFile(Guid fileId);
        Task<StandardResponse<bool>> AddNewFile(NewTrainingFileModel model);
        Task<StandardResponse<bool>> DeleteAssignedUser(Guid userId, Guid trainingId);
        Task<StandardResponse<bool>> AssignNewUser(Guid userId, Guid trainingId);
        Task<StandardResponse<bool>> StartTraining(Guid userId, Guid trainingId, Guid fileId);
        Task<StandardResponse<bool>> CompleteTraining(Guid userId, Guid trainingId, Guid fileId);
        Task<StandardResponse<PagedCollection<TrainingMaterialView>>> ListUserTrainings(PagingOptions pagingOptions, Guid userId);
        Task<StandardResponse<List<TrainingAssigneeView>>> ListUserTrainingMaterials(Guid userId, Guid trainingId);
    }
}
