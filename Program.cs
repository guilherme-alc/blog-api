using Blog.Data;
using Blog.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

namespace Blog
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigureAuthentication(builder);
            ConfigureMvc(builder);
            ConfigureServices(builder);

            var app = builder.Build();

            LoadConfiguration(app);

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        static void LoadConfiguration(WebApplication app)
        {
            Configuration.JwtKey = app.Configuration.GetValue<string>("JwtKey");
            Configuration.ApiKeyName = app.Configuration.GetValue<string>("ApiKeyName");
            Configuration.ApiKey = app.Configuration.GetValue<string>("ApiKey");
        }

        static void ConfigureAuthentication (WebApplicationBuilder builder)
        {
            var key = Encoding.ASCII.GetBytes(Configuration.JwtKey);

            builder.Services.AddAuthentication(authOptions =>
            {
                authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(jwtOptions =>
            {
                jwtOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
        }

        static void ConfigureMvc(WebApplicationBuilder builder)
        {
            builder.Services
                .AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    // Desativa a validação automática do ModelState para utilizar o ResultViewModel para padronização dos retornos
                    options.SuppressModelStateInvalidFilter = true;
                })
                .AddJsonOptions(opts =>
                {
                    opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
                });
        }

        static void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.Configuration.AddUserSecrets<Program>();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddDbContext<BlogDataContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            //Lifetime
            builder.Services.AddTransient<TokenService>();
        }
    }
}
