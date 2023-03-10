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

        public UserService(UserManager<User> userManager, SignInManager<User> signInManager, IMapper mapper, IUserRepository userRepository,
            IOptions<Globals> appSettings, IHttpContextAccessor httpContextAccessor, ICodeProvider codeProvider, IEmailHandler emailHandler,
            IConfigurationProvider configuration, RoleManager<Role> roleManager, ILogger<UserService> logger, IEmployeeInformationRepository employeeInformationRepository,
            IContractRepository contractRepository, IConfigurationProvider configurationProvider, IUtilityMethods utilityMethods, INotificationService notificationService)
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
        }

        public async Task<StandardResponse<UserView>> CreateUser(RegisterModel model)
        {
            try
            {
                var ExistingUser = _userManager.FindByEmailAsync(model.Email).Result;
                model.Password = "genericpassword";


                if (ExistingUser != null)
                    return StandardResponse<UserView>.Failed(StandardResponseMessages.USER_ALREADY_EXISTS, HttpStatusCode.OK).AddStatusMessage(StandardResponseMessages.USER_ALREADY_EXISTS);

                var thisUser = _mapper.Map<User>(model);
                thisUser.EmployeeInformationId = null;

                var roleExists = AscertainRoleExists(model.Role);

                var Result = _userRepository.CreateUser(thisUser).Result;

                if (!Result.Succeeded)
                    return StandardResponse<UserView>.Error(Result.ErrorMessage);

                var result = _userManager.AddToRoleAsync(Result.CreatedUser, model.Role).Result.Succeeded;

                var createdUser = _userManager.FindByEmailAsync(model.Email).Result;

                createdUser.Role = model.Role;
                createdUser.IsActive = false;

                var updateResult = _userManager.UpdateAsync(createdUser).Result;
                if(model.Role.ToLower() == "team member")
                {
                    //get all admins and superadmins emails
                    var getAdmins = _userRepository.Query().Where(x => x.Role.ToLower() == "super admin" || x.Role.ToLower() == "admin").ToList();
                    var adminEmails = new List<string>();
                    foreach (var admin in getAdmins)
                    {
                        adminEmails.Add(admin.Email);
                    }
                    return InitiateTeamMemberActivation(new InitiateTeamMemberActivationModel { AdminEmails = adminEmails, Email = createdUser.Email }).Result;

                }
                return InitiateNewUserPasswordReset(new InitiateResetModel { Email = createdUser.Email }).Result;
            }
            catch (Exception ex)
            {
                return StandardResponse<UserView>.Failed(ex.Message);
            }
        }

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
                ConfirmationLink = $"{_appSettings.FrontEndBaseUrl}{_appSettings.CompletePasswordResetUrl}{PasswordResetCode.CodeString}";

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
                ActivationLink = $"{_appSettings.FrontEndBaseUrl}{_appSettings.ActivateTeamMemberUrl}{ThisUser.Id}";

                foreach(var email in model.AdminEmails)
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
                    return StandardResponse<UserView>.Failed().AddStatusMessage((Result.ErrorMessage ?? StandardResponseMessages.ERROR_OCCURRED));

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

            if (!User.IsActive)
                return StandardResponse<UserView>.Failed().AddStatusMessage("Your account has been deactivated please contact admin");

            User = _mapper.Map<LoginModel, User>(userToLogin);

            var Result = _userRepository.Authenticate(User).Result;

            if (!Result.Succeeded)
                return StandardResponse<UserView>.Failed().AddStatusMessage((Result.ErrorMessage ?? StandardResponseMessages.ERROR_OCCURRED));

            var mapped = _mapper.Map<UserView>(Result.LoggedInUser);
            var rroles = _userManager.GetRolesAsync(Result.LoggedInUser).Result;
            mapped.Role = _userManager.GetRolesAsync(Result.LoggedInUser).Result.FirstOrDefault();
            var employeeInformation = _employeeInformationRepository.Query().Include(user => user.PayrollType).FirstOrDefault(empInfo => empInfo.Id == Result.LoggedInUser.EmployeeInformationId);
            mapped.PayrollType = employeeInformation?.PayrollType.Name;

            return StandardResponse<UserView>.Ok(mapped);
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
                ConfirmationLink = $"{_appSettings.FrontEndBaseUrl}{_appSettings.CompletePasswordResetUrl}{PasswordResetCode.CodeString}";

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
            Code ThisCode = _codeProvider.GetByCodeString(payload.Code);

            var ThisUser = _userManager.FindByIdAsync(ThisCode.UserId.ToString()).Result;

            if (ThisUser == null)
                return StandardResponse<UserView>.Failed().AddStatusMessage(StandardResponseMessages.USER_NOT_FOUND);

            if (ThisCode.IsExpired || ThisCode.ExpiryDate < DateTime.Now || ThisCode.Key != Constants.PASSWORD_RESET_CODE)
                return StandardResponse<UserView>.Failed().AddStatusMessage(StandardResponseMessages.PASSWORD_RESET_FAILED);

            var Result = _userManager.ResetPasswordAsync(ThisUser, ThisCode.Token, payload.NewPassword).Result;

            if (!Result.Succeeded)
                return StandardResponse<UserView>.Failed().AddStatusMessage(StandardResponseMessages.ERROR_OCCURRED);

            ThisUser.EmailConfirmed = true;
            var updateResult = _userManager.UpdateAsync(ThisUser);

            return StandardResponse<UserView>.Ok().AddStatusMessage(StandardResponseMessages.PASSWORD_RESET_COMPLETE);
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

        public async Task<StandardResponse<UserView>> AdminUpdateUser(UpdateUserModel model)
        {
            try
            {
                var thisUser = _userRepository.ListUsers().Result.Users.FirstOrDefault(u => u.Id == model.Id);
                if (!string.IsNullOrEmpty(model.PhoneNumber))
                {
                    thisUser.PhoneNumber = model.PhoneNumber;
                }
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

        public async Task<StandardResponse<PagedCollection<UserView>>> ListUsers(string role, PagingOptions options, string search = null, DateFilter dateFilter = null)
        {
            try
            {
                var users = _userRepository.Query();

                if (role.ToLower() == "admins")
                    users = users.Where(u => u.Role == "Admin" || u.Role == "Super Admin" || u.Role == "Payroll Manager").OrderByDescending(x => x.DateCreated);
                else if(role.ToLower() == "team member")
                    users = users.Where(u => u.Role.ToLower() == "team member").OrderByDescending(x => x.DateCreated);
                else if(role.ToLower() == "supervisor")
                    users = users.Where(u => u.Role.ToLower() == "supervisor" || u.Role.ToLower() == "internal supervisor").OrderByDescending(x => x.DateCreated);
                else if (role.ToLower() == "payroll manager")
                    users = users.Where(u => u.Role.ToLower() == "payroll manager" || u.Role.ToLower() == "internal payroll manager").OrderByDescending(x => x.DateCreated);
                else if (role.ToLower() == "admin")
                    users = users.Where(u => u.Role.ToLower() == "admin" || u.Role.ToLower() == "internal admin").OrderByDescending(x => x.DateCreated);
                else
                    users = users.Where(u => u.Role.ToLower() == role.ToLower()).OrderByDescending(x => x.DateCreated); ;

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
                .Include(x => x.EmployeeInformation).ThenInclude(x => x.Supervisor).ThenInclude(x => x.EmployeeInformation).ThenInclude(x => x.Supervisor).ThenInclude(x => x.Client)
                .Include(x => x.EmployeeInformation).ThenInclude(x => x.PaymentPartner)
                .Include(x => x.EmployeeInformation).ThenInclude(x => x.PayrollType)
                .Include(x => x.EmployeeInformation).ThenInclude(x => x.PayrollGroup)
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

                thisUser = _userRepository.Query().Include(u => u.EmployeeInformation).ThenInclude(e => e.Contracts).FirstOrDefault(u => u.Email == model.Email);

                string clientName = "";

                var getSupervisor = _userRepository.Query().Include(u => u.EmployeeInformation).ThenInclude(u => u.Supervisor).ThenInclude(u => u.Client).FirstOrDefault(u => u.Id == model.SupervisorId);

                if (getSupervisor.Role.ToLower() == "internal supervisor")
                    clientName = getSupervisor?.EmployeeInformation?.Supervisor?.Client?.OrganizationName;
                else
                    clientName = _userRepository.Query().Include(supervisor => supervisor.Client)
                    .FirstOrDefault(user => user.Id == model.SupervisorId).Client.OrganizationName;

                var mapped = _mapper.Map<UserView>(thisUser);
                mapped.ClientName = clientName;

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

                return InitiateNewUserPasswordReset(new InitiateResetModel { Email = thisUser.Email }).Result;
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
                    var supervisor = _userRepository.Query().FirstOrDefault(x => x.Id == thisUser.Id);
                    if (supervisor.Supervisees.Count() > 0)
                        return StandardResponse<UserView>.Failed().AddStatusMessage("A Team Member Is Assigned To This Supervisor");
                }

                if (model.Role.ToLower() != thisUser.Role.ToLower())
                {
                    var userRole = await _userManager.RemoveFromRoleAsync(thisUser, thisUser.Role);
                    userRole = await _userManager.AddToRoleAsync(thisUser, model.Role);
                }

                //if (model.Role.ToUpper() == "INTERNAL SUPERVISOR")
                //{
                //    var userRole = await _userManager.RemoveFromRoleAsync(thisUser, "Team Member");
                //    userRole = await _userManager.AddToRoleAsync(thisUser, "Internal Supervisor");
                //}

                //if (model.Role.ToUpper() == "INTERNAL ADMIN")
                //{
                //    var userRole = await _userManager.RemoveFromRoleAsync(thisUser, "Team Member");
                //    userRole = await _userManager.AddToRoleAsync(thisUser, "Internal Admin");
                //}

                //if (model.Role.ToUpper() == "INTERNAL PAYROLL MANAGER")
                //{
                //    var userRole = await _userManager.RemoveFromRoleAsync(thisUser, "Team Member");
                //    userRole = await _userManager.AddToRoleAsync(thisUser, "Internal Payroll Manager");
                //}

                
                //{
                //    if (thisUser.Role.ToLower() == "internal supervisor")
                //    {
                //        var supervisor = _userRepository.Query().FirstOrDefault(x => x.Id == thisUser.Id);
                //        if (supervisor.Supervisees.Count() > 0)
                //            return StandardResponse<UserView>.Failed().AddStatusMessage("A Team Member Is Assigned To This Supervisor");
                //    }
                //    var userRole = await _userManager.RemoveFromRoleAsync(thisUser, model.Role);
                //    userRole = await _userManager.AddToRoleAsync(thisUser, "Team Member");
                //}

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

                //employeeInformation.ClientId = model.ClientId;
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
                employeeInformation.PayrollGroupId = model.PayrollGroupId;

                employeeInformation = _employeeInformationRepository.Update(employeeInformation);

                thisUser = _userRepository.Query().Include(u => u.EmployeeInformation).ThenInclude(e => e.Contracts).FirstOrDefault(u => u.Id == thisUser.Id);



                var mapped = _mapper.Map<UserView>(thisUser);

                if(isInitialRole == false)
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

                    var getAdmins = _userRepository.Query().Where(x => x.Role.ToLower() == "super admin").ToList();
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
                var supervisors = _userRepository.Query().Include(u => u.EmployeeInformation).Where(u => u.ClientId == clientId && u.Role == "Supervisor" || u.EmployeeInformation.Supervisor.ClientId == clientId && u.Role == "Internal Supervisor").OrderByDescending(x => x.DateCreated).ToList();

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
                    Where(supervisor => supervisor.ClientId == loggenInUserId && supervisor.Role == "Supervisor" || supervisor.EmployeeInformation.Supervisor.ClientId == loggenInUserId && supervisor.Role == "Internal Supervisor").OrderByDescending(x => x.DateCreated);

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

                var teamMembers = _userRepository.Query().Where(teams => teams.EmployeeInformation.Supervisor.ClientId == loggenInUserId && teams.Role.ToLower() == "team member" || teams.Role.ToLower() == "internal admin" || teams.Role.ToLower() == "internal supervisor").OrderByDescending(x => x.DateCreated);

                if (dateFilter.StartDate.HasValue)
                    teamMembers = teamMembers.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

                if (dateFilter.EndDate.HasValue)
                    teamMembers = teamMembers.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);

                if (!string.IsNullOrEmpty(search))
                    teamMembers = teamMembers.Where(u => u.FirstName.ToLower().Contains(search.ToLower()) || u.LastName.ToLower().Contains(search.ToLower()) || (u.FirstName.ToLower() + " " + u.LastName.ToLower()).Contains(search.ToLower())
                    || u.Email.ToLower().Contains(search.ToLower())).OrderByDescending(x => x.DateCreated);

                var muSup = teamMembers.ToList();

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
    }
}
