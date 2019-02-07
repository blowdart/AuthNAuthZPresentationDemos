using AuthN;
using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Builder
{
    public static class QueryStringAuthenicationAppBuilderExtensions
    {
        public static AuthenticationBuilder AddQueryString(
            this AuthenticationBuilder builder)
            => builder.AddQueryString(
                QueryStringAuthenticationDefaults.AuthenticationScheme);

        public static AuthenticationBuilder AddQueryString(
            this AuthenticationBuilder builder,
            string authenticationScheme)
            => builder.AddQueryString(
                authenticationScheme,
                configureOptions: null);

        public static AuthenticationBuilder AddQueryString(
            this AuthenticationBuilder builder,
            Action<AuthenticationSchemeOptions> configureOptions)
            => builder.AddQueryString(
                QueryStringAuthenticationDefaults.AuthenticationScheme,
                configureOptions);

        public static AuthenticationBuilder AddQueryString(
            this AuthenticationBuilder builder,
            string authenticationScheme,
            Action<AuthenticationSchemeOptions> configureOptions)
        {
            return builder.AddScheme<
                AuthenticationSchemeOptions,
                QueryStringAuthenticationHandler>(
                    authenticationScheme,
                    configureOptions);
        }
    }
}
