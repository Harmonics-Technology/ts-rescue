using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using KissLog;
using KissLog.AspNetCore;
using KissLog.CloudListeners.Auth;
using KissLog.CloudListeners.RequestLogsListener;
using KissLog.Formatters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SendGrid.Extensions.DependencyInjection;
using Serilog;
using Serilog.Sinks.MariaDB.Extensions;
using Stripe;
using TimesheetBE.Context;
using TimesheetBE.Filters;
using TimesheetBE.Models.IdentityModels;
using TimesheetBE.Models.SeederModels;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Repositories;
using TimesheetBE.Repositories.Interfaces;
using TimesheetBE.Services;
using TimesheetBE.Services.Abstractions;
using TimesheetBE.Services.ConnectedServices.Stripe;
using TimesheetBE.Services.HostedServices;
using TimesheetBE.Services.Interfaces;
using TimesheetBE.Utilities;
using TimesheetBE.Utilities.Abstrctions;
using TimesheetBE.Utilities.Constants;
using TimesheetBE.Utilities.Extentions;
using Application = KissLog.CloudListeners.Auth.Application;
using InvoiceService = TimesheetBE.Services.InvoiceService;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ApplicationName = typeof(Program).Assembly.FullName,
    ContentRootPath = Path.GetFullPath(Directory.GetCurrentDirectory()),
    WebRootPath = Directory.GetCurrentDirectory(),
    Args = args
});

var connectionString = Environment.GetEnvironmentVariable("DbConnect");

var Configuration = builder.Configuration;
Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.MariaDB(
                connectionString: connectionString)
            .CreateLogger();

Log.Information("Logger works");

builder.Host.UseSerilog();
var services = builder.Services;
var serverVersion = new MySqlServerVersion(new Version(8, 0, 27));
var assembly = typeof(Program).Assembly.GetName().Name;
var AppSettingsSection = builder.Configuration.GetSection("AppSettings");
builder.Services.Configure<Globals>(AppSettingsSection);
builder.Services.Configure<PagingOptions>(builder.Configuration.GetSection("DefaultPagingOptions"));

var AppSettings = AppSettingsSection.Get<Globals>();
var Key = Encoding.ASCII.GetBytes(AppSettings.Secret);

builder.Services.AddSingleton<IConfiguration>(provider => builder.Configuration);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    //var connectionString = "server=proinsightdev.mysql.database.azure.com;userid=proinsightdev;password=@p@55word!;database=timesheetbe;";
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), b => b.MigrationsAssembly(assembly)).UseCamelCaseNamingConvention();
    options.UseOpenIddict<int>();
});

//try to initialize npoco so i do not have to pass connection string anywhere in the code
//services.AddNpoco()

builder.Services.AddLogging(logging =>
{
    logging.AddKissLog(options =>
    {
        options.Formatter = (FormatterArgs args) =>
        {
            if (args.Exception == null)
                return args.DefaultValue;

            string exceptionStr = new ExceptionFormatter().Format(args.Exception, args.Logger);

            return string.Join(Environment.NewLine, new[] { args.DefaultValue, exceptionStr });
        };
    });
});


builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 0;
    options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Name;
    options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
    options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
});

builder.Services.AddIdentity<User, Role>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

})
    .AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Key),
            ValidateIssuer = false,
            ValidateAudience = false
        };

        x.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                // If the request is for our hub...
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/chat"))
                {
                    // Read the token out of the query string
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthentication();




builder.Services.AddAuthorization();

AddIdentityCoreServices(services);
//Configure app dependencies
ConfigureServices(services);

builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddControllers().AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
);

//builder.Services.AddReverseProxy()
//    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddMvc(options =>
{
    options.Filters.Add<LinkRewritingFilter>();
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TimesheetBE", Version = "V1" });
    c.OperationFilter<SwaggerHeaderFilter>();

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        },
        // {
        //    new OpenApiSecurityScheme
        //    {
        //        Reference = new OpenApiReference
        //        {
        //            Type = ReferenceType.SecurityScheme,
        //            Id = "Api-Key"
        //        }
        //    },
        //    Array.Empty<string>()
        //}
    });
});



builder.Services.AddHttpContextAccessor();

builder.Services.AddSendGrid(options =>
{
    options.ApiKey = UtilityConstants.SendGridApiKey;
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
        policy.AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TimesheetBE");
});

app.UseKissLogMiddleware(options => ConfigureKissLog(options));

app.UseHttpsRedirection();

app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true) // allow any origin
    .AllowCredentials());

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});


using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var _userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
    var _userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var _roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
    var _userService = scope.ServiceProvider.GetRequiredService<IUserService>();

    // use context
    new SeedData(context, _userRepository, _userManager, _roleManager, _userService).SeedInitialData();
}


var _conteext = app.Services.GetRequiredService<IHttpContextAccessor>();

app.Run();


