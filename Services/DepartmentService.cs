using AutoMapper;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using System;
using Throw;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Repositories;
using TimesheetBE.Repositories.Interfaces;
using TimesheetBE.Utilities.Extentions;
using TimesheetBE.Utilities;
using System.Collections.Generic;
using TimesheetBE.Models.ViewModels;
using AutoMapper.QueryableExtensions;
using TimesheetBE.Services.Interfaces;

namespace TimesheetBE.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IMapper _mapper;
        private readonly IConfigurationProvider _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserRepository _userRepository;
        public DepartmentService(IDepartmentRepository departmentRepository, IMapper mapper, IConfigurationProvider configuration, IHttpContextAccessor httpContextAccessor, 
            IUserRepository userRepository)
        {
            _departmentRepository = departmentRepository;
            _mapper = mapper;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _userRepository = userRepository;
        }

        public async Task<StandardResponse<bool>> CreateDepartment(Guid superAdminId, string name)
        {
            try
            {
                //Guid UserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();

                var user = _userRepository.Query().FirstOrDefault(x => x.Id == superAdminId);

                if(user == null) return StandardResponse<bool>.Failed("User not found");

                name.Throw().IfEmpty().IfEquals(null);

                var existingDepartment = _departmentRepository.Query().Where(x => x.Name.ToLower() == name.ToLower());

                if(existingDepartment != null) return StandardResponse<bool>.Failed("Department already exist");

                var newDepartment = new Department
                {
                    SuperAdminId = superAdminId,
                    Name = name
                };

                _departmentRepository.CreateAndReturn(newDepartment);

                return StandardResponse<bool>.Ok();

            }
            catch (System.Exception ex)
            {
                return StandardResponse<bool>.Error(ex.Message);
            }
        }

        public async Task<StandardResponse<IEnumerable<DepartmentView>>> ListDepartments(Guid SuperAdminId)
        {
            try
            {
                var departments = _departmentRepository.Query().Where(x => x.SuperAdminId == SuperAdminId).OrderByDescending(u => u.DateCreated);

                var mappedView = departments.ProjectTo<DepartmentView>(_configuration).ToList();

                return StandardResponse<IEnumerable<DepartmentView>>.Ok(mappedView);

            }
            catch (Exception ex)
            {
                return StandardResponse<IEnumerable<DepartmentView>>.Failed("An error occured");
            }
        }

        public async Task<StandardResponse<bool>> DeleteDepartment(Guid departmentId)
        {
            try
            {
                var department = _departmentRepository.Query().FirstOrDefault(x => x.Id == departmentId);

                if(department == null) return StandardResponse<bool>.Failed("department not found");

                _departmentRepository.Delete(department);

                return StandardResponse<bool>.Ok();

            }
            catch (Exception ex)
            {
                return StandardResponse<bool>.Failed("An error occured");
            }
        }
    }
}
