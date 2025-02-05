using Blog.Data;
using Blog.Services;
using Microsoft.EntityFrameworkCore;

namespace Blog
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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
            app.MapControllers();

            app.Run();
        }
    }
}
