using TimesheetBE.Models.AppModels;
using TimesheetBE.Models.IdentityModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace TimesheetBE.Context
{
    public class AppDbContext : IdentityDbContext<User, Role, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Role>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

            modelBuilder.Entity<User>().HasOne(u => u.EmployeeInformation).WithOne(e => e.User).HasForeignKey<EmployeeInformation>(e => e.UserId);
            //modelBuilder.Entity<EmployeeInformation>().HasOne(u => u.User).WithOne(e => e.EmployeeInformation).HasForeignKey<User>(e => e.EmployeeInformationId);

            //add this when this run
            modelBuilder.Entity<User>().HasOne(u => u.CreatedBy).WithOne().HasForeignKey<User>(e => e.CreatedById);
            //modelBuilder.Entity<User>().HasOne(u => u.SuperAdmin).WithOne().HasForeignKey<User>(e => e.SuperAdminId);


            //delete if below works
            //modelBuilder.Entity<User>().HasOne(u => u.SuperAdmin).WithOne().HasForeignKey<User>(e => e.SuperAdminId);
            //modelBuilder.Entity<User>().HasOne(u => u.SuperAdmin).WithOne().HasForeignKey<User>(e => e.SuperAdminId);
            //commented already
            //modelBuilder.Entity<EmployeeInformation>().HasOne(e => e.SuperAdmin).WithMany(u => u.SuperAdminTeamMembers).HasForeignKey(c => c.SuperAdminId);
            modelBuilder.Entity<EmployeeInformation>().HasOne(e => e.Client).WithMany(u => u.TeamMembers).HasForeignKey(c => c.ClientId);
            modelBuilder.Entity<EmployeeInformation>().HasOne(e => e.Supervisor).WithMany(u => u.Supervisees).HasForeignKey(e => e.SupervisorId);
            modelBuilder.Entity<EmployeeInformation>().HasOne(e => e.PaymentPartner).WithMany(u => u.Payees).HasForeignKey(c => c.PaymentPartnerId);
            modelBuilder.Entity<User>().HasOne(u => u.Client).WithMany(u => u.Supervisors).HasForeignKey(u => u.ClientId);
            //modelBuilder.Entity<User>().HasOne(u => u.SuperAdmin).WithMany(u => u.SuperAdminAdmins).HasForeignKey(u => u.SuperAdminId);

            modelBuilder.Entity<Expense>().HasOne(e => e.Invoice).WithMany(u => u.Expenses).HasForeignKey(e => e.InvoiceId);

            modelBuilder.Entity<Invoice>().HasOne(e => e.Parent).WithMany(u => u.Children).HasForeignKey(e => e.ParentId);

            modelBuilder.Entity<Invoice>().HasOne(e => e.ClientInvoice).WithMany(u => u.ClientInvoiceChildren).HasForeignKey(e => e.ClientInvoiceId);

            //modelBuilder.Entity<Shift>().HasOne(e => e.Swap).WithMany(u => u.Shifts).HasForeignKey(e => e.SwapId);

        }

        public DbSet<Code> Codes { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<ExpenseType> ExpenseTypes { get; set; }
        public DbSet<EmployeeInformation> EmployeeInformation { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<PayRollType> PayRollTypes { get; set; }
        public DbSet<TimeSheet> TimeSheets { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<PaySlip> PaySlips { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceType> InvoiceTypes { get; internal set; }
        public DbSet<PaymentSchedule> PaymentSchedules { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<OnboardingFee> OnboardingFees { get; set; }
        public DbSet<PayrollGroup> PayrollGroups { get; set; }
        public DbSet<LeaveType> LeaveTypes { get; set; }
        public DbSet<Leave> Leaves { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<Swap> Swaps { get; set; }
        public DbSet<ShiftType> ShiftTypes { get; set; }
        public DbSet<LeaveConfiguration> LeaveConfigurations { get; set; }
        public DbSet<ControlSetting> ControlSettings { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectTask> projectTasks { get; set; }
        public DbSet<ProjectSubTask> projectSubTasks { get; set; }
        public DbSet<ProjectTimesheet> projectTimesheets { get; set; }
        public DbSet<ProjectTaskAsignee> projectTaskAsignees { get; set; }

    }

}

