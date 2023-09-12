using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TimesheetBE.Controllers;
using TimesheetBE.Models;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Models.IdentityModels;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Repositories.Interfaces;
using TimesheetBE.Services.Abstractions;
using TimesheetBE.Services.Interfaces;
using TimesheetBE.Utilities;
using TimesheetBE.Utilities.Abstrctions;
using TimesheetBE.Utilities.Constants;
using TimesheetBE.Utilities.Extentions;
using GoogleAuthenticatorService.Core;
using ClosedXML.Excel;
using System.Net.Http;
using Newtonsoft.Json;
using RestSharp;
using TimesheetBE.Services.ConnectedServices.Stripe.Resource;
using TimesheetBE.Services.ConnectedServices.Stripe;

namespace TimesheetBE.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        public readonly RoleManager<Role> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ICodeProvider _codeProvider;
        private readonly Globals _appSettings;
        private readonly IEmailHandler _emailHandler;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfigurationProvider _configuration;
        private readonly ILogger<UserService> _logger;
        private readonly IEmployeeInformationRepository _employeeInformationRepository;
        private readonly IContractRepository _contractRepository;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IUtilityMethods _utilityMethods;
        private readonly INotificationService _notificationService;
        private readonly IDataExport _dataExport;
        private readonly IShiftService _shiftService;
        private readonly ILeaveService _leaveService;
        private readonly IControlSettingRepository _controlSettingRepository;
        private readonly ILeaveConfigurationRepository _leaveConfigurationRepository;
        private readonly IStripeService _stripeService;

        public UserService(UserManager<User> userManager, SignInManager<User> signInManager, IMapper mapper, IUserRepository userRepository,
            IOptions<Globals> appSettings, IHttpContextAccessor httpContextAccessor, ICodeProvider codeProvider, IEmailHandler emailHandler,
            IConfigurationProvider configuration, RoleManager<Role> roleManager, ILogger<UserService> logger, IEmployeeInformationRepository employeeInformationRepository,
            IContractRepository contractRepository, IConfigurationProvider configurationProvider, IUtilityMethods utilityMethods, INotificationService notificationService,
            IDataExport dataExport, IShiftService shiftService, ILeaveService leaveService, IControlSettingRepository controlSettingRepository, ILeaveConfigurationRepository leaveConfigurationRepository,
            IStripeService stripeService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
            _userRepository = userRepository;
            _appSettings = appSettings.Value;
            _httpContextAccessor = httpContextAccessor;
            _codeProvider = codeProvider;
            _emailHandler = emailHandler;
            _configuration = configuration;
            _roleManager = roleManager;
            _logger = logger;
            _employeeInformationRepository = employeeInformationRepository;
            _contractRepository = contractRepository;
            _configuration = configuration;
            _configurationProvider = configurationProvider;
            _utilityMethods = utilityMethods;
            _notificationService = notificationService;
            _dataExport = dataExport;
            _shiftService = shiftService;
            _leaveService = leaveService;
            _controlSettingRepository = controlSettingRepository;
            _leaveConfigurationRepository = leaveConfigurationRepository;
            _stripeService = stripeService;
        }

        public async Task<StandardResponse<UserView>> CreateUser(RegisterModel model)
        {
            try
            {
                var ExistingUser = _userManager.FindByEmailAsync(model.Email).Result;
                model.Password = !string.IsNullOrEmpty(model.Password) ? model.Password : "genericpassword";


                if (ExistingUser != null)
                    return StandardResponse<UserView>.Failed(StandardResponseMessages.USER_ALREADY_EXISTS, HttpStatusCode.BadRequest).AddStatusMessage(StandardResponseMessages.USER_ALREADY_EXISTS);

                var thisUser = _mapper.Map<User>(model);
                thisUser.EmployeeInformationId = null;

                var roleExists = AscertainRoleExists(model.Role);

                var Result = _userRepository.CreateUser(thisUser).Result;

                if (!Result.Succeeded)
                    return StandardResponse<UserView>.Error(Result.ErrorMessage);

                var result = _userManager.AddToRoleAsync(Result.CreatedUser, model.Role).Result.Succeeded;

                var createdUser = _userManager.FindByEmailAsync(model.Email).Result;

                if (model.Role.ToLower() == "super admin")
                {
                    var settings = _controlSettingRepository.CreateAndReturn(new ControlSetting { SuperAdminId = createdUser.Id });
                    var leaveConfig = _leaveConfigurationRepository.CreateAndReturn(new LeaveConfiguration { SuperAdminId = createdUser.Id });
                    createdUser.ControlSettingId = settings.Id;
                    createdUser.LeaveConfigurationId = leaveConfig.Id;
                }

                createdUser.Role = model.Role;
                createdUser.IsActive = false;
                createdUser.TwoFactorCode = Guid.NewGuid();

                var updateResult = _userManager.UpdateAsync(createdUser).Result;

                if (model.Role.ToLower() == "team member")
                {
                    //get all admins and superadmins emails
                    //var getAdmins = _userRepository.Query().Where(x => (x.Role.ToLower() == "super admin" || x.Role.ToLower() == "admin") && x.SuperAdminId == model.SuperAdminId).ToList();
                    var getAdmins = _userRepository.Query().Where(x => x.Role.ToLower() == "super admin" && x.SuperAdminId == model.SuperAdminId).ToList();

                    var adminEmails = new List<string>();
                    foreach (var admin in getAdmins)
                    {
                        adminEmails.Add(admin.Email);
                    }
                    return InitiateTeamMemberActivation(new InitiateTeamMemberActivationModel { AdminEmails = adminEmails, Email = createdUser.Email }).Result;

                }
                return SendNewUserPasswordReset(new InitiateResetModel { Email = createdUser.Email }).Result;
            }
            catch (Exception ex)
            {
                return StandardResponse<UserView>.Failed(ex.Message);
            }
        }

        //public async Task<StandardResponse<UserView>> CreateSubscription()
        //{
        //    try
        //    {

        //    }
        //    catch (Exception ex)
        //    {
        //        return StandardResponse<UserView>.Failed(ex.Message);
        //    }
        //}

        public async Task<StandardResponse<UserView>> InitiateNewUserPasswordReset(InitiateResetModel model)
        {
            try
            {

                var ThisUser = _userRepository.ListUsers().Result.Users.FirstOrDefault(u => u.Email == model.Email);
                if (ThisUser == null)
                {
                    ThisUser = _userManager.FindByEmailAsync(model.Email).Result;
                }

                var Token = _userManager.GeneratePasswordResetTokenAsync(ThisUser).Result;

                Code PasswordResetCode = _codeProvider.New(ThisUser.Id, Constants.PASSWORD_RESET_CODE, _appSettings.PasswordResetExpiry);

                PasswordResetCode.Token = Token;
                _codeProvider.Update(PasswordResetCode);

                var ConfirmationLink = "";
                ConfirmationLink = $"{Globals.FrontEndBaseUrl}{_appSettings.CompletePasswordResetUrl}{PasswordResetCode.CodeString}";

                List<KeyValuePair<string, string>> EmailParameters = new()
                {
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_URL, ConfirmationLink),
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, ThisUser.FirstName),
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_LOGO_URL, _appSettings.LOGO),
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_EXPIRYDATE, PasswordResetCode.ExpiryDate.ToShortDateString()),
                    new KeyValuePair<string, string>("Reset Password", "Click Here To Verify")
                };


                var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.PASSWORD_RESET_EMAIL_FILENAME, EmailParameters);
                var SendEmail = _emailHandler.SendEmail(ThisUser.Email, Constants.PASSWORD_RESET_EMAIL_SUBJECT, EmailTemplate, "");

                var mappedView = _mapper.Map<UserView>(ThisUser);
                return StandardResponse<UserView>.Ok(mappedView).AddStatusMessage(StandardResponseMessages.PASSWORD_RESET_EMAIL_SENT);

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            }
        }

        public async Task<StandardResponse<UserView>> SendNewUserPasswordReset(InitiateResetModel model)
        {
            try
            {

                var ThisUser = _userRepository.ListUsers().Result.Users.FirstOrDefault(u => u.Email == model.Email);
                if (ThisUser == null)
                {
                    ThisUser = _userManager.FindByEmailAsync(model.Email).Result;
                }

                var Token = _userManager.GeneratePasswordResetTokenAsync(ThisUser).Result;

                Code PasswordResetCode = _codeProvider.New(ThisUser.Id, Constants.PASSWORD_RESET_CODE, _appSettings.PasswordResetExpiry);

                PasswordResetCode.Token = Token;
                _codeProvider.Update(PasswordResetCode);

                var ConfirmationLink = "";
                ConfirmationLink = $"{Globals.FrontEndBaseUrl}{_appSettings.CompletePasswordResetUrl}{PasswordResetCode.CodeString}";

                List<KeyValuePair<string, string>> EmailParameters = new()
                {
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_URL, ConfirmationLink),
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, ThisUser.FirstName),
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_LOGO_URL, _appSettings.LOGO),
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_EXPIRYDATE, PasswordResetCode.ExpiryDate.ToShortDateString()),
                    new KeyValuePair<string, string>("Reset Password", "Click Here To Verify")
                };


                var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.NEW_USER_PASSWORD_RESET_FILENAME, EmailParameters);
                var SendEmail = _emailHandler.SendEmail(ThisUser.Email, Constants.NEW_USER_PASSWORD_RESET, EmailTemplate, "");

                var mappedView = _mapper.Map<UserView>(ThisUser);
                return StandardResponse<UserView>.Ok(mappedView).AddStatusMessage(StandardResponseMessages.PASSWORD_RESET_EMAIL_SENT);

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            }
        }

        public async Task<StandardResponse<UserView>> InitiateTeamMemberActivation(InitiateTeamMemberActivationModel model)
        {
            try
            {

                var ThisUser = _userRepository.ListUsers().Result.Users.FirstOrDefault(u => u.Email == model.Email);
                if (ThisUser == null)
                {
                    ThisUser = _userManager.FindByEmailAsync(model.Email).Result;
                }

                var ActivationLink = "";
                ActivationLink = $"{Globals.FrontEndBaseUrl}{_appSettings.ActivateTeamMemberUrl}{ThisUser.Id}";

                foreach (var email in model.AdminEmails)
                {
                    var user = _userManager.FindByEmailAsync(email).Result;
                    List<KeyValuePair<string, string>> EmailParameters = new()
                    {
                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_URL, ActivationLink),
                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, user.FirstName),
                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_TEAMMEMBER_NAME, ThisUser.FirstName),
                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_LOGO_URL, _appSettings.LOGO),
                        new KeyValuePair<string, string>("Activate Team Member", "Click Here To Activate")
                    };


                    var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.ACTIVATE_TEAMMEMBER_EMAIL_FILENAME, EmailParameters);
                    var SendEmail = _emailHandler.SendEmail(user.Email, Constants.ACTIVATE_TEAMMEMBER_EMAIL_SUBJECT, EmailTemplate, "");
                }

                var mappedView = _mapper.Map<UserView>(ThisUser);
                return StandardResponse<UserView>.Ok(mappedView).AddStatusMessage(StandardResponseMessages.ACTIVATE_RESET_EMAIL_SENT);

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            }
        }

        private bool AscertainRoleExists(string roleName)
        {
            var roleExists = _roleManager.RoleExistsAsync(roleName).Result;
            if (!roleExists)
            {
                var role = new Role()
                {
                    Name = roleName
                };
                var roleCreated = _roleManager.CreateAsync(role).Result;
                return roleExists;
            }
            return true;
        }

        public async Task<StandardResponse<UserView>> CreateAdminUser(RegisterModel newUser)
        {
            try
            {
                var ExistingUser = _userManager.FindByEmailAsync(newUser.Email).Result;

                if (ExistingUser != null)
                    return StandardResponse<UserView>.Failed().AddStatusMessage(StandardResponseMessages.USER_ALREADY_EXISTS);

                var roleExists = AscertainRoleExists("Super Admin");
                var User = _mapper.Map<RegisterModel, User>(newUser);
                User.UserName = newUser.Email;
                User.Password = newUser.Password;

                var Result = _userRepository.CreateUser(User).Result;

                if (!Result.Succeeded)
                    return StandardResponse<UserView>.Error(Result.ErrorMessage);

                var result = _userManager.AddToRoleAsync(Result.CreatedUser, "Supper Admin").Result.Succeeded;

                var key = _codeProvider.New(Guid.Empty, Constants.ADMIN_USER_API_KEY, 100000, 36, "US");

                var mapped = _mapper.Map<UserView>(Result.CreatedUser);

                return StandardResponse<UserView>.Ok(mapped);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return StandardResponse<UserView>.Failed().AddStatusMessage(StandardResponseMessages.ERROR_OCCURRED);
            }

        }
        public async Task<StandardResponse<UserView>> AuthenticateAdmin(LoginModel userToLogin)
        {
            try
            {
                var User = _userManager.FindByEmailAsync(userToLogin.Email).Result;

                if (!User.IsActive)
                    return StandardResponse<UserView>.Failed().AddStatusMessage("Your account has been deactivated please contact admin");

                if (User == null)
                    return StandardResponse<UserView>.Failed().AddStatusMessage(StandardResponseMessages.USER_NOT_FOUND);

                var isInRole = _userManager.IsInRoleAsync(User, "ADMIN").Result;

                var roles = _userManager.GetRolesAsync(User).Result;

                User = _mapper.Map<LoginModel, User>(userToLogin);

                if (!isInRole)
                    return StandardResponse<UserView>.Failed().AddStatusMessage(StandardResponseMessages.USER_NOT_PERMITTED);


                var Result = _userRepository.Authenticate(User).Result;

                if (!Result.Succeeded)
                    return StandardResponse<UserView>.Failed().AddStatusMessage(Result.ErrorMessage ?? StandardResponseMessages.ERROR_OCCURRED);

                if (Result.LoggedInUser.TwoFactorCode == null)
                {
                    Result.LoggedInUser.TwoFactorCode = Guid.NewGuid();
                    var res = _userManager.UpdateAsync(Result.LoggedInUser).Result;
                }

                var mapped = _mapper.Map<UserView>(Result.LoggedInUser);

                return StandardResponse<UserView>.Ok(mapped);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StandardResponse<UserView>.Failed();
            }
        }

        public async Task<StandardResponse<UserView>> VerifyUser(string token)
        {
            try
            {

                Code ThisCode = _codeProvider.GetByCodeString(token.ToLower());

                var UserToVerify = _userManager.FindByIdAsync(ThisCode.UserId.ToString()).Result;

                if (UserToVerify == null)
                    return StandardResponse<UserView>.Failed().AddStatusMessage(StandardResponseMessages.USER_NOT_FOUND);

                if (UserToVerify.EmailConfirmed)
                    return StandardResponse<UserView>.Ok().AddStatusMessage(StandardResponseMessages.ALREADY_ACTIVATED);

                if (ThisCode.IsExpired || ThisCode.ExpiryDate < DateTime.Now || ThisCode.Key != Constants.NEW_EMAIL_VERIFICATION_CODE)
                    return StandardResponse<UserView>.Failed().AddStatusMessage(StandardResponseMessages.EMAIL_VERIFICATION_FAILED);

                UserToVerify.EmailConfirmed = true;
                UserToVerify.IsActive = true;

                var Verified = _userManager.UpdateAsync(UserToVerify).Result;

                if (!Verified.Succeeded)
                    return StandardResponse<UserView>.Failed().AddStatusMessage(StandardResponseMessages.ERROR_OCCURRED);

                _codeProvider.SetExpired(ThisCode);

                return StandardResponse<UserView>.Ok().AddStatusMessage(StandardResponseMessages.EMAIL_VERIFIED);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return StandardResponse<UserView>.Failed();
            }
        }

        public async Task<StandardResponse<UserView>> Authenticate(LoginModel userToLogin)
        {
            var User = _userManager.FindByEmailAsync(userToLogin.Email).Result;

            if (User == null)
                return StandardResponse<UserView>.Failed().AddStatusMessage(StandardResponseMessages.USER_NOT_FOUND);

            if (!User.EmailConfirmed)
                return StandardResponse<UserView>.Failed().AddStatusMessage("Please check your email to verify your account");
            if (!User.IsActive)
                return StandardResponse<UserView>.Failed().AddStatusMessage("Your account has been deactivated please contact admin");

            User = _mapper.Map<LoginModel, User>(userToLogin);

            var Result = _userRepository.Authenticate(User).Result;

            if (!Result.Succeeded)
                return StandardResponse<UserView>.Failed().AddStatusMessage((Result.ErrorMessage ?? StandardResponseMessages.ERROR_OCCURRED));
            if (Result.LoggedInUser.TwoFactorCode == null)
            {
                Result.LoggedInUser.TwoFactorCode = Guid.NewGuid();
                var res = _userManager.UpdateAsync(Result.LoggedInUser).Result;
            }

            var mapped = _mapper.Map<UserView>(Result.LoggedInUser);
            var rroles = _userManager.GetRolesAsync(Result.LoggedInUser).Result;
            mapped.Role = _userManager.GetRolesAsync(Result.LoggedInUser).Result.FirstOrDefault();
            var employeeInformation = _employeeInformationRepository.Query().Include(user => user.PayrollType).FirstOrDefault(empInfo => empInfo.Id == Result.LoggedInUser.EmployeeInformationId);
            mapped.PayrollType = employeeInformation?.PayrollType.Name;
            mapped.NumberOfDaysEligible = employeeInformation?.NumberOfDaysEligible;
            mapped.NumberOfLeaveDaysTaken = employeeInformation?.NumberOfEligibleLeaveDaysTaken;
            mapped.NumberOfHoursEligible = employeeInformation?.NumberOfHoursEligible;
            mapped.EmployeeType = employeeInformation?.EmployeeType;

            var user = _userRepository.Query().Include(x => x.EmployeeInformation).Include(x => x.SuperAdmin).Where(x => x.Id == mapped.Id).FirstOrDefault();

            if (user.Role.ToLower() == "super admin")
            {
                mapped.SubscriptiobDetails = GetSubscriptionDetails(user.ClientSubscriptionId).Result.Data;
                mapped.SuperAdminId = user.Id;
            }

            else
            {
                mapped.SubscriptiobDetails = GetSubscriptionDetails(user.SuperAdmin.ClientSubscriptionId).Result.Data;
            }

            if (user.Role.ToLower() == "admin")
            {
                var controlSetting = _controlSettingRepository.Query().FirstOrDefault(x => x.SuperAdminId == user.SuperAdminId);
                mapped.ControlSettingView = _mapper.Map<ControlSettingView>(controlSetting);
            }

            if (employeeInformation != null)
            {
                var getNumberOfDaysEligible = _leaveService.GetEligibleLeaveDays(employeeInformation.Id);

                mapped.NumberOfDaysEligible = getNumberOfDaysEligible - employeeInformation?.NumberOfEligibleLeaveDaysTaken;
                mapped.ClientId = employeeInformation?.ClientId;
            }

            return StandardResponse<UserView>.Ok(mapped);
        }

        public async Task<StandardResponse<UserView>> Complete2FALogin(string Code, Guid TwoFactorCode)
        {
            try
            {
                var validationResult = ValidateTwoFactorPIN(Code, TwoFactorCode);
                if (!validationResult)
                    return StandardResponse<UserView>.Error("Invalid Code");

                var user = _userRepository.Query().FirstOrDefault(u => u.TwoFactorCode == TwoFactorCode);
                if (user == null)
                    return StandardResponse<UserView>.Error("An Error Occurred");

                var userView = _mapper.Map<UserView>(user);
                return StandardResponse<UserView>.Ok(userView);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Complete2FALogin");
                return StandardResponse<UserView>.Error("An Error Occurred");
            }
        }

        public async Task<StandardResponse<UserView>> UpdatePassword(string newPassword)
        {
            try
            {
                Guid UserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();
                var ThisUser = _userManager.FindByIdAsync(UserId.ToString()).Result;

                var Token = _userManager.GeneratePasswordResetTokenAsync(ThisUser).Result;

                var Result = _userManager.ResetPasswordAsync(ThisUser, Token, newPassword).Result;

                if (!Result.Succeeded)
                    return StandardResponse<UserView>.Failed().AddStatusMessage(StandardResponseMessages.ERROR_OCCURRED);

                List<KeyValuePair<string, string>> EmailParameters = new List<KeyValuePair<string, string>>();

                EmailParameters.Add(new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, ThisUser.FirstName));
                EmailParameters.Add(new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_LOGO_URL, _appSettings.LOGO));


                var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.PASSWORD_RESET_SUCCESS_EMAIL_FILENAME, EmailParameters);
                var SendEmail = _emailHandler.SendEmail(ThisUser.Email, Constants.PASSWORD_RESET_EMAIL_SUBJECT, EmailTemplate, "");

                return StandardResponse<UserView>.Ok().AddStatusMessage(StandardResponseMessages.PASSWORD_RESET_COMPLETE);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StandardResponse<UserView>.Failed();
            }
        }

        public async Task<StandardResponse<UserView>> InitiatePasswordReset(InitiateResetModel model, string redirectUrl = null)
        {
            try
            {

                var ThisUser = _userRepository.ListUsers().Result.Users.FirstOrDefault(u => u.Email == model.Email);
                if (ThisUser == null)
                {
                    ThisUser = _userManager.FindByEmailAsync(model.Email).Result;
                }

                var Token = _userManager.GeneratePasswordResetTokenAsync(ThisUser).Result;

                Code PasswordResetCode = _codeProvider.New(ThisUser.Id, Constants.PASSWORD_RESET_CODE, _appSettings.PasswordResetExpiry);

                PasswordResetCode.Token = Token;
                _codeProvider.Update(PasswordResetCode);

                var ConfirmationLink = "";
                ConfirmationLink = $"{Globals.FrontEndBaseUrl}{_appSettings.CompletePasswordResetUrl}{PasswordResetCode.CodeString}";

                var EmailParameters = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_URL, ConfirmationLink),
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, ThisUser.FirstName),
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_LOGO_URL, _appSettings.LOGO),
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_EXPIRYDATE, PasswordResetCode.ExpiryDate.ToShortDateString())
                };


                var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.PASSWORD_RESET_EMAIL_FILENAME, EmailParameters);
                var SendEmail = _emailHandler.SendEmail(ThisUser.Email, Constants.PASSWORD_RESET_EMAIL_SUBJECT, EmailTemplate, "");

                return StandardResponse<UserView>.Ok().AddStatusMessage(StandardResponseMessages.PASSWORD_RESET_EMAIL_SENT);

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            }
        }

        public async Task<StandardResponse<UserView>> CompletePasswordReset(PasswordReset payload)
        {
            try
            {
                Code ThisCode = _codeProvider.GetByCodeString(payload.Code);

                var ThisUser = _userManager.FindByIdAsync(ThisCode.UserId.ToString()).Result;
                var isUserConfirmed = ThisUser.EmailConfirmed;

                if (ThisUser == null)
                    return StandardResponse<UserView>.Failed().AddStatusMessage(StandardResponseMessages.USER_NOT_FOUND);

                if (ThisCode.IsExpired || ThisCode.ExpiryDate < DateTime.Now || ThisCode.Key != Constants.PASSWORD_RESET_CODE)
                    return StandardResponse<UserView>.Failed().AddStatusMessage(StandardResponseMessages.PASSWORD_RESET_FAILED);

                var Result = _userManager.ResetPasswordAsync(ThisUser, ThisCode.Token, payload.NewPassword).Result;

                if (!Result.Succeeded)
                    return StandardResponse<UserView>.Failed().AddStatusMessage(StandardResponseMessages.ERROR_OCCURRED);

                ThisUser.EmailConfirmed = true;
                ThisUser.IsActive = true;

                if (ThisUser.Role.ToLower() == "super admin")
                {
                    var updatedOnCommandCenter = await ActivateClientOnCommandCenter(ThisUser.CommandCenterClientId);

                    if (updatedOnCommandCenter.Status == false)
                    {
                        ThisUser.EmailConfirmed = false;
                        ThisUser.IsActive = false;
                        return StandardResponse<UserView>.Failed().AddStatusMessage("Unable to activate team member on command center, please try again");
                    }
                }


                var updateResult = _userManager.UpdateAsync(ThisUser).Result;
                if (!isUserConfirmed && ThisUser.Role.ToLower() == "team member")
                {
                    var getAdmins = _userRepository.Query().Include(x => x.EmployeeInformation).Where(x => (x.Role.ToLower() == "super admin" || (x.Role.ToLower() == "admin") && x.SuperAdminId == ThisUser.SuperAdminId)).ToList();
                    //var getAdmins = _userRepository.Query().Include(x => x.EmployeeInformation).Where(x => x.Role.ToLower() == "super admin" || (x.Role.ToLower() == "admin")).ToList();
                    foreach (var admin in getAdmins)
                    {
                        List<KeyValuePair<string, string>> EmailParameters = new()
                    {
                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, admin.FirstName),
                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_TEAMMEMBER_NAME, ThisUser.FullName),
                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_LOGO_URL, _appSettings.LOGO),
                    };


                        var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.PASSWORD_RESET_NOTIFICATION_FILENAME, EmailParameters);
                        var SendEmail = _emailHandler.SendEmail(admin.Email, Constants.PASSWORD_RESET_NOTIFICATION_SUBJECT, EmailTemplate, "");
                    }
                }
                return StandardResponse<UserView>.Ok().AddStatusMessage(StandardResponseMessages.PASSWORD_RESET_COMPLETE);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return StandardResponse<UserView>.Failed(e.Message);
            }

        }

        public async Task<StandardResponse<UserView>> UpdateUser(UpdateUserModel model)
        {
            try
            {
                Guid UserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();
                var thisUser = _userRepository.ListUsers().Result.Users.FirstOrDefault(u => u.Id == UserId);
                if (!string.IsNullOrEmpty(model.PhoneNumber))
                {
                    thisUser.PhoneNumber = model.PhoneNumber;
                }
                thisUser.FirstName = model.FirstName;
                thisUser.LastName = model.LastName;
                thisUser.ProfilePicture = model.ProfilePicture;
                thisUser.DateOfBirth = model.DateOfBirth;
                thisUser.Address = model.Address;
                thisUser.IsActive = model.IsActive;

                var up = _userManager.UpdateAsync(thisUser).Result;
                if (!up.Succeeded)
                    return StandardResponse<UserView>.Failed(up.Errors.FirstOrDefault().Description);

                var updatedUser = _userRepository.Query().FirstOrDefault(u => u.Id == UserId);

                return StandardResponse<UserView>.Ok(_mapper.Map<UserView>(updatedUser));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StandardResponse<UserView>.Failed();
            }
        }

        public async Task<StandardResponse<bool>> UpdateControlSettings(ControlSettingModel model)
        {
            try
            {
                var settings = _controlSettingRepository.Query().FirstOrDefault(x => x.SuperAdminId == model.SuperAdminId);

                if (model.TwoFactorEnabled.HasValue) settings.TwoFactorEnabled = model.TwoFactorEnabled.Value;
                if (model.AdminOBoarding.HasValue) settings.AdminOBoarding = model.AdminOBoarding.Value;
                if (model.AdminContractManagement.HasValue) settings.AdminContractManagement = model.AdminContractManagement.Value;
                if (model.AdminLeaveManagement.HasValue) settings.AdminLeaveManagement = model.AdminLeaveManagement.Value;
                if (model.AdminShiftManagement.HasValue) settings.AdminShiftManagement = model.AdminShiftManagement.Value;
                if (model.AdminReport.HasValue) settings.AdminReport = model.AdminReport.Value;
                if (model.AdminExpenseTypeAndHST.HasValue) settings.AdminExpenseTypeAndHST = model.AdminExpenseTypeAndHST.Value;
                if (model.AllowShiftSwapRequest.HasValue) settings.AllowShiftSwapRequest = model.AllowShiftSwapRequest.Value;
                if (model.AllowShiftSwapApproval.HasValue) settings.AllowShiftSwapApproval = model.AllowShiftSwapApproval.Value;
                if (model.AllowIneligibleLeaveCode.HasValue) settings.AllowIneligibleLeaveCode = model.AllowIneligibleLeaveCode.Value;

                _controlSettingRepository.Update(settings);

                return StandardResponse<bool>.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StandardResponse<bool>.Failed();
            }
        }

        public async Task<StandardResponse<ControlSettingView>> GetControlSettingById(Guid superAdminId)
        {
            try
            {
                var controlSettings = _controlSettingRepository.Query().FirstOrDefault(x => x.SuperAdminId == superAdminId);

                if (controlSettings == null) return StandardResponse<ControlSettingView>.Failed().AddStatusMessage("Conrol setting not found");

                var mappedSettings = _mapper.Map<ControlSettingView>(controlSettings);

                return StandardResponse<ControlSettingView>.Ok(mappedSettings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StandardResponse<ControlSettingView>.Error(ex.Message);
            }
        }

        public async Task<StandardResponse<UserView>> UpdateClientSubscription(UpdateClientSubscriptionModel model)
        {
            try
            {
                var thisUser = _userRepository.ListUsers().Result.Users.FirstOrDefault(u => u.CommandCenterClientId == model.CommandCenterClientId);
                thisUser.ClientSubscriptionId = model.ClientSubscriptionId;
                thisUser.ClientSubscriptionStatus = model.SubscriptionStatus;

                var up = _userManager.UpdateAsync(thisUser).Result;
                if (!up.Succeeded)
                    return StandardResponse<UserView>.Failed(up.Errors.FirstOrDefault().Description);

                var updatedUser = _userRepository.Query().FirstOrDefault(u => u.Id == model.CommandCenterClientId);

                return StandardResponse<UserView>.Ok(_mapper.Map<UserView>(updatedUser));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StandardResponse<UserView>.Failed();
            }
        }

        public async Task<StandardResponse<UserView>> AdminUpdateUser(UpdateUserModel model)
        {
            try
            {
                var thisUser = _userRepository.ListUsers().Result.Users.FirstOrDefault(u => u.Id == model.Id);
                if (!string.IsNullOrEmpty(model.PhoneNumber))
                {
                    thisUser.PhoneNumber = model.PhoneNumber;
                }

                var isInitialRole = thisUser.Role.ToLower() == model.Role.ToLower() ? true : false;

                thisUser.FirstName = model.FirstName;
                thisUser.LastName = model.LastName;
                thisUser.IsActive = model.IsActive;
                thisUser.OrganizationName = model.OrganizationName;
                thisUser.OrganizationAddress = model.OrganizationAddress;
                thisUser.InvoiceGenerationFrequency = model.InvoiceGenerationFrequency;
                thisUser.Term = model.Term;

                if (thisUser.Role.ToLower() != model.Role.ToLower())
                {
                    var res = _userManager.RemoveFromRoleAsync(thisUser, thisUser.Role).Result;
                    var roleExists = AscertainRoleExists(model.Role);
                    var added = _userManager.AddToRoleAsync(thisUser, model.Role).Result;
                }
                thisUser.Role = model.Role;

                var up = _userManager.UpdateAsync(thisUser).Result;

                if (!up.Succeeded)
                    return StandardResponse<UserView>.Failed(up.Errors.FirstOrDefault().Description);

                if (model.IsActive == false)
                {
                    List<KeyValuePair<string, string>> EmailParameters = new()
                    {
                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, thisUser.FirstName),
                    };


                    var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.DEACTIVATE_USER_EMAIL_FILENAME, EmailParameters);
                    var SendEmail = _emailHandler.SendEmail(thisUser.Email, "Account Deactivation", EmailTemplate, "");

                    var getAdmins = _userRepository.Query().Where(x => x.Role.ToLower() == "super admin").ToList();
                    foreach (var admin in getAdmins)
                    {
                        await _notificationService.SendNotification(new NotificationModel { UserId = admin.Id, Title = "Account Deactivation", Type = "Notification", Message = "Account Deactivation Was succesful" });
                    }
                }

                if (isInitialRole == false)
                {
                    List<KeyValuePair<string, string>> EmailParameters = new()
                    {
                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, thisUser.FirstName),
                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_ROLE, model.Role.ToUpper()),

                    };

                    var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.ROLE_CHANGE_FILENAME, EmailParameters);
                    var SendEmail = _emailHandler.SendEmail(thisUser.Email, "User Role Updated", EmailTemplate, "");
                }

                return StandardResponse<UserView>.Ok(_mapper.Map<UserView>(thisUser));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StandardResponse<UserView>.Failed();
            }
        }

        public async Task<StandardResponse<UserProfileView>> UserProfile(Guid userId)
        {
            try
            {
                var userProfile = _userRepository.ListUsers().Result.Users
                                                 .FirstOrDefault(user => user.Id == userId);

                if (userProfile == null)
                    return StandardResponse<UserProfileView>.Error(StandardResponseMessages.USER_NOT_FOUND);

                var mappedUserProfile = _mapper.Map<UserProfileView>(userProfile);

                return StandardResponse<UserProfileView>.Ok(mappedUserProfile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return StandardResponse<UserProfileView>.Failed(ex.Message);
            }
        }

        public async Task<StandardResponse<UserView>> GetUserByToken()
        {
            try
            {
                Guid UserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();
                var thisUser = _userRepository.ListUsers().Result.Users.FirstOrDefault(u => u.Id == UserId);
                if (thisUser == null)
                    return StandardResponse<UserView>.Failed().AddStatusMessage(StandardResponseMessages.USER_NOT_FOUND);

                return StandardResponse<UserView>.Ok(_mapper.Map<UserView>(thisUser));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StandardResponse<UserView>.Failed();
            }
        }

        public async Task<StandardResponse<PagedCollection<UserView>>> ListUsers(Guid superAdminId, string role, PagingOptions options, string search = null, DateFilter dateFilter = null)
        {
            try
            {
                var users = _userRepository.Query().Where(x => x.SuperAdminId == superAdminId).AsQueryable();
                //var users = _userRepository.Query().Include(x => x.EmployeeInformation).ThenInclude(x => x.Supervisor).Include(x => x.EmployeeInformation).ThenInclude(x => x.Client).Where(x => x.SuperAdminId == superAdminId).AsQueryable();

                if (role.ToLower() == "admins")
                    users = users.Where(u => u.Role == "Admin" || u.Role == "Super Admin" || u.Role == "Payroll Manager").OrderByDescending(x => x.DateCreated);
                else if (role.ToLower() == "team member")
                    users = users.Where(u => u.Role.ToLower() == "team member").OrderByDescending(x => x.DateCreated);
                else if (role.ToLower() == "supervisor")
                    users = users.Where(u => u.Role.ToLower() == "supervisor" || u.Role.ToLower() == "internal supervisor").OrderByDescending(x => x.DateCreated);
                else if (role.ToLower() == "payroll manager")
                    users = users.Where(u => u.Role.ToLower() == "payroll manager" || u.Role.ToLower() == "internal payroll manager").OrderByDescending(x => x.DateCreated);
                else if (role.ToLower() == "admin")
                    users = users.Where(u => u.Role.ToLower() == "admin" || u.Role.ToLower() == "internal admin").OrderByDescending(x => x.DateCreated);
                else
                    users = users.Where(u => u.Role.ToLower() == role.ToLower()).OrderByDescending(x => x.DateCreated);

                if (!string.IsNullOrEmpty(search))
                    users = users.Where(u => u.FirstName.ToLower().Contains(search.ToLower()) || u.LastName.ToLower().Contains(search.ToLower()) || (u.FirstName.ToLower() + " " + u.LastName.ToLower()).Contains(search.ToLower()) || u.Email.ToLower().Contains(search.ToLower())
                    || u.Role.ToLower().Contains(search.ToLower()) || u.EmployeeInformation.PayrollType.Name.ToLower().Contains(search.ToLower())).OrderByDescending(x => x.DateCreated); ;

                if (dateFilter.StartDate.HasValue)
                    users = users.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

                if (dateFilter.EndDate.HasValue)
                    users = users.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);

                var pagedUsers = users.Skip(options.Offset.Value).Take(options.Limit.Value).AsQueryable();

                var mappedUsers = pagedUsers.ProjectTo<UserView>(_configuration);

                var pagedCollection = PagedCollection<UserView>.Create(Link.ToCollection(nameof(UserController.ListUsers)), mappedUsers.ToArray(), users.Count(), options);

                return StandardResponse<PagedCollection<UserView>>.Ok(pagedCollection);

            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<UserView>>.Error(e.Message);
            }

        }

        //403 error
        public async Task<StandardResponse<UserView>> GetById(Guid id)
        {
            try
            {
                var thisUser = _userRepository.Query()
                .Include(x => x.EmployeeInformation).ThenInclude(x => x.Contracts).ThenInclude(c => c.Status)
                .Include(x => x.EmployeeInformation).ThenInclude(x => x.Client)
                .Include(x => x.EmployeeInformation).ThenInclude(x => x.Supervisor).ThenInclude(x => x.Client)
                .Include(x => x.EmployeeInformation).ThenInclude(x => x.PaymentPartner)
                .Include(x => x.EmployeeInformation).ThenInclude(x => x.PayrollType)
                //.Include(x => x.EmployeeInformation).ThenInclude(x => x.PayrollGroup)
                .FirstOrDefault(u => u.Id == id);

                if (thisUser == null)
                    return StandardResponse<UserView>.Failed().AddStatusMessage(StandardResponseMessages.USER_NOT_FOUND);

                var userView = _mapper.Map<UserView>(thisUser);

                return StandardResponse<UserView>.Ok(userView);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StandardResponse<UserView>.Error(ex.Message);
            }
        }

        public async Task<StandardResponse<UserView>> ToggleUserIsActive(Guid id)
        {
            try
            {
                var thisUser = _userRepository.Query().FirstOrDefault(u => u.Id == id);

                if (thisUser == null)
                    return StandardResponse<UserView>.Failed().AddStatusMessage(StandardResponseMessages.USER_NOT_FOUND);

                thisUser.IsActive = thisUser.IsActive ? false : true;
                var updatedUser = _userManager.UpdateAsync(thisUser).Result;

                var mapped = _mapper.Map<UserView>(updatedUser);

                return StandardResponse<UserView>.Ok(mapped);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StandardResponse<UserView>.Error(ex.Message);
            }
        }

        public async Task<StandardResponse<UserView>> AddTeamMember(TeamMemberModel model)
        {
            try
            {
                Guid UserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();

                var user = _userRepository.Query().FirstOrDefault(x => x.Id == UserId);

                var superAdminSettings = _controlSettingRepository.Query().FirstOrDefault(x => x.SuperAdminId == model.SuperAdminId);

                if (!superAdminSettings.AdminOBoarding && user.Role.ToLower() != "super admin") return StandardResponse<UserView>.NotFound("Team member onboarding is disabled for admins");

                var thisUser = _userRepository.Query().FirstOrDefault(u => u.Email == model.Email);

                if (thisUser != null)
                    return StandardResponse<UserView>.Failed().AddStatusMessage(StandardResponseMessages.USER_ALREADY_EXISTS);

                var registerModel = _mapper.Map<RegisterModel>(model);

                var result = await CreateUser(registerModel);

                if (!result.Status)
                    return StandardResponse<UserView>.Failed().AddStatusMessage(result.Message);

                var employeeInformation = _mapper.Map<EmployeeInformation>(model);

                employeeInformation.UserId = result.Data.Id;

                employeeInformation = _employeeInformationRepository.CreateAndReturn(employeeInformation);

                thisUser = _userRepository.Query().FirstOrDefault(u => u.Email == model.Email);

                thisUser.EmployeeInformationId = employeeInformation.Id;
                thisUser.IsActive = false;

                var thisUpdaterUserResult = _userManager.UpdateAsync(thisUser).Result;

                var contract = _mapper.Map<Contract>(model);

                contract.EmployeeInformationId = employeeInformation.Id;
                contract.StatusId = (int)Statuses.INACTIVE;

                contract = _contractRepository.CreateAndReturn(contract);

                thisUser = _userRepository.Query().Include(x => x.Client).Include(u => u.EmployeeInformation).ThenInclude(e => e.Contracts).FirstOrDefault(u => u.Email == model.Email);

                string clientName = "";

                //var getSupervisor = _userRepository.Query().Include(u => u.EmployeeInformation).ThenInclude(u => u.Supervisor).ThenInclude(u => u.Client).FirstOrDefault(u => u.Id == model.SupervisorId);

                //if (getSupervisor.Role.ToLower() == "internal supervisor")
                //    clientName = getSupervisor?.EmployeeInformation?.Supervisor?.Client?.OrganizationName;
                //else
                //    clientName = _userRepository.Query().Include(supervisor => supervisor.Client)
                //    .FirstOrDefault(user => user.Id == model.SupervisorId).Client.OrganizationName;

                var mapped = _mapper.Map<UserView>(thisUser);
                mapped.ClientName = clientName;
                mapped.ClientName = thisUser.Client.OrganizationName;

                await _notificationService.SendNotification(new NotificationModel { UserId = UserId, Title = "Onboarding", Type = "Notification", Message = "onboarding Successful" });

                return StandardResponse<UserView>.Ok(mapped);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StandardResponse<UserView>.Error(ex.Message);
            }
        }

        public async Task<StandardResponse<UserView>> ActivateTeamMember(Guid teamMemberId)
        {
            try
            {
                var thisUser = _userRepository.Query().FirstOrDefault(u => u.Id == teamMemberId);

                if (thisUser == null)
                    return StandardResponse<UserView>.Failed().AddStatusMessage(StandardResponseMessages.NOT_FOUND);

                var contract = _contractRepository.Query().FirstOrDefault(x => x.EmployeeInformationId == thisUser.EmployeeInformationId);
                contract.StatusId = (int)Statuses.ACTIVE;

                thisUser.IsActive = true;
                var updateResult = _userManager.UpdateAsync(thisUser).Result;
                thisUser = _userRepository.Query().Include(u => u.EmployeeInformation).ThenInclude(e => e.Contracts).FirstOrDefault(u => u.Id == thisUser.Id);

                return SendNewUserPasswordReset(new InitiateResetModel { Email = thisUser.Email }).Result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StandardResponse<UserView>.Error(ex.Message);
            }
        }

        public async Task<StandardResponse<UserView>> UpdateTeamMember(TeamMemberModel model)
        {
            try
            {
                var thisUser = _userRepository.Query().FirstOrDefault(u => u.Email == model.Email);
                var isInitialRole = thisUser.Role.ToLower() == model.Role.ToLower() ? true : false;
                var isUserActive = thisUser.IsActive;
                if (thisUser == null)
                    return StandardResponse<UserView>.Failed().AddStatusMessage(StandardResponseMessages.USER_NOT_FOUND);

                if (model.Role.ToUpper() == "TEAM MEMBER" && thisUser.Role.ToLower() == "internal supervisor")
                {
                    var teamMembers = _employeeInformationRepository.Query().Where(x => x.SupervisorId == thisUser.Id).Any();
                    if (teamMembers)
                        return StandardResponse<UserView>.Failed().AddStatusMessage("A Team Member Is Assigned To This Supervisor");
                }

                if (model.Role.ToLower() != thisUser.Role.ToLower())
                {
                    var userRole = await _userManager.RemoveFromRoleAsync(thisUser, thisUser.Role);
                    userRole = await _userManager.AddToRoleAsync(thisUser, model.Role);
                }

                thisUser.FirstName = model.FirstName;
                thisUser.LastName = model.LastName;
                thisUser.Email = model.Email;
                thisUser.Role = model.Role;
                thisUser.Address = model.Address;
                thisUser.PhoneNumber = model.PhoneNumber;
                thisUser.OrganizationName = model.OrganizationName;
                thisUser.OrganizationEmail = model.OrganizationEmail;
                thisUser.OrganizationPhone = model.OrganizationPhone;
                thisUser.OrganizationAddress = model.OrganizationAddress;
                thisUser.DateOfBirth = model.DateOfBirth;
                thisUser.IsActive = model.IsActive;

                var updateResult = _userManager.UpdateAsync(thisUser).Result;

                var employeeInformation = _employeeInformationRepository.Query().FirstOrDefault(e => e.Id == thisUser.EmployeeInformationId);

                employeeInformation.ClientId = model.ClientId;
                employeeInformation.SupervisorId = model.SupervisorId;
                employeeInformation.RatePerHour = model.RatePerHour;
                employeeInformation.JobTitle = model.JobTitle;
                employeeInformation.HoursPerDay = model.HoursPerDay;
                employeeInformation.InCorporationDocumentUrl = model.InCorporationDocumentUrl;
                employeeInformation.VoidCheckUrl = model.VoidCheckUrl;
                employeeInformation.InsuranceDocumentUrl = model.InsuranceDocumentUrl;
                employeeInformation.HstNumber = model.HstNumber;
                employeeInformation.PaymentPartnerId = model.PaymentPartnerId;
                employeeInformation.PaymentRate = model.PaymentRate;
                employeeInformation.Currency = model.Currency;
                employeeInformation.FixedAmount = model.FixedAmount;
                employeeInformation.ClientRate = model.ClientRate;
                employeeInformation.MonthlyPayoutRate = model.MonthlyPayoutRate;
                employeeInformation.PaymentFrequency = model.PaymentFrequency;
                employeeInformation.OnBoradingFee = model.onBordingFee;
                employeeInformation.IsEligibleForLeave = model.IsEligibleForLeave;
                employeeInformation.NumberOfDaysEligible = model.NumberOfDaysEligible;
                employeeInformation.NumberOfHoursEligible = model.NumberOfHoursEligible;
                employeeInformation.EmployeeType = model.EmployeeType;
                employeeInformation.InvoiceGenerationType = model.InvoiceGenerationType;

                employeeInformation = _employeeInformationRepository.Update(employeeInformation);

                thisUser = _userRepository.Query().Include(u => u.EmployeeInformation).ThenInclude(e => e.Contracts).FirstOrDefault(u => u.Id == thisUser.Id);



                var mapped = _mapper.Map<UserView>(thisUser);

                if (isInitialRole == false)
                {
                    List<KeyValuePair<string, string>> EmailParameters = new()
                    {
                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, thisUser.FirstName),
                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_ROLE, model.Role.ToUpper()),

                    };

                    var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.ROLE_CHANGE_FILENAME, EmailParameters);
                    var SendEmail = _emailHandler.SendEmail(thisUser.Email, "Role Updated", EmailTemplate, "");
                }

                if (isUserActive == true && model.IsActive == false)
                {
                    List<KeyValuePair<string, string>> EmailParameters = new()
                    {
                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, thisUser.FirstName),
                    };


                    var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.DEACTIVATE_USER_EMAIL_FILENAME, EmailParameters);
                    var SendEmail = _emailHandler.SendEmail(thisUser.Email, "Account Deactivation", EmailTemplate, "");

                    var getAdmins = _userRepository.Query().Where(x => x.Role.ToLower() == "super admin" && x.Id == model.SuperAdminId).ToList();
                    foreach (var admin in getAdmins)
                    {
                        await _notificationService.SendNotification(new NotificationModel { UserId = admin.Id, Title = "Account Deactivation", Type = "Notification", Message = "Account Deactivation Was succesful" });
                    }
                }

                return StandardResponse<UserView>.Ok(mapped);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StandardResponse<UserView>.Error(ex.Message);
            }
        }

        /// <summary>
        /// list supervisors for a particular client
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public async Task<StandardResponse<List<UserView>>> ListSupervisors(Guid clientId)
        {
            try
            {
                //var supervisors = _userRepository.Query().Include(u => u.EmployeeInformation).Where(u => u.ClientId == clientId && u.Role == "Supervisor" || u.EmployeeInformation.Supervisor.ClientId == clientId && u.Role == "Internal Supervisor").OrderByDescending(x => x.DateCreated).ToList();
                var supervisors = _userRepository.Query().Include(u => u.EmployeeInformation).Where(u => u.ClientId == clientId && u.Role.ToLower() == "supervisor" || u.EmployeeInformation.Supervisor.ClientId == clientId && u.Role == "Internal Supervisor").OrderByDescending(x => x.DateCreated).ToList();

                var mapped = _mapper.Map<List<UserView>>(supervisors);

                return StandardResponse<List<UserView>>.Ok(mapped);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StandardResponse<List<UserView>>.Error(ex.Message);
            }
        }

        public async Task<StandardResponse<PagedCollection<UserView>>> ListSupervisees(PagingOptions options, string search = null, Guid? supervisorId = null, DateFilter dateFilter = null)
        {
            try
            {
                Guid UserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();

                var loggenInUserId = supervisorId == null ? UserId : supervisorId.Value;

                var users = _userRepository.Query().Where(u => u.EmployeeInformation.SupervisorId == loggenInUserId).OrderByDescending(x => x.DateCreated);

                if (dateFilter.StartDate.HasValue)
                    users = users.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

                if (dateFilter.EndDate.HasValue)
                    users = users.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);

                if (!string.IsNullOrEmpty(search))
                {
                    users = users.Where(u => u.FirstName.Contains(search) || u.LastName.Contains(search) || (u.FirstName.ToLower() + " " + u.LastName.ToLower()).Contains(search.ToLower())
                    || u.Email.Contains(search)).OrderByDescending(x => x.DateCreated);
                }

                var paged = users.Skip(options.Offset.Value).Take(options.Limit.Value);

                var mapped = users.ProjectTo<UserView>(_configurationProvider);

                var pagedCollection = PagedCollection<UserView>.Create(Link.ToCollection(nameof(UserController.GetSupervisees)), mapped.ToArray(), users.Count(), options);

                return StandardResponse<PagedCollection<UserView>>.Ok(pagedCollection);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StandardResponse<PagedCollection<UserView>>.Error(ex.Message);
            }
        }
        public async Task<StandardResponse<PagedCollection<UserView>>> ListClientSupervisors(PagingOptions options, string search = null, Guid? clientId = null, DateFilter dateFilter = null)
        {
            try
            {
                Guid UserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();

                var loggenInUserId = clientId == null ? UserId : clientId.Value;

                var supervisors = _userRepository.Query().Include(supervisor => supervisor.Client).Include(supervisor => supervisor.EmployeeInformation).ThenInclude(supervisor => supervisor.Client).
                    Where(supervisor => supervisor.ClientId == loggenInUserId && supervisor.Role.ToLower() == "supervisor" || supervisor.EmployeeInformation.ClientId == loggenInUserId && supervisor.Role.ToLower() == "internal supervisor").OrderByDescending(x => x.DateCreated);

                if (dateFilter.StartDate.HasValue)
                    supervisors = supervisors.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

                if (dateFilter.EndDate.HasValue)
                    supervisors = supervisors.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);


                if (!string.IsNullOrEmpty(search))
                    supervisors = supervisors.Where(u => u.FirstName.ToLower().Contains(search.ToLower()) || u.LastName.ToLower().Contains(search.ToLower()) || (u.FirstName.ToLower() + " " + u.LastName.ToLower()).Contains(search.ToLower())
                    || u.Email.ToLower().Contains(search.ToLower())).OrderByDescending(x => x.DateCreated);

                var pagedResponse = supervisors.Skip(options.Offset.Value).Take(options.Limit.Value).AsQueryable();

                var mapped = supervisors.ProjectTo<UserView>(_configurationProvider).ToList();

                var pagedCollection = PagedCollection<UserView>.Create(Link.ToCollection(nameof(UserController.GetClientSupervisors)), mapped.ToArray(), supervisors.Count(), options);

                return StandardResponse<PagedCollection<UserView>>.Ok(pagedCollection);

            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<UserView>>.Error(e.Message);
            }
        }

        public async Task<StandardResponse<PagedCollection<UserView>>> ListClientTeamMembers(PagingOptions options, string search = null, Guid? clientId = null, DateFilter dateFilter = null)
        {
            try
            {
                Guid UserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();

                var loggenInUserId = clientId == null ? UserId : clientId.Value;

                var teamMembers = _userRepository.Query().Where(teams => teams.EmployeeInformation.Supervisor.ClientId == loggenInUserId && teams.Role.ToLower() == "team member" || teams.EmployeeInformation.Supervisor.ClientId == loggenInUserId && teams.Role.ToLower() == "internal admin" ||
                teams.EmployeeInformation.Supervisor.ClientId == loggenInUserId && teams.Role.ToLower() == "internal supervisor" || teams.EmployeeInformation.Supervisor.ClientId == loggenInUserId && teams.Role.ToLower() == "internal payroll manager").OrderByDescending(x => x.DateCreated);

                if (dateFilter.StartDate.HasValue)
                    teamMembers = teamMembers.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

                if (dateFilter.EndDate.HasValue)
                    teamMembers = teamMembers.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);

                if (!string.IsNullOrEmpty(search))
                    teamMembers = teamMembers.Where(u => u.FirstName.ToLower().Contains(search.ToLower()) || u.LastName.ToLower().Contains(search.ToLower()) || (u.FirstName.ToLower() + " " + u.LastName.ToLower()).Contains(search.ToLower())
                    || u.Email.ToLower().Contains(search.ToLower())).OrderByDescending(x => x.DateCreated);

                var mapped = teamMembers.ProjectTo<UserView>(_configurationProvider).ToList();

                var pagedCollection = PagedCollection<UserView>.Create(Link.ToCollection(nameof(UserController.GetClientTeamMembers)), mapped.ToArray(), teamMembers.Count(), options);

                return StandardResponse<PagedCollection<UserView>>.Ok(pagedCollection);

            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<UserView>>.Error(e.Message);
            }
        }

        public async Task<StandardResponse<PagedCollection<UserView>>> ListPaymentPartnerTeamMembers(PagingOptions options, string search = null, Guid? paymentPartnerId = null, DateFilter dateFilter = null)
        {
            try
            {
                Guid UserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();

                var loggenInUserId = paymentPartnerId == null ? UserId : paymentPartnerId.Value;

                var teamMembers = _userRepository.Query().Where(teams => teams.EmployeeInformation.PaymentPartnerId == loggenInUserId).OrderByDescending(x => x.DateCreated);

                if (dateFilter.StartDate.HasValue)
                    teamMembers = teamMembers.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

                if (dateFilter.EndDate.HasValue)
                    teamMembers = teamMembers.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);

                if (!string.IsNullOrEmpty(search))
                    teamMembers = teamMembers.Where(u => u.FirstName.ToLower().Contains(search.ToLower()) || u.LastName.ToLower().Contains(search.ToLower()) || (u.FirstName.ToLower() + " " + u.LastName.ToLower()).Contains(search.ToLower())
                    || u.Email.ToLower().Contains(search.ToLower())).OrderByDescending(x => x.DateCreated);

                var pagedResponse = teamMembers.Skip(options.Offset.Value).Take(options.Limit.Value).AsQueryable();

                var mapped = pagedResponse.ProjectTo<UserView>(_configurationProvider).ToList();

                var pagedCollection = PagedCollection<UserView>.Create(Link.ToCollection(nameof(UserController.GetPaymentPartnerTeamMembers)), mapped.ToArray(), teamMembers.Count(), options);

                return StandardResponse<PagedCollection<UserView>>.Ok(pagedCollection);

            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<UserView>>.Error(e.Message);
            }
        }

        public async Task<StandardResponse<PagedCollection<ShiftUsersListView>>> ListShiftUsers(PagingOptions options, Guid superAdminId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var shiftUsers = _userRepository.Query().Include(u => u.EmployeeInformation).Where(u => u.EmployeeInformation.EmployeeType.ToLower() == "shift" && u.SuperAdminId == superAdminId).OrderByDescending(x => x.DateCreated);

                var pagedResponse = shiftUsers.Skip(options.Offset.Value).Take(options.Limit.Value).AsQueryable().ToList();

                var users = new List<ShiftUsersListView>();

                foreach (var user in pagedResponse)
                {
                    var userDetails = _shiftService.GetUsersAndTotalHours(user, startDate, endDate);
                    users.Add(userDetails);
                };
                //var mapped = _mapper.Map<List<>>(shiftUsers);
                var pagedCollection = PagedCollection<ShiftUsersListView>.Create(Link.ToCollection(nameof(UserController.GetPaymentPartnerTeamMembers)), users.ToArray(), shiftUsers.Count(), options);

                return StandardResponse<PagedCollection<ShiftUsersListView>>.Ok(pagedCollection);
                //return StandardResponse<List<ShiftUsersListView>>.Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StandardResponse<PagedCollection<ShiftUsersListView>>.Error(ex.Message);
            }
        }

        public StandardResponse<byte[]> ExportUserRecord(UserRecordDownloadModel model, DateFilter dateFilter)
        {
            try
            {
                if (model.Record == RecordsToDownload.ClientSupervisors && model.ClientId == null || model.Record == RecordsToDownload.ClientTeamMembers && model.ClientId == null ||
                model.Record == RecordsToDownload.Supervisees && model.SupervisorId == null || model.Record == RecordsToDownload.PaymentPartnerTeamMembers && model.PaymentPartnerId == null)
                    return StandardResponse<byte[]>.Error("Please enter a client or supervisor identifier for these request");
                var users = _userRepository.Query().Include(x => x.EmployeeInformation).ThenInclude(x => x.Supervisor).Include(x => x.EmployeeInformation).ThenInclude(x => x.Client).
                    Where(x => x.DateCreated >= dateFilter.StartDate && x.DateCreated <= dateFilter.EndDate && x.SuperAdminId == model.SuperAdminId);
                switch (model.Record)
                {
                    case RecordsToDownload.AdminUsers:
                        users = users.Where(u => u.Role == "Admin" || u.Role == "Super Admin" || u.Role == "Payroll Manager").OrderByDescending(x => x.DateCreated);
                        break;
                    case RecordsToDownload.TeamMembers:
                        users = users.Where(u => u.Role.ToLower() == "team member").OrderByDescending(x => x.DateCreated);
                        break;
                    case RecordsToDownload.Supervisors:
                        users = users.Where(u => u.Role.ToLower() == "supervisor" || u.Role.ToLower() == "internal supervisor").OrderByDescending(x => x.DateCreated);
                        break;
                    case RecordsToDownload.Client:
                        users = users.Where(u => u.Role.ToLower() == "client").OrderByDescending(x => x.DateCreated);
                        break;
                    case RecordsToDownload.PaymentPartner:
                        users = users.Where(u => u.Role.ToLower() == "payment partner").OrderByDescending(x => x.DateCreated);
                        break;
                    case RecordsToDownload.PayrollManagers:
                        users = users.Where(u => u.Role.ToLower() == "payroll manager" || u.Role.ToLower() == "internal payroll manager").OrderByDescending(x => x.DateCreated);
                        break;
                    case RecordsToDownload.Admin:
                        users = users.Where(u => u.Role.ToLower() == "admin" || u.Role.ToLower() == "internal admin").OrderByDescending(x => x.DateCreated);
                        break;
                    case RecordsToDownload.ClientSupervisors:
                        users = users.Where(u => u.ClientId == model.ClientId && u.Role.ToLower() == "supervisor" || u.EmployeeInformation.ClientId == model.ClientId && u.Role.ToLower() == "internal supervisor").OrderByDescending(x => x.DateCreated);
                        break;
                    case RecordsToDownload.Supervisees:
                        users = users.Where(u => u.EmployeeInformation.SupervisorId == model.SupervisorId).OrderByDescending(x => x.DateCreated);
                        break;
                    case RecordsToDownload.ClientTeamMembers:
                        users = users.Where(u => u.EmployeeInformation.ClientId == model.ClientId).OrderByDescending(x => x.DateCreated);
                        break;
                    case RecordsToDownload.PaymentPartnerTeamMembers:
                        users = users.Where(u => u.EmployeeInformation.PaymentPartnerId == model.PaymentPartnerId).OrderByDescending(x => x.DateCreated);
                        break;
                    default:
                        break;
                }

                var userList = users.ToList();
                var workbook = _dataExport.ExportAdminUsers(model.Record, userList, model.rowHeaders);
                return StandardResponse<byte[]>.Ok(workbook);
            }
            catch (Exception e)
            {
                return StandardResponse<byte[]>.Error(e.Message);
            }


        }

        public StandardResponse<Enable2FAView> EnableTwoFactorAuthentication(bool is2FAEnabled)
        {
            try
            {
                var loggedInUserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();
                var user = _userRepository.Query().FirstOrDefault(u => u.Id == loggedInUserId);
                Enable2FAView response = new();
                if (is2FAEnabled)
                {
                    TwoFactorAuthenticator Authenticator = new TwoFactorAuthenticator();
                    var SetupResult = Authenticator.GenerateSetupCode("Providers Portal", $"{_appSettings.Secret}{user.TwoFactorCode}", 250, 250);
                    string QrCodeUrl = SetupResult.QrCodeSetupImageUrl;
                    string ManualCode = SetupResult.ManualEntryKey;

                    response = new Enable2FAView()
                    {
                        AlternativeKey = ManualCode,
                        QrCodeUrl = QrCodeUrl,
                        SecretKey = (Guid)user.TwoFactorCode,
                        Enable2FA = true
                    };
                }
                else
                {
                    if (user.TwoFactorEnabled == true)
                    {
                        user.TwoFactorEnabled = false;
                        var result = _userManager.UpdateAsync(user).Result;
                    }
                    response = new Enable2FAView()
                    {
                        AlternativeKey = null,
                        QrCodeUrl = null,
                        SecretKey = (Guid)user.TwoFactorCode,
                        Enable2FA = false
                    };
                }


                return StandardResponse<Enable2FAView>.Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EnableTwoFactorAuthentication");
                return StandardResponse<Enable2FAView>.Error("An Error Occurred");
            }
        }

        public StandardResponse<UserView> Complete2FASetup(string Code, Guid TwoFactorCode)
        {
            try
            {
                var validationResult = ValidateTwoFactorPIN(Code, TwoFactorCode);
                if (!validationResult)
                    return StandardResponse<UserView>.Error("Invalid Code");

                var user = _userRepository.Query().FirstOrDefault(u => u.TwoFactorCode == TwoFactorCode);
                if (user == null)
                    return StandardResponse<UserView>.Error("An Error Occurred");

                user.TwoFactorEnabled = true;

                var result = _userManager.UpdateAsync(user).Result;
                List<KeyValuePair<string, string>> EmailParameters = new()
                {
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, user.FirstName),
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_LOGO_URL, _appSettings.LOGO),
                };


                var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.TWO_FA_COMPLETED_FILENAME, EmailParameters);
                var SendEmail = _emailHandler.SendEmail(user.Email, Constants.TWO_FA_COMPLETED_SUBJECT, EmailTemplate, "");

                var userView = _mapper.Map<UserView>(user);
                return StandardResponse<UserView>.Ok(userView);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Complete2FASetup");
                return StandardResponse<UserView>.Error("An Error Occurred");
            }
        }

        public async Task<StandardResponse<List<UserCountByPayrollTypeView>>> GetUserCountByPayrolltypePerYear(int year)
        {
            try
            {
                int[] months = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
                var groupRecordsByYear = new List<UserCountByPayrollTypeView>();
                foreach (var month in months)
                {
                    var groupedTeammembers = _employeeInformationRepository.Query().Where(x => x.DateCreated.Year == year).ToList();
                    var onShoreTeams = groupedTeammembers.Count(x => x.PayRollTypeId == 1 && x.DateCreated.Month == month);
                    var offShoreTeams = groupedTeammembers.Count(x => x.PayRollTypeId == 2 && x.DateCreated.Month == month);
                    var record = new UserCountByPayrollTypeView
                    {
                        Month = ((Month)month).ToString(),
                        OnShore = onShoreTeams,
                        OffShore = offShoreTeams
                    };
                    groupRecordsByYear.Add(record);
                }
                return StandardResponse<List<UserCountByPayrollTypeView>>.Ok(groupRecordsByYear);
            }
            catch (Exception ex)
            {
                return StandardResponse<List<UserCountByPayrollTypeView>>.Error(ex.Message);
            }
        }

        public bool ValidateTwoFactorPIN(string code, Guid TwoFactorCode)
        {
            TwoFactorAuthenticator Authenticator = new TwoFactorAuthenticator();
            var result = Authenticator.ValidateTwoFactorPIN($"{_appSettings.Secret}{TwoFactorCode}", code);
            return result;
        }

        private async Task<StandardResponse<string>> ActivateClientOnCommandCenter(Guid? clientId)
        {
            //var headers = new Dictionary<string, string> { { "Authorization", "Basic " + "" } };
            if (!clientId.HasValue) return StandardResponse<string>.Failed();
            try
            {
                //HttpResponseMessage httpResponse = await _utilityMethods.MakeHttpRequest(null, _appSettings.CommandCenterUrl, $"api/Client/activate/{clientId}", HttpMethod.Post);
                var client = new RestClient(_appSettings.CommandCenterUrl);
                var request = new RestRequest($"api/Client/activate/{clientId}", Method.Post);
                var response = await client.ExecuteAsync<dynamic>(request);
                return StandardResponse<string>.Ok();
                //if (httpResponse != null && httpResponse.IsSuccessStatusCode)
                //{
                //    return StandardResponse<string>.Ok();
                //}
            }
            catch (Exception ex) { return StandardResponse<string>.Failed(ex.Message); }

            return StandardResponse<string>.Failed();
        }

        private async Task<StandardResponse<ClientSubscriptionResponseViewModel>> GetSubscriptionDetails(Guid? subscriptionId)
        {
            //var headers = new Dictionary<string, string> { { "Authorization", "Basic " + "" } };
            if (!subscriptionId.HasValue) return StandardResponse<ClientSubscriptionResponseViewModel>.Failed();
            try
            {
                HttpResponseMessage httpResponse = await _utilityMethods.MakeHttpRequest(null, _appSettings.CommandCenterUrl, $"api/Subscription/client-subscription/{subscriptionId}", HttpMethod.Get);
                if (httpResponse != null && httpResponse.IsSuccessStatusCode)
                {
                    dynamic stringContent = await httpResponse.Content.ReadAsStringAsync();
                    var responseData = JsonConvert.DeserializeObject<ClientSubscriptionResponseViewModel>(stringContent);
                    return StandardResponse<ClientSubscriptionResponseViewModel>.Ok(responseData);
                }

            }
            catch (Exception ex) { return StandardResponse<ClientSubscriptionResponseViewModel>.Failed(ex.Message); }

            return StandardResponse<ClientSubscriptionResponseViewModel>.Failed(null);
        }

        public async Task<StandardResponse<SubscriptionHistoryViewModel>> GetClientSubscriptionHistory(Guid userId, PagingOptions options, string search = null)
        {
            var user = _userRepository.Query().FirstOrDefault(x => x.Id == userId);

            if (user == null) return StandardResponse<SubscriptionHistoryViewModel>.NotFound("User not found");
            try
            {
                HttpResponseMessage httpResponse = await _utilityMethods.MakeHttpRequest(null, _appSettings.CommandCenterUrl, $"api/Subscription/client-subscription-history?clientId={user.CommandCenterClientId}&Offset={options.Offset}&Limit={options.Limit}&search={search}", HttpMethod.Get);
                if (httpResponse != null && httpResponse.IsSuccessStatusCode)
                {
                    dynamic stringContent = await httpResponse.Content.ReadAsStringAsync();
                    var responseData = JsonConvert.DeserializeObject<SubscriptionHistoryViewModel>(stringContent);
                    return StandardResponse<SubscriptionHistoryViewModel>.Ok(responseData);
                }
            }
            catch (Exception ex) { return StandardResponse<SubscriptionHistoryViewModel>.Failed(ex.Message); }

            return StandardResponse<SubscriptionHistoryViewModel>.Failed(null);
        }

        public async Task<StandardResponse<object>> CancelSubscription(Guid subscriptionId)
        {
            try
            {
                HttpResponseMessage httpResponse = await _utilityMethods.MakeHttpRequest(null, _appSettings.CommandCenterUrl, $"api/Subscription/deactivate-client-subscription/{subscriptionId}", HttpMethod.Get);
                if (httpResponse != null && httpResponse.IsSuccessStatusCode)
                {
                    dynamic stringContent = await httpResponse.Content.ReadAsStringAsync();
                    var responseData = JsonConvert.DeserializeObject<object>(stringContent);
                    return StandardResponse<object>.Ok(responseData);
                }
            }
            catch (Exception ex) { return StandardResponse<object>.Failed(ex.Message); }

            return StandardResponse<object>.Failed(null);
        }

        public async Task<StandardResponse<CardView>> CreateStripeCustomerCard(Guid userId, CreateCardResource resource)
        {
            try
            {
                var user = _userRepository.Query().FirstOrDefault(x => x.Id == userId);

                string customerId = user.StripeCustomerId;

                if (string.IsNullOrEmpty(user.StripeCustomerId))
                {
                    var result = await _stripeService.CreateCustomer(new CreateCustomerResource { Name = user.OrganizationName, Email = user.OrganizationName });
                    customerId = result.Message;

                    if (string.IsNullOrEmpty(customerId)) return StandardResponse<CardView>.Failed("Failed to create customer on stripe");
                }
                var card = await _stripeService.CreateCard(customerId, resource);
                return StandardResponse<CardView>.Ok(card);
            }
            catch (Exception ex) { return StandardResponse<CardView>.Failed(ex.Message); }
        }

        public async Task<StandardResponse<List<CardView>>> ListStripreCustomerCard(Guid userId)
        {
            try
            {
                var user = _userRepository.Query().FirstOrDefault(x => x.Id == userId);

                var cards = await _stripeService.GetClientCards(user.StripeCustomerId, 5);

                return StandardResponse<List<CardView>>.Ok(cards);
            }
            catch (Exception ex) { return StandardResponse<List<CardView>>.Failed(ex.Message); }
        }

        public async Task<StandardResponse<bool>> SetCardAsDefault(Guid userId, string cardId)
        {
            try
            {
                var user = _userRepository.Query().FirstOrDefault(x => x.Id == userId);

                var cards = await _stripeService.UpdateCustomerDefaultCard(user.StripeCustomerId, cardId);

                return StandardResponse<bool>.Ok(cards);
            }
            catch (Exception ex) { return StandardResponse<bool>.Failed(ex.Message); }
        }

        public async Task<StandardResponse<CustomerView>> UpdateStripeCustomer(Guid userId, CreateCustomerResource model)
        {
            try
            {
                var user = _userRepository.Query().FirstOrDefault(x => x.Id == userId);

                var customer = await _stripeService.UpdateCustomerDetail(user.StripeCustomerId, model.Name, model.Email);
                return StandardResponse<CustomerView>.Ok(customer);
            }
            catch (Exception ex) { return StandardResponse<CustomerView>.Failed(ex.Message); }
        }

        public async Task<StandardResponse<bool>> DeleteCard(Guid userId, string cardId)
        {
            try
            {
                var user = _userRepository.Query().FirstOrDefault(x => x.Id == userId);

                var cards = await _stripeService.DeleteCard(user.StripeCustomerId, cardId);

                return StandardResponse<bool>.Ok(cards);
            }
            catch (Exception ex) { return StandardResponse<bool>.Failed(ex.Message); }
        }

        public async Task<StandardResponse<bool>> MakePayment(Guid userId, CreateChargeResource model)
        {
            try
            {
                var user = _userRepository.Query().FirstOrDefault(x => x.Id == userId);

                var charge = await _stripeService.CreateCharge(user.StripeCustomerId, model);

                return StandardResponse<bool>.Ok(charge);
            }
            catch (Exception ex) { return StandardResponse<bool>.Failed(ex.Message); }
        }

        public async Task<StandardResponse<ClientSubscriptionResponseViewModel>> UpgradeSubscription(UpdateClientStripeSubscriptionModel model)
        {
            var user = _userRepository.Query().FirstOrDefault(x => x.Id == model.UserId);

            try
            {
                var request = new
                {
                    id = user.CommandCenterClientId,
                    subscriptionId = model.SubscriptionId,
                    totalAmount = model.TotalAmount
                };
                HttpResponseMessage httpResponse = await _utilityMethods.MakeHttpRequest(request, _appSettings.CommandCenterUrl, $"api/Subscription/upgrade-client-subscription", HttpMethod.Post);
                if (httpResponse != null && httpResponse.IsSuccessStatusCode)
                {
                    dynamic stringContent = await httpResponse.Content.ReadAsStringAsync();
                    var responseData = JsonConvert.DeserializeObject<ClientSubscriptionResponseViewModel>(stringContent);
                    return StandardResponse<ClientSubscriptionResponseViewModel>.Ok(responseData);
                }

            }
            catch (Exception ex) { return StandardResponse<ClientSubscriptionResponseViewModel>.Failed(ex.Message); }

            return StandardResponse<ClientSubscriptionResponseViewModel>.Failed(null);
        }

        public async Task<StandardResponse<bool>> PauseSubscription(Guid userId, int pauseDuration)
        {
            var user = _userRepository.Query().FirstOrDefault(x => x.Id == userId);

            try
            {
                HttpResponseMessage httpResponse = await _utilityMethods.MakeHttpRequest(null, _appSettings.CommandCenterUrl, $"api/Subscription/pause-subscription?subscriptionId={user.ClientSubscriptionId}&pauseDuration={pauseDuration}", HttpMethod.Post);
                if (httpResponse != null && httpResponse.IsSuccessStatusCode)
                {
                    dynamic stringContent = await httpResponse.Content.ReadAsStringAsync();
                    //var responseData = JsonConvert.DeserializeObject<object>(stringContent);
                    return StandardResponse<bool>.Ok(true);
                }

            }
            catch (Exception ex) { return StandardResponse<bool>.Failed(ex.Message); }

            return StandardResponse<bool>.Failed(null);
        }

        public async Task<StandardResponse<bool>> CancelSubscription(CancelSubscriptionModel model)
        {
            var user = _userRepository.Query().FirstOrDefault(x => x.Id == model.UserId);

            try
            {
                var request = new
                {
                    subscriptionId = user.ClientSubscriptionId,
                    reason = model.Reason
                };
                HttpResponseMessage httpResponse = await _utilityMethods.MakeHttpRequest(request, _appSettings.CommandCenterUrl, $"api/Subscription/cancel-subscription", HttpMethod.Post);
                if (httpResponse != null && httpResponse.IsSuccessStatusCode)
                {
                    dynamic stringContent = await httpResponse.Content.ReadAsStringAsync();
                    //var responseData = JsonConvert.DeserializeObject<status>(stringContent);
                    return StandardResponse<bool>.Ok(true);
                }

            }
            catch (Exception ex) { return StandardResponse<bool>.Failed(ex.Message); }

            return StandardResponse<bool>.Failed(null);
        }

        public async Task<StandardResponse<Cards>> GetUserCards(Guid userId)
        {
            var user = _userRepository.Query().FirstOrDefault(x => x.Id == userId);

            try
            {
                HttpResponseMessage httpResponse = await _utilityMethods.MakeHttpRequest(null, _appSettings.CommandCenterUrl, $"api/Subscription/user/cards?clientId={user.CommandCenterClientId}", HttpMethod.Get);
                if (httpResponse != null && httpResponse.IsSuccessStatusCode)
                {
                    dynamic stringContent = await httpResponse.Content.ReadAsStringAsync();
                    var responseData = JsonConvert.DeserializeObject<Cards>(stringContent);
                    return StandardResponse<Cards>.Ok(responseData);
                }

            }
            catch (Exception ex) { return StandardResponse<Cards>.Failed(ex.Message); }

            return StandardResponse<Cards>.Failed(null);
        }

        public async Task<StandardResponse<CommandCenterAddCardResponse>> AddNewCard(Guid userId)
        {
            var user = _userRepository.Query().FirstOrDefault(x => x.Id == userId);

            try
            {
                HttpResponseMessage httpResponse = await _utilityMethods.MakeHttpRequest(null, _appSettings.CommandCenterUrl, $"api/Subscription/add-new-card-new?clientId={user.CommandCenterClientId}", HttpMethod.Post);
                if (httpResponse != null && httpResponse.IsSuccessStatusCode)
                {
                    var stringContent = await httpResponse.Content.ReadAsStringAsync();
                    var responseData = JsonConvert.DeserializeObject<CommandCenterResponseModel>(stringContent);
                    return StandardResponse<CommandCenterAddCardResponse>.Ok(responseData.data);
                }

            }
            catch (Exception ex) { return StandardResponse<CommandCenterAddCardResponse>.Failed(ex.Message); }

            return StandardResponse<CommandCenterAddCardResponse>.Failed(null);
        }

        public async Task<StandardResponse<bool>> SetAsDefaulCard(Guid userId, string paymentMethod)
        {
            var user = _userRepository.Query().FirstOrDefault(x => x.Id == userId);

            try
            {
                HttpResponseMessage httpResponse = await _utilityMethods.MakeHttpRequest(null, _appSettings.CommandCenterUrl, $"api/Subscription/set-default-card?clientId={user.CommandCenterClientId}&paymentMethodId={paymentMethod}", HttpMethod.Post);
                if (httpResponse != null && httpResponse.IsSuccessStatusCode)
                {
                    return StandardResponse<bool>.Ok(true);
                }

            }
            catch (Exception ex) { return StandardResponse<bool>.Failed(ex.Message); }

            return StandardResponse<bool>.Failed(null);
        }

        public async Task<StandardResponse<bool>> UpdateUserCardDetails(Guid userId, UpdateCardDetailsModel model)
        {
            var user = _userRepository.Query().FirstOrDefault(x => x.Id == userId);

            try
            {
                var request = new
                {
                    paymentMethodId = model.PaymentMethodId,
                    name = model.Name,
                    email = model.Email
                };
                HttpResponseMessage httpResponse = await _utilityMethods.MakeHttpRequest(request, _appSettings.CommandCenterUrl, $"api/Subscription/update-card?clientId={user.CommandCenterClientId}", HttpMethod.Post);
                if (httpResponse != null && httpResponse.IsSuccessStatusCode)
                {
                    return StandardResponse<bool>.Ok(true);
                }

            }
            catch (Exception ex) { return StandardResponse<bool>.Failed(ex.Message); }

            return StandardResponse<bool>.Failed(null);
        }

        public async Task<StandardResponse<bool>> DeletePaymentCard(Guid userId, string paymentMethod)
        {
            var user = _userRepository.Query().FirstOrDefault(x => x.Id == userId);

            try
            {
                HttpResponseMessage httpResponse = await _utilityMethods.MakeHttpRequest(null, _appSettings.CommandCenterUrl, $"api/Subscription/delete-card?clientId={user.CommandCenterClientId}&paymentMethodId={paymentMethod}", HttpMethod.Post);
                if (httpResponse != null && httpResponse.IsSuccessStatusCode)
                {
                    return StandardResponse<bool>.Ok(true);
                }

            }
            catch (Exception ex) { return StandardResponse<bool>.Failed(ex.Message); }

            return StandardResponse<bool>.Failed(null);
        }

        public async Task<StandardResponse<UserView>> MicrosoftLogin(MicrosoftIdTokenDetailsModel model)
        {
            try
            {
                if (model == null) return StandardResponse<UserView>.Error("Invalid token");
                if (model.Aud.ToString() != _appSettings.AzureAd.ClientId) return StandardResponse<UserView>.Error("Invalid token");
                var thisUser = _userRepository.Query().FirstOrDefault(x => x.Email == model.PreferredUsername);

                if (thisUser == null) return StandardResponse<UserView>.Error("You do nt have access to this application. Please reach out to an admin to send you an invite");

                var Result = _userRepository.Authenticate(thisUser, true).Result;

                if (!Result.Succeeded)
                    return StandardResponse<UserView>.Failed().AddStatusMessage((Result.ErrorMessage ?? StandardResponseMessages.ERROR_OCCURRED));
                if (Result.LoggedInUser.TwoFactorCode == null)
                {
                    Result.LoggedInUser.TwoFactorCode = Guid.NewGuid();
                    var res = _userManager.UpdateAsync(Result.LoggedInUser).Result;
                }

                var mapped = _mapper.Map<UserView>(Result.LoggedInUser);
                var rroles = _userManager.GetRolesAsync(Result.LoggedInUser).Result;
                mapped.Role = _userManager.GetRolesAsync(Result.LoggedInUser).Result.FirstOrDefault();
                var employeeInformation = _employeeInformationRepository.Query().Include(user => user.PayrollType).FirstOrDefault(empInfo => empInfo.Id == Result.LoggedInUser.EmployeeInformationId);
                mapped.PayrollType = employeeInformation?.PayrollType.Name;
                mapped.NumberOfDaysEligible = employeeInformation?.NumberOfDaysEligible;
                mapped.NumberOfLeaveDaysTaken = employeeInformation?.NumberOfEligibleLeaveDaysTaken;
                mapped.NumberOfHoursEligible = employeeInformation?.NumberOfHoursEligible;
                mapped.EmployeeType = employeeInformation?.EmployeeType;

                var user = _userRepository.Query().Include(x => x.EmployeeInformation).Include(x => x.SuperAdmin).Where(x => x.Id == mapped.Id).FirstOrDefault();

                if (user.Role.ToLower() == "super admin")
                {
                    mapped.SubscriptiobDetails = GetSubscriptionDetails(user.ClientSubscriptionId).Result.Data;
                    mapped.SuperAdminId = user.Id;
                }

                else
                {
                    mapped.SubscriptiobDetails = GetSubscriptionDetails(user.SuperAdmin.ClientSubscriptionId).Result.Data;
                }

                if (user.Role.ToLower() == "admin")
                {
                    var controlSetting = _controlSettingRepository.Query().FirstOrDefault(x => x.SuperAdminId == user.SuperAdminId);
                    mapped.ControlSettingView = _mapper.Map<ControlSettingView>(controlSetting);
                }

                if (employeeInformation != null)
                {
                    var getNumberOfDaysEligible = _leaveService.GetEligibleLeaveDays(employeeInformation.Id);

                    mapped.NumberOfDaysEligible = getNumberOfDaysEligible - employeeInformation?.NumberOfEligibleLeaveDaysTaken;
                    mapped.ClientId = employeeInformation?.ClientId;
                }

                return StandardResponse<UserView>.Ok(mapped);
            }
            catch (Exception ex)
            {
                return StandardResponse<UserView>.Error(ex.Message);
            }
        }
    }
}
