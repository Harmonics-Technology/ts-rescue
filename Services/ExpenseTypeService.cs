using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Throw;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Repositories.Interfaces;
using TimesheetBE.Services.Interfaces;
using TimesheetBE.Utilities;
using TimesheetBE.Utilities.Extentions;

namespace TimesheetBE.Services
{
    public class ExpenseTypeService : IExpenseTypeService
    {
        private readonly IExpenseTypeRepository _expenseTypeRepository;
        private readonly IMapper _mapper;
        private readonly IConfigurationProvider _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IControlSettingRepository _controlSettingRepository;
        private readonly IUserRepository _userRepository;

        public ExpenseTypeService(IExpenseTypeRepository expenseTypeRepository, IMapper mapper, IConfigurationProvider configuration, IHttpContextAccessor httpContextAccessor,
             IControlSettingRepository controlSettingRepository, IUserRepository userRepository)
        {
            _expenseTypeRepository = expenseTypeRepository;
            _mapper = mapper;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _controlSettingRepository = controlSettingRepository;
            _userRepository = userRepository;
        }

        public async Task<StandardResponse<ExpenseTypeView>> CreateExpenseType(Guid superAdminId, string name)
        {
            try
            {
                Guid UserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();

                var user = _userRepository.Query().FirstOrDefault(x => x.Id == UserId);

                if (user.Role.ToLower() != "super admin")
                {
                    var superAdminSettings = _controlSettingRepository.Query().FirstOrDefault(x => x.SuperAdminId == user.SuperAdminId);

                    if (!superAdminSettings.AdminLeaveManagement) return StandardResponse<ExpenseTypeView>.Failed("Expense type configuration is disabled for admins");
                }

                name.Throw().IfEmpty().IfEquals(null);

                var existingExpenseType = _expenseTypeRepository.Query().Where(x => x.Name.ToLower() == name.ToLower());

                existingExpenseType.ThrowIfNull();

                var newExpenseType = new ExpenseType
                {
                    SuperAdminId = superAdminId,
                    Name = name,
                    DateCreated = DateTime.Now,
                    DateModified = DateTime.Now,
                    StatusId = (int)Statuses.ACTIVE
                };

                newExpenseType = _expenseTypeRepository.CreateAndReturn(newExpenseType);

                var mappedView = _mapper.Map<ExpenseTypeView>(newExpenseType);

                return StandardResponse<ExpenseTypeView>.Ok(mappedView);

            }
            catch (System.Exception ex)
            {
                return StandardResponse<ExpenseTypeView>.Error(ex.Message);
            }
        }

        public async Task<StandardResponse<IEnumerable<ExpenseTypeView>>> ListExpenseTypes(Guid SuperAdminId)
        {
            try
            {
                var expenseTypes = _expenseTypeRepository.Query().Where(x => x.SuperAdminId == SuperAdminId).OrderByDescending(u => u.DateCreated);

                var mappedView = expenseTypes.ProjectTo<ExpenseTypeView>(_configuration).ToList();

                return StandardResponse<IEnumerable<ExpenseTypeView>>.Ok(mappedView);

            }
            catch (Exception ex)
            {
                return StandardResponse<IEnumerable<ExpenseTypeView>>.Ok(ex.Message);
            }
        }

        public async Task<StandardResponse<ExpenseTypeView>> ToggleStatus(Guid expenseTypeId)
        {
            try
            {
                Guid UserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();

                var user = _userRepository.Query().FirstOrDefault(x => x.Id == UserId);

                if (user.Role.ToLower() != "super admin")
                {
                    var superAdminSettings = _controlSettingRepository.Query().FirstOrDefault(x => x.SuperAdminId == user.SuperAdminId);

                    if (!superAdminSettings.AdminLeaveManagement) return StandardResponse<ExpenseTypeView>.Failed("Expense type configuration is disabled for admins");
                }

                expenseTypeId.ThrowIfNull();
                var existingExpenseType = _expenseTypeRepository.Query().Where(x => x.Id == expenseTypeId).FirstOrDefault();

                existingExpenseType.ThrowIfNull();

                existingExpenseType.StatusId = existingExpenseType.StatusId ==  (int)Statuses.ACTIVE ? (int)Statuses.INACTIVE : (int)Statuses.ACTIVE;

                existingExpenseType = _expenseTypeRepository.Update(existingExpenseType);

                var mappedView = _mapper.Map<ExpenseTypeView>(existingExpenseType);
                return StandardResponse<ExpenseTypeView>.Ok(mappedView);
            }
            catch (Exception ex)
            {
                return StandardResponse<ExpenseTypeView>.Error(ex.Message);
            }
        }
    }
}