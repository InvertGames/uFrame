using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

/// <summary>
/// The base class for a View that binds to a ViewModel
/// </summary>
public abstract class ViewBase : ViewContainer, IUFSerializable, IBindable
{
    public T AddComponentBinding<T>() where T : ObservableComponent
    {
        var component = gameObject.AddComponent<T>();
        AddBinding(component);
        return component;
    }
    public IDisposable AddComponentBinding(ObservableComponent component)
    {
        return AddBinding(component);
    }

    Subject<Unit> _updateObservable;

    private Subject<Transform> _transformChangedObservable;


    /// <summary>
    /// 	<para>Update is called every frame, if the MonoBehaviour is enabled.  It is important to make sure that you override this method instead of just creating
    /// it.  uFrame uses this method as an observable, and that observable is used for all default Scene Property implementations.</para>
    /// </summary>
    public virtual void Update()
    {
        if (_updateObservable != null)
            _updateObservable.OnNext(Unit.Default);

        if (_transformChangedObservable != null && transform.hasChanged)
        {
            _transformChangedObservable.OnNext(transform);
            transform.hasChanged = false;
        }
    }

    /// <summary>This Observable allows you to use the Update method of a monobehaviour as an observable.</summary>
    /// <example>
    /// 	<code title="Simple Update Observable" description="In this example we subscribe to the update observable, and print out a message every frame." groupname="Views" lang="CS">
    /// this.UpdateAsObservable().Subscribe(_=&gt;Debug.Log("Output every frame"));</code>
    /// </example>
    public IObservable<Unit> UpdateAsObservable()
    {
        return _updateObservable ?? (_updateObservable = new Subject<Unit>());
    }
    /// <summary>
    /// The View Event delegate that takes a string for the event name.
    /// </summary>
    /// <param name="eventName">The event that has occured.</param>
    public delegate void ViewEvent(string eventName);

    /// <summary>
    /// Should we log an event for each View event that occurs.
    /// </summary>
    [HideInInspector]
    public bool _LogEvents;

    private List<IBindingProvider> _bindingProviders;

    private bool _bound = false;

    ///// <summary>
    ///// Where should the viewmodel come from or how should it be instantiated.
    ///// </summary>
    //[HideInInspector]
    //public ViewModelRegistryType _ViewModelFrom = ViewModelRegistryType.ResolveInstance;
    private List<ViewBase> _children;

    [SerializeField, HideInInspector, UFGroup("View Model Properties")]
    private bool _forceResolveViewModel = false;

    [HideInInspector]
    private string _id;

    [SerializeField, HideInInspector]
    private bool _InjectView = false;

    private int _instanceId;

    //[HideInInspector]
    //public string _ViewModelControllerType;
    [HideInInspector]
    private ViewModel _Model;

    [SerializeField, HideInInspector, UFGroup("View Model Properties")]
    private bool _overrideViewModel;

    //[HideInInspector]
    //public string _ViewModelControllerMethod;
    private ViewBase _parentView;

    private IObservable<Vector3> _positionObservable;

    [SerializeField, HideInInspector, UFGroup("View Model Properties")]
    private string _resolveName = null;

    private IObservable<Quaternion> _rotationObservable;

    [SerializeField, HideInInspector]
    private bool _Save = false;

    private IObservable<Vector3> _scaleObservable;

    [NonSerialized]
    private bool _shouldRebindOnEnable = false;

    public List<IBindingProvider> BindingProviders
    {
        get { return _bindingProviders ?? (_bindingProviders = new List<IBindingProvider>()); }
        set { _bindingProviders = value; }
    }

    /// <summary>
    /// A wrapper for this view's viewmodel bindings.  It is a wrapper
    /// for ViewModel.Bindings[gameObject.GetInstanceId()]
    /// </summary>
    public List<IDisposable> Bindings
    {
        get
        {
            if (!ViewModelObject.Bindings.ContainsKey(InstanceId))
            {
                ViewModelObject.Bindings.Add(InstanceId, new List<IDisposable>());
            }

            return ViewModelObject.Bindings[InstanceId];
        }
    }

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


    /// <summary>
    /// This is the default identifier to use when "ResolveName" is not specified and it's a single instance.
    /// This field is automatically overriden by the uFrame designer.
    /// </summary>
    public virtual string DefaultIdentifier
    {
        get
        {
            return null;
        }
    }

    public bool ForceResolveViewModel
    {
        get { return _forceResolveViewModel; }
        set { _forceResolveViewModel = value; }
    }

