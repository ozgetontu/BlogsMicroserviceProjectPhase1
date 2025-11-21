using Blogs.App.Domain;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the IoC Container.

// 1. DbContext (Veritabaný Baðlantýsý):
// Hocanýn yapýsýndaki gibi DbContext base class'ý ile birlikte kaydediyoruz.
// <DbContext, BlogsDb> yazarak hem base sýnýfý hem de kendi sýnýfýmýzý tanýtýyoruz.
builder.Services.AddDbContext<DbContext, BlogsDb>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("BlogsDb")));

// 2. Mediator:
// Hocanýn döngüsel (loop) yapýsý. Tüm assembly'leri tarayýp MediatR servislerini bulur.
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(BlogsDb).Assembly));

// 3. Authentication (JWT):
// Hocanýn kodundaki SecurityKey, Issuer ve Audience ayarlarý.
builder.Configuration["SecurityKey"] = "blogs_microservices_security_key_2025="; // Senin için özelleþtirdim
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(config =>
    {
        config.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["SecurityKey"] ?? string.Empty)),
            ValidIssuer = builder.Configuration["Issuer"],
            ValidAudience = builder.Configuration["Audience"],
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true
        };
    });

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// 4. Swagger Konfigürasyonu (Kilit Simgesi Eklemek Ýçin):
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Blogs API",
        Version = "v1"
    });
    // JWT yetkilendirme butonu (Kilit simgesi) ekler
    c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = """
        JWT Authorization header using the Bearer scheme.
        Enter your JWT as: "Bearer jwt"
        Example: "Bearer a1b2c3"
        """
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            },
            Array.Empty<string>()
        }
    });
});

// 5. CORS Politikasý:
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder => builder
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 6. Middleware Sýralamasý (Çok Önemli):
app.UseAuthentication(); // Önce kimlik kontrolü
app.UseAuthorization();  // Sonra yetki kontrolü

app.MapControllers();

app.UseCors();

app.Run();