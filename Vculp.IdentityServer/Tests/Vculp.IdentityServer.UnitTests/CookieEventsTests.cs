using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Vculp.IdentityServer.UnitTests
{
    public class CookieEventsTests
    {
        [Fact]
        public async Task SigningIn_AddsAuthenticatedFlag_ToCookie_WhenRequestPath_IsTo_AuthorizeEndpoint()
        {
            //Arrange
            var sut = GetTestInstance();

            var context = new CookieSigningInContext(
                            new DefaultHttpContext(),
                            new AuthenticationScheme("test-scheme", "Test Scheme", typeof(CookieAuthenticationHandler)),
                            new CookieAuthenticationOptions(),
                            new ClaimsPrincipal(),
                            new AuthenticationProperties(),
                            new CookieOptions());

            context.HttpContext.Request.Path = "/connect/authorize";

            //Act
            await sut.SigningIn(context);

            //Assert
            Assert.Contains("has-authenticated", context.Properties.Items);
        }

        [Fact]
        public async Task SigningIn_DoesNotAdd_AuthenticatedFlag_ToCookie_WhenRequestPath_IsNotTo_AuthorizeEndpoint()
        {
            //Arrange
            var sut = GetTestInstance();

            var context = new CookieSigningInContext(
                            new DefaultHttpContext(),
                            new AuthenticationScheme("test-scheme", "Test Scheme", typeof(CookieAuthenticationHandler)),
                            new CookieAuthenticationOptions(),
                            new ClaimsPrincipal(),
                            new AuthenticationProperties(),
                            new CookieOptions());

            context.HttpContext.Request.Path = "/my-test-request-path";

            //Act
            await sut.SigningIn(context);

            //Assert
            Assert.DoesNotContain("has-authenticated", context.Properties.Items);
        }

        [Fact]
        public async Task SigningIn_DoesNotAdd_AuthenticatedFlag_ToCookie_WhenRequestPath_IsTo_AuthorizeEndpoint_And_AuthenticationItems_AlreadyContains_AuthenticatedFlag()
        {
            //Arrange
            var sut = GetTestInstance();

            var context = new CookieSigningInContext(
                            new DefaultHttpContext(),
                            new AuthenticationScheme("test-scheme", "Test Scheme", typeof(CookieAuthenticationHandler)),
                            new CookieAuthenticationOptions(),
                            new ClaimsPrincipal(),
                            new AuthenticationProperties(),
                            new CookieOptions());

            context.HttpContext.Request.Path = "/connect/authorize";
            context.Properties.Items.Add("has-authenticated", "true");

            //Act
            await sut.SigningIn(context);

            //Assert
            Assert.Single(context.Properties.Items);
            Assert.Contains("has-authenticated", context.Properties.Items);
        }

        [Fact]
        public async Task ValidatePrincipal_InvalidatesUserPrincipal_GivenRequest_ToAuthorizeEndpoint_FromCustomerAppClient_WhenCookieContains_HasAuthenticatedFlag()
        {
            //Arrange
            var sut = GetTestInstance();

            var context = new CookieValidatePrincipalContext(
                            new DefaultHttpContext(),
                            new AuthenticationScheme("test-scheme", "Test Scheme", typeof(CookieAuthenticationHandler)),
                            new CookieAuthenticationOptions(),
                            new AuthenticationTicket(new ClaimsPrincipal(), "test-scheme"));

            var queryStringValues = new Dictionary<string, StringValues>
            {
                {"client_id", new StringValues(ClientIdConstants.VculpAppClient) }
            };

            context.HttpContext.Request.Path = "/connect/authorize";
            context.HttpContext.Request.Query = new QueryCollection(queryStringValues);
            context.Properties.Items.Add("has-authenticated", "true");

            //Act
            await sut.ValidatePrincipal(context);

            //Assert
            Assert.Null(context.Principal);
        }

        [Fact]
        public async Task ValidatePrincipal_InvalidatesUserPrincipal_GivenRequest_ToAuthorizeEndpoint_FromHaulierAppClient_WhenCookieContains_HasAuthenticatedFlag()
        {
            //Arrange
            var sut = GetTestInstance();

            var context = new CookieValidatePrincipalContext(
                            new DefaultHttpContext(),
                            new AuthenticationScheme("test-scheme", "Test Scheme", typeof(CookieAuthenticationHandler)),
                            new CookieAuthenticationOptions(),
                            new AuthenticationTicket(new ClaimsPrincipal(), "test-scheme"));

            var queryStringValues = new Dictionary<string, StringValues>
            {
                {"client_id", new StringValues(ClientIdConstants.VculpAppClient) }
            };

            context.HttpContext.Request.Path = "/connect/authorize";
            context.HttpContext.Request.Query = new QueryCollection(queryStringValues);
            context.Properties.Items.Add("has-authenticated", "true");

            //Act
            await sut.ValidatePrincipal(context);

            //Assert
            Assert.Null(context.Principal);
        }

        [Fact]
        public async Task ValidatePrincipal_DoesNot_InvalidateUserPrincipal_GivenRequest_ToAuthorizeEndpoint_FromCustomerAppClient_WhenCookie_DoesNotContains_HasAuthenticatedFlag()
        {
            //Arrange
            var sut = GetTestInstance();

            var context = new CookieValidatePrincipalContext(
                            new DefaultHttpContext(),
                            new AuthenticationScheme("test-scheme", "Test Scheme", typeof(CookieAuthenticationHandler)),
                            new CookieAuthenticationOptions(),
                            new AuthenticationTicket(new ClaimsPrincipal(), "test-scheme"));

            var queryStringValues = new Dictionary<string, StringValues>
            {
                {"client_id", new StringValues(ClientIdConstants.VculpAppClient) }
            };

            context.HttpContext.Request.Path = "/connect/authorize";
            context.HttpContext.Request.Query = new QueryCollection(queryStringValues);

            //Act
            await sut.ValidatePrincipal(context);

            //Assert
            Assert.NotNull(context.Principal);
        }

        [Fact]
        public async Task ValidatePrincipal_DoesNot_InvalidateUserPrincipal_GivenRequest_ToAuthorizeEndpoint_FromClient_ThatIsNot_TheXamarinClient()
        {
            //Arrange
            var sut = GetTestInstance();

            var context = new CookieValidatePrincipalContext(
                            new DefaultHttpContext(),
                            new AuthenticationScheme("test-scheme", "Test Scheme", typeof(CookieAuthenticationHandler)),
                            new CookieAuthenticationOptions(),
                            new AuthenticationTicket(new ClaimsPrincipal(), "test-scheme"));

            var queryStringValues = new Dictionary<string, StringValues>
            {
                {"client_id", new StringValues("my-test-client-id") }
            };

            context.Properties.Items.Add("has-authenticated", "true");
            context.HttpContext.Request.Path = "/connect/authorize";
            context.HttpContext.Request.Query = new QueryCollection(queryStringValues);

            //Act
            await sut.ValidatePrincipal(context);

            //Assert
            Assert.NotNull(context.Principal);
        }

        [Fact]
        public async Task ValidatePrincipal_DoesNot_InvalidateUserPrincipal_GivenRequest_ToAuthorizeEndpoint_ThatHasNo_ClientId_InQueryString()
        {
            //Arrange
            var sut = GetTestInstance();

            var context = new CookieValidatePrincipalContext(
                            new DefaultHttpContext(),
                            new AuthenticationScheme("test-scheme", "Test Scheme", typeof(CookieAuthenticationHandler)),
                            new CookieAuthenticationOptions(),
                            new AuthenticationTicket(new ClaimsPrincipal(), "test-scheme"));

            var queryStringValues = new Dictionary<string, StringValues>();

            context.Properties.Items.Add("has-authenticated", "true");
            context.HttpContext.Request.Path = "/connect/authorize";
            context.HttpContext.Request.Query = new QueryCollection(queryStringValues);

            //Act
            await sut.ValidatePrincipal(context);

            //Assert
            Assert.NotNull(context.Principal);
        }

        [Fact]
        public async Task ValidatePrincipal_DoesNot_InvalidateUserPrincipal_GivenRequest_ThatIsNotTo_AuthorizeEndpoint()
        {
            //Arrange
            var sut = GetTestInstance();

            var context = new CookieValidatePrincipalContext(
                            new DefaultHttpContext(),
                            new AuthenticationScheme("test-scheme", "Test Scheme", typeof(CookieAuthenticationHandler)),
                            new CookieAuthenticationOptions(),
                            new AuthenticationTicket(new ClaimsPrincipal(), "test-scheme"));

            var queryStringValues = new Dictionary<string, StringValues>
            {
                {"client_id", new StringValues("my-test-client-id") }
            };

            context.Properties.Items.Add("has-authenticated", "true");
            context.HttpContext.Request.Path = "/my-test-endpoint-name";
            context.HttpContext.Request.Query = new QueryCollection(queryStringValues);

            //Act
            await sut.ValidatePrincipal(context);

            //Assert
            Assert.NotNull(context.Principal);
        }

        private CookieEvents GetTestInstance()
        {
            return new CookieEvents();
        }
    }
}
