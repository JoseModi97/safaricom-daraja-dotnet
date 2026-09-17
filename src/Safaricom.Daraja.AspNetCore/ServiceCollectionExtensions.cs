using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Safaricom.Daraja.Models;

namespace Safaricom.Daraja.AspNetCore
{
    /// <summary>Extension methods for registering <see cref="DarajaClient"/> in an <see cref="IServiceCollection"/>.</summary>
    public static class ServiceCollectionExtensions
    {
        public const string DefaultConfigurationSection = "Daraja";

        /// <summary>Registers <see cref="DarajaClient"/> configured via a delegate.</summary>
        public static IServiceCollection AddDaraja(this IServiceCollection services, Action<DarajaConfig> configure)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            var config = new DarajaConfig();
            configure(config);

            return services.AddDarajaCore(config);
        }

        /// <summary>Registers <see cref="DarajaClient"/> bound from an <see cref="IConfiguration"/> section (default: "Daraja").</summary>
        public static IServiceCollection AddDaraja(this IServiceCollection services, IConfiguration configuration, string sectionName = DefaultConfigurationSection)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            var config = new DarajaConfig();
            configuration.GetSection(sectionName).Bind(config);

            return services.AddDarajaCore(config);
        }

        private static IServiceCollection AddDarajaCore(this IServiceCollection services, DarajaConfig config)
        {
            services.AddSingleton(config);
            services.AddHttpClient(nameof(DarajaClient));

            services.AddSingleton(sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient(nameof(DarajaClient));
                return new DarajaClient(config, httpClient);
            });

            return services;
        }
    }
}
