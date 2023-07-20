﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Services.Abstractions;
using TimesheetBE.Services.ConnectedServices.Stripe.Resource;
using TimesheetBE.Utilities;

namespace TimesheetBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : StandardControllerResponse
    {
        private readonly IUserService _userService;
        private readonly PagingOptions _defaultPagingOptions;

        public UserController(IUserService userService, IOptions<PagingOptions> defaultPagingOptions)
        {
            _userService = userService;
            _defaultPagingOptions = defaultPagingOptions.Value;
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
        public async Task<ActionResult<StandardResponse<PagedCollection<UserView>>>> ListUsers([FromQuery] Guid superAdminId, string role, [FromQuery] PagingOptions options, [FromQuery]string Search, [FromQuery] DateFilter dateFilter = null)
        {
            options.Replace(_defaultPagingOptions);
            return Result(await _userService.ListUsers(superAdminId, role, options, Search, dateFilter));
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
        public async Task<ActionResult<StandardResponse<object>>> GetClientSubscriptionHistory([FromQuery] Guid superAdminId, string search = null)
        {
            return Result(await _userService.GetClientSubscriptionHistory(superAdminId, search));
        }

        [HttpPost("subscription/cancel", Name = nameof(CancelSubscription))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<object>>> CancelSubscription([FromQuery] Guid subscriptionId)
        {
            return Result(await _userService.CancelSubscription(subscriptionId));
        }

        //[HttpPost("create-customer", Name = nameof(CreateStripeCustomer))]
        ////[Authorize]
        //public async Task<ActionResult<StandardResponse<Customer>>> CreateStripeCustomer(CreateCustomerResource model)
        //{
        //    return Result(await _userService.CreateStripeCustomer(model));
        //}

        //[HttpPost("create-card", Name = nameof(CreateStripeCustomerCard))]
        ////[Authorize]
        //public async Task<ActionResult<StandardResponse<Customer>>> CreateStripeCustomerCard([FromQuery] string stripeCustomerId, CreateCardResource model)
        //{
        //    return Result(await _userService.CreateStripeCustomerCard(stripeCustomerId, model));
        //}


    }
}