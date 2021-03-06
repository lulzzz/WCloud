﻿using FluentAssertions;
using Lib.data;
using Lib.extension;
using Lib.helper;
using Lib.ioc;
using Lib.redis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Volo.Abp.VirtualFileSystem;
using WCloud.Core;
using WCloud.Core.Cache;
using WCloud.Core.PollyExtension;
using WCloud.Core.Validator;
using WCloud.Framework.Filters;
using WCloud.Framework.Logging;
using WCloud.Framework.Middleware;
using WCloud.Framework.Startup;

namespace WCloud.Framework.MVC
{
    public static class Bootstrap
    {
        public static IServiceProvider CurrentServiceProvider(this HttpContext context)
        {
            var res = context.RequestServices ?? throw new Exception("无法获取当前ioc scope");
            return res;
        }

        public static IServiceCollection AddHttpContextAccessor_(this IServiceCollection collection)
        {
            collection.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            return collection;
        }

        public static IApplicationBuilder UseLibMvcMiddleware(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<SetEncodingMiddleware>();
            return builder;
        }

        /// <summary>
        /// 添加没有平台依赖的注册项
        /// </summary>
        public static IServiceCollection AddBasicNoDependencyServices(this IServiceCollection collection)
        {
            var config = collection.GetConfiguration();
            var env = collection.GetHostingEnvironment();

            //解决中文被编码
            collection.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));
            //use httpcontext
            collection.AddHttpContextAccessor_();
            collection.AddHttpClient();

            //基础注册项
            collection.AddSingleton<ISerializeProvider, DefaultSerializeProvider>();
            collection.AddSingleton<IStrategyFactory, StrategyFactory>();
            collection.AddSingleton<ICommonExceptionHandler, CommonExceptionHandler>();
            //dto验证
            collection.AddFluentValidatorHelper();

            //缓存
            collection.AddScoped<ICacheKeyManager, CacheKeyManager>();
            collection.AddCacheProvider_().AddMemoryCache().AddDistributedMemoryCache();

            //日志
            collection.AddLogging(builder =>
            {
                builder.ClearProviders();
#if DEBUG
                //console debug
                builder.AddConsole().AddDebug();
#endif
                builder.__add_nlog__(config, Path.Combine(env.ContentRootPath, "nlog.config"));
                builder.__try_add_sentry__(config);
                builder.__add_logger_filter__(config);
            });

            return collection;
        }

        public static IServiceCollection AddBasicServices(this IServiceCollection collection)
        {
            var config = collection.GetConfiguration();
            var env = collection.GetHostingEnvironment();

            if (collection.Where(x => x.ServiceType == typeof(ISerializeProvider)).IsEmpty_())
            {
                AddBasicNoDependencyServices(collection);
            }
            //加密相关
            var data_protect_builder = collection.AddFileBasedDataProtection_(config, env);
            //----------------------------------------------------------------------------------
            //redis-connection
            var redis_server = config.GetRedisConnectionString();
            if (ValidateHelper.IsNotEmpty(redis_server))
            {
                var redis_wrapper = new RedisConnectionWrapper(redis_server);
                collection.AddDisposableSingleInstanceService(redis_wrapper);
                //redis kv存储
                collection.AddScoped<IRedisAll>(provider =>
                {
                    var kv_db = (int)ConfigSet.Redis.KV存储;
                    var serializer = provider.Resolve_<ISerializeProvider>();
                    var helper = new RedisHelper(redis_wrapper.Connection, kv_db, serializer);
                    return helper;
                });
                //使用redis替换内存缓存
                collection.AddRedisCacheProvider_(redis_server);
                //加密私钥
                var db = (int)ConfigSet.Redis.加密KEY;
                var app_name = config.GetAppName();
                data_protect_builder.PersistKeysToStackExchangeRedis(
                    () => redis_wrapper.Connection.SelectDatabase(db),
                    $"data_protection_key:{app_name}");
            }

            return collection;
        }

        /// <summary>
        /// 拦截通用错误，并给约定返回
        /// 这个中间件的注册必须紧跟use mvc之前
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseMsgExceptionHandlerMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionMsgHandlerMiddleware>();
            return app;
        }

        public static MvcOptions AddExceptionHandler(this MvcOptions option)
        {
            //abp在AbpAspNetCoreMvcModule就添加了异常处理的过滤器
            //TODO 这里的优先级要考虑下
            option.Filters.Add<WCloud.Framework.Filters.CommonExceptionFilter>();
            return option;
        }

        public static IMvcBuilder AddJsonProvider_(this IMvcBuilder builder)
        {
            builder.AddNewtonsoftJson(option =>
            {
                option.SerializerSettings.Converters = JsonHelper._setting.Converters;
                option.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                //key大小写
                option.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                //option.UseCamelCasing(true);
            });
            return builder;
        }

        public static IServiceCollection AbpReplaceEmbeddedByPhysical<T>(this IServiceCollection services)
        {
            var app = services.GetHostingEnvironment();
            if (app.IsDevelopment())
            {
                services.Configure<AbpVirtualFileSystemOptions>(options =>
                {
                    var ns = typeof(T).Namespace;

                    var path = new DirectoryInfo(app.ContentRootPath).Parent.FullName;
                    path = Path.Combine(path, ns);
                    File.Exists(path).Should().BeTrue(path);

                    options.FileSets.ReplaceEmbeddedByPhysical<T>(path);
                });
            }

            return services;
        }
    }
}
