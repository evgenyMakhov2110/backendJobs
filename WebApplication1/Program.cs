using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// ����������� ������� �����
Console.WriteLine($"Current Environment: {builder.Environment.EnvironmentName}");

// ��������� DbContext � PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ��������� �����������
builder.Services.AddControllers();

// ����������� CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ��������� Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ������ � �������� JWT �����
var keyString = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(keyString))
{
    throw new ArgumentNullException("Jwt:Key", "JWT Key is not configured in appsettings.json.");
}

var key = Encoding.ASCII.GetBytes(keyString);
if (key.Length < 32) // 32 ����� * 8 = 256 ���
{
    throw new ArgumentOutOfRangeException("Jwt:Key", "JWT Key must be at least 256 bits (32 bytes) long.");
}

// ��� �������: ����� ����� �����
Console.WriteLine($"JWT Key Length: {key.Length * 8} bits");

// ����������� �������������� JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // ��� ����������, ���������� true � ����������
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true, // ��������� ����� �������
        IssuerSigningKey = new SymmetricSecurityKey(key), // ���� �������
        ValidateIssuer = true, // ��������� ��������
        ValidIssuer = builder.Configuration["Jwt:Issuer"], // ��������
        ValidateAudience = true, // ��������� ���������
        ValidAudience = builder.Configuration["Jwt:Audience"], // ���������
        ValidateLifetime = true // ��������� ����� ��������
    };
});

// ��������� �����������
builder.Services.AddAuthorization();

var app = builder.Build();

// ����������� middleware

// ���������� CORS
app.UseHttpsRedirection();
app.UseCors("AllowAll");

// ���������� Swagger
app.UseSwagger();
app.UseSwaggerUI();

// ���������� �������������� � �����������
app.UseAuthentication(); // �����: ����� UseAuthorization
app.UseAuthorization();

// �������� ������������
app.MapControllers();

app.Run();


