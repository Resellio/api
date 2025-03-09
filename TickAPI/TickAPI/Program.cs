using TickAPI;
using TickAPI.Admin.Abstractions;
using TickAPI.Admin.Repositories;
using TickAPI.Admin.Services;
using TickAPI.Common.Auth.Abstractions;
using TickAPI.Common.Auth.Services;
using TickAPI.Common.Pagination.Abstractions;
using TickAPI.Common.Pagination.Services;
using TickAPI.Customer.Abstractions;
using TickAPI.Customer.Repositories;
using TickAPI.Customer.Services;
using TickAPI.Event.Abstractions;
using TickAPI.Event.Repositories;
using TickAPI.Event.Services;
using TickAPI.Organizer.Abstractions;
using TickAPI.Organizer.Repositories;
using TickAPI.Organizer.Services;
using TickAPI.Ticket.Abstractions;
using TickAPI.Ticket.Repositories;
using TickAPI.Ticket.Services;

// Builder constants
const string allowClientPolicyName = "AllowClient";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthorization();

// Add controllers to the container.
builder.Services.AddControllers();

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
builder.Services.AddScoped<IPaginationService, PaginationService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.Run();