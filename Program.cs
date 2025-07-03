
using GradProject.Data;
using GradProject.Models.Entities;
using GradProject.Repositories; // Added this using statement
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models; // Added for Swagger security configuration


namespace GradProject
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "GradProject API", Version = "v1" });

                // Configure Swagger to use JWT Bearer authentication
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}",
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
                        new string[] {}
                    }
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
            //Register the LabManager in Dependency Injection
            builder.Services.AddSingleton<LabManager>();


            // Add DbContext
            builder.Services.AddDbContext<AppDbContext>(options =>
options.UseMySql(
  builder.Configuration.GetConnectionString("MariaDbConnection"),
  new MySqlServerVersion(new Version(10, 11, 8))  // تأكد من رقم إصدار MariaDB لديك
));

            // Register Repositories and Unit of Work
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ILabRepository, LabRepository>();
            builder.Services.AddScoped<IUserLabRepository, UserLabRepository>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register AuthService
            builder.Services.AddScoped<GradProject.Services.IAuthService, GradProject.Services.AuthService>();

            // Add Identity with int keys
            builder.Services.AddIdentity<User, IdentityRole<int>>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            // Configure JWT Authentication
            var jwtSettings = builder.Configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);


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
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });
            //  Add CORS policy
            builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSwaggerUI", policy =>
    {
        policy.WithOrigins("http://localhost:5147", "http://127.0.0.1:5147") // Match the server origin
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });


});



            var app = builder.Build();
            //app.UseCors("AllowSwaggerUI");

            //// Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseHttpsRedirection();


            // Enable static file serving (e.g., HTML, CSS, JS)
            app.UseDefaultFiles();   // 🔹 لتقديم ملفات مثل index.html تلقائيًا
            app.UseStaticFiles();    // 🔹 للسماح بتحميل ملفات CSS, JS, Images, وغيرها

            app.UseRouting();        // 🔹 يجب أن يكون قبل المصادقة والتفويض
            app.UseCors("AllowSwaggerUI");
            //app.UseCors(builder => builder.AllowAnyOrigin());  // 🔹 يجب أن يكون قبل UseAuthentication

            app.UseAuthentication();  // 🔹 يجب أن يكون قبل UseAuthorization
            app.UseAuthorization();   // 🔹 يجب أن يكون بعد UseAuthentication

            app.MapControllers();     // 🔹 لتحديد مسارات API

            app.MapFallbackToFile("main.html"); // 🔹 تحويل الطلبات غير المعروفة إلى main.html
                                                //app.Urls.Add("http://0.0.0.0:5000");

            app.Run();

        }
    }
}
