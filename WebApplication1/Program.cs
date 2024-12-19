using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Логирование текущей среды
Console.WriteLine($"Current Environment: {builder.Environment.EnvironmentName}");

// Добавляем DbContext с PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Добавляем контроллеры
builder.Services.AddControllers();

// Настраиваем CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Добавляем Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Чтение и проверка JWT ключа
var keyString = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(keyString))
{
    throw new ArgumentNullException("Jwt:Key", "JWT Key is not configured in appsettings.json.");
}

var key = Encoding.ASCII.GetBytes(keyString);
if (key.Length < 32) // 32 байта * 8 = 256 бит
{
    throw new ArgumentOutOfRangeException("Jwt:Key", "JWT Key must be at least 256 bits (32 bytes) long.");
}

// Для отладки: вывод длины ключа
Console.WriteLine($"JWT Key Length: {key.Length * 8} bits");

// Настраиваем аутентификацию JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Для разработки, установить true в продакшене
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true, // Валидация ключа подписи
        IssuerSigningKey = new SymmetricSecurityKey(key), // Ключ подписи
        ValidateIssuer = true, // Валидация издателя
        ValidIssuer = builder.Configuration["Jwt:Issuer"], // Издатель
        ValidateAudience = true, // Валидация аудитории
        ValidAudience = builder.Configuration["Jwt:Audience"], // Аудитория
        ValidateLifetime = true // Валидация срока действия
    };
});

// Добавляем авторизацию
builder.Services.AddAuthorization();

var app = builder.Build();

// Настраиваем middleware

// Используем CORS
app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Используем Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Используем аутентификацию и авторизацию
app.UseAuthentication(); // Важно: перед UseAuthorization
app.UseAuthorization();

// Маршруты контроллеров
app.MapControllers();

app.Run();


