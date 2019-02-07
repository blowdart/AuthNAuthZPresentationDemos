using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AuthN
{
    public class Startup
    {
        private static int RequestCount = 0;
        private const string AccessTokenClaim = "urn:tokens:facebook:accesstoken";
        private const string TransformedClaim = "urn:transformedOn";
        private readonly ILogger _logger;

        public Startup(ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger<Startup>();
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddDataProtection(options =>
                {
                    options.ApplicationDiscriminator = "AuthNDemo";
                });

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = FacebookDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.Cookie.Expiration = new System.TimeSpan(0, 15, 0);
                    options.Events = new CookieAuthenticationEvents()
                    {
                        OnValidatePrincipal = Startup.ValidateAsync
                    };
                })
                .AddFacebook(options =>
                {
                    options.AppId = Configuration["Facebook:AppId"];
                    options.AppSecret = Configuration["Facebook:AppSecret"];
                    options.Events = new OAuthEvents()
                    {
                        OnRedirectToAuthorizationEndpoint = context =>
                        {
                            _logger.LogInformation("Redirecting to {0}", context.RedirectUri);
                            context.Response.Redirect(context.RedirectUri);
                            return Task.CompletedTask;
                        },
                        OnCreatingTicket = context =>
                        {
                            _logger.LogInformation("Creating tickets.");
                            var identity = (ClaimsIdentity)context.Principal.Identity;
                            identity.AddClaim(new Claim(AccessTokenClaim, context.AccessToken));
                            return Task.CompletedTask;
                        },
                        OnTicketReceived = context =>
                        {
                            _logger.LogInformation("Ticket received.");
                            return Task.CompletedTask;
                        },
                        OnRemoteFailure = context =>
                        {
                            _logger.LogInformation("Something went horribly wrong.");
                            return Task.CompletedTask;
                        },
                    };
                });

            services.AddTransient<IClaimsTransformation, ClaimsTransformer>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();

            app.Run(async (context) =>
            {
                if (!context.User.Identity.IsAuthenticated)
                {
                    await context.ChallengeAsync();
                }
                await context.Response.WriteAsync("Hello " + context.User.Identity.Name + "!\r");

                var claimsIdentity = (ClaimsIdentity)context.User.Identity;
                var accessTokenClaim = claimsIdentity.Claims.FirstOrDefault(x => x.Type == AccessTokenClaim);

                if (accessTokenClaim != null)
                {
                    await context.Response.WriteAsync("Access Token is available\r");
                }

                var transformedClaim = claimsIdentity.Claims.FirstOrDefault(x => x.Type == TransformedClaim);

                if (transformedClaim != null)
                {
                    await context.Response.WriteAsync("Transformed On " + transformedClaim.Value + "\r");
                }
            });

        }

        public static async Task ValidateAsync(CookieValidatePrincipalContext context)
        {
            if (context.Request.Path.HasValue && context.Request.Path == "/")
            {
                System.Threading.Interlocked.Increment(ref RequestCount);
            }

            if (RequestCount % 5 == 0)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        }

        class ClaimsTransformer : IClaimsTransformation
        {
            public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
            {
                var claimsIdentity = (ClaimsIdentity)principal.Identity;

                if (claimsIdentity.Claims.FirstOrDefault(x => x.Type == TransformedClaim) == null)
                {
                    ((ClaimsIdentity)principal.Identity).AddClaim(
                        new Claim(TransformedClaim, DateTime.Now.ToString()));
                }

                return Task.FromResult(principal);
            }
        }
    }
}
