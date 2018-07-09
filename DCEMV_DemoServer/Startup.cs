/*
*************************************************************************
DC EMV
Open Source EMV
Copyright (C) 2018  Vicente Da Silva

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see http://www.gnu.org/licenses/
*************************************************************************
*/
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.Swagger;
using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using DCEMV.DemoServer.Persistence.Credentials;
using DCEMV.DemoServer.Persistence.Api;

using DCEMV.DemoServer.Persistence.Api.Repository;
using DCEMV.DemoServer.Components;

namespace DCEMV.DemoServer
{
    //public class ResourceOwnerPasswordValidator<TUser> : IResourceOwnerPasswordValidator
    //    where TUser : class
    //{
    //    private readonly SignInManager<TUser> _signInManager;
    //    private readonly UserManager<TUser> _userManager;
    //    private readonly ILogger<ResourceOwnerPasswordValidator<TUser>> _logger;

    //    public ResourceOwnerPasswordValidator(
    //        UserManager<TUser> userManager,
    //        SignInManager<TUser> signInManager,
    //        ILogger<ResourceOwnerPasswordValidator<TUser>> logger)
    //    {
    //        _userManager = userManager;
    //        _signInManager = signInManager;
    //        _logger = logger;
    //    }

    //    public virtual async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
    //    {
    //        var user = await _userManager.FindByNameAsync(context.UserName);
    //        if (user != null)
    //        {
    //            Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(user, context.Password, false, true);
    //            if (result.Succeeded)
    //            {
    //                _logger.LogInformation("Credentials validated for username: {username}", context.UserName);

    //                string sub = await _userManager.GetUserIdAsync(user);
    //                context.Result = new GrantValidationResult(sub, AuthenticationMethods.Password);
    //                return;
    //            }
    //            else if (result.IsLockedOut)
    //            {
    //                _logger.LogInformation("Authentication failed for username: {username}, reason: locked out", context.UserName);
    //            }
    //            else if (result.IsNotAllowed)
    //            {
    //                _logger.LogInformation("Authentication failed for username: {username}, reason: not allowed", context.UserName);
    //            }
    //            else if (result.RequiresTwoFactor)
    //            {
    //                _logger.LogInformation("Authentication failed for username: {username}, reason: two factor required", context.UserName);
    //            }
    //            else
    //            {
    //                _logger.LogInformation("Authentication failed for username: {username}, reason: invalid credentials", context.UserName);
    //            }
    //        }
    //        else
    //        {
    //            _logger.LogInformation("No user found matching username: {username}", context.UserName);
    //        }

    //        context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant);
    //    }
    //}

    public class SecurityRequirementsOperationFilter : IOperationFilter
    {
        public void Apply(Swashbuckle.AspNetCore.Swagger.Operation operation, OperationFilterContext context)
        {
            // Policy names map to scopes
            var controllerScopes = context.ApiDescription.ControllerAttributes()
                .OfType<AuthorizeAttribute>()
                .Select(attr => attr.Policy);

            var actionScopes = context.ApiDescription.ActionAttributes()
                .OfType<AuthorizeAttribute>()
                .Select(attr => attr.Policy);

            var requiredScopes = controllerScopes.Union(actionScopes).Distinct();

            if (requiredScopes.Any())
            {
                operation.Responses.Add("401", new Swashbuckle.AspNetCore.Swagger.Response { Description = "Unauthorized" });
                operation.Responses.Add("403", new Swashbuckle.AspNetCore.Swagger.Response { Description = "Forbidden" });
                operation.Responses.Add("500", new Response { Description = "Server Error" });

                operation.Security = new List<IDictionary<string, IEnumerable<string>>>();
                operation.Security.Add(new Dictionary<string, IEnumerable<string>>
                {
                    { "oauth2", requiredScopes }
                });
            }
        }
    }

