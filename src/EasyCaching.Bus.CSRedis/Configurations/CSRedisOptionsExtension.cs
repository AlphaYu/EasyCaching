﻿namespace EasyCaching.Bus.CSRedis
{
    using System;
    using EasyCaching.Core.Bus;
    using EasyCaching.Core.Configurations;
    using EasyCaching.Core.Serialization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Redis options extension.
    /// </summary>
    internal sealed class CSRedisOptionsExtension : IEasyCachingOptionsExtension
    {
        /// <summary>
        /// The name.
        /// </summary>
        private const string _name = "easycachingbus";

        /// <summary>
        /// The configure.
        /// </summary>
        private readonly Action<CSRedisBusOptions> _configure;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.CSRedis.RedisOptionsExtension"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="configure">Configure.</param>
        public CSRedisOptionsExtension(Action<CSRedisBusOptions> configure)
        {
            this._configure = configure;
        }

        /// <summary>
        /// Adds the services.
        /// </summary>
        /// <param name="services">Services.</param>
        public void AddServices(IServiceCollection services)
        {
            services.AddOptions();

            services.TryAddSingleton<IEasyCachingSerializer, DefaultBinaryFormatterSerializer>();

            services.Configure(_name, _configure);

            services.AddSingleton<EasyCachingCSRedisClient>(x =>
            {
                var optionsMon = x.GetRequiredService<IOptionsMonitor<CSRedisBusOptions>>();
                var options = optionsMon.Get(_name);

                var conns = options.ConnectionStrings;
                var rule = options.NodeRule;

                if (conns.Count == 1)
                {
                    var redisClient = new EasyCachingCSRedisClient(_name, conns[0]);
                    return redisClient;
                }
                else
                {
                    var redisClient = new EasyCachingCSRedisClient(_name, rule, conns.ToArray());
                    return redisClient;
                }
            });

            services.AddSingleton<IEasyCachingBus, DefaultCSRedisBus>();
        }

        /// <summary>
        /// Withs the services.
        /// </summary>
        /// <param name="services">Services.</param>
        public void WithServices(IApplicationBuilder services)
        {
            // Method intentionally left empty.
        }
    }
}
