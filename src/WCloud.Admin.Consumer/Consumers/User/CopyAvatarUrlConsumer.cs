﻿using FluentAssertions;
using Lib.cache;
using Lib.extension;
using Lib.ioc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using WCloud.CommonService.Application.FileUpload;
using WCloud.Core;
using WCloud.Core.Cache;
using WCloud.Framework.MessageBus;
using WCloud.Member.DataAccess.EF;
using WCloud.Member.Domain.User;
using WCloud.Member.Shared.MessageBody;

namespace WCloud.Admin.Consumer.Consumers
{
    /// <summary>
    /// 复制微信头像到自己服务器
    /// </summary>
    [QueueConfig(ConfigSet.MessageBus.Queue.User)]
    public class CopyAvatarUrlConsumer : IMessageConsumer<CopyAvatarMessage>, Lib.core.IFinder
    {
        private readonly IServiceProvider provider;
        public CopyAvatarUrlConsumer(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public async Task Consume(IMessageConsumeContext<CopyAvatarMessage> context)
        {
            try
            {
                context.Message.UserUID.Should().NotBeNullOrEmpty();
                context.Message.AvatarUrl.Should().NotBeNullOrEmpty();

                var user_uid = context.Message.UserUID;

                var repo = provider.Resolve_<IMSRepository<UserEntity>>();
                var db = repo.Database;

                var user = await db.Set<UserEntity>().FirstOrDefaultAsync(x => x.UID == user_uid);
                user.Should().NotBeNull();

                var client = provider.Resolve_<IHttpClientFactory>().CreateClient("copy_avatar_client");

                var fileUploadService = provider.Resolve_<IFileUploadService>();

                user.UserImg = await this.__download_and_upload__(client, fileUploadService, context.Message.AvatarUrl);

                await db.SaveChangesAsync();

                var cache = provider.ResolveDistributedCache_();
                var keyManager = provider.Resolve_<ICacheKeyManager>();

                await cache.RemoveAsync(keyManager.UserLoginInfo(user_uid));
            }
            catch (Exception e)
            {
                var _logger = provider.Resolve_<ILogger<CopyAvatarUrlConsumer>>();
                _logger.AddErrorLog(nameof(CopyAvatarUrlConsumer), e);
            }
        }

        async Task<string> __download_and_upload__(HttpClient client, IFileUploadService fileUploadService, string url)
        {
            using var res = await client.GetAsync(url);
            res.EnsureSuccessStatusCode();

            var bs = await res.Content.ReadAsByteArrayAsync();

            var data = await fileUploadService.Upload(bs, "avatar.png", "wx-login-copy-avatar");
            data.ThrowIfNotSuccess();

            data.Data.Url.Should().NotBeNullOrEmpty("url为空");

            return data.Data.Url;
        }
    }
}
