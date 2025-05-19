using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace apiDDKMA.Handlers
{
    public class AllowAnonymousAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public AllowAnonymousAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[] { new Claim(ClaimTypes.Name, "Anonymous") };
            var identity = new ClaimsIdentity(claims, "Anonymous");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Anonymous");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}