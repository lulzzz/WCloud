﻿namespace WCloud.CommonService.Application.Infrastructure
{
    public enum QueueStatusEnum : int
    {
        未开始 = 0,
        正在运行 = 1,
        处理成功 = 2,
        处理失败 = 3
    }
}
