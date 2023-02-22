using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimesheetBE.Context;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Repositories;
using TimesheetBE.Repositories.Interfaces;
using TimesheetBE.Utilities.Abstrctions;
using Microsoft.Extensions.Logging;

namespace TimesheetBE.Utilities
{
    public class CodeProvider : BaseRepository<Code>, ICodeProvider
    {
        private readonly IUtilityMethods _utilityMethods;
        private readonly ILogger<CodeProvider> _logger;

        public CodeProvider(AppDbContext context, IUtilityMethods utilityMethods, ILogger<CodeProvider> logger) : base(context)
        {
            _utilityMethods = utilityMethods;
            _logger = logger;
        }

        public Code New(Guid userId, string key, int expiryInMinutes = 2880, int length = 6, string prefix = "", string suffix = "")
        {
            try
            {
                var NewCode = new Code
                {
                    UserId = string.IsNullOrEmpty(userId.ToString()) || userId == Guid.Empty ? null : userId,
                    Key = key ?? "",
                    CodeString = prefix.ToLower() + _utilityMethods.RandomCode(length).ToLower() + suffix.ToLower(),
                    ExpiryDate = DateTime.Now.AddMinutes(expiryInMinutes),
                    DateCreated = DateTime.Now,
                    DateModified = DateTime.Now
                };
                while ((GetByCodeString(NewCode.CodeString) != null) || NewCode.CodeString == null)
                {
                    NewCode.CodeString = prefix + _utilityMethods.RandomCode(length) + suffix;
                }

                NewCode = CreateAndReturn(NewCode);

                return NewCode;


            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return null;
            }
        }

        public Code GetByCodeString(string code)
        {
            return GetAll().FirstOrDefault(c => c.CodeString == code);
        }

        public IEnumerable<Code> GetByUserId(Guid Id)
        {
            var codes = ToList();
            var userCodes = codes.Where(c => c.UserId == Id);
            return userCodes;
        } 

        public bool SetExpired(Code code)
        {
            try
            {
                code.ExpiryDate = DateTime.Now;
                code.IsExpired = true;
                Update(code);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return true;
            }
        }

        // public bool Update(Code thisCode)
        // {
        //     try
        //     {
        //         Update(thisCode);
        //         return true;
        //     }
        //     catch(Exception e)
        //     {
        //         Logger.Error(e);
        //         return false;
        //     }
        // }
    }
}
