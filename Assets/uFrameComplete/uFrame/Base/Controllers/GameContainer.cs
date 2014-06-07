using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

#if DLL

namespace Invert.uFrame
{
#endif

    /// <summary>
    /// A ViewModel Container and a factory for Controllers and commands.
    /// </summary>
#if DLL
    public class uFrameContainer : IUFrameContainer
#else
public class GameContainer : IGameContainer
#endif

    {
        private TypeInstanceCollection _instances;
        private TypeMappingCollection _mappings;


        public TypeMappingCollection Mappings
        {
            get { return _mappings ?? (_mappings = new TypeMappingCollection()); }
            set { _mappings = value; }
        }

        protected TypeInstanceCollection Instances
        {
            get { return _instances ?? (_instances = new TypeInstanceCollection()); }
            set { _instances = value; }
        }

        public Dictionary<Type, Dictionary<Type, Type>> AdapterMappings
        {
            get { return _adapterMappings; }
            set { _adapterMappings = value; }
        }



        public IEnumerable<TType> ResolveAll<TType>()
        {
            foreach (var obj in ResolveAll(typeof(TType)))
            {
                yield return (TType) obj;
            }
        }

        /// <summary>
        /// Resolves all instances of TType or subclasses of TType.  Either named or not.
        /// </summary>
        /// <typeparam name="TType">The Type to resolve</typeparam>
        /// <returns>List of objects.</returns>
       public IEnumerable<object> ResolveAll(Type type)
        {
            foreach (var instance1 in Instances.Where(p => p.Base == type && !string.IsNullOrEmpty(p.Name)))
            {
                yield return instance1.Instance;
            }

            foreach (var mapping in Mappings)
            {
                if (!string.IsNullOrEmpty(mapping.Name))
                {
                    if (type.IsAssignableFrom(mapping.From))
                    {
                        var item = Activator.CreateInstance(mapping.To);
                        Inject(item);
                        yield return item;
                    }
                }
            }
        }
        /// <summary>
        /// Clears all type-mappings and instances.
        /// </summary>
        public void Clear()
        {
            Instances.Clear();
            Mappings.Clear();
            AdapterMappings.Clear();
        }

        /// <summary>
        /// Injects registered types/mappings into an object
        /// </summary>
        /// <param name="obj"></param>
        public void Inject(object obj)
        {
            if (obj == null) return;

            var members = obj.GetType().GetMembers();
            foreach (var memberInfo in members)
            {
                var injectAttribute =
                    memberInfo.GetCustomAttributes(typeof(InjectAttribute), true).FirstOrDefault() as InjectAttribute;
                if (injectAttribute != null)
                {
                    if (memberInfo is PropertyInfo)
                    {
                        var propertyInfo = memberInfo as PropertyInfo;
                        propertyInfo.SetValue(obj, Resolve(propertyInfo.PropertyType, injectAttribute.Name), null);
                    }
                    else if (memberInfo is FieldInfo)
                    {
                        var fieldInfo = memberInfo as FieldInfo;
                        fieldInfo.SetValue(obj, Resolve(fieldInfo.FieldType, injectAttribute.Name));
                    }
                }
            }
        }

        /// <summary>
        /// Register a type mapping
        /// </summary>
        /// <typeparam name="TSource">The base type.</typeparam>
        /// <typeparam name="TTarget">The concrete type</typeparam>
        public void Register<TSource, TTarget>(string name = null)
        {
            Mappings[typeof(TSource), name] = typeof(TTarget);
        }

        public void Register(Type source, Type target, string name = null)
        {
            Mappings[source, name] = target;
        }

        /// <summary>
        /// Register a named instance
        /// </summary>
        /// <param name="baseType">The type to register the instance for.</param>        
        /// <param name="instance">The instance that will be resolved be the name</param>
        /// <param name="injectNow">Perform the injection immediately</param>
        public void RegisterInstance(Type baseType, object instance = null, bool injectNow = true)
        {
            RegisterInstance(baseType, instance, null, injectNow);
        }

        /// <summary>
        /// Register a named instance
        /// </summary>
        /// <param name="baseType">The type to register the instance for.</param>
        /// <param name="name">The name for the instance to be resolved.</param>
        /// <param name="instance">The instance that will be resolved be the name</param>
        /// <param name="injectNow">Perform the injection immediately</param>
        public void RegisterInstance(Type baseType, object instance = null, string name = null, bool injectNow = true)
        {


            Instances[baseType, name] = instance;
            if (injectNow)
            {
                Inject(instance);
            }
        }

        public void RegisterInstance<TBase>(TBase instance) where TBase : class
        {
            RegisterInstance<TBase>(instance, true);
        }

        public void RegisterInstance<TBase>(TBase instance, bool injectNow) where TBase : class
        {
            RegisterInstance<TBase>(instance, null, injectNow);
        }

        public void RegisterInstance<TBase>(TBase instance, string name, bool injectNow = true) where TBase : class
        {
            RegisterInstance(typeof(TBase), instance, name, injectNow);
        }

        /// <summary>
        ///  If an instance of T exist then it will return that instance otherwise it will create a new one based off mappings.
        /// </summary>
        /// <typeparam name="T">The type of instance to resolve</typeparam>
        /// <returns>The/An instance of 'instanceType'</returns>
        public T Resolve<T>(string name = null, bool requireInstance = false) where T : class
        {
            return (T)Resolve(typeof(T), name, requireInstance);
        }

        /// <summary>
        /// If an instance of instanceType exist then it will return that instance otherwise it will create a new one based off mappings.
        /// </summary>
        /// <param name="baseType">The type of instance to resolve</param>
        /// <param name="name">The type of instance to resolve</param>
        /// <param name="requireInstance">If true will return null if an instance isn't registered.</param>
        /// <returns>The/An instance of 'instanceType'</returns>
        public object Resolve(Type baseType, string name = null, bool requireInstance = false)
        {
            // Look for an instance first
            var item = Instances[baseType, name];
            if (item != null)
            {
                return item;
            }
            if (requireInstance)
                return null;
            // Check if there is a mapping of the type
            var namedMapping = Mappings[baseType, name];
            if (namedMapping != null)
            {
                var obj = Activator.CreateInstance(namedMapping);
                Inject(obj);
                return obj;
            }
            return null;
        }

        public void InjectAll()
        {
            foreach (var instance in Instances)
            {
                Inject(instance.Instance);
            }
            foreach (var namedInstance in Instances)
            {
                Inject(namedInstance.Instance);
            }
        }

        private Dictionary<Type, Dictionary<Type, Type>> _adapterMappings = new Dictionary<Type, Dictionary<Type, Type>>();
        public void RegisterRelation<TFor, TBase, TConcrete>()
        {
            if (AdapterMappings.ContainsKey(typeof(TFor)))
            {
                var dictionary = AdapterMappings[typeof(TFor)] ?? (AdapterMappings[typeof(TFor)] = new Dictionary<Type, Type>());
                if (dictionary.ContainsKey(typeof(TBase)))
                {
                    dictionary[typeof(TBase)] = typeof(TConcrete);
                }
                else
                {
                    dictionary.Add(typeof(TBase), typeof(TConcrete));
                }
            }
            else
            {
                AdapterMappings.Add(typeof(TFor), new Dictionary<Type, Type>());
                AdapterMappings[typeof(TFor)].Add(typeof(TBase), typeof(TConcrete));
            }
        }

        public TBase ResolveRelation<TBase>(Type tfor)
        {

            if (!AdapterMappings.ContainsKey(tfor)) return default(TBase);
            var dictionary = AdapterMappings[tfor];
            if (dictionary == null || !dictionary.ContainsKey(typeof(TBase))) return default(TBase);

            var result = (TBase)Activator.CreateInstance(dictionary[typeof(TBase)]);
            Inject(result);
            return result;
        }
        public TBase ResolveRelation<TFor, TBase>()
        {

            if (!AdapterMappings.ContainsKey(typeof(TFor)))
                return default(TBase);
            var dictionary = AdapterMappings[typeof(TFor)];
            if (dictionary == null || !dictionary.ContainsKey(typeof(TBase))) return default(TBase);

            var result = (TBase)Activator.CreateInstance(dictionary[typeof(TBase)]);
            Inject(result);
            return result;
        }
    }
    public class TypeMapping
    {
        public Type From
        {
            get;
            set;
        }

