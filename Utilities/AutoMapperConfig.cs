
using AutoMapper;
using System.Linq;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Models.IdentityModels;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.ViewModels;

namespace TimesheetBE.Utilities
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            CreateMap<RegisterModel, User>();
            CreateMap<User, UserView>()
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.EmployeeInformation.Client.OrganizationName));
            //.ForMember(dest => dest.Client, opt => opt.MapFrom(src => src.Client));
            //CreateMap<UserView, User>();

            CreateMap<User, StrippedUserView>()
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.EmployeeInformation.Client.OrganizationName));


            CreateMap<LoginModel, User>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));

            CreateMap<ExpenseType, ExpenseTypeView>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.Name));

            CreateMap<TeamMemberModel, User>().ForAllMembers(options => options.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<TeamMemberModel, EmployeeInformation>();
            CreateMap<TeamMemberModel, Contract>();
            CreateMap<TeamMemberModel, RegisterModel>();
            CreateMap<TimeSheet, TimeSheetMonthlyView>();
            CreateMap<TimeSheet, TimeSheetView>().ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.Name));

            CreateMap<Contract, ContractView>().ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.Name))
            .ForMember(dest => dest.Tenor, opt => opt.MapFrom(src => src.EndDate.Subtract(src.StartDate).Days))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.EmployeeInformation.User.FullName))
            .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(src => src.EmployeeInformation.JobTitle))
            .ForMember(dest => dest.Document, opt => opt.MapFrom(src => src.EmployeeInformation.InCorporationDocumentUrl))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.EmployeeInformation.UserId));


            CreateMap<EmployeeInformation, EmployeeInformationView>()
            .ForMember(dest => dest.PayrollType, opt => opt.MapFrom(src => src.PayrollType.Name))
            //.ForMember(dest => dest.PayrollGroup, opt => opt.MapFrom(src => src.PayrollGroup.Name))
            .ForMember(dest => dest.Client, opt => opt.MapFrom(src => src.Client));

            CreateMap<ContractModel, Contract>();
            CreateMap<ExpenseModel, Expense>();
            CreateMap<Payroll, PayrollView>();
            CreateMap<Expense, ExpenseView>()
            .ForMember(dest => dest.ExpenseType, opt => opt.MapFrom(src => src.ExpenseType.Name))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.Name));

            CreateMap<Invoice, InvoiceView>().ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.Name))
            .ForMember(dest => dest.InvoiceType, opt => opt.MapFrom(src => src.InvoiceType.Name))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.EmployeeInformation.User.FullName))
            .ForMember(dest => dest.PayrollGroupName, opt => opt.MapFrom(src => src.Client.OrganizationName))
            .ForMember(dest => dest.PaymentPartnerName, opt => opt.MapFrom(src => src.PaymentPartner.OrganizationName));


            CreateMap<PaySlip, PaySlipView>();

            CreateMap<Notification, NotificationView>();

            CreateMap<NotificationModel, Notification>();

            CreateMap<PaymentPartnerInvoiceModel, Invoice>();

            CreateMap<OnboardingFee, OnboardingFeeView>();
            CreateMap<LeaveModel, Leave>();

            CreateMap<Leave, LeaveView>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.Name));

            CreateMap<LeaveTypeModel, LeaveType>();
            CreateMap<LeaveType, LeaveTypeView>();

            CreateMap<ShiftModel, Shift>();
            CreateMap<Shift, ShiftToSwapView>();
            CreateMap<Shift, ShiftView>();

            CreateMap<LeaveConfigurationModel, LeaveConfiguration>();
            CreateMap<LeaveConfiguration, LeaveConfigurationView>();

            CreateMap<ControlSettingModel, ControlSetting>();
            CreateMap<ControlSetting, ControlSettingView>();

            CreateMap<ShiftTypeModel, ShiftType>();
            CreateMap<ShiftType, ShiftTypeView>();


            CreateMap<Swap, SwapView>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.Name));

            CreateMap<ProjectModel, Project>();
            CreateMap<Project, ProjectView>();

            CreateMap<ProjectTaskModel, ProjectTask>();
            CreateMap<ProjectTask, ProjectTaskView>()
                .ForMember(dest => dest.SubTaskCount, opt => opt.MapFrom(src => src.SubTasks.Count()));

            CreateMap<ProjectSubTaskModel, ProjectSubTask>();
            CreateMap<ProjectSubTask, ProjectSubTaskView>()
                .ForMember(dest => dest.HoursSpent, opt => opt.MapFrom(src => src.ProjectTimesheets.Sum(x => x.TotalHours))); 

            CreateMap<ProjectTimesheetModel, ProjectTimesheet>();

            CreateMap<ProjectTimesheet, ProjectTimesheetView>()
                 .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.Name));

            CreateMap<ProjectTaskAsignee, ProjectTaskAsigneeView>();

            CreateMap<UserDraftModel, UserDraft>();
            CreateMap<UserDraft, UserDraftView>();

            CreateMap<ClientSubscriptionDetail, ClientSubscriptionDetailView>();

            CreateMap<ProjectManagementSetting, ProjectManagementSettingView>();

            CreateMap<Department, DepartmentView>();

            CreateMap<Country, CountryView>();
        }
    }
}
