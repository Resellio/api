using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TickAPI.Admins.Abstractions;
using TickAPI.Admins.Repositories;
using TickAPI.Admins.Services;
using TickAPI.Common.Auth.Abstractions;
using TickAPI.Common.Auth.Enums;
using TickAPI.Common.Auth.Services;
using TickAPI.Common.Pagination.Abstractions;
using TickAPI.Common.Pagination.Services;
using TickAPI.Common.TickApiDbContext;
using TickAPI.Common.Time.Abstractions;
using TickAPI.Common.Time.Services;
using TickAPI.Customers.Abstractions;
using TickAPI.Customers.Repositories;
using TickAPI.Customers.Services;
using TickAPI.Events.Abstractions;
using TickAPI.Events.Repositories;
using TickAPI.Events.Services;
using TickAPI.Organizers.Abstractions;
using TickAPI.Organizers.Repositories;
using TickAPI.Organizers.Services;
using TickAPI.Tickets.Abstractions;
using TickAPI.Tickets.Repositories;
using TickAPI.Tickets.Services;

// Builder constants
const string allowClientPolicyName = "AllowClient";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthorization();

// Add controllers to the container.
builder.Services.AddControllers();

// Add authentication.
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Authentication:Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Authentication:Jwt:SecurityKey"]))
        };
    });

// Add authorization.
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthPolicies.AdminPolicy.ToString(), policy => policy.RequireRole(UserRole.Admin.ToString()));
    options.AddPolicy(AuthPolicies.OrganizerPolicy.ToString(), policy => policy.RequireRole(UserRole.Organizer.ToString()));
    options.AddPolicy(AuthPolicies.CustomerPolicy.ToString(), policy => policy.RequireRole(UserRole.Customer.ToString()));
    
    options.AddPolicy(AuthPolicies.NewOrganizerPolicy.ToString(), policy => policy.RequireRole(UserRole.NewOrganizer.ToString()));
    options.AddPolicy(AuthPolicies.NewCustomerPolicy.ToString(), policy => policy.RequireRole(UserRole.NewCustomer.ToString()));
});

// Add admin services.
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();

// Add customer services.
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

// Add event services.
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IEventRepository, EventRepository>();

// Add organizer services.
builder.Services.AddScoped<IOrganizerService, OrganizerService>();
builder.Services.AddScoped<IOrganizerRepository, OrganizerRepository>();

// Add ticket services.
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();

// Add common services.
builder.Services.AddScoped<IAuthService, GoogleAuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();
builder.Services.AddScoped<IPaginationService, PaginationService>();
builder.Services.AddScoped<IDateTimeService, DateTimeService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddDbContext<TickApiDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("ResellioLocalDB"));
});

// Create CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy(allowClientPolicyName,
        policy =>
        {
            var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
            policy.WithOrigins(allowedOrigins!)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
});

// TODO: when we start using redis we should probably also check here if we can connect to it
// Setup healtcheck
builder.Services.AddHealthChecks().AddSqlServer(connectionString: builder.Configuration.GetConnectionString("ResellioDatabase") ?? "");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors(allowClientPolicyName);

app.MapHealthChecks("/health");

app.MapControllers();

app.Run();