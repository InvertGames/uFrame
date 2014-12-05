using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if DLL
namespace Invert.Common.MVVM
{
#endif

/// <summary>
/// A controller is a group of commands usually to provide an abstract level
/// </summary>
public abstract class Controller
{
    [Inject]
    public ICommandDispatcher CommandDispatcher { get; set; }

    private SceneContext _context;

#if TESTS

    /// <summary>
    /// The dependency container that this controller will use
    /// </summary>
    public IGameContainer Container { get; set; }
    /// <summary>
    /// The scene context that contains the View-Models for the current scene.
    /// </summary>
    public SceneContext Context
    {
        get { return _context; }
        set
        {
            _context = value;
            if (value != null)
                Container = value.Container;
        }
    }

#else
    /// <summary>
    /// The dependency container that this controller will use
    /// </summary>
    public IGameContainer Container { get; set; }
    /// <summary>
    /// The scene context that contains the View-Models for the current scene.
    /// </summary>
    [Obsolete]
    public SceneContext Context
    {
        get { return _context ?? GameManager.ActiveSceneManager.Context; }
        set
        {
            _context = value;
            if (value != null)
                Container = value.Container;
        }
    }

#endif

    protected Controller()
    {
        //throw new Exception("Default constructor is not allowed.  Please regenerate your diagram or create the controller with a SceneContext.");
    }

    /// <summary>
    /// Create a new ViewModel. This will generate a Unique Identifier for the VM.  If this is a specific instance use the overload and pass
    /// an identifier.
    /// </summary>
    /// <returns></returns>
    public virtual ViewModel Create()
    {
        return Create(Guid.NewGuid().ToString());
    }

    /// <summary>
    /// Creates a new ViewModel with a specific identifier.  If it already exists in the SceneContext it will return that instead
    /// </summary>
    /// <param name="identifier">The identifier that will be used to check the context to see if it already exists.</param>
    /// <returns></returns>
    public virtual ViewModel Create(string identifier)
    {

        var vm = CreateEmpty(identifier);
        return vm;
    }

    /// <summary>
    /// Create an empty view-model with the specified identifer. Note: This method does not wire up the view-model to this controller.
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns>A new View-Model or the view-model found in the context with the same identifier.</returns>
    public virtual ViewModel CreateEmpty(string identifier)
    {
        var vm = CreateEmpty();
        vm.Identifier = identifier;
        return vm;
    }

    /// <summary>
    /// Create an empty view-model . Note: This method does not wire up the view-model to this controller and only instantiates an associated view-model.
    /// </summary>
    /// <returns>A new View-Model or the view-model found in the context with the same identifier.</returns>
    public virtual ViewModel CreateEmpty()
    {
        throw new NotImplementedException("You propably need to resave you're diagram. Or you need to not call create on an abstract controller.");
    }


    [Obsolete("Game event is not longer used for transitions.  Regenerate your diagram.")]
    public void GameEvent(string name) { }

    public abstract void Initialize(ViewModel viewModel);

    [Obsolete("WireCommands no longer lives in the controller. Regenerate your diagram.")]
    public virtual void WireCommands(ViewModel viewModel)
    {

    }


    public void ExecuteCommand(IuFrameCommand command, object argument)
    {
        CommandDispatcher.ExecuteCommand(command, argument);
    }

    public virtual void ExecuteCommand(IuFrameCommand command)
    {
        CommandDispatcher.ExecuteCommand(command, null);
    }

    public void ExecuteCommand<TArgument>(ICommandWith<TArgument> command, TArgument argument)
    {
        CommandDispatcher.ExecuteCommand(command, argument);
    }


#if !TESTS
    [Obsolete("Coroutines are no longer used in controllers for external compatability, use Observable.Timer or Observable.Interval")]
    public UnityEngine.Coroutine StartCoroutine(IEnumerator routine)
    {
        return GameManager.Instance.StartCoroutine(routine);
    }

    [Obsolete("Coroutines are no longer used in controllers for external compatability, use Observable.Timer or Observable.Interval")]
    public void StopAllCoroutines()
    {
        GameManager.Instance.StopAllCoroutines();
    }

    [Obsolete("Coroutines are no longer used in controllers for external compatability, use Observable.Timer or Observable.Interval")]
    public void StopCoroutine(string name)
    {
        GameManager.Instance.StopCoroutine(name);
    }

    [Obsolete("This method is obsolete. Use Property.Subscribe")]
    public ModelPropertyBinding SubscribeToProperty<TViewModel, TBindingType>(TViewModel source, P<TBindingType> sourceProperty, Action<TViewModel, TBindingType> changedAction) where TViewModel : ViewModel
    {
        return null;
    }
#endif
    [Obsolete("This method is obsolete. Use Property.Subscribe")]
    public ModelPropertyBinding SubscribeToProperty<TBindingType>(P<TBindingType> sourceProperty, Action<TBindingType> targetSetter)
    {
        return null;
    }

    protected void SubscribeToCommand(IuFrameCommand command, Action action)
    {
        command.OnCommandExecuted += () => action();

    }
};


#if DLL
}
#endif