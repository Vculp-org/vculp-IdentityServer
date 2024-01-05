using System.Collections.Generic;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;

namespace Vculp.IdentityServer
{
    public class Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource
                {
                    Name = "Vculp.Api",
                    Description = "Vculp",
                    Scopes = {
                        ScopeConstants.Read,
                        ScopeConstants.Write,
                        // ScopeConstants.DriversTerminalRead,
                        // ScopeConstants.DriversTerminalWrite
                    },
                    UserClaims = { "name", "admin_id"}
                }
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {

                new Client
                {
                    ClientId = "c16a7279-738c-458f-8e77-25e42eb965ff",
                    AllowedGrantTypes = GrantTypes.Code,
                    RedirectUris =
                    {
                        "vculp://auth"
                    },
                    AllowedScopes = { IdentityServerConstants.StandardScopes.OpenId, IdentityServerConstants.StandardScopes.Profile, "read", "write" },
                    RequireConsent = false,
                    RequirePkce = true,
                    RequireClientSecret = false,
                    AllowOfflineAccess = true
                },

                new Client
                {
                    ClientId = ClientIdConstants.VculpSwaggerUiClient,
                    ClientName = "Swagger UI",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,

                    RedirectUris =
                    {
                        "http://localhost:5000/swagger/o2c.html",
                        "http://localhost:5000/swagger/oauth2-redirect.html"
                    },
                    PostLogoutRedirectUris =
                    {
                        "http://localhost:5000"
                    },
                    RequireConsent = false,
                    AllowOfflineAccess = false,

                    AllowedScopes =
                    {
                        "read",
                        "write"
                    },

                    AccessTokenType = AccessTokenType.Jwt
                }
            };
        }

        public static IEnumerable<ApiScope> GetScopes()
        {
            return new List<ApiScope>
            {
                new ApiScope
                {
                    Name = ScopeConstants.Read,
                    DisplayName = "Read",
                },
                new ApiScope
                {
                    Name = ScopeConstants.Write,
                    DisplayName = "Write",
                }
            };
        }
    }
}
