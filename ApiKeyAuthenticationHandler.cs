using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
namespace Api_Key_Authentication
{
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly AppDbContext _dbContext;

        public ApiKeyAuthenticationHandler(
                                           IOptionsMonitor<AuthenticationSchemeOptions> options,
                                           ILoggerFactory logger,
                                           System.Text.Encodings.Web.UrlEncoder encoder,
                                           AppDbContext dbContext)
                                    : base(options, logger, encoder)
        {
            _dbContext = dbContext;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("X-API-KEY", out var extractedApiKey))
            {
                return AuthenticateResult.Fail("API Key is missing.");
            }

            var apiKeys = await _dbContext.ApiKeys.Where(k => k.IsActive).ToListAsync();

            var key = apiKeys.FirstOrDefault(k => BCrypt.Net.BCrypt.Verify(extractedApiKey, k.KeyHash));


            if (key == null)
            {
                return AuthenticateResult.Fail("Invalid API Key.");
            }

            var claims = new[] { new Claim(ClaimTypes.Name, "ApiKeyUser") };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}
