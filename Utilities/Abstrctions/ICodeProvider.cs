using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Utilities.Abstrctions
{
    public interface ICodeProvider
    {
        public Code New(Guid userId, string key, int expiryInMinutes = 2880, int length = 6, string prefix = "", string suffix = "");

        public Code GetByCodeString(string code);

        public bool SetExpired(Code code);
        //Code GetById(Guid id);
        List<Code> ToList();
        IEnumerable<Code> GetByUserId(Guid Id);
        IEnumerable<Code> GetAll();
        Code Update(Code code);
    }
}
