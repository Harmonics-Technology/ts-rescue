using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TimesheetBE.Controllers;
using TimesheetBE.Models;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Models.IdentityModels;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Repositories.Interfaces;
using TimesheetBE.Services.Interfaces;
using TimesheetBE.Utilities;
using TimesheetBE.Utilities.Abstrctions;
using TimesheetBE.Utilities.Constants;
using TimesheetBE.Utilities.Extentions;

namespace TimesheetBE.Services
{
    public class ContractService : IContractService
    {
        private readonly IContractRepository _contractRepository;
        private readonly IMapper _mapper;
        private readonly IEmployeeInformationRepository _employeeInformationRepository;
        private readonly IConfigurationProvider _configuration;
        private readonly ICustomLogger<ContractService> _customLogger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserRepository _userRepository;
        private readonly IControlSettingRepository _controlSettingRepository;
        private readonly Globals _appSettings;
        private readonly IEmailHandler _emailHandler;

        public ContractService(IContractRepository contractRepository, IMapper mapper, IEmployeeInformationRepository employeeInformationRepository, 
            IConfigurationProvider configuration, ICustomLogger<ContractService> customLogger, IHttpContextAccessor httpContextAccessor, IUserRepository  userRepository, IControlSettingRepository controlSettingRepository)
        {
            _contractRepository = contractRepository;
            _mapper = mapper;
            _employeeInformationRepository = employeeInformationRepository;
            _configuration = configuration;
            _customLogger = customLogger;
            _httpContextAccessor = httpContextAccessor;
            _userRepository = userRepository;
            _controlSettingRepository = controlSettingRepository;
        }

