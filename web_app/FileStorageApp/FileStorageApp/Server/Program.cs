using FileStorageApp.Server.Database;
using FileStorageApp.Server.Repositories;
using FileStorageApp.Server.SecurityFolder;
using FileStorageApp.Server.SeederClass;
using FileStorageApp.Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

//http client
builder.Services.AddHttpClient();

//CORS
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      builder =>
                      {
                          builder.AllowAnyOrigin()
                                 .AllowAnyHeader()
                                 .AllowAnyMethod();
                      });
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

//Database Service
builder.Services.AddDbContext<DataContext>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//Repositories
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<RoleRepository>();
builder.Services.AddScoped<FileRepository>();
builder.Services.AddScoped<RespRepository>();
builder.Services.AddScoped<UserFileRepo>();
builder.Services.AddScoped<FileTransferRepo>();

//Services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<SecurityManager>();
builder.Services.AddScoped<FileService>();
//builder.Services.AddScoped<IConfiguration>();
builder.Services.AddScoped<AzureBlobService>();

//Seeder
builder.Services.AddScoped<DataSeeder>();

//swagg ---------------------------------------------------------------
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter the JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
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
            new string[] { }
        }
    });
});
//swagg ---------------------------------------------------------------

//Jwt
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidAudience = "fileStorage.com",
                        ValidIssuer = "fileStorage.com",
                        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Secret"]))
                    };
                }
                );
/*builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("admin"));
    options.AddPolicy("RequireClientRole", policy => policy.RequireRole("client"));
});*/

//Configure Maximum File Size
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 1000 * 1024 * 1024; // 1000 MB
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 1000 * 1024 * 1024; // 1000 MB
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//CORS
app.UseCors(MyAllowSpecificOrigins);

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();


app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

//Jwt
app.UseAuthentication();
app.UseAuthorization();

//swagg ---------------------------------------------------------------
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
});
//swagg ---------------------------------------------------------------

//Seeder
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var seeder = serviceProvider.GetRequiredService<DataSeeder>();

    // Ensure that SeedData is asynchronous and returns Task
    await seeder.SeedData();
}

app.Run();