    /*
     * ASP.NET Core dependency injection provides application services during an application's startup. You can request these services by including the 
     * appropriate interface as a parameter on your Startup class's constructor or one of its Configure or ConfigureServices methods. 
     * 
     * Looking at each method in the Startup class in the order in which they are called, the following services may be requested as parameters:
     * In the constructor: IHostingEnvironment, ILoggerFactory
     * In the ConfigureServices method: IServiceCollection
     * In the Configure method: IApplicationBuilder, IHostingEnvironment, ILoggerFactory, IApplicationLifetime
     */
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        //The ConfigureServices method is optional; but if used, it's called before the Configure method by the runtime 
        //(some features are added before they're wired up to the request pipeline). Configuration options are set in this method.
        public void ConfigureServices(IServiceCollection services)
        {
            //string tokenConnectionString = Configuration["ConnectionStrings:TokenServerConnection"];
            string userConnectionString = Configuration["ConnectionStrings:UserServerConnection"];
            //string apiConnectionString = Configuration["ConnectionStrings:ApiServerConnection"];

            string env = Environment.GetEnvironmentVariable("DB_SERVER_NAME");
            if (String.IsNullOrEmpty(env))
                throw new Exception("DB_SERVER_NAME env variable not found");

            //apiConnectionString = apiConnectionString.Replace("{DB_SERVER_NAME}", env);
            userConnectionString = userConnectionString.Replace("{DB_SERVER_NAME}", env);
            //tokenConnectionString = tokenConnectionString.Replace("{DB_SERVER_NAME}", env);

            string migrationAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services.AddDbContext<IdentityUserDbContext>(builder => builder.UseSqlServer(userConnectionString, options => options.MigrationsAssembly(migrationAssembly)));
            services.AddDbContext<ApiDbContext>(builder => builder.UseSqlServer(userConnectionString, options => options.MigrationsAssembly(migrationAssembly)));

            services.AddIdentity<ApplicationUser, IdentityRole>(config =>
            {
                config.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                config.Lockout.MaxFailedAccessAttempts = 10;
                config.SignIn.RequireConfirmedEmail = true;
                //config.SignIn.RequireConfirmedPhoneNumber = true;
                config.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<IdentityUserDbContext>()
            .AddDefaultTokenProviders();

            services.AddMvc(options =>
            {
            //    options.SslPort = 44359;
            //    options.Filters.Add(new RequireHttpsAttribute());
            })
            .AddJsonOptions(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);//ignore circular references in entities

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddTransient<ITransactionsRepository, TransactionsRepository>();
            services.AddTransient<ICardsRepository, CardsRepository>();
            services.AddTransient<IStoreRepository, StoreRepository>();
            services.AddTransient<IAccountsRepository, AccountsRepository>();


            services.AddIdentityServer(/*x => { x.IssuerUri = ConfigSingleton.IdentityServerUrl;}*/)
                .AddTemporarySigningCredential()
               //.AddInMemoryIdentityResources(Config.GetIdentityResources())
               //.AddInMemoryApiResources(Config.GetApiResources())
               //.AddInMemoryClients(Config.GetClients())
               //.AddTestUsers(Config.GetUsers())
               .AddConfigurationStore(builder => builder.UseSqlServer(userConnectionString, options => options.MigrationsAssembly(migrationAssembly)))
               .AddOperationalStore(builder => builder.UseSqlServer(userConnectionString, options => options.MigrationsAssembly(migrationAssembly)))
               .AddAspNetIdentity<ApplicationUser>();
               //.AddResourceOwnerValidator<ResourceOwnerPasswordValidator<ApplicationUser>>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                    new Info
                    {
                        Title = "DCEMV Demo Server",
                        Version = "v1"
                    }
                );

                // Define the OAuth2.0 scheme that's in use (i.e. Implicit Flow)
                c.AddSecurityDefinition("oauth2", new OAuth2Scheme
                {
                    Type = "oauth2",
                    Flow = "implicit",
                    AuthorizationUrl = ConfigSingleton.IdentityServerUrl +  "/connect/authorize",
                    Scopes = new Dictionary<string, string>
                    {
                        { "readAccess", "readAccess" },
                        { "writeAccess", "writeAccess" },
                    }
                });
                // Assign scope requirements to operations based on AuthorizeAttribute
                c.OperationFilter<SecurityRequirementsOperationFilter>();
            });
        }

        //The Configure method is used to specify how the ASP.NET application will respond to HTTP requests. 
        //The request pipeline is configured by adding middleware components to an IApplicationBuilder instance that is provided by dependency injection.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            //app.UseDeveloperExceptionPage();
            app.UseExceptionHandler(options =>
            {
                options.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "text/html";
                    IExceptionHandlerFeature ex = context.Features.Get<IExceptionHandlerFeature>();
                    string err;
                    if (ex != null)
                    {
                        if(ex.Error is TechnicalException)
                        {
                            err = $"Technical Error: {ex.Error.Message}";
                        }
                        else if(ex.Error is ValidationException)
                        {
                            err = $"Validation Error: {ex.Error.Message}";
                        }
                        else
                        {
                            err = $"Exception: {ex.Error.Message}";
                        }
                        await context.Response.WriteAsync(err).ConfigureAwait(false);
                    }
                });
            });

            app.UseIdentity();

            ForwardedHeadersOptions fordwardedHeaderOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            };
            fordwardedHeaderOptions.KnownNetworks.Clear();
            fordwardedHeaderOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(fordwardedHeaderOptions);

            app.UseStaticFiles();

            app.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions
            {
                Authority = ConfigSingleton.IdentityServerUrl,
                RequireHttpsMetadata = false,
            });

            //app.Map("/id", api =>
            //{
            app.UseIdentityServer();

            app.UseMvcWithDefaultRoute();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "DC EMV Demo Server API V1");
                //c.ConfigureOAuth2("swagger-ui", "swagger-ui", null, "Swagger UI");
            });
            //});
        }
    }
}

