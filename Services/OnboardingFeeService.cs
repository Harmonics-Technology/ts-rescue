using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using TimesheetBE.Controllers;
using TimesheetBE.Models;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Repositories;
using TimesheetBE.Repositories.Interfaces;
using TimesheetBE.Services.Interfaces;
using TimesheetBE.Utilities;
using TimesheetBE.Utilities.Abstrctions;

namespace TimesheetBE.Services
{
    public class OnboardingFeeService : IOnboardingFeeService
    {
        private readonly IOnboardingFeeRepository _onboradingFeeRepository;
        private readonly ICustomLogger<OnboardingFeeService> _logger;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IMapper _mapper;
        public OnboardingFeeService(IOnboardingFeeRepository onboradingFeeRepository, IConfigurationProvider configurationProvider, ICustomLogger<OnboardingFeeService> logger, IMapper mapper)
        {
            _onboradingFeeRepository = onboradingFeeRepository;
            _configurationProvider = configurationProvider;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<StandardResponse<OnboardingFeeModel>> AddOnboardingFee(OnboardingFeeModel model)
        {
            try
            {
                if (!model.SuperAdminId.HasValue) return StandardResponse<OnboardingFeeModel>.Failed("Super admin required");
                //check if onborading fee type is fixed amount
                //if(model.OnboardingType.ToLower() == "fixedamount")
                //{
                //    var fixedAmount = _onboradingFeeRepository.Query().FirstOrDefault(x => x.OnboardingFeeType.ToLower() == "fixedamount" && x.SuperAdminId == model.SuperAdminId);
                //    if(fixedAmount != null)
                //    {
                //        fixedAmount.Fee = model.Fee;
                //        _onboradingFeeRepository.Update(fixedAmount);
                //        return StandardResponse<OnboardingFeeModel>.Ok(model);
                //    } 
                //}

                //check if onboarding fee type is HST
                if (model.OnboardingType.ToLower() == "hst")
                {
                    var hst = _onboradingFeeRepository.Query().FirstOrDefault(x => x.OnboardingFeeType.ToLower() == "hst" && x.SuperAdminId == model.SuperAdminId);
                    if (hst != null)
                    {
                        hst.Fee = model.Fee;
                        _onboradingFeeRepository.Update(hst);
                        return StandardResponse<OnboardingFeeModel>.Ok(model);
                    }
                }

                var onBordingFee = new OnboardingFee
                {
                    SuperAdminId = model.SuperAdminId,
                    Fee = model.Fee,
                    OnboardingFeeType = model.OnboardingType.ToLower(),
                    Currency = model.Currency
                };
                _onboradingFeeRepository.CreateAndReturn(onBordingFee);
                return StandardResponse<OnboardingFeeModel>.Ok(model);
            }
            catch(Exception ex)
            {
                _logger.Error<bool>(_logger.GetMethodName(), ex);
                return StandardResponse<OnboardingFeeModel>.Error(ex.Message);
            }
        }

        public async Task<StandardResponse<bool>> RemoveOnboardingFee(Guid id)
        {
            try
            {
                var fee = _onboradingFeeRepository.Query().FirstOrDefault(x => x.Id == id);
                if (fee == null)
                    return StandardResponse<bool>.NotFound("Fee does not exists");

                _onboradingFeeRepository.Delete(fee);


                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                return _logger.Error<bool>(_logger.GetMethodName(), ex);
            }

        }

        public async Task<StandardResponse<PagedCollection<OnboardingFeeView>>> GetPercentageOnboardingFees(PagingOptions pagingOptions, Guid superAdminId)
        {
            try
            {
                var fees = _onboradingFeeRepository.Query().Where(x => x.OnboardingFeeType.ToLower() == "percentage" && x.SuperAdminId == superAdminId);

                var total = fees.Count();
                var paginatedFess = fees.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedFees = fees.ProjectTo<OnboardingFeeView>(_configurationProvider).ToArray();

                var result = PagedCollection<OnboardingFeeView>.Create(Link.ToCollection(nameof(OnboardingFeeController.ListPercentageOnboardingFees)), mappedFees, total, pagingOptions);

                return StandardResponse<PagedCollection<OnboardingFeeView>>.Ok(result);
            }
            catch (Exception ex)
            {
                return StandardResponse<PagedCollection<OnboardingFeeView>>.Error(ex.Message);
            }
            

        }

        public async Task<StandardResponse<PagedCollection<OnboardingFeeView>>> ListFixedAmountFee(PagingOptions pagingOptions, Guid superAdminId)
        {
            try
            {
                var fees = _onboradingFeeRepository.Query().Where(x => x.OnboardingFeeType.ToLower() == "fixedamount" && x.SuperAdminId == superAdminId);
                var total = fees.Count();
                var paginatedFess = fees.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedFees = fees.ProjectTo<OnboardingFeeView>(_configurationProvider).ToArray();

                var result = PagedCollection<OnboardingFeeView>.Create(Link.ToCollection(nameof(OnboardingFeeController.ListFixedAmountFee)), mappedFees, total, pagingOptions);

                return StandardResponse<PagedCollection<OnboardingFeeView>>.Ok(result);
            }
            catch (Exception ex)
            {
                return StandardResponse<PagedCollection<OnboardingFeeView>>.Error(ex.Message);
            }


        }

        public async Task<StandardResponse<OnboardingFeeView>> GetHST(Guid superAdminId)
        {
            try
            {
                var fee = _onboradingFeeRepository.Query().FirstOrDefault(x => x.OnboardingFeeType.ToLower() == "hst" && x.SuperAdminId == superAdminId);

                return StandardResponse<OnboardingFeeView>.Ok(_mapper.Map<OnboardingFeeView>(fee));


            }
            catch (Exception ex)
            {
                return StandardResponse<OnboardingFeeView>.Failed(ex.Message);
            }


        }
    }
}
