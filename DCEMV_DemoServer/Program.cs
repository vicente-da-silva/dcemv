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
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using IdentityServer4.EntityFramework.Mappers;
using DCEMV.DemoServer.Persistence.Credentials;
using DCEMV.DemoServer.Persistence.Api;
using System.Threading;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Configuration;

namespace DCEMV.DemoServer
{
    public static class Ext
    {
        public static IWebHost Migrate(this IWebHost webhost)
        {
            using (var scope = webhost.Services.GetService<IServiceScopeFactory>().CreateScope())
            {
                using (var db1 = scope.ServiceProvider.GetRequiredService<IdentityUserDbContext>())
                {
                    db1.Database.Migrate();
                }
                using (var db2 = scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>())
                {
                    db2.Database.Migrate();
                }
                using (var context = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>())
                {
                    context.Database.Migrate();

                    if (!context.Clients.Any())
                    {
                        foreach (IdentityServer4.Models.Client client in Config.GetClients())
                        {
                            context.Clients.Add(client.ToEntity());
                        }
                        context.SaveChanges();
                    }

                    if (!context.IdentityResources.Any())
                    {
                        foreach (IdentityServer4.Models.IdentityResource resource in Config.GetIdentityResources())
                        {
                            context.IdentityResources.Add(resource.ToEntity());
                        }
                        context.SaveChanges();
                    }

                    if (!context.ApiResources.Any())
                    {
                        foreach (IdentityServer4.Models.ApiResource resource in Config.GetApiResources())
                        {
                            context.ApiResources.Add(resource.ToEntity());
                        }
                        context.SaveChanges();
                    }
                }
                using (var db4 = scope.ServiceProvider.GetRequiredService<ApiDbContext>())
                {
                    db4.Database.Migrate();
                }
            }
            return webhost;
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                //Console.Title = "DCEMV Demo Server";
                BuildWebHost(args).Migrate().Run();
            }
            catch (Exception ex)
            {
                //do nothing so we can check container logs
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine("DB:" + Environment.GetEnvironmentVariable("DB_SERVER_NAME"));
                System.Diagnostics.Debug.WriteLine("ID:" + Environment.GetEnvironmentVariable("ID_SERVER_URL"));
            }
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls(ConfigSingleton.ThisServerUrl)
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    // delete all default configuration providers
                    config.Sources.Clear();
                    config.AddJsonFile("appsettings.json", optional: true);
                })
                .Build();
    }
}
