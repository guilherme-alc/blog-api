using Blog.Data;
using Blog.Repositories;
using Blog.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IO.Compression;
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
            app.UseHttpsRedirection();
            app.UseResponseCompression();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.Run();
        }

        static void LoadConfiguration(WebApplication app)
        {
            Configuration.JwtKey = app.Configuration.GetValue<string>("JwtKey");
            //Configuration.ApiKeyName = app.Configuration.GetValue<string>("ApiKeyName");
            //Configuration.ApiKey = app.Configuration.GetValue<string>("ApiKey");
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
            builder.Services.AddMemoryCache();
            builder.Services.AddResponseCompression(options =>
            {
                //options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
            });
            builder.Services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });

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

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "BlogApi",
                    Version = "v1",
                    Contact = new OpenApiContact
                    {
                        Name = "guilherme-alc",
                        Email = "guilhermelac01@gmail.com",
                        Url = new Uri("https://www.linkedin.com/in/guilherme-alc/")
                    }
                });

                var xmlFile = "Blog.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                c.IncludeXmlComments(xmlPath);

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Insira o token JWT no campo abaixo. Exemplo: Bearer {seu token}"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme()
                        {
                            Reference = new OpenApiReference()
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new List<string> ()
                    }
                });
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
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddScoped<PostRepository>();
            builder.Services.AddScoped<UserRepository>();
            builder.Services.AddScoped<CategoryService>();
            builder.Services.AddScoped<TagService>();
            builder.Services.AddScoped<RoleService>();
            builder.Services.AddScoped<PostService>();
            builder.Services.AddScoped<UserService>();
            builder.Services.AddTransient<TokenService>();
        }
    }
}
