﻿using Lib.extension;
using Microsoft.Extensions.Logging;
using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lib.zookeeper.ServiceManager
{
    /// <summary>
    /// 应该作为静态类
    /// </summary>
    public class ServiceRegister : ServiceManageBase
    {
        private readonly Func<List<ContractModel>> _contracts;

        public ServiceRegister(ILogger logger, string host, Func<List<ContractModel>> _contracts) : base(logger, host)
        {
            this._contracts = _contracts ?? throw new ArgumentNullException(nameof(_contracts));

            //链接成功后调用注册
            this.OnConnectedAsync += this.Reg;
            //尝试打开链接
            this.CreateClient();
        }

        public async Task Reg()
        {
            try
            {
                await this.RetryAsync(async () => await this.RegisterService());
            }
            catch (Exception e)
            {
                var err = new Exception("注册服务失败", e);
                this.logger.AddErrorLog(err.Message,err);
            }
        }

        private async Task RegisterService()
        {
            var _node_id = this.Client.getSessionId().ToString();

            var list = this._contracts.Invoke().Select(x => new AddressModel()
            {
                Url = x.Url,
                ServiceNodeName = ServiceManageHelper.ParseServiceName(x.Contract),
                EndpointNodeName = ServiceManageHelper.EndpointNodeName(_node_id),
            }).ToList();

            var now = DateTime.Now;
            foreach (var m in list)
            {
                m.UpdateTime = now;

                var service_path = this._base_path + "/" + m.ServiceNodeName;
                await this.Client.EnsurePersistentPath(service_path);

                var path = service_path + "/" + m.EndpointNodeName;
                var data = this._serializer.Serialize(m);
                if (await this.Client.ExistAsync_(path))
                    //如果节点存在，就刷新节点数据
                    await this.Client.setDataAsync(path, data);
                else
                    //创建临时节点，服务端下线自动删除
                    await this.Client.CreateNode_(path, CreateMode.EPHEMERAL, data);
            }
        }
    }
}