        public async Task<StandardResponse<ContractView>> CreateContract(ContractModel model)
        {
            try
            {
                var employeeInformation = _employeeInformationRepository.Query().FirstOrDefault(e => e.UserId == model.UserId);

                if (employeeInformation == null)
                    return _customLogger.Error<ContractView>(_customLogger.GetMethodName(), new System.Exception("Employee Information not found"));

                var contract = _mapper.Map<Contract>(model);
                contract.EmployeeInformationId = employeeInformation.Id;
                contract.StatusId = (int)Statuses.ACTIVE;
                contract = _contractRepository.CreateAndReturn(contract);

                var contractView = _mapper.Map<ContractView>(contract);
                return StandardResponse<ContractView>.Ok(contractView);
            }
            catch (System.Exception ex)
            {
                return _customLogger.Error<ContractView>(_customLogger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<ContractView>> GetContract(Guid id)
        {
            try
            {
                var contract = _contractRepository.Query().Include(c => c.EmployeeInformation).ThenInclude(e => e.User).FirstOrDefault(c => c.Id == id);

                if (contract == null)
                    return _customLogger.Error<ContractView>(_customLogger.GetMethodName(), new System.Exception("Contract not found"));

                var contractView = _mapper.Map<ContractView>(contract);
                return StandardResponse<ContractView>.Ok(contractView);
            }
            catch (System.Exception ex)
            {
                return _customLogger.Error<ContractView>(_customLogger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<ContractView>> UpdateContract(ContractModel model)
        {
            try
            {
                Guid UserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();

                var user = _userRepository.Query().FirstOrDefault(x => x.Id == UserId);

                var teamMember = _userRepository.Query().FirstOrDefault(x => x.Id == model.UserId);

                if(teamMember == null) return _customLogger.Error<ContractView>(_customLogger.GetMethodName(), new System.Exception("Team member not found"));

                User superAdmin = null;

                if(user.Role.ToLower() == "super admin")
                {
                    superAdmin = _userRepository.Query().FirstOrDefault(x => x.Id == user.Id);
                }
                else
                {
                    superAdmin = _userRepository.Query().FirstOrDefault(x => x.Id == user.SuperAdminId);

                    var superAdminSettings = _controlSettingRepository.Query().FirstOrDefault(x => x.SuperAdminId == user.SuperAdminId);

                    if (!superAdminSettings.AdminLeaveManagement) return StandardResponse<ContractView>.Failed("Contract management is disabled for admins");
                }

                var contract = _contractRepository.Query().FirstOrDefault(c => c.Id == model.Id);

                if (contract == null)
                    return _customLogger.Error<ContractView>(_customLogger.GetMethodName(), new System.Exception("Contract not found"));

                //DateTime contractEndDate = contract.EndDate.Date;

                contract.Title = model.Title;
                contract.StartDate = model.StartDate;
                contract.EndDate = model.EndDate;
                contract.Document = model.Document;
                contract.StatusId = model.EndDate.Date > DateTime.Now.Date ? (int)Statuses.ACTIVE : contract.StatusId;

                contract = _contractRepository.Update(contract);

                //List<KeyValuePair<string, string>> EmailParameters = new()
                //        {
                //            new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, superAdmin.FirstName),
                //            new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_COWORKER, teamMember.FullName),
                //            new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_URL, $"{Globals.FrontEndBaseUrl}SuperAdmin/contracts")
                //        };

                //var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.CONTRACT_UPDATE_FILENAME, EmailParameters);
                //var SendEmail = _emailHandler.SendEmail(superAdmin.Email, "Contract Information Update !!!", EmailTemplate, "");

                var contractView = _mapper.Map<ContractView>(contract);
                return StandardResponse<ContractView>.Ok(contractView);
            }
            catch (System.Exception ex)
            {
                return _customLogger.Error<ContractView>(_customLogger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<ContractView>> TerminateContract(Guid id)
        {
            try
            {
                Guid UserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();

                var user = _userRepository.Query().FirstOrDefault(x => x.Id == UserId);

                if (user.Role.ToLower() != "super admin")
                {
                    var superAdminSettings = _controlSettingRepository.Query().FirstOrDefault(x => x.SuperAdminId == user.SuperAdminId);

                    if (!superAdminSettings.AdminLeaveManagement) return StandardResponse<ContractView>.Failed("Contract management is disabled for admins");
                }
                var contract = _contractRepository.Query().FirstOrDefault(c => c.Id == id);

                if (contract == null)
                    return _customLogger.Error<ContractView>(_customLogger.GetMethodName(), new System.Exception("Contract not found"));

                contract.StatusId = (int)Statuses.TERMINATED;
                contract = _contractRepository.Update(contract);

                var contractView = _mapper.Map<ContractView>(contract);
                return StandardResponse<ContractView>.Ok(contractView);
            }
            catch (System.Exception ex)
            {
                return _customLogger.Error<ContractView>(_customLogger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<PagedCollection<ContractView>>> ListContracts(PagingOptions options, Guid superAdminId, string search = null, DateFilter dateFilter = null)
        {
            try
            {
                var contracts = _contractRepository.Query().Include(contract => contract.EmployeeInformation).ThenInclude(e => e.User).Where(x => x.EmployeeInformation.User.SuperAdminId == superAdminId).OrderByDescending(u => u.DateCreated).AsQueryable();

                if (dateFilter.StartDate.HasValue)
                    contracts = contracts.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

                if (dateFilter.EndDate.HasValue)
                    contracts = contracts.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);

                if (!string.IsNullOrEmpty(search))
                    contracts = contracts.Where(c => c.EmployeeInformation.JobTitle.ToLower().Contains(search.ToLower()) || c.EmployeeInformation.User.FirstName.ToLower().Contains(search.ToLower()) || c.EmployeeInformation.User.LastName.ToLower().Contains(search.ToLower())
                    || (c.EmployeeInformation.User.FirstName.ToLower() + " " + c.EmployeeInformation.User.LastName.ToLower()).Contains(search.ToLower()));



                var pagedContracts = contracts.Skip(options.Offset.Value).Take(options.Limit.Value);

                var mappedContracts = pagedContracts.ProjectTo<ContractView>(_configuration);

                var pagedCollection = PagedCollection<ContractView>.Create(Link.ToCollection(nameof(ContractController.ListContracts)), mappedContracts.ToArray(), contracts.Count(), options);

                return  StandardResponse<PagedCollection<ContractView>>.Ok(pagedCollection);

            }
            catch (System.Exception ex)
            {
                return _customLogger.Error<PagedCollection<ContractView>>(_customLogger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<ContractView>> GetTeamMemberContract(Guid employeeInformationId, DateFilter dateFilter = null)
        {
            try
            {
                var contracts = _contractRepository.Query().Where(contract => contract.EmployeeInformationId == employeeInformationId).Include(c => c.EmployeeInformation).ThenInclude(e => e.User).OrderByDescending(u => u.DateCreated).AsQueryable();

                if (contracts == null)
                    return _customLogger.Error<ContractView>(_customLogger.GetMethodName(), new System.Exception("Contract not found"));

                if (dateFilter.StartDate.HasValue)
                    contracts = contracts.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

                if (dateFilter.EndDate.HasValue)
                    contracts = contracts.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);

                var contractView = _mapper.Map<ContractView>(contracts);
                return StandardResponse<ContractView>.Ok(contractView);
            }
            catch (System.Exception ex)
            {
                return _customLogger.Error<ContractView>(_customLogger.GetMethodName(), ex);
            }
        }

        public Contract GetCurrentContract(Guid employeeInformationId)
        {
            var currentContract = _contractRepository.Query().Where(x => x.EmployeeInformationId == employeeInformationId).OrderBy(x => x.DateCreated).LastOrDefault();
            if (currentContract == null) return null;
            return currentContract;
        }

    }
}