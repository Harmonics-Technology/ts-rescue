using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using TimesheetBE.Controllers;
using TimesheetBE.Models;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Repositories.Interfaces;
using TimesheetBE.Services.Interfaces;
using TimesheetBE.Utilities;

namespace TimesheetBE.Services
{
    public class PaySlipService : IPaySlipService
    {
        private readonly IPaySlipRepository _paySlipRepository;
        private readonly IMapper _mapper;
        private readonly IConfigurationProvider _configuration;


        public PaySlipService(IPaySlipRepository paySlipRepository, IMapper mapper, IConfigurationProvider configuration)
        {
            _paySlipRepository = paySlipRepository;
            _mapper = mapper;
            _configuration = configuration;
        }

        // Get Payslip for team member
        public async Task<StandardResponse<PagedCollection<PayslipUserView>>> GetTeamMembersPaySlips(Guid EmployeeInformationId, PagingOptions options, string search = null, DateFilter dateFilter = null, int? payrollTypeFilter = null)
        {
            try
            {
                var paySlips = _paySlipRepository.Query().Include(x => x.EmployeeInformation).Where(x => x.EmployeeInformationId == EmployeeInformationId).OrderByDescending(x => x.DateCreated).AsQueryable();

                if (dateFilter.StartDate.HasValue)
                    paySlips = paySlips.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

                if (dateFilter.EndDate.HasValue)
                    paySlips = paySlips.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);

                if(payrollTypeFilter.HasValue)
                    paySlips = paySlips.Where(u => u.EmployeeInformation.PayRollTypeId == payrollTypeFilter.Value).OrderByDescending(u => u.DateCreated);

                if (!string.IsNullOrEmpty(search))
                    paySlips = paySlips.Where(x => x.EmployeeInformation.User.FirstName.Contains(search) || x.EmployeeInformation.User.LastName.Contains(search)
                    || (x.EmployeeInformation.User.FirstName.ToLower() + " " + x.EmployeeInformation.User.LastName.ToLower()).Contains(search.ToLower()));

                var pagedPaySlips = paySlips.Skip(options.Offset.Value).Take(options.Limit.Value).ProjectTo<PaySlipView>(_configuration).ToList();

                var usersPayslip = new List<PayslipUserView>();
                
                foreach(var payslip in pagedPaySlips)
                {
                    var totalEarning = paySlips.Where(payslip => payslip.DateCreated.Year == payslip.DateCreated.Year).Sum(payslip => payslip.TotalAmount);
                    var userPayslip = new PayslipUserView
                    {
                        PayslipView = payslip,
                        TotalEarnings = totalEarning
                    };
                    usersPayslip.Add(userPayslip);
                }

                
                var pagedCollection = PagedCollection<PayslipUserView>.Create(Link.ToCollection(nameof(PaySlipController.GetTeamMembersPaySlips)), usersPayslip.ToArray(), paySlips.Count(), options);
                return StandardResponse<PagedCollection<PayslipUserView>>.Ok(pagedCollection);
            }
            catch (Exception ex)
            {
                return StandardResponse<PagedCollection<PayslipUserView>>.Error(ex.Message);
            }
        }

        // Get all payslips for all team members
        public async Task<StandardResponse<PagedCollection<PayslipUserView>>> GetAllPaySlips(PagingOptions options, string search = null, DateFilter dateFilter = null, int? payrollTypeFilter = null)
        {
            try
            {
                var paySlips = _paySlipRepository.Query().Include(x => x.EmployeeInformation).OrderByDescending(x => x.DateCreated).AsQueryable();

                if (dateFilter.StartDate.HasValue)
                    paySlips = paySlips.Where(u => u.DateCreated.Date >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

                if (dateFilter.EndDate.HasValue)
                    paySlips = paySlips.Where(u => u.DateCreated.Date <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);

                if (payrollTypeFilter.HasValue)
                    paySlips = paySlips.Where(u => u.EmployeeInformation.PayRollTypeId == payrollTypeFilter.Value).OrderByDescending(u => u.DateCreated);

                if (!string.IsNullOrEmpty(search))
                    paySlips = paySlips.Where(x => x.EmployeeInformation.User.FirstName.Contains(search) || x.EmployeeInformation.User.LastName.Contains(search)
                    || (x.EmployeeInformation.User.FirstName.ToLower() + " " + x.EmployeeInformation.User.LastName.ToLower()).Contains(search.ToLower()));
                
                var pagedPaySlips = paySlips.Skip(options.Offset.Value).Take(options.Limit.Value).ProjectTo<PaySlipView>(_configuration).ToList();

                var usersPayslip = new List<PayslipUserView>();

                foreach (var payslip in pagedPaySlips)
                {
                    var totalEarning = paySlips.Where(x => x.DateCreated.Year == payslip.DateCreated.Year && x.EmployeeInformationId == payslip.EmployeeInformationId).Sum(payslip => payslip.TotalAmount);
                    var userPayslip = new PayslipUserView
                    {
                        PayslipView = payslip,
                        TotalEarnings = totalEarning
                    };
                    usersPayslip.Add(userPayslip);
                }

                var pagedCollection = PagedCollection<PayslipUserView>.Create(Link.ToCollection(nameof(PaySlipController.GetAllPaySlips)), usersPayslip.ToArray(), paySlips.Count(), options);
                return StandardResponse<PagedCollection<PayslipUserView>>.Ok(pagedCollection);
            }
            catch (Exception ex)
            {
                return StandardResponse<PagedCollection<PayslipUserView>>.Error(ex.Message);
            }
        }


    }
}