using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Vculp.IdentityServer
{
    public class CookieEvents : CookieAuthenticationEvents
    {
        private const string _authorizationEndpointAuthIdentifierKey = "has-authenticated";

        public override Task SigningIn(CookieSigningInContext context)
        {
            //If the request is for the Authorize endpoint and the user has not signed in before
            //persist a flag to the cookie.
            if (IsRequestToAuthorizeEndpoint(context.Request) &&
                !context.Properties.Items.ContainsKey(_authorizationEndpointAuthIdentifierKey))
            {
                context.Properties.Items.Add(_authorizationEndpointAuthIdentifierKey, "true");
            }

            return base.SigningIn(context);
        }

        public override Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            //If the request is a native app sign in and the user has already authorized with the auth endpoint,
            //invalidate the principal. This ensures that sign in from the native app always goes through the login process
            //and redirect, even if the users browser has a valid authentication cookie for Identity Server.
            if (IsNativeAppSignIn(context.Request) &&
                context.Properties.Items.ContainsKey(_authorizationEndpointAuthIdentifierKey))
            {
                context.RejectPrincipal();
                return Task.CompletedTask;
            }

            return base.ValidatePrincipal(context);
        }

        private bool IsRequestToAuthorizeEndpoint(HttpRequest request)
        {
            var requestPath = request.Path.HasValue ? request.Path.Value : string.Empty;
            return requestPath.Contains("connect/authorize");
        }

        private bool IsNativeAppSignIn(HttpRequest request)
        {
            var requestPath = request.Path.HasValue ? request.Path.Value : string.Empty;

            return IsRequestToAuthorizeEndpoint(request) &&
                   request.Query.TryGetValue("client_id", out StringValues clientId) &&
                   clientId.Any(x => x == ClientIdConstants.VculpAppClient);
        }
    }
}
