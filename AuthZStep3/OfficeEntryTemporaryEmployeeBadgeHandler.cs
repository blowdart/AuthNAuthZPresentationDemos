using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace AuthZ
{
    public class OfficeEntryTemporaryEmployeeBadgeHandler : AuthorizationHandler<OfficeEntryRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OfficeEntryRequirement requirement)
        {
            {
                if (!context.User.HasClaim(c => c.Type == "TemporaryBadgeExpiry" &&
                                                c.Issuer == "https://contoso.com"))
                {
                    return Task.CompletedTask;
                }

                var temporaryBadgeExpiry =
                    Convert.ToDateTime(context.User.FindFirst(
                                           c => c.Type == "TemporaryBadgeExpiry" &&
                                           c.Issuer == "https://contoso.com").Value);

                if (temporaryBadgeExpiry > DateTime.Now)
                {
                    context.Succeed(requirement);
                }

                return Task.CompletedTask;
            }
        }
    }
}
