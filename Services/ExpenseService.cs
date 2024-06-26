using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TimesheetBE.Controllers;
using TimesheetBE.Models;
using TimesheetBE.Models.AppModels;
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
    public class ExpenseService : IExpenseService
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly ICustomLogger<ExpenseService> _logger;
        private readonly IMapper _mapper;
        private readonly IConfigurationProvider _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IEmployeeInformationRepository _employeeInformationRepository;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IDataExport _dataExport;
        private readonly IUserRepository _userRepository;
        private readonly IEmailHandler _emailHandler;
        private readonly Globals _appSettings;

        public ExpenseService(ICustomLogger<ExpenseService> logger, IExpenseRepository expenseRepository, IMapper mapper, IConfigurationProvider configuration, 
            IHttpContextAccessor httpContextAccessor, IInvoiceRepository invoiceRepository, IEmployeeInformationRepository employeeInformationRepository, 
            IConfigurationProvider configurationProvider, IDataExport dataExport, IUserRepository userRepository, IEmailHandler emailHandler, IOptions<Globals> appSettings)
        {
            _logger = logger;
            _expenseRepository = expenseRepository;
            _mapper = mapper;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _invoiceRepository = invoiceRepository;
            _employeeInformationRepository = employeeInformationRepository;
            _configurationProvider = configurationProvider;
            _dataExport = dataExport;
            _userRepository = userRepository;
            _emailHandler = emailHandler;
            _appSettings = appSettings.Value;
        }

        /// <summary>
        /// Creates a new expense and returns it
        /// </summary>
        /// <param name="expense"></param>
        /// <returns></returns>
        public async Task<StandardResponse<ExpenseView>> AddExpense(ExpenseModel expense)
        {
            try
            {
                var loggedInUserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();
                var user = _userRepository.Query().FirstOrDefault(x => x.Id == loggedInUserId);
                var mappedExpense = _mapper.Map<Expense>(expense);
                mappedExpense.CreatedByUserId = loggedInUserId;
                mappedExpense.StatusId = (int)Statuses.PENDING;
                mappedExpense.IsInvoiced = false;
                var createdExpense = _expenseRepository.CreateAndReturn(mappedExpense);
                var mappedExpenseView = _mapper.Map<ExpenseView>(createdExpense);
                List<KeyValuePair<string, string>> EmailParameters = new()
                                {
                                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_LOGO_URL, _appSettings.LOGO),
                                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, user.FirstName),
                                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_URL, "#")
                                };

                var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.EXPENSE_REVIEWED_FILENAME, EmailParameters);
                var SendEmail = _emailHandler.SendEmail(user.Email, "YOU HAVE AN EXPENSE AWAITING REVIEW", EmailTemplate, "");
                return StandardResponse<ExpenseView>.Ok(mappedExpenseView);
            }
            catch (Exception ex)
            {
                return _logger.Error<ExpenseView>(_logger.GetMethodName(), ex);
            }
        }

        /// <summary>
        /// Returns a list of paginated enxpenseviews
        /// </summary>
        /// <param name="pagingOptions"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        public async Task<StandardResponse<PagedCollection<ExpenseView>>> ListExpenses(PagingOptions pagingOptions, Guid superAdminId, Guid? employeeInformationId = null, string search = null, DateFilter dateFilter = null)
        {
            try
            {
                var expenses = _expenseRepository.Query().Include(x => x.TeamMember).ThenInclude(x => x.EmployeeInformation).Include(x => x.ExpenseType).Include(x => x.Status).Where(x => x.TeamMember.SuperAdminId == superAdminId).OrderByDescending(u => u.DateCreated).AsNoTracking();

                if (employeeInformationId.HasValue)
                    expenses = expenses.Where(x => x.TeamMember.EmployeeInformationId == employeeInformationId).OrderByDescending(u => u.DateCreated);

                if (dateFilter.StartDate.HasValue)
                    expenses = expenses.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

                if (dateFilter.EndDate.HasValue)
                    expenses = expenses.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);

                if (!string.IsNullOrEmpty(search))
                    expenses = expenses.Where(x => x.Description.Contains(search) || x.TeamMember.FirstName.Contains(search) || x.TeamMember.LastName.Contains(search) || x.ExpenseType.Name.Contains(search)
                    || (x.TeamMember.FirstName.ToLower() + " " + x.TeamMember.LastName.ToLower()).Contains(search.ToLower()) || x.Status.Name.Contains(search)).OrderByDescending(u => u.DateCreated);

                var pagedExpenses = expenses.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedExpenses = pagedExpenses.ProjectTo<ExpenseView>(_configuration).ToArray();

                var pagedCollection = PagedCollection<ExpenseView>.Create(Link.ToCollection(nameof(FinancialController.ListExpenses)), mappedExpenses, expenses.Count(), pagingOptions);

                return StandardResponse<PagedCollection<ExpenseView>>.Ok(pagedCollection);
            }
            catch (Exception ex)
            {
                return _logger.Error<PagedCollection<ExpenseView>>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<PagedCollection<ExpenseView>>> ListReviewedExpenses(PagingOptions pagingOptions, Guid superAdminId, string search = null, DateFilter dateFilter = null)
        {
            try
            {
                var expenses = _expenseRepository.Query().Include(x => x.TeamMember).ThenInclude(x => x.EmployeeInformation).Include(x => x.ExpenseType).Include(x => x.Status).
                    Where(expense => (expense.StatusId == (int)Statuses.REVIEWED && expense.TeamMember.SuperAdminId == superAdminId) || (expense.StatusId == (int)Statuses.PENDING && expense.TeamMember.SuperAdminId == superAdminId)).OrderByDescending(u => u.DateCreated).AsNoTracking();

                if (dateFilter.StartDate.HasValue)
                    expenses = expenses.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

                if (dateFilter.EndDate.HasValue)
                    expenses = expenses.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);

                if (!string.IsNullOrEmpty(search))
                    expenses = expenses.Where(x => x.Description.Contains(search) || x.TeamMember.FirstName.Contains(search) || x.TeamMember.LastName.Contains(search) || (x.TeamMember.FirstName.ToLower() + " " + x.TeamMember.LastName.ToLower()).Contains(search.ToLower()) || 
                    x.ExpenseType.Name.Contains(search) || x.Status.Name.Contains(search));

                var pagedExpenses = expenses.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedExpenses = pagedExpenses.ProjectTo<ExpenseView>(_configuration).ToArray();

                var pagedCollection = PagedCollection<ExpenseView>.Create(Link.ToCollection(nameof(FinancialController.ListReviewedExpenses)), mappedExpenses, mappedExpenses.Count(), pagingOptions);

                return StandardResponse<PagedCollection<ExpenseView>>.Ok(pagedCollection);
            }
            catch (Exception ex)
            {
                return _logger.Error<PagedCollection<ExpenseView>>(_logger.GetMethodName(), ex);
            }
        }

        /// <summary>
        /// Review an expense and returns an expenseview
        /// </summary>
        /// <param name="expenseId"></param>
        /// <returns></returns>

        public async Task<StandardResponse<ExpenseView>> ReviewExpense(Guid expenseId)
        {
            try
            {
                var expense = _expenseRepository.Query().Include(x => x.TeamMember).Include(x => x.ExpenseType).Include(x => x.Status).FirstOrDefault(x => x.Id == expenseId);
                if (expense == null)
                    return StandardResponse<ExpenseView>.NotFound("Expense not found");

                expense.StatusId = (int)Statuses.REVIEWED;
                var updatedExpense = _expenseRepository.Update(expense);
                var mappedExpense = _mapper.Map<ExpenseView>(updatedExpense);
                var expenseStatus = (Statuses)expense.StatusId;
                mappedExpense.Status = expenseStatus.ToString();
                return StandardResponse<ExpenseView>.Ok(mappedExpense);
            }
            catch (Exception ex)
            {
                return _logger.Error<ExpenseView>(_logger.GetMethodName(), ex);
            }
        }

        /// <summary>
        /// Approves an expense and returns an expenseview
        /// </summary>
        /// <param name="expenseId"></param>
        /// <returns></returns>
        public async Task<StandardResponse<ExpenseView>> ApproveExpense(Guid expenseId)
        {
            try
            {
                var loggedInUser = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();

                var user = _userRepository.Query().FirstOrDefault(x => x.Id == loggedInUser);

                if(user == null) return StandardResponse<ExpenseView>.NotFound("User not found");

                var expense = _expenseRepository.Query().Include(x => x.TeamMember).Include(x => x.ExpenseType).Include(x => x.Status).FirstOrDefault(x => x.Id == expenseId);

                if (expense == null)
                    return StandardResponse<ExpenseView>.NotFound("Expense not found");

                if (user.Role.ToLower() != "super admin" && user.Role.ToLower() != "admin" && expense.StatusId != (int)Statuses.REVIEWED)
                    return StandardResponse<ExpenseView>.Error("Expense has not been reviewed");

                expense.StatusId = (int)Statuses.APPROVED;
                var updatedExpense = _expenseRepository.Update(expense);
                var mappedExpense = _mapper.Map<ExpenseView>(updatedExpense);
                return StandardResponse<ExpenseView>.Ok(mappedExpense);


                

                //if (employeeInformation.PayRollTypeId == (int)PayrollTypes.ONSHORE)
                //{
                //    //Create a new invoice for the approved expense
                //    var invoice = new Invoice
                //    {
                //        PaymentDate = DateTime.Now,
                //        EmployeeInformationId = (Guid)expense.TeamMember.EmployeeInformationId,
                //        TotalAmount = Convert.ToDouble(expense.Amount),
                //        StatusId = (int)Statuses.PENDING,
                //        DateCreated = DateTime.Now,
                //        DateModified = DateTime.Now,
                //    };
                //    invoice = _invoiceRepository.CreateAndReturn(invoice);
                //}
            }
            catch (Exception ex)
            {
                return _logger.Error<ExpenseView>(_logger.GetMethodName(), ex);
            }
        }

        /// <summary>
        /// Declines an expense and returns an expenseview
        /// </summary>
        /// <param name="expenseId"></param>
        /// <returns></returns>
        public async Task<StandardResponse<ExpenseView>> DeclineExpense(Guid expenseId)
        {
            try
            {
                var expense = _expenseRepository.Query().Include(x => x.TeamMember).Include(x => x.ExpenseType).Include(x => x.Status).FirstOrDefault(x => x.Id == expenseId);
                if (expense == null)
                    return StandardResponse<ExpenseView>.NotFound("Expense not found");

                expense.StatusId = (int)Statuses.DECLINED;
                var updatedExpense = _expenseRepository.Update(expense);
                var mappedExpense = _mapper.Map<ExpenseView>(updatedExpense);
                return StandardResponse<ExpenseView>.Ok(mappedExpense);
            }
            catch (Exception ex)
            {
                return _logger.Error<ExpenseView>(_logger.GetMethodName(), ex);
            }
        }

        /// <summary>
        /// Returns a list of approved expenses for offshore employees
        /// </summary>
        /// <param name="pagingOptions"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        public async Task<StandardResponse<PagedCollection<ExpenseView>>> ListApprovedExpenses(PagingOptions pagingOptions, string search = null, DateFilter dateFilter = null)
        {
            try
            {
                var loggedInUserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();
                var expenses = _expenseRepository.Query().Include(x => x.TeamMember).ThenInclude(x => x.EmployeeInformation).Include(x => x.ExpenseType).Include(x => x.Status).
                    Where(expense => expense.StatusId == (int)Statuses.APPROVED && expense.TeamMember.EmployeeInformation.PayRollTypeId == (int)PayrollTypes.OFFSHORE && expense.TeamMember.EmployeeInformation.PaymentPartnerId == loggedInUserId).OrderByDescending(u => u.DateCreated).AsNoTracking();

                if (dateFilter.StartDate.HasValue)
                    expenses = expenses.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

                if (dateFilter.EndDate.HasValue)
                    expenses = expenses.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);

                if (!string.IsNullOrEmpty(search))
                    expenses = expenses.Where(x => x.Description.Contains(search) || x.TeamMember.FirstName.Contains(search) || x.TeamMember.LastName.Contains(search) || (x.TeamMember.FirstName.ToLower() + " " + x.TeamMember.LastName.ToLower()).Contains(search.ToLower())
                    || x.ExpenseType.Name.Contains(search) || x.Status.Name.Contains(search)).OrderByDescending(u => u.DateCreated);

                var pagedExpenses = expenses.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedExpenses = pagedExpenses.ProjectTo<ExpenseView>(_configuration).ToArray();

                var pagedCollection = PagedCollection<ExpenseView>.Create(Link.ToCollection(nameof(FinancialController.ListApprovedExpenses)), mappedExpenses, expenses.Count(), pagingOptions);

                return StandardResponse<PagedCollection<ExpenseView>>.Ok(pagedCollection);
            }
            catch (Exception ex)
            {
                return _logger.Error<PagedCollection<ExpenseView>>(_logger.GetMethodName(), ex);
            }
        }
    
        /// <summary>
        /// Returns a list of all approved expenses
        /// </summary>
        /// <param name="pagingOptions"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        public async Task<StandardResponse<PagedCollection<ExpenseView>>> ListAllApprovedExpenses(PagingOptions pagingOptions, Guid superAdminId, string search = null, DateFilter dateFilter = null)
        {
            try
            {
                var expenses = _expenseRepository.Query().Include(x => x.TeamMember).ThenInclude(x => x.EmployeeInformation).Include(x => x.ExpenseType).Include(x => x.Status).
                    Where(expense => expense.StatusId == (int)Statuses.APPROVED  && expense.TeamMember.SuperAdminId == superAdminId).OrderByDescending(u => u.DateCreated).AsNoTracking();

                if (dateFilter.StartDate.HasValue)
                    expenses = expenses.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

                if (dateFilter.EndDate.HasValue)
                    expenses = expenses.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);

                if (!string.IsNullOrEmpty(search))
                    expenses = expenses.Where(x => x.Description.Contains(search) || x.TeamMember.FirstName.Contains(search) || x.TeamMember.LastName.Contains(search) || (x.TeamMember.FirstName.ToLower() + " " + x.TeamMember.LastName.ToLower()).Contains(search.ToLower())
                    || x.ExpenseType.Name.Contains(search) || x.Status.Name.Contains(search)).OrderByDescending(u => u.DateCreated);

                var pagedExpenses = expenses.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedExpenses = pagedExpenses.AsQueryable().ProjectTo<ExpenseView>(_configuration).ToArray();

                var pagedCollection = PagedCollection<ExpenseView>.Create(Link.ToCollection(nameof(FinancialController.ListAllApprovedExpenses)), mappedExpenses, expenses.Count(), pagingOptions);

                return StandardResponse<PagedCollection<ExpenseView>>.Ok(pagedCollection);
            }
            catch (Exception ex)
            {
                return _logger.Error<PagedCollection<ExpenseView>>(_logger.GetMethodName(), ex);
            }

        }
        
        /// <summary>
        /// Returns a list of all expenses where the employee belongs to a particular payment partner 
        /// </summary>
        /// <param name="pagingOptions"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        public async Task<StandardResponse<PagedCollection<ExpenseView>>> ListExpensesForPaymentPartner(PagingOptions pagingOptions, string search = null, DateFilter dateFilter = null)
        {
            try
            {
                var loggedInUserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();
                var expenses = _expenseRepository.Query().Include(x => x.TeamMember).ThenInclude(x => x.EmployeeInformation).Include(x => x.ExpenseType).Include(x => x.Status).
                    Where(expense => expense.TeamMember.EmployeeInformation.PaymentPartnerId == loggedInUserId).OrderByDescending(u => u.DateCreated).AsNoTracking();

                if (dateFilter.StartDate.HasValue)
                    expenses = expenses.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

                if (dateFilter.EndDate.HasValue)
                    expenses = expenses.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);

                if (!string.IsNullOrEmpty(search))
                    expenses = expenses.Where(x => x.Description.Contains(search) || x.TeamMember.FirstName.Contains(search) || x.TeamMember.LastName.Contains(search) || (x.TeamMember.FirstName.ToLower() + " " + x.TeamMember.LastName.ToLower()).Contains(search.ToLower())
                    || x.ExpenseType.Name.Contains(search) || x.Status.Name.Contains(search)).OrderByDescending(u => u.DateCreated);

                var pagedExpenses = expenses.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedExpenses = pagedExpenses.ProjectTo<ExpenseView>(_configuration).ToArray();

                var pagedCollection = PagedCollection<ExpenseView>.Create(Link.ToCollection(nameof(FinancialController.ListExpensesByPaymentPartner)), mappedExpenses, expenses.Count(), pagingOptions);

                return StandardResponse<PagedCollection<ExpenseView>>.Ok(pagedCollection);
            }
            catch (Exception ex)
            {
                return _logger.Error<PagedCollection<ExpenseView>>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<PagedCollection<ExpenseView>>> ListSuperviseesExpenses(PagingOptions pagingOptions, string search = null, DateFilter dateFilter = null)
        {
            try
            {
                Guid UserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();
                var expenses = _expenseRepository.Query().Include(x => x.TeamMember).ThenInclude(x => x.EmployeeInformation).Include(x => x.ExpenseType).Include(x => x.Status).Include(x => x.TeamMember.EmployeeInformation)
                    .Where(expense => expense.TeamMember.EmployeeInformation.SupervisorId == UserId).OrderByDescending(u => u.DateCreated).AsNoTracking();

                if (dateFilter.StartDate.HasValue)
                    expenses = expenses.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

                if (dateFilter.EndDate.HasValue)
                    expenses = expenses.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);


                if (!string.IsNullOrEmpty(search))
                    expenses = expenses.Where(x => x.Description.Contains(search) || x.TeamMember.FirstName.Contains(search) || x.TeamMember.LastName.Contains(search) || (x.TeamMember.FirstName.ToLower() + " " + x.TeamMember.LastName.ToLower()).Contains(search.ToLower())
                    || x.ExpenseType.Name.Contains(search) || x.Status.Name.Contains(search)).OrderByDescending(u => u.DateCreated);

                var pagedExpenses = expenses.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedExpenses = pagedExpenses.ProjectTo<ExpenseView>(_configuration).ToList();

                var pagedCollection = PagedCollection<ExpenseView>.Create(Link.ToCollection(nameof(FinancialController.ListSuperviseesExpenses)), mappedExpenses.ToArray(), mappedExpenses.Count(), pagingOptions);

                return StandardResponse<PagedCollection<ExpenseView>>.Ok(pagedCollection);
            }
            catch (Exception ex)
            {
                return _logger.Error<PagedCollection<ExpenseView>>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<PagedCollection<ExpenseView>>> ListClientTeamMembersExpenses(PagingOptions pagingOptions, string search = null, DateFilter dateFilter = null)
        {
            try
            {
                Guid UserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();
                var expenses = _expenseRepository.Query().Include(x => x.TeamMember).ThenInclude(x => x.EmployeeInformation).Include(x => x.ExpenseType).Include(x => x.Status).Include(x => x.TeamMember.EmployeeInformation)
                    .Where(expense => expense.TeamMember.EmployeeInformation.ClientId == UserId).OrderByDescending(u => u.DateCreated).AsNoTracking();

                if (dateFilter.StartDate.HasValue)
                    expenses = expenses.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

                if (dateFilter.EndDate.HasValue)
                    expenses = expenses.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);

                if (!string.IsNullOrEmpty(search))
                    expenses = expenses.Where(x => x.Description.Contains(search) || x.TeamMember.FirstName.Contains(search) || x.TeamMember.LastName.Contains(search) || (x.TeamMember.FirstName.ToLower() + " " + x.TeamMember.LastName.ToLower()).Contains(search.ToLower())
                    || x.ExpenseType.Name.Contains(search) || x.Status.Name.Contains(search)).OrderByDescending(u => u.DateCreated);

                var pagedExpenses = expenses.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedExpenses = pagedExpenses.ProjectTo<ExpenseView>(_configuration).ToList();

                var pagedCollection = PagedCollection<ExpenseView>.Create(Link.ToCollection(nameof(FinancialController.ListClientTeamMembersExpenses)), mappedExpenses.ToArray(), mappedExpenses.Count(), pagingOptions);

                return StandardResponse<PagedCollection<ExpenseView>>.Ok(pagedCollection);
            }
            catch (Exception ex)
            {
                return _logger.Error<PagedCollection<ExpenseView>>(_logger.GetMethodName(), ex);
            }
        }

        public StandardResponse<byte[]> ExportExpenseRecord(ExpenseRecordDownloadModel model, DateFilter dateFilter)
        {
            try
            {
                var expenses = _expenseRepository.Query().Include(x => x.TeamMember).Include(x => x.ExpenseType).Include(x => x.Status).
                    Where(x => x.DateCreated >= dateFilter.StartDate && x.DateCreated <= dateFilter.EndDate && x.TeamMember.SuperAdminId == model.SuperAdminId);

                switch (model.Record)
                {
                    case ExpenseRecordsToDownload.ReviwedExpenses:
                        expenses = expenses.Where(expense => expense.StatusId == (int)Statuses.REVIEWED).OrderByDescending(u => u.DateCreated);
                        break;
                    case ExpenseRecordsToDownload.ApprovedExpenses:
                        expenses = expenses.Where(expense => expense.StatusId == (int)Statuses.APPROVED).OrderByDescending(u => u.DateCreated);
                        break;
                    default:
                        break;
                }
                var expenseList = expenses.ToList();
                var workbook = _dataExport.ExportExpenseRecords(model.Record, expenseList, model.rowHeaders);
                return StandardResponse<byte[]>.Ok(workbook);
            }
            catch (Exception e)
            {
                return StandardResponse<byte[]>.Error(e.Message);
            }
        }


    }
}