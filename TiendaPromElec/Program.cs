using Microsoft.EntityFrameworkCore;
using ProductApi.Models;
using TiendaPromElec.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using TiendaPromElec.Helpers;

var builder = WebApplication.CreateBuilder(args);

// DB SETUP
// if docker or not
var useInMemory = Environment.GetEnvironmentVariable("USE_IN_MEMORY_DB") == "true";

if (useInMemory)
{
    // Docker
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("PromElecDb"));
}
else
{
    // LocalDB
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));
}
// -----------------------------------------

// Repositorio
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// JWT Config
var jwtKey = JwtHelper.GetJwtKey(builder.Configuration);
// Log key source
if (jwtKey == "EstaEsUnaClaveSuperSecretaParaElRetoDelTec2026!")
{
    Console.WriteLine("--> USING FALLBACK JWT KEY (Configuration 'Jwt:Key' was null or empty)");
}
else
{
    Console.WriteLine("--> USING CONFIGURED JWT KEY");
}

var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// Configuracion CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader());
});

builder.Services.AddControllers();
builder.Services.AddRazorPages();

// Swagger con  JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TiendaPromElec API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
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
            new List<string>()
        }
    });
});

var app = builder.Build();

// Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Auth 
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();



using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();

    context.Database.EnsureCreated();
}

app.Run();

public partial class Program { }