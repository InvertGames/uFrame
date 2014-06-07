using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Lifetime;
using UnityEngine;

/// <summary>
/// The base class for a View that binds to a ViewModel
/// </summary>
public abstract class ViewBase : ViewContainer
{
    public List<IBindingProvider> BindingProviders
    {
        get { return _bindingProviders ?? (_bindingProviders = new List<IBindingProvider>()); }
        set { _bindingProviders = value; }
    }

    /// <summary>
    /// The View Event delegate that takes a string for the event name.
    /// </summary>
    /// <param name="eventName">The event that has occured.</param>
    public delegate void ViewEvent(string eventName);

    /// <summary>
    /// An event that is invoked whe calling Event("MyEvent")
    /// </summary>
    public event ViewEvent EventTriggered;

    /// <summary>
    /// Should we log an event for each View event that occurs.
    /// </summary>
    [HideInInspector]
    public bool _LogEvents;

    //[HideInInspector]
    //public string _ViewModelControllerMethod;

    //[HideInInspector]
    //public string _ViewModelControllerType;

    ///// <summary>
    ///// Where should the viewmodel come from or how should it be instantiated.
    ///// </summary>
    //[HideInInspector]
    //public ViewModelRegistryType _ViewModelFrom = ViewModelRegistryType.ResolveInstance;

    private bool _bound = false;

    private List<ViewBase> _children;

    [HideInInspector]
    private ViewModel _Model;

    private ViewBase _parentView;
    private List<IBindingProvider> _bindingProviders;

    public IEnumerable<ViewModel> ChildViewModels
    {
        get
        {
            return ChildViews.Select(p => p.ViewModelObject);
        }
    }

    public List<ViewBase> ChildViews
    {
        get { return _children ?? (_children = new List<ViewBase>()); }
        set { _children = value; }
    }

    public bool Instantiated { get; set; }

    public ViewBase ParentView
    {
        get
        {
            if (_parentView == null)
            {
                if (transform == null) return null;
                if (transform.parent == null) return null;
                _parentView = transform.parent.GetView();
            }
            return _parentView;
        }
        set
        {
            _parentView = value;
        }
    }

    public ViewModel ParentViewModel
    {
        get
        {
            var pv = ParentView;
            if (pv == null) return null;
            return pv.ViewModelObject;
        }
    }

    public virtual ViewModel ViewModelObject
    {
        get
        {
            return _Model ?? (_Model = CreateModel());
        }
        set
        {
            // Skip if its that same
            if (_Model == value) return;

            if (value == null) return;

            _Model = value;
            // Should we rebind?
            if (_bound)
            {
                Unbind();
                SetupBindings();
            }
            //Bind();
        }
    }

    public abstract Type ViewModelType { get; }

    /// <summary>
    /// The name of the prefab that created this view
    /// </summary>
    public string ViewName { get; set; }

    public override void AddBinding(IBinding binding)
    {
        base.AddBinding(binding);
        // If this view has already been binded invoke the bind method
        if (_bound)
        {
            binding.Bind();
        }
    }

    public virtual void Awake()
    {
        if (GameManager.ActiveSceneManager != null)
            GameManager.ActiveSceneManager.RegisterRootView(this);
    }

    public abstract void Bind();



    protected ViewModel RequestViewModel(Controller controller)
    {
        if (ForceResolveViewModel || !IsMultiInstance)
        {
            if (string.IsNullOrEmpty(_resolveName))
            {
                if (OverrideViewModel)
                    return controller.GetByType(ViewModelType, true, InitializeViewModel);
                
                return controller.GetByType(ViewModelType, false);
            }

            if (OverrideViewModel)
                return controller.GetByName(_resolveName, true, InitializeViewModel);
                
            return controller.GetByName(_resolveName, false);
        }

        if (IsMultiInstance && OverrideViewModel)
        {
            return controller.Create(InitializeViewModel);
        }

        return controller.Create();
    }

    protected ViewModel ResolveViewModel(Controller controller = null)
    {
        var vm = !string.IsNullOrEmpty(_resolveName) ?
              GameManager.Container.Resolve<ViewModel>(_resolveName) :
              GameManager.Container.Resolve(ViewModelType) as ViewModel;
        return vm;
    }
    public abstract ViewModel CreateModel();
    //{

