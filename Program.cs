using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using WebApplication.Data;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;

var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

var jwtSecret = builder.Configuration["JwtSettings:Secret"]
    ?? throw new InvalidOperationException("JwtSettings:Secret missing");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecret)
            ),
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Students API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header
    });

    c.AddSecurityRequirement(doc =>
    {
        var req = new OpenApiSecurityRequirement();
        req.Add(new OpenApiSecuritySchemeReference("Bearer", doc), new List<string>());
        return req;
    });
});

builder.Services.AddDbContext<SchoolContext>(options =>
    options.UseSqlite("Data Source=school.db"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WebApplication.Data.SchoolContext>();
    db.Database.EnsureCreated();

    var admin = db.Users.SingleOrDefault(u => u.Email == "admin@test.com");

    if (admin == null)
    {
        db.Users.Add(new WebApplication.Models.User
        {
            Email = "admin@test.com",
            PasswordHash = "1234",
            Role = "Admin"
        });
        db.SaveChanges();
    }
    else
    {
        admin.PasswordHash = "1234";
        admin.Role = "Admin";
        db.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
