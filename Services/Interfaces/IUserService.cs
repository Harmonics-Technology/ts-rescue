using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Utilities;
using System;
using System.Threading.Tasks;
using TimesheetBE.Models.UtilityModels;
using System.Collections.Generic;

namespace TimesheetBE.Services.Abstractions
{
    public interface IUserService
    {
        Task<StandardResponse<UserView>> CreateUser(RegisterModel model);
        Task<StandardResponse<UserView>> Authenticate(LoginModel userToLogin);
        Task<StandardResponse<UserView>> VerifyUser(string token);
        Task<StandardResponse<UserView>> UpdatePassword(string newPassword);
        Task<StandardResponse<UserView>> InitiatePasswordReset(InitiateResetModel model, string redirectUrl = null);
        Task<StandardResponse<UserView>> CompletePasswordReset(PasswordReset payload);
        Task<StandardResponse<UserView>> UpdateUser(UpdateUserModel model);
        Task<StandardResponse<UserView>> AdminUpdateUser(UpdateUserModel model);
        Task<StandardResponse<UserProfileView>> UserProfile(Guid userId);
        Task<StandardResponse<UserView>> CreateAdminUser(RegisterModel newUser);
        Task<StandardResponse<UserView>> GetUserByToken();
        Task<StandardResponse<PagedCollection<UserView>>> ListUsers(string role, PagingOptions options, string search = null, DateFilter dateFilter = null);
        Task<StandardResponse<UserView>> InitiateNewUserPasswordReset(InitiateResetModel model);
        Task<StandardResponse<UserView>> GetById(Guid id);
        Task<StandardResponse<UserView>> ToggleUserIsActive(Guid id);
        Task<StandardResponse<UserView>> AddTeamMember(TeamMemberModel model);
        Task<StandardResponse<UserView>> ActivateTeamMember(Guid teamMemberId);
        Task<StandardResponse<UserView>> UpdateTeamMember(TeamMemberModel model);
        Task<StandardResponse<List<UserView>>> ListSupervisors(Guid clientId);
        Task<StandardResponse<PagedCollection<UserView>>> ListSupervisees(PagingOptions options, string search = null, Guid? supervisorId = null, DateFilter dateFilter = null);
        Task<StandardResponse<PagedCollection<UserView>>> ListClientSupervisors(PagingOptions options, string search = null, Guid? clientId = null, DateFilter dateFilter = null);
        Task<StandardResponse<PagedCollection<UserView>>> ListClientTeamMembers(PagingOptions options, string search = null, Guid? clientId = null, DateFilter dateFilter = null);
        Task<StandardResponse<PagedCollection<UserView>>> ListPaymentPartnerTeamMembers(PagingOptions options, string search = null, Guid? paymentPartnerId = null, DateFilter dateFilter = null);
    }
}

