using Blogs.App.Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// --- 1. Veritabaný (DbContext) - KRÝTÝK DÜZELTME BURADA ---
// <DbContext, BlogsDb> diyerek hem base sýnýfý hem de kendi sýnýfýmýzý tanýtýyoruz.
// Böylece Handler'lar "DbContext" istediðinde hata vermeyecek.
builder.Services.AddDbContext<DbContext, BlogsDb>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("BlogsDb")));

// --- 2. MediatR (Handler'lar) ---
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(BlogsDb).Assembly));

// --- 3. Authentication (JWT Ayarlarý) ---
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(config =>
    {
        // Güvenlik anahtarýný al, boþsa hata verme
        var securityKey = builder.Configuration["SecurityKey"] ?? string.Empty;

        config.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey)),

            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Issuer"],

            ValidateAudience = true,
            ValidAudience = builder.Configuration["Audience"],

            ValidateLifetime = true
        };
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// --- 4. Swagger (Kilit Butonu) ---
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Blogs API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
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
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// --- 5. Middleware Sýralamasý ---
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();