﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Linq;
using System.Threading.Tasks;
using TimesheetBE.Controllers;
using TimesheetBE.Models;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Repositories.Interfaces;
using TimesheetBE.Services.Interfaces;
using TimesheetBE.Utilities;

namespace TimesheetBE.Services
{
    public class UserDraftService : IUserDraftService
    {
        private readonly IUserDraftRepository _userDraftRepository;
        private readonly IUserRepository _userRepository;
        private readonly IConfigurationProvider _configuration;
        private readonly IMapper _mapper;
        public UserDraftService(IMapper mapper, IUserDraftRepository userDraftRepository, IUserRepository userRepository, IConfigurationProvider configuration)
        {

            _mapper = mapper;
            _userDraftRepository = userDraftRepository;
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<StandardResponse<bool>> CreateDraft(UserDraftModel model)
        {
            try
            {
                var superAdmin = _userRepository.Query().FirstOrDefault(x => x.Id == model.SuperAdminId);

                if (superAdmin == null) return StandardResponse<bool>.NotFound("super admin not found");

                var mappedUserDraft = _mapper.Map<UserDraft>(model);

                var createdDraft = _userDraftRepository.CreateAndReturn(mappedUserDraft);

                return StandardResponse<bool>.Ok();
            }
            catch (Exception ex)
            {
                return StandardResponse<bool>.Error("Error saving draft");
            }
        }

        public async Task<StandardResponse<bool>> UpdateDraft(UserDraftModel model)
        {
            try
            {
                var superAdmin = _userRepository.Query().FirstOrDefault(x => x.Id == model.SuperAdminId);

                if (superAdmin == null) return StandardResponse<bool>.NotFound("super admin not found");

                var draft = _userDraftRepository.Query().FirstOrDefault(x => x.Id == model.Id);

                if(draft == null) return StandardResponse<bool>.NotFound("draft not found");

                draft.FirstName = model.FirstName;
                draft.LastName = model.LastName;
                draft.Email = model.Email;
                draft.Role = model.Role;
                draft.TeammemberId = model.TeammemberId;
                draft.PhoneNumber = model.PhoneNumber;
                draft.Address = model.Address;
                draft.OrganizationName = model.OrganizationName;
                draft.ContactFirstName = model.ContactFirstName;
                draft.ContactLastName = model.ContactLastName;
                draft.ContactEmail = model.ContactEmail;
                draft.ContactPhoneNumber = model.ContactPhoneNumber;
                draft.Frequency = model.Frequency;
                draft.Term = model.Term;
                draft.ClientId = model.ClientId;
                draft.DateOfBirth = model.DateOfBirth;
                draft.ProfileStatus = model.ProfileStatus;
                draft.JobTitle = model.JobTitle;
                draft.SupervisorId = model.SupervisorId;
                draft.EnableFinancials = model.EnableFinancials;
                draft.HoursPerDay = model.HoursPerDay;
                draft.EmployeeType = model.EmployeeType;
                draft.ContractTitle = model.ContractTitle;
                draft.StartDate = model.StartDate;
                draft.EndDate = model.EndDate;
                draft.Document = model.Document;
                draft.IsEligibleForLeave = model.IsEligibleForLeave;
                draft.NumberOfDaysEligible = model.NumberOfDaysEligible;
                draft.NumberOfHoursEligible = model.NumberOfHoursEligible;

                _userDraftRepository.Update(draft);

                return StandardResponse<bool>.Ok();
            }
            catch (Exception ex)
            {
                return StandardResponse<bool>.Error("Error updating draft");
            }
        }

        public async Task<StandardResponse<bool>> DeleteDraft(Guid id)
        {
            try
            {
                var draft = _userDraftRepository.Query().FirstOrDefault(x => x.Id == id);

                if (draft == null) return StandardResponse<bool>.NotFound("draft not found");

                _userDraftRepository.Delete(draft);

                return StandardResponse<bool>.Ok();
            }
            catch (Exception ex)
            {
                return StandardResponse<bool>.Error("Error deleting draft");
            }
        }

        public async Task<StandardResponse<PagedCollection<UserDraftView>>> ListDrafts(PagingOptions pagingOptions, Guid superAdminId, string role)
        {
            try
            {
                var superAdmin = _userRepository.Query().FirstOrDefault(x => x.Id == superAdminId);

                if (superAdmin == null) return StandardResponse<PagedCollection<UserDraftView>>.NotFound("super admin not found");

                var drafts = _userDraftRepository.Query().Where(x => x.SuperAdminId == superAdminId);

                if (role.ToLower() == "admin")
                    drafts = drafts.Where(u => u.Role.ToLower() == "admin" || u.Role.ToLower() == "payroll manager" || u.Role.ToLower() == "internal admin" || u.Role.ToLower() == "internal supervisor" || u.Role.ToLower() == "business manager").OrderByDescending(x => x.DateCreated);
                else if(role.ToLower() == "client")
                    drafts = drafts.Where(u => u.Role.ToLower() == "client").OrderByDescending(x => x.DateCreated);
                else if (role.ToLower() == "supervisor")
                    drafts = drafts.Where(u => u.Role.ToLower() == "supervisor").OrderByDescending(x => x.DateCreated);
                else if (role.ToLower() == "team member")
                    drafts = drafts.Where(u => u.Role.ToLower() == "team member").OrderByDescending(x => x.DateCreated);
                else if (role.ToLower() == "payment partner")
                    drafts = drafts.Where(u => u.Role.ToLower() == "payment partner").OrderByDescending(x => x.DateCreated);

                var pagedDraft = drafts.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var mappedDraft = drafts.AsQueryable().ProjectTo<UserDraftView>(_configuration).ToArray();

                var pagedCollection = PagedCollection<UserDraftView>.Create(Link.ToCollection(nameof(DraftController.ListDrafts)), mappedDraft, drafts.Count(), pagingOptions);

                return StandardResponse<PagedCollection<UserDraftView>>.Ok(pagedCollection);
            }
            catch (Exception ex)
            {
                return StandardResponse<PagedCollection<UserDraftView>>.NotFound("Error listing drafts");
            }
        }
    }
}
