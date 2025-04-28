using System.Text;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
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
using TickAPI.Addresses.Abstractions;
using TickAPI.Addresses.Repositories;
using TickAPI.Addresses.Services;
using TickAPI.Categories.Abstractions;
using TickAPI.Categories.Respositories;
using TickAPI.Categories.Services;
using TickAPI.Common.Claims.Abstractions;
using TickAPI.Common.Claims.Services;
using TickAPI.Common.Redis.Abstractions;
using TickAPI.Common.Redis.Services;
using TickAPI.Common.Mail.Abstractions;
using TickAPI.Common.Mail.Services;

// Builder constants
const string allowClientPolicyName = "AllowClient";

var builder = WebApplication.CreateBuilder(args);

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
    options.AddPolicy(AuthPolicies.VerifiedOrganizerPolicy.ToString(), policy => policy.RequireRole(UserRole.Organizer.ToString()));
    options.AddPolicy(AuthPolicies.CustomerPolicy.ToString(), policy => policy.RequireRole(UserRole.Customer.ToString()));
    
    options.AddPolicy(AuthPolicies.NewOrganizerPolicy.ToString(), policy => policy.RequireRole(UserRole.NewOrganizer.ToString()));
    options.AddPolicy(AuthPolicies.CreatedOrganizerPolicy.ToString(), policy => policy.RequireRole(UserRole.UnverifiedOrganizer.ToString(), UserRole.Organizer.ToString()));
    options.AddPolicy(AuthPolicies.VerifiedUserPolicy.ToString(), policy => policy.RequireRole(UserRole.Admin.ToString(), UserRole.Organizer.ToString(), UserRole.Customer.ToString()));
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

// Add address services.
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();

// Add organizer services.
builder.Services.AddScoped<IOrganizerService, OrganizerService>();
builder.Services.AddScoped<IOrganizerRepository, OrganizerRepository>();

// Add ticket services.
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();

// Add category services.
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

// Add common services.
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IGoogleDataFetcher, GoogleDataFetcher>();
builder.Services.AddScoped<IPaginationService, PaginationService>();
builder.Services.AddScoped<IDateTimeService, DateTimeService>();
builder.Services.AddScoped<IClaimsService, ClaimsService>();
builder.Services.AddScoped<IRedisService, RedisService>();
builder.Services.AddScoped<IMailService, MailService>();

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
    options.UseSqlServer(builder.Configuration.GetConnectionString("ResellioDatabase"),
        sqlOptions =>
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null));
});

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("ResellioRedisCache"))
);

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

// Add http client
builder.Services.AddHttpClient();

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
