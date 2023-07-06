﻿using System;
using System.Threading.Tasks;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Utilities;

namespace TimesheetBE.Services.Interfaces
{
    public interface IOnboardingFeeService
    {
        Task<StandardResponse<OnboardingFeeModel>> AddOnboardingFee(OnboardingFeeModel model);
        Task<StandardResponse<bool>> RemoveOnboardingFee(Guid id);
        Task<StandardResponse<PagedCollection<OnboardingFeeView>>> GetPercentageOnboardingFees(PagingOptions pagingOptions, Guid superAdminId);
        Task<StandardResponse<OnboardingFeeView>> GetFixedAmountFee(Guid superAdminId);
        Task<StandardResponse<OnboardingFeeView>> GetHST(Guid superAdminId);
    }
}
