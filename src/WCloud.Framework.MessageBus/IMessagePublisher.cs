﻿using System.Threading;
using System.Threading.Tasks;
using WCloud.Core.MessageBus;

namespace WCloud.Framework.MessageBus
{
    /// <summary>
    /// 发送消息
    /// </summary>
    public interface IMessagePublisher
    {
        Task PublishAsync<T>(string key, T model, CancellationToken cancellationToken = default) where T : class, IMessageBody;
    }
}