static void AddIdentityCoreServices(IServiceCollection services)
{
    var builder = services.AddIdentityCore<User>();
    builder = new IdentityBuilder(
        builder.UserType,
        typeof(Role),
        builder.Services
    );

    builder.AddRoles<Role>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders()
        .AddSignInManager<SignInManager<User>>();
}

void ConfigureServices(IServiceCollection services)
{
    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<TokenService>();
    services.AddScoped<CustomerService>();
    services.AddScoped<ChargeService>();
    services.AddScoped<CardService>();
    services.AddScoped<IStripeService, StripeService>();
    services.AddTransient<IUserService, UserService>();
    services.AddTransient<IEmailHandler, EmailHandler>();
    services.AddTransient<IUtilityMethods, UtilityMethods>();
    services.AddTransient<ICodeProvider, CodeProvider>();
    services.AddTransient<IDataExport, DataExport>();
    services.AddTransient<IExpenseTypeRepository, ExpenseTypeRepository>();
    services.AddTransient<IExpenseTypeService, ExpenseTypeService>();
    services.AddTransient<IDashboardService, DashboardService>();
    services.AddTransient<IEmployeeInformationRepository, EmployeeInformationRepository>();
    services.AddTransient<IContractRepository, ContractRepository>();
    services.AddTransient<IContractService, ContractService>();
    services.AddTransient<ITimeSheetRepository, TimeSheetRepository>();
    services.AddTransient<ITimeSheetService, TimeSheetService>();
    services.AddTransient<IExpenseRepository, ExpenseRepository>();
    services.AddTransient<IExpenseService, ExpenseService>();
    services.AddTransient<IPayrollRepository, PayrollRepository>();
    services.AddTransient<IPayrollService, PayrollService>();
    services.AddTransient<IPaySlipRepository, PaySlipRepository>();
    services.AddTransient<IInvoiceRepository, InvoiceRepository>();
    services.AddTransient<IInvoiceService, InvoiceService>();
    services.AddTransient<IPaymentScheduleRepository, PaymentScheduleRepository>();
    services.AddTransient<INotificationRepository, NotificationRepository>();
    services.AddTransient<INotificationService, NotificationService>();
    services.AddTransient<IReminderService, ReminderService>();
    services.AddTransient<IPaySlipService, PaySlipService>();
    services.AddTransient<IOnboardingFeeRepository, OnboardingFeeRepository>();
    services.AddTransient<IOnboardingFeeService, OnboardingFeeService>();
    services.AddTransient<ILeaveTypeRepository, LeaveTypeRepository>();
    services.AddTransient<ILeaveRepository, LeaveRepository>();
    services.AddTransient<ILeaveService, LeaveService>();
    services.AddTransient<IShiftRepository, ShiftRepository>();
    services.AddTransient<IShiftService, ShiftService>();
    services.AddTransient<ISwapRepository, SwapRepository>();
    services.AddTransient<ILeaveConfigurationRepository, LeaveConfigurationRepository>();
    services.AddTransient<IShiftTypeRepository, ShiftTypeRepository>();
    services.AddTransient<IControlSettingRepository, ControlSettingRepository>();
    services.AddTransient<IProjectRepository, ProjectRepository>();
    services.AddTransient<IProjectTaskRepository, ProjectTaskRepository>();
    services.AddTransient<IProjectSubTaskRepository, ProjectSubTaskRepository>();
    services.AddTransient<IProjectTimesheetRepository, ProjectTimesheetRepository>();
    services.AddTransient<IProjectTaskAsigneeRepository, ProjectTaskAsigneeRepository>();
    services.AddTransient<IProjectManagementService, ProjectManagementService>();
    services.AddTransient<IUtilityService, UtilityService>();
    services.AddTransient<IUserDraftRepository, UserDraftRepository>();
    services.AddTransient<IUserDraftService, UserDraftService>();
    services.AddTransient<IClientSubscriptionDetailRepository, ClientSubscriptionDetailRepository>();
    services.AddTransient<IProjectManagementSettingRepository, ProjectManagementSettingRepository>();
    services.AddTransient<IDepartmentRepository, DepartmentRepository>();
    services.AddTransient<IDepartmentService, DepartmentService>();
    services.AddTransient<ICountryRepository, CountryRepository>();
    services.AddSingleton(typeof(ICustomLogger<>), typeof(CustomLogger<>));
    //services.AddHostedService<TimeSheetReminderService>();
    services.AddHostedService<InvoiceGenerator>();
    services.AddHostedService<ClientInvoiceGenerator>();
    services.AddHostedService<UpdateContractStatus>();
    services.AddHostedService<PaymentScheduleGenerator>();
    services.AddHostedService<NotificationBackgroundService>();
}



void ConfigureKissLog(IOptionsBuilder options)
{
    KissLogConfiguration.Listeners.Add(new RequestLogsApiListener(new Application(
        Configuration["KissLog.OrganizationId"],
        Configuration["KissLog.ApplicationId"])
    )
    {
        ApiUrl = Configuration["KissLog.ApiUrl"]
    });
}
