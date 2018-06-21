using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace K8
{   
    public class ComponentInfo
    {

    }

    public class ComponentRegistry
    {
        internal void Register(Type componentType, ComponentInfo info)
        {

        }
    }

    public class K8Runtime
    {
        private readonly ComponentRegistry componentRegistry;

        public K8Runtime()
        {
            this.componentRegistry = new ComponentRegistry();
        }

        public ComponentRegistry ComponentRegistry
        {
            get { return this.componentRegistry; }
        }
    }

    internal class ComponentLoader
    {
        private readonly K8Runtime runtime;

        public ComponentHandler LoadComponent(Type componentType)
        {
            return new Session(this, componentType).Load();
        }

        private class Session
        {
            private readonly ComponentLoader loader;
            private readonly Type componentType;

            private ComponentAttribute componentAttribute;
            private ComponentInfo componentInfo;

            public Session(ComponentLoader loader, Type componentType)
            {
                this.loader = loader;

                if (!typeof(IK8Component).IsAssignableFrom(componentType))
                    throw new InvalidOperationException("An object must implement IK8Component to a valid component");

                this.componentType = componentType;
            }

            public ComponentHandler Load()
            {
                this.componentAttribute = this.GetComponentAttribute();
                this.componentInfo = this.ExtractComponentInfo();
                var componentFactory = this.GetComponentFactory();
                var componentInstance = componentFactory.Create(this.loader.runtime);

                this.ActivateComponent(componentInstance);
            }

            private void ActivateComponent(IK8Component component)
            {

            }

            private ComponentAttribute GetComponentAttribute()
            {
                var attr = this.componentType.GetCustomAttribute<ComponentAttribute>();
                if (attr == null)
                    throw new InvalidOperationException("A component my have a component attribute to be loaded!");

                return attr;
            }

            private ComponentInfo ExtractComponentInfo()
            {
                var componentName = this.componentAttribute.ComponentName != null
                    ? this.componentAttribute.ComponentName
                    : this.GenerateComponentName();

                var displayName = componentName;
                var displayNameAttr = this.componentType.GetCustomAttribute<DisplayNameAttribute>();
                if (displayNameAttr != null)
                    displayName = displayNameAttr.DisplayName;

                var description = this.componentType.GetCustomAttribute<DescriptionAttribute>()?.Description;
                var componentId = this.loader.runtime.ComponentRegistry.Define(componentName);

                return new ComponentInfo(componentId, componentName) {
                    DisplayName = displayName,
                    Description = description
                };
            }

            private IComponentFactory GetComponentFactory()
            {
                var factoryType = this.GetFactoryType();
                if (factoryType != null)
                    return this.ActivateFactory(factoryType);

                var defaultConstructor = this.componentType.GetConstructor(Type.EmptyTypes);
                if (defaultConstructor != null)
                    return new DefaultCtorComponentFactory(defaultConstructor);

                throw new InvalidOperationException("Component is not constructable and no factory was found");
            }

            private Type GetFactoryType()
            {
                if (this.componentAttribute.FactoryType != null)
                    return this.componentAttribute.FactoryType;
                
                var subTypes = this.componentType.GetNestedTypes();
                var innerFactory = subTypes.FirstOrDefault(type => 
                    (type.Name.Equals("Factory", StringComparison.InvariantCultureIgnoreCase) ||
                    type.Name.Equals("ComponentFactory", StringComparison.InvariantCultureIgnoreCase)) &&
                    typeof(IComponentFactory).IsAssignableFrom(type));

                if (innerFactory != null) 
                    return innerFactory;

                if (typeof(IComponentFactory).IsAssignableFrom(this.componentType))
                    return this.componentType;

                return null;
            }

            private IComponentFactory ActivateFactory(Type factoryType)
            {
                if (!typeof(IComponentFactory).IsAssignableFrom(factoryType))
                    throw new InvalidOperationException("Invalid component factory type");

                var factoryConstructor = factoryType.GetConstructor(Type.EmptyTypes);
                if (factoryConstructor == null)
                    throw new InvalidOperationException("Component factory in not constructable");

                return (IComponentFactory)factoryConstructor.Invoke(new object[0]);
            }

            private string GenerateComponentName()
            {
                var typeName = this.componentType.Name;
                var componentGuid = this.componentType.GUID;

                return $"{typeName}::{componentGuid}";
            }
        }
    }

    internal class DefaultCtorComponentFactory : IComponentFactory
    {
        private readonly ConstructorInfo defaultConstructor;

        public DefaultCtorComponentFactory(ConstructorInfo defaultConstructor) 
        {
            this.defaultConstructor = defaultConstructor;
        }

        public IK8Component Create(K8Runtime runtime)
        {
            return (IK8Component)this.defaultConstructor.Invoke(new object[0]);
        }
    }

    internal class ComponentHandler
    {
        private readonly IK8Component component;

        public ComponentHandler(IK8Component component)
        {
            this.component = component;
        }
    }

    public class ComponentHandle
    {
        private readonly object component;
        private readonly string name;

        public ComponentHandle(object component, string name)
        {
            this.component = component;
            this.name = name;
        }

        public object Component
        {
            get { return this.component; }
        }

        public string Name
        {
            get { return this.name; }
        }
        
        public string DisplayName { get; set; }
        public string Description { get; set; }
    }

    public class SystemGraph
    {
        

        public class Node
        {
            private readonly Type interconnect;
            private readonly
        }
    }

    public class ComponentGraph
    {
        private readonly Node rootComponent;

        public class Node
        {
            private readonly ComponentHandle component;

            private readonly Node parent;
            private readonly Node root;

            private readonly ICollection<Node> children;

            private readonly Lazy<string> path;

            public Node(ComponentHandle component, Node parent)
            {
                this.parent = parent;
                if (parent != null) 
                    this.root = parent.Root;

                this.children = new List<Node>();
                this.path = new Lazy<string>(this.BuildPath);
            }

            public Node AddChild(ComponentHandle component)
            {
                var childNode = new Node(component, this);
                this.children.Add(childNode);

                return childNode;
            }

            private string BuildPath()
            {
                var basePath = this.IsRoot ? "~" : this.parent.Path;
                var totalLength = basePath.Length + this.component.Name.Length + 1;
                var resultBuilder = new StringBuilder(basePath, totalLength);
                resultBuilder.Append('/');
                resultBuilder.Append(this.component.Name);

                return resultBuilder.ToString();
            }

            public T GetComponent<T>()
            {
                return (T)this.component.Component;
            }

            public ComponentHandle Component
            {
                get { return this.component; }
            }

            public Node Parent
            {
                get { return this.parent; }
            }

            public Node Root
            {
                get { return this.root ?? this; }
            }

            public bool IsRoot
            {
                get { return this.parent == null; }
            }

            public string Path
            {
                get { return this.path.Value; }
            }
        }
    }
}