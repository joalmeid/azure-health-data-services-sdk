﻿using System;
using System.Reflection;
using Azure.Health.DataServices.Tests.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AzureHealth.DataServices.Tests.Assets
{
    public static class ConfigExtensions
    {
        public static IServiceCollection AddServiceConfiguration(this IServiceCollection services,
            out ServiceConfig config)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.SetBasePath(Environment.CurrentDirectory)
                .AddUserSecrets(Assembly.GetExecutingAssembly(), false)
                .AddEnvironmentVariables("PROXY_");
            IConfigurationRoot root = builder.Build();
            config = new ServiceConfig();
            root.Bind(config);
            services.AddSingleton(config);

            return services;
        }

        public static IServiceCollection AddServiceConfiguration(this IServiceCollection services,
            out EventHubConfig settings)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.SetBasePath(Environment.CurrentDirectory)
                .AddUserSecrets(Assembly.GetExecutingAssembly(), false)
                .AddEnvironmentVariables("PROXY_");
            IConfigurationRoot root = builder.Build();
            settings = new EventHubConfig();
            root.Bind(settings);
            services.AddSingleton(settings);

            return services;
        }
        public static IServiceCollection AddServiceConfiguration(this IServiceCollection services,
            out ServiceBusConfig settings)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.SetBasePath(Environment.CurrentDirectory)
                .AddUserSecrets(Assembly.GetExecutingAssembly(), false)
                .AddEnvironmentVariables("PROXY_");
            IConfigurationRoot root = builder.Build();
            settings = new ServiceBusConfig();
            root.Bind(settings);
            services.AddSingleton(settings);

            return services;
        }
    }
}
