﻿using Lib.data;
using Lib.ioc;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;
using System.Linq;
using WCloud.Framework.MessageBus.Rabbitmq_.Providers;

namespace WCloud.Framework.MessageBus.Rabbitmq_
{
    /// <summary>
    /// 不同消息中间件支持的特性不一样，没法兼顾灵活和通用。
    /// 这里的interface只针对rabbitmq。
    /// 如果要设计通用的iconsumer和iproducer，那么要牺牲部分特性。
    /// 设计：
    /// lib.mq.rabbit_provider:lib.iproducer->lib.rabbitmq
    /// lib.mq.kafka_provider:lib.iproducer->lib.kafka
    /// etc...
    /// </summary>
    public static class RabbitmqBootstrap
    {
        /// <summary>
        /// 使用消息队列
        /// </summary>
        public static IServiceCollection UseRabbitMq(this IServiceCollection collection, RabbitMqOption configuration)
        {
            var factory = new ConnectionFactory
            {
                AutomaticRecoveryEnabled = true,
                UseBackgroundThreadsForIO = true,
                HostName = configuration.HostName,
                UserName = configuration.UserName,
                Password = configuration.Password,
                VirtualHost = configuration.VirtualHost,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(1)
            };
            if (configuration.SocketTimeout != null)
            {
                factory.RequestedConnectionTimeout = factory.SocketWriteTimeout = factory.SocketReadTimeout
                    = configuration.SocketTimeout.Value;
            }
            if (configuration.ContinuationTimeout != null)
            {
                factory.ContinuationTimeout = factory.HandshakeContinuationTimeout = configuration.ContinuationTimeout.Value;
            }

            collection.AddDisposableSingleInstanceService(new RabbitConnectionWrapper(factory));
            collection.AddSingleton(provider => provider.Resolve_<RabbitConnectionWrapper>().Connection);
            //add default message producer
            collection.AddSingleton<IRabbitMqProducer>(provider =>
            {
                var con = provider.Resolve_<IConnection>();
                var serializer = provider.Resolve_<ISerializeProvider>();
                var res = new RabbitMqProducer(con, serializer);
                return res;
            });

            return collection;
        }

        /// <summary>
        /// 找到依赖注入中的消费（单例），开启他们
        /// </summary>
        public static IServiceProvider StartConcume(this IServiceProvider provider)
        {
            var consumers = provider.ResolveAll_<IRabbitMqConsumer>().ToList();
            try
            {
                foreach (var m in consumers)
                {
                    m.StartConsume();
                }
            }
            catch
            {
                consumers.ForEach(x => x.Dispose());
                throw;
            }

            return provider;
        }
    }
}
