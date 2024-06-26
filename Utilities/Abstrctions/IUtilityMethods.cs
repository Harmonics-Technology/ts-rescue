﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Models.InputModels;

namespace TimesheetBE.Utilities.Abstrctions
{
    public interface IUtilityMethods
    {
        string RandomCode(int size);

        string FormattedDate(DateTime thisDate);

        string GetUniqueFileName(string fileName);
        string GetMonthName(int month);
        List<DateTime> GetDatesBetweenTwoDates(DateTime start, DateTime end);
        IQueryable<T> ApplyFilter<T> (IQueryable<T> query, FilterOptions options) where T : BaseModel;
        Task<HttpResponseMessage> MakeHttpRequest(object request, string baseAddress, string requestUri, HttpMethod method, Dictionary<string, string> headers = null);
    }
}
