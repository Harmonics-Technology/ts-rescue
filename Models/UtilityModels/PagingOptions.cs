using System;
using System.ComponentModel.DataAnnotations;

namespace TimesheetBE.Models.UtilityModels
{
    public class PagingOptions
    {
        public int? Offset { get; set; }

        [Range(1, 100, ErrorMessage = "Limit must be greater than 0 and less than 100.")]
        public int? Limit { get; set; }

        public PagingOptions Replace(PagingOptions newer)
        {

            if (this.Limit == null || this.Limit <= 0)
            {
                this.Limit = newer.Limit;
                this.Offset = newer.Offset;
                return this;
            }
            else
            {
                return this;
            };
            // return new PagingOptions
            // {
            //     Offset = (!this.Offset.HasValue || this.Offset <= 0 ) ? newer.Offset : this.Offset,
            //     Limit = (!this.Limit.HasValue || this.Limit <= 0) ? newer.Limit : this.Limit 
            // };
        }
    }
}

