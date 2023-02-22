using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
                //check if onborading fee type is fixed amount
                if(model.OnboardingTypeId == 2)
                {
                    var fixedAmount = _onboradingFeeRepository.Query().FirstOrDefault(x => x.OnboardingFeeTypeId == 2);
                    if(fixedAmount != null)
                    {
                        fixedAmount.Fee = model.Fee;
                        _onboradingFeeRepository.Update(fixedAmount);
                        return StandardResponse<OnboardingFeeModel>.Ok(model);
                    } 
                }

                //check if onboarding fee type is HST
                if (model.OnboardingTypeId == 3)
                {
                    var hst = _onboradingFeeRepository.Query().FirstOrDefault(x => x.OnboardingFeeTypeId == 3);
                    if (hst != null)
                    {
                        hst.Fee = model.Fee;
                        _onboradingFeeRepository.Update(hst);
                        return StandardResponse<OnboardingFeeModel>.Ok(model);
                    }
                }

                var onBordingFee = new OnboardingFee
                {
                    Fee = model.Fee,
                    OnboardingFeeTypeId = model.OnboardingTypeId
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

        public async Task<StandardResponse<PagedCollection<OnboardingFeeView>>> GetPercentageOnboardingFees(PagingOptions pagingOptions)
        {
            try
            {
                var fees = _onboradingFeeRepository.Query().Where(x => x.OnboardingFeeTypeId == 1);

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

        public async Task<StandardResponse<OnboardingFeeView>> GetFixedAmountFee()
        {
            try
            {
                var fee = _onboradingFeeRepository.Query().FirstOrDefault(x => x.OnboardingFeeTypeId == 2);

                return StandardResponse<OnboardingFeeView>.Ok(_mapper.Map<OnboardingFeeView>(fee));

                
            }
            catch (Exception ex)
            {
                return StandardResponse<OnboardingFeeView>.Failed(ex.Message);
            }


        }

        public async Task<StandardResponse<OnboardingFeeView>> GetHST()
        {
            try
            {
                var fee = _onboradingFeeRepository.Query().FirstOrDefault(x => x.OnboardingFeeTypeId == 3);

                return StandardResponse<OnboardingFeeView>.Ok(_mapper.Map<OnboardingFeeView>(fee));


            }
            catch (Exception ex)
            {
                return StandardResponse<OnboardingFeeView>.Failed(ex.Message);
            }


        }
    }
}
