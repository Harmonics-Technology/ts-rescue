using System.Threading.Tasks;
using System;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Utilities;
using TimesheetBE.Utilities.Abstrctions;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using TimesheetBE.Utilities.Constants;
using TimesheetBE.Services.Interfaces;
using TimesheetBE.Repositories.Interfaces;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Models.IdentityModels;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace TimesheetBE.Services
{
    public class UtilityService : IUtilityService
    {
        private readonly Globals _appSettings;
        private readonly IEmailHandler _emailHandler;
        private readonly IProjectManagementSettingRepository _projectManagementSettingRepository;
        private readonly IUserRepository _userRepository;
        private readonly IClientSubscriptionDetailRepository _clientSubscriptionDetailRepository;
        private readonly UserManager<User> _userManager;
        private readonly IClientSubscriptionDetailRepository _subscriptionDetailRepository;
        private readonly ILogger<UtilityService> _logger;
        private readonly IMapper _mapper;
        public UtilityService(IEmailHandler emailHandler, IOptions<Globals> appSettings, IUserRepository userRepository, 
            IClientSubscriptionDetailRepository clientSubscriptionDetailRepository, IProjectManagementSettingRepository projectManagementSettingRepository, 
            UserManager<User> userManager, IClientSubscriptionDetailRepository subscriptionDetailRepository, ILogger<UtilityService> logger, IMapper mapper)
        {
            _emailHandler = emailHandler;
            _appSettings = appSettings.Value;
            _userRepository = userRepository;
            _clientSubscriptionDetailRepository = clientSubscriptionDetailRepository;
            _projectManagementSettingRepository = projectManagementSettingRepository;
            _userManager = userManager;
            _subscriptionDetailRepository = subscriptionDetailRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<StandardResponse<bool>> SendContactMessage(ContactMessageModel model)
        {
            try
            {
                List<KeyValuePair<string, string>> EmailParameters = new()
                                {
                                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_LOGO_URL, _appSettings.LOGO),
                                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_CONTACTSUBJECT, model.Subject),
                                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_CONTACTFULLNAME, model.FullName),
                                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_CONTACTEMAIL, model.Email),
                                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_CONTACTMESSAGE, model.Message),
                                };

                var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.CONTACT_US_FILENAME, EmailParameters);
                var SendEmail = _emailHandler.SendEmail(_appSettings.ContactUsEmail, "New Message", EmailTemplate, "");

                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception e)
            {
                return StandardResponse<bool>.Error("Error creating subtask");
            }
        }

        public async Task<StandardResponse<UserView>> UpdateClientSubscriptionMigration(UpdateClientSubscriptionModel model)
        {
            try
            {
                var thisUser = _userRepository.ListUsers().Result.Users.FirstOrDefault(u => u.CommandCenterClientId == model.CommandCenterClientId);

                var projectManagementSetting = _projectManagementSettingRepository.CreateAndReturn(new ProjectManagementSetting
                {
                    SuperAdminId = thisUser.Id,
                    AdminProjectCreation = true,
                    PMProjectCreation = true,
                    AllProjectCreation = false,
                    AdminTaskCreation = true,
                    AssignedPMTaskCreation = true,
                    ProjectMembersTaskCreation = true,
                    AdminTaskViewing = true,
                    AssignedPMTaskViewing = true,
                    ProjectMembersTaskViewing = true,
                    PMTaskEditing = true,
                    TaskMembersTaskEditing = true,
                    ProjectMembersTaskEditing = false,
                    ProjectMembersTimesheetVisibility = true,
                    TaskMembersTimesheetVisibility = false
                });

                thisUser.SuperAdminId = thisUser.Id;
                thisUser.ProjectManagementSettingId = projectManagementSetting.Id;

                var updateResult = _userManager.UpdateAsync(thisUser).Result;

                var users = _userRepository.ListUsers().Result.Users.Where(u => u.SuperAdminId == thisUser.SuperAdminId).ToList();

                var subscriptionDetail = _subscriptionDetailRepository.Query().FirstOrDefault(x => x.SuperAdminId == thisUser.Id &&
                x.SubscriptionId == model.ClientSubscriptionId);

                if (subscriptionDetail == null)
                {
                    var newSubscription = new ClientSubscriptionDetail
                    {
                        SuperAdminId = thisUser.Id,
                        SubscriptionId = model.ClientSubscriptionId,
                        NoOfLicensePurchased = users.Count() + 100,
                        SubscriptionStatus = model.SubscriptionStatus,
                        SubscriptionType = model.SubscriptionType,
                        AnnualBilling = model.AnnualBilling,
                        TotalAmount = model.TotalAmount,
                        StartDate = model.StartDate,
                        EndDate = model.EndDate,
                        SubscriptionPrice = model.SubscriptionPrice,
                        NoOfLicenceUsed = users.Count()
                    };

                    _subscriptionDetailRepository.CreateAndReturn(newSubscription);

                    //if (thisUser.ClientSubscriptionId == null)
                    //{
                    //    thisUser.ClientSubscriptionId = model.ClientSubscriptionId;

                    //    var up = _userManager.UpdateAsync(thisUser).Result;
                    //}

                    foreach (var user in users)
                    {
                        user.ClientSubscriptionId = model.ClientSubscriptionId;

                        var up = _userManager.UpdateAsync(user).Result;
                    }
                }
                //else
                //{
                //    subscriptionDetail.NoOfLicensePurchased = model.NoOfLicense;
                //    subscriptionDetail.SubscriptionId = model.ClientSubscriptionId;
                //    subscriptionDetail.SubscriptionStatus = model.SubscriptionStatus;
                //    subscriptionDetail.SubscriptionType = model.SubscriptionType;
                //    subscriptionDetail.AnnualBilling = model.AnnualBilling;
                //    subscriptionDetail.TotalAmount = model.TotalAmount;
                //    subscriptionDetail.StartDate = model.StartDate;
                //    subscriptionDetail.EndDate = model.EndDate;
                //    subscriptionDetail.SubscriptionPrice = model.SubscriptionPrice;
                //    //thisUser.ClientSubscriptionId = model.ClientSubscriptionId;
                //    //thisUser.ClientSubscriptionStatus = model.SubscriptionStatus;

                //    _subscriptionDetailRepository.Update(subscriptionDetail);
                //}

                return StandardResponse<UserView>.Ok(_mapper.Map<UserView>(thisUser));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StandardResponse<UserView>.Failed();
            }
        }

        public async Task<StandardResponse<bool>> AddProjectanaagementSettingForExistingSuperAdmin(Guid superAdminId)
        {
            try
            {
                var superAdmin = _userRepository.Query().FirstOrDefault(x => x.Id == superAdminId);

                if (superAdmin == null) return StandardResponse<bool>.Failed("user not found");

                var projectManagementSetting = _projectManagementSettingRepository.CreateAndReturn(new ProjectManagementSetting
                {
                    SuperAdminId = superAdmin.Id,
                    AdminProjectCreation = true,
                    PMProjectCreation = true,
                    AllProjectCreation = false,
                    AdminTaskCreation = true,
                    AssignedPMTaskCreation = true,
                    ProjectMembersTaskCreation = true,
                    AdminTaskViewing = true,
                    AssignedPMTaskViewing = true,
                    ProjectMembersTaskViewing = true,
                    PMTaskEditing = true,
                    TaskMembersTaskEditing = true,
                    ProjectMembersTaskEditing = false,
                    ProjectMembersTimesheetVisibility = true,
                    TaskMembersTimesheetVisibility = false
                });

                return StandardResponse<bool>.Ok("success");
            }
            catch(Exception e)
            {
                return StandardResponse<bool>.Error("An error occured");
            }
        }
    }
}
