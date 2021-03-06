﻿using System;

namespace Lib.core
{
    /// <summary>
    /// 不是异常
    /// </summary>
    internal class NotException : Exception { }

    [Serializable]
    public class BaseException : Exception
    {
        public BaseException(string msg) : base(msg) { }
        public BaseException(string msg, Exception inner) : base(msg, inner) { }
        public BaseException(string msg, string code, Exception inner = null) : base(msg, inner ?? new NotException()) { }
    }

    [Serializable]
    public class MsgException : Exception
    {
        public virtual string ErrorCode { get; set; }

        public MsgException(string msg) : base(msg) { }
    }

    [Serializable]
    public class ConfigExcetpion : Exception
    {
        public ConfigExcetpion(string msg) : base(msg) { }
    }

    [Serializable]
    public class InstanceDeadException : Exception
    {
        public InstanceDeadException(string msg) : base(msg) { }
    }

    [Serializable]
    public class NotExistException : Exception
    {
        public NotExistException(string msg) : base(msg) { }
    }

    [Serializable]
    public class DataNotExistException : Exception
    {
        public DataNotExistException(string msg) : base(msg) { }
    }

    [Serializable]
    public class ConfigNotExistException : Exception
    {
        public ConfigNotExistException(string msg) : base(msg) { }
    }

    [Serializable]
    public class SourceNotExistException : Exception
    {
        public SourceNotExistException(string msg) : base(msg) { }
    }

    [Serializable]
    public class AccessDenyException : Exception
    {
        public AccessDenyException(string msg) : base(msg) { }
    }

    [Serializable]
    public class DeleteNodeException : Exception
    {
        public DeleteNodeException(string msg) : base(msg) { }
    }

    [Serializable]
    public class NodeNotExistException : Exception
    {
        public NodeNotExistException(string msg) : base(msg) { }
    }

    [Serializable]
    public class NodeTooMuchException : Exception
    {
        public NodeTooMuchException(string msg) : base(msg) { }
    }

    [Serializable]
    public class ExceptionLogException : Exception
    {
        public ExceptionLogException(string msg) : base(msg) { }
    }

    [Serializable]
    public class NoPrimaryKeyException : Exception
    {
        public NoPrimaryKeyException(string msg) : base(msg) { }
    }

    [Serializable]
    public class NoValidFieldsException : Exception
    {
        public NoValidFieldsException(string msg) : base(msg) { }
    }
}
