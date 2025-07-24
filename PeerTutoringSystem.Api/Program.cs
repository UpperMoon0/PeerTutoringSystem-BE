using System.Text;
using PeerTutoringSystem.Application.Services;
using Firebase.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PeerTutoringSystem.Application.DTOs.Authentication;
using PeerTutoringSystem.Application.Interfaces.Authentication;
using PeerTutoringSystem.Application.Interfaces.Booking;
using PeerTutoringSystem.Application.Interfaces.Profile_Bio;
using PeerTutoringSystem.Application.Interfaces.Reviews;
using PeerTutoringSystem.Application.Interfaces.Skills;
using PeerTutoringSystem.Application.Interfaces.Tutor;
using PeerTutoringSystem.Application.Services.Authentication;
using PeerTutoringSystem.Application.Services.Booking;
using PeerTutoringSystem.Application.Services.Profile_Bio;
using PeerTutoringSystem.Application.Services.Reviews;
using PeerTutoringSystem.Application.Services.Tutor;
using PeerTutoringSystem.Application.Interfaces.Chat;
using PeerTutoringSystem.Application.Services.Chat;
using PeerTutoringSystem.Domain.Interfaces.Chat;
using PeerTutoringSystem.Domain.Interfaces.Authentication;
using PeerTutoringSystem.Domain.Interfaces.Booking;
using PeerTutoringSystem.Domain.Interfaces.Profile_Bio;
using PeerTutoringSystem.Domain.Interfaces.Reviews;
using PeerTutoringSystem.Domain.Interfaces.Skills;
using PeerTutoringSystem.Infrastructure.Data;
using PeerTutoringSystem.Infrastructure.Repositories.Authentication;
using PeerTutoringSystem.Infrastructure.Repositories.Booking;
using PeerTutoringSystem.Infrastructure.Repositories.Profile_Bio;
using PeerTutoringSystem.Infrastructure.Repositories.Reviews;
using PeerTutoringSystem.Infrastructure.Repositories.Skills;
using PeerTutoringSystem.Infrastructure.Repositories.Chat;
using PeerTutoringSystem.Domain.Interfaces.Payment;
using PeerTutoringSystem.Infrastructure.Repositories.Payment;
using PeerTutoringSystem.Application.Services.Payment;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

var builder = WebApplication.CreateBuilder(args);

// Initialize Firebase Admin SDK
var firebaseConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "serviceAccountKey.json");
if (File.Exists(firebaseConfigPath))
{
    FirebaseApp.Create(new AppOptions()
    {
        Credential = GoogleCredential.FromFile(firebaseConfigPath),
        ProjectId = builder.Configuration["Firebase:ProjectId"]
    });
}
else
{
    Console.WriteLine($"Firebase service account key not found at: {firebaseConfigPath}");
}

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();

// Configure DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});
builder.Services.AddLogging();

// Register services and repositories
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<ITutorVerificationService, TutorVerificationService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserTokenRepository, UserTokenRepository>();
builder.Services.AddScoped<ITutorVerificationRepository, TutorVerificationRepository>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IUserBioService, UserBioService>();
builder.Services.AddScoped<IUserBioRepository, UserBioRepository>();
builder.Services.AddScoped<ISkillRepository, SkillRepository>();
builder.Services.AddScoped<IUserSkillRepository, UserSkillRepository>();
builder.Services.AddScoped<ISkillService, SkillService>();
builder.Services.AddScoped<IUserSkillService, UserSkillService>();
builder.Services.AddScoped<ITutorService, TutorService>();
builder.Services.AddScoped<ITutorAvailabilityRepository, TutorAvailabilityRepository>();
builder.Services.AddScoped<IBookingSessionRepository, BookingSessionRepository>();
builder.Services.AddScoped<ITutorAvailabilityService, TutorAvailabilityService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();

builder.Services.AddHttpClient();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<FirebaseStorageService>();
builder.Services.AddSingleton(provider =>
{
    var firebaseDatabaseUrl = builder.Configuration["Firebase:DatabaseUrl"];
    return new FirebaseClient(firebaseDatabaseUrl);
});
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IAiChatService, AiChatService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.WithOrigins("http://localhost:5173")
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials();
        });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PeerTutoringSystem API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter the JWT token in the format 'Bearer {token}'",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
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
        }
    });

    // Cấu hình Swagger để loại bỏ SkillID khỏi request body của POST /api/skills
    c.MapType<CreateSkillDto>(() => new OpenApiSchema
    {
        Type = "object",
        Properties = new Dictionary<string, OpenApiSchema>
        {
            ["skillName"] = new OpenApiSchema { Type = "string" },
            ["skillLevel"] = new OpenApiSchema { Type = "string" },
            ["description"] = new OpenApiSchema { Type = "string" }
        },
        Required = new HashSet<string> { "skillName" }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PeerTutoringSystem API V1");
        c.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<PeerTutoringSystem.Api.Hubs.ChatHub>("/chatHub");

app.Run();