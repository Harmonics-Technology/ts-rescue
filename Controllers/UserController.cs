using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using Stripe;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Models.ViewModels.CommandCenterViewModels;
using TimesheetBE.Services.Abstractions;
using TimesheetBE.Services.ConnectedServices.Stripe.Resource;
using TimesheetBE.Services.Interfaces;
using TimesheetBE.Utilities;

namespace TimesheetBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : StandardControllerResponse
    {
        private readonly IUserService _userService;
        private readonly IUtilityService _utilityService;
        private readonly PagingOptions _defaultPagingOptions;

        public UserController(IUserService userService, IOptions<PagingOptions> defaultPagingOptions, IUtilityService utilityService)
        {
            _userService = userService;
            _defaultPagingOptions = defaultPagingOptions.Value;
            _utilityService = utilityService;
        }


        [HttpPost("register", Name = nameof(Create))]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<UserView>>> Create(RegisterModel newUser)
        {
            return Result(await _userService.CreateUser(newUser));
        }

        [HttpPost("login/", Name = nameof(LoginUser))]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<UserView>>> LoginUser(LoginModel model)
        {
            return Ok(await _userService.Authenticate(model));
        }

        [HttpGet("verifyUser/{token}", Name = nameof(Verify))]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<UserView>>> Verify(string token)
        {
            return Ok(await _userService.VerifyUser(token));
        }

        [HttpPost("reset/initiate", Name = nameof(InitiateReset))]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<UserView>>> InitiateReset(InitiateResetModel model, string redirectUrl = null)
        {
            return Ok(await _userService.InitiatePasswordReset(model, redirectUrl));
        }

        [HttpPost("reset/complete", Name = nameof(CompleteReset))]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<UserView>>> CompleteReset(PasswordReset payload)
        {
            return Ok(_userService.CompletePasswordReset(payload));
        }

        [HttpGet("control-settings", Name = nameof(GetControlSettingById))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<ControlSettingView>>> GetControlSettingById([FromQuery] Guid superAdminId)
        {
            return Result(await _userService.GetControlSettingById(superAdminId));
        }

        [HttpPost("update-control-settings", Name = nameof(UpdateControlSettings))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<bool>>> UpdateControlSettings(ControlSettingModel model)
        {
            return Ok(await _userService.UpdateControlSettings(model));
        }

        [HttpGet("project-management-settings", Name = nameof(GetSuperAdminProjectManagementSettings))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<ProjectManagementSettingView>>> GetSuperAdminProjectManagementSettings([FromQuery] Guid superAdminId)
        {
            return Result(await _userService.GetSuperAdminProjectManagementSettings(superAdminId));
        }

        [HttpPost("update-project-management-settings", Name = nameof(UpdateProjectManagementSettings))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<bool>>> UpdateProjectManagementSettings(ProjectManagementSettingModel model)
        {
            return Ok(await _userService.UpdateProjectManagementSettings(model));
        }

        [HttpPost("update", Name = nameof(UpdateUser))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<UserView>>> UpdateUser(UpdateUserModel model)
        {
            return Ok(await _userService.UpdateUser(model));
        }

        [HttpPost("update/client-subscription", Name = nameof(UpdateClientSubscription))]
        //[Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<UserView>>> UpdateClientSubscription(UpdateClientSubscriptionModel model)
        {
            return Ok(await _userService.UpdateClientSubscription(model));
        }

        // handle microsoft login here 
        [HttpPost("microsoft-login", Name = nameof(MicrosoftLogin))]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<UserView>>> MicrosoftLogin(MicrosoftIdTokenDetailsModel model)
        {
            return Result(await _userService.MicrosoftLogin(model));
        }

        [HttpGet("change_password", Name = nameof(UpdatePassword))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<UserView>>> UpdatePassword(string password)
        {
            return Ok(_userService.UpdatePassword(password));
        }

        [HttpGet("user-profile/{userId}", Name = nameof(UserProfile))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<UserProfileView>>> UserProfile(Guid userId)
        {
            return Ok(_userService.UserProfile(userId));
        }

        [HttpPost("validate-token", Name = nameof(ValidateToken))]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<StandardResponse<UserView>>> ValidateToken()
        {
            return Result(await _userService.GetUserByToken());
        }

        [HttpGet("list/{role}", Name = nameof(ListUsers))]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<StandardResponse<PagedCollection<UserView>>>> ListUsers([FromQuery] Guid superAdminId, 
            [FromQuery] PagingOptions options, [FromQuery] string role = null, [FromQuery] string Search = null, [FromQuery] DateFilter dateFilter = null, [FromQuery] Guid? subscriptionId = null,
            [FromQuery] bool? productManagers = null)
        {
            options.Replace(_defaultPagingOptions);
            return Result(await _userService.ListUsers(superAdminId, options, role, Search, dateFilter, subscriptionId, productManagers));
        }

        [HttpPost("invite/resend", Name = nameof(ResendInvite))]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<UserView>>> ResendInvite(InitiateResetModel model)
        {
            return Ok(await _userService.SendNewUserPasswordReset(model));
        }

        [HttpGet("get/{id}", Name = nameof(GetUserById))]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<StandardResponse<UserView>>> GetUserById(Guid id)
        {
            return Ok(await _userService.GetById(id));
        }

        [HttpGet("toggle-active/{id}", Name = nameof(ToggleUserActive))]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<StandardResponse<UserView>>> ToggleUserActive(Guid id)
        {
            return Ok(await _userService.ToggleUserIsActive(id));
        }

        [HttpPost("admin-update-user", Name = nameof(AdminUpdateUser))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<UserView>>> AdminUpdateUser(UpdateUserModel model)
        {
            return Ok(await _userService.AdminUpdateUser(model));
        }

        [HttpPost("add-team-member", Name = nameof(AddTeamMember))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<UserView>>> AddTeamMember(TeamMemberModel model)
        {
            return Ok(await _userService.AddTeamMember(model));
        }

        [HttpGet("activate-team-member/{id}", Name = nameof(ActivateTeamMember))]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<UserView>>> ActivateTeamMember(Guid id)
        {
            return Ok(await _userService.ActivateTeamMember(id));
        }

        [HttpPost("update-team-member", Name = nameof(UpdateTeamMember))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<UserView>>> UpdateTeamMember(TeamMemberModel model)
        {
            return Ok(await _userService.UpdateTeamMember(model));
        }

        /// <summary>
        /// Get all supervisor for a client 
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        [HttpGet("supervisors/{clientId}", Name = nameof(GetSupervisors))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<List<UserView>>>> GetSupervisors(Guid clientId)
        {
            return Ok(await _userService.ListSupervisors(clientId));
        }

        [HttpGet("shift-users", Name = nameof(ListShiftUsers))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<PagedCollection<ShiftUsersListView>>>> ListShiftUsers([FromQuery] PagingOptions options, [FromQuery] Guid superAdminId, [FromQuery] DateTime startDate, DateTime endDate)
        {
            options.Replace(_defaultPagingOptions);
            return Ok(await _userService.ListShiftUsers(options, superAdminId, startDate, endDate));
        }

        [HttpGet("supervisees", Name = nameof(GetSupervisees))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<PagedCollection<UserView>>>> GetSupervisees([FromQuery] PagingOptions options, [FromQuery] string search = null, [FromQuery] Guid? supervisorId = null, [FromQuery] DateFilter dateFilter = null)
        {
            options.Replace(_defaultPagingOptions);
            return Ok(await _userService.ListSupervisees(options, search, supervisorId, dateFilter));
        }

        [HttpGet("client/supervisors", Name = nameof(GetClientSupervisors))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<PagedCollection<UserView>>>> GetClientSupervisors([FromQuery] PagingOptions options, [FromQuery] string search = null, [FromQuery] Guid? clientId = null, [FromQuery] DateFilter dateFilter = null)
        {
            options.Replace(_defaultPagingOptions);
            return Ok(await _userService.ListClientSupervisors(options, search, clientId, dateFilter));
        }

        [HttpGet("client/team-members", Name = nameof(GetClientTeamMembers))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<PagedCollection<UserView>>>> GetClientTeamMembers([FromQuery] PagingOptions options, [FromQuery] string search = null, [FromQuery] Guid? clientId = null, [FromQuery] DateFilter dateFilter = null)
        {
            options.Replace(_defaultPagingOptions);
            return Ok(await _userService.ListClientTeamMembers(options, search, clientId, dateFilter));
        }

        [HttpGet("payment-partner/team-members", Name = nameof(GetPaymentPartnerTeamMembers))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<PagedCollection<UserView>>>> GetPaymentPartnerTeamMembers([FromQuery] PagingOptions options, [FromQuery] string search = null, [FromQuery] Guid? paymentPartnerId = null, [FromQuery] DateFilter dateFilter = null)
        {
            options.Replace(_defaultPagingOptions);
            return Ok(await _userService.ListPaymentPartnerTeamMembers(options, search, paymentPartnerId, dateFilter));
        }

        [HttpPost("enable2fa", Name = nameof(Enable2FA))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<Enable2FAView>>> Enable2FA([FromQuery] bool is2FAEnabled)
        {
            return Result(_userService.EnableTwoFactorAuthentication(is2FAEnabled));
        }

        [HttpPost("enable2fa/complete/{code}/{twoFactorCode}", Name = nameof(CompleteTowFactorAuthentication))]
        public async Task<ActionResult<StandardResponse<UserView>>> CompleteTowFactorAuthentication(string code, Guid twoFactorCode)
        {
            return Result(_userService.Complete2FASetup(code, twoFactorCode));
        }

        [HttpPost("login/complete/{code}/{twoFactorCode}", Name = nameof(CompleteTowFactorAuthenticationLogin))]
        public async Task<ActionResult<StandardResponse<UserView>>> CompleteTowFactorAuthenticationLogin(string code, Guid twoFactorCode)
        {
            return Result(await _userService.Complete2FALogin(code, twoFactorCode));
        }

        [HttpGet("chart/teammembers-by-payrolls", Name = nameof(GetUserCountByPayrolltypePerYear))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<List<UserCountByPayrollTypeView>>>> GetUserCountByPayrolltypePerYear([FromQuery] int year)
        {
            return Result(await _userService.GetUserCountByPayrolltypePerYear(year));
        }

        [HttpGet("subscription/history", Name = nameof(GetClientSubscriptionHistory))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<SubscriptionHistoryViewModel>>> GetClientSubscriptionHistory([FromQuery] Guid superAdminId, [FromQuery] PagingOptions options, [FromQuery] string search = null)
        {
            options.Replace(_defaultPagingOptions);
            return Result(await _userService.GetClientSubscriptionHistory(superAdminId, options, search));
        }
        [HttpGet("subscription/invoices", Name = nameof(GetClientInvoices))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<ClientSubscriptionInvoiceView>>> GetClientInvoices([FromQuery] Guid superAdminId, [FromQuery] PagingOptions options, [FromQuery] string search = null)
        {
            options.Replace(_defaultPagingOptions);
            return Result(await _userService.GetClientInvoices(superAdminId, options, search));
        }

        [HttpPost("subscription/cancel", Name = nameof(CancelSubscription))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<bool>>> CancelSubscription(CancelSubscriptionModel model)
        {
            return Result(await _userService.CancelSubscription(model));
        }

        [HttpPost("subscription/pause", Name = nameof(PauseSubscription))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<bool>>> PauseSubscription([FromQuery] Guid subscriptionId, [FromQuery] int pauseDuration)
        {
            return Result(await _userService.PauseSubscription(subscriptionId, pauseDuration));
        }

        [HttpPost("subscription/upgrade", Name = nameof(UpgradeSubscription))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<ClientSubscriptionResponseViewModel>>> UpgradeSubscription(UpdateClientStripeSubscriptionModel model)
        {
            return Result(await _userService.UpgradeSubscription(model));
        }

        [HttpGet("billing/cards", Name = nameof(GetUserCards))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<Cards>>> GetUserCards([FromQuery] Guid userId)
        {
            return Result(await _userService.GetUserCards(userId));
        }

        [HttpPost("billing/add-card", Name = nameof(AddNewCard))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<CommandCenterAddCardResponse>>> AddNewCard([FromQuery] Guid userId)
        {
            return Result(await _userService.AddNewCard(userId));
        }

        [HttpPost("billing/set-as-default", Name = nameof(SetAsDefaulCard))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<bool>>> SetAsDefaulCard([FromQuery] Guid userId, [FromQuery] string paymentMethod)
        {
            return Result(await _userService.SetAsDefaulCard(userId, paymentMethod));
        }

        [HttpPost("billing/update-card", Name = nameof(UpdateUserCardDetails))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<bool>>> UpdateUserCardDetails([FromQuery] Guid userId, UpdateCardDetailsModel model)
        {
            return Result(await _userService.UpdateUserCardDetails(userId, model));
        }

        [HttpPost("billing/delete-card", Name = nameof(DeletePaymentCard))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<bool>>> DeletePaymentCard([FromQuery] Guid userId, [FromQuery] string paymentMethod)
        {
            return Result(await _userService.DeletePaymentCard(userId, paymentMethod));
        }

        [HttpPost("license/purchase-new-license", Name = nameof(PurchaseNewLicensePlan))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<ClientSubscriptionResponseViewModel>>> PurchaseNewLicensePlan(PurchaseNewLicensePlanModel model)
        {
            return Result(await _userService.PurchaseNewLicensePlan(model));
        }

        [HttpPost("license/update-license-count", Name = nameof(AddOrRemoveLicense))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<ClientSubscriptionResponseViewModel>>> AddOrRemoveLicense(LicenseUpdateModel model)
        {
            return Result(await _userService.AddOrRemoveLicense(model));
        }

        [HttpGet("subscription-types", Name = nameof(GetSubscriptionTypes))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<CommandCenterResponseModel<SubscriptionTypesModel>>>> GetSubscriptionTypes()
        {
            return Result(await _userService.GetSubscriptionTypes());
        }

        [HttpGet("subscriptions", Name = nameof(GetClientSubScriptions))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<List<ClientSubscriptionDetailView>>>> GetClientSubScriptions([FromQuery] Guid superAdminId)
        {
            return Result(await _userService.GetClientSubScriptions(superAdminId));
        }

        [HttpPost("set-as-pm", Name = nameof(ToggleOrganizationProjectManager))]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<StandardResponse<bool>>> ToggleOrganizationProjectManager([FromQuery] Guid id)
        {
            return Result(await _userService.ToggleOrganizationProjectManager(id));
        }

        [HttpPost("revoke-user-license", Name = nameof(RevokeUserLicense))]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<StandardResponse<bool>>> RevokeUserLicense([FromQuery] Guid userId)
        {
            return Result(await _userService.RevokeUserLicense(userId));
        }

        //[HttpPost("billing/add-card", Name = nameof(CreateStripeCustomerCard))]
        //[Authorize]
        //public async Task<ActionResult<StandardResponse<object>>> CreateStripeCustomerCard([FromQuery] Guid userId, CreateCardResource model)
        //{
        //    return Result(await _userService.CreateStripeCustomerCard(userId, model));
        //}

        //[HttpPost("billing/cards", Name = nameof(ListStripreCustomerCard))]
        //[Authorize]
        //public async Task<ActionResult<StandardResponse<List<CardView>>>> ListStripreCustomerCard([FromQuery] Guid userId)
        //{
        //    return Result(await _userService.ListStripreCustomerCard(userId));
        //}

        //[HttpPost("billing/set-as-default", Name = nameof(SetCardAsDefault))]
        //[Authorize]
        //public async Task<ActionResult<StandardResponse<bool>>> SetCardAsDefault([FromQuery] Guid userId, [FromQuery] string cardId)
        //{
        //    return Result(await _userService.SetCardAsDefault(userId, cardId));
        //}

        //[HttpPost("billing/account-update", Name = nameof(UpdateBillingAccountInfomation))]
        //[Authorize]
        //public async Task<ActionResult<StandardResponse<CustomerView>>> UpdateBillingAccountInfomation([FromQuery] Guid userId, CreateCustomerResource model)
        //{
        //    return Result(await _userService.UpdateStripeCustomer(userId, model));
        //}

        //[HttpPost("billing/delete-card", Name = nameof(DeleteCard))]
        //[Authorize]
        //public async Task<ActionResult<StandardResponse<bool>>> DeleteCard([FromQuery] Guid userId, [FromQuery] string cardId)
        //{
        //    return Result(await _userService.DeleteCard(userId, cardId));
        //}

        //[HttpPost("billing/make-payment", Name = nameof(MakePayment))]
        //[Authorize]
        //public async Task<ActionResult<StandardResponse<bool>>> MakePayment([FromQuery] Guid userId, CreateChargeResource model)
        //{
        //    return Result(await _userService.MakePayment(userId, model));
        //}


    }

    public class UserProfile
    {
        public string DisplayName { get; set; }
        public string GivenName { get; set; }
        public string Surname { get; set; }
    }
}