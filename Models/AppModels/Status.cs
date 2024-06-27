using System;
namespace TimesheetBE.Models.AppModels
{
    public class Status
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
    }

    public enum Statuses
    {
        PENDING = 1,
        APPROVED,
        ONGOING,
        RESOLVED,
        VERIFIED,
        DRAFTED,
        ACTIVE,
        INACTIVE,
        REJECTED,
        SOLD,
        COMPLETED,
        ACCEPTED,
        REVIEWED,
        TERMINATED,
        DECLINED,
        INVOICED,
        SUBMITTED,
        REVIEWING,
        NOTSTARTED,
        CANCELED,
        PROCESSED
    }
}

