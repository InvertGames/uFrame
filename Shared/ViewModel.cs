using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
#if DLL
using Invert.MVVM;
using Invert.uFrame.Editor;
namespace Invert.MVVM
{
#endif
using UniRx;

/// <summary>
///  A data structure that contains information/data needed for a 'View'
/// </summary>
[Serializable]
public abstract class ViewModel
#if !DLL
    :  IUFSerializable, INotifyPropertyChanged , IObservable<IObservableProperty>, IDisposable, IBindable
#else
 : INotifyPropertyChanged
#endif
{
    [Obsolete]
    public bool Dirty { get; set; }
    public event PropertyChangedEventHandler PropertyChanged;
    private Dictionary<int, List<IDisposable>> _bindings;
    private Dictionary<string, IuFrameCommand> _commands;
    private Controller _controller;
    private List<ViewModelPropertyInfo> _modelProperties;
    private string _identifier;

    protected ViewModel()
    {
        Controller = null;
    }


    protected ViewModel(Controller controller, bool initialize = true)
    {
        Controller = controller;
        if (initialize)
        {
            controller.Initialize(this);
        }
    }

    /// <summary>
    ///Access a model property via string.  This is optimized using a compiled delegate to
    ///access derived classes properties so use as needed
    /// </summary>
    /// <param name="bindingPropertyName">The name of the property/field to access</param>
    /// <returns>ModelPropertyBase The Model Property class.  Use value to get the value of the property</returns>
    public ViewModelPropertyInfo this[string bindingPropertyName]
    {
        get
        {
            try
            {
                CacheReflectedModelProperties();
                return _modelProperties.FirstOrDefault(p=>p.Property.PropertyName == bindingPropertyName);
            }
            catch (Exception ex)
            {
                throw new Exception(bindingPropertyName + " was not found on " + this.ToString() + ": " + ex.Message);
            }
        }
    }

    public Dictionary<int, List<IDisposable>> Bindings
    {
        get { return _bindings ?? (_bindings = new Dictionary<int, List<IDisposable>>()); }
        set { _bindings = value; }
    }

    [Obsolete("Use GetViewModelCommands")]
    public Dictionary<string, IuFrameCommand> Commands
    {
        get
        {
            foreach (var key in Bindings.Keys)
            {
                if (key < 0) continue;
                
            }
            if (_commands == null)
            {
                var dictionary = new Dictionary<string, IuFrameCommand>();
                foreach (KeyValuePair<string, PropertyInfo> command in GetReflectedCommands(this.GetType()))
                    dictionary.Add(command.Key, (IuFrameCommand)command.Value.GetValue(this, null));
                _commands = dictionary;
            }
            return _commands;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public Controller Controller
    {
        get { return _controller; }
        set
        {
            
            if (value != null && _controller == value)
                return;

            if (value != null)
            {
                WireCommands(value);
            }
            _controller = value;
            if (!_isBound)
            {
                Bind();
                _isBound = true;
            }
           
        }
    }

    private bool _isBound;
    public virtual void Bind()
    {
        
    }

    public virtual string Identifier
    {
        get { return _identifier; }
        set { _identifier = value; }
    }

    public List<ViewModelPropertyInfo> Properties
    {
        get
        {
            CacheReflectedModelProperties();
            return _modelProperties;
        }
    }

    public int References { get; set; }

    /// <summary>
    /// Grabs all the commands available for a viewmodel type
    /// </summary>
    /// <param name="modelType"></param>
    /// <returns></returns>
    [Obsolete("Use GetViewModelCommands")]
    public static Dictionary<string, PropertyInfo> GetReflectedCommands(Type modelType)
    {
        var modelProperties = new Dictionary<string, PropertyInfo>();
        var fields = modelType.GetProperties();
        foreach (var field in fields)
        {
            if (typeof(IuFrameCommand).IsAssignableFrom(field.PropertyType))
            {
                modelProperties.Add(field.Name, field);
            }
        }
        return modelProperties;
    }

    /// <summary>
    /// Grab the bindable properties for the view-model
    /// </summary>
    /// <param name="modelType"></param>
    /// <returns></returns>
    [Obsolete("Use GetViewModelProperties")]
    public static Dictionary<string, FieldInfo> GetReflectedModelProperties(Type modelType)
    {
        var modelProperties = new Dictionary<string, FieldInfo>();
        var fields = modelType.GetFields();
        foreach (var field in fields.Where(p => p.IsPublic && p.IsInitOnly))
        {
            modelProperties.Add(field.Name, field);
        }
        return modelProperties;
    }


    public IDisposable AddBinding(IDisposable binding)
    {
        if (!Bindings.ContainsKey(-1))
        {
            Bindings[-1] = new List<IDisposable>();
        }
        Bindings[-1].Add(binding);
        return binding;
    }

    /// <summary>
    /// Reflection-less get of all view-model commands generated by the designer tool.
    /// </summary>
    /// <returns></returns>
    public List<ViewModelCommandInfo> GetViewModelCommands()
    {
        var list = new List<ViewModelCommandInfo>();
        FillCommands(list);
        return list;
    }

    /// <summary>
    /// Reflection-less get of all view-model commands generated by the designer tool.
    /// </summary>
    /// <returns></returns>
    public List<ViewModelPropertyInfo> GetViewModelProperties()
    {
        var list = new List<ViewModelPropertyInfo>();
        FillProperties(list);
        return list;
    }

    /// <summary>
    /// Implementation of Microsoft's INotifyPropertyChanged
    /// </summary>
    /// <param name="propertyName"></param>
    public virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChangedEventHandler handler = PropertyChanged;
        if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Implementation of Microsoft's INotifyPropertyChanged
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="propertyName"></param>
    public virtual void OnPropertyChanged(object sender, string propertyName)
    {
        PropertyChangedEventHandler handler = PropertyChanged;
        if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }

#if !DLL
    public virtual void Read(ISerializerStream stream)
    {
        Identifier = stream.DeserializeString("Identifier");
        var controllerName = stream.DeserializeString("Controller");
        if (controllerName != null)
        {
            var controller = stream.DependencyContainer.Resolve(stream.TypeResolver.GetType(controllerName)) as Controller;
            Controller = controller;
        }
  
    }
    public virtual void Write(ISerializerStream stream)
    {
        stream.SerializeString("Identifier", Identifier);
        if (Controller != null)
            stream.SerializeString("Controller", Controller.GetType().AssemblyQualifiedName);
    }

    [Obsolete]
    protected IuFrameCommand Command(Action command)
    {
        return new Command(command);
    }

    [Obsolete]
    protected IuFrameCommand Command(Func<IEnumerator> command)
    {
        return new YieldCommand(command);
    }

#endif

    public IDisposable Subscribe(IObserver<IObservableProperty> observer)
    {
        PropertyChangedEventHandler propertyChanged = (sender, args) =>
        {
            var property = sender as IObservableProperty;
            //if (property != null)
                observer.OnNext(property);
        };

        PropertyChanged += propertyChanged;
        return new SimpleDisposable(() => PropertyChanged -= propertyChanged);
    }

    public override string ToString()
    {
        // TODO
        return base.ToString();
    }

    public void Dispose()
    {
        Unbind();
    }

    public virtual void Unbind()
    {
        foreach (var binding in Bindings)
        {
            foreach (var binding1 in binding.Value)
            {
                binding1.Dispose();
            }
            binding.Value.Clear();
        }
        Bindings.Clear();
    }


    protected virtual void FillCommands(List<ViewModelCommandInfo> list)
    {
    }

    protected virtual void FillProperties(List<ViewModelPropertyInfo> list)
    {
    }

    protected virtual void WireCommands(Controller controller)
    {
    }

    
    private void CacheReflectedModelProperties()
    {
        if (_modelProperties != null) return;
        _modelProperties = GetViewModelProperties();
    }
}

public class ViewModelCommandInfo
{
    public IuFrameCommand Command { get; set; }

    public string Name { get; set; }

    public Type ParameterType { get; set; }

    public ViewModelCommandInfo(string name, IuFrameCommand command)
    {
        Name = name;
        Command = command;
    }

    public ViewModelCommandInfo(Type parameterType, string name, IuFrameCommand command)
    {
        ParameterType = parameterType;
        Name = name;
        Command = command;
    }
}

public class ViewModelPropertyInfo
{
    public bool IsComputed { get; set; }
    public bool IsCollectionProperty { get; set; }

    public bool IsElementProperty { get; set; }

    public bool IsEnum { get; set; }

    public IObservableProperty Property { get; set; }

    public ViewModelPropertyInfo(IObservableProperty property, bool isElementProperty, bool isCollectionProperty, bool isEnum, bool isComputed = false)
    {
        Property = property;
        IsElementProperty = isElementProperty;
        IsCollectionProperty = isCollectionProperty;
        IsEnum = isEnum;
        IsComputed = isComputed;
    }
}

#if DLL
}
#endif