
namespace shop.commerce.api.presentation
{
    using shop.commerce.api.Application.Configuration;
    using shop.commerce.api.Application.Models;
    using shop.commerce.api.infrastructure.Repositories;
    using shop.commerce.api.presentation.Configuration.Accessors;
    using shop.commerce.api.services.Helpers;
    using shop.commerce.api.services.Testing;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.OpenApi.Models;
    using System.Text;
    using shop.commerce.api.infrastructure.Repositories.EntityFramework;
    using Hangfire;
    using Hangfire.SqlServer;
    using System;
    using Microsoft.AspNetCore.HttpOverrides;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IConfiguration _configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();

            var (secrets, settings) = GetGlobalConfiguration(services);

            // Configure JWT authentication.
            //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //  .AddJwtBearerConfiguration(
            //    secrets.Authentication.Issuer,
            //    secrets.Authentication.Audience
            //  );
            
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(jwt =>
            {
                var key = Encoding.ASCII.GetBytes(secrets.Authentication.Secret);

                jwt.SaveToken = true;
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true, // this will validate the 3rd part of the jwt token using the secret that we added in the appsettings and verify we have generated the jwt token
                    IssuerSigningKey = new SymmetricSecurityKey(key), // Add the secret key to our Jwt encryption
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    RequireExpirationTime = false,
                    ValidateLifetime = true,
                };
            });

            ConfigureSwagger(services);
            ConfigureLogger();


            //services.Configure<ForwardedHeadersOptions>(options =>
            //{
            //    options.KnownProxies.Add(IPAddress.Parse("192.248.183.59"));
            //});

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });

            //services.AddHealthChecks()
            //    .AddSqlServer(secrets.ConnectionStrings.MainDatabase)
            //    .AddHangfire(setup =>
            //    {
            //        setup.MaximumJobsFailed = 10;
            //        setup.MinimumAvailableServers = 1;
            //    });

            ConfigureApplicationsServices(services, secrets, settings);

            services.AddScoped<InitApp>();
            services.AddScoped<TestData>();
            services.AddScoped<MessagesHelper>();

            services.AddScoped<IApplicationSettingsAccessor, ApplicationSettingsAccessor>();
            services.AddScoped<IApplicationSecretsAccessor, ApplicationSecretsAccessor>();

            // services
            //services.AddScoped<IAdminService, AdminService>();
            //services.AddScoped<IUserService, UserService>();
            //services.AddScoped<ICategoryService, CategoryService>();
            //services.AddScoped<IAccountService, AccountService>();
            //services.AddScoped<IHomeService, HomeService>();

            // repositories
            //services.AddScoped<IProductRepository, ProductRepositoryEF>();
            //services.AddScoped<ICategoryRepository, CategoryRepository>();
            //services.AddScoped<IUserRepository, UserRepository>();
            //services.AddScoped<IOrderRepository, OrderRepositoryEF>();
            //services.AddScoped<ISlideRepository, SlideRepository>();
            
            //services.AddTransient<IConnectionFactory, ConnectionFactory>();
            
            //services.AddScoped<ShopContextFactory>();

            ServiceProvider buildServiceProvider = services.BuildServiceProvider();
            
            //var applicationSecretsAccessor = buildServiceProvider.GetRequiredService<IApplicationSecretsAccessor>();
            //services.AddDbContext<ShopContext>(o => o.UseNpgsql(applicationSecretsAccessor.GetMainDatabase(), opt => opt.SetPostgresVersion(new Version(9, 6))));

            var initapp = buildServiceProvider.GetRequiredService<InitApp>();
            initapp.InitAsync().ConfigureAwait(false);

            services.AddScoped<MessagesHelper>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApi v1"));
            // app.UseHttpsRedirection();
            app.UseRouting();


            app.UseAuthentication();
            app.UseAuthorization();

            //app.UseCors("CorsPolicy");
            app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // allow any origin
                .AllowCredentials()); // allow credentials

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void ConfigureSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApi", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });
            });
        }

        private (ApplicationSecrets secrets, ApplicationSettings settings) GetGlobalConfiguration(IServiceCollection services)
        {
            // set the globalConfig
            services.Configure<ApplicationSecrets>(_configuration.GetSection("ApplicationSecrets"));
            services.Configure<ApplicationSettings>(_configuration.GetSection("ApplicationSettings"));

            var secrets = new ApplicationSecrets();
            var globalConfig = new ApplicationSettings();

            _configuration.Bind("ApplicationSettings", globalConfig);
            _configuration.Bind("ApplicationSecrets", secrets);

            return (secrets, globalConfig);
        }

        private void ConfigureLogger()
        {
            //Log.Logger = new LoggerConfiguration()
            //    .ReadFrom.Configuration(Configuration)
            //    .CreateLogger();
        }
        /// <summary>
        /// this method is used to configure hangfire integration
        /// </summary>
        private void ConfigureHangfire(
            IServiceCollection services,
            ApplicationSecrets secrets,
            ApplicationSettings settings)
        {
            // Add Hangfire services.
            services.AddHangfire((provider, configuration) =>
                configuration
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UseSqlServerStorage(secrets.ConnectionStrings.HangfireDatabase, new SqlServerStorageOptions
                    {
                        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                        QueuePollInterval = TimeSpan.Zero,
                        UseRecommendedIsolationLevel = true,
                        DisableGlobalLocks = true
                    }));

            // Add the processing server as IHostedService
            services.AddHangfireServer();

            // add global hangfire filters
            //GlobalJobFilters.Filters
            //    .Add(new ProlongExpirationTimeAttribute(settings.Queue.ProlongExpirationTime));
        }

        private void ConfigureApplicationsServices(IServiceCollection services, ApplicationSecrets secrets, ApplicationSettings settings)
        {
            // add application configuration 
            services.AddApplicationLayer(secrets.ClientId)
                .AddPresentationLayer()
                .AddInfrastructureLayer(options =>
                {
                    options.DatabaseConnectionString = secrets.ConnectionStrings.MainDatabase;
                    options.MemoryDatabase = secrets.MemoryDatabase;
                    options.IsMemoryDatabase = secrets.IsMemoryDatabase;
                    options.IsPGSQL = secrets.IsPGSQL;
                    options.IsSqlServer = secrets.IsSqlServer;
                });
                //.AddAuthentication(options =>
                //{
                //    options.Issuer = secrets.Authentication.Issuer;
                //    options.Secret = secrets.Authentication.Secret;
                //    options.Audience = secrets.Authentication.Audience;
                //    options.AccessExpiration = secrets.Authentication.AccessExpiration;
                //    options.RefreshExpiration = secrets.Authentication.RefreshExpiration;
                //});

        }
    }
}