    //if (_ViewModelFrom == ViewModelRegistryType.ResolveInstance)
    //{
    //    var resolved = GameManager.Container.Resolve(ViewModelType) as ViewModel;
    //    if (OverrideViewModel)
    //        InitializeViewModel(resolved);
    //    return resolved;
    //}
    //else if (_ViewModelFrom == ViewModelRegistryType.Controller)
    //{
    //    if (string.IsNullOrEmpty(_ViewModelControllerMethod))
    //    {
    //        Debug.LogError("You have specified NewFromControllerMethod on View Model From Property but no Controller Method is specified.");
    //        return null;
    //    }

    //    var controllerType = Type.GetType(_ViewModelControllerType);
    //    if (controllerType != null)
    //    {
    //        var controllerMethod = controllerType.GetMethod(_ViewModelControllerMethod);
    //        var controller = GameManager.Container.Resolve(controllerType, true);
    //        if (controllerMethod != null)
    //        {
    //            try
    //            {
    //                var vm = controllerMethod.Invoke(controller, null) as ViewModel;
    //                if (OverrideViewModel)
    //                    InitializeViewModel(vm);

    //                return vm;
    //            }
    //            catch (TargetException)
    //            {
    //                Debug.LogError(string.Format("ViewModel can't be created.  You need to add the {0} controller to the scene in order to properly invoke its {1} method. ", _ViewModelControllerType, _ViewModelControllerMethod), this.gameObject);
    //                return null;
    //            }
    //        }
    //        else
    //        {
    //            Debug.LogError(string.Format("The controller method {0} on type {1} was not found either it was renamed or removed. Please fix the view model from property", _ViewModelControllerMethod, _ViewModelControllerType), this.gameObject);
    //            return null;
    //        }
    //    }
    //    else
    //    {
    //        Debug.LogError(string.Format("The controller type {0} was not found either it was renamed or removed. Please fix the view model from property", _ViewModelControllerType), this.gameObject);
    //        return null;
    //    }
    //}
    //return Activator.CreateInstance(ViewModelType) as ViewModel;
    //}

    public virtual void OnDestroy()
    {
        var pv = ParentView;
        if (pv != null)
        {
            pv.ChildViews.Remove(this);
        }
        Unbind();
        Bindings.Clear();
    }

    public virtual void OnDisable()
    {
        var pv = ParentView;
        if (pv != null)
        {
            pv.ChildViews.Remove(this);
        }
        Unbind();
        _shouldRebindOnEnable = true;
    }

    [NonSerialized]
    private bool _shouldRebindOnEnable = false;

    [SerializeField, HideInInspector, UFGroup("View Model Properties")]
    private bool _overrideViewModel;
    [SerializeField, HideInInspector, UFGroup("View Model Properties")]
    private bool _forceResolveViewModel = false;

    [SerializeField, HideInInspector, UFGroup("View Model Properties")]
    private string _resolveName = null;

    public virtual bool IsMultiInstance
    {
        get
        {
            return true;
        }
    }

    public virtual void OnEnable()
    {
        if (_shouldRebindOnEnable)
            SetupBindings();
    }

    public bool OverrideViewModel
    {
        get { return _overrideViewModel; }
        set { _overrideViewModel = value; }
    }

    public bool ForceResolveViewModel
    {
        get { return _forceResolveViewModel; }
        set { _forceResolveViewModel = value; }
    }

    /// <summary>
    /// This method will setup all bindings on this view.  Bindings don't actually occur on a view until this method is called.
    /// In the bind method it will simply add to the collection of bindings.  You should never have to call this method manually.
    /// </summary>
    public void SetupBindings()
    {
        if (ViewModelObject == null)
        {
            _Model = CreateModel();
        }

        if (_bound)
            return;
        // Initialize the model


        // Loop through and binding providers and let them add bindings
        foreach (var bindingProvider in BindingProviders)
            bindingProvider.Bind(this);

        // Add any programming bindings
        Bind();
        // Initialize the bindings
        for (var i = 0; i < Bindings.Count; i++)
            Bindings[i].Bind();

        for (var i = 0; i < transform.childCount; i++)
        {
            var view = transform.GetChild(i).GetView();
            if (view == null) continue;
            view.SetupBindings();
        }
        foreach (var childView in ChildViews)
        {
            if (childView.transform != this.transform)
            {
                childView.SetupBindings();
            }
        }

        // Mark this view as bound
        _bound = true;

        AfterBind();
    }