        public Type To
        {
            get;
            set;
        }
        public string Name { get; set; }
    }
    public class TypeMappingCollection : List<TypeMapping>
    {
        public Type this[Type from, string name = null]
        {
            get
            {
                var mapping = this.FirstOrDefault(p => p.From == from && p.Name == name);
                if (mapping != null)
                {
                    return mapping.To;
                }
                return null;
            }
            set
            {
                var mapping = this.FirstOrDefault(p => p.From == from && p.Name == name);
                if (mapping == null)
                {
                    Add(new TypeMapping() { From = from, Name = name, To = value });
                }
                else
                {
                    mapping.To = value;
                    mapping.Name = name;
                }
            }
        }
    }
    public class TypeInstanceCollection : List<RegisteredInstance>
    {

        public object this[Type from, string name = null]
        {
            get
            {
                var mapping = this.FirstOrDefault(p => p.Base == from && p.Name == name);
                if (mapping != null)
                {
                    return mapping.Instance;
                }
                return null;
            }
            set
            {
                var mapping = this.FirstOrDefault(p => p.Base == from && p.Name == name);
                if (mapping == null)
                {
                    Add(new RegisteredInstance() { Base = from, Name = name, Instance = value });
                }
                else
                {
                    mapping.Instance = value;
                    mapping.Name = name;
                }
            }
        }
    }

    public class RegisteredInstance
    {
        public Type Base
        {
            get;
            set;
        }

        public object Instance
        {
            get;
            set;
        }

        public string Name { get; set; }
    }
#if DLL
}

#endif