    /// <summary>
    /// The identifier used for requesting a view-model.
    /// Implementation Details:
    /// -If its not a multiinstance viewmodel and the "ResolveName" is empty it will use "DefaultIdentifier" property otherwise it will use
    /// the resolve name.
    /// - If it's a multiinstance viewmodel and the resolvename is specified it will use that.
    /// - If the Use Hashcode as identifier is checked it will use this views hashcode.
    ///
    /// Note: If using a prefab that is placed in the Unity editor in various places around a scene and it still needs to be unique every
    /// scene load (for scene loading and saving) you will want to override this property and supply a identifier that makes it unique.
    /// </summary>
    public virtual string Identifier
    {
        get
        {

            if (ForceResolveViewModel)
            {
                if (string.IsNullOrEmpty(_resolveName)) return null;
                return _resolveName;
            }
            if (!string.IsNullOrEmpty(_id))
            {
                return _id;
            }
            return _id = Guid.NewGuid().ToString();
        }
        set { _id = value; }
    }

    public bool InjectView
    {
        get { return _InjectView; }
        set { _InjectView = value; }
    }

    /// <summary>
    /// A lazy loaded property for "GetInstanceId" on the game-object.
    /// </summary>
    public int InstanceId
    {
        get
        {
            if (_instanceId == 0)
            {
                _instanceId = this.gameObject.GetInstanceID();
            }
            return _instanceId;
        }
    }

    public bool Instantiated { get; set; }

    //public virtual bool IsMultiInstance
    //{
    //    get
    //    {
    //        return true;
    //    }
    //}

    public bool IsBound
    {
        get { return _bound; }
        set { _bound = value; }
    }

    [Obsolete]
    public virtual bool IsMultiInstance
    {
        get { return true; }
    }

    public bool OverrideViewModel
    {
        get { return _overrideViewModel; }
        set { _overrideViewModel = value; }
    }