    public virtual void AfterBind()
    {

    }
    public virtual void Start()
    {
        var pv = ParentView;
        if (pv != null)
        {
            pv.ChildViews.Add(this);
        }

        if (ViewModelObject == null)
            _Model = CreateModel();
        // If its instantiated then we dont want to recall
        // the bindings init because instantiate does that already
        if (Instantiated) return;
        if (ParentView == null || ParentView._bound)
        {
            SetupBindings();
        }
    }

    /// <summary>
    /// Unbind the current bindings.
    /// </summary>
    public override void Unbind()
    {
        base.Unbind();

        // Loop through and binding providers and let them add bindings
        foreach (var bindingProvider in BindingProviders)
            bindingProvider.Unbind(this);

        _bound = false;
    }

    /// <summary>
    /// Invoke a .NET event on this view.  This is a convinience method for Event Bindings.
    /// </summary>
    /// <param name="eventname">The name of the event that occured</param>
    public virtual void Event(string eventname)
    {
        if (_LogEvents)
        {
            Debug.Log(string.Format("Event: {0} occured.", eventname), this.gameObject);
        }
        ViewEvent handler = EventTriggered;
        if (handler != null)
            handler(eventname);
    }

    /// <summary>
    /// This method should be overriden to Initialize the ViewModel
    /// with any options specified in a unity component inspector.
    /// </summary>
    /// <param name="model">The model to initialize.</param>
    public abstract void InitializeViewModel(ViewModel model);

    protected virtual void LateUpdate()
    {
        foreach (var binding in Bindings)
        {
            if (binding.TwoWay)
            {
                ((ITwoWayBinding)binding).BindReverse();
            }
        }
    }

    public void ExecuteCommand(ICommand command, object argument)
    {
        if (command == null) return;

        command.Parameter = argument;
        if (command.Parameter == null)
        {
            command.Parameter = this.ViewModelObject;
        }
        IEnumerator enumerator = command.Execute();
        if (enumerator != null)
            StartCoroutine(enumerator);
    }

    public virtual void ExecuteCommand(ICommand command)
    {
        if (command == null) return;

        if (command.Parameter == null)
            command.Parameter = this.ViewModelObject;

        IEnumerator enumerator = command.Execute();
        if (enumerator != null)
            StartCoroutine(enumerator);
    }
    public void ExecuteCommand<TArgument>(ICommandWith<TArgument> command, ViewModel sender, TArgument argument)
    {
        if (command == null) return;
        command.Parameter = argument;
        if (command.Parameter == null)
        {
            command.Parameter = this.ViewModelObject;
        }
        IEnumerator enumerator = command.Execute();
        if (enumerator != null)
            StartCoroutine(enumerator);
    }
    public void ExecuteCommand<TArgument>(ICommandWith<TArgument> command, TArgument argument)
    {
        if (command == null) return;
        command.Parameter = argument;
        if (command.Parameter == null)
        {
            command.Parameter = this.ViewModelObject;
        }
        IEnumerator enumerator = command.Execute();
        if (enumerator != null)
            StartCoroutine(enumerator);
    }
}


[AttributeUsage(AttributeTargets.Field)]
public class ViewModelOverrideAttribute : Attribute
{

}

[AttributeUsage(AttributeTargets.Field)]
public class UFGroup : Attribute
{
    public UFGroup(string name)
    {
        Name = name;
    }

    public string Name { get; set; }
}

[AttributeUsage(AttributeTargets.Field)]
public class UFRequireInstanceMethod : Attribute
{
    public string MethodName { get; set; }

    public UFRequireInstanceMethod(string methodName)
    {
        MethodName = methodName;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class UFToggleGroup : Attribute
{
    public UFToggleGroup(string name)
    {
        Name = name;
    }

    public string Name { get; set; }
}