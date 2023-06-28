using System.Security.Claims;
using System.Text.Encodings.Web;

using FirebaseAdmin;
using FirebaseAdmin.Auth;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Server.Services.AuthenticaionService
{
    public class FirebaseAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private FirebaseApp _firebaseApp;

        public FirebaseAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, FirebaseApp firebaseApp)
            : base(options, logger, encoder, clock)
        {
            _firebaseApp = firebaseApp;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Context.Request.Headers.ContainsKey("Authorization"))
            {
                return AuthenticateResult.NoResult();
            }


            string? authorizationHeader = Context.Request.Headers["Authorization"];
            if (authorizationHeader == null)
            {
                return AuthenticateResult.NoResult();
            }

            if (!authorizationHeader.StartsWith("Bearer "))
            {
                return AuthenticateResult.Fail("Invalid Scheme");
            }

            string token = authorizationHeader.Substring("Bearer ".Length);
            try
            {
                FirebaseToken firebaseToken = await FirebaseAuth.GetAuth(_firebaseApp).VerifyIdTokenAsync(token);
                List<Claim> claims = GetClaims(firebaseToken.Claims);
                ClaimsIdentity claimIdentity = new ClaimsIdentity(claims, nameof(FirebaseAuthenticationHandler));
                List<ClaimsIdentity> claimIdentities = new List<ClaimsIdentity>();
                claimIdentities.Add(claimIdentity);
                ClaimsPrincipal principal = new ClaimsPrincipal(claimIdentities);

                AuthenticationTicket ticket = new AuthenticationTicket(principal, "Bearer");
                return AuthenticateResult.Success(ticket);
            }
            catch (Exception ex)
            {
                return AuthenticateResult.Fail(ex);
            }
        }


        private List<Claim> GetClaims(IReadOnlyDictionary<string, object> claims)
        {
            return new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, claims["user_id"]?.ToString()),
            };
        }
    }
}