    public ViewBase ParentView
    {
        get
        {

            if (_parentView == null)
            {
                var parent = this.transform.parent;
                if (parent == null) return null;
                while (parent != null)
                {
                    var view = parent.GetView();
                    if (view != null)
                    {
                        _parentView = view;
                        break;
                    }
                    parent = parent.parent;
                }
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

    /// <summary>
    /// Should this view be saved in the "SceneContext"
    /// </summary>
    public bool Save
    {
        get { return _Save; }
        set { _Save = value; }
    }


    public SceneManager SceneManager
    {
        get { return GameManager.ActiveSceneManager; }
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
            if (IsBound)
            {
                Unbind();
                SetupBindings();
            }
            //Bind();
        }
    }

    public abstract Type ViewModelType { get; }

    public IObservable<Vector3> PositionAsObservable
    {
        get
        {
            return TransformChangedObservable.Select(p => p.transform.position).DistinctUntilChanged();
        }
    }
    public IObservable<Vector3> LocalPositionAsObservable
    {
        get
        {
            return TransformChangedObservable.Select(p => p.transform.localPosition).DistinctUntilChanged();
        }
    }
    public IObservable<Quaternion> LocalRotationAsObservable
    {
        get
        {
            return TransformChangedObservable.Select(p => p.transform.localRotation).DistinctUntilChanged();
        }
    }

    public IObservable<Quaternion> RotationAsObservable
    {
        get
        {
            return TransformChangedObservable.Select(p => p.transform.rotation).DistinctUntilChanged();
        }
    }
    public IObservable<Vector3> ScaleAsObservable
    {
        get
        {
            return TransformChangedObservable.Select(p => p.transform.localScale).DistinctUntilChanged();
        }
    }
    /// <summary>
    /// The name of the prefab that created this view
    /// </summary>
    public string ViewName { get; set; }


    /// <summary>
    /// Observable that notifies its subscribers only when the transform has changed.
    /// </summary>
    public Subject<Transform> TransformChangedObservable
    {
        get { return _transformChangedObservable ?? (_transformChangedObservable = new Subject<Transform>()); }
        set { _transformChangedObservable = value; }
    }

    /// <summary>
    /// 	<para>This method adds a binding directly onto the view-model.  It will be registered with a key of this object instance id, this allows any disposable to be
    /// properly disposed when this view is destroyed.</para>
    /// </summary>
    /// <param name="binding">The IDisposable that will be invoked when a view or view-model is un-bound.</param>
    /// <returns>The same disposable you pass in order to store a local destruction of the binding if needed.</returns>
    /// <example>
    /// 	<code title="Adding a simple binding." description="This example will register the HealthChange subscription so that when the view is destroyed, it will no longer be invoked when the health changes." groupname="Views" lang="CS">
    /// this.AddBinding(MyViewModel.HealthProperty.Subscribe(HealthChanged));</code>
    /// </example>
    public IDisposable AddBinding(IDisposable binding)
    {
        Bindings.Add(binding);
        return binding;
    }

    /// <summary>
    /// 	<para>This method is invoked right after the view-model has been bound.</para>
    /// </summary>

    public virtual void AfterBind()
    {
    }

    /// <summary>The awake method currenlty doesn't overried anything, but is left as a virutal metthod so that in the future if the method is needed other methods will not
    /// hide any implementation.</summary>
    public virtual void Awake()
    {


    }

    /// <summary>
    /// 	<para>This method is the primary method of a view.  It's purpose is to provide a safe place to create subscriptions/bindings to it's view-model.  When
    /// this method is invoked it will allways have an instance to a view-model.</para>
    /// 	<para>In this method you should subscribe to it's owned view-models properties, collections, and execute commands.</para>
    /// </summary>
    /// <example>
    /// 	<code title="Example" description="" lang="CS">
    /// var viewModel = ViewModelObject as FPSWeaponViewModel; // &lt;-- for clarity, use property instead
    /// this.BindProperty(viewModel.AmmoProperty, ammo=&gt; { _AmmoLabel.text = ammo.ToString(); });</code>
    /// </example>
    public virtual void Bind()
    {
    }

    /// <summary>
    /// This method is called in order to create a model for this view.  In a uFrame Designer generated
    /// view it will implement this method and call the "RequestViewModel" on the scene manager.
    /// </summary>
    public abstract ViewModel CreateModel();

    /// <summary>All of the designer generated "Execute{CommandName}" ultimately use this method. So when you need to execute a command on an outside view-model(meaning not the
    /// view-model of this view) this method can be used. e.g. ExecuteCommand(command, argument)</summary>
    /// <param name="command">The command to execute e.g. MyGameViewModel.MainMenuCommand</param>
    /// <param name="argument">The argument to pass along if needed.</param>
    public void ExecuteCommand(ICommand command, object argument)
    {
        GameManager.CommandDispatcher.ExecuteCommand(command, argument);
    }

    /// <summary>
    /// All of the designer generated "Execute{CommandName}" ultimately use this method.  So when
    /// need to execute a command on an outside view-model(meaning not the view-model of this view) this
    /// method can be used. e.g. ExecuteCommand(MyGameViewModel.MainMenuCommand)
    /// </summary>
    /// <param name="command">The command to execute e.g. MyGameViewModel.MainMenuCommand</param>
    public virtual void ExecuteCommand(ICommand command)
    {
        GameManager.CommandDispatcher.ExecuteCommand(command, null);
    }

    /// <summary>
    /// Executes a command of type ICommand.
    /// </summary>
    /// <typeparam name="TArgument"></typeparam>
    /// <param name="command">The command instance to execute.</param>
    /// <param name="sender">The sender of the command.</param>
    /// <param name="argument">The argument required by the command.</param>
    public void ExecuteCommand<TArgument>(ICommandWith<TArgument> command, ViewModel sender, TArgument argument)
    {
        GameManager.CommandDispatcher.ExecuteCommand(command, null);
    }

    /// <summary>
    /// Executes a command of type ICommand.
    /// </summary>
    /// <typeparam name="TArgument"></typeparam>
    /// <param name="command">The command instance to execute.</param>
    /// <param name="argument">The argument required by the command.</param>
    public void ExecuteCommand<TArgument>(ICommandWith<TArgument> command, TArgument argument)
    {
        GameManager.CommandDispatcher.ExecuteCommand(command, argument);
    }

    /// <summary>
    /// 	<para>A wrapper for "InitializeViewModel" which takes the information supplied in the inspector and applies it to the view-model.</para>
    /// 	<innovasys:widget type="Note Box" layout="block" xmlns:innovasys="http://www.innovasys.com/widgets">
    /// 		<innovasys:widgetproperty layout="block" name="Content">If your viewmodel is a composite view-model containing properties of other view-models, a view can
    ///     be referenced, and this method will use that view's view-model.</innovasys:widgetproperty>
    /// 	</innovasys:widget>
    /// </summary>
    /// <param name="model"></param>
    /// <example>
    /// 	<code title="Initialize Data Example" description="Demonstrates using initializedata to pull use the values from the inspector on an instantiated view." groupname="Views" lang="CS">
    /// var myView = InstantiateView(new FPSWeaponViewModel() { Ammo=60 });
    /// myView.InitializeData();
    /// Debug.Log(myView.FPSWeapon.Ammo);
    /// // output will not be 60, it will be the value specified on the views insepctor.</code>
    /// </example>
    public void InitializeData(ViewModel model)
    {
        if (!Initialized)
            InitializeViewModel(model);

        Initialized = true;
    }
    [NonSerialized]
    private bool _initialized = false;

    public bool Initialized {
        get { return _initialized; } 
        set { _initialized = value; }
    }

    /// <summary>
    /// When this view is destroy it will decrememnt the ViewModel's reference count.  If the reference count reaches 0
    /// it will call "Unbind" on the viewmodel properly unbinding anything subscribed to it.
    /// </summary>
    public virtual void OnDestroy()
    {
         Initialized = false; // Some weird bug where unity keeps this value at true between runs
        SceneManager.UnRegisterView(this);
        Unbind();
        var pv = ParentView;
        if (pv != null)
        {
            pv.ChildViews.Remove(this);
        }
    }

    public virtual void OnDisable()
    {

    }

    public virtual void OnEnable()
    {
        Initialized = false; // Some weird bug where unity keeps this value at true between runs
        if (_shouldRebindOnEnable)
            SetupBindings();
    }

    /// <summary>This method will ensure that a view-model exists and then call the bind method when it's appropriate.</summary>
    public void SetupBindings()
    {
        if (ViewModelObject == null)
        {
            _Model = CreateModel();
        }


        if (IsBound)
            return;
        // Initialize the model
        if (ViewModelObject != null)
        {
            ViewModelObject.References++;
        }

        // Loop through and binding providers and let them add bindings
        foreach (var bindingProvider in BindingProviders)
            bindingProvider.Bind(this);

        // Add any programming bindings
        PreBind();
        Bind();

        //// Initialize the bindings
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
        IsBound = true;

        AfterBind();
    }

    /// <summary>
    /// The start method approparitely initializes the "ChildViews" collection.
    /// </summary>
    public virtual void Start()
    {
        if (this.Save)
        {
            var sm = SceneManager;
            if (sm != null)
            {
                sm.RegisterView(this);
            }
        }

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
        if (ParentView == null || ParentView.IsBound)
        {
            SetupBindings();
        }
    }

    /// <summary>Unbind any binding or disposable that has been added via the "AddBinding" method.</summary>
    public virtual void Unbind()
    {
        if (_Model != null)
        {
            _Model.References--;
            foreach (var binding in Bindings)
            {
                binding.Dispose();
            }
        }

        foreach (var bindingProvider in BindingProviders)
            bindingProvider.Unbind(this);

        IsBound = false;
    }

    public virtual void Write(ISerializerStream stream)
    {

        //ViewModelObject.Write(stream);
        stream.SerializeString("Identifier", this.Identifier);
        stream.SerializeString("ViewType", this.GetType().FullName);
    }
    /// <summary>Will deserialize this view directly from a stream.</summary>
    public virtual void Read(ISerializerStream stream)
    {
        this.Identifier = stream.DeserializeString("Identifier");


    }
    /// <summary>
    /// Overriden by the the uFrame designer to apply any two-way/reverse properties.
    /// </summary>
    /// <buildflag>Exclude from Online</buildflag>
    /// <buildflag>Exclude from Booklet</buildflag>
    [Obsolete("No longer used by uFrame designer. You most likely need to Save and Compile your diagram")]
    protected virtual void Apply()
    {
    }

    /// <summary>
    /// This method should be overriden to Initialize the ViewModel
    /// with any options specified in a unity component inspector.
    /// </summary>
    /// <buildflag>Exclude from Online</buildflag>
    /// <buildflag>Exclude from Booklet</buildflag>
    /// <param name="model">The model to initialize.</param>
    protected abstract void InitializeViewModel(ViewModel model);

    /// <summary>
    /// Just calls the apply method.
    /// </summary>
    public virtual void LateUpdate()
    {

    }

    /// <summary>
    /// This method is called immediately before "Bind".  This method is used
    /// by uFrames designer generated code to set-up defined bindings.
    /// </summary>
    protected virtual void PreBind()
    {
    }

    /// <summary>This method will destroy a view if it exists, then replace it with another view.</summary>
    protected virtual ViewBase ReplaceView(ViewBase current, ViewModel value, GameObject prefab)
    {
        if (value == null && current != null && current.gameObject != null)
        {
            Destroy(current.gameObject);
        }
        if (prefab == null)
        {
            return ((this.InstantiateView(value)));
        }
        else
        {
            return ((this.InstantiateView(prefab, value)));
        }
    }

    /// <summary>
    /// Request a view-model with a given controller if any.
    /// </summary>
    /// <param name="controller"></param>
    /// <returns></returns>
    protected ViewModel RequestViewModel(Controller controller)
    {
        return SceneManager.RequestViewModel(this, controller);
    }
}