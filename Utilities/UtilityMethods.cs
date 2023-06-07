using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TimesheetBE.Utilities.Abstrctions;
using Microsoft.Extensions.Logging;
using System.Globalization;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.AppModels;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace TimesheetBE.Utilities
{
    public class UtilityMethods : IUtilityMethods
    {
        private readonly ILogger<UtilityMethods> _logger;

        public UtilityMethods(ILogger<UtilityMethods> logger)
        {
            _logger = logger;
        }

        public string RandomCode(int size)
        {
            try
            {
                char[] chars = new char[62];
                chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
                byte[] data = new byte[1];
                using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
                {
                    crypto.GetNonZeroBytes(data);
                    data = new byte[size];
                    crypto.GetNonZeroBytes(data);

                }
                StringBuilder Result = new StringBuilder(size);
                foreach (byte b in data)
                {
                    Result.Append(chars[b % (chars.Length)]);
                }
                return Result.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return null;
        }

        public string FormattedDate(DateTime thisDate)
        {
            try
            {
                if (thisDate != DateTime.MaxValue && thisDate != DateTime.MinValue)
                {
                    return thisDate.ToShortDateString() + ":" + thisDate.ToShortTimeString();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return "";
        }

        public string GetUniqueFileName(string fileName)
        {
            fileName = Path.GetFileName(fileName);
            return Path.GetFileNameWithoutExtension(fileName) + "_" + Guid.NewGuid().ToString().Substring(5, 5) + Path.GetExtension(fileName);
        }

        public override bool Equals(object obj)
        {
            return obj is UtilityMethods methods &&
                   EqualityComparer<ILogger<UtilityMethods>>.Default.Equals(_logger, methods._logger);
        }

        public string GetMonthName(int month)
        {
            return CultureInfo.CurrentCulture.
            DateTimeFormat.GetMonthName
            (month);
        }

        public IQueryable<T> ApplyFilter<T> (IQueryable<T> query, FilterOptions options) where T : BaseModel
        {
            // if (options.Status != null)
            // {
            //     int positionId = (int)options.Status;
            //     query = query.Where(x => x.StatusId == positionId);
            // }
            if (options.Order != null)
            {
                if (options.Order == OrderType.Ascending)
                {
                    query = query.OrderBy(x => x.DateCreated);
                }
                else
                {
                    query = query.OrderByDescending(x => x.DateCreated);
                }
            }
            return query;
        }
    }
}
