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
using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace DCEMV.DemoServer
{
    public class Config
    {
        // scopes define the resources in your system
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResource("roles", new[] {"role"}),
            };
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            var api = new ApiResource("DCEMVDemoServer", "DC EMV Demo Server");
            api.Scopes = new[]
            {
                new Scope("readAccess", "readAccess Description"),
                new Scope("writeAccess", "writeAccess Description"),
            };

            return new List<ApiResource>
            {
                api    
            };
        }

        // clients want to access resources (aka scopes)
        public static IEnumerable<Client> GetClients()
        {
            // client credentials client
            return new List<Client>
            {
                new Client
                {
                    ClientId = "clientROP",
                    ClientName = "DC EMV Demo Server Client Resource Owner Password Flow",
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "readAccess",
                        "writeAccess",
                        "roles",
                    },

                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    AllowOfflineAccess = true,
                    //IncludeJwtId = true,
                    //AlwaysIncludeUserClaimsInIdToken = true,
                    
                },

                new Client
                {
                    ClientId = "clientHybrid",
                    ClientName = "DC EMV Demo Server Client Hybrid Flow",
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    //RedirectUris = {"http://localhost:64458/signin-oidc"},
                    //PostLogoutRedirectUris = {"http://localhost:64458"},
                    RedirectUris = { "ms-app://s-1-15-2-2424778797-1941291215-301139157-3294858471-3391296937-98445529-3483318145/" },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "readAccess",
                        "writeAccess",
                        "roles",
                    },

                    AllowedGrantTypes = GrantTypes.Hybrid,
                    RequirePkce = true,
                    AllowOfflineAccess = true,

                },

                new Client
                {
                    ClientId = "swagger-ui",
                    ClientName = "Swagger UI",
                    ClientSecrets =
                    {
                        new Secret("swagger-ui".Sha256())
                    },

                    RedirectUris = new[] { "https://localhost:44354/swagger/o2c.html" },
                    AllowAccessTokensViaBrowser = true,
                    
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "readAccess",
                        "writeAccess",
                        "roles",
                    },

                    AllowedGrantTypes = GrantTypes.Implicit,
                }
            };
        }

        //public static List<TestUser> GetUsers()
        //{
        //    return new List<TestUser>
        //    {
        //        new TestUser
        //        {
        //            SubjectId = "d10841fe-d702-434b-9050-745eea366b87",
        //            Username = "testusername",
        //            Password = "password",

        //            Claims = new List<Claim>
        //            {
        //                new Claim("name", "TestName"),
        //                new Claim("role", "organizer"),
        //            }
        //        }
        //    };
        //}
    }
}
