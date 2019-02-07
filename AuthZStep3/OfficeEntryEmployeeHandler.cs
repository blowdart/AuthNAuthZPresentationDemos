using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace AuthZ
{
    public class OfficeEntryEmployeeHandler : AuthorizationHandler<OfficeEntryRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OfficeEntryRequirement requirement)
        {
            {
                if (!context.User.HasClaim(c => c.Type == "EmployeeId" &&
                                                c.Issuer == "https://contoso.com"))
                {
                    return Task.CompletedTask;
                }

                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }
    }
}
