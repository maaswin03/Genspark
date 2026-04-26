using backend.Data;
using backend.Models.Enums;
using Microsoft.EntityFrameworkCore;
using backend.Services;
using backend.Helpers;
using backend.Jobs;
using backend.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

// Allow Npgsql to map 'timestamp without time zone' <-> DateTime(Kind=Unspecified)
// The trips table stores departure_time as plain local timestamps (no timezone).
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions =>
        {
            npgsqlOptions.MapEnum<ApprovalStatus>("approval_status_enum");
            npgsqlOptions.MapEnum<BookingStatus>("booking_status_enum");
            npgsqlOptions.MapEnum<PaymentStatus>("payment_status_enum");
            npgsqlOptions.MapEnum<SeatStatus>("seat_status_enum");
            npgsqlOptions.MapEnum<BusType>("bus_type_enum");
            npgsqlOptions.MapEnum<DeckType>("deck_type_enum");
            npgsqlOptions.MapEnum<SeatType>("seat_type_enum");
            npgsqlOptions.MapEnum<Gender>("gender_enum");
            npgsqlOptions.MapEnum<TripStatus>("trip_status_enum");
            npgsqlOptions.MapEnum<BookingSeatStatus>("booking_seat_status_enum");
            npgsqlOptions.MapEnum<DocumentType>("document_type_enum");
            npgsqlOptions.MapEnum<FeeType>("fee_type_enum");
            npgsqlOptions.MapEnum<ChangeType>("change_type_enum");
            npgsqlOptions.MapEnum<NotificationChannel>("notification_channel_enum");
            npgsqlOptions.MapEnum<EntityType>("entity_type_enum");
            npgsqlOptions.MapEnum<backend.Models.Enums.ReferenceType>("reference_type_enum");
        }));

builder.Services.AddControllers();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IRouteService, RouteService>();
builder.Services.AddScoped<IOperatorService, OperatorService>();
builder.Services.AddScoped<IBusService, BusService>();
builder.Services.AddScoped<ITripService, TripService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IRevenueService, RevenueService>();
builder.Services.AddScoped<JwtHelper>();
builder.Services.AddSingleton<SeatLockExpiryJob>();
builder.Services.AddSingleton<TripGeneratorJob>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendCors", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:4000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<LoggingMiddleware>();

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("FrontendCors");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
// Start background jobs
var seatLockJob = app.Services.GetRequiredService<SeatLockExpiryJob>();
var tripGeneratorJob = app.Services.GetRequiredService<TripGeneratorJob>();

seatLockJob.Start();
tripGeneratorJob.Start();

// Graceful shutdown
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(async () =>
{
    await seatLockJob.StopAsync();
    await tripGeneratorJob.StopAsync();
});


app.Run();