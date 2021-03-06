﻿using Autofac;
using Lib.core;
using Lib.helper;
using System;

namespace WCloud.Framework.Aop.AutofacProvider
{
    /// <summary>
    /// IOC容器
    /// https://autofac.org/
    /// http://autofac.readthedocs.io/en/latest/getting-started/index.html
    /// </summary>
    public class AutofacIocContext : IDisposable
    {
        /// <summary>
        /// 默认实例
        /// </summary>
        public static readonly AutofacIocContext Instance = new AutofacIocContext();

        private IContainer _root;

        /// <summary>
        /// 创建之前调用
        /// </summary>
        public event RefAction<ContainerBuilder> OnContainerBuilding;

        public AutofacIocContext()
        {
            //
        }

        /// <summary>
        /// 销毁容器
        /// </summary>
        public void Dispose()
        {
            this.Container?.Dispose();
        }

        /// <summary>
        /// 获取ioc容器，第一次访问将创建容器
        /// </summary>
        /// <returns></returns>
        public IContainer Container => this._root ?? throw new Exception("顶级容器未生成");

        /// <summary>
        /// 是否在容器中注册
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool IsRegistered<T>() => this.Container.IsRegistered<T>();

        /// <summary>
        /// 是否在容器中注册
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsRegisteredWithName<T>(string name) => this.Container.IsRegisteredWithName<T>(name);

        /// <summary>
        /// 是否在容器中注册
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsRegistered<T>(string name)
        {
            if (ValidateHelper.IsNotEmpty(name))
            {
                return IsRegisteredWithName<T>(name);
            }
            else
            {
                return IsRegistered<T>();
            }
        }

        /// <summary>
        /// 创建一个作用域
        /// </summary>
        /// <returns></returns>
        public ILifetimeScope Scope() => this.Container.BeginLifetimeScope();
    }

    /*
     1、InstancePerDependency

对每一个依赖或每一次调用创建一个新的唯一的实例。这也是默认的创建实例的方式。

官方文档解释：Configure the component so that every dependent component or call to Resolve() gets a new, unique instance (default.)

2、InstancePerLifetimeScope

在一个生命周期域中，每一个依赖或调用创建一个单一的共享的实例，且每一个不同的生命周期域，实例是唯一的，不共享的。

官方文档解释：Configure the component so that every dependent component or call to Resolve() within a single ILifetimeScope gets the same, shared instance. Dependent components in different lifetime scopes will get different instances.

3、InstancePerMatchingLifetimeScope

在一个做标识的生命周期域中，每一个依赖或调用创建一个单一的共享的实例。打了标识了的生命周期域中的子标识域中可以共享父级域中的实例。若在整个继承层次中没有找到打标识的生命周期域，则会抛出异常：DependencyResolutionException。

官方文档解释：Configure the component so that every dependent component or call to Resolve() within a ILifetimeScope tagged with any of the provided tags value gets the same, shared instance. Dependent components in lifetime scopes that are children of the tagged scope will share the parent's instance. If no appropriately tagged scope can be found in the hierarchy an DependencyResolutionException is thrown.

4、InstancePerOwned

在一个生命周期域中所拥有的实例创建的生命周期中，每一个依赖组件或调用Resolve()方法创建一个单一的共享的实例，并且子生命周期域共享父生命周期域中的实例。若在继承层级中没有发现合适的拥有子实例的生命周期域，则抛出异常：DependencyResolutionException。

官方文档解释：

Configure the component so that every dependent component or call to Resolve() within a ILifetimeScope created by an owned instance gets the same, shared instance. Dependent components in lifetime scopes that are children of the owned instance scope will share the parent's instance. If no appropriate owned instance scope can be found in the hierarchy an DependencyResolutionException is thrown.

5、SingleInstance

每一次依赖组件或调用Resolve()方法都会得到一个相同的共享的实例。其实就是单例模式。

官方文档解释：Configure the component so that every dependent component or call to Resolve() gets the same, shared instance.

6、InstancePerHttpRequest

在一次Http请求上下文中,共享一个组件实例。仅适用于asp.net mvc开发。
     */
}
