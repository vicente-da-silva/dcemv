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

namespace DCEMV.DemoServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                //Console.Title = "DCEMV Demo Server";

                IWebHost host = new WebHostBuilder()
                    .UseUrls(ConfigSingleton.ThisServerUrl)
                    .UseKestrel()
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    //.UseIISIntegration()
                    .UseStartup<Startup>()
                    .Build();

                SanityCheckDatabase(host);
                host.Run();
            }
            catch (Exception ex)
            {
                //do nothing so we can check container logs
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine("DB:" + Environment.GetEnvironmentVariable("DB_SERVER_NAME"));
                System.Diagnostics.Debug.WriteLine("ID:" + Environment.GetEnvironmentVariable("ID_SERVER_URL"));
            }
        }
        
        private static void SanityCheckDatabase(IWebHost host)
        {
            IdentityUserDbContext db1 = (IdentityUserDbContext)host.Services.GetService(typeof(IdentityUserDbContext));
            PersistedGrantDbContext db2 = (PersistedGrantDbContext)host.Services.GetService(typeof(PersistedGrantDbContext));
            ConfigurationDbContext db3 = (ConfigurationDbContext)host.Services.GetService(typeof(ConfigurationDbContext));
            ApiDbContext db4 = (ApiDbContext)host.Services.GetService(typeof(ApiDbContext));
            
            db1.Database.Migrate();
            db2.Database.Migrate();
            db3.Database.Migrate();
            db4.Database.Migrate();

            InitializeIdentityDatabase(host);
        }

        private static void InitializeIdentityDatabase(IWebHost host)
        {
            using (var serviceScope = host.Services.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
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
        }
    }
}
