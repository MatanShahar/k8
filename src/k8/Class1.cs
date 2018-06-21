using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace K8
{
    public interface IBusDescriptor
    {
        int Width { get; }
    }

    public interface IPowerRailDescriptor 
    {
        
    }

    public interface IPowerRailDrain
    {

    }

    public interface IPowerRailSource
    {

    }

    public interface IDataLineIn
    {
        bool Read();
    }

    public interface IDataLineOut
    {
        void Write(bool value);
    }

    public interface ITerminal
    {
        BusMask Mask { get; }
    }

    public interface IInputTerminal
    {
        BitArray Read();
    }

    public interface ISignal
    {
        bool Read();
    }

    public interface IOutputTerminal
    {
        void Write(BitArray data);
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class HostAttribute : Attribute
    {
        private readonly string name;

        public HostAttribute(string name)
        {
            this.name = name;
        }

        public string Name
        {
            get { return this.name; }
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class BindAttribute : Attribute
    {
        private readonly string name;

        public BindAttribute(string name)
        {
            this.name = name;
        }

        public string Name
        {
            get { return this.name; }
        }

        public string Scope { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class LatchedAttribute : Attribute
    {
        public string ResetSignal { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class SelectAttribute : Attribute
    {
        private readonly int offset;
        private readonly int count;

        public SelectAttribute(int offset, int count)
        {
            this.offset = offset;
            this.count = count;
        }

        public int Offset
        {
            get { return this.offset; }
        }

        public int Count
        {
            get { return this.count; }
        }
    }

    public interface ITransaction
    {
        int Id { get; }
    }

    public interface IUpdateHandler
    {
        TransactionAction Update(ITransaction transaction);
    }

    public enum TransactionAction
    {
        Commit,
        Rollback,
        Delay
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class SignalAttribute : Attribute
    {
        private readonly string name;

        public SignalAttribute(string name)
        {
            this.name = name;
        }

        public string Name
        {
            get { return this.name; }
        }

        public int SelectBit { get; set; }
    }

    public struct BusMask
    {
        private readonly int offset;
        private readonly int count;

        public BusMask(int offset, int count)
        {
            this.offset = offset;
            this.count = count;
        }

        public int Offset
        {
            get { return this.offset; }
        }

        public int Count 
        {
            get { return this.count; }
        }
    }

    public interface ISystemLoader
    {
        Type GetRootComponent();
    }

    public interface IConfigurable
    {
        void Configure(K8Runtime runtime);
    }

    public interface ISelfChecked
    {
        void SelfCheck(K8Runtime runtime);
    }

    public struct SelfCheckResult
    {
        private readonly string[] errors;

        public SelfCheckResult(params string[] errors)
        {
            this.errors = errors;
        }

        public bool Success
        {
            get { return this.errors == null || this.errors.Length == 0; }
        }

        public bool HasErrors
        {
            get { return this.errors != null && this.errors.Length > 0; }
        }

        public string[] Errors
        {
            get { return this.errors ?? new string[0]; }
        }
    }

    public interface IComponentLinker
    {
        void Link(ITerminal source, ITerminal target);
    }

    public enum LinkType
    {
        Bridge,
        Mirror,
        Copy
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class InterconnectAttribute : Attribute
    {
        private readonly string name;

        public InterconnectAttribute(string name)
        {
            this.name = name;
        }

        public string Name
        {
            get { return this.name; }
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class AccessAttribute : Attribute
    {
        private readonly ElementAccess access;

        public AccessAttribute(ElementAccess access)
        {
            this.access = access;
        }

        public ElementAccess Access
        {
            get { return this.access; }
        }
    }

    public enum ElementAccess
    {
        Internal,
        Private,
        External
    }
}
