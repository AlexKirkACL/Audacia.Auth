﻿using Audacia.Auth.OpenIddict.Common.Configuration;
using Audacia.Auth.OpenIddict.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Audacia.Auth.OpenIddict.QuartzCleanup
{
    /// <summary>
    /// Extensions to the <see cref="IServiceCollection"/> type.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds OpenIddict services to the given <paramref name="services"/>.
        /// Automatic cleanup of expired tokens via a hosted service is also set up.
        /// </summary>
        /// <typeparam name="TUser">The user type.</typeparam>
        /// <typeparam name="TKey">The type of the user's primary key.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> object to which to add the services.</param>
        /// <param name="optionsBuilder">A delegate containing the additional OpenIddict configuration.</param>
        /// <param name="openIdConnectConfig">An <see cref="OpenIdConnectConfig"/> object, which represents the configuration of the authorization server.</param>
        /// <param name="hostingEnvironment">The current <see cref="IWebHostEnvironment"/>.</param>
        /// <returns>An instance of <see cref="OpenIddictBuilder"/> to which further configuration can be performed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="openIdConnectConfig"/> is <see langword="null"/>.</exception>
        public static OpenIddictBuilder AddOpenIddictWithCleanup<TUser, TKey>(
            this IServiceCollection services,
            Action<OpenIddictCoreBuilder> optionsBuilder,
            OpenIdConnectConfig openIdConnectConfig,
            IWebHostEnvironment hostingEnvironment)
                where TUser : IdentityUser<TKey>
                where TKey : IEquatable<TKey>
        {
            Action<OpenIddictCoreBuilder> quartzOptions = builder => builder.UseQuartz();
            Action<OpenIddictCoreBuilder> combinedBuilder = optionsBuilder + quartzOptions;

            return services
                .AddQuartz(options =>
                {
                    options.UseMicrosoftDependencyInjectionJobFactory();
                    options.UseSimpleTypeLoader();
                    options.UseInMemoryStore();
                })
                .AddQuartzHostedService(options => options.WaitForJobsToComplete = true)
                .AddOpenIddict<TUser, TKey>(combinedBuilder, openIdConnectConfig, hostingEnvironment);
        }
    }
}