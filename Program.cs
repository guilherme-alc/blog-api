using Blog.Data;
using Blog.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Blog
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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

            builder.Configuration.AddUserSecrets<Program>();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddDbContext<BlogDataContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            builder.Services
                .AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    // Desativa a validação automática do ModelState para utilizar o ResultViewModel para padronização dos retornos
                    options.SuppressModelStateInvalidFilter = true;
                });

            builder.Services.AddTransient<TokenService>();

            var app = builder.Build();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
