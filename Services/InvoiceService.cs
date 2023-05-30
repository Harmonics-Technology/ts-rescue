using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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
using TimesheetBE.Utilities.Extentions;

namespace TimesheetBE.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly IPaySlipRepository _paySlipRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IPayrollRepository _payRollRepository;
        private readonly IConfigurationProvider _mapperConfiguration;
        private readonly ICodeProvider _codeProvider;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IUserRepository _userRepository;
        private readonly ICustomLogger<InvoiceService> _logger;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly IDataExport _dataExport;

        public InvoiceService(IInvoiceRepository invoiceRepository, IExpenseRepository expenseRepository, IPayrollRepository payRollRepository, IConfigurationProvider mapperConfiguration, ICodeProvider codeProvider, 
            IHttpContextAccessor httpContext, IUserRepository userRepository, ICustomLogger<InvoiceService> logger, IMapper mapper, IPaySlipRepository paySlipRepository, INotificationService notificationService, IDataExport dataExport)
        {
            _invoiceRepository = invoiceRepository;
            _expenseRepository = expenseRepository;
            _payRollRepository = payRollRepository;
            _paySlipRepository = paySlipRepository;
            _mapperConfiguration = mapperConfiguration;
            _codeProvider = codeProvider;
            _httpContext = httpContext;
            _userRepository = userRepository;
            _logger = logger;
            _mapper = mapper;
            _notificationService = notificationService;
            _dataExport = dataExport;
        }

        /// <summary>
        /// Generate single invoice for multiple expenses from an array of expense ids
        /// </summary>
        /// <param name="expenseIds"></param>
        /// <returns></returns>
        public async Task<StandardResponse<bool>> GenerateExpenseInvoices(Guid[] expenseIds)
        {
            try
            {
                var loggedInUserId = _httpContext.HttpContext.User.GetLoggedInUserId<Guid>();
                var invoice = new Invoice();
                var totalAmount = 0.0;
                var totalHours = 0;
                var startDate = DateTime.Now;
                var endDate = DateTime.Now;

                foreach (var expenseId in expenseIds)
                {
                    var expense = _expenseRepository.Query().FirstOrDefault(x => x.Id == expenseId);
                    if (expense == null)
                    {
                        return StandardResponse<bool>.Ok(false);
                    }

                    if (expense.StatusId != (int)Statuses.APPROVED)
                    {
                        return StandardResponse<bool>.Ok(false);
                    }
                    totalAmount += Convert.ToDouble(expense.Amount);
                }


                invoice.TotalAmount = totalAmount;
                invoice.TotalHours = totalHours;
                invoice.StartDate = startDate;
                invoice.EndDate = endDate;
                invoice.StatusId = (int)Statuses.PENDING;
                invoice.Rate = "Hourly";
                invoice.PaymentDate = DateTime.Now;
                invoice.InvoiceTypeId = (int)InvoiceTypes.EXPENSE;
                invoice.InvoiceReference = _codeProvider.New(Guid.Empty, "Invoice Reference", 0, 6, "INV-").CodeString;
                invoice.CreatedByUserId = loggedInUserId;

                var result = _invoiceRepository.CreateAndReturn(invoice);

                // update all expense to be invoiced
                foreach (var expenseId in expenseIds)
                {
                    var expense = _expenseRepository.Query().FirstOrDefault(x => x.Id == expenseId);
                    if (expense == null)
                    {
                        return StandardResponse<bool>.Ok(false);
                    }

                    if (expense.StatusId != (int)Statuses.APPROVED)
                    {
                        return StandardResponse<bool>.Ok(false);
                    }

                    expense.StatusId = (int)Statuses.INVOICED;
                    expense.InvoiceId = result.Id;
                    _expenseRepository.Update(expense);
                }
                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception e)
            {
                return StandardResponse<bool>.Error("Error generating invoice");
            }
        }

        /// <summary>
        /// Generate single invoice for multiple expenses from an array of payroll ids
        /// </summary>
        /// <param name="payrollIds"></param>
        /// <returns></returns>
        public async Task<StandardResponse<bool>> GenerateInvoiceFromPayroll(Guid[] payrollIds)
        {
            try
            {
                var loggedInUserId = _httpContext.HttpContext.User.GetLoggedInUserId<Guid>();
                var invoice = new Invoice();
                var totalAmount = 0.0;
                var totalHours = 0;
                var startDate = DateTime.Now;
                var endDate = DateTime.Now;

                foreach (var payrollId in payrollIds)
                {
                    var payroll = _payRollRepository.Query().FirstOrDefault(x => x.Id == payrollId);
                    if (payroll == null)
                    {
                        return StandardResponse<bool>.Ok(false);
                    }

                    if (payroll.StatusId != (int)Statuses.APPROVED)
                    {
                        return StandardResponse<bool>.Ok(false);
                    }

                    totalAmount += Convert.ToDouble(payroll.TotalAmount);
                    totalHours += payroll.TotalHours;
                }

                invoice.TotalAmount = totalAmount;
                invoice.TotalHours = totalHours;
                invoice.StartDate = startDate;
                invoice.EndDate = endDate;
                invoice.StatusId = (int)Statuses.PENDING;
                invoice.Rate = "Hourly";
                invoice.PaymentDate = DateTime.Now;
                invoice.InvoiceTypeId = (int)InvoiceTypes.PAYROLL;
                invoice.InvoiceReference = _codeProvider.New(Guid.Empty, "Invoice Reference", 0, 6, "INV-").CodeString;
                invoice.CreatedByUserId = loggedInUserId;

                var result = _invoiceRepository.CreateAndReturn(invoice);

                // update all payrolls to be invoiced
                foreach (var payrollId in payrollIds)
                {
                    var payroll = _payRollRepository.Query().FirstOrDefault(x => x.Id == payrollId);
                    if (payroll == null)
                    {
                        return StandardResponse<bool>.Ok(false);
                    }

                    if (payroll.StatusId != (int)Statuses.APPROVED)
                    {
                        return StandardResponse<bool>.Ok(false);
                    }

                    payroll.StatusId = (int)Statuses.INVOICED;
                    payroll.InvoiceId = result.Id;
                    _payRollRepository.Update(payroll);
                }
                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception e)
            {
                return StandardResponse<bool>.Error("Error generating invoice");
            }
        }

        /// <summary>
        /// List all invoices
        /// </summary>
        /// <param name="pagingOptions"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        public async Task<StandardResponse<PagedCollection<InvoiceView>>> ListInvoices(PagingOptions pagingOptions, string search = null, DateFilter dateFilter = null)
        {
            try
            {
                var invoices = _invoiceRepository.Query().Include(x => x.Payrolls).Include(x => x.Expenses).OrderByDescending(u => u.DateCreated).AsQueryable();

                if (dateFilter.StartDate.HasValue)
                    invoices = invoices.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

                if (dateFilter.EndDate.HasValue)
                    invoices = invoices.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);

                if (!string.IsNullOrEmpty(search))
                {
                    invoices = invoices.Where(x => x.Id.ToString().Contains(search)).OrderByDescending(u => u.DateCreated);
                }

                var total = invoices.Count();
                var items = invoices.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedInvoices = items.ProjectTo<InvoiceView>(_mapperConfiguration).ToArray();

                var result = PagedCollection<InvoiceView>.Create(Link.ToCollection(nameof(FinancialController.ListInvoices)), mappedInvoices, total, pagingOptions);

                return StandardResponse<PagedCollection<InvoiceView>>.Ok(result);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<InvoiceView>>.Error("Error listing invoices");
            }
        }

        public async Task<StandardResponse<PagedCollection<InvoiceView>>> ListTeamMemberInvoices(PagingOptions pagingOptions, string search = null, DateFilter dateFilter = null, int? status = null)
        {
            try
            {
                Guid UserId = _httpContext.HttpContext.User.GetLoggedInUserId<Guid>();
                var invoices = _invoiceRepository.Query().Include(x => x.Payrolls).Include(x => x.Expenses).
                    Where(invoice => invoice.EmployeeInformation.UserId == UserId).OrderByDescending(u => u.DateCreated).AsQueryable();

                if (status.HasValue)
                {
                    invoices = _invoiceRepository.Query().Include(x => x.Payrolls).Include(x => x.Expenses).
                        Where(invoice => invoice.EmployeeInformation.UserId == UserId && invoice.StatusId == status).OrderByDescending(u => u.DateCreated).AsQueryable();
                }

                if (dateFilter.StartDate.HasValue)
                    invoices = invoices.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

                if (dateFilter.EndDate.HasValue)
                    invoices = invoices.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);

                if (!string.IsNullOrEmpty(search))
                {
                    invoices = invoices.Where(x => x.Id.ToString().Contains(search)).OrderByDescending(u => u.DateCreated);
                }

                var total = invoices.Count();
                var items = invoices.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedInvoices = items.ProjectTo<InvoiceView>(_mapperConfiguration).ToArray();

                var result = PagedCollection<InvoiceView>.Create(Link.ToCollection(nameof(FinancialController.ListTeamMemberInvoices)), mappedInvoices, total, pagingOptions);

                return StandardResponse<PagedCollection<InvoiceView>>.Ok(result);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<InvoiceView>>.Error("Error listing invoices");
            }
        }

        public async Task<StandardResponse<bool>> SubmitInvoice(Guid invoiceId)
        {
            try
            {
                var invoice = _invoiceRepository.Query().FirstOrDefault(invoice => invoice.Id == invoiceId);

                if (invoice == null)
                    return StandardResponse<bool>.NotFound("No ivoice found for this cycle");


                invoice.StatusId = (int)Statuses.SUBMITTED;
                _invoiceRepository.Update(invoice);

                var getAdmins = _userRepository.Query().Where(x => x.Role.ToLower() == "payroll manager").ToList();

                foreach (var admin in getAdmins)
                {
                    await _notificationService.SendNotification(new NotificationModel { UserId = admin.Id, Title = "Pending Invoice", Type = "Notification", Message = "A new item has been added to an invoice awaiting your approval." });
                }


                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                return _logger.Error<bool>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<PagedCollection<InvoiceView>>> ListSubmittedOnshoreInvoices(PagingOptions pagingOptions, string search = null, DateFilter dateFilter = null)
        {
            try
            {
                //Guid UserId = _httpContext.HttpContext.User.GetLoggedInUserId<Guid>();
                var invoices = _invoiceRepository.Query().Include(x => x.Payrolls).Include(x => x.Expenses).Include(x => x.EmployeeInformation).
                    Where(invoice => invoice.StatusId == (int)Statuses.SUBMITTED && invoice.EmployeeInformation.PayRollTypeId == 1).OrderByDescending(u => u.DateCreated).AsQueryable();

                if (dateFilter.StartDate.HasValue)
                    invoices = invoices.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

                if (dateFilter.EndDate.HasValue)
                    invoices = invoices.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);

                if (!string.IsNullOrEmpty(search))
                {
                    invoices = invoices.Where(x => x.EmployeeInformation.User.FirstName.ToLower().Contains(search.ToLower()) || x.EmployeeInformation.User.LastName.ToLower().Contains(search.ToLower()) 
                    || (x.EmployeeInformation.User.FirstName.ToLower() + " " + x.EmployeeInformation.User.LastName.ToLower()).Contains(search.ToLower())
                    || x.InvoiceReference.ToLower().Contains(search.ToLower())).OrderByDescending(u => u.DateCreated); //team member name and refrence
                }

                var total = invoices.Count();
                var items = invoices.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedInvoices = items.ProjectTo<InvoiceView>(_mapperConfiguration).ToArray();

                var result = PagedCollection<InvoiceView>.Create(Link.ToCollection(nameof(FinancialController.ListSubmittedOnshoreInvoices)), mappedInvoices, total, pagingOptions);

                return StandardResponse<PagedCollection<InvoiceView>>.Ok(result);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<InvoiceView>>.Error("Error listing invoices");
            }
        }

        public async Task<StandardResponse<PagedCollection<InvoiceView>>> ListSubmittedOffshoreInvoices(PagingOptions pagingOptions, string search = null, DateFilter dateFilter = null)
        {
            try
            {
                //Guid UserId = _httpContext.HttpContext.User.GetLoggedInUserId<Guid>();
                var invoices = _invoiceRepository.Query().Include(x => x.Payrolls).Include(x => x.Expenses).Include(x => x.EmployeeInformation).
                    Where(invoice => invoice.StatusId == (int)Statuses.SUBMITTED && invoice.EmployeeInformation.PayRollTypeId == 2).OrderByDescending(u => u.DateCreated).AsQueryable();

                if (dateFilter.StartDate.HasValue)
                    invoices = invoices.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

                if (dateFilter.EndDate.HasValue)
                    invoices = invoices.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);

                if (!string.IsNullOrEmpty(search))
                {
                    invoices = invoices.Where(x => x.EmployeeInformation.User.FirstName.ToLower().Contains(search.ToLower()) || x.EmployeeInformation.User.LastName.ToLower().Contains(search.ToLower())
                    || (x.EmployeeInformation.User.FirstName.ToLower() + " " + x.EmployeeInformation.User.LastName.ToLower()).Contains(search.ToLower())
                    || x.InvoiceReference.ToLower().Contains(search.ToLower())).OrderByDescending(u => u.DateCreated); //team member name and refrence
                }

                var total = invoices.Count();
                var items = invoices.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedInvoices = items.ProjectTo<InvoiceView>(_mapperConfiguration).ToArray();

                var result = PagedCollection<InvoiceView>.Create(Link.ToCollection(nameof(FinancialController.ListSubmittedOffshoreInvoices)), mappedInvoices, total, pagingOptions);

                return StandardResponse<PagedCollection<InvoiceView>>.Ok(result);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<InvoiceView>>.Error("Error listing invoices");
            }
        }

        public async Task<StandardResponse<PagedCollection<InvoiceView>>> ListSubmittedInvoices(PagingOptions pagingOptions, string search = null, DateFilter dateFilter = null, int? payrollTypeFilter = null)
        {
            try
            {
                //Guid UserId = _httpContext.HttpContext.User.GetLoggedInUserId<Guid>();
                var invoices = _invoiceRepository.Query().Include(x => x.Payrolls).Include(x => x.Expenses).Include(x => x.EmployeeInformation).
                    Where(invoice => invoice.StatusId != (int)Statuses.PENDING && invoice.StatusId != (int)Statuses.SUBMITTED && invoice.PaymentPartnerId == null).OrderByDescending(u => u.DateCreated).AsQueryable();

                if (dateFilter.StartDate.HasValue)
                    invoices = invoices.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

                if (dateFilter.EndDate.HasValue)
                    invoices = invoices.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);

                if (payrollTypeFilter.HasValue)
                    invoices = invoices.Where(u => u.EmployeeInformation.PayRollTypeId == payrollTypeFilter.Value).OrderByDescending(u => u.DateCreated);

                if (!string.IsNullOrEmpty(search))
                {
                    invoices = invoices.Where(x => x.EmployeeInformation.User.FirstName.ToLower().Contains(search.ToLower()) || x.EmployeeInformation.User.LastName.ToLower().Contains(search.ToLower())
                    || (x.EmployeeInformation.User.FirstName.ToLower() + " " + x.EmployeeInformation.User.LastName.ToLower()).Contains(search.ToLower())
                    || x.InvoiceReference.ToLower().Contains(search.ToLower())).OrderByDescending(u => u.DateCreated); //team member name and refrence
                }

                var total = invoices.Count();
                var items = invoices.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedInvoices = items.ProjectTo<InvoiceView>(_mapperConfiguration).ToArray();

                var result = PagedCollection<InvoiceView>.Create(Link.ToCollection(nameof(FinancialController.ListSubmittedInvoices)), mappedInvoices, total, pagingOptions);

                return StandardResponse<PagedCollection<InvoiceView>>.Ok(result);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<InvoiceView>>.Error("Error listing invoices");
            }
        }

        public async Task<StandardResponse<PagedCollection<InvoiceView>>> ListTeamMemberSubmittedInvoices(PagingOptions pagingOptions, string search = null, DateFilter dateFilter = null)
        {
            try
            {
                Guid UserId = _httpContext.HttpContext.User.GetLoggedInUserId<Guid>();
                var invoices = _invoiceRepository.Query().Include(x => x.Payrolls).Include(x => x.Expenses).
                    Where(invoice => invoice.StatusId != (int)Statuses.PENDING && invoice.EmployeeInformation.UserId == UserId).OrderByDescending(u => u.DateCreated).AsQueryable();

                if (dateFilter.StartDate.HasValue)
                    invoices = invoices.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

                if (dateFilter.EndDate.HasValue)
                    invoices = invoices.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);

                if (!string.IsNullOrEmpty(search))
                {
                    invoices = invoices.Where(x => x.Id.ToString().Contains(search)).OrderByDescending(u => u.DateCreated);
                }

                var total = invoices.Count();
                var items = invoices.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedInvoices = items.ProjectTo<InvoiceView>(_mapperConfiguration).ToArray();

                var result = PagedCollection<InvoiceView>.Create(Link.ToCollection(nameof(FinancialController.ListTeamMemberSubmittedInvoices)), mappedInvoices, total, pagingOptions);

                return StandardResponse<PagedCollection<InvoiceView>>.Ok(result);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<InvoiceView>>.Error("Error listing invoices");
            }
        }

        public async Task<StandardResponse<bool>> TreatSubmittedInvoice(Guid invoiceId)
        {
            try
            {
                var invoice = _invoiceRepository.Query().Include(x => x.Children).Include(x => x.EmployeeInformation).FirstOrDefault(invoice => invoice.Id == invoiceId);

                if (invoice == null)
                    return StandardResponse<bool>.NotFound("No invoice found");


                if (invoice.EmployeeInformation != null)
                {
                    if (invoice.EmployeeInformation.PayRollTypeId == 1)
                    {
                        invoice.StatusId = (int)Statuses.INVOICED;
                        invoice.PaymentDate = DateTime.Now;
                        GeneratePaySlip(invoiceId);
                        await _notificationService.SendNotification(new NotificationModel { UserId = invoice.EmployeeInformation.UserId, Title = "Invoice Approved", Type = "Notification", Message = $"Your invoice for work cycle {invoice.StartDate.Date} - {invoice.EndDate.Date} has been reviewed and approved" });
                    }
                    else
                    {
                        invoice.StatusId = (int)Statuses.APPROVED;
                    }
                }
                else
                {
                    if (invoice.StatusId == (int)Statuses.APPROVED)
                    {
                        invoice.StatusId = (int)Statuses.INVOICED;
                        foreach (var children in invoice.Children)
                        {
                            children.StatusId = (int)Statuses.INVOICED;
                            _invoiceRepository.Update(children);
                        }
                    }
                    else
                    {

                        invoice.StatusId = (int)Statuses.APPROVED;
                        foreach(var children in invoice.Children)
                        {
                            children.StatusId = (int)Statuses.REVIEWED;
                            _invoiceRepository.Update(children);
                            GeneratePaySlip(children.Id);
                            await _notificationService.SendNotification(new NotificationModel { UserId = invoice.EmployeeInformation.UserId, Title = "Invoice Approved", Type = "Notification", Message = $"Your invoice for work cycle {children.StartDate.Date.ToString()} - {children.EndDate.Date.ToString()} has been reviewed and approved" });
                        }
                        invoice.PaymentDate = DateTime.Now;
                    }
                }

                _invoiceRepository.Update(invoice);


                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                return _logger.Error<bool>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<PagedCollection<InvoiceView>>> ListPendingInvoiceForPaymentPartner(PagingOptions pagingOptions, string search = null, int? payrollGroupId = null, DateFilter dateFilter = null)
        {
            try
            {
                Guid UserId = _httpContext.HttpContext.User.GetLoggedInUserId<Guid>();
                var invoices = _invoiceRepository.Query().Include(x => x.PayrollGroup).
                    Where(invoice => invoice.StatusId == (int)Statuses.APPROVED && invoice.EmployeeInformation.PaymentPartnerId == UserId).OrderByDescending(u => u.DateCreated).AsQueryable();

                var inv = invoices.ToList();

                if (dateFilter.StartDate.HasValue)
                    invoices = invoices.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

                if (dateFilter.EndDate.HasValue)
                    invoices = invoices.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);

                if (payrollGroupId.HasValue)
                    invoices = invoices.Where(u => u.PayrollGroupId == payrollGroupId).OrderByDescending(u => u.DateCreated);

                if (!string.IsNullOrEmpty(search))
                {
                    invoices = invoices.Where(x => x.PayrollGroup.Name.ToLower().Contains(search.ToLower()) || x.InvoiceReference.ToLower().Contains(search.ToLower())).OrderByDescending(u => u.DateCreated); //team member name and refrence
                }

                var total = invoices.Count();
                var items = invoices.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedInvoices = items.ProjectTo<InvoiceView>(_mapperConfiguration).ToArray();

                var result = PagedCollection<InvoiceView>.Create(Link.ToCollection(nameof(FinancialController.ListPendingInvoiceForPaymentPartner)), mappedInvoices, total, pagingOptions);

                return StandardResponse<PagedCollection<InvoiceView>>.Ok(result);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<InvoiceView>>.Error("Error listing invoices");
            }
        }

        public async Task<StandardResponse<PagedCollection<InvoiceView>>> ListPendingInvoicedInvoicesForPaymentPartner(PagingOptions pagingOptions, string search = null, DateFilter dateFilter = null)
        {
            try
            {
                Guid UserId = _httpContext.HttpContext.User.GetLoggedInUserId<Guid>();
                var invoices = _invoiceRepository.Query().Include(x => x.Payrolls).Include(x => x.Expenses).
                    Where(invoice => invoice.StatusId != (int)Statuses.INVOICED && invoice.PaymentPartnerId == UserId).OrderByDescending(u => u.DateCreated).AsQueryable();

                if (dateFilter.StartDate.HasValue)
                    invoices = invoices.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

                if (dateFilter.EndDate.HasValue)
                    invoices = invoices.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);

                if (!string.IsNullOrEmpty(search))
                {
                    invoices = invoices.Where(x => x.EmployeeInformation.User.FirstName.ToLower().Contains(search.ToLower()) || x.EmployeeInformation.User.LastName.ToLower().Contains(search.ToLower())
                    || (x.EmployeeInformation.User.FirstName.ToLower() + " " + x.EmployeeInformation.User.LastName.ToLower()).Contains(search.ToLower())
                    || x.InvoiceReference.ToLower().Contains(search.ToLower())).OrderByDescending(u => u.DateCreated); //team member name and refrence
                }

                var total = invoices.Count();
                var items = invoices.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedInvoices = items.ProjectTo<InvoiceView>(_mapperConfiguration).ToArray();

                var result = PagedCollection<InvoiceView>.Create(Link.ToCollection(nameof(FinancialController.ListPendingInvoicedInvoicesForPaymentPartner)), mappedInvoices, total, pagingOptions);

                return StandardResponse<PagedCollection<InvoiceView>>.Ok(result);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<InvoiceView>>.Error("Error listing invoices");
            }
        }
        public async Task<StandardResponse<PagedCollection<InvoiceView>>> ListInvoicedInvoices(PagingOptions pagingOptions, string search = null, DateFilter dateFilter = null, int? payrollTypeFilter = null)
        {
            try
            {
                //Guid UserId = _httpContext.HttpContext.User.GetLoggedInUserId<Guid>();
                var invoices = _invoiceRepository.Query().Include(x => x.Payrolls).Include(x => x.Expenses).
                    Where(invoice => invoice.StatusId == (int)Statuses.INVOICED && invoice.InvoiceTypeId == (int)InvoiceTypes.PAYROLL).OrderByDescending(u => u.DateCreated).AsQueryable();

                if (dateFilter.StartDate.HasValue)
                    invoices = invoices.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

                if (dateFilter.EndDate.HasValue)
                    invoices = invoices.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);

                if (payrollTypeFilter.HasValue)
                    invoices = invoices.Where(u => u.EmployeeInformation.PayRollTypeId == payrollTypeFilter.Value).OrderByDescending(u => u.DateCreated);

                if (!string.IsNullOrEmpty(search))
                {
                    invoices = invoices.Where(x => x.EmployeeInformation.User.FirstName.ToLower().Contains(search.ToLower()) || x.EmployeeInformation.User.LastName.ToLower().Contains(search.ToLower())
                    || (x.EmployeeInformation.User.FirstName.ToLower() + " " + x.EmployeeInformation.User.LastName.ToLower()).Contains(search.ToLower())
                    || x.InvoiceReference.ToLower().Contains(search.ToLower())).OrderByDescending(u => u.DateCreated); //team member name and refrence
                }

                var total = invoices.Count();
                var items = invoices.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedInvoices = items.ProjectTo<InvoiceView>(_mapperConfiguration).ToArray();

                var result = PagedCollection<InvoiceView>.Create(Link.ToCollection(nameof(FinancialController.ListInvoicedInvoices)), mappedInvoices, total, pagingOptions);

                return StandardResponse<PagedCollection<InvoiceView>>.Ok(result);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<InvoiceView>>.Error("Error listing invoices");
            }
        }

        /// <summary>
        /// List all invoices related to a payment Partner
        /// </summary>
        /// <param name="pagingOptions"></param>
        /// <param name="search"></param>
        /// <returns></returns>                                                                                                                                                                                                             
        public async Task<StandardResponse<PagedCollection<InvoiceView>>> ListInvoicesByPaymentPartner(PagingOptions pagingOptions, string search = null, int? payrollGroupId = null, DateFilter dateFilter = null)
        {
            try
            {
                var loggedInUser = _httpContext.HttpContext.User.GetLoggedInUserId<Guid>();
                var invoices = _invoiceRepository.Query().Include(x => x.PayrollGroup).OrderByDescending(u => u.DateCreated).AsQueryable();

                if (dateFilter.StartDate.HasValue)
                    invoices = invoices.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

                if (dateFilter.EndDate.HasValue)
                    invoices = invoices.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);

                if (payrollGroupId.HasValue)
                    invoices = invoices.Where(u => u.PayrollGroupId == payrollGroupId).OrderByDescending(u => u.DateCreated);

                if (!string.IsNullOrEmpty(search))
                {
                    invoices = invoices.Where(x => x.PayrollGroup.Name.ToLower().Contains(search.ToLower()) || x.InvoiceReference.ToLower().Contains(search.ToLower())).OrderByDescending(u => u.DateCreated); //team member name and refrence
                }

                invoices = invoices.Where(x => x.CreatedByUserId == loggedInUser);

                var total = invoices.Count();
                var items = invoices.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedInvoices = items.ProjectTo<InvoiceView>(_mapperConfiguration).ToArray();

                var result = PagedCollection<InvoiceView>.Create(Link.ToCollection(nameof(FinancialController.ListInvoices)), mappedInvoices, total, pagingOptions);

                return StandardResponse<PagedCollection<InvoiceView>>.Ok(result);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<InvoiceView>>.Error("Error listing invoices");
            }
        }

        /// <summary>
        /// Create an invoice for payment parrtner 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<StandardResponse<InvoiceView>> CreatePaymentPartnerInvoice(PaymentPartnerInvoiceModel model)
        {
            try
            {
                var laggedInUser = _httpContext.HttpContext.User.GetLoggedInUserId<Guid>();
                var invoice = _mapper.Map<Invoice>(model);
                invoice.CreatedByUserId = laggedInUser;
                invoice.StatusId = (int)Statuses.PENDING;
                invoice.DateCreated = DateTime.Now;
                invoice.PaymentPartnerId = laggedInUser;
                invoice.InvoiceReference = _codeProvider.New(Guid.Empty, "INV", 6, 5).CodeString;
                invoice.InvoiceTypeId = (int)InvoiceTypes.PAYMENT_PARTNER;

                invoice = _invoiceRepository.CreateAndReturn(invoice);

                model.InvoiceIds.ForEach(id =>
                {
                    var thisInvoice = _invoiceRepository.Query().FirstOrDefault(x => x.Id == id);
                    thisInvoice.ParentId = invoice.Id;
                    thisInvoice.StatusId = (int)Statuses.REVIEWING;
                    thisInvoice.Rate = model.Rate;
                    var result = _invoiceRepository.Update(thisInvoice);
                });

                var mappedInvoice = _mapper.Map<InvoiceView>(invoice);
                return StandardResponse<InvoiceView>.Ok(mappedInvoice);
            }
            catch (Exception e)
            {
                return StandardResponse<InvoiceView>.Error("Error creating invoice");
            }
        }

        /// <summary>
        /// Reject payment partner invoice
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>

        public async Task<StandardResponse<bool>> RejectPaymentPartnerInvoice(RejectPaymentPartnerInvoiceModel model)
        {
            try
            {
                var invoice = _invoiceRepository.Query().Include(x => x.Children).FirstOrDefault(invoice => invoice.Id == model.InvoiceId);
                if (invoice == null)
                    return StandardResponse<bool>.NotFound("No invoice found");

                invoice.Rejected = true;
                invoice.StatusId = (int)Statuses.REJECTED;
                invoice.RejectionReason = model.RejectionReason;

                foreach (var children in invoice.Children)
                {
                    children.StatusId = (int)Statuses.APPROVED;
                    _invoiceRepository.Update(children);
                }

                _invoiceRepository.Update(invoice);
                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                return _logger.Error<bool>(_logger.GetMethodName(), ex);
            }
            
        }

        /// <summary>
        /// list Payment Partner Invocies
        /// </summary>
        /// <param name="pagingOptions"></param>
        /// <param name="search"></param>
        /// <param name="dateFilter"></param>
        /// <returns></returns>
        public async Task<StandardResponse<PagedCollection<InvoiceView>>> ListPaymentPartnerInvoices(PagingOptions pagingOptions, string search = null, int? payrollGroupId = null, DateFilter dateFilter = null)
        {
            try
            {
                var loggedInUser = _httpContext.HttpContext.User.GetLoggedInUserId<Guid>();
                var invoices = _invoiceRepository.Query().Include(x => x.PayrollGroup).Where(x => x.EmployeeInformation != null && x.StatusId == (int)Statuses.APPROVED).OrderByDescending(u => u.DateCreated).AsQueryable();

                if (dateFilter.StartDate.HasValue)
                    invoices = invoices.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

                if (dateFilter.EndDate.HasValue)
                    invoices = invoices.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);

                if (payrollGroupId.HasValue)
                    invoices = invoices.Where(u => u.PayrollGroupId == payrollGroupId).OrderByDescending(u => u.DateCreated);

                if (!string.IsNullOrEmpty(search))
                {
                    invoices = invoices.Where(x => x.PayrollGroup.Name.ToLower().Contains(search.ToLower()) || x.InvoiceReference.ToLower().Contains(search.ToLower())).OrderByDescending(u => u.DateCreated); //team member name and refrence
                }

                invoices = invoices.Where(x => x.EmployeeInformation.PaymentPartnerId == loggedInUser);

                var total = invoices.Count();
                var items = invoices.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedInvoices = items.ProjectTo<InvoiceView>(_mapperConfiguration).ToArray();

                var result = PagedCollection<InvoiceView>.Create(Link.ToCollection(nameof(FinancialController.ListPaymentPartnerInvoices)), mappedInvoices, total, pagingOptions);

                return StandardResponse<PagedCollection<InvoiceView>>.Ok(result);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<InvoiceView>>.Error("Error listing invoices");
            }
        }

        /// <summary>
        /// list Payment client Invocies
        /// </summary>
        /// <param name="pagingOptions"></param>
        /// <param name="search"></param>
        /// <param name="dateFilter"></param>
        /// <returns></returns>
        public async Task<StandardResponse<PagedCollection<InvoiceView>>> ListClientInvoices(PagingOptions pagingOptions, Guid? clientId = null, string search = null, DateFilter dateFilter = null)
        {
            try
            {
                var loggedInUser = _httpContext.HttpContext.User.GetLoggedInUserId<Guid>();

                var invoices = _invoiceRepository.Query().Include(x => x.ClientInvoiceChildren).Where(x => x.InvoiceTypeId == (int)InvoiceTypes.CLIENT).OrderByDescending(u => u.DateCreated).AsQueryable();

                if (clientId.HasValue)
                    invoices = invoices.Where(u => u.CreatedByUserId == clientId).OrderByDescending(u => u.DateCreated);

                if (dateFilter.StartDate.HasValue)
                    invoices = invoices.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

                if (dateFilter.EndDate.HasValue)
                    invoices = invoices.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);

                if (!string.IsNullOrEmpty(search))
                {
                    invoices = invoices.Where(x => x.InvoiceReference.Contains(search.ToLower())).OrderByDescending(u => u.DateCreated); //team member name and refrence
                }

                invoices = invoices.Where(x => x.CreatedByUserId == loggedInUser);

                var total = invoices.Count();
                var items = invoices.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedInvoices = items.ProjectTo<InvoiceView>(_mapperConfiguration).ToArray();

                var result = PagedCollection<InvoiceView>.Create(Link.ToCollection(nameof(FinancialController.ListClientInvoices)), mappedInvoices, total, pagingOptions);

                return StandardResponse<PagedCollection<InvoiceView>>.Ok(result);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<InvoiceView>>.Error("Error listing invoices");
            }
        }

        public async Task<StandardResponse<PagedCollection<InvoiceView>>> ListAllClientInvoices(PagingOptions pagingOptions, string search = null, DateFilter dateFilter = null)
        {
            try
            {
                var invoices = _invoiceRepository.Query().Include(x => x.ClientInvoiceChildren).Where(x => x.InvoiceTypeId == (int)InvoiceTypes.CLIENT).OrderByDescending(u => u.DateCreated).AsQueryable();

                if (dateFilter.StartDate.HasValue)
                    invoices = invoices.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

                if (dateFilter.EndDate.HasValue)
                    invoices = invoices.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);

                if (!string.IsNullOrEmpty(search))
                {
                    invoices = invoices.Where(x => x.InvoiceReference.Contains(search.ToLower())).OrderByDescending(u => u.DateCreated); //team member name and refrence
                }

                var total = invoices.Count();
                var items = invoices.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedInvoices = items.ProjectTo<InvoiceView>(_mapperConfiguration).ToArray();

                var result = PagedCollection<InvoiceView>.Create(Link.ToCollection(nameof(FinancialController.ListAllClientInvoices)), mappedInvoices, total, pagingOptions);

                return StandardResponse<PagedCollection<InvoiceView>>.Ok(result);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<InvoiceView>>.Error("Error listing invoices");
            }
        }
        /// <summary>
        /// list Payroll group Invocies
        /// </summary>
        /// /// <param name="payrollGroupId"></param>
        /// <param name="pagingOptions"></param>
        /// <param name="search"></param>
        /// <param name="dateFilter"></param>
        /// <returns></returns>
        public async Task<StandardResponse<PagedCollection<InvoiceView>>> ListPayrollGroupInvoices(int payrollGroupId, PagingOptions pagingOptions, string search = null, DateFilter dateFilter = null)
        {
            try
            {
                var loggedInUser = _httpContext.HttpContext.User.GetLoggedInUserId<Guid>();
                var invoices = _invoiceRepository.Query().Include(x => x.EmployeeInformation).Include(x => x.Expenses).Where(x => x.EmployeeInformation != null && x.StatusId == (int)Statuses.APPROVED).OrderByDescending(u => u.DateCreated).AsQueryable();

                if (dateFilter.StartDate.HasValue)
                    invoices = invoices.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

                if (dateFilter.EndDate.HasValue)
                    invoices = invoices.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);

                if (!string.IsNullOrEmpty(search))
                {
                    invoices = invoices.Where(x => x.EmployeeInformation.User.FirstName.ToLower().Contains(search.ToLower()) || x.EmployeeInformation.User.LastName.ToLower().Contains(search.ToLower())
                    || (x.EmployeeInformation.User.FirstName.ToLower() + " " + x.EmployeeInformation.User.LastName.ToLower()).Contains(search.ToLower())
                    || x.InvoiceReference.ToLower().Contains(search.ToLower())).OrderByDescending(u => u.DateCreated); //team member name and refrence
                }

                invoices = invoices.Where(x => x.EmployeeInformation.PayrollGroupId == payrollGroupId && x.EmployeeInformation.PaymentPartnerId == loggedInUser);

                var total = invoices.Count();
                var items = invoices.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedInvoices = items.ProjectTo<InvoiceView>(_mapperConfiguration).ToArray();

                var result = PagedCollection<InvoiceView>.Create(Link.ToCollection(nameof(FinancialController.ListPayrollGroupInvoices)), mappedInvoices, total, pagingOptions);

                return StandardResponse<PagedCollection<InvoiceView>>.Ok(result);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<InvoiceView>>.Error("Error listing invoices");
            }
        }

        /// <summary>
        /// list Payment Partner children Invocies
        /// </summary>
        /// <param name="pagingOptions"></param>
        /// <param name="search"></param>
        /// <param name="dateFilter"></param>
        /// <returns></returns>

        public async Task<StandardResponse<PagedCollection<InvoiceView>>> ListInvoicesHistories(PagingOptions pagingOptions, string search = null, DateFilter dateFilter = null)
        {
            try
            {
                var invoices = _invoiceRepository.Query().Include(x => x.EmployeeInformation).Include(x => x.Expenses).Where(x => x.InvoiceTypeId == (int)InvoiceTypes.PAYROLL && x.StatusId != (int)Statuses.PENDING && x.StatusId != (int)Statuses.SUBMITTED).OrderByDescending(u => u.DateCreated).AsQueryable();

                if (dateFilter.StartDate.HasValue)
                    invoices = invoices.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

                if (dateFilter.EndDate.HasValue)
                    invoices = invoices.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);

                if (!string.IsNullOrEmpty(search))
                {
                    invoices = invoices.Where(x => x.EmployeeInformation.User.FirstName.ToLower().Contains(search.ToLower()) || x.EmployeeInformation.User.LastName.ToLower().Contains(search.ToLower())
                    || (x.EmployeeInformation.User.FirstName.ToLower() + " " + x.EmployeeInformation.User.LastName.ToLower()).Contains(search.ToLower())
                    || x.InvoiceReference.ToLower().Contains(search.ToLower())).OrderByDescending(u => u.DateCreated); //team member name and refrence
                }

                var total = invoices.Count();
                var items = invoices.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedInvoices = items.ProjectTo<InvoiceView>(_mapperConfiguration).ToArray();

                var result = PagedCollection<InvoiceView>.Create(Link.ToCollection(nameof(FinancialController.ListInvoicesHistories)), mappedInvoices, total, pagingOptions);

                return StandardResponse<PagedCollection<InvoiceView>>.Ok(result);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<InvoiceView>>.Error("Error listing invoices");
            }
        }


        /// <summary>
        /// list Payment Partner Invoices for payroll managers
        /// </summary>
        /// <param name="pagingOptions"></param>
        /// <param name="search"></param>
        /// <param name="dateFilter"></param>
        /// <returns></returns>
        public async Task<StandardResponse<PagedCollection<InvoiceView>>> ListPaymentPartnerInvoicesForPayrollManagers(PagingOptions pagingOptions, string search = null, int? payrollGroupId = null, DateFilter dateFilter = null)
        {
            try
            {
                var invoices = _invoiceRepository.Query().Include(x => x.PayrollGroup).Where(x => x.PaymentPartnerId != null).OrderByDescending(u => u.DateCreated).AsQueryable();

                if (dateFilter.StartDate.HasValue)
                    invoices = invoices.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

                if (dateFilter.EndDate.HasValue)
                    invoices = invoices.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);

                if (payrollGroupId.HasValue)
                    invoices = invoices.Where(u => u.PayrollGroupId == payrollGroupId).OrderByDescending(u => u.DateCreated);

                if (!string.IsNullOrEmpty(search))
                {
                    invoices = invoices.Where(x => x.PayrollGroup.Name.ToLower().Contains(search.ToLower()) || x.InvoiceReference.ToLower().Contains(search.ToLower())).OrderByDescending(u => u.DateCreated); //team member name and refrence
                }

                invoices = invoices.Where(x => x.PaymentPartnerId != null);

                var total = invoices.Count();
                var items = invoices.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedInvoices = items.ProjectTo<InvoiceView>(_mapperConfiguration).ToArray();

                var result = PagedCollection<InvoiceView>.Create(Link.ToCollection(nameof(FinancialController.ListPaymentPartnerInvoices)), mappedInvoices, total, pagingOptions);

                return StandardResponse<PagedCollection<InvoiceView>>.Ok(result);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<InvoiceView>>.Error("Error listing invoices");
            }
        }

        public async Task<StandardResponse<PagedCollection<InvoiceView>>> ListClientTeamMemberInvoices(PagingOptions pagingOptions, Guid clientId, string search = null, DateFilter dateFilter = null)
        {
            try
            {
                //Guid UserId = _httpContext.HttpContext.User.GetLoggedInUserId<Guid>();
                var invoices = _invoiceRepository.Query().Include(x => x.Payrolls).Include(x => x.Expenses).
                    Where(invoice => invoice.StatusId == (int)Statuses.INVOICED && invoice.EmployeeInformation.Supervisor.ClientId == clientId).OrderByDescending(u => u.DateCreated).AsQueryable();

                if (dateFilter.StartDate.HasValue)
                    invoices = invoices.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

                if (dateFilter.EndDate.HasValue)
                    invoices = invoices.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);

                if (!string.IsNullOrEmpty(search))
                {
                    invoices = invoices.Where(x => x.Id.ToString().Contains(search)).OrderByDescending(u => u.DateCreated);
                }

                var total = invoices.Count();
                var items = invoices.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedInvoices = items.ProjectTo<InvoiceView>(_mapperConfiguration).ToArray();

                var result = PagedCollection<InvoiceView>.Create(Link.ToCollection(nameof(FinancialController.ListClientTeamMemberInvoices)), mappedInvoices, total, pagingOptions);

                return StandardResponse<PagedCollection<InvoiceView>>.Ok(result);
            }
            catch (Exception e)
            {
                return StandardResponse<PagedCollection<InvoiceView>>.Error("Error listing invoices");
            }
        }

        public StandardResponse<byte[]> ExportInvoiceRecord(InvoiceRecordDownloadModel model, DateFilter dateFilter)
        {
            try
            {
                if (model.Record == InvoiceRecord.PaymentPartnerInvoices && model.PayrollGroupId == null) return StandardResponse<byte[]>.Error("Please enter a payroll group identifier for these request");
                var invoices = _invoiceRepository.Query().Include(x => x.Payrolls).Include(x => x.Status).Include(x => x.EmployeeInformation).Include(x => x.CreatedByUser).
                    Where(x => x.DateCreated >= dateFilter.StartDate && x.DateCreated <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);
                switch (model.Record)
                {
                    case InvoiceRecord.PendingPayrolls:
                        invoices = invoices.Where(invoice => invoice.StatusId == (int)Statuses.SUBMITTED && invoice.EmployeeInformation.PayRollTypeId == 2).OrderByDescending(u => u.DateCreated);
                        break;
                    case InvoiceRecord.ProcessedPayrolls:
                        invoices = invoices.Where(invoice => invoice.StatusId != (int)Statuses.PENDING && invoice.StatusId != (int)Statuses.SUBMITTED && invoice.PaymentPartnerId == null && invoice.EmployeeInformation.PayRollTypeId == 2).OrderByDescending(u => u.DateCreated);
                        break;
                    case InvoiceRecord.PendingInvoices:
                        invoices = invoices.Where(invoice => invoice.StatusId == (int)Statuses.SUBMITTED && invoice.EmployeeInformation.PayRollTypeId == 1).OrderByDescending(u => u.DateCreated);
                        break;
                    case InvoiceRecord.ProcessedInvoices:
                        invoices = invoices.Where(invoice => invoice.StatusId != (int)Statuses.PENDING && invoice.StatusId != (int)Statuses.SUBMITTED && invoice.PaymentPartnerId == null && invoice.EmployeeInformation.PayRollTypeId == 1).OrderByDescending(u => u.DateCreated);
                        break;
                    case InvoiceRecord.PaymentPartnerInvoices:
                        invoices = invoices.Where(x => x.PaymentPartnerId != null && x.PayrollGroupId == model.PayrollGroupId).OrderByDescending(u => u.DateCreated);
                        break;
                    case InvoiceRecord.ClientInvoices:
                        invoices = invoices.Where(x => x.InvoiceTypeId == (int)InvoiceTypes.CLIENT).OrderByDescending(u => u.DateCreated);
                        break;
                    default:
                        break;
                }

                var invoiceList = invoices.ToList();
                var workbook = _dataExport.ExportInvoiceRecords(model.Record, invoiceList, model.rowHeaders);
                return StandardResponse<byte[]>.Ok(workbook);
            }
            catch(Exception e)
            {
                return StandardResponse<byte[]>.Error(e.Message);
            }  
        }

        private void GeneratePaySlip(Guid invoiceId)
        {
            var invoice = _invoiceRepository.Query().Include(u => u.EmployeeInformation).ThenInclude(v => v.User)
                .FirstOrDefault(invoice => invoice.Id == invoiceId);

            var paySlip = new PaySlip
            {
                EmployeeInformationId = invoice.EmployeeInformationId,
                StartDate = invoice.StartDate,
                EndDate = invoice.EndDate,
                TotalHours = invoice.TotalHours,
                TotalAmount = invoice.TotalAmount,
                Rate = invoice.Rate?.ToString() ?? null,
                PaymentDate = DateTime.Now,
                InvoiceId = invoiceId
            };
            _paySlipRepository.CreateAndReturn(paySlip);
        }
    }
}
