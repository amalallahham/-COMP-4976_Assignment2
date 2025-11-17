using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ObituaryApplication.Data;
using ObituaryApplication.Models;
using ObituaryApplication.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------
// Data + Identity
// -----------------------------
// var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
//     ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// builder.Services.AddDbContext<ApplicationDbContext>(options =>
//     options.UseSqlite(connectionString));

builder.AddSqlServerDbContext<ApplicationDbContext>("sqldata");

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// -----------------------------
// JWT + Token service
// -----------------------------
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddScoped<JwtTokenService>();

builder.Services
    .AddAuthentication()
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        var jwt = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
                  ?? throw new InvalidOperationException("JWT settings not found.");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey))
        };
    });

// -----------------------------
// HttpClient for external API calls
// -----------------------------
builder.Services.AddHttpClient();

// -----------------------------
// Controllers + Swagger
// -----------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Obituary Application API",
        Version = "v1",
        Description = "API for managing obituaries with JWT authentication"
    });

    // JWT Support
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter 'Bearer' followed by your JWT token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// -----------------------------
// Razor Pages
// -----------------------------
builder.Services.AddRazorPages();


// -----------------------------
// CORS
// -----------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy
            .AllowAnyOrigin()   
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


var app = builder.Build();

// -----------------------------
// Seed roles/users
// -----------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    await DbInitializer.Seed(userManager, roleManager, context);
}

// -----------------------------
// Middleware Pipeline
// -----------------------------
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Obituary Application API v1");
    // optional: shows swagger at the root URL
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


app.UseCors("AllowBlazorClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();  // API endpoints
app.MapRazorPages();   // UI pages

app.Run();